using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using BCnEncTests.Support;
using SixLabors.ImageSharp;
using Xunit;

namespace BCnEncTests
{
	public class DdsReadTests
	{
		[Fact]
		public void ReadRgba()
		{
			TestHelper.ExecuteDdsReadingTest(DdsLoader.TestDecompressRgba, DxgiFormat.DxgiFormatR8G8B8A8Unorm, "decoding_test_dds_rgba_mip{0}.png");
		}

		[Fact]
		public void ReadBc1()
		{
			TestHelper.ExecuteDdsReadingTest(DdsLoader.TestDecompressBc1, DxgiFormat.DxgiFormatBc1Unorm, "decoding_test_dds_bc1_mip{0}.png");
		}

		[Fact]
		public void ReadBc1A()
		{
			TestHelper.ExecuteDdsReadingTest(DdsLoader.TestDecompressBc1A, DxgiFormat.DxgiFormatBc1Unorm, "decoding_test_dds_bc1a_mip{0}.png");
		}

		[Fact]
		public void ReadBc7()
		{
			TestHelper.ExecuteDdsReadingTest(DdsLoader.TestDecompressBc7, DxgiFormat.DxgiFormatUnknown, "decoding_test_dds_bc7_mip{0}.png");
		}

		[Fact]
		public void ReadFromStream()
		{
			using var fs = File.OpenRead(DdsLoader.TestDecompressBc1Name);

			var decoder = new BcDecoder();
			var images = decoder.DecodeAllMipMapsToImageRgba32(fs);

			for (var i = 0; i < images.Length; i++)
			{
				using var outFs = File.OpenWrite($"decoding_test_dds_stream_bc1_mip{i}.png");
				images[i].SaveAsPng(outFs);
				images[i].Dispose();
			}
		}
	}
}
