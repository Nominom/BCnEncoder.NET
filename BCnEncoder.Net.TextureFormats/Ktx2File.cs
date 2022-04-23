using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncoder.TextureFormats
{
	public class Ktx2File : ITextureFileFormat<Ktx2File>
	{
		/// <inheritdoc />
		public bool SupportsLdr { get; }

		/// <inheritdoc />
		public bool SupportsHdr { get; }

		/// <inheritdoc />
		public bool SupportsCubeMap { get; }

		/// <inheritdoc />
		public bool SupportsMipMaps { get; }

		/// <inheritdoc />
		public bool IsSupportedFormat(CompressionFormat format)
		{
			return false;
		}

		/// <inheritdoc />
		public void FromTextureData(BCnTextureData textureData)
		{
		}

		/// <inheritdoc />
		public BCnTextureData ToTextureData()
		{
			return null;
		}

		/// <inheritdoc />
		public void ReadFromStream(Stream inputStream)
		{
		}

		/// <inheritdoc />
		public void WriteToStream(Stream outputStream)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Ktx2Header
	{
		public fixed byte Identifier[12];
		public VkFormat VkFormat;
		public uint TypeSize;
		public uint PixelWidth;
		public uint PixelHeight;
		public uint PixelDepth;
		public uint LayerCount;
		public uint FaceCount;
		public uint LevelCount;
		public SuperCompressionScheme SuperCompressionScheme;

		public bool VerifyHeader()
		{
			Span<byte> id = stackalloc byte[] { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x32, 0x30, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };
			for (var i = 0; i < id.Length; i++)
			{
				if (Identifier[i] != id[i]) return false;
			}
			return true;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Ktx2Index
	{
		public uint DfdByteOffset; // Data format descriptor
		public uint DfdByteLength;
		public uint KvdByteOffset; // Key value data
		public uint KvdByteLength;
		public ulong SgdByteOffset; // SuperCompressionGlobalData
		public ulong SgdByteLength;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Ktx2LevelIndex // For each mip level
	{
		public ulong ByteOffset;
		public ulong ByteLength;
		public ulong UncompressedByteLength;
	}

	public enum SuperCompressionScheme : uint
	{
		None = 0,
		BasisLz = 1,
		ZStandard = 2,
		ZLib = 3
	}
}
