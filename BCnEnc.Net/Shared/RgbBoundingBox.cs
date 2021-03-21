using System;
using System.Runtime.InteropServices;

namespace BCnEncoder.Shared
{
	/// <summary>
	/// Calculate the bounding box of rgb values as described in
	/// "Real-Time DXT Compression by J.M.P. van Waveren, 2006, Id Software, Inc." and
	/// "Real-Time YCoCg-DXT Compression J.M.P. van Waveren,  Ignacio Castaño id Software, Inc. NVIDIA Corp."
	/// </summary>
	internal static class RgbBoundingBox
	{

		public static void Create565(ReadOnlySpan<ColorRgba32> colors, out ColorRgb565 min, out ColorRgb565 max)
		{
			const int colorInsetShift = 4;
			const int c5655Mask = 0xF8;
			const int c5656Mask = 0xFC;

			int minR = 255,
				minG = 255,
				minB = 255;
			int maxR = 0,
				maxG = 0,
				maxB = 0;

			for (var i = 0; i < colors.Length; i++)
			{
				var c = colors[i];

				if (c.r < minR) minR = c.r;
				if (c.g < minG) minG = c.g;
				if (c.b < minB) minB = c.b;

				if (c.r > maxR) maxR = c.r;
				if (c.g > maxG) maxG = c.g;
				if (c.b > maxB) maxB = c.b;
			}

			var insetR = (maxR - minR) >> colorInsetShift;
			var insetG = (maxG - minG) >> colorInsetShift;
			var insetB = (maxB - minB) >> colorInsetShift;

			// Inset by 1/16th
			minR = ((minR << colorInsetShift) + insetR) >> colorInsetShift;
			minG = ((minG << colorInsetShift) + insetG) >> colorInsetShift;
			minB = ((minB << colorInsetShift) + insetB) >> colorInsetShift;

			maxR = ((maxR << colorInsetShift) - insetR) >> colorInsetShift;
			maxG = ((maxG << colorInsetShift) - insetG) >> colorInsetShift;
			maxB = ((maxB << colorInsetShift) - insetB) >> colorInsetShift;

			minR = minR >= 0 ? minR : 0;
			minG = minG >= 0 ? minG : 0;
			minB = minB >= 0 ? minB : 0;

			maxR = maxR <= 255 ? maxR : 255;
			maxG = maxG <= 255 ? maxG : 255;
			maxB = maxB <= 255 ? maxB : 255;

			// Optimal rounding
			minR = (minR & c5655Mask) | (minR >> 5);
			minG = (minG & c5656Mask) | (minG >> 6);
			minB = (minB & c5655Mask) | (minB >> 5);

			maxR = (maxR & c5655Mask) | (maxR >> 5);
			maxG = (maxG & c5656Mask) | (maxG >> 6);
			maxB = (maxB & c5655Mask) | (maxB >> 5);

			min = new ColorRgb565((byte)minR, (byte)minG, (byte)minB);
			max = new ColorRgb565((byte)maxR, (byte)maxG, (byte)maxB);
		}

