using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnComp.Net.Shared
{
	public static class SSIM
	{

		/// <summary>
		/// C1 = (0.01*L).^2, where L is the specified DynamicRange value.
		/// C2 = (0.03*L).^2, where L is the specified DynamicRange value.
		/// C3 = C2/2
		/// </summary>
		const double 
			C1 = (0.01*255) * (0.01*255),
			C2 = (0.03*255) * (0.03*255),
			C3 = C2 / 2;

		public static float CalculateSSIM(ReadOnlySpan<Rgba32> original, ReadOnlySpan<Rgba32> compressed) {

			float l = GetSSIMLuminance(original, compressed);
			float c = GetSSIMContrast(original, compressed);
			float s = GetSSIMStructural(original, compressed);

			return l * c * s;
		}

		private static double ColorToLuminance(Rgba32 color) {
			return 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B;
		}

		private static float GetSSIMLuminance(ReadOnlySpan<Rgba32> o, ReadOnlySpan<Rgba32> c)
		{
			double oLum = GetAvgLuminance(o);
			double cLum = GetAvgLuminance(c);

			double v = (2 * oLum * cLum+C1) / (oLum * oLum + cLum * cLum+C1);

			return (float)v;
		}

		private static float GetSSIMContrast(ReadOnlySpan<Rgba32> o, ReadOnlySpan<Rgba32> c)
		{
			double v = 0;
			double oVariance=GetVariance(o);
			double cVariance=GetVariance(c);
			v = (2 * oVariance * cVariance + C2) / (oVariance * oVariance + cVariance * cVariance + C2);
			return (float)v;
		}

		private static float GetSSIMStructural(ReadOnlySpan<Rgba32> o, ReadOnlySpan<Rgba32> c)
		{
			double v = 0;
			double co = GetCovariance(o, c);
			double vo = GetVariance(o);
			double vc = GetVariance(c);
			v = (co + C3) / (vo * vc + C3);
			return (float)v;
		}

		private static double GetAvgLuminance(ReadOnlySpan<Rgba32> pixels){
			double avgLuminance = 0;

			for (int i = 0; i < pixels.Length; i++) {
				var color = pixels[i];
				double c_lumin = ColorToLuminance(color);
				avgLuminance += c_lumin;
			}
			return avgLuminance / pixels.Length;
		}

		private static double GetVariance(ReadOnlySpan<Rgba32> pixels)
		{
			double v = 0;
			for (int i = 0; i < pixels.Length; i++) {
				var color = pixels[i];
				double c_lumin = ColorToLuminance(color);
				v += c_lumin;
			}
			
			double avgV = v / 64;
			v = 0;

			for (int i = 0; i < pixels.Length; i++) {
				var color = pixels[i];
				double c_lumin = ColorToLuminance(color);
				v += (c_lumin - avgV) * (c_lumin - avgV);
			}

			return Math.Sqrt(v / (pixels.Length));
		}

        private static double GetCovariance(ReadOnlySpan<Rgba32> o, ReadOnlySpan<Rgba32> c)
        {
	        double v = 0;

            for (int i = 0; i < o.Length; i++) {
	            var color = o[i];
	            double c_lumin = ColorToLuminance(color);
	            v += c_lumin;
            }

            double o_avgV = v / (o.Length);

            v = 0;
            for (int i = 0; i < c.Length; i++) {
	            var color = c[i];
	            double c_lumin = ColorToLuminance(color);
	            v += c_lumin;
            }
            double c_avgV = v / c.Length;



            v = 0;

            for (int i = 0; i < o.Length; i++) {
	            var o_color = o[i];
	            double o_lumin = ColorToLuminance(o_color);

	            var c_color = c[i];
	            double c_lumin = ColorToLuminance(c_color);

	            v += (o_lumin - o_avgV) * (c_lumin - o_avgV);
            }
            return v / o.Length;
        }
	}
}
