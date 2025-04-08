using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using Xunit;

namespace BCnEncTests.BlockCompression
{
	public class Bc1Tests
	{

		[Fact]
		public void Decode() {
			var block = new Bc1Block
			{
				color0 = new ColorB5G6R5Packed(0.3f, 1, 0.6f),
				color1 = new ColorB5G6R5Packed(0, 0, 0)
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

			var raw = block.Decode(false, false);
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p00.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p10.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p20.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p30.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(25, 85, 52), raw.p01.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(25, 85, 52), raw.p11.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(25, 85, 52), raw.p21.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(25, 85, 52), raw.p31.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(49, 170, 104), raw.p02.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(49, 170, 104), raw.p12.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(49, 170, 104), raw.p22.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(49, 170, 104), raw.p32.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(74, 255, 156), raw.p03.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(74, 255, 156), raw.p13.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(74, 255, 156), raw.p23.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(74, 255, 156), raw.p33.As<ColorRgba32>());
		}

		[Fact]
		public void DecodeIgnoreOrder() {
			var block = new Bc1Block
			{
				color0 = new ColorB5G6R5Packed(0, 0, 0),
				color1 = new ColorB5G6R5Packed(0.3f, 1, 0.6f)
			};

			Assert.True(block.HasAlphaOrBlack);
			block[0] = 0;
			block[1] = 0;
			block[2] = 0;
			block[3] = 0;

			block[4] = 2;
			block[5] = 2;
			block[6] = 2;
			block[7] = 2;

			block[8] =  3;
			block[9] =  3;
			block[10] = 3;
			block[11] = 3;

			block[12] = 1;
			block[13] = 1;
			block[14] = 1;
			block[15] = 1;

			var raw = block.Decode(false, false);
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p00.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p10.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p20.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p30.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(25, 85, 52), raw.p01.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(25, 85, 52), raw.p11.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(25, 85, 52), raw.p21.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(25, 85, 52), raw.p31.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(49, 170, 104), raw.p02.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(49, 170, 104), raw.p12.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(49, 170, 104), raw.p22.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(49, 170, 104), raw.p32.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(74, 255, 156), raw.p03.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(74, 255, 156), raw.p13.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(74, 255, 156), raw.p23.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(74, 255, 156), raw.p33.As<ColorRgba32>());
		}

		[Fact]
		public void DecodeBlack() {
			var block = new Bc1Block
			{
				color0 = new ColorB5G6R5Packed(100 / 255f, 200 / 255f, 200 / 255f),
				color1 = new ColorB5G6R5Packed(1, 1, 1)
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

			var raw = block.Decode(true, false);
			Assert.Equal(new ColorRgba32(99, 198, 197), raw.p00.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(99, 198, 197), raw.p10.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(99, 198, 197), raw.p20.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(99, 198, 197), raw.p30.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p01.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p11.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p21.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0, 0, 0), raw.p31.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(177, 227, 226), raw.p02.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(177, 227, 226), raw.p12.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(177, 227, 226), raw.p22.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(177, 227, 226), raw.p32.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p03.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p13.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p23.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p33.As<ColorRgba32>());
		}

		[Fact]
		public void DecodeAlpha() {
			var block = new Bc1Block
			{
				color0 = new ColorB5G6R5Packed(100 / 255f, 200 / 255f, 200 / 255f),
				color1 = new ColorB5G6R5Packed(1, 1, 1)
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

			var raw = block.Decode(true, true);
			Assert.Equal(new ColorRgba32(99, 198, 197), raw.p00.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(99, 198, 197), raw.p10.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(99, 198, 197), raw.p20.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(99, 198, 197), raw.p30.As<ColorRgba32>());

			// Alpha 0
			Assert.Equal(new ColorRgba32(0,0,0,0), raw.p01.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0,0,0,0), raw.p11.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0,0,0,0), raw.p21.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(0,0,0,0), raw.p31.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(177, 227, 226), raw.p02.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(177, 227, 226), raw.p12.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(177, 227, 226), raw.p22.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(177, 227, 226), raw.p32.As<ColorRgba32>());

			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p03.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p13.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p23.As<ColorRgba32>());
			Assert.Equal(new ColorRgba32(255, 255, 255), raw.p33.As<ColorRgba32>());
		}
	}
}
