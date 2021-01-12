using System;
using BCnEncoder.Shared;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.Encoder
{
	internal static class ColorChooser
	{

		public static int ChooseClosestColor4(ReadOnlySpan<ColorRgb24> colors, Rgba32 color, float rWeight, float gWeight, float bWeight, out float error)
		{
			ReadOnlySpan<float> d = stackalloc float[4] {
				MathF.Abs(colors[0].r - color.R) * rWeight
				+ MathF.Abs(colors[0].g - color.G) * gWeight
				+ MathF.Abs(colors[0].b - color.B) * bWeight,
				MathF.Abs(colors[1].r - color.R) * rWeight
				+ MathF.Abs(colors[1].g - color.G) * gWeight
				+ MathF.Abs(colors[1].b - color.B) * bWeight,
				MathF.Abs(colors[2].r - color.R) * rWeight
				+ MathF.Abs(colors[2].g - color.G) * gWeight
				+ MathF.Abs(colors[2].b - color.B) * bWeight,
				MathF.Abs(colors[3].r - color.R) * rWeight
				+ MathF.Abs(colors[3].g - color.G) * gWeight
				+ MathF.Abs(colors[3].b - color.B) * bWeight,
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


		public static int ChooseClosestColor4AlphaCutoff(ReadOnlySpan<ColorRgb24> colors, Rgba32 color, float rWeight, float gWeight, float bWeight, int alphaCutoff, bool hasAlpha, out float error)
		{

			if (hasAlpha && color.A < alphaCutoff)
			{
				error = 0;
				return 3;
			}

			ReadOnlySpan<float> d = stackalloc float[4] {
				MathF.Abs(colors[0].r - color.R) * rWeight
				+ MathF.Abs(colors[0].g - color.G) * gWeight
				+ MathF.Abs(colors[0].b - color.B) * bWeight,
				MathF.Abs(colors[1].r - color.R) * rWeight
				+ MathF.Abs(colors[1].g - color.G) * gWeight
				+ MathF.Abs(colors[1].b - color.B) * bWeight,
				MathF.Abs(colors[2].r - color.R) * rWeight
				+ MathF.Abs(colors[2].g - color.G) * gWeight
				+ MathF.Abs(colors[2].b - color.B) * bWeight,

				hasAlpha ? 999 :
				MathF.Abs(colors[3].r - color.R) * rWeight
				+ MathF.Abs(colors[3].g - color.G) * gWeight
				+ MathF.Abs(colors[3].b - color.B) * bWeight,
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

		public static int ChooseClosestColor(Span<ColorRgb24> colors, Rgba32 color)
		{
			var closest = 0;
			var closestError =
				Math.Abs(colors[0].r - color.R)
				+ Math.Abs(colors[0].g - color.G)
				+ Math.Abs(colors[0].b - color.B);

			for (var i = 1; i < colors.Length; i++)
			{
				var error =
					Math.Abs(colors[i].r - color.R)
					+ Math.Abs(colors[i].g - color.G)
					+ Math.Abs(colors[i].b - color.B);
				if (error < closestError)
				{
					closest = i;
					closestError = error;
				}
			}
			return closest;
		}

		public static int ChooseClosestColor(Span<ColorRgba32> colors, Rgba32 color)
		{
			var closest = 0;
			var closestError =
				Math.Abs(colors[0].r - color.R)
				+ Math.Abs(colors[0].g - color.G)
				+ Math.Abs(colors[0].b - color.B)
				+ Math.Abs(colors[0].a - color.A);

			for (var i = 1; i < colors.Length; i++)
			{
				var error =
					Math.Abs(colors[i].r - color.R)
					+ Math.Abs(colors[i].g - color.G)
					+ Math.Abs(colors[i].b - color.B)
					+ Math.Abs(colors[i].a - color.A);
				if (error < closestError)
				{
					closest = i;
					closestError = error;
				}
			}
			return closest;
		}

		public static int ChooseClosestColorAlphaCutOff(Span<ColorRgba32> colors, Rgba32 color, byte alphaCutOff = 255 / 2)
		{
			if (color.A <= alphaCutOff)
			{
				return 3;
			}

			var closest = 0;
			var closestError =
				Math.Abs(colors[0].r - color.R)
				+ Math.Abs(colors[0].g - color.G)
				+ Math.Abs(colors[0].b - color.B);

			for (var i = 1; i < colors.Length; i++)
			{
				if (i == 3) continue; // Skip transparent
				var error =
					Math.Abs(colors[i].r - color.R)
					+ Math.Abs(colors[i].g - color.G)
					+ Math.Abs(colors[i].b - color.B);
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

		public static int ChooseClosestColor(Span<ColorYCbCr> colors, Rgba32 color, float luminanceMultiplier = 4)
			=> ChooseClosestColor(colors, new ColorYCbCr(color), luminanceMultiplier);
	}
}
