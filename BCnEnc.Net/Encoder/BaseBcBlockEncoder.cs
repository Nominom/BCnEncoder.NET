using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;

namespace BCnEncoder.Encoder
{
	internal abstract class BaseBcBlockEncoder<T, TBlock> : IBcBlockEncoder<TBlock> where T : unmanaged where TBlock : unmanaged
	{
		private static readonly object lockObj = new object();

		public byte[] Encode(TBlock[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, OperationContext context)
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
				var outputBlocks = MemoryMarshal.Cast<byte, T>(outputData);
				for (var i = 0; i < blocks.Length; i++)
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					outputBlocks[i] = EncodeBlock(blocks[i], quality);

					context.Progress?.Report(++currentBlocks);
				}
			}

			return outputData;
		}

		public void EncodeBlock(TBlock block, CompressionQuality quality, Span<byte> output)
		{
			if (output.Length != Unsafe.SizeOf<T>())
			{
				throw new Exception("Cannot encode block! Output buffer is not the correct size.");
			}
			var encoded = EncodeBlock(block, quality);
			MemoryMarshal.Cast<byte, T>(output)[0] = encoded;
		}

		public abstract GlInternalFormat GetInternalFormat();
		public abstract GlFormat GetBaseInternalFormat();
		public abstract DxgiFormat GetDxgiFormat();
		public int GetBlockSize()
		{
			return Unsafe.SizeOf<T>();
		}

		public abstract T EncodeBlock(TBlock block, CompressionQuality quality);
	}
}
