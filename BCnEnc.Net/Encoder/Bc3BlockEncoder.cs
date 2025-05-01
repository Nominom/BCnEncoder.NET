using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal class Bc3BlockEncoder : BaseBcBlockEncoder<Bc3Block>
	{
		private static readonly Bc4ComponentBlockEncoder bc4BlockEncoder = new Bc4ComponentBlockEncoder(ColorComponent.A);

		public override Bc3Block EncodeBlock(RawBlock4X4RgbaFloat block, OperationContext context)
		{
			// TODO: Do better.
			block.ColorConvert(context.ColorConversionMode);

			switch (context.Quality)
			{
				case CompressionQuality.Fast:
					return Bc3BlockEncoderFast.EncodeBlock(block, context);
				case CompressionQuality.Balanced:
					return Bc3BlockEncoderBalanced.EncodeBlock(block, context);
				case CompressionQuality.BestQuality:
					return Bc3BlockEncoderSlow.EncodeBlock(block, context);

				default:
					throw new ArgumentOutOfRangeException(nameof(context.Quality), context.Quality, null);
			}
		}

		#region Encoders

		private static class Bc3BlockEncoderFast
		{
			internal static Bc3Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock, OperationContext context)
			{
				Bc3Block result = new Bc3Block();
				result. colorBlock = Bc1BlockEncoder.Bc1BlockEncoderFast.EncodeBlock(rawBlock, context, false);
				result.alphaBlock = bc4BlockEncoder.EncodeBlock(rawBlock, CompressionQuality.Fast);

				return result;
			}
		}

		private static class Bc3BlockEncoderBalanced
		{
			private const int MaxTries = 24 * 2;
			private const float ErrorThreshold = 0.05f;

			internal static Bc3Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock, OperationContext context)
			{
				Bc3Block result = new Bc3Block();
				result. colorBlock = Bc1BlockEncoder.Bc1BlockEncoderBalanced.EncodeBlock(rawBlock, context, false);
				result.alphaBlock = bc4BlockEncoder.EncodeBlock(rawBlock, CompressionQuality.Balanced);

				return result;
			}
		}

		private static class Bc3BlockEncoderSlow
		{
			internal static Bc3Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock, OperationContext context)
			{
				Bc3Block result = new Bc3Block();
				result. colorBlock = Bc1BlockEncoder.Bc1BlockEncoderSlow.EncodeBlock(rawBlock, context, false);
				result.alphaBlock = bc4BlockEncoder.EncodeBlock(rawBlock, CompressionQuality.BestQuality);

				return result;
			}
		}
		#endregion
	}
}
