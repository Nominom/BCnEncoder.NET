using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Shared
{
	// ReSharper disable InconsistentNaming
	public enum CompressionFormat
	{
		/// <summary>
		/// Raw unsigned byte 8-bit Luminance (or red) data. Pixel format <see cref="ColorR8"/>.
		/// </summary>
		R8,

		/// <summary>
		/// Raw signed byte 8-bit Luminance (or red) data. Pixel format <see cref="ColorR8S"/>.
		/// </summary>
		R8S,

		/// <summary>
		/// Raw unsigned byte 16-bit RG data. Pixel format <see cref="ColorR8G8"/>.
		/// </summary>
		R8G8,

		/// <summary>
		/// Raw signed byte 16-bit RG data. Pixel format <see cref="ColorR8G8S"/>.
		/// </summary>
		R8G8S,

		/// <summary>
		/// Raw unsigned byte 24-bit RGB data in linear colorspace. Pixel format <see cref="ColorRgb24"/>.
		/// </summary>
		Rgb24,

		/// <summary>
		/// Raw unsigned byte 24-bit RGB data in the sRGB colorspace. Pixel format <see cref="ColorRgb24"/>.
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
		/// Raw unsigned byte 32-bit BGR data in linear colorspace. Pixel format <see cref="ColorBgra32"/>.
		/// 8 bits are unused.
		/// </summary>
		Bgr24x8,

		/// <summary>
		/// Raw unsigned byte 32-bit BGR data in the sRGB colorspace. Pixel format <see cref="ColorBgra32"/>.
		/// 8 bits are unused.
		/// </summary>
		Bgr24x8_sRGB,

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
		Rgbe32,

		/// <summary>
		/// 32-bit-per-pixel Hdr data with 24-bits for RGB (in CIE_1931 XYZ colorspace) and 8 bits for exponent. Pixel format <see cref="ColorXyze"/>.
		/// </summary>
		Xyze32,

		/// <summary>
		/// Raw 16-bit Unorm single channel format. 16 bits for Red channel.
		/// </summary>
		R16,

		/// <summary>
		/// Raw 16-bit Snorm single channel format. 16 bits for Red channel.
		/// </summary>
		R16S,

		/// <summary>
		/// Raw 16-bit Float single channel format. 16 bits for Red channel.
		/// </summary>
		R16F,

		/// <summary>
		/// Raw 32-bit Float single channel format. 32 bits for Red channel.
		/// </summary>
		R32F,

		/// <summary>
		/// Raw 16-bit-per-channel Unorm dual channel format. 16 bits each for Red and Green channels.
		/// </summary>
		R16G16,

		/// <summary>
		/// Raw 16-bit-per-channel Snorm dual channel format. 16 bits each for Red and Green channels.
		/// </summary>
		R16G16S,

		/// <summary>
		/// Raw 16-bit-per-channel Float dual channel format. 16 bits each for Red and Green channels.
		/// </summary>
		R16G16F,

		/// <summary>
		/// Raw 32-bit-per-channel Float dual channel format. 32 bits each for Red and Green channels.
		/// </summary>
		R32G32F,



		// ==== PACKED Formats ====

		/// <summary>
		/// 32-bit packed format with 2-bit Alpha, 10-bit Blue, 10-bit Green, and 10-bit Red components.
		/// Bits ordered from MSB to LSB as: A[31:30] B[29:20] G[19:10] R[9:0].
		/// Equivalent to VK_FORMAT_A2B10G10R10_UNORM_PACK32 and DXGI_FORMAT_R10G10B10A2_UNORM.
		/// </summary>
		R10G10B10A2_Packed,

		/// <summary>
		/// 32-bit packed format with 2-bit Alpha, 10-bit Red, 10-bit Green, and 10-bit Blue components.
		/// Bits ordered from MSB to LSB as: A[31:30] R[29:20] G[19:10] B[9:0].
		/// Equivalent to VK_FORMAT_A2R10G10B10_UNORM_PACK32.
		/// </summary>
		B10G10R10A2_Packed,

		/// <summary>
		/// 16-bit packed format with 5-bit Red, 6-bit Green, and 5-bit Blue components.
		/// Bits ordered from MSB to LSB as: R[15:11] G[10:5] B[4:0].
		/// Equivalent to VK_FORMAT_R5G6B5_UNORM_PACK16 and DXGI_FORMAT_B5G6R5_UNORM.
		/// </summary>
		B5G6R5_Packed,

		/// <summary>
		/// 16-bit packed format with 5-bit Blue, 6-bit Green, and 5-bit Red components.
		/// Bits ordered from MSB to LSB as: B[15:11] G[10:5] R[4:0].
		/// Equivalent to VK_FORMAT_B5G6R5_UNORM_PACK16.
		/// </summary>
		R5G6B5_Packed,

		/// <summary>
		/// 16-bit packed format with 5-bit Red, 5-bit Green, 5-bit Blue, and 1-bit Alpha components.
		/// Bits ordered from MSB to LSB as: R[15:11] G[10:6] B[5:1] A[0].
		/// Equivalent to VK_FORMAT_R5G5B5A1_UNORM_PACK16.
		/// </summary>
		A1B5G5R5_Packed,

		/// <summary>
		/// 16-bit packed format with 5-bit Blue, 5-bit Green, 5-bit Red, and 1-bit Alpha components.
		/// Bits ordered from MSB to LSB as: B[15:11] G[10:6] R[5:1] A[0].
		/// Equivalent to VK_FORMAT_B5G5R5A1_UNORM_PACK16.
		/// </summary>
		A1R5G5B5_Packed,

		/// <summary>
		/// 16-bit packed format with 1-bit Alpha, 5-bit Red, 5-bit Green, and 5-bit Blue components.
		/// Bits ordered from MSB to LSB as: A[15] R[14:10] G[9:5] B[4:0].
		/// Equivalent to VK_FORMAT_A1R5G5B5_UNORM_PACK16 and DXGI_FORMAT_B5G5R5A1_UNORM.
		/// </summary>
		B5G5R5A1_Packed,

		/// <summary>
		/// 16-bit packed format with 1-bit Alpha, 5-bit Blue, 5-bit Green, and 5-bit Red components.
		/// Bits ordered from MSB to LSB as: A[15] B[14:10] G[9:5] R[4:0].
		/// Equivalent to VK_FORMAT_A1B5G5R5_UNORM_PACK16.
		/// </summary>
		R5G5B5A1_Packed,

		/// <summary>
		/// 16-bit packed format with 4-bit Red, 4-bit Green, 4-bit Blue, and 4-bit Alpha components.
		/// Bits ordered from MSB to LSB as: R[15:12] G[11:8] B[7:4] A[3:0].
		/// Equivalent to VK_FORMAT_R4G4B4A4_UNORM_PACK16 and DXGI_FORMAT_A4B4G4R4_UNORM.
		/// </summary>
		A4B4G4R4_Packed,

		/// <summary>
		/// 16-bit packed format with 4-bit Blue, 4-bit Green, 4-bit Red, and 4-bit Alpha components.
		/// Bits ordered from MSB to LSB as: B[15:12] G[11:8] R[7:4] A[3:0].
		/// Equivalent to VK_FORMAT_B4G4R4A4_UNORM_PACK16.
		/// </summary>
		A4R4G4B4_Packed,

		/// <summary>
		/// 16-bit packed format with 4-bit Alpha, 4-bit Red, 4-bit Green, and 4-bit Blue components.
		/// Bits ordered from MSB to LSB as: A[15:12] R[11:8] G[7:4] B[3:0].
		/// Equivalent to VK_FORMAT_A4R4G4B4_UNORM_PACK16 and DXGI_FORMAT_B4G4R4A4_UNORM.
		/// </summary>
		B4G4R4A4_Packed,

		/// <summary>
		/// 16-bit packed format with 4-bit Alpha, 4-bit Blue, 4-bit Green, and 4-bit Red components.
		/// Bits ordered from MSB to LSB as: A[15:12] B[11:8] G[7:4] R[3:0].
		/// Equivalent to VK_FORMAT_A4B4G4R4_UNORM_PACK16.
		/// </summary>
		R4G4B4A4_Packed,

		/// <summary>
		/// 32-bit packed shared-exponent format with 5-bit Exponent, 9-bit Blue, 9-bit Green, and 9-bit Red components.
		/// Bits ordered from MSB to LSB as: E[31:27] B[26:18] G[17:9] R[8:0].
		/// Equivalent to VK_FORMAT_E5B9G9R9_UFLOAT_PACK32 and DXGI_FORMAT_R9G9B9E5_SHAREDEXP.
		/// </summary>
		R9G9B9E5_Packed,

		/// <summary>
		/// 32-bit packed floating point format with 10-bit Blue, 11-bit Green, and 11-bit Red components.
		/// Bits ordered from MSB to LSB as: B[31:22] G[21:11] R[10:0].
		/// Equivalent to VK_FORMAT_B10G11R11_UFLOAT_PACK32 and DXGI_FORMAT_R11G11B10_FLOAT.
		/// </summary>
		R11G11B10UF_Packed,


		// ==== BCN Formats ====

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
		/// Signed BC4 single-channel encoding. Only luminance (or red channel) is encoded. Always linear.
		/// </summary>
		Bc4S,

		/// <summary>
		/// BC5 dual-channel encoding. Only red and green channels are encoded. Mostly used for normal maps. Always linear.
		/// </summary>
		Bc5,

		/// <summary>
		/// Signed BC5 dual-channel encoding. Only red and green channels are encoded. Mostly used for normal maps. Always linear.
		/// </summary>
		Bc5S,

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
		/// ATC / Adreno Texture Compression encoding in (assumed) sRGB colorspace. Derivative of BC1.
		/// </summary>
		Atc,

		/// <summary>
		/// ATC / Adreno Texture Compression encoding in (assumed) sRGB colorspace. Derivative of BC2. Good for sharp alpha transitions.
		/// </summary>
		AtcExplicitAlpha,

		/// <summary>
		/// ATC / Adreno Texture Compression encoding in (assumed) sRGB colorspace. Derivative of BC3. Good for smooth alpha transitions.
		/// </summary>
		AtcInterpolatedAlpha,

		/// <summary>
		/// Unknown format
		/// </summary>
		Unknown
	}
	// ReSharper restore InconsistentNaming

	public enum CompressionFormatType
	{
		Undefined,
		RawUnorm,
		RawUnormSrgb,
		RawSnorm,
		RawFloat,
		RawUFloat,
		RawSharedExponent,
		BlockUnorm,
		BlockUnormSrgb,
		BlockSnorm,
		BlockSFloat,
		BlockUFloat
	}

	public readonly struct CompressionFormatInfo
	{
		public CompressionFormat Format { get; }
		public CompressionFormatType FormatType { get; }
		public bool SupportsAlpha { get; }
		public BlockPixelSize BlockSize { get; }
		public Type TexelType { get; }
		public int BytesPerTexel { get; }

		public CompressionFormatInfo(CompressionFormat format, CompressionFormatType formatType, bool supportsAlpha, BlockPixelSize blockSize, Type texelType)
		{
			Format = format;
			FormatType = formatType;
			SupportsAlpha = supportsAlpha;
			BlockSize = blockSize;
			TexelType = texelType;

			// Calculate bytes per texel
				var method = typeof(Unsafe)
					.GetMethods(BindingFlags.Public | BindingFlags.Static)
					.First(x => x.Name == nameof(Unsafe.SizeOf) &&
					            x.GetParameters().Length == 0);

			BytesPerTexel = method.MakeGenericMethod(texelType).Invoke(null, null) as int? ?? 0;
		}

		public override string ToString()
		{
			return $"{nameof(Format)}: {Format}, {nameof(FormatType)}: {FormatType}, {nameof(SupportsAlpha)}: {SupportsAlpha}, {nameof(BlockSize)}: {BlockSize}, {nameof(TexelType)}: {TexelType}, {nameof(BytesPerTexel)}: {BytesPerTexel}";
		}

		public static IReadOnlyDictionary<CompressionFormat, CompressionFormatInfo> Infos { get; } = new Dictionary<CompressionFormat, CompressionFormatInfo>()
		{
			// Single-channel formats (R)
			{ CompressionFormat.R8,              new CompressionFormatInfo(CompressionFormat.R8,              CompressionFormatType.RawUnorm,     false, BlockPixelSize.Size1x1x1, typeof(ColorR8))},
			{ CompressionFormat.R8S,             new CompressionFormatInfo(CompressionFormat.R8S,             CompressionFormatType.RawSnorm,     false, BlockPixelSize.Size1x1x1, typeof(ColorR8S))},
			{ CompressionFormat.R16,             new CompressionFormatInfo(CompressionFormat.R16,             CompressionFormatType.RawUnorm,     false, BlockPixelSize.Size1x1x1, typeof(ColorR16))},
			{ CompressionFormat.R16S,            new CompressionFormatInfo(CompressionFormat.R16S,            CompressionFormatType.RawSnorm,     false, BlockPixelSize.Size1x1x1, typeof(ColorR16S))},
			{ CompressionFormat.R16F,            new CompressionFormatInfo(CompressionFormat.R16F,            CompressionFormatType.RawFloat,     false, BlockPixelSize.Size1x1x1, typeof(ColorR16F))},
			{ CompressionFormat.R32F,            new CompressionFormatInfo(CompressionFormat.R32F,            CompressionFormatType.RawFloat,     false, BlockPixelSize.Size1x1x1, typeof(ColorR32F))},

			// Dual-channel formats (RG)
			{ CompressionFormat.R8G8,            new CompressionFormatInfo(CompressionFormat.R8G8,            CompressionFormatType.RawUnorm,     false, BlockPixelSize.Size1x1x1, typeof(ColorR8G8))},
			{ CompressionFormat.R8G8S,           new CompressionFormatInfo(CompressionFormat.R8G8S,           CompressionFormatType.RawSnorm,     false, BlockPixelSize.Size1x1x1, typeof(ColorR8G8S))},
			{ CompressionFormat.R16G16,          new CompressionFormatInfo(CompressionFormat.R16G16,          CompressionFormatType.RawUnorm,     false, BlockPixelSize.Size1x1x1, typeof(System.Numerics.Vector2))},
			{ CompressionFormat.R16G16S,         new CompressionFormatInfo(CompressionFormat.R16G16S,         CompressionFormatType.RawSnorm,     false, BlockPixelSize.Size1x1x1, typeof(System.Numerics.Vector2))},
			{ CompressionFormat.R16G16F,         new CompressionFormatInfo(CompressionFormat.R16G16F,         CompressionFormatType.RawFloat,     false, BlockPixelSize.Size1x1x1, typeof(System.Numerics.Vector2))},
			{ CompressionFormat.R32G32F,         new CompressionFormatInfo(CompressionFormat.R32G32F,         CompressionFormatType.RawFloat,     false, BlockPixelSize.Size1x1x1, typeof(System.Numerics.Vector2))},

			// Triple-channel formats (RGB)
			{ CompressionFormat.Rgb24,           new CompressionFormatInfo(CompressionFormat.Rgb24,           CompressionFormatType.RawUnorm,     false, BlockPixelSize.Size1x1x1, typeof(ColorRgb24))},
			{ CompressionFormat.Rgb24_sRGB,      new CompressionFormatInfo(CompressionFormat.Rgb24_sRGB,      CompressionFormatType.RawUnormSrgb, false, BlockPixelSize.Size1x1x1, typeof(ColorRgb24))},
			{ CompressionFormat.Bgr24,           new CompressionFormatInfo(CompressionFormat.Bgr24,           CompressionFormatType.RawUnorm,     false, BlockPixelSize.Size1x1x1, typeof(ColorBgr24))},
			{ CompressionFormat.Bgr24_sRGB,      new CompressionFormatInfo(CompressionFormat.Bgr24_sRGB,      CompressionFormatType.RawUnormSrgb, false, BlockPixelSize.Size1x1x1, typeof(ColorBgr24))},
			{ CompressionFormat.RgbFloat,        new CompressionFormatInfo(CompressionFormat.RgbFloat,        CompressionFormatType.RawFloat,     false, BlockPixelSize.Size1x1x1, typeof(ColorRgbFloat))},
			{ CompressionFormat.RgbHalf,         new CompressionFormatInfo(CompressionFormat.RgbHalf,         CompressionFormatType.RawFloat,     false, BlockPixelSize.Size1x1x1, typeof(ColorRgbHalf))},
			{ CompressionFormat.Bgr24x8,         new CompressionFormatInfo(CompressionFormat.Bgr24x8,         CompressionFormatType.RawUnorm,     false, BlockPixelSize.Size1x1x1, typeof(ColorBgra32))},
			{ CompressionFormat.Bgr24x8_sRGB,    new CompressionFormatInfo(CompressionFormat.Bgr24x8_sRGB,    CompressionFormatType.RawUnormSrgb, false, BlockPixelSize.Size1x1x1, typeof(ColorBgra32))},

			// Quad-channel formats (RGBA)
			{ CompressionFormat.Rgba32,          new CompressionFormatInfo(CompressionFormat.Rgba32,          CompressionFormatType.RawUnorm,     true,  BlockPixelSize.Size1x1x1, typeof(ColorRgba32))},
			{ CompressionFormat.Rgba32_sRGB,     new CompressionFormatInfo(CompressionFormat.Rgba32_sRGB,     CompressionFormatType.RawUnormSrgb, true,  BlockPixelSize.Size1x1x1, typeof(ColorRgba32))},
			{ CompressionFormat.Bgra32,          new CompressionFormatInfo(CompressionFormat.Bgra32,          CompressionFormatType.RawUnorm,     true,  BlockPixelSize.Size1x1x1, typeof(ColorBgra32))},
			{ CompressionFormat.Bgra32_sRGB,     new CompressionFormatInfo(CompressionFormat.Bgra32_sRGB,     CompressionFormatType.RawUnormSrgb, true,  BlockPixelSize.Size1x1x1, typeof(ColorBgra32))},
			{ CompressionFormat.RgbaFloat,       new CompressionFormatInfo(CompressionFormat.RgbaFloat,       CompressionFormatType.RawFloat,     true,  BlockPixelSize.Size1x1x1, typeof(ColorRgbaFloat))},
			{ CompressionFormat.RgbaHalf,        new CompressionFormatInfo(CompressionFormat.RgbaHalf,        CompressionFormatType.RawFloat,     true,  BlockPixelSize.Size1x1x1, typeof(ColorRgbaHalf))},

			// Special packed formats
			{ CompressionFormat.R5G6B5_Packed,          new CompressionFormatInfo(CompressionFormat.R5G6B5_Packed,          CompressionFormatType.RawUnorm,     false, BlockPixelSize.Size1x1x1, typeof(ColorR5G6B5Packed))},
			{ CompressionFormat.B5G6R5_Packed,          new CompressionFormatInfo(CompressionFormat.B5G6R5_Packed,          CompressionFormatType.RawUnorm,     false, BlockPixelSize.Size1x1x1, typeof(ColorB5G6R5Packed))},
			{ CompressionFormat.R5G5B5A1_Packed,        new CompressionFormatInfo(CompressionFormat.R5G5B5A1_Packed,        CompressionFormatType.RawUnorm,     true,  BlockPixelSize.Size1x1x1, typeof(ColorR5G5B5A1Packed))},
			{ CompressionFormat.B5G5R5A1_Packed,        new CompressionFormatInfo(CompressionFormat.B5G5R5A1_Packed,        CompressionFormatType.RawUnorm,     true,  BlockPixelSize.Size1x1x1, typeof(ColorB5G5R5A1Packed))},
			{ CompressionFormat.A1B5G5R5_Packed,        new CompressionFormatInfo(CompressionFormat.A1B5G5R5_Packed,        CompressionFormatType.RawUnorm,     true,  BlockPixelSize.Size1x1x1, typeof(ColorA1B5G5R5Packed))},
			{ CompressionFormat.A1R5G5B5_Packed,        new CompressionFormatInfo(CompressionFormat.A1R5G5B5_Packed,        CompressionFormatType.RawUnorm,     true,  BlockPixelSize.Size1x1x1, typeof(ColorA1R5G5B5Packed))},
			{ CompressionFormat.R4G4B4A4_Packed,        new CompressionFormatInfo(CompressionFormat.R4G4B4A4_Packed,        CompressionFormatType.RawUnorm,     true,  BlockPixelSize.Size1x1x1, typeof(ColorR4G4B4A4Packed))},
			{ CompressionFormat.B4G4R4A4_Packed,        new CompressionFormatInfo(CompressionFormat.B4G4R4A4_Packed,        CompressionFormatType.RawUnorm,     true,  BlockPixelSize.Size1x1x1, typeof(ColorB4G4R4A4Packed))},
			{ CompressionFormat.A4B4G4R4_Packed,        new CompressionFormatInfo(CompressionFormat.A4B4G4R4_Packed,        CompressionFormatType.RawUnorm,     true,  BlockPixelSize.Size1x1x1, typeof(ColorA4B4G4R4Packed))},
			{ CompressionFormat.A4R4G4B4_Packed,        new CompressionFormatInfo(CompressionFormat.A4R4G4B4_Packed,        CompressionFormatType.RawUnorm,     true,  BlockPixelSize.Size1x1x1, typeof(ColorA4R4G4B4Packed))},
			{ CompressionFormat.R10G10B10A2_Packed,     new CompressionFormatInfo(CompressionFormat.R10G10B10A2_Packed,     CompressionFormatType.RawUnorm,     true,  BlockPixelSize.Size1x1x1, typeof(ColorR10G10B10A2Packed))},
			{ CompressionFormat.B10G10R10A2_Packed,     new CompressionFormatInfo(CompressionFormat.B10G10R10A2_Packed,     CompressionFormatType.RawUnorm,     true,  BlockPixelSize.Size1x1x1, typeof(ColorB10G10R10A2Packed))},
			{ CompressionFormat.R11G11B10UF_Packed,     new CompressionFormatInfo(CompressionFormat.R11G11B10UF_Packed,     CompressionFormatType.RawUFloat,     false, BlockPixelSize.Size1x1x1, typeof(ColorR11G11B10PackedUFloat))},
			{ CompressionFormat.R9G9B9E5_Packed,        new CompressionFormatInfo(CompressionFormat.R9G9B9E5_Packed,        CompressionFormatType.RawSharedExponent, false, BlockPixelSize.Size1x1x1, typeof(ColorR9G9B9E5))},

			// Shared exponent formats
			{ CompressionFormat.Rgbe32,        new CompressionFormatInfo(CompressionFormat.Rgbe32,        CompressionFormatType.RawSharedExponent, false, BlockPixelSize.Size1x1x1, typeof(ColorRgbe))},
			{ CompressionFormat.Xyze32,        new CompressionFormatInfo(CompressionFormat.Xyze32,        CompressionFormatType.RawSharedExponent, false, BlockPixelSize.Size1x1x1, typeof(ColorXyze))},

			// Block Formats
			{ CompressionFormat.Bc1,             new CompressionFormatInfo(CompressionFormat.Bc1,             CompressionFormatType.BlockUnorm,    false, BlockPixelSize.Size4x4x1, typeof(Bc1Block))},
			{ CompressionFormat.Bc1_sRGB,        new CompressionFormatInfo(CompressionFormat.Bc1_sRGB,        CompressionFormatType.BlockUnormSrgb, false, BlockPixelSize.Size4x4x1, typeof(Bc1Block))},
			{ CompressionFormat.Bc1WithAlpha,    new CompressionFormatInfo(CompressionFormat.Bc1WithAlpha,    CompressionFormatType.BlockUnorm,    true,  BlockPixelSize.Size4x4x1, typeof(Bc1Block))},
			{ CompressionFormat.Bc1WithAlpha_sRGB, new CompressionFormatInfo(CompressionFormat.Bc1WithAlpha_sRGB, CompressionFormatType.BlockUnormSrgb, true, BlockPixelSize.Size4x4x1, typeof(Bc1Block))},
			{ CompressionFormat.Bc2,             new CompressionFormatInfo(CompressionFormat.Bc2,             CompressionFormatType.BlockUnorm,    true,  BlockPixelSize.Size4x4x1, typeof(Bc2Block))},
			{ CompressionFormat.Bc2_sRGB,        new CompressionFormatInfo(CompressionFormat.Bc2_sRGB,        CompressionFormatType.BlockUnormSrgb, true, BlockPixelSize.Size4x4x1, typeof(Bc2Block))},
			{ CompressionFormat.Bc3,             new CompressionFormatInfo(CompressionFormat.Bc3,             CompressionFormatType.BlockUnorm,    true,  BlockPixelSize.Size4x4x1, typeof(Bc3Block))},
			{ CompressionFormat.Bc3_sRGB,        new CompressionFormatInfo(CompressionFormat.Bc3_sRGB,        CompressionFormatType.BlockUnormSrgb, true, BlockPixelSize.Size4x4x1, typeof(Bc3Block))},
			{ CompressionFormat.Bc4,             new CompressionFormatInfo(CompressionFormat.Bc4,             CompressionFormatType.BlockUnorm,    false, BlockPixelSize.Size4x4x1, typeof(Bc4Block))},
			{ CompressionFormat.Bc5,             new CompressionFormatInfo(CompressionFormat.Bc5,             CompressionFormatType.BlockUnorm,    false, BlockPixelSize.Size4x4x1, typeof(Bc5Block))},
			{ CompressionFormat.Bc7,             new CompressionFormatInfo(CompressionFormat.Bc7,             CompressionFormatType.BlockUnorm,    true,  BlockPixelSize.Size4x4x1, typeof(Bc7Block))},
			{ CompressionFormat.Bc7_sRGB,        new CompressionFormatInfo(CompressionFormat.Bc7_sRGB,        CompressionFormatType.BlockUnormSrgb, true, BlockPixelSize.Size4x4x1, typeof(Bc7Block))},
			{ CompressionFormat.Atc,             new CompressionFormatInfo(CompressionFormat.Atc,             CompressionFormatType.BlockUnorm, false, BlockPixelSize.Size4x4x1, typeof(AtcBlock))},
			{ CompressionFormat.AtcExplicitAlpha, new CompressionFormatInfo(CompressionFormat.AtcExplicitAlpha, CompressionFormatType.BlockUnorm, true, BlockPixelSize.Size4x4x1, typeof(AtcExplicitAlphaBlock))},
			{ CompressionFormat.AtcInterpolatedAlpha, new CompressionFormatInfo(CompressionFormat.AtcInterpolatedAlpha, CompressionFormatType.BlockUnorm, true, BlockPixelSize.Size4x4x1, typeof(AtcInterpolatedAlphaBlock))},

			// Block Signed Formats
			{ CompressionFormat.Bc4S,            new CompressionFormatInfo(CompressionFormat.Bc4S,            CompressionFormatType.BlockSnorm,    false, BlockPixelSize.Size4x4x1, typeof(Bc4Block))},
			{ CompressionFormat.Bc5S,            new CompressionFormatInfo(CompressionFormat.Bc5S,            CompressionFormatType.BlockSnorm,    false, BlockPixelSize.Size4x4x1, typeof(Bc5Block))},

			// Block Float Formats
			{ CompressionFormat.Bc6U,            new CompressionFormatInfo(CompressionFormat.Bc6U,            CompressionFormatType.BlockUFloat,    false, BlockPixelSize.Size4x4x1, typeof(Bc6Block))},
			{ CompressionFormat.Bc6S,            new CompressionFormatInfo(CompressionFormat.Bc6S,            CompressionFormatType.BlockSFloat,    false, BlockPixelSize.Size4x4x1, typeof(Bc6Block))},
		};
	}

	public static class CompressionFormatExtensions
	{
		public static CompressionFormatInfo GetInfo(this CompressionFormat format)
		{
			if (!CompressionFormatInfo.Infos.TryGetValue(format, out var info))
			{
				throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}

			return info;
		}

		public static bool IsBlockCompressedFormat(this CompressionFormat format)
		{
			CompressionFormatInfo info = format.GetInfo();

			return info.FormatType == CompressionFormatType.BlockUnorm
				|| info.FormatType == CompressionFormatType.BlockUnormSrgb
				|| info.FormatType == CompressionFormatType.BlockSnorm
				|| info.FormatType == CompressionFormatType.BlockSFloat
				|| info.FormatType == CompressionFormatType.BlockUFloat;
		}

		public static bool IsRawPixelFormat(this CompressionFormat format)
		{
			return !IsBlockCompressedFormat(format);
		}

		/// <summary>
		/// Return bytes per block (or per pixel if not block compressed format)
		/// </summary>
		public static int GetBytesPerBlock(this CompressionFormat format)
		{
			CompressionFormatInfo info = format.GetInfo();

			return info.BytesPerTexel;
		}

		public static bool IsHdrFormat(this CompressionFormat format)
		{
			CompressionFormatInfo info = format.GetInfo();

			return info.FormatType == CompressionFormatType.RawFloat
				|| info.FormatType == CompressionFormatType.RawUFloat
				|| info.FormatType == CompressionFormatType.BlockSFloat
				|| info.FormatType == CompressionFormatType.BlockUFloat
				|| info.FormatType == CompressionFormatType.RawSharedExponent;
		}

		public static bool SupportsAlpha(this CompressionFormat format)
		{
			CompressionFormatInfo info = format.GetInfo();

			return info.SupportsAlpha;
		}

		public static bool IsSRGBFormat(this CompressionFormat format)
		{
			CompressionFormatInfo info = format.GetInfo();

			return info.FormatType == CompressionFormatType.RawUnormSrgb
				|| info.FormatType == CompressionFormatType.BlockUnormSrgb;
		}

		public static bool IsSignedFormat(this CompressionFormat format)
		{
			CompressionFormatInfo info = format.GetInfo();

			return info.FormatType == CompressionFormatType.RawSnorm
				|| info.FormatType == CompressionFormatType.BlockSnorm
				|| info.FormatType == CompressionFormatType.RawFloat
				|| info.FormatType == CompressionFormatType.BlockSFloat;
		}

		// Whether the format is a signed normalized format, i.e. values range from [-1, 1]
		public static bool IsSNormFormat(this CompressionFormat format)
		{
			CompressionFormatInfo info = format.GetInfo();

			return info.FormatType == CompressionFormatType.RawSnorm
				|| info.FormatType == CompressionFormatType.BlockSnorm;
		}

		// Whether the format is a unsigned normalized format, i.e. values range from [0, 1]
		public static bool IsUNormFormat(this CompressionFormat format)
		{
			CompressionFormatInfo info = format.GetInfo();

			return info.FormatType == CompressionFormatType.RawUnorm
				|| info.FormatType == CompressionFormatType.RawUnormSrgb
				|| info.FormatType == CompressionFormatType.BlockUnorm
				|| info.FormatType == CompressionFormatType.BlockUnormSrgb;
		}

		public static long CalculateMipByteSize(this CompressionFormat format, int width, int height, int depth)
		{
			if (format.IsBlockCompressedFormat())
			{
				return ImageToBlocks.CalculateNumOfBlocks(format, width, height, depth) * (long)format.GetBytesPerBlock();
			}

			return (long)format.GetBytesPerBlock() * width * height * depth;
		}

		internal static System.Type GetPixelType(this CompressionFormat format)
		{
			CompressionFormatInfo info = format.GetInfo();

			return info.TexelType;
		}

		public static CompressionFormat ToNonSrgbFormat(this CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.Rgb24_sRGB:
					return CompressionFormat.Rgb24;
				case CompressionFormat.Bgr24_sRGB:
					return CompressionFormat.Bgr24;
				case CompressionFormat.Rgba32_sRGB:
					return CompressionFormat.Rgba32;
				case CompressionFormat.Bgra32_sRGB:
					return CompressionFormat.Bgra32;
				case CompressionFormat.Bc1_sRGB:
					return CompressionFormat.Bc1;
				case CompressionFormat.Bc1WithAlpha_sRGB:
					return CompressionFormat.Bc1WithAlpha;
				case CompressionFormat.Bc2_sRGB:
					return CompressionFormat.Bc2;
				case CompressionFormat.Bc3_sRGB:
					return CompressionFormat.Bc3;
				case CompressionFormat.Bc7_sRGB:
					return CompressionFormat.Bc7;
				default:
					return format;
			}
		}

		internal static ColorConversionMode GetColorConversionMode(this CompressionFormat sourceFormat,
			CompressionFormat targetFormat)
		{
			var sourceIsSrgb = sourceFormat.IsSRGBFormat();
			var targetIsSrgb = targetFormat.IsSRGBFormat();

			return (sourceIsSrgb, targetIsSrgb) switch
			{
				(false, false) or
					(true, true) => ColorConversionMode.None,
				(false, true) => ColorConversionMode.LinearToSrgb,
				(true, false) => ColorConversionMode.SrgbToLinear
			};
		}

		/// <summary>
		/// Attempts to guess the alpha channel encoding type based on the compression format.
		/// Block compressed formats typically use premultiplied alpha, while HDR formats use straight alpha.
		/// </summary>
		/// <param name="format">The compression format to analyze</param>
		/// <returns>Premultiplied for block compressed formats, Straight for HDR formats, Unknown otherwise</returns>
		internal static AlphaChannelHint GuessAlphaChannelHint(this CompressionFormat format)
		{
			if (format.IsHdrFormat())
				return AlphaChannelHint.Straight;
			if (format.IsBlockCompressedFormat())
				return AlphaChannelHint.Premultiplied;

			return AlphaChannelHint.Unknown;
		}

		internal static BlockPixelSize GetBlockPixelSize(this CompressionFormat format)
		{
			CompressionFormatInfo info = format.GetInfo();

			return info.BlockSize;
		}
	}
}
