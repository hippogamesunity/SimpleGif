using System;

namespace SimpleGif.GifCore
{
	internal class BitHelper
	{
		public static byte[] ReadBytes(byte[] bytes, int length, ref int index)
		{
			var sequence = new byte[length];

			Array.Copy(bytes, index, sequence, 0, length);
			index += length;

			return sequence;
		}

		public static ushort ReadInt16(byte[] bytes, ref int index)
		{
			var value = (ushort) BitConverter.ToInt16(bytes, index);

			index += 2;

			return value;
		}

		public static byte ReadPackedByte(byte input, int start, int count)
		{
			var shift = 8 - (start + count);
			var mask = (1 << count) - 1;
			var result = (input >> shift) & mask;

			return (byte) result;
		}

		public static byte PackByte(bool bit0, bool bit1, bool bit2, bool bit3, bool bit4, bool bit5, bool bit6, bool bit7)
		{
			byte packedByte = 0;

			if (bit0) packedByte |= 1 << 7;
			if (bit1) packedByte |= 1 << 6;
			if (bit2) packedByte |= 1 << 5;
			if (bit3) packedByte |= 1 << 4;
			if (bit4) packedByte |= 1 << 3;
			if (bit5) packedByte |= 1 << 2;
			if (bit6) packedByte |= 1 << 1;
			if (bit7) packedByte |= 1 << 0;

			return packedByte;
		}

		public static bool ReadByte(byte value, int index)
		{
			return (value & (1 << index)) != 0;
		}
	}
}