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
			(r, g, b, e) = ColorUtils.RgbToSharedExponent(
				red: color.r,
				green:color.g,
				blue:color.b,
				mantissaBits: 8,
				bias: 128,
				exponentMax: 255);
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
			var fexp = MathHelper.LdExp(1f, e - (128 + 8));

			return new ColorXyz(
				(float)((x + 0.5) * fexp),
				(float)((y + 0.5) * fexp),
				(float)((z + 0.5) * fexp)
			).ToColorRgbaFloat();
		}
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		ColorXyz xyz = color.As<ColorXyz>();
		var max = MathF.Max(xyz.x, MathF.Max(xyz.y, xyz.z));
		if (max <= 1e-32f)
		{
			x = y = z = e = 0;
		}
		else
		{
			max = (float)(MathHelper.FrExp(max, out var exponent) * 255.9999f / max);

			x = (byte)(max * xyz.x);
			y = (byte)(max * xyz.y);
			z = (byte)(max * xyz.z);
			e = (byte)(exponent + 128);
		}
	}
}
