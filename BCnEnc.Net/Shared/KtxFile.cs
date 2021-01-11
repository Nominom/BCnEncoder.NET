using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static System.Text.Encoding;

namespace BCnEncoder.Shared
{
	/// <summary>
	/// A representation of a ktx file.
	/// This class handles loading and saving ktx files into streams.
	/// The full spec can be found here: https://www.khronos.org/opengles/sdk/tools/KTX/file_format_spec/
	/// </summary>
	public class KtxFile
	{

		public KtxHeader header;
		public List<KtxKeyValuePair> KeyValuePairs { get; } = new List<KtxKeyValuePair>();
		public List<KtxMipmap> MipMaps { get; } = new List<KtxMipmap>();

		public KtxFile() { }
		public KtxFile(KtxHeader header)
		{
			this.header = header;
		}

		/// <summary>
		/// Writes this ktx file into a stream.
		/// </summary>
		public void Write(Stream s)
		{
			if (MipMaps.Count < 1 || MipMaps[0].NumberOfFaces < 1)
			{
				throw new InvalidOperationException("The KTX structure should have at least 1 mipmap level and 1 Face before writing to file.");
			}

			using (var bw = new BinaryWriter(s, UTF8, true))
			{
				var bytesOfKeyValueData = (uint)KeyValuePairs.Sum(x => x.GetSizeWithPadding());

				header.BytesOfKeyValueData = bytesOfKeyValueData;
				header.NumberOfFaces = MipMaps[0].NumberOfFaces;
				header.NumberOfMipmapLevels = (uint)MipMaps.Count;
				header.NumberOfArrayElements = 0;

				if (!header.VerifyHeader())
				{
					throw new InvalidOperationException("Please verify the header validity before writing to file.");
				}

				bw.WriteStruct(header);

				foreach (var keyValuePair in KeyValuePairs)
				{
					KtxKeyValuePair.WriteKeyValuePair(bw, keyValuePair);
				}

				for (var mip = 0; mip < header.NumberOfMipmapLevels; mip++)
				{
					var imageSize = MipMaps[mip].SizeInBytes;
					bw.Write(imageSize);
					var isCubemap = header.NumberOfFaces == 6 && header.NumberOfArrayElements == 0;
					for (var f = 0; f < header.NumberOfFaces; f++)
					{
						bw.Write(MipMaps[mip].Faces[f].Data);
						var cubePadding = 0u;
						if (isCubemap)
						{
							cubePadding = 3 - (imageSize + 3) % 4;
						}
						bw.AddPadding(cubePadding);
					}

					var mipPaddingBytes = 3 - (imageSize + 3) % 4;
					bw.AddPadding(mipPaddingBytes);
				}

			}
		}

		/// <summary>
		/// Loads a KTX file from a stream.
		/// </summary>
		public static KtxFile Load(Stream s)
		{

			using (var br = new BinaryReader(s, UTF8, true))
			{
				var header = br.ReadStruct<KtxHeader>();

				if (header.NumberOfArrayElements > 0)
				{
					throw new NotSupportedException("KTX files with arrays are not supported.");
				}

				var ktx = new KtxFile(header);

				var keyValuePairBytesRead = 0;
				while (keyValuePairBytesRead < header.BytesOfKeyValueData)
				{
					var kvp = KtxKeyValuePair.ReadKeyValuePair(br, out var read);
					keyValuePairBytesRead += read;
					ktx.KeyValuePairs.Add(kvp);
				}

				var numberOfFaces = Math.Max(1, header.NumberOfFaces);
				ktx.MipMaps.Capacity = (int)header.NumberOfMipmapLevels;
				for (uint mipLevel = 0; mipLevel < header.NumberOfMipmapLevels; mipLevel++)
				{
					var imageSize = br.ReadUInt32();
					var mipWidth = header.PixelWidth / (uint)Math.Pow(2, mipLevel);
					var mipHeight = header.PixelHeight / (uint)Math.Pow(2, mipLevel);

					ktx.MipMaps.Add(new KtxMipmap(imageSize, mipWidth, mipHeight, numberOfFaces));

					var cubemap = header.NumberOfFaces > 1 && header.NumberOfArrayElements == 0;
					for (uint face = 0; face < numberOfFaces; face++)
					{
						var faceData = br.ReadBytes((int)imageSize);
						ktx.MipMaps[(int)mipLevel].Faces[(int)face] = new KtxMipFace(faceData, mipWidth, mipHeight);
						if (cubemap)
						{
							var cubePadding = 0u;
							cubePadding = 3 - (imageSize + 3) % 4;
							br.SkipPadding(cubePadding);
						}
					}

					var mipPaddingBytes = 3 - (imageSize + 3) % 4;
					br.SkipPadding(mipPaddingBytes);
				}

				return ktx;
			}
		}

