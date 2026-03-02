using System.IO;
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
			using var stream = File.OpenRead("../../../../BCnEncTests/testImages/test_hdr_kiara.hdr");
			var hdrImg = HdrImage.Read(stream);
			Assert.True(hdrImg.width > 0);
			Assert.True(hdrImg.height > 0);
			Assert.True(hdrImg.pixels.Length == hdrImg.width * hdrImg.height);

			// Save as HDR to verify round-trip
			var hdrOut = new HdrImage(new Span2D<ColorRgbFloat>(hdrImg.pixels, hdrImg.height, hdrImg.width));
			using var outStream = File.OpenWrite("test_hdr_load.hdr");
			hdrOut.Write(outStream);
		}
	}
}
