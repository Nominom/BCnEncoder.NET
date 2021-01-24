using System.IO;
using System.Linq;
using System.Text;

namespace BCnEncoder.Shared.ImageFiles
{
	/// <summary>
	/// The format identifier of an image file.
	/// </summary>
	public enum ImagefileFormat
	{
		/// <summary>
		/// Represents the KTX image file format.
		/// </summary>
		Ktx,

		/// <summary>
		/// Represents the DDS image file format.
		/// </summary>
		Dds,

		/// <summary>
		/// Represents an unknown image file format.
		/// </summary>
		Unknown
	}

	/// <summary>
	/// Static helper class to determine the format of an image file.
	/// </summary>
	public static class ImageFile
	{
		private static readonly byte[] ktx1Identifier = { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };

		/// <summary>
		/// Determines the image file format of the given stream.
		/// </summary>
		/// <param name="stream">The stream of data to identify.</param>
		/// <returns>The format this image file may contain.</returns>
		public static ImagefileFormat DetermineImageFormat(Stream stream)
		{
			if (IsDds(stream))
			{
				return ImagefileFormat.Dds;
			}

			if (IsKtx(stream))
			{
				return ImagefileFormat.Ktx;
			}

			return ImagefileFormat.Unknown;
		}

		private static bool IsDds(Stream stream)
		{
			using var br = new BinaryReader(stream, Encoding.UTF8, true);

			var magic = br.ReadUInt32();
			stream.Position -= 4;

			return magic == 0x20534444U;
		}

		private static bool IsKtx(Stream stream)
		{
			// Only checks for version 1
			using var br = new BinaryReader(stream, Encoding.ASCII, true);

			var identifier = br.ReadBytes(12);
			stream.Position -= 12;

			return identifier.SequenceEqual(ktx1Identifier);
		}
	}
}
