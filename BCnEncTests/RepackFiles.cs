using System.IO;
using BCnEncoder.TextureFormats;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests;

public class RepackFiles
{
	[Theory]
	[InlineData("testktx/raw-bc1-rgb.ktx", "testktx/raw-bc1-rgb-linear.ktx", GlInternalFormat.GlCompressedRgbS3TcDxt1Ext)]
	[InlineData("testktx/raw-bc1-rgb.ktx", "testktx/raw-bc1-rgb-srgb.ktx", GlInternalFormat.GlCompressedSrgbS3TcDxt1Ext)]
	[InlineData("testktx/raw-bc1-rgba.ktx", "testktx/raw-bc1-rgba-linear.ktx", GlInternalFormat.GlCompressedRgbaS3TcDxt1Ext)]
	[InlineData("testktx/raw-bc1-rgba.ktx", "testktx/raw-bc1-rgba-srgb.ktx", GlInternalFormat.GlCompressedSrgbAlphaS3TcDxt1Ext)]
	[InlineData("testktx/raw-bc2.ktx", "testktx/raw-bc2-linear.ktx", GlInternalFormat.GlCompressedRgbaS3TcDxt3Ext)]
	[InlineData("testktx/raw-bc2.ktx", "testktx/raw-bc2-srgb.ktx", GlInternalFormat.GlCompressedSrgbAlphaS3TcDxt3Ext)]
	[InlineData("testktx/raw-bc3.ktx", "testktx/raw-bc3-linear.ktx", GlInternalFormat.GlCompressedRgbaS3TcDxt5Ext)]
	[InlineData("testktx/raw-bc3.ktx", "testktx/raw-bc3-srgb.ktx", GlInternalFormat.GlCompressedSrgbAlphaS3TcDxt5Ext)]
	[InlineData("testktx/raw-bc7.ktx", "testktx/raw-bc7-linear.ktx", GlInternalFormat.GlCompressedRgbaBptcUnormArb)]
	[InlineData("testktx/raw-bc7.ktx", "testktx/raw-bc7-srgb.ktx", GlInternalFormat.GlCompressedSrgbAlphaBptcUnormArb)]
	[InlineData("testktx/raw-etc1.ktx", "testktx/raw-etc1-linear.ktx", GlInternalFormat.GlEtc1Rgb8Oes)]
	[InlineData("testktx/raw-etc2-rgb.ktx", "testktx/raw-etc2-rgb-linear.ktx", GlInternalFormat.GlCompressedRgb8Etc2)]
	[InlineData("testktx/raw-etc2-rgb.ktx", "testktx/raw-etc2-rgb-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Etc2)]
	[InlineData("testktx/raw-etc2-rgb-a1.ktx", "testktx/raw-etc2-rgb-a1-linear.ktx", GlInternalFormat.GlCompressedRgb8PunchthroughAlpha1Etc2)]
	[InlineData("testktx/raw-etc2-rgb-a1.ktx", "testktx/raw-etc2-rgb-a1-srgb.ktx", GlInternalFormat.GlCompressedSrgb8PunchthroughAlpha1Etc2)]
	[InlineData("testktx/raw-etc2-rgba.ktx", "testktx/raw-etc2-rgba-linear.ktx", GlInternalFormat.GlCompressedRgba8Etc2Eac)]
	[InlineData("testktx/raw-etc2-rgba.ktx", "testktx/raw-etc2-rgba-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Etc2Eac)]
	[InlineData("testktx/raw-astc-4x4.ktx", "testktx/raw-astc-4x4-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc4X4Khr)]
	[InlineData("testktx/raw-astc-4x4.ktx", "testktx/raw-astc-4x4-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc4X4Khr)]

	[InlineData("testktx/raw-astc-4x4.ktx", "testktx/raw-astc-4x4-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc4X4Khr)]
	[InlineData("testktx/raw-astc-4x4.ktx", "testktx/raw-astc-4x4-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc4X4Khr)]

	[InlineData("testktx/raw-astc-5x4.ktx", "testktx/raw-astc-5x4-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc5X4Khr)]
	[InlineData("testktx/raw-astc-5x4.ktx", "testktx/raw-astc-5x4-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc5X4Khr)]
	[InlineData("testktx/raw-astc-5x5.ktx", "testktx/raw-astc-5x5-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc5X5Khr)]
	[InlineData("testktx/raw-astc-5x5.ktx", "testktx/raw-astc-5x5-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc5X5Khr)]

	[InlineData("testktx/raw-astc-6x5.ktx", "testktx/raw-astc-6x5-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc6X5Khr)]
	[InlineData("testktx/raw-astc-6x5.ktx", "testktx/raw-astc-6x5-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc6X5Khr)]
	[InlineData("testktx/raw-astc-6x6.ktx", "testktx/raw-astc-6x6-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc6X6Khr)]
	[InlineData("testktx/raw-astc-6x6.ktx", "testktx/raw-astc-6x6-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc6X6Khr)]

	[InlineData("testktx/raw-astc-8x5.ktx", "testktx/raw-astc-8x5-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc8X5Khr)]
	[InlineData("testktx/raw-astc-8x5.ktx", "testktx/raw-astc-8x5-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc8X5Khr)]
	[InlineData("testktx/raw-astc-8x6.ktx", "testktx/raw-astc-8x6-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc8X6Khr)]
	[InlineData("testktx/raw-astc-8x6.ktx", "testktx/raw-astc-8x6-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc8X6Khr)]
	[InlineData("testktx/raw-astc-8x8.ktx", "testktx/raw-astc-8x8-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc8X8Khr)]
	[InlineData("testktx/raw-astc-8x8.ktx", "testktx/raw-astc-8x8-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc8X8Khr)]

	[InlineData("testktx/raw-astc-10x5.ktx", "testktx/raw-astc-10x5-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc10X5Khr)]
	[InlineData("testktx/raw-astc-10x5.ktx", "testktx/raw-astc-10x5-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc10X5Khr)]
	[InlineData("testktx/raw-astc-10x6.ktx", "testktx/raw-astc-10x6-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc10X6Khr)]
	[InlineData("testktx/raw-astc-10x6.ktx", "testktx/raw-astc-10x6-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc10X6Khr)]
	[InlineData("testktx/raw-astc-10x8.ktx", "testktx/raw-astc-10x8-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc10X8Khr)]
	[InlineData("testktx/raw-astc-10x8.ktx", "testktx/raw-astc-10x8-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc10X8Khr)]
	[InlineData("testktx/raw-astc-10x10.ktx", "testktx/raw-astc-10x10-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc10X10Khr)]
	[InlineData("testktx/raw-astc-10x10.ktx", "testktx/raw-astc-10x10-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc10X10Khr)]

	[InlineData("testktx/raw-astc-12x10.ktx", "testktx/raw-astc-12x10-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc12X10Khr)]
	[InlineData("testktx/raw-astc-12x10.ktx", "testktx/raw-astc-12x10-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc12X10Khr)]
	[InlineData("testktx/raw-astc-12x12.ktx", "testktx/raw-astc-12x12-linear.ktx", GlInternalFormat.GlCompressedRgbaAstc12X12Khr)]
	[InlineData("testktx/raw-astc-12x12.ktx", "testktx/raw-astc-12x12-srgb.ktx", GlInternalFormat.GlCompressedSrgb8Alpha8Astc12X12Khr)]

	[InlineData("testktx/raw-r8g8b8.ktx", "testktx/raw-r8g8b8-linear.ktx", GlInternalFormat.GlRgb8)]
	[InlineData("testktx/raw-r8g8b8.ktx", "testktx/raw-r8g8b8-srgb.ktx", GlInternalFormat.GlSrgb8)]
	[InlineData("testktx/raw-r8g8b8a8.ktx", "testktx/raw-r8g8b8a8-linear.ktx", GlInternalFormat.GlRgba8)]
	[InlineData("testktx/raw-r8g8b8a8.ktx", "testktx/raw-r8g8b8a8-srgb.ktx", GlInternalFormat.GlSrgb8Alpha8)]
	public void RepackKtx(string input, string output, GlInternalFormat newFormat)
	{
		string iPath = Path.Combine(TestHelper.GetProjectRoot(), "testImages", input);
		string oPath = Path.Combine(TestHelper.GetProjectRoot(), "testImages", output);

		KtxFile ktx;
		using (var ifs = File.OpenRead(iPath))
		{
			ktx = KtxFile.Load(ifs);
		}
		ktx.header.GlInternalFormat = newFormat;

		using var ofs = File.OpenWrite(oPath);
		ktx.WriteToStream(ofs);
	}

	[Theory]
	[InlineData("testdds/raw-bc1-srgb.dds", "testdds/raw-bc1-linear.dds", DxgiFormat.DxgiFormatBc1Unorm)]
	public void RepackDds(string input, string output, DxgiFormat newFormat)
	{
		string iPath = Path.Combine(TestHelper.GetProjectRoot(), "testImages", input);
		string oPath = Path.Combine(TestHelper.GetProjectRoot(), "testImages", output);

		using var ifs = File.OpenRead(iPath);
		var dds = DdsFile.Load(ifs);
		dds.dx10Header.dxgiFormat = newFormat;

		using var ofs = File.OpenWrite(oPath);
		dds.WriteToStream(ofs);
	}
}
