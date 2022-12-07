using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;

using BCnEncTests.Support;

using Xunit;

namespace BCnEncTests
{
	public class BgrTests
	{
		[Fact]
		public void Bgr565DdsDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.B5G6R5);
			var original = ImageLoader.TestLenna;

			// Act
			var dds = encoder.EncodeToDds(original);
			var image = decoder.DecodeToImageRgba32(dds);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void BgrDdsDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Bgr);
			var original = ImageLoader.TestLenna;

			// Act
			var dds = encoder.EncodeToDds(original);
			var image = decoder.DecodeToImageRgba32(dds);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}
	}
}
