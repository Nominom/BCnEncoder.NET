using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.ImageSharp
{
	public static class BCnEncoderExtensions
	{
		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static void EncodeToStream(this BcEncoder encoder, Image<Rgba32> inputImage, Stream outputStream)
		{
			encoder.EncodeToStream(ImageToMemory2D(inputImage), outputStream);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public static KtxFile EncodeToKtx(this BcEncoder encoder, Image<Rgba32> inputImage)
		{
			return encoder.EncodeToKtx(ImageToMemory2D(inputImage));
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public static DdsFile EncodeToDds(this BcEncoder encoder, Image<Rgba32> inputImage)
		{
			return encoder.EncodeToDds(ImageToMemory2D(inputImage));
		}

		/// <summary>
		/// Encodes all mipmap levels into an array of byte buffers. This data does not contain any file headers, just the raw encoded data.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <returns>A list of raw encoded data.</returns>
		public static byte[][] EncodeToRawBytes(this BcEncoder encoder, Image<Rgba32> inputImage)
		{
			return encoder.EncodeToRawBytes(ImageToMemory2D(inputImage));
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer. This data does not contain any file headers, just the raw encoded data.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="mipWidth">The width of the mipmap.</param>
		/// <param name="mipHeight">The height of the mipmap.</param>
		/// <returns>The raw encoded data.</returns>
		public static byte[] EncodeToRawBytes(this BcEncoder encoder, Image<Rgba32> inputImage, int mipLevel, out int mipWidth, out int mipHeight)
		{
			return encoder.EncodeToRawBytes(ImageToMemory2D(inputImage), mipLevel, out mipWidth, out mipHeight);
		}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into either a ktx or a dds file and writes it to the output stream.
		/// Order is +X, -X, +Y, -Y, +Z, -Z
		/// </summary>
		/// <param name="right">The right face of the cubemap.</param>
		/// <param name="left">The left face of the cubemap.</param>
		/// <param name="top">The top face of the cubemap.</param>
		/// <param name="down">The bottom face of the cubemap.</param>
		/// <param name="back">The back face of the cubemap.</param>
		/// <param name="front">The front face of the cubemap.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static void EncodeCubeMapToStream(this BcEncoder encoder, Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, Stream outputStream)
		{
			encoder.EncodeCubeMapToStream(
				ImageToMemory2D(right),
				ImageToMemory2D(left),
				ImageToMemory2D(top),
				ImageToMemory2D(down),
				ImageToMemory2D(back),
				ImageToMemory2D(front),
				outputStream
			);
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
		public static KtxFile EncodeCubeMapToKtx(this BcEncoder encoder, Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front)
		{
			return encoder.EncodeCubeMapToKtx(
				ImageToMemory2D(right),
				ImageToMemory2D(left),
				ImageToMemory2D(top),
				ImageToMemory2D(down),
				ImageToMemory2D(back),
				ImageToMemory2D(front)
			);
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
		public static DdsFile EncodeCubeMapToDds(this BcEncoder encoder, Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front)
		{
			return encoder.EncodeCubeMapToDds(
				ImageToMemory2D(right),
				ImageToMemory2D(left),
				ImageToMemory2D(top),
				ImageToMemory2D(down),
				ImageToMemory2D(back),
				ImageToMemory2D(front)
			);
		}

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream asynchronously.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		public static Task EncodeToStreamAsync(this BcEncoder encoder, Image<Rgba32> inputImage, Stream outputStream, CancellationToken token = default)
		{
			return encoder.EncodeToStreamAsync(ImageToMemory2D(inputImage), outputStream, token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file asynchronously.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public static Task<KtxFile> EncodeToKtxAsync(this BcEncoder encoder, Image<Rgba32> inputImage, CancellationToken token = default)
		{
			return encoder.EncodeToKtxAsync(ImageToMemory2D(inputImage), token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file asynchronously.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public static Task<DdsFile> EncodeToDdsAsync(this BcEncoder encoder, Image<Rgba32> inputImage, CancellationToken token = default)
		{
			return encoder.EncodeToDdsAsync(ImageToMemory2D(inputImage), token);
		}

		/// <summary>
		/// Encodes all mipmap levels into an array of byte buffers asynchronously. This data does not contain any file headers, just the raw encoded data.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>A list of raw encoded data.</returns>
		public static Task<byte[][]> EncodeToRawBytesAsync(this BcEncoder encoder, Image<Rgba32> inputImage, CancellationToken token = default)
		{
			return encoder.EncodeToRawBytesAsync(ImageToMemory2D(inputImage), token);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer asynchronously. This data does not contain any file headers, just the raw encoded data.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The raw encoded data.</returns>
		/// <remarks>To get the width and height of the encoded mipLevel, see <see cref="CalculateMipMapSize(Image{Rgba32},int,out int,out int)"/>.</remarks>
		public static Task<byte[]> EncodeToRawBytesAsync(this BcEncoder encoder, Image<Rgba32> inputImage, int mipLevel, CancellationToken token = default)
		{
			return encoder.EncodeToRawBytesAsync(ImageToMemory2D(inputImage), mipLevel, token);
		}

		/// <summary>
		/// Encodes all cubemap faces and mipmap levels into either a ktx or a dds file and writes it to the output stream asynchronously.
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
		public static Task EncodeCubeMapToStreamAsync(this BcEncoder encoder, Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, Stream outputStream, CancellationToken token = default)
		{
			return encoder.EncodeCubeMapToStreamAsync(
				ImageToMemory2D(right),
				ImageToMemory2D(left),
				ImageToMemory2D(top),
				ImageToMemory2D(down),
				ImageToMemory2D(back),
				ImageToMemory2D(front),
				outputStream,
				token
			);
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
		public static Task<KtxFile> EncodeCubeMapToKtxAsync(this BcEncoder encoder, Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top,
			Image<Rgba32> down, Image<Rgba32> back, Image<Rgba32> front, CancellationToken token = default)
		{
			return encoder.EncodeCubeMapToKtxAsync(
				ImageToMemory2D(right),
				ImageToMemory2D(left),
				ImageToMemory2D(top),
				ImageToMemory2D(down),
				ImageToMemory2D(back),
				ImageToMemory2D(front),
				token
			);
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
		public static Task<DdsFile> EncodeCubeMapToDdsAsync(this BcEncoder encoder, Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top,
			Image<Rgba32> down, Image<Rgba32> back, Image<Rgba32> front, CancellationToken token = default)
		{
			return encoder.EncodeCubeMapToDdsAsync(
				ImageToMemory2D(right),
				ImageToMemory2D(left),
				ImageToMemory2D(top),
				ImageToMemory2D(down),
				ImageToMemory2D(back),
				ImageToMemory2D(front),
				token
			);
		}

		/// <summary>
		/// Calculates the number of mipmap levels that will be generated for the given input image.
		/// </summary>
		/// <param name="inputImage">The image to use for the calculation.</param>
		/// <returns>The number of mipmap levels that will be generated for the input image.</returns>
		public static int CalculateNumberOfMipLevels(this BcEncoder encoder, Image<Rgba32> inputImage)
		{
			return encoder.CalculateNumberOfMipLevels(inputImage.Width, inputImage.Height);
		}

		/// <summary>
		/// Calculates the size of a given mipmap level.
		/// </summary>
		/// <param name="inputImage">The image to use for the calculation.</param>
		/// <param name="mipLevel">The mipLevel to calculate (0 is original image)</param>
		/// <param name="mipWidth">The mipmap width calculated</param>
		/// <param name="mipHeight">The mipmap height calculated</param>
		public static void CalculateMipMapSize(this BcEncoder encoder, Image<Rgba32> inputImage, int mipLevel, out int mipWidth, out int mipHeight)
		{
			encoder.CalculateMipMapSize(inputImage.Width, inputImage.Height,
				mipLevel, out mipWidth, out mipHeight);
		}

		
		private static Memory2D<ColorRgba32> ImageToMemory2D(Image<Rgba32> inputImage)
		{
			var pixels = inputImage.GetPixelMemoryGroup()[0];
			var colors = new ColorRgba32[inputImage.Width * inputImage.Height];
			for (var y = 0; y < inputImage.Height; y++)
			{
				var yPixels = inputImage.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y);
				var yColors = colors.AsSpan(y * inputImage.Width, inputImage.Width);

				MemoryMarshal.Cast<Rgba32, ColorRgba32>(yPixels).CopyTo(yColors);
			}
			var memory = colors.AsMemory().AsMemory2D(inputImage.Height, inputImage.Width);
			return memory;
		}
	}
}
