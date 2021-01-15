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

		// HINT: Parallel decoding is too fast to be cancelled.
		// HINT: Even with TimeSpan.FromTicks(1) the test never successfully threw an exception when executed in bulk with other tests.

		[Fact]
		public async void DecodeNonParallelCancellation()
		{
			var file = DdsLoader.TestDecompressBc7;

			var decoder = new BcDecoder();
			decoder.Options.IsParallel = false;

			var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
					decoder.DecodeAsync(file, source.Token));
		}
	}
}
