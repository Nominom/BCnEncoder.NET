using System.Collections.Generic;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal static class ColorVariationGenerator
	{

		private static readonly int[] variatePatternEp0R = new int[] { 1, 1, 0, 0, -1, 0, 0, -1, 1, -1, 1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		private static readonly int[] variatePatternEp0G = new int[] { 1, 0, 1, 0, 0, -1, 0, -1, 1, -1, 0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		private static readonly int[] variatePatternEp0B = new int[] { 1, 0, 0, 1, 0, 0, -1, -1, 1, -1, 0, 0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0 };
		private static readonly int[] variatePatternEp1R = new int[] { -1, -1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, -1, 1, 0, 0, -1, 0, 0 };
		private static readonly int[] variatePatternEp1G = new int[] { -1, 0, -1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, -1, 0, 1, 0, 0, -1, 0 };
		private static readonly int[] variatePatternEp1B = new int[] { -1, 0, 0, -1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, -1, 0, 0, 1, 0, 0, -1 };
		public static int VarPatternCount => variatePatternEp0R.Length;

		public static (ColorRgb565, ColorRgb565) Variate565(ColorRgb565 c0, ColorRgb565 c1, int i)
		{
			var idx = i % variatePatternEp0R.Length;
			var newEp0 = new ColorRgb565();
			var newEp1 = new ColorRgb565();

			newEp0.RawR = ByteHelper.ClampToByte(c0.RawR + variatePatternEp0R[idx]);
			newEp0.RawG = ByteHelper.ClampToByte(c0.RawG + variatePatternEp0G[idx]);
			newEp0.RawB = ByteHelper.ClampToByte(c0.RawB + variatePatternEp0B[idx]);

			newEp1.RawR = ByteHelper.ClampToByte(c1.RawR + variatePatternEp1R[idx]);
			newEp1.RawG = ByteHelper.ClampToByte(c1.RawG + variatePatternEp1G[idx]);
			newEp1.RawB = ByteHelper.ClampToByte(c1.RawB + variatePatternEp1B[idx]);

			return (newEp0, newEp1);
		}


		public static ((int, int, int), (int, int, int)) VariateInt((int, int, int) ep0,
			(int, int, int) ep1, int i)
		{
			var idx = i % variatePatternEp0R.Length;

			return ((
					ep0.Item1 + variatePatternEp0R[idx],
					ep0.Item2 + variatePatternEp0G[idx],
					ep0.Item3 + variatePatternEp0B[idx]
				),
				(
					ep1.Item1 + variatePatternEp1R[idx],
					ep1.Item2 + variatePatternEp1G[idx],
					ep1.Item3 + variatePatternEp1B[idx]
				));
		}
	}
}
