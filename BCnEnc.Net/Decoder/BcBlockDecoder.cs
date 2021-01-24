using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Decoder
{
	internal interface IBcBlockDecoder
	{
		RawBlock4X4Rgba32[] Decode(ReadOnlyMemory<byte> data, OperationContext context);
	}

	internal abstract class BaseBcBlockDecoder<T> : IBcBlockDecoder where T : unmanaged
	{
		public RawBlock4X4Rgba32[] Decode(ReadOnlyMemory<byte> data, OperationContext context)
		{
			// calculate number of 4x4 blocks by padding width/height to a multiple of 4 and shift it right by 2
			//blockWidth = ((pixelWidth + 3) & ~3) >> 2;
			//blockHeight = ((pixelHeight + 3) & ~3) >> 2;

			if (data.Length % Unsafe.SizeOf<T>() != 0)
			{
				throw new InvalidDataException("Given data does not align with the block length.");
			}

			var blockCount = data.Length / Unsafe.SizeOf<T>();
			var output = new RawBlock4X4Rgba32[blockCount];

			var currentBlocks = 0;
			if (context.IsParallel)
			{
				var options = new ParallelOptions
				{
					CancellationToken = context.CancellationToken,
					MaxDegreeOfParallelism = context.TaskCount
				};
				Parallel.For(0, blockCount, options, i =>
				{
					var encodedBlocks = MemoryMarshal.Cast<byte, T>(data.Span);
					output[i] = DecodeBlock(encodedBlocks[i]);

					if (context.Progress != null)
					{
						var progressValue = Interlocked.Add(ref currentBlocks, 1);
						context.Progress.Report(progressValue);
					}
				});
			}
			else
			{
				var encodedBlocks = MemoryMarshal.Cast<byte, T>(data.Span);
				for (var i = 0; i < blockCount; i++)
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					output[i] = DecodeBlock(encodedBlocks[i]);

					context.Progress?.Report(currentBlocks++);
				}
			}

			return output;
		}

		protected abstract RawBlock4X4Rgba32 DecodeBlock(T block);
	}

	internal class Bc1NoAlphaDecoder : BaseBcBlockDecoder<Bc1Block>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc1Block block)
		{
			return block.Decode(false);
		}
	}

	internal class Bc1ADecoder : BaseBcBlockDecoder<Bc1Block>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc1Block block)
		{
			return block.Decode(true);
		}
	}

	internal class Bc2Decoder : BaseBcBlockDecoder<Bc2Block>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc2Block block)
		{
			return block.Decode();
		}
	}

	internal class Bc3Decoder : BaseBcBlockDecoder<Bc3Block>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc3Block block)
		{
			return block.Decode();
		}
	}

	internal class Bc4Decoder : BaseBcBlockDecoder<Bc4Block>
	{
		private readonly bool redAsLuminance;

		public Bc4Decoder(bool redAsLuminance)
		{
			this.redAsLuminance = redAsLuminance;
		}

		protected override RawBlock4X4Rgba32 DecodeBlock(Bc4Block block)
		{
			return block.Decode(redAsLuminance);
		}
	}

	internal class Bc5Decoder : BaseBcBlockDecoder<Bc5Block>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc5Block block)
		{
			return block.Decode();
		}
	}

	internal class Bc7Decoder : BaseBcBlockDecoder<Bc7Block>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc7Block block)
		{
			return block.Decode();
		}
	}

	internal class AtcDecoder : BaseBcBlockDecoder<AtcBlock>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(AtcBlock block)
		{
			return block.Decode();
		}
	}

	internal class AtcExplicitAlphaDecoder : BaseBcBlockDecoder<AtcExplicitAlphaBlock>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(AtcExplicitAlphaBlock block)
		{
			return block.Decode();
		}
	}

	internal class AtcInterpolatedAlphaDecoder : BaseBcBlockDecoder<AtcInterpolatedAlphaBlock>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(AtcInterpolatedAlphaBlock block)
		{
			return block.Decode();
		}
	}
}
