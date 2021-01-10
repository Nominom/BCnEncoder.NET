namespace BCnEncoder.Decoder.Options
{
	public class DecoderInputOptions
    {
        /// <summary>
        /// The DDS file format doesn't seem to have a standard for indicating whether a BC1 texture
        /// includes 1bit of alpha. This option will assume that all Bc1 textures contain alpha.
        /// If this option is false, but the dds header includes a DDPF_ALPHAPIXELS flag, alpha will be included.
        /// Default is true.
        /// </summary>
        public bool ddsBc1ExpectAlpha = true;

        /// <summary>
        /// If true, when encoding to a format that only includes a red channel,
        /// use the pixel luminance instead of just the red channel. Default is false.
        /// </summary>
        public bool luminanceAsRed = false;
    }
}
