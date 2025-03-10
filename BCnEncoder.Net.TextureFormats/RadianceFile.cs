using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BCnEncoder.Shared;

namespace BCnEncoder.TextureFormats
{
	public class RadianceFile : ITextureFileFormat<RadianceFile>
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
		public ColorSpace colorSpace;

		public byte[]? pixelData;


		/// <inheritdoc />
		public bool SupportsLdr => false;

		/// <inheritdoc />
		public bool SupportsHdr => true;

		/// <inheritdoc />
		public bool SupportsCubeMap => false;

		/// <inheritdoc />
		public bool SupportsMipMaps => false;

		/// <inheritdoc />
		public bool SupportsArrays => false;

		/// <inheritdoc />
		public bool IsSupportedFormat(CompressionFormat format)
		{
			return format == CompressionFormat.Rgbe || format == CompressionFormat.Xyze;
		}

		/// <inheritdoc />
		public void FromTextureData(BCnTextureData textureData)
		{
			if (!IsSupportedFormat(textureData.Format))
			{
				textureData = textureData.ConvertTo(CompressionFormat.Rgbe);
			}

			width = textureData.Width;
			height = textureData.Height;
			colorSpace = textureData.Format == CompressionFormat.Rgbe ? ColorSpace.Rgbe : ColorSpace.Xyze;
			pixelData = textureData.First.Data;
			exposure = 1;
			gamma = 1;
		}

		/// <inheritdoc />
		public BCnTextureData ToTextureData()
		{
			return BCnTextureData.FromSingle(
				colorSpace == ColorSpace.Rgbe ? CompressionFormat.Rgbe : CompressionFormat.Xyze,
				width,
				height,
				pixelData);
		}


		// StreamReader class does not work. Have to use custom string reading.
		private static string ReadLineFromStream(Stream stream)
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
				var b = (byte)c;
				br.Write(b);
			}
			br.Write((byte)10);
		}


		/// <inheritdoc />
		public void ReadFromStream(Stream stream)
		{

			var line = ReadLineFromStream(stream);

			if (!(line == "#?RGBE" || line == "#?RADIANCE" || line == "#?AUTOPANO"))
			{
				throw new FileLoadException("Correct file type specifier was not found.");
			}

			colorSpace = ColorSpace.Rgbe;

			do
			{
				line = ReadLineFromStream(stream);

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
					exposure = float.Parse(line.Replace("EXPOSURE=", "").Trim(), CultureInfo.InvariantCulture);
				}

				else if (line.StartsWith("GAMMA="))
				{
					gamma = float.Parse(line.Replace("GAMMA=", "").Trim(), CultureInfo.InvariantCulture);
				}

			} while (true);

			if (exposure < 0.000001)
			{
				exposure = 1.0f;
			}

			if (gamma < 0.000001)
			{
				gamma = 1.0f;
			}

			var imgSize = Regex.Replace(ReadLineFromStream(stream), @"\s+", " ").Split(' ');
			var yStr = imgSize[0];
			height = int.Parse(imgSize[1]);
			var xStr = imgSize[2];
			width = int.Parse(imgSize[3]);

			pixelData = new byte[width * height * 4];

			ReadPixels(stream);
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

		private void ReadPixels(Stream stream)
		{
			pixelData = new byte[width * height * 4];

			Span<byte> lineBytes = new byte[width * 4];

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
						RleReadChannel(br, lineBytes.Slice(width * i, width), width);
					}

					for (var x = 0; x < width; x++)
					{
						pixelData[y * width * 4 + x * 4 + 0] = lineBytes[x + width * 0];
						pixelData[y * width * 4 + x * 4 + 1] = lineBytes[x + width * 1];
						pixelData[y * width * 4 + x * 4 + 2] = lineBytes[x + width * 2];
						pixelData[y * width * 4 + x * 4 + 3] = lineBytes[x + width * 3];
					}
				}
				else
				{
					br.Read(lineBytes.Slice(4));
					header.CopyTo(lineBytes);

					lineBytes.CopyTo(pixelData.AsSpan().Slice(y * width * 4, width * 4));
				}
			}
		}

		/// <inheritdoc />
		public void WriteToStream(Stream stream)
		{
			using var br = new BinaryWriter(stream, Encoding.ASCII, true);

			WriteLineToStream(br, "#?RADIANCE");
			WriteLineToStream(br, "# Made with BCnEncoder.Net");
			if (colorSpace == ColorSpace.Rgbe)
			{
				WriteLineToStream(br, "FORMAT=32-bit_rle_rgbe");
			}
			else
			{
				WriteLineToStream(br, "FORMAT=32-bit_rle_xyze");
			}
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
			if (pixelData == null)
			{
				throw new NullReferenceException("pixelData is null.");
			}

			br.Write(pixelData);
		}

		public static RadianceFile Load(Stream stream)
		{
			var tex = new RadianceFile();
			tex.ReadFromStream(stream);
			return tex;
		}
	}
}
