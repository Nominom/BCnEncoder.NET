using BCnEncoder.Shared;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace BCnEncTests
{
	public class BC1Tests
	{
		
		[Fact]
		public void Decode() {
			Bc1Block block = new Bc1Block();
			block.color0 = new ColorRgb565(255, 255, 255);
			block.color1 = new ColorRgb565(0, 0, 0);
			
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
			Assert.Equal(new Rgba32(0, 0, 0), raw.p00);
			Assert.Equal(new Rgba32(0, 0, 0), raw.p10);
			Assert.Equal(new Rgba32(0, 0, 0), raw.p20);
			Assert.Equal(new Rgba32(0, 0, 0), raw.p30);

			Assert.Equal(new Rgba32(85, 85, 85), raw.p01);
			Assert.Equal(new Rgba32(85, 85, 85), raw.p11);
			Assert.Equal(new Rgba32(85, 85, 85), raw.p21);
			Assert.Equal(new Rgba32(85, 85, 85), raw.p31);

			Assert.Equal(new Rgba32(170, 170, 170), raw.p02);
			Assert.Equal(new Rgba32(170, 170, 170), raw.p12);
			Assert.Equal(new Rgba32(170, 170, 170), raw.p22);
			Assert.Equal(new Rgba32(170, 170, 170), raw.p32);

			Assert.Equal(new Rgba32(255, 255, 255), raw.p03);
			Assert.Equal(new Rgba32(255, 255, 255), raw.p13);
			Assert.Equal(new Rgba32(255, 255, 255), raw.p23);
			Assert.Equal(new Rgba32(255, 255, 255), raw.p33);
		}

		[Fact]
		public void DecodeBlack() {
			Bc1Block block = new Bc1Block();
			block.color0 = new ColorRgb565(200, 200, 200);
			block.color1 = new ColorRgb565(255, 255, 255);

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
			Assert.Equal(new Rgba32(206, 203, 206), raw.p00);
			Assert.Equal(new Rgba32(206, 203, 206), raw.p10);
			Assert.Equal(new Rgba32(206, 203, 206), raw.p20);
			Assert.Equal(new Rgba32(206, 203, 206), raw.p30);

			Assert.Equal(new Rgba32(0, 0, 0), raw.p01);
			Assert.Equal(new Rgba32(0, 0, 0), raw.p11);
			Assert.Equal(new Rgba32(0, 0, 0), raw.p21);
			Assert.Equal(new Rgba32(0, 0, 0), raw.p31);

			Assert.Equal(new Rgba32(230, 228, 230), raw.p02);
			Assert.Equal(new Rgba32(230, 228, 230), raw.p12);
			Assert.Equal(new Rgba32(230, 228, 230), raw.p22);
			Assert.Equal(new Rgba32(230, 228, 230), raw.p32);

			Assert.Equal(new Rgba32(255, 255, 255), raw.p03);
			Assert.Equal(new Rgba32(255, 255, 255), raw.p13);
			Assert.Equal(new Rgba32(255, 255, 255), raw.p23);
			Assert.Equal(new Rgba32(255, 255, 255), raw.p33);
		}

		[Fact]
		public void DecodeAlpha() {
			Bc1Block block = new Bc1Block();
			block.color0 = new ColorRgb565(200, 200, 200);
			block.color1 = new ColorRgb565(255, 255, 255);

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
			Assert.Equal(new Rgba32(206, 203, 206), raw.p00);
			Assert.Equal(new Rgba32(206, 203, 206), raw.p10);
			Assert.Equal(new Rgba32(206, 203, 206), raw.p20);
			Assert.Equal(new Rgba32(206, 203, 206), raw.p30);

			Assert.Equal(new Rgba32(0,0,0,0), raw.p01);
			Assert.Equal(new Rgba32(0,0,0,0), raw.p11);
			Assert.Equal(new Rgba32(0,0,0,0), raw.p21);
			Assert.Equal(new Rgba32(0,0,0,0), raw.p31);

			Assert.Equal(new Rgba32(230, 228, 230), raw.p02);
			Assert.Equal(new Rgba32(230, 228, 230), raw.p12);
			Assert.Equal(new Rgba32(230, 228, 230), raw.p22);
			Assert.Equal(new Rgba32(230, 228, 230), raw.p32);

			Assert.Equal(new Rgba32(255, 255, 255), raw.p03);
			Assert.Equal(new Rgba32(255, 255, 255), raw.p13);
			Assert.Equal(new Rgba32(255, 255, 255), raw.p23);
			Assert.Equal(new Rgba32(255, 255, 255), raw.p33);
		}

	}
}
