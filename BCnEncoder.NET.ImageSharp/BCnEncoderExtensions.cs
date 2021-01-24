using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.NET.ImageSharp
{
	public static class BCnEncoderExtensions
	{
		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream.
		/// </summary>
		/// <param name="encoder">The BcEncoder instance.</param>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static void EncodeToStream(this BcEncoder encoder, Image<Rgba32> inputImage, Stream outputStream)
		{
			var pixels = inputImage.GetPixelMemoryGroup()[0];
			encoder.EncodeToStream(pixels, inputImage.Width, inputImage.Height, outputStream);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file.
		/// </summary>
		/// <param name="encoder">The BcEncoder instance.</param>
		/// <param name="inputImage">The image to encode.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public static KtxFile EncodeToKtx(this BcEncoder encoder, Image<Rgba32> inputImage)
		{
			return EncodeToKtxInternal(inputImage, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file.
		/// </summary>
		/// <param name="encoder">The BcEncoder instance.</param>
		/// <param name="inputImage">The image to encode.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public static DdsFile EncodeToDds(this BcEncoder encoder, Image<Rgba32> inputImage)
		{
			return EncodeToDdsInternal(inputImage, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a list of byte buffers.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <returns>A list of raw encoded data.</returns>
		public IList<byte[]> EncodeToRawBytes(Image<Rgba32> inputImage)
		{
			return EncodeToRawBytesInternal(this BcEncoder encoder, inputImage, default);
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
			return EncodeToRawBytesInternal(this BcEncoder encoder, inputImage, mipLevel, out mipWidth, out mipHeight, default);
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
		public void EncodeCubeMapToStream(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, Stream outputStream)
		{
			EncodeCubeMapInternal(this BcEncoder encoder, right, left, top, down, back, front, outputStream, default);
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
			return EncodeCubeMapToKtxInternal(this BcEncoder encoder, right, left, top, down, back, front, default);
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
			return EncodeCubeMapToDdsInternal(this BcEncoder encoder, right, left, top, down, back, front, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream asynchronously.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		public Task EncodeToStreamAsync(Image<Rgba32> inputImage, Stream outputStream, CancellationToken token = default)
		{
			
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file asynchronously.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public Task<KtxFile> EncodeToKtxAsync(Image<Rgba32> inputImage, CancellationToken token = default)
		{
			
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file asynchronously.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public Task<DdsFile> EncodeToDdsAsync(Image<Rgba32> inputImage, CancellationToken token = default)
		{
			
		}

		/// <summary>
		/// Encodes all mipmap levels into a list of byte buffers asynchronously.
		/// </summary>
		/// <param name="inputImage">The image to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>A list of raw encoded data.</returns>
		public Task<IList<byte[]>> EncodeToRawBytesAsync(Image<Rgba32> inputImage, CancellationToken token = default)
		{
			
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
		public Task EncodeCubeMapToStreamAsync(Image<Rgba32> right, Image<Rgba32> left, Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, Stream outputStream, CancellationToken token = default)
		{
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
		}

		/// <summary>
		/// Calculates the number of mipmap levels that will be generated for the given input image.
		/// </summary>
		/// <param name="inputImage">The image to use for the calculation.</param>
		/// <returns>The number of mipmap levels that will be generated for the input image.</returns>
		public int CalculateNumberOfMipLevels(Image<Rgba32> inputImage)
		{
			
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
			
		}
	}
}
