using System;
using System.IO;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;

namespace BCnEncoder.Shared
{
	public class BcStream : Stream
	{
		private readonly Stream baseStream;
		private readonly IBcBlockDecoder decoder;
		private readonly IBcBlockEncoder encoder;

		public override bool CanRead => baseStream.CanRead;
		public override bool CanSeek => baseStream.CanSeek;
		public override bool CanWrite => baseStream.CanWrite;
		public override long Length => baseStream.Length;
		public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

		public BcStream(Stream baseStream, CompressionFormat format)
		{
			this.baseStream = baseStream;

			decoder = BcDecoder.GetDecoder(format, false);
			encoder = BcEncoder.GetEncoder(format, false);
		}

		public override void Flush()
		{
			baseStream.Flush();
		}

		public override void SetLength(long value)
		{
			throw new System.NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return baseStream.Seek(offset, origin);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			// TODO: What to read here?
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			// TODO: What to write here?
			throw new NotImplementedException();
		}

		public int Read(RawBlock4X4Rgba32[] blocks, int offset, int count)
		{
			// Align position to low block boundary
			var blockPosition = Position / decoder.BlockSize;
			baseStream.Position = blockPosition;

			// Cap count
			count = (int)Math.Min(count, (Length - blockPosition) / decoder.BlockSize);

			// Decode 'count' blocks
			var block = new byte[decoder.BlockSize];
			for (var i = 0; i < count; i++)
			{
				baseStream.Read(block, 0, decoder.BlockSize);
				blocks[offset++] = decoder.DecodeBlock(block);
			}

			return count;
		}

		public void Write(RawBlock4X4Rgba32[] blocks, int offset, int count)
		{
			// Align position to low block boundary
			var blockPosition = Position / encoder.BlockSize;
			baseStream.Position = blockPosition;

			// Cap count
			count = Math.Min(count, blocks.Length - offset);

			// Encode 'count' blocks
			for (var i = 0; i < count; i++)
			{
				baseStream.Write(encoder.EncodeBlock(blocks[offset++], CompressionQuality.Balanced));
			}
		}
	}
}
