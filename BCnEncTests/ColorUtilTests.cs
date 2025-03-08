using System;
using System.Collections.Generic;
using System.Text;
using BCnEncoder.Shared;
using BCnEncoder.TextureFormats;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests
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
			var shared = ColorUtils.RgbToSharedExponent(testFloat, mantissaBits, exponentBias, exponentMax);

			Assert.Equal(shared.Exponent & ((1 << exponentBits) - 1), shared.Exponent);
			Assert.Equal(shared.Red & ((1 << mantissaBits) - 1), shared.Red);
			Assert.Equal(shared.Green & ((1 << mantissaBits) - 1), shared.Green);
			Assert.Equal(shared.Blue & ((1 << mantissaBits) - 1), shared.Blue);

			var outputFloat = ColorUtils.SharedExponentToRgb(shared, mantissaBits, exponentBias);

			Assert.Equal(testFloat.r, outputFloat.r, testPrecision);
			Assert.Equal(testFloat.g, outputFloat.g, testPrecision);
			Assert.Equal(testFloat.b, outputFloat.b, testPrecision);
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
			var testFloat = new ColorRgbaFloat(r, g, b).ToLRgb();

			var shared = ColorUtils.RgbToSharedExponent(testFloat,
				8, 128, 255);

			Assert.Equal(shared.Exponent & 255, shared.Exponent);
			Assert.Equal(shared.Red & 255, shared.Red);
			Assert.Equal(shared.Green & 255, shared.Green);
			Assert.Equal(shared.Blue & 255, shared.Blue);

			var rgbe = new ColorRgbe((byte)shared.Red, (byte)shared.Green, (byte)shared.Blue, (byte)shared.Exponent);

			var outputFloat = rgbe.ToColorRgbaFloat();

			output.WriteLine($"Original: {new ColorRgbaFloat(r, g, b)}");
			output.WriteLine($"Output  : {outputFloat}");

			Assert.True(MathF.Abs(r - outputFloat.r) < resultWithin, $"R Expected {r}, got {outputFloat.r}");
			Assert.True(MathF.Abs(g - outputFloat.g) < resultWithin, $"G Expected {g}, got {outputFloat.g}");
			Assert.True(MathF.Abs(b - outputFloat.b) < resultWithin, $"B Expected {b}, got {outputFloat.b}");
		}
	}
}
