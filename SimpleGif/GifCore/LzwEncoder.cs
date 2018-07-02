using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleGif.GifCore
{
	internal static class LzwEncoder
	{
		public static byte GetMinCodeSize(int[] colorIndexes)
		{
			byte minCodeSize = 2;
			var max = colorIndexes.Max();

			while (1 << minCodeSize <= max)
			{
				minCodeSize++;
			}

			return minCodeSize;
		}

		public static byte[] Encode(int[] colorIndexes, int minCodeSize)
		{
			var codeCache = Enumerable.Range(byte.MinValue, byte.MaxValue).Select(b => new[] { (byte) b }).ToArray();

			var dict = InitializeDictionary(minCodeSize, codeCache);
			var clearCode = 1 << minCodeSize;
			var endOfInformation = clearCode + 1;
			var code = codeCache[colorIndexes[0]];
			var codeSize = minCodeSize + 1;
			var bits = new List<bool>();

			ReadBits(clearCode, codeSize, ref bits);

			for (var i = 1; i < colorIndexes.Length; i++)
			{
				var next = code.Add((byte) colorIndexes[i]);
				if (dict.ContainsKey(next))
				{
					code = next;
				}
				else
				{
					ReadBits(dict[code], codeSize, ref bits);
					code = codeCache[(byte) colorIndexes[i]];

					if (dict.Count < 4096)
					{
						dict.Add(next, dict.Count);

						if (dict.Count - 1 == 1 << codeSize)
						{
							codeSize++;
						}
					}
				}
			}

			ReadBits(dict[code], codeSize, ref bits);
			ReadBits(endOfInformation, codeSize, ref bits);

			var bytes = GetBytes(bits);

			return bytes;
		}

		private static Dictionary<byte[], int> InitializeDictionary(int minCodeSize, byte[][] codeCache)
		{
			var dict = new Dictionary<byte[], int>(new ByteArrayEqualityComparer());

			for (var i = 0; i < (1 << minCodeSize) + 2; i++)
			{
				dict.Add(codeCache[i], i);
			}

			return dict;
		}

		private static void ReadBits(int key, int codeSize, ref List<bool> destination)
		{
			for (var j = 0; j < codeSize; j++)
			{
				destination.Add(GetBit(key, j));
			}
		}

		private static bool GetBit(int value, int index)
		{
			return (value & (1 << index)) != 0;
		}

		private static byte[] GetBytes(List<bool> bits)
		{
			var array = new BitArray(bits.Count);

			for (var i = 0; i < bits.Count; i++)
			{
				array[i] = bits[i];
			}

			var bytes = new byte[(int)Math.Ceiling(array.Length / 8d)];

			array.CopyTo(bytes, 0);

			return bytes;
		}
	}

	internal class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
	{
		public bool Equals(byte[] x, byte[] y)
		{
			if (x.Length != y.Length)
				return false;

			for (int i = 0; i < x.Length; i++)
			{
				if (x[i] != y[i])
					return false;
			}

			return true;
		}

		public int GetHashCode(byte[] obj)
		{
			int hash = obj.Length;
			for (int i = 0; i < obj.Length; i++)
				hash = unchecked(hash * 314159 + obj[i]);

			return hash;
		}
	}

	internal static class ByteArrayHelper
	{
		public static byte[] Add(this byte[] bytes, byte val)
		{
			var newBytes = new byte[bytes.Length + 1];
			Array.Copy(bytes, newBytes, bytes.Length);
			newBytes[bytes.Length] = val;
			return newBytes;
		}
	}
}