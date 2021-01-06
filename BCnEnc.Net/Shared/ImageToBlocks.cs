using System;

namespace BCnEncoder.Shared
{
	internal static class ImageToBlocks
	{

		internal static RawBlock4X4Rgba32[] ImageTo4X4(byte[] imageRgba, int imageWidth, int imageHeight, out int blocksWidth, out int blocksHeight)
		{
			blocksWidth = (int)Math.Ceiling(imageWidth / 4.0f);
			blocksHeight = (int)Math.Ceiling(imageHeight / 4.0f);
			RawBlock4X4Rgba32[] output = new RawBlock4X4Rgba32[blocksWidth * blocksHeight];


			for (int y = 0; y < imageHeight; y++)
			{
				for (int x = 0; x < imageWidth; x++)
				{
					byte r = imageRgba[(x + y * imageWidth) * 4 + 0];
					byte g = imageRgba[(x + y * imageWidth) * 4 + 1];
					byte b = imageRgba[(x + y * imageWidth) * 4 + 2];
					byte a = imageRgba[(x + y * imageWidth) * 4 + 3];
					Rgba32 color = new Rgba32(r, g, b, a);
					int blockIndexX = (int)Math.Floor(x / 4.0f);
					int blockIndexY = (int)Math.Floor(y / 4.0f);
					int blockInternalIndexX = x % 4;
					int blockInternalIndexY = y % 4;

					output[blockIndexX + blockIndexY * blocksWidth][blockInternalIndexX, blockInternalIndexY] = color;
				}
			}

			//Fill in block y with edge color
			if (imageHeight % 4 != 0)
			{
				int yPaddingStart = imageHeight % 4;
				for (int i = 0; i < blocksWidth; i++)
				{
					var lastBlock = output[i + blocksWidth * (blocksHeight - 1)];
					for (int y = yPaddingStart; y < 4; y++)
					{
						for (int x = 0; x < 4; x++)
						{
							lastBlock[x, y] = lastBlock[x, y - 1];
						}
					}
					output[i + blocksWidth * (blocksHeight - 1)] = lastBlock;
				}
			}

			//Fill in block x with edge color
			if (imageWidth % 4 != 0)
			{
				int xPaddingStart = imageWidth % 4;
				for (int i = 0; i < blocksHeight; i++)
				{
					var lastBlock = output[blocksWidth - 1 + i * blocksWidth];
					for (int x = xPaddingStart; x < 4; x++)
					{
						for (int y = 0; y < 4; y++)
						{
							lastBlock[x, y] = lastBlock[x - 1, y];
						}
					}
					output[blocksWidth - 1 + i * blocksWidth] = lastBlock;
				}
			}

			return output;
		}

		internal static byte[] ImageFromRawBlocks(RawBlock4X4Rgba32[,] blocks, int blocksWidth, int blocksHeight)
			=> ImageFromRawBlocks(blocks, blocksWidth, blocksHeight, blocksWidth * 4, blocksHeight * 4);


		internal static byte[] ImageFromRawBlocks(RawBlock4X4Rgba32[,] blocks, int blocksWidth, int blocksHeight, int pixelWidth, int pixelHeight)
		{
			byte[] output = new byte[pixelWidth * pixelHeight * 4];

			for (int y = 0; y < pixelHeight; y++)
			{
				for (int x = 0; x < pixelWidth; x++)
				{
					int blockIndexX = (int)Math.Floor(x / 4.0f);
					int blockIndexY = (int)Math.Floor(y / 4.0f);
					int blockInternalIndexX = x % 4;
					int blockInternalIndexY = y % 4;

					output[(x + y * pixelWidth) * 4 + 0] = blocks[blockIndexX, blockIndexY][blockInternalIndexX, blockInternalIndexY].R;
					output[(x + y * pixelWidth) * 4 + 1] = blocks[blockIndexX, blockIndexY][blockInternalIndexX, blockInternalIndexY].G;
					output[(x + y * pixelWidth) * 4 + 2] = blocks[blockIndexX, blockIndexY][blockInternalIndexX, blockInternalIndexY].B;
					output[(x + y * pixelWidth) * 4 + 3] = blocks[blockIndexX, blockIndexY][blockInternalIndexX, blockInternalIndexY].A;
				}
			}

			return output;
		}

		internal static byte[] ImageFromRawBlocks(RawBlock4X4Rgba32[] blocks, int blocksWidth, int blocksHeight)
			=> ImageFromRawBlocks(blocks, blocksWidth, blocksHeight, blocksWidth * 4, blocksHeight * 4);


		internal static byte[] ImageFromRawBlocks(RawBlock4X4Rgba32[] blocks, int blocksWidth, int blocksHeight, int pixelWidth, int pixelHeight)
		{
			byte[] output = new byte[pixelWidth * pixelHeight * 4];


			for (int y = 0; y < pixelHeight; y++)
			{
				for (int x = 0; x < pixelWidth; x++)
				{
					int blockIndexX = (int)Math.Floor(x / 4.0f);
					int blockIndexY = (int)Math.Floor(y / 4.0f);
					int blockInternalIndexX = x % 4;
					int blockInternalIndexY = y % 4;
					
					output[(x + y * pixelWidth) * 4 + 0] = blocks[blockIndexX + blockIndexY * blocksWidth][blockInternalIndexX, blockInternalIndexY].R;
					output[(x + y * pixelWidth) * 4 + 1] = blocks[blockIndexX + blockIndexY * blocksWidth][blockInternalIndexX, blockInternalIndexY].G;
					output[(x + y * pixelWidth) * 4 + 2] = blocks[blockIndexX + blockIndexY * blocksWidth][blockInternalIndexX, blockInternalIndexY].B;
					output[(x + y * pixelWidth) * 4 + 3] = blocks[blockIndexX + blockIndexY * blocksWidth][blockInternalIndexX, blockInternalIndexY].A;
				}
			}

			return output;
		}
	}
}
