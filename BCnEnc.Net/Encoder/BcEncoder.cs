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
			var inputData = new BCnTextureData(CompressionFormat.Rgba32, right.Width, right.Height, 1, 1, 1, true, false);

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
			var inputData = new BCnTextureData(CompressionFormat.Rgba32, right.Width, right.Height, 1, 1, 1, true, false);

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
			var inputData = BCnTextureData.FromSingle(CompressionFormat.Rgba32, input.Width, input.Height, input.CopyAsBytes());
			return EncodeInternal(inputData, default);
		}

		/// <summary>
		/// Encode input data as raw bytes into a <see cref="BCnTextureData"/>.
		/// </summary>
		/// <param name="input">The input data to encode.</param>
		/// <returns>A <see cref="BCnTextureData"/> containing the encoded texture data.</returns>
		public Task<BCnTextureData> EncodeAsync(ReadOnlyMemory2D<ColorRgba32> input, CancellationToken token = default)
		{
			var inputData = BCnTextureData.FromSingle(CompressionFormat.Rgba32, input.Width, input.Height, input.CopyAsBytes());
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
			CalculateMipMapSize(input.Width, input.Height, mipLevel, out int mipWidth, out int mipHeight);

			var output = AllocateOutputBuffer(mipWidth, mipHeight);

			await Task.Run(() => EncodeSingleInternal(input.First.Data, output, input.Format, input.Width, input.Height, mipLevel, token), token);

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
			var inputData = BCnTextureData.FromSingle(inputFormat, width, height, input);
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
			var inputData = BCnTextureData.FromSingle(inputFormat, width, height, input);
			return EncodeInternal(inputData, default);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		/// </summary>
		public byte[] EncodeToRawBytes<TIn>(ReadOnlyMemory<TIn> input, int width, int height, CompressionFormat inputFormat, int mipLevel, out int mipWidth, out int mipHeight)
			where TIn : unmanaged
		{
			CalculateMipMapSize(width, height, mipLevel, out mipWidth, out mipHeight);

			var inputBytes = PrepareInputBuffer(input, width, height, inputFormat);
			var output = AllocateOutputBuffer(mipWidth, mipHeight);

			EncodeSingleInternal(inputBytes, output, inputFormat, width, height, mipLevel, default);

			return output;
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer. This data does not contain any file headers, just the raw encoded pixel data.
		/// Note that even if the input data already contains mipLevels, new mips are generated from the first mip.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="mipWidth">The width of the mipmap.</param>
		/// <param name="mipHeight">The height of the mipmap.</param>
		/// <returns>A byte buffer containing the encoded data of the requested mip-level.</returns>
		public byte[] EncodeToRawBytes(BCnTextureData input, int mipLevel, out int mipWidth, out int mipHeight)
		{
			CalculateMipMapSize(input.Width, input.Height, mipLevel, out mipWidth, out mipHeight);

			var output = AllocateOutputBuffer(mipWidth, mipHeight);

			EncodeSingleInternal(input.First.Data, output, input.Format, input.Width, input.Height, mipLevel, default);

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
				input.CopyAsBytes());
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
				input.CopyAsBytes());
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

		/// <summary>
		/// Calculates the byte size of a given mipmap level.
		/// This takes into account the current <see cref="EncoderOutputOptions.Format"/>
		/// </summary>
		/// <param name="imagePixelWidth">The width of the input image in pixels</param>
		/// <param name="imagePixelHeight">The height of the input image in pixels</param>
		/// <param name="mipLevel">The mipLevel to calculate (0 is original image)</param>
		public long CalculateMipMapByteSize(int imagePixelWidth, int imagePixelHeight, int mipLevel)
		{
			MipMapper.CalculateMipLevelSize(imagePixelWidth, imagePixelHeight, mipLevel, out var mipWidth,
				out var mipHeight);
			return OutputOptions.Format.CalculateMipByteSize(mipWidth, mipHeight);
		}

		#endregion

		#region Private

		private void EncodeSingleInternal(ReadOnlyMemory<byte> input, Memory<byte> output, CompressionFormat inputFormat, int width,
			int height, int mipLevel, CancellationToken token)
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

			MipMapper.CalculateMipLevelSize(width, height, mipLevel, out var mipWidth, out var mipHeight);

			var totalBlocks = outputFormat.CalculateMipByteSize(mipWidth, mipHeight) / outputFormat.GetBytesPerBlock();

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount,
				Progress = new OperationProgress(Options.Progress, totalBlocks),
				ColorConversionMode = CompressionFormat.RgbaFloat.GetColorConversionMode(outputFormat)
			};

			ReadOnlyMemory<ColorRgbaFloat> floatData;
			if (inputFormat != CompressionFormat.RgbaFloat)
			{
				floatData = ColorExtensions.InternalConvertToAsBytesFromBytes(input, inputFormat, CompressionFormat.RgbaFloat,
						inputFormat.GetColorConversionMode(CompressionFormat.RgbaFloat))
					.AsMemory().Cast<byte, ColorRgbaFloat>();
			}
			else
			{
				floatData = input.Cast<byte, ColorRgbaFloat>();
			}

			MipMapper.GenerateSingleMip(floatData.AsMemory2D(height, width), mipLevel).TryGetMemory(out floatData);

			var result = encoder.Encode(floatData, mipWidth, mipHeight, OutputOptions.Quality, context);

			result.CopyTo(output);
		}

		private BCnTextureData EncodeInternal(BCnTextureData textureData, CancellationToken token)
		{
			var encoder = GetEncoder(OutputOptions.Format);

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;

			textureData = MipMapper.GenerateMipChain(textureData, ref numMipMaps);

			var newData = new BCnTextureData(
				OutputOptions.Format,
				textureData.Width,
				textureData.Height,
				1,
				numMipMaps,
				textureData.NumArrayElements,
				textureData.IsCubeMap, false);

			var totalBlocks = newData.TotalByteSize / OutputOptions.Format.GetBytesPerBlock();

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = !Debugger.IsAttached && Options.IsParallel,
				TaskCount = Options.TaskCount,
				Progress = new OperationProgress(Options.Progress, totalBlocks),
				ColorConversionMode = CompressionFormat.RgbaFloat.GetColorConversionMode(OutputOptions.Format)
			};

			for (var m = 0; m < newData.NumMips; m++)
			{
				for (var f = 0; f < newData.NumFaces; f++)
				{
					for (var a = 0; a < newData.NumArrayElements; a++)
					{
						var mipWidth = textureData.Mips[m].Width;
						var mipHeight = textureData.Mips[m].Height;
						var colorMemory = textureData.Mips[m][(CubeMapFaceDirection)f, a].AsMemory<ColorRgbaFloat>();
						var encoded = encoder.Encode(colorMemory, mipWidth, mipHeight, OutputOptions.Quality,
							context);

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

		// private byte[] EncodeBlockHdrInternal(ReadOnlySpan2D<ColorRgbFloat> input)
		// {
		// 	var compressedEncoder = GetFloatBlockEncoder(OutputOptions.Format);
		// 	if (compressedEncoder == null)
		// 	{
		// 		throw new NotSupportedException($"This Format is not supported for hdr single block encoding: {OutputOptions.Format}");
		// 	}
		//
		// 	var output = new byte[compressedEncoder.GetBlockSize()];
		//
		// 	var rawBlock = new RawBlock4X4RgbFloat();
		//
		// 	var pixels = rawBlock.AsSpan;
		//
		// 	input.GetRowSpan(0).CopyTo(pixels);
		// 	input.GetRowSpan(1).CopyTo(pixels.Slice(4));
		// 	input.GetRowSpan(2).CopyTo(pixels.Slice(8));
		// 	input.GetRowSpan(3).CopyTo(pixels.Slice(12));
		//
		// 	compressedEncoder.EncodeBlock(rawBlock, OutputOptions.Quality, output);
		//
		// 	return output;
		// }
		//
		// private void EncodeBlockHdrInternal(ReadOnlySpan2D<ColorRgbFloat> input, Stream outputStream)
		// {
		// 	var compressedEncoder = GetFloatBlockEncoder(OutputOptions.Format);
		// 	if (compressedEncoder == null)
		// 	{
		// 		throw new NotSupportedException($"This Format is not supported for hdr single block encoding: {OutputOptions.Format}");
		// 	}
		// 	if (input.Width != 4 || input.Height != 4)
		// 	{
		// 		throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
		// 	}
		//
		// 	Span<byte> output = stackalloc byte[16];
		// 	output = output.Slice(0, compressedEncoder.GetBlockSize());
		//
		// 	var rawBlock = new RawBlock4X4RgbFloat();
		//
		// 	var pixels = rawBlock.AsSpan;
		//
		// 	input.GetRowSpan(0).CopyTo(pixels);
		// 	input.GetRowSpan(1).CopyTo(pixels.Slice(4));
		// 	input.GetRowSpan(2).CopyTo(pixels.Slice(8));
		// 	input.GetRowSpan(3).CopyTo(pixels.Slice(12));
		//
		// 	compressedEncoder.EncodeBlock(rawBlock, OutputOptions.Quality, output);
		//
		// 	outputStream.Write(output);
		// }
		//
		// private byte[] EncodeBlockLdrInternal(ReadOnlySpan2D<ColorRgba32> input)
		// {
		// 	var compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
		// 	if (compressedEncoder == null)
		// 	{
		// 		throw new NotSupportedException($"This Format is not supported for ldr single block encoding: {OutputOptions.Format}");
		// 	}
		//
		// 	var output = new byte[compressedEncoder.GetBlockSize()];
		//
		// 	var rawBlock = new RawBlock4X4Rgba32();
		//
		// 	var pixels = rawBlock.AsSpan;
		//
		// 	input.GetRowSpan(0).CopyTo(pixels);
		// 	input.GetRowSpan(1).CopyTo(pixels.Slice(4));
		// 	input.GetRowSpan(2).CopyTo(pixels.Slice(8));
		// 	input.GetRowSpan(3).CopyTo(pixels.Slice(12));
		//
		// 	compressedEncoder.EncodeBlock(rawBlock, OutputOptions.Quality, output);
		//
		// 	return output;
		// }
		//
		// private void EncodeBlockLdrInternal(ReadOnlySpan2D<ColorRgba32> input, Stream outputStream)
		// {
		// 	var compressedEncoder = GetRgba32BlockEncoder(OutputOptions.Format);
		// 	if (compressedEncoder == null)
		// 	{
		// 		throw new NotSupportedException($"This Format is not supported for ldr single block encoding: {OutputOptions.Format}");
		// 	}
		// 	if (input.Width != 4 || input.Height != 4)
		// 	{
		// 		throw new ArgumentException($"Single block encoding can only encode blocks of 4x4");
		// 	}
		//
		// 	Span<byte> output = stackalloc byte[16];
		// 	output = output.Slice(0, compressedEncoder.GetBlockSize());
		//
		// 	var rawBlock = new RawBlock4X4Rgba32();
		//
		// 	var pixels = rawBlock.AsSpan;
		//
		// 	input.GetRowSpan(0).CopyTo(pixels);
		// 	input.GetRowSpan(1).CopyTo(pixels.Slice(4));
		// 	input.GetRowSpan(2).CopyTo(pixels.Slice(8));
		// 	input.GetRowSpan(3).CopyTo(pixels.Slice(12));
		//
		// 	compressedEncoder.EncodeBlock(rawBlock, OutputOptions.Quality, output);
		//
		// 	outputStream.Write(output);
		// }

		#endregion

		#region Support

		private IBcBlockEncoder GetBlockEncoder(CompressionFormat format)
		{
			return GetEncoder(format) as IBcBlockEncoder;
		}

		private IBcEncoder GetEncoder(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R8:
					return new RawPixelEncoder<ColorR8>();
				case CompressionFormat.R8G8:
					return new RawPixelEncoder<ColorR8G8>();
				case CompressionFormat.Rgb24:
				case CompressionFormat.Rgb24_sRGB:
					return new RawPixelEncoder<ColorRgb24>();
				case CompressionFormat.Bgr24:
				case CompressionFormat.Bgr24_sRGB:
					return new RawPixelEncoder<ColorBgr24>();
				case CompressionFormat.Rgba32:
				case CompressionFormat.Rgba32_sRGB:
					return new RawPixelEncoder<ColorRgba32>();
				case CompressionFormat.Bgra32:
				case CompressionFormat.Bgra32_sRGB:
					return new RawPixelEncoder<ColorBgra32>();
				case CompressionFormat.RgbaFloat:
					return new RawPixelEncoder<ColorRgbaFloat>();
				case CompressionFormat.RgbaHalf:
					return new RawPixelEncoder<ColorRgbaHalf>();
				case CompressionFormat.RgbFloat:
					return new RawPixelEncoder<ColorRgbFloat>();
				case CompressionFormat.RgbHalf:
					return new RawPixelEncoder<ColorRgbHalf>();
				case CompressionFormat.Rgbe:
					return new RawPixelEncoder<ColorRgbe>();
				case CompressionFormat.Xyze:
					return new RawPixelEncoder<ColorXyze>();
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

		private byte[] AllocateOutputBuffer(int pixelWidth, int pixelHeight)
		{
			var output = new byte[OutputOptions.Format.CalculateMipByteSize(pixelWidth, pixelHeight)];
			return output;
		}

		private ReadOnlyMemory<byte> PrepareInputBuffer<TIn>(ReadOnlyMemory<TIn> input, int pixelWidth, int pixelHeight, CompressionFormat inputFormat)
			where TIn : unmanaged
		{
			var bytes = input.Cast<TIn, byte>();

			if (bytes.Length != inputFormat.CalculateMipByteSize(pixelWidth, pixelHeight))
			{
				throw new ArgumentException($"Invalid input size. Expected {inputFormat.CalculateMipByteSize(pixelWidth, pixelHeight)} bytes, but got {bytes.Length} bytes.", nameof(input));
			}

			return bytes;
		}
		#endregion
	}
}
