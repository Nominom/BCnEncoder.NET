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
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.ImageSharp
{
	public static class BCnDecoderExtensions
	{

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method will read the expected amount of bytes from the given input stream and decode it.
		/// Make sure there is no file header information left in the stream before the encoded data.
		/// </summary>
		/// <param name="inputStream">The stream containing the encoded image.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The format the encoded data is in.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static Image<Rgba32> DecodeRawToImageRgba32(this BcDecoder decoder, Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			return ColorMemoryToImage(decoder.DecodeRaw2D(inputStream, pixelWidth, pixelHeight, format));
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// </summary>
		/// <param name="input">The array containing the encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The format the encoded data is in.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static Image<Rgba32> DecodeRawToImageRgba32(this BcDecoder decoder, byte[] input, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			return ColorMemoryToImage(decoder.DecodeRaw2D(input, pixelWidth, pixelHeight, format));
		}

		/// <summary>
		/// Read a Ktx or Dds file from a stream and decode the main image from it.
		/// The type of file will be detected automatically.
		/// </summary>
		/// <param name="inputStream">The stream that contains either ktx or dds file.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static Image<Rgba32> DecodeToImageRgba32(this BcDecoder decoder, Stream inputStream)
		{
			return ColorMemoryToImage(decoder.Decode2D(inputStream));
		}

		/// <summary>
		/// Read a Ktx or Dds file from a stream and decode all available mipmaps from it.
		/// The type of file will be detected automatically.
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
		/// Decode the main image from a Ktx file.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static Image<Rgba32> DecodeToImageRgba32(this BcDecoder decoder, KtxFile file)
		{
			return ColorMemoryToImage(decoder.Decode2D(file));
		}

		/// <summary>
		/// Decode all available mipmaps from a Ktx file.
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
		/// Decode the main image from a Dds file.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static Image<Rgba32> DecodeToImageRgba32(this BcDecoder decoder, DdsFile file)
		{
			return ColorMemoryToImage(decoder.Decode2D(file));
		}

		/// <summary>
		/// Decode all available mipmaps from a Dds file.
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
		/// Decode a single encoded image from raw bytes.
		/// This method will read the expected amount of bytes from the given input stream and decode it.
		/// Make sure there is no file header information left in the stream before the encoded data.
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
		/// Decode a single encoded image from raw bytes.
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
		/// Read a Ktx or Dds file from a stream and decode the main image from it.
		/// The type of file will be detected automatically.
		/// </summary>
		/// <param name="inputStream">The stream that contains either ktx or dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static async Task<Image<Rgba32>> DecodeToImageRgba32Async(this BcDecoder decoder, Stream inputStream, CancellationToken token = default)
		{
			return ColorMemoryToImage(await decoder.Decode2DAsync(inputStream, token));
		}

		/// <summary>
		/// Read a Ktx or Dds file from a stream and decode all available mipmaps from it.
		/// The type of file will be detected automatically.
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
		/// Decode the main image from a Ktx file.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static async Task<Image<Rgba32>> DecodeToImageRgba32Async(this BcDecoder decoder, KtxFile file, CancellationToken token = default)
		{
			return ColorMemoryToImage(await decoder.Decode2DAsync(file, token));
		}

		/// <summary>
		/// Decode all available mipmaps from a Ktx file.
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
		/// Decode the main image from a Dds file.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public static async Task<Image<Rgba32>> DecodeToImageRgba32Async(this BcDecoder decoder, DdsFile file, CancellationToken token = default)
		{
			return ColorMemoryToImage(await decoder.Decode2DAsync(file, token));
		}

		/// <summary>
		/// Decode all available mipmaps from a Dds file.
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
			for (var y = 0; y < colors.Height; y++)
			{
				var yPixels = output.Frames.RootFrame.PixelBuffer.DangerousGetRowSpan(y);
				var yColors = colors.Span.GetRowSpan(y);

				MemoryMarshal.Cast<ColorRgba32, Rgba32>(yColors).CopyTo(yPixels);
			}
			return output;
		}
	}
}
