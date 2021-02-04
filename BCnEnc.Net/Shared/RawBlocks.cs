using System;
using System.Runtime.InteropServices;

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
