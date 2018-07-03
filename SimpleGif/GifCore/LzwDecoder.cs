using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleGif.GifCore
{
	namespace Assets.GifCore
	{
		internal static class LzwDecoder
		{
			public static byte[] Decode(byte[] bytes, int minCodeSize)
			{
				var bits = new BitArray(bytes);
				var clearCode = 1 << minCodeSize;
				var endOfInformation = clearCode + 1;
				var codeSize = minCodeSize + 1;
				var dict = InitializeDictionary(minCodeSize);
				var index = codeSize;
				var value = ReadBits(bits, codeSize, ref index);
				var prev = dict[value];
				var colorIndexes = prev.ToList();
				
				while (index + codeSize <= bits.Length)
				{
					value = ReadBits(bits, codeSize, ref index);

					if (value == clearCode)
					{
						codeSize = minCodeSize + 1;
						dict = InitializeDictionary(minCodeSize);
						value = ReadBits(bits, codeSize, ref index);
						colorIndexes.AddRange(prev = dict[value]);
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

			private static Dictionary<int, List<byte>> InitializeDictionary(int minCodeSize)
			{
				var dict = new Dictionary<int, List<byte>>();

				for (var i = 0; i < (1 << minCodeSize) + 2; i++)
				{
					dict.Add(i, new List<byte> { (byte) i });
				}

				return dict;
			}

			private static int ReadBits(BitArray bits, int size, ref int cursor) // TODO: Most 'heavy' operation
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