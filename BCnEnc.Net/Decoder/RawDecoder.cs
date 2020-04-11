using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Decoder
{
	internal interface IRawDecoder {
		Rgba32[] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight);
	}

	public class RawRDecoder : IRawDecoder {
		private readonly bool redAsLuminance;
		public RawRDecoder(bool redAsLuminance) {
			this.redAsLuminance = redAsLuminance;
		}

		public Rgba32[] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight) {
			Rgba32[] output = new Rgba32[pixelWidth * pixelHeight];
			for (int i = 0; i < output.Length; i++) {
				if (redAsLuminance) {
					output[i].R = data[i];
					output[i].G = data[i];
					output[i].B = data[i];
				}
				else {
					output[i].R = data[i];
					output[i].G = 0;
					output[i].B = 0;
				}
				output[i].A = 255;
			}
			return output;
		}
	}

	public class RawRGDecoder : IRawDecoder
	{
		public Rgba32[] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight) {
			Rgba32[] output = new Rgba32[pixelWidth * pixelHeight];
			for (int i = 0; i < output.Length; i++) {
				output[i].R = data[i * 2];
				output[i].G = data[i * 2 + 1];
				output[i].B = 0;
				output[i].A = 255;
			}
			return output;
		}
	}

	public class RawRGBDecoder : IRawDecoder
	{
		public Rgba32[] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight) {
			Rgba32[] output = new Rgba32[pixelWidth * pixelHeight];
			for (int i = 0; i < output.Length; i++) {
				output[i].R = data[i * 3];
				output[i].G = data[i * 3 + 1];
				output[i].B = data[i * 3 + 2];
				output[i].A = 255;
			}
			return output;
		}
	}

	public class RawRGBADecoder : IRawDecoder
	{
		public Rgba32[] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight) {
			Rgba32[] output = new Rgba32[pixelWidth * pixelHeight];
			for (int i = 0; i < output.Length; i++) {
				output[i].R = data[i * 4];
				output[i].G = data[i * 4 + 1];
				output[i].B = data[i * 4 + 2];
				output[i].A = data[i * 4 + 3];
			}
			return output;
		}
	}
}
