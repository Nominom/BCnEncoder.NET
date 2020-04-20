using System;
using System.Collections.Generic;
using System.Text;
using BCnEnc.Net.Shared;

namespace BCnEnc.Net.Encoder.Bc7
{
	internal static class Bc7Mode4Encoder
	{

		private static ReadOnlySpan<int> partitionTable => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		const int subset = 0;

		public static Bc7Block EncodeBlock(RawBlock4X4Rgba32 block, int startingVariation)
		{
			var type = Bc7BlockType.Type4;

			Span<Bc7Block> outputs = stackalloc Bc7Block[8];

			for (int idxMode = 0; idxMode < 2; idxMode++)
			{
				for (int rotation = 0; rotation < 4; rotation++)
				{
					var rotatedBlock = Bc7EncodingHelpers.RotateBlockColors(block, rotation);
					Bc7Block output = new Bc7Block();

					Bc7EncodingHelpers.GetInitialUnscaledEndpoints(rotatedBlock, out var ep0, out var ep1);

					ColorRgba32 scaledEp0 =
						Bc7EncodingHelpers.ScaleDownEndpoint(ep0, type, false, out byte _);
					ColorRgba32 scaledEp1 =
						Bc7EncodingHelpers.ScaleDownEndpoint(ep1, type, false, out byte _);

					byte pBit = 0; //fake pBit

					Bc7EncodingHelpers.OptimizeSubsetEndpointsWithPBit(type, rotatedBlock, ref scaledEp0,
						ref scaledEp1, ref pBit, ref pBit, startingVariation, partitionTable, subset,
						false, true, idxMode);

					ep0 = Bc7EncodingHelpers.ExpandEndpoint(type, scaledEp0, 0);
					ep1 = Bc7EncodingHelpers.ExpandEndpoint(type, scaledEp1, 0);
					byte[] colorIndices = new byte[16];
					byte[] alphaIndices = new byte[16];
					Bc7EncodingHelpers.FillAlphaColorIndices(type, rotatedBlock,
						ep0,
						ep1,
						colorIndices, alphaIndices, idxMode);

					bool needsRedo = false;


					if ((colorIndices[0] & (idxMode == 0 ? 0b10 : 0b100)) > 0) //If anchor index most significant bit is 1, switch endpoints
					{
						var c = scaledEp0;
						var alpha0 = scaledEp0.a;
						var alpha1 = scaledEp1.a;

						scaledEp0 = scaledEp1;
						scaledEp1 = c;
						scaledEp0.a = alpha0;
						scaledEp1.a = alpha1;

						needsRedo = true;
					}
					if ((alphaIndices[0] & (idxMode == 0 ? 0b100 : 0b10)) > 0) //If anchor index most significant bit is 1, switch endpoints
					{
						var a = scaledEp0.a;

						scaledEp0.a = scaledEp1.a;
						scaledEp1.a = a;

						needsRedo = true;
					}

					if (needsRedo)
					{
						//redo indices
						ep0 = Bc7EncodingHelpers.ExpandEndpoint(type, scaledEp0, 0);
						ep1 = Bc7EncodingHelpers.ExpandEndpoint(type, scaledEp1, 0);
						Bc7EncodingHelpers.FillAlphaColorIndices(type, rotatedBlock,
							ep0,
							ep1,
							colorIndices, alphaIndices, idxMode);
					}

					if (idxMode == 0)
					{
						output.PackType4(rotation, (byte)idxMode, new[]{
								new byte[]{scaledEp0.r, scaledEp0.g, scaledEp0.b},
								new byte[]{scaledEp1.r, scaledEp1.g, scaledEp1.b},
							},
							new[] { scaledEp0.a, scaledEp1.a },
							colorIndices, alphaIndices);
					}
					else
					{
						output.PackType4(rotation, (byte)idxMode, new[]{
								new byte[]{scaledEp0.r, scaledEp0.g, scaledEp0.b},
								new byte[]{scaledEp1.r, scaledEp1.g, scaledEp1.b},
							},
							new[] { scaledEp0.a, scaledEp1.a },
							alphaIndices, colorIndices);
					}


					outputs[idxMode * 4 + rotation] = output;
				}
			}

			int bestIndex = 0;
			float bestError = 0;
			bool first = true;

			// Find best out of generated blocks
			for (int i = 0; i < outputs.Length; i++)
			{
				var decoded = outputs[i].Decode();

				float error = block.CalculateYCbCrAlphaError(decoded);
				if (error < bestError || first)
				{
					first = false;
					bestError = error;
					bestIndex = i;
				}
			}

			return outputs[bestIndex];
		}
	}
}
