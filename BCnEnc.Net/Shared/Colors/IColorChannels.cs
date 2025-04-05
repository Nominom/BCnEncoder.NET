using System.Numerics;

namespace BCnEncoder.Shared.Colors;


/// <summary>
/// Interface for common operations on single-channel color formats for testing purposes.
/// </summary>
public interface IColorRed<TColor, TRaw> : IColor<TColor>
	where TColor : unmanaged, IColor<TColor>
	where TRaw : unmanaged, INumber<TRaw>
{
	/// <summary>
	/// Gets or sets the red component as a floating-point value.
	/// </summary>
	float R { get; set; }

	/// <summary>
	/// Gets or sets the red component as a raw value.
	/// </summary>
	TRaw RawR { get; set; }
}

/// <summary>
/// Interface for common operations on dual-channel color formats for testing purposes.
/// </summary>
public interface IColorRedGreen<TColor, TRaw> : IColorRed<TColor, TRaw>
	where TColor : unmanaged, IColor<TColor>
	where TRaw : unmanaged, INumber<TRaw>
{
	/// <summary>
	/// Gets or sets the green component as a floating-point value.
	/// </summary>
	float G { get; set; }

	/// <summary>
	/// Gets or sets the green component as a raw value.
	/// </summary>
	TRaw RawG { get; set; }
}

/// <summary>
/// Interface for common operations on Rgb color formats for testing purposes.
/// </summary>
public interface IColorRgb<TColor, TRaw> : IColorRedGreen<TColor, TRaw>
	where TColor : unmanaged, IColor<TColor>
	where TRaw : unmanaged, INumber<TRaw>
{
	/// <summary>
	/// Gets or sets the blue component as a floating-point value.
	/// </summary>
	float B { get; set; }

	/// <summary>
	/// Gets or sets the blue component as a raw value.
	/// </summary>
	TRaw RawB { get; set; }
}

/// <summary>
/// Interface for common operations on Rgba color formats for testing purposes.
/// </summary>
public interface IColorRgba<TColor, TRaw> : IColorRgb<TColor, TRaw>
	where TColor : unmanaged, IColor<TColor>
	where TRaw : unmanaged, INumber<TRaw>
{
	/// <summary>
	/// Gets or sets the alpha component as a floating-point value.
	/// </summary>
	float A { get; set; }

	/// <summary>
	/// Gets or sets the alpha component as a raw value.
	/// </summary>
	TRaw RawA { get; set; }
}

/// <summary>
/// Interface for common operations on packed color formats for testing purposes.
/// </summary>
public interface IColorPacked<TPacked>
where TPacked : unmanaged, INumber<TPacked>, IBinaryInteger<TPacked>
{
	public static abstract PackedRgbaMask ChannelMask { get; }

	public TPacked Data { get; set; }
}
