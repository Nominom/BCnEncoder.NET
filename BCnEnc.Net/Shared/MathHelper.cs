using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BCnEncoder.Shared
{
	public static unsafe class MathHelper
	{
		private const double two54 =  1.80143985094819840000e+16; /* 0x43500000, 0x00000000 */

		private const int MinExponent = -126;
		private const int MaxExponent = 127;

		// FrExp modified to C# from http://www.netlib.org
		/* @(#)fdlibm.h 1.5 04/04/22 */
		/*
		 * ====================================================
		 * Copyright (C) 2004 by Sun Microsystems, Inc. All rights reserved.
		 *
		 * Permission to use, copy, modify, and distribute this
		 * software is freely granted, provided that this notice 
		 * is preserved.
		 * ====================================================
		 */
		/// <summary>
		/// Breaks down the floating-point value x into a component m for the normalized fraction component and another term n for the exponent, such that the absolute value of m is greater than or equal to 0.5 and less than 1.0 or equal to 0, and x = m * 2n. The function stores the integer exponent n at the location to which expptr points. 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="eptr"></param>
		/// <returns>Returns the normalized fraction m. If x is 0, the function returns 0 for both the fraction and exponent. The fraction has the same sign as the argument x. The result of the function cannot have a range error.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double FrExp(double x, out int eptr)
		{
			unchecked
			{
				int hx, ix, lx;
				hx = *(1 + (int*) &x);
				ix = 0x7fffffff & hx;
				lx = *(int*) &x;
				eptr = 0;
				if (ix >= 0x7ff00000 || ((ix | lx) == 0)) return x; /* 0,inf,nan */
				if (ix < 0x00100000)
				{
					/* subnormal */
					x *= two54;
					hx = *(1 + (int*) &x);
					ix = hx & 0x7fffffff;
					eptr = -54;
				}

				eptr += (ix >> 20) - 1022;
				hx = (hx & (int) 0x800fffff) | (int) 0x3fe00000;
				*(1 + (int*) &x) = hx;
				return x;
			}
		}

		/// <summary>
		/// Multiplies a floating point value arg by the number 2 raised to the exp power.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="exp"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float LdExp(float arg, int exp)
		{
			if (exp >= MinExponent && exp <= MaxExponent)
			{
				return arg * Exp2(exp);
			}

			return arg * MathF.Pow(2, exp);
		}

		/// <summary>
		/// Builds 2^exp directly from its IEEE-754 exponent field. Only valid for normal exponents.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Exp2(int exp)
		{
			var bits = (exp + 127) << 23;
			return *(float*)&bits;
		}
	}
}
