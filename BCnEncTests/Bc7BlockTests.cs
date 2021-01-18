using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BCnEncoder.Shared;
using Xunit;

namespace BCnEncTests
{
	public class Bc7BlockTests
	{
		[Fact]
		public void PackTypes()
		{
			var numWidthBlocks = 4;
			var numHeightBlocks = 2;
			var outputBlocks = new Bc7Block[64 * numWidthBlocks * numHeightBlocks];
			var encoded = new byte[64 * numWidthBlocks * numHeightBlocks * Unsafe.SizeOf<Bc7Block>()];

			var output = new KtxFile(
				KtxHeader.InitializeCompressed(numWidthBlocks * 8 * 4, numHeightBlocks * 8 * 4,
					GlInternalFormat.GlCompressedRgbaBptcUnormArb,
					GlFormat.GlRgba));

			var type0 = new Span<Bc7Block>(outputBlocks, 0, 64);
			Type0Pack(type0);
			PlaceBlock(0, 0, type0, MemoryMarshal.Cast<byte, Bc7Block>(new Span<byte>(encoded)), numWidthBlocks);

			var type1 = new Span<Bc7Block>(outputBlocks, 64 * 1, 64);
			Type1Pack(type1);
			PlaceBlock(1, 0, type1, MemoryMarshal.Cast<byte, Bc7Block>(new Span<byte>(encoded)), numWidthBlocks);

			var type2 = new Span<Bc7Block>(outputBlocks, 64 * 2, 64);
			Type2Pack(type2);
			PlaceBlock(2, 0, type2, MemoryMarshal.Cast<byte, Bc7Block>(new Span<byte>(encoded)), numWidthBlocks);

			var type3 = new Span<Bc7Block>(outputBlocks, 64 * 3, 64);
			Type3Pack(type3);
			PlaceBlock(3, 0, type3, MemoryMarshal.Cast<byte, Bc7Block>(new Span<byte>(encoded)), numWidthBlocks);

			var type4 = new Span<Bc7Block>(outputBlocks, 64 * 4, 64);
			Type4Pack(type4);
			PlaceBlock(0, 1, type4, MemoryMarshal.Cast<byte, Bc7Block>(new Span<byte>(encoded)), numWidthBlocks);

			var type5 = new Span<Bc7Block>(outputBlocks, 64 * 5, 64);
			Type5Pack(type5);
			PlaceBlock(1, 1, type5, MemoryMarshal.Cast<byte, Bc7Block>(new Span<byte>(encoded)), numWidthBlocks);

			var type6 = new Span<Bc7Block>(outputBlocks, 64 * 6, 64);
			Type6Pack(type6);
			PlaceBlock(2, 1, type6, MemoryMarshal.Cast<byte, Bc7Block>(new Span<byte>(encoded)), numWidthBlocks);

			var type7 = new Span<Bc7Block>(outputBlocks, 64 * 7, 64);
			Type7Pack(type7);
			PlaceBlock(3, 1, type7, MemoryMarshal.Cast<byte, Bc7Block>(new Span<byte>(encoded)), numWidthBlocks);

			output.MipMaps.Add(new KtxMipmap((uint)encoded.Length,
				(uint)(8 * 4 * numWidthBlocks),
				(uint)(8 * 4 * numHeightBlocks), 1));
			output.MipMaps[0].Faces[0] = new KtxMipFace(encoded,
				(uint)(8 * 4 * numWidthBlocks),
				(uint)(8 * 4 * numHeightBlocks));

			var fs = File.OpenWrite("bc7_blocktests.ktx");
			output.Write(fs);
		}

		#region Type Packs

