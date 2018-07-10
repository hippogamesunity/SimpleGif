using System;

namespace SimpleGif.Data
{
	public class DecodeProgress
	{
		public int Progress;
		public int FrameCount;
		public bool Completed;
		public Gif Gif;
		public Exception Exception;
	}
}