﻿using System;

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
			return r == other.r && g == other.g && b == other.b && a == other.a;
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
				return r + 256 * g + 65536 * b + 16777216 * a;
			}
		}
	}
}