using System;
using Microsoft.Toolkit.HighPerformance;

namespace BCnEncoder.Shared
{
	internal static class MipMapper
	{
		/// <summary>
		/// Generate a chain of <paramref name="numMipMaps"/> elements.
		/// Output texture format will be <see cref="CompressionFormat.RgbaFloat"/>
		/// </summary>
		/// <param name="input">The original image to scale down.</param>
		/// <param name="numMipMaps">The number of mipmaps to generate.</param>
		/// <returns> If <paramref name="numMipMaps"/> is 0 or smaller, will generate as many mipmaps as possible until a mipmap of 1x1 is reached.</returns>
		public static BCnTextureData GenerateMipChainHdr(BCnTextureData input, ref int numMipMaps)
		{
			numMipMaps = CalculateMipChainLength(input.Width, input.Height, numMipMaps);

			input = input.ConvertTo(CompressionFormat.RgbaFloat);
			
			if (input.MipLevels.Length == numMipMaps)
			{
				return input;
			}

			var newData = new BCnTextureData(CompressionFormat.RgbaFloat, input.Width, input.Height, numMipMaps,
				input.IsCubeMap, true);

			for (var f = 0; f < newData.NumFaces; f++)
			{
				var rgbaFloatMemory = input.Faces[f].Mips[0].AsMemory2D<ColorRgbaFloat>();
				var chain = GenerateMipChain(rgbaFloatMemory, ref numMipMaps);

				for (var i = 0; i < numMipMaps; i++)
				{
					chain[i].CopyTo(newData.Faces[f].Mips[i].AsMemory2D<ColorRgbaFloat>());
				}
			}

			return newData;
		}

		/// <summary>
		/// Generate a chain of <paramref name="numMipMaps"/> elements.
		/// Output texture format will be <see cref="CompressionFormat.Rgba32"/>
		/// </summary>
		/// <param name="input">The original image to scale down.</param>
		/// <param name="numMipMaps">The number of mipmaps to generate.</param>
		/// <returns> If <paramref name="numMipMaps"/> is 0 or smaller, will generate as many mipmaps as possible until a mipmap of 1x1 is reached.</returns>
		public static BCnTextureData GenerateMipChainLdr(BCnTextureData input, ref int numMipMaps)
		{
			numMipMaps = CalculateMipChainLength(input.Width, input.Height, numMipMaps);

			input = input.ConvertTo(CompressionFormat.Rgba32);

			if (input.MipLevels.Length == numMipMaps)
			{
				return input;
			}

			var newData = new BCnTextureData(CompressionFormat.Rgba32, input.Width, input.Height, numMipMaps,
				input.IsCubeMap, true);

			for (var f = 0; f < newData.NumFaces; f++)
			{
				var rgbaMemory = input.Faces[f].Mips[0].AsMemory2D<ColorRgba32>();
				var chain = GenerateMipChain(rgbaMemory, ref numMipMaps);

				for (var i = 0; i < numMipMaps; i++)
				{
					chain[i].CopyTo(newData.Faces[f].Mips[i].AsMemory2D<ColorRgba32>());
				}
			}

			return newData;
		}

		/// <summary>
		/// Generate a chain of <paramref name="numMipMaps"/> elements.
		/// </summary>
		/// <param name="input">The original image to scale down.</param>
		/// <param name="width">The original image width.</param>
		/// <param name="height">The original image height.</param>
		/// <param name="numMipMaps">The number of mipmaps to generate.</param>
		/// <returns> If <paramref name="numMipMaps"/> is 0 or smaller, will generate as many mipmaps as possible until a mipmap of 1x1 is reached.</returns>
		public static ReadOnlyMemory2D<ColorRgba32>[] GenerateMipChain(ReadOnlyMemory<ColorRgba32> input, int width, int height, ref int numMipMaps)
		{
			return GenerateMipChain(input.AsMemory2D(height, width), ref numMipMaps);
		}

