using System.IO;
using BCnEnc.Net.Decoder;
using BCnEnc.Net.Encoder;
using BCnEnc.Net.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests
{
	public class Bc1GradientTest
	{
		private readonly ITestOutputHelper output;

		public Bc1GradientTest(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void Bc1GradientBestQuality() {
			var image = ImageLoader.testGradient1;
			
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.BestQuality;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC1;

			using FileStream fs = File.OpenWrite("encoding_bc1_gradient_bestQuality.ktx");
			encoder.Encode(image, fs);
			fs.Close();
			var psnr = DecodeCheckPSNR("encoding_bc1_gradient_bestQuality.ktx", image);
			output.WriteLine("PSNR: "+psnr+"db");
		}

		[Fact]
		public void Bc1GradientBalanced() {
			var image = ImageLoader.testGradient1;
			
			BcEncoder encoder = new BcEncoder();

			encoder.OutputOptions.quality = EncodingQuality.Balanced;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC1;

			using FileStream fs = File.OpenWrite("encoding_bc1_gradient_balanced.ktx");
			encoder.Encode(image, fs);
			fs.Close();
			var psnr = DecodeCheckPSNR("encoding_bc1_gradient_balanced.ktx", image);
			output.WriteLine("PSNR: "+psnr+"db");
		}

		[Fact]
		public void Bc1GradientFast() {
			var image = ImageLoader.testGradient1;
			
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Fast;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC1;

			using FileStream fs = File.OpenWrite("encoding_bc1_gradient_fast.ktx");
			encoder.Encode(image, fs);
			fs.Close();
			var psnr = DecodeCheckPSNR("encoding_bc1_gradient_fast.ktx", image);
			output.WriteLine("PSNR: "+psnr+"db");
		}

		private float DecodeCheckPSNR(string filename, Image<Rgba32> original) {
			using FileStream fs = File.OpenRead(filename);
			var ktx = KtxFile.Load(fs);
			var decoder = new BcDecoder();
			using var img = decoder.Decode(ktx);
			var pixels = original.GetPixelSpan();
			var pixels2 = img.GetPixelSpan();

			return ImageQuality.PeakSignalToNoiseRatio(pixels, pixels2, false);
		}
	}

	public class Bc1DiffuseTest
	{

		private readonly ITestOutputHelper output;

		public Bc1DiffuseTest(ITestOutputHelper output)
		{
			this.output = output;
		}


		[Fact]
		public void Bc1DiffuseBestQuality() {
			var image = ImageLoader.testDiffuse1;
			
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.BestQuality;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC1;

			using FileStream fs = File.OpenWrite("encoding_bc1_diffuse_bestQuality.ktx");
			encoder.Encode(image, fs);
			fs.Close();
			var psnr = DecodeCheckPSNR("encoding_bc1_diffuse_bestQuality.ktx", image);
			output.WriteLine("PSNR: "+psnr+"db");
		}

		[Fact]
		public void Bc1DiffuseBalanced() {
			var image = ImageLoader.testDiffuse1;
			
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Balanced;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC1;

			using FileStream fs = File.OpenWrite("encoding_bc1_diffuse_balanced.ktx");
			encoder.Encode(image, fs);
			fs.Close();
			var psnr = DecodeCheckPSNR("encoding_bc1_diffuse_balanced.ktx", image);
			output.WriteLine("PSNR: "+psnr+"db");
		}

		[Fact]
		public void Bc1DiffuseFast() {
			var image = ImageLoader.testDiffuse1;
			
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Fast;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC1;

			using FileStream fs = File.OpenWrite("encoding_bc1_diffuse_fast.ktx");
			encoder.Encode(image, fs);
			fs.Close();
			var psnr = DecodeCheckPSNR("encoding_bc1_diffuse_fast.ktx", image);
			output.WriteLine("PSNR: "+psnr+"db");
		}

		private float DecodeCheckPSNR(string filename, Image<Rgba32> original) {
			using FileStream fs = File.OpenRead(filename);
			var ktx = KtxFile.Load(fs);
			var decoder = new BcDecoder();
			using var img = decoder.Decode(ktx);
			var pixels = original.GetPixelSpan();
			var pixels2 = img.GetPixelSpan();

			return ImageQuality.PeakSignalToNoiseRatio(pixels, pixels2, false);
		}
	}
}
