using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncTests.Support
{
	/// <summary>
	/// Defines different texture types with their specific quality requirements.
	/// </summary>
	public enum TextureType
	{
		/// <summary>
		/// Standard color/albedo maps. More tolerant of compression artifacts.
		/// </summary>
		Albedo,

		/// <summary>
		/// Normal maps containing surface normal vectors. Very sensitive to compression.
		/// </summary>
		Normal,

		/// <summary>
		/// Height/displacement maps. Sensitive to precision loss.
		/// </summary>
		Height,

		/// <summary>
		/// Specular/metallic/roughness maps
		/// </summary>
		Specular,

		/// <summary>
		/// High dynamic range textures.
		/// </summary>
		Hdr
	}

	public class ImageQuality
	{
		public static float PeakSignalToNoiseRatio(ReadOnlySpan<ColorRgba32> original, ReadOnlySpan<ColorRgba32> other, bool countAlpha = true) {
			if (original.Length != other.Length) {
				throw new ArgumentException("Both spans should be the same length");
			}
			float error = 0;
			for (var i = 0; i < original.Length; i++)
			{
				var o = original[i].As<ColorYCbCr>();
				var c = other[i].As<ColorYCbCr>();
				error += (o.y - c.y) * (o.y - c.y);
				error += (o.cb - c.cb) * (o.cb - c.cb);
				error += (o.cr - c.cr) * (o.cr - c.cr);
				if (countAlpha) {
					error += (original[i].a - other[i].a) / 255.0f * ((original[i].a - other[i].a) / 255.0f);
				}

			}
			if (error < float.Epsilon) {
				return 100;
			}
			if (countAlpha) {
				error /= original.Length * 4;
			}
			else
			{
				error /= original.Length * 3;
			}

			return 20 * MathF.Log10(1 / MathF.Sqrt(error));
		}

		/// <summary>
		/// Calculates the Peak Signal-to-Noise Ratio between two images.
		/// </summary>
		/// <param name="original">The original image pixels.</param>
		/// <param name="other">The comparison image pixels.</param>
		/// <param name="channelMask">String indicating which channels to include ("rgba"). Case-insensitive.</param>
		/// <returns>PSNR value in decibels. Higher values indicate better quality.</returns>
		/// <remarks>
		/// This implementation assumes color values are normalized in the 0-1 range.
		/// PSNR approaches infinity as error approaches zero, so a maximum value of 100dB is returned for extremely small errors.
		/// </remarks>
		public static float PeakSignalToNoiseRatio(ReadOnlySpan<ColorRgba32> original, ReadOnlySpan<ColorRgba32> other, string channelMask) {
			if (original.Length != other.Length) {
				throw new ArgumentException("Both spans should be the same length");
			}

			if (string.IsNullOrEmpty(channelMask)) {
				throw new ArgumentException("Channel mask must contain at least one channel", nameof(channelMask));
			}

			channelMask = channelMask.ToLowerInvariant();

			int activeChannels = 0;
			bool useRed = channelMask.Contains('r');
			bool useGreen = channelMask.Contains('g');
			bool useBlue = channelMask.Contains('b');
			bool useAlpha = channelMask.Contains('a');

			if (useRed) activeChannels++;
			if (useGreen) activeChannels++;
			if (useBlue) activeChannels++;
			if (useAlpha) activeChannels++;

			if (activeChannels == 0) {
				throw new ArgumentException("Channel mask must contain at least one valid channel (r, g, b, or a)", nameof(channelMask));
			}

			float sumSquaredError = 0;
			for (var i = 0; i < original.Length; i++)
			{
				var o = original[i].ToColorRgbaFloat();
				var c = other[i].ToColorRgbaFloat();

				if (useRed)
					sumSquaredError += (o.r - c.r) * (o.r - c.r);
				if (useGreen)
					sumSquaredError += (o.g - c.g) * (o.g - c.g);
				if (useBlue)
					sumSquaredError += (o.b - c.b) * (o.b - c.b);
				if (useAlpha)
					sumSquaredError += (o.a - c.a) * (o.a - c.a);
			}

			float mse = sumSquaredError / (original.Length * activeChannels);

			if (mse < float.Epsilon) {
				return 100.0f;
			}

			// PSNR = 20 * log10(MAX / sqrt(MSE))
			return 20 * MathF.Log10(1.0f / MathF.Sqrt(mse));
		}

		public static float CalculateLogRMSE(ReadOnlySpan<ColorRgbaFloat> original, ReadOnlySpan<ColorRgbaFloat> other, bool countAlpha)
		{
			if (original.Length != other.Length)
			{
				throw new ArgumentException("Both spans should be the same length");
			}
			float error = 0;
			for (var i = 0; i < original.Length; i++)
			{
				var dr = Math.Sign(other[i].r) * MathF.Log(1 + MathF.Abs(other[i].r)) - Math.Sign(original[i].r) * MathF.Log(1 + MathF.Abs(original[i].r));
				var dg = Math.Sign(other[i].g) * MathF.Log(1 + MathF.Abs(other[i].g)) - Math.Sign(original[i].g) * MathF.Log(1 + MathF.Abs(original[i].g));
				var db = Math.Sign(other[i].b) * MathF.Log(1 + MathF.Abs(other[i].b)) - Math.Sign(original[i].b) * MathF.Log(1 + MathF.Abs(original[i].b));
				var da = Math.Sign(other[i].a) * MathF.Log(1 + MathF.Abs(other[i].a)) - Math.Sign(original[i].a) * MathF.Log(1 + MathF.Abs(original[i].a));

				error += dr * dr;
				error += dg * dg;
				error += db * db;

				if (countAlpha)
				{
					error += da * da;
				}

			}
			return countAlpha ?
				MathF.Sqrt(error / (4.0f * original.Length)):
				MathF.Sqrt(error / (3.0f * original.Length));
		}

		/// <summary>
		/// Asserts that the quality between two images meets the threshold for the specified texture type and compression quality.
		/// Automatically selects the appropriate metric based on texture type.
		/// </summary>
		/// <param name="original">The original image</param>
		/// <param name="compressed">The compressed/processed image to compare</param>
		/// <param name="textureType">Type of texture, which determines the quality metric and threshold</param>
		/// <param name="quality">Compression quality used, which determines the expected threshold</param>
		/// <param name="channelMask">Optional mask specifying which channels to include ("rgba"). Default is all channels.</param>
		/// <param name="output">Optional test output helper for writing results</param>
		public static void AssertImageQuality(
			SixLabors.ImageSharp.Image<RgbaVector> original,
			SixLabors.ImageSharp.Image<RgbaVector> compressed,
			TextureType textureType,
			BCnEncoder.Encoder.CompressionQuality quality,
			string channelMask = "rgba",
			Xunit.Abstractions.ITestOutputHelper output = null)
		{
			// Verify images have same dimensions
			if (original.Width != compressed.Width || original.Height != compressed.Height)
			{
				throw new System.ArgumentException("Both images must have the same dimensions");
			}

			// Select appropriate metric and threshold based on texture type
			switch (textureType)
			{
				case TextureType.Normal:
					AssertNormalMapQuality(original, compressed, quality, channelMask, output);
					break;

				case TextureType.Height:
					AssertHeightMapQuality(original, compressed, quality, channelMask, output);
					break;

				case TextureType.Hdr:
					AssertHdrQuality(original, compressed, quality, channelMask, output);
					break;

				case TextureType.Specular:
					AssertSpecularMapQuality(original, compressed, quality, channelMask, output);
					break;

				case TextureType.Albedo:
				default:
					AssertAlbedoQuality(original, compressed, quality, channelMask, output);
					break;
			}
		}

		// Individual quality assessment methods for each texture type

		private static void AssertAlbedoQuality(
			SixLabors.ImageSharp.Image<RgbaVector> original,
			SixLabors.ImageSharp.Image<RgbaVector> compressed,
			BCnEncoder.Encoder.CompressionQuality quality,
			string channelMask,
			Xunit.Abstractions.ITestOutputHelper output)
		{
			// For standard albedo maps, MS-SSIM is a good perceptual metric
			float msssim = StructuralSimilarity.MultiScaleStructuralSimilarity(original, compressed, channelMask);

			// Different thresholds based on compression quality
			float threshold = quality switch
			{
				BCnEncoder.Encoder.CompressionQuality.Fast => 0.82f,
				BCnEncoder.Encoder.CompressionQuality.Balanced => 0.88f,
				BCnEncoder.Encoder.CompressionQuality.BestQuality => 0.94f,
				_ => 0.88f
			};

			output?.WriteLine($"Albedo MS-SSIM: {msssim:F4}, threshold: {threshold:F4}, quality: {quality}");
			Xunit.Assert.True(msssim >= threshold,
				$"Image quality below threshold. MS-SSIM: {msssim:F4}, required: {threshold:F4}");
		}

		/// <summary>
		/// Calculates the L₂ norm (Euclidean distance) between normalized normal vectors.
		/// This is more appropriate for normal maps than standard RMSE as it considers the directional nature of normals.
		/// </summary>
		/// <param name="original">Original normal map data</param>
		/// <param name="compressed">Compressed normal map data</param>
		/// <param name="useRGB">Whether to use all three channels for the vector calculation</param>
		/// <returns>Average angular error between the normal vectors</returns>
		public static float CalculateNormalVectorDifference(ReadOnlySpan<ColorRgbaFloat> original, ReadOnlySpan<ColorRgbaFloat> compressed, bool useRGB = true)
		{
			if (original.Length != compressed.Length)
			{
				throw new ArgumentException("Both spans should be the same length");
			}

			float totalError = 0;
			for (var i = 0; i < original.Length; i++)
			{
				// Extract vector components - we need to transform from [0,1] to [-1,1] range
				// Normal maps typically store normals as: RGB = (Normal.x+1, Normal.y+1, Normal.z+1)/2
				float origX = original[i].r * 2 - 1;
				float origY = original[i].g * 2 - 1;
				float origZ = useRGB ? (original[i].b * 2 - 1) : 0;

				float compX = compressed[i].r * 2 - 1;
				float compY = compressed[i].g * 2 - 1;
				float compZ = useRGB ? (compressed[i].b * 2 - 1) : 0;

				// If Z isn't used, we need to recompute it (assuming unit length vector)
				if (!useRGB)
				{
					if (MathF.Abs(origX) <= 1.0f && MathF.Abs(origY) <= 1.0f)
					{
						origZ = MathF.Sqrt(MathF.Max(0, 1 - origX * origX - origY * origY));
					}
					else
					{
						origZ = 0; // Invalid normal
					}

					if (MathF.Abs(compX) <= 1.0f && MathF.Abs(compY) <= 1.0f)
					{
						compZ = MathF.Sqrt(MathF.Max(0, 1 - compX * compX - compY * compY));
					}
					else
					{
						compZ = 0; // Invalid normal
					}
				}

				// Normalize vectors (they should already be normalized, but compression can introduce errors)
				float origLength = MathF.Sqrt(origX * origX + origY * origY + origZ * origZ);
				float compLength = MathF.Sqrt(compX * compX + compY * compY + compZ * compZ);

				// Avoid division by zero
				if (origLength > float.Epsilon && compLength > float.Epsilon)
				{
					origX /= origLength;
					origY /= origLength;
					origZ /= origLength;

					compX /= compLength;
					compY /= compLength;
					compZ /= compLength;

					// Calculate squared L₂ norm (Euclidean distance) between the normalized vectors
					float dx = origX - compX;
					float dy = origY - compY;
					float dz = origZ - compZ;
					float vectorDiff = dx * dx + dy * dy + dz * dz;

					totalError += vectorDiff;
				}
				else
				{
					// If either vector is zero length, consider it a significant error
					totalError += 2.0f; // Max possible squared distance between unit vectors
				}
			}

			// Return average error (square root of mean squared error)
			// The theoretical maximum L₂ distance between two unit vectors is 2.0
			return MathF.Sqrt(totalError / original.Length);
		}

		private static void AssertNormalMapQuality(
			SixLabors.ImageSharp.Image<RgbaVector> original,
			SixLabors.ImageSharp.Image<RgbaVector> compressed,
			BCnEncoder.Encoder.CompressionQuality quality,
			string channelMask,
			Xunit.Abstractions.ITestOutputHelper output)
		{
			// For normal maps, vector difference is more appropriate than perceptual metrics
			// Convert to float arrays for processing
			var originalData = new ColorRgbaFloat[original.Width * original.Height];
			var compressedData = new ColorRgbaFloat[compressed.Width * compressed.Height];

			// Copy image data to arrays
			for (int y = 0; y < original.Height; y++)
			{
				for (int x = 0; x < original.Width; x++)
				{
					var pixel = original[x, y];
					originalData[y * original.Width + x] = new ColorRgbaFloat(pixel.R, pixel.G, pixel.B, pixel.A);

					pixel = compressed[x, y];
					compressedData[y * compressed.Width + x] = new ColorRgbaFloat(pixel.R, pixel.G, pixel.B, pixel.A);
				}
			}

			// Determine if we should use all three channels (RGB) based on the channel mask
			bool useRGB = channelMask.Contains("b", StringComparison.OrdinalIgnoreCase);

			// Calculate L₂ norm of difference between normalized vectors
			float vectorDiff = CalculateNormalVectorDifference(originalData, compressedData, useRGB);

			// For reference, also calculate standard RMSE
			bool useAlpha = channelMask.Contains("a", StringComparison.OrdinalIgnoreCase);
			float rmse = CalculateRMSE(originalData, compressedData, useAlpha);

			// Threshold for vector difference in normal maps
			// The theoretical maximum L₂ distance between unit vectors is 2.0 (opposite directions)
			float threshold = quality switch
			{
				BCnEncoder.Encoder.CompressionQuality.Fast => 0.15f,      // Max vector diff of 0.15 for fast
				BCnEncoder.Encoder.CompressionQuality.Balanced => 0.08f,   // Max vector diff of 0.08 for balanced
				BCnEncoder.Encoder.CompressionQuality.BestQuality => 0.05f, // Max vector diff of 0.05 for best quality
				_ => 0.08f
			};

			output?.WriteLine($"Normal map Vector Difference: {vectorDiff:F4}, threshold: {threshold:F4}");
			output?.WriteLine($"Normal map RMSE (for reference): {rmse:F4}, quality: {quality}");

			Xunit.Assert.True(vectorDiff <= threshold,
				$"Normal map quality below threshold. Vector Difference: {vectorDiff:F4}, max allowed: {threshold:F4}");
		}

		private static void AssertHeightMapQuality(
			SixLabors.ImageSharp.Image<RgbaVector> original,
			SixLabors.ImageSharp.Image<RgbaVector> compressed,
			BCnEncoder.Encoder.CompressionQuality quality,
			string channelMask,
			Xunit.Abstractions.ITestOutputHelper output)
		{
			// For height maps, combine RMSE and SSIM for both precision and structure
			// SSIM for structure preservation
			float ssim = StructuralSimilarity.SingleScaleStructuralSimilarity(original, compressed, channelMask);

			// RMSE for precision
			var originalData = new ColorRgbaFloat[original.Width * original.Height];
			var compressedData = new ColorRgbaFloat[compressed.Width * compressed.Height];

			for (int y = 0; y < original.Height; y++)
			{
				for (int x = 0; x < original.Width; x++)
				{
					var pixel = original[x, y];
					originalData[y * original.Width + x] = new ColorRgbaFloat(pixel.R, pixel.G, pixel.B, pixel.A);

					pixel = compressed[x, y];
					compressedData[y * compressed.Width + x] = new ColorRgbaFloat(pixel.R, pixel.G, pixel.B, pixel.A);
				}
			}

			bool useAlpha = channelMask.Contains("a", StringComparison.OrdinalIgnoreCase);
			float rmse = CalculateRMSE(originalData, compressedData, useAlpha);

			// Thresholds for height maps
			float ssimThreshold = quality switch
			{
				BCnEncoder.Encoder.CompressionQuality.Fast => 0.90f,
				BCnEncoder.Encoder.CompressionQuality.Balanced => 0.93f,
				BCnEncoder.Encoder.CompressionQuality.BestQuality => 0.96f,
				_ => 0.93f
			};

			float rmseThreshold = quality switch
			{
				BCnEncoder.Encoder.CompressionQuality.Fast => 0.04f,
				BCnEncoder.Encoder.CompressionQuality.Balanced => 0.02f,
				BCnEncoder.Encoder.CompressionQuality.BestQuality => 0.01f,
				_ => 0.025f
			};

			output?.WriteLine($"Height map SSIM: {ssim:F4}, threshold: {ssimThreshold:F4}");
			output?.WriteLine($"Height map RMSE: {rmse:F4}, threshold: {rmseThreshold:F4}, quality: {quality}");

			Xunit.Assert.True(ssim >= ssimThreshold,
				$"Height map structure quality below threshold. SSIM: {ssim:F4}, required: {ssimThreshold:F4}");
			Xunit.Assert.True(rmse <= rmseThreshold,
				$"Height map precision below threshold. RMSE: {rmse:F4}, max allowed: {rmseThreshold:F4}");
		}

		private static void AssertHdrQuality(
			SixLabors.ImageSharp.Image<RgbaVector> original,
			SixLabors.ImageSharp.Image<RgbaVector> compressed,
			BCnEncoder.Encoder.CompressionQuality quality,
			string channelMask,
			Xunit.Abstractions.ITestOutputHelper output)
		{
			// For HDR textures, log-based RMSE is most appropriate
			var originalData = new ColorRgbaFloat[original.Width * original.Height];
			var compressedData = new ColorRgbaFloat[compressed.Width * compressed.Height];

			for (int y = 0; y < original.Height; y++)
			{
				for (int x = 0; x < original.Width; x++)
				{
					var pixel = original[x, y];
					originalData[y * original.Width + x] = new ColorRgbaFloat(pixel.R, pixel.G, pixel.B, pixel.A);

					pixel = compressed[x, y];
					compressedData[y * compressed.Width + x] = new ColorRgbaFloat(pixel.R, pixel.G, pixel.B, pixel.A);
				}
			}

			bool useAlpha = channelMask.Contains("a", StringComparison.OrdinalIgnoreCase);
			float logRmse = CalculateLogRMSE(originalData, compressedData, useAlpha);

			// Thresholds for HDR content
			float threshold = quality switch
			{
				BCnEncoder.Encoder.CompressionQuality.Fast => 0.045f,
				BCnEncoder.Encoder.CompressionQuality.Balanced => 0.03f,
				BCnEncoder.Encoder.CompressionQuality.BestQuality => 0.02f,
				_ => 0.03f
			};

			output?.WriteLine($"HDR Log-RMSE: {logRmse:F4}, threshold: {threshold:F4}, quality: {quality}");
			Xunit.Assert.True(logRmse <= threshold,
				$"HDR quality below threshold. Log-RMSE: {logRmse:F4}, max allowed: {threshold:F4}");
		}

		private static void AssertSpecularMapQuality(
			SixLabors.ImageSharp.Image<RgbaVector> original,
			SixLabors.ImageSharp.Image<RgbaVector> compressed,
			BCnEncoder.Encoder.CompressionQuality quality,
			string channelMask,
			Xunit.Abstractions.ITestOutputHelper output)
		{
			// For specular/roughness/metallic maps, use MS-SSIM with moderate thresholds
			float msssim = StructuralSimilarity.MultiScaleStructuralSimilarity(original, compressed, channelMask);

			float threshold = quality switch
			{
				BCnEncoder.Encoder.CompressionQuality.Fast => 0.85f,
				BCnEncoder.Encoder.CompressionQuality.Balanced => 0.90f,
				BCnEncoder.Encoder.CompressionQuality.BestQuality => 0.95f,
				_ => 0.90f
			};

			output?.WriteLine($"Specular map MS-SSIM: {msssim:F4}, threshold: {threshold:F4}, quality: {quality}");
			Xunit.Assert.True(msssim >= threshold,
				$"Specular map quality below threshold. MS-SSIM: {msssim:F4}, required: {threshold:F4}");
		}

	}
}
