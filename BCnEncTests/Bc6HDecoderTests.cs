using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests
{
	public class Bc6HDecoderTests
	{
		private ITestOutputHelper output;
		
		public Bc6HDecoderTests(ITestOutputHelper output) => this.output = output;

		[Fact]
		public void DecodeDds()
		{
			var decoder = new BcDecoder();
			var decoded = decoder.DecodeHdr(HdrLoader.TestHdrKiaraDds);

			var img = new Image<RgbaVector>((int)HdrLoader.TestHdrKiaraDds.header.dwWidth,
				(int)HdrLoader.TestHdrKiaraDds.header.dwHeight);
			
			var pixels = TestHelper.GetSinglePixelArray(img);

			for (var i = 0; i < decoded.Length; i++)
			{
				pixels[i] = new RgbaVector(decoded[i].r, decoded[i].g, decoded[i].b);
			}

			TestHelper.SetSinglePixelArray(img, pixels);

			var hdr = new HdrImage((int) HdrLoader.TestHdrKiaraDds.header.dwWidth,
				(int) HdrLoader.TestHdrKiaraDds.header.dwHeight);
			
			Assert.Equal(hdr.pixels.Length, decoded.Length);
			
			hdr.pixels = decoded;
			using var sfs = File.OpenWrite("decoding_test_dds_bc6h.hdr");
			hdr.Write(sfs);
			
			img.SaveAsPng("decoding_test_dds_bc6h.png");

			TestHelper.AssertPixelsEqual(HdrLoader.TestHdrKiara.pixels, decoded, CompressionQuality.Fast, output);
		}

		[Fact]
		public void DecodeKtx()
		{
			var decoder = new BcDecoder();
			var decoded = decoder.DecodeHdr(HdrLoader.TestHdrKiaraKtx);

			var img = new Image<RgbaVector>((int)HdrLoader.TestHdrKiaraKtx.header.PixelWidth,
				(int)HdrLoader.TestHdrKiaraKtx.header.PixelHeight);
			
			var pixels = TestHelper.GetSinglePixelArray(img);

			for (var i = 0; i < decoded.Length; i++)
			{
				pixels[i] = new RgbaVector(decoded[i].r, decoded[i].g, decoded[i].b);
			}

			TestHelper.SetSinglePixelArray(img, pixels);

			var hdr = new HdrImage((int)HdrLoader.TestHdrKiaraKtx.header.PixelWidth,
				(int)HdrLoader.TestHdrKiaraKtx.header.PixelHeight);

			Assert.Equal(hdr.pixels.Length, decoded.Length);

			hdr.pixels = decoded;
			using var sfs = File.OpenWrite("decoding_test_ktx_bc6h.hdr");
			hdr.Write(sfs);

			img.SaveAsPng("decoding_test_ktx_bc6h.png");

			TestHelper.AssertPixelsEqual(HdrLoader.TestHdrKiara.pixels, decoded, CompressionQuality.BestQuality, output);
		}

		// TestHdrKiaraDds includes some blocks with all modes.
		// The test_hdr_kiara_dds_float16_data.bin file contains Half-float values decoded with a reference decoder.
		// This test ensures that decoding is bit-exact.
		[Fact]
		public void AllBlocksDecodesExact()
		{
			var decoder = new BcDecoder();
			var decoded = decoder.DecodeHdr(HdrLoader.TestHdrKiaraDds);

			using var fs = File.OpenRead("../../../testImages/test_hdr_kiara_dds_float16_data.bin");
			using var ms = new MemoryStream();
			fs.CopyTo(ms);
			var length = (int)ms.Position;

			var bytes = ms.GetBuffer().AsSpan(0, length);
			var halfs = MemoryMarshal.Cast<byte, Half>(bytes);
			Assert.Equal(halfs.Length / 4, decoded.Length);

			for (var i = 0; i < decoded.Length; i++)
			{
				float r = halfs[i * 4 + 0];
				float g = halfs[i * 4 + 1];
				float b = halfs[i * 4 + 2];

				Assert.Equal(r, decoded[i].r);
				Assert.Equal(g, decoded[i].g);
				Assert.Equal(b, decoded[i].b);
			}
		}

		// Test that above statement holds true. And that modes are recognized correctly
		[Fact]
		public void DecodeModes()
		{
			var modes = new int[31];

			var hdr = HdrLoader.TestHdrKiaraDds;

			var bytes = hdr.Faces[0].MipMaps[0].Data;
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

			var decoded = decoder.DecodeRawHdr(buffer, width * 4, height * 4, CompressionFormat.Bc6U);
			Assert.Contains(new ColorRgbFloat(1, 0, 1), decoded);

			HdrImage image = new HdrImage(new Span2D<ColorRgbFloat>(decoded, height * 4, width * 4));
			using var fs = File.OpenWrite("test_decode_bc6h_error.hdr");
			image.Write(fs);
		}

	}
}
