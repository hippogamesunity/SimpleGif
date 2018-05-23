using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SimpleGif;

namespace Example
{
	internal class Program
	{
		private static void Main()
		{
			const string path = "Hentai.gif";
			var bytes = File.ReadAllBytes(path);
			var gif = Gif.FromBytes(bytes);

			Console.WriteLine("GIF size: {0}x{1}, frames: {2}", gif.Frames[0].Texture.Width, gif.Frames[0].Texture.Height, gif.Frames.Count);

			//gif.Frames = gif.Frames.Take(10).ToList();

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var binary = gif.GetBytes();
			stopwatch.Stop();
			Console.WriteLine("Operation completed in {0:N2}s", stopwatch.Elapsed.TotalSeconds);

			File.WriteAllBytes(path.Replace(".gif", "_.gif"), binary);

			Gif.FromBytes(binary);

			Console.WriteLine("Test passed!");
			Console.Read();
		}

		private void StopwatchAction(Action action)
		{
			var stopwatch = new Stopwatch();

			stopwatch.Start();
			action();
			stopwatch.Stop();
			Console.WriteLine("Operation completed in {0:N2}s", stopwatch.Elapsed.TotalSeconds);
		}
	}
}