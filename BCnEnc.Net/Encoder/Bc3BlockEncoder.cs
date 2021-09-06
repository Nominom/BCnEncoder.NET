using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;

namespace BCnEncoder.Encoder
{
	internal class Bc3BlockEncoder : BaseBcBlockEncoder<Bc3Block, RawBlock4X4Rgba32>
	{
		private static readonly Bc4ComponentBlockEncoder bc4BlockEncoder = new Bc4ComponentBlockEncoder(ColorComponent.A);

		public override Bc3Block EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			switch (quality)
			{
				case CompressionQuality.Fast:
					return Bc3BlockEncoderFast.EncodeBlock(block);
				case CompressionQuality.Balanced:
					return Bc3BlockEncoderBalanced.EncodeBlock(block);
				case CompressionQuality.BestQuality:
					return Bc3BlockEncoderSlow.EncodeBlock(block);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		public override GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlCompressedRgbaS3TcDxt5Ext;
		}

		public override GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRgba;
		}

		public override DxgiFormat GetDxgiFormat()
		{
			return DxgiFormat.DxgiFormatBc3Unorm;
		}

		#region Encoding private stuff

		private static Bc3Block TryColors(RawBlock4X4Rgba32 rawBlock, ColorRgb565 color0, ColorRgb565 color1, out float error, float rWeight = 0.3f, float gWeight = 0.6f, float bWeight = 0.1f)
		{
			var output = new Bc3Block();

			var pixels = rawBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			var c0 = color0.ToColorRgb24();
			var c1 = color1.ToColorRgb24();

			ReadOnlySpan<ColorRgb24> colors = stackalloc ColorRgb24[] {
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

		private static class Bc3BlockEncoderFast
		{
			internal static Bc3Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var principalAxis);
				PcaVectors.GetMinMaxColor565(pixels, mean, principalAxis, out var min, out var max);

				var c0 = max;
				var c1 = min;

				if (c0.data <= c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				var output = TryColors(rawBlock, c0, c1, out _);
				output.alphaBlock = bc4BlockEncoder.EncodeBlock(rawBlock, CompressionQuality.Fast);

				return output;
			}
		}

		private static class Bc3BlockEncoderBalanced
		{
			private const int MaxTries = 24 * 2;
			private const float ErrorThreshold = 0.05f;

			internal static Bc3Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var pa);
				PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);

				var c0 = max;
				var c1 = min;

				var best = TryColors(rawBlock, c0, c1, out var bestError);

				for (var i = 0; i < MaxTries; i++)
				{
					var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);

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
				best.alphaBlock = bc4BlockEncoder.EncodeBlock(rawBlock, CompressionQuality.Balanced);
				return best;
			}
		}

		private static class Bc3BlockEncoderSlow
		{
			private const int MaxTries = 9999;
			private const float ErrorThreshold = 0.01f;


			internal static Bc3Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
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

				best.alphaBlock = bc4BlockEncoder.EncodeBlock(rawBlock, CompressionQuality.BestQuality);
				return best;
			}
		}
		#endregion
	}
}
