using System;
using System.Collections.Generic;

namespace SimpleGif.GifCore.Blocks
{
	internal class GraphicControlExtension : Block
	{
		public byte BlockSize;
		public byte Reserved;
		public byte DisposalMethod;
		public byte UserInputFlag;
		public byte TransparentColorFlag;
		public ushort DelayTime;
		public byte TransparentColorIndex;

		public GraphicControlExtension(byte[] bytes, ref int index)
		{
			if (bytes[index++] != ExtensionIntroducer) throw new Exception("Expected: " + ExtensionIntroducer);
			if (bytes[index++] != GraphicControlExtensionLabel) throw new Exception("Expected: " + GraphicControlExtensionLabel);
			
			BlockSize = bytes[index++];

			Reserved = BitHelper.ReadPackedByte(bytes[index], 0, 3);
			DisposalMethod = BitHelper.ReadPackedByte(bytes[index], 3, 3);
			UserInputFlag = BitHelper.ReadPackedByte(bytes[index], 6, 1);
			TransparentColorFlag = BitHelper.ReadPackedByte(bytes[index++], 7, 1);

			DelayTime = BitHelper.ReadInt16(bytes, ref index);
			TransparentColorIndex = bytes[index++];

			if (bytes[index++] != BlockTerminatorLabel) throw new Exception("Expected: " + BlockTerminatorLabel);
		}

		public GraphicControlExtension(byte blockSize, byte reserved, byte disposalMethod, byte userInputFlag, byte transparentColorFlag, ushort delayTime, byte transparentColorIndex)
		{
			BlockSize = blockSize;
			Reserved = reserved;
			DisposalMethod = disposalMethod;
			UserInputFlag = userInputFlag;
			TransparentColorFlag = transparentColorFlag;
			DelayTime = delayTime;
			TransparentColorIndex = transparentColorIndex;
		}

		public List<byte> GetBytes()
		{
			var bytes = new List<byte> { ExtensionIntroducer, GraphicControlExtensionLabel, BlockSize };
			var packedByte = BitHelper.PackByte(
				BitHelper.ReadByte(Reserved, 2),
				BitHelper.ReadByte(Reserved, 1),
				BitHelper.ReadByte(Reserved, 0),
				BitHelper.ReadByte(DisposalMethod, 2),
				BitHelper.ReadByte(DisposalMethod, 1),
				BitHelper.ReadByte(DisposalMethod, 0),
				UserInputFlag == 1,
				TransparentColorFlag == 1);

			bytes.Add(packedByte);
			bytes.AddRange(BitConverter.GetBytes(DelayTime));
			bytes.Add(TransparentColorIndex);
			bytes.Add(BlockTerminatorLabel);

			return bytes;
		}
	}
}