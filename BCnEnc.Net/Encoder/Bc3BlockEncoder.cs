using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using BCnEnc.Net.Shared;

namespace BCnEnc.Net.Encoder
{
	internal class Bc3BlockEncoder : IBcBlockEncoder
	{

		public byte[] Encode(RawBlock4X4Rgba32[,] blocks, int blockWidth, int blockHeight, EncodingQuality quality, bool parallel)
		{
			byte[] outputData = new byte[blockWidth * blockHeight * Marshal.SizeOf<Bc3Block>()];
			Memory<RawBlock4X4Rgba32> inputBlocks = new Memory<RawBlock4X4Rgba32>(blocks.Reshape(MatrixOrder.FortranColumnMajor));
			Span<Bc3Block> outputBlocks = MemoryMarshal.Cast<byte, Bc3Block>(outputData);

			if (parallel)
			{
				Parallel.For(0, inputBlocks.Length, i =>
				{
					Span<Bc3Block> outputBlocks = MemoryMarshal.Cast<byte, Bc3Block>(outputData);
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

		private Bc3Block EncodeBlock(RawBlock4X4Rgba32 block, EncodingQuality quality)
		{
			switch (quality)
			{
				case EncodingQuality.Fast:
					return Bc3BlockEncoderFast.EncodeBlock(block);
				case EncodingQuality.Balanced:
					return Bc3BlockEncoderYcbcrBalanced.EncodeBlock(block);
				case EncodingQuality.BestQuality:
					return Bc3BlockEncoderYcbcrSlowBest.EncodeBlock(block);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		public GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT;
		}

		public GLFormat GetBaseInternalFormat()
		{
			return GLFormat.GL_RGBA;
		}

		public DXGI_FORMAT GetDxgiFormat() {
			return DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM;
		}

		#region Encoding private stuff

		private static Bc3Block TryColors(RawBlock4X4Rgba32 rawBlock, ColorRgb565 color0, ColorRgb565 color1)
		{
			Bc3Block output = new Bc3Block();

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
			}

			return output;
		}

		private static Bc3Block FindAlphaValues(Bc3Block colorBlock, RawBlock4X4Rgba32 rawBlock, int variations) {
			var pixels = rawBlock.AsSpan;

			//Find min and max alpha
			byte minAlpha = 255;
			byte maxAlpha = 0;
			bool hasExtremeValues = false;
			for (int i = 0; i < pixels.Length; i++) {
				if (pixels[i].A < 255 && pixels[i].A > 0) {
					if (pixels[i].A < minAlpha) minAlpha = pixels[i].A;
					if (pixels[i].A > maxAlpha) maxAlpha = pixels[i].A;
				}
				else {
					hasExtremeValues = true;
				}
			}


			int SelectAlphaIndices(ref Bc3Block block) {
				int cumulativeError = 0;
				var a0 = block.Alpha0;
				var a1 = block.Alpha1;
				Span<byte> alphas = a0 > a1 ? stackalloc byte[] {
					a0,
					a1,
					(byte) (6/7.0 * a0 + 1/7.0 * a1),
					(byte) (5/7.0 * a0 + 2/7.0 * a1),
					(byte) (4/7.0 * a0 + 3/7.0 * a1),
					(byte) (3/7.0 * a0 + 4/7.0 * a1),
					(byte) (2/7.0 * a0 + 5/7.0 * a1),
					(byte) (1/7.0 * a0 + 6/7.0 * a1),
				} : stackalloc byte[] {
					a0,
					a1,
					(byte) (4/5.0 * a0 + 1/5.0 * a1),
					(byte) (3/5.0 * a0 + 2/5.0 * a1),
					(byte) (2/5.0 * a0 + 3/5.0 * a1),
					(byte) (1/5.0 * a0 + 4/5.0 * a1),
					0,
					255
				};
				var pixels = rawBlock.AsSpan;
				for (int i = 0; i < pixels.Length; i++) {
					byte bestIndex = 0;
					int bestError = Math.Abs(pixels[i].A - alphas[0]);
					for (byte j = 1; j < alphas.Length; j++) {
						int error = Math.Abs(pixels[i].A - alphas[j]);
						if (error < bestError) {
							bestIndex = j;
							bestError = error;
						}
						if (bestError == 0) break;
					}
					block.SetAlphaIndex(i, bestIndex);
					cumulativeError += bestError * bestError;
				}

				return cumulativeError;
			}

			//everything is either fully opaque or fully transparent
			if (hasExtremeValues && minAlpha == 255 && maxAlpha == 0) {
				colorBlock.Alpha0 = 0;
				colorBlock.Alpha1 = 255;
				var error = SelectAlphaIndices(ref colorBlock);
				Debug.Assert(0 == error);
				return colorBlock;
			}

			var best = colorBlock;
			best.Alpha0 = maxAlpha;
			best.Alpha1 = minAlpha;
			int bestError = SelectAlphaIndices(ref best);
			if (bestError == 0) {
				return best;
			}
			for (byte i = 1; i < variations; i++) {
				{
					byte a0 = ByteHelper.ClampToByte(maxAlpha - i * 2);
					byte a1 = ByteHelper.ClampToByte(minAlpha + i * 2);
					var block = colorBlock;
					block.Alpha0 = hasExtremeValues ? a1 : a0;
					block.Alpha1 = hasExtremeValues ? a0 : a1;
					int error = SelectAlphaIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
					}
				}
				{
					byte a0 = ByteHelper.ClampToByte(maxAlpha + i * 2);
					byte a1 = ByteHelper.ClampToByte(minAlpha - i * 2);
					var block = colorBlock;
					block.Alpha0 = hasExtremeValues ? a1 : a0;
					block.Alpha1 = hasExtremeValues ? a0 : a1;
					int error = SelectAlphaIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
					}
				}
				{
					byte a0 = ByteHelper.ClampToByte(maxAlpha);
					byte a1 = ByteHelper.ClampToByte(minAlpha - i * 2);
					var block = colorBlock;
					block.Alpha0 = hasExtremeValues ? a1 : a0;
					block.Alpha1 = hasExtremeValues ? a0 : a1;
					int error = SelectAlphaIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
					}
				}
				{
					byte a0 = ByteHelper.ClampToByte(maxAlpha + i * 2);
					byte a1 = ByteHelper.ClampToByte(minAlpha);
					var block = colorBlock;
					block.Alpha0 = hasExtremeValues ? a1 : a0;
					block.Alpha1 = hasExtremeValues ? a0 : a1;
					int error = SelectAlphaIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
					}
				}
				{
					byte a0 = ByteHelper.ClampToByte(maxAlpha);
					byte a1 = ByteHelper.ClampToByte(minAlpha + i * 2);
					var block = colorBlock;
					block.Alpha0 = hasExtremeValues ? a1 : a0;
					block.Alpha1 = hasExtremeValues ? a0 : a1;
					int error = SelectAlphaIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
					}
				}
				{
					byte a0 = ByteHelper.ClampToByte(maxAlpha - i * 2);
					byte a1 = ByteHelper.ClampToByte(minAlpha);
					var block = colorBlock;
					block.Alpha0 = hasExtremeValues ? a1 : a0;
					block.Alpha1 = hasExtremeValues ? a0 : a1;
					int error = SelectAlphaIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
					}
				}

