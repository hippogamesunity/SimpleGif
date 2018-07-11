using System;
using SimpleGif.Data;
using SimpleGif.Enums;

namespace SimpleGif.GifCore
{
	/// <summary>
	/// Converter textures.
	/// </summary>
	internal static class TextureConverter
	{
		/// <summary>
		/// /// Apply master palette to convert true color image to 256-color image.
		/// </summary>
		public static void ConvertTo8Bits(ref Texture2D texture, MasterPalette palette)
		{
			if (palette == MasterPalette.DontApply) return;

			var pixels = texture.GetPixels32();

			if (palette == MasterPalette.Grayscale)
			{
				for (var j = 0; j < pixels.Length; j++)
				{
					if (pixels[j].a < 128)
					{
						pixels[j] = new Color32();
					}
					else
					{
						var brightness = (byte) (0.2126 * pixels[j].r + 0.7152 * pixels[j].g + 0.0722 * pixels[j].b);
						var color = new Color32(brightness, brightness, brightness, 255);

						pixels[j] = color;
					}
				}

				texture.SetPixels32(pixels);
			}
			else
			{
				var levels = GetLevels(palette);
				var dividers = new[] { 256 / levels[0], 256 / levels[1], 256 / levels[2] };
				
				for (var j = 0; j < pixels.Length; j++)
				{
					var r = (byte) (pixels[j].r / dividers[0] * dividers[0]);
					var g = (byte) (pixels[j].g / dividers[1] * dividers[1]);
					var b = (byte) (pixels[j].b / dividers[2] * dividers[2]);
					var a = (byte) (pixels[j].a < 128 ? 0 : 255);
					var color = a == 0 ? new Color32() : new Color32(r, g, b, a);

					pixels[j] = color;
				}

				texture.SetPixels32(pixels);
			}
		}

		private static int[] GetLevels(MasterPalette palette)
		{
			switch (palette)
			{
				case MasterPalette.Levels666: return new[] { 6, 6, 6 };
				case MasterPalette.Levels676: return new[] { 6, 7, 6 };
				case MasterPalette.Levels685: return new[] { 6, 8, 5 };
				case MasterPalette.Levels884: return new[] { 8, 8, 4 };
				default: throw new ArgumentOutOfRangeException("Unsupported master palette: " + palette);
			}
		}
	}
}