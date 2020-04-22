using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BCnEnc.Net.Shared
{
	public class DdsFile
	{
		public DdsHeader Header;
		public DdsHeaderDxt10 Dxt10Header;
		public List<DdsFace> Faces { get; } = new List<DdsFace>();

		public DdsFile() { }
		public DdsFile(DdsHeader header)
		{
			this.Header = header;
		}

		public DdsFile(DdsHeader header, DdsHeaderDxt10 dxt10Header)
		{
			this.Header = header;
			this.Dxt10Header = dxt10Header;
		}

		public static DdsFile Load(Stream s)
		{
			using (BinaryReader br = new BinaryReader(s, Encoding.UTF8, true))
			{
				var magic = br.ReadUInt32();
				if (magic != 0x20534444U)
				{
					throw new FormatException("The file does not contain a dds file.");
				}
				DdsHeader header = br.ReadStruct<DdsHeader>();
				DdsHeaderDxt10 dxt10Header = default;
				if (header.dwSize != 124)
				{
					throw new FormatException("The file header contains invalid dwSize.");
				}

				bool dxt10Format = header.ddsPixelFormat.IsDxt10Format;

				DdsFile output;

				if (dxt10Format)
				{
					dxt10Header = br.ReadStruct<DdsHeaderDxt10>();
					output = new DdsFile(header, dxt10Header);
				}
				else
				{
					output = new DdsFile(header);
				}

				uint mipMapCount = (header.dwCaps & HeaderCaps.DDSCAPS_MIPMAP) != 0 ? header.dwMipMapCount : 1;
				uint faceCount = ((header.dwCaps2 & HeaderCaps2.DDSCAPS2_CUBEMAP) != 0) ? (uint)6 : (uint)1;
				uint width = header.dwWidth;
				uint height = header.dwHeight;

				for (int face = 0; face < faceCount; face++)
				{
					uint sizeInBytes = Math.Max(1, ((width + 3) / 4)) * Math.Max(1, ((height + 3) / 4));
					if (!dxt10Format)
					{
						if (header.ddsPixelFormat.IsDxt1To5CompressedFormat)
						{
							if (header.ddsPixelFormat.DxgiFormat == DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM)
							{
								sizeInBytes *= 8;
							}
							else
							{
								sizeInBytes *= 16;
							}
						}
						else
						{
							sizeInBytes = header.dwPitchOrLinearSize * height;
						}
					}
					else if (dxt10Header.dxgiFormat.IsCompressedFormat())
					{
						sizeInBytes = (uint)(sizeInBytes * dxt10Header.dxgiFormat.GetByteSize());
					}
					else
					{
						sizeInBytes = header.dwPitchOrLinearSize * height;
					}
					output.Faces.Add(new DdsFace(width, height, sizeInBytes, (int)mipMapCount));

					for (int mip = 0; mip < mipMapCount; mip++)
					{
						uint mipWidth = header.dwWidth / (uint)(Math.Pow(2, mip));
						uint mipHeight = header.dwHeight / (uint)(Math.Pow(2, mip));

						if (mip > 0) //Calculate new byteSize
						{
							sizeInBytes = Math.Max(1, ((mipWidth + 3) / 4)) * Math.Max(1, ((mipHeight + 3) / 4));
							if (!dxt10Format)
							{
								if (header.ddsPixelFormat.IsDxt1To5CompressedFormat)
								{
									if (header.ddsPixelFormat.DxgiFormat == DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM)
									{
										sizeInBytes *= 8;
									}
									else
									{
										sizeInBytes *= 16;
									}
								}
								else
								{
									sizeInBytes = header.dwPitchOrLinearSize / (uint)(Math.Pow(2, mip)) * mipHeight;
								}
							}
							else if (dxt10Header.dxgiFormat.IsCompressedFormat())
							{
								sizeInBytes = (uint)(sizeInBytes * dxt10Header.dxgiFormat.GetByteSize());
							}
							else
							{
								sizeInBytes = header.dwPitchOrLinearSize / (uint)(Math.Pow(2, mip)) * mipHeight;
							}
						}
						byte[] data = new byte[sizeInBytes];
						br.Read(data);
						output.Faces[face].MipMaps[mip] = new DdsMipMap(data, mipWidth, mipHeight);
					}
				}

				return output;
			}
		}

		public void Write(Stream outputStream)
		{
			if (Faces.Count < 1 || Faces[0].MipMaps.Length < 1)
			{
				throw new InvalidOperationException("The DDS structure should have at least 1 mipmap level and 1 Face before writing to file.");
			}

			Header.dwFlags |= HeaderFlags.REQUIRED;

			Header.dwMipMapCount = (uint)Faces[0].MipMaps.Length;
			if (Header.dwMipMapCount > 1) // MipMaps
			{
				Header.dwCaps |= HeaderCaps.DDSCAPS_MIPMAP | HeaderCaps.DDSCAPS_COMPLEX;
			}
			if (Faces.Count == 6) // CubeMap
			{
				Header.dwCaps |= HeaderCaps.DDSCAPS_COMPLEX;
				Header.dwCaps2 |= HeaderCaps2.DDSCAPS2_CUBEMAP |
								  HeaderCaps2.DDSCAPS2_CUBEMAP_POSITIVEX |
								  HeaderCaps2.DDSCAPS2_CUBEMAP_NEGATIVEX |
								  HeaderCaps2.DDSCAPS2_CUBEMAP_POSITIVEY |
								  HeaderCaps2.DDSCAPS2_CUBEMAP_NEGATIVEY |
								  HeaderCaps2.DDSCAPS2_CUBEMAP_POSITIVEZ |
								  HeaderCaps2.DDSCAPS2_CUBEMAP_NEGATIVEZ;
			}

			Header.dwWidth = Faces[0].Width;
			Header.dwHeight = Faces[0].Height;

			for (int i = 0; i < Faces.Count; i++)
			{
				if (Faces[i].Width != Header.dwWidth || Faces[i].Height != Header.dwHeight)
				{
					throw new InvalidOperationException("Faces with different sizes are not supported.");
				}
			}

			int faceCount = Faces.Count;
			int mipCount = (int)Header.dwMipMapCount;

			using (BinaryWriter bw = new BinaryWriter(outputStream, Encoding.UTF8, true))
			{
				bw.Write(0x20534444U); // magic 'DDS '

				bw.WriteStruct(Header);

				if (Header.ddsPixelFormat.IsDxt10Format)
				{
					bw.WriteStruct(Dxt10Header);
				}

				for (int face = 0; face < faceCount; face++)
				{
					for (int mip = 0; mip < mipCount; mip++)
					{
						bw.Write(Faces[face].MipMaps[mip].Data);
					}
				}
			}
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

		public static (DdsHeader, DdsHeaderDxt10) InitializeCompressed(int width, int height, DXGI_FORMAT format)
		{
			DdsHeader header = new DdsHeader();
			DdsHeaderDxt10 dxt10Header = new DdsHeaderDxt10();

			header.dwSize = 124;
			header.dwFlags = HeaderFlags.REQUIRED;
			header.dwWidth = (uint)width;
			header.dwHeight = (uint)height;
			header.dwDepth = 1;
			header.dwMipMapCount = 1;
			header.dwCaps = HeaderCaps.DDSCAPS_TEXTURE;

			if (format == DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM)
			{
				header.ddsPixelFormat = new DdsPixelFormat()
				{
					dwSize = 32,
					dwFlags = PixelFormatFlags.DDPF_FOURCC,
					dwFourCC = DdsPixelFormat.DXT1
				};
			}
			else if (format == DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM)
			{
				header.ddsPixelFormat = new DdsPixelFormat()
				{
					dwSize = 32,
					dwFlags = PixelFormatFlags.DDPF_FOURCC,
					dwFourCC = DdsPixelFormat.DXT3
				};
			}
			else if (format == DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM)
			{
				header.ddsPixelFormat = new DdsPixelFormat()
				{
					dwSize = 32,
					dwFlags = PixelFormatFlags.DDPF_FOURCC,
					dwFourCC = DdsPixelFormat.DXT5
				};
			}
			else
			{
				header.ddsPixelFormat = new DdsPixelFormat()
				{
					dwSize = 32,
					dwFlags = PixelFormatFlags.DDPF_FOURCC,
					dwFourCC = DdsPixelFormat.DX10
				};
				dxt10Header.arraySize = 1;
				dxt10Header.dxgiFormat = format;
				dxt10Header.resourceDimension = D3D10_RESOURCE_DIMENSION.D3D10_RESOURCE_DIMENSION_TEXTURE2D;
			}

			return (header, dxt10Header);
		}

		public static DdsHeader InitializeUncompressed(int width, int height, DXGI_FORMAT format)
		{
			DdsHeader header = new DdsHeader();

			header.dwSize = 124;
			header.dwFlags = HeaderFlags.REQUIRED | HeaderFlags.DDSD_PITCH;
			header.dwWidth = (uint)width;
			header.dwHeight = (uint)height;
			header.dwDepth = 1;
			header.dwMipMapCount = 1;
			header.dwCaps = HeaderCaps.DDSCAPS_TEXTURE;

			if (format == DXGI_FORMAT.DXGI_FORMAT_R8_UNORM)
			{
				header.ddsPixelFormat = new DdsPixelFormat()
				{
					dwSize = 32,
					dwFlags = PixelFormatFlags.DDPF_LUMINANCE,
					dwRGBBitCount = 8,
					dwRBitMask = 0xFF
				};
				header.dwPitchOrLinearSize = (uint)((width * 8 + 7) / 8);
			}
			else if (format == DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM)
			{
				header.ddsPixelFormat = new DdsPixelFormat()
				{
					dwSize = 32,
					dwFlags = PixelFormatFlags.DDPF_LUMINANCE | PixelFormatFlags.DDPF_ALPHAPIXELS,
					dwRGBBitCount = 16,
					dwRBitMask = 0xFF,
					dwGBitMask = 0xFF00
				};
				header.dwPitchOrLinearSize = (uint)((width * 16 + 7) / 8);
			}
			else if (format == DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM)
			{
				header.ddsPixelFormat = new DdsPixelFormat()
				{
					dwSize = 32,
					dwFlags = PixelFormatFlags.DDPF_RGB | PixelFormatFlags.DDPF_ALPHAPIXELS,
					dwRGBBitCount = 32,
					dwRBitMask = 0xFF,
					dwGBitMask = 0xFF00,
					dwBBitMask = 0xFF0000,
					dwABitMask = 0xFF000000,
				};
				header.dwPitchOrLinearSize = (uint)((width * 32 + 7) / 8);
			}
			else
			{
				throw new NotImplementedException("This format is not implemented in this method");
			}

			return header;
		}
	}

	public struct DdsPixelFormat
	{
		public const uint DXT1 = 0x31545844U;
		public const uint DXT2 = 0x32545844U;
		public const uint DXT3 = 0x33545844U;
		public const uint DXT4 = 0x34545844U;
		public const uint DXT5 = 0x35545844U;
		public const uint DX10 = 0x30315844U;

		public uint dwSize;
		public PixelFormatFlags dwFlags;
		public uint dwFourCC;
		public uint dwRGBBitCount;
		public uint dwRBitMask;
		public uint dwGBitMask;
		public uint dwBBitMask;
		public uint dwABitMask;

		public DXGI_FORMAT DxgiFormat
		{
			get
			{
				if ((dwFlags & PixelFormatFlags.DDPF_FOURCC) != 0)
				{
					switch (dwFourCC)
					{
						case 0x31545844U:
							return DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM;
						case 0x33545844U:
							return DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM;
						case 0x35545844U:
							return DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM;
					}
				}
				else
				{
					if ((dwFlags & PixelFormatFlags.DDPF_RGB) != 0) // RGB/A
					{
						if ((dwFlags & PixelFormatFlags.DDPF_ALPHAPIXELS) != 0) //RGBA
						{
							if (dwRGBBitCount == 32)
							{
								if (dwRBitMask == 0xff && dwGBitMask == 0xff00 && dwBBitMask == 0xff0000 &&
									dwABitMask == 0xff000000)
								{
									return DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;
								}
								else if (dwRBitMask == 0x0000ff && dwGBitMask == 0xff00 && dwBBitMask == 0xff &&
										 dwABitMask == 0xff000000)
								{
									return DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM;
								}
							}
						}
						else //RGB
						{
							if (dwRGBBitCount == 32)
							{
								if (dwRBitMask == 0x0000ff && dwGBitMask == 0xff00 && dwBBitMask == 0xff)
								{
									return DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM;
								}
							}
						}
					}
					else if ((dwFlags & PixelFormatFlags.DDPF_LUMINANCE) != 0) // R/RG
					{
						if ((dwFlags & PixelFormatFlags.DDPF_ALPHAPIXELS) != 0) // RG
						{
							if (dwRGBBitCount == 16)
							{
								if (dwRBitMask == 0x0000ff && dwGBitMask == 0xff00)
								{
									return DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM;
								}
							}
						}
						else // Luminance only
						{
							if (dwRGBBitCount == 8)
							{
								if (dwRBitMask == 0x0000ff && dwGBitMask == 0xff00)
								{
									return DXGI_FORMAT.DXGI_FORMAT_R8_UNORM;
								}
							}
						}
					}
				}
				return DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;
			}
		}

		public bool IsDxt10Format => ((dwFlags & PixelFormatFlags.DDPF_FOURCC) == PixelFormatFlags.DDPF_FOURCC)
									 && dwFourCC == DX10;

		public bool IsDxt1To5CompressedFormat => ((dwFlags & PixelFormatFlags.DDPF_FOURCC) == PixelFormatFlags.DDPF_FOURCC)
									 && (dwFourCC == DXT1
										 || dwFourCC == DXT2
										 || dwFourCC == DXT3
										 || dwFourCC == DXT4
										 || dwFourCC == DXT5);
	}

	public struct DdsHeaderDxt10
	{
		public DXGI_FORMAT dxgiFormat;
		public D3D10_RESOURCE_DIMENSION resourceDimension;
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

	/// <summary>
	/// Flags to indicate which members contain valid data.
	/// </summary>
	[Flags]
	public enum HeaderFlags : uint
	{
		/// <summary>
		/// Required in every .dds file.
		/// </summary>
		DDSD_CAPS = 0x1,
		/// <summary>
		/// Required in every .dds file.
		/// </summary>
		DDSD_HEIGHT = 0x2,
		/// <summary>
		/// Required in every .dds file.
		/// </summary>
		DDSD_WIDTH = 0x4,
		/// <summary>
		/// Required when pitch is provided for an uncompressed texture.
		/// </summary>
		DDSD_PITCH = 0x8,
		/// <summary>
		/// Required in every .dds file.
		/// </summary>
		DDSD_PIXELFORMAT = 0x1000,
		/// <summary>
		/// Required in a mipmapped texture.
		/// </summary>
		DDSD_MIPMAPCOUNT = 0x20000,
		/// <summary>
		/// Required when pitch is provided for a compressed texture.
		/// </summary>
		DDSD_LINEARSIZE = 0x80000,
		/// <summary>
		/// Required in a depth texture.
		/// </summary>
		DDSD_DEPTH = 0x800000,

		REQUIRED = DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT
	}

	/// <summary>
	/// Specifies the complexity of the surfaces stored.
	/// </summary>
	[Flags]
	public enum HeaderCaps : uint
	{
		/// <summary>
		/// Optional; must be used on any file that contains more than one surface (a mipmap, a cubic environment map, or mipmapped volume texture).
		/// </summary>
		DDSCAPS_COMPLEX = 0x8,
		/// <summary>
		/// Optional; should be used for a mipmap.
		/// </summary>
		DDSCAPS_MIPMAP = 0x400000,
		/// <summary>
		/// Required
		/// </summary>
		DDSCAPS_TEXTURE = 0x1000
	}

	/// <summary>
	/// Additional detail about the surfaces stored.
	/// </summary>
	[Flags]
	public enum HeaderCaps2 : uint
	{
		/// <summary>
		/// Required for a cube map.
		/// </summary>
		DDSCAPS2_CUBEMAP = 0x200,
		/// <summary>
		/// Required when these surfaces are stored in a cube map.
		/// </summary>
		DDSCAPS2_CUBEMAP_POSITIVEX = 0x400,
		/// <summary>
		/// Required when these surfaces are stored in a cube map.
		/// </summary>
		DDSCAPS2_CUBEMAP_NEGATIVEX = 0x800,
		/// <summary>
		/// Required when these surfaces are stored in a cube map.
		/// </summary>
		DDSCAPS2_CUBEMAP_POSITIVEY = 0x1000,
		/// <summary>
		/// Required when these surfaces are stored in a cube map.
		/// </summary>
		DDSCAPS2_CUBEMAP_NEGATIVEY = 0x2000,
		/// <summary>
		/// Required when these surfaces are stored in a cube map.
		/// </summary>
		DDSCAPS2_CUBEMAP_POSITIVEZ = 0x4000,
		/// <summary>
		/// Required when these surfaces are stored in a cube map.
		/// </summary>
		DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x8000,
		/// <summary>
		/// Required for a volume texture.
		/// </summary>
		DDSCAPS2_VOLUME = 0x200000
	}

	[Flags]
	public enum PixelFormatFlags : uint
	{
		/// <summary>
		/// Texture contains alpha data; dwRGBAlphaBitMask contains valid data.
		/// </summary>
		DDPF_ALPHAPIXELS = 0x1,
		/// <summary>
		/// Used in some older DDS files for alpha channel only uncompressed data (dwRGBBitCount contains the alpha channel bitcount; dwABitMask contains valid data)
		/// </summary>
		DDPF_ALPHA = 0x2,
		/// <summary>
		/// Texture contains compressed RGB data; dwFourCC contains valid data.
		/// </summary>
		DDPF_FOURCC = 0x4,
		/// <summary>
		/// Texture contains uncompressed RGB data; dwRGBBitCount and the RGB masks (dwRBitMask, dwGBitMask, dwBBitMask) contain valid data.
		/// </summary>
		DDPF_RGB = 0x40,
		/// <summary>
		/// Used in some older DDS files for YUV uncompressed data (dwRGBBitCount contains the YUV bit count; dwRBitMask contains the Y mask, dwGBitMask contains the U mask, dwBBitMask contains the V mask)
		/// </summary>
		DDPF_YUV = 0x200,
		/// <summary>
		/// Used in some older DDS files for single channel color uncompressed data (dwRGBBitCount contains the luminance channel bit count; dwRBitMask contains the channel mask). Can be combined with DDPF_ALPHAPIXELS for a two channel DDS file.
		/// </summary>
		DDPF_LUMINANCE = 0x20000
	}

	public enum D3D10_RESOURCE_DIMENSION : uint
	{
		D3D10_RESOURCE_DIMENSION_UNKNOWN,
		D3D10_RESOURCE_DIMENSION_BUFFER,
		D3D10_RESOURCE_DIMENSION_TEXTURE1D,
		D3D10_RESOURCE_DIMENSION_TEXTURE2D,
		D3D10_RESOURCE_DIMENSION_TEXTURE3D
	};

	public enum DXGI_FORMAT : uint
	{
		DXGI_FORMAT_UNKNOWN,
		DXGI_FORMAT_R32G32B32A32_TYPELESS,
		DXGI_FORMAT_R32G32B32A32_FLOAT,
		DXGI_FORMAT_R32G32B32A32_UINT,
		DXGI_FORMAT_R32G32B32A32_SINT,
		DXGI_FORMAT_R32G32B32_TYPELESS,
		DXGI_FORMAT_R32G32B32_FLOAT,
		DXGI_FORMAT_R32G32B32_UINT,
		DXGI_FORMAT_R32G32B32_SINT,
		DXGI_FORMAT_R16G16B16A16_TYPELESS,
		DXGI_FORMAT_R16G16B16A16_FLOAT,
		DXGI_FORMAT_R16G16B16A16_UNORM,
		DXGI_FORMAT_R16G16B16A16_UINT,
		DXGI_FORMAT_R16G16B16A16_SNORM,
		DXGI_FORMAT_R16G16B16A16_SINT,
		DXGI_FORMAT_R32G32_TYPELESS,
		DXGI_FORMAT_R32G32_FLOAT,
		DXGI_FORMAT_R32G32_UINT,
		DXGI_FORMAT_R32G32_SINT,
		DXGI_FORMAT_R32G8X24_TYPELESS,
		DXGI_FORMAT_D32_FLOAT_S8X24_UINT,
		DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS,
		DXGI_FORMAT_X32_TYPELESS_G8X24_UINT,
		DXGI_FORMAT_R10G10B10A2_TYPELESS,
		DXGI_FORMAT_R10G10B10A2_UNORM,
		DXGI_FORMAT_R10G10B10A2_UINT,
		DXGI_FORMAT_R11G11B10_FLOAT,
		DXGI_FORMAT_R8G8B8A8_TYPELESS,
		DXGI_FORMAT_R8G8B8A8_UNORM,
		DXGI_FORMAT_R8G8B8A8_UNORM_SRGB,
		DXGI_FORMAT_R8G8B8A8_UINT,
		DXGI_FORMAT_R8G8B8A8_SNORM,
		DXGI_FORMAT_R8G8B8A8_SINT,
		DXGI_FORMAT_R16G16_TYPELESS,
		DXGI_FORMAT_R16G16_FLOAT,
		DXGI_FORMAT_R16G16_UNORM,
		DXGI_FORMAT_R16G16_UINT,
		DXGI_FORMAT_R16G16_SNORM,
		DXGI_FORMAT_R16G16_SINT,
		DXGI_FORMAT_R32_TYPELESS,
		DXGI_FORMAT_D32_FLOAT,
		DXGI_FORMAT_R32_FLOAT,
		DXGI_FORMAT_R32_UINT,
		DXGI_FORMAT_R32_SINT,
		DXGI_FORMAT_R24G8_TYPELESS,
		DXGI_FORMAT_D24_UNORM_S8_UINT,
		DXGI_FORMAT_R24_UNORM_X8_TYPELESS,
		DXGI_FORMAT_X24_TYPELESS_G8_UINT,
		DXGI_FORMAT_R8G8_TYPELESS,
		DXGI_FORMAT_R8G8_UNORM,
		DXGI_FORMAT_R8G8_UINT,
		DXGI_FORMAT_R8G8_SNORM,
		DXGI_FORMAT_R8G8_SINT,
		DXGI_FORMAT_R16_TYPELESS,
		DXGI_FORMAT_R16_FLOAT,
		DXGI_FORMAT_D16_UNORM,
		DXGI_FORMAT_R16_UNORM,
		DXGI_FORMAT_R16_UINT,
		DXGI_FORMAT_R16_SNORM,
		DXGI_FORMAT_R16_SINT,
		DXGI_FORMAT_R8_TYPELESS,
		DXGI_FORMAT_R8_UNORM,
		DXGI_FORMAT_R8_UINT,
		DXGI_FORMAT_R8_SNORM,
		DXGI_FORMAT_R8_SINT,
		DXGI_FORMAT_A8_UNORM,
		DXGI_FORMAT_R1_UNORM,
		DXGI_FORMAT_R9G9B9E5_SHAREDEXP,
		DXGI_FORMAT_R8G8_B8G8_UNORM,
		DXGI_FORMAT_G8R8_G8B8_UNORM,
		DXGI_FORMAT_BC1_TYPELESS,
		DXGI_FORMAT_BC1_UNORM,
		DXGI_FORMAT_BC1_UNORM_SRGB,
		DXGI_FORMAT_BC2_TYPELESS,
		DXGI_FORMAT_BC2_UNORM,
		DXGI_FORMAT_BC2_UNORM_SRGB,
		DXGI_FORMAT_BC3_TYPELESS,
		DXGI_FORMAT_BC3_UNORM,
		DXGI_FORMAT_BC3_UNORM_SRGB,
		DXGI_FORMAT_BC4_TYPELESS,
		DXGI_FORMAT_BC4_UNORM,
		DXGI_FORMAT_BC4_SNORM,
		DXGI_FORMAT_BC5_TYPELESS,
		DXGI_FORMAT_BC5_UNORM,
		DXGI_FORMAT_BC5_SNORM,
		DXGI_FORMAT_B5G6R5_UNORM,
		DXGI_FORMAT_B5G5R5A1_UNORM,
		DXGI_FORMAT_B8G8R8A8_UNORM,
		DXGI_FORMAT_B8G8R8X8_UNORM,
		DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM,
		DXGI_FORMAT_B8G8R8A8_TYPELESS,
		DXGI_FORMAT_B8G8R8A8_UNORM_SRGB,
		DXGI_FORMAT_B8G8R8X8_TYPELESS,
		DXGI_FORMAT_B8G8R8X8_UNORM_SRGB,
		DXGI_FORMAT_BC6H_TYPELESS,
		DXGI_FORMAT_BC6H_UF16,
		DXGI_FORMAT_BC6H_SF16,
		DXGI_FORMAT_BC7_TYPELESS,
		DXGI_FORMAT_BC7_UNORM,
		DXGI_FORMAT_BC7_UNORM_SRGB,
		DXGI_FORMAT_AYUV,
		DXGI_FORMAT_Y410,
		DXGI_FORMAT_Y416,
		DXGI_FORMAT_NV12,
		DXGI_FORMAT_P010,
		DXGI_FORMAT_P016,
		DXGI_FORMAT_420_OPAQUE,
		DXGI_FORMAT_YUY2,
		DXGI_FORMAT_Y210,
		DXGI_FORMAT_Y216,
		DXGI_FORMAT_NV11,
		DXGI_FORMAT_AI44,
		DXGI_FORMAT_IA44,
		DXGI_FORMAT_P8,
		DXGI_FORMAT_A8P8,
		DXGI_FORMAT_B4G4R4A4_UNORM,
		DXGI_FORMAT_P208,
		DXGI_FORMAT_V208,
		DXGI_FORMAT_V408,
		DXGI_FORMAT_FORCE_UINT
	};

	public static class DxgiFormatExtensions
	{
		public static int GetByteSize(this DXGI_FORMAT format)
		{
			switch (format)
			{
				case DXGI_FORMAT.DXGI_FORMAT_UNKNOWN:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_TYPELESS:
					return 4 * 4;
				case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT:
					return 4 * 4;
				case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_UINT:
					return 4 * 4;
				case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_SINT:
					return 4 * 4;
				case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_TYPELESS:
					return 4 * 3;
				case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_FLOAT:
					return 4 * 3;
				case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_UINT:
					return 4 * 3;
				case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_SINT:
					return 4 * 3;
				case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_TYPELESS:
					return 4 * 2;
				case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT:
					return 4 * 2;
				case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM:
					return 4 * 2;
				case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UINT:
					return 4 * 2;
				case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SNORM:
					return 4 * 2;
				case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SINT:
					return 4 * 2;
				case DXGI_FORMAT.DXGI_FORMAT_R32G32_TYPELESS:
					return 4 * 2;
				case DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT:
					return 4 * 2;
				case DXGI_FORMAT.DXGI_FORMAT_R32G32_UINT:
					return 4 * 2;
				case DXGI_FORMAT.DXGI_FORMAT_R32G32_SINT:
					return 4 * 2;
				case DXGI_FORMAT.DXGI_FORMAT_R32G8X24_TYPELESS:
					return 4 * 2;
				case DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT_S8X24_UINT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_X32_TYPELESS_G8X24_UINT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_TYPELESS:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UNORM:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UINT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R11G11B10_FLOAT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_TYPELESS:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UINT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SNORM:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SINT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R16G16_TYPELESS:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R16G16_FLOAT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R16G16_UNORM:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R16G16_UINT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R16G16_SNORM:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R16G16_SINT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R32_TYPELESS:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R32_UINT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R32_SINT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R24G8_TYPELESS:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R24_UNORM_X8_TYPELESS:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_X24_TYPELESS_G8_UINT:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R8G8_TYPELESS:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_R8G8_UINT:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_R8G8_SNORM:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_R8G8_SINT:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_R16_TYPELESS:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_R16_FLOAT:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_D16_UNORM:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_R16_UNORM:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_R16_UINT:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_R16_SNORM:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_R16_SINT:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_R8_TYPELESS:
					return 1;
				case DXGI_FORMAT.DXGI_FORMAT_R8_UNORM:
					return 1;
				case DXGI_FORMAT.DXGI_FORMAT_R8_UINT:
					return 1;
				case DXGI_FORMAT.DXGI_FORMAT_R8_SNORM:
					return 1;
				case DXGI_FORMAT.DXGI_FORMAT_R8_SINT:
					return 1;
				case DXGI_FORMAT.DXGI_FORMAT_A8_UNORM:
					return 1;
				case DXGI_FORMAT.DXGI_FORMAT_R1_UNORM:
					return 1;
				case DXGI_FORMAT.DXGI_FORMAT_R9G9B9E5_SHAREDEXP:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R8G8_B8G8_UNORM:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_G8R8_G8B8_UNORM:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
					return 8;
				case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
					return 8;
				case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
					return 8;
				case DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
					return 16;
				case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
					return 16;
				case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
					return 16;
				case DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
					return 16;
				case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
					return 16;
				case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
					return 16;
				case DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
					return 8;
				case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
					return 8;
				case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
					return 8;
				case DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
					return 8;
				case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
					return 8;
				case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
					return 8;
				case DXGI_FORMAT.DXGI_FORMAT_B5G6R5_UNORM:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_TYPELESS:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM_SRGB:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_TYPELESS:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM_SRGB:
					return 4;
				case DXGI_FORMAT.DXGI_FORMAT_BC6H_TYPELESS:
					return 16;
				case DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
					return 16;
				case DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
					return 16;
				case DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
					return 16;
				case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
					return 16;
				case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
					return 16;
				case DXGI_FORMAT.DXGI_FORMAT_P8:
					return 1;
				case DXGI_FORMAT.DXGI_FORMAT_A8P8:
					return 2;
				case DXGI_FORMAT.DXGI_FORMAT_B4G4R4A4_UNORM:
					return 2;
			}
			return 4;
		}

		public static bool IsCompressedFormat(this DXGI_FORMAT format)
		{
			switch (format)
			{
				case DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
				case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
				case DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
				case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
				case DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
				case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
				case DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
				case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
				case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC6H_TYPELESS:
				case DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
				case DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
				case DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
				case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
					return true;

				default:
					return false;
			}
		}
	}
}