		private void Type0Pack(Span<Bc7Block> output)
		{
			var subsetEndpoints = new[] {
				//subset 1
				new byte[]{0b1111, 0, 0},
				new byte[]{0b1000, 0, 0},
				// subset 2
				new byte[]{0, 0b1111, 0},
				new byte[]{0, 0b1000, 0},
				//subset 3
				new byte[]{0, 0, 0b1111},
				new byte[]{0, 0, 0b1000}
			};
			var pBits = new byte[] {
				1, 0, 1, 0, 1, 1
			};
			var indices = new byte[] {
				0, 1, 2, 3,
				0, 1, 2, 3,
				3, 2, 1, 0,
				3, 2, 1, 0
			};
			for (var i = 0; i < 16; i++)
			{
				output[i].PackType0(i, subsetEndpoints, pBits, indices);
				Assert.Equal(Bc7BlockType.Type0, output[i].Type);
				Assert.Equal(i, output[i].PartitionSetId);
			}

			pBits = new byte[] {
				0, 0, 0, 0, 0, 0
			};
			indices = new byte[] {
				0, 1, 0, 3,
				1, 1, 1, 3,
				2, 2, 2, 0,
				3, 2, 3, 0
			};

			for (var i = 0; i < 16; i++)
			{
				output[i + 16].PackType0(i, subsetEndpoints, pBits, indices);
				Assert.Equal(Bc7BlockType.Type0, output[i].Type);
				Assert.Equal(i, output[i].PartitionSetId);
			}

			pBits = new byte[] {
				1, 1, 1, 1, 1, 1
			};
			indices = new byte[] {
				1, 0, 3, 3,
				2, 1, 2, 3,
				3, 2, 1, 0,
				0, 3, 0, 0
			};

			for (var i = 0; i < 16; i++)
			{
				output[i + 32].PackType0(i, subsetEndpoints, pBits, indices);
				Assert.Equal(Bc7BlockType.Type0, output[i].Type);
				Assert.Equal(i, output[i].PartitionSetId);
			}

			pBits = new byte[] {
				0, 1, 0, 1, 0, 0
			};
			indices = new byte[] {
				3, 2, 1, 0,
				0, 1, 2, 3,
				3, 2, 1, 0,
				0, 1, 2, 3
			};

			for (var i = 0; i < 16; i++)
			{
				output[i + 48].PackType0(i, subsetEndpoints, pBits, indices);
				Assert.Equal(Bc7BlockType.Type0, output[i].Type);
				Assert.Equal(i, output[i].PartitionSetId);
			}
		}

		private void Type1Pack(Span<Bc7Block> output)
		{
			var subsetEndpoints = new[] {
				//subset 1
				new byte[]{0b111111, 0b0100, 0},
				new byte[]{0b0100, 0b111111, 0},
				// subset 2
				new byte[]{0, 0, 0b111111},
				new byte[]{0, 0, 0b1010}
			};
			var pBits = new byte[] {
				1, 1
			};
			var indices = new byte[] {
				0, 1, 2, 3,
				0, 1, 2, 3,
				3, 2, 1, 0,
				3, 2, 1, 0
			};
			for (var i = 0; i < 64; i++)
			{
				output[i].PackType1(i, subsetEndpoints, pBits, indices);
				Assert.Equal(Bc7BlockType.Type1, output[i].Type);
				Assert.Equal(i, output[i].PartitionSetId);
			}
		}

		private void Type2Pack(Span<Bc7Block> output)
		{
			var subsetEndpoints = new[] {
				//subset 1
				new byte[]{0b11111, 0, 0},
				new byte[]{0b01000, 0, 0},
				// subset 2
				new byte[]{0, 0b11111, 0},
				new byte[]{0, 0b01000, 0},
				//subset 3
				new byte[]{0, 0, 0b11111},
				new byte[]{0, 0, 0b01000}
			};
			var indices = new byte[] {
				0, 1, 2, 3,
				0, 1, 2, 3,
				3, 2, 1, 0,
				3, 2, 1, 0
			};
			for (var i = 0; i < 64; i++)
			{
				output[i].PackType2(i, subsetEndpoints, indices);
				Assert.Equal(Bc7BlockType.Type2, output[i].Type);
				Assert.Equal(i, output[i].PartitionSetId);
			}
		}

		private void Type3Pack(Span<Bc7Block> output)
		{
			var subsetEndpoints = new[] {
				//subset 1
				new byte[]{0b1111111, 0b10100, 0},
				new byte[]{0b10100, 0b1111111, 0},
				// subset 2
				new byte[]{0, 0, 0b1111111},
				new byte[]{0, 0, 0b11010}
			};
			var pBits = new byte[] {
				1, 0, 0, 1
			};
			var indices = new byte[] {
				0, 1, 2, 3,
				0, 1, 2, 3,
				3, 2, 1, 0,
				3, 2, 1, 0
			};
			for (var i = 0; i < 64; i++)
			{
				output[i].PackType3(i, subsetEndpoints, pBits, indices);
				Assert.Equal(Bc7BlockType.Type3, output[i].Type);
				Assert.Equal(i, output[i].PartitionSetId);
			}
		}

