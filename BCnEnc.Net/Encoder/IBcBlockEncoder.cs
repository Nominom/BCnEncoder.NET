using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;

namespace BCnEncoder.Encoder
{
	internal interface IBcBlockEncoder<T> where T : unmanaged
	{
		byte[] Encode(T[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, OperationContext context);
		void EncodeBlock(T block, CompressionQuality quality, Span<byte> output);
		GlInternalFormat GetInternalFormat();
		GlFormat GetBaseInternalFormat();
		DxgiFormat GetDxgiFormat();
		int GetBlockSize();
	}


}
