using System;
using BCnEncoder.Shared.Colors;
using Xunit;

namespace BCnEncTests.Colors
{
	public class ColorTest
	{

		[Fact]
		public void Rgb565Test()
		{
			var color = new ColorRgb565(255, 255, 255);

			Assert.Equal(255, color.R);
			Assert.Equal(255, color.G);
			Assert.Equal(255, color.B);

			color.R = 0;
			Assert.Equal(0, color.R);
			Assert.Equal(255, color.G);
			Assert.Equal(255, color.B);

			color.G = 0;
			Assert.Equal(0, color.R);
			Assert.Equal(0, color.G);
			Assert.Equal(255, color.B);

			color.B = 0;
			Assert.Equal(0, color.R);
			Assert.Equal(0, color.G);
			Assert.Equal(0, color.B);

			color = new ColorRgb565(255, 255, 255);

			color.B = 0;
			Assert.Equal(255, color.R);
			Assert.Equal(255, color.G);
			Assert.Equal(0, color.B);

			color.G = 0;
			Assert.Equal(255, color.R);
			Assert.Equal(0, color.G);
			Assert.Equal(0, color.B);

			color.R = 0;
			Assert.Equal(0, color.R);
			Assert.Equal(0, color.G);
			Assert.Equal(0, color.B);

			color = new ColorRgb565(127, 127, 127);

			Assert.Equal(123, color.R);
			Assert.Equal(125, color.G);
			Assert.Equal(123, color.B);
		}
	}
}
