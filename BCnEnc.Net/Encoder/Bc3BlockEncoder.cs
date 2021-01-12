using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal class Bc3BlockEncoder : BaseBcBlockEncoder<Bc3Block>
	{
		protected override Bc3Block EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
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

		public override DxgiFormat GetDxgiFormat() {
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
				c0 * (2.0 / 3.0) + c1 * (1.0 / 3.0),
				c0 * (1.0 / 3.0) + c1 * (2.0 / 3.0)
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

		private static Bc3Block FindAlphaValues(Bc3Block colorBlock, RawBlock4X4Rgba32 rawBlock, int variations) {
			var pixels = rawBlock.AsSpan;

			//Find min and max alpha
			byte minAlpha = 255;
			byte maxAlpha = 0;
			var hasExtremeValues = false;
			for (var i = 0; i < pixels.Length; i++) {
				if (pixels[i].A < 255 && pixels[i].A > 0) {
					if (pixels[i].A < minAlpha) minAlpha = pixels[i].A;
					if (pixels[i].A > maxAlpha) maxAlpha = pixels[i].A;
				}
				else {
					hasExtremeValues = true;
				}
			}


			int SelectAlphaIndices(ref Bc3Block block) {
				var cumulativeError = 0;
				var a0 = block.Alpha0;
				var a1 = block.Alpha1;
				var alphas = a0 > a1 ? stackalloc byte[] {
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
				for (var i = 0; i < pixels.Length; i++) {
					byte bestIndex = 0;
					var bestError = Math.Abs(pixels[i].A - alphas[0]);
					for (byte j = 1; j < alphas.Length; j++) {
						var error = Math.Abs(pixels[i].A - alphas[j]);
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
			var bestError = SelectAlphaIndices(ref best);
			if (bestError == 0) {
				return best;
			}
			for (byte i = 1; i < variations; i++) {
				{
					var a0 = ByteHelper.ClampToByte(maxAlpha - i * 2);
					var a1 = ByteHelper.ClampToByte(minAlpha + i * 2);
					var block = colorBlock;
					block.Alpha0 = hasExtremeValues ? a1 : a0;
					block.Alpha1 = hasExtremeValues ? a0 : a1;
					var error = SelectAlphaIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
					}
				}
				{
					var a0 = ByteHelper.ClampToByte(maxAlpha + i * 2);
					var a1 = ByteHelper.ClampToByte(minAlpha - i * 2);
					var block = colorBlock;
					block.Alpha0 = hasExtremeValues ? a1 : a0;
					block.Alpha1 = hasExtremeValues ? a0 : a1;
					var error = SelectAlphaIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
					}
				}
				{
					var a0 = ByteHelper.ClampToByte(maxAlpha);
					var a1 = ByteHelper.ClampToByte(minAlpha - i * 2);
					var block = colorBlock;
					block.Alpha0 = hasExtremeValues ? a1 : a0;
					block.Alpha1 = hasExtremeValues ? a0 : a1;
					var error = SelectAlphaIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
					}
				}
				{
					var a0 = ByteHelper.ClampToByte(maxAlpha + i * 2);
					var a1 = ByteHelper.ClampToByte(minAlpha);
					var block = colorBlock;
					block.Alpha0 = hasExtremeValues ? a1 : a0;
					block.Alpha1 = hasExtremeValues ? a0 : a1;
					var error = SelectAlphaIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
					}
				}
				{
					var a0 = ByteHelper.ClampToByte(maxAlpha);
					var a1 = ByteHelper.ClampToByte(minAlpha + i * 2);
					var block = colorBlock;
					block.Alpha0 = hasExtremeValues ? a1 : a0;
					block.Alpha1 = hasExtremeValues ? a0 : a1;
					var error = SelectAlphaIndices(ref block);
					if (error < bestError) {
						best = block;
						bestError = error;
					}
				}
				{
					var a0 = ByteHelper.ClampToByte(maxAlpha - i * 2);
					var a1 = ByteHelper.ClampToByte(minAlpha);
					var block = colorBlock;
					block.Alpha0 = hasExtremeValues ? a1 : a0;
					block.Alpha1 = hasExtremeValues ? a0 : a1;
					var error = SelectAlphaIndices(ref block);
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
				PcaVectors.GetMinMaxColor565(pixels, mean, principalAxis, out var min, out var max);

				var c0 = max;
				var c1 = min;

				if (c0.data <= c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				var output = TryColors(rawBlock, c0, c1, out var _);
				output = FindAlphaValues(output, rawBlock, 3);

				return output;
			}
		}

		private static class Bc3BlockEncoderBalanced {
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
				
				for (var i = 0; i < MaxTries; i++) {
					var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);
					
					var block = TryColors(rawBlock, newC0, newC1, out var error);
					
					if (error < bestError)
					{
						best = block;
						bestError = error;
						c0 = newC0;
						c1 = newC1;
					}

					if (bestError < ErrorThreshold) {
						break;
					}
				}
				best = FindAlphaValues(best, rawBlock, 5);
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

				for (var i = 0; i < MaxTries; i++) {
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

					if (bestError < ErrorThreshold || lastChanged > ColorVariationGenerator.VarPatternCount) {
						break;
					}
				}

				best = FindAlphaValues(best, rawBlock, 8);
				return best;
			}
		}
		#endregion
	}
}