		private void Type4Pack(Span<Bc7Block> output)
		{
			var colorEndpoints = new[] {
				//subset 1
				new byte[]{0b11111, 0, 0b0100},
				new byte[]{0, 0b11111, 0b0100}
			};
			var alphaEndPoints = new byte[]{
				0b111111,
				0
			};
			var indices2Bit = new byte[] {
				0, 1, 2, 3,
				0, 1, 2, 3,
				3, 2, 1, 0,
				3, 2, 1, 0
			};

			var indices3Bit = new byte[] {
				0, 1, 2, 3,
				7, 6, 5, 4,
				7, 6, 5, 4,
				3, 2, 1, 0
			};
			var rotation = 0;
			byte idxMode = 0;
			for (var i = 0; i < 64; i++)
			{
				rotation = (rotation + 1) % 4;
				idxMode = (byte)((idxMode + 1) % 2);
				output[i].PackType4(rotation, idxMode, colorEndpoints, alphaEndPoints, indices2Bit, indices3Bit);
				Assert.Equal(Bc7BlockType.Type4, output[i].Type);
				Assert.Equal(rotation, output[i].RotationBits);
				Assert.Equal(idxMode, output[i].Type4IndexMode);
			}
		}

		private void Type5Pack(Span<Bc7Block> output)
		{
			var colorEndpoints = new[] {
				//subset 1
				new byte[]{0b1111111,  0b0100, 0},
				new byte[]{0, 0, 0b1111111}
			};
			var alphaEndPoints = new byte[]{
				255,
				100
			};
			var colorIndices = new byte[] {
				0, 1, 2, 3,
				0, 1, 2, 3,
				3, 2, 1, 0,
				3, 2, 1, 0
			};
			var alphaIndices = new byte[] {
				1, 2, 3, 0,
				0, 1, 2, 3,
				2, 3, 0, 1,
				3, 2, 1, 0
			};
			var rotation = 0;
			for (var i = 0; i < 64; i++)
			{
				rotation = (rotation + 1) % 4;
				output[i].PackType5(rotation, colorEndpoints, alphaEndPoints, colorIndices, alphaIndices);
				Assert.Equal(Bc7BlockType.Type5, output[i].Type);
				Assert.Equal(rotation, output[i].RotationBits);
			}
		}

		private void Type6Pack(Span<Bc7Block> output)
		{
			var colorEndpoints = new[] {
				//subset 1
				new byte[]{0b1111111,  0b0100, 0, 0b1111111},
				new byte[]{0, 0, 0b1111111, 0b1111}
			};
			var pBits = new byte[] {
				1, 0
			};
			var colorIndices = new byte[] {
				0, 1, 2, 3,
				4, 5, 6, 7,
				8, 9, 10, 11,
				12, 13, 14, 15
			};
			for (var i = 0; i < 64; i++)
			{
				output[i].PackType6(colorEndpoints, pBits, colorIndices);
				Assert.Equal(Bc7BlockType.Type6, output[i].Type);
			}
		}

		private void Type7Pack(Span<Bc7Block> output)
		{
			var colorEndpoints = new[] {
				//subset 1
				new byte[]{0b11111,  0b0100, 0, 0b11111},
				new byte[]{0, 0b11111, 0, 0b111},
				//subset 2
				new byte[]{0b11111,  0b0100, 0, 0b1111},
				new byte[]{0b10100, 0, 0b11111, 0b11111}
			};
			var pBits = new byte[] {
				1, 0, 1, 0
			};
			var indices = new byte[] {
				0, 1, 2, 3,
				0, 1, 2, 3,
				3, 2, 1, 0,
				3, 2, 1, 0
			};
			for (var i = 0; i < 64; i++)
			{
				output[i].PackType7(i, colorEndpoints, pBits, indices);
				Assert.Equal(Bc7BlockType.Type7, output[i].Type);
				Assert.Equal(i, output[i].PartitionSetId);
			}
		}

		private void PlaceBlock(int x, int y, Span<Bc7Block> data, Span<Bc7Block> destination, int destBlockWidth)
		{
			for (var i = 0; i < 8; i++)
			{
				var xStart = x * 8 + (y * 8 + i) * 8 * destBlockWidth;
				var row = data.Slice(i * 8, 8);
				row.CopyTo(destination.Slice(xStart, 8));
			}
		}

		#endregion
	}
}
