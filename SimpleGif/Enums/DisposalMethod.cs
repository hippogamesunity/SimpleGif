namespace SimpleGif.Enums
{
	/// <summary>
	/// Indicates the way in which the graphic is to be treated after being displayed.
	/// More info: https://www.w3.org/Graphics/GIF/spec-gif89a.txt
	/// </summary>
	public enum DisposalMethod
	{
		/// <summary>
		/// The decoder is not required to take any action.
		/// </summary>
		NoDisposalSpecified = 0,

		/// <summary>
		/// The graphic is to be left in place.
		/// </summary>
		DoNotDispose = 1,

		/// <summary>
		/// The area used by the graphic must be restored to the background color.
		/// </summary>
		RestoreToBackgroundColor = 2,

		/// <summary>
		/// The decoder is required to restore the area overwritten by the graphic with what was there prior to rendering the graphic.
		/// </summary>
		RestoreToPrevious = 3
	}
}