using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.HighPerformance;

namespace BCnEncoder.Shared
{
	public interface IColor
	{
		ColorRgbaFloat ToColorRgbaFloat();
		void FromColorRgbaFloat(ColorRgbaFloat color);
	}

	public interface IColor<TColor> : IColor, IEquatable<TColor>
		where TColor : unmanaged, IColor<TColor>
	{
	}

	public static class ColorExtensions
	{
		public static TTo ConvertTo<TFrom, TTo>(this TFrom fromColor)
			where TFrom : unmanaged, IColor
			where TTo : unmanaged, IColor
		{
			var floatColor = fromColor.ToColorRgbaFloat();
			var dest = new TTo();
			dest.FromColorRgbaFloat(floatColor);
			return dest;
		}

		public static TTo[] ConvertTo<TFrom, TTo>(this TFrom[] fromColor)
			where TFrom : unmanaged, IColor
			where TTo : unmanaged, IColor
			=> ConvertTo<TFrom, TTo>(fromColor.AsMemory());

		public static TTo[] ConvertTo<TFrom, TTo>(this ReadOnlyMemory<TFrom> fromColor)
			where TFrom : unmanaged, IColor
			where TTo : unmanaged, IColor
		{
			var fromSpan = fromColor.Span;
			var returnArray = new TTo[fromColor.Length];
			for (var i = 0; i < fromColor.Length; i++)
			{
				returnArray[i] = fromSpan[i].ConvertTo<TFrom, TTo>();
			}

			return returnArray;
		}

		public static Memory2D<TTo> ConvertTo<TFrom, TTo>(this TFrom[,] fromColor)
			where TFrom : unmanaged, IColor
			where TTo : unmanaged, IColor
			=> ConvertTo<TFrom, TTo>(fromColor.AsMemory2D());

		public static Memory2D<TTo> ConvertTo<TFrom, TTo>(this ReadOnlyMemory2D<TFrom> fromColor)
			where TFrom : unmanaged, IColor
			where TTo : unmanaged, IColor
		{
			var fromSpan = fromColor.Span;
			var returnMemory = new Memory2D<TTo>(
				new TTo[fromColor.Width * fromColor.Height],
				fromColor.Height,
				fromColor.Width);
			var toSpan = returnMemory.Span;

			for (var y = 0; y < fromColor.Height; y++)
			{
				for (var x = 0; x < fromColor.Width; x++)
				{
					toSpan[y, x] = fromSpan[y, x].ConvertTo<TFrom, TTo>();
				}
			}

			return returnMemory;
		}

		public static byte[] ConvertToAsBytes<TFrom, TTo>(this ReadOnlyMemory<TFrom> fromColor)
			where TFrom : unmanaged, IColor
			where TTo : unmanaged, IColor
		{
			var toSize = Unsafe.SizeOf<TTo>();
			var fromSpan = fromColor.Span;
			var returnArray = new byte[fromColor.Length * toSize];
			var toSpan = returnArray.AsSpan().Cast<byte, TTo>();

			for (var i = 0; i < fromColor.Length; i++)
			{
				toSpan[i] = fromSpan[i].ConvertTo<TFrom, TTo>();
			}

			return returnArray;
		}

		public static byte[] ConvertToAsBytes<TFrom, TTo>(this ReadOnlyMemory2D<TFrom> fromColor)
			where TFrom : unmanaged, IColor
			where TTo : unmanaged, IColor
		{
			var toSize = Unsafe.SizeOf<TTo>();
			var fromSpan = fromColor.Span;
			var returnArray = new byte[fromColor.Height * fromColor.Width * toSize];
			var toSpan = returnArray.AsSpan().Cast<byte, TTo>().AsSpan2D(fromColor.Height, fromColor.Width);

			for (var y = 0; y < fromColor.Height; y++)
			{
				for (var x = 0; x < fromColor.Width; x++)
				{
					toSpan[y, x] = fromSpan[y, x].ConvertTo<TFrom, TTo>();
				}
			}

			return returnArray;
		}

		internal static byte[] InternalConvertToAsBytesFromBytes<TFrom, TTo>(ReadOnlyMemory<byte> fromBytes)
			where TFrom : unmanaged, IColor
			where TTo : unmanaged, IColor
		{
			var toSize = Unsafe.SizeOf<TTo>();
			var fromSpan = fromBytes.Cast<byte, TFrom>().Span;
			var returnArray = new byte[fromSpan.Length * toSize];
			var toSpan = returnArray.AsSpan().Cast<byte, TTo>();

			for (var i = 0; i < fromSpan.Length; i++)
			{
				toSpan[i] = fromSpan[i].ConvertTo<TFrom, TTo>();
			}

			return returnArray;
		}

		internal static byte[] InternalConvertToAsBytesFromBytes(ReadOnlyMemory<byte> fromBytes, CompressionFormat inFormat, CompressionFormat outFormat)
		{
			var inPixelType = inFormat.GetPixelType();
			var outPixelType = outFormat.GetPixelType();
			var method = typeof(ColorExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
				.First(x => x.Name == nameof(InternalConvertToAsBytesFromBytes) &&
				            x.GetParameters().Length == 1);

			var genericMethod = method.MakeGenericMethod(inPixelType, outPixelType);
			
			return genericMethod.Invoke(null, new object[]{ fromBytes }) as byte[];
		}

		public static byte[] CopyAsBytes<TFrom>(this ReadOnlyMemory<TFrom> fromColor)
			where TFrom : unmanaged, IColor
		{
			var toSize = Unsafe.SizeOf<TFrom>();
			var returnArray = new byte[fromColor.Length * toSize];
			var toSpan = returnArray.AsSpan().Cast<byte, TFrom>();
			fromColor.Span.CopyTo(toSpan);
			return returnArray;
		}

		public static byte[] CopyAsBytes<TFrom>(this ReadOnlyMemory2D<TFrom> fromColor)
			where TFrom : unmanaged, IColor
		{
			var toSize = Unsafe.SizeOf<TFrom>();
			var returnArray = new byte[fromColor.Width * fromColor.Height * toSize];
			var toSpan = returnArray.AsSpan().Cast<byte, TFrom>().AsSpan2D(fromColor.Height, fromColor.Width);
			fromColor.Span.CopyTo(toSpan);
			return returnArray;
		}

		public static byte[] CopyAsBytes<TFrom>(this TFrom[] fromColor)
			where TFrom : unmanaged, IColor
			=> CopyAsBytes<TFrom>(fromColor.AsMemory());

		public static ReadOnlyMemory<TFrom> Flatten<TFrom>(this ReadOnlyMemory2D<TFrom> fromColor)
			where TFrom : unmanaged, IColor
		{
			if (fromColor.TryGetMemory(out var returnMemory))
			{
				return returnMemory;
			}

			var returnArray = new TFrom[fromColor.Width * fromColor.Height];

			fromColor.Span.CopyTo(returnArray.AsMemory().AsMemory2D(fromColor.Height, fromColor.Width).Span);
			return returnArray;
		}

		public static BCnTextureData AsBCnTextureData<TFrom>(this TFrom[] fromColor, int width,
			int height)
			where TFrom : unmanaged, IColor
			=> AsBCnTextureData<TFrom>(fromColor.AsMemory().AsMemory2D(height, width));

		public static BCnTextureData AsBCnTextureData<TFrom>(this ReadOnlyMemory<TFrom> fromColor, int width,
			int height)
			where TFrom : unmanaged, IColor
			=> AsBCnTextureData<TFrom>(fromColor.AsMemory2D(height, width));

		public static BCnTextureData AsBCnTextureData<TFrom>(this ReadOnlyMemory2D<TFrom> fromColor)
			where TFrom : unmanaged, IColor
		{
			return new BCnTextureData(FromColorType(typeof(TFrom)), fromColor.Width, fromColor.Height,
				fromColor.CopyAsBytes());
		}

		private static CompressionFormat FromColorType(Type type)
		{
			if (type == typeof(ColorR8))
				return CompressionFormat.R8;
			if (type == typeof(ColorR8G8))
				return CompressionFormat.R8G8;
			if (type == typeof(ColorRgb24))
				return CompressionFormat.Rgb24;
			if (type == typeof(ColorBgr24))
				return CompressionFormat.Bgr24;
			if (type == typeof(ColorRgba32))
				return CompressionFormat.Rgba32;
			if (type == typeof(ColorBgra32))
				return CompressionFormat.Bgra32;
			if (type == typeof(ColorRgbaFloat))
				return CompressionFormat.RgbaFloat;
			if (type == typeof(ColorRgbaHalf))
				return CompressionFormat.RgbaHalf;
			if (type == typeof(ColorRgbFloat))
				return CompressionFormat.RgbFloat;
			if (type == typeof(ColorRgbHalf))
				return CompressionFormat.RgbHalf;
			if (type == typeof(ColorRgbe))
				return CompressionFormat.Rgbe;
			if (type == typeof(ColorXyze))
				return CompressionFormat.Xyze;

			return CompressionFormat.Unknown;
		}
	}

	public struct ColorR8 : IColor<ColorR8>
	{
		public byte r;

		public ColorR8(byte r)
		{
			this.r = r;
		}

		/// <inheritdoc />
		public ColorRgbaFloat ToColorRgbaFloat()
		{
			return new ColorRgbaFloat(
				r / 255f,
				0,
				0
			);
		}

		/// <inheritdoc />
		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			r = ByteHelper.ClampToByte(color.r * 255f);
		}

		/// <inheritdoc />
		public bool Equals(ColorR8 other)
		{
			return r == other.r;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is ColorR8 other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return r.GetHashCode();
		}
	}

	public struct ColorR8G8 : IColor<ColorR8G8>
	{
		public byte r, g;

		public ColorR8G8(byte r, byte g)
		{
			this.r = r;
			this.g = g;
		}

		/// <inheritdoc />
		public ColorRgbaFloat ToColorRgbaFloat()
		{
			return new ColorRgbaFloat(
				r / 255f,
				g / 255f,
				0);
		}

		/// <inheritdoc />
		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			r = ByteHelper.ClampToByte(color.r * 255f);
			g = ByteHelper.ClampToByte(color.g * 255f);
		}

		/// <inheritdoc />
		public bool Equals(ColorR8G8 other)
		{
			return r == other.r && g == other.g;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is ColorR8G8 other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return HashCode.Combine(r, g);
		}
	}

	public struct ColorRgba32 : IColor<ColorRgba32>
	{
		public byte r, g, b, a;

		public ColorRgba32(byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public ColorRgba32(byte r, byte g, byte b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = 255;
		}

		public bool Equals(ColorRgba32 other)
		{
			return r == other.r &&
			       g == other.g &&
			       b == other.b &&
			       a == other.a;
		}

		public override bool Equals(object obj)
		{
			return obj is ColorRgba32 other && Equals(other);
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

		public static bool operator ==(ColorRgba32 left, ColorRgba32 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ColorRgba32 left, ColorRgba32 right)
		{
			return !left.Equals(right);
		}

		public static ColorRgba32 operator +(ColorRgba32 left, ColorRgba32 right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r + right.r),
				ByteHelper.ClampToByte(left.g + right.g),
				ByteHelper.ClampToByte(left.b + right.b),
				ByteHelper.ClampToByte(left.a + right.a));
		}

		public static ColorRgba32 operator -(ColorRgba32 left, ColorRgba32 right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r - right.r),
				ByteHelper.ClampToByte(left.g - right.g),
				ByteHelper.ClampToByte(left.b - right.b),
				ByteHelper.ClampToByte(left.a - right.a));
		}

		public static ColorRgba32 operator /(ColorRgba32 left, double right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte((int)(left.r / right)),
				ByteHelper.ClampToByte((int)(left.g / right)),
				ByteHelper.ClampToByte((int)(left.b / right)),
				ByteHelper.ClampToByte((int)(left.a / right))
			);
		}

		public static ColorRgba32 operator *(ColorRgba32 left, double right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte((int)(left.r * right)),
				ByteHelper.ClampToByte((int)(left.g * right)),
				ByteHelper.ClampToByte((int)(left.b * right)),
				ByteHelper.ClampToByte((int)(left.a * right))
			);
		}

		/// <summary>
		/// Component-wise left shift
		/// </summary>
		public static ColorRgba32 operator <<(ColorRgba32 left, int right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r << right),
				ByteHelper.ClampToByte(left.g << right),
				ByteHelper.ClampToByte(left.b << right),
				ByteHelper.ClampToByte(left.a << right)
			);
		}

		/// <summary>
		/// Component-wise right shift
		/// </summary>
		public static ColorRgba32 operator >> (ColorRgba32 left, int right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r >> right),
				ByteHelper.ClampToByte(left.g >> right),
				ByteHelper.ClampToByte(left.b >> right),
				ByteHelper.ClampToByte(left.a >> right)
			);
		}

		/// <summary>
		/// Component-wise bitwise OR operation
		/// </summary>
		public static ColorRgba32 operator |(ColorRgba32 left, ColorRgba32 right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r | right.r),
				ByteHelper.ClampToByte(left.g | right.g),
				ByteHelper.ClampToByte(left.b | right.b),
				ByteHelper.ClampToByte(left.a | right.a)
			);
		}

		/// <summary>
		/// Component-wise bitwise OR operation
		/// </summary>
		public static ColorRgba32 operator |(ColorRgba32 left, int right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r | right),
				ByteHelper.ClampToByte(left.g | right),
				ByteHelper.ClampToByte(left.b | right),
				ByteHelper.ClampToByte(left.a | right)
			);
		}

		/// <summary>
		/// Component-wise bitwise AND operation
		/// </summary>
		public static ColorRgba32 operator &(ColorRgba32 left, ColorRgba32 right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r & right.r),
				ByteHelper.ClampToByte(left.g & right.g),
				ByteHelper.ClampToByte(left.b & right.b),
				ByteHelper.ClampToByte(left.a & right.a)
			);
		}

		/// <summary>
		/// Component-wise bitwise AND operation
		/// </summary>
		public static ColorRgba32 operator &(ColorRgba32 left, int right)
		{
			return new ColorRgba32(
				ByteHelper.ClampToByte(left.r & right),
				ByteHelper.ClampToByte(left.g & right),
				ByteHelper.ClampToByte(left.b & right),
				ByteHelper.ClampToByte(left.a & right)
			);
		}

		public override string ToString()
		{
			return $"r : {r} g : {g} b : {b} a : {a}";
		}

		public readonly ColorRgbFloat ToRgbFloat()
		{
			return new ColorRgbFloat(this);
		}

		/// <inheritdoc />
		public readonly ColorRgbaFloat ToColorRgbaFloat()
		{
			return new ColorRgbaFloat(this);
		}

		/// <inheritdoc />
		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			r = ByteHelper.ClampToByte(color.r * 255f);
			g = ByteHelper.ClampToByte(color.g * 255f);
			b = ByteHelper.ClampToByte(color.b * 255f);
			a = ByteHelper.ClampToByte(color.a * 255f);
		}
	}

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

		public ColorRgbaFloat(ColorRgba32 other)
		{
			this.r = other.r / 255f;
			this.g = other.g / 255f;
			this.b = other.b / 255f;
			this.a = other.a / 255f;
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
			return $"r : {r:0.00} g : {g:0.00} b : {b:0.00} a : {a:0.00}";
		}

		public ColorRgba32 ToRgba32()
		{
			return new ColorRgba32(
				(byte)(ByteHelper.ClampToByte(r * 255)),
				(byte)(ByteHelper.ClampToByte(g * 255)),
				(byte)(ByteHelper.ClampToByte(b * 255)),
				(byte)(ByteHelper.ClampToByte(a * 255))
			);
		}

		public ColorRgbFloat ToRgb()
		{
			return new ColorRgbFloat(r, g, b);
		}

		internal void ClampToHalf()
		{
			if (r < Half.MinValue) r = Half.MinValue;
			else if (g > Half.MaxValue) g = Half.MaxValue;
			if (b < Half.MinValue) b = Half.MinValue;
			else if (r > Half.MaxValue) r = Half.MaxValue;
			if (g < Half.MinValue) g = Half.MinValue;
			else if (b > Half.MaxValue) b = Half.MaxValue;
			if (a < Half.MinValue) a = Half.MinValue;
			else if (a > Half.MaxValue) a = Half.MaxValue;
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

		/// <inheritdoc cref="LRgbToSRgb"/>
		public ColorRgbaFloat ToSRgb()
		{
			return new ColorRgbaFloat(
				LRgbToSRgb(r),
				LRgbToSRgb(g),
				LRgbToSRgb(b),
				a
			);
		}

		/// <inheritdoc cref="SRgbToLRgb"/>
		public ColorRgbaFloat ToLRgb()
		{
			return new ColorRgbaFloat(
				SRgbToLRgb(r),
				SRgbToLRgb(g),
				SRgbToLRgb(b),
				a
			);
		}

		/// <summary>
		/// Change sRGB (Gamma 2.2) to Linear RGB
		/// </summary>
		public static float SRgbToLRgb(float n)
		{
			return (n > 0.04045f ? MathF.Pow((n + 0.055f) / 1.055f, 2.4f) : n / 12.92f);
		}

		/// <summary>
		/// Change Linear RGB to sRGB (Gamma 2.2)
		/// </summary>
		public static float LRgbToSRgb(float n)
		{
			return (n > 0.0031308f ? MathF.Pow(n, 1 / 2.4f) * 1.055f - 0.055f : n * 12.92f);
		}
	}
	
	internal struct ColorBgra32 : IColor<ColorBgra32>
	{
		public byte b, g, r, a;
		
		public ColorBgra32(byte b, byte g, byte r)
		{
			this.b = b;
			this.g = g;
			this.r = r;
			this.a = 255;
		}

		public ColorBgra32(byte b, byte g, byte r, byte a)
		{
			this.b = b;
			this.g = g;
			this.r = r;
			this.a = a;
		}

		/// <inheritdoc />
		public bool Equals(ColorBgra32 other)
		{
			return b == other.b && g == other.g && r == other.r && a == other.a;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is ColorBgra32 other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return HashCode.Combine(b, g, r, a);
		}

		/// <inheritdoc />
		public readonly ColorRgbaFloat ToColorRgbaFloat()
		{
			return new ColorRgbaFloat(
				this.r / 255f,
				this.g / 255f,
				this.b / 255f,
				this.a / 255f);
		}

		/// <inheritdoc />
		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			r = ByteHelper.ClampToByte(color.r * 255f);
			g = ByteHelper.ClampToByte(color.g * 255f);
			b = ByteHelper.ClampToByte(color.b * 255f);
			a = ByteHelper.ClampToByte(color.a * 255f);
		}
	}

	internal struct ColorBgr24 : IColor<ColorBgr24>
	{
		public byte b, g, r;

		public ColorBgr24(byte b, byte g, byte r)
		{
			this.b = b;
			this.g = g;
			this.r = r;
		}

		/// <inheritdoc />
		public bool Equals(ColorBgr24 other)
		{
			return b == other.b && g == other.g && r == other.r;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is ColorBgr24 other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return HashCode.Combine(b, g, r);
		}

		/// <inheritdoc />
		public readonly ColorRgbaFloat ToColorRgbaFloat()
		{
			return new ColorRgbaFloat(
				this.r / 255f,
				this.g / 255f,
				this.b / 255f);
		}

		/// <inheritdoc />
		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			r = ByteHelper.ClampToByte(color.r * 255f);
			g = ByteHelper.ClampToByte(color.g * 255f);
			b = ByteHelper.ClampToByte(color.b * 255f);
		}
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
			if (r < Half.MinValue) r = Half.MinValue;
			else if (g > Half.MaxValue) g = Half.MaxValue;
			if (b < Half.MinValue) b = Half.MinValue;
			else if (r > Half.MaxValue) r = Half.MaxValue;
			if (g < Half.MinValue) g = Half.MinValue;
			else if (b > Half.MaxValue) b = Half.MaxValue;
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
	}

	public struct ColorRgbHalf : IColor<ColorRgbHalf>
	{
		public Half r, g, b;

		public ColorRgbHalf(float r, float g, float b)
		{
			this.r = new Half(r);
			this.g = new Half(g);
			this.b = new Half(b);
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
			return new ColorRgbaFloat(r, g, b);
		}

		/// <inheritdoc />
		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			color.ClampToHalf();
			r = new Half(color.r);
			g = new Half(color.g);
			b = new Half(color.b);
		}
	}

	public struct ColorRgbaHalf : IColor<ColorRgbaHalf>
	{
		public Half r, g, b, a;

		public ColorRgbaHalf(float r, float g, float b)
		{
			this.r = new Half(r);
			this.g = new Half(g);
			this.b = new Half(b);
			a = 1;
		}

		public ColorRgbaHalf(float r, float g, float b, float a)
		{
			this.r = new Half(r);
			this.g = new Half(g);
			this.b = new Half(b);
			this.a = new Half(a);
		}

		public ColorRgbaHalf(Half r, Half g, Half b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			a = 1;
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
			return new ColorRgbaFloat(r, g, b, a);
		}

		/// <inheritdoc />
		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			color.ClampToHalf();
			r = new Half(color.r);
			g = new Half(color.g);
			b = new Half(color.b);
			a = new Half(color.a);
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

		internal ColorYCbCr(ColorRgb24 rgb)
		{
			var fr = (float)rgb.r / 255;
			var fg = (float)rgb.g / 255;
			var fb = (float)rgb.b / 255;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
		}

		internal ColorYCbCr(ColorRgbaFloat rgb)
		{
			var fr = rgb.r;
			var fg = rgb.g;
			var fb = rgb.b;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
		}

		internal ColorYCbCr(ColorRgbFloat rgb)
		{
			var fr = rgb.r;
			var fg = rgb.g;
			var fb = rgb.b;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
		}

		internal ColorYCbCr(ColorRgb565 rgb)
		{
			var fr = (float)rgb.R / 255;
			var fg = (float)rgb.G / 255;
			var fb = (float)rgb.B / 255;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
		}

		public ColorYCbCr(ColorRgba32 rgba)
		{
			var fr = (float)rgba.r / 255;
			var fg = (float)rgba.g / 255;
			var fb = (float)rgba.b / 255;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
		}

		public ColorYCbCr(Vector3 vec)
		{
			var fr = (float)vec.X;
			var fg = (float)vec.Y;
			var fb = (float)vec.Z;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
		}

		public ColorRgb565 ToColorRgb565()
		{
			var r = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 0.0000 * cb + 1.4022 * cr)));
			var g = Math.Max(0.0f, Math.Min(1.0f, (float)(y - 0.3456 * cb - 0.7145 * cr)));
			var b = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 1.7710 * cb + 0.0000 * cr)));

			return new ColorRgb565((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
		}

		public ColorRgba32 ToColorRgba32()
		{
			var r = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 0.0000 * cb + 1.4022 * cr)));
			var g = Math.Max(0.0f, Math.Min(1.0f, (float)(y - 0.3456 * cb - 0.7145 * cr)));
			var b = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 1.7710 * cb + 0.0000 * cr)));

			return new ColorRgba32((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), 255);
		}

		public override string ToString()
		{
			var r = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 0.0000 * cb + 1.4022 * cr)));
			var g = Math.Max(0.0f, Math.Min(1.0f, (float)(y - 0.3456 * cb - 0.7145 * cr)));
			var b = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 1.7710 * cb + 0.0000 * cr)));

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
			var r = y + 0.0000f * cb + 1.4022f * cr;
			var g = y - 0.3456f * cb - 0.7145f * cr;
			var b = y + 1.7710f * cb + 0.0000f * cr;
			return new ColorRgbaFloat(r, g, b);
		}

		/// <inheritdoc />
		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			var fr = color.r;
			var fg = color.g;
			var fb = color.b;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
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

	internal struct ColorRgb555 : IColor<ColorRgb555>
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

	public struct ColorRgb24 : IColor<ColorRgb24>
	{
		public byte r, g, b;

		public ColorRgb24(byte r, byte g, byte b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

		public ColorRgb24(ColorRgb565 color)
		{
			this.r = color.R;
			this.g = color.G;
			this.b = color.B;
		}

		public ColorRgb24(ColorRgba32 color)
		{
			this.r = color.r;
			this.g = color.g;
			this.b = color.b;
		}

		public bool Equals(ColorRgb24 other)
		{
			return r == other.r && g == other.g && b == other.b;
		}

		public override bool Equals(object obj)
		{
			return obj is ColorRgb24 other && Equals(other);
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

		public static bool operator ==(ColorRgb24 left, ColorRgb24 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ColorRgb24 left, ColorRgb24 right)
		{
			return !left.Equals(right);
		}
		
		public static ColorRgb24 operator +(ColorRgb24 left, ColorRgb24 right)
		{
			return new ColorRgb24(
				ByteHelper.ClampToByte(left.r + right.r),
				ByteHelper.ClampToByte(left.g + right.g),
				ByteHelper.ClampToByte(left.b + right.b));
		}

		public static ColorRgb24 operator -(ColorRgb24 left, ColorRgb24 right)
		{
			return new ColorRgb24(
				ByteHelper.ClampToByte(left.r - right.r),
				ByteHelper.ClampToByte(left.g - right.g),
				ByteHelper.ClampToByte(left.b - right.b));
		}

		public static ColorRgb24 operator /(ColorRgb24 left, double right)
		{
			return new ColorRgb24(
				ByteHelper.ClampToByte((int)(left.r / right)),
				ByteHelper.ClampToByte((int)(left.g / right)),
				ByteHelper.ClampToByte((int)(left.b / right))
			);
		}

		public static ColorRgb24 operator *(ColorRgb24 left, double right)
		{
			return new ColorRgb24(
				ByteHelper.ClampToByte((int)(left.r * right)),
				ByteHelper.ClampToByte((int)(left.g * right)),
				ByteHelper.ClampToByte((int)(left.b * right))
			);
		}

		public override string ToString()
		{
			return $"r : {r} g : {g} b : {b}";
		}

		public ColorRgbaFloat ToColorRgbaFloat()
		{
			return new ColorRgbaFloat(
				r / 255f,
				g / 255f,
				b / 255f);
		}

		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			r = ByteHelper.ClampToByte(color.r * 255f);
			g = ByteHelper.ClampToByte(color.g * 255f);
			b = ByteHelper.ClampToByte(color.b * 255f);
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

		public ColorYCbCrAlpha(ColorRgb24 rgb)
		{
			var fr = (float)rgb.r / 255;
			var fg = (float)rgb.g / 255;
			var fb = (float)rgb.b / 255;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
			a = 1;
		}

		public ColorYCbCrAlpha(ColorRgb565 rgb)
		{
			var fr = (float)rgb.R / 255;
			var fg = (float)rgb.G / 255;
			var fb = (float)rgb.B / 255;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
			a = 1;
		}

		public ColorYCbCrAlpha(ColorRgba32 rgba)
		{
			var fr = (float)rgba.r / 255;
			var fg = (float)rgba.g / 255;
			var fb = (float)rgba.b / 255;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
			a = rgba.a / 255f;
		}

		public ColorYCbCrAlpha(ColorRgbaFloat rgba)
		{
			var fr = rgba.r;
			var fg = rgba.g;
			var fb = rgba.b;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
			a = rgba.a;
		}


		public ColorRgb565 ToColorRgb565()
		{
			var r = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 0.0000 * cb + 1.4022 * cr)));
			var g = Math.Max(0.0f, Math.Min(1.0f, (float)(y - 0.3456 * cb - 0.7145 * cr)));
			var b = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 1.7710 * cb + 0.0000 * cr)));

			return new ColorRgb565((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
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
			var r = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 0.0000 * cb + 1.4022 * cr)));
			var g = Math.Max(0.0f, Math.Min(1.0f, (float)(y - 0.3456 * cb - 0.7145 * cr)));
			var b = Math.Max(0.0f, Math.Min(1.0f, (float)(y + 1.7710 * cb + 0.0000 * cr)));

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
			var r = y + 0.0000f * cb + 1.4022f * cr;
			var g = y - 0.3456f * cb - 0.7145f * cr;
			var b = y + 1.7710f * cb + 0.0000f * cr;
			return new ColorRgbaFloat(r, g, b, a);
		}

		/// <inheritdoc />
		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			var fr = color.r;
			var fg = color.g;
			var fb = color.b;

			y = 0.2989f * fr + 0.5866f * fg + 0.1145f * fb;
			cb = -0.1687f * fr - 0.3313f * fg + 0.5000f * fb;
			cr = 0.5000f * fr - 0.4184f * fg - 0.0816f * fb;
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

		public ColorXyz(ColorRgb24 color)
		{
			this = ColorToXyz(color);
		}

		public ColorXyz(ColorRgbFloat color)
		{
			this = ColorToXyz(color);
		}

		public ColorRgbFloat ToColorRgbFloat()
		{

			// Observer. = 2°, Illuminant = D65
			//return new ColorRgbFloat(
			//	3.2404542f * x - 1.5371385f * y - 0.4985314f * z,
			//	-0.9692660f * x + 1.8760108f * y + 0.0415560f * z,
			//	0.0556434f * x - 0.2040259f * y + 1.0572252f * z
			//);
			return new ColorRgbFloat(
			 2.565f  * x - 1.167f * y - 0.398f * z,
			 -1.022f * x + 1.978f * y + 0.044f * z,
			 0.075f  * x - 0.252f * y + 1.177f * z);
		}

		public static ColorXyz ColorToXyz(ColorRgb24 color)
		{
			var r = color.r / 255.0f;
			var g = color.g / 255.0f;
			var b = color.b / 255.0f;

			// Observer. = 2°, Illuminant = D65
			return new ColorXyz(new ColorRgbFloat(r, g, b));
		}

		public static ColorXyz ColorToXyz(ColorRgbFloat color)
		{
			var r = color.r;
			var g = color.g;
			var b = color.b;

			// Observer. = 2°, Illuminant = D65
			//return new ColorXyz(
			//	r * 0.4124f + g * 0.3576f + b * 0.1805f,
			//	r * 0.2126f + g * 0.7152f + b * 0.0722f,
			//	r * 0.0193f + g * 0.1192f + b * 0.9505f);
			return new ColorXyz(
				0.5142f * r + 0.3240f * g + 0.1618f * b,
				0.2652f * r + 0.6702f * g + 0.0646f * b,
				0.0240f * r + 0.1229f * g + 0.8531f * b);
		}

		/// <inheritdoc />
		public ColorRgbaFloat ToColorRgbaFloat()
		{
			return new ColorRgbaFloat(ToColorRgbFloat());
		}

		/// <inheritdoc />
		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			var r = color.r;
			var g = color.g;
			var b = color.b;

			// Observer. = 2°, Illuminant = D65
			//x = r * 0.4124f + g * 0.3576f + b * 0.1805f;
			//y = r * 0.2126f + g * 0.7152f + b * 0.0722f;
			//z = r * 0.0193f + g * 0.1192f + b * 0.9505f;

			x = 0.5142f * r + 0.3240f * g + 0.1618f * b;
			y = 0.2652f * r + 0.6702f * g + 0.0646f * b;
			z = 0.0240f * r + 0.1229f * g + 0.8531f * b;
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

	internal struct ColorLab
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

		public ColorLab(ColorRgb24 color)
		{
			this = ColorToLab(color);
		}

		public ColorLab(ColorRgba32 color)
		{
			this = ColorToLab(new ColorRgb24(color.r, color.g, color.b));
		}

		public ColorLab(ColorRgbFloat color)
		{
			this = XyzToLab(new ColorXyz(color));
		}

		public static ColorLab ColorToLab(ColorRgb24 color)
		{
			var xyz = new ColorXyz(color);
			return XyzToLab(xyz);
		}


		public static ColorLab XyzToLab(ColorXyz xyz)
		{
			var refX = 95.047f; // Observer= 2°, Illuminant= D65
			var refY = 100.000f;
			var refZ = 108.883f;

			var x = PivotXyz(xyz.x * 100 / refX);
			var y = PivotXyz(xyz.y * 100 / refY);
			var z = PivotXyz(xyz.z * 100 / refZ);

			return new ColorLab(116 * y - 16, 500 * (x - y), 200 * (y - z));
		}

		private static float PivotXyz(float n)
		{
			var i = MathF.Cbrt(n);
			return n > 0.008856f ? i : 7.787f * n + 16 / 116f;
		}
	}

	internal struct ColorRgbe : IColor<ColorRgbe>
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
				var fexp = MathHelper.LdExp(1f, e - (128 + 8));

				return new ColorRgbaFloat(
					r == 0 ? 0 : (float)((r + 0.5)* fexp),
					g == 0 ? 0 : (float)((g + 0.5)* fexp),
					b == 0 ? 0 : (float)((b + 0.5)* fexp)
				).ToSRgb();
			}
		}

		/// <inheritdoc />
		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			color = color.ToLRgb();
			var max = MathF.Max(color.b, MathF.Max(color.g, color.r));
			if (max <= 1e-32f)
			{
				r = g = b = e = 0;
			}
			else
			{
				max = (float)(MathHelper.FrExp(max, out var exponent) * 255.9999f / max);

				r = (byte)(max * color.r);
				g = (byte)(max * color.g);
				b = (byte)(max * color.b);
				e = (byte)(exponent + 128);
			}
		}
	}

	internal struct ColorXyze : IColor<ColorXyze>
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
				).ToColorRgbaFloat().ToSRgb();
			}
		}

		/// <inheritdoc />
		public void FromColorRgbaFloat(ColorRgbaFloat color)
		{
			color = color.ToLRgb();
			ColorXyz xyz = new ColorXyz(color.ToRgb());
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
}
