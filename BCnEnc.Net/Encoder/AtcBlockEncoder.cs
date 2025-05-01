using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal unsafe class AtcBlockEncoder : BaseBcBlockEncoder<AtcBlock>
	{
		private readonly Bc1BlockEncoder bc1BlockEncoder;

		public AtcBlockEncoder()
		{
			bc1BlockEncoder = new Bc1BlockEncoder();
		}

		public override AtcBlock EncodeBlock(RawBlock4X4RgbaFloat block, OperationContext context)
		{
			var atcBlock = new AtcBlock();

			Bc1Block bc1Block;
			// EncodeBlock with BC1 first
			switch (context.Quality)
			{
				case CompressionQuality.Fast:
					bc1Block = Bc1BlockEncoder.Bc1BlockEncoderFast.EncodeBlock(block, context, false);
					break;
				case CompressionQuality.Balanced:
					bc1Block = Bc1BlockEncoder.Bc1BlockEncoderBalanced.EncodeBlock(block, context, false);
					break;
				case CompressionQuality.BestQuality:
					bc1Block = Bc1BlockEncoder.Bc1BlockEncoderSlow.EncodeBlock(block, context, false);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(context.Quality), context.Quality, null);
			}

			// Atc specific modifications to BC1
			// According to http://www.guildsoftware.com/papers/2012.Converting.DXTC.to.ATC.pdf

			// Change color0 from rgb565 to rgb555 with method 0
			atcBlock.color0 = new ColorB5G5R5M1Packed(bc1Block.color0.R, bc1Block.color0.G, bc1Block.color0.B);
			atcBlock.color1 = bc1Block.color1;

			// Remap color indices from BC1 to ATC
			var remap = stackalloc byte[] { 0, 3, 1, 2 };
			for (var i = 0; i < 16; i++)
			{
				atcBlock[i] = remap[bc1Block[i]];
			}

			return atcBlock;
		}
	}

	internal class AtcExplicitAlphaBlockEncoder : BaseBcBlockEncoder<AtcExplicitAlphaBlock>
	{
		private readonly AtcBlockEncoder atcBlockEncoder;

		public AtcExplicitAlphaBlockEncoder()
		{
			atcBlockEncoder = new AtcBlockEncoder();
		}

		public override AtcExplicitAlphaBlock EncodeBlock(RawBlock4X4RgbaFloat block, OperationContext context)
		{
			var atcBlock = atcBlockEncoder.EncodeBlock(block, context);

			// EncodeBlock alpha
			var bc2AlphaBlock = new Bc2AlphaBlock();
			for (var i = 0; i < 16; i++)
			{
				bc2AlphaBlock.SetAlpha(i, block[i].a);
			}

			return new AtcExplicitAlphaBlock
			{
				alphas = bc2AlphaBlock,
				colors = atcBlock
			};
		}
	}

	internal class AtcInterpolatedAlphaBlockEncoder : BaseBcBlockEncoder<AtcInterpolatedAlphaBlock>
	{
		private readonly Bc4ComponentBlockEncoder bc4BlockEncoder;
		private readonly AtcBlockEncoder atcBlockEncoder;

		public AtcInterpolatedAlphaBlockEncoder()
		{
			bc4BlockEncoder = new Bc4ComponentBlockEncoder(ColorComponent.A);
			atcBlockEncoder = new AtcBlockEncoder();
		}

		public override AtcInterpolatedAlphaBlock EncodeBlock(RawBlock4X4RgbaFloat block, OperationContext context)
		{
			var bc4Block = bc4BlockEncoder.EncodeBlock(block, context.Quality);
			var atcBlock = atcBlockEncoder.EncodeBlock(block, context);

			return new AtcInterpolatedAlphaBlock
			{
				alphas = bc4Block,
				colors = atcBlock
			};
		}
	}
}
