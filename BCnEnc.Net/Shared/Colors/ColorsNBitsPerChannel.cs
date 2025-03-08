using System.Numerics;

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

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(
			R / 255f,
			G / 255f,
			B / 255f);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		R = ByteHelper.ClampToByte(color.r * 255f);
		G = ByteHelper.ClampToByte(color.g * 255f);
		B = ByteHelper.ClampToByte(color.b * 255f);
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

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(
			R / 255f,
			G / 255f,
			B / 255f);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		R = ByteHelper.ClampToByte(color.r * 255f);
		G = ByteHelper.ClampToByte(color.g * 255f);
		B = ByteHelper.ClampToByte(color.b * 255f);
	}
}
