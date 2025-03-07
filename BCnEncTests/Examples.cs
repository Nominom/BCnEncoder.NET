using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.TextureFormats;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using EncoderExtensions = BCnEncoder.ImageSharp.EncoderExtensions;

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
			using FileStream fs = File.OpenWrite("example.ktx");

			encoder.EncodeToStream<KtxFile>(image, fs);
		}

		public void DecodeImageSharp()
		{
			using FileStream fs = File.OpenRead("compressed_bc1.ktx");

			BcDecoder decoder = new BcDecoder();
			using Image<Rgba32> image = decoder.Decode<KtxFile>(fs).AsImageRgba32();

			using FileStream outFs = File.OpenWrite("decoding_test_bc1.png");
			image.SaveAsPng(outFs);
		}

		public void EncodeHdr()
		{
			using FileStream ifs = File.OpenRead("example.hdr");
			RadianceFile image = RadianceFile.Load(ifs);


			BcEncoder encoder = new BcEncoder();

			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Quality = CompressionQuality.Balanced;
			encoder.OutputOptions.Format = CompressionFormat.Bc6U;

			using FileStream fs = File.OpenWrite("example.dds");
			encoder.EncodeToStream<DdsFile>(image.ToTextureData(), fs);
		}

		public void DecodeHdr()
		{
			using FileStream fs = File.OpenRead("compressed_bc6.ktx");

			BcDecoder decoder = new BcDecoder();
			BCnTextureData decodedData = decoder.Decode<KtxFile>(fs);

			BCnTextureData rgbeData = decodedData.ConvertTo(CompressionFormat.Rgbe);

			RadianceFile image = rgbeData.AsTexture<RadianceFile>();

			using FileStream outFs = File.OpenWrite("decoded.hdr");
			image.WriteToStream(outFs);
		}
	}
}
