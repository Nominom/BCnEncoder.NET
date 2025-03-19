using System;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Bptc
{
	internal static class Bc7Mode4Encoder
	{

		private static ReadOnlySpan<int> PartitionTable => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		const int Subset = 0;

		public static Bc7Block EncodeBlock(RawBlock4X4RgbaFloat block, int startingVariation)
		{
			var type = Bc7BlockType.Type4;

			Span<Bc7Block> outputs = stackalloc Bc7Block[8];

			for (var idxMode = 0; idxMode < 2; idxMode++)
			{
				for (var rotation = 0; rotation < 4; rotation++)
				{
					var rotatedBlock = Bc7EncodingHelpers.RotateBlockColors(block, rotation);
					var output = new Bc7Block();

					Bc7EncodingHelpers.GetInitialUnscaledEndpoints(rotatedBlock, out var ep0, out var ep1);

					var scaledEp0 =
						Bc7EncodingHelpers.ScaleDownEndpoint(ep0, type, false, out var _);
					var scaledEp1 =
						Bc7EncodingHelpers.ScaleDownEndpoint(ep1, type, false, out var _);

					byte pBit = 0; //fake pBit

					Bc7EncodingHelpers.OptimizeSubsetEndpointsWithPBit(type, rotatedBlock, ref scaledEp0,
						ref scaledEp1, ref pBit, ref pBit, startingVariation, PartitionTable, Subset,
						false, true, idxMode);

					ep0 = Bc7EncodingHelpers.ExpandEndpoint(type, scaledEp0, 0);
					ep1 = Bc7EncodingHelpers.ExpandEndpoint(type, scaledEp1, 0);
					var colorIndices = new byte[16];
					var alphaIndices = new byte[16];
					Bc7EncodingHelpers.FillAlphaColorIndices(type, rotatedBlock,
						ep0,
						ep1,
						colorIndices, alphaIndices, idxMode);

					var needsRedo = false;


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

			var bestIndex = 0;
			float bestError = 0;
			var first = true;

			// Find best out of generated blocks
			for (var i = 0; i < outputs.Length; i++)
			{
				var decoded = outputs[i].Decode();

				var error = block.CalculateYCbCrAlphaError(decoded);
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
