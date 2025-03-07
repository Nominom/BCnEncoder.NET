using System;
using System.Numerics;

namespace BCnEncoder.Shared
{
	public struct ColorRgba32 : IEquatable<ColorRgba32>
	{
		public byte r, g, b, a;

		public ColorRgba32(byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public ColorRgba32(byte r, byte g, byte b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = 255;
		}

		public bool Equals(ColorRgba32 other)
		{
			return r == other.r && g == other.g && b == other.b && a == other.a;
		}

		public override bool Equals(object obj)
		{
			return obj is ColorRgba32 other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = r.GetHashCode();
				hashCode = (hashCode * 397) ^ g.GetHashCode();
				hashCode = (hashCode * 397) ^ b.GetHashCode();
				hashCode = (hashCode * 397) ^ a.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(ColorRgba32 left, ColorRgba32 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ColorRgba32 left, ColorRgba32 right)
		{
			return !left.Equals(right);
		}

		public static ColorRgba32 operator +(ColorRgba32 left, ColorRgba32 right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r + right.r),
				ByteHelper.ClampToByte(left.g + right.g),
				ByteHelper.ClampToByte(left.b + right.b),
				ByteHelper.ClampToByte(left.a + right.a));
		}

		public static ColorRgba32 operator -(ColorRgba32 left, ColorRgba32 right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r - right.r),
				ByteHelper.ClampToByte(left.g - right.g),
				ByteHelper.ClampToByte(left.b - right.b),
				ByteHelper.ClampToByte(left.a - right.a));
		}

		public static ColorRgba32 operator /(ColorRgba32 left, double right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte((int)(left.r / right)),
				ByteHelper.ClampToByte((int)(left.g / right)),
				ByteHelper.ClampToByte((int)(left.b / right)),
				ByteHelper.ClampToByte((int)(left.a / right))
			);
		}

		public static ColorRgba32 operator *(ColorRgba32 left, double right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte((int)(left.r * right)),
				ByteHelper.ClampToByte((int)(left.g * right)),
				ByteHelper.ClampToByte((int)(left.b * right)),
				ByteHelper.ClampToByte((int)(left.a * right))
			);
		}

		/// <summary>
		/// Component-wise left shift
		/// </summary>
		public static ColorRgba32 operator <<(ColorRgba32 left, int right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r << right),
				ByteHelper.ClampToByte(left.g << right),
				ByteHelper.ClampToByte(left.b << right),
				ByteHelper.ClampToByte(left.a << right)
			);
		}

		/// <summary>
		/// Component-wise right shift
		/// </summary>
		public static ColorRgba32 operator >>(ColorRgba32 left, int right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r >> right),
				ByteHelper.ClampToByte(left.g >> right),
				ByteHelper.ClampToByte(left.b >> right),
				ByteHelper.ClampToByte(left.a >> right)
			);
		}

		/// <summary>
		/// Component-wise bitwise OR operation
		/// </summary>
		public static ColorRgba32 operator |(ColorRgba32 left, ColorRgba32 right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r | right.r),
				ByteHelper.ClampToByte(left.g | right.g),
				ByteHelper.ClampToByte(left.b | right.b),
				ByteHelper.ClampToByte(left.a | right.a)
			);
		}

		/// <summary>
		/// Component-wise bitwise OR operation
		/// </summary>
		public static ColorRgba32 operator |(ColorRgba32 left, int right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r | right),
				ByteHelper.ClampToByte(left.g | right),
				ByteHelper.ClampToByte(left.b | right),
				ByteHelper.ClampToByte(left.a | right)
			);
		}

		/// <summary>
		/// Component-wise bitwise AND operation
		/// </summary>
		public static ColorRgba32 operator &(ColorRgba32 left, ColorRgba32 right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r & right.r),
				ByteHelper.ClampToByte(left.g & right.g),
				ByteHelper.ClampToByte(left.b & right.b),
				ByteHelper.ClampToByte(left.a & right.a)
			);
		}

		/// <summary>
		/// Component-wise bitwise AND operation
		/// </summary>
		public static ColorRgba32 operator &(ColorRgba32 left, int right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r & right),
				ByteHelper.ClampToByte(left.g & right),
				ByteHelper.ClampToByte(left.b & right),
				ByteHelper.ClampToByte(left.a & right)
			);
		}

		public override string ToString()
		{
			return $"r : {r} g : {g} b : {b} a : {a}";
		}

		internal readonly ColorRgbaFloat ToFloat()
		{
			return new ColorRgbaFloat(this);
		}

		public readonly ColorRgbFloat ToRgbFloat()
		{
			return new ColorRgbFloat(this);
		}
	}

	internal struct ColorRgbaFloat : IEquatable<ColorRgbaFloat>
	{
		public float r, g, b, a;

		public ColorRgbaFloat(float r, float g, float b, float a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public ColorRgbaFloat(ColorRgba32 other)
		{
			this.r = other.r / 255f;
			this.g = other.g / 255f;
			this.b = other.b / 255f;
			this.a = other.a / 255f;
		}

		public ColorRgbaFloat(float r, float g, float b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = 1;
		}

		public bool Equals(ColorRgbaFloat other)
		{
			return r == other.r && g == other.g && b == other.b && a == other.a;
		}

		public override bool Equals(object obj)
		{
			return obj is ColorRgbaFloat other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = r.GetHashCode();
				hashCode = (hashCode * 397) ^ g.GetHashCode();
				hashCode = (hashCode * 397) ^ b.GetHashCode();
				hashCode = (hashCode * 397) ^ a.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(ColorRgbaFloat left, ColorRgbaFloat right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ColorRgbaFloat left, ColorRgbaFloat right)
		{
			return !left.Equals(right);
		}

		public static ColorRgbaFloat operator +(ColorRgbaFloat left, ColorRgbaFloat right)
		{
			return new ColorRgbaFloat(
				left.r + right.r,
				left.g + right.g,
				left.b + right.b,
				left.a + right.a);
		}

		public static ColorRgbaFloat operator -(ColorRgbaFloat left, ColorRgbaFloat right)
		{
			return new ColorRgbaFloat(
				left.r - right.r,
				left.g - right.g,
				left.b - right.b,
				left.a - right.a);
		}

		public static ColorRgbaFloat operator /(ColorRgbaFloat left, float right)
		{
			return new ColorRgbaFloat(
				left.r / right,
				left.g / right,
				left.b / right,
				left.a / right
			);
		}

		public static ColorRgbaFloat operator *(ColorRgbaFloat left, float right)
		{
			return new ColorRgbaFloat(
				left.r * right,
				left.g * right,
				left.b * right,
				left.a * right
			);
		}

		public static ColorRgbaFloat operator *(float left, ColorRgbaFloat right)
		{
			return new ColorRgbaFloat(
				right.r * left,
				right.g * left,
				right.b * left,
				right.a * left
			);
		}

		public override string ToString()
		{
			return $"r : {r:0.00} g : {g:0.00} b : {b:0.00} a : {a:0.00}";
		}

		public ColorRgba32 ToRgba32()
		{
			return new ColorRgba32(
				(byte)(ByteHelper.ClampToByte(r * 255)),
				(byte)(ByteHelper.ClampToByte(g * 255)),
				(byte)(ByteHelper.ClampToByte(b * 255)),
				(byte)(ByteHelper.ClampToByte(a * 255))
				);
		}

	}
	
	public struct ColorRgbFloat : IEquatable<ColorRgbFloat>
	{
		public float r, g, b;

		public ColorRgbFloat(float r, float g, float b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

		public ColorRgbFloat(ColorRgba32 other)
		{
			this.r = other.r / 255f;
			this.g = other.g / 255f;
			this.b = other.b / 255f;
		}

		public ColorRgbFloat(Vector3 vector)
		{
			r = vector.X;
			g = vector.Y;
			b = vector.Z;
		}

		public bool Equals(ColorRgbFloat other)
		{
			return r == other.r && g == other.g && b == other.b;
		}

		public override bool Equals(object obj)
		{
			return obj is ColorRgbFloat other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = r.GetHashCode();
				hashCode = (hashCode * 397) ^ g.GetHashCode();
				hashCode = (hashCode * 397) ^ b.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(ColorRgbFloat left, ColorRgbFloat right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ColorRgbFloat left, ColorRgbFloat right)
		{
			return !left.Equals(right);
		}

		public static ColorRgbFloat operator +(ColorRgbFloat left, ColorRgbFloat right)
		{
			return new ColorRgbFloat(
				left.r + right.r,
				left.g + right.g,
				left.b + right.b);
		}

		public static ColorRgbFloat operator -(ColorRgbFloat left, ColorRgbFloat right)
		{
			return new ColorRgbFloat(
				left.r - right.r,
				left.g - right.g,
				left.b - right.b);
		}

		public static ColorRgbFloat operator /(ColorRgbFloat left, float right)
		{
			return new ColorRgbFloat(
				left.r / right,
				left.g / right,
				left.b / right
			);
		}

		public static ColorRgbFloat operator *(ColorRgbFloat left, float right)
		{
			return new ColorRgbFloat(
				left.r * right,
				left.g * right,
				left.b * right
			);
		}

		public static ColorRgbFloat operator *(float left, ColorRgbFloat right)
		{
			return new ColorRgbFloat(
				right.r * left,
				right.g * left,
				right.b * left
			);
		}

		public override string ToString()
		{
			return $"r : {r:0.00} g : {g:0.00} b : {b:0.00}";
		}

		public ColorRgba32 ToRgba32()
		{
			return new ColorRgba32(
				(byte)(ByteHelper.ClampToByte(r * 255)),
				(byte)(ByteHelper.ClampToByte(g * 255)),
				(byte)(ByteHelper.ClampToByte(b * 255)),
				255
				);
		}

		public Vector3 ToVector3()
		{
			return new Vector3(r, g, b);
		}

		internal float CalcLogDist(ColorRgbFloat other)
		{
			var dr = Math.Sign(other.r) * MathF.Log(1 + MathF.Abs(other.r)) - Math.Sign(r) * MathF.Log(1 + MathF.Abs(r));
			var dg = Math.Sign(other.g) * MathF.Log(1 + MathF.Abs(other.g)) - Math.Sign(g) * MathF.Log(1 + MathF.Abs(g));
			var db = Math.Sign(other.b) * MathF.Log(1 + MathF.Abs(other.b)) - Math.Sign(b) * MathF.Log(1 + MathF.Abs(b));
			return MathF.Sqrt((dr * dr) + (dg * dg) + (db * db));
		}

		internal float CalcDist(ColorRgbFloat other)
		{
			var dr = other.r - r;
			var dg = other.g - g;
			var db = other.b - b;
			return MathF.Sqrt((dr * dr) + (dg * dg) + (db * db));
		}

		internal void ClampToPositive()
		{
			if (r < 0) r = 0;
			if (g < 0) g = 0;
			if (b < 0) b = 0;
		}

		internal void ClampToHalf()
		{
			if (r < Half.MinValue) r = Half.MinValue;
			else if (g > Half.MaxValue) g = Half.MaxValue;
			if (b < Half.MinValue) b = Half.MinValue;
			else if (r > Half.MaxValue) r = Half.MaxValue;
			if (g < Half.MinValue) g = Half.MinValue;
			else if (b > Half.MaxValue) b = Half.MaxValue;
		}
	}

	internal struct ColorYCbCr
	{
		public float y;
		public float cb;
		public float cr;

		public ColorYCbCr(float y, float cb, float cr)
		{
			this.y = y;
			this.cb = cb;
			this.cr = cr;
		}

		internal ColorYCbCr(ColorRgb24 rgb)
		{
			var fr = (float)rgb.r / 255;
			var fg = (float)rgb.g / 255;
			var fb = (float)rgb.b / 255;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
		}

		internal ColorYCbCr(ColorRgbaFloat rgb)
		{
			var fr = rgb.r;
			var fg = rgb.g;
			var fb = rgb.b;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
		}

		internal ColorYCbCr(ColorRgbFloat rgb)
		{
			var fr = rgb.r;
			var fg = rgb.g;
			var fb = rgb.b;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
		}

		internal ColorYCbCr(ColorRgb565 rgb)
		{
			var fr = (float)rgb.R / 255;
			var fg = (float)rgb.G / 255;
			var fb = (float)rgb.B / 255;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
		}

		public ColorYCbCr(ColorRgba32 rgba)
		{
			var fr = (float)rgba.r / 255;
			var fg = (float)rgba.g / 255;
			var fb = (float)rgba.b / 255;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
		}

		public ColorYCbCr(Vector3 vec)
		{
			var fr = (float)vec.X;
			var fg = (float)vec.Y;
			var fb = (float)vec.Z;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
		}

		public ColorRgb565 ToColorRgb565()
		{
			var r = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 0.0000 * cb + 1.4022 * cr)));
			var g = Math.Max(0.0f, Math.Min(1.0f, (float)(y - 0.3456 * cb - 0.7145 * cr)));
			var b = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 1.7710 * cb + 0.0000 * cr)));

			return new ColorRgb565((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
		}

		public ColorRgba32 ToColorRgba32()
		{
			var r = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 0.0000 * cb + 1.4022 * cr)));
			var g = Math.Max(0.0f, Math.Min(1.0f, (float)(y - 0.3456 * cb - 0.7145 * cr)));
			var b = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 1.7710 * cb + 0.0000 * cr)));

			return new ColorRgba32((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), 255);
		}

		public override string ToString()
		{
			var r = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 0.0000 * cb + 1.4022 * cr)));
			var g = Math.Max(0.0f, Math.Min(1.0f, (float)(y - 0.3456 * cb - 0.7145 * cr)));
			var b = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 1.7710 * cb + 0.0000 * cr)));

			return $"r : {r * 255} g : {g * 255} b : {b * 255}";
		}

		public float CalcDistWeighted(ColorYCbCr other, float yWeight = 4)
		{
			var dy = (y - other.y) * (y - other.y) * yWeight;
			var dcb = (cb - other.cb) * (cb - other.cb);
			var dcr = (cr - other.cr) * (cr - other.cr);

			return MathF.Sqrt(dy + dcb + dcr);
		}

		public static ColorYCbCr operator +(ColorYCbCr left, ColorYCbCr right)
		{
			return new ColorYCbCr(
				left.y + right.y,
				left.cb + right.cb,
				left.cr + right.cr);
		}

		public static ColorYCbCr operator /(ColorYCbCr left, float right)
		{
			return new ColorYCbCr(
				left.y / right,
				left.cb / right,
				left.cr / right);
		}
	}

	internal struct ColorRgb555 : IEquatable<ColorRgb555>
	{
		public bool Equals(ColorRgb555 other)
		{
			return data == other.data;
		}

		public override bool Equals(object obj)
		{
			return obj is ColorRgb555 other && Equals(other);
		}

		public override int GetHashCode()
		{
			return data.GetHashCode();
		}

		public static bool operator ==(ColorRgb555 left, ColorRgb555 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ColorRgb555 left, ColorRgb555 right)
		{
			return !left.Equals(right);
		}

		private const ushort ModeMask = 0b1_00000_00000_00000;
		private const int ModeShift = 15;
		private const ushort RedMask = 0b0_11111_00000_00000;
		private const int RedShift = 10;
		private const ushort GreenMask = 0b0_00000_11111_00000;
		private const int GreenShift = 5;
		private const ushort BlueMask = 0b0_00000_00000_11111;

		public ushort data;

		public byte Mode
		{
			readonly get
			{
				var mode = (data & ModeMask) >> ModeShift;
				return (byte)mode;
			}
			set
			{
				var mode = value;
				data = (ushort)(data & ~ModeMask);
				data = (ushort)(data | (mode << ModeShift));
			}
		}

		public byte R
		{
			readonly get
			{
				var r5 = (data & RedMask) >> RedShift;
				return (byte)((r5 << 3) | (r5 >> 2));
			}
			set
			{
				var r5 = value >> 3;
				data = (ushort)(data & ~RedMask);
				data = (ushort)(data | (r5 << RedShift));
			}
		}

		public byte G
		{
			readonly get
			{
				var g5 = (data & GreenMask) >> GreenShift;
				return (byte)((g5 << 3) | (g5 >> 2));
			}
			set
			{
				var g5 = value >> 3;
				data = (ushort)(data & ~GreenMask);
				data = (ushort)(data | (g5 << GreenShift));
			}
		}

		public byte B
		{
			readonly get
			{
				var b5 = data & BlueMask;
				return (byte)((b5 << 3) | (b5 >> 2));
			}
			set
			{
				var b5 = value >> 3;
				data = (ushort)(data & ~BlueMask);
				data = (ushort)(data | b5);
			}
		}

		public int RawR
		{
			readonly get => (data & RedMask) >> RedShift;
			set
			{
				if (value > 31) value = 31;
				if (value < 0) value = 0;
				data = (ushort)(data & ~RedMask);
				data = (ushort)(data | (value << RedShift));
			}
		}

		public int RawG
		{
			readonly get => (data & GreenMask) >> GreenShift;
			set
			{
				if (value > 31) value = 31;
				if (value < 0) value = 0;
				data = (ushort)(data & ~GreenMask);
				data = (ushort)(data | (value << GreenShift));
			}
		}

		public int RawB
		{
			readonly get => data & BlueMask;
			set
			{
				if (value > 31) value = 31;
				if (value < 0) value = 0;
				data = (ushort)(data & ~BlueMask);
				data = (ushort)(data | value);
			}
		}

		public ColorRgb555(byte r, byte g, byte b)
		{
			data = 0;
			R = r;
			G = g;
			B = b;
		}

		public ColorRgb555(Vector3 colorVector)
		{
			data = 0;
			R = ByteHelper.ClampToByte(colorVector.X * 255);
			G = ByteHelper.ClampToByte(colorVector.Y * 255);
			B = ByteHelper.ClampToByte(colorVector.Z * 255);
		}

		public ColorRgb555(ColorRgb24 color)
		{
			data = 0;
			R = color.r;
			G = color.g;
			B = color.b;
		}

		public readonly ColorRgb24 ToColorRgb24()
		{
			return new ColorRgb24(R, G, B);
		}

		public override string ToString()
		{
			return $"r : {R} g : {G} b : {B}";
		}

		public ColorRgba32 ToColorRgba32()
		{
			return new ColorRgba32(R, G, B, 255);
		}
	}

	internal struct ColorRgb565 : IEquatable<ColorRgb565>
	{
		public bool Equals(ColorRgb565 other)
		{
			return data == other.data;
		}

		public override bool Equals(object obj)
		{
			return obj is ColorRgb565 other && Equals(other);
		}

		public override int GetHashCode()
		{
			return data.GetHashCode();
		}

		public static bool operator ==(ColorRgb565 left, ColorRgb565 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ColorRgb565 left, ColorRgb565 right)
		{
			return !left.Equals(right);
		}

		private const ushort RedMask = 0b11111_000000_00000;
		private const int RedShift = 11;
		private const ushort GreenMask = 0b00000_111111_00000;
		private const int GreenShift = 5;
		private const ushort BlueMask = 0b00000_000000_11111;

		public ushort data;

		public byte R
		{
			readonly get
			{
				var r5 = (data & RedMask) >> RedShift;
				return (byte)((r5 << 3) | (r5 >> 2));
			}
			set
			{
				var r5 = value >> 3;
				data = (ushort)(data & ~RedMask);
				data = (ushort)(data | (r5 << RedShift));
			}
		}

		public byte G
		{
			readonly get
			{
				var g6 = (data & GreenMask) >> GreenShift;
				return (byte)((g6 << 2) | (g6 >> 4));
			}
			set
			{
				var g6 = value >> 2;
				data = (ushort)(data & ~GreenMask);
				data = (ushort)(data | (g6 << GreenShift));
			}
		}

		public byte B
		{
			readonly get
			{
				var b5 = data & BlueMask;
				return (byte)((b5 << 3) | (b5 >> 2));
			}
			set
			{
				var b5 = value >> 3;
				data = (ushort)(data & ~BlueMask);
				data = (ushort)(data | b5);
			}
		}

		public int RawR
		{
			readonly get { return (data & RedMask) >> RedShift; }
			set
			{
				if (value > 31) value = 31;
				if (value < 0) value = 0;
				data = (ushort)(data & ~RedMask);
				data = (ushort)(data | (value << RedShift));
			}
		}

		public int RawG
		{
			readonly get { return (data & GreenMask) >> GreenShift; }
			set
			{
				if (value > 63) value = 63;
				if (value < 0) value = 0;
				data = (ushort)(data & ~GreenMask);
				data = (ushort)(data | (value << GreenShift));
			}
		}

		public int RawB
		{
			readonly get { return data & BlueMask; }
			set
			{
				if (value > 31) value = 31;
				if (value < 0) value = 0;
				data = (ushort)(data & ~BlueMask);
				data = (ushort)(data | value);
			}
		}

		public ColorRgb565(byte r, byte g, byte b)
		{
			data = 0;
			R = r;
			G = g;
			B = b;
		}

		public ColorRgb565(Vector3 colorVector)
		{
			data = 0;
			R = ByteHelper.ClampToByte(colorVector.X * 255);
			G = ByteHelper.ClampToByte(colorVector.Y * 255);
			B = ByteHelper.ClampToByte(colorVector.Z * 255);
		}

		public ColorRgb565(ColorRgb24 color)
		{
			data = 0;
			R = color.r;
			G = color.g;
			B = color.b;
		}

		public readonly ColorRgb24 ToColorRgb24()
		{
			return new ColorRgb24(R, G, B);
		}

		public override string ToString()
		{
			return $"r : {R} g : {G} b : {B}";
		}

		public ColorRgba32 ToColorRgba32()
		{
			return new ColorRgba32(R, G, B, 255);
		}
	}

	internal struct ColorRgb24 : IEquatable<ColorRgb24>
	{
		public byte r, g, b;

		public ColorRgb24(byte r, byte g, byte b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

		public ColorRgb24(ColorRgb565 color)
		{
			this.r = color.R;
			this.g = color.G;
			this.b = color.B;
		}

		public ColorRgb24(ColorRgba32 color)
		{
			this.r = color.r;
			this.g = color.g;
			this.b = color.b;
		}

		public bool Equals(ColorRgb24 other)
		{
			return r == other.r && g == other.g && b == other.b;
		}

		public override bool Equals(object obj)
		{
			return obj is ColorRgb24 other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = r.GetHashCode();
				hashCode = (hashCode * 397) ^ g.GetHashCode();
				hashCode = (hashCode * 397) ^ b.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(ColorRgb24 left, ColorRgb24 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ColorRgb24 left, ColorRgb24 right)
		{
			return !left.Equals(right);
		}

		public static ColorRgb24 operator +(ColorRgb24 left, ColorRgb24 right)
		{
			return new ColorRgb24(
				ByteHelper.ClampToByte(left.r + right.r),
				ByteHelper.ClampToByte(left.g + right.g),
				ByteHelper.ClampToByte(left.b + right.b));
		}

		public static ColorRgb24 operator -(ColorRgb24 left, ColorRgb24 right)
		{
			return new ColorRgb24(
				ByteHelper.ClampToByte(left.r - right.r),
				ByteHelper.ClampToByte(left.g - right.g),
				ByteHelper.ClampToByte(left.b - right.b));
		}

		public static ColorRgb24 operator /(ColorRgb24 left, double right)
		{
			return new ColorRgb24(
				ByteHelper.ClampToByte((int)(left.r / right)),
				ByteHelper.ClampToByte((int)(left.g / right)),
				ByteHelper.ClampToByte((int)(left.b / right))
				);
		}

		public static ColorRgb24 operator *(ColorRgb24 left, double right)
		{
			return new ColorRgb24(
				ByteHelper.ClampToByte((int)(left.r * right)),
				ByteHelper.ClampToByte((int)(left.g * right)),
				ByteHelper.ClampToByte((int)(left.b * right))
			);
		}

		public override string ToString()
		{
			return $"r : {r} g : {g} b : {b}";
		}
	}

	internal struct ColorYCbCrAlpha
	{
		public float y;
		public float cb;
		public float cr;
		public float alpha;

		public ColorYCbCrAlpha(float y, float cb, float cr, float alpha)
		{
			this.y = y;
			this.cb = cb;
			this.cr = cr;
			this.alpha = alpha;
		}

		public ColorYCbCrAlpha(ColorRgb24 rgb)
		{
			var fr = (float)rgb.r / 255;
			var fg = (float)rgb.g / 255;
			var fb = (float)rgb.b / 255;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
			alpha = 1;
		}

		public ColorYCbCrAlpha(ColorRgb565 rgb)
		{
			var fr = (float)rgb.R / 255;
			var fg = (float)rgb.G / 255;
			var fb = (float)rgb.B / 255;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
			alpha = 1;
		}

		public ColorYCbCrAlpha(ColorRgba32 rgba)
		{
			var fr = (float)rgba.r / 255;
			var fg = (float)rgba.g / 255;
			var fb = (float)rgba.b / 255;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
			alpha = rgba.a / 255f;
		}

		public ColorYCbCrAlpha(ColorRgbaFloat rgba)
		{
			var fr = rgba.r;
			var fg = rgba.g;
			var fb = rgba.b;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
			alpha = rgba.a;
		}


		public ColorRgb565 ToColorRgb565()
		{
			var r = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 0.0000 * cb + 1.4022 * cr)));
			var g = Math.Max(0.0f, Math.Min(1.0f, (float)(y - 0.3456 * cb - 0.7145 * cr)));
			var b = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 1.7710 * cb + 0.0000 * cr)));

			return new ColorRgb565((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
		}

		public override string ToString()
		{
			var r = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 0.0000 * cb + 1.4022 * cr)));
			var g = Math.Max(0.0f, Math.Min(1.0f, (float)(y - 0.3456 * cb - 0.7145 * cr)));
			var b = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 1.7710 * cb + 0.0000 * cr)));

			return $"r : {r * 255} g : {g * 255} b : {b * 255}";
		}

		public float CalcDistWeighted(ColorYCbCrAlpha other, float yWeight = 4, float aWeight = 1)
		{
			var dy = (y - other.y) * (y - other.y) * yWeight;
			var dcb = (cb - other.cb) * (cb - other.cb);
			var dcr = (cr - other.cr) * (cr - other.cr);
			var da = (alpha - other.alpha) * (alpha - other.alpha) * aWeight;

			return MathF.Sqrt(dy + dcb + dcr + da);
		}

		public static ColorYCbCrAlpha operator +(ColorYCbCrAlpha left, ColorYCbCrAlpha right)
		{
			return new ColorYCbCrAlpha(
				left.y + right.y,
				left.cb + right.cb,
				left.cr + right.cr,
				left.alpha + right.alpha);
		}

		public static ColorYCbCrAlpha operator /(ColorYCbCrAlpha left, float right)
		{
			return new ColorYCbCrAlpha(
				left.y / right,
				left.cb / right,
				left.cr / right,
				left.alpha / right);
		}
	}

	internal struct ColorXyz
	{
		public float x;
		public float y;
		public float z;

		public ColorXyz(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public ColorXyz(ColorRgb24 color)
		{
			this = ColorToXyz(color);
		}

		public ColorXyz(ColorRgbFloat color)
		{
			this = ColorToXyz(color);
		}

		public ColorRgbFloat ToColorRgbFloat()
		{
			return new ColorRgbFloat(
				3.2404542f * x - 1.5371385f * y - 0.4985314f * z,
				-0.9692660f * x + 1.8760108f * y + 0.0415560f * z,
				0.0556434f * x - 0.2040259f * y + 1.0572252f * z
			);
		}

		public static ColorXyz ColorToXyz(ColorRgb24 color)
		{
			var r = PivotRgb(color.r / 255.0f);
			var g = PivotRgb(color.g / 255.0f);
			var b = PivotRgb(color.b / 255.0f);

			// Observer. = 2°, Illuminant = D65
			return new ColorXyz(r * 0.4124f + g * 0.3576f + b * 0.1805f, r * 0.2126f + g * 0.7152f + b * 0.0722f, r * 0.0193f + g * 0.1192f + b * 0.9505f);
		}

		public static ColorXyz ColorToXyz(ColorRgbFloat color)
		{
			var r = PivotRgb(color.r);
			var g = PivotRgb(color.g);
			var b = PivotRgb(color.b);

			// Observer. = 2°, Illuminant = D65
			return new ColorXyz(r * 0.4124f + g * 0.3576f + b * 0.1805f, r * 0.2126f + g * 0.7152f + b * 0.0722f, r * 0.0193f + g * 0.1192f + b * 0.9505f);
		}

		private static float PivotRgb(float n)
		{
			return (n > 0.04045f ? MathF.Pow((n + 0.055f) / 1.055f, 2.4f) : n / 12.92f) * 100;
		}
	}

	internal struct ColorLab
	{
		public float l;
		public float a;
		public float b;

		public ColorLab(float l, float a, float b)
		{
			this.l = l;
			this.a = a;
			this.b = b;
		}

		public ColorLab(ColorRgb24 color)
		{
			this = ColorToLab(color);
		}

		public ColorLab(ColorRgba32 color)
		{
			this = ColorToLab(new ColorRgb24(color.r, color.g, color.b));
		}

		public ColorLab(ColorRgbFloat color)
		{
			this = XyzToLab(new ColorXyz(color));
		}

		public static ColorLab ColorToLab(ColorRgb24 color)
		{
			var xyz = new ColorXyz(color);
			return XyzToLab(xyz);
		}


		public static ColorLab XyzToLab(ColorXyz xyz)
		{
			var refX = 95.047f; // Observer= 2°, Illuminant= D65
			var refY = 100.000f;
			var refZ = 108.883f;

			var x = PivotXyz(xyz.x / refX);
			var y = PivotXyz(xyz.y / refY);
			var z = PivotXyz(xyz.z / refZ);

			return new ColorLab(116 * y - 16, 500 * (x - y), 200 * (y - z));
		}

		private static float PivotXyz(float n)
		{
			var i = MathF.Cbrt(n);
			return n > 0.008856f ? i : 7.787f * n + 16 / 116f;
		}
	}

	internal struct ColorRgbe : IEquatable<ColorRgbe>
	{
		public byte r;
		public byte g;
		public byte b;
		public byte e;

		public ColorRgbe(byte r, byte g, byte b, byte e)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.e = e;
		}

		public ColorRgbe(ColorRgbFloat color)
		{
			var max = MathF.Max(color.b, MathF.Max(color.g, color.r));
			if (max <= 1e-32f)
			{
				r = g = b = e = 0;
			}
			else
			{
				MathHelper.FrExp(max, out var exponent);
				var scale = MathHelper.LdExp(1f, -exponent + 8);
				r = (byte)(scale * color.r);
				g = (byte)(scale * color.g);
				b = (byte)(scale * color.b);
				e = (byte)(exponent + 128);
			}
		}

		public ColorRgbFloat ToColorRgbFloat(float exposure = 1.0f)
		{
			if (e == 0)
			{
				return new ColorRgbFloat(0, 0, 0);
			}
			else
			{
				var fexp = MathHelper.LdExp(1f, e - (128 + 8)) / exposure;

				return new ColorRgbFloat(
					(r + 0.5f) * fexp,
					(g + 0.5f) * fexp,
					(b + 0.5f) * fexp
					);
			}
		}


		public bool Equals(ColorRgbe other)
		{
			return r == other.r && g == other.g && b == other.b && e == other.e;
		}

		public override bool Equals(object obj)
		{
			return obj is ColorRgbe other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = r.GetHashCode();
				hashCode = (hashCode * 397) ^ g.GetHashCode();
				hashCode = (hashCode * 397) ^ b.GetHashCode();
				hashCode = (hashCode * 397) ^ e.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(ColorRgbe left, ColorRgbe right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ColorRgbe left, ColorRgbe right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return $"{nameof(r)}: {r}, {nameof(g)}: {g}, {nameof(b)}: {b}, {nameof(e)}: {e}";
		}
	}
}
