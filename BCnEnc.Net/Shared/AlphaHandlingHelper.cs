using System;
using BCnEncoder.Encoder.Options;
using BCnEncoder.Shared.Colors;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Shared
{
    /// <summary>
    /// Helper class for handling alpha premultiplication detection and conversion.
    /// </summary>
    internal static class AlphaHandlingHelper
    {
        private const float AlphaEpsilon = 0.0001f;

        /// <summary>
        /// Detects if an array of ColorRgbaFloat values is already using premultiplied alpha.
        /// </summary>
        /// <param name="colors">The array of colors to check.</param>
        /// <returns><see cref="AlphaChannelHint.Premultiplied"/> or <see cref="AlphaChannelHint.Straight"/></returns>
        public static AlphaChannelHint GuessAlphaChannel(ReadOnlySpan<ColorRgbaFloat> colors)
        {
            // Start by assuming it's premultiplied until we find evidence otherwise
            for (int i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                // If any color channel exceeds alpha value (plus a small epsilon for float precision),
                // then it's using straight alpha, not premultiplied
                if (color.a > AlphaEpsilon && (color.r > color.a + AlphaEpsilon ||
                                             color.g > color.a + AlphaEpsilon ||
                                             color.b > color.a + AlphaEpsilon))
                {
                    return AlphaChannelHint.Straight;
                }
            }

            // If we didn't find any evidence to the contrary, consider it premultiplied
            return AlphaChannelHint.Premultiplied;
        }

        public static AlphaChannelHint GuessAlphaChannel(BCnTextureData texture)
        {
	        if (texture.Format != CompressionFormat.RgbaFloat)
		        throw new ArgumentException("Format must be RgbaFloat before calling this method.");

	        bool isPremultiplied = true;

	        for (var m = 0; m < texture.NumMips && isPremultiplied; m++)
	        {
		        for (var f = 0; f < texture.NumFaces && isPremultiplied; f++)
		        {
			        for (var a = 0; a < texture.NumArrayElements && isPremultiplied; a++)
			        {
				        var face = (CubeMapFaceDirection)f;
				        var channelHint = GuessAlphaChannel(texture.Mips[m][face, a].AsMemory<ColorRgbaFloat>().Span);

				        if (channelHint == AlphaChannelHint.Straight)
					        isPremultiplied = false;
			        }
		        }
	        }

	        return isPremultiplied ? AlphaChannelHint.Premultiplied : AlphaChannelHint.Straight;
        }

        /// <summary>
        /// Premultiplies the alpha of an array of ColorRgbaFloat values.
        /// </summary>
        /// <param name="colors">The colors to premultiply.</param>
        public static void PremultiplyAlpha(Span<ColorRgbaFloat> colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                colors[i] = new ColorRgbaFloat(
                    color.r * color.a,
                    color.g * color.a,
                    color.b * color.a,
                    color.a);
            }
        }

        /// <summary>
        /// Unpremultiplies the alpha of an array of ColorRgbaFloat values.
        /// </summary>
        /// <param name="colors">The colors to unpremultiply.</param>
        public static void UnpremultiplyAlpha(Span<ColorRgbaFloat> colors)
        {
	        static float Unpremultiply(float c, float a) => a > 0 ?  Math.Min(1.0f, c / a): 0;

            for (int i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                if (color.a > AlphaEpsilon)
                {
                    colors[i] = new ColorRgbaFloat(
                        Unpremultiply(color.r, color.a),
                        Unpremultiply(color.g, color.a),
                        Unpremultiply(color.b, color.a),
                        color.a);
                }
                else
                {
                    colors[i] = new ColorRgbaFloat(0, 0, 0, 0);
                }
            }
        }

        /// <summary>
        /// Handles alpha processing for a TextureData based on the specified AlphaHandling mode.
        /// </summary>
        /// <param name="textureData">The texture data to process.</param>
        /// <param name="alphaHandling">The alpha handling mode to apply.</param>
        /// <returns>True if the data was modified, false otherwise.</returns>
        public static bool ProcessAlpha(BCnTextureData textureData, AlphaHandling alphaHandling)
        {
            if (textureData.Mips.Length == 0)
            {
                return false;
            }

            if (textureData.AlphaChannelHint == AlphaChannelHint.Unknown)
            {
	            textureData.AlphaChannelHint = GuessAlphaChannel(textureData);
            }

            bool modified = false;

            if (alphaHandling == AlphaHandling.Auto)
            {
                // If not premultiplied, convert to premultiplied
                if (textureData.AlphaChannelHint == AlphaChannelHint.Straight)
                {
	                for (var m = 0; m < textureData.NumMips; m++)
	                {
		                for (var f = 0; f < textureData.NumFaces; f++)
		                {
			                for (var a = 0; a < textureData.NumArrayElements; a++)
			                {
				                var face = (CubeMapFaceDirection)f;
				                var pixels = textureData.Mips[m][face, a].AsMemory<ColorRgbaFloat>();
				                PremultiplyAlpha(pixels.Span);
			                }
		                }
	                }
	                modified = true;
                }
            }
            else if (alphaHandling == AlphaHandling.LinearToPremultiplied)
            {
                // Always convert to premultiplied
                for (var f = 0; f < textureData.NumFaces; f++)
                {
                    for (var a = 0; a < textureData.NumArrayElements; a++)
                    {
                        var face = (CubeMapFaceDirection)f;
                        var pixels = textureData.Mips[0][face, a].AsMemory<ColorRgbaFloat>();
                        PremultiplyAlpha(pixels.Span);
                    }
                }

                textureData.AlphaChannelHint = AlphaChannelHint.Premultiplied;
                modified = true;
            }

            return modified;
        }

        /// <summary>
        /// Handles alpha processing for a single memory buffer based on the specified AlphaHandling mode.
        /// </summary>
        /// <param name="floatData">The color data to process.</param>
        /// <param name="alphaHandling">The alpha handling mode to apply.</param>
        /// <param name="alphaChannelHint">Optional hint about how the alpha is currently encoded. If Unknown, will attempt to detect.</param>
        /// <returns>A tuple containing the processed data (which may be the same as the input if no processing was needed) and the resulting alpha channel hint.</returns>
        public static (ReadOnlyMemory<ColorRgbaFloat> data, AlphaChannelHint alphaChannelHint) ProcessAlpha(
            ReadOnlyMemory<ColorRgbaFloat> floatData, AlphaHandling alphaHandling, AlphaChannelHint alphaChannelHint = AlphaChannelHint.Unknown)
        {
            if (floatData.Length == 0)
            {
                return (floatData, alphaChannelHint);
            }

            // Determine alpha channel type if unknown
            if (alphaChannelHint == AlphaChannelHint.Unknown)
            {
                alphaChannelHint = GuessAlphaChannel(floatData.Span);
            }

            if (alphaHandling == AlphaHandling.Auto)
            {
                // Convert to premultiplied if it's straight alpha
                if (alphaChannelHint == AlphaChannelHint.Straight)
                {
                    var rgbaPreMul = new ColorRgbaFloat[floatData.Length];
                    floatData.CopyTo(rgbaPreMul);
                    PremultiplyAlpha(rgbaPreMul);
                    return (new ReadOnlyMemory<ColorRgbaFloat>(rgbaPreMul), AlphaChannelHint.Premultiplied);
                }
            }
            else if (alphaHandling == AlphaHandling.LinearToPremultiplied)
            {
                // Always convert to premultiplied
                var rgbaPreMul = new ColorRgbaFloat[floatData.Length];
                floatData.CopyTo(rgbaPreMul);
                PremultiplyAlpha(rgbaPreMul);
                return (new ReadOnlyMemory<ColorRgbaFloat>(rgbaPreMul), AlphaChannelHint.Premultiplied);
            }

            // For AsIs or already premultiplied data, return the original
            return (floatData, alphaChannelHint);
        }
    }
}
