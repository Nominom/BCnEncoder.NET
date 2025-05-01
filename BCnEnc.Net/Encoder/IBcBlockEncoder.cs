using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal interface IBcBlockEncoder : IBcEncoder
	{
		void EncodeBlocks(ReadOnlySpan<RawBlock4X4RgbaFloat> blocks, Span<byte> output, OperationContext context);
		int GetBlockSize();
	}
}
