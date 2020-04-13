using System;
using System.Diagnostics;
using BCnEnc.Net.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Decoder
{
	public class DecoderOutputOptions {
		/// <summary>
		/// If true, when decoding from a format that only includes a red channel,
		/// output pixels will have all colors set to the same value (greyscale). Default is true.
		/// </summary>
		public bool redAsLuminance = true;
	}

	public class BcDecoder
	{
		public DecoderOutputOptions OutputOptions { get; set; } = new DecoderOutputOptions();

		private bool IsSupportedRawFormat(GlInternalFormat format)
		{
			switch (format)
			{
				case GlInternalFormat.GL_R8:
				case GlInternalFormat.GL_RG8:
				case GlInternalFormat.GL_RGB8:
				case GlInternalFormat.GL_RGBA8:
					return true;
				default:
					return false;
			}
		}

		private IBcBlockDecoder GetDecoder(GlInternalFormat format)
		{
			switch (format)
			{
				case GlInternalFormat.GL_COMPRESSED_RGB_S3TC_DXT1_EXT:
					return new Bc1NoAlphaDecoder();
				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT:
					return new Bc1ADecoder();
				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT:
					return new Bc2Decoder();
				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT:
					return new Bc3Decoder();
				case GlInternalFormat.GL_COMPRESSED_RED_RGTC1_EXT:
					return new Bc4Decoder(OutputOptions.redAsLuminance);
				case GlInternalFormat.GL_COMPRESSED_RED_GREEN_RGTC2_EXT:
					return new Bc5Decoder();
				case GlInternalFormat.GL_COMPRESSED_RGBA_BPTC_UNORM_ARB:
					return new Bc7Decoder();
				default:
					return null;
			}
		}

		private IRawDecoder GetRawDecoder(GlInternalFormat format)
		{
			switch (format)
			{
				case GlInternalFormat.GL_R8:
					return new RawRDecoder(OutputOptions.redAsLuminance);
				case GlInternalFormat.GL_RG8:
					return new RawRGDecoder();
				case GlInternalFormat.GL_RGB8:
					return new RawRGBDecoder();
				case GlInternalFormat.GL_RGBA8:
					return new RawRGBADecoder();
				default:
					return null;
			}
		}

		public Image<Rgba32> Decode(KtxFile file)
		{
			if (IsSupportedRawFormat(file.Header.GlInternalFormat)) {
				var decoder = GetRawDecoder(file.Header.GlInternalFormat);
				var data = file.MipMaps[0].Faces[0].Data;
				var pixelWidth = file.MipMaps[0].Width;
				var pixelHeight = file.MipMaps[0].Height;

				var image = new Image<Rgba32>((int)pixelWidth, (int)pixelHeight);
				var output = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight);
				var pixels = image.GetPixelSpan();

				output.CopyTo(pixels);
				return image;
			}
			else {
				var decoder = GetDecoder(file.Header.GlInternalFormat);
				if (decoder == null)
				{
					throw new NotSupportedException($"This format is not supported: {file.Header.GlInternalFormat}");
				}

				var data = file.MipMaps[0].Faces[0].Data;
				var pixelWidth = file.MipMaps[0].Width;
				var pixelHeight = file.MipMaps[0].Height;

				var blocks = decoder.Decode(data, (int)pixelWidth, (int)pixelHeight, out var blockWidth, out var blockHeight);

				return ImageToBlocks.ImageFromRawBlocks(blocks, blockWidth, blockHeight);
			}
		}
	}
}
