using System;
using System.Diagnostics;
using System.IO;
using SimpleGif;

namespace Example
{
	internal class Program
	{
		private static void Main()
		{
			const string path = "Sample.gif";
			var bytes = File.ReadAllBytes(path);

			var stopwatch = new Stopwatch();

			stopwatch.Start();

			var gif = Gif.Decode(bytes);

			stopwatch.Stop();

			Console.WriteLine("GIF loaded in {0:n2}s, size: {1}x{2}, frames: {3}.", stopwatch.Elapsed.TotalSeconds, gif.Frames[0].Texture.width, gif.Frames[0].Texture.height, gif.Frames.Count);

			stopwatch.Reset();
			stopwatch.Start();

			var binary = gif.Encode();

			stopwatch.Stop();

			Console.WriteLine("GIF encoded in {0:n2}s to binary.", stopwatch.Elapsed.TotalSeconds);

			stopwatch.Reset();
			stopwatch.Start();

			Gif.Decode(binary);

			Console.WriteLine("GIF loaded from binary in {0:n2}s.", stopwatch.Elapsed.TotalSeconds);

			File.WriteAllBytes(path.Replace(".gif", "_.gif"), binary);

			Console.WriteLine("GIF saved as {0}.", path);
			Console.WriteLine("Test passed!");
			Console.Read();
		}

		private double StopwatchAction(Action action, string message)
		{
			var stopwatch = new Stopwatch();

			stopwatch.Start();
			action();
			stopwatch.Stop();

			return stopwatch.Elapsed.TotalSeconds;
		}
	}
}