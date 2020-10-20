using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace BCnEncTests
{
	public class ClusterTests
	{
		[Fact]
		public void Clusterize() {
			using var testImage = ImageLoader.testBlur1.Clone();
			
			if (!testImage.TryGetSinglePixelSpan(out var pixels)) {
				throw new Exception("Cannot get pixel span.");
			}

			int numClusters = (testImage.Width / 32) * (testImage.Height / 32);
			
			var clusters = LinearClustering.ClusterPixels(pixels, testImage.Width, testImage.Height, numClusters, 10, 10);

			ColorYCbCr[] pixC = new ColorYCbCr[numClusters];
			int[] counts = new int[numClusters];
			for (int i = 0; i < pixels.Length; i++) {
				pixC[clusters[i]] += new ColorYCbCr(pixels[i]);
				counts[clusters[i]]++;
			}
			for (int i = 0; i < numClusters; i++) {
				pixC[i] /= counts[i];
			}
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = pixC[clusters[i]].ToRgba32();
			}

			using var fs = File.OpenWrite("test_cluster.png");
			testImage.SaveAsPng(fs);
		}
	}
}
