using System;
using System.Runtime.InteropServices;
using BCnEncoder.Encoder;

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
					pixels[i] = new ColorRgba32(0, 0, 0, 0);
				}
				else
				{
					pixels[i] = new ColorRgba32(color.r, color.g, color.b, 255);
				}
			}
			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Bc2Block
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

				pixels[i] = new ColorRgba32(color.r, color.g, color.b, GetAlpha(i));
			}
			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Bc3Block
	{
		public Bc4ComponentBlock alphaBlock;
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

			var alphas = alphaBlock.Decode();

			for (var i = 0; i < pixels.Length; i++)
			{
				var colorIndex = (int)((colorIndices >> (i * 2)) & 0b11);
				var color = colors[colorIndex];

				pixels[i] = new ColorRgba32(color.r, color.g, color.b, alphas[i]);
			}
			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Bc4Block
	{
		public Bc4ComponentBlock componentBlock;

		public byte Endpoint0
		{
			readonly get => componentBlock.Endpoint0;
			set => componentBlock.Endpoint0 = value;
		}

		public byte Endpoint1
		{
			readonly get => componentBlock.Endpoint1;
			set => componentBlock.Endpoint1 = value;
		}

		public readonly byte GetComponentIndex(int pixelIndex) => componentBlock.GetComponentIndex(pixelIndex);

		public void SetComponentIndex(int pixelIndex, byte redIndex) => componentBlock.SetComponentIndex(pixelIndex, redIndex);

		public readonly RawBlock4X4Rgba32 Decode(ColorComponent component = ColorComponent.R)
		{
			var output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var components = componentBlock.Decode();

			for (var i = 0; i < pixels.Length; i++)
			{
				pixels[i] = ComponentHelper.ComponentToColor(component, components[i]);
			}

			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Bc5Block
	{
		public Bc4ComponentBlock redBlock;
		public Bc4ComponentBlock greenBlock;

		public byte Red0
		{
			readonly get => redBlock.Endpoint0;
			set => redBlock.Endpoint0 = value;
		}

		public byte Red1
		{
			readonly get => redBlock.Endpoint1;
			set => redBlock.Endpoint1 = value;
		}

		public byte Green0
		{
			readonly get => greenBlock.Endpoint0;
			set => greenBlock.Endpoint0 = value;
		}

		public byte Green1
		{
			readonly get => greenBlock.Endpoint1;
			set => greenBlock.Endpoint1 = value;
		}

		public readonly byte GetRedIndex(int pixelIndex) => redBlock.GetComponentIndex(pixelIndex);

		public void SetRedIndex(int pixelIndex, byte redIndex) => redBlock.SetComponentIndex(pixelIndex, redIndex);

		public readonly byte GetGreenIndex(int pixelIndex) => greenBlock.GetComponentIndex(pixelIndex);

		public void SetGreenIndex(int pixelIndex, byte greenIndex) => greenBlock.SetComponentIndex(pixelIndex, greenIndex);

		public readonly RawBlock4X4Rgba32 Decode(ColorComponent component1 = ColorComponent.R, ColorComponent component2 = ColorComponent.G, bool recalculateBlueChannel = false)
		{
			var output = new RawBlock4X4Rgba32();
			var pixels = output.AsSpan;

			var reds = redBlock.Decode();
			var greens = greenBlock.Decode();

			var blues = new byte[pixels.Length];
			if (recalculateBlueChannel)
			{
				for (var i = 0; i < pixels.Length; i++)
				{
					var red = (float) reds[i] / 255;
					var green = (float) greens[i] / 255;

					var blue = 1 - red * red - green * green;
					blue = blue < 0 ? 0f : (float) Math.Sqrt(blue);
					blue = Math.Clamp((blue + 1) / 2f, 0f, 1f);

					blues[i] = (byte) (blue * 255);
				}
			}

			for (var i = 0; i < pixels.Length; i++)
			{
				pixels[i] = ComponentHelper.ComponentToColor(component1, reds[i]);
				pixels[i] = ComponentHelper.ComponentToColor(pixels[i], component2, greens[i]);
				pixels[i] = ComponentHelper.ComponentToColor(pixels[i], ColorComponent.B, blues[i]);
			}

			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct AtcBlock
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

				pixels[i] = new ColorRgba32(color.r, color.g, color.b, 255);
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
	internal struct Bc4ComponentBlock
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

		public readonly byte[] Decode()
		{
			var output = new byte[16];

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

			for (var i = 0; i < output.Length; i++)
			{
				var index = GetComponentIndex(i);
				output[i] = components[index];
			}

			return output;
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
				pixels[i].a = alphas.GetAlpha(i);
			}
			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct AtcInterpolatedAlphaBlock
	{
		public Bc4ComponentBlock alphas;
		public AtcBlock colors;

		public readonly RawBlock4X4Rgba32 Decode()
		{
			var output = colors.Decode();
			var pixels = output.AsSpan;

			var componentValues = alphas.Decode();

			for (var i = 0; i < pixels.Length; i++)
			{
				pixels[i].a = componentValues[i];
			}
			return output;
		}
	}
}
