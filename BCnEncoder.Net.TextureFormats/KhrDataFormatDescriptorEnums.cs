using System;
using System.Collections.Generic;
using System.Text;

namespace BCnEncoder.TextureFormats
{
	/* The Khronos Data Format Specification (version 1.1) */
	/*
	** Copyright (c) 2015 The Khronos Group Inc.
	**
	** Permission is hereby granted, free of charge, to any person obtaining a
	** copy of this software and/or associated documentation files (the
	** "Materials"), to deal in the Materials without restriction, including
	** without limitation the rights to use, copy, modify, merge, publish,
	** distribute, sublicense, and/or sell copies of the Materials, and to
	** permit persons to whom the Materials are furnished to do so, subject to
	** the following conditions:
	**
	** The above copyright notice and this permission notice shall be included
	** in all copies or substantial portions of the Materials.
	**
	** THE MATERIALS ARE PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
	** EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
	** MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
	** IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
	** CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
	** TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
	** MATERIALS OR THE USE OR OTHER DEALINGS IN THE MATERIALS.
	*/

	/* This header defines a structure that can describe the layout of image
	   formats in memory. This means that the data format is transparent to
	   the application, and the expectation is that this should be used when
	   the layout is defined external to the API. Many Khronos APIs deliberately
	   keep the internal layout of images opaque, to allow proprietary layouts
	   and optimisations. This structure is not appropriate for describing
	   opaque layouts. */

	/* Vendor ids */
	public enum KhrDfVendorId : uint
	{
		/* Standard Khronos descriptor */
		KhrDfVendoridKhronos = 0U,
		KhrDfVendoridMax = 0xFFFFU
	}


	/* Descriptor types */
	public enum KhrDfKhrDescriptorType : uint
	{
		/* Default Khronos descriptor */
		KhrDfKhrDescriptortypeBasicformat = 0U,
		KhrDfKhrDescriptortypeMax = 0xFFFFU
	}


	/* Model in which the color coordinate space is defined.
   There is no requirement that a color format use all the
   channel types that are defined in the color model. */
	public enum KhrDfModel : uint
	{
		/* No interpretation of color channels defined */
		KhrDfModelUnspecified = 0U,

		/* Color primaries (red, green, blue) + alpha, depth and stencil */
		KhrDfModelRgbsda = 1U,

		/* Color differences (Y', Cb, Cr) + alpha, depth and stencil */
		KhrDfModelYuvsda = 2U,

		/* Color differences (Y', I, Q) + alpha, depth and stencil */
		KhrDfModelYiqsda = 3U,

		/* Perceptual color (CIE L*a*b*) + alpha, depth and stencil */
		KhrDfModelLabsda = 4U,

		/* Subtractive colors (cyan, magenta, yellow, black) + alpha */
		KhrDfModelCmyka = 5U,

		/* Non-color coordinate data (X, Y, Z, W) */
		KhrDfModelXyzw = 6U,

		/* Hue, saturation, value, hue angle on color circle, plus alpha */
		KhrDfModelHsvaAng = 7U,

		/* Hue, saturation, lightness, hue angle on color circle, plus alpha */
		KhrDfModelHslaAng = 8U,

		/* Hue, saturation, value, hue on color hexagon, plus alpha */
		KhrDfModelHsvaHex = 7U,

		/* Hue, saturation, lightness, hue on color hexagon, plus alpha */
		KhrDfModelHslaHex = 8U,

		/* Lightweight approximate color difference (luma, orange, green) */
		KhrDfModelYcgcoa = 9U,

		/* Compressed formats start at 128. */
		/* These compressed formats should generally have a single sample,
		   sited at the 0,0 position of the texel block. Where multiple
		   channels are used to distinguish formats, these should be cosited. */
		/* Direct3D (and S3) compressed formats */
		/* Note that premultiplied status is recorded separately */
		/* DXT1 "channels" are RGB (0), Alpha (1) */
		/* DXT1/BC1 with one channel is opaque */
		/* DXT1/BC1 with a cosited alpha sample is transparent */
		KhrDfModelDxt1A = 128U,
		KhrDfModelBc1A = 128U,

		/* DXT2/DXT3/BC2, with explicit 4-bit alpha */
		KhrDfModelDxt2 = 129U,
		KhrDfModelDxt3 = 129U,
		KhrDfModelBc2 = 129U,

		/* DXT4/DXT5/BC3, with interpolated alpha */
		KhrDfModelDxt4 = 130U,
		KhrDfModelDxt5 = 130U,
		KhrDfModelBc3 = 130U,

		/* BC4 - single channel interpolated 8-bit data */
		/* (The UNORM/SNORM variation is recorded in the channel data) */
		KhrDfModelBc4 = 131U,

