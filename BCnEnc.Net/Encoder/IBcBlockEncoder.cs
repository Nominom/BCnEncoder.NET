using System;
using BCnEnc.Net.Shared;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Encoder
{
	internal interface IBcBlockEncoder
	{
		/// <summary>
		/// Only used in Bc7
		/// </summary>
		void SetReferenceData(ReadOnlySpan<Rgba32> originalPixels, int pixelWidth, int pixelHeight) {}
		byte[] Encode(RawBlock4X4Rgba32[] blocks, int blockWidth, int blockHeight, EncodingQuality quality, bool parallel = true);
		GlInternalFormat GetInternalFormat();
		GLFormat GetBaseInternalFormat();
		DXGI_FORMAT GetDxgiFormat();
	}


}
