namespace BCnEncoder.Shared
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
		GlUnsignedShort565Rev = 33636

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
}
