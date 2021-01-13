using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
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
			var encoder = new BcEncoder()
			{
				OutputOptions = { Quality = CompressionQuality.BestQuality }
			};

			var encodedRawBytes = encoder.EncodeToRawBytes(inputImage);
			var decodedImage = decoder.DecodeRaw(encodedRawBytes[0], CompressionFormat.Bc1, inputImage.Width, inputImage.Height);

			ImageLoader.TestGradient1.TryGetSinglePixelSpan(out var originalPixels);
			decodedImage.TryGetSinglePixelSpan(out var decodedPixels);

			var psnr = ImageQuality.PeakSignalToNoiseRatio(originalPixels, decodedPixels);

			Assert.True(psnr > 30);
		}

		[Fact]
		public void EncodeDecodeStream()
		{
			var inputImage = ImageLoader.TestGradient1;
			var decoder = new BcDecoder();
			var encoder = new BcEncoder()
			{
				OutputOptions = { Quality = CompressionQuality.BestQuality }
			};

			var encodedRawBytes = encoder.EncodeToRawBytes(inputImage);

			using MemoryStream ms = new MemoryStream(encodedRawBytes[0]);

			Assert.Equal(0, ms.Position);

			var decodedImage = decoder.DecodeRaw(ms, CompressionFormat.Bc1, inputImage.Width, inputImage.Height);

			inputImage.TryGetSinglePixelSpan(out var originalPixels);
			decodedImage.TryGetSinglePixelSpan(out var decodedPixels);

			var psnr = ImageQuality.PeakSignalToNoiseRatio(originalPixels, decodedPixels);

			Assert.True(psnr > 30);
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

			using MemoryStream ms = new MemoryStream();

			var encodedRawBytes = encoder.EncodeToRawBytes(inputImage);

			int mipLevels = encoder.CalculateNumberOfMipLevels(inputImage);
			Assert.True(mipLevels > 1);

			for (int i = 0; i < mipLevels; i++)
			{
				ms.Write(encodedRawBytes[i]);
			}

			ms.Position = 0;
			Assert.Equal(0, ms.Position);

			for (int i = 0; i < mipLevels; i++)
			{
				encoder.CalculateMipMapSize(inputImage, i, out int mipWidth, out int mipHeight);
				using var resized = inputImage.Clone(x => x.Resize(mipWidth, mipHeight));

				var decodedImage = decoder.DecodeRaw(ms, CompressionFormat.Bc1, mipWidth, mipHeight);
				resized.TryGetSinglePixelSpan(out var originalPixels);
				decodedImage.TryGetSinglePixelSpan(out var decodedPixels);
				var psnr = ImageQuality.PeakSignalToNoiseRatio(originalPixels, decodedPixels);
				Assert.True(psnr > 30);
			}

			encoder.CalculateMipMapSize(inputImage, mipLevels - 1, out int lastMWidth, out int lastMHeight);
			Assert.Equal(1, lastMWidth);
			Assert.Equal(1, lastMWidth);
		}
	}
}
