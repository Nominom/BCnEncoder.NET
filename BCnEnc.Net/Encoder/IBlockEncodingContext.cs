using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Encoder;

internal interface IBlockEncodingContext
{
	/// <summary>
	/// Raw block in linear rgba color space
	/// </summary>
	public RawBlock4X4RgbaFloat RawBlock { get; }

	/// <summary>
	/// Raw block in linear rgba color space as Vector4s
	/// </summary>
	public RawBlock4X4Vector4 RawBlockVec { get; }

	/// <summary>
	/// The compression quality to use for the operation.
	/// </summary>
	public CompressionQuality Quality { get; }
}

internal static class BlockEncodingContextHelpers
{
	public static TContext Create<TContext>(RawBlock4X4RgbaFloat rawBlock, OperationContext context)
		where TContext : struct, IBlockEncodingContext
	{
		if (typeof(TContext) == typeof(RgbEncodingContext))
		{
			var rgbContext = new RgbEncodingContext(rawBlock, context.Quality, context.Weights, context.ColorConversionMode, context.AlphaThreshold);
			return (TContext)(object)rgbContext;
		}
		if (typeof(TContext) == typeof(RgEncodingContext))
		{
			var rgContext = new RgEncodingContext(rawBlock, context.Quality);
			return (TContext)(object)rgContext;
		}
		throw new NotSupportedException($"The type {typeof(TContext).Name} is not supported.");
	}
}

internal struct RgbEncodingContext : IBlockEncodingContext
{
	/// <inheritdoc />
	public RawBlock4X4RgbaFloat RawBlock { get; init; }

	/// <inheritdoc />
	public RawBlock4X4Vector4 RawBlockVec { get; init; }

	/// <inheritdoc />
	public CompressionQuality Quality { get; init; }

	/// <summary>
	/// RawBlock converted to perceptual space
	/// </summary>
	public RawBlock4X4Vector4 PerceptualBlock { get; init; }

	/// <summary>
	/// Color weights.
	/// </summary>
	public RgbWeights Weights { get; init; }

	/// <summary>
	/// Color conversion mode. Used for final conversion before quantizing endpoints.
	/// </summary>
	public ColorConversionMode ColorConversionMode { get; init; }

	/// <summary>
	/// For 1-bit alpha compression, the alpha threshold value.
	/// </summary>
	public float AlphaThreshold { get; init; }

	public RgbEncodingContext(RawBlock4X4RgbaFloat rawBlock, CompressionQuality quality, RgbWeights weights, ColorConversionMode colorConversionMode, float alphaThreshold)
	{
		RawBlock = rawBlock;
		RawBlockVec = rawBlock.AsVector4();
		Quality = quality;
		Weights = weights;
		ColorConversionMode = colorConversionMode;
		AlphaThreshold = alphaThreshold;

		// Precompute perceptual block
		RawBlock4X4Vector4 perceptualBlock = new RawBlock4X4Vector4();
		Span<Vector4> perceptualPixels = perceptualBlock.AsSpan;
		for (var i = 0; i < perceptualPixels.Length; i++)
		{
			perceptualPixels[i] = weights.TransformToPerceptual(RawBlockVec[i]);
		}

		PerceptualBlock = perceptualBlock;
	}
}

internal struct RgEncodingContext : IBlockEncodingContext
{
	/// <inheritdoc />
	public RawBlock4X4RgbaFloat RawBlock { get; }
	/// <inheritdoc />
	public RawBlock4X4Vector4 RawBlockVec { get; }
	/// <inheritdoc />
	public CompressionQuality Quality { get; }

	public RgEncodingContext(RawBlock4X4RgbaFloat rawBlock, CompressionQuality quality)
	{
		RawBlock = rawBlock;
		RawBlockVec = rawBlock.AsVector4();
		Quality = quality;
	}
}
