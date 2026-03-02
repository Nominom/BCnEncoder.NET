using System;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using CommunityToolkit.HighPerformance;
using Xunit;

namespace BCnEncTests
{
	public class EncodingAsyncTest
	{
		private readonly BcEncoder encoder;
		private readonly BcDecoder decoder;
		private readonly Memory2D<ColorRgba32> originalImage;
		private readonly Memory2D<ColorRgba32>[] originalCubeMap;

		public EncodingAsyncTest()
		{
			encoder = new BcEncoder();
			decoder = new BcDecoder();
			originalImage = ImageLoader.TestGradient1;
			originalCubeMap = ImageLoader.TestCubemap;
		}

		[Fact]
		public async Task EncodeToDdsAsync()
		{
			var file = await encoder.EncodeToDdsAsync(originalImage);
			var image = decoder.Decode2D(file);

			TestHelper.AssertImagesEqual(originalImage, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task EncodeToKtxAsync()
		{
			var file = await encoder.EncodeToKtxAsync(originalImage);
			var image = decoder.Decode2D(file);

			TestHelper.AssertImagesEqual(originalImage, image, encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task EncodeCubemapToDdsAsync()
		{
			var file = await encoder.EncodeCubeMapToDdsAsync(originalCubeMap[0], originalCubeMap[1], originalCubeMap[2],
				originalCubeMap[3], originalCubeMap[4], originalCubeMap[5]);

			for (var i = 0; i < 6; i++)
			{
				var rawPixels = decoder.DecodeRaw(file.Faces[i].MipMaps[0].Data,
					(int)file.Faces[i].Width, (int)file.Faces[i].Height, CompressionFormat.Bc1);
				var decodedImage = rawPixels.AsMemory().AsMemory2D((int)file.Faces[i].Height, (int)file.Faces[i].Width);

				TestHelper.AssertImagesEqual(originalCubeMap[i], decodedImage, encoder.OutputOptions.Quality);
			}
		}

		[Fact]
		public async Task EncodeCubemapToKtxAsync()
		{
			var file = await encoder.EncodeCubeMapToKtxAsync(originalCubeMap[0], originalCubeMap[1], originalCubeMap[2],
				originalCubeMap[3], originalCubeMap[4], originalCubeMap[5]);

			for (var i = 0; i < 6; i++)
			{
				var rawPixels = decoder.DecodeRaw(file.MipMaps[0].Faces[i].Data,
					(int)file.MipMaps[0].Faces[i].Width, (int)file.MipMaps[0].Faces[i].Height, CompressionFormat.Bc1);
				var decodedImage = rawPixels.AsMemory().AsMemory2D(
					(int)file.MipMaps[0].Faces[i].Height, (int)file.MipMaps[0].Faces[i].Width);

				TestHelper.AssertImagesEqual(originalCubeMap[i], decodedImage, encoder.OutputOptions.Quality);
			}
		}

		[Fact]
		public async Task EncodeToRawBytesAsync()
		{
			var data = await encoder.EncodeToRawBytesAsync(originalImage);
			var rawPixels = decoder.DecodeRaw(data[0], originalImage.Width, originalImage.Height, CompressionFormat.Bc1);
			var image = rawPixels.AsMemory().AsMemory2D(originalImage.Height, originalImage.Width);

			TestHelper.AssertImagesEqual(originalImage, image, encoder.OutputOptions.Quality);
		}
	}
}
