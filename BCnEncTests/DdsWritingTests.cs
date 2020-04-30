using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using Xunit;

namespace BCnEncTests
{
	public class DdsWritingTests
	{
		[Fact]
		public void DdsWriteRgba() {
			var image = ImageLoader.testLenna;

			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Fast;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.RGBA;
			encoder.OutputOptions.fileFormat = OutputFileFormat.Dds;

			using FileStream fs = File.OpenWrite("encoding_dds_rgba.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteBc1() {
			var image = ImageLoader.testLenna;

			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Fast;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC1;
			encoder.OutputOptions.fileFormat = OutputFileFormat.Dds;

			using FileStream fs = File.OpenWrite("encoding_dds_bc1.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteBc2() {
			var image = ImageLoader.testAlpha1;

			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Fast;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC2;
			encoder.OutputOptions.fileFormat = OutputFileFormat.Dds;

			using FileStream fs = File.OpenWrite("encoding_dds_bc2.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteBc3() {
			var image = ImageLoader.testAlpha1;

			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Fast;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC3;
			encoder.OutputOptions.fileFormat = OutputFileFormat.Dds;

			using FileStream fs = File.OpenWrite("encoding_dds_bc3.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteBc4() {
			var image = ImageLoader.testHeight1;

			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Fast;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC4;
			encoder.OutputOptions.fileFormat = OutputFileFormat.Dds;

			using FileStream fs = File.OpenWrite("encoding_dds_bc4.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteBc5() {
			var image = ImageLoader.testRedGreen1;

			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Fast;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC5;
			encoder.OutputOptions.fileFormat = OutputFileFormat.Dds;

			using FileStream fs = File.OpenWrite("encoding_dds_bc5.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteBc7() {
			var image = ImageLoader.testLenna;

			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Fast;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC7;
			encoder.OutputOptions.fileFormat = OutputFileFormat.Dds;

			using FileStream fs = File.OpenWrite("encoding_dds_bc7.dds");
			encoder.Encode(image, fs);
			fs.Close();
		}

		[Fact]
		public void DdsWriteCubemap() {
			var images = ImageLoader.testCubemap;

			BcEncoder encoder = new BcEncoder();
			encoder.OutputOptions.quality = EncodingQuality.Fast;
			encoder.OutputOptions.generateMipMaps = true;
			encoder.OutputOptions.format = CompressionFormat.BC1;
			encoder.OutputOptions.fileFormat = OutputFileFormat.Dds;

			using FileStream fs = File.OpenWrite("encoding_dds_cubemap_bc1.dds");
			encoder.EncodeCubeMap(images[0],images[1],images[2],images[3],images[4],images[5], fs);
			fs.Close();
		}
	}
}
