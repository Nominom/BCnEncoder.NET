using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal static class ColorChooser
	{
		/// <summary>
		/// Weighted per-channel distance of <paramref name="color"/> to all four palette entries at
		/// once. One lane per entry, so the four candidates cost a single set of SIMD operations.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Vector4 DistancesTo4(ReadOnlySpan<ColorRgb24> colors, ColorRgba32 color, float rWeight, float gWeight, float bWeight)
		{
			var dr = Vector4.Abs(new Vector4(colors[0].r, colors[1].r, colors[2].r, colors[3].r) - new Vector4(color.r));
			var dg = Vector4.Abs(new Vector4(colors[0].g, colors[1].g, colors[2].g, colors[3].g) - new Vector4(color.g));
			var db = Vector4.Abs(new Vector4(colors[0].b, colors[1].b, colors[2].b, colors[3].b) - new Vector4(color.b));

			return dr * rWeight + dg * gWeight + db * bWeight;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Select(Vector4 v, int index)
		{
			switch (index)
			{
				case 0: return v.X;
				case 1: return v.Y;
				case 2: return v.Z;
				default: return v.W;
			}
		}

		public static int ChooseClosestColor4(ReadOnlySpan<ColorRgb24> colors, ColorRgba32 color, float rWeight, float gWeight, float bWeight, out float error)
		{
			var d = DistancesTo4(colors, color, rWeight, gWeight, bWeight);

			var b0 = d.X > d.W ? 1 : 0;
			var b1 = d.Y > d.Z ? 1 : 0;
			var b2 = d.X > d.Z ? 1 : 0;
			var b3 = d.Y > d.W ? 1 : 0;
			var b4 = d.Z > d.W ? 1 : 0;

			var x0 = b1 & b2;
			var x1 = b0 & b3;
			var x2 = b0 & b4;

			var idx = x2 | ((x0 | x1) << 1);
			error = Select(d, idx);
			return idx;
		}


		public static int ChooseClosestColor4AlphaCutoff(ReadOnlySpan<ColorRgb24> colors, ColorRgba32 color, float rWeight, float gWeight, float bWeight, int alphaCutoff, bool hasAlpha, out float error)
		{

			if (hasAlpha && color.a < alphaCutoff)
			{
				error = 0;
				return 3;
			}

			var d = DistancesTo4(colors, color, rWeight, gWeight, bWeight);
			if (hasAlpha)
			{
				d.W = 999;
			}

			var b0 = d.X > d.Z ? 1 : 0;
			var b1 = d.Y > d.W ? 1 : 0;
			var b2 = d.X > d.W ? 1 : 0;
			var b3 = d.Y > d.Z ? 1 : 0;
			var nb3 = d.Y > d.Z ? 0 : 1;
			var b4 = d.X > d.Y ? 1 : 0;
			var b5 = d.Z > d.W ? 1 : 0;

			var idx = (nb3 & b4) | (b2 & b5) | (((b0 & b3) | (b1 & b2)) << 1);

			error = Select(d, idx);
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
			=> ChooseClosestColor(colors, new ColorYCbCr(color), luminanceMultiplier);
	}
}
