using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests
{
	public static class TestHelper
	{
		public static float DecodeCheckPSNR(string filename, Image<Rgba32> original) {
			using FileStream fs = File.OpenRead(filename);
			var ktx = KtxFile.Load(fs);
			var decoder = new BcDecoder();
			using var img = decoder.Decode(ktx);

			if (!original.TryGetSinglePixelSpan(out var pixels)) {
				throw new Exception("Cannot get pixel span.");
			}
			if (!img.TryGetSinglePixelSpan(out var pixels2)) {
				throw new Exception("Cannot get pixel span.");
			}

			return ImageQuality.PeakSignalToNoiseRatio(pixels, pixels2, true);
		}

		public static void ExecuteEncodingTest(Image<Rgba32> image, CompressionFormat format, CompressionQuality quality, string filename, ITestOutputHelper output) {
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.Quality = quality;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = format;

			using FileStream fs = File.OpenWrite(filename);
			encoder.Encode(image, fs);
			fs.Close();
			var psnr = TestHelper.DecodeCheckPSNR(filename, image);
			output.WriteLine("RGBA PSNR: " + psnr + "db");
			if(quality == CompressionQuality.Fast)
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
