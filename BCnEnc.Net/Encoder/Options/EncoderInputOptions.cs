using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Options
{
    /// <summary>
    /// The input options for the decoder.
    /// </summary>
	public class EncoderInputOptions
    {
	    /// <summary>
	    /// If true, when encoding to R8 raw format,
	    /// use the pixel luminance instead of just the red channel. Default is false. (Does not apply to BC4 format)
	    /// </summary>
	    public bool LuminanceAsRed { get; set; } = false;

	    /// <summary>
	    /// The color channel to take for the values of a BC4 block.
	    /// </summary>
		public Bc4Component Bc4Component { get; set; } = Bc4Component.R;

		/// <summary>
		/// The color channel to take for the values of the first BC5 block.
		/// </summary>
		public Bc4Component Bc5Component1 { get; set; } = Bc4Component.R;

		/// <summary>
		/// The color channel to take for the values of the second BC5 block.
		/// </summary>
		public Bc4Component Bc5Component2 { get; set; } = Bc4Component.G;
	}
}
