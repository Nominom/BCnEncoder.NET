using System;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Rgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;

namespace BCnEncTests
{
	public class BlockTests
	{
		[Fact]
		public void CreateBlocksExact()
		{
			using Image<Rgba32> testImage = new Image<Rgba32>(16, 16);

			testImage.Frames[0].TryGetSinglePixelSpan(out var span);
			var blocks = ImageToBlocks.ImageTo4X4(span.ToArray().ToBytes(), testImage.Frames[0].Width, testImage.Frames[0].Height, out var blocksWidth, out var blocksHeight);

			Assert.Equal(16, blocks.Length);
			Assert.Equal(4, blocksWidth);
			Assert.Equal(4, blocksHeight);
		}

		[Fact]
		public void CreateBlocksPadding()
		{
			using Image<Rgba32> testImage = new Image<Rgba32>(11, 15);

			testImage.Frames[0].TryGetSinglePixelSpan(out var span);
			var blocks = ImageToBlocks.ImageTo4X4(span.ToArray().ToBytes(), testImage.Frames[0].Width, testImage.Frames[0].Height, out var blocksWidth, out var blocksHeight);

			Assert.Equal(12, blocks.Length);
			Assert.Equal(3, blocksWidth);
			Assert.Equal(4, blocksHeight);
		}

		[Fact]
		public void PaddingColor()
		{
			using Image<Rgba32> testImage = new Image<Rgba32>(13, 13);

			if (!testImage.TryGetSinglePixelSpan(out var pixels)) {
				throw new Exception("Cannot get pixel span.");
			}
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = new Rgba32(0, 125, 125);
			}

			testImage.Frames[0].TryGetSinglePixelSpan(out var span);
			var blocks = ImageToBlocks.ImageTo4X4(span.ToArray().ToBytes(), testImage.Frames[0].Width, testImage.Frames[0].Height, out var blocksWidth, out var blocksHeight);

			Assert.Equal(16, blocks.Length);
			Assert.Equal(4, blocksWidth);
			Assert.Equal(4, blocksHeight);

			for (int x = 0; x < blocksWidth; x++) {
				for (int y = 0; y < blocksHeight; y++) {
					foreach (var color in blocks[x + y * blocksWidth].AsArray) {
						Assert.Equal(new BCnEncoder.Shared.Rgba32(0, 125, 125), color);
					}
				}
			}
		}

		[Fact]
		public void BlocksToImage()
		{
			Random r = new Random(0);
			using Image<Rgba32> testImage = new Image<Rgba32>(16, 16);

			if (!testImage.TryGetSinglePixelSpan(out var pixels)) {
				throw new Exception("Cannot get pixel span.");
			}
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = new Rgba32(
					(byte)r.Next(255),
					(byte)r.Next(255),
					(byte)r.Next(255),
					(byte)r.Next(255));
			}

			testImage.Frames[0].TryGetSinglePixelSpan(out var span);
			var blocks = ImageToBlocks.ImageTo4X4(span.ToArray().ToBytes(), testImage.Frames[0].Width, testImage.Frames[0].Height, out var blocksWidth, out var blocksHeight);

			Assert.Equal(16, blocks.Length);
			Assert.Equal(4, blocksWidth);
			Assert.Equal(4, blocksHeight);

			var output = ImageToBlocks.ImageFromRawBlocks(blocks, blocksWidth, blocksHeight);

			Assert.Equal(pixels.Length * 4, output.Length);
			for (int i = 0; i < pixels.Length; i++)
			{
				Rgba32 o = new Rgba32(output[i * 4 + 0], output[i * 4 + 1], output[i * 4 + 2], output[i * 4 + 3]);
				Assert.Equal(pixels[i], o);
			}
		}

		[Fact]
		public void BlockError()
		{
			using Image<Rgba32> testImage = new Image<Rgba32>(16, 16);

			testImage.Frames[0].TryGetSinglePixelSpan(out var span);
			var blocks = ImageToBlocks.ImageTo4X4(span.ToArray().ToBytes(), testImage.Frames[0].Width, testImage.Frames[0].Height, out var blocksWidth, out var blocksHeight);

			var block1 = blocks[2 + 2 * blocksWidth];
			var block2 = blocks[2 + 2 * blocksWidth];

			Assert.Equal(0, block1.CalculateError(block2));

			for (int i = 0; i < block2.AsArray.Length; i++)
			{
				block2[i] = new BCnEncoder.Shared.Rgba32(
					(byte)(block1.AsArray[i].R + 2),
					block1.AsArray[i].G,
					block1.AsArray[i].B,
					block1.AsArray[i].A);
			}
			Assert.Equal(2, block1.CalculateError(block2));

			for (int i = 0; i < block2.AsArray.Length; i++) {
				block2[i] = new BCnEncoder.Shared.Rgba32(
					block2.AsArray[i].R,
				(byte)(block2.AsArray[i].R + 20),
					block2.AsArray[i].B,
					block2.AsArray[i].A);
			}
			Assert.Equal(22, block1.CalculateError(block2));
		}
	}
}
