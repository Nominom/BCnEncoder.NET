using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using Xunit;

namespace BCnEncTests
{
	public class AsyncTests
	{
		[Fact]
		public async void EncodeParallelCancellation()
		{
			var alphaGradient = ImageLoader.TestAlphaGradient1;

			var encoder = new BcEncoder(CompressionFormat.Bc7);
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.Options.IsParallel = true;

			var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
			await Assert.ThrowsAsync<OperationCanceledException>(() =>
				encoder.EncodeToRawBytesAsync(alphaGradient, 0, source.Token));
		}

		[Fact]
		public async void EncodeNonParallelCancellation()
		{
			var alphaGradient = ImageLoader.TestAlphaGradient1;

			var encoder = new BcEncoder(CompressionFormat.Bc7);
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.Options.IsParallel = false;

			var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
			await Assert.ThrowsAsync<OperationCanceledException>(() =>
				encoder.EncodeToRawBytesAsync(alphaGradient, 0, source.Token));
		}

		// HINT: Parallel decoding is too fast to be cancelled.
		// HINT: Even with TimeSpan.FromTicks(1) the test never successfully threw an exception when executed in bulk with other tests.

		[Fact]
		public async void DecodeNonParallelCancellation()
		{
			using FileStream fs = File.OpenRead(@"../../../testImages/test_decompress_bc7.dds");
			DdsFile file = DdsFile.Load(fs);

			var decoder = new BcDecoder();
			decoder.Options.IsParallel = false;

			var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(10));
			await Assert.ThrowsAsync<OperationCanceledException>(() =>
				decoder.DecodeAsync(file, source.Token));
		}
	}
}
