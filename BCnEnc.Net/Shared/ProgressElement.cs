namespace BCnEncoder.Shared
{
	public struct ProgressElement
	{
		public int CurrentBlock { get; }

		public int TotalBlockses { get; }

		public ProgressElement(int currentBlock, int totalBlocks)
		{
			CurrentBlock = currentBlock;
			TotalBlockses = totalBlocks;
		}
	}
}
