using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using BCnEncoder.Shared.Vectorized;

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
	/// <param name="srgb">A sRGB float value.</param>
	/// <returns>The linear RGB value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static float SrgbToLrgb(float srgb)
	{
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
	/// <returns>The sRGB float value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static float LrgbToSrgb(float lrgb)
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

		return srgb;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static ColorRgbaFloat SrgbToLrgb(ColorRgbaFloat lrgb)
	{
		return new ColorRgbaFloat(
			SrgbToLrgb(lrgb.r),
			SrgbToLrgb(lrgb.g),
			SrgbToLrgb(lrgb.b),
			lrgb.a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static ColorRgbaFloat LrgbToSrgb(ColorRgbaFloat lrgb)
	{
		return new ColorRgbaFloat(
			LrgbToSrgb(lrgb.r),
			LrgbToSrgb(lrgb.g),
			LrgbToSrgb(lrgb.b),
			lrgb.a);
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
		// x = 0.4124f * r + 0.3576f * g + 0.1805f * b;
		// y = 0.2126f * r + 0.7152f * g + 0.0722f * b;
		// z = 0.0193f * r + 0.1192f * g + 0.9505f * b;

		// Which one???

		x = 0.5142f * r + 0.3240f * g + 0.1618f * b;
		y = 0.2652f * r + 0.6702f * g + 0.0646f * b;
		z = 0.0240f * r + 0.1229f * g + 0.8531f * b;
	}

	public static void XyzToLrgb(float x, float y, float z, out float r, out float g, out float b)
	{
		// Observer. = 2°, Illuminant = D65
		// r = 3.2406f * x - 1.5372f * y - 0.4986f * z;
		// g = -0.9689f * x + 1.8758f * y + 0.0415f * z;
		// b = 0.0557f * x - 0.2040f * y + 1.0570f * z;

		// Which one????

		r = 2.565f * x - 1.167f * y - 0.398f * z;
		g = -1.022f * x + 1.978f * y + 0.044f * z;
		b = 0.075f * x - 0.252f * y + 1.177f * z;
	}

	public static class Srgb
	{
		public static Vector4 ToLrgb(Vector4 srgb)
		{
			// Preserve alpha
			float A = srgb.W;
			srgb.W = 0f;

			Vector128<float> cmp128 = Vector128.Create(0.04045f);
			Vector128<float> linearMultiplier128 = Vector128.Create(12.92f);
			Vector128<float> gammaMultiplier128 = Vector128.Create(1.055f);
			Vector128<float> gammaBias128 = Vector128.Create(0.055f);

			Vector128<float> srgb128 = srgb.AsVector128();

			// if (srgb <= 0.04045f)
			// {
			// 	lrgb = srgb / 12.92f;
			// }
			Vector128<float> linearMask = Vector128.LessThanOrEqual(srgb128, cmp128);
			Vector128<float> lrgb128 = srgb128 / linearMultiplier128;

			// else
			// {
			// 	lrgb = MathF.Pow((srgb + 0.055f) / 1.055f, 2.4f);
			// }

			// Avoid expensive operations if possible
			if (!Vector128.EqualsAll(linearMask, Vector128<float>.AllBitsSet))
			{
				Vector128<float> gamma = srgb128 + gammaBias128;
				gamma = gamma / gammaMultiplier128;
				gamma = MathV.Pow(gamma, Vector128.Create(2.4f));

				lrgb128 = Vector128.ConditionalSelect(linearMask, lrgb128, gamma);
			}

			Vector4 lrgb = lrgb128.AsVector4();

			// Restore Alpha
			lrgb.W = A;

			return lrgb;
		}

		public static Vector4 ToSrgb(Vector4 lrgb)
		{
			// Preserve alpha
			float A = lrgb.W;
			lrgb.W = 0f;

			Vector128<float> cmp128 = Vector128.Create(0.0031308f);
			Vector128<float> linearMultiplier128 = Vector128.Create(12.92f);
			Vector128<float> gammaMultiplier128 = Vector128.Create(1.055f);
			Vector128<float> gammaBias128 = Vector128.Create(0.055f);

			Vector128<float> lrgb128 = lrgb.AsVector128();

			// if (lrgb <= 0.0031308f)
			// {
			// 	srgb = lrgb * 12.92f;
			// }

			Vector128<float> linearMask = Vector128.LessThanOrEqual(lrgb128, cmp128);
			Vector128<float> srgb128 = Vector128.Multiply(lrgb128, linearMultiplier128);

			// else
			// {
			// 	srgb = 1.055f * MathF.Pow(lrgb, 1 / 2.4f) - 0.055f;
			// }

			// Avoid expensive operations if possible
			if (!Vector128.EqualsAll(linearMask, Vector128<float>.AllBitsSet))
			{
				Vector128<float> gamma = MathV.Pow(lrgb128, Vector128.Create(1f / 2.4f));
				gamma = gamma * gammaMultiplier128;
				gamma = gamma - gammaBias128;
				srgb128 = Vector128.ConditionalSelect(linearMask, srgb128, gamma);
			}

			Vector4 srgb = srgb128.AsVector4();

			// Restore Alpha
			srgb.W = A;

			return srgb;
		}
	}

	public static class CieXyz
	{
		public static readonly Vector4 WhitePointD65 = new Vector4(0.95047f, 1.0f, 1.08883f, 1.0f);

		public static readonly Matrix4x4 RgbToXyz = new Matrix4x4(
			0.4124564f, 0.2126729f, 0.0193339f, 0f,
			0.3575761f, 0.7151522f, 0.1191920f, 0f,
			0.1804375f, 0.0721750f, 0.9503041f, 0f,
			0f, 0f, 0f, 1f
		);

		public static readonly Matrix4x4 XyzToRgb = new Matrix4x4(
			3.2404542f, -1.5371385f, -0.4985314f, 0f,
			-0.9692660f, 1.8759121f, 0.0415550f, 0f,
			0.0556434f, -0.2040259f, 1.0572252f, 0f,
			0f, 0f, 0f, 1f
		);

		public static Vector4 ToXyza(Vector4 rgba) => Vector4.Transform(rgba, RgbToXyz);

		public static Vector4 ToRgb(Vector4 xyza) => Vector4.Transform(xyza, XyzToRgb);
	}

	public static class CieLuv
	{
		private const float Yn = 1.0f;
		private const float epsilon = 0.008856f;
		private const float kappa = 903.3f;

		private static readonly Vector4 DenomWeights = new Vector4(1f, 15f, 3f, 0f);
		private static readonly Vector4 UWeights = new Vector4(4f, 0f, 0f, 0f);
		private static readonly Vector4 VWeights = new Vector4(0f, 9f, 0f, 0f);

		private const float uPrime_n = 0.197830f;
		private const float vPrime_n = 0.468319f;

		public static Vector4 XyzaToLuva(Vector4 xyzAlpha)
		{
			float Y = xyzAlpha.Y;
			float A = xyzAlpha.W;

			float denom = Vector4.Dot(xyzAlpha, DenomWeights);
			float uPrime = denom == 0 ? 0 : Vector4.Dot(xyzAlpha, UWeights) / denom;
			float vPrime = denom == 0 ? 0 : Vector4.Dot(xyzAlpha, VWeights) / denom;

			float yr = Y / Yn;
			float L = yr > epsilon ? 116f * MathF.Pow(yr, 1f / 3f) - 16f : kappa * yr;

			float u = 13f * L * (uPrime - uPrime_n);
			float v = 13f * L * (vPrime - vPrime_n);

			return new Vector4(L, u, v, A);
		}
	}


	public static class CieLab
	{
		public static Vector4 XyzaToLaba(Vector4 xyza)
		{
			Vector3 xyz = new Vector3(xyza.X, xyza.Y, xyza.Z);
			Vector3 wp = new Vector3(CieXyz.WhitePointD65.X, CieXyz.WhitePointD65.Y, CieXyz.WhitePointD65.Z);

			// Normalize by white point
			xyz /= wp;

			// Apply the nonlinear function
			Vector3 f = new Vector3(
				PivotXyz(xyz.X),
				PivotXyz(xyz.Y),
				PivotXyz(xyz.Z)
			);

			float L = 116f * f.Y - 16f;
			float a = 500f * (f.X - f.Y);
			float b = 200f * (f.Y - f.Z);

			return new Vector4(L, a, b, xyza.W); // Preserve alpha
		}

		private static float PivotXyz(float t)
		{
			const float epsilon = 0.008856f; // (6/29)^3
			const float kappa = 903.3f;      // (29/3)^3

			return (t > epsilon) ? MathF.Pow(t, 1f / 3f) : (kappa * t + 16f) / 116f;
		}

		public static float CieDE2000(Vector4 lab1, Vector4 lab2)
		{
			// Step 1: Extract components
			float L1 = lab1.X, a1 = lab1.Y, b1 = lab1.Z;
			float L2 = lab2.X, a2 = lab2.Y, b2 = lab2.Z;

			float avgLp = (L1 + L2) / 2f;

			float C1 = MathF.Sqrt(a1 * a1 + b1 * b1);
			float C2 = MathF.Sqrt(a2 * a2 + b2 * b2);
			float avgC = (C1 + C2) / 2f;

			float G = 0.5f * (1 - MathF.Sqrt(MathF.Pow(avgC, 7) / (MathF.Pow(avgC, 7) + MathF.Pow(25f, 7))));

			float a1p = (1 + G) * a1;
			float a2p = (1 + G) * a2;

			float C1p = MathF.Sqrt(a1p * a1p + b1 * b1);
			float C2p = MathF.Sqrt(a2p * a2p + b2 * b2);

			float avgCp = (C1p + C2p) / 2f;

			float h1p = MathF.Atan2(b1, a1p);
			float h2p = MathF.Atan2(b2, a2p);
			if (h1p < 0) h1p += 2 * MathF.PI;
			if (h2p < 0) h2p += 2 * MathF.PI;

			float deltahp;
			if (MathF.Abs(h1p - h2p) > MathF.PI)
			{
				deltahp = h2p <= h1p ? h2p - h1p + 2 * MathF.PI : h2p - h1p - 2 * MathF.PI;
			}
			else
			{
				deltahp = h2p - h1p;
			}

			float deltaLp = L2 - L1;
			float deltaCp = C2p - C1p;
			float deltaHp = 2 * MathF.Sqrt(C1p * C2p) * MathF.Sin(deltahp / 2f);

			float avgHp;
			if (MathF.Abs(h1p - h2p) > MathF.PI)
			{
				avgHp = (h1p + h2p + 2 * MathF.PI) / 2f;
			}
			else
			{
				avgHp = (h1p + h2p) / 2f;
			}

			float T = 1
			          - 0.17f * MathF.Cos(avgHp - MathF.PI / 6)
			          + 0.24f * MathF.Cos(2 * avgHp)
			          + 0.32f * MathF.Cos(3 * avgHp + MathF.PI / 30)
			          - 0.20f * MathF.Cos(4 * avgHp - 21 * MathF.PI / 60);

			float deltaTheta = 30 * MathF.PI / 180 * MathF.Exp(-MathF.Pow((avgHp * 180f / MathF.PI - 275f) / 25f, 2));
			float Rc = 2 * MathF.Sqrt(MathF.Pow(avgCp, 7) / (MathF.Pow(avgCp, 7) + MathF.Pow(25f, 7)));
			float Sl = 1 + ((0.015f * MathF.Pow(avgLp - 50, 2)) / MathF.Sqrt(20 + MathF.Pow(avgLp - 50, 2)));
			float Sc = 1 + 0.045f * avgCp;
			float Sh = 1 + 0.015f * avgCp * T;
			float Rt = -MathF.Sin(2 * deltaTheta) * Rc;

			float deltaE = MathF.Sqrt(
				MathF.Pow(deltaLp / Sl, 2) +
				MathF.Pow(deltaCp / Sc, 2) +
				MathF.Pow(deltaHp / Sh, 2) +
				Rt * (deltaCp / Sc) * (deltaHp / Sh)
			);

			return deltaE;
		}
	}

	public static class Oklab
	{
		private static readonly Matrix4x4 M1 = new Matrix4x4(
			0.8189330101f, 0.0329845436f, 0.0482003018f, 0,
			0.3618667424f, 0.9293118715f, 0.2643662691f, 0,
			-0.1288597137f, 0.0361456387f, 0.6338517070f, 0,
			0,             0,             0,             1);

		private static readonly Matrix4x4 M2 = new Matrix4x4(
			0.2104542553f, 1.9779984951f, 0.0259040371f, 0,
			0.7936177850f, -2.4285922050f, 0.7827717662f, 0,
			-0.0040720468f, 0.4505937099f, -0.8086757660f, 0,
			0,             0,             0,              1);

		public static Vector4 XyzaToLaba(Vector4 xyzAlpha)
		{
			// xyzAlpha /= CieXyz.WhitePointD65;

			// Apply first linear transform
			Vector4 labAlpha = Vector4.Transform(xyzAlpha, M1);

			labAlpha = new Vector4(
				MathF.Cbrt(labAlpha.X),
				MathF.Cbrt(labAlpha.Y),
				MathF.Cbrt(labAlpha.Z),
				labAlpha.W);

			labAlpha = Vector4.Transform(labAlpha, M2);

			return labAlpha;
		}

		public static Vector4 XyzaToLCha(Vector4 xyzAlpha)
		{
			Vector4 labAlpha = XyzaToLaba(xyzAlpha);

			float C = MathF.Sqrt(labAlpha.Y * labAlpha.Y + labAlpha.Z * labAlpha.Z);
			float h = MathF.Atan2(labAlpha.Z, labAlpha.Y);
			if (h < 0) h += 2 * MathF.PI; // Normalize to [0, 2π)

			return new Vector4(labAlpha.X, C, h, labAlpha.W);
		}

		public static Vector4 LrgbToOklab(Vector4 rgba)
		{
			float l = 0.4122214708f * rgba.X + 0.5363325363f * rgba.Y + 0.0514459929f * rgba.Z;
			float m = 0.2119034982f * rgba.X + 0.6806995451f * rgba.Y + 0.1073969566f * rgba.Z;
			float s = 0.0883024619f * rgba.X + 0.2817188376f * rgba.Y + 0.6299787005f * rgba.Z;

			l = MathF.Cbrt(l);
			m = MathF.Cbrt(m);
			s = MathF.Cbrt(s);

			return new Vector4(
				0.2104542553f*l + 0.7936177850f*m - 0.0040720468f*s,
				1.9779984951f*l - 2.4285922050f*m + 0.4505937099f*s,
				0.0259040371f*l + 0.7827717662f*m - 0.8086757660f*s,
				rgba.W
			);
		}

		public static Vector4 OklabToLrgb(Vector4 okLab)
		{
			float l = okLab.X + 0.3963377774f * okLab.Y + 0.2158037573f * okLab.Z;
			float m = okLab.X - 0.1055613458f * okLab.Y - 0.0638541728f * okLab.Z;
			float s = okLab.X - 0.0894841775f * okLab.Y - 1.2914855480f * okLab.Z;

			l = l*l*l;
			m = m*m*m;
			s = s*s*s;

			return new Vector4(
				+4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s,
				-1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s,
				-0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s,
				okLab.W
			);
		}

		public static Vector4 OklabToLcha(Vector4 okLab)
		{
			float C = MathF.Sqrt(okLab.Y * okLab.Y + okLab.Z * okLab.Z);
			float h = MathF.Atan2(okLab.Z, okLab.Y);
			if (h < 0) h += 2 * MathF.PI; // Normalize to [0, 2π)

			return new Vector4(okLab.X, C, h, okLab.W);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DeltaE(Vector4 lab1, Vector4 lab2)
		{
			Vector4 lScale = new Vector4(2.016f, 1, 1, 1);

			Vector4 diff = lab1 - lab2;
			diff *= lScale;

			return diff.Length();
		}
	}
}
