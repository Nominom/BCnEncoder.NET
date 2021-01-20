using System;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Options
{
	/// <summary>
	/// General options for the encoder.
	/// </summary>
	public class EncoderOptions
	{
		/// <summary>
		/// Whether the blocks should be encoded in parallel. This can be much faster than single-threaded encoding,
		/// but is slow if multiple textures are being processed at the same time.
		/// When a debugger is attached, the encoder defaults to single-threaded operation to ease debugging.
		/// Default is true.
		/// </summary>
		public bool IsParallel { get; set; } = true;

		/// <summary>
		/// Determines how many tasks should be used for parallel processing.
		/// </summary>
		public int TaskCount { get; set; } = Environment.ProcessorCount;

		/// <summary>
		/// The progress context for the operation.
		/// </summary>
		public IProgress<ProgressElement> Progress { get; set; }
	}
}
