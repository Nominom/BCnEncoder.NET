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
			using FileStream fs = File.OpenRead(@"..\..\..\testImages\test_decompress_bc1.ktx");
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
	}
}
