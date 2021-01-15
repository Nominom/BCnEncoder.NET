using System.IO;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
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
			var image = ImageLoader.TestGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc1,
				CompressionQuality.BestQuality,
				"encoding_bc1_gradient_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc1GradientBalanced()
		{
			var image = ImageLoader.TestGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc1,
				CompressionQuality.Balanced,
				"encoding_bc1_gradient_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc1GradientFast()
		{
			var image = ImageLoader.TestGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc1,
				CompressionQuality.Fast,
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
			var image = ImageLoader.TestDiffuse1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc1,
				CompressionQuality.BestQuality,
				"encoding_bc1_diffuse_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc1DiffuseBalanced()
		{
			var image = ImageLoader.TestDiffuse1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc1,
				CompressionQuality.Balanced,
				"encoding_bc1_diffuse_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc1DiffuseFast()
		{
			var image = ImageLoader.TestDiffuse1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc1,
				CompressionQuality.Fast,
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
			var image = ImageLoader.TestBlur1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc1,
				CompressionQuality.BestQuality,
				"encoding_bc1_blur_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc1BlurBalanced()
		{
			var image = ImageLoader.TestBlur1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc1,
				CompressionQuality.Balanced,
				"encoding_bc1_blur_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc1BlurFast()
		{
			var image = ImageLoader.TestBlur1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc1,
				CompressionQuality.Fast,
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
		public void Bc1ASpriteBestQuality()
		{
			var image = ImageLoader.TestTransparentSprite1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc1WithAlpha,
				CompressionQuality.BestQuality,
				"encoding_bc1a_sprite_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc1ASpriteBalanced()
		{
			var image = ImageLoader.TestTransparentSprite1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc1WithAlpha,
				CompressionQuality.Balanced,
				"encoding_bc1a_sprite_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc1ASpriteFast()
		{
			var image = ImageLoader.TestTransparentSprite1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc1WithAlpha,
				CompressionQuality.Fast,
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
			var image = ImageLoader.TestAlphaGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc2,
				CompressionQuality.BestQuality,
				"encoding_bc2_gradient_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc2GradientBalanced()
		{
			var image = ImageLoader.TestAlphaGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc2,
				CompressionQuality.Balanced,
				"encoding_bc2_gradient_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc2GradientFast()
		{
			var image = ImageLoader.TestAlphaGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc2,
				CompressionQuality.Fast,
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
			var image = ImageLoader.TestAlphaGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc3,
				CompressionQuality.BestQuality,
				"encoding_bc3_gradient_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc3GradientBalanced()
		{
			var image = ImageLoader.TestAlphaGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc3,
				CompressionQuality.Balanced,
				"encoding_bc3_gradient_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc3GradientFast()
		{
			var image = ImageLoader.TestAlphaGradient1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc3,
				CompressionQuality.Fast,
				"encoding_bc3_gradient_fast.ktx",
				output);
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
			var image = ImageLoader.TestHeight1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc4,
				CompressionQuality.BestQuality,
				"encoding_bc4_red_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc4RedBalanced()
		{
			var image = ImageLoader.TestHeight1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc4,
				CompressionQuality.Balanced,
				"encoding_bc4_red_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc4RedFast()
		{
			var image = ImageLoader.TestHeight1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc4,
				CompressionQuality.Fast,
				"encoding_bc4_red_fast.ktx",
				output);
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
			var image = ImageLoader.TestRedGreen1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc5,
				CompressionQuality.BestQuality,
				"encoding_bc5_red_green_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc5RedGreenBalanced()
		{
			var image = ImageLoader.TestRedGreen1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc5,
				CompressionQuality.Balanced,
				"encoding_bc5_red_green_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc5RedGreenFast()
		{
			var image = ImageLoader.TestRedGreen1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc5,
				CompressionQuality.Fast,
				"encoding_bc5_red_green_fast.ktx",
				output);
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
			var image = ImageLoader.TestRgbHard1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc7,
				CompressionQuality.BestQuality,
				"encoding_bc7_rgb_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc7RgbBalanced()
		{
			var image = ImageLoader.TestRgbHard1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc7,
				CompressionQuality.Balanced,
				"encoding_bc7_rgb_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc7LennaBalanced()
		{
			var image = ImageLoader.TestLenna;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc7,
				CompressionQuality.Balanced,
				"encoding_bc7_lenna_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc7RgbFast()
		{
			var image = ImageLoader.TestRgbHard1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc7,
				CompressionQuality.Fast,
				"encoding_bc7_rgb_fast.ktx",
				output);
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
			var image = ImageLoader.TestAlpha1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc7,
				CompressionQuality.BestQuality,
				"encoding_bc7_rgba_bestQuality.ktx",
				output);
		}

		[Fact]
		public void Bc7RgbaBalanced()
		{
			var image = ImageLoader.TestAlpha1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc7,
				CompressionQuality.Balanced,
				"encoding_bc7_rgba_balanced.ktx",
				output);
		}

		[Fact]
		public void Bc7RgbaFast()
		{
			var image = ImageLoader.TestAlpha1;

			TestHelper.ExecuteEncodingTest(image,
				CompressionFormat.Bc7,
				CompressionQuality.Fast,
				"encoding_bc7_rgba_fast.ktx",
				output);
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
			encoder.EncodeCubeMap(images[0], images[1], images[2], images[3], images[4], images[5], fs);
			fs.Close();
		}
	}
}
