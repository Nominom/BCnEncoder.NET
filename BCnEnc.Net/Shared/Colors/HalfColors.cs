using System;

namespace BCnEncoder.Shared.Colors;

public struct ColorRgbHalf : IColor<ColorRgbHalf>
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
}

public struct ColorRgbaHalf : IColor<ColorRgbaHalf>
{
	public Half r, g, b, a;

	public ColorRgbaHalf(float r, float g, float b)
	{
		this.r = (Half)r;
		this.g = (Half)g;
		this.b = (Half)b;
		a = (Half)1;
	}

	public ColorRgbaHalf(float r, float g, float b, float a)
	{
		this.r = (Half)r;
		this.g = (Half)g;
		this.b = (Half)b;
		this.a = (Half)a;
	}

	public ColorRgbaHalf(Half r, Half g, Half b)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		a = (Half)1;
	}

	public ColorRgbaHalf(Half r, Half g, Half b, Half a)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat((float)r, (float)g, (float)b, (float)a);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		color.ClampToHalf();
		r = (Half)color.r;
		g = (Half)color.g;
		b = (Half)color.b;
		a = (Half)color.a;
	}

	/// <inheritdoc />
	public bool Equals(ColorRgbaHalf other)
	{
		return r.Equals(other.r) && g.Equals(other.g) && b.Equals(other.b) && a.Equals(other.a);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ColorRgbaHalf other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(r, g, b, a);
	}
}
