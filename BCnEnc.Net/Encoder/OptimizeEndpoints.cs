using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Encoder;

internal static class EndpointOptimizer
{
	/// <summary>
	/// Quantize the given color vector to the 565 color range.
	/// </summary>
	/// <param name="color">The color vector to quantize.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void Quantize(ref Vector4 color)
	{
		color.X = MathF.Round(color.X * 31) / 31f;
		color.Y = MathF.Round(color.Y * 63) / 63f;
		color.Z = MathF.Round(color.Z * 31) / 31f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void Saturate(ref Vector4 color)
	{
		color = Vector4.Clamp(color, Vector4.Zero, Vector4.One);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static int GetColorIndex(ReadOnlySpan<Vector4> colors, Vector4 color)
	{
		int bestIndex = 0;
		float bestDistance = Vector4.DistanceSquared(colors[0], color);
		for (int i = 1; i < colors.Length; i++)
		{
			float distance = Vector4.DistanceSquared(colors[i], color);
			if (distance < bestDistance)
			{
				bestDistance = distance;
				bestIndex = i;
			}
		}

		return bestIndex;
	}


	public static void Optimize565Endpoints(Span<ColorRgbaFloat> colors, Vector4 mean, Vector4 principalAxis,
		ref ColorB5G6R5Packed ep0, ref ColorB5G6R5Packed ep1, RgbWeights weights, bool useBc1Blacks)
	{
		ReadOnlySpan<float> paletteWeights = useBc1Blacks
			? stackalloc float[3]
			{
				0f,
				1f,
				.5f
			}
			: stackalloc float[4]
			{
				0f,
				1f,
				1f / 3f,
				2f / 3f
			};

		Vector4 ep0Vec = ep0.ToColorRgbaFloat().ToVector4();
		Vector4 ep1Vec = ep1.ToColorRgbaFloat().ToVector4();

		Span<Vector4> palette = stackalloc Vector4[4];

		for (int it = 0; it < 3; it++)
		{
			palette[0] = ep0Vec;
			palette[1] = ep1Vec;

			if (useBc1Blacks) // Bc1 palette[3] is black when c0 <= c1
			{
				palette[2] = ep0Vec + (ep1Vec - ep0Vec) * .5f;
				palette[3] = Vector4.Zero;
			}
			else
			{
				palette[2] = ep0Vec + (ep1Vec - ep0Vec) * (1f / 3f);
				palette[3] = ep0Vec + (ep1Vec - ep0Vec) * (2f / 3f);
			}

			float sum_w0w0 = 0f, // Sum of squares of weights from endpoint 0
				sum_w0w1 = 0f, // Sum of cross terms (w0 * w1)
				sum_w1w1 = 0f; // Sum of squares of weights from endpoint 1
			float sum_w0_proj = 0f, // Sum of (w0 * projected color)
				sum_w1_proj = 0f; // Sum of (w1 * projected color)

			for (int i = 0; i < 16; i++)
			{
				// Convert color to vector
				Vector4 colorVec = colors[i].ToVector4();
				// colorVec *= weightsVec;

				// int colorIndex = GetColorIndex(palette, colorVec);
				int colorIndex = ColorChooser.ChooseClosestRgbColor4(palette, colorVec, weights, out _);

				// Ignore black when optimizing
				if (useBc1Blacks && colorIndex == 3) continue;

				// Take vector from mean to color
				Vector4 delta = colorVec - mean;
				// Project onto principal axis
				float projected = Vector4.Dot(delta, principalAxis);

				// Assign weights
				float w0 = paletteWeights[colorIndex];
				float w1 = 1f - w0;

				// AT: Matrix2x2 = transpose(A)
				// ATA: Matrix2x2 = matmul(AT, A)
				// ATb: Vector = matmul(AT, b)
				sum_w0w0 += w0 * w0;
				sum_w0w1 += w0 * w1;
				sum_w1w1 += w1 * w1;

				sum_w0_proj += w0 * projected;
				sum_w1_proj += w1 * projected;
			}

			// Solve 2x2 linear system
			// a, b_scalar = solve_2x2_linear_system(ATA, ATb)

			float determinant = sum_w0w0 * sum_w1w1 - sum_w0w1 * sum_w0w1;
			float endpoint0_scalar = 0f, endpoint1_scalar = 0f;

			if (Math.Abs(determinant) < 1e-6f)
			{
				determinant = 1e-6f;
			}

			float invDet = 1f / determinant;
			endpoint0_scalar = (sum_w0_proj * sum_w1w1 - sum_w1_proj * sum_w0w1) * invDet;
			endpoint1_scalar = (-sum_w0_proj * sum_w0w1 + sum_w1_proj * sum_w0w0) * invDet;

			// Un-project a and b back into RGB space
			ep0Vec = mean + principalAxis * endpoint0_scalar;
			ep1Vec = mean + principalAxis * endpoint1_scalar;
		}

		// Convert to 565
		ep0 = new ColorB5G6R5Packed(ep0Vec.X, ep0Vec.Y, ep0Vec.Z);
		ep1 = new ColorB5G6R5Packed(ep1Vec.X, ep1Vec.Y, ep1Vec.Z);
	}
}
