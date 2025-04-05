using System;
using BCnEncoder.Shared.Colors;
using Xunit;

namespace BCnEncTests.Colors
{
    /// <summary>
    /// Tests for the ColorR11G11B10F format which uses packed float components.
    /// </summary>
    public class ColorR11G11B10PackedUFloatTest
    {
        [Fact]
        public void AccessComponentValues_HighAndLowBitValues()
        {
            // Test high values (all bits set)
            var colorHigh = new ColorR11G11B10PackedUFloat(float.MaxValue, float.MaxValue, float.MaxValue);

            // Values should be clamped to the maximum representable by the format
            Assert.True(colorHigh.R > 0);
            Assert.True(colorHigh.G > 0);
            Assert.True(colorHigh.B > 0);

            Assert.Equal(0x7C0U, colorHigh.RawR); // Exponent all 1s, mantissa 0
            Assert.Equal(0x7C0U, colorHigh.RawG);
            Assert.Equal(0x3E0U, colorHigh.RawB); // Exponent all 1s, mantissa 0

            // Test low values (no bits set)
            var colorLow = new ColorR11G11B10PackedUFloat(0.0f, 0.0f, 0.0f);

            Assert.Equal(0.0f, colorLow.R);
            Assert.Equal(0.0f, colorLow.G);
            Assert.Equal(0.0f, colorLow.B);

            Assert.Equal(0U, colorLow.RawR);
            Assert.Equal(0U, colorLow.RawG);
            Assert.Equal(0U, colorLow.RawB);
        }

        [Fact]
        public void TestBitFieldPacking()
        {
            // Create a color with known bit patterns
            var color = new ColorR11G11B10PackedUFloat(0);
            color.RawR = 0x123U;  // 11-bit value
            color.RawG = 0x456U;  // 11-bit value
            color.RawB = 0x78U;   // 10-bit value

            // Verify the bits are in the correct positions
            uint expected = 0x123U | (0x456U << 11) | (0x78U << 22);
            Assert.Equal(expected, color.data);

            // Verify reading them back works correctly
            Assert.Equal(0x123U, color.RawR);
            Assert.Equal(0x456U, color.RawG);
            Assert.Equal(0x78U, color.RawB);
        }

        [Fact]
        public void TestConversionToAndFromRgbaFloat()
        {
            // Start with RGBA
            var originalRgba = new ColorRgbaFloat(0.5f, 0.25f, 0.125f);

            // Convert to our format
            var packedColor = new ColorR11G11B10PackedUFloat();
            packedColor.FromColorRgbaFloat(originalRgba);

            // Convert back to RGBA
            var roundTripRgba = packedColor.ToColorRgbaFloat();

            // Verify the values are approximately equal (allowing for precision loss)
            const float epsilon = 1e-3f;
            Assert.True(Math.Abs(originalRgba.r - roundTripRgba.r) < epsilon, $"Red mismatch: {originalRgba.r} vs {roundTripRgba.r}");
            Assert.True(Math.Abs(originalRgba.g - roundTripRgba.g) < epsilon, $"Green mismatch: {originalRgba.g} vs {roundTripRgba.g}");
            Assert.True(Math.Abs(originalRgba.b - roundTripRgba.b) < epsilon, $"Blue mismatch: {originalRgba.b} vs {roundTripRgba.b}");
        }

        [Fact]
        public void TestEqualityAndHashCode()
        {
            var color1 = new ColorR11G11B10PackedUFloat(0.1f, 0.2f, 0.3f);
            var color2 = new ColorR11G11B10PackedUFloat(0.1f, 0.2f, 0.3f);
            var color3 = new ColorR11G11B10PackedUFloat(0.3f, 0.2f, 0.1f);

            // Test equality
            Assert.True(color1.Equals(color2));
            Assert.True(color1 == color2);
            Assert.False(color1.Equals(color3));
            Assert.False(color1 == color3);
            Assert.True(color1 != color3);

            // Test hash codes
            Assert.Equal(color1.GetHashCode(), color2.GetHashCode());
        }

        [Fact]
        public void TestNegativeAndSpecialValues()
        {
            // Test with negative values (should be clamped to 0)
            var negativeColor = new ColorR11G11B10PackedUFloat(-1.0f, -2.0f, -3.0f);
            Assert.Equal(0.0f, negativeColor.R);
            Assert.Equal(0.0f, negativeColor.G);
            Assert.Equal(0.0f, negativeColor.B);

            // Test with NaN
            var nanColor = new ColorR11G11B10PackedUFloat(float.NaN, float.NaN, float.NaN);
            Assert.Equal(float.NaN, nanColor.R);
            Assert.Equal(float.NaN, nanColor.G);
            Assert.Equal(float.NaN, nanColor.B);

            // Test with infinity
            var infColor = new ColorR11G11B10PackedUFloat(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Assert.Equal(0x7C0U, infColor.RawR);
            Assert.Equal(0x7C0U, infColor.RawG);
            Assert.Equal(0x3E0U, infColor.RawB);
            Assert.True(float.IsPositiveInfinity(infColor.R));
            Assert.True(float.IsPositiveInfinity(infColor.G));
            Assert.True(float.IsPositiveInfinity(infColor.B));
        }
    }
}
