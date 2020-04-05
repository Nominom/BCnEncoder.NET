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

		public int this[int index] {
			readonly get => (int)(colorIndices >> (index * 2)) & 0b11;
			set {
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
				if (useAlpha && colorIndex == 3) {
					pixels[i] = new Rgba32(0,0,0,0);
				}
				else
				{
					pixels[i] = new Rgba32(color.r, color.g, color.b, 255);
				}
			}
			return output;
		}
	}
}
