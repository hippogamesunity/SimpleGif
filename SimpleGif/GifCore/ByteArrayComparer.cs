using System.Collections.Generic;

namespace SimpleGif.GifCore
{
	internal sealed class ByteArrayComparer : IEqualityComparer<byte[]>
	{
		public bool Equals(byte[] x, byte[] y)
		{
            if (x.Length != y.Length) return false;

			for (var i = 0; i < x.Length; i++)
			{
				if (x[i] != y[i]) return false;
			}

			return true;
		}

		public int GetHashCode(byte[] array)
		{
			var hash = array.Length;

			for (var i = 0; i < array.Length; i++)
			{
				hash = unchecked(hash * 314159 + array[i]);
			}

			return hash;
		}
    }
}