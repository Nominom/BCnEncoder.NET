using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal abstract class BaseBcBlockEncoder<T> : IBcBlockEncoder where T : unmanaged
	{
		public int BlockSize => Unsafe.SizeOf<T>();

		public byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, OperationContext context)
		{
			var outputData = new byte[blockWidth * blockHeight * BlockSize];

			var currentBlocks = 0;
			if (context.IsParallel)
			{
				var options = new ParallelOptions
				{
					CancellationToken = context.CancellationToken,
					MaxDegreeOfParallelism = context.TaskCount
				};
				Parallel.For(0, blocks.Length, options, i =>
				 {
					 var outputBlocks = MemoryMarshal.Cast<byte, T>(outputData);
					 outputBlocks[i] = EncodeBlockInternal(blocks[i], quality);

					 if (context.Progress != null)
					 {
						 var progressValue = Interlocked.Add(ref currentBlocks, 1);
						 context.Progress.Report(progressValue);
					 }
				 });
			}
			else
			{
				var outputBlocks = MemoryMarshal.Cast<byte, T>(outputData);
				for (var i = 0; i < blocks.Length; i++)
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					outputBlocks[i] = EncodeBlockInternal(blocks[i], quality);

					context.Progress?.Report(currentBlocks++);
				}
			}

			return outputData;
		}

		public byte[] EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			var encoded = new[] { EncodeBlockInternal(block, quality) };
			return MemoryMarshal.Cast<T, byte>(encoded).ToArray();
		}

		public abstract GlInternalFormat GetInternalFormat();
		public abstract GlFormat GetBaseInternalFormat();
		public abstract DxgiFormat GetDxgiFormat();

		public abstract T EncodeBlockInternal(RawBlock4X4Rgba32 block, CompressionQuality quality);
	}
}
