using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using Xunit;

namespace BCnEncTests.Helpers
{
	public class ImageToBlocksTests
	{
		[Fact]
		public void CreateBlocksExact()
		{
			var testImage = new ColorRgbaFloat[16 * 16];

			var blocks = ImageToBlocks.ImageTo4X4(testImage, 16, 16, out var blocksWidth, out var blocksHeight);

			Assert.Equal(16, blocks.Length);
			Assert.Equal(4, blocksWidth);
			Assert.Equal(4, blocksHeight);
		}

		[Fact]
		public void CreateBlocksPadding()
		{
			var testImage = new ColorRgbaFloat[15 * 11];

			var blocks = ImageToBlocks.ImageTo4X4(testImage, 11, 15,  out var blocksWidth, out var blocksHeight);

			Assert.Equal(12, blocks.Length);
			Assert.Equal(3, blocksWidth);
			Assert.Equal(4, blocksHeight);
		}

		[Fact]
		public void PaddingColor()
		{
			var testImage = new ColorRgbaFloat[15 * 11];

			var pixels = testImage.AsSpan();

			for (var i = 0; i < pixels.Length; i++) {
				pixels[i] = new ColorRgbaFloat(0, .5f, .5f);
			}

			var blocks = ImageToBlocks.ImageTo4X4(testImage, 11, 15, out var blocksWidth, out var blocksHeight);

			Assert.Equal(12, blocks.Length);
			Assert.Equal(3, blocksWidth);
			Assert.Equal(4, blocksHeight);

			for (var x = 0; x < blocksWidth; x++) {
				for (var y = 0; y < blocksHeight; y++) {
					foreach (var color in blocks[x + y * blocksWidth].AsSpan) {
						Assert.Equal(new ColorRgbaFloat(0, .5f, .5f), color);
					}
				}
			}
		}

		[Fact]
		public void BlocksToImage()
		{
			var r = new Random(0);
			var testImage = new ColorRgbaFloat[16 * 16];

			var pixels = testImage.AsSpan();

			for (var i = 0; i < pixels.Length; i++) {
				pixels[i] = new ColorRgbaFloat(
					r.NextSingle(),
					r.NextSingle(),
					r.NextSingle(),
					r.NextSingle());
			}

			var blocks = ImageToBlocks.ImageTo4X4(testImage, 16, 16, out var blocksWidth, out var blocksHeight);

			Assert.Equal(16, blocks.Length);
			Assert.Equal(4, blocksWidth);
			Assert.Equal(4, blocksHeight);

			var output = new ColorRgbaFloat[16 * 16];

			ImageToBlocks.ColorsFromRawBlocks(blocks, output, 16, 16);

			var pixels2 = output.AsSpan();

			Assert.Equal(pixels.Length, pixels2.Length);
			for (var i = 0; i < pixels.Length; i++) {
				Assert.Equal(pixels[i], pixels2[i]);
			}
		}
	}
}
