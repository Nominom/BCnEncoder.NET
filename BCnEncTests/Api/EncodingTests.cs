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

	public static (string, TextureType)[] TestImages = [
		("rgb_hard", TextureType.Albedo)
	];
	public static string[] RgTestImages = ["blocks", "rg"];

	public static CompressionFormat[] OverrideFormats =
	[
		CompressionFormat.Bc1
	];

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

		IEnumerable<CompressionFormat> formats = OverrideFormats.Length > 0 ? OverrideFormats : Enum.GetValues<CompressionFormat>();

		foreach (CompressionFormat format in formats)
		{
			(string, TextureType)[] images = TestImages;

			if (tex.IsSupportedFormat(format))
			{
				foreach ((string fileName, TextureType texType) image in images)
				{
					if (format.IsRawPixelFormat())
					{
						yield return new object[] { fileType, image.fileName, image.texType, format, CompressionQuality.Fast, tex };
					}
					else
					{
						yield return new object[] { fileType, image.fileName, image.texType, format, CompressionQuality.Fast, tex };
						yield return new object[] { fileType, image.fileName, image.texType, format, CompressionQuality.Balanced, tex };
						yield return new object[] { fileType, image.fileName, image.texType, format, CompressionQuality.BestQuality, tex };
					}
				}
			}
		}
	}


	[Theory]
	[MemberData(nameof(GetTestCases), TestFileType.dds)]
	public void TestEncoding<TTexture>(TestFileType fileType, string testImage, TextureType texType, CompressionFormat format, CompressionQuality quality, TTexture _)
		where TTexture : class, ITextureFileFormat<TTexture>, new()
	{
		var outFileName = $"test_enc_{fileType.ToString()}_{testImage}_{format}_{quality}.{fileType.ToString()}";
		var rawFile = ImageLoader.TestRawImages[testImage];

		TestHelper.TestEncodingLdr<TTexture>(rawFile, texType, outFileName, format, quality, output);
	}
}
