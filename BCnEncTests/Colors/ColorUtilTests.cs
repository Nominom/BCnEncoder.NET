using System;
using System.Collections.Generic;
using System.Text;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using BCnEncoder.TextureFormats;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests.Colors
{
	public class ColorUtilTests
	{
		private ITestOutputHelper output;

		public ColorUtilTests(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Theory]
		[InlineData(0.1f, 0.4f, 0.3f, 9, 5, 15, 31, 2)]
		[InlineData(1f, 0.4f, 0.1f, 9, 5, 15, 31, 1)]
		[InlineData(3f, 1f, 1.5f, 9, 5, 15, 31, 1)]
		[InlineData(0.002f, 0.01f, 0.05f, 9, 5, 15, 31, 3)]
		[InlineData(100f, 10f, 30f, 9, 5, 15, 31, 1)]
		public void TestFloatToSharedExponent(
			float r, float g, float b,
			int mantissaBits, int exponentBits, int exponentBias, int exponentMax,
			int testPrecision)
		{
			var testFloat = new ColorRgbaFloat(r, g, b);
			var shared = ColorUtils.RgbToSharedExponent(r, g, b, mantissaBits, exponentBias, exponentMax);

			Assert.Equal(shared.exponent & ((1 << exponentBits) - 1), shared.exponent);
			Assert.Equal(shared.red & ((1 << mantissaBits) - 1), shared.red);
			Assert.Equal(shared.green & ((1 << mantissaBits) - 1), shared.green);
			Assert.Equal(shared.blue & ((1 << mantissaBits) - 1), shared.blue);

			var outputFloat = ColorUtils.SharedExponentToRgb(shared, mantissaBits, exponentBias);

			Assert.Equal(testFloat.r, outputFloat.red, testPrecision);
			Assert.Equal(testFloat.g, outputFloat.green, testPrecision);
			Assert.Equal(testFloat.b, outputFloat.blue, testPrecision);
		}


		[Theory]
		[InlineData(0.1f, 0.4f, 0.3f, .01f)]
		[InlineData(1f, 0.4f, 0.1f,   .05f)]
		[InlineData(3f, 1f, 1.5f,     .1f)]
		[InlineData(0.002f, 0.01f, 0.05f, .001f)]
		[InlineData(100f, 10f, 30f, 1)]
		public void TestSharedExponentRgbe(
			float r, float g, float b,
			float resultWithin)
		{
			var testFloat = new ColorRgbaFloat(r, g, b);

			var shared = ColorUtils.RgbToSharedExponent(r, g, b,
				8, 128, 255);

			Assert.Equal(shared.exponent & 255, shared.exponent);
			Assert.Equal(shared.red & 255, shared.red);
			Assert.Equal(shared.green & 255, shared.green);
			Assert.Equal(shared.blue & 255, shared.blue);

			var rgbe = new ColorRgbe((byte)shared.red, (byte)shared.green, (byte)shared.blue, (byte)shared.exponent);

			var outputFloat = rgbe.ToColorRgbaFloat();

			output.WriteLine($"Original: {new ColorRgbaFloat(r, g, b)}");
			output.WriteLine($"Output  : {outputFloat}");

			Assert.True(MathF.Abs(r - outputFloat.r) < resultWithin, $"R Expected {r}, got {outputFloat.r}");
			Assert.True(MathF.Abs(g - outputFloat.g) < resultWithin, $"G Expected {g}, got {outputFloat.g}");
			Assert.True(MathF.Abs(b - outputFloat.b) < resultWithin, $"B Expected {b}, got {outputFloat.b}");
		}
	}
}
