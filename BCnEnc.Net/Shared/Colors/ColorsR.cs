using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BCnEncoder.Shared.Colors;

using static ColorBitConversionHelpers;


[StructLayout(LayoutKind.Sequential)]
public struct ColorR8 : IColorRed<ColorR8, byte>
{
	public byte r;

	/// <summary>
	/// The normalized float value [0-1].
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm8ToFloat(r);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = FloatToUnorm8(value);
	}

	/// <summary>
	/// The raw 8-bit unsigned integer value.
	/// </summary>
	public byte RawR
	{
		get => r;
		set => r = value;
	}

	public ColorR8(byte r)
	{
		this.r = r;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		float value = Unorm8ToFloat(r);
		return new ColorRgbaFloat(
			value,
			value,
			value
		);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = FloatToUnorm8(color.r);
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorR8S : IColorRed<ColorR8S, sbyte>
{
	public sbyte r;

	/// <summary>
	/// The normalized float value [-1 to 1].
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => SnormToFloat(r, 8);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = (sbyte)FloatToSnorm(value, 8);
	}

	/// <summary>
	/// The raw 8-bit signed integer value.
	/// </summary>
	public sbyte RawR
	{
		get => r;
		set => r = value;
	}

	public ColorR8S(sbyte r)
	{
		this.r = r;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		float value = SnormToFloat(r, 8);
		return new ColorRgbaFloat(
			value,
			value,
			value
		);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = (sbyte)FloatToSnorm(color.r, 8);
	}

	/// <inheritdoc />
	public bool Equals(ColorR8S other)
	{
		return r == other.r;
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorR8S other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return r.GetHashCode();
	}
}


/// <summary>
/// Raw 16-bit unsigned normalized single-channel format. 16 bits for Red channel.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ColorR16 : IColorRed<ColorR16, ushort>
{
	/// <summary>
	/// The raw 16-bit unsigned integer value.
	/// </summary>
	public ushort r;

	/// <summary>
	/// The normalized float value [0-1].
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm16ToFloat(r);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = FloatToUnorm16(value);
	}

	/// <summary>
	/// The raw 16-bit unsigned integer value.
	/// </summary>
	public ushort RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// Creates a new ColorR16 instance.
	/// </summary>
	/// <param name="r">The 16-bit raw value.</param>
	public ColorR16(ushort r)
	{
		this.r = r;
	}

	/// <summary>
	/// Creates a new ColorR16 instance from a normalized float value [0-1].
	/// </summary>
	/// <param name="r">The float value [0-1].</param>
	public ColorR16(float r)
	{
		this.r = FloatToUnorm16(r);
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		float value = Unorm16ToFloat(r);
		return new ColorRgbaFloat(
			value,
			value,
			value
		);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = FloatToUnorm16(color.r);
	}

	/// <inheritdoc />
	public bool Equals(ColorR16 other)
	{
		return r == other.r;
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorR16 other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return r.GetHashCode();
	}

	/// <summary>
	/// Equality operator.
	/// </summary>
	public static bool operator ==(ColorR16 left, ColorR16 right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Inequality operator.
	/// </summary>
	public static bool operator !=(ColorR16 left, ColorR16 right)
	{
		return !left.Equals(right);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"R: {r} ({R:F6})";
	}
}

/// <summary>
/// Raw 16-bit signed normalized single-channel format. 16 bits for Red channel.
/// Values range from -1 to 1.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ColorR16S : IColorRed<ColorR16S, short>
{
	/// <summary>
	/// The raw 16-bit signed integer value.
	/// </summary>
	public short r;

	/// <summary>
	/// The normalized float value [-1 to 1].
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => SnormToFloat(r, 16);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = (short)FloatToSnorm(value, 16);
	}

	/// <summary>
	/// The raw 16-bit signed integer value.
	/// </summary>
	public short RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// Creates a new ColorR16S instance.
	/// </summary>
	/// <param name="r">The 16-bit signed raw value.</param>
	public ColorR16S(short r)
	{
		this.r = r;
	}

	/// <summary>
	/// Creates a new ColorR16S instance from a normalized float value [-1 to 1].
	/// </summary>
	/// <param name="r">The float value [-1 to 1].</param>
	public ColorR16S(float r)
	{
		this.r = (short)FloatToSnorm(r, 16);
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		float value = SnormToFloat(r, 16);
		return new ColorRgbaFloat(
			value,
			value,
			value
		);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = (short)FloatToSnorm(color.r, 16);
	}

	/// <inheritdoc />
	public bool Equals(ColorR16S other)
	{
		return r == other.r;
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorR16S other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return r.GetHashCode();
	}

	/// <summary>
	/// Equality operator.
	/// </summary>
	public static bool operator ==(ColorR16S left, ColorR16S right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Inequality operator.
	/// </summary>
	public static bool operator !=(ColorR16S left, ColorR16S right)
	{
		return !left.Equals(right);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"R: {r} ({R:F6})";
	}
}

/// <summary>
/// Raw 16-bit floating-point single-channel format. 16 bits for Red channel.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ColorR16F : IColorRed<ColorR16F, Half>
{
	/// <summary>
	/// The 16-bit half-precision floating-point value.
	/// </summary>
	public Half r;

	/// <summary>
	/// The floating-point value as a regular float.
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => (float)r;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = (Half)value;
	}

	/// <summary>
	/// The raw half-precision floating-point value.
	/// </summary>
	public Half RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// Creates a new ColorR16F instance.
	/// </summary>
	/// <param name="r">The half-precision value.</param>
	public ColorR16F(Half r)
	{
		this.r = r;
	}

	/// <summary>
	/// Creates a new ColorR16F instance from a float value.
	/// </summary>
	/// <param name="r">The float value.</param>
	public ColorR16F(float r)
	{
		this.r = (Half)r;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(
			(float)r,
			(float)r,
			(float)r
		);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = (Half)color.r;
	}

	/// <inheritdoc />
	public bool Equals(ColorR16F other)
	{
		return r.Equals(other.r);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorR16F other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return r.GetHashCode();
	}

	/// <summary>
	/// Equality operator
	/// </summary>
	public static bool operator ==(ColorR16F left, ColorR16F right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Inequality operator
	/// </summary>
	public static bool operator !=(ColorR16F left, ColorR16F right)
	{
		return !left.Equals(right);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"R: {r} ({R:F6})";
	}
}

/// <summary>
/// Raw 32-bit floating-point single-channel format. 32 bits for Red channel.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ColorR32F : IColorRed<ColorR32F, float>
{
	/// <summary>
	/// The 32-bit single-precision floating-point value.
	/// </summary>
	public float r;

	/// <summary>
	/// The floating-point value as a regular float.
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => r;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = value;
	}

	/// <summary>
	/// The raw 32-bit floating-point value.
	/// </summary>
	public float RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// Creates a new ColorR32F instance.
	/// </summary>
	/// <param name="r">The float value.</param>
	public ColorR32F(float r)
	{
		this.r = r;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(
			r,
			r,
			r
		);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = color.r;
	}

	/// <inheritdoc />
	public bool Equals(ColorR32F other)
	{
		return r.Equals(other.r);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorR32F other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return r.GetHashCode();
	}

	/// <summary>
	/// Equality operator.
	/// </summary>
	public static bool operator ==(ColorR32F left, ColorR32F right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Inequality operator.
	/// </summary>
	public static bool operator !=(ColorR32F left, ColorR32F right)
	{
		return !left.Equals(right);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"R: {r:F6}";
	}
}
