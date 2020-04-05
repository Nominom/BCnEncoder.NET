using System;
using BCnComp.Net.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnComp.Net.Decoder
{
	public class BcDecoder
	{
		private IBcBlockDecoder GetDecoder(GlInternalFormat format) {
			switch (format) {
				case GlInternalFormat.GL_COMPRESSED_RGB_S3TC_DXT1_EXT:
					return new Bc1NoAlphaDecoder();
				case GlInternalFormat.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT:
					return new Bc1ADecoder();
				default:
					return null;
			}
		}
		public Image<Rgba32> Decode(KtxFile file) {
			var decoder = GetDecoder(file.Header.GlInternalFormat);
			if (decoder == null) {
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
