using System;
using System.Numerics;
using Accord.Statistics.Analysis;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Shared
{
	internal static class PcaVectors
	{

		public static void Create(Span<Rgba32> colors, out Vector3 mean, out Vector3 principalAxis)
		{
			double[][] data = new double[colors.Length][];
			for (int i = 0; i < colors.Length; i++)
			{
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
			principalAxis.Y = (float)eigenVectors[0][1];
			principalAxis.Z = (float)eigenVectors[0][2];
		}

		public static void CreateWithAlpha(Span<Rgba32> colors, out Vector4 mean, out Vector4 principalAxis)
		{
			double[][] data = new double[colors.Length][];
			for (int i = 0; i < colors.Length; i++)
			{
				data[i] = new double[4];
				data[i][0] = colors[i].R / 255.0;
				data[i][1] = colors[i].G / 255.0;
				data[i][2] = colors[i].B / 255.0;
				data[i][3] = colors[i].A / 255.0;
			}
			PrincipalComponentAnalysis pca = new PrincipalComponentAnalysis();
			pca.Learn(data);
			var eigenVectors = pca.ComponentVectors;
			var pcaMeans = pca.Means;
			mean.X = (float)pcaMeans[0];
			mean.Y = (float)pcaMeans[1];
			mean.Z = (float)pcaMeans[2];
			mean.W = (float)pcaMeans[3];

			principalAxis.X = (float)eigenVectors[0][0];
			principalAxis.Y = (float)eigenVectors[0][1];
			principalAxis.Z = (float)eigenVectors[0][2];
			principalAxis.W = (float)eigenVectors[0][3];
		}


		public static void GetExtremePoints(Span<Rgba32> colors, Vector3 mean, Vector3 principalAxis, out Vector3 min,
			out Vector3 max)
		{

			float minD = 0;
			float maxD = 0;

			for (int i = 0; i < colors.Length; i++)
			{
				var colorVec = new Vector3(colors[i].R / 255f, colors[i].G / 255f, colors[i].B / 255f);

				var v = colorVec - mean;
				var d = Vector3.Dot(v, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			min = mean + (principalAxis * minD);
			max = mean + (principalAxis * maxD);
		}

		public static void GetExtremePointsWithAlpha(Span<Rgba32> colors, Vector4 mean, Vector4 principalAxis, out Vector4 min,
			out Vector4 max)
		{

			float minD = 0;
			float maxD = 0;

			for (int i = 0; i < colors.Length; i++)
			{
				var colorVec = new Vector4(colors[i].R / 255f, colors[i].G / 255f, colors[i].B / 255f, colors[i].A / 255f);

				var v = colorVec - mean;
				var d = Vector4.Dot(v, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			min = mean + (principalAxis * minD);
			max = mean + (principalAxis * maxD);
		}

		public static void OptimizeEndpoints(Span<Rgba32> colors, Vector3 mean, Vector3 principalAxis, out ColorRgb565 min, out ColorRgb565 max)
		{
			int length = colors.Length;
			Vector3[] vectorColors = new Vector3[length];
			for (int i = 0; i < colors.Length; i++)
			{
				vectorColors[i] = new Vector3(colors[i].R / 255f, colors[i].G / 255f, colors[i].B / 255f);
			}

			float minD = 0;
			float maxD = 0;

			Vector3 Clamp565(Vector3 vec) {
				if (vec.X < 0) vec.X = 0;
				if (vec.X > 31) vec.X = 31;
				if (vec.Y < 0) vec.Y = 0;
				if (vec.Y > 63) vec.Y = 63;
				if (vec.Z < 0) vec.Z = 0;
				if (vec.Z > 31) vec.Z = 31;
				return new Vector3(MathF.Round(vec.X), MathF.Round(vec.Y), MathF.Round(vec.Z));
			}

			float Distance(Vector3 v, Vector3 p) {
				return MathF.Sqrt((v.X - p.X) * (v.X - p.X)
				                  + (v.Y - p.Y) * (v.Y - p.Y) * 4
				                  + (v.Z - p.Z) * (v.Z - p.Z));
				;
			}

			float selectClosestDistance(Vector3 selector, Vector3 f0, Vector3 f1, Vector3 f2, Vector3 f3)
			{
				float d0 = Distance(selector, f0);
				float d1 = Distance(selector, f1);
				float d2 = Distance(selector, f2);
				float d3 = Distance(selector, f3);

				if (d0 < d1 && d0 < d2 && d0 < d3) return d0;
				if (d1 < d0 && d1 < d2 && d1 < d3) return d1;
				if (d2 < d0 && d2 < d1 && d2 < d3) return d2;
				else return d3;
			}

			Vector3 endPoint0;
			Vector3 endPoint1;

			double calculateError()
			{
				double cumulativeError = 0;
				Vector3 ep0 = new Vector3(endPoint0.X / 31, endPoint0.Y / 63, endPoint0.Z / 31);
				Vector3 ep1 = new Vector3(endPoint1.X / 31, endPoint1.Y / 63, endPoint1.Z / 31);
				Vector3 ep2 = ep0 + ((ep1 - ep0) * 1 / 3f);
				Vector3 ep3 = ep0 + ((ep1 - ep0) * 2 / 3f);

				for (int i = 0; i < length; i++)
				{
					double distance = selectClosestDistance(vectorColors[i], ep0, ep1, ep2, ep3);
					cumulativeError += distance;
				}
				return cumulativeError;
			}


			for (int i = 0; i < vectorColors.Length; i++)
			{
				float d = ProjectPointOnLine(vectorColors[i], mean, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			endPoint0 = mean + (principalAxis * minD);
			endPoint1 = mean + (principalAxis * maxD);

			endPoint0 = new Vector3(MathF.Round(endPoint0.X * 31), MathF.Round(endPoint0.Y * 63), MathF.Round(endPoint0.Z * 31));
			endPoint1 = new Vector3(MathF.Round(endPoint1.X * 31), MathF.Round(endPoint1.Y * 63), MathF.Round(endPoint1.Z * 31));
			endPoint0 = Clamp565(endPoint0);
			endPoint1 = Clamp565(endPoint1);

			double best = calculateError();
			int increment = 5;
			bool foundBetter = true;
			int rounds = 0;
			// Variate color and look for better endpoints
			while (increment > 1 || foundBetter) {
				rounds++;
				foundBetter = false;
				{ // decrement ep0
					var prev = endPoint0;
					endPoint0 -= principalAxis * increment * 2;
					endPoint0 = Clamp565(endPoint0);
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0 = prev;
					}
				}

				{ // decrement ep1
					var prev = endPoint1;
					endPoint1 -= principalAxis * increment * 2;
					endPoint1 = Clamp565(endPoint1);
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1 = prev;
					}
				}

				{ // increment ep0
					var prev = endPoint0;
					endPoint0 += principalAxis * increment * 2;
					endPoint0 = Clamp565(endPoint0);
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0 = prev;
					}
				}

				{ // increment ep1
					var prev = endPoint1;
					endPoint1 += principalAxis * increment * 2;
					endPoint1 = Clamp565(endPoint1);
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1 = prev;
					}
				}

				{ // scaleUp 
					var prev0 = endPoint0;
					var prev1 = endPoint1;
					endPoint0 -= principalAxis * increment * 2;
					endPoint1 += principalAxis * increment * 2;
					endPoint0 = Clamp565(endPoint0);
					endPoint1 = Clamp565(endPoint1);
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0 = prev0;
						endPoint1 = prev1;
					}
				}

				{ // scaleDown
					var prev0 = endPoint0;
					var prev1 = endPoint1;
					endPoint0 += principalAxis * increment * 2;
					endPoint1 -= principalAxis * increment * 2;
					endPoint0 = Clamp565(endPoint0);
					endPoint1 = Clamp565(endPoint1);
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0 = prev0;
						endPoint1 = prev1;
					}
				}

				#region G
				if(endPoint0.Y - increment >= 0){ // decrement ep0 G
					float prevY = endPoint0.Y;
					endPoint0.Y -= increment;
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0.Y = prevY;
					}
				}

				if(endPoint1.Y - increment >= 0){ // decrement ep1 G
					float prevY = endPoint1.Y;
					endPoint1.Y -= increment;
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1.Y = prevY;
					}
				}

				if (foundBetter && increment > 1) {
					increment--;
				}

				if(endPoint1.Y + increment <= 63){ // increment ep1 G
					float prevY = endPoint1.Y;
					endPoint1.Y += increment;
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1.Y = prevY;
					}
				}

				if(endPoint0.Y + increment <= 63){ // increment ep0 G
					float prevY = endPoint0.Y;
					endPoint0.Y += increment;
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0.Y = prevY;
					}
				}

				#endregion

				#region R
				if(endPoint0.X - increment >= 0){ // decrement ep0 R
					float prevX = endPoint0.X;
					endPoint0.X -= increment;
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0.X = prevX;
					}
				}

				if(endPoint1.X - increment >= 0){ // decrement ep1 R
					float prevX = endPoint1.X;
					endPoint1.X -= increment;
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1.X = prevX;
					}
				}

				if (foundBetter && increment > 1) {
					increment--;
				}

				if(endPoint1.X + increment <= 31){ // increment ep1 R
					float prevX = endPoint1.X;
					endPoint1.X += increment;
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1.X = prevX;
					}
				}

				if(endPoint0.X + increment <= 31){ // increment ep0 R
					float prevX = endPoint0.X;
					endPoint0.X += increment;
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0.X = prevX;
					}
				}
				#endregion

				#region B

				if(endPoint0.Z - increment >= 0){ // decrement ep0 B
					float prevZ = endPoint0.Z;
					endPoint0.Z -= increment;
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0.Z = prevZ;
					}
				}
				
				if(endPoint1.Z - increment >= 0){ // decrement ep1 B
					float prevZ = endPoint1.Z;
					endPoint1.Z -= increment;
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1.Z = prevZ;
					}
				}

				if (foundBetter && increment > 1) {
					increment--;
				}

				if(endPoint1.Z + increment <= 31){ // increment ep1 B
					float prevZ = endPoint1.Z;
					endPoint1.Z += increment;
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint1.Z = prevZ;
					}
				}

				if(endPoint0.Z + increment <= 31){ // increment ep0 B
					float prevZ = endPoint0.Z;
					endPoint0.Z += increment;
					double error = calculateError();
					if (error < best)
					{
						foundBetter = true;
						best = error;
					}
					else
					{
						endPoint0.Z = prevZ;
					}
				}

				#endregion

				endPoint0 = Clamp565(endPoint0);
				endPoint1 = Clamp565(endPoint1);

				if (!foundBetter && increment > 1) {
					increment--;
				}
			}

			min = new ColorRgb565();
			min.RawR = (int)endPoint0.X;
			min.RawG = (int)endPoint0.Y;
			min.RawB = (int)endPoint0.Z;

			max = new ColorRgb565();
			max.RawR = (int)endPoint1.X;
			max.RawG = (int)endPoint1.Y;
			max.RawB = (int)endPoint1.Z;
		}

		private static float ProjectPointOnLine(Vector3 point, Vector3 linePoint, Vector3 lineDir)
		{
			var v = point - linePoint;
			var d = Vector3.Dot(v, lineDir);
			return d;
		}
	}
}
