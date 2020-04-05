using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncTests
{
	public static class ImageLoader {
		public static Image<Rgba32> testDiffuse1 { get; } = LoadTestImage("../../../testImages/test_diffuse_1_512.jpg");
		public static Image<Rgba32> testNormal1 { get; } = LoadTestImage("../../../testImages/test_normal_1_512.jpg");
		public static Image<Rgba32> testHeight1 { get; } = LoadTestImage("../../../testImages/test_height_1_512.jpg");
		public static Image<Rgba32> testGradient1 { get; } = LoadTestImage("../../../testImages/test_gradient_1_512.jpg");


		private static Image<Rgba32> LoadTestImage(string filename) {
			return Image.Load<Rgba32>(filename);
		}
	}
}
