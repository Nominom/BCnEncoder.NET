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
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Bgra);
			var original = ImageLoader.TestLenna;

			var dds = encoder.EncodeToDds(original);
			var image = decoder.Decode2D(dds);

			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void BgraAlphaDdsDecode()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Bgra);
			var original = ImageLoader.TestAlphaGradient1;

			var dds = encoder.EncodeToDds(original);
			var image = decoder.Decode2D(dds);

			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void BgraKtxDecode()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Bgra);
			var original = ImageLoader.TestLenna;

			var ktx = encoder.EncodeToKtx(original);
			var image = decoder.Decode2D(ktx);

			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void BgraAlphaKtxDecode()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder(CompressionFormat.Bgra);
			var original = ImageLoader.TestAlphaGradient1;

			var ktx = encoder.EncodeToKtx(original);
			var image = decoder.Decode2D(ktx);

			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}
	}
}
