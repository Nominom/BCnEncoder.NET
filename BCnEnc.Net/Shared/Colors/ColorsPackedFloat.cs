using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BCnEncoder.Shared.Colors;

using static BCnEncoder.Shared.Colors.ColorBitConversionHelpers;

/// <summary>
/// Represents a packed R11G11B10F floating point color.
/// This format uses 11 bits for red, 11 bits for green, and 10 bits for blue, all as floating point values.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ColorR11G11B10PackedUFloat : IColorRgb<ColorR11G11B10PackedUFloat, uint>, IColorPacked<uint>
{
	/// <inheritdoc/>
	public static PackedRgbaMask ChannelMask { get; } = "R11G11B10";

	uint IColorPacked<uint>.Data
	{
		get => data;
		set => data = value;
	}

    // Masks and shifts for accessing components
    private const uint RedMask = 0x7FFU;
    private const uint GreenMask = 0x7FFU << 11;
    private const int GreenShift = 11;
    private const uint BlueMask = 0x3FFU << 22;
    private const int BlueShift = 22;

    // The packed 32-bit value containing all three channels
    public uint data;

    /// <summary>
    /// Gets or sets the red component as a floating-point value.
    /// </summary>
    public float R
    {
        readonly get => Float11ToFloat(RawR);
        set => RawR = FloatToFloat11(value);
    }

    /// <summary>
    /// Gets or sets the green component as a floating-point value.
    /// </summary>
    public float G
    {
        readonly get => Float11ToFloat(RawG);
        set => RawG = FloatToFloat11(value);
    }

    /// <summary>
    /// Gets or sets the blue component as a floating-point value.
    /// </summary>
    public float B
    {
        readonly get => Float10ToFloat(RawB);
        set => RawB = FloatToFloat10(value);
    }

    /// <summary>
    /// Gets or sets the raw 11-bit red component.
    /// </summary>
    public uint RawR
    {
        readonly get => (data & RedMask);
        set
        {
            if (value > 0x7FF) value = 0x7FF; // Clamp to 11 bits
            data = (data & ~RedMask) | value;
        }
    }

    /// <summary>
    /// Gets or sets the raw 11-bit green component.
    /// </summary>
    public uint RawG
    {
        readonly get => (data & GreenMask) >> GreenShift;
        set
        {
            if (value > 0x7FF) value = 0x7FF; // Clamp to 11 bits
            data = (data & ~GreenMask) | (value << GreenShift);
        }
    }

    /// <summary>
    /// Gets or sets the raw 10-bit blue component.
    /// </summary>
    public uint RawB
    {
        readonly get => (data & BlueMask) >> BlueShift;
        set
        {
            if (value > 0x3FF) value = 0x3FF; // Clamp to 10 bits
            data = (data & ~BlueMask) | (value << BlueShift);
        }
    }

    /// <summary>
    /// Creates a new ColorR11G11B10F from a 32-bit packed value.
    /// </summary>
    public ColorR11G11B10PackedUFloat(uint data)
    {
        this.data = data;
    }

    /// <summary>
    /// Creates a new ColorR11G11B10F from individual floating point values.
    /// </summary>
    public ColorR11G11B10PackedUFloat(float r, float g, float b)
    {
        data = 0;
        R = r;
        G = g;
        B = b;
    }

    /// <inheritdoc />
    public ColorRgbaFloat ToColorRgbaFloat()
    {
        return new ColorRgbaFloat(R, G, B);
    }

    /// <inheritdoc />
    public void FromColorRgbaFloat(ColorRgbaFloat color)
    {
        R = color.r;
        G = color.g;
        B = color.b;
    }

    /// <inheritdoc />
    public bool Equals(ColorR11G11B10PackedUFloat other)
    {
        return data == other.data;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return obj is ColorR11G11B10PackedUFloat other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return data.GetHashCode();
    }

    public static bool operator ==(ColorR11G11B10PackedUFloat left, ColorR11G11B10PackedUFloat right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ColorR11G11B10PackedUFloat left, ColorR11G11B10PackedUFloat right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"R: {R}, G: {G}, B: {B}";
    }
}
