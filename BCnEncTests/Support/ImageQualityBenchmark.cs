using System;
using System.IO;
using System.Linq;
using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.TextureFormats;
using BCnEncTests.Support;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests
{
    /// <summary>
    /// This test utility measures image quality between a reference image and a pre-encoded DDS image.
    /// </summary>
    public class ImageQualityBenchmark
    {
        private readonly ITestOutputHelper _output;

        public ImageQualityBenchmark(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Compares a reference image with a pre-encoded DDS image and prints out quality metrics.
        /// </summary>
        /// <param name="referenceImagePath">Path to the original reference image</param>
        /// <param name="encodedDdsPath">Path to the DDS file created with another encoding tool</param>
        /// <param name="textureType">The type of texture being compared</param>
        /// <param name="channelMask">Which channels to include in the comparison (default: "rgb")</param>
        /// <returns>A result object containing all the quality metrics</returns>
        public QualityResult BenchmarkQuality(
            string referenceImagePath,
            string encodedDdsPath,
            TextureType textureType,
            string channelMask = "rgb")
        {
            // Load reference image
            using var referenceImage = Image.Load<RgbaVector>(referenceImagePath);
            _output.WriteLine($"Loaded reference image: {Path.GetFileName(referenceImagePath)}");

            // Load encoded DDS image
            var decoder = new BcDecoder()
            {
	            OutputOptions = { Bc5ComponentCalculated = ColorComponent.B }
            };

            using var fs = new FileStream(encodedDdsPath, FileMode.Open, FileAccess.Read);

            DdsFile ddsFile = ITextureFileFormat<DdsFile>.Read(fs);
            ddsFile.GetBCnCompressionFormat(out CompressionFormat format, out _);

            using var ddsImage = decoder.Decode(ddsFile, CompressionFormat.RgbaFloat).AsImageRgbaVector();

            _output.WriteLine($"Loaded DDS image: {Path.GetFileName(encodedDdsPath)}");
            _output.WriteLine($"Format: {format}");

            // Ensure images have the same dimensions
            if (referenceImage.Width != ddsImage.Width || referenceImage.Height != ddsImage.Height)
            {
				throw new ArgumentException("Both images must have the same dimensions");
            }

            // Calculate metrics
            var result = new QualityResult();

            // Calculate RMSE
            result.Rmse = ImageQuality.CalculateRMSE(referenceImage, ddsImage, channelMask);
            _output.WriteLine($"RMSE: {result.Rmse:F6}");

            // Calculate LogRMSE (for HDR textures)
            result.LogRmse = ImageQuality.CalculateLogRMSE(referenceImage, ddsImage, channelMask);
            _output.WriteLine($"LogRMSE: {result.LogRmse:F6}");

            // Calculate StructuralSimilarity (Single-Scale SSIM and Multi-Scale SSIM)
            result.SsimResult = StructuralSimilarity.SingleScaleStructuralSimilarity(referenceImage, ddsImage, channelMask, false, false);
            result.MsSsimResult = StructuralSimilarity.MultiScaleStructuralSimilarity(referenceImage, ddsImage, channelMask, false, false);
            _output.WriteLine($"SSIM: {result.SsimResult}");
            _output.WriteLine($"MS-SSIM: {result.MsSsimResult}");

            // For normal maps, calculate normal vector difference
            if (textureType == TextureType.Normal)
            {
	            result.NormalVectorDiff = ImageQuality.CalculateNormalVectorDifference(referenceImage, ddsImage, channelMask.Contains("b"));
	            _output.WriteLine($"Normal Vector Difference: {result.NormalVectorDiff:F6}");
            }

            return result;
        }

        /// <summary>
        /// Run this method to batch test multiple images and formats
        /// </summary>
        [Fact]
        public void CompareDdsWithReferenceImages()
        {
            // Use the actual generated test images from the testImages/Quality directory
            string testImagesDir = Path.Combine(TestHelper.GetProjectRoot(),"testImages", "Quality");
            if (!Directory.Exists(testImagesDir))
            {
                _output.WriteLine("Test images directory not found. Please run QualityTester.ps1 first to generate test images.");
                return;
            }

            // Get all DDS files in the directory
            var ddsFiles = Directory.GetFiles(testImagesDir, "*.dds");
            _output.WriteLine($"Found {ddsFiles.Length} DDS files to test");

            if (ddsFiles.Length == 0)
            {
                _output.WriteLine("No DDS files found. Please run GenerateReferenceEncodes.ps1 first to generate test images.");
                return;
            }

            // Process each DDS file
            foreach (var ddsFile in ddsFiles)
            {
                // Parse the filename to extract information
                // Format is: basename_toolname_format.dds (e.g., albedo_directxtex_bc1.dds)
                string fileName = Path.GetFileNameWithoutExtension(ddsFile);
                string[] parts = fileName.Split('_');

                if (parts.Length < 3)
                {
                    _output.WriteLine($"Skipping file with unexpected format: {fileName}");
                    continue;
                }

                string baseName = parts[0];         // e.g., albedo, normal, specular
                string toolName = parts[1];         // e.g., directxtex, pvrtextool
                string formatName = parts[2];       // e.g., bc1, bc3, bc5, bc7

                var textureType = GetTextureTypeFromBaseName(baseName);

                // Find corresponding reference image
                string referenceFile = Path.Combine(testImagesDir, $"{baseName}_reference.png");
                if (!File.Exists(referenceFile))
                {
                    _output.WriteLine($"Reference file not found for {fileName}: {referenceFile}");
                    continue;
                }

                // Run the benchmark
                _output.WriteLine($"\nTesting {formatName.ToUpper()} compressed {baseName} image ({toolName})...");
                BenchmarkQuality(
                    referenceFile,
                    ddsFile,
                    textureType.Value,
                    "rgb"
                );
            }
        }

        /// <summary>
        /// Maps a base name to a TextureType
        /// </summary>
        private TextureType? GetTextureTypeFromBaseName(string baseName)
        {
            foreach (var texType in Enum.GetValues<TextureType>())
            {
	            if (baseName.Contains(texType.ToString(), StringComparison.OrdinalIgnoreCase))
		            return texType;
            }

            throw new ArgumentException($"Could not find matching texture type for: {baseName}");
        }
	}

    /// <summary>
    /// Container for all quality metrics to be returned from benchmarking
    /// </summary>
    public class QualityResult
    {
        public float Rmse { get; set; }
        public float LogRmse { get; set; }
        public float NormalVectorDiff { get; set; }
        public StructuralSimilarityResult SsimResult { get; set; }
        public StructuralSimilarityResult MsSsimResult { get; set; }
    }
}
