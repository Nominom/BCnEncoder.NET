using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;

namespace BCnEncoder.Encoder
{
	internal interface IRawEncoder
	{
		byte[] Encode(Memory<ColorRgba32> pixels);
		GlInternalFormat GetInternalFormat();
		GlFormat GetBaseInternalFormat();
		GlFormat GetGlFormat();
		GlType GetGlType();
		uint GetGlTypeSize();
		DxgiFormat GetDxgiFormat();
	}

	internal class RawLuminanceEncoder : IRawEncoder
	{
		private readonly bool useLuminance;

		public RawLuminanceEncoder(bool useLuminance)
		{
			this.useLuminance = useLuminance;
		}

		public byte[] Encode(Memory<ColorRgba32> pixels)
		{
			var span = pixels.Span;

			var output = new byte[pixels.Length];
			for (var i = 0; i < pixels.Length; i++)
			{
				if (useLuminance)
				{
					output[i] = (byte)(new ColorYCbCr(span[i]).y * 255);
				}
				else
				{
					output[i] = span[i].r;
				}

			}
			return output;
		}

		public GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlR8;
		}

		public GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRed;
		}

		public GlFormat GetGlFormat()
		{
			return GlFormat.GlRed;
		}

		public GlType GetGlType()
		{
			return GlType.GlByte;
		}

		public uint GetGlTypeSize()
		{
			return 1;
		}

		public DxgiFormat GetDxgiFormat()
		{
			return DxgiFormat.DxgiFormatR8Unorm;
		}
	}

	internal class RawRgEncoder : IRawEncoder
	{
		public byte[] Encode(Memory<ColorRgba32> pixels)
		{
			var span = pixels.Span;

			var output = new byte[pixels.Length * 2];
			for (var i = 0; i < pixels.Length; i++)
			{
				output[i * 2] = span[i].r;
				output[i * 2 + 1] = span[i].g;
			}
			return output;
		}

		public GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlRg8;
		}

		public GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRg;
		}

		public GlFormat GetGlFormat()
		{
			return GlFormat.GlRg;
		}

		public GlType GetGlType()
		{
			return GlType.GlByte;
		}

		public uint GetGlTypeSize()
		{
			return 1;
		}

		public DxgiFormat GetDxgiFormat()
		{
			return DxgiFormat.DxgiFormatR8G8Unorm;
		}
	}

	internal class RawRgbEncoder : IRawEncoder
	{
		public byte[] Encode(Memory<ColorRgba32> pixels)
		{
			var span = pixels.Span;

			var output = new byte[pixels.Length * 3];
			for (var i = 0; i < pixels.Length; i++)
			{
				output[i * 3] = span[i].r;
				output[i * 3 + 1] = span[i].g;
				output[i * 3 + 2] = span[i].b;
			}
			return output;
		}

		public GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlRgb8;
		}

		public GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRgb;
		}

		public GlFormat GetGlFormat()
		{
			return GlFormat.GlRgb;
		}

		public GlType GetGlType()
		{
			return GlType.GlByte;
		}

		public uint GetGlTypeSize()
		{
			return 1;
		}

		public DxgiFormat GetDxgiFormat()
		{
			throw new NotSupportedException("RGB Format is not supported for dds files.");
		}
	}

	internal class RawRgbaEncoder : IRawEncoder
	{
		public byte[] Encode(Memory<ColorRgba32> pixels)
		{
			var span = pixels.Span;

			var output = new byte[pixels.Length * 4];
			for (var i = 0; i < pixels.Length; i++)
			{
				output[i * 4] = span[i].r;
				output[i * 4 + 1] = span[i].g;
				output[i * 4 + 2] = span[i].b;
				output[i * 4 + 3] = span[i].a;
			}
			return output;
		}

		public GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlRgba8;
		}

		public GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlRgba;
		}

		public GlFormat GetGlFormat()
		{
			return GlFormat.GlRgba;
		}

		public GlType GetGlType()
		{
			return GlType.GlByte;
		}

		public uint GetGlTypeSize()
		{
			return 1;
		}

		public DxgiFormat GetDxgiFormat()
		{
			return DxgiFormat.DxgiFormatR8G8B8A8Unorm;
		}
	}

	internal class RawBgraEncoder : IRawEncoder
	{
		public byte[] Encode(Memory<ColorRgba32> pixels)
		{
			var span = pixels.Span;

			var output = new byte[pixels.Length * 4];
			for (var i = 0; i < pixels.Length; i++)
			{
				output[i * 4] = span[i].b;
				output[i * 4 + 1] = span[i].g;
				output[i * 4 + 2] = span[i].r;
				output[i * 4 + 3] = span[i].a;
			}
			return output;
		}

		public GlInternalFormat GetInternalFormat()
		{
			return GlInternalFormat.GlBgra8Extension;
		}

		public GlFormat GetBaseInternalFormat()
		{
			return GlFormat.GlBgra;
		}

		public GlFormat GetGlFormat()
		{
			return GlFormat.GlBgra;
		}

		public GlType GetGlType()
		{
			return GlType.GlByte;
		}

		public uint GetGlTypeSize()
		{
			return 1;
		}

		public DxgiFormat GetDxgiFormat()
		{
			return DxgiFormat.DxgiFormatB8G8R8A8Unorm;
		}
	}
}
