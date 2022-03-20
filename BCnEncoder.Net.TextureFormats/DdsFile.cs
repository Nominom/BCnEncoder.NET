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
		public DdsHeader header;
		public DdsHeaderDx10 dx10Header;
		public List<DdsFace> Faces { get; } = new List<DdsFace>();

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
			return format.ToDxgiFormat() != DxgiFormat.DxgiFormatUnknown;
		}

		/// <inheritdoc />
		public void FromTextureData(BCnTextureData textureData)
			=> FromTextureData(textureData, true);

		/// <inheritdoc cref="FromTextureData(BCnEncoder.Shared.BCnTextureData)"/>/>
		public void FromTextureData(BCnTextureData textureData, bool preferDxt10Header)
		{
			if (!IsSupportedFormat(textureData.Format))
			{
				throw new ArgumentException($"Unsupported format {textureData.Format} in textureData.", nameof(textureData));
			}

			(header, dx10Header) = DdsHeader.InitializeFor(textureData.Width, textureData.Height, textureData.Format, preferDxt10Header);

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
			}

			if ((header.dwFlags & HeaderFlags.DdsdLinearsize) != 0)
			{
				header.dwPitchOrLinearSize = (uint)textureData.TotalSize;
			}

			var uWidth = (uint)textureData.Width;
			var uHeight = (uint)textureData.Height;

			Faces.Clear();

			foreach (var face in textureData.Faces)
			{
				DdsFace ddsFace = new DdsFace(uWidth, uHeight, (uint)face.Mips[0].Data.Length, textureData.NumMips);
				for (var m = 0; m < face.Mips.Length; m++)
				{
					ddsFace.MipMaps[m] = new DdsMipMap(
						face.Mips[m].Data,
						(uint)face.Mips[m].Width,
						(uint)face.Mips[m].Height);
				}
				Faces.Add(ddsFace);
			}
		}

		/// <inheritdoc />
		public BCnTextureData ToTextureData()
		{
			var compressionFormat = GetBCnCompressionFormat();
			var isCubeMap = (header.dwCaps2 & HeaderCaps2.Ddscaps2Cubemap) != 0;
			var data = new BCnTextureData(
				compressionFormat,
				(int)header.dwWidth,
				(int)header.dwHeight,
				(int)header.dwMipMapCount,
				isCubeMap);

			for (var f = 0; f < Faces.Count; f++)
			{
				for (var m = 0; m < data.NumMips; m++)
				{
					if (Faces[f].MipMaps[m].SizeInBytes != data.Faces[f].Mips[m].SizeInBytes)
					{
						throw new TextureFormatException("DdsFile mipmap size different from expected!");
					}

					data.Faces[f].Mips[m].Data = Faces[f].MipMaps[m].Data;
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
				var width = header.dwWidth;
				var height = header.dwHeight;

				for (var face = 0; face < faceCount; face++)
				{
					var format = dx10Format ? dx10Header.dxgiFormat : header.ddsPixelFormat.DxgiFormat;
					var sizeInBytes = GetSizeInBytes(format, width, height);

					Faces.Add(new DdsFace(width, height, sizeInBytes, (int)mipMapCount));

					for (var mip = 0; mip < mipMapCount; mip++)
					{
						MipMapper.CalculateMipLevelSize(
							(int)header.dwWidth,
							(int)header.dwHeight,
							mip,
							out var mipWidth,
							out var mipHeight);

						if (mip > 0) //Calculate new byteSize
						{
							sizeInBytes = GetSizeInBytes(format, (uint)mipWidth, (uint)mipHeight);
						}

						var data = new byte[sizeInBytes];
						br.Read(data);
						Faces[face].MipMaps[mip] = new DdsMipMap(data, (uint)mipWidth, (uint)mipHeight);
					}
				}
			}
		}

		/// <inheritdoc />
		public void WriteToStream(Stream outputStream)
		{
			if (Faces.Count < 1 || Faces[0].MipMaps.Length < 1)
			{
				throw new InvalidOperationException("The DDS structure should have at least 1 mipmap level and 1 Face before writing to file.");
			}

			for (var i = 0; i < Faces.Count; i++)
			{
				if (Faces[i].Width != header.dwWidth || Faces[i].Height != header.dwHeight)
				{
					throw new InvalidOperationException("Faces with different sizes are not supported.");
				}
			}

			var faceCount = Faces.Count;
			var mipCount = (int)header.dwMipMapCount;

			using (var bw = new BinaryWriter(outputStream, Encoding.UTF8, true))
			{
				bw.Write(0x20534444U); // magic 'DDS '

				bw.WriteStruct(header);

				if (header.ddsPixelFormat.IsDxt10Format)
				{
					bw.WriteStruct(dx10Header);
				}

				for (var face = 0; face < faceCount; face++)
				{
					for (var mip = 0; mip < mipCount; mip++)
					{
						bw.Write(Faces[face].MipMaps[mip].Data);
					}
				}
			}
		}

		public CompressionFormat GetBCnCompressionFormat()
		{
			var format = header.ddsPixelFormat.IsDxt10Format ?
				dx10Header.dxgiFormat :
				header.ddsPixelFormat.DxgiFormat;


			switch (format)
			{
				case DxgiFormat.DxgiFormatR8Unorm:
					return CompressionFormat.R8;

				case DxgiFormat.DxgiFormatR8G8Unorm:
					return CompressionFormat.R8G8;

				// HINT: R8G8B8 has no DxgiFormat to convert from
				case DxgiFormat.DxgiFormatR8G8B8A8Unorm:
					return CompressionFormat.Rgba32;

				case DxgiFormat.DxgiFormatB8G8R8A8Unorm:
					return CompressionFormat.Bgra32;
				
				case DxgiFormat.DxgiFormatR32G32B32A32Typeless:
				case DxgiFormat.DxgiFormatR32G32B32A32Float:
					return CompressionFormat.RgbaFloat;

				case DxgiFormat.DxgiFormatR16G16B16A16Typeless:
				case DxgiFormat.DxgiFormatR16G16B16A16Float:
					return CompressionFormat.RgbaHalf;

				case DxgiFormat.DxgiFormatR32G32B32Typeless:
				case DxgiFormat.DxgiFormatR32G32B32Float:
					return CompressionFormat.RgbFloat;

				case DxgiFormat.DxgiFormatBc1Unorm:
				case DxgiFormat.DxgiFormatBc1UnormSrgb:
				case DxgiFormat.DxgiFormatBc1Typeless:
					if (header.ddsPixelFormat.dwFlags.HasFlag(PixelFormatFlags.DdpfAlphaPixels))
						return CompressionFormat.Bc1WithAlpha;

					return CompressionFormat.Bc1;

				case DxgiFormat.DxgiFormatBc2Unorm:
				case DxgiFormat.DxgiFormatBc2UnormSrgb:
				case DxgiFormat.DxgiFormatBc2Typeless:
					return CompressionFormat.Bc2;

				case DxgiFormat.DxgiFormatBc3Unorm:
				case DxgiFormat.DxgiFormatBc3UnormSrgb:
				case DxgiFormat.DxgiFormatBc3Typeless:
					return CompressionFormat.Bc3;

				case DxgiFormat.DxgiFormatBc4Unorm:
				case DxgiFormat.DxgiFormatBc4Snorm:
				case DxgiFormat.DxgiFormatBc4Typeless:
					return CompressionFormat.Bc4;

				case DxgiFormat.DxgiFormatBc5Unorm:
				case DxgiFormat.DxgiFormatBc5Snorm:
				case DxgiFormat.DxgiFormatBc5Typeless:
					return CompressionFormat.Bc5;

				case DxgiFormat.DxgiFormatBc6HTypeless:
				case DxgiFormat.DxgiFormatBc6HUf16:
					return CompressionFormat.Bc6U;

				case DxgiFormat.DxgiFormatBc6HSf16:
					return CompressionFormat.Bc6S;

				case DxgiFormat.DxgiFormatBc7Unorm:
				case DxgiFormat.DxgiFormatBc7UnormSrgb:
				case DxgiFormat.DxgiFormatBc7Typeless:
					return CompressionFormat.Bc7;

				case DxgiFormat.DxgiFormatAtcExt:
					return CompressionFormat.Atc;

				case DxgiFormat.DxgiFormatAtcExplicitAlphaExt:
					return CompressionFormat.AtcExplicitAlpha;

				case DxgiFormat.DxgiFormatAtcInterpolatedAlphaExt:
					return CompressionFormat.AtcInterpolatedAlpha;

				default:
					return CompressionFormat.Unknown;
			}
		}

		private static uint GetSizeInBytes(DxgiFormat format, uint width, uint height)
		{
			uint sizeInBytes;
			if (format.IsCompressedFormat())
			{
				sizeInBytes = (uint)ImageToBlocks.CalculateNumOfBlocks((int)width, (int)height);
				sizeInBytes *= (uint)format.GetByteSize();
			}
			else
			{
				sizeInBytes = width * height;
				sizeInBytes = (uint)(sizeInBytes * format.GetByteSize());
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

		public static (DdsHeader, DdsHeaderDx10) InitializeDx10Header(int width, int height, DxgiFormat format, bool preferDxt10Header)
		{
			var header = new DdsHeader();
			var dxt10Header = new DdsHeaderDx10();

			header.dwSize = 124;
			header.dwFlags = HeaderFlags.Required | HeaderFlags.DdsdMipmapcount;
			header.dwWidth = (uint)width;
			header.dwHeight = (uint)height;
			header.dwDepth = 1;
			header.dwMipMapCount = 1;
			header.dwCaps = HeaderCaps.DdscapsTexture;

			if (!format.IsCompressedFormat())
			{
				header.dwFlags |= HeaderFlags.DdsdPitch;
				header.dwPitchOrLinearSize = (uint)((width * format.GetByteSize() * 8 + 7) / 8);
			}
			else
			{
				header.dwFlags |= HeaderFlags.DdsdLinearsize;
				header.dwPitchOrLinearSize = 0;
			}

			if (format == DxgiFormat.DxgiFormatR8Unorm)
			{
				header.ddsPixelFormat = new DdsPixelFormat()
				{
					dwSize = 32,
					dwFlags = PixelFormatFlags.DdpfLuminance,
					dwRgbBitCount = 8,
					dwRBitMask = 0xFF
				};
			}
			else if (format == DxgiFormat.DxgiFormatR8G8Unorm)
			{
				header.ddsPixelFormat = new DdsPixelFormat()
				{
					dwSize = 32,
					dwFlags = PixelFormatFlags.DdpfLuminance | PixelFormatFlags.DdpfAlphaPixels,
					dwRgbBitCount = 16,
					dwRBitMask = 0xFF,
					dwABitMask = 0xFF00
				};
			}
			else if (format == DxgiFormat.DxgiFormatR8G8B8A8Unorm)
			{
				header.ddsPixelFormat = new DdsPixelFormat()
				{
					dwSize = 32,
					dwFlags = PixelFormatFlags.DdpfRgb | PixelFormatFlags.DdpfAlphaPixels,
					dwRgbBitCount = 32,
					dwRBitMask = 0xFF,
					dwGBitMask = 0xFF00,
					dwBBitMask = 0xFF0000,
					dwABitMask = 0xFF000000,
				};
			}
			else if (format == DxgiFormat.DxgiFormatB8G8R8A8Unorm)
			{
				header.ddsPixelFormat = new DdsPixelFormat()
				{
					dwSize = 32,
					dwFlags = PixelFormatFlags.DdpfRgb | PixelFormatFlags.DdpfAlphaPixels,
					dwRgbBitCount = 32,
					dwRBitMask = 0xFF0000,
					dwGBitMask = 0xFF00,
					dwBBitMask = 0xFF,
					dwABitMask = 0xFF000000,
				};
			}
			else
			{
				if (preferDxt10Header)
				{
					// ATC formats cannot be written to DXT10 header due to lack of a DxgiFormat enum
					switch (format)
					{
						case DxgiFormat.DxgiFormatAtcExt:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Atc
							};
							break;

						case DxgiFormat.DxgiFormatAtcExplicitAlphaExt:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Atci
							};
							break;

						case DxgiFormat.DxgiFormatAtcInterpolatedAlphaExt:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Atca
							};
							break;

						default:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Dx10
							};
							dxt10Header.arraySize = 1;
							dxt10Header.dxgiFormat = format;
							dxt10Header.resourceDimension = D3D10ResourceDimension.D3D10ResourceDimensionTexture2D;
							break;
					}
				}
				else
				{
					switch (format)
					{
						case DxgiFormat.DxgiFormatBc1Unorm:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Dxt1
							};
							break;

						case DxgiFormat.DxgiFormatBc2Unorm:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Dxt3
							};
							break;

						case DxgiFormat.DxgiFormatBc3Unorm:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Dxt5
							};
							break;

						case DxgiFormat.DxgiFormatBc4Unorm:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Bc4U
							};
							break;

						case DxgiFormat.DxgiFormatBc5Unorm:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Ati2
							};
							break;

						case DxgiFormat.DxgiFormatAtcExt:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Atc
							};
							break;

						case DxgiFormat.DxgiFormatAtcExplicitAlphaExt:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Atci
							};
							break;

						case DxgiFormat.DxgiFormatAtcInterpolatedAlphaExt:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Atca
							};
							break;

						default:
							header.ddsPixelFormat = new DdsPixelFormat
							{
								dwSize = 32,
								dwFlags = PixelFormatFlags.DdpfFourcc,
								dwFourCc = DdsPixelFormat.Dx10
							};
							dxt10Header.arraySize = 1;
							dxt10Header.dxgiFormat = format;
							dxt10Header.resourceDimension = D3D10ResourceDimension.D3D10ResourceDimensionTexture2D;
							break;
					}
				}
			}

			return (header, dxt10Header);
		}

		public static (DdsHeader, DdsHeaderDx10) InitializeFor(int width, int height, CompressionFormat format, bool preferDxt10Header)
		{
			DdsHeader header;
			DdsHeaderDx10 dx10Header;

			var dxgiFormat = format.ToDxgiFormat();

			if (dxgiFormat == DxgiFormat.DxgiFormatUnknown)
			{
				throw new ArgumentException("The requested format is not supported!", nameof(format));
			}

			(header, dx10Header) = InitializeDx10Header(width, height, dxgiFormat, preferDxt10Header);

			if (format == CompressionFormat.Bc1WithAlpha)
			{
				header.ddsPixelFormat.dwFlags |= PixelFormatFlags.DdpfAlphaPixels;
			}

			return (header, dx10Header);
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

		private static uint MakeFourCc(char c0, char c1, char c2, char c3)
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

		public DxgiFormat DxgiFormat
		{
			get
			{
				if (dwFlags.HasFlag(PixelFormatFlags.DdpfFourcc))
				{
					if (dwFourCc == Dxt1) return DxgiFormat.DxgiFormatBc1Unorm;
					if (dwFourCc == Dxt2 || dwFourCc == Dxt3) return DxgiFormat.DxgiFormatBc2Unorm;
					if (dwFourCc == Dxt4 || dwFourCc == Dxt5) return DxgiFormat.DxgiFormatBc3Unorm;
					if (dwFourCc == Ati1 || dwFourCc == Bc4S || dwFourCc == Bc4U) return DxgiFormat.DxgiFormatBc4Unorm;
					if (dwFourCc == Ati2 || dwFourCc == Bc5S || dwFourCc == Bc5U) return DxgiFormat.DxgiFormatBc5Unorm;
					if (dwFourCc == Atc) return DxgiFormat.DxgiFormatAtcExt;
					if (dwFourCc == Atci) return DxgiFormat.DxgiFormatAtcExplicitAlphaExt;
					if (dwFourCc == Atca) return DxgiFormat.DxgiFormatAtcInterpolatedAlphaExt;
				}
				else
				{
					if (dwFlags.HasFlag(PixelFormatFlags.DdpfRgb)) // RGB/A
					{
						if (dwFlags.HasFlag(PixelFormatFlags.DdpfAlphaPixels)) //RGBA
						{
							if (dwRgbBitCount == 32)
							{
								if (dwRBitMask == 0xff && dwGBitMask == 0xff00 && dwBBitMask == 0xff0000 &&
									dwABitMask == 0xff000000)
								{
									return DxgiFormat.DxgiFormatR8G8B8A8Unorm;
								}

								if (dwRBitMask == 0xff0000 && dwGBitMask == 0xff00 && dwBBitMask == 0xff &&
									dwABitMask == 0xff000000)
								{
									return DxgiFormat.DxgiFormatB8G8R8A8Unorm;
								}
							}
						}
						else //RGB
						{
							if (dwRgbBitCount == 32)
							{
								if (dwRBitMask == 0xff0000 && dwGBitMask == 0xff00 && dwBBitMask == 0xff)
								{
									return DxgiFormat.DxgiFormatB8G8R8X8Unorm;
								}
							}
						}
					}
					else if (dwFlags.HasFlag(PixelFormatFlags.DdpfLuminance)) // R/RG
					{
						if (dwFlags.HasFlag(PixelFormatFlags.DdpfAlphaPixels)) // RG
						{
							if (dwRgbBitCount == 16)
							{
								if (dwRBitMask == 0xff && dwABitMask == 0xff00)
								{
									return DxgiFormat.DxgiFormatR8G8Unorm;
								}
							}
						}
						else // Luminance only
						{
							if (dwRgbBitCount == 8)
							{
								if (dwRBitMask == 0xff)
								{
									return DxgiFormat.DxgiFormatR8Unorm;
								}
							}
						}
					}
				}
				return DxgiFormat.DxgiFormatUnknown;
			}
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

	public class DdsFace
	{
		public uint Width { get; set; }
		public uint Height { get; set; }
		public uint SizeInBytes { get; }
		public DdsMipMap[] MipMaps { get; }

		public DdsFace(uint width, uint height, uint sizeInBytes, int numMipMaps)
		{
			Width = width;
			Height = height;
			SizeInBytes = sizeInBytes;
			MipMaps = new DdsMipMap[numMipMaps];
		}
	}

	public class DdsMipMap
	{
		public uint Width { get; set; }
		public uint Height { get; set; }
		public uint SizeInBytes { get; }
		public byte[] Data { get; }
		public DdsMipMap(byte[] data, uint width, uint height)
		{
			Width = width;
			Height = height;
			SizeInBytes = (uint)data.Length;
			Data = data;
		}
	}
}
