using System;
using System.IO;
using System.Text;
using BCnEncoder.Decoder.Options;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.Decoder
{
    /// <summary>
	/// Decodes compressed files into Rgba format.
	/// </summary>
	public class BcDecoder
    {
        /// <summary>
        /// The input options of the decoder.
        /// </summary>
        public DecoderInputOptions InputOptions { get; } = new DecoderInputOptions();

        /// <summary>
        /// The output options of the decoder.
        /// </summary>
        public DecoderOutputOptions OutputOptions { get; } = new DecoderOutputOptions();

        /// <summary>
        /// Decode raw encoded image data.
        /// </summary>
        /// <param name="inputStream">The stream containing the encoded data.</param>
        /// <param name="format">The format the encoded data is in.</param>
        /// <param name="pixelWidth">The pixelWidth of the image.</param>
        /// <param name="pixelHeight">The pixelHeight of the image.</param>
        /// <returns>The decoded Rgba32 image.</returns>
        public Image<Rgba32> DecodeRaw(Stream inputStream, CompressionFormat format, int pixelWidth, int pixelHeight)
        {
            var dataArray = new byte[inputStream.Position];
            inputStream.Read(dataArray, 0, dataArray.Length);

            return DecodeRaw(dataArray, format, pixelWidth, pixelHeight);
        }

        /// <summary>
        /// Decode raw encoded image data.
        /// </summary>
        /// <param name="input">The array containing the encoded data.</param>
        /// <param name="format">The format the encoded data is in.</param>
        /// <param name="pixelWidth">The pixelWidth of the image.</param>
        /// <param name="pixelHeight">The pixelHeight of the image.</param>
        /// <returns>The decoded Rgba32 image.</returns>
        public Image<Rgba32> DecodeRaw(byte[] input, CompressionFormat format, int pixelWidth, int pixelHeight)
        {
            var isCompressedFormat = format.IsCompressedFormat();
            if (isCompressedFormat)
            {
                // Decode as compressed data
                var decoder = GetDecoder(format);
                var blocks = decoder.Decode(input, pixelWidth, pixelHeight, out var blockWidth, out var blockHeight);

                return ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight, pixelWidth, pixelHeight);
            }

            // Decode as raw data
            var rawDecoder = GetRawDecoder(format);

            var image = new Image<Rgba32>(pixelWidth, pixelHeight);
            var output = rawDecoder.Decode(input, pixelWidth, pixelHeight);
            if (!image.TryGetSinglePixelSpan(out var pixels))
                throw new Exception("Cannot get pixel span.");

            output.CopyTo(pixels);
            return image;
        }

        /// <summary>
        /// Read a Ktx or a Dds file from a stream and decode it.
        /// </summary>
        /// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
        /// <returns>The decoded Rgba32 image.</returns>
        public Image<Rgba32> Decode(Stream inputStream)
        {
            return Decode(inputStream, false)[0];
        }

        /// <summary>
        /// Read a Ktx or a Dds file from a stream and decode it.
        /// </summary>
        /// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
        /// <returns>An array of decoded Rgba32 images.</returns>
        public Image<Rgba32>[] DecodeAllMipMaps(Stream inputStream)
        {
            return Decode(inputStream, true);
        }

        /// <summary>
        /// Read a Ktx file and decode it.
        /// </summary>
        /// <param name="file">The loaded Ktx file.</param>
        /// <returns>The decoded Rgba32 image.</returns>
        public Image<Rgba32> Decode(KtxFile file)
        {
            return Decode(file, false)[0];
        }

        /// <summary>
        /// Read a Ktx file and decode it.
        /// </summary>
        /// <param name="file">The loaded Ktx file.</param>
        /// <returns>An array of decoded Rgba32 images.</returns>
        public Image<Rgba32>[] DecodeAllMipMaps(KtxFile file)
        {
            return Decode(file, true);
        }

        /// <summary>
        /// Read a Dds file and decode it.
        /// </summary>
        /// <param name="file">The loaded Dds file.</param>
        /// <returns>The decoded Rgba32 image.</returns>
        public Image<Rgba32> Decode(DdsFile file)
        {
            return Decode(file, false)[0];
        }

        /// <summary>
        /// Read a Dds file and decode it.
        /// </summary>
        /// <param name="file">The loaded Dds file.</param>
        /// <returns>An array of decoded Rgba32 images.</returns>
        public Image<Rgba32>[] DecodeAllMipMaps(DdsFile file)
        {
            return Decode(file, true);
        }

        /// <summary>
        /// Load a KTX or DDS file from a stream and extract either the main image or all mip maps.
        /// </summary>
        /// <param name="inputStream">The input stream to decode.</param>
        /// <param name="allMipMaps">If all mip maps or only the main image should be decoded.</param>
        /// <returns>An array of decoded Rgba32 images.</returns>
        private Image<Rgba32>[] Decode(Stream inputStream, bool allMipMaps)
        {
            var position = inputStream.Position;
            try
            {
                // Detect if file is a KTX or DDS by file extension
                if (inputStream is FileStream fs)
                {
                    var extension = Path.GetExtension(fs.Name).ToLower();
                    switch (extension)
                    {
                        case ".dds":
                            var ddsFile = DdsFile.Load(inputStream);
                            return Decode(ddsFile, allMipMaps);

                        case ".file":
                            var ktxFile = KtxFile.Load(inputStream);
                            return Decode(ktxFile, allMipMaps);
                    }
                }

                // Otherwise detect KTX or DDS by content of the stream
                bool isDds;
                using (var br = new BinaryReader(inputStream, Encoding.UTF8, true))
                {
                    var magic = br.ReadUInt32();
                    isDds = magic == 0x20534444U;
                }

                inputStream.Seek(position, SeekOrigin.Begin);

                if (isDds)
                {
                    var dds = DdsFile.Load(inputStream);
                    return Decode(dds, allMipMaps);
                }

                var ktx = KtxFile.Load(inputStream);
                return Decode(ktx, allMipMaps);
            }
            catch (Exception)
            {
                inputStream.Seek(position, SeekOrigin.Begin);
                throw;
            }
        }

        /// <summary>
        /// Load a KTX file and extract either the main image or all mip maps.
        /// </summary>
        /// <param name="file">The Ktx file to decode.</param>
        /// <param name="allMipMaps">If all mip maps or only the main image should be decoded.</param>
        /// <returns>An array of decoded Rgba32 images.</returns>
        private Image<Rgba32>[] Decode(KtxFile file, bool allMipMaps)
        {
            var images = new Image<Rgba32>[file.MipMaps.Count];

            if (IsSupportedRawFormat(file.Header.GlInternalFormat))
            {
                var decoder = GetRawDecoder(file.Header.GlInternalFormat);

                var mipMaps = allMipMaps ? file.MipMaps.Count : 1;
                for (var mip = 0; mip < mipMaps; mip++)
                {
                    var data = file.MipMaps[mip].Faces[0].Data;
                    var pixelWidth = file.MipMaps[mip].Width;
                    var pixelHeight = file.MipMaps[mip].Height;

                    var image = new Image<Rgba32>((int)pixelWidth, (int)pixelHeight);
                    var output = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight);
                    if (!image.TryGetSinglePixelSpan(out var pixels))
                        throw new Exception("Cannot get pixel span.");

                    output.CopyTo(pixels);
                    images[mip] = image;
                }
            }
            else
            {
                var decoder = GetDecoder(file.Header.GlInternalFormat);
                if (decoder == null)
                    throw new NotSupportedException($"This format is not supported: {file.Header.GlInternalFormat}");

                var mipMaps = allMipMaps ? file.MipMaps.Count : 1;
                for (var mip = 0; mip < mipMaps; mip++)
                {
                    var data = file.MipMaps[mip].Faces[0].Data;
                    var pixelWidth = file.MipMaps[mip].Width;
                    var pixelHeight = file.MipMaps[mip].Height;

                    var blocks = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight, out var blockWidth, out var blockHeight);

                    images[mip] = ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight, (int)pixelWidth, (int)pixelHeight);
                }
            }

            return images;
        }

        /// <summary>
        /// Load a DDS file and extract either the main image or all mip maps.
        /// </summary>
        /// <param name="file">The Dds file to decode.</param>
        /// <param name="allMipMaps">If all mip maps or only the main image should be decoded.</param>
        /// <returns>An array of decoded Rgba32 images.</returns>
        private Image<Rgba32>[] Decode(DdsFile file, bool allMipMaps)
        {
            var images = new Image<Rgba32>[file.Header.dwMipMapCount];

            if (IsSupportedRawFormat(file.Header.ddsPixelFormat.DxgiFormat))
            {
                var decoder = GetRawDecoder(file.Header.ddsPixelFormat.DxgiFormat);

                var mipMaps = allMipMaps ? file.Header.dwMipMapCount : 1;
                for (var mip = 0; mip < mipMaps; mip++)
                {
                    var data = file.Faces[0].MipMaps[mip].Data;
                    var pixelWidth = file.Faces[0].MipMaps[mip].Width;
                    var pixelHeight = file.Faces[0].MipMaps[mip].Height;

                    var image = new Image<Rgba32>((int)pixelWidth, (int)pixelHeight);
                    var output = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight);
                    if (!image.TryGetSinglePixelSpan(out var pixels))
                        throw new Exception("Cannot get pixel span.");

                    output.CopyTo(pixels);
                    images[mip] = image;
                }
            }
            else
            {
                var format = file.Header.ddsPixelFormat.IsDxt10Format ?
                    file.Dxt10Header.dxgiFormat :
                    file.Header.ddsPixelFormat.DxgiFormat;
                var decoder = GetDecoder(format, file.Header);

                if (decoder == null)
                    throw new NotSupportedException($"This format is not supported: {format}");

                for (var mip = 0; mip < file.Header.dwMipMapCount; mip++)
                {
                    var data = file.Faces[0].MipMaps[mip].Data;
                    var pixelWidth = file.Faces[0].MipMaps[mip].Width;
                    var pixelHeight = file.Faces[0].MipMaps[mip].Height;

                    var blocks = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight, out var blockWidth,
                        out var blockHeight);

                    var image = ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight,
                        (int)pixelWidth, (int)pixelHeight);

                    images[mip] = image;
                }
            }

            return images;
        }

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
                    if ((header.ddsPixelFormat.dwFlags & PixelFormatFlags.DDPF_ALPHAPIXELS) != 0)
                        return new Bc1ADecoder();

                    if (InputOptions.ddsBc1ExpectAlpha)
                        return new Bc1ADecoder();

                    return new Bc1NoAlphaDecoder();

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

        private IRawDecoder GetRawDecoder(CompressionFormat format)
        {
            switch (format)
            {
                case CompressionFormat.R:
                    return new RawRDecoder(OutputOptions.redAsLuminance);

                case CompressionFormat.RG:
                    return new RawRGDecoder();

                case CompressionFormat.RGB:
                    return new RawRGBDecoder();

                case CompressionFormat.RGBA:
                    return new RawRGBADecoder();

                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
    }
}
