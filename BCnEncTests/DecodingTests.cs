using System;
using System.Collections.Generic;
using System.IO;
using BCnEnc.Net.Decoder;
using BCnEnc.Net.Shared;
using SixLabors.ImageSharp;
using Xunit;

namespace BCnEncTests
{
	public class DecodingTests
	{
		[Fact]
		public void Bc1Decode() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc1.ktx");
			KtxFile file = KtxFile.Load(fs);
			Assert.True(file.Header.VerifyHeader());
			Assert.Equal((uint)1, file.Header.NumberOfFaces);

			BcDecoder decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.Header.PixelWidth);
			Assert.Equal((uint)image.Height, file.Header.PixelHeight);

			using FileStream outFs = File.OpenWrite("decoding_test_bc1.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc1AlphaDecode() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc1a.ktx");
			KtxFile file = KtxFile.Load(fs);
			Assert.True(file.Header.VerifyHeader());
			Assert.Equal((uint)1, file.Header.NumberOfFaces);

			BcDecoder decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.Header.PixelWidth);
			Assert.Equal((uint)image.Height, file.Header.PixelHeight);

			using FileStream outFs = File.OpenWrite("decoding_test_bc1a.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc2Decode() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc2.ktx");
			KtxFile file = KtxFile.Load(fs);
			Assert.True(file.Header.VerifyHeader());
			Assert.Equal((uint)1, file.Header.NumberOfFaces);

			BcDecoder decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.Header.PixelWidth);
			Assert.Equal((uint)image.Height, file.Header.PixelHeight);

			using FileStream outFs = File.OpenWrite("decoding_test_bc2.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc3Decode() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc3.ktx");
			KtxFile file = KtxFile.Load(fs);
			Assert.True(file.Header.VerifyHeader());
			Assert.Equal((uint)1, file.Header.NumberOfFaces);

			BcDecoder decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.Header.PixelWidth);
			Assert.Equal((uint)image.Height, file.Header.PixelHeight);

			using FileStream outFs = File.OpenWrite("decoding_test_bc3.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc4Decode() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc4_unorm.ktx");
			KtxFile file = KtxFile.Load(fs);
			Assert.True(file.Header.VerifyHeader());
			Assert.Equal((uint)1, file.Header.NumberOfFaces);

			BcDecoder decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.Header.PixelWidth);
			Assert.Equal((uint)image.Height, file.Header.PixelHeight);

			using FileStream outFs = File.OpenWrite("decoding_test_bc4.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc5Decode() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc5_unorm.ktx");
			KtxFile file = KtxFile.Load(fs);
			Assert.True(file.Header.VerifyHeader());
			Assert.Equal((uint)1, file.Header.NumberOfFaces);

			BcDecoder decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.Header.PixelWidth);
			Assert.Equal((uint)image.Height, file.Header.PixelHeight);

			using FileStream outFs = File.OpenWrite("decoding_test_bc5.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc7DecodeRgb() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc7_rgb.ktx");
			KtxFile file = KtxFile.Load(fs);
			Assert.True(file.Header.VerifyHeader());
			Assert.Equal((uint)1, file.Header.NumberOfFaces);

			BcDecoder decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.Header.PixelWidth);
			Assert.Equal((uint)image.Height, file.Header.PixelHeight);

			using FileStream outFs = File.OpenWrite("decoding_test_bc7_rgb.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc7DecodeUnorm() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc7_unorm.ktx");
			KtxFile file = KtxFile.Load(fs);
			Assert.True(file.Header.VerifyHeader());
			Assert.Equal((uint)1, file.Header.NumberOfFaces);

			BcDecoder decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.Header.PixelWidth);
			Assert.Equal((uint)image.Height, file.Header.PixelHeight);

			using FileStream outFs = File.OpenWrite("decoding_test_bc7_unorm.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc7DecodeEveryBlockType() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc7_types.ktx");
			KtxFile file = KtxFile.Load(fs);
			Assert.True(file.Header.VerifyHeader());
			Assert.Equal((uint)1, file.Header.NumberOfFaces);

			BcDecoder decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.Header.PixelWidth);
			Assert.Equal((uint)image.Height, file.Header.PixelHeight);

			using FileStream outFs = File.OpenWrite("decoding_test_bc7_types.png");
			image.SaveAsPng(outFs);
		}
	}
}
