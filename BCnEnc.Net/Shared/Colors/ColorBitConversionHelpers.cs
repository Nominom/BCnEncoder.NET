using System;
using System.Numerics;

namespace BCnEncoder.Shared.Colors
{
    /// <summary>
    /// Provides helper methods for converting between different color formats.
    /// </summary>
    internal static class ColorBitConversionHelpers
    {
        #region Helper Methods
        /// <summary>
        /// Saturates a float value between 0 and 1
        /// </summary>
        public static float Saturate(float v)
        {
            if (float.IsNaN(v))
            {
                return 0;
            }
            return Math.Min(Math.Max(v, 0), 1);
        }

        /// <summary>
        /// Saturates a float value between -1 and 1
        /// </summary>
        public static float SaturateSigned(float v)
        {
            if (float.IsNaN(v))
            {
                return 0;
            }
            return Math.Min(Math.Max(v, -1), 1);
        }

        /// <summary>
        /// Converts float to uint with scaling
        /// </summary>
        public static uint FloatToUint(float v, float scale)
        {
            return (uint)MathF.Floor(v * scale + 0.5f);
        }

        /// <summary>
        /// Converts int to float with scaling
        /// </summary>
        public static float IntToFloat(int v, float scale)
        {
            float scaled = v / scale;
            // The integer is a two's-complement signed
            // number so the negative range is slightly
            // larger than the positive range, meaning
            // the scaled value can be slight less than -1.
            // Clamp to keep the float range [-1, 1].
            return Math.Max(scaled, -1.0f);
        }

        /// <summary>
        /// Converts float to int with scaling
        /// </summary>
        public static int FloatToInt(float v, float scale)
        {
            return (int)MathF.Truncate(v * scale + (v >= 0 ? 0.5f : -0.5f));
        }

        #endregion

        /// <summary>
        /// Converts a signed normalized integer to its floating-point value in the range [-1, 1].
        /// </summary>
        /// <param name="value">The signed normalized integer value.</param>
        /// <param name="bits">The number of bits used to represent the signed normalized integer.</param>
        /// <returns>The floating-point value of the signed normalized integer.</returns>
        /// <remarks>
        /// This function is the inverse of <see cref="FloatToSnorm"/>.
        /// </remarks>
        public static float SnormToFloat(int value, int bits)
        {
	        return MathF.Max(value / (MathF.Pow(2, bits - 1) - 1), -1.0f);
        }

        /// <summary>
        /// Converts a floating-point value in the range [-1, 1] to its signed normalized integer representation.
        /// </summary>
        /// <param name="value">The floating-point value to convert.</param>
        /// <param name="bits">The number of bits used to represent the signed normalized integer.</param>
        /// <returns>The signed normalized integer representation of the floating-point value.</returns>
        /// <remarks>
        /// This function is the inverse of <see cref="SnormToFloat"/>.
        /// </remarks>
		public static int FloatToSnorm(float value, int bits)
		{
			return (int)MathF.Round(SaturateSigned(value) * (MathF.Pow(2, bits - 1) - 1));
		}
    }
}
