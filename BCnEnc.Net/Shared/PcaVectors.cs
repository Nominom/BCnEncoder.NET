using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Accord.Statistics.Analysis;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnComp.Net.Shared
{
	internal static class PcaVectors
	{

		public static void Create(Span<Rgba32> colors, out Vector3 mean, out Vector3 principalAxis) {
			double[][] data = new double[colors.Length][];
			for (int i = 0; i < colors.Length; i++) {
				data[i] = new double[3];
				data[i][0] = colors[i].R / 255.0;
				data[i][1] = colors[i].G / 255.0;
				data[i][2] = colors[i].B / 255.0;
			}
			PrincipalComponentAnalysis pca = new PrincipalComponentAnalysis();
			pca.Learn(data);
			var eigenVectors = pca.ComponentVectors;
			var pcaMeans = pca.Means;
			mean.X = (float)pcaMeans[0];
			mean.Y = (float)pcaMeans[1];
			mean.Z = (float)pcaMeans[2];

			principalAxis.X = (float)eigenVectors[0][0];
			principalAxis.Y =  (float)eigenVectors[0][1];
			principalAxis.Z =  (float)eigenVectors[0][2];
		}

		public static void GetExtremePoints(Span<Rgba32> colors, Vector3 mean, Vector3 principalAxis, out Vector3 min,
			out Vector3 max) {

			float minD = 0;
			float maxD = 0;

			for (int i = 0; i < colors.Length; i++) {
				var colorVec = new Vector3(colors[i].R / 255f, colors[i].G / 255f, colors[i].B / 255f);
				
				var v = colorVec - mean;
				var d = Vector3.Dot(v, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			min = mean + (principalAxis * minD);
			max = mean + (principalAxis * maxD);
		}
	}
}
