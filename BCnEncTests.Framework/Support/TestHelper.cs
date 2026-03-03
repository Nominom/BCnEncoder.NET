using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using CommunityToolkit.HighPerformance;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests.Support
{
	public static class TestHelper
	{
		#region Assertions

		public static void AssertPixelsEqual(Span<ColorRgba32> originalPixels, Span<ColorRgba32> pixels, CompressionQuality quality,  ITestOutputHelper output = null)
		{
			var psnr = ImageQuality.PeakSignalToNoiseRatio(originalPixels, pixels);
			AssertPSNR(psnr, quality, output);
		}

		public static void AssertPixelsEqual(Span<ColorRgbFloat> originalPixels, Span<ColorRgbFloat> pixels, CompressionQuality quality, ITestOutputHelper output = null)
		{
			var rmse = ImageQuality.CalculateLogRMSE(originalPixels, pixels);
			AssertRMSE(rmse, quality, output);
		}

		public static void AssertImagesEqual(Memory2D<ColorRgba32> original, Memory2D<ColorRgba32> image, CompressionQuality quality, bool countAlpha = true)
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
			var pixels = decoder.Decode2D(file);

			Assert.Equal((uint)pixels.Width, file.header.PixelWidth);
			Assert.Equal((uint)pixels.Height, file.header.PixelHeight);

			using (var outFs = File.OpenWrite(outputFile))
			{
				SaveAsPng(pixels, outFs);
			}
		}

		#region Dds

		public static void ExecuteDdsWritingTest(Memory2D<ColorRgba32> image, CompressionFormat format, string outputFile)
		{
			ExecuteDdsWritingTest(new[] { image }, format, outputFile);
		}

		public static void ExecuteDdsWritingTest(Memory2D<ColorRgba32>[] images, CompressionFormat format, string outputFile)
		{
			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = format;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

			using (var fs = File.OpenWrite(outputFile))
			{
				if (images.Length == 1)
				{
					encoder.EncodeToStream(images[0], fs);
				}
				else
				{
					encoder.EncodeCubeMapToStream(images[0], images[1], images[2], images[3], images[4], images[5], fs);
				}
			}
		}

		public static void ExecuteDdsReadingTest(DdsFile file, DxgiFormat format, string outputFile, bool assertAlpha = false)
		{
			Assert.Equal(format, file.header.ddsPixelFormat.DxgiFormat);
			Assert.Equal(file.header.dwMipMapCount, (uint)file.Faces[0].MipMaps.Length);

			var decoder = new BcDecoder();
			decoder.InputOptions.DdsBc1ExpectAlpha = assertAlpha;
			var images = decoder.DecodeAllMipMaps2D(file);

			Assert.Equal((uint)images[0].Width, file.header.dwWidth);
			Assert.Equal((uint)images[0].Height, file.header.dwHeight);

			for (var i = 0; i < images.Length; i++)
			{
				if (assertAlpha)
				{
					var pixels = GetSinglePixelArrayAsColors(images[0]);
					Assert.Contains(pixels, x => x.a == 0);
				}

				using (var outFs = File.OpenWrite(string.Format(outputFile, i)))
				{
					SaveAsPng(images[i], outFs);
				}
			}
		}

		#endregion

		#region Cancellation

		public static async Task ExecuteCancellationTest(Memory2D<ColorRgba32> image, bool isParallel)
		{
			var encoder = new BcEncoder(CompressionFormat.Bc7);
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.Options.IsParallel = isParallel;

			var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
				encoder.EncodeToRawBytesAsync(image, source.Token));
		}

		#endregion

		#endregion

		public static float DecodeKtxCheckPSNR(string filename, Memory2D<ColorRgba32> original)
		{
			using (var fs = File.OpenRead(filename))
			{
				var ktx = KtxFile.Load(fs);
				var decoder = new BcDecoder()
				{
					OutputOptions = { Bc4Component = ColorComponent.Luminance }
				};
				var decoded = decoder.Decode2D(ktx);

				return CalculatePSNR(original, decoded);
			}
		}

		public static float DecodeKtxCheckRMSEHdr(string filename, HdrImage original)
		{
			using (var fs = File.OpenRead(filename))
			{
				var ktx = KtxFile.Load(fs);
				var decoder = new BcDecoder();

				var decoded = decoder.DecodeHdr(ktx);

				return ImageQuality.CalculateLogRMSE(original.pixels, decoded);
			}
		}

		public static void ExecuteEncodingTest(Memory2D<ColorRgba32> image, CompressionFormat format, CompressionQuality quality, string filename, ITestOutputHelper output)
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

		private static float CalculatePSNR(Memory2D<ColorRgba32> original, Memory2D<ColorRgba32> decoded, bool countAlpha = true)
		{
			var pixels  = GetSinglePixelArrayAsColors(original);
			var pixels2 = GetSinglePixelArrayAsColors(decoded);

			return ImageQuality.PeakSignalToNoiseRatio(pixels, pixels2, countAlpha);
		}

		public static void AssertPSNR(float psnr, CompressionQuality quality, ITestOutputHelper output = null)
		{
			output?.WriteLine($"PSNR: {psnr} , quality: {quality}");
			if (quality == CompressionQuality.Fast)
			{
				Assert.True(psnr > 25, $"PSNR was less than 25: {psnr} , quality: {quality}");
			}
			else
			{
				Assert.True(psnr > 30, $"PSNR was less than 30: {psnr} , quality: {quality}");
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

		public static ColorRgba32[] GetSinglePixelArrayAsColors(ReadOnlyMemory2D<ColorRgba32> image)
		{
			var pixels = new ColorRgba32[image.Width * image.Height];
			var span = image.Span;
			for (var y = 0; y < image.Height; y++)
			{
				for (var x = 0; x < image.Width; x++)
				{
					pixels[y * image.Width + x] = span[y, x];
				}
			}
			return pixels;
		}

		public static unsafe void SaveAsPng(Memory2D<ColorRgba32> image, Stream stream)
		{
			int width = image.Width;
			int height = image.Height;
			using (var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
			{
				var data = bmp.LockBits(new Rectangle(0, 0, width, height),
					ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				byte* ptr = (byte*)data.Scan0;
				var span = image.Span;
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						var c = span[y, x];
						ptr[0] = c.b;
						ptr[1] = c.g;
						ptr[2] = c.r;
						ptr[3] = c.a;
						ptr += 4;
					}
				}
				bmp.UnlockBits(data);
				bmp.Save(stream, ImageFormat.Png);
			}
		}

		public static void SaveAsPng(ColorRgbFloat[] pixels, int width, int height, Stream stream)
		{
			var rgba = new ColorRgba32[pixels.Length];
			for (var i = 0; i < pixels.Length; i++)
			{
				var p = pixels[i];
				byte r = (byte)(Math.Max(0, Math.Min(1, p.r)) * 255 + 0.5f);
				byte g = (byte)(Math.Max(0, Math.Min(1, p.g)) * 255 + 0.5f);
				byte b = (byte)(Math.Max(0, Math.Min(1, p.b)) * 255 + 0.5f);
				rgba[i] = new ColorRgba32(r, g, b, 255);
			}
			var mem = new Memory2D<ColorRgba32>(rgba, height, width);
			SaveAsPng(mem, stream);
		}
	}
}
