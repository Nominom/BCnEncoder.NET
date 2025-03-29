using System;
using BCnEncoder.Shared.Colors;
using Xunit;

namespace BCnEncTests.Colors
{
    public class ColorR10G10B10A2Test
    {
        [Fact]
        public void AccessComponentValues_HighAndLowBitValues()
        {
            // Test high values (all bits set)
            var colorHigh = new ColorR10G10B10A2(1.0f, 1.0f, 1.0f, 1.0f);

            Assert.Equal(1.0f, colorHigh.R);
            Assert.Equal(1.0f, colorHigh.G);
            Assert.Equal(1.0f, colorHigh.B);
            Assert.Equal(1.0f, colorHigh.A);

            Assert.Equal(1023u, colorHigh.RawR); // 10 bits all set = 1023
            Assert.Equal(1023u, colorHigh.RawG);
            Assert.Equal(1023u, colorHigh.RawB);
            Assert.Equal(3u, colorHigh.RawA);    // 2 bits all set = 3

            // Test low values (no bits set)
            var colorLow = new ColorR10G10B10A2(0.0f, 0.0f, 0.0f, 0.0f);

            Assert.Equal(0.0f, colorLow.R);
            Assert.Equal(0.0f, colorLow.G);
            Assert.Equal(0.0f, colorLow.B);
            Assert.Equal(0.0f, colorLow.A);

            Assert.Equal(0u, colorLow.RawR);
            Assert.Equal(0u, colorLow.RawG);
            Assert.Equal(0u, colorLow.RawB);
            Assert.Equal(0u, colorLow.RawA);

            // Test mid values (roughly half bits set)
            var colorMid = new ColorR10G10B10A2(0.5f, 0.5f, 0.5f, 0.5f);

            Assert.Equal(0.5f, colorMid.R, 3); // Allow small floating point error
            Assert.Equal(0.5f, colorMid.G, 3);
            Assert.Equal(0.5f, colorMid.B, 3);

            // For alpha, with only 2 bits (0-3), a value of 0.5 gets mapped to 1,
            // which when converted back is 1/3 or approximately 0.33f
            // So we expect either 0.33f (if mapped to 1) or 0.67f (if mapped to 2)
            Assert.True(Math.Abs(colorMid.A - 0.33f) < 0.01f || Math.Abs(colorMid.A - 0.67f) < 0.01f,
                        $"Alpha value {colorMid.A} should be close to either 0.33 or 0.67 due to 2-bit precision");

            // Floating point imprecision may make these slightly off from exactly 511 (half of 1023)
            Assert.InRange(colorMid.RawR, 510u, 512u);
            Assert.InRange(colorMid.RawG, 510u, 512u);
            Assert.InRange(colorMid.RawB, 510u, 512u);
            // With only 2 bits, 0.5 should get mapped to either 1 or 2
            Assert.True(colorMid.RawA == 1u || colorMid.RawA == 2u);
        }

