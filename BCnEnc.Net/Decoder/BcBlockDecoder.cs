using System;
using System.IO;
using System.Runtime.InteropServices;
using BCnEncoder.Shared;

namespace BCnEncoder.Decoder
{
	internal interface IBcBlockDecoder {
		RawBlock4X4Rgba32[,] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight, out int blockWidth,
			out int blockHeight);
	}

	internal class Bc1NoAlphaDecoder : IBcBlockDecoder {
		public RawBlock4X4Rgba32[,] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight) {
			blockWidth = (int)MathF.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)MathF.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * Marshal.SizeOf<Bc1Block>())) {
				throw new InvalidDataException();
			}

			var encodedBlocks = MemoryMarshal.Cast<byte, Bc1Block>(data);

			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			for (int x = 0; x < blockWidth; x++) {
				for (int y = 0; y < blockHeight; y++) {
					output[x, y] = encodedBlocks[x + y * blockWidth].Decode(false);
				}
			}

			return output;
		}
	}

	internal class Bc1ADecoder : IBcBlockDecoder {
		public RawBlock4X4Rgba32[,] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight) {
			blockWidth = (int)MathF.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)MathF.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * Marshal.SizeOf<Bc1Block>())) {
				throw new InvalidDataException();
			}

			var encodedBlocks = MemoryMarshal.Cast<byte, Bc1Block>(data);

			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			for (int x = 0; x < blockWidth; x++) {
				for (int y = 0; y < blockHeight; y++) {
					output[x, y] = encodedBlocks[x + y * blockWidth].Decode(true);
				}
			}

			return output;
		}
	}

	internal class Bc2Decoder : IBcBlockDecoder {
		public RawBlock4X4Rgba32[,] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight) {
			blockWidth = (int)MathF.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)MathF.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * Marshal.SizeOf<Bc2Block>())) {
				throw new InvalidDataException();
			}

			var encodedBlocks = MemoryMarshal.Cast<byte, Bc2Block>(data);

			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			for (int x = 0; x < blockWidth; x++) {
				for (int y = 0; y < blockHeight; y++) {
					output[x, y] = encodedBlocks[x + y * blockWidth].Decode();
				}
			}

			return output;
		}
	}

	internal class Bc3Decoder : IBcBlockDecoder {
		public RawBlock4X4Rgba32[,] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight) {
			blockWidth = (int)MathF.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)MathF.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * Marshal.SizeOf<Bc3Block>())) {
				throw new InvalidDataException();
			}

			var encodedBlocks = MemoryMarshal.Cast<byte, Bc3Block>(data);

			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			for (int x = 0; x < blockWidth; x++) {
				for (int y = 0; y < blockHeight; y++) {
					output[x, y] = encodedBlocks[x + y * blockWidth].Decode();
				}
			}

			return output;
		}
	}

	internal class Bc4Decoder : IBcBlockDecoder {
		private readonly bool redAsLuminance;
		public Bc4Decoder(bool redAsLuminance) {
			this.redAsLuminance = redAsLuminance;
		}

		public RawBlock4X4Rgba32[,] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight) {
			blockWidth = (int)MathF.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)MathF.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * Marshal.SizeOf<Bc4Block>())) {
				throw new InvalidDataException();
			}

			var encodedBlocks = MemoryMarshal.Cast<byte, Bc4Block>(data);

			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			for (int x = 0; x < blockWidth; x++) {
				for (int y = 0; y < blockHeight; y++) {
					output[x, y] = encodedBlocks[x + y * blockWidth].Decode(redAsLuminance);
				}
			}

			return output;
		}
	}

	internal class Bc5Decoder : IBcBlockDecoder {

		public RawBlock4X4Rgba32[,] Decode(ReadOnlySpan<byte> data, int pixelWidth, int pixelHeight, out int blockWidth, out int blockHeight) {
			blockWidth = (int)MathF.Ceiling(pixelWidth / 4.0f);
			blockHeight = (int)MathF.Ceiling(pixelHeight / 4.0f);

			if (data.Length != (blockWidth * blockHeight * Marshal.SizeOf<Bc5Block>())) {
				throw new InvalidDataException();
			}

			var encodedBlocks = MemoryMarshal.Cast<byte, Bc5Block>(data);

			RawBlock4X4Rgba32[,] output = new RawBlock4X4Rgba32[blockWidth, blockHeight];

			for (int x = 0; x < blockWidth; x++) {
				for (int y = 0; y < blockHeight; y++) {
					output[x, y] = encodedBlocks[x + y * blockWidth].Decode();
				}
			}

			return output;
		}
	}
}