		/* BC5 - two channel interpolated 8-bit data */
		/* (The UNORM/SNORM variation is recorded in the channel data) */
		KhrDfModelBc5 = 132U,

		/* BC6H - DX11 format for 16-bit float channels */
		KhrDfModelBc6H = 133U,

		/* BC7 - DX11 format */
		KhrDfModelBc7 = 134U,
		/* Gap left for future desktop expansion */

		/* Mobile compressed formats follow */
		/* A format of ETC1 indicates that the format shall be decodable
		   by an ETC1-compliant decoder and not rely on ETC2 features */
		KhrDfModeltc1 = 160U,

		/* A format of ETC2 is permitted to use ETC2 encodings on top of
		   the baseline ETC1 specification */
		/* The ETC2 format has channels "red", "green", "RGB" and "alpha",
		   which should be cosited samples */
		/* Punch-through alpha can be distinguished from full alpha by
		   the plane size in bytes required for the texel block */
		KhrDfModeltc2 = 161U,

		/* Adaptive Scalable Texture Compression */
		/* ASTC HDR vs LDR is determined by the float flag in the channel */
		/* ASTC block size can be distinguished by texel block size */
		KhrDfModelAstc = 162U,

		/* Proprietary formats (PVRTC, ATITC, etc.) should follow */
		KhrDfModelMax = 0xFFU
	}

	/* Definition of channel names for each color model */
	public enum KhrDfModelChannels : uint
	{
		/* Unspecified format with nominal channel numbering */
		KhrDfChannelUnspecified0 = 0U,
		KhrDfChannelUnspecified1 = 1U,
		KhrDfChannelUnspecified2 = 2U,
		KhrDfChannelUnspecified3 = 3U,
		KhrDfChannelUnspecified4 = 4U,
		KhrDfChannelUnspecified5 = 5U,
		KhrDfChannelUnspecified6 = 6U,
		KhrDfChannelUnspecified7 = 7U,
		KhrDfChannelUnspecified8 = 8U,
		KhrDfChannelUnspecified9 = 9U,
		KhrDfChannelUnspecified10 = 10U,
		KhrDfChannelUnspecified11 = 11U,
		KhrDfChannelUnspecified12 = 12U,
		KhrDfChannelUnspecified13 = 13U,
		KhrDfChannelUnspecified14 = 14U,
		KhrDfChannelUnspecified15 = 15U,

		/* MODEL_RGBSDA - red, green, blue, stencil, depth, alpha */
		KhrDfChannelRgbsdaRed = 0U,
		KhrDfChannelRgbsdaR = 0U,
		KhrDfChannelRgbsdaGreen = 1U,
		KhrDfChannelRgbsdaG = 1U,
		KhrDfChannelRgbsdaBlue = 2U,
		KhrDfChannelRgbsdaB = 2U,
		KhrDfChannelRgbsdaStencil = 13U,
		KhrDfChannelRgbsdaS = 13U,
		KhrDfChannelRgbsdaDepth = 14U,
		KhrDfChannelRgbsdaD = 14U,
		KhrDfChannelRgbsdaAlpha = 15U,
		KhrDfChannelRgbsdaA = 15U,

		/* MODEL_YUVSDA - luma, Cb, Cr, stencil, depth, alpha */
		KhrDfChannelYuvsdaY = 0U,
		KhrDfChannelYuvsdaCb = 1U,
		KhrDfChannelYuvsdaU = 1U,
		KhrDfChannelYuvsdaCr = 2U,
		KhrDfChannelYuvsdaV = 2U,
		KhrDfChannelYuvsdaStencil = 13U,
		KhrDfChannelYuvsdaS = 13U,
		KhrDfChannelYuvsdaDepth = 14U,
		KhrDfChannelYuvsdaD = 14U,
		KhrDfChannelYuvsdaAlpha = 15U,
		KhrDfChannelYuvsdaA = 15U,

		/* MODEL_YIQSDA - luma, in-phase, quadrature, stencil, depth, alpha */
		KhrDfChannelYiqsdaY = 0U,
		KhrDfChannelYiqsdaI = 1U,
		KhrDfChannelYiqsdaQ = 2U,
		KhrDfChannelYiqsdaStencil = 13U,
		KhrDfChannelYiqsdaS = 13U,
		KhrDfChannelYiqsdaDepth = 14U,
		KhrDfChannelYiqsdaD = 14U,
		KhrDfChannelYiqsdaAlpha = 15U,
		KhrDfChannelYiqsdaA = 15U,

		/* MODEL_LABSDA - CIELAB/L*a*b* luma, red-green, blue-yellow, stencil, depth, alpha */
		KhrDfChannelLabsdaL = 0U,
		KhrDfChannelLabsdaA = 1U,
		KhrDfChannelLabsdaB = 2U,
		KhrDfChannelLabsdaStencil = 13U,
		KhrDfChannelLabsdaS = 13U,
		KhrDfChannelLabsdaDepth = 14U,
		KhrDfChannelLabsdaD = 14U,
		KhrDfChannelLabsdaAlpha = 15U,

