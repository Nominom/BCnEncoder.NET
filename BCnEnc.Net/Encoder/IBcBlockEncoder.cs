using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;

namespace BCnEncoder.Encoder
{
	internal interface IBcBlockEncoder
	{
		byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, OperationContext context);
		void EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality, Span<byte> output);
		GlInternalFormat GetInternalFormat();
		GlFormat GetBaseInternalFormat();
		DxgiFormat GetDxgiFormat();
		int GetBlockSize();
	}


}
