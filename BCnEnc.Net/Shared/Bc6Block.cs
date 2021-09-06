using System;
using System.Diagnostics;
using BCnEncoder.Encoder.Bptc;

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

	internal static class Bc6BlockTypeExtensions
	{
		public static bool HasSubsets(this Bc6BlockType Type) => Type switch
		{
			Bc6BlockType.Type3 => false,
			Bc6BlockType.Type7 => false,
			Bc6BlockType.Type11 => false,
			Bc6BlockType.Type15 => false,
			_ => true
		};

		public static bool HasTransformedEndpoints(this Bc6BlockType Type) => Type switch
		{
			Bc6BlockType.Type3 => false,
			Bc6BlockType.Type30 => false,
			_ => true
		};

		public static int EndpointBits(this Bc6BlockType Type) => Type switch
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

		public static (int, int, int) DeltaBits(this Bc6BlockType Type) => Type switch
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
	}

	internal struct Bc6Block
	{
		public ulong lowBits;
		public ulong highBits;

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

		public static readonly RawBlock4X4RgbFloat ErrorBlock = new RawBlock4X4RgbFloat(new ColorRgbFloat(1, 0, 1));

		public static readonly Bc6BlockType[] Subsets1Types =
		{
			Bc6BlockType.Type3,
			Bc6BlockType.Type7,
			Bc6BlockType.Type11,
			Bc6BlockType.Type15
		};

		public static readonly Bc6BlockType[] Subsets2Types =
		{
			Bc6BlockType.Type0 ,
			Bc6BlockType.Type1 ,
			Bc6BlockType.Type2 ,
			Bc6BlockType.Type6 ,
			Bc6BlockType.Type10,
			Bc6BlockType.Type14,
			Bc6BlockType.Type18,
			Bc6BlockType.Type22,
			Bc6BlockType.Type26,
			Bc6BlockType.Type30
		};

		public readonly Bc6BlockType Type
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

		public readonly bool HasSubsets => Type.HasSubsets();

		public readonly int NumEndpoints => HasSubsets ? 4 : 2;

		public readonly bool HasTransformedEndpoints => Type.HasTransformedEndpoints();

		public readonly int PartitionSetId => HasSubsets ? ByteHelper.Extract5(highBits, 13) : -1;

		public readonly int EndpointBits => Type.EndpointBits();

		public readonly (int, int, int) DeltaBits => Type.DeltaBits();

		public readonly int ColorIndexBitCount => HasSubsets ? 3 : 4;

		internal void StoreEp0((int, int, int) endpoint)
		{
			var r0 = (ulong)endpoint.Item1;
			var g0 = (ulong)endpoint.Item2;
			var b0 = (ulong)endpoint.Item3;
			
			(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 5, Math.Min(10, EndpointBits), r0);
			(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 15, Math.Min(10, EndpointBits), g0);
			(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 25, Math.Min(10, EndpointBits), b0);
			
			switch (Type)
			{
				case Bc6BlockType.Type2:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 40, 1, r0 >> 10);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 49, 1, g0 >> 10);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 59, 1, b0 >> 10);
																				   
					break;
				case Bc6BlockType.Type6:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 39, 1, r0 >> 10);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 50, 1, g0 >> 10);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 59, 1, b0 >> 10);
					
					break;
				case Bc6BlockType.Type10:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 39, 1, r0 >> 10);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 49, 1, g0 >> 10);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 60, 1, b0 >> 10);
					
					break;
				case Bc6BlockType.Type7:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 44, 1, r0 >> 10);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 54, 1, g0 >> 10);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 64, 1, b0 >> 10);
					
					break;
				case Bc6BlockType.Type11:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 44, 1, r0 >> 10);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 54, 1, g0 >> 10);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 64, 1, b0 >> 10);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 43, 1, r0 >> 11);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 53, 1, g0 >> 11);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 63, 1, b0 >> 11);
																						
					break;
				case Bc6BlockType.Type15:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 44, 1, r0 >> 10);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 54, 1, g0 >> 10);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 64, 1, b0 >> 10);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 43, 1, r0 >> 11);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 53, 1, g0 >> 11);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 63, 1, b0 >> 11);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 42, 1, r0 >> 12);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 52, 1, g0 >> 12);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 62, 1, b0 >> 12);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 41, 1, r0 >> 13);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 51, 1, g0 >> 13);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 61, 1, b0 >> 13);
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 40, 1, r0 >> 14);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 50, 1, g0 >> 14);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 60, 1, b0 >> 14);
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 39, 1, r0 >> 15);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 49, 1, g0 >> 15);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 59, 1, b0 >> 15);
					
					break;
			}
		}

		internal readonly (int, int, int) ExtractEp0()
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

		internal void StoreEp1((int, int, int) endpoint)
		{
			var r1 = (ulong)endpoint.Item1;
			var g1 = (ulong)endpoint.Item2;
			var b1 = (ulong)endpoint.Item3;

			if (HasTransformedEndpoints)
			{
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 35, Math.Min(5, DeltaBits.Item1), r1);
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 45, Math.Min(5, DeltaBits.Item2), g1);
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 55, Math.Min(5, DeltaBits.Item3), b1);
				
			}

			switch (Type)
			{
				case Bc6BlockType.Type1:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 40, 1, r1 >> 5);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 50, 1, g1 >> 5);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 60, 1, b1 >> 5);
					

					break;
				case Bc6BlockType.Type18:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 40, 1, r1 >> 5);
					

					break;
				case Bc6BlockType.Type22:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 50, 1, g1 >> 5);

					break;
				case Bc6BlockType.Type26:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 60, 1, b1 >> 5);

					break;
				case Bc6BlockType.Type30:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 35, 6, r1);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 45, 6, g1);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 55, 6, b1);

					break;
				case Bc6BlockType.Type3:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 35, 10, r1);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 45, 10, g1);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 55, 10, b1);

					break;
				case Bc6BlockType.Type7:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 40, 4, r1 >> 5);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 50, 4, g1 >> 5);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 60, 4, b1 >> 5);

					break;
				case Bc6BlockType.Type11:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 40, 3, r1 >> 5);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 50, 3, g1 >> 5);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 60, 3, b1 >> 5);

					break;
			}
		}

		internal readonly (int, int, int) ExtractEp1()
		{
			ulong r1 = 0;
			ulong g1 = 0;
			ulong b1 = 0;

			if (HasTransformedEndpoints)
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

		internal void StoreEp2((int, int, int) endpoint)
		{
			var r2 = (ulong) endpoint.Item1;
			var g2 = (ulong) endpoint.Item2;
			var b2 = (ulong) endpoint.Item3;

			(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 65, Math.Min(5, DeltaBits.Item1), r2);
			(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 41, 4, g2);
			(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 61, 4, b2);

			switch (Type)
			{
				case Bc6BlockType.Type0:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 2, 1, g2 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 3, 1, b2 >> 4);
					
					break;
				case Bc6BlockType.Type1:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 70, 1, r2 >> 5);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 24, 1, g2 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 2, 1, g2 >> 5);
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 14, 1, b2 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 22, 1, b2 >> 5);
					
					break;
				case Bc6BlockType.Type6:

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 75, 1, g2 >> 4);

					break;
				case Bc6BlockType.Type10:

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 40, 1, b2 >> 4);

					break;
				case Bc6BlockType.Type14:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 24, 1, g2 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 14, 1, b2 >> 4);
					
					break;
				case Bc6BlockType.Type18:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 70, 1, r2 >> 4);
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 24, 1, g2 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 14, 1, b2 >> 4);
					
					break;
				case Bc6BlockType.Type22:

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 24, 1, g2 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 23, 1, g2 >> 5);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 14, 1, b2 >> 4);
					
					break;
				case Bc6BlockType.Type26:
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 24, 1, g2 >> 4);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 14, 1, b2 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 23, 1, b2 >> 5);
					
					break;
				case Bc6BlockType.Type30:
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 65, 6, r2);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 24, 1, g2 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 21, 1, g2 >> 5);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 14, 1, b2 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 22, 1, b2 >> 5);
					
					break;
			}
		}

		internal readonly (int, int, int) ExtractEp2()
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

		internal void StoreEp3((int, int, int) endpoint)
		{
			var r3 = (ulong)endpoint.Item1;
			var g3 = (ulong)endpoint.Item2;
			var b3 = (ulong)endpoint.Item3;

			(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 71, Math.Min(5, DeltaBits.Item1), r3);
			(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 51, 4, g3);

			switch (Type)
			{
				case Bc6BlockType.Type0:
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 40, 1, g3 >> 4);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 50, 1, b3 >> 0);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 60, 1, b3 >> 1);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 70, 1, b3 >> 2);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 76, 1, b3 >> 3);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 4, 1, b3 >> 4);
					
					break;
				case Bc6BlockType.Type1:

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 76, 1, r3 >> 5);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 3, 2, g3 >> 4);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 12, 2, b3 >> 0);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 23, 1, b3 >> 2);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 32, 1, b3 >> 3);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 34, 1, b3 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 33, 1, b3 >> 5);

					break;
				case Bc6BlockType.Type2:
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 50, 1, b3 >> 0);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 60, 1, b3 >> 1);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 70, 1, b3 >> 2);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 76, 1, b3 >> 3);

					break;
				case Bc6BlockType.Type6:

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 40, 1, g3 >> 4);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 69, 1, b3 >> 0);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 60, 1, b3 >> 1);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 70, 1, b3 >> 2);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 76, 1, b3 >> 3);

					break;
				case Bc6BlockType.Type10:

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 50, 1, b3 >> 0);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 69, 1, b3 >> 1);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 70, 1, b3 >> 2);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 76, 1, b3 >> 3);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 75, 1, b3 >> 4);
					
					break;
				case Bc6BlockType.Type14:

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 40, 1, g3 >> 4);
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 50, 1, b3 >> 0);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 60, 1, b3 >> 1);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 70, 1, b3 >> 2);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 76, 1, b3 >> 3);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 34, 1, b3 >> 4);
					
					break;
				case Bc6BlockType.Type18:

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 76, 1, r3 >> 4);
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 13, 1, g3 >> 4);
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 50, 1, b3 >> 0);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 60, 1, b3 >> 1);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 23, 1, b3 >> 2);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 33, 1, b3 >> 3);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 34, 1, b3 >> 4);
					
					break;
				case Bc6BlockType.Type22:

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 40, 1, g3 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 33, 1, g3 >> 5);
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 13, 1, b3 >> 0);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 60, 1, b3 >> 1);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 70, 1, b3 >> 2);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 76, 1, b3 >> 3);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 34, 1, b3 >> 4);
					
					break;
				case Bc6BlockType.Type26:

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 40, 1, g3 >> 4);
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 50, 1, b3 >> 0);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 13, 1, b3 >> 1);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 70, 1, b3 >> 2);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 76, 1, b3 >> 3);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 34, 1, b3 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 33, 1, b3 >> 5);
					
					break;
				case Bc6BlockType.Type30:

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 71, 6, r3);
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 11, 1, g3 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 31, 1, g3 >> 5);
					
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 12, 2, b3 >> 0);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 23, 1, b3 >> 2);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 32, 1, b3 >> 3);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 34, 1, b3 >> 4);
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 33, 1, b3 >> 5);
					
					break;
			}
		}

		internal readonly (int, int, int) ExtractEp3()
		{
			ulong r3 = 0;
			ulong g3 = 0;
			ulong b3 = 0;

			r3 = ByteHelper.ExtractFrom128(lowBits, highBits, 71, Math.Min(5, DeltaBits.Item1));
			g3 = ByteHelper.ExtractFrom128(lowBits, highBits, 51, 4);

			switch (Type)
			{
				case Bc6BlockType.Type0:
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

		private readonly (int, int, int)[] ExtractRawEndpoints(bool signedBc6)
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

			if (HasTransformedEndpoints)
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

				if (HasTransformedEndpoints)
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

		internal static int UnQuantize(int component, int endpointBits, bool signedBc6)
		{
			int unq;
			var sign = false;

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

		internal static (int, int, int) UnQuantize((int, int, int) components, int endpointBits, bool signedBc6)
		{
			return (
				UnQuantize(components.Item1, endpointBits, signedBc6),
				UnQuantize(components.Item2, endpointBits, signedBc6),
				UnQuantize(components.Item3, endpointBits, signedBc6)
			);
		}

		internal static Half FinishUnQuantize(int component, bool signedBc6)
		{
			if (!signedBc6)
			{
				component = (component * 31) >> 6; // scale the magnitude by 31/64
				return Half.ToHalf((ushort)component);
			}
			else // signed format
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

		internal static (Half, Half, Half) FinishUnQuantize((int, int, int) components, bool signedBc6)
		{
			return (
				FinishUnQuantize(components.Item1, signedBc6),
				FinishUnQuantize(components.Item2, signedBc6),
				FinishUnQuantize(components.Item3, signedBc6)
			);
		}

		private static int GetPartitionIndex(int numSubsets, int partitionSetId, int i)
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

		private static int GetIndexOffset(int numSubsets, int partitionIndex, int bitCount, int index)
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
		private static int GetIndexBitCount(int numSubsets, int partitionIndex, int bitCount, int index)
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

		private readonly int GetIndexBegin()
		{
			return HasSubsets ? 82 : 65;
		}

		internal readonly int GetColorIndex(int numSubsets, int partitionIndex, int bitCount, int index)
		{
			var indexOffset = GetIndexOffset(numSubsets, partitionIndex, bitCount, index);
			var indexBitCount = GetIndexBitCount(numSubsets, partitionIndex, bitCount, index);
			var indexBegin = GetIndexBegin();
			return (int)ByteHelper.ExtractFrom128(lowBits, highBits, indexBegin + indexOffset, indexBitCount);
		}

		internal static (int, int, int) InterpolateColor((int, int, int) endPointStart, (int, int, int) endPointEnd,
			int colorIndex, int colorBitCount)
		{
			var result = (
				BptcEncodingHelpers.InterpolateInt(endPointStart.Item1, endPointEnd.Item1, colorIndex, colorBitCount),
				BptcEncodingHelpers.InterpolateInt(endPointStart.Item2, endPointEnd.Item2, colorIndex, colorBitCount),
				BptcEncodingHelpers.InterpolateInt(endPointStart.Item3, endPointEnd.Item3, colorIndex, colorBitCount)
			);

			return result;
		}

		public readonly RawBlock4X4RgbFloat Decode(bool signed)
		{
			var output = new RawBlock4X4RgbFloat();
			var pixels = output.AsSpan;

			if (Type == Bc6BlockType.Unknown)
			{
				return ErrorBlock;
			}

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
				endpoints[i] = UnQuantize(endpoints[i], EndpointBits, signed);
			}

			for (var i = 0; i < pixels.Length; i++)
			{
				var subsetIndex = GetPartitionIndex(numSubsets, partitionIndex, i);

				var endPointStart = endpoints[2 * subsetIndex];
				var endPointEnd = endpoints[2 * subsetIndex + 1];

				var colorIndex = GetColorIndex(numSubsets, partitionIndex, ColorIndexBitCount, i);

				var (r, g, b) = FinishUnQuantize(InterpolateColor(endPointStart, endPointEnd, colorIndex, ColorIndexBitCount), signed);

				pixels[i] = new ColorRgbFloat(r, g, b);
			}

			return output;
		}

		#region pack methods

		private void StoreIndices(Span<byte> indices)
		{
			Debug.Assert(indices.Length == 16);
			var numSubsets = HasSubsets ? 2 : 1;
			var partSetId = PartitionSetId;
			var colorBitCount = ColorIndexBitCount;
			var colorIndexBegin = GetIndexBegin();
			for (var i = 0; i < indices.Length; i++)
			{
				var partitionIndex = GetPartitionIndex(numSubsets, partSetId, i);
				var indexOffset = GetIndexOffset(numSubsets, partitionIndex, colorBitCount, i);
				var indexBitCount = GetIndexBitCount(numSubsets, partitionIndex, colorBitCount, i);

				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits,
					colorIndexBegin + indexOffset, indexBitCount, indices[i]);
			}
		}

		private void StorePartitionSetId(int partitionSetId)
		{
			highBits = ByteHelper.Store5(highBits, 13, (byte) partitionSetId);
		}
		
		public static Bc6Block PackType0((int, int, int) endpoint0, (int, int, int) endpoint1,
			(int, int, int) endpoint2, (int, int, int) endpoint3, int partitionSetId, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 0;
			block.StorePartitionSetId(partitionSetId);
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreEp2(endpoint2);
			block.StoreEp3(endpoint3);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType1((int, int, int) endpoint0, (int, int, int) endpoint1,
			(int, int, int) endpoint2, (int, int, int) endpoint3, int partitionSetId, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 1;
			block.StorePartitionSetId(partitionSetId);
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreEp2(endpoint2);
			block.StoreEp3(endpoint3);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType2((int, int, int) endpoint0, (int, int, int) endpoint1,
			(int, int, int) endpoint2, (int, int, int) endpoint3, int partitionSetId, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 2;
			block.StorePartitionSetId(partitionSetId);
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreEp2(endpoint2);
			block.StoreEp3(endpoint3);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType6((int, int, int) endpoint0, (int, int, int) endpoint1,
			(int, int, int) endpoint2, (int, int, int) endpoint3, int partitionSetId, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 6;
			block.StorePartitionSetId(partitionSetId);
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreEp2(endpoint2);
			block.StoreEp3(endpoint3);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType10((int, int, int) endpoint0, (int, int, int) endpoint1,
			(int, int, int) endpoint2, (int, int, int) endpoint3, int partitionSetId, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 10;
			block.StorePartitionSetId(partitionSetId);
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreEp2(endpoint2);
			block.StoreEp3(endpoint3);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType14((int, int, int) endpoint0, (int, int, int) endpoint1,
			(int, int, int) endpoint2, (int, int, int) endpoint3, int partitionSetId, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 14;
			block.StorePartitionSetId(partitionSetId);
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreEp2(endpoint2);
			block.StoreEp3(endpoint3);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType18((int, int, int) endpoint0, (int, int, int) endpoint1,
			(int, int, int) endpoint2, (int, int, int) endpoint3, int partitionSetId, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 18;
			block.StorePartitionSetId(partitionSetId);
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreEp2(endpoint2);
			block.StoreEp3(endpoint3);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType22((int, int, int) endpoint0, (int, int, int) endpoint1,
			(int, int, int) endpoint2, (int, int, int) endpoint3, int partitionSetId, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 22;
			block.StorePartitionSetId(partitionSetId);
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreEp2(endpoint2);
			block.StoreEp3(endpoint3);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType26((int, int, int) endpoint0, (int, int, int) endpoint1,
			(int, int, int) endpoint2, (int, int, int) endpoint3, int partitionSetId, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 26;
			block.StorePartitionSetId(partitionSetId);
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreEp2(endpoint2);
			block.StoreEp3(endpoint3);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType30((int, int, int) endpoint0, (int, int, int) endpoint1,
			(int, int, int) endpoint2, (int, int, int) endpoint3, int partitionSetId, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 30;
			block.StorePartitionSetId(partitionSetId);
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreEp2(endpoint2);
			block.StoreEp3(endpoint3);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType3((int, int, int) endpoint0, (int, int, int) endpoint1, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 3;
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType7((int, int, int) endpoint0, (int, int, int) endpoint1, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 7;
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType11((int, int, int) endpoint0, (int, int, int) endpoint1, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 11;
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreIndices(indices);

			return block;
		}

		public static Bc6Block PackType15((int, int, int) endpoint0, (int, int, int) endpoint1, Span<byte> indices)
		{
			var block = new Bc6Block();
			block.lowBits = 15;
			block.StoreEp0(endpoint0);
			block.StoreEp1(endpoint1);
			block.StoreIndices(indices);

			return block;
		}

		#endregion


	}
}
