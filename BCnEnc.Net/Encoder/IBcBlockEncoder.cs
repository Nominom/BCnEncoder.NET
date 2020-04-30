using System;
using BCnEncoder.Shared;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.Encoder
{
	internal interface IBcBlockEncoder
	{
		byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, EncodingQuality quality, bool parallel = true);
		GlInternalFormat GetInternalFormat();
		GLFormat GetBaseInternalFormat();
		DXGI_FORMAT GetDxgiFormat();
	}


}
