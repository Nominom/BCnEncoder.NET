using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.NET.ImageSharp;
using BCnEncoder.Shared;
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
			var inputImage = ImageLoader.TestGradient1;
			var decoder = new BcDecoder();
			var encoder = new BcEncoder
			{
				OutputOptions = { Quality = CompressionQuality.BestQuality }
			};

			var encodedRawBytes = encoder.EncodeToRawBytes(inputImage);
			var decodedImage = decoder.DecodeRawToImageRgba32(encodedRawBytes[0], inputImage.Width, inputImage.Height, CompressionFormat.Bc1);

			ImageLoader.TestGradient1.TryGetSinglePixelSpan(out var originalPixels);
			decodedImage.TryGetSinglePixelSpan(out var decodedPixels);

			TestHelper.AssertPixelsEqual(originalPixels, decodedPixels, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void EncodeDecodeStream()
		{
			var inputImage = ImageLoader.TestGradient1;
			var decoder = new BcDecoder();
			var encoder = new BcEncoder
			{
				OutputOptions = { Quality = CompressionQuality.BestQuality }
			};

			var encodedRawBytes = encoder.EncodeToRawBytes(inputImage);

			using var ms = new MemoryStream(encodedRawBytes[0]);

			Assert.Equal(0, ms.Position);

			var decodedImage = decoder.DecodeRawToImageRgba32(ms, inputImage.Width, inputImage.Height, CompressionFormat.Bc1);

			inputImage.TryGetSinglePixelSpan(out var originalPixels);
			decodedImage.TryGetSinglePixelSpan(out var decodedPixels);

			TestHelper.AssertPixelsEqual(originalPixels, decodedPixels, encoder.OutputOptions.Quality);
		}


		[Fact]
		public void EncodeDecodeAllMipMapsStream()
		{
			var inputImage = ImageLoader.TestGradient1;
			var decoder = new BcDecoder();
			var encoder = new BcEncoder
			{
				OutputOptions =
				{
					Quality = CompressionQuality.BestQuality,
					GenerateMipMaps = true,
					MaxMipMapLevel = 0
				}
			};

			using var ms = new MemoryStream();

			var encodedRawBytes = encoder.EncodeToRawBytes(inputImage);

			var mipLevels = encoder.CalculateNumberOfMipLevels(inputImage);
			Assert.True(mipLevels > 1);

			for (var i = 0; i < mipLevels; i++)
			{
				ms.Write(encodedRawBytes[i]);
			}

			ms.Position = 0;
			Assert.Equal(0, ms.Position);

			for (var i = 0; i < mipLevels; i++)
			{
				encoder.CalculateMipMapSize(inputImage, i, out var mipWidth, out var mipHeight);
				using var resized = inputImage.Clone(x => x.Resize(mipWidth, mipHeight));

				var decodedImage = decoder.DecodeRawToImageRgba32(ms, mipWidth, mipHeight, CompressionFormat.Bc1);
				resized.TryGetSinglePixelSpan(out var originalPixels);
				decodedImage.TryGetSinglePixelSpan(out var decodedPixels);

				TestHelper.AssertPixelsEqual(originalPixels, decodedPixels, encoder.OutputOptions.Quality);
			}

			encoder.CalculateMipMapSize(inputImage, mipLevels - 1, out var lastMWidth, out var lastMHeight);
			Assert.Equal(1, lastMWidth);
			Assert.Equal(1, lastMHeight);
		}
	}
}
