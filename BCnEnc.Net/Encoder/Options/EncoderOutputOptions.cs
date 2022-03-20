using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Options
{
    /// <summary>
    /// The output options for the encoder.
    /// </summary>
	public class EncoderOutputOptions
    {
        /// <summary>
        /// Whether to generate mipMaps. Default is true.
        /// </summary>
        public bool GenerateMipMaps { get; set; } = true;

        /// <summary>
        /// The maximum number of mipmap levels to generate. -1 or 0 is unbounded.
        /// Default is -1.
        /// </summary>
        public int MaxMipMapLevel { get; set; } = -1;

        /// <summary>
        /// The compression Format to use. Default is Bc1.
        /// </summary>
        public CompressionFormat Format { get; set; } = CompressionFormat.Bc1;

        /// <summary>
        /// The Quality of the compression. Use either fast or balanced for testing.
        /// Fast can be used for near real-time encoding for most algorithms.
        /// Use bestQuality when needed. Default is balanced.
        /// </summary>
        public CompressionQuality Quality { get; set; } = CompressionQuality.Balanced;
    }
}
