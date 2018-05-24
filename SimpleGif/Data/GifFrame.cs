using SimpleGif.Enums;

namespace SimpleGif.Data
{
	/// <summary>
	/// Texture + delay
	/// </summary>
	public class GifFrame
	{
		public Texture2D Texture;
		public float Delay;
		public DisposalMethod DisposalMethod = DisposalMethod.RestoreToBackgroundColor;
	}
}