				if (bestError < 10) {
					break;
				}
			}
			
			return best;
		}


		#endregion

		#region Encoders

		private static class Bc3BlockEncoderFast
		{

			internal static Bc3Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
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
				output = FindAlphaValues(output, rawBlock, 3);

				return output;
			}
		}
		private static class Bc3BlockEncoderYcbcrSlowBest
		{
			private const int variations = 2;

			internal static Bc3Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var rawBlockYcbcr = rawBlock.ToRawBlockYcbcr();
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var principalAxis);
				PcaVectors.GetExtremePoints(pixels, mean, principalAxis, out var min, out var max);

				var minYcbcr = new ColorYCbCr(min);
				var maxYcbcr = new ColorYCbCr(max);
				List<ColorRgb565> uniqueColors =
					ColorVariationGenerator.GenerateVariationsSidewaysMinMax(variations, minYcbcr, maxYcbcr);

				Bc3Block best = new Bc3Block();
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

				best = FindAlphaValues(best, rawBlock, 4);

				return best;
			}
		}
		private static class Bc3BlockEncoderYcbcrBalanced
		{
			private const int variations = 2;

			internal static Bc3Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var rawBlockYcbcr = rawBlock.ToRawBlockYcbcr();
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out var mean, out var principalAxis);
				PcaVectors.GetExtremePoints(pixels, mean, principalAxis, out var min, out var max);

				var minYcbcr = new ColorYCbCr(min);
				var maxYcbcr = new ColorYCbCr(max);
				List<ColorRgb565> uniqueColors =
					ColorVariationGenerator.GenerateVariationsSidewaysMax(variations, minYcbcr, maxYcbcr);

				Bc3Block best = new Bc3Block();
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
				
				best = FindAlphaValues(best, rawBlock, 6);

				return best;
			}
		}
		#endregion
	}
}
