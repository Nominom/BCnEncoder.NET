using BCnEncoder.Encoder;
using BCnEncoder.Shared;

namespace BCnEncoder.Decoder.Options
{
	/// <summary>
	/// Controls how the color space of the input data should be interpreted.
	/// </summary>
	public enum InputColorSpaceAssumption
	{
		/// <summary>
		/// [DEFAULT] Use the color space indicated by the format.
		/// For formats that explicitly support sRGB (like BC1_SRGB), those indicators will be respected.
		/// </summary>
		Auto,

		/// <summary>
		/// Assume the input data is in sRGB color space (with gamma curve), regardless of format indicators.
		/// Useful for older textures or those from sources that didn't properly tag their color space.
		/// </summary>
		ForceSrgb,

		/// <summary>
		/// Assume the input data is in linear color space (without gamma curve), regardless of format indicators.
		/// Useful for normal maps, physical property maps, or other technical textures that store linear data.
		/// </summary>
		ForceLinear
	}

	/// <summary>
	/// Controls the target color space for the decoded output.
	/// </summary>
	public enum OutputColorSpaceTarget
	{
		/// <summary>
		/// Colors are converted to linear colorspace for processing. Output will match the input colorspace regardless of chosen output format.
		/// </summary>
		ProcessLinearPreserveColorSpace,

		/// <summary>
		/// Keep the color space the same as the input (or assumed input) color space.
		/// No color space conversion will be performed.
		/// </summary>
		KeepAsIs,

		/// <summary>
		/// Convert to linear color space for the output.
		/// Use this when you need physically accurate color values for lighting calculations or HDR rendering.
		/// </summary>
		Linear,

		/// <summary>
		/// Convert to sRGB color space for the output.
		/// Use this when preparing images for display or when working with photo/UI editing software.
		/// </summary>
		Srgb,

		/// <summary>
		/// Automatically determine the best output color space based on the output format.
		/// The decoder will select linear for HDR formats and numeric data, and sRGB for display formats.
		/// </summary>
		Auto
	}

	/// <summary>
	/// Controls how transparency (alpha channel) is handled during decoding.
	/// </summary>
	public enum DecoderAlphaHandling
	{
		/// <summary>
		/// Decode alpha channel exactly as stored without modification.
		/// Best option for most cases when you want to preserve the original data.
		/// </summary>
		KeepAsIs,

		/// <summary>
		/// Convert from premultiplied alpha (where color channels are already multiplied by alpha) to straight alpha.
		/// Use this when preparing textures for storing as conventional image data or when working with photo/UI editing software.
		/// Most GPU texture formats (dds, ktx) are premultiplied alpha by default, while most standard image formats (png, etc..) are straight alpha.
		/// </summary>
		Unpremultiply
	}

	/// <summary>
	/// A class for the decoder output options.
	/// </summary>
	public class DecoderOutputOptions
	{
		/// <summary>
		/// The color channel to populate with the values of a BC4 block.
		/// </summary>
		public ColorComponent Bc4Component { get; set; } = ColorComponent.RGB;

		/// <summary>
		/// The color channel to populate with the values of the first BC5 block.
		/// </summary>
		public ColorComponent Bc5Component1 { get; set; } = ColorComponent.R;

		/// <summary>
		/// The color channel to populate with the values of the second BC5 block.
		/// </summary>
		public ColorComponent Bc5Component2 { get; set; } = ColorComponent.G;

		/// <summary>
		/// <para>The color channel to populate with the values calculated from the first and second BC5 blocks.</para>
		/// <para>z = Sqrt(1 - x^2 - y^2)</para>
		/// </summary>
		public ColorComponent Bc5ComponentCalculated { get; set; } = ColorComponent.None;

		/// <summary>
		/// Controls how the color space of the input data should be interpreted.
		/// </summary>
		public InputColorSpaceAssumption InputColorSpace { get; set; } = InputColorSpaceAssumption.Auto;

		/// <summary>
		/// Controls the target color space for the decoded output.
		/// </summary>
		public OutputColorSpaceTarget OutputColorSpace { get; set; } = OutputColorSpaceTarget.ProcessLinearPreserveColorSpace;

		/// <summary>
		/// Controls how transparency (alpha channel) is handled during decoding.
		/// </summary>
		public DecoderAlphaHandling AlphaHandling { get; set; } = DecoderAlphaHandling.KeepAsIs;

		/// <summary>
		/// Whether to rescale signed normalized values [-1, 1] to unsigned normalized values [0, 1], when applicable.
		/// </summary>
		public bool RescaleSnormToUnorm { get; set; } = true;
	}
}
