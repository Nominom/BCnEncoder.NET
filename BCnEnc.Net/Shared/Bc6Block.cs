using System;

namespace BCnEncoder.Shared
{

	internal enum Bc6BlockType : uint
	{
		Type0 = 0, // Mode 1
		Type1 = 1, // Mode 2
		Type2 = 2, // Mode 3
		Type6 = 6, // Mode 4
		Type10 = 10, // Mode 5
		Type14 = 14, // Mode 6
		Type18 = 18, // Mode 7
		Type22 = 22, // Mode 8
		Type26 = 26, // Mode 9
		Type30 = 30, // Mode 10
		Type3 = 3, // Mode 11
		Type7 = 7, // Mode 12
		Type11 = 11, // Mode 13
		Type15 = 15, // Mode 14
		Unknown
	}

	internal struct Bc6Block
	{
		public ulong lowBits;
		public ulong highBits;

		public static readonly byte[] ColorInterpolationWeights3 = new byte[] { 0, 9, 18, 27, 37, 46, 55, 64 };
		public static readonly byte[] ColorInterpolationWeights4 = new byte[] { 0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64 };


		public static readonly int[][] Subsets2PartitionTable = new int[32][]{
			new[] {0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
			new[] {0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1},
			new[] {0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1},
			new[] {0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 1},
			new[] {0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1},
			new[] {0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1},
			new[] {0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1},
			new[] {0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1},
			new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1},
			new[] {0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			new[] {0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1},
			new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1},
			new[] {0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			new[] {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1},
			new[] {0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1},
			new[] {0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1},
			new[] {0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
			new[] {0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0},
			new[] {0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0},
			new[] {0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
			new[] {0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0},
			new[] {0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0},
			new[] {0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1},
			new[] {0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0},
			new[] {0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0},
			new[] {0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0},
			new[] {0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0},
			new[] {0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0},
			new[] {0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0},
			new[] {0, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0},
			new[] {0, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0}
		};

		public static readonly int[] Subsets2AnchorIndices = {
			15, 15, 15, 15, 15, 15, 15, 15,
			15, 15, 15, 15, 15, 15, 15, 15,
			15, 2, 8, 2, 2, 8, 8, 15,
			2, 8, 2, 2, 8, 8, 2, 2,
			15, 15, 6, 8, 2, 8, 15, 15,
			2, 8, 2, 2, 2, 15, 15, 6,
			6, 2, 6, 8, 15, 15, 2, 2,
			15, 15, 15, 15, 15, 2, 2, 15
		};

		public Bc6BlockType Type
		{
			get
			{
				const ulong smallMask = 0b11;
				const ulong bigMask = 0b11111;
				// Type 0 or 1
				if ((lowBits & smallMask) < 2)
				{
					return (Bc6BlockType)(lowBits & smallMask);
				}
				else
				{
					var typeNum = (lowBits & bigMask);
					switch (typeNum)
					{
						case 2: return Bc6BlockType.Type2;
						case 3: return Bc6BlockType.Type3;
						case 6: return Bc6BlockType.Type6;
						case 7: return Bc6BlockType.Type7;
						case 10: return Bc6BlockType.Type10;
						case 11: return Bc6BlockType.Type11;
						case 14: return Bc6BlockType.Type14;
						case 15: return Bc6BlockType.Type15;
						case 18: return Bc6BlockType.Type18;
						case 22: return Bc6BlockType.Type22;
						case 26: return Bc6BlockType.Type26;
						case 30: return Bc6BlockType.Type30;
						default: return Bc6BlockType.Unknown;
					}
				}
			}
		}

		public bool HasSubsets => Type switch
		{
			Bc6BlockType.Type3 => false,
			Bc6BlockType.Type7 => false,
			Bc6BlockType.Type11 => false,
			Bc6BlockType.Type15 => false,
			_ => true
		};

		public int NumEndpoints => Type switch
		{
			Bc6BlockType.Type3 => 2,
			Bc6BlockType.Type7 => 2,
			Bc6BlockType.Type11 => 2,
			Bc6BlockType.Type15 => 2,
			_ => 4
		};

		public bool TransformedEndpoints => Type switch
		{
			Bc6BlockType.Type3 => false,
			Bc6BlockType.Type30 => false,
			_ => true
		};

		public int PartitionSetId => HasSubsets ? ByteHelper.Extract5(highBits, 13) : -1;

		public int EndpointBits => Type switch
		{
			Bc6BlockType.Type0 => 10,
			Bc6BlockType.Type1 => 7,
			Bc6BlockType.Type2 => 11,
			Bc6BlockType.Type6 => 11,
			Bc6BlockType.Type10 => 11,
			Bc6BlockType.Type14 => 9,
			Bc6BlockType.Type18 => 8,
			Bc6BlockType.Type22 => 8,
			Bc6BlockType.Type26 => 8,
			Bc6BlockType.Type30 => 6,
			Bc6BlockType.Type3 => 10,
			Bc6BlockType.Type7 => 11,
			Bc6BlockType.Type11 => 12,
			Bc6BlockType.Type15 => 16,
			_ => 0
		};

		public (int, int, int) DeltaBits => Type switch
		{
			Bc6BlockType.Type0 => (5, 5, 5),
			Bc6BlockType.Type1 => (6, 6, 6),
			Bc6BlockType.Type2 => (5, 4, 4),
			Bc6BlockType.Type6 => (4, 5, 4),
			Bc6BlockType.Type10 => (4, 4, 5),
			Bc6BlockType.Type14 => (5, 5, 5),
			Bc6BlockType.Type18 => (6, 5, 5),
			Bc6BlockType.Type22 => (5, 6, 5),
			Bc6BlockType.Type26 => (5, 5, 6),
			Bc6BlockType.Type30 => (0, 0, 0),
			Bc6BlockType.Type3 => (0, 0, 0),
			Bc6BlockType.Type7 => (9, 9, 9),
			Bc6BlockType.Type11 => (8, 8, 8),
			Bc6BlockType.Type15 => (4, 4, 4),
			_ => (0, 0, 0)
		};

		public int ColorIndexBitCount => Type switch
		{
			Bc6BlockType.Type3 => 4,
			Bc6BlockType.Type7 => 4,
			Bc6BlockType.Type11 => 4,
			Bc6BlockType.Type15 => 4,
			_ => 3
		};

		private (int, int, int) ExtractEp0()
		{
			ulong r0 = 0;
			ulong g0 = 0;
			ulong b0 = 0;

			r0 = ByteHelper.ExtractFrom128(lowBits, highBits, 5, Math.Min(10, EndpointBits));
			g0 = ByteHelper.ExtractFrom128(lowBits, highBits, 15, Math.Min(10, EndpointBits));
			b0 = ByteHelper.ExtractFrom128(lowBits, highBits, 25, Math.Min(10, EndpointBits));

			switch (Type)
			{
				case Bc6BlockType.Type2:

					r0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 1) << 10;
					g0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 49, 1) << 10;
					b0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 59, 1) << 10;
					break;
				case Bc6BlockType.Type6:

					r0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 39, 1) << 10;
					g0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 50, 1) << 10;
					b0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 59, 1) << 10;
					break;
				case Bc6BlockType.Type10:

					r0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 39, 1) << 10;
					g0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 49, 1) << 10;
					b0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 60, 1) << 10;
					break;
				case Bc6BlockType.Type7:
					r0 = ByteHelper.ExtractFrom128(lowBits, highBits, 5, 10);
					g0 = ByteHelper.ExtractFrom128(lowBits, highBits, 15, 10);
					b0 = ByteHelper.ExtractFrom128(lowBits, highBits, 25, 10);

					r0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 44, 1) << 10;
					g0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 54, 1) << 10;
					b0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 64, 1) << 10;
					break;
				case Bc6BlockType.Type11:
					r0 = ByteHelper.ExtractFrom128(lowBits, highBits, 5, 10);
					g0 = ByteHelper.ExtractFrom128(lowBits, highBits, 15, 10);
					b0 = ByteHelper.ExtractFrom128(lowBits, highBits, 25, 10);

					r0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 44, 1) << 10;
					g0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 54, 1) << 10;
					b0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 64, 1) << 10;

					r0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 43, 1) << 11;
					g0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 53, 1) << 11;
					b0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 63, 1) << 11;
					break;
				case Bc6BlockType.Type15:
					r0 = ByteHelper.ExtractFrom128(lowBits, highBits, 5, 10);
					g0 = ByteHelper.ExtractFrom128(lowBits, highBits, 15, 10);
					b0 = ByteHelper.ExtractFrom128(lowBits, highBits, 25, 10);

					r0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 44, 1) << 10;
					g0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 54, 1) << 10;
					b0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 64, 1) << 10;

					r0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 43, 1) << 11;
					g0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 53, 1) << 11;
					b0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 63, 1) << 11;

					r0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 42, 1) << 12;
					g0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 52, 1) << 12;
					b0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 62, 1) << 12;

					r0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 41, 1) << 13;
					g0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 51, 1) << 13;
					b0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 61, 1) << 13;

					r0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 1) << 14;
					g0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 50, 1) << 14;
					b0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 60, 1) << 14;

					r0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 39, 1) << 15;
					g0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 49, 1) << 15;
					b0 |= ByteHelper.ExtractFrom128(lowBits, highBits, 59, 1) << 15;
					break;
			}

			return ((int)r0, (int)g0, (int)b0);
		}

		private (int, int, int) ExtractEp1()
		{
			ulong r1 = 0;
			ulong g1 = 0;
			ulong b1 = 0;

			if (TransformedEndpoints)
			{
				r1 = ByteHelper.ExtractFrom128(lowBits, highBits, 35, Math.Min(5, DeltaBits.Item1));
				g1 = ByteHelper.ExtractFrom128(lowBits, highBits, 45, Math.Min(5, DeltaBits.Item2));
				b1 = ByteHelper.ExtractFrom128(lowBits, highBits, 55, Math.Min(5, DeltaBits.Item3));
			}

			switch (Type)
			{
				case Bc6BlockType.Type1:
					r1 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 1) << 5;
					g1 |= ByteHelper.ExtractFrom128(lowBits, highBits, 50, 1) << 5;
					b1 |= ByteHelper.ExtractFrom128(lowBits, highBits, 60, 1) << 5;

					break;
				case Bc6BlockType.Type18:
					r1 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 1) << 5;

					break;
				case Bc6BlockType.Type22:
					g1 |= ByteHelper.ExtractFrom128(lowBits, highBits, 50, 1) << 5;

					break;
				case Bc6BlockType.Type26:
					b1 |= ByteHelper.ExtractFrom128(lowBits, highBits, 60, 1) << 5;

					break;
				case Bc6BlockType.Type30:
					r1 = ByteHelper.ExtractFrom128(lowBits, highBits, 35, 6);
					g1 = ByteHelper.ExtractFrom128(lowBits, highBits, 45, 6);
					b1 = ByteHelper.ExtractFrom128(lowBits, highBits, 55, 6);

					break;
				case Bc6BlockType.Type3:
					r1 = ByteHelper.ExtractFrom128(lowBits, highBits, 35, 10);
					g1 = ByteHelper.ExtractFrom128(lowBits, highBits, 45, 10);
					b1 = ByteHelper.ExtractFrom128(lowBits, highBits, 55, 10);

					break;
				case Bc6BlockType.Type7:
					r1 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 4) << 5;
					g1 |= ByteHelper.ExtractFrom128(lowBits, highBits, 50, 4) << 5;
					b1 |= ByteHelper.ExtractFrom128(lowBits, highBits, 60, 4) << 5;

					break;
				case Bc6BlockType.Type11:
					r1 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 3) << 5;
					g1 |= ByteHelper.ExtractFrom128(lowBits, highBits, 50, 3) << 5;
					b1 |= ByteHelper.ExtractFrom128(lowBits, highBits, 60, 3) << 5;

					break;
			}

			return ((int)r1, (int)g1, (int)b1);
		}

		private (int, int, int) ExtractEp2()
		{
			ulong r2 = 0;
			ulong g2 = 0;
			ulong b2 = 0;

			r2 = ByteHelper.ExtractFrom128(lowBits, highBits, 65, Math.Min(5, DeltaBits.Item1));
			g2 = ByteHelper.ExtractFrom128(lowBits, highBits, 41, 4);
			b2 = ByteHelper.ExtractFrom128(lowBits, highBits, 61, 4);

			switch (Type)
			{
				case Bc6BlockType.Type0:
					//r2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 1) << 5;
					g2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 2, 1) << 4;
					b2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 3, 1) << 4;
					break;
				case Bc6BlockType.Type1:
					r2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 70, 1) << 5;

					g2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 24, 1) << 4;
					g2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 2, 1) << 5;

					b2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 14, 1) << 4;
					b2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 22, 1) << 5;

					break;
				case Bc6BlockType.Type6:

					g2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 75, 1) << 4;

					break;
				case Bc6BlockType.Type10:

					b2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 1) << 4;

					break;
				case Bc6BlockType.Type14:
					g2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 24, 1) << 4;
					b2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 14, 1) << 4;

					break;
				case Bc6BlockType.Type18:
					r2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 70, 1) << 5;

					g2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 24, 1) << 4;
					b2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 14, 1) << 4;

					break;
				case Bc6BlockType.Type22:

					g2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 24, 1) << 4;
					g2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 23, 1) << 5;

					b2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 14, 1) << 4;
					break;
				case Bc6BlockType.Type26:
					g2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 24, 1) << 4;

					b2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 14, 1) << 4;
					b2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 23, 1) << 5;
					break;
				case Bc6BlockType.Type30:

					r2 = ByteHelper.ExtractFrom128(lowBits, highBits, 65, 6);

					g2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 24, 1) << 4;
					g2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 21, 1) << 5;

					b2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 14, 1) << 4;
					b2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 22, 1) << 5;

					break;
			}

			return ((int)r2, (int)g2, (int)b2);
		}

		private (int, int, int) ExtractEp3()
		{
			ulong r3 = 0;
			ulong g3 = 0;
			ulong b3 = 0;

			r3 = ByteHelper.ExtractFrom128(lowBits, highBits, 71, Math.Min(5, DeltaBits.Item1));
			g3 = ByteHelper.ExtractFrom128(lowBits, highBits, 51, 4);

			switch (Type)
			{
				case Bc6BlockType.Type0:
					//r2 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 1) << 5;
					g3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 1) << 4;

					b3 = ByteHelper.ExtractFrom128(lowBits, highBits, 50, 1);
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 60, 1) << 1;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 70, 1) << 2;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 76, 1) << 3;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 4, 1) << 4;
					break;
				case Bc6BlockType.Type1:
					r3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 76, 1) << 5;

					g3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 3, 2) << 4;

					b3 = ByteHelper.ExtractFrom128(lowBits, highBits, 12, 2);
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 23, 1) << 2;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 32, 1) << 3;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 34, 1) << 4;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 33, 1) << 5;

					break;
				case Bc6BlockType.Type2:

					b3 = ByteHelper.ExtractFrom128(lowBits, highBits, 50, 1);
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 60, 1) << 1;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 70, 1) << 2;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 76, 1) << 3;

					break;
				case Bc6BlockType.Type6:

					g3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 1) << 4;

					b3 = ByteHelper.ExtractFrom128(lowBits, highBits, 69, 1);
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 60, 1) << 1;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 70, 1) << 2;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 76, 1) << 3;

					break;
				case Bc6BlockType.Type10:

					b3 = ByteHelper.ExtractFrom128(lowBits, highBits, 50, 1);
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 69, 1) << 1;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 70, 1) << 2;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 76, 1) << 3;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 75, 1) << 4;

					break;
				case Bc6BlockType.Type14:
					g3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 1) << 4;


					b3 = ByteHelper.ExtractFrom128(lowBits, highBits, 50, 1);
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 60, 1) << 1;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 70, 1) << 2;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 76, 1) << 3;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 34, 1) << 4;

					break;
				case Bc6BlockType.Type18:
					r3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 76, 1) << 5;

					g3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 13, 1) << 4;

					b3 = ByteHelper.ExtractFrom128(lowBits, highBits, 50, 1);
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 60, 1) << 1;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 23, 1) << 2;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 33, 1) << 3;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 34, 1) << 4;

					break;
				case Bc6BlockType.Type22:

					g3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 1) << 4;
					g3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 33, 1) << 5;

					b3 = ByteHelper.ExtractFrom128(lowBits, highBits, 13, 1);
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 60, 1) << 1;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 70, 1) << 2;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 76, 1) << 3;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 34, 1) << 4;

					break;
				case Bc6BlockType.Type26:
					g3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 40, 1) << 4;

					b3 = ByteHelper.ExtractFrom128(lowBits, highBits, 50, 1);
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 13, 1) << 1;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 70, 1) << 2;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 76, 1) << 3;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 34, 1) << 4;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 33, 1) << 5;

					break;
				case Bc6BlockType.Type30:

					r3 = ByteHelper.ExtractFrom128(lowBits, highBits, 71, 6);


					g3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 11, 1) << 4;
					g3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 31, 1) << 5;

					b3 = ByteHelper.ExtractFrom128(lowBits, highBits, 12, 2);
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 23, 1) << 2;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 32, 1) << 3;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 34, 1) << 4;
					b3 |= ByteHelper.ExtractFrom128(lowBits, highBits, 33, 1) << 5;

					break;
			}

			return ((int)r3, (int)g3, (int)b3);
		}

		private (int, int, int)[] ExtractRawEndpoints(bool signedBc6)
		{
			var outEndpoints = new (int, int, int)[HasSubsets ? 4 : 2];
			var endpointBits = EndpointBits;

			var (r0, g0, b0) = ExtractEp0();

			// If bc6h is in signed mode, sign extend the base endpoints
			if (signedBc6)
			{
				r0 = IntHelper.SignExtend(r0, endpointBits);
				g0 = IntHelper.SignExtend(g0, endpointBits);
				b0 = IntHelper.SignExtend(b0, endpointBits);
			}

			outEndpoints[0] = (r0, g0, b0);

			var (r1, g1, b1) = ExtractEp1();

			if (TransformedEndpoints)
			{
				r1 = IntHelper.SignExtend(r1, DeltaBits.Item1);
				g1 = IntHelper.SignExtend(g1, DeltaBits.Item2);
				b1 = IntHelper.SignExtend(b1, DeltaBits.Item3);

				r1 = (r1 + r0) & ((1 << endpointBits) - 1);
				g1 = (g1 + g0) & ((1 << endpointBits) - 1);
				b1 = (b1 + b0) & ((1 << endpointBits) - 1);
			}
			if (signedBc6)
			{
				r1 = IntHelper.SignExtend(r1, endpointBits);
				g1 = IntHelper.SignExtend(g1, endpointBits);
				b1 = IntHelper.SignExtend(b1, endpointBits);
			}

			outEndpoints[1] = (r1, g1, b1);

			if (HasSubsets)
			{
				var (r2, g2, b2) = ExtractEp2();
				var (r3, g3, b3) = ExtractEp3();

				if (TransformedEndpoints)
				{
					r2 = IntHelper.SignExtend(r2, DeltaBits.Item1);
					g2 = IntHelper.SignExtend(g2, DeltaBits.Item2);
					b2 = IntHelper.SignExtend(b2, DeltaBits.Item3);

					r2 = (r2 + r0) & ((1 << endpointBits) - 1);
					g2 = (g2 + g0) & ((1 << endpointBits) - 1);
					b2 = (b2 + b0) & ((1 << endpointBits) - 1);

					r3 = IntHelper.SignExtend(r3, DeltaBits.Item1);
					g3 = IntHelper.SignExtend(g3, DeltaBits.Item2);
					b3 = IntHelper.SignExtend(b3, DeltaBits.Item3);

					r3 = (r3 + r0) & ((1 << endpointBits) - 1);
					g3 = (g3 + g0) & ((1 << endpointBits) - 1);
					b3 = (b3 + b0) & ((1 << endpointBits) - 1);
				}

				if (signedBc6)
				{
					r2 = IntHelper.SignExtend(r2, endpointBits);
					g2 = IntHelper.SignExtend(g2, endpointBits);
					b2 = IntHelper.SignExtend(b2, endpointBits);

					r3 = IntHelper.SignExtend(r3, endpointBits);
					g3 = IntHelper.SignExtend(g3, endpointBits);
					b3 = IntHelper.SignExtend(b3, endpointBits);
				}

				outEndpoints[2] = (r2, g2, b2);
				outEndpoints[3] = (r3, g3, b3);
			}

			return outEndpoints;
		}

		private int UnQuantize(int component, bool signedBc6)
		{
			int unq;
			var sign = false;
			var endpointBits = EndpointBits;

			if (!signedBc6)
			{
				if (endpointBits >= 15)
					unq = component;
				else if (component == 0)
					unq = 0;
				else if (component == ((1 << endpointBits) - 1))
					unq = 0xFFFF;
				else
					//unq = ((component << 16) + 0x8000) >> endpointBits;
					unq = ((component << 15) + 0x4000) >> (endpointBits - 1);

			}
			else
			{
				if (endpointBits >= 16)
					unq = component;
				else
				{
					if (component < 0)
					{
						sign = true;
						component = -component;
					}

					if (component == 0)
						unq = 0;
					else if (component >= ((1 << (endpointBits - 1)) - 1))
						unq = 0x7FFF;
					else
						unq = ((component << 15) + 0x4000) >> (endpointBits - 1);

					if (sign)
						unq = -unq;
				}
			}
			return unq;
		}

		private (int, int, int) UnQuantize((int, int, int) components, bool signedBc6)
		{
			return (
				UnQuantize(components.Item1, signedBc6),
				UnQuantize(components.Item2, signedBc6),
				UnQuantize(components.Item3, signedBc6)
			);
		}

		private Half FinishUnQuantize(int component, bool signedBc6)
		{
			if (!signedBc6)
			{
				component = (component * 31) >> 6; // scale the magnitude by 31/64
				return Half.ToHalf((ushort)component);
			}
			else // (BC6H::FORMAT == SIGNED_F16)
			{
				component = (component < 0) ? -(((-component) * 31) >> 5) : (component * 31) >> 5; // scale the magnitude by 31/32
				var s = 0;
				if (component < 0)
				{
					s = 0x8000;
					component = -component;
				}
				return Half.ToHalf((ushort)(s | component));
			}
		}

		private (Half, Half, Half) FinishUnQuantize((int, int, int) components, bool signedBc6)
		{
			return (
				FinishUnQuantize(components.Item1, signedBc6),
				FinishUnQuantize(components.Item2, signedBc6),
				FinishUnQuantize(components.Item3, signedBc6)
			);
		}

		private int GetPartitionIndex(int numSubsets, int partitionSetId, int i)
		{
			switch (numSubsets)
			{
				case 1:
					return 0;
				case 2:
					return Subsets2PartitionTable[partitionSetId][i];
				default:
					throw new ArgumentOutOfRangeException(nameof(numSubsets), numSubsets, "Number of subsets can only be 1, 2 or 3");
			}
		}

		private int GetIndexOffset(int numSubsets, int partitionIndex, int bitCount, int index)
		{
			if (index == 0) return 0;
			if (numSubsets == 1)
			{
				return bitCount * index - 1;
			}
			if (numSubsets == 2)
			{
				var anchorIndex = Subsets2AnchorIndices[partitionIndex];
				if (index <= anchorIndex)
				{
					return bitCount * index - 1;
				}
				else
				{
					return bitCount * index - 2;
				}
			}
			throw new ArgumentOutOfRangeException(nameof(numSubsets), numSubsets, "Number of subsets can only be 1, 2 or 3");
		}

		/// <summary>
		/// Decrements bitCount by one if index is one of the anchor indices.
		/// </summary>
		private int GetIndexBitCount(int numSubsets, int partitionIndex, int bitCount, int index)
		{
			if (index == 0) return bitCount - 1;
			if (numSubsets == 2)
			{
				var anchorIndex = Subsets2AnchorIndices[partitionIndex];
				if (index == anchorIndex)
				{
					return bitCount - 1;
				}
			}
			return bitCount;
		}

		private int GetIndexBegin()
		{
			return HasSubsets ? 82 : 65;
		}

		private int GetColorIndex(Bc6BlockType type, int numSubsets, int partitionIndex, int bitCount, int index)
		{
			var indexOffset = GetIndexOffset(numSubsets, partitionIndex, bitCount, index);
			var indexBitCount = GetIndexBitCount(numSubsets, partitionIndex, bitCount, index);
			var indexBegin = GetIndexBegin();
			return (int)ByteHelper.ExtractFrom128(lowBits, highBits, indexBegin + indexOffset, indexBitCount);
		}

		private (int, int, int) InterpolateColor((int, int, int) endPointStart, (int, int, int) endPointEnd,
			int colorIndex, int colorBitCount)
		{

			int InterpolateInt(int e0, int e1, int index, int indexPrecision)
			{
				if (indexPrecision == 0) return e0;
				var aWeights3 = ColorInterpolationWeights3;
				var aWeights4 = ColorInterpolationWeights4;

				if (indexPrecision == 3)
					return ((64 - aWeights3[index]) * e0 + aWeights3[index] * e1 + 32) >> 6;
				else // indexprecision == 4
					return ((64 - aWeights4[index]) * e0 + aWeights4[index] * e1 + 32) >> 6;
			}

			var result = (
				InterpolateInt(endPointStart.Item1, endPointEnd.Item1, colorIndex, colorBitCount),
				InterpolateInt(endPointStart.Item2, endPointEnd.Item2, colorIndex, colorBitCount),
				InterpolateInt(endPointStart.Item3, endPointEnd.Item3, colorIndex, colorBitCount)
			);

			return result;
		}

		public RawBlock4X4RgbFloat Decode(bool signed)
		{
			var output = new RawBlock4X4RgbFloat();
			var pixels = output.AsSpan;

			var endpoints = ExtractRawEndpoints(signed);
			var numSubsets = 1;
			var partitionIndex = 0;

			if (HasSubsets)
			{
				numSubsets = 2;
				partitionIndex = PartitionSetId;
			}
			
			for (var i = 0; i < NumEndpoints; i++)
			{
				endpoints[i] = UnQuantize(endpoints[i], signed);
			}

			for (var i = 0; i < pixels.Length; i++)
			{
				var subsetIndex = GetPartitionIndex(numSubsets, partitionIndex, i);

				var endPointStart = endpoints[2 * subsetIndex];
				var endPointEnd = endpoints[2 * subsetIndex + 1];

				var colorIndex = GetColorIndex(Type, numSubsets, partitionIndex, ColorIndexBitCount, i);

				var outputColor = FinishUnQuantize(InterpolateColor(endPointStart, endPointEnd, colorIndex, ColorIndexBitCount), signed);

				pixels[i] = new ColorRgbFloat(outputColor.Item1, outputColor.Item2, outputColor.Item3);
			}

			return output;
		}
	}
}
