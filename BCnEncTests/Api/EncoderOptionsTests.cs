using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.TextureFormats;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests.Api;

public class EncoderOptionsTests
{

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(5)]
	public void MaxMipMaps(int requestedMipMaps)
	{
		var testImage = ImageLoader.TestRawImages["rgba_1"];
		var encoder = new BcEncoder()
		{
			OutputOptions =
			{
				GenerateMipMaps = true,
				MaxMipMapLevel = requestedMipMaps
			}
		};

		Assert.Equal(requestedMipMaps, encoder.CalculateNumberOfMipLevels(testImage.Width, testImage.Height, 1));

		var bcnData = encoder.Encode(testImage);

		Assert.Equal(requestedMipMaps, bcnData.NumMips);
		Assert.Equal(requestedMipMaps, bcnData.Mips.Length);

		var ktx = encoder.EncodeToTexture<KtxFile>(testImage);

		Assert.Equal(requestedMipMaps, (int)ktx.header.NumberOfMipmapLevels);
		Assert.Equal(requestedMipMaps, ktx.MipMaps.Count);

		var dds = encoder.EncodeToTexture<DdsFile>(testImage);

		Assert.Equal(requestedMipMaps, (int)dds.header.dwMipMapCount);
		Assert.Equal(requestedMipMaps, dds.ArrayElements[0].MipMaps.Length);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void GenerateMipMaps(bool generateMipMaps)
	{
		var testImage = ImageLoader.TestRawImages["rgba_1"];
		int requestedMipMaps = generateMipMaps ?
			MipMapper.CalculateMipChainLength(testImage.Width, testImage.Height, 1, 0) :
			1;
		var encoder = new BcEncoder()
		{
			OutputOptions =
			{
				GenerateMipMaps = generateMipMaps
			}
		};

		Assert.Equal(requestedMipMaps, encoder.CalculateNumberOfMipLevels(testImage.Width, testImage.Height));

		var bcnData = encoder.Encode(testImage);

		Assert.Equal(requestedMipMaps, bcnData.NumMips);
		Assert.Equal(requestedMipMaps, bcnData.Mips.Length);

		var ktx = encoder.EncodeToTexture<KtxFile>(testImage);

		Assert.Equal(requestedMipMaps, (int)ktx.header.NumberOfMipmapLevels);
		Assert.Equal(requestedMipMaps, (int)ktx.MipMaps.Count);

		var dds = encoder.EncodeToTexture<DdsFile>(testImage);

		Assert.Equal(requestedMipMaps, (int)dds.header.dwMipMapCount);
		Assert.Equal(requestedMipMaps, dds.ArrayElements[0].MipMaps.Length);
	}
}