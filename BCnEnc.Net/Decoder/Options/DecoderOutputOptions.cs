namespace BCnEncoder.Decoder.Options
{
    /// <summary>
    /// A class for the decoder output options.
    /// </summary>
	public class DecoderOutputOptions
    {
        /// <summary>
        /// If true, when decoding from a Format that only includes a red channel,
        /// output pixels will have all colors set to the same value (greyscale). Default is true.
        /// </summary>
        public bool RedAsLuminance { get; set; } = true;
    }
}
