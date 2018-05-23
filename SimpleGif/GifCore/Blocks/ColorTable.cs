using System;

namespace SimpleGif.GifCore.Blocks
{
	internal class ColorTable : Block
	{
		public byte[] Bytes;

		public ColorTable(int size, byte[] bytes, ref int index)
		{
			var length = 3 * (int) Math.Pow(2, size + 1);

			Bytes = BitHelper.ReadBytes(bytes, length, ref index);
		}
	}
}