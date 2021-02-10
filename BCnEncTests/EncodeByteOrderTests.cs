using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.NET.ImageSharp;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace BCnEncTests
{
	public class EncodeByteOrderTests
	{

		private void EncodeByteOrderTest<T>(PixelFormat pixelFormat) where T : unmanaged, IPixel<T>
		{
			var imageOrig = ImageLoader.TestAlpha1;
			var testImage = imageOrig.CloneAs<T>();
			
			var pixels = testImage.GetPixelMemoryGroup()[0];
			var bytes = MemoryMarshal.AsBytes(pixels.Span);

			var encoder = new BcEncoder(CompressionFormat.Bc3);
			var decoder = new BcDecoder();

			using var ms = new MemoryStream();

			encoder.EncodeToStream(bytes, testImage.Width, testImage.Height, pixelFormat, ms);

			ms.Position = 0;

			var decoded = decoder.DecodeToImageRgba32(ms);

			bool hasNoAlpha = pixelFormat == PixelFormat.Rgb24 || pixelFormat == PixelFormat.Bgr24;

			TestHelper.AssertImagesEqual(imageOrig, decoded, encoder.OutputOptions.Quality,
				!hasNoAlpha);
		}

		[Fact]
		public void EncodeByteOrderBgra()
		{
			EncodeByteOrderTest<Bgra32>(PixelFormat.Bgra32);
		}

		[Fact]
		public void EncodeByteOrderBgr()
		{
			EncodeByteOrderTest<Bgr24>(PixelFormat.Bgr24);
		}

		[Fact]
		public void EncodeByteOrderArgb()
		{
			EncodeByteOrderTest<Argb32>(PixelFormat.Argb32);
		}

		[Fact]
		public void EncodeByteOrderRgba()
		{
			EncodeByteOrderTest<Rgba32>(PixelFormat.Rgba32);
		}

		[Fact]
		public void EncodeByteOrderRgb24()
		{
			EncodeByteOrderTest<Rgb24>(PixelFormat.Rgb24);
		}
	}
}
