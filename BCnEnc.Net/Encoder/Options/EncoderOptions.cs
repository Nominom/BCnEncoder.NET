namespace BCnEncoder.Encoder.Options
{
	public class EncoderOptions
    {
        /// <summary>
        /// Whether the blocks should be encoded in parallel. This can be much faster than single-threaded encoding,
        /// but is slow if multiple textures are being processed at the same time.
        /// When a debugger is attached, the encoder defaults to single-threaded operation to ease debugging.
        /// Default is true.
        /// </summary>
        public bool multiThreaded = true;
    }
}
