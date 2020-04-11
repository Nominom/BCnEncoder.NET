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
		public void Bc1GradientBestQuality()
		{
			var image = ImageLoader.testGradient1;
			
			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC1,
				EncodingQuality.BestQuality, 
				"encoding_bc1_gradient_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc1GradientBalanced()
		{
			var image = ImageLoader.testGradient1;

			
			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC1,
				EncodingQuality.Balanced, 
				"encoding_bc1_gradient_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc1GradientFast()
		{
			var image = ImageLoader.testGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC1,
				EncodingQuality.Fast, 
				"encoding_bc1_gradient_fast.ktx",
				output);
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
		public void Bc1DiffuseBestQuality()
		{
			var image = ImageLoader.testDiffuse1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC1,
				EncodingQuality.BestQuality, 
				"encoding_bc1_diffuse_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc1DiffuseBalanced()
		{
			var image = ImageLoader.testDiffuse1;
			
			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC1,
				EncodingQuality.Balanced, 
				"encoding_bc1_diffuse_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc1DiffuseFast()
		{
			var image = ImageLoader.testDiffuse1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC1,
				EncodingQuality.Fast, 
				"encoding_bc1_diffuse_fast.ktx",
				output);
		}


	}


	public class Bc1BlurryTest
	{
		private readonly ITestOutputHelper output;

		public Bc1BlurryTest(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void Bc1BlurBestQuality()
		{
			var image = ImageLoader.testBlur1;
			
			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC1,
				EncodingQuality.BestQuality, 
				"encoding_bc1_blur_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc1BlurBalanced()
		{
			var image = ImageLoader.testBlur1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC1,
				EncodingQuality.Balanced, 
				"encoding_bc1_blur_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc1BlurFast()
		{
			var image = ImageLoader.testBlur1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC1,
				EncodingQuality.Fast, 
				"encoding_bc1_blur_fast.ktx",
				output);
		}

	}

	public class Bc1ASpriteTest
	{

		private readonly ITestOutputHelper output;

		public Bc1ASpriteTest(ITestOutputHelper output)
		{
			this.output = output;
		}


		[Fact]
		public void Bc1aSpriteBestQuality()
		{
			var image = ImageLoader.testTransparentSprite1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC1WithAlpha,
				EncodingQuality.BestQuality, 
				"encoding_bc1a_sprite_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc1aSpriteBalanced()
		{
			var image = ImageLoader.testTransparentSprite1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC1WithAlpha,
				EncodingQuality.Balanced, 
				"encoding_bc1a_sprite_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc1aSpriteFast()
		{
			var image = ImageLoader.testTransparentSprite1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC1WithAlpha,
				EncodingQuality.Fast, 
				"encoding_bc1a_sprite_fast.ktx",
				output);
		}

	}

	public class Bc2GradientTest
	{

		private readonly ITestOutputHelper output;

		public Bc2GradientTest(ITestOutputHelper output)
		{
			this.output = output;
		}


		[Fact]
		public void Bc2GradientBestQuality()
		{
			var image = ImageLoader.testAlphaGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC2,
				EncodingQuality.BestQuality, 
				"encoding_bc2_gradient_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc2GradientBalanced()
		{
			var image = ImageLoader.testAlphaGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC2,
				EncodingQuality.Balanced, 
				"encoding_bc2_gradient_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc2GradientFast()
		{
			var image = ImageLoader.testAlphaGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC2,
				EncodingQuality.Fast, 
				"encoding_bc2_gradient_fast.ktx",
				output);
		}

	}

	public class Bc3GradientTest
	{

		private readonly ITestOutputHelper output;

		public Bc3GradientTest(ITestOutputHelper output)
		{
			this.output = output;
		}


		[Fact]
		public void Bc3GradientBestQuality()
		{
			var image = ImageLoader.testAlphaGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC3,
				EncodingQuality.BestQuality, 
				"encoding_bc3_gradient_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc3GradientBalanced()
		{
			var image = ImageLoader.testAlphaGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC3,
				EncodingQuality.Balanced, 
				"encoding_bc3_gradient_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc3GradientFast()
		{
			var image = ImageLoader.testAlphaGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.BC3,
				EncodingQuality.Fast, 
				"encoding_bc3_gradient_fast.ktx",
				output);
		}

	}
}
