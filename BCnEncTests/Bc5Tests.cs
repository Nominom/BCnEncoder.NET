using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using BCnEncoder.TextureFormats;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests
{
	public class Bc5Tests
	{
		[Fact]
		public void Bc5Indices()
		{
			var block = new Bc5Block();

			for (var i = 0; i < 16; i++)
			{
				block.SetRedIndex(i, (byte)(i % 8));
				block.SetGreenIndex(i, (byte)((i + 3) % 8));
			}

			for (var i = 0; i < 16; i++)
			{
				int rI = block.GetRedIndex(i);
				int gI = block.GetGreenIndex(i);

				Assert.Equal((byte)(i % 8), rI);
				Assert.Equal((byte)((i + 3) % 8), gI);
			}
		}

		[Fact]
		public void Bc5BlockDecode()
		{
			var block = new Bc5Block()
			{
				greenBlock = new Bc4ComponentBlock() { componentBlock = 0x91824260008935ee },
				redBlock = new Bc4ComponentBlock() { componentBlock = 0x6d900f66d3c0a70d }
			};

			var referenceBlock = new RawBlock4X4RgbaFloat
			{
				p00 = new ColorRgba32(13, 53, 0, 255).ToColorRgbaFloat(),
				p01 = new ColorRgba32(136, 238, 0, 255).ToColorRgbaFloat(),
				p02 = new ColorRgba32(255, 212, 0, 255).ToColorRgbaFloat(),
				p03 = new ColorRgba32(167, 238, 0, 255).ToColorRgbaFloat(),
				p10 = new ColorRgba32(13, 53, 0, 255).ToColorRgbaFloat(),
				p11 = new ColorRgba32(136, 238, 0, 255).ToColorRgbaFloat(),
				p12 = new ColorRgba32(167, 238, 0, 255).ToColorRgbaFloat(),
				p13 = new ColorRgba32(75, 185, 0, 255).ToColorRgbaFloat(),
				p20 = new ColorRgba32(255, 212, 0, 255).ToColorRgbaFloat(),
				p21 = new ColorRgba32(167, 238, 0, 255).ToColorRgbaFloat(),
				p22 = new ColorRgba32(13, 53, 0, 255).ToColorRgbaFloat(),
				p23 = new ColorRgba32(75, 159, 0, 255).ToColorRgbaFloat(),
				p30 = new ColorRgba32(167, 238, 0, 255).ToColorRgbaFloat(),
				p31 = new ColorRgba32(75, 185, 0, 255).ToColorRgbaFloat(),
				p32 = new ColorRgba32(13, 53, 0, 255).ToColorRgbaFloat(),
				p33 = new ColorRgba32(75, 159, 0, 255).ToColorRgbaFloat()
			};

			var decodedBlock = block.Decode();

			for (var i = 0; i < 16; i++)
				Assert.Equal(referenceBlock[i], decodedBlock[i]);
		}
	}
}
