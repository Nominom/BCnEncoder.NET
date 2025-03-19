using BCnEncoder.Encoder;
using BCnEncoder.Shared;

namespace BCnEncoder.Decoder.Options
{
	/// <summary>
	/// A class for the decoder output options.
	/// </summary>
	public class DecoderOutputOptions
	{
		/// <summary>
		/// The color channel to populate with the values of a BC4 block.
		/// </summary>
		public ColorComponent Bc4Component { get; set; } = ColorComponent.R;

		/// <summary>
		/// The color channel to populate with the values of the first BC5 block.
		/// </summary>
		public ColorComponent Bc5Component1 { get; set; } = ColorComponent.R;

		/// <summary>
		/// The color channel to populate with the values of the second BC5 block.
		/// </summary>
		public ColorComponent Bc5Component2 { get; set; } = ColorComponent.G;

		/// <summary>
		/// <para>The color channel to populate with the values calculated from the first and second BC5 blocks.</para>
		/// <para>z = Sqrt(1 - x^2 - y^2)</para>
		/// </summary>
		public ColorComponent Bc5ComponentCalculated { get; set; } = ColorComponent.None;

		/// <summary>
		/// Whether to do automatic colorspace conversion when the source colorspace does not match the target colorspace.
		/// </summary>
		public bool DoColorspaceConversion { get; set; } = true;
	}
}
