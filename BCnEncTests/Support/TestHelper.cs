using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Decoder.Options;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using BCnEncoder.TextureFormats;
using CommunityToolkit.HighPerformance;
using SharpEXR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
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

		public static void AssertPixelsEqual(ReadOnlyMemory2D<ColorRgbaFloat> expected,
			ReadOnlyMemory2D<ColorRgbaFloat> actual, string channelMask, float tolerance)
		{
			Assert.Equal(expected.Height, actual.Height);
			Assert.Equal(expected.Width, actual.Width);

			ReadOnlySpan2D<ColorRgbaFloat> expectedSpan = expected.Span;
			ReadOnlySpan2D<ColorRgbaFloat> actualSpan = actual.Span;

			for (int y = 0; y < expected.Height; y++)
			{
				for (int x = 0; x < expected.Width; x++)
				{
					ColorRgbaFloat expectedPixel = expectedSpan[y, x];
					ColorRgbaFloat actualPixel = actualSpan[y, x];

					if ((channelMask.Contains('r') && Math.Abs(expectedPixel.r - actualPixel.r) > tolerance) ||
					    (channelMask.Contains('g') && Math.Abs(expectedPixel.g - actualPixel.g) > tolerance) ||
					    (channelMask.Contains('b') && Math.Abs(expectedPixel.b - actualPixel.b) > tolerance) ||
					    (channelMask.Contains('a') && Math.Abs(expectedPixel.a - actualPixel.a) > tolerance))
					{
						ColorRgba32 expectedPixel32 = expectedPixel.As<ColorRgba32>();
						ColorRgba32 actualPixel32 = actualPixel.As<ColorRgba32>();

						Assert.Fail($"Pixels != with tolerance: {tolerance} at (x: {x}, y: {y})\nExpected: {expectedPixel}\nActual: {actualPixel}\nExpected32: {expectedPixel32}\nActual32: {actualPixel32}\nMask: {channelMask}");
					}
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

		public static void TestDecoding<TTexture>(string testImage, TTexture texture, ReadOnlyMemory2D<ColorRgbaFloat> reference, bool referenceIsLdr, BcDecoder decoder, string outFileFormat = null, float tolerance = 0)
			where TTexture : ITextureFileFormat
		{
			var bcnData = texture.ToTextureData();

			string channelMask = GetChannelMask(bcnData.Format);

			Assert.True(bcnData.Format != CompressionFormat.Unknown, $"Unknown format for {testImage}");
			Assert.True(bcnData.Width > 0, $"Invalid width for {testImage}");
			Assert.True(bcnData.Height > 0, $"Invalid height for {testImage}");
			Assert.True(bcnData.IsValid(), $"Invalid data for {testImage}");
			Assert.Equal(testImage.Contains("srgb"), bcnData.Format.IsSRGBFormat());

			BCnTextureData decoded;

			if (referenceIsLdr)
			{
				// Roundtrip from Rgba32, to match reference images.
				decoded = decoder.Decode(bcnData, CompressionFormat.Rgba32);
				decoded = decoded.ConvertTo(CompressionFormat.RgbaFloat);
			}
			else
			{
				decoded = decoder.Decode(bcnData, CompressionFormat.RgbaFloat);
			}

			if (!string.IsNullOrEmpty(outFileFormat))
			{
				for (var i = 0; i < decoded.NumMips; i++)
				{
					var mipDecoded = decoded.AsImage(i);
					mipDecoded.SaveAsPng(string.Format(outFileFormat, i));
				}
			}

			AssertPixelsEqual(reference, decoded.First.AsMemory2D<ColorRgbaFloat>(), channelMask, tolerance);
		}

		private static string GetChannelMask(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R8:
				case CompressionFormat.R8S:
				case CompressionFormat.Bc4:
				case CompressionFormat.Bc4S:
					return "r";
				case CompressionFormat.R8G8:
				case CompressionFormat.R8G8S:
				case CompressionFormat.Bc5:
				case CompressionFormat.Bc5S:
					return "rg";
			}

			return format.SupportsAlpha() ? "rgba" : "rgb";
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

		public static Memory2D<ColorRgbaFloat> GetReferenceImage(string filePath)
		{
			var imageSharpExtensions = new string[] { ".png", ".jpg", ".jpeg", ".bmp" };
			var openExrExtensions = new string[] { ".exr" };
			var radianceExtensions = new string[] { ".hdr" };

			var extension = Path.GetExtension(filePath);

			if (imageSharpExtensions.Contains(extension))
			{
				var image = Image.Load<RgbaVector>(filePath);

				// Convert to linear rgb
				// for (int y = 0; y < image.Height; y++)
				// {
				// 	Memory<RgbaVector> row = image.DangerousGetPixelRowMemory(y);
				//
				// 	for (int x = 0; x < image.Width; x++)
				// 	{
				// 		var pixel = row.Span[x];
				//
				// 		Rgb rgb = new Rgb(pixel.R, pixel.G, pixel.B);
				// 		LinearRgb linearRgb = ColorSpaceConverter.ToLinearRgb(rgb);
				// 		pixel.R = linearRgb.R;
				// 		pixel.G = linearRgb.G;
				// 		pixel.B = linearRgb.B;
				//
				// 		row.Span[x] = pixel;
				// 	}
				// }

				return image.ToBCnTextureData().First.AsMemory2D<ColorRgbaFloat>();
			}
			if (openExrExtensions.Contains(extension))
			{
				var exrFile = EXRFile.FromFile(filePath);
				var part = exrFile.Parts[0];

				part.Open(filePath);

				float[] exrData = part.GetFloats(ChannelConfiguration.RGB, false, GammaEncoding.Linear, true);

				Memory<ColorRgbaFloat> rgbaData = exrData.AsMemory().Cast<float, ColorRgbaFloat>();

				return rgbaData.AsMemory2D(part.DataWindow.Height, part.DataWindow.Width);
			}

			throw new ArgumentException($"Unsupported image format: {extension}");
		}

		public static string GetProjectRoot()
		{
			// Get the directory where the assembly is located
			string assemblyLocation = Path.GetDirectoryName(typeof(DecodingTests).Assembly.Location);

			string projectRoot = assemblyLocation;

			// Go up to the project folder (or however many levels needed)
			do
			{
				if (Path.GetPathRoot(projectRoot) == projectRoot)
					throw new Exception("Could not find project root");

				projectRoot = Path.GetFullPath(Path.Combine(projectRoot, ".."));
			} while(!Directory.Exists(Path.Combine(projectRoot, "bin")));

			return projectRoot;
		}
	}
}
