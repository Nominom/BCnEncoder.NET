using System;
using System.Diagnostics;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal class Bc4BlockEncoder : BaseBcLdrBlockEncoder<Bc4Block>
	{
		private readonly Bc4ComponentBlockEncoder bc4Encoder;

		public Bc4BlockEncoder(ColorComponent component)
		{
			bc4Encoder = new Bc4ComponentBlockEncoder(component);
		}

		public override Bc4Block EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			var output = new Bc4Block
			{
				componentBlock = bc4Encoder.EncodeBlock(block, quality)
			};

			return output;
		}

		/// <inheritdoc />
		public override CompressionFormat EncodedFormat => CompressionFormat.Bc4;
	}

	internal class Bc4ComponentBlockEncoder
	{
		private readonly ColorComponent component;

		public Bc4ComponentBlockEncoder(ColorComponent component)
		{
			this.component = component;
		}

		public Bc4ComponentBlock EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			var output = new Bc4ComponentBlock();

			var pixels = block.AsSpan;
			var colors = new byte[pixels.Length];

			for (var i = 0; i < pixels.Length; i++)
				colors[i] = ComponentHelper.ColorToComponent(pixels[i], component);

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

		#region Encoding private stuff

		private static Bc4ComponentBlock FindComponentValues(Bc4ComponentBlock colorBlock, byte[] pixels, int variations)
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


			int SelectIndices(ref Bc4ComponentBlock block)
			{
				var cumulativeError = 0;
				var c0 = block.Endpoint0;
				var c1 = block.Endpoint1;
				var colors = c0 > c1 ? stackalloc byte[] {
					c0,
					c1,
					c0.InterpolateSeventh(c1, 1),
					c0.InterpolateSeventh(c1, 2),
					c0.InterpolateSeventh(c1, 3),
					c0.InterpolateSeventh(c1, 4),
					c0.InterpolateSeventh(c1, 5),
					c0.InterpolateSeventh(c1, 6)
				} : stackalloc byte[] {
					c0,
					c1,
					c0.InterpolateFifth(c1, 1),
					c0.InterpolateFifth(c1, 2),
					c0.InterpolateFifth(c1, 3),
					c0.InterpolateFifth(c1, 4),
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
