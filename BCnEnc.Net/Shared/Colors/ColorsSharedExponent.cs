using System;

namespace BCnEncoder.Shared.Colors;

public struct ColorRgbe : IColor<ColorRgbe>
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

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		if (e == 0)
		{
			return new ColorRgbaFloat(0, 0, 0);
		}
		else
		{
			var (rf, gf, bf) = ColorUtils.SharedExponentToRgb((r, g, b, e), 8, 128);
			return new ColorRgbaFloat(rf, gf, bf);
		}
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		var max = MathF.Max(color.b, MathF.Max(color.g, color.r));
		if (max <= 1e-32f)
		{
			r = g = b = e = 0;
		}
		else
		{
			var (ri, gi, bi, ei) = ColorUtils.RgbToSharedExponent(
				red: color.r,
				green:color.g,
				blue:color.b,
				mantissaBits: 8,
				bias: 128,
				exponentMax: 255);

			r = (byte)ri;
			g = (byte)gi;
			b = (byte)bi;
			e = (byte)ei;
		}
	}
}

public struct ColorXyze : IColor<ColorXyze>
{
	public byte x;
	public byte y;
	public byte z;
	public byte e;

	public ColorXyze(byte x, byte y, byte z, byte e)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.e = e;
	}

	public bool Equals(ColorXyze other)
	{
		return x == other.x && y == other.y && z == other.z && e == other.e;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorXyze other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = x.GetHashCode();
			hashCode = (hashCode * 397) ^ y.GetHashCode();
			hashCode = (hashCode * 397) ^ z.GetHashCode();
			hashCode = (hashCode * 397) ^ e.GetHashCode();
			return hashCode;
		}
	}

	public static bool operator ==(ColorXyze left, ColorXyze right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorXyze left, ColorXyze right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return $"{nameof(x)}: {x}, {nameof(y)}: {y}, {nameof(z)}: {z}, {nameof(e)}: {e}";
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		if (e == 0)
		{
			return new ColorRgbaFloat(0, 0, 0);
		}
		else
		{
			// First convert to XYZ using the shared exponent
			var (xf, yf, zf) = ColorUtils.SharedExponentToRgb((x, y, z, e), 8, 128);

			// Then convert from XYZ to RGB
			return new ColorXyz(xf, yf, zf).ToColorRgbaFloat();
		}
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		// First convert from RGB to XYZ
		ColorXyz xyz = color.As<ColorXyz>();

		// Handle zero case
		if (xyz.x <= 1e-32f && xyz.y <= 1e-32f && xyz.z <= 1e-32f)
		{
			x = y = z = e = 0;
			return;
		}

		// Use shared exponent helper to convert from XYZ to shared exponent format
		var (xi, yi, zi, ei) = ColorUtils.RgbToSharedExponent(
			red: xyz.x,
			green: xyz.y,
			blue: xyz.z,
			mantissaBits: 8,
			bias: 128,
			exponentMax: 255);

		x = (byte)xi;
		y = (byte)yi;
		z = (byte)zi;
		e = (byte)ei;
	}
}

/// <summary>
/// Represents a packed R9G9B9E5 shared exponent color.
/// This format uses 9 bits each for red, green, and blue mantissas, with a shared 5-bit exponent.
/// </summary>
public struct ColorR9G9B9E5 : IColor<ColorR9G9B9E5>
{
	// The packed 32-bit value containing all components
	public uint packedValue;

	/// <summary>
	/// Creates a new ColorR9G9B9E5 from a 32-bit packed value.
	/// </summary>
	public ColorR9G9B9E5(uint packedValue)
	{
		this.packedValue = packedValue;
	}

	/// <summary>
	/// Creates a new ColorR9G9B9E5 from individual floating point values.
	/// </summary>
	public ColorR9G9B9E5(float r, float g, float b)
	{
		// Extract mantissas and shared exponent using ColorUtils helper
		var (rm, gm, bm, exp) = ColorUtils.RgbToSharedExponent(
			red: r,
			green: g,
			blue: b,
			mantissaBits: 9,
			bias: 15,
			exponentMax: 31);

		// Clamp mantissas to 9 bits
		rm = Math.Min(rm, 0x1FF);
		gm = Math.Min(gm, 0x1FF);
		bm = Math.Min(bm, 0x1FF);

		// Pack into 32-bit value
		packedValue = rm | (gm << 9) | (bm << 18) | (exp << 27);
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		// No need to convert if all values are zero
		if (packedValue == 0)
		{
			return new ColorRgbaFloat(0, 0, 0);
		}

		// Extract components
		uint rm = packedValue & 0x1FF;
		uint gm = (packedValue >> 9) & 0x1FF;
		uint bm = (packedValue >> 18) & 0x1FF;
		uint exp = (packedValue >> 27) & 0x1F;

		// Convert using ColorUtils helper
		var (rf, gf, bf) = ColorUtils.SharedExponentToRgb((rm, gm, bm, exp), 9, 15);
		return new ColorRgbaFloat(rf, gf, bf);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		// Use constructor that handles the conversion
		ColorR9G9B9E5 newColor = new ColorR9G9B9E5(color.r, color.g, color.b);
		packedValue = newColor.packedValue;
	}

	/// <inheritdoc />
	public bool Equals(ColorR9G9B9E5 other)
	{
		return packedValue == other.packedValue;
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorR9G9B9E5 other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return packedValue.GetHashCode();
	}

	public static bool operator ==(ColorR9G9B9E5 left, ColorR9G9B9E5 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorR9G9B9E5 left, ColorR9G9B9E5 right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		ColorRgbaFloat rgbaFloat = ToColorRgbaFloat();
		return $"R: {rgbaFloat.r}, G: {rgbaFloat.g}, B: {rgbaFloat.b}";
	}
}
