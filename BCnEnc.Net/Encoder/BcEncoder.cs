using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BCnEncoder.Encoder.Bc7;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	public class EncoderInputOptions
	{
		/// <summary>
		/// If true, when encoding to a format that only includes a red channel,
		/// use the pixel luminance instead of just the red channel. Default is false.
		/// </summary>
		public bool luminanceAsRed = false;
	}

	public class EncoderOutputOptions
	{
		/// <summary>
		/// The compression format to use. Default is BC1.
		/// </summary>
		public CompressionFormat format = CompressionFormat.BC1;
		/// <summary>
		/// The quality of the compression. Use either fast or balanced for testing.
		/// Fast can be used for near real-time encoding for most algorithms.
		/// Use bestQuality when needed. Default is balanced.
		/// </summary>
		public CompressionQuality quality = CompressionQuality.Balanced;
		/// <summary>
		/// The output file format of the data. Either Ktx or Dds.
		/// Default is Ktx.
		/// </summary>
		public OutputFileFormat fileFormat = OutputFileFormat.Ktx;
		/// <summary>
		/// The DDS file format doesn't seem to have a standard for indicating whether a BC1 texture
		/// includes 1bit of alpha. This option will write DDPF_ALPHAPIXELS flag to the header
		/// to indicate the presence of an alpha channel. Some programs read and write this flag,
		/// but some programs don't like it and get confused. Your mileage may vary.
		/// Default is false.
		/// </summary>
		public bool ddsBc1WriteAlphaFlag = false;
	}

	public class EncoderOptions {
		/// <summary>
		/// Whether the blocks should be encoded in parallel. This can be much faster than single-threaded encoding,
		/// but is slow if multiple textures are being processed at the same time.
		/// When a debugger is attached, the encoder defaults to single-threaded operation to ease debugging.
		/// Default is true.
		/// </summary>
		public bool multiThreaded = true;
	}

	/// <summary>
	/// Handles all encoding of images into compressed or uncompressed formats. For decoding, <see cref="Decoder.BcDecoder"/>
	/// </summary>
	public class BcEncoder
	{
		public EncoderInputOptions InputOptions { get; set; } = new EncoderInputOptions();
		public EncoderOutputOptions OutputOptions { get; set; } = new EncoderOutputOptions();
		public EncoderOptions Options { get; set; } = new EncoderOptions();

		public BcEncoder() { }
		public BcEncoder(CompressionFormat format)
		{
			OutputOptions.format = format;
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

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream.
		/// </summary>
		public void Encode(byte[] inputImageRgba, int inputImageWidth, int inputImageHeight, Stream outputStream)
		{
			if (OutputOptions.fileFormat == OutputFileFormat.Ktx)
			{
				KtxFile output = EncodeToKtx(inputImageRgba, inputImageWidth, inputImageHeight);
				output.Write(outputStream);
			}
			else if (OutputOptions.fileFormat == OutputFileFormat.Dds)
			{
				DdsFile output = EncodeToDds(inputImageRgba, inputImageWidth, inputImageHeight);
				output.Write(outputStream);
			}
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file.
		/// </summary>
		public KtxFile EncodeToKtx(byte[] inputImageRgba, int inputImageWidth, int inputImageHeight)
		{
			KtxFile output;
			IBcBlockEncoder compressedEncoder = null;
			
			compressedEncoder = GetEncoder(OutputOptions.format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
			}
			output = new KtxFile(
				KtxHeader.InitializeCompressed(inputImageWidth, inputImageHeight,
					compressedEncoder.GetInternalFormat(),
					compressedEncoder.GetBaseInternalFormat()));

			uint numMipMaps = 1;

			byte[] encoded = null;
			if (OutputOptions.format.IsCompressedFormat())
			{
				var blocks = ImageToBlocks.ImageTo4X4(inputImageRgba, inputImageWidth, inputImageHeight, out int blocksWidth, out int blocksHeight);
				encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality,
					!Debugger.IsAttached && Options.multiThreaded);
			}

			output.MipMaps.Add(new KtxMipmap((uint)encoded.Length,
				(uint)inputImageWidth,
				(uint)inputImageHeight, 1));
			output.MipMaps[0].Faces[0] = new KtxMipFace(encoded,
				(uint)inputImageWidth,
				(uint)inputImageHeight);

			output.Header.NumberOfFaces = 1;
			output.Header.NumberOfMipmapLevels = numMipMaps;

			return output;
		}

		/// <summary>
		/// Encodes Rgba image into a Dds file.
		/// </summary>
		public DdsFile EncodeToDds(byte[] inputImageRgba, int inputImageWidth, int inputImageHeight)
		{
			DdsFile output;
			IBcBlockEncoder compressedEncoder = null;
			compressedEncoder = GetEncoder(OutputOptions.format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
			}

			DdsHeaderDxt10 dxt10Header;
			var ddsHeader = DdsHeader.InitializeCompressed(inputImageWidth, inputImageHeight,
				compressedEncoder.GetDxgiFormat(), out dxt10Header);
			
			output = new DdsFile(ddsHeader, dxt10Header);

			if (OutputOptions.ddsBc1WriteAlphaFlag &&
			    OutputOptions.format == CompressionFormat.BC1WithAlpha)
			{
				output.Header.ddsPixelFormat.dwFlags |= PixelFormatFlags.DDPF_ALPHAPIXELS;
			}

			uint numMipMaps = 1;

			var blocks = ImageToBlocks.ImageTo4X4(inputImageRgba, inputImageWidth, inputImageHeight, out int blocksWidth, out int blocksHeight);
			var encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality,
				!Debugger.IsAttached && Options.multiThreaded);

			output.Faces.Add(new DdsFace((uint)inputImageWidth, (uint)inputImageHeight,
				(uint)encoded.Length, (int)numMipMaps));
			

			output.Faces[0].MipMaps[0] = new DdsMipMap(encoded,
				(uint)inputImageWidth,
				(uint)inputImageHeight);

			output.Header.dwMipMapCount = numMipMaps;
			if (numMipMaps > 1)
			{
				output.Header.dwCaps |= HeaderCaps.DDSCAPS_COMPLEX | HeaderCaps.DDSCAPS_MIPMAP;
			}

			return output;
		}

		/// <summary>
		/// Encodes an RGBA byte buffer to raw compressed bytes
		/// </summary>
		public byte[] EncodeToRawBytes(byte[] inputImageRgba, int inputImageWidth, int inputImageHeight, out int outputImageWidth, out int outputImageHeight)
		{
			IBcBlockEncoder compressedEncoder = GetEncoder(OutputOptions.format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
			}

			var blocks = ImageToBlocks.ImageTo4X4(inputImageRgba, inputImageWidth, inputImageHeight, out int blocksWidth, out int blocksHeight);
			var encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality,
				!Debugger.IsAttached && Options.multiThreaded);
			outputImageWidth = blocksWidth * 4;
			outputImageHeight = blocksHeight * 4;

			return encoded;
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer.
		/// </summary>
		//public byte[] EncodeToRawBytes(Image<Rgba32> inputImage, int mipLevel, out int mipWidth, out int mipHeight)
		//{
		//	if (mipLevel < 0)
		//	{
		//		throw new ArgumentException($"{nameof(mipLevel)} cannot be less than zero.");
		//	}

		//	IBcBlockEncoder compressedEncoder = null;
		//	IRawEncoder uncompressedEncoder = null;
		//	if (OutputOptions.format.IsCompressedFormat())
		//	{
		//		compressedEncoder = GetEncoder(OutputOptions.format);
		//		if (compressedEncoder == null)
		//		{
		//			throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
		//		}
		//	}
		//	else
		//	{
		//		uncompressedEncoder = GetRawEncoder(OutputOptions.format);

		//	}

		//	uint numMipMaps = (uint)OutputOptions.maxMipMapLevel;
		//	if (!OutputOptions.generateMipMaps)
		//	{
		//		numMipMaps = 1;
		//	}

		//	var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

		//	if (mipLevel > numMipMaps - 1)
		//	{
		//		foreach (var image in mipChain)
		//		{
		//			image.Dispose();
		//		}
		//		throw new ArgumentException($"{nameof(mipLevel)} cannot be more than number of mipmaps");
		//	}

		//	byte[] encoded = null;
		//	if (OutputOptions.format.IsCompressedFormat())
		//	{
		//		var blocks = ImageToBlocks.ImageTo4X4(mipChain[mipLevel].Frames[0], out int blocksWidth, out int blocksHeight);
		//		encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality, 
		//			!Debugger.IsAttached && Options.multiThreaded);
		//	}
		//	else
		//	{
		//		if (!mipChain[mipLevel].TryGetSinglePixelSpan(out var mipPixels)) {
		//			throw new Exception("Cannot get pixel span.");
		//		}
		//		encoded = uncompressedEncoder.Encode(mipPixels);
		//	}

		//	mipWidth = mipChain[mipLevel].Width;
		//	mipHeight = mipChain[mipLevel].Height;

		//	foreach (var image in mipChain)
		//	{
		//		image.Dispose();
		//	}

		//	return encoded;
		//}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into Ktx file and writes it to the output stream.
		/// Order is +X, -X, +Y, -Y, +Z, -Z
		/// </summary>
		//public void EncodeCubeMap(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
		//	Image<Rgba32> back, Image<Rgba32> front, Stream outputStream)
		//{
		//	if (OutputOptions.fileFormat == OutputFileFormat.Ktx)
		//	{
		//		KtxFile output = EncodeCubeMapToKtx(right, left, top, down, back, front);
		//		output.Write(outputStream);
		//	}
		//	else if (OutputOptions.fileFormat == OutputFileFormat.Dds)
		//	{
		//		DdsFile output = EncodeCubeMapToDds(right, left, top, down, back, front);
		//		output.Write(outputStream);
		//	}
		//}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into a Ktx file.
		/// Order is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		//public KtxFile EncodeCubeMapToKtx(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
		//	Image<Rgba32> back, Image<Rgba32> front)
		//{
		//	KtxFile output;
		//	IBcBlockEncoder compressedEncoder = null;
		//	IRawEncoder uncompressedEncoder = null;

		//	if (right.Width != left.Width || right.Width != top.Width || right.Width != down.Width
		//		|| right.Width != back.Width || right.Width != front.Width ||
		//		right.Height != left.Height || right.Height != top.Height || right.Height != down.Height
		//		|| right.Height != back.Height || right.Height != front.Height)
		//	{
		//		throw new ArgumentException("All input images of a cubemap should be the same size.");
		//	}

		//	Image<Rgba32>[] faces = new[] { right, left, top, down, back, front };

		//	if (OutputOptions.format.IsCompressedFormat())
		//	{
		//		compressedEncoder = GetEncoder(OutputOptions.format);
		//		if (compressedEncoder == null)
		//		{
		//			throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
		//		}
		//		output = new KtxFile(
		//			KtxHeader.InitializeCompressed(right.Width, right.Height,
		//				compressedEncoder.GetInternalFormat(),
		//				compressedEncoder.GetBaseInternalFormat()));
		//	}
		//	else
		//	{
		//		uncompressedEncoder = GetRawEncoder(OutputOptions.format);
		//		output = new KtxFile(
		//			KtxHeader.InitializeUncompressed(right.Width, right.Height,
		//				uncompressedEncoder.GetGlType(),
		//				uncompressedEncoder.GetGlFormat(),
		//				uncompressedEncoder.GetGlTypeSize(),
		//				uncompressedEncoder.GetInternalFormat(),
		//				uncompressedEncoder.GetBaseInternalFormat()));

		//	}
		//	uint numMipMaps = (uint)OutputOptions.maxMipMapLevel;
		//	if (!OutputOptions.generateMipMaps)
		//	{
		//		numMipMaps = 1;
		//	}

		//	uint mipLength = MipMapper.CalculateMipChainLength(right.Width, right.Height, numMipMaps);
		//	for (uint i = 0; i < mipLength; i++)
		//	{
		//		output.MipMaps.Add(new KtxMipmap(0, 0, 0, (uint)faces.Length));
		//	}

		//	for (int f = 0; f < faces.Length; f++)
		//	{

		//		var mipChain = MipMapper.GenerateMipChain(faces[f], ref numMipMaps);

		//		for (int i = 0; i < numMipMaps; i++)
		//		{
		//			byte[] encoded = null;
		//			if (OutputOptions.format.IsCompressedFormat())
		//			{
		//				var blocks = ImageToBlocks.ImageTo4X4(mipChain[i].Frames[0], out int blocksWidth, out int blocksHeight);
		//				encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality, 
		//					!Debugger.IsAttached && Options.multiThreaded);
		//			}
		//			else
		//			{
		//				if (!mipChain[i].TryGetSinglePixelSpan(out var mipPixels)) {
		//					throw new Exception("Cannot get pixel span.");
		//				}
		//				encoded = uncompressedEncoder.Encode(mipPixels);
		//			}

		//			if (f == 0)
		//			{
		//				output.MipMaps[i] = new KtxMipmap((uint)encoded.Length,
		//					(uint)mipChain[i].Width,
		//					(uint)mipChain[i].Height, (uint)faces.Length);
		//			}

		//			output.MipMaps[i].Faces[f] = new KtxMipFace(encoded,
		//				(uint)mipChain[i].Width,
		//				(uint)mipChain[i].Height);
		//		}

		//		foreach (var image in mipChain)
		//		{
		//			image.Dispose();
		//		}
		//	}

		//	output.Header.NumberOfFaces = (uint)faces.Length;
		//	output.Header.NumberOfMipmapLevels = mipLength;

		//	return output;
		//}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into a Dds file.
		/// Order is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		//public DdsFile EncodeCubeMapToDds(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
		//	Image<Rgba32> back, Image<Rgba32> front)
		//{
		//	DdsFile output;
		//	IBcBlockEncoder compressedEncoder = null;
		//	IRawEncoder uncompressedEncoder = null;

		//	if (right.Width != left.Width || right.Width != top.Width || right.Width != down.Width
		//		|| right.Width != back.Width || right.Width != front.Width ||
		//		right.Height != left.Height || right.Height != top.Height || right.Height != down.Height
		//		|| right.Height != back.Height || right.Height != front.Height)
		//	{
		//		throw new ArgumentException("All input images of a cubemap should be the same size.");
		//	}

		//	Image<Rgba32>[] faces = new[] { right, left, top, down, back, front };

		//	if (OutputOptions.format.IsCompressedFormat())
		//	{
		//		compressedEncoder = GetEncoder(OutputOptions.format);
		//		if (compressedEncoder == null)
		//		{
		//			throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
		//		}

		//		var (ddsHeader, dxt10Header) = DdsHeader.InitializeCompressed(right.Width, right.Height,
		//			compressedEncoder.GetDxgiFormat());
		//		output = new DdsFile(ddsHeader, dxt10Header);

		//		if (OutputOptions.ddsBc1WriteAlphaFlag &&
		//		    OutputOptions.format == CompressionFormat.BC1WithAlpha)
		//		{
		//			output.Header.ddsPixelFormat.dwFlags |= PixelFormatFlags.DDPF_ALPHAPIXELS;
		//		}
		//	}
		//	else
		//	{
		//		uncompressedEncoder = GetRawEncoder(OutputOptions.format);
		//		var ddsHeader = DdsHeader.InitializeUncompressed(right.Width, right.Height,
		//			uncompressedEncoder.GetDxgiFormat());
		//		output = new DdsFile(ddsHeader);
		//	}

		//	uint numMipMaps = (uint)OutputOptions.maxMipMapLevel;
		//	if (!OutputOptions.generateMipMaps)
		//	{
		//		numMipMaps = 1;
		//	}

		//	for (int f = 0; f < faces.Length; f++)
		//	{

		//		var mipChain = MipMapper.GenerateMipChain(faces[f], ref numMipMaps);


		//		for (int mip = 0; mip < numMipMaps; mip++)
		//		{
		//			byte[] encoded = null;
		//			if (OutputOptions.format.IsCompressedFormat())
		//			{
		//				var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out int blocksWidth, out int blocksHeight);
		//				encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality, 
		//					!Debugger.IsAttached && Options.multiThreaded);
		//			}
		//			else
		//			{
		//				if (!mipChain[mip].TryGetSinglePixelSpan(out var mipPixels)) {
		//					throw new Exception("Cannot get pixel span.");
		//				}
		//				encoded = uncompressedEncoder.Encode(mipPixels);
		//			}

		//			if (mip == 0)
		//			{
		//				output.Faces.Add(new DdsFace((uint)mipChain[mip].Width, (uint)mipChain[mip].Height,
		//					(uint)encoded.Length, mipChain.Count));
		//			}

		//			output.Faces[f].MipMaps[mip] = new DdsMipMap(encoded,
		//				(uint)mipChain[mip].Width,
		//				(uint)mipChain[mip].Height);
		//		}

		//		foreach (var image in mipChain)
		//		{
		//			image.Dispose();
		//		}
		//	}

		//	output.Header.dwCaps |= HeaderCaps.DDSCAPS_COMPLEX;
		//	output.Header.dwMipMapCount = numMipMaps;
		//	if (numMipMaps > 1)
		//	{
		//		output.Header.dwCaps |= HeaderCaps.DDSCAPS_MIPMAP;
		//	}
		//	output.Header.dwCaps2 |= HeaderCaps2.DDSCAPS2_CUBEMAP |
		//					  HeaderCaps2.DDSCAPS2_CUBEMAP_POSITIVEX |
		//					  HeaderCaps2.DDSCAPS2_CUBEMAP_NEGATIVEX |
		//					  HeaderCaps2.DDSCAPS2_CUBEMAP_POSITIVEY |
		//					  HeaderCaps2.DDSCAPS2_CUBEMAP_NEGATIVEY |
		//					  HeaderCaps2.DDSCAPS2_CUBEMAP_POSITIVEZ |
		//					  HeaderCaps2.DDSCAPS2_CUBEMAP_NEGATIVEZ;

		//	return output;
		//}
	}
}
