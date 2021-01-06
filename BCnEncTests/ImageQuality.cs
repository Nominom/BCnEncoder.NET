using System;
using System.Collections.Generic;
using System.Text;
using BCnEncoder.Shared;
using Rgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;

namespace BCnEncTests
{
	public class ImageQuality
	{
		public static float PeakSignalToNoiseRatio(ReadOnlySpan<Rgba32> original, ReadOnlySpan<Rgba32> other, bool countAlpha = true)
		{
			if (original.Length != other.Length)
			{
				throw new ArgumentException("Both spans should be the same length");
			}
			float error = 0;
			for (int i = 0; i < original.Length; i++)
			{
				var o = new ColorYCbCr(original[i].ToBcn());
				var c = new ColorYCbCr(other[i].ToBcn());
				error += (o.y - c.y) * (o.y - c.y);
				error += (o.cb - c.cb) * (o.cb - c.cb);
				error += (o.cr - c.cr) * (o.cr - c.cr);
				if (countAlpha)
				{
					error += ((original[i].A - other[i].A) / 255.0f) * ((original[i].A - other[i].A) / 255.0f);
				}

			}
			if (error < float.Epsilon)
			{
				return 100;
			}
			if (countAlpha)
			{
				error /= original.Length * 4;
			}
			else
			{
				error /= original.Length * 3;
			}

			return 20 * MathF.Log10(1 / MathF.Sqrt(error));
		}

		public static float PeakSignalToNoiseRatioLuminance(ReadOnlySpan<Rgba32> original, ReadOnlySpan<Rgba32> other, bool countAlpha = true)
		{
			if (original.Length != other.Length)
			{
				throw new ArgumentException("Both spans should be the same length");
			}
			float error = 0;
			for (int i = 0; i < original.Length; i++)
			{
				var o = new ColorYCbCr(original[i].ToBcn());
				var c = new ColorYCbCr(other[i].ToBcn());
				error += (o.y - c.y) * (o.y - c.y);
				if (countAlpha)
				{
					error += ((original[i].A - other[i].A) / 255.0f) * ((original[i].A - other[i].A) / 255.0f);
				}

			}
			if (error < float.Epsilon)
			{
				return 100;
			}
			if (countAlpha)
			{
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
