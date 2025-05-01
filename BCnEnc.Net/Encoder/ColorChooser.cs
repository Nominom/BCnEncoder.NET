using System;
using System.Drawing;
using System.Numerics;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Encoder
{
	internal static class ColorChooser
	{

		public static int ChooseClosestRgbColor4(ReadOnlySpan<Vector4> colors, Vector4 color, RgbWeights weights, out float err)
		{
			ReadOnlySpan<ColorRgbaFloat> colorsF = colors.Cast<Vector4, ColorRgbaFloat>();
			ColorRgbaFloat colorF = new ColorRgbaFloat(color);
			return ChooseClosestRgbColor4(colorsF, colorF, weights, out err);
		}

		public static int ChooseClosestRgbColor4(ReadOnlySpan<ColorRgbaFloat> colors, ColorRgbaFloat color, RgbWeights weights, out float error)
		{
		// 	Vector3 weightV = new Vector3(2f, 1f, 1f);

			Span<Vector4> colorsV = stackalloc Vector4[4];
			Vector4 colorV = weights.TransformToPerceptual(color.ToVector4());

			colorsV[0] = weights.TransformToPerceptual(colors[0].ToVector4());
			colorsV[1] = weights.TransformToPerceptual(colors[1].ToVector4());
			colorsV[2] = weights.TransformToPerceptual(colors[2].ToVector4());
			colorsV[3] = weights.TransformToPerceptual(colors[3].ToVector4());

			int idx = 0;
			// error = Vector4.DistanceSquared(colorsV[0], colorV);
			error = weights.CalculateColorDiff(colorsV[0], colorV);
			for (int i = 1; i < 4; i++)
			{
				// float d = Vector4.DistanceSquared(colorsV[i], colorV);
				float d = weights.CalculateColorDiff(colorsV[i], colorV);
				if (d < error)
				{
					idx = i;
					error = d;
				}
			}
			return idx;
		}


		public static int ChooseClosestRgbColor4AlphaCutoff(ReadOnlySpan<ColorRgbaFloat> colors, ColorRgbaFloat color, RgbWeights weights, float alphaCutoff, bool hasAlpha, out float error)
		{
			if (hasAlpha && color.a < alphaCutoff)
			{
				error = 0;
				return 3;
			}

			ReadOnlySpan<float> d = stackalloc float[4] {
				MathF.Abs(colors[0].r - color.r) * weights.R
				+ MathF.Abs(colors[0].g - color.g) * weights.G
				+ MathF.Abs(colors[0].b - color.b) * weights.B,
				MathF.Abs(colors[1].r - color.r) * weights.R
				+ MathF.Abs(colors[1].g - color.g) * weights.G
				+ MathF.Abs(colors[1].b - color.b) * weights.B,
				MathF.Abs(colors[2].r - color.r) * weights.R
				+ MathF.Abs(colors[2].g - color.g) * weights.G
				+ MathF.Abs(colors[2].b - color.b) * weights.B,

				hasAlpha ? 999 :
				MathF.Abs(colors[3].r - color.r) * weights.R
				+ MathF.Abs(colors[3].g - color.g) * weights.G
				+ MathF.Abs(colors[3].b - color.b) * weights.B,
			};

			int idx = 0;
			error = d[idx];
			for (int i = 1; i < 4; i++)
			{
				if (d[i] < error)
				{
					idx = i;
					error = d[idx];
				}
			}

			return idx;
		}

		public static int ChooseClosestColor(Span<ColorYCbCr> colors, ColorYCbCr color, float luminanceMultiplier = 4)
		{
			var closest = 0;
			float closestError = 0;
			var first = true;

			for (var i = 0; i < colors.Length; i++)
			{
				var error = MathF.Abs(colors[i].y - color.y) * luminanceMultiplier
                            + MathF.Abs(colors[i].cb - color.cb)
                            + MathF.Abs(colors[i].cr - color.cr);
				if (first)
				{
					closestError = error;
					first = false;
				}
				else if (error < closestError)
				{
					closest = i;
					closestError = error;
				}
			}
			return closest;
		}

		public static int ChooseClosestColor(Span<ColorYCbCr> colors, ColorRgba32 color, float luminanceMultiplier = 4)
			=> ChooseClosestColor(colors, color.As<ColorYCbCr>(), luminanceMultiplier);
	}
}
