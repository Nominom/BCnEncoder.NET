using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Encoder.Bptc;
using BCnEncoder.Encoder.Options;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Encoder
{
	/// <summary>
	/// The pixel format determining the rgba layout of input data in <see cref="BcEncoder"/>.
	/// </summary>
	public enum PixelFormat
	{
		/// <summary>
		/// 8 bits per channel RGBA.
		/// </summary>
		Rgba32,

		/// <summary>
		/// 8 bits per channel BGRA.
		/// </summary>
		Bgra32,

		/// <summary>
		/// 8 bits per channel ARGB.
		/// </summary>
		Argb32,

		/// <summary>
		/// 8 bits per channel RGB.
		/// </summary>
		Rgb24,

		/// <summary>
		/// 8 bits per channel BGR.
		/// </summary>
		Bgr24
	}

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

		#region LDR
		#region Async Api

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream asynchronously.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory{T}"/>.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="format">The pixel format the given data is in.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		public Task EncodeToStreamAsync(ReadOnlyMemory<byte> input, int width, int height, PixelFormat format, Stream outputStream, CancellationToken token = default)
		{
			return Task.Run(() =>
			{
				EncodeToStreamInternal(ByteToColorMemory(input.Span, width, height, format), outputStream, token);
			}, token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream asynchronously.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		public Task EncodeToStreamAsync(ReadOnlyMemory2D<ColorRgba32> input, Stream outputStream, CancellationToken token = default)
		{
			return Task.Run(() =>
			{
				EncodeToStreamInternal(input, outputStream, default);
			}, token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file asynchronously.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory{T}"/>.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="format">The pixel format the given data is in.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public Task<KtxFile> EncodeToKtxAsync(ReadOnlyMemory<byte> input, int width, int height, PixelFormat format, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToKtxInternal(ByteToColorMemory(input.Span, width, height, format), token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file asynchronously.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public Task<KtxFile> EncodeToKtxAsync(ReadOnlyMemory2D<ColorRgba32> input, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToKtxInternal(input, token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file asynchronously.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory{T}"/>.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="format">The pixel format the given data is in.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public Task<DdsFile> EncodeToDdsAsync(ReadOnlyMemory<byte> input, int width, int height, PixelFormat format, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToDdsInternal(ByteToColorMemory(input.Span, width, height, format), token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file asynchronously.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public Task<DdsFile> EncodeToDdsAsync(ReadOnlyMemory2D<ColorRgba32> input, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToDdsInternal(input, token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a list of byte buffers asynchronously. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory{T}"/>.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="format">The pixel format the given data is in.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>A list of raw encoded mipmap input.</returns>
		public Task<byte[][]> EncodeToRawBytesAsync(ReadOnlyMemory<byte> input, int width, int height, PixelFormat format, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToRawInternal(ByteToColorMemory(input.Span, width, height, format), token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels into an array of byte buffers asynchronously. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>A list of raw encoded mipmap input.</returns>
		/// <remarks>To get the width and height of the encoded mip levels, see <see cref="CalculateMipMapSize"/>.</remarks>
		public Task<byte[][]> EncodeToRawBytesAsync(ReadOnlyMemory2D<ColorRgba32> input, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToRawInternal(input, token), token);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer asynchronously. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory{T}"/>.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="format">The pixel format the given data is in.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The raw encoded input.</returns>
		/// <remarks>To get the width and height of the encoded mip level, see <see cref="CalculateMipMapSize"/>.</remarks>
		public Task<byte[]> EncodeToRawBytesAsync(ReadOnlyMemory<byte> input, int width, int height, PixelFormat format, int mipLevel, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToRawInternal(ByteToColorMemory(input.Span, width, height, format), mipLevel, out _, out _, token), token);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer asynchronously. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>The raw encoded input.</returns>
		/// <remarks>To get the width and height of the encoded mip level, see <see cref="CalculateMipMapSize"/>.</remarks>
		public Task<byte[]> EncodeToRawBytesAsync(ReadOnlyMemory2D<ColorRgba32> input, int mipLevel, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToRawInternal(input, mipLevel, out _, out _, token), token);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap image to a stream asynchronously either in ktx or dds format.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>A <see cref="Task"/>.</returns>
		public Task EncodeCubeMapToStreamAsync(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front, Stream outputStream, CancellationToken token = default)
		{
			return Task.Run(() => EncodeCubeMapToStreamInternal(right, left, top, down, back, front, outputStream, token), token);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap image to a <see cref="KtxFile"/> asynchronously.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>A <see cref="Task{KtxFile}"/> of type <see cref="KtxFile"/>.</returns>
		public Task<KtxFile> EncodeCubeMapToKtxAsync(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front, CancellationToken token = default)
		{
			return Task.Run(() => EncodeCubeMapToKtxInternal(right, left, top, down, back, front, token), token);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap image to a <see cref="DdsFile"/> asynchronously.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>A <see cref="Task{DdsFile}"/> of type <see cref="DdsFile"/>.</returns>
		public Task<DdsFile> EncodeCubeMapToDdsAsync(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front, CancellationToken token = default)
		{
			return Task.Run(() => EncodeCubeMapToDdsInternal(right, left, top, down, back, front, token), token);
		}

		#endregion

		#region Sync Api

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlySpan{T}"/>.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="format">The pixel format the input data is in.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public void EncodeToStream(ReadOnlySpan<byte> input, int width, int height, PixelFormat format, Stream outputStream)
		{
			EncodeToStream(ByteToColorMemory(input, width, height, format), outputStream);
		}

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public void EncodeToStream(ReadOnlyMemory2D<ColorRgba32> input, Stream outputStream)
		{
			EncodeToStreamInternal(input, outputStream, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlySpan{T}"/>.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="format">The pixel format the input data is in.</param>
		/// <returns>The <see cref="KtxFile"/> containing the encoded image.</returns>
		public KtxFile EncodeToKtx(ReadOnlySpan<byte> input, int width, int height, PixelFormat format)
		{
			return EncodeToKtx(ByteToColorMemory(input, width, height, format));
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <returns>The <see cref="KtxFile"/> containing the encoded image.</returns>
		public KtxFile EncodeToKtx(ReadOnlyMemory2D<ColorRgba32> input)
		{
			return EncodeToKtxInternal(input, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlySpan{T}"/>.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="format">The pixel format the input data is in.</param>
		/// <returns>The <see cref="DdsFile"/> containing the encoded image.</returns>
		public DdsFile EncodeToDds(ReadOnlySpan<byte> input, int width, int height, PixelFormat format)
		{
			return EncodeToDds(ByteToColorMemory(input, width, height, format));
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <returns>The <see cref="DdsFile"/> containing the encoded image.</returns>
		public DdsFile EncodeToDds(ReadOnlyMemory2D<ColorRgba32> input)
		{
			return EncodeToDdsInternal(input, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into an array of byte buffers. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlySpan{T}"/>.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="format">The pixel format the input data is in.</param>
		/// <returns>An array of byte buffers containing all mipmap levels.</returns>
		/// <remarks>To get the width and height of the encoded mip levels, see <see cref="CalculateMipMapSize"/>.</remarks>
		public byte[][] EncodeToRawBytes(ReadOnlySpan<byte> input, int width, int height, PixelFormat format)
		{
			return EncodeToRawBytes(ByteToColorMemory(input, width, height, format));
		}

		/// <summary>
		/// Encodes all mipmap levels into a list of byte buffers. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <returns>An array of byte buffers containing all mipmap levels.</returns>
		/// <remarks>To get the width and height of the encoded mip levels, see <see cref="CalculateMipMapSize"/>.</remarks>
		public byte[][] EncodeToRawBytes(ReadOnlyMemory2D<ColorRgba32> input)
		{
			return EncodeToRawInternal(input, default);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlySpan{T}"/>.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="format">The pixel format the input data is in.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="mipWidth">The width of the mipmap.</param>
		/// <param name="mipHeight">The height of the mipmap.</param>
		/// <returns>A byte buffer containing the encoded data of the requested mip-level.</returns>
		public byte[] EncodeToRawBytes(ReadOnlySpan<byte> input, int width, int height, PixelFormat format, int mipLevel, out int mipWidth, out int mipHeight)
		{
			return EncodeToRawInternal(ByteToColorMemory(input, width, height, format), mipLevel, out mipWidth, out mipHeight, default);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="mipWidth">The width of the mipmap.</param>
		/// <param name="mipHeight">The height of the mipmap.</param>
		/// <returns>A byte buffer containing the encoded data of the requested mip-level.</returns>
		public byte[] EncodeToRawBytes(ReadOnlyMemory2D<ColorRgba32> input, int mipLevel, out int mipWidth, out int mipHeight)
		{
			return EncodeToRawInternal(input, mipLevel, out mipWidth, out mipHeight, default);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap image to a stream either in ktx or dds format.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="width">The width of the faces.</param>
		/// <param name="height">The height of the faces.</param>
		/// <param name="format">The pixel format the input data is in.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public void EncodeCubeMapToStream(ReadOnlySpan<byte> right, ReadOnlySpan<byte> left,
			ReadOnlySpan<byte> top, ReadOnlySpan<byte> down,
			ReadOnlySpan<byte> back, ReadOnlySpan<byte> front,
			int width, int height, PixelFormat format, Stream outputStream)
		{
			EncodeCubeMapToStreamInternal(
				ByteToColorMemory(right, width, height, format),
				ByteToColorMemory(left, width, height, format),
				ByteToColorMemory(top, width, height, format),
				ByteToColorMemory(down, width, height, format),
				ByteToColorMemory(back, width, height, format),
				ByteToColorMemory(front, width, height, format),
				outputStream, default);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap image to a stream either in ktx or dds format.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public void EncodeCubeMapToStream(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front, Stream outputStream)
		{
			EncodeCubeMapToStreamInternal(right, left, top, down, back, front, outputStream, default);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap image to a <see cref="KtxFile"/>.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="width">The width of the faces.</param>
		/// <param name="height">The height of the faces.</param>
		/// <param name="format">The pixel format the input data is in.</param>
		/// <returns>The encoded image as a <see cref="KtxFile"/>.</returns>
		public KtxFile EncodeCubeMapToKtx(ReadOnlySpan<byte> right, ReadOnlySpan<byte> left,
			ReadOnlySpan<byte> top, ReadOnlySpan<byte> down,
			ReadOnlySpan<byte> back, ReadOnlySpan<byte> front,
			int width, int height, PixelFormat format)
		{
			return EncodeCubeMapToKtxInternal(
				ByteToColorMemory(right, width, height, format),
				ByteToColorMemory(left, width, height, format),
				ByteToColorMemory(top, width, height, format),
				ByteToColorMemory(down, width, height, format),
				ByteToColorMemory(back, width, height, format),
				ByteToColorMemory(front, width, height, format), default);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap image to a <see cref="KtxFile"/>.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <returns>The encoded image as a <see cref="KtxFile"/>.</returns>
		public KtxFile EncodeCubeMapToKtx(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front)
		{
			return EncodeCubeMapToKtxInternal(right, left, top, down, back, front, default);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap image to a <see cref="DdsFile"/>.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="width">The width of the faces.</param>
		/// <param name="height">The height of the faces.</param>
		/// <param name="format">The pixel format the input data is in.</param>
		/// <returns>The encoded image as a <see cref="DdsFile"/>.</returns>
		public DdsFile EncodeCubeMapToDds(ReadOnlySpan<byte> right, ReadOnlySpan<byte> left,
			ReadOnlySpan<byte> top, ReadOnlySpan<byte> down,
			ReadOnlySpan<byte> back, ReadOnlySpan<byte> front,
			int width, int height, PixelFormat format)
		{
			return EncodeCubeMapToDdsInternal(
				ByteToColorMemory(right, width, height, format),
				ByteToColorMemory(left, width, height, format),
				ByteToColorMemory(top, width, height, format),
				ByteToColorMemory(down, width, height, format),
				ByteToColorMemory(back, width, height, format),
				ByteToColorMemory(front, width, height, format), default);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap image to a <see cref="DdsFile"/>.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <returns>The encoded image as a <see cref="DdsFile"/>.</returns>
		public DdsFile EncodeCubeMapToDds(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front)
		{
			return EncodeCubeMapToDdsInternal(right, left, top, down, back, front, default);
		}

		/// <summary>
		/// Encodes a single 4x4 block to raw encoded bytes. Input Span length must be exactly 16.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <returns>Raw encoded data</returns>
		public byte[] EncodeBlock(ReadOnlySpan<ColorRgba32> inputBlock)
		{
			if (inputBlock.Length != 16)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			return EncodeBlockInternal(inputBlock.AsSpan2D(4, 4));
		}

		/// <summary>
		/// Encodes a single 4x4 block to raw encoded bytes. Input Span width and height must be exactly 4.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <returns>Raw encoded data</returns>
		public byte[] EncodeBlock(ReadOnlySpan2D<ColorRgba32> inputBlock)
		{
			if (inputBlock.Width != 4 || inputBlock.Height != 4)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			return EncodeBlockInternal(inputBlock);
		}

		/// <summary>
		/// Encodes a single 4x4 block and writes the encoded block to a stream. Input Span length must be exactly 16.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <param name="outputStream">Output stream where the encoded block will be written to.</param>
		public void EncodeBlock(ReadOnlySpan<ColorRgba32> inputBlock, Stream outputStream)
		{
			if (inputBlock.Length != 16)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			EncodeBlockInternal(inputBlock.AsSpan2D(4, 4), outputStream);
		}

		/// <summary>
		/// Encodes a single 4x4 block and writes the encoded block to a stream. Input Span width and height must be exactly 4.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <param name="outputStream">Output stream where the encoded block will be written to.</param>
		public void EncodeBlock(ReadOnlySpan2D<ColorRgba32> inputBlock, Stream outputStream)
		{
			if (inputBlock.Width != 4 || inputBlock.Height != 4)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			EncodeBlockInternal(inputBlock, outputStream);
		}

		/// <summary>
		/// Gets the block size of the currently selected compression format in bytes.
		/// </summary>
		/// <returns>The size of a single 4x4 block in bytes</returns>
		public int GetBlockSize()
		{
			var compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				var hdrEncoder = GetFloatBlockEncoder(OutputOptions.Format);
				
				if (hdrEncoder == null)
				{
					throw new NotSupportedException($"This format is either not supported or does not use block compression: {OutputOptions.Format}");
				}

				return hdrEncoder.GetBlockSize();
			}
			return compressedEncoder.GetBlockSize();
		}

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

		#endregion
		#endregion

		#region HDR

		#region Async Api

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream asynchronously.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		public Task EncodeToStreamHdrAsync(ReadOnlyMemory2D<ColorRgbFloat> input, Stream outputStream, CancellationToken token = default)
		{
			return Task.Run(() =>
			{
				EncodeToStreamInternalHdr(input, outputStream, default);
			}, token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file asynchronously.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public Task<KtxFile> EncodeToKtxHdrAsync(ReadOnlyMemory2D<ColorRgbFloat> input, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToKtxInternalHdr(input, token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file asynchronously.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public Task<DdsFile> EncodeToDdsHdrAsync(ReadOnlyMemory2D<ColorRgbFloat> input, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToDdsInternalHdr(input, token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels of an HDR image into an array of byte buffers asynchronously. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>A list of raw encoded mipmap input.</returns>
		/// <remarks>To get the width and height of the encoded mip levels, see <see cref="CalculateMipMapSize"/>.</remarks>
		public Task<byte[][]> EncodeToRawBytesHdrAsync(ReadOnlyMemory2D<ColorRgbFloat> input, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToRawInternalHdr(input, token), token);
		}

		/// <summary>
		/// Encodes a single mip level of the input HDR image to a byte buffer asynchronously. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>The raw encoded input.</returns>
		/// <remarks>To get the width and height of the encoded mip level, see <see cref="CalculateMipMapSize"/>.</remarks>
		public Task<byte[]> EncodeToRawBytesHdrAsync(ReadOnlyMemory2D<ColorRgbFloat> input, int mipLevel, CancellationToken token = default)
		{
			return Task.Run(() => EncodeToRawInternalHdr(input, mipLevel, out _, out _, token), token);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap HDR image to a stream asynchronously either in ktx or dds format.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>A <see cref="Task"/>.</returns>
		public Task EncodeCubeMapToStreamHdrAsync(ReadOnlyMemory2D<ColorRgbFloat> right, ReadOnlyMemory2D<ColorRgbFloat> left,
			ReadOnlyMemory2D<ColorRgbFloat> top, ReadOnlyMemory2D<ColorRgbFloat> down,
			ReadOnlyMemory2D<ColorRgbFloat> back, ReadOnlyMemory2D<ColorRgbFloat> front, Stream outputStream, CancellationToken token = default)
		{
			return Task.Run(() => EncodeCubeMapToStreamInternalHdr(right, left, top, down, back, front, outputStream, token), token);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap HDR image to a <see cref="KtxFile"/> asynchronously.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>A <see cref="Task{KtxFile}"/> of type <see cref="KtxFile"/>.</returns>
		public Task<KtxFile> EncodeCubeMapToKtxHdrAsync(ReadOnlyMemory2D<ColorRgbFloat> right, ReadOnlyMemory2D<ColorRgbFloat> left,
			ReadOnlyMemory2D<ColorRgbFloat> top, ReadOnlyMemory2D<ColorRgbFloat> down,
			ReadOnlyMemory2D<ColorRgbFloat> back, ReadOnlyMemory2D<ColorRgbFloat> front, CancellationToken token = default)
		{
			return Task.Run(() => EncodeCubeMapToKtxInternalHdr(right, left, top, down, back, front, token), token);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap HDR image to a <see cref="DdsFile"/> asynchronously.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>A <see cref="Task{DdsFile}"/> of type <see cref="DdsFile"/>.</returns>
		public Task<DdsFile> EncodeCubeMapToDdsHdrAsync(ReadOnlyMemory2D<ColorRgbFloat> right, ReadOnlyMemory2D<ColorRgbFloat> left,
			ReadOnlyMemory2D<ColorRgbFloat> top, ReadOnlyMemory2D<ColorRgbFloat> down,
			ReadOnlyMemory2D<ColorRgbFloat> back, ReadOnlyMemory2D<ColorRgbFloat> front, CancellationToken token = default)
		{
			return Task.Run(() => EncodeCubeMapToDdsInternalHdr(right, left, top, down, back, front, token), token);
		}

		#endregion

		#region Sync Api

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public void EncodeToStreamHdr(ReadOnlyMemory2D<ColorRgbFloat> input, Stream outputStream)
		{
			EncodeToStreamInternalHdr(input, outputStream, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <returns>The <see cref="KtxFile"/> containing the encoded image.</returns>
		public KtxFile EncodeToKtxHdr(ReadOnlyMemory2D<ColorRgbFloat> input)
		{
			return EncodeToKtxInternalHdr(input, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <returns>The <see cref="DdsFile"/> containing the encoded image.</returns>
		public DdsFile EncodeToDdsHdr(ReadOnlyMemory2D<ColorRgbFloat> input)
		{
			return EncodeToDdsInternalHdr(input, default);
		}

		/// <summary>
		/// Encodes all mipmap levels of a HDR image into a list of byte buffers. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <returns>An array of byte buffers containing all mipmap levels.</returns>
		/// <remarks>To get the width and height of the encoded mip levels, see <see cref="CalculateMipMapSize"/>.</remarks>
		public byte[][] EncodeToRawBytesHdr(ReadOnlyMemory2D<ColorRgbFloat> input)
		{
			return EncodeToRawInternalHdr(input, default);
		}

		/// <summary>
		/// Encodes a single mip level of the HDR input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="mipWidth">The width of the mipmap.</param>
		/// <param name="mipHeight">The height of the mipmap.</param>
		/// <returns>A byte buffer containing the encoded data of the requested mip-level.</returns>
		public byte[] EncodeToRawBytesHdr(ReadOnlyMemory2D<ColorRgbFloat> input, int mipLevel, out int mipWidth, out int mipHeight)
		{
			return EncodeToRawInternalHdr(input, mipLevel, out mipWidth, out mipHeight, default);
		}

		/// <summary>
		/// Encodes all mipMaps of a HDR cubeMap image to a stream either in ktx or dds format.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public void EncodeCubeMapToStreamHdr(ReadOnlyMemory2D<ColorRgbFloat> right, ReadOnlyMemory2D<ColorRgbFloat> left,
			ReadOnlyMemory2D<ColorRgbFloat> top, ReadOnlyMemory2D<ColorRgbFloat> down,
			ReadOnlyMemory2D<ColorRgbFloat> back, ReadOnlyMemory2D<ColorRgbFloat> front, Stream outputStream)
		{
			EncodeCubeMapToStreamInternalHdr(right, left, top, down, back, front, outputStream, default);
		}

		/// <summary>
		/// Encodes all mipMaps of a HDR cubeMap image to a <see cref="KtxFile"/>.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <returns>The encoded image as a <see cref="KtxFile"/>.</returns>
		public KtxFile EncodeCubeMapToKtxHdr(ReadOnlyMemory2D<ColorRgbFloat> right, ReadOnlyMemory2D<ColorRgbFloat> left,
			ReadOnlyMemory2D<ColorRgbFloat> top, ReadOnlyMemory2D<ColorRgbFloat> down,
			ReadOnlyMemory2D<ColorRgbFloat> back, ReadOnlyMemory2D<ColorRgbFloat> front)
		{
			return EncodeCubeMapToKtxInternalHdr(right, left, top, down, back, front, default);
		}

		/// <summary>
		/// Encodes all mipMaps of a HDR cubeMap image to a <see cref="DdsFile"/>.
		/// The format can be set in <see cref="EncoderOutputOptions.FileFormat"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <returns>The encoded image as a <see cref="DdsFile"/>.</returns>
		public DdsFile EncodeCubeMapToDdsHdr(ReadOnlyMemory2D<ColorRgbFloat> right, ReadOnlyMemory2D<ColorRgbFloat> left,
			ReadOnlyMemory2D<ColorRgbFloat> top, ReadOnlyMemory2D<ColorRgbFloat> down,
			ReadOnlyMemory2D<ColorRgbFloat> back, ReadOnlyMemory2D<ColorRgbFloat> front)
		{
			return EncodeCubeMapToDdsInternalHdr(right, left, top, down, back, front, default);
		}

		/// <summary>
		/// Encodes a single 4x4 HDR block to raw encoded bytes. Input Span length must be exactly 16.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <returns>Raw encoded data</returns>
		public byte[] EncodeBlockHdr(ReadOnlySpan<ColorRgbFloat> inputBlock)
		{
			if (inputBlock.Length != 16)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			return EncodeBlockInternalHdr(inputBlock.AsSpan2D(4, 4));
		}

		/// <summary>
		/// Encodes a single 4x4 HDR block to raw encoded bytes. Input Span width and height must be exactly 4.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <returns>Raw encoded data</returns>
		public byte[] EncodeBlockHdr(ReadOnlySpan2D<ColorRgbFloat> inputBlock)
		{
			if (inputBlock.Width != 4 || inputBlock.Height != 4)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			return EncodeBlockInternalHdr(inputBlock);
		}

		/// <summary>
		/// Encodes a single 4x4 HDR block and writes the encoded block to a stream. Input Span length must be exactly 16.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <param name="outputStream">Output stream where the encoded block will be written to.</param>
		public void EncodeBlockHdr(ReadOnlySpan<ColorRgbFloat> inputBlock, Stream outputStream)
		{
			if (inputBlock.Length != 16)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			EncodeBlockInternalHdr(inputBlock.AsSpan2D(4, 4), outputStream);
		}

		/// <summary>
		/// Encodes a single 4x4 HDR block and writes the encoded block to a stream. Input Span width and height must be exactly 4.
		/// </summary>
		/// <param name="inputBlock">Input 4x4 color block</param>
		/// <param name="outputStream">Output stream where the encoded block will be written to.</param>
		public void EncodeBlockHdr(ReadOnlySpan2D<ColorRgbFloat> inputBlock, Stream outputStream)
		{
			if (inputBlock.Width != 4 || inputBlock.Height != 4)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}
			EncodeBlockInternalHdr(inputBlock, outputStream);
		}

		#endregion

		#endregion
		#region MipMap operations

		/// <summary>
		/// Calculates the number of mipmap levels that will be generated for the given input image.
		/// </summary>
		/// <param name="imagePixelWidth">The width of the input image in pixels</param>
		/// <param name="imagePixelHeight">The height of the input image in pixels</param>
		/// <returns>The number of mipmap levels that will be generated for the input image.</returns>
		public int CalculateNumberOfMipLevels(int imagePixelWidth, int imagePixelHeight)
		{
			return MipMapper.CalculateMipChainLength(imagePixelWidth, imagePixelHeight,
				OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1);
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

		#region HDR

		private void EncodeToStreamInternalHdr(ReadOnlyMemory2D<ColorRgbFloat> input, Stream outputStream, CancellationToken token)
		{
			switch (OutputOptions.FileFormat)
			{
				case OutputFileFormat.Dds:
					var dds = EncodeToDdsInternalHdr(input, token);
					dds.Write(outputStream);
					break;

				case OutputFileFormat.Ktx:
					var ktx = EncodeToKtxInternalHdr(input, token);
					ktx.Write(outputStream);
					break;
			}
		}

		private KtxFile EncodeToKtxInternalHdr(ReadOnlyMemory2D<ColorRgbFloat> input, CancellationToken token)
		{
			KtxFile output;
			IBcBlockEncoder<RawBlock4X4RgbFloat> compressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(input, ref numMipMaps);

			// Setup encoders
			if (!OutputOptions.Format.IsHdrFormat())
			{
				throw new NotSupportedException($"This Format is not supported for hdr images: {OutputOptions.Format}");
			}
			compressedEncoder = GetFloatBlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
			}

			output = new KtxFile(
				KtxHeader.InitializeCompressed(input.Width, input.Height,
					compressedEncoder.GetInternalFormat(),
					compressedEncoder.GetBaseInternalFormat()));

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var totalBlocks = mipChain.Sum(m => ImageToBlocks.CalculateNumOfBlocks(m.Width, m.Height));
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// Encode mipmap levels
			for (var mip = 0; mip < numMipMaps; mip++)
			{
				byte[] encoded;
				var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip], out var blocksWidth,
					out var blocksHeight);
				encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality,
					context);

				context.Progress.SetProcessedBlocks(mipChain.Take(mip + 1).Sum(x => ImageToBlocks.CalculateNumOfBlocks(x.Width, x.Height)));


				output.MipMaps.Add(new KtxMipmap((uint)encoded.Length,
					(uint)mipChain[mip].Width,
					(uint)mipChain[mip].Height, 1));
				output.MipMaps[mip].Faces[0] = new KtxMipFace(encoded,
					(uint)mipChain[mip].Width,
					(uint)mipChain[mip].Height);
			}

			output.header.NumberOfFaces = 1;
			output.header.NumberOfMipmapLevels = (uint)numMipMaps;

			return output;
		}

		private DdsFile EncodeToDdsInternalHdr(ReadOnlyMemory2D<ColorRgbFloat> input, CancellationToken token)
		{
			DdsFile output;
			IBcBlockEncoder<RawBlock4X4RgbFloat> compressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(input, ref numMipMaps);

			// Setup encoder
			if (!OutputOptions.Format.IsHdrFormat())
			{
				throw new NotSupportedException($"This Format is not supported for hdr images: {OutputOptions.Format}");
			}
			compressedEncoder = GetFloatBlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
			}

			var (ddsHeader, dxt10Header) = DdsHeader.InitializeCompressed(input.Width, input.Height,
				compressedEncoder.GetDxgiFormat(), OutputOptions.DdsPreferDxt10Header);
			output = new DdsFile(ddsHeader, dxt10Header);

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var totalBlocks = mipChain.Sum(m => ImageToBlocks.CalculateNumOfBlocks(m.Width, m.Height));
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// Encode mipmap levels
			for (var mip = 0; mip < numMipMaps; mip++)
			{
				byte[] encoded;

				var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip], out var blocksWidth, out var blocksHeight);
				encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);

				context.Progress.SetProcessedBlocks(mipChain.Take(mip + 1).Sum(x => ImageToBlocks.CalculateNumOfBlocks(x.Width, x.Height)));


				if (mip == 0)
				{
					output.Faces.Add(new DdsFace((uint)input.Width, (uint)input.Height,
						(uint)encoded.Length, numMipMaps));
				}

				output.Faces[0].MipMaps[mip] = new DdsMipMap(encoded,
					(uint)mipChain[mip].Width,
					(uint)mipChain[mip].Height);
			}


			output.header.dwMipMapCount = (uint)numMipMaps;
			if (numMipMaps > 1)
			{
				output.header.dwCaps |= HeaderCaps.DdscapsComplex | HeaderCaps.DdscapsMipmap;
			}

			return output;
		}

		private byte[][] EncodeToRawInternalHdr(ReadOnlyMemory2D<ColorRgbFloat> input, CancellationToken token)
		{
			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(input, ref numMipMaps);

			var output = new byte[numMipMaps][];
			IBcBlockEncoder<RawBlock4X4RgbFloat> compressedEncoder = null;

			// Setup encoder

			compressedEncoder = GetFloatBlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var totalBlocks = mipChain.Sum(m => ImageToBlocks.CalculateNumOfBlocks(m.Width, m.Height));
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// Encode all mipmap levels
			for (var mip = 0; mip < numMipMaps; mip++)
			{
				byte[] encoded;

				var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip], out var blocksWidth, out var blocksHeight);
				encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);

				context.Progress.SetProcessedBlocks(mipChain.Take(mip + 1).Sum(x => ImageToBlocks.CalculateNumOfBlocks(x.Width, x.Height)));

				output[mip] = encoded;
			}

			return output;
		}

		private byte[] EncodeToRawInternalHdr(ReadOnlyMemory2D<ColorRgbFloat> input, int mipLevel, out int mipWidth, out int mipHeight, CancellationToken token)
		{
			mipLevel = Math.Max(0, mipLevel);

			IBcBlockEncoder<RawBlock4X4RgbFloat> compressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(input, ref numMipMaps);

			// Setup encoder

			compressedEncoder = GetFloatBlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
			}

			// Dispose all mipmap levels
			if (mipLevel > numMipMaps - 1)
			{
				throw new ArgumentException($"{nameof(mipLevel)} cannot be more than number of mipmaps.");
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var totalBlocks = mipChain.Sum(m => ImageToBlocks.CalculateNumOfBlocks(m.Width, m.Height));
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// Encode mipmap level
			byte[] encoded;

			var blocks = ImageToBlocks.ImageTo4X4(mipChain[mipLevel], out var blocksWidth, out var blocksHeight);
			encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);

			mipWidth = mipChain[mipLevel].Width;
			mipHeight = mipChain[mipLevel].Height;

			return encoded;
		}

		private void EncodeCubeMapToStreamInternalHdr(ReadOnlyMemory2D<ColorRgbFloat> right, ReadOnlyMemory2D<ColorRgbFloat> left, ReadOnlyMemory2D<ColorRgbFloat> top, ReadOnlyMemory2D<ColorRgbFloat> down,
			ReadOnlyMemory2D<ColorRgbFloat> back, ReadOnlyMemory2D<ColorRgbFloat> front, Stream outputStream, CancellationToken token)
		{
			switch (OutputOptions.FileFormat)
			{
				case OutputFileFormat.Ktx:
					var ktx = EncodeCubeMapToKtxInternalHdr(right, left, top, down, back, front, token);
					ktx.Write(outputStream);
					break;

				case OutputFileFormat.Dds:
					var dds = EncodeCubeMapToDdsInternalHdr(right, left, top, down, back, front, token);
					dds.Write(outputStream);
					break;
			}
		}

		private KtxFile EncodeCubeMapToKtxInternalHdr(ReadOnlyMemory2D<ColorRgbFloat> right, ReadOnlyMemory2D<ColorRgbFloat> left, ReadOnlyMemory2D<ColorRgbFloat> top, ReadOnlyMemory2D<ColorRgbFloat> down,
			ReadOnlyMemory2D<ColorRgbFloat> back, ReadOnlyMemory2D<ColorRgbFloat> front, CancellationToken token)
		{
			KtxFile output;
			IBcBlockEncoder<RawBlock4X4RgbFloat> compressedEncoder = null;

			var faces = new[] { right, left, top, down, back, front };

			var width = right.Width;
			var height = right.Height;

			// Setup encoder
			compressedEncoder = GetFloatBlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
			}

			output = new KtxFile(
				KtxHeader.InitializeCompressed(width, height,
					compressedEncoder.GetInternalFormat(),
					compressedEncoder.GetBaseInternalFormat()));

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipLength = MipMapper.CalculateMipChainLength(width, height, numMipMaps);
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

			// Calculate total blocks
			var totalBlocks = 0;
			foreach (var face in faces)
			{
				for (var mip = 0; mip < numMipMaps; mip++)
				{
					MipMapper.CalculateMipLevelSize(width, height, mip, out var mipWidth, out var mipHeight);
					totalBlocks += ImageToBlocks.CalculateNumOfBlocks(mipWidth, mipHeight);
				}
			}
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// Encode all faces
			var processedBlocks = 0;
			for (var face = 0; face < faces.Length; face++)
			{
				var mipChain = MipMapper.GenerateMipChain(faces[face], ref numMipMaps);

				// Encode all mipmap levels per face
				for (var mipLevel = 0; mipLevel < numMipMaps; mipLevel++)
				{
					byte[] encoded;
					var blocks = ImageToBlocks.ImageTo4X4(mipChain[mipLevel], out var blocksWidth, out var blocksHeight);
					encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);

					processedBlocks += blocks.Length;
					context.Progress.SetProcessedBlocks(processedBlocks);

					if (face == 0)
					{
						output.MipMaps[mipLevel] = new KtxMipmap((uint)encoded.Length,
							(uint)mipChain[mipLevel].Width,
							(uint)mipChain[mipLevel].Height, (uint)faces.Length);
					}

					output.MipMaps[mipLevel].Faces[face] = new KtxMipFace(encoded,
						(uint)mipChain[mipLevel].Width,
						(uint)mipChain[mipLevel].Height);
				}
			}

			output.header.NumberOfFaces = (uint)faces.Length;
			output.header.NumberOfMipmapLevels = (uint)mipLength;

			return output;
		}

		private DdsFile EncodeCubeMapToDdsInternalHdr(ReadOnlyMemory2D<ColorRgbFloat> right, ReadOnlyMemory2D<ColorRgbFloat> left, ReadOnlyMemory2D<ColorRgbFloat> top, ReadOnlyMemory2D<ColorRgbFloat> down,
			ReadOnlyMemory2D<ColorRgbFloat> back, ReadOnlyMemory2D<ColorRgbFloat> front, CancellationToken token)
		{
			DdsFile output;
			IBcBlockEncoder<RawBlock4X4RgbFloat> compressedEncoder = null;
			
			var faces = new[] { right, left, top, down, back, front };

			var width = right.Width;
			var height = right.Height;

			// Setup encoder
			compressedEncoder = GetFloatBlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
			}

			var (ddsHeader, dxt10Header) = DdsHeader.InitializeCompressed(width, height,
				compressedEncoder.GetDxgiFormat(), OutputOptions.DdsPreferDxt10Header);
			output = new DdsFile(ddsHeader, dxt10Header);

			if (OutputOptions.DdsBc1WriteAlphaFlag &&
			    OutputOptions.Format == CompressionFormat.Bc1WithAlpha)
			{
				output.header.ddsPixelFormat.dwFlags |= PixelFormatFlags.DdpfAlphaPixels;
			}

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var totalBlocks = 0;
			foreach (var face in faces)
			{
				for (var mip = 0; mip < numMipMaps; mip++)
				{
					MipMapper.CalculateMipLevelSize(width, height, mip, out var mipWidth, out var mipHeight);
					totalBlocks += ImageToBlocks.CalculateNumOfBlocks(mipWidth, mipHeight);
				}
			}
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// EncodeBlock all faces
			var processedBlocks = 0;
			for (var face = 0; face < faces.Length; face++)
			{
				var mipChain = MipMapper.GenerateMipChain(faces[face], ref numMipMaps);

				// Encode all mipmap levels per face
				for (var mip = 0; mip < numMipMaps; mip++)
				{
					byte[] encoded;
					
					var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip], out var blocksWidth, out var blocksHeight);
					encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);

					processedBlocks += blocks.Length;
					context.Progress.SetProcessedBlocks(processedBlocks);

					if (mip == 0)
					{
						output.Faces.Add(new DdsFace((uint)mipChain[mip].Width, (uint)mipChain[mip].Height,
							(uint)encoded.Length, mipChain.Length));
					}

					output.Faces[face].MipMaps[mip] = new DdsMipMap(encoded,
						(uint)mipChain[mip].Width,
						(uint)mipChain[mip].Height);
				}
			}

			output.header.dwCaps |= HeaderCaps.DdscapsComplex;
			output.header.dwMipMapCount = (uint)numMipMaps;
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

		private byte[] EncodeBlockInternalHdr(ReadOnlySpan2D<ColorRgbFloat> input)
		{
			var compressedEncoder = GetFloatBlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported for single block encoding: {OutputOptions.Format}");
			}

			var output = new byte[compressedEncoder.GetBlockSize()];

			var rawBlock = new RawBlock4X4RgbFloat();

			var pixels = rawBlock.AsSpan;

			input.GetRowSpan(0).CopyTo(pixels);
			input.GetRowSpan(1).CopyTo(pixels.Slice(4));
			input.GetRowSpan(2).CopyTo(pixels.Slice(8));
			input.GetRowSpan(3).CopyTo(pixels.Slice(12));

			compressedEncoder.EncodeBlock(rawBlock, OutputOptions.Quality, output);

			return output;
		}

		private void EncodeBlockInternalHdr(ReadOnlySpan2D<ColorRgbFloat> input, Stream outputStream)
		{
			var compressedEncoder = GetFloatBlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported for single block encoding: {OutputOptions.Format}");
			}
			if (input.Width != 4 || input.Height != 4)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}

			Span<byte> output = stackalloc byte[16];
			output = output.Slice(0, compressedEncoder.GetBlockSize());

			var rawBlock = new RawBlock4X4RgbFloat();

			var pixels = rawBlock.AsSpan;

			input.GetRowSpan(0).CopyTo(pixels);
			input.GetRowSpan(1).CopyTo(pixels.Slice(4));
			input.GetRowSpan(2).CopyTo(pixels.Slice(8));
			input.GetRowSpan(3).CopyTo(pixels.Slice(12));

			compressedEncoder.EncodeBlock(rawBlock, OutputOptions.Quality, output);

			outputStream.Write(output);
		}

		#endregion

		#region LDR
		private void EncodeToStreamInternal(ReadOnlyMemory2D<ColorRgba32> input, Stream outputStream, CancellationToken token)
		{
			switch (OutputOptions.FileFormat)
			{
				case OutputFileFormat.Dds:
					var dds = EncodeToDdsInternal(input, token);
					dds.Write(outputStream);
					break;

				case OutputFileFormat.Ktx:
					var ktx = EncodeToKtxInternal(input, token);
					ktx.Write(outputStream);
					break;
			}
		}

		private KtxFile EncodeToKtxInternal(ReadOnlyMemory2D<ColorRgba32> input, CancellationToken token)
		{
			KtxFile output;
			IBcBlockEncoder<RawBlock4X4Rgba32> compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(input, ref numMipMaps);

			// Setup encoders
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}

				output = new KtxFile(
					KtxHeader.InitializeCompressed(input.Width, input.Height,
						compressedEncoder.GetInternalFormat(),
						compressedEncoder.GetBaseInternalFormat()));
			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.Format);
				output = new KtxFile(
					KtxHeader.InitializeUncompressed(input.Width, input.Height,
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

			// Calculate total blocks
			var totalBlocks = isCompressedFormat ? mipChain.Sum(m => ImageToBlocks.CalculateNumOfBlocks(m.Width, m.Height)) : mipChain.Sum(m => m.Width * m.Height);
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// Encode mipmap levels
			for (var mip = 0; mip < numMipMaps; mip++)
			{
				byte[] encoded;
				if (isCompressedFormat)
				{
					var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip], out var blocksWidth,
						out var blocksHeight);
					encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality,
						context);

					context.Progress.SetProcessedBlocks(mipChain.Take(mip + 1).Sum(x => ImageToBlocks.CalculateNumOfBlocks(x.Width, x.Height)));
				}
				else
				{
					if (!mipChain[mip].TryGetMemory(out var mipMemory))
					{
						throw new InvalidOperationException("Could not get Memory<T> from Memory2D<T>.");
					}

					encoded = uncompressedEncoder.Encode(mipMemory);

					context.Progress.SetProcessedBlocks(mipChain.Take(mip + 1).Sum(x => x.Width * x.Height));
				}

				output.MipMaps.Add(new KtxMipmap((uint)encoded.Length,
					(uint)mipChain[mip].Width,
					(uint)mipChain[mip].Height, 1));
				output.MipMaps[mip].Faces[0] = new KtxMipFace(encoded,
					(uint)mipChain[mip].Width,
					(uint)mipChain[mip].Height);
			}

			output.header.NumberOfFaces = 1;
			output.header.NumberOfMipmapLevels = (uint)numMipMaps;

			return output;
		}

		private DdsFile EncodeToDdsInternal(ReadOnlyMemory2D<ColorRgba32> input, CancellationToken token)
		{
			DdsFile output;
			IBcBlockEncoder<RawBlock4X4Rgba32> compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(input, ref numMipMaps);

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}

				var (ddsHeader, dxt10Header) = DdsHeader.InitializeCompressed(input.Width, input.Height,
					compressedEncoder.GetDxgiFormat(), OutputOptions.DdsPreferDxt10Header);
				output = new DdsFile(ddsHeader, dxt10Header);

				if (OutputOptions.DdsBc1WriteAlphaFlag &&
					OutputOptions.Format == CompressionFormat.Bc1WithAlpha)
				{
					output.header.ddsPixelFormat.dwFlags |= PixelFormatFlags.DdpfAlphaPixels;
				}
			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.Format);
				var ddsHeader = DdsHeader.InitializeUncompressed(input.Width, input.Height,
					uncompressedEncoder.GetDxgiFormat());
				output = new DdsFile(ddsHeader);
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var totalBlocks = isCompressedFormat ? mipChain.Sum(m => ImageToBlocks.CalculateNumOfBlocks(m.Width, m.Height)) : mipChain.Sum(m => m.Width * m.Height);
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// Encode mipmap levels
			for (var mip = 0; mip < numMipMaps; mip++)
			{
				byte[] encoded;
				if (isCompressedFormat)
				{
					var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip], out var blocksWidth, out var blocksHeight);
					encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);

					context.Progress.SetProcessedBlocks(mipChain.Take(mip + 1).Sum(x => ImageToBlocks.CalculateNumOfBlocks(x.Width, x.Height)));
				}
				else
				{
					if (!mipChain[mip].TryGetMemory(out var mipMemory))
					{
						throw new InvalidOperationException("Could not get Memory<T> from Memory2D<T>.");
					}

					encoded = uncompressedEncoder.Encode(mipMemory);

					context.Progress.SetProcessedBlocks(mipChain.Take(mip + 1).Sum(x => x.Width * x.Height));
				}

				if (mip == 0)
				{
					output.Faces.Add(new DdsFace((uint)input.Width, (uint)input.Height,
						(uint)encoded.Length, numMipMaps));
				}

				output.Faces[0].MipMaps[mip] = new DdsMipMap(encoded,
					(uint)mipChain[mip].Width,
					(uint)mipChain[mip].Height);
			}


			output.header.dwMipMapCount = (uint)numMipMaps;
			if (numMipMaps > 1)
			{
				output.header.dwCaps |= HeaderCaps.DdscapsComplex | HeaderCaps.DdscapsMipmap;
			}

			return output;
		}

		private byte[][] EncodeToRawInternal(ReadOnlyMemory2D<ColorRgba32> input, CancellationToken token)
		{
			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(input, ref numMipMaps);

			var output = new byte[numMipMaps][];
			IBcBlockEncoder<RawBlock4X4Rgba32> compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();

			if (isCompressedFormat)
			{
				compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
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

			// Calculate total blocks
			var totalBlocks = isCompressedFormat ? mipChain.Sum(m => ImageToBlocks.CalculateNumOfBlocks(m.Width, m.Height)) : mipChain.Sum(m => m.Width * m.Height);
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// Encode all mipmap levels
			for (var mip = 0; mip < numMipMaps; mip++)
			{
				byte[] encoded;
				if (isCompressedFormat)
				{
					var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip], out var blocksWidth, out var blocksHeight);
					encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);

					context.Progress.SetProcessedBlocks(mipChain.Take(mip + 1).Sum(x => ImageToBlocks.CalculateNumOfBlocks(x.Width, x.Height)));
				}
				else
				{
					if (!mipChain[mip].TryGetMemory(out var mipMemory))
					{
						throw new InvalidOperationException("Could not get Memory<T> from Memory2D<T>.");
					}

					encoded = uncompressedEncoder.Encode(mipMemory);

					context.Progress.SetProcessedBlocks(mipChain.Take(mip + 1).Sum(x => x.Width * x.Height));
				}

				output[mip] = encoded;
			}

			return output;
		}

		private byte[] EncodeToRawInternal(ReadOnlyMemory2D<ColorRgba32> input, int mipLevel, out int mipWidth, out int mipHeight, CancellationToken token)
		{
			mipLevel = Math.Max(0, mipLevel);

			IBcBlockEncoder<RawBlock4X4Rgba32> compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(input, ref numMipMaps);

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
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
				throw new ArgumentException($"{nameof(mipLevel)} cannot be more than number of mipmaps.");
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var totalBlocks = isCompressedFormat ? ImageToBlocks.CalculateNumOfBlocks(mipChain[mipLevel].Width, mipChain[mipLevel].Height) : mipChain[mipLevel].Width * mipChain[mipLevel].Height;
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// Encode mipmap level
			byte[] encoded;
			if (isCompressedFormat)
			{
				var blocks = ImageToBlocks.ImageTo4X4(mipChain[mipLevel], out var blocksWidth, out var blocksHeight);
				encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);
			}
			else
			{
				if (!mipChain[mipLevel].TryGetMemory(out var mipMemory))
				{
					throw new InvalidOperationException("Could not get Memory<T> from Memory2D<T>.");
				}

				encoded = uncompressedEncoder.Encode(mipMemory);
			}

			mipWidth = mipChain[mipLevel].Width;
			mipHeight = mipChain[mipLevel].Height;

			return encoded;
		}

		private void EncodeCubeMapToStreamInternal(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left, ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front, Stream outputStream, CancellationToken token)
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

		private KtxFile EncodeCubeMapToKtxInternal(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left, ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front, CancellationToken token)
		{
			KtxFile output;
			IBcBlockEncoder<RawBlock4X4Rgba32> compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var faces = new[] { right, left, top, down, back, front };

			var width = right.Width;
			var height = right.Height;

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}

				output = new KtxFile(
					KtxHeader.InitializeCompressed(width, height,
						compressedEncoder.GetInternalFormat(),
						compressedEncoder.GetBaseInternalFormat()));
			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.Format);
				output = new KtxFile(
					KtxHeader.InitializeUncompressed(width, height,
						uncompressedEncoder.GetGlType(),
						uncompressedEncoder.GetGlFormat(),
						uncompressedEncoder.GetGlTypeSize(),
						uncompressedEncoder.GetInternalFormat(),
						uncompressedEncoder.GetBaseInternalFormat()));

			}

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipLength = MipMapper.CalculateMipChainLength(width, height, numMipMaps);
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

			// Calculate total blocks
			var totalBlocks = 0;
			foreach (var face in faces)
			{
				for (var mip = 0; mip < numMipMaps; mip++)
				{
					MipMapper.CalculateMipLevelSize(width, height, mip, out var mipWidth, out var mipHeight);
					totalBlocks += isCompressedFormat ? ImageToBlocks.CalculateNumOfBlocks(mipWidth, mipHeight) : mipWidth * mipHeight;
				}
			}
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// Encode all faces
			var processedBlocks = 0;
			for (var face = 0; face < faces.Length; face++)
			{
				var mipChain = MipMapper.GenerateMipChain(faces[face], ref numMipMaps);

				// Encode all mipmap levels per face
				for (var mipLevel = 0; mipLevel < numMipMaps; mipLevel++)
				{
					byte[] encoded;
					if (isCompressedFormat)
					{
						var blocks = ImageToBlocks.ImageTo4X4(mipChain[mipLevel], out var blocksWidth, out var blocksHeight);
						encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);

						processedBlocks += blocks.Length;
						context.Progress.SetProcessedBlocks(processedBlocks);
					}
					else
					{
						if (!mipChain[mipLevel].TryGetMemory(out var mipMemory))
						{
							throw new InvalidOperationException("Could not get Memory<T> from Memory2D<T>.");
						}

						encoded = uncompressedEncoder.Encode(mipMemory);

						processedBlocks += mipMemory.Length;
						context.Progress.SetProcessedBlocks(processedBlocks);
					}

					if (face == 0)
					{
						output.MipMaps[mipLevel] = new KtxMipmap((uint)encoded.Length,
							(uint)mipChain[mipLevel].Width,
							(uint)mipChain[mipLevel].Height, (uint)faces.Length);
					}

					output.MipMaps[mipLevel].Faces[face] = new KtxMipFace(encoded,
						(uint)mipChain[mipLevel].Width,
						(uint)mipChain[mipLevel].Height);
				}
			}

			output.header.NumberOfFaces = (uint)faces.Length;
			output.header.NumberOfMipmapLevels = (uint)mipLength;

			return output;
		}

		private DdsFile EncodeCubeMapToDdsInternal(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left, ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front, CancellationToken token)
		{
			DdsFile output;
			IBcBlockEncoder<RawBlock4X4Rgba32> compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var faces = new[] { right, left, top, down, back, front };

			var width = right.Width;
			var height = right.Height;

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}

				var (ddsHeader, dxt10Header) = DdsHeader.InitializeCompressed(width, height,
					compressedEncoder.GetDxgiFormat(), OutputOptions.DdsPreferDxt10Header);
				output = new DdsFile(ddsHeader, dxt10Header);

				if (OutputOptions.DdsBc1WriteAlphaFlag &&
					OutputOptions.Format == CompressionFormat.Bc1WithAlpha)
				{
					output.header.ddsPixelFormat.dwFlags |= PixelFormatFlags.DdpfAlphaPixels;
				}
			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.Format);
				var ddsHeader = DdsHeader.InitializeUncompressed(width, height,
					uncompressedEncoder.GetDxgiFormat());

				output = new DdsFile(ddsHeader);
			}

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var totalBlocks = 0;
			foreach (var face in faces)
			{
				for (var mip = 0; mip < numMipMaps; mip++)
				{
					MipMapper.CalculateMipLevelSize(width, height, mip, out var mipWidth, out var mipHeight);
					totalBlocks += isCompressedFormat ? ImageToBlocks.CalculateNumOfBlocks(mipWidth, mipHeight) : mipWidth * mipHeight;
				}
			}
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// EncodeBlock all faces
			var processedBlocks = 0;
			for (var face = 0; face < faces.Length; face++)
			{
				var mipChain = MipMapper.GenerateMipChain(faces[face], ref numMipMaps);

				// Encode all mipmap levels per face
				for (var mip = 0; mip < numMipMaps; mip++)
				{
					byte[] encoded;
					if (isCompressedFormat)
					{
						var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip], out var blocksWidth, out var blocksHeight);
						encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);

						processedBlocks += blocks.Length;
						context.Progress.SetProcessedBlocks(processedBlocks);
					}
					else
					{
						if (!mipChain[mip].TryGetMemory(out var mipMemory))
						{
							throw new InvalidOperationException("Could not get Memory<T> from Memory2D<T>.");
						}

						encoded = uncompressedEncoder.Encode(mipMemory);

						processedBlocks += mipMemory.Length;
						context.Progress.SetProcessedBlocks(processedBlocks);
					}

					if (mip == 0)
					{
						output.Faces.Add(new DdsFace((uint)mipChain[mip].Width, (uint)mipChain[mip].Height,
							(uint)encoded.Length, mipChain.Length));
					}

					output.Faces[face].MipMaps[mip] = new DdsMipMap(encoded,
						(uint)mipChain[mip].Width,
						(uint)mipChain[mip].Height);
				}
			}

			output.header.dwCaps |= HeaderCaps.DdscapsComplex;
			output.header.dwMipMapCount = (uint)numMipMaps;
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

		private byte[] EncodeBlockInternal(ReadOnlySpan2D<ColorRgba32> input)
		{
			var compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported for single block encoding: {OutputOptions.Format}");
			}

			var output = new byte[compressedEncoder.GetBlockSize()];

			var rawBlock = new RawBlock4X4Rgba32();

			var pixels = rawBlock.AsSpan;

			input.GetRowSpan(0).CopyTo(pixels);
			input.GetRowSpan(1).CopyTo(pixels.Slice(4));
			input.GetRowSpan(2).CopyTo(pixels.Slice(8));
			input.GetRowSpan(3).CopyTo(pixels.Slice(12));

			compressedEncoder.EncodeBlock(rawBlock, OutputOptions.Quality, output);

			return output;
		}

		private void EncodeBlockInternal(ReadOnlySpan2D<ColorRgba32> input, Stream outputStream)
		{
			var compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
			if (compressedEncoder == null)
			{
				throw new NotSupportedException($"This Format is not supported for single block encoding: {OutputOptions.Format}");
			}
			if (input.Width != 4 || input.Height != 4)
			{
				throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
			}

			Span<byte> output = stackalloc byte[16];
			output = output.Slice(0, compressedEncoder.GetBlockSize());

			var rawBlock = new RawBlock4X4Rgba32();

			var pixels = rawBlock.AsSpan;

			input.GetRowSpan(0).CopyTo(pixels);
			input.GetRowSpan(1).CopyTo(pixels.Slice(4));
			input.GetRowSpan(2).CopyTo(pixels.Slice(8));
			input.GetRowSpan(3).CopyTo(pixels.Slice(12));

			compressedEncoder.EncodeBlock(rawBlock, OutputOptions.Quality, output);

			outputStream.Write(output);
		}

		#endregion

		#endregion

		#region Support

		private IBcBlockEncoder<RawBlock4X4Rgba32> GetRgba32BlockEncoder(CompressionFormat format)
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
					return new Bc4BlockEncoder(InputOptions.Bc4Component);

				case CompressionFormat.Bc5:
					return new Bc5BlockEncoder(InputOptions.Bc5Component1, InputOptions.Bc5Component2);

				case CompressionFormat.Bc7:
					return new Bc7Encoder();

				case CompressionFormat.Atc:
					return new AtcBlockEncoder();

				case CompressionFormat.AtcExplicitAlpha:
					return new AtcExplicitAlphaBlockEncoder();

				case CompressionFormat.AtcInterpolatedAlpha:
					return new AtcInterpolatedAlphaBlockEncoder();

				default:
					return null;
			}
		}

		private IBcBlockEncoder<RawBlock4X4RgbFloat> GetFloatBlockEncoder(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.Bc6S:
					return new Bc6Encoder(true);
				case CompressionFormat.Bc6U:
					return new Bc6Encoder(false);
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

				case CompressionFormat.Bgra:
					return new RawBgraEncoder();

				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		private ReadOnlyMemory2D<ColorRgba32> ByteToColorMemory(ReadOnlySpan<byte> span, int width, int height, PixelFormat format)
		{
			var pixels = new ColorRgba32[width * height];

			switch (format)
			{
				case PixelFormat.Rgba32:
					for (var i = 0; i < width * height * 4; i += 4)
						pixels[i / 4] = new ColorRgba32(span[i], span[i + 1], span[i + 2], span[i + 3]);
					break;

				case PixelFormat.Rgb24:
					for (var i = 0; i < width * height * 3; i += 3)
						pixels[i / 3] = new ColorRgba32(span[i], span[i + 1], span[i + 2], 255);
					break;

				case PixelFormat.Bgra32:
					for (var i = 0; i < width * height * 4; i += 4)
						pixels[i / 4] = new ColorRgba32(span[i + 2], span[i + 1], span[i], span[i + 3]);
					break;

				case PixelFormat.Bgr24:
					for (var i = 0; i < width * height * 3; i += 3)
						pixels[i / 3] = new ColorRgba32(span[i + 2], span[i + 1], span[i], 255);
					break;

				case PixelFormat.Argb32:
					for (var i = 0; i < width * height * 4; i += 4)
						pixels[i / 4] = new ColorRgba32(span[i + 1], span[i + 2], span[i + 3], span[i]);
					break;
			}

			return new ReadOnlyMemory2D<ColorRgba32>(pixels, height, width);
		}

		#endregion
	}
}
