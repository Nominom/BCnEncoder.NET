using BCnEncoder.Decoder.Options;
using BCnEncoder.Shared;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Shared.ImageFiles;
using CommunityToolkit.HighPerformance;

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
		/// The options for the decoder.
		/// </summary>
		public DecoderOptions Options { get; } = new DecoderOptions();

		/// <summary>
		/// The output options of the decoder.
		/// </summary>
		public DecoderOutputOptions OutputOptions { get; } = new DecoderOutputOptions();
		#region LDR
		#region Async Api

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method will read the expected amount of bytes from the given input stream and decode it.
		/// Make sure there is no file header information left in the stream before the encoded data.
		/// </summary>
		/// <param name="inputStream">The stream containing the encoded data.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<ColorRgba32[]> DecodeRawAsync(Stream inputStream, CompressionFormat format, int pixelWidth, int pixelHeight, CancellationToken token = default)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			inputStream.Read(dataArray, 0, dataArray.Length);

			return Task.Run(() => DecodeRawInternal(dataArray, pixelWidth, pixelHeight, format, token), token);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// </summary>
		/// <param name="input">The <see cref="Memory{T}"/> containing the encoded data.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<ColorRgba32[]> DecodeRawAsync(ReadOnlyMemory<byte> input, CompressionFormat format, int pixelWidth, int pixelHeight, CancellationToken token = default)
		{
			return Task.Run(() => DecodeRawInternal(input, pixelWidth, pixelHeight, format, token), token);
		}

		/// <summary>
		/// Decode the main image from a Ktx file.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<ColorRgba32[]> DecodeAsync(KtxFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(file, false, token)[0], token);
		}

		/// <summary>
		/// Decode all available mipmaps from a Ktx file.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<ColorRgba32[][]> DecodeAllMipMapsAsync(KtxFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(file, true, token), token);
		}

		/// <summary>
		/// Decode the main image from a Dds file.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<ColorRgba32[]> DecodeAsync(DdsFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(file, false, token)[0], token);
		}

		/// <summary>
		/// Decode all available mipmaps from a Dds file.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<ColorRgba32[][]> DecodeAllMipMapsAsync(DdsFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(file, true, token), token);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method will read the expected amount of bytes from the given input stream and decode it.
		/// Make sure there is no file header information left in the stream before the encoded data.
		/// </summary>
		/// <param name="inputStream">The stream containing the raw encoded data.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgba32>> DecodeRaw2DAsync(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			inputStream.Read(dataArray, 0, dataArray.Length);

			return Task.Run(() => DecodeRawInternal(dataArray, pixelWidth, pixelHeight, format, token)
				.AsMemory().AsMemory2D(pixelHeight, pixelWidth), token);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// </summary>
		/// <param name="input">The <see cref="Memory{T}"/> containing the encoded data.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgba32>> DecodeRaw2DAsync(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			return Task.Run(() => DecodeRawInternal(input, pixelWidth, pixelHeight, format, token)
				.AsMemory().AsMemory2D(pixelHeight, pixelWidth), token);
		}

		/// <summary>
		/// Read a Ktx or Dds file from a stream and decode the main image from it.
		/// The type of file will be detected automatically.
		/// </summary>
		/// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgba32>> Decode2DAsync(Stream inputStream, CancellationToken token = default)
		{
			return Task.Run(() => DecodeFromStreamInternal2D(inputStream, false, token)[0], token);
		}

		/// <summary>
		/// Read a Ktx or Dds file from a stream and decode all available mipmaps from it.
		/// The type of file will be detected automatically.
		/// </summary>
		/// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgba32>[]> DecodeAllMipMaps2DAsync(Stream inputStream, CancellationToken token = default)
		{
			return Task.Run(() => DecodeFromStreamInternal2D(inputStream, false, token), token);
		}

		/// <summary>
		/// Decode the main image from a Ktx file.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgba32>> Decode2DAsync(KtxFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(file, false, token)[0]
				.AsMemory().AsMemory2D((int)file.header.PixelHeight, (int)file.header.PixelWidth), token);
		}

		/// <summary>
		/// Decode all available mipmaps from a Ktx file.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgba32>[]> DecodeAllMipMaps2DAsync(KtxFile file, CancellationToken token = default)
		{
			return Task.Run(() =>
			{
				var decoded = DecodeInternal(file, true, token);
				var mem2Ds = new Memory2D<ColorRgba32>[decoded.Length];
				for (var i = 0; i < decoded.Length; i++)
				{
					var mip = file.MipMaps[i];
					mem2Ds[i] = decoded[i].AsMemory().AsMemory2D((int)mip.Height, (int)mip.Width);
				}
				return mem2Ds;
			}, token);
		}

		/// <summary>
		/// Decode the main image from a Dds file.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgba32>> Decode2DAsync(DdsFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(file, false, token)[0]
				.AsMemory().AsMemory2D((int)file.header.dwHeight, (int)file.header.dwWidth), token);
		}

		/// <summary>
		/// Decode all available mipmaps from a Dds file.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgba32>[]> DecodeAllMipMaps2DAsync(DdsFile file, CancellationToken token = default)
		{
			return Task.Run(() =>
			{
				var decoded = DecodeInternal(file, true, token);
				var mem2Ds = new Memory2D<ColorRgba32>[decoded.Length];
				for (var i = 0; i < decoded.Length; i++)
				{
					var mip = file.Faces[0].MipMaps[i];
					mem2Ds[i] = decoded[i].AsMemory().AsMemory2D((int)mip.Height, (int)mip.Width);
				}
				return mem2Ds;
			}, token);
		}

		#endregion

		#region Sync API

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method will read the expected amount of bytes from the given input stream and decode it.
		/// Make sure there is no file header information left in the stream before the encoded data.
		/// </summary>
		/// <param name="inputStream">The stream containing the raw encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded image.</returns>
		public ColorRgba32[] DecodeRaw(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			inputStream.Read(dataArray, 0, dataArray.Length);

			return DecodeRaw(dataArray, pixelWidth, pixelHeight, format);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// </summary>
		/// <param name="input">The byte array containing the raw encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded image.</returns>
		public ColorRgba32[] DecodeRaw(byte[] input, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			return DecodeRawInternal(input, pixelWidth, pixelHeight, format, default);
		}

		/// <summary>
		/// Decode the main image from a Ktx file.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>The decoded image.</returns>
		public ColorRgba32[] Decode(KtxFile file)
		{
			return DecodeInternal(file, false, default)[0];
		}

		/// <summary>
		/// Decode all available mipmaps from a Ktx file.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>An array of decoded images.</returns>
		public ColorRgba32[][] DecodeAllMipMaps(KtxFile file)
		{
			return DecodeInternal(file, true, default);
		}

		/// <summary>
		/// Decode the main image from a Dds file.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>The decoded image.</returns>
		public ColorRgba32[] Decode(DdsFile file)
		{
			return DecodeInternal(file, false, default)[0];
		}

		/// <summary>
		/// Decode all available mipmaps from a Dds file.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>An array of decoded images.</returns>
		public ColorRgba32[][] DecodeAllMipMaps(DdsFile file)
		{
			return DecodeInternal(file, true, default);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method will read the expected amount of bytes from the given input stream and decode it.
		/// Make sure there is no file header information left in the stream before the encoded data.
		/// </summary>
		/// <param name="inputStream">The stream containing the encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded image.</returns>
		public Memory2D<ColorRgba32> DecodeRaw2D(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			inputStream.Read(dataArray, 0, dataArray.Length);

			var decoded = DecodeRaw(dataArray, pixelWidth, pixelHeight, format);
			return decoded.AsMemory().AsMemory2D(pixelHeight, pixelWidth);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// </summary>
		/// <param name="input">The byte array containing the raw encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded image.</returns>
		public Memory2D<ColorRgba32> DecodeRaw2D(byte[] input, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var decoded = DecodeRawInternal(input, pixelWidth, pixelHeight, format, default);
			return decoded.AsMemory().AsMemory2D(pixelHeight, pixelWidth);
		}

		/// <summary>
		/// Read a Ktx or Dds file from a stream and decode the main image from it.
		/// The type of file will be detected automatically.
		/// </summary>
		/// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
		/// <returns>The decoded image.</returns>
		public Memory2D<ColorRgba32> Decode2D(Stream inputStream)
		{
			return DecodeFromStreamInternal2D(inputStream, false, default)[0];
		}

		/// <summary>
		/// Read a Ktx or Dds file from a stream and decode all available mipmaps from it.
		/// The type of file will be detected automatically.
		/// </summary>
		/// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
		/// <returns>An array of decoded images.</returns>
		public Memory2D<ColorRgba32>[] DecodeAllMipMaps2D(Stream inputStream)
		{
			return DecodeFromStreamInternal2D(inputStream, true, default);
		}

		/// <summary>
		/// Decode the main image from a Ktx file.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>The decoded image.</returns>
		public Memory2D<ColorRgba32> Decode2D(KtxFile file)
		{
			return DecodeInternal(file, false, default)[0].AsMemory().AsMemory2D((int)file.header.PixelHeight, (int)file.header.PixelWidth);
		}

		/// <summary>
		/// Decode all available mipmaps from a Ktx file.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>An array of decoded images.</returns>
		public Memory2D<ColorRgba32>[] DecodeAllMipMaps2D(KtxFile file)
		{
			var decoded = DecodeInternal(file, true, default);
			var mem2Ds = new Memory2D<ColorRgba32>[decoded.Length];
			for (var i = 0; i < decoded.Length; i++)
			{
				var mip = file.MipMaps[i];
				mem2Ds[i] = decoded[i].AsMemory().AsMemory2D((int)mip.Height, (int)mip.Width);
			}
			return mem2Ds;
		}

		/// <summary>
		/// Decode the main image from a Dds file.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>The decoded image.</returns>
		public Memory2D<ColorRgba32> Decode2D(DdsFile file)
		{
			return DecodeInternal(file, false, default)[0].AsMemory().AsMemory2D((int)file.header.dwHeight, (int)file.header.dwWidth);
		}

		/// <summary>
		/// Decode all available mipmaps from a Dds file.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>An array of decoded images.</returns>
		public Memory2D<ColorRgba32>[] DecodeAllMipMaps2D(DdsFile file)
		{
			var decoded = DecodeInternal(file, true, default);
			var mem2Ds = new Memory2D<ColorRgba32>[decoded.Length];
			for (var i = 0; i < decoded.Length; i++)
			{
				var mip = file.Faces[0].MipMaps[i];
				mem2Ds[i] = decoded[i].AsMemory().AsMemory2D((int)mip.Height, (int)mip.Width);
			}
			return mem2Ds;
		}

		/// <summary>
		/// Decode a single block from raw bytes and return it as a <see cref="Memory2D{T}"/>.
		/// Input Span size needs to equal the block size.
		/// To get the block size (in bytes) of the compression format used, see <see cref="GetBlockSize(BCnEncoder.Shared.CompressionFormat)"/>.
		/// </summary>
		/// <param name="blockData">The encoded block in bytes.</param>
		/// <param name="format">The compression format used.</param>
		/// <returns>The decoded 4x4 block.</returns>
		public Memory2D<ColorRgba32> DecodeBlock(ReadOnlySpan<byte> blockData, CompressionFormat format)
		{
			var output = new ColorRgba32[4, 4];
			DecodeBlockInternal(blockData, format, output);
			return output;
		}

		/// <summary>
		/// Decode a single block from raw bytes and write it to the given output span.
		/// Output span size must be exactly 4x4 and input Span size needs to equal the block size.
		/// To get the block size (in bytes) of the compression format used, see <see cref="GetBlockSize(BCnEncoder.Shared.CompressionFormat)"/>.
		/// </summary>
		/// <param name="blockData">The encoded block in bytes.</param>
		/// <param name="format">The compression format used.</param>
		/// <param name="outputSpan">The destination span of the decoded data.</param>
		public void DecodeBlock(ReadOnlySpan<byte> blockData, CompressionFormat format, Span2D<ColorRgba32> outputSpan)
		{
			if (outputSpan.Width != 4 || outputSpan.Height != 4)
			{
				throw new ArgumentException($"Single block decoding needs an output span of exactly 4x4");
			}
			DecodeBlockInternal(blockData, format, outputSpan);
		}

		/// <summary>
		/// Decode a single block from a stream and write it to the given output span.
		/// Output span size must be exactly 4x4.
		/// </summary>
		/// <param name="inputStream">The stream to read encoded blocks from.</param>
		/// <param name="format">The compression format used.</param>
		/// <param name="outputSpan">The destination span of the decoded data.</param>
		/// <returns>The number of bytes read from the stream. Zero (0) if reached the end of stream.</returns>
		public int DecodeBlock(Stream inputStream, CompressionFormat format, Span2D<ColorRgba32> outputSpan)
		{
			if (outputSpan.Width != 4 || outputSpan.Height != 4)
			{
				throw new ArgumentException($"Single block decoding needs an output span of exactly 4x4");
			}

			Span<byte> input = stackalloc byte[16];
			input = input.Slice(0, GetBlockSize(format));

			var bytesRead = inputStream.Read(input);

			if (bytesRead == 0)
			{
				return 0; //End of stream
			}

			if (bytesRead != input.Length)
			{
				throw new Exception("Input stream does not have enough data available for a full block.");
			}

			DecodeBlockInternal(input, format, outputSpan);
			return bytesRead;
		}

		/// <summary>
		/// Check whether a file is encoded in a supported format.
		/// </summary>
		/// <param name="file">The loaded ktx file to check</param>
		/// <returns>If the format of the file is one of the supported formats.</returns>
		public bool IsSupportedFormat(KtxFile file)
		{
			return GetCompressionFormat(file.header.GlInternalFormat) != CompressionFormat.Unknown;
		}

		/// <summary>
		/// Check whether a file is encoded in a supported format.
		/// </summary>
		/// <param name="file">The loaded dds file to check</param>
		/// <returns>If the format of the file is one of the supported formats.</returns>
		public bool IsSupportedFormat(DdsFile file)
		{
			return GetCompressionFormat(file) != CompressionFormat.Unknown;
		}

		/// <summary>
		/// Gets the format of the file.
		/// </summary>
		/// <param name="file">The loaded ktx file to check</param>
		/// <returns>The <see cref="CompressionFormat"/> of the file.</returns>
		public CompressionFormat GetFormat(KtxFile file)
		{
			return GetCompressionFormat(file.header.GlInternalFormat);
		}

		/// <summary>
		/// Gets the format of the file.
		/// </summary>
		/// <param name="file">The loaded dds file to check</param>
		/// <returns>The <see cref="CompressionFormat"/> of the file.</returns>
		public CompressionFormat GetFormat(DdsFile file)
		{
			return GetCompressionFormat(file);
		}


		#endregion
		#endregion

		#region HDR
		#region Async Api

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method will read the expected amount of bytes from the given input stream and decode it.
		/// Make sure there is no file header information left in the stream before the encoded data.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="inputStream">The stream containing the encoded data.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<ColorRgbFloat[]> DecodeRawHdrAsync(Stream inputStream, CompressionFormat format, int pixelWidth, int pixelHeight, CancellationToken token = default)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			inputStream.Read(dataArray, 0, dataArray.Length);

			return Task.Run(() => DecodeRawInternalHdr(dataArray, pixelWidth, pixelHeight, format, token), token);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="input">The <see cref="Memory{T}"/> containing the encoded data.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<ColorRgbFloat[]> DecodeRawHdrAsync(ReadOnlyMemory<byte> input, CompressionFormat format, int pixelWidth, int pixelHeight, CancellationToken token = default)
		{
			return Task.Run(() => DecodeRawInternalHdr(input, pixelWidth, pixelHeight, format, token), token);
		}

		/// <summary>
		/// Decode the main image from a Ktx file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<ColorRgbFloat[]> DecodeHdrAsync(KtxFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternalHdr(file, false, token)[0], token);
		}

		/// <summary>
		/// Decode all available mipmaps from a Ktx file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<ColorRgbFloat[][]> DecodeAllMipMapsHdrAsync(KtxFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternalHdr(file, true, token), token);
		}

		/// <summary>
		/// Decode the main image from a Dds file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<ColorRgbFloat[]> DecodeHdrAsync(DdsFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternalHdr(file, false, token)[0], token);
		}

		/// <summary>
		/// Decode all available mipmaps from a Dds file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<ColorRgbFloat[][]> DecodeAllMipMapsHdrAsync(DdsFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternalHdr(file, true, token), token);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method will read the expected amount of bytes from the given input stream and decode it.
		/// Make sure there is no file header information left in the stream before the encoded data.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="inputStream">The stream containing the raw encoded data.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgbFloat>> DecodeRawHdr2DAsync(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			inputStream.Read(dataArray, 0, dataArray.Length);

			return Task.Run(() => DecodeRawInternalHdr(dataArray, pixelWidth, pixelHeight, format, token)
				.AsMemory().AsMemory2D(pixelHeight, pixelWidth), token);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="input">The <see cref="Memory{T}"/> containing the encoded data.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgbFloat>> DecodeRawHdr2DAsync(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			return Task.Run(() => DecodeRawInternalHdr(input, pixelWidth, pixelHeight, format, token)
				.AsMemory().AsMemory2D(pixelHeight, pixelWidth), token);
		}

		/// <summary>
		/// Read a Ktx or Dds file from a stream and decode the main image from it.
		/// The type of file will be detected automatically.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgbFloat>> DecodeHdr2DAsync(Stream inputStream, CancellationToken token = default)
		{
			return Task.Run(() => DecodeFromStreamInternalHdr2D(inputStream, false, token)[0], token);
		}

		/// <summary>
		/// Read a Ktx or Dds file from a stream and decode all available mipmaps from it.
		/// The type of file will be detected automatically.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgbFloat>[]> DecodeAllMipMapsHdr2DAsync(Stream inputStream, CancellationToken token = default)
		{
			return Task.Run(() => DecodeFromStreamInternalHdr2D(inputStream, false, token), token);
		}

		/// <summary>
		/// Decode the main image from a Ktx file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgbFloat>> DecodeHdr2DAsync(KtxFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternalHdr(file, false, token)[0]
				.AsMemory().AsMemory2D((int)file.header.PixelHeight, (int)file.header.PixelWidth), token);
		}

		/// <summary>
		/// Decode all available mipmaps from a Ktx file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgbFloat>[]> DecodeAllMipMapsHdr2DAsync(KtxFile file, CancellationToken token = default)
		{
			return Task.Run(() =>
			{
				var decoded = DecodeInternalHdr(file, true, token);
				var mem2Ds = new Memory2D<ColorRgbFloat>[decoded.Length];
				for (var i = 0; i < decoded.Length; i++)
				{
					var mip = file.MipMaps[i];
					mem2Ds[i] = decoded[i].AsMemory().AsMemory2D((int)mip.Height, (int)mip.Width);
				}
				return mem2Ds;
			}, token);
		}

		/// <summary>
		/// Decode the main image from a Dds file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgbFloat>> DecodeHdr2DAsync(DdsFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternalHdr(file, false, token)[0]
				.AsMemory().AsMemory2D((int)file.header.dwHeight, (int)file.header.dwWidth), token);
		}

		/// <summary>
		/// Decode all available mipmaps from a Dds file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded image.</returns>
		public Task<Memory2D<ColorRgbFloat>[]> DecodeAllMipMapsHdr2DAsync(DdsFile file, CancellationToken token = default)
		{
			return Task.Run(() =>
			{
				var decoded = DecodeInternalHdr(file, true, token);
				var mem2Ds = new Memory2D<ColorRgbFloat>[decoded.Length];
				for (var i = 0; i < decoded.Length; i++)
				{
					var mip = file.Faces[0].MipMaps[i];
					mem2Ds[i] = decoded[i].AsMemory().AsMemory2D((int)mip.Height, (int)mip.Width);
				}
				return mem2Ds;
			}, token);
		}

		#endregion

		#region Sync API

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method will read the expected amount of bytes from the given input stream and decode it.
		/// Make sure there is no file header information left in the stream before the encoded data.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="inputStream">The stream containing the raw encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded image.</returns>
		public ColorRgbFloat[] DecodeRawHdr(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			inputStream.Read(dataArray, 0, dataArray.Length);

			return DecodeRawHdr(dataArray, pixelWidth, pixelHeight, format);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="input">The byte array containing the raw encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded image.</returns>
		public ColorRgbFloat[] DecodeRawHdr(byte[] input, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			return DecodeRawInternalHdr(input, pixelWidth, pixelHeight, format, default);
		}

		/// <summary>
		/// Decode the main image from a Ktx file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>The decoded image.</returns>
		public ColorRgbFloat[] DecodeHdr(KtxFile file)
		{
			return DecodeInternalHdr(file, false, default)[0];
		}

		/// <summary>
		/// Decode all available mipmaps from a Ktx file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>An array of decoded images.</returns>
		public ColorRgbFloat[][] DecodeAllMipMapsHdr(KtxFile file)
		{
			return DecodeInternalHdr(file, true, default);
		}

		/// <summary>
		/// Decode the main image from a Dds file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>The decoded image.</returns>
		public ColorRgbFloat[] DecodeHdr(DdsFile file)
		{
			return DecodeInternalHdr(file, false, default)[0];
		}

		/// <summary>
		/// Decode all available mipmaps from a Dds file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>An array of decoded images.</returns>
		public ColorRgbFloat[][] DecodeAllMipMapsHdr(DdsFile file)
		{
			return DecodeInternalHdr(file, true, default);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method will read the expected amount of bytes from the given input stream and decode it.
		/// Make sure there is no file header information left in the stream before the encoded data.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="inputStream">The stream containing the encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded image.</returns>
		public Memory2D<ColorRgbFloat> DecodeRawHdr2D(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			inputStream.Read(dataArray, 0, dataArray.Length);

			var decoded = DecodeRawHdr(dataArray, pixelWidth, pixelHeight, format);
			return decoded.AsMemory().AsMemory2D(pixelHeight, pixelWidth);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="input">The byte array containing the raw encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded image.</returns>
		public Memory2D<ColorRgbFloat> DecodeRawHdr2D(byte[] input, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var decoded = DecodeRawInternalHdr(input, pixelWidth, pixelHeight, format, default);
			return decoded.AsMemory().AsMemory2D(pixelHeight, pixelWidth);
		}

		/// <summary>
		/// Read a Ktx or Dds file from a stream and decode the main image from it.
		/// The type of file will be detected automatically.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
		/// <returns>The decoded image.</returns>
		public Memory2D<ColorRgbFloat> DecodeHdr2D(Stream inputStream)
		{
			return DecodeFromStreamInternalHdr2D(inputStream, false, default)[0];
		}

		/// <summary>
		/// Read a Ktx or Dds file from a stream and decode all available mipmaps from it.
		/// The type of file will be detected automatically.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
		/// <returns>An array of decoded images.</returns>
		public Memory2D<ColorRgbFloat>[] DecodeAllMipMapsHdr2D(Stream inputStream)
		{
			return DecodeFromStreamInternalHdr2D(inputStream, true, default);
		}

		/// <summary>
		/// Decode the main image from a Ktx file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>The decoded image.</returns>
		public Memory2D<ColorRgbFloat> DecodeHdr2D(KtxFile file)
		{
			return DecodeInternalHdr(file, false, default)[0].AsMemory().AsMemory2D((int)file.header.PixelHeight, (int)file.header.PixelWidth);
		}

		/// <summary>
		/// Decode all available mipmaps from a Ktx file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>An array of decoded images.</returns>
		public Memory2D<ColorRgbFloat>[] DecodeAllMipMapsHdr2D(KtxFile file)
		{
			var decoded = DecodeInternalHdr(file, true, default);
			var mem2Ds = new Memory2D<ColorRgbFloat>[decoded.Length];
			for (var i = 0; i < decoded.Length; i++)
			{
				var mip = file.MipMaps[i];
				mem2Ds[i] = decoded[i].AsMemory().AsMemory2D((int)mip.Height, (int)mip.Width);
			}
			return mem2Ds;
		}

		/// <summary>
		/// Decode the main image from a Dds file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>The decoded image.</returns>
		public Memory2D<ColorRgbFloat> DecodeHdr2D(DdsFile file)
		{
			return DecodeInternalHdr(file, false, default)[0].AsMemory().AsMemory2D((int)file.header.dwHeight, (int)file.header.dwWidth);
		}

		/// <summary>
		/// Decode all available mipmaps from a Dds file.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>An array of decoded images.</returns>
		public Memory2D<ColorRgbFloat>[] DecodeAllMipMapsHdr2D(DdsFile file)
		{
			var decoded = DecodeInternalHdr(file, true, default);
			var mem2Ds = new Memory2D<ColorRgbFloat>[decoded.Length];
			for (var i = 0; i < decoded.Length; i++)
			{
				var mip = file.Faces[0].MipMaps[i];
				mem2Ds[i] = decoded[i].AsMemory().AsMemory2D((int)mip.Height, (int)mip.Width);
			}
			return mem2Ds;
		}

		/// <summary>
		/// Decode a single block from raw bytes and return it as a <see cref="Memory2D{T}"/>.
		/// Input Span size needs to equal the block size.
		/// To get the block size (in bytes) of the compression format used, see <see cref="GetBlockSize(BCnEncoder.Shared.CompressionFormat)"/>.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="blockData">The encoded block in bytes.</param>
		/// <param name="format">The compression format used.</param>
		/// <returns>The decoded 4x4 block.</returns>
		public Memory2D<ColorRgbFloat> DecodeBlockHdr(ReadOnlySpan<byte> blockData, CompressionFormat format)
		{
			var output = new ColorRgbFloat[4, 4];
			DecodeBlockInternalHdr(blockData, format, output);
			return output;
		}

		/// <summary>
		/// Decode a single block from raw bytes and write it to the given output span.
		/// Output span size must be exactly 4x4 and input Span size needs to equal the block size.
		/// To get the block size (in bytes) of the compression format used, see <see cref="GetBlockSize(BCnEncoder.Shared.CompressionFormat)"/>.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="blockData">The encoded block in bytes.</param>
		/// <param name="format">The compression format used.</param>
		/// <param name="outputSpan">The destination span of the decoded data.</param>
		public void DecodeBlockHdr(ReadOnlySpan<byte> blockData, CompressionFormat format, Span2D<ColorRgbFloat> outputSpan)
		{
			if (outputSpan.Width != 4 || outputSpan.Height != 4)
			{
				throw new ArgumentException($"Single block decoding needs an output span of exactly 4x4");
			}
			DecodeBlockInternalHdr(blockData, format, outputSpan);
		}

		/// <summary>
		/// Decode a single block from a stream and write it to the given output span.
		/// Output span size must be exactly 4x4.
		/// This method is only for compressed Hdr formats. Please use the non-Hdr methods for other formats.
		/// </summary>
		/// <param name="inputStream">The stream to read encoded blocks from.</param>
		/// <param name="format">The compression format used.</param>
		/// <param name="outputSpan">The destination span of the decoded data.</param>
		/// <returns>The number of bytes read from the stream. Zero (0) if reached the end of stream.</returns>
		public int DecodeBlockHdr(Stream inputStream, CompressionFormat format, Span2D<ColorRgbFloat> outputSpan)
		{
			if (outputSpan.Width != 4 || outputSpan.Height != 4)
			{
				throw new ArgumentException($"Single block decoding needs an output span of exactly 4x4");
			}

			Span<byte> input = stackalloc byte[16];
			input = input.Slice(0, GetBlockSize(format));

			var bytesRead = inputStream.Read(input);

			if (bytesRead == 0)
			{
				return 0; //End of stream
			}

			if (bytesRead != input.Length)
			{
				throw new Exception("Input stream does not have enough data available for a full block.");
			}

			DecodeBlockInternalHdr(input, format, outputSpan);
			return bytesRead;
		}

		/// <summary>
		/// Check whether a file is encoded in a supported HDR format.
		/// </summary>
		/// <param name="file">The loaded ktx file to check</param>
		/// <returns>If the format of the file is one of the supported HDR formats.</returns>
		public bool IsHdrFormat(KtxFile file)
		{
			return GetCompressionFormat(file.header.GlInternalFormat).IsHdrFormat();
		}

		/// <summary>
		/// Check whether a file is encoded in a supported HDR format.
		/// </summary>
		/// <param name="file">The loaded dds file to check</param>
		/// <returns>If the format of the file is one of the supported HDR formats.</returns>
		public bool IsHdrFormat(DdsFile file)
		{
			return GetCompressionFormat(file).IsHdrFormat();
		}

		#endregion
		#endregion
		/// <summary>
		/// Load a stream and extract either the main image or all mip maps.
		/// </summary>
		/// <param name="stream">The stream containing the image file.</param>
		/// <param name="allMipMaps">If all mip maps or only the main image should be decoded.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		private Memory2D<ColorRgba32>[] DecodeFromStreamInternal2D(Stream stream, bool allMipMaps, CancellationToken token)
		{
			var format = ImageFile.DetermineImageFormat(stream);

			switch (format)
			{
				case ImageFileFormat.Dds:
					{
						var file = DdsFile.Load(stream);
						var decoded = DecodeInternal(file, allMipMaps, token);
						var mem2Ds = new Memory2D<ColorRgba32>[decoded.Length];
						for (var i = 0; i < decoded.Length; i++)
						{
							var mip = file.Faces[0].MipMaps[i];
							mem2Ds[i] = decoded[i].AsMemory().AsMemory2D((int)mip.Height, (int)mip.Width);
						}

						return mem2Ds;
					}

				case ImageFileFormat.Ktx:
					{
						var file = KtxFile.Load(stream);
						var decoded = DecodeInternal(file, allMipMaps, token);
						var mem2Ds = new Memory2D<ColorRgba32>[decoded.Length];
						for (var i = 0; i < decoded.Length; i++)
						{
							var mip = file.MipMaps[i];
							mem2Ds[i] = decoded[i].AsMemory().AsMemory2D((int)mip.Height, (int)mip.Width);
						}

						return mem2Ds;
					}

				default:
					throw new InvalidOperationException("Unknown image format.");
			}
		}

		/// <summary>
		/// Load a KTX file and extract either the main image or all mip maps.
		/// </summary>
		/// <param name="file">The Ktx file to decode.</param>
		/// <param name="allMipMaps">If all mip maps or only the main image should be decoded.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		private ColorRgba32[][] DecodeInternal(KtxFile file, bool allMipMaps, CancellationToken token)
		{
			var mipMaps = allMipMaps ? file.MipMaps.Count : 1;
			var colors = new ColorRgba32[mipMaps][];

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var blockSize = GetBlockSize(file.header.GlInternalFormat);
			var totalBlocks = file.MipMaps.Take(mipMaps).Sum(m => m.Faces[0].Data.Length / blockSize);

			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			if (IsSupportedRawFormat(file.header.GlInternalFormat))
			{
				var decoder = GetRawDecoder(file.header.GlInternalFormat);

				for (var mip = 0; mip < mipMaps; mip++)
				{
					var data = file.MipMaps[mip].Faces[0].Data;

					colors[mip] = decoder.Decode(data, context);

					context.Progress.SetProcessedBlocks(file.MipMaps.Take(mip + 1).Sum(x => x.Faces[0].Data.Length / blockSize));
				}
			}
			else
			{
				var decoder = GetRgba32Decoder(file.header.GlInternalFormat);
				var format = GetCompressionFormat(file.header.GlInternalFormat);
				if (format.IsHdrFormat())
				{
					throw new NotSupportedException($"This Format is not an RGBA32 compatible format: {format}, please use the HDR versions of the decode methods.");
				}
				if (decoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {file.header.GlInternalFormat}");
				}

				for (var mip = 0; mip < mipMaps; mip++)
				{
					var data = file.MipMaps[mip].Faces[0].Data;
					var pixelWidth = file.MipMaps[mip].Width;
					var pixelHeight = file.MipMaps[mip].Height;

					var blocks = decoder.Decode(data, context);

					colors[mip] = ImageToBlocks.ColorsFromRawBlocks(blocks, (int)pixelWidth, (int)pixelHeight);

					context.Progress.SetProcessedBlocks(file.MipMaps.Take(mip + 1).Sum(x => x.Faces[0].Data.Length / blockSize));
				}
			}

			return colors;
		}

		/// <summary>
		/// Load a DDS file and extract either the main image or all mip maps.
		/// </summary>
		/// <param name="file">The Dds file to decode.</param>
		/// <param name="allMipMaps">If all mip maps or only the main image should be decoded.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		private ColorRgba32[][] DecodeInternal(DdsFile file, bool allMipMaps, CancellationToken token)
		{
			var mipMaps = allMipMaps ? file.header.dwMipMapCount : 1;
			var colors = new ColorRgba32[mipMaps][];

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var blockSize = GetBlockSize(file);
			var totalBlocks = file.Faces[0].MipMaps.Take((int)mipMaps).Sum(m => m.Data.Length / blockSize);

			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			if (IsSupportedRawFormat(file))
			{
				var decoder = GetRawDecoder(file);

				for (var mip = 0; mip < mipMaps; mip++)
				{
					var data = file.Faces[0].MipMaps[mip].Data;

					colors[mip] = decoder.Decode(data, context);

					context.Progress.SetProcessedBlocks(file.Faces[0].MipMaps.Take(mip + 1).Sum(x => x.Data.Length / blockSize));
				}
			}
			else
			{
				var dxtFormat = file.header.ddsPixelFormat.IsDxt10Format
					? file.dx10Header.dxgiFormat
					: file.header.ddsPixelFormat.DxgiFormat;
				var format = GetCompressionFormat(file);
				var decoder = GetRgba32Decoder(format);

				if (format.IsHdrFormat())
				{
					throw new NotSupportedException($"This Format is not an RGBA32 compatible format: {format}, please use the HDR versions of the decode methods.");
				}
				if (decoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {dxtFormat}");
				}

				for (var mip = 0; mip < mipMaps; mip++)
				{
					var data = file.Faces[0].MipMaps[mip].Data;
					var pixelWidth = file.Faces[0].MipMaps[mip].Width;
					var pixelHeight = file.Faces[0].MipMaps[mip].Height;

					var blocks = decoder.Decode(data, context);

					var image = ImageToBlocks.ColorsFromRawBlocks(blocks, (int)pixelWidth, (int)pixelHeight);

					colors[mip] = image;

					context.Progress.SetProcessedBlocks(file.Faces[0].MipMaps.Take(mip + 1).Sum(x => x.Data.Length / blockSize));
				}
			}

			return colors;
		}

		/// <summary>
		/// Decode raw encoded image asynchronously.
		/// </summary>
		/// <param name="input">The <see cref="Memory{T}"/> containing the encoded data.</param>
		/// <param name="pixelWidth">The width of the image.</param>
		/// <param name="pixelHeight">The height of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="token">The cancellation token for this operation. May be default, if the operation is not asynchronous.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		private ColorRgba32[] DecodeRawInternal(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token)
		{
			if (input.Length % GetBlockSize(format) != 0)
			{
				throw new ArgumentException("The size of the input buffer does not align with the compression format.");
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var blockSize = GetBlockSize(format);
			var totalBlocks = input.Length / blockSize;

			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			var isCompressedFormat = format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				// DecodeInternal as compressed data
				var decoder = GetRgba32Decoder(format);

				if (format.IsHdrFormat())
				{
					throw new NotSupportedException($"This Format is not an RGBA32 compatible format: {format}, please use the HDR versions of the decode methods.");
				}
				if (decoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {format}");
				}

				var blocks = decoder.Decode(input, context);

				return ImageToBlocks.ColorsFromRawBlocks(blocks, pixelWidth, pixelHeight); ;
			}

			// DecodeInternal as raw data
			var rawDecoder = GetRawDecoder(format);

			return rawDecoder.Decode(input, context);
		}

		private void DecodeBlockInternal(ReadOnlySpan<byte> blockData, CompressionFormat format, Span2D<ColorRgba32> outputSpan)
		{
			var decoder = GetRgba32Decoder(format);
			if (format.IsHdrFormat())
			{
				throw new NotSupportedException($"This Format is not an RGBA32 compatible format: {format}, please use the HDR versions of the decode methods.");
			}
			if (decoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {format}");
			}
			if (blockData.Length != GetBlockSize(format))
			{
				throw new ArgumentException("The size of the input buffer does not align with the compression format.");
			}

			var rawBlock = decoder.DecodeBlock(blockData);
			var pixels = rawBlock.AsSpan;

			pixels.Slice(0, 4).CopyTo(outputSpan.GetRowSpan(0));
			pixels.Slice(4, 4).CopyTo(outputSpan.GetRowSpan(1));
			pixels.Slice(8, 4).CopyTo(outputSpan.GetRowSpan(2));
			pixels.Slice(12, 4).CopyTo(outputSpan.GetRowSpan(3));
		}

		#region Hdr internals

		/// <summary>
		/// Load a stream and extract either the main image or all mip maps.
		/// </summary>
		/// <param name="stream">The stream containing the image file.</param>
		/// <param name="allMipMaps">If all mip maps or only the main image should be decoded.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		private Memory2D<ColorRgbFloat>[] DecodeFromStreamInternalHdr2D(Stream stream, bool allMipMaps, CancellationToken token)
		{
			var format = ImageFile.DetermineImageFormat(stream);

			switch (format)
			{
				case ImageFileFormat.Dds:
					{
						var file = DdsFile.Load(stream);
						var decoded = DecodeInternalHdr(file, allMipMaps, token);
						var mem2Ds = new Memory2D<ColorRgbFloat>[decoded.Length];
						for (var i = 0; i < decoded.Length; i++)
						{
							var mip = file.Faces[0].MipMaps[i];
							mem2Ds[i] = decoded[i].AsMemory().AsMemory2D((int)mip.Height, (int)mip.Width);
						}

						return mem2Ds;
					}

				case ImageFileFormat.Ktx:
					{
						var file = KtxFile.Load(stream);
						var decoded = DecodeInternalHdr(file, allMipMaps, token);
						var mem2Ds = new Memory2D<ColorRgbFloat>[decoded.Length];
						for (var i = 0; i < decoded.Length; i++)
						{
							var mip = file.MipMaps[i];
							mem2Ds[i] = decoded[i].AsMemory().AsMemory2D((int)mip.Height, (int)mip.Width);
						}

						return mem2Ds;
					}

				default:
					throw new InvalidOperationException("Unknown image format.");
			}
		}

		/// <summary>
		/// Load a KTX file and extract either the main image or all mip maps.
		/// </summary>
		/// <param name="file">The Ktx file to decode.</param>
		/// <param name="allMipMaps">If all mip maps or only the main image should be decoded.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		private ColorRgbFloat[][] DecodeInternalHdr(KtxFile file, bool allMipMaps, CancellationToken token)
		{
			var mipMaps = allMipMaps ? file.MipMaps.Count : 1;
			var colors = new ColorRgbFloat[mipMaps][];

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var blockSize = GetBlockSize(file.header.GlInternalFormat);
			var totalBlocks = file.MipMaps.Take(mipMaps).Sum(m => m.Faces[0].Data.Length / blockSize);

			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			var decoder = GetRgbFloatDecoder(file.header.GlInternalFormat);
			var format = GetCompressionFormat(file.header.GlInternalFormat);
			if (!format.IsHdrFormat())
			{
				throw new NotSupportedException($"This Format is not an HDR format: {format}, please use the non-HDR versions of the decode methods.");
			}
			if (decoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {file.header.GlInternalFormat}");
			}

			for (var mip = 0; mip < mipMaps; mip++)
			{
				var data = file.MipMaps[mip].Faces[0].Data;
				var pixelWidth = file.MipMaps[mip].Width;
				var pixelHeight = file.MipMaps[mip].Height;

				var blocks = decoder.Decode(data, context);

				colors[mip] = ImageToBlocks.ColorsFromRawBlocks(blocks, (int)pixelWidth, (int)pixelHeight);

				context.Progress.SetProcessedBlocks(file.MipMaps.Take(mip + 1).Sum(x => x.Faces[0].Data.Length / blockSize));
			}

			return colors;
		}

		/// <summary>
		/// Load a DDS file and extract either the main image or all mip maps.
		/// </summary>
		/// <param name="file">The Dds file to decode.</param>
		/// <param name="allMipMaps">If all mip maps or only the main image should be decoded.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		private ColorRgbFloat[][] DecodeInternalHdr(DdsFile file, bool allMipMaps, CancellationToken token)
		{
			var mipMaps = allMipMaps ? file.header.dwMipMapCount : 1;
			var colors = new ColorRgbFloat[mipMaps][];

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var blockSize = GetBlockSize(file);
			var totalBlocks = file.Faces[0].MipMaps.Take((int)mipMaps).Sum(m => m.Data.Length / blockSize);

			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			var dxtFormat = file.header.ddsPixelFormat.IsDxt10Format
				? file.dx10Header.dxgiFormat
				: file.header.ddsPixelFormat.DxgiFormat;
			var format = GetCompressionFormat(file);
			var decoder = GetRgbFloatDecoder(format);

			if (!format.IsHdrFormat())
			{
				throw new NotSupportedException($"This Format is not an HDR format: {format}, please use the non-HDR versions of the decode methods.");
			}
			if (decoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {dxtFormat}");
			}

			for (var mip = 0; mip < mipMaps; mip++)
			{
				var data = file.Faces[0].MipMaps[mip].Data;
				var pixelWidth = file.Faces[0].MipMaps[mip].Width;
				var pixelHeight = file.Faces[0].MipMaps[mip].Height;

				var blocks = decoder.Decode(data, context);

				var image = ImageToBlocks.ColorsFromRawBlocks(blocks, (int)pixelWidth, (int)pixelHeight);

				colors[mip] = image;

				context.Progress.SetProcessedBlocks(file.Faces[0].MipMaps.Take(mip + 1).Sum(x => x.Data.Length / blockSize));
			}

			return colors;
		}

		/// <summary>
		/// Decode raw encoded image asynchronously.
		/// </summary>
		/// <param name="input">The <see cref="Memory{T}"/> containing the encoded data.</param>
		/// <param name="pixelWidth">The width of the image.</param>
		/// <param name="pixelHeight">The height of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="token">The cancellation token for this operation. May be default, if the operation is not asynchronous.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		private ColorRgbFloat[] DecodeRawInternalHdr(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token)
		{
			if (input.Length % GetBlockSize(format) != 0)
			{
				throw new ArgumentException("The size of the input buffer does not align with the compression format.");
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var blockSize = GetBlockSize(format);
			var totalBlocks = input.Length / blockSize;

			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			var decoder = GetRgbFloatDecoder(format);

			if (!format.IsHdrFormat())
			{
				throw new NotSupportedException($"This Format is not an HDR format: {format}, please use the non-HDR versions of the decode methods.");
			}
			if (decoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {format}");
			}

			var blocks = decoder.Decode(input, context);

			return ImageToBlocks.ColorsFromRawBlocks(blocks, pixelWidth, pixelHeight);
		}

		private void DecodeBlockInternalHdr(ReadOnlySpan<byte> blockData, CompressionFormat format, Span2D<ColorRgbFloat> outputSpan)
		{
			var decoder = GetRgbFloatDecoder(format);
			if (!format.IsHdrFormat())
			{
				throw new NotSupportedException($"This Format is not an HDR format: {format}, please use the non-HDR versions of the decode methods.");
			}
			if (decoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {format}");
			}
			if (blockData.Length != GetBlockSize(format))
			{
				throw new ArgumentException("The size of the input buffer does not align with the compression format.");
			}

			var rawBlock = decoder.DecodeBlock(blockData);
			var pixels = rawBlock.AsSpan;

			pixels.Slice(0, 4).CopyTo(outputSpan.GetRowSpan(0));
			pixels.Slice(4, 4).CopyTo(outputSpan.GetRowSpan(1));
			pixels.Slice(8, 4).CopyTo(outputSpan.GetRowSpan(2));
			pixels.Slice(12, 4).CopyTo(outputSpan.GetRowSpan(3));
		}
		#endregion

		#region Support

		#region Is supported format

		private bool IsSupportedRawFormat(GlInternalFormat format)
		{
			return IsSupportedRawFormat(GetCompressionFormat(format));
		}

		private bool IsSupportedRawFormat(DdsFile file)
		{
			return IsSupportedRawFormat(GetCompressionFormat(file));
		}

		private bool IsSupportedRawFormat(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R:
				case CompressionFormat.Rg:
				case CompressionFormat.Rgb:
				case CompressionFormat.Rgba:
				case CompressionFormat.Bgra:
					return true;

				default:
					return false;
			}
		}

		#endregion

		#region Get decoder

		private IBcBlockDecoder<RawBlock4X4Rgba32> GetRgba32Decoder(GlInternalFormat format)
		{
			return GetRgba32Decoder(GetCompressionFormat(format));
		}

		private IBcBlockDecoder<RawBlock4X4Rgba32> GetRgba32Decoder(DdsFile file)
		{
			return GetRgba32Decoder(GetCompressionFormat(file));
		}

		private IBcBlockDecoder<RawBlock4X4Rgba32> GetRgba32Decoder(CompressionFormat format)
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
					return new Bc4Decoder(OutputOptions.Bc4Component);

				case CompressionFormat.Bc5:
					return new Bc5Decoder(OutputOptions.Bc5Component1, OutputOptions.Bc5Component2);

				case CompressionFormat.Bc7:
					return new Bc7Decoder();

				case CompressionFormat.Atc:
					return new AtcDecoder();

				case CompressionFormat.AtcExplicitAlpha:
					return new AtcExplicitAlphaDecoder();

				case CompressionFormat.AtcInterpolatedAlpha:
					return new AtcInterpolatedAlphaDecoder();

				default:
					return null;
			}
		}

		private IBcBlockDecoder<RawBlock4X4RgbFloat> GetRgbFloatDecoder(GlInternalFormat format)
		{
			return GetRgbFloatDecoder(GetCompressionFormat(format));
		}

		private IBcBlockDecoder<RawBlock4X4RgbFloat> GetRgbFloatDecoder(DdsFile file)
		{
			return GetRgbFloatDecoder(GetCompressionFormat(file));
		}

		private IBcBlockDecoder<RawBlock4X4RgbFloat> GetRgbFloatDecoder(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.Bc6S:
					return new Bc6SDecoder();
				case CompressionFormat.Bc6U:
					return new Bc6UDecoder();
				default:
					return null;
			}
		}

		#endregion

		#region Get raw decoder

		private IRawDecoder GetRawDecoder(GlInternalFormat format)
		{
			return GetRawDecoder(GetCompressionFormat(format));
		}

		private IRawDecoder GetRawDecoder(DdsFile file)
		{
			return GetRawDecoder(GetCompressionFormat(file));
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

				case CompressionFormat.Bgra:
					return new RawBgraDecoder();

				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		#endregion

		#region Get block size

		/// <summary>
		/// Gets the number of total blocks in an image with the given pixel width and height.
		/// </summary>
		/// <param name="pixelWidth">The pixel width of the image</param>
		/// <param name="pixelHeight">The pixel height of the image</param>
		/// <returns>The total number of blocks.</returns>
		public int GetBlockCount(int pixelWidth, int pixelHeight)
		{
			return ImageToBlocks.CalculateNumOfBlocks(pixelWidth, pixelHeight);
		}

		/// <summary>
		/// Gets the number of blocks in an image with the given pixel width and height.
		/// </summary>
		/// <param name="pixelWidth">The pixel width of the image</param>
		/// <param name="pixelHeight">The pixel height of the image</param>
		/// <param name="blocksWidth">The amount of blocks in the x-axis</param>
		/// <param name="blocksHeight">The amount of blocks in the y-axis</param>
		public void GetBlockCount(int pixelWidth, int pixelHeight, out int blocksWidth, out int blocksHeight)
		{
			ImageToBlocks.CalculateNumOfBlocks(pixelWidth, pixelHeight, out blocksWidth, out blocksHeight);
		}

		private int GetBlockSize(GlInternalFormat format)
		{
			return GetBlockSize(GetCompressionFormat(format));
		}

		private int GetBlockSize(DdsFile file)
		{
			return GetBlockSize(GetCompressionFormat(file));
		}

		/// <summary>
		/// Get the size of blocks for the given compression format in bytes.
		/// </summary>
		/// <param name="format">The compression format used.</param>
		/// <returns>The size of a single block in bytes.</returns>
		public int GetBlockSize(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R:
					return 1;

				case CompressionFormat.Rg:
					return 2;

				case CompressionFormat.Rgb:
					return 3;

				case CompressionFormat.Rgba:
					return 4;

				case CompressionFormat.Bgra:
					return 4;

				case CompressionFormat.Bc1:
				case CompressionFormat.Bc1WithAlpha:
					return Unsafe.SizeOf<Bc1Block>();

				case CompressionFormat.Bc2:
					return Unsafe.SizeOf<Bc2Block>();

				case CompressionFormat.Bc3:
					return Unsafe.SizeOf<Bc3Block>();

				case CompressionFormat.Bc4:
					return Unsafe.SizeOf<Bc4Block>();

				case CompressionFormat.Bc5:
					return Unsafe.SizeOf<Bc5Block>();

				case CompressionFormat.Bc6S:
				case CompressionFormat.Bc6U:
					return Unsafe.SizeOf<Bc6Block>();

				case CompressionFormat.Bc7:
					return Unsafe.SizeOf<Bc7Block>();

				case CompressionFormat.Atc:
					return Unsafe.SizeOf<AtcBlock>();

				case CompressionFormat.AtcExplicitAlpha:
					return Unsafe.SizeOf<AtcExplicitAlphaBlock>();

				case CompressionFormat.AtcInterpolatedAlpha:
					return Unsafe.SizeOf<AtcInterpolatedAlphaBlock>();

				case CompressionFormat.Unknown:
					return 0;
				
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		#endregion

		private CompressionFormat GetCompressionFormat(GlInternalFormat format)
		{
			switch (format)
			{
				case GlInternalFormat.GlR8:
					return CompressionFormat.R;

				case GlInternalFormat.GlRg8:
					return CompressionFormat.Rg;

				case GlInternalFormat.GlRgb8:
					return CompressionFormat.Rgb;

				case GlInternalFormat.GlRgba8:
					return CompressionFormat.Rgba;

				// HINT: Bgra is not supported by default. The format enum is added by an extension by Apple.
				case GlInternalFormat.GlBgra8Extension:
					return CompressionFormat.Bgra;

				case GlInternalFormat.GlCompressedRgbS3TcDxt1Ext:
					return CompressionFormat.Bc1;

				case GlInternalFormat.GlCompressedRgbaS3TcDxt1Ext:
					return CompressionFormat.Bc1WithAlpha;

				case GlInternalFormat.GlCompressedRgbaS3TcDxt3Ext:
					return CompressionFormat.Bc2;

				case GlInternalFormat.GlCompressedRgbaS3TcDxt5Ext:
					return CompressionFormat.Bc3;

				case GlInternalFormat.GlCompressedRedRgtc1Ext:
					return CompressionFormat.Bc4;

				case GlInternalFormat.GlCompressedRedGreenRgtc2Ext:
					return CompressionFormat.Bc5;

				case GlInternalFormat.GlCompressedRgbBptcUnsignedFloatArb:
					return CompressionFormat.Bc6U;

				case GlInternalFormat.GlCompressedRgbBptcSignedFloatArb:
					return CompressionFormat.Bc6S;

				// TODO: Not sure what to do with SRGB input.
				case GlInternalFormat.GlCompressedRgbaBptcUnormArb:
				case GlInternalFormat.GlCompressedSrgbAlphaBptcUnormArb:
					return CompressionFormat.Bc7;

				case GlInternalFormat.GlCompressedRgbAtc:
					return CompressionFormat.Atc;

				case GlInternalFormat.GlCompressedRgbaAtcExplicitAlpha:
					return CompressionFormat.AtcExplicitAlpha;

				case GlInternalFormat.GlCompressedRgbaAtcInterpolatedAlpha:
					return CompressionFormat.AtcInterpolatedAlpha;

				default:
					return CompressionFormat.Unknown;
			}
		}

		private CompressionFormat GetCompressionFormat(DdsFile file)
		{
			var format = file.header.ddsPixelFormat.IsDxt10Format ?
				file.dx10Header.dxgiFormat :
				file.header.ddsPixelFormat.DxgiFormat;

			switch (format)
			{
				case DxgiFormat.DxgiFormatR8Unorm:
					return CompressionFormat.R;

				case DxgiFormat.DxgiFormatR8G8Unorm:
					return CompressionFormat.Rg;

				// HINT: R8G8B8 has no DxgiFormat to convert from

				case DxgiFormat.DxgiFormatR8G8B8A8Unorm:
					return CompressionFormat.Rgba;

				case DxgiFormat.DxgiFormatB8G8R8A8Unorm:
					return CompressionFormat.Bgra;

				case DxgiFormat.DxgiFormatBc1Unorm:
				case DxgiFormat.DxgiFormatBc1UnormSrgb:
				case DxgiFormat.DxgiFormatBc1Typeless:
					if (file.header.ddsPixelFormat.dwFlags.HasFlag(PixelFormatFlags.DdpfAlphaPixels))
						return CompressionFormat.Bc1WithAlpha;

					if (InputOptions.DdsBc1ExpectAlpha)
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

		private int GetBufferSize(CompressionFormat format, int pixelWidth, int pixelHeight)
		{
			switch (format)
			{
				case CompressionFormat.R:
					return pixelWidth * pixelHeight;

				case CompressionFormat.Rg:
					return 2 * pixelWidth * pixelHeight;

				case CompressionFormat.Rgb:
					return 3 * pixelWidth * pixelHeight;

				case CompressionFormat.Rgba:
				case CompressionFormat.Bgra:
					return 4 * pixelWidth * pixelHeight;

				case CompressionFormat.Bc1:
				case CompressionFormat.Bc1WithAlpha:
				case CompressionFormat.Bc2:
				case CompressionFormat.Bc3:
				case CompressionFormat.Bc4:
				case CompressionFormat.Bc5:
				case CompressionFormat.Bc6S:
				case CompressionFormat.Bc6U:
				case CompressionFormat.Bc7:
				case CompressionFormat.Atc:
				case CompressionFormat.AtcExplicitAlpha:
				case CompressionFormat.AtcInterpolatedAlpha:
					return GetBlockSize(format) * ImageToBlocks.CalculateNumOfBlocks(pixelWidth, pixelHeight);

				case CompressionFormat.Unknown:
					return 0;
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		#endregion
	}
}
