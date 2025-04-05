using System;
using BCnEncoder.Shared.Colors;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests.Colors
{
    /// <summary>
    /// Tests conformance to the Khronos Data Format Specification for
    /// 11-bit and 10-bit floating point formats.
    /// See: https://registry.khronos.org/DataFormat/specs/1.3/dataformat.1.3.html#_packed_formats
    /// </summary>
    public class FloatingPointConversionTests
    {
	    private readonly ITestOutputHelper output;

	    public FloatingPointConversionTests(ITestOutputHelper output)
	    {
		    this.output = output;
	    }

	    private float ParseFloat11(uint bits)
	    {
		    bits = bits & 0x7FF;
		    int exponent = (int)((bits >> 6) & 0x1F);
		    int testExponent = (int)(bits / 64);
		    int mantissa = (int)(bits & 0x3F);
		    int testMantissa = (int)(bits % 64);

			Assert.Equal(exponent, testExponent);
			Assert.Equal(mantissa, testMantissa);

			if (exponent == 0 && mantissa == 0)
				return 0.0f;
			if (exponent == 0 && mantissa != 0)
				return (float)(Math.Pow(2, -14) * (mantissa / 64.0));
			if (exponent > 0 && exponent < 31)
				return (float)(Math.Pow(2, exponent - 15) * (1.0 + mantissa / 64.0));
			if (exponent == 31 && mantissa == 0)
				return float.PositiveInfinity;
			return float.NaN;
	    }

	    private float ParseFloat10(uint bits)
	    {
		    bits = bits & 0x3FF;
		    int exponent = (int)((bits >> 5) & 0x1F);
		    int testExponent = (int)(bits / 32);
		    int mantissa = (int)(bits & 0x1F);
			int testMantissa = (int)(bits % 32);

			Assert.Equal(exponent, testExponent);
			Assert.Equal(mantissa, testMantissa);

			if (exponent == 0 && mantissa == 0)
				return 0.0f;
			if (exponent == 0 && mantissa != 0)
				return (float)(Math.Pow(2, -14) * (mantissa / 32.0));
			if (exponent > 0 && exponent < 31)
				return (float)(Math.Pow(2, exponent - 15) * (1.0 + mantissa / 32.0));
			if (exponent == 31 && mantissa == 0)
				return float.PositiveInfinity;
			return float.NaN;
	    }

        [Fact]
	    public void Float11RoundingTest()
	    {
            for (float expected = 0.001f; expected <= 1000f; expected  *= 1.5f)
            {
                uint bits = ColorBitConversionHelpers.FloatToFloat11(expected);
                float value = ColorBitConversionHelpers.Float11ToFloat(bits);
                float reference = ParseFloat11(bits); // Khronos reference

                uint exponent = (bits >> 6) & 0x1F;
                uint mantissa = bits & 0x3F;

                double allowedError = 0.000001 * Math.Pow(2, exponent);

                output.WriteLine($"Expected: {expected}, Actual: {value}, Exponent: {exponent}, Mantissa: {mantissa}, AllowedError: {allowedError}");
                Assert.Equal(expected, value, allowedError);
                Assert.Equal(expected, reference, allowedError);
            }
        }

	    [Fact]
	    public void Float10RoundingTest()
	    {
		    for (float expected = 0.001f; expected <= 1000f; expected *= 1.5f)
		    {
			    uint bits = ColorBitConversionHelpers.FloatToFloat10(expected);
			    float value = ColorBitConversionHelpers.Float10ToFloat(bits);
			    float reference = ParseFloat10(bits); // Khronos reference

			    uint exponent = (bits >> 5) & 0x1F;
			    uint mantissa = bits & 0x1F;

			    double allowedError = 0.000001 * Math.Pow(2, exponent);

			    output.WriteLine($"Expected: {expected}, Actual: {value}, Reference: {reference}, Exponent: {exponent}, Mantissa: {mantissa}, AllowedError: {allowedError}");
			    Assert.Equal(expected, value, allowedError);
			    Assert.Equal(expected, reference, allowedError);
		    }
	    }

	    [Fact]
	    public void Float11Test()
	    {
		    // Go through all possible exponents
		    for (uint exponent = 0; exponent <= 31; exponent++)
		    {
			    // Go through all possible mantissas
			    for (uint mantissa = 0; mantissa <= 63; mantissa++)
			    {
				    double allowedError = 0.00001 * Math.Pow(2, exponent);

				    uint bits = (exponent << 6) | mantissa;
				    float value = ColorBitConversionHelpers.Float11ToFloat(bits);
				    float expected = ParseFloat11(bits);

				    output.WriteLine($"Expected: {expected}, Actual: {value}, Exponent: {exponent}, Mantissa: {mantissa}, AllowedError: {allowedError}");
				    Assert.Equal(expected, value, allowedError);

				    // Verify round-trip
				    uint roundTrip = ColorBitConversionHelpers.FloatToFloat11(value);

				    if (exponent < 31)
				    {
					    Assert.True(bits == roundTrip,
						    $"Expected: 0x{bits:x8}, RoundTrip: 0x{roundTrip:x8}, Exponent: {exponent}, Mantissa: {mantissa}");
				    }else if (exponent == 31 && mantissa == 0)
				    {
					    Assert.Equal((exponent << 6), roundTrip);
				    }
				    else
				    {
					    Assert.Equal((exponent << 6) | 1, roundTrip);
				    }
			    }
		    }
	    }

	    [Fact]
	    public void Float10Test()
	    {
		    // Go through all possible exponents
		    for (uint exponent = 0; exponent <= 31; exponent++)
		    {
			    // Go through all possible mantissas
			    for (uint mantissa = 0; mantissa <= 31; mantissa++)
			    {
				    double allowedError = 0.00001 * Math.Pow(2, exponent);
				    uint bits = (exponent << 5) | mantissa;
				    float value = ColorBitConversionHelpers.Float10ToFloat(bits);
				    float expected = ParseFloat10(bits);

				    output.WriteLine($"Expected: {expected}, Actual: {value}, Exponent: {exponent}, Mantissa: {mantissa}, AllowedError: {allowedError}");
				    Assert.Equal(expected, value, allowedError);

				    uint roundTrip = ColorBitConversionHelpers.FloatToFloat10(value);
				    // Verify round-trip
				    if (exponent < 31)
				    {
					    Assert.True(bits == roundTrip,
						    $"Expected: 0x{bits:x8}, RoundTrip: 0x{roundTrip:x8}, Exponent: {exponent}, Mantissa: {mantissa}");
				    }else if (exponent == 31 && mantissa == 0)
				    {
					    Assert.Equal((exponent << 5), roundTrip);
				    }
				    else
				    {
					    Assert.Equal((exponent << 5) | 1, roundTrip);
				    }
			    }
		    }
	    }

        [Theory]
        [InlineData(0, 0, 0.0f)] // Zero
        [InlineData(31, 0, float.PositiveInfinity)] // Positive Infinity
        [InlineData(31, 1, float.NaN)] // NaN
        public void Float11EdgeCases(uint exponent, uint mantissa, float expected)
        {
	        uint bits = (exponent << 6) | mantissa;
	        float value = ColorBitConversionHelpers.Float11ToFloat(bits);

	        output.WriteLine($"Expected: {expected}, Actual: {value}, Exponent: {exponent}, Mantissa: {mantissa}");
	        Assert.Equal(expected, value);

	        // Verify round-trip
	        uint roundTrip = ColorBitConversionHelpers.FloatToFloat11(value);
	        Assert.True(bits == roundTrip,
		        $"Expected: {bits}, RoundTrip: {roundTrip}, Exponent: {exponent}, Mantissa: {mantissa}");
        }

        [Theory]
        [InlineData(0, 0, 0.0f)] // Zero
        [InlineData(31, 0, float.PositiveInfinity)] // Positive Infinity
		[InlineData(31, 1, float.NaN)] // NaN
        public void Float10EdgeCases(uint exponent, uint mantissa, float expected)
        {
	        uint bits = (exponent << 5) | mantissa;
	        float value = ColorBitConversionHelpers.Float10ToFloat(bits);

	        output.WriteLine($"Expected: {expected}, Actual: {value}, Exponent: {exponent}, Mantissa: {mantissa}");
	        Assert.Equal(expected, value);

	        // Verify round-trip
	        uint roundTrip = ColorBitConversionHelpers.FloatToFloat10(value);
	        Assert.True(bits == roundTrip,
		        $"Expected: {bits}, RoundTrip: {roundTrip}, Exponent: {exponent}, Mantissa: {mantissa}");
        }
    }
}
