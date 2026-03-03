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
	public class SingleBlockTests
	{
		[Theory]
		[InlineData(CompressionFormat.Bc1, CompressionQuality.Fast)]
		[InlineData(CompressionFormat.Bc1, CompressionQuality.Balanced)]
		[InlineData(CompressionFormat.Bc3, CompressionQuality.Fast)]
		[InlineData(CompressionFormat.Bc3, CompressionQuality.Balanced)]
		[InlineData(CompressionFormat.Bc7, CompressionQuality.Fast)]
		public void SingleBlockEncodeDecodeStream(CompressionFormat format, CompressionQuality quality)
		{
			var testImage = ImageLoader.TestAlpha1;
			int height = testImage.Height;
			int width = testImage.Width;

			var encoder = new BcEncoder()
			{
				OutputOptions =
				{
					Format = format,
					Quality = quality
				}
			};

			var colors = testImage.Span;

			var ms = new MemoryStream();

			for (var y = 0; y < height; y += 4)
			{
				for (var x = 0; x < width; x += 4)
				{
					encoder.EncodeBlock(colors.Slice(y, x, 4, 4), ms);
				}
			}

			Assert.Equal(ms.Position, encoder.GetBlockSize() * encoder.GetBlockCount(width, height));

			ms.Position = 0;

			var decoder = new BcDecoder();

			var decoded = new ColorRgba32[height, width];

			for (var y = 0; y < height; y += 4)
			{
				for (var x = 0; x < width; x += 4)
				{
					decoder.DecodeBlock(ms, format,
						decoded.AsSpan2D().Slice(y, x, 4, 4));
				}
			}

			var oPixels = TestHelper.GetSinglePixelArrayAsColors(testImage);
			var dPixels = FlattenDecoded(decoded, height, width);
			var psnr = ImageQuality.PeakSignalToNoiseRatio(oPixels, dPixels,
				format != CompressionFormat.Bc1);

			TestHelper.AssertPSNR(psnr, quality);
		}

		[Theory]
		[InlineData(CompressionFormat.Bc1, CompressionQuality.Fast)]
		[InlineData(CompressionFormat.Bc1, CompressionQuality.Balanced)]
		[InlineData(CompressionFormat.Bc3, CompressionQuality.Fast)]
		[InlineData(CompressionFormat.Bc3, CompressionQuality.Balanced)]
		[InlineData(CompressionFormat.Bc7, CompressionQuality.Fast)]
		public void SingleBlockEncodeDecode(CompressionFormat format, CompressionQuality quality)
		{
			var testImage = ImageLoader.TestAlpha1;
			int height = testImage.Height;
			int width = testImage.Width;

			var encoder = new BcEncoder()
			{
				OutputOptions =
				{
					Format = format,
					Quality = quality
				}
			};

			var colors = testImage.Span;

			var encMs = new MemoryStream();

			for (var y = 0; y < height; y += 4)
			{
				for (var x = 0; x < width; x += 4)
				{
					encoder.EncodeBlock(colors.Slice(y, x, 4, 4), encMs);
				}
			}

			var buffer = encMs.ToArray();

			var decoder = new BcDecoder();

			var decoded = new ColorRgba32[height, width];

			var blockIndex = 0;
			for (var y = 0; y < height; y += 4)
			{
				for (var x = 0; x < width; x += 4)
				{
					decoder.DecodeBlock(
						new Span<byte>(buffer,
							blockIndex * decoder.GetBlockSize(format),
							decoder.GetBlockSize(format)),
						format,
						decoded.AsSpan2D().Slice(y, x, 4, 4));
					blockIndex++;
				}
			}

			var oPixels = TestHelper.GetSinglePixelArrayAsColors(testImage);
			var dPixels = FlattenDecoded(decoded, height, width);
			var psnr = ImageQuality.PeakSignalToNoiseRatio(oPixels, dPixels,
				format != CompressionFormat.Bc1);

			TestHelper.AssertPSNR(psnr, quality);
		}

		private static ColorRgba32[] FlattenDecoded(ColorRgba32[,] decoded, int height, int width)
		{
			var result = new ColorRgba32[height * width];
			for (var y = 0; y < height; y++)
				for (var x = 0; x < width; x++)
					result[y * width + x] = decoded[y, x];
			return result;
		}
	}
}
