using System;
using System.Collections.Generic;
using System.Text;

namespace BCnEncoder.Shared
{
	internal static class InternalUtils
	{
		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			var temp = lhs;
			lhs = rhs;
			rhs = temp;
		}
	}
}
