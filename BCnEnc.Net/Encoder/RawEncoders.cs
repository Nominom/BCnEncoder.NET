using System;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal interface IBcEncoder
	{
		CompressionFormat EncodedFormat { get; }
		byte[] Encode(ReadOnlyMemory<ColorRgbaFloat> pixels, int width, int height, CompressionQuality quality, OperationContext context);
	}

	internal interface IBcLdrEncoder : IBcEncoder
	{
		byte[] Encode(ReadOnlyMemory<ColorRgba32> pixels, int width, int height, CompressionQuality quality, OperationContext context);
	}

	internal class RawPixelEncoder<TPixelFormat> : IBcEncoder
		where TPixelFormat : unmanaged, IColor
	{
		public RawPixelEncoder(CompressionFormat encodedFormat)
		{
			EncodedFormat = encodedFormat;
		}

		/// <inheritdoc />
		public CompressionFormat EncodedFormat { get; }

		/// <inheritdoc />
		public byte[] Encode(ReadOnlyMemory<ColorRgbaFloat> pixels, int width, int height, CompressionQuality quality, OperationContext context)
		{
			var output = pixels.ConvertToAsBytes<ColorRgbaFloat, TPixelFormat>();

			context.CancellationToken.ThrowIfCancellationRequested();
			context.Progress?.Report(width * height);

			return output;
		}
	}

	internal class RawPixelLdrEncoder<TPixelFormat> : IBcLdrEncoder
		where TPixelFormat : unmanaged, IColor
	{
		public RawPixelLdrEncoder(CompressionFormat encodedFormat)
		{
			EncodedFormat = encodedFormat;
		}

		/// <inheritdoc />
		public CompressionFormat EncodedFormat { get; }

		/// <inheritdoc />
		public byte[] Encode(ReadOnlyMemory<ColorRgbaFloat> pixels, int width, int height, CompressionQuality quality, OperationContext context)
		{
			var output = pixels.ConvertToAsBytes<ColorRgbaFloat, TPixelFormat>();

			context.CancellationToken.ThrowIfCancellationRequested();
			context.Progress.Report(width * height);

			return output;
		}

		/// <inheritdoc />
		public byte[] Encode(ReadOnlyMemory<ColorRgba32> pixels, int width, int height, CompressionQuality quality, OperationContext context)
		{
			var output = pixels.ConvertToAsBytes<ColorRgba32, TPixelFormat>();

			context.CancellationToken.ThrowIfCancellationRequested();
			context.Progress.Report(width * height);

			return output;
		}
	}
}
