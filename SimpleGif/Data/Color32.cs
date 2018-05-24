namespace SimpleGif.Data
{
	/// <summary>
	/// Stub for Color32 from UnityEngine.CoreModule
	/// </summary>
	public struct Color32
	{
		// ReSharper disable InconsistentNaming (original naming saved)
		public readonly byte r;
		public readonly byte g;
		public readonly byte b;
		public readonly byte a;
		// ReSharper restore InconsistentNaming

		public Color32(byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public bool Equals(Color32 other)
		{
			return r == other.r && g == other.g && b == other.b && a == other.a;
		}
	}
}