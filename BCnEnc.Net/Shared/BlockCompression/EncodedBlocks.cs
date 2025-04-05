using System;
using System.Runtime.InteropServices;
using BCnEncoder.Encoder;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Shared
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct Bc1Block
	{
		public ColorB5G6R5Packed color0;
		public ColorB5G6R5Packed color1;
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

		public readonly RawBlock4X4RgbaFloat Decode(bool useColorComparisonModeSwitch, bool useAlpha)
		{
			var output = new RawBlock4X4RgbaFloat();
			var pixels = output.AsSpan;

			var color0 = this.color0.ToColorRgbaFloat();
			var color1 = this.color1.ToColorRgbaFloat();

			useAlpha = useAlpha && HasAlphaOrBlack;

			var colors = HasAlphaOrBlack && useColorComparisonModeSwitch ?
				stackalloc ColorRgbaFloat[] {
				color0,
				color1,
				Interpolation.InterpolateColor(color0, color1, .5f),
				new ColorRgbaFloat(0, 0, 0)
			} : stackalloc ColorRgbaFloat[] {
				color0,
				color1,
				Interpolation.InterpolateColor(color0, color1, 1f / 3f),
				Interpolation.InterpolateColor(color0, color1, 2f / 3f)
			};

			for (var i = 0; i < pixels.Length; i++)
			{
				var colorIndex = (int)((colorIndices >> (i * 2)) & 0b11);
				var color = colors[colorIndex];
				if (useAlpha && colorIndex == 3)
				{
					pixels[i] = new ColorRgbaFloat(0, 0, 0, 0);
				}
				else
				{
					pixels[i] = color;
				}
			}

			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Bc2Block
	{
		public Bc2AlphaBlock alphaBlock;
		public Bc1Block colorBlock;

		public int this[int index]
		{
			readonly get => colorBlock[index];
			set => colorBlock[index] = value;
		}

		public readonly float GetAlpha(int index) => alphaBlock.GetAlpha(index);

		public void SetAlpha(int index, float alpha) => alphaBlock.SetAlpha(index, alpha);

		public readonly RawBlock4X4RgbaFloat Decode()
		{
			var output = colorBlock.Decode(false, false);
			var pixels = output.AsSpan;

			for (var i = 0; i < pixels.Length; i++)
			{
				pixels[i].a = GetAlpha(i);
			}

			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Bc3Block
	{
		public Bc4ComponentBlock alphaBlock;
		public Bc1Block colorBlock;

		public readonly RawBlock4X4RgbaFloat Decode()
		{
			var output = colorBlock.Decode(false, false);
			var pixels = output.AsSpan;

			var alphas = alphaBlock.Decode();

			for (var i = 0; i < pixels.Length; i++)
			{
				pixels[i].a = alphas[i];
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

		public readonly RawBlock4X4RgbaFloat Decode(ColorComponent component = ColorComponent.R)
		{
			var output = new RawBlock4X4RgbaFloat();
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

		public readonly byte GetRedIndex(int pixelIndex) => redBlock.GetComponentIndex(pixelIndex);

		public void SetRedIndex(int pixelIndex, byte redIndex) => redBlock.SetComponentIndex(pixelIndex, redIndex);

		public readonly byte GetGreenIndex(int pixelIndex) => greenBlock.GetComponentIndex(pixelIndex);

		public void SetGreenIndex(int pixelIndex, byte greenIndex) => greenBlock.SetComponentIndex(pixelIndex, greenIndex);

		public readonly RawBlock4X4RgbaFloat Decode(ColorComponent component1 = ColorComponent.R, ColorComponent component2 = ColorComponent.G, ColorComponent componentCalculated = ColorComponent.None)
		{
			var output = new RawBlock4X4RgbaFloat();
			var pixels = output.AsSpan;

			var reds = redBlock.Decode();
			var greens = greenBlock.Decode();

			for (var i = 0; i < pixels.Length; i++)
			{
				ColorRgbaFloat color = ComponentHelper.ComponentToColor(component1, reds[i]);
				ComponentHelper.ComponentToColor(ref color, component2, greens[i]);

				// Calculate Z
				if (componentCalculated != ColorComponent.None)
				{
					float x = reds[i] * 2 - 1;
					float y = greens[i] * 2 - 1;
					float z = 1 - x * x - y * y;

					if (z < 0)
						z = 0;
					else
						z = MathF.Sqrt(z);

					ComponentHelper.ComponentToColor(ref color, componentCalculated, z);
				}

				pixels[i] = color;
			}

			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct AtcBlock
	{
		public ColorB5G5R5M1Packed color0;
		public ColorB5G6R5Packed color1;
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

		public readonly RawBlock4X4RgbaFloat Decode()
		{
			var output = new RawBlock4X4RgbaFloat();
			var pixels = output.AsSpan;

			var color0 = this.color0.ToColorRgbaFloat();
			var color1 = this.color1.ToColorRgbaFloat();

			Span<ColorRgbaFloat> colors = stackalloc ColorRgbaFloat[] {
				new ColorRgbaFloat(0, 0, 0),
				color0.InterpolateFourthAtc(color1, 1),
				color0,
				color1
			};

			for (var i = 0; i < pixels.Length; i++)
			{
				var colorIndex = this[i];

				var color = this.color0.Mode == 0 ? color0.InterpolateThird(color1, colorIndex) : colors[colorIndex];

				pixels[i] = color;
			}
			return output;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Bc2AlphaBlock
	{
		public ulong alphas;

		public readonly float GetAlpha(int index)
		{
			var mask = 0xFUL << (index * 4);
			var shift = index * 4;

			// Unorm4 alpha
			var alphaUnscaled = (alphas & mask) >> shift;
			return ColorBitConversionHelpers.Unorm4ToFloat((uint)alphaUnscaled);
		}

		public void SetAlpha(int index, float alpha)
		{
			var mask = 0xFUL << (index * 4);
			var shift = index * 4;
			alphas &= ~mask;
			var a = ColorBitConversionHelpers.FloatToUnorm4(alpha);
			alphas |= (ulong)(a & 0xF) << shift;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Bc4ComponentBlock
	{
		public ulong componentBlock;

		public float FloatEndpoint0
		{
			readonly get => ColorBitConversionHelpers.Unorm8ToFloat((byte)(componentBlock & 0xFFUL));
			set
			{
				componentBlock &= ~0xFFUL;
				componentBlock |= ColorBitConversionHelpers.FloatToUnorm8(value);
			}
		}

		public float FloatEndpoint1
		{
			readonly get =>ColorBitConversionHelpers.Unorm8ToFloat((byte)((componentBlock >> 8) & 0xFFUL));
			set
			{
				componentBlock &= ~0xFF00UL;
				componentBlock |= (ulong)ColorBitConversionHelpers.FloatToUnorm8(value) << 8;
			}
		}

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

		public readonly float[] Decode()
		{
			var output = new float[16];

			var c0 = FloatEndpoint0;
			var c1 = FloatEndpoint1;

			var components = c0 > c1 ? stackalloc float[] {
				c0,
				c1,
				c0.InterpolateSeventh(c1, 1),
				c0.InterpolateSeventh(c1, 2),
				c0.InterpolateSeventh(c1, 3),
				c0.InterpolateSeventh(c1, 4),
				c0.InterpolateSeventh(c1, 5),
				c0.InterpolateSeventh(c1, 6),
			} : stackalloc float[] {
				c0,
				c1,
				c0.InterpolateFifth(c1, 1),
				c0.InterpolateFifth(c1, 2),
				c0.InterpolateFifth(c1, 3),
				c0.InterpolateFifth(c1, 4),
				0,
				1
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

		public readonly RawBlock4X4RgbaFloat Decode()
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

		public readonly RawBlock4X4RgbaFloat Decode()
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
