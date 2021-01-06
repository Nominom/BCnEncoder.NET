using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace BCnEncoder.Shared
{
	internal static class BinaryReaderWriterExtensions
	{
		public static unsafe void WriteStruct<T>(this BinaryWriter bw, T t) where T : unmanaged
		{
			int size = sizeof(T);
			byte* bytes = stackalloc byte[size];
			((T*) bytes)[0] = t;
			for (int b = 0; b < size; b++)
			{
				bw.Write(bytes[b]);
			}
		}

		public static unsafe T ReadStruct<T>(this BinaryReader br) where T : unmanaged
		{
			int size = sizeof(T);
			byte* bytes = stackalloc byte[size];
			for (int b = 0; b < size; b++)
			{
				bytes[b] = br.ReadByte();
			}
			return ((T*)bytes)[0];
		}

		public static void Read(this BinaryReader br, byte[] data)
		{
			for (int b = 0; b < data.Length; b++)
			{
				data[b] = br.ReadByte();
			}
		}

		public static byte[] Slice(this byte[] bytes, int start, int length)
		{
			byte[] output = new byte[length];
			for (int i = 0; i < length; i++)
			{
				output[i] = bytes[i + start];
			}
			return output;
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
	}
}