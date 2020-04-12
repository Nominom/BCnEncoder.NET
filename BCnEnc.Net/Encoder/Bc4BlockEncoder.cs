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
	internal class Bc4BlockEncoder : IBcBlockEncoder {

		private readonly bool luminanceAsRed;
		public Bc4BlockEncoder(bool luminanceAsRed) {
			this.luminanceAsRed = luminanceAsRed;
		}

		public byte[] Encode(RawBlock4X4Rgba32[,] blocks, int blockWidth, int blockHeight, EncodingQuality quality,
			bool parallel) {
			byte[] outputData = new byte[blockWidth * blockHeight * Marshal.SizeOf<Bc4Block>()];
			Memory<RawBlock4X4Rgba32> inputBlocks =
				new Memory<RawBlock4X4Rgba32>(blocks.Reshape(MatrixOrder.FortranColumnMajor));
			Span<Bc4Block> outputBlocks = MemoryMarshal.Cast<byte, Bc4Block>(outputData);

			if (parallel) {
				Parallel.For(0, inputBlocks.Length, i => {
					Span<Bc4Block> outputBlocks = MemoryMarshal.Cast<byte, Bc4Block>(outputData);
					outputBlocks[i] = EncodeBlock(inputBlocks.Span[i], quality);
				});
			}
			else {
				for (int i = 0; i < inputBlocks.Length; i++) {
					outputBlocks[i] = EncodeBlock(inputBlocks.Span[i], quality);
				}
			}

			return outputData;
		}

		private Bc4Block EncodeBlock(RawBlock4X4Rgba32 block, EncodingQuality quality) {
			Bc4Block output = new Bc4Block();
			byte[] colors = new byte[16];
			var pixels = block.AsSpan;
			for (int i = 0; i < 16; i++) {
				if (luminanceAsRed) {
					colors[i] = (byte)(new ColorYCbCr(pixels[i]).y * 255);
				}
				else {
					colors[i] = pixels[i].R;
				}
			}
 			switch (quality) {
				case EncodingQuality.Fast:
					return FindRedValues(output, colors, 3);
				case EncodingQuality.Balanced:
					return FindRedValues(output, colors, 4);
				case EncodingQuality.BestQuality:
					return FindRedValues(output, colors, 8);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		public GlInternalFormat GetInternalFormat() {
			return GlInternalFormat.GL_COMPRESSED_RED_RGTC1_EXT;
		}

		public GLFormat GetBaseInternalFormat() {
			return GLFormat.GL_RED;
		}

		#region Encoding private stuff

		private static Bc4Block FindRedValues(Bc4Block colorBlock, byte[] pixels, int variations) {

			//Find min and max alpha
			byte min = 255;
			byte max = 0;
			bool hasExtremeValues = false;
			for (int i = 0; i < pixels.Length; i++) {
				if (pixels[i] < 255 && pixels[i] > 0) {
					if (pixels[i] < min) min = pixels[i];
					if (pixels[i] > max) max = pixels[i];
				}
				else {
					hasExtremeValues = true;
				}
			}


			int SelectIndices(ref Bc4Block block) {
				int cumulativeError = 0;
				var c0 = block.Red0;
				var c1 = block.Red1;
				Span<byte> colors = c0 > c1
					? stackalloc byte[] {
						c0,
						c1,
						(byte) (6 / 7.0 * c0 + 1 / 7.0 * c1),
						(byte) (5 / 7.0 * c0 + 2 / 7.0 * c1),
						(byte) (4 / 7.0 * c0 + 3 / 7.0 * c1),
						(byte) (3 / 7.0 * c0 + 4 / 7.0 * c1),
						(byte) (2 / 7.0 * c0 + 5 / 7.0 * c1),
						(byte) (1 / 7.0 * c0 + 6 / 7.0 * c1),
					}
					: stackalloc byte[] {
						c0,
						c1,
						(byte) (4 / 5.0 * c0 + 1 / 5.0 * c1),
						(byte) (3 / 5.0 * c0 + 2 / 5.0 * c1),
						(byte) (2 / 5.0 * c0 + 3 / 5.0 * c1),
						(byte) (1 / 5.0 * c0 + 4 / 5.0 * c1),
						0,
						255
					};
				for (int i = 0; i < pixels.Length; i++) {
					byte bestIndex = 0;
					int bestError = Math.Abs(pixels[i] - colors[0]);
					for (byte j = 1; j < colors.Length; j++) {
						int error = Math.Abs(pixels[i] - colors[j]);
						if (error < bestError) {
							bestIndex = j;
							bestError = error;
						}

						if (bestError == 0) break;
					}

					block.SetRedIndex(i, bestIndex);
					cumulativeError += bestError * bestError;
				}

				return cumulativeError;
			}

			//everything is either fully black or fully red
			if (hasExtremeValues && min == 255 && max == 0) {
				colorBlock.Red0 = 0;
				colorBlock.Red1 = 255;
				Debug.Assert(0 == SelectIndices(ref colorBlock));
				return colorBlock;
			}

			var best = colorBlock;
			best.Red0 = max;
			best.Red1 = min;
			int bestError = SelectIndices(ref best);
			if (bestError == 0) {
				return best;
			}

			for (byte i = (byte)variations; i > 0; i--) {
				{
					byte c0 = ByteHelper.ClampToByte(max - i);
					byte c1 = ByteHelper.ClampToByte(min + i);
					var block = colorBlock;
					block.Red0 = hasExtremeValues ? c1 : c0;
					block.Red1 = hasExtremeValues ? c0 : c1;
					int error = SelectIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}
				{
					byte c0 = ByteHelper.ClampToByte(max + i);
					byte c1 = ByteHelper.ClampToByte(min - i);
					var block = colorBlock;
					block.Red0 = hasExtremeValues ? c1 : c0;
					block.Red1 = hasExtremeValues ? c0 : c1;
					int error = SelectIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}
				{
					byte c0 = ByteHelper.ClampToByte(max);
					byte c1 = ByteHelper.ClampToByte(min - i);
					var block = colorBlock;
					block.Red0 = hasExtremeValues ? c1 : c0;
					block.Red1 = hasExtremeValues ? c0 : c1;
					int error = SelectIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}
				{
					byte c0 = ByteHelper.ClampToByte(max + i);
					byte c1 = ByteHelper.ClampToByte(min);
					var block = colorBlock;
					block.Red0 = hasExtremeValues ? c1 : c0;
					block.Red1 = hasExtremeValues ? c0 : c1;
					int error = SelectIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}
				{
					byte c0 = ByteHelper.ClampToByte(max);
					byte c1 = ByteHelper.ClampToByte(min + i);
					var block = colorBlock;
					block.Red0 = hasExtremeValues ? c1 : c0;
					block.Red1 = hasExtremeValues ? c0 : c1;
					int error = SelectIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}
				{
					byte c0 = ByteHelper.ClampToByte(max - i);
					byte c1 = ByteHelper.ClampToByte(min);
					var block = colorBlock;
					block.Red0 = hasExtremeValues ? c1 : c0;
					block.Red1 = hasExtremeValues ? c0 : c1;
					int error = SelectIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}

				if (bestError < 5) {
					break;
				}
			}

			return best;
		}


		#endregion
	}
}
