using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace BCnEncTests
{
	public class EncodingAsyncTest
	{
		private readonly BcEncoder encoder;
		private readonly BcDecoder decoder;
		private readonly Image<Rgba32> originalImage;
		private readonly Image<Rgba32>[] originalCubeMap;

		public EncodingAsyncTest()
		{
			encoder = new BcEncoder();
			decoder = new BcDecoder();
			originalImage = ImageLoader.TestGradient1;
			originalCubeMap = ImageLoader.TestCubemap;
		}

		[Fact]
		public async void EncodeToDdsAsync()
		{
			var file = await encoder.EncodeToDdsAsync(originalImage);
			var image = decoder.Decode(file);

			TestHelper.AssertImagesEqual(originalImage, image, encoder.OutputOptions.Quality);
			image.Dispose();
		}

		[Fact]
		public async void EncodeToKtxAsync()
		{
			var file = await encoder.EncodeToKtxAsync(originalImage);
			var image = decoder.Decode(file);

			TestHelper.AssertImagesEqual(originalImage, image, encoder.OutputOptions.Quality);
			image.Dispose();
		}

		[Fact]
		public async void EncodeCubemapToDdsAsync()
		{
			var file = await encoder.EncodeCubeMapToDdsAsync(originalCubeMap[0], originalCubeMap[1], originalCubeMap[2],
				originalCubeMap[3], originalCubeMap[4], originalCubeMap[5]);

			for (var i = 0; i < 6; i++)
			{
				var image = decoder.DecodeRaw(file.Faces[i].MipMaps[0].Data, CompressionFormat.Bc1, (int)file.Faces[i].Width, (int)file.Faces[i].Height);

				TestHelper.AssertImagesEqual(originalCubeMap[i], image, encoder.OutputOptions.Quality);
				image.Dispose();
			}
		}

		[Fact]
		public async void EncodeCubemapToKtxAsync()
		{
			var file = await encoder.EncodeCubeMapToKtxAsync(originalCubeMap[0], originalCubeMap[1], originalCubeMap[2],
				originalCubeMap[3], originalCubeMap[4], originalCubeMap[5]);

			for (var i = 0; i < 6; i++)
			{
				var image = decoder.DecodeRaw(file.MipMaps[0].Faces[i].Data, CompressionFormat.Bc1, (int)file.MipMaps[0].Faces[i].Width, (int)file.MipMaps[0].Faces[i].Height);

				TestHelper.AssertImagesEqual(originalCubeMap[i], image, encoder.OutputOptions.Quality);
				image.Dispose();
			}
		}

		[Fact]
		public async void EncodeToRawBytesAsync()
		{
			var data = await encoder.EncodeToRawBytesAsync(originalImage);
			var image = decoder.DecodeRaw(data[0], CompressionFormat.Bc1, originalImage.Width, originalImage.Height);

			TestHelper.AssertImagesEqual(originalImage, image, encoder.OutputOptions.Quality);
			image.Dispose();
		}
	}
}
