using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BCnEncoder.Shared.Colors;


using static BCnEncoder.Shared.Colors.ColorBitConversionHelpers;


[StructLayout(LayoutKind.Sequential)]
public struct ColorRgb24 : IColorRgb<ColorRgb24, byte>
{
	public byte r, g, b;

	public ColorRgb24(byte r, byte g, byte b)
	{
		this.r = r;
		this.g = g;
		this.b = b;
	}

	public ColorRgb24(ColorRgba32 color)
	{
		this.r = color.r;
		this.g = color.g;
		this.b = color.b;
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
			Unorm8ToFloat(r),
			Unorm8ToFloat(g),
			Unorm8ToFloat(b));
	}

	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = FloatToUnorm8(color.r);
		g = FloatToUnorm8(color.g);
		b = FloatToUnorm8(color.b);
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct ColorBgr24 : IColorRgb<ColorBgr24, byte>
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

	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(
			Unorm8ToFloat(r),
			Unorm8ToFloat(g),
			Unorm8ToFloat(b));
	}

	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = FloatToUnorm8(color.r);
		g = FloatToUnorm8(color.g);
		b = FloatToUnorm8(color.b);
	}
}

public struct ColorRgbFloat : IColorRgb<ColorRgbFloat, float>
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
		var dr = Math.Sign(other.r) * MathF.Log(1 + MathF.Abs(other.r)) -
		         Math.Sign(r) * MathF.Log(1 + MathF.Abs(r));
		var dg = Math.Sign(other.g) * MathF.Log(1 + MathF.Abs(other.g)) -
		         Math.Sign(g) * MathF.Log(1 + MathF.Abs(g));
		var db = Math.Sign(other.b) * MathF.Log(1 + MathF.Abs(other.b)) -
		         Math.Sign(b) * MathF.Log(1 + MathF.Abs(b));
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
		if (r < (float)Half.MinValue) r = (float)Half.MinValue;
		else if (g > (float)Half.MaxValue) g = (float)Half.MaxValue;
		if (b < (float)Half.MinValue) b = (float)Half.MinValue;
		else if (r > (float)Half.MaxValue) r = (float)Half.MaxValue;
		if (g < (float)Half.MinValue) g = (float)Half.MinValue;
		else if (b > (float)Half.MaxValue) b = (float)Half.MaxValue;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(this);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		this.r = color.r;
		this.g = color.g;
		this.b = color.b;
	}

	public static implicit operator ColorRgbaFloat(ColorRgbFloat c) => c.ToColorRgbaFloat();

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
}

public struct ColorRgbHalf : IColorRgb<ColorRgbHalf, Half>
{
	public Half r, g, b;

	public ColorRgbHalf(float r, float g, float b)
	{
		this.r = (Half)r;
		this.g = (Half)g;
		this.b = (Half)b;
	}

	public ColorRgbHalf(Half r, Half g, Half b)
	{
		this.r = r;
		this.g = g;
		this.b = b;
	}

	/// <inheritdoc />
	public bool Equals(ColorRgbHalf other)
	{
		return r.Equals(other.r) && g.Equals(other.g) && b.Equals(other.b);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorRgbHalf other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(r, g, b);
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat((float)r, (float)g, (float)b);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		color.ClampToHalf();
		r = (Half)(color.r);
		g = (Half)(color.g);
		b = (Half)(color.b);
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
}
