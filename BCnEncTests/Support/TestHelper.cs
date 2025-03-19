using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using BCnEncoder.TextureFormats;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace BCnEncTests.Support
{
	public static class TestHelper
	{
		#region Assertions

		public static void AssertImagesEqual<TPixel>(Image<TPixel> expected, Image<TPixel> actual, int tolerance = 0)
			where TPixel : unmanaged, IPixel<TPixel>
		{
			Assert.Equal(expected.Width, actual.Width);
			Assert.Equal(expected.Height, actual.Height);
			var size = Unsafe.SizeOf<TPixel>();
			var bytes1 = new byte[expected.Height * expected.Width * size];
			var bytes2 = new byte[actual.Height * actual.Width * size];
			var pixels1 = bytes1.AsSpan().Cast<byte, TPixel>();
			var pixels2 = bytes2.AsSpan().Cast<byte, TPixel>();

			expected.CopyPixelDataTo(bytes1);
			actual.CopyPixelDataTo(bytes2);

			for (var i = 0; i < bytes1.Length; i++)
			{
				if (Math.Abs(bytes1[i] - bytes2[i]) > tolerance)
				{
					var tPixel1 = pixels1[i / size];
					var tPixel2 = pixels2[i / size];
					throw NotEqualException.ForEqualValues(tPixel1.ToString()!, tPixel2.ToString()!,
						$"Pixels were not exactly equal within tolerance of {tolerance}!");
				}
			}
		}

		public static void AssertPixelsSimilar(Span<Rgba32> originalPixels, Span<Rgba32> pixels, CompressionQuality quality, bool countAlpha = true, ITestOutputHelper output = null)
		{
			var psnr = ImageQuality.PeakSignalToNoiseRatio(
				MemoryMarshal.Cast<Rgba32, ColorRgba32>(originalPixels),
				MemoryMarshal.Cast<Rgba32, ColorRgba32>(pixels), countAlpha);
			AssertPSNR(psnr, quality, output);
		}

		public static void AssertPixelsSimilar(Span<ColorRgbaFloat> originalPixels, Span<ColorRgbaFloat> pixels, CompressionQuality quality, bool countAlpha = true, ITestOutputHelper output = null)
		{
			var rmse = ImageQuality.CalculateLogRMSE(originalPixels,pixels, countAlpha);
			AssertRMSE(rmse, quality, output);
		}

		public static void AssertImagesSimilar(Image<Rgba32> original, Image<Rgba32> image, CompressionQuality quality, bool countAlpha = true, ITestOutputHelper output = null)
		{
			Assert.Equal(original.Width, image.Width);
			Assert.Equal(original.Height, image.Height);

			var psnr = CalculatePSNR(original, image, countAlpha);
			AssertPSNR(psnr, quality, output);
		}

		public static void AssertImagesSimilar(Image<RgbaVector> original, Image<RgbaVector> image, CompressionQuality quality, bool countAlpha = true, ITestOutputHelper output = null)
		{
			Assert.Equal(original.Width, image.Width);
			Assert.Equal(original.Height, image.Height);

			var rmse = CalculateLogRMSE(original, image, countAlpha);
			AssertRMSE(rmse, quality, output);
		}

		#endregion

		public static void TestDecodingLdr<TTexture>(TTexture texture, Image<Rgba32> reference, BcDecoder decoder = null, string outMipFileFormat = null, int tolerance = 0)
			where TTexture : ITextureFileFormat
		{
			var bcnData = texture.ToTextureData();
			if (bcnData.Format == CompressionFormat.Bc1 && texture is DdsFile)
			{
				bcnData.Format = CompressionFormat.Bc1WithAlpha;
			}

			decoder ??= new BcDecoder();
			var decoded = decoder.Decode(bcnData, CompressionFormat.Rgba32_sRGB);

			Assert.Equal(CompressionFormat.Rgba32_sRGB, decoded.Format);

			if (!string.IsNullOrEmpty(outMipFileFormat))
			{
				for (var i = 0; i < decoded.NumMips; i++)
				{
					var mipDecoded = decoded.AsImageRgba32(i);
					mipDecoded.SaveAsPng(string.Format(outMipFileFormat, i));
				}
			}

			var imageDecoded = decoded.AsImageRgba32();

			AssertImagesEqual(reference, imageDecoded, tolerance);
		}

		public static void TestDecodingHdr<TTexture>(TTexture texture, Image<RgbaVector> reference, BcDecoder decoder = null, string outMipFileFormat = null)
			where TTexture : ITextureFileFormat
		{
			decoder ??= new BcDecoder();
			var decoded = decoder.Decode(texture.ToTextureData(), CompressionFormat.RgbaFloat);

			Assert.Equal(CompressionFormat.RgbaFloat, decoded.Format);

			using var imageDecoded = decoded.AsImageRgbaVector();

			AssertImagesEqual(reference, imageDecoded);

			if (!string.IsNullOrEmpty(outMipFileFormat))
			{
				for (var i = 0; i < decoded.NumMips; i++)
				{
					var mipDecoded = decoded.AsImageRgbaVector(i);
					mipDecoded.SaveAsPng(string.Format(outMipFileFormat, i));
				}
			}
		}

		public static void TestEncodingLdr<TTexture>(Image<Rgba32> original, string outFileName, CompressionFormat format, CompressionQuality quality, ITestOutputHelper output)
			where TTexture : class, ITextureFileFormat<TTexture>, new()
		{
			var encoder = new BcEncoder(format)
			{
				OutputOptions =
				{
					GenerateMipMaps = true,
					Quality = quality
				}
			};

			var texture = encoder.EncodeToTexture<TTexture>(original);
			using var fs = File.OpenWrite(outFileName);
			texture.WriteToStream(fs);

			var decoder = new BcDecoder();
			var decoded = decoder.Decode(texture.ToTextureData(), CompressionFormat.Rgba32_sRGB);

			using var imageDecoded = decoded.AsImageRgba32();

			AssertImagesSimilar(original, imageDecoded, quality, format.SupportsAlpha(), output);
		}

		public static void TestEncodingHdr<TTexture>(Image<RgbaVector> original, string outFileName, CompressionFormat format, CompressionQuality quality, ITestOutputHelper output)
			where TTexture : class, ITextureFileFormat<TTexture>, new()
		{
			var encoder = new BcEncoder(format)
			{
				OutputOptions =
				{
					GenerateMipMaps = true,
					Quality = quality
				}
			};

			var texture = encoder.EncodeToTexture<TTexture>(original.ToBCnTextureData());
			using var fs = File.OpenWrite(outFileName);
			texture.WriteToStream(fs);

			var decoder = new BcDecoder();
			var decoded = decoder.Decode(texture.ToTextureData(), CompressionFormat.RgbaFloat);

			Assert.Equal(CompressionFormat.RgbaFloat, decoded.Format);

			using var imageDecoded = decoded.AsImageRgbaVector();

			AssertImagesSimilar(original, imageDecoded, quality, format.SupportsAlpha(), output);
		}

		private static float CalculatePSNR(Image<Rgba32> original, Image<Rgba32> decoded, bool countAlpha = true)
		{
			var pixels  = GetSinglePixelArrayAsColors(original);
			var pixels2 = GetSinglePixelArrayAsColors(decoded);

			return ImageQuality.PeakSignalToNoiseRatio(pixels, pixels2, countAlpha);
		}

		private static float CalculateLogRMSE(Image<RgbaVector> original, Image<RgbaVector> decoded, bool countAlpha = true)
		{
			var pixels = GetSinglePixelArrayAsColors(original);
			var pixels2 = GetSinglePixelArrayAsColors(decoded);

			return ImageQuality.CalculateLogRMSE(pixels, pixels2, countAlpha);
		}

		public static void AssertPSNR(float psnr, CompressionQuality quality, ITestOutputHelper output = null)
		{
			output?.WriteLine($"PSNR: {psnr} , quality: {quality}");
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

		public static ColorRgbaFloat[] GetSinglePixelArrayAsColors(Image<RgbaVector> original)
		{
			var pixels = new ColorRgbaFloat[original.Width * original.Height];
			for (var y = 0; y < original.Height; y++)
			{
				for (var x = 0; x < original.Width; x++)
				{
					var oPixel = original[x, y];
					pixels[y * original.Width + x] = new ColorRgbaFloat(oPixel.R, oPixel.G, oPixel.B, oPixel.A);
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
