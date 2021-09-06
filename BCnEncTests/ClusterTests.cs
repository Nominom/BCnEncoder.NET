using System;
using System.IO;
using System.Runtime.InteropServices;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace BCnEncTests
{
	public class ClusterTests
	{
		[Fact]
		public void Clusterize()
		{
			var testImage = ImageLoader.TestBlur1.Clone();

			if (!testImage.TryGetSinglePixelSpan(out var pix))
			{
				throw new Exception("Cannot get pixel span.");
			}

			var pixels = MemoryMarshal.Cast<Rgba32, ColorRgba32>(pix).ToArray();

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
				pixels[i] = pixC[clusters[i]].ToColorRgba32();
				pix[i] = new Rgba32(pixels[i].r, pixels[i].g, pixels[i].b, pixels[i].a);
			}

			using var fs = File.OpenWrite("test_cluster.png");
			testImage.SaveAsPng(fs);
		}
	}
}
