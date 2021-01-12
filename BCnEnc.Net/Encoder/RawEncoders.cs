using System;
using BCnEncoder.Shared;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.Encoder
{
	internal interface IRawEncoder
	{
		byte[] Encode(ReadOnlySpan<Rgba32> pixels);
		GlInternalFormat GetInternalFormat();
		GlFormat GetBaseInternalFormat();
		GlFormat GetGlFormat();
		GlType GetGlType();
		uint GetGlTypeSize();
		DxgiFormat GetDxgiFormat();
	}

	internal class RawLuminanceEncoder : IRawEncoder {
		private readonly bool useLuminance;

		public RawLuminanceEncoder(bool useLuminance) {
			this.useLuminance = useLuminance;
		}

		public byte[] Encode(ReadOnlySpan<Rgba32> pixels) {
			var output = new byte[pixels.Length];
			for (var i = 0; i < pixels.Length; i++) {
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
			=> GlInternalFormat.GlR8;

		public GlFormat GetBaseInternalFormat()
			=> GlFormat.GlRed;

		public GlFormat GetGlFormat() => GlFormat.GlRed;

		public GlType GetGlType()
			=> GlType.GlByte;

		public uint GetGlTypeSize()
			=> 1;

		public DxgiFormat GetDxgiFormat() => DxgiFormat.DxgiFormatR8Unorm;
	}

	internal class RawRgEncoder : IRawEncoder
	{
		public byte[] Encode(ReadOnlySpan<Rgba32> pixels) {
			var output = new byte[pixels.Length * 2];
			for (var i = 0; i < pixels.Length; i++) {
				output[i * 2] = pixels[i].R;
				output[i * 2 + 1] = pixels[i].G;
			}
			return output;
		}

		public GlInternalFormat GetInternalFormat()
			=> GlInternalFormat.GlRg8;

		public GlFormat GetBaseInternalFormat()
			=> GlFormat.GlRg;

		public GlFormat GetGlFormat() => GlFormat.GlRg;

		public GlType GetGlType()
			=> GlType.GlByte;

		public uint GetGlTypeSize()
			=> 1;

		public DxgiFormat GetDxgiFormat() => DxgiFormat.DxgiFormatR8G8Unorm;
	}

	internal class RawRgbEncoder : IRawEncoder
	{
		public byte[] Encode(ReadOnlySpan<Rgba32> pixels) {
			var output = new byte[pixels.Length * 3];
			for (var i = 0; i < pixels.Length; i++) {
				output[i * 3] = pixels[i].R;
				output[i * 3 + 1] = pixels[i].G;
				output[i * 3 + 2] = pixels[i].B;
			}
			return output;
		}

		public GlInternalFormat GetInternalFormat()
			=> GlInternalFormat.GlRgb8;

		public GlFormat GetBaseInternalFormat()
			=> GlFormat.GlRgb;

		public GlFormat GetGlFormat() => GlFormat.GlRgb;

		public GlType GetGlType()
			=> GlType.GlByte;

		public uint GetGlTypeSize()
			=> 1;

		public DxgiFormat GetDxgiFormat() => throw new NotSupportedException("RGB Format is not supported for dds files.");
	}

	internal class RawRgbaEncoder : IRawEncoder
	{
		public byte[] Encode(ReadOnlySpan<Rgba32> pixels) {
			var output = new byte[pixels.Length * 4];
			for (var i = 0; i < pixels.Length; i++) {
				output[i * 4] = pixels[i].R;
				output[i * 4 + 1] = pixels[i].G;
				output[i * 4 + 2] = pixels[i].B;
				output[i * 4 + 3] = pixels[i].A;
			}
			return output;
		}

		public GlInternalFormat GetInternalFormat()
			=> GlInternalFormat.GlRgba8;

		public GlFormat GetBaseInternalFormat()
			=> GlFormat.GlRgba;

		public GlFormat GetGlFormat() => GlFormat.GlRgba;

		public GlType GetGlType()
			=> GlType.GlByte;

		public uint GetGlTypeSize()
			=> 1;

		public DxgiFormat GetDxgiFormat() => DxgiFormat.DxgiFormatR8G8B8A8Unorm;
	}
}
