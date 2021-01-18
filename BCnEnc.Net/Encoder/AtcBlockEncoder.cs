using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal unsafe class AtcBlockEncoder : BaseBcBlockEncoder<AtcBlock>
	{
		private readonly Bc1BlockEncoder bc1BlockEncoder;

		public AtcBlockEncoder()
		{
			bc1BlockEncoder = new Bc1BlockEncoder();
		}

		public override AtcBlock EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			var atcBlock = new AtcBlock();

			// EncodeBlock with BC1 first
			var bc1Block = bc1BlockEncoder.EncodeBlock(block, quality);

			// Atc specific modifications to BC1
			// According to http://www.guildsoftware.com/papers/2012.Converting.DXTC.to.Atc.pdf

			// Change color0 from rgb565 to rgb555 with method 0
			atcBlock.color0 = new ColorRgb555(bc1Block.color0.R, bc1Block.color0.G, bc1Block.color0.B);
			atcBlock.color1 = bc1Block.color1;

			// Remap color indices from BC1 to ATC
			var remap = stackalloc byte[] { 0, 3, 1, 2 };
			for (var i = 0; i < 16; i++)
			{
				atcBlock[i] = remap[bc1Block[i]];
			}

			return atcBlock;
		}

		public override GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlCompressedRgbAtc;
		}

		public override GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRgb;
		}

		public override DxgiFormat GetDxgiFormat()
		{
			return DxgiFormat.DxgiFormatAtc;
		}
	}

	internal class AtcExplicitAlphaBlockEncoder : BaseBcBlockEncoder<AtcExplicitAlphaBlock>
	{
		private readonly AtcBlockEncoder atcBlockEncoder;

		public AtcExplicitAlphaBlockEncoder()
		{
			atcBlockEncoder = new AtcBlockEncoder();
		}

		public override AtcExplicitAlphaBlock EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			var atcBlock = atcBlockEncoder.EncodeBlock(block, quality);

			// EncodeBlock alpha
			var bc2AlphaBlock = new Bc2AlphaBlock();
			for (var i = 0; i < 16; i++)
			{
				bc2AlphaBlock.SetAlpha(i, block[i].A);
			}

			return new AtcExplicitAlphaBlock
			{
				alphas = bc2AlphaBlock,
				colors = atcBlock
			};
		}

		public override GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlCompressedRgbaAtcExplicitAlpha;
		}

		public override GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRgba;
		}

		public override DxgiFormat GetDxgiFormat()
		{
			return DxgiFormat.DxgiFormatAtcExplicitAlpha;
		}
	}

	internal class AtcInterpolatedAlphaBlockEncoder : BaseBcBlockEncoder<AtcInterpolatedAlphaBlock>
	{
		private readonly Bc4ComponentBlockEncoder bc4BlockEncoder;
		private readonly AtcBlockEncoder atcBlockEncoder;

		public AtcInterpolatedAlphaBlockEncoder()
		{
			bc4BlockEncoder = new Bc4ComponentBlockEncoder(Bc4Component.A);
			atcBlockEncoder = new AtcBlockEncoder();
		}

		public override AtcInterpolatedAlphaBlock EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			var bc4Block = bc4BlockEncoder.EncodeBlock(block, quality);
			var atcBlock = atcBlockEncoder.EncodeBlock(block, quality);

			return new AtcInterpolatedAlphaBlock
			{
				alphas = bc4Block,
				colors = atcBlock
			};
		}

		public override GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlCompressedRgbaAtcInterpolatedAlpha;
		}

		public override GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRgba;
		}

		public override DxgiFormat GetDxgiFormat()
		{
			return DxgiFormat.DxgiFormatAtcInterpolatedAlpha;
		}
	}
}
