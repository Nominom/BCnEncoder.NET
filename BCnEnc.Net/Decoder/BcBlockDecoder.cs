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
		RawBlock4X4Rgba32[,] Decode(ReadOnlyMemory<byte> data, int pixelWidth, int pixelHeight, OperationContext context,
			out int blockWidth, out int blockHeight);
	}

	internal abstract class BaseBcBlockDecoder<T> : IBcBlockDecoder where T : unmanaged
	{
		public RawBlock4X4Rgba32[,] Decode(ReadOnlyMemory<byte> data, int pixelWidth, int pixelHeight, OperationContext context,
			out int blockWidth, out int blockHeight)
		{
			blockWidth = (int)MathF.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)MathF.Ceiling(pixelHeight / 4.0f);

			if (data.Length != blockWidth * blockHeight * Unsafe.SizeOf<T>())
			{
				throw new InvalidDataException("Given data does not match expected length.");
			}

			var output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			var currentBlocks = 0;
			if (context.IsParallel)
			{
				var localBlockWidth = blockWidth;

				var options = new ParallelOptions
				{
					CancellationToken = context.CancellationToken,
					MaxDegreeOfParallelism = context.TaskCount
				};
				Parallel.For(0, blockWidth * blockHeight, options, i =>
				{
					var encodedBlocks = MemoryMarshal.Cast<byte, T>(data.Span);
					output[i % localBlockWidth, i / localBlockWidth] = DecodeBlock(encodedBlocks[i]);

					var progressValue = Interlocked.Add(ref currentBlocks, 1);
					context.Progress.Report(new ProgressElement(progressValue, output.Length));
				});
			}
			else
			{
				var encodedBlocks = MemoryMarshal.Cast<byte, T>(data.Span);
				for (var x = 0; x < blockWidth; x++)
				{
					for (var y = 0; y < blockHeight; y++)
					{
						context.CancellationToken.ThrowIfCancellationRequested();

						output[x, y] = DecodeBlock(encodedBlocks[x + y * blockWidth]);

						context.Progress.Report(new ProgressElement(currentBlocks++, output.Length));
					}
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
