namespace BCnEncoder.Encoder
{
	public enum CompressionQuality
	{
		/// <summary>
		/// Fast, but low Quality. Especially bad with gradients.
		/// </summary>
		Fast,
		/// <summary>
		/// Strikes a balance between speed and Quality. Good enough for most purposes.
		/// </summary>
		Balanced,
		/// <summary>
		/// Aims for best Quality encoding. Can be very slow.
		/// </summary>
		BestQuality
	}
}
