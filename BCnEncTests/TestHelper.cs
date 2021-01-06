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
using Rgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;

namespace BCnEncTests
{
	public static class TestHelper
	{

		public static byte[] ToBytes(this Rgba32[] rgba)
		{
			byte[] output = new byte[rgba.Length * 4];
			for (int i = 0; i < rgba.Length; i++)
			{
				output[i * 4 + 0] = rgba[i].R;
				output[i * 4 + 1] = rgba[i].G;
				output[i * 4 + 2] = rgba[i].B;
				output[i * 4 + 3] = rgba[i].A;
			}
			return output;
		}

		public static BCnEncoder.Shared.Rgba32 ToBcn(this Rgba32 rgba)
		{
			return new BCnEncoder.Shared.Rgba32(rgba.R, rgba.G, rgba.B, rgba.A);
		}

		public static BCnEncoder.Shared.Rgba32[] ToBcn(this Span<Rgba32> rgba)
		{
			BCnEncoder.Shared.Rgba32[] output = new BCnEncoder.Shared.Rgba32[rgba.Length];
			for (int i = 0; i < rgba.Length; i++)
			{
				output[i] = rgba[i].ToBcn();
			}
			return output;
		}

		public static byte[] ToBytes(this Span<Rgba32> rgba)
		{
			byte[] output = new byte[rgba.Length * 4];
			for (int i = 0; i < rgba.Length; i++)
			{
				output[i * 4 + 0] = rgba[i].R;
				output[i * 4 + 1] = rgba[i].G;
				output[i * 4 + 2] = rgba[i].B;
				output[i * 4 + 3] = rgba[i].A;
			}
			return output;
		}

		public static Rgba32[] ToRgba(this byte[] rgba)
		{
			Rgba32[] output = new Rgba32[rgba.Length / 4];
			for (int i = 0; i < output.Length; i++)
			{
				output[i] = new Rgba32
				(
					rgba[i * 4 + 0],
					rgba[i * 4 + 1],
					rgba[i * 4 + 2],
					rgba[i * 4 + 3]
				);
			}
			return output;
		}

		public static float DecodeCheckPSNR(string filename, Image<Rgba32> original) {
			using FileStream fs = File.OpenRead(filename);
			var ktx = KtxFile.Load(fs);
			var decoder = new BcDecoder();
			var img = decoder.Decode(ktx);

			if (!original.TryGetSinglePixelSpan(out var pixels)) {
				throw new Exception("Cannot get pixel span.");
			}

			return ImageQuality.PeakSignalToNoiseRatio(pixels, img.data.ToRgba(), true);
		}

		public static void ExecuteEncodingTest(Image<Rgba32> image, CompressionFormat format, CompressionQuality quality, string filename, ITestOutputHelper output) {
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = quality;
			encoder.OutputOptions.format = format;

			using FileStream fs = File.OpenWrite(filename);
			image.TryGetSinglePixelSpan(out var span);
			encoder.Encode(span.ToBytes(), image.Width, image.Height, fs);
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
