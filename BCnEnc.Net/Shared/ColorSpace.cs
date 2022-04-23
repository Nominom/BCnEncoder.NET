using System;
using System.Collections.Generic;
using System.Text;

namespace BCnEncoder.Shared
{
	public enum ColorSpace
	{
		/// <summary>
		/// Standard Rgb (IEC 61966-2-1) (gamma 2.2)
		/// <para>
		/// Most input data is in this color space.
		/// </para>
		/// </summary>
		sRGB = 0,
		/// <summary>
		/// Linear sRGB color space. All floats
		/// <para>
		/// Image operations should be done in this color space.
		/// </para>
		/// </summary>
		LinearRgb = 1
	}
}
