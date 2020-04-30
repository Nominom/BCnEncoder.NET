using System;
using System.IO;
using System.Text;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.Decoder
{
	public class DecoderInputOptions {
		/// <summary>
		/// The DDS file format doesn't seem to have a standard for indicating whether a BC1 texture
		/// includes 1bit of alpha. This option will assume that all Bc1 textures contain alpha.
		/// If this option is false, but the dds header includes a DDPF_ALPHAPIXELS flag, alpha will be included.
		/// Default is true.
		/// </summary>
		public bool ddsBc1ExpectAlpha = true;
	}

	public class DecoderOutputOptions {
		/// <summary>
		/// If true, when decoding from a format that only includes a red channel,
		/// output pixels will have all colors set to the same value (greyscale). Default is true.
		/// </summary>
		public bool redAsLuminance = true;
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

		private IBcBlockDecoder GetDecoder(DXGI_FORMAT format, DdsHeader header)
		{
			switch (format)
			{
				case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
				case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
				case DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
					if ((header.ddsPixelFormat.dwFlags & PixelFormatFlags.DDPF_ALPHAPIXELS) != 0) {
						return new Bc1ADecoder();
					}
					else if(InputOptions.ddsBc1ExpectAlpha){
						return new Bc1ADecoder();
					}
					else {
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
		public Image<Rgba32> Decode(Stream inputStream) {
			var position = inputStream.Position;
			try {
				if (inputStream is FileStream fs) {
					var extension = Path.GetExtension(fs.Name).ToLower();
					if (extension == ".ktx") {
						KtxFile file = KtxFile.Load(inputStream);
						return Decode(file);
					}
					else if (extension == ".dds") {
						DdsFile file = DdsFile.Load(inputStream);
						return Decode(file);
					}
				}

				bool isDDS = false;
				using (var br = new BinaryReader(inputStream, Encoding.UTF8, true)) {
					var magic = br.ReadUInt32();
					if (magic == 0x20534444U) {
						isDDS = true;
					}
				}

				inputStream.Seek(position, SeekOrigin.Begin);

				if (isDDS) {
					DdsFile dds = DdsFile.Load(inputStream);
					return Decode(dds);
				}
				else {
					KtxFile ktx = KtxFile.Load(inputStream);
					return Decode(ktx);
				}
			}
			catch (Exception) {
				inputStream.Seek(position, SeekOrigin.Begin);
				throw;
			}
		}

		/// <summary>
		/// Read a Ktx or a Dds file from a stream and decode it.
		/// </summary>
		public Image<Rgba32>[] DecodeAllMipMaps(Stream inputStream) {
			var position = inputStream.Position;
			try {
				if (inputStream is FileStream fs) {
					var extension = Path.GetExtension(fs.Name).ToLower();
					if (extension == ".ktx") {
						KtxFile file = KtxFile.Load(inputStream);
						return DecodeAllMipMaps(file);
					}
					else if (extension == ".dds") {
						DdsFile file = DdsFile.Load(inputStream);
						return DecodeAllMipMaps(file);
					}
				}

				bool isDDS = false;
				using (var br = new BinaryReader(inputStream, Encoding.UTF8, true)) {
					var magic = br.ReadUInt32();
					if (magic == 0x20534444U) {
						isDDS = true;
					}
				}

				inputStream.Seek(position, SeekOrigin.Begin);

				if (isDDS) {
					DdsFile dds = DdsFile.Load(inputStream);
					return DecodeAllMipMaps(dds);
				}
				else {
					KtxFile ktx = KtxFile.Load(inputStream);
					return DecodeAllMipMaps(ktx);
				}
			}
			catch (Exception) {
				inputStream.Seek(position, SeekOrigin.Begin);
				throw;
			}
		}

		/// <summary>
		/// Read a KtxFile and decode it.
		/// </summary>
		public Image<Rgba32> Decode(KtxFile file)
		{
			if (IsSupportedRawFormat(file.Header.GlInternalFormat)) {
				var decoder = GetRawDecoder(file.Header.GlInternalFormat);
				var data = file.MipMaps[0].Faces[0].Data;
				var pixelWidth = file.MipMaps[0].Width;
				var pixelHeight = file.MipMaps[0].Height;

				var image = new Image<Rgba32>((int)pixelWidth, (int)pixelHeight);
				var output = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight);
				var pixels = image.GetPixelSpan();

				output.CopyTo(pixels);
				return image;
			}
			else {
				var decoder = GetDecoder(file.Header.GlInternalFormat);
				if (decoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {file.Header.GlInternalFormat}");
				}

				var data = file.MipMaps[0].Faces[0].Data;
				var pixelWidth = file.MipMaps[0].Width;
				var pixelHeight = file.MipMaps[0].Height;

				var blocks = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight, out var blockWidth, out var blockHeight);

				return ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight, 
					(int)pixelWidth, (int)pixelHeight);
			}
		}

		/// <summary>
		/// Read a KtxFile and decode it.
		/// </summary>
		public Image<Rgba32>[] DecodeAllMipMaps(KtxFile file)
		{
			if (IsSupportedRawFormat(file.Header.GlInternalFormat)) {
				var decoder = GetRawDecoder(file.Header.GlInternalFormat);
				var images = new Image<Rgba32>[file.MipMaps.Count];

				for (int mip = 0; mip < file.MipMaps.Count; mip++) {
					var data = file.MipMaps[mip].Faces[0].Data;
					var pixelWidth = file.MipMaps[mip].Width;
					var pixelHeight = file.MipMaps[mip].Height;

					var image = new Image<Rgba32>((int)pixelWidth, (int)pixelHeight);
					var output = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight);
					var pixels = image.GetPixelSpan();

					output.CopyTo(pixels);
					images[mip] = image;
				}
				
				return images;
			}
			else {
				var decoder = GetDecoder(file.Header.GlInternalFormat);
				if (decoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {file.Header.GlInternalFormat}");
				}
				var images = new Image<Rgba32>[file.MipMaps.Count];

				for (int mip = 0; mip < file.MipMaps.Count; mip++) {

					var data = file.MipMaps[mip].Faces[0].Data;
					var pixelWidth = file.MipMaps[mip].Width;
					var pixelHeight = file.MipMaps[mip].Height;

					var blocks = decoder.Decode(data, (int) pixelWidth, (int) pixelHeight, out var blockWidth,
						out var blockHeight);

					var image = ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight, 
						(int)pixelWidth, (int)pixelHeight);
					images[mip] = image;
				}
				return images;
			}
		}

		/// <summary>
		/// Read a DdsFile and decode it
		/// </summary>
		public Image<Rgba32> Decode(DdsFile file)
		{
			if (IsSupportedRawFormat(file.Header.ddsPixelFormat.DxgiFormat)) {
				var decoder = GetRawDecoder(file.Header.ddsPixelFormat.DxgiFormat);
				var data = file.Faces[0].MipMaps[0].Data;
				var pixelWidth = file.Faces[0].Width;
				var pixelHeight = file.Faces[0].Height;

				var image = new Image<Rgba32>((int)pixelWidth, (int)pixelHeight);
				var output = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight);
				var pixels = image.GetPixelSpan();

				output.CopyTo(pixels);
				return image;
			}
			else {
				DXGI_FORMAT format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;
				if (file.Header.ddsPixelFormat.IsDxt10Format) {
					format = file.Dxt10Header.dxgiFormat;
				}
				else {
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

				return ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight, 
					(int)pixelWidth, (int)pixelHeight);
			}
		}

		/// <summary>
		/// Read a DdsFile and decode it
		/// </summary>
		public Image<Rgba32>[] DecodeAllMipMaps(DdsFile file)
		{
			if (IsSupportedRawFormat(file.Header.ddsPixelFormat.DxgiFormat)) {
				var decoder = GetRawDecoder(file.Header.ddsPixelFormat.DxgiFormat);

				var images = new Image<Rgba32>[file.Header.dwMipMapCount];

				for (int mip = 0; mip < file.Header.dwMipMapCount; mip++) {
					var data = file.Faces[0].MipMaps[mip].Data;
					var pixelWidth = file.Faces[0].MipMaps[mip].Width;
					var pixelHeight = file.Faces[0].MipMaps[mip].Height;

					var image = new Image<Rgba32>((int) pixelWidth, (int) pixelHeight);
					var output = decoder.Decode(data, (int) pixelWidth, (int) pixelHeight);
					var pixels = image.GetPixelSpan();

					output.CopyTo(pixels);
					images[mip] = image;
				}
				return images;
			}
			else {
				DXGI_FORMAT format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;
				if (file.Header.ddsPixelFormat.IsDxt10Format) {
					format = file.Dxt10Header.dxgiFormat;
				}
				else {
					format = file.Header.ddsPixelFormat.DxgiFormat;
				}
				IBcBlockDecoder decoder = GetDecoder(format, file.Header);
				
				if (decoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {format}");
				}
				var images = new Image<Rgba32>[file.Header.dwMipMapCount];

				for (int mip = 0; mip < file.Header.dwMipMapCount; mip++) {
					var data = file.Faces[0].MipMaps[mip].Data;
					var pixelWidth = file.Faces[0].MipMaps[mip].Width;
					var pixelHeight = file.Faces[0].MipMaps[mip].Height;

					var blocks = decoder.Decode(data, (int) pixelWidth, (int) pixelHeight, out var blockWidth,
						out var blockHeight);

					var image = ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight, 
						(int)pixelWidth, (int)pixelHeight);

					images[mip] = image;
				}

				return images;
			}
		}
	}
}
