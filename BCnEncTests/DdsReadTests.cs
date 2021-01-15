using System;
using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using Xunit;

namespace BCnEncTests
{
	public class DdsReadTests
	{
		[Fact]
		public void ReadRgba()
		{
			var file = DdsLoader.TestDecompressRgba;
			Assert.Equal(DxgiFormat.DxgiFormatR8G8B8A8Unorm, file.header.ddsPixelFormat.DxgiFormat);
			Assert.Equal(file.header.dwMipMapCount, (uint)file.Faces[0].MipMaps.Length);

			var decoder = new BcDecoder();
			var images = decoder.DecodeAllMipMaps(file);

			Assert.Equal((uint)images[0].Width, file.header.dwWidth);
			Assert.Equal((uint)images[0].Height, file.header.dwHeight);

			for (var i = 0; i < images.Length; i++)
			{
				using var outFs = File.OpenWrite($"decoding_test_dds_rgba_mip{i}.png");
				images[i].SaveAsPng(outFs);
				images[i].Dispose();
			}
		}

		[Fact]
		public void ReadBc1()
		{
			var file = DdsLoader.TestDecompressBc1;
			Assert.Equal(DxgiFormat.DxgiFormatBc1Unorm, file.header.ddsPixelFormat.DxgiFormat);
			Assert.Equal(file.header.dwMipMapCount, (uint)file.Faces[0].MipMaps.Length);


			var decoder = new BcDecoder();
			var images = decoder.DecodeAllMipMaps(file);

			Assert.Equal((uint)images[0].Width, file.header.dwWidth);
			Assert.Equal((uint)images[0].Height, file.header.dwHeight);

			for (var i = 0; i < images.Length; i++)
			{
				using var outFs = File.OpenWrite($"decoding_test_dds_bc1_mip{i}.png");
				images[i].SaveAsPng(outFs);
				images[i].Dispose();
			}
		}

		[Fact]
		public void ReadBc1A()
		{
			var file = DdsLoader.TestDecompressBc1A;
			Assert.Equal(DxgiFormat.DxgiFormatBc1Unorm, file.header.ddsPixelFormat.DxgiFormat);
			Assert.Equal(file.header.dwMipMapCount, (uint)file.Faces[0].MipMaps.Length);


			var decoder = new BcDecoder();
			decoder.InputOptions.DdsBc1ExpectAlpha = true;
			var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.header.dwWidth);
			Assert.Equal((uint)image.Height, file.header.dwHeight);

			if (!image.TryGetSinglePixelSpan(out var pixels))
			{
				throw new Exception("Cannot get pixel span.");
			}
			Assert.Contains(pixels.ToArray(), x => x.A == 0);

			using var outFs = File.OpenWrite("decoding_test_dds_bc1a.png");
			image.SaveAsPng(outFs);
			image.Dispose();
		}

		[Fact]
		public void ReadBc7()
		{
			using var fs = File.OpenRead(@"../../../testImages/test_decompress_bc7.dds");
			var decoder = new BcDecoder();
			var images = decoder.DecodeAllMipMaps(fs);

			for (var i = 0; i < images.Length; i++)
			{
				using var outFs = File.OpenWrite($"decoding_test_dds_bc7_mip{i}.png");
				images[i].SaveAsPng(outFs);
				images[i].Dispose();
			}
		}

		[Fact]
		public void ReadFromStream()
		{
			using var fs = File.OpenRead(@"../../../testImages/test_decompress_bc1.dds");

			var decoder = new BcDecoder();
			var images = decoder.DecodeAllMipMaps(fs);

			for (var i = 0; i < images.Length; i++)
			{
				using var outFs = File.OpenWrite($"decoding_test_dds_stream_bc1_mip{i}.png");
				images[i].SaveAsPng(outFs);
				images[i].Dispose();
			}
		}
	}
}
