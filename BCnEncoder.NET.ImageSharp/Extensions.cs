using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncoder.TextureFormats;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.ImageSharp
{
	public static class BCnTextureDataExtensions
	{
		/// <summary>
		/// Wrap a <see cref="BCnTextureData"/> single face and mipLevel as a <see cref="Image{TPixel}"/> of <see cref="Rgba32"/>.
		/// Make sure first that <see cref="BCnTextureData.Format"/> is <see cref="CompressionFormat.Rgba32"/>.
		/// The owner of the pixel memory is still <paramref name="data"/>,
		/// so modifying the data in the <see cref="Image{TPixel}"/> affects the original <see cref="BCnTextureData"/>.
		/// </summary>
		public static Image<Rgba32> AsImageRgba32(this BCnTextureData data, int mipLevel = 0, CubeMapFaceDirection faceDirection = CubeMapFaceDirection.XPositive)
			=> AsImage<Rgba32>(data, CompressionFormat.Rgba32, faceDirection, mipLevel);

		/// <summary>
		/// Wrap a <see cref="BCnTextureData"/> single face and mipLevel as a <see cref="Image{TPixel}"/> of <see cref="RgbaVector"/>.
		/// Make sure first that <see cref="BCnTextureData.Format"/> is <see cref="CompressionFormat.RgbaFloat"/>.
		/// The pixel data in the <see cref="BCnTextureData"/> is reused,
		/// so modifying the data in the <see cref="Image{TPixel}"/> affects the original <see cref="BCnTextureData"/>.
		/// </summary>
		public static Image<RgbaVector> AsImageRgbaVector(this BCnTextureData data, int mipLevel = 0,
			CubeMapFaceDirection faceDirection = CubeMapFaceDirection.XPositive)
			=> AsImage<RgbaVector>(data, CompressionFormat.RgbaFloat, faceDirection, mipLevel);

		private static Image<TPixel> AsImage<TPixel>(this BCnTextureData data, CompressionFormat expectedFormat,
			CubeMapFaceDirection faceDirection, int mipLevel)
			where TPixel : unmanaged, IPixel<TPixel>
		{
			if (data.Format != expectedFormat)
			{
				throw new ArgumentException(
					$"The provided {nameof(BCnTextureData)} is not in CompressionFormat.RgbaFloat. Use the {nameof(Shared.BCnTextureDataExtensions.ConvertTo)} method to convert it first!",
					nameof(data));
			}
			if (mipLevel >= data.NumMips)
			{
				throw new ArgumentException(
					$"The provided {nameof(BCnTextureData)} does not have the requested mipLevel ({mipLevel})",
					nameof(mipLevel));
			}
			if (faceDirection > 0 && !data.IsCubeMap)
			{
				throw new ArgumentException(
					$"The provided {nameof(BCnTextureData)} is not a cubeMap so it only has one face!",
					nameof(mipLevel));
			}

			var mip = data[faceDirection].Mips[mipLevel];

			return Image.WrapMemory<TPixel>(Configuration.Default, mip.AsMemory<TPixel>(), mip.Width, mip.Height);
		}
	}

	public static class ImageExtensions
	{
		/// <summary>
		/// Copy the contents of a <see cref="Image{TPixel}"/> of type <see cref="Rgba32"/> into <see cref="BCnTextureData"/>.
		/// The format of the data will be <see cref="CompressionFormat.Rgba32"/>.
		/// </summary>
		public static BCnTextureData ToBCnTextureData(this Image<Rgba32> image)
		{
			var bytes = new byte[image.Width * image.Height * 4L];
			image.CopyPixelDataTo(bytes);
			return new BCnTextureData(CompressionFormat.Rgba32, image.Width, image.Height, bytes);
		}

		/// <summary>
		/// Copy the contents of a <see cref="Image{TPixel}"/> of type <see cref="RgbaVector"/> into <see cref="BCnTextureData"/>.
		/// The format of the data will be <see cref="CompressionFormat.RgbaFloat"/>.
		/// </summary>
		public static BCnTextureData ToBCnTextureData(this Image<RgbaVector> image)
		{
			var bytes = new byte[image.Width * image.Height * 16L];
			image.CopyPixelDataTo(bytes);
			return new BCnTextureData(CompressionFormat.RgbaFloat, image.Width, image.Height, bytes);
		}

		/// <summary>
		/// Copy the contents of a <see cref="Image{TPixel}"/> of type <see cref="Rgb24"/> into <see cref="BCnTextureData"/>.
		/// The format of the data will be <see cref="CompressionFormat.Rgb24"/>.
		/// </summary>
		public static BCnTextureData ToBCnTextureData(this Image<Rgb24> image)
		{
			var bytes = new byte[image.Width * image.Height * 3L];
			image.CopyPixelDataTo(bytes);
			return new BCnTextureData(CompressionFormat.Rgb24, image.Width, image.Height, bytes);
		}
	}

	public static class EncoderExtensions
	{
		/// <inheritdoc cref="BcEncoder.Encode(BCnTextureData)"/>
		public static BCnTextureData Encode(this BcEncoder encoder, Image<Rgba32> image)
			=> encoder.Encode(image.ToBCnTextureData());

		/// <inheritdoc cref="BcEncoder.EncodeAsync(BCnTextureData, CancellationToken)"/>
		public static Task<BCnTextureData> EncodeAsync(this BcEncoder encoder, Image<Rgba32> image, CancellationToken token = default)
			=> encoder.EncodeAsync(image.ToBCnTextureData(), token);


		/// <inheritdoc cref="BcEncoder.Encode(BCnTextureData)"/>
		public static BCnTextureData EncodeHdr(this BcEncoder encoder, Image<RgbaVector> image)
			=> encoder.Encode(image.ToBCnTextureData());

		/// <inheritdoc cref="BcEncoder.EncodeAsync(BCnTextureData, CancellationToken)"/>
		public static Task<BCnTextureData> EncodeHdrAsync(this BcEncoder encoder, Image<RgbaVector> image, CancellationToken token = default)
			=> encoder.EncodeAsync(image.ToBCnTextureData(), token);

		/// <inheritdoc cref="BcEncoder.EncodeToRawBytes"/>
		public static byte[] EncodeToRawBytes(this BcEncoder encoder, Image<Rgba32> input, int mipLevel,
			out int mipWidth, out int mipHeight)
			=> encoder.EncodeToRawBytes(
				input.ToBCnTextureData().MipLevels[0].AsMemory2D<ColorRgba32>(),
				mipLevel,
				out mipWidth,
				out mipHeight);

		/// <inheritdoc cref="BcEncoder.EncodeToRawBytesHdr"/>
		public static byte[] EncodeToRawBytesHdr(this BcEncoder encoder, Image<RgbaVector> input, int mipLevel,
			out int mipWidth, out int mipHeight)
			=> encoder.EncodeToRawBytesHdr(
				input.ToBCnTextureData().MipLevels[0].AsMemory2D<ColorRgbaFloat>(),
				mipLevel,
				out mipWidth,
				out mipHeight);

		/// <inheritdoc cref="BcEncoder.EncodeToRawBytesAsync"/>
		public static Task<byte[]> EncodeToRawBytesAsync(this BcEncoder encoder, Image<Rgba32> input, int mipLevel,
			CancellationToken token = default)
			=> encoder.EncodeToRawBytesAsync(
				input.ToBCnTextureData().MipLevels[0].AsMemory2D<ColorRgba32>(),
				mipLevel, token);


		/// <inheritdoc cref="BcEncoder.EncodeToRawBytesHdrAsync"/>
		public static Task<byte[]> EncodeToRawBytesHdrAsync(this BcEncoder encoder, Image<RgbaVector> input, int mipLevel,
			CancellationToken token = default)
			=> encoder.EncodeToRawBytesHdrAsync(
				input.ToBCnTextureData().MipLevels[0].AsMemory2D<ColorRgbaFloat>(),
				mipLevel, token);

		/// <summary>
		/// Encodes all mipmap levels into a texture file container.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		public static TTextureFormat EncodeToTextureHdr<TTextureFormat>(this BcEncoder encoder, Image<RgbaVector> input)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = encoder.EncodeHdr(input);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file container.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		public static async Task<TTextureFormat> EncodeToTextureHdrAsync<TTextureFormat>(this BcEncoder encoder, Image<RgbaVector> input, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = await encoder.EncodeHdrAsync(input, token);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file container.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		public static TTextureFormat EncodeToTexture<TTextureFormat>(this BcEncoder encoder, Image<Rgba32> input)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = encoder.Encode(input);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file container.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		public static async Task<TTextureFormat> EncodeToTextureAsync<TTextureFormat>(this BcEncoder encoder, Image<Rgba32> input, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = await encoder.EncodeAsync(input, token);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file and writes it to the output stream.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static void EncodeToStreamHdr<TTextureFormat>(this BcEncoder encoder, Image<RgbaVector> input, Stream outputStream)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();
			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = encoder.EncodeHdr(input);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file and writes it to the output stream.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static async Task EncodeToStreamHdrAsync<TTextureFormat>(this BcEncoder encoder, Image<RgbaVector> input, Stream outputStream, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();
			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = await encoder.EncodeHdrAsync(input, token);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file and writes it to the output stream.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static void EncodeToStream<TTextureFormat>(this BcEncoder encoder, Image<Rgba32> input, Stream outputStream)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = encoder.Encode(input);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipmap levels into a texture file and writes it to the output stream.
		/// </summary>
		/// <param name="input">The input to encode represented by a <see cref="ReadOnlyMemory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static async Task EncodeToStreamAsync<TTextureFormat>(this BcEncoder encoder, Image<Rgba32> input, Stream outputStream, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = await encoder.EncodeAsync(input, token);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}






		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		public static TTextureFormat EncodeCubeMapToTextureHdr<TTextureFormat>(this BcEncoder encoder,
			Image<RgbaVector> right, Image<RgbaVector> left,
			Image<RgbaVector> top, Image<RgbaVector> down,
			Image<RgbaVector> back, Image<RgbaVector> front)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = encoder.Encode(BuildCubeMap(right, left, top, down, back, front));
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		public static async Task<TTextureFormat> EncodeCubeMapToTextureHdrAsync<TTextureFormat>(this BcEncoder encoder,
			Image<RgbaVector> right, Image<RgbaVector> left,
			Image<RgbaVector> top, Image<RgbaVector> down,
			Image<RgbaVector> back, Image<RgbaVector> front, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = await encoder.EncodeAsync(BuildCubeMap(right, left, top, down, back, front), token);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		public static TTextureFormat EncodeCubeMapToTexture<TTextureFormat>(this BcEncoder encoder,
			Image<Rgba32> right, Image<Rgba32> left,
			Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = encoder.Encode(BuildCubeMap(right, left, top, down, back, front));
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		public static async Task<TTextureFormat> EncodeCubeMapToTextureAsync<TTextureFormat>(this BcEncoder encoder,
			Image<Rgba32> right, Image<Rgba32> left,
			Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = await encoder.EncodeAsync(BuildCubeMap(right, left, top, down, back, front), token);
			tex.FromTextureData(encoded);
			return tex;
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static void EncodeCubeMapToStreamHdr<TTextureFormat>(this BcEncoder encoder,
			Image<RgbaVector> right, Image<RgbaVector> left,
			Image<RgbaVector> top, Image<RgbaVector> down,
			Image<RgbaVector> back, Image<RgbaVector> front, Stream outputStream)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = encoder.Encode(BuildCubeMap(right, left, top, down, back, front));
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static async Task EncodeCubeMapToStreamHdrAsync<TTextureFormat>(this BcEncoder encoder,
			Image<RgbaVector> right, Image<RgbaVector> left,
			Image<RgbaVector> top, Image<RgbaVector> down,
			Image<RgbaVector> back, Image<RgbaVector> front, Stream outputStream, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.SupportsHdr)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support HDR formats!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = await encoder.EncodeAsync(BuildCubeMap(right, left, top, down, back, front), token);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static void EncodeCubeMapToStream<TTextureFormat>(this BcEncoder encoder,
			Image<Rgba32> right, Image<Rgba32> left,
			Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, Stream outputStream)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}

			var encoded = encoder.Encode(BuildCubeMap(right, left, top, down, back, front));
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		/// <summary>
		/// Encodes all mipMaps of a cubeMap to a stream either in the specified texture format.
		/// Order of faces is +X, -X, +Y, -Y, +Z, -Z. Back maps to positive Z and front to negative Z.
		/// </summary>
		/// <param name="right">The positive X-axis face of the cubeMap</param>
		/// <param name="left">The negative X-axis face of the cubeMap</param>
		/// <param name="top">The positive Y-axis face of the cubeMap</param>
		/// <param name="down">The negative Y-axis face of the cubeMap</param>
		/// <param name="back">The positive Z-axis face of the cubeMap</param>
		/// <param name="front">The negative Z-axis face of the cubeMap</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public static async Task EncodeCubeMapToStreamAsync<TTextureFormat>(this BcEncoder encoder,
			Image<Rgba32> right, Image<Rgba32> left,
			Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front, Stream outputStream, CancellationToken token = default)
			where TTextureFormat : class, ITextureFileFormat<TTextureFormat>, new()
		{
			var tex = new TTextureFormat();

			if (!tex.SupportsCubeMap)
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support cubemaps!");
			}
			if (!tex.IsSupportedFormat(encoder.OutputOptions.Format))
			{
				throw new InvalidOperationException(
					$"{typeof(TTextureFormat).Name} does not support the {encoder.OutputOptions.Format} format!");
			}
			var encoded = await encoder.EncodeAsync(BuildCubeMap(right, left, top, down, back, front), token);
			tex.FromTextureData(encoded);
			tex.WriteToStream(outputStream);
		}

		private static BCnTextureData BuildCubeMap(
			Image<Rgba32> right, Image<Rgba32> left,
			Image<Rgba32> top, Image<Rgba32> down,
			Image<Rgba32> back, Image<Rgba32> front)
		{
			var data = new BCnTextureData(CompressionFormat.Rgba32, right.Width, right.Height, 1, true, true);

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

			right.CopyPixelDataTo(data.Faces[0].Mips[0].Data);
			left.CopyPixelDataTo(data.Faces[1].Mips[0].Data);
			top.CopyPixelDataTo(data.Faces[2].Mips[0].Data);
			down.CopyPixelDataTo(data.Faces[3].Mips[0].Data);
			back.CopyPixelDataTo(data.Faces[4].Mips[0].Data);
			front.CopyPixelDataTo(data.Faces[5].Mips[0].Data);

			return data;
		}

		private static BCnTextureData BuildCubeMap(
			Image<RgbaVector> right, Image<RgbaVector> left,
			Image<RgbaVector> top,   Image<RgbaVector> down,
			Image<RgbaVector> back,  Image<RgbaVector> front)
		{
			var data = new BCnTextureData(CompressionFormat.RgbaFloat, right.Width, right.Height, 1, true, true);

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

			right.CopyPixelDataTo(data.Faces[0].Mips[0].Data);
			left.CopyPixelDataTo(data.Faces[1].Mips[0].Data);
			top.CopyPixelDataTo(data.Faces[2].Mips[0].Data);
			down.CopyPixelDataTo(data.Faces[3].Mips[0].Data);
			back.CopyPixelDataTo(data.Faces[4].Mips[0].Data);
			front.CopyPixelDataTo(data.Faces[5].Mips[0].Data);

			return data;
		}
	}
}
