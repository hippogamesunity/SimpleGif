namespace SimpleGif.Enums
{
	/// <summary>
	/// These are selections of colors based in evenly ordered RGB levels which provide complete RGB combinations,
	/// mainly used as master palettes to display any kind of image within the limitations of the 8-bit pixel depth.
	/// More info: https://en.wikipedia.org/wiki/List_of_software_palettes#RGB_arrangements
	/// </summary>
	public enum MasterPalette
	{
		DontApply,
		Levels666,
		Levels676,
		Levels685,
		Levels884,
		Grayscale
	}
}