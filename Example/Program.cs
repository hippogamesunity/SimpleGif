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
			const string path = "Pacman.gif";
			var bytes = File.ReadAllBytes(path);
			var gif = Gif.FromBytes(bytes);

				Console.WriteLine("GIF loaded, size: {0}x{1}, frames: {2}.", gif.Frames[0].Texture.Width, gif.Frames[0].Texture.Height, gif.Frames.Count);

			var binary = gif.GetBytes();

				Console.WriteLine("GIF encoded to binary.");

			Gif.FromBytes(binary);

				Console.WriteLine("GIF loaded from binary.");

			File.WriteAllBytes(path.Replace(".gif", "_.gif"), binary);

				Console.WriteLine("GIF saved as {0}.", path);
				Console.WriteLine("Test passed!");
				Console.Read();
		}
	}
}