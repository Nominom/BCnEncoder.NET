using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal class Bc1BlockEncoder : BaseBcBlockEncoder<Bc1Block>
	{
		public override Bc1Block EncodeBlock(RawBlock4X4RgbaFloat block, CompressionQuality quality, ColorConversionMode colorConversionMode)
		{
			// TODO: Do better.
			block.ColorConvert(colorConversionMode);

			switch (quality)
			{
				case CompressionQuality.Fast:
					// return Bc1BlockEncoderFast.EncodeBlock(block);
				case CompressionQuality.Balanced:
					return Bc1BlockEncoderBalanced.EncodeBlock(block, true);
				case CompressionQuality.BestQuality:
					return Bc1BlockEncoderSlow.EncodeBlock(block, true);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		#region Encoding private stuff

		private static Bc1Block TryColors(RawBlock4X4RgbaFloat rawBlock, ColorB5G6R5Packed color0, ColorB5G6R5Packed color1, bool useColorModeSwitch, out float error, float rWeight = 0.3f, float gWeight = 0.6f, float bWeight = 0.1f)
		{
			var output = new Bc1Block();

			var pixels = rawBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			var c0 = color0.ToColorRgbaFloat();
			var c1 = color1.ToColorRgbaFloat();

			ReadOnlySpan<ColorRgbaFloat> colors = output.HasAlphaOrBlack ?
				stackalloc ColorRgbaFloat[] {
				c0,
				c1,
				c0.InterpolateHalf(c1),
				new ColorRgbaFloat(0, 0, 0)
			} : stackalloc ColorRgbaFloat[] {
				c0,
				c1,
				c0.InterpolateThird(c1, 1),
				c0.InterpolateThird(c1, 2)
			};

			error = 0;
			for (var i = 0; i < 16; i++)
			{
				var color = pixels[i];
				output[i] = ColorChooser.ChooseClosestRgbColor4(colors, color, rWeight, gWeight, bWeight, out var e);
				error += e;
			}

			return output;
		}


		#endregion

		#region Encoders

		// private static class Bc1BlockEncoderFast
		// {
		//
		// 	internal static Bc1Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock)
		// 	{
		// 		var output = new Bc1Block();
		//
		// 		var pixels = rawBlock.AsSpan;
		//
		// 		RgbBoundingBox.Create565(pixels, out var min, out var max);
		//
		// 		var c0 = max;
		// 		var c1 = min;
		//
		// 		output = TryColors(rawBlock, c0, c1, out var error);
		//
		// 		return output;
		// 	}
		// }

		internal static class Bc1BlockEncoderBalanced
		{
			private const int MaxTries = 24 * 2;
			private const float ErrorThreshold = 0.05f;

			internal static Bc1Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock, bool useColorModeSwitch)
			{
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var pa);
				PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);

				var c0 = max;
				var c1 = min;

				if (c0.data < c1.data)
				{
					(c0, c1) = (c1, c0);
				}

				var best = TryColors(rawBlock, c0, c1, useColorModeSwitch, out var bestError);

				for (var i = 0; i < MaxTries; i++)
				{
					var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);

					if (newC0.data < newC1.data)
					{
						(newC0, newC1) = (newC1, newC0);
					}

					var block = TryColors(rawBlock, newC0, newC1, useColorModeSwitch, out var error);

					if (error < bestError)
					{
						best = block;
						bestError = error;
						c0 = newC0;
						c1 = newC1;
					}

					if (bestError < ErrorThreshold)
					{
						break;
					}
				}

				return best;
			}
		}

		internal static class Bc1BlockEncoderSlow
		{
			private const int MaxTries = 9999;
			private const float ErrorThreshold = 0.01f;

			internal static Bc1Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock, bool useColorModeSwitch)
			{
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var pa);
				PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);

				var c0 = max;
				var c1 = min;

				if (c0.data < c1.data)
				{
					(c0, c1) = (c1, c0);
				}

				var best = TryColors(rawBlock, c0, c1, useColorModeSwitch, out var bestError);

				var lastChanged = 0;

				for (var i = 0; i < MaxTries; i++)
				{
					var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);

					if (newC0.data < newC1.data)
					{
						(newC0, newC1) = (newC1, newC0);
					}

					var block = TryColors(rawBlock, newC0, newC1, useColorModeSwitch, out var error);

					lastChanged++;

					if (error < bestError)
					{
						best = block;
						bestError = error;
						c0 = newC0;
						c1 = newC1;
						lastChanged = 0;
					}

					if (bestError < ErrorThreshold || lastChanged > ColorVariationGenerator.VarPatternCount)
					{
						break;
					}
				}

				return best;
			}
		}

		#endregion
	}

	internal class Bc1AlphaBlockEncoder : BaseBcBlockEncoder<Bc1Block>
	{
		public override Bc1Block EncodeBlock(RawBlock4X4RgbaFloat block, CompressionQuality quality, ColorConversionMode colorConversionMode)
		{
			// TODO: Do better.
			block.ColorConvert(colorConversionMode);

			switch (quality)
			{
				case CompressionQuality.Fast:
					// return Bc1AlphaBlockEncoderFast.EncodeBlock(block);
				case CompressionQuality.Balanced:
					return Bc1AlphaBlockEncoderBalanced.EncodeBlock(block);
				case CompressionQuality.BestQuality:
					return Bc1AlphaBlockEncoderSlow.EncodeBlock(block);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		#region Encoding private stuff

		private static Bc1Block TryColors(RawBlock4X4RgbaFloat rawBlock, ColorB5G6R5Packed color0, ColorB5G6R5Packed color1, out float error, float rWeight = 0.3f, float gWeight = 0.6f, float bWeight = 0.1f)
		{
			var output = new Bc1Block();

			var pixels = rawBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			var c0 = color0.ToColorRgbaFloat();
			var c1 = color1.ToColorRgbaFloat();

			var hasAlpha = output.HasAlphaOrBlack;

			ReadOnlySpan<ColorRgbaFloat> colors = hasAlpha ?
				stackalloc ColorRgbaFloat[] {
				c0,
				c1,
				c0.InterpolateHalf(c1),
				new ColorRgbaFloat(0, 0, 0)
			} : stackalloc ColorRgbaFloat[] {
				c0,
				c1,
				c0.InterpolateThird(c1, 1),
				c0.InterpolateThird(c1, 2)
			};

			error = 0;
			for (var i = 0; i < 16; i++)
			{
				var color = pixels[i];
				output[i] = ColorChooser.ChooseClosestRgbColor4AlphaCutoff(colors, color, rWeight, gWeight, bWeight,
					.5f, hasAlpha, out var e);
				error += e;
			}

			return output;
		}

		#endregion

		#region Encoders

		// private static class Bc1AlphaBlockEncoderFast
		// {
		//
		// 	internal static Bc1Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock)
		// 	{
		// 		var output = new Bc1Block();
		//
		// 		var pixels = rawBlock.AsSpan;
		//
		// 		var hasAlpha = rawBlock.HasTransparentPixels();
		//
		// 		RgbBoundingBox.Create565AlphaCutoff(pixels, out var min, out var max);
		//
		// 		var c0 = max;
		// 		var c1 = min;
		//
		// 		if (hasAlpha && c0.data > c1.data)
		// 		{
		// 			var c = c0;
		// 			c0 = c1;
		// 			c1 = c;
		// 		}
		//
		// 		output = TryColors(rawBlock, c0, c1, out var error);
		//
		// 		return output;
		// 	}
		// }

		private static class Bc1AlphaBlockEncoderBalanced
		{
			private const int MaxTries = 24 * 2;
			private const float ErrorThreshold = 0.05f;


			internal static Bc1Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				var hasAlpha = rawBlock.HasTransparentPixels();

				PcaVectors.Create(pixels, out var mean, out var pa);
				PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);

				var c0 = max;
				var c1 = min;

				if (!hasAlpha && c0.data < c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}
				else if (hasAlpha && c1.data < c0.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				var best = TryColors(rawBlock, c0, c1, out var bestError);

				for (var i = 0; i < MaxTries; i++)
				{
					var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);

					if (!hasAlpha && newC0.data < newC1.data)
					{
						var c = newC0;
						newC0 = newC1;
						newC1 = c;
					}
					else if (hasAlpha && newC1.data < newC0.data)
					{
						var c = newC0;
						newC0 = newC1;
						newC1 = c;
					}

					var block = TryColors(rawBlock, newC0, newC1, out var error);

					if (error < bestError)
					{
						best = block;
						bestError = error;
						c0 = newC0;
						c1 = newC1;
					}

					if (bestError < ErrorThreshold)
					{
						break;
					}
				}

				return best;
			}
		}

		private static class Bc1AlphaBlockEncoderSlow
		{
			private const int MaxTries = 9999;
			private const float ErrorThreshold = 0.05f;

			internal static Bc1Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				var hasAlpha = rawBlock.HasTransparentPixels();

				PcaVectors.Create(pixels, out var mean, out var pa);
				PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);

				var c0 = max;
				var c1 = min;

				if (!hasAlpha && c0.data < c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}
				else if (hasAlpha && c1.data < c0.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				var best = TryColors(rawBlock, c0, c1, out var bestError);

				var lastChanged = 0;
				for (var i = 0; i < MaxTries; i++)
				{
					var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);

					if (!hasAlpha && newC0.data < newC1.data)
					{
						var c = newC0;
						newC0 = newC1;
						newC1 = c;
					}
					else if (hasAlpha && newC1.data < newC0.data)
					{
						var c = newC0;
						newC0 = newC1;
						newC1 = c;
					}

					var block = TryColors(rawBlock, newC0, newC1, out var error);

					lastChanged++;

					if (error < bestError)
					{
						best = block;
						bestError = error;
						c0 = newC0;
						c1 = newC1;
						lastChanged = 0;
					}

					if (bestError < ErrorThreshold || lastChanged > ColorVariationGenerator.VarPatternCount)
					{
						break;
					}
				}

				return best;
			}
		}

		#endregion

	}
}
