using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
		/// <param name="format">The block compression Format to encode an image with.</param>
		public BcEncoder(CompressionFormat format = CompressionFormat.Bc1)
		{
			OutputOptions.Format = format;
		}

		#region Async Api

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream asynchronously.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		public Task EncodeAsync(Image<Rgba32> inputImage, Stream outputStream, CancellationToken token = default)
		{
			return Task.Run(() => EncodeInternal(inputImage, outputStream, token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file asynchronously.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public Task<KtxFile> EncodeToKtxAsync(Image<Rgba32> inputImage, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToKtxInternal(inputImage, token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file asynchronously.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public Task<DdsFile> EncodeToDdsAsync(Image<Rgba32> inputImage, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToDdsInternal(inputImage, token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a list of byte buffers asynchronously.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>A list of raw encoded data.</returns>
		public Task<IList<byte[]>> EncodeToRawBytesAsync(Image<Rgba32> inputImage, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToRawBytesInternal(inputImage, token), token);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer asynchronously.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The raw encoded data.</returns>
		/// <remarks>To get the width and height of the encoded mipLevel, see <see cref="CalculateMipMapSize(Image{Rgba32},int,out int,out int)"/>.</remarks>
		public Task<byte[]> EncodeToRawBytesAsync(Image<Rgba32> inputImage, int mipLevel, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToRawBytesInternal(inputImage, mipLevel, out _, out _, token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a list of byte buffers asynchronously.
		/// </summary>
		/// <param name="inputPixelsRgba">The raw pixels of the input image in rgba format.</param>
		/// <param name="inputImageWidth">The width of the input image.</param>
		/// <param name="inputImageHeight">The height of the input image.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>A list of raw encoded mipmap data.</returns>
		public Task<IList<byte[]>> EncodeToRawBytesAsync(byte[] inputPixelsRgba, int inputImageWidth, int inputImageHeight, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToRawBytesInternal(inputPixelsRgba, inputImageWidth, inputImageHeight, token), token);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer asynchronously.
		/// </summary>
		/// <param name="inputPixelsRgba">The raw pixels of the input image in rgba format.</param>
		/// <param name="inputImageWidth">The width of the input image.</param>
		/// <param name="inputImageHeight">The height of the input image.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The raw encoded data.</returns>
		/// <remarks>To get the width and height of the encoded mipLevel, see <see cref="CalculateMipMapSize(Image{Rgba32},int,out int,out int)"/>.</remarks>
		public Task<byte[]> EncodeToRawBytesAsync(byte[] inputPixelsRgba, int inputImageWidth, int inputImageHeight, int mipLevel, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToRawBytesInternal(inputPixelsRgba, inputImageWidth, inputImageHeight, mipLevel, out _, out _, token), token);
		}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into Ktx file and writes it to the output stream asynchronously.
		/// Order is +X, -X, +Y, -Y, +Z, -Z
		/// </summary>
		/// <param name="right">The right face of the cubemap.</param>
		/// <param name="left">The left face of the cubemap.</param>
		/// <param name="top">The top face of the cubemap.</param>
		/// <param name="down">The bottom face of the cubemap.</param>
		/// <param name="back">The back face of the cubemap.</param>
		/// <param name="front">The front face of the cubemap.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		public Task EncodeCubeMapAsync(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, Stream outputStream, CancellationToken token = default)
		{
			return Task.Run(() => EncodeCubeMapInternal(right, left, top, down, back, front, outputStream, token), token);
		}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into a Ktx file asynchronously.
		/// Order is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The right face of the cubemap.</param>
		/// <param name="left">The left face of the cubemap.</param>
		/// <param name="top">The top face of the cubemap.</param>
		/// <param name="down">The bottom face of the cubemap.</param>
		/// <param name="back">The back face of the cubemap.</param>
		/// <param name="front">The front face of the cubemap.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public Task<KtxFile> EncodeCubeMapToKtxAsync(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top,
			Image<Rgba32> down, Image<Rgba32> back, Image<Rgba32> front, CancellationToken token = default)
		{
			return Task.Run(() => EncodeCubeMapToKtxInternal(right, left, top, down, back, front, token), token);
		}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into a Dds file asynchronously.
		/// Order is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The right face of the cubemap.</param>
		/// <param name="left">The left face of the cubemap.</param>
		/// <param name="top">The top face of the cubemap.</param>
		/// <param name="down">The bottom face of the cubemap.</param>
		/// <param name="back">The back face of the cubemap.</param>
		/// <param name="front">The front face of the cubemap.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public Task<DdsFile> EncodeCubeMapToDdsAsync(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top,
			Image<Rgba32> down, Image<Rgba32> back, Image<Rgba32> front, CancellationToken token = default)
		{
			return Task.Run(() => EncodeCubeMapToDdsInternal(right, left, top, down, back, front, token), token);
		}

		#endregion

		#region Sync Api

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public void Encode(Image<Rgba32> inputImage, Stream outputStream)
		{
			EncodeInternal(inputImage, outputStream, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public KtxFile EncodeToKtx(Image<Rgba32> inputImage)
		{
			return EncodeToKtxInternal(inputImage, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public DdsFile EncodeToDds(Image<Rgba32> inputImage)
		{
			return EncodeToDdsInternal(inputImage, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a list of byte buffers.
		/// </summary>
		/// <param name="inputPixelsRgba">The raw pixels of the input image in rgba format.</param>
		/// <param name="inputImageWidth">The width of the input image.</param>
		/// <param name="inputImageHeight">The height of the input image.</param>
		/// <returns>A list of raw encoded mipmap data.</returns>
		public IList<byte[]> EncodeToRawBytes(byte[] inputPixelsRgba, int inputImageWidth, int inputImageHeight)
		{
			return EncodeToRawBytesInternal(inputPixelsRgba, inputImageWidth, inputImageHeight, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a list of byte buffers.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <returns>A list of raw encoded data.</returns>
		public IList<byte[]> EncodeToRawBytes(Image<Rgba32> inputImage)
		{
			return EncodeToRawBytesInternal(inputImage, default);
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
			return EncodeToRawBytesInternal(inputImage, mipLevel, out mipWidth, out mipHeight, default);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer.
		/// </summary>
		/// <param name="inputPixelsRgba">The raw pixels of the input image in rgba format.</param>
		/// <param name="inputImageWidth">The width of the input image.</param>
		/// <param name="inputImageHeight">The height of the input image.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="mipWidth">The width of the mipmap.</param>
		/// <param name="mipHeight">The height of the mipmap.</param>
		/// <returns>The raw encoded data.</returns>
		public byte[] EncodeToRawBytes(byte[] inputPixelsRgba, int inputImageWidth, int inputImageHeight, int mipLevel, out int mipWidth, out int mipHeight)
		{
			return EncodeToRawBytesInternal(inputPixelsRgba, inputImageWidth, inputImageHeight, mipLevel, out mipWidth,
				out mipHeight, default);
		}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into Ktx file and writes it to the output stream.
		/// Order is +X, -X, +Y, -Y, +Z, -Z
		/// </summary>
		/// <param name="right">The right face of the cubemap.</param>
		/// <param name="left">The left face of the cubemap.</param>
		/// <param name="top">The top face of the cubemap.</param>
		/// <param name="down">The bottom face of the cubemap.</param>
		/// <param name="back">The back face of the cubemap.</param>
		/// <param name="front">The front face of the cubemap.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public void EncodeCubeMap(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, Stream outputStream)
		{
			EncodeCubeMapInternal(right, left, top, down, back, front, outputStream, default);
		}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into a Ktx file.
		/// Order is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The right face of the cubemap.</param>
		/// <param name="left">The left face of the cubemap.</param>
		/// <param name="top">The top face of the cubemap.</param>
		/// <param name="down">The bottom face of the cubemap.</param>
		/// <param name="back">The back face of the cubemap.</param>
		/// <param name="front">The front face of the cubemap.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public KtxFile EncodeCubeMapToKtx(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front)
		{
			return EncodeCubeMapToKtxInternal(right, left, top, down, back, front, default);
		}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into a Dds file.
		/// Order is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The right face of the cubemap.</param>
		/// <param name="left">The left face of the cubemap.</param>
		/// <param name="top">The top face of the cubemap.</param>
		/// <param name="down">The bottom face of the cubemap.</param>
		/// <param name="back">The back face of the cubemap.</param>
		/// <param name="front">The front face of the cubemap.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public DdsFile EncodeCubeMapToDds(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front)
		{
			return EncodeCubeMapToDdsInternal(right, left, top, down, back, front, default);
		}

		#endregion

		#region MipMap operations

		/// <summary>
		/// Calculates the number of mipmap levels that will be generated for the given input image.
		/// </summary>
		/// <param name="inputImage">The image to use for the calculation.</param>
		/// <returns>The number of mipmap levels that will be generated for the input image.</returns>
		public int CalculateNumberOfMipLevels(Image<Rgba32> inputImage)
		{
			return (int)MipMapper.CalculateMipChainLength(inputImage.Width, inputImage.Height,
				(OutputOptions.GenerateMipMaps ? (uint)OutputOptions.MaxMipMapLevel : 1));
		}

		/// <summary>
		/// Calculates the number of mipmap levels that will be generated for the given input image.
		/// </summary>
		/// <param name="imagePixelWidth">The width of the input image in pixels</param>
		/// <param name="imagePixelHeight">The height of the input image in pixels</param>
		/// <returns>The number of mipmap levels that will be generated for the input image.</returns>
		public int CalculateNumberOfMipLevels(int imagePixelWidth, int imagePixelHeight)
		{
			return (int)MipMapper.CalculateMipChainLength(imagePixelWidth, imagePixelHeight,
				(OutputOptions.GenerateMipMaps ? (uint)OutputOptions.MaxMipMapLevel : 1));
		}

		/// <summary>
		/// Calculates the size of a given mipmap level.
		/// </summary>
		/// <param name="inputImage">The image to use for the calculation.</param>
		/// <param name="mipLevel">The mipLevel to calculate (0 is original image)</param>
		/// <param name="mipWidth">The mipmap width calculated</param>
		/// <param name="mipHeight">The mipmap height calculated</param>
		public void CalculateMipMapSize(Image<Rgba32> inputImage, int mipLevel, out int mipWidth, out int mipHeight)
		{
			MipMapper.CalculateMipLevelSize(inputImage.Width, inputImage.Height, mipLevel, out mipWidth,
				out mipHeight);
		}

		/// <summary>
		/// Calculates the size of a given mipmap level.
		/// </summary>
		/// <param name="imagePixelWidth">The width of the input image in pixels</param>
		/// <param name="imagePixelHeight">The height of the input image in pixels</param>
		/// <param name="mipLevel">The mipLevel to calculate (0 is original image)</param>
		/// <param name="mipWidth">The mipmap width calculated</param>
		/// <param name="mipHeight">The mipmap height calculated</param>
		public void CalculateMipMapSize(int imagePixelWidth, int imagePixelHeight, int mipLevel, out int mipWidth, out int mipHeight)
		{
			MipMapper.CalculateMipLevelSize(imagePixelWidth, imagePixelHeight, mipLevel, out mipWidth,
				out mipHeight);
		}

		#endregion

		#region Private

		private void EncodeInternal(Image<Rgba32> inputImage, Stream outputStream, CancellationToken token)
		{
			switch (OutputOptions.FileFormat)
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

		private KtxFile EncodeToKtxInternal(Image<Rgba32> inputImage, CancellationToken token)
		{
			KtxFile output;
			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? (uint)OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

			// Setup encoders
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}

				output = new KtxFile(
					KtxHeader.InitializeCompressed(inputImage.Width, inputImage.Height,
						compressedEncoder.GetInternalFormat(),
						compressedEncoder.GetBaseInternalFormat()));
			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.Format);
				output = new KtxFile(
					KtxHeader.InitializeUncompressed(inputImage.Width, inputImage.Height,
						uncompressedEncoder.GetGlType(),
						uncompressedEncoder.GetGlFormat(),
						uncompressedEncoder.GetGlTypeSize(),
						uncompressedEncoder.GetInternalFormat(),
						uncompressedEncoder.GetBaseInternalFormat()));
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Encode mipmap levels
			for (var mip = 0; mip < numMipMaps; mip++)
			{
				byte[] encoded;
				if (isCompressedFormat)
				{
					var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out var blocksWidth, out var blocksHeight);
					encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);
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

			output.header.NumberOfFaces = 1;
			output.header.NumberOfMipmapLevels = numMipMaps;

			return output;
		}

		private DdsFile EncodeToDdsInternal(Image<Rgba32> inputImage, CancellationToken token)
		{
			DdsFile output;
			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? (uint)OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}

				var (ddsHeader, dxt10Header) = DdsHeader.InitializeCompressed(inputImage.Width, inputImage.Height,
					compressedEncoder.GetDxgiFormat());
				output = new DdsFile(ddsHeader, dxt10Header);

				if (OutputOptions.DdsBc1WriteAlphaFlag &&
					OutputOptions.Format == CompressionFormat.Bc1WithAlpha)
				{
					output.header.ddsPixelFormat.dwFlags |= PixelFormatFlags.DdpfAlphapixels;
				}
			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.Format);
				var ddsHeader = DdsHeader.InitializeUncompressed(inputImage.Width, inputImage.Height,
					uncompressedEncoder.GetDxgiFormat());
				output = new DdsFile(ddsHeader);
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Encode mipmap levels
			for (var mip = 0; mip < numMipMaps; mip++)
			{
				byte[] encoded;
				if (isCompressedFormat)
				{
					var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out var blocksWidth, out var blocksHeight);
					encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);
				}
				else
				{
					if (!mipChain[mip].TryGetSinglePixelSpan(out var mipPixels))
					{
						throw new Exception("Cannot get pixel span.");
					}

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
			{
				image.Dispose();
			}

			output.header.dwMipMapCount = numMipMaps;
			if (numMipMaps > 1)
			{
				output.header.dwCaps |= HeaderCaps.DdscapsComplex | HeaderCaps.DdscapsMipmap;
			}

			return output;
		}

		private IList<byte[]> EncodeToRawBytesInternal(byte[] inputPixelsRgba, int inputImageWidth, int inputImageHeight, CancellationToken token)
		{
			if (inputPixelsRgba.Length != inputImageWidth * inputImageHeight * 4)
			{
				throw new ArgumentException("The input pixels must be provided in 4 bytes per pixel rgba format.");
			}

			using var image = Image.LoadPixelData<Rgba32>(inputPixelsRgba, inputImageWidth, inputImageHeight);
			return EncodeToRawBytesInternal(image, token);
		}

		private IList<byte[]> EncodeToRawBytesInternal(Image<Rgba32> inputImage, CancellationToken token)
		{
			var output = new List<byte[]>();
			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? (uint)OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}
			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.Format);
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Encode all mipmap levels
			for (var mip = 0; mip < numMipMaps; mip++)
			{
				byte[] encoded;
				if (isCompressedFormat)
				{
					var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out var blocksWidth, out var blocksHeight);
					encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);
				}
				else
				{
					if (!mipChain[mip].TryGetSinglePixelSpan(out var mipPixels))
					{
						throw new Exception("Cannot get pixel span.");
					}

					encoded = uncompressedEncoder.Encode(mipPixels);
				}

				output.Add(encoded);
			}

			// Dispose all mipmap levels
			foreach (var image in mipChain)
			{
				image.Dispose();
			}

			return output;
		}

		private byte[] EncodeToRawBytesInternal(byte[] inputPixelsRgba, int inputImageWidth, int inputImageHeight, int mipLevel, out int mipWidth, out int mipHeight, CancellationToken token)
		{
			if (inputPixelsRgba.Length != inputImageWidth * inputImageHeight * 4)
			{
				throw new ArgumentException("The input pixels must be provided in 4 bytes per pixel rgba format.");
			}
			using var image = Image.LoadPixelData<Rgba32>(inputPixelsRgba, inputImageWidth, inputImageHeight);
			return EncodeToRawBytesInternal(image, mipLevel, out mipWidth, out mipHeight, token);
		}

		private byte[] EncodeToRawBytesInternal(Image<Rgba32> inputImage, int mipLevel, out int mipWidth, out int mipHeight, CancellationToken token)
		{
			if (mipLevel < 0)
				throw new ArgumentException($"{nameof(mipLevel)} cannot be less than zero.");

			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? (uint)OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}
			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.Format);
			}

			// Dispose all mipmap levels
			if (mipLevel > numMipMaps - 1)
			{
				foreach (var image in mipChain)
				{
					image.Dispose();
				}

				throw new ArgumentException($"{nameof(mipLevel)} cannot be more than number of mipmaps.");
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Encode mipmap level
			byte[] encoded;
			if (isCompressedFormat)
			{
				var blocks = ImageToBlocks.ImageTo4X4(mipChain[mipLevel].Frames[0], out var blocksWidth, out var blocksHeight);
				encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);
			}
			else
			{
				if (!mipChain[mipLevel].TryGetSinglePixelSpan(out var mipPixels))
				{
					throw new Exception("Cannot get pixel span.");
				}

				encoded = uncompressedEncoder.Encode(mipPixels);
			}

			mipWidth = mipChain[mipLevel].Width;
			mipHeight = mipChain[mipLevel].Height;

			// Dispose all mipmap levels
			foreach (var image in mipChain)
			{
				image.Dispose();
			}

			return encoded;
		}

		private void EncodeCubeMapInternal(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, Stream outputStream, CancellationToken token)
		{
			switch (OutputOptions.FileFormat)
			{
				case OutputFileFormat.Ktx:
					var ktx = EncodeCubeMapToKtxInternal(right, left, top, down, back, front, token);
					ktx.Write(outputStream);
					break;

				case OutputFileFormat.Dds:
					var dds = EncodeCubeMapToDdsInternal(right, left, top, down, back, front, token);
					dds.Write(outputStream);
					break;
			}
		}

		private KtxFile EncodeCubeMapToKtxInternal(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, CancellationToken token)
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
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}

				output = new KtxFile(
					KtxHeader.InitializeCompressed(right.Width, right.Height,
						compressedEncoder.GetInternalFormat(),
						compressedEncoder.GetBaseInternalFormat()));
			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.Format);
				output = new KtxFile(
					KtxHeader.InitializeUncompressed(right.Width, right.Height,
						uncompressedEncoder.GetGlType(),
						uncompressedEncoder.GetGlFormat(),
						uncompressedEncoder.GetGlTypeSize(),
						uncompressedEncoder.GetInternalFormat(),
						uncompressedEncoder.GetBaseInternalFormat()));

			}

			var numMipMaps = OutputOptions.GenerateMipMaps ? (uint)OutputOptions.MaxMipMapLevel : 1;
			var mipLength = MipMapper.CalculateMipChainLength(right.Width, right.Height, numMipMaps);
			for (uint i = 0; i < mipLength; i++)
			{
				output.MipMaps.Add(new KtxMipmap(0, 0, 0, (uint)faces.Length));
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

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
						var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out var blocksWidth, out var blocksHeight);
						encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);
					}
					else
					{
						if (!mipChain[mip].TryGetSinglePixelSpan(out var mipPixels))
						{
							throw new Exception("Cannot get pixel span.");
						}

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
				{
					image.Dispose();
				}
			}

			output.header.NumberOfFaces = (uint)faces.Length;
			output.header.NumberOfMipmapLevels = mipLength;

			return output;
		}

		private DdsFile EncodeCubeMapToDdsInternal(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, CancellationToken token)
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
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}

				var (ddsHeader, dxt10Header) = DdsHeader.InitializeCompressed(right.Width, right.Height,
					compressedEncoder.GetDxgiFormat());
				output = new DdsFile(ddsHeader, dxt10Header);

				if (OutputOptions.DdsBc1WriteAlphaFlag &&
					OutputOptions.Format == CompressionFormat.Bc1WithAlpha)
				{
					output.header.ddsPixelFormat.dwFlags |= PixelFormatFlags.DdpfAlphapixels;
				}
			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.Format);
				var ddsHeader = DdsHeader.InitializeUncompressed(right.Width, right.Height,
					uncompressedEncoder.GetDxgiFormat());

				output = new DdsFile(ddsHeader);
			}

			var numMipMaps = OutputOptions.GenerateMipMaps ? (uint)OutputOptions.MaxMipMapLevel : 1;

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

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
						var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out var blocksWidth, out var blocksHeight);
						encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);
					}
					else
					{
						if (!mipChain[mip].TryGetSinglePixelSpan(out var mipPixels))
						{
							throw new Exception("Cannot get pixel span.");
						}

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
				{
					image.Dispose();
				}
			}

			output.header.dwCaps |= HeaderCaps.DdscapsComplex;
			output.header.dwMipMapCount = numMipMaps;
			if (numMipMaps > 1)
			{
				output.header.dwCaps |= HeaderCaps.DdscapsMipmap;
			}

			output.header.dwCaps2 |= HeaderCaps2.Ddscaps2Cubemap |
							  HeaderCaps2.Ddscaps2CubemapPositivex |
							  HeaderCaps2.Ddscaps2CubemapNegativex |
							  HeaderCaps2.Ddscaps2CubemapPositivey |
							  HeaderCaps2.Ddscaps2CubemapNegativey |
							  HeaderCaps2.Ddscaps2CubemapPositivez |
							  HeaderCaps2.Ddscaps2CubemapNegativez;

			return output;
		}

		#endregion

		#region Support

		private IBcBlockEncoder GetEncoder(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.Bc1:
					return new Bc1BlockEncoder();

				case CompressionFormat.Bc1WithAlpha:
					return new Bc1AlphaBlockEncoder();

				case CompressionFormat.Bc2:
					return new Bc2BlockEncoder();

				case CompressionFormat.Bc3:
					return new Bc3BlockEncoder();

				case CompressionFormat.Bc4:
					return new Bc4BlockEncoder(InputOptions.LuminanceAsRed);

				case CompressionFormat.Bc5:
					return new Bc5BlockEncoder();

				case CompressionFormat.Bc7:
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
					return new RawLuminanceEncoder(InputOptions.LuminanceAsRed);

				case CompressionFormat.Rg:
					return new RawRgEncoder();

				case CompressionFormat.Rgb:
					return new RawRgbEncoder();

				case CompressionFormat.Rgba:
					return new RawRgbaEncoder();

				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		#endregion
	}
}
