using System.Numerics;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Shared
{
	internal static class Interpolation
	{
		/// <summary>
		/// Interpolates two colors by half.
		/// </summary>
		/// <param name="c0">The first color endpoint.</param>
		/// <param name="c1">The second color endpoint.</param>
		/// <returns>The interpolated color.</returns>
		public static Vector4 InterpolateHalf(this Vector4 c0, Vector4 c1)
		{
			return (c0 + c1) * 0.5f;
		}

		/// <summary>
		/// Interpolates two colors by third.
		/// </summary>
		/// <param name="c0">The first color endpoint.</param>
		/// <param name="c1">The second color endpoint.</param>
		/// <param name="num">The dividend in the third.</param>
		/// <returns>The interpolated color.</returns>
		public static Vector4 InterpolateThird(this Vector4 c0, Vector4 c1, int num)
		{
			return ((3 - num) * c0 + num * c1) / 3;
		}

		/// <summary>
		/// Interpolates two colors by half.
		/// </summary>
		/// <param name="c0">The first color endpoint.</param>
		/// <param name="c1">The second color endpoint.</param>
		/// <returns>The interpolated color.</returns>
		public static ColorRgbaFloat InterpolateHalf(this ColorRgbaFloat c0, ColorRgbaFloat c1) =>
			InterpolateColor(c0, c1, 1, 2);

		/// <summary>
		/// Interpolates two colors by third.
		/// </summary>
		/// <param name="c0">The first color endpoint.</param>
		/// <param name="c1">The second color endpoint.</param>
		/// <param name="num">The dividend in the third.</param>
		/// <returns>The interpolated color.</returns>
		public static ColorRgbaFloat InterpolateThird(this ColorRgbaFloat c0, ColorRgbaFloat c1, int num) =>
			InterpolateColor(c0, c1, num, 3);

		/// <summary>
		/// Interpolates two colors by fourth with ATC interpolation.
		/// </summary>
		/// <param name="c0">The first color endpoint.</param>
		/// <param name="c1">The second color endpoint.</param>
		/// <param name="num">The dividend in the fourth.</param>
		/// <returns>The interpolated color.</returns>
		public static ColorRgbaFloat InterpolateFourthAtc(this ColorRgbaFloat c0, ColorRgbaFloat c1, int num) =>
			InterpolateColorAtc(c0, c1, num, 4);

		/// <summary>
		/// Interpolates two colors by fifth.
		/// </summary>
		/// <param name="a0">The first component.</param>
		/// <param name="a1">The second component.</param>
		/// <param name="num">The dividend in the fifth.</param>
		/// <returns>The interpolated component.</returns>
		public static byte InterpolateFifth(this byte a0, byte a1, int num) =>
			(byte)InterpolateInt(a0, a1, num, 5, 2);

		/// <summary>
		/// Interpolates two colors by seventh.
		/// </summary>
		/// <param name="a0">The first component.</param>
		/// <param name="a1">The second component.</param>
		/// <param name="num">The dividend in the seventh.</param>
		/// <returns>The interpolated component.</returns>
		public static byte InterpolateSeventh(this byte a0, byte a1, int num) =>
			(byte)InterpolateInt(a0, a1, num, 7, 3);

		public static float InterpolateFifth(this float a0, float a1, int num) =>
			((5 - num) * a0 + num * a1) / 5f;

		public static float InterpolateSeventh(this float a0, float a1, int num) =>
			((7 - num) * a0 + num * a1) / 7f;

		/// <summary>
		/// Interpolates two colors.
		/// </summary>
		/// <param name="c0">The first color.</param>
		/// <param name="c1">The second color.</param>
		/// <param name="num">The dividend on each color component.</param>
		/// <param name="den">The divisor on each color component.</param>
		/// <returns>The interpolated color.</returns>
		private static ColorRgbaFloat InterpolateColor(ColorRgbaFloat c0, ColorRgbaFloat c1, int num, int den) => new ColorRgbaFloat(
			Interpolate(c0.r, c1.r, num, den),
			Interpolate(c0.g, c1.g, num, den),
			Interpolate(c0.b, c1.b, num, den),
			Interpolate(c0.a, c1.a, num, den)
			);

		public static ColorRgbaFloat InterpolateColor(ColorRgbaFloat c0, ColorRgbaFloat c1, float s) => new ColorRgbaFloat(
			Interpolate(c0.r, c1.r, s),
			Interpolate(c0.g, c1.g, s),
			Interpolate(c0.b, c1.b, s),
			Interpolate(c0.a, c1.a, s)
		);

		/// <summary>
		/// Interpolates two colors with the ATC interpolation.
		/// </summary>
		/// <param name="c0">The first color.</param>
		/// <param name="c1">The second color.</param>
		/// <param name="num">The dividend on each color component.</param>
		/// <param name="den">The divisor on each color component.</param>
		/// <returns>The interpolated color.</returns>
		private static ColorRgbaFloat InterpolateColorAtc(ColorRgbaFloat c0, ColorRgbaFloat c1, int num, int den) => new ColorRgbaFloat(
			InterpolateAtc(c0.r, c1.r, num, den),
			InterpolateAtc(c0.g, c1.g, num, den),
			InterpolateAtc(c0.b, c1.b, num, den),
			1);

		/// <summary>
		/// Interpolates two components.
		/// </summary>
		/// <param name="a">The first component.</param>
		/// <param name="b">The second component.</param>
		/// <param name="num">The dividend.</param>
		/// <param name="den">The divisor.</param>
		/// <returns>The interpolated component.</returns>
		private static float Interpolate(float a, float b, int num, int den) =>
			((den - num) * a + num * b) / den;

		private static float Interpolate(float a, float b, float s) =>
			a + s * (b - a);

		/// <summary>
		/// Interpolates two components with the ATC interpolation.
		/// </summary>
		/// <param name="a">The first component.</param>
		/// <param name="b">The second component.</param>
		/// <param name="num">The dividend.</param>
		/// <param name="den">The divisor.</param>
		/// <returns>The interpolated component.</returns>
		private static float InterpolateAtc(float a, float b, int num, int den) =>
			a - num / (float)den * b;

		/// <summary>
		/// Interpolates two components.
		/// </summary>
		/// <param name="a">The first component.</param>
		/// <param name="b">The second component.</param>
		/// <param name="num">The dividend.</param>
		/// <param name="den">The divisor.</param>
		/// <param name="correction">A correction value for increased accuracy when working with integer interpolated values.</param>
		/// <returns>The interpolated component.</returns>
		private static int InterpolateInt(int a, int b, int num, int den, int correction) =>
			(int)(((den - num) * a + num * b + correction) / (float)den);

		/// <summary>
		/// Interpolates two components with the ATC interpolation.
		/// </summary>
		/// <param name="a">The first component.</param>
		/// <param name="b">The second component.</param>
		/// <param name="num">The dividend.</param>
		/// <param name="den">The divisor.</param>
		/// <returns>The interpolated component.</returns>
		private static int InterpolateIntAtc(int a, int b, int num, int den) =>
			(int)(a - num / (float)den * b);

		/// <summary>
		/// Interpolates two colors by half.
		/// </summary>
		/// <param name="c0">The first color endpoint.</param>
		/// <param name="c1">The second color endpoint.</param>
		/// <returns>The interpolated color.</returns>
		public static ColorRgb24 InterpolateHalf(this ColorRgb24 c0, ColorRgb24 c1) =>
			InterpolateColor(c0, c1, 1, 2, 1);

		/// <summary>
		/// Interpolates two colors by third.
		/// </summary>
		/// <param name="c0">The first color endpoint.</param>
		/// <param name="c1">The second color endpoint.</param>
		/// <param name="num">The dividend in the third.</param>
		/// <returns>The interpolated color.</returns>
		public static ColorRgb24 InterpolateThird(this ColorRgb24 c0, ColorRgb24 c1, int num) =>
			InterpolateColor(c0, c1, num, 3, 1);

		/// <summary>
		/// Interpolates two colors by fourth with ATC interpolation.
		/// </summary>
		/// <param name="c0">The first color endpoint.</param>
		/// <param name="c1">The second color endpoint.</param>
		/// <param name="num">The dividend in the fourth.</param>
		/// <returns>The interpolated color.</returns>
		public static ColorRgb24 InterpolateFourthAtc(this ColorRgb24 c0, ColorRgb24 c1, int num) =>
			InterpolateColorAtc(c0, c1, num, 4);

		/// <summary>
		/// Interpolates two colors.
		/// </summary>
		/// <param name="c0">The first color.</param>
		/// <param name="c1">The second color.</param>
		/// <param name="num">The dividend on each color component.</param>
		/// <param name="den">The divisor on each color component.</param>
		/// <returns>The interpolated color.</returns>
		private static ColorRgb24 InterpolateColor(ColorRgb24 c0, ColorRgb24 c1, int num, int den, int correction) => new ColorRgb24(
			(byte)Interpolate(c0.r, c1.r, num, den, correction),
			(byte)Interpolate(c0.g, c1.g, num, den, correction),
			(byte)Interpolate(c0.b, c1.b, num, den, correction));

		/// <summary>
		/// Interpolates two colors with the ATC interpolation.
		/// </summary>
		/// <param name="c0">The first color.</param>
		/// <param name="c1">The second color.</param>
		/// <param name="num">The dividend on each color component.</param>
		/// <param name="den">The divisor on each color component.</param>
		/// <returns>The interpolated color.</returns>
		private static ColorRgb24 InterpolateColorAtc(ColorRgb24 c0, ColorRgb24 c1, int num, int den) => new ColorRgb24(
			(byte)InterpolateAtc(c0.r, c1.r, num, den),
			(byte)InterpolateAtc(c0.g, c1.g, num, den),
			(byte)InterpolateAtc(c0.b, c1.b, num, den));

		/// <summary>
		/// Interpolates two components.
		/// </summary>
		/// <param name="a">The first component.</param>
		/// <param name="b">The second component.</param>
		/// <param name="num">The dividend.</param>
		/// <param name="den">The divisor.</param>
		/// <param name="correction">A correction value for increased accuracy when working with integer interpolated values.</param>
		/// <returns>The interpolated component.</returns>
		private static int Interpolate(int a, int b, int num, int den, int correction = 0) =>
			(int)(((den - num) * a + num * b + correction) / (float)den);

		/// <summary>
		/// Interpolates two components with the ATC interpolation.
		/// </summary>
		/// <param name="a">The first component.</param>
		/// <param name="b">The second component.</param>
		/// <param name="num">The dividend.</param>
		/// <param name="den">The divisor.</param>
		/// <returns>The interpolated component.</returns>
		private static int InterpolateAtc(int a, int b, int num, int den) =>
			(int)(a - num / (float)den * b);
	}
}
