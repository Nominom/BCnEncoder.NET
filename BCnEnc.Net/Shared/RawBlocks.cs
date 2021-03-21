using System;
using System.Runtime.InteropServices;
using BCnEncoder.Encoder.Bptc;

namespace BCnEncoder.Shared
{

	public struct RawBlock4X4Rgba32
	{
		public ColorRgba32 p00, p10, p20, p30;
		public ColorRgba32 p01, p11, p21, p31;
		public ColorRgba32 p02, p12, p22, p32;
		public ColorRgba32 p03, p13, p23, p33;
		public Span<ColorRgba32> AsSpan => MemoryMarshal.CreateSpan(ref p00, 16);

		public ColorRgba32 this[int x, int y]
		{
			get => AsSpan[x + y * 4];
			set => AsSpan[x + y * 4] = value;
		}

		public ColorRgba32 this[int index]
		{
			get => AsSpan[index];
			set => AsSpan[index] = value;
		}

		internal int CalculateError(RawBlock4X4Rgba32 other, bool useAlpha = false)
		{
			float error = 0;
			var pix1 = AsSpan;
			var pix2 = other.AsSpan;
			for (var i = 0; i < pix1.Length; i++)
			{
				var col1 = pix1[i];
				var col2 = pix2[i];

				var re = col1.r - col2.r;
				var ge = col1.g - col2.g;
				var be = col1.b - col2.b;

				error += re * re;
				error += ge * ge;
				error += be * be;

				if (useAlpha)
				{
					var ae = col1.a - col2.a;
					error += ae * ae * 4;
				}
			}

			error /= pix1.Length;
			error = MathF.Sqrt(error);

			return (int)error;
		}

		internal float CalculateYCbCrError(RawBlock4X4Rgba32 other)
		{
			float yError = 0;
			float cbError = 0;
			float crError = 0;
			var pix1 = AsSpan;
			var pix2 = other.AsSpan;
			for (var i = 0; i < pix1.Length; i++)
			{
				var col1 = new ColorYCbCr(pix1[i]);
				var col2 = new ColorYCbCr(pix2[i]);

				var ye = col1.y - col2.y;
				var cbe = col1.cb - col2.cb;
				var cre = col1.cr - col2.cr;

				yError += ye * ye;
				cbError += cbe * cbe;
				crError += cre * cre;
			}

			var error = yError * 2 + cbError / 2 + crError / 2;

			return error;
		}

		internal float CalculateYCbCrAlphaError(RawBlock4X4Rgba32 other, float yMultiplier = 2, float alphaMultiplier = 1)
		{
			float yError = 0;
			float cbError = 0;
			float crError = 0;
			float alphaError = 0;
			var pix1 = AsSpan;
			var pix2 = other.AsSpan;
			for (var i = 0; i < pix1.Length; i++)
			{
				var col1 = new ColorYCbCrAlpha(pix1[i]);
				var col2 = new ColorYCbCrAlpha(pix2[i]);

				var ye = (col1.y - col2.y) * yMultiplier;
				var cbe = col1.cb - col2.cb;
				var cre = col1.cr - col2.cr;
				var ae = (col1.alpha - col2.alpha) * alphaMultiplier;

				yError += ye * ye;
				cbError += cbe * cbe;
				crError += cre * cre;
				alphaError += ae * ae;
			}

			var error = yError + cbError + crError + alphaError;
			return error;
		}

		internal RawBlock4X4Ycbcr ToRawBlockYcbcr()
		{
			var rawYcbcr = new RawBlock4X4Ycbcr();
			var pixels = AsSpan;
			var ycbcrPs = rawYcbcr.AsSpan;
			for (var i = 0; i < pixels.Length; i++)
			{
				ycbcrPs[i] = new ColorYCbCr(pixels[i]);
			}
			return rawYcbcr;
		}

		public bool HasTransparentPixels()
		{
			var pixels = AsSpan;
			for (var i = 0; i < pixels.Length; i++)
			{
				if (pixels[i].a < 255) return true;
			}
			return false;
		}
	}

	public struct RawBlock4X4RgbFloat
	{
		public ColorRgbFloat p00, p10, p20, p30;
		public ColorRgbFloat p01, p11, p21, p31;
		public ColorRgbFloat p02, p12, p22, p32;
		public ColorRgbFloat p03, p13, p23, p33;
		public Span<ColorRgbFloat> AsSpan => MemoryMarshal.CreateSpan(ref p00, 16);

