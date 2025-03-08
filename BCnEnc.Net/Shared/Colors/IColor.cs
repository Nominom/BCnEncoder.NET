using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Shared.Colors;

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
	public static TTo As<TTo>(this IColor fromColor)
		where TTo : unmanaged, IColor<TTo>
	{
		var floatColor = fromColor.ToColorRgbaFloat();
		var dest = new TTo();
		dest.FromColorRgbaFloat(floatColor);
		return dest;
	}

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

	internal static byte[] InternalConvertToAsBytesFromBytes(ReadOnlyMemory<byte> fromBytes, CompressionFormat inFormat,
		CompressionFormat outFormat)
	{
		var inPixelType = inFormat.GetPixelType();
		var outPixelType = outFormat.GetPixelType();
		var method = typeof(ColorExtensions)
			.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
			.First(x => x.Name == nameof(InternalConvertToAsBytesFromBytes) &&
			            x.GetParameters().Length == 1);

		var genericMethod = method.MakeGenericMethod(inPixelType, outPixelType);

		return genericMethod.Invoke(null, new object[] { fromBytes }) as byte[];
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
		int height, bool isSrgb)
		where TFrom : unmanaged, IColor
		=> AsBCnTextureData<TFrom>(fromColor.AsMemory().AsMemory2D(height, width), isSrgb);

	public static BCnTextureData AsBCnTextureData<TFrom>(this ReadOnlyMemory<TFrom> fromColor, int width,
		int height, bool isSrgb)
		where TFrom : unmanaged, IColor
		=> AsBCnTextureData<TFrom>(fromColor.AsMemory2D(height, width), isSrgb);

	public static BCnTextureData AsBCnTextureData<TFrom>(this ReadOnlyMemory2D<TFrom> fromColor, bool isSrgb)
		where TFrom : unmanaged, IColor
	{
		return BCnTextureData.FromSingle(FromColorType(typeof(TFrom), isSrgb), fromColor.Width, fromColor.Height,
			fromColor.CopyAsBytes());
	}

	private static CompressionFormat FromColorType(Type type, bool isSrgb)
	{
		if (type == typeof(ColorR8) && !isSrgb)
			return CompressionFormat.R8;
		if (type == typeof(ColorR8G8) && !isSrgb)
			return CompressionFormat.R8G8;
		if (type == typeof(ColorRgb24) && !isSrgb)
			return CompressionFormat.Rgb24;
		if (type == typeof(ColorRgb24) && isSrgb)
			return CompressionFormat.Rgb24_sRGB;
		if (type == typeof(ColorBgr24) && !isSrgb)
			return CompressionFormat.Bgr24;
		if (type == typeof(ColorBgr24) && isSrgb)
			return CompressionFormat.Bgr24_sRGB;
		if (type == typeof(ColorRgba32) && !isSrgb)
			return CompressionFormat.Rgba32;
		if (type == typeof(ColorRgba32) && isSrgb)
			return CompressionFormat.Rgba32_sRGB;
		if (type == typeof(ColorBgra32) && !isSrgb)
			return CompressionFormat.Bgra32;
		if (type == typeof(ColorBgra32) && isSrgb)
			return CompressionFormat.Bgra32_sRGB;
		if (type == typeof(ColorRgbaFloat) && !isSrgb)
			return CompressionFormat.RgbaFloat;
		if (type == typeof(ColorRgbaHalf) && !isSrgb)
			return CompressionFormat.RgbaHalf;
		if (type == typeof(ColorRgbFloat) && !isSrgb)
			return CompressionFormat.RgbFloat;
		if (type == typeof(ColorRgbHalf) && !isSrgb)
			return CompressionFormat.RgbHalf;
		if (type == typeof(ColorRgbe) && !isSrgb)
			return CompressionFormat.Rgbe;
		if (type == typeof(ColorXyze) && !isSrgb)
			return CompressionFormat.Xyze;

		return CompressionFormat.Unknown;
	}
}
