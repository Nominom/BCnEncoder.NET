using System;
using System.Numerics;
using BCnEncoder.Encoder;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Shared;

internal class PcaVectors2
{
	/// <summary>
	/// Converts colors to Vector4 format with perceptual weights applied.
	/// </summary>
	/// <param name="colors">Input colors</param>
	/// <param name="vectors">Output vectors</param>
	/// <param name="weights">Weight for the rgb channels</param>
	private static void ConvertToVector4Weighted(ReadOnlySpan<Vector4> colors, Span<Vector4> vectors,
		RgbWeights weights)
	{
		for (var i = 0; i < colors.Length; i++)
		{
			vectors[i] = weights.TransformPca(colors[i]);
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

	internal static Matrix4x4 CalculateCovariance(Span<Vector4> values, out Vector4 mean)
	{
		mean = CalculateMean(values);
		for (var i = 0; i < values.Length; i++)
		{
			values[i] -= mean;
		}

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

			// Upper triangle elements
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
	/// <returns>The normalized principal axis vector</returns>
	internal static Vector4 CalculatePrincipalAxis(Matrix4x4 covarianceMatrix)
	{
		// Start with a combination of basis vectors for better robustness
		var lastDa = Vector4.Normalize(new Vector4(1, 1, 1, 1));

		for (var i = 0; i < 30; i++)
		{
			var dA = Vector4.Transform(lastDa, covarianceMatrix);

			// Check if the vector has zero length
			float lengthSquared = dA.LengthSquared();
			if (lengthSquared < 1e-10f)
			{
				// If we get a zero vector, we've hit a null space
				// Try a different direction
				if (i == 0)
				{
					lastDa = Vector4.UnitX;
					continue;
				}
				else
				{
					break;
				}
			}

			dA = Vector4.Normalize(dA);

			// Check for convergence - note that we also check for sign flips
			float dotProduct = Vector4.Dot(lastDa, dA);
			if (Math.Abs(dotProduct) > 0.9999f)
			{
				// If dot product is negative, flip the direction
				if (dotProduct < 0)
				{
					dA = -dA;
				}

				return dA;
			}

			lastDa = dA;
		}

		// If we didn't converge within iterations, return the last approximation
		return lastDa;
	}

	/// <summary>
	/// Creates PCA vectors with color weighting
	/// </summary>
	public static void Create(ReadOnlySpan<Vector4> colorsPerceptual, RgbWeights weights, out Vector4 mean, out Vector4 principalAxis,
		out Vector4 min, out Vector4 max)
	{
		Span<Vector4> vectors = stackalloc Vector4[colorsPerceptual.Length];
		Span<Vector4> colorsWeighted = stackalloc Vector4[colorsPerceptual.Length];

		ConvertToVector4Weighted(colorsPerceptual, vectors, weights);

		// Make a copy, because we will modify the vectors
		vectors.CopyTo(colorsWeighted);

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
		// Get extreme points in weighted perceptual space
		GetExtremePoints(colorsWeighted, mean, principalAxis, out var minV, out var maxV);

		// Convert min and max back to linear space
		minV = weights.TransformFromPerceptual(weights.InverseTransformPca(minV));
		maxV = weights.TransformFromPerceptual(weights.InverseTransformPca(maxV));

		// Recalculate principal axis in linear space
		principalAxis = maxV - minV;

		// Mean is in the middle
		mean = minV + principalAxis * 0.5f;

		if (principalAxis.LengthSquared() == 0)
			principalAxis = Vector4.UnitY;

		principalAxis = Vector4.Normalize(principalAxis);

		min = minV;
		max = maxV;
	}

	/// <summary>
	/// Creates PCA vectors with color weighting, ignoring black pixels
	/// </summary>
	private static int CreateIgnorePixels(Span<Vector4> colorsPerceptual, RgbWeights weights, Func<Vector4, bool> ignorePixel, out Vector4 mean, out Vector4 principalAxis,
		out Vector4 min, out Vector4 max)
	{
		int numIgnored = 0;

		for (int i = 0; i < colorsPerceptual.Length; i++)
		{
			if (ignorePixel(colorsPerceptual[i]))
			{
				numIgnored++;
			}
		}

		Span<Vector4> colorsWeighted = stackalloc Vector4[colorsPerceptual.Length - numIgnored];
		Span<Vector4> vectors = stackalloc Vector4[colorsPerceptual.Length - numIgnored];

		int vecIdx = 0;
		for (int i = 0; i < colorsPerceptual.Length; i++)
		{
			if (!ignorePixel(colorsPerceptual[i]))
			{
				var color = colorsPerceptual[i];
				vectors[vecIdx] = weights.TransformPca(color);
				colorsWeighted[vecIdx] = vectors[vecIdx];
				vecIdx++;
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

		// Get extreme points in weighted perceptual space
		GetExtremePoints(colorsWeighted, mean, principalAxis, out var minV, out var maxV);

		// Convert min and max back to linear space
		minV = weights.TransformFromPerceptual(weights.InverseTransformPca(minV));
		maxV = weights.TransformFromPerceptual(weights.InverseTransformPca(maxV));

		// Recalculate principal axis in linear space
		principalAxis = maxV - minV;

		// Mean is in the middle
		mean = minV + principalAxis * 0.5f;

		if (principalAxis.LengthSquared() == 0)
			principalAxis = Vector4.UnitY;

		principalAxis = Vector4.Normalize(principalAxis);

		min = minV;
		max = maxV;

		return numIgnored;
	}

	/// <summary>
	/// Creates PCA vectors with color weighting, ignoring transaprent pixels
	/// </summary>
	public static int CreateIgnoreTransparent(Span<Vector4> colorsWeighted, RgbWeights weights, float alphaCutoff, out Vector4 mean, out Vector4 principalAxis,
		out Vector4 min, out Vector4 max)
	{
		bool IsTransparentPixel(Vector4 c) => c.W < alphaCutoff;

		return CreateIgnorePixels(colorsWeighted, weights, IsTransparentPixel, out mean, out principalAxis, out min, out max);
	}

	/// <summary>
	/// Creates PCA vectors with color weighting, ignoring black pixels
	/// </summary>
	public static int CreateIgnoreBlack(Span<Vector4> colorsWeighted, RgbWeights weights, out Vector4 mean, out Vector4 principalAxis,
		out Vector4 min, out Vector4 max)
	{
		bool IsBlackPixel(Vector4 c) => (c.X < 0.01f && c.Y < 0.01f && c.Z < 0.01f);

		return CreateIgnorePixels(colorsWeighted, weights, IsBlackPixel, out mean, out principalAxis, out min, out max);
	}

	public static void GetExtremePoints(Span<Vector4> colors, Vector4 mean, Vector4 principalAxis,
		out Vector4 min,
		out Vector4 max)
	{
		float minD = 0;
		float maxD = 0;

		for (var i = 0; i < colors.Length; i++)
		{
			var colorVec = colors[i];

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
			var colorVec = colors[i];

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
