using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;

using BCnEncTests.Support;

using System.IO;

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
			var image = decoder.DecodeToImageRgba32(dds);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void Bgra4444DdsDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.B4G4R4A4);
			var original = ImageLoader.TestLenna;

			// Act
			var dds = encoder.EncodeToDds(original);
			var image = decoder.DecodeToImageRgba32(dds);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void Bgra5551DdsDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.B5G5R5A1);
			var original = ImageLoader.TestLenna;

			// Act
			var dds = encoder.EncodeToDds(original);
			var image = decoder.DecodeToImageRgba32(dds);

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
			var image = decoder.DecodeToImageRgba32(dds);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void Bgra4444AlphaDdsDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.B4G4R4A4);
			var original = ImageLoader.TestAlphaGradient1;

			// Act
			var dds = encoder.EncodeToDds(original);
			var image = decoder.DecodeToImageRgba32(dds);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void Bgra5551AlphaDdsDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.B5G5R5A1);
			var original = ImageLoader.TestTransparentSprite1;

			// Act
			var dds = encoder.EncodeToDds(original);
			var image = decoder.DecodeToImageRgba32(dds);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void BgraKtxDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Bgra);
			var original = ImageLoader.TestLenna;

			// Act
			var ktx = encoder.EncodeToKtx(original);
			var image = decoder.DecodeToImageRgba32(ktx);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void BgraAlphaKtxDecode()
		{
			// Arrange
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Bgra);
			var original = ImageLoader.TestAlphaGradient1;

			// Act
			var ktx = encoder.EncodeToKtx(original);
			var image = decoder.DecodeToImageRgba32(ktx);

			// Assert
			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}
	}
}
