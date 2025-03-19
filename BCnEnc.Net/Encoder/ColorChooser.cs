using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal static class ColorChooser
	{

		public static int ChooseClosestRgbColor4(ReadOnlySpan<ColorRgbaFloat> colors, ColorRgbaFloat color, float rWeight, float gWeight, float bWeight, out float error)
		{
			ReadOnlySpan<float> d = stackalloc float[4] {
				MathF.Abs(colors[0].r - color.r) * rWeight
				+ MathF.Abs(colors[0].g - color.g) * gWeight
				+ MathF.Abs(colors[0].b - color.b) * bWeight,
				MathF.Abs(colors[1].r - color.r) * rWeight
				+ MathF.Abs(colors[1].g - color.g) * gWeight
				+ MathF.Abs(colors[1].b - color.b) * bWeight,
				MathF.Abs(colors[2].r - color.r) * rWeight
				+ MathF.Abs(colors[2].g - color.g) * gWeight
				+ MathF.Abs(colors[2].b - color.b) * bWeight,
				MathF.Abs(colors[3].r - color.r) * rWeight
				+ MathF.Abs(colors[3].g - color.g) * gWeight
				+ MathF.Abs(colors[3].b - color.b) * bWeight,
			};

			var b0 = d[0] > d[3] ? 1 : 0;
			var b1 = d[1] > d[2] ? 1 : 0;
			var b2 = d[0] > d[2] ? 1 : 0;
			var b3 = d[1] > d[3] ? 1 : 0;
			var b4 = d[2] > d[3] ? 1 : 0;

			var x0 = b1 & b2;
			var x1 = b0 & b3;
			var x2 = b0 & b4;

			var idx = x2 | ((x0 | x1) << 1);
			error = d[idx];
			return idx;
		}


		public static int ChooseClosestRgbColor4AlphaCutoff(ReadOnlySpan<ColorRgbaFloat> colors, ColorRgbaFloat color, float rWeight, float gWeight, float bWeight, float alphaCutoff, bool hasAlpha, out float error)
		{

			if (hasAlpha && color.a < alphaCutoff)
			{
				error = 0;
				return 3;
			}

			ReadOnlySpan<float> d = stackalloc float[4] {
				MathF.Abs(colors[0].r - color.r) * rWeight
				+ MathF.Abs(colors[0].g - color.g) * gWeight
				+ MathF.Abs(colors[0].b - color.b) * bWeight,
				MathF.Abs(colors[1].r - color.r) * rWeight
				+ MathF.Abs(colors[1].g - color.g) * gWeight
				+ MathF.Abs(colors[1].b - color.b) * bWeight,
				MathF.Abs(colors[2].r - color.r) * rWeight
				+ MathF.Abs(colors[2].g - color.g) * gWeight
				+ MathF.Abs(colors[2].b - color.b) * bWeight,

				hasAlpha ? 999 :
				MathF.Abs(colors[3].r - color.r) * rWeight
				+ MathF.Abs(colors[3].g - color.g) * gWeight
				+ MathF.Abs(colors[3].b - color.b) * bWeight,
			};

			var b0 = d[0] > d[2] ? 1 : 0;
			var b1 = d[1] > d[3] ? 1 : 0;
			var b2 = d[0] > d[3] ? 1 : 0;
			var b3 = d[1] > d[2] ? 1 : 0;
			var nb3 = d[1] > d[2] ? 0 : 1;
			var b4 = d[0] > d[1] ? 1 : 0;
			var b5 = d[2] > d[3] ? 1 : 0;

			var idx = (nb3 & b4) | (b2 & b5) | (((b0 & b3) | (b1 & b2)) << 1);

			error = d[idx];
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
