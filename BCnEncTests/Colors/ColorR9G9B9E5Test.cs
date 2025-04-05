using System;
using BCnEncoder.Shared.Colors;
using Xunit;

namespace BCnEncTests.Colors
{
    public class ColorR9G9B9E5Test
    {
        [Fact]
        public void AccessComponentValues_HighAndLowBitValues()
        {
            // Test high values (all bits set)
            var colorHigh = new ColorR9G9B9E5(float.MaxValue, float.MaxValue, float.MaxValue);

            // Extract components to check bit patterns
            uint rm = colorHigh.packedValue & 0x1FF;
            uint gm = (colorHigh.packedValue >> 9) & 0x1FF;
            uint bm = (colorHigh.packedValue >> 18) & 0x1FF;
            uint exp = (colorHigh.packedValue >> 27) & 0x1F;

            // For max values, we expect the mantissas to be maximized and exponent to be high
            Assert.Equal(0x1FFU, rm); // 9 bits all set = 0x1FF (511)
            Assert.Equal(0x1FFU, gm);
            Assert.Equal(0x1FFU, bm);
            Assert.Equal(31U, exp);   // 5 bits all set = 31

            // Test low values (no bits set)
            var colorLow = new ColorR9G9B9E5(0.0f, 0.0f, 0.0f);
            Assert.Equal(0U, colorLow.packedValue);

            var rgbaLow = colorLow.ToColorRgbaFloat();
            Assert.Equal(0.0f, rgbaLow.r);
            Assert.Equal(0.0f, rgbaLow.g);
            Assert.Equal(0.0f, rgbaLow.b);
        }

        [Fact]
        public void TestSharedExponentPacking()
        {
            // Test with values that would create a specific known bit pattern
            // We'll use values that should produce a simple bit pattern after conversion

            // First, create a color with power-of-2 values that can be exactly represented
            var color = new ColorR9G9B9E5(1.0f, 0.5f, 0.25f);

            // Convert to RGBA and back to verify round-trip conversion
            var rgba = color.ToColorRgbaFloat();
            var roundTrip = new ColorR9G9B9E5();
            roundTrip.FromColorRgbaFloat(rgba);

            // Values should be very close after round-trip
            const float epsilon = 1e-3f;
            Assert.True(Math.Abs(1.0f - rgba.r) < epsilon);
            Assert.True(Math.Abs(0.5f - rgba.g) < epsilon);
            Assert.True(Math.Abs(0.25f - rgba.b) < epsilon);

            // The original and round-trip packed values should be the same or very close
            Assert.Equal(color.packedValue, roundTrip.packedValue);
        }

        [Fact]
        public void TestConversionToAndFromRgbaFloat()
        {
            // Create a set of test colors
            ColorRgbaFloat[] testColors = new []
            {
                new ColorRgbaFloat(1.0f, 1.0f, 1.0f),       // White
                new ColorRgbaFloat(1.0f, 0.0f, 0.0f),       // Red
                new ColorRgbaFloat(0.0f, 1.0f, 0.0f),       // Green
                new ColorRgbaFloat(0.0f, 0.0f, 1.0f),       // Blue
                new ColorRgbaFloat(0.5f, 0.5f, 0.5f),       // Gray
                new ColorRgbaFloat(0.1f, 0.2f, 0.3f),       // Arbitrary color
                new ColorRgbaFloat(24f, 32f, 0f),           // Large exponent
                new ColorRgbaFloat(64f, 0f, 24f),
                new ColorRgbaFloat(6f, 0f, 8f),
            };

            foreach (var originalColor in testColors)
            {
                // Convert to our format
                var packedColor = new ColorR9G9B9E5();
                packedColor.FromColorRgbaFloat(originalColor);

                // Convert back to RGBA
                var roundTripColor = packedColor.ToColorRgbaFloat();

                // Verify the values are approximately equal (allowing for precision loss)
                const float epsilon = 1e-2f; // Larger epsilon due to shared exponent quantization
                Assert.True(Math.Abs(originalColor.r - roundTripColor.r) < epsilon,
                    $"Red mismatch: {originalColor.r} vs {roundTripColor.r}");
                Assert.True(Math.Abs(originalColor.g - roundTripColor.g) < epsilon,
                    $"Green mismatch: {originalColor.g} vs {roundTripColor.g}");
                Assert.True(Math.Abs(originalColor.b - roundTripColor.b) < epsilon,
                    $"Blue mismatch: {originalColor.b} vs {roundTripColor.b}");
            }
        }

        [Fact]
        public void TestEqualityAndHashCode()
        {
            var color1 = new ColorR9G9B9E5(0.1f, 0.2f, 0.3f);
            var color2 = new ColorR9G9B9E5(0.1f, 0.2f, 0.3f);
            var color3 = new ColorR9G9B9E5(0.3f, 0.2f, 0.1f);

            // First, verify we have distinct packed values for color1 and color3
            Assert.NotEqual(color1.packedValue, color3.packedValue);

            // Test equality methods using ColorR9G9B9E5.Equals
            Assert.True(color1.Equals(color2));
            Assert.False(color1.Equals(color3));

            // Test operator overloads
            Assert.True(color1 == color2);
            Assert.False(color1 == color3);
            Assert.True(color1 != color3);
            Assert.False(color1 != color2);

            // Test GetHashCode
            Assert.Equal(color1.GetHashCode(), color2.GetHashCode());
            Assert.NotEqual(color1.GetHashCode(), color3.GetHashCode());
        }

        [Fact]
        public void TestNegativeAndSpecialValues()
        {
            // Test with negative values (should be clamped to 0)
            var negativeColor = new ColorR9G9B9E5(-1.0f, -2.0f, -3.0f);
            var rgbaNegative = negativeColor.ToColorRgbaFloat();
            Assert.Equal(0.0f, rgbaNegative.r);
            Assert.Equal(0.0f, rgbaNegative.g);
            Assert.Equal(0.0f, rgbaNegative.b);

            // Test with NaN (should be treated as 0)
            var nanColor = new ColorR9G9B9E5(float.NaN, float.NaN, float.NaN);
            var rgbaNan = nanColor.ToColorRgbaFloat();
            Assert.Equal(0.0f, rgbaNan.r);
            Assert.Equal(0.0f, rgbaNan.g);
            Assert.Equal(0.0f, rgbaNan.b);

            // Test with very large values (should use max representable value)
            var largeColor = new ColorR9G9B9E5(float.MaxValue, float.MaxValue, float.MaxValue);
            // Extract the exponent - should be the maximum value (31)
            uint exp = (largeColor.packedValue >> 27) & 0x1F;
            Assert.Equal(31U, exp);
        }
    }
}
