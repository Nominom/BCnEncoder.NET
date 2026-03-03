using System;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using CommunityToolkit.HighPerformance;
using Xunit;

namespace BCnEncTests
{
	public class MipMapperTests
	{
		[Fact]
		public void MipChainHasCorrectDimensions()
		{
			var image = ImageLoader.TestGradient1; // 512x416
			var numMips = 0;
			var chain = MipMapper.GenerateMipChain(image, ref numMips);

			Assert.Equal(chain.Length, numMips);
			Assert.True(numMips > 1);

			for (var i = 0; i < numMips; i++)
			{
				Assert.Equal(Math.Max(1, image.Width  >> i), chain[i].Width);
				Assert.Equal(Math.Max(1, image.Height >> i), chain[i].Height);
			}

			// Last level must be 1x1
			Assert.Equal(1, chain[numMips - 1].Width);
			Assert.Equal(1, chain[numMips - 1].Height);
		}

		/// <summary>
		/// A solid-color image must downsample to exactly the same color at
		/// every mip level, regardless of how many times the box filter is applied.
		/// </summary>
		[Fact]
		public void SolidColorMipChainIsExact()
		{
			var color = new ColorRgba32(200, 100, 50, 255);
			var pixels = new ColorRgba32[16 * 16];
			for (var i = 0; i < pixels.Length; i++) pixels[i] = color;

			var image = new Memory2D<ColorRgba32>(pixels, 16, 16);
			var numMips = 0;
			var chain = MipMapper.GenerateMipChain(image, ref numMips);

			// 16x16 -> 8x8 -> 4x4 -> 2x2 -> 1x1 = 5 levels
			Assert.Equal(5, numMips);

			for (var level = 0; level < numMips; level++)
			{
				var span = chain[level].Span;
				for (var y = 0; y < chain[level].Height; y++)
				for (var x = 0; x < chain[level].Width; x++)
					Assert.Equal(color, span[y, x]);
			}
		}

		/// <summary>
		/// Verifies the exact 2x2 box-filter arithmetic with a known input.
		/// A 2x2 image of two values repeated both rows should average exactly.
		/// </summary>
		[Fact]
		public void KnownPatternDownsamplesCorrectly()
		{
			// 2-wide, 2-tall: left column = 100 red, right column = 200 red.
			// Expected 1x1 average: r = (100+200+100+200)/4 = 150, g=b=0, a=255.
			var pixels = new[]
			{
				new ColorRgba32(100, 0, 0, 255), new ColorRgba32(200, 0, 0, 255),
				new ColorRgba32(100, 0, 0, 255), new ColorRgba32(200, 0, 0, 255),
			};
			var image = new Memory2D<ColorRgba32>(pixels, 2, 2);
			var numMips = 0;
			var chain = MipMapper.GenerateMipChain(image, ref numMips);

			Assert.Equal(2, numMips);          // 2x2 -> 1x1
			Assert.Equal(1, chain[1].Width);
			Assert.Equal(1, chain[1].Height);
			Assert.Equal(new ColorRgba32(150, 0, 0, 255), chain[1].Span[0, 0]);
		}
	}
}
