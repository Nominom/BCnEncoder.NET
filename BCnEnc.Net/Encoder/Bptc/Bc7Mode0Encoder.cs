using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder.Bptc
{
	internal static class Bc7Mode0Encoder
	{

		public static Bc7Block EncodeBlock(RawBlock4X4RgbaFloat block, int startingVariation, int bestPartition)
		{
			var output = new Bc7Block();
			const Bc7BlockType type = Bc7BlockType.Type0;

			if(bestPartition >= 16)
			{
				throw new IndexOutOfRangeException("Mode0 only has 16 partitions");
			}

			var endpoints = new ColorRgba32[6];
			var pBits = new byte[6];
			ReadOnlySpan<int> partitionTable = Bc7Block.Subsets3PartitionTable[bestPartition];

			var indices = new byte[16];

			var anchorIndices = new int[] {
				0,
				Bc7Block.Subsets3AnchorIndices2[bestPartition],
				Bc7Block.Subsets3AnchorIndices3[bestPartition]
			};

			for (var subset = 0; subset < 3; subset++) {

				Bc7EncodingHelpers.GetInitialUnscaledEndpointsForSubset(block, out var ep0, out var ep1,
					partitionTable, subset);
				var scaledEp0 =
					Bc7EncodingHelpers.ScaleDownEndpoint(ep0, type, true, out var pBit0);
				var scaledEp1 =
					Bc7EncodingHelpers.ScaleDownEndpoint(ep1, type, true, out var pBit1);

				Bc7EncodingHelpers.OptimizeSubsetEndpointsWithPBit(type, block, ref scaledEp0,
					ref scaledEp1, ref pBit0, ref pBit1, startingVariation, partitionTable, subset, true, false);

				ep0 = Bc7EncodingHelpers.ExpandEndpoint(type, scaledEp0, pBit0);
				ep1 = Bc7EncodingHelpers.ExpandEndpoint(type, scaledEp1, pBit1);
				Bc7EncodingHelpers.FillSubsetIndices(type, block,
					ep0,
					ep1,
					partitionTable, subset, indices);

				if ((indices[anchorIndices[subset]] & 0b100) > 0) //If anchor index most significant bit is 1, switch endpoints
				{
					var c = scaledEp0;
					var p = pBit0;

					scaledEp0 = scaledEp1;
					pBit0 = pBit1;
					scaledEp1 = c;
					pBit1 = p;

					//redo indices
					ep0 = Bc7EncodingHelpers.ExpandEndpoint(type, scaledEp0, pBit0);
					ep1 = Bc7EncodingHelpers.ExpandEndpoint(type, scaledEp1, pBit1);
					Bc7EncodingHelpers.FillSubsetIndices(type, block,
						ep0,
						ep1,
						partitionTable, subset, indices);
				}

				endpoints[subset * 2] = scaledEp0;
				endpoints[subset * 2 + 1] = scaledEp1;
				pBits[subset * 2] = pBit0;
				pBits[subset * 2 + 1] = pBit1;
			}

			output.PackType0(bestPartition, new[]{
					new byte[]{endpoints[0].r, endpoints[0].g, endpoints[0].b},
					new byte[]{endpoints[1].r, endpoints[1].g, endpoints[1].b},
					new byte[]{endpoints[2].r, endpoints[2].g, endpoints[2].b},
					new byte[]{endpoints[3].r, endpoints[3].g, endpoints[3].b},
					new byte[]{endpoints[4].r, endpoints[4].g, endpoints[4].b},
					new byte[]{endpoints[5].r, endpoints[5].g, endpoints[5].b}
				},
				pBits,
				indices);

			return output;
		}
	}
}
