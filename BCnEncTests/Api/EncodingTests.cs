using System;
using System.Collections.Generic;
using System.IO;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.TextureFormats;
using BCnEncTests.Support;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests.Api;

public class EncoderTests
{
	private readonly ITestOutputHelper output;

	public EncoderTests(ITestOutputHelper output)
	{
		this.output = output;
	}

	public static string[] TestImages = ["blocks"];
	public static string[] RgTestImages = ["blocks", "rg"];

	public enum TestFileType
	{
		dds,
		ktx
	}

	public static IEnumerable<object[]> GetTestCases(TestFileType fileType)
	{
		ITextureFileFormat tex = fileType switch
		{
			TestFileType.dds => new DdsFile(),
			TestFileType.ktx => new KtxFile(),
		};

		foreach (CompressionFormat format in Enum.GetValues<CompressionFormat>())
		{
			string[] images = TestImages;

			if (tex.IsSupportedFormat(format))
			{
				foreach (string image in images)
				{
					if (format.IsRawPixelFormat())
					{
						yield return new object[] { fileType, image, format, CompressionQuality.Fast, tex };
					}
					else
					{
						yield return new object[] { fileType, image, format, CompressionQuality.Fast, tex };
						yield return new object[] { fileType, image, format, CompressionQuality.Balanced, tex };
						yield return new object[] { fileType, image, format, CompressionQuality.BestQuality, tex };
					}
				}
			}
		}
	}


	[Theory]
	[MemberData(nameof(GetTestCases), TestFileType.dds)]
	public void TestEncoding<TTexture>(TestFileType fileType, string testImage, CompressionFormat format, CompressionQuality quality, TTexture _)
		where TTexture : class, ITextureFileFormat<TTexture>, new()
	{
		var outFileName = $"test_enc_{fileType.ToString()}_{testImage}_{format}_{quality}.{fileType.ToString()}";
		var rawFile = ImageLoader.TestRawImages[testImage];

		TestHelper.TestEncodingLdr<TTexture>(rawFile, outFileName, format, quality, output);
	}
}
