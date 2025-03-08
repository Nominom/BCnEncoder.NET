using System;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Shared
{

	// ReSharper disable InconsistentNaming
	public enum CompressionFormat
	{
		/// <summary>
		/// Raw unsigned byte 8-bit Luminance data. Pixel format <see cref="ColorR8"/>.
		/// </summary>
		R8,

		/// <summary>
		/// Raw unsigned byte 16-bit RG data. Pixel format <see cref="ColorR8G8"/>.
		/// </summary>
		R8G8,

		/// <summary>
		/// Raw unsigned byte 24-bit RGB data in linear colorspace. Pixel format <see cref="ColorBgr24"/>.
		/// </summary>
		Rgb24,

		/// <summary>
		/// Raw unsigned byte 24-bit RGB data in the sRGB colorspace. Pixel format <see cref="ColorBgr24"/>.
		/// </summary>
		Rgb24_sRGB,

		/// <summary>
		/// Raw unsigned byte 24-bit BGR data in linear colorspace. Pixel format <see cref="ColorBgr24"/>.
		/// Most texture formats do not support this format. Use <see cref="Bgra32"/> instead.
		/// </summary>
		Bgr24,

		/// <summary>
		/// Raw unsigned byte 24-bit BGR data in the sRGB colorspace. Pixel format <see cref="ColorBgr24"/>.
		/// Most texture formats do not support this format. Use <see cref="Bgra32"/> instead.
		/// </summary>
		Bgr24_sRGB,

		/// <summary>
		/// Raw unsigned byte 32-bit RGBA data in linear colorspace. Pixel format <see cref="ColorRgba32"/>.
		/// </summary>
		Rgba32,

		/// <summary>
		/// Raw unsigned byte 32-bit RGBA data in the sRGB colorspace. Pixel format <see cref="ColorRgba32"/>.
		/// </summary>
		Rgba32_sRGB,

		/// <summary>
		/// Raw unsigned byte 32-bit BGRA data in linear colorspace. Pixel format <see cref="ColorBgra32"/>.
		/// </summary>
		Bgra32,

		/// <summary>
		/// Raw unsigned byte 32-bit BGRA data in the sRGB colorspace. Pixel format <see cref="ColorBgra32"/>.
		/// </summary>
		Bgra32_sRGB,

		/// <summary>
		/// Raw floating point 32-bit-per-channel RGBA data in linear colorspace. Pixel format <see cref="ColorRgbaFloat"/>.
		/// </summary>
		RgbaFloat,

		/// <summary>
		/// Raw floating point 16-bit-per-channel RGBA data in linear colorspace. Pixel format <see cref="ColorRgbaHalf"/>.
		/// </summary>
		RgbaHalf,

		/// <summary>
		/// Raw floating point 32-bit-per-channel RGB data in linear colorspace. Pixel format <see cref="ColorRgbFloat"/>.
		/// </summary>
		RgbFloat,

		/// <summary>
		/// Raw floating point 16-bit-per-channel RGB data in linear colorspace. Pixel format <see cref="ColorRgbHalf"/>.
		/// </summary>
		RgbHalf,

		/// <summary>
		/// 32-bit-per-pixel Hdr data with 24-bits for RGB (in linear colorspace) and 8 bits for exponent. Pixel format <see cref="ColorRgbe"/>.
		/// </summary>
		Rgbe,

		/// <summary>
		/// 32-bit-per-pixel Hdr data with 24-bits for RGB (in CIE_1931 XYZ colorspace) and 8 bits for exponent. Pixel format <see cref="ColorXyze"/>.
		/// </summary>
		Xyze,

		/// <summary>
		/// BC1 / DXT1 with no alpha in linear colorspace. Very widely supported and good compression ratio.
		/// </summary>
		Bc1,

		/// <summary>
		/// BC1 / DXT1 with no alpha in the sRGB colorspace.
		/// </summary>
		Bc1_sRGB,

		/// <summary>
		/// BC1 / DXT1 with 1-bit of alpha in linear colorspace.
		/// </summary>
		Bc1WithAlpha,

		/// <summary>
		/// BC1 / DXT1 with 1-bit of alpha in the sRGB colorspace.
		/// </summary>
		Bc1WithAlpha_sRGB,

		/// <summary>
		/// BC2 / DXT3 encoding with alpha in linear colorspace. Good for sharp alpha transitions.
		/// </summary>
		Bc2,

		/// <summary>
		/// BC2 / DXT3 encoding with alpha in sRGB colorspace. Alpha channel is always linear.
		/// </summary>
		Bc2_sRGB,

		/// <summary>
		/// BC3 / DXT5 encoding with alpha in linear colorspace. Good for smooth alpha transitions.
		/// </summary>
		Bc3,

		/// <summary>
		/// BC3 / DXT5 encoding with alpha in the sRGB colorspace. Alpha channel is always linear.
		/// </summary>
		Bc3_sRGB,

		/// <summary>
		/// BC4 single-channel encoding. Only luminance (or red channel) is encoded. Always linear.
		/// </summary>
		Bc4,

		/// <summary>
		/// BC5 dual-channel encoding. Only red and green channels are encoded. Mostly used for normal maps. Always linear.
		/// </summary>
		Bc5,

		/// <summary>
		/// BC6H / BPTC unsigned float encoding in linear colorspace. Can compress HDR textures without alpha. Does not support negative values.
		/// </summary>
		Bc6U,

		/// <summary>
		/// BC6H / BPTC signed float encoding in linear colorspace. Can compress HDR textures without alpha. Supports negative values.
		/// </summary>
		Bc6S,

		/// <summary>
		/// BC7 / BPTC unorm encoding in linear colorspace. Very high Quality rgba or rgb encoding. Also very slow.
		/// </summary>
		Bc7,

		/// <summary>
		/// BC7 / BPTC unorm encoding in the sRGB colorspace. Very high Quality rgba or rgb encoding. Also very slow.
		/// </summary>
		Bc7_sRGB,

		/// <summary>
		/// ATC / Adreno Texture Compression encoding in linear colorspace. Derivative of BC1.
		/// </summary>
		Atc,

		/// <summary>
		/// ATC / Adreno Texture Compression encoding in linear colorspace. Derivative of BC2. Good for sharp alpha transitions.
		/// </summary>
		AtcExplicitAlpha,

		/// <summary>
		/// ATC / Adreno Texture Compression encoding in linear colorspace. Derivative of BC3. Good for smooth alpha transitions.
		/// </summary>
		AtcInterpolatedAlpha,

		/// <summary>
		/// Unknown format
		/// </summary>
		Unknown
	}
	// ReSharper restore InconsistentNaming

	public static class CompressionFormatExtensions
	{
		public static bool IsBlockCompressedFormat(this CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R8:
				case CompressionFormat.R8G8:
				case CompressionFormat.Rgb24:
				case CompressionFormat.Rgb24_sRGB:
				case CompressionFormat.Bgr24:
				case CompressionFormat.Bgr24_sRGB:
				case CompressionFormat.Rgba32:
				case CompressionFormat.Rgba32_sRGB:
				case CompressionFormat.Bgra32:
				case CompressionFormat.Bgra32_sRGB:
				case CompressionFormat.RgbaFloat:
				case CompressionFormat.RgbaHalf:
				case CompressionFormat.RgbFloat:
				case CompressionFormat.RgbHalf:
				case CompressionFormat.Rgbe:
				case CompressionFormat.Xyze:
					return false;

				default:
					return true;
			}
		}

		/// <summary>
		/// Return bytes per block (or per pixel if not block compressed format)
		/// </summary>
		public static int BytesPerBlock(this CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R8:
					return 1;
				case CompressionFormat.R8G8:
					return 2;
				case CompressionFormat.Rgb24:
				case CompressionFormat.Rgb24_sRGB:
					return 3;
				case CompressionFormat.Bgr24:
				case CompressionFormat.Bgr24_sRGB:
					return 3;
				case CompressionFormat.Rgba32:
				case CompressionFormat.Rgba32_sRGB:
					return 4;
				case CompressionFormat.Bgra32:
				case CompressionFormat.Bgra32_sRGB:
					return 4;
				case CompressionFormat.RgbaFloat:
					return 4 * 4;
				case CompressionFormat.RgbaHalf:
					return 4 * 2;
				case CompressionFormat.RgbFloat:
					return 3 * 4;
				case CompressionFormat.RgbHalf:
					return 3 * 2;
				case CompressionFormat.Rgbe:
					return 4;
				case CompressionFormat.Xyze:
					return 4;
				// Block compressed
				case CompressionFormat.Bc1:
				case CompressionFormat.Bc1_sRGB:
					return 8;
				case CompressionFormat.Bc1WithAlpha:
				case CompressionFormat.Bc1WithAlpha_sRGB:
					return 8;
				case CompressionFormat.Bc2:
				case CompressionFormat.Bc2_sRGB:
					return 16;
				case CompressionFormat.Bc3:
				case CompressionFormat.Bc3_sRGB:
					return 16;
				case CompressionFormat.Bc4:
					return 8;
				case CompressionFormat.Bc5:
					return 16;
				case CompressionFormat.Bc6U:
					return 16;
				case CompressionFormat.Bc6S:
					return 16;
				case CompressionFormat.Bc7:
				case CompressionFormat.Bc7_sRGB:
					return 16;
				case CompressionFormat.Atc:
					return 8;
				case CompressionFormat.AtcExplicitAlpha:
					return 16;
				case CompressionFormat.AtcInterpolatedAlpha:
					return 16;
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		public static bool IsHdrFormat(this CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.Bc6S:
				case CompressionFormat.Bc6U:
				case CompressionFormat.RgbaFloat:
				case CompressionFormat.RgbaHalf:
				case CompressionFormat.RgbFloat:
				case CompressionFormat.RgbHalf:
				case CompressionFormat.Rgbe:
				case CompressionFormat.Xyze:
					return true;

				default:
					return false;
			}
		}

		public static bool SupportsAlpha(this CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.Rgba32:
				case CompressionFormat.Rgba32_sRGB:
				case CompressionFormat.Bgra32:
				case CompressionFormat.Bgra32_sRGB:
				case CompressionFormat.RgbaFloat:
				case CompressionFormat.RgbaHalf:
				case CompressionFormat.Bc1WithAlpha:
				case CompressionFormat.Bc1WithAlpha_sRGB:
				case CompressionFormat.Bc2:
				case CompressionFormat.Bc2_sRGB:
				case CompressionFormat.Bc3:
				case CompressionFormat.Bc3_sRGB:
				case CompressionFormat.Bc7:
				case CompressionFormat.Bc7_sRGB:
				case CompressionFormat.AtcExplicitAlpha:
				case CompressionFormat.AtcInterpolatedAlpha:
					return true;
				default:
					return false;
			}
		}

		public static bool IsSRGBFormat(this CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.Rgb24_sRGB:
				case CompressionFormat.Bgr24_sRGB:
				case CompressionFormat.Rgba32_sRGB:
				case CompressionFormat.Bgra32_sRGB:
				case CompressionFormat.Bc1_sRGB:
				case CompressionFormat.Bc1WithAlpha_sRGB:
				case CompressionFormat.Bc2_sRGB:
				case CompressionFormat.Bc3_sRGB:
				case CompressionFormat.Bc7_sRGB:
					return true;
			}

			return false;
		}

		public static long CalculateMipByteSize(this CompressionFormat format, int width, int height)
		{
			if (format.IsBlockCompressedFormat())
			{
				return ImageToBlocks.CalculateNumOfBlocks(width, height) * (long)format.BytesPerBlock();
			}

			return (long)format.BytesPerBlock() * width * height;
		}

		internal static System.Type GetPixelType(this CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R8:
					return typeof(ColorR8);
				case CompressionFormat.R8G8:
					return typeof(ColorR8G8);
				case CompressionFormat.Rgb24:
				case CompressionFormat.Rgb24_sRGB:
					return typeof(ColorRgb24);
				case CompressionFormat.Bgr24:
				case CompressionFormat.Bgr24_sRGB:
					return typeof(ColorBgr24);
				case CompressionFormat.Rgba32:
				case CompressionFormat.Rgba32_sRGB:
					return typeof(ColorRgba32);
				case CompressionFormat.Bgra32:
				case CompressionFormat.Bgra32_sRGB:
					return typeof(ColorBgra32);
				case CompressionFormat.RgbaFloat:
					return typeof(ColorRgbaFloat);
				case CompressionFormat.RgbaHalf:
					return typeof(ColorRgbaHalf);
				case CompressionFormat.RgbFloat:
					return typeof(ColorRgbFloat);
				case CompressionFormat.RgbHalf:
					return typeof(ColorRgbHalf);
				case CompressionFormat.Rgbe:
					return typeof(ColorRgbe);
				case CompressionFormat.Xyze:
					return typeof(ColorXyze);
				default:
					throw new ArgumentOutOfRangeException(nameof(format));
			}
		}
	}
}
