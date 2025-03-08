using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests
{
	public class CancellationTests
	{
		private static BcEncoder MakeEncoder(CompressionFormat format, bool parallel)
		{
			var encoder = new BcEncoder(format);
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.MaxMipMapLevel = -1;
			encoder.Options.IsParallel = parallel;
			return encoder;
		}

		private static BcDecoder MakeDecoder(bool parallel)
		{
			return new BcDecoder()
			{
				Options = { IsParallel = parallel }
			};
		}

		[Theory]
		[InlineData(true, "rgba_1", CompressionFormat.Bc1)]
		[InlineData(true, "hdr_1_rgbe", CompressionFormat.Rgb24)]
		[InlineData(true, "hdr_1_rgbe", CompressionFormat.RgbaHalf)]
		[InlineData(true, "hdr_1_rgbe", CompressionFormat.Bc6U)]
		[InlineData(false, "rgba_1", CompressionFormat.Bc1)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Rgb24)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.RgbaHalf)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Bc6U)]
		public async void TestEncodeToRawBytesCancel(bool parallel, string name, CompressionFormat format)
		{
			var encoder = MakeEncoder(format, parallel);

			var inputData = ImageLoader.LoadTestData(name);

			Assert.NotEqual(CompressionFormat.Unknown, inputData.Format);

			var cancelSource = new CancellationTokenSource();
			// Test

			var task = Assert.ThrowsAnyAsync<OperationCanceledException>(() => encoder.EncodeToRawBytesAsync(inputData, 0, cancelSource.Token));
			cancelSource.Cancel();
			await task;
		}

		[Theory]
		[InlineData(true, "rgba_1", CompressionFormat.Bc1)]
		[InlineData(true, "hdr_1_rgbe", CompressionFormat.Rgb24)]
		[InlineData(true, "hdr_1_rgbe", CompressionFormat.RgbaHalf)]
		[InlineData(true, "hdr_1_rgbe", CompressionFormat.Bc6U)]
		[InlineData(false, "rgba_1", CompressionFormat.Bc1)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Rgb24)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.RgbaHalf)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Bc6U)]
		public async void TestEncodeCancel(bool parallel, string name, CompressionFormat format)
		{
			var encoder = MakeEncoder(format, parallel);

			var inputData = ImageLoader.LoadTestData(name);

			Assert.NotEqual(CompressionFormat.Unknown, inputData.Format);

			var cancelSource = new CancellationTokenSource();
			// Test

			var task = Assert.ThrowsAnyAsync<OperationCanceledException>(() => encoder.EncodeAsync(inputData, cancelSource.Token));
			cancelSource.Cancel();
			await task;
		}

		[Theory]
		[InlineData(true, "bc1_unorm")]
		[InlineData(true, "bc1a_unorm")]
		[InlineData(true, "bc7_unorm")]
		[InlineData(true, "bc6h_ufloat")]
		[InlineData(false, "bc1_unorm")]
		[InlineData(false, "bc1a_unorm")]
		[InlineData(false, "bc7_unorm")]
		[InlineData(false, "bc6h_ufloat")]
		public async Task TestDecodeRawBytesCancel(bool parallel, string name)
		{
			var decoder = MakeDecoder(parallel);

			var inputData = ImageLoader.LoadTestData(name);

			Assert.NotEqual(CompressionFormat.Unknown, inputData.Format);

			var cancelSource = new CancellationTokenSource();
			Task<OperationCanceledException> task;
			// Test
			#pragma warning disable xUnit2021
			if (inputData.Format.IsHdrFormat())
			{
				task = Assert.ThrowsAnyAsync<OperationCanceledException>(
					() => decoder.DecodeRawHdrAsync(
						inputData.MipLevels[0].Data,
						inputData.Width,
						inputData.Height,
						inputData.Format,
						cancelSource.Token));
			}
			else
			{
				task = Assert.ThrowsAnyAsync<OperationCanceledException>(
					() => decoder.DecodeRawLdrAsync(
						inputData.MipLevels[0].Data,
						inputData.Width,
						inputData.Height,
						inputData.Format,
						cancelSource.Token));
			}
			#pragma warning restore xUnit2021

			cancelSource.Cancel();
			await task;
		}

		[Theory]
		[InlineData(true, "bc1_unorm")]
		[InlineData(true, "bc1a_unorm")]
		[InlineData(true, "bc7_unorm")]
		[InlineData(true, "bc6h_ufloat")]
		[InlineData(false, "bc1_unorm")]
		[InlineData(false, "bc1a_unorm")]
		[InlineData(false, "bc7_unorm")]
		[InlineData(false, "bc6h_ufloat")]
		public async void TestDecodeCancel(bool parallel, string name)
		{
			var decoder = MakeDecoder(parallel);

			var inputData = ImageLoader.LoadTestData(name);

			Assert.NotEqual(CompressionFormat.Unknown, inputData.Format);

			var cancelSource = new CancellationTokenSource();

			// Test
			var task = Assert.ThrowsAnyAsync<OperationCanceledException>(() => decoder.DecodeAsync(inputData, cancelSource.Token));
			cancelSource.Cancel();
			await task;
		}
	}
}
