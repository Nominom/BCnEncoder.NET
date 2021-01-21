using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal interface IBcBlockEncoder
	{
		int BlockSize { get; }

		byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, OperationContext context);
		byte[] EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality);

		GlInternalFormat GetInternalFormat();
		GlFormat GetBaseInternalFormat();
		DxgiFormat GetDxgiFormat();
	}


}
