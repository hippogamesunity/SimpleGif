using System;
using SimpleGif.GifCore.Blocks;

namespace SimpleGif.GifCore
{
	internal class TableBasedImageData : Block
	{
		public byte LzwMinimumCodeSize;
		public byte[] ImageData;
		public byte BlockTerminator;

		public TableBasedImageData(byte[] bytes, ref int index)
		{
			LzwMinimumCodeSize = bytes[index++];
			ImageData = ReadDataSubBlocks(bytes, ref index);
			BlockTerminator = bytes[index++];

			if (BlockTerminator != 0x00) throw new Exception("0x00 expected!");
		}

		public TableBasedImageData(byte minCodeSize, byte[] imageData)
		{
			LzwMinimumCodeSize = minCodeSize;
			ImageData = imageData;
		}

		public byte[] GetBytes()
		{
			var bytes = new byte[ImageData.Length + (int) Math.Ceiling(ImageData.Length / 255d) + 2];
			var i = 0;
			var j = 0;

			bytes[0] = LzwMinimumCodeSize;
			j++;

			while (i < ImageData.Length)
			{
				var left = ImageData.Length - i;
				var size = (byte) Math.Min(255, left);
				
				bytes[j] = size;
				Array.Copy(ImageData, i, bytes, j + 1, size);
				j += size + 1;
				i += size;
			}

			bytes[bytes.Length - 1] = 0x00;

			return bytes;
		}
	}
}