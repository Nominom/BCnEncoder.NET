using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Encoder
{
	internal abstract class BaseBcBlockEncoder<TEncodedBlock, TEncodingContext> : IBcBlockEncoder
		where TEncodedBlock: unmanaged
		where TEncodingContext: struct, IBlockEncodingContext
	{
		private static readonly object lockObj = new object();

		public int GetBlockSize()
		{
			return Unsafe.SizeOf<TEncodedBlock>();
		}

		public abstract TEncodedBlock EncodeBlock(in TEncodingContext context);

		public virtual void EncodeBlocks(ReadOnlySpan<RawBlock4X4RgbaFloat> blocks, Span<TEncodedBlock> outputBlocks, OperationContext context)
		{
			for (var i = 0; i < blocks.Length; i++)
			{
				// EncodeBlock
				TEncodingContext encContext = BlockEncodingContextHelpers.Create<TEncodingContext>(blocks[i], context);
				outputBlocks[i] = EncodeBlock(encContext);
			}
		}

		/// <inheritdoc />
		public byte[] Encode(
			ReadOnlyMemory<ColorRgbaFloat> pixels,
			int width,
			int height,
			OperationContext context)
		{
			var blocks = ImageToBlocks.ImageTo4X4(pixels, width, height, out var blocksWidth, out var blocksHeight);
			return EncodeBlocks(blocks, blocksWidth, blocksHeight, context);
		}

		public byte[] EncodeBlocks(RawBlock4X4RgbaFloat[] blocks, int blockWidth, int blockHeight, OperationContext context)
		{
			var outputData = new byte[blockWidth * blockHeight * Unsafe.SizeOf<TEncodedBlock>()];

			if (context.IsParallel)
			{
				var options = new ParallelOptions
				{
					CancellationToken = context.CancellationToken,
					MaxDegreeOfParallelism = context.TaskCount
				};
				Parallel.For(0, blockHeight, options, y =>
				{
					var outputBlocks = MemoryMarshal.Cast<byte, TEncodedBlock>(outputData).Slice(y * blockWidth, blockWidth);
					var inputBlocks = blocks.AsSpan().Slice(y * blockWidth, blockWidth);

					EncodeBlocks(inputBlocks, outputBlocks, context);

					if (context.Progress != null)
					{
						lock (lockObj)
						{
							context.Progress.Report(blockWidth);
						}
					}
				});
			}
			else
			{
				for (var y = 0; y < blockHeight; y++)
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					var outputBlocks = MemoryMarshal.Cast<byte, TEncodedBlock>(outputData).Slice(y * blockWidth, blockWidth);
					var inputBlocks = blocks.AsSpan().Slice(y * blockWidth, blockWidth);

					EncodeBlocks(inputBlocks, outputBlocks, context);

					context.Progress?.Report(blockWidth);
				}
			}

			return outputData;
		}

		public void EncodeBlocks(ReadOnlySpan<RawBlock4X4RgbaFloat> blocks, Span<byte> output, OperationContext context)
		{
			if (output.Length != Unsafe.SizeOf<TEncodedBlock>() * blocks.Length)
			{
				throw new Exception("Cannot encode block! Output buffer is not the correct size.");
			}
			EncodeBlocks(blocks, MemoryMarshal.Cast<byte, TEncodedBlock>(output), context);
		}
	}
}
