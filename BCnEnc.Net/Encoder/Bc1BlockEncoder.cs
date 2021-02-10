using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;

namespace BCnEncoder.Encoder
{
	internal class Bc1BlockEncoder : BaseBcBlockEncoder<Bc1Block>
	{
		public override Bc1Block EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			switch (quality)
			{
				case CompressionQuality.Fast:
					return Bc1BlockEncoderFast.EncodeBlock(block);
				case CompressionQuality.Balanced:
					return Bc1BlockEncoderBalanced.EncodeBlock(block);
				case CompressionQuality.BestQuality:
					return Bc1BlockEncoderSlow.EncodeBlock(block);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		public override GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlCompressedRgbS3TcDxt1Ext;
		}

		public override GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRgb;
		}

		public override DxgiFormat GetDxgiFormat()
		{
			return DxgiFormat.DxgiFormatBc1Unorm;
		}

		#region Encoding private stuff

		private static Bc1Block TryColors(RawBlock4X4Rgba32 rawBlock, ColorRgb565 color0, ColorRgb565 color1, out float error, float rWeight = 0.3f, float gWeight = 0.6f, float bWeight = 0.1f)
		{
			var output = new Bc1Block();

			var pixels = rawBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			var c0 = color0.ToColorRgb24();
			var c1 = color1.ToColorRgb24();

			ReadOnlySpan<ColorRgb24> colors = output.HasAlphaOrBlack ?
				stackalloc ColorRgb24[] {
				c0,
				c1,
				c0.InterpolateHalf(c1),
				new ColorRgb24(0, 0, 0)
			} : stackalloc ColorRgb24[] {
				c0,
				c1,
				c0.InterpolateThird(c1, 1),
				c0.InterpolateThird(c1, 2)
			};

			error = 0;
			for (var i = 0; i < 16; i++)
			{
				var color = pixels[i];
				output[i] = ColorChooser.ChooseClosestColor4(colors, color, rWeight, gWeight, bWeight, out var e);
				error += e;
			}

			return output;
		}


		#endregion

		#region Encoders

		private static class Bc1BlockEncoderFast
		{

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var output = new Bc1Block();

				var pixels = rawBlock.AsSpan;

				RgbBoundingBox.Create565(pixels, out var min, out var max);

				var c0 = max;
				var c1 = min;

				output = TryColors(rawBlock, c0, c1, out var error);

				return output;
			}
		}

		private static class Bc1BlockEncoderBalanced
		{
			private const int MaxTries = 24 * 2;
			private const float ErrorThreshold = 0.05f;

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var pa);
				PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);

				var c0 = max;
				var c1 = min;

				if (c0.data < c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				var best = TryColors(rawBlock, c0, c1, out var bestError);

				for (var i = 0; i < MaxTries; i++)
				{
					var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);

					if (newC0.data < newC1.data)
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

		private static class Bc1BlockEncoderSlow
		{
			private const int MaxTries = 9999;
			private const float ErrorThreshold = 0.01f;

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var pa);
				PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);

				var c0 = max;
				var c1 = min;

				if (c0.data < c1.data)
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

					if (newC0.data < newC1.data)
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

	internal class Bc1AlphaBlockEncoder : BaseBcBlockEncoder<Bc1Block>
	{
		public override Bc1Block EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			switch (quality)
			{
				case CompressionQuality.Fast:
					return Bc1AlphaBlockEncoderFast.EncodeBlock(block);
				case CompressionQuality.Balanced:
					return Bc1AlphaBlockEncoderBalanced.EncodeBlock(block);
				case CompressionQuality.BestQuality:
					return Bc1AlphaBlockEncoderSlow.EncodeBlock(block);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		public override GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlCompressedRgbaS3TcDxt1Ext;
		}

		public override GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRgba;
		}

		public override DxgiFormat GetDxgiFormat()
		{
			return DxgiFormat.DxgiFormatBc1Unorm;
		}

		#region Encoding private stuff

		private static Bc1Block TryColors(RawBlock4X4Rgba32 rawBlock, ColorRgb565 color0, ColorRgb565 color1, out float error, float rWeight = 0.3f, float gWeight = 0.6f, float bWeight = 0.1f)
		{
			var output = new Bc1Block();

			var pixels = rawBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			var c0 = color0.ToColorRgb24();
			var c1 = color1.ToColorRgb24();

			var hasAlpha = output.HasAlphaOrBlack;

			ReadOnlySpan<ColorRgb24> colors = hasAlpha ?
				stackalloc ColorRgb24[] {
				c0,
				c1,
				c0.InterpolateHalf(c1),
				new ColorRgb24(0, 0, 0)
			} : stackalloc ColorRgb24[] {
				c0,
				c1,
				c0.InterpolateThird(c1, 1),
				c0.InterpolateThird(c1, 2)
			};

			error = 0;
			for (var i = 0; i < 16; i++)
			{
				var color = pixels[i];
				output[i] = ColorChooser.ChooseClosestColor4AlphaCutoff(colors, color, rWeight, gWeight, bWeight,
					128, hasAlpha, out var e);
				error += e;
			}

			return output;
		}

		#endregion

		#region Encoders

		private static class Bc1AlphaBlockEncoderFast
		{

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var output = new Bc1Block();

				var pixels = rawBlock.AsSpan;

				var hasAlpha = rawBlock.HasTransparentPixels();

				RgbBoundingBox.Create565AlphaCutoff(pixels, out var min, out var max);

				var c0 = max;
				var c1 = min;

				if (hasAlpha && c0.data > c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				output = TryColors(rawBlock, c0, c1, out var error);

				return output;
			}
		}

		private static class Bc1AlphaBlockEncoderBalanced
		{
			private const int MaxTries = 24 * 2;
			private const float ErrorThreshold = 0.05f;


			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
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

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
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
