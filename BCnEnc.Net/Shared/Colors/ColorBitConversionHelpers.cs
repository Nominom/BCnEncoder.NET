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
	        // (1 << (bits - 1)) == MathF.Pow(2, bits - 1)
	        float mult = 1.0f / ((1 << (bits - 1)) - 1);
	        return MathF.Max(value * mult, -1.0f);
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
			// (1 << (bits - 1)) == MathF.Pow(2, bits - 1)
			float mult = ((1 << (bits - 1)) - 1);
			return (int)MathF.Round(SaturateSigned(value) * mult);
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
		public static float Unorm1ToFloat(uint value)
		{
			// We don't need to do any scaling here
			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte FloatToUnorm1(float value)
		{
			// Only do saturation and rounding
			return (byte)(Saturate(value) + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm2ToFloat(uint value)
		{
			// (1 << 2) == MathF.Pow(2, 2)
			const float mult = 1.0f / ((1 << 2) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte FloatToUnorm2(float value)
		{
			// (1 << 2) == MathF.Pow(2, 2)
			const float mult = ((1 << 2) - 1);
			return (byte)(Saturate(value) * mult + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm4ToFloat(uint value)
		{
			// (1 << 4) == MathF.Pow(2, 4)
			const float mult = 1.0f / ((1 << 4) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte FloatToUnorm4(float value)
		{
			// (1 << 4) == MathF.Pow(2, 4)
			const float mult = ((1 << 4) - 1);
			return (byte)(Saturate(value) * mult + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm5ToFloat(uint value)
		{
			// (1 << 5) == MathF.Pow(2, 5)
			const float mult = 1.0f / ((1 << 5) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte FloatToUnorm5(float value)
		{
			// (1 << 5) == MathF.Pow(2, 5)
			const float mult = ((1 << 5) - 1);
			return (byte)(Saturate(value) * mult + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm6ToFloat(uint value)
		{
			// (1 << 6) == MathF.Pow(2, 6)
			const float mult = 1.0f / ((1 << 6) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte FloatToUnorm6(float value)
		{
			// (1 << 6) == MathF.Pow(2, 6)
			const float mult = ((1 << 6) - 1);
			return (byte)(Saturate(value) * mult + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm8ToFloat(uint value)
		{
			// (1 << 8) == MathF.Pow(2, 8)
			const float mult = 1.0f / ((1 << 8) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte FloatToUnorm8(float value)
		{
			// (1 << 8) == MathF.Pow(2, 8)
			const float mult = ((1 << 8) - 1);
			return (byte)(Saturate(value) * mult + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm10ToFloat(uint value)
		{
			// (1 << 10) == MathF.Pow(2, 10)
			const float mult = 1.0f / ((1 << 10) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort FloatToUnorm10(float value)
		{
			// (1 << 10) == MathF.Pow(2, 10)
			const float mult = ((1 << 10) - 1);
			return (ushort)(Saturate(value) * mult + 0.5f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Unorm16ToFloat(uint value)
		{
			// (1 << 16) == MathF.Pow(2, 16)
			const float mult = 1.0f / ((1 << 16) - 1);
			return value * mult;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort FloatToUnorm16(float value)
		{
			// (1 << 16) == MathF.Pow(2, 16)
			const float mult = ((1 << 16) - 1);
			return (ushort)(Saturate(value) * mult + 0.5f);
		}

        #region Special Floating Point Formats

        /// <summary>
        /// Packs a float value into an 11-bit floating point representation (5 exponent, 6 mantissa).
        /// </summary>
        /// <remarks>
        /// Conforms to the packed 11-bit floating point format as defined in the Khronos Data Format Specification 1.4.
        /// See: <a href="https://registry.khronos.org/DataFormat/specs/1.4/dataformat.1.4.html#11bitfp"/>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FloatToFloat11(float value)
        {
            // Handle special cases
            if (float.IsNaN(value)) return 0x7C0 | 1; // Exponent all 1s, mantissa non-zero
            if (value <= 0) return 0; // Map negative values to zero
            if (float.IsPositiveInfinity(value)) return 0x7C0; // Exponent all 1s, mantissa 0

            // Extract components from the floating point value
            uint floatBits = (uint)BitConverter.SingleToInt32Bits(value);
            int biasedExponent = (int)((floatBits >> 23) & 0xFF);
            int unbiasedExponent = biasedExponent - 127; // Remove IEEE-754 bias
            uint mantissa = floatBits & 0x7FFFFF;

            // Case 1: Denormalized values (exponent < -14)
            if (unbiasedExponent < -14)
            {
                // Special case for very small values that can't be normalized
                // For extremely small values, directly calculate the mantissa bits
                float e = -14.0f; // Denorm exponent
                float m = value / (float)Math.Pow(2, e); // Scale to get mantissa
                uint bits = (uint)Math.Round(m * 64.0f); // Scale to 6-bit mantissa range
                return Math.Max(1, Math.Min(bits, 0x3F)); // Ensure within valid range and non-zero
            }

            // Case 2: Normalized values within representable range
            if (unbiasedExponent <= 15)
            {
                // Adjust exponent bias (from -127 to -15)
                uint biased11Exponent = (uint)(unbiasedExponent + 15);

                // Extract the 6 most significant bits of mantissa
                uint bits11Mantissa = mantissa >> 17;

                // Combine exponent and mantissa
                return (biased11Exponent << 6) | bits11Mantissa;
            }

            // Case 3: Values too large (exponent > 15) become infinity
            return 0x7C0; // Infinity representation in float11
        }

        /// <summary>
        /// Unpacks an 11-bit floating point value to a standard 32-bit float.
        /// </summary>
        /// <remarks>
        /// Conforms to the packed 11-bit floating point format as defined in the Khronos Data Format Specification 1.4.
        /// See: <a href="https://registry.khronos.org/DataFormat/specs/1.4/dataformat.1.4.html#11bitfp"/>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Float11ToFloat(uint packedValue)
        {
            // Extract components
            uint mantissa = packedValue & 0x3F; // 6-bit mantissa
            uint exponent = (packedValue >> 6) & 0x1F; // 5-bit exponent

            // Handle special cases
            if (exponent == 0)
            {
                if (mantissa == 0) return 0.0f; // Zero

                // Denormalized value: 2^-14 * (mantissa/64)
                float value = (float)(Math.Pow(2, -14) * (mantissa / 64.0));
                return value;
            }
            else if (exponent == 31) // 5 bits all set
            {
                if (mantissa == 0) return float.PositiveInfinity; // Infinity
                return float.NaN; // NaN
            }

            // Normalized value: 2^(exponent-15) * (1 + mantissa/64)
            // Cast to int to avoid unsigned integer overflow when exponent < 15
            int expValue = (int)exponent - 15;
            float result = (float)(Math.Pow(2, expValue) * (1.0 + mantissa / 64.0));
            return result;
        }

        /// <summary>
        /// Packs a float value into a 10-bit floating point representation (5 exponent, 5 mantissa).
        /// </summary>
        /// <remarks>
        /// Conforms to the packed 10-bit floating point format as defined in the Khronos Data Format Specification 1.4.
        /// See: <a href="https://registry.khronos.org/DataFormat/specs/1.4/dataformat.1.4.html#10bitfp"/>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FloatToFloat10(float value)
        {
            // Handle special cases
            if (float.IsNaN(value)) return 0x3E0 | 1; // Exponent all 1s, mantissa non-zero
            if (value <= 0) return 0; // Map negative values to zero
            if (float.IsPositiveInfinity(value)) return 0x3E0; // Exponent all 1s, mantissa 0

            // Extract components from the floating point value
            uint floatBits = (uint)BitConverter.SingleToInt32Bits(value);
            int biasedExponent = (int)((floatBits >> 23) & 0xFF);
            int unbiasedExponent = biasedExponent - 127; // Remove IEEE-754 bias
            uint mantissa = floatBits & 0x7FFFFF;

            // Case 1: Denormalized values (exponent < -14)
            if (unbiasedExponent < -14)
            {
                // Special case for very small values that can't be normalized
                // Directly calculate the mantissa bits
                float e = -14.0f; // Denorm exponent
                float m = value / (float)Math.Pow(2, e); // Scale to get mantissa
                uint bits = (uint)Math.Round(m * 32.0f); // Scale to 5-bit mantissa range
                return Math.Max(1, Math.Min(bits, 0x1F)); // Ensure within valid range and non-zero
            }

            // Case 2: Normalized values within representable range
            if (unbiasedExponent <= 15)
            {
                // Adjust exponent bias (from -127 to -15)
                uint biased10Exponent = (uint)(unbiasedExponent + 15);

                // Extract the 5 most significant bits of mantissa
                uint bits10Mantissa = mantissa >> 18;

                // Combine exponent and mantissa
                return (biased10Exponent << 5) | bits10Mantissa;
            }

            // Case 3: Values too large (exponent > 15) become infinity
            return 0x3E0; // Infinity representation in float10
        }

        /// <summary>
        /// Unpacks a 10-bit floating point value to a standard 32-bit float.
        /// </summary>
        /// <remarks>
        /// Conforms to the packed 10-bit floating point format as defined in the Khronos Data Format Specification 1.4.
        /// See: <a href="https://registry.khronos.org/DataFormat/specs/1.4/dataformat.1.4.html#10bitfp"/>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Float10ToFloat(uint packedValue)
        {
            // Extract components
            uint mantissa = packedValue & 0x1F; // 5-bit mantissa
            uint exponent = (packedValue >> 5) & 0x1F; // 5-bit exponent

            // Handle special cases
            if (exponent == 0)
            {
                if (mantissa == 0) return 0.0f; // Zero

                // Denormalized value: 2^-14 * (mantissa/32)
                float value = (float)(Math.Pow(2, -14) * (mantissa / 32.0));
                return value;
            }
            else if (exponent == 31) // 5 bits all set
            {
                if (mantissa == 0) return float.PositiveInfinity;
                return float.NaN;
            }

            // Normalized value: 2^(exponent-15) * (1 + mantissa/32)
            // Cast to int to avoid unsigned integer underflow when exponent < 15
            int expValue = (int)exponent - 15;
            float result = (float)(Math.Pow(2, expValue) * (1.0 + mantissa / 32.0));
            return result;
        }
        #endregion
    }
}
