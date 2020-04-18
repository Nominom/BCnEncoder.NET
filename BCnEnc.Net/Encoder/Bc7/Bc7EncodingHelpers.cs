using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using BCnEnc.Net.Shared;

namespace BCnEnc.Net.Encoder
{
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
				int distinct = 0;
				for (int i = 0; i < 16; i++)
				{
					var cluster = t[i];
					bool found = false;
					for (int j = 0; j < distinct; j++)
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
			int next = 0;
			for (int i = 0; i < 16; i++)
			{
				var index = 0;
				var cluster = indices[i];
				bool found = false;
				for (int j = 0; j < next; j++)
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

	internal static class Bc7EncodingHelpers
	{
		public static bool TypeHasPBits(Bc7BlockType type) => type switch
		{
			Bc7BlockType.Type0 => true,
			Bc7BlockType.Type1 => true,
			Bc7BlockType.Type3 => true,
			Bc7BlockType.Type6 => true,
			Bc7BlockType.Type7 => true,
			_ => false
		};

		public static bool TypeHasSharedPBits(Bc7BlockType type) => type switch
		{
			Bc7BlockType.Type1 => true,
			_ => false
		};

		/// <summary>
		/// Includes PBit
		/// </summary>
		public static int GetColorComponentPrecisionWithPBit(Bc7BlockType type) => type switch
		{
			Bc7BlockType.Type0 => 5,
			Bc7BlockType.Type1 => 7,
			Bc7BlockType.Type2 => 5,
			Bc7BlockType.Type3 => 8,
			Bc7BlockType.Type4 => 5,
			Bc7BlockType.Type5 => 7,
			Bc7BlockType.Type6 => 8,
			Bc7BlockType.Type7 => 6,
			_ => 0
		};

		/// <summary>
		/// Includes PBit
		/// </summary>
		public static int GetAlphaComponentPrecisionWithPBit(Bc7BlockType type) => type switch
		{

			Bc7BlockType.Type4 => 6,
			Bc7BlockType.Type5 => 8,
			Bc7BlockType.Type6 => 8,
			Bc7BlockType.Type7 => 6,
			_ => 0
		};

		public static void ExpandEndpoints(Bc7BlockType type, ColorRgba32[] endpoints, byte[] pBits)
		{
			if (type == Bc7BlockType.Type0 || type == Bc7BlockType.Type1 || type == Bc7BlockType.Type3 || type == Bc7BlockType.Type6 || type == Bc7BlockType.Type7)
			{
				for (int i = 0; i < endpoints.Length; i++)
				{
					endpoints[i] <<= 1;
				}

				if (type == Bc7BlockType.Type1)
				{
					endpoints[0] |= pBits[0];
					endpoints[1] |= pBits[0];
					endpoints[2] |= pBits[1];
					endpoints[3] |= pBits[1];
				}
				else
				{
					for (int i = 0; i < endpoints.Length; i++)
					{
						endpoints[i] |= pBits[i];
					}
				}
			}

			var colorPrecision = GetColorComponentPrecisionWithPBit(type);
			var alphaPrecision = GetAlphaComponentPrecisionWithPBit(type);
			for (int i = 0; i < endpoints.Length; i++)
			{
				// ColorComponentPrecision & AlphaComponentPrecision includes pbit
				// left shift endpoint components so that their MSB lies in bit 7
				endpoints[i].r = (byte)(endpoints[i].r << (8 - colorPrecision));
				endpoints[i].g = (byte)(endpoints[i].g << (8 - colorPrecision));
				endpoints[i].b = (byte)(endpoints[i].b << (8 - colorPrecision));
				endpoints[i].a = (byte)(endpoints[i].a << (8 - alphaPrecision));

				// Replicate each component's MSB into the LSBs revealed by the left-shift operation above
				endpoints[i].r = (byte)(endpoints[i].r | (endpoints[i].r >> colorPrecision));
				endpoints[i].g = (byte)(endpoints[i].g | (endpoints[i].g >> colorPrecision));
				endpoints[i].b = (byte)(endpoints[i].b | (endpoints[i].b >> colorPrecision));
				endpoints[i].a = (byte)(endpoints[i].a | (endpoints[i].a >> alphaPrecision));
			}

			//If this mode does not explicitly define the alpha component
			//set alpha equal to 255
			if (type == Bc7BlockType.Type0 || type == Bc7BlockType.Type1 || type == Bc7BlockType.Type2 || type == Bc7BlockType.Type3)
			{
				for (int i = 0; i < endpoints.Length; i++)
				{
					endpoints[i].a = 255;
				}
			}
		}

		public static int SelectBest2SubsetPartition(ClusterIndices4X4 reducedIndicesBlock, int numDistinctClusters, out int bestError)
		{
			bool first = true;
			bestError = 999;
			int bestPartition = 0;


			int CalculatePartitionError(int partitionIndex)
			{
				int error = 0;
				ReadOnlySpan<int> partitionTable = Bc7Block.Subsets2PartitionTable[partitionIndex];
				Span<int> subset0 = stackalloc int[numDistinctClusters];
				Span<int> subset1 = stackalloc int[numDistinctClusters];
				int max0Idx = 0;
				int max1Idx = 0;

				//Calculate largest cluster index for each subset 
				for (int i = 0; i < 16; i++)
				{
					if (partitionTable[i] == 0)
					{
						int r = reducedIndicesBlock[i];
						subset0[r]++;
						int count = subset0[r];
						if (count > subset0[max0Idx])
						{
							max0Idx = r;
						}
					}
					else
					{
						int r = reducedIndicesBlock[i];
						subset1[r]++;
						int count = subset1[r];
						if (count > subset1[max1Idx])
						{
							max1Idx = r;
						}
					}
				}

				// Calculate error by counting as error everything that does not match the largest cluster
				for (int i = 0; i < 16; i++)
				{
					if (partitionTable[i] == 0)
					{
						if (reducedIndicesBlock[i] != max0Idx) error++;
					}
					else
					{
						if (reducedIndicesBlock[i] != max1Idx) error++;
					}
				}

				return error;
			}

			for (int i = 0; i < 64; i++) // Loop through all possible indices
			{
				int error = CalculatePartitionError(i);
				if (first)
				{
					bestError = error;
					bestPartition = i;
					first = false;
				}
				else if (error < bestError)
				{
					bestError = error;
					bestPartition = i;
				}
				// Break early if exact match
				if (bestError == 0)
				{
					break;
				}
			}

			return bestPartition;
		}

		public static int SelectBest3SubsetPartition(ClusterIndices4X4 reducedIndicesBlock, int numDistinctClusters, out int bestError)
		{
			bool first = true;
			bestError = 999;
			int bestPartition = 0;



			int CalculatePartitionError(int partitionIndex)
			{
				int error = 0;
				ReadOnlySpan<int> partitionTable = Bc7Block.Subsets3PartitionTable[partitionIndex];

				Span<int> subset0 = stackalloc int[numDistinctClusters];
				Span<int> subset1 = stackalloc int[numDistinctClusters];
				Span<int> subset2 = stackalloc int[numDistinctClusters];
				int max0Idx = 0;
				int max1Idx = 0;
				int max2Idx = 0;

				//Calculate largest cluster index for each subset 
				for (int i = 0; i < 16; i++)
				{
					if (partitionTable[i] == 0)
					{
						int r = reducedIndicesBlock[i];
						subset0[r]++;
						int count = subset0[r];
						if (count > subset0[max0Idx])
						{
							max0Idx = r;
						}
					}
					else if (partitionTable[i] == 1)
					{
						int r = reducedIndicesBlock[i];
						subset1[r]++;
						int count = subset1[r];
						if (count > subset1[max1Idx])
						{
							max1Idx = r;
						}
					}
					else
					{
						int r = reducedIndicesBlock[i];
						subset2[r]++;
						int count = subset2[r];
						if (count > subset2[max2Idx])
						{
							max2Idx = r;
						}
					}
				}

				// Calculate error by counting as error everything that does not match the largest cluster
				for (int i = 0; i < 16; i++)
				{
					if (partitionTable[i] == 0)
					{
						if (reducedIndicesBlock[i] != max0Idx) error++;
					}
					else if (partitionTable[i] == 1)
					{
						if (reducedIndicesBlock[i] != max1Idx) error++;
					}
					else
					{
						if (reducedIndicesBlock[i] != max2Idx) error++;
					}
				}

				return error;
			}

			for (int i = 0; i < 64; i++) // Loop through all possible indices
			{
				int error = CalculatePartitionError(i);
				if (first)
				{
					bestError = error;
					bestPartition = i;
					first = false;
				}
				else if (error < bestError)
				{
					bestError = error;
					bestPartition = i;
				}
				// Break early if exact match
				if (bestError == 0)
				{
					break;
				}
			}

			return bestPartition;
		}

	}
}
