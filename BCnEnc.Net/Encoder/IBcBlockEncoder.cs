using System;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal interface IBcBlockEncoder<TRawBlock> : IBcEncoder
		where TRawBlock : unmanaged
	{
		void EncodeBlock(TRawBlock block, CompressionQuality quality, Span<byte> output);
		int GetBlockSize();
	}

	internal interface IBcLdrBlockEncoder : IBcBlockEncoder<RawBlock4X4Rgba32>, IBcLdrEncoder
	{
	}

	internal interface IBcHdrBlockEncoder : IBcBlockEncoder<RawBlock4X4RgbFloat>
	{
	}
}
