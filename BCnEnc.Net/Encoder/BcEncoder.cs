using System;
using System.IO;
using BCnEnc.Net.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Encoder
{
	public class EncoderInputOptions {
		/// <summary>
		/// If true, when encoding to a format that only includes a red channel,
		/// use the pixel luminance instead of just the red channel. Default is false.
		/// </summary>
		public bool luminanceAsRed = false;
	}

	public class EncoderOutputOptions
	{
		public bool generateMipMaps = true;
		public int maxMipMapLevel = -1;
		public CompressionFormat format = CompressionFormat.BC1;
		public EncodingQuality quality = EncodingQuality.Balanced;
	}

	public class BcEncoder
	{
		public EncoderInputOptions InputOptions { get; set; } = new EncoderInputOptions();
		public EncoderOutputOptions OutputOptions { get; set; } = new EncoderOutputOptions();


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
				case CompressionFormat.BC1WithAlpha:
					return new Bc1AlphaBlockEncoder();
				case CompressionFormat.BC2:
					return new Bc2BlockEncoder();
				case CompressionFormat.BC3:
					return new Bc3BlockEncoder();
				case CompressionFormat.BC4:
					return new Bc4BlockEncoder(InputOptions.luminanceAsRed);
				case CompressionFormat.BC5:
					return new Bc5BlockEncoder();
				default:
					return null;
			}
		}

		private IRawEncoder GetRawEncoder(CompressionFormat format)
		{
			switch (format) {
				case CompressionFormat.R:
					return new RawLuminanceEncoder(InputOptions.luminanceAsRed);
				case CompressionFormat.RG:
					return new RawRGEncoder();
				case CompressionFormat.RGB:
					return new RawRGBEncoder();
				case CompressionFormat.RGBA:
					return new RawRGBAEncoder();
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		public void Encode(Image<Rgba32> inputImage, Stream outputStream)
		{
			KtxFile output;
			IBcBlockEncoder compressedEncoder = null;
			IRawEncoder uncompressedEncoder = null;
			if (OutputOptions.format.IsCompressedFormat()) {
				compressedEncoder = GetEncoder(OutputOptions.format);
				if (compressedEncoder == null) {
					throw new NotSupportedException($"This format is not supported: {OutputOptions.format}");
				}
				output = new KtxFile(
					KtxHeader.InitializeCompressed(inputImage.Width, inputImage.Height,
						compressedEncoder.GetInternalFormat(), 
						compressedEncoder.GetBaseInternalFormat()));
			}
			else {
				uncompressedEncoder = GetRawEncoder(OutputOptions.format);
				output = new KtxFile(
					KtxHeader.InitializeUncompressed(inputImage.Width, inputImage.Height,
						uncompressedEncoder.GetGlType(),
						uncompressedEncoder.GetGlFormat(),
						uncompressedEncoder.GetGlTypeSize(),
						uncompressedEncoder.GetInternalFormat(),
						uncompressedEncoder.GetBaseInternalFormat()));

			}

			uint numMipMaps = (uint)OutputOptions.maxMipMapLevel;
			if (!OutputOptions.generateMipMaps) {
				numMipMaps = 1;
			}

			var mipChain = MipMapper.GenerateMipChain(inputImage, ref numMipMaps);

			for (int i = 0; i < numMipMaps; i++) {
				byte[] encoded = null;
				if (OutputOptions.format.IsCompressedFormat()) {
					var blocks = ImageToBlocks.ImageTo4X4(mipChain[i].Frames[0], out int blocksWidth, out int blocksHeight);
					encoded = compressedEncoder.Encode(blocks, blocksWidth, blocksHeight, OutputOptions.quality, true);
				}
				else {
					encoded = uncompressedEncoder.Encode(mipChain[i].GetPixelSpan());
				}

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

			output.Write(outputStream);
		}
	}
}
