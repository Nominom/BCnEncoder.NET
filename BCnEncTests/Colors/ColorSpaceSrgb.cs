using System;
using System.Numerics;
using BCnEncoder.Shared.Colors;
using Xunit;

namespace BCnEncTests.Colors;

public class ColorSpaceSrgb
{
	[Fact]
	public void ToSrgb_PerChannelCorrectness()
	{
		// R in linear region, G in gamma region, B at boundary
		var linear = new Vector4(0.002f, 0.5f, 0.0031308f, .5f);
		var expected = new Vector4(
			0.002f * 12.92f,                          // Linear path
			1.055f * MathF.Pow(0.5f, 1f / 2.4f) - 0.055f, // Gamma path
			0.04045f,                                 // Exact threshold boundary
			.5f                                    // Preserve alpha
		);

		var result =  ColorSpace.Srgb.ToSrgb(linear);

		Assert.Equal(expected.X, result.X, 5);
		Assert.Equal(expected.Y, result.Y, 5);
		Assert.Equal(expected.Z, result.Z, 5);
		Assert.Equal(expected.W, result.W);
	}

	[Fact]
	public void ToLrgb_PerChannelCorrectness()
	{
		// R in linear region, G in gamma region, B at boundary
		var srgb = new Vector4(0.02f, 0.7353569f, 0.04045f, .5f);
		var expected = new Vector4(
			0.02f / 12.92f,                               // Linear path
			MathF.Pow((0.7353569f + 0.055f) / 1.055f, 2.4f), // Gamma path
			0.0031308f,                                  // Exact threshold
			.5f                                       // Preserve alpha
		);

		var result = ColorSpace.Srgb.ToLrgb(srgb);

		Assert.Equal(expected.X, result.X, 5);
		Assert.Equal(expected.Y, result.Y, 5);
		Assert.Equal(expected.Z, result.Z, 5);
		Assert.Equal(expected.W, result.W);
	}
}
