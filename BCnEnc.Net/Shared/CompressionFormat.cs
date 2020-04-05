namespace BCnComp.Net.Shared
{
	public enum CompressionFormat
	{
		/// <summary>
		/// Raw unsigned byte 8-bit Luminance data
		/// </summary>
		R,
		/// <summary>
		/// Raw unsigned byte 16-bit RG data
		/// </summary>
		RG,
		/// <summary>
		/// Raw unsigned byte 24-bit RGB data
		/// </summary>
		RGB,
		/// <summary>
		/// Raw unsigned byte 32-bit RGBA data
		/// </summary>
		RGBA,
		/// <summary>
		/// BC1 / DXT1 with no alpha
		/// </summary>
		BC1,
		/// <summary>
		/// BC1 / DXT1 with 1-bit of alpha
		/// </summary>
		BC1WithAlpha,
		BC2,
		BC3,
		BC4,
		BC5,
		BC6,
		BC7
	}
}
