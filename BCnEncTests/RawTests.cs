using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using Xunit;

namespace BCnEncTests
{
    public class RawTests
    {
        [Fact]
        public void Decode()
        {
            using var fs = File.OpenRead(@"../../../testImages/test_decompress_bc1.ktx");
            var ktx = KtxFile.Load(fs);
            var decoder = new BcDecoder();
            var encoder = new BcEncoder();

            var originalImage = decoder.Decode(ktx);

            var rawBytes = encoder.EncodeToRawBytes(originalImage);
            var recodedImage = decoder.DecodeRaw(rawBytes[0], CompressionFormat.BC1, originalImage.Width, originalImage.Height);

            originalImage.TryGetSinglePixelSpan(out var originalPixels);
            recodedImage.TryGetSinglePixelSpan(out var recodedPixels);

            var psnr=ImageQuality.PeakSignalToNoiseRatio(originalPixels, recodedPixels);
            if (encoder.OutputOptions.quality == CompressionQuality.Fast)
            {
                Assert.True(psnr > 25);
            }
            else
            {
                Assert.True(psnr > 30);
            }
        }
    }
}
