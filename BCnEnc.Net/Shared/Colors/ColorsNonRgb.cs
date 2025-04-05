using System;
using System.Numerics;

namespace BCnEncoder.Shared.Colors;

internal struct ColorYCbCr : IColor<ColorYCbCr>
{
	public float y;
	public float cb;
	public float cr;

	public ColorYCbCr(float y, float cb, float cr)
	{
		this.y = y;
		this.cb = cb;
		this.cr = cr;
	}

	public override string ToString()
	{
		ColorSpace.YCbCrToLrgb(y, cb, cr, out var r, out var g, out var b);

		return $"r : {r * 255} g : {g * 255} b : {b * 255}";
	}

	public float CalcDistWeighted(ColorYCbCr other, float yWeight = 4)
	{
		var dy = (y - other.y) * (y - other.y) * yWeight;
		var dcb = (cb - other.cb) * (cb - other.cb);
		var dcr = (cr - other.cr) * (cr - other.cr);

		return MathF.Sqrt(dy + dcb + dcr);
	}

	public static ColorYCbCr operator +(ColorYCbCr left, ColorYCbCr right)
	{
		return new ColorYCbCr(
			left.y + right.y,
			left.cb + right.cb,
			left.cr + right.cr);
	}

	public static ColorYCbCr operator /(ColorYCbCr left, float right)
	{
		return new ColorYCbCr(
			left.y / right,
			left.cb / right,
			left.cr / right);
	}


	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		ColorSpace.YCbCrToLrgb(y, cb, cr, out var r, out var g, out var b);
		return new ColorRgbaFloat(r, g, b);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		ColorSpace.LrgbToYCbCr(color.r, color.g, color.b, out y, out cb, out cr);
	}

	/// <inheritdoc />
	public bool Equals(ColorYCbCr other)
	{
		return y.Equals(other.y) && cb.Equals(other.cb) && cr.Equals(other.cr);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorYCbCr other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(y, cb, cr);
	}
}

internal struct ColorYCbCrAlpha : IColor<ColorYCbCrAlpha>
{
	public float y;
	public float cb;
	public float cr;
	public float a;

	public ColorYCbCrAlpha(float y, float cb, float cr, float a)
	{
		this.y = y;
		this.cb = cb;
		this.cr = cr;
		this.a = a;
	}

	/// <inheritdoc />
	public bool Equals(ColorYCbCrAlpha other)
	{
		return
			y == other.y &&
			cb == other.cb &&
			cr == other.cr &&
			a == other.a;
	}

	public override string ToString()
	{
		ColorSpace.YCbCrToLrgb(y, cb, cr, out var r, out var g, out var b);

		return $"r : {r * 255} g : {g * 255} b : {b * 255}";
	}

	public float CalcDistWeighted(ColorYCbCrAlpha other, float yWeight = 4, float aWeight = 1)
	{
		var dy = (y - other.y) * (y - other.y) * yWeight;
		var dcb = (cb - other.cb) * (cb - other.cb);
		var dcr = (cr - other.cr) * (cr - other.cr);
		var da = (a - other.a) * (a - other.a) * aWeight;

		return MathF.Sqrt(dy + dcb + dcr + da);
	}

	public static ColorYCbCrAlpha operator +(ColorYCbCrAlpha left, ColorYCbCrAlpha right)
	{
		return new ColorYCbCrAlpha(
			left.y + right.y,
			left.cb + right.cb,
			left.cr + right.cr,
			left.a + right.a);
	}

	public static ColorYCbCrAlpha operator /(ColorYCbCrAlpha left, float right)
	{
		return new ColorYCbCrAlpha(
			left.y / right,
			left.cb / right,
			left.cr / right,
			left.a / right);
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		ColorSpace.YCbCrToLrgb(y, cb, cr, out var r, out var g, out var b);
		return new ColorRgbaFloat(r, g, b, a);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		ColorSpace.LrgbToYCbCr(color.r, color.g, color.b, out y, out cb, out cr);
		a = color.a;
	}
}

internal struct ColorXyz : IColor<ColorXyz>
{
	public float x;
	public float y;
	public float z;

	public ColorXyz(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		ColorSpace.XyzToLrgb(x, y, z, out var r, out var g, out var b);
		return new ColorRgbaFloat(r, g, b);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		ColorSpace.LrgbToXyz(color.r, color.g, color.b, out x, out y, out z);
	}

	/// <inheritdoc />
	public bool Equals(ColorXyz other)
	{
		return
			x == other.x &&
			y == other.y &&
			z == other.z;
	}
}

internal struct ColorLab : IColor<ColorLab>
{
	public float l;
	public float a;
	public float b;

	public ColorLab(float l, float a, float b)
	{
		this.l = l;
		this.a = a;
		this.b = b;
	}

	private static ColorLab XyzToLab(ColorXyz xyz)
	{
		var refX = 95.047f; // Observer= 2°, Illuminant= D65
		var refY = 100.000f;
		var refZ = 108.883f;

		var x = PivotXyz(xyz.x * 100 / refX);
		var y = PivotXyz(xyz.y * 100 / refY);
		var z = PivotXyz(xyz.z * 100 / refZ);

		return new ColorLab(116 * y - 16, 500 * (x - y), 200 * (y - z));
	}

	private static ColorXyz LabToXyz(ColorLab lab)
	{
		var y = (lab.l + 16) / 116f;
		var x = lab.a / 500f + y;
		var z = y - lab.b / 200f;

		var refX = 95.047f; // Observer= 2°, Illuminant= D65
		var refY = 100.000f;
		var refZ = 108.883f;

		x = ReversePivotXyz(x) * refX / 100;
		y = ReversePivotXyz(y) * refY / 100;
		z = ReversePivotXyz(z) * refZ / 100;

		return new ColorXyz(x, y, z);
	}

	private static float ReversePivotXyz(float n)
	{
		return n > 0.2068966f ? MathF.Pow(n, 3) : (n - 16 / 116f) / 7.787f;
	}

	private static float PivotXyz(float n)
	{
		var i = MathF.Cbrt(n);
		return n > 0.008856f ? i : 7.787f * n + 16 / 116f;
	}

	public ColorRgbaFloat ToColorRgbaFloat()
	{
		var xyz = LabToXyz(this);
		return xyz.ToColorRgbaFloat();
	}

	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		ColorXyz xyz = color.As<ColorXyz>();
		var lab = XyzToLab(xyz);
		l = lab.l;
		a = lab.a;
		b = lab.b;
	}

	public bool Equals(ColorLab other)
	{
		return
			l == other.l &&
			a == other.a &&
			b == other.b;
	}
}
