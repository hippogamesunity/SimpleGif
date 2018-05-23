using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleGif.GifCore
{
	namespace Assets.GifCore
	{
		internal static class LzwDecoder
		{
			public static int[] Decode(byte[] bytes, int minCodeSize)
			{
				var bits = new BitArray(bytes);
				var colorIndexes = new List<int>();
				var dict = InitializeDictionary(minCodeSize);
				var endOfInformation = (1 << minCodeSize) + 1;
				var codeSize = minCodeSize + 1;
				var index = codeSize;

				List<int> prev = null;

				while (index + codeSize <= bits.Length)
				{
					var value = ReadBits(bits, codeSize, ref index);

					if (value == endOfInformation) break;

					if (prev != null && dict.Count < 4096)
					{
						var code = prev.ToList();

						if (dict.ContainsKey(value))
						{
							code.Add(dict[value][0]);
							dict.Add(dict.Count, code);
						}
						else
						{
							code.Add(prev[0]);
							dict.Add(value, code);
						}

						if (dict.Count == 1 << codeSize)
						{
							codeSize++;
						}
					}

					prev = dict[value];

					foreach (var colorIndex in dict[value])
					{
						colorIndexes.Add(colorIndex);
					}
				}

				return colorIndexes.ToArray();
			}

			private static Dictionary<int, List<int>> InitializeDictionary(int minCodeSize)
			{
				var dict = new Dictionary<int, List<int>>();

				for (var i = 0; i < (1 << minCodeSize) + 2; i++)
				{
					dict.Add(i, new List<int> { i });
				}

				return dict;
			}

			private static int ReadBits(BitArray bits, int size, ref int cursor)
			{
				var value = 0;

				for (var i = 0; i < size; i++)
				{
					if (bits[cursor + i])
					{
						value += 1 << i;
					}
				}

				cursor += size;

				return value;
			}
		}
	}
}