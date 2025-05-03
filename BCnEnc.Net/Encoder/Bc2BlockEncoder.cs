using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal class Bc2BlockEncoder : BaseBcBlockEncoder<Bc2Block, RgbEncodingContext>
	{
		private static readonly Bc1BlockEncoder bc1BlockEncoder = new Bc1BlockEncoder(false, false);

		public override Bc2Block EncodeBlock(in RgbEncodingContext context)
		{
			Bc1Block colorBlock = bc1BlockEncoder.EncodeBlock(context);

			return EncodeAlpha(colorBlock, context.RawBlockVec);
		}

		private static Bc2Block EncodeAlpha(Bc1Block colorBlock, RawBlock4X4Vector4 rawBlock)
		{
			var pixels = rawBlock.AsSpan;

			Bc2Block result = new Bc2Block();
			result.colorBlock = colorBlock;

			for (int i = 0; i < pixels.Length; i++)
			{
				result.SetAlpha(i, pixels[i].W);
			}

			return result;
		}
	}
}
