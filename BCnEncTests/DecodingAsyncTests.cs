using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
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

			var psnr = TestHelper.CalculatePSNR(original, image);
			if (encoder.OutputOptions.Quality == CompressionQuality.Fast)
			{
				Assert.True(psnr > 25);
			}
			else
			{
				Assert.True(psnr > 30);
			}
		}

		[Fact]
		public async void DecodeAllMipMapsAsync()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder();
			var original = ImageLoader.TestGradient1;

			var file = encoder.EncodeToKtx(original);
			var image = await decoder.DecodeAllMipMapsAsync(file);

			var psnr = TestHelper.CalculatePSNR(original, image[0]);
			if (encoder.OutputOptions.Quality == CompressionQuality.Fast)
			{
				Assert.True(psnr > 25);
			}
			else
			{
				Assert.True(psnr > 30);
			}
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

			var psnr = TestHelper.CalculatePSNR(original, image);
			if (encoder.OutputOptions.Quality == CompressionQuality.Fast)
			{
				Assert.True(psnr > 25);
			}
			else
			{
				Assert.True(psnr > 30);
			}
		}
	}
}
