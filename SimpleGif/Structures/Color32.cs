namespace SimpleGif.Structures
{
	/// <summary>
	/// Stab for Color32 from UnityEngine.CoreModule
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

		public static bool operator ==(Color32 c1, Color32 c2)
		{
			return c1.Equals(c2);
		}

		public static bool operator !=(Color32 c1, Color32 c2)
		{
			return !c1.Equals(c2);
		}

		public bool Equals(Color32 other)
		{
			return R == other.R && G == other.G && B == other.B && A == other.A;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;

			return obj is Color32 && Equals((Color32) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = R.GetHashCode();

				hashCode = (hashCode * 397) ^ G.GetHashCode();
				hashCode = (hashCode * 397) ^ B.GetHashCode();
				hashCode = (hashCode * 397) ^ A.GetHashCode();

				return hashCode;
			}
		}
	}
}