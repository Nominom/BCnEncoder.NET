using System;
using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using CommunityToolkit.HighPerformance;
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
			var decodedPixels = decoder.DecodeRaw(encodedRawBytes[0], inputImage.Width, inputImage.Height, CompressionFormat.Bc1);
			var decodedImage = new Memory2D<ColorRgba32>(decodedPixels, inputImage.Height, inputImage.Width);

			var originalColors = TestHelper.GetSinglePixelArrayAsColors(inputImage);
			var decodedColors  = TestHelper.GetSinglePixelArrayAsColors(decodedImage);

			TestHelper.AssertPixelsEqual(originalColors, decodedColors, encoder.OutputOptions.Quality);
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

			var rawPixels = decoder.DecodeRaw(ms, inputImage.Width, inputImage.Height, CompressionFormat.Bc1);
			var decodedImage = new Memory2D<ColorRgba32>(rawPixels, inputImage.Height, inputImage.Width);

			var originalColors = TestHelper.GetSinglePixelArrayAsColors(inputImage);
			var decodedColors  = TestHelper.GetSinglePixelArrayAsColors(decodedImage);

			TestHelper.AssertPixelsEqual(originalColors, decodedColors, encoder.OutputOptions.Quality);
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

			var mipLevels = encoder.CalculateNumberOfMipLevels(inputImage.Width, inputImage.Height);
			Assert.True(mipLevels > 1);

			for (var i = 0; i < mipLevels; i++)
			{
				ms.Write(encodedRawBytes[i], 0, encodedRawBytes[i].Length);
			}

			ms.Position = 0;
			Assert.Equal(0, ms.Position);
			var resized = ResizeImageToMips(inputImage);


			for (var i = 0; i < mipLevels; i++)
			{
				encoder.CalculateMipMapSize(inputImage.Width, inputImage.Height, i, out var mipWidth, out var mipHeight);

				var blockSize = decoder.GetBlockSize(CompressionFormat.Bc1);
				var blockCount = decoder.GetBlockCount(mipWidth, mipHeight);
				var buffer = new byte[blockSize * blockCount];
				ms.Read(buffer, 0, buffer.Length);

				var rawPixels = decoder.DecodeRaw(buffer, mipWidth, mipHeight, CompressionFormat.Bc1);
				var decodedImage = new Memory2D<ColorRgba32>(rawPixels, mipHeight, mipWidth);

				var originalColors = TestHelper.GetSinglePixelArrayAsColors(resized[i]);
				var decodedColors  = TestHelper.GetSinglePixelArrayAsColors(decodedImage);

				TestHelper.AssertPixelsEqual(originalColors, decodedColors, encoder.OutputOptions.Quality);
			}

			encoder.CalculateMipMapSize(inputImage.Width, inputImage.Height, mipLevels - 1, out var lastMWidth, out var lastMHeight);
			Assert.Equal(1, lastMWidth);
			Assert.Equal(1, lastMHeight);
		}

		private static ReadOnlyMemory2D<ColorRgba32>[] ResizeImageToMips(Memory2D<ColorRgba32> image)
		{
			int numMips = 0;
			var chain = MipMapper.GenerateMipChain(image, ref numMips);
			return chain;
		}
	}
}
