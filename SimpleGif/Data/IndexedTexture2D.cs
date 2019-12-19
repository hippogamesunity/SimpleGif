using System.Linq;

namespace SimpleGif.Data
{
	/// <summary>
	/// Stub for Texture2D from UnityEngine.CoreModule
	/// </summary>
	public class IndexedTexture2D
    {
		// ReSharper disable once InconsistentNaming (original naming saved)
		public readonly int width;

		// ReSharper disable once InconsistentNaming (original naming saved)
		public readonly int height;

        private Color32[] _colorTable;
        private int[] _colorIndexes;

		public IndexedTexture2D(int width, int height)
		{
			this.width = width;
			this.height = height;
		}

		public void SetPixels32(Color32[] colorTable, int[] colorIndexes)
		{
		    _colorTable = colorTable.ToArray();
		    _colorIndexes = colorIndexes.ToArray();
		}

		public Color32[] GetPixels32()
		{
            var emptyColor = new Color32();

			return _colorIndexes.Select(i => i == -1 ? emptyColor : _colorTable[i]).ToArray();
		}

		public void Apply()
		{
		}
	}
}