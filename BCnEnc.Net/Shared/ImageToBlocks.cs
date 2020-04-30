using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.Shared
{
	internal static class ImageToBlocks
	{

		internal static RawBlock4X4Rgba32[] ImageTo4X4(ImageFrame<Rgba32> image, out int blocksWidth, out int blocksHeight)
		{
			blocksWidth = (int)MathF.Ceiling(image.Width / 4.0f);
			blocksHeight = (int)MathF.Ceiling(image.Height / 4.0f);
			RawBlock4X4Rgba32[] output = new RawBlock4X4Rgba32[blocksWidth * blocksHeight];
			Span<Rgba32> pixels = image.GetPixelSpan();

			for (int y = 0; y < image.Height; y++)
			{
				for (int x = 0; x < image.Width; x++)
				{
					Rgba32 color = pixels[x + y * image.Width];
					int blockIndexX = (int)MathF.Floor(x / 4.0f);
					int blockIndexY = (int)MathF.Floor(y / 4.0f);
					int blockInternalIndexX = x % 4;
					int blockInternalIndexY = y % 4;

					output[blockIndexX + blockIndexY * blocksWidth][blockInternalIndexX, blockInternalIndexY] = color;
				}
			}

			//Fill in block y with edge color
			if (image.Height % 4 != 0)
			{
				int yPaddingStart = image.Height % 4;
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
			if (image.Width % 4 != 0)
			{
				int xPaddingStart = image.Width % 4;
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

		internal static Image<Rgba32> ImageFromRawBlocks(RawBlock4X4Rgba32[,] blocks, int blocksWidth, int blocksHeight)
			=> ImageFromRawBlocks(blocks, blocksWidth, blocksHeight, blocksWidth * 4, blocksHeight * 4);


		internal static Image<Rgba32> ImageFromRawBlocks(RawBlock4X4Rgba32[,] blocks, int blocksWidth, int blocksHeight, int pixelWidth, int pixelHeight)
		{
			Image<Rgba32> output = new Image<Rgba32>(pixelWidth, pixelHeight);
			Span<Rgba32> pixels = output.GetPixelSpan();

			for (int y = 0; y < output.Height; y++)
			{
				for (int x = 0; x < output.Width; x++)
				{
					int blockIndexX = (int)MathF.Floor(x / 4.0f);
					int blockIndexY = (int)MathF.Floor(y / 4.0f);
					int blockInternalIndexX = x % 4;
					int blockInternalIndexY = y % 4;

					pixels[x + y * output.Width] =
						blocks[blockIndexX, blockIndexY]
							[blockInternalIndexX, blockInternalIndexY];
				}
			}

			return output;
		}

		internal static Image<Rgba32> ImageFromRawBlocks(RawBlock4X4Rgba32[] blocks, int blocksWidth, int blocksHeight)
			=> ImageFromRawBlocks(blocks, blocksWidth, blocksHeight, blocksWidth * 4, blocksHeight * 4);


		internal static Image<Rgba32> ImageFromRawBlocks(RawBlock4X4Rgba32[] blocks, int blocksWidth, int blocksHeight, int pixelWidth, int pixelHeight)
		{
			Image<Rgba32> output = new Image<Rgba32>(pixelWidth, pixelHeight);
			Span<Rgba32> pixels = output.GetPixelSpan();

			for (int y = 0; y < output.Height; y++)
			{
				for (int x = 0; x < output.Width; x++)
				{
					int blockIndexX = (int)MathF.Floor(x / 4.0f);
					int blockIndexY = (int)MathF.Floor(y / 4.0f);
					int blockInternalIndexX = x % 4;
					int blockInternalIndexY = y % 4;

					pixels[x + y * output.Width] =
						blocks[blockIndexX + blockIndexY * blocksWidth]
							[blockInternalIndexX, blockInternalIndexY];
				}
			}

			return output;
		}
	}
}
