using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;

namespace BCnEncTests.Support
{
	public static class HdrLoader
	{
		public static HdrImage TestHdrKiara { get; } = HdrImage.Read("../../../../BCnEncTests/testImages/test_hdr_kiara.hdr");
		public static HdrImage TestHdrProbe { get; } = HdrImage.Read("../../../../BCnEncTests/testImages/test_hdr_probe.hdr");
		public static DdsFile TestHdrKiaraDds { get; } =
			DdsLoader.LoadDdsFile("../../../../BCnEncTests/testImages/test_hdr_kiara_bc6h.dds");
		public static KtxFile TestHdrKiaraKtx { get; } =
			KtxLoader.LoadKtxFile("../../../../BCnEncTests/testImages/test_hdr_kiara_bc6h_ktx.ktx");
	}
}
