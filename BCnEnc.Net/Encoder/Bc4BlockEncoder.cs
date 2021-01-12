using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal class Bc4BlockEncoder : BaseBcBlockEncoder<Bc4Block>
	{
		private readonly bool luminanceAsRed;
		
		public Bc4BlockEncoder(bool luminanceAsRed) {
			this.luminanceAsRed = luminanceAsRed;
		}
		
		protected override Bc4Block EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality) {
			var output = new Bc4Block();
			var colors = new byte[16];
			var pixels = block.AsSpan;
			for (var i = 0; i < 16; i++) {
				if (luminanceAsRed) {
					colors[i] = (byte)(new ColorYCbCr(pixels[i]).y * 255);
				}
				else {
					colors[i] = pixels[i].R;
				}
			}
 			switch (quality) {
				case CompressionQuality.Fast:
					return FindRedValues(output, colors, 3);
				case CompressionQuality.Balanced:
					return FindRedValues(output, colors, 4);
				case CompressionQuality.BestQuality:
					return FindRedValues(output, colors, 8);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		public override GlInternalFormat GetInternalFormat() {
			return GlInternalFormat.GlCompressedRedRgtc1Ext;
		}

		public override GlFormat GetBaseInternalFormat() {
			return GlFormat.GlRed;
		}

		public override DxgiFormat GetDxgiFormat() {
			return DxgiFormat.DxgiFormatBc4Unorm;
		}

		#region Encoding private stuff

		private static Bc4Block FindRedValues(Bc4Block colorBlock, byte[] pixels, int variations) {

			//Find min and max alpha
			byte min = 255;
			byte max = 0;
			var hasExtremeValues = false;
			for (var i = 0; i < pixels.Length; i++) {
				if (pixels[i] < 255 && pixels[i] > 0) {
					if (pixels[i] < min) min = pixels[i];
					if (pixels[i] > max) max = pixels[i];
				}
				else {
					hasExtremeValues = true;
				}
			}


			int SelectIndices(ref Bc4Block block) {
				var cumulativeError = 0;
				var c0 = block.Red0;
				var c1 = block.Red1;
				var colors = c0 > c1
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
				for (var i = 0; i < pixels.Length; i++) {
					byte bestIndex = 0;
					var bestError = Math.Abs(pixels[i] - colors[0]);
					for (byte j = 1; j < colors.Length; j++) {
						var error = Math.Abs(pixels[i] - colors[j]);
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
				var error = SelectIndices(ref colorBlock);
				Debug.Assert(0 == error);
				return colorBlock;
			}

			var best = colorBlock;
			best.Red0 = max;
			best.Red1 = min;
			var bestError = SelectIndices(ref best);
			if (bestError == 0) {
				return best;
			}

			for (var i = (byte)variations; i > 0; i--) {
				{
					var c0 = ByteHelper.ClampToByte(max - i);
					var c1 = ByteHelper.ClampToByte(min + i);
					var block = colorBlock;
					block.Red0 = hasExtremeValues ? c1 : c0;
					block.Red1 = hasExtremeValues ? c0 : c1;
					var error = SelectIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}
				{
					var c0 = ByteHelper.ClampToByte(max + i);
					var c1 = ByteHelper.ClampToByte(min - i);
					var block = colorBlock;
					block.Red0 = hasExtremeValues ? c1 : c0;
					block.Red1 = hasExtremeValues ? c0 : c1;
					var error = SelectIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}
				{
					var c0 = ByteHelper.ClampToByte(max);
					var c1 = ByteHelper.ClampToByte(min - i);
					var block = colorBlock;
					block.Red0 = hasExtremeValues ? c1 : c0;
					block.Red1 = hasExtremeValues ? c0 : c1;
					var error = SelectIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}
				{
					var c0 = ByteHelper.ClampToByte(max + i);
					var c1 = ByteHelper.ClampToByte(min);
					var block = colorBlock;
					block.Red0 = hasExtremeValues ? c1 : c0;
					block.Red1 = hasExtremeValues ? c0 : c1;
					var error = SelectIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}
				{
					var c0 = ByteHelper.ClampToByte(max);
					var c1 = ByteHelper.ClampToByte(min + i);
					var block = colorBlock;
					block.Red0 = hasExtremeValues ? c1 : c0;
					block.Red1 = hasExtremeValues ? c0 : c1;
					var error = SelectIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}
				{
					var c0 = ByteHelper.ClampToByte(max - i);
					var c1 = ByteHelper.ClampToByte(min);
					var block = colorBlock;
					block.Red0 = hasExtremeValues ? c1 : c0;
					block.Red1 = hasExtremeValues ? c0 : c1;
					var error = SelectIndices(ref block);
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
