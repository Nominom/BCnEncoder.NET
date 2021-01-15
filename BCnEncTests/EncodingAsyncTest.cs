using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests
{
	public class EncodingAsyncTest
	{
		private readonly Image<Rgba32> originalImage;
		private readonly Image<Rgba32>[] originalCubeMap;

		public EncodingAsyncTest()
		{
			originalImage = ImageLoader.TestGradient1;
			originalCubeMap = ImageLoader.TestCubemap;
		}

		[Fact]
		public async void EncodeToDdsAsync()
		{
			var encoder = new BcEncoder();
			var decoder = new BcDecoder();

			var file = await encoder.EncodeToDdsAsync(originalImage);
			var image = decoder.Decode(file);

			var psnr = TestHelper.CalculatePSNR(originalImage, image);
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
		public async void EncodeToKtxAsync()
		{
			var encoder = new BcEncoder();
			var decoder = new BcDecoder();

			var file = await encoder.EncodeToKtxAsync(originalImage);
			var image = decoder.Decode(file);

			var psnr = TestHelper.CalculatePSNR(originalImage, image);
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
		public async void EncodeCubemapToDdsAsync()
		{
			var encoder = new BcEncoder();
			var decoder = new BcDecoder();

			var file = await encoder.EncodeCubeMapToDdsAsync(originalCubeMap[0], originalCubeMap[1], originalCubeMap[2],
				originalCubeMap[3], originalCubeMap[4], originalCubeMap[5]);

			for (var i = 0; i < 6; i++)
			{
				var image = decoder.DecodeRaw(file.Faces[i].MipMaps[0].Data, CompressionFormat.Bc1, (int)file.Faces[i].Width, (int)file.Faces[i].Height);

				var psnr = TestHelper.CalculatePSNR(originalCubeMap[i], image);
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

		[Fact]
		public async void EncodeCubemapToKtxAsync()
		{
			var encoder = new BcEncoder();
			var decoder = new BcDecoder();

			var file = await encoder.EncodeCubeMapToKtxAsync(originalCubeMap[0], originalCubeMap[1], originalCubeMap[2],
				originalCubeMap[3], originalCubeMap[4], originalCubeMap[5]);

			for (var i = 0; i < 6; i++)
			{
				var image = decoder.DecodeRaw(file.MipMaps[0].Faces[i].Data, CompressionFormat.Bc1, (int)file.MipMaps[0].Faces[i].Width, (int)file.MipMaps[0].Faces[i].Height);

				var psnr = TestHelper.CalculatePSNR(originalCubeMap[i], image);
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

		[Fact]
		public async void EncodeToRawBytesAsync()
		{
			var encoder = new BcEncoder();
			var decoder = new BcDecoder();

			var data = await encoder.EncodeToRawBytesAsync(originalImage);
			var image = decoder.DecodeRaw(data[0], CompressionFormat.Bc1, originalImage.Width, originalImage.Height);

			var psnr = TestHelper.CalculatePSNR(originalImage, image);
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
