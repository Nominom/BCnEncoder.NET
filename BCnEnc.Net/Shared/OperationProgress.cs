using System;

namespace BCnEncoder.Shared
{
	public class OperationProgress
	{
		private readonly IProgress<ProgressElement> progress;
		private readonly int totalBlocks;
		private int processedBlocks;

		public OperationProgress(IProgress<ProgressElement> progress, int totalBlocks)
		{
			this.progress = progress;
			this.totalBlocks = totalBlocks;
		}

		public void SetProcessedBlocks(int processedBlocks)
		{
			this.processedBlocks = processedBlocks;
		}

		public void Report(int currentBlock)
		{
			progress?.Report(new ProgressElement(processedBlocks + currentBlock, totalBlocks));
		}
	}
}
