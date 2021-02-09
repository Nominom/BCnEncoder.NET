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
		/// If true, when decoding from a Format that only includes a red channel,
		/// output pixels will have all colors set to the same value (greyscale). Default is true.
		/// </summary>
		public bool RedAsLuminance { get; set; } = true;

		/// <summary>
		/// The color channel to populate with the values of a BC4 block.
		/// </summary>
		public Bc4Component Bc4Component { get; } = Bc4Component.R;

		/// <summary>
		/// The color channel to populate with the values of the first BC5 block.
		/// </summary>
		public Bc4Component Bc5Component1 { get; } = Bc4Component.R;

		/// <summary>
		/// The color channel to populate with the values of the second BC5 block.
		/// </summary>
		public Bc4Component Bc5Component2 { get; } = Bc4Component.G;
	}
}
