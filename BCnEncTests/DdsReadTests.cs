using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Rgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;

namespace BCnEncTests
{
	public class DdsReadTests
	{
		[Fact]
		public void ReadRgba() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_rgba.dds");
			DdsFile file = DdsFile.Load(fs);
			Assert.Equal(DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM, file.Header.ddsPixelFormat.DxgiFormat);
			Assert.Equal(file.Header.dwMipMapCount, (uint)file.Faces[0].MipMaps.Length);

			BcDecoder decoder = new BcDecoder();
			var images = decoder.DecodeAllMipMaps(file);

			Assert.Equal((uint)images[0].Width, file.Header.dwWidth);
			Assert.Equal((uint)images[0].Height, file.Header.dwHeight);

			for (int i = 0; i < images.Length; i++)
			{
				using var img =
					Image.LoadPixelData<Rgba32>(images[i].data, images[i].Width, images[i].Height);
				using FileStream outFs = File.OpenWrite($"decoding_test_dds_rgba_mip{i}.png");
				img.SaveAsPng(outFs);
			}
		}

		[Fact]
		public void ReadBc1() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc1.dds");
			DdsFile file = DdsFile.Load(fs);
			Assert.Equal(DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM, file.Header.ddsPixelFormat.DxgiFormat);
			Assert.Equal(file.Header.dwMipMapCount, (uint)file.Faces[0].MipMaps.Length);


			BcDecoder decoder = new BcDecoder();
			var images = decoder.DecodeAllMipMaps(file);

			Assert.Equal((uint)images[0].Width, file.Header.dwWidth);
			Assert.Equal((uint)images[0].Height, file.Header.dwHeight);

			for (int i = 0; i < images.Length; i++) {
				using var img =
					Image.LoadPixelData<Rgba32>(images[i].data, images[i].Width, images[i].Height);
				using FileStream outFs = File.OpenWrite($"decoding_test_dds_bc1_mip{i}.png");
				img.SaveAsPng(outFs);
			}
		}

		[Fact]
		public void ReadBc1a() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc1a.dds");
			DdsFile file = DdsFile.Load(fs);
			Assert.Equal(DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM, file.Header.ddsPixelFormat.DxgiFormat);
			Assert.Equal(file.Header.dwMipMapCount, (uint)file.Faces[0].MipMaps.Length);


			BcDecoder decoder = new BcDecoder();
			decoder.InputOptions.ddsBc1ExpectAlpha = true;
			var image = decoder.Decode(file);

			Assert.Equal((uint)image.Width, file.Header.dwWidth);
			Assert.Equal((uint)image.Height, file.Header.dwHeight);

			using var img =
				Image.LoadPixelData<Rgba32>(image.data, image.Width, image.Height);
			using FileStream outFs = File.OpenWrite($"decoding_test_dds_bc1a.png");
			img.SaveAsPng(outFs);
		}

		[Fact]
		public void ReadBc7() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc7.dds");
			BcDecoder decoder = new BcDecoder();
			var images = decoder.DecodeAllMipMaps(fs);

			for (int i = 0; i < images.Length; i++) {
				using var img =
					Image.LoadPixelData<Rgba32>(images[i].data, images[i].Width, images[i].Height);
				using FileStream outFs = File.OpenWrite($"decoding_test_dds_bc7_mip{i}.png");
				img.SaveAsPng(outFs);
			}
		}

		[Fact]
		public void ReadFromStream() {
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc1.dds");

			BcDecoder decoder = new BcDecoder();
			var images = decoder.DecodeAllMipMaps(fs);

			for (int i = 0; i < images.Length; i++) {
				using var img =
					Image.LoadPixelData<Rgba32>(images[i].data, images[i].Width, images[i].Height);
				using FileStream outFs = File.OpenWrite($"decoding_test_dds_stream_bc1_mip{i}.png");
				img.SaveAsPng(outFs);
			}
		}
	}
}
