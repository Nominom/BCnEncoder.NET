using System;
using System.IO;
using System.Threading.Tasks;
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
		public async Task DecodeAsync()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder();
			var original = ImageLoader.TestGradient1;

			var file = encoder.EncodeToKtx(original);
			var image = await decoder.Decode2DAsync(file);

			TestHelper.AssertImagesEqual(original, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task DecodeAllMipMapsAsync()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder();
			var original = ImageLoader.TestGradient1;

			var file = encoder.EncodeToKtx(original);
			var images = await decoder.DecodeAllMipMaps2DAsync(file);

			TestHelper.AssertImagesEqual(original, images[0], encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task DecodeRawAsync()
		{
			var decoder = new BcDecoder();
			var encoder = new BcEncoder();
			var original = ImageLoader.TestGradient1;

			var file = encoder.EncodeToKtx(original);

			var rawBytes = file.MipMaps[0].Faces[0].Data;
			var mipWidth = (int)file.MipMaps[0].Width;
			var mipHeight = (int)file.MipMaps[0].Height;

			var decoded = await Task.Run(() =>
			{
				var pixels = decoder.DecodeRaw(rawBytes, mipWidth, mipHeight, CompressionFormat.Bc1);
				return pixels.AsMemory().AsMemory2D(mipHeight, mipWidth);
			});

			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}
	}
}
