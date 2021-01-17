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
		public static ColorRgb24 InterpolateHalf(this ColorRgb24 c0, ColorRgb24 c1) =>
			InterpolateColor(c0, c1, 1, 2);

		/// <summary>
		/// Interpolates two colors by third.
		/// </summary>
		/// <param name="c0">The first color endpoint.</param>
		/// <param name="c1">The second color endpoint.</param>
		/// <param name="num">The dividend in the third.</param>
		/// <returns>The interpolated color.</returns>
		public static ColorRgb24 InterpolateThird(this ColorRgb24 c0, ColorRgb24 c1, int num) =>
			InterpolateColor(c0, c1, num, 3);

		/// <summary>
		/// Interpolates two colors by fifth.
		/// </summary>
		/// <param name="a0">The first component.</param>
		/// <param name="a1">The second component.</param>
		/// <param name="num">The dividend in the fifth.</param>
		/// <returns>The interpolated component.</returns>
		public static byte InterpolateFifth(this byte a0, byte a1, int num) =>
			(byte)Interpolate(a0, a1, num, 5, 2);

		/// <summary>
		/// Interpolates two colors by seventh.
		/// </summary>
		/// <param name="a0">The first component.</param>
		/// <param name="a1">The second component.</param>
		/// <param name="num">The dividend in the seventh.</param>
		/// <returns>The interpolated component.</returns>
		public static byte InterpolateSeventh(this byte a0, byte a1, int num) =>
			(byte)Interpolate(a0, a1, num, 7, 3);

		/// <summary>
		/// Interpolates two colors.
		/// </summary>
		/// <param name="c0">The first color.</param>
		/// <param name="c1">The second color.</param>
		/// <param name="num">The dividend on each color component.</param>
		/// <param name="den">The divisor on each color component.</param>
		/// <returns>The interpolated color.</returns>
		private static ColorRgb24 InterpolateColor(ColorRgb24 c0, ColorRgb24 c1, int num, int den) => new ColorRgb24(
			(byte)Interpolate(c0.r, c1.r, num, den),
			(byte)Interpolate(c0.g, c1.g, num, den),
			(byte)Interpolate(c0.b, c1.b, num, den));

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
	}
}
