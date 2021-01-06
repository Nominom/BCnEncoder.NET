using System;
using System.IO;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncoder.Decoder
{
	public class DecoderInputOptions
	{
		/// <summary>
		/// The DDS file format doesn't seem to have a standard for indicating whether a BC1 texture
		/// includes 1bit of alpha. This option will assume that all Bc1 textures contain alpha.
		/// If this option is false, but the dds header includes a DDPF_ALPHAPIXELS flag, alpha will be included.
		/// Default is true.
		/// </summary>
		public bool ddsBc1ExpectAlpha = true;
	}

	public class DecoderOutputOptions
	{
		/// <summary>
		/// If true, when decoding from a format that only includes a red channel,
		/// output pixels will have all colors set to the same value (greyscale). Default is true.
		/// </summary>
		public bool redAsLuminance = true;
	}

	public struct DecodedMipMap
	{
		public int Width;
		public int Height;
		public byte[] data;
	}

	/// <summary>
	/// Decodes compressed files into Rgba format.
	/// </summary>
	public class BcDecoder
	{
		public DecoderOutputOptions OutputOptions { get; set; } = new DecoderOutputOptions();
		public DecoderInputOptions InputOptions { get; set; } = new DecoderInputOptions();

		private bool IsSupportedRawFormat(GlInternalFormat format)
		{
			switch (format)
			{
				case GlInternalFormat.GL_R8:
				case GlInternalFormat.GL_RG8:
				case GlInternalFormat.GL_RGB8:
				case GlInternalFormat.GL_RGBA8:
					return true;
				default:
					return false;
			}
		}

		private bool IsSupportedRawFormat(DXGI_FORMAT format)
		{
			switch (format)
			{
				case DXGI_FORMAT.DXGI_FORMAT_R8_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM:
					return true;
				default:
					return false;
			}
		}

		private IBcBlockDecoder GetDecoder(GlInternalFormat format)
		{
			switch (format)
			{
				case GlInternalFormat.GL_COMPRESSED_RGB_S3TC_DXT1_EXT:
					return new Bc1NoAlphaDecoder();
				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT:
					return new Bc1ADecoder();
				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT:
					return new Bc2Decoder();
				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT:
					return new Bc3Decoder();
				case GlInternalFormat.GL_COMPRESSED_RED_RGTC1_EXT:
					return new Bc4Decoder(OutputOptions.redAsLuminance);
				case GlInternalFormat.GL_COMPRESSED_RED_GREEN_RGTC2_EXT:
					return new Bc5Decoder();
				case GlInternalFormat.GL_COMPRESSED_RGBA_BPTC_UNORM_ARB:
					return new Bc7Decoder();
				// TODO: Not sure what to do with SRGB input.
				case GlInternalFormat.GL_COMPRESSED_SRGB_ALPHA_BPTC_UNORM_ARB:
					return new Bc7Decoder();
				default:
					return null;
			}
		}

		private IBcBlockDecoder GetDecoder(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.BC1:
					return new Bc1NoAlphaDecoder();
				case CompressionFormat.BC1WithAlpha:
					return new Bc1ADecoder();
				case CompressionFormat.BC2:
					return new Bc2Decoder();
				case CompressionFormat.BC3:
					return new Bc3Decoder();
				case CompressionFormat.BC4:
					return new Bc4Decoder(OutputOptions.redAsLuminance);
				case CompressionFormat.BC5:
					return new Bc5Decoder();
				case CompressionFormat.BC7:
					return new Bc7Decoder();
				default:
					return null;
			}
		}

		private IBcBlockDecoder GetDecoder(DXGI_FORMAT format, DdsHeader header)
		{
			switch (format)
			{
				case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
				case DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
					if ((header.ddsPixelFormat.dwFlags & PixelFormatFlags.DDPF_ALPHAPIXELS) != 0)
					{
						return new Bc1ADecoder();
					}
					else if (InputOptions.ddsBc1ExpectAlpha)
					{
						return new Bc1ADecoder();
					}
					else
					{
						return new Bc1NoAlphaDecoder();
					}

				case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
				case DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
					return new Bc2Decoder();
				case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
				case DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
					return new Bc3Decoder();
				case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
					return new Bc4Decoder(OutputOptions.redAsLuminance);
				case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
					return new Bc5Decoder();
				case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
				case DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
					return new Bc7Decoder();
				default:
					return null;
			}
		}

		private IRawDecoder GetRawDecoder(GlInternalFormat format)
		{
			switch (format)
			{
				case GlInternalFormat.GL_R8:
					return new RawRDecoder(OutputOptions.redAsLuminance);
				case GlInternalFormat.GL_RG8:
					return new RawRGDecoder();
				case GlInternalFormat.GL_RGB8:
					return new RawRGBDecoder();
				case GlInternalFormat.GL_RGBA8:
					return new RawRGBADecoder();
				default:
					return null;
			}
		}

		private IRawDecoder GetRawDecoder(DXGI_FORMAT format)
		{
			switch (format)
			{
				case DXGI_FORMAT.DXGI_FORMAT_R8_UNORM:
					return new RawRDecoder(OutputOptions.redAsLuminance);
				case DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM:
					return new RawRGDecoder();
				case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM:
					return new RawRGBADecoder();
				default:
					return null;
			}
		}

		/// <summary>
		/// Read a Ktx or a Dds file from a stream and decode it.
		/// </summary>
		public DecodedMipMap Decode(Stream inputStream)
		{
			var position = inputStream.Position;
			try
			{

				bool isDDS = false;
				using (var br = new BinaryReader(inputStream, Encoding.UTF8, true))
				{
					var magic = br.ReadUInt32();
					if (magic == 0x20534444U)
					{
						isDDS = true;
					}
				}

				inputStream.Seek(position, SeekOrigin.Begin);

				if (isDDS)
				{
					DdsFile dds = DdsFile.Load(inputStream);
					return Decode(dds);
				}
				else
				{
					KtxFile ktx = KtxFile.Load(inputStream);
					return Decode(ktx);
				}
			}
			catch (Exception)
			{
				inputStream.Seek(position, SeekOrigin.Begin);
				throw;
			}
		}

		/// <summary>
		/// Read a Ktx or a Dds file from a stream and decode it.
		/// </summary>
		public DecodedMipMap[] DecodeAllMipMaps(Stream inputStream)
		{
			var position = inputStream.Position;
			try
			{

				bool isDDS = false;
				using (var br = new BinaryReader(inputStream, Encoding.UTF8, true))
				{
					var magic = br.ReadUInt32();
					if (magic == 0x20534444U)
					{
						isDDS = true;
					}
				}

				inputStream.Seek(position, SeekOrigin.Begin);

				if (isDDS)
				{
					DdsFile dds = DdsFile.Load(inputStream);
					return DecodeAllMipMaps(dds);
				}
				else
				{
					KtxFile ktx = KtxFile.Load(inputStream);
					return DecodeAllMipMaps(ktx);
				}
			}
			catch (Exception)
			{
				inputStream.Seek(position, SeekOrigin.Begin);
				throw;
			}
		}

		/// <summary>
		/// Read a KtxFile and decode it.
		/// </summary>
		public DecodedMipMap Decode(KtxFile file)
		{
			if (IsSupportedRawFormat(file.Header.GlInternalFormat))
			{
				var decoder = GetRawDecoder(file.Header.GlInternalFormat);
				var data = file.MipMaps[0].Faces[0].Data;
				var pixelWidth = file.MipMaps[0].Width;
				var pixelHeight = file.MipMaps[0].Height;

				var image = new byte[(int)pixelWidth * (int)pixelHeight * 4];
				var output = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight);

				for (int i = 0; i < output.Length; i++)
				{
					image[i * 4 + 0] = output[i].R;
					image[i * 4 + 1] = output[i].G;
					image[i * 4 + 2] = output[i].B;
					image[i * 4 + 3] = output[i].A;
				}

				return new DecodedMipMap()
				{
					Width = (int)pixelWidth,
					Height = (int)pixelHeight,
					data = image
				};
			}
			else
			{
				var decoder = GetDecoder(file.Header.GlInternalFormat);
				if (decoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {file.Header.GlInternalFormat}");
				}

				var data = file.MipMaps[0].Faces[0].Data;
				var pixelWidth = file.MipMaps[0].Width;
				var pixelHeight = file.MipMaps[0].Height;

				var blocks = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight, out var blockWidth, out var blockHeight);

				return new DecodedMipMap()
				{
					Width = (int)pixelWidth,
					Height = (int)pixelHeight,
					data = ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight,
						(int)pixelWidth, (int)pixelHeight)
				};
			}
		}

		/// <summary>
		/// Read a KtxFile and decode it.
		/// </summary>
		public DecodedMipMap[] DecodeAllMipMaps(KtxFile file)
		{
			if (IsSupportedRawFormat(file.Header.GlInternalFormat))
			{
				var decoder = GetRawDecoder(file.Header.GlInternalFormat);
				var images = new DecodedMipMap[file.MipMaps.Count];

				for (int mip = 0; mip < file.MipMaps.Count; mip++)
				{
					var data = file.MipMaps[mip].Faces[0].Data;
					var pixelWidth = file.MipMaps[mip].Width;
					var pixelHeight = file.MipMaps[mip].Height;

					var image = new byte[(int)pixelWidth * (int)pixelHeight * 4];
					var output = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight);

					for (int i = 0; i < output.Length; i++)
					{
						image[i * 4 + 0] = output[i].R;
						image[i * 4 + 1] = output[i].G;
						image[i * 4 + 2] = output[i].B;
						image[i * 4 + 3] = output[i].A;
					}
					
					images[mip] = new DecodedMipMap()
					{
						Width = (int)pixelWidth,
						Height = (int)pixelHeight,
						data = image
					};
				}

				return images;
			}
			else
			{
				var decoder = GetDecoder(file.Header.GlInternalFormat);
				if (decoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {file.Header.GlInternalFormat}");
				}
				var images = new DecodedMipMap[file.MipMaps.Count];

				for (int mip = 0; mip < file.MipMaps.Count; mip++)
				{

					var data = file.MipMaps[mip].Faces[0].Data;
					var pixelWidth = file.MipMaps[mip].Width;
					var pixelHeight = file.MipMaps[mip].Height;

					var blocks = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight, out var blockWidth,
						out var blockHeight);

					var image = ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight,
						(int)pixelWidth, (int)pixelHeight);
					images[mip] = new DecodedMipMap()
					{
						Width = (int)pixelWidth,
						Height = (int)pixelHeight,
						data = image
					};
				}
				return images;
			}
		}

		/// <summary>
		/// Read a DdsFile and decode it
		/// </summary>
		public DecodedMipMap Decode(DdsFile file)
		{
			if (IsSupportedRawFormat(file.Header.ddsPixelFormat.DxgiFormat))
			{
				var decoder = GetRawDecoder(file.Header.ddsPixelFormat.DxgiFormat);
				var data = file.Faces[0].MipMaps[0].Data;
				var pixelWidth = file.Faces[0].Width;
				var pixelHeight = file.Faces[0].Height;

				var image = new byte[(int)pixelWidth * (int)pixelHeight * 4];
				var output = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight);

				for (int i = 0; i < output.Length; i++)
				{
					image[i * 4 + 0] = output[i].R;
					image[i * 4 + 1] = output[i].G;
					image[i * 4 + 2] = output[i].B;
					image[i * 4 + 3] = output[i].A;
				}
				
				return new DecodedMipMap()
				{
					Width = (int)pixelWidth,
					Height = (int)pixelHeight,
					data = image
				};
			}
			else
			{
				DXGI_FORMAT format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;
				if (file.Header.ddsPixelFormat.IsDxt10Format)
				{
					format = file.Dxt10Header.dxgiFormat;
				}
				else
				{
					format = file.Header.ddsPixelFormat.DxgiFormat;
				}
				IBcBlockDecoder decoder = GetDecoder(format, file.Header);

				if (decoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {format}");
				}

				var data = file.Faces[0].MipMaps[0].Data;
				var pixelWidth = file.Faces[0].Width;
				var pixelHeight = file.Faces[0].Height;

				var blocks = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight, out var blockWidth, out var blockHeight);

				return new DecodedMipMap()
				{
					Width = (int)pixelWidth,
					Height = (int)pixelHeight,
					data = ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight,
						(int)pixelWidth, (int)pixelHeight)
				};
			}
		}

		/// <summary>
		/// Read a DdsFile and decode it
		/// </summary>
		public DecodedMipMap[] DecodeAllMipMaps(DdsFile file)
		{
			if (IsSupportedRawFormat(file.Header.ddsPixelFormat.DxgiFormat))
			{
				var decoder = GetRawDecoder(file.Header.ddsPixelFormat.DxgiFormat);

				var images = new DecodedMipMap[file.Header.dwMipMapCount];

				for (int mip = 0; mip < file.Header.dwMipMapCount; mip++)
				{
					var data = file.Faces[0].MipMaps[mip].Data;
					var pixelWidth = file.Faces[0].MipMaps[mip].Width;
					var pixelHeight = file.Faces[0].MipMaps[mip].Height;

					var image = new byte[(int)pixelWidth * (int)pixelHeight * 4];
					var output = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight);

					for (int i = 0; i < output.Length; i++)
					{
						image[i * 4 + 0] = output[i].R;
						image[i * 4 + 1] = output[i].G;
						image[i * 4 + 2] = output[i].B;
						image[i * 4 + 3] = output[i].A;
					}
					images[mip] = new DecodedMipMap()
					{
						Width = (int)pixelWidth,
						Height = (int)pixelHeight,
						data = image
					};
				}
				return images;
			}
			else
			{
				DXGI_FORMAT format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;
				if (file.Header.ddsPixelFormat.IsDxt10Format)
				{
					format = file.Dxt10Header.dxgiFormat;
				}
				else
				{
					format = file.Header.ddsPixelFormat.DxgiFormat;
				}
				IBcBlockDecoder decoder = GetDecoder(format, file.Header);

				if (decoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {format}");
				}
				var images = new DecodedMipMap[file.Header.dwMipMapCount];

				for (int mip = 0; mip < file.Header.dwMipMapCount; mip++)
				{
					var data = file.Faces[0].MipMaps[mip].Data;
					var pixelWidth = file.Faces[0].MipMaps[mip].Width;
					var pixelHeight = file.Faces[0].MipMaps[mip].Height;

					var blocks = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight, out var blockWidth,
						out var blockHeight);

					var image = ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight,
						(int)pixelWidth, (int)pixelHeight);

					images[mip] = new DecodedMipMap()
					{
						Width = (int)pixelWidth,
						Height = (int)pixelHeight,
						data = image
					};
				}

				return images;
			}
		}

		/// <summary>
		/// Read raw block compressed data and decode it to RGBA.
		/// </summary>
		public byte[] DecodeRawData(byte[] bcData, int imageWidth, int imageHeight, CompressionFormat format)
		{

			var decoder = GetDecoder(format);
			if (decoder == null)
			{
				throw new NotSupportedException($"This format is not supported: {format}");
			}

			var blocks = decoder.Decode(bcData, imageWidth, imageHeight, out var blockWidth, out var blockHeight);

			return ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight,
				imageWidth, imageHeight);

		}
	}
}
