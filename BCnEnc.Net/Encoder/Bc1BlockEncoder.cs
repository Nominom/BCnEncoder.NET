using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Accord.Diagnostics;
using Accord.Math;
using BCnEnc.Net.Shared;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Encoder
{
	internal class Bc1BlockEncoder : IBcBlockEncoder
	{

		public byte[] Encode(RawBlock4X4Rgba32[,] blocks, int blockWidth, int blockHeight, EncodingQuality quality, bool parallel)
		{
			byte[] outputData = new byte[blockWidth * blockHeight * Marshal.SizeOf<Bc1Block>()];
			Memory<RawBlock4X4Rgba32> inputBlocks = new Memory<RawBlock4X4Rgba32>(blocks.Reshape(MatrixOrder.FortranColumnMajor));
			Span<Bc1Block> outputBlocks = MemoryMarshal.Cast<byte, Bc1Block>(outputData);

			if (parallel)
			{
				Parallel.For(0, inputBlocks.Length, i =>
				{
					Span<Bc1Block> outputBlocks = MemoryMarshal.Cast<byte, Bc1Block>(outputData);
					outputBlocks[i] = EncodeBlock(inputBlocks.Span[i], quality);
				});
			}
			else
			{
				for (int i = 0; i < inputBlocks.Length; i++)
				{
					outputBlocks[i] = EncodeBlock(inputBlocks.Span[i], quality);
				}
			}

			return outputData;
		}

		private Bc1Block EncodeBlock(RawBlock4X4Rgba32 block, EncodingQuality quality)
		{
			switch (quality)
			{
				case EncodingQuality.Fast:
					return Bc1BlockEncoderFast.EncodeBlock(block);
				case EncodingQuality.Balanced:
					return Bc1BlockEncoderYcbcrBalanced.EncodeBlock(block);
				case EncodingQuality.BestQuality:
					return Bc1BlockEncoderYcbcrSlowBest.EncodeBlock(block);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		public GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GL_COMPRESSED_RGB_S3TC_DXT1_EXT;
		}

		public GLFormat GetBaseInternalFormat()
		{
			return GLFormat.GL_RGB;
		}

		#region Encoding private stuff

		private static Bc1Block TryColors(RawBlock4X4Ycbcr rawBlock, ColorRgb565 color0, ColorRgb565 color1)
		{
			Bc1Block output = new Bc1Block();

			var pixels = rawBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			var c0 = color0.ToColorRgb24();
			var c1 = color1.ToColorRgb24();

			Span<ColorYCbCr> colors = output.HasAlphaOrBlack ?
				stackalloc ColorYCbCr[] {
				new ColorYCbCr(c0),
				new ColorYCbCr(c1),
				new ColorYCbCr(c0 * (1.0 / 2.0) + c1 * (1.0 / 2.0)),
				new ColorYCbCr(0, 0, 0)
			} : stackalloc ColorYCbCr[] {
				new ColorYCbCr(c0),
				new ColorYCbCr(c1),
				new ColorYCbCr(c0 * (2.0 / 3.0) + c1 * (1.0 / 3.0)),
				new ColorYCbCr(c0 * (1.0 / 3.0) + c1 * (2.0 / 3.0))
			};

			for (int i = 0; i < 16; i++)
			{
				var color = pixels[i];
				output[i] = ColorChooser.ChooseClosestColor(colors, color);
			}

			return output;
		}


		#endregion

		#region Encoders

		private static class Bc1BlockEncoderFast
		{

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				Bc1Block output = new Bc1Block();

				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var principalAxis);
				//PcaVectors.GetExtremePoints(pixels, mean, principalAxis, out var min, out var max);
				PcaVectors.OptimizeEndpoints(pixels, mean, principalAxis, out var min, out var max);

				ColorRgb565 c0 = min;
				ColorRgb565 c1 = max;

				if (c0.data <= c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				output = TryColors(rawBlock.ToRawBlockYcbcr(), c0, c1);

				return output;
			}
		}

		private static class Bc1BlockEncoderYcbcrSlowBest
		{
			private const int variations = 2;

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var rawBlockYcbcr = rawBlock.ToRawBlockYcbcr();
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var principalAxis);
				PcaVectors.GetExtremePoints(pixels, mean, principalAxis, out var min, out var max);

				var minYcbcr = new ColorYCbCr(min);
				var maxYcbcr = new ColorYCbCr(max);
				List<ColorRgb565> uniqueColors =
					ColorVariationGenerator.GenerateVariationsSidewaysMinMax(variations, minYcbcr, maxYcbcr);

				Bc1Block best = new Bc1Block();
				float bestError = 0;
				bool first = true;
				for (int i = 0; i < uniqueColors.Count - 1; i++)
				{
					for (int j = i + 1; j < uniqueColors.Count; j++)
					{
						var color0 = uniqueColors[i];
						var color1 = uniqueColors[j];


						if (color0.data < color1.data)
						{
							var c = color0;
							color0 = color1;
							color1 = c;
						}

						var encoded = TryColors(rawBlockYcbcr, color0, color1);
						var decoded = encoded.Decode(false);
						var error = rawBlockYcbcr.CalculateError(decoded);

						if (first)
						{
							best = encoded;
							bestError = error;
							first = false;
						}
						else if (error < bestError)
						{
							best = encoded;
							bestError = error;
						}
					}
				}

				return best;
			}
		}
		private static class Bc1BlockEncoderYcbcrBalanced
		{
			private const int variations = 2;

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var rawBlockYcbcr = rawBlock.ToRawBlockYcbcr();
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var principalAxis);
				PcaVectors.GetExtremePoints(pixels, mean, principalAxis, out var min, out var max);

				var minYcbcr = new ColorYCbCr(min);
				var maxYcbcr = new ColorYCbCr(max);
				List<ColorRgb565> uniqueColors =
					ColorVariationGenerator.GenerateVariationsSidewaysMax(variations, minYcbcr, maxYcbcr);

				Bc1Block best = new Bc1Block();
				float bestError = 0;
				bool first = true;
				for (int i = 0; i < uniqueColors.Count - 1; i++)
				{
					for (int j = i + 1; j < uniqueColors.Count; j++)
					{
						var color0 = uniqueColors[i];
						var color1 = uniqueColors[j];

						if (color0.data < color1.data)
						{
							var c = color0;
							color0 = color1;
							color1 = c;
						}

						var encoded = TryColors(rawBlockYcbcr, color0, color1);
						var decoded = encoded.Decode(false);
						var error = rawBlockYcbcr.CalculateError(decoded);

						if (first)
						{
							best = encoded;
							bestError = error;
							first = false;
						}
						else if (error < bestError)
						{
							best = encoded;
							bestError = error;
						}
					}
				}

				return best;
			}
		}
		#endregion
	}

	internal class Bc1AlphaBlockEncoder : IBcBlockEncoder
	{

		public byte[] Encode(RawBlock4X4Rgba32[,] blocks, int blockWidth, int blockHeight, EncodingQuality quality,
			bool parallel)
		{
			byte[] outputData = new byte[blockWidth * blockHeight * Marshal.SizeOf<Bc1Block>()];
			Memory<RawBlock4X4Rgba32> inputBlocks =
				new Memory<RawBlock4X4Rgba32>(blocks.Reshape(MatrixOrder.FortranColumnMajor));
			Span<Bc1Block> outputBlocks = MemoryMarshal.Cast<byte, Bc1Block>(outputData);

			if (parallel)
			{
				Parallel.For(0, inputBlocks.Length, i =>
				{
					Span<Bc1Block> outputBlocks = MemoryMarshal.Cast<byte, Bc1Block>(outputData);
					outputBlocks[i] = EncodeBlock(inputBlocks.Span[i], quality);
				});
			}
			else
			{
				for (int i = 0; i < inputBlocks.Length; i++)
				{
					outputBlocks[i] = EncodeBlock(inputBlocks.Span[i], quality);
				}
			}

			return outputData;
		}

		private Bc1Block EncodeBlock(RawBlock4X4Rgba32 block, EncodingQuality quality)
		{
			switch (quality)
			{
				case EncodingQuality.Fast:
					return Bc1AlphaBlockEncoderFast.EncodeBlock(block);
				case EncodingQuality.Balanced:
					return Bc1BlockEncoderYcbcrBalanced.EncodeBlock(block);
				case EncodingQuality.BestQuality:
					return Bc1BlockEncoderYcbcrSlowBest.EncodeBlock(block);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		public GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT;
		}

		public GLFormat GetBaseInternalFormat()
		{
			return GLFormat.GL_RGBA;
		}

		private static Bc1Block TryColors(RawBlock4X4Rgba32 rawBlock, ColorRgb565 color0, ColorRgb565 color1)
		{
			Bc1Block output = new Bc1Block();

			var pixels = rawBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			var c0 = color0.ToColorRgba32();
			var c1 = color1.ToColorRgba32();

			Span<ColorRgba32> colors = output.HasAlphaOrBlack
				? stackalloc ColorRgba32[] {
				c0,
				c1,
				c0 * (1.0 / 2.0) + c1 * (1.0 / 2.0),
				new ColorRgba32(0, 0, 0, 0)
			}
				: stackalloc ColorRgba32[] {
				c0,
				c1,
				c0 * (2.0 / 3.0) + c1 * (1.0 / 3.0),
				c0 * (1.0 / 3.0) + c1 * (2.0 / 3.0)
			};

			for (int i = 0; i < 16; i++)
			{
				var color = pixels[i];
				output[i] = ColorChooser.ChooseClosestColorAlphaCutOff(colors, color);
			}

			return output;
		}

		private static class Bc1AlphaBlockEncoderFast
		{

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{


				var pixels = rawBlock.AsSpan;

				bool hasTransparency = rawBlock.HasTransparentPixels();

				PcaVectors.Create(pixels, out var mean, out var principalAxis);
				PcaVectors.OptimizeEndpoints(pixels, mean, principalAxis, out var min, out var max);

				ColorRgb565 c0 = min;
				ColorRgb565 c1 = max;

				if (!hasTransparency && c0.data < c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}
				else if (hasTransparency && c0.data > c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				Bc1Block output = TryColors(rawBlock, c0, c1);

				return output;
			}
		}

		private static class Bc1BlockEncoderYcbcrSlowBest
		{
			private const int variations = 2;

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				bool hasTransparency = rawBlock.HasTransparentPixels();

				PcaVectors.Create(pixels, out var mean, out var principalAxis);
				PcaVectors.GetExtremePoints(pixels, mean, principalAxis, out var min, out var max);

				var minYcbcr = new ColorYCbCr(min);
				var maxYcbcr = new ColorYCbCr(max);
				List<ColorRgb565> uniqueColors =
					ColorVariationGenerator.GenerateVariationsSidewaysMinMax(variations, minYcbcr, maxYcbcr);


				Bc1Block best = new Bc1Block();
				float bestError = 0;
				bool first = true;
				for (int i = 0; i < uniqueColors.Count - 1; i++)
				{
					for (int j = i + 1; j < uniqueColors.Count; j++)
					{
						var c0 = uniqueColors[i];
						var c1 = uniqueColors[j];


						if (!hasTransparency && c0.data < c1.data)
						{
							var c = c0;
							c0 = c1;
							c1 = c;
						}
						else if (hasTransparency && c0.data > c1.data)
						{
							var c = c0;
							c0 = c1;
							c1 = c;
						}

						var encoded = TryColors(rawBlock, c0, c1);
						var decoded = encoded.Decode(true);
						var error = rawBlock.CalculateError(decoded, true);

						if (first)
						{
							best = encoded;
							bestError = error;
							first = false;
						}
						else if (error < bestError)
						{
							best = encoded;
							bestError = error;
						}
					}
				}

				return best;
			}
		}
		private static class Bc1BlockEncoderYcbcrBalanced
		{
			private const int variations = 2;


			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				bool hasTransparency = rawBlock.HasTransparentPixels();

				PcaVectors.Create(pixels, out var mean, out var principalAxis);
				PcaVectors.GetExtremePoints(pixels, mean, principalAxis, out var min, out var max);

				var minYcbcr = new ColorYCbCr(min);
				var maxYcbcr = new ColorYCbCr(max);
				List<ColorRgb565> uniqueColors =
					ColorVariationGenerator.GenerateVariationsSidewaysMax(variations, minYcbcr, maxYcbcr);

				Bc1Block best = new Bc1Block();
				float bestError = 0;
				bool first = true;
				for (int i = 0; i < uniqueColors.Count - 1; i++)
				{
					for (int j = i + 1; j < uniqueColors.Count; j++)
					{
						var c0 = uniqueColors[i];
						var c1 = uniqueColors[j];

						if (!hasTransparency && c0.data < c1.data)
						{
							var c = c0;
							c0 = c1;
							c1 = c;
						}
						else if (hasTransparency && c0.data > c1.data)
						{
							var c = c0;
							c0 = c1;
							c1 = c;
						}

						var encoded = TryColors(rawBlock, c0, c1);
						var decoded = encoded.Decode(true);
						var error = rawBlock.CalculateError(decoded, true);

						if (first)
						{
							best = encoded;
							bestError = error;
							first = false;
						}
						else if (error < bestError)
						{
							best = encoded;
							bestError = error;
						}
					}
				}

				return best;
			}
		}
	}
}
