using System;
using System.IO;
using System.Numerics;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using BCnEncTests.Support;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests.Support
{
    public class StructuralSimilarityTests
    {
        private readonly ITestOutputHelper _output;

        public StructuralSimilarityTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void MultiScaleStructuralSimilarity_IdenticalImages_ReturnsOne()
        {
            // Arrange
            using var image = new Image<RgbaVector>(100, 100);

            // Fill with random data to ensure we're not just testing empty images
            var random = new Random(42); // Fixed seed for reproducibility
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    image[x, y] = new RgbaVector(
                        (float)random.NextDouble(),
                        (float)random.NextDouble(),
                        (float)random.NextDouble(),
                        1.0f);
                }
            }

            using var clone = image.Clone();
            string channelMask = "rgb"; // Test only RGB channels

            // Act
            StructuralSimilarityResult similarity = StructuralSimilarity.MultiScaleStructuralSimilarity(image, clone, channelMask);

            // Assert
            _output.WriteLine($"MS-SSIM between identical images: {similarity}");
            Assert.Equal(1.0f, similarity.Average, 4); // Should be exactly 1.0 with tolerance of 10^-4
        }

        [Fact]
        public void MultiScaleStructuralSimilarity_SimilarImages_ReturnsHighValue()
        {
            // Arrange
            using var original = new Image<RgbaVector>(100, 100);

            // Fill with random data
            var random = new Random(42);
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    original[x, y] = new RgbaVector(
                        (float)random.NextDouble(),
                        (float)random.NextDouble(),
                        (float)random.NextDouble(),
                        (float)random.NextDouble());
                }
            }

            // Create slightly modified image (add small amount of noise)
            using var modified = original.Clone();
            modified.Mutate(ctx => ctx.ProcessPixelRowsAsVector4((row, point) =>
            {
                for (int x = 0; x < row.Length; x++)
                {
                    row[x] = new Vector4(
                        row[x].X + (float)(random.NextDouble() * 0.05), // Small random noise
                        row[x].Y + (float)(random.NextDouble() * 0.05),
                        row[x].Z + (float)(random.NextDouble() * 0.05),
                        row[x].W);
                }
            }));

            string channelMask = "rgba";

            // Act
            StructuralSimilarityResult similarity = StructuralSimilarity.MultiScaleStructuralSimilarity(original, modified, channelMask);

            // Assert
            _output.WriteLine($"MS-SSIM between similar images: {similarity}");
            Assert.True(similarity.Average > 0.9f, $"MS-SSIM value {similarity} is lower than expected for similar images");
            Assert.True(similarity.Average < 1.0f, $"MS-SSIM value {similarity} should be less than 1.0 for non-identical images");
        }

        [Fact]
        public void MultiScaleStructuralSimilarity_DifferentImages_ReturnsLowerValue()
        {
            // Arrange
            using var image1 = new Image<RgbaVector>(100, 100);
            using var image2 = new Image<RgbaVector>(100, 100);

            // Fill with completely different patterns
            for (int y = 0; y < image1.Height; y++)
            {
                for (int x = 0; x < image1.Width; x++)
                {
                    // Image 1: Gradient pattern
                    float normalizedX = x / (float)image1.Width;
                    float normalizedY = y / (float)image1.Height;
                    image1[x, y] = new RgbaVector(normalizedX, normalizedY, 0.5f, 1.0f);

                    // Image 2: Checkerboard pattern
                    bool isEvenX = (x / 10) % 2 == 0;
                    bool isEvenY = (y / 10) % 2 == 0;
                    float value = (isEvenX == isEvenY) ? 1.0f : 0.0f;
                    image2[x, y] = new RgbaVector(value, value, value, 1.0f);
                }
            }

            string channelMask = "rgba";

            // Act
            StructuralSimilarityResult similarity = StructuralSimilarity.MultiScaleStructuralSimilarity(image1, image2, channelMask);

            // Assert
            _output.WriteLine($"MS-SSIM between different images: {similarity}");
            Assert.True(similarity.Average < 0.8f, $"MS-SSIM value {similarity} is higher than expected for different images");
        }

        [Fact]
        public void MultiScaleStructuralSimilarity_WithDifferentScales_ProducesConsistentResults()
        {
            // Arrange
            using var original = new Image<RgbaVector>(128, 128); // Power of 2 size for clean downscaling
            using var modified = new Image<RgbaVector>(128, 128);

            // Create patterns with some similarity but clear differences
            var random = new Random(42);
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    // Original has a base pattern
                    float normalizedX = x / (float)original.Width;
                    float normalizedY = y / (float)original.Height;
                    original[x, y] = new RgbaVector(normalizedX, normalizedY, 0.5f, 1.0f);

                    // Modified adds noise to that pattern
                    modified[x, y] = new RgbaVector(
                        normalizedX + (float)(random.NextDouble() * 0.1 - 0.05),
                        normalizedY + (float)(random.NextDouble() * 0.1 - 0.05),
                        0.5f + (float)(random.NextDouble() * 0.1 - 0.05),
                        1.0f);
                }
            }

            string channelMask = "rgba";

            // Act - Test with different scale values
            StructuralSimilarityResult similarity3 = StructuralSimilarity.MultiScaleStructuralSimilarity(original, modified, channelMask, scales: 3);
            StructuralSimilarityResult similarity5 = StructuralSimilarity.MultiScaleStructuralSimilarity(original, modified, channelMask, scales: 5);

            // Assert
            _output.WriteLine($"MS-SSIM with 3 scales: {similarity3}");
            _output.WriteLine($"MS-SSIM with 5 scales: {similarity5}");

            // Results should be stable but not identical with different scale counts
            Assert.True(Math.Abs(similarity3.Average - similarity5.Average) < 0.1f,
                $"MS-SSIM values with different scales differ too much: {similarity3} vs {similarity5}");
        }

        [Fact]
        public void MultiScaleStructuralSimilarity_ChannelMaskVariations_WorkCorrectly()
        {
            // Arrange
            using var original = new Image<RgbaVector>(100, 100);

            // Create image with R=1, G=0.5, B=0 for all pixels
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    original[x, y] = new RgbaVector(1.0f, 0.5f, 0.0f, 1.0f);
                }
            }

            // Create modified image with only the green channel changed
            using var greenModified = original.Clone();
            greenModified.Mutate(ctx => ctx.ProcessPixelRowsAsVector4((row, point) =>
            {
                for (int x = 0; x < row.Length; x++)
                {
                    row[x] = new Vector4(row[x].X, 1.0f, row[x].Z, row[x].W); // Change only green to 1.0
                }
            }));

            // Act & Assert
            StructuralSimilarityResult similarityRgb = StructuralSimilarity.MultiScaleStructuralSimilarity(original, greenModified, "rgb");
            StructuralSimilarityResult similarityRb = StructuralSimilarity.MultiScaleStructuralSimilarity(original, greenModified, "rb");
            StructuralSimilarityResult similarityG = StructuralSimilarity.MultiScaleStructuralSimilarity(original, greenModified, "g");

            _output.WriteLine($"MS-SSIM RGB channels: {similarityRgb}");
            _output.WriteLine($"MS-SSIM RB channels (unchanged): {similarityRb}");
            _output.WriteLine($"MS-SSIM G channel (changed): {similarityG}");

            // RB should be close to 1.0 as those channels are unchanged
            Assert.True(similarityRb.Average > 0.99f, $"MS-SSIM for unchanged channels should be close to 1.0, got {similarityRb}");

            // G should be lower as that channel was modified
            Assert.True(similarityG.Average < 0.95f, $"MS-SSIM for changed channel should be lower, got {similarityG}");

            // RGB combined should be between the two
            Assert.True(similarityRgb.Average < similarityRb.Average, $"MS-SSIM for RGB should be lower than RB, got {similarityRgb} vs {similarityRb}");
            Assert.True(similarityRgb.Average > similarityG.Average, $"MS-SSIM for RGB should be higher than G, got {similarityRgb} vs {similarityG}");
        }
    }
}
