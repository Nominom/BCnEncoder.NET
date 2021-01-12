namespace BCnEncoder.Decoder.Options
{
    /// <summary>
    /// A class for the decoder input options.
    /// </summary>
	public class DecoderInputOptions
    {
        /// <summary>
        /// The DDS file Format doesn't seem to have a standard for indicating whether a BC1 texture
        /// includes 1bit of alpha. This option will assume that all Bc1 textures contain alpha.
        /// If this option is false, but the dds header includes a DDPF_ALPHAPIXELS flag, alpha will be included.
        /// Default is true.
        /// </summary>
        public bool DdsBc1ExpectAlpha { get; set; } = true;
    }
}
