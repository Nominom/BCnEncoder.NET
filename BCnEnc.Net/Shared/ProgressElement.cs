namespace BCnEncoder.Shared
{
	public readonly struct ProgressElement
	{
		/// <summary>
		/// Current block being processed
		/// </summary>
		public long CurrentBlock { get; }

		/// <summary>
		/// The total amount of blocks to be processed
		/// </summary>
		public long TotalBlocks { get; }

		/// <summary>
		/// Returns the progress percentage as a float from 0 to 1
		/// </summary>
		public float Percentage => CurrentBlock / (float) TotalBlocks;

		public ProgressElement(long currentBlock, long totalBlocks)
		{
			CurrentBlock = currentBlock;
			TotalBlocks = totalBlocks;
		}

		public override string ToString()
		{
			return $"{nameof(CurrentBlock)}: {CurrentBlock}, {nameof(TotalBlocks)}: {TotalBlocks}, {nameof(Percentage)}: {Percentage}";
		}
	}
}
