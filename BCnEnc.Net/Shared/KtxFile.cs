using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Text.Encoding;

namespace BCnComp.Net.Shared
{

	public class KtxFile
	{

		public KtxHeader Header;
		public List<KtxKeyValuePair> KeyValuePairs { get; } = new List<KtxKeyValuePair>();
		public List<KtxMipmap> MipMaps { get; } = new List<KtxMipmap>();

		public KtxFile() { }
		public KtxFile(KtxHeader header)
		{
			Header = header;
		}

		public void Write(Stream s)
		{
			if (MipMaps.Count < 1 || MipMaps[0].NumberOfFaces < 1)
			{
				throw new InvalidOperationException("The KTX structure should have at least 1 mipmap level and 1 Face before writing to file.");
			}

			using (BinaryWriter bw = new BinaryWriter(s, UTF8, true))
			{
				uint bytesOfKeyValueData = (uint)KeyValuePairs.Sum(x => x.GetSizeWithPadding());

				Header.BytesOfKeyValueData = bytesOfKeyValueData;
				Header.NumberOfFaces = MipMaps[0].NumberOfFaces;
				Header.NumberOfMipmapLevels = (uint)MipMaps.Count;
				Header.NumberOfArrayElements = 0;

				if (!Header.VerifyHeader())
				{
					throw new InvalidOperationException("Please verify the header validity before writing to file.");
				}

				bw.WriteStruct(Header);

				foreach (KtxKeyValuePair keyValuePair in KeyValuePairs)
				{
					KtxKeyValuePair.WriteKeyValuePair(bw, keyValuePair);
				}

				for (int mip = 0; mip < Header.NumberOfMipmapLevels; mip++)
				{
					uint imageSize = MipMaps[mip].SizeInBytes;
					bw.Write(imageSize);
					bool isCubemap = Header.NumberOfFaces == 6 && Header.NumberOfArrayElements == 0;
					for (int f = 0; f < Header.NumberOfFaces; f++)
					{
						bw.Write(MipMaps[mip].Faces[f].Data);
						uint cubePadding = 0u;
						if (isCubemap)
						{
							cubePadding = 3 - ((imageSize + 3) % 4);
						}
						bw.AddPadding(cubePadding);
					}

					uint mipPaddingBytes = 3 - ((imageSize + 3) % 4);
					bw.AddPadding(mipPaddingBytes);
				}

			}
		}

		public static KtxFile Load(Stream s)
		{

			using (BinaryReader br = new BinaryReader(s, UTF8, true))
			{
				KtxHeader header = br.ReadStruct<KtxHeader>();

				if (header.NumberOfArrayElements > 0)
				{
					throw new NotSupportedException("KTX files with arrays are not supported.");
				}

				KtxFile ktx = new KtxFile(header);

				int keyValuePairBytesRead = 0;
				while (keyValuePairBytesRead < header.BytesOfKeyValueData)
				{
					KtxKeyValuePair kvp = KtxKeyValuePair.ReadKeyValuePair(br, out int read);
					keyValuePairBytesRead += read;
					ktx.KeyValuePairs.Add(kvp);
				}

				uint numberOfFaces = Math.Max(1, header.NumberOfFaces);
				ktx.MipMaps.Capacity = (int)header.NumberOfMipmapLevels;
				for (uint mipLevel = 0; mipLevel < header.NumberOfMipmapLevels; mipLevel++)
				{
					uint imageSize = br.ReadUInt32();
					uint mipWidth = header.PixelWidth / (uint)(Math.Pow(2, mipLevel));
					uint mipHeight = header.PixelHeight / (uint)(Math.Pow(2, mipLevel));

					ktx.MipMaps.Add(new KtxMipmap(imageSize, mipWidth, mipHeight, numberOfFaces));

					bool cubemap = header.NumberOfFaces == 6 && header.NumberOfArrayElements == 0;
					for (uint face = 0; face < numberOfFaces; face++)
					{
						byte[] faceData = br.ReadBytes((int)imageSize);
						ktx.MipMaps[(int)mipLevel].Faces[(int)face] = new KtxMipFace(faceData, mipWidth, mipHeight);
						uint cubePadding = 0u;
						if (cubemap)
						{
							cubePadding = 3 - ((imageSize + 3) % 4);
						}
						br.SkipPadding(cubePadding);
					}

					uint mipPaddingBytes = 3 - ((imageSize + 3) % 4);
					br.SkipPadding(mipPaddingBytes);
				}

				return ktx;
			}
		}

