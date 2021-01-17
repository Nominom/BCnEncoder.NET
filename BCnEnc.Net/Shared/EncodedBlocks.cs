using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BCnEncoder.Encoder;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncoder.Shared
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
				colorIndices = (uint)(colorIndices & ~(0b11 << (index * 2)));
				var val = value & 0b11;
				colorIndices = colorIndices | ((uint)val << (index * 2));
			}
		}

		public readonly bool HasAlphaOrBlack => color0.data <= color1.data;

		public readonly RawBlock4X4Rgba32 Decode(bool useAlpha)
		{
			var output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var color0 = this.color0.ToColorRgb24();
			var color1 = this.color1.ToColorRgb24();

			useAlpha = useAlpha && HasAlphaOrBlack;

			var colors = HasAlphaOrBlack ?
				stackalloc ColorRgb24[] {
				color0,
				color1,
				color0.InterpolateHalf(color1),
				new ColorRgb24(0, 0, 0)
			} : stackalloc ColorRgb24[] {
				color0,
				color1,
				color0.InterpolateThird(color1, 1),
				color0.InterpolateThird(color1, 2)
			};

			for (var i = 0; i < pixels.Length; i++)
			{
				var colorIndex = (int)((colorIndices >> (i * 2)) & 0b11);
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
		public Bc2AlphaBlock alphaBlock;
		public ColorRgb565 color0;
		public ColorRgb565 color1;
		public uint colorIndices;

		public int this[int index]
		{
			readonly get => (int)(colorIndices >> (index * 2)) & 0b11;
			set
			{
				colorIndices = (uint)(colorIndices & ~(0b11 << (index * 2)));
				var val = value & 0b11;
				colorIndices = colorIndices | ((uint)val << (index * 2));
			}
		}

		public readonly byte GetAlpha(int index) => alphaBlock.GetAlpha(index);

		public void SetAlpha(int index, byte alpha) => alphaBlock.SetAlpha(index, alpha);

		public readonly RawBlock4X4Rgba32 Decode()
		{
			var output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var color0 = this.color0.ToColorRgb24();
			var color1 = this.color1.ToColorRgb24();

			Span<ColorRgb24> colors = stackalloc ColorRgb24[] {
				color0,
				color1,
				color0.InterpolateThird(color1, 1),
				color0.InterpolateThird(color1, 2)
			};

			for (var i = 0; i < pixels.Length; i++)
			{
				var colorIndex = (int)((colorIndices >> (i * 2)) & 0b11);
				var color = colors[colorIndex];

				pixels[i] = new Rgba32(color.r, color.g, color.b, GetAlpha(i));
			}
			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct Bc3Block
	{
		public Bc4Block alphaBlock;
		public ColorRgb565 color0;
		public ColorRgb565 color1;
		public uint colorIndices;

		public int this[int index]
		{
			readonly get => (int)(colorIndices >> (index * 2)) & 0b11;
			set
			{
				colorIndices = (uint)(colorIndices & ~(0b11 << (index * 2)));
				var val = value & 0b11;
				colorIndices = colorIndices | ((uint)val << (index * 2));
			}
		}

		public byte Alpha0
		{
			get => alphaBlock.Endpoint0;
			set => alphaBlock.Endpoint0 = value;
		}

		public byte Alpha1
		{
			get => alphaBlock.Endpoint1;
			set => alphaBlock.Endpoint1 = value;
		}

		public readonly byte GetAlphaIndex(int pixelIndex) => alphaBlock.GetComponentIndex(pixelIndex);

		public void SetAlphaIndex(int pixelIndex, byte alphaIndex) => alphaBlock.SetComponentIndex(pixelIndex, alphaIndex);

		public readonly RawBlock4X4Rgba32 Decode()
		{
			var output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var color0 = this.color0.ToColorRgb24();
			var color1 = this.color1.ToColorRgb24();

			Span<ColorRgb24> colors = stackalloc ColorRgb24[] {
				color0,
				color1,
				color0.InterpolateThird(color1, 1),
				color0.InterpolateThird(color1, 2)
			};

			for (var i = 0; i < pixels.Length; i++)
			{
				var colorIndex = (int)((colorIndices >> (i * 2)) & 0b11);
				var color = colors[colorIndex];

				pixels[i] = new Rgba32(color.r, color.g, color.b, alphaBlock.DecodeSingle(i));
			}
			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct Bc4Block
	{
		public ulong componentBlock;

		public byte Endpoint0
		{
			readonly get => (byte)(componentBlock & 0xFFUL);
			set
			{
				componentBlock &= ~0xFFUL;
				componentBlock |= value;
			}
		}

		public byte Endpoint1
		{
			readonly get => (byte)((componentBlock >> 8) & 0xFFUL);
			set
			{
				componentBlock &= ~0xFF00UL;
				componentBlock |= (ulong)value << 8;
			}
		}

		public readonly byte GetComponentIndex(int pixelIndex)
		{
			var mask = 0b0111UL << (pixelIndex * 3 + 16);
			var shift = pixelIndex * 3 + 16;
			var redIndex = (componentBlock & mask) >> shift;
			return (byte)redIndex;
		}

		public void SetComponentIndex(int pixelIndex, byte redIndex)
		{
			var mask = 0b0111UL << (pixelIndex * 3 + 16);
			var shift = pixelIndex * 3 + 16;
			componentBlock &= ~mask;
			componentBlock |= (ulong)(redIndex & 0b111) << shift;
		}

		public readonly byte DecodeSingle(int pixelIndex)
		{
			var r0 = Endpoint0;
			var r1 = Endpoint1;

			var reds = r0 > r1 ? stackalloc byte[] {
				r0,
				r1,
				r0.InterpolateSeventh(r1, 1),
				r0.InterpolateSeventh(r1, 2),
				r0.InterpolateSeventh(r1, 3),
				r0.InterpolateSeventh(r1, 4),
				r0.InterpolateSeventh(r1, 5),
				r0.InterpolateSeventh(r1, 6),
			} : stackalloc byte[] {
				r0,
				r1,
				r0.InterpolateFifth(r1, 1),
				r0.InterpolateFifth(r1, 2),
				r0.InterpolateFifth(r1, 3),
				r0.InterpolateFifth(r1, 4),
				0,
				255
			};

			return reds[GetComponentIndex(pixelIndex)];
		}

		public readonly RawBlock4X4Rgba32 Decode(bool redAsLuminance, Bc4Component component)
		{
			var output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var c0 = Endpoint0;
			var c1 = Endpoint1;

			var components = c0 > c1 ? stackalloc byte[] {
				c0,
				c1,
				c0.InterpolateSeventh(c1, 1),
				c0.InterpolateSeventh(c1, 2),
				c0.InterpolateSeventh(c1, 3),
				c0.InterpolateSeventh(c1, 4),
				c0.InterpolateSeventh(c1, 5),
				c0.InterpolateSeventh(c1, 6),
			} : stackalloc byte[] {
				c0,
				c1,
				c0.InterpolateFifth(c1, 1),
				c0.InterpolateFifth(c1, 2),
				c0.InterpolateFifth(c1, 3),
				c0.InterpolateFifth(c1, 4),
				0,
				255
			};

			if (redAsLuminance)
			{
				for (var i = 0; i < pixels.Length; i++)
				{
					var index = GetComponentIndex(i);
					pixels[i] = new Rgba32(components[index], components[index], components[index], 255);
				}
			}
			else
			{
				for (var i = 0; i < pixels.Length; i++)
				{
					var index = GetComponentIndex(i);
					switch (component)
					{
						case Bc4Component.R:
							pixels[i] = new Rgba32(components[index], 0, 0, 255);
							break;

						case Bc4Component.G:
							pixels[i] = new Rgba32(0, components[index], 0, 255);
							break;

						case Bc4Component.B:
							pixels[i] = new Rgba32(0, 0, components[index], 255);
							break;

						case Bc4Component.A:
							pixels[i] = new Rgba32(0, 0, 0, components[index]);
							break;
					}
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
			var mask = 0b0111UL << (pixelIndex * 3 + 16);
			var shift = pixelIndex * 3 + 16;
			var redIndex = (redBlock & mask) >> shift;
			return (byte)redIndex;
		}

		public void SetRedIndex(int pixelIndex, byte redIndex)
		{
			var mask = 0b0111UL << (pixelIndex * 3 + 16);
			var shift = pixelIndex * 3 + 16;
			redBlock &= ~mask;
			redBlock |= (ulong)(redIndex & 0b111) << shift;
		}

		public readonly byte GetGreenIndex(int pixelIndex)
		{
			var mask = 0b0111UL << (pixelIndex * 3 + 16);
			var shift = pixelIndex * 3 + 16;
			var greenIndex = (greenBlock & mask) >> shift;
			return (byte)greenIndex;
		}

		public void SetGreenIndex(int pixelIndex, byte greenIndex)
		{
			var mask = 0b0111UL << (pixelIndex * 3 + 16);
			var shift = pixelIndex * 3 + 16;
			greenBlock &= ~mask;
			greenBlock |= (ulong)(greenIndex & 0b111) << shift;
		}

		public readonly RawBlock4X4Rgba32 Decode()
		{
			var output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var r0 = Red0;
			var r1 = Red1;

			var reds = r0 > r1 ? stackalloc byte[] {
				r0,
				r1,
				r0.InterpolateSeventh(r1, 1),
				r0.InterpolateSeventh(r1, 2),
				r0.InterpolateSeventh(r1, 3),
				r0.InterpolateSeventh(r1, 4),
				r0.InterpolateSeventh(r1, 5),
				r0.InterpolateSeventh(r1, 6),
			} : stackalloc byte[] {
				r0,
				r1,
				r0.InterpolateFifth(r1, 1),
				r0.InterpolateFifth(r1, 2),
				r0.InterpolateFifth(r1, 3),
				r0.InterpolateFifth(r1, 4),
				0,
				255
			};

			var g0 = Green0;
			var g1 = Green1;

			var greens = g0 > g1 ? stackalloc byte[] {
				g0,
				g1,
				g0.InterpolateSeventh(g1, 1),
				g0.InterpolateSeventh(g1, 2),
				g0.InterpolateSeventh(g1, 3),
				g0.InterpolateSeventh(g1, 4),
				g0.InterpolateSeventh(g1, 5),
				g0.InterpolateSeventh(g1, 6),
			} : stackalloc byte[] {
				g0,
				g1,
				g0.InterpolateFifth(g1, 1),
				g0.InterpolateFifth(g1, 2),
				g0.InterpolateFifth(g1, 3),
				g0.InterpolateFifth(g1, 4),
				0,
				255
			};

			for (var i = 0; i < pixels.Length; i++)
			{
				var redIndex = GetRedIndex(i);
				var greenIndex = GetGreenIndex(i);
				pixels[i] = new Rgba32(reds[redIndex], greens[greenIndex], 0, 255);
			}

			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct AtcBlock
	{
		public ColorRgb555 color0;
		public ColorRgb565 color1;
		public uint colorIndices;

		public int this[int index]
		{
			readonly get => (int)(colorIndices >> (index * 2)) & 0b11;
			set
			{
				colorIndices = (uint)(colorIndices & ~(0b11 << (index * 2)));
				var val = value & 0b11;
				colorIndices = colorIndices | ((uint)val << (index * 2));
			}
		}

		public readonly RawBlock4X4Rgba32 Decode()
		{
			var output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var color0 = this.color0.ToColorRgb24();
			var color1 = this.color1.ToColorRgb24();

			Span<ColorRgb24> colors = stackalloc ColorRgb24[] {
				new ColorRgb24(0, 0, 0),
				color0.InterpolateFourthAtc(color1, 1),
				color0,
				color1
			};

			for (var i = 0; i < pixels.Length; i++)
			{
				var colorIndex = this[i];

				var color = this.color0.Mode == 0 ? color0.InterpolateThird(color1, colorIndex) : colors[colorIndex];

				pixels[i] = new Rgba32(color.r, color.g, color.b, 255);
			}
			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Bc2AlphaBlock
	{
		public ulong alphas;

		public readonly byte GetAlpha(int index)
		{
			var mask = 0xFUL << (index * 4);
			var shift = index * 4;
			var alphaUnscaled = (alphas & mask) >> shift;
			return (byte)(alphaUnscaled * 17);
		}

		public void SetAlpha(int index, byte alpha)
		{
			var mask = 0xFUL << (index * 4);
			var shift = index * 4;
			alphas &= ~mask;
			var a = (byte)(alpha / 17);
			alphas |= (ulong)(a & 0xF) << shift;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct AtcExplicitAlphaBlock
	{
		public Bc2AlphaBlock alphas;
		public AtcBlock colors;

		public readonly RawBlock4X4Rgba32 Decode()
		{
			var output = colors.Decode();
			var pixels = output.AsSpan;

			for (var i = 0; i < pixels.Length; i++)
			{
				pixels[i].A = alphas.GetAlpha(i);
			}
			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct AtcInterpolatedAlphaBlock
	{
		public Bc4Block alphas;
		public AtcBlock colors;

		public readonly RawBlock4X4Rgba32 Decode()
		{
			var output = colors.Decode();
			var pixels = output.AsSpan;

			for (var i = 0; i < pixels.Length; i++)
			{
				pixels[i].A = (byte)alphas.DecodeSingle(i);
			}
			return output;
		}
	}
}
