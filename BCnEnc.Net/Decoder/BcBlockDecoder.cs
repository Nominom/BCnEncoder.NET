using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Decoder
{
	internal interface IBcBlockDecoder<T> where T : unmanaged
	{
		T[] Decode(ReadOnlyMemory<byte> data, OperationContext context);
		T DecodeBlock(ReadOnlySpan<byte> data);
	}

	internal abstract class BaseBcBlockDecoder<T, TBlock> : IBcBlockDecoder<TBlock> where T : unmanaged where TBlock : unmanaged
	{
		private static readonly object lockObj = new object();

		public TBlock[] Decode(ReadOnlyMemory<byte> data, OperationContext context)
		{

			if (data.Length % Unsafe.SizeOf<T>() != 0)
			{
				throw new InvalidDataException("Given data does not align with the block length.");
			}

			var blockCount = data.Length / Unsafe.SizeOf<T>();
			var output = new TBlock[blockCount];

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
						lock (lockObj)
						{
							context.Progress.Report(++currentBlocks);
						}
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

					context.Progress?.Report(++currentBlocks);
				}
			}

			return output;
		}

		public TBlock DecodeBlock(ReadOnlySpan<byte> data)
		{
			var encodedBlock = MemoryMarshal.Cast<byte, T>(data)[0];
			return DecodeBlock(encodedBlock);
		}

		protected abstract TBlock DecodeBlock(T block);
	}

	internal class Bc1NoAlphaDecoder : BaseBcBlockDecoder<Bc1Block, RawBlock4X4Rgba32>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc1Block block)
		{
			return block.Decode(false);
		}
	}

	internal class Bc1ADecoder : BaseBcBlockDecoder<Bc1Block, RawBlock4X4Rgba32>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc1Block block)
		{
			return block.Decode(true);
		}
	}

	internal class Bc2Decoder : BaseBcBlockDecoder<Bc2Block, RawBlock4X4Rgba32>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc2Block block)
		{
			return block.Decode();
		}
	}

	internal class Bc3Decoder : BaseBcBlockDecoder<Bc3Block, RawBlock4X4Rgba32>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc3Block block)
		{
			return block.Decode();
		}
	}

	internal class Bc4Decoder : BaseBcBlockDecoder<Bc4Block, RawBlock4X4Rgba32>
	{
		private readonly ColorComponent component;

		public Bc4Decoder(ColorComponent component)
		{
			this.component = component;
		}

		protected override RawBlock4X4Rgba32 DecodeBlock(Bc4Block block)
		{
			return block.Decode(component);
		}
	}

	internal class Bc5Decoder : BaseBcBlockDecoder<Bc5Block, RawBlock4X4Rgba32>
	{
		private readonly ColorComponent component1;
		private readonly ColorComponent component2;
		private readonly bool recalculateBlueChannel;

		public Bc5Decoder(ColorComponent component1, ColorComponent component2, bool recalculateBlueChannel)
		{
			this.component1 = component1;
			this.component2 = component2;
			this.recalculateBlueChannel = recalculateBlueChannel;
		}

		protected override RawBlock4X4Rgba32 DecodeBlock(Bc5Block block)
		{
			return block.Decode(component1, component2, recalculateBlueChannel);
		}
	}

	internal class Bc6UDecoder : BaseBcBlockDecoder<Bc6Block, RawBlock4X4RgbFloat>
	{
		protected override RawBlock4X4RgbFloat DecodeBlock(Bc6Block block)
		{
			return block.Decode(false);
		}
	}

	internal class Bc6SDecoder : BaseBcBlockDecoder<Bc6Block, RawBlock4X4RgbFloat>
	{
		protected override RawBlock4X4RgbFloat DecodeBlock(Bc6Block block)
		{
			return block.Decode(true);
		}
	}

	internal class Bc7Decoder : BaseBcBlockDecoder<Bc7Block, RawBlock4X4Rgba32>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc7Block block)
		{
			return block.Decode();
		}
	}

	internal class AtcDecoder : BaseBcBlockDecoder<AtcBlock, RawBlock4X4Rgba32>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(AtcBlock block)
		{
			return block.Decode();
		}
	}

	internal class AtcExplicitAlphaDecoder : BaseBcBlockDecoder<AtcExplicitAlphaBlock, RawBlock4X4Rgba32>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(AtcExplicitAlphaBlock block)
		{
			return block.Decode();
		}
	}

	internal class AtcInterpolatedAlphaDecoder : BaseBcBlockDecoder<AtcInterpolatedAlphaBlock, RawBlock4X4Rgba32>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(AtcInterpolatedAlphaBlock block)
		{
			return block.Decode();
		}
	}
}
