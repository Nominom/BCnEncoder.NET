using System;
using System.Collections.Generic;
using System.Text;
using BCnEncoder.Shared.ImageFiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncTests.Support
{
	public static class HdrLoader
	{
		public static HdrImage TestHdrKiara { get; } = HdrImage.Read("../../../testImages/test_hdr_kiara.hdr");
		public static Image<Rgba32> ReferenceKiara { get; } = ImageLoader.LoadTestImage("../../../testImages/test_hdr_kiara.png");
		public static DdsFile TestHdrKiaraDds { get; } =
			DdsLoader.LoadDdsFile("../../../testImages/test_hdr_kiara_bc6h.dds");
		public static KtxFile TestHdrKiaraKtx { get; } =
			KtxLoader.LoadKtxFile("../../../testImages/test_hdr_kiara_bc6h_ktx.ktx");
		
	}
}
