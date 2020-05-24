using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BCnEncoder.Encoder.Bc7;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

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
		/// Whether to generate mipMaps. Default is true.
		/// </summary>
		public bool generateMipMaps = true;
		/// <summary>
		/// The maximum number of mipmap levels to generate. -1 or 0 is unbounded.
		/// Default is -1.
		/// </summary>
		public int maxMipMapLevel = -1;
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

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream.
		/// </summary>
		public void Encode(Image<Rgba32> inputImage, Stream outputStream)
		{
			if (OutputOptions.fileFormat == OutputFileFormat.Ktx)
			{
				KtxFile output = EncodeToKtx(inputImage);
				output.Write(outputStream);
			}
			else if (OutputOptions.fileFormat == OutputFileFormat.Dds)
			{
				DdsFile output = EncodeToDds(inputImage);
				output.Write(outputStream);
			}
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file.
		/// </summary>
		public KtxFile EncodeToKtx(Image<Rgba32> inputImage)
		{
			KtxFile output;
			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;
			if (OutputOptions.format.IsCompressedFormat())
			{
				compressedEncoder = GetEncoder(OutputOptions.format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
				}
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

			uint numMipMaps = (uint)OutputOptions.maxMipMapLevel;
			if (!OutputOptions.generateMipMaps)
			{
				numMipMaps = 1;
			}

			var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

			for (int i = 0; i < numMipMaps; i++)
			{
				byte[] encoded = null;
				if (OutputOptions.format.IsCompressedFormat())
				{
					var blocks = ImageToBlocks.ImageTo4X4(mipChain[i].Frames[0], out int blocksWidth, out int blocksHeight);
					encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality, 
						!Debugger.IsAttached && Options.multiThreaded);
				}
				else
				{
					encoded = uncompressedEncoder.Encode(mipChain[i].GetPixelSpan());
				}

				output.MipMaps.Add(new KtxMipmap((uint)encoded.Length,
					(uint)inputImage.Width,
					(uint)inputImage.Height, 1));
				output.MipMaps[i].Faces[0] = new KtxMipFace(encoded,
					(uint)inputImage.Width,
					(uint)inputImage.Height);
			}

			foreach (var image in mipChain)
			{
				image.Dispose();
			}

			output.Header.NumberOfFaces = 1;
			output.Header.NumberOfMipmapLevels = numMipMaps;

			return output;
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file.
		/// </summary>
		public DdsFile EncodeToDds(Image<Rgba32> inputImage)
		{
			DdsFile output;
			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;
			if (OutputOptions.format.IsCompressedFormat())
			{
				compressedEncoder = GetEncoder(OutputOptions.format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
				}

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

			uint numMipMaps = (uint)OutputOptions.maxMipMapLevel;
			if (!OutputOptions.generateMipMaps)
			{
				numMipMaps = 1;
			}

			var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

			for (int mip = 0; mip < numMipMaps; mip++)
			{
				byte[] encoded = null;
				if (OutputOptions.format.IsCompressedFormat())
				{
					var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out int blocksWidth, out int blocksHeight);
					encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality, 
						!Debugger.IsAttached && Options.multiThreaded);
				}
				else
				{
					encoded = uncompressedEncoder.Encode(mipChain[mip].GetPixelSpan());
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

			foreach (var image in mipChain)
			{
				image.Dispose();
			}

			output.Header.dwMipMapCount = numMipMaps;
			if (numMipMaps > 1)
			{
				output.Header.dwCaps |= HeaderCaps.DDSCAPS_COMPLEX | HeaderCaps.DDSCAPS_MIPMAP;
			}

			return output;
		}

		/// <summary>
		/// Encodes all mipmap levels into a list of byte buffers.
		/// </summary>
		public List<byte[]> EncodeToRawBytes(Image<Rgba32> inputImage)
		{
			List<byte[]> output = new List<byte[]>();
			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;
			if (OutputOptions.format.IsCompressedFormat())
			{
				compressedEncoder = GetEncoder(OutputOptions.format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
				}

			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.format);

			}

			uint numMipMaps = (uint)OutputOptions.maxMipMapLevel;
			if (!OutputOptions.generateMipMaps)
			{
				numMipMaps = 1;
			}

			var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

			for (int i = 0; i < numMipMaps; i++)
			{
				byte[] encoded = null;
				if (OutputOptions.format.IsCompressedFormat())
				{
					var blocks = ImageToBlocks.ImageTo4X4(mipChain[i].Frames[0], out int blocksWidth, out int blocksHeight);
					encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality, 
						!Debugger.IsAttached && Options.multiThreaded);
				}
				else
				{
					encoded = uncompressedEncoder.Encode(mipChain[i].GetPixelSpan());
				}

				output.Add(encoded);
			}

			foreach (var image in mipChain)
			{
				image.Dispose();
			}

			return output;
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer.
		/// </summary>
		public byte[] EncodeToRawBytes(Image<Rgba32> inputImage, int mipLevel, out int mipWidth, out int mipHeight)
		{
			if (mipLevel < 0)
			{
				throw new ArgumentException($"{nameof(mipLevel)} cannot be less than zero.");
			}

			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;
			if (OutputOptions.format.IsCompressedFormat())
			{
				compressedEncoder = GetEncoder(OutputOptions.format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
				}
			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.format);

			}

			uint numMipMaps = (uint)OutputOptions.maxMipMapLevel;
			if (!OutputOptions.generateMipMaps)
			{
				numMipMaps = 1;
			}

			var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

			if (mipLevel > numMipMaps - 1)
			{
				foreach (var image in mipChain)
				{
					image.Dispose();
				}
				throw new ArgumentException($"{nameof(mipLevel)} cannot be more than number of mipmaps");
			}

			byte[] encoded = null;
			if (OutputOptions.format.IsCompressedFormat())
			{
				var blocks = ImageToBlocks.ImageTo4X4(mipChain[mipLevel].Frames[0], out int blocksWidth, out int blocksHeight);
				encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality, 
					!Debugger.IsAttached && Options.multiThreaded);
			}
			else
			{
				encoded = uncompressedEncoder.Encode(mipChain[mipLevel].GetPixelSpan());
			}

			mipWidth = mipChain[mipLevel].Width;
			mipHeight = mipChain[mipLevel].Height;

			foreach (var image in mipChain)
			{
				image.Dispose();
			}

			return encoded;
		}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into Ktx file and writes it to the output stream.
		/// Order is +X, -X, +Y, -Y, +Z, -Z
		/// </summary>
		public void EncodeCubeMap(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, Stream outputStream)
		{
			if (OutputOptions.fileFormat == OutputFileFormat.Ktx)
			{
				KtxFile output = EncodeCubeMapToKtx(right, left, top, down, back, front);
				output.Write(outputStream);
			}
			else if (OutputOptions.fileFormat == OutputFileFormat.Dds)
			{
				DdsFile output = EncodeCubeMapToDds(right, left, top, down, back, front);
				output.Write(outputStream);
			}
		}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into a Ktx file.
		/// Order is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
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

			Image<Rgba32>[] faces = new[] { right, left, top, down, back, front };

			if (OutputOptions.format.IsCompressedFormat())
			{
				compressedEncoder = GetEncoder(OutputOptions.format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
				}
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
			uint numMipMaps = (uint)OutputOptions.maxMipMapLevel;
			if (!OutputOptions.generateMipMaps)
			{
				numMipMaps = 1;
			}

			uint mipLength = MipMapper.CalculateMipChainLength(right.Width, right.Height, numMipMaps);
			for (uint i = 0; i < mipLength; i++)
			{
				output.MipMaps.Add(new KtxMipmap(0, 0, 0, (uint)faces.Length));
			}

			for (int f = 0; f < faces.Length; f++)
			{

				var mipChain = MipMapper.GenerateMipChain(faces[f], ref numMipMaps);

				for (int i = 0; i < numMipMaps; i++)
				{
					byte[] encoded = null;
					if (OutputOptions.format.IsCompressedFormat())
					{
						var blocks = ImageToBlocks.ImageTo4X4(mipChain[i].Frames[0], out int blocksWidth, out int blocksHeight);
						encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality, 
							!Debugger.IsAttached && Options.multiThreaded);
					}
					else
					{
						encoded = uncompressedEncoder.Encode(mipChain[i].GetPixelSpan());
					}

					if (f == 0)
					{
						output.MipMaps[i] = new KtxMipmap((uint)encoded.Length,
							(uint)mipChain[i].Width,
							(uint)mipChain[i].Height, (uint)faces.Length);
					}

					output.MipMaps[i].Faces[f] = new KtxMipFace(encoded,
						(uint)mipChain[i].Width,
						(uint)mipChain[i].Height);
				}

				foreach (var image in mipChain)
				{
					image.Dispose();
				}
			}

			output.Header.NumberOfFaces = (uint)faces.Length;
			output.Header.NumberOfMipmapLevels = mipLength;

			return output;
		}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into a Dds file.
		/// Order is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
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

			Image<Rgba32>[] faces = new[] { right, left, top, down, back, front };

			if (OutputOptions.format.IsCompressedFormat())
			{
				compressedEncoder = GetEncoder(OutputOptions.format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
				}

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

			uint numMipMaps = (uint)OutputOptions.maxMipMapLevel;
			if (!OutputOptions.generateMipMaps)
			{
				numMipMaps = 1;
			}

			for (int f = 0; f < faces.Length; f++)
			{

				var mipChain = MipMapper.GenerateMipChain(faces[f], ref numMipMaps);


				for (int mip = 0; mip < numMipMaps; mip++)
				{
					byte[] encoded = null;
					if (OutputOptions.format.IsCompressedFormat())
					{
						var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out int blocksWidth, out int blocksHeight);
						encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality, 
							!Debugger.IsAttached && Options.multiThreaded);
					}
					else
					{
						encoded = uncompressedEncoder.Encode(mipChain[mip].GetPixelSpan());
					}

					if (mip == 0)
					{
						output.Faces.Add(new DdsFace((uint)mipChain[mip].Width, (uint)mipChain[mip].Height,
							(uint)encoded.Length, mipChain.Count));
					}

					output.Faces[f].MipMaps[mip] = new DdsMipMap(encoded,
						(uint)mipChain[mip].Width,
						(uint)mipChain[mip].Height);
				}

				foreach (var image in mipChain)
				{
					image.Dispose();
				}
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
	}
}
