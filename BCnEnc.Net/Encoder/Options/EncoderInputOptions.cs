namespace BCnEncoder.Encoder.Options
{
    /// <summary>
    /// The input options for the decoder.
    /// </summary>
	public class EncoderInputOptions
    {
        /// <summary>
        /// If true, when encoding to a Format that only includes a red channel,
        /// use the pixel luminance instead of just the red channel. Default is false.
        /// </summary>
        public bool LuminanceAsRed { get; set; } = false;
    }
}
