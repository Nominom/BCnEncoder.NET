using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Bc7
{
	internal class Bc7Encoder : IBcBlockEncoder
	{

		public unsafe byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, bool parallel)
		{
			byte[] outputData = new byte[blockWidth * blockHeight * sizeof(Bc7Block)];
			fixed (byte* oDataBytes = outputData)
			{
				Bc7Block* oDataBlocks = (Bc7Block*)oDataBytes;
				int oDataBlocksLength = outputData.Length / sizeof(Bc7Block);

				for (int i = 0; i < oDataBlocksLength; i++)
				{
					oDataBlocks[i] = EncodeBlock(blocks[i], quality);
				}
			}

			return outputData;
		}

		public GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GL_COMPRESSED_RGBA_BPTC_UNORM_ARB;
		}

		public GLFormat GetBaseInternalFormat()
		{
			return GLFormat.GL_RGBA;
		}

		public DXGI_FORMAT GetDxgiFormat() {
			return DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM;
		}

		private static ClusterIndices4X4 CreateClusterIndexBlock(RawBlock4X4Rgba32 raw, out int outputNumClusters, 
			int numClusters = 3)
		{

			ClusterIndices4X4 indexBlock = new ClusterIndices4X4();

			var indices = LinearClustering.ClusterPixels(raw.AsArray, 4, 4,
				numClusters, 1, 10, false);

			var output = indexBlock.AsArray;
			for (int i = 0; i < output.Length; i++)
			{
				output[i] = indices[i];
			}

			int nClusters = indexBlock.NumClusters;
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
			private const float errorThreshold = 0.005f;
			private const int maxTries = 5;

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
					for (int i = 0; i < 64; i++) {
						if(best3SubsetPartitions[i] < 16) {
							yield return Bc7Mode0Encoder.EncodeBlock(rawBlock, 3, best3SubsetPartitions[i]);
						}
						
						yield return Bc7Mode1Encoder.EncodeBlock(rawBlock, 4, best2SubsetPartitions[i]);
						
					}
				}
			}

			public static Bc7Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				bool hasAlpha = rawBlock.HasTransparentPixels();

				var indexBlock2 = CreateClusterIndexBlock(rawBlock, out int clusters2, 2);
				var indexBlock3 = CreateClusterIndexBlock(rawBlock, out int clusters3, 3);

				if (clusters2 < 2) {
					clusters2 = clusters3;
					indexBlock2 = indexBlock3;
				}

				int[] best2SubsetPartitions = Bc7EncodingHelpers.Rank2SubsetPartitions(indexBlock2, clusters2);
				int[] best3SubsetPartitions = Bc7EncodingHelpers.Rank3SubsetPartitions(indexBlock3, clusters3);

				float bestError = 99999;
				Bc7Block best = new Bc7Block();
				int tries = 0;
				foreach (Bc7Block block in TryMethods(rawBlock, best2SubsetPartitions, best3SubsetPartitions, hasAlpha)) {
					var decoded = block.Decode();
					float error = rawBlock.CalculateYCbCrAlphaError(decoded);
					tries++;

					if(error < bestError) {
						best = block;
						bestError = error;
					}

					if (error < errorThreshold || tries > maxTries) {
						break;
					}

				}

				return best;
			}
		}

		private static class Bc7EncoderBalanced
		{
			private const float errorThreshold = 0.005f;
			private const int maxTries = 25;

			private static IEnumerable<Bc7Block> TryMethods(RawBlock4X4Rgba32 rawBlock, int[] best2SubsetPartitions, int[] best3SubsetPartitions, bool alpha)
			{
				if (alpha)
				{
					yield return Bc7Mode6Encoder.EncodeBlock(rawBlock, 6);
					yield return Bc7Mode5Encoder.EncodeBlock(rawBlock, 4);
					yield return Bc7Mode4Encoder.EncodeBlock(rawBlock, 4);
					for (int i = 0; i < 64; i++)
					{
						yield return Bc7Mode7Encoder.EncodeBlock(rawBlock, 3, best2SubsetPartitions[i]);
					}
				}
				else
				{
					yield return Bc7Mode6Encoder.EncodeBlock(rawBlock, 6);
					yield return Bc7Mode5Encoder.EncodeBlock(rawBlock, 4);
					yield return Bc7Mode4Encoder.EncodeBlock(rawBlock, 4);
					for (int i = 0; i < 64; i++) {
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
				bool hasAlpha = rawBlock.HasTransparentPixels();

				var indexBlock2 = CreateClusterIndexBlock(rawBlock, out int clusters2, 2);
				var indexBlock3 = CreateClusterIndexBlock(rawBlock, out int clusters3, 3);

				if (clusters2 < 2) {
					clusters2 = clusters3;
					indexBlock2 = indexBlock3;
				}

				int[] best2SubsetPartitions = Bc7EncodingHelpers.Rank2SubsetPartitions(indexBlock2, clusters2);
				int[] best3SubsetPartitions = Bc7EncodingHelpers.Rank3SubsetPartitions(indexBlock3, clusters3);

				float bestError = 99999;
				Bc7Block best = new Bc7Block();
				int tries = 0;
				foreach (Bc7Block block in TryMethods(rawBlock, best2SubsetPartitions, best3SubsetPartitions, hasAlpha)) {
					var decoded = block.Decode();
					float error = rawBlock.CalculateYCbCrAlphaError(decoded);
					tries++;

					if(error < bestError) {
						best = block;
						bestError = error;
					}

					if (error < errorThreshold || tries > maxTries) {
						break;
					}

				}

				return best;
			}
		}

		private static class Bc7EncoderBestQuality
		{

			private const float errorThreshold = 0.001f;
			private const int maxTries = 40;

			private static IEnumerable<Bc7Block> TryMethods(RawBlock4X4Rgba32 rawBlock, int[] best2SubsetPartitions, int[] best3SubsetPartitions, bool alpha)
			{
				if (alpha)
				{
					yield return Bc7Mode6Encoder.EncodeBlock(rawBlock, 8);
					yield return Bc7Mode5Encoder.EncodeBlock(rawBlock, 5);
					yield return Bc7Mode4Encoder.EncodeBlock(rawBlock, 5);
					for (int i = 0; i < 64; i++)
					{
						yield return Bc7Mode7Encoder.EncodeBlock(rawBlock, 4, best2SubsetPartitions[i]);

					}
				}
				else
				{
					yield return Bc7Mode6Encoder.EncodeBlock(rawBlock, 8);
					yield return Bc7Mode5Encoder.EncodeBlock(rawBlock, 5);
					yield return Bc7Mode4Encoder.EncodeBlock(rawBlock, 5);
					for (int i = 0; i < 64; i++) {
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
				bool hasAlpha = rawBlock.HasTransparentPixels();

				var indexBlock2 = CreateClusterIndexBlock(rawBlock, out int clusters2, 2);
				var indexBlock3 = CreateClusterIndexBlock(rawBlock, out int clusters3, 3);

				if (clusters2 < 2) {
					clusters2 = clusters3;
					indexBlock2 = indexBlock3;
				}

				int[] best2SubsetPartitions = Bc7EncodingHelpers.Rank2SubsetPartitions(indexBlock2, clusters2);
				int[] best3SubsetPartitions = Bc7EncodingHelpers.Rank3SubsetPartitions(indexBlock3, clusters3);


				float bestError = 99999;
				Bc7Block best = new Bc7Block();
				int tries = 0;
				foreach (Bc7Block block in TryMethods(rawBlock, best2SubsetPartitions, best3SubsetPartitions, hasAlpha)) {
					var decoded = block.Decode();
					float error = rawBlock.CalculateYCbCrAlphaError(decoded);
					tries++;

					if(error < bestError) {
						best = block;
						bestError = error;
					}

					if (error < errorThreshold || tries > maxTries) {
						break;
					}

				}

				return best;
			}
		}
	}
}
