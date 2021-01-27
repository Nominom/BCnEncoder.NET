using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Encoder.Bc7;
using BCnEncoder.Encoder.Options;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using Microsoft.Toolkit.HighPerformance.Extensions;
using Microsoft.Toolkit.HighPerformance.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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

		#region Async Api

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream asynchronously.
		/// </summary>
		/// <param name="inputPixelsRgba">The input image rgba data.</param>
		/// <param name="width">The width of the input image.</param>
		/// <param name="height">The height of the input image.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		public Task EncodeToStreamAsync<T>(Memory<T> inputPixelsRgba, int width, int height, Stream outputStream, CancellationToken token = default) where T : unmanaged
		{
			return Task.Run(() =>
			{
				if (Unsafe.SizeOf<T>() != 4 || Unsafe.SizeOf<T>() != 1)
				{
					throw new ArgumentException(
						"The input pixels should be given in either byte format or a 4-byte rgba wrapper");
				}
				var bytes = MemoryMarshal.AsBytes(inputPixelsRgba.Span);
				EncodeToStreamInternal(bytes, width, height, outputStream, token);
			}, token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file asynchronously.
		/// </summary>
		/// <param name="inputPixelsRgba">The input image rgba data.</param>
		/// <param name="width">The width of the input image.</param>
		/// <param name="height">The height of the input image.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public Task<KtxFile> EncodeToKtxAsync<T>(Memory<T> inputPixelsRgba, int width, int height, CancellationToken token = default) where T : unmanaged
		{
			return Task.Run(() =>
			{
				if (Unsafe.SizeOf<T>() != 4 || Unsafe.SizeOf<T>() != 1)
				{
					throw new ArgumentException(
						"The input pixels should be given in either byte format or a 4-byte rgba wrapper");
				}
				var bytes = MemoryMarshal.AsBytes(inputPixelsRgba.Span);
				return EncodeToKtxInternal(bytes, width, height, token);
			}, token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file asynchronously.
		/// </summary>
		/// <param name="inputPixelsRgba">The input image rgba data.</param>
		/// <param name="width">The width of the input image.</param>
		/// <param name="height">The height of the input image.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public Task<DdsFile> EncodeToDdsAsync<T>(Memory<T> inputPixelsRgba, int width, int height, CancellationToken token = default) where T : unmanaged
		{
			return Task.Run(() =>
			{
				if (Unsafe.SizeOf<T>() != 4 || Unsafe.SizeOf<T>() != 1)
				{
					throw new ArgumentException(
						"The input pixels should be given in either byte format or a 4-byte rgba wrapper");
				}
				var bytes = MemoryMarshal.AsBytes(inputPixelsRgba.Span);
				return EncodeToDdsInternal(bytes, width, height, token);
			}, token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a list of byte buffers asynchronously.
		/// </summary>
		/// <param name="inputPixelsRgba">The input image rgba data.</param>
		/// <param name="width">The width of the input image.</param>
		/// <param name="height">The height of the input image.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>A list of raw encoded mipmap data.</returns>
		public Task<IList<byte[]>> EncodeToRawBytesAsync<T>(Memory<T> inputPixelsRgba, int width, int height, CancellationToken token = default) where T : unmanaged
		{
			return Task.Run(() =>
			{
				if (Unsafe.SizeOf<T>() != 4 || Unsafe.SizeOf<T>() != 1)
				{
					throw new ArgumentException(
						"The input pixels should be given in either byte format or a 4-byte rgba wrapper");
				}
				var bytes = MemoryMarshal.AsBytes(inputPixelsRgba.Span);
				return EncodeToRawBytesInternal(bytes, width, height, token);
			}, token);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer asynchronously.
		/// </summary>
		/// <param name="inputPixelsRgba">The input image rgba data.</param>
		/// <param name="width">The width of the input image.</param>
		/// <param name="height">The height of the input image.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The raw encoded data.</returns>
		/// <remarks>To get the width and height of the encoded mipLevel, see <see cref="CalculateMipMapSize(Image{Rgba32},int,out int,out int)"/>.</remarks>
		public Task<byte[]> EncodeToRawBytesAsync<T>(Memory<T> inputPixelsRgba, int width, int height, int mipLevel, CancellationToken token = default) where T : unmanaged
		{
			return Task.Run(() =>
			{
				if (Unsafe.SizeOf<T>() != 4 || Unsafe.SizeOf<T>() != 1)
				{
					throw new ArgumentException(
						"The input pixels should be given in either byte format or a 4-byte rgba wrapper");
				}
				var bytes = MemoryMarshal.AsBytes(inputPixelsRgba.Span);
				return EncodeToRawBytesInternal(bytes, width, height, mipLevel, out _,
						out _, token);
			}, token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream asynchronously.
		/// </summary>
		/// <param name="inputPixelsRgba">The input image rgba data.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		public Task EncodeToStreamAsync<T>(Memory2D<T> inputPixelsRgba, Stream outputStream, CancellationToken token = default) where T : unmanaged
		{
			return Task.Run(() =>
			{
				if (Unsafe.SizeOf<T>() != 4 || Unsafe.SizeOf<T>() != 1)
				{
					throw new ArgumentException(
						"The input pixels should be given in either byte format or a 4-byte rgba wrapper");
				}
				if (!inputPixelsRgba.Span.TryGetSpan(out var span))
				{
					throw new ArgumentException("Unable to get a Span out of Memory2D. Is the underlying buffer contiguous?");
				}
				var bytes = MemoryMarshal.AsBytes(span);
				var width = inputPixelsRgba.Width;
				var height = inputPixelsRgba.Height;
				EncodeToStreamInternal(bytes, width, height, outputStream, token);
			}, token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file asynchronously.
		/// </summary>
		/// <param name="inputPixelsRgba">The input image rgba data.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public Task<KtxFile> EncodeToKtxAsync<T>(Memory2D<T> inputPixelsRgba, CancellationToken token = default) where T : unmanaged
		{
			return Task.Run(() =>
			{
				if (Unsafe.SizeOf<T>() != 4 || Unsafe.SizeOf<T>() != 1)
				{
					throw new ArgumentException(
						"The input pixels should be given in either byte format or a 4-byte rgba wrapper");
				}
				if (!inputPixelsRgba.Span.TryGetSpan(out var span))
				{
					throw new ArgumentException("Unable to get a Span out of Memory2D. Is the underlying buffer contiguous?");
				}
				var bytes = MemoryMarshal.AsBytes(span);
				var width = inputPixelsRgba.Width;
				var height = inputPixelsRgba.Height;
				return EncodeToKtxInternal(bytes, width, height, token);
			}, token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file asynchronously.
		/// </summary>
		/// <param name="inputPixelsRgba">The input image rgba data.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public Task<DdsFile> EncodeToDdsAsync<T>(Memory2D<T> inputPixelsRgba, CancellationToken token = default) where T : unmanaged
		{
			return Task.Run(() =>
			{
				if (Unsafe.SizeOf<T>() != 4 || Unsafe.SizeOf<T>() != 1)
				{
					throw new ArgumentException(
						"The input pixels should be given in either byte format or a 4-byte rgba wrapper");
				}
				if (!inputPixelsRgba.Span.TryGetSpan(out var span))
				{
					throw new ArgumentException("Unable to get a Span out of Memory2D. Is the underlying buffer contiguous?");
				}
				var bytes = MemoryMarshal.AsBytes(span);
				var width = inputPixelsRgba.Width;
				var height = inputPixelsRgba.Height;
				return EncodeToDdsInternal(bytes, width, height, token);
			}, token);
		}

		/// <summary>
		/// Encodes all mipmap levels into a list of byte buffers asynchronously.
		/// </summary>
		/// <param name="inputPixelsRgba">The input image rgba data.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>A list of raw encoded mipmap data.</returns>
		public Task<IList<byte[]>> EncodeToRawBytesAsync<T>(Memory2D<T> inputPixelsRgba, CancellationToken token = default) where T : unmanaged
		{
			return Task.Run(() =>
			{
				if (Unsafe.SizeOf<T>() != 4 || Unsafe.SizeOf<T>() != 1)
				{
					throw new ArgumentException(
						"The input pixels should be given in either byte format or a 4-byte rgba wrapper");
				}
				if (!inputPixelsRgba.Span.TryGetSpan(out var span))
				{
					throw new ArgumentException("Unable to get a Span out of Memory2D. Is the underlying buffer contiguous?");
				}
				var bytes = MemoryMarshal.AsBytes(span);
				var width = inputPixelsRgba.Width;
				var height = inputPixelsRgba.Height;
				return EncodeToRawBytesInternal(bytes, width, height, token);
			}, token);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer asynchronously.
		/// </summary>
		/// <param name="inputPixelsRgba">The input image rgba data.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>The raw encoded data.</returns>
		/// <remarks>To get the width and height of the encoded mipLevel, see <see cref="CalculateMipMapSize(Image{Rgba32},int,out int,out int)"/>.</remarks>
		public Task<byte[]> EncodeToRawBytesAsync<T>(Memory2D<T> inputPixelsRgba, int mipLevel, CancellationToken token = default) where T : unmanaged
		{
			return Task.Run(() =>
			{
				if (Unsafe.SizeOf<T>() != 4 || Unsafe.SizeOf<T>() != 1)
				{
					throw new ArgumentException(
						"The input pixels should be given in either byte format or a 4-byte rgba wrapper");
				}
				if (!inputPixelsRgba.Span.TryGetSpan(out var span))
				{
					throw new ArgumentException("Unable to get a Span out of Memory2D. Is the underlying buffer contiguous?");
				}
				var bytes = MemoryMarshal.AsBytes(span);
				var width = inputPixelsRgba.Width;
				var height = inputPixelsRgba.Height;
				return EncodeToRawBytesInternal(bytes, width, height, mipLevel, out _,
						out _, token);
			}, token);
		}

		#endregion

		#region Sync Api

		/// <summary>
		/// Encodes all mipmap levels into a ktx or a dds file and writes it to the output stream.
		/// </summary>
		/// <param name="input">The data to encode represented by a <see cref="Memory2D{T}"/>.</param>
		/// <param name="outputStream">The stream to write the encoded image to.</param>
		public void EncodeToStream(Memory2D<ColorRgba32> input, Stream outputStream)
		{
			EncodeToStreamInternal(input, outputStream, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Ktx file.
		/// </summary>
		/// <param name="input">The data to encode represented by a <see cref="Memory2D{T}"/>.</param>
		/// <returns>The Ktx file containing the encoded image.</returns>
		public KtxFile EncodeToKtx(Memory2D<ColorRgba32> input)
		{
			return EncodeToKtxInternal(input, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a Dds file.
		/// </summary>
		/// <param name="input">The data to encode represented by a <see cref="Memory2D{T}"/>.</param>
		/// <returns>The Dds file containing the encoded image.</returns>
		public DdsFile EncodeToDds<T>(Memory2D<ColorRgba32> input)
		{
			return EncodeToDdsInternal(input, default);
		}

		/// <summary>
		/// Encodes all mipmap levels into a list of byte buffers.
		/// </summary>
		/// <param name="input">The data to encode represented by a <see cref="Memory2D{T}"/>.</param>
		/// <returns>A list of raw encoded mipmap data.</returns>
		public byte[][] EncodeToRawBytes(Memory2D<ColorRgba32> input)
		{
			return EncodeToRawBytesInternal(input, default);
		}

		/// <summary>
		/// Encodes a single mip level of the input image to a byte buffer.
		/// </summary>
		/// <param name="input">The data to encode represented by a <see cref="Memory2D{T}"/>.</param>
		/// <param name="mipLevel">The mipmap to encode.</param>
		/// <param name="mipWidth">The width of the mipmap.</param>
		/// <param name="mipHeight">The height of the mipmap.</param>
		/// <returns>The raw encoded data.</returns>
		public byte[] EncodeToRawBytes(Memory2D<ColorRgba32> input, int mipLevel, out int mipWidth, out int mipHeight)
		{
			return EncodeToRawBytesInternal(input, mipLevel, out mipWidth, out mipHeight, default);
		}

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

		private void EncodeToStreamInternal(Memory<ColorRgba32> input, int width, int height, Stream outputStream, CancellationToken token)
		{
			EncodeToStreamInternal(input.AsMemory2D(width, height), outputStream, token);
		}

		private void EncodeToStreamInternal(Memory2D<ColorRgba32> input, Stream outputStream, CancellationToken token)
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

		private KtxFile EncodeToKtxInternal(Memory<ColorRgba32> input, int width, int height, CancellationToken token)
		{
			return EncodeToKtxInternal(input.AsMemory2D(width, height), token);
		}

		private KtxFile EncodeToKtxInternal(Memory2D<ColorRgba32> input, CancellationToken token)
		{
			KtxFile output;
			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(input, ref numMipMaps);

			// Setup encoders
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetEncoder(OutputOptions.Format);
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

		private DdsFile EncodeToDdsInternal(Memory<ColorRgba32> input, int width, int height, CancellationToken token)
		{
			return EncodeToDdsInternal(input.AsMemory2D(width, height), token);
		}

		private DdsFile EncodeToDdsInternal(Memory2D<ColorRgba32> input, CancellationToken token)
		{
			DdsFile output;
			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(input, ref numMipMaps);

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}

				var (ddsHeader, dxt10Header) = DdsHeader.InitializeCompressed(input.Width, input.Height,
					compressedEncoder.GetDxgiFormat());
				output = new DdsFile(ddsHeader, dxt10Header);

				if (OutputOptions.DdsBc1WriteAlphaFlag &&
					OutputOptions.Format == CompressionFormat.Bc1WithAlpha)
				{
					output.header.ddsPixelFormat.dwFlags |= PixelFormatFlags.DdpfAlphapixels;
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

		private IList<byte[]> EncodeToRawInternal(Memory<ColorRgba32> input, int width, int height, CancellationToken token)
		{
			return EncodeToRawInternal(input.AsMemory2D(width, height), token);
		}

		private IList<byte[]> EncodeToRawInternal(Memory2D<ColorRgba32> input, CancellationToken token)
		{
			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(input, ref numMipMaps);

			var output = new List<byte[]>(numMipMaps);
			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();

			if (isCompressedFormat)
			{
				compressedEncoder = GetEncoder(OutputOptions.Format);
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

				output.Add(encoded);
			}

			return output;
		}

		private byte[] EncodeToRawInternal(Memory<ColorRgba32> input, int width, int height, int mipLevel, out int mipWidth, out int mipHeight, CancellationToken token)
		{
			return EncodeToRawInternal(input.AsMemory2D(width, height), mipLevel, out mipWidth, out mipHeight, token);
		}

		private byte[] EncodeToRawInternal(Memory2D<ColorRgba32> input, int mipLevel, out int mipWidth, out int mipHeight, CancellationToken token)
		{
			mipLevel = Math.Max(0, mipLevel);

			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipChain = MipMapper.GenerateMipChain(input, ref numMipMaps);

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetEncoder(OutputOptions.Format);
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

		private void EncodeCubeMapToStreamInternal(Memory<ColorRgba32> right, Memory<ColorRgba32> left, Memory<ColorRgba32> top, Memory<ColorRgba32> down,
			Memory<ColorRgba32> back, Memory<ColorRgba32> front, int width, int height, Stream outputStream, CancellationToken token)
		{
			EncodeCubeMapToStreamInternal(
				right.AsMemory2D(width, height), left.AsMemory2D(width, height),
				top.AsMemory2D(width, height), down.AsMemory2D(width, height),
				back.AsMemory2D(width, height), front.AsMemory2D(width, height),
				outputStream, token);
		}

		private void EncodeCubeMapToStreamInternal(Memory2D<ColorRgba32> right, Memory2D<ColorRgba32> left, Memory2D<ColorRgba32> top, Memory2D<ColorRgba32> down,
			Memory2D<ColorRgba32> back, Memory2D<ColorRgba32> front, Stream outputStream, CancellationToken token)
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

		private KtxFile EncodeCubeMapToKtxInternal(Memory<ColorRgba32> right, Memory<ColorRgba32> left, Memory<ColorRgba32> top, Memory<ColorRgba32> down,
			Memory<ColorRgba32> back, Memory<ColorRgba32> front, int width, int height, Stream outputStream, CancellationToken token)
		{
			return EncodeCubeMapToKtxInternal(
				right.AsMemory2D(width, height), left.AsMemory2D(width, height),
				top.AsMemory2D(width, height), down.AsMemory2D(width, height),
				back.AsMemory2D(width, height), front.AsMemory2D(width, height),
				outputStream, token);
		}

		private KtxFile EncodeCubeMapToKtxInternal(Memory2D<ColorRgba32> right, Memory2D<ColorRgba32> left, Memory2D<ColorRgba32> top, Memory2D<ColorRgba32> down,
			Memory2D<ColorRgba32> back, Memory2D<ColorRgba32> front, CancellationToken token)
		{
			KtxFile output;
			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			var faces = new[] { right, left, top, down, back, front };

			var width = right.Width;
			var height = right.Height;

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}

				output = new KtxFile(
					KtxHeader.InitializeCompressed(width,height,
						compressedEncoder.GetInternalFormat(),
						compressedEncoder.GetBaseInternalFormat()));
			}
			else
			{
				uncompressedEncoder = GetRawEncoder(OutputOptions.Format);
				output = new KtxFile(
					KtxHeader.InitializeUncompressed(width,height,
						uncompressedEncoder.GetGlType(),
						uncompressedEncoder.GetGlFormat(),
						uncompressedEncoder.GetGlTypeSize(),
						uncompressedEncoder.GetInternalFormat(),
						uncompressedEncoder.GetBaseInternalFormat()));

			}

			var numMipMaps = OutputOptions.GenerateMipMaps ? OutputOptions.MaxMipMapLevel : 1;
			var mipLength = MipMapper.CalculateMipChainLength(width,height, numMipMaps);
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
					MipMapper.CalculateMipLevelSize(width,height, mip, out var mipWidth, out var mipHeight);
					totalBlocks += isCompressedFormat ? ImageToBlocks.CalculateNumOfBlocks(mipWidth, mipHeight) : mipWidth * mipHeight;
				}
			}
			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			// Encode all faces
			var processedBlocks = 0;
			for (var face = 0; face < faces.Length; face++)
			{
				var mipChain = MipMapper.GenerateMipChain(faces[face], width, height, ref numMipMaps);

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

		private DdsFile EncodeCubeMapToDdsInternal(ReadOnlySpan<byte> right, ReadOnlySpan<byte> left, ReadOnlySpan<byte> top, ReadOnlySpan<byte> down,
			ReadOnlySpan<byte> back, ReadOnlySpan<byte> front, int width, int height, CancellationToken token)
		{
			DdsFile output;
			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;

			if (right.Length != width * height * 4)
			{
				throw new ArgumentException("The input pixels must be provided in 4 bytes per pixel rgba format.");
			}

			if (right.Length != left.Length || right.Length != top.Length || right.Length != down.Length
				|| right.Length != back.Length || right.Length != front.Length)
			{
				throw new ArgumentException("All input images of a cubemap should be the same size.");
			}

			var faces = new[] { right.ToArray(), left.ToArray(), top.ToArray(), down.ToArray(), back.ToArray(), front.ToArray() };

			// Setup encoder
			var isCompressedFormat = OutputOptions.Format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				compressedEncoder = GetEncoder(OutputOptions.Format);
				if (compressedEncoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {OutputOptions.Format}");
				}

				var (ddsHeader, dxt10Header) = DdsHeader.InitializeCompressed(width, height,
					compressedEncoder.GetDxgiFormat());
				output = new DdsFile(ddsHeader, dxt10Header);

				if (OutputOptions.DdsBc1WriteAlphaFlag &&
					OutputOptions.Format == CompressionFormat.Bc1WithAlpha)
				{
					output.header.ddsPixelFormat.dwFlags |= PixelFormatFlags.DdpfAlphapixels;
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
				var mipChain = MipMapper.GenerateMipChain(faces[face], width, height, ref numMipMaps);
				try
				{
					// Encode all mipmap levels per face
					for (var mip = 0; mip < numMipMaps; mip++)
					{
						byte[] encoded;
						if (isCompressedFormat)
						{
							var blocks = ImageToBlocks.ImageTo4X4(mipChain[mip].Frames[0], out var blocksWidth, out var blocksHeight);
							encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.Quality, context);

							processedBlocks += blocks.Length;
							context.Progress.SetProcessedBlocks(processedBlocks);
						}
						else
						{
							if (!mipChain[mip].TryGetSinglePixelSpan(out var mipPixels))
							{
								throw new Exception("Cannot get pixel span.");
							}

							encoded = uncompressedEncoder.Encode(mipPixels);

							processedBlocks += mipPixels.Length;
							context.Progress.SetProcessedBlocks(processedBlocks);
						}

						if (mip == 0)
						{
							output.Faces.Add(new DdsFace((uint)mipChain[mip].Width, (uint)mipChain[mip].Height,
								(uint)encoded.Length, mipChain.Count));
						}

						output.Faces[face].MipMaps[mip] = new DdsMipMap(encoded,
							(uint)mipChain[mip].Width,
							(uint)mipChain[mip].Height);
					}
				}
				finally
				{
					// Dispose all mipmap levels, even if operation was cancelled
					foreach (var image in mipChain)
						image.Dispose();
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

		#endregion

		#region Support

		private IBcBlockEncoder GetEncoder(CompressionFormat format)
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
					return new Bc4BlockEncoder(InputOptions.LuminanceAsRed);

				case CompressionFormat.Bc5:
					return new Bc5BlockEncoder();

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

		#endregion
	}
}
