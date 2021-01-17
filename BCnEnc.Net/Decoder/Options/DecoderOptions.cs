using System;

namespace BCnEncoder.Decoder.Options
{
	/// <summary>
	/// General options for the decoder.
	/// </summary>
	public class DecoderOptions
	{
		/// <summary>
		/// Whether the blocks should be decoded in parallel. This can be much faster than single-threaded decoding,
		/// but is slow if multiple textures are being processed at the same time.
		/// When a debugger is attached, the decoder defaults to single-threaded operation to ease debugging.
		/// Default is false.
		/// </summary>
		/// <remarks>Parallel execution will be ignored in RawDecoders, due to minimal performance gain.</remarks>
		public bool IsParallel { get; set; }

		/// <summary>
		/// Determines how many tasks should be used for parallel processing.
		/// </summary>
		public int TaskCount { get; set; } = Environment.ProcessorCount;
	}
}
