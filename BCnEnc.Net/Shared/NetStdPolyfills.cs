
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance;


namespace BCnEncoder.Shared
{
#if NETSTANDARD2_0
	internal static class MemoryPolyfills
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Memory2D<T> AsMemory2D<T>(this Memory<T> memory, int height, int width)
		{
			if (MemoryMarshal.TryGetArray(memory, out ArraySegment<T> segment))
			{
				T[] array = segment.Array;
				ref T value = ref array.DangerousGetReference();
				return Memory2D<T>.DangerousCreate(array, ref value, height, width, 0);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlyMemory2D<T> AsMemory2D<T>(this ReadOnlyMemory<T> memory, int height, int width)
		{
			if (MemoryMarshal.TryGetArray(memory, out ArraySegment<T> segment))
			{
				T[] array = segment.Array;
				ref T value = ref array.DangerousGetReference();
				return ReadOnlyMemory2D<T>.DangerousCreate(array, ref value, height, width, 0);
			}
			else
			{
				throw new NotSupportedException();
			}
		}
	}

	internal static class SpanPolyfills
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe ReadOnlySpan2D<T> AsSpan2D<T>(this ReadOnlySpan<T> span, int height, int width)
		{
			ref T value = ref span.DangerousGetReference();
			void* pointer = Unsafe.AsPointer(ref value);
			return new ReadOnlySpan2D<T>(pointer, height, width, 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe Span<T> GetRowSpan<T>(this Span2D<T> span, int row)
		{
			ref T value = ref span.DangerousGetReferenceAt(row, 0);
			void* pointer = Unsafe.AsPointer(ref value);
			return new Span<T>(pointer, span.Width);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe ReadOnlySpan<T> GetRowSpan<T>(this ReadOnlySpan2D<T> span, int row)
		{
			ref T value = ref span.DangerousGetReferenceAt(row, 0);
			void* pointer = Unsafe.AsPointer(ref value);
			return new Span<T>(pointer, span.Width);
		}
	}

	internal static class BinaryWriterPolyfills
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(this BinaryWriter writer, ReadOnlySpan<byte> buffer)
		{
			writer.BaseStream.Write(buffer);
		}
	}

	internal static class BinaryReaderPolyfills
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Read(this BinaryReader reader, Span<byte> buffer)
		{
			return reader.BaseStream.Read(buffer);
		}
	}

	internal static class EncodingPolyfills
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> buffer)
		{
			ref byte value = ref buffer.DangerousGetReference();
			byte* pointer = (byte*)Unsafe.AsPointer(ref value);
			return encoding.GetString(pointer, buffer.Length);
		}
	}

	internal static class MemoryMarshalPolyfills
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe Span<T> CreateSpan<T>(ref T reference, int length)
		{
			return new Span<T>(Unsafe.AsPointer(ref reference), length);
		}
	}

	internal static class ArrayPolyfills
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Fill<T>(T[] array, T value)
		{
			array.AsSpan().Fill(value);
		}
	}

	internal static class MathPolyfills
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamp(float value, float min, float max)
		{
			if (min > max)
			{
				throw new ArgumentException();
			}

			if (value < min)
			{
				return min;
			}
			else if (value > max)
			{
				return max;
			}

			return value;
		}
	}
#endif

	internal static class MathCbrt
	{
		public static float Cbrt(float f)
		{
			#if NETSTANDARD2_0
			return MathF.Pow(f, 1 / 3.0f);
			#else
			return MathF.Cbrt(f);
			#endif
		}
	}
}
