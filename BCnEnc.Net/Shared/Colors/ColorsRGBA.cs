using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BCnEncoder.Shared.Colors;


using static BCnEncoder.Shared.Colors.ColorBitConversionHelpers;

[StructLayout(LayoutKind.Sequential)]
public struct ColorRgba32 : IColorRgba<ColorRgba32, byte>
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
		return new ColorRgbaFloat(
			Unorm8ToFloat(r),
			Unorm8ToFloat(g),
			Unorm8ToFloat(b),
			Unorm8ToFloat(a)
			);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = FloatToUnorm8(color.r);
		g = FloatToUnorm8(color.g);
		b = FloatToUnorm8(color.b);
		a = FloatToUnorm8(color.a);
	}

	/// <summary>
	/// Gets or sets the red component as a floating-point value.
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm8ToFloat(r);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = FloatToUnorm8(value);
	}

	/// <summary>
	/// Gets or sets the green component as a floating-point value.
	/// </summary>
	public float G
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm8ToFloat(g);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => g = FloatToUnorm8(value);
	}

	/// <summary>
	/// Gets or sets the blue component as a floating-point value.
	/// </summary>
	public float B
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm8ToFloat(b);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => b = FloatToUnorm8(value);
	}

	/// <summary>
	/// Gets or sets the alpha component as a floating-point value.
	/// </summary>
	public float A
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm8ToFloat(a);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => a = FloatToUnorm8(value);
	}

	/// <summary>
	/// Gets or sets the raw red component value.
	/// </summary>
	public byte RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// Gets or sets the raw green component value.
	/// </summary>
	public byte RawG
	{
		get => g;
		set => g = value;
	}

	/// <summary>
	/// Gets or sets the raw blue component value.
	/// </summary>
	public byte RawB
	{
		get => b;
		set => b = value;
	}

	/// <summary>
	/// Gets or sets the raw alpha component value.
	/// </summary>
	public byte RawA
	{
		get => a;
		set => a = value;
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct ColorBgra32 : IColorRgba<ColorBgra32, byte>
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
			Unorm8ToFloat(r),
			Unorm8ToFloat(g),
			Unorm8ToFloat(b),
			Unorm8ToFloat(a)
		);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = FloatToUnorm8(color.r);
		g = FloatToUnorm8(color.g);
		b = FloatToUnorm8(color.b);
		a = FloatToUnorm8(color.a);
	}

	/// <summary>
	/// Gets or sets the red component as a floating-point value.
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm8ToFloat(r);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = FloatToUnorm8(value);
	}

	/// <summary>
	/// Gets or sets the green component as a floating-point value.
	/// </summary>
	public float G
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm8ToFloat(g);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => g = FloatToUnorm8(value);
	}

	/// <summary>
	/// Gets or sets the blue component as a floating-point value.
	/// </summary>
	public float B
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm8ToFloat(b);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => b = FloatToUnorm8(value);
	}

	/// <summary>
	/// Gets or sets the alpha component as a floating-point value.
	/// </summary>
	public float A
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm8ToFloat(a);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => a = FloatToUnorm8(value);
	}

	/// <summary>
	/// Gets or sets the raw red component value.
	/// </summary>
	public byte RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// Gets or sets the raw green component value.
	/// </summary>
	public byte RawG
	{
		get => g;
		set => g = value;
	}

	/// <summary>
	/// Gets or sets the raw blue component value.
	/// </summary>
	public byte RawB
	{
		get => b;
		set => b = value;
	}

	/// <summary>
	/// Gets or sets the raw alpha component value.
	/// </summary>
	public byte RawA
	{
		get => a;
		set => a = value;
	}
}

public struct ColorRgbaFloat : IColorRgba<ColorRgbaFloat, float>
{
	public float r, g, b, a;

	public ColorRgbaFloat(float r, float g, float b, float a)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public ColorRgbaFloat(ColorRgbFloat other)
	{
		this.r = other.r;
		this.g = other.g;
		this.b = other.b;
		this.a = 1;
	}

