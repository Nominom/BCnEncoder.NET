using System;
using System.Runtime.InteropServices;
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
        public ulong alphaColors;
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

        public readonly byte GetAlpha(int index)
        {
            var mask = 0xFUL << (index * 4);
            var shift = index * 4;
            var alphaUnscaled = (alphaColors & mask) >> shift;
            return (byte)(alphaUnscaled / 15.0 * 255);
        }

        public void SetAlpha(int index, byte alpha)
        {
            var mask = 0xFUL << (index * 4);
            var shift = index * 4;
            alphaColors &= ~mask;
            var a = (byte)(alpha / 255.0 * 15);
            alphaColors |= (ulong)(a & 0xF) << shift;
        }

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
        public ulong alphaBlock;
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
            var mask = 0b0111UL << (pixelIndex * 3 + 16);
            var shift = pixelIndex * 3 + 16;
            var alphaIndex = (alphaBlock & mask) >> shift;
            return (byte)alphaIndex;
        }

        public void SetAlphaIndex(int pixelIndex, byte alphaIndex)
        {
            var mask = 0b0111UL << (pixelIndex * 3 + 16);
            var shift = pixelIndex * 3 + 16;
            alphaBlock &= ~mask;
            alphaBlock |= (ulong)(alphaIndex & 0b111) << shift;
        }

        public readonly RawBlock4X4Rgba32 Decode()
        {
            var output = new RawBlock4X4Rgba32();
            var pixels = output.AsSpan;

            var color0 = this.color0.ToColorRgb24();
            var color1 = this.color1.ToColorRgb24();

            var a0 = Alpha0;
            var a1 = Alpha1;
            Span<ColorRgb24> colors = stackalloc ColorRgb24[] {
                color0,
                color1,
                color0.InterpolateThird(color1, 1),
                color0.InterpolateThird(color1, 2)
            };

            var alphas = a0 > a1 ? stackalloc byte[] {
                a0,
                a1,
                (byte)Interpolation.Interpolate(a0, a1, 1, 7),
                (byte)Interpolation.Interpolate(a0, a1, 2, 7),
                (byte)Interpolation.Interpolate(a0, a1, 3, 7),
                (byte)Interpolation.Interpolate(a0, a1, 4, 7),
                (byte)Interpolation.Interpolate(a0, a1, 5, 7),
                (byte)Interpolation.Interpolate(a0, a1, 6, 7)
            } : stackalloc byte[] {
                a0,
                a1,
                (byte)Interpolation.Interpolate(a0, a1, 1, 5),
                (byte)Interpolation.Interpolate(a0, a1, 2, 5),
                (byte)Interpolation.Interpolate(a0, a1, 3, 5),
                (byte)Interpolation.Interpolate(a0, a1, 4, 5),
                0,
                255
            };

            for (var i = 0; i < pixels.Length; i++)
            {
                var colorIndex = (int)((colorIndices >> (i * 2)) & 0b11);
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

        public readonly RawBlock4X4Rgba32 Decode(bool redAsLuminance)
        {
            var output = new RawBlock4X4Rgba32();
            var pixels = output.AsSpan;

            var r0 = Red0;
            var r1 = Red1;

            var reds = r0 > r1 ? stackalloc byte[] {
                r0,
                r1,
                (byte)Interpolation.Interpolate(r0, r1, 1, 7),
                (byte)Interpolation.Interpolate(r0, r1, 2, 7),
                (byte)Interpolation.Interpolate(r0, r1, 3, 7),
                (byte)Interpolation.Interpolate(r0, r1, 4, 7),
                (byte)Interpolation.Interpolate(r0, r1, 5, 7),
                (byte)Interpolation.Interpolate(r0, r1, 6, 7)
            } : stackalloc byte[] {
                r0,
                r1,
                (byte)Interpolation.Interpolate(r0, r1, 1, 5),
                (byte)Interpolation.Interpolate(r0, r1, 2, 5),
                (byte)Interpolation.Interpolate(r0, r1, 3, 5),
                (byte)Interpolation.Interpolate(r0, r1, 4, 5),
                0,
                255
            };

            if (redAsLuminance)
            {
                for (var i = 0; i < pixels.Length; i++)
                {
                    var index = GetRedIndex(i);
                    pixels[i] = new Rgba32(reds[index], reds[index], reds[index], 255);
                }
            }
            else
            {
                for (var i = 0; i < pixels.Length; i++)
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
                (byte)Interpolation.Interpolate(r0, r1, 1, 7),
                (byte)Interpolation.Interpolate(r0, r1, 2, 7),
                (byte)Interpolation.Interpolate(r0, r1, 3, 7),
                (byte)Interpolation.Interpolate(r0, r1, 4, 7),
                (byte)Interpolation.Interpolate(r0, r1, 5, 7),
                (byte)Interpolation.Interpolate(r0, r1, 6, 7)
            } : stackalloc byte[] {
                r0,
                r1,
                (byte)Interpolation.Interpolate(r0, r1, 1, 5),
                (byte)Interpolation.Interpolate(r0, r1, 2, 5),
                (byte)Interpolation.Interpolate(r0, r1, 3, 5),
                (byte)Interpolation.Interpolate(r0, r1, 4, 5),
                0,
                255
            };

            var g0 = Green0;
            var g1 = Green1;

            var greens = g0 > g1 ? stackalloc byte[] {
                g0,
                g1,
                (byte)Interpolation.Interpolate(g0, g1, 1, 7),
                (byte)Interpolation.Interpolate(g0, g1, 2, 7),
                (byte)Interpolation.Interpolate(g0, g1, 3, 7),
                (byte)Interpolation.Interpolate(g0, g1, 4, 7),
                (byte)Interpolation.Interpolate(g0, g1, 5, 7),
                (byte)Interpolation.Interpolate(g0, g1, 6, 7)
            } : stackalloc byte[] {
                g0,
                g1,
                (byte)Interpolation.Interpolate(g0, g1, 1, 5),
                (byte)Interpolation.Interpolate(g0, g1, 2, 5),
                (byte)Interpolation.Interpolate(g0, g1, 3, 5),
                (byte)Interpolation.Interpolate(g0, g1, 4, 5),
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

}
