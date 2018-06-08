using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleGif.Data;
using SimpleGif.Enums;
using SimpleGif.GifCore;
using SimpleGif.GifCore.Assets.GifCore;
using SimpleGif.GifCore.Blocks;

namespace SimpleGif
{
	/// <summary>
	/// Simple class for working with GIF format
	/// </summary>
	public class Gif
	{
		/// <summary>
		/// List of GIF frames
		/// </summary>
		public List<GifFrame> Frames;

		/// <summary>
		/// Create a new instance from GIF frames.
		/// </summary>
		public Gif(List<GifFrame> frames)
		{
			Frames = frames;
		}

		/// <summary>
		/// Decode byte array and return a new instance.
		/// </summary>
		public static Gif Decode(byte[] bytes)
		{
			return new Gif(DecodeIterator(bytes).Select(i => i).ToList());
		}

		/// <summary>
		/// Iterator can be used for large GIF-files in order to display progress bar.
		/// </summary>
		public static IEnumerable<GifFrame> DecodeIterator(byte[] bytes)
		{
			var parser = new GifParser(bytes);
			var blocks = parser.Blocks;
			var width = parser.LogicalScreenDescriptor.LogicalScreenWidth;
			var height = parser.LogicalScreenDescriptor.LogicalScreenHeight;
			var globalColorTable = parser.LogicalScreenDescriptor.GlobalColorTableFlag == 1 ? GetUnityColors(parser.GlobalColorTable) : null;
			var backgroundColor = globalColorTable?[parser.LogicalScreenDescriptor.BackgroundColorIndex] ?? new Color32();
			GraphicControlExtension graphicControlExtension = null;
			var state = new Color32[width * height];
			var filled = false;
			
			for (var j = 0; j < parser.Blocks.Count; j++)
			{
				switch (blocks[j])
				{
					case GraphicControlExtension _:
					{
						graphicControlExtension = (GraphicControlExtension) blocks[j];
						break;
					}
					case ImageDescriptor _:
					{
						var imageDescriptor = (ImageDescriptor) blocks[j];

						if (imageDescriptor.InterlaceFlag == 1) throw new NotSupportedException("Interlacing is not supported!");

						var colorTable = imageDescriptor.LocalColorTableFlag == 1 ? GetUnityColors((ColorTable) blocks[j + 1]) : globalColorTable;
						var data = (TableBasedImageData) blocks[j + 1 + imageDescriptor.LocalColorTableFlag];
						var frame = DecodeFrame(graphicControlExtension, imageDescriptor, data, filled, width, height, state, colorTable);

						yield return frame;

						switch (frame.DisposalMethod)
						{
							case DisposalMethod.NoDisposalSpecified:
							case DisposalMethod.DoNotDispose:
								break;
							case DisposalMethod.RestoreToBackgroundColor:
								for (var i = 0; i < state.Length; i++)
								{
									state[i] = backgroundColor;
								}
								filled = true;
								break;
							case DisposalMethod.RestoreToPrevious: // 'state' was already copied before decoding current frame
								filled = false;
								break;
							default:
								throw new NotSupportedException($"Unknown disposal method: {frame.DisposalMethod}!");
						}

						break;
					}
				}
			}
		}

		/// <summary>
		/// Get frame count. Can be used with DecodeIterator to display progress bar.
		/// </summary>
		public static int GetDecodeIteratorSize(byte[] bytes)
		{
			var parser = new GifParser(bytes);

			return parser.Blocks.Count(i => i is ImageDescriptor);
		}

		/// <summary>
		/// Encode all frames to byte array
		/// </summary>
		public byte[] Encode()
		{
			const string header = "GIF89a";
			var width = (ushort) Frames[0].Texture.width;
			var height = (ushort) Frames[0].Texture.height;
			var globalColorTable = GetColorTable(out var transparentColorFlag, out var transparentColorIndex);
			var globalColorTableSize = GetColorTableSize(globalColorTable);
			var logicalScreenDescriptor = new LogicalScreenDescriptor(width, height, 1, 7, 0, globalColorTableSize, 0, 0);
			var applicationExtension = new ApplicationExtension();
			var bytes = new List<byte>();

			bytes.AddRange(Encoding.UTF8.GetBytes(header));
			bytes.AddRange(logicalScreenDescriptor.GetBytes());
			bytes.AddRange(ColorTableToBytes(globalColorTable, globalColorTableSize));
			bytes.AddRange(applicationExtension.GetBytes());

			foreach (var frame in Frames)
			{
				var graphicControlExtension = new GraphicControlExtension(4, 0, (byte) frame.DisposalMethod, 0, transparentColorFlag, (ushort) (100 * frame.Delay), transparentColorIndex);
				var imageDescriptor = new ImageDescriptor(0, 0, width, height, 0, 0, 0, 0, 0);
				var colorIndexes = GetColorIndexes(frame.Texture, globalColorTable, transparentColorFlag, transparentColorIndex);
				var minCodeSize = LzwEncoder.GetMinCodeSize(colorIndexes);
				var encoded = LzwEncoder.Encode(colorIndexes, minCodeSize);
				var tableBasedImageData = new TableBasedImageData(minCodeSize, encoded);

				bytes.AddRange(graphicControlExtension.GetBytes());
				bytes.AddRange(imageDescriptor.GetBytes());
				bytes.AddRange(tableBasedImageData.GetBytes());
			}

			bytes.Add(0x3B);

			return bytes.ToArray();
		}

		/// <summary>
		/// Iterator can be used for large GIF-files in order to display progress bar.
		/// </summary>
		public IEnumerable<List<byte>> EncodeIterator()
		{
			const string header = "GIF89a";
			var width = (ushort) Frames[0].Texture.width;
			var height = (ushort) Frames[0].Texture.height;
			var globalColorTable = new List<Color32> { new Color32(0, 255, 0, 255) };
			const byte transparentColorFlag = 1;
			const byte transparentColorIndex = 0;
			var applicationExtension = new ApplicationExtension();
			List<byte> bytes;

			foreach (var frame in Frames)
			{
				bytes = new List<byte>();

				var graphicControlExtension = new GraphicControlExtension(4, 0, (byte) frame.DisposalMethod, 0, transparentColorFlag, (ushort) (100 * frame.Delay), transparentColorIndex);
				var imageDescriptor = new ImageDescriptor(0, 0, width, height, 0, 0, 0, 0, 0);
				var colorIndexes = GetColorIndexes(frame.Texture, globalColorTable, transparentColorFlag, transparentColorIndex, extendColorTable: true);
				var minCodeSize = LzwEncoder.GetMinCodeSize(colorIndexes);
				var encoded = LzwEncoder.Encode(colorIndexes, minCodeSize);
				var tableBasedImageData = new TableBasedImageData(minCodeSize, encoded);

				bytes.AddRange(graphicControlExtension.GetBytes());
				bytes.AddRange(imageDescriptor.GetBytes());
				bytes.AddRange(tableBasedImageData.GetBytes());

				yield return bytes;
			}

			yield return new List<byte> { 0x3B }; // GIF ending

			// Then output GIF header as last iterator element! This way we can build global color table "on fly" instead of expensive building operation in the beginning like Encode() does.

			var globalColorTableSize = GetColorTableSize(globalColorTable);
			var logicalScreenDescriptor = new LogicalScreenDescriptor(width, height, 1, 7, 0, globalColorTableSize, 0, 0);

			bytes = new List<byte>();
			bytes.AddRange(Encoding.UTF8.GetBytes(header));
			bytes.AddRange(logicalScreenDescriptor.GetBytes());
			bytes.AddRange(ColorTableToBytes(globalColorTable, globalColorTableSize));
			bytes.AddRange(applicationExtension.GetBytes());

			yield return bytes;
		}

		/// <summary>
		/// Get parts count for EncodeIterator. Can be used with EncodeIterator to display progress bar.
		/// </summary>
		public int GetEncodeIteratorSize()
		{
			return Frames.Count + 2;
		}

		private List<Color32> GetColorTable(out byte transparentColorFlag, out byte transparentColorIndex)
		{
			transparentColorFlag = 0;

			var colorTable = Frames.SelectMany(i => i.Texture.GetPixels32().Distinct()).Distinct().ToList();

			if (colorTable.Count > 256) throw new NotSupportedException("Global color table exceeds size limit 256! Please consider using max 256 colors for all image frames.");

			transparentColorFlag = 0;
			transparentColorIndex = 0;

			for (var i = 0; i < colorTable.Count; i++)
			{
				if (colorTable[i].a == 0)
				{
					colorTable[i] = GetTransparentColor(colorTable);
					transparentColorFlag = 1;
					transparentColorIndex = (byte) i;

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
				bytes[3 * i] = colorTable[i].r;
				bytes[3 * i + 1] = colorTable[i].g;
				bytes[3 * i + 2] = colorTable[i].b;
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

		private static int[] GetColorIndexes(Texture2D texture, List<Color32> colorTable, byte transparentColorFlag, byte transparentColorIndex, bool extendColorTable = false)
		{
			var pixels = texture.GetPixels32();
			var colorIndexes = new int[pixels.Length];

			for (var y = 0; y < texture.height; y++)
			{
				for (var x = 0; x < texture.width; x++)
				{
					var pixel = pixels[x + (texture.height - y - 1) * texture.width];

					if (transparentColorFlag == 1 && pixel.a == 0)
					{
						colorIndexes[x + y * texture.width] = transparentColorIndex;
					}
					else
					{
						var index = colorTable.IndexOf(pixel);

						if (index >= 0)
						{
							colorIndexes[x + y * texture.width] = index;
						}
						else if (extendColorTable)
						{
							if (colorTable.Count >= 256)
							{
								throw new Exception("Color table exceeds 256 size limit.");
							}

							colorIndexes[x + y * texture.width] = colorTable.Count;
							colorTable.Add(pixel);
						}
						else
						{
							throw new Exception("Color index not found: " + pixel);
						}
					}
				}
			}

			return colorIndexes;
		}

		private static GifFrame DecodeFrame(GraphicControlExtension extension, ImageDescriptor descriptor, TableBasedImageData data, bool filled, int width, int height, Color32[] state, Color32[] colorTable)
		{
			var frame = new GifFrame();
			var pixels = state;
			var transparentIndex = -1;

			if (extension != null)
			{
				frame.Delay = extension.DelayTime / 100f;
				frame.DisposalMethod = (DisposalMethod) extension.DisposalMethod;

				if (frame.DisposalMethod == DisposalMethod.RestoreToPrevious)
				{
					pixels = state.ToArray();
				}

				if (extension.TransparentColorFlag == 1)
				{
					transparentIndex = extension.TransparentColorIndex;
				}
			}

			var colorIndexes = LzwDecoder.Decode(data.ImageData, data.LzwMinimumCodeSize);

			for (var y = 0; y < descriptor.ImageHeight; y++)
			{
				for (var x = 0; x < descriptor.ImageWidth; x++)
				{
					var colorIndex = colorIndexes[x + y * descriptor.ImageWidth];
					var transparent = colorIndex == transparentIndex;

					if (transparent && !filled) continue;

					var color = transparent ? new Color32() : colorTable[colorIndex];
					var fx = x + descriptor.ImageLeftPosition;
					var fy = height - y - 1 - descriptor.ImageTopPosition; // Y-flip

					pixels[fx + fy * width] = pixels[fx + fy * width] = color;
				}
			}

			frame.Texture = new Texture2D(width, height);
			frame.Texture.SetPixels32(pixels);
			frame.Texture.Apply();

			return frame;
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