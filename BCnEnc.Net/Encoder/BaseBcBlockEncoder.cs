using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Shared;
using Microsoft.Toolkit.HighPerformance;

namespace BCnEncoder.Encoder
{
	internal abstract class BaseBcBlockEncoder<TRawBlock, TEncodedBlock> : IBcBlockEncoder<TRawBlock>
		where TRawBlock: unmanaged
		where TEncodedBlock: unmanaged
	{
		private static readonly object lockObj = new object();

		public int GetBlockSize()
		{
			return Unsafe.SizeOf<TEncodedBlock>();
		}

		public abstract TEncodedBlock EncodeBlock(TRawBlock block, CompressionQuality quality);

		/// <inheritdoc />
		public abstract CompressionFormat EncodedFormat { get; }

		/// <inheritdoc />
		public abstract byte[] Encode(ReadOnlyMemory<ColorRgbaFloat> pixels, int width, int height,
			CompressionQuality quality, OperationContext context);

		public byte[] EncodeBlocks(TRawBlock[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, OperationContext context)
		{
			var outputData = new byte[blockWidth * blockHeight * Unsafe.SizeOf<TEncodedBlock>()];
			
			if (context.IsParallel)
			{
				var options = new ParallelOptions
				{
					CancellationToken = context.CancellationToken,
					MaxDegreeOfParallelism = context.TaskCount
				};
				Parallel.For(0, blocks.Length, options, i =>
				{
					var outputBlocks = MemoryMarshal.Cast<byte, TEncodedBlock>(outputData);
					outputBlocks[i] = EncodeBlock(blocks[i], quality);

					if (context.Progress != null)
					{
						lock (lockObj)
						{
							context.Progress.Report(1);
						}
					}
				});
			}
			else
			{
				var outputBlocks = MemoryMarshal.Cast<byte, TEncodedBlock>(outputData);
				for (var i = 0; i < blocks.Length; i++)
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					outputBlocks[i] = EncodeBlock(blocks[i], quality);

					context.Progress?.Report(1);
				}
			}

			return outputData;
		}

		public void EncodeBlock(TRawBlock block, CompressionQuality quality, Span<byte> output)
		{
			if (output.Length != Unsafe.SizeOf<TEncodedBlock>())
			{
				throw new Exception("Cannot encode block! Output buffer is not the correct size.");
			}
			var encoded = EncodeBlock(block, quality);
			MemoryMarshal.Cast<byte, TEncodedBlock>(output)[0] = encoded;
		}
	}
	internal abstract class BaseBcLdrBlockEncoder<TEncodedBlock> : BaseBcBlockEncoder<RawBlock4X4Rgba32, TEncodedBlock>, IBcLdrBlockEncoder
		where TEncodedBlock : unmanaged
	{
		/// <inheritdoc />
		public override byte[] Encode(
			ReadOnlyMemory<ColorRgbaFloat> pixels,
			int width,
			int height,
			CompressionQuality quality,
			OperationContext context)
			=> Encode(pixels.ConvertTo<ColorRgbaFloat, ColorRgba32>(), width, height, quality, context);

		/// <inheritdoc />
		public byte[] Encode(ReadOnlyMemory<ColorRgba32> pixels, int width, int height, CompressionQuality quality, OperationContext context)
		{
			var blocks = ImageToBlocks.ImageTo4X4(pixels.AsMemory2D(height, width), out var blocksWidth, out var blocksHeight);
			return EncodeBlocks(blocks, blocksWidth, blocksHeight, quality, context);
		}
	}

	internal abstract class BaseBcHdrBlockEncoder<TEncodedBlock> : BaseBcBlockEncoder<RawBlock4X4RgbFloat, TEncodedBlock>
		where TEncodedBlock : unmanaged
	{
		/// <inheritdoc />
		public override byte[] Encode(
			ReadOnlyMemory<ColorRgbaFloat> pixels,
			int width,
			int height,
			CompressionQuality quality,
			OperationContext context)
		{
			var memory2D = pixels.ConvertTo<ColorRgbaFloat, ColorRgbFloat>().AsMemory().AsMemory2D(height, width);
			var blocks = ImageToBlocks.ImageTo4X4(memory2D, out var blocksWidth, out var blocksHeight);
			return EncodeBlocks(blocks, blocksWidth, blocksHeight, quality, context);
		}
	}
}
