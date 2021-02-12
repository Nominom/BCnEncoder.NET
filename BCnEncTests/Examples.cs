using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
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
	}
}
