using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using Microsoft.Toolkit.HighPerformance.Extensions;
using Microsoft.Toolkit.HighPerformance.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.NET.ImageSharp
{
	public static class BCnDecoderExtensions
	{

		/// <summary>
		/// Decode raw encoded image data.
		/// </summary>
		/// <param name="inputStream">The stream containing the encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static Image<Rgba32> DecodeRawToImageRgba32(this BcDecoder decoder, Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			return ColorMemoryToImage(decoder.DecodeRaw2D(inputStream, pixelWidth, pixelHeight, format));
		}

		/// <summary>
		/// Decode raw encoded image data.
		/// </summary>
		/// <param name="input">The array containing the encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static Image<Rgba32> DecodeRawToImageRgba32(this BcDecoder decoder, byte[] input, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			return ColorMemoryToImage(decoder.DecodeRaw2D(input, pixelWidth, pixelHeight, format));
		}

		/// <summary>
		/// Read a Ktx file and decode it.
		/// </summary>
		/// <param name="inputStream">The stream that contains either ktx or dds file.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static Image<Rgba32> DecodeToImageRgba32(this BcDecoder decoder, Stream inputStream)
		{
			return ColorMemoryToImage(decoder.Decode2D(inputStream));
		}

		/// <summary>
		/// Read a Ktx file and decode it.
		/// </summary>
		/// <param name="inputStream">The stream that contains either ktx or dds file.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		public static Image<Rgba32>[] DecodeAllMipMapsToImageRgba32(this BcDecoder decoder, Stream inputStream)
		{
			var decoded = decoder.DecodeAllMipMaps2D(inputStream);
			var output = new Image<Rgba32>[decoded.Length];
			for (var i = 0; i < decoded.Length; i++)
			{
				output[i] = ColorMemoryToImage(decoded[i]);
			}
			return output;
		}


		/// <summary>
		/// Read a Ktx file and decode it.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static Image<Rgba32> DecodeToImageRgba32(this BcDecoder decoder, KtxFile file)
		{
			return ColorMemoryToImage(decoder.Decode2D(file));
		}

		/// <summary>
		/// Read a Ktx file and decode it.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		public static Image<Rgba32>[] DecodeAllMipMapsToImageRgba32(this BcDecoder decoder, KtxFile file)
		{
			var decoded = decoder.DecodeAllMipMaps2D(file);
			var output = new Image<Rgba32>[decoded.Length];
			for (var i = 0; i < decoded.Length; i++)
			{
				output[i] = ColorMemoryToImage(decoded[i]);
			}
			return output;
		}

		/// <summary>
		/// Read a Dds file and decode it.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static Image<Rgba32> DecodeToImageRgba32(this BcDecoder decoder, DdsFile file)
		{
			return ColorMemoryToImage(decoder.Decode2D(file));
		}

		/// <summary>
		/// Read a Dds file and decode it.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		public static Image<Rgba32>[] DecodeAllMipMapsToImageRgba32(this BcDecoder decoder, DdsFile file)
		{
			var decoded = decoder.DecodeAllMipMaps2D(file);
			var output = new Image<Rgba32>[decoded.Length];
			for (var i = 0; i < decoded.Length; i++)
			{
				output[i] = ColorMemoryToImage(decoded[i]);
			}
			return output;
		}



		/// <summary>
		/// Decode raw encoded image data.
		/// </summary>
		/// <param name="inputStream">The stream containing the encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static async Task<Image<Rgba32>> DecodeRawToImageRgba32Async(this BcDecoder decoder, Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			return ColorMemoryToImage(await decoder.DecodeRaw2DAsync(inputStream, pixelWidth, pixelHeight, format, token));
		}

		/// <summary>
		/// Decode raw encoded image data.
		/// </summary>
		/// <param name="input">The array containing the encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static async Task<Image<Rgba32>> DecodeRawToImageRgba32Async(this BcDecoder decoder, byte[] input, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token = default)
		{
			return ColorMemoryToImage(await decoder.DecodeRaw2DAsync(input, pixelWidth, pixelHeight, format, token));
		}

		/// <summary>
		/// Read a Ktx file and decode it.
		/// </summary>
		/// <param name="inputStream">The stream that contains either ktx or dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static async Task<Image<Rgba32>> DecodeToImageRgba32Async(this BcDecoder decoder, Stream inputStream, CancellationToken token = default)
		{
			return ColorMemoryToImage(await decoder.Decode2DAsync(inputStream, token));
		}

		/// <summary>
		/// Read a Ktx file and decode it.
		/// </summary>
		/// <param name="inputStream">The stream that contains either ktx or dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		public static async Task<Image<Rgba32>[]> DecodeAllMipMapsToImageRgba32Async(this BcDecoder decoder, Stream inputStream, CancellationToken token = default)
		{
			var decoded = await decoder.DecodeAllMipMaps2DAsync(inputStream, token);
			var output = new Image<Rgba32>[decoded.Length];
			for (var i = 0; i < decoded.Length; i++)
			{
				output[i] = ColorMemoryToImage(decoded[i]);
			}
			return output;
		}


		/// <summary>
		/// Read a Ktx file and decode it.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static async Task<Image<Rgba32>> DecodeToImageRgba32Async(this BcDecoder decoder, KtxFile file, CancellationToken token = default)
		{
			return ColorMemoryToImage(await decoder.Decode2DAsync(file, token));
		}

		/// <summary>
		/// Read a Ktx file and decode it.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		public static async Task<Image<Rgba32>[]> DecodeAllMipMapsToImageRgba32Async(this BcDecoder decoder, KtxFile file, CancellationToken token = default)
		{
			var decoded = await decoder.DecodeAllMipMaps2DAsync(file, token);
			var output = new Image<Rgba32>[decoded.Length];
			for (var i = 0; i < decoded.Length; i++)
			{
				output[i] = ColorMemoryToImage(decoded[i]);
			}
			return output;
		}

		/// <summary>
		/// Read a Dds file and decode it.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static async Task<Image<Rgba32>> DecodeToImageRgba32Async(this BcDecoder decoder, DdsFile file, CancellationToken token = default)
		{
			return ColorMemoryToImage(await decoder.Decode2DAsync(file, token));
		}

		/// <summary>
		/// Read a Dds file and decode it.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		public static async Task<Image<Rgba32>[]> DecodeAllMipMapsToImageRgba32Async(this BcDecoder decoder, DdsFile file, CancellationToken token = default)
		{
			var decoded = await decoder.DecodeAllMipMaps2DAsync(file, token);
			var output = new Image<Rgba32>[decoded.Length];
			for (var i = 0; i < decoded.Length; i++)
			{
				output[i] = ColorMemoryToImage(decoded[i]);
			}
			return output;
		}



		private static Image<Rgba32> ColorMemoryToImage(Memory2D<ColorRgba32> colors)
		{
			var output = new Image<Rgba32>(colors.Width, colors.Height);
			output.TryGetSinglePixelSpan(out var pixels);
			colors.Span.TryGetSpan(out var decodedPixels);

			for (var i = 0; i < pixels.Length; i++)
			{
				var c = decodedPixels[i];
				pixels[i] = new Rgba32(c.r, c.g, c.b, c.a);
			}
			return output;
		}
	}
}