	public ColorRgbaFloat(float r, float g, float b)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = 1;
	}

	public ColorRgbaFloat(Vector4 other)
	{
		this.r = other.X;
		this.g = other.Y;
		this.b = other.Z;
		this.a = other.W;
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
		return $"r : {r:0.00000} g : {g:0.00000} b : {b:0.00000} a : {a:0.00000}";
	}

	internal void ClampToHalf()
	{
		if (r < (float)Half.MinValue) r = (float)Half.MinValue;
		else if (g > (float)Half.MaxValue) g = (float)Half.MaxValue;
		if (b < (float)Half.MinValue) b = (float)Half.MinValue;
		else if (r > (float)Half.MaxValue) r = (float)Half.MaxValue;
		if (g < (float)Half.MinValue) g = (float)Half.MinValue;
		else if (b > (float)Half.MaxValue) b = (float)Half.MaxValue;
		if (a < (float)Half.MinValue) a = (float)Half.MinValue;
		else if (a > (float)Half.MaxValue) a = (float)Half.MaxValue;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return this;
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = color.r;
		g = color.g;
		b = color.b;
		a = color.a;
	}

	public Vector3 ToVector3()
	{
		return new Vector3(r, g, b);
	}

	public Vector4 ToVector4()
	{
		return new Vector4(r, g, b, a);
	}

	public static implicit operator ColorRgbFloat(ColorRgbaFloat c) => new(c.r, c.g, c.b);

	/// <summary>
	/// Gets or sets the red component as a floating-point value.
	/// </summary>
	public float R
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// Gets or sets the green component as a floating-point value.
	/// </summary>
	public float G
	{
		get => g;
		set => g = value;
	}

	/// <summary>
	/// Gets or sets the blue component as a floating-point value.
	/// </summary>
	public float B
	{
		get => b;
		set => b = value;
	}

	/// <summary>
	/// Gets or sets the alpha component as a floating-point value.
	/// </summary>
	public float A
	{
		get => a;
		set => a = value;
	}

	/// <summary>
	/// Gets or sets the raw red component value.
	/// </summary>
	public float RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// Gets or sets the raw green component value.
	/// </summary>
	public float RawG
	{
		get => g;
		set => g = value;
	}

	/// <summary>
	/// Gets or sets the raw blue component value.
	/// </summary>
	public float RawB
	{
		get => b;
		set => b = value;
	}

	/// <summary>
	/// Gets or sets the raw alpha component value.
	/// </summary>
	public float RawA
	{
		get => a;
		set => a = value;
	}
}

public struct ColorRgbaHalf : IColorRgba<ColorRgbaHalf, Half>
{
	public Half r, g, b, a;

	public ColorRgbaHalf(float r, float g, float b)
	{
		this.r = (Half)r;
		this.g = (Half)g;
		this.b = (Half)b;
		a = (Half)1;
	}

	public ColorRgbaHalf(float r, float g, float b, float a)
	{
		this.r = (Half)r;
		this.g = (Half)g;
		this.b = (Half)b;
		this.a = (Half)a;
	}

	public ColorRgbaHalf(Half r, Half g, Half b)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		a = (Half)1;
	}

	public ColorRgbaHalf(Half r, Half g, Half b, Half a)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat((float)r, (float)g, (float)b, (float)a);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		color.ClampToHalf();
		r = (Half)color.r;
		g = (Half)color.g;
		b = (Half)color.b;
		a = (Half)color.a;
	}

	/// <inheritdoc />
	public bool Equals(ColorRgbaHalf other)
	{
		return r.Equals(other.r) && g.Equals(other.g) && b.Equals(other.b) && a.Equals(other.a);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorRgbaHalf other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(r, g, b, a);
	}

	/// <summary>
	/// Gets or sets the red component as a floating-point value.
	/// </summary>
	public float R
	{
		get => (float)r;
		set => r = (Half)value;
	}

	/// <summary>
	/// Gets or sets the green component as a floating-point value.
	/// </summary>
	public float G
	{
		get => (float)g;
		set => g = (Half)value;
	}

	/// <summary>
	/// Gets or sets the blue component as a floating-point value.
	/// </summary>
	public float B
	{
		get => (float)b;
		set => b = (Half)value;
	}

	/// <summary>
	/// Gets or sets the alpha component as a floating-point value.
	/// </summary>
	public float A
	{
		get => (float)a;
		set => a = (Half)value;
	}

	/// <summary>
	/// Gets or sets the raw red component value.
	/// </summary>
	public Half RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// Gets or sets the raw green component value.
	/// </summary>
	public Half RawG
	{
		get => g;
		set => g = value;
	}

	/// <summary>
	/// Gets or sets the raw blue component value.
	/// </summary>
	public Half RawB
	{
		get => b;
		set => b = value;
	}

	/// <summary>
	/// Gets or sets the raw alpha component value.
	/// </summary>
	public Half RawA
	{
		get => a;
		set => a = value;
	}
}
