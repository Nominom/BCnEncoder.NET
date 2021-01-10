using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BCnEncoder.Encoder.Bc7;
using BCnEncoder.Encoder.Options;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.Encoder
{
    /// <summary>
	/// Handles all encoding of images into compressed or uncompressed formats. For decoding, <see cref="Decoder.BcDecoder"/>
	/// </summary>
	public class BcEncoder
    {
        /// <summary>
        /// The input options of the encoder.
        /// </summary>
		public EncoderInputOptions InputOptions { get; } = new EncoderInputOptions();

        /// <summary>
        /// The output options of the encoder.
        /// </summary>
		public EncoderOutputOptions OutputOptions { get; } = new EncoderOutputOptions();

        /// <summary>
        /// The encoder options.
        /// </summary>
        public EncoderOptions Options { get; } = new EncoderOptions();

        /// <summary>
        /// Creates a new instance of <see cref="BcEncoder"/>.
        /// </summary>
        /// <param name="format">The block compression format to encode an image with.</param>
        public BcEncoder(CompressionFormat format = CompressionFormat.BC1)
        {
            OutputOptions.format = format;
        }

        /// <summary>
        /// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream.
        /// </summary>
        /// <param name="inputImage">The image to encode.</param>
        /// <param name="outputStream">The stream to write the encoded image to.</param>
        public void Encode(Image<Rgba32> inputImage, Stream outputStream)
        {
            switch (OutputOptions.fileFormat)
            {
                case OutputFileFormat.Dds:
                    var dds = EncodeToDds(inputImage);
                    dds.Write(outputStream);
                    break;

                case OutputFileFormat.Ktx:
                    var ktx = EncodeToKtx(inputImage);
                    ktx.Write(outputStream);
                    break;
            }
        }

        /// <summary>
        /// Encodes all mipmap levels into a Ktx file.
        /// </summary>
        /// <param name="inputImage">The image to encode.</param>
        /// <returns>The Ktx file containing the encoded image.</returns>
        public KtxFile EncodeToKtx(Image<Rgba32> inputImage)
        {
            KtxFile output;
            IBcBlockEncoder compressedEncoder = null;
            IRawEncoder uncompressedEncoder = null;

            var numMipMaps = OutputOptions.generateMipMaps ? (uint)OutputOptions.maxMipMapLevel : 1;
            var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

            // Setup encoders
            var isCompressedFormat = OutputOptions.format.IsCompressedFormat();
            if (isCompressedFormat)
            {
                compressedEncoder = GetEncoder(OutputOptions.format);
                if (compressedEncoder == null)
                    throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");

                output = new KtxFile(
                    KtxHeader.InitializeCompressed(inputImage.Width, inputImage.Height,
                        compressedEncoder.GetInternalFormat(),
                        compressedEncoder.GetBaseInternalFormat()));
            }
            else
            {
                uncompressedEncoder = GetRawEncoder(OutputOptions.format);
                output = new KtxFile(
                    KtxHeader.InitializeUncompressed(inputImage.Width, inputImage.Height,
                        uncompressedEncoder.GetGlType(),
                        uncompressedEncoder.GetGlFormat(),
                        uncompressedEncoder.GetGlTypeSize(),
                        uncompressedEncoder.GetInternalFormat(),
                        uncompressedEncoder.GetBaseInternalFormat()));
            }

            // Encode mipmap levels
            for (var mip = 0; mip < numMipMaps; mip++)
            {
                byte[] encoded;
                if (isCompressedFormat)
                {
                    var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out int blocksWidth, out int blocksHeight);
                    encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality,
                        !Debugger.IsAttached && Options.multiThreaded);
                }
                else
                {
                    if (!mipChain[mip].TryGetSinglePixelSpan(out var mipPixels))
                        throw new Exception("Cannot get pixel span.");

                    encoded = uncompressedEncoder.Encode(mipPixels);
                }

                output.MipMaps.Add(new KtxMipmap((uint)encoded.Length,
                    (uint)inputImage.Width,
                    (uint)inputImage.Height, 1));
                output.MipMaps[mip].Faces[0] = new KtxMipFace(encoded,
                    (uint)inputImage.Width,
                    (uint)inputImage.Height);
            }

            // Dispose all mipmap levels
            foreach (var image in mipChain)
                image.Dispose();

            output.Header.NumberOfFaces = 1;
            output.Header.NumberOfMipmapLevels = numMipMaps;

            return output;
        }

        /// <summary>
        /// Encodes all mipmap levels into a Dds file.
        /// </summary>
        /// <param name="inputImage">The image to encode.</param>
        /// <returns>The Dds file containing the encoded image.</returns>
        public DdsFile EncodeToDds(Image<Rgba32> inputImage)
        {
            DdsFile output;
            IBcBlockEncoder compressedEncoder = null;
            IRawEncoder uncompressedEncoder = null;

            var numMipMaps = OutputOptions.generateMipMaps ? (uint)OutputOptions.maxMipMapLevel : 1;
            var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

            // Setup encoder
            var isCompressedFormat = OutputOptions.format.IsCompressedFormat();
            if (isCompressedFormat)
            {
                compressedEncoder = GetEncoder(OutputOptions.format);
                if (compressedEncoder == null)
                    throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");

                var (ddsHeader, dxt10Header) = DdsHeader.InitializeCompressed(inputImage.Width, inputImage.Height,
                    compressedEncoder.GetDxgiFormat());
                output = new DdsFile(ddsHeader, dxt10Header);

                if (OutputOptions.ddsBc1WriteAlphaFlag &&
                    OutputOptions.format == CompressionFormat.BC1WithAlpha)
                {
                    output.Header.ddsPixelFormat.dwFlags |= PixelFormatFlags.DDPF_ALPHAPIXELS;
                }
            }
            else
            {
                uncompressedEncoder = GetRawEncoder(OutputOptions.format);
                var ddsHeader = DdsHeader.InitializeUncompressed(inputImage.Width, inputImage.Height,
                    uncompressedEncoder.GetDxgiFormat());
                output = new DdsFile(ddsHeader);
            }

            // Encode mipmap levels
            for (var mip = 0; mip < numMipMaps; mip++)
            {
                byte[] encoded;
                if (isCompressedFormat)
                {
                    var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out var blocksWidth, out var blocksHeight);
                    encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality,
                        !Debugger.IsAttached && Options.multiThreaded);
                }
                else
                {
                    if (!mipChain[mip].TryGetSinglePixelSpan(out var mipPixels))
                        throw new Exception("Cannot get pixel span.");

                    encoded = uncompressedEncoder.Encode(mipPixels);
                }

                if (mip == 0)
                {
                    output.Faces.Add(new DdsFace((uint)inputImage.Width, (uint)inputImage.Height,
                        (uint)encoded.Length, (int)numMipMaps));
                }

                output.Faces[0].MipMaps[mip] = new DdsMipMap(encoded,
                    (uint)inputImage.Width,
                    (uint)inputImage.Height);
            }

            // Dispose all mipmap levels
            foreach (var image in mipChain)
                image.Dispose();

            output.Header.dwMipMapCount = numMipMaps;
            if (numMipMaps > 1)
                output.Header.dwCaps |= HeaderCaps.DDSCAPS_COMPLEX | HeaderCaps.DDSCAPS_MIPMAP;

            return output;
        }

        /// <summary>
        /// Encodes all mipmap levels into a list of byte buffers.
        /// </summary>
        /// <param name="inputImage">The image to encode.</param>
        /// <returns>A list of raw encoded data.</returns>
        public IList<byte[]> EncodeToRawBytes(Image<Rgba32> inputImage)
        {
            var output = new List<byte[]>();
            IBcBlockEncoder compressedEncoder = null;
            IRawEncoder uncompressedEncoder = null;

            var numMipMaps = OutputOptions.generateMipMaps ? (uint)OutputOptions.maxMipMapLevel : 1;
            var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

            // Setup encoder
            var isCompressedFormat = OutputOptions.format.IsCompressedFormat();
            if (isCompressedFormat)
            {
                compressedEncoder = GetEncoder(OutputOptions.format);
                if (compressedEncoder == null)
                    throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
            }
            else
            {
                uncompressedEncoder = GetRawEncoder(OutputOptions.format);
            }

            // Encode all mipmap levels
            for (var mip = 0; mip < numMipMaps; mip++)
            {
                byte[] encoded;
                if (isCompressedFormat)
                {
                    var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out int blocksWidth, out int blocksHeight);
                    encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality,
                        !Debugger.IsAttached && Options.multiThreaded);
                }
                else
                {
                    if (!mipChain[mip].TryGetSinglePixelSpan(out var mipPixels))
                        throw new Exception("Cannot get pixel span.");

                    encoded = uncompressedEncoder.Encode(mipPixels);
                }

                output.Add(encoded);
            }

            // Dispose all mipmap levels
            foreach (var image in mipChain)
                image.Dispose();

            return output;
        }

        /// <summary>
        /// Encodes a single mip level of the input image to a byte buffer.
        /// </summary>
        /// <param name="inputImage">The image to encode.</param>
        /// <param name="mipLevel">The mipmap to encode.</param>
        /// <param name="mipWidth">The width of the mipmap.</param>
        /// <param name="mipHeight">The height of the mipmap.</param>
        /// <returns>The raw encoded data.</returns>
        public byte[] EncodeToRawBytes(Image<Rgba32> inputImage, int mipLevel, out int mipWidth, out int mipHeight)
        {
            if (mipLevel < 0)
                throw new ArgumentException($"{nameof(mipLevel)} cannot be less than zero.");

            IBcBlockEncoder compressedEncoder = null;
            IRawEncoder uncompressedEncoder = null;

            var numMipMaps = OutputOptions.generateMipMaps ? (uint)OutputOptions.maxMipMapLevel : 1;
            var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

            // Setup encoder
            var isCompressedFormat = OutputOptions.format.IsCompressedFormat();
            if (isCompressedFormat)
            {
                compressedEncoder = GetEncoder(OutputOptions.format);
                if (compressedEncoder == null)
                    throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
            }
            else
            {
                uncompressedEncoder = GetRawEncoder(OutputOptions.format);
            }

            // Dispose all mipmap levels
            if (mipLevel > numMipMaps - 1)
            {
                foreach (var image in mipChain)
                    image.Dispose();

                throw new ArgumentException($"{nameof(mipLevel)} cannot be more than number of mipmaps.");
            }

            // Encode mipmap level
            byte[] encoded;
            if (isCompressedFormat)
            {
                var blocks = ImageToBlocks.ImageTo4X4(mipChain[mipLevel].Frames[0], out int blocksWidth, out int blocksHeight);
                encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality,
                    !Debugger.IsAttached && Options.multiThreaded);
            }
            else
            {
                if (!mipChain[mipLevel].TryGetSinglePixelSpan(out var mipPixels))
                    throw new Exception("Cannot get pixel span.");

                encoded = uncompressedEncoder.Encode(mipPixels);
            }

            mipWidth = mipChain[mipLevel].Width;
            mipHeight = mipChain[mipLevel].Height;

            // Dispose all mipmap levels
            foreach (var image in mipChain)
                image.Dispose();

            return encoded;
        }

        /// <summary>
        /// Encodes all cubemap faces and mipmap levels into Ktx file and writes it to the output stream.
        /// Order is +X, -X, +Y, -Y, +Z, -Z
        /// </summary>
        /// <param name="right">The right face of the cubemap.</param>
        /// <param name="left">The right face of the cubemap.</param>
        /// <param name="top">The right face of the cubemap.</param>
        /// <param name="down">The right face of the cubemap.</param>
        /// <param name="back">The right face of the cubemap.</param>
        /// <param name="front">The right face of the cubemap.</param>
        /// <param name="outputStream">The stream to write the encoded image to.</param>
        public void EncodeCubeMap(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
            Image<Rgba32> back, Image<Rgba32> front, Stream outputStream)
        {
            switch (OutputOptions.fileFormat)
            {
                case OutputFileFormat.Ktx:
                    var ktx = EncodeCubeMapToKtx(right, left, top, down, back, front);
                    ktx.Write(outputStream);
                    break;

                case OutputFileFormat.Dds:
                    var dds = EncodeCubeMapToDds(right, left, top, down, back, front);
                    dds.Write(outputStream);
                    break;
            }
        }

        /// <summary>
        /// Encodes all cubemap faces and mipmap levels into a Ktx file.
        /// Order is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
        /// </summary>
        /// <param name="right">The right face of the cubemap.</param>
        /// <param name="left">The right face of the cubemap.</param>
        /// <param name="top">The right face of the cubemap.</param>
        /// <param name="down">The right face of the cubemap.</param>
        /// <param name="back">The right face of the cubemap.</param>
        /// <param name="front">The right face of the cubemap.</param>
        /// <returns>The Ktx file containing the encoded image.</returns>
        public KtxFile EncodeCubeMapToKtx(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
            Image<Rgba32> back, Image<Rgba32> front)
        {
            KtxFile output;
            IBcBlockEncoder compressedEncoder = null;
            IRawEncoder uncompressedEncoder = null;

            if (right.Width != left.Width || right.Width != top.Width || right.Width != down.Width
                || right.Width != back.Width || right.Width != front.Width ||
                right.Height != left.Height || right.Height != top.Height || right.Height != down.Height
                || right.Height != back.Height || right.Height != front.Height)
            {
                throw new ArgumentException("All input images of a cubemap should be the same size.");
            }

            var faces = new[] { right, left, top, down, back, front };

            // Setup encoder
            var isCompressedFormat = OutputOptions.format.IsCompressedFormat();
            if (isCompressedFormat)
            {
                compressedEncoder = GetEncoder(OutputOptions.format);
                if (compressedEncoder == null)
                    throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");

                output = new KtxFile(
                    KtxHeader.InitializeCompressed(right.Width, right.Height,
                        compressedEncoder.GetInternalFormat(),
                        compressedEncoder.GetBaseInternalFormat()));
            }
            else
            {
                uncompressedEncoder = GetRawEncoder(OutputOptions.format);
                output = new KtxFile(
                    KtxHeader.InitializeUncompressed(right.Width, right.Height,
                        uncompressedEncoder.GetGlType(),
                        uncompressedEncoder.GetGlFormat(),
                        uncompressedEncoder.GetGlTypeSize(),
                        uncompressedEncoder.GetInternalFormat(),
                        uncompressedEncoder.GetBaseInternalFormat()));

            }

            var numMipMaps = OutputOptions.generateMipMaps ? (uint)OutputOptions.maxMipMapLevel : 1;
            var mipLength = MipMapper.CalculateMipChainLength(right.Width, right.Height, numMipMaps);
            for (uint i = 0; i < mipLength; i++)
            {
                output.MipMaps.Add(new KtxMipmap(0, 0, 0, (uint)faces.Length));
            }

            // Encode all faces
            for (var face = 0; face < faces.Length; face++)
            {
                var mipChain = MipMapper.GenerateMipChain(faces[face], ref numMipMaps);

                // Encode all mipmap levels per face
                for (var mip = 0; mip < numMipMaps; mip++)
                {
                    byte[] encoded;
                    if (isCompressedFormat)
                    {
                        var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out int blocksWidth, out int blocksHeight);
                        encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality,
                            !Debugger.IsAttached && Options.multiThreaded);
                    }
                    else
                    {
                        if (!mipChain[mip].TryGetSinglePixelSpan(out var mipPixels))
                            throw new Exception("Cannot get pixel span.");

                        encoded = uncompressedEncoder.Encode(mipPixels);
                    }

                    if (face == 0)
                    {
                        output.MipMaps[mip] = new KtxMipmap((uint)encoded.Length,
                            (uint)mipChain[mip].Width,
                            (uint)mipChain[mip].Height, (uint)faces.Length);
                    }

                    output.MipMaps[mip].Faces[face] = new KtxMipFace(encoded,
                        (uint)mipChain[mip].Width,
                        (uint)mipChain[mip].Height);
                }

                // Dispose all mipmap levels
                foreach (var image in mipChain)
                    image.Dispose();
            }

            output.Header.NumberOfFaces = (uint)faces.Length;
            output.Header.NumberOfMipmapLevels = mipLength;

            return output;
        }

        /// <summary>
        /// Encodes all cubemap faces and mipmap levels into a Dds file.
        /// Order is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
        /// </summary>
        /// <param name="right">The right face of the cubemap.</param>
        /// <param name="left">The right face of the cubemap.</param>
        /// <param name="top">The right face of the cubemap.</param>
        /// <param name="down">The right face of the cubemap.</param>
        /// <param name="back">The right face of the cubemap.</param>
        /// <param name="front">The right face of the cubemap.</param>
        /// <returns>The Dds file containing the encoded image.</returns>
        public DdsFile EncodeCubeMapToDds(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
            Image<Rgba32> back, Image<Rgba32> front)
        {
            DdsFile output;
            IBcBlockEncoder compressedEncoder = null;
            IRawEncoder uncompressedEncoder = null;

            if (right.Width != left.Width || right.Width != top.Width || right.Width != down.Width
                || right.Width != back.Width || right.Width != front.Width ||
                right.Height != left.Height || right.Height != top.Height || right.Height != down.Height
                || right.Height != back.Height || right.Height != front.Height)
            {
                throw new ArgumentException("All input images of a cubemap should be the same size.");
            }

            var faces = new[] { right, left, top, down, back, front };

            // Setup encoder
            var isCompressedFormat = OutputOptions.format.IsCompressedFormat();
            if (isCompressedFormat)
            {
                compressedEncoder = GetEncoder(OutputOptions.format);
                if (compressedEncoder == null)
                    throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");

                var (ddsHeader, dxt10Header) = DdsHeader.InitializeCompressed(right.Width, right.Height,
                    compressedEncoder.GetDxgiFormat());
                output = new DdsFile(ddsHeader, dxt10Header);

                if (OutputOptions.ddsBc1WriteAlphaFlag &&
                    OutputOptions.format == CompressionFormat.BC1WithAlpha)
                {
                    output.Header.ddsPixelFormat.dwFlags |= PixelFormatFlags.DDPF_ALPHAPIXELS;
                }
            }
            else
            {
                uncompressedEncoder = GetRawEncoder(OutputOptions.format);
                var ddsHeader = DdsHeader.InitializeUncompressed(right.Width, right.Height,
                    uncompressedEncoder.GetDxgiFormat());

                output = new DdsFile(ddsHeader);
            }

            var numMipMaps = OutputOptions.generateMipMaps ? (uint)OutputOptions.maxMipMapLevel : 1;

            // Encode all faces
            for (var face = 0; face < faces.Length; face++)
            {
                var mipChain = MipMapper.GenerateMipChain(faces[face], ref numMipMaps);

                // Encode all mipmap levels per face
                for (var mip = 0; mip < numMipMaps; mip++)
                {
                    byte[] encoded;
                    if (isCompressedFormat)
                    {
                        var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out int blocksWidth, out int blocksHeight);
                        encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality,
                            !Debugger.IsAttached && Options.multiThreaded);
                    }
                    else
                    {
                        if (!mipChain[mip].TryGetSinglePixelSpan(out var mipPixels))
                            throw new Exception("Cannot get pixel span.");

                        encoded = uncompressedEncoder.Encode(mipPixels);
                    }

                    if (mip == 0)
                    {
                        output.Faces.Add(new DdsFace((uint)mipChain[mip].Width, (uint)mipChain[mip].Height,
                            (uint)encoded.Length, mipChain.Count));
                    }

                    output.Faces[face].MipMaps[mip] = new DdsMipMap(encoded,
                        (uint)mipChain[mip].Width,
                        (uint)mipChain[mip].Height);
                }

                // Dispose all mipmap levels
                foreach (var image in mipChain)
                    image.Dispose();
            }

            output.Header.dwCaps |= HeaderCaps.DDSCAPS_COMPLEX;
            output.Header.dwMipMapCount = numMipMaps;
            if (numMipMaps > 1)
            {
                output.Header.dwCaps |= HeaderCaps.DDSCAPS_MIPMAP;
            }
            output.Header.dwCaps2 |= HeaderCaps2.DDSCAPS2_CUBEMAP |
                              HeaderCaps2.DDSCAPS2_CUBEMAP_POSITIVEX |
                              HeaderCaps2.DDSCAPS2_CUBEMAP_NEGATIVEX |
                              HeaderCaps2.DDSCAPS2_CUBEMAP_POSITIVEY |
                              HeaderCaps2.DDSCAPS2_CUBEMAP_NEGATIVEY |
                              HeaderCaps2.DDSCAPS2_CUBEMAP_POSITIVEZ |
                              HeaderCaps2.DDSCAPS2_CUBEMAP_NEGATIVEZ;

            return output;
        }

        private IBcBlockEncoder GetEncoder(CompressionFormat format)
        {
            switch (format)
            {
                case CompressionFormat.BC1:
                    return new Bc1BlockEncoder();

                case CompressionFormat.BC1WithAlpha:
                    return new Bc1AlphaBlockEncoder();

                case CompressionFormat.BC2:
                    return new Bc2BlockEncoder();

                case CompressionFormat.BC3:
                    return new Bc3BlockEncoder();

                case CompressionFormat.BC4:
                    return new Bc4BlockEncoder(InputOptions.luminanceAsRed);

                case CompressionFormat.BC5:
                    return new Bc5BlockEncoder();

                case CompressionFormat.BC7:
                    return new Bc7Encoder();

                default:
                    return null;
            }
        }

        private IRawEncoder GetRawEncoder(CompressionFormat format)
        {
            switch (format)
            {
                case CompressionFormat.R:
                    return new RawLuminanceEncoder(InputOptions.luminanceAsRed);

                case CompressionFormat.RG:
                    return new RawRGEncoder();

                case CompressionFormat.RGB:
                    return new RawRGBEncoder();

                case CompressionFormat.RGBA:
                    return new RawRGBAEncoder();

                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
    }
}
