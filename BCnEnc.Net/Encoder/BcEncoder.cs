using System;
using System.IO;
using BCnEnc.Net.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Encoder
{
	public class CompressorInputOptions
	{

	}

	public class CompressorOutputOptions
	{
		public bool generateMipMaps = true;
		public int maxMipMapLevel = -1;
		public CompressionFormat format = CompressionFormat.BC1;
		public EncodingQuality quality = EncodingQuality.Balanced;
	}

	public class BcEncoder
	{
		public CompressorInputOptions InputOptions { get; set; } = new CompressorInputOptions();
		public CompressorOutputOptions OutputOptions { get; set; } = new CompressorOutputOptions();


		public BcEncoder() { }
		public BcEncoder(CompressionFormat format)
		{
			OutputOptions.format = format;
		}

		private IBcBlockEncoder GetEncoder(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.BC1:
					return new Bc1BlockEncoder();
				default:
					return null;
			}
		}

		public void Encode(Image<Rgba32> inputImage, Stream outputStream)
		{
			var encoder = GetEncoder(OutputOptions.format);
			if (encoder == null)
			{
				throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
			}

			KtxFile output = new KtxFile(
				KtxHeader.InitializeCompressed(inputImage.Width, inputImage.Height,
					encoder.GetInternalFormat(), 
					encoder.GetBaseInternalFormat()));

			uint numMipMaps = (uint)OutputOptions.maxMipMapLevel;
			if (!OutputOptions.generateMipMaps) {
				numMipMaps = 1;
			}

			var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

			for (int i = 0; i < numMipMaps; i++) {
				var blocks = ImageToBlocks.ImageTo4X4(mipChain[i].Frames[0], out int blocksWidth, out int blocksHeight);
				var encoded = encoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality);

				output.MipMaps.Add(new KtxMipmap((uint)encoded.Length, 
					(uint)inputImage.Width, 
					(uint)inputImage.Height, 1));
				output.MipMaps[i].Faces[0] = new KtxMipFace(encoded, 
					(uint)inputImage.Width, 
					(uint)inputImage.Height);
			}

			foreach (var image in mipChain) {
				image.Dispose();
			}

			output.Header.NumberOfFaces = 1;
			output.Header.NumberOfMipmapLevels = 1;

			output.Write(outputStream);
		}
	}
}
