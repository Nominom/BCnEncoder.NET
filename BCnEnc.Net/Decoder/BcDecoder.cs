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
	/// Decodes compressed files into Rgba Format.
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
        /// <param name="format">The Format the encoded data is in.</param>
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
        /// <param name="format">The Format the encoded data is in.</param>
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
            {
                throw new Exception("Cannot get pixel span.");
            }

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

            if (IsSupportedRawFormat(file.header.GlInternalFormat))
            {
                var decoder = GetRawDecoder(file.header.GlInternalFormat);

                var mipMaps = allMipMaps ? file.MipMaps.Count : 1;
                for (var mip = 0; mip < mipMaps; mip++)
                {
                    var data = file.MipMaps[mip].Faces[0].Data;
                    var pixelWidth = file.MipMaps[mip].Width;
                    var pixelHeight = file.MipMaps[mip].Height;

                    var image = new Image<Rgba32>((int)pixelWidth, (int)pixelHeight);
                    var output = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight);
                    if (!image.TryGetSinglePixelSpan(out var pixels))
                    {
                        throw new Exception("Cannot get pixel span.");
                    }

                    output.CopyTo(pixels);
                    images[mip] = image;
                }
            }
            else
            {
                var decoder = GetDecoder(file.header.GlInternalFormat);
                if (decoder == null)
                {
                    throw new NotSupportedException($"This Format is not supported: {file.header.GlInternalFormat}");
                }

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
            var images = new Image<Rgba32>[file.header.dwMipMapCount];

            if (IsSupportedRawFormat(file.header.ddsPixelFormat.DxgiFormat))
            {
                var decoder = GetRawDecoder(file.header.ddsPixelFormat.DxgiFormat);

                var mipMaps = allMipMaps ? file.header.dwMipMapCount : 1;
                for (var mip = 0; mip < mipMaps; mip++)
                {
                    var data = file.Faces[0].MipMaps[mip].Data;
                    var pixelWidth = file.Faces[0].MipMaps[mip].Width;
                    var pixelHeight = file.Faces[0].MipMaps[mip].Height;

                    var image = new Image<Rgba32>((int)pixelWidth, (int)pixelHeight);
                    var output = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight);
                    if (!image.TryGetSinglePixelSpan(out var pixels))
                    {
                        throw new Exception("Cannot get pixel span.");
                    }

                    output.CopyTo(pixels);
                    images[mip] = image;
                }
            }
            else
            {
                var format = file.header.ddsPixelFormat.IsDxt10Format ?
                    file.dxt10Header.dxgiFormat :
                    file.header.ddsPixelFormat.DxgiFormat;
                var decoder = GetDecoder(format, file.header);

                if (decoder == null)
                {
                    throw new NotSupportedException($"This Format is not supported: {format}");
                }

                for (var mip = 0; mip < file.header.dwMipMapCount; mip++)
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
                case GlInternalFormat.GlR8:
                case GlInternalFormat.GlRg8:
                case GlInternalFormat.GlRgb8:
                case GlInternalFormat.GlRgba8:
                    return true;

                default:
                    return false;
            }
        }

        private bool IsSupportedRawFormat(DxgiFormat format)
        {
            switch (format)
            {
                case DxgiFormat.DxgiFormatR8Unorm:
                case DxgiFormat.DxgiFormatR8G8Unorm:
                case DxgiFormat.DxgiFormatR8G8B8A8Unorm:
                    return true;

                default:
                    return false;
            }
        }

        private IBcBlockDecoder GetDecoder(GlInternalFormat format)
        {
            switch (format)
            {
                case GlInternalFormat.GlCompressedRgbS3TcDxt1Ext:
                    return new Bc1NoAlphaDecoder();

                case GlInternalFormat.GlCompressedRgbaS3TcDxt1Ext:
                    return new Bc1ADecoder();

                case GlInternalFormat.GlCompressedRgbaS3TcDxt3Ext:
                    return new Bc2Decoder();

                case GlInternalFormat.GlCompressedRgbaS3TcDxt5Ext:
                    return new Bc3Decoder();

                case GlInternalFormat.GlCompressedRedRgtc1Ext:
                    return new Bc4Decoder(OutputOptions.RedAsLuminance);

                case GlInternalFormat.GlCompressedRedGreenRgtc2Ext:
                    return new Bc5Decoder();

                case GlInternalFormat.GlCompressedRgbaBptcUnormArb:
                    return new Bc7Decoder();

                // TODO: Not sure what to do with SRGB input.
                case GlInternalFormat.GlCompressedSrgbAlphaBptcUnormArb:
                    return new Bc7Decoder();

                default:
                    return null;
            }
        }

        private IBcBlockDecoder GetDecoder(DxgiFormat format, DdsHeader header)
        {
            switch (format)
            {
                case DxgiFormat.DxgiFormatBc1Unorm:
                case DxgiFormat.DxgiFormatBc1UnormSrgb:
                case DxgiFormat.DxgiFormatBc1Typeless:
                    if ((header.ddsPixelFormat.dwFlags & PixelFormatFlags.DdpfAlphapixels) != 0)
                        return new Bc1ADecoder();

                    if (InputOptions.DdsBc1ExpectAlpha)
                        return new Bc1ADecoder();

                    return new Bc1NoAlphaDecoder();

                case DxgiFormat.DxgiFormatBc2Unorm:
                case DxgiFormat.DxgiFormatBc2UnormSrgb:
                case DxgiFormat.DxgiFormatBc2Typeless:
                    return new Bc2Decoder();

                case DxgiFormat.DxgiFormatBc3Unorm:
                case DxgiFormat.DxgiFormatBc3UnormSrgb:
                case DxgiFormat.DxgiFormatBc3Typeless:
                    return new Bc3Decoder();

                case DxgiFormat.DxgiFormatBc4Unorm:
                case DxgiFormat.DxgiFormatBc4Snorm:
                case DxgiFormat.DxgiFormatBc4Typeless:
                    return new Bc4Decoder(OutputOptions.RedAsLuminance);

                case DxgiFormat.DxgiFormatBc5Unorm:
                case DxgiFormat.DxgiFormatBc5Snorm:
                case DxgiFormat.DxgiFormatBc5Typeless:
                    return new Bc5Decoder();

                case DxgiFormat.DxgiFormatBc7Unorm:
                case DxgiFormat.DxgiFormatBc7UnormSrgb:
                case DxgiFormat.DxgiFormatBc7Typeless:
                    return new Bc7Decoder();

                default:
                    return null;
            }
        }

        private IRawDecoder GetRawDecoder(GlInternalFormat format)
        {
            switch (format)
            {
                case GlInternalFormat.GlR8:
                    return new RawRDecoder(OutputOptions.RedAsLuminance);

                case GlInternalFormat.GlRg8:
                    return new RawRgDecoder();

                case GlInternalFormat.GlRgb8:
                    return new RawRgbDecoder();

                case GlInternalFormat.GlRgba8:
                    return new RawRgbaDecoder();

                default:
                    return null;
            }
        }

        private IRawDecoder GetRawDecoder(DxgiFormat format)
        {
            switch (format)
            {
                case DxgiFormat.DxgiFormatR8Unorm:
                    return new RawRDecoder(OutputOptions.RedAsLuminance);

                case DxgiFormat.DxgiFormatR8G8Unorm:
                    return new RawRgDecoder();

                case DxgiFormat.DxgiFormatR8G8B8A8Unorm:
                    return new RawRgbaDecoder();

                default:
                    return null;
            }
        }

        private IBcBlockDecoder GetDecoder(CompressionFormat format)
        {
            switch (format)
            {
                case CompressionFormat.Bc1:
                    return new Bc1NoAlphaDecoder();

                case CompressionFormat.Bc1WithAlpha:
                    return new Bc1ADecoder();

                case CompressionFormat.Bc2:
                    return new Bc2Decoder();

                case CompressionFormat.Bc3:
                    return new Bc3Decoder();

                case CompressionFormat.Bc4:
                    return new Bc4Decoder(OutputOptions.RedAsLuminance);

                case CompressionFormat.Bc5:
                    return new Bc5Decoder();

                case CompressionFormat.Bc7:
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
                    return new RawRDecoder(OutputOptions.RedAsLuminance);

                case CompressionFormat.Rg:
                    return new RawRgDecoder();

                case CompressionFormat.Rgb:
                    return new RawRgbDecoder();

                case CompressionFormat.Rgba:
                    return new RawRgbaDecoder();

                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
    }
}
