using System.Threading.Tasks;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests
{
	public class CancellationTests
	{
		[Fact]
		public async Task EncodeParallelCancellation()
		{
			await TestHelper.ExecuteCancellationTest(ImageLoader.TestAlphaGradient1, true);
		}

		[Fact]
		public async Task EncodeNonParallelCancellation()
		{
			await TestHelper.ExecuteCancellationTest(ImageLoader.TestAlphaGradient1, false);
		}
	}
}
