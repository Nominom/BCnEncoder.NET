namespace BCnEncoder.Encoder
{
	public enum EncodingQuality
	{
		/// <summary>
		/// Fast, but low quality. Especially bad with gradients.
		/// </summary>
		Fast,
		/// <summary>
		/// Strikes a balance between speed and quality. Enough for most purposes.
		/// </summary>
		Balanced,
		/// <summary>
		/// Aims for best quality encoding. Can be very slow at times.
		/// </summary>
		BestQuality
	}
}
