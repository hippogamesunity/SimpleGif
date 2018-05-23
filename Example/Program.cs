using System;
using System.IO;
using SimpleGif;

namespace Example
{
	internal class Program
	{
		private static void Main()
		{
			var bytes = File.ReadAllBytes("Pacman.gif");
			var gif = Gif.FromBytes(bytes);

			Console.WriteLine("GIF size: {0}x{1}, frames: {2}", gif.Frames[0].Texture.Width, gif.Frames[0].Texture.Height, gif.Frames.Count);

			var binary = gif.GetBytes();

			File.WriteAllBytes("Pacman_.gif", binary);

			Console.WriteLine("Test passed!");
			Console.Read();
		}
	}
}