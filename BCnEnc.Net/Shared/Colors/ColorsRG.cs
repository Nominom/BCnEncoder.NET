using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Shared.Colors;

using static ColorBitConversionHelpers;

/// <summary>
/// Raw 8-bit unsigned normalized two-channel format. 8 bits for Red, 8 bits for Green.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ColorR8G8 : IColorRedGreen<ColorR8G8, byte>
{
	/// <summary>
	/// The raw 8-bit unsigned integer red value.
	/// </summary>
	public byte r;

	/// <summary>
	/// The raw 8-bit unsigned integer green value.
	/// </summary>
	public byte g;

	/// <summary>
	/// The normalized float value for red [0-1].
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm8ToFloat(r);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = FloatToUnorm8(value);
	}

	/// <summary>
	/// The normalized float value for green [0-1].
	/// </summary>
	public float G
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm8ToFloat(g);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => g = FloatToUnorm8(value);
	}

	/// <summary>
	/// The raw 8-bit unsigned integer red value.
	/// </summary>
	public byte RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// The raw 8-bit unsigned integer green value.
	/// </summary>
	public byte RawG
	{
		get => g;
		set => g = value;
	}

	/// <summary>
	/// Creates a new ColorR8G8 with the specified values.
	/// </summary>
	public ColorR8G8(byte r, byte g)
	{
		this.r = r;
		this.g = g;
	}

	/// <summary>
	/// Creates a new ColorR8G8 with the specified normalized values.
	/// </summary>
	public ColorR8G8(float r, float g)
	{
		this.r = FloatToUnorm8(r);
		this.g = FloatToUnorm8(g);
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(R, G, 0, 1);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		R = color.r;
		G = color.g;
	}

	/// <summary>
	/// Returns a string representation of this color.
	/// </summary>
	public override string ToString()
	{
		return $"R8G8[R={R:F3}, G={G:F3}]";
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		return obj is ColorR8G8 other && Equals(other);
	}

	/// <inheritdoc />
	public bool Equals(ColorR8G8 other)
	{
		return r == other.r && g == other.g;
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(r, g);
	}

	/// <summary>
	/// Compares two <see cref="ColorR8G8"/> objects for equality.
	/// </summary>
	public static bool operator ==(ColorR8G8 left, ColorR8G8 right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Compares two <see cref="ColorR8G8"/> objects for inequality.
	/// </summary>
	public static bool operator !=(ColorR8G8 left, ColorR8G8 right)
	{
		return !(left == right);
	}
}

/// <summary>
/// Raw 8-bit signed normalized two-channel format. 8 bits for Red, 8 bits for Green.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ColorR8G8S : IColorRedGreen<ColorR8G8S, sbyte>
{
	/// <summary>
	/// The raw 8-bit signed integer red value.
	/// </summary>
	public sbyte r;

	/// <summary>
	/// The raw 8-bit signed integer green value.
	/// </summary>
	public sbyte g;

	/// <summary>
	/// The normalized float value for red [0-1].
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => SnormToFloat(r, 8);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = (sbyte)FloatToSnorm(value, 8);
	}

	/// <summary>
	/// The normalized float value for green [0-1].
	/// </summary>
	public float G
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => SnormToFloat(g, 8);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => g = (sbyte)FloatToSnorm(value, 8);
	}

	/// <summary>
	/// The raw 8-bit unsigned integer red value.
	/// </summary>
	public sbyte RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// The raw 8-bit unsigned integer green value.
	/// </summary>
	public sbyte RawG
	{
		get => g;
		set => g = value;
	}

	/// <summary>
	/// Creates a new ColorR8G8 with the specified values.
	/// </summary>
	public ColorR8G8S(sbyte r, sbyte g)
	{
		this.r = r;
		this.g = g;
	}

	/// <summary>
	/// Creates a new ColorR8G8 with the specified normalized values.
	/// </summary>
	public ColorR8G8S(float r, float g)
	{
		R = r;
		G = g;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(R, G, 0, 1);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		R = color.r;
		G = color.g;
	}

	/// <summary>
	/// Returns a string representation of this color.
	/// </summary>
	public override string ToString()
	{
		return $"R8G8S[R={R:F3}, G={G:F3}]";
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		return obj is ColorR8G8S other && Equals(other);
	}

	/// <inheritdoc />
	public bool Equals(ColorR8G8S other)
	{
		return r == other.r && g == other.g;
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(r, g);
	}

	/// <summary>
	/// Compares two <see cref="ColorR8G8"/> objects for equality.
	/// </summary>
	public static bool operator ==(ColorR8G8S left, ColorR8G8S right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Compares two <see cref="ColorR8G8"/> objects for inequality.
	/// </summary>
	public static bool operator !=(ColorR8G8S left, ColorR8G8S right)
	{
		return !(left == right);
	}
}

