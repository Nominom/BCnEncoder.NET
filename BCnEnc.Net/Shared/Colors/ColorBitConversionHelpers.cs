using System;
using System.Numerics;
using System.Runtime.CompilerServices;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SaturateSigned(float v)
        {
            if (float.IsNaN(v))
            {
                return 0;
            }
            return Math.Min(Math.Max(v, -1), 1);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FloatToSnorm(float value, int bits)
		{
			return (int)(SaturateSigned(value) * (MathF.Pow(2, bits - 1) - 1) + 0.5f);
		}


        /// <summary>
        /// Converts an unsigned normalized integer to its floating-point value in the range [0, 1].
        /// </summary>
        /// <param name="value">The unsigned normalized integer value.</param>
        /// <param name="bits">The number of bits used to represent the unsigned normalized integer.</param>
        /// <returns>The floating-point value of the unsigned normalized integer.</returns>
        /// <remarks>
        /// This function is the inverse of <see cref="FloatToUnorm"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float UnormToFloat(uint value, int bits)
		{
			return value * (1.0f / (MathF.Pow(2, bits) - 1));
		}

        /// <summary>
        /// Converts a floating-point value in the range [0, 1] to its unsigned normalized integer representation.
        /// </summary>
        /// <param name="value">The floating-point value to convert.</param>
        /// <param name="bits">The number of bits used to represent the unsigned normalized integer.</param>
        /// <returns>The unsigned normalized integer representation of the floating-point value.</returns>
        /// <remarks>
        /// This function is the inverse of <see cref="UnormToFloat"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint FloatToUnorm(float value, int bits)
		{
			return (uint)(Saturate(value) * (MathF.Pow(2, bits) - 1) + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm2ToFloat(uint value)
		{
			// (2 << 1) == MathF.Pow(2, 2)
			const float mult = 1.0f / ((2 << 1) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint FloatToUnorm2(float value)
		{
			// (2 << 1) == MathF.Pow(2, 2)
			const float mult = ((2 << 1) - 1);
			return (uint)(Saturate(value) * mult + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm4ToFloat(uint value)
		{
			// (2 << 3) == MathF.Pow(2, 4)
			const float mult = 1.0f / ((2 << 3) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint FloatToUnorm4(float value)
		{
			// (2 << 3) == MathF.Pow(2, 4)
			const float mult = ((2 << 3) - 1);
			return (uint)(Saturate(value) * mult + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm5ToFloat(uint value)
		{
			// (2 << 4) == MathF.Pow(2, 5)
			const float mult = 1.0f / ((2 << 4) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint FloatToUnorm5(float value)
		{
			// (2 << 4) == MathF.Pow(2, 5)
			const float mult = ((2 << 4) - 1);
			return (uint)(Saturate(value) * mult + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm6ToFloat(uint value)
		{
			// (2 << 5) == MathF.Pow(2, 6)
			const float mult = 1.0f / ((2 << 5) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint FloatToUnorm6(float value)
		{
			// (2 << 5) == MathF.Pow(2, 6)
			const float mult = ((2 << 5) - 1);
			return (uint)(Saturate(value) * mult + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm8ToFloat(uint value)
		{
			// (2 << 7) == MathF.Pow(2, 8)
			const float mult = 1.0f / ((2 << 7) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte FloatToUnorm8(float value)
		{
			// (2 << 7) == MathF.Pow(2, 8)
			const float mult = ((2 << 7) - 1);
			return (byte)(Saturate(value) * mult + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm10ToFloat(uint value)
		{
			// (2 << 9) == MathF.Pow(2, 10)
			const float mult = 1.0f / ((2 << 9) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint FloatToUnorm10(float value)
		{
			// (2 << 9) == MathF.Pow(2, 10)
			const float mult = ((2 << 9) - 1);
			return (uint)(Saturate(value) * mult + 0.5f);
		}
    }
}
