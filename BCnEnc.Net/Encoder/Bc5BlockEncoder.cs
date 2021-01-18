using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal class Bc5BlockEncoder : BaseBcBlockEncoder<Bc5Block>
	{
		private readonly Bc4ComponentBlockEncoder redBlockEncoder;
		private readonly Bc4ComponentBlockEncoder greenBlockEncoder;

		public Bc5BlockEncoder()
		{
			redBlockEncoder = new Bc4ComponentBlockEncoder(Bc4Component.R);
			greenBlockEncoder = new Bc4ComponentBlockEncoder(Bc4Component.G);
		}

		public override Bc5Block EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			return new Bc5Block
			{
				redBlock = redBlockEncoder.EncodeBlock(block, quality),
				greenBlock = greenBlockEncoder.EncodeBlock(block, quality)
			};
		}

		public override GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlCompressedRedGreenRgtc2Ext;
		}

		public override GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRg;
		}

		public override DxgiFormat GetDxgiFormat()
		{
			return DxgiFormat.DxgiFormatBc5Unorm;
		}
	}
}
