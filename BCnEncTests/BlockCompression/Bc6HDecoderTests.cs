using System;
using System.IO;
using System.Runtime.InteropServices;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using BCnEncoder.TextureFormats;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests.BlockCompression
{
	public class Bc6HDecoderTests
	{
		private ITestOutputHelper output;

		public Bc6HDecoderTests(ITestOutputHelper output) => this.output = output;


		// bc6h_alltypes.dds includes some blocks with all modes.
		// The bc6h_alltypes.bin file contains Half-float values decoded with a reference decoder.
		// This test ensures that decoding is bit-exact.
		[Fact]
		public void AllBlocksDecodesExact()
		{
			var decoder = new BcDecoder();
			using var fs1 = File.OpenRead("../../../testImages/bc6h_alltypes.dds");

			var ddsFile = DdsFile.Load(fs1);
			var decoded = decoder.Decode(ddsFile).First.AsMemory<ColorRgbaFloat>();

			using var fs = File.OpenRead("../../../testImages/bc6h_alltypes.bin");
			using var ms = new MemoryStream();
			fs.CopyTo(ms);
			var length = (int)ms.Position;

			var bytes = ms.GetBuffer().AsSpan(0, length);
			var halfs = MemoryMarshal.Cast<byte, Half>(bytes);
			Assert.Equal(halfs.Length / 4, decoded.Length);

			for (var i = 0; i < decoded.Length; i++)
			{
				float r = (float)halfs[i * 4 + 0];
				float g = (float)halfs[i * 4 + 1];
				float b = (float)halfs[i * 4 + 2];

				Assert.Equal(r, decoded.Span[i].r);
				Assert.Equal(g, decoded.Span[i].g);
				Assert.Equal(b, decoded.Span[i].b);
			}
		}

		// Test that above statement holds true. And that modes are recognized correctly
		[Fact]
		public void DecodeModes()
		{
			var modes = new int[31];

			using var fs1 = File.OpenRead("../../../testImages/bc6h_alltypes.dds");

			var hdr = DdsFile.Load(fs1);

			var bytes = hdr.ArrayElements[0].MipMaps[0].Data;
			var blocks = MemoryMarshal.Cast<byte, Bc6Block>(bytes);

			for (var i = 0; i < blocks.Length; i++)
			{
				var block = blocks[i];
				var mode = block.Type;
				modes[(int) mode]++;
			}

			for (var i = 0; i < modes.Length; i++)
			{
				output.WriteLine($"Mode {i}: {modes[i]}");
			}

			Assert.True(modes[0] > 0);
			Assert.True(modes[1] > 0);
			Assert.True(modes[2] > 0);
			Assert.True(modes[6] > 0);
			Assert.True(modes[10] > 0);
			Assert.True(modes[14] > 0);
			Assert.True(modes[18] > 0);
			Assert.True(modes[22] > 0);
			Assert.True(modes[26] > 0);
			Assert.True(modes[30] > 0);
			Assert.True(modes[3] > 0);
			Assert.True(modes[7] > 0);
			Assert.True(modes[11] > 0);
			Assert.True(modes[15] > 0);
		}

		[Fact]
		public void DecodeErrorBlock()
		{
			var decoder = new BcDecoder();

			var width = 16;
			var height = 16;
			var bufferSize = decoder.GetBlockSize(CompressionFormat.Bc6U) * width * height;

			var buffer = new byte[bufferSize];
			Random r = new Random(44);
			r.NextBytes(buffer);

			var decoded = decoder.DecodeRaw<ColorRgbaFloat>(buffer, width * 4, height * 4, CompressionFormat.Bc6U, CompressionFormat.RgbaFloat);
			Assert.Contains(new ColorRgbaFloat(1, 0, 1), decoded);

			var image = decoded.AsBCnTextureData(width * 4, height * 4, false).AsTexture<RadianceFile>();
			using var fs = File.OpenWrite("test_decode_bc6h_error.hdr");
			image.WriteToStream(fs);
		}

	}
}
