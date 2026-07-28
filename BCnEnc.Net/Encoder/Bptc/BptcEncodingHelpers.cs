using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BCnEncoder.Shared;

#if NETSTANDARD2_0
using MemoryMarshal = BCnEncoder.Shared.MemoryMarshalPolyfills;
#endif

namespace BCnEncoder.Encoder.Bptc
{
	internal static class BptcEncodingHelpers
	{
		private static readonly byte[] ColorInterpolationWeights0 = new byte[16];
		private static readonly byte[] ColorInterpolationWeights2 = new byte[] { 0, 21, 43, 64 };
		private static readonly byte[] ColorInterpolationWeights3 = new byte[] { 0, 9, 18, 27, 37, 46, 55, 64 };
		private static readonly byte[] ColorInterpolationWeights4 = new byte[] { 0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64 };

		/// <summary>
		/// Interpolation weights for an index precision. Precision 0 means "always endpoint 0",
		/// which is a zero weight for every index, so callers never need to special-case it.
		/// </summary>
		public static ReadOnlySpan<byte> GetInterpolationWeights(int indexPrecision)
		{
			switch (indexPrecision)
			{
				case 2: return ColorInterpolationWeights2;
				case 3: return ColorInterpolationWeights3;
				case 4: return ColorInterpolationWeights4;
				default: return ColorInterpolationWeights0;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int InterpolateInt(int e0, int e1, int weight)
		{
			return ((64 - weight) * e0 + weight * e1 + 32) >> 6;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte InterpolateByte(byte e0, byte e1, int weight)
		{
			return (byte)InterpolateInt(e0, e1, weight);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int InterpolateInt(int e0, int e1, int index, int indexPrecision)
		{
			return InterpolateInt(e0, e1, GetInterpolationWeights(indexPrecision)[index]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte InterpolateByte(byte e0, byte e1, int index, int indexPrecision)
		{
			return InterpolateByte(e0, e1, GetInterpolationWeights(indexPrecision)[index]);
		}



		public static int[] Rank2SubsetPartitions(ClusterIndices4X4 reducedIndicesBlock, int numDistinctClusters, bool smallIndex = false)
		{
			var partitionCount = smallIndex ? 32 : 64;
			var indices = reducedIndicesBlock.AsSpan;

			Span<int> errors = stackalloc int[partitionCount];
			for (var p = 0; p < partitionCount; p++)
			{
				errors[p] = CalculatePartitionError(indices, Bc7Block.Subsets2PartitionTable[p], 2, numDistinctClusters);
			}

			return RankByError(errors);
		}

		public static int[] Rank3SubsetPartitions(ClusterIndices4X4 reducedIndicesBlock, int numDistinctClusters)
		{
			const int partitionCount = 64;
			var indices = reducedIndicesBlock.AsSpan;

			Span<int> errors = stackalloc int[partitionCount];
			for (var p = 0; p < partitionCount; p++)
			{
				errors[p] = CalculatePartitionError(indices, Bc7Block.Subsets3PartitionTable[p], 3, numDistinctClusters);
			}

			return RankByError(errors);
		}

		/// <summary>
		/// Counts, per subset, how many pixels do not belong to that subset's most common cluster.
		/// </summary>
		private static int CalculatePartitionError(ReadOnlySpan<int> indices, ReadOnlySpan<int> partitionTable,
			int numSubsets, int numDistinctClusters)
		{
			Span<int> histogram = stackalloc int[numSubsets * numDistinctClusters];
			Span<int> largestCluster = stackalloc int[numSubsets];
			histogram.Clear();
			largestCluster.Clear();

			for (var i = 0; i < 16; i++)
			{
				var subset = partitionTable[i];
				var cluster = indices[i];
				var bucket = subset * numDistinctClusters;

				histogram[bucket + cluster]++;
				if (histogram[bucket + cluster] > histogram[bucket + largestCluster[subset]])
				{
					largestCluster[subset] = cluster;
				}
			}

			var error = 0;
			for (var i = 0; i < 16; i++)
			{
				if (indices[i] != largestCluster[partitionTable[i]]) error++;
			}

			return error;
		}

		/// <summary>
		/// Stable ascending sort of partition indices by error. Errors are pixel counts, so a
		/// counting sort over the 0..16 range replaces a comparison sort entirely.
		/// </summary>
		private static int[] RankByError(ReadOnlySpan<int> errors)
		{
			Span<int> offsets = stackalloc int[18];
			offsets.Clear();

			for (var i = 0; i < errors.Length; i++)
			{
				offsets[errors[i] + 1]++;
			}
			for (var i = 1; i < offsets.Length; i++)
			{
				offsets[i] += offsets[i - 1];
			}

			var output = new int[errors.Length];
			for (var i = 0; i < errors.Length; i++)
			{
				output[offsets[errors[i]]++] = i;
			}
			return output;
		}
	}

	internal struct ClusterIndices4X4
	{
		public int i00, i10, i20, i30;
		public int i01, i11, i21, i31;
		public int i02, i12, i22, i32;
		public int i03, i13, i23, i33;

		public Span<int> AsSpan => MemoryMarshal.CreateSpan(ref i00, 16);

		public int this[int x, int y]
		{
			get => AsSpan[x + y * 4];
			set => AsSpan[x + y * 4] = value;
		}

		public int this[int index]
		{
			get => AsSpan[index];
			set => AsSpan[index] = value;
		}

		public int NumClusters
		{
			get
			{
				var t = AsSpan;
				Span<int> clusters = stackalloc int[16];
				var distinct = 0;
				for (var i = 0; i < 16; i++)
				{
					var cluster = t[i];
					var found = false;
					for (var j = 0; j < distinct; j++)
					{
						if (clusters[j] == cluster)
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						clusters[distinct] = cluster;
						++distinct;
					}
				}
				return distinct;
			}
		}

		/// <summary>
		/// Reduces block down to adjacent cluster indices. For example,
		/// block that contains clusters 5, 16 and 77 will become a block that contains clusters 0, 1 and 2
		/// </summary>
		public ClusterIndices4X4 Reduce(out int numClusters)
		{
			var result = new ClusterIndices4X4();
			numClusters = NumClusters;
			Span<int> mapKey = stackalloc int[numClusters];
			var indices = AsSpan;
			var outIndices = result.AsSpan;
			var next = 0;
			for (var i = 0; i < 16; i++)
			{
				var cluster = indices[i];
				var found = false;
				for (var j = 0; j < next; j++)
				{
					if (mapKey[j] == cluster)
					{
						found = true;
						outIndices[i] = j;
						break;
					}
				}
				if (!found)
				{
					outIndices[i] = next;
					mapKey[next] = cluster;
					++next;
				}
			}

			return result;
		}
	}


	internal struct IndexBlock4x4
	{
		public byte i00, i10, i20, i30;
		public byte i01, i11, i21, i31;
		public byte i02, i12, i22, i32;
		public byte i03, i13, i23, i33;

		public Span<byte> AsSpan => MemoryMarshal.CreateSpan(ref i00, 16);

		public byte this[int x, int y]
		{
			get => AsSpan[x + y * 4];
			set => AsSpan[x + y * 4] = value;
		}

		public byte this[int index]
		{
			get => AsSpan[index];
			set => AsSpan[index] = value;
		}
	}
}
