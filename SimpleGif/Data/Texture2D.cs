using System.Linq;

namespace SimpleGif.Data
{
	/// <summary>
	/// Stub for Texture2D from UnityEngine.CoreModule
	/// </summary>
	public class Texture2D
	{
		// ReSharper disable once InconsistentNaming (original naming saved)
		public readonly int width;

		// ReSharper disable once InconsistentNaming (original naming saved)
		public readonly int height;
		
		private Color32[] _pixels;

		public Texture2D(int width, int height)
		{
			this.width = width;
			this.height = height;
		}

		public void SetPixels32(Color32[] pixels)
		{
			_pixels = pixels.ToArray();
		}

		public Color32[] GetPixels32()
		{
			return _pixels.ToArray();
		}

		public void Apply()
		{
		}
	}
}