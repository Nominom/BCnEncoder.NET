using BCnEncoder.Shared;
using Xunit;

namespace BCnEncTests.Helpers
{
	public class IntHelperTests
	{

		[Theory]
		[InlineData(0, 2, 0)]
		[InlineData(0b0111, 4, 0b0111)]
		[InlineData(0b1111, 4, -1)]
		[InlineData(0b1111_1110, 8, -2)]
		[InlineData(0b11110011, 8, unchecked((int)0b1111_1111_1111_1111_1111_1111_1111_0011))]
		[InlineData(0b10100011, 8, unchecked((int)0b1111_1111_1111_1111_1111_1111_1010_0011))]
		[InlineData(0b10100011, 7, 0b010_0011)]
		public void SignExtend(int orig, int precision, int expected)
		{
			var result = IntHelper.SignExtend(orig, precision);
			Assert.Equal(expected, result);
		}
	}
}
