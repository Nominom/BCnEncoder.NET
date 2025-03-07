using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
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

			var encoder = new BcEncoder()
			{
				OutputOptions =
				{
					Format = format,
					Quality = quality
				}
			};

			var pixels = TestHelper.GetSinglePixelArray(testImage);
			var colors = MemoryMarshal.Cast<Rgba32, ColorRgba32>(pixels)
				.AsSpan2D(testImage.Height, testImage.Width);

			var ms = new MemoryStream(); 
			
			for (var y = 0; y < testImage.Height; y+=4)
			{
				for (var x = 0; x < testImage.Width; x+=4)
				{
					encoder.EncodeBlock(colors.Slice(y, x, 4, 4), ms);
				}
			}

			Assert.Equal(ms.Position, encoder.GetBlockSize() * encoder.GetBlockCount(testImage.Width, testImage.Height));

			ms.Position = 0;

			var decoder = new BcDecoder();

			var decoded = new ColorRgba32[testImage.Height, testImage.Width];

			for (var y = 0; y < testImage.Height; y += 4)
			{
				for (var x = 0; x < testImage.Width; x += 4)
				{
					decoder.DecodeBlock(ms, format,
						decoded.AsSpan2D().Slice(y, x, 4, 4));
				}
			}

			colors.TryGetSpan(out var oPixels);
			decoded.AsSpan2D().TryGetSpan(out var dPixels);
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

			var encoder = new BcEncoder()
			{
				OutputOptions =
				{
					Format = format,
					Quality = quality
				}
			};

			var pixels = TestHelper.GetSinglePixelArray(testImage);
			var colors = MemoryMarshal.Cast<Rgba32, ColorRgba32>(pixels)
				.AsSpan2D(testImage.Height, testImage.Width);

			Span<byte> buffer = new byte[encoder.GetBlockSize() * encoder.GetBlockCount(testImage.Width, testImage.Height)];

			var blockIndex = 0;
			for (var y = 0; y < testImage.Height; y += 4)
			{
				for (var x = 0; x < testImage.Width; x += 4)
				{
					var bytes = encoder.EncodeBlock(colors.Slice(y, x, 4, 4));
					bytes.CopyTo(buffer.Slice(blockIndex * encoder.GetBlockSize()));
					blockIndex++;
				}
			}
			
			var decoder = new BcDecoder();

			var decoded = new ColorRgba32[testImage.Height, testImage.Width];

			blockIndex = 0;
			for (var y = 0; y < testImage.Height; y += 4)
			{
				for (var x = 0; x < testImage.Width; x += 4)
				{
					
					decoder.DecodeBlock(
						buffer.Slice(
							blockIndex * decoder.GetBlockSize(format),
							decoder.GetBlockSize(format)
							),
						format,
						decoded.AsSpan2D().Slice(y, x, 4, 4));
					blockIndex++;
				}
			}

			colors.TryGetSpan(out var oPixels);
			decoded.AsSpan2D().TryGetSpan(out var dPixels);
			var psnr = ImageQuality.PeakSignalToNoiseRatio(oPixels, dPixels,
				format != CompressionFormat.Bc1);

			TestHelper.AssertPSNR(psnr, quality);
		}
	}
}
