using System.IO;
using BCnComp.Net.Encoder;
using Xunit;

namespace BCnEncTests
{
	public class Bc1GradientTest
	{
		[Fact]
		public void Bc1GradientBestQuality() {
			var image = ImageLoader.testGradient1;
			
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.BestQuality;
			using FileStream fs = File.OpenWrite("encoding_bc1_gradient_bestQuality.ktx");
			encoder.Encode(image, fs);
		}

		[Fact]
		public void Bc1GradientBalanced() {
			var image = ImageLoader.testGradient1;
			
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Balanced;
			using FileStream fs = File.OpenWrite("encoding_bc1_gradient_balanced.ktx");
			encoder.Encode(image, fs);
		}

		[Fact]
		public void Bc1GradientFast() {
			var image = ImageLoader.testGradient1;
			
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Fast;
			using FileStream fs = File.OpenWrite("encoding_bc1_gradient_fast.ktx");
			encoder.Encode(image, fs);
		}
	}

	public class Bc1DiffuseTest
	{
		[Fact]
		public void Bc1DiffuseBestQuality() {
			var image = ImageLoader.testDiffuse1;
			
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.BestQuality;
			using FileStream fs = File.OpenWrite("encoding_bc1_diffuse_bestQuality.ktx");
			encoder.Encode(image, fs);
		}

		[Fact]
		public void Bc1DiffuseBalanced() {
			var image = ImageLoader.testDiffuse1;
			
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Balanced;
			using FileStream fs = File.OpenWrite("encoding_bc1_diffuse_balanced.ktx");
			encoder.Encode(image, fs);
		}

		[Fact]
		public void Bc1DiffuseFast() {
			var image = ImageLoader.testDiffuse1;
			
			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Fast;
			using FileStream fs = File.OpenWrite("encoding_bc1_diffuse_fast.ktx");
			encoder.Encode(image, fs);
		}
	}
}
