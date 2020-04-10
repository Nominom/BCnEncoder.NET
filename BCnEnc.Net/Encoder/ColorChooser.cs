using System;
using System.Collections.Generic;
using System.Text;
using BCnEnc.Net.Shared;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Encoder
{
	internal static class ColorChooser
	{
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

		public static int ChooseClosestColorAlphaCutOff(Span<ColorRgba32> colors, Rgba32 color, byte alphaCutOff = 255 / 2)
		{
			if (color.A <= alphaCutOff) {
				return 3;
			}

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
	}
}
