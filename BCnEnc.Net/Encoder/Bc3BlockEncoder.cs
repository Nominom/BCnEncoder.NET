using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal class Bc3BlockEncoder : IBcBlockEncoder
	{

		public byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, EncodingQuality quality, bool parallel)
		{
			byte[] outputData = new byte[blockWidth * blockHeight * Marshal.SizeOf<Bc3Block>()];
			Span<Bc3Block> outputBlocks = MemoryMarshal.Cast<byte, Bc3Block>(outputData);

			if (parallel)
			{
				Parallel.For(0, blocks.Length, i =>
				{
					Span<Bc3Block> outputBlocks = MemoryMarshal.Cast<byte, Bc3Block>(outputData);
					outputBlocks[i] = EncodeBlock(blocks[i], quality);
				});
			}
			else
			{
				for (int i = 0; i < blocks.Length; i++)
				{
					outputBlocks[i] = EncodeBlock(blocks[i], quality);
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
					return Bc3BlockEncoderBalanced.EncodeBlock(block);
				case EncodingQuality.BestQuality:
					return Bc3BlockEncoderSlow.EncodeBlock(block);

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

		private static Bc3Block TryColors(RawBlock4X4Rgba32 rawBlock, ColorRgb565 color0, ColorRgb565 color1, out float error, float rWeight = 0.3f, float gWeight = 0.6f, float bWeight = 0.1f)
		{
			Bc3Block output = new Bc3Block();

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
			for (int i = 0; i < 16; i++)
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
				PcaVectors.GetMinMaxColor565(pixels, mean, principalAxis, out var min, out var max);

				ColorRgb565 c0 = max;
				ColorRgb565 c1 = min;

				if (c0.data <= c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				var output = TryColors(rawBlock, c0, c1, out float _);
				output = FindAlphaValues(output, rawBlock, 3);

				return output;
			}
		}

		private static class Bc3BlockEncoderBalanced {
			private const int maxTries = 24 * 2;
			private const float errorThreshold = 0.05f;

			internal static Bc3Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out System.Numerics.Vector3 mean, out System.Numerics.Vector3 pa);
				PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);

				var c0 = max;
				var c1 = min;

				Bc3Block best = TryColors(rawBlock, c0, c1, out float bestError);
				
				for (int i = 0; i < maxTries; i++) {
					var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);
					
					var block = TryColors(rawBlock, newC0, newC1, out var error);
					
					if (error < bestError)
					{
						best = block;
						bestError = error;
						c0 = newC0;
						c1 = newC1;
					}

					if (bestError < errorThreshold) {
						break;
					}
				}
				best = FindAlphaValues(best, rawBlock, 5);
				return best;
			}
		}

		private static class Bc3BlockEncoderSlow
		{
			private const int maxTries = 9999;
			private const float errorThreshold = 0.01f;


			internal static Bc3Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				PcaVectors.Create(pixels, out System.Numerics.Vector3 mean, out System.Numerics.Vector3 pa);
				PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);

				var c0 = max;
				var c1 = min;

				if (c0.data < c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				Bc3Block best = TryColors(rawBlock, c0, c1, out float bestError);

				int lastChanged = 0;

				for (int i = 0; i < maxTries; i++) {
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

					if (bestError < errorThreshold || lastChanged > ColorVariationGenerator.VarPatternCount) {
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
