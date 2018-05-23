using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleGif.GifCore
{
	public static class LzwEncoder
	{
		public static byte GetCodeSize(int[] colorIndexes)
		{
			byte codeSize = 0;
			var max = colorIndexes.Max();

			while (1 << codeSize <= max)
			{
				codeSize++;
			}

			return codeSize;
		}

		public static byte[] Encode(int[] colorIndexes, int minCodeSize)
		{
			var dict = InitializeDictionary(minCodeSize);
			var clearCode = 1 << minCodeSize;
			var endOfInformation = clearCode + 1;
			var code = colorIndexes[0].ToString();
			var codeSize = minCodeSize + 1;
			var bits = new List<bool>();

			ReadBits(clearCode, codeSize, ref bits);

			for (var i = 1; i < colorIndexes.Length; i++)
			{
				var next = code + " " + colorIndexes[i];

				if (dict.ContainsKey(next))
				{
					code = next;
				}
				else
				{
					ReadBits(FindKey(dict, code), codeSize, ref bits);
					dict.Add(next, dict.Count);
					code = colorIndexes[i].ToString();

					if (dict.Count - 1 == 1 << codeSize)
					{
						codeSize++;
					}
				}
			}

			ReadBits(FindKey(dict, code), codeSize, ref bits);
			ReadBits(endOfInformation, codeSize, ref bits);

			var bytes = GetBytes(bits);

			return bytes;
		}

		private static Dictionary<string, int> InitializeDictionary(int minCodeSize)
		{
			var dict = new Dictionary<string, int>();

			for (var i = 0; i < (1 << minCodeSize) + 2; i++)
			{
				dict.Add(i.ToString(), i);
			}

			return dict;
		}

		private static int FindKey(Dictionary<string, int> dict, string code)
		{
			return dict[code];
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
}