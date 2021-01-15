using System.IO;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using Xunit;

namespace BCnEncTests
{
	public class DdsWritingTests
	{
		[Fact]
		public void DdsWriteRgba()
		{
			var image = ImageLoader.TestLenna;

			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Rgba;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

			using var fs = File.OpenWrite("encoding_dds_rgba.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteBc1()
		{
			var image = ImageLoader.TestLenna;

			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc1;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

			using var fs = File.OpenWrite("encoding_dds_bc1.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteBc2()
		{
			var image = ImageLoader.TestAlpha1;

			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc2;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

			using var fs = File.OpenWrite("encoding_dds_bc2.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteBc3()
		{
			var image = ImageLoader.TestAlpha1;

			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc3;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

			using var fs = File.OpenWrite("encoding_dds_bc3.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteBc4()
		{
			var image = ImageLoader.TestHeight1;

			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc4;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

			using var fs = File.OpenWrite("encoding_dds_bc4.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteBc5()
		{
			var image = ImageLoader.TestRedGreen1;

			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc5;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

			using var fs = File.OpenWrite("encoding_dds_bc5.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteBc7()
		{
			var image = ImageLoader.TestLenna;

			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc7;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

			using var fs = File.OpenWrite("encoding_dds_bc7.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteCubemap()
		{
			var images = ImageLoader.TestCubemap;

			var encoder = new BcEncoder();
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.Format = CompressionFormat.Bc1;
			encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

			using var fs = File.OpenWrite("encoding_dds_cubemap_bc1.dds");
			encoder.EncodeCubeMap(images[0], images[1], images[2], images[3], images[4], images[5], fs);
			fs.Close();
		}
	}
}
