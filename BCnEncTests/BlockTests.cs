using System;
using BCnEncoder.Shared;
using CommunityToolkit.HighPerformance;
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
			var testImage = new ColorRgba32[16, 16];

			var blocks = ImageToBlocks.ImageTo4X4(testImage, out var blocksWidth, out var blocksHeight);

			Assert.Equal(16, blocks.Length);
			Assert.Equal(4, blocksWidth);
			Assert.Equal(4, blocksHeight);
		}

		[Fact]
		public void CreateBlocksPadding()
		{
			var testImage = new ColorRgba32[15, 11];

			var blocks = ImageToBlocks.ImageTo4X4(testImage, out var blocksWidth, out var blocksHeight);

			Assert.Equal(12, blocks.Length);
			Assert.Equal(3, blocksWidth);
			Assert.Equal(4, blocksHeight);
		}

		[Fact]
		public void PaddingColor()
		{
			var testImage = new ColorRgba32[15, 11];

			var pixels = testImage.AsSpan();
			
			for (var i = 0; i < pixels.Length; i++) {
				pixels[i] = new ColorRgba32(0, 125, 125);
			}

			var blocks = ImageToBlocks.ImageTo4X4(testImage, out var blocksWidth, out var blocksHeight);

			Assert.Equal(12, blocks.Length);
			Assert.Equal(3, blocksWidth);
			Assert.Equal(4, blocksHeight);

			for (var x = 0; x < blocksWidth; x++) {
				for (var y = 0; y < blocksHeight; y++) {
					foreach (var color in blocks[x + y * blocksWidth].AsSpan) {
						Assert.Equal(new ColorRgba32(0, 125, 125), color);
					}
				}
			}
		}

		[Fact]
		public void BlocksToImage()
		{
			var r = new Random(0);
			var testImage = new ColorRgba32[16, 16];

			var pixels = testImage.AsSpan();
			
			for (var i = 0; i < pixels.Length; i++) {
				pixels[i] = new ColorRgba32(
					(byte)r.Next(255),
					(byte)r.Next(255),
					(byte)r.Next(255),
					(byte)r.Next(255));
			}

			var blocks = ImageToBlocks.ImageTo4X4(testImage, out var blocksWidth, out var blocksHeight);

			Assert.Equal(16, blocks.Length);
			Assert.Equal(4, blocksWidth);
			Assert.Equal(4, blocksHeight);

			var output = ImageToBlocks.ColorsFromRawBlocks(blocks, 16, 16);

			var pixels2 = output.AsSpan();

			Assert.Equal(pixels.Length, pixels2.Length);
			for (var i = 0; i < pixels.Length; i++) {
				Assert.Equal(pixels[i], pixels2[i]);
			}
		}

		[Fact]
		public void BlockError()
		{
			var testImage = new ColorRgba32[16, 16];

			var blocks = ImageToBlocks.ImageTo4X4(testImage, out var blocksWidth, out var blocksHeight);

			var block1 = blocks[2 + 2 * blocksWidth];
			var block2 = blocks[2 + 2 * blocksWidth];

			Assert.Equal(0, block1.CalculateError(block2));

			for (var i = 0; i < block2.AsSpan.Length; i++) {
				block2.AsSpan[i].r = (byte) (block2.AsSpan[i].r + 2);
			}
			Assert.Equal(2, block1.CalculateError(block2));

			for (var i = 0; i < block2.AsSpan.Length; i++) {
				block2.AsSpan[i].g = (byte) (block2.AsSpan[i].r + 20);
			}
			Assert.Equal(22, block1.CalculateError(block2));
		}
	}
}
