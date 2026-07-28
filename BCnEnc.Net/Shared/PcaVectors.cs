using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BCnEncoder.Shared
{
	internal static class PcaVectors
	{
		private const int C565_5Mask = 0xF8;
		private const int C565_6Mask = 0xFC;

		private static void ConvertToVector4(ReadOnlySpan<ColorRgba32> colors, Span<Vector4> vectors)
		{
			var normalized = ColorTables.Normalized;
			for (var i = 0; i < colors.Length; i++)
			{
				var c = colors[i];
				vectors[i] = new Vector4(normalized[c.r], normalized[c.g], normalized[c.b], normalized[c.a]);
			}
		}

		private static void ConvertToVector4(ReadOnlySpan<ColorRgbFloat> colors, Span<Vector4> vectors)
		{
			for (var i = 0; i < colors.Length; i++)
			{
				var c = colors[i];
				vectors[i] = new Vector4(c.r, c.g, c.b, 0);
			}
		}

		private static Vector4 CalculateMean(ReadOnlySpan<Vector4> colors)
		{
			var sum = Vector4.Zero;
			for (var i = 0; i < colors.Length; i++)
			{
				sum += colors[i];
			}

			return sum / colors.Length;
		}

		internal static Matrix4x4 CalculateCovariance(Span<Vector4> values, out Vector4 mean) {
			mean = CalculateMean(values);
			for (var i = 0; i < values.Length; i++)
			{
				values[i] -= mean;
			}

			// Each row of the symmetric covariance matrix is one broadcasted component times the
			// whole vector, so the outer product accumulates four products per SIMD multiply.
			var row1 = Vector4.Zero;
			var row2 = Vector4.Zero;
			var row3 = Vector4.Zero;
			var row4 = Vector4.Zero;

			for (var i = 0; i < values.Length; i++)
			{
				var v = values[i];
				row1 += new Vector4(v.X) * v;
				row2 += new Vector4(v.Y) * v;
				row3 += new Vector4(v.Z) * v;
				row4 += new Vector4(v.W) * v;
			}

			var scale = 1f / (values.Length - 1);
			row1 *= scale;
			row2 *= scale;
			row3 *= scale;
			row4 *= scale;

			return new Matrix4x4(
				row1.X, row1.Y, row1.Z, row1.W,
				row2.X, row2.Y, row2.Z, row2.W,
				row3.X, row3.Y, row3.Z, row3.W,
				row4.X, row4.Y, row4.Z, row4.W);
		}

		/// <summary>
		/// Calculate principal axis with the power-method
		/// </summary>
		/// <param name="covarianceMatrix"></param>
		/// <returns></returns>
		internal static Vector4 CalculatePrincipalAxis(Matrix4x4 covarianceMatrix) {
			// Held as rows so the iteration below is four multiply-adds instead of a matrix copy.
			var row1 = new Vector4(covarianceMatrix.M11, covarianceMatrix.M12, covarianceMatrix.M13, covarianceMatrix.M14);
			var row2 = new Vector4(covarianceMatrix.M21, covarianceMatrix.M22, covarianceMatrix.M23, covarianceMatrix.M24);
			var row3 = new Vector4(covarianceMatrix.M31, covarianceMatrix.M32, covarianceMatrix.M33, covarianceMatrix.M34);
			var row4 = new Vector4(covarianceMatrix.M41, covarianceMatrix.M42, covarianceMatrix.M43, covarianceMatrix.M44);

			var lastDa = Vector4.UnitY;

			for (var i = 0; i < 30; i++) {
				var dA = new Vector4(lastDa.X) * row1
					+ new Vector4(lastDa.Y) * row2
					+ new Vector4(lastDa.Z) * row3
					+ new Vector4(lastDa.W) * row4;

				// Vector4.Normalize would recompute the squared length internally, so divide directly.
				var lengthSquared = dA.LengthSquared();
				if(lengthSquared == 0) {
					break;
				}

				dA /= MathF.Sqrt(lengthSquared);
				if (Vector4.Dot(lastDa, dA) > 0.999999) {
					lastDa = dA;
					break;
				}
				else {
					lastDa = dA;
				}
			}
			return lastDa;
		}

		public static void Create(Span<ColorRgba32> colors, out Vector3 mean, out Vector3 principalAxis)
		{
			Span<Vector4> vectors = stackalloc Vector4[colors.Length];
			ConvertToVector4(colors, vectors);
			

			var cov = CalculateCovariance(vectors, out var v4Mean);
			mean = new Vector3(v4Mean.X, v4Mean.Y, v4Mean.Z);

			var pa = CalculatePrincipalAxis(cov);
			principalAxis = new Vector3(pa.X, pa.Y, pa.Z);
			if (principalAxis.LengthSquared() == 0) {
				principalAxis = Vector3.UnitY;
			}
			else {
				principalAxis = Vector3.Normalize(principalAxis);
			}

		}

		public static void Create(Span<ColorRgbFloat> colors, out Vector3 mean, out Vector3 principalAxis)
		{
			Span<Vector4> vectors = stackalloc Vector4[colors.Length];
			ConvertToVector4(colors, vectors);


			var cov = CalculateCovariance(vectors, out var v4Mean);
			mean = new Vector3(v4Mean.X, v4Mean.Y, v4Mean.Z);

			var pa = CalculatePrincipalAxis(cov);
			principalAxis = new Vector3(pa.X, pa.Y, pa.Z);
			if (principalAxis.LengthSquared() == 0)
			{
				principalAxis = Vector3.UnitY;
			}
			else
			{
				principalAxis = Vector3.Normalize(principalAxis);
			}

		}

		public static void CreateWithAlpha(Span<ColorRgba32> colors, out Vector4 mean, out Vector4 principalAxis)
		{
			Span<Vector4> vectors = stackalloc Vector4[colors.Length];
			ConvertToVector4(colors, vectors);
			
			var cov = CalculateCovariance(vectors, out mean);
			principalAxis = CalculatePrincipalAxis(cov);
		}


		public static void GetExtremePoints(Span<ColorRgba32> colors, Vector3 mean, Vector3 principalAxis, out ColorRgb24 min,
			out ColorRgb24 max)
		{

			float minD = 0;
			float maxD = 0;
			var normalized = ColorTables.Normalized;

			for (var i = 0; i < colors.Length; i++)
			{
				var c = colors[i];
				var colorVec = new Vector3(normalized[c.r], normalized[c.g], normalized[c.b]);

				var v = colorVec - mean;
				var d = Vector3.Dot(v, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			var minVec = mean + principalAxis * minD;
			var maxVec = mean + principalAxis * maxD;

			var minR = (int) (minVec.X * 255);
			var minG = (int) (minVec.Y * 255);
			var minB = (int) (minVec.Z * 255);

			var maxR = (int) (maxVec.X * 255);
			var maxG = (int) (maxVec.Y * 255);
			var maxB = (int) (maxVec.Z * 255);

			minR = minR >= 0 ? minR : 0;
			minG = minG >= 0 ? minG : 0;
			minB = minB >= 0 ? minB : 0;

			maxR = maxR <= 255 ? maxR : 255;
			maxG = maxG <= 255 ? maxG : 255;
			maxB = maxB <= 255 ? maxB : 255;

			min = new ColorRgb24((byte)minR, (byte)minG, (byte)minB);
			max = new ColorRgb24((byte)maxR, (byte)maxG, (byte)maxB);
		}

		public static void GetMinMaxColor565(Span<ColorRgba32> colors, Vector3 mean, Vector3 principalAxis, 
			out ColorRgb565 min, out ColorRgb565 max)
		{

			float minD = 0;
			float maxD = 0;
			var normalized = ColorTables.Normalized;

			for (var i = 0; i < colors.Length; i++)
			{
				var c = colors[i];
				var colorVec = new Vector3(normalized[c.r], normalized[c.g], normalized[c.b]);

				var v = colorVec - mean;
				var d = Vector3.Dot(v, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			//Inset
			minD *= 15 / 16f;
			maxD *= 15 / 16f;

			var minVec = mean + principalAxis * minD;
			var maxVec = mean + principalAxis * maxD;

			var minR = (int) (minVec.X * 255);
			var minG = (int) (minVec.Y * 255);
			var minB = (int) (minVec.Z * 255);

			var maxR = (int) (maxVec.X * 255);
			var maxG = (int) (maxVec.Y * 255);
			var maxB = (int) (maxVec.Z * 255);

			minR = minR >= 0 ? minR : 0;
			minG = minG >= 0 ? minG : 0;
			minB = minB >= 0 ? minB : 0;

			maxR = maxR <= 255 ? maxR : 255;
			maxG = maxG <= 255 ? maxG : 255;
			maxB = maxB <= 255 ? maxB : 255;

			// Optimal round
			minR = (minR & C565_5Mask) | (minR >> 5);
			minG = (minG & C565_6Mask) | (minG >> 6);
			minB = (minB & C565_5Mask) | (minB >> 5);

			maxR = (maxR & C565_5Mask) | (maxR >> 5);
			maxG = (maxG & C565_6Mask) | (maxG >> 6);
			maxB = (maxB & C565_5Mask) | (maxB >> 5);

			min = new ColorRgb565((byte)minR, (byte)minG, (byte)minB);
			max = new ColorRgb565((byte)maxR, (byte)maxG, (byte)maxB);

		}

		public static void GetExtremePointsWithAlpha(Span<ColorRgba32> colors, Vector4 mean, Vector4 principalAxis, out Vector4 min,
			out Vector4 max)
		{

			float minD = 0;
			float maxD = 0;
			var normalized = ColorTables.Normalized;

			for (var i = 0; i < colors.Length; i++)
			{
				var c = colors[i];
				var colorVec = new Vector4(normalized[c.r], normalized[c.g], normalized[c.b], normalized[c.a]);

				var v = colorVec - mean;
				var d = Vector4.Dot(v, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			min = mean + principalAxis * minD;
			max = mean + principalAxis * maxD;
		}

		public static void GetExtremePoints(Span<ColorRgbFloat> colors, Vector3 mean, Vector3 principalAxis, out Vector3 min,
			out Vector3 max)
		{

			float minD = 0;
			float maxD = 0;

			for (var i = 0; i < colors.Length; i++)
			{
				var colorVec = new Vector3(colors[i].r, colors[i].g, colors[i].b);

				var v = colorVec - mean;
				var d = Vector3.Dot(v, principalAxis);
				if (d < minD) minD = d;
				if (d > maxD) maxD = d;
			}

			minD *= 15 / 16f;
			maxD *= 15 / 16f;

			min = mean + principalAxis * minD;
			max = mean + principalAxis * maxD;
		}

	}
}
