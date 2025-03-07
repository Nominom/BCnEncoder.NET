using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Bptc
{
	internal static class Bc6EncodingHelpers
	{

		/// <summary>
		/// Opposite of <see cref="Bc6Block.FinishUnQuantize(int,bool)"/>
		/// </summary>
		internal static int PreQuantize(float value, bool signed)
		{
			var half = new Half(value);
			var bits = (int)Half.GetBits(half);
			if (!signed)
			{

				return (bits << 6) / 31;
			}
			else
			{
				const int signMask = ~0x8000;

				if (half < new Half(0))
				{
					var component = -(bits & signMask);

					return -(((-component) << 5) / 31); //-(((-component) * 31) >> 5)
				}

				return (bits << 5) / 31;
			}
		}

		/// <summary>
		/// Opposite of <see cref="Bc6Block.UnQuantize(int,int,bool)"/>
		/// </summary>
		internal static int Quantize(int component, int endpointBits, bool signed)
		{
			if (!signed)
			{
				if (endpointBits >= 15)
					return component;
				if (component == 0)
					return 0;
				if (component == 0xFFFF)
					return ((1 << endpointBits) - 1);
				else
					return ((component << endpointBits) - 0x8000) >> 16;

			}
			else
			{
				if (endpointBits >= 16)
					return component;
				else
				{
					if (component == 0) return 0;
					if (component > 0)
					{
						if (component == 0x7FFF)
						{
							return (1 << (endpointBits - 1)) - 1;
						}

						return ((component << (endpointBits - 1)) - 0x4000) >> 15;
					}
					else
					{
						if (-component == 0x7FFF)
						{
							return -((1 << (endpointBits - 1)) - 1);
						}

						return -(((-component << (endpointBits - 1)) + 0x4000) >> 15);
					}
				}
			}

		}

		public static (int, int, int) PreQuantizeRawEndpoint(ColorRgbFloat endpoint, bool signed)
		{
			var r = PreQuantize(endpoint.r, signed);
			var g = PreQuantize(endpoint.g, signed);
			var b = PreQuantize(endpoint.b, signed);

			return (
				r,
				g,
				b
			);
		}

		public static (int, int, int) FinishQuantizeEndpoint((int, int, int) endpoint, int endpointBits, bool signed)
		{
			return (
				Quantize(endpoint.Item1, endpointBits, signed),
				Quantize(endpoint.Item2, endpointBits, signed),
				Quantize(endpoint.Item3, endpointBits, signed)
			);
		}

		public static int CreateTranformedEndpoint(int quantizedEp0,
			int quantizedEpT, int deltaBits, ref bool badTransform)
		{
			var delta = quantizedEpT - quantizedEp0;
			var max = (1 << (deltaBits - 1));

			if (delta >= 0 ? delta >= max : -delta > max) // delta overflow
			{
				badTransform = true;
			}

			if (delta >= 0)
			{
				if (delta >= max)
				{
					delta = max - 1;
				}
			}
			else if (-delta > max)
			{
				delta = max;
			}
			else
			{
				delta = delta & ((1 << deltaBits) - 1);
			}
			return delta;
		}

		public static (int, int, int) CreateTransformedEndpoint((int, int, int) quantizedEp0,
			(int, int, int) quantizedEpT, (int, int, int) deltaBits, ref bool badTransform)
		{
			return (
				CreateTranformedEndpoint(quantizedEp0.Item1, quantizedEpT.Item1, deltaBits.Item1, ref badTransform),
				CreateTranformedEndpoint(quantizedEp0.Item2, quantizedEpT.Item2, deltaBits.Item2, ref badTransform),
				CreateTranformedEndpoint(quantizedEp0.Item3, quantizedEpT.Item3, deltaBits.Item3, ref badTransform)
			);
		}

		public static void GeneratePalette(Span<ColorRgbFloat> palette, (int, int, int) unQuantizedEp0, (int, int, int) unQuantizedEp1, int indexPrecision, bool signed)
		{
			var paletteSize = 1 << indexPrecision;

			for (var i = 0; i < paletteSize; i++)
			{
				var interpolated = Bc6Block.InterpolateColor(unQuantizedEp0, unQuantizedEp1, i, indexPrecision);
				var (r, g, b) = Bc6Block.FinishUnQuantize(interpolated, signed);
				palette[i] = new ColorRgbFloat(r, g, b);
			}
		}

		public static void GeneratePaletteInt(Span<(int, int, int)> palette, (int, int, int) unQuantizedEp0, (int, int, int) unQuantizedEp1, int indexPrecision, bool signed)
		{
			var paletteSize = 1 << indexPrecision;

			for (var i = 0; i < paletteSize; i++)
			{
				var interpolated = Bc6Block.InterpolateColor(unQuantizedEp0, unQuantizedEp1, i, indexPrecision);
				palette[i] = interpolated;
			}
		}

		private static int FindClosestColorIndexInt((int, int, int) color, ReadOnlySpan<(int, int, int)> colors, out float bestError)
		{
			static float CalculateError((int, int, int) c0, (int, int, int) c1) =>
					Math.Abs(c0.Item1 - c1.Item1) +
					Math.Abs(c0.Item2 - c1.Item2) +
					Math.Abs(c0.Item3 - c1.Item3);

			bestError = CalculateError(color, colors[0]);
			var bestIndex = 0;
			for (var i = 1; i < colors.Length; i++)
			{
				var error = CalculateError(color, colors[i]);
				if (error < bestError)
				{
					bestIndex = i;
					bestError = error;
				}
				if (bestError == 0)
				{
					break;
				}
			}
			return bestIndex;
		}

		private static int FindClosestColorIndex(ColorRgbFloat color, ReadOnlySpan<ColorRgbFloat> colors, out float bestError)
		{
			bestError = color.CalcLogDist(colors[0]);
			var bestIndex = 0;
			for (var i = 1; i < colors.Length; i++)
			{
				var error = color.CalcLogDist(colors[i]);
				if (error < bestError)
				{
					bestIndex = i;
					bestError = error;
				}
				if (bestError == 0)
				{
					break;
				}
			}
			return bestIndex;
		}

		public static float FindOptimalIndicesInt1Sub(RawBlock4X4RgbFloat block, (int, int, int) unQuantizedEp0, (int, int, int) unQuantizedEp1,
			Span<byte> indices, bool signed)
		{
			const int paletteSize = 1 << 4;
			Span<(int, int, int)> palette = stackalloc (int, int, int)[paletteSize];
			GeneratePaletteInt(palette, unQuantizedEp0, unQuantizedEp1, 4, signed);

			var pixels = block.AsSpan;
			var error = 0f;
			for (var i = 0; i < pixels.Length; i++)
			{
				var intPixel = PreQuantizeRawEndpoint(pixels[i], signed);
				indices[i] = (byte)FindClosestColorIndexInt(intPixel, palette, out var e);
				error += e;
			}
			return MathF.Sqrt(error / (3 * 16));
		}


		public static float FindOptimalIndices1Sub(RawBlock4X4RgbFloat block, (int, int, int) unQuantizedEp0, (int, int, int) unQuantizedEp1,
			Span<byte> indices, bool signed)
		{
			const int paletteSize = 1 << 4;
			Span<ColorRgbFloat> palette = stackalloc ColorRgbFloat[paletteSize];
			GeneratePalette(palette, unQuantizedEp0, unQuantizedEp1, 4, signed);

			var pixels = block.AsSpan;
			var error = 0f;
			for (var i = 0; i < pixels.Length; i++)
			{

				indices[i] = (byte)FindClosestColorIndex(pixels[i], palette, out var e);
				error += e;
			}
			return error;
		}

		public static float FindOptimalIndicesInt2Sub(RawBlock4X4RgbFloat block, (int, int, int) unQuantizedEp0, (int, int, int) unQuantizedEp1,
			Span<byte> indices, int partitionSetId, int subsetIndex, bool signed)
		{
			const int paletteSize = 1 << 3;
			Span<(int, int, int)> palette = stackalloc (int, int, int)[paletteSize];
			GeneratePaletteInt(palette, unQuantizedEp0, unQuantizedEp1, 3, signed);

			var pixels = block.AsSpan;
			var error = 0f;
			for (var i = 0; i < pixels.Length; i++)
			{
				if (Bc6Block.Subsets2PartitionTable[partitionSetId][i] == subsetIndex)
				{
					var intPixel = PreQuantizeRawEndpoint(pixels[i], signed);
					indices[i] = (byte)FindClosestColorIndexInt(intPixel, palette, out var e);
					error += e;
				}
			}
			return error;
		}

		public static float FindOptimalIndices2Sub(RawBlock4X4RgbFloat block, (int, int, int) unQuantizedEp0, (int, int, int) unQuantizedEp1,
			Span<byte> indices, int partitionSetId, int subsetIndex, bool signed)
		{
			const int paletteSize = 1 << 3;
			Span<ColorRgbFloat> palette = stackalloc ColorRgbFloat[paletteSize];
			GeneratePalette(palette, unQuantizedEp0, unQuantizedEp1, 3, signed);

			var pixels = block.AsSpan;
			var error = 0f;
			for (var i = 0; i < pixels.Length; i++)
			{
				if (Bc6Block.Subsets2PartitionTable[partitionSetId][i] == subsetIndex)
				{
					indices[i] = (byte)FindClosestColorIndex(pixels[i], palette, out var e);
					error += e;
				}
			}
			return error;
		}

		public static void SwapIndicesIfNecessary1Sub(RawBlock4X4RgbFloat block, ref (int, int, int) unQuantizedEp0, ref (int, int, int) unQuantizedEp1,
			Span<byte> indices, bool signed)
		{
			const int msb = 1 << (3);

			if ((indices[0] & msb) == 0)
			{
				return;
			}

			InternalUtils.Swap(ref unQuantizedEp0, ref unQuantizedEp1);
			FindOptimalIndicesInt1Sub(block, unQuantizedEp0, unQuantizedEp1, indices, signed);
		}

		public static void SwapIndicesIfNecessary2Sub(RawBlock4X4RgbFloat block, ref (int, int, int) unQuantizedEp0, ref (int, int, int) unQuantizedEp1,
			Span<byte> indices, int partitionSetId, int subsetIndex, bool signed)
		{
			const int msb = 1 << (2);

			var anchorIndex = subsetIndex == 0 ? 0 : Bc6Block.Subsets2AnchorIndices[partitionSetId];

			if ((indices[anchorIndex] & msb) == 0)
			{
				return;
			}

			InternalUtils.Swap(ref unQuantizedEp0, ref unQuantizedEp1);
			FindOptimalIndicesInt2Sub(block, unQuantizedEp0, unQuantizedEp1, indices, partitionSetId, subsetIndex, signed);
		}

		public static void GetInitialUnscaledEndpointsForSubset(RawBlock4X4RgbFloat block, out ColorRgbFloat ep0,
			out ColorRgbFloat ep1, int partitionSetId, int subsetIndex)
		{

			var originalPixels = block.AsSpan;

			var count = 0;
			for (var i = 0; i < 16; i++)
			{
				if (Bc6Block.Subsets2PartitionTable[partitionSetId][i] == subsetIndex)
				{
					count++;
				}
			}

			Span<ColorRgbFloat> subsetColors = stackalloc ColorRgbFloat[count];
			var next = 0;
			for (var i = 0; i < 16; i++)
			{
				if (Bc6Block.Subsets2PartitionTable[partitionSetId][i] == subsetIndex)
				{
					subsetColors[next++] = originalPixels[i];
				}
			}

			PcaVectors.Create(subsetColors, out var mean, out var pa);
			PcaVectors.GetExtremePoints(subsetColors, mean, pa, out var min, out var max);

			ep0 = new ColorRgbFloat(min);
			ep1 = new ColorRgbFloat(max);
		}

		public static void GetInitialUnscaledEndpoints(RawBlock4X4RgbFloat block, out ColorRgbFloat ep0,
			out ColorRgbFloat ep1)
		{

			var originalPixels = block.AsSpan;

			PcaVectors.Create(originalPixels, out var mean, out var pa);
			PcaVectors.GetExtremePoints(originalPixels, mean, pa, out var min, out var max);

			ep0 = new ColorRgbFloat(min);
			ep1 = new ColorRgbFloat(max);
		}


	}
}
