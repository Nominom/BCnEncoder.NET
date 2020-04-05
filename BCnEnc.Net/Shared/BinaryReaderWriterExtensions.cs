using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace BCnEnc.Net.Shared
{
	public static class BinaryReaderWriterExtensions
	{
		public static unsafe void WriteStruct<T>(this BinaryWriter bw, T t) where T : unmanaged
		{
			int size = Unsafe.SizeOf<T>();
			byte* bytes = stackalloc byte[size];
			Unsafe.Write(bytes, t);
			Span<byte> bSpan = new Span<byte>(bytes, size);
			bw.Write(bSpan);
		}

		public static unsafe T ReadStruct<T>(this BinaryReader br) where T : unmanaged
		{
			int size = Unsafe.SizeOf<T>();
			byte* bytes = stackalloc byte[size];
			Span<byte> bSpan = new Span<byte>(bytes, size);
			br.Read(bSpan);
			return Unsafe.Read<T>(bytes);
		}

		public static void AddPadding(this BinaryWriter bw, uint padding)
		{
			for (int i = 0; i < padding; i++)
			{
				bw.Write((byte)0);
			}
		}
		public static void AddPadding(this BinaryWriter bw, int padding)
			=> AddPadding(bw, (uint) padding);

		public static void SkipPadding(this BinaryReader br, uint padding)
		{
			br.BaseStream.Seek(padding, SeekOrigin.Current);
		}

		public static void SkipPadding(this BinaryReader br, int padding)
			=> SkipPadding(br, (uint) padding);

		public static void WriteString(this BinaryWriter writer, string input, Encoding encoding = null) {
			encoding ??= Encoding.UTF8;

			var inputSpan = input.AsSpan();
			int inputByteCount = encoding.GetByteCount(input);
			Span<byte> inputBytes = stackalloc byte[inputByteCount];

			encoding.GetBytes(inputSpan, inputBytes);

			writer.Write(inputByteCount);
			writer.Write(inputBytes);
		}

		public static string ReadString(this BinaryReader reader, Encoding encoding = null) {
			encoding ??= Encoding.UTF8;

			int byteCount = reader.ReadInt32();
			Span<byte> bytes = stackalloc byte[byteCount];
			reader.Read(bytes);

			return encoding.GetString(bytes);
		}
	}
}