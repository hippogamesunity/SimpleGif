using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleGif.GifCore;
using SimpleGif.GifCore.Assets.GifCore;
using SimpleGif.GifCore.Blocks;
using SimpleGif.Structures;

namespace SimpleGif
{
	public class Gif
	{
		public List<GifFrame> Frames;

		public Gif(List<GifFrame> frames)
		{
			Frames = frames;
		}

		public static Gif FromBytes(byte[] bytes)
		{
			var frames = new List<GifFrame>();
			var parser = new GifParser(bytes);
			var blocks = parser.Blocks;
			var width = parser.LogicalScreenDescriptor.LogicalScreenWidth;
			var height = parser.LogicalScreenDescriptor.LogicalScreenHeight;
			var globalColorTable = GetUnityColors(parser.GlobalColorTable);
			var backgroundColor = globalColorTable[parser.LogicalScreenDescriptor.BackgroundColorIndex];

			for (var j = 0; j < parser.Blocks.Count; j++)
			{
				if (!(blocks[j] is ImageDescriptor imageDescriptor)) continue;

				if (imageDescriptor.InterlaceFlag == 1) throw new NotSupportedException("Interlacing is not supported!");

				var colorTable = imageDescriptor.LocalColorTableFlag == 1 ? GetUnityColors((ColorTable)blocks[j + 1]) : globalColorTable;
				var data = (TableBasedImageData)blocks[j + 1 + imageDescriptor.LocalColorTableFlag];
				var extension = j > 0 ? blocks[j - 1] as GraphicControlExtension : null;
				var pixels = ParsePixels(data, extension, width, height, frames, backgroundColor, imageDescriptor, colorTable);
				var texture = new Texture2D(width, height);

				texture.SetPixels32(pixels);
				texture.Apply();

				var frame = new GifFrame
				{
					Texture = texture,
					Delay = extension?.DelayTime / 100f ?? 0
				};

				frames.Add(frame);
			}

			return new Gif(frames);
		}

		public byte[] GetBytes()
		{
			const string header = "GIF89a";
			var imageWidth = (ushort)Frames[0].Texture.Width;
			var imageHeight = (ushort)Frames[0].Texture.Height;
			var globalColorTable = GetColorTable(out var transparentColorFlag, out var transparentColorIndex);
			var globalColorTableSize = GetColorTableSize(globalColorTable);
			var logicalScreenDescriptor = new LogicalScreenDescriptor(imageWidth, imageHeight, 1, 7, 0, globalColorTableSize, 0, 0);
			var applicationExtension = new ApplicationExtension();
			var bytes = new List<byte>();

			bytes.AddRange(Encoding.UTF8.GetBytes(header));
			bytes.AddRange(logicalScreenDescriptor.GetBytes());
			bytes.AddRange(ColorTableToBytes(globalColorTable, globalColorTableSize));
			bytes.AddRange(applicationExtension.GetBytes());

			foreach (var frame in Frames)
			{
				var graphicControlExtension = new GraphicControlExtension(4, 0, 2, 0, transparentColorFlag, (ushort)(100 * frame.Delay), transparentColorIndex);
				var imageDescriptor = new ImageDescriptor(0, 0, imageWidth, imageHeight, 0, 0, 0, 0, 0);
				var colorIndexes = GetColorIndexes(frame.Texture, globalColorTable, transparentColorFlag, transparentColorIndex);
				var minCodeSize = LzwEncoder.GetCodeSize(colorIndexes);
				var encoded = LzwEncoder.Encode(colorIndexes, minCodeSize);
				var tableBasedImageData = new TableBasedImageData(minCodeSize, encoded);

				bytes.AddRange(graphicControlExtension.GetBytes());
				bytes.AddRange(imageDescriptor.GetBytes());
				bytes.AddRange(tableBasedImageData.GetBytes());
			}

			bytes.Add(0x3B);

			return bytes.ToArray();
		}

		private List<Color32> GetColorTable(out byte transparentColorFlag, out byte transparentColorIndex)
		{
			var colorTable = new List<Color32>();

			transparentColorFlag = 0;

			foreach (var frame in Frames)
			{
				colorTable.AddRange(frame.Texture.GetPixels32());
			}

			colorTable = colorTable.Distinct().ToList();

			if (colorTable.Count > 0xFF) throw new NotSupportedException("Global color table exceeds size limit 255! Please consider using max 255 colors for all image frames.");

			transparentColorFlag = 0;
			transparentColorIndex = 0;

			for (var i = 0; i < colorTable.Count; i++)
			{
				if (colorTable[i].A == 0)
				{
					transparentColorFlag = 1;
					transparentColorIndex = 0;

					var transparentColor = GetTransparentColor(colorTable);

					colorTable.Insert(0, transparentColor);

					break;
				}
			}

			return colorTable;
		}

		private static Color32 GetTransparentColor(List<Color32> colorTable)
		{
			for (byte r = 0; r < 0xFF; r++)
			{
				for (byte g = 0; g < 0xFF; g++)
				{
					for (byte b = 0; b < 0xFF; b++)
					{
						var transparentColor = new Color32(r, g, b, 1);

						if (!colorTable.Contains(transparentColor))
						{
							return transparentColor;
						}
					}
				}
			}

			throw new Exception("Unable to resolve transparent color!");
		}

		private static byte[] ColorTableToBytes(List<Color32> colorTable, byte colorTableSize)
		{
			if (colorTable.Count > 256) throw new Exception("Color table size exceeds 256 size limit: " + colorTable.Count);

			var size = 1 << (colorTableSize + 1);
			var bytes = new byte[3 * size];

			for (var i = 0; i < colorTable.Count; i++)
			{
				bytes[3 * i] = colorTable[i].R;
				bytes[3 * i + 1] = colorTable[i].G;
				bytes[3 * i + 2] = colorTable[i].B;
			}

			return bytes;
		}

		private static byte GetColorTableSize(List<Color32> colorTable)
		{
			byte size = 0;

			while (1 << (size + 1) < colorTable.Count)
			{
				size++;
			}

			return size;
		}

		private static int[] GetColorIndexes(Texture2D texture, List<Color32> colorTable, byte transparentColorFlag, byte transparentColorIndex)
		{
			var pixels = texture.GetPixels32();
			var colorIndexes = new int[pixels.Length];

			for (var y = 0; y < texture.Height; y++)
			{
				for (var x = 0; x < texture.Width; x++)
				{
					var pixel = pixels[x + (texture.Height - y - 1) * texture.Width];

					if (transparentColorFlag == 1 && pixel.A == 0)
					{
						colorIndexes[x + y * texture.Width] = transparentColorIndex;
					}
					else
					{
						colorIndexes[x + y * texture.Width] = colorTable.IndexOf(pixel);

						if (colorIndexes[x + y * texture.Width] == -1) throw new Exception("Color index not found: " + pixel);
					}
				}
			}

			return colorIndexes;
		}

		private static Color32[] ParsePixels(TableBasedImageData data, GraphicControlExtension extension, int width, int height, List<GifFrame> frames, Color32 backgroundColor, ImageDescriptor imageDescriptor, Color32[] colors)
		{
			var decmpressed = LzwDecoder.Decode(data.ImageData, data.LzwMinimumCodeSize);
			var pixels = new Color32[width * height];
			var clear = new Color32(0, 0, 0, 0);

			if (extension != null)
			{
				switch (extension.DisposalMethod)
				{
					case 0:
					case 1:
					case 3:
						if (frames.Any())
						{
							pixels = frames.Last().Texture.GetPixels32();
						}
						break;
					case 2:
						for (var i = 0; i < pixels.Length; i++)
						{
							pixels[i] = backgroundColor;
						}
						break;
					default:
						throw new NotSupportedException($"Unknown method: {extension.DisposalMethod}!");
				}
			}

			for (var y = 0; y < imageDescriptor.ImageHeight; y++)
			{
				for (var x = 0; x < imageDescriptor.ImageWidth; x++)
				{
					var colorIndex = decmpressed[x + y * imageDescriptor.ImageWidth];
					var transparent = extension != null && extension.TransparentColorFlag == 1 && colorIndex == extension.TransparentColorIndex;
					var fx = x + imageDescriptor.ImageLeftPosition;
					var fy = height - y - 1 - imageDescriptor.ImageTopPosition; // Y-flip

					if (extension != null && extension.DisposalMethod == 3 && transparent) continue;

					pixels[fx + fy * width] = transparent ? clear : colors[colorIndex];
				}
			}

			return pixels;
		}

		private static Color32[] GetUnityColors(ColorTable table)
		{
			var colors = new Color32[table.Bytes.Length / 3];

			for (var i = 0; i < colors.Length; i++)
			{
				colors[i] = new Color32(table.Bytes[3 * i], table.Bytes[3 * i + 1], table.Bytes[3 * i + 2], 0xFF);
			}

			return colors;
		}
	}
}