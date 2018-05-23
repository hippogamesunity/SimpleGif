using System;

namespace SimpleGif.GifCore.Blocks
{
	internal abstract class Block
	{
		public const byte ExtensionIntroducer = 0x21;
		public const byte PlainTextExtensionLabel = 0x1;
		public const byte GraphicControlExtensionLabel = 0xF9;
		public const byte CommentExtensionLabel = 0xFE;
		public const byte ImageDescriptorLabel = 0x2C;
		public const byte ApplicationExtensionLabel = 0xFF;
		public const byte BlockTerminatoLabel = 0x00;

		protected byte[] ReadDataSubBlocks(byte[] bytes, ref int index)
		{
			byte[] data = null;

			while (bytes[index] > 0) // Sub-block size
			{
				var subBlock = BitHelper.ReadBytes(bytes, bytes[index++], ref index);

				if (data == null)
				{
					data = subBlock;
				}
				else
				{
					Array.Resize(ref data, data.Length + subBlock.Length);
					Buffer.BlockCopy(subBlock, 0, data, data.Length - subBlock.Length, subBlock.Length);
				}
			}

			return data;
		}
	}
}