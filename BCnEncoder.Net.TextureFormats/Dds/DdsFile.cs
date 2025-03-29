using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncoder.TextureFormats
{
	public class DdsFile : ITextureFileFormat<DdsFile>
	{
		/// <summary>
		/// Direct X API levels
		/// </summary>
		public enum DxApiLevel
		{
			Dx9  = 9,
			Dx10 = 10,
			Dx11 = 11,
			Dx12 = 12
		}

		/// <summary>
		/// Which Minimum DirectX API level to use when writing DDS files.
		/// </summary>
		public static DxApiLevel PreferredMinApiLevel = DxApiLevel.Dx9;

		public DdsHeader header;
		public DdsHeaderDx10 dx10Header;
		public List<DdsArrayElement> ArrayElements { get; } = new List<DdsArrayElement>();

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
			return format.ToDxgiFormat() != DxgiFormat.DxgiFormatUnknown;
		}

		/// <inheritdoc />
		public void FromTextureData(BCnTextureData textureData)
		{
			if (!IsSupportedFormat(textureData.Format))
			{
				throw new ArgumentException($"Unsupported format {textureData.Format} in textureData.",
					nameof(textureData));
			}

			if (textureData.Depth > 1)
				throw new NotSupportedException("Depth > 1 is not supported.");

			(header, dx10Header) = DdsHeader.InitializeFor(textureData.Width, textureData.Height, textureData.Depth, textureData.Format,
				textureData.IsArrayTexture, textureData.AlphaChannelHint);

			if (textureData.IsArrayTexture)
			{
				if (!header.ddsPixelFormat.IsDxt10Format)
					throw new InvalidOperationException("Cannot create an array texture with a non-DXT10 supported format.");

				header.dwCaps |= HeaderCaps.DdscapsComplex;
				dx10Header.arraySize = (uint)textureData.NumArrayElements;
			}

			if (textureData.HasMipLevels) // MipMaps
			{
				header.dwCaps |= HeaderCaps.DdscapsMipmap | HeaderCaps.DdscapsComplex;
				header.dwMipMapCount = (uint)textureData.NumMips;
			}

			if (textureData.IsCubeMap) // CubeMap
			{
				header.dwCaps |= HeaderCaps.DdscapsComplex;
				header.dwCaps2 |= HeaderCaps2.Ddscaps2Cubemap |
				                  HeaderCaps2.Ddscaps2CubemapPositivex |
				                  HeaderCaps2.Ddscaps2CubemapNegativex |
				                  HeaderCaps2.Ddscaps2CubemapPositivey |
				                  HeaderCaps2.Ddscaps2CubemapNegativey |
				                  HeaderCaps2.Ddscaps2CubemapPositivez |
				                  HeaderCaps2.Ddscaps2CubemapNegativez;

				if (header.ddsPixelFormat.IsDxt10Format)
				{
					dx10Header.arraySize *= 6;
				}
			}

			if ((header.dwFlags & HeaderFlags.DdsdLinearsize) != 0)
			{
				header.dwPitchOrLinearSize = (uint)textureData.Mips[0].SizeInBytes;
			}

			var uWidth = (uint)textureData.Width;
			var uHeight = (uint)textureData.Height;
			var uDepth = (uint)textureData.Depth;

			ArrayElements.Clear();

			for (var a = 0; a < textureData.NumArrayElements; a++)
			{
				for (var f = 0; f < textureData.NumFaces; f++)
				{
					var face = (CubeMapFaceDirection)f;
					DdsArrayElement ddsArrayElement = new DdsArrayElement(uWidth, uHeight, uDepth,
						(uint)textureData.Mips[0].SizeInBytes, textureData.NumMips);

					for (var m = 0; m < textureData.Mips.Length; m++)
					{
						ddsArrayElement.MipMaps[m] = new DdsMipMap(
							textureData.Mips[m][face, a].Data,
							(uint)textureData.Mips[m].Width,
							(uint)textureData.Mips[m].Height,
							(uint)textureData.Mips[m].Depth);
					}

					ArrayElements.Add(ddsArrayElement);
				}
			}
		}

		/// <inheritdoc />
		public BCnTextureData ToTextureData()
		{
			GetBCnCompressionFormat(out CompressionFormat compressionFormat, out AlphaChannelHint alphaChannelHint);

			var isCubeMap = (header.dwCaps2 & HeaderCaps2.Ddscaps2Cubemap) != 0;
			var faceCount = isCubeMap ? 6 : 1;
			var arrayCount = 1;

			if (header.ddsPixelFormat.IsDxt10Format )
			{
				if (dx10Header.arraySize <= 1)
					arrayCount = 1; // Assume a single texture in array
				else if (dx10Header.arraySize % faceCount != 0)
					throw new FormatException(
						"The DXT10 header contains invalid arraySize. Should be divisible by faceCount.");
				else
					arrayCount = (int)dx10Header.arraySize / faceCount;
			}

			var width = (header.dwFlags & HeaderFlags.DdsdWidth) != 0 ? header.dwWidth : 1;
			var height = (header.dwFlags & HeaderFlags.DdsdHeight) != 0 ? header.dwHeight : 1;
			var depth = (header.dwFlags & HeaderFlags.DdsdDepth) != 0 ? header.dwDepth : 1;

			var data = new BCnTextureData(
				compressionFormat,
				(int)width,
				(int)height,
				(int)depth,
				(int)header.MipMapCountOrOne,
				arrayCount,
				isCubeMap,
				false,
				alphaChannelHint);

			for (var a = 0; a < data.NumArrayElements; a++)
			{
				for (var f = 0; f < data.NumFaces; f++)
				{
					for (var m = 0; m < data.NumMips; m++)
					{
						int arrayIndex = f + a * data.NumFaces;
						if (ArrayElements[arrayIndex].MipMaps[m].SizeInBytes != data.Mips[m].SizeInBytes)
						{
							throw new TextureFormatException("DdsFile mipmap size different from expected!");
						}

						data.Mips[m][(CubeMapFaceDirection)f, a].Data = ArrayElements[arrayIndex].MipMaps[m].Data;
					}
				}
			}

			return data;
		}

		/// <inheritdoc />
		public void ReadFromStream(Stream inputStream)
		{
			using (var br = new BinaryReader(inputStream, Encoding.UTF8, true))
			{
				var magic = br.ReadUInt32();
				if (magic != 0x20534444U)
				{
					throw new FormatException("The file does not contain a dds file.");
				}

				header = br.ReadStruct<DdsHeader>();
				if (header.dwSize != 124)
				{
					throw new FormatException("The file header contains invalid dwSize.");
				}

				var dx10Format = header.ddsPixelFormat.IsDxt10Format;

				if (dx10Format)
				{
					dx10Header = br.ReadStruct<DdsHeaderDx10>();
				}

				var mipMapCount = (header.dwCaps & HeaderCaps.DdscapsMipmap) != 0 ? header.dwMipMapCount : 1;
				var faceCount = (header.dwCaps2 & HeaderCaps2.Ddscaps2Cubemap) != 0 ? 6u : 1u;

				uint arrayCount = 1;
				if (dx10Format)
				{
					if (dx10Header.arraySize <= 1)
						arrayCount = 1; // Assume a single texture in array
					else if (dx10Header.arraySize % faceCount != 0)
						throw new FormatException(
							"The file header contains invalid arraySize. Should be divisible by faceCount.");
					else
						arrayCount = dx10Header.arraySize / faceCount;
				}
				var width = (header.dwFlags & HeaderFlags.DdsdWidth) != 0 ? header.dwWidth : 1;
				var height = (header.dwFlags & HeaderFlags.DdsdHeight) != 0 ? header.dwHeight : 1;
				var depth = (header.dwFlags & HeaderFlags.DdsdDepth) != 0 ? header.dwDepth : 1;

				GetBCnCompressionFormat(out CompressionFormat format, out _);

				for (var element = 0; element < faceCount * arrayCount; element++)
				{
					var sizeInBytes = GetSizeInBytes(format, width, height, depth);

					ArrayElements.Add(new DdsArrayElement(width, height, depth, sizeInBytes, (int)mipMapCount));

					for (var mip = 0; mip < mipMapCount; mip++)
					{
						MipMapper.CalculateMipLevelSize(
							(int)width,
							(int)height,
							(int)depth,
							mip,
							out var mipWidth,
							out var mipHeight,
							out var mipDepth);

						if (mip > 0) //Calculate new byteSize
						{
							sizeInBytes = GetSizeInBytes(format, (uint)mipWidth, (uint)mipHeight, (uint)mipDepth);
						}

						var data = new byte[sizeInBytes];
						br.Read(data);
						ArrayElements[element].MipMaps[mip] = new DdsMipMap(data, (uint)mipWidth, (uint)mipHeight, (uint)mipDepth);
					}
				}
			}
		}

		/// <inheritdoc />
		public void WriteToStream(Stream outputStream)
		{
			if (ArrayElements.Count < 1 || ArrayElements[0].MipMaps.Length < 1)
			{
				throw new InvalidOperationException(
					"The DDS structure should have at least 1 mipmap level and 1 Face before writing to file.");
			}

			for (var i = 0; i < ArrayElements.Count; i++)
			{
				if (ArrayElements[i].Width != header.dwWidth || ArrayElements[i].Height != header.dwHeight || ArrayElements[i].Depth != header.dwDepth)
				{
					throw new InvalidOperationException("Faces with different sizes are not supported.");
				}
			}

			var arrayElementsCount = ArrayElements.Count;
			var mipCount = header.MipMapCountOrOne;

			using (var bw = new BinaryWriter(outputStream, Encoding.UTF8, true))
			{
				bw.Write(0x20534444U); // magic 'DDS '

				bw.WriteStruct(header);

				if (header.ddsPixelFormat.IsDxt10Format)
				{
					bw.WriteStruct(dx10Header);
				}

				for (var i = 0; i < arrayElementsCount; i++)
				{
					for (var mip = 0; mip < mipCount; mip++)
					{
						bw.Write(ArrayElements[i].MipMaps[mip].Data);
					}
				}
			}
		}

		public void GetBCnCompressionFormat(out CompressionFormat format, out AlphaChannelHint alphaChannelHint)
		{
			format = CompressionFormat.Unknown;
			alphaChannelHint = AlphaChannelHint.Unknown;

			if (header.ddsPixelFormat.TryGetDx9(out format, out alphaChannelHint))
			{
				return;
			}

			DxgiFormat dxgiFormat = dx10Header.dxgiFormat;
			format = dxgiFormat.ToCompressionFormat();

			if (header.ddsPixelFormat.dwFlags.HasFlag(PixelFormatFlags.DdpfAlphaPixels))
			{
				if (format == CompressionFormat.Bc1)
				{
					format = CompressionFormat.Bc1WithAlpha;
				}
				else if (format == CompressionFormat.Bc1_sRGB)
				{
					format = CompressionFormat.Bc1WithAlpha_sRGB;
				}
			}

			// Alpha channel hint. Only supported in DX12 and later
			uint maskedAlphaMode = (dx10Header.miscFlags2 & 0x7);

			if (maskedAlphaMode == (uint)Dx10HeaderMiscFlags2.Dx10HeaderMiscFlags2AlphaModeUnknown)
			{
				alphaChannelHint = AlphaChannelHint.Unknown;
			}
			else if (maskedAlphaMode == (uint)Dx10HeaderMiscFlags2.Dx10HeaderMiscFlags2AlphaModePremultiplied)
			{
				alphaChannelHint = AlphaChannelHint.Premultiplied;
			}
			else
			{
				alphaChannelHint = AlphaChannelHint.Straight;
			}
		}

		private static uint GetSizeInBytes(CompressionFormat format, uint width, uint height, uint depth)
		{
			uint sizeInBytes;
			if (format.IsBlockCompressedFormat())
			{
				sizeInBytes = (uint)ImageToBlocks.CalculateNumOfBlocks(format, (int)width, (int)height, (int)depth);
				sizeInBytes *= (uint)format.GetBytesPerBlock();
			}
			else
			{
				sizeInBytes = width * height * depth;
				sizeInBytes = (uint)(sizeInBytes * format.GetBytesPerBlock());
			}

			return sizeInBytes;
		}

		public static DdsFile Load(Stream stream)
		{
			var tex = new DdsFile();
			tex.ReadFromStream(stream);
			return tex;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct DdsHeader
	{
		/// <summary>
		/// Has to be 124
		/// </summary>
		public uint dwSize;

		public HeaderFlags dwFlags;
		public uint dwHeight;
		public uint dwWidth;
		public uint dwPitchOrLinearSize;
		public uint dwDepth;
		public uint dwMipMapCount;
		public fixed uint dwReserved1[11];
		public DdsPixelFormat ddsPixelFormat;
		public HeaderCaps dwCaps;
		public HeaderCaps2 dwCaps2;
		public uint dwCaps3;
		public uint dwCaps4;
		public uint dwReserved2;

		public uint MipMapCountOrOne => (dwFlags & HeaderFlags.DdsdMipmapcount) != 0 ? dwMipMapCount : 1;

		public static (DdsHeader, DdsHeaderDx10) InitializeDx10Header(int width, int height, int depth, CompressionFormat format,
			bool forceDxt10Header, AlphaChannelHint alphaChannelHint)
		{
			var header = new DdsHeader();
			var dxt10Header = new DdsHeaderDx10();

			if (format == CompressionFormat.Unknown)
				throw new ArgumentException("Format must be set");
			if (width < 1)
				throw new ArgumentException("Width must be greater than 0");
			if (height < 1)
				throw new ArgumentException("Height must be greater than 0");
			if (depth < 1)
				throw new ArgumentException("Depth must be greater than 0");

			header.dwSize = 124;
			header.dwFlags = HeaderFlags.Required | HeaderFlags.DdsdMipmapcount;
			header.dwWidth = (uint)width;
			header.dwHeight = (uint)height;
			header.dwDepth = (uint)depth;
			header.dwMipMapCount = 1;
			header.dwCaps = HeaderCaps.DdscapsTexture;

			// Set additional flags for 3D textures
			if (depth > 1)
			{
				header.dwFlags |= HeaderFlags.DdsdDepth;
				header.dwCaps |= HeaderCaps.DdscapsComplex;
				header.dwCaps2 |= HeaderCaps2.Ddscaps2Volume;
			}

			if (!format.IsBlockCompressedFormat())
			{
				header.dwFlags |= HeaderFlags.DdsdPitch;
				header.dwPitchOrLinearSize = (uint)((width * format.GetBytesPerBlock() * 8 + 7) / 8);
			}
			else
			{
				header.dwFlags |= HeaderFlags.DdsdLinearsize;
				header.dwPitchOrLinearSize = (uint)(width * height * depth * format.GetBytesPerBlock());
			}


			// Try to make a DX9 pixel format
			bool isDx9Compatible = DdsPixelFormat.TryMakeDx9(format, alphaChannelHint, out DdsPixelFormat pixelFormat);

			if (forceDxt10Header || !isDx9Compatible || DdsFile.PreferredMinApiLevel >= DdsFile.DxApiLevel.Dx10)
			{
				// Use DX10 header if preferred or if DX9 is not compatible

				// Get the DXGI format
				DxgiFormat dxgiFormat = format.ToDxgiFormat();

				// Special case for ATC formats which don't have proper DXGI format definitions
				switch (dxgiFormat)
				{
					case DxgiFormat.DxgiFormatAtcExt:
					case DxgiFormat.DxgiFormatAtcExplicitAlphaExt:
					case DxgiFormat.DxgiFormatAtcInterpolatedAlphaExt:
						throw new NotSupportedException("ATC formats are not supported for DX10+ textures.");

					default:
						header.ddsPixelFormat = new DdsPixelFormat
						{
							dwSize = 32,
							dwFlags = PixelFormatFlags.DdpfFourcc,
							dwFourCc = DdsPixelFormat.Dx10
						};
						dxt10Header.arraySize = 1;
						dxt10Header.dxgiFormat = dxgiFormat;

						// Set the appropriate resource dimension based on the texture dimensions
						if (depth > 1)
						{
							dxt10Header.resourceDimension = D3D10ResourceDimension.D3D10ResourceDimensionTexture3D;
						}
						else if (height == 1)
						{
							dxt10Header.resourceDimension = D3D10ResourceDimension.D3D10ResourceDimensionTexture1D;
						}
						else
						{
							dxt10Header.resourceDimension = D3D10ResourceDimension.D3D10ResourceDimensionTexture2D;
						}

						// Set the alpha mode (only supported for DX12 and above)
						if (DdsFile.PreferredMinApiLevel >= DdsFile.DxApiLevel.Dx12)
						{
							if (!format.SupportsAlpha())
							{
								dxt10Header.miscFlags2 = (uint)Dx10HeaderMiscFlags2.Dx10HeaderMiscFlags2AlphaModeOpaque;
							}
							switch (alphaChannelHint)
							{
								case AlphaChannelHint.Unknown:
									dxt10Header.miscFlags2 = (uint)Dx10HeaderMiscFlags2.Dx10HeaderMiscFlags2AlphaModeUnknown;
									break;
								case AlphaChannelHint.Premultiplied:
									dxt10Header.miscFlags2 = (uint)Dx10HeaderMiscFlags2.Dx10HeaderMiscFlags2AlphaModePremultiplied;
									break;
								case AlphaChannelHint.Straight:
									dxt10Header.miscFlags2 = (uint)Dx10HeaderMiscFlags2.Dx10HeaderMiscFlags2AlphaModeStraight;
									break;
							}
						}
						break;
				}
			}
			else
			{
				// Use the DX9 pixel format
				header.ddsPixelFormat = pixelFormat;
			}

			return (header, dxt10Header);
		}

		public static (DdsHeader, DdsHeaderDx10) InitializeFor(int width, int height, int depth, CompressionFormat format, bool forceDx10, AlphaChannelHint alphaChannelHint = AlphaChannelHint.Straight)
		{
			// Initialize the DX10 header
			var (header, dxt10Header) = InitializeDx10Header(width, height, depth, format, forceDx10, alphaChannelHint);

			return (header, dxt10Header);
		}
	}

	public struct DdsPixelFormat
	{
		public static readonly uint Dx10 = MakeFourCc('D', 'X', '1', '0');

		public static readonly uint Dxt1 = MakeFourCc('D', 'X', 'T', '1');
		public static readonly uint Dxt2 = MakeFourCc('D', 'X', 'T', '2');
		public static readonly uint Dxt3 = MakeFourCc('D', 'X', 'T', '3');
		public static readonly uint Dxt4 = MakeFourCc('D', 'X', 'T', '4');
		public static readonly uint Dxt5 = MakeFourCc('D', 'X', 'T', '5');
		public static readonly uint Ati1 = MakeFourCc('A', 'T', 'I', '1');
		public static readonly uint Ati2 = MakeFourCc('A', 'T', 'I', '2');
		public static readonly uint Atc = MakeFourCc('A', 'T', 'C', ' ');
		public static readonly uint Atci = MakeFourCc('A', 'T', 'C', 'I');
		public static readonly uint Atca = MakeFourCc('A', 'T', 'C', 'A');

		public static readonly uint Bc4S = MakeFourCc('B', 'C', '4', 'S');
		public static readonly uint Bc4U = MakeFourCc('B', 'C', '4', 'U');
		public static readonly uint Bc5S = MakeFourCc('B', 'C', '5', 'S');
		public static readonly uint Bc5U = MakeFourCc('B', 'C', '5', 'U');

		public static readonly uint Rgbg = MakeFourCc('R', 'G', 'B', 'G');
		public static readonly uint Grgb = MakeFourCc('G', 'R', 'G', 'B');
		public static readonly uint Uyvy = MakeFourCc('U', 'Y', 'V', 'Y');
		public static readonly uint Yuy2 = MakeFourCc('Y', 'U', 'Y', '2');

		public static readonly uint A16B16G16R16 = 36;
		public static readonly uint Q16W16V16U16 = 110;
		public static readonly uint R16F = 111;
		public static readonly uint G16R16F = 112;
		public static readonly uint A16B16G16R16F = 113;
		public static readonly uint R32F = 114;
		public static readonly uint G32R32F = 115;
		public static readonly uint A32B32G32R32F = 116;
		public static readonly uint CxV8U8 = 117;

		public static readonly (DxgiFormat dxgiFormat, D3DFormat d3dFormat, PixelFormatFlags flags, uint bitCount, uint rMask, uint gMask, uint bMask, uint aMask)[] MappingTable = new []
		{
			(DxgiFormat.DxgiFormatR8G8B8A8Unorm, D3DFormat.D3DFormatA8B8G8R8, PixelFormatFlags.DdpfRgb | PixelFormatFlags.DdpfAlphaPixels, 32U, 0xffU, 0xff00U, 0xff0000U, 0xff000000U),
			(DxgiFormat.DxgiFormatR16G16Unorm, D3DFormat.D3DFormatG16R16, PixelFormatFlags.DdpfRgb | PixelFormatFlags.DdpfAlphaPixels, 32U, 0xffffU, 0xffff0000U, 0x0U, 0x0U),
			(DxgiFormat.DxgiFormatR10G10B10A2Unorm, D3DFormat.D3DFormatA2B10G10R10, PixelFormatFlags.DdpfRgb | PixelFormatFlags.DdpfAlphaPixels, 32U, 0x3ffU, 0xffc00U, 0x3ff00000U, 0xc0000000U),
			(DxgiFormat.DxgiFormatR16G16Unorm, D3DFormat.D3DFormatG16R16, PixelFormatFlags.DdpfRgb, 32U, 0xffffU, 0xffff0000U, 0x0U, 0x0U),
			(DxgiFormat.DxgiFormatB5G5R5A1Unorm, D3DFormat.D3DFormatA1R5G5B5, PixelFormatFlags.DdpfRgb | PixelFormatFlags.DdpfAlphaPixels, 16U, 0x7c00U, 0x3e0U, 0x1fU, 0x8000U),
			(DxgiFormat.DxgiFormatB5G6R5Unorm, D3DFormat.D3DFormatR5G6B5, PixelFormatFlags.DdpfRgb, 16U, 0xf800U, 0x7e0U, 0x1fU, 0x0U),
			(DxgiFormat.DxgiFormatA8Unorm, D3DFormat.D3DFormatA8, PixelFormatFlags.DdpfAlpha, 8U, 0x0U, 0x0U, 0x0U, 0xffU),
			(DxgiFormat.DxgiFormatR8Unorm, D3DFormat.D3DFormatL8, PixelFormatFlags.DdpfLuminance, 8U, 0xffU, 0x0U, 0x0U, 0x0U),
			(DxgiFormat.DxgiFormatR8G8Unorm, D3DFormat.D3DFormatA8L8, PixelFormatFlags.DdpfLuminance | PixelFormatFlags.DdpfAlphaPixels, 16U, 0xffU, 0x0U, 0x0U, 0xff00U),
			(DxgiFormat.DxgiFormatUnknown, D3DFormat.D3DFormatA8R8G8B8, PixelFormatFlags.DdpfRgb | PixelFormatFlags.DdpfAlphaPixels, 32U, 0xff0000U, 0xff00U, 0xffU, 0xff000000U),
			(DxgiFormat.DxgiFormatUnknown, D3DFormat.D3DFormatX8R8G8B8, PixelFormatFlags.DdpfRgb, 32U, 0xff0000U, 0xff00U, 0xffU, 0x0U),
			(DxgiFormat.DxgiFormatUnknown, D3DFormat.D3DFormatX8B8G8R8, PixelFormatFlags.DdpfRgb, 32U, 0xffU, 0xff00U, 0xff0000U, 0x0U),
			(DxgiFormat.DxgiFormatUnknown, D3DFormat.D3DFormatA2R10G10B10, PixelFormatFlags.DdpfRgb | PixelFormatFlags.DdpfAlphaPixels, 32U, 0x3ff00000U, 0xffc00U, 0x3ffU, 0xc0000000U),
			(DxgiFormat.DxgiFormatUnknown, D3DFormat.D3DFormatR8G8B8, PixelFormatFlags.DdpfRgb, 24U, 0xff0000U, 0xff00U, 0xffU, 0x0U),
			(DxgiFormat.DxgiFormatUnknown, D3DFormat.D3DFormatX1R5G5B5, PixelFormatFlags.DdpfRgb, 16U, 0x7c00U, 0x3e0U, 0x1fU, 0x0U),
			(DxgiFormat.DxgiFormatUnknown, D3DFormat.D3DFormatA4R4G4B4, PixelFormatFlags.DdpfRgb | PixelFormatFlags.DdpfAlphaPixels, 16U, 0xf00U, 0xf0U, 0xfU, 0xf000U),
			(DxgiFormat.DxgiFormatUnknown, D3DFormat.D3DFormatX4R4G4B4, PixelFormatFlags.DdpfRgb, 16U, 0xf00U, 0xf0U, 0xfU, 0x0U),
			(DxgiFormat.DxgiFormatUnknown, D3DFormat.D3DFormatA8R3G3B2, PixelFormatFlags.DdpfRgb | PixelFormatFlags.DdpfAlphaPixels, 16U, 0xe0U, 0x1cU, 0x3U, 0xff00U),
			(DxgiFormat.DxgiFormatUnknown, D3DFormat.D3DFormatL16, PixelFormatFlags.DdpfLuminance, 16U, 0xffffU, 0x0U, 0x0U, 0x0U),
			(DxgiFormat.DxgiFormatUnknown, D3DFormat.D3DFormatA4L4, PixelFormatFlags.DdpfLuminance | PixelFormatFlags.DdpfAlphaPixels, 8U, 0xfU, 0x0U, 0x0U, 0xf0U)
		};

		/// <summary>
		/// Mapping for FourCC codes to DXGI formats, with premultiplied alpha distinction.
		/// DXT2 = Premultiplied DXT3
		/// DXT4 = Premultiplied DXT5
		/// </summary>
		public static readonly (DxgiFormat dxgiFormat, uint standardFourCc, uint premultipliedFourCc)[] PremultipliedFourCcTable = new []
		{
			(DxgiFormat.DxgiFormatBc2Unorm, Dxt3, Dxt2),
			(DxgiFormat.DxgiFormatBc3Unorm, Dxt5, Dxt4),
		};

		/// <summary>
		/// Mapping for FourCC codes to DXGI formats.
		/// This is used for DDS files with FourCC codes.
		/// </summary>
		public static readonly (uint fourCc, DxgiFormat dxgiFormat)[] FourCcTable = new []
		{
			// ATC formats.
			(Atc, DxgiFormat.DxgiFormatAtcExt),
			(Atca, DxgiFormat.DxgiFormatAtcExplicitAlphaExt),
			(Atci, DxgiFormat.DxgiFormatAtcInterpolatedAlphaExt),

			// BC1 format.
			(Dxt1, DxgiFormat.DxgiFormatBc1Unorm),

			// BC4 formats. Ati1 is earlier in the list for wider support, even if it doesn't differentiate between signed and unsigned
			(Ati1, DxgiFormat.DxgiFormatBc4Unorm),
			(Ati1, DxgiFormat.DxgiFormatBc4Snorm),
			(Bc4U, DxgiFormat.DxgiFormatBc4Unorm),
			(Bc4S, DxgiFormat.DxgiFormatBc4Snorm),

			// BC5 formats. Ati2 is higher up for the same reason.
			(Ati2, DxgiFormat.DxgiFormatBc5Unorm),
			(Ati2, DxgiFormat.DxgiFormatBc5Snorm),
			(Bc5U, DxgiFormat.DxgiFormatBc5Unorm),
			(Bc5S, DxgiFormat.DxgiFormatBc5Snorm),

			// YUV-like packed formats
			(Rgbg, DxgiFormat.DxgiFormatR8G8B8G8Unorm),
			(Grgb, DxgiFormat.DxgiFormatG8R8G8B8Unorm),
			// Numeric format codes
			(A16B16G16R16, DxgiFormat.DxgiFormatR16G16B16A16Unorm),
			(Q16W16V16U16, DxgiFormat.DxgiFormatR16G16B16A16Snorm),
			// Float formats
			(R16F, DxgiFormat.DxgiFormatR16Float),
			(G16R16F, DxgiFormat.DxgiFormatR16G16Float),
			(A16B16G16R16F, DxgiFormat.DxgiFormatR16G16B16A16Float),
			(R32F, DxgiFormat.DxgiFormatR32Float),
			(G32R32F, DxgiFormat.DxgiFormatR32G32Float),
			(A32B32G32R32F, DxgiFormat.DxgiFormatR32G32B32A32Float),

			// Unmapped formats
			(CxV8U8, DxgiFormat.DxgiFormatUnknown), // Maps to D3DFMT_CxV8U8
			(Uyvy, DxgiFormat.DxgiFormatUnknown), // Maps to D3DFMT_UYVY
			(Yuy2, DxgiFormat.DxgiFormatUnknown), // Maps to D3DFMT_YUY2
		};

		public static uint MakeFourCc(char c0, char c1, char c2, char c3)
		{
			uint result = c0;
			result |= (uint)c1 << 8;
			result |= (uint)c2 << 16;
			result |= (uint)c3 << 24;
			return result;
		}

		public uint dwSize;
		public PixelFormatFlags dwFlags;
		public uint dwFourCc;
		public uint dwRgbBitCount;
		public uint dwRBitMask;
		public uint dwGBitMask;
		public uint dwBBitMask;
		public uint dwABitMask;

		/// <summary>
		/// Attempt to determine the <see cref="CompressionFormat"/> represented by the DDPixelFormat structure.
		/// </summary>
		/// <param name="format">The determined format, or <see cref="CompressionFormat.Unknown"/> if the format could not be determined.</param>
		/// <param name="alphaChannelHint">The hint for the alpha channel of the format, or <see cref="AlphaChannelHint.Unknown"/> if the format could not be determined.</param>
		/// <returns><c>false</c> if the format should be read from the DX10 header, <c>true</c> otherwise.</returns>
		internal bool TryGetDx9(out CompressionFormat format, out AlphaChannelHint alphaChannelHint)
		{
			format = CompressionFormat.Unknown;
			alphaChannelHint = AlphaChannelHint.Unknown;

			// Handle block-compressed formats with FourCC codes
			if (dwFlags.HasFlag(PixelFormatFlags.DdpfFourcc))
			{
				// If it's DX10, we can't determine the format from the pixel format alone
				if (dwFourCc == Dx10)
				{
					return false;
				}

				// Check against PremultipliedFourCcTable
				foreach (var entry in PremultipliedFourCcTable)
				{
					if (dwFourCc == entry.standardFourCc || dwFourCc == entry.premultipliedFourCc)
					{
						// Convert from DxgiFormat to CompressionFormat
						format = entry.dxgiFormat.ToCompressionFormat();
						alphaChannelHint = dwFourCc == entry.standardFourCc ? AlphaChannelHint.Straight : AlphaChannelHint.Premultiplied;
						return true;
					}
				}

				// Check against rest of the FourCcs
				foreach (var entry in FourCcTable)
				{
					if (dwFourCc == entry.fourCc)
					{
						// Convert from DxgiFormat to CompressionFormat
						format = entry.dxgiFormat.ToCompressionFormat();
						alphaChannelHint = AlphaChannelHint.Straight;
						return true;
					}
				}
			}
			else
			{
				// Handle non-FourCC formats using mapping table
				foreach (var entry in MappingTable)
				{
					if (dwFlags == entry.flags &&
					    dwRgbBitCount == entry.bitCount &&
					    dwRBitMask == entry.rMask &&
					    dwGBitMask == entry.gMask &&
					    dwBBitMask == entry.bMask &&
					    dwABitMask == entry.aMask)
					{
						if (entry.dxgiFormat != DxgiFormat.DxgiFormatUnknown)
						{
							// If there's a DXGI format, use that
							format = entry.dxgiFormat.ToCompressionFormat();
						}
						else
						{
							// Otherwise use the D3D format
							format = entry.d3dFormat.ToCompressionFormat();
						}

						// If the format supports alpha, alpha channel is unknown, otherwise we know it's straight alpha.
						alphaChannelHint = format.SupportsAlpha() ? AlphaChannelHint.Unknown : AlphaChannelHint.Straight;

						return true;
					}
				}
			}

			// If we got here, we couldn't determine the format from the header, but we know it's not DX10.
			return true;
		}

		/// <summary>
		/// Attempts to make a Direct3D 9 compatible pixel format structure from a compression format.
		/// </summary>
		/// <param name="format">The <see cref="CompressionFormat"/>.</param>
		/// <param name="alphaChannelHint">Unknown | Straight | Premultiplied</param>
		/// <param name="pixelFormat">A Dx9-compatible filled <see cref="DdsPixelFormat"/> structure, or Dx10 if not possible.</param>
		/// <returns>True if a Direct3D 9 compatible format was found, false if format requires DX10 extension.</returns>
		internal static bool TryMakeDx9(CompressionFormat format, AlphaChannelHint alphaChannelHint,
			out DdsPixelFormat pixelFormat)
		{
			// Initialize the pixel format structure
			pixelFormat = new DdsPixelFormat
			{
				dwSize = 32 // Size of the structure
			};

			// Convert CompressionFormat to DxgiFormat to check formats in the table
			DxgiFormat dxgiFormat = format.ToDxgiFormat();
			D3DFormat d3dFormat = format.ToD3DFormat();

			// First check if we can use a FourCC code with premultiplied distinction
			foreach (var mapping in PremultipliedFourCcTable)
			{
				if (mapping.dxgiFormat == dxgiFormat)
				{
					pixelFormat.dwFlags = PixelFormatFlags.DdpfFourcc;
					// Use premultiplied FourCC if specified
					pixelFormat.dwFourCc = alphaChannelHint == AlphaChannelHint.Premultiplied
						? mapping.premultipliedFourCc
						: mapping.standardFourCc;
					return true;
				}
			}

			// Next, check the normal FourCC table
			foreach (var mapping in FourCcTable)
			{
				if (mapping.dxgiFormat == dxgiFormat)
				{
					pixelFormat.dwFlags = PixelFormatFlags.DdpfFourcc;
					pixelFormat.dwFourCc = mapping.fourCc;
					return true;
				}
			}

			// Try to find a match in the format mapping table for non-FourCC formats
			foreach (var mapping in MappingTable)
			{
				// Check if either the DXGI format matches or the D3D format matches
				if ((mapping.dxgiFormat != DxgiFormat.DxgiFormatUnknown && mapping.dxgiFormat == dxgiFormat) ||
				    (mapping.d3dFormat != D3DFormat.D3DFormatUnknown && mapping.d3dFormat == d3dFormat))
				{
					pixelFormat.dwFlags = mapping.flags;
					pixelFormat.dwRgbBitCount = mapping.bitCount;
					pixelFormat.dwRBitMask = mapping.rMask;
					pixelFormat.dwGBitMask = mapping.gMask;
					pixelFormat.dwBBitMask = mapping.bMask;
					pixelFormat.dwABitMask = mapping.aMask;
					return true;
				}
			}

			// If no appropriate format is found, use DX10 extension
			pixelFormat.dwFlags = PixelFormatFlags.DdpfFourcc;
			pixelFormat.dwFourCc = Dx10;
			return false;
		}

		public bool IsDxt10Format => (dwFlags & PixelFormatFlags.DdpfFourcc) == PixelFormatFlags.DdpfFourcc
		                             && dwFourCc == Dx10;
	}

	public struct DdsHeaderDx10
	{
		public DxgiFormat dxgiFormat;
		public D3D10ResourceDimension resourceDimension;
		public uint miscFlag;
		public uint arraySize;
		public uint miscFlags2;
	}

	/// <summary>
	/// An array element in a texture array, or a face in a cubemap.
	/// </summary>
	public class DdsArrayElement
	{
		public uint Width { get; set; }
		public uint Height { get; set; }
		public uint Depth { get; set; }
		public uint SizeInBytes { get; }
		public DdsMipMap[] MipMaps { get; }

		public DdsArrayElement(uint width, uint height, uint depth, uint sizeInBytes, int numMipMaps)
		{
			Width = width;
			Height = height;
			Depth = depth;
			SizeInBytes = sizeInBytes;
			MipMaps = new DdsMipMap[numMipMaps];
		}
	}

	public class DdsMipMap
	{
		public uint Width { get; set; }
		public uint Height { get; set; }
		public uint Depth { get; set; }
		public uint SizeInBytes { get; }
		public byte[] Data { get; }

		public DdsMipMap(byte[] data, uint width, uint height, uint depth)
		{
			Width = width;
			Height = height;
			Depth = depth;
			SizeInBytes = (uint)data.Length;
			Data = data;
		}
	}
}
