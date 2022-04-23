using System;
using System.Collections.Generic;
using System.Text;
using BCnEncoder.Shared;
using BCnEncoder.TextureFormats;
using Xunit;

namespace BCnEncTests
{
	public class KhrDataFormatUtilTests
	{

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
			var shared = KhrDataFormatUtils.RgbToSharedExponent(testFloat, mantissaBits, exponentBias, exponentMax);

			Assert.Equal(shared.Exponent & ((1 << exponentBits) - 1), shared.Exponent);
			Assert.Equal(shared.Red & ((1 << mantissaBits) - 1), shared.Red);
			Assert.Equal(shared.Green & ((1 << mantissaBits) - 1), shared.Green);
			Assert.Equal(shared.Blue & ((1 << mantissaBits) - 1), shared.Blue);

			var outputFloat = KhrDataFormatUtils.SharedExponentToRgb(shared, mantissaBits, exponentBias);

			Assert.Equal(testFloat.r, outputFloat.r, testPrecision);
			Assert.Equal(testFloat.g, outputFloat.g, testPrecision);
			Assert.Equal(testFloat.b, outputFloat.b, testPrecision);
		}


		[Theory]
		[InlineData(0.1f, 0.4f, 0.3f, 2)]
		[InlineData(1f, 0.4f, 0.1f,   2)]
		[InlineData(3f, 1f, 1.5f,     2)]
		[InlineData(0.002f, 0.01f, 0.05f, 3)]
		[InlineData(100f, 10f, 30f, 1)]
		public void TestSharedExponentRgbe(
			float r, float g, float b,
			int testPrecision)
		{
			var testFloat = new ColorRgbaFloat(r, g, b).ToLRgb();

			var shared = KhrDataFormatUtils.RgbToSharedExponent(testFloat,
				8, 128, 255);


			Assert.Equal(shared.Exponent & 255, shared.Exponent);
			Assert.Equal(shared.Red & 255, shared.Red);
			Assert.Equal(shared.Green & 255, shared.Green);
			Assert.Equal(shared.Blue & 255, shared.Blue);

			var rgbe = new ColorRgbe((byte)shared.Red, (byte)shared.Green, (byte)shared.Blue, (byte)shared.Exponent);

			var outputFloat = rgbe.ToColorRgbaFloat();

			Assert.Equal(r, outputFloat.r, testPrecision);
			Assert.Equal(g, outputFloat.g, testPrecision);
			Assert.Equal(b, outputFloat.b, testPrecision);
		}
	}
}
