using System;

namespace BCnEncoder.Shared
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

		public static float CalculateLogRMSE(ReadOnlySpan<ColorRgbFloat> original, ReadOnlySpan<ColorRgbFloat> other)
		{
			if (original.Length != other.Length)
			{
				throw new ArgumentException("Both spans should be the same length");
			}
			float error = 0;
			for (var i = 0; i < original.Length; i++)
			{
				//var dr = MathF.Log(other[i].r + 1.0f) - MathF.Log(original[i].r + 1.0f);
				//var dg = MathF.Log(other[i].g + 1.0f) - MathF.Log(original[i].g + 1.0f);
				//var db = MathF.Log(other[i].b + 1.0f) - MathF.Log(original[i].b + 1.0f);
				var dr = Math.Sign(other[i].r) * MathF.Log(1 + MathF.Abs(other[i].r)) - Math.Sign(original[i].r) * MathF.Log(1 + MathF.Abs(original[i].r));
				var dg = Math.Sign(other[i].g) * MathF.Log(1 + MathF.Abs(other[i].g)) - Math.Sign(original[i].g) * MathF.Log(1 + MathF.Abs(original[i].g));
				var db = Math.Sign(other[i].b) * MathF.Log(1 + MathF.Abs(other[i].b)) - Math.Sign(original[i].b) * MathF.Log(1 + MathF.Abs(original[i].b));

				error += dr * dr;
				error += dg * dg;
				error += db * db;

			}
			return MathF.Sqrt(error / (3.0f * original.Length));
		}

		public static float PeakSignalToNoiseRatioLuminance(ReadOnlySpan<ColorRgba32> original, ReadOnlySpan<ColorRgba32> other, bool countAlpha = true) {
			if (original.Length != other.Length) {
				throw new ArgumentException("Both spans should be the same length");
			}
			float error = 0;
			for (var i = 0; i < original.Length; i++) {
				var o = new ColorYCbCr(original[i]);
				var c = new ColorYCbCr(other[i]);
				error += (o.y - c.y) * (o.y - c.y);
				if (countAlpha) {
					error += (original[i].a - other[i].a) / 255.0f * ((original[i].a - other[i].a) / 255.0f);
				}
				
			}
			if (error < float.Epsilon) {
				return 100;
			}
			if (countAlpha) {
				error /= original.Length * 2;
			}
			else
			{
				error /= original.Length;
			}

			return 20 * MathF.Log10(1 / MathF.Sqrt(error));
		}
	}
}
