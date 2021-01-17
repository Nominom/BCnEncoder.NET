using System;
using System.Threading;
using BCnEncoder.Decoder;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests
{
	public class CancellationTest
	{
		[Fact]
		public async void EncodeParallelCancellation()
		{
			await TestHelper.ExecuteCancellationTest(ImageLoader.TestAlphaGradient1, true);
		}

		[Fact]
		public async void EncodeNonParallelCancellation()
		{
			await TestHelper.ExecuteCancellationTest(ImageLoader.TestAlphaGradient1, false);
		}

		// HINT: Decoding in general is too fast to be cancelled.
		// HINT: For parallel decoding even with TimeSpan.FromTicks(1) the test never successfully threw an exception when executed in bulk with other tests.
		// HINT: For non parallel decoding the test was partially successful, due to fluctuations in how much time the decoding needed and when the cancellation was introduced.
	}
}