		/// <summary>
		/// Generate a chain of <paramref name="numMipMaps"/> elements.
		/// </summary>
		/// <param name="pixels">The original image to scale down.</param>
		/// <param name="numMipMaps">The number of mipmaps to generate.</param>
		/// <returns> If <paramref name="numMipMaps"/> is 0 or smaller, will generate as many mipmaps as possible until a mipmap of 1x1 is reached.</returns>
		public static ReadOnlyMemory2D<ColorRgba32>[] GenerateMipChain(ReadOnlyMemory2D<ColorRgba32> pixels, ref int numMipMaps)
		{
			var width = pixels.Width;
			var height = pixels.Height;
			var mipChainLength = CalculateMipChainLength(width, height, numMipMaps);

			var result = new ReadOnlyMemory2D<ColorRgba32>[mipChainLength];
			result[0] = pixels;

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
				var mipWidth = Math.Max(1, width >> mipLevel);
				var mipHeight = Math.Max(1, height >> mipLevel);
				
				var newMip = ResizeToHalf(result[mipLevel - 1].Span);
				result[mipLevel] = newMip;

				// Stop generating if last generated mipmap was of size 1x1
				if (mipWidth == 1 && mipHeight == 1)
				{
					numMipMaps = mipLevel + 1;
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// Generate a chain of <paramref name="numMipMaps"/> elements.
		/// </summary>
		/// <param name="input">The original image to scale down.</param>
		/// <param name="width">The original image width.</param>
		/// <param name="height">The original image height.</param>
		/// <param name="numMipMaps">The number of mipmaps to generate.</param>
		/// <returns> If <paramref name="numMipMaps"/> is 0 or smaller, will generate as many mipmaps as possible until a mipmap of 1x1 is reached.</returns>
		public static ReadOnlyMemory2D<ColorRgbaFloat>[] GenerateMipChain(ReadOnlyMemory<ColorRgbaFloat> input, int width, int height, ref int numMipMaps)
		{
			return GenerateMipChain(input.AsMemory2D(height, width), ref numMipMaps);
		}

		/// <summary>
		/// Generate a chain of <paramref name="numMipMaps"/> elements.
		/// </summary>
		/// <param name="pixels">The original image to scale down.</param>
		/// <param name="numMipMaps">The number of mipmaps to generate.</param>
		/// <returns>Will generate as many mipmaps as possible until a mipmap of 1x1 is reached for <paramref name="numMipMaps"/> 0 or smaller.</returns>
		public static ReadOnlyMemory2D<ColorRgbaFloat>[] GenerateMipChain(ReadOnlyMemory2D<ColorRgbaFloat> pixels, ref int numMipMaps)
		{
			var width = pixels.Width;
			var height = pixels.Height;
			var mipChainLength = CalculateMipChainLength(width, height, numMipMaps);

			var result = new ReadOnlyMemory2D<ColorRgbaFloat>[mipChainLength];
			result[0] = pixels;

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
				var mipWidth = Math.Max(1, width >> mipLevel);
				var mipHeight = Math.Max(1, height >> mipLevel);

				var newMip = ResizeToHalf(result[mipLevel - 1].Span);
				result[mipLevel] = newMip;

				// Stop generating if last generated mipmap was of size 1x1
				if (mipWidth == 1 && mipHeight == 1)
				{
					numMipMaps = mipLevel + 1;
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// Generate a single mipmap of the specified mipLevel.
		/// </summary>
		/// <param name="pixels">The original image to scale down.</param>
		/// <param name="mipLevel">The mipLevel to generate.</param>
		public static ReadOnlyMemory2D<ColorRgbaFloat> GenerateSingleMip(ReadOnlyMemory2D<ColorRgbaFloat> pixels, int mipLevel)
		{
			var width = pixels.Width;
			var height = pixels.Height;
			var mipChainLength = CalculateMipChainLength(width, height, -1);

			if (mipLevel >= mipChainLength)
			{
				throw new InvalidOperationException("Requested mipLevel too high.");
			}

			// If only one mipmap was requested, return original image only
			if (mipLevel == 0)
			{
				return pixels;
			}

			// Generate mipmap
			for (var level = 1; level <= mipLevel; level++)
			{
				var mipWidth = Math.Max(1, width >> level);
				var mipHeight = Math.Max(1, height >> level);

				pixels = ResizeToHalf(pixels.Span);

				// Stop generating if last generated mipmap was of size 1x1
				if (mipWidth == 1 && mipHeight == 1)
				{
					break;
				}
			}

			return pixels;
		}

		/// <summary>
		/// Generate a single mipmap of the specified mipLevel.
		/// </summary>
		/// <param name="pixels">The original image to scale down.</param>
		/// <param name="mipLevel">The mipLevel to generate.</param>
		public static ReadOnlyMemory2D<ColorRgba32> GenerateSingleMip(ReadOnlyMemory2D<ColorRgba32> pixels, int mipLevel)
		{
			var width = pixels.Width;
			var height = pixels.Height;
			var mipChainLength = CalculateMipChainLength(width, height, -1);

			if (mipLevel >= mipChainLength)
			{
				throw new InvalidOperationException("Requested mipLevel too high.");
			}

			// If only one mipmap was requested, return original image only
			if (mipLevel == 0)
			{
				return pixels;
			}

			// Generate mipmap
			for (var level = 1; level <= mipLevel; level++)
			{
				var mipWidth = Math.Max(1, width >> level);
				var mipHeight = Math.Max(1, height >> level);

				pixels = ResizeToHalf(pixels.Span);

				// Stop generating if last generated mipmap was of size 1x1
				if (mipWidth == 1 && mipHeight == 1)
				{
					break;
				}
			}

			return pixels;
		}

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
			for (var mipLevel = 1; mipLevel <= maxNumMipMaps; mipLevel++)
			{
				var mipWidth = Math.Max(1, width >> mipLevel);
				var mipHeight = Math.Max(1, height >> mipLevel);

				if (mipLevel == maxNumMipMaps)
				{
					return maxNumMipMaps;
				}

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
			mipWidth = Math.Max(1, width >> mipIdx);
			mipHeight = Math.Max(1, height >> mipIdx);
		}

		private static ColorRgba32[,] ResizeToHalf(ReadOnlySpan2D<ColorRgba32> pixelsRgba)
		{

			var oldWidth = pixelsRgba.Width;
			var oldHeight = pixelsRgba.Height;
			var newWidth = Math.Max(1, oldWidth >> 1);
			var newHeight = Math.Max(1, oldHeight >> 1);
			
			var result = new ColorRgba32[newHeight, newWidth];
			
			int ClampW(int x) => Math.Max(0, Math.Min(oldWidth - 1, x));
			int ClampH(int y) => Math.Max(0, Math.Min(oldHeight - 1, y));
			
			for (var y2 = 0; y2 < newHeight; y2++)
			{
				for (var x2 = 0; x2 < newWidth; x2++)
				{
					var ul = pixelsRgba[ClampH(y2 * 2), ClampW(x2 * 2)].ToColorRgbaFloat();
					var ur = pixelsRgba[ClampH(y2 * 2), ClampW(x2 * 2 + 1)].ToColorRgbaFloat();
					var ll = pixelsRgba[ClampH(y2 * 2 + 1), ClampW(x2 * 2)].ToColorRgbaFloat();
					var lr = pixelsRgba[ClampH(y2 * 2 + 1), ClampW(x2 * 2 + 1)].ToColorRgbaFloat();

					result[y2, x2] = ((ul + ur + ll + lr) / 4).ToRgba32();
				}
			}

			return result;
		}

		private static ColorRgbaFloat[,] ResizeToHalf(ReadOnlySpan2D<ColorRgbaFloat> pixelsRgba)
		{

			var oldWidth = pixelsRgba.Width;
			var oldHeight = pixelsRgba.Height;
			var newWidth = Math.Max(1, oldWidth >> 1);
			var newHeight = Math.Max(1, oldHeight >> 1);

			var result = new ColorRgbaFloat[newHeight, newWidth];

			int ClampW(int x) => Math.Max(0, Math.Min(oldWidth - 1, x));
			int ClampH(int y) => Math.Max(0, Math.Min(oldHeight - 1, y));

			for (var y2 = 0; y2 < newHeight; y2++)
			{
				for (var x2 = 0; x2 < newWidth; x2++)
				{
					var ul = pixelsRgba[ClampH(y2 * 2), ClampW(x2 * 2)];
					var ur = pixelsRgba[ClampH(y2 * 2), ClampW(x2 * 2 + 1)];
					var ll = pixelsRgba[ClampH(y2 * 2 + 1), ClampW(x2 * 2)];
					var lr = pixelsRgba[ClampH(y2 * 2 + 1), ClampW(x2 * 2 + 1)];

					result[y2, x2] = ((ul + ur + ll + lr) / 4);
				}
			}

			return result;
		}
	}
}
