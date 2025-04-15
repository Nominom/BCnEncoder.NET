using System;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

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
		/// <summary>
		/// Calculates the Root Mean Square Error between two images.
		/// </summary>
		/// <param name="original">Original image</param>
		/// <param name="compressed">Processed/compressed image to compare</param>
		/// <param name="channelMask">String indicating which channels to include in calculation ("rgba")</param>
		/// <returns>RMSE value (lower is better)</returns>
		public static float CalculateRMSE(Image<RgbaVector> original, Image<RgbaVector> compressed, string channelMask = "rgb")
		{
			if (original.Width != compressed.Width || original.Height != compressed.Height)
			{
				throw new ArgumentException("Both images must have the same dimensions");
			}

			int length = original.Width * original.Height;

			bool useRed = channelMask.Contains("r", StringComparison.OrdinalIgnoreCase);
			bool useGreen = channelMask.Contains("g", StringComparison.OrdinalIgnoreCase);
			bool useBlue = channelMask.Contains("b", StringComparison.OrdinalIgnoreCase);
			bool useAlpha = channelMask.Contains("a", StringComparison.OrdinalIgnoreCase);

			int activeChannels = 0;
			if (useRed) activeChannels++;
			if (useGreen) activeChannels++;
			if (useBlue) activeChannels++;
			if (useAlpha) activeChannels++;

			if (activeChannels == 0)
			{
				throw new ArgumentException("Channel mask must contain at least one valid channel (r, g, b, or a)", nameof(channelMask));
			}

			float error = 0;
			for (int y = 0; y < original.Height; y++)
			{
				for (int x = 0; x < original.Width; x++)
				{
					var pixel = original[x, y];
					var otherPixel = compressed[x, y];

					if (useRed)
					{
						var dr = otherPixel.R - pixel.R;
						error += dr * dr;
					}
					if (useGreen)
					{
						var dg = otherPixel.G - pixel.G;
						error += dg * dg;
					}
					if (useBlue)
					{
						var db = otherPixel.B - pixel.B;
						error += db * db;
					}
					if (useAlpha)
					{
						var da = otherPixel.A - pixel.A;
						error += da * da;
					}
				}
			}

			return MathF.Sqrt(error / (activeChannels * length));
		}

		/// <summary>
		/// Calculates the logarithmic Root Mean Square Error between two images.
		/// This is especially useful for HDR content as it accounts for the perceptual differences at different luminance levels.
		/// </summary>
		/// <param name="original">Original image</param>
		/// <param name="compressed">Processed/compressed image to compare</param>
		/// <param name="channelMask">String indicating which channels to include in calculation ("rgba")</param>
		/// <returns>Log-RMSE value (lower is better)</returns>
		public static float CalculateLogRMSE(Image<RgbaVector> original, Image<RgbaVector> compressed, string channelMask = "rgb")
		{
			if (original.Width != compressed.Width || original.Height != compressed.Height)
			{
				throw new ArgumentException("Both images must have the same dimensions");
			}

			int length = original.Width * original.Height;

			bool useRed = channelMask.Contains("r", StringComparison.OrdinalIgnoreCase);
			bool useGreen = channelMask.Contains("g", StringComparison.OrdinalIgnoreCase);
			bool useBlue = channelMask.Contains("b", StringComparison.OrdinalIgnoreCase);
			bool useAlpha = channelMask.Contains("a", StringComparison.OrdinalIgnoreCase);

			int activeChannels = 0;
			if (useRed) activeChannels++;
			if (useGreen) activeChannels++;
			if (useBlue) activeChannels++;
			if (useAlpha) activeChannels++;

			if (activeChannels == 0)
			{
				throw new ArgumentException("Channel mask must contain at least one valid channel (r, g, b, or a)", nameof(channelMask));
			}

			float error = 0;
			for (int y = 0; y < original.Height; y++)
			{
				for (int x = 0; x < original.Width; x++)
				{
					var pixel = original[x, y];
					var otherPixel = compressed[x, y];

					if (useRed)
					{
						var dr = Math.Sign(otherPixel.R) * MathF.Log(1 + MathF.Abs(otherPixel.R)) -
						         Math.Sign(pixel.R) * MathF.Log(1 + MathF.Abs(pixel.R));
						error += dr * dr;
					}

					if (useGreen)
					{
						var dg = Math.Sign(otherPixel.G) * MathF.Log(1 + MathF.Abs(otherPixel.G)) -
						         Math.Sign(pixel.G) * MathF.Log(1 + MathF.Abs(pixel.G));
						error += dg * dg;
					}

					if (useBlue)
					{
						var db = Math.Sign(otherPixel.B) * MathF.Log(1 + MathF.Abs(otherPixel.B)) -
						         Math.Sign(pixel.B) * MathF.Log(1 + MathF.Abs(pixel.B));
						error += db * db;
					}

					if (useAlpha)
					{
						var da = Math.Sign(otherPixel.A) * MathF.Log(1 + MathF.Abs(otherPixel.A)) -
						         Math.Sign(pixel.A) * MathF.Log(1 + MathF.Abs(pixel.A));
						error += da * da;
					}
				}
			}

			return MathF.Sqrt(error / (activeChannels * length));
		}

		/// <summary>
		/// Calculates the L₂ norm (Euclidean distance) between normalized normal vectors.
		/// This is more appropriate for normal maps than standard RMSE as it considers the directional nature of normals.
		/// </summary>
		/// <param name="original">Original normal map data</param>
		/// <param name="compressed">Compressed normal map data</param>
		/// <param name="useRGB">Whether to use all three channels for the vector calculation</param>
		/// <returns>Average angular error between the normal vectors</returns>
		public static float CalculateNormalVectorDifference(Image<RgbaVector> original, Image<RgbaVector> compressed, bool useRGB = true)
		{
			if (original.Width != compressed.Width || original.Height != compressed.Height)
			{
				throw new ArgumentException("Both images must have the same dimensions");
			}

			int length = original.Width * original.Height;

			float totalError = 0;
			for (int y = 0; y < original.Height; y++)
			{
				for (int x = 0; x < original.Width; x++)
				{
					// Extract vector components - we need to transform from [0,1] to [-1,1] range
					// Normal maps typically store normals as: RGB = (Normal.x+1, Normal.y+1, Normal.z+1)/2
					float origX = original[x, y].R * 2 - 1;
					float origY = original[x, y].G * 2 - 1;
					float origZ = useRGB ? (original[x, y].B * 2 - 1) : 0;

					float compX = compressed[x, y].R * 2 - 1;
					float compY = compressed[x, y].G * 2 - 1;
					float compZ = useRGB ? (compressed[x, y].B * 2 - 1) : 0;

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
			}

			// Return average error (square root of mean squared error)
			// The theoretical maximum L₂ distance between two unit vectors is 2.0
			return MathF.Sqrt(totalError / length);
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
			Image<RgbaVector> original,
			Image<RgbaVector> compressed,
			TextureType textureType,
			BCnEncoder.Encoder.CompressionQuality quality,
			string channelMask,
			Xunit.Abstractions.ITestOutputHelper output = null)
		{
			// Verify images have same dimensions
			if (original.Width != compressed.Width || original.Height != compressed.Height)
			{
				throw new ArgumentException("Both images must have the same dimensions");
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
			Image<RgbaVector> original,
			Image<RgbaVector> compressed,
			BCnEncoder.Encoder.CompressionQuality quality,
			string channelMask,
			Xunit.Abstractions.ITestOutputHelper output)
		{
			// For standard albedo maps, MS-SSIM is a good perceptual metric
			float msssim = StructuralSimilarity.MultiScaleStructuralSimilarity(original, compressed, channelMask, 3);

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

		private static void AssertNormalMapQuality(
			Image<RgbaVector> original,
			Image<RgbaVector> compressed,
			BCnEncoder.Encoder.CompressionQuality quality,
			string channelMask,
			Xunit.Abstractions.ITestOutputHelper output)
		{
			// Determine if we should use all three channels (RGB) based on the channel mask
			bool useRGB = channelMask.Contains("b", StringComparison.OrdinalIgnoreCase);

			// Calculate L₂ norm of difference between normalized vectors
			float vectorDiff = CalculateNormalVectorDifference(original, compressed, useRGB);

			// For reference, also calculate standard RMSE
			float rmse = CalculateRMSE(original, compressed, channelMask);

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
			Image<RgbaVector> original,
			Image<RgbaVector> compressed,
			BCnEncoder.Encoder.CompressionQuality quality,
			string channelMask,
			Xunit.Abstractions.ITestOutputHelper output)
		{
			// For height maps, combine RMSE and SSIM for both precision and structure
			// SSIM for structure preservation
			float ssim = StructuralSimilarity.SingleScaleStructuralSimilarity(original, compressed, channelMask);

			// RMSE for precision - use the direct Image<RgbaVector> overload
			float rmse = CalculateRMSE(original, compressed, channelMask);

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
			Image<RgbaVector> original,
			Image<RgbaVector> compressed,
			BCnEncoder.Encoder.CompressionQuality quality,
			string channelMask,
			Xunit.Abstractions.ITestOutputHelper output)
		{
			// For HDR textures, log-based RMSE is most appropriate - use direct Image<RgbaVector> overload
			float logRmse = CalculateLogRMSE(original, compressed, channelMask);

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
			Image<RgbaVector> original,
			Image<RgbaVector> compressed,
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