		/* NOTE: KHR_DF_CHANNEL_LABSDA_A is not a synonym for alpha! */
		/* MODEL_CMYKA - cyan, magenta, yellow, key/blacK, alpha */
		KhrDfChannelCmyksdaCyan = 0U,
		KhrDfChannelCmyksdaC = 0U,
		KhrDfChannelCmyksdaMagenta = 1U,
		KhrDfChannelCmyksdaM = 1U,
		KhrDfChannelCmyksdaYellow = 2U,
		KhrDfChannelCmyksdaY = 2U,
		KhrDfChannelCmyksdaKey = 3U,
		KhrDfChannelCmyksdaBlack = 3U,
		KhrDfChannelCmyksdaK = 3U,
		KhrDfChannelCmyksdaAlpha = 15U,
		KhrDfChannelCmyksdaA = 15U,

		/* MODEL_XYZW - coordinates x, y, z, w */
		KhrDfChannelXyzwX = 0U,
		KhrDfChannelXyzwY = 1U,
		KhrDfChannelXyzwZ = 2U,
		KhrDfChannelXyzwW = 3U,

		/* MODEL_HSVA_ANG - value (luma), saturation, hue, alpha, angular projection, conical space */
		KhrDfChannelHsvaAngValue = 0U,
		KhrDfChannelHsvaAngV = 0U,
		KhrDfChannelHsvaAngSaturation = 1U,
		KhrDfChannelHsvaAngS = 1U,
		KhrDfChannelHsvaAngHue = 2U,
		KhrDfChannelHsvaAngH = 2U,
		KhrDfChannelHsvaAngAlpha = 15U,
		KhrDfChannelHsvaAngA = 15U,

		/* MODEL_HSLA_ANG - lightness (luma), saturation, hue, alpha, angular projection, double conical space */
		KhrDfChannelHslaAngLightness = 0U,
		KhrDfChannelHslaAngL = 0U,
		KhrDfChannelHslaAngSaturation = 1U,
		KhrDfChannelHslaAngS = 1U,
		KhrDfChannelHslaAngHue = 2U,
		KhrDfChannelHslaAngH = 2U,
		KhrDfChannelHslaAngAlpha = 15U,
		KhrDfChannelHslaAngA = 15U,

		/* MODEL_HSVA_HEX - value (luma), saturation, hue, alpha, hexagonal projection, conical space */
		KhrDfChannelHsvaHexValue = 0U,
		KhrDfChannelHsvaHexV = 0U,
		KhrDfChannelHsvaHexSaturation = 1U,
		KhrDfChannelHsvaHexS = 1U,
		KhrDfChannelHsvaHexHue = 2U,
		KhrDfChannelHsvaHexH = 2U,
		KhrDfChannelHsvaHexAlpha = 15U,
		KhrDfChannelHsvaHexA = 15U,

		/* MODEL_HSLA_HEX - lightness (luma), saturation, hue, alpha, hexagonal projection, double conical space */
		KhrDfChannelHslaHexLightness = 0U,
		KhrDfChannelHslaHexL = 0U,
		KhrDfChannelHslaHexSaturation = 1U,
		KhrDfChannelHslaHexS = 1U,
		KhrDfChannelHslaHexHue = 2U,
		KhrDfChannelHslaHexH = 2U,
		KhrDfChannelHslaHexAlpha = 15U,
		KhrDfChannelHslaHexA = 15U,

		/* MODEL_YCGCOA - luma, green delta, orange delta, alpha */
		KhrDfChannelYcgcoaY = 0U,
		KhrDfChannelYcgcoaCg = 1U,
		KhrDfChannelYcgcoaCo = 2U,
		KhrDfChannelYcgcoaAlpha = 15U,
		KhrDfChannelYcgcoaA = 15U,

		/* Compressed formats */
		/* MODEL_DXT1A/MODEL_BC1A */
		KhrDfChannelDxt1AColor = 0U,
		KhrDfChannelBc1AColor = 0U,
		KhrDfChannelDxt1AAlphapresent = 1U,
		KhrDfChannelBc1AAlphapresent = 1U,

		/* MODEL_DXT2/3/MODEL_BC2 */
		KhrDfChannelDxt2Color = 0U,
		KhrDfChannelDxt3Color = 0U,
		KhrDfChannelBc2Color = 0U,
		KhrDfChannelDxt2Alpha = 15U,
		KhrDfChannelDxt3Alpha = 15U,
		KhrDfChannelBc2Alpha = 15U,

