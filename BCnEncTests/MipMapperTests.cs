using System;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests
{
	public class MipMapperTests
	{
		private readonly ITestOutputHelper output;

		public MipMapperTests(ITestOutputHelper output) => this.output = output;

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
		/// Compares each mip level produced by MipMapper against the equivalent
		/// ImageSharp Box-filter resize (Compand=false so both operate in sRGB
		/// byte space without gamma correction). The PSNR should be very high
		/// because both algorithms perform the same 2x2 box average.
		/// </summary>
		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(4)]
		public void MipLevelMatchesImageSharpBoxFilter(int mipLevel)
		{
			var image = ImageLoader.TestGradient1;
			var numMips = mipLevel + 1;
			var chain = MipMapper.GenerateMipChain(image, ref numMips);

			var mipW = chain[mipLevel].Width;
			var mipH = chain[mipLevel].Height;

			// ImageSharp Box sampler + Compand=false: no gamma correction,
			// averages pixel values in sRGB space — matches MipMapper exactly.
			using var imageSharp = ImageLoader.LoadTestImageSharp("../../../testImages/test_gradient_1_512.jpg");
			using var reference = imageSharp.Clone(x => x.Resize(new ResizeOptions
			{
				Size = new Size(mipW, mipH),
				Sampler = KnownResamplers.Box,
				Compand = false
			}));

			var mipColors = TestHelper.GetSinglePixelArrayAsColors(chain[mipLevel]);
			var refColors  = TestHelper.GetSinglePixelArrayAsColors(reference);

			var psnr = ImageQuality.PeakSignalToNoiseRatio(mipColors, refColors);
			output.WriteLine($"Mip level {mipLevel} ({mipW}x{mipH}): PSNR vs ImageSharp Box = {psnr:F2} dB");

			Assert.True(psnr > 40,
				$"Mip level {mipLevel}: PSNR vs ImageSharp Box was {psnr:F2} dB (expected > 40)");
		}
	}
}
