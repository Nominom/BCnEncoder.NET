using System;
using System.Diagnostics;
using System.Numerics;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder
{
	internal class Bc1BlockEncoder : BaseBcBlockEncoder<Bc1Block, RgbEncodingContext>
	{
		public bool UseColorModeSwitch { get; private set; }
		public bool BlackModeIsTransparent { get; private set; }

		public Bc1BlockEncoder(bool useColorModeSwitch, bool blackModeIsTransparent)
		{
			UseColorModeSwitch = useColorModeSwitch;
			BlackModeIsTransparent = blackModeIsTransparent;

			if (BlackModeIsTransparent && !UseColorModeSwitch)
				throw new ArgumentException("BlackModeIsTransparent is only valid when UseColorModeSwitch is true.");
		}

		public override Bc1Block EncodeBlock(in RgbEncodingContext context)
		{
			return EncodeBlockImpl(context);
		}

		#region Encoding private stuff
		private Bc1Block EncodeBlockImpl(in RgbEncodingContext context)
		{
			Bc1Block initialBlock = GetInitialBlock(context);

			// TODO: Optimize endpoints

			return initialBlock;
		}

		private Bc1Block GetInitialBlock(in RgbEncodingContext context)
		{
			int blacks = 0;
			bool hasTransparent = BlackModeIsTransparent && context.RawBlock.HasTransparentPixels(context.AlphaThreshold);
			Vector4 mean, pa;
			Vector4 min, max;

			PcaVectors2.Create(context.PerceptualBlock.AsSpan, context.Weights, out mean, out pa, out min, out max);

			var (c0, c1) = QuantizeColors(context, false, min, max);

			Bc1Block noBlacksBlock = TryColors(context, c0, c1, out float noBlacksError);

			if (UseColorModeSwitch)
			{
				if (BlackModeIsTransparent)
					blacks = PcaVectors2.CreateIgnoreTransparent(context.PerceptualBlock.AsSpan, context.Weights, context.AlphaThreshold, out mean, out pa, out min, out max);
				else
					blacks = PcaVectors2.CreateIgnoreBlack(context.PerceptualBlock.AsSpan, context.Weights, out mean, out pa, out min, out max);

				var (c0b, c1b) = QuantizeColors(context, true, min, max);
				Bc1Block blacksBlock = TryColors(context, c0b, c1b, out float blacksError);

				if (hasTransparent || blacksError < noBlacksError)
				{
					return blacksBlock;
				}
			}

			return noBlacksBlock;
		}

		private static (ColorB5G6R5Packed, ColorB5G6R5Packed) QuantizeColors(in RgbEncodingContext context,
			bool useBlackMode, Vector4 min, Vector4 max)
		{
			// Convert to sRGB
			if (context.ColorConversionMode == ColorConversionMode.LinearToSrgb)
			{
				min = ColorSpace.Srgb.ToSrgb(min);
				max = ColorSpace.Srgb.ToSrgb(max);
			}

			var color0 = new ColorB5G6R5Packed(min);
			var color1 = new ColorB5G6R5Packed(max);

			// Color0 <= Color1 means block uses alpha or black pixels in index 3
			return useBlackMode switch
			{
				false when color0.data < color1.data => (color1, color0),
				true when color0.data > color1.data => (color1, color0),
				_ => (color0, color1)
			};
		}

		private Bc1Block TryColors(in RgbEncodingContext context, ColorB5G6R5Packed color0, ColorB5G6R5Packed color1, out float error)
		{
			var output = new Bc1Block();

			// Reference pixels in perceptual space
			var referencePixels = context.PerceptualBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			// Quantized colors in final space
			var c0 = color0.ToColorRgbaFloat().ToVector4();
			var c1 = color1.ToColorRgbaFloat().ToVector4();

			bool hasAlphaOrBlack = output.HasAlphaOrBlack;

			Span<Vector4> colors = hasAlphaOrBlack ?
				stackalloc Vector4[] {
					c0,
					c1,
					c0.InterpolateHalf(c1),
					BlackModeIsTransparent ? Vector4.Zero : new Vector4(0, 0, 0, 1)
				} : stackalloc Vector4[] {
					c0,
					c1,
					c0.InterpolateThird(c1, 1),
					c0.InterpolateThird(c1, 2)
				};

			// Convert from sRGB back to linear (Inverted if here is intended)
			if (context.ColorConversionMode == ColorConversionMode.LinearToSrgb)
			{
				colors[0] = ColorSpace.Srgb.ToLrgb(colors[0]);
				colors[1] = ColorSpace.Srgb.ToLrgb(colors[1]);
				colors[2] = ColorSpace.Srgb.ToLrgb(colors[2]);
				colors[3] = ColorSpace.Srgb.ToLrgb(colors[3]);
			}

			// Convert to perceptual
			colors[0] = context.Weights.TransformToPerceptual(colors[0]);
			colors[1] = context.Weights.TransformToPerceptual(colors[1]);
			colors[2] = context.Weights.TransformToPerceptual(colors[2]);
			colors[3] = context.Weights.TransformToPerceptual(colors[3]);

			error = 0;
			for (var i = 0; i < 16; i++)
			{
				var color = referencePixels[i];
				float e;
				if (BlackModeIsTransparent)
					output[i] = ColorChooser.ChooseClosestRgbColor4AlphaCutoff(colors, color, context.Weights, context.AlphaThreshold, hasAlphaOrBlack, out e);
				else
					output[i] = ColorChooser.ChooseClosestRgbColor4(colors, color, context.Weights, out e);
				error += e;
			}

			return output;
		}
		#endregion

		#region Encoders
		//
		// internal static class Bc1BlockEncoderFast
		// {
		// 	internal static Bc1Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock, OperationContext context, bool useColorModeSwitch)
		// 	{
		// 		var pixels = rawBlock.AsSpan;
		//
		// 		int blacks = 0;
		// 		Vector4 mean, pa;
		// 		ColorB5G6R5Packed min, max;
		// 		if (useColorModeSwitch)
		// 		{
		// 			blacks = PcaVectors2.CreateIgnoreBlacks(pixels, out mean, out pa, context.Weights, out min, out max);
		// 		}
		// 		else
		// 		{
		// 			PcaVectors2.Create(pixels, out  mean, out pa, context.Weights, out min, out max);
		// 		}
		//
		// 		// PcaVectors2.GetMinMaxColor565(pixels, mean, pa, out min, out max);
		//
		// 		var c0 = max;
		// 		var c1 = min;
		//
		// 		(c0, c1) = useColorModeSwitch switch
		// 		{
		// 			true when c0.data < c1.data && blacks == 0 => (c1, c0),
		// 			true when c0.data > c1.data && blacks > 0 => (c1, c0),
		// 			false when c0.data < c1.data => (c1, c0),
		// 			_ => (c0, c1)
		// 		};
		//
		// 		var best = TryColors(rawBlock, c0, c1, useColorModeSwitch, out var bestError, context.Weights);
		//
		// 		return best;
		// 	}
		// }
		//
		// internal static class Bc1BlockEncoderBalanced
		// {
		// 	private const int MaxTries = 24 * 2;
		// 	private const float ErrorThreshold = 0.05f;
		//
		// 	internal static Bc1Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock, OperationContext context, bool useColorModeSwitch)
		// 	{
		// 		var pixels = rawBlock.AsSpan;
		//
		// 		int blacks = 0;
		// 		Vector4 mean, pa;
		//
		// 		PcaVectors2.Create(pixels, out mean, out pa, context.Weights, out var ep0, out var ep1);
		// 		// PcaVectors2.GetMinMaxColor565(pixels, mean, pa, out var ep0, out var ep1);
		// 		EndpointOptimizer.Optimize565Endpoints(pixels, mean, pa, ref ep0, ref ep1, context.Weights, false);
		//
		// 		if (ep0.data < ep1.data) // swap endpoints
		// 			(ep0, ep1) = (ep1, ep0);
		//
		// 		var noBlacksBlock = TryColors(rawBlock, ep0, ep1, true, out var noBlacksError, context.Weights);
		//
		// 		if (useColorModeSwitch)
		// 		{
		// 			blacks = PcaVectors2.CreateIgnoreBlacks(pixels, out mean, out pa, context.Weights, out ep0, out ep1);
		// 			//
		// 			// if (blacks == 0)
		// 			// 	return noBlacksBlock;
		//
		// 			// PcaVectors2.GetMinMaxColor565(pixels, mean, pa, out ep0, out ep1);
		// 			EndpointOptimizer.Optimize565Endpoints(pixels, mean, pa, ref ep0, ref ep1, context.Weights, true);
		//
		// 			if (ep0.data > ep1.data) // swap endpoints
		// 				(ep0, ep1) = (ep1, ep0);
		//
		// 			var useBlacksBlock = TryColors(rawBlock, ep0, ep1, true, out var useBlacksError, context.Weights);
		//
		// 			if (useBlacksError < noBlacksError)
		// 				return useBlacksBlock;
		// 		}
		//
		// 		return noBlacksBlock;
		// 	}
		// }
		//
		// internal static class Bc1BlockEncoderSlow
		// {
		// 	private const int MaxTries = 9999;
		// 	private const float ErrorThreshold = 0.01f;
		//
		// 	internal static Bc1Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock, OperationContext context, bool useColorModeSwitch)
		// 	{
		// 		var pixels = rawBlock.AsSpan;
		//
		// 		int blacks = 0;
		// 		Vector4 mean, pa;
		//
		// 		if (useColorModeSwitch)
		// 		{
		// 			blacks = PcaVectors.CreateIgnoreBlacks(pixels, out mean, out pa, context.Weights);
		// 		}
		// 		else
		// 		{
		// 			PcaVectors.Create(pixels, out  mean, out pa, context.Weights);
		// 		}
		//
		// 		PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);
		//
		// 		var c0 = max;
		// 		var c1 = min;
		//
		// 		(c0, c1) = useColorModeSwitch switch
		// 		{
		// 			true when c0.data < c1.data && blacks == 0 => (c1, c0),
		// 			true when c0.data > c1.data && blacks > 0 => (c1, c0),
		// 			false when c0.data < c1.data => (c1, c0),
		// 			_ => (c0, c1)
		// 		};
		//
		// 		var best = TryColors(rawBlock, c0, c1, useColorModeSwitch, out var bestError, context.Weights);
		//
		// 		var lastChanged = 0;
		//
		// 		for (var i = 0; i < MaxTries; i++)
		// 		{
		// 			var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);
		//
		// 			(newC0, newC1) = useColorModeSwitch switch
		// 			{
		// 				true when newC0.data < newC1.data && blacks == 0 => (newC1, newC0),
		// 				true when newC0.data > newC1.data && blacks > 0 => (newC1, newC0),
		// 				false when newC0.data < newC1.data => (newC1, newC0),
		// 				_ => (newC0, newC1)
		// 			};
		//
		// 			var block = TryColors(rawBlock, newC0, newC1, useColorModeSwitch, out var error, context.Weights);
		//
		// 			lastChanged++;
		//
		// 			if (error < bestError)
		// 			{
		// 				best = block;
		// 				bestError = error;
		// 				c0 = newC0;
		// 				c1 = newC1;
		// 				lastChanged = 0;
		// 			}
		//
		// 			if (bestError < ErrorThreshold || lastChanged > ColorVariationGenerator.VarPatternCount)
		// 			{
		// 				break;
		// 			}
		// 		}
		//
		// 		return best;
		// 	}
		// }

		#endregion
	}

	internal class Bc1AlphaBlockEncoder : BaseBcBlockEncoder<Bc1Block, RgbEncodingContext>
	{
		public override Bc1Block EncodeBlock(in RgbEncodingContext context)
		{
			bool hasAlpha = context.RawBlockVec.HasTransparentPixels(context.AlphaThreshold);

			Vector4 mean, pa;
			Vector4 min, max;

			if (hasAlpha)
			{
				PcaVectors2.CreateIgnoreTransparent(context.PerceptualBlock.AsSpan, context.Weights, context.AlphaThreshold,
					out mean, out pa, out min, out max);
			}
			else
			{
				PcaVectors2.Create(context.PerceptualBlock.AsSpan, context.Weights, out mean, out pa, out min, out max);
			}

			var (c0, c1) = QuantizeColors(context, hasAlpha, min, max);

			return TryColors(context, c0, c1, out float bestError);
		}

		#region Encoding private stuff

		private static (ColorB5G6R5Packed, ColorB5G6R5Packed) QuantizeColors(in RgbEncodingContext context,
			bool useTransparentMode, Vector4 min, Vector4 max)
		{
			// Convert to sRGB
			if (context.ColorConversionMode == ColorConversionMode.LinearToSrgb)
			{
				min = ColorSpace.Srgb.ToSrgb(min);
				max = ColorSpace.Srgb.ToSrgb(max);
			}

			var color0 = new ColorB5G6R5Packed(min);
			var color1 = new ColorB5G6R5Packed(max);

			// Color0 <= Color1 means block uses alpha or black pixels in index 3
			return useTransparentMode switch
			{
				false when color0.data < color1.data => (color1, color0),
				true when color0.data > color1.data => (color1, color0),
				_ => (color0, color1)
			};
		}

		private static Bc1Block TryColors(in RgbEncodingContext context, ColorB5G6R5Packed color0, ColorB5G6R5Packed color1, out float error)
		{
			var output = new Bc1Block();

			// Reference pixels in perceptual space
			var referencePixels = context.PerceptualBlock.AsSpan;

			output.color0 = color0;
			output.color1 = color1;

			// Quantized colors in final space
			var c0 = color0.ToColorRgbaFloat().ToVector4();
			var c1 = color1.ToColorRgbaFloat().ToVector4();

			bool hasAlpha = output.HasAlphaOrBlack;

			Span<Vector4> colors = hasAlpha ?
				stackalloc Vector4[] {
					c0,
					c1,
					c0.InterpolateHalf(c1),
					new Vector4(0, 0, 0, 0)
				} : stackalloc Vector4[] {
					c0,
					c1,
					c0.InterpolateThird(c1, 1),
					c0.InterpolateThird(c1, 2)
				};

			// Convert from sRGB back to linear (Inverted if here is intended)
			if (context.ColorConversionMode == ColorConversionMode.LinearToSrgb)
			{
				colors[0] = ColorSpace.Srgb.ToLrgb(colors[0]);
				colors[1] = ColorSpace.Srgb.ToLrgb(colors[1]);
				colors[2] = ColorSpace.Srgb.ToLrgb(colors[2]);
				colors[3] = ColorSpace.Srgb.ToLrgb(colors[3]);
			}

			// Convert to perceptual
			colors[0] = context.Weights.TransformToPerceptual(colors[0]);
			colors[1] = context.Weights.TransformToPerceptual(colors[1]);
			colors[2] = context.Weights.TransformToPerceptual(colors[2]);
			colors[3] = context.Weights.TransformToPerceptual(colors[3]);

			error = 0;
			for (var i = 0; i < 16; i++)
			{
				var color = referencePixels[i];
				output[i] = ColorChooser.ChooseClosestRgbColor4AlphaCutoff(colors, color, context.Weights, context.AlphaThreshold, hasAlpha, out var e);
				error += e;
			}

			return output;
		}

		#endregion

		#region Encoders

		// private static class Bc1AlphaBlockEncoderFast
		// {
		// 	internal static Bc1Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock, OperationContext context)
		// 	{
		// 		var pixels = rawBlock.AsSpan;
		//
		// 		var hasAlpha = rawBlock.HasTransparentPixels(context.AlphaThreshold);
		//
		// 		Vector4 mean, pa;
		//
		// 		if (hasAlpha)
		// 		{
		// 			PcaVectors.CreateIgnoreTransparent(pixels, out mean, out pa, context.AlphaThreshold, context.Weights);
		// 		}
		// 		else
		// 		{
		// 			PcaVectors.Create(pixels, out mean, out pa, context.Weights);
		// 		}
		//
		// 		PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);
		//
		// 		var c0 = max;
		// 		var c1 = min;
		//
		// 		if (!hasAlpha && c0.data < c1.data)
		// 		{
		// 			var c = c0;
		// 			c0 = c1;
		// 			c1 = c;
		// 		}
		// 		else if (hasAlpha && c1.data < c0.data)
		// 		{
		// 			var c = c0;
		// 			c0 = c1;
		// 			c1 = c;
		// 		}
		//
		// 		var best = TryColors(rawBlock, c0, c1, out var bestError, context);
		//
		// 		return best;
		// 	}
		// }
		//
		// private static class Bc1AlphaBlockEncoderBalanced
		// {
		// 	private const int MaxTries = 24 * 2;
		// 	private const float ErrorThreshold = 0.05f;
		//
		//
		// 	internal static Bc1Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock, OperationContext context)
		// 	{
		// 		var pixels = rawBlock.AsSpan;
		//
		// 		var hasAlpha = rawBlock.HasTransparentPixels(context.AlphaThreshold);
		//
		// 		Vector4 mean, pa;
		//
		// 		if (hasAlpha)
		// 		{
		// 			PcaVectors.CreateIgnoreTransparent(pixels, out mean, out pa, context.AlphaThreshold, context.Weights);
		// 		}
		// 		else
		// 		{
		// 			PcaVectors.Create(pixels, out mean, out pa, context.Weights);
		// 		}
		//
		// 		PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);
		//
		// 		var c0 = max;
		// 		var c1 = min;
		//
		// 		if (!hasAlpha && c0.data < c1.data)
		// 		{
		// 			var c = c0;
		// 			c0 = c1;
		// 			c1 = c;
		// 		}
		// 		else if (hasAlpha && c1.data < c0.data)
		// 		{
		// 			var c = c0;
		// 			c0 = c1;
		// 			c1 = c;
		// 		}
		//
		// 		var best = TryColors(rawBlock, c0, c1, out var bestError, context);
		//
		// 		for (var i = 0; i < MaxTries; i++)
		// 		{
		// 			var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);
		//
		// 			if (!hasAlpha && newC0.data < newC1.data)
		// 			{
		// 				var c = newC0;
		// 				newC0 = newC1;
		// 				newC1 = c;
		// 			}
		// 			else if (hasAlpha && newC1.data < newC0.data)
		// 			{
		// 				var c = newC0;
		// 				newC0 = newC1;
		// 				newC1 = c;
		// 			}
		//
		// 			var block = TryColors(rawBlock, newC0, newC1, out var error, context);
		//
		// 			if (error < bestError)
		// 			{
		// 				best = block;
		// 				bestError = error;
		// 				c0 = newC0;
		// 				c1 = newC1;
		// 			}
		//
		// 			if (bestError < ErrorThreshold)
		// 			{
		// 				break;
		// 			}
		// 		}
		//
		// 		return best;
		// 	}
		// }
		//
		// private static class Bc1AlphaBlockEncoderSlow
		// {
		// 	private const int MaxTries = 9999;
		// 	private const float ErrorThreshold = 0.05f;
		//
		// 	internal static Bc1Block EncodeBlock(RawBlock4X4RgbaFloat rawBlock, OperationContext context)
		// 	{
		// 		var pixels = rawBlock.AsSpan;
		//
		// 		var hasAlpha = rawBlock.HasTransparentPixels(context.AlphaThreshold);
		//
		// 		Vector4 mean, pa;
		//
		// 		if (hasAlpha)
		// 		{
		// 			PcaVectors.CreateIgnoreTransparent(pixels, out mean, out pa, context.AlphaThreshold, context.Weights);
		// 		}
		// 		else
		// 		{
		// 			PcaVectors.Create(pixels, out mean, out pa, context.Weights);
		// 		}
		//
		// 		PcaVectors.GetMinMaxColor565(pixels, mean, pa, out var min, out var max);
		//
		// 		var c0 = max;
		// 		var c1 = min;
		//
		// 		if (!hasAlpha && c0.data < c1.data)
		// 		{
		// 			var c = c0;
		// 			c0 = c1;
		// 			c1 = c;
		// 		}
		// 		else if (hasAlpha && c1.data < c0.data)
		// 		{
		// 			var c = c0;
		// 			c0 = c1;
		// 			c1 = c;
		// 		}
		//
		// 		var best = TryColors(rawBlock, c0, c1, out var bestError, context);
		//
		// 		var lastChanged = 0;
		// 		for (var i = 0; i < MaxTries; i++)
		// 		{
		// 			var (newC0, newC1) = ColorVariationGenerator.Variate565(c0, c1, i);
		//
		// 			if (!hasAlpha && newC0.data < newC1.data)
		// 			{
		// 				var c = newC0;
		// 				newC0 = newC1;
		// 				newC1 = c;
		// 			}
		// 			else if (hasAlpha && newC1.data < newC0.data)
		// 			{
		// 				var c = newC0;
		// 				newC0 = newC1;
		// 				newC1 = c;
		// 			}
		//
		// 			var block = TryColors(rawBlock, newC0, newC1, out var error, context);
		//
		// 			lastChanged++;
		//
		// 			if (error < bestError)
		// 			{
		// 				best = block;
		// 				bestError = error;
		// 				c0 = newC0;
		// 				c1 = newC1;
		// 				lastChanged = 0;
		// 			}
		//
		// 			if (bestError < ErrorThreshold || lastChanged > ColorVariationGenerator.VarPatternCount)
		// 			{
		// 				break;
		// 			}
		// 		}
		//
		// 		return best;
		// 	}
		// }

		#endregion

	}
}
