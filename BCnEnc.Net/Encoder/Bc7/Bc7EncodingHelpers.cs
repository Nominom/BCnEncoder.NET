using System;
using System.Linq;
using System.Runtime.InteropServices;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Bc7
{
	internal struct ClusterIndices4X4
	{
		public int i00, i10, i20, i30;
		public int i01, i11, i21, i31;
		public int i02, i12, i22, i32;
		public int i03, i13, i23, i33;

		public int[] AsArray => new int[]
		{
			i00, i10, i20, i30,
			i01, i11, i21, i31,
			i02, i12, i22, i32,
			i03, i13, i23, i33
		};

		public int this[int x, int y]
		{
			get
			{
				switch (x)
				{
					case 0:
						switch (y)
						{
							case 0: return i00;
							case 1: return i01;
							case 2: return i02;
							case 3: return i03;
						}
						break;
					case 1:
						switch (y)
						{
							case 0: return i10;
							case 1: return i11;
							case 2: return i12;
							case 3: return i13;
						}
						break;
					case 2:
						switch (y)
						{
							case 0: return i20;
							case 1: return i21;
							case 2: return i22;
							case 3: return i23;
						}
						break;
					case 3:
						switch (y)
						{
							case 0: return i30;
							case 1: return i31;
							case 2: return i32;
							case 3: return i33;
						}
						break;
				}
				return default;
			}
			set
			{
				switch (x)
				{
					case 0:
						switch (y)
						{
							case 0: i00 = value; break;
							case 1: i01 = value; break;
							case 2: i02 = value; break;
							case 3: i03 = value; break;
						}
						break;
					case 1:
						switch (y)
						{
							case 0: i10 = value; break;
							case 1: i11 = value; break;
							case 2: i12 = value; break;
							case 3: i13 = value; break;
						}
						break;
					case 2:
						switch (y)
						{
							case 0: i20 = value; break;
							case 1: i21 = value; break;
							case 2: i22 = value; break;
							case 3: i23 = value; break;
						}
						break;
					case 3:
						switch (y)
						{
							case 0: i30 = value; break;
							case 1: i31 = value; break;
							case 2: i32 = value; break;
							case 3: i33 = value; break;
						}
						break;
				}
			}
		}

		public int this[int index]
		{
			get => this[index % 4, (int)Math.Floor(index / 4.0)];
			set => this[index % 4, (int)Math.Floor(index / 4.0)] = value;
		}
		
		public int NumClusters
		{
			get
			{
				var t = AsArray;
				int[] clusters = new int[16];
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
			int[] mapKey = new int[numClusters];
			var indices = AsArray;
			var outIndices = result.AsArray;
			int next = 0;
			for (int i = 0; i < 16; i++)
			{
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
		private static int[] varPatternRAlpha = new int[] { 1, -1, 1, 0, 0, -1, 0, 0, 0, 0 };
		private static int[] varPatternRNoAlpha = new int[] { 1, -1, 1, 0, 0, -1, 0, 0 };

		private static int[] varPatternGAlpha = new int[] { 1, -1, 0, 1, 0, 0, -1, 0, 0, 0 };
		private static int[] varPatternGNoAlpha = new int[] { 1, -1, 0, 1, 0, 0, -1, 0 };

		private static int[] varPatternBAlpha = new int[] { 1, -1, 0, 0, 1, 0, 0, -1, 0, 0 };
		private static int[] varPatternBNoAlpha = new int[] { 1, -1, 0, 0, 1, 0, 0, -1 };

		private static int[] varPatternAAlpha = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, -1 };
		private static int[] varPatternANoAlpha = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };

		public static bool TypeHasPBits(Bc7BlockType type)
		{
			switch (type)
			{
				case (Bc7BlockType.Type0): return true;
				case (Bc7BlockType.Type1): return true;
				case (Bc7BlockType.Type3): return true;
				case (Bc7BlockType.Type6): return true;
				case (Bc7BlockType.Type7): return true;
				default: return false;
			}
		}

		public static bool TypeHasSharedPBits(Bc7BlockType type)
		{
			return type == Bc7BlockType.Type1;
		}

		/// <summary>
		/// Includes PBit
		/// </summary>
		public static int GetColorComponentPrecisionWithPBit(Bc7BlockType type)
		{
			switch (type)
			{
				case (Bc7BlockType.Type0): return 5;
				case (Bc7BlockType.Type1): return 7;
				case (Bc7BlockType.Type2): return 5;
				case (Bc7BlockType.Type3): return 8;
				case (Bc7BlockType.Type4): return 5;
				case (Bc7BlockType.Type5): return 7;
				case (Bc7BlockType.Type6): return 8;
				case (Bc7BlockType.Type7): return 6;
				default: return 0;
			}
		}

		/// <summary>
		/// Includes PBit
		/// </summary>
		public static int GetAlphaComponentPrecisionWithPBit(Bc7BlockType type)
		{
			switch (type)
			{
				case
					Bc7BlockType.Type4:
					return 6;
				case
					Bc7BlockType.Type5:
					return 8;
				case
					Bc7BlockType.Type6:
					return 8;
				case
					Bc7BlockType.Type7:
					return 6;
				default: return 0;
			}
		}

		/// <summary>
		/// Does not include pBit
		/// </summary>
		public static int GetColorComponentPrecision(Bc7BlockType type)
		{
			switch (type)
			{
				case Bc7BlockType.Type0: return 4;
				case Bc7BlockType.Type1: return 6;
				case Bc7BlockType.Type2: return 5;
				case Bc7BlockType.Type3: return 7;
				case Bc7BlockType.Type4: return 5;
				case Bc7BlockType.Type5: return 7;
				case Bc7BlockType.Type6: return 7;
				case Bc7BlockType.Type7: return 5;
				default: return 0;
			}
		}

		/// <summary>
		/// Does not include pBit
		/// </summary>
		public static int GetAlphaComponentPrecision(Bc7BlockType type)
		{
			switch (type)
			{
				case Bc7BlockType.Type4: return 6;
				case Bc7BlockType.Type5: return 8;
				case Bc7BlockType.Type6: return 7;
				case Bc7BlockType.Type7: return 5;
				default: return 0;
			}
		}

		public static int GetColorIndexBitCount(Bc7BlockType type, int type4IdxMode = 0)
		{

			switch (type)
			{
				case Bc7BlockType.Type0: return 3;
				case Bc7BlockType.Type1: return 3;
				case Bc7BlockType.Type2: return 2;
				case Bc7BlockType.Type3: return 2;
				case Bc7BlockType.Type4: return type4IdxMode == 0 ? 2 : 3;
				case Bc7BlockType.Type5: return 2;
				case Bc7BlockType.Type6: return 4;
				case Bc7BlockType.Type7: return 2;
				default: return 0;
			}

		}

		public static int GetAlphaIndexBitCount(Bc7BlockType type, int type4IdxMode = 0)
		{
			switch (type)
			{
				case Bc7BlockType.Type4: return type4IdxMode == 0 ? 3 : 2;
				case Bc7BlockType.Type5: return 2;
				case Bc7BlockType.Type6: return 4;
				case Bc7BlockType.Type7: return 2;
				default: return 0;
			}
		}


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


		public static ColorRgba32 ExpandEndpoint(Bc7BlockType type, ColorRgba32 endpoint, byte pBit)
		{
			if (type == Bc7BlockType.Type0 || type == Bc7BlockType.Type1 || type == Bc7BlockType.Type3 || type == Bc7BlockType.Type6 || type == Bc7BlockType.Type7)
			{
				endpoint <<= 1;
				endpoint |= pBit;
			}

			var colorPrecision = GetColorComponentPrecisionWithPBit(type);
			var alphaPrecision = GetAlphaComponentPrecisionWithPBit(type);
			endpoint.r = (byte)(endpoint.r << (8 - colorPrecision));
			endpoint.g = (byte)(endpoint.g << (8 - colorPrecision));
			endpoint.b = (byte)(endpoint.b << (8 - colorPrecision));
			endpoint.a = (byte)(endpoint.a << (8 - alphaPrecision));

			// Replicate each component's MSB into the LSBs revealed by the left-shift operation above
			endpoint.r = (byte)(endpoint.r | (endpoint.r >> colorPrecision));
			endpoint.g = (byte)(endpoint.g | (endpoint.g >> colorPrecision));
			endpoint.b = (byte)(endpoint.b | (endpoint.b >> colorPrecision));
			endpoint.a = (byte)(endpoint.a | (endpoint.a >> alphaPrecision));

			//If this mode does not explicitly define the alpha component
			//set alpha equal to 255
			if (type == Bc7BlockType.Type0 || type == Bc7BlockType.Type1 || type == Bc7BlockType.Type2 || type == Bc7BlockType.Type3)
			{
				endpoint.a = 255;
			}

			return endpoint;
		}

		public static int SelectBest2SubsetPartition(ClusterIndices4X4 reducedIndicesBlock, int numDistinctClusters, out int bestError)
		{
			bool first = true;
			bestError = 999;
			int bestPartition = 0;


			int CalculatePartitionError(int partitionIndex)
			{
				int error = 0;
				int[] partitionTable = Bc7Block.Subsets2PartitionTable[partitionIndex];
				int[] subset0 = new int[numDistinctClusters];
				int[] subset1 = new int[numDistinctClusters];
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

		public static int[] Rank2SubsetPartitions(ClusterIndices4X4 reducedIndicesBlock, int numDistinctClusters)
		{
			int[] output = Enumerable.Range(0, 64).ToArray();


			int CalculatePartitionError(int partitionIndex)
			{
				int error = 0;
				int[] partitionTable = Bc7Block.Subsets2PartitionTable[partitionIndex];
				int[] subset0 = new int[numDistinctClusters];
				int[] subset1 = new int[numDistinctClusters];
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

			output = output.OrderBy(CalculatePartitionError).ToArray();

			return output;
		}

		public static int SelectBest3SubsetPartition(ClusterIndices4X4 reducedIndicesBlock, int numDistinctClusters, out int bestError)
		{
			bool first = true;
			bestError = 999;
			int bestPartition = 0;



			int CalculatePartitionError(int partitionIndex)
			{
				int error = 0;
				int[] partitionTable = Bc7Block.Subsets3PartitionTable[partitionIndex];

				int[] subset0 = new int[numDistinctClusters];
				int[] subset1 = new int[numDistinctClusters];
				int[] subset2 = new int[numDistinctClusters];
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

		public static int[] Rank3SubsetPartitions(ClusterIndices4X4 reducedIndicesBlock, int numDistinctClusters)
		{
			int[] output = Enumerable.Range(0, 64).ToArray();

			int CalculatePartitionError(int partitionIndex)
			{
				int error = 0;
				int[] partitionTable = Bc7Block.Subsets3PartitionTable[partitionIndex];

				int[] subset0 = new int[numDistinctClusters];
				int[] subset1 = new int[numDistinctClusters];
				int[] subset2 = new int[numDistinctClusters];
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

			output = output.OrderBy(CalculatePartitionError).ToArray();

			return output;
		}


		public static void GetInitialUnscaledEndpoints(RawBlock4X4Rgba32 block, out ColorRgba32 ep0,
			out ColorRgba32 ep1)
		{

			var originalPixels = block.AsArray;
			PcaVectors.CreateWithAlpha(originalPixels, out var mean, out var pa);
			PcaVectors.GetExtremePointsWithAlpha(block.AsArray, mean, pa, out var min, out var max);

			ep0 = new ColorRgba32((byte)(min.X * 255), (byte)(min.Y * 255), (byte)(min.Z * 255), (byte)(min.W * 255));
			ep1 = new ColorRgba32((byte)(max.X * 255), (byte)(max.Y * 255), (byte)(max.Z * 255), (byte)(max.W * 255));
		}

		public static void GetInitialUnscaledEndpointsForSubset(RawBlock4X4Rgba32 block, out ColorRgba32 ep0,
			out ColorRgba32 ep1, int[] partitionTable, int subsetIndex)
		{

			var originalPixels = block.AsArray;

			int count = 0;
			for (int i = 0; i < 16; i++)
			{
				if (partitionTable[i] == subsetIndex)
				{
					count++;
				}
			}

			Rgba32[] subsetColors = new Rgba32[count];
			int next = 0;
			for (int i = 0; i < 16; i++)
			{
				if (partitionTable[i] == subsetIndex)
				{
					subsetColors[next++] = originalPixels[i];
				}
			}

			PcaVectors.CreateWithAlpha(subsetColors, out var mean, out var pa);
			PcaVectors.GetExtremePointsWithAlpha(block.AsArray, mean, pa, out var min, out var max);

			ep0 = new ColorRgba32((byte)(min.X * 255), (byte)(min.Y * 255), (byte)(min.Z * 255), (byte)(min.W * 255));
			ep1 = new ColorRgba32((byte)(max.X * 255), (byte)(max.Y * 255), (byte)(max.Z * 255), (byte)(max.W * 255));
		}

		public static ColorRgba32 ScaleDownEndpoint(ColorRgba32 endpoint, Bc7BlockType type, bool ignoreAlpha, out byte pBit)
		{
			int colorPrecision = GetColorComponentPrecisionWithPBit(type);
			int alphaPrecision = GetAlphaComponentPrecisionWithPBit(type);

			var r = (byte)(endpoint.r >> (8 - colorPrecision));
			var g = (byte)(endpoint.g >> (8 - colorPrecision));
			var b = (byte)(endpoint.b >> (8 - colorPrecision));
			var a = (byte)(endpoint.a >> (8 - alphaPrecision));

			if (TypeHasPBits(type))
			{
				int pBitVotingMask = (1 << (8 - colorPrecision + 1)) - 1;
				float pBitVotes = 0;
				pBitVotes += endpoint.r & pBitVotingMask;
				pBitVotes += endpoint.g & pBitVotingMask;
				pBitVotes += endpoint.b & pBitVotingMask;
				pBitVotes /= 3;

				if (pBitVotes >= (pBitVotingMask / 2f))
				{
					pBit = 1;
				}
				else
				{
					pBit = 0;
				}

				r >>= 1;
				g >>= 1;
				b >>= 1;
				a >>= 1;
			}
			else
			{
				pBit = 0;
			}

			if (ignoreAlpha)
			{
				return new ColorRgba32(r, g, b, 0);
			}
			else
			{
				return new ColorRgba32(r, g, b, a);
			}
		}

		public static ColorRgba32 InterpolateColor(ColorRgba32 endPointStart, ColorRgba32 endPointEnd,
			int colorIndex, int alphaIndex, int colorBitCount, int alphaBitCount)
		{

			byte InterpolateByte(byte e0, byte e1, int index, int indexPrecision)
			{
				if (indexPrecision == 0) return e0;
				byte[] aWeights2 = Bc7Block.colorInterpolationWeights2;
				byte[] aWeights3 = Bc7Block.colorInterpolationWeights3;
				byte[] aWeights4 = Bc7Block.colorInterpolationWeights4;

				if (indexPrecision == 2)
					return (byte)(((64 - aWeights2[index]) * (e0) + aWeights2[index] * (e1) + 32) >> 6);
				else if (indexPrecision == 3)
					return (byte)(((64 - aWeights3[index]) * (e0) + aWeights3[index] * (e1) + 32) >> 6);
				else // indexprecision == 4
					return (byte)(((64 - aWeights4[index]) * (e0) + aWeights4[index] * (e1) + 32) >> 6);
			}

			ColorRgba32 result = new ColorRgba32(
				InterpolateByte(endPointStart.r, endPointEnd.r, colorIndex, colorBitCount),
				InterpolateByte(endPointStart.g, endPointEnd.g, colorIndex, colorBitCount),
				InterpolateByte(endPointStart.b, endPointEnd.b, colorIndex, colorBitCount),
				InterpolateByte(endPointStart.a, endPointEnd.a, alphaIndex, alphaBitCount)
			);

			return result;
		}

		public static void ClampEndpoint(ref ColorRgba32 endpoint, byte colorMax, byte alphaMax)
		{
			if (endpoint.r > colorMax) endpoint.r = colorMax;
			if (endpoint.g > colorMax) endpoint.g = colorMax;
			if (endpoint.b > colorMax) endpoint.b = colorMax;
			if (endpoint.a > alphaMax) endpoint.a = alphaMax;
		}

		private static int FindClosestColorIndex(ColorYCbCrAlpha color, ColorYCbCrAlpha[] colors, out float bestError)
		{
			bestError = color.CalcDistWeighted(colors[0], 4, 2);
			int bestIndex = 0;
			for (int i = 1; i < colors.Length; i++)
			{
				float error = color.CalcDistWeighted(colors[i], 4, 2);
				if (error < bestError)
				{
					bestIndex = i;
					bestError = error;
				}
			}
			return bestIndex;
		}

		private static int FindClosestColorIndex(ColorYCbCr color, ColorYCbCr[] colors, out float bestError)
		{
			bestError = color.CalcDistWeighted(colors[0], 4);
			int bestIndex = 0;
			for (int i = 1; i < colors.Length; i++)
			{
				float error = color.CalcDistWeighted(colors[i], 4);
				if (error < bestError)
				{
					bestIndex = i;
					bestError = error;
				}
				if (bestError == 0)
				{
					break;
				}
			}
			return bestIndex;
		}

		private static int FindClosestAlphaIndex(byte alpha, byte[] alphas, out float bestError)
		{
			bestError = (alpha - alphas[0]) * (alpha - alphas[0]);
			int bestIndex = 0;
			for (int i = 1; i < alphas.Length; i++)
			{
				float error = (alpha - alphas[i]) * (alpha - alphas[i]);
				if (error < bestError)
				{
					bestIndex = i;
					bestError = error;
				}

				if (bestError == 0)
				{
					break;
				}
			}
			return bestIndex;
		}


		private static float TrySubsetEndpoints(Bc7BlockType type, RawBlock4X4Rgba32 raw, ColorRgba32 ep0, ColorRgba32 ep1,
			int[] partitionTable, int subsetIndex, int type4IdxMode)
		{
			int colorIndexPrecision = GetColorIndexBitCount(type, type4IdxMode);
			int alphaIndexPrecision = GetAlphaIndexBitCount(type, type4IdxMode);

			if (type == Bc7BlockType.Type4 || type == Bc7BlockType.Type5)
			{ //separate indices for color and alpha
				ColorYCbCr[] colors = new ColorYCbCr[1 << colorIndexPrecision];
				byte[] alphas = new byte[1 << alphaIndexPrecision];

				for (int i = 0; i < colors.Length; i++)
				{
					colors[i] = new ColorYCbCr(InterpolateColor(ep0, ep1, i,
						0, colorIndexPrecision, 0));
				}

				for (int i = 0; i < alphas.Length; i++)
				{
					alphas[i] = InterpolateColor(ep0, ep1, 0,
						i, 0, alphaIndexPrecision).a;
				}

				var pixels = raw.AsArray;
				float error = 0;

				for (int i = 0; i < 16; i++)
				{
					var pixelColor = new ColorYCbCr(pixels[i]);

					FindClosestColorIndex(pixelColor, colors, out var ce);
					FindClosestAlphaIndex(pixels[i].A, alphas, out var ae);

					error += ce + ae;
				}

				return error / 16;
			}
			else
			{
				ColorYCbCrAlpha[] colors = new ColorYCbCrAlpha[1 << colorIndexPrecision];
				for (int i = 0; i < colors.Length; i++)
				{
					colors[i] = new ColorYCbCrAlpha(InterpolateColor(ep0, ep1, i,
						i, colorIndexPrecision, alphaIndexPrecision));
				}

				var pixels = raw.AsArray;
				float error = 0;
				float count = 0;

				for (int i = 0; i < 16; i++)
				{
					if (partitionTable[i] == subsetIndex)
					{
						var pixelColor = new ColorYCbCrAlpha(pixels[i]);

						FindClosestColorIndex(pixelColor, colors, out var e);
						error += e * e;
						count++;
					}
				}

				error /= count;
				return error;
			}

		}

		public static void FillSubsetIndices(Bc7BlockType type, RawBlock4X4Rgba32 raw, ColorRgba32 ep0, ColorRgba32 ep1, int[] partitionTable, int subsetIndex,
			byte[] indicesToFill)
		{
			int colorIndexPrecision = GetColorIndexBitCount(type);
			int alphaIndexPrecision = GetAlphaIndexBitCount(type);

			if (type == Bc7BlockType.Type4 || type == Bc7BlockType.Type5)
			{ //separate indices for color and alpha
				throw new ArgumentException();
			}
			else
			{
				ColorYCbCrAlpha[] colors = new ColorYCbCrAlpha[1 << colorIndexPrecision];
				for (int i = 0; i < colors.Length; i++)
				{
					colors[i] = new ColorYCbCrAlpha(InterpolateColor(ep0, ep1, i,
						i, colorIndexPrecision, alphaIndexPrecision));
				}

				var pixels = raw.AsArray;

				for (int i = 0; i < 16; i++)
				{
					if (partitionTable[i] == subsetIndex)
					{
						var pixelColor = new ColorYCbCrAlpha(pixels[i]);

						var index = FindClosestColorIndex(pixelColor, colors, out var e);
						indicesToFill[i] = (byte)index;
					}
				}
			}
		}

		/// <summary>
		/// Used for Modes 4 and 5
		/// </summary>
		public static void FillAlphaColorIndices(Bc7BlockType type, RawBlock4X4Rgba32 raw, ColorRgba32 ep0, ColorRgba32 ep1,
			byte[] colorIndicesToFill, byte[] alphaIndicesToFill, int idxMode = 0)
		{
			int colorIndexPrecision = GetColorIndexBitCount(type, idxMode);
			int alphaIndexPrecision = GetAlphaIndexBitCount(type, idxMode);

			if (type == Bc7BlockType.Type4 || type == Bc7BlockType.Type5)
			{
				ColorYCbCr[] colors = new ColorYCbCr[1 << colorIndexPrecision];
				byte[] alphas = new byte[1 << alphaIndexPrecision];

				for (int i = 0; i < colors.Length; i++)
				{
					colors[i] = new ColorYCbCr(InterpolateColor(ep0, ep1, i,
						0, colorIndexPrecision, 0));
				}

				for (int i = 0; i < alphas.Length; i++)
				{
					alphas[i] = InterpolateColor(ep0, ep1, 0,
						i, 0, alphaIndexPrecision).a;
				}

				var pixels = raw.AsArray;

				for (int i = 0; i < 16; i++)
				{
					var pixelColor = new ColorYCbCr(pixels[i]);

					var index = FindClosestColorIndex(pixelColor, colors, out var e);
					colorIndicesToFill[i] = (byte)index;

					index = FindClosestAlphaIndex(pixels[i].A, alphas, out var _);
					alphaIndicesToFill[i] = (byte)index;
				}
			}
			else
			{
				throw new ArgumentException();
			}
		}

		public static void OptimizeSubsetEndpointsWithPBit(Bc7BlockType type, RawBlock4X4Rgba32 raw, ref ColorRgba32 ep0, ref ColorRgba32 ep1, ref byte pBit0, ref byte pBit1,
			int variation, int[] partitionTable, int subsetIndex, bool variatePBits, bool variateAlpha, int type4IdxMode = 0)
		{

			byte colorMax = (byte)((1 << GetColorComponentPrecision(type)) - 1);
			byte alphaMax = (byte)((1 << GetAlphaComponentPrecision(type)) - 1);

			float bestError = TrySubsetEndpoints(type, raw,
				ExpandEndpoint(type, ep0, pBit0),
				ExpandEndpoint(type, ep1, pBit1), partitionTable, subsetIndex, type4IdxMode
			);

			int[] varPatternR = variateAlpha
				? varPatternRAlpha
				: varPatternRNoAlpha;
			int[] varPatternG = variateAlpha
				? varPatternGAlpha
				: varPatternGNoAlpha;
			int[] varPatternB = variateAlpha
				? varPatternBAlpha
				: varPatternBNoAlpha;
			int[] varPatternA = variateAlpha
				? varPatternAAlpha
				: varPatternANoAlpha;


			while (variation > 0)
			{
				bool foundBetter = false;

				for (int i = 0; i < varPatternR.Length; i++)
				{
					ColorRgba32 testEndPoint0 = new ColorRgba32(
						(byte)(ep0.r - variation * varPatternR[i]),
						(byte)(ep0.g - variation * varPatternG[i]),
						(byte)(ep0.b - variation * varPatternB[i]),
						(byte)(ep0.a - variation * varPatternA[i])
					);

					ColorRgba32 testEndPoint1 = new ColorRgba32(
						(byte)(ep1.r + variation * varPatternR[i]),
						(byte)(ep1.g + variation * varPatternG[i]),
						(byte)(ep1.b + variation * varPatternB[i]),
						(byte)(ep1.a + variation * varPatternA[i])
					);
					ClampEndpoint(ref testEndPoint0, colorMax, alphaMax);
					ClampEndpoint(ref testEndPoint1, colorMax, alphaMax);

					float error = TrySubsetEndpoints(type, raw,
						ExpandEndpoint(type, testEndPoint0, pBit0),
						ExpandEndpoint(type, testEndPoint1, pBit1), partitionTable, subsetIndex, type4IdxMode
					);
					if (error < bestError)
					{
						bestError = error;
						ep0 = testEndPoint0;
						ep1 = testEndPoint1;
						foundBetter = true;
					}
				}

				for (int i = 0; i < varPatternR.Length; i++)
				{
					ColorRgba32 testEndPoint0 = new ColorRgba32(
						(byte)(ep0.r + variation * varPatternR[i]),
						(byte)(ep0.g + variation * varPatternG[i]),
						(byte)(ep0.b + variation * varPatternB[i]),
						(byte)(ep0.a + variation * varPatternA[i])
						);
					ClampEndpoint(ref testEndPoint0, colorMax, alphaMax);

					float error = TrySubsetEndpoints(type, raw,
						ExpandEndpoint(type, testEndPoint0, pBit0),
						ExpandEndpoint(type, ep1, pBit1), partitionTable, subsetIndex, type4IdxMode
					);
					if (error < bestError)
					{
						bestError = error;
						ep0 = testEndPoint0;
						foundBetter = true;
					}
				}

				for (int i = 0; i < varPatternR.Length; i++)
				{
					ColorRgba32 testEndPoint1 = new ColorRgba32(
						(byte)(ep1.r + variation * varPatternR[i]),
						(byte)(ep1.g + variation * varPatternG[i]),
						(byte)(ep1.b + variation * varPatternB[i]),
						(byte)(ep1.a + variation * varPatternA[i])
					);
					ClampEndpoint(ref testEndPoint1, colorMax, alphaMax);

					float error = TrySubsetEndpoints(type, raw,
						ExpandEndpoint(type, ep0, pBit0),
						ExpandEndpoint(type, testEndPoint1, pBit1), partitionTable, subsetIndex, type4IdxMode
					);
					if (error < bestError)
					{
						bestError = error;
						ep1 = testEndPoint1;
						foundBetter = true;
					}
				}

				if (variatePBits)
				{
					{
						byte testPBit0 = pBit0 == 0 ? (byte)1 : (byte)0;
						float error = TrySubsetEndpoints(type, raw,
							ExpandEndpoint(type, ep0, testPBit0),
							ExpandEndpoint(type, ep1, pBit1), partitionTable, subsetIndex, type4IdxMode
						);
						if (error < bestError)
						{
							bestError = error;
							pBit0 = testPBit0;
							foundBetter = true;
						}
					}
					{
						byte testPBit1 = pBit1 == 0 ? (byte)1 : (byte)0;
						float error = TrySubsetEndpoints(type, raw,
							ExpandEndpoint(type, ep0, pBit0),
							ExpandEndpoint(type, ep1, testPBit1), partitionTable, subsetIndex, type4IdxMode
						);
						if (error < bestError)
						{
							bestError = error;
							pBit1 = testPBit1;
							foundBetter = true;
						}
					}
				}
				if (!foundBetter)
				{
					variation--;
				}
			}
		}

		public static RawBlock4X4Rgba32 RotateBlockColors(RawBlock4X4Rgba32 block, int rotation)
		{
			if (rotation == 0)
			{
				return block;
			}

			RawBlock4X4Rgba32 rotated = new RawBlock4X4Rgba32();
			var pixels = block.AsArray;
			var output = rotated.AsArray;
			for (int i = 0; i < 16; i++)
			{
				var c = pixels[i];
				switch (rotation)
				{
					case 1:
						output[i] = new Rgba32(c.A, c.G, c.B, c.R);
						break;
					case 2:
						output[i] = new Rgba32(c.R, c.A, c.B, c.G);
						break;
					case 3:
						output[i] = new Rgba32(c.R, c.G, c.A, c.B);
						break;
				}
			}

			return rotated;
		}

	}
}
