using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
		public void Bc5DdsDecode()
		{
			var reference = ImageLoader.TestDecodingBc5Reference;
			var decoded = new BcDecoder().Decode(DdsLoader.TestDecompressBc5);

			reference.TryGetSinglePixelSpan(out var refSpan);
			decoded.TryGetSinglePixelSpan(out var decSpan);

			Assert.Equal(reference.Width, decoded.Width);
			Assert.Equal(reference.Height, decoded.Height);

			// Exactly equal
			for (var i = 0; i < reference.Width * reference.Height; i++)
				Assert.Equal(refSpan[i], decSpan[i]);
		}

		[Fact]
		public void Bc5BlockDecode()
		{
			var block = new Bc5Block()
			{
				greenBlock = 0x91824260008935ee,
				redBlock = 0x6d900f66d3c0a70d
			};
			
			var referenceBlock = new RawBlock4X4Rgba32
			{
				p00 = new Rgba32(13, 53, 0, 255),
				p01 = new Rgba32(136, 238, 0, 255),
				p02 = new Rgba32(255, 212, 0, 255),
				p03 = new Rgba32(167, 238, 0, 255),
				p10 = new Rgba32(13, 53, 0, 255),
				p11 = new Rgba32(136, 238, 0, 255),
				p12 = new Rgba32(167, 238, 0, 255),
				p13 = new Rgba32(75, 185, 0, 255),
				p20 = new Rgba32(255, 212, 0, 255),
				p21 = new Rgba32(167, 238, 0, 255),
				p22 = new Rgba32(13, 53, 0, 255),
				p23 = new Rgba32(75, 159, 0, 255),
				p30 = new Rgba32(167, 238, 0, 255),
				p31 = new Rgba32(75, 185, 0, 255),
				p32 = new Rgba32(13, 53, 0, 255),
				p33 = new Rgba32(75, 159, 0, 255)
			};

			var decodedBlock = block.Decode();

			for (var i = 0; i < 16; i++)
				Assert.Equal(referenceBlock[i], decodedBlock[i]);
		}
	}
}
