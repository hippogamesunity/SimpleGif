using System;

namespace SimpleGif.Data
{
	/// <summary>
	/// Stub for Color32 from UnityEngine.CoreModule
	/// </summary>
	public struct Color32 : IEquatable<Color32>
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
			return a == 0 && other.a == 0 || r == other.r && g == other.g && b == other.b && a > 0 && other.a > 0;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType()) return false;

			var other = (Color32) obj;

			return Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return a == 0 ? 0 : r + 256 * g + 65536 * b;
			}
		}
	}
}