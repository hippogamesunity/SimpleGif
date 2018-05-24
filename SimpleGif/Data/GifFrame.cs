using SimpleGif.Enums;

namespace SimpleGif.Data
{
	/// <summary>
	/// Texture + delay + disposal method
	/// </summary>
	public class GifFrame
	{
		public Texture2D Texture;
		public float Delay;
		public DisposalMethod DisposalMethod = DisposalMethod.RestoreToBackgroundColor;
	}
}