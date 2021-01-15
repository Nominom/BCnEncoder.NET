using System;
using System.IO;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using Xunit;

namespace BCnEncTests
{
	public class ClusterTests
	{
		[Fact]
		public void Clusterize()
		{
			using var testImage = ImageLoader.TestBlur1.Clone();

			if (!testImage.TryGetSinglePixelSpan(out var pixels))
			{
				throw new Exception("Cannot get pixel span.");
			}

			var numClusters = (testImage.Width / 32) * (testImage.Height / 32);

			var clusters = LinearClustering.ClusterPixels(pixels, testImage.Width, testImage.Height, numClusters, 10, 10);

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
				pixels[i] = pixC[clusters[i]].ToRgba32();
			}

			using var fs = File.OpenWrite("test_cluster.png");
			testImage.SaveAsPng(fs);
		}
	}
}
