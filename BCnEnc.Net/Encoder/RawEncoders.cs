using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal interface IBcEncoder
	{
		byte[] Encode(ReadOnlyMemory<ColorRgbaFloat> pixels, int width, int height, CompressionQuality quality, OperationContext context);
	}

	internal class RawPixelEncoder<TPixelFormat> : IBcEncoder
		where TPixelFormat : unmanaged, IColor
	{
		public RawPixelEncoder() { }

		/// <inheritdoc />
		public byte[] Encode(ReadOnlyMemory<ColorRgbaFloat> pixels, int width, int height, CompressionQuality quality, OperationContext context)
		{
			var output = pixels.ConvertToAsBytes<ColorRgbaFloat, TPixelFormat>(context.ColorConversionMode);

			context.CancellationToken.ThrowIfCancellationRequested();
			context.Progress?.Report(width * height);

			return output;
		}
	}
}
