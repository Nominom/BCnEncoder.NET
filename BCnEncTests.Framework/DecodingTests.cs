using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests
{
	public class DecodingTests
	{
		[Fact]
		public void Bc1Decode()
		{
			TestHelper.ExecuteDecodingTest(KtxLoader.TestDecompressBc1, "decoding_test_bc1.png");
		}

		[Fact]
		public void Bc1AlphaDecode()
		{
			TestHelper.ExecuteDecodingTest(KtxLoader.TestDecompressBc1A, "decoding_test_bc1a.png");
		}

		[Fact]
		public void Bc2Decode()
		{
			TestHelper.ExecuteDecodingTest(KtxLoader.TestDecompressBc2, "decoding_test_bc2.png");
		}

		[Fact]
		public void Bc3Decode()
		{
			TestHelper.ExecuteDecodingTest(KtxLoader.TestDecompressBc3, "decoding_test_bc3.png");
		}

		[Fact]
		public void Bc4Decode()
		{
			TestHelper.ExecuteDecodingTest(KtxLoader.TestDecompressBc4Unorm, "decoding_test_bc4.png");
		}

		[Fact]
		public void Bc5Decode()
		{
			TestHelper.ExecuteDecodingTest(KtxLoader.TestDecompressBc5Unorm, "decoding_test_bc5.png");
		}

		[Fact]
		public void Bc7DecodeRgb()
		{
			TestHelper.ExecuteDecodingTest(KtxLoader.TestDecompressBc7Rgb, "decoding_test_bc7_rgb.png");
		}

		[Fact]
		public void Bc7DecodeUnorm()
		{
			TestHelper.ExecuteDecodingTest(KtxLoader.TestDecompressBc7Unorm, "decoding_test_bc7_unorm.png");
		}

		[Fact]
		public void Bc7DecodeEveryBlockType()
		{
			TestHelper.ExecuteDecodingTest(KtxLoader.TestDecompressBc7Types, "decoding_test_bc7_types.png");
		}
	}
}
