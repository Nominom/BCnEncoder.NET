using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Encoder.Bptc;
using BCnEncoder.Encoder.Options;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using CommunityToolkit.HighPerformance;

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

		#region LDR

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode. This can be in any supported format.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public BCnTextureData Encode(BCnTextureData input)
		{
			return EncodeInternal(input, default);
		}

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode. This can be in any supported format.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public Task<BCnTextureData> EncodeAsync(BCnTextureData input, CancellationToken token = default)
		{
			return Task.Run(() => EncodeInternal(input, token), token);
		}

		/// <summary>
		/// Encode a cubemap as raw bytes into a <see cref="BCnTextureData"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public BCnTextureData EncodeCubeMap(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front)
		{
			var inputData = new BCnTextureData(CompressionFormat.Rgba32, right.Width, right.Height, 1, 1, 1, true, false, AlphaChannelHint.Unknown);

			if (
				right.Width != left.Width || right.Height != left.Height ||
				right.Width != top.Width || right.Height != top.Height ||
				right.Width != down.Width || right.Height != down.Height ||
				right.Width != back.Width || right.Height != back.Height ||
				right.Width != front.Width || right.Height != front.Height
			)
			{
				throw new ArgumentException("All faces of a cubeMap must be of equal width and height!");
			}

			inputData.Mips[0][CubeMapFaceDirection.XPositive].Data = right.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.XNegative].Data = left.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.YPositive].Data = top.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.YNegative].Data = down.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.ZPositive].Data = back.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.ZNegative].Data = front.CopyAsBytes();

			return EncodeInternal(inputData, default);
		}

		/// <summary>
		/// Encode a cubemap as raw bytes into a <see cref="BCnTextureData"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public Task<BCnTextureData> EncodeCubeMapAsync(ReadOnlyMemory2D<ColorRgba32> right, ReadOnlyMemory2D<ColorRgba32> left,
			ReadOnlyMemory2D<ColorRgba32> top, ReadOnlyMemory2D<ColorRgba32> down,
			ReadOnlyMemory2D<ColorRgba32> back, ReadOnlyMemory2D<ColorRgba32> front, CancellationToken token = default)
		{
			var inputData = new BCnTextureData(CompressionFormat.Rgba32, right.Width, right.Height, 1, 1, 1, true, false, AlphaChannelHint.Unknown);

			if (
				right.Width != left.Width || right.Height != left.Height ||
				right.Width != top.Width || right.Height != top.Height ||
				right.Width != down.Width || right.Height != down.Height ||
				right.Width != back.Width || right.Height != back.Height ||
				right.Width != front.Width || right.Height != front.Height
			)
			{
				throw new ArgumentException("All faces of a cubeMap must be of equal width and height!");
			}

			inputData.Mips[0][CubeMapFaceDirection.XPositive].Data = right.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.XNegative].Data = left.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.YPositive].Data = top.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.YNegative].Data = down.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.ZPositive].Data = back.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.ZNegative].Data = front.CopyAsBytes();

			return Task.Run(() => EncodeInternal(inputData, token), token);
		}

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public BCnTextureData Encode(ReadOnlyMemory2D<ColorRgba32> input)
		{
			var inputData = BCnTextureData.FromSingle(CompressionFormat.Rgba32, input.Width, input.Height, input.CopyAsBytes(), AlphaChannelHint.Unknown);
			return EncodeInternal(inputData, default);
		}

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public Task<BCnTextureData> EncodeAsync(ReadOnlyMemory2D<ColorRgba32> input, CancellationToken token = default)
		{
			var inputData = BCnTextureData.FromSingle(CompressionFormat.Rgba32, input.Width, input.Height, input.CopyAsBytes(), AlphaChannelHint.Unknown);
			return Task.Run(() => EncodeInternal(inputData, token), token);
		}

		// /// <summary>
		// /// Encodes a single mip level of the input image to a byte buffer asynchronously. This data does not contain any file headers, just the raw encoded pixel data.
		// /// </summary>
		// /// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		// /// <param name="mipLevel">The mipmap to encode.</param>
		// /// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		// /// <returns>The raw encoded input.</returns>
		// /// <remarks>To get the width and height of the encoded mip level, see <see cref="CalculateMipMapSize"/>.</remarks>
		// public Task<byte[]> EncodeToRawBytesAsync(ReadOnlyMemory2D<ColorRgba32> input, int mipLevel, CancellationToken token = default)
		// {
		// 	return Task.Run(() => EncodeSingleLdrInternal(input.Flatten(), input.Width, input.Height, mipLevel, token), token);
		// }
		//

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer asynchronously. This data does not contain any file headers, just the raw encoded pixel data.
		/// Note that even if the input data already contains mipLevels, new mips are generated from the first mip.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="BCnTextureData"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		/// <returns>The raw encoded input.</returns>
		/// <remarks>To get the width and height of the encoded mip level, see <see cref="CalculateMipMapSize"/>.</remarks>
		public async Task<byte[]> EncodeToRawBytesAsync(BCnTextureData input, int mipLevel, CancellationToken token = default)
		{
			CalculateMipMapSize(input.Width, input.Height, 1, mipLevel, out int mipWidth, out int mipHeight, out _);

			var output = AllocateOutputBuffer(mipWidth, mipHeight, 1);

			await Task.Run(() => EncodeSingleInternal(input.First.Data, output, input.Format, input.Width, input.Height, 1, mipLevel, token), token);

			return output;
		}

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode. This can be in any supported format.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="inputFormat">The pixel format the input data is in.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public Task<BCnTextureData> EncodeBytesAsync(byte[] input, int width, int height, CompressionFormat inputFormat, CancellationToken token = default)
		{
			var inputData = BCnTextureData.FromSingle(inputFormat, width, height, input, AlphaChannelHint.Unknown);
			return Task.Run(() => EncodeInternal(inputData, token), token);
		}

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode. This can be in any supported format.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="inputFormat">The pixel format the input data is in.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public BCnTextureData EncodeBytes(byte[] input, int width, int height, CompressionFormat inputFormat)
		{
			var inputData = BCnTextureData.FromSingle(inputFormat, width, height, input, AlphaChannelHint.Unknown);
			return EncodeInternal(inputData, default);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		public byte[] EncodeToRawBytes<TIn>(ReadOnlyMemory<TIn> input, int width, int height, CompressionFormat inputFormat, int mipLevel, out int mipWidth, out int mipHeight)
			where TIn : unmanaged
		{
			CalculateMipMapSize(width, height, 1, mipLevel, out mipWidth, out mipHeight, out int mipDepth);

			var inputBytes = PrepareInputBuffer(input, width, height, 1, inputFormat);
			var output = AllocateOutputBuffer(mipWidth, mipHeight, mipDepth);

			EncodeSingleInternal(inputBytes, output, inputFormat, width, height, 1, mipLevel, default);

			return output;
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		/// Note that even if the input data already contains mipLevels, new mips are generated from the first mip.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="BCnTextureData"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="mipWidth">The width of the mipmap.</param>
		/// <param name="mipHeight">The height of the mipmap.</param>
		/// <param name="mipDepth">The depth of the mipmap.</param>
		/// <returns>A byte buffer containing the encoded data of the requested mip-level.</returns>
		public byte[] EncodeToRawBytes(BCnTextureData input, int mipLevel, out int mipWidth, out int mipHeight, out int mipDepth)
		{
			CalculateMipMapSize(input.Width, input.Height, input.Depth, mipLevel, out mipWidth, out mipHeight, out mipDepth);

			var output = AllocateOutputBuffer(mipWidth, mipHeight, mipDepth);

			EncodeSingleInternal(input.First.Data, output, input.Format, input.Width, input.Height, input.Depth, mipLevel, default);

			return output;
		}

		// /// <summary>
		// /// Encodes a single 4x4 block to raw encoded bytes. Input Span length must be exactly 16.
		// /// </summary>
		// /// <param name="inputBlock">Input 4x4 color block</param>
		// /// <returns>Raw encoded data</returns>
		// public byte[] EncodeBlock(ReadOnlySpan<ColorRgba32> inputBlock)
		// {
		// 	if (inputBlock.Length != 16)
		// 	{
		// 		throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
		// 	}
		// 	return EncodeBlockLdrInternal(inputBlock.AsSpan2D(4, 4));
		// }
		//
		// /// <summary>
		// /// Encodes a single 4x4 block to raw encoded bytes. Input Span width and height must be exactly 4.
		// /// </summary>
		// /// <param name="inputBlock">Input 4x4 color block</param>
		// /// <returns>Raw encoded data</returns>
		// public byte[] EncodeBlock(ReadOnlySpan2D<ColorRgba32> inputBlock)
		// {
		// 	if (inputBlock.Width != 4 || inputBlock.Height != 4)
		// 	{
		// 		throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
		// 	}
		// 	return EncodeBlockLdrInternal(inputBlock);
		// }
		//
		// /// <summary>
		// /// Encodes a single 4x4 block and writes the encoded block to a stream. Input Span length must be exactly 16.
		// /// </summary>
		// /// <param name="inputBlock">Input 4x4 color block</param>
		// /// <param name="outputStream">Output stream where the encoded block will be written to.</param>
		// public void EncodeBlock(ReadOnlySpan<ColorRgba32> inputBlock, Stream outputStream)
		// {
		// 	if (inputBlock.Length != 16)
		// 	{
		// 		throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
		// 	}
		// 	EncodeBlockLdrInternal(inputBlock.AsSpan2D(4, 4), outputStream);
		// }
		//
		// /// <summary>
		// /// Encodes a single 4x4 block and writes the encoded block to a stream. Input Span width and height must be exactly 4.
		// /// </summary>
		// /// <param name="inputBlock">Input 4x4 color block</param>
		// /// <param name="outputStream">Output stream where the encoded block will be written to.</param>
		// public void EncodeBlock(ReadOnlySpan2D<ColorRgba32> inputBlock, Stream outputStream)
		// {
		// 	if (inputBlock.Width != 4 || inputBlock.Height != 4)
		// 	{
		// 		throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
		// 	}
		// 	EncodeBlockLdrInternal(inputBlock, outputStream);
		// }
		/// <summary>
		/// Gets the block size of the currently selected compression format in bytes.
		/// </summary>
		/// <returns>The size of a single 4x4 block in bytes</returns>
		public int GetBlockSize()
		{
			return GetBlockEncoder(OutputOptions.Format).GetBlockSize();
		}

		/// <summary>
		/// Gets the number of total blocks in an image with the given pixel width and height.
		/// </summary>
		/// <param name="pixelWidth">The pixel width of the image</param>
		/// <param name="pixelHeight">The pixel height of the image</param>
		/// <returns>The total number of blocks.</returns>
		public int GetBlockCount(int pixelWidth, int pixelHeight)
		{
			return ImageToBlocks.CalculateNumOfBlocks(OutputOptions.Format, pixelWidth, pixelHeight);
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
			ImageToBlocks.CalculateNumOfBlocks(OutputOptions.Format, pixelWidth, pixelHeight, 1, out blocksWidth, out blocksHeight, out _);
		}

		#endregion

		#region HDR

		/// <summary>
		/// Encode a cubemap as raw bytes into a <see cref="BCnTextureData"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public BCnTextureData EncodeCubeMapHdr(ReadOnlyMemory2D<ColorRgbaFloat> right, ReadOnlyMemory2D<ColorRgbaFloat> left,
			ReadOnlyMemory2D<ColorRgbaFloat> top, ReadOnlyMemory2D<ColorRgbaFloat> down,
			ReadOnlyMemory2D<ColorRgbaFloat> back, ReadOnlyMemory2D<ColorRgbaFloat> front)
		{
			var inputData = new BCnTextureData(CompressionFormat.RgbaFloat, right.Width, right.Height, 1, 1, 1, true, false);

			if (
				right.Width != left.Width || right.Height != left.Height ||
				right.Width != top.Width || right.Height != top.Height ||
				right.Width != down.Width || right.Height != down.Height ||
				right.Width != back.Width || right.Height != back.Height ||
				right.Width != front.Width || right.Height != front.Height
			)
			{
				throw new ArgumentException("All faces of a cubeMap must be of equal width and height!");
			}

			inputData.Mips[0][CubeMapFaceDirection.XPositive].Data = right.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.XNegative].Data = left.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.YPositive].Data = top.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.YNegative].Data = down.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.ZPositive].Data = back.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.ZNegative].Data = front.CopyAsBytes();

			return EncodeInternal(inputData, default);
		}

		/// <summary>
		/// Encode a cubemap as raw bytes into a <see cref="BCnTextureData"/>.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public Task<BCnTextureData> EncodeCubeMapHdrAsync(ReadOnlyMemory2D<ColorRgbaFloat> right, ReadOnlyMemory2D<ColorRgbaFloat> left,
			ReadOnlyMemory2D<ColorRgbaFloat> top, ReadOnlyMemory2D<ColorRgbaFloat> down,
			ReadOnlyMemory2D<ColorRgbaFloat> back, ReadOnlyMemory2D<ColorRgbaFloat> front, CancellationToken token = default)
		{
			var inputData = new BCnTextureData(CompressionFormat.RgbaFloat, right.Width, right.Height, 1, 1, 1, true, false, AlphaChannelHint.Straight);

			if (
				right.Width != left.Width || right.Height != left.Height ||
				right.Width != top.Width || right.Height != top.Height ||
				right.Width != down.Width || right.Height != down.Height ||
				right.Width != back.Width || right.Height != back.Height ||
				right.Width != front.Width || right.Height != front.Height
			)
			{
				throw new ArgumentException("All faces of a cubeMap must be of equal width and height!");
			}

			inputData.Mips[0][CubeMapFaceDirection.XPositive].Data = right.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.XNegative].Data = left.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.YPositive].Data = top.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.YNegative].Data = down.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.ZPositive].Data = back.CopyAsBytes();
			inputData.Mips[0][CubeMapFaceDirection.ZNegative].Data = front.CopyAsBytes();

			return Task.Run(() => EncodeInternal(inputData, token), token);
		}

		/// <summary>
		/// Encodes all mipmap levels of a HDR image into <see cref="BCnTextureData"/>. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		public Task<BCnTextureData> EncodeHdrAsync(ReadOnlyMemory2D<ColorRgbaFloat> input, CancellationToken token = default)
		{
			var inputData = BCnTextureData.FromSingle(
				CompressionFormat.RgbaFloat,
				input.Width,
				input.Height,
				input.CopyAsBytes(),
				AlphaChannelHint.Straight);
			return Task.Run(() => EncodeInternal(inputData, token), token);
		}

		// /// <summary>
		// /// Encodes a single mip level of the input HDR image to a byte buffer asynchronously. This data does not contain any file headers, just the raw encoded pixel data.
		// /// </summary>
		// /// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		// /// <param name="mipLevel">The mipmap to encode.</param>
		// /// <param name="token">The cancellation token for this operation. Can be default if cancellation is not needed.</param>
		// /// <returns>The raw encoded input.</returns>
		// /// <remarks>To get the width and height of the encoded mip level, see <see cref="CalculateMipMapSize"/>.</remarks>
		// public Task<byte[]> EncodeToRawBytesHdrAsync(ReadOnlyMemory2D<ColorRgbaFloat> input, int mipLevel, CancellationToken token = default)
		// {
		// 	return Task.Run(() => EncodeSingleHdrInternal(input.Flatten(), input.Width, input.Height, mipLevel, token), token);
		// }

		/// <summary>
		/// Encodes all mipmap levels of a HDR image into <see cref="BCnTextureData"/>. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		public BCnTextureData EncodeHdr(ReadOnlyMemory2D<ColorRgbaFloat> input)
		{
			var inputData = BCnTextureData.FromSingle(
				CompressionFormat.RgbaFloat,
				input.Width,
				input.Height,
				input.CopyAsBytes(),
				AlphaChannelHint.Straight);
			return EncodeInternal(inputData, default);
		}

		// /// <summary>
		// /// Encodes a single mip level of the HDR input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		// /// </summary>
		// /// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		// /// <param name="mipLevel">The mipmap to encode.</param>
		// /// <param name="mipWidth">The width of the mipmap.</param>
		// /// <param name="mipHeight">The height of the mipmap.</param>
		// /// <returns>A byte buffer containing the encoded data of the requested mip-level.</returns>
		// public byte[] EncodeToRawBytesHdr(ReadOnlyMemory2D<ColorRgbaFloat> input, int mipLevel, out int mipWidth, out int mipHeight)
		// {
		// 	CalculateMipMapSize(input.Width, input.Height, mipLevel, out mipWidth, out mipHeight);
		// 	return EncodeSingleHdrInternal(input.Flatten(), input.Width, input.Height, mipLevel, default);
		// }

		// /// <summary>
		// /// Encodes a single 4x4 HDR block to raw encoded bytes. Input Span length must be exactly 16.
		// /// </summary>
		// /// <param name="inputBlock">Input 4x4 color block</param>
		// /// <returns>Raw encoded data</returns>
		// public byte[] EncodeBlockHdr(ReadOnlySpan<ColorRgbFloat> inputBlock)
		// {
		// 	if (inputBlock.Length != 16)
		// 	{
		// 		throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
		// 	}
		// 	return EncodeBlockHdrInternal(inputBlock.AsSpan2D(4, 4));
		// }
		//
		// /// <summary>
		// /// Encodes a single 4x4 HDR block to raw encoded bytes. Input Span width and height must be exactly 4.
		// /// </summary>
		// /// <param name="inputBlock">Input 4x4 color block</param>
		// /// <returns>Raw encoded data</returns>
		// public byte[] EncodeBlockHdr(ReadOnlySpan2D<ColorRgbFloat> inputBlock)
		// {
		// 	if (inputBlock.Width != 4 || inputBlock.Height != 4)
		// 	{
		// 		throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
		// 	}
		// 	return EncodeBlockHdrInternal(inputBlock);
		// }
		//
		// /// <summary>
		// /// Encodes a single 4x4 HDR block and writes the encoded block to a stream. Input Span length must be exactly 16.
		// /// </summary>
		// /// <param name="inputBlock">Input 4x4 color block</param>
		// /// <param name="outputStream">Output stream where the encoded block will be written to.</param>
		// public void EncodeBlockHdr(ReadOnlySpan<ColorRgbFloat> inputBlock, Stream outputStream)
		// {
		// 	if (inputBlock.Length != 16)
		// 	{
		// 		throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
		// 	}
		// 	EncodeBlockHdrInternal(inputBlock.AsSpan2D(4, 4), outputStream);
		// }
		//
		// /// <summary>
		// /// Encodes a single 4x4 HDR block and writes the encoded block to a stream. Input Span width and height must be exactly 4.
		// /// </summary>
		// /// <param name="inputBlock">Input 4x4 color block</param>
		// /// <param name="outputStream">Output stream where the encoded block will be written to.</param>
		// public void EncodeBlockHdr(ReadOnlySpan2D<ColorRgbFloat> inputBlock, Stream outputStream)
		// {
		// 	if (inputBlock.Width != 4 || inputBlock.Height != 4)
		// 	{
		// 		throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
		// 	}
		// 	EncodeBlockHdrInternal(inputBlock, outputStream);
		// }

		#endregion
		#region MipMap operations

		/// <summary>
		/// Calculates the number of mipmap levels that will be generated for the given input image.
		/// This method takes into account <see cref="EncoderOutputOptions.GenerateMipMaps"/> and
		/// <see cref="EncoderOutputOptions.MaxMipMapLevel"/>.
		/// </summary>
		/// <param name="imagePixelWidth">The width of the input image in pixels</param>
		/// <param name="imagePixelHeight">The height of the input image in pixels</param>
		/// <param name="imagePixelDepth">The depth of the input image in pixels</param>
		/// <returns>The number of mipmap levels that will be generated for the input image.</returns>
		public int CalculateNumberOfMipLevels(int imagePixelWidth, int imagePixelHeight, int imagePixelDepth)
		{
			return MipMapper.CalculateMipChainLength(imagePixelWidth, imagePixelHeight, imagePixelDepth,
				OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1);
		}

		public int CalculateNumberOfMipLevels(int imagePixelWidth, int imagePixelHeight)
		{
			return MipMapper.CalculateMipChainLength(imagePixelWidth, imagePixelHeight, 1,
				OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1);
		}

		/// <summary>
		/// Calculates the size of a given mipmap level.
		/// </summary>
		/// <param name="imagePixelWidth">The width of the input image in pixels</param>
		/// <param name="imagePixelHeight">The height of the input image in pixels</param>
		/// <param name="imagePixelDepth">The depth of the input image in pixels</param>
		/// <param name="mipLevel">The mipLevel to calculate (0 is original image)</param>
		/// <param name="mipWidth">The mipmap width calculated</param>
		/// <param name="mipHeight">The mipmap height calculated</param>
		/// <param name="mipDepth">The mipmap depth calculated</param>
		public void CalculateMipMapSize(int imagePixelWidth, int imagePixelHeight, int imagePixelDepth, int mipLevel, out int mipWidth, out int mipHeight, out int mipDepth)
		{
			MipMapper.CalculateMipLevelSize(imagePixelWidth, imagePixelHeight, imagePixelDepth, mipLevel, out mipWidth,
				out mipHeight, out mipDepth);
		}

		public void CalculateMipMapSize(int imagePixelWidth, int imagePixelHeight, int mipLevel, out int mipWidth, out int mipHeight)
		{
			MipMapper.CalculateMipLevelSize(imagePixelWidth, imagePixelHeight, 1, mipLevel, out mipWidth,
				out mipHeight, out _);
		}

		/// <summary>
		/// Calculates the byte size of a given mipmap level.
		/// This takes into account the current <see cref="EncoderOutputOptions.Format"/>
		/// </summary>
		/// <param name="imagePixelWidth">The width of the input image in pixels</param>
		/// <param name="imagePixelHeight">The height of the input image in pixels</param>
		/// <param name="imagePixelDepth">The depth of the input image in pixels</param>
		/// <param name="mipLevel">The mipLevel to calculate (0 is original image)</param>
		public long CalculateMipMapByteSize(int imagePixelWidth, int imagePixelHeight, int imagePixelDepth, int mipLevel)
		{
			MipMapper.CalculateMipLevelSize(imagePixelWidth, imagePixelHeight, imagePixelDepth, mipLevel, out var mipWidth,
				out var mipHeight, out var mipDepth);
			return OutputOptions.Format.CalculateMipByteSize(mipWidth, mipHeight, mipDepth);
		}

		public long CalculateMipMapByteSize(int imagePixelWidth, int imagePixelHeight, int mipLevel)
		{
			MipMapper.CalculateMipLevelSize(imagePixelWidth, imagePixelHeight, 1, mipLevel, out var mipWidth,
				out var mipHeight, out var mipDepth);
			return OutputOptions.Format.CalculateMipByteSize(mipWidth, mipHeight, mipDepth);
		}

		#endregion

		#region Private

		private void EncodeSingleInternal(ReadOnlyMemory<byte> input, Memory<byte> output, CompressionFormat inputFormat, int width,
			int height, int depth, int mipLevel, CancellationToken token)
		{
			var outputFormat = OutputOptions.Format;
			var encoder = GetEncoder(outputFormat);

			if (encoder == null)
			{
				throw new NotSupportedException($"This format is not supported for this method: {OutputOptions.Format}");
			}

			if (inputFormat.IsBlockCompressedFormat())
			{
				throw new InvalidOperationException("Single mip encoding only supports raw formats as inputs!");
			}

			MipMapper.CalculateMipLevelSize(width, height, depth, mipLevel, out var mipWidth, out var mipHeight, out var mipDepth);

			var totalBlocks = outputFormat.CalculateMipByteSize(mipWidth, mipHeight, mipDepth) / outputFormat.GetBytesPerBlock();

			// Track whether the input is in sRGB space to respect AsIs option
			var inputIsSrgb = inputFormat.IsSRGBFormat();
			var ignoreColorSpace = OutputOptions.ColorSpaceHandling == EncoderColorSpaceHandling.KeepAsIs;

			// 1. Convert to RgbaFloat format
			Memory<ColorRgbaFloat> floatData;

			if (inputFormat != CompressionFormat.RgbaFloat)
			{
				// For KeepAsIs, don't apply color space conversion
				var initialConversion = ignoreColorSpace ?
					ColorConversionMode.None :
					(inputIsSrgb ? ColorConversionMode.SrgbToLinear : ColorConversionMode.None);

				floatData = ColorExtensions.InternalConvertToAsBytesFromBytes(
					input, inputFormat, CompressionFormat.RgbaFloat, initialConversion)
					.AsMemory().Cast<byte, ColorRgbaFloat>();
			}
			else
			{
				floatData = new ColorRgbaFloat[width * height * depth];
				input.Cast<byte, ColorRgbaFloat>().CopyTo(floatData);
			}

			// 2. Process alpha based on AlphaHandling setting
			if (OutputOptions.Format.SupportsAlpha() && !OutputOptions.Format.IsHdrFormat())
			{
				var alphaHint = AlphaChannelHint.Unknown;
				var resultAlphaHint = AlphaHandlingHelper.ProcessAlpha(floatData, OutputOptions.AlphaHandling, alphaHint);
			}

			// 3. Generate mipmap if needed
			if (mipLevel > 0)
			{
				// Generate the mipmap (alpha handling already done above)
				MipMapper.GenerateSingleMip(floatData.AsMemory2D(height, width), mipLevel).TryGetMemory(out floatData);
			}

			// 4. Determine color conversion mode for final encoding
			ColorConversionMode colorConversionMode = ColorConversionMode.None;

			if (!ignoreColorSpace)
			{
				if (OutputOptions.ColorSpaceHandling == EncoderColorSpaceHandling.ProcessLinearPreserveColorSpace)
				{
					if (inputIsSrgb)
						colorConversionMode = ColorConversionMode.LinearToSrgb;
				}
				else if (OutputOptions.ColorSpaceHandling == EncoderColorSpaceHandling.Auto)
				{
					// Determine color conversion mode
					colorConversionMode = DetermineColorConversionMode(
						CompressionFormat.RgbaFloat, outputFormat);
				}
			}

			// Setup encoding context
			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount,
				Progress = new OperationProgress(Options.Progress, totalBlocks),
				ColorConversionMode = colorConversionMode,
				Weights = OutputOptions.UsePerceptualMetrics ? RgbWeights.Perceptual : RgbWeights.Linear,
				Quality = OutputOptions.Quality
			};

			if (inputFormat.IsUNormFormat() && outputFormat.IsSNormFormat() && OutputOptions.RescaleUnormToSnorm)
			{
				UNormToSNorm(floatData.Span);
			}

			// 5. Encode the data
			var result = encoder.Encode(floatData, mipWidth, mipHeight, context);

			result.CopyTo(output);
		}

		private BCnTextureData EncodeInternal(BCnTextureData textureData, CancellationToken token)
		{
			var encoder = GetEncoder(OutputOptions.Format);
			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;

			// Track whether the input is in sRGB space to respect AsIs option
			var inputFormat = textureData.Format;
			var inputIsSrgb = textureData.Format.IsSRGBFormat();
			var ignoreColorSpace = OutputOptions.ColorSpaceHandling == EncoderColorSpaceHandling.KeepAsIs;

			// 1. Convert texture data to RgbaFloat format for consistent processing, also ensuring we're in linear space
			textureData = textureData.ConvertTo(CompressionFormat.RgbaFloat, convertColorspace: !ignoreColorSpace);

			// 2. Process alpha according to the chosen handling method
			if (OutputOptions.Format.SupportsAlpha() && !OutputOptions.Format.IsHdrFormat())
				AlphaHandlingHelper.ProcessAlpha(textureData, OutputOptions.AlphaHandling);

			// 3. Generate mipmaps if needed (now that we're in linear space with correct alpha)
			if (OutputOptions.GenerateMipMaps)
			{
				textureData = MipMapper.GenerateMipChain(textureData, ref numMipMaps);
			}

			// 4. Create output texture data with the correct format
			var newData = new BCnTextureData(
				OutputOptions.Format,
				textureData.Width,
				textureData.Height,
				textureData.Depth,
				numMipMaps,
				textureData.NumArrayElements,
				textureData.IsCubeMap, false,
				textureData.AlphaChannelHint);

			ColorConversionMode colorConversionMode = ColorConversionMode.None;

			// 5. Determine final color conversion mode for encoding
			if (!ignoreColorSpace)
			{
				if (OutputOptions.ColorSpaceHandling == EncoderColorSpaceHandling.ProcessLinearPreserveColorSpace)
				{
					if (inputIsSrgb)
						colorConversionMode = ColorConversionMode.LinearToSrgb;
				}
				else if (OutputOptions.ColorSpaceHandling == EncoderColorSpaceHandling.Auto)
				{
					colorConversionMode = DetermineColorConversionMode(
						CompressionFormat.RgbaFloat, OutputOptions.Format);
				}
			}

			// Setup encoding context
			var totalBlocks = newData.TotalByteSize / OutputOptions.Format.GetBytesPerBlock();
			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount,
				Progress = new OperationProgress(Options.Progress, totalBlocks),
				ColorConversionMode = colorConversionMode,
				Weights = OutputOptions.UsePerceptualMetrics ? RgbWeights.Perceptual : RgbWeights.Linear,
				Quality = OutputOptions.Quality
			};

			// 6. Encode each mip level
			for (var m = 0; m < newData.NumMips; m++)
			{
				for (var f = 0; f < newData.NumFaces; f++)
				{
					for (var a = 0; a < newData.NumArrayElements; a++)
					{
						var mipWidth = textureData.Mips[m].Width;
						var mipHeight = textureData.Mips[m].Height;
						var colorMemory = textureData.Mips[m][(CubeMapFaceDirection)f, a].AsMemory<ColorRgbaFloat>();

						if (inputFormat.IsUNormFormat() && OutputOptions.Format.IsSNormFormat() &&
						    OutputOptions.RescaleUnormToSnorm)
						{
							UNormToSNorm(colorMemory.Span);
						}

						var encoded = encoder.Encode(colorMemory, mipWidth, mipHeight, context);

						if (newData.Mips[m].SizeInBytes != encoded.Length)
						{
							throw new InvalidOperationException("Encoded size does not match expected!");
						}

						newData.Mips[m][(CubeMapFaceDirection)f, a].Data = encoded;
					}
				}
			}

			return newData;
		}

		/// <summary>
		/// Determines the color conversion mode based on the source and destination formats
		/// </summary>
		private ColorConversionMode DetermineColorConversionMode(CompressionFormat sourceFormat, CompressionFormat destFormat)
		{
			ColorConversionMode colorConversionMode = ColorConversionMode.None;
			if (OutputOptions.ColorSpaceHandling != EncoderColorSpaceHandling.KeepAsIs)
			{
				// Auto-detect appropriate colorspace conversion based on formats
				colorConversionMode = sourceFormat.GetColorConversionMode(destFormat);
			}
			return colorConversionMode;
		}
		#endregion

		#region Support

		private IBcBlockEncoder GetBlockEncoder(CompressionFormat format)
		{
			return GetEncoder(format) as IBcBlockEncoder;
		}

		private IBcEncoder GetEncoder(CompressionFormat format)
		{
			if (format.IsRawPixelFormat())
			{
				return Activator.CreateInstance(typeof(RawPixelEncoder<>).MakeGenericType(format.GetPixelType())) as IBcEncoder;
			}
			switch (format)
			{
				case CompressionFormat.Bc1:
				case CompressionFormat.Bc1_sRGB:
					return new Bc1BlockEncoder();
				case CompressionFormat.Bc1WithAlpha:
				case CompressionFormat.Bc1WithAlpha_sRGB:
					return new Bc1AlphaBlockEncoder();
				case CompressionFormat.Bc2:
				case CompressionFormat.Bc2_sRGB:
					return new Bc2BlockEncoder();
				case CompressionFormat.Bc3:
				case CompressionFormat.Bc3_sRGB:
					return new Bc3BlockEncoder();
				case CompressionFormat.Bc4:
					return new Bc4BlockEncoder(InputOptions.Bc4Component);
				case CompressionFormat.Bc5:
					return new Bc5BlockEncoder(InputOptions.Bc5Component1, InputOptions.Bc5Component2);
				case CompressionFormat.Bc6U:
					return new Bc6Encoder(false);
				case CompressionFormat.Bc6S:
					return new Bc6Encoder(true);
				case CompressionFormat.Bc7:
				case CompressionFormat.Bc7_sRGB:
					return new Bc7Encoder();
				case CompressionFormat.Atc:
					return new AtcBlockEncoder();
				case CompressionFormat.AtcExplicitAlpha:
					return new AtcExplicitAlphaBlockEncoder();
				case CompressionFormat.AtcInterpolatedAlpha:
					return new AtcInterpolatedAlphaBlockEncoder();
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		private byte[] AllocateOutputBuffer(int pixelWidth, int pixelHeight, int pixelDepth)
		{
			var output = new byte[OutputOptions.Format.CalculateMipByteSize(pixelWidth, pixelHeight, pixelDepth)];
			return output;
		}

		private ReadOnlyMemory<byte> PrepareInputBuffer<TIn>(ReadOnlyMemory<TIn> input, int pixelWidth, int pixelHeight, int pixelDepth, CompressionFormat inputFormat)
			where TIn : unmanaged
		{
			var bytes = input.Cast<TIn, byte>();

			if (bytes.Length != inputFormat.CalculateMipByteSize(pixelWidth, pixelHeight, pixelDepth))
			{
				throw new ArgumentException($"Invalid input size. Expected {inputFormat.CalculateMipByteSize(pixelWidth, pixelHeight, pixelDepth)} bytes, but got {bytes.Length} bytes.", nameof(input));
			}

			return bytes;
		}

		private static void UNormToSNorm(Span<ColorRgbaFloat> colors)
		{
			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = new ColorRgbaFloat(
					(colors[i].r - 0.5f) * 2,
					(colors[i].g - 0.5f) * 2,
					(colors[i].b - 0.5f) * 2
				);
			}
		}

		#endregion
	}
}
