using System;
using System.Runtime.CompilerServices;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Decoder
{
	internal class RawDecoder<TColor> : IBcDecoder where TColor : unmanaged, IColor
	{

		/// <inheritdoc />
		public byte[] Decode(ReadOnlyMemory<byte> data, int width, int height, OperationContext context)
		{
			if (data.Length != width * height * Unsafe.SizeOf<TColor>())
			{
				throw new ArgumentException("Input data buffer incorrectly sized!");
			}

			var bytes = data.Cast<byte, TColor>().ConvertToAsBytes<TColor, ColorRgbaFloat>(context.ColorConversionMode);

			context.Progress?.Report(width * height);
			return bytes;
		}

		/// <inheritdoc />
		public ColorRgbaFloat[] DecodeColor(ReadOnlyMemory<byte> data, int width, int height, OperationContext context)
		{
			if (data.Length != width * height * Unsafe.SizeOf<TColor>())
			{
				throw new ArgumentException("Input data buffer incorrectly sized!");
			}

			var colors = data.Cast<byte, TColor>().ConvertTo<TColor, ColorRgbaFloat>(context.ColorConversionMode);

			context.Progress?.Report(width * height);
			return colors;
		}
	}
}
