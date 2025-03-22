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
		public bool SupportsArrays => true;

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
			header.NumberOfArrayElements = textureData.IsArrayTexture ? (uint)textureData.NumArrayElements : 0;

			MipMaps.Clear();

			for (var m = 0; m < textureData.NumMips; m++)
			{
				MipMaps.Add(new KtxMipmap(
					(uint)textureData.Mips[m].SizeInBytes,
					(uint)textureData.Mips[m].Width,
					(uint)textureData.Mips[m].Height,
					(uint)textureData.NumArrayElements,
					(uint)textureData.NumFaces));

				for (var a = 0; a < textureData.NumArrayElements; a++)
				{
					for (var f = 0; f < textureData.NumFaces; f++)
					{
						var index = a * textureData.NumFaces + f;
						var face = (CubeMapFaceDirection)f;

						var data = textureData.Mips[m][face, a].Data;
						if (!textureData.IsBlockCompressed)
						{
							data = TextureUnPacker.UnPackTexture(
								data,
								textureData.Mips[m].Width,
								textureData.Mips[m].Height,
								textureData.Format,
								4);
						}

						MipMaps[m].SizeInBytes = (uint)data.Length;
						MipMaps[m].SurfaceArray[index] = new KtxMipSurface(
							data,
							(uint)textureData.Mips[m].Width,
							(uint)textureData.Mips[m].Height);
					}
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
				depth: 1,
				numMips:(int)header.NumberOfMipmapLevels,
				numArrayElements: (int) header.NumberOfArrayElements,
				isCubeMap: header.NumberOfFaces > 1,
				allocateBuffers: false,
				alphaChannelHint: AlphaChannelHint.Premultiplied);

			for (var m = 0; m < textureData.NumMips; m++)
			{
				for (var a = 0; a < textureData.NumArrayElements; a++)
				{
					for (var f = 0; f < textureData.NumFaces; f++)
					{
						var index = a * textureData.NumFaces + f;
						var face = (CubeMapFaceDirection)f;

						var data = MipMaps[m].SurfaceArray[index].Data;

						if (!textureData.IsBlockCompressed)
						{
							data = TextureUnPacker.PackTexture(
								data,
								textureData.Mips[m].Width,
								textureData.Mips[m].Height,
								textureData.Format,
								4);
						}

						if (data.Length != textureData.Mips[m].SizeInBytes)
						{
							throw new TextureFormatException("KtxFile mipmap size different from expected!");
						}

						textureData.Mips[m][face, a].Data = data;
					}
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

				var keyValuePairBytesRead = 0;
				while (keyValuePairBytesRead < header.BytesOfKeyValueData)
				{
					var kvp = KtxKeyValuePair.ReadKeyValuePair(br, out var read);
					keyValuePairBytesRead += read;
					KeyValuePairs.Add(kvp);
				}

				var numberOfFaces = Math.Max(1, header.NumberOfFaces);
				var numberOfArrayElements = Math.Max(1, header.NumberOfArrayElements);

				var totalAmountOfFaces = numberOfFaces * numberOfArrayElements;

				MipMaps.Capacity = (int)header.NumberOfMipmapLevels;
				for (uint mipLevel = 0; mipLevel < header.NumberOfMipmapLevels; mipLevel++)
				{
					var isNonArrayCubemap = header.NumberOfFaces > 1 && header.NumberOfArrayElements == 0;

					var imageSize = br.ReadUInt32();
					var mipWidth = header.PixelWidth / (uint)Math.Pow(2, mipLevel);
					var mipHeight = header.PixelHeight / (uint)Math.Pow(2, mipLevel);

					var faceSize = isNonArrayCubemap ? imageSize : imageSize / totalAmountOfFaces;

					MipMaps.Add(new KtxMipmap(faceSize, mipWidth, mipHeight, numberOfArrayElements, numberOfFaces));

					for (uint face = 0; face < totalAmountOfFaces; face++)
					{
						var faceData = br.ReadBytes((int)faceSize);
						MipMaps[(int)mipLevel].SurfaceArray[(int)face] = new KtxMipSurface(faceData, mipWidth, mipHeight);
						if (isNonArrayCubemap)
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
				header.NumberOfArrayElements = MipMaps[0].NumberOfArrayElements > 1 ?
					MipMaps[0].NumberOfArrayElements : 0;

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
					var isNonArrayCubemap = header.NumberOfFaces == 6 && header.NumberOfArrayElements == 0;
					var numArrayElements = Math.Max(header.NumberOfArrayElements, 1);

					// From https://registry.khronos.org/KTX/specs/1.0/ktxspec.v1.html
					//
					// For most textures imageSize is the number of bytes of pixel data in the current LOD level.
					// This includes all array layers, all z slices, all faces, all rows and all pixelsin each row for the mipmap level.
					// It does not include any bytes in mipPadding.
					//
					// The exception is non-array cubemap textures
					// (any texture where numberOfFaces is 6 and numberOfArrayElements is 0).
					// For these textures imageSize is the number of bytes in each face of the texture for the current LOD level,
					// not including bytes in cubePadding or mipPadding.

					if (isNonArrayCubemap)
					{
						imageSize *= numArrayElements * header.NumberOfFaces;
					}

					bw.Write(imageSize);

					for (var f = 0; f < header.NumberOfFaces * numArrayElements; f++)
					{
						bw.Write(MipMaps[mip].SurfaceArray[f].Data);

						if (isNonArrayCubemap)
						{
							var cubePadding = 3 - (imageSize + 3) % 4;
							bw.AddPadding(cubePadding);
						}
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

	public class KtxMipSurface
	{
		public uint Width { get; set; }
		public uint Height { get; set; }
		public uint SizeInBytes { get; set; }
		public byte[] Data { get; set; }

		public KtxMipSurface(byte[] data, uint width, uint height)
		{
			Width = width;
			Height = height;
			SizeInBytes = (uint)data.Length;
			Data = data;
		}
	}

	public class KtxMipmap
	{
		/// <summary>
		/// Size of a single face in bytes.
		/// </summary>
		public uint SizeInBytes { get; set; }
		public uint Width { get; init; }
		public uint Height { get; init; }
		public uint NumberOfFaces { get; init; }
		public uint NumberOfArrayElements { get; init; }

		/// <summary>
		/// Length will be Number of Faces * Number of Array Elements
		/// Index with (arrayIndex * NumFaces + faceIndex)
		/// </summary>
		public KtxMipSurface[] SurfaceArray { get; init; }


		public KtxMipmap(uint sizeInBytes, uint width, uint height, uint numArrayElements, uint numberOfFaces)
		{
			SizeInBytes = sizeInBytes;
			Width = Math.Max(1, width);
			Height = Math.Max(1, height);
			NumberOfArrayElements = Math.Max(1, numArrayElements);
			NumberOfFaces = Math.Max(1, numberOfFaces);
			SurfaceArray = new KtxMipSurface[numArrayElements * numberOfFaces];
		}
	}
}
