using System;
using System.Collections.Generic;
using System.Text;

namespace BCnEncoder.Shared
{
	internal static class ArrayExtensions
	{

		public static void Fill<T>(this T[] arr, T item)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				arr[i] = item;
			}
		}

		public static void Clear<T>(this T[] arr)
		{
			Fill(arr, default);
		}
	}
}
