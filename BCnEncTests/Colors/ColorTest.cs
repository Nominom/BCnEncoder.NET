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
			var color = new ColorRgb565(1, 1, 1);

			Assert.Equal(1, color.R);
			Assert.Equal(1, color.G);
			Assert.Equal(1, color.B);

			color.R = 0;
			Assert.Equal(0, color.R);
			Assert.Equal(1, color.G);
			Assert.Equal(1, color.B);

			color.G = 0;
			Assert.Equal(0, color.R);
			Assert.Equal(0, color.G);
			Assert.Equal(1, color.B);

			color.B = 0;
			Assert.Equal(0, color.R);
			Assert.Equal(0, color.G);
			Assert.Equal(0, color.B);

			color = new ColorRgb565(1, 1, 1);

			color.B = 0;
			Assert.Equal(1, color.R);
			Assert.Equal(1, color.G);
			Assert.Equal(0, color.B);

			color.G = 0;
			Assert.Equal(1, color.R);
			Assert.Equal(0, color.G);
			Assert.Equal(0, color.B);

			color.R = 0;
			Assert.Equal(0, color.R);
			Assert.Equal(0, color.G);
			Assert.Equal(0, color.B);

			color = new ColorRgb565(.5f, .5f, .5f);

			Assert.Equal(.5f, color.R, 1/31f);
			Assert.Equal(.5f, color.G, 1/63f);
			Assert.Equal(.5f, color.B, 1/31f);
		}
	}
}
