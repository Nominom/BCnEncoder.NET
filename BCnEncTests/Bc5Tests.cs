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

			var referenceBlock = new ColorRgba32[]
			{
				// 1st row
				new ColorRgba32(13, 53, 0, 255),
				new ColorRgba32(13, 53, 0, 255),
				new ColorRgba32(255, 212, 0, 255),
				new ColorRgba32(167, 238, 0, 255),
				// 2nd row
				new ColorRgba32(136, 238, 0, 255),
				new ColorRgba32(136, 238, 0, 255),
				new ColorRgba32(167, 238, 0, 255),
				new ColorRgba32(75, 185, 0, 255),
				// 3rd row
				new ColorRgba32(255, 212, 0, 255),
				new ColorRgba32(167, 238, 0, 255),
				new ColorRgba32(13, 53, 0, 255),
				new ColorRgba32(13, 53, 0, 255),
				// 4th row
				new ColorRgba32(167, 238, 0, 255),
				new ColorRgba32(75, 185, 0, 255),
				new ColorRgba32(75, 159, 0, 255),
				new ColorRgba32(75, 159, 0, 255)
			};

			var decodedBlock = block.Decode();

			for (var i = 0; i < 16; i++)
				Assert.Equal(referenceBlock[i], decodedBlock[i].As<ColorRgba32>());
		}
	}
}
