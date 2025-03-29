using System;
using System.Numerics;

namespace BCnEncoder.Shared.Colors;

public struct ColorRgbaFloat : IColor<ColorRgbaFloat>
{
	public float r, g, b, a;

	public ColorRgbaFloat(float r, float g, float b, float a)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public ColorRgbaFloat(ColorRgbFloat other)
	{
		this.r = other.r;
		this.g = other.g;
		this.b = other.b;
		this.a = 1;
	}

	public ColorRgbaFloat(float r, float g, float b)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = 1;
	}

	public ColorRgbaFloat(Vector4 other)
	{
		this.r = other.X;
		this.g = other.Y;
		this.b = other.Z;
		this.a = other.W;
	}

	public bool Equals(ColorRgbaFloat other)
	{
		return r == other.r && g == other.g && b == other.b && a == other.a;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorRgbaFloat other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = r.GetHashCode();
			hashCode = (hashCode * 397) ^ g.GetHashCode();
			hashCode = (hashCode * 397) ^ b.GetHashCode();
			hashCode = (hashCode * 397) ^ a.GetHashCode();
			return hashCode;
		}
	}

	public static bool operator ==(ColorRgbaFloat left, ColorRgbaFloat right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorRgbaFloat left, ColorRgbaFloat right)
	{
		return !left.Equals(right);
	}

	public static ColorRgbaFloat operator +(ColorRgbaFloat left, ColorRgbaFloat right)
	{
		return new ColorRgbaFloat(
			left.r + right.r,
			left.g + right.g,
			left.b + right.b,
			left.a + right.a);
	}

	public static ColorRgbaFloat operator -(ColorRgbaFloat left, ColorRgbaFloat right)
	{
		return new ColorRgbaFloat(
			left.r - right.r,
			left.g - right.g,
			left.b - right.b,
			left.a - right.a);
	}

	public static ColorRgbaFloat operator /(ColorRgbaFloat left, float right)
	{
		return new ColorRgbaFloat(
			left.r / right,
			left.g / right,
			left.b / right,
			left.a / right
		);
	}

	public static ColorRgbaFloat operator *(ColorRgbaFloat left, float right)
	{
		return new ColorRgbaFloat(
			left.r * right,
			left.g * right,
			left.b * right,
			left.a * right
		);
	}

	public static ColorRgbaFloat operator *(float left, ColorRgbaFloat right)
	{
		return new ColorRgbaFloat(
			right.r * left,
			right.g * left,
			right.b * left,
			right.a * left
		);
	}

	public override string ToString()
	{
		return $"r : {r:0.00000} g : {g:0.00000} b : {b:0.00000} a : {a:0.00000}";
	}

	internal void ClampToHalf()
	{
		if (r < (float)Half.MinValue) r = (float)Half.MinValue;
		else if (g > (float)Half.MaxValue) g = (float)Half.MaxValue;
		if (b < (float)Half.MinValue) b = (float)Half.MinValue;
		else if (r > (float)Half.MaxValue) r = (float)Half.MaxValue;
		if (g < (float)Half.MinValue) g = (float)Half.MinValue;
		else if (b > (float)Half.MaxValue) b = (float)Half.MaxValue;
		if (a < (float)Half.MinValue) a = (float)Half.MinValue;
		else if (a > (float)Half.MaxValue) a = (float)Half.MaxValue;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return this;
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		r = color.r;
		g = color.g;
		b = color.b;
		a = color.a;
	}

	public Vector3 ToVector3()
	{
		return new Vector3(r, g, b);
	}

	public Vector4 ToVector4()
	{
		return new Vector4(r, g, b, a);
	}

	public static implicit operator ColorRgbFloat(ColorRgbaFloat c) => new(c.r, c.g, c.b);
}

public struct ColorRgbFloat : IColor<ColorRgbFloat>
{
	public float r, g, b;

	public ColorRgbFloat(float r, float g, float b)
	{
		this.r = r;
		this.g = g;
		this.b = b;
	}

	public ColorRgbFloat(ColorRgba32 other)
	{
		this.r = other.r / 255f;
		this.g = other.g / 255f;
		this.b = other.b / 255f;
	}

	public ColorRgbFloat(Vector3 vector)
	{
		r = vector.X;
		g = vector.Y;
		b = vector.Z;
	}

	public bool Equals(ColorRgbFloat other)
	{
		return r == other.r && g == other.g && b == other.b;
	}

	public override bool Equals(object obj)
	{
		return obj is ColorRgbFloat other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = r.GetHashCode();
			hashCode = (hashCode * 397) ^ g.GetHashCode();
			hashCode = (hashCode * 397) ^ b.GetHashCode();
			return hashCode;
		}
	}

	public static bool operator ==(ColorRgbFloat left, ColorRgbFloat right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorRgbFloat left, ColorRgbFloat right)
	{
		return !left.Equals(right);
	}

	public static ColorRgbFloat operator +(ColorRgbFloat left, ColorRgbFloat right)
	{
		return new ColorRgbFloat(
			left.r + right.r,
			left.g + right.g,
			left.b + right.b);
	}

	public static ColorRgbFloat operator -(ColorRgbFloat left, ColorRgbFloat right)
	{
		return new ColorRgbFloat(
			left.r - right.r,
			left.g - right.g,
			left.b - right.b);
	}

	public static ColorRgbFloat operator /(ColorRgbFloat left, float right)
	{
		return new ColorRgbFloat(
			left.r / right,
			left.g / right,
			left.b / right
		);
	}

	public static ColorRgbFloat operator *(ColorRgbFloat left, float right)
	{
		return new ColorRgbFloat(
			left.r * right,
			left.g * right,
			left.b * right
		);
	}

	public static ColorRgbFloat operator *(float left, ColorRgbFloat right)
	{
		return new ColorRgbFloat(
			right.r * left,
			right.g * left,
			right.b * left
		);
	}

	public override string ToString()
	{
		return $"r : {r:0.00} g : {g:0.00} b : {b:0.00}";
	}

	public ColorRgba32 ToRgba32()
	{
		return new ColorRgba32(
			(byte)(ByteHelper.ClampToByte(r * 255)),
			(byte)(ByteHelper.ClampToByte(g * 255)),
			(byte)(ByteHelper.ClampToByte(b * 255)),
			255
		);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(r, g, b);
	}

	internal float CalcLogDist(ColorRgbFloat other)
	{
		var dr = Math.Sign(other.r) * MathF.Log(1 + MathF.Abs(other.r)) -
		         Math.Sign(r) * MathF.Log(1 + MathF.Abs(r));
		var dg = Math.Sign(other.g) * MathF.Log(1 + MathF.Abs(other.g)) -
		         Math.Sign(g) * MathF.Log(1 + MathF.Abs(g));
		var db = Math.Sign(other.b) * MathF.Log(1 + MathF.Abs(other.b)) -
		         Math.Sign(b) * MathF.Log(1 + MathF.Abs(b));
		return MathF.Sqrt((dr * dr) + (dg * dg) + (db * db));
	}

	internal float CalcDist(ColorRgbFloat other)
	{
		var dr = other.r - r;
		var dg = other.g - g;
		var db = other.b - b;
		return MathF.Sqrt((dr * dr) + (dg * dg) + (db * db));
	}

	internal void ClampToPositive()
	{
		if (r < 0) r = 0;
		if (g < 0) g = 0;
		if (b < 0) b = 0;
	}

	internal void ClampToHalf()
	{
		if (r < (float)Half.MinValue) r = (float)Half.MinValue;
		else if (g > (float)Half.MaxValue) g = (float)Half.MaxValue;
		if (b < (float)Half.MinValue) b = (float)Half.MinValue;
		else if (r > (float)Half.MaxValue) r = (float)Half.MaxValue;
		if (g < (float)Half.MinValue) g = (float)Half.MinValue;
		else if (b > (float)Half.MaxValue) b = (float)Half.MaxValue;
	}

	/// <inheritdoc />
	public ColorRgbaFloat ToColorRgbaFloat()
	{
		return new ColorRgbaFloat(this);
	}

	/// <inheritdoc />
	public void FromColorRgbaFloat(ColorRgbaFloat color)
	{
		this.r = color.r;
		this.g = color.g;
		this.b = color.b;
	}

	public static implicit operator ColorRgbaFloat(ColorRgbFloat c) => c.ToColorRgbaFloat();
}

