using System;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Bptc
{
	internal static class Bc7Mode6Encoder
	{

		public static Bc7Block EncodeBlock(RawBlock4X4RgbaFloat block, int startingVariation, in RgbEncodingContext context)
		{
			var hasAlpha = block.HasTransparentPixels();

			var output = new Bc7Block();
			Bc7EncodingHelpers.GetInitialUnscaledEndpoints(block, out var ep0, out var ep1, context.Weights);

			var scaledEp0 =
				Bc7EncodingHelpers.ScaleDownEndpoint(ep0, Bc7BlockType.Type6, false, out var pBit0);
			var scaledEp1 =
				Bc7EncodingHelpers.ScaleDownEndpoint(ep1, Bc7BlockType.Type6, false, out var pBit1);

			ReadOnlySpan<int> partitionTable = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			const int subset = 0;

			//Force 255 alpha if fully opaque
			if (!hasAlpha)
			{
				pBit0 = 1;
				pBit1 = 1;
			}

			Bc7EncodingHelpers.OptimizeSubsetEndpointsWithPBit(Bc7BlockType.Type6, block, ref scaledEp0,
				ref scaledEp1, ref pBit0, ref pBit1, startingVariation, partitionTable, subset, hasAlpha, hasAlpha);

			ep0 = Bc7EncodingHelpers.ExpandEndpoint(Bc7BlockType.Type6, scaledEp0, pBit0);
			ep1 = Bc7EncodingHelpers.ExpandEndpoint(Bc7BlockType.Type6, scaledEp1, pBit1);
			var indices = new byte[16];
			Bc7EncodingHelpers.FillSubsetIndices(Bc7BlockType.Type6, block,
				ep0,
				ep1,
				partitionTable, subset, indices);



			if ((indices[0] & 0b1000) > 0) //If anchor index most significant bit is 1, switch endpoints
			{
				var c = scaledEp0;
				var p = pBit0;

				scaledEp0 = scaledEp1;
				pBit0 = pBit1;
				scaledEp1 = c;
				pBit1 = p;

				//redo indices
				ep0 = Bc7EncodingHelpers.ExpandEndpoint(Bc7BlockType.Type6, scaledEp0, pBit0);
				ep1 = Bc7EncodingHelpers.ExpandEndpoint(Bc7BlockType.Type6, scaledEp1, pBit1);
				Bc7EncodingHelpers.FillSubsetIndices(Bc7BlockType.Type6, block,
					ep0,
					ep1,
					partitionTable, subset, indices);
			}

			output.PackType6(new[]{
					new byte[]{scaledEp0.r, scaledEp0.g, scaledEp0.b, scaledEp0.a},
					new byte[]{scaledEp1.r, scaledEp1.g, scaledEp1.b, scaledEp1.a},
				},
				new[] { pBit0, pBit1 },
				indices);

			return output;
		}
	}
}
