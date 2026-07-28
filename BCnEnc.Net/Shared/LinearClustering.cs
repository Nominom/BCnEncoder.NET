using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if NETSTANDARD2_0
using Array = BCnEncoder.Shared.ArrayPolyfills;
#endif

namespace BCnEncoder.Shared
{
	/// <summary>
	/// Simple Linear Iterative Clustering.
	/// </summary>
	internal static class LinearClustering
	{

		private struct LabXy
		{
			public float l;
			public float a;
			public float b;
			public float x;
			public float y;


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ClusterCenter(LabXy labxy)
			{
				this.l = labxy.l;
				this.a = labxy.a;
				this.b = labxy.b;
				this.x = labxy.x;
				this.y = labxy.y;
				count = 0;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public readonly float Distance(LabXy other, float mOverS)
			{
				var dl = l - other.l;
				var da = a - other.a;
				var db = b - other.b;
				var dx = x - other.x;
				var dy = y - other.y;

				var dLab = MathF.Sqrt(dl * dl + da * da + db * db);
				var dXy = MathF.Sqrt(dx * dx + dy * dy);
				return dLab + mOverS * dXy;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public readonly float Distance(ClusterCenter other, float mOverS)
			{
				var dl = l - other.l;
				var da = a - other.a;
				var db = b - other.b;
				var dx = x - other.x;
				var dy = y - other.y;

				var dLab = MathF.Sqrt(dl * dl + da * da + db * db);
				var dXy = MathF.Sqrt(dx * dx + dy * dy);
				return dLab + mOverS * dXy;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		public static int[] ClusterPixels(ReadOnlySpan<ColorRgba32> pixels, int width, int height,
			int clusters, float m = 10, int maxIterations = 10, bool enforceConnectivity = true)
		{
			if (clusters < 2)
			{
				throw new ArgumentException("Number of clusters should be more than 1");
			}

			return Cluster(ConvertToLabXy(pixels, width, height), width, height, clusters, m, maxIterations, enforceConnectivity);
		}

		/// <summary>
		/// The greater the value of M,
		/// the more spatial proximity is emphasized and the more compact the cluster,
		/// M should be in range of 1 to 20.
		/// </summary>
		public static int[] ClusterPixels(ReadOnlySpan<ColorRgbFloat> pixels, int width, int height,
			int clusters, float m = 10, int maxIterations = 10, bool enforceConnectivity = true)
		{

			if (clusters < 2)
			{
				throw new ArgumentException("Number of clusters should be more than 1");
			}

			return Cluster(ConvertToLabXy(pixels, width, height), width, height, clusters, m, maxIterations, enforceConnectivity);
		}

		private static int[] Cluster(LabXy[] labXys, int width, int height,
			int clusters, float m, int maxIterations, bool enforceConnectivity)
		{
			//Grid interval S
			var s = MathF.Sqrt(labXys.Length / (float)clusters);
			var mOverS = m / s;
			var clusterIndices = new int[labXys.Length];

			Span<ClusterCenter> clusterCenters = InitialClusterCenters(width, height, clusters, s, labXys);
			Span<ClusterCenter> previousCenters = new ClusterCenter[clusters];

			float error = 999;
			const float threshold = 0.1f;
			var iter = 0;
			while (error > threshold)
			{
				if (maxIterations > 0 && iter >= maxIterations)
				{
					break;
				}
				iter++;

				clusterCenters.CopyTo(previousCenters);

				Array.Fill(clusterIndices, -1);

				// Find closest cluster for pixels
				for (var j = 0; j < clusters; j++)
				{
					var center = clusterCenters[j];
					var xL = Math.Max(0, (int)(center.x - s));
					var xH = Math.Min(width, (int)(center.x + s));
					var yL = Math.Max(0, (int)(center.y - s));
					var yH = Math.Min(height, (int)(center.y + s));

					for (var x = xL; x < xH; x++)
					{
						for (var y = yL; y < yH; y++)
						{
							var i = x + y * width;

							if (clusterIndices[i] == -1)
							{
								clusterIndices[i] = j;
							}
							else
							{
								var labXy = labXys[i];
								var prevDistance = clusterCenters[clusterIndices[i]].Distance(labXy, mOverS);
								var distance = center.Distance(labXy, mOverS);
								if (distance < prevDistance)
								{
									clusterIndices[i] = j;
								}
							}
						}
					}
				}

				error = RecalculateCenters(clusters, labXys, clusterIndices, previousCenters, mOverS, ref clusterCenters);
			}

			if (enforceConnectivity)
			{
				clusterIndices = EnforceConnectivity(clusterIndices, width, height, clusters);
			}

			return clusterIndices;
		}

		private static float RecalculateCenters(int clusters, LabXy[] labXys, int[] clusterIndices,
			Span<ClusterCenter> previousCenters, float mOverS, ref Span<ClusterCenter> clusterCenters)
		{
			clusterCenters.Clear();
			for (var i = 0; i < labXys.Length; i++)
			{
				var labXy = labXys[i];
				var clusterIndex = clusterIndices[i];
				// Sometimes a pixel is out of the range of any cluster,
				// in that case, find the nearest cluster and add it to it
				if (clusterIndex == -1)
				{
					var bestCluster = 0;
					var bestDistance = previousCenters[0].Distance(labXy, mOverS);
					for (var j = 1; j < clusters; j++) {
						var dist = previousCenters[j].Distance(labXy, mOverS);
						if (dist < bestDistance) {
							bestDistance = dist;
							bestCluster = j;
						}
					}
					clusterCenters[bestCluster] += labXy;
					clusterIndices[i] = bestCluster;
				}
				else {
					clusterCenters[clusterIndex] += labXy;
				}
			}

			float error = 0;
			for (var i = 0; i < clusters; i++)
			{
				if (clusterCenters[i].count > 0)
				{
					clusterCenters[i] /= clusterCenters[i].count;
					error += clusterCenters[i].Distance(previousCenters[i], mOverS);
				}
			}

			error /= clusters;
			return error;
		}

		private static ClusterCenter[] InitialClusterCenters(int width, int height, int clusters, float s, LabXy[] labXys)
		{
			var clusterCenters = new ClusterCenter[clusters];

			if (clusters == 2) {
				var x0 = (int)MathF.Floor(width * 0.333f);
				var y0 = (int)MathF.Floor(height * 0.333f);

				var x1 = (int)MathF.Floor(width * 0.666f);
				var y1 = (int)MathF.Floor(height * 0.666f);

				var i0 = x0 + y0 * width;
				clusterCenters[0] = new ClusterCenter(labXys[i0]);

				var i1 = x1 + y1 * width;
				clusterCenters[1] = new ClusterCenter(labXys[i1]);
			}else if(clusters == 3)
			{
				var x0 = (int)MathF.Floor(width * 0.333f);
				var y0 = (int)MathF.Floor(height * 0.333f);
				var i0 = x0 + y0 * width;
				clusterCenters[0] = new ClusterCenter(labXys[i0]);

				var x1 = (int)MathF.Floor(width * 0.666f);
				var y1 = (int)MathF.Floor(height * 0.333f);
				var i1 = x1 + y1 * width;
				clusterCenters[1] = new ClusterCenter(labXys[i1]);

				var x2 = (int)MathF.Floor(width * 0.5f);
				var y2 = (int)MathF.Floor(height * 0.666f);
				var i2 = x2 + y2 * width;
				clusterCenters[2] = new ClusterCenter(labXys[i2]);
			}
			else {
				var cIdx = 0;
				//Choose initial centers
				for (var x = s / 2; x < width; x += s)
				{
					for (var y = s / 2; y < height; y += s)
					{
						if (cIdx >= clusterCenters.Length)
						{
							break;
						}

						var i = (int)x + (int)y * width;
						clusterCenters[cIdx] = new ClusterCenter(labXys[i]);
						cIdx++;
					}
				}
			}
			return clusterCenters;
		}

		private static LabXy[] ConvertToLabXy(ReadOnlySpan<ColorRgba32> pixels, int width, int height)
		{
			var labXys = new LabXy[pixels.Length];
			//Convert pixels to LabXy
			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					var i = x + y * width;
					var lab = new ColorLab(pixels[i]);
					labXys[i] = new LabXy
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

		private static LabXy[] ConvertToLabXy(ReadOnlySpan<ColorRgbFloat> pixels, int width, int height)
		{
			var labXys = new LabXy[pixels.Length];
			//Convert pixels to LabXy
			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					var i = x + y * width;
					var lab = new ColorLab(pixels[i]);
					labXys[i] = new LabXy
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

			var sSquared = width * height / clusters;

			var clusterX = new List<int>(sSquared);
			var clusterY = new List<int>(sSquared);

			var adjacentLabel = 0;
			var newLabels = new int[oldLabels.Length];
			var usedLabels = new bool[clusters];
			Array.Fill(newLabels, -1);

			for (var y = 0; y < height; ++y)
			{
				for (var x = 0; x < width; ++x)
				{
					var xyIndex = x + y * width;
					if (newLabels[xyIndex] < 0) {
						var label = oldLabels[xyIndex];
						newLabels[xyIndex] = label;

						//New cluster
						clusterX.Add(x);
						clusterY.Add(y);

						//Search neighbors for already completed clusters
						for (var i = 0; i < neighborX.Length; ++i)
						{
							var nX = x + neighborX[i];
							var nY = y + neighborY[i];
							var nI = nX + nY * width;
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
						for (var c = 0; c < clusterX.Count; ++c)
						{
							for (var i = 0; i < neighborX.Length; ++i)
							{
								var nX = clusterX[c] + neighborX[i];
								var nY = clusterY[c] + neighborY[i];
								var nI = nX + nY * width;
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
						if (clusterX.Count < sSquared / 4 || usedLabels[label])
						{
							for (var i = 0; i < clusterX.Count; ++i)
							{
								newLabels[clusterY[i] * width + clusterX[i]] = adjacentLabel;
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
