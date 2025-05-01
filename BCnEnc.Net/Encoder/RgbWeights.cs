using System;
using System.Numerics;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Encoder;

internal class RgbWeights
{
	public Vector4 PcaWeights = new Vector4(2f, 1f, 1f, 1f);

	public bool UseLinear { get; init; }


	/// <summary>
	/// Transform from perceptual space to PCA space
	/// </summary>
	public Vector4 TransformPca(Vector4 color) => UseLinear ? color : color * PcaWeights;

	/// <summary>
	/// Transform from PCA space to perceptual space
	/// </summary>
	public Vector4 InverseTransformPca(Vector4 color) => UseLinear ? color : color / PcaWeights;

	public Vector4 TransformToPerceptual(Vector4 color)
	{
		if (UseLinear) return color;

		// Convert to lRGB
		// if (InputSrgb) color = ColorSpace.Srgb.ToLrgb(color);

		return ColorSpace.Oklab.LrgbToOklab(color);
	}

	public Vector4 TransformFromPerceptual(Vector4 color)
	{
		if (UseLinear) return color;
		var lrgb = ColorSpace.Oklab.OklabToLrgb(color);

		// Convert back to sRGB
		// if (InputSrgb) lrgb = ColorSpace.Srgb.ToSrgb(lrgb);

		return lrgb;
	}

	/// <summary>
	/// Calculate color difference in perceptual space
	/// </summary>
	public float CalculateColorDiff(Vector4 color1, Vector4 color2)
	{
		if (UseLinear) return Vector4.Distance(color1, color2);

		return ColorSpace.Oklab.DeltaE(color1, color2);
	}

	public RgbWeights(bool useLinear)
	{
		UseLinear = useLinear;
	}
}
