using System;

namespace BCnEncoder.Shared.Colors;

public static class ColorSpace
{
	/// <summary>
	/// Converts sRGB to linear RGB.
	/// </summary>
	/// <remarks>
	/// The sRGB -> linear RGB conversion is based on the sRGB spec:
	/// http://www.w3.org/Graphics/Color/sRGB.html
	/// </remarks>
	/// <param name="srgbByte">A byte value between 0 and 255, representing the sRGB value.</param>
	/// <returns>The linear RGB value.</returns>
	public static float SrgbToLrgb(byte srgbByte)
	{
		float srgb = srgbByte / 255.0f;
		float lrgb;

		if (srgb <= 0.04045f)
		{
			lrgb = srgb / 12.92f;
		}
		else
		{
			lrgb = MathF.Pow((srgb + 0.055f) / 1.055f, 2.4f);
		}

		return lrgb;
	}

	/// <summary>
	/// Converts linear RGB to sRGB.
	/// </summary>
	/// <remarks>
	/// The linear RGB -> sRGB conversion is based on the sRGB spec:
	/// http://www.w3.org/Graphics/Color/sRGB.html
	/// </remarks>
	/// <param name="lrgb">A linear RGB value.</param>
	/// <returns>The sRGB byte value.</returns>
	public static byte LrgbToSrgb(float lrgb)
	{
		float srgb;

		if (lrgb <= 0.0031308f)
		{
			srgb = lrgb * 12.92f;
		}
		else
		{
			srgb = 1.055f * MathF.Pow(lrgb, 1 / 2.4f) - 0.055f;
		}

		return (byte)MathF.Round(srgb * 255);
	}

	public static void LrgbToYCbCr(float r, float g, float b, out float y, out float cb, out float cr)
	{
		y = 0.2989f * r + 0.5866f * g + 0.1145f * b;
		cb = -0.1687f * r - 0.3313f * g + 0.5000f * b;
		cr = 0.5000f * r - 0.4184f * g - 0.0816f * b;
	}

	public static void YCbCrToLrgb(float y, float cb, float cr, out float r, out float g, out float b)
	{
		r = y + 0.0000f * cb + 1.4022f * cr;
		g = y - 0.3456f * cb - 0.7145f * cr;
		b = y + 1.7710f * cb + 0.0000f * cr;
	}

	public static void LrgbToXyz(float r, float g, float b, out float x, out float y, out float z)
	{
		// Observer. = 2°, Illuminant = D65
		x = 0.4124f * r + 0.3576f * g + 0.1805f * b;
		y = 0.2126f * r + 0.7152f * g + 0.0722f * b;
		z = 0.0193f * r + 0.1192f * g + 0.9505f * b;

		// Which one???

		// x = 0.5142f * r + 0.3240f * g + 0.1618f * b;
		// y = 0.2652f * r + 0.6702f * g + 0.0646f * b;
		// z = 0.0240f * r + 0.1229f * g + 0.8531f * b;
	}

	public static void XyzToLrgb(float x, float y, float z, out float r, out float g, out float b)
	{
		// Observer. = 2°, Illuminant = D65
		r = 3.2406f * x - 1.5372f * y - 0.4986f * z;
		g = -0.9689f * x + 1.8758f * y + 0.0415f * z;
		b = 0.0557f * x - 0.2040f * y + 1.0570f * z;

		// Which one????

		// r = 2.565f * x - 1.167f * y - 0.398f * z;
		// g = -1.022f * x + 1.978f * y + 0.044f * z;
		// b = 0.075f * x - 0.252f * y + 1.177f * z;
	}
}
