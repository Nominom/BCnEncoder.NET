using System;

namespace BCnEncoder.Shared
{
	internal class OperationProgress
	{
		internal readonly IProgress<ProgressElement> progress;
		internal readonly long totalBlocks;
		internal long processedBlocks;

		public OperationProgress(IProgress<ProgressElement> progress, long totalBlocks)
		{
			this.progress = progress;
			this.totalBlocks = totalBlocks;
		}

		public void Report(long blocksSinceLastReport)
		{
			processedBlocks += blocksSinceLastReport;
			progress?.Report(new ProgressElement(processedBlocks, totalBlocks));
		}
	}
}
