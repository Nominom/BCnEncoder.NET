using System;
using System.Collections.Generic;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncoder.Decoder
{
	internal interface IBcDecoder
	{
		CompressionFormat DecodedFormat { get; }
		byte[] Decode(ReadOnlyMemory<byte> data, int width, int height, OperationContext context);
	}

	internal interface IBcLdrDecoder : IBcDecoder
	{
		CompressionFormat IBcDecoder.DecodedFormat => CompressionFormat.Rgba32;
		ColorRgba32[] DecodeColor(ReadOnlyMemory<byte> data, int width, int height, OperationContext context);
	}

	internal interface IBcHdrDecoder : IBcDecoder
	{
		CompressionFormat IBcDecoder.DecodedFormat => CompressionFormat.RgbaFloat;
		ColorRgbaFloat[] DecodeColor(ReadOnlyMemory<byte> data, int width, int height, OperationContext context);
	}
}
