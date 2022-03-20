using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncoder.TextureFormats
{
	/// <summary>
	/// A representation of a ktx file.
	/// This class handles loading and saving ktx files into streams.
	/// The full spec can be found here: https://www.khronos.org/opengles/sdk/tools/KTX/file_format_spec/
	/// </summary>
	public class KtxFile : ITextureFileFormat<KtxFile>
	{
		public KtxHeader header;
		public List<KtxKeyValuePair> KeyValuePairs { get; } = new List<KtxKeyValuePair>();
		public List<KtxMipmap> MipMaps { get; } = new List<KtxMipmap>();

		/// <inheritdoc />
		public bool SupportsLdr => true;

		/// <inheritdoc />
		public bool SupportsHdr => true;

		/// <inheritdoc />
		public bool SupportsCubeMap => true;

		/// <inheritdoc />
		public bool SupportsMipMaps => true;

		/// <inheritdoc />
		public bool IsSupportedFormat(CompressionFormat format)
		{
			var (_, internalFormat, _) = format.GetGlFormat();
			return internalFormat != 0;
		}

		/// <inheritdoc />
		public void FromTextureData(BCnTextureData textureData)
		{
			if (!IsSupportedFormat(textureData.Format))
			{
				throw new ArgumentException($"Unsupported format {textureData.Format} in textureData.", nameof(textureData));
			}

			var (glFormat, internalFormat, glType) = textureData.Format.GetGlFormat();

			if (textureData.Format.IsBlockCompressedFormat())
			{
				header = KtxHeader.InitializeCompressed(textureData.Width, textureData.Height, internalFormat, glFormat);
			}
			else
			{
				header = KtxHeader.InitializeUncompressed(
					textureData.Width,
					textureData.Height,
					glType,
					glFormat,
					glType.GetTypeSize(),
					internalFormat,
					glFormat);
			}

			header.NumberOfMipmapLevels = (uint)textureData.NumMips;
			header.NumberOfFaces = (uint)textureData.NumFaces;
			header.NumberOfArrayElements = 0;

			MipMaps.Clear();

			for (var m = 0; m < textureData.NumMips; m++)
			{
				MipMaps.Add(new KtxMipmap(
					(uint)textureData.MipLevels[m].Data.Length,
					(uint)textureData.MipLevels[m].Width,
					(uint)textureData.MipLevels[m].Height,
					(uint)textureData.NumFaces));

				for (var f = 0; f < textureData.NumFaces; f++)
				{
					var data = textureData.Faces[f].Mips[m].Data;
					if (!textureData.IsBlockCompressed)
					{
						data = TextureUnPacker.UnPackTexture(
							data,
							textureData.Faces[f].Mips[m].Width,
							textureData.Faces[f].Mips[m].Height,
							textureData.Format,
							4);
					}

					MipMaps[m].SizeInBytes = (uint)data.Length;
					MipMaps[m].Faces[f] = new KtxMipFace(
						data,
						(uint)textureData.Faces[f].Mips[m].Width,
						(uint)textureData.Faces[f].Mips[m].Height);
				}
			}
			
		}

		/// <inheritdoc />
		public BCnTextureData ToTextureData()
		{
			var format = header.GlInternalFormat.GetCompressionFormat();
			var textureData = new BCnTextureData(
				format,
				(int)header.PixelWidth,
				(int)header.PixelHeight,
				(int)header.NumberOfMipmapLevels,
				header.NumberOfFaces > 1);

			for (var f = 0; f < textureData.NumFaces; f++)
			{
				for (var m = 0; m < textureData.NumMips; m++)
				{
					var data = MipMaps[m].Faces[f].Data;
					if (!textureData.IsBlockCompressed)
					{
						data = TextureUnPacker.PackTexture(
							data,
							textureData.Faces[f].Mips[m].Width,
							textureData.Faces[f].Mips[m].Height,
							textureData.Format,
							4);
					}

					if (data.Length != textureData.Faces[f].Mips[m].SizeInBytes)
					{
						throw new TextureFormatException("KtxFile mipmap size different from expected!");
					}
					
					textureData.Faces[f].Mips[m].Data = data;
				}
			}

			return textureData;
		}

		/// <inheritdoc />
		public void ReadFromStream(Stream inputStream)
		{
			KeyValuePairs.Clear();
			MipMaps.Clear();
			
			using (var br = new BinaryReader(inputStream, Encoding.UTF8, true))
			{
				header = br.ReadStruct<KtxHeader>();

				if (header.NumberOfArrayElements > 0)
				{
					throw new NotSupportedException("KTX files with arrays are not supported.");
				}

				var keyValuePairBytesRead = 0;
				while (keyValuePairBytesRead < header.BytesOfKeyValueData)
				{
					var kvp = KtxKeyValuePair.ReadKeyValuePair(br, out var read);
					keyValuePairBytesRead += read;
					KeyValuePairs.Add(kvp);
				}

				var numberOfFaces = Math.Max(1, header.NumberOfFaces);
				MipMaps.Capacity = (int)header.NumberOfMipmapLevels;
				for (uint mipLevel = 0; mipLevel < header.NumberOfMipmapLevels; mipLevel++)
				{
					var imageSize = br.ReadUInt32();
					var mipWidth = header.PixelWidth / (uint)Math.Pow(2, mipLevel);
					var mipHeight = header.PixelHeight / (uint)Math.Pow(2, mipLevel);

					MipMaps.Add(new KtxMipmap(imageSize, mipWidth, mipHeight, numberOfFaces));

					var cubemap = header.NumberOfFaces > 1 && header.NumberOfArrayElements == 0;
					for (uint face = 0; face < numberOfFaces; face++)
					{
						var faceData = br.ReadBytes((int)imageSize);
						MipMaps[(int)mipLevel].Faces[(int)face] = new KtxMipFace(faceData, mipWidth, mipHeight);
						if (cubemap)
						{
							var cubePadding = 3 - (imageSize + 3) % 4;
							br.SkipPadding(cubePadding);
						}
					}

					var mipPaddingBytes = 3 - (imageSize + 3) % 4;
					br.SkipPadding(mipPaddingBytes);
				}
			}
		}

		/// <inheritdoc />
		public void WriteToStream(Stream outputStream)
		{
			if (MipMaps.Count < 1 || MipMaps[0].NumberOfFaces < 1)
			{
				throw new InvalidOperationException("The KTX structure should have at least 1 mipmap level and 1 Face before writing to file.");
			}

			using (var bw = new BinaryWriter(outputStream, Encoding.UTF8, true))
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

					if (mip < header.NumberOfMipmapLevels - 1)
					{
						var mipPaddingBytes = 3 - (imageSize + 3) % 4;
						bw.AddPadding(mipPaddingBytes);
					}
				}
			}
		}

		public static KtxFile Load(Stream stream)
		{
			var tex = new KtxFile();
			tex.ReadFromStream(stream);
			return tex;
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
			var keySpanLength = Encoding.UTF8.GetByteCount(Key);
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
			var key = Encoding.UTF8.GetString(keyValueBytes.Slice(0, keySize));

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
			var keySpanLength = Encoding.UTF8.GetByteCount(pair.Key);
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
		public uint SizeInBytes { get; set; }
		public byte[] Data { get; set; }

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
		public uint SizeInBytes { get; set; }
		public uint Width { get; set; }
		public uint Height { get; set; }
		public uint NumberOfFaces { get; set; }
		public KtxMipFace[] Faces { get; set; }
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
