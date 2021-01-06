using System;
using System.IO;
using System.Runtime.InteropServices;
using BCnEncoder.Shared;

namespace BCnEncoder.Decoder
{
	internal class Bc7Decoder : IBcBlockDecoder
	{
		public unsafe RawBlock4X4Rgba32[,] Decode(byte[] data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight) {
			blockWidth = (int)Math.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)Math.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * sizeof(Bc7Block))) {
				throw new InvalidDataException();
			}
			
			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			fixed (byte* iDataBytes = data)
			{
				Bc7Block* encodedBlocks = (Bc7Block*)iDataBytes;

				for (int x = 0; x < blockWidth; x++)
				{
					for (int y = 0; y < blockHeight; y++)
					{
						output[x, y] = encodedBlocks[x + y * blockWidth].Decode();
					}
				}
			}

			return output;
		}
	}
}
