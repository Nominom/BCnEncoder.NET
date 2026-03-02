using System;
using System.Drawing;
using System.Drawing.Imaging;
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
			var decodedImage = decodedPixels.AsMemory().AsMemory2D(inputImage.Height, inputImage.Width);

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
			var decodedImage = rawPixels.AsMemory().AsMemory2D(inputImage.Height, inputImage.Width);

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

			for (var i = 0; i < mipLevels; i++)
			{
				encoder.CalculateMipMapSize(inputImage.Width, inputImage.Height, i, out var mipWidth, out var mipHeight);
				var resized = ResizeImage(inputImage, mipWidth, mipHeight);

				var blockSize = decoder.GetBlockSize(CompressionFormat.Bc1);
				var blockCount = decoder.GetBlockCount(mipWidth, mipHeight);
				var buffer = new byte[blockSize * blockCount];
				ms.Read(buffer, 0, buffer.Length);

				var rawPixels = decoder.DecodeRaw(buffer, mipWidth, mipHeight, CompressionFormat.Bc1);
				var decodedImage = rawPixels.AsMemory().AsMemory2D(mipHeight, mipWidth);

				var originalColors = TestHelper.GetSinglePixelArrayAsColors(resized);
				var decodedColors  = TestHelper.GetSinglePixelArrayAsColors(decodedImage);

				TestHelper.AssertPixelsEqual(originalColors, decodedColors, encoder.OutputOptions.Quality);
			}

			encoder.CalculateMipMapSize(inputImage.Width, inputImage.Height, mipLevels - 1, out var lastMWidth, out var lastMHeight);
			Assert.Equal(1, lastMWidth);
			Assert.Equal(1, lastMHeight);
		}

		private static ReadOnlyMemory2D<ColorRgba32> ResizeImage(Memory2D<ColorRgba32> image, int newWidth, int newHeight)
		{
			int numMips = 0;
			var chain = MipMapper.GenerateMipChain(image, ref numMips);

			foreach (var memory2D in chain)
			{
				if (memory2D.Width == newWidth && memory2D.Height == newHeight)
				{
					return memory2D;
				}
			}
			throw new InvalidOperationException("Cannot resize image to non-mip dimensions");
		}

		private static unsafe Bitmap ToBitmap(Memory2D<ColorRgba32> image)
		{
			var bmp = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			var data = bmp.LockBits(new Rectangle(0, 0, image.Width, image.Height),
				ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			byte* ptr = (byte*)data.Scan0;
			var span = image.Span;
			for (int y = 0; y < image.Height; y++)
			{
				for (int x = 0; x < image.Width; x++)
				{
					var c = span[y, x];
					ptr[0] = c.b;
					ptr[1] = c.g;
					ptr[2] = c.r;
					ptr[3] = c.a;
					ptr += 4;
				}
			}
			bmp.UnlockBits(data);
			return bmp;
		}
	}
}
