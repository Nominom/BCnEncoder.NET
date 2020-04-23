using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Accord.Diagnostics;
using Accord.Math;
using BCnEnc.Net.Shared;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Encoder
{
	internal class Bc1BlockEncoder : IBcBlockEncoder
	{

		public byte[] Encode(RawBlock4X4Rgba32[,] blocks, int blockWidth, int blockHeight, EncodingQuality quality, bool parallel)
		{
			byte[] outputData = new byte[blockWidth * blockHeight * Marshal.SizeOf<Bc1Block>()];
			Memory<RawBlock4X4Rgba32> inputBlocks = new Memory<RawBlock4X4Rgba32>(blocks.Reshape(MatrixOrder.FortranColumnMajor));
			Span<Bc1Block> outputBlocks = MemoryMarshal.Cast<byte, Bc1Block>(outputData);

			if (parallel)
			{
				Parallel.For(0, inputBlocks.Length, i =>
				{
					Span<Bc1Block> outputBlocks = MemoryMarshal.Cast<byte, Bc1Block>(outputData);
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

		private Bc1Block EncodeBlock(RawBlock4X4Rgba32 block, EncodingQuality quality)
		{
			switch (quality)
			{
				case EncodingQuality.Fast:
					return Bc1BlockEncoderFast.EncodeBlock(block);
				case EncodingQuality.Balanced:
					return Bc1BlockEncoderBalanced.EncodeBlock(block);
				case EncodingQuality.BestQuality:
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

		private static Bc1Block TryColors(RawBlock4X4Ycbcr rawBlock, ColorRgb565 color0, ColorRgb565 color1)
		{
			Bc1Block output = new Bc1Block();

			var pixels = rawBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			var c0 = color0.ToColorRgb24();
			var c1 = color1.ToColorRgb24();

			Span<ColorYCbCr> colors = output.HasAlphaOrBlack ?
				stackalloc ColorYCbCr[] {
				new ColorYCbCr(c0),
				new ColorYCbCr(c1),
				new ColorYCbCr(c0 * (1.0 / 2.0) + c1 * (1.0 / 2.0)),
				new ColorYCbCr(0, 0, 0)
			} : stackalloc ColorYCbCr[] {
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

		private static class Bc1BlockEncoderBalanced
		{

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				RgbBoundingBox.Create565(pixels, out var min, out var max);

				var c0565 = max;
				var c1565 = min;
				Span<ColorRgb565> c0 = stackalloc ColorRgb565[] {
					new ColorRgb565(c0565.R, c0565.G, c0565.B),
					new ColorRgb565(c0565.R, ByteHelper.ClampToByte(c0565.G + 8), c0565.B),
					new ColorRgb565(c0565.R, ByteHelper.ClampToByte(c0565.G + 4), c0565.B),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 8), c0565.B),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 4), c0565.B),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 8), ByteHelper.ClampToByte(c0565.B + 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 4), ByteHelper.ClampToByte(c0565.B + 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 12), ByteHelper.ClampToByte(c0565.B + 8)),

					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 8), ByteHelper.ClampToByte(c0565.B + 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 4), ByteHelper.ClampToByte(c0565.B + 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 12), ByteHelper.ClampToByte(c0565.B + 8)),

					new ColorRgb565(c0565.R, c0565.G, c0565.B),
					new ColorRgb565(c0565.R, c0565.G, c0565.B),
					new ColorRgb565(c0565.R, c0565.G, c0565.B),
				};
				Span<ColorRgb565> c1 = stackalloc ColorRgb565[] {
					new ColorRgb565(c1565.R, c1565.G, c1565.B),
					new ColorRgb565(c1565.R, ByteHelper.ClampToByte(c1565.G - 8), c1565.B),
					new ColorRgb565(c1565.R, ByteHelper.ClampToByte(c1565.G - 4), c1565.B),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 8), c1565.B),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 4), c1565.B),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 8), ByteHelper.ClampToByte(c1565.B - 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 4), ByteHelper.ClampToByte(c1565.B - 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 12), ByteHelper.ClampToByte(c1565.B - 8)),

					new ColorRgb565(c1565.R, c1565.G, c1565.B),
					new ColorRgb565(c1565.R, c1565.G, c1565.B),
					new ColorRgb565(c1565.R, c1565.G, c1565.B),

					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 8), ByteHelper.ClampToByte(c1565.B - 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 4), ByteHelper.ClampToByte(c1565.B - 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 12), ByteHelper.ClampToByte(c1565.B - 8)),
				};

				float bestError = 999;
				Bc1Block best = default;
				for (int i = 0; i < c0.Length; i++)
				{
					var block = TryColors(rawBlock, c0[i], c1[i], out var error);
					if (error < bestError)
					{
						best = block;
						bestError = error;
					}
				}

				return best;
			}
		}

		private static class Bc1BlockEncoderSlow
		{
			private const int maxTries = 500;
			private const float errorThreshold = 0.01f;

			private static IEnumerable<(ColorRgb565, ColorRgb565)> VariateEndpoints(ColorRgb565 ep0, ColorRgb565 ep1)
			{
				yield return (ep0, ep1);

				Random r = new Random(0);
				byte[] randoms = new byte[6];
				while (true) {
					r.NextBytes(randoms);

					var newEp0 = ep0;
					newEp0.RawR = newEp0.RawR + ((randoms[0] >> 6) - 2);
					newEp0.RawG = newEp0.RawG + ((randoms[1] >> 5) - 4);
					newEp0.RawB = newEp0.RawB + ((randoms[2] >> 6) - 2);

					yield return (newEp0, ep1);

					var newEp1 = ep1;
					newEp1.RawR = newEp1.RawR - ((randoms[3] >> 6) - 2);
					newEp1.RawG = newEp1.RawG - ((randoms[4] >> 5) - 4);
					newEp1.RawB = newEp1.RawB - ((randoms[5] >> 6) - 2);

					yield return (ep0, newEp1);

					yield return (newEp0, newEp1);
				}
			}

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				RgbBoundingBox.Create565(pixels, out var min, out var max);

				float bestError = 999;
				Bc1Block best = default;
				int tries = 0;
				foreach (var (c0, c1) in VariateEndpoints(max, min)) {
					Bc1Block block;
					float error;
					if (c0.data < c1.data) {
						block = TryColors(rawBlock, c1, c0, out error,
							0.1f, 0.85f, 0.05f);
						
					}
					else {
						block = TryColors(rawBlock, c0, c1, out error,
							0.1f, 0.85f, 0.05f);

					}

					if (error < bestError)
					{
						best = block;
						bestError = error;
					}
					
					tries++;
					if (tries > maxTries || bestError < errorThreshold)
					{
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

		public byte[] Encode(RawBlock4X4Rgba32[,] blocks, int blockWidth, int blockHeight, EncodingQuality quality,
			bool parallel)
		{
			byte[] outputData = new byte[blockWidth * blockHeight * Marshal.SizeOf<Bc1Block>()];
			Memory<RawBlock4X4Rgba32> inputBlocks =
				new Memory<RawBlock4X4Rgba32>(blocks.Reshape(MatrixOrder.FortranColumnMajor));
			Span<Bc1Block> outputBlocks = MemoryMarshal.Cast<byte, Bc1Block>(outputData);

			if (parallel)
			{
				Parallel.For(0, inputBlocks.Length, i =>
				{
					Span<Bc1Block> outputBlocks = MemoryMarshal.Cast<byte, Bc1Block>(outputData);
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

		private Bc1Block EncodeBlock(RawBlock4X4Rgba32 block, EncodingQuality quality)
		{
			switch (quality)
			{
				case EncodingQuality.Fast:
					return Bc1AlphaBlockEncoderFast.EncodeBlock(block);
				case EncodingQuality.Balanced:
					return Bc1AlphaBlockEncoderBalanced.EncodeBlock(block);
				case EncodingQuality.BestQuality:
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

				if (hasAlpha && c0.data > c1.data) {
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

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				bool hasAlpha = rawBlock.HasTransparentPixels();

				RgbBoundingBox.Create565AlphaCutoff(pixels, out var min, out var max);

				var c0565 = max;
				var c1565 = min;
				Span<ColorRgb565> c0 = stackalloc ColorRgb565[] {
					new ColorRgb565(c0565.R, c0565.G, c0565.B),
					new ColorRgb565(c0565.R, ByteHelper.ClampToByte(c0565.G + 8), c0565.B),
					new ColorRgb565(c0565.R, ByteHelper.ClampToByte(c0565.G + 4), c0565.B),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 8), c0565.B),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 4), c0565.B),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 8), ByteHelper.ClampToByte(c0565.B + 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 4), ByteHelper.ClampToByte(c0565.B + 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 12), ByteHelper.ClampToByte(c0565.B + 8)),

					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 8), ByteHelper.ClampToByte(c0565.B + 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 4), ByteHelper.ClampToByte(c0565.B + 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c0565.R + 8), ByteHelper.ClampToByte(c0565.G + 12), ByteHelper.ClampToByte(c0565.B + 8)),

					new ColorRgb565(c0565.R, c0565.G, c0565.B),
					new ColorRgb565(c0565.R, c0565.G, c0565.B),
					new ColorRgb565(c0565.R, c0565.G, c0565.B),
				};
				Span<ColorRgb565> c1 = stackalloc ColorRgb565[] {
					new ColorRgb565(c1565.R, c1565.G, c1565.B),
					new ColorRgb565(c1565.R, ByteHelper.ClampToByte(c1565.G - 8), c1565.B),
					new ColorRgb565(c1565.R, ByteHelper.ClampToByte(c1565.G - 4), c1565.B),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 8), c1565.B),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 4), c1565.B),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 8), ByteHelper.ClampToByte(c1565.B - 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 4), ByteHelper.ClampToByte(c1565.B - 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 12), ByteHelper.ClampToByte(c1565.B - 8)),

					new ColorRgb565(c1565.R, c1565.G, c1565.B),
					new ColorRgb565(c1565.R, c1565.G, c1565.B),
					new ColorRgb565(c1565.R, c1565.G, c1565.B),

					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 8), ByteHelper.ClampToByte(c1565.B - 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 4), ByteHelper.ClampToByte(c1565.B - 8)),
					new ColorRgb565(ByteHelper.ClampToByte(c1565.R - 8), ByteHelper.ClampToByte(c1565.G - 12), ByteHelper.ClampToByte(c1565.B - 8)),
				};

				float bestError = 999;
				Bc1Block best = default;
				for (int i = 0; i < c0.Length; i++)
				{

					if (hasAlpha && c0[i].data > c1[i].data) {
						var c = c0[i];
						c0[i] = c1[i];
						c1[i] = c;
					}

					var block = TryColors(rawBlock, c0[i], c1[i], out var error);
					if (error < bestError)
					{
						best = block;
						bestError = error;
					}
				}

				if (best.Decode(true).AsSpan.ToArray().All(x =>
					x.R == 0 &&
					x.G == 0 &&
					x.B == 0 &&
					x.A == 255
				)) {
					Console.Write("");
				}

				return best;
			}
		}

		private static class Bc1AlphaBlockEncoderSlow
		{
			private const int maxTries = 500;
			private const float errorThreshold = 0.01f;

			private static IEnumerable<(ColorRgb565, ColorRgb565)> VariateEndpoints(ColorRgb565 ep0, ColorRgb565 ep1)
			{
				yield return (ep0, ep1);

				Random r = new Random(0);
				byte[] randoms = new byte[6];
				while (true) {
					r.NextBytes(randoms);

					var newEp0 = ep0;
					newEp0.RawR = newEp0.RawR + ((randoms[0] >> 6) - 2);
					newEp0.RawG = newEp0.RawG + ((randoms[1] >> 5) - 4);
					newEp0.RawB = newEp0.RawB + ((randoms[2] >> 6) - 2);

					yield return (newEp0, ep1);

					var newEp1 = ep1;
					newEp1.RawR = newEp1.RawR - ((randoms[3] >> 6) - 2);
					newEp1.RawG = newEp1.RawG - ((randoms[4] >> 5) - 4);
					newEp1.RawB = newEp1.RawB - ((randoms[5] >> 6) - 2);

					yield return (ep0, newEp1);

					yield return (newEp0, newEp1);
				}
			}

			internal static Bc1Block EncodeBlock(RawBlock4X4Rgba32 rawBlock)
			{
				var pixels = rawBlock.AsSpan;

				bool hasAlpha = rawBlock.HasTransparentPixels();

				RgbBoundingBox.Create565(pixels, out var min, out var max);

				float bestError = 999;
				Bc1Block best = default;
				int tries = 0;
				foreach (var (c0, c1) in VariateEndpoints(max, min)) {
					Bc1Block block;
					float error;
					if (!hasAlpha && c0.data < c1.data) {
						block = TryColors(rawBlock, c1, c0, out error);
					}
					else if(!hasAlpha) {
						block = TryColors(rawBlock, c0, c1, out error);

					}else if (c0.data > c1.data) {
						block = TryColors(rawBlock, c1, c0, out error);
					}
					else {
						block = TryColors(rawBlock, c0, c1, out error);
					}

					if (error < bestError)
					{
						best = block;
						bestError = error;
					}
					
					tries++;
					if (tries > maxTries || bestError < errorThreshold)
					{
						break;
					}
				}

				return best;
			}
		}
		
		#endregion
	
	}
}
