using BCnEncoder.Shared;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests
{
	public class DdsWritingTests
	{
		[Fact]
		public void DdsWriteRgba()
		{
			TestHelper.ExecuteDdsWritingTest(ImageLoader.TestLenna, CompressionFormat.Rgba, "encoding_dds_rgba.dds");
		}

		[Fact]
		public void DdsWriteBc1()
		{
			TestHelper.ExecuteDdsWritingTest(ImageLoader.TestLenna, CompressionFormat.Bc1, "encoding_dds_bc1.dds");
		}

		[Fact]
		public void DdsWriteBc2()
		{
			TestHelper.ExecuteDdsWritingTest(ImageLoader.TestAlpha1, CompressionFormat.Bc2, "encoding_dds_bc2.dds");
		}

		[Fact]
		public void DdsWriteBc3()
		{
			TestHelper.ExecuteDdsWritingTest(ImageLoader.TestAlpha1, CompressionFormat.Bc3, "encoding_dds_bc3.dds");
		}

		[Fact]
		public void DdsWriteBc4()
		{
			TestHelper.ExecuteDdsWritingTest(ImageLoader.TestHeight1, CompressionFormat.Bc4, "encoding_dds_bc4.dds");
		}

		[Fact]
		public void DdsWriteBc5()
		{
			TestHelper.ExecuteDdsWritingTest(ImageLoader.TestRedGreen1, CompressionFormat.Bc5, "encoding_dds_bc5.dds");
		}

		[Fact]
		public void DdsWriteBc7()
		{
			TestHelper.ExecuteDdsWritingTest(ImageLoader.TestLenna, CompressionFormat.Bc7, "encoding_dds_bc7.dds");
		}

		[Fact]
		public void DdsWriteCubemap()
		{
			TestHelper.ExecuteDdsWritingTest(ImageLoader.TestCubemap, CompressionFormat.Bc1, "encoding_dds_cubemap_bc1.dds");
		}
	}
}
