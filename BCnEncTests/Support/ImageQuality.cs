using System;
using BCnEncoder.Shared;

namespace BCnEncTests.Support
{
	public class ImageQuality
	{
		public static float PeakSignalToNoiseRatio(ReadOnlySpan<ColorRgba32> original, ReadOnlySpan<ColorRgba32> other, bool countAlpha = true) {
			if (original.Length != other.Length) {
				throw new ArgumentException("Both spans should be the same length");
			}
			float error = 0;
			for (var i = 0; i < original.Length; i++) {
				var o = new ColorYCbCr(original[i]);
				var c = new ColorYCbCr(other[i]);
				error += (o.y - c.y) * (o.y - c.y);
				error += (o.cb - c.cb) * (o.cb - c.cb);
				error += (o.cr - c.cr) * (o.cr - c.cr);
				if (countAlpha) {
					error += (original[i].a - other[i].a) / 255.0f * ((original[i].a - other[i].a) / 255.0f);
				}
				
			}
			if (error < float.Epsilon) {
				return 100;
			}
			if (countAlpha) {
				error /= original.Length * 4;
			}
			else
			{
				error /= original.Length * 3;
			}

			return 20 * MathF.Log10(1 / MathF.Sqrt(error));
		}

		public static float CalculateLogRMSE(ReadOnlySpan<ColorRgbaFloat> original, ReadOnlySpan<ColorRgbaFloat> other, bool countAlpha)
		{
			if (original.Length != other.Length)
			{
				throw new ArgumentException("Both spans should be the same length");
			}
			float error = 0;
			for (var i = 0; i < original.Length; i++)
			{
				var dr = Math.Sign(other[i].r) * MathF.Log(1 + MathF.Abs(other[i].r)) - Math.Sign(original[i].r) * MathF.Log(1 + MathF.Abs(original[i].r));
				var dg = Math.Sign(other[i].g) * MathF.Log(1 + MathF.Abs(other[i].g)) - Math.Sign(original[i].g) * MathF.Log(1 + MathF.Abs(original[i].g));
				var db = Math.Sign(other[i].b) * MathF.Log(1 + MathF.Abs(other[i].b)) - Math.Sign(original[i].b) * MathF.Log(1 + MathF.Abs(original[i].b));
				var da = Math.Sign(other[i].a) * MathF.Log(1 + MathF.Abs(other[i].a)) - Math.Sign(original[i].a) * MathF.Log(1 + MathF.Abs(original[i].a));

				error += dr * dr;
				error += dg * dg;
				error += db * db;

				if (countAlpha)
				{
					error += da * da;
				}

			}
			return countAlpha ?
				MathF.Sqrt(error / (4.0f * original.Length)):
				MathF.Sqrt(error / (3.0f * original.Length));
		}
	}
}
