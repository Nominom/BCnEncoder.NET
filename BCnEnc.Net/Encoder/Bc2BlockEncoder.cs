using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal class Bc2BlockEncoder : BaseBcBlockEncoder<Bc2Block>
	{
		public override Bc2Block EncodeBlock(RawBlock4X4RgbaFloat block, CompressionQuality quality, ColorConversionMode colorConversionMode)
		{
			// TODO: Do better.
			block.ColorConvert(colorConversionMode);

			switch (quality)
			{
				case CompressionQuality.Fast:
					return Bc2BlockEncoderFast.EncodeBlock(block);
				case CompressionQuality.Balanced:
					return Bc2BlockEncoderBalanced.EncodeBlock(block);
				case CompressionQuality.BestQuality:
					return Bc2BlockEncoderSlow.EncodeBlock(block);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		private static Bc2Block EncodeAlpha(Bc1Block colorBlock, RawBlock4X4RgbaFloat rawBlock)
		{
			var pixels = rawBlock.AsSpan;

			Bc2Block result = new Bc2Block();
			result.colorBlock = colorBlock;

			for (int i = 0; i < pixels.Length; i++)
			{
				result.SetAlpha(i, pixels[i].a);
			}

			return result;
		}

		#region Encoders

		private static class Bc2BlockEncoderFast
		{

			internal static Bc2Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock)
			{
				Bc1Block colorBlock = Bc1BlockEncoder.Bc1BlockEncoderBalanced.EncodeBlock(rawBlock, false);

				return EncodeAlpha(colorBlock, rawBlock);
			}
		}

		private static class Bc2BlockEncoderBalanced
		{
			internal static Bc2Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock)
			{
				Bc1Block colorBlock = Bc1BlockEncoder.Bc1BlockEncoderBalanced.EncodeBlock(rawBlock, false);

				return EncodeAlpha(colorBlock, rawBlock);
			}
		}

		private static class Bc2BlockEncoderSlow
		{
			internal static Bc2Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock)
			{
				Bc1Block colorBlock = Bc1BlockEncoder.Bc1BlockEncoderSlow.EncodeBlock(rawBlock, false);

				return EncodeAlpha(colorBlock, rawBlock);
			}
		}

		#endregion
	}
}