		public static void Create565AlphaCutoff(ReadOnlySpan<ColorRgba32> colors, out ColorRgb565 min, out ColorRgb565 max, int alphaCutoff = 128)
		{
			const int colorInsetShift = 4;
			const int c5655Mask = 0xF8;
			const int c5656Mask = 0xFC;

			int minR = 255,
				minG = 255,
				minB = 255;
			int maxR = 0,
				maxG = 0,
				maxB = 0;

			for (var i = 0; i < colors.Length; i++)
			{
				var c = colors[i];
				if (c.a < alphaCutoff) continue;
				if (c.r < minR) minR = c.r;
				if (c.g < minG) minG = c.g;
				if (c.b < minB) minB = c.b;

				if (c.r > maxR) maxR = c.r;
				if (c.g > maxG) maxG = c.g;
				if (c.b > maxB) maxB = c.b;
			}

			var insetR = (maxR - minR) >> colorInsetShift;
			var insetG = (maxG - minG) >> colorInsetShift;
			var insetB = (maxB - minB) >> colorInsetShift;

			minR = ((minR << colorInsetShift) + insetR) >> colorInsetShift;
			minG = ((minG << colorInsetShift) + insetG) >> colorInsetShift;
			minB = ((minB << colorInsetShift) + insetB) >> colorInsetShift;

			maxR = ((maxR << colorInsetShift) - insetR) >> colorInsetShift;
			maxG = ((maxG << colorInsetShift) - insetG) >> colorInsetShift;
			maxB = ((maxB << colorInsetShift) - insetB) >> colorInsetShift;

			minR = minR >= 0 ? minR : 0;
			minG = minG >= 0 ? minG : 0;
			minB = minB >= 0 ? minB : 0;

			maxR = maxR <= 255 ? maxR : 255;
			maxG = maxG <= 255 ? maxG : 255;
			maxB = maxB <= 255 ? maxB : 255;

			minR = (minR & c5655Mask) | (minR >> 5);
			minG = (minG & c5656Mask) | (minG >> 6);
			minB = (minB & c5655Mask) | (minB >> 5);

			maxR = (maxR & c5655Mask) | (maxR >> 5);
			maxG = (maxG & c5656Mask) | (maxG >> 6);
			maxB = (maxB & c5655Mask) | (maxB >> 5);

			min = new ColorRgb565((byte)minR, (byte)minG, (byte)minB);
			max = new ColorRgb565((byte)maxR, (byte)maxG, (byte)maxB);
		}

		public static void Create565A(ReadOnlySpan<ColorRgba32> colors, out ColorRgb565 min, out ColorRgb565 max, out byte minAlpha, out byte maxAlpha)
		{
			const int colorInsetShift = 4;
			const int alphaInsetShift = 5;
			const int c5655Mask = 0xF8;
			const int c5656Mask = 0xFC;

			int minR = 255,
				minG = 255,
				minB = 255,
				minA = 255;
			int maxR = 0,
				maxG = 0,
				maxB = 0,
				maxA = 0;

			for (var i = 0; i < colors.Length; i++)
			{
				var c = colors[i];
				if (c.r < minR) minR = c.r;
				if (c.g < minG) minG = c.g;
				if (c.b < minB) minB = c.b;
				if (c.a < minA) minA = c.a;

				if (c.r > maxR) maxR = c.r;
				if (c.g > maxG) maxG = c.g;
				if (c.b > maxB) maxB = c.b;
				if (c.a > maxA) maxA = c.a;
			}


			var insetR = (maxR - minR) >> colorInsetShift;
			var insetG = (maxG - minG) >> colorInsetShift;
			var insetB = (maxB - minB) >> colorInsetShift;
			var insetA = (maxA - minA) >> alphaInsetShift;

			minR = ((minR << colorInsetShift) + insetR) >> colorInsetShift;
			minG = ((minG << colorInsetShift) + insetG) >> colorInsetShift;
			minB = ((minB << colorInsetShift) + insetB) >> colorInsetShift;
			minA = ((minA << alphaInsetShift) + insetA) >> alphaInsetShift;

			maxR = ((maxR << colorInsetShift) - insetR) >> colorInsetShift;
			maxG = ((maxG << colorInsetShift) - insetG) >> colorInsetShift;
			maxB = ((maxB << colorInsetShift) - insetB) >> colorInsetShift;
			maxA = ((maxA << alphaInsetShift) - insetA) >> alphaInsetShift;

			minR = minR >= 0 ? minR : 0;
			minG = minG >= 0 ? minG : 0;
			minB = minB >= 0 ? minB : 0;
			minA = minA >= 0 ? minA : 0;

			maxR = maxR <= 255 ? maxR : 255;
			maxG = maxG <= 255 ? maxG : 255;
			maxB = maxB <= 255 ? maxB : 255;
			maxA = maxA <= 255 ? maxA : 255;

			minR = (minR & c5655Mask) | (minR >> 5);
			minG = (minG & c5656Mask) | (minG >> 6);
			minB = (minB & c5655Mask) | (minB >> 5);

			maxR = (maxR & c5655Mask) | (maxR >> 5);
			maxG = (maxG & c5656Mask) | (maxG >> 6);
			maxB = (maxB & c5655Mask) | (maxB >> 5);

			min = new ColorRgb565((byte)minR, (byte)minG, (byte)minB);
			max = new ColorRgb565((byte)maxR, (byte)maxG, (byte)maxB);
			minAlpha = (byte)minA;
			maxAlpha = (byte)maxA;
		}

