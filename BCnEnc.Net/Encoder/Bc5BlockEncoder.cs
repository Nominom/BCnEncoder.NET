using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal class Bc5BlockEncoder : BaseBcBlockEncoder<Bc5Block>
	{
		protected override Bc5Block EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality) {
			var output = new Bc5Block();
			var reds = new byte[16];
			var greens = new byte[16];
			var pixels = block.AsSpan;
			for (var i = 0; i < 16; i++) {
				reds[i] = pixels[i].R;
				greens[i] = pixels[i].G;
			}

			var variations = 0;
			var errorThreshold = 0;
 			switch (quality) {
				case CompressionQuality.Fast:
					variations = 3;
					errorThreshold = 5;
					break;
				case CompressionQuality.Balanced:
					variations = 5;
					errorThreshold = 1;
					break;
				case CompressionQuality.BestQuality:
					variations = 8;
					errorThreshold = 0;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}


            output =  FindValues(output, reds, variations, errorThreshold,
	            (block, i, idx) => {
		            block.SetRedIndex(i, idx);
		            return block;
	            },
	            (block, col) => {
		            block.Red0 = col;
		            return block;
	            },
	            (block, col) => {
		            block.Red1 = col;
		            return block;
	            },
	            (block) => {
		            return block.Red0;
	            },
	            (block) => {
		            return block.Red1;
	            }
            );
            output =  FindValues(output, greens, variations, errorThreshold,
	            (block, i, idx) => {
		            block.SetGreenIndex(i, idx);
		            return block;
	            },
	            (block, col) => {
		            block.Green0 = col;
		            return block;
	            },
	            (block, col) => {
		            block.Green1 = col;
		            return block;
	            },
	            (block) => {
		            return block.Green0;
	            },
	            (block) => {
		            return block.Green1;
	            });
            return output;
		}

		public override GlInternalFormat GetInternalFormat() {
			return GlInternalFormat.GlCompressedRedGreenRgtc2Ext;
		}

		public override GlFormat GetBaseInternalFormat() {
			return GlFormat.GlRg;
		}

		public override DxgiFormat GetDxgiFormat() {
			return DxgiFormat.DxgiFormatBc5Unorm;
		}

		#region Encoding private stuff

		private static Bc5Block FindValues(Bc5Block colorBlock, byte[] pixels, int variations, int errorThreshold, 
			Func<Bc5Block, int, byte, Bc5Block> indexSetter, 
			Func<Bc5Block, byte, Bc5Block> col0Setter,
			Func<Bc5Block, byte, Bc5Block> col1Setter,
			Func<Bc5Block, byte> col0Getter,
			Func<Bc5Block, byte> col1Getter) {

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


			int SelectIndices(ref Bc5Block block) {
				var cumulativeError = 0;
				//var c0 = block.Red0;
				//var c1 = block.Red1;
				var c0 = col0Getter(block);
				var c1 = col1Getter(block);
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

					block = indexSetter(block, i, bestIndex);
					//block.SetRedIndex(i, bestIndex);
					cumulativeError += bestError * bestError;
				}

				return cumulativeError;
			}

			//everything is either fully black or fully red
			if (hasExtremeValues && min == 255 && max == 0) {
				//colorBlock.Red0 = 0;
				//colorBlock.Red1 = 255;
				colorBlock = col0Setter(colorBlock, 0);
				colorBlock = col1Setter(colorBlock, 255);
				var error = SelectIndices(ref colorBlock);
				Debug.Assert(0 == error);
				return colorBlock;
			}

			var best = colorBlock;
			//best.Red0 = max;
			//best.Red1 = min;
			best = col0Setter(best, max);
			best = col1Setter(best, min);
			var bestError = SelectIndices(ref best);
			if (bestError == 0) {
				return best;
			}

			for (var i = (byte)variations; i > 0; i--) {
				{
					var c0 = ByteHelper.ClampToByte(max - i);
					var c1 = ByteHelper.ClampToByte(min + i);
					var block = colorBlock;
					//block.Red0 = hasExtremeValues ? c1 : c0;
					//block.Red1 = hasExtremeValues ? c0 : c1;
					block = col0Setter(block, hasExtremeValues ? c1 : c0);
					block = col1Setter(block, hasExtremeValues ? c0 : c1);
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
					//block.Red0 = hasExtremeValues ? c1 : c0;
					//block.Red1 = hasExtremeValues ? c0 : c1;
					block = col0Setter(block, hasExtremeValues ? c1 : c0);
					block = col1Setter(block, hasExtremeValues ? c0 : c1);
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
					//block.Red0 = hasExtremeValues ? c1 : c0;
					//block.Red1 = hasExtremeValues ? c0 : c1;
					block = col0Setter(block, hasExtremeValues ? c1 : c0);
					block = col1Setter(block, hasExtremeValues ? c0 : c1);
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
					//block.Red0 = hasExtremeValues ? c1 : c0;
					//block.Red1 = hasExtremeValues ? c0 : c1;
					block = col0Setter(block, hasExtremeValues ? c1 : c0);
					block = col1Setter(block, hasExtremeValues ? c0 : c1);
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
					//block.Red0 = hasExtremeValues ? c1 : c0;
					//block.Red1 = hasExtremeValues ? c0 : c1;
					block = col0Setter(block, hasExtremeValues ? c1 : c0);
					block = col1Setter(block, hasExtremeValues ? c0 : c1);
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
					//block.Red0 = hasExtremeValues ? c1 : c0;
					//block.Red1 = hasExtremeValues ? c0 : c1;
					block = col0Setter(block, hasExtremeValues ? c1 : c0);
					block = col1Setter(block, hasExtremeValues ? c0 : c1);
					var error = SelectIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}

				if (bestError <= errorThreshold) {
					break;
				}
			}

			return best;
		}


		#endregion
	}
}
