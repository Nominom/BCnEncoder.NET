using System.IO;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using BCnEncTests.Support;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace BCnEncTests
{
	public class ImageSharpEncodingTests
	{
		private readonly BcEncoder encoder;
		private readonly BcDecoder decoder;
		private readonly Image<Rgba32> original;
		private readonly Image<Rgba32>[] cubemap;

		public ImageSharpEncodingTests()
		{
			encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			decoder = new BcDecoder();
			original = ImageLoader.LoadTestImageSharp("../../../testImages/test_gradient_1_512.jpg");
			cubemap = new[]
			{
				ImageLoader.LoadTestImageSharp("../../../testImages/cubemap/right.png"),
				ImageLoader.LoadTestImageSharp("../../../testImages/cubemap/left.png"),
				ImageLoader.LoadTestImageSharp("../../../testImages/cubemap/top.png"),
				ImageLoader.LoadTestImageSharp("../../../testImages/cubemap/bottom.png"),
				ImageLoader.LoadTestImageSharp("../../../testImages/cubemap/back.png"),
				ImageLoader.LoadTestImageSharp("../../../testImages/cubemap/forward.png"),
			};
		}

		[Fact]
		public void EncodeToKtx()
		{
			var ktx = encoder.EncodeToKtx(original);
			using var decoded = decoder.DecodeToImageRgba32(ktx);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void EncodeToDds()
		{
			var dds = encoder.EncodeToDds(original);
			using var decoded = decoder.DecodeToImageRgba32(dds);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void EncodeToStreamKtx()
		{
			encoder.OutputOptions.FileFormat = OutputFileFormat.Ktx;
			using var ms = new MemoryStream();
			encoder.EncodeToStream(original, ms);
			ms.Position = 0;
			using var decoded = decoder.DecodeToImageRgba32(ms);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void EncodeToStreamDds()
		{
			encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;
			using var ms = new MemoryStream();
			encoder.EncodeToStream(original, ms);
			ms.Position = 0;
			using var decoded = decoder.DecodeToImageRgba32(ms);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void EncodeToRawBytes()
		{
			var rawBytes = encoder.EncodeToRawBytes(original);
			using var decoded = decoder.DecodeRawToImageRgba32(rawBytes[0], original.Width, original.Height, CompressionFormat.Bc1);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void EncodeToRawBytesSingleMip()
		{
			var mip0 = encoder.EncodeToRawBytes(original, 0, out var mipWidth, out var mipHeight);
			Assert.Equal(original.Width, mipWidth);
			Assert.Equal(original.Height, mipHeight);
			Assert.True(mip0.Length > 0);
		}

		[Fact]
		public void CalculateMipLevelCount()
		{
			var fromImage = encoder.CalculateNumberOfMipLevels(original);
			var fromDims  = encoder.CalculateNumberOfMipLevels(original.Width, original.Height);
			Assert.Equal(fromDims, fromImage);
		}

		[Fact]
		public void CalculateMipMapSize()
		{
			encoder.CalculateMipMapSize(original, 0, out var w0, out var h0);
			Assert.Equal(original.Width,  w0);
			Assert.Equal(original.Height, h0);

			encoder.CalculateMipMapSize(original, 1, out var w1, out var h1);
			Assert.Equal(original.Width  / 2, w1);
			Assert.Equal(original.Height / 2, h1);
		}

		[Fact]
		public void EncodeCubeMapToKtx()
		{
			var ktx = encoder.EncodeCubeMapToKtx(cubemap[0], cubemap[1], cubemap[2], cubemap[3], cubemap[4], cubemap[5]);
			Assert.Equal(6, ktx.MipMaps[0].Faces.Length);
			for (var i = 0; i < 6; i++)
			{
				var face = ktx.MipMaps[0].Faces[i];
				using var decoded = decoder.DecodeRawToImageRgba32(face.Data, (int)face.Width, (int)face.Height, CompressionFormat.Bc1);
				TestHelper.AssertImagesEqual(cubemap[i], decoded, encoder.OutputOptions.Quality);
			}
		}

		[Fact]
		public void EncodeCubeMapToDds()
		{
			var dds = encoder.EncodeCubeMapToDds(cubemap[0], cubemap[1], cubemap[2], cubemap[3], cubemap[4], cubemap[5]);
			Assert.Equal(6, dds.Faces.Count);
			for (var i = 0; i < 6; i++)
			{
				var face = dds.Faces[i].MipMaps[0];
				using var decoded = decoder.DecodeRawToImageRgba32(face.Data, (int)face.Width, (int)face.Height, CompressionFormat.Bc1);
				TestHelper.AssertImagesEqual(cubemap[i], decoded, encoder.OutputOptions.Quality);
			}
		}

		[Fact]
		public void EncodeCubeMapToStream()
		{
			encoder.OutputOptions.FileFormat = OutputFileFormat.Ktx;
			using var ms = new MemoryStream();
			encoder.EncodeCubeMapToStream(cubemap[0], cubemap[1], cubemap[2], cubemap[3], cubemap[4], cubemap[5], ms);
			ms.Position = 0;
			var ktx = KtxFile.Load(ms);
			Assert.Equal(6, ktx.MipMaps[0].Faces.Length);
		}

		[Fact]
		public async Task EncodeToKtxAsync()
		{
			var ktx = await encoder.EncodeToKtxAsync(original);
			using var decoded = decoder.DecodeToImageRgba32(ktx);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task EncodeToDdsAsync()
		{
			var dds = await encoder.EncodeToDdsAsync(original);
			using var decoded = decoder.DecodeToImageRgba32(dds);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task EncodeToStreamAsync()
		{
			encoder.OutputOptions.FileFormat = OutputFileFormat.Ktx;
			using var ms = new MemoryStream();
			await encoder.EncodeToStreamAsync(original, ms);
			ms.Position = 0;
			using var decoded = decoder.DecodeToImageRgba32(ms);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task EncodeToRawBytesAsync()
		{
			var rawBytes = await encoder.EncodeToRawBytesAsync(original);
			using var decoded = decoder.DecodeRawToImageRgba32(rawBytes[0], original.Width, original.Height, CompressionFormat.Bc1);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task EncodeCubeMapToKtxAsync()
		{
			var ktx = await encoder.EncodeCubeMapToKtxAsync(cubemap[0], cubemap[1], cubemap[2], cubemap[3], cubemap[4], cubemap[5]);
			Assert.Equal(6, ktx.MipMaps[0].Faces.Length);
		}

		[Fact]
		public async Task EncodeCubeMapToDdsAsync()
		{
			var dds = await encoder.EncodeCubeMapToDdsAsync(cubemap[0], cubemap[1], cubemap[2], cubemap[3], cubemap[4], cubemap[5]);
			Assert.Equal(6, dds.Faces.Count);
		}

		[Fact]
		public async Task EncodeCubeMapToStreamAsync()
		{
			encoder.OutputOptions.FileFormat = OutputFileFormat.Ktx;
			using var ms = new MemoryStream();
			await encoder.EncodeCubeMapToStreamAsync(cubemap[0], cubemap[1], cubemap[2], cubemap[3], cubemap[4], cubemap[5], ms);
			ms.Position = 0;
			var ktx = KtxFile.Load(ms);
			Assert.Equal(6, ktx.MipMaps[0].Faces.Length);
		}
	}

	public class ImageSharpDecodingTests
	{
		private readonly BcEncoder encoder;
		private readonly BcDecoder decoder;
		private readonly Image<Rgba32> original;
		private readonly KtxFile encodedKtx;
		private readonly DdsFile encodedDds;
		private readonly byte[] rawEncoded;

		public ImageSharpDecodingTests()
		{
			encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			decoder = new BcDecoder();
			original = ImageLoader.LoadTestImageSharp("../../../testImages/test_gradient_1_512.jpg");
			encodedKtx  = encoder.EncodeToKtx(ImageLoader.TestGradient1);
			encodedDds  = encoder.EncodeToDds(ImageLoader.TestGradient1);
			rawEncoded  = encoder.EncodeToRawBytes(ImageLoader.TestGradient1)[0];
		}

		[Fact]
		public void DecodeKtxFile()
		{
			using var decoded = decoder.DecodeToImageRgba32(encodedKtx);
			Assert.Equal(original.Width,  decoded.Width);
			Assert.Equal(original.Height, decoded.Height);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void DecodeDdsFile()
		{
			using var decoded = decoder.DecodeToImageRgba32(encodedDds);
			Assert.Equal(original.Width,  decoded.Width);
			Assert.Equal(original.Height, decoded.Height);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void DecodeStream()
		{
			using var ms = new MemoryStream();
			encodedKtx.Write(ms);
			ms.Position = 0;
			using var decoded = decoder.DecodeToImageRgba32(ms);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void DecodeAllMipMapsKtx()
		{
			encoder.OutputOptions.GenerateMipMaps = true;
			var ktxWithMips = encoder.EncodeToKtx(ImageLoader.TestGradient1);
			var images = decoder.DecodeAllMipMapsToImageRgba32(ktxWithMips);

			Assert.Equal((int)ktxWithMips.header.NumberOfMipmapLevels, images.Length);
			Assert.True(images.Length > 1);
			TestHelper.AssertImagesEqual(original, images[0], encoder.OutputOptions.Quality);

			foreach (var img in images) img.Dispose();
		}

		[Fact]
		public void DecodeAllMipMapsDds()
		{
			encoder.OutputOptions.GenerateMipMaps = true;
			var ddsWithMips = encoder.EncodeToDds(ImageLoader.TestGradient1);
			var images = decoder.DecodeAllMipMapsToImageRgba32(ddsWithMips);

			Assert.Equal((int)ddsWithMips.header.dwMipMapCount, images.Length);
			Assert.True(images.Length > 1);

			foreach (var img in images) img.Dispose();
		}

		[Fact]
		public void DecodeAllMipMapsStream()
		{
			encoder.OutputOptions.GenerateMipMaps = true;
			var ktxWithMips = encoder.EncodeToKtx(ImageLoader.TestGradient1);
			using var ms = new MemoryStream();
			ktxWithMips.Write(ms);
			ms.Position = 0;

			var images = decoder.DecodeAllMipMapsToImageRgba32(ms);
			Assert.Equal((int)ktxWithMips.header.NumberOfMipmapLevels, images.Length);

			foreach (var img in images) img.Dispose();
		}

		[Fact]
		public void DecodeRaw()
		{
			using var decoded = decoder.DecodeRawToImageRgba32(rawEncoded, original.Width, original.Height, CompressionFormat.Bc1);
			Assert.Equal(original.Width,  decoded.Width);
			Assert.Equal(original.Height, decoded.Height);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public void DecodeRawStream()
		{
			using var ms = new MemoryStream(rawEncoded);
			using var decoded = decoder.DecodeRawToImageRgba32(ms, original.Width, original.Height, CompressionFormat.Bc1);
			Assert.Equal(original.Width,  decoded.Width);
			Assert.Equal(original.Height, decoded.Height);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task DecodeKtxFileAsync()
		{
			using var decoded = await decoder.DecodeToImageRgba32Async(encodedKtx);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task DecodeDdsFileAsync()
		{
			using var decoded = await decoder.DecodeToImageRgba32Async(encodedDds);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task DecodeStreamAsync()
		{
			using var ms = new MemoryStream();
			encodedKtx.Write(ms);
			ms.Position = 0;
			using var decoded = await decoder.DecodeToImageRgba32Async(ms);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task DecodeAllMipMapsKtxAsync()
		{
			encoder.OutputOptions.GenerateMipMaps = true;
			var ktxWithMips = encoder.EncodeToKtx(ImageLoader.TestGradient1);
			var images = await decoder.DecodeAllMipMapsToImageRgba32Async(ktxWithMips);

			Assert.Equal((int)ktxWithMips.header.NumberOfMipmapLevels, images.Length);
			Assert.True(images.Length > 1);

			foreach (var img in images) img.Dispose();
		}

		[Fact]
		public async Task DecodeAllMipMapsStreamAsync()
		{
			encoder.OutputOptions.GenerateMipMaps = true;
			var ktxWithMips = encoder.EncodeToKtx(ImageLoader.TestGradient1);
			using var ms = new MemoryStream();
			ktxWithMips.Write(ms);
			ms.Position = 0;

			var images = await decoder.DecodeAllMipMapsToImageRgba32Async(ms);
			Assert.Equal((int)ktxWithMips.header.NumberOfMipmapLevels, images.Length);

			foreach (var img in images) img.Dispose();
		}

		[Fact]
		public async Task DecodeRawAsync()
		{
			using var decoded = await decoder.DecodeRawToImageRgba32Async(rawEncoded, original.Width, original.Height, CompressionFormat.Bc1);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}

		[Fact]
		public async Task DecodeRawStreamAsync()
		{
			using var ms = new MemoryStream(rawEncoded);
			using var decoded = await decoder.DecodeRawToImageRgba32Async(ms, original.Width, original.Height, CompressionFormat.Bc1);
			TestHelper.AssertImagesEqual(original, decoded, encoder.OutputOptions.Quality);
		}
	}
}
