using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal abstract class BaseBcBlockEncoder<T> : IBcBlockEncoder where T : unmanaged
	{
		public byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, OperationContext context)
		{
			var outputData = new byte[blockWidth * blockHeight * Unsafe.SizeOf<T>()];

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
					 outputBlocks[i] = EncodeBlock(blocks[i], quality);

					 var progressValue = Interlocked.Add(ref currentBlocks, 1);
					 context.Progress.Report(new ProgressElement(progressValue, blocks.Length));
				 });
			}
			else
			{
				var outputBlocks = MemoryMarshal.Cast<byte, T>(outputData);
				for (var i = 0; i < blocks.Length; i++)
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					outputBlocks[i] = EncodeBlock(blocks[i], quality);

					context.Progress.Report(new ProgressElement(currentBlocks++, blocks.Length));
				}
			}

			return outputData;
		}

		public abstract GlInternalFormat GetInternalFormat();
		public abstract GlFormat GetBaseInternalFormat();
		public abstract DxgiFormat GetDxgiFormat();

		public abstract T EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality);
	}
}
