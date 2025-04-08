using BCnEncoder.Shared;
using Xunit;

namespace BCnEncTests.Helpers
{
	public class MathHelperTests
	{
		[Theory]
		[InlineData(16.4, 0.512500, 5)]
		public void FrExp(double val, double expectedMantissa, int expectedExponent)
		{
			int exp;
			var mantissa = MathHelper.FrExp(val, out exp);
			Assert.Equal(expectedExponent, exp);
			Assert.Equal(expectedMantissa, mantissa, 5);
		}

		[Theory]
		[InlineData(16.4f, 0.512500f, 5)]
		public void FrExpFloat(float val, float expectedMantissa, int expectedExponent)
		{
			var mantissa = (float)MathHelper.FrExp(val, out var exp);
			Assert.Equal(expectedExponent, exp);
			Assert.Equal(expectedMantissa, mantissa, 5);
		}

		[Theory]
		[InlineData(7f, -4, 0.437500)]
		public void LdExpFloat(float val, int exponent, float expectedValue)
		{
			var value = (float)MathHelper.LdExp(val, exponent);
			Assert.Equal(expectedValue, value);
		}
	}
}
