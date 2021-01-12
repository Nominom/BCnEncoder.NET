using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace BCnEncoder.Shared
{
	internal static class MipMapper
	{

		public static uint CalculateMipChainLength(int width, int height, uint maxNumMipMaps) {
			if (maxNumMipMaps == 1) {
				return 1;
			}
			if (maxNumMipMaps == 0) {
				maxNumMipMaps = 999;
			}
			uint output = 0;
			for (uint mipLevel = 1; mipLevel < maxNumMipMaps; mipLevel++) {
				var mipWidth = Math.Max(1, width / (int)Math.Pow(2, mipLevel));
				var mipHeight = Math.Max(1, height / (int)Math.Pow(2, mipLevel));
				if (mipWidth == 1 && mipHeight == 1) {
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

			if (mipIdx == 0) return;
			
			for (uint mipLevel = 1; mipLevel < 99999; mipLevel++)
			{
				mipWidth = Math.Max(1, width / (int)Math.Pow(2, mipLevel));
				mipHeight = Math.Max(1, height / (int)Math.Pow(2, mipLevel));
				if (mipLevel == mipIdx) return;
				if (mipWidth == 1 && mipHeight == 1)
				{
					return;
				}
			}
		}

		public static List<Image<Rgba32>> GenerateMipChain(Image<Rgba32> sourceImage, ref uint numMipMaps) {
			var result = new List<Image<Rgba32>>();
			result.Add(sourceImage.Clone());

			if (numMipMaps == 1) {
				return result;
			}

			if (numMipMaps == 0) {
				numMipMaps = 999;
			}

			for (uint mipLevel = 1; mipLevel < numMipMaps; mipLevel++) {
				var mipWidth = Math.Max(1, sourceImage.Width / (int)Math.Pow(2, mipLevel));
				var mipHeight = Math.Max(1, sourceImage.Height / (int)Math.Pow(2, mipLevel));

				var newImage = sourceImage.Clone(x => x.Resize(mipWidth, mipHeight));
				result.Add(newImage);

				if (mipWidth == 1 && mipHeight == 1) {
					numMipMaps = mipLevel + 1;
					break;
				}
			}

			return result;
		}
	}
}
