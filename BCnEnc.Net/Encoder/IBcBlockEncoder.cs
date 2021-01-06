using System;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal interface IBcBlockEncoder
	{
		byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, bool parallel = true);
		GlInternalFormat GetInternalFormat();
		GLFormat GetBaseInternalFormat();
		DXGI_FORMAT GetDxgiFormat();
	}


}
