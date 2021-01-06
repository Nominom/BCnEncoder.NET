using System;
using System.Runtime.InteropServices;

namespace BCnEncoder.Shared
{

	internal struct RawBlock4X4Rgba32
	{
		public Rgba32 p00, p10, p20, p30;
		public Rgba32 p01, p11, p21, p31;
		public Rgba32 p02, p12, p22, p32;
		public Rgba32 p03, p13, p23, p33;

		public Rgba32[] AsArray
		{
			get
			{
				return new[]
				{
				p00, p10, p20, p30,
				p01, p11, p21, p31,
				p02, p12, p22, p32,
				p03, p13, p23, p33,
				};
			}
		}

		public Rgba32 this[int x, int y]
		{
			get
			{
				switch (x)
				{
					case 0:
						switch (y)
						{
							case 0: return p00;
							case 1: return p01;
							case 2: return p02;
							case 3: return p03;
						}
						break;
					case 1:
						switch (y)
						{
							case 0: return p10;
							case 1: return p11;
							case 2: return p12;
							case 3: return p13;
						}
						break;
					case 2:
						switch (y)
						{
							case 0: return p20;
							case 1: return p21;
							case 2: return p22;
							case 3: return p23;
						}
						break;
					case 3:
						switch (y)
						{
							case 0: return p30;
							case 1: return p31;
							case 2: return p32;
							case 3: return p33;
						}
						break;
				}
				return default;
			}
			set
			{
				switch (x)
				{
					case 0:
						switch (y)
						{
							case 0: p00 = value; break;
							case 1: p01 = value; break;
							case 2: p02 = value; break;
							case 3: p03 = value; break;
						}
						break;
					case 1:
						switch (y)
						{
							case 0: p10 = value; break;
							case 1: p11 = value; break;
							case 2: p12 = value; break;
							case 3: p13 = value; break;
						}
						break;
					case 2:
						switch (y)
						{
							case 0: p20 = value; break;
							case 1: p21 = value; break;
							case 2: p22 = value; break;
							case 3: p23 = value; break;
						}
						break;
					case 3:
						switch (y)
						{
							case 0: p30 = value; break;
							case 1: p31 = value; break;
							case 2: p32 = value; break;
							case 3: p33 = value; break;
						}
						break;
				}
			}
		}

		public Rgba32 this[int index]
		{
			get => this[index % 4, (int)Math.Floor(index / 4.0)];
			set => this[index % 4, (int)Math.Floor(index / 4.0)] = value;
		}

		public int CalculateError(RawBlock4X4Rgba32 other, bool useAlpha = false)
		{
			float error = 0;
			var pix1 = AsArray;
			var pix2 = other.AsArray;
			for (int i = 0; i < pix1.Length; i++)
			{
				var col1 = pix1[i];
				var col2 = pix2[i];

				var re = col1.R - col2.R;
				var ge = col1.G - col2.G;
				var be = col1.B - col2.B;

				error += re * re;
				error += ge * ge;
				error += be * be;

				if (useAlpha)
				{
					var ae = col1.A - col2.A;
					error += ae * ae * 4;
				}
			}

			error /= pix1.Length;
			error = (float)Math.Sqrt(error);

			return (int)error;
		}

		public float CalculateYCbCrError(RawBlock4X4Rgba32 other)
		{
			float yError = 0;
			float cbError = 0;
			float crError = 0;
			var pix1 = AsArray;
			var pix2 = other.AsArray;
			for (int i = 0; i < pix1.Length; i++)
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

		public float CalculateYCbCrAlphaError(RawBlock4X4Rgba32 other, float yMultiplier = 2, float alphaMultiplier = 1)
		{
			float yError = 0;
			float cbError = 0;
			float crError = 0;
			float alphaError = 0;
			var pix1 = AsArray;
			var pix2 = other.AsArray;
			for (int i = 0; i < pix1.Length; i++)
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

		public RawBlock4X4Ycbcr ToRawBlockYcbcr()
		{
			RawBlock4X4Ycbcr rawYcbcr = new RawBlock4X4Ycbcr();
			var pixels = AsArray;
			var ycbcrPs = rawYcbcr.AsArray;
			for (int i = 0; i < pixels.Length; i++)
			{
				ycbcrPs[i] = new ColorYCbCr(pixels[i]);
			}
			return rawYcbcr;
		}

		public bool HasTransparentPixels()
		{
			var pixels = AsArray;
			for (int i = 0; i < pixels.Length; i++)
			{
				if (pixels[i].A < 255) return true;
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

		public ColorYCbCr[] AsArray
		{
			get
			{
				return new[]
				{
					p00, p10, p20, p30,
					p01, p11, p21, p31,
					p02, p12, p22, p32,
					p03, p13, p23, p33,
				};
			}
		}

		public ColorYCbCr this[int x, int y]
		{
			get
			{
				switch (x)
				{
					case 0:
						switch (y)
						{
							case 0: return p00;
							case 1: return p01;
							case 2: return p02;
							case 3: return p03;
						}
						break;
					case 1:
						switch (y)
						{
							case 0: return p10;
							case 1: return p11;
							case 2: return p12;
							case 3: return p13;
						}
						break;
					case 2:
						switch (y)
						{
							case 0: return p20;
							case 1: return p21;
							case 2: return p22;
							case 3: return p23;
						}
						break;
					case 3:
						switch (y)
						{
							case 0: return p30;
							case 1: return p31;
							case 2: return p32;
							case 3: return p33;
						}
						break;
				}
				return default;
			}
			set
			{
				switch (x)
				{
					case 0:
						switch (y)
						{
							case 0: p00 = value; break;
							case 1: p01 = value; break;
							case 2: p02 = value; break;
							case 3: p03 = value; break;
						}
						break;
					case 1:
						switch (y)
						{
							case 0: p10 = value; break;
							case 1: p11 = value; break;
							case 2: p12 = value; break;
							case 3: p13 = value; break;
						}
						break;
					case 2:
						switch (y)
						{
							case 0: p20 = value; break;
							case 1: p21 = value; break;
							case 2: p22 = value; break;
							case 3: p23 = value; break;
						}
						break;
					case 3:
						switch (y)
						{
							case 0: p30 = value; break;
							case 1: p31 = value; break;
							case 2: p32 = value; break;
							case 3: p33 = value; break;
						}
						break;
				}
			}
		}

		public ColorYCbCr this[int index]
		{
			get => this[index % 4, (int)Math.Floor(index / 4.0)];
			set => this[index % 4, (int)Math.Floor(index / 4.0)] = value;
		}

		public float CalculateError(RawBlock4X4Rgba32 other)
		{
			float yError = 0;
			float cbError = 0;
			float crError = 0;
			var pix1 = AsArray;
			var pix2 = other.AsArray;
			for (int i = 0; i < pix1.Length; i++)
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
