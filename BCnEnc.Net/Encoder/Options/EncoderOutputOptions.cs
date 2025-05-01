using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Options
{
    /// <summary>
    /// The output options for the encoder.
    /// </summary>
	public class EncoderOutputOptions
    {
        /// <summary>
        /// Whether to generate mipMaps. Default is true.
        /// </summary>
        public bool GenerateMipMaps { get; set; } = true;

        /// <summary>
        /// The maximum number of mipmap levels to generate. -1 or 0 is unbounded.
        /// Default is -1.
        /// </summary>
        public int MaxMipMapLevel { get; set; } = -1;

        /// <summary>
        /// The compression Format to use. Default is Bc1.
        /// </summary>
        public CompressionFormat Format { get; set; } = CompressionFormat.Bc1;

        /// <summary>
        /// The Quality of the compression. Use either fast or balanced for testing.
        /// Fast can be used for near real-time encoding for most algorithms.
        /// Use bestQuality if time is not an issue. Default is balanced.
        /// </summary>
        public CompressionQuality Quality { get; set; } = CompressionQuality.Balanced;

        /// <summary>
        /// The colorspace to use for encoding and mipmap generation.
        /// </summary>
        public EncoderColorSpaceHandling ColorSpaceHandling { get; set; } = EncoderColorSpaceHandling.ProcessLinearPreserveColorSpace;

        /// <summary>
        /// How to handle alpha channel during processing.
        /// </summary>
        public EncoderAlphaHandling AlphaHandling { get; set; } = EncoderAlphaHandling.Auto;

        /// <summary>
        /// <para>
        /// Whether to rescale unsigned normalized values [0, 1] to signed normalized values [-1, 1], when applicable.
        /// </para>
        /// <para>
        /// Default is true.
        /// </para>
        /// </summary>
        public bool RescaleUnormToSnorm { get; set; } = true;

        /// <summary>
        /// <para>
        /// Whether to use perceptual metrics for encoding.
        /// This makes the encoder calculate the error based on human perception of luma, instead of absolute color difference.
        /// This option has no effect on R and RG formats.
        /// </para>
        /// <para>
        /// Default is true.
        /// </para>
        /// </summary>
        public bool UsePerceptualMetrics { get; set; } = true;
    }

    /// <summary>
    /// Defines how colors should be interpreted and processed during encoding.
    /// </summary>
    public enum EncoderColorSpaceHandling
    {
        /// <summary>
        /// Colors are converted to linear colorspace for processing. Output will match the input colorspace regardless of chosen output format.
        /// </summary>
        ProcessLinearPreserveColorSpace,

        /// <summary>
        /// Do not convert colors to linear colorspace for processing. Output will match the input colorspace.
        /// </summary>
        KeepAsIs,

        /// <summary>
        /// Detect appropriate colorspace based on input/output formats (e.g., use sRGB for sRGB formats, linear for HDR).
        /// The RGB colorspace will be automatically determined based on the selected input and output formats.
        /// Conversion will be automatically applied if necessary.
        /// </summary>
        Auto
    }

    /// <summary>
    /// Defines how the alpha channel should be handled during processing.
    /// </summary>
    public enum EncoderAlphaHandling
    {
        /// <summary>
        /// Automatically detect if the source data is already using premultiplied alpha by checking
        /// if any color channel value is higher than its corresponding alpha value.
        /// If not premultiplied, convert to premultiplied for processing.
        /// </summary>
        Auto,

        /// <summary>
        /// Use the source alpha representation as-is without any conversion.
        /// </summary>
        AsIs,

        /// <summary>
        /// Expect input data in linear format with straight alpha and convert to premultiplied
        /// for processing and output.
        /// </summary>
        LinearToPremultiplied
    }
}
