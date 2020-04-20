using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Shared
{

	internal struct RawBlock4X4Rgba32 {
		public Rgba32 p00, p10, p20, p30;
		public Rgba32 p01, p11, p21, p31;
		public Rgba32 p02, p12, p22, p32;
		public Rgba32 p03, p13, p23, p33;
		public Span<Rgba32> AsSpan => MemoryMarshal.CreateSpan(ref p00, 16);

		public Rgba32 this[int x, int y] {
			get => AsSpan[x + y * 4];
			set => AsSpan[x + y * 4] = value;
		}

		public Rgba32 this[int index] {
			get => AsSpan[index];
			set => AsSpan[index] = value;
		}

		public int CalculateError(RawBlock4X4Rgba32 other, bool useAlpha = false) {
			float error = 0;
			var pix1 = AsSpan;
			var pix2 = other.AsSpan;
			for (int i = 0; i < pix1.Length; i++) {
				var col1 = pix1[i];
				var col2 = pix2[i];

				var re = col1.R - col2.R;
				var ge = col1.G - col2.G;
				var be = col1.B - col2.B;

				error += re * re;
				error += ge * ge;
				error += be * be;

				if (useAlpha) {
					var ae = col1.A - col2.A;
					error += ae * ae * 4;
				}
			}

			error /= pix1.Length;
			error = MathF.Sqrt(error);

			return (int)error;
		}

		public float CalculateYCbCrError(RawBlock4X4Rgba32 other) {
			float yError = 0;
			float cbError = 0;
			float crError = 0;
			var pix1 = AsSpan;
			var pix2 = other.AsSpan;
			for (int i = 0; i < pix1.Length; i++) {
				var col1 = new ColorYCbCr(pix1[i]);
				var col2 = new ColorYCbCr(pix2[i]);

				var ye = col1.y - col2.y;
				var cbe = col1.cb - col2.cb;
				var cre = col1.cr - col2.cr;

				yError += ye * ye;
				cbError += cbe * cbe;
				crError += cre * cre;
			}

			yError /= 16;
			cbError /= 16;
			crError /= 16;

			yError = MathF.Sqrt(yError);
			cbError = MathF.Sqrt(cbError);
			crError = MathF.Sqrt(crError);

			var error = yError * 2 + cbError / 2 + crError / 2;
			if (error > 1) {
				error = 1;
			}
			return error;
		}

		public float CalculateYCbCrAlphaError(RawBlock4X4Rgba32 other, float yMultiplier = 2, float alphaMultiplier = 1) {
			float yError = 0;
			float cbError = 0;
			float crError = 0;
			float alphaError = 0;
			var pix1 = AsSpan;
			var pix2 = other.AsSpan;
			for (int i = 0; i < pix1.Length; i++) {
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

			yError /= 16;
			cbError /= 16;
			crError /= 16;
			alphaError /= 16;

			var error = yError + cbError + crError + alphaError;
			return error;
		}

		public RawBlock4X4Ycbcr ToRawBlockYcbcr() {
			RawBlock4X4Ycbcr rawYcbcr = new RawBlock4X4Ycbcr();
			var pixels = AsSpan;
			var ycbcrPs = rawYcbcr.AsSpan;
			for (int i = 0; i < pixels.Length; i++) {
				ycbcrPs[i] = new ColorYCbCr(pixels[i]);
			}
			return rawYcbcr;
		}

		public bool HasTransparentPixels() {
			var pixels = AsSpan;
			for (int i = 0; i < pixels.Length; i++) {
				if (pixels[i].A < 255) return true;
			}
			return false;
		}
	}


	internal struct RawBlock4X4Ycbcr {
		public ColorYCbCr p00, p10, p20, p30;
		public ColorYCbCr p01, p11, p21, p31;
		public ColorYCbCr p02, p12, p22, p32;
		public ColorYCbCr p03, p13, p23, p33;
		public Span<ColorYCbCr> AsSpan => MemoryMarshal.CreateSpan(ref p00, 16);

		public ColorYCbCr this[int x, int y] {
			get => AsSpan[x + y * 4];
			set => AsSpan[x + y * 4] = value;
		}

		public float CalculateError(RawBlock4X4Rgba32 other) {
			float yError = 0;
			float cbError = 0;
			float crError = 0;
			var pix1 = AsSpan;
			var pix2 = other.AsSpan;
			for (int i = 0; i < pix1.Length; i++) {
				var col1 = pix1[i];
				var col2 = new ColorYCbCr(pix2[i]);

				var ye = col1.y - col2.y;
				var cbe = col1.cb - col2.cb;
				var cre = col1.cr - col2.cr;

				yError += ye * ye;
				cbError += cbe * cbe;
				crError += cre * cre;
			}

			yError /= 16;
			cbError /= 16;
			crError /= 16;

			yError = MathF.Sqrt(yError);
			cbError = MathF.Sqrt(cbError);
			crError = MathF.Sqrt(crError);

			var error = yError * 2 + cbError / 2 + crError / 2;
			if (error > 1) {
				error = 1;
			}
			return error;
		}
	}
}