		public ulong GetTotalSize()
		{
			ulong totalSize = 0;

			for (int mipLevel = 0; mipLevel < Header.NumberOfMipmapLevels; mipLevel++)
			{
				for (int face = 0; face < Header.NumberOfFaces; face++)
				{
					KtxMipFace ktxface = MipMaps[mipLevel].Faces[face];
					totalSize += ktxface.SizeInBytes;
				}
			}

			return totalSize;
		}

		/// <summary>
		/// Gets all texture data of the file in face-major order (face0_mip0 ... face0_mip1 ... face1_mip0 ...)
		/// </summary>
		/// <returns></returns>
		public byte[] GetAllTextureDataFaceMajor()
		{
			byte[] result = new byte[GetTotalSize()];
			uint start = 0;
			for (int face = 0; face < Header.NumberOfFaces; face++)
			{
				for (int mipLevel = 0; mipLevel < Header.NumberOfMipmapLevels; mipLevel++)
				{
					KtxMipFace ktxMipFace = MipMaps[mipLevel].Faces[face];
					ktxMipFace.Data.CopyTo(result, (int)start);
					start += ktxMipFace.SizeInBytes;
				}
			}

			return result;
		}

		/// <summary>
		/// Gets all texture data of the file in MipMap-major order (face0_mip0 ... face1_mip0 ... face0_mip1 ...)
		/// </summary>
		/// <returns></returns>
		public byte[] GetAllTextureDataMipMajor()
		{
			byte[] result = new byte[GetTotalSize()];
			uint start = 0;
			for (int mipLevel = 0; mipLevel < Header.NumberOfMipmapLevels; mipLevel++)
			{
				for (int face = 0; face < Header.NumberOfFaces; face++)
				{
					KtxMipFace ktxMipFace = MipMaps[mipLevel].Faces[face];
					ktxMipFace.Data.CopyTo(result, (int)start);
					start += ktxMipFace.SizeInBytes;
				}
			}

			return result;
		}
	}

	public class KtxKeyValuePair
	{
		public string Key { get; }
		public byte[] Value { get; }
		public KtxKeyValuePair(string key, byte[] value)
		{
			Key = key;
			Value = value;
		}

		public uint GetSizeWithPadding()
		{
			int keySpanLength = UTF8.GetByteCount(Key);
			uint totalSize = (uint)(keySpanLength + 1 + Value.Length);
			int paddingBytes = (int)(3 - ((totalSize + 3) % 4));

			return (uint)(totalSize + paddingBytes);
		}

		public static KtxKeyValuePair ReadKeyValuePair(BinaryReader br, out int bytesRead)
		{
			uint totalSize = br.ReadUInt32();
			Span<byte> keyValueBytes = stackalloc byte[(int)totalSize];
			br.Read(keyValueBytes);

			// Find the key's null terminator
			int i;
			for (i = 0; i < totalSize; i++)
			{
				if (keyValueBytes[i] == 0)
				{
					break;
				}

				if (i >= totalSize)
				{
					throw new InvalidDataException();
				}
			}


			int keySize = i;
			string key = UTF8.GetString(keyValueBytes.Slice(0, keySize));

			int valueSize = (int)(totalSize - keySize - 1);
			Span<byte> valueBytes = keyValueBytes.Slice(i + 1, valueSize);
			byte[] value = new byte[valueSize];
			valueBytes.CopyTo(value);

			int paddingBytes = (int)(3 - ((totalSize + 3) % 4));
			br.SkipPadding(paddingBytes);

			bytesRead = (int)(totalSize + paddingBytes + sizeof(uint));
			return new KtxKeyValuePair(key, value);
		}

