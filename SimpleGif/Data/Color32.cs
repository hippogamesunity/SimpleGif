namespace SimpleGif.Data
{
	/// <summary>
	/// Stub for Color32 from UnityEngine.CoreModule
	/// </summary>
	public struct Color32
	{
		public readonly byte R;
		public readonly byte G;
		public readonly byte B;
		public readonly byte A;

		public Color32(byte r, byte g, byte b, byte a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public bool Equals(Color32 other)
		{
			return R == other.R && G == other.G && B == other.B && A == other.A;
		}
	}
}