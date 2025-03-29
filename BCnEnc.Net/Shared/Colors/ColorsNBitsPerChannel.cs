using System.Numerics;

using static BCnEncoder.Shared.Colors.ColorBitConversionHelpers;

namespace BCnEncoder.Shared.Colors;

public struct ColorRgb555 : IColor<ColorRgb555>
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

	public float R
	{
		readonly get => Unorm5ToFloat(RawR);
		set => RawR = FloatToUnorm5(value);
	}

	public float G
	{
		readonly get => Unorm5ToFloat(RawG);
		set => RawG = FloatToUnorm5(value);
	}

	public float B
	{
		readonly get => Unorm5ToFloat(RawB);
		set => RawB = FloatToUnorm5(value);
	}

	public byte ByteR
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

	public byte ByteG
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

	public byte ByteB
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

	public uint RawR
	{
		readonly get => (ushort)((data & RedMask) >> RedShift);
		set
		{
			if (value > 31) value = 31;
			if (value < 0) value = 0;
			data = (ushort)(data & ~RedMask);
			data = (ushort)(data | (value << RedShift));
		}
	}

	public uint RawG
	{
		readonly get => (ushort)((data & GreenMask) >> GreenShift);
		set
		{
			if (value > 31) value = 31;
			if (value < 0) value = 0;
			data = (ushort)(data & ~GreenMask);
			data = (ushort)(data | (value << GreenShift));
		}
	}

	public uint RawB
	{
		readonly get => (ushort)(data & BlueMask);
		set
		{
			if (value > 31) value = 31;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | value);
		}
	}

	public ColorRgb555(float r, float g, float b)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
	}

	public override string ToString()
	{
		return $"r : {R} g : {G} b : {B}";
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(
			R,
			G,
			B);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		R = color.r;
		G = color.g;
		B = color.b;
	}
}

public struct ColorRgb565 : IColor<ColorRgb565>
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

	public float R
	{
		readonly get => Unorm5ToFloat(RawR);
		set => RawR = FloatToUnorm5(value);
	}

	public float G
	{
		readonly get => Unorm6ToFloat(RawG);
		set => RawG = FloatToUnorm6(value);
	}

	public float B
	{
		readonly get => Unorm5ToFloat(RawB);
		set => RawB = FloatToUnorm5(value);
	}

	public byte ByteR
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

	public byte ByteG
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

	public byte ByteB
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

	public uint RawR
	{
		readonly get => (ushort)((data & RedMask) >> RedShift);
		set
		{
			if (value > 31) value = 31;
			data = (ushort)(data & ~RedMask);
			data = (ushort)(data | (value << RedShift));
		}
	}

	public uint RawG
	{
		readonly get => (ushort)((data & GreenMask) >> GreenShift);
		set
		{
			if (value > 63) value = 63;
			data = (ushort)(data & ~GreenMask);
			data = (ushort)(data | (value << GreenShift));
		}
	}

	public uint RawB
	{
		readonly get => (ushort)(data & BlueMask);
		set
		{
			if (value > 31) value = 31;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | value);
		}
	}

	public ColorRgb565(float r, float g, float b)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
	}

	public override string ToString()
	{
		return $"r : {R} g : {G} b : {B}";
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(
			R,
			G,
			B);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		R = color.r;
		G = color.g;
		B = color.b;
	}
}

public struct ColorR10G10B10A2 : IColor<ColorR10G10B10A2>
{
	public bool Equals(ColorR10G10B10A2 other)
	{
		return data == other.data;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorR10G10B10A2 other && Equals(other);
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorR10G10B10A2 left, ColorR10G10B10A2 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorR10G10B10A2 left, ColorR10G10B10A2 right)
	{
		return !left.Equals(right);
	}

	private const uint RedMask = 0x000003FF;
	private const int RedShift = 0;
	private const uint GreenMask = 0x000FFC00;
	private const int GreenShift = 10;
	private const uint BlueMask = 0x3FF00000;
	private const int BlueShift = 20;
	private const uint AlphaMask = 0xC0000000;
	private const int AlphaShift = 30;

	private const uint RedMaxValue = 1023;
	private const uint GreenMaxValue = 1023;
	private const uint BlueMaxValue = 1023;
	private const uint AlphaMaxValue = 3;

	public uint data;

	public float R
	{
		readonly get => Unorm10ToFloat(RawR);
		set => RawR = FloatToUnorm10(value);
	}

	public float G
	{
		readonly get => Unorm10ToFloat(RawG);
		set => RawG = FloatToUnorm10(value);
	}

	public float B
	{
		readonly get => Unorm10ToFloat(RawB);
		set => RawB = FloatToUnorm10(value);
	}

	public float A
	{
		readonly get => Unorm2ToFloat(RawA);
		set => RawA = FloatToUnorm2(value);
	}

	public uint RawR
	{
		readonly get => (data & RedMask) >> RedShift;
		set
		{
			if (value > RedMaxValue) value = RedMaxValue;
			data = (data & ~RedMask) | (value << RedShift);
		}
	}

	public uint RawG
	{
		readonly get => (data & GreenMask) >> GreenShift;
		set
		{
			if (value > GreenMaxValue) value = GreenMaxValue;
			data = (data & ~GreenMask) | (value << GreenShift);
		}
	}

	public uint RawB
	{
		readonly get => (data & BlueMask) >> BlueShift;
		set
		{
			if (value > BlueMaxValue) value = BlueMaxValue;
			data = (data & ~BlueMask) | (value << BlueShift);
		}
	}

	public uint RawA
	{
		readonly get => (data & AlphaMask) >> AlphaShift;
		set
		{
			if (value > AlphaMaxValue) value = AlphaMaxValue;
			data = (data & ~AlphaMask) | (value << AlphaShift);
		}
	}

	public ColorR10G10B10A2(float r, float g, float b, float a)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public override string ToString()
	{
		return $"r : {R:F3} g : {G:F3} b : {B:F3} a : {A:F3}";
	}

	/// <inheritdoc />
	public readonly ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(R, G, B, A);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		R = color.r;
		G = color.g;
		B = color.b;
		A = color.a;
	}
}
