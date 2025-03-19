using System;
using System.Collections.Generic;
using System.Text;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Decoder
{
	internal interface IBcDecoder
	{
		byte[] Decode(ReadOnlyMemory<byte> data, int width, int height, OperationContext context);
		ColorRgbaFloat[] DecodeColor(ReadOnlyMemory<byte> data, int width, int height, OperationContext context);
	}
}
