using System;
using System.Collections.Generic;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder.Bptc
{
	internal class Bc6Encoder : BaseBcBlockEncoder<Bc6Block>
	{
		private readonly bool signed;

		public Bc6Encoder(bool signed)
		{
			this.signed = signed;
		}

		public override Bc6Block EncodeBlock(RawBlock4X4RgbaFloat block, OperationContext context)
		{
			// TODO: Do better.
			block.ColorConvert(context.ColorConversionMode);

			switch (context.Quality)
			{
				case CompressionQuality.Fast:
					return Bc6EncoderFast.EncodeBlock(block, context, signed);
				case CompressionQuality.Balanced:
					return Bc6EncoderBalanced.EncodeBlock(block, context, signed);
				case CompressionQuality.BestQuality:
					return Bc6EncoderBestQuality.EncodeBlock(block, context, signed);
				default:
					throw new ArgumentOutOfRangeException(nameof(context.Quality), context.Quality, null);
			}
		}

		internal static ClusterIndices4X4 CreateClusterIndexBlock(RawBlock4X4RgbaFloat raw, out int outputNumClusters,
			int numClusters = 2)
		{

			var indexBlock = new ClusterIndices4X4();

			var indices = LinearClustering.ClusterPixels(raw.AsSpan, 4, 4,
				numClusters, 1, 10, false);

			var output = indexBlock.AsSpan;
			for (var i = 0; i < output.Length; i++)
			{
				output[i] = indices[i];
			}

			var nClusters = indexBlock.NumClusters;
			if (nClusters < numClusters)
			{
				indexBlock = indexBlock.Reduce(out nClusters);
			}

			outputNumClusters = nClusters;
			return indexBlock;
		}

		internal static class Bc6EncoderFast
		{
			internal static Bc6Block EncodeBlock(RawBlock4X4RgbaFloat block, OperationContext context, bool signed)
			{
				RgbBoundingBox.CreateFloat(block.AsSpan, out var min, out var max);
				ColorRgbFloat minRgb = min, maxRgb = max;

				LeastSquares.OptimizeEndpoints1Sub(block, ref minRgb, ref maxRgb);

				return Bc6ModeEncoder.EncodeBlock1Sub(Bc6BlockType.Type3, block, min, max, signed, out _);
			}
		}

		internal static class Bc6EncoderBalanced
		{

			private const float TargetError = 0.001f;
			private const int MaxTries = 10;

			private static IEnumerable<Bc6Block> GenerateCandidates(RawBlock4X4RgbaFloat block, OperationContext context, bool signed)
			{
				var candidates = 0;
				Bc6EncodingHelpers.GetInitialUnscaledEndpoints(block, out var ep0Sub1, out var ep1Sub1, context.Weights);

				if (!signed)
				{
					LeastSquares.OptimizeEndpoints1Sub(block, ref ep0Sub1, ref ep1Sub1);
				}

				ep0Sub1.ClampToHalf();
				ep1Sub1.ClampToHalf();

				if (!signed)
				{
					ep0Sub1.ClampToPositive();
					ep1Sub1.ClampToPositive();
				}

				//Type3 Always ok!
				yield return Bc6ModeEncoder.EncodeBlock1Sub(Bc6BlockType.Type3, block, ep0Sub1, ep1Sub1,
					signed, out _);
				candidates++;

				var type15Block = Bc6ModeEncoder.EncodeBlock1Sub(Bc6BlockType.Type15, block, ep0Sub1, ep1Sub1,
					signed, out var badType15);
				candidates++;
				if (!badType15)
				{
					yield return type15Block;
				}
				else
				{
					var indexBlock = CreateClusterIndexBlock(block, out var numClusters, 2);
					var best2SubsetPartitions = BptcEncodingHelpers.Rank2SubsetPartitions(indexBlock, numClusters, true);

					foreach (var subsetPartition in best2SubsetPartitions)
					{
						Bc6EncodingHelpers.GetInitialUnscaledEndpointsForSubset(block, out var ep0, out var ep1, subsetPartition, 0, context.Weights);
						Bc6EncodingHelpers.GetInitialUnscaledEndpointsForSubset(block, out var ep2, out var ep3, subsetPartition, 1, context.Weights);

						if (!signed)
						{
							LeastSquares.OptimizeEndpoints2Sub(block, ref ep0, ref ep1, subsetPartition, 0);
							LeastSquares.OptimizeEndpoints2Sub(block, ref ep2, ref ep3, subsetPartition, 1);
						}

						ep0.ClampToHalf();
						ep1.ClampToHalf();
						ep2.ClampToHalf();
						ep3.ClampToHalf();

						if (!signed)
						{
							ep0.ClampToPositive();
							ep1.ClampToPositive();
							ep2.ClampToPositive();
							ep3.ClampToPositive();
						}

						{
							var type1Block = Bc6ModeEncoder.EncodeBlock2Sub(Bc6BlockType.Type1, block, ep0, ep1, ep2, ep3,
								subsetPartition, signed, out var badType1);
							candidates++;

							if (!badType1)
							{
								yield return type1Block;
							}

							if (candidates >= MaxTries)
							{
								yield break;
							}
						}

						{
							var type14Block = Bc6ModeEncoder.EncodeBlock2Sub(Bc6BlockType.Type14, block, ep0, ep1, ep2, ep3,
								subsetPartition, signed, out var badType14);
							candidates++;

							if (!badType14)
							{
								yield return type14Block;
							}

							if (candidates >= MaxTries)
							{
								yield break;
							}
						}
					}
				}
			}

			internal static Bc6Block EncodeBlock(RawBlock4X4RgbaFloat block, OperationContext context, bool signed)
			{
				var result = new Bc6Block();
				var bestError = 9999999f;

				foreach (var candidate in GenerateCandidates(block, context, signed))
				{
					var error = block.CalculateError(candidate.Decode(signed));

					if (error < bestError)
					{
						result = candidate;
						bestError = error;
					}

					if (error <= TargetError)
					{
						break;
					}
				}

				return result;
			}
		}

		internal static class Bc6EncoderBestQuality
		{
			private const float TargetError = 0.0005f;
			private const int MaxTries = 500;

			private static IEnumerable<Bc6Block> GenerateCandidates(RawBlock4X4RgbaFloat block, OperationContext context, bool signed)
			{
				var candidates = 0;
				Bc6EncodingHelpers.GetInitialUnscaledEndpoints(block, out var ep0Sub1, out var ep1Sub1, context.Weights);

				if (!signed)
				{
					LeastSquares.OptimizeEndpoints1Sub(block, ref ep0Sub1, ref ep1Sub1);
				}

				ep0Sub1.ClampToHalf();
				ep1Sub1.ClampToHalf();

				if (!signed)
				{
					ep0Sub1.ClampToPositive();
					ep1Sub1.ClampToPositive();
				}
				//Type3 Always ok!
				yield return Bc6ModeEncoder.EncodeBlock1Sub(Bc6BlockType.Type3, block, ep0Sub1, ep1Sub1,
					signed, out _);
				candidates++;

				//Type7
				{
					var type7Block = Bc6ModeEncoder.EncodeBlock1Sub(Bc6BlockType.Type7, block, ep0Sub1, ep1Sub1,
						signed, out var badType7);
					candidates++;
					if (!badType7)
					{
						yield return type7Block;
					}
				}
				//Type11
				{
					var type11Block = Bc6ModeEncoder.EncodeBlock1Sub(Bc6BlockType.Type11, block, ep0Sub1, ep1Sub1,
						signed, out var badType11);
					candidates++;
					if (!badType11)
					{
						yield return type11Block;
					}
				}
				//Type15
				{
					var type15Block = Bc6ModeEncoder.EncodeBlock1Sub(Bc6BlockType.Type15, block, ep0Sub1, ep1Sub1,
						signed, out var badType15);
					candidates++;
					if (!badType15)
					{
						yield return type15Block;
					}
				}

				var indexBlock = CreateClusterIndexBlock(block, out var numClusters, 2);
				var best2SubsetPartitions = BptcEncodingHelpers.Rank2SubsetPartitions(indexBlock, numClusters, true);

				foreach (var subsetPartition in best2SubsetPartitions)
				{
					Bc6EncodingHelpers.GetInitialUnscaledEndpointsForSubset(block, out var ep0, out var ep1, subsetPartition, 0, context.Weights);
					Bc6EncodingHelpers.GetInitialUnscaledEndpointsForSubset(block, out var ep2, out var ep3, subsetPartition, 1, context.Weights);

					if (!signed)
					{
						LeastSquares.OptimizeEndpoints2Sub(block, ref ep0, ref ep1, subsetPartition, 0);
						LeastSquares.OptimizeEndpoints2Sub(block, ref ep2, ref ep3, subsetPartition, 1);
					}

					ep0.ClampToHalf();
					ep1.ClampToHalf();
					ep2.ClampToHalf();
					ep3.ClampToHalf();

					if (!signed)
					{
						ep0.ClampToPositive();
						ep1.ClampToPositive();
						ep2.ClampToPositive();
						ep3.ClampToPositive();
					}

					foreach (var type in Bc6Block.Subsets2Types)
					{
						var sub2Block = Bc6ModeEncoder.EncodeBlock2Sub(type, block, ep0, ep1, ep2, ep3,
							subsetPartition, signed, out var badTransform);
						candidates++;

						if (!badTransform)
						{
							yield return sub2Block;
						}

						if (candidates >= MaxTries)
						{
							yield break;
						}
					}
				}
			}

			internal static Bc6Block EncodeBlock(RawBlock4X4RgbaFloat block, OperationContext context, bool signed)
			{
				var result = new Bc6Block();
				float bestError = 9999999;

				foreach (var candidate in GenerateCandidates(block, context, signed))
				{
					var error = block.CalculateError(candidate.Decode(signed));

					if (error < bestError)
					{
						result = candidate;
						bestError = error;
					}

					if (error <= TargetError)
					{
						break;
					}
				}

				return result;
			}
		}
	}
}
