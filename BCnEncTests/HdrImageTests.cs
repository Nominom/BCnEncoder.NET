using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace BCnEncTests
{
	public class HdrImageTests
	{
		[Fact]
		public void LoadHdr()
		{
			using var stream = File.OpenRead("../../../testImages/test_hdr_kiara.hdr");
			var hdrImg = HdrImage.Read(stream);
			Assert.True(hdrImg.width > 0);
			Assert.True(hdrImg.height > 0);
			Assert.True(hdrImg.pixels.Length == hdrImg.width * hdrImg.height);
			
			var img = new Image<RgbaVector>(hdrImg.width, hdrImg.height);

			img.TryGetSinglePixelSpan(out var pixels);
			
			for (var i = 0; i < hdrImg.pixels.Length; i++)
			{
				pixels[i] = new RgbaVector(hdrImg.pixels[i].r, hdrImg.pixels[i].g, hdrImg.pixels[i].b);
			}

			img.SaveAsPng("test_hdr_load.png");

			var img2 = img.CloneAs<Rgba32>();

			TestHelper.AssertImagesEqual(HdrLoader.ReferenceKiara, img2, CompressionQuality.BestQuality);
		}
	}
}
