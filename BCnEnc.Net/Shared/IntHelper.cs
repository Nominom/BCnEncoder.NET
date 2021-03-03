using System;
using System.Collections.Generic;
using System.Text;

namespace BCnEncoder.Shared
{
	internal static class IntHelper
	{

		public static int SignExtend(int orig, int precision)
		{
			var signMask = (1 << (precision - 1));
			var numberMask = signMask - 1;

			if ((orig & signMask) != 0)
			{
				return (~numberMask) | (orig & numberMask);
			}

			return (orig & numberMask);
		}
	}
}
