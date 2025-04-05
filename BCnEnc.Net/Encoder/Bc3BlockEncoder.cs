using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal class Bc3BlockEncoder : BaseBcBlockEncoder<Bc3Block>
	{
		private static readonly Bc4ComponentBlockEncoder bc4BlockEncoder = new Bc4ComponentBlockEncoder(ColorComponent.A);

		public override Bc3Block EncodeBlock(RawBlock4X4RgbaFloat block, CompressionQuality quality, ColorConversionMode colorConversionMode)
		{
			// TODO: Do better.
			block.ColorConvert(colorConversionMode);

			switch (quality)
			{
				case CompressionQuality.Fast:
					return Bc3BlockEncoderFast.EncodeBlock(block);
				case CompressionQuality.Balanced:
					return Bc3BlockEncoderBalanced.EncodeBlock(block);
				case CompressionQuality.BestQuality:
					return Bc3BlockEncoderSlow.EncodeBlock(block);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		#region Encoders

		private static class Bc3BlockEncoderFast
		{
			internal static Bc3Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock)
			{
				Bc3Block result = new Bc3Block();
				result. colorBlock = Bc1BlockEncoder.Bc1BlockEncoderBalanced.EncodeBlock(rawBlock, false);
				result.alphaBlock = bc4BlockEncoder.EncodeBlock(rawBlock, CompressionQuality.Fast);

				return result;
			}
		}

		private static class Bc3BlockEncoderBalanced
		{
			private const int MaxTries = 24 * 2;
			private const float ErrorThreshold = 0.05f;

			internal static Bc3Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock)
			{
				Bc3Block result = new Bc3Block();
				result. colorBlock = Bc1BlockEncoder.Bc1BlockEncoderBalanced.EncodeBlock(rawBlock, false);
				result.alphaBlock = bc4BlockEncoder.EncodeBlock(rawBlock, CompressionQuality.Balanced);

				return result;
			}
		}

		private static class Bc3BlockEncoderSlow
		{
			internal static Bc3Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock)
			{
				Bc3Block result = new Bc3Block();
				result. colorBlock = Bc1BlockEncoder.Bc1BlockEncoderSlow.EncodeBlock(rawBlock, false);
				result.alphaBlock = bc4BlockEncoder.EncodeBlock(rawBlock, CompressionQuality.BestQuality);

				return result;
			}
		}
		#endregion
	}
}
