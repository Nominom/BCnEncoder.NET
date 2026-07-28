using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Bptc
{

	internal static class Bc7EncodingHelpers
	{
		/// <summary>Luma is weighted more heavily than chroma when scoring endpoint candidates.</summary>
		private const float YWeight = 4;
		private const float AlphaWeight = 2;

		private static readonly int[] varPatternRAlpha = new int[] { 1, -1, 1, 0, 0, -1, 0, 0, 0, 0 };
		private static readonly int[] varPatternRNoAlpha = new int[] { 1, -1, 1, 0, 0, -1, 0, 0 };
			
		private static readonly int[] varPatternGAlpha = new int[] { 1, -1, 0, 1, 0, 0, -1, 0, 0, 0 };
		private static readonly int[] varPatternGNoAlpha = new int[] { 1, -1, 0, 1, 0, 0, -1, 0 };
			
		private static readonly int[] varPatternBAlpha = new int[] { 1, -1, 0, 0, 1, 0, 0, -1, 0, 0 };
		private static readonly int[] varPatternBNoAlpha = new int[] { 1, -1, 0, 0, 1, 0, 0, -1 };
			
		private static readonly int[] varPatternAAlpha = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, -1 };
		private static readonly int[] varPatternANoAlpha = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };

		public static bool TypeHasPBits(Bc7BlockType type) => type switch
		{
			Bc7BlockType.Type0 => true,
			Bc7BlockType.Type1 => true,
			Bc7BlockType.Type3 => true,
			Bc7BlockType.Type6 => true,
			Bc7BlockType.Type7 => true,
			_ => false
		};

		public static bool TypeHasSharedPBits(Bc7BlockType type) => type switch
		{
			Bc7BlockType.Type1 => true,
			_ => false
		};

		/// <summary>
		/// Includes PBit
		/// </summary>
		public static int GetColorComponentPrecisionWithPBit(Bc7BlockType type) => type switch
		{
			Bc7BlockType.Type0 => 5,
			Bc7BlockType.Type1 => 7,
			Bc7BlockType.Type2 => 5,
			Bc7BlockType.Type3 => 8,
			Bc7BlockType.Type4 => 5,
			Bc7BlockType.Type5 => 7,
			Bc7BlockType.Type6 => 8,
			Bc7BlockType.Type7 => 6,
			_ => 0
		};

		/// <summary>
		/// Includes PBit
		/// </summary>
		public static int GetAlphaComponentPrecisionWithPBit(Bc7BlockType type) => type switch
		{

			Bc7BlockType.Type4 => 6,
			Bc7BlockType.Type5 => 8,
			Bc7BlockType.Type6 => 8,
			Bc7BlockType.Type7 => 6,
			_ => 0
		};

		/// <summary>
		/// Does not include pBit
		/// </summary>
		public static int GetColorComponentPrecision(Bc7BlockType type) => type switch
		{
			Bc7BlockType.Type0 => 4,
			Bc7BlockType.Type1 => 6,
			Bc7BlockType.Type2 => 5,
			Bc7BlockType.Type3 => 7,
			Bc7BlockType.Type4 => 5,
			Bc7BlockType.Type5 => 7,
			Bc7BlockType.Type6 => 7,
			Bc7BlockType.Type7 => 5,
			_ => 0
		};

		/// <summary>
		/// Does not include pBit
		/// </summary>
		public static int GetAlphaComponentPrecision(Bc7BlockType type) => type switch
		{

			Bc7BlockType.Type4 => 6,
			Bc7BlockType.Type5 => 8,
			Bc7BlockType.Type6 => 7,
			Bc7BlockType.Type7 => 5,
			_ => 0
		};

		public static int GetColorIndexBitCount(Bc7BlockType type, int type4IdxMode = 0) => type switch
		{
			Bc7BlockType.Type0 => 3,
			Bc7BlockType.Type1 => 3,
			Bc7BlockType.Type2 => 2,
			Bc7BlockType.Type3 => 2,
			Bc7BlockType.Type4 when type4IdxMode == 0 => 2,
			Bc7BlockType.Type4 when type4IdxMode == 1 => 3,
			Bc7BlockType.Type5 => 2,
			Bc7BlockType.Type6 => 4,
			Bc7BlockType.Type7 => 2,
			_ => 0
		};

		public static int GetAlphaIndexBitCount(Bc7BlockType type, int type4IdxMode = 0) => type switch
		{
			Bc7BlockType.Type4 when type4IdxMode == 0 => 3,
			Bc7BlockType.Type4 when type4IdxMode == 1 => 2,
			Bc7BlockType.Type5 => 2,
			Bc7BlockType.Type6 => 4,
			Bc7BlockType.Type7 => 2,
			_ => 0
		};


		public static void ExpandEndpoints(Bc7BlockType type, ColorRgba32[] endpoints, byte[] pBits)
		{
			if (type == Bc7BlockType.Type0 || type == Bc7BlockType.Type1 || type == Bc7BlockType.Type3 || type == Bc7BlockType.Type6 || type == Bc7BlockType.Type7)
			{
				for (var i = 0; i < endpoints.Length; i++)
				{
					endpoints[i] <<= 1;
				}

				if (type == Bc7BlockType.Type1)
				{
					endpoints[0] |= pBits[0];
					endpoints[1] |= pBits[0];
					endpoints[2] |= pBits[1];
					endpoints[3] |= pBits[1];
				}
				else
				{
					for (var i = 0; i < endpoints.Length; i++)
					{
						endpoints[i] |= pBits[i];
					}
				}
			}

			var colorPrecision = GetColorComponentPrecisionWithPBit(type);
			var alphaPrecision = GetAlphaComponentPrecisionWithPBit(type);
			for (var i = 0; i < endpoints.Length; i++)
			{
				// ColorComponentPrecision & AlphaComponentPrecision includes pbit
				// left shift endpoint components so that their MSB lies in bit 7
				endpoints[i].r = (byte)(endpoints[i].r << (8 - colorPrecision));
				endpoints[i].g = (byte)(endpoints[i].g << (8 - colorPrecision));
				endpoints[i].b = (byte)(endpoints[i].b << (8 - colorPrecision));
				endpoints[i].a = (byte)(endpoints[i].a << (8 - alphaPrecision));

				// Replicate each component's MSB into the LSBs revealed by the left-shift operation above
				endpoints[i].r = (byte)(endpoints[i].r | (endpoints[i].r >> colorPrecision));
				endpoints[i].g = (byte)(endpoints[i].g | (endpoints[i].g >> colorPrecision));
				endpoints[i].b = (byte)(endpoints[i].b | (endpoints[i].b >> colorPrecision));
				endpoints[i].a = (byte)(endpoints[i].a | (endpoints[i].a >> alphaPrecision));
			}

			//If this mode does not explicitly define the alpha component
			//set alpha equal to 255
			if (type == Bc7BlockType.Type0 || type == Bc7BlockType.Type1 || type == Bc7BlockType.Type2 || type == Bc7BlockType.Type3)
			{
				for (var i = 0; i < endpoints.Length; i++)
				{
					endpoints[i].a = 255;
				}
			}
		}


		public static ColorRgba32 ExpandEndpoint(Bc7BlockType type, ColorRgba32 endpoint, byte pBit)
		{
			if (type == Bc7BlockType.Type0 || type == Bc7BlockType.Type1 || type == Bc7BlockType.Type3 || type == Bc7BlockType.Type6 || type == Bc7BlockType.Type7)
			{
				endpoint <<= 1;
				endpoint |= pBit;
			}

			var colorPrecision = GetColorComponentPrecisionWithPBit(type);
			var alphaPrecision = GetAlphaComponentPrecisionWithPBit(type);
			endpoint.r = (byte)(endpoint.r << (8 - colorPrecision));
			endpoint.g = (byte)(endpoint.g << (8 - colorPrecision));
			endpoint.b = (byte)(endpoint.b << (8 - colorPrecision));
			endpoint.a = (byte)(endpoint.a << (8 - alphaPrecision));

			// Replicate each component's MSB into the LSBs revealed by the left-shift operation above
			endpoint.r = (byte)(endpoint.r | (endpoint.r >> colorPrecision));
			endpoint.g = (byte)(endpoint.g | (endpoint.g >> colorPrecision));
			endpoint.b = (byte)(endpoint.b | (endpoint.b >> colorPrecision));
			endpoint.a = (byte)(endpoint.a | (endpoint.a >> alphaPrecision));

			//If this mode does not explicitly define the alpha component
			//set alpha equal to 255
			if (type == Bc7BlockType.Type0 || type == Bc7BlockType.Type1 || type == Bc7BlockType.Type2 || type == Bc7BlockType.Type3)
			{
				endpoint.a = 255;
			}

			return endpoint;
		}


		public static void GetInitialUnscaledEndpoints(RawBlock4X4Rgba32 block, out ColorRgba32 ep0,
			out ColorRgba32 ep1)
		{

			var originalPixels = block.AsSpan;
			PcaVectors.CreateWithAlpha(originalPixels, out var mean, out var pa);
			PcaVectors.GetExtremePointsWithAlpha(block.AsSpan, mean, pa, out var min, out var max);

			ep0 = new ColorRgba32((byte)(min.X * 255), (byte)(min.Y * 255), (byte)(min.Z * 255), (byte)(min.W * 255));
			ep1 = new ColorRgba32((byte)(max.X * 255), (byte)(max.Y * 255), (byte)(max.Z * 255), (byte)(max.W * 255));
		}

		public static void GetInitialUnscaledEndpointsForSubset(RawBlock4X4Rgba32 block, out ColorRgba32 ep0,
			out ColorRgba32 ep1, ReadOnlySpan<int> partitionTable, int subsetIndex)
		{

			var originalPixels = block.AsSpan;

			var count = 0;
			for (var i = 0; i < 16; i++)
			{
				if (partitionTable[i] == subsetIndex)
				{
					count++;
				}
			}

			Span<ColorRgba32> subsetColors = stackalloc ColorRgba32[count];
			var next = 0;
			for (var i = 0; i < 16; i++)
			{
				if (partitionTable[i] == subsetIndex)
				{
					subsetColors[next++] = originalPixels[i];
				}
			}

			PcaVectors.CreateWithAlpha(subsetColors, out var mean, out var pa);
			PcaVectors.GetExtremePointsWithAlpha(block.AsSpan, mean, pa, out var min, out var max);

			ep0 = new ColorRgba32((byte)(min.X * 255), (byte)(min.Y * 255), (byte)(min.Z * 255), (byte)(min.W * 255));
			ep1 = new ColorRgba32((byte)(max.X * 255), (byte)(max.Y * 255), (byte)(max.Z * 255), (byte)(max.W * 255));
		}

		public static ColorRgba32 ScaleDownEndpoint(ColorRgba32 endpoint, Bc7BlockType type, bool ignoreAlpha, out byte pBit)
		{
			var colorPrecision = GetColorComponentPrecisionWithPBit(type);
			var alphaPrecision = GetAlphaComponentPrecisionWithPBit(type);

			var r = (byte)(endpoint.r >> (8 - colorPrecision));
			var g = (byte)(endpoint.g >> (8 - colorPrecision));
			var b = (byte)(endpoint.b >> (8 - colorPrecision));
			var a = (byte)(endpoint.a >> (8 - alphaPrecision));

			if (TypeHasPBits(type))
			{
				var pBitVotingMask = (1 << (8 - colorPrecision + 1)) - 1;
				float pBitVotes = 0;
				pBitVotes += endpoint.r & pBitVotingMask;
				pBitVotes += endpoint.g & pBitVotingMask;
				pBitVotes += endpoint.b & pBitVotingMask;
				pBitVotes /= 3;

				if (pBitVotes >= pBitVotingMask / 2f)
				{
					pBit = 1;
				}
				else
				{
					pBit = 0;
				}

				r >>= 1;
				g >>= 1;
				b >>= 1;
				a >>= 1;
			}
			else
			{
				pBit = 0;
			}

			if (ignoreAlpha)
			{
				return new ColorRgba32(r, g, b, 0);
			}
			else
			{
				return new ColorRgba32(r, g, b, a);
			}
		}

		public static ColorRgba32 InterpolateColor(ColorRgba32 endPointStart, ColorRgba32 endPointEnd,
			int colorIndex, int alphaIndex, int colorBitCount, int alphaBitCount)
		{
			return InterpolateColor(endPointStart, endPointEnd,
				BptcEncodingHelpers.GetInterpolationWeights(colorBitCount)[colorIndex],
				BptcEncodingHelpers.GetInterpolationWeights(alphaBitCount)[alphaIndex]);
		}

		/// <summary>
		/// Interpolates with weights already looked up by the caller, so palette loops select the
		/// weight table once instead of once per component.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ColorRgba32 InterpolateColor(ColorRgba32 endPointStart, ColorRgba32 endPointEnd,
			int colorWeight, int alphaWeight)
		{
			return new ColorRgba32(
				BptcEncodingHelpers.InterpolateByte(endPointStart.r, endPointEnd.r, colorWeight),
				BptcEncodingHelpers.InterpolateByte(endPointStart.g, endPointEnd.g, colorWeight),
				BptcEncodingHelpers.InterpolateByte(endPointStart.b, endPointEnd.b, colorWeight),
				BptcEncodingHelpers.InterpolateByte(endPointStart.a, endPointEnd.a, alphaWeight)
			);
		}

		public static void ClampEndpoint(ref ColorRgba32 endpoint, byte colorMax, byte alphaMax)
		{
			if (endpoint.r > colorMax) endpoint.r = colorMax;
			if (endpoint.g > colorMax) endpoint.g = colorMax;
			if (endpoint.b > colorMax) endpoint.b = colorMax;
			if (endpoint.a > alphaMax) endpoint.a = alphaMax;
		}

		private static int FindClosestColorIndex(ColorYCbCrAlpha color, in YcbcrAlphaPalette palette, out float bestError)
		{
			var y = new Vector4(color.y);
			var cb = new Vector4(color.cb);
			var cr = new Vector4(color.cr);
			var alpha = new Vector4(color.alpha);

			var bestSquared = float.MaxValue;
			var bestIndex = 0;

			for (var g = 0; g < palette.Groups; g++)
			{
				var dy = y - palette.Y[g];
				var dcb = cb - palette.Cb[g];
				var dcr = cr - palette.Cr[g];
				var da = alpha - palette.Alpha[g];

				var d = dy * dy * YWeight + dcb * dcb + dcr * dcr + da * da * AlphaWeight;

				// Lanes are checked in index order so ties keep the lowest index, like a scalar scan.
				var first = g * 4;
				if (d.X < bestSquared) { bestSquared = d.X; bestIndex = first; }
				if (d.Y < bestSquared) { bestSquared = d.Y; bestIndex = first + 1; }
				if (d.Z < bestSquared) { bestSquared = d.Z; bestIndex = first + 2; }
				if (d.W < bestSquared) { bestSquared = d.W; bestIndex = first + 3; }
			}

			bestError = MathF.Sqrt(bestSquared);
			return bestIndex;
		}

		private static int FindClosestColorIndex(ColorYCbCr color, ReadOnlySpan<ColorYCbCr> colors, out float bestError)
		{
			var bestSquared = color.CalcDistWeightedSquared(colors[0], 4);
			var bestIndex = 0;
			for (var i = 1; i < colors.Length; i++)
			{
				var error = color.CalcDistWeightedSquared(colors[i], 4);
				if (error < bestSquared)
				{
					bestIndex = i;
					bestSquared = error;
				}
				if (bestSquared == 0)
				{
					break;
				}
			}
			bestError = MathF.Sqrt(bestSquared);
			return bestIndex;
		}

		private static int FindClosestAlphaIndex(byte alpha, ReadOnlySpan<byte> alphas, out float bestError)
		{
			bestError = (alpha - alphas[0]) * (alpha - alphas[0]);
			var bestIndex = 0;
			for (var i = 1; i < alphas.Length; i++)
			{
				float error = (alpha - alphas[i]) * (alpha - alphas[i]);
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


		/// <summary>
		/// Converts the block once so the endpoint search below does not redo it for every candidate.
		/// The y/cb/cr components match <see cref="ColorYCbCr"/> built from the same pixel.
		/// </summary>
		private static void ConvertPixels(RawBlock4X4Rgba32 raw, Span<ColorYCbCrAlpha> pixelColors)
		{
			var pixels = raw.AsSpan;
			for (var i = 0; i < 16; i++)
			{
				pixelColors[i] = new ColorYCbCrAlpha(pixels[i]);
			}
		}

		/// <summary>
		/// The interpolated endpoint palette in YCbCrAlpha, stored channel-wise in groups of four.
		/// Scoring a pixel against four candidates is then a single set of SIMD operations, which is
		/// what the endpoint search spends nearly all of its time on.
		/// </summary>
		private readonly ref struct YcbcrAlphaPalette
		{
			public readonly Span<Vector4> Y;
			public readonly Span<Vector4> Cb;
			public readonly Span<Vector4> Cr;
			public readonly Span<Vector4> Alpha;

			/// <summary>
			/// Splits <paramref name="storage"/> into the four channels. It needs one <see cref="Vector4"/>
			/// per palette entry, since four entries per group times four channels is four vectors.
			/// </summary>
			public YcbcrAlphaPalette(Span<Vector4> storage)
			{
				var groups = storage.Length / 4;
				Y = storage.Slice(0, groups);
				Cb = storage.Slice(groups, groups);
				Cr = storage.Slice(groups * 2, groups);
				Alpha = storage.Slice(groups * 3, groups);
			}

			public int Groups => Y.Length;
		}

		/// <summary>
		/// Builds the interpolated endpoint palette shared by color and alpha indices.
		/// </summary>
		private static YcbcrAlphaPalette BuildPalette(ColorRgba32 ep0, ColorRgba32 ep1, int colorIndexPrecision, int alphaIndexPrecision,
			Span<Vector4> storage)
		{
			var colorWeights = BptcEncodingHelpers.GetInterpolationWeights(colorIndexPrecision);
			var alphaWeights = BptcEncodingHelpers.GetInterpolationWeights(alphaIndexPrecision);
			var normalized = ColorTables.Normalized;
			var palette = new YcbcrAlphaPalette(storage);

			for (var group = 0; group < palette.Groups; group++)
			{
				var first = group * 4;
				var c0 = InterpolateColor(ep0, ep1, colorWeights[first], alphaWeights[first]);
				var c1 = InterpolateColor(ep0, ep1, colorWeights[first + 1], alphaWeights[first + 1]);
				var c2 = InterpolateColor(ep0, ep1, colorWeights[first + 2], alphaWeights[first + 2]);
				var c3 = InterpolateColor(ep0, ep1, colorWeights[first + 3], alphaWeights[first + 3]);

				var r = new Vector4(normalized[c0.r], normalized[c1.r], normalized[c2.r], normalized[c3.r]);
				var g = new Vector4(normalized[c0.g], normalized[c1.g], normalized[c2.g], normalized[c3.g]);
				var b = new Vector4(normalized[c0.b], normalized[c1.b], normalized[c2.b], normalized[c3.b]);

				ColorYCbCrAlpha.FromNormalized(r, g, b, out var y, out var cb, out var cr);

				palette.Y[group] = y;
				palette.Cb[group] = cb;
				palette.Cr[group] = cr;
				palette.Alpha[group] = new Vector4(normalized[c0.a], normalized[c1.a], normalized[c2.a], normalized[c3.a]);
			}

			return palette;
		}

		/// <summary>
		/// Builds the palettes for the modes that index color and alpha independently.
		/// </summary>
		private static void BuildSeparatePalettes(ColorRgba32 ep0, ColorRgba32 ep1, int colorIndexPrecision, int alphaIndexPrecision,
			Span<ColorYCbCr> colors, Span<byte> alphas)
		{
			var colorWeights = BptcEncodingHelpers.GetInterpolationWeights(colorIndexPrecision);
			var alphaWeights = BptcEncodingHelpers.GetInterpolationWeights(alphaIndexPrecision);

			for (var i = 0; i < colors.Length; i++)
			{
				colors[i] = new ColorYCbCr(InterpolateColor(ep0, ep1, colorWeights[i], 0));
			}

			for (var i = 0; i < alphas.Length; i++)
			{
				alphas[i] = InterpolateColor(ep0, ep1, 0, alphaWeights[i]).a;
			}
		}

		private static float TrySubsetEndpoints(Bc7BlockType type, RawBlock4X4Rgba32 raw, ReadOnlySpan<ColorYCbCrAlpha> pixelColors,
			ColorRgba32 ep0, ColorRgba32 ep1, ReadOnlySpan<int> partitionTable, int subsetIndex, int type4IdxMode)
		{
			var colorIndexPrecision = GetColorIndexBitCount(type, type4IdxMode);
			var alphaIndexPrecision = GetAlphaIndexBitCount(type, type4IdxMode);

			if (type == Bc7BlockType.Type4 || type == Bc7BlockType.Type5)
			{ //separate indices for color and alpha
				Span<ColorYCbCr> colors = stackalloc ColorYCbCr[1 << colorIndexPrecision];
				Span<byte> alphas = stackalloc byte[1 << alphaIndexPrecision];
				BuildSeparatePalettes(ep0, ep1, colorIndexPrecision, alphaIndexPrecision, colors, alphas);

				var pixels = raw.AsSpan;
				float error = 0;

				for (var i = 0; i < 16; i++)
				{
					var cached = pixelColors[i];
					var pixelColor = new ColorYCbCr(cached.y, cached.cb, cached.cr);

					FindClosestColorIndex(pixelColor, colors, out var ce);
					FindClosestAlphaIndex(pixels[i].a, alphas, out var ae);

					error += ce + ae;
				}

				return error / 16;
			}
			else
			{
				Span<Vector4> paletteStorage = stackalloc Vector4[1 << colorIndexPrecision];
				var palette = BuildPalette(ep0, ep1, colorIndexPrecision, alphaIndexPrecision, paletteStorage);

				float error = 0;
				float count = 0;

				for (var i = 0; i < 16; i++)
				{
					if (partitionTable[i] == subsetIndex)
					{
						FindClosestColorIndex(pixelColors[i], palette, out var e);
						error += e * e;
						count++;
					}
				}

				error /= count;
				return error;
			}

		}

		public static void FillSubsetIndices(Bc7BlockType type, RawBlock4X4Rgba32 raw, ColorRgba32 ep0, ColorRgba32 ep1, ReadOnlySpan<int> partitionTable, int subsetIndex,
			Span<byte> indicesToFill)
		{
			var colorIndexPrecision = GetColorIndexBitCount(type);
			var alphaIndexPrecision = GetAlphaIndexBitCount(type);

			if (type == Bc7BlockType.Type4 || type == Bc7BlockType.Type5)
			{ //separate indices for color and alpha
				throw new ArgumentException();
			}
			else
			{
				Span<Vector4> paletteStorage = stackalloc Vector4[1 << colorIndexPrecision];
				var palette = BuildPalette(ep0, ep1, colorIndexPrecision, alphaIndexPrecision, paletteStorage);

				var pixels = raw.AsSpan;

				for (var i = 0; i < 16; i++)
				{
					if (partitionTable[i] == subsetIndex)
					{
						var pixelColor = new ColorYCbCrAlpha(pixels[i]);

						var index = FindClosestColorIndex(pixelColor, palette, out var e);
						indicesToFill[i] = (byte)index;
					}
				}
			}
		}

		/// <summary>
		/// Used for Modes 4 and 5
		/// </summary>
		public static void FillAlphaColorIndices(Bc7BlockType type, RawBlock4X4Rgba32 raw, ColorRgba32 ep0, ColorRgba32 ep1,
			Span<byte> colorIndicesToFill, Span<byte> alphaIndicesToFill, int idxMode = 0)
		{
			var colorIndexPrecision = GetColorIndexBitCount(type, idxMode);
			var alphaIndexPrecision = GetAlphaIndexBitCount(type, idxMode);

			if (type == Bc7BlockType.Type4 || type == Bc7BlockType.Type5)
			{
				Span<ColorYCbCr> colors = stackalloc ColorYCbCr[1 << colorIndexPrecision];
				Span<byte> alphas = stackalloc byte[1 << alphaIndexPrecision];
				BuildSeparatePalettes(ep0, ep1, colorIndexPrecision, alphaIndexPrecision, colors, alphas);

				var pixels = raw.AsSpan;

				for (var i = 0; i < 16; i++)
				{
					var pixelColor = new ColorYCbCr(pixels[i]);

					var index = FindClosestColorIndex(pixelColor, colors, out _);
					colorIndicesToFill[i] = (byte)index;

					index = FindClosestAlphaIndex(pixels[i].a, alphas, out _);
					alphaIndicesToFill[i] = (byte)index;
				}
			}
			else
			{
				throw new ArgumentException();
			}
		}

		public static void OptimizeSubsetEndpointsWithPBit(Bc7BlockType type, RawBlock4X4Rgba32 raw, ref ColorRgba32 ep0, ref ColorRgba32 ep1, ref byte pBit0, ref byte pBit1,
			int variation, ReadOnlySpan<int> partitionTable, int subsetIndex, bool variatePBits, bool variateAlpha, int type4IdxMode = 0)
		{

			var colorMax = (byte)((1 << GetColorComponentPrecision(type)) - 1);
			var alphaMax = (byte)((1 << GetAlphaComponentPrecision(type)) - 1);

			Span<ColorYCbCrAlpha> pixelColors = stackalloc ColorYCbCrAlpha[16];
			ConvertPixels(raw, pixelColors);

			var bestError = TrySubsetEndpoints(type, raw, pixelColors,
				ExpandEndpoint(type, ep0, pBit0),
				ExpandEndpoint(type, ep1, pBit1), partitionTable, subsetIndex, type4IdxMode
			);

			ReadOnlySpan<int> patternR = variateAlpha
				? varPatternRAlpha
				: varPatternRNoAlpha;
			ReadOnlySpan<int> patternG = variateAlpha
				? varPatternGAlpha
				: varPatternGNoAlpha;
			ReadOnlySpan<int> patternB = variateAlpha
				? varPatternBAlpha
				: varPatternBNoAlpha;
			ReadOnlySpan<int> patternA = variateAlpha
				? varPatternAAlpha
				: varPatternANoAlpha;


			while (variation > 0)
			{
				var foundBetter = false;

				for (var i = 0; i < patternR.Length; i++)
				{
					var testEndPoint0 = new ColorRgba32(
						(byte)(ep0.r - variation * patternR[i]),
						(byte)(ep0.g - variation * patternG[i]),
						(byte)(ep0.b - variation * patternB[i]),
						(byte)(ep0.a - variation * patternA[i])
					);

					var testEndPoint1 = new ColorRgba32(
						(byte)(ep1.r + variation * patternR[i]),
						(byte)(ep1.g + variation * patternG[i]),
						(byte)(ep1.b + variation * patternB[i]),
						(byte)(ep1.a + variation * patternA[i])
					);
					ClampEndpoint(ref testEndPoint0, colorMax, alphaMax);
					ClampEndpoint(ref testEndPoint1, colorMax, alphaMax);

					var error = TrySubsetEndpoints(type, raw, pixelColors,
						ExpandEndpoint(type, testEndPoint0, pBit0),
						ExpandEndpoint(type, testEndPoint1, pBit1), partitionTable, subsetIndex, type4IdxMode
					);
					if (error < bestError)
					{
						bestError = error;
						ep0 = testEndPoint0;
						ep1 = testEndPoint1;
						foundBetter = true;
					}
				}

				for (var i = 0; i < patternR.Length; i++)
				{
					var testEndPoint0 = new ColorRgba32(
						(byte)(ep0.r + variation * patternR[i]),
						(byte)(ep0.g + variation * patternG[i]),
						(byte)(ep0.b + variation * patternB[i]),
						(byte)(ep0.a + variation * patternA[i])
						);
					ClampEndpoint(ref testEndPoint0, colorMax, alphaMax);

					var error = TrySubsetEndpoints(type, raw, pixelColors,
						ExpandEndpoint(type, testEndPoint0, pBit0),
						ExpandEndpoint(type, ep1, pBit1), partitionTable, subsetIndex, type4IdxMode
					);
					if (error < bestError)
					{
						bestError = error;
						ep0 = testEndPoint0;
						foundBetter = true;
					}
				}

				for (var i = 0; i < patternR.Length; i++)
				{
					var testEndPoint1 = new ColorRgba32(
						(byte)(ep1.r + variation * patternR[i]),
						(byte)(ep1.g + variation * patternG[i]),
						(byte)(ep1.b + variation * patternB[i]),
						(byte)(ep1.a + variation * patternA[i])
					);
					ClampEndpoint(ref testEndPoint1, colorMax, alphaMax);

					var error = TrySubsetEndpoints(type, raw, pixelColors,
						ExpandEndpoint(type, ep0, pBit0),
						ExpandEndpoint(type, testEndPoint1, pBit1), partitionTable, subsetIndex, type4IdxMode
					);
					if (error < bestError)
					{
						bestError = error;
						ep1 = testEndPoint1;
						foundBetter = true;
					}
				}

				if (variatePBits)
				{
					{
						var testPBit0 = pBit0 == 0 ? (byte)1 : (byte)0;
						var error = TrySubsetEndpoints(type, raw, pixelColors,
							ExpandEndpoint(type, ep0, testPBit0),
							ExpandEndpoint(type, ep1, pBit1), partitionTable, subsetIndex, type4IdxMode
						);
						if (error < bestError)
						{
							bestError = error;
							pBit0 = testPBit0;
							foundBetter = true;
						}
					}
					{
						var testPBit1 = pBit1 == 0 ? (byte)1 : (byte)0;
						var error = TrySubsetEndpoints(type, raw, pixelColors,
							ExpandEndpoint(type, ep0, pBit0),
							ExpandEndpoint(type, ep1, testPBit1), partitionTable, subsetIndex, type4IdxMode
						);
						if (error < bestError)
						{
							bestError = error;
							pBit1 = testPBit1;
							foundBetter = true;
						}
					}
				}
				if (!foundBetter)
				{
					variation--;
				}
			}
		}

		public static RawBlock4X4Rgba32 RotateBlockColors(RawBlock4X4Rgba32 block, int rotation)
		{
			if (rotation == 0)
			{
				return block;
			}

			var rotated = new RawBlock4X4Rgba32();
			var pixels = block.AsSpan;
			var output = rotated.AsSpan;
			for (var i = 0; i < 16; i++)
			{
				var c = pixels[i];
				switch (rotation)
				{
					case 1:
						output[i] = new ColorRgba32(c.a, c.g, c.b, c.r);
						break;
					case 2:
						output[i] = new ColorRgba32(c.r, c.a, c.b, c.g);
						break;
					case 3:
						output[i] = new ColorRgba32(c.r, c.g, c.a, c.b);
						break;
				}
			}

			return rotated;
		}

	}
}
