using System;
using System.IO;
using System.Runtime.InteropServices;
using BCnEncoder.Shared;

namespace BCnEncoder.Decoder
{
	internal interface IBcBlockDecoder
	{
		RawBlock4X4Rgba32[,] Decode(byte[] data, int pixelWidth, int pixelHeight, out int blockWidth,
			out int blockHeight);
	}

	internal class Bc1NoAlphaDecoder : IBcBlockDecoder
	{
		public unsafe RawBlock4X4Rgba32[,] Decode(byte[] data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight)
		{
			blockWidth = (int)Math.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)Math.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * sizeof(Bc1Block)))
			{
				throw new InvalidDataException();
			}

			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			fixed (byte* iDataBytes = data)
			{
				Bc1Block* encodedBlocks = (Bc1Block*)iDataBytes;

				for (int x = 0; x < blockWidth; x++)
				{
					for (int y = 0; y < blockHeight; y++)
					{
						output[x, y] = encodedBlocks[x + y * blockWidth].Decode(false);
					}
				}
			}

			return output;
		}
	}

	internal class Bc1ADecoder : IBcBlockDecoder
	{
		public unsafe RawBlock4X4Rgba32[,] Decode(byte[] data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight)
		{
			blockWidth = (int)Math.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)Math.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * sizeof(Bc1Block)))
			{
				throw new InvalidDataException();
			}
			
			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			fixed (byte* iDataBytes = data)
			{
				Bc1Block* encodedBlocks = (Bc1Block*)iDataBytes;

				for (int x = 0; x < blockWidth; x++)
				{
					for (int y = 0; y < blockHeight; y++)
					{
						output[x, y] = encodedBlocks[x + y * blockWidth].Decode(true);
					}
				}
			}

			return output;
		}
	}

	internal class Bc2Decoder : IBcBlockDecoder
	{
		public unsafe RawBlock4X4Rgba32[,] Decode(byte[] data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight)
		{
			blockWidth = (int)Math.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)Math.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * sizeof(Bc2Block)))
			{
				throw new InvalidDataException();
			}
			
			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			fixed (byte* iDataBytes = data)
			{
				Bc2Block* encodedBlocks = (Bc2Block*)iDataBytes;

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

	internal class Bc3Decoder : IBcBlockDecoder
	{
		public unsafe RawBlock4X4Rgba32[,] Decode(byte[] data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight)
		{
			blockWidth = (int)Math.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)Math.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * sizeof(Bc3Block)))
			{
				throw new InvalidDataException();
			}
			
			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			fixed (byte* iDataBytes = data)
			{
				Bc3Block* encodedBlocks = (Bc3Block*)iDataBytes;

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

	internal class Bc4Decoder : IBcBlockDecoder
	{
		private readonly bool redAsLuminance;
		public Bc4Decoder(bool redAsLuminance)
		{
			this.redAsLuminance = redAsLuminance;
		}

		public unsafe RawBlock4X4Rgba32[,] Decode(byte[] data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight)
		{
			blockWidth = (int)Math.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)Math.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * sizeof(Bc4Block)))
			{
				throw new InvalidDataException();
			}
			
			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			fixed (byte* iDataBytes = data)
			{
				Bc4Block* encodedBlocks = (Bc4Block*)iDataBytes;

				for (int x = 0; x < blockWidth; x++)
				{
					for (int y = 0; y < blockHeight; y++)
					{
						output[x, y] = encodedBlocks[x + y * blockWidth].Decode(redAsLuminance);
					}
				}
			}

			return output;
		}
	}

	internal class Bc5Decoder : IBcBlockDecoder
	{

		public unsafe RawBlock4X4Rgba32[,] Decode(byte[] data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight)
		{
			blockWidth = (int)Math.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)Math.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * sizeof(Bc5Block)))
			{
				throw new InvalidDataException();
			}
			
			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			fixed (byte* iDataBytes = data)
			{
				Bc5Block* encodedBlocks = (Bc5Block*)iDataBytes;

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
