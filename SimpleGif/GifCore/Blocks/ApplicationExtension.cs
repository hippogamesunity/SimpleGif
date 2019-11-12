using System;

namespace SimpleGif.GifCore.Blocks
{
	internal class ApplicationExtension : Block
	{
		public byte BlockSize;
		public byte[] ApplicationIdentifier;
		public byte[] ApplicationAuthenticationCode;
		public byte[] ApplicationData;

		public ApplicationExtension(byte[] bytes, ref int index)
		{
			if (bytes[index++] != ExtensionIntroducer) throw new Exception("Expected: " + ExtensionIntroducer);
			if (bytes[index++] != ApplicationExtensionLabel) throw new Exception("Expected: " + ApplicationExtensionLabel);

			BlockSize = bytes[index++];
			ApplicationIdentifier = BitHelper.ReadBytes(bytes, 8, ref index);
			ApplicationAuthenticationCode = BitHelper.ReadBytes(bytes, 3, ref index);
			ApplicationData = ReadDataSubBlocks(bytes, ref index);

			if (bytes[index++] != BlockTerminatorLabel) throw new Exception("Expected: " + BlockTerminatorLabel);
		}

		public ApplicationExtension()
		{
		}

		public byte[] GetBytes()
		{
			return new byte[] { 0x21, 0xFF, 0x0B, 0x4E, 0x45, 0x54, 0x53, 0x43, 0x41, 0x50, 0x45, 0x32, 0x2E, 0x30, 0x03, 0x01, 0x00, 0x00, 0x00 };
		}
	}
}