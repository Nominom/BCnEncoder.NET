using System;
using System.Collections.Generic;
using System.Text;

namespace BCnEncoder.Shared
{
	public static unsafe class MathHelper
	{
		private static  double two54 =  1.80143985094819840000e+16; /* 0x43500000, 0x00000000 */

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
		/// <param name="exponent"></param>
		/// <returns>Returns the normalized fraction m. If x is 0, the function returns 0 for both the fraction and exponent. The fraction has the same sign as the argument x. The result of the function cannot have a range error.</returns>
		public static double FrExp(double x, out int exponent)
		{
			unchecked
			{
				int hx, ix, lx;
				hx = *(1 + (int*) &x);
				ix = 0x7fffffff & hx;
				lx = *(int*) &x;
				exponent = 0;
				if (ix >= 0x7ff00000 || ((ix | lx) == 0)) return x; /* 0,inf,nan */
				if (ix < 0x00100000)
				{
					/* subnormal */
					x *= two54;
					hx = *(1 + (int*) &x);
					ix = hx & 0x7fffffff;
					exponent = -54;
				}

				exponent += (ix >> 20) - 1022;
				hx = (hx & (int) 0x800fffff) | (int) 0x3fe00000;
				*(1 + (int*) &x) = hx;
				return x;
			}
		}
		
		/// <summary>
		/// Breaks down the floating-point value x into a component m for the normalized fraction component and another term n for the exponent, such that the absolute value of m is greater than or equal to 0.5 and less than 1.0 or equal to 0, and x = m * 2n. The function stores the integer exponent n at the location to which exponent points. 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="exponent"></param>
		/// <returns>Returns the normalized fraction m. If x is 0, the function returns 0 for both the fraction and exponent. The fraction has the same sign as the argument x. The result of the function cannot have a range error.</returns>
		public static float FrExpF(float x, out int exponent)
		{
			uint fl;

			*((float*)&fl) = x;
			/* Find the exponent (power of 2) */
			long exp = (fl >> 23) & 0x000000ff;
			exp -= 0x7e;
			exponent = (int)exp;
			fl &= 0x807fffff; /* strip all exponent bits */
			fl |= 0x3f000000; /* mantissa between 0.5 and 1 */
			return *((float*)&fl);
		}


		/// <summary>
		/// Multiplies a floating point value arg by the number 2 raised to the exp power.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="exp"></param>
		/// <returns></returns>
		public static float LdExpF(float arg, int exp)
		{
			return arg * MathF.Pow(2, exp);
		}

		/// <summary>
		/// Multiplies a floating point value arg by the number 2 raised to the exp power.
		/// </summary>
		/// <param name="arg"></param>
		/// <param name="exp"></param>
		/// <returns></returns>
		public static double LdExp(double arg, int exp)
		{
			return arg * Math.Pow(2, exp);
		}
	}
}
