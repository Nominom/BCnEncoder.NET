using System;
using System.Numerics;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Shared
{
	internal static class PcaVectors
	{
		private const int C565_5Mask = 0xF8;
		private const int C565_6Mask = 0xFC;

		private static void ConvertToVector4(ReadOnlySpan<ColorRgba32> colors, Span<Vector4> vectors)
		{
			for (var i = 0; i < colors.Length; i++)
			{
				vectors[i].X += colors[i].r / 255f;
				vectors[i].Y += colors[i].g / 255f;
				vectors[i].Z += colors[i].b / 255f;
				vectors[i].W += colors[i].a / 255f;
			}
		}

		private static void ConvertToVector4(ReadOnlySpan<ColorRgbFloat> colors, Span<Vector4> vectors)
		{
			for (var i = 0; i < colors.Length; i++)
			{
				vectors[i].X += colors[i].r;
				vectors[i].Y += colors[i].g;
				vectors[i].Z += colors[i].b;
				vectors[i].W = 0;
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

		internal static Matrix4x4 CalculateCovariance(Span<Vector4> values, out Vector4 mean) {
			mean = CalculateMean(values);
			for (var i = 0; i < values.Length; i++)
			{
				values[i] -= mean;
			}

			//4x4 matrix
			var mat = new Matrix4x4();

			for (var i = 0; i < values.Length; i++)
			{
				mat.M11 += values[i].X * values[i].X;
				mat.M12 += values[i].X * values[i].Y;
				mat.M13 += values[i].X * values[i].Z;
				mat.M14 += values[i].X * values[i].W;

				mat.M22 += values[i].Y * values[i].Y;
				mat.M23 += values[i].Y * values[i].Z;
				mat.M24 += values[i].Y * values[i].W;

				mat.M33 += values[i].Z * values[i].Z;
				mat.M34 += values[i].Z * values[i].W;

				mat.M44 += values[i].W * values[i].W;
			}

			mat = Matrix4x4.Multiply(mat, 1f / (values.Length - 1));

			mat.M21 = mat.M12;
			mat.M31 = mat.M13;
			mat.M32 = mat.M23;
			mat.M41 = mat.M14;
			mat.M42 = mat.M24;
			mat.M43 = mat.M34;

			return mat;
		}

		/// <summary>
		/// Calculate principal axis with the power-method
		/// </summary>
		/// <param name="covarianceMatrix"></param>
		/// <returns></returns>
		internal static Vector4 CalculatePrincipalAxis(Matrix4x4 covarianceMatrix) {
			var lastDa = Vector4.UnitY;

			for (var i = 0; i < 30; i++) {
				var dA = Vector4.Transform(lastDa, covarianceMatrix);

				if(dA.LengthSquared() == 0) {
					break;
				}

				dA = Vector4.Normalize(dA);
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

			for (var i = 0; i < colors.Length; i++)
			{
				var colorVec = new Vector3(colors[i].r / 255f, colors[i].g / 255f, colors[i].b / 255f);

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

			for (var i = 0; i < colors.Length; i++)
			{
				var colorVec = new Vector3(colors[i].r / 255f, colors[i].g / 255f, colors[i].b / 255f);

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

			for (var i = 0; i < colors.Length; i++)
			{
				var colorVec = new Vector4(colors[i].r / 255f, colors[i].g / 255f, colors[i].b / 255f, colors[i].a / 255f);

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
