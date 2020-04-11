using System;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEnc.Net.Shared
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct Bc1Block
	{
		public ColorRgb565 color0;
		public ColorRgb565 color1;
		public uint colorIndices;

		public int this[int index]
		{
			readonly get => (int)(colorIndices >> (index * 2)) & 0b11;
			set
			{
				colorIndices = (uint)(colorIndices & (~(0b11 << (index * 2))));
				int val = value & 0b11;
				colorIndices = (colorIndices | ((uint)val << (index * 2)));
			}
		}

		public readonly bool HasAlphaOrBlack => color0.data <= color1.data;

		public readonly RawBlock4X4Rgba32 Decode(bool useAlpha)
		{
			RawBlock4X4Rgba32 output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var color0 = this.color0.ToColorRgb24();
			var color1 = this.color1.ToColorRgb24();

			useAlpha = useAlpha && HasAlphaOrBlack;

			Span<ColorRgb24> colors = HasAlphaOrBlack ?
				stackalloc ColorRgb24[] {
				color0,
				color1,
				color0 * (1.0 / 2.0) + color1 * (1.0 / 2.0),
				new ColorRgb24(0, 0, 0)
			} : stackalloc ColorRgb24[] {
				color0,
				color1,
				color0 * (2.0 / 3.0) + color1 * (1.0 / 3.0),
				color0 * (1.0 / 3.0) + color1 * (2.0 / 3.0)
			};

			for (int i = 0; i < pixels.Length; i++)
			{
				int colorIndex = (int)((colorIndices >> (i * 2)) & 0b11);
				var color = colors[colorIndex];
				if (useAlpha && colorIndex == 3)
				{
					pixels[i] = new Rgba32(0, 0, 0, 0);
				}
				else
				{
					pixels[i] = new Rgba32(color.r, color.g, color.b, 255);
				}
			}
			return output;
		}
	}


	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct Bc2Block
	{
		public ulong alphaColors;
		public ColorRgb565 color0;
		public ColorRgb565 color1;
		public uint colorIndices;

		public int this[int index]
		{
			readonly get => (int)(colorIndices >> (index * 2)) & 0b11;
			set
			{
				colorIndices = (uint)(colorIndices & (~(0b11 << (index * 2))));
				int val = value & 0b11;
				colorIndices = (colorIndices | ((uint)val << (index * 2)));
			}
		}

		public readonly byte GetAlpha(int index)
		{
			ulong mask = 0xFUL << (index * 4);
			int shift = (index * 4);
			ulong alphaUnscaled = (((alphaColors & mask) >> shift));
			return (byte)((alphaUnscaled / 15.0) * 255);
		}

		public void SetAlpha(int index, byte alpha)
		{
			ulong mask = 0xFUL << (index * 4);
			int shift = (index * 4);
			alphaColors &= ~mask;
			byte a = (byte)((alpha / 255.0) * 15);
			alphaColors |= (ulong)(a & 0xF) << shift;
		}

		public readonly RawBlock4X4Rgba32 Decode()
		{
			RawBlock4X4Rgba32 output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var color0 = this.color0.ToColorRgb24();
			var color1 = this.color1.ToColorRgb24();

			Span<ColorRgb24> colors = stackalloc ColorRgb24[] {
				color0,
				color1,
				color0 * (2.0 / 3.0) + color1 * (1.0 / 3.0),
				color0 * (1.0 / 3.0) + color1 * (2.0 / 3.0)
			};

			for (int i = 0; i < pixels.Length; i++)
			{
				int colorIndex = (int)((colorIndices >> (i * 2)) & 0b11);
				var color = colors[colorIndex];

				pixels[i] = new Rgba32(color.r, color.g, color.b, GetAlpha(i));
			}
			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct Bc3Block
	{
		public ulong alphaBlock;
		public ColorRgb565 color0;
		public ColorRgb565 color1;
		public uint colorIndices;

		public int this[int index]
		{
			readonly get => (int)(colorIndices >> (index * 2)) & 0b11;
			set
			{
				colorIndices = (uint)(colorIndices & (~(0b11 << (index * 2))));
				int val = value & 0b11;
				colorIndices = (colorIndices | ((uint)val << (index * 2)));
			}
		}

		public byte Alpha0
		{
			readonly get => (byte)(alphaBlock & 0xFFUL);
			set
			{
				alphaBlock &= ~0xFFUL;
				alphaBlock |= value;
			}
		}

		public byte Alpha1
		{
			readonly get => (byte)((alphaBlock >> 8) & 0xFFUL);
			set
			{
				alphaBlock &= ~0xFF00UL;
				alphaBlock |= (ulong)value << 8;
			}
		}

		public readonly byte GetAlphaIndex(int pixelIndex)
		{
			ulong mask = 0b0111UL << (pixelIndex * 3 + 16);
			int shift = (pixelIndex * 3 + 16);
			ulong alphaIndex = (((alphaBlock & mask) >> shift));
			return (byte)alphaIndex;
		}

		public void SetAlphaIndex(int pixelIndex, byte alphaIndex)
		{
			ulong mask = 0b0111UL << (pixelIndex * 3 + 16);
			int shift = (pixelIndex * 3 + 16);
			alphaBlock &= ~mask;
			alphaBlock |= (ulong)(alphaIndex & 0b111) << shift;
		}

		public readonly RawBlock4X4Rgba32 Decode()
		{
			RawBlock4X4Rgba32 output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var color0 = this.color0.ToColorRgb24();
			var color1 = this.color1.ToColorRgb24();

			var a0 = Alpha0;
			var a1 = Alpha1;
			Span<ColorRgb24> colors = stackalloc ColorRgb24[] {
				color0,
				color1,
				color0 * (2.0 / 3.0) + color1 * (1.0 / 3.0),
				color0 * (1.0 / 3.0) + color1 * (2.0 / 3.0)
			};

			Span<byte> alphas = a0 > a1 ? stackalloc byte[] {
				a0,
				a1,
				(byte)(6 / 7.0 * a0 + 1 / 7.0 * a1),
				(byte)(5 / 7.0 * a0 + 2 / 7.0 * a1),
				(byte)(4 / 7.0 * a0 + 3 / 7.0 * a1),
				(byte)(3 / 7.0 * a0 + 4 / 7.0 * a1),
				(byte)(2 / 7.0 * a0 + 5 / 7.0 * a1),
				(byte)(1 / 7.0 * a0 + 6 / 7.0 * a1),
			} : stackalloc byte[] {
				a0,
				a1,
				(byte)(4 / 5.0 * a0 + 1 / 5.0 * a1),
				(byte)(3 / 5.0 * a0 + 2 / 5.0 * a1),
				(byte)(2 / 5.0 * a0 + 3 / 5.0 * a1),
				(byte)(1 / 5.0 * a0 + 4 / 5.0 * a1),
				0,
				255
			};

			for (int i = 0; i < pixels.Length; i++)
			{
				int colorIndex = (int)((colorIndices >> (i * 2)) & 0b11);
				var color = colors[colorIndex];

				pixels[i] = new Rgba32(color.r, color.g, color.b, alphas[GetAlphaIndex(i)]);
			}
			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct Bc4Block
	{
		public ulong redBlock;

		public byte Red0
		{
			readonly get => (byte)(redBlock & 0xFFUL);
			set
			{
				redBlock &= ~0xFFUL;
				redBlock |= value;
			}
		}

		public byte Red1
		{
			readonly get => (byte)((redBlock >> 8) & 0xFFUL);
			set
			{
				redBlock &= ~0xFF00UL;
				redBlock |= (ulong)value << 8;
			}
		}

		public readonly byte GetRedIndex(int pixelIndex)
		{
			ulong mask = 0b0111UL << (pixelIndex * 3 + 16);
			int shift = (pixelIndex * 3 + 16);
			ulong redIndex = (((redBlock & mask) >> shift));
			return (byte)redIndex;
		}

		public void SetRedIndex(int pixelIndex, byte redIndex)
		{
			ulong mask = 0b0111UL << (pixelIndex * 3 + 16);
			int shift = (pixelIndex * 3 + 16);
			redBlock &= ~mask;
			redBlock |= (ulong)(redIndex & 0b111) << shift;
		}

		public readonly RawBlock4X4Rgba32 Decode(bool redAsLuminance)
		{
			RawBlock4X4Rgba32 output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var r0 = Red0;
			var r1 = Red1;

			Span<byte> reds = r0 > r1 ? stackalloc byte[] {
				r0,
				r1,
				(byte)(6 / 7.0 * r0 + 1 / 7.0 * r1),
				(byte)(5 / 7.0 * r0 + 2 / 7.0 * r1),
				(byte)(4 / 7.0 * r0 + 3 / 7.0 * r1),
				(byte)(3 / 7.0 * r0 + 4 / 7.0 * r1),
				(byte)(2 / 7.0 * r0 + 5 / 7.0 * r1),
				(byte)(1 / 7.0 * r0 + 6 / 7.0 * r1),
			} : stackalloc byte[] {
				r0,
				r1,
				(byte)(4 / 5.0 * r0 + 1 / 5.0 * r1),
				(byte)(3 / 5.0 * r0 + 2 / 5.0 * r1),
				(byte)(2 / 5.0 * r0 + 3 / 5.0 * r1),
				(byte)(1 / 5.0 * r0 + 4 / 5.0 * r1),
				0,
				255
			};

			if (redAsLuminance)
			{
				for (int i = 0; i < pixels.Length; i++)
				{
					var index = GetRedIndex(i);
					pixels[i] = new Rgba32(reds[index], reds[index], reds[index], 255);
				}
			}
			else
			{
				for (int i = 0; i < pixels.Length; i++)
				{
					var index = GetRedIndex(i);
					pixels[i] = new Rgba32(reds[index], 0, 0, 255);
				}
			}

			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct Bc5Block
	{
		public ulong redBlock;
		public ulong greenBlock;

		public byte Red0
		{
			readonly get => (byte)(redBlock & 0xFFUL);
			set
			{
				redBlock &= ~0xFFUL;
				redBlock |= value;
			}
		}

		public byte Red1
		{
			readonly get => (byte)((redBlock >> 8) & 0xFFUL);
			set
			{
				redBlock &= ~0xFF00UL;
				redBlock |= (ulong)value << 8;
			}
		}

		public byte Green0
		{
			readonly get => (byte)(greenBlock & 0xFFUL);
			set
			{
				greenBlock &= ~0xFFUL;
				greenBlock |= value;
			}
		}

		public byte Green1
		{
			readonly get => (byte)((greenBlock >> 8) & 0xFFUL);
			set
			{
				greenBlock &= ~0xFF00UL;
				greenBlock |= (ulong)value << 8;
			}
		}

		public readonly byte GetRedIndex(int pixelIndex)
		{
			ulong mask = 0b0111UL << (pixelIndex * 3 + 16);
			int shift = (pixelIndex * 3 + 16);
			ulong redIndex = (((redBlock & mask) >> shift));
			return (byte)redIndex;
		}

		public void SetRedIndex(int pixelIndex, byte redIndex)
		{
			ulong mask = 0b0111UL << (pixelIndex * 3 + 16);
			int shift = (pixelIndex * 3 + 16);
			redBlock &= ~mask;
			redBlock |= (ulong)(redIndex & 0b111) << shift;
		}

		public readonly byte GetGreenIndex(int pixelIndex)
		{
			ulong mask = 0b0111UL << (pixelIndex * 3 + 16);
			int shift = (pixelIndex * 3 + 16);
			ulong greenIndex = (((greenBlock & mask) >> shift));
			return (byte)greenIndex;
		}

		public void SetGreenIndex(int pixelIndex, byte greenIndex)
		{
			ulong mask = 0b0111UL << (pixelIndex * 3 + 16);
			int shift = (pixelIndex * 3 + 16);
			greenBlock &= ~mask;
			greenBlock |= (ulong)(greenIndex & 0b111) << shift;
		}

		public readonly RawBlock4X4Rgba32 Decode()
		{
			RawBlock4X4Rgba32 output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var r0 = Red0;
			var r1 = Red1;

			Span<byte> reds = r0 > r1 ? stackalloc byte[] {
				r0,
				r1,
				(byte)(6 / 7.0 * r0 + 1 / 7.0 * r1),
				(byte)(5 / 7.0 * r0 + 2 / 7.0 * r1),
				(byte)(4 / 7.0 * r0 + 3 / 7.0 * r1),
				(byte)(3 / 7.0 * r0 + 4 / 7.0 * r1),
				(byte)(2 / 7.0 * r0 + 5 / 7.0 * r1),
				(byte)(1 / 7.0 * r0 + 6 / 7.0 * r1),
			} : stackalloc byte[] {
				r0,
				r1,
				(byte)(4 / 5.0 * r0 + 1 / 5.0 * r1),
				(byte)(3 / 5.0 * r0 + 2 / 5.0 * r1),
				(byte)(2 / 5.0 * r0 + 3 / 5.0 * r1),
				(byte)(1 / 5.0 * r0 + 4 / 5.0 * r1),
				0,
				255
			};

			var g0 = Green0;
			var g1 = Green1;

			Span<byte> greens = g0 > g1 ? stackalloc byte[] {
				g0,
				g1,
				(byte)(6 / 7.0 * g0 + 1 / 7.0 * g1),
				(byte)(5 / 7.0 * g0 + 2 / 7.0 * g1),
				(byte)(4 / 7.0 * g0 + 3 / 7.0 * g1),
				(byte)(3 / 7.0 * g0 + 4 / 7.0 * g1),
				(byte)(2 / 7.0 * g0 + 5 / 7.0 * g1),
				(byte)(1 / 7.0 * g0 + 6 / 7.0 * g1),
			} : stackalloc byte[] {
				g0,
				g1,
				(byte)(4 / 5.0 * g0 + 1 / 5.0 * g1),
				(byte)(3 / 5.0 * g0 + 2 / 5.0 * g1),
				(byte)(2 / 5.0 * g0 + 3 / 5.0 * g1),
				(byte)(1 / 5.0 * g0 + 4 / 5.0 * g1),
				0,
				255
			};

			for (int i = 0; i < pixels.Length; i++)
			{
				var redIndex = GetRedIndex(i);
				var greenIndex = GetGreenIndex(i);
				pixels[i] = new Rgba32(reds[redIndex], greens[greenIndex], 0, 255);
			}

			return output;
		}
	}

}
