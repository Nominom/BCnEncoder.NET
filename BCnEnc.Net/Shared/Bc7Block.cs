using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BCnEncoder.Shared
{
	internal enum Bc7BlockType : uint
	{
		Type0,
		Type1,
		Type2,
		Type3,
		Type4,
		Type5,
		Type6,
		Type7,
		Type8Reserved
	}

	internal struct Bc7Block
	{
		public ulong lowBits;
		public ulong highBits;

		public static ReadOnlySpan<byte> ColorInterpolationWeights2 => new byte[] { 0, 21, 43, 64 };
		public static ReadOnlySpan<byte> ColorInterpolationWeights3 => new byte[] { 0, 9, 18, 27, 37, 46, 55, 64 };
		public static ReadOnlySpan<byte> ColorInterpolationWeights4 => new byte[] { 0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64 };


		public static readonly int[][] Subsets2PartitionTable = {
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
			new[] {0, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0},
			new[] {0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1},
			new[] {0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1},
			new[] {0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0},
			new[] {0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0},
			new[] {0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0},
			new[] {0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0},
			new[] {0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1},
			new[] {0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1},
			new[] {0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0},
			new[] {0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0},
			new[] {0, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0},
			new[] {0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 0, 0},
			new[] {0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0},
			new[] {0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1},
			new[] {0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1},
			new[] {0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0},
			new[] {0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0},
			new[] {0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0},
			new[] {0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0},
			new[] {0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0},
			new[] {0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1},
			new[] {0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1},
			new[] {0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0},
			new[] {0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 0},
			new[] {0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 1},
			new[] {0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1},
			new[] {0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1},
			new[] {0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1},
			new[] {0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
			new[] {0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0},
			new[] {0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0},
			new[] {0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1}
		};

		public static readonly int[][] Subsets3PartitionTable = {
			new[] {0, 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 1, 2, 2, 2, 2},
			new[] {0, 0, 0, 1, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 2, 1},
			new[] {0, 0, 0, 0, 2, 0, 0, 1, 2, 2, 1, 1, 2, 2, 1, 1},
			new[] {0, 2, 2, 2, 0, 0, 2, 2, 0, 0, 1, 1, 0, 1, 1, 1},
			new[] {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2},
			new[] {0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 2, 2, 0, 0, 2, 2},
			new[] {0, 0, 2, 2, 0, 0, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1},
			new[] {0, 0, 1, 1, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1},
			new[] {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2},
			new[] {0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2},
			new[] {0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2},
			new[] {0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2},
			new[] {0, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2},
			new[] {0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 2},
			new[] {0, 0, 1, 1, 0, 1, 1, 2, 1, 1, 2, 2, 1, 2, 2, 2},
			new[] {0, 0, 1, 1, 2, 0, 0, 1, 2, 2, 0, 0, 2, 2, 2, 0},
			new[] {0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 2, 1, 1, 2, 2},
			new[] {0, 1, 1, 1, 0, 0, 1, 1, 2, 0, 0, 1, 2, 2, 0, 0},
			new[] {0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2},
			new[] {0, 0, 2, 2, 0, 0, 2, 2, 0, 0, 2, 2, 1, 1, 1, 1},
			new[] {0, 1, 1, 1, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2},
			new[] {0, 0, 0, 1, 0, 0, 0, 1, 2, 2, 2, 1, 2, 2, 2, 1},
			new[] {0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 2, 2, 0, 1, 2, 2},
			new[] {0, 0, 0, 0, 1, 1, 0, 0, 2, 2, 1, 0, 2, 2, 1, 0},
			new[] {0, 1, 2, 2, 0, 1, 2, 2, 0, 0, 1, 1, 0, 0, 0, 0},
			new[] {0, 0, 1, 2, 0, 0, 1, 2, 1, 1, 2, 2, 2, 2, 2, 2},
			new[] {0, 1, 1, 0, 1, 2, 2, 1, 1, 2, 2, 1, 0, 1, 1, 0},
			new[] {0, 0, 0, 0, 0, 1, 1, 0, 1, 2, 2, 1, 1, 2, 2, 1},
			new[] {0, 0, 2, 2, 1, 1, 0, 2, 1, 1, 0, 2, 0, 0, 2, 2},
			new[] {0, 1, 1, 0, 0, 1, 1, 0, 2, 0, 0, 2, 2, 2, 2, 2},
			new[] {0, 0, 1, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 0, 1, 1},
			new[] {0, 0, 0, 0, 2, 0, 0, 0, 2, 2, 1, 1, 2, 2, 2, 1},
			new[] {0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 2, 2, 2},
			new[] {0, 2, 2, 2, 0, 0, 2, 2, 0, 0, 1, 2, 0, 0, 1, 1},
			new[] {0, 0, 1, 1, 0, 0, 1, 2, 0, 0, 2, 2, 0, 2, 2, 2},
			new[] {0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0},
			new[] {0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0, 0, 0, 0},
			new[] {0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0},
			new[] {0, 1, 2, 0, 2, 0, 1, 2, 1, 2, 0, 1, 0, 1, 2, 0},
			new[] {0, 0, 1, 1, 2, 2, 0, 0, 1, 1, 2, 2, 0, 0, 1, 1},
			new[] {0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0, 0, 0, 0, 1, 1},
			new[] {0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2},
			new[] {0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 2, 1, 2, 1, 2, 1},
			new[] {0, 0, 2, 2, 1, 1, 2, 2, 0, 0, 2, 2, 1, 1, 2, 2},
			new[] {0, 0, 2, 2, 0, 0, 1, 1, 0, 0, 2, 2, 0, 0, 1, 1},
			new[] {0, 2, 2, 0, 1, 2, 2, 1, 0, 2, 2, 0, 1, 2, 2, 1},
			new[] {0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 0, 1, 0, 1},
			new[] {0, 0, 0, 0, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1},
			new[] {0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2},
			new[] {0, 2, 2, 2, 0, 1, 1, 1, 0, 2, 2, 2, 0, 1, 1, 1},
			new[] {0, 0, 0, 2, 1, 1, 1, 2, 0, 0, 0, 2, 1, 1, 1, 2},
			new[] {0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2},
			new[] {0, 2, 2, 2, 0, 1, 1, 1, 0, 1, 1, 1, 0, 2, 2, 2},
			new[] {0, 0, 0, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2},
			new[] {0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 2, 2},
			new[] {0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 1, 2},
			new[] {0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 2, 2, 2, 2, 2, 2},
			new[] {0, 0, 2, 2, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 2, 2},
			new[] {0, 0, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 0, 0, 2, 2},
			new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2},
			new[] {0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 1},
			new[] {0, 2, 2, 2, 1, 2, 2, 2, 0, 2, 2, 2, 1, 2, 2, 2},
			new[] {0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2},
			new[] {0, 1, 1, 1, 2, 0, 1, 1, 2, 2, 0, 1, 2, 2, 2, 0},
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

		public static readonly int[] Subsets3AnchorIndices2 = {
			3, 3, 15, 15, 8, 3, 15, 15,
			8, 8, 6, 6, 6, 5, 3, 3,
			3, 3, 8, 15, 3, 3, 6, 10,
			5, 8, 8, 6, 8, 5, 15, 15,
			8, 15, 3, 5, 6, 10, 8, 15,
			15, 3, 15, 5, 15, 15, 15, 15,
			3, 15, 5, 5, 5, 8, 5, 10,
			5, 10, 8, 13, 15, 12, 3, 3
		};

		public static readonly int[] Subsets3AnchorIndices3 = {
			15, 8, 8, 3, 15, 15, 3, 8,
			15, 15, 15, 15, 15, 15, 15, 8,
			15, 8, 15, 3, 15, 8, 15, 8,
			3, 15, 6, 10, 15, 15, 10, 8,
			15, 3, 15, 10, 10, 8, 9, 10,
			6, 15, 8, 15, 3, 6, 6, 8,
			15, 3, 15, 15, 15, 15, 15, 15,
			15, 15, 15, 15, 3, 15, 15, 8
		};

		public Bc7BlockType Type
		{
			get
			{
				for (var i = 0; i < 8; i++)
				{
					var mask = (ulong)(1 << i);
					if ((lowBits & mask) == mask)
					{
						return (Bc7BlockType)i;
					}
				}

				return Bc7BlockType.Type8Reserved;
			}
		}

		public int NumSubsets => Type switch
		{
			Bc7BlockType.Type0 => 3,
			Bc7BlockType.Type1 => 2,
			Bc7BlockType.Type2 => 3,
			Bc7BlockType.Type3 => 2,
			Bc7BlockType.Type7 => 2,
			_ => 1
		};

		public bool HasSubsets => Type switch
		{
			Bc7BlockType.Type0 => true,
			Bc7BlockType.Type1 => true,
			Bc7BlockType.Type2 => true,
			Bc7BlockType.Type3 => true,
			Bc7BlockType.Type7 => true,
			_ => false
		};

		public int PartitionSetId => Type switch
		{
			Bc7BlockType.Type0 => ByteHelper.Extract4(lowBits, 1),
			Bc7BlockType.Type1 => ByteHelper.Extract6(lowBits, 2),
			Bc7BlockType.Type2 => ByteHelper.Extract6(lowBits, 3),
			Bc7BlockType.Type3 => ByteHelper.Extract6(lowBits, 4),
			Bc7BlockType.Type7 => ByteHelper.Extract6(lowBits, 8),
			_ => -1
		};

		public byte RotationBits => Type switch
		{
			Bc7BlockType.Type4 => ByteHelper.Extract2(lowBits, 5),
			Bc7BlockType.Type5 => ByteHelper.Extract2(lowBits, 6),
			_ => 0
		};

		/// <summary>
		/// Bitcount of color component including Pbit
		/// </summary>
		public int ColorComponentPrecision => Type switch
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
		/// Bitcount of alpha component including Pbit
		/// </summary>
		public int AlphaComponentPrecision => Type switch
		{

			Bc7BlockType.Type4 => 6,
			Bc7BlockType.Type5 => 8,
			Bc7BlockType.Type6 => 8,
			Bc7BlockType.Type7 => 6,
			_ => 0
		};

		public bool HasRotationBits => Type switch
		{
			Bc7BlockType.Type4 => true,
			Bc7BlockType.Type5 => true,
			_ => false
		};

		public bool HasPBits => Type switch
		{
			Bc7BlockType.Type0 => true,
			Bc7BlockType.Type1 => true,
			Bc7BlockType.Type3 => true,
			Bc7BlockType.Type6 => true,
			Bc7BlockType.Type7 => true,
			_ => false
		};

		public bool HasAlpha => Type switch
		{
			Bc7BlockType.Type4 => true,
			Bc7BlockType.Type5 => true,
			Bc7BlockType.Type6 => true,
			Bc7BlockType.Type7 => true,
			_ => false
		};

		/// <summary>
		/// Type 4 has 2-bit and 3-bit indices. If index mode is 0, color components will use 2-bit indices and alpha will use 3-bit indices.
		/// In index mode 1, color will use 3-bit indices and alpha will use 2-bit indices.
		/// </summary>
		public int Type4IndexMode => Type switch
		{
			Bc7BlockType.Type4 => ByteHelper.Extract1(lowBits, 7),
			_ => 0
		};

		public int ColorIndexBitCount => Type switch
		{
			Bc7BlockType.Type0 => 3,
			Bc7BlockType.Type1 => 3,
			Bc7BlockType.Type2 => 2,
			Bc7BlockType.Type3 => 2,
			Bc7BlockType.Type4 when Type4IndexMode == 0 => 2,
			Bc7BlockType.Type4 when Type4IndexMode == 1 => 3,
			Bc7BlockType.Type5 => 2,
			Bc7BlockType.Type6 => 4,
			Bc7BlockType.Type7 => 2,
			_ => 0
		};

		public int AlphaIndexBitCount => Type switch
		{
			Bc7BlockType.Type4 when Type4IndexMode == 0 => 3,
			Bc7BlockType.Type4 when Type4IndexMode == 1 => 2,
			Bc7BlockType.Type5 => 2,
			Bc7BlockType.Type6 => 4,
			Bc7BlockType.Type7 => 2,
			_ => 0
		};



		private void ExtractRawEndpoints(ColorRgba32[] endpoints)
		{
			switch (Type)
			{
				case Bc7BlockType.Type0:
					endpoints[0].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 5, 4);
					endpoints[1].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 9, 4);
					endpoints[2].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 13, 4);
					endpoints[3].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 17, 4);
					endpoints[4].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 21, 4);
					endpoints[5].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 25, 4);

					endpoints[0].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 29, 4);
					endpoints[1].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 33, 4);
					endpoints[2].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 37, 4);
					endpoints[3].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 41, 4);
					endpoints[4].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 45, 4);
					endpoints[5].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 49, 4);

					endpoints[0].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 53, 4);
					endpoints[1].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 57, 4);
					endpoints[2].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 61, 4);
					endpoints[3].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 65, 4);
					endpoints[4].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 69, 4);
					endpoints[5].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 73, 4);
					break;
				case Bc7BlockType.Type1:
					endpoints[0].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 8, 6);
					endpoints[1].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 14, 6);
					endpoints[2].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 20, 6);
					endpoints[3].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 26, 6);

					endpoints[0].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 32, 6);
					endpoints[1].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 38, 6);
					endpoints[2].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 44, 6);
					endpoints[3].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 50, 6);

					endpoints[0].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 56, 6);
					endpoints[1].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 62, 6);
					endpoints[2].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 68, 6);
					endpoints[3].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 74, 6);
					break;
				case Bc7BlockType.Type2:
					endpoints[0].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 9, 5);
					endpoints[1].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 14, 5);
					endpoints[2].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 19, 5);
					endpoints[3].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 24, 5);
					endpoints[4].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 29, 5);
					endpoints[5].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 34, 5);

					endpoints[0].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 39, 5);
					endpoints[1].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 44, 5);
					endpoints[2].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 49, 5);
					endpoints[3].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 54, 5);
					endpoints[4].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 59, 5);
					endpoints[5].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 64, 5);

					endpoints[0].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 69, 5);
					endpoints[1].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 74, 5);
					endpoints[2].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 79, 5);
					endpoints[3].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 84, 5);
					endpoints[4].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 89, 5);
					endpoints[5].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 94, 5);
					break;
				case Bc7BlockType.Type3:
					endpoints[0].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 10, 7);
					endpoints[1].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 17, 7);
					endpoints[2].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 24, 7);
					endpoints[3].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 31, 7);

					endpoints[0].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 38, 7);
					endpoints[1].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 45, 7);
					endpoints[2].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 52, 7);
					endpoints[3].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 59, 7);

					endpoints[0].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 66, 7);
					endpoints[1].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 73, 7);
					endpoints[2].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 80, 7);
					endpoints[3].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 87, 7);
					break;
				case Bc7BlockType.Type4:
					endpoints[0].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 8, 5);
					endpoints[1].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 13, 5);

					endpoints[0].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 18, 5);
					endpoints[1].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 23, 5);

					endpoints[0].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 28, 5);
					endpoints[1].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 33, 5);

					endpoints[0].a = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 38, 6);
					endpoints[1].a = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 44, 6);
					break;
				case Bc7BlockType.Type5:
					endpoints[0].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 8, 7);
					endpoints[1].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 15, 7);

					endpoints[0].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 22, 7);
					endpoints[1].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 29, 7);

					endpoints[0].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 36, 7);
					endpoints[1].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 43, 7);

					endpoints[0].a = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 50, 8);
					endpoints[1].a = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 58, 8);
					break;
				case Bc7BlockType.Type6:
					endpoints[0].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 7, 7);
					endpoints[1].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 14, 7);

					endpoints[0].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 21, 7);
					endpoints[1].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 28, 7);

					endpoints[0].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 35, 7);
					endpoints[1].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 42, 7);

					endpoints[0].a = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 49, 7);
					endpoints[1].a = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 56, 7);
					break;
				case Bc7BlockType.Type7:
					endpoints[0].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 14, 5);
					endpoints[1].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 19, 5);
					endpoints[2].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 24, 5);
					endpoints[3].r = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 29, 5);

					endpoints[0].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 34, 5);
					endpoints[1].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 39, 5);
					endpoints[2].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 44, 5);
					endpoints[3].g = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 49, 5);

					endpoints[0].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 54, 5);
					endpoints[1].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 59, 5);
					endpoints[2].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 64, 5);
					endpoints[3].b = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 69, 5);

					endpoints[0].a = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 74, 5);
					endpoints[1].a = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 79, 5);
					endpoints[2].a = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 84, 5);
					endpoints[3].a = (byte)ByteHelper.ExtractFrom128(lowBits, highBits, 89, 5);
					break;
				default:
					throw new InvalidDataException();
			}
		}

		private byte[] ExtractPBitArray()
		{
			switch (Type)
			{
				case Bc7BlockType.Type0:
					return new[]{
						ByteHelper.Extract1(highBits, 77 - 64),
						ByteHelper.Extract1(highBits, 78 - 64),
						ByteHelper.Extract1(highBits, 79 - 64),
						ByteHelper.Extract1(highBits, 80 - 64),
						ByteHelper.Extract1(highBits, 81 - 64),
						ByteHelper.Extract1(highBits, 82 - 64),
					};
				case Bc7BlockType.Type1:
					return new[]{
						ByteHelper.Extract1(highBits, 80 - 64),
						ByteHelper.Extract1(highBits, 81 - 64)
					};
				case Bc7BlockType.Type3:
					return new[]{
						ByteHelper.Extract1(highBits, 94 - 64),
						ByteHelper.Extract1(highBits, 95 - 64),
						ByteHelper.Extract1(highBits, 96 - 64),
						ByteHelper.Extract1(highBits, 97 - 64)
					};
				case Bc7BlockType.Type6:
					return new[]{
						ByteHelper.Extract1(lowBits, 63),
						ByteHelper.Extract1(highBits, 0)
					};
				case Bc7BlockType.Type7:
					return new[]{
						ByteHelper.Extract1(highBits, 94 - 64),
						ByteHelper.Extract1(highBits, 95 - 64),
						ByteHelper.Extract1(highBits, 96 - 64),
						ByteHelper.Extract1(highBits, 97 - 64)
					};
				default:
					return Array.Empty<byte>();
			}
		}

		private void FinalizeEndpoints(ColorRgba32[] endpoints)
		{
			if (HasPBits)
			{
				for (var i = 0; i < endpoints.Length; i++)
				{
					endpoints[i] <<= 1;
				}
				var pBits = ExtractPBitArray();

				if (Type == Bc7BlockType.Type1)
				{
					endpoints[0] |= pBits[0];
					endpoints[1] |= pBits[0];
					endpoints[2] |= pBits[1];
					endpoints[3] |= pBits[1];
				}
				else
				{
					for (var i = 0; i < endpoints.Length; i++)
					{
						endpoints[i] |= pBits[i];
					}
				}
			}

			var colorPrecision = ColorComponentPrecision;
			var alphaPrecision = AlphaComponentPrecision;
			for (var i = 0; i < endpoints.Length; i++)
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
			if (!HasAlpha)
			{
				for (var i = 0; i < endpoints.Length; i++)
				{
					endpoints[i].a = 255;
				}
			}
		}

		public ColorRgba32[] ExtractEndpoints()
		{
			var endpoints = new ColorRgba32[NumSubsets * 2];
			ExtractRawEndpoints(endpoints);
			FinalizeEndpoints(endpoints);
			return endpoints;
		}

		private int GetPartitionIndex(int numSubsets, int partitionSetId, int i)
		{
			switch (numSubsets)
			{
				case 1:
					return 0;
				case 2:
					return Subsets2PartitionTable[partitionSetId][i];
				case 3:
					return Subsets3PartitionTable[partitionSetId][i];
				default:
					throw new ArgumentOutOfRangeException(nameof(numSubsets), numSubsets, "Number of subsets can only be 1, 2 or 3");
			}
		}

		private int GetIndexOffset(Bc7BlockType type, int numSubsets, int partitionIndex, int bitCount, int index)
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
			if (numSubsets == 3)
			{
				var anchor2Index = Subsets3AnchorIndices2[partitionIndex];
				var anchor3Index = Subsets3AnchorIndices3[partitionIndex];

				if (index <= anchor2Index && index <= anchor3Index)
				{
					return bitCount * index - 1;
				}
				else if (index > anchor2Index && index > anchor3Index)
				{
					return bitCount * index - 3;
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
			else if (numSubsets == 3)
			{
				var anchor2Index = Subsets3AnchorIndices2[partitionIndex];
				var anchor3Index = Subsets3AnchorIndices3[partitionIndex];
				if (index == anchor2Index)
				{
					return bitCount - 1;
				}
				if (index == anchor3Index)
				{
					return bitCount - 1;
				}
			}
			return bitCount;
		}

		private int GetIndexBegin(Bc7BlockType type, int bitCount, bool isAlpha)
		{
			switch (type)
			{
				case Bc7BlockType.Type0:
					return 83;
				case Bc7BlockType.Type1:
					return 82;
				case Bc7BlockType.Type2:
					return 99;
				case Bc7BlockType.Type3:
					return 98;
				case Bc7BlockType.Type4:
					if (bitCount == 2)
					{
						return 50;
					}
					else
					{
						return 81;
					}
				case Bc7BlockType.Type5:
					if (isAlpha)
					{
						return 97;
					}
					else
					{
						return 66;
					}
				case Bc7BlockType.Type6:
					return 65;
				case Bc7BlockType.Type7:
					return 98;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		private int GetAlphaIndex(Bc7BlockType type, int numSubsets, int partitionIndex, int bitCount, int index) {
			if (bitCount == 0) return 0; // No Alpha
			var indexOffset = GetIndexOffset(type, numSubsets, partitionIndex, bitCount, index);
			var indexBitCount = GetIndexBitCount(numSubsets, partitionIndex, bitCount, index);
			var indexBegin = GetIndexBegin(type, bitCount, true);
			return (int)ByteHelper.ExtractFrom128(lowBits, highBits, indexBegin + indexOffset, indexBitCount);
		}

		private int GetColorIndex(Bc7BlockType type, int numSubsets, int partitionIndex, int bitCount, int index)
		{
			var indexOffset = GetIndexOffset(type, numSubsets, partitionIndex, bitCount, index);
			var indexBitCount = GetIndexBitCount(numSubsets, partitionIndex, bitCount, index);
			var indexBegin = GetIndexBegin(type, bitCount, false);
			return (int)ByteHelper.ExtractFrom128(lowBits, highBits, indexBegin + indexOffset, indexBitCount);
		}

		private ColorRgba32 InterpolateColor(ColorRgba32 endPointStart, ColorRgba32 endPointEnd,
			int colorIndex, int alphaIndex, int colorBitCount, int alphaBitCount)
		{
			
			byte InterpolateByte(byte e0, byte e1, int index, int indexPrecision) {
				if (indexPrecision == 0) return e0;
				var aWeights2 = ColorInterpolationWeights2;
				var aWeights3 = ColorInterpolationWeights3;
				var aWeights4 = ColorInterpolationWeights4;

				if(indexPrecision == 2)
					return (byte) (((64 - aWeights2[index])* e0 + aWeights2[index]*e1 + 32) >> 6);
				else if(indexPrecision == 3)
					return (byte) (((64 - aWeights3[index])*e0 + aWeights3[index]*e1 + 32) >> 6);
				else // indexprecision == 4
					return (byte) (((64 - aWeights4[index])*e0 + aWeights4[index]*e1 + 32) >> 6);
			}

			var result = new ColorRgba32(
				InterpolateByte(endPointStart.r, endPointEnd.r, colorIndex, colorBitCount),
				InterpolateByte(endPointStart.g, endPointEnd.g, colorIndex, colorBitCount),
				InterpolateByte(endPointStart.b, endPointEnd.b, colorIndex, colorBitCount),
				InterpolateByte(endPointStart.a, endPointEnd.a, alphaIndex, alphaBitCount)
				);

			return result;
		}
		/// <summary>
		/// 00 – no swapping
		/// 01 – swap A and R
		/// 10 – swap A and G
		/// 11 - swap A and B
		/// </summary>
		private static ColorRgba32 SwapChannels(ColorRgba32 source, int rotation) {
			switch (rotation) {
				case 0b00:
					return source;
				case 0b01:
					return new ColorRgba32(source.a, source.g, source.b, source.r);
				case 0b10:
					return new ColorRgba32(source.r, source.a, source.b, source.g);
				case 0b11:
					return new ColorRgba32(source.r, source.g, source.a, source.b);
				default:
					return source;
			}
		}


		public RawBlock4X4Rgba32 Decode()
		{
			var output = new RawBlock4X4Rgba32();
			var type = Type;

			////decode partition data from explicit partition bits
			//subset_index = 0;
			var numSubsets = 1;
			var partitionIndex = 0;

			if (HasSubsets)
			{
				numSubsets = NumSubsets;
				partitionIndex = PartitionSetId;
				//subset_index = get_partition_index(num_subsets, partition_set_id, x, y);
			}

			var pixels = output.AsSpan;

			var hasRotationBits = HasRotationBits;
			int rotation = RotationBits;

			var endpoints = ExtractEndpoints();
			for (var i = 0; i < pixels.Length; i++)
			{
				var subsetIndex = GetPartitionIndex(numSubsets, partitionIndex, i);

				var endPointStart = endpoints[2 * subsetIndex];
				var endPointEnd = endpoints[2 * subsetIndex + 1];

				var alphaBitCount = AlphaIndexBitCount;
				var colorBitCount = ColorIndexBitCount;
				var alphaIndex = GetAlphaIndex(type, numSubsets, partitionIndex, alphaBitCount, i);
				var colorIndex = GetColorIndex(type, numSubsets, partitionIndex, colorBitCount, i);

				var outputColor = InterpolateColor(endPointStart, endPointEnd, colorIndex, alphaIndex,
					colorBitCount, alphaBitCount);

				if (hasRotationBits) {
					//Decode the 2 color rotation bits as follows:
					// 00 – Block Format is Scalar(A) Vector(RGB) - no swapping
					// 01 – Block Format is Scalar(R) Vector(AGB) - swap A and R
					// 10 – Block Format is Scalar(G) Vector(RAB) - swap A and G
					// 11 - Block Format is Scalar(B) Vector(RGA) - swap A and B
					outputColor = SwapChannels(outputColor, rotation);
				}

				pixels[i] = outputColor;
			}

			return output;
		}


		public void PackType0(int partitionIndex4Bit, byte[][] subsetEndpoints, byte[] pBits, byte[] indices) {
			Debug.Assert(partitionIndex4Bit < 16, "Mode 0 should have 4bit partition index");
			Debug.Assert(subsetEndpoints.Length == 6, "Mode 0 should have 6 endpoints");
			Debug.Assert(subsetEndpoints.All(x => x.Length == 3) , "Mode 0 should have RGB endpoints");
			Debug.Assert(subsetEndpoints.All(x => x.All(y => y < 1 << 4)) , "Mode 0 should have 4bit endpoints");
			Debug.Assert(pBits.Length == 6, "Mode 0 should have 6 pBits");
			Debug.Assert(indices.Length == 16, "Provide 16 indices");
			Debug.Assert(indices.All(x => x < 1 << 3) , "Mode 0 should have 3bit indices");

			lowBits = 1; // Set Mode 0
			highBits = 0;

			lowBits = ByteHelper.Store4(lowBits, 1, (byte) partitionIndex4Bit);

			var nextIdx = 5;
			//Store endpoints
			for (var i = 0; i < subsetEndpoints[0].Length; i++) {
				for (var j = 0; j < subsetEndpoints.Length; j++) {
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, nextIdx, 4, subsetEndpoints[j][i]);
					nextIdx += 4;
				}
			}
			//Store pBits
			for (var i = 0; i < pBits.Length; i++) {
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, nextIdx, 1, pBits[i]);
				nextIdx++;
			}
			Debug.Assert(nextIdx == 83);

			var colorBitCount = ColorIndexBitCount;
			var indexBegin = GetIndexBegin(Bc7BlockType.Type0, colorBitCount, false);
			for (var i = 0; i < 16; i++) {
				var indexOffset = GetIndexOffset(Bc7BlockType.Type0, NumSubsets, 
					partitionIndex4Bit, colorBitCount, i);
				var indexBitCount = GetIndexBitCount(NumSubsets, 
					partitionIndex4Bit, colorBitCount, i);
				
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
					indexBegin + indexOffset, indexBitCount, indices[i]);
			}
		}

		public void PackType1(int partitionIndex6Bit, byte[][] subsetEndpoints, byte[] pBits, byte[] indices) {
			Debug.Assert(partitionIndex6Bit < 64, "Mode 1 should have 6bit partition index");
			Debug.Assert(subsetEndpoints.Length == 4, "Mode 1 should have 4 endpoints");
			Debug.Assert(subsetEndpoints.All(x => x.Length == 3) , "Mode 1 should have RGB endpoints");
			Debug.Assert(subsetEndpoints.All(x => x.All(y => y < 1 << 6)) , "Mode 1 should have 6bit endpoints");
			Debug.Assert(pBits.Length == 2, "Mode 1 should have 2 pBits");
			Debug.Assert(indices.Length == 16, "Provide 16 indices");
			Debug.Assert(indices.All(x => x < 1 << 3) , "Mode 1 should have 3bit indices");

			lowBits = 2; // Set Mode 1
			highBits = 0;

			lowBits = ByteHelper.Store6(lowBits, 2, (byte) partitionIndex6Bit);

			var nextIdx = 8;
			//Store endpoints
			for (var i = 0; i < subsetEndpoints[0].Length; i++) {
				for (var j = 0; j < subsetEndpoints.Length; j++) {
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, nextIdx, 6, subsetEndpoints[j][i]);
					nextIdx += 6;
				}
			}
			//Store pBits
			for (var i = 0; i < pBits.Length; i++) {
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, nextIdx, 1, pBits[i]);
				nextIdx++;
			}
			Debug.Assert(nextIdx == 82);

			var colorBitCount = ColorIndexBitCount;
			var indexBegin = GetIndexBegin(Bc7BlockType.Type1, colorBitCount, false);
			for (var i = 0; i < 16; i++) {
				var indexOffset = GetIndexOffset(Bc7BlockType.Type1, NumSubsets, 
					partitionIndex6Bit, colorBitCount, i);
				var indexBitCount = GetIndexBitCount(NumSubsets, 
					partitionIndex6Bit, colorBitCount, i);
				
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
					indexBegin + indexOffset, indexBitCount, indices[i]);
			}
		}

		public void PackType2(int partitionIndex6Bit, byte[][] subsetEndpoints,  byte[] indices) {
			Debug.Assert(partitionIndex6Bit < 64, "Mode 2 should have 6bit partition index");
			Debug.Assert(subsetEndpoints.Length == 6, "Mode 2 should have 6 endpoints");
			Debug.Assert(subsetEndpoints.All(x => x.Length == 3) , "Mode 2 should have RGB endpoints");
			Debug.Assert(subsetEndpoints.All(x => x.All(y => y < 1 << 5)) , "Mode 2 should have 5bit endpoints");
			Debug.Assert(indices.Length == 16, "Provide 16 indices");
			Debug.Assert(indices.All(x => x < 1 << 2) , "Mode 2 should have 2bit indices");

			lowBits = 0b100; // Set Mode 2
			highBits = 0;

			lowBits = ByteHelper.Store6(lowBits, 3, (byte) partitionIndex6Bit);

			var nextIdx = 9;
			//Store endpoints
			for (var i = 0; i < subsetEndpoints[0].Length; i++) {
				for (var j = 0; j < subsetEndpoints.Length; j++) {
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
						nextIdx, 5, subsetEndpoints[j][i]);
					nextIdx += 5;
				}
			}
			Debug.Assert(nextIdx == 99);

			var colorBitCount = ColorIndexBitCount;
			var indexBegin = GetIndexBegin(Bc7BlockType.Type2, colorBitCount, false);
			for (var i = 0; i < 16; i++) {
				var indexOffset = GetIndexOffset(Bc7BlockType.Type2, NumSubsets, 
					partitionIndex6Bit, colorBitCount, i);
				var indexBitCount = GetIndexBitCount(NumSubsets, 
					partitionIndex6Bit, colorBitCount, i);
				
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
					indexBegin + indexOffset, indexBitCount, indices[i]);
			}
		}

		public void PackType3(int partitionIndex6Bit, byte[][] subsetEndpoints, byte[] pBits, byte[] indices) {
			Debug.Assert(partitionIndex6Bit < 64, "Mode 3 should have 6bit partition index");
			Debug.Assert(subsetEndpoints.Length == 4, "Mode 3 should have 4 endpoints");
			Debug.Assert(subsetEndpoints.All(x => x.Length == 3) , "Mode 3 should have RGB endpoints");
			Debug.Assert(subsetEndpoints.All(x => x.All(y => y < 1 << 7)) , "Mode 3 should have 7bit endpoints");
			Debug.Assert(pBits.Length == 4, "Mode 3 should have 4 pBits");
			Debug.Assert(indices.Length == 16, "Provide 16 indices");
			Debug.Assert(indices.All(x => x < 1 << 2) , "Mode 3 should have 2bit indices");

			lowBits = 0b1000; // Set Mode 3
			highBits = 0;

			lowBits = ByteHelper.Store6(lowBits, 4, (byte) partitionIndex6Bit);

			var nextIdx = 10;
			//Store endpoints
			for (var i = 0; i < subsetEndpoints[0].Length; i++) {
				for (var j = 0; j < subsetEndpoints.Length; j++) {
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
						nextIdx, 7, subsetEndpoints[j][i]);
					nextIdx += 7;
				}
			}
			//Store pBits
			for (var i = 0; i < pBits.Length; i++) {
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, nextIdx, 
					1, pBits[i]);
				nextIdx++;
			}
			Debug.Assert(nextIdx == 98);

			var colorBitCount = ColorIndexBitCount;
			var indexBegin = GetIndexBegin(Bc7BlockType.Type3, colorBitCount, false);
			for (var i = 0; i < 16; i++) {
				var indexOffset = GetIndexOffset(Bc7BlockType.Type3, NumSubsets, 
					partitionIndex6Bit, colorBitCount, i);
				var indexBitCount = GetIndexBitCount(NumSubsets, 
					partitionIndex6Bit, colorBitCount, i);
				
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
					indexBegin + indexOffset, indexBitCount, indices[i]);
			}
		}

		public void PackType4(int rotation, byte idxMode, byte[][] colorEndPoints, byte[] alphaEndPoints, byte[] indices2Bit, byte[] indices3Bit) {
			Debug.Assert(rotation < 4, "Rotation can only be 0-3");
			Debug.Assert(idxMode < 2, "IndexMode can only be 0 or 1");
			Debug.Assert(colorEndPoints.Length == 2, "Mode 4 should have 2 endpoints");
			Debug.Assert(colorEndPoints.All(x => x.Length == 3) , "Mode 4 should have RGB color endpoints");
			Debug.Assert(colorEndPoints.All(x => x.All(y => y < 1 << 5)) , "Mode 4 should have 5bit color endpoints");
			Debug.Assert(alphaEndPoints.Length == 2, "Mode 4 should have 2 endpoints");
			Debug.Assert(alphaEndPoints.All(x => x < 1 << 6) , "Mode 4 should have 6bit alpha endpoints");

			Debug.Assert(indices2Bit.Length == 16, "Provide 16 indices");
			Debug.Assert(indices2Bit.All(x => x < 1 << 2) , "Mode 4 should have 2bit indices");
			Debug.Assert(indices3Bit.Length == 16, "Provide 16 indices");
			Debug.Assert(indices3Bit.All(x => x < 1 << 3) , "Mode 4 should have 3bit indices");

			lowBits = 0b10000; // Set Mode 4
			highBits = 0;

			lowBits = ByteHelper.Store2(lowBits, 5, (byte) rotation);
			lowBits = ByteHelper.Store1(lowBits, 7, (byte) idxMode);

			var nextIdx = 8;
			//Store color endpoints
			for (var i = 0; i < colorEndPoints[0].Length; i++) {
				for (var j = 0; j < colorEndPoints.Length; j++) {
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
						nextIdx, 5, colorEndPoints[j][i]);
					nextIdx += 5;
				}
			}
			//Store alpha endpoints
			for (var i = 0; i < alphaEndPoints.Length; i++) {
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, nextIdx, 
					6, alphaEndPoints[i]);
				nextIdx+=6;
			}
			Debug.Assert(nextIdx == 50);

			var colorBitCount = ColorIndexBitCount;
			var colorIndexBegin = GetIndexBegin(Bc7BlockType.Type4, colorBitCount, false);
			for (var i = 0; i < 16; i++) {
				var indexOffset = GetIndexOffset(Bc7BlockType.Type4, NumSubsets, 
					0, colorBitCount, i);
				var indexBitCount = GetIndexBitCount(NumSubsets, 
					0, colorBitCount, i);

				if (idxMode == 0) {
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
						colorIndexBegin + indexOffset, indexBitCount, indices2Bit[i]);
				}
				else {
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
						colorIndexBegin + indexOffset, indexBitCount, indices3Bit[i]);
				}
			}

			var alphaBitCount = AlphaIndexBitCount;
			var alphaIndexBegin = GetIndexBegin(Bc7BlockType.Type4, alphaBitCount, true);
			for (var i = 0; i < 16; i++) {
				var indexOffset = GetIndexOffset(Bc7BlockType.Type4, NumSubsets, 
					0, alphaBitCount, i);
				var indexBitCount = GetIndexBitCount(NumSubsets, 
					0, alphaBitCount, i);

				if (idxMode == 0) {
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
						alphaIndexBegin + indexOffset, indexBitCount, indices3Bit[i]);
				}
				else {
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
						alphaIndexBegin + indexOffset, indexBitCount, indices2Bit[i]);
				}
			}
		}

		public void PackType5(int rotation, byte[][] colorEndPoints, byte[] alphaEndPoints, byte[] colorIndices, byte[] alphaIndices) {
			Debug.Assert(rotation < 4, "Rotation can only be 0-3");
			Debug.Assert(colorEndPoints.Length == 2, "Mode 5 should have 2 endpoints");
			Debug.Assert(colorEndPoints.All(x => x.Length == 3) , "Mode 5 should have RGB color endpoints");
			Debug.Assert(colorEndPoints.All(x => x.All(y => y < 1 << 7)) , "Mode 5 should have 7bit color endpoints");
			Debug.Assert(alphaEndPoints.Length == 2, "Mode 5 should have 2 endpoints");

			Debug.Assert(colorIndices.Length == 16, "Provide 16 indices");
			Debug.Assert(colorIndices.All(x => x < 1 << 2) , "Mode 5 should have 2bit color indices");
			Debug.Assert(alphaIndices.Length == 16, "Provide 16 indices");
			Debug.Assert(alphaIndices.All(x => x < 1 << 2) , "Mode 5 should have 2bit alpha indices");

			lowBits = 0b100000; // Set Mode 5
			highBits = 0;

			lowBits = ByteHelper.Store2(lowBits, 6, (byte) rotation);

			var nextIdx = 8;
			//Store color endpoints
			for (var i = 0; i < colorEndPoints[0].Length; i++) {
				for (var j = 0; j < colorEndPoints.Length; j++) {
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
						nextIdx, 7, colorEndPoints[j][i]);
					nextIdx += 7;
				}
			}
			//Store alpha endpoints
			for (var i = 0; i < alphaEndPoints.Length; i++) {
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, nextIdx, 
					8, alphaEndPoints[i]);
				nextIdx+=8;
			}
			Debug.Assert(nextIdx == 66);

			var colorBitCount = ColorIndexBitCount;
			var colorIndexBegin = GetIndexBegin(Bc7BlockType.Type5, colorBitCount, false);
			for (var i = 0; i < 16; i++) {
				var indexOffset = GetIndexOffset(Bc7BlockType.Type5, NumSubsets, 
					0, colorBitCount, i);
				var indexBitCount = GetIndexBitCount(NumSubsets, 
					0, colorBitCount, i);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
						colorIndexBegin + indexOffset, indexBitCount, colorIndices[i]);
				
			}

			var alphaBitCount = AlphaIndexBitCount;
			var alphaIndexBegin = GetIndexBegin(Bc7BlockType.Type5, alphaBitCount, true);
			for (var i = 0; i < 16; i++) {
				var indexOffset = GetIndexOffset(Bc7BlockType.Type5, NumSubsets, 
					0, alphaBitCount, i);
				var indexBitCount = GetIndexBitCount(NumSubsets, 
					0, alphaBitCount, i);

				
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
					alphaIndexBegin + indexOffset, indexBitCount, alphaIndices[i]);

			}
		}

		public void PackType6(byte[][] colorAlphaEndPoints, byte[] pBits, byte[] indices) {
			Debug.Assert(colorAlphaEndPoints.Length == 2, 
				"Mode 6 should have 2 endpoints");
			Debug.Assert(colorAlphaEndPoints.All(x => x.Length == 4) , 
				"Mode 6 should have RGBA color endpoints");
			Debug.Assert(colorAlphaEndPoints.All(x => x.All(y => y < 1 << 7)) , 
				"Mode 6 should have 7bit color and alpha endpoints");
			Debug.Assert(pBits.Length == 2, "Mode 6 should have 2 pBits");
			Debug.Assert(indices.Length == 16, "Provide 16 indices");
			Debug.Assert(indices.All(x => x < 1 << 4) , "Mode 6 should have 4bit color indices");
			
			lowBits = 0b1000000; // Set Mode 6
			highBits = 0;

			var nextIdx = 7;
			//Store color and alpha endpoints
			for (var i = 0; i < colorAlphaEndPoints[0].Length; i++) {
				for (var j = 0; j < colorAlphaEndPoints.Length; j++) {
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
						nextIdx, 7, colorAlphaEndPoints[j][i]);
					nextIdx += 7;
				}
			}
			//Store pBits
			for (var i = 0; i < pBits.Length; i++) {
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, nextIdx, 
					1, pBits[i]);
				nextIdx++;
			}
			Debug.Assert(nextIdx == 65);

			var colorBitCount = ColorIndexBitCount;
			var colorIndexBegin = GetIndexBegin(Bc7BlockType.Type6, colorBitCount, false);
			for (var i = 0; i < 16; i++) {
				var indexOffset = GetIndexOffset(Bc7BlockType.Type6, NumSubsets, 
					0, colorBitCount, i);
				var indexBitCount = GetIndexBitCount(NumSubsets, 
					0, colorBitCount, i);

					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
						colorIndexBegin + indexOffset, indexBitCount, indices[i]);
				
			}
		}

		public void PackType7(int partitionIndex6Bit, byte[][] subsetEndpoints, byte[] pBits, byte[] indices) {
			Debug.Assert(partitionIndex6Bit < 64, "Mode 7 should have 6bit partition index");
			Debug.Assert(subsetEndpoints.Length == 4, "Mode 7 should have 4 endpoints");
			Debug.Assert(subsetEndpoints.All(x => x.Length == 4) , "Mode 7 should have RGBA endpoints");
			Debug.Assert(subsetEndpoints.All(x => x.All(y => y < 1 << 5)) , "Mode 7 should have 5bit endpoints");
			Debug.Assert(pBits.Length == 4, "Mode 7 should have 4 pBits");
			Debug.Assert(indices.Length == 16, "Provide 16 indices");
			Debug.Assert(indices.All(x => x < 1 << 2) , "Mode 3 should have 2bit indices");

			lowBits = 0b10000000; // Set Mode 7
			highBits = 0;

			lowBits = ByteHelper.Store6(lowBits, 8, (byte) partitionIndex6Bit);

			var nextIdx = 14;
			//Store endpoints
			for (var i = 0; i < subsetEndpoints[0].Length; i++) {
				for (var j = 0; j < subsetEndpoints.Length; j++) {
					(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
						nextIdx, 5, subsetEndpoints[j][i]);
					nextIdx += 5;
				}
			}
			//Store pBits
			for (var i = 0; i < pBits.Length; i++) {
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, nextIdx, 
					1, pBits[i]);
				nextIdx++;
			}
			Debug.Assert(nextIdx == 98);

			var colorBitCount = ColorIndexBitCount;
			var indexBegin = GetIndexBegin(Bc7BlockType.Type7, colorBitCount, false);
			for (var i = 0; i < 16; i++) {
				var indexOffset = GetIndexOffset(Bc7BlockType.Type7, NumSubsets, 
					partitionIndex6Bit, colorBitCount, i);
				var indexBitCount = GetIndexBitCount(NumSubsets, 
					partitionIndex6Bit, colorBitCount, i);
				
				(lowBits, highBits) = ByteHelper.StoreTo128(lowBits, highBits, 
					indexBegin + indexOffset, indexBitCount, indices[i]);
			}
		}
	}

}
