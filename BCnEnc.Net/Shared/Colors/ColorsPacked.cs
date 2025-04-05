using System.Runtime.InteropServices;

namespace BCnEncoder.Shared.Colors;

using static ColorBitConversionHelpers;

[StructLayout(LayoutKind.Sequential)]
public struct ColorB4G4R4A4Packed : IColorRgba<ColorB4G4R4A4Packed, ushort>, IColorPacked<ushort>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "B4G4R4A4";

	ushort IColorPacked<ushort>.Data
	{
		get => data;
		set => data = value;
	}

	public bool Equals(ColorB4G4R4A4Packed other)
	{
		return data == other.data;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorB4G4R4A4Packed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorB4G4R4A4Packed left, ColorB4G4R4A4Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorB4G4R4A4Packed left, ColorB4G4R4A4Packed right)
	{
		return !left.Equals(right);
	}

	private const ushort BlueMask = 0x000F;
	private const int BlueShift = 0;
	private const ushort GreenMask = 0x00F0;
	private const int GreenShift = 4;
	private const ushort RedMask = 0x0F00;
	private const int RedShift = 8;
	private const ushort AlphaMask = 0xF000;
	private const int AlphaShift = 12;

	private const ushort RedMaxValue = 15;
	private const ushort GreenMaxValue = 15;
	private const ushort BlueMaxValue = 15;
	private const ushort AlphaMaxValue = 15;

	public ushort data;

	public float R
	{
		readonly get => Unorm4ToFloat(RawR);
		set => RawR = FloatToUnorm4(value);
	}

	public float G
	{
		readonly get => Unorm4ToFloat(RawG);
		set => RawG = FloatToUnorm4(value);
	}

	public float B
	{
		readonly get => Unorm4ToFloat(RawB);
		set => RawB = FloatToUnorm4(value);
	}

	public float A
	{
		readonly get => Unorm4ToFloat(RawA);
		set => RawA = FloatToUnorm4(value);
	}

	public ushort RawR
	{
		readonly get => (ushort)((data & RedMask) >> RedShift);
		set
		{
			if (value > RedMaxValue) value = RedMaxValue;
			data = (ushort)(data & ~RedMask);
			data = (ushort)(data | (value << RedShift));
		}
	}

	public ushort RawG
	{
		readonly get => (ushort)((data & GreenMask) >> GreenShift);
		set
		{
			if (value > GreenMaxValue) value = GreenMaxValue;
			data = (ushort)(data & ~GreenMask);
			data = (ushort)(data | (value << GreenShift));
		}
	}

	public ushort RawB
	{
		readonly get => (ushort)((data & BlueMask) >> BlueShift);
		set
		{
			if (value > BlueMaxValue) value = BlueMaxValue;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | (value << BlueShift));
		}
	}

	public ushort RawA
	{
		readonly get => (ushort)((data & AlphaMask) >> AlphaShift);
		set
		{
			if (value > AlphaMaxValue) value = AlphaMaxValue;
			data = (ushort)(data & ~AlphaMask);
			data = (ushort)(data | (value << AlphaShift));
		}
	}

	public ColorB4G4R4A4Packed(float r, float g, float b, float a)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public override string ToString()
	{
		return $"r : {R} g : {G} b : {B} a : {A}";
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorR4G4B4A4Packed : IColorRgba<ColorR4G4B4A4Packed, ushort>, IColorPacked<ushort>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "R4G4B4A4";

	ushort IColorPacked<ushort>.Data
	{
		get => data;
		set => data = value;
	}

	public bool Equals(ColorR4G4B4A4Packed other)
	{
		return data == other.data;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorR4G4B4A4Packed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorR4G4B4A4Packed left, ColorR4G4B4A4Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorR4G4B4A4Packed left, ColorR4G4B4A4Packed right)
	{
		return !left.Equals(right);
	}

	private const ushort RedMask = 0x000F;
	private const int RedShift = 0;
	private const ushort GreenMask = 0x00F0;
	private const int GreenShift = 4;
	private const ushort BlueMask = 0x0F00;
	private const int BlueShift = 8;
	private const ushort AlphaMask = 0xF000;
	private const int AlphaShift = 12;

	private const ushort RedMaxValue = 15;
	private const ushort GreenMaxValue = 15;
	private const ushort BlueMaxValue = 15;
	private const ushort AlphaMaxValue = 15;

	public ushort data;

	public float R
	{
		readonly get => Unorm4ToFloat(RawR);
		set => RawR = FloatToUnorm4(value);
	}

	public float G
	{
		readonly get => Unorm4ToFloat(RawG);
		set => RawG = FloatToUnorm4(value);
	}

	public float B
	{
		readonly get => Unorm4ToFloat(RawB);
		set => RawB = FloatToUnorm4(value);
	}

	public float A
	{
		readonly get => Unorm4ToFloat(RawA);
		set => RawA = FloatToUnorm4(value);
	}

	public ushort RawR
	{
		readonly get => (ushort)((data & RedMask) >> RedShift);
		set
		{
			if (value > RedMaxValue) value = RedMaxValue;
			data = (ushort)(data & ~RedMask);
			data = (ushort)(data | (value << RedShift));
		}
	}

	public ushort RawG
	{
		readonly get => (ushort)((data & GreenMask) >> GreenShift);
		set
		{
			if (value > GreenMaxValue) value = GreenMaxValue;
			data = (ushort)(data & ~GreenMask);
			data = (ushort)(data | (value << GreenShift));
		}
	}

	public ushort RawB
	{
		readonly get => (ushort)((data & BlueMask) >> BlueShift);
		set
		{
			if (value > BlueMaxValue) value = BlueMaxValue;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | (value << BlueShift));
		}
	}

	public ushort RawA
	{
		readonly get => (ushort)((data & AlphaMask) >> AlphaShift);
		set
		{
			if (value > AlphaMaxValue) value = AlphaMaxValue;
			data = (ushort)(data & ~AlphaMask);
			data = (ushort)(data | (value << AlphaShift));
		}
	}

	public ColorR4G4B4A4Packed(float r, float g, float b, float a)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public override string ToString()
	{
		return $"r : {R} g : {G} b : {B} a : {A}";
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorA4B4G4R4Packed : IColorRgba<ColorA4B4G4R4Packed, ushort>, IColorPacked<ushort>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "A4B4G4R4";

	ushort IColorPacked<ushort>.Data
	{
		get => data;
		set => data = value;
	}

	public bool Equals(ColorA4B4G4R4Packed other)
	{
		return data == other.data;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorA4B4G4R4Packed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorA4B4G4R4Packed left, ColorA4B4G4R4Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorA4B4G4R4Packed left, ColorA4B4G4R4Packed right)
	{
		return !left.Equals(right);
	}

	private const ushort AlphaMask = 0x000F;
	private const int AlphaShift = 0;
	private const ushort BlueMask = 0x00F0;
	private const int BlueShift = 4;
	private const ushort GreenMask = 0x0F00;
	private const int GreenShift = 8;
	private const ushort RedMask = 0xF000;
	private const int RedShift = 12;

	private const ushort RedMaxValue = 15;
	private const ushort GreenMaxValue = 15;
	private const ushort BlueMaxValue = 15;
	private const ushort AlphaMaxValue = 15;

	public ushort data;

	public float R
	{
		readonly get => Unorm4ToFloat(RawR);
		set => RawR = FloatToUnorm4(value);
	}

	public float G
	{
		readonly get => Unorm4ToFloat(RawG);
		set => RawG = FloatToUnorm4(value);
	}

	public float B
	{
		readonly get => Unorm4ToFloat(RawB);
		set => RawB = FloatToUnorm4(value);
	}

	public float A
	{
		readonly get => Unorm4ToFloat(RawA);
		set => RawA = FloatToUnorm4(value);
	}

	public ushort RawR
	{
		readonly get => (ushort)((data & RedMask) >> RedShift);
		set
		{
			if (value > RedMaxValue) value = RedMaxValue;
			data = (ushort)(data & ~RedMask);
			data = (ushort)(data | (value << RedShift));
		}
	}

	public ushort RawG
	{
		readonly get => (ushort)((data & GreenMask) >> GreenShift);
		set
		{
			if (value > GreenMaxValue) value = GreenMaxValue;
			data = (ushort)(data & ~GreenMask);
			data = (ushort)(data | (value << GreenShift));
		}
	}

	public ushort RawB
	{
		readonly get => (ushort)((data & BlueMask) >> BlueShift);
		set
		{
			if (value > BlueMaxValue) value = BlueMaxValue;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | (value << BlueShift));
		}
	}

	public ushort RawA
	{
		readonly get => (ushort)((data & AlphaMask) >> AlphaShift);
		set
		{
			if (value > AlphaMaxValue) value = AlphaMaxValue;
			data = (ushort)(data & ~AlphaMask);
			data = (ushort)(data | (value << AlphaShift));
		}
	}

	public ColorA4B4G4R4Packed(float r, float g, float b, float a)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public override string ToString()
	{
		return $"r : {R} g : {G} b : {B} a : {A}";
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorA4R4G4B4Packed : IColorRgba<ColorA4R4G4B4Packed, ushort>, IColorPacked<ushort>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "A4R4G4B4";

	ushort IColorPacked<ushort>.Data
	{
		get => data;
		set => data = value;
	}

	public bool Equals(ColorA4R4G4B4Packed other)
	{
		return data == other.data;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorA4R4G4B4Packed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorA4R4G4B4Packed left, ColorA4R4G4B4Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorA4R4G4B4Packed left, ColorA4R4G4B4Packed right)
	{
		return !left.Equals(right);
	}

	private const ushort AlphaMask = 0x000F;
	private const int AlphaShift = 0;
	private const ushort RedMask = 0x00F0;
	private const int RedShift = 4;
	private const ushort GreenMask = 0x0F00;
	private const int GreenShift = 8;
	private const ushort BlueMask = 0xF000;
	private const int BlueShift = 12;

	private const ushort RedMaxValue = 15;
	private const ushort GreenMaxValue = 15;
	private const ushort BlueMaxValue = 15;
	private const ushort AlphaMaxValue = 15;

	public ushort data;

	public float R
	{
		readonly get => Unorm4ToFloat(RawR);
		set => RawR = FloatToUnorm4(value);
	}

	public float G
	{
		readonly get => Unorm4ToFloat(RawG);
		set => RawG = FloatToUnorm4(value);
	}

	public float B
	{
		readonly get => Unorm4ToFloat(RawB);
		set => RawB = FloatToUnorm4(value);
	}

	public float A
	{
		readonly get => Unorm4ToFloat(RawA);
		set => RawA = FloatToUnorm4(value);
	}

	public ushort RawR
	{
		readonly get => (ushort)((data & RedMask) >> RedShift);
		set
		{
			if (value > RedMaxValue) value = RedMaxValue;
			data = (ushort)(data & ~RedMask);
			data = (ushort)(data | (value << RedShift));
		}
	}

	public ushort RawG
	{
		readonly get => (ushort)((data & GreenMask) >> GreenShift);
		set
		{
			if (value > GreenMaxValue) value = GreenMaxValue;
			data = (ushort)(data & ~GreenMask);
			data = (ushort)(data | (value << GreenShift));
		}
	}

	public ushort RawB
	{
		readonly get => (ushort)((data & BlueMask) >> BlueShift);
		set
		{
			if (value > BlueMaxValue) value = BlueMaxValue;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | (value << BlueShift));
		}
	}

	public ushort RawA
	{
		readonly get => (ushort)((data & AlphaMask) >> AlphaShift);
		set
		{
			if (value > AlphaMaxValue) value = AlphaMaxValue;
			data = (ushort)(data & ~AlphaMask);
			data = (ushort)(data | (value << AlphaShift));
		}
	}

	public ColorA4R4G4B4Packed(float r, float g, float b, float a)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public override string ToString()
	{
		return $"r : {R} g : {G} b : {B} a : {A}";
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorB5G5R5M1Packed : IColorRgb<ColorB5G5R5M1Packed, ushort>, IColorPacked<ushort>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "B5G5R5M1";

	ushort IColorPacked<ushort>.Data
	{
		readonly get => data;
		set => data = value;
	}

	public readonly bool Equals(ColorB5G5R5M1Packed other)
	{
		return data == other.data;
	}

	public readonly override bool Equals(object obj)
	{
		return obj is ColorB5G5R5M1Packed other && Equals(other);
	}

	public readonly override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorB5G5R5M1Packed left, ColorB5G5R5M1Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorB5G5R5M1Packed left, ColorB5G5R5M1Packed right)
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

	public ushort RawR
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

	public ushort RawG
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

	public ushort RawB
	{
		readonly get => (ushort)(data & BlueMask);
		set
		{
			if (value > 31) value = 31;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | value);
		}
	}

	public ColorB5G5R5M1Packed(float r, float g, float b)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
	}

	public readonly override string ToString()
	{
		return $"r : {R} g : {G} b : {B}";
	}

	/// <inheritdoc />
	public readonly ColorRgbaFloat ToColorRgbaFloat()
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorB5G6R5Packed : IColorRgb<ColorB5G6R5Packed, ushort>, IColorPacked<ushort>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "B5G6R5";

	ushort IColorPacked<ushort>.Data
	{
		readonly get => data;
		set => data = value;
	}

	public readonly bool Equals(ColorB5G6R5Packed other)
	{
		return data == other.data;
	}

	public readonly override bool Equals(object obj)
	{
		return obj is ColorB5G6R5Packed other && Equals(other);
	}

	public readonly override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorB5G6R5Packed left, ColorB5G6R5Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorB5G6R5Packed left, ColorB5G6R5Packed right)
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

	public ushort RawR
	{
		readonly get => (ushort)((data & RedMask) >> RedShift);
		set
		{
			if (value > 31) value = 31;
			data = (ushort)(data & ~RedMask);
			data = (ushort)(data | (value << RedShift));
		}
	}

	public ushort RawG
	{
		readonly get => (ushort)((data & GreenMask) >> GreenShift);
		set
		{
			if (value > 63) value = 63;
			data = (ushort)(data & ~GreenMask);
			data = (ushort)(data | (value << GreenShift));
		}
	}

	public ushort RawB
	{
		readonly get => (ushort)(data & BlueMask);
		set
		{
			if (value > 31) value = 31;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | value);
		}
	}

	public ColorB5G6R5Packed(float r, float g, float b)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
	}

	public readonly override string ToString()
	{
		return $"r : {R} g : {G} b : {B}";
	}

	/// <inheritdoc />
	public readonly ColorRgbaFloat ToColorRgbaFloat()
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorR5G6B5Packed : IColorRgb<ColorR5G6B5Packed, ushort>, IColorPacked<ushort>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "R5G6B5";

	ushort IColorPacked<ushort>.Data
	{
		get => data;
		set => data = value;
	}

	public bool Equals(ColorR5G6B5Packed other)
	{
		return data == other.data;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorR5G6B5Packed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorR5G6B5Packed left, ColorR5G6B5Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorR5G6B5Packed left, ColorR5G6B5Packed right)
	{
		return !left.Equals(right);
	}

	private const ushort RedMask = 0x001F;
	private const int RedShift = 0;
	private const ushort GreenMask = 0x07E0;
	private const int GreenShift = 5;
	private const ushort BlueMask = 0xF800;
	private const int BlueShift = 11;

	private const ushort RedMaxValue = 31;
	private const ushort GreenMaxValue = 63;
	private const ushort BlueMaxValue = 31;

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


	public ushort RawR
	{
		readonly get => (ushort)((data & RedMask) >> RedShift);
		set
		{
			if (value > RedMaxValue) value = RedMaxValue;
			data = (ushort)(data & ~RedMask);
			data = (ushort)(data | (value << RedShift));
		}
	}

	public ushort RawG
	{
		readonly get => (ushort)((data & GreenMask) >> GreenShift);
		set
		{
			if (value > GreenMaxValue) value = GreenMaxValue;
			data = (ushort)(data & ~GreenMask);
			data = (ushort)(data | (value << GreenShift));
		}
	}

	public ushort RawB
	{
		readonly get => (ushort)((data & BlueMask) >> BlueShift);
		set
		{
			if (value > BlueMaxValue) value = BlueMaxValue;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | (value << BlueShift));
		}
	}

	public ColorR5G6B5Packed(float r, float g, float b)
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorB5G5R5A1Packed : IColorRgba<ColorB5G5R5A1Packed, ushort>, IColorPacked<ushort>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "B5G5R5A1";

	ushort IColorPacked<ushort>.Data
	{
		get => data;
		set => data = value;
	}

	public bool Equals(ColorB5G5R5A1Packed other)
	{
		return data == other.data;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorB5G5R5A1Packed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorB5G5R5A1Packed left, ColorB5G5R5A1Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorB5G5R5A1Packed left, ColorB5G5R5A1Packed right)
	{
		return !left.Equals(right);
	}

	private const ushort BlueMask = 0x001F;
	private const int BlueShift = 0;
	private const ushort GreenMask = 0x03E0;
	private const int GreenShift = 5;
	private const ushort RedMask = 0x7C00;
	private const int RedShift = 10;
	private const ushort AlphaMask = 0x8000;
	private const int AlphaShift = 15;

	private const ushort RedMaxValue = 31;
	private const ushort GreenMaxValue = 31;
	private const ushort BlueMaxValue = 31;
	private const ushort AlphaMaxValue = 1;

	public ushort data;

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

	public float A
	{
		readonly get => Unorm1ToFloat(RawA);
		set => RawA = FloatToUnorm1(value);
	}

	public ushort RawR
	{
		readonly get => (ushort)((data & RedMask) >> RedShift);
		set
		{
			if (value > RedMaxValue) value = RedMaxValue;
			data = (ushort)(data & ~RedMask);
			data = (ushort)(data | (value << RedShift));
		}
	}

	public ushort RawG
	{
		readonly get => (ushort)((data & GreenMask) >> GreenShift);
		set
		{
			if (value > GreenMaxValue) value = GreenMaxValue;
			data = (ushort)(data & ~GreenMask);
			data = (ushort)(data | (value << GreenShift));
		}
	}

	public ushort RawB
	{
		readonly get => (ushort)((data & BlueMask) >> BlueShift);
		set
		{
			if (value > BlueMaxValue) value = BlueMaxValue;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | (value << BlueShift));
		}
	}

	public ushort RawA
	{
		readonly get => (ushort)((data & AlphaMask) >> AlphaShift);
		set
		{
			if (value > AlphaMaxValue) value = AlphaMaxValue;
			data = (ushort)(data & ~AlphaMask);
			data = (ushort)(data | (value << AlphaShift));
		}
	}

	public ColorB5G5R5A1Packed(float r, float g, float b, float a)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public override string ToString()
	{
		return $"r : {R} g : {G} b : {B} a : {A}";
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorR5G5B5A1Packed : IColorRgba<ColorR5G5B5A1Packed, ushort>, IColorPacked<ushort>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "R5G5B5A1";

	ushort IColorPacked<ushort>.Data
	{
		get => data;
		set => data = value;
	}

	public bool Equals(ColorR5G5B5A1Packed other)
	{
		return data == other.data;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorR5G5B5A1Packed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorR5G5B5A1Packed left, ColorR5G5B5A1Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorR5G5B5A1Packed left, ColorR5G5B5A1Packed right)
	{
		return !left.Equals(right);
	}

	private const ushort RedMask = 0x001F;
	private const int RedShift = 0;
	private const ushort GreenMask = 0x03E0;
	private const int GreenShift = 5;
	private const ushort BlueMask = 0x7C00;
	private const int BlueShift = 10;
	private const ushort AlphaMask = 0x8000;
	private const int AlphaShift = 15;

	private const ushort RedMaxValue = 31;
	private const ushort GreenMaxValue = 31;
	private const ushort BlueMaxValue = 31;
	private const ushort AlphaMaxValue = 1;

	public ushort data;

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

	public float A
	{
		readonly get => Unorm1ToFloat(RawA);
		set => RawA = FloatToUnorm1(value);
	}

	public ushort RawR
	{
		readonly get => (ushort)((data & RedMask) >> RedShift);
		set
		{
			if (value > RedMaxValue) value = RedMaxValue;
			data = (ushort)(data & ~RedMask);
			data = (ushort)(data | (value << RedShift));
		}
	}

	public ushort RawG
	{
		readonly get => (ushort)((data & GreenMask) >> GreenShift);
		set
		{
			if (value > GreenMaxValue) value = GreenMaxValue;
			data = (ushort)(data & ~GreenMask);
			data = (ushort)(data | (value << GreenShift));
		}
	}

	public ushort RawB
	{
		readonly get => (ushort)((data & BlueMask) >> BlueShift);
		set
		{
			if (value > BlueMaxValue) value = BlueMaxValue;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | (value << BlueShift));
		}
	}

	public ushort RawA
	{
		readonly get => (ushort)((data & AlphaMask) >> AlphaShift);
		set
		{
			if (value > AlphaMaxValue) value = AlphaMaxValue;
			data = (ushort)(data & ~AlphaMask);
			data = (ushort)(data | (value << AlphaShift));
		}
	}

	public ColorR5G5B5A1Packed(float r, float g, float b, float a)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public override string ToString()
	{
		return $"r : {R} g : {G} b : {B} a : {A}";
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorA1B5G5R5Packed : IColorRgba<ColorA1B5G5R5Packed, ushort>, IColorPacked<ushort>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "A1B5G5R5";

	ushort IColorPacked<ushort>.Data
	{
		get => data;
		set => data = value;
	}

	public bool Equals(ColorA1B5G5R5Packed other)
	{
		return data == other.data;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorA1B5G5R5Packed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorA1B5G5R5Packed left, ColorA1B5G5R5Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorA1B5G5R5Packed left, ColorA1B5G5R5Packed right)
	{
		return !left.Equals(right);
	}

	private const ushort AlphaMask = 0x0001;
	private const int AlphaShift = 0;
	private const ushort BlueMask = 0x003E;
	private const int BlueShift = 1;
	private const ushort GreenMask = 0x07C0;
	private const int GreenShift = 6;
	private const ushort RedMask = 0xF800;
	private const int RedShift = 11;

	private const ushort RedMaxValue = 31;
	private const ushort GreenMaxValue = 31;
	private const ushort BlueMaxValue = 31;
	private const ushort AlphaMaxValue = 1;

	public ushort data;

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

	public float A
	{
		readonly get => Unorm1ToFloat(RawA);
		set => RawA = FloatToUnorm1(value);
	}

	public ushort RawR
	{
		readonly get => (ushort)((data & RedMask) >> RedShift);
		set
		{
			if (value > RedMaxValue) value = RedMaxValue;
			data = (ushort)(data & ~RedMask);
			data = (ushort)(data | (value << RedShift));
		}
	}

	public ushort RawG
	{
		readonly get => (ushort)((data & GreenMask) >> GreenShift);
		set
		{
			if (value > GreenMaxValue) value = GreenMaxValue;
			data = (ushort)(data & ~GreenMask);
			data = (ushort)(data | (value << GreenShift));
		}
	}

	public ushort RawB
	{
		readonly get => (ushort)((data & BlueMask) >> BlueShift);
		set
		{
			if (value > BlueMaxValue) value = BlueMaxValue;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | (value << BlueShift));
		}
	}

	public ushort RawA
	{
		readonly get => (ushort)((data & AlphaMask) >> AlphaShift);
		set
		{
			if (value > AlphaMaxValue) value = AlphaMaxValue;
			data = (ushort)(data & ~AlphaMask);
			data = (ushort)(data | (value << AlphaShift));
		}
	}

	public ColorA1B5G5R5Packed(float r, float g, float b, float a)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public override string ToString()
	{
		return $"r : {R} g : {G} b : {B} a : {A}";
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorA1R5G5B5Packed : IColorRgba<ColorA1R5G5B5Packed, ushort>, IColorPacked<ushort>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "A1R5G5B5";

	ushort IColorPacked<ushort>.Data
	{
		get => data;
		set => data = value;
	}

	public bool Equals(ColorA1R5G5B5Packed other)
	{
		return data == other.data;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorA1R5G5B5Packed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorA1R5G5B5Packed left, ColorA1R5G5B5Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorA1R5G5B5Packed left, ColorA1R5G5B5Packed right)
	{
		return !left.Equals(right);
	}

	private const ushort AlphaMask = 0x0001;
	private const int AlphaShift = 0;
	private const ushort RedMask = 0x003E;
	private const int RedShift = 1;
	private const ushort GreenMask = 0x07C0;
	private const int GreenShift = 6;
	private const ushort BlueMask = 0xF800;
	private const int BlueShift = 11;

	private const ushort RedMaxValue = 31;
	private const ushort GreenMaxValue = 31;
	private const ushort BlueMaxValue = 31;
	private const ushort AlphaMaxValue = 1;

	public ushort data;

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

	public float A
	{
		readonly get => Unorm1ToFloat(RawA);
		set => RawA = FloatToUnorm1(value);
	}

	public ushort RawR
	{
		readonly get => (ushort)((data & RedMask) >> RedShift);
		set
		{
			if (value > RedMaxValue) value = RedMaxValue;
			data = (ushort)(data & ~RedMask);
			data = (ushort)(data | (value << RedShift));
		}
	}

	public ushort RawG
	{
		readonly get => (ushort)((data & GreenMask) >> GreenShift);
		set
		{
			if (value > GreenMaxValue) value = GreenMaxValue;
			data = (ushort)(data & ~GreenMask);
			data = (ushort)(data | (value << GreenShift));
		}
	}

	public ushort RawB
	{
		readonly get => (ushort)((data & BlueMask) >> BlueShift);
		set
		{
			if (value > BlueMaxValue) value = BlueMaxValue;
			data = (ushort)(data & ~BlueMask);
			data = (ushort)(data | (value << BlueShift));
		}
	}

	public ushort RawA
	{
		readonly get => (ushort)((data & AlphaMask) >> AlphaShift);
		set
		{
			if (value > AlphaMaxValue) value = AlphaMaxValue;
			data = (ushort)(data & ~AlphaMask);
			data = (ushort)(data | (value << AlphaShift));
		}
	}

	public ColorA1R5G5B5Packed(float r, float g, float b, float a)
	{
		data = 0;
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public override string ToString()
	{
		return $"r : {R} g : {G} b : {B} a : {A}";
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorR10G10B10A2Packed : IColorRgba<ColorR10G10B10A2Packed, uint>, IColorPacked<uint>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "R10G10B10A2";

	uint IColorPacked<uint>.Data
	{
		get => data;
		set => data = value;
	}

	public bool Equals(ColorR10G10B10A2Packed other)
	{
		return data == other.data;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorR10G10B10A2Packed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorR10G10B10A2Packed left, ColorR10G10B10A2Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorR10G10B10A2Packed left, ColorR10G10B10A2Packed right)
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

	public ColorR10G10B10A2Packed(float r, float g, float b, float a)
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

[StructLayout(LayoutKind.Sequential)]
public struct ColorB10G10R10A2Packed : IColorRgba<ColorB10G10R10A2Packed, uint>, IColorPacked<uint>
{
	/// <inheritdoc />
	public static PackedRgbaMask ChannelMask { get; } = "B10G10R10A2";

	uint IColorPacked<uint>.Data
	{
		get => data;
		set => data = value;
	}

	public bool Equals(ColorB10G10R10A2Packed other)
	{
		return data == other.data;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorB10G10R10A2Packed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return data.GetHashCode();
	}

	public static bool operator ==(ColorB10G10R10A2Packed left, ColorB10G10R10A2Packed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorB10G10R10A2Packed left, ColorB10G10R10A2Packed right)
	{
		return !left.Equals(right);
	}

	private const uint RedMask = 0x3FF00000;
	private const int RedShift = 20;
	private const uint GreenMask = 0x000FFC00;
	private const int GreenShift = 10;
	private const uint BlueMask = 0x000003FF;
	private const int BlueShift = 00;
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

	public ColorB10G10R10A2Packed(float r, float g, float b, float a)
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
