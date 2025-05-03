using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal class Bc3BlockEncoder : BaseBcBlockEncoder<Bc3Block, RgbEncodingContext>
	{
		private static readonly Bc1BlockEncoder bc1BlockEncoder = new Bc1BlockEncoder(false, false);
		private static readonly Bc4ComponentBlockEncoder bc4BlockEncoder = new Bc4ComponentBlockEncoder(ColorComponent.A);

		public override Bc3Block EncodeBlock(in RgbEncodingContext context)
		{
			Bc3Block result = new Bc3Block();
			result.colorBlock = bc1BlockEncoder.EncodeBlock(context);
			result.alphaBlock = bc4BlockEncoder.EncodeBlock(context.RawBlock, context.Quality);

			return result;
		}
	}
}
