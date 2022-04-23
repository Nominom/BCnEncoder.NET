using System;
using System.Collections.Generic;
using System.Text;

namespace BCnEncoder.TextureFormats
{
	// From https://www.khronos.org/registry/vulkan/specs/1.2-extensions/html/vkspec.html#formats-definition
	public enum VkFormat : UInt32
	{
		VkFormatUndefined = 0,
		VkFormatR4G4UnormPack8 = 1,
		VkFormatR4G4B4A4UnormPack16 = 2,
		VkFormatB4G4R4A4UnormPack16 = 3,
		VkFormatR5G6B5UnormPack16 = 4,
		VkFormatB5G6R5UnormPack16 = 5,
		VkFormatR5G5B5A1UnormPack16 = 6,
		VkFormatB5G5R5A1UnormPack16 = 7,
		VkFormatA1R5G5B5UnormPack16 = 8,
		VkFormatR8Unorm = 9,
		VkFormatR8Snorm = 10,
		VkFormatR8Uscaled = 11,
		VkFormatR8Sscaled = 12,
		VkFormatR8Uint = 13,
		VkFormatR8Sint = 14,
		VkFormatR8Srgb = 15,
		VkFormatR8G8Unorm = 16,
		VkFormatR8G8Snorm = 17,
		VkFormatR8G8Uscaled = 18,
		VkFormatR8G8Sscaled = 19,
		VkFormatR8G8Uint = 20,
		VkFormatR8G8Sint = 21,
		VkFormatR8G8Srgb = 22,
		VkFormatR8G8B8Unorm = 23,
		VkFormatR8G8B8Snorm = 24,
		VkFormatR8G8B8Uscaled = 25,
		VkFormatR8G8B8Sscaled = 26,
		VkFormatR8G8B8Uint = 27,
		VkFormatR8G8B8Sint = 28,
		VkFormatR8G8B8Srgb = 29,
		VkFormatB8G8R8Unorm = 30,
		VkFormatB8G8R8Snorm = 31,
		VkFormatB8G8R8Uscaled = 32,
		VkFormatB8G8R8Sscaled = 33,
		VkFormatB8G8R8Uint = 34,
		VkFormatB8G8R8Sint = 35,
		VkFormatB8G8R8Srgb = 36,
		VkFormatR8G8B8A8Unorm = 37,
		VkFormatR8G8B8A8Snorm = 38,
		VkFormatR8G8B8A8Uscaled = 39,
		VkFormatR8G8B8A8Sscaled = 40,
		VkFormatR8G8B8A8Uint = 41,
		VkFormatR8G8B8A8Sint = 42,
		VkFormatR8G8B8A8Srgb = 43,
		VkFormatB8G8R8A8Unorm = 44,
		VkFormatB8G8R8A8Snorm = 45,
		VkFormatB8G8R8A8Uscaled = 46,
		VkFormatB8G8R8A8Sscaled = 47,
		VkFormatB8G8R8A8Uint = 48,
		VkFormatB8G8R8A8Sint = 49,
		VkFormatB8G8R8A8Srgb = 50,
		VkFormatA8B8G8R8UnormPack32 = 51,
		VkFormatA8B8G8R8SnormPack32 = 52,
		VkFormatA8B8G8R8UscaledPack32 = 53,
		VkFormatA8B8G8R8SscaledPack32 = 54,
		VkFormatA8B8G8R8UintPack32 = 55,
		VkFormatA8B8G8R8SintPack32 = 56,
		VkFormatA8B8G8R8SrgbPack32 = 57,
		VkFormatA2R10G10B10UnormPack32 = 58,
		VkFormatA2R10G10B10SnormPack32 = 59,
		VkFormatA2R10G10B10UscaledPack32 = 60,
		VkFormatA2R10G10B10SscaledPack32 = 61,
		VkFormatA2R10G10B10UintPack32 = 62,
		VkFormatA2R10G10B10SintPack32 = 63,
		VkFormatA2B10G10R10UnormPack32 = 64,
		VkFormatA2B10G10R10SnormPack32 = 65,
		VkFormatA2B10G10R10UscaledPack32 = 66,
		VkFormatA2B10G10R10SscaledPack32 = 67,
		VkFormatA2B10G10R10UintPack32 = 68,
		VkFormatA2B10G10R10SintPack32 = 69,
		VkFormatR16Unorm = 70,
		VkFormatR16Snorm = 71,
		VkFormatR16Uscaled = 72,
		VkFormatR16Sscaled = 73,
		VkFormatR16Uint = 74,
		VkFormatR16Sint = 75,
		VkFormatR16Sfloat = 76,
		VkFormatR16G16Unorm = 77,
		VkFormatR16G16Snorm = 78,
		VkFormatR16G16Uscaled = 79,
		VkFormatR16G16Sscaled = 80,
		VkFormatR16G16Uint = 81,
		VkFormatR16G16Sint = 82,
		VkFormatR16G16Sfloat = 83,
		VkFormatR16G16B16Unorm = 84,
		VkFormatR16G16B16Snorm = 85,
		VkFormatR16G16B16Uscaled = 86,
		VkFormatR16G16B16Sscaled = 87,
		VkFormatR16G16B16Uint = 88,
		VkFormatR16G16B16Sint = 89,
		VkFormatR16G16B16Sfloat = 90,
		VkFormatR16G16B16A16Unorm = 91,
		VkFormatR16G16B16A16Snorm = 92,
		VkFormatR16G16B16A16Uscaled = 93,
		VkFormatR16G16B16A16Sscaled = 94,
		VkFormatR16G16B16A16Uint = 95,
		VkFormatR16G16B16A16Sint = 96,
		VkFormatR16G16B16A16Sfloat = 97,
		VkFormatR32Uint = 98,
		VkFormatR32Sint = 99,
		VkFormatR32Sfloat = 100,
		VkFormatR32G32Uint = 101,
		VkFormatR32G32Sint = 102,
		VkFormatR32G32Sfloat = 103,
		VkFormatR32G32B32Uint = 104,
		VkFormatR32G32B32Sint = 105,
		VkFormatR32G32B32Sfloat = 106,
		VkFormatR32G32B32A32Uint = 107,
		VkFormatR32G32B32A32Sint = 108,
		VkFormatR32G32B32A32Sfloat = 109,
		VkFormatR64Uint = 110,
		VkFormatR64Sint = 111,
		VkFormatR64Sfloat = 112,
		VkFormatR64G64Uint = 113,
		VkFormatR64G64Sint = 114,
		VkFormatR64G64Sfloat = 115,
		VkFormatR64G64B64Uint = 116,
		VkFormatR64G64B64Sint = 117,
		VkFormatR64G64B64Sfloat = 118,
		VkFormatR64G64B64A64Uint = 119,
		VkFormatR64G64B64A64Sint = 120,
		VkFormatR64G64B64A64Sfloat = 121,
		VkFormatB10G11R11UfloatPack32 = 122,
		VkFormatE5B9G9R9UfloatPack32 = 123,
		VkFormatD16Unorm = 124,
		VkFormatX8D24UnormPack32 = 125,
		VkFormatD32Sfloat = 126,
		VkFormatS8Uint = 127,
		VkFormatD16UnormS8Uint = 128,
		VkFormatD24UnormS8Uint = 129,
		VkFormatD32SfloatS8Uint = 130,
		VkFormatBc1RgbUnormBlock = 131,
		VkFormatBc1RgbSrgbBlock = 132,
		VkFormatBc1RgbaUnormBlock = 133,
		VkFormatBc1RgbaSrgbBlock = 134,
		VkFormatBc2UnormBlock = 135,
		VkFormatBc2SrgbBlock = 136,
		VkFormatBc3UnormBlock = 137,
		VkFormatBc3SrgbBlock = 138,
		VkFormatBc4UnormBlock = 139,
		VkFormatBc4SnormBlock = 140,
		VkFormatBc5UnormBlock = 141,
		VkFormatBc5SnormBlock = 142,
		VkFormatBc6HUfloatBlock = 143,
		VkFormatBc6HSfloatBlock = 144,
		VkFormatBc7UnormBlock = 145,
		VkFormatBc7SrgbBlock = 146,
		VkFormatEtc2R8G8B8UnormBlock = 147,
		VkFormatEtc2R8G8B8SrgbBlock = 148,
		VkFormatEtc2R8G8B8A1UnormBlock = 149,
		VkFormatEtc2R8G8B8A1SrgbBlock = 150,
		VkFormatEtc2R8G8B8A8UnormBlock = 151,
		VkFormatEtc2R8G8B8A8SrgbBlock = 152,
		VkFormatEacR11UnormBlock = 153,
		VkFormatEacR11SnormBlock = 154,
		VkFormatEacR11G11UnormBlock = 155,
		VkFormatEacR11G11SnormBlock = 156,
		VkFormatAstc4X4UnormBlock = 157,
		VkFormatAstc4X4SrgbBlock = 158,
		VkFormatAstc5X4UnormBlock = 159,
		VkFormatAstc5X4SrgbBlock = 160,
		VkFormatAstc5X5UnormBlock = 161,
		VkFormatAstc5X5SrgbBlock = 162,
		VkFormatAstc6X5UnormBlock = 163,
		VkFormatAstc6X5SrgbBlock = 164,
		VkFormatAstc6X6UnormBlock = 165,
		VkFormatAstc6X6SrgbBlock = 166,
		VkFormatAstc8X5UnormBlock = 167,
		VkFormatAstc8X5SrgbBlock = 168,
		VkFormatAstc8X6UnormBlock = 169,
		VkFormatAstc8X6SrgbBlock = 170,
		VkFormatAstc8X8UnormBlock = 171,
		VkFormatAstc8X8SrgbBlock = 172,
		VkFormatAstc10X5UnormBlock = 173,
		VkFormatAstc10X5SrgbBlock = 174,
		VkFormatAstc10X6UnormBlock = 175,
		VkFormatAstc10X6SrgbBlock = 176,
		VkFormatAstc10X8UnormBlock = 177,
		VkFormatAstc10X8SrgbBlock = 178,
		VkFormatAstc10X10UnormBlock = 179,
		VkFormatAstc10X10SrgbBlock = 180,
		VkFormatAstc12X10UnormBlock = 181,
		VkFormatAstc12X10SrgbBlock = 182,
		VkFormatAstc12X12UnormBlock = 183,
		VkFormatAstc12X12SrgbBlock = 184,
		// Provided by VK_VERSION_1_1
		VkFormatG8B8G8R8422Unorm = 1000156000,
		// Provided by VK_VERSION_1_1
		VkFormatB8G8R8G8422Unorm = 1000156001,
		// Provided by VK_VERSION_1_1
		VkFormatG8B8R83Plane420Unorm = 1000156002,
		// Provided by VK_VERSION_1_1
		VkFormatG8B8R82Plane420Unorm = 1000156003,
		// Provided by VK_VERSION_1_1
		VkFormatG8B8R83Plane422Unorm = 1000156004,
		// Provided by VK_VERSION_1_1
		VkFormatG8B8R82Plane422Unorm = 1000156005,
		// Provided by VK_VERSION_1_1
		VkFormatG8B8R83Plane444Unorm = 1000156006,
		// Provided by VK_VERSION_1_1
		VkFormatR10X6UnormPack16 = 1000156007,
		// Provided by VK_VERSION_1_1
		VkFormatR10X6G10X6Unorm2Pack16 = 1000156008,
		// Provided by VK_VERSION_1_1
		VkFormatR10X6G10X6B10X6A10X6Unorm4Pack16 = 1000156009,
		// Provided by VK_VERSION_1_1
		VkFormatG10X6B10X6G10X6R10X6422Unorm4Pack16 = 1000156010,
		// Provided by VK_VERSION_1_1
		VkFormatB10X6G10X6R10X6G10X6422Unorm4Pack16 = 1000156011,
		// Provided by VK_VERSION_1_1
		VkFormatG10X6B10X6R10X63Plane420Unorm3Pack16 = 1000156012,
		// Provided by VK_VERSION_1_1
		VkFormatG10X6B10X6R10X62Plane420Unorm3Pack16 = 1000156013,
		// Provided by VK_VERSION_1_1
		VkFormatG10X6B10X6R10X63Plane422Unorm3Pack16 = 1000156014,
		// Provided by VK_VERSION_1_1
		VkFormatG10X6B10X6R10X62Plane422Unorm3Pack16 = 1000156015,
		// Provided by VK_VERSION_1_1
		VkFormatG10X6B10X6R10X63Plane444Unorm3Pack16 = 1000156016,
		// Provided by VK_VERSION_1_1
		VkFormatR12X4UnormPack16 = 1000156017,
		// Provided by VK_VERSION_1_1
		VkFormatR12X4G12X4Unorm2Pack16 = 1000156018,
		// Provided by VK_VERSION_1_1
		VkFormatR12X4G12X4B12X4A12X4Unorm4Pack16 = 1000156019,
		// Provided by VK_VERSION_1_1
		VkFormatG12X4B12X4G12X4R12X4422Unorm4Pack16 = 1000156020,
		// Provided by VK_VERSION_1_1
		VkFormatB12X4G12X4R12X4G12X4422Unorm4Pack16 = 1000156021,
		// Provided by VK_VERSION_1_1
		VkFormatG12X4B12X4R12X43Plane420Unorm3Pack16 = 1000156022,
		// Provided by VK_VERSION_1_1
		VkFormatG12X4B12X4R12X42Plane420Unorm3Pack16 = 1000156023,
		// Provided by VK_VERSION_1_1
		VkFormatG12X4B12X4R12X43Plane422Unorm3Pack16 = 1000156024,
		// Provided by VK_VERSION_1_1
		VkFormatG12X4B12X4R12X42Plane422Unorm3Pack16 = 1000156025,
		// Provided by VK_VERSION_1_1
		VkFormatG12X4B12X4R12X43Plane444Unorm3Pack16 = 1000156026,
		// Provided by VK_VERSION_1_1
		VkFormatG16B16G16R16422Unorm = 1000156027,
		// Provided by VK_VERSION_1_1
		VkFormatB16G16R16G16422Unorm = 1000156028,
		// Provided by VK_VERSION_1_1
		VkFormatG16B16R163Plane420Unorm = 1000156029,
		// Provided by VK_VERSION_1_1
		VkFormatG16B16R162Plane420Unorm = 1000156030,
		// Provided by VK_VERSION_1_1
		VkFormatG16B16R163Plane422Unorm = 1000156031,
		// Provided by VK_VERSION_1_1
		VkFormatG16B16R162Plane422Unorm = 1000156032,
		// Provided by VK_VERSION_1_1
		VkFormatG16B16R163Plane444Unorm = 1000156033,
		VkFormatG8B8R82Plane444Unorm = 1000330000,
		VkFormatG10X6B10X6R10X62Plane444Unorm3Pack16 = 1000330001,
		VkFormatG12X4B12X4R12X42Plane444Unorm3Pack16 = 1000330002,
		VkFormatG16B16R162Plane444Unorm = 1000330003,
		VkFormatA4R4G4B4UnormPack16 = 1000340000,
		VkFormatA4B4G4R4UnormPack16 = 1000340001,
		VkFormatAstc4X4SfloatBlock = 1000066000,
		VkFormatAstc5X4SfloatBlock = 1000066001,
		VkFormatAstc5X5SfloatBlock = 1000066002,
		VkFormatAstc6X5SfloatBlock = 1000066003,
		VkFormatAstc6X6SfloatBlock = 1000066004,
		VkFormatAstc8X5SfloatBlock = 1000066005,
		VkFormatAstc8X6SfloatBlock = 1000066006,
		VkFormatAstc8X8SfloatBlock = 1000066007,
		VkFormatAstc10X5SfloatBlock = 1000066008,
		VkFormatAstc10X6SfloatBlock = 1000066009,
		VkFormatAstc10X8SfloatBlock = 1000066010,
		VkFormatAstc10X10SfloatBlock = 1000066011,
		VkFormatAstc12X10SfloatBlock = 1000066012,
		VkFormatAstc12X12SfloatBlock = 1000066013,
		// Provided by VK_IMG_format_pvrtc
		VkFormatPvrtc12BppUnormBlockImg = 1000054000,
		// Provided by VK_IMG_format_pvrtc
		VkFormatPvrtc14BppUnormBlockImg = 1000054001,
		// Provided by VK_IMG_format_pvrtc
		VkFormatPvrtc22BppUnormBlockImg = 1000054002,
		// Provided by VK_IMG_format_pvrtc
		VkFormatPvrtc24BppUnormBlockImg = 1000054003,
		// Provided by VK_IMG_format_pvrtc
		VkFormatPvrtc12BppSrgbBlockImg = 1000054004,
		// Provided by VK_IMG_format_pvrtc
		VkFormatPvrtc14BppSrgbBlockImg = 1000054005,
		// Provided by VK_IMG_format_pvrtc
		VkFormatPvrtc22BppSrgbBlockImg = 1000054006,
		// Provided by VK_IMG_format_pvrtc
		VkFormatPvrtc24BppSrgbBlockImg = 1000054007,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc4X4SfloatBlockExt = VkFormatAstc4X4SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc5X4SfloatBlockExt = VkFormatAstc5X4SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc5X5SfloatBlockExt = VkFormatAstc5X5SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc6X5SfloatBlockExt = VkFormatAstc6X5SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc6X6SfloatBlockExt = VkFormatAstc6X6SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc8X5SfloatBlockExt = VkFormatAstc8X5SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc8X6SfloatBlockExt = VkFormatAstc8X6SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc8X8SfloatBlockExt = VkFormatAstc8X8SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc10X5SfloatBlockExt = VkFormatAstc10X5SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc10X6SfloatBlockExt = VkFormatAstc10X6SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc10X8SfloatBlockExt = VkFormatAstc10X8SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc10X10SfloatBlockExt = VkFormatAstc10X10SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc12X10SfloatBlockExt = VkFormatAstc12X10SfloatBlock,
		// Provided by VK_EXT_texture_compression_astc_hdr
		VkFormatAstc12X12SfloatBlockExt = VkFormatAstc12X12SfloatBlock,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG8B8G8R8422UnormKhr = VkFormatG8B8G8R8422Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatB8G8R8G8422UnormKhr = VkFormatB8G8R8G8422Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG8B8R83Plane420UnormKhr = VkFormatG8B8R83Plane420Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG8B8R82Plane420UnormKhr = VkFormatG8B8R82Plane420Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG8B8R83Plane422UnormKhr = VkFormatG8B8R83Plane422Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG8B8R82Plane422UnormKhr = VkFormatG8B8R82Plane422Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG8B8R83Plane444UnormKhr = VkFormatG8B8R83Plane444Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatR10X6UnormPack16Khr = VkFormatR10X6UnormPack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatR10X6G10X6Unorm2Pack16Khr = VkFormatR10X6G10X6Unorm2Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatR10X6G10X6B10X6A10X6Unorm4Pack16Khr = VkFormatR10X6G10X6B10X6A10X6Unorm4Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG10X6B10X6G10X6R10X6422Unorm4Pack16Khr = VkFormatG10X6B10X6G10X6R10X6422Unorm4Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatB10X6G10X6R10X6G10X6422Unorm4Pack16Khr = VkFormatB10X6G10X6R10X6G10X6422Unorm4Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG10X6B10X6R10X63Plane420Unorm3Pack16Khr = VkFormatG10X6B10X6R10X63Plane420Unorm3Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG10X6B10X6R10X62Plane420Unorm3Pack16Khr = VkFormatG10X6B10X6R10X62Plane420Unorm3Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG10X6B10X6R10X63Plane422Unorm3Pack16Khr = VkFormatG10X6B10X6R10X63Plane422Unorm3Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG10X6B10X6R10X62Plane422Unorm3Pack16Khr = VkFormatG10X6B10X6R10X62Plane422Unorm3Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG10X6B10X6R10X63Plane444Unorm3Pack16Khr = VkFormatG10X6B10X6R10X63Plane444Unorm3Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatR12X4UnormPack16Khr = VkFormatR12X4UnormPack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatR12X4G12X4Unorm2Pack16Khr = VkFormatR12X4G12X4Unorm2Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatR12X4G12X4B12X4A12X4Unorm4Pack16Khr = VkFormatR12X4G12X4B12X4A12X4Unorm4Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG12X4B12X4G12X4R12X4422Unorm4Pack16Khr = VkFormatG12X4B12X4G12X4R12X4422Unorm4Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatB12X4G12X4R12X4G12X4422Unorm4Pack16Khr = VkFormatB12X4G12X4R12X4G12X4422Unorm4Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG12X4B12X4R12X43Plane420Unorm3Pack16Khr = VkFormatG12X4B12X4R12X43Plane420Unorm3Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG12X4B12X4R12X42Plane420Unorm3Pack16Khr = VkFormatG12X4B12X4R12X42Plane420Unorm3Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG12X4B12X4R12X43Plane422Unorm3Pack16Khr = VkFormatG12X4B12X4R12X43Plane422Unorm3Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG12X4B12X4R12X42Plane422Unorm3Pack16Khr = VkFormatG12X4B12X4R12X42Plane422Unorm3Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG12X4B12X4R12X43Plane444Unorm3Pack16Khr = VkFormatG12X4B12X4R12X43Plane444Unorm3Pack16,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG16B16G16R16422UnormKhr = VkFormatG16B16G16R16422Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatB16G16R16G16422UnormKhr = VkFormatB16G16R16G16422Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG16B16R163Plane420UnormKhr = VkFormatG16B16R163Plane420Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG16B16R162Plane420UnormKhr = VkFormatG16B16R162Plane420Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG16B16R163Plane422UnormKhr = VkFormatG16B16R163Plane422Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG16B16R162Plane422UnormKhr = VkFormatG16B16R162Plane422Unorm,
		// Provided by VK_KHR_sampler_ycbcr_conversion
		VkFormatG16B16R163Plane444UnormKhr = VkFormatG16B16R163Plane444Unorm,
		// Provided by VK_EXT_ycbcr_2plane_444_formats
		VkFormatG8B8R82Plane444UnormExt = VkFormatG8B8R82Plane444Unorm,
		// Provided by VK_EXT_ycbcr_2plane_444_formats
		VkFormatG10X6B10X6R10X62Plane444Unorm3Pack16Ext = VkFormatG10X6B10X6R10X62Plane444Unorm3Pack16,
		// Provided by VK_EXT_ycbcr_2plane_444_formats
		VkFormatG12X4B12X4R12X42Plane444Unorm3Pack16Ext = VkFormatG12X4B12X4R12X42Plane444Unorm3Pack16,
		// Provided by VK_EXT_ycbcr_2plane_444_formats
		VkFormatG16B16R162Plane444UnormExt = VkFormatG16B16R162Plane444Unorm,
		// Provided by VK_EXT_4444_formats
		VkFormatA4R4G4B4UnormPack16Ext = VkFormatA4R4G4B4UnormPack16,
		// Provided by VK_EXT_4444_formats
		VkFormatA4B4G4R4UnormPack16Ext = VkFormatA4B4G4R4UnormPack16,
	}
}
