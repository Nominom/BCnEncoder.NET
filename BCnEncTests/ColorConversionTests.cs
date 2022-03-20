using System;
using System.Collections.Generic;
using System.Text;
using BCnEncoder.Shared;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests
{
	public class ColorConversionTests
	{
		private ITestOutputHelper output;

		public ColorConversionTests(ITestOutputHelper output)
		{
			this.output = output;
		}


		[Fact]
		public void Rgba32ToRgbaFloat()
		{
			for (var b = 0; b < 256; b++)
			{
				var orig = new ColorRgba32((byte)b, (byte)b, (byte)b, (byte)b);
				var floatRgba = orig.ToColorRgbaFloat();

				var rgba = floatRgba.ToRgba32();
				Assert.Equal(orig, rgba);
			}
		}

		[Fact]
		public void RgbeToRgbaFloat()
		{
			var inc = 0.01f;
			for (var b = 0f; b < 256f; b+=inc)
			{
				var orig = new ColorRgbaFloat(b, b, b);
				var rgbe = new ColorRgbe();
				rgbe.FromColorRgbaFloat(orig);

				var rgba = rgbe.ToColorRgbaFloat();

				Assert.True(MathF.Abs(orig.r - rgba.r) < inc / 2);
				Assert.True(MathF.Abs(orig.g - rgba.g) < inc / 2);
				Assert.True(MathF.Abs(orig.b - rgba.b) < inc / 2);

				inc *= 2;

				output.WriteLine($"Original: {orig}");
				output.WriteLine($"New     : {rgba}");
			}
		}

		[Fact]
		public void XyzeToRgbaFloat()
		{
			var inc = 0.02f;
			for (var b = 0.02f; b < 256f; b += inc)
			{
				var orig = new ColorRgbaFloat(b, b, b);
				var xyze = new ColorXyze();
				xyze.FromColorRgbaFloat(orig);

				var rgba = xyze.ToColorRgbaFloat();

				Assert.True(MathF.Abs(orig.r - rgba.r) < inc / 2, $"Expected {orig.r}, got {rgba.r}");
				Assert.True(MathF.Abs(orig.g - rgba.g) < inc / 2, $"Expected {orig.g}, got {rgba.g}");
				Assert.True(MathF.Abs(orig.b - rgba.b) < inc / 2, $"Expected {orig.b}, got {rgba.b}");

				inc *= 2;

				output.WriteLine($"Original: {orig}");
				output.WriteLine($"New     : {rgba}");
			}
		}

		[Fact]
		public void XyzToRgbaFloat()
		{
			var inc = 0.02f;
			for (var b = 0.02f; b < 256f; b += inc)
			{
				var orig = new ColorRgbaFloat(b, b, b);
				var xyz = new ColorXyz();
				xyz.FromColorRgbaFloat(orig);

				var rgba = xyz.ToColorRgbaFloat();

				Assert.True(MathF.Abs(orig.r - rgba.r) < inc / 2, $"Expected {orig.r}, got {rgba.r}");
				Assert.True(MathF.Abs(orig.g - rgba.g) < inc / 2, $"Expected {orig.g}, got {rgba.g}");
				Assert.True(MathF.Abs(orig.b - rgba.b) < inc / 2, $"Expected {orig.b}, got {rgba.b}");

				inc *= 2;

				output.WriteLine($"Original: {orig}");
				output.WriteLine($"New     : {rgba}");
			}
		}

		[Fact]
		public void RgbaFloatToSRgbaFloat()
		{
			for (var b = 0; b < 256; b++)
			{
				var orig = new ColorRgbaFloat(b / 255f, b / 255f, b / 255f, b / 255f);
				var srgb = orig.ToSRgb();

				var rgba = srgb.ToLRgb();
				Assert.Equal(orig.r, rgba.r, 4);
				Assert.Equal(orig.g, rgba.g, 4);
				Assert.Equal(orig.b, rgba.b, 4);
				Assert.Equal(orig.a, rgba.a, 4);
			}
		}

	}
}
