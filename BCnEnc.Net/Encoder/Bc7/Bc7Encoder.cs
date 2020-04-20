using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using BCnEnc.Net.Encoder.Bc7;
using BCnEnc.Net.Shared;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Encoder
{
	internal class Bc7Encoder : IBcBlockEncoder
	{

		private int numClusters;
		private int[] clusterIndices;
		private int pixelWidth;
		private int pixelHeight;

		void IBcBlockEncoder.SetReferenceData(ReadOnlySpan<Rgba32> originalPixels, int pixelWidth, int pixelHeight)
		{
			this.pixelWidth = pixelWidth;
			this.pixelHeight = pixelHeight;
			numClusters = (pixelWidth / 4) * (pixelHeight / 4);
			if (numClusters > 4)
			{
				clusterIndices = LinearClustering.ClusterPixels(originalPixels, pixelWidth, pixelHeight,
					numClusters, 10f, 5);
			}
			else
			{
				clusterIndices = new int[originalPixels.Length];
			}
		}
		public byte[] Encode(RawBlock4X4Rgba32[,] blocks, int blockWidth, int blockHeight, EncodingQuality quality,
			bool parallel = true)
		{

			if (clusterIndices == null)
			{
				throw new InvalidOperationException("Reference data needs to be passed before calling Encode");
			}

			byte[] outputData = new byte[blockWidth * blockHeight * Marshal.SizeOf<Bc7Block>()];
			RawBlock4X4Rgba32[] inputBlocks = blocks.Reshape(MatrixOrder.FortranColumnMajor);
			ClusterIndices4X4[] inputClusterIndices = CreateClusterIndexBlocks(blockWidth, blockHeight);

			Span<Bc7Block> outputBlocks = MemoryMarshal.Cast<byte, Bc7Block>(outputData);

			Debug.Assert(inputBlocks.Length == inputClusterIndices.Length);

#if DEBUG
			int[] subsetCounts = new int[3];
			foreach (var indices4X4 in inputClusterIndices)
			{
				var count = indices4X4.NumClusters;
				if (count == 1)
				{
					subsetCounts[0]++;
				}
				else if (count == 2)
				{
					subsetCounts[1]++;
				}
				else
				{
					subsetCounts[2]++;
				}
			}
#endif

			if (parallel)
			{
				Parallel.For(0, inputBlocks.Length, i =>
				{
					Span<Bc7Block> outputBlocks = MemoryMarshal.Cast<byte, Bc7Block>(outputData);
					outputBlocks[i] = EncodeBlock(inputBlocks[i], inputClusterIndices[i], quality);
				});
			}
			else
			{
				for (int i = 0; i < inputBlocks.Length; i++)
				{
					outputBlocks[i] = EncodeBlock(inputBlocks[i], inputClusterIndices[i], quality);
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

		private ClusterIndices4X4[] CreateClusterIndexBlocks(int blockWidth, int blockHeight)
		{

			ClusterIndices4X4[] indexBlocks = new ClusterIndices4X4[blockWidth * blockHeight];

			for (int x = 0; x < pixelWidth; x++)
			{
				for (int y = 0; y < pixelHeight; y++)
				{
					int index = clusterIndices[x + y * pixelWidth];
					int blockIndexX = (int)MathF.Floor(x / 4.0f);
					int blockIndexY = (int)MathF.Floor(y / 4.0f);
					int bI = blockIndexX + blockIndexY * blockWidth;

					int blockInternalIndexX = x % 4;
					int blockInternalIndexY = y % 4;

					indexBlocks[bI][blockInternalIndexX, blockInternalIndexY] = index;
				}
			}

			return indexBlocks;
		}


		private Bc7Block EncodeBlock(RawBlock4X4Rgba32 rawBlock, ClusterIndices4X4 indicesBlock, EncodingQuality quality)
		{
			switch (quality)
			{
				case EncodingQuality.Fast:
					return Bc7EncoderFast.EncodeBlock(rawBlock, indicesBlock);
				case EncodingQuality.Balanced:
					return Bc7EncoderBalanced.EncodeBlock(rawBlock, indicesBlock);
				case EncodingQuality.BestQuality:
					return Bc7EncoderBestQuality.EncodeBlock(rawBlock, indicesBlock);
				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		private static class Bc7EncoderFast
		{


			public static Bc7Block EncodeBlock(RawBlock4X4Rgba32 rawBlock, ClusterIndices4X4 indicesBlock)
			{
				bool hasAlpha = rawBlock.HasTransparentPixels();

				var reduced = indicesBlock.Reduce(out int numDistinctClusters);

				int best2SubsetPartition = Bc7EncodingHelpers.SelectBest2SubsetPartition(reduced, numDistinctClusters, out int subset2Error);
				int best3SubsetPartition = Bc7EncodingHelpers.SelectBest3SubsetPartition(reduced, numDistinctClusters, out int subset3Error);

				List<Bc7Block> blocks = new List<Bc7Block>();

				if (hasAlpha)
				{
					blocks.Add(Bc7Mode5Encoder.EncodeBlock(rawBlock, 5));
					blocks.Add(Bc7Mode6Encoder.EncodeBlock(rawBlock, 3));
				}
				else
				{

					blocks.Add(Bc7Mode6Encoder.EncodeBlock(rawBlock, 4));

					blocks.Add(Bc7Mode1Encoder.EncodeBlock(rawBlock, 2, best2SubsetPartition));
					//blocks.Add(Bc7Mode6Encoder.EncodeBlock(rawBlock, 3));
				}

				int bestIndex = 0;
				float bestError = 0;
				bool first = true;


				for (int i = 0; i < blocks.Count; i++)
				{
					var decoded = blocks[i].Decode();

					float error = rawBlock.CalculateYCbCrAlphaError(decoded);
					if (error < bestError || first)
					{
						first = false;
						bestError = error;
						bestIndex = i;
					}
				}

				return blocks[bestIndex];
			}
		}

		private static class Bc7EncoderBalanced
		{

			public static Bc7Block EncodeBlock(RawBlock4X4Rgba32 rawBlock, ClusterIndices4X4 indicesBlock)
			{
				bool hasAlpha = rawBlock.HasTransparentPixels();

				var reduced = indicesBlock.Reduce(out int numDistinctClusters);

				int best2SubsetPartition = Bc7EncodingHelpers.SelectBest2SubsetPartition(reduced, numDistinctClusters, out int subset2Error);
				int best3SubsetPartition = Bc7EncodingHelpers.SelectBest3SubsetPartition(reduced, numDistinctClusters, out int subset3Error);

				List<Bc7Block> blocks = new List<Bc7Block>();

				if (hasAlpha)
				{
					blocks.Add(Bc7Mode5Encoder.EncodeBlock(rawBlock, 5));
					blocks.Add(Bc7Mode6Encoder.EncodeBlock(rawBlock, 5));
					blocks.Add(Bc7Mode7Encoder.EncodeBlock(rawBlock, 3, best2SubsetPartition));
				}
				else
				{
					if (best3SubsetPartition < 16)
					{
						blocks.Add(Bc7Mode0Encoder.EncodeBlock(rawBlock, 3, best3SubsetPartition));
					}
					else
					{
						blocks.Add(Bc7Mode2Encoder.EncodeBlock(rawBlock, 3, best3SubsetPartition));
					}

					blocks.Add(Bc7Mode6Encoder.EncodeBlock(rawBlock, 5));

					blocks.Add(Bc7Mode1Encoder.EncodeBlock(rawBlock, 3, best2SubsetPartition));
					blocks.Add(Bc7Mode3Encoder.EncodeBlock(rawBlock, 5, best2SubsetPartition));

				}

				int bestIndex = 0;
				float bestError = 0;
				bool first = true;


				for (int i = 0; i < blocks.Count; i++)
				{
					var decoded = blocks[i].Decode();

					float error = rawBlock.CalculateYCbCrAlphaError(decoded);
					if (error < bestError || first)
					{
						first = false;
						bestError = error;
						bestIndex = i;
					}
				}

				return blocks[bestIndex];
			}
		}

		private static class Bc7EncoderBestQuality
		{

			public static Bc7Block EncodeBlock(RawBlock4X4Rgba32 rawBlock, ClusterIndices4X4 indicesBlock)
			{
				bool hasAlpha = rawBlock.HasTransparentPixels();

				var reduced = indicesBlock.Reduce(out int numDistinctClusters);

				int best2SubsetPartition = Bc7EncodingHelpers.SelectBest2SubsetPartition(reduced, numDistinctClusters, out int subset2Error);
				int best3SubsetPartition = Bc7EncodingHelpers.SelectBest3SubsetPartition(reduced, numDistinctClusters, out int subset3Error);

				List<Bc7Block> blocks = new List<Bc7Block>();

				if (hasAlpha)
				{
					blocks.Add(Bc7Mode4Encoder.EncodeBlock(rawBlock, 5));
					blocks.Add(Bc7Mode5Encoder.EncodeBlock(rawBlock, 5));
					blocks.Add(Bc7Mode6Encoder.EncodeBlock(rawBlock, 8));
					blocks.Add(Bc7Mode7Encoder.EncodeBlock(rawBlock, 4, best2SubsetPartition));
				}
				else
				{
					if (best3SubsetPartition < 16)
					{
						blocks.Add(Bc7Mode0Encoder.EncodeBlock(rawBlock, 4, best3SubsetPartition));
					}
					blocks.Add(Bc7Mode2Encoder.EncodeBlock(rawBlock, 5, best3SubsetPartition));

					blocks.Add(Bc7Mode4Encoder.EncodeBlock(rawBlock, 5));
					blocks.Add(Bc7Mode5Encoder.EncodeBlock(rawBlock, 5));

					blocks.Add(Bc7Mode6Encoder.EncodeBlock(rawBlock, 8));

					blocks.Add(Bc7Mode1Encoder.EncodeBlock(rawBlock, 4, best2SubsetPartition));
					blocks.Add(Bc7Mode3Encoder.EncodeBlock(rawBlock, 5, best2SubsetPartition));
				}

				int bestIndex = 0;
				float bestError = 0;
				bool first = true;


				for (int i = 0; i < blocks.Count; i++)
				{
					var decoded = blocks[i].Decode();

					float error = rawBlock.CalculateYCbCrAlphaError(decoded);
					if (error < bestError || first)
					{
						first = false;
						bestError = error;
						bestIndex = i;
					}
				}

				return blocks[bestIndex];
			}
		}
	}
}
