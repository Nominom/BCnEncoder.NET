using System;
using System.Diagnostics;

namespace BCnEncoder.Shared.Colors;

public class ColorUtils
{
	/// <summary>
	/// From: https://www.khronos.org/registry/vulkan/specs/1.2/html/chap16.html#_conversion_formulas
	/// </summary>
	public static (uint red, uint green, uint blue, uint exponent) RgbToSharedExponent(float red, float green, float blue, int mantissaBits, int bias, int exponentMax)
	{
		var twoPowN = (float)(1 << mantissaBits);
		var twoPowEmaxMinusB = MathF.Pow(2, exponentMax - bias);
		var sharedExpMax = ((twoPowN - 1) / twoPowN) * twoPowEmaxMinusB;

		var redClamp = MathF.Max(0, MathF.Min(sharedExpMax, red));
		var greClamp = MathF.Max(0, MathF.Min(sharedExpMax, green));
		var bluClamp = MathF.Max(0, MathF.Min(sharedExpMax, blue));

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
			red: redShared,
			green: greShared,
			blue: bluShared,
			exponent: (uint)expShared);
	}

	/// <summary>
	/// From: https://www.khronos.org/registry/vulkan/specs/1.2/html/chap16.html#_conversion_formulas
	/// </summary>
	public static (float red, float green, float blue) SharedExponentToRgb((uint Red, uint Green, uint Blue, uint Exp) shared, int mantissaBits,
		int bias)
	{
		var twoPowExp = MathF.Pow(2, shared.Exp - bias - mantissaBits);
		var red = shared.Red * twoPowExp;
		var green = shared.Green * twoPowExp;
		var blue = shared.Blue * twoPowExp;

		return (red, green, blue);
	}
}
