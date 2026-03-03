using System;
using System.IO;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using CommunityToolkit.HighPerformance;
using Xunit;

namespace BCnEncTests
{
	public class HdrImageTests
	{
		[Fact]
		public void LoadHdr()
		{
			using (var stream = File.OpenRead("../../../../BCnEncTests/testImages/test_hdr_kiara.hdr"))
			{
				var hdrImg = HdrImage.Read(stream);
				Assert.True(hdrImg.width > 0);
				Assert.True(hdrImg.height > 0);
				Assert.True(hdrImg.pixels.Length == hdrImg.width * hdrImg.height);

				var rgba = new ColorRgba32[hdrImg.pixels.Length];
				for (var i = 0; i < hdrImg.pixels.Length; i++)
				{
					var p = hdrImg.pixels[i];
					rgba[i] = new ColorRgba32(
						(byte)(Math.Max(0, Math.Min(1, p.r)) * 255 + 0.5f),
						(byte)(Math.Max(0, Math.Min(1, p.g)) * 255 + 0.5f),
						(byte)(Math.Max(0, Math.Min(1, p.b)) * 255 + 0.5f),
						255);
				}
				var converted = new Memory2D<ColorRgba32>(rgba, hdrImg.height, hdrImg.width);
				var reference = ImageLoader.LoadTestImage("../../../../BCnEncTests/testImages/test_hdr_kiara.png");
				TestHelper.AssertImagesEqual(reference, converted, CompressionQuality.BestQuality);
			}
		}

		[Fact]
		public async Task DecodeAllMipMapsHdrStreamAsync()
		{
			var encoder = new BcEncoder();
			encoder.OutputOptions.Format = CompressionFormat.Bc6U;
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;

			var decoder = new BcDecoder();
			var input = HdrLoader.TestHdrKiara;
			var ktxWithMips = encoder.EncodeToKtxHdr(new Memory2D<ColorRgbFloat>(input.pixels, input.height, input.width));
			using (var ms = new MemoryStream())
			{
				ktxWithMips.Write(ms);
				ms.Position = 0;

				var images = await decoder.DecodeAllMipMapsHdr2DAsync(ms);
				Assert.Equal((int)ktxWithMips.header.NumberOfMipmapLevels, images.Length);
				Assert.True(images.Length > 1);
			}
		}
	}
}
