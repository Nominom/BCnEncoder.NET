using System;
using System.Threading;
using BCnEncoder.Encoder;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Shared
{
	/// <summary>
	/// The operation context.
	/// </summary>
	internal class OperationContext
	{
		/// <summary>
		/// Whether the blocks should be decoded in parallel.
		/// </summary>
		public bool IsParallel { get; set; }

		/// <summary>
		/// Determines how many tasks should be used for parallel processing.
		/// </summary>
		public int TaskCount { get; set; } = Environment.ProcessorCount;

		/// <summary>
		/// The cancellation token to check if the asynchronous operation was cancelled.
		/// </summary>
		public CancellationToken CancellationToken { get; set; }

		/// <summary>
		/// The progress context for the operation.
		/// </summary>
		public OperationProgress Progress { get; set; }

		/// <summary>
		/// The compression quality to use for the operation.
		/// </summary>
		public CompressionQuality Quality { get; set; }

		/// <summary>
		/// The color conversion mode for the operation.
		/// </summary>
		public ColorConversionMode ColorConversionMode { get; set; }

		/// <summary>
		/// The Rgb weights to use for the operation, affects error calculation and PCA.
		/// </summary>
		public RgbWeights Weights { get; set; }

		/// <summary>
		/// The alpha cutoff to use when converting to 1bit alpha.
		/// </summary>
		public float AlphaThreshold { get; set; } = .5f;
	}
}
