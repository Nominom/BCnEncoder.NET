using System.ComponentModel.DataAnnotations;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests
{
	public class DecodingAsyncTests
	{
		[Fact]
		public async void DecodeAsync()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder();
			var original = ImageLoader.TestGradient1;

			var file = encoder.EncodeToKtx(original);
			var image = await decoder.DecodeAsync(file);

			TestHelper.AssertImagesEqual(original, image,encoder.OutputOptions.Quality);
			image.Dispose();
		}

		[Fact]
		public async void DecodeAllMipMapsAsync()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder();
			var original = ImageLoader.TestGradient1;

			var file = encoder.EncodeToKtx(original);
			var images = await decoder.DecodeAllMipMapsAsync(file);

			TestHelper.AssertImagesEqual(original, images[0], encoder.OutputOptions.Quality);
			foreach(var img in images)
				img.Dispose();
		}

		[Fact]
		public async void DecodeRawAsync()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder();
			var original = ImageLoader.TestGradient1;

			var file = encoder.EncodeToKtx(original);
			var image = await decoder.DecodeRawAsync(file.MipMaps[0].Faces[0].Data, CompressionFormat.Bc1,
				(int) file.MipMaps[0].Width, (int) file.MipMaps[0].Height);

			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
			image.Dispose();
		}
	}
}
