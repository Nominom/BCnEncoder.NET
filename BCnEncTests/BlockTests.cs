using System;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace BCnEncTests
{
	public class BlockTests
	{
		[Fact]
		public void CreateBlocksExact()
		{
			using Image<Rgba32> testImage = new Image<Rgba32>(16, 16);

			var blocks = ImageToBlocks.ImageTo4X4(testImage.Frames[0], out var blocksWidth, out var blocksHeight);

			Assert.Equal(16, blocks.Length);
			Assert.Equal(4, blocksWidth);
			Assert.Equal(4, blocksHeight);
		}

		[Fact]
		public void CreateBlocksPadding()
		{
			using Image<Rgba32> testImage = new Image<Rgba32>(11, 15);

			var blocks = ImageToBlocks.ImageTo4X4(testImage.Frames[0], out var blocksWidth, out var blocksHeight);

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

			var blocks = ImageToBlocks.ImageTo4X4(testImage.Frames[0], out var blocksWidth, out var blocksHeight);

			Assert.Equal(16, blocks.Length);
			Assert.Equal(4, blocksWidth);
			Assert.Equal(4, blocksHeight);

			for (int x = 0; x < blocksWidth; x++) {
				for (int y = 0; y < blocksHeight; y++) {
					foreach (var color in blocks[x + y * blocksWidth].AsSpan) {
						Assert.Equal(new Rgba32(0, 125, 125), color);
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

			var blocks = ImageToBlocks.ImageTo4X4(testImage.Frames[0], out var blocksWidth, out var blocksHeight);

			Assert.Equal(16, blocks.Length);
			Assert.Equal(4, blocksWidth);
			Assert.Equal(4, blocksHeight);

			using var output = ImageToBlocks.ImageFromRawBlocks(blocks, blocksWidth, blocksHeight);
			
			if (!output.TryGetSinglePixelSpan(out var pixels2)) {
				throw new Exception("Cannot get pixel span.");
			}

			Assert.Equal(pixels.Length, pixels2.Length);
			for (int i = 0; i < pixels.Length; i++) {
				Assert.Equal(pixels[i], pixels2[i]);
			}
		}

		[Fact]
		public void BlockError()
		{
			using Image<Rgba32> testImage = new Image<Rgba32>(16, 16);

			var blocks = ImageToBlocks.ImageTo4X4(testImage.Frames[0], out var blocksWidth, out var blocksHeight);

			var block1 = blocks[2 + 2 * blocksWidth];
			var block2 = blocks[2 + 2 * blocksWidth];

			Assert.Equal(0, block1.CalculateError(block2));

			for (int i = 0; i < block2.AsSpan.Length; i++) {
				block2.AsSpan[i].R = (byte) (block2.AsSpan[i].R + 2);
			}
			Assert.Equal(2, block1.CalculateError(block2));

			for (int i = 0; i < block2.AsSpan.Length; i++) {
				block2.AsSpan[i].G = (byte) (block2.AsSpan[i].R + 20);
			}
			Assert.Equal(22, block1.CalculateError(block2));
		}
	}
}
