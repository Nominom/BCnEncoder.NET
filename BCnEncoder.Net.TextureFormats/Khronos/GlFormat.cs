using System;
using System.Collections.Generic;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncoder.TextureFormats
{
	public enum GlFormat : uint
	{
		GlRed = 0x1903,
		GlBgra = 0x80E1,
		GlRgb = 0x1907,
		GlRgba = 0x1908,
		GlRg = 0x8227,
		GlRedInteger = 0x8D94,
		GlRgInteger = 0x8228,
		GlRedSnorm = 0x8F90,
		GlRgSnorm = 0x8F91,
		GlRgbSnorm = 0x8F92,
		GlRgbaSnorm = 0x8F93,
	}

	public enum GlType : uint
	{
		None = 0,
		GlByte = 5120,
		GlUnsignedByte = 5121,
		GlShort = 5122,
		GlUnsignedShort = 5123,
		GlInt = 5124,
		GlUnsignedInt = 5125,
		GlFloat = 5126,
		GlHalfFloat = 5131,
		GlUnsignedByte233Rev = 33634,
		GlUnsignedByte332 = 32818,
		GlUnsignedInt1010102 = 32822,
		GlUnsignedInt2101010Rev = 33640,
		GlUnsignedInt8888 = 32821,
		GlUnsignedInt8888Rev = 33639,
		GlUnsignedShort1555Rev = 33638,
		GlUnsignedShort4444 = 32819,
		GlUnsignedShort4444Rev = 33637,
		GlUnsignedShort5551 = 32820,
		GlUnsignedShort565 = 33635,
		GlUnsignedShort565Rev = 33636,
		GlUnsignedInt10F11F11FRev = 0x8C3B,
		GlUnsignedInt5999Rev = 0x8C3E,
	}

	public enum GlInternalFormat : uint
	{
		GlRgba4 = 0x8056,
		GlRgb5 = 0x8050,
		GlRgb565 = 0x8D62,
		GlRgba8 = 0x8058,
		GlRgb5A1 = 0x8057,
		GlRgba16 = 0x805B,
		GlDepthComponent16 = 0x81A5,
		GlDepthComponent24 = 0x81A6,
		GlDepthComponent32F = 36012,
		GlStencilIndex8 = 36168,
		GlDepth24Stencil8 = 0x88F0,
		GlDepth32FStencil8 = 36013,

		GlR8 = 0x8229,
		GlRg8 = 0x822B,
		GlRg16 = 0x822C,
		GlR16F = 0x822D,
		GlR32F = 0x822E,
		GlRg16F = 0x822F,
		GlRg32F = 0x8230,
		GlRgba32F = 0x8814,
		GlRgba16F = 0x881A,

		GlR8Ui = 33330,
		GlR8I = 33329,
		GlR16 = 33322,
		GlR16I = 33331,
		GlR16Ui = 33332,
		GlR32I = 33333,
		GlR32Ui = 33334,

		GlRg8I = 33335,
		GlRg8Ui = 33336,
		GlRg16I = 33337,
		GlRg16Ui = 33338,
		GlRg32I = 33339,
		GlRg32Ui = 33340,

		GlRgb8 = 32849,
		GlRgb8I = 36239,
		GlRgb8Ui = 36221,

		GlRgba12 = 32858,
		GlRgba2 = 32853,
		GlRgba8I = 36238,
		GlRgba8Ui = 36220,

		GlRgba16I = 36232,
		GlRgba16Ui = 36214,
		GlRgba32I = 36226,
		GlRgba32Ui = 36208,


		GlR8Snorm = 0x8F94,
		GlRg8Snorm = 0x8F95,
		GlRgb8Snorm = 0x8F96,
		GlRgba8Snorm = 0x8F97,
		GlR16Snorm = 0x8F98,
		GlRg16Snorm = 0x8F99,
		GlRgb16Snorm = 0x8F9A,
		GlRgba16Snorm = 0x8F9B,

		GlRgb10A2 = 32857,
		GlRgb10A2Ui = 36975,

		GlRgb16 = 32852,
		GlRgb16F = 34843,
		GlRgb16I = 36233,
		GlRgb16Ui = 36215,

		GlRgb32F = 34837,
		GlRgb32I = 36227,
		GlRgb32Ui = 36209,

		// EXT_texture_sRGB formats
		GlSrgb = 0x8C40,
		GlSrgb8 = 0x8C41,
		GlSrgbAlpha = 0x8C42,
		GlSrgb8Alpha8 = 0x8C43,
		GlSluminanceAlpha = 0x8C44,
		GlSluminance8Alpha8 = 0x8C45,
		GlSluminance = 0x8C46,
		GlSluminance8 = 0x8C47,
		GlCompressedSrgb = 0x8C48,
		GlCompressedSrgbAlpha = 0x8C49,
		GlCompressedSluminance = 0x8C4A,
		GlCompressedSluminanceAlpha = 0x8C4B,

		// EXT_packed_float
		GlR11G11B10F = 0x8C3A,

		// EXT_texture_shared_exponent
		GlRgb9E5 = 0x8C3D,

		//BC1
		GlCompressedRgbS3TcDxt1Ext = 0x83F0,
		GlCompressedSrgbS3TcDxt1Ext = 0x8C4C,
		GlCompressedRgbaS3TcDxt1Ext = 0x83F1,
		GlCompressedSrgbAlphaS3TcDxt1Ext = 0x8C4D,

		//BC2
		GlCompressedRgbaS3TcDxt3Ext = 0x83F2,
		GlCompressedSrgbAlphaS3TcDxt3Ext = 0x8C4E,

		//BC3
		GlCompressedRgbaS3TcDxt5Ext = 0x83F3,
		GlCompressedSrgbAlphaS3TcDxt5Ext = 0x8C4F,

		//BC4 & BC5
		GlCompressedRedGreenRgtc2Ext = 36285,
		GlCompressedRedRgtc1Ext = 36283,
		GlCompressedSignedRedGreenRgtc2Ext = 36286,
		GlCompressedSignedRedRgtc1Ext = 36284,

		//BC6 & BC7
		GlCompressedRgbBptcSignedFloatArb = 36494,
		GlCompressedRgbBptcUnsignedFloatArb = 36495,
		GlCompressedRgbaBptcUnormArb = 36492,
		GlCompressedSrgbAlphaBptcUnormArb = 36493,

		GlCompressedRgbAtc = 0x8C92,
		GlCompressedRgbaAtcExplicitAlpha = 0x8C93,
		GlCompressedRgbaAtcInterpolatedAlpha = 0x87EE,

		// ETC1 & 2
		GlEtc1Rgb8Oes = 0x8D64,

		GlCompressedR11Eac = 0x9270,
		GlCompressedSignedR11Eac = 0x9271,
		GlCompressedRg11Eac = 0x9272,
		GlCompressedSignedRg11Eac = 0x9273,

		GlCompressedRgb8Etc2 = 0x9274,
		GlCompressedSrgb8Etc2 = 0x9275,
		GlCompressedRgb8PunchthroughAlpha1Etc2 = 0x9276,
		GlCompressedSrgb8PunchthroughAlpha1Etc2 = 0x9277,
		GlCompressedRgba8Etc2Eac = 0x9278,
		GlCompressedSrgb8Alpha8Etc2Eac = 0x9279,

		// Apple extension BGRA8
		GlBgra8Extension = 0x93A1,
		GlFormatBgra = 0x80E1,

		// ASTC
		GlCompressedRgbaAstc4X4Khr = 0x93B0,
		GlCompressedRgbaAstc5X4Khr = 0x93B1,
		GlCompressedRgbaAstc5X5Khr = 0x93B2,
		GlCompressedRgbaAstc6X5Khr = 0x93B3,
		GlCompressedRgbaAstc6X6Khr = 0x93B4,
		GlCompressedRgbaAstc8X5Khr = 0x93B5,
		GlCompressedRgbaAstc8X6Khr = 0x93B6,
		GlCompressedRgbaAstc8X8Khr = 0x93B7,
		GlCompressedRgbaAstc10X5Khr = 0x93B8,
		GlCompressedRgbaAstc10X6Khr = 0x93B9,
		GlCompressedRgbaAstc10X8Khr = 0x93BA,
		GlCompressedRgbaAstc10X10Khr = 0x93BB,
		GlCompressedRgbaAstc12X10Khr = 0x93BC,
		GlCompressedRgbaAstc12X12Khr = 0x93BD,

		GlCompressedSrgb8Alpha8Astc4X4Khr = 0x93D0,
		GlCompressedSrgb8Alpha8Astc5X4Khr = 0x93D1,
		GlCompressedSrgb8Alpha8Astc5X5Khr = 0x93D2,
		GlCompressedSrgb8Alpha8Astc6X5Khr = 0x93D3,
		GlCompressedSrgb8Alpha8Astc6X6Khr = 0x93D4,
		GlCompressedSrgb8Alpha8Astc8X5Khr = 0x93D5,
		GlCompressedSrgb8Alpha8Astc8X6Khr = 0x93D6,
		GlCompressedSrgb8Alpha8Astc8X8Khr = 0x93D7,
		GlCompressedSrgb8Alpha8Astc10X5Khr = 0x93D8,
		GlCompressedSrgb8Alpha8Astc10X6Khr = 0x93D9,
		GlCompressedSrgb8Alpha8Astc10X8Khr = 0x93DA,
		GlCompressedSrgb8Alpha8Astc10X10Khr = 0x93DB,
		GlCompressedSrgb8Alpha8Astc12X10Khr = 0x93DC,
		GlCompressedSrgb8Alpha8Astc12X12Khr = 0x93DD
	}

	public static class GLFormatHelpers
	{
		public static (GlFormat, GlInternalFormat, GlType) GetGlFormat(this CompressionFormat format) =>
			FormatMapping.TryGetValue(format, out var glFormat) ? glFormat : (0, 0, 0);

		public static uint GetTypeSize(this GlType type)
		{
			switch (type)
			{
				case GlType.None:
				case GlType.GlByte:
				case GlType.GlUnsignedByte:
				case GlType.GlUnsignedByte233Rev:
				case GlType.GlUnsignedByte332:
					return 1;

				case GlType.GlShort:
				case GlType.GlUnsignedShort:
				case GlType.GlHalfFloat:
				case GlType.GlUnsignedShort1555Rev:
				case GlType.GlUnsignedShort4444:
				case GlType.GlUnsignedShort4444Rev:
				case GlType.GlUnsignedShort5551:
				case GlType.GlUnsignedShort565:
				case GlType.GlUnsignedShort565Rev:
					return 2;

				case GlType.GlInt:
				case GlType.GlUnsignedInt:
				case GlType.GlFloat:
				case GlType.GlUnsignedInt1010102:
				case GlType.GlUnsignedInt2101010Rev:
				case GlType.GlUnsignedInt8888:
				case GlType.GlUnsignedInt8888Rev:
					return 4;

				default:
					return 1; // Default to 1 byte for unknown types
			}
		}

		public static CompressionFormat GetCompressionFormat(this GlInternalFormat internalFormat, GlFormat format = 0)
		{
			// First try with exact match of provided format
			if (format != 0 && FormatMappingReverse.TryGetValue((internalFormat, format), out var bcnFormat))
			{
				return bcnFormat;
			}

			// Fallback: Try any format that matches the internal format
			foreach (var key in FormatMappingReverse.Keys)
			{
				if (key.internalFormat == internalFormat)
				{
					FormatMappingReverse.TryGetValue(key, out bcnFormat);
					return bcnFormat;
				}
			}

			return CompressionFormat.Unknown;
		}

		public static bool IsCompressedFormat(this GlInternalFormat internalFormat)
		{
			switch (internalFormat)
			{
				//BC1
				case GlInternalFormat.GlCompressedRgbS3TcDxt1Ext:
				case GlInternalFormat.GlCompressedSrgbS3TcDxt1Ext:
				case GlInternalFormat.GlCompressedRgbaS3TcDxt1Ext:
				case GlInternalFormat.GlCompressedSrgbAlphaS3TcDxt1Ext:

				//BC2
				case GlInternalFormat.GlCompressedRgbaS3TcDxt3Ext:
				case GlInternalFormat.GlCompressedSrgbAlphaS3TcDxt3Ext:

				//BC3
				case GlInternalFormat.GlCompressedRgbaS3TcDxt5Ext:
				case GlInternalFormat.GlCompressedSrgbAlphaS3TcDxt5Ext:

				//BC4 & BC5
				case GlInternalFormat.GlCompressedRedGreenRgtc2Ext:
				case GlInternalFormat.GlCompressedRedRgtc1Ext:
				case GlInternalFormat.GlCompressedSignedRedGreenRgtc2Ext:
				case GlInternalFormat.GlCompressedSignedRedRgtc1Ext:

				//BC6 & BC7
				case GlInternalFormat.GlCompressedRgbBptcSignedFloatArb:
				case GlInternalFormat.GlCompressedRgbBptcUnsignedFloatArb:
				case GlInternalFormat.GlCompressedRgbaBptcUnormArb:
				case GlInternalFormat.GlCompressedSrgbAlphaBptcUnormArb:

				case GlInternalFormat.GlCompressedRgbAtc:
				case GlInternalFormat.GlCompressedRgbaAtcExplicitAlpha:
				case GlInternalFormat.GlCompressedRgbaAtcInterpolatedAlpha:

				// ETC1 & 2
				case GlInternalFormat.GlEtc1Rgb8Oes:

				case GlInternalFormat.GlCompressedR11Eac:
				case GlInternalFormat.GlCompressedSignedR11Eac:
				case GlInternalFormat.GlCompressedRg11Eac:
				case GlInternalFormat.GlCompressedSignedRg11Eac:
				case GlInternalFormat.GlCompressedRgb8Etc2:
				case GlInternalFormat.GlCompressedSrgb8Etc2:
				case GlInternalFormat.GlCompressedRgb8PunchthroughAlpha1Etc2:
				case GlInternalFormat.GlCompressedSrgb8PunchthroughAlpha1Etc2:
				case GlInternalFormat.GlCompressedRgba8Etc2Eac:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Etc2Eac:

				// ASTC
				case GlInternalFormat.GlCompressedRgbaAstc4X4Khr :
				case GlInternalFormat.GlCompressedRgbaAstc5X4Khr :
				case GlInternalFormat.GlCompressedRgbaAstc5X5Khr :
				case GlInternalFormat.GlCompressedRgbaAstc6X5Khr :
				case GlInternalFormat.GlCompressedRgbaAstc6X6Khr :
				case GlInternalFormat.GlCompressedRgbaAstc8X5Khr :
				case GlInternalFormat.GlCompressedRgbaAstc8X6Khr :
				case GlInternalFormat.GlCompressedRgbaAstc8X8Khr :
				case GlInternalFormat.GlCompressedRgbaAstc10X5Khr:
				case GlInternalFormat.GlCompressedRgbaAstc10X6Khr:
				case GlInternalFormat.GlCompressedRgbaAstc10X8Khr:
				case GlInternalFormat.GlCompressedRgbaAstc10X10Khr:
				case GlInternalFormat.GlCompressedRgbaAstc12X10Khr:
				case GlInternalFormat.GlCompressedRgbaAstc12X12Khr:

				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc4X4Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc5X4Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc5X5Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc6X5Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc6X6Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc8X5Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc8X6Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc8X8Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc10X5Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc10X6Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc10X8Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc10X10Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc12X10Khr:
				case GlInternalFormat.GlCompressedSrgb8Alpha8Astc12X12Khr:
					return true;
				default:
					return false;
			}
		}

		private static Dictionary<CompressionFormat, (GlFormat format, GlInternalFormat internalFormat, GlType type)> FormatMapping { get; } =
			new() {
				// Red-only formats
				{ CompressionFormat.R8, (GlFormat.GlRed, GlInternalFormat.GlR8, GlType.GlUnsignedByte) },
				{ CompressionFormat.R8S, (GlFormat.GlRed, GlInternalFormat.GlR8Snorm, GlType.GlByte) },
				{ CompressionFormat.R16, (GlFormat.GlRed, GlInternalFormat.GlR16, GlType.GlUnsignedShort) },
				{ CompressionFormat.R16S, (GlFormat.GlRed, GlInternalFormat.GlR16Snorm, GlType.GlShort) },
				{ CompressionFormat.R16F, (GlFormat.GlRed, GlInternalFormat.GlR16F, GlType.GlHalfFloat) },
				{ CompressionFormat.R32F, (GlFormat.GlRed, GlInternalFormat.GlR32F, GlType.GlFloat) },

				// Red-green formats
				{ CompressionFormat.R8G8, (GlFormat.GlRg, GlInternalFormat.GlRg8, GlType.GlUnsignedByte) },
				{ CompressionFormat.R8G8S, (GlFormat.GlRg, GlInternalFormat.GlRg8Snorm, GlType.GlByte) },
				{ CompressionFormat.R16G16, (GlFormat.GlRg, GlInternalFormat.GlRg16, GlType.GlUnsignedShort) },
				{ CompressionFormat.R16G16S, (GlFormat.GlRg, GlInternalFormat.GlRg16Snorm, GlType.GlShort) },
				{ CompressionFormat.R16G16F, (GlFormat.GlRg, GlInternalFormat.GlRg16F, GlType.GlHalfFloat) },
				{ CompressionFormat.R32G32F, (GlFormat.GlRg, GlInternalFormat.GlRg32F, GlType.GlFloat) },

				// RGB formats
				{ CompressionFormat.Rgb24, (GlFormat.GlRgb, GlInternalFormat.GlRgb8, GlType.GlUnsignedByte) },
				{ CompressionFormat.Rgb24_sRGB, (GlFormat.GlRgb, GlInternalFormat.GlSrgb8, GlType.GlUnsignedByte) },
				{ CompressionFormat.RgbFloat, (GlFormat.GlRgb, GlInternalFormat.GlRgb32F, GlType.GlFloat) },
				{ CompressionFormat.RgbHalf, (GlFormat.GlRgb, GlInternalFormat.GlRgb16F, GlType.GlHalfFloat) },
				{ CompressionFormat.R11G11B10UF_Packed, (GlFormat.GlRgb, GlInternalFormat.GlR11G11B10F, GlType.GlUnsignedInt10F11F11FRev) },
				{ CompressionFormat.R9G9B9E5_Packed, (GlFormat.GlRgb, GlInternalFormat.GlRgb9E5, GlType.GlUnsignedInt5999Rev) },
				{ CompressionFormat.R5G6B5_Packed, (GlFormat.GlRgb, GlInternalFormat.GlRgb565, GlType.GlUnsignedShort565Rev) },

				// RGBA formats
				{ CompressionFormat.R10G10B10A2_Packed, (GlFormat.GlRgba, GlInternalFormat.GlRgb10A2, GlType.GlUnsignedInt2101010Rev) },
				{ CompressionFormat.Rgba32, (GlFormat.GlRgba, GlInternalFormat.GlRgba8, GlType.GlUnsignedByte) },
				{ CompressionFormat.Rgba32_sRGB, (GlFormat.GlRgba, GlInternalFormat.GlSrgb8Alpha8, GlType.GlUnsignedByte) },
				{ CompressionFormat.RgbaFloat, (GlFormat.GlRgba, GlInternalFormat.GlRgba32F, GlType.GlFloat) },
				{ CompressionFormat.RgbaHalf, (GlFormat.GlRgba, GlInternalFormat.GlRgba16F, GlType.GlHalfFloat) },
				{ CompressionFormat.R5G5B5A1_Packed, (GlFormat.GlRgba, GlInternalFormat.GlRgb5A1, GlType.GlUnsignedShort1555Rev) },
				{ CompressionFormat.R4G4B4A4_Packed, (GlFormat.GlRgba, GlInternalFormat.GlRgba4, GlType.GlUnsignedShort4444Rev) },

				// BGRA formats
				{ CompressionFormat.Bgra32, (GlFormat.GlBgra, GlInternalFormat.GlRgba8, GlType.GlUnsignedByte) },
				{ CompressionFormat.Bgra32_sRGB, (GlFormat.GlBgra, GlInternalFormat.GlSrgb8Alpha8, GlType.GlUnsignedByte) },

				// Special formats (Rgbe and Xyze don't have direct OpenGL equivalents)
				{ CompressionFormat.Rgbe32, (GlFormat.GlRgba, GlInternalFormat.GlRgba8, GlType.GlUnsignedByte) }, // Best approximation
				{ CompressionFormat.Xyze32, (GlFormat.GlRgba, GlInternalFormat.GlRgba8, GlType.GlUnsignedByte) }, // Best approximation

				// BC formats
				{ CompressionFormat.Bc1, (GlFormat.GlRgb, GlInternalFormat.GlCompressedRgbS3TcDxt1Ext, GlType.None) },
				{ CompressionFormat.Bc1_sRGB, (GlFormat.GlRgb, GlInternalFormat.GlCompressedSrgbS3TcDxt1Ext, GlType.None) },
				{ CompressionFormat.Bc1WithAlpha, (GlFormat.GlRgba, GlInternalFormat.GlCompressedRgbaS3TcDxt1Ext, GlType.None) },
				{ CompressionFormat.Bc1WithAlpha_sRGB, (GlFormat.GlRgba, GlInternalFormat.GlCompressedSrgbAlphaS3TcDxt1Ext, GlType.None) },
				{ CompressionFormat.Bc2, (GlFormat.GlRgba, GlInternalFormat.GlCompressedRgbaS3TcDxt3Ext, GlType.None) },
				{ CompressionFormat.Bc2_sRGB, (GlFormat.GlRgba, GlInternalFormat.GlCompressedSrgbAlphaS3TcDxt3Ext, GlType.None) },
				{ CompressionFormat.Bc3, (GlFormat.GlRgba, GlInternalFormat.GlCompressedRgbaS3TcDxt5Ext, GlType.None) },
				{ CompressionFormat.Bc3_sRGB, (GlFormat.GlRgba, GlInternalFormat.GlCompressedSrgbAlphaS3TcDxt5Ext, GlType.None) },
				{ CompressionFormat.Bc4, (GlFormat.GlRed, GlInternalFormat.GlCompressedRedRgtc1Ext, GlType.None) },
				{ CompressionFormat.Bc4S, (GlFormat.GlRed, GlInternalFormat.GlCompressedSignedRedRgtc1Ext, GlType.None) },
				{ CompressionFormat.Bc5, (GlFormat.GlRg, GlInternalFormat.GlCompressedRedGreenRgtc2Ext, GlType.None) },
				{ CompressionFormat.Bc5S, (GlFormat.GlRg, GlInternalFormat.GlCompressedSignedRedGreenRgtc2Ext, GlType.None) },
				{ CompressionFormat.Bc6U, (GlFormat.GlRgb, GlInternalFormat.GlCompressedRgbBptcUnsignedFloatArb, GlType.None) },
				{ CompressionFormat.Bc6S, (GlFormat.GlRgb, GlInternalFormat.GlCompressedRgbBptcSignedFloatArb, GlType.None) },
				{ CompressionFormat.Bc7, (GlFormat.GlRgba, GlInternalFormat.GlCompressedRgbaBptcUnormArb, GlType.None) },
				{ CompressionFormat.Bc7_sRGB, (GlFormat.GlRgba, GlInternalFormat.GlCompressedSrgbAlphaBptcUnormArb, GlType.None) },

				// ATC formats
				{ CompressionFormat.Atc, (GlFormat.GlRgb, GlInternalFormat.GlCompressedRgbAtc, GlType.None) },
				{ CompressionFormat.AtcExplicitAlpha, (GlFormat.GlRgba, GlInternalFormat.GlCompressedRgbaAtcExplicitAlpha, GlType.None) },
				{ CompressionFormat.AtcInterpolatedAlpha, (GlFormat.GlRgba, GlInternalFormat.GlCompressedRgbaAtcInterpolatedAlpha, GlType.None) }
			};

		private static Dictionary<(GlInternalFormat internalFormat, GlFormat format), CompressionFormat> FormatMappingReverse { get; } =
			new() {
				// Red-only formats
				{ (GlInternalFormat.GlR8, GlFormat.GlRed), CompressionFormat.R8 },
				{ (GlInternalFormat.GlR8Ui, GlFormat.GlRed), CompressionFormat.R8 },
				{ (GlInternalFormat.GlR8I, GlFormat.GlRed), CompressionFormat.R8S },
				{ (GlInternalFormat.GlR8Snorm, GlFormat.GlRed), CompressionFormat.R8S },
				{ (GlInternalFormat.GlR16, GlFormat.GlRed), CompressionFormat.R16 },
				{ (GlInternalFormat.GlR16Ui, GlFormat.GlRed), CompressionFormat.R16 },
				{ (GlInternalFormat.GlR16I, GlFormat.GlRed), CompressionFormat.R16S },
				{ (GlInternalFormat.GlR16Snorm, GlFormat.GlRed), CompressionFormat.R16S },
				{ (GlInternalFormat.GlR16F, GlFormat.GlRed), CompressionFormat.R16F },
				{ (GlInternalFormat.GlR32F, GlFormat.GlRed), CompressionFormat.R32F },

				// Red-green formats
				{ (GlInternalFormat.GlRg8, GlFormat.GlRg), CompressionFormat.R8G8 },
				{ (GlInternalFormat.GlRg8Ui, GlFormat.GlRg), CompressionFormat.R8G8 },
				{ (GlInternalFormat.GlRg8I, GlFormat.GlRg), CompressionFormat.R8G8S },
				{ (GlInternalFormat.GlRg8Snorm, GlFormat.GlRg), CompressionFormat.R8G8S },
				{ (GlInternalFormat.GlRg16, GlFormat.GlRg), CompressionFormat.R16G16 },
				{ (GlInternalFormat.GlRg16Ui, GlFormat.GlRg), CompressionFormat.R16G16 },
				{ (GlInternalFormat.GlRg16I, GlFormat.GlRg), CompressionFormat.R16G16S },
				{ (GlInternalFormat.GlRg16Snorm, GlFormat.GlRg), CompressionFormat.R16G16S },
				{ (GlInternalFormat.GlRg16F, GlFormat.GlRg), CompressionFormat.R16G16F },
				{ (GlInternalFormat.GlRg32F, GlFormat.GlRg), CompressionFormat.R32G32F },

				// RGB formats
				{ (GlInternalFormat.GlRgb8, GlFormat.GlRgb), CompressionFormat.Rgb24 },
				{ (GlInternalFormat.GlRgb8I, GlFormat.GlRgb), CompressionFormat.Rgb24 },
				{ (GlInternalFormat.GlRgb8Ui, GlFormat.GlRgb), CompressionFormat.Rgb24 },
				{ (GlInternalFormat.GlRgb8Snorm, GlFormat.GlRgb), CompressionFormat.Rgb24 },
				{ (GlInternalFormat.GlSrgb8, GlFormat.GlRgb), CompressionFormat.Rgb24_sRGB },
				{ (GlInternalFormat.GlSrgb, GlFormat.GlRgb), CompressionFormat.Rgb24_sRGB },
				{ (GlInternalFormat.GlRgb32F, GlFormat.GlRgb), CompressionFormat.RgbFloat },
				{ (GlInternalFormat.GlRgb16F, GlFormat.GlRgb), CompressionFormat.RgbHalf },
				{ (GlInternalFormat.GlR11G11B10F, GlFormat.GlRgb), CompressionFormat.R11G11B10UF_Packed },
				{ (GlInternalFormat.GlRgb9E5, GlFormat.GlRgb), CompressionFormat.R9G9B9E5_Packed },
				{ (GlInternalFormat.GlRgb565, GlFormat.GlRgb), CompressionFormat.R5G6B5_Packed },

				// RGBA formats
				{ (GlInternalFormat.GlRgb10A2, GlFormat.GlRgba), CompressionFormat.R10G10B10A2_Packed },
				{ (GlInternalFormat.GlRgb10A2Ui, GlFormat.GlRgba), CompressionFormat.R10G10B10A2_Packed },
				{ (GlInternalFormat.GlRgba8, GlFormat.GlRgba), CompressionFormat.Rgba32 },
				{ (GlInternalFormat.GlRgba8I, GlFormat.GlRgba), CompressionFormat.Rgba32 },
				{ (GlInternalFormat.GlRgba8Ui, GlFormat.GlRgba), CompressionFormat.Rgba32 },
				{ (GlInternalFormat.GlRgba8Snorm, GlFormat.GlRgba), CompressionFormat.Rgba32 },
				{ (GlInternalFormat.GlSrgb8Alpha8, GlFormat.GlRgba), CompressionFormat.Rgba32_sRGB },
				{ (GlInternalFormat.GlSrgbAlpha, GlFormat.GlRgba), CompressionFormat.Rgba32_sRGB },
				{ (GlInternalFormat.GlRgba32F, GlFormat.GlRgba), CompressionFormat.RgbaFloat },
				{ (GlInternalFormat.GlRgba16F, GlFormat.GlRgba), CompressionFormat.RgbaHalf },
				{ (GlInternalFormat.GlRgb5A1, GlFormat.GlRgba), CompressionFormat.R5G5B5A1_Packed },
				{ (GlInternalFormat.GlRgba4, GlFormat.GlRgba), CompressionFormat.R4G4B4A4_Packed },

				// BGRA formats
				{ (GlInternalFormat.GlBgra8Extension, GlFormat.GlBgra), CompressionFormat.Bgra32 },
				{ (GlInternalFormat.GlFormatBgra, GlFormat.GlBgra), CompressionFormat.Bgra32 },
				{ (GlInternalFormat.GlRgba8, GlFormat.GlBgra), CompressionFormat.Bgra32 },
				{ (GlInternalFormat.GlSrgb8Alpha8, GlFormat.GlBgra), CompressionFormat.Bgra32_sRGB },

				// BC formats
				{ (GlInternalFormat.GlCompressedRgbS3TcDxt1Ext, GlFormat.GlRgb), CompressionFormat.Bc1 },
				{ (GlInternalFormat.GlCompressedSrgbS3TcDxt1Ext, GlFormat.GlRgb), CompressionFormat.Bc1_sRGB },
				{ (GlInternalFormat.GlCompressedRgbaS3TcDxt1Ext, GlFormat.GlRgba), CompressionFormat.Bc1WithAlpha },
				{ (GlInternalFormat.GlCompressedSrgbAlphaS3TcDxt1Ext, GlFormat.GlRgba), CompressionFormat.Bc1WithAlpha_sRGB },
				{ (GlInternalFormat.GlCompressedRgbaS3TcDxt3Ext, GlFormat.GlRgba), CompressionFormat.Bc2 },
				{ (GlInternalFormat.GlCompressedSrgbAlphaS3TcDxt3Ext, GlFormat.GlRgba), CompressionFormat.Bc2_sRGB },
				{ (GlInternalFormat.GlCompressedRgbaS3TcDxt5Ext, GlFormat.GlRgba), CompressionFormat.Bc3 },
				{ (GlInternalFormat.GlCompressedSrgbAlphaS3TcDxt5Ext, GlFormat.GlRgba), CompressionFormat.Bc3_sRGB },
				{ (GlInternalFormat.GlCompressedRedRgtc1Ext, GlFormat.GlRed), CompressionFormat.Bc4 },
				{ (GlInternalFormat.GlCompressedSignedRedRgtc1Ext, GlFormat.GlRed), CompressionFormat.Bc4S },
				{ (GlInternalFormat.GlCompressedRedGreenRgtc2Ext, GlFormat.GlRg), CompressionFormat.Bc5 },
				{ (GlInternalFormat.GlCompressedSignedRedGreenRgtc2Ext, GlFormat.GlRg), CompressionFormat.Bc5S },
				{ (GlInternalFormat.GlCompressedRgbBptcUnsignedFloatArb, GlFormat.GlRgb), CompressionFormat.Bc6U },
				{ (GlInternalFormat.GlCompressedRgbBptcSignedFloatArb, GlFormat.GlRgb), CompressionFormat.Bc6S },
				{ (GlInternalFormat.GlCompressedRgbaBptcUnormArb, GlFormat.GlRgba), CompressionFormat.Bc7 },
				{ (GlInternalFormat.GlCompressedSrgbAlphaBptcUnormArb, GlFormat.GlRgba), CompressionFormat.Bc7_sRGB },

				// ATC formats
				{ (GlInternalFormat.GlCompressedRgbAtc, GlFormat.GlRgb), CompressionFormat.Atc },
				{ (GlInternalFormat.GlCompressedRgbaAtcExplicitAlpha, GlFormat.GlRgba), CompressionFormat.AtcExplicitAlpha },
				{ (GlInternalFormat.GlCompressedRgbaAtcInterpolatedAlpha, GlFormat.GlRgba), CompressionFormat.AtcInterpolatedAlpha }
			};
	}
}
