using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.HighPerformance.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace BCnEncoder.Shared
{
	internal static class MipMapper
	{

		public static int CalculateMipChainLength(int width, int height, int maxNumMipMaps)
		{
			if (maxNumMipMaps == 1)
			{
				return 1;
			}

			if (maxNumMipMaps <= 0)
			{
				maxNumMipMaps = int.MaxValue;
			}

			var output = 0;
			for (var mipLevel = 1; mipLevel < maxNumMipMaps; mipLevel++)
			{
				var mipWidth = Math.Max(1, width / (int)Math.Pow(2, mipLevel));
				var mipHeight = Math.Max(1, height / (int)Math.Pow(2, mipLevel));

				if (mipWidth == 1 && mipHeight == 1)
				{
					output = mipLevel + 1;
					break;
				}
			}

			return output;
		}

		public static void CalculateMipLevelSize(int width, int height, int mipIdx, out int mipWidth, out int mipHeight)
		{
			mipWidth = width;
			mipHeight = height;

			if (mipIdx == 0)
			{
				return;
			}

			for (var mipLevel = 1; mipLevel < int.MaxValue; mipLevel++)
			{
				mipWidth = Math.Max(1, width / (int)Math.Pow(2, mipLevel));
				mipHeight = Math.Max(1, height / (int)Math.Pow(2, mipLevel));

				if (mipLevel == mipIdx)
				{
					return;
				}

				if (mipWidth == 1 && mipHeight == 1)
				{
					return;
				}
			}
		}

		/// <summary>
		/// Generate a chain of <paramref name="numMipMaps"/> elements.
		/// </summary>
		/// <param name="pixelsRgba">The original image to scale down.</param>
		/// <param name="width">The original image width.</param>
		/// <param name="height">The original image height.</param>
		/// <param name="numMipMaps">The number of mipmaps to generate.</param>
		/// <returns></returns>
		/// <returns>Will generate as many mipmaps as possible until a mipmap of 1x1 is reached for <paramref name="numMipMaps"/> 0 or smaller.</returns>
		public static List<Memory2D<ColorRgba32>> GenerateMipChain(ReadOnlySpan<byte> pixelsRgba, int width, int height, ref int numMipMaps)
		{
			var span2d = ReadOnlySpan2D<ColorRgba32>.DangerousCreate(
				MemoryMarshal.Cast<byte, ColorRgba32>(pixelsRgba)[0], height, width, 0);
			
			var result = new List<Memory2D<ColorRgba32>> {  span2d.ToArray() };

			// If only one mipmap was requested, return original image only
			if (numMipMaps == 1)
			{
				return result;
			}

			// If number of mipmaps is "marked as boundless", do as many mipmaps as it takes to reach a size of 1x1
			if (numMipMaps <= 0)
			{
				numMipMaps = int.MaxValue;
			}

			// Generate mipmaps
			for (var mipLevel = 1; mipLevel < numMipMaps; mipLevel++)
			{
				var mipWidth = Math.Max(1, width / (int)Math.Pow(2, mipLevel));
				var mipHeight = Math.Max(1, height / (int)Math.Pow(2, mipLevel));

				ColorRgba32[,] newMip = Resize(result[mipLevel - 1].Span, mipWidth, mipHeight);
				result.Add(newMip);

				// Stop generating if last generated mipmap was of size 1x1
				if (mipWidth == 1 && mipHeight == 1)
				{
					numMipMaps = mipLevel + 1;
					break;
				}
			}

			return result;
		}

		private static ColorRgba32[,] Resize(ReadOnlySpan2D<ColorRgba32> pixelsRgba, int newWidth, int newHeight)
		{
			//TODO: Make better
			
			ColorRgba32[,] result = new ColorRgba32[newHeight, newWidth];
			var oldWidth = pixelsRgba.Width;
			var oldHeight = pixelsRgba.Height;
			
			for (int x = 0; x < newWidth; x++)
			{
				for (int y = 0; y < newHeight; y++)
				{
					var xOrig = (int) (x / (double) newWidth * oldWidth);
					var yOrig = (int) (y / (double) newHeight * oldHeight);
					result[y, x] = pixelsRgba[yOrig, xOrig];
				}
			}
			
			return result;
		}
	}
}