		/// <summary>
		/// Gets the total size of all mipmaps and faces.
		/// </summary>
		public ulong GetTotalSize()
		{
			ulong totalSize = 0;

			for (var mipLevel = 0; mipLevel < header.NumberOfMipmapLevels; mipLevel++)
			{
				for (var face = 0; face < header.NumberOfFaces; face++)
				{
					var ktxface = MipMaps[mipLevel].Faces[face];
					totalSize += ktxface.SizeInBytes;
				}
			}

			return totalSize;
		}

		/// <summary>
		/// Gets all texture data of the file in face-major order (face0_mip0 ... face0_mip1 ... face1_mip0 ...)
		/// </summary>
		public byte[] GetAllTextureDataFaceMajor()
		{
			var result = new byte[GetTotalSize()];
			uint start = 0;
			for (var face = 0; face < header.NumberOfFaces; face++)
			{
				for (var mipLevel = 0; mipLevel < header.NumberOfMipmapLevels; mipLevel++)
				{
					var ktxMipFace = MipMaps[mipLevel].Faces[face];
					ktxMipFace.Data.CopyTo(result, (int)start);
					start += ktxMipFace.SizeInBytes;
				}
			}

			return result;
		}

		/// <summary>
		/// Gets all texture data of the file in MipMap-major order (face0_mip0 ... face1_mip0 ... face0_mip1 ...)
		/// </summary>
		public byte[] GetAllTextureDataMipMajor()
		{
			var result = new byte[GetTotalSize()];
			uint start = 0;
			for (var mipLevel = 0; mipLevel < header.NumberOfMipmapLevels; mipLevel++)
			{
				for (var face = 0; face < header.NumberOfFaces; face++)
				{
					var ktxMipFace = MipMaps[mipLevel].Faces[face];
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
			var keySpanLength = UTF8.GetByteCount(Key);
			var totalSize = (uint)(keySpanLength + 1 + Value.Length);
			var paddingBytes = (int)(3 - (totalSize + 3) % 4);

			return (uint)(totalSize + paddingBytes);
		}

		public static KtxKeyValuePair ReadKeyValuePair(BinaryReader br, out int bytesRead)
		{
			var totalSize = br.ReadUInt32();
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


			var keySize = i;
			var key = UTF8.GetString(keyValueBytes.Slice(0, keySize));

			var valueSize = (int)(totalSize - keySize - 1);
			var valueBytes = keyValueBytes.Slice(i + 1, valueSize);
			var value = new byte[valueSize];
			valueBytes.CopyTo(value);

			var paddingBytes = (int)(3 - (totalSize + 3) % 4);
			br.SkipPadding(paddingBytes);

			bytesRead = (int)(totalSize + paddingBytes + sizeof(uint));
			return new KtxKeyValuePair(key, value);
		}

		public static uint WriteKeyValuePair(BinaryWriter bw, KtxKeyValuePair pair)
		{
			var keySpanLength = UTF8.GetByteCount(pair.Key);
			Span<byte> keySpan = stackalloc byte[keySpanLength];
			Span<byte> valueSpan = pair.Value;

			var totalSize = (uint)(keySpan.Length + 1 + valueSpan.Length);
			var paddingBytes = (int)(3 - (totalSize + 3) % 4);

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
		public GlType GlType;
		public uint GlTypeSize;
		public GlFormat GlFormat;
		public GlInternalFormat GlInternalFormat;
		public GlFormat GlBaseInternalFormat;
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
			for (var i = 0; i < id.Length; i++)
			{
				if (Identifier[i] != id[i]) return false;
			}
			return true;
		}

		public static KtxHeader InitializeCompressed(int width, int height, GlInternalFormat internalFormat, GlFormat baseInternalFormat)
		{
			var header = new KtxHeader();
			Span<byte> id = stackalloc byte[] { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };
			for (var i = 0; i < id.Length; i++)
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

		public static KtxHeader InitializeUncompressed(int width, int height, GlType type, GlFormat format, uint glTypeSize, GlInternalFormat internalFormat, GlFormat baseInternalFormat)
		{
			var header = new KtxHeader();
			Span<byte> id = stackalloc byte[] { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };
			for (var i = 0; i < id.Length; i++)
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