		public static uint WriteKeyValuePair(BinaryWriter bw, KtxKeyValuePair pair)
		{
			int keySpanLength = UTF8.GetByteCount(pair.Key);
			Span<byte> keySpan = stackalloc byte[keySpanLength];
			Span<byte> valueSpan = pair.Value;

			uint totalSize = (uint)(keySpan.Length + 1 + valueSpan.Length);
			int paddingBytes = (int)(3 - ((totalSize + 3) % 4));

			bw.Write(totalSize);
			bw.Write(keySpan);
			bw.Write((byte)0);
			bw.Write(valueSpan);

			return (uint)(totalSize + paddingBytes);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct KtxHeader
	{
		public fixed byte Identifier[12];
		public uint Endianness;
		public GLType GlType;
		public uint GlTypeSize;
		public GLFormat GlFormat;
		public GlInternalFormat GlInternalFormat;
		public GLFormat GlBaseInternalFormat;
		public uint PixelWidth;
		public uint PixelHeight;

		public uint PixelDepth;
		public uint NumberOfArrayElements;
		public uint NumberOfFaces;
		public uint NumberOfMipmapLevels;
		public uint BytesOfKeyValueData;

		public bool VerifyHeader()
		{
			Span<byte> id = stackalloc byte[] { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };
			for (int i = 0; i < id.Length; i++)
			{
				if (Identifier[i] != id[i]) return false;
			}
			return true;
		}

		public static KtxHeader InitializeCompressed(int width, int height, GlInternalFormat internalFormat, GLFormat baseInternalFormat)
		{
			KtxHeader header = new KtxHeader();
			Span<byte> id = stackalloc byte[] { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };
			for (int i = 0; i < id.Length; i++)
			{
				header.Identifier[i] = id[i];
			}
			header.Endianness = 0x04030201;
			header.PixelWidth = (uint)width;
			header.PixelHeight = (uint)height;
			header.GlType = 0;
			header.GlTypeSize = 1;
			header.GlFormat = 0;
			header.GlInternalFormat = internalFormat;
			header.GlBaseInternalFormat = baseInternalFormat;

			return header;
		}

		public static KtxHeader InitializeUncompressed(int width, int height, GLType type, GLFormat format, uint glTypeSize, GlInternalFormat internalFormat, GLFormat baseInternalFormat)
		{
			KtxHeader header = new KtxHeader();
			Span<byte> id = stackalloc byte[] { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };
			for (int i = 0; i < id.Length; i++)
			{
				header.Identifier[i] = id[i];
			}
			header.Endianness = 0x04030201;
			header.PixelWidth = (uint)width;
			header.PixelHeight = (uint)height;
			header.GlType = type;
			header.GlTypeSize = glTypeSize;
			header.GlFormat = format;
			header.GlInternalFormat = internalFormat;
			header.GlBaseInternalFormat = baseInternalFormat;

			return header;
		}
	}

	public class KtxMipFace
	{
		public uint Width { get; set; }
		public uint Height { get; set; }
		public uint SizeInBytes { get; }
		public byte[] Data { get; }

		public KtxMipFace(byte[] data, uint width, uint height)
		{
			Width = width;
			Height = height;
			SizeInBytes = (uint)data.Length;
			Data = data;
		}
	}

	public class KtxMipmap
	{
		public uint SizeInBytes { get; }
		public uint Width { get; }
		public uint Height { get; }
		public uint NumberOfFaces { get; }
		public KtxMipFace[] Faces { get; }
		public KtxMipmap(uint sizeInBytes, uint width, uint height, uint numberOfFaces)
		{
			SizeInBytes = sizeInBytes;
			Width = Math.Max(1, width);
			Height = Math.Max(1, height);
			NumberOfFaces = numberOfFaces;
			Faces = new KtxMipFace[numberOfFaces];
		}
	}
}