        [Fact]
        public void SetComponentValues_Individual()
        {
            // Create a color and modify R, G, B, A individually
            var color = new ColorR10G10B10A2(0.0f, 0.0f, 0.0f, 0.0f);

            // Test setting R component
            color.R = 1.0f;
            Assert.Equal(1.0f, color.R);
            Assert.Equal(0.0f, color.G);
            Assert.Equal(0.0f, color.B);
            Assert.Equal(0.0f, color.A);
            Assert.Equal(1023u, color.RawR);
            Assert.Equal(0u, color.RawG);
            Assert.Equal(0u, color.RawB);
            Assert.Equal(0u, color.RawA);

            // Test setting G component
            color.G = 0.75f;
            Assert.Equal(1.0f, color.R);
            Assert.Equal(0.75f, color.G, 3); // Allow small floating point error
            Assert.Equal(0.0f, color.B);
            Assert.Equal(0.0f, color.A);
            Assert.Equal(1023u, color.RawR);
            Assert.InRange(color.RawG, 766u, 768u); // ~0.75 * 1023
            Assert.Equal(0u, color.RawB);
            Assert.Equal(0u, color.RawA);

            // Test setting B component
            color.B = 0.25f;
            Assert.Equal(1.0f, color.R);
            Assert.Equal(0.75f, color.G, 3);
            Assert.Equal(0.25f, color.B, 3);
            Assert.Equal(0.0f, color.A);
            Assert.Equal(1023u, color.RawR);
            Assert.InRange(color.RawG, 766u, 768u);
            Assert.InRange(color.RawB, 255u, 257u); // ~0.25 * 1023
            Assert.Equal(0u, color.RawA);

            // Test setting A component
            // With only 2 bits of precision, 0.66 should map to 2, which is 2/3 ≈ 0.67
            color.A = 0.66f;
            Assert.Equal(1.0f, color.R);
            Assert.Equal(0.75f, color.G, 3);
            Assert.Equal(0.25f, color.B, 3);
            // For 2-bit alpha, 0.66 gets quantized to 2/3 ≈ 0.67
            Assert.Equal(2u, color.RawA); // Should map to 2 out of 3
            Assert.InRange(color.A, 0.65f, 0.68f); // Should be approximately 0.67
            Assert.Equal(1023u, color.RawR);
            Assert.InRange(color.RawG, 766u, 768u);
            Assert.InRange(color.RawB, 255u, 257u);
        }

        [Fact]
        public void SetBitMasks_EnsureCorrectBitPositions()
        {
            // Test that bit masks are working correctly
            var color = new ColorR10G10B10A2(0.0f, 0.0f, 0.0f, 0.0f);

            // Set raw values directly to test masks
            color.RawR = 1023; // All 10 bits of R set
            color.RawG = 0;
            color.RawB = 0;
            color.RawA = 0;

            // Data should have only R bits set
            Assert.Equal(0x000003FFu, color.data);

            // Reset and test G mask
            color.RawR = 0;
            color.RawG = 1023; // All 10 bits of G set
            color.RawB = 0;
            color.RawA = 0;

            // Data should have only G bits set
            Assert.Equal(0x000FFC00u, color.data);

            // Reset and test B mask
            color.RawR = 0;
            color.RawG = 0;
            color.RawB = 1023; // All 10 bits of B set
            color.RawA = 0;

            // Data should have only B bits set
            Assert.Equal(0x3FF00000u, color.data);

            // Reset and test A mask
            color.RawR = 0;
            color.RawG = 0;
            color.RawB = 0;
            color.RawA = 3; // All 2 bits of A set

            // Data should have only A bits set
            Assert.Equal(0xC0000000u, color.data);

            // Test all bits set
            color.RawR = 1023;
            color.RawG = 1023;
            color.RawB = 1023;
            color.RawA = 3;

            // All bits should be set
            Assert.Equal(0xFFFFFFFFu, color.data);
        }

        [Fact]
        public void BoundaryValues_ClampNegativeAndOverflow()
        {
            // Test negative values (should clamp to 0)
            var color = new ColorR10G10B10A2(-1.0f, -0.5f, -0.1f, -0.2f);

            Assert.Equal(0.0f, color.R);
            Assert.Equal(0.0f, color.G);
            Assert.Equal(0.0f, color.B);
            Assert.Equal(0.0f, color.A);

            // Test overflow values (should clamp to 1.0)
            color = new ColorR10G10B10A2(1.5f, 2.0f, 10.0f, 5.0f);

            Assert.Equal(1.0f, color.R);
            Assert.Equal(1.0f, color.G);
            Assert.Equal(1.0f, color.B);
            Assert.Equal(1.0f, color.A);

            // Test setting raw values beyond limits
            color = new ColorR10G10B10A2(0, 0, 0, 0);
            color.RawR = 2000; // Beyond 10-bit limit
            color.RawG = 5000; // Beyond 10-bit limit
            color.RawB = 10000; // Beyond 10-bit limit
            color.RawA = 10; // Beyond 2-bit limit

            Assert.Equal(1023u, color.RawR); // Should clamp to 1023
            Assert.Equal(1023u, color.RawG); // Should clamp to 1023
            Assert.Equal(1023u, color.RawB); // Should clamp to 1023
            Assert.Equal(3u, color.RawA); // Should clamp to 3
        }

