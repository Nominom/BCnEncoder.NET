using BCnEncoder.Shared;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared.Colors;

namespace BCnEncTests
{
	public class Bc1Tests
	{

		[Fact]
		public void Decode() {
			var block = new Bc1Block
			{
				color0 = new ColorRgb565(255, 255, 255),
				color1 = new ColorRgb565(0, 0, 0)
			};

			Assert.False(block.HasAlphaOrBlack);
			block[0] = 1;
			block[1] = 1;
			block[2] = 1;
			block[3] = 1;

			block[4] = 3;
			block[5] = 3;
			block[6] = 3;
			block[7] = 3;

			block[8] = 2;
			block[9] = 2;
			block[10] = 2;
			block[11] = 2;

			block[12] = 0;
			block[13] = 0;
			block[14] = 0;
			block[15] = 0;

			var raw = block.Decode(false);
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p00.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p10.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p20.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p30.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(85, 85, 85), raw.p01.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(85, 85, 85), raw.p11.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(85, 85, 85), raw.p21.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(85, 85, 85), raw.p31.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(170, 170, 170), raw.p02.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(170, 170, 170), raw.p12.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(170, 170, 170), raw.p22.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(170, 170, 170), raw.p32.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p03.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p13.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p23.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p33.As<ColorRgba32>());
		}

		[Fact]
		public void DecodeBlack() {
			var block = new Bc1Block
			{
				color0 = new ColorRgb565(200, 200, 200),
				color1 = new ColorRgb565(255, 255, 255)
			};

			Assert.True(block.HasAlphaOrBlack);
			block[0] = 0;
			block[1] = 0;
			block[2] = 0;
			block[3] = 0;

			block[4] = 3;
			block[5] = 3;
			block[6] = 3;
			block[7] = 3;

			block[8] = 2;
			block[9] = 2;
			block[10] = 2;
			block[11] = 2;

			block[12] = 1;
			block[13] = 1;
			block[14] = 1;
			block[15] = 1;

			var raw = block.Decode(false);
			Assert.Equal(new ColorRgba32(206, 203, 206), raw.p00.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(206, 203, 206), raw.p10.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(206, 203, 206), raw.p20.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(206, 203, 206), raw.p30.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p01.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p11.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p21.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p31.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(231, 229, 231), raw.p02.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(231, 229, 231), raw.p12.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(231, 229, 231), raw.p22.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(231, 229, 231), raw.p32.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p03.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p13.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p23.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p33.As<ColorRgba32>());
		}

		[Fact]
		public void DecodeAlpha() {
			var block = new Bc1Block
			{
				color0 = new ColorRgb565(200, 200, 200),
				color1 = new ColorRgb565(255, 255, 255)
			};

			Assert.True(block.HasAlphaOrBlack);
			block[0] = 0;
			block[1] = 0;
			block[2] = 0;
			block[3] = 0;

			block[4] = 3;
			block[5] = 3;
			block[6] = 3;
			block[7] = 3;

			block[8] = 2;
			block[9] = 2;
			block[10] = 2;
			block[11] = 2;

			block[12] = 1;
			block[13] = 1;
			block[14] = 1;
			block[15] = 1;

			var raw = block.Decode(true);
			Assert.Equal(new ColorRgba32(206, 203, 206), raw.p00.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(206, 203, 206), raw.p10.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(206, 203, 206), raw.p20.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(206, 203, 206), raw.p30.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(0,0,0,0), raw.p01.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0,0,0,0), raw.p11.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0,0,0,0), raw.p21.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0,0,0,0), raw.p31.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(231, 229, 231), raw.p02.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(231, 229, 231), raw.p12.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(231, 229, 231), raw.p22.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(231, 229, 231), raw.p32.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p03.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p13.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p23.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p33.As<ColorRgba32>());
		}
	}
}
