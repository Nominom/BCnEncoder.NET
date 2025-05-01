using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal class Bc5BlockEncoder : BaseBcBlockEncoder<Bc5Block>
	{
		private readonly Bc4ComponentBlockEncoder redBlockEncoder;
		private readonly Bc4ComponentBlockEncoder greenBlockEncoder;

		public Bc5BlockEncoder(ColorComponent component1, ColorComponent component2)
		{
			redBlockEncoder = new Bc4ComponentBlockEncoder(component1);
			greenBlockEncoder = new Bc4ComponentBlockEncoder(component2);
		}

		public override Bc5Block EncodeBlock(RawBlock4X4RgbaFloat block, OperationContext context)
		{
			return new Bc5Block
			{
				redBlock = redBlockEncoder.EncodeBlock(block, context.Quality),
				greenBlock = greenBlockEncoder.EncodeBlock(block, context.Quality)
			};
		}
	}
}
