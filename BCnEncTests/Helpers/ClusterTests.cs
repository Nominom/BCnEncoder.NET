namespace BCnEncTests.Helpers
{
	// public class ClusterTests
	// {
	// 	[Fact]
	// 	public void Clusterize()
	// 	{
	// 		var testImage = ImageLoader.TestRawImages["diffuse_1"].Clone();
	//
	// 		var pix = TestHelper.GetSinglePixelArray(testImage);
	//
	// 		var pixels = MemoryMarshal.Cast<Rgba32, ColorRgba32>(pix).ToArray();
	//
	// 		var numClusters = (testImage.Width / 32) * (testImage.Height / 32);
	// 		var clusters = LinearClustering.ClusterPixels(pixels, testImage.Width, testImage.Height, numClusters, 10, 10);
	//
	// 		var pixC = new ColorYCbCr[numClusters];
	// 		var counts = new int[numClusters];
	//
	// 		for (var i = 0; i < pixels.Length; i++)
	// 		{
	// 			pixC[clusters[i]] += pixels[i].As<ColorYCbCr>();
	// 			counts[clusters[i]]++;
	// 		}
	//
	// 		for (var i = 0; i < numClusters; i++)
	// 		{
	// 			pixC[i] /= counts[i];
	// 		}
	//
	// 		for (var i = 0; i < pixels.Length; i++)
	// 		{
	// 			pixels[i] = pixC[clusters[i]].As<ColorRgba32>();
	// 			pix[i] = new Rgba32(pixels[i].r, pixels[i].g, pixels[i].b, pixels[i].a);
	// 		}
	//
	// 		TestHelper.SetSinglePixelArray(testImage, pix);
	//
	// 		using var fs = File.OpenWrite("test_cluster.png");
	// 		testImage.SaveAsPng(fs);
	// 	}
	// }
}
