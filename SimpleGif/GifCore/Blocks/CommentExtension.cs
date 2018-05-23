using System;

namespace SimpleGif.GifCore.Blocks
{
	public class CommentExtension : Block
	{
		public byte[] CommentData;

		public CommentExtension(byte[] bytes, ref int index)
		{
			if (bytes[index++] != ExtensionIntroducer) throw new Exception("Expected: " + ExtensionIntroducer);
			if (bytes[index++] != CommentExtensionLabel) throw new Exception("Expected: " + CommentExtensionLabel);

			CommentData = ReadDataSubBlocks(bytes, ref index);

			if (bytes[index++] != BlockTerminatoLabel) throw new Exception("Expected: " + BlockTerminatoLabel);
		}
	}
}