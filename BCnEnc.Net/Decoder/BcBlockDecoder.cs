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
	internal interface IBcBlockDecoder<TRawBlock> where TRawBlock : unmanaged
	{
		TRawBlock[] Decode(ReadOnlyMemory<byte> data, OperationContext context);
		TRawBlock DecodeBlock(ReadOnlySpan<byte> data);
	}

	internal interface IBcLdrBlockDecoder : IBcBlockDecoder<RawBlock4X4Rgba32>, IBcLdrDecoder {}
	internal interface IBcHdrBlockDecoder : IBcBlockDecoder<RawBlock4X4RgbFloat>, IBcHdrDecoder {}

	internal abstract class BaseLdrBlockDecoder<TEncodedBlock> : BaseBcBlockDecoder<TEncodedBlock, RawBlock4X4Rgba32>, IBcLdrBlockDecoder
		where TEncodedBlock : unmanaged
	{

		/// <inheritdoc />
		public byte[] Decode(ReadOnlyMemory<byte> data, int width, int height, OperationContext context)
		{
			var blocks = Decode(data, context);
			return ImageToBlocks.ColorsFromRawBlocks(blocks, width, height).CopyAsBytes();
		}

		/// <inheritdoc />
		public ColorRgba32[] DecodeColor(ReadOnlyMemory<byte> data, int width, int height, OperationContext context)
		{
			var blocks = Decode(data, context);
			return ImageToBlocks.ColorsFromRawBlocks(blocks, width, height);
		}
	}

	internal abstract class BaseHdrBlockDecoder<TEncodedBlock> : BaseBcBlockDecoder<TEncodedBlock, RawBlock4X4RgbFloat>, IBcHdrBlockDecoder
		where TEncodedBlock : unmanaged
	{

		/// <inheritdoc />
		public byte[] Decode(ReadOnlyMemory<byte> data, int width, int height, OperationContext context)
		{
			var blocks = Decode(data, context);
			return ImageToBlocks.ColorsFromRawBlocks(blocks, width, height).ConvertTo<ColorRgbFloat, ColorRgbaFloat>().CopyAsBytes();
		}

		/// <inheritdoc />
		public ColorRgbaFloat[] DecodeColor(ReadOnlyMemory<byte> data, int width, int height, OperationContext context)
		{
			var blocks = Decode(data, context);
			return ImageToBlocks.ColorsFromRawBlocks(blocks, width, height).ConvertTo<ColorRgbFloat, ColorRgbaFloat>();
		}
	}

	internal abstract class BaseBcBlockDecoder<TEncodedBlock, TRawBlock> : IBcBlockDecoder<TRawBlock>
		where TEncodedBlock : unmanaged
		where TRawBlock : unmanaged
	{
		private static readonly object lockObj = new object();

		public TRawBlock[] Decode(ReadOnlyMemory<byte> data, OperationContext context)
		{

			if (data.Length % Unsafe.SizeOf<TEncodedBlock>() != 0)
			{
				throw new InvalidDataException("Given data does not align with the block length.");
			}

			var blockCount = data.Length / Unsafe.SizeOf<TEncodedBlock>();
			var output = new TRawBlock[blockCount];

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

					context.Progress?.Report(1);
				}
			}

			return output;
		}

		public TRawBlock DecodeBlock(ReadOnlySpan<byte> data)
		{
			var encodedBlock = MemoryMarshal.Cast<byte, TEncodedBlock>(data)[0];
			return DecodeBlock(encodedBlock);
		}

		protected abstract TRawBlock DecodeBlock(TEncodedBlock block);
	}

	internal class Bc1NoAlphaDecoder : BaseLdrBlockDecoder<Bc1Block>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc1Block block)
		{
			return block.Decode(false);
		}
	}

	internal class Bc1ADecoder : BaseLdrBlockDecoder<Bc1Block>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc1Block block)
		{
			return block.Decode(true);
		}
	}

	internal class Bc2Decoder : BaseLdrBlockDecoder<Bc2Block>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc2Block block)
		{
			return block.Decode();
		}
	}

	internal class Bc3Decoder : BaseLdrBlockDecoder<Bc3Block>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc3Block block)
		{
			return block.Decode();
		}
	}

	internal class Bc4Decoder : BaseLdrBlockDecoder<Bc4Block>
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

	internal class Bc5Decoder : BaseLdrBlockDecoder<Bc5Block>
	{
		private readonly ColorComponent component1;
		private readonly ColorComponent component2;

		public Bc5Decoder(ColorComponent component1, ColorComponent component2)
		{
			this.component1 = component1;
			this.component2 = component2;
		}

		protected override RawBlock4X4Rgba32 DecodeBlock(Bc5Block block)
		{
			return block.Decode(component1, component2);
		}
	}

	internal class Bc6UDecoder : BaseHdrBlockDecoder<Bc6Block>
	{
		protected override RawBlock4X4RgbFloat DecodeBlock(Bc6Block block)
		{
			return block.Decode(false);
		}
	}

	internal class Bc6SDecoder : BaseHdrBlockDecoder<Bc6Block>
	{
		protected override RawBlock4X4RgbFloat DecodeBlock(Bc6Block block)
		{
			return block.Decode(true);
		}
	}

	internal class Bc7Decoder : BaseLdrBlockDecoder<Bc7Block>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(Bc7Block block)
		{
			return block.Decode();
		}
	}

	internal class AtcDecoder : BaseLdrBlockDecoder<AtcBlock>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(AtcBlock block)
		{
			return block.Decode();
		}
	}

	internal class AtcExplicitAlphaDecoder : BaseLdrBlockDecoder<AtcExplicitAlphaBlock>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(AtcExplicitAlphaBlock block)
		{
			return block.Decode();
		}
	}

	internal class AtcInterpolatedAlphaDecoder : BaseLdrBlockDecoder<AtcInterpolatedAlphaBlock>
	{
		protected override RawBlock4X4Rgba32 DecodeBlock(AtcInterpolatedAlphaBlock block)
		{
			return block.Decode();
		}
	}
}
