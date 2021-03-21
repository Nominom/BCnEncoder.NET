using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncTests.Support
{
	/// <summary>
	/// Reads .hdr RGBE/Radiance HDR files. File format by Gregory Ward
	/// </summary>
	public class HdrImage
	{
		public enum ColorSpace
		{
			Rgbe,
			Xyze
		}

		public float exposure = -1;
		public float gamma = -1;
		public int width;
		public int height;

		public ColorRgbFloat[] pixels;

		// StreamReader class does not work. Have to use custom string reading.
		private static string ReadFromStream(Stream stream)
		{
			var i = 0;
			var buffer = new char[512];
			char c;
			do
			{
				var b = stream.ReadByte();
				if (b == -1)
				{
					return null;
				}
				c = (char)b;
				buffer[i++] = c;
			} while (c != (char)10);
			return new string(buffer.AsSpan().Slice(0, i)).Trim();
		}

		public static HdrImage Read(string filename)
		{
			using var fs = File.OpenRead(filename);
			return Read(fs);
		}

		public static HdrImage Read(Stream stream)
		{
			HdrImage image = new HdrImage();

			var line = ReadFromStream(stream);

			if (!(line == "#?RGBE" || line == "#?RADIANCE" || line == "#?AUTOPANO"))
			{
				throw new FileLoadException("Correct file type specifier was not found.");
			}

			var colorSpace = ColorSpace.Rgbe;
			
			do
			{
				line = ReadFromStream(stream);

				if (line == null)
				{
					throw new FileLoadException("Reached end of stream.");
				}

				line = line.Trim();

				if (line == "")
				{
					break;
				}

				if (line.StartsWith("#")) // Found comment
				{
					continue;
				}

				if (line == "FORMAT=32-bit_rle_rgbe")
				{
					colorSpace = ColorSpace.Rgbe;
				}

				else if (line == "FORMAT=32-bit_rle_xyze")
				{
					colorSpace = ColorSpace.Xyze;
				}

				else if (line.StartsWith("EXPOSURE="))
				{
					image.exposure = float.Parse(line.Replace("EXPOSURE=", ""));
				}

				else if (line.StartsWith("GAMMA="))
				{
					image.gamma = float.Parse(line.Replace("GAMMA=", ""));
				}

			} while (true);

			if (image.exposure < 0.000001)
			{
				image.exposure = 1.0f;
			}

			if (image.gamma < 0.000001)
			{
				image.gamma = 1.0f;
			}

			var imgSize = ReadFromStream(stream).Split(' ');

			var yStr = imgSize[0];
			image.height = int.Parse(imgSize[1]);
			var xStr = imgSize[2];
			image.width = int.Parse(imgSize[3]);

			ReadPixels(image, stream);

			if (colorSpace == ColorSpace.Xyze)
			{
				// Transform colorspace
				var xyzColors = MemoryMarshal.Cast<ColorRgbFloat, ColorXyz>(image.pixels.AsSpan());
				for (var i = 0; i < xyzColors.Length; i++)
				{
					image.pixels[i] = xyzColors[i].ToColorRgbFloat();
				}
			}

			return image;
		}

		
		private static void RleReadChannel(BinaryReader br, Span<byte> dest, int width)
		{
			var i = 0;
			var data = new byte[2];
			while (i < width)
			{
				if (br.Read(data) == 0)
				{
					throw new FileLoadException("Not enough data in RLE");
				}
				if (data[0] > 128)
				{
					// same byte is repeated many times
					var len = data[0] - 128;
					for (; len > 0; len--)
					{
						dest[i++] = data[1];
					}
				}
				else
				{
					// different byte sequence
					dest[i++] = data[1];

					var len = data[0] - 1;
					if (len > 0)
					{
						if (br.Read(dest.Slice(i, len)) == 0)
						{
							throw new FileLoadException("Not enough data in RLE");
						}
						i += len;
					}
				}
			}

			if (i != width)
			{
				throw new FileLoadException("Scanline size was different from width");
			}
		}


		private static void ReadPixels(HdrImage destImage, Stream stream)
		{
			var height = destImage.height;
			var width = destImage.width;
			destImage.pixels = new ColorRgbFloat[destImage.height * destImage.width];
			Span<byte> bytes = new byte[destImage.width * 4];

			using var br = new BinaryReader(stream, Encoding.ASCII, true);

			var header = new byte[4];

			for (var y = 0; y < height; y++)
			{
				br.Read(header);

				var isRle = header[0] == 2 && header[1] == 2 &&
				             (header[2] << 8) + header[3] == width; // whether the scanline is rle or not
				
				if (isRle)
				{
					// for each channel
					for (var i = 0; i < 4; i++)
					{
						RleReadChannel(br, bytes.Slice(width * i, width), width);
					}

					for (var x = 0; x < width; x++)
					{
						ColorRgbe color = new ColorRgbe(
							bytes[x + width * 0],
							bytes[x + width * 1],
							bytes[x + width * 2],
							bytes[x + width * 3]
						);

						destImage.pixels[y * width + x] = color.ToColorRgbFloat(destImage.exposure);
					}
				}
				else
				{
					br.Read(bytes.Slice(4));
					header.CopyTo(bytes);

					for (var x = 0; x < width; x++)
					{
						ColorRgbe color = new ColorRgbe(
							bytes[4 * x + 0],
							bytes[4 * x + 1],
							bytes[4 * x + 2],
							bytes[4 * x + 3]
						);

						destImage.pixels[y * width + x] = color.ToColorRgbFloat(destImage.exposure);
					}
				}
			}
		}
	}
}
