using System;

namespace BCnEncoder.Shared.Colors;

public struct ColorR8 : IColor<ColorR8>
{
	public byte r;

	public ColorR8(byte r)
	{
		this.r = r;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(
			r / 255f,
			0,
			0
		);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = ByteHelper.ClampToByte(color.r * 255f);
	}

	/// <inheritdoc />
	public bool Equals(ColorR8 other)
	{
		return r == other.r;
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorR8 other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return r.GetHashCode();
	}
}

public struct ColorR8G8 : IColor<ColorR8G8>
{
	public byte r, g;

	public ColorR8G8(byte r, byte g)
	{
		this.r = r;
		this.g = g;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(
			r / 255f,
			g / 255f,
			0);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = ByteHelper.ClampToByte(color.r * 255f);
		g = ByteHelper.ClampToByte(color.g * 255f);
	}

	/// <inheritdoc />
	public bool Equals(ColorR8G8 other)
	{
		return r == other.r && g == other.g;
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorR8G8 other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(r, g);
	}
}

public struct ColorRgb24 : IColor<ColorRgb24>
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

		public ColorRgbaFloat ToColorRgbaFloat()
		{
			return new ColorRgbaFloat(
				r / 255f,
				g / 255f,
				b / 255f);
		}

		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			r = ByteHelper.ClampToByte(color.r * 255f);
			g = ByteHelper.ClampToByte(color.g * 255f);
			b = ByteHelper.ClampToByte(color.b * 255f);
		}
	}

public struct ColorRgba32 : IColor<ColorRgba32>
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
		return r == other.r &&
		       g == other.g &&
		       b == other.b &&
		       a == other.a;
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
	public static ColorRgba32 operator >> (ColorRgba32 left, int right)
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

	public readonly ColorRgbFloat ToRgbFloat()
	{
		return new ColorRgbFloat(this);
	}

	/// <inheritdoc />
	public readonly ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(this);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = ByteHelper.ClampToByte(color.r * 255f);
		g = ByteHelper.ClampToByte(color.g * 255f);
		b = ByteHelper.ClampToByte(color.b * 255f);
		a = ByteHelper.ClampToByte(color.a * 255f);
	}
}

internal struct ColorBgr24 : IColor<ColorBgr24>
{
	public byte b, g, r;

	public ColorBgr24(byte b, byte g, byte r)
	{
		this.b = b;
		this.g = g;
		this.r = r;
	}

	/// <inheritdoc />
	public bool Equals(ColorBgr24 other)
	{
		return b == other.b && g == other.g && r == other.r;
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorBgr24 other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(b, g, r);
	}

	/// <inheritdoc />
	public readonly ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(
			this.r / 255f,
			this.g / 255f,
			this.b / 255f);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = ByteHelper.ClampToByte(color.r * 255f);
		g = ByteHelper.ClampToByte(color.g * 255f);
		b = ByteHelper.ClampToByte(color.b * 255f);
	}
}

internal struct ColorBgra32 : IColor<ColorBgra32>
{
	public byte b, g, r, a;

	public ColorBgra32(byte b, byte g, byte r)
	{
		this.b = b;
		this.g = g;
		this.r = r;
		this.a = 255;
	}

	public ColorBgra32(byte b, byte g, byte r, byte a)
	{
		this.b = b;
		this.g = g;
		this.r = r;
		this.a = a;
	}

	/// <inheritdoc />
	public bool Equals(ColorBgra32 other)
	{
		return b == other.b && g == other.g && r == other.r && a == other.a;
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorBgra32 other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(b, g, r, a);
	}

	/// <inheritdoc />
	public readonly ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(
			this.r / 255f,
			this.g / 255f,
			this.b / 255f,
			this.a / 255f);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = ByteHelper.ClampToByte(color.r * 255f);
		g = ByteHelper.ClampToByte(color.g * 255f);
		b = ByteHelper.ClampToByte(color.b * 255f);
		a = ByteHelper.ClampToByte(color.a * 255f);
	}
}
