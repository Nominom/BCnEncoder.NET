using BCnEncoder.Decoder.Options;
using BCnEncoder.Shared;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Decoder
{
	/// <summary>
	/// Decodes block-compressed formats into Rgba.
	/// </summary>
	public class BcDecoder
	{
		/// <summary>
		/// The options for the decoder.
		/// </summary>
		public DecoderOptions Options { get; } = new DecoderOptions();

		/// <summary>
		/// The output options of the decoder.
		/// </summary>
		public DecoderOutputOptions OutputOptions { get; } = new DecoderOutputOptions();


		/// <inheritdoc cref="DecodeInternal"/>
		public Task<BCnTextureData> DecodeAsync(BCnTextureData texture, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(texture, token), token);
		}

		/// <inheritdoc cref="DecodeInternal"/>
		public BCnTextureData Decode(BCnTextureData texture)
		{
			return DecodeInternal(texture, CancellationToken.None);
		}

		#region Ldr Async Api

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
		public Task<ColorRgba32[]> DecodeRawLdrAsync(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			var readBytes = inputStream.Read(dataArray, 0, dataArray.Length);

			if (readBytes < dataArray.Length)
			{
				throw new InvalidOperationException("Not enough bytes available in stream to read whole Image!");
			}

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
		public Task<ColorRgba32[]> DecodeRawLdrAsync(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			return Task.Run(() => DecodeRawInternal(input, pixelWidth, pixelHeight, format, token), token);
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
		public Task<Memory2D<ColorRgba32>> DecodeRawLdr2DAsync(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			var readBytes = inputStream.Read(dataArray, 0, dataArray.Length);

			if (readBytes < dataArray.Length)
			{
				throw new InvalidOperationException("Not enough bytes available in stream to read whole Image!");
			}

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
		public Task<Memory2D<ColorRgba32>> DecodeRawLdr2DAsync(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			return Task.Run(() => DecodeRawInternal(input, pixelWidth, pixelHeight, format, token)
				.AsMemory().AsMemory2D(pixelHeight, pixelWidth), token);
		}

		#endregion

		#region Ldr Sync API

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
		public ColorRgba32[] DecodeRawLdr(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			var readBytes = inputStream.Read(dataArray, 0, dataArray.Length);

			if (readBytes < dataArray.Length)
			{
				throw new InvalidOperationException("Not enough bytes available in stream to read whole Image!");
			}

			return DecodeRawLdr(dataArray, pixelWidth, pixelHeight, format);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// </summary>
		/// <param name="input">The byte array containing the raw encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded image.</returns>
		public ColorRgba32[] DecodeRawLdr(byte[] input, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			return DecodeRawInternal(input, pixelWidth, pixelHeight, format, default);
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
		public Memory2D<ColorRgba32> DecodeRawLdr2D(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			var readBytes = inputStream.Read(dataArray, 0, dataArray.Length);

			if (readBytes < dataArray.Length)
			{
				throw new InvalidOperationException("Not enough bytes available in stream to read whole Image!");
			}

			var decoded = DecodeRawLdr(dataArray, pixelWidth, pixelHeight, format);
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
		public Memory2D<ColorRgba32> DecodeRawLdr2D(byte[] input, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var decoded = DecodeRawInternal(input, pixelWidth, pixelHeight, format, default);
			return decoded.AsMemory().AsMemory2D(pixelHeight, pixelWidth);
		}

		/// <summary>
		/// Decode a single block from raw bytes and return it as a <see cref="Memory2D{T}"/>.
		/// Input Span size needs to equal the block size.
		/// To get the block size (in bytes) of the compression format used, see <see cref="GetBlockSize(BCnEncoder.Shared.CompressionFormat)"/>.
		/// </summary>
		/// <param name="blockData">The encoded block in bytes.</param>
		/// <param name="format">The compression format used.</param>
		/// <returns>The decoded 4x4 block.</returns>
		public Memory2D<ColorRgba32> DecodeBlockLdr(ReadOnlySpan<byte> blockData, CompressionFormat format)
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
		public void DecodeBlockLdr(ReadOnlySpan<byte> blockData, CompressionFormat format, Span2D<ColorRgba32> outputSpan)
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
		public int DecodeBlockLdr(Stream inputStream, CompressionFormat format, Span2D<ColorRgba32> outputSpan)
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

		#endregion

		#region Hdr Async Api

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
		public Task<ColorRgbaFloat[]> DecodeRawHdrAsync(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			var readBytes = inputStream.Read(dataArray, 0, dataArray.Length);

			if (readBytes < dataArray.Length)
			{
				throw new InvalidOperationException("Not enough bytes available in stream to read whole Image!");
			}

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
		public Task<ColorRgbaFloat[]> DecodeRawHdrAsync(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			return Task.Run(() => DecodeRawInternalHdr(input, pixelWidth, pixelHeight, format, token), token);
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
		public Task<Memory2D<ColorRgbaFloat>> DecodeRawHdr2DAsync(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			var readBytes = inputStream.Read(dataArray, 0, dataArray.Length);

			if (readBytes < dataArray.Length)
			{
				throw new InvalidOperationException("Not enough bytes available in stream to read whole Image!");
			}

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
		public Task<Memory2D<ColorRgbaFloat>> DecodeRawHdr2DAsync(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			return Task.Run(() => DecodeRawInternalHdr(input, pixelWidth, pixelHeight, format, token)
				.AsMemory().AsMemory2D(pixelHeight, pixelWidth), token);
		}

		#endregion

		#region Hdr Sync API

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
		public ColorRgbaFloat[] DecodeRawHdr(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			var readBytes = inputStream.Read(dataArray, 0, dataArray.Length);

			if (readBytes < dataArray.Length)
			{
				throw new InvalidOperationException("Not enough bytes available in stream to read whole Image!");
			}

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
		public ColorRgbaFloat[] DecodeRawHdr(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			return DecodeRawInternalHdr(input, pixelWidth, pixelHeight, format, default);
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
		public Memory2D<ColorRgbaFloat> DecodeRawHdr2D(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			var readBytes = inputStream.Read(dataArray, 0, dataArray.Length);

			if (readBytes < dataArray.Length)
			{
				throw new InvalidOperationException("Not enough bytes available in stream to read whole Image!");
			}

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
		public Memory2D<ColorRgbaFloat> DecodeRawHdr2D(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var decoded = DecodeRawInternalHdr(input, pixelWidth, pixelHeight, format, default);
			return decoded.AsMemory().AsMemory2D(pixelHeight, pixelWidth);
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
		#endregion


		/// <summary>
		/// Decode all faces and mipmaps of a <see cref="BCnTextureData"/> into <see cref="CompressionFormat.Rgba32"/> for ldr formats,
		/// and <see cref="CompressionFormat.RgbaFloat"/> for hdr formats.
		/// </summary>
		/// <param name="texture">The texture data to decode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>A new <see cref="BCnTextureData"/> containing the decoded data.</returns>
		private BCnTextureData DecodeInternal(BCnTextureData texture, CancellationToken token)
		{
			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = Options.IsParallel,
				TaskCount = Options.TaskCount
			};
			var blockSize = texture.Format.BytesPerBlock();
			var totalBlocks = 0L;
			for (var m = 0; m < texture.NumMips; m++)
			{
				totalBlocks += texture.MipLevels[m].SizeInBytes / blockSize;
			}
			totalBlocks *= texture.NumFaces;
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			var decoder = GetDecoder(texture.Format);

			if (decoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {texture.Format}");
			}

			var outputData = new BCnTextureData(decoder.DecodedFormat, texture.Width, texture.Height,
				texture.NumMips, texture.IsCubeMap, false);

			for (var f = 0; f < texture.NumFaces; f++)
			{
				for (var m = 0; m < texture.NumMips; m++)
				{
					var data = texture.Faces[f].Mips[m].Data;
					outputData.Faces[f].Mips[m].Data = decoder.Decode(data, texture.Faces[f].Mips[m].Width, texture.Faces[f].Mips[m].Height, context);
				}
			}

			return outputData;
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


			// DecodeInternal as compressed data
			var decoder = GetLdrDecoder(format);

			if (format.IsHdrFormat())
			{
				throw new NotSupportedException($"This Format is not an RGBA32 compatible format: {format}, please use the HDR versions of the decode methods.");
			}
			if (decoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {format}");
			}

			return decoder.DecodeColor(input, pixelWidth, pixelHeight, context);
		}

		private void DecodeBlockInternal(ReadOnlySpan<byte> blockData, CompressionFormat format, Span2D<ColorRgba32> outputSpan)
		{
			if (format.IsHdrFormat())
			{
				throw new NotSupportedException($"This Format is not an RGBA32 compatible format: {format}, please use the HDR versions of the decode methods.");
			}

			var decoder = GetLdrDecoder(format) as IBcLdrBlockDecoder;

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

		/// <summary>
		/// Decode raw encoded image asynchronously.
		/// </summary>
		/// <param name="input">The <see cref="Memory{T}"/> containing the encoded data.</param>
		/// <param name="pixelWidth">The width of the image.</param>
		/// <param name="pixelHeight">The height of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="token">The cancellation token for this operation. May be default, if the operation is not asynchronous.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		private ColorRgbaFloat[] DecodeRawInternalHdr(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token)
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

			var decoder = GetHdrDecoder(format);

			if (!format.IsHdrFormat())
			{
				throw new NotSupportedException($"This Format is not an HDR format: {format}, please use the non-HDR versions of the decode methods.");
			}
			if (decoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {format}");
			}

			return decoder.DecodeColor(input, pixelWidth, pixelHeight, context);
		}

		private void DecodeBlockInternalHdr(ReadOnlySpan<byte> blockData, CompressionFormat format, Span2D<ColorRgbFloat> outputSpan)
		{
			if (!format.IsBlockCompressedFormat())
			{
				throw new NotSupportedException($"This Format is not a block-compressed format: {format}.");
			}
			var decoder = GetHdrDecoder(format) as IBcHdrBlockDecoder;

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

		#region Support

		#region Get decoder

		private IBcDecoder GetDecoder(CompressionFormat format)
		{
			return (IBcDecoder)GetLdrDecoder(format) ??
			       GetHdrDecoder(format);
		}

		private IBcLdrDecoder GetLdrDecoder(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R8:
					return new RawLdrDecoder<ColorR8>(OutputOptions.RedAsLuminance);

				case CompressionFormat.R8G8:
					return new RawLdrDecoder<ColorR8G8>(false);

				case CompressionFormat.Rgb24:
					return new RawLdrDecoder<ColorRgb24>(false);

				case CompressionFormat.Bgr24:
					return new RawLdrDecoder<ColorBgr24>(false);

				case CompressionFormat.Rgba32:
					return new RawLdrDecoder<ColorRgba32>(false);

				case CompressionFormat.Bgra32:
					return new RawLdrDecoder<ColorBgra32>(false);

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

		private IBcHdrDecoder GetHdrDecoder(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.Bc6S:
					return new Bc6SDecoder();

				case CompressionFormat.Bc6U:
					return new Bc6UDecoder();

				case CompressionFormat.RgbaFloat:
					return new RawHdrDecoder<ColorRgbaFloat>();

				case CompressionFormat.RgbaHalf:
					return new RawHdrDecoder<ColorRgbaHalf>();

				case CompressionFormat.RgbFloat:
					return new RawHdrDecoder<ColorRgbFloat>();

				case CompressionFormat.RgbHalf:
					return new RawHdrDecoder<ColorRgbHalf>();

				case CompressionFormat.Rgbe:
					return new RawHdrDecoder<ColorRgbe>();

				case CompressionFormat.Xyze:
					return new RawHdrDecoder<ColorXyze>();

				default:
					return null;
			}
		}

		#endregion

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

		/// <summary>
		/// Get the size of blocks for the given compression format in bytes.
		/// </summary>
		/// <param name="format">The compression format used.</param>
		/// <returns>The size of a single block in bytes.</returns>
		public int GetBlockSize(CompressionFormat format)
		{
			return format.BytesPerBlock();
		}

		private long GetBufferSize(CompressionFormat format, int pixelWidth, int pixelHeight)
		{
			return format.CalculateMipByteSize(pixelWidth, pixelHeight);
		}

		#endregion
	}
}
