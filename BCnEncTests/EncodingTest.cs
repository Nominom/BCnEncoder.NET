using System;
using System.IO;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.TextureFormats;
using BCnEncTests.Support;
using SixLabors.ImageSharp;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests
{

	public class EncoderTests
	{
		private readonly ITestOutputHelper output;

		public EncoderTests(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Theory]
		// Bc1
		[InlineData("diffuse_1", CompressionFormat.Bc1, CompressionQuality.Fast)]
		[InlineData("diffuse_1", CompressionFormat.Bc1, CompressionQuality.Balanced)]
		[InlineData("diffuse_1", CompressionFormat.Bc1, CompressionQuality.BestQuality)]
		[InlineData("diffuse_2", CompressionFormat.Bc1, CompressionQuality.Fast)]
		[InlineData("diffuse_2", CompressionFormat.Bc1, CompressionQuality.Balanced)]
		[InlineData("diffuse_2", CompressionFormat.Bc1, CompressionQuality.BestQuality)]
		//[InlineData("diffuse_3", CompressionFormat.Bc1, CompressionQuality.Fast)]
		//[InlineData("diffuse_3", CompressionFormat.Bc1, CompressionQuality.Balanced)]
		//[InlineData("diffuse_3", CompressionFormat.Bc1, CompressionQuality.BestQuality)]
		[InlineData("diffuse_4", CompressionFormat.Bc1, CompressionQuality.Fast)]
		[InlineData("diffuse_4", CompressionFormat.Bc1, CompressionQuality.Balanced)]
		[InlineData("diffuse_4", CompressionFormat.Bc1, CompressionQuality.BestQuality)]
		[InlineData("diffuse_5", CompressionFormat.Bc1, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.Bc1, CompressionQuality.Balanced)]
		[InlineData("diffuse_5", CompressionFormat.Bc1, CompressionQuality.BestQuality)]
		[InlineData("diffuse_6", CompressionFormat.Bc1, CompressionQuality.Fast)]
		[InlineData("diffuse_6", CompressionFormat.Bc1, CompressionQuality.Balanced)]
		[InlineData("diffuse_6", CompressionFormat.Bc1, CompressionQuality.BestQuality)]
		// Bc7
		[InlineData("diffuse_1", CompressionFormat.Bc7, CompressionQuality.Fast)]
		[InlineData("diffuse_1", CompressionFormat.Bc7, CompressionQuality.Balanced)]
		[InlineData("diffuse_1", CompressionFormat.Bc7, CompressionQuality.BestQuality)]
		[InlineData("diffuse_2", CompressionFormat.Bc7, CompressionQuality.Fast)]
		[InlineData("diffuse_2", CompressionFormat.Bc7, CompressionQuality.Balanced)]
		[InlineData("diffuse_2", CompressionFormat.Bc7, CompressionQuality.BestQuality)]
		[InlineData("diffuse_3", CompressionFormat.Bc7, CompressionQuality.Fast)]
		[InlineData("diffuse_3", CompressionFormat.Bc7, CompressionQuality.Balanced)]
		[InlineData("diffuse_3", CompressionFormat.Bc7, CompressionQuality.BestQuality)]
		[InlineData("diffuse_4", CompressionFormat.Bc7, CompressionQuality.Fast)]
		[InlineData("diffuse_4", CompressionFormat.Bc7, CompressionQuality.Balanced)]
		[InlineData("diffuse_4", CompressionFormat.Bc7, CompressionQuality.BestQuality)]
		[InlineData("diffuse_5", CompressionFormat.Bc7, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.Bc7, CompressionQuality.Balanced)]
		[InlineData("diffuse_5", CompressionFormat.Bc7, CompressionQuality.BestQuality)]
		[InlineData("diffuse_6", CompressionFormat.Bc7, CompressionQuality.Fast)]
		[InlineData("diffuse_6", CompressionFormat.Bc7, CompressionQuality.Balanced)]
		[InlineData("diffuse_6", CompressionFormat.Bc7, CompressionQuality.BestQuality)]


		[InlineData("height_1", CompressionFormat.R8, CompressionQuality.Fast)]
		[InlineData("rg_1", CompressionFormat.R8G8, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.Rgba32, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.Bgra32, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.RgbaHalf, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.RgbaFloat, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.RgbFloat, CompressionQuality.Fast)]
		[InlineData("diffuse_3", CompressionFormat.Rgba32, CompressionQuality.Fast)]
		[InlineData("diffuse_3", CompressionFormat.Bgra32, CompressionQuality.Fast)]
		[InlineData("diffuse_3", CompressionFormat.RgbaHalf, CompressionQuality.Fast)]
		[InlineData("diffuse_3", CompressionFormat.RgbaFloat, CompressionQuality.Fast)]
		[InlineData("diffuse_3", CompressionFormat.RgbFloat, CompressionQuality.Fast)]
		public void TestEncodingDds(string testImage, CompressionFormat format, CompressionQuality quality)
		{
			var outFileName = $"test_enc_dds_{testImage}_{format}_{quality}.dds";
			var rawFile = ImageLoader.TestRawImages[testImage];
			TestHelper.TestEncodingLdr<DdsFile>(rawFile, outFileName, format, quality, output);
		}

		[Theory]
		// Bc1
		[InlineData("diffuse_1", CompressionFormat.Bc1, CompressionQuality.Fast)]
		[InlineData("diffuse_2", CompressionFormat.Bc1, CompressionQuality.Fast)]
		//[InlineData("diffuse_3", CompressionFormat.Bc1, CompressionQuality.Fast)]
		[InlineData("diffuse_4", CompressionFormat.Bc1, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.Bc1, CompressionQuality.Fast)]

		[InlineData("height_1", CompressionFormat.R8, CompressionQuality.Fast)]
		[InlineData("rg_1", CompressionFormat.R8G8, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.Rgb24, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.Rgba32, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.Bgra32, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.RgbaHalf, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.RgbaFloat, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.RgbHalf, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.RgbFloat, CompressionQuality.Fast)]
		public void TestEncodingKtx(string testImage, CompressionFormat format, CompressionQuality quality)
		{
			var outFileName = $"test_enc_ktx_{testImage}_{format}_{quality}.ktx";
			var rawFile = ImageLoader.TestRawImages[testImage];
			TestHelper.TestEncodingLdr<KtxFile>(rawFile, outFileName, format, quality, output);
		}

		[Theory]
		// Rgbe
		[InlineData("diffuse_1", CompressionFormat.Rgbe32, CompressionQuality.Fast)]
		[InlineData("diffuse_2", CompressionFormat.Rgbe32, CompressionQuality.Fast)]
		[InlineData("diffuse_3", CompressionFormat.Rgbe32, CompressionQuality.Fast)]
		[InlineData("diffuse_4", CompressionFormat.Rgbe32, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.Rgbe32, CompressionQuality.Fast)]
		// Xyze
		[InlineData("diffuse_1", CompressionFormat.Xyze32, CompressionQuality.Fast)]
		[InlineData("diffuse_2", CompressionFormat.Xyze32, CompressionQuality.Fast)]
		[InlineData("diffuse_3", CompressionFormat.Xyze32, CompressionQuality.Fast)]
		[InlineData("diffuse_4", CompressionFormat.Xyze32, CompressionQuality.Fast)]
		[InlineData("diffuse_5", CompressionFormat.Xyze32, CompressionQuality.Fast)]
		public void TestEncodingRgbe(string testImage, CompressionFormat format, CompressionQuality quality)
		{
			var outFileName = $"test_enc_hdr_{testImage}_{format}_{quality}.hdr";
			var rawFile = ImageLoader.TestRawImages[testImage];
			TestHelper.TestEncodingLdr<RadianceFile>(rawFile, outFileName, format, quality, output);
		}

		[Fact]
		public void TestImagesLoaded()
		{
			foreach (var readThrownException in ImageLoader.ReadThrownExceptions)
			{
				throw new AggregateException($"Failed to load: {readThrownException.Item1}", readThrownException.Item2);
			}
		}
	}
	public class CubemapTest
	{
		[Fact]
		public void WriteCubeMapFile()
		{
			var images = ImageLoader.TestCubemap;

			var filename = "test_enc_bc1_cubemap.ktx";

			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc1;

			using var fs = File.OpenWrite(filename);
			encoder.EncodeCubeMapToStream<KtxFile>(images[0], images[1], images[2], images[3], images[4], images[5], fs);
		}

		[Fact]
		public void WriteCubeMapFileDds()
		{
			var images = ImageLoader.TestCubemap2;

			var filename = "test_enc_bc1_cubemap.dds";

			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc1;

			using var fs = File.OpenWrite(filename);
			encoder.EncodeCubeMapToStream<DdsFile>(images[0], images[1], images[2], images[3], images[4], images[5], fs);
		}
	}
}
