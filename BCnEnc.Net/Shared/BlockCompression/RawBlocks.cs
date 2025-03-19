using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using BCnEncoder.Encoder.Bptc;
using BCnEncoder.Shared.Colors;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Shared
{

	// public struct RawBlock4X4Rgba32
	// {
	// 	public ColorRgba32 p00, p10, p20, p30;
	// 	public ColorRgba32 p01, p11, p21, p31;
	// 	public ColorRgba32 p02, p12, p22, p32;
	// 	public ColorRgba32 p03, p13, p23, p33;
	//
	// 	public RawBlock4X4Rgba32(ColorRgba32 fillColor)
	// 	{
	// 		p00 = p01 = p02 = p03 =
	// 			p10 = p11 = p12 = p13 =
	// 				p20 = p21 = p22 = p23 =
	// 					p30 = p31 = p32 = p33 = fillColor;
	// 	}
	//
	// 	public Span<ColorRgba32> AsSpan => MemoryMarshal.CreateSpan(ref p00, 16);
	//
	// 	public ColorRgba32 this[int x, int y]
	// 	{
	// 		get => AsSpan[x + y * 4];
	// 		set => AsSpan[x + y * 4] = value;
	// 	}
	//
	// 	public ColorRgba32 this[int index]
	// 	{
	// 		get => AsSpan[index];
	// 		set => AsSpan[index] = value;
	// 	}
	//
	// 	internal int CalculateError(RawBlock4X4Rgba32 other, bool useAlpha = false)
	// 	{
	// 		float error = 0;
	// 		var pix1 = AsSpan;
	// 		var pix2 = other.AsSpan;
	// 		for (var i = 0; i < pix1.Length; i++)
	// 		{
	// 			var col1 = pix1[i];
	// 			var col2 = pix2[i];
	//
	// 			var re = col1.r - col2.r;
	// 			var ge = col1.g - col2.g;
	// 			var be = col1.b - col2.b;
	//
	// 			error += re * re;
	// 			error += ge * ge;
	// 			error += be * be;
	//
	// 			if (useAlpha)
	// 			{
	// 				var ae = col1.a - col2.a;
	// 				error += ae * ae * 4;
	// 			}
	// 		}
	//
	// 		error /= pix1.Length;
	// 		error = MathF.Sqrt(error);
	//
	// 		return (int)error;
	// 	}
	//
	// 	internal float CalculateYCbCrError(RawBlock4X4Rgba32 other)
	// 	{
	// 		float yError = 0;
	// 		float cbError = 0;
	// 		float crError = 0;
	// 		var pix1 = AsSpan;
	// 		var pix2 = other.AsSpan;
	// 		for (var i = 0; i < pix1.Length; i++)
	// 		{
	// 			var col1 = pix1[i].As<ColorYCbCr>();
	// 			var col2 = pix2[i].As<ColorYCbCr>();
	//
	// 			var ye = col1.y - col2.y;
	// 			var cbe = col1.cb - col2.cb;
	// 			var cre = col1.cr - col2.cr;
	//
	// 			yError += ye * ye;
	// 			cbError += cbe * cbe;
	// 			crError += cre * cre;
	// 		}
	//
	// 		var error = yError * 2 + cbError / 2 + crError / 2;
	//
	// 		return error;
	// 	}
	//
	// 	internal float CalculateYCbCrAlphaError(RawBlock4X4Rgba32 other, float yMultiplier = 2, float alphaMultiplier = 1)
	// 	{
	// 		float yError = 0;
	// 		float cbError = 0;
	// 		float crError = 0;
	// 		float alphaError = 0;
	// 		var pix1 = AsSpan;
	// 		var pix2 = other.AsSpan;
	// 		for (var i = 0; i < pix1.Length; i++)
	// 		{
	// 			var col1 = pix1[i].As<ColorYCbCrAlpha>();
	// 			var col2 = pix2[i].As<ColorYCbCrAlpha>();
	//
	// 			var ye = (col1.y - col2.y) * yMultiplier;
	// 			var cbe = col1.cb - col2.cb;
	// 			var cre = col1.cr - col2.cr;
	// 			var ae = (col1.a - col2.a) * alphaMultiplier;
	//
	// 			yError += ye * ye;
	// 			cbError += cbe * cbe;
	// 			crError += cre * cre;
	// 			alphaError += ae * ae;
	// 		}
	//
	// 		var error = yError + cbError + crError + alphaError;
	// 		return error;
	// 	}
	//
	// 	internal RawBlock4X4Ycbcr ToRawBlockYcbcr()
	// 	{
	// 		var rawYcbcr = new RawBlock4X4Ycbcr();
	// 		var pixels = AsSpan;
	// 		var ycbcrPs = rawYcbcr.AsSpan;
	// 		for (var i = 0; i < pixels.Length; i++)
	// 		{
	// 			ycbcrPs[i] = pixels[i].As<ColorYCbCr>();
	// 		}
	// 		return rawYcbcr;
	// 	}
	//
	// 	public bool HasTransparentPixels()
	// 	{
	// 		var pixels = AsSpan;
	// 		for (var i = 0; i < pixels.Length; i++)
	// 		{
	// 			if (pixels[i].a < 255) return true;
	// 		}
	// 		return false;
	// 	}
	// }

	public struct RawBlock4X4RgbaFloat
	{
		public ColorRgbaFloat p00, p10, p20, p30;
		public ColorRgbaFloat p01, p11, p21, p31;
		public ColorRgbaFloat p02, p12, p22, p32;
		public ColorRgbaFloat p03, p13, p23, p33;

		public RawBlock4X4RgbaFloat(ColorRgbaFloat fillColor)
		{
			p00 = p01 = p02 = p03 =
				p10 = p11 = p12 = p13 =
					p20 = p21 = p22 = p23 =
						p30 = p31 = p32 = p33 = fillColor;
		}

		public Span<ColorRgbaFloat> AsSpan => MemoryMarshal.CreateSpan(ref p00, 16);
		internal Span<Vector128<float> > AsVector128 => MemoryMarshal.CreateSpan(ref p00, 16).Cast<ColorRgbaFloat, Vector128<float>>();
		internal Span<Vector256<float> > AsVector256 => MemoryMarshal.CreateSpan(ref p00, 16).Cast<ColorRgbaFloat, Vector256<float>>();

		public ColorRgbaFloat this[int x, int y]
		{
			get => AsSpan[x + y * 4];
			set => AsSpan[x + y * 4] = value;
		}

		public ColorRgbaFloat this[int index]
		{
			get => AsSpan[index];
			set => AsSpan[index] = value;
		}

		internal float CalculateError(RawBlock4X4RgbaFloat other)
		{
			float error = 0;
			var pix1 = AsSpan;
			var pix2 = other.AsSpan;

			for (var i = 0; i < pix1.Length; i++)
			{
				var col1 = pix1[i];
				var col2 = pix2[i];

				var re = Math.Sign(col1.r) * MathF.Log( 1 + MathF.Abs(col1.r)) - Math.Sign(col2.r) * MathF.Log( 1 + MathF.Abs(col2.r));
				var ge = Math.Sign(col1.g) * MathF.Log( 1 + MathF.Abs(col1.g)) - Math.Sign(col2.g) * MathF.Log( 1 + MathF.Abs(col2.g));
				var be = Math.Sign(col1.b) * MathF.Log( 1 + MathF.Abs(col1.b)) - Math.Sign(col2.b) * MathF.Log( 1 + MathF.Abs(col2.b));
				var ae = Math.Sign(col1.a) * MathF.Log( 1 + MathF.Abs(col1.a)) - Math.Sign(col2.a) * MathF.Log( 1 + MathF.Abs(col2.a));

				error += re * re;
				error += ge * ge;
				error += be * be;
				error += ae * ae;
			}

			error /= pix1.Length * 4;
			error = MathF.Sqrt(error);

			return error;
		}


		internal RawBlock4X4YcbcrAlpha ToRawBlockYcbcrAlpha()
		{
			var rawYcbcr = new RawBlock4X4YcbcrAlpha();
			var pixels = AsSpan;
			var ycbcrPs = rawYcbcr.AsSpan;
			for (var i = 0; i < pixels.Length; i++)
			{
				pixels[i].To(ref ycbcrPs[i]);
			}
			return rawYcbcr;
		}

		public bool HasTransparentPixels()
		{
			var pixels = AsSpan;
			for (var i = 0; i < pixels.Length; i++)
			{
				if (pixels[i].a < 1) return true;
			}
			return false;
		}

		internal void ColorConvert(ColorConversionMode mode)
		{
			if (mode == ColorConversionMode.None)
				return;
			Span<ColorRgbaFloat> pixels = AsSpan;

			if (mode == ColorConversionMode.LinearToSrgb)
			{
				for (var i = 0; i < pixels.Length; i++)
				{
					pixels[i] = ColorSpace.LrgbToSrgb(pixels[i]);
				}
			}
			else
			{
				for (var i = 0; i < pixels.Length; i++)
				{
					pixels[i] = ColorSpace.SrgbToLrgb(pixels[i]);
				}
			}
		}

		internal float CalculateYCbCrAlphaError(RawBlock4X4RgbaFloat other, float yMultiplier = 2,
			float alphaMultiplier = 1)
		{
			// TODO: COLORSPACE
			return ToRawBlockYcbcrAlpha()
				.CalculateError(other.ToRawBlockYcbcrAlpha(), yMultiplier, 1, 1, alphaMultiplier);
		}
	}

	//Used for Bc6H
	// internal struct RawBlock4X4RgbHalfInt
	// {
	// 	public (int, int, int) p00, p10, p20, p30;
	// 	public (int, int, int) p01, p11, p21, p31;
	// 	public (int, int, int) p02, p12, p22, p32;
	// 	public (int, int, int) p03, p13, p23, p33;
	// 	public Span<(int, int, int)> AsSpan => MemoryMarshal.CreateSpan(ref p00, 16);
	//
	// 	public (int, int, int) this[int x, int y]
	// 	{
	// 		get => AsSpan[x + y * 4];
	// 		set => AsSpan[x + y * 4] = value;
	// 	}
	//
	// 	public (int, int, int) this[int index]
	// 	{
	// 		get => AsSpan[index];
	// 		set => AsSpan[index] = value;
	// 	}
	//
	// 	public static RawBlock4X4RgbHalfInt FromRawFloats(RawBlock4X4RgbFloat other, bool signed)
	// 	{
	// 		var output = new RawBlock4X4RgbHalfInt();
	// 		var span = output.AsSpan;
	// 		var floats = other.AsSpan;
	// 		for (var i = 0; i < 16; i++)
	// 		{
	// 			span[i] = Bc6EncodingHelpers.PreQuantizeRawEndpoint(floats[i], signed);
	// 		}
	// 		return output;
	// 	}
	//
	// }

	[StructLayout(LayoutKind.Sequential)]
	internal struct RawBlock4X4YcbcrAlpha
	{
		public ColorYCbCrAlpha p00, p10, p20, p30;
		public ColorYCbCrAlpha p01, p11, p21, p31;
		public ColorYCbCrAlpha p02, p12, p22, p32;
		public ColorYCbCrAlpha p03, p13, p23, p33;
		public Span<ColorYCbCrAlpha> AsSpan => MemoryMarshal.CreateSpan(ref p00, 16);

		public ColorYCbCrAlpha this[int x, int y]
		{
			get => AsSpan[x + y * 4];
			set => AsSpan[x + y * 4] = value;
		}

		public float CalculateError(RawBlock4X4YcbcrAlpha other, float yErrorWeight, float cbErrorWeight, float crErrorWeight, float aErrorWeight)
		{
			float yError = 0;
			float cbError = 0;
			float crError = 0;
			float aError = 0;

			var pix1 = AsSpan;
			var pix2 = other.AsSpan;

			for (var i = 0; i < pix1.Length; i++)
			{
				var col1 = pix1[i];
				var col2 = pix2[i];

				var ye = col1.y - col2.y;
				var cbe = col1.cb - col2.cb;
				var cre = col1.cr - col2.cr;
				var ae = col1.a - col2.a;

				yError += ye * ye;
				cbError += cbe * cbe;
				crError += cre * cre;
				aError += ae * ae;
			}

			var error = yError * yErrorWeight + cbError * cbErrorWeight + crError * crErrorWeight + aError * aErrorWeight;

			return error;
		}
	}
}
