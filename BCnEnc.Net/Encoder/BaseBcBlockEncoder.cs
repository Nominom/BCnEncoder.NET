using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal abstract class BaseBcBlockEncoder<T> : IBcBlockEncoder where T : unmanaged
	{
		public byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, bool parallel)
		{
			var outputData = new byte[blockWidth * blockHeight * Unsafe.SizeOf<T>()];
			var outputBlocks = MemoryMarshal.Cast<byte, T>(outputData);

			if (parallel)
			{
				Parallel.For(0, blocks.Length, i =>
				{
					var outputBlocks = MemoryMarshal.Cast<byte, T>(outputData);
					outputBlocks[i] = EncodeBlock(blocks[i], quality);
				});
			}
			else
			{
				for (var i = 0; i < blocks.Length; i++)
				{
					outputBlocks[i] = EncodeBlock(blocks[i], quality);
				}
			}

			return outputData;
		}

		public abstract GlInternalFormat GetInternalFormat();
		public abstract GlFormat GetBaseInternalFormat();
		public abstract DxgiFormat GetDxgiFormat();

		protected abstract T EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality);
	}
}
