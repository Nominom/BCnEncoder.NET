using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder
{
	internal class Bc1BlockEncoder : IBcBlockEncoder
	{

		public byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, bool parallel)
		{
			byte[] outputData = new byte[blockWidth * blockHeight * Marshal.SizeOf<Bc1Block>()];
			Span<Bc1Block> outputBlocks = MemoryMarshal.Cast<byte, Bc1Block>(outputData);

			if (parallel)
			{
				Parallel.For(0, blocks.Length, i =>
				{
					Span<Bc1Block> outputBlocks = MemoryMarshal.Cast<byte, Bc1Block>(outputData);
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

		private Bc1Block EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			switch (quality)
			{
				case CompressionQuality.Fast:
					return Bc1BlockEncoderFast.EncodeBlock(block);
				case CompressionQuality.Balanced:
					return Bc1BlockEncoderBalanced.EncodeBlock(block);
				case CompressionQuality.BestQuality:
					return Bc1BlockEncoderSlow.EncodeBlock(block);

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

		public DXGI_FORMAT GetDxgiFormat()
		{
			return DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM;
		}

		#region Encoding private stuff

		private static Bc1Block TryColors(RawBlock4X4Rgba32 rawBlock, ColorRgb565 color0, ColorRgb565 color1, out float error, float rWeight = 0.3f, float gWeight = 0.6f, float bWeight = 0.1f)
		{
			Bc1Block output = new Bc1Block();

			var pixels = rawBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			var c0 = color0.ToColorRgb24();
			var c1 = color1.ToColorRgb24();

			ReadOnlySpan<ColorRgb24> colors = output.HasAlphaOrBlack ?
				stackalloc ColorRgb24[] {
				c0,
				c1,
				c0 * (1.0 / 2.0) + c1 * (1.0 / 2.0),
				new ColorRgb24(0, 0, 0)
			} : stackalloc ColorRgb24[] {
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


		#endregion

		#region Encoders

		private static class Bc1BlockEncoderFast
		{

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				Bc1Block output = new Bc1Block();

				var pixels = rawBlock.AsSpan;

				RgbBoundingBox.Create565(pixels, out var min, out var max);

				ColorRgb565 c0 = max;
				ColorRgb565 c1 = min;

				output = TryColors(rawBlock, c0, c1, out var error);

				return output;
			}
		}

		private static class Bc1BlockEncoderBalanced {
			private const int maxTries = 24 * 2;
			private const float errorThreshold = 0.05f;

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
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

				Bc1Block best = TryColors(rawBlock, c0, c1, out float bestError);
				
				for (int i = 0; i < maxTries; i++) {
					var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);
					
					if (newC0.data < newC1.data)
					{
						var c = newC0;
						newC0 = newC1;
						newC1 = c;
					}
					
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

				return best;
			}
		}

		private static class Bc1BlockEncoderSlow
		{
			private const int maxTries = 9999;
			private const float errorThreshold = 0.01f;

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
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

				Bc1Block best = TryColors(rawBlock, c0, c1, out float bestError);

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

				return best;
			}
		}

		#endregion
	}

	internal class Bc1AlphaBlockEncoder : IBcBlockEncoder
	{

		public byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, CompressionQuality quality, bool parallel)
		{
			byte[] outputData = new byte[blockWidth * blockHeight * Marshal.SizeOf<Bc1Block>()];
			Span<Bc1Block> outputBlocks = MemoryMarshal.Cast<byte, Bc1Block>(outputData);

			if (parallel)
			{
				Parallel.For(0, blocks.Length, i =>
				{
					Span<Bc1Block> outputBlocks = MemoryMarshal.Cast<byte, Bc1Block>(outputData);
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

		private Bc1Block EncodeBlock(RawBlock4X4Rgba32 block, CompressionQuality quality)
		{
			switch (quality)
			{
				case CompressionQuality.Fast:
					return Bc1AlphaBlockEncoderFast.EncodeBlock(block);
				case CompressionQuality.Balanced:
					return Bc1AlphaBlockEncoderBalanced.EncodeBlock(block);
				case CompressionQuality.BestQuality:
					return Bc1AlphaBlockEncoderSlow.EncodeBlock(block);

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

		public DXGI_FORMAT GetDxgiFormat()
		{
			return DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM;
		}

		private static Bc1Block TryColors(RawBlock4X4Rgba32 rawBlock, ColorRgb565 color0, ColorRgb565 color1, out float error, float rWeight = 0.3f, float gWeight = 0.6f, float bWeight = 0.1f)
		{
			Bc1Block output = new Bc1Block();

			var pixels = rawBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			var c0 = color0.ToColorRgb24();
			var c1 = color1.ToColorRgb24();

			bool hasAlpha = output.HasAlphaOrBlack;

			ReadOnlySpan<ColorRgb24> colors = hasAlpha ?
				stackalloc ColorRgb24[] {
				c0,
				c1,
				c0 * (1.0 / 2.0) + c1 * (1.0 / 2.0),
				new ColorRgb24(0, 0, 0)
			} : stackalloc ColorRgb24[] {
				c0,
				c1,
				c0 * (2.0 / 3.0) + c1 * (1.0 / 3.0),
				c0 * (1.0 / 3.0) + c1 * (2.0 / 3.0)
			};

			error = 0;
			for (int i = 0; i < 16; i++)
			{
				var color = pixels[i];
				output[i] = ColorChooser.ChooseClosestColor4AlphaCutoff(colors, color, rWeight, gWeight, bWeight,
					128, hasAlpha, out var e);
				error += e;
			}

			return output;
		}


		#region Encoders

		private static class Bc1AlphaBlockEncoderFast
		{

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				Bc1Block output = new Bc1Block();

				var pixels = rawBlock.AsSpan;

				bool hasAlpha = rawBlock.HasTransparentPixels();

				RgbBoundingBox.Create565AlphaCutoff(pixels, out var min, out var max);

				ColorRgb565 c0 = max;
				ColorRgb565 c1 = min;

				if (hasAlpha && c0.data > c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				output = TryColors(rawBlock, c0, c1, out var error);

				return output;
			}
		}

		private static class Bc1AlphaBlockEncoderBalanced
		{
			private const int maxTries = 24 * 2;
			private const float errorThreshold = 0.05f;


			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				bool hasAlpha = rawBlock.HasTransparentPixels();

				PcaVectors.Create(pixels, out System.Numerics.Vector3 mean, out System.Numerics.Vector3 pa);
				PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);

				var c0 = max;
				var c1 = min;

				if (!hasAlpha && c0.data < c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}else if (hasAlpha && c1.data < c0.data) {
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				Bc1Block best = TryColors(rawBlock, c0, c1, out float bestError);
				
				for (int i = 0; i < maxTries; i++) {
					var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);
					
					if (!hasAlpha && newC0.data < newC1.data)
					{
						var c = newC0;
						newC0 = newC1;
						newC1 = c;
					}else if (hasAlpha && newC1.data < newC0.data) {
						var c = newC0;
						newC0 = newC1;
						newC1 = c;
					}
					
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

				return best;
			}
		}

		private static class Bc1AlphaBlockEncoderSlow
		{
			private const int maxTries = 9999;
			private const float errorThreshold = 0.05f;

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				bool hasAlpha = rawBlock.HasTransparentPixels();

				PcaVectors.Create(pixels, out System.Numerics.Vector3 mean, out System.Numerics.Vector3 pa);
				PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);

				var c0 = max;
				var c1 = min;

				if (!hasAlpha && c0.data < c1.data)
				{
					var c = c0;
					c0 = c1;
					c1 = c;
				}else if (hasAlpha && c1.data < c0.data) {
					var c = c0;
					c0 = c1;
					c1 = c;
				}

				Bc1Block best = TryColors(rawBlock, c0, c1, out float bestError);

				int lastChanged = 0;
				for (int i = 0; i < maxTries; i++) {
					var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);
					
					if (!hasAlpha && newC0.data < newC1.data)
					{
						var c = newC0;
						newC0 = newC1;
						newC1 = c;
					}else if (hasAlpha && newC1.data < newC0.data) {
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

				return best;
			}
		}

		#endregion

	}
}