		/* MODEL_DXT4/5/MODEL_BC3 */
		KhrDfChannelDxt4Color = 0U,
		KhrDfChannelDxt5Color = 0U,
		KhrDfChannelBc3Color = 0U,
		KhrDfChannelDxt4Alpha = 15U,
		KhrDfChannelDxt5Alpha = 15U,
		KhrDfChannelBc3Alpha = 15U,

		/* MODEL_BC4 */
		KhrDfChannelBc4Data = 0U,

		/* MODEL_BC5 */
		KhrDfChannelBc5Red = 0U,
		KhrDfChannelBc5R = 0U,
		KhrDfChannelBc5Green = 1U,
		KhrDfChannelBc5G = 1U,

		/* MODEL_BC6H */
		KhrDfChannelBc6HColor = 0U,

		/* MODEL_BC7 */
		KhrDfChannelBc7Data = 0U,

		/* MODELTC1 */
		KhrDfChanneltc1Data = 0U,
		KhrDfChanneltc1Color = 0U,

		/* MODELTC2 */
		KhrDfChanneltc2Red = 0U,
		KhrDfChanneltc2R = 0U,
		KhrDfChanneltc2Green = 1U,
		KhrDfChanneltc2G = 1U,
		KhrDfChanneltc2Color = 2U,
		KhrDfChanneltc2Alpha = 15U,
		KhrDfChanneltc2A = 15U,

		/* MODEL_ASTC */
		KhrDfChannelAstcData = 0U,

		/* Common channel names shared by multiple formats */
		KhrDfChannelCommonLuma = 0U,
		KhrDfChannelCommonL = 0U,
		KhrDfChannelCommonStencil = 13U,
		KhrDfChannelCommonS = 13U,
		KhrDfChannelCommonDepth = 14U,
		KhrDfChannelCommonD = 14U,
		KhrDfChannelCommonAlpha = 15U,
		KhrDfChannelCommonA = 15U
	}


	/* Definition of the primary colors in color coordinates.
   This is implicitly responsible for defining the conversion
   between RGB an YUV color spaces.
   LAB and related absolute color models should use
   KHR_DF_PRIMARIES_CIEXYZ. */
	public enum KhrDfPrimaries : uint
	{
		/* No color primaries defined */
		KhrDfPrimariesUnspecified = 0U,

		/* Color primaries of ITU-R BT.709 and sRGB */
		KhrDfPrimariesBt709 = 1U,

		/* Synonym for KHR_DF_PRIMARIES_BT709 */
		KhrDfPrimariesSrgb = 1U,

		/* Color primaries of ITU-R BT.601 (625-line EBU variant) */
		KhrDfPrimariesBt601Bu = 2U,

		/* Color primaries of ITU-R BT.601 (525-line SMPTE C variant) */
		KhrDfPrimariesBt601Smpte = 3U,

		/* Color primaries of ITU-R BT.2020 */
		KhrDfPrimariesBt2020 = 4U,

		/* CIE theoretical color coordinate space */
		KhrDfPrimariesCiexyz = 5U,

		/* Academy Color Encoding System primaries */
		KhrDfPrimariesAces = 6U,
		KhrDfPrimariesMax = 0xFFU
	}


	/* Definition of the optical to digital transfer function
   ("gamma correction"). Most transfer functions are not a pure
   power function and also include a linear element.
   LAB and related absolute color representations should use
   KHR_DF_TRANSFER_UNSPECIFIED. */
	public enum KhrDfTransfer : uint
	{
		/* No transfer function defined */
		KhrDfTransferUnspecified = 0U,

		/* Linear transfer function (value proportional to intensity) */
		KhrDfTransferLinear = 1U,

		/* Perceptually-linear transfer function of sRGH (~2.4) */
		KhrDfTransferSrgb = 2U,

		/* Perceptually-linear transfer function of ITU specifications (~1/.45) */
		KhrDfTransferItu = 3U,

		/* Perceptually-linear gamma function of NTSC (simple 2.2 gamma) */
		KhrDfTransferNtsc = 4U,

		/* Sony S-log used by Sony video cameras */
		KhrDfTransferSlog = 5U,

		/* Sony S-log 2 used by Sony video cameras */
		KhrDfTransferSlog2 = 6U,
		KhrDfTransferMax = 0xFFU
	}

	public enum KhrDfFlags : uint
	{
		KhrDfFlagAlphaStraight = 0U,
		KhrDfFlagAlphaPremultiplied = 1U
	}

	[Flags]
	public enum KhrDfSampleDatatypeQualifiers : uint
	{
		KhrDfSampleDatatypeLinear = 1U << 4,
		KhrDfSampleDatatypexponent = 1U << 5,
		KhrDfSampleDatatypeSigned = 1U << 6,
		KhrDfSampleDatatypeFloat = 1U << 7
	}
}
