using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace BCnEncoder.Shared.Vectorized;

internal static class MathV
{
	/// <summary>
	/// <para>Calculates the power of each element in <paramref name="value"/> with the corresponding element in <paramref name="exponent"/>.</para>
	/// <para>Value and exponent are expected to be positive and exponent should be non-zero.</para>
	/// </summary>
	/// <param name="value">The base values.</param>
	/// <param name="exponent">The exponents.</param>
	/// <returns>A vector containing the results of raising each element in <paramref name="value"/> to the power of the corresponding element in <paramref name="exponent"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector128<float> Pow(Vector128<float> value, Vector128<float> exponent)
	{
		// pow(x,y) = exp(y*log(x))
		Vector128<float> logX = Vector128.Log(value);
		Vector128<float> multYLogX = exponent * logX;
		Vector128<float> result = Vector128.Exp(multYLogX);

		return result;
	}

	/// <inheritdoc cref="Pow(Vector128{float},Vector128{float})"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector256<float> Pow(Vector256<float> value, Vector256<float> exponent)
	{
		// pow(x,y) = exp(y*log(x))
		Vector256<float> logX = Vector256.Log(value);
		Vector256<float> multYLogX = exponent * logX;
		Vector256<float> result = Vector256.Exp(multYLogX);

		return result;
	}
}