/// <summary>
/// Raw 16-bit unsigned normalized two-channel format. 16 bits for Red, 16 bits for Green.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ColorR16G16 : IColorRedGreen<ColorR16G16, ushort>
{
	/// <summary>
	/// The 16-bit unsigned integer red value.
	/// </summary>
	public ushort r;

	/// <summary>
	/// The 16-bit unsigned integer green value.
	/// </summary>
	public ushort g;

	/// <summary>
	/// The floating-point value for red.
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm16ToFloat(r);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = FloatToUnorm16(value);
	}

	/// <summary>
	/// The floating-point value for green.
	/// </summary>
	public float G
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Unorm16ToFloat(g);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => g = FloatToUnorm16(value);
	}

	/// <summary>
	/// The raw 16-bit unsigned integer red value.
	/// </summary>
	public ushort RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// The raw 16-bit unsigned integer green value.
	/// </summary>
	public ushort RawG
	{
		get => g;
		set => g = value;
	}

	/// <summary>
	/// Creates a new ColorR16G16 with the specified values.
	/// </summary>
	public ColorR16G16(ushort r, ushort g)
	{
		this.r = r;
		this.g = g;
	}

	/// <summary>
	/// Creates a new ColorR16G16 with the specified values.
	/// </summary>
	public ColorR16G16(float r, float g)
	{
		this.R = r;
		this.G = g;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(R, G, 0, 1);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		R = color.r;
		G = color.g;
	}

	/// <summary>
	/// Returns a string representation of this color.
	/// </summary>
	public override string ToString()
	{
		return $"R16G16[R={R:F3}, G={G:F3}]";
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		return obj is ColorR16G16 other && Equals(other);
	}

	/// <inheritdoc />
	public bool Equals(ColorR16G16 other)
	{
		return r == other.r && g == other.g;
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(r, g);
	}

	/// <summary>
	/// Compares two <see cref="ColorR16G16"/> objects for equality.
	/// </summary>
	public static bool operator ==(ColorR16G16 left, ColorR16G16 right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Compares two <see cref="ColorR16G16"/> objects for inequality.
	/// </summary>
	public static bool operator !=(ColorR16G16 left, ColorR16G16 right)
	{
		return !(left == right);
	}
}

/// <summary>
/// Raw 16-bit unsigned normalized two-channel format. 16 bits for Red, 16 bits for Green.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ColorR16G16S : IColorRedGreen<ColorR16G16S, short>
{
	/// <summary>
	/// The 16-bit signed integer red value.
	/// </summary>
	public short r;

	/// <summary>
	/// The 16-bit signed integer green value.
	/// </summary>
	public short g;

	/// <summary>
	/// The floating-point value for red.
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => SnormToFloat(r, 16);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = (short)FloatToSnorm(value, 16);
	}

	/// <summary>
	/// The floating-point value for green.
	/// </summary>
	public float G
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => SnormToFloat(g, 16);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => g = (short)FloatToSnorm(value, 16);
	}

	/// <summary>
	/// The raw 16-bit signed integer red value.
	/// </summary>
	public short RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// The raw 16-bit signed integer green value.
	/// </summary>
	public short RawG
	{
		get => g;
		set => g = value;
	}

	/// <summary>
	/// Creates a new ColorR16G16S with the specified values.
	/// </summary>
	public ColorR16G16S(short r, short g)
	{
		this.r = r;
		this.g = g;
	}

	/// <summary>
	/// Creates a new ColorR16G16S with the specified values.
	/// </summary>
	public ColorR16G16S(float r, float g)
	{
		R = r;
		G = g;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(R, G, 0, 1);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		R = color.r;
		G = color.g;
	}

	/// <summary>
	/// Returns a string representation of this color.
	/// </summary>
	public override string ToString()
	{
		return $"R16G16S[R={R:F3}, G={G:F3}]";
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		return obj is ColorR16G16S other && Equals(other);
	}

	/// <inheritdoc />
	public bool Equals(ColorR16G16S other)
	{
		return r == other.r && g == other.g;
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(r, g);
	}

	/// <summary>
	/// Compares two <see cref="ColorR16G16S"/> objects for equality.
	/// </summary>
	public static bool operator ==(ColorR16G16S left, ColorR16G16S right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Compares two <see cref="ColorR16G16S"/> objects for inequality.
	/// </summary>
	public static bool operator !=(ColorR16G16S left, ColorR16G16S right)
	{
		return !(left == right);
	}
}

/// <summary>
/// Raw 16-bit half-precision floating-point two-channel format. 16 bits for Red, 16 bits for Green.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ColorR16G16F : IColorRedGreen<ColorR16G16F, Half>
{
	/// <summary>
	/// The 16-bit half-precision floating-point red value.
	/// </summary>
	public Half r;

	/// <summary>
	/// The 16-bit half-precision floating-point green value.
	/// </summary>
	public Half g;

	/// <summary>
	/// The floating-point value for red.
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => (float)r;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = (Half)value;
	}

	/// <summary>
	/// The floating-point value for green.
	/// </summary>
	public float G
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => (float)g;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => g = (Half)value;
	}

	/// <summary>
	/// The raw 16-bit half-precision floating-point red value.
	/// </summary>
	public Half RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// The raw 16-bit half-precision floating-point green value.
	/// </summary>
	public Half RawG
	{
		get => g;
		set => g = value;
	}

	/// <summary>
	/// Creates a new ColorR16G16F with the specified values.
	/// </summary>
	public ColorR16G16F(Half r, Half g)
	{
		this.r = r;
		this.g = g;
	}

	/// <summary>
	/// Creates a new ColorR16G16F with the specified values.
	/// </summary>
	public ColorR16G16F(float r, float g)
	{
		this.r = (Half)r;
		this.g = (Half)g;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat((float)r, (float)g, 0, 1);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = (Half)color.r;
		g = (Half)color.g;
	}

	/// <summary>
	/// Returns a string representation of this color.
	/// </summary>
	public override string ToString()
	{
		return $"R16G16F[R={R:F3}, G={G:F3}]";
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		return obj is ColorR16G16F other && Equals(other);
	}

	/// <inheritdoc />
	public bool Equals(ColorR16G16F other)
	{
		return r == other.r && g == other.g;
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(r, g);
	}

	/// <summary>
	/// Compares two <see cref="ColorR16G16F"/> objects for equality.
	/// </summary>
	public static bool operator ==(ColorR16G16F left, ColorR16G16F right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Compares two <see cref="ColorR16G16F"/> objects for inequality.
	/// </summary>
	public static bool operator !=(ColorR16G16F left, ColorR16G16F right)
	{
		return !(left == right);
	}
}

/// <summary>
/// Raw 32-bit single-precision floating-point two-channel format. 32 bits for Red, 32 bits for Green.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ColorR32G32F : IColorRedGreen<ColorR32G32F, float>
{
	/// <summary>
	/// The 32-bit single-precision floating-point red value.
	/// </summary>
	public float r;

	/// <summary>
	/// The 32-bit single-precision floating-point green value.
	/// </summary>
	public float g;

	/// <summary>
	/// The floating-point value for red.
	/// </summary>
	public float R
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => r;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => r = value;
	}

	/// <summary>
	/// The floating-point value for green.
	/// </summary>
	public float G
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => g;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set => g = value;
	}

	/// <summary>
	/// The raw 32-bit single-precision floating-point red value.
	/// </summary>
	public float RawR
	{
		get => r;
		set => r = value;
	}

	/// <summary>
	/// The raw 32-bit single-precision floating-point green value.
	/// </summary>
	public float RawG
	{
		get => g;
		set => g = value;
	}

	/// <summary>
	/// Creates a new ColorR32G32F with the specified values.
	/// </summary>
	public ColorR32G32F(float r, float g)
	{
		this.r = r;
		this.g = g;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(r, g, 0, 1);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = color.r;
		g = color.g;
	}

	/// <summary>
	/// Returns a string representation of this color.
	/// </summary>
	public override string ToString()
	{
		return $"R32G32F[R={R:F3}, G={G:F3}]";
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		return obj is ColorR32G32F other && Equals(other);
	}

	/// <inheritdoc />
	public bool Equals(ColorR32G32F other)
	{
		return r == other.r && g == other.g;
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(r, g);
	}

	/// <summary>
	/// Compares two <see cref="ColorR32G32F"/> objects for equality.
	/// </summary>
	public static bool operator ==(ColorR32G32F left, ColorR32G32F right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Compares two <see cref="ColorR32G32F"/> objects for inequality.
	/// </summary>
	public static bool operator !=(ColorR32G32F left, ColorR32G32F right)
	{
		return !(left == right);
	}
}
