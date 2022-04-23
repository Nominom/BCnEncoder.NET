using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncoder.TextureFormats
{
	internal static class KhrDataFormatUtils
	{
		/// <summary>
		/// From: https://www.khronos.org/registry/vulkan/specs/1.2/html/chap16.html#_conversion_formulas
		/// </summary>
		public static (uint Red, uint Green, uint Blue, uint Exponent) RgbToSharedExponent(ColorRgbaFloat color, int mantissaBits, int bias, int exponentMax)
		{
			var twoPowN = (float)(1 << mantissaBits);
			var twoPowEmaxMinusB = MathF.Pow(2, exponentMax - bias);
			var sharedExpMax = ((twoPowN - 1) /
									twoPowN) * twoPowEmaxMinusB;

			var redClamp = MathF.Max(0, MathF.Min(sharedExpMax, color.r));
			var greClamp = MathF.Max(0, MathF.Min(sharedExpMax, color.g));
			var bluClamp = MathF.Max(0, MathF.Min(sharedExpMax, color.b));

			var maxClamped = MathF.Max(redClamp, MathF.Max(bluClamp, greClamp));

			var prelimExp = maxClamped > MathF.Pow(2, -(bias + 1)) ?
				(int)MathF.Floor(MathF.Log(maxClamped, 2)) + (bias + 1)
				: 0;

			var maxShared = (int) MathF.Floor((maxClamped / MathF.Pow(2, prelimExp - bias - mantissaBits)) + 0.5f);
			var expShared = maxShared == (1 << mantissaBits) ?
					prelimExp + 1 :
					prelimExp;

			Debug.Assert(expShared >= 0, "expShared < 0!!");

			var twoPowExpSharedMinusBMantissa = MathF.Pow(2, expShared - bias - mantissaBits);
			var redShared = (uint)MathF.Floor((redClamp / twoPowExpSharedMinusBMantissa) + 0.5f);
			var greShared = (uint)MathF.Floor((greClamp / twoPowExpSharedMinusBMantissa) + 0.5f);
			var bluShared = (uint)MathF.Floor((bluClamp / twoPowExpSharedMinusBMantissa) + 0.5f);

			return (
				Red: redShared,
				Green: greShared,
				Blue: bluShared,
				Exponent: (uint)expShared);
		}

		/// <summary>
		/// From: https://www.khronos.org/registry/vulkan/specs/1.2/html/chap16.html#_conversion_formulas
		/// </summary>
		public static ColorRgbaFloat SharedExponentToRgb((uint Red, uint Green, uint Blue, uint Exp) shared, int mantissaBits,
			int bias)
		{
			var twoPowExp = MathF.Pow(2, shared.Exp - bias - mantissaBits);
			var red = shared.Red * twoPowExp;
			var green = shared.Green * twoPowExp;
			var blue = shared.Blue * twoPowExp;

			return new ColorRgbaFloat(red, green, blue);
		}
	}
}
