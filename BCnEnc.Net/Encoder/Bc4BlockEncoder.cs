using System;
using System.Diagnostics;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal enum Bc4Component
	{
		R,
		G,
		B,
		A
	}

	internal class Bc4BlockEncoder : BaseBcBlockEncoder<Bc4Block>
	{
		private readonly bool luminanceAsComponent;
		private readonly Bc4Component component;

		public Bc4BlockEncoder(bool luminanceAsComponent, Bc4Component component = Bc4Component.R)
		{
			this.luminanceAsComponent = luminanceAsComponent;
			this.component = component;
		}

		public override Bc4Block EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			var output = new Bc4Block();
			var colors = new byte[16];
			var pixels = block.AsSpan;
			for (var i = 0; i < 16; i++)
			{
				if (luminanceAsComponent)
				{
					colors[i] = (byte)(new ColorYCbCr(pixels[i]).y * 255);
				}
				else
				{
					switch (component)
					{
						case Bc4Component.R:
							colors[i] = pixels[i].R;
							break;

						case Bc4Component.G:
							colors[i] = pixels[i].G;
							break;

						case Bc4Component.B:
							colors[i] = pixels[i].B;
							break;

						case Bc4Component.A:
							colors[i] = pixels[i].A;
							break;
					}
				}
			}
			switch (quality)
			{
				case CompressionQuality.Fast:
					return FindComponentValues(output, colors, 3);
				case CompressionQuality.Balanced:
					return FindComponentValues(output, colors, 4);
				case CompressionQuality.BestQuality:
					return FindComponentValues(output, colors, 8);

				default:
					throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
			}
		}

		public override GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlCompressedRedRgtc1Ext;
		}

		public override GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRed;
		}

		public override DxgiFormat GetDxgiFormat()
		{
			return DxgiFormat.DxgiFormatBc4Unorm;
		}

		#region Encoding private stuff

		private static Bc4Block FindComponentValues(Bc4Block colorBlock, byte[] pixels, int variations)
		{

			//Find min and max alpha
			byte min = 255;
			byte max = 0;
			var hasExtremeValues = false;
			for (var i = 0; i < pixels.Length; i++)
			{
				if (pixels[i] < 255 && pixels[i] > 0)
				{
					if (pixels[i] < min) min = pixels[i];
					if (pixels[i] > max) max = pixels[i];
				}
				else
				{
					hasExtremeValues = true;
				}
			}


			int SelectIndices(ref Bc4Block block)
			{
				var cumulativeError = 0;
				var c0 = block.Endpoint0;
				var c1 = block.Endpoint1;
				var colors = c0 > c1
					? stackalloc byte[] {
					c0,
					c1,
					(byte)(6 / 7.0 * c0 + 1 / 7.0 * c1),
					(byte)(5 / 7.0 * c0 + 2 / 7.0 * c1),
					(byte)(4 / 7.0 * c0 + 3 / 7.0 * c1),
					(byte)(3 / 7.0 * c0 + 4 / 7.0 * c1),
					(byte)(2 / 7.0 * c0 + 5 / 7.0 * c1),
					(byte)(1 / 7.0 * c0 + 6 / 7.0 * c1),
				}
					: stackalloc byte[] {
					c0,
					c1,
					(byte)(4 / 5.0 * c0 + 1 / 5.0 * c1),
					(byte)(3 / 5.0 * c0 + 2 / 5.0 * c1),
					(byte)(2 / 5.0 * c0 + 3 / 5.0 * c1),
					(byte)(1 / 5.0 * c0 + 4 / 5.0 * c1),
					0,
					255
				};
				for (var i = 0; i < pixels.Length; i++)
				{
					byte bestIndex = 0;
					var bestError = Math.Abs(pixels[i] - colors[0]);
					for (byte j = 1; j < colors.Length; j++)
					{
						var error = Math.Abs(pixels[i] - colors[j]);
						if (error < bestError)
						{
							bestIndex = j;
							bestError = error;
						}

						if (bestError == 0) break;
					}

					block.SetComponentIndex(i, bestIndex);
					cumulativeError += bestError * bestError;
				}

				return cumulativeError;
			}

			//everything is either fully black or fully red
			if (hasExtremeValues && min == 255 && max == 0)
			{
				colorBlock.Endpoint0 = 0;
				colorBlock.Endpoint1 = 255;
				var error = SelectIndices(ref colorBlock);
				Debug.Assert(0 == error);
				return colorBlock;
			}

			var best = colorBlock;
			best.Endpoint0 = max;
			best.Endpoint1 = min;
			var bestError = SelectIndices(ref best);
			if (bestError == 0)
			{
				return best;
			}

			for (var i = (byte)variations; i > 0; i--)
			{
				{
					var c0 = ByteHelper.ClampToByte(max - i);
					var c1 = ByteHelper.ClampToByte(min + i);
					var block = colorBlock;
					block.Endpoint0 = hasExtremeValues ? c1 : c0;
					block.Endpoint1 = hasExtremeValues ? c0 : c1;
					var error = SelectIndices(ref block);
					if (error < bestError)
					{
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
					block.Endpoint0 = hasExtremeValues ? c1 : c0;
					block.Endpoint1 = hasExtremeValues ? c0 : c1;
					var error = SelectIndices(ref block);
					if (error < bestError)
					{
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
					block.Endpoint0 = hasExtremeValues ? c1 : c0;
					block.Endpoint1 = hasExtremeValues ? c0 : c1;
					var error = SelectIndices(ref block);
					if (error < bestError)
					{
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
					block.Endpoint0 = hasExtremeValues ? c1 : c0;
					block.Endpoint1 = hasExtremeValues ? c0 : c1;
					var error = SelectIndices(ref block);
					if (error < bestError)
					{
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
					block.Endpoint0 = hasExtremeValues ? c1 : c0;
					block.Endpoint1 = hasExtremeValues ? c0 : c1;
					var error = SelectIndices(ref block);
					if (error < bestError)
					{
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
					block.Endpoint0 = hasExtremeValues ? c1 : c0;
					block.Endpoint1 = hasExtremeValues ? c0 : c1;
					var error = SelectIndices(ref block);
					if (error < bestError)
					{
						best = block;
						bestError = error;
						max = c0;
						min = c1;
					}
				}

				if (bestError < 5)
				{
					break;
				}
			}

			return best;
		}

		#endregion
	}
}
