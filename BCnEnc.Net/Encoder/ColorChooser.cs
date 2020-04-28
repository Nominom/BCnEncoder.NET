using System;
using System.Collections.Generic;
using System.Text;
using BCnEnc.Net.Shared;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Encoder
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

			int b0 = d[0] > d[3] ? 1 : 0;
			int b1 = d[1] > d[2] ? 1 : 0;
			int b2 = d[0] > d[2] ? 1 : 0;
			int b3 = d[1] > d[3] ? 1 : 0;
			int b4 = d[2] > d[3] ? 1 : 0;

			int x0 = b1 & b2;
			int x1 = b0 & b3;
			int x2 = b0 & b4;

			int idx = (x2 | ((x0 | x1) << 1));
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

			int b0 = d[0] > d[2] ? 1 : 0;
			int b1 = d[1] > d[3] ? 1 : 0;
			int b2 = d[0] > d[3] ? 1 : 0;
			int b3 = d[1] > d[2] ? 1 : 0;
			int nb3 = d[1] > d[2] ? 0 : 1;
			int b4 = d[0] > d[1] ? 1 : 0;
			int b5 = d[2] > d[3] ? 1 : 0;

			int idx = (nb3 & b4) | (b2 & b5) | (((b0 & b3) | (b1 & b2)) << 1);

			error = d[idx];
			return idx;
		}

		public static int ChooseClosestColor(Span<ColorRgb24> colors, Rgba32 color)
		{
			int closest = 0;
			int closestError =
				Math.Abs(colors[0].r - color.R)
				+ Math.Abs(colors[0].g - color.G)
				+ Math.Abs(colors[0].b - color.B);

			for (int i = 1; i < colors.Length; i++)
			{
				int error =
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
			int closest = 0;
			int closestError =
				Math.Abs(colors[0].r - color.R)
				+ Math.Abs(colors[0].g - color.G)
				+ Math.Abs(colors[0].b - color.B)
				+ Math.Abs(colors[0].a - color.A);

			for (int i = 1; i < colors.Length; i++)
			{
				int error =
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

			int closest = 0;
			int closestError =
				Math.Abs(colors[0].r - color.R)
				+ Math.Abs(colors[0].g - color.G)
				+ Math.Abs(colors[0].b - color.B);

			for (int i = 1; i < colors.Length; i++)
			{
				if (i == 3) continue; // Skip transparent
				int error =
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
			int closest = 0;
			float closestError = 0;
			bool first = true;

			for (int i = 0; i < colors.Length; i++)
			{
				float error = MathF.Abs(colors[i].y - color.y) * luminanceMultiplier
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
