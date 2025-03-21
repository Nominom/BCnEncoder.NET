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
        /// <returns>True if the data is premultiplied, false if it's using straight alpha.</returns>
        public static bool IsAlreadyPremultiplied(ReadOnlySpan<ColorRgbaFloat> colors)
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
                    return false;
                }
            }

            // If we didn't find any evidence to the contrary, consider it premultiplied
            return true;
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
            for (int i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                if (color.a > AlphaEpsilon)
                {
                    colors[i] = new ColorRgbaFloat(
                        color.r / color.a,
                        color.g / color.a,
                        color.b / color.a,
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

            bool modified = false;

            if (alphaHandling == AlphaHandling.Auto)
            {
                // First, check if the data is already premultiplied
                bool isPremultiplied = true;

                // Check the first mip level of all faces/arrays
                for (var f = 0; f < textureData.NumFaces && isPremultiplied; f++)
                {
                    for (var a = 0; a < textureData.NumArrayElements && isPremultiplied; a++)
                    {
                        var face = (CubeMapFaceDirection)f;
                        var pixels = textureData.Mips[0][face, a].AsMemory<ColorRgbaFloat>();

                        if (!IsAlreadyPremultiplied(pixels.Span))
                        {
                            isPremultiplied = false;
                            break;
                        }
                    }
                }

                // If not premultiplied, convert to premultiplied
                if (!isPremultiplied)
                {
                    for (var f = 0; f < textureData.NumFaces; f++)
                    {
                        for (var a = 0; a < textureData.NumArrayElements; a++)
                        {
                            var face = (CubeMapFaceDirection)f;
                            var pixels = textureData.Mips[0][face, a].AsMemory<ColorRgbaFloat>();
                            PremultiplyAlpha(pixels.Span);
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
                modified = true;
            }

            return modified;
        }

        /// <summary>
        /// Handles alpha processing for a single memory buffer based on the specified AlphaHandling mode.
        /// </summary>
        /// <param name="floatData">The color data to process.</param>
        /// <param name="alphaHandling">The alpha handling mode to apply.</param>
        /// <returns>The processed data, which may be the same as the input if no processing was needed.</returns>
        public static ReadOnlyMemory<ColorRgbaFloat> ProcessAlpha(
            ReadOnlyMemory<ColorRgbaFloat> floatData, AlphaHandling alphaHandling)
        {
            if (floatData.Length == 0)
            {
                return floatData;
            }

            if (alphaHandling == AlphaHandling.Auto)
            {
                // Check if the data is already premultiplied
                if (!IsAlreadyPremultiplied(floatData.Span))
                {
                    // Not premultiplied, so convert
                    var rgbaPreMul = new ColorRgbaFloat[floatData.Length];
                    floatData.CopyTo(rgbaPreMul);
                    PremultiplyAlpha(rgbaPreMul);
                    return new ReadOnlyMemory<ColorRgbaFloat>(rgbaPreMul);
                }
            }
            else if (alphaHandling == AlphaHandling.LinearToPremultiplied)
            {
                // Always convert to premultiplied
                var rgbaPreMul = new ColorRgbaFloat[floatData.Length];
                floatData.CopyTo(rgbaPreMul);
                PremultiplyAlpha(rgbaPreMul);
                return new ReadOnlyMemory<ColorRgbaFloat>(rgbaPreMul);
            }

            // For AsIs or already premultiplied data, return the original
            return floatData;
        }
    }
}
