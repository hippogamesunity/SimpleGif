namespace SimpleGif.Structures
{
	/// <summary>
	/// Stab for Texture2D from UnityEngine.CoreModule
	/// </summary>
	public class Texture2D
	{
		private Color32[] _pixels;

		public int Width { get; }
		public int Height { get; }

		public Texture2D(int width, int height)
		{
			Width = width;
			Height = height;
		}

		public void SetPixels32(Color32[] pixels)
		{
			_pixels = pixels;
		}

		public Color32[] GetPixels32()
		{
			return _pixels;
		}

		public void Apply()
		{
		}
	}
}