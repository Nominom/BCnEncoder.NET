using System;
using System.IO;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using CommunityToolkit.HighPerformance;
using Xunit;

namespace BCnEncTests
{
	public class ClusterTests
	{
		[Fact]
		public void Clusterize()
		{
			var original = ImageLoader.TestBlur1;
			int height = original.Height;
			int width = original.Width;

			// Copy pixels from the source image
			var pixels = new ColorRgba32[height * width];
			var srcSpan = original.Span;
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++)
					pixels[y * width + x] = srcSpan[y, x];

			var numClusters = (width / 32) * (height / 32);
			var clusters = LinearClustering.ClusterPixels(pixels, width, height, numClusters, 10, 10);

			var pixC = new ColorYCbCr[numClusters];
			var counts = new int[numClusters];

			for (var i = 0; i < pixels.Length; i++)
			{
				pixC[clusters[i]] += new ColorYCbCr(pixels[i]);
				counts[clusters[i]]++;
			}

			for (var i = 0; i < numClusters; i++)
			{
				pixC[i] /= counts[i];
			}

			for (var i = 0; i < pixels.Length; i++)
			{
				pixels[i] = pixC[clusters[i]].ToColorRgba32();
			}

			var result = new Memory2D<ColorRgba32>(pixels, height, width);
			using var fs = File.OpenWrite("test_cluster.png");
			TestHelper.SaveAsPng(result, fs);
		}
	}
}
