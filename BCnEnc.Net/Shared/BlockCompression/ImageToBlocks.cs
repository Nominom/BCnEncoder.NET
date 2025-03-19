using System;
using BCnEncoder.Shared.Colors;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Shared
{
	internal static class ImageToBlocks
	{
		internal static void ColorsFromRawBlocks(RawBlock4X4RgbaFloat[] blocks, Span<ColorRgbaFloat> output, int pixelWidth, int pixelHeight)
		{
			var blocksWidth = ((pixelWidth + 3) & ~3) >> 2;

			for (var y = 0; y < pixelHeight; y++)
			{
				for (var x = 0; x < pixelWidth; x++)
				{
					var blockIndexX = x >> 2;
					var blockIndexY = y >> 2;
					var blockInternalIndexX = x & 3;
					var blockInternalIndexY = y & 3;

					var blockIndex = blockIndexX + blockIndexY * blocksWidth;

					output[x + y * pixelWidth] = blocks[blockIndex][blockInternalIndexX, blockInternalIndexY];
				}
			}
		}

		internal static RawBlock4X4RgbaFloat[] ImageTo4X4(ReadOnlyMemory<ColorRgbaFloat> image, int width, int height, out int blocksWidth, out int blocksHeight)
		{
			blocksWidth = ((width + 3) & ~3) >> 2;
			blocksHeight = ((height + 3) & ~3) >> 2;

			var output = new RawBlock4X4RgbaFloat[blocksWidth * blocksHeight];

			var span = image.Span;

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					var color = span[y * width + x];

					var blockIndexX = x >> 2;
					var blockIndexY = y >> 2;
					var blockInternalIndexX = x & 3;
					var blockInternalIndexY = y & 3;

					output[blockIndexX + blockIndexY * blocksWidth][blockInternalIndexX, blockInternalIndexY] = color;
				}
			}

			// Fill in block y with edge color
			if ((height & 3) != 0)
			{
				var yPaddingStart = height & 3;
				for (var i = 0; i < blocksWidth; i++)
				{
					var lastBlock = output[i + blocksWidth * (blocksHeight - 1)];
					for (var y = yPaddingStart; y < 4; y++)
					{
						for (var x = 0; x < 4; x++)
						{
							lastBlock[x, y] = lastBlock[x, y - 1];
						}
					}
					output[i + blocksWidth * (blocksHeight - 1)] = lastBlock;
				}
			}

			// Fill in block x with edge color
			if ((width & 3) != 0)
			{
				var xPaddingStart = width & 3;
				for (var i = 0; i < blocksHeight; i++)
				{
					var lastBlock = output[blocksWidth - 1 + i * blocksWidth];
					for (var x = xPaddingStart; x < 4; x++)
					{
						for (var y = 0; y < 4; y++)
						{
							lastBlock[x, y] = lastBlock[x - 1, y];
						}
					}
					output[blocksWidth - 1 + i * blocksWidth] = lastBlock;
				}
			}

			return output;
		}

		public static int CalculateNumOfBlocks(int pixelWidth, int pixelHeight)
		{
			CalculateNumOfBlocks(pixelWidth, pixelHeight, out var blocksWidth, out var blocksHeight);
			return blocksWidth * blocksHeight;
		}
		public static void CalculateNumOfBlocks(int pixelWidth, int pixelHeight, out int blocksWidth, out int blocksHeight)
		{
			blocksWidth = ((pixelWidth + 3) & ~3) >> 2;
			blocksHeight = ((pixelHeight + 3) & ~3) >> 2;
		}
	}
}
