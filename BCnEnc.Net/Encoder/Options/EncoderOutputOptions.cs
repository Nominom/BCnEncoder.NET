using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Options
{
	public class EncoderOutputOptions
    {
        /// <summary>
        /// Whether to generate mipMaps. Default is true.
        /// </summary>
        public bool generateMipMaps = true;

        /// <summary>
        /// The maximum number of mipmap levels to generate. -1 or 0 is unbounded.
        /// Default is -1.
        /// </summary>
        public int maxMipMapLevel = -1;

        /// <summary>
        /// The compression format to use. Default is BC1.
        /// </summary>
        public CompressionFormat format = CompressionFormat.BC1;

        /// <summary>
        /// The quality of the compression. Use either fast or balanced for testing.
        /// Fast can be used for near real-time encoding for most algorithms.
        /// Use bestQuality when needed. Default is balanced.
        /// </summary>
        public CompressionQuality quality = CompressionQuality.Balanced;

        /// <summary>
        /// The output file format of the data. Either Ktx or Dds.
        /// Default is Ktx.
        /// </summary>
        public OutputFileFormat fileFormat = OutputFileFormat.Ktx;

        /// <summary>
        /// The DDS file format doesn't seem to have a standard for indicating whether a BC1 texture
        /// includes 1bit of alpha. This option will write DDPF_ALPHAPIXELS flag to the header
        /// to indicate the presence of an alpha channel. Some programs read and write this flag,
        /// but some programs don't like it and get confused. Your mileage may vary.
        /// Default is false.
        /// </summary>
        public bool ddsBc1WriteAlphaFlag = false;
    }
}
