using System;
using System.Collections.Generic;
using System.Text;
using BCnEnc.Net.Shared;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Encoder
{
	internal interface IRawEncoder
	{
		byte[] Encode(ReadOnlySpan<Rgba32> pixels);
		GlInternalFormat GetInternalFormat();
		GLFormat GetBaseInternalFormat();
		GLFormat GetGlFormat();
		GLType GetGlType();
		uint GetGlTypeSize();
	}

	internal class RawLuminanceEncoder : IRawEncoder {
		private readonly bool useLuminance;

		public RawLuminanceEncoder(bool useLuminance) {
			this.useLuminance = useLuminance;
		}

		public byte[] Encode(ReadOnlySpan<Rgba32> pixels) {
			byte[] output = new byte[pixels.Length];
			for (int i = 0; i < pixels.Length; i++) {
				if (useLuminance) {
					output[i] = (byte)(new ColorYCbCr(pixels[i]).y * 255);
				}
				else {
					output[i] = pixels[i].R;
				}
				
			}
			return output;
		}

		public GlInternalFormat GetInternalFormat()
			=> GlInternalFormat.GL_R8;

		public GLFormat GetBaseInternalFormat()
			=> GLFormat.GL_RED;

		public GLFormat GetGlFormat() => GLFormat.GL_RED;

		public GLType GetGlType()
			=> GLType.GL_BYTE;

		public uint GetGlTypeSize()
			=> 1;
	}

	internal class RawRGEncoder : IRawEncoder
	{
		public byte[] Encode(ReadOnlySpan<Rgba32> pixels) {
			byte[] output = new byte[pixels.Length * 2];
			for (int i = 0; i < pixels.Length; i++) {
				output[i * 2] = pixels[i].R;
				output[i * 2 + 1] = pixels[i].G;
			}
			return output;
		}

		public GlInternalFormat GetInternalFormat()
			=> GlInternalFormat.GL_RG8;

		public GLFormat GetBaseInternalFormat()
			=> GLFormat.GL_RG;

		public GLFormat GetGlFormat() => GLFormat.GL_RG;

		public GLType GetGlType()
			=> GLType.GL_BYTE;

		public uint GetGlTypeSize()
			=> 1;
	}

	internal class RawRGBEncoder : IRawEncoder
	{
		public byte[] Encode(ReadOnlySpan<Rgba32> pixels) {
			byte[] output = new byte[pixels.Length * 3];
			for (int i = 0; i < pixels.Length; i++) {
				output[i * 3] = pixels[i].R;
				output[i * 3 + 1] = pixels[i].G;
				output[i * 3 + 2] = pixels[i].B;
			}
			return output;
		}

		public GlInternalFormat GetInternalFormat()
			=> GlInternalFormat.GL_RGB8;

		public GLFormat GetBaseInternalFormat()
			=> GLFormat.GL_RGB;

		public GLFormat GetGlFormat() => GLFormat.GL_RGB;

		public GLType GetGlType()
			=> GLType.GL_BYTE;

		public uint GetGlTypeSize()
			=> 1;
	}

	internal class RawRGBAEncoder : IRawEncoder
	{
		public byte[] Encode(ReadOnlySpan<Rgba32> pixels) {
			byte[] output = new byte[pixels.Length * 4];
			for (int i = 0; i < pixels.Length; i++) {
				output[i * 4] = pixels[i].R;
				output[i * 4 + 1] = pixels[i].G;
				output[i * 4 + 2] = pixels[i].B;
				output[i * 4 + 3] = pixels[i].A;
			}
			return output;
		}

		public GlInternalFormat GetInternalFormat()
			=> GlInternalFormat.GL_RGBA8;

		public GLFormat GetBaseInternalFormat()
			=> GLFormat.GL_RGBA;

		public GLFormat GetGlFormat() => GLFormat.GL_RGBA;

		public GLType GetGlType()
			=> GLType.GL_BYTE;

		public uint GetGlTypeSize()
			=> 1;
	}
}
