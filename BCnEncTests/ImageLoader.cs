using System.IO;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncTests
{
	public static class ImageLoader
	{
		public static Image<Rgba32> TestDiffuse1 { get; } = LoadTestImage("../../../testImages/test_diffuse_1_512.jpg");
		public static Image<Rgba32> TestBlur1 { get; } = LoadTestImage("../../../testImages/test_blur_1_512.jpg");
		public static Image<Rgba32> TestNormal1 { get; } = LoadTestImage("../../../testImages/test_normal_1_512.jpg");
		public static Image<Rgba32> TestHeight1 { get; } = LoadTestImage("../../../testImages/test_height_1_512.jpg");
		public static Image<Rgba32> TestGradient1 { get; } = LoadTestImage("../../../testImages/test_gradient_1_512.jpg");

		public static Image<Rgba32> TestTransparentSprite1 { get; } = LoadTestImage("../../../testImages/test_transparent.png");
		public static Image<Rgba32> TestAlphaGradient1 { get; } = LoadTestImage("../../../testImages/test_alphagradient_1_512.png");
		public static Image<Rgba32> TestAlpha1 { get; } = LoadTestImage("../../../testImages/test_alpha_1_512.png");
		public static Image<Rgba32> TestRedGreen1 { get; } = LoadTestImage("../../../testImages/test_red_green_1_64.png");
		public static Image<Rgba32> TestRgbHard1 { get; } = LoadTestImage("../../../testImages/test_rgb_hard_1.png");
		public static Image<Rgba32> TestLenna { get; } = LoadTestImage("../../../testImages/test_lenna_512.png");

		public static Image<Rgba32>[] TestCubemap { get; } = {
			LoadTestImage("../../../testImages/cubemap/right.png"),
			LoadTestImage("../../../testImages/cubemap/left.png"),
			LoadTestImage("../../../testImages/cubemap/top.png"),
			LoadTestImage("../../../testImages/cubemap/bottom.png"),
			LoadTestImage("../../../testImages/cubemap/back.png"),
			LoadTestImage("../../../testImages/cubemap/forward.png")
		};

		private static Image<Rgba32> LoadTestImage(string filename)
		{
			return Image.Load<Rgba32>(filename);
		}
	}

	public static class DdsLoader
	{
		public static DdsFile TestDecompressBc1 { get; } = LoadDdsFile("../../../testImages/test_decompress_bc1.dds");
		public static DdsFile TestDecompressBc1A { get; } = LoadDdsFile("../../../testImages/test_decompress_bc1a.dds");
		public static DdsFile TestDecompressBc7 { get; } = LoadDdsFile("../../../testImages/test_decompress_bc7.dds");
		public static DdsFile TestDecompressRgba { get; } = LoadDdsFile("../../../testImages/test_decompress_rgba.dds");

		private static DdsFile LoadDdsFile(string filename)
		{
			return DdsFile.Load(File.OpenRead(filename));
		}
	}

	public static class KtxLoader
	{
		public static KtxFile TestDecompressBc1 { get; } = LoadKtxFile("../../../testImages/test_decompress_bc1.ktx");
		public static KtxFile TestDecompressBc1A { get; } = LoadKtxFile("../../../testImages/test_decompress_bc1a.ktx");
		public static KtxFile TestDecompressBc2 { get; } = LoadKtxFile("../../../testImages/test_decompress_bc2.ktx");
		public static KtxFile TestDecompressBc3 { get; } = LoadKtxFile("../../../testImages/test_decompress_bc3.ktx");
		public static KtxFile TestDecompressBc4Unorm { get; } = LoadKtxFile("../../../testImages/test_decompress_bc4_unorm.ktx");
		public static KtxFile TestDecompressBc5Unorm { get; } = LoadKtxFile("../../../testImages/test_decompress_bc5_unorm.ktx");
		public static KtxFile TestDecompressBc7Rgb { get; } = LoadKtxFile("../../../testImages/test_decompress_bc7_rgb.ktx");
		public static KtxFile TestDecompressBc7Types { get; } = LoadKtxFile("../../../testImages/test_decompress_bc7_types.ktx");
		public static KtxFile TestDecompressBc7Unorm { get; } = LoadKtxFile("../../../testImages/test_decompress_bc7_unorm.ktx");

		private static KtxFile LoadKtxFile(string filename)
		{
			return KtxFile.Load(File.OpenRead(filename));
		}
	}
}
