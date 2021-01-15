using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using Xunit;

namespace BCnEncTests
{
	public class DecodingTests
	{
		[Fact]
		public void Bc1Decode()
		{
			var file = KtxLoader.TestDecompressBc1;
			Assert.True(file.header.VerifyHeader());
			Assert.Equal((uint)1, file.header.NumberOfFaces);

			var decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.header.PixelWidth);
			Assert.Equal((uint)image.Height, file.header.PixelHeight);

			using var outFs = File.OpenWrite("decoding_test_bc1.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc1AlphaDecode()
		{
			var file = KtxLoader.TestDecompressBc1A;
			Assert.True(file.header.VerifyHeader());
			Assert.Equal((uint)1, file.header.NumberOfFaces);

			var decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.header.PixelWidth);
			Assert.Equal((uint)image.Height, file.header.PixelHeight);

			using var outFs = File.OpenWrite("decoding_test_bc1a.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc2Decode()
		{
			var file = KtxLoader.TestDecompressBc2;
			Assert.True(file.header.VerifyHeader());
			Assert.Equal((uint)1, file.header.NumberOfFaces);

			var decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.header.PixelWidth);
			Assert.Equal((uint)image.Height, file.header.PixelHeight);

			using var outFs = File.OpenWrite("decoding_test_bc2.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc3Decode()
		{
			var file = KtxLoader.TestDecompressBc3;
			Assert.True(file.header.VerifyHeader());
			Assert.Equal((uint)1, file.header.NumberOfFaces);

			var decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.header.PixelWidth);
			Assert.Equal((uint)image.Height, file.header.PixelHeight);

			using var outFs = File.OpenWrite("decoding_test_bc3.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc4Decode()
		{
			var file = KtxLoader.TestDecompressBc4Unorm;
			Assert.True(file.header.VerifyHeader());
			Assert.Equal((uint)1, file.header.NumberOfFaces);

			var decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.header.PixelWidth);
			Assert.Equal((uint)image.Height, file.header.PixelHeight);

			using var outFs = File.OpenWrite("decoding_test_bc4.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc5Decode()
		{
			var file = KtxLoader.TestDecompressBc5Unorm;
			Assert.True(file.header.VerifyHeader());
			Assert.Equal((uint)1, file.header.NumberOfFaces);

			var decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.header.PixelWidth);
			Assert.Equal((uint)image.Height, file.header.PixelHeight);

			using var outFs = File.OpenWrite("decoding_test_bc5.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc7DecodeRgb()
		{
			var file = KtxLoader.TestDecompressBc7Rgb;
			Assert.True(file.header.VerifyHeader());
			Assert.Equal((uint)1, file.header.NumberOfFaces);

			var decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.header.PixelWidth);
			Assert.Equal((uint)image.Height, file.header.PixelHeight);

			using var outFs = File.OpenWrite("decoding_test_bc7_rgb.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc7DecodeUnorm()
		{
			var file = KtxLoader.TestDecompressBc7Unorm;
			Assert.True(file.header.VerifyHeader());
			Assert.Equal((uint)1, file.header.NumberOfFaces);

			var decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.header.PixelWidth);
			Assert.Equal((uint)image.Height, file.header.PixelHeight);

			using var outFs = File.OpenWrite("decoding_test_bc7_unorm.png");
			image.SaveAsPng(outFs);
		}

		[Fact]
		public void Bc7DecodeEveryBlockType()
		{
			var file = KtxLoader.TestDecompressBc7Types;
			Assert.True(file.header.VerifyHeader());
			Assert.Equal((uint)1, file.header.NumberOfFaces);

			var decoder = new BcDecoder();
			using var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.header.PixelWidth);
			Assert.Equal((uint)image.Height, file.header.PixelHeight);

			using var outFs = File.OpenWrite("decoding_test_bc7_types.png");
			image.SaveAsPng(outFs);
		}
	}
}
