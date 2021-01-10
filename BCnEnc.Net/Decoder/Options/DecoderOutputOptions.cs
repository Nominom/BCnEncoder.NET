namespace BCnEncoder.Decoder.Options
{
	public class DecoderOutputOptions
    {
        /// <summary>
        /// If true, when decoding from a format that only includes a red channel,
        /// output pixels will have all colors set to the same value (greyscale). Default is true.
        /// </summary>
        public bool redAsLuminance = true;
    }
}
