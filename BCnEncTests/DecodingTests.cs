using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using BCnEncoder.TextureFormats;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests
{
	public class DecodingTests
	{
		[Theory]
		[InlineData("alpha_1_bc1a", 1)]
		[InlineData("alpha_1_bc2", 1)]
		[InlineData("alpha_1_bc3", 1)]
		[InlineData("alpha_1_bc7", 0)]
		[InlineData("alpha_1_bgra", 0)]
		[InlineData("alpha_2_bc1a", 1)]
		[InlineData("alpha_2_bc2", 1)]
		[InlineData("alpha_2_bc3", 1)]
		[InlineData("alpha_2_bc7", 0)]
		[InlineData("alpha_2_rgba", 0)]
		[InlineData("bc1_unorm", 0)]
		[InlineData("bc1a_unorm", 0)]
		[InlineData("bc2_unorm", 0)]
		[InlineData("bc3_unorm", 0)]
		[InlineData("bc4_unorm", 0)]
		[InlineData("bc5_unorm", 0)]
		[InlineData("bc7_unorm", 0)]
		[InlineData("bc7_unorm_alltypes", 0)]
		[InlineData("bc6h_ufloat", 1)]
		[InlineData("hdr_1_rgbe", 2)]
		[InlineData("hdr_1_xyze", 2)] // Need high tolerance for xyze :/ ?
		[InlineData("hdr_2_rgbe", 2)]
		[InlineData("raw_r8_unorm", 0)]
		[InlineData("raw_r8g8_unorm", 0)]
		[InlineData("raw_r8g8b8_unorm", 0)]
		[InlineData("raw_r8g8b8a8_unorm", 0)]
		[InlineData("raw_r16g16b16_sfloat", 1)]
		public void TestDecoding(string testImage, int tolerance)
		{
			var decoder = new BcDecoder();

			var encodedFile = ImageLoader.TestEncodedImages[testImage];
			var bcnData = encodedFile.Item1.ToTextureData();

			Assert.True(bcnData.Format != CompressionFormat.Unknown);
			Assert.True(bcnData.Width > 0);
			Assert.True(bcnData.Height > 0);
			Assert.True(bcnData.NumMips > 0);
			Assert.True(bcnData.NumFaces == 1);

			TestHelper.TestDecodingLdr(encodedFile.Item1, encodedFile.Item2, decoder, $"test_decode_{testImage}_{bcnData.Format}_mip{{0}}.png", tolerance);
		}
	}
}
