using System.IO;
using BCnEncoder.TextureFormats;
using Xunit;

namespace BCnEncTests.FileFormats
{
	public class FileFormatTests
	{
		[Theory]
		[InlineData("../../../testImages/encoded/raw_r8_unorm.ktx")]
		[InlineData("../../../testImages/encoded/raw_r8g8_unorm.ktx")]
		[InlineData("../../../testImages/encoded/raw_r8g8b8_unorm.ktx")]
		[InlineData("../../../testImages/encoded/raw_r16g16b16_sfloat.ktx")]
		[InlineData("../../../testImages/encoded/bc1a_unorm.ktx")]
		[InlineData("../../../testImages/encoded/bc2_unorm.ktx")]
		public void TestReadWriteKtxBitExact(string testImageFile)
		{
			var originalStream = new MemoryStream();
			using var ofs = File.OpenRead(testImageFile);
			ofs.CopyTo(originalStream);

			originalStream.Seek(0, SeekOrigin.Begin);
			var ktx1 = KtxFile.Load(originalStream);

			var ktx2 = ktx1.ToTextureData().AsTexture<KtxFile>();

			using var newStream = new MemoryStream();
			ktx2.WriteToStream(newStream);

			newStream.Seek(0, SeekOrigin.Begin);
			originalStream.Seek(0, SeekOrigin.Begin);

			Assert.Equal(newStream.Length, originalStream.Length);
			Assert.Equal(newStream.ToArray(), originalStream.ToArray());
		}

		[Theory]
		[InlineData("../../../testImages/encoded/alpha_1_bc1a.dds")]
		[InlineData("../../../testImages/encoded/alpha_1_bc3.dds")]
		[InlineData("../../../testImages/encoded/alpha_1_bgra.dds")]
		[InlineData("../../../testImages/encoded/alpha_2_rgba.dds")]
		[InlineData("../../../testImages/encoded/alpha_2_bc3.dds")]
		public void TestReadWriteDdsBitExact(string testImageFile)
		{
			var originalStream = new MemoryStream();
			using var ofs = File.OpenRead(testImageFile);
			ofs.CopyTo(originalStream);

			originalStream.Seek(0, SeekOrigin.Begin);
			var dds1 = DdsFile.Load(originalStream);

			var dds2 = new DdsFile();
			dds2.FromTextureData(dds1.ToTextureData());

			using var newStream = new MemoryStream();
			dds2.WriteToStream(newStream);

			newStream.Seek(0, SeekOrigin.Begin);
			originalStream.Seek(0, SeekOrigin.Begin);

			Assert.Equal(newStream.Length, originalStream.Length);
			Assert.Equal(newStream.ToArray(), originalStream.ToArray());
		}
	}
}
