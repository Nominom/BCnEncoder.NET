using System;
using System.Numerics;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Shared
{
	internal static class PcaVectors
	{
		private static void ConvertToVector4(ReadOnlySpan<ColorRgbaFloat> colors, Span<Vector4> vectors)
		{
			for (var i = 0; i < colors.Length; i++)
			{
				vectors[i].X = colors[i].r;
				vectors[i].Y = colors[i].g;
				vectors[i].Z = colors[i].b;
				vectors[i].W = colors[i].a;
			}
		}

		private static Vector4 CalculateMean(Span<Vector4> colors)
		{

			float r = 0;
			float g = 0;
			float b = 0;
			float a = 0;

			for (var i = 0; i < colors.Length; i++)
			{
				r += colors[i].X;
				g += colors[i].Y;
				b += colors[i].Z;
				a += colors[i].W;
			}

			return new Vector4(
				r / colors.Length,
				g / colors.Length,
				b / colors.Length,
				a / colors.Length
				);
		}

		// internal static Matrix4x4 CalculateCovariance(Span<Vector4> values, out Vector4 mean) {
		// 	mean = CalculateMean(values);
		// 	for (var i = 0; i < values.Length; i++)
		// 	{
		// 		values[i] -= mean;
		// 	}
		//
		// 	//4x4 matrix
		// 	var mat = new Matrix4x4();
		//
		// 	if (values.Length < 2)
		// 		return mat;
		//
		// 	for (var i = 0; i < values.Length; i++)
		// 	{
		// 		mat.M11 += values[i].X * values[i].X;
		// 		mat.M12 += values[i].X * values[i].Y;
		// 		mat.M13 += values[i].X * values[i].Z;
		// 		mat.M14 += values[i].X * values[i].W;
		//
		// 		mat.M22 += values[i].Y * values[i].Y;
		// 		mat.M23 += values[i].Y * values[i].Z;
		// 		mat.M24 += values[i].Y * values[i].W;
		//
		// 		mat.M33 += values[i].Z * values[i].Z;
		// 		mat.M34 += values[i].Z * values[i].W;
		//
		// 		mat.M44 += values[i].W * values[i].W;
		// 	}
		//
		// 	mat = Matrix4x4.Multiply(mat, 1f / (values.Length - 1));
		//
		// 	mat.M21 = mat.M12;
		// 	mat.M31 = mat.M13;
		// 	mat.M32 = mat.M23;
		// 	mat.M41 = mat.M14;
		// 	mat.M42 = mat.M24;
		// 	mat.M43 = mat.M34;
		//
		// 	return mat;
		// }

		internal static Matrix4x4 CalculateCovariance(Span<Vector4> values, out Vector4 mean) {
			mean = CalculateMean(values);
			for (var i = 0; i < values.Length; i++)
			{
				values[i] -= mean;
			}

			//4x4 matrix
			var mat = new Matrix4x4();

			if (values.Length < 2)
				return mat;

			for (var i = 0; i < values.Length; i++)
			{
				// Diagonal elements
				mat.M11 += values[i].X * values[i].X;
				mat.M22 += values[i].Y * values[i].Y;
				mat.M33 += values[i].Z * values[i].Z;
				mat.M44 += values[i].W * values[i].W;

				// Upper triangle elements only (more efficient)
				mat.M12 += values[i].X * values[i].Y;
				mat.M13 += values[i].X * values[i].Z;
				mat.M14 += values[i].X * values[i].W;
				mat.M23 += values[i].Y * values[i].Z;
				mat.M24 += values[i].Y * values[i].W;
				mat.M34 += values[i].Z * values[i].W;
			}

			mat = Matrix4x4.Multiply(mat, 1f / (values.Length - 1));

			// Fill in the lower triangle from the upper triangle
			mat.M21 = mat.M12;
			mat.M31 = mat.M13;
			mat.M41 = mat.M14;
			mat.M32 = mat.M23;
			mat.M42 = mat.M24;
			mat.M43 = mat.M34;

			return mat;
		}

		/// <summary>
		/// Calculate principal axis with the power-method
		/// </summary>
		/// <param name="covarianceMatrix"></param>
		/// <returns></returns>
		// internal static Vector4 CalculatePrincipalAxis(Matrix4x4 covarianceMatrix) {
		// 	var lastDa = Vector4.UnitY;
		//
		// 	for (var i = 0; i < 30; i++) {
		// 		var dA = Vector4.Transform(lastDa, covarianceMatrix);
		//
		// 		if(dA.LengthSquared() == 0) {
		// 			break;
		// 		}
		//
		// 		dA = Vector4.Normalize(dA);
		// 		if (Vector4.Dot(lastDa, dA) > 0.999999) {
		// 			lastDa = dA;
		// 			break;
		// 		}
		// 		else {
		// 			lastDa = dA;
		// 		}
		// 	}
		// 	return lastDa;
		// }

		/// <summary>
		/// Calculate principal axis with the power-method
		/// </summary>
		/// <param name="covarianceMatrix"></param>
		/// <returns>The normalized principal axis vector</returns>
		internal static Vector4 CalculatePrincipalAxis(Matrix4x4 covarianceMatrix) {
			// Start with a combination of basis vectors for better robustness
			var lastDa = Vector4.Normalize(new Vector4(1, 1, 1, 1));

			for (var i = 0; i < 30; i++) {
				var dA = Vector4.Transform(lastDa, covarianceMatrix);

				// Check if the vector has zero length
				float lengthSquared = dA.LengthSquared();
				if (lengthSquared < 1e-10f) {
					// If we get a zero vector, we've hit a null space
					// Try a different direction
					if (i == 0) {
						lastDa = Vector4.UnitX;
						continue;
					} else {
						break;
					}
				}

				dA = Vector4.Normalize(dA);

				// Check for convergence - note that we also check for sign flips
				float dotProduct = Vector4.Dot(lastDa, dA);
				if (Math.Abs(dotProduct) > 0.9999f) {
					// If dot product is negative, flip the direction
					if (dotProduct < 0) {
						dA = -dA;
					}
					return dA;
				}

				lastDa = dA;
			}

			// If we didn't converge within iterations, return the last approximation
			return lastDa;
		}

		public static void Create(Span<ColorRgbaFloat> colors, out Vector4 mean, out Vector4 principalAxis)
		{
			Span<Vector4> vectors = stackalloc Vector4[colors.Length];
			ConvertToVector4(colors, vectors);

			var cov = CalculateCovariance(vectors, out var v4Mean);
			mean = new Vector4(v4Mean.X, v4Mean.Y, v4Mean.Z, v4Mean.W);

			var pa = CalculatePrincipalAxis(cov);
			principalAxis = new Vector4(pa.X, pa.Y, pa.Z, pa.W);
			if (principalAxis.LengthSquared() == 0)
			{
				principalAxis = Vector4.UnitY;
			}
			else
			{
				principalAxis = Vector4.Normalize(principalAxis);
			}

		}

		public static int CreateIgnoreBlacks(Span<ColorRgbaFloat> colors, out Vector4 mean, out Vector4 principalAxis)
		{
			int numBlackPixels = 0;
			bool isBlackPixel(in ColorRgbaFloat c) => (c.r < 0.0001f && c.g < 0.0001f && c.b < 0.0001f) ||  c.a < 0.0001f;

			for (int i = 0; i < colors.Length; i++)
			{
				if (isBlackPixel(colors[i]))
				{
					numBlackPixels++;
				}
			}

			Span<Vector4> vectors = stackalloc Vector4[colors.Length - numBlackPixels];
			int vecIdx = 0;
			for (int i = 0; i < colors.Length; i++)
			{
				if (!isBlackPixel(colors[i]))
				{
					vectors[vecIdx++] = colors[i].ToVector4();
				}
			}

			var cov = CalculateCovariance(vectors, out var v4Mean);
			mean = new Vector4(v4Mean.X, v4Mean.Y, v4Mean.Z, v4Mean.W);

			var pa = CalculatePrincipalAxis(cov);
			principalAxis = new Vector4(pa.X, pa.Y, pa.Z, pa.W);
			if (principalAxis.LengthSquared() == 0)
			{
				principalAxis = Vector4.UnitY;
			}
			else
			{
				principalAxis = Vector4.Normalize(principalAxis);
			}

			return numBlackPixels;
		}

		public static void GetMinMaxColor565(Span<ColorRgbaFloat> colors, Vector4 mean, Vector4 principalAxis,
			out ColorB5G6R5Packed min, out ColorB5G6R5Packed max)
		{
			GetExtremePoints(colors, mean, principalAxis, out var minVec, out var maxVec);

			minVec = Vector4.Clamp(minVec, Vector4.Zero, Vector4.One);
			maxVec = Vector4.Clamp(maxVec, Vector4.Zero, Vector4.One);

			min = new ColorB5G6R5Packed(minVec.X, minVec.Y, minVec.Z);
			max = new ColorB5G6R5Packed(maxVec.X, maxVec.Y, maxVec.Z);
		}

		public static void GetExtremePoints(Span<ColorRgbaFloat> colors, Vector4 mean, Vector4 principalAxis, out Vector4 min,
			out Vector4 max)
		{

			float minD = 0;
			float maxD = 0;

			for (var i = 0; i < colors.Length; i++)
			{
				var colorVec = new Vector4(colors[i].r, colors[i].g, colors[i].b, colors[i].a);

				var v = colorVec - mean;
				var d = Vector4.Dot(v, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			Vector4 minPoint = mean + principalAxis * minD;
			Vector4 maxPoint = mean + principalAxis * maxD;

			int nearMinCount = 0;
			int nearMaxCount = 0;
			float thresholdDist = 0.05f;
			float percentageSkipInset = 0.6f;

			for (var i = 0; i < colors.Length; i++)
			{
				var colorVec = colors[i].ToVector4();

				float distToMin = Vector4.Distance(colorVec, minPoint);
				float distToMax = Vector4.Distance(colorVec, maxPoint);

				if (distToMin < thresholdDist)
					nearMinCount++;
				else if (distToMax < thresholdDist)
					nearMaxCount++;
			}

			// Skip inset if a significant percentage of colors are near the endpoints
			bool skipInset = (nearMinCount + nearMaxCount) > (colors.Length * percentageSkipInset);

			// Inset
			if (!skipInset)
			{
				minD *= 15 / 16f;
				maxD *= 15 / 16f;
			}

			min = mean + principalAxis * minD;
			max = mean + principalAxis * maxD;
		}

	}
}
