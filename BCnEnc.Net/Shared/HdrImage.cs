using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Shared
{
	/// <summary>
	/// Reads and writes .hdr RGBE/Radiance HDR files. File format by Gregory Ward.
	/// This class is experimental, incomplete and probably going to be removed in a future version.
	/// Use only if you don't have anything better.
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

		/// <summary>
		/// Gets a span2D over the <see cref="pixels"/> array.
		/// </summary>
		public Span2D<ColorRgbFloat> PixelSpan => new Span2D<ColorRgbFloat>(pixels, height, width);
		/// <summary>
		/// Gets a span2D over the <see cref="pixels"/> array.
		/// </summary>
		public Memory2D<ColorRgbFloat> PixelMemory => new Memory2D<ColorRgbFloat>(pixels, height, width);

		public HdrImage() {}

		public HdrImage(int width, int height, float exposure = 0, float gamma = 0)
		{
			this.width = width;
			this.height = height;
			this.exposure = exposure;
			this.gamma = gamma;
			this.pixels = new ColorRgbFloat[width * height];
		}

		public HdrImage(Span2D<ColorRgbFloat> pixels, float exposure = 0, float gamma = 0)
		{
			this.width = pixels.Width;
			this.height = pixels.Height;
			this.exposure = exposure;
			this.gamma = gamma;
			this.pixels = new ColorRgbFloat[width * height];
			pixels.CopyTo(this.pixels);
		}

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
		
		private static void WriteLineToStream(BinaryWriter br, string s)
		{
			foreach (var c in s)
			{
				var b = (byte) c;
				br.Write(b);
			}
			br.Write((byte)10);
		}

		/// <summary>
		/// Read a Radiance HDR image by filename.
		/// Just calls <see cref="Read(Stream)"/> internally.
		/// </summary>
		/// <param name="filename">The filename or path of the image</param>
		/// <returns>A new HdrImage with the data</returns>
		public static HdrImage Read(string filename)
		{
			using var fs = File.OpenRead(filename);
			return Read(fs);
		}

		/// <summary>
		/// Read a Radiance HDR image from a stream
		/// </summary>
		/// <param name="stream">The stream to read from</param>
		/// <returns>A new HdrImage with the data</returns>
		public static HdrImage Read(Stream stream)
		{
			var image = new HdrImage();

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
					image.exposure = float.Parse(line.Replace("EXPOSURE=", "").Trim(), CultureInfo.InvariantCulture);
				}

				else if (line.StartsWith("GAMMA="))
				{
					image.gamma = float.Parse(line.Replace("GAMMA=", "").Trim(), CultureInfo.InvariantCulture);
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
						var color = new ColorRgbe(
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
						var color = new ColorRgbe(
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

		/// <summary>
		/// Write this file to a stream.
		/// </summary>
		/// <param name="stream">The stream to write it to.</param>
		public void Write(Stream stream)
		{
			using var br = new BinaryWriter(stream, Encoding.ASCII, true);

			WriteLineToStream(br, "#?RADIANCE");
			WriteLineToStream(br, "# BCnEncoder.Net HdrImage");
			WriteLineToStream(br, "FORMAT=32-bit_rle_rgbe");
			if (exposure > 0)
			{
				WriteLineToStream(br, "EXPOSURE=" + exposure.ToString(CultureInfo.InvariantCulture));
			}
			if (gamma > 0)
			{
				WriteLineToStream(br, "GAMMA=" + gamma.ToString(CultureInfo.InvariantCulture));
			}
			
			WriteLineToStream(br, ""); // Start data with empty row
			WriteLineToStream(br, $"-Y {height} +X {width}");
			
			WritePixels(br);
		}

		private void WritePixels(BinaryWriter br)
		{
			var buffer = new byte[4];
			var span = PixelSpan;
			
			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					var pixel = span[y, x];
					var rgbe = new ColorRgbe(pixel);
					
					buffer[0] = rgbe.r;
					buffer[1] = rgbe.g;
					buffer[2] = rgbe.b;
					buffer[3] = rgbe.e;

					br.Write(buffer);
				}
			}
		}
	}
}