		/// <summary>
		/// Hdr rgb bounding box inset by Krzysztof Narkowicz. https://github.com/knarkowicz/GPURealTimeBC6H
		/// Code is public domain.
		/// </summary>
		private static void InsetHdrChannel(ReadOnlySpan<ColorRgbFloat> colors, int channel, ref float blockMax, ref float blockMin)
		{
			var offset = 0f;
			if (blockMin < 0)
			{
				offset = -blockMin;
				blockMin += offset;
				blockMax += offset;
			}
			
			float Select(ReadOnlySpan<float> span, int i)
			{
				return span[i * 3 + channel] + offset;
			}
			
			var floats = MemoryMarshal.Cast<ColorRgbFloat, float>(colors);
			
			var refinedBlockMin = blockMax;
			var refinedBlockMax = blockMin;

			for (var i = 0; i < 16; ++i)
			{
				refinedBlockMin = MathF.Min(refinedBlockMin, Select(floats, i) == blockMin ? refinedBlockMin : Select(floats, i));
				refinedBlockMax = MathF.Max(refinedBlockMax, Select(floats, i) == blockMax ? refinedBlockMax : Select(floats, i));
			}

			var logRefinedBlockMax = MathF.Log(refinedBlockMax + 1.0f, 2);
			var logRefinedBlockMin = MathF.Log(refinedBlockMin + 1.0f, 2);

			var logBlockMax = MathF.Log(blockMax + 1.0f, 2);
			var logBlockMin = MathF.Log(blockMin + 1.0f, 2);
			var logBlockMaxExt = (logBlockMax - logBlockMin) * (1.0f / 32.0f);

			logBlockMin += MathF.Min(logRefinedBlockMin - logBlockMin, logBlockMaxExt);
			logBlockMax -= MathF.Min(logBlockMax - logRefinedBlockMax, logBlockMaxExt);

			blockMin = MathF.Pow(2, logBlockMin) - 1.0f - offset;
			blockMax = MathF.Pow(2, logBlockMax) - 1.0f - offset;
		}

		public static void CreateFloat(ReadOnlySpan<ColorRgbFloat> colors, out ColorRgbFloat min, out ColorRgbFloat max)
		{

			float minR = float.MaxValue,
				  minG = float.MaxValue,
				  minB = float.MaxValue;
			float maxR = float.MinValue,
				  maxG = float.MinValue,
				  maxB = float.MinValue;

			for (var i = 0; i < colors.Length; i++)
			{
				var c = colors[i];

				if (c.r < minR) minR = c.r;
				if (c.g < minG) minG = c.g;
				if (c.b < minB) minB = c.b;

				if (c.r > maxR) maxR = c.r;
				if (c.g > maxG) maxG = c.g;
				if (c.b > maxB) maxB = c.b;
			}

			InsetHdrChannel(colors, 0, ref maxR, ref minR);
			InsetHdrChannel(colors, 1, ref maxG, ref minG);
			InsetHdrChannel(colors, 2, ref maxB, ref minB);

			min = new ColorRgbFloat(minR, minG, minB);
			max = new ColorRgbFloat(maxR, maxG, maxB);
		}
	}
}
