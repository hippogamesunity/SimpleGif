﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
			return new Gif(DecodeIterator(bytes).ToList());
		}

		/// <summary>
		/// Decode byte array in multiple threads.
		/// </summary>
		public static void DecodeParallel(byte[] bytes, Action<DecodeProgress> onProgress) // TODO: Refact
		{
			var parser = new GifParser(bytes);
			var decoded = new Dictionary<ImageDescriptor, byte[]>();
			var frameCount = parser.Blocks.Count(i => i is ImageDescriptor);
			var decodeProgress = new DecodeProgress { FrameCount = frameCount };

			for (var i = 0; i < parser.Blocks.Count; i++)
			{
				var imageDescriptor = parser.Blocks[i] as ImageDescriptor;

				if (imageDescriptor == null) continue;

				var data = (TableBasedImageData) parser.Blocks[i + 1 + imageDescriptor.LocalColorTableFlag];

				ThreadPool.QueueUserWorkItem(context =>
				{
					var colorIndexes = LzwDecoder.Decode(data.ImageData, data.LzwMinimumCodeSize);

					lock (decoded)
					{
						decoded.Add(imageDescriptor, colorIndexes);
						decodeProgress.Progress++;
						
						if (decoded.Count == frameCount)
						{
							decodeProgress.Gif = CompleteDecode(parser, decoded);
							decodeProgress.Completed = true;
						}

						onProgress(decodeProgress);
					}
				});
			}
		}

		private static Gif CompleteDecode(GifParser parser, IDictionary<ImageDescriptor, byte[]> decoded)
		{
			var globalColorTable = parser.LogicalScreenDescriptor.GlobalColorTableFlag == 1 ? GetUnityColors(parser.GlobalColorTable) : null;
			var backgroundColor = globalColorTable?[parser.LogicalScreenDescriptor.BackgroundColorIndex] ?? new Color32();
			GraphicControlExtension graphicControlExtension = null;
			var width = parser.LogicalScreenDescriptor.LogicalScreenWidth;
			var height = parser.LogicalScreenDescriptor.LogicalScreenHeight;
			var state = new Color32[width * height];
			var filled = false;
			var frames = new List<GifFrame>();

			for (var j = 0; j < parser.Blocks.Count; j++)
			{
				if (parser.Blocks[j] is GraphicControlExtension)
				{
					graphicControlExtension = (GraphicControlExtension) parser.Blocks[j];
				}
				else if (parser.Blocks[j] is ImageDescriptor)
				{
					var imageDescriptor = (ImageDescriptor) parser.Blocks[j];

					if (imageDescriptor.InterlaceFlag == 1) throw new NotSupportedException("Interlacing is not supported!");

					var colorTable = imageDescriptor.LocalColorTableFlag == 1 ? GetUnityColors((ColorTable) parser.Blocks[j + 1]) : globalColorTable;
					var colorIndexes = decoded[imageDescriptor];
					var frame = DecodeFrame(graphicControlExtension, imageDescriptor, colorIndexes, filled, width, height, state, colorTable);

					frames.Add(frame);

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
				}
			}

			return new Gif(frames);
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
				if (blocks[j] is GraphicControlExtension)
				{
					graphicControlExtension = (GraphicControlExtension)blocks[j];
				}
				else if (blocks[j] is ImageDescriptor)
				{
					var imageDescriptor = (ImageDescriptor)blocks[j];

					if (imageDescriptor.InterlaceFlag == 1) throw new NotSupportedException("Interlacing is not supported!");

					var colorTable = imageDescriptor.LocalColorTableFlag == 1 ? GetUnityColors((ColorTable) blocks[j + 1]) : globalColorTable;
					var data = (TableBasedImageData)blocks[j + 1 + imageDescriptor.LocalColorTableFlag];
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
			var bytes = new List<byte>();
			var iterator = EncodeIterator();
			var iteratorSize = GetEncodeIteratorSize();
			var index = 0;

			foreach (var part in iterator)
			{
				if (index == iteratorSize - 1) // GIF header should be placed to sequence start!
				{
					bytes.InsertRange(0, part);
				}
				else
				{
					bytes.AddRange(part);
				}

				index++;
			}

			return bytes.ToArray();
		}

		/// <summary>
		/// Encode GIF in multiple threads.
		/// </summary>
		public void EncodeParallel(Action<EncodeProgress> onProgress) // TODO: Refact
		{
			const string header = "GIF89a";
			var width = (ushort) Frames[0].Texture.width;
			var height = (ushort) Frames[0].Texture.height;
			var globalColorTable = new List<Color32> { new Color32() };
			var applicationExtension = new ApplicationExtension();
			var encoded = new Dictionary<int, List<byte>>();
			var encodeProgress = new EncodeProgress { FrameCount = Frames.Count };
			var colorTables = new List<Color32>[Frames.Count];
			var distinctColors = new Dictionary<int, List<Color32>>();
			var manualResetEvent = new ManualResetEvent(false);

			for (var i = 0; i < Frames.Count; i++)
			{
				var frame = Frames[i];

				ThreadPool.QueueUserWorkItem(context =>
				{
					var distinct = frame.Texture.GetPixels32().Distinct().Where(j => j.a != 0).ToList();

					lock (distinctColors)
					{
						distinctColors.Add((int) context, distinct);

						if (distinctColors.Count == Frames.Count) manualResetEvent.Set();
					}
				}, i);
			}

			manualResetEvent.WaitOne();

			for (var i = 0; i < Frames.Count; i++)
			{
				var colors = distinctColors[i];
				var add = colors.Where(j => !globalColorTable.Contains(j)).ToList();

				if (globalColorTable.Count + add.Count <= 256)
				{
					globalColorTable.AddRange(add);
					colorTables[i] = globalColorTable;
				}
				else if (add.Count <= 256) // Introducing local color table
				{
					colorTables[i] = colors;
				}
				else
				{
					throw new Exception($"Frame #{i} contains more than 256 colors!");
				}
			}

			for (var i = 0; i < Frames.Count; i++) // Don't use Parallel.For to leave .NET compatibility.
			{
				ThreadPool.QueueUserWorkItem(context =>
				{
					var index = (int) context;
					var colorTable = colorTables[index];
					var localColorTableFlag = (byte) (colorTable == globalColorTable ? 0 : 1);
					var localColorTableSize = GetColorTableSize(colorTable);
					byte transparentColorFlag;
					byte transparentColorIndex;
					var colorIndexes = GetColorIndexes(Frames[index].Texture, colorTable, out transparentColorFlag, out transparentColorIndex);
					var imageDescriptor = new ImageDescriptor(0, 0, width, height, localColorTableFlag, 0, 0, 0, localColorTableSize);
					var graphicControlExtension = new GraphicControlExtension(4, 0, (byte) Frames[index].DisposalMethod, 0, transparentColorFlag, (ushort) (100 * Frames[index].Delay), transparentColorIndex);
					var minCodeSize = LzwEncoder.GetMinCodeSize(colorIndexes);
					var lzw = LzwEncoder.Encode(colorIndexes, minCodeSize);
					var tableBasedImageData = new TableBasedImageData(minCodeSize, lzw);
					var bytes = new List<byte>();

					bytes.AddRange(graphicControlExtension.GetBytes());
					bytes.AddRange(imageDescriptor.GetBytes());

					if (localColorTableFlag == 1)
					{
						bytes.AddRange(ColorTableToBytes(colorTable, localColorTableSize));
					}

					bytes.AddRange(tableBasedImageData.GetBytes());

					lock (encoded)
					{
						encoded.Add(index, bytes);
						encodeProgress.Progress++;
						
						if (encoded.Count == Frames.Count)
						{
							globalColorTable[0] = GetTransparentColor(globalColorTable);

							var globalColorTableSize = GetColorTableSize(globalColorTable);
							var logicalScreenDescriptor = new LogicalScreenDescriptor(width, height, 1, 7, 0, globalColorTableSize, 0, 0);
							var binary = new List<byte>();

							binary.AddRange(Encoding.UTF8.GetBytes(header));
							binary.AddRange(logicalScreenDescriptor.GetBytes());
							binary.AddRange(ColorTableToBytes(globalColorTable, globalColorTableSize));
							binary.AddRange(applicationExtension.GetBytes());
							binary.AddRange(encoded.OrderBy(j => j.Key).SelectMany(j => j.Value));
							binary.Add(0x3B); // GIF Trailer.

							encodeProgress.Bytes = binary.ToArray();
							encodeProgress.Completed = true;
						}

						onProgress(encodeProgress);
					}
				}, i);
			}
		}

		/// <summary>
		/// Iterator can be used for large GIF-files in order to display progress bar.
		/// </summary>
		public IEnumerable<List<byte>> EncodeIterator()
		{
			const string header = "GIF89a";
			var width = (ushort) Frames[0].Texture.width;
			var height = (ushort) Frames[0].Texture.height;
			var globalColorTable = new List<Color32> { new Color32() };
			var applicationExtension = new ApplicationExtension();
			List<byte> bytes;

			foreach (var frame in Frames)
			{
				var colors = frame.Texture.GetPixels32().Distinct().ToList();
				var add = colors.Where(i => !globalColorTable.Contains(i)).ToList();
				byte localColorTableFlag = 0;
				byte localColorTableSize = 0;
				
				List<Color32> colorTable;

				if (globalColorTable.Count + add.Count <= 256)
				{
					globalColorTable.AddRange(add);
					colorTable = globalColorTable;
				}
				else if (add.Count <= 256) // Introducing local color table
				{
					colorTable = colors;
					localColorTableFlag = 1;
					localColorTableSize = GetColorTableSize(colorTable);
				}
				else
				{
					throw new Exception($"Frame #{Frames.IndexOf(frame)} contains more than 256 colors!");
				}

				bytes = new List<byte>();

				byte transparentColorFlag;
				byte transparentColorIndex;
				var colorIndexes = GetColorIndexes(frame.Texture, colorTable, out transparentColorFlag, out transparentColorIndex);
				var graphicControlExtension = new GraphicControlExtension(4, 0, (byte) frame.DisposalMethod, 0, transparentColorFlag, (ushort) (100 * frame.Delay), transparentColorIndex);
				var imageDescriptor = new ImageDescriptor(0, 0, width, height, localColorTableFlag, 0, 0, 0, localColorTableSize);
				var minCodeSize = LzwEncoder.GetMinCodeSize(colorIndexes);
				var encoded = LzwEncoder.Encode(colorIndexes, minCodeSize);
				var tableBasedImageData = new TableBasedImageData(minCodeSize, encoded);

				bytes.AddRange(graphicControlExtension.GetBytes());
				bytes.AddRange(imageDescriptor.GetBytes());

				if (localColorTableFlag == 1)
				{
					bytes.AddRange(ColorTableToBytes(colorTable, localColorTableSize));
				}

				bytes.AddRange(tableBasedImageData.GetBytes());

				yield return bytes;
			}

			yield return new List<byte> { 0x3B }; // GIF Trailer.

			// Then output GIF header as last iterator element! This way we can build global color table "on fly" instead of expensive building operation.

			globalColorTable[0] = GetTransparentColor(globalColorTable);

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

		private static byte[] GetColorIndexes(Texture2D texture, IList<Color32> colorTable, out byte transparentColorFlag, out byte transparentColorIndex)
		{
			transparentColorFlag = 0;
			transparentColorIndex = 0;

			var pixels = texture.GetPixels32();
			var colorIndexes = new byte[pixels.Length];

			for (var y = 0; y < texture.height; y++)
			{
				for (var x = 0; x < texture.width; x++)
				{
					var pixel = pixels[x + (texture.height - y - 1) * texture.width];

					if (pixel.a == 0)
					{
						if (transparentColorFlag == 0)
						{
							transparentColorFlag = 1;
							transparentColorIndex = (byte) colorTable.IndexOf(pixel);
						}
						
						colorIndexes[x + y * texture.width] = transparentColorIndex;
					}
					else
					{
						var index = colorTable.IndexOf(pixel);

						if (index >= 0)
						{
							colorIndexes[x + y * texture.width] = (byte) index;
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

		private static GifFrame DecodeFrame(GraphicControlExtension extension, ImageDescriptor descriptor, byte[] colorIndexes, bool filled, int width, int height, Color32[] state, Color32[] colorTable)
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

					color.a = 255;
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