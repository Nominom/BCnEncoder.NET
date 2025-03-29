using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncoder.TextureFormats
{
	/// <summary>
	/// Flags to indicate which members contain valid data.
	/// </summary>
	[Flags]
	public enum HeaderFlags : uint
	{
		/// <summary>
		/// Required in every .dds file.
		/// </summary>
		DdsdCaps = 0x1,
		/// <summary>
		/// Required in every .dds file.
		/// </summary>
		DdsdHeight = 0x2,
		/// <summary>
		/// Required in every .dds file.
		/// </summary>
		DdsdWidth = 0x4,
		/// <summary>
		/// Required when pitch is provided for an uncompressed texture.
		/// </summary>
		DdsdPitch = 0x8,
		/// <summary>
		/// Required in every .dds file.
		/// </summary>
		DdsdPixelformat = 0x1000,
		/// <summary>
		/// Required in a mipmapped texture.
		/// </summary>
		DdsdMipmapcount = 0x20000,
		/// <summary>
		/// Required when pitch is provided for a compressed texture.
		/// </summary>
		DdsdLinearsize = 0x80000,
		/// <summary>
		/// Required in a depth texture.
		/// </summary>
		DdsdDepth = 0x800000,

		Required = DdsdCaps | DdsdHeight | DdsdWidth | DdsdPixelformat
	}

	/// <summary>
	/// Specifies the complexity of the surfaces stored.
	/// </summary>
	[Flags]
	public enum HeaderCaps : uint
	{
		/// <summary>
		/// Optional; must be used on any file that contains more than one surface (a mipmap, a cubic environment map, or mipmapped volume texture).
		/// </summary>
		DdscapsComplex = 0x8,
		/// <summary>
		/// Optional; should be used for a mipmap.
		/// </summary>
		DdscapsMipmap = 0x400000,
		/// <summary>
		/// Required
		/// </summary>
		DdscapsTexture = 0x1000
	}

	/// <summary>
	/// Additional detail about the surfaces stored.
	/// </summary>
	[Flags]
	public enum HeaderCaps2 : uint
	{
		/// <summary>
		/// Required for a cube map.
		/// </summary>
		Ddscaps2Cubemap = 0x200,
		/// <summary>
		/// Required when these surfaces are stored in a cube map.
		/// </summary>
		Ddscaps2CubemapPositivex = 0x400,
		/// <summary>
		/// Required when these surfaces are stored in a cube map.
		/// </summary>
		Ddscaps2CubemapNegativex = 0x800,
		/// <summary>
		/// Required when these surfaces are stored in a cube map.
		/// </summary>
		Ddscaps2CubemapPositivey = 0x1000,
		/// <summary>
		/// Required when these surfaces are stored in a cube map.
		/// </summary>
		Ddscaps2CubemapNegativey = 0x2000,
		/// <summary>
		/// Required when these surfaces are stored in a cube map.
		/// </summary>
		Ddscaps2CubemapPositivez = 0x4000,
		/// <summary>
		/// Required when these surfaces are stored in a cube map.
		/// </summary>
		Ddscaps2CubemapNegativez = 0x8000,
		/// <summary>
		/// Required for a volume texture.
		/// </summary>
		Ddscaps2Volume = 0x200000
	}

	[Flags]
	public enum PixelFormatFlags : uint
	{
		/// <summary>
		/// Texture contains alpha data; dwRGBAlphaBitMask contains valid data.
		/// </summary>
		DdpfAlphaPixels = 0x1,
		/// <summary>
		/// Used in some older DDS files for alpha channel only uncompressed data (dwRGBBitCount contains the alpha channel bitcount; dwABitMask contains valid data)
		/// </summary>
		DdpfAlpha = 0x2,
		/// <summary>
		/// Texture contains compressed RGB data; dwFourCC contains valid data.
		/// </summary>
		DdpfFourcc = 0x4,
		/// <summary>
		/// Texture contains uncompressed RGB data; dwRGBBitCount and the RGB masks (dwRBitMask, dwGBitMask, dwBBitMask) contain valid data.
		/// </summary>
		DdpfRgb = 0x40,
		/// <summary>
		/// Used in some older DDS files for YUV uncompressed data (dwRGBBitCount contains the YUV bit count; dwRBitMask contains the Y mask, dwGBitMask contains the U mask, dwBBitMask contains the V mask)
		/// </summary>
		DdpfYuv = 0x200,
		/// <summary>
		/// Used in some older DDS files for single channel color uncompressed data (dwRGBBitCount contains the luminance channel bit count; dwRBitMask contains the channel mask). Can be combined with DDPF_ALPHAPIXELS for a two channel DDS file.
		/// </summary>
		DdpfLuminance = 0x20000,
		DdpfRgba = DdpfRgb | DdpfAlphaPixels
	}

	// Note only avaialble on DX12+ dds files.
	// Any decoder that only support dx11 or lower, will fail.
	public enum Dx10HeaderMiscFlags2 : uint
	{
		Dx10HeaderMiscFlags2AlphaModeUnknown = 0x0,
		Dx10HeaderMiscFlags2AlphaModeStraight = 0x1,
		Dx10HeaderMiscFlags2AlphaModePremultiplied = 0x2,
		Dx10HeaderMiscFlags2AlphaModeOpaque = 0x3,
		Dx10HeaderMiscFlags2AlphaModeCustom = 0x4
	}

	public enum D3D10ResourceDimension : uint
	{
		D3D10ResourceDimensionUnknown,
		D3D10ResourceDimensionBuffer,
		D3D10ResourceDimensionTexture1D,
		D3D10ResourceDimensionTexture2D,
		D3D10ResourceDimensionTexture3D
	};

	public enum DxgiFormat : uint
	{
		DxgiFormatUnknown,
		DxgiFormatR32G32B32A32Typeless,
		DxgiFormatR32G32B32A32Float,
		DxgiFormatR32G32B32A32Uint,
		DxgiFormatR32G32B32A32Sint,
		DxgiFormatR32G32B32Typeless,
		DxgiFormatR32G32B32Float,
		DxgiFormatR32G32B32Uint,
		DxgiFormatR32G32B32Sint,
		DxgiFormatR16G16B16A16Typeless,
		DxgiFormatR16G16B16A16Float,
		DxgiFormatR16G16B16A16Unorm,
		DxgiFormatR16G16B16A16Uint,
		DxgiFormatR16G16B16A16Snorm,
		DxgiFormatR16G16B16A16Sint,
		DxgiFormatR32G32Typeless,
		DxgiFormatR32G32Float,
		DxgiFormatR32G32Uint,
		DxgiFormatR32G32Sint,
		DxgiFormatR32G8X24Typeless,
		DxgiFormatD32FloatS8X24Uint,
		DxgiFormatR32FloatX8X24Typeless,
		DxgiFormatX32TypelessG8X24Uint,
		DxgiFormatR10G10B10A2Typeless,
		DxgiFormatR10G10B10A2Unorm,
		DxgiFormatR10G10B10A2Uint,
		DxgiFormatR11G11B10Float,
		DxgiFormatR8G8B8A8Typeless,
		DxgiFormatR8G8B8A8Unorm,
		DxgiFormatR8G8B8A8UnormSrgb,
		DxgiFormatR8G8B8A8Uint,
		DxgiFormatR8G8B8A8Snorm,
		DxgiFormatR8G8B8A8Sint,
		DxgiFormatR16G16Typeless,
		DxgiFormatR16G16Float,
		DxgiFormatR16G16Unorm,
		DxgiFormatR16G16Uint,
		DxgiFormatR16G16Snorm,
		DxgiFormatR16G16Sint,
		DxgiFormatR32Typeless,
		DxgiFormatD32Float,
		DxgiFormatR32Float,
		DxgiFormatR32Uint,
		DxgiFormatR32Sint,
		DxgiFormatR24G8Typeless,
		DxgiFormatD24UnormS8Uint,
		DxgiFormatR24UnormX8Typeless,
		DxgiFormatX24TypelessG8Uint,
		DxgiFormatR8G8Typeless,
		DxgiFormatR8G8Unorm,
		DxgiFormatR8G8Uint,
		DxgiFormatR8G8Snorm,
		DxgiFormatR8G8Sint,
		DxgiFormatR16Typeless,
		DxgiFormatR16Float,
		DxgiFormatD16Unorm,
		DxgiFormatR16Unorm,
		DxgiFormatR16Uint,
		DxgiFormatR16Snorm,
		DxgiFormatR16Sint,
		DxgiFormatR8Typeless,
		DxgiFormatR8Unorm,
		DxgiFormatR8Uint,
		DxgiFormatR8Snorm,
		DxgiFormatR8Sint,
		DxgiFormatA8Unorm,
		DxgiFormatR1Unorm,
		DxgiFormatR9G9B9E5Sharedexp,
		DxgiFormatR8G8B8G8Unorm,
		DxgiFormatG8R8G8B8Unorm,
		DxgiFormatBc1Typeless,
		DxgiFormatBc1Unorm,
		DxgiFormatBc1UnormSrgb,
		DxgiFormatBc2Typeless,
		DxgiFormatBc2Unorm,
		DxgiFormatBc2UnormSrgb,
		DxgiFormatBc3Typeless,
		DxgiFormatBc3Unorm,
		DxgiFormatBc3UnormSrgb,
		DxgiFormatBc4Typeless,
		DxgiFormatBc4Unorm,
		DxgiFormatBc4Snorm,
		DxgiFormatBc5Typeless,
		DxgiFormatBc5Unorm,
		DxgiFormatBc5Snorm,
		DxgiFormatB5G6R5Unorm,
		DxgiFormatB5G5R5A1Unorm,
		DxgiFormatB8G8R8A8Unorm,
		DxgiFormatB8G8R8X8Unorm,
		DxgiFormatR10G10B10XrBiasA2Unorm,
		DxgiFormatB8G8R8A8Typeless,
		DxgiFormatB8G8R8A8UnormSrgb,
		DxgiFormatB8G8R8X8Typeless,
		DxgiFormatB8G8R8X8UnormSrgb,
		DxgiFormatBc6HTypeless,
		DxgiFormatBc6HUf16,
		DxgiFormatBc6HSf16,
		DxgiFormatBc7Typeless,
		DxgiFormatBc7Unorm,
		DxgiFormatBc7UnormSrgb,
		DxgiFormatAyuv,
		DxgiFormatY410,
		DxgiFormatY416,
		DxgiFormatNv12,
		DxgiFormatP010,
		DxgiFormatP016,
		DxgiFormat420Opaque,
		DxgiFormatYuy2,
		DxgiFormatY210,
		DxgiFormatY216,
		DxgiFormatNv11,
		DxgiFormatAi44,
		DxgiFormatIa44,
		DxgiFormatP8,
		DxgiFormatA8P8,
		DxgiFormatB4G4R4A4Unorm,
		DxgiFormatP208,
		DxgiFormatV208,
		DxgiFormatV408,
		DxgiFormatForceUint,

		// Added here due to lack of an official value
		DxgiFormatAtcExt = 300,
		DxgiFormatAtcExplicitAlphaExt,
		DxgiFormatAtcInterpolatedAlphaExt
	};

	public static class DxgiFormatExtensions
	{
		public static int GetByteSize(this DxgiFormat format)
		{
			switch (format)
			{
				case DxgiFormat.DxgiFormatUnknown:
					return 4;
				case DxgiFormat.DxgiFormatR32G32B32A32Typeless:
					return 4 * 4;
				case DxgiFormat.DxgiFormatR32G32B32A32Float:
					return 4 * 4;
				case DxgiFormat.DxgiFormatR32G32B32A32Uint:
					return 4 * 4;
				case DxgiFormat.DxgiFormatR32G32B32A32Sint:
					return 4 * 4;
				case DxgiFormat.DxgiFormatR32G32B32Typeless:
					return 4 * 3;
				case DxgiFormat.DxgiFormatR32G32B32Float:
					return 4 * 3;
				case DxgiFormat.DxgiFormatR32G32B32Uint:
					return 4 * 3;
				case DxgiFormat.DxgiFormatR32G32B32Sint:
					return 4 * 3;
				case DxgiFormat.DxgiFormatR16G16B16A16Typeless:
					return 4 * 2;
				case DxgiFormat.DxgiFormatR16G16B16A16Float:
					return 4 * 2;
				case DxgiFormat.DxgiFormatR16G16B16A16Unorm:
					return 4 * 2;
				case DxgiFormat.DxgiFormatR16G16B16A16Uint:
					return 4 * 2;
				case DxgiFormat.DxgiFormatR16G16B16A16Snorm:
					return 4 * 2;
				case DxgiFormat.DxgiFormatR16G16B16A16Sint:
					return 4 * 2;
				case DxgiFormat.DxgiFormatR32G32Typeless:
					return 4 * 2;
				case DxgiFormat.DxgiFormatR32G32Float:
					return 4 * 2;
				case DxgiFormat.DxgiFormatR32G32Uint:
					return 4 * 2;
				case DxgiFormat.DxgiFormatR32G32Sint:
					return 4 * 2;
				case DxgiFormat.DxgiFormatR32G8X24Typeless:
					return 4 * 2;
				case DxgiFormat.DxgiFormatD32FloatS8X24Uint:
					return 4;
				case DxgiFormat.DxgiFormatR32FloatX8X24Typeless:
					return 4;
				case DxgiFormat.DxgiFormatX32TypelessG8X24Uint:
					return 4;
				case DxgiFormat.DxgiFormatR10G10B10A2Typeless:
					return 4;
				case DxgiFormat.DxgiFormatR10G10B10A2Unorm:
					return 4;
				case DxgiFormat.DxgiFormatR10G10B10A2Uint:
					return 4;
				case DxgiFormat.DxgiFormatR11G11B10Float:
					return 4;
				case DxgiFormat.DxgiFormatR8G8B8A8Typeless:
					return 4;
				case DxgiFormat.DxgiFormatR8G8B8A8Unorm:
					return 4;
				case DxgiFormat.DxgiFormatR8G8B8A8UnormSrgb:
					return 4;
				case DxgiFormat.DxgiFormatR8G8B8A8Uint:
					return 4;
				case DxgiFormat.DxgiFormatR8G8B8A8Snorm:
					return 4;
				case DxgiFormat.DxgiFormatR8G8B8A8Sint:
					return 4;
				case DxgiFormat.DxgiFormatR16G16Typeless:
					return 4;
				case DxgiFormat.DxgiFormatR16G16Float:
					return 4;
				case DxgiFormat.DxgiFormatR16G16Unorm:
					return 4;
				case DxgiFormat.DxgiFormatR16G16Uint:
					return 4;
				case DxgiFormat.DxgiFormatR16G16Snorm:
					return 4;
				case DxgiFormat.DxgiFormatR16G16Sint:
					return 4;
				case DxgiFormat.DxgiFormatR32Typeless:
					return 4;
				case DxgiFormat.DxgiFormatD32Float:
					return 4;
				case DxgiFormat.DxgiFormatR32Float:
					return 4;
				case DxgiFormat.DxgiFormatR32Uint:
					return 4;
				case DxgiFormat.DxgiFormatR32Sint:
					return 4;
				case DxgiFormat.DxgiFormatR24G8Typeless:
					return 4;
				case DxgiFormat.DxgiFormatD24UnormS8Uint:
					return 4;
				case DxgiFormat.DxgiFormatR24UnormX8Typeless:
					return 4;
				case DxgiFormat.DxgiFormatX24TypelessG8Uint:
					return 4;
				case DxgiFormat.DxgiFormatR8G8Typeless:
					return 2;
				case DxgiFormat.DxgiFormatR8G8Unorm:
					return 2;
				case DxgiFormat.DxgiFormatR8G8Uint:
					return 2;
				case DxgiFormat.DxgiFormatR8G8Snorm:
					return 2;
				case DxgiFormat.DxgiFormatR8G8Sint:
					return 2;
				case DxgiFormat.DxgiFormatR16Typeless:
					return 2;
				case DxgiFormat.DxgiFormatR16Float:
					return 2;
				case DxgiFormat.DxgiFormatD16Unorm:
					return 2;
				case DxgiFormat.DxgiFormatR16Unorm:
					return 2;
				case DxgiFormat.DxgiFormatR16Uint:
					return 2;
				case DxgiFormat.DxgiFormatR16Snorm:
					return 2;
				case DxgiFormat.DxgiFormatR16Sint:
					return 2;
				case DxgiFormat.DxgiFormatR8Typeless:
					return 1;
				case DxgiFormat.DxgiFormatR8Unorm:
					return 1;
				case DxgiFormat.DxgiFormatR8Uint:
					return 1;
				case DxgiFormat.DxgiFormatR8Snorm:
					return 1;
				case DxgiFormat.DxgiFormatR8Sint:
					return 1;
				case DxgiFormat.DxgiFormatA8Unorm:
					return 1;
				case DxgiFormat.DxgiFormatR1Unorm:
					return 1;
				case DxgiFormat.DxgiFormatR9G9B9E5Sharedexp:
					return 4;
				case DxgiFormat.DxgiFormatR8G8B8G8Unorm:
					return 4;
				case DxgiFormat.DxgiFormatG8R8G8B8Unorm:
					return 4;
				case DxgiFormat.DxgiFormatBc1Typeless:
					return 8;
				case DxgiFormat.DxgiFormatBc1Unorm:
					return 8;
				case DxgiFormat.DxgiFormatBc1UnormSrgb:
					return 8;
				case DxgiFormat.DxgiFormatBc2Typeless:
					return 16;
				case DxgiFormat.DxgiFormatBc2Unorm:
					return 16;
				case DxgiFormat.DxgiFormatBc2UnormSrgb:
					return 16;
				case DxgiFormat.DxgiFormatBc3Typeless:
					return 16;
				case DxgiFormat.DxgiFormatBc3Unorm:
					return 16;
				case DxgiFormat.DxgiFormatBc3UnormSrgb:
					return 16;
				case DxgiFormat.DxgiFormatBc4Typeless:
					return 8;
				case DxgiFormat.DxgiFormatBc4Unorm:
					return 8;
				case DxgiFormat.DxgiFormatBc4Snorm:
					return 8;
				case DxgiFormat.DxgiFormatBc5Typeless:
					return 16;
				case DxgiFormat.DxgiFormatBc5Unorm:
					return 16;
				case DxgiFormat.DxgiFormatBc5Snorm:
					return 16;
				case DxgiFormat.DxgiFormatB5G6R5Unorm:
					return 2;
				case DxgiFormat.DxgiFormatB5G5R5A1Unorm:
					return 2;
				case DxgiFormat.DxgiFormatB8G8R8A8Unorm:
					return 4;
				case DxgiFormat.DxgiFormatB8G8R8X8Unorm:
					return 4;
				case DxgiFormat.DxgiFormatR10G10B10XrBiasA2Unorm:
					return 4;
				case DxgiFormat.DxgiFormatB8G8R8A8Typeless:
					return 4;
				case DxgiFormat.DxgiFormatB8G8R8A8UnormSrgb:
					return 4;
				case DxgiFormat.DxgiFormatB8G8R8X8Typeless:
					return 4;
				case DxgiFormat.DxgiFormatB8G8R8X8UnormSrgb:
					return 4;
				case DxgiFormat.DxgiFormatBc6HTypeless:
					return 16;
				case DxgiFormat.DxgiFormatBc6HUf16:
					return 16;
				case DxgiFormat.DxgiFormatBc6HSf16:
					return 16;
				case DxgiFormat.DxgiFormatBc7Typeless:
					return 16;
				case DxgiFormat.DxgiFormatBc7Unorm:
					return 16;
				case DxgiFormat.DxgiFormatBc7UnormSrgb:
					return 16;
				case DxgiFormat.DxgiFormatP8:
					return 1;
				case DxgiFormat.DxgiFormatA8P8:
					return 2;
				case DxgiFormat.DxgiFormatB4G4R4A4Unorm:
					return 2;
				case DxgiFormat.DxgiFormatAtcExt:
					return 8;
				case DxgiFormat.DxgiFormatAtcExplicitAlphaExt:
					return 16;
				case DxgiFormat.DxgiFormatAtcInterpolatedAlphaExt:
					return 16;
			}
			return 4;
		}

		public static bool IsCompressedFormat(this DxgiFormat format)
		{
			switch (format)
			{
				case DxgiFormat.DxgiFormatBc1Typeless:
				case DxgiFormat.DxgiFormatBc1Unorm:
				case DxgiFormat.DxgiFormatBc1UnormSrgb:
				case DxgiFormat.DxgiFormatBc2Typeless:
				case DxgiFormat.DxgiFormatBc2Unorm:
				case DxgiFormat.DxgiFormatBc2UnormSrgb:
				case DxgiFormat.DxgiFormatBc3Typeless:
				case DxgiFormat.DxgiFormatBc3Unorm:
				case DxgiFormat.DxgiFormatBc3UnormSrgb:
				case DxgiFormat.DxgiFormatBc4Typeless:
				case DxgiFormat.DxgiFormatBc4Unorm:
				case DxgiFormat.DxgiFormatBc4Snorm:
				case DxgiFormat.DxgiFormatBc5Typeless:
				case DxgiFormat.DxgiFormatBc5Unorm:
				case DxgiFormat.DxgiFormatBc5Snorm:
				case DxgiFormat.DxgiFormatBc6HTypeless:
				case DxgiFormat.DxgiFormatBc6HUf16:
				case DxgiFormat.DxgiFormatBc6HSf16:
				case DxgiFormat.DxgiFormatBc7Typeless:
				case DxgiFormat.DxgiFormatBc7Unorm:
				case DxgiFormat.DxgiFormatBc7UnormSrgb:
				case DxgiFormat.DxgiFormatAtcExt:
				case DxgiFormat.DxgiFormatAtcExplicitAlphaExt:
				case DxgiFormat.DxgiFormatAtcInterpolatedAlphaExt:
					return true;

				default:
					return false;
			}
		}

		public static DxgiFormat ToDxgiFormat(this CompressionFormat format) =>
			FormatMapping.TryGetValue(format, out var dxgiFormat) ? dxgiFormat : DxgiFormat.DxgiFormatUnknown;

		public static CompressionFormat ToCompressionFormat(this DxgiFormat format) =>
			FormatMappingReverse.TryGetValue(format, out var bcnFormat) ? bcnFormat : CompressionFormat.Unknown;

		private static Dictionary<CompressionFormat, DxgiFormat> FormatMapping { get; } =
			new() {
				// Raw formats
				{ CompressionFormat.R8, DxgiFormat.DxgiFormatR8Unorm },
				{ CompressionFormat.R8S, DxgiFormat.DxgiFormatR8Snorm },
				{ CompressionFormat.R8G8, DxgiFormat.DxgiFormatR8G8Unorm },
				{ CompressionFormat.R8G8S, DxgiFormat.DxgiFormatR8G8Snorm },
				{ CompressionFormat.Rgba32, DxgiFormat.DxgiFormatR8G8B8A8Unorm },
				{ CompressionFormat.Rgba32_sRGB, DxgiFormat.DxgiFormatR8G8B8A8UnormSrgb },
				{ CompressionFormat.Bgra32, DxgiFormat.DxgiFormatB8G8R8A8Unorm },
				{ CompressionFormat.Bgra32_sRGB, DxgiFormat.DxgiFormatB8G8R8A8UnormSrgb },
				{ CompressionFormat.R10G10B10A2, DxgiFormat.DxgiFormatR10G10B10A2Unorm },
				{ CompressionFormat.RgbaFloat, DxgiFormat.DxgiFormatR32G32B32A32Float },
				{ CompressionFormat.RgbaHalf, DxgiFormat.DxgiFormatR16G16B16A16Float },
				{ CompressionFormat.RgbFloat, DxgiFormat.DxgiFormatR32G32B32Float },

	            // RGB formats without direct DXGI equivalents
	            // { CompressionFormat.Rgb24, DxgiFormat.DxgiFormatUnknown },
	            // { CompressionFormat.Rgb24_sRGB, DxgiFormat.DxgiFormatUnknown },
	            // { CompressionFormat.Bgr24, DxgiFormat.DxgiFormatUnknown },
	            // { CompressionFormat.Bgr24_sRGB, DxgiFormat.DxgiFormatUnknown },
	            // { CompressionFormat.Rgbe, DxgiFormat.DxgiFormatUnknown },
	            // { CompressionFormat.Xyze, DxgiFormat.DxgiFormatUnknown },
	            // { CompressionFormat.RgbHalf, DxgiFormat.DxgiFormatUnknown },

				// BC formats
				{ CompressionFormat.Bc1, DxgiFormat.DxgiFormatBc1Unorm },
				{ CompressionFormat.Bc1_sRGB, DxgiFormat.DxgiFormatBc1UnormSrgb },
				{ CompressionFormat.Bc1WithAlpha, DxgiFormat.DxgiFormatBc1Unorm },
				{ CompressionFormat.Bc1WithAlpha_sRGB, DxgiFormat.DxgiFormatBc1UnormSrgb },
				{ CompressionFormat.Bc2, DxgiFormat.DxgiFormatBc2Unorm },
				{ CompressionFormat.Bc2_sRGB, DxgiFormat.DxgiFormatBc2UnormSrgb },
				{ CompressionFormat.Bc3, DxgiFormat.DxgiFormatBc3Unorm },
				{ CompressionFormat.Bc3_sRGB, DxgiFormat.DxgiFormatBc3UnormSrgb },
				{ CompressionFormat.Bc4, DxgiFormat.DxgiFormatBc4Unorm },
				{ CompressionFormat.Bc4S, DxgiFormat.DxgiFormatBc4Snorm },
				{ CompressionFormat.Bc5, DxgiFormat.DxgiFormatBc5Unorm },
				{ CompressionFormat.Bc5S, DxgiFormat.DxgiFormatBc5Snorm },
				{ CompressionFormat.Bc6U, DxgiFormat.DxgiFormatBc6HUf16 },
				{ CompressionFormat.Bc6S, DxgiFormat.DxgiFormatBc6HSf16 },
				{ CompressionFormat.Bc7, DxgiFormat.DxgiFormatBc7Unorm },
				{ CompressionFormat.Bc7_sRGB, DxgiFormat.DxgiFormatBc7UnormSrgb },

				// ATC formats
				{ CompressionFormat.Atc, DxgiFormat.DxgiFormatAtcExt },
				{ CompressionFormat.AtcExplicitAlpha, DxgiFormat.DxgiFormatAtcExplicitAlphaExt },
				{ CompressionFormat.AtcInterpolatedAlpha, DxgiFormat.DxgiFormatAtcInterpolatedAlphaExt }
			};

		private static Dictionary<DxgiFormat, CompressionFormat> FormatMappingReverse { get; } =
			FormatMapping.Where(x =>
				// Filter out Bc1A formats, which have no DXGI equivalent
				x.Key != CompressionFormat.Bc1WithAlpha &&
				x.Key != CompressionFormat.Bc1WithAlpha_sRGB
				).ToDictionary(x => x.Value, x => x.Key);
	}
}
