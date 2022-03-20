using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.TextureFormats;
using BCnEncTests.Support;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests
{
	public class ProgressTests
	{
		private readonly ITestOutputHelper output;

		public ProgressTests(ITestOutputHelper output) => this.output = output;

		private async Task ExecuteEncodeProgressReport(BcEncoder encoder, Image<Rgba32> testImage)
		{
			var lastProgress = new ProgressElement(0, 1);
			
			encoder.Options.Progress = new SynchronousProgress<ProgressElement>(element =>
			{
				Assert.True(lastProgress.CurrentBlock < element.CurrentBlock);
				lastProgress = element;
			});

			var numMips = encoder.CalculateNumberOfMipLevels(testImage.Width, testImage.Height);

			var expectedTotalBlocks = 0L;

			for (var i = 0; i < numMips; i++)
			{
				encoder.CalculateMipMapSize(testImage.Width, testImage.Height, i, out var mW, out var mH);
				expectedTotalBlocks += encoder.OutputOptions.Format.CalculateMipByteSize(mW, mH) / encoder.OutputOptions.Format.BytesPerBlock();
			}


			await using var ms = new MemoryStream();
			await encoder.EncodeToStreamAsync<DdsFile>(testImage, ms);

			output.WriteLine("LastProgress = " + lastProgress);

			Assert.Equal(expectedTotalBlocks, lastProgress.TotalBlocks);
			Assert.Equal(1, lastProgress.Percentage);
		}

		private async Task ExecuteDecodeProgressReport<T>(BcDecoder decoder, T image)
			where T : ITextureFileFormat
		{
			var lastProgress = new ProgressElement(0, 1);
			
			decoder.Options.Progress = new SynchronousProgress<ProgressElement>(element =>
			{
				Assert.True(lastProgress.CurrentBlock < element.CurrentBlock);
				lastProgress = element;
			});

			var bcnData = image.ToTextureData();

			var expectedTotalBlocks = bcnData.MipLevels.Sum(m =>
				bcnData.Format.CalculateMipByteSize(m.Width, m.Height)) / bcnData.Format.BytesPerBlock();

			await decoder.DecodeAsync(bcnData);

			output.WriteLine("LastProgress = " + lastProgress);

			Assert.Equal(expectedTotalBlocks, lastProgress.TotalBlocks);
			Assert.Equal(1, lastProgress.Percentage);
		}

		private async Task ExecuteEncodeSingleMipProgressReport(BcEncoder encoder, Image<Rgba32> testImage, int mipLevel)
		{
			var lastProgress = new ProgressElement(0, 1);
			
			encoder.Options.Progress = new SynchronousProgress<ProgressElement>(element =>
			{
				Assert.True(lastProgress.CurrentBlock < element.CurrentBlock);
				lastProgress = element;
			});

			encoder.CalculateMipMapSize(testImage.Width, testImage.Height, mipLevel, out var mW, out var mH);
			var expectedTotalBlocks = encoder.OutputOptions.Format.CalculateMipByteSize(mW, mH) / encoder.OutputOptions.Format.BytesPerBlock();

			await using var ms = new MemoryStream();
			await encoder.EncodeToRawBytesAsync(testImage, mipLevel);

			output.WriteLine("LastProgress = " + lastProgress);

			Assert.Equal(expectedTotalBlocks, lastProgress.TotalBlocks);
			Assert.Equal(1, lastProgress.Percentage);
		}

		private async Task ExecuteDecodeSingleMipProgressReport<T>(BcDecoder decoder, T image, int mipLevel)
			where T : ITextureFileFormat
		{
			var lastProgress = new ProgressElement(0, 1);
			
			decoder.Options.Progress = new SynchronousProgress<ProgressElement>(element =>
			{
				Assert.True(lastProgress.CurrentBlock < element.CurrentBlock);
				lastProgress = element;
			});

			var bcnData = image.ToTextureData();
			var expectedTotalBlocks = bcnData.Format.CalculateMipByteSize(bcnData.MipLevels[mipLevel].Width, bcnData.MipLevels[mipLevel].Height) / bcnData.Format.BytesPerBlock();

			await decoder.DecodeRawLdrAsync(bcnData.MipLevels[mipLevel].Data, bcnData.MipLevels[mipLevel].Width, bcnData.MipLevels[mipLevel].Height, bcnData.Format);

			output.WriteLine("LastProgress = " + lastProgress);

			Assert.Equal(expectedTotalBlocks, lastProgress.TotalBlocks);
			Assert.Equal(1, lastProgress.Percentage);
		}



		[Theory]
		[InlineData("bc1_unorm", true)]
		[InlineData("bc1_unorm", false)]
		[InlineData("alpha_1_bgra", true)]
		[InlineData("alpha_1_bgra", false)]
		public async void DecodeProgress(string decodeFile, bool parallel)
		{
			var testImage = ImageLoader.TestEncodedImages[decodeFile].Item1;
			var decoder = new BcDecoder
			{
				Options = { IsParallel = parallel }
			};
			
			await ExecuteDecodeProgressReport(decoder, testImage);
			await ExecuteDecodeSingleMipProgressReport(decoder, testImage, 0);
			await ExecuteDecodeSingleMipProgressReport(decoder, testImage, 1);
			await ExecuteDecodeSingleMipProgressReport(decoder, testImage, 2);
		}

		[Theory]
		[InlineData("diffuse_1", true, CompressionFormat.Bc1)]
		[InlineData("diffuse_1", false, CompressionFormat.Bc1)]
		[InlineData("diffuse_1", true, CompressionFormat.R8G8)]
		[InlineData("diffuse_1", false, CompressionFormat.R8G8)]
		public async void EncodeProgressReport(string testRaw, bool parallel, CompressionFormat format)
		{
			var testImage = ImageLoader.TestRawImages[testRaw];
			var encoder = new BcEncoder(format)
			{
				Options = { IsParallel = parallel },
				OutputOptions = { Quality = CompressionQuality.Fast }
			};
			
			await ExecuteEncodeProgressReport(encoder, testImage);
			await ExecuteEncodeSingleMipProgressReport(encoder, testImage, 0);
			await ExecuteEncodeSingleMipProgressReport(encoder, testImage, 1);
			await ExecuteEncodeSingleMipProgressReport(encoder, testImage, 2);
			await ExecuteEncodeSingleMipProgressReport(encoder, testImage, 3);
		}
	}

	class SynchronousProgress<T> : IProgress<T>
	{
		private readonly Action<T> handler;

		public SynchronousProgress(Action<T> handler)
		{
			this.handler = handler;
		}

		public void Report(T value)
		{
			handler.Invoke(value);
		}
	}
}
