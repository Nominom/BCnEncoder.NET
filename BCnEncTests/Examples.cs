using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace BCnEncTests
{
	public class Examples
	{
		
		public void EncodeImageSharp()
		{
			using Image<Rgba32> image = Image.Load<Rgba32>("example.png");

			BcEncoder encoder = new BcEncoder();

			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Quality = CompressionQuality.Balanced;
			encoder.OutputOptions.Format = CompressionFormat.Bc1;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Ktx; //Change to Dds for a dds file.

			using FileStream fs = File.OpenWrite("example.ktx");
			encoder.EncodeToStream(image, fs);
		}
		
		public void DecodeImageSharp()
		{
			using FileStream fs = File.OpenRead("compressed_bc1.ktx");

			BcDecoder decoder = new BcDecoder();
			using Image<Rgba32> image = decoder.DecodeToImageRgba32(fs);
			
			using FileStream outFs = File.OpenWrite("decoding_test_bc1.png");
			image.SaveAsPng(outFs);
		}

		public void EncodeHdr()
		{
			HdrImage image = HdrImage.Read("example.hdr");
			

			BcEncoder encoder = new BcEncoder();

			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Quality = CompressionQuality.Balanced;
			encoder.OutputOptions.Format = CompressionFormat.Bc6U;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Ktx; //Change to Dds for a dds file.

			using FileStream fs = File.OpenWrite("example.ktx");
			encoder.EncodeToStreamHdr(image.PixelMemory, fs);
		}

		public void DecodeHdr()
		{
			using FileStream fs = File.OpenRead("compressed_bc6.ktx");

			BcDecoder decoder = new BcDecoder();
			Memory2D<ColorRgbFloat> pixels = decoder.DecodeHdr2D(fs);

			HdrImage image = new HdrImage(pixels.Span);

			using FileStream outFs = File.OpenWrite("decoded.hdr");
			image.Write(outFs);
		}
	}
}
