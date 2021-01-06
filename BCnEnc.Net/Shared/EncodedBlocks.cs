using System;
using System.Runtime.InteropServices;

namespace BCnEncoder.Shared
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct Bc1Block
	{
		public ColorRgb565 color0;
		public ColorRgb565 color1;
		public uint colorIndices;

		public int this[int index]
		{
			get => (int)(colorIndices >> (index * 2)) & 0b11;
			set
			{
				colorIndices = (uint)(colorIndices & (~(0b11 << (index * 2))));
				int val = value & 0b11;
				colorIndices = (colorIndices | ((uint)val << (index * 2)));
			}
		}

		public bool HasAlphaOrBlack => color0.data <= color1.data;

		public RawBlock4X4Rgba32 Decode(bool useAlpha)
		{
			RawBlock4X4Rgba32 output = new RawBlock4X4Rgba32();

			var color0 = this.color0.ToColorRgb24();
			var color1 = this.color1.ToColorRgb24();

			useAlpha = useAlpha && HasAlphaOrBlack;

			ColorRgb24[] colors = HasAlphaOrBlack ?
				new ColorRgb24[] {
				color0,
				color1,
				color0 * (1.0 / 2.0) + color1 * (1.0 / 2.0),
				new ColorRgb24(0, 0, 0)
			} : new ColorRgb24[] {
				color0,
				color1,
				color0 * (2.0 / 3.0) + color1 * (1.0 / 3.0),
				color0 * (1.0 / 3.0) + color1 * (2.0 / 3.0)
			};

			for (int i = 0; i < 16; i++)
			{
				int colorIndex = (int)((colorIndices >> (i * 2)) & 0b11);
				var color = colors[colorIndex];
				if (useAlpha && colorIndex == 3)
				{
					output[i] = new Rgba32(0, 0, 0, 0);
				}
				else
				{
					output[i] = new Rgba32(color.r, color.g, color.b, 255);
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
			get => (int)(colorIndices >> (index * 2)) & 0b11;
			set
			{
				colorIndices = (uint)(colorIndices & (~(0b11 << (index * 2))));
				int val = value & 0b11;
				colorIndices = (colorIndices | ((uint)val << (index * 2)));
			}
		}

		public byte GetAlpha(int index)
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

		public RawBlock4X4Rgba32 Decode()
		{
			RawBlock4X4Rgba32 output = new RawBlock4X4Rgba32();

			var color0 = this.color0.ToColorRgb24();
			var color1 = this.color1.ToColorRgb24();

			ColorRgb24[] colors = new ColorRgb24[] {
				color0,
				color1,
				color0 * (2.0 / 3.0) + color1 * (1.0 / 3.0),
				color0 * (1.0 / 3.0) + color1 * (2.0 / 3.0)
			};

			for (int i = 0; i < 16; i++)
			{
				int colorIndex = (int)((colorIndices >> (i * 2)) & 0b11);
				var color = colors[colorIndex];

				output[i] = new Rgba32(color.r, color.g, color.b, GetAlpha(i));
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
			get => (int)(colorIndices >> (index * 2)) & 0b11;
			set
			{
				colorIndices = (uint)(colorIndices & (~(0b11 << (index * 2))));
				int val = value & 0b11;
				colorIndices = (colorIndices | ((uint)val << (index * 2)));
			}
		}

		public byte Alpha0
		{
			get => (byte)(alphaBlock & 0xFFUL);
			set
			{
				alphaBlock &= ~0xFFUL;
				alphaBlock |= value;
			}
		}

		public byte Alpha1
		{
			get => (byte)((alphaBlock >> 8) & 0xFFUL);
			set
			{
				alphaBlock &= ~0xFF00UL;
				alphaBlock |= (ulong)value << 8;
			}
		}

		public byte GetAlphaIndex(int pixelIndex)
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

		public RawBlock4X4Rgba32 Decode()
		{
			RawBlock4X4Rgba32 output = new RawBlock4X4Rgba32();

			var color0 = this.color0.ToColorRgb24();
			var color1 = this.color1.ToColorRgb24();

			var a0 = Alpha0;
			var a1 = Alpha1;
			ColorRgb24[] colors = new ColorRgb24[] {
				color0,
				color1,
				color0 * (2.0 / 3.0) + color1 * (1.0 / 3.0),
				color0 * (1.0 / 3.0) + color1 * (2.0 / 3.0)
			};

			byte[] alphas = a0 > a1 ? new byte[] {
				a0,
				a1,
				(byte)(6 / 7.0 * a0 + 1 / 7.0 * a1),
				(byte)(5 / 7.0 * a0 + 2 / 7.0 * a1),
				(byte)(4 / 7.0 * a0 + 3 / 7.0 * a1),
				(byte)(3 / 7.0 * a0 + 4 / 7.0 * a1),
				(byte)(2 / 7.0 * a0 + 5 / 7.0 * a1),
				(byte)(1 / 7.0 * a0 + 6 / 7.0 * a1),
			} : new byte[] {
				a0,
				a1,
				(byte)(4 / 5.0 * a0 + 1 / 5.0 * a1),
				(byte)(3 / 5.0 * a0 + 2 / 5.0 * a1),
				(byte)(2 / 5.0 * a0 + 3 / 5.0 * a1),
				(byte)(1 / 5.0 * a0 + 4 / 5.0 * a1),
				0,
				255
			};

			for (int i = 0; i < 16; i++)
			{
				int colorIndex = (int)((colorIndices >> (i * 2)) & 0b11);
				var color = colors[colorIndex];

				output[i] = new Rgba32(color.r, color.g, color.b, alphas[GetAlphaIndex(i)]);
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
			get => (byte)(redBlock & 0xFFUL);
			set
			{
				redBlock &= ~0xFFUL;
				redBlock |= value;
			}
		}

		public byte Red1
		{
			get => (byte)((redBlock >> 8) & 0xFFUL);
			set
			{
				redBlock &= ~0xFF00UL;
				redBlock |= (ulong)value << 8;
			}
		}

		public byte GetRedIndex(int pixelIndex)
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

		public RawBlock4X4Rgba32 Decode(bool redAsLuminance)
		{
			RawBlock4X4Rgba32 output = new RawBlock4X4Rgba32();

			var r0 = Red0;
			var r1 = Red1;

			byte[] reds = r0 > r1 ? new byte[] {
				r0,
				r1,
				(byte)(6 / 7.0 * r0 + 1 / 7.0 * r1),
				(byte)(5 / 7.0 * r0 + 2 / 7.0 * r1),
				(byte)(4 / 7.0 * r0 + 3 / 7.0 * r1),
				(byte)(3 / 7.0 * r0 + 4 / 7.0 * r1),
				(byte)(2 / 7.0 * r0 + 5 / 7.0 * r1),
				(byte)(1 / 7.0 * r0 + 6 / 7.0 * r1),
			} : new byte[] {
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
				for (int i = 0; i < 16; i++)
				{
					var index = GetRedIndex(i);
					output[i] = new Rgba32(reds[index], reds[index], reds[index], 255);
				}
			}
			else
			{
				for (int i = 0; i < 16; i++)
				{
					var index = GetRedIndex(i);
					output[i] = new Rgba32(reds[index], 0, 0, 255);
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
			get => (byte)(redBlock & 0xFFUL);
			set
			{
				redBlock &= ~0xFFUL;
				redBlock |= value;
			}
		}

		public byte Red1
		{
			get => (byte)((redBlock >> 8) & 0xFFUL);
			set
			{
				redBlock &= ~0xFF00UL;
				redBlock |= (ulong)value << 8;
			}
		}

		public byte Green0
		{
			get => (byte)(greenBlock & 0xFFUL);
			set
			{
				greenBlock &= ~0xFFUL;
				greenBlock |= value;
			}
		}

		public byte Green1
		{
			get => (byte)((greenBlock >> 8) & 0xFFUL);
			set
			{
				greenBlock &= ~0xFF00UL;
				greenBlock |= (ulong)value << 8;
			}
		}

		public byte GetRedIndex(int pixelIndex)
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

		public byte GetGreenIndex(int pixelIndex)
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

		public RawBlock4X4Rgba32 Decode()
		{
			RawBlock4X4Rgba32 output = new RawBlock4X4Rgba32();

			var r0 = Red0;
			var r1 = Red1;

			byte[] reds = r0 > r1 ? new byte[] {
				r0,
				r1,
				(byte)(6 / 7.0 * r0 + 1 / 7.0 * r1),
				(byte)(5 / 7.0 * r0 + 2 / 7.0 * r1),
				(byte)(4 / 7.0 * r0 + 3 / 7.0 * r1),
				(byte)(3 / 7.0 * r0 + 4 / 7.0 * r1),
				(byte)(2 / 7.0 * r0 + 5 / 7.0 * r1),
				(byte)(1 / 7.0 * r0 + 6 / 7.0 * r1),
			} : new byte[] {
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

			byte[] greens = g0 > g1 ? new byte[] {
				g0,
				g1,
				(byte)(6 / 7.0 * g0 + 1 / 7.0 * g1),
				(byte)(5 / 7.0 * g0 + 2 / 7.0 * g1),
				(byte)(4 / 7.0 * g0 + 3 / 7.0 * g1),
				(byte)(3 / 7.0 * g0 + 4 / 7.0 * g1),
				(byte)(2 / 7.0 * g0 + 5 / 7.0 * g1),
				(byte)(1 / 7.0 * g0 + 6 / 7.0 * g1),
			} : new byte[] {
				g0,
				g1,
				(byte)(4 / 5.0 * g0 + 1 / 5.0 * g1),
				(byte)(3 / 5.0 * g0 + 2 / 5.0 * g1),
				(byte)(2 / 5.0 * g0 + 3 / 5.0 * g1),
				(byte)(1 / 5.0 * g0 + 4 / 5.0 * g1),
				0,
				255
			};

			for (int i = 0; i < 16; i++)
			{
				var redIndex = GetRedIndex(i);
				var greenIndex = GetGreenIndex(i);
				output[i] = new Rgba32(reds[redIndex], greens[greenIndex], 0, 255);
			}

			return output;
		}
	}

}
