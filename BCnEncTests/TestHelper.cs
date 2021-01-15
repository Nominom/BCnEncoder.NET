using System;
using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests
{
	public static class TestHelper
	{
		public static float DecodeCheckPSNR(string filename, Image<Rgba32> original)
		{
			using var fs = File.OpenRead(filename);
			var ktx = KtxFile.Load(fs);
			var decoder = new BcDecoder();
			using var img = decoder.Decode(ktx);

			return CalculatePSNR(original, img);
		}

		public static float CalculatePSNR(Image<Rgba32> original, Image<Rgba32> decoded)
		{
			if (!original.TryGetSinglePixelSpan(out var pixels))
			{
				throw new Exception("Cannot get pixel span.");
			}
			if (!decoded.TryGetSinglePixelSpan(out var pixels2))
			{
				throw new Exception("Cannot get pixel span.");
			}

			return ImageQuality.PeakSignalToNoiseRatio(pixels, pixels2);
		}

		public static void ExecuteEncodingTest(Image<Rgba32> image, CompressionFormat format, CompressionQuality quality, string filename, ITestOutputHelper output)
		{
			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = quality;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = format;

			using var fs = File.OpenWrite(filename);
			encoder.Encode(image, fs);
			fs.Close();
			var psnr = TestHelper.DecodeCheckPSNR(filename, image);
			output.WriteLine("RGBA PSNR: " + psnr + "db");
			if (quality == CompressionQuality.Fast)
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
