using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Shared
{
	internal static class ImageToBlocks
	{
		#region Blocks to colors

		internal static ColorRgba32[] ColorsFromRawBlocks(RawBlock4X4Rgba32[,] blocks, int pixelWidth, int pixelHeight)
		{
			var output = new ColorRgba32[pixelWidth * pixelHeight];

			for (var y = 0; y < pixelHeight; y++)
			{
				for (var x = 0; x < pixelWidth; x++)
				{
					var blockIndexX = x >> 2;
					var blockIndexY = y >> 2;
					var blockInternalIndexX = x & 3;
					var blockInternalIndexY = y & 3;

					output[x + y * pixelWidth] = blocks[blockIndexX, blockIndexY][blockInternalIndexX, blockInternalIndexY];
				}
			}

			return output;
		}

		internal static ColorRgba32[] ColorsFromRawBlocks(RawBlock4X4Rgba32[] blocks, int pixelWidth, int pixelHeight)
		{
			var blocksWidth = ((pixelWidth + 3) & ~3) >> 2;
			var output = new ColorRgba32[pixelWidth * pixelHeight];

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

			return output;
		}

		internal static ColorRgbFloat[] ColorsFromRawBlocks(RawBlock4X4RgbFloat[] blocks, int pixelWidth, int pixelHeight)
		{
			var blocksWidth = ((pixelWidth + 3) & ~3) >> 2;
			var output = new ColorRgbFloat[pixelWidth * pixelHeight];

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

			return output;
		}

		#endregion

		#region Image to blocks

		internal static RawBlock4X4Rgba32[] ImageTo4X4(ReadOnlyMemory2D<ColorRgba32> image, out int blocksWidth, out int blocksHeight)
		{
			blocksWidth = ((image.Width + 3) & ~3) >> 2;
			blocksHeight = ((image.Height + 3) & ~3) >> 2;
			
			var output = new RawBlock4X4Rgba32[blocksWidth * blocksHeight];

			var span = image.Span;

			for (var y = 0; y < image.Height; y++)
			{
				for (var x = 0; x < image.Width; x++)
				{
					var color = span[y, x];

					var blockIndexX = x >> 2;
					var blockIndexY = y >> 2;
					var blockInternalIndexX = x & 3;
					var blockInternalIndexY = y & 3;

					output[blockIndexX + blockIndexY * blocksWidth][blockInternalIndexX, blockInternalIndexY] = color;
				}
			}

			// Fill in block y with edge color
			if ((image.Height & 3) != 0)
			{
				var yPaddingStart = image.Height & 3;
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
			if ((image.Width & 3) != 0)
			{
				var xPaddingStart = image.Width & 3;
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

		internal static RawBlock4X4RgbFloat[] ImageTo4X4(ReadOnlyMemory2D<ColorRgbFloat> image, out int blocksWidth, out int blocksHeight)
		{
			blocksWidth = ((image.Width + 3) & ~3) >> 2;
			blocksHeight = ((image.Height + 3) & ~3) >> 2;

			var output = new RawBlock4X4RgbFloat[blocksWidth * blocksHeight];

			var span = image.Span;

			for (var y = 0; y < image.Height; y++)
			{
				for (var x = 0; x < image.Width; x++)
				{
					var color = span[y, x];

					var blockIndexX = x >> 2;
					var blockIndexY = y >> 2;
					var blockInternalIndexX = x & 3;
					var blockInternalIndexY = y & 3;

					output[blockIndexX + blockIndexY * blocksWidth][blockInternalIndexX, blockInternalIndexY] = color;
				}
			}

			// Fill in block y with edge color
			if ((image.Height & 3) != 0)
			{
				var yPaddingStart = image.Height & 3;
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
			if ((image.Width & 3) != 0)
			{
				var xPaddingStart = image.Width & 3;
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

		#endregion

		public static int CalculateNumOfBlocks(int pixelWidth, int pixelHeight)
		{
			var blocksWidth = ((pixelWidth + 3) & ~3) >> 2;
			var blocksHeight = ((pixelHeight + 3) & ~3) >> 2;

			return blocksWidth * blocksHeight;
		}
		public static void CalculateNumOfBlocks(int pixelWidth, int pixelHeight, out int blocksWidth, out int blocksHeight)
		{
			blocksWidth = ((pixelWidth + 3) & ~3) >> 2;
			blocksHeight = ((pixelHeight + 3) & ~3) >> 2;
		}
	}
}
