using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal static class ColorChooser
	{

		public static int ChooseClosestColor4(ReadOnlySpan<ColorRgb24> colors, ColorRgba32 color, float rWeight, float gWeight, float bWeight, out float error)
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


		public static int ChooseClosestColor4AlphaCutoff(ReadOnlySpan<ColorRgb24> colors, ColorRgba32 color, float rWeight, float gWeight, float bWeight, int alphaCutoff, bool hasAlpha, out float error)
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

		public static int ChooseClosestColor(Span<ColorRgb24> colors, ColorRgba32 color)
		{
			var closest = 0;
			var closestError =
				Math.Abs(colors[0].r - color.r)
				+ Math.Abs(colors[0].g - color.g)
				+ Math.Abs(colors[0].b - color.b);

			for (var i = 1; i < colors.Length; i++)
			{
				var error =
					Math.Abs(colors[i].r - color.r)
					+ Math.Abs(colors[i].g - color.g)
					+ Math.Abs(colors[i].b - color.b);
				if (error < closestError)
				{
					closest = i;
					closestError = error;
				}
			}
			return closest;
		}

		public static int ChooseClosestColor(Span<ColorRgba32> colors, ColorRgba32 color)
		{
			var closest = 0;
			var closestError =
				Math.Abs(colors[0].r - color.r)
				+ Math.Abs(colors[0].g - color.g)
				+ Math.Abs(colors[0].b - color.b)
				+ Math.Abs(colors[0].a - color.a);

			for (var i = 1; i < colors.Length; i++)
			{
				var error =
					Math.Abs(colors[i].r - color.r)
					+ Math.Abs(colors[i].g - color.g)
					+ Math.Abs(colors[i].b - color.b)
					+ Math.Abs(colors[i].a - color.a);
				if (error < closestError)
				{
					closest = i;
					closestError = error;
				}
			}
			return closest;
		}

		public static int ChooseClosestColorAlphaCutOff(Span<ColorRgba32> colors, ColorRgba32 color, byte alphaCutOff = 255 / 2)
		{
			if (color.a <= alphaCutOff)
			{
				return 3;
			}

			var closest = 0;
			var closestError =
				Math.Abs(colors[0].r - color.r)
				+ Math.Abs(colors[0].g - color.g)
				+ Math.Abs(colors[0].b - color.b);

			for (var i = 1; i < colors.Length; i++)
			{
				if (i == 3) continue; // Skip transparent
				var error =
					Math.Abs(colors[i].r - color.r)
					+ Math.Abs(colors[i].g - color.g)
					+ Math.Abs(colors[i].b - color.b);
				if (error < closestError)
				{
					closest = i;
					closestError = error;
				}
			}
			return closest;
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
