using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests
{
	public class AtcTests
	{
		[Fact]
		public void AtcKtxDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Atc);
			var original = ImageLoader.TestLenna;

			// Act
			var ktx = encoder.EncodeToKtx(original);
			var image = decoder.Decode(ktx);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void AtcDdsDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Atc);
			var original = ImageLoader.TestLenna;

			// Act
			var dds = encoder.EncodeToDds(original);
			var image = decoder.Decode(dds);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void AtcExplicitKtxDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.AtcExplicitAlpha);
			var original = ImageLoader.TestAlphaGradient1;

			// Act
			var ktx = encoder.EncodeToKtx(original);
			var image = decoder.Decode(ktx);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void AtcExplicitDdsDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.AtcExplicitAlpha);
			var original = ImageLoader.TestAlphaGradient1;

			// Act
			var dds = encoder.EncodeToDds(original);
			var image = decoder.Decode(dds);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void AtcInterpolatedKtxDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.AtcInterpolatedAlpha);
			var original = ImageLoader.TestAlphaGradient1;

			// Act
			var ktx = encoder.EncodeToKtx(original);
			var image = decoder.Decode(ktx);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void AtcInterpolatedDdsDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.AtcInterpolatedAlpha);
			var original = ImageLoader.TestAlphaGradient1;

			// Act
			var dds = encoder.EncodeToDds(original);
			var image = decoder.Decode(dds);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}
	}
}
