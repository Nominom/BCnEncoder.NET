using System;
using System.Collections.Generic;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncoder.TextureFormats
{
	internal static class TextureUnPacker
	{
		public static byte[] UnPackTexture(byte[] texData, int width, int height, CompressionFormat format, int byteAlignment)
		{
			if (format.IsBlockCompressedFormat())
			{
				throw new ArgumentException("Format must be a non-block-compressed format", nameof(format));
			}

			var lineBytes = format.BytesPerBlock() * width;

			if (texData.Length != lineBytes * height)
			{
				throw new ArgumentException("Trying to un-pack a texture with unexpected size!", nameof(texData));
			}

			var alignedLineBytes = ((lineBytes + (byteAlignment - 1)) / byteAlignment) * byteAlignment;

			if (alignedLineBytes == lineBytes)
			{
				return texData;
			}

			var newData = new byte[alignedLineBytes * height];
			for (var y = 0; y < height; y++)
			{
				var oldLine = texData.AsSpan(y * lineBytes, lineBytes);
				var newLine = newData.AsSpan(y * alignedLineBytes, alignedLineBytes);
				oldLine.CopyTo(newLine);
			}
			return newData;
		}

		public static byte[] PackTexture(byte[] texData, int width, int height, CompressionFormat format, int byteAlignment)
		{
			if (format.IsBlockCompressedFormat())
			{
				throw new ArgumentException("Format must be a non-block-compressed format", nameof(format));
			}

			var lineBytes = format.BytesPerBlock() * width;
			var alignedLineBytes = ((lineBytes + (byteAlignment - 1)) / byteAlignment) * byteAlignment;

			if (lineBytes == alignedLineBytes && lineBytes == texData.Length)
			{
				return texData;
			}

			if (texData.Length != alignedLineBytes * height)
			{
				if (texData.Length == lineBytes * height) // Already packed
				{
					return texData;
				}

				throw new ArgumentException("Trying to pack a texture with unexpected size!", nameof(texData));
			}

			var newData = new byte[lineBytes * height];
			for (var y = 0; y < height; y++)
			{
				var oldLine = texData.AsSpan(y * alignedLineBytes, lineBytes);
				var newLine = newData.AsSpan(y * lineBytes, lineBytes);
				oldLine.CopyTo(newLine);
			}
			return newData;
		}
	}
}
