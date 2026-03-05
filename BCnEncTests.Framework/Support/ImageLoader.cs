using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using CommunityToolkit.HighPerformance;

namespace BCnEncTests.Support
{
	public static class ImageLoader
	{
		public static Memory2D<ColorRgba32> TestDiffuse1 { get; } = LoadTestImage("../../../../BCnEncTests/testImages/test_diffuse_1_512.jpg");
		public static Memory2D<ColorRgba32> TestBlur1 { get; } = LoadTestImage("../../../../BCnEncTests/testImages/test_blur_1_512.jpg");
		public static Memory2D<ColorRgba32> TestNormal1 { get; } = LoadTestImage("../../../../BCnEncTests/testImages/test_normal_1_512.jpg");
		public static Memory2D<ColorRgba32> TestHeight1 { get; } = LoadTestImage("../../../../BCnEncTests/testImages/test_height_1_512.jpg");
		public static Memory2D<ColorRgba32> TestGradient1 { get; } = LoadTestImage("../../../../BCnEncTests/testImages/test_gradient_1_512.jpg");

		public static Memory2D<ColorRgba32> TestTransparentSprite1 { get; } = LoadTestImage("../../../../BCnEncTests/testImages/test_transparent.png");
		public static Memory2D<ColorRgba32> TestAlphaGradient1 { get; } = LoadTestImage("../../../../BCnEncTests/testImages/test_alphagradient_1_512.png");
		public static Memory2D<ColorRgba32> TestAlpha1 { get; } = LoadTestImage("../../../../BCnEncTests/testImages/test_alpha_1_512.png");
		public static Memory2D<ColorRgba32> TestRedGreen1 { get; } = LoadTestImage("../../../../BCnEncTests/testImages/test_red_green_1_64.png");
		public static Memory2D<ColorRgba32> TestRgbHard1 { get; } = LoadTestImage("../../../../BCnEncTests/testImages/test_rgb_hard_1.png");
		public static Memory2D<ColorRgba32> TestLenna { get; } = LoadTestImage("../../../../BCnEncTests/testImages/test_lenna_512.png");
		public static Memory2D<ColorRgba32> TestDecodingBc5Reference { get; } = LoadTestImage("../../../../BCnEncTests/testImages/decoding_dds_bc5_reference.png");

		public static Memory2D<ColorRgba32>[] TestCubemap { get; } = {
			LoadTestImage("../../../../BCnEncTests/testImages/cubemap/right.png"),
			LoadTestImage("../../../../BCnEncTests/testImages/cubemap/left.png"),
			LoadTestImage("../../../../BCnEncTests/testImages/cubemap/top.png"),
			LoadTestImage("../../../../BCnEncTests/testImages/cubemap/bottom.png"),
			LoadTestImage("../../../../BCnEncTests/testImages/cubemap/back.png"),
			LoadTestImage("../../../../BCnEncTests/testImages/cubemap/forward.png")
		};

		internal static Memory2D<ColorRgba32> LoadTestImage(string filename)
		{
			using (var bmp = new Bitmap(filename))
			{
				return FromBitmap(bmp);
			}
		}

		internal static unsafe Memory2D<ColorRgba32> FromBitmap(Bitmap bmp)
		{
			var pixels = new ColorRgba32[bmp.Width * bmp.Height];
			var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
				ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			byte* ptr = (byte*)data.Scan0;
			for (int i = 0; i < pixels.Length; i++)
			{
				// GDI+ Format32bppArgb memory order: B,G,R,A
				pixels[i] = new ColorRgba32(ptr[2], ptr[1], ptr[0], ptr[3]);
				ptr += 4;
			}
			bmp.UnlockBits(data);
			return new Memory2D<ColorRgba32>(pixels,  bmp.Width, bmp.Height);
		}
	}

	public static class DdsLoader
	{
		public const string TestDecompressBc1Name = "../../../../BCnEncTests/testImages/test_decompress_bc1.dds";
		public const string TestDecompressBc1AName = "../../../../BCnEncTests/testImages/test_decompress_bc1a.dds";
		public const string TestDecompressBc7Name = "../../../../BCnEncTests/testImages/test_decompress_bc7.dds";
		public const string TestDecompressBc5Name = "../../../../BCnEncTests/testImages/decoding_dds_bc5.dds";
		public const string TestDecompressRgbaName = "../../../../BCnEncTests/testImages/test_decompress_rgba.dds";

		public static DdsFile TestDecompressBc1 { get; } = LoadDdsFile(TestDecompressBc1Name);
		public static DdsFile TestDecompressBc1A { get; } = LoadDdsFile(TestDecompressBc1AName);
		public static DdsFile TestDecompressBc5 { get; } = LoadDdsFile(TestDecompressBc5Name);
		public static DdsFile TestDecompressBc7 { get; } = LoadDdsFile(TestDecompressBc7Name);
		public static DdsFile TestDecompressRgba { get; } = LoadDdsFile(TestDecompressRgbaName);

		internal static DdsFile LoadDdsFile(string filename)
		{
			using (var fs = File.OpenRead(filename))
			{
				return DdsFile.Load(fs);
			}
		}
	}

	public static class KtxLoader
	{
		public static KtxFile TestDecompressBc1 { get; } = LoadKtxFile("../../../../BCnEncTests/testImages/test_decompress_bc1.ktx");
		public static KtxFile TestDecompressBc1A { get; } = LoadKtxFile("../../../../BCnEncTests/testImages/test_decompress_bc1a.ktx");
		public static KtxFile TestDecompressBc2 { get; } = LoadKtxFile("../../../../BCnEncTests/testImages/test_decompress_bc2.ktx");
		public static KtxFile TestDecompressBc3 { get; } = LoadKtxFile("../../../../BCnEncTests/testImages/test_decompress_bc3.ktx");
		public static KtxFile TestDecompressBc4Unorm { get; } = LoadKtxFile("../../../../BCnEncTests/testImages/test_decompress_bc4_unorm.ktx");
		public static KtxFile TestDecompressBc5Unorm { get; } = LoadKtxFile("../../../../BCnEncTests/testImages/test_decompress_bc5_unorm.ktx");
		public static KtxFile TestDecompressBc7Rgb { get; } = LoadKtxFile("../../../../BCnEncTests/testImages/test_decompress_bc7_rgb.ktx");
		public static KtxFile TestDecompressBc7Types { get; } = LoadKtxFile("../../../../BCnEncTests/testImages/test_decompress_bc7_types.ktx");
		public static KtxFile TestDecompressBc7Unorm { get; } = LoadKtxFile("../../../../BCnEncTests/testImages/test_decompress_bc7_unorm.ktx");

		internal static KtxFile LoadKtxFile(string filename)
		{
			using (var fs = File.OpenRead(filename))
			{
				return KtxFile.Load(fs);
			}
		}
	}
}
