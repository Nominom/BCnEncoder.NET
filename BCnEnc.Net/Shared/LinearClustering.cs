using System;
using System.Collections.Generic;
using System.Text;
using Accord;
using Accord.Math.Geometry;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace BCnEnc.Net.Shared
{
	/// <summary>
	/// Simple Linear Iterative Clustering.
	/// </summary>
	public static class LinearClustering
	{

		private struct LabXy
		{
			public float l;
			public float a;
			public float b;
			public float x;
			public float y;


			public static LabXy operator +(LabXy left, LabXy right)
			{
				return new LabXy()
				{
					l = left.l + right.l,
					a = left.a + right.a,
					b = left.b + right.b,
					x = left.x + right.x,
					y = left.y + right.y,
				};
			}

			public static LabXy operator /(LabXy left, int right)
			{
				return new LabXy()
				{
					l = left.l / right,
					a = left.a / right,
					b = left.b / right,
					x = left.x / right,
					y = left.y / right,
				};
			}
		}

		private struct ClusterCenter
		{
			public float l;
			public float a;
			public float b;
			public float x;
			public float y;
			public int count;

			public ClusterCenter(LabXy labxy)
			{
				this.l = labxy.l;
				this.a = labxy.a;
				this.b = labxy.b;
				this.x = labxy.x;
				this.y = labxy.y;
				count = 0;
			}

			public readonly float Distance(LabXy other, float m, float s)
			{
				float dLab = MathF.Sqrt(
					MathF.Pow(l - other.l, 2) +
					MathF.Pow(a - other.a, 2) +
					MathF.Pow(b - other.b, 2));

				float dXy = MathF.Sqrt(
					MathF.Pow(x - other.x, 2) +
					MathF.Pow(y - other.y, 2));
				return dLab + (m / s) * dXy;
			}

			public readonly float Distance(ClusterCenter other, float m, float s)
			{
				float dLab = MathF.Sqrt(
					(l - other.l) * (l - other.l) +
					(a - other.a) * (a - other.a) +
					(b - other.b) * (b - other.b));

				float dXy = MathF.Sqrt(
					(x - other.x) * (x - other.x) +
					(y - other.y) * (y - other.y));
				return dLab + m / s * dXy;
			}

			public static ClusterCenter operator +(ClusterCenter left, LabXy right)
			{
				return new ClusterCenter()
				{
					l = left.l + right.l,
					a = left.a + right.a,
					b = left.b + right.b,
					x = left.x + right.x,
					y = left.y + right.y,
					count = left.count + 1
				};
			}

			public static ClusterCenter operator /(ClusterCenter left, int right)
			{
				return new ClusterCenter()
				{
					l = left.l / right,
					a = left.a / right,
					b = left.b / right,
					x = left.x / right,
					y = left.y / right,
					count = left.count
				};
			}
		}

		/// <summary>
		/// The greater the value of M,
		/// the more spatial proximity is emphasized and the more compact the cluster,
		/// M should be in range of 1 to 20.
		/// </summary>
		public static int[] ClusterPixels(ReadOnlySpan<Rgba32> pixels, int width, int height,
			int clusters, float m = 10, int maxIterations = 10, bool enforceConnectivity = true)
		{

			if (clusters < 2)
			{
				throw new ArgumentException("Number of clusters should be more than 1");
			}

			//Grid interval S
			float S = MathF.Sqrt(pixels.Length / (float)clusters);
			int[] clusterIndices = new int[pixels.Length];

			var labXys = ConvertToLabXy(pixels, width, height);


			Span<ClusterCenter> clusterCenters = InitialClusterCenters(width, height, clusters, S, labXys);
			Span<ClusterCenter> previousCenters = new ClusterCenter[clusters];

			float Error = 999;
			const float threshold = 0.1f;
			int iter = 0;
			while (Error > threshold)
			{
				if (maxIterations > 0 && iter >= maxIterations)
				{
					break;
				}
				iter++;

				clusterCenters.CopyTo(previousCenters);

				Array.Fill(clusterIndices, -1);

				// Find closest cluster for pixels
				for (int j = 0; j < clusters; j++)
				{
					int xL = Math.Max(0, (int)(clusterCenters[j].x - S));
					int xH = Math.Min(width, (int)(clusterCenters[j].x + S));
					int yL = Math.Max(0, (int)(clusterCenters[j].y - S));
					int yH = Math.Min(height, (int)(clusterCenters[j].y + S));

					for (int x = xL; x < xH; x++)
					{
						for (int y = yL; y < yH; y++)
						{
							int i = x + y * width;

							if (clusterIndices[i] == -1)
							{
								clusterIndices[i] = j;
							}
							else
							{
								float prevDistance = clusterCenters[clusterIndices[i]].Distance(labXys[i], m, S);
								float distance = clusterCenters[j].Distance(labXys[i], m, S);
								if (distance < prevDistance)
								{
									clusterIndices[i] = j;
								}
							}
						}
					}
				}

				Error = RecalculateCenters(clusters, m, labXys, clusterIndices, previousCenters, S, ref clusterCenters);
			}

			if (enforceConnectivity) {
				clusterIndices = EnforceConnectivity(clusterIndices, width, height, clusters);
			}
			
			return clusterIndices;
		}

		private static float RecalculateCenters(int clusters, float m, LabXy[] labXys, int[] clusterIndices,
			Span<ClusterCenter> previousCenters, float S, ref Span<ClusterCenter> clusterCenters)
		{
			clusterCenters.Clear();
			for (int i = 0; i < labXys.Length; i++)
			{
				int clusterIndex = clusterIndices[i];
				// Sometimes a pixel is out of the range of any cluster,
				// in that case, find the nearest cluster and add it to it
				if (clusterIndex == -1)
				{
					int bestCluster = 0;
					float bestDistance = previousCenters[0].Distance(labXys[i], m, S);
					for (int j = 1; j < clusters; j++) {
						float dist = previousCenters[j].Distance(labXys[i], m, S);
						if (dist < bestDistance) {
							bestDistance = dist;
							bestCluster = j;
						}
					}
					clusterCenters[bestCluster] += labXys[i];
					clusterIndices[i] = bestCluster;
				}
				else {
					clusterCenters[clusterIndex] += labXys[i];
				}
			}

			float error = 0;
			for (int i = 0; i < clusters; i++)
			{
				if (clusterCenters[i].count > 0)
				{
					clusterCenters[i] /= clusterCenters[i].count;
					error += clusterCenters[i].Distance(previousCenters[i], m, S);
				}
			}

			error /= clusters;
			return error;
		}

		private static ClusterCenter[] InitialClusterCenters(int width, int height, int clusters, float S, LabXy[] labXys)
		{
			ClusterCenter[] clusterCenters = new ClusterCenter[clusters];

			if (clusters == 2) {
				int x0 = (int)MathF.Floor(width * 0.333f);
				int y0 = (int)MathF.Floor(height * 0.333f);

				int x1 = (int)MathF.Floor(width * 0.666f);
				int y1 = (int)MathF.Floor(height * 0.666f);

				int i0 = x0 + y0 * width;
				clusterCenters[0] = new ClusterCenter(labXys[i0]);

				int i1 = x1 + y1 * width;
				clusterCenters[1] = new ClusterCenter(labXys[i1]);
			}else if(clusters == 3)
			{
				int x0 = (int)MathF.Floor(width * 0.333f);
				int y0 = (int)MathF.Floor(height * 0.333f);
				int i0 = x0 + y0 * width;
				clusterCenters[0] = new ClusterCenter(labXys[i0]);

				int x1 = (int)MathF.Floor(width * 0.666f);
				int y1 = (int)MathF.Floor(height * 0.333f);
				int i1 = x1 + y1 * width;
				clusterCenters[1] = new ClusterCenter(labXys[i1]);

				int x2 = (int)MathF.Floor(width * 0.5f);
				int y2 = (int)MathF.Floor(height * 0.666f);
				int i2 = x2 + y2 * width;
				clusterCenters[2] = new ClusterCenter(labXys[i2]);
			}
			else {
				int cIdx = 0;
				//Choose initial centers
				for (float x = S / 2; x < width; x += S)
				{
					for (float y = S / 2; y < height; y += S)
					{
						if (cIdx >= clusterCenters.Length)
						{
							break;
						}

						int i = (int)x + (int)y * width;
						clusterCenters[cIdx] = new ClusterCenter(labXys[i]);
						cIdx++;
					}
				}
			}
			return clusterCenters;
		}

		private static LabXy[] ConvertToLabXy(ReadOnlySpan<Rgba32> pixels, int width, int height)
		{
			LabXy[] labXys = new LabXy[pixels.Length];
			//Convert pixels to LabXy
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					int i = x + y * width;
					var lab = new ColorLab(pixels[i]);
					labXys[i] = new LabXy()
					{
						l = lab.l,
						a = lab.a,
						b = lab.b,
						x = x,
						y = y
					};
				}
			}

			return labXys;
		}

		private static int[] EnforceConnectivity(int[] oldLabels, int width, int height, int clusters)
		{
			ReadOnlySpan<int> neighborX = new[] { -1, 0, 1, 0 };
			ReadOnlySpan<int> neighborY = new[] { 0, -1, 0, 1 };
			
			int sSquared = (width * height) / clusters;

			List<int> clusterX = new List<int>(sSquared);
			List<int> clusterY = new List<int>(sSquared);

			int adjacentLabel = 0;
			int[] newLabels = new int[oldLabels.Length];
			bool[] usedLabels = new bool[clusters];
			Array.Fill(newLabels, -1);

			for (int y = 0; y < height; ++y)
			{
				for (int x = 0; x < width; ++x)
				{
					int xyIndex = x + y * width;
					if (newLabels[xyIndex] < 0) {
						int label = oldLabels[xyIndex];
						newLabels[xyIndex] = label;

						//New cluster
						clusterX.Add(x);
						clusterY.Add(y);

						//Search neighbors for already completed clusters
						for (int i = 0; i < neighborX.Length; ++i)
						{
							int nX = x + neighborX[i];
							int nY = y + neighborY[i];
							int nI = nX + nY * width;
							if (nX < width && nX >= 0 && nY < height && nY >= 0)
							{
								if (newLabels[nI] >= 0)
								{
									adjacentLabel = newLabels[nI];
									break;
								}
							}
						}

						//Count pixels in this cluster
						for (int c = 0; c < clusterX.Count; ++c)
						{
							for (int i = 0; i < neighborX.Length; ++i)
							{
								int nX = clusterX[c] + neighborX[i];
								int nY = clusterY[c] + neighborY[i];
								int nI = nX + nY * width;
								if (nX < width && nX >= 0 && nY < height && nY >= 0)
								{
									if (newLabels[nI] == -1 && label == oldLabels[nI])
									{
										clusterX.Add(nX);
										clusterY.Add(nY);
										newLabels[nI] = label;
									}
								}
							}
						}

						// If this is unusually small cluster or this label is already used,
						// merge with adjacent cluster
						if (clusterX.Count < (sSquared / 4) || usedLabels[label])
						{
							for (int i = 0; i < clusterX.Count; ++i)
							{
								newLabels[(clusterY[i] * width + clusterX[i])] = adjacentLabel;
							}
						}
						else {
							usedLabels[label] = true;
						}

						clusterX.Clear();
						clusterY.Clear();
					}
				}
			}

			return newLabels;
		}
	}
}
