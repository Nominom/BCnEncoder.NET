using System;
using System.Collections.Generic;
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
		/// <param name="sourceImage">The image to scale down.</param>
		/// <param name="numMipMaps">The number of mipmaps to generate.</param>
		/// <returns></returns>
		/// <returns>Will generate as many mipmaps as possible until a mipmap of 1x1 is reached for <paramref name="numMipMaps"/> 0 or smaller.</returns>
		public static List<Image<Rgba32>> GenerateMipChain(Image<Rgba32> sourceImage, ref int numMipMaps)
		{
			var result = new List<Image<Rgba32>> { sourceImage.Clone() };

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
				var mipWidth = Math.Max(1, sourceImage.Width / (int)Math.Pow(2, mipLevel));
				var mipHeight = Math.Max(1, sourceImage.Height / (int)Math.Pow(2, mipLevel));

				var newImage = sourceImage.Clone(x => x.Resize(mipWidth, mipHeight));
				result.Add(newImage);

				// Stop generating of last generated mipmap was of size 1x1
				if (mipWidth == 1 && mipHeight == 1)
				{
					numMipMaps = mipLevel + 1;
					break;
				}
			}

			return result;
		}
	}
}
