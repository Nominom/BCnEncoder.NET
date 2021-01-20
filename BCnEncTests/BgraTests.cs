using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests
{
	public class BgraTests
	{
		[Fact]
		public void BgraDdsDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Bgra);
			var original = ImageLoader.TestLenna;

			// Act
			var dds = encoder.EncodeToDds(original);
			var image = decoder.Decode(dds);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void BgraAlphaDdsDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Bgra);
			var original = ImageLoader.TestAlphaGradient1;

			// Act
			var dds = encoder.EncodeToDds(original);
			var image = decoder.Decode(dds);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}
	}
}
