using System;
using System.Diagnostics;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;

namespace BCnEncoder.Encoder.Bptc
{
	internal class Bc6Encoder : BaseBcBlockEncoder<Bc6Block, RawBlock4X4RgbFloat>
	{
		private readonly bool signed;
		
		public Bc6Encoder(bool signed)
		{
			this.signed = signed;
		}


		public override GlInternalFormat GetInternalFormat()
		{
			return signed ? GlInternalFormat.GlCompressedRgbBptcSignedFloatArb : GlInternalFormat.GlCompressedRgbBptcUnsignedFloatArb;
		}

		public override GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRgb;
		}

		public override DxgiFormat GetDxgiFormat()
		{
			return signed ? DxgiFormat.DxgiFormatBc6HSf16 : DxgiFormat.DxgiFormatBc6HUf16;
		}

		public override Bc6Block EncodeBlock(RawBlock4X4RgbFloat block, CompressionQuality quality)
		{
			switch (quality)
			{
				case CompressionQuality.Fast:
					return Bc6EncoderFast.EncodeBlock(block, signed);
				case CompressionQuality.Balanced:
					return Bc6EncoderBalanced.EncodeBlock(block, signed);
				case CompressionQuality.BestQuality:
					return Bc6EncoderBestQuality.EncodeBlock(block, signed);
				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		internal static ClusterIndices4X4 CreateClusterIndexBlock(RawBlock4X4RgbFloat raw, out int outputNumClusters,
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

		internal static Bc6Block SelectBestCandidate(RawBlock4X4RgbFloat block, ReadOnlySpan<Bc6Block> candidates, bool signed)
		{
			Debug.Assert(candidates.Length > 0);
			var bestError = candidates[0].Decode(signed).CalculateError(block);
			var bestIdx = 0;
			for (var i = 1; i < candidates.Length; i++)
			{
				var error = candidates[i].Decode(signed).CalculateError(block);
				if (error < bestError)
				{
					bestError = error;
					bestIdx = i;
				}
			}
			return candidates[bestIdx];
		}

		internal static class Bc6EncoderFast
		{
			internal static Bc6Block EncodeBlock(RawBlock4X4RgbFloat block, bool signed)
			{
				var intBlock = RawBlock4X4RgbHalfInt.FromRawFloats(block, signed);
				RgbBoundingBox.CreateFloat(block.AsSpan, out var min, out var max);
				//var (ep0, ep1) = LeastSquares.OptimizeEndpoints1SubInt(intBlock, ref min, ref max, signed);
				//LeastSquares.OptimizeEndpoints1Sub(block, ref min, ref max);
				min.ClampToHalf();
				max.ClampToHalf();
				return Bc6ModeEncoder.EncodeBlock1Sub(Bc6BlockType.Type3, block, min, max, false, 0, signed, out _);
			}
		}

		internal static class Bc6EncoderBalanced
		{
			private const float targetError = 0.01f;
			
			internal static Bc6Block EncodeBlock(RawBlock4X4RgbFloat block, bool signed)
			{
				var indexBlock = CreateClusterIndexBlock(block, out var numClusters, 2);
				var best2SubsetPartitions = BptcEncodingHelpers.Rank2SubsetPartitions(indexBlock, numClusters, true);
				var bestPartition = best2SubsetPartitions[0];
				
				Bc6EncodingHelpers.GetInitialUnscaledEndpointsForSubset(block, out var ep0, out var ep1, bestPartition, 0);
				Bc6EncodingHelpers.GetInitialUnscaledEndpointsForSubset(block, out var ep2, out var ep3, bestPartition, 1);
				Bc6EncodingHelpers.GetInitialUnscaledEndpoints(block, out var ep0Sub1, out var ep1Sub1);

				ep0.ClampToHalf();
				ep1.ClampToHalf();
				ep2.ClampToHalf();
				ep3.ClampToHalf();
				ep0Sub1.ClampToHalf();
				ep1Sub1.ClampToHalf();

				if (!signed)
				{
					ep0.ClampToPositive();
					ep1.ClampToPositive();
					ep2.ClampToPositive();
					ep3.ClampToPositive();
					ep0Sub1.ClampToPositive();
					ep1Sub1.ClampToPositive();
				}
				
				var type1Block = Bc6ModeEncoder.EncodeBlock2Sub(Bc6BlockType.Type1, block, ep0, ep1, ep2, ep3,
					bestPartition, false, 0, signed, out var badType1);
				var type14Block = Bc6ModeEncoder.EncodeBlock2Sub(Bc6BlockType.Type14, block, ep0, ep1, ep2, ep3,
					bestPartition, false, 0, signed, out var badType14);

				var type15Block = Bc6ModeEncoder.EncodeBlock1Sub(Bc6BlockType.Type15, block, ep0Sub1, ep1Sub1,
					false, 0, signed, out var badType15);

				var backupCandidate = Bc6ModeEncoder.EncodeBlock1Sub(Bc6BlockType.Type3, block, ep0Sub1, ep1Sub1,
					true, targetError, signed, out _);

				Span<Bc6Block> candidates = stackalloc Bc6Block[3];
				var i = 0;
				if (!badType1) candidates[i++] = type1Block;
				if (!badType14) candidates[i++] = type14Block;
				if (!badType15) candidates[i++] = type15Block;

				if (i == 0)
				{
					return backupCandidate;
				}
				
				var bestCandidate = SelectBestCandidate(block, candidates.Slice(0, i), signed);

				Bc6Block finalCandidate;
				bool badFinal;
				if (bestCandidate.HasSubsets)
				{
					finalCandidate = Bc6ModeEncoder.EncodeBlock2Sub(bestCandidate.Type, block, ep0, ep1, ep2, ep3,
						bestPartition, true, targetError, signed, out badFinal);
				}
				else
				{
					finalCandidate = Bc6ModeEncoder.EncodeBlock1Sub(bestCandidate.Type, block, ep0Sub1, ep1Sub1,
						true, targetError, signed, out badFinal);
				}
				
				if (badFinal)
				{
					return backupCandidate;
				}
				
				var finalError = block.CalculateError(finalCandidate.Decode(signed));
				var backupError = block.CalculateError(backupCandidate.Decode(signed));
				
				return finalError < backupError ? finalCandidate : backupCandidate;
			}
		}

		internal static class Bc6EncoderBestQuality
		{
			internal static Bc6Block EncodeBlock(RawBlock4X4RgbFloat block, bool signed)
			{
				Bc6Block result = new Bc6Block();

				return result;
			}
		}
	}
}
