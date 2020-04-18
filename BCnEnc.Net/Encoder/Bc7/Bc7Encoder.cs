using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
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
			numClusters = (pixelWidth / 3) * (pixelHeight / 3);
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



		private static ColorRgba32 GetAverageColorForPartition(ReadOnlySpan<int> partitionTable, int targetPartition,
			RawBlock4X4Rgba32 block)
		{
			var pixels = block.AsSpan;

			float r = 0;
			float g = 0;
			float b = 0;
			float a = 0;
			int count = 0;
			for (int i = 0; i < 16; i++)
			{
				if (partitionTable[i] == targetPartition)
				{
					var c = pixels[i];
					r += c.R;
					g += c.G;
					b += c.B;
					a += c.A;
					count++;
				}

			}

			return new ColorRgba32((byte)(r / count), (byte)(g / count), (byte)(b / count), (byte)(a / count));
		}

		private static ColorRgba32 GetAverageColorForBlock(RawBlock4X4Rgba32 block)
		{
			var pixels = block.AsSpan;

			float r = 0;
			float g = 0;
			float b = 0;
			float a = 0;
			int count = 0;
			for (int i = 0; i < 16; i++)
			{
				var c = pixels[i];
				r += c.R;
				g += c.G;
				b += c.B;
				a += c.A;
				count++;

			}

			return new ColorRgba32((byte)(r / count), (byte)(g / count), (byte)(b / count), (byte)(a / count));
		}


		private Bc7Block EncodeBlock(RawBlock4X4Rgba32 rawBlock, ClusterIndices4X4 indicesBlock, EncodingQuality quality)
		{
			switch (quality)
			{
				case EncodingQuality.Fast:
					return Bc7EncoderFast.EncodeBlock(rawBlock, indicesBlock);
				case EncodingQuality.Balanced:
					return Bc7EncoderFast.EncodeBlock(rawBlock, indicesBlock);
				case EncodingQuality.BestQuality:
					return Bc7EncoderFast.EncodeBlock(rawBlock, indicesBlock);
				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		private static class Bc7EncoderFast
		{


			public static Bc7Block EncodeBlock(RawBlock4X4Rgba32 rawBlock, ClusterIndices4X4 indicesBlock)
			{
				Bc7Block block = new Bc7Block();

				var reduced = indicesBlock.Reduce(out int numDistinctClusters);

				int best2SubsetPartition = Bc7EncodingHelpers.SelectBest2SubsetPartition(reduced, numDistinctClusters, out int subset2Error);
				int best3SubsetPartition = Bc7EncodingHelpers.SelectBest3SubsetPartition(reduced, numDistinctClusters, out int subset3Error);

				if (numDistinctClusters == 1)
				{ // Prefer single subset
					var avgColor = GetAverageColorForBlock(rawBlock);

					byte[][] colorEndpoints = new[] {
					//subset 1
						new byte[]{(byte)(avgColor.r >> 1),  (byte)(avgColor.g >> 1), (byte)(avgColor.b >> 1), (byte)(avgColor.a >> 1)},
						new byte[]{0, 0, 0, 0}
					};
					byte[] pBits = new byte[] {
						1, 1
					};
					byte[] colorIndices = new byte[] {
						0, 0, 0, 0,
						0, 0, 0, 0,
						0, 0, 0, 0,
						0, 0, 0, 0
					};
					block.PackType6(colorEndpoints, pBits, colorIndices);
				}
				else if (numDistinctClusters == 2)
				{ // Prefer two subsets
					var sub0Color = GetAverageColorForPartition(Bc7Block.Subsets2PartitionTable[best2SubsetPartition], 0,
						rawBlock);
					var sub1Color = GetAverageColorForPartition(Bc7Block.Subsets2PartitionTable[best2SubsetPartition], 1,
						rawBlock);

					byte[][] subsetEndpoints = new[] {
						//subset 1
						new byte[]{(byte)(sub0Color.r >> 2),  (byte)(sub0Color.g >> 2), (byte)(sub0Color.b >> 2)},
						new byte[]{(byte)(sub0Color.r >> 2),  (byte)(sub0Color.g >> 2), (byte)(sub0Color.b >> 2)},
						// subset 2
						new byte[]{(byte)(sub1Color.r >> 2),  (byte)(sub1Color.g >> 2), (byte)(sub1Color.b >> 2)},
						new byte[]{(byte)(sub1Color.r >> 2),  (byte)(sub1Color.g >> 2), (byte)(sub1Color.b >> 2)}
					};
					byte[] pBits = new byte[] {
						1, 1
					};
					byte[] indices = new byte[] {
						0, 0, 0, 0,
						0, 0, 0, 0,
						0, 0, 0, 0,
						0, 0, 0, 0
					};
					block.PackType1(best2SubsetPartition, subsetEndpoints, pBits, indices);
				}
				else
				{ // Prefer three subsets
					var sub0Color = GetAverageColorForPartition(Bc7Block.Subsets3PartitionTable[best3SubsetPartition], 0,
						rawBlock);
					var sub1Color = GetAverageColorForPartition(Bc7Block.Subsets3PartitionTable[best3SubsetPartition], 1,
						rawBlock);
					var sub2Color = GetAverageColorForPartition(Bc7Block.Subsets3PartitionTable[best3SubsetPartition], 2,
						rawBlock);
					byte[][] subsetEndpoints = new[] {
						//subset 1
						new byte[]{(byte)(sub0Color.r >> 3),  (byte)(sub0Color.g >> 3), (byte)(sub0Color.b >> 3)},
						new byte[]{(byte)(sub0Color.r >> 3),  (byte)(sub0Color.g >> 3), (byte)(sub0Color.b >> 3)},
						// subset 2
						new byte[]{(byte)(sub1Color.r >> 3),  (byte)(sub1Color.g >> 3), (byte)(sub1Color.b >> 3)},
						new byte[]{(byte)(sub1Color.r >> 3),  (byte)(sub1Color.g >> 3), (byte)(sub1Color.b >> 3)},
						//subset 3
						new byte[]{(byte)(sub2Color.r >> 3),  (byte)(sub2Color.g >> 3), (byte)(sub2Color.b >> 3)},
						new byte[]{(byte)(sub2Color.r >> 3),  (byte)(sub2Color.g >> 3), (byte)(sub2Color.b >> 3)},
					};
					byte[] indices = new byte[] {
						0, 0, 0, 0,
						0, 0, 0, 0,
						0, 0, 0, 0,
						0, 0, 0, 0
					};

					block.PackType2(best3SubsetPartition, subsetEndpoints, indices);
				}

				return block;
			}
		}
	}
}
