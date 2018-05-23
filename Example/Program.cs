using System;
using System.IO;
using SimpleGif;

namespace Example
{
	internal class Program
	{
		private static void Main()
		{
			const string path = "Girl.gif";
			var bytes = File.ReadAllBytes(path);
			var gif = Gif.FromBytes(bytes);

			Console.WriteLine("GIF size: {0}x{1}, frames: {2}", gif.Frames[0].Texture.Width, gif.Frames[0].Texture.Height, gif.Frames.Count);

			var binary = gif.GetBytes();

			File.WriteAllBytes(path.Replace(".gif", "_.gif"), binary);

			Console.WriteLine("Test passed!");
			Console.Read();
		}
	}
}