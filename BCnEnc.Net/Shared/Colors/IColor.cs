using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using ArgumentException = System.ArgumentException;

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

internal enum ColorConversionMode
{
	None,
	LinearToSrgb,
	SrgbToLinear
}

public static class ColorExtensions
{

	/// <summary>
	/// Converts one color type to another.
	/// </summary>
	/// <param name="from">The source color.</param>
	/// <param name="to">Reference to the target color.</param>
	/// <typeparam name="TFrom">The type of the source color.</typeparam>
	/// <typeparam name="TTo">The type of the target color.</typeparam>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static void To<TFrom, TTo>(this TFrom from, ref TTo to)
		where TFrom : unmanaged, IColor
		where TTo : unmanaged, IColor
	{
		if (from is not ColorRgbaFloat floatColor)
			floatColor = from.ToColorRgbaFloat();

		to.FromColorRgbaFloat(floatColor);
	}

	/// <summary>
	/// Converts a color from linear RGB to sRGB.
	/// </summary>
	/// <param name="from">The source color in linear RGB.</param>
	/// <param name="to">Reference to the target color in sRGB.</param>
	/// <typeparam name="TFrom">The type of the source color.</typeparam>
	/// <typeparam name="TTo">The type of the target color.</typeparam>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static void ToSrgb<TFrom, TTo>(this TFrom from, ref TTo to)
		where TFrom : unmanaged, IColor
		where TTo : unmanaged, IColor
	{
		if (from is not ColorRgbaFloat floatColor)
			floatColor = from.ToColorRgbaFloat();

		floatColor = ColorSpace.LrgbToSrgb(floatColor);

		to.FromColorRgbaFloat(floatColor);
	}

	/// <summary>
	/// Converts a color from sRGB to linear RGB.
	/// </summary>
	/// <param name="from">The source color in sRGB.</param>
	/// <param name="to">Reference to the target color in linear RGB.</param>
	/// <typeparam name="TFrom">The type of the source color.</typeparam>
	/// <typeparam name="TTo">The type of the target color.</typeparam>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static void ToLinear<TFrom, TTo>(this TFrom from, ref TTo to)
		where TFrom : unmanaged, IColor
		where TTo : unmanaged, IColor
	{
		if (from is not ColorRgbaFloat floatColor)
			floatColor = from.ToColorRgbaFloat();

		floatColor = ColorSpace.SrgbToLrgb(floatColor);

		to.FromColorRgbaFloat(floatColor);
	}

	/// <summary>
	/// Converts one color type to another.
	/// </summary>
	/// <typeparam name="TTo">The type to convert to.</typeparam>
	/// <param name="fromColor">The color to convert.</param>
	/// <returns>A new instance of <typeparamref name="TTo"/>, which is a copy of <paramref name="fromColor"/>.</returns>
	public static TTo As<TTo>(this IColor fromColor)
		where TTo : unmanaged, IColor
	{
		var floatColor = fromColor.ToColorRgbaFloat();
		var dest = new TTo();
		dest.FromColorRgbaFloat(floatColor);
		return dest;
	}

	internal static TTo[] ConvertTo<TFrom, TTo>(this ReadOnlyMemory<TFrom> fromColor, ColorConversionMode mode)
		where TFrom : unmanaged, IColor
		where TTo : unmanaged, IColor
	{
		var fromSpan = fromColor.Span;
		var toArray = new TTo[fromColor.Length];

		ConvertTo<TFrom, TTo>(fromColor, toArray, mode);

		return toArray;
	}

	internal static byte[] ConvertToAsBytes<TFrom, TTo>(this ReadOnlyMemory<TFrom> fromColor, ColorConversionMode mode)
		where TFrom : unmanaged, IColor
		where TTo : unmanaged, IColor
	{
		var toSize = Unsafe.SizeOf<TTo>();

		int bufferLen = fromColor.Length * toSize;
		byte[] toBytes = new byte[bufferLen];

		ConvertToAsBytes<TFrom, TTo>(fromColor, toBytes, mode);

		return toBytes;
	}

	internal static byte[] InternalConvertToAsBytesFromBytes<TFrom, TTo>(ReadOnlyMemory<byte> fromBytes, ColorConversionMode mode)
		where TFrom : unmanaged, IColor<TFrom>
		where TTo : unmanaged, IColor<TTo>
	{
		var toSize = Unsafe.SizeOf<TTo>();
		var fromSize = Unsafe.SizeOf<TFrom>();

		int bufferLen = fromBytes.Length / fromSize * toSize;
		byte[] toBytes = new byte[bufferLen];

		InternalConvertToAsBytesFromBytes<TFrom, TTo>(fromBytes, toBytes, mode);

		return toBytes;
	}

	internal static byte[] InternalConvertToAsBytesFromBytes(ReadOnlyMemory<byte> fromBytes, CompressionFormat inFormat,
		CompressionFormat outFormat, ColorConversionMode colorConversionMode)
	{
		var inPixelSize = inFormat.GetBytesPerBlock();
		var outPixelSize = outFormat.GetBytesPerBlock();

		var bufferLen = fromBytes.Length / inPixelSize * outPixelSize;
		var toBytes = new byte[bufferLen];

		InternalConvertToAsBytesFromBytes(fromBytes, toBytes, inFormat, outFormat, colorConversionMode);

		return toBytes;
	}

	internal static void ConvertTo<TFrom, TTo>(this ReadOnlyMemory<TFrom> fromColor, Memory<TTo> toColor, ColorConversionMode mode)
		where TFrom : unmanaged, IColor
		where TTo : unmanaged, IColor
	{
		var fromSpan = fromColor.Span;
		var toSpan = toColor.Span;

		if (fromSpan.Length != toSpan.Length)
			throw new ArgumentException($"Invalid buffer size. Expecting {fromSpan.Length} elements, got {toSpan.Length} elements.", nameof(toColor));

		for (var i = 0; i < fromColor.Length; i++)
		{
			switch (mode)
			{
				case ColorConversionMode.SrgbToLinear:
					fromSpan[i].ToLinear(ref toSpan[i]); break;
				case ColorConversionMode.LinearToSrgb:
					fromSpan[i].ToSrgb(ref toSpan[i]); break;
				case ColorConversionMode.None:
					fromSpan[i].To(ref toSpan[i]); break;
			}
		}
	}

	internal static void ConvertToAsBytes<TFrom, TTo>(this ReadOnlyMemory<TFrom> fromColor, Memory<byte> toBytes, ColorConversionMode mode)
		where TFrom : unmanaged, IColor
		where TTo : unmanaged, IColor
	{
		var toSize = Unsafe.SizeOf<TTo>();
		var fromSpan = fromColor.Span;
		if (toBytes.Length != fromSpan.Length * toSize)
			throw new ArgumentException($"Invalid buffer size. Expecting {fromSpan.Length * toSize} bytes, got {toBytes.Length} bytes.", nameof(toBytes));

		var toSpan = toBytes.Span.Cast<byte, TTo>();

		for (var i = 0; i < fromColor.Length; i++)
		{
			switch (mode)
			{
				case ColorConversionMode.SrgbToLinear:
					fromSpan[i].ToLinear(ref toSpan[i]); break;
				case ColorConversionMode.LinearToSrgb:
					fromSpan[i].ToSrgb(ref toSpan[i]); break;
				case ColorConversionMode.None:
					fromSpan[i].To(ref toSpan[i]); break;
			}
		}
	}

	internal static void InternalConvertToAsBytesFromBytes<TFrom, TTo>(ReadOnlyMemory<byte> fromBytes, Memory<byte> toBytes, ColorConversionMode mode)
		where TFrom : unmanaged, IColor<TFrom>
		where TTo : unmanaged, IColor<TTo>
	{
		var toSize = Unsafe.SizeOf<TTo>();
		var fromSpan = fromBytes.Cast<byte, TFrom>().Span;
		if (toBytes.Length != fromSpan.Length * toSize)
			throw new ArgumentException($"Invalid buffer size. Expecting {fromSpan.Length * toSize} bytes, got {toBytes.Length} bytes.", nameof(toBytes));

		var toSpan = toBytes.Span.Cast<byte, TTo>();

		for (var i = 0; i < fromSpan.Length; i++)
		{
			switch (mode)
			{
				case ColorConversionMode.SrgbToLinear:
					fromSpan[i].ToLinear(ref toSpan[i]); break;
				case ColorConversionMode.LinearToSrgb:
					fromSpan[i].ToSrgb(ref toSpan[i]); break;
				case ColorConversionMode.None:
					fromSpan[i].To(ref toSpan[i]); break;
			}
		}
	}

	internal static void InternalConvertToAsBytesFromBytes(ReadOnlyMemory<byte> fromBytes, Memory<byte> toBytes, CompressionFormat inFormat,
		CompressionFormat outFormat, ColorConversionMode colorConversionMode)
	{
		var inPixelType = inFormat.GetPixelType();
		var outPixelType = outFormat.GetPixelType();
		var method = typeof(ColorExtensions)
			.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
			.First(x => x.Name == nameof(InternalConvertToAsBytesFromBytes) &&
			            x.GetParameters().Length == 3);

		var genericMethod = method.MakeGenericMethod(inPixelType, outPixelType);

		genericMethod.Invoke(null, new object[] { fromBytes, toBytes, colorConversionMode });
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
			fromColor.CopyAsBytes(), AlphaChannelHint.Unknown);
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
			return CompressionFormat.Rgbe32;
		if (type == typeof(ColorXyze) && !isSrgb)
			return CompressionFormat.Xyze32;

		return CompressionFormat.Unknown;
	}
}
