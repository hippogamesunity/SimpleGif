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
				var clearCode = 1 << minCodeSize;
				var endOfInformation = clearCode + 1;
				var codeSize = minCodeSize + 1;
				Dictionary<int, List<int>> dict;
				int value;
				var colorIndexes = new List<int>();
				var index = codeSize;
				List<int> prev;

				void Clear()
				{
					codeSize = minCodeSize + 1;
					dict = InitializeDictionary(minCodeSize);
					value = ReadBits(bits, codeSize, ref index);
					colorIndexes.AddRange(prev = dict[value]);
				}

				Clear();

				while (index + codeSize <= bits.Length)
				{
					value = ReadBits(bits, codeSize, ref index);

					if (value == clearCode)
					{
						Clear();
						continue;
					}

					if (value == endOfInformation)
					{
						break;
					}

					if (dict.Count < 4096)
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

						if (dict.Count == 1 << codeSize && codeSize < 12)
						{
							codeSize++;
						}
					}

					colorIndexes.AddRange(prev = dict[value]);
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