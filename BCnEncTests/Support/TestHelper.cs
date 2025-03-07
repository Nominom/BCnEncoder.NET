using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests.Support
{
	public static class TestHelper
	{
		#region Assertions

		public static void AssertPixelsEqual(Span<Rgba32> originalPixels, Span<Rgba32> pixels, CompressionQuality quality)
		{
			var psnr = ImageQuality.PeakSignalToNoiseRatio(
				MemoryMarshal.Cast<Rgba32, ColorRgba32>(originalPixels),
				MemoryMarshal.Cast<Rgba32, ColorRgba32>(pixels));
			AssertPSNR(psnr, quality);
		}

		public static void AssertPixelsEqual(Span<ColorRgbFloat> originalPixels, Span<ColorRgbFloat> pixels, CompressionQuality quality, ITestOutputHelper output = null)
		{
			var rmse = ImageQuality.CalculateLogRMSE(originalPixels,pixels);
			AssertRMSE(rmse, quality, output);
		}

		public static void AssertImagesEqual(Image<Rgba32> original, Image<Rgba32> image, CompressionQuality quality, bool countAlpha = true)
		{
			var psnr = CalculatePSNR(original, image, countAlpha);
			AssertPSNR(psnr, quality);
		}

		#endregion

		#region Execute methods

		public static void ExecuteDecodingTest(KtxFile file, string outputFile)
		{
			Assert.True(file.header.VerifyHeader());
			Assert.Equal((uint)1, file.header.NumberOfFaces);

			var decoder = new BcDecoder();
			using var image = decoder.DecodeToImageRgba32(file);

			Assert.Equal((uint)image.Width, file.header.PixelWidth);
			Assert.Equal((uint)image.Height, file.header.PixelHeight);

			using var outFs = File.OpenWrite(outputFile);
			image.SaveAsPng(outFs);
		}

		#region Dds

		public static void ExecuteDdsWritingTest(Image<Rgba32> image, CompressionFormat format, string outputFile)
		{
			ExecuteDdsWritingTest(new[] { image }, format, outputFile);
		}

		public static void ExecuteDdsWritingTest(Image<Rgba32>[] images, CompressionFormat format, string outputFile)
		{
			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = format;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

			using var fs = File.OpenWrite(outputFile);

			if (images.Length == 1)
			{
				encoder.EncodeToStream(images[0], fs);
			}
			else
			{
				encoder.EncodeCubeMapToStream(images[0], images[1], images[2], images[3], images[4], images[5], fs);
			}
		}

		public static void ExecuteDdsReadingTest(DdsFile file, DxgiFormat format, string outputFile, bool assertAlpha = false)
		{
			Assert.Equal(format, file.header.ddsPixelFormat.DxgiFormat);
			Assert.Equal(file.header.dwMipMapCount, (uint)file.Faces[0].MipMaps.Length);

			var decoder = new BcDecoder();
			decoder.InputOptions.DdsBc1ExpectAlpha = assertAlpha;
			var images = decoder.DecodeAllMipMapsToImageRgba32(file);

			Assert.Equal((uint)images[0].Width, file.header.dwWidth);
			Assert.Equal((uint)images[0].Height, file.header.dwHeight);

			for (var i = 0; i < images.Length; i++)
			{
				if (assertAlpha)
				{
					var pixels = GetSinglePixelArrayAsColors(images[0]);
					Assert.Contains(pixels, x => x.a == 0);
				}

				using var outFs = File.OpenWrite(string.Format(outputFile, i));
				images[i].SaveAsPng(outFs);
				images[i].Dispose();
			}
		}

		#endregion

		#region Cancellation

		public static async Task ExecuteCancellationTest(Image<Rgba32> image, bool isParallel)
		{
			var encoder = new BcEncoder(CompressionFormat.Bc7);
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.Options.IsParallel = isParallel;

			var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
				encoder.EncodeToRawBytesAsync(image, 0, source.Token));
		}

		#endregion

		#endregion

		public static float DecodeKtxCheckPSNR(string filename, Image<Rgba32> original)
		{
			using var fs = File.OpenRead(filename);
			var ktx = KtxFile.Load(fs);
			var decoder = new BcDecoder()
			{
				OutputOptions = { Bc4Component = ColorComponent.Luminance }
			};
			using var img = decoder.DecodeToImageRgba32(ktx);

			return CalculatePSNR(original, img);
		}

		public static float DecodeKtxCheckRMSEHdr(string filename, HdrImage original)
		{
			using var fs = File.OpenRead(filename);
			var ktx = KtxFile.Load(fs);
			var decoder = new BcDecoder()
			{
			};
			
			var decoded = decoder.DecodeHdr(ktx);

			return ImageQuality.CalculateLogRMSE(original.pixels, decoded);
		}


		public static void ExecuteEncodingTest(Image<Rgba32> image, CompressionFormat format, CompressionQuality quality, string filename, ITestOutputHelper output)
		{
			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = quality;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = format;

			var fs = File.OpenWrite(filename);
			encoder.EncodeToStream(image, fs);
			fs.Close();

			var psnr = DecodeKtxCheckPSNR(filename, image);
			output.WriteLine("RGBA PSNR: " + psnr + "db");
			AssertPSNR(psnr, encoder.OutputOptions.Quality);
		}

		public static void ExecuteHdrEncodingTest(HdrImage image, CompressionFormat format, CompressionQuality quality, string filename, ITestOutputHelper output)
		{
			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = quality;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = format;

			var fs = File.OpenWrite(filename);
			encoder.EncodeToStreamHdr(image.pixels.AsMemory().AsMemory2D(image.height, image.width), fs);
			fs.Close();

			var rmse = DecodeKtxCheckRMSEHdr(filename, image);
			output.WriteLine("RGBFloat RMSE: " + rmse);
			AssertRMSE(rmse, encoder.OutputOptions.Quality);
		}

		private static float CalculatePSNR(Image<Rgba32> original, Image<Rgba32> decoded, bool countAlpha = true)
		{
			var pixels  = GetSinglePixelArrayAsColors(original);
			var pixels2 = GetSinglePixelArrayAsColors(decoded);
			
			return ImageQuality.PeakSignalToNoiseRatio(pixels, pixels2, countAlpha);
		}

		public static void AssertPSNR(float psnr, CompressionQuality quality)
		{
			if (quality == CompressionQuality.Fast)
			{
				Assert.True(psnr > 25);
			}
			else
			{
				Assert.True(psnr > 30);
			}
		}

		public static void AssertRMSE(float rmse, CompressionQuality quality, ITestOutputHelper output = null)
		{
			output?.WriteLine($"RMSE: {rmse} , quality: {quality}");
			if (quality == CompressionQuality.Fast)
			{
				Assert.True(rmse < 0.1);
			}
			else
			{
				Assert.True(rmse < 0.04);
			}
		}

		public static ColorRgba32[] GetSinglePixelArrayAsColors(Image<Rgba32> original)
		{
			ColorRgba32[] pixels = new ColorRgba32[original.Width * original.Height];
			for (var y = 0; y < original.Height; y++)
			{
				for (var x = 0; x < original.Width; x++)
				{
					var oPixel = original[x, y];
					pixels[y * original.Width + x] = new ColorRgba32(oPixel.R, oPixel.G, oPixel.B, oPixel.A);
				}
			}
			return pixels;
		}

		public static T[] GetSinglePixelArray<T>(Image<T> original) where T : unmanaged, IPixel<T>
		{
			T[] pixels = new T[original.Width * original.Height];
			for (var y = 0; y < original.Height; y++)
			{
				for (var x = 0; x < original.Width; x++)
				{
					var oPixel = original[x, y];
					pixels[y * original.Width + x] = oPixel;
				}
			}
			return pixels;
		}

		public static void SetSinglePixelArray<T>(Image<T> dest, T[] pixels) where T : unmanaged, IPixel<T>
		{
			for (var y = 0; y < dest.Height; y++)
			{
				for (var x = 0; x < dest.Width; x++)
				{
					dest[x, y] = pixels[y * dest.Width + x];
				}
			}
		}
	}
}
