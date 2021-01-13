using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncTests
{
	public static class ImageLoader {
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
		
		public static Image<Rgba32>[] TestCubemap { get; } = new [] {
			LoadTestImage("../../../testImages/cubemap/right.png"),
			LoadTestImage("../../../testImages/cubemap/left.png"),
			LoadTestImage("../../../testImages/cubemap/top.png"),
			LoadTestImage("../../../testImages/cubemap/bottom.png"),
			LoadTestImage("../../../testImages/cubemap/back.png"),
			LoadTestImage("../../../testImages/cubemap/forward.png")
		};

		private static Image<Rgba32> LoadTestImage(string filename) {
			return Image.Load<Rgba32>(filename);
		}
	}
}
