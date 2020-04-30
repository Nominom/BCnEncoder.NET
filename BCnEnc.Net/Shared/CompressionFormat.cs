namespace BCnEncoder.Shared
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
		/// BC1 / DXT1 with no alpha. Very widely supported and good compression ratio.
		/// </summary>
		BC1,
		/// <summary>
		/// BC1 / DXT1 with 1-bit of alpha.
		/// </summary>
		BC1WithAlpha,
		/// <summary>
		/// BC2 / DXT3 encoding with alpha. Good for sharp alpha transitions.
		/// </summary>
		BC2,
		/// <summary>
		/// BC3 / DXT5 encoding with alpha. Good for smooth alpha transitions.
		/// </summary>
		BC3,
		/// <summary>
		/// BC4 single-channel encoding. Only luminance is encoded.
		/// </summary>
		BC4,
		/// <summary>
		/// BC5 dual-channel encoding. Only red and green channels are encoded.
		/// </summary>
		BC5,
		/// <summary>
		/// BC6H / BPTC float encoding. Can compress HDR textures without alpha. Currently not supported.
		/// </summary>
		BC6,
		/// <summary>
		/// BC7 / BPTC unorm encoding. Very high quality rgba or rgb encoding. Also very slow.
		/// </summary>
		BC7
	}

	public static class CompressionFormatExtensions {
		public static bool IsCompressedFormat(this CompressionFormat format)
		{
			switch (format) {
				case CompressionFormat.R:
				case CompressionFormat.RG:
				case CompressionFormat.RGB:
				case CompressionFormat.RGBA:
					return false;
				
				default:
					return true;
			}
		}
	}
}
