using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using BCnEncoder.Encoder;
using BCnEncoder.Encoder.Bptc;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using CommunityToolkit.HighPerformance;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests
{
	public class Bc6EncoderTests
	{
		private ITestOutputHelper output;
		
		public Bc6EncoderTests(ITestOutputHelper output) => this.output = output;
		
		[Theory]
		[InlineData(0x7F, 8, false)]
		[InlineData(0xFF, 8, false)]
		[InlineData(0x7F, 8, true)]
		[InlineData(0xFF, 8, true)]
		[InlineData(0b111_1100_1101, 11, true)]
		[InlineData(0b111_1100_1101, 11, false)]
		[InlineData(0b011_1100_1101, 11, true)]
		[InlineData(0b011_1100_1101, 11, false)]
		[InlineData(0b0_1001, 5, true)]
		[InlineData(0b0_1001, 5, false)]
		[InlineData(0b1_1100, 5, true)]
		[InlineData(0b1_1100, 5, false)]
		public void Quantize(int initialQuantizedValue, int endpointBits, bool signed)
		{
			if (signed)
			{
				initialQuantizedValue = IntHelper.SignExtend(initialQuantizedValue, endpointBits);
			}
			var unquantized = Bc6Block.UnQuantize(initialQuantizedValue, endpointBits, signed);
			var finishedUnquantized = Bc6Block.FinishUnQuantize(unquantized, signed);
			
			var prequantized = Bc6EncodingHelpers.PreQuantize(finishedUnquantized, signed);
			var quantized = Bc6EncodingHelpers.Quantize(prequantized, endpointBits, signed);

			Assert.InRange(prequantized, unquantized - 2, unquantized + 2);
			Assert.InRange(quantized, initialQuantizedValue - 1, initialQuantizedValue + 1);
		}

		[Fact]
		public void PreQuantizeRawEndpoint()
		{
			var ep0 = new ColorRgbFloat(1, 0.001f, 0.2f);

			var preQuantized = Bc6EncodingHelpers.PreQuantizeRawEndpoint(ep0, false);
			var unQu = Bc6Block.FinishUnQuantize(preQuantized, false);

			Assert.InRange(unQu.Item1, ep0.r - 0.001f, ep0.r + 0.001f);
			Assert.InRange(unQu.Item2, ep0.g - 0.001f, ep0.g + 0.001f);
			Assert.InRange(unQu.Item3, ep0.b - 0.001f, ep0.b + 0.001f);
		}

		[Fact]
		public void RgbBoundingBox()
		{
			var testBlock = new RawBlock4X4RgbFloat()
			{
				p00 = new ColorRgbFloat(0.01f, -0.05f, 0.02f),
				p01 = new ColorRgbFloat(0.02f, 0.02f, 0.03f),
				p02 = new ColorRgbFloat(0.02f, 0.02f, 0.02f),
				p03 = new ColorRgbFloat(0.01f, 0.02f, 0.02f),
				p10 = new ColorRgbFloat(0.02f, 0.02f, 0.02f),
				p11 = new ColorRgbFloat(0.045f, 0.02f, 0.02f),
				p12 = new ColorRgbFloat(0.04f, 0.02f, 0.02f),
				p13 = new ColorRgbFloat(0.04f, 0.02f, 0.02f),
				p20 = new ColorRgbFloat(0.21f, 0.02f, 0.02f),
				p21 = new ColorRgbFloat(0.01f, 0.02f, 0.02f),
				p22 = new ColorRgbFloat(0.01f, 0.32f, 0.02f),
				p23 = new ColorRgbFloat(1.01f, 0.22f, 0.02f),
				p30 = new ColorRgbFloat(0.01f, 0.12f, 0.02f),
				p31 = new ColorRgbFloat(0.01f, 0.02f, 0.02f),
				p32 = new ColorRgbFloat(0.01f, 0.62f, 0.7f),
				p33 = new ColorRgbFloat(0.01f, 0.02f, 0.02f)
			};

			BCnEncoder.Shared.RgbBoundingBox.CreateFloat(testBlock.AsSpan, out var min, out var max);

			Assert.InRange(min.r, 0.01, 0.02);
			Assert.InRange(min.g, -0.05, -0.03);
			Assert.InRange(min.b, 0.02, 0.03);

			Assert.InRange(max.r, 0.9, 1.01);
			Assert.InRange(max.g, 0.59, 0.62);
			Assert.InRange(max.b, 0.60, 0.7);
		}


		[Fact]
		public void PackMode3()
		{
			const int endpointPrecision = 10;
			var random = new Random();
			var rand = random.Next();
			
			var ep0 = (random.Next() & (1 << endpointPrecision) - 1,
				random.Next() & (1 << endpointPrecision) - 1,
				random.Next() & (1 << endpointPrecision) - 1);
			var ep1 = (random.Next() & (1 << endpointPrecision) - 1,
				random.Next() & (1 << endpointPrecision) - 1,
				random.Next() & (1 << endpointPrecision) - 1);

			var indices = new byte[16];
			for (var i = 1; i < indices.Length; i++)
			{
				indices[i] = (byte) random.Next(1 << 4);
			}

			var block = Bc6Block.PackType3(ep0, ep1, indices);
			
			Assert.Equal(Bc6BlockType.Type3, block.Type);

			var extracted0 = block.ExtractEp0();
			var extracted1 = block.ExtractEp1();

			Assert.Equal(ep0, extracted0);
			Assert.Equal(ep1, extracted1);

			for (var i = 0; i < indices.Length; i++)
			{
				var idx = block.GetColorIndex(1, 0, 4, i);
				Assert.Equal(indices[i], idx);
			}
		}

		
		[Theory]
		[InlineData(Bc6BlockType.Type0)]
		[InlineData(Bc6BlockType.Type1)]
		[InlineData(Bc6BlockType.Type2)]
		[InlineData(Bc6BlockType.Type6)]
		[InlineData(Bc6BlockType.Type10)]
		[InlineData(Bc6BlockType.Type14)]
		[InlineData(Bc6BlockType.Type18)]
		[InlineData(Bc6BlockType.Type22)]
		[InlineData(Bc6BlockType.Type26)]
		[InlineData(Bc6BlockType.Type30)]
		[InlineData(Bc6BlockType.Type3)]
		[InlineData(Bc6BlockType.Type7)]
		[InlineData(Bc6BlockType.Type11)]
		//[InlineData(Bc6BlockType.Type15)] //deltabits too small to encode the testblock
		internal void EncodeAllModesUnsigned(Bc6BlockType type)
		{
			var testBlock = new RawBlock4X4RgbFloat()
			{
				p00 = new ColorRgbFloat(1.011f, 10.01f, 2.01f),
				p01 = new ColorRgbFloat(1.01f, 10.014f, 2.012f),
				p02 = new ColorRgbFloat(1.005f, 10.012f, 2.02f),
				p03 = new ColorRgbFloat(1.01f, 10.013f, 2.023f),
				p10 = new ColorRgbFloat(1.011f, 10.01f, 2.01f),
				p11 = new ColorRgbFloat(1.01f, 10.014f, 2.012f),
				p12 = new ColorRgbFloat(1.005f, 10.012f, 2.02f),
				p13 = new ColorRgbFloat(1.01f, 10.013f, 2.023f),
				p20 = new ColorRgbFloat(1.011f, 10.01f, 2.01f),
				p21 = new ColorRgbFloat(1.01f, 10.014f, 2.012f),
				p22 = new ColorRgbFloat(1.005f, 10.012f, 2.02f),
				p23 = new ColorRgbFloat(1.01f, 10.013f, 2.023f),
				p30 = new ColorRgbFloat(1.011f, 10.01f, 2.01f),
				p31 = new ColorRgbFloat(1.01f, 10.014f, 2.012f),
				p32 = new ColorRgbFloat(1.005f, 10.012f, 2.02f),
				p33 = new ColorRgbFloat(1.01f, 10.013f, 2.023f)
			};
			Bc6Block encoded;
			var badTransform = false;
			
			if(type.HasSubsets())
			{
				var indexBlock = Bc6Encoder.CreateClusterIndexBlock(testBlock, out var numClusters, 2);
				var best2SubsetPartitions = BptcEncodingHelpers.Rank2SubsetPartitions(indexBlock, numClusters, true);

				var bestPartition = best2SubsetPartitions[0];

				Bc6EncodingHelpers.GetInitialUnscaledEndpointsForSubset(testBlock, out var ep0, out var ep1, bestPartition, 0);
				Bc6EncodingHelpers.GetInitialUnscaledEndpointsForSubset(testBlock, out var ep2, out var ep3, bestPartition, 1);

				encoded = Bc6ModeEncoder.EncodeBlock2Sub(type, testBlock, ep0, ep1, ep2, ep3, bestPartition,
					false, out badTransform);
			}
			else
			{
				BCnEncoder.Shared.RgbBoundingBox.CreateFloat(testBlock.AsSpan, out var min, out var max);
				encoded = Bc6ModeEncoder.EncodeBlock1Sub(type, testBlock, min, max, false,
					out badTransform);
			}

			Assert.False(badTransform);
			Assert.Equal(type, encoded.Type);
			var decoded = encoded.Decode(false);
			var error = testBlock.CalculateError(decoded);
			Assert.InRange(error, 0, 0.3);
		}

		[Theory]
		[InlineData(Bc6BlockType.Type0)]
		[InlineData(Bc6BlockType.Type1)]
		[InlineData(Bc6BlockType.Type2)]
		[InlineData(Bc6BlockType.Type6)]
		[InlineData(Bc6BlockType.Type10)]
		[InlineData(Bc6BlockType.Type14)]
		[InlineData(Bc6BlockType.Type18)]
		[InlineData(Bc6BlockType.Type22)]
		[InlineData(Bc6BlockType.Type26)]
		[InlineData(Bc6BlockType.Type30)]
		[InlineData(Bc6BlockType.Type3)]
		[InlineData(Bc6BlockType.Type7)]
		[InlineData(Bc6BlockType.Type11)]
		[InlineData(Bc6BlockType.Type15)]
		internal void EncodeAllModesSigned(Bc6BlockType type)
		{
			var testBlock = new RawBlock4X4RgbFloat()
			{
				p00 = new ColorRgbFloat(-1.011f, 10.01f, 2.01f),
				p01 = new ColorRgbFloat(-1.01f, 10.014f, 2.012f),
				p02 = new ColorRgbFloat(-1.005f, 10.012f, 2.02f),
				p03 = new ColorRgbFloat(-1.01f, 10.013f, 2.023f),
				p10 = new ColorRgbFloat(-1.011f, 10.01f, 2.01f),
				p11 = new ColorRgbFloat(-1.01f, 10.014f, 2.012f),
				p12 = new ColorRgbFloat(-1.005f, 10.012f, 2.02f),
				p13 = new ColorRgbFloat(-1.01f, 10.013f, 2.023f),
				p20 = new ColorRgbFloat(-1.011f, 10.01f, 2.01f),
				p21 = new ColorRgbFloat(-1.01f, 10.014f, 2.012f),
				p22 = new ColorRgbFloat(-1.005f, 10.012f, 2.02f),
				p23 = new ColorRgbFloat(-1.01f, 10.013f, 2.023f),
				p30 = new ColorRgbFloat(-1.011f, 10.01f, 2.01f),
				p31 = new ColorRgbFloat(-1.01f, 10.014f, 2.012f),
				p32 = new ColorRgbFloat(-1.005f, 10.012f, 2.02f),
				p33 = new ColorRgbFloat(-1.01f, 10.013f, 2.023f)
			};
			Bc6Block encoded;
			var badTransform = false;

			if (type.HasSubsets())
			{
				var indexBlock = Bc6Encoder.CreateClusterIndexBlock(testBlock, out var numClusters, 2);
				var best2SubsetPartitions = BptcEncodingHelpers.Rank2SubsetPartitions(indexBlock, numClusters, true);

				var bestPartition = best2SubsetPartitions[0];

				Bc6EncodingHelpers.GetInitialUnscaledEndpointsForSubset(testBlock, out var ep0, out var ep1, bestPartition, 0);
				Bc6EncodingHelpers.GetInitialUnscaledEndpointsForSubset(testBlock, out var ep2, out var ep3, bestPartition, 1);

				encoded = Bc6ModeEncoder.EncodeBlock2Sub(type, testBlock, ep0, ep1, ep2, ep3, bestPartition,
					 true, out badTransform);
			}
			else
			{
				BCnEncoder.Shared.RgbBoundingBox.CreateFloat(testBlock.AsSpan, out var min, out var max);
				encoded = Bc6ModeEncoder.EncodeBlock1Sub(type, testBlock, min, max, true,
					out badTransform);
			}

			Assert.False(badTransform);
			Assert.Equal(type, encoded.Type);
			var decoded = encoded.Decode(true);
			var error = testBlock.CalculateError(decoded);
			Assert.InRange(error, 0, 0.5);
		}

		[Fact]
		public void Encode()
		{
			var signed = true;
			var image = HdrLoader.TestHdrKiara;
			var blocks = ImageToBlocks.ImageTo4X4(image.pixels.AsMemory().AsMemory2D(image.height, image.width), out var bW, out var bH);

			for (var i = 0; i < blocks.Length; i++)
			{
				var encoded = Bc6Encoder.Bc6EncoderBalanced.EncodeBlock(blocks[i], signed);
				var decoded = encoded.Decode(signed);

				var error = decoded.CalculateError(blocks[i]);
				if (error > 0.06 || i == 14749)
				{
					Debugger.Break();
					encoded = Bc6Encoder.Bc6EncoderBalanced.EncodeBlock(blocks[i], signed);
				}
			}
		}

		[Fact]
		public void EncodeFast()
		{
			TestHelper.ExecuteHdrEncodingTest(HdrLoader.TestHdrKiara, CompressionFormat.Bc6U, CompressionQuality.Fast,
				"encoding_bc6_kiara_fast.ktx", output);
		}

		[Fact]
		public void EncodeBalanced()
		{
			TestHelper.ExecuteHdrEncodingTest(HdrLoader.TestHdrKiara, CompressionFormat.Bc6U, CompressionQuality.Balanced,
				"encoding_bc6_kiara_balanced.ktx", output);
		}

		[Fact]
		public void EncodeBestQuality()
		{
			TestHelper.ExecuteHdrEncodingTest(HdrLoader.TestHdrKiara, CompressionFormat.Bc6U, CompressionQuality.BestQuality,
				"encoding_bc6_kiara_bestquality.ktx", output);
		}

		[Fact]
		public void EncodeProbeFast()
		{
			TestHelper.ExecuteHdrEncodingTest(HdrLoader.TestHdrProbe, CompressionFormat.Bc6U, CompressionQuality.Fast,
				"encoding_bc6_probe_fast.ktx", output);
		}

		[Fact]
		public void EncodeProbeBalanced()
		{
			TestHelper.ExecuteHdrEncodingTest(HdrLoader.TestHdrProbe, CompressionFormat.Bc6U, CompressionQuality.Balanced,
				"encoding_bc6_probe_balanced.ktx", output);
		}

		[Fact]
		public void EncodeProbeBestQuality()
		{
			TestHelper.ExecuteHdrEncodingTest(HdrLoader.TestHdrProbe, CompressionFormat.Bc6U, CompressionQuality.BestQuality,
				"encoding_bc6_probe_bestquality.ktx", output);
		}

		[Fact]
		public void EncodeProbeSignedFast()
		{
			TestHelper.ExecuteHdrEncodingTest(HdrLoader.TestHdrProbe, CompressionFormat.Bc6S, CompressionQuality.Fast,
				"encoding_bc6_probe_signed_fast.ktx", output);
		}

		[Fact]
		public void EncodeProbSignedBalanced()
		{
			TestHelper.ExecuteHdrEncodingTest(HdrLoader.TestHdrProbe, CompressionFormat.Bc6S, CompressionQuality.Balanced,
				"encoding_bc6_probe_signed_balanced.ktx", output);
		}

		[Fact]
		public void EncodeProbeSignedBestQuality()
		{
			TestHelper.ExecuteHdrEncodingTest(HdrLoader.TestHdrProbe, CompressionFormat.Bc6S, CompressionQuality.BestQuality,
				"encoding_bc6_probe_signed_bestquality.ktx", output);
		}

		[Fact]
		public void EncodeToKtx()
		{
			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc6U;

			var ktx = encoder.EncodeToKtxHdr(HdrLoader.TestHdrKiara.PixelMemory);

			using var fs = File.OpenWrite("encoding_bc6_ktx.ktx");
			ktx.Write(fs);
		}

		[Fact]
		public void EncodeToDds()
		{
			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc6U;

			var dds = encoder.EncodeToDdsHdr(HdrLoader.TestHdrKiara.PixelMemory);

			using var fs = File.OpenWrite("encoding_bc6_dds.dds");
			dds.Write(fs);
		}

		[Fact]
		public void EncodeToRaw()
		{
			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc6U;

			var ktx = encoder.EncodeToKtxHdr(HdrLoader.TestHdrKiara.PixelMemory);

			var allMips = encoder.EncodeToRawBytesHdr(HdrLoader.TestHdrKiara.PixelMemory);

			Assert.True(allMips.Length > 1);
			Assert.True(allMips.Length == ktx.MipMaps.Count);
			
			for (var i = 0; i < allMips.Length; i++)
			{
				var single = encoder.EncodeToRawBytesHdr(HdrLoader.TestHdrKiara.PixelMemory, i, out var mW, out var mH);

				Assert.Equal(ktx.MipMaps[i].Faces[0].Data, single);
				Assert.Equal(allMips[i], single);
			}
		}
	}
}
