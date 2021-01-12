using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Bc7
{
	internal class Bc7Encoder : IBcBlockEncoder
	{

		public byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, bool parallel)
		{
			var outputData = new byte[blockWidth * blockHeight * Marshal.SizeOf<Bc7Block>()];
			var outputBlocks = MemoryMarshal.Cast<byte, Bc7Block>(outputData);

			if (parallel)
			{
				Parallel.For(0, blocks.Length, i =>
				{
					var outputBlocks = MemoryMarshal.Cast<byte, Bc7Block>(outputData);
					outputBlocks[i] = EncodeBlock(blocks[i], quality);
				});
			}
			else
			{
				for (var i = 0; i < blocks.Length; i++)
				{
					outputBlocks[i] = EncodeBlock(blocks[i], quality);
				}
			}


			return outputData;
		}

		public GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlCompressedRgbaBptcUnormArb;
		}

		public GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRgba;
		}

		public DxgiFormat GetDxgiFormat() {
			return DxgiFormat.DxgiFormatBc7Unorm;
		}

		private static ClusterIndices4X4 CreateClusterIndexBlock(RawBlock4X4Rgba32 raw, out int outputNumClusters, 
			int numClusters = 3)
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


		private static Bc7Block EncodeBlock(RawBlock4X4Rgba32 rawBlock, CompressionQuality quality)
		{
			switch (quality)
			{
				case CompressionQuality.Fast:
					return Bc7EncoderFast.EncodeBlock(rawBlock);
				case CompressionQuality.Balanced:
					return Bc7EncoderBalanced.EncodeBlock(rawBlock);
				case CompressionQuality.BestQuality:
					return Bc7EncoderBestQuality.EncodeBlock(rawBlock);
				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		private static class Bc7EncoderFast
		{
			private const float ErrorThreshold_ = 0.005f;
			private const int MaxTries_ = 5;

			private static IEnumerable<Bc7Block> TryMethods(RawBlock4X4Rgba32 rawBlock, int[] best2SubsetPartitions, int[] best3SubsetPartitions, bool alpha)
			{
				if (alpha)
				{
					yield return Bc7Mode6Encoder.EncodeBlock(rawBlock, 5);
					yield return Bc7Mode5Encoder.EncodeBlock(rawBlock, 3);
				}
				else
				{
					yield return Bc7Mode6Encoder.EncodeBlock(rawBlock, 6);
					for (var i = 0; i < 64; i++) {
						if(best3SubsetPartitions[i] < 16) {
							yield return Bc7Mode0Encoder.EncodeBlock(rawBlock, 3, best3SubsetPartitions[i]);
						}
						
						yield return Bc7Mode1Encoder.EncodeBlock(rawBlock, 4, best2SubsetPartitions[i]);
						
					}
				}
			}

			public static Bc7Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var hasAlpha = rawBlock.HasTransparentPixels();

				var indexBlock2 = CreateClusterIndexBlock(rawBlock, out var clusters2, 2);
				var indexBlock3 = CreateClusterIndexBlock(rawBlock, out var clusters3, 3);

				if (clusters2 < 2) {
					clusters2 = clusters3;
					indexBlock2 = indexBlock3;
				}

				var best2SubsetPartitions = Bc7EncodingHelpers.Rank2SubsetPartitions(indexBlock2, clusters2);
				var best3SubsetPartitions = Bc7EncodingHelpers.Rank3SubsetPartitions(indexBlock3, clusters3);

				float bestError = 99999;
				var best = new Bc7Block();
				var tries = 0;
				foreach (var block in TryMethods(rawBlock, best2SubsetPartitions, best3SubsetPartitions, hasAlpha)) {
					var decoded = block.Decode();
					var error = rawBlock.CalculateYCbCrAlphaError(decoded);
					tries++;

					if(error < bestError) {
						best = block;
						bestError = error;
					}

					if (error < ErrorThreshold_ || tries > MaxTries_) {
						break;
					}

				}

				return best;
			}
		}

		private static class Bc7EncoderBalanced
		{
			private const float ErrorThreshold_ = 0.005f;
			private const int MaxTries_ = 25;

			private static IEnumerable<Bc7Block> TryMethods(RawBlock4X4Rgba32 rawBlock, int[] best2SubsetPartitions, int[] best3SubsetPartitions, bool alpha)
			{
				if (alpha)
				{
					yield return Bc7Mode6Encoder.EncodeBlock(rawBlock, 6);
					yield return Bc7Mode5Encoder.EncodeBlock(rawBlock, 4);
					yield return Bc7Mode4Encoder.EncodeBlock(rawBlock, 4);
					for (var i = 0; i < 64; i++)
					{
						yield return Bc7Mode7Encoder.EncodeBlock(rawBlock, 3, best2SubsetPartitions[i]);
					}
				}
				else
				{
					yield return Bc7Mode6Encoder.EncodeBlock(rawBlock, 6);
					yield return Bc7Mode5Encoder.EncodeBlock(rawBlock, 4);
					yield return Bc7Mode4Encoder.EncodeBlock(rawBlock, 4);
					for (var i = 0; i < 64; i++) {
						if(best3SubsetPartitions[i] < 16) {
							yield return Bc7Mode0Encoder.EncodeBlock(rawBlock, 3, best3SubsetPartitions[i]);
						}
						else {
							yield return Bc7Mode2Encoder.EncodeBlock(rawBlock, 5, best3SubsetPartitions[i]);
						}

						yield return Bc7Mode1Encoder.EncodeBlock(rawBlock, 4, best2SubsetPartitions[i]);
					}
				}
			}

			public static Bc7Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var hasAlpha = rawBlock.HasTransparentPixels();

				var indexBlock2 = CreateClusterIndexBlock(rawBlock, out var clusters2, 2);
				var indexBlock3 = CreateClusterIndexBlock(rawBlock, out var clusters3, 3);

				if (clusters2 < 2) {
					clusters2 = clusters3;
					indexBlock2 = indexBlock3;
				}

				var best2SubsetPartitions = Bc7EncodingHelpers.Rank2SubsetPartitions(indexBlock2, clusters2);
				var best3SubsetPartitions = Bc7EncodingHelpers.Rank3SubsetPartitions(indexBlock3, clusters3);

				float bestError = 99999;
				var best = new Bc7Block();
				var tries = 0;
				foreach (var block in TryMethods(rawBlock, best2SubsetPartitions, best3SubsetPartitions, hasAlpha)) {
					var decoded = block.Decode();
					var error = rawBlock.CalculateYCbCrAlphaError(decoded);
					tries++;

					if(error < bestError) {
						best = block;
						bestError = error;
					}

					if (error < ErrorThreshold_ || tries > MaxTries_) {
						break;
					}

				}

				return best;
			}
		}

		private static class Bc7EncoderBestQuality
		{

			private const float ErrorThreshold_ = 0.001f;
			private const int MaxTries_ = 40;

			private static IEnumerable<Bc7Block> TryMethods(RawBlock4X4Rgba32 rawBlock, int[] best2SubsetPartitions, int[] best3SubsetPartitions, bool alpha)
			{
				if (alpha)
				{
					yield return Bc7Mode6Encoder.EncodeBlock(rawBlock, 8);
					yield return Bc7Mode5Encoder.EncodeBlock(rawBlock, 5);
					yield return Bc7Mode4Encoder.EncodeBlock(rawBlock, 5);
					for (var i = 0; i < 64; i++)
					{
						yield return Bc7Mode7Encoder.EncodeBlock(rawBlock, 4, best2SubsetPartitions[i]);

					}
				}
				else
				{
					yield return Bc7Mode6Encoder.EncodeBlock(rawBlock, 8);
					yield return Bc7Mode5Encoder.EncodeBlock(rawBlock, 5);
					yield return Bc7Mode4Encoder.EncodeBlock(rawBlock, 5);
					for (var i = 0; i < 64; i++) {
						if(best3SubsetPartitions[i] < 16) {
							yield return Bc7Mode0Encoder.EncodeBlock(rawBlock, 4, best3SubsetPartitions[i]);
						}
						yield return Bc7Mode2Encoder.EncodeBlock(rawBlock, 5, best3SubsetPartitions[i]);

						yield return Bc7Mode1Encoder.EncodeBlock(rawBlock, 4, best2SubsetPartitions[i]);
						yield return Bc7Mode3Encoder.EncodeBlock(rawBlock, 5, best2SubsetPartitions[i]);

					}
				}
			}

			public static Bc7Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var hasAlpha = rawBlock.HasTransparentPixels();

				var indexBlock2 = CreateClusterIndexBlock(rawBlock, out var clusters2, 2);
				var indexBlock3 = CreateClusterIndexBlock(rawBlock, out var clusters3, 3);

				if (clusters2 < 2) {
					clusters2 = clusters3;
					indexBlock2 = indexBlock3;
				}

				var best2SubsetPartitions = Bc7EncodingHelpers.Rank2SubsetPartitions(indexBlock2, clusters2);
				var best3SubsetPartitions = Bc7EncodingHelpers.Rank3SubsetPartitions(indexBlock3, clusters3);


				float bestError = 99999;
				var best = new Bc7Block();
				var tries = 0;
				foreach (var block in TryMethods(rawBlock, best2SubsetPartitions, best3SubsetPartitions, hasAlpha)) {
					var decoded = block.Decode();
					var error = rawBlock.CalculateYCbCrAlphaError(decoded);
					tries++;

					if(error < bestError) {
						best = block;
						bestError = error;
					}

					if (error < ErrorThreshold_ || tries > MaxTries_) {
						break;
					}

				}

				return best;
			}
		}
	}
}