		public ColorRgbFloat this[int x, int y]
		{
			get => AsSpan[x + y * 4];
			set => AsSpan[x + y * 4] = value;
		}

		public ColorRgbFloat this[int index]
		{
			get => AsSpan[index];
			set => AsSpan[index] = value;
		}

		internal float CalculateError(RawBlock4X4RgbFloat other)
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
																	   
				error += re * re;
				error += ge * ge;
				error += be * be;

			}

			error /= pix1.Length;
			error = MathF.Sqrt(error);

			return error;
		}

		internal float CalculateYCbCrError(RawBlock4X4RgbFloat other)
		{
			float yError = 0;
			float cbError = 0;
			float crError = 0;
			var pix1 = AsSpan;
			var pix2 = other.AsSpan;
			for (var i = 0; i < pix1.Length; i++)
			{
				var col1 = new ColorYCbCr(pix1[i]);
				var col2 = new ColorYCbCr(pix2[i]);

				var ye = col1.y - col2.y;
				var cbe = col1.cb - col2.cb;
				var cre = col1.cr - col2.cr;

				yError += ye * ye;
				cbError += cbe * cbe;
				crError += cre * cre;
			}

			var error = yError * 2 + cbError / 2 + crError / 2;

			return error;
		}
		

		internal RawBlock4X4Ycbcr ToRawBlockYcbcr()
		{
			var rawYcbcr = new RawBlock4X4Ycbcr();
			var pixels = AsSpan;
			var ycbcrPs = rawYcbcr.AsSpan;
			for (var i = 0; i < pixels.Length; i++)
			{
				ycbcrPs[i] = new ColorYCbCr(pixels[i]);
			}
			return rawYcbcr;
		}
	}

	//Used for Bc6H
	internal struct RawBlock4X4RgbHalfInt
	{
		public (int, int, int) p00, p10, p20, p30;
		public (int, int, int) p01, p11, p21, p31;
		public (int, int, int) p02, p12, p22, p32;
		public (int, int, int) p03, p13, p23, p33;
		public Span<(int, int, int)> AsSpan => MemoryMarshal.CreateSpan(ref p00, 16);

		public (int, int, int) this[int x, int y]
		{
			get => AsSpan[x + y * 4];
			set => AsSpan[x + y * 4] = value;
		}

		public (int, int, int) this[int index]
		{
			get => AsSpan[index];
			set => AsSpan[index] = value;
		}

		public static RawBlock4X4RgbHalfInt FromRawFloats(RawBlock4X4RgbFloat other, bool signed)
		{
			var output = new RawBlock4X4RgbHalfInt();
			var span = output.AsSpan;
			var floats = other.AsSpan;
			for (var i = 0; i < 16; i++)
			{
				span[i] = Bc6EncodingHelpers.PreQuantizeRawEndpoint(floats[i], signed);
			}
			return output;
		}

	}


	internal struct RawBlock4X4Ycbcr
	{
		public ColorYCbCr p00, p10, p20, p30;
		public ColorYCbCr p01, p11, p21, p31;
		public ColorYCbCr p02, p12, p22, p32;
		public ColorYCbCr p03, p13, p23, p33;
		public Span<ColorYCbCr> AsSpan => MemoryMarshal.CreateSpan(ref p00, 16);

		public ColorYCbCr this[int x, int y]
		{
			get => AsSpan[x + y * 4];
			set => AsSpan[x + y * 4] = value;
		}

		public float CalculateError(RawBlock4X4Rgba32 other)
		{
			float yError = 0;
			float cbError = 0;
			float crError = 0;
			var pix1 = AsSpan;
			var pix2 = other.AsSpan;
			for (var i = 0; i < pix1.Length; i++)
			{
				var col1 = pix1[i];
				var col2 = new ColorYCbCr(pix2[i]);

				var ye = col1.y - col2.y;
				var cbe = col1.cb - col2.cb;
				var cre = col1.cr - col2.cr;

				yError += ye * ye;
				cbError += cbe * cbe;
				crError += cre * cre;
			}

			var error = yError * 2 + cbError / 2 + crError / 2;

			return error;
		}
	}
}
