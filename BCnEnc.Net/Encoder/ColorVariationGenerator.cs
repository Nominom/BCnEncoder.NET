using System;
using System.Collections.Generic;
using System.Text;
using BCnEnc.Net.Shared;

namespace BCnEnc.Net.Encoder
{
	internal static class ColorVariationGenerator
	{
		public static List<ColorRgb565> GenerateVariationsSidewaysMax(int variations, ColorYCbCr min, ColorYCbCr max)
		{
			List<ColorRgb565> colors = new List<ColorRgb565>();
			colors.Add(min.ToColorRgb565());
			colors.Add(max.ToColorRgb565());

			for (int i = 0; i < variations; i++)
			{
				max.y -= 0.05f;
				min.y += 0.05f;

				var ma = max.ToColorRgb565();
				var mi = min.ToColorRgb565();
				if (!colors.Contains(ma))
				{
					colors.Add(ma);
				}

				if (!colors.Contains(mi))
				{
					colors.Add(mi);
				}

				//variate reds in max
				ma.RawR += 1;
				if (!colors.Contains(ma))
				{
					colors.Add(ma);
				}
				ma.RawR -= 2;
				if (!colors.Contains(ma))
				{
					colors.Add(ma);
				}

				//variate blues in max
				ma.RawR += 1;
				ma.RawB += 1;
				if (!colors.Contains(ma))
				{
					colors.Add(ma);
				}
				ma.RawB -= 2;
				if (!colors.Contains(ma))
				{
					colors.Add(ma);
				}

			}

			return colors;
		}

		public static List<ColorRgb565> GenerateVariationsSidewaysMinMax(int variations, ColorYCbCr min, ColorYCbCr max)
		{
			List<ColorRgb565> colors = new List<ColorRgb565>();
			colors.Add(min.ToColorRgb565());
			colors.Add(max.ToColorRgb565());

			for (int i = 0; i < variations; i++)
			{
				max.y -= 0.05f;
				min.y += 0.05f;

				var ma = max.ToColorRgb565();
				var mi = min.ToColorRgb565();
				if (!colors.Contains(ma))
				{
					colors.Add(ma);
				}

				if (!colors.Contains(mi))
				{
					colors.Add(mi);
				}

				//variate reds in max
				ma.RawR += 1;
				if (!colors.Contains(ma))
				{
					colors.Add(ma);
				}
				ma.RawR -= 2;
				if (!colors.Contains(ma))
				{
					colors.Add(ma);
				}

				//variate blues in max
				ma.RawR += 1;
				ma.RawB += 1;
				if (!colors.Contains(ma))
				{
					colors.Add(ma);
				}
				ma.RawB -= 2;
				if (!colors.Contains(ma))
				{
					colors.Add(ma);
				}

				//variate reds in min
				mi.RawR += 1;
				if (!colors.Contains(mi))
				{
					colors.Add(mi);
				}
				ma.RawR -= 2;
				if (!colors.Contains(ma))
				{
					colors.Add(ma);
				}

				//variate blues in min
				mi.RawR += 1;
				mi.RawB += 1;
				if (!colors.Contains(mi))
				{
					colors.Add(mi);
				}
				mi.RawB -= 2;
				if (!colors.Contains(mi))
				{
					colors.Add(mi);
				}

			}

			return colors;
		}
	}
}
