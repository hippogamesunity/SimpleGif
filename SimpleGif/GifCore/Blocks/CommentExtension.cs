using System;

namespace SimpleGif.GifCore.Blocks
{
	internal class CommentExtension : Block
	{
		public byte[] CommentData;

		public CommentExtension(byte[] bytes, ref int index)
		{
			if (bytes[index++] != ExtensionIntroducer) throw new Exception("Expected: " + ExtensionIntroducer);
			if (bytes[index++] != CommentExtensionLabel) throw new Exception("Expected: " + CommentExtensionLabel);

			CommentData = ReadDataSubBlocks(bytes, ref index);

			if (bytes[index++] != BlockTerminatorLabel) throw new Exception("Expected: " + BlockTerminatorLabel);
		}
	}
}