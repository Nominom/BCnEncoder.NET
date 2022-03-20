using System;
using System.Runtime.CompilerServices;
using BCnEncoder.Shared;
using Microsoft.Toolkit.HighPerformance;

namespace BCnEncoder.Decoder
{
	internal class RawLdrDecoder<TColor> : IBcLdrDecoder where TColor : unmanaged, IColor
	{
		public RawLdrDecoder(bool redAsLuminance)
		{
			this.redAsLuminance = redAsLuminance;
		}

		private readonly bool redAsLuminance;

		/// <inheritdoc />
		public byte[] Decode(ReadOnlyMemory<byte> data, int width, int height, OperationContext context)
		{
			if (data.Length != width * height * Unsafe.SizeOf<TColor>())
			{
				throw new ArgumentException("Input data buffer incorrectly sized!");
			}

			var bytes = data.Cast<byte, TColor>().ConvertToAsBytes<TColor, ColorRgba32>();
			if (redAsLuminance)
			{
				ExpandRToLuminance(bytes);
			}
			context.Progress?.Report(width * height);
			return bytes;
		}

		/// <inheritdoc />
		public ColorRgba32[] DecodeColor(ReadOnlyMemory<byte> data, int width, int height, OperationContext context)
		{
			if (data.Length != width * height * Unsafe.SizeOf<TColor>())
			{
				throw new ArgumentException("Input data buffer incorrectly sized!");
			}
			var colors = data.Cast<byte, TColor>().ConvertTo<TColor, ColorRgba32>();
			if (redAsLuminance)
			{
				ExpandRToLuminance(colors.AsSpan().AsBytes());
			}
			context.Progress?.Report(width * height);
			return colors;
		}

		private static void ExpandRToLuminance(Span<byte> bytes)
		{
			for (var i = 0; i < bytes.Length; i += 4)
			{
				bytes[i + 1] = bytes[i];
				bytes[i + 2] = bytes[i];
			}
		}
	}

	internal class RawHdrDecoder<TColor> : IBcHdrDecoder where TColor : unmanaged, IColor
	{
		/// <inheritdoc />
		public byte[] Decode(ReadOnlyMemory<byte> data, int width, int height, OperationContext context)
		{
			if (data.Length != width * height * Unsafe.SizeOf<TColor>())
			{
				throw new ArgumentException("Input data buffer incorrectly sized!");
			}

			context.Progress?.Report(width * height);
			return data.Cast<byte, TColor>().ConvertToAsBytes<TColor, ColorRgbaFloat>();
		}

		/// <inheritdoc />
		public ColorRgbaFloat[] DecodeColor(ReadOnlyMemory<byte> data, int width, int height, OperationContext context)
		{
			if (data.Length != width * height * Unsafe.SizeOf<TColor>())
			{
				throw new ArgumentException("Input data buffer incorrectly sized!");
			}

			context.Progress?.Report(width * height);
			return data.Cast<byte, TColor>().ConvertTo<TColor, ColorRgbaFloat>();
		}
	}
}
