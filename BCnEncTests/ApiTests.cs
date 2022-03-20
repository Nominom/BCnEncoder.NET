using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests
{
	public class EncoderApiTests
	{
		private static BcEncoder MakeEncoder(CompressionFormat format)
		{
			var encoder = new BcEncoder(format);
			encoder.OutputOptions.Quality = CompressionQuality.Fast;
			encoder.OutputOptions.GenerateMipMaps = true;
			encoder.OutputOptions.MaxMipMapLevel = -1;
			return encoder;
		}

		private static BCnTextureData LoadTestFile(bool raw, string name)
		{
			if (raw)
			{
				return ImageLoader.TestRawImages[name].ToBCnTextureData();
			}
			else
			{
				return ImageLoader.TestEncodedImages[name].Item1.ToTextureData();
			}
		}

		private static void ValidateData(BcEncoder encoder, int inputWidth, int inputHeight, BCnTextureData outputData)
		{
			Assert.Equal(outputData.NumMips, encoder.CalculateNumberOfMipLevels(inputWidth, inputHeight));

			for (var f = 0; f < outputData.NumFaces; f++)
			{
				Assert.Equal((CubeMapFaceDirection)f, outputData.Faces[f].Direction);

				for (var m = 0; m < outputData.NumMips; m++)
				{
					var mip = outputData.Faces[f].Mips[m];
					encoder.CalculateMipMapSize(inputWidth, inputHeight, m, out var mW, out var mH);
					var byteSize = encoder.CalculateMipMapByteSize(inputWidth, inputHeight, m);
					Assert.Equal(mW, mip.Width);
					Assert.Equal(mH, mip.Height);
					Assert.Equal(byteSize, mip.SizeInBytes);
					Assert.Equal(byteSize, mip.Data.Length);
				}
			}
		}

		private static void AssertNonCube(CompressionFormat format, BCnTextureData outputData, BCnTextureData inputData)
		{
			Assert.True(outputData.HasMipLevels);
			Assert.False(outputData.IsCubeMap);
			Assert.Equal(1, outputData.NumFaces);
			Assert.Equal(format, outputData.Format);
			Assert.Equal(inputData.Width, outputData.Width);
			Assert.Equal(inputData.Height, outputData.Height);
			Assert.True(outputData.NumMips > 1);
			Assert.Equal(outputData.NumMips, outputData.MipLevels.Length);
		}

		[Theory]
		[InlineData(true, "rgba_1", CompressionFormat.Bc1)]
		[InlineData(true, "rg_1", CompressionFormat.Bc5)]
		[InlineData(false, "raw_r8g8b8_unorm", CompressionFormat.Bc1)]
		[InlineData(false, "bc1_unorm", CompressionFormat.Bc7)]
		[InlineData(false, "bc6h_ufloat", CompressionFormat.RgbaHalf)]
		[InlineData(false, "bc6h_ufloat", CompressionFormat.Rgb24)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Rgb24)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.RgbaHalf)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Bc6S)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Bc6U)]
		public void TestEncode(bool raw, string name, CompressionFormat format)
		{
			var encoder = MakeEncoder(format);

			var inputData = LoadTestFile(raw, name);
			
			Assert.True(inputData.Format != CompressionFormat.Unknown);

			// Test
			var outputData = encoder.Encode(inputData);

			AssertNonCube(format, outputData, inputData);
			ValidateData(encoder, inputData.Width, inputData.Height, outputData);
		}

		[Theory]
		[InlineData(true, "rgba_1", CompressionFormat.Bc1)]
		[InlineData(true, "rg_1", CompressionFormat.Bc5)]
		[InlineData(false, "raw_r8g8b8_unorm", CompressionFormat.Bc1)]
		[InlineData(false, "bc1_unorm", CompressionFormat.Bc7)]
		[InlineData(false, "bc6h_ufloat", CompressionFormat.RgbaHalf)]
		[InlineData(false, "bc6h_ufloat", CompressionFormat.Rgb24)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Rgb24)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.RgbaHalf)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Bc6S)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Bc6U)]
		public void TestEncodeBytes(bool raw, string name, CompressionFormat format)
		{
			var encoder = MakeEncoder(format);

			var inputData = LoadTestFile(raw, name);

			Assert.True(inputData.Format != CompressionFormat.Unknown);

			// Test
			var outputData = encoder.EncodeBytes(inputData.MipLevels[0].Data, inputData.Width, inputData.Height, inputData.Format);
			
			AssertNonCube(format, outputData, inputData);
			ValidateData(encoder, inputData.Width, inputData.Height, outputData);
		}

		[Theory]
		[InlineData("rgba_1", CompressionFormat.Bc1)]
		[InlineData("rgba_1", CompressionFormat.RgbaFloat)]
		public void TestEncodeLdr(string name, CompressionFormat format)
		{
			var encoder = MakeEncoder(format);

			var inputData = LoadTestFile(true, name);

			Assert.Equal(CompressionFormat.Rgba32, inputData.Format);

			// Test
			var outputData = encoder.Encode(inputData.MipLevels[0].AsMemory2D<ColorRgba32>());

			AssertNonCube(format, outputData, inputData);
			ValidateData(encoder, inputData.Width, inputData.Height, outputData);
		}

		[Theory]
		[InlineData("hdr_1_rgbe", CompressionFormat.Bc6U)]
		[InlineData("hdr_1_rgbe", CompressionFormat.RgbaFloat)]
		[InlineData("hdr_1_rgbe", CompressionFormat.Bc1)]
		public void TestEncodeHdr(string name, CompressionFormat format)
		{
			var encoder = MakeEncoder(format);

			var inputData = LoadTestFile(false, name).ConvertTo(CompressionFormat.RgbaFloat);

			Assert.Equal(CompressionFormat.RgbaFloat, inputData.Format);

			// Test
			var outputData = encoder.EncodeHdr(inputData.MipLevels[0].AsMemory2D<ColorRgbaFloat>());

			AssertNonCube(format, outputData, inputData);

			ValidateData(encoder, inputData.Width, inputData.Height, outputData);
		}

		[Theory]
		[InlineData(true, "rgba_1", CompressionFormat.Bc1)]
		[InlineData(true, "rg_1", CompressionFormat.Bc5)]
		[InlineData(false, "raw_r8g8b8_unorm", CompressionFormat.Bc1)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Rgb24)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.RgbaHalf)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Bc6S)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Bc6U)]
		public void TestEncodeToRawBytes(bool raw, string name, CompressionFormat format)
		{
			var encoder = MakeEncoder(format);

			var inputData = LoadTestFile(raw, name);

			Assert.NotEqual(CompressionFormat.Unknown, inputData.Format);

			var outputData = new BCnTextureData(format, inputData.Width, inputData.Height,
				encoder.CalculateNumberOfMipLevels(inputData.Width, inputData.Height));

			// Test
			for (var m = 0; m < outputData.NumMips; m++)
			{
				outputData.MipLevels[m].Data =
					encoder.EncodeToRawBytes(inputData, m, out var mipWidth, out var mipHeight);

				Assert.Equal(outputData.MipLevels[m].Width, mipWidth);
				Assert.Equal(outputData.MipLevels[m].Height, mipHeight);
			}
			
			AssertNonCube(format, outputData, inputData);
			ValidateData(encoder, inputData.Width, inputData.Height, outputData);
		}

		[Theory]
		[InlineData(true, "rgba_1", CompressionFormat.Bc1)]
		[InlineData(true, "rg_1", CompressionFormat.Bc5)]
		[InlineData(false, "raw_r8g8b8_unorm", CompressionFormat.Bc1)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Rgb24)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.RgbaHalf)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Bc6S)]
		[InlineData(false, "hdr_1_rgbe", CompressionFormat.Bc6U)]
		public void TestEncodeBytesToRawBytes(bool raw, string name, CompressionFormat format)
		{
			var encoder = MakeEncoder(format);

			var inputData = LoadTestFile(raw, name);

			Assert.NotEqual(CompressionFormat.Unknown, inputData.Format);

			var outputData = new BCnTextureData(format, inputData.Width, inputData.Height,
				encoder.CalculateNumberOfMipLevels(inputData.Width, inputData.Height));

			// Test
			for (var m = 0; m < outputData.NumMips; m++)
			{
				outputData.MipLevels[m].Data =
					encoder.EncodeBytesToRawBytes(inputData.MipLevels[0].Data, inputData.Width, inputData.Height, inputData.Format, m, out var mipWidth, out var mipHeight);

				Assert.Equal(outputData.MipLevels[m].Width, mipWidth);
				Assert.Equal(outputData.MipLevels[m].Height, mipHeight);
			}

			AssertNonCube(format, outputData, inputData);
			ValidateData(encoder, inputData.Width, inputData.Height, outputData);
		}

		[Theory]
		[InlineData("rgba_1", CompressionFormat.Bc1)]
		[InlineData("rgba_1", CompressionFormat.RgbaFloat)]
		public void TestEncodeToRawBytesLdr(string name, CompressionFormat format)
		{
			var encoder = MakeEncoder(format);

			var inputData = LoadTestFile(true, name);

			Assert.Equal(CompressionFormat.Rgba32, inputData.Format);

			var outputData = new BCnTextureData(format, inputData.Width, inputData.Height,
				encoder.CalculateNumberOfMipLevels(inputData.Width, inputData.Height));

			// Test
			for (var m = 0; m < outputData.NumMips; m++)
			{
				outputData.MipLevels[m].Data =
					encoder.EncodeToRawBytes(inputData.MipLevels[0].AsMemory2D<ColorRgba32>(), m, out var mipWidth, out var mipHeight);

				Assert.Equal(outputData.MipLevels[m].Width, mipWidth);
				Assert.Equal(outputData.MipLevels[m].Height, mipHeight);
			}

			AssertNonCube(format, outputData, inputData);
			ValidateData(encoder, inputData.Width, inputData.Height, outputData);
		}

		[Theory]
		[InlineData("hdr_1_rgbe", CompressionFormat.Bc6U)]
		[InlineData("hdr_1_rgbe", CompressionFormat.RgbaFloat)]
		[InlineData("hdr_1_rgbe", CompressionFormat.Bc1)]
		public void TestEncodeToRawBytesHdr(string name, CompressionFormat format)
		{
			var encoder = MakeEncoder(format);

			var inputData = LoadTestFile(false, name).ConvertTo(CompressionFormat.RgbaFloat);

			Assert.Equal(CompressionFormat.RgbaFloat, inputData.Format);

			var outputData = new BCnTextureData(format, inputData.Width, inputData.Height,
				encoder.CalculateNumberOfMipLevels(inputData.Width, inputData.Height));

			// Test
			for (var m = 0; m < outputData.NumMips; m++)
			{
				outputData.MipLevels[m].Data =
					encoder.EncodeToRawBytesHdr(inputData.MipLevels[0].AsMemory2D<ColorRgbaFloat>(), m, out var mipWidth, out var mipHeight);

				Assert.Equal(outputData.MipLevels[m].Width, mipWidth);
				Assert.Equal(outputData.MipLevels[m].Height, mipHeight);
			}

			AssertNonCube(format, outputData, inputData);
			ValidateData(encoder, inputData.Width, inputData.Height, outputData);
		}
	}
}
