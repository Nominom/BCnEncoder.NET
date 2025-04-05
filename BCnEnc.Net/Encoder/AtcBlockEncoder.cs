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

		public override AtcBlock EncodeBlock(RawBlock4X4RgbaFloat block, CompressionQuality quality, ColorConversionMode colorConversionMode)
		{
			var atcBlock = new AtcBlock();

			// EncodeBlock with BC1 first
			var bc1Block = bc1BlockEncoder.EncodeBlock(block, quality, colorConversionMode);

			// Atc specific modifications to BC1
			// According to http://www.guildsoftware.com/papers/2012.Converting.DXTC.to.Atc.pdf

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

		public override void EncodeBlocks(ReadOnlySpan<RawBlock4X4RgbaFloat> blocks, Span<AtcBlock> outputBlocks, CompressionQuality quality,
			ColorConversionMode colorConversionMode)
		{
			for (var i = 0; i < blocks.Length; i++)
			{
				outputBlocks[i] = EncodeBlock(blocks[i], quality, colorConversionMode);
			}
		}
	}

	internal class AtcExplicitAlphaBlockEncoder : BaseBcBlockEncoder<AtcExplicitAlphaBlock>
	{
		private readonly AtcBlockEncoder atcBlockEncoder;

		public AtcExplicitAlphaBlockEncoder()
		{
			atcBlockEncoder = new AtcBlockEncoder();
		}

		public override AtcExplicitAlphaBlock EncodeBlock(RawBlock4X4RgbaFloat block, CompressionQuality quality, ColorConversionMode colorConversionMode)
		{
			var atcBlock = atcBlockEncoder.EncodeBlock(block, quality, colorConversionMode);

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

		public override void EncodeBlocks(ReadOnlySpan<RawBlock4X4RgbaFloat> blocks, Span<AtcExplicitAlphaBlock> outputBlocks, CompressionQuality quality,
			ColorConversionMode colorConversionMode)
		{
			for (var i = 0; i < blocks.Length; i++)
			{
				outputBlocks[i] = EncodeBlock(blocks[i], quality, colorConversionMode);
			}
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

		public override AtcInterpolatedAlphaBlock EncodeBlock(RawBlock4X4RgbaFloat block, CompressionQuality quality, ColorConversionMode colorConversionMode)
		{
			var bc4Block = bc4BlockEncoder.EncodeBlock(block, quality);
			var atcBlock = atcBlockEncoder.EncodeBlock(block, quality, colorConversionMode);

			return new AtcInterpolatedAlphaBlock
			{
				alphas = bc4Block,
				colors = atcBlock
			};
		}
	}
}
