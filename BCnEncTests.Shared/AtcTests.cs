using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using CommunityToolkit.HighPerformance;
using Xunit;

namespace BCnEncTests
{
	public class AtcTests
	{
		[Fact]
		public void AtcKtxDecode()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Atc);
			var original = ImageLoader.TestLenna;

			var ktx = encoder.EncodeToKtx(original);
			var image = decoder.Decode2D(ktx);

			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void AtcDdsDecode()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Atc);
			var original = ImageLoader.TestLenna;

			var dds = encoder.EncodeToDds(original);
			var image = decoder.Decode2D(dds);

			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void AtcExplicitKtxDecode()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.AtcExplicitAlpha);
			var original = ImageLoader.TestAlphaGradient1;

			var ktx = encoder.EncodeToKtx(original);
			var image = decoder.Decode2D(ktx);

			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void AtcExplicitDdsDecode()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.AtcExplicitAlpha);
			var original = ImageLoader.TestAlphaGradient1;

			var dds = encoder.EncodeToDds(original);
			var image = decoder.Decode2D(dds);

			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void AtcInterpolatedKtxDecode()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.AtcInterpolatedAlpha);
			var original = ImageLoader.TestAlphaGradient1;

			var ktx = encoder.EncodeToKtx(original);
			var image = decoder.Decode2D(ktx);

			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void AtcInterpolatedDdsDecode()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.AtcInterpolatedAlpha);
			var original = ImageLoader.TestAlphaGradient1;

			var dds = encoder.EncodeToDds(original);
			var image = decoder.Decode2D(dds);

			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}
	}
}
