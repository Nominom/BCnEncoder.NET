using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Decoder
{
	internal interface IBcBlockDecoder : IBcDecoder
	{
		RawBlock4X4RgbaFloat[] Decode(ReadOnlyMemory<byte> data, OperationContext context);
		RawBlock4X4RgbaFloat DecodeBlock(ReadOnlySpan<byte> data);
	}

	internal abstract class BaseBcBlockDecoder<TEncodedBlock> : IBcBlockDecoder
		where TEncodedBlock : unmanaged
	{
		private static readonly object lockObj = new object();

		public RawBlock4X4RgbaFloat[] Decode(ReadOnlyMemory<byte> data, OperationContext context)
		{
			if (data.Length % Unsafe.SizeOf<TEncodedBlock>() != 0)
			{
				throw new InvalidDataException("Given data does not align with the block length.");
			}

			var blockCount = data.Length / Unsafe.SizeOf<TEncodedBlock>();
			var output = new RawBlock4X4RgbaFloat[blockCount];

			if (context.IsParallel)
			{
				var options = new ParallelOptions
				{
					CancellationToken = context.CancellationToken,
					MaxDegreeOfParallelism = context.TaskCount
				};
				Parallel.For(0, blockCount, options, i =>
				{
					var encodedBlocks = MemoryMarshal.Cast<byte, TEncodedBlock>(data.Span);
					output[i] = DecodeBlock(encodedBlocks[i]);
					output[i].ColorConvert(context.ColorConversionMode);

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
				var encodedBlocks = MemoryMarshal.Cast<byte, TEncodedBlock>(data.Span);
				for (var i = 0; i < blockCount; i++)
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					output[i] = DecodeBlock(encodedBlocks[i]);
					output[i].ColorConvert(context.ColorConversionMode);

					context.Progress?.Report(1);
				}
			}



			return output;
		}

		public RawBlock4X4RgbaFloat DecodeBlock(ReadOnlySpan<byte> data)
		{
			var encodedBlock = MemoryMarshal.Cast<byte, TEncodedBlock>(data)[0];
			return DecodeBlock(encodedBlock);
		}

		protected abstract RawBlock4X4RgbaFloat DecodeBlock(TEncodedBlock block);

		public byte[] Decode(ReadOnlyMemory<byte> data, int width, int height, OperationContext context)
		{
			byte[] output = new byte[width * height * Unsafe.SizeOf<ColorRgbaFloat>()];
			Span<ColorRgbaFloat> colors = MemoryMarshal.Cast<byte, ColorRgbaFloat>(output);

			ImageToBlocks.ColorsFromRawBlocks(Decode(data, context), colors, width, height);

			return output;
		}

		public ColorRgbaFloat[] DecodeColor(ReadOnlyMemory<byte> data, int width, int height, OperationContext context)
		{
			ColorRgbaFloat[] output = new ColorRgbaFloat[width * height];

			ImageToBlocks.ColorsFromRawBlocks(Decode(data, context), output, width, height);

			return output;
		}
	}

	internal class Bc1NoAlphaDecoder : BaseBcBlockDecoder<Bc1Block>
	{
		protected override RawBlock4X4RgbaFloat DecodeBlock(Bc1Block block)
		{
			return block.Decode(false);
		}
	}

	internal class Bc1ADecoder : BaseBcBlockDecoder<Bc1Block>
	{
		protected override RawBlock4X4RgbaFloat DecodeBlock(Bc1Block block)
		{
			return block.Decode(true);
		}
	}

	internal class Bc2Decoder : BaseBcBlockDecoder<Bc2Block>
	{
		protected override RawBlock4X4RgbaFloat DecodeBlock(Bc2Block block)
		{
			return block.Decode();
		}
	}

	internal class Bc3Decoder : BaseBcBlockDecoder<Bc3Block>
	{
		protected override RawBlock4X4RgbaFloat DecodeBlock(Bc3Block block)
		{
			return block.Decode();
		}
	}

	internal class Bc4Decoder : BaseBcBlockDecoder<Bc4Block>
	{
		private readonly ColorComponent component;

		public Bc4Decoder(ColorComponent component)
		{
			this.component = component;
		}

		protected override RawBlock4X4RgbaFloat DecodeBlock(Bc4Block block)
		{
			return block.Decode(component);
		}
	}

	internal class Bc5Decoder : BaseBcBlockDecoder<Bc5Block>
	{
		private readonly ColorComponent component1;
		private readonly ColorComponent component2;
		private readonly ColorComponent componentCalculated;

		public Bc5Decoder(ColorComponent component1, ColorComponent component2, ColorComponent componentCalculated)
		{
			this.component1 = component1;
			this.component2 = component2;
			this.componentCalculated = componentCalculated;
		}

		protected override RawBlock4X4RgbaFloat DecodeBlock(Bc5Block block)
		{
			return block.Decode(component1, component2, componentCalculated);
		}
	}

	internal class Bc6UDecoder : BaseBcBlockDecoder<Bc6Block>
	{
		protected override RawBlock4X4RgbaFloat DecodeBlock(Bc6Block block)
		{
			return block.Decode(false);
		}
	}

	internal class Bc6SDecoder : BaseBcBlockDecoder<Bc6Block>
	{
		protected override RawBlock4X4RgbaFloat DecodeBlock(Bc6Block block)
		{
			return block.Decode(true);
		}
	}

	internal class Bc7Decoder : BaseBcBlockDecoder<Bc7Block>
	{
		protected override RawBlock4X4RgbaFloat DecodeBlock(Bc7Block block)
		{
			return block.Decode();
		}
	}

	internal class AtcDecoder : BaseBcBlockDecoder<AtcBlock>
	{
		protected override RawBlock4X4RgbaFloat DecodeBlock(AtcBlock block)
		{
			return block.Decode();
		}
	}

	internal class AtcExplicitAlphaDecoder : BaseBcBlockDecoder<AtcExplicitAlphaBlock>
	{
		protected override RawBlock4X4RgbaFloat DecodeBlock(AtcExplicitAlphaBlock block)
		{
			return block.Decode();
		}
	}

	internal class AtcInterpolatedAlphaDecoder : BaseBcBlockDecoder<AtcInterpolatedAlphaBlock>
	{
		protected override RawBlock4X4RgbaFloat DecodeBlock(AtcInterpolatedAlphaBlock block)
		{
			return block.Decode();
		}
	}
}
