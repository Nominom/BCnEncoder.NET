namespace BCnEncoder.Encoder.Options
{
	public class EncoderInputOptions
    {
        /// <summary>
        /// If true, when encoding to a format that only includes a red channel,
        /// use the pixel luminance instead of just the red channel. Default is false.
        /// </summary>
        public bool luminanceAsRed = false;
    }
}
