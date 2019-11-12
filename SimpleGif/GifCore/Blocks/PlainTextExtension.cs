using System;

namespace SimpleGif.GifCore.Blocks
{
	internal class PlainTextExtension : Block
	{
		public byte BlockSize;
		public ushort TextGridLeftPosition;
		public ushort TextGridTopPosition;
		public ushort TextGridWidth;
		public ushort TextGridHeight;
		public byte CharacterCellWidth;
		public byte CharacterCellHeight;
		public byte TextForegroundColorIndex;
		public byte TextBackgroundColorIndex;
		public byte[] PlainTextData;

		public PlainTextExtension(byte[] bytes, ref int index)
		{
			if (bytes[index++] != ExtensionIntroducer) throw new Exception("Expected :" + ExtensionIntroducer);
			if (bytes[index++] != PlainTextExtensionLabel) throw new Exception("Expected :" + PlainTextExtensionLabel);

			BlockSize = bytes[index++];
			TextGridLeftPosition = BitHelper.ReadInt16(bytes, ref index);
			TextGridTopPosition = BitHelper.ReadInt16(bytes, ref index);
			TextGridWidth = BitHelper.ReadInt16(bytes, ref index);
			TextGridHeight = BitHelper.ReadInt16(bytes, ref index);
			CharacterCellWidth = bytes[index++];
			CharacterCellHeight = bytes[index++];
			TextForegroundColorIndex = bytes[index++];
			TextBackgroundColorIndex = bytes[index++];
			PlainTextData = ReadDataSubBlocks(bytes, ref index);

			if (bytes[index++] != BlockTerminatorLabel) throw new Exception("Expected: " + BlockTerminatorLabel);
		}
	}
}