using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SimpleGif;
using SimpleGif.Data;

namespace Example
{
	internal class Program
	{
		private const string Path = "LargeSample.gif";

		public static void Main()
		{
			var gif = DecodeIteratorExample();

			EncodeIteratorExample(gif);
			Console.Read();
		}

		public static Gif DecodeExample()
		{
			var bytes = File.ReadAllBytes(Path);
			var stopwatch = new Stopwatch();

			stopwatch.Start();

			var gif = Gif.Decode(bytes);

			stopwatch.Stop();

			Console.WriteLine("GIF loaded in {0:n2}s, size: {1}x{2}, frames: {3}.", stopwatch.Elapsed.TotalSeconds,
				gif.Frames[0].Texture.width, gif.Frames[0].Texture.height, gif.Frames.Count);

			stopwatch.Reset();
			stopwatch.Start();

			return gif;
		}

		public static void EncodeExample(Gif gif)
		{
			var stopwatch = new Stopwatch();

			stopwatch.Start();

			var binary = gif.Encode();

			stopwatch.Stop();

			Console.WriteLine("GIF encoded in {0:n2}s to binary.", stopwatch.Elapsed.TotalSeconds);
		}

		public static void EncodeDecodeSaveTest()
		{
			var gif = DecodeExample();
			var stopwatch = new Stopwatch();

			stopwatch.Start();

			var binary = gif.Encode();

			stopwatch.Stop();

			Console.WriteLine("GIF encoded in {0:n2}s to binary.", stopwatch.Elapsed.TotalSeconds);

			stopwatch.Reset();
			stopwatch.Start();

			Gif.Decode(binary);

			Console.WriteLine("GIF loaded from binary in {0:n2}s.", stopwatch.Elapsed.TotalSeconds);

			var path = Path.Replace(".gif", "_.gif");

			File.WriteAllBytes(path, binary);

			Console.WriteLine("GIF saved as {0}.", path);
			Console.WriteLine("Test passed!");
		}

		/// <summary>
		/// Iterator can be used for large GIF-files in order to display progress bar.
		/// </summary>
		public static Gif DecodeIteratorExample()
		{
			var bytes = File.ReadAllBytes(Path);
			var parts = Gif.DecodeIterator(bytes);
			var frames = new List<GifFrame>();
			var stopwatch = new Stopwatch();
			var index = 0;
			var time = 0d;

			stopwatch.Start();

			foreach (var frame in parts)
			{
				frames.Add(frame);
				stopwatch.Stop();
				time += stopwatch.Elapsed.TotalSeconds;

				Console.WriteLine("GIF frame #{0} loaded in {1:n4}s", index++, stopwatch.Elapsed.TotalSeconds);

				stopwatch.Reset();
				stopwatch.Start();
			}

			Console.WriteLine("GIF loaded with iterator in {0:n4}s", time);

			return new Gif(frames);
		}

		/// <summary>
		/// Iterator can be used for large GIF-files in order to display progress bar.
		/// </summary>
		public static byte[] EncodeIteratorExample(Gif gif)
		{
			var bytes = new List<byte>();
			var parts = gif.EncodeIterator();
			var stopwatch = new Stopwatch();
			var index = 0;
			var time = 0d;

			stopwatch.Start();

			foreach (var part in parts)
			{
				bytes.AddRange(part);
				stopwatch.Stop();
				time += stopwatch.Elapsed.TotalSeconds;

				Console.WriteLine("GIF part #{0} encoded in {1:n4}s", index++, stopwatch.Elapsed.TotalSeconds);

				stopwatch.Reset();
				stopwatch.Start();
			}

			Console.WriteLine("GIF encoded with iterator in {0:n4}s", time);

			return bytes.ToArray();
		}
	}
}