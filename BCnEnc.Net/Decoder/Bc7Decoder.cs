using System;
using System.IO;
using System.Runtime.InteropServices;
using BCnEncoder.Shared;

namespace BCnEncoder.Decoder
{
	internal class Bc7Decoder : IBcBlockDecoder
	{
		public RawBlock4X4Rgba32[,] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight) {
			blockWidth = (int)MathF.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)MathF.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * Marshal.SizeOf<Bc7Block>())) {
				throw new InvalidDataException();
			}

			var encodedBlocks = MemoryMarshal.Cast<byte, Bc7Block>(data);

			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

 			for (int x = 0; x < blockWidth; x++) {
				for (int y = 0; y < blockHeight; y++) {
					var rawBlock = encodedBlocks[x + y * blockWidth];
					output[x, y] = rawBlock.Decode();
				}
			}

            return output;
		}
	}
}
