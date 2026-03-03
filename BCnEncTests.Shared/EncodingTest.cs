using System.IO;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using CommunityToolkit.HighPerformance;
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
			TestHelper.ExecuteEncodingTest(ImageLoader.TestGradient1, CompressionFormat.Bc1, CompressionQuality.BestQuality, "encoding_bc1_gradient_bestQuality.ktx", output);
		}

		[Fact]
		public void Bc1GradientBalanced()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestGradient1, CompressionFormat.Bc1, CompressionQuality.Balanced, "encoding_bc1_gradient_balanced.ktx", output);
		}

		[Fact]
		public void Bc1GradientFast()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestGradient1, CompressionFormat.Bc1, CompressionQuality.Fast, "encoding_bc1_gradient_fast.ktx", output);
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
			TestHelper.ExecuteEncodingTest(ImageLoader.TestDiffuse1, CompressionFormat.Bc1, CompressionQuality.BestQuality, "encoding_bc1_diffuse_bestQuality.ktx", output);
		}

		[Fact]
		public void Bc1DiffuseBalanced()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestDiffuse1, CompressionFormat.Bc1, CompressionQuality.Balanced, "encoding_bc1_diffuse_balanced.ktx", output);
		}

		[Fact]
		public void Bc1DiffuseFast()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestDiffuse1, CompressionFormat.Bc1, CompressionQuality.Fast, "encoding_bc1_diffuse_fast.ktx", output);
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
			TestHelper.ExecuteEncodingTest(ImageLoader.TestBlur1, CompressionFormat.Bc1, CompressionQuality.BestQuality, "encoding_bc1_blur_bestQuality.ktx", output);
		}

		[Fact]
		public void Bc1BlurBalanced()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestBlur1, CompressionFormat.Bc1, CompressionQuality.Balanced, "encoding_bc1_blur_balanced.ktx", output);
		}

		[Fact]
		public void Bc1BlurFast()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestBlur1, CompressionFormat.Bc1, CompressionQuality.Fast, "encoding_bc1_blur_fast.ktx", output);
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
		public void Bc1ASpriteBestQuality()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestTransparentSprite1, CompressionFormat.Bc1WithAlpha, CompressionQuality.BestQuality, "encoding_bc1a_sprite_bestQuality.ktx", output);
		}

		[Fact]
		public void Bc1ASpriteBalanced()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestTransparentSprite1, CompressionFormat.Bc1WithAlpha, CompressionQuality.Balanced, "encoding_bc1a_sprite_balanced.ktx", output);
		}

		[Fact]
		public void Bc1ASpriteFast()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestTransparentSprite1, CompressionFormat.Bc1WithAlpha, CompressionQuality.Fast, "encoding_bc1a_sprite_fast.ktx", output);
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
			TestHelper.ExecuteEncodingTest(ImageLoader.TestAlphaGradient1, CompressionFormat.Bc2, CompressionQuality.BestQuality, "encoding_bc2_gradient_bestQuality.ktx", output);
		}

		[Fact]
		public void Bc2GradientBalanced()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestAlphaGradient1, CompressionFormat.Bc2, CompressionQuality.Balanced, "encoding_bc2_gradient_balanced.ktx", output);
		}

		[Fact]
		public void Bc2GradientFast()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestAlphaGradient1, CompressionFormat.Bc2, CompressionQuality.Fast, "encoding_bc2_gradient_fast.ktx", output);
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
			TestHelper.ExecuteEncodingTest(ImageLoader.TestAlphaGradient1, CompressionFormat.Bc3, CompressionQuality.BestQuality, "encoding_bc3_gradient_bestQuality.ktx", output);
		}

		[Fact]
		public void Bc3GradientBalanced()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestAlphaGradient1, CompressionFormat.Bc3, CompressionQuality.Balanced, "encoding_bc3_gradient_balanced.ktx", output);
		}

		[Fact]
		public void Bc3GradientFast()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestAlphaGradient1, CompressionFormat.Bc3, CompressionQuality.Fast, "encoding_bc3_gradient_fast.ktx", output);
		}
	}

	public class Bc4RedTest
	{
		private readonly ITestOutputHelper output;

		public Bc4RedTest(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void Bc4RedBestQuality()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestHeight1, CompressionFormat.Bc4, CompressionQuality.BestQuality, "encoding_bc4_red_bestQuality.ktx", output);
		}

		[Fact]
		public void Bc4RedBalanced()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestHeight1, CompressionFormat.Bc4, CompressionQuality.Balanced, "encoding_bc4_red_balanced.ktx", output);
		}

		[Fact]
		public void Bc4RedFast()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestHeight1, CompressionFormat.Bc4, CompressionQuality.Fast, "encoding_bc4_red_fast.ktx", output);
		}
	}

	public class Bc5RedGreenTest
	{
		private readonly ITestOutputHelper output;

		public Bc5RedGreenTest(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void Bc5RedGreenBestQuality()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestRedGreen1, CompressionFormat.Bc5, CompressionQuality.BestQuality, "encoding_bc5_red_green_bestQuality.ktx", output);
		}

		[Fact]
		public void Bc5RedGreenBalanced()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestRedGreen1, CompressionFormat.Bc5, CompressionQuality.Balanced, "encoding_bc5_red_green_balanced.ktx", output);
		}

		[Fact]
		public void Bc5RedGreenFast()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestRedGreen1, CompressionFormat.Bc5, CompressionQuality.Fast, "encoding_bc5_red_green_fast.ktx", output);
		}
	}

	public class Bc7RgbTest
	{
		private readonly ITestOutputHelper output;

		public Bc7RgbTest(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void Bc7RgbBestQuality()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestRgbHard1, CompressionFormat.Bc7, CompressionQuality.BestQuality, "encoding_bc7_rgb_bestQuality.ktx", output);
		}

		[Fact]
		public void Bc7RgbBalanced()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestRgbHard1, CompressionFormat.Bc7, CompressionQuality.Balanced, "encoding_bc7_rgb_balanced.ktx", output);
		}

		[Fact]
		public void Bc7LennaBalanced()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestLenna, CompressionFormat.Bc7, CompressionQuality.Balanced, "encoding_bc7_lenna_balanced.ktx", output);
		}

		[Fact]
		public void Bc7RgbFast()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestRgbHard1, CompressionFormat.Bc7, CompressionQuality.Fast, "encoding_bc7_rgb_fast.ktx", output);
		}
	}

	public class Bc7RgbaTest
	{
		private readonly ITestOutputHelper output;

		public Bc7RgbaTest(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void Bc7RgbaBestQuality()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestAlpha1, CompressionFormat.Bc7, CompressionQuality.BestQuality, "encoding_bc7_rgba_bestQuality.ktx", output);
		}

		[Fact]
		public void Bc7RgbaBalanced()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestAlpha1, CompressionFormat.Bc7, CompressionQuality.Balanced, "encoding_bc7_rgba_balanced.ktx", output);
		}

		[Fact]
		public void Bc7RgbaFast()
		{
			TestHelper.ExecuteEncodingTest(ImageLoader.TestAlpha1, CompressionFormat.Bc7, CompressionQuality.Fast, "encoding_bc7_rgba_fast.ktx", output);
		}
	}

	public class CubemapTest
	{
		[Fact]
		public void WriteCubeMapFile()
		{
			var images = ImageLoader.TestCubemap;

			var filename = "encoding_bc1_cubemap.ktx";

			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc1;

			using var fs = File.OpenWrite(filename);
			encoder.EncodeCubeMapToStream(images[0], images[1], images[2], images[3], images[4], images[5], fs);
		}
	}
}
