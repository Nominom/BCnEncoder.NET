using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests.Api;

public class BcTextureDataTests
{
	[Fact]
	public void TestConversions()
	{
		var originalData = ImageLoader.TestEncodedImages["bc1_unorm"].Item1.ToTextureData();
		Assert.Equal(CompressionFormat.Bc1, originalData.Format);

		BcDecoder decoder = new BcDecoder();

		var decompressedRgba = decoder.Decode(originalData, CompressionFormat.Rgba32);
		Assert.Equal(CompressionFormat.Rgba32, decompressedRgba.Format);

		var convertedBgra = decompressedRgba.ConvertTo(CompressionFormat.Bgra32);
		Assert.Equal(CompressionFormat.Bgra32, convertedBgra.Format);

		for (var f = 0; f < decompressedRgba.NumFaces; f++)
		{
			for (var m = 0; m < decompressedRgba.NumMips; m++)
			{
				var mip = decompressedRgba.Mips[m][(CubeMapFaceDirection)f];
				var mip2 = convertedBgra.Mips[m][(CubeMapFaceDirection)f];

				Assert.Equal(mip2.SizeInBytes, mip.SizeInBytes);
				Assert.Equal(mip2.Data.Length, mip2.SizeInBytes);
				Assert.Equal(mip.Data.Length, mip.SizeInBytes);

				for (var i = 0; i < mip2.SizeInBytes; i += 4)
				{
					Assert.Equal(mip.Data[i+0], mip2.Data[i+2]);
					Assert.Equal(mip.Data[i+1], mip2.Data[i+1]);
					Assert.Equal(mip.Data[i+2], mip2.Data[i+0]);
					Assert.Equal(mip.Data[i+3], mip2.Data[i+3]);
				}
			}
		}


		var convertedRg = convertedBgra.ConvertTo(CompressionFormat.R8G8);
		Assert.Equal(CompressionFormat.R8G8, convertedRg.Format);
		for (var f = 0; f < convertedBgra.NumFaces; f++)
		{
			for (var m = 0; m < convertedBgra.NumMips; m++)
			{
				var mip = convertedBgra.Mips[m][(CubeMapFaceDirection)f];
				var mip2 = convertedRg.Mips[m][(CubeMapFaceDirection)f];

				Assert.Equal(mip2.SizeInBytes, mip.SizeInBytes / 2);
				Assert.Equal(mip2.Data.Length, mip2.SizeInBytes);
				Assert.Equal(mip.Data.Length, mip.SizeInBytes);

				for (var i = 0; i < mip2.Width * mip2.Height; i++)
				{
					Assert.Equal(mip.Data[i * 4 + 2], mip2.Data[i * 2 + 0]);
					Assert.Equal(mip.Data[i * 4 + 1], mip2.Data[i * 2 + 1]);
				}
			}
		}
	}
}