        [Fact]
        public void ColorRgbaFloat_ConversionRoundTrip()
        {
            // Test conversion from ColorR10G10B10A2 to ColorRgbaFloat and back
            var original = new ColorR10G10B10A2(0.1f, 0.5f, 0.75f, 0.66f);
            var rgbaFloat = original.ToColorRgbaFloat();
            var roundTrip = rgbaFloat.As<ColorR10G10B10A2>();

            // Compare float values with small tolerance due to precision loss
            Assert.Equal(original.R, rgbaFloat.r, 3);
            Assert.Equal(original.G, rgbaFloat.g, 3);
            Assert.Equal(original.B, rgbaFloat.b, 3);
            Assert.Equal(original.A, rgbaFloat.a, 3);

            // Compare original with round-trip values
            Assert.Equal(original.R, roundTrip.R, 3);
            Assert.Equal(original.G, roundTrip.G, 3);
            Assert.Equal(original.B, roundTrip.B, 3);
            Assert.Equal(original.A, roundTrip.A, 3);

            // Try with all 1s
            original = new ColorR10G10B10A2(1.0f, 1.0f, 1.0f, 1.0f);
            rgbaFloat = original.ToColorRgbaFloat();
	        roundTrip = rgbaFloat.As<ColorR10G10B10A2>();

            Assert.Equal(1.0f, rgbaFloat.r);
            Assert.Equal(1.0f, rgbaFloat.g);
            Assert.Equal(1.0f, rgbaFloat.b);
            Assert.Equal(1.0f, rgbaFloat.a);

            Assert.Equal(original.R, roundTrip.R);
            Assert.Equal(original.G, roundTrip.G);
            Assert.Equal(original.B, roundTrip.B);
            Assert.Equal(original.A, roundTrip.A);

            // Try with all 0s
            original = new ColorR10G10B10A2(0.0f, 0.0f, 0.0f, 0.0f);
            rgbaFloat = original.ToColorRgbaFloat();
            roundTrip = rgbaFloat.As<ColorR10G10B10A2>();

            Assert.Equal(0.0f, rgbaFloat.r);
            Assert.Equal(0.0f, rgbaFloat.g);
            Assert.Equal(0.0f, rgbaFloat.b);
            Assert.Equal(0.0f, rgbaFloat.a);

            Assert.Equal(original.R, roundTrip.R);
            Assert.Equal(original.G, roundTrip.G);
            Assert.Equal(original.B, roundTrip.B);
            Assert.Equal(original.A, roundTrip.A);
        }

        [Fact]
        public void ColorRgba32_ConversionRoundTrip()
        {
            // Create ColorR10G10B10A2 from ColorRgba32 and convert back
            var rgba32 = new ColorRgba32(128, 64, 192, 255);
            var r10g10b10a2 = rgba32.As<ColorR10G10B10A2>();

            // Check that values were preserved within reasonable limits
            // There will be some precision loss due to converting from 8-bit to 10-bit and back
            Assert.InRange(r10g10b10a2.R * 255f, 127, 129); // ~128/255
            Assert.InRange(r10g10b10a2.G * 255f, 63, 65);   // ~64/255
            Assert.InRange(r10g10b10a2.B * 255f, 191, 193); // ~192/255
            Assert.InRange(r10g10b10a2.A * 255f, 254, 255); // ~255/255

            // Convert to ColorRgba32 with the FromColorRgbaFloat method
            var roundTrip = new ColorRgba32();
            var rgbaFloat = r10g10b10a2.ToColorRgbaFloat();
            roundTrip.FromColorRgbaFloat(rgbaFloat);

            // Check that values were preserved within reasonable limits
            // There will be some precision loss due to the 10-bit to 8-bit conversions
            Assert.InRange(roundTrip.r, 127, 129); // ~128
            Assert.InRange(roundTrip.g, 63, 65);   // ~64
            Assert.InRange(roundTrip.b, 191, 193); // ~192
            Assert.InRange(roundTrip.a, 254, 255); // ~255
        }
    }
}
