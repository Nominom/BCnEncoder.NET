using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using BCnEncTests.Support;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace BCnEncTests
{
	public class RawTests
	{
		[Fact]
		public void EncodeDecode()
		{
			var inputImage = ImageLoader.TestRawImages["diffuse_1"];
			var decoder = new BcDecoder();
			var encoder = new BcEncoder
			{
				OutputOptions =
				{
					Format  = CompressionFormat.Bc1,
					Quality = CompressionQuality.BestQuality
				}
			};

			var encodedRawBytes = encoder.EncodeToRawBytes(inputImage, 0, out _, out _);
			var decodedImage = decoder.DecodeRaw<ColorRgba32>(encodedRawBytes, inputImage.Width, inputImage.Height, CompressionFormat.Bc1, CompressionFormat.Rgba32_sRGB);

			var originalPixels = TestHelper.GetSinglePixelArray(inputImage);
			var decodedPixels  = TestHelper.GetSinglePixelArray(decodedImage.AsBCnTextureData(inputImage.Width, inputImage.Height, true).AsImageRgba32());

			TestHelper.AssertPixelsSimilar(originalPixels, decodedPixels, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void EncodeDecodeStream()
		{
			var inputImage = ImageLoader.TestRawImages["diffuse_2"];
			var decoder = new BcDecoder();
			var encoder = new BcEncoder
			{
				OutputOptions =
				{
					Format  = CompressionFormat.Bc1,
					Quality = CompressionQuality.BestQuality
				}
			};

			var encodedRawBytes = encoder.EncodeToRawBytes(inputImage, 0, out _, out _);

			using var ms = new MemoryStream(encodedRawBytes);

			Assert.Equal(0, ms.Position);

			var decodedImage = decoder.DecodeRaw<ColorRgba32>(ms, inputImage.Width, inputImage.Height, CompressionFormat.Bc1, CompressionFormat.Rgba32_sRGB);

			var originalPixels = TestHelper.GetSinglePixelArray(inputImage);
			var decodedPixels = TestHelper.GetSinglePixelArray(decodedImage.AsBCnTextureData(inputImage.Width, inputImage.Height, true).AsImageRgba32());

			TestHelper.AssertPixelsSimilar(originalPixels, decodedPixels, encoder.OutputOptions.Quality);
		}
	}
}
