using BCnEncoder.Decoder.Options;
using BCnEncoder.Shared;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Shared.Colors;
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

		public BcDecoder() { }

		public BcDecoder(DecoderOptions options)
		{
			Options = options;
		}

		public BcDecoder(DecoderOutputOptions outputOptions)
		{
			OutputOptions = outputOptions;
		}

		public BcDecoder(DecoderOptions options, DecoderOutputOptions outputOptions)
		{
			Options = options;
			OutputOptions = outputOptions;
		}

		/// <inheritdoc cref="DecodeInternal"/>
		public Task<BCnTextureData> DecodeAsync(BCnTextureData texture, CompressionFormat outputFormat, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(texture, outputFormat, token), token);
		}

		/// <inheritdoc cref="DecodeInternal"/>
		public BCnTextureData Decode(BCnTextureData texture, CompressionFormat outputFormat)
		{
			return DecodeInternal(texture, outputFormat, CancellationToken.None);
		}

		/// <summary>
		/// Decode a single encoded image from raw bytes.
		/// This method will read the expected amount of bytes from the given input stream and decode it.
		/// Make sure there is no file header information left in the stream before the encoded data.
		/// </summary>
		public async Task DecodeRawAsync<TOut>(Stream inputStream, Memory<TOut> output, int pixelWidth, int pixelHeight, CompressionFormat inputFormat, CompressionFormat outputFormat, CancellationToken token = default)
			where TOut : unmanaged
		{
			var dataArray = new byte[GetBufferSize(inputFormat, pixelWidth, pixelHeight, 1)];
			await inputStream.ReadExactlyAsync(dataArray, 0, dataArray.Length, token);

			await Task.Run(() => DecodeRawInternal(dataArray, output.Cast<TOut, byte>(), pixelWidth, pixelHeight, 1, inputFormat, outputFormat, token), token);
		}

		public Task DecodeRawAsync<TOut>(ReadOnlyMemory<byte> input, Memory<TOut> output, int pixelWidth, int pixelHeight, CompressionFormat inputFormat, CompressionFormat outputFormat, CancellationToken token = default)
			where TOut : unmanaged
		{
			return Task.Run(() => DecodeRawInternal(input, output.Cast<TOut, byte>(), pixelWidth, pixelHeight, 1, inputFormat, outputFormat, token), token);
		}

		public async Task<TOut[]> DecodeRawAsync<TOut>(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat inputFormat, CompressionFormat outputFormat, CancellationToken token = default)
			where TOut : unmanaged
		{
			var dataArray = new byte[GetBufferSize(inputFormat, pixelWidth, pixelHeight, 1)];
			await inputStream.ReadExactlyAsync(dataArray, 0, dataArray.Length, token);

			var output = AllocateOutputBuffer<TOut>(pixelWidth, pixelHeight, 1, outputFormat);

			await Task.Run(() => DecodeRawInternal(dataArray, output.AsMemory().Cast<TOut, byte>(), pixelWidth, pixelHeight, 1, inputFormat, outputFormat, token), token);

			return output;
		}

		public async Task<TOut[]> DecodeRawAsync<TOut>(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat inputFormat, CompressionFormat outputFormat, CancellationToken token = default)
			where TOut : unmanaged
		{
			var output = AllocateOutputBuffer<TOut>(pixelWidth, pixelHeight, 1, outputFormat);

			await Task.Run(() => DecodeRawInternal(input, output.AsMemory().Cast<TOut, byte>(), pixelWidth, pixelHeight, 1, inputFormat, outputFormat, token), token);

			return output;
		}

		public void DecodeRaw<TOut>(Stream inputStream, Memory<TOut> output, int pixelWidth, int pixelHeight, CompressionFormat inputFormat, CompressionFormat outputFormat)
			where TOut : unmanaged
		{
			var dataArray = new byte[GetBufferSize(inputFormat, pixelWidth, pixelHeight, 1)];
			inputStream.ReadExactly(dataArray, 0, dataArray.Length);

			DecodeRawInternal(dataArray, output.Cast<TOut, byte>(), pixelWidth, pixelHeight, 1, inputFormat, outputFormat, CancellationToken.None);
		}

		public void DecodeRaw<TOut>(ReadOnlyMemory<byte> input, Memory<TOut> output, int pixelWidth, int pixelHeight, CompressionFormat inputFormat, CompressionFormat outputFormat)
			where TOut : unmanaged
		{
			DecodeRawInternal(input, output.Cast<TOut, byte>(), pixelWidth, pixelHeight, 1,inputFormat, outputFormat, CancellationToken.None);
		}

		public TOut[] DecodeRaw<TOut>(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat inputFormat, CompressionFormat outputFormat)
			where TOut : unmanaged
		{
			var dataArray = new byte[GetBufferSize(inputFormat, pixelWidth, pixelHeight, 1)];
			inputStream.ReadExactly(dataArray, 0, dataArray.Length);

			var output = AllocateOutputBuffer<TOut>(pixelWidth, pixelHeight, 1, outputFormat);

			DecodeRawInternal(dataArray, output.AsMemory().Cast<TOut, byte>(), pixelWidth, pixelHeight, 1, inputFormat, outputFormat, CancellationToken.None);

			return output;
		}

		public TOut[] DecodeRaw<TOut>(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat inputFormat, CompressionFormat outputFormat)
			where TOut : unmanaged
		{
			var output = AllocateOutputBuffer<TOut>(pixelWidth, pixelHeight, 1, outputFormat);

			DecodeRawInternal(input, output.AsMemory().Cast<TOut, byte>(), pixelWidth, pixelHeight, 1, inputFormat, outputFormat, CancellationToken.None);

			return output;
		}

		/// <summary>
		/// Decode a single block from raw bytes and write it to the given output span.
		/// Output span size must be exactly 4x4 and input Span size needs to equal the block size.
		/// To get the block size (in bytes) of the compression format used, see <see cref="GetBlockSize(BCnEncoder.Shared.CompressionFormat)"/>.
		/// </summary>
		/// <param name="blockData">The encoded block in bytes.</param>
		/// <param name="inputFormat">The compression format used.</param>
		/// <param name="outputSpan">The destination span of the decoded data.</param>
		public void DecodeBlock<TOut>(ReadOnlySpan<byte> blockData, Memory2D<TOut> output, CompressionFormat inputFormat, CompressionFormat outputFormat)
			where TOut : unmanaged
		{
			if (output.Width != 4 || output.Height != 4)
			{
				throw new ArgumentException($"Single block decoding needs an output span of exactly 4x4");
			}
			DecodeBlockInternal(blockData, output, inputFormat, outputFormat);
		}

		/// <summary>
		/// Decode a single block from a stream and write it to the given output span.
		/// Output span size must be exactly 4x4.
		/// </summary>
		/// <param name="inputStream">The stream to read encoded blocks from.</param>
		/// <param name="inputFormat">The compression format used.</param>
		/// <param name="output">The destination span of the decoded data.</param>
		/// <returns>The number of bytes read from the stream. Zero (0) if reached the end of stream.</returns>
		public int DecodeBlock<TOut>(Stream inputStream, Memory2D<TOut> output, CompressionFormat inputFormat, CompressionFormat outputFormat)
			where TOut : unmanaged
		{
			if (output.Width != 4 || output.Height != 4)
			{
				throw new ArgumentException($"Single block decoding needs an output span of exactly 4x4");
			}

			Span<byte> input = stackalloc byte[16];
			input = input.Slice(0, GetBlockSize(inputFormat));

			var bytesRead = inputStream.Read(input);

			if (bytesRead == 0)
			{
				return 0; //End of stream
			}

			if (bytesRead != input.Length)
			{
				throw new Exception("Input stream does not have enough data available for a full block.");
			}

			DecodeBlockInternal(input, output, inputFormat, outputFormat);
			return bytesRead;
		}

		/// <summary>
		/// Decode all faces and mipmaps of a <see cref="BCnTextureData"/> into <see cref="CompressionFormat.Rgba32"/> for ldr formats,
		/// and <see cref="CompressionFormat.RgbaFloat"/> for hdr formats.
		/// </summary>
		/// <param name="texture">The texture data to decode.</param>
		/// <param name="outputFormat">The output format.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>A new <see cref="BCnTextureData"/> containing the decoded data.</returns>
		private BCnTextureData DecodeInternal(BCnTextureData texture, CompressionFormat outputFormat, CancellationToken token)
		{
			if (!outputFormat.IsRawPixelFormat())
			{
				throw new NotSupportedException($"The given output format is not a raw pixel format: {outputFormat}");
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = Options.IsParallel,
				TaskCount = Options.TaskCount,
				ColorConversionMode = GetColorConversionInput(texture.Format)
			};

			var blockSize = texture.Format.GetBytesPerBlock();
			var totalBlocks = 0L;
			for (var m = 0; m < texture.NumMips; m++)
			{
				totalBlocks += texture.Mips[m].SizeInBytes / blockSize;
			}
			totalBlocks *= texture.NumFaces;
			totalBlocks *= texture.NumArrayElements;

			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			var decoder = GetDecoder(texture.Format);

			if (decoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {texture.Format}");
			}

			var outputAlphaChannelhint = OutputOptions.AlphaHandling == AlphaHandling.Unpremultiply
				? AlphaChannelHint.Straight
				: texture.AlphaChannelHint;

			var outputData = new BCnTextureData(outputFormat, texture.Width, texture.Height, texture.Depth,
				texture.NumMips, texture.NumArrayElements, texture.IsCubeMap, true, outputAlphaChannelhint);

			for (var m = 0; m < texture.NumMips; m++)
			{
				for (var f = 0; f < texture.NumFaces; f++)
				{
					for (var a = 0; a < texture.NumArrayElements; a++)
					{
						var data = texture.Mips[m][(CubeMapFaceDirection)f, a].Data;

						var decoded = decoder.Decode(data, texture.Mips[m].Width,
							texture.Mips[m].Height, context);

						// Get as span to process in-place
						var resultSpan = decoded.AsSpan().Cast<byte, ColorRgbaFloat>();

						// Convert to UNorm if needed
						if (texture.Format.IsSNormFormat() && outputFormat.IsUNormFormat() && OutputOptions.RescaleSnormToUnorm)
						{
							SNormToUNorm(resultSpan);
						}

						// Apply alpha handling if needed
						if (texture.Format.SupportsAlpha() && !texture.Format.IsHdrFormat())
						{
							if (OutputOptions.AlphaHandling == AlphaHandling.Unpremultiply &&
							    texture.AlphaChannelHint == AlphaChannelHint.Premultiplied)
							{

								// Unpremultiply alpha
								AlphaHandlingHelper.UnpremultiplyAlpha(resultSpan);
							}
						}

						ColorConversionMode outColorConversionMode = GetColorConversionOutput(texture.Format, outputFormat);

						ColorExtensions.InternalConvertToAsBytesFromBytes(decoded, outputData.Mips[m][(CubeMapFaceDirection)f, a].Data, CompressionFormat.RgbaFloat,
							outputFormat, outColorConversionMode);
					}
				}
			}

			return outputData;
		}

		/// <summary>
		/// Decode raw encoded image.
		/// </summary>
		/// <param name="input">The <see cref="Memory{T}"/> containing the encoded data.</param>
		/// <param name="pixelWidth">The width of the image.</param>
		/// <param name="pixelHeight">The height of the image.</param>
		/// <param name="pixelDepth">The depth of the image.</param>
		/// <param name="inputFormat">The Format the encoded data is in.</param>
		/// <param name="token">The cancellation token for this operation. May be default, if the operation is not asynchronous.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		private void DecodeRawInternal(ReadOnlyMemory<byte> input, Memory<byte> output, int pixelWidth, int pixelHeight, int pixelDepth, CompressionFormat inputFormat, CompressionFormat outputFormat, CancellationToken token)
		{
			if (input.Length != GetBufferSize(inputFormat, pixelWidth, pixelHeight, pixelDepth))
			{
				throw new ArgumentException("The size of the input buffer does not align with the compression format. Expected: " + GetBufferSize(inputFormat, pixelWidth, pixelHeight, pixelDepth) + ", Actual: " + input.Length);
			}
			if (output.Length != GetBufferSize(outputFormat, pixelWidth, pixelHeight, pixelDepth))
			{
				throw new ArgumentException("The size of the output buffer does not align with the output format. Expected: " + GetBufferSize(outputFormat, pixelWidth, pixelHeight, pixelDepth) + ", Actual: " + output.Length);
			}
			if (!outputFormat.IsRawPixelFormat())
			{
				throw new NotSupportedException($"The given output format is not a raw pixel format: {outputFormat}");
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = Options.IsParallel,
				TaskCount = Options.TaskCount,
				ColorConversionMode = GetColorConversionInput(inputFormat)
			};

			// Calculate total blocks
			var blockSize = GetBlockSize(inputFormat);
			var totalBlocks = input.Length / blockSize;

			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			var decoder = GetDecoder(inputFormat);

			if (decoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {inputFormat}");
			}

			var result = decoder.Decode(input, pixelWidth, pixelHeight, context);
			var resultSpan = result.AsSpan().Cast<byte, ColorRgbaFloat>();

			// Convert to UNorm if needed
			if (inputFormat.IsSNormFormat() && outputFormat.IsUNormFormat() && OutputOptions.RescaleSnormToUnorm)
			{
				SNormToUNorm(resultSpan);
			}

			// Apply alpha handling if needed
			if (inputFormat.SupportsAlpha() && !inputFormat.IsHdrFormat())
			{
				if (OutputOptions.AlphaHandling == AlphaHandling.Unpremultiply)
				{
					AlphaHandlingHelper.UnpremultiplyAlpha(resultSpan);
				}
			}

			ColorExtensions.InternalConvertToAsBytesFromBytes(result, output, CompressionFormat.RgbaFloat,
				outputFormat, GetColorConversionOutput(inputFormat, outputFormat));
		}

		private void DecodeBlockInternal<TOut>(ReadOnlySpan<byte> blockData, Memory2D<TOut> output, CompressionFormat inputFormat, CompressionFormat outputFormat)
			where TOut: unmanaged
		{
			if (!inputFormat.IsBlockCompressedFormat())
			{
				throw new NotSupportedException($"This Format is not a block-compressed format: {inputFormat}.");
			}
			var decoder = GetDecoder(inputFormat) as IBcBlockDecoder;
			if (decoder == null)
			{
				throw new NotSupportedException($"This Format is not supported: {inputFormat}");
			}
			if (blockData.Length != GetBlockSize(inputFormat))
			{
				throw new ArgumentException("The size of the input buffer does not align with the compression format.");
			}
			if (output.Height != 4 || output.Width != 4)
			{
				throw new ArgumentException("The output memory must be 4x4.");
			}
			if (Unsafe.SizeOf<TOut>() != outputFormat.GetBytesPerBlock())
			{
				throw new ArgumentException($"The size of {typeof(TOut).Name} does not match the output format!");
			}
			if (!outputFormat.IsRawPixelFormat())
			{
				throw new NotSupportedException($"The given output format is not a raw pixel format: {outputFormat}");
			}

			var rawBlock = decoder.DecodeBlock(blockData);
			var pixels = rawBlock.AsSpan.Cast<ColorRgbaFloat, byte>();

			byte[] floatBytes = new byte[16 * Unsafe.SizeOf<ColorRgbaFloat>()];
			pixels.CopyTo(floatBytes);

			byte [] outputBytes = new byte[16 * Unsafe.SizeOf<TOut>()];

			ColorExtensions.InternalConvertToAsBytesFromBytes(floatBytes, outputBytes, CompressionFormat.RgbaFloat,
				outputFormat, ColorConversionMode.None);

			Span<TOut> outSpan = outputBytes.AsSpan().Cast<byte, TOut>();

			outSpan.Slice(0, 4).CopyTo(output.Span.GetRowSpan(0));
			outSpan.Slice(4, 4).CopyTo(output.Span.GetRowSpan(1));
			outSpan.Slice(8, 4).CopyTo(output.Span.GetRowSpan(2));
			outSpan.Slice(12, 4).CopyTo(output.Span.GetRowSpan(3));
		}

		#region Support

		#region Get decoder
		private IBcDecoder GetDecoder(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R8:
					return new RawDecoder<ColorR8>();

				case CompressionFormat.R8S:
					return new RawDecoder<ColorR8S>();

				case CompressionFormat.R8G8:
					return new RawDecoder<ColorR8G8>();

				case CompressionFormat.R8G8S:
					return new RawDecoder<ColorR8G8S>();

				case CompressionFormat.R10G10B10A2_Packed:
					return new RawDecoder<ColorR10G10B10A2Packed>();

				case CompressionFormat.Rgb24:
				case CompressionFormat.Rgb24_sRGB:
					return new RawDecoder<ColorRgb24>();

				case CompressionFormat.Bgr24:
				case CompressionFormat.Bgr24_sRGB:
					return new RawDecoder<ColorBgr24>();

				case CompressionFormat.Rgba32:
				case CompressionFormat.Rgba32_sRGB:
					return new RawDecoder<ColorRgba32>();

				case CompressionFormat.Bgra32:
				case CompressionFormat.Bgra32_sRGB:
					return new RawDecoder<ColorBgra32>();

				case CompressionFormat.RgbaFloat:
					return new RawDecoder<ColorRgbaFloat>();

				case CompressionFormat.RgbaHalf:
					return new RawDecoder<ColorRgbaHalf>();

				case CompressionFormat.RgbFloat:
					return new RawDecoder<ColorRgbFloat>();

				case CompressionFormat.RgbHalf:
					return new RawDecoder<ColorRgbHalf>();

				case CompressionFormat.Rgbe32:
					return new RawDecoder<ColorRgbe>();

				case CompressionFormat.Xyze32:
					return new RawDecoder<ColorXyze>();

				case CompressionFormat.Bc1:
				case CompressionFormat.Bc1_sRGB:
					return new Bc1NoAlphaDecoder();

				case CompressionFormat.Bc1WithAlpha:
				case CompressionFormat.Bc1WithAlpha_sRGB:
					return new Bc1ADecoder();

				case CompressionFormat.Bc2:
				case CompressionFormat.Bc2_sRGB:
					return new Bc2Decoder();

				case CompressionFormat.Bc3:
				case CompressionFormat.Bc3_sRGB:
					return new Bc3Decoder();

				case CompressionFormat.Bc4:
					return new Bc4Decoder(OutputOptions.Bc4Component);

				case CompressionFormat.Bc5:
					return new Bc5Decoder(OutputOptions.Bc5Component1, OutputOptions.Bc5Component2, OutputOptions.Bc5ComponentCalculated);

				case CompressionFormat.Bc7:
				case CompressionFormat.Bc7_sRGB:
					return new Bc7Decoder();

				case CompressionFormat.Bc6S:
					return new Bc6SDecoder();

				case CompressionFormat.Bc6U:
					return new Bc6UDecoder();

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
		#endregion

		private ColorConversionMode GetColorConversionInput(CompressionFormat sourceFormat)
		{
			if (OutputOptions.OutputColorSpace == OutputColorSpaceTarget.KeepAsIs)
				return ColorConversionMode.None;

			// Determine input color space based on settings
			bool inputIsSrgb = sourceFormat.IsSRGBFormat();
			if (OutputOptions.InputColorSpace == InputColorSpaceAssumption.ForceSrgb)
			{
				inputIsSrgb = true;
			}
			else if (OutputOptions.InputColorSpace == InputColorSpaceAssumption.ForceLinear)
			{
				inputIsSrgb = false;
			}

			if (inputIsSrgb)
				return ColorConversionMode.SrgbToLinear;

			return ColorConversionMode.None;
		}

		private ColorConversionMode GetColorConversionOutput(CompressionFormat sourceFormat, CompressionFormat targetFormat)
		{
			CompressionFormat processFormat = CompressionFormat.RgbaFloat;

			bool sourceIsSrgb = sourceFormat.IsSRGBFormat();

			// Apply output color space preferences
			switch (OutputOptions.OutputColorSpace)
			{
				case OutputColorSpaceTarget.KeepAsIs:
					// No conversion needed
					return ColorConversionMode.None;

				case OutputColorSpaceTarget.Linear:
					return ColorConversionMode.None;

				case OutputColorSpaceTarget.Srgb:
					return ColorConversionMode.LinearToSrgb;

				case OutputColorSpaceTarget.Auto:
					// Auto mode determines the best conversion based on formats
					return processFormat.GetColorConversionMode(targetFormat);

				case OutputColorSpaceTarget.ProcessLinearPreserveColorSpace:
					return sourceIsSrgb ? ColorConversionMode.LinearToSrgb : ColorConversionMode.None;

				default:
					return ColorConversionMode.None;
			}
		}

		/// <summary>
		/// Get the size of blocks for the given compression format in bytes.
		/// </summary>
		/// <param name="format">The compression format used.</param>
		/// <returns>The size of a single block in bytes.</returns>
		public int GetBlockSize(CompressionFormat format)
		{
			return format.GetBytesPerBlock();
		}

		private long GetBufferSize(CompressionFormat format, int pixelWidth, int pixelHeight, int pixelDepth)
		{
			return format.CalculateMipByteSize(pixelWidth, pixelHeight, pixelDepth);
		}

		private static TOut[] AllocateOutputBuffer<TOut>(int pixelWidth, int pixelHeight, int pixelDepth, CompressionFormat outputFormat)
			where TOut : unmanaged
		{
			if (outputFormat.IsBlockCompressedFormat())
				throw new ArgumentException("The output format must be a raw pixel format.");

			var tOutSize = Unsafe.SizeOf<TOut>();
			var realTOutSize = outputFormat.GetBytesPerBlock();

			if (tOutSize > realTOutSize)
			{
				throw new ArgumentException($"SizeOf<TOut> must be smaller than or equal to {realTOutSize} bytes in size.");
			}
			if (realTOutSize % tOutSize != 0)
			{
				throw new ArgumentException($"SizeOf<TOut> must be a multiple of {realTOutSize} bytes in size.");
			}

			var tOutMultiplier = realTOutSize / tOutSize;

			var output = new TOut[pixelWidth * pixelHeight * pixelDepth * tOutMultiplier];
			return output;
		}

		private static void SNormToUNorm(Span<ColorRgbaFloat> colors)
		{
			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = new ColorRgbaFloat(
					colors[i].r * 0.5f + 0.5f,
					colors[i].g * 0.5f + 0.5f,
					colors[i].b * 0.5f + 0.5f
					);
			}
		}

		#endregion
	}
}
