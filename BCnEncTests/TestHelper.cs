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
			var pixels = original.GetPixelSpan();
			var pixels2 = img.GetPixelSpan();

			return ImageQuality.PeakSignalToNoiseRatio(pixels, pixels2, true);
		}

		public static void ExecuteEncodingTest(Image<Rgba32> image, CompressionFormat format, CompressionQuality quality, string filename, ITestOutputHelper output) {
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = quality;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = format;

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
