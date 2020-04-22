using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using BCnEnc.Net.Shared;

namespace BCnEnc.Net.Encoder
{
	internal class Bc2BlockEncoder : IBcBlockEncoder
	{

		public byte[] Encode(RawBlock4X4Rgba32[,] blocks, int blockWidth, int blockHeight, EncodingQuality quality, bool parallel)
		{
			byte[] outputData = new byte[blockWidth * blockHeight * Marshal.SizeOf<Bc2Block>()];
			Memory<RawBlock4X4Rgba32> inputBlocks = new Memory<RawBlock4X4Rgba32>(blocks.Reshape(MatrixOrder.FortranColumnMajor));
			Span<Bc2Block> outputBlocks = MemoryMarshal.Cast<byte, Bc2Block>(outputData);

			if (parallel)
			{
				Parallel.For(0, inputBlocks.Length, i =>
				{
					Span<Bc2Block> outputBlocks = MemoryMarshal.Cast<byte, Bc2Block>(outputData);
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

		private Bc2Block EncodeBlock(RawBlock4X4Rgba32 block, EncodingQuality quality)
		{
			switch (quality)
			{
				case EncodingQuality.Fast:
					return Bc2BlockEncoderFast.EncodeBlock(block);
				case EncodingQuality.Balanced:
					return Bc2BlockEncoderYcbcrBalanced.EncodeBlock(block);
				case EncodingQuality.BestQuality:
					return Bc2BlockEncoderYcbcrSlowBest.EncodeBlock(block);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		public GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT;
		}

		public GLFormat GetBaseInternalFormat()
		{
			return GLFormat.GL_RGBA;
		}

		public DXGI_FORMAT GetDxgiFormat() {
			return DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM;
		}

		#region Encoding private stuff

		private static Bc2Block TryColors(RawBlock4X4Rgba32 rawBlock, ColorRgb565 color0, ColorRgb565 color1)
		{
			Bc2Block output = new Bc2Block();

			var pixels = rawBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			var c0 = color0.ToColorRgb24();
			var c1 = color1.ToColorRgb24();

			Span<ColorYCbCr> colors = stackalloc ColorYCbCr[] {
				new ColorYCbCr(c0),
				new ColorYCbCr(c1),
				new ColorYCbCr(c0 * (2.0 / 3.0) + c1 * (1.0 / 3.0)),
				new ColorYCbCr(c0 * (1.0 / 3.0) + c1 * (2.0 / 3.0))
			};

			for (int i = 0; i < 16; i++)
			{
				var color = pixels[i];
				output[i] = ColorChooser.ChooseClosestColor(colors, color);
				output.SetAlpha(i, rawBlock[i].A);
			}

			return output;
		}


		#endregion

		#region Encoders

		private static class Bc2BlockEncoderFast
		{

			internal static Bc2Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
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
				var output = TryColors(rawBlock, c0, c1);

				return output;
			}
		}
		private static class Bc2BlockEncoderYcbcrSlowBest
		{
			private const int variations = 2;

			internal static Bc2Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var rawBlockYcbcr = rawBlock.ToRawBlockYcbcr();
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var principalAxis);
				PcaVectors.GetExtremePoints(pixels, mean, principalAxis, out var min, out var max);

				var minYcbcr = new ColorYCbCr(min);
				var maxYcbcr = new ColorYCbCr(max);
				List<ColorRgb565> uniqueColors =
					ColorVariationGenerator.GenerateVariationsSidewaysMinMax(variations, minYcbcr, maxYcbcr);

				Bc2Block best = new Bc2Block();
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

						var encoded = TryColors(rawBlock, color0, color1);
						var decoded = encoded.Decode();
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
		private static class Bc2BlockEncoderYcbcrBalanced
		{
			private const int variations = 2;

			internal static Bc2Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var rawBlockYcbcr = rawBlock.ToRawBlockYcbcr();
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var principalAxis);
				PcaVectors.GetExtremePoints(pixels, mean, principalAxis, out var min, out var max);

				var minYcbcr = new ColorYCbCr(min);
				var maxYcbcr = new ColorYCbCr(max);
				List<ColorRgb565> uniqueColors =
					ColorVariationGenerator.GenerateVariationsSidewaysMax(variations, minYcbcr, maxYcbcr);

				Bc2Block best = new Bc2Block();
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

						var encoded = TryColors(rawBlock, color0, color1);
						var decoded = encoded.Decode();
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
}
