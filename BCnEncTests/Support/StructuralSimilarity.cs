using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Processing.Processors.Filters;

namespace BCnEncTests.Support
{
	/// <summary>
	/// Implementation of Multi-Scale Structural Similarity Index based on:
	/// Z. Wang, E. P. Simoncelli and A. C. Bovik, "Multiscale structural similarity for image quality assessment,"
	/// The Thirty-Seventh Asilomar Conference on Signals, Systems & Computers, 2003.
	/// </summary>
	public static class StructuralSimilarity
	{
		/// <summary>
		/// Calculates the Multi-Scale Structural Similarity Index (MS-SSIM) between two images.
		/// </summary>
		/// <param name="original">The original image.</param>
		/// <param name="other">The comparison image.</param>
		/// <param name="channelMask">String indicating which channels to include ("rgba"). Case-insensitive.</param>
		/// <param name="scales">Number of scales to use in calculation (default: 5).</param>
		/// <returns>MS-SSIM value between 0-1, where 1 indicates identical images.</returns>
		public static float MultiScaleStructuralSimilarity(
			Image<RgbaVector> original,
			Image<RgbaVector> other,
			string channelMask,
			int scales = 5)
		{
			if (original.Width != other.Width || original.Height != other.Height)
			{
				throw new ArgumentException("Both images must have the same dimensions");
			}

			if (string.IsNullOrEmpty(channelMask))
			{
				throw new ArgumentException("Channel mask must contain at least one channel", nameof(channelMask));
			}

			// Validate scales based on image dimensions
			int minDimension = Math.Min(original.Width, original.Height);
			int maxScales = (int)Math.Log2(minDimension) - 1; // Ensure smallest scale is at least 8×8
			scales = Math.Min(scales, maxScales);

			if (scales < 1)
			{
				throw new ArgumentException("Image is too small for MS-SSIM calculation");
			}

			channelMask = channelMask.ToLowerInvariant();

			// Prepare channel flags
			bool useRed = channelMask.Contains('r');
			bool useGreen = channelMask.Contains('g');
			bool useBlue = channelMask.Contains('b');
			bool useAlpha = channelMask.Contains('a');

			int activeChannels = 0;
			if (useRed) activeChannels++;
			if (useGreen) activeChannels++;
			if (useBlue) activeChannels++;
			if (useAlpha) activeChannels++;

			if (activeChannels == 0)
			{
				throw new ArgumentException("Channel mask must contain at least one valid channel (r, g, b, or a)",
					nameof(channelMask));
			}

			// Standard weights from the paper
			float[] standardBetaWeights =
			[
				0.0448f * 5,
				0.2856f * 5,
				0.3001f * 5,
				0.2363f * 5,
				0.1333f * 5
			];
			float[] standardGammaWeights =
			[
				0.0448f * 5,
				0.2856f * 5,
				0.3001f * 5,
				0.2363f * 5,
				0.1333f * 5
			];

			// Standard ITU-R BT.709 channel weights, with alpha having half the weight.
			float[] standardChannelWeights =
			[
				(0.2126f * 3.5f) / 4, // (0.2126f * 3.5f) / 4
				(0.7152f * 3.5f) / 4, // (0.7152f * 3.5f) / 4
				(0.0722f * 3.5f) / 4, // (0.0722f * 3.5f) / 4
				.5f / 4f              // .5f / 4f
			];

			// Define MS-SSIM weights (from Wang et al. paper)
			// Luminance weight (alpha) is only used at lowest scale
			float[] betaWeights = new float[scales]; // Contrast weights
			float[] gammaWeights = new float[scales]; // Structure weights

			// Fill with standard weights (can be adjusted)
			for (int i = 0; i < scales; i++)
			{
				// If 5 scales or less, use standard scales
				if (scales <= 5)
				{
					betaWeights[i] = standardBetaWeights[i] / scales;
					gammaWeights[i] = standardGammaWeights[i] / scales;
				}
				else // Otherwise, use uniform scales
				{
					betaWeights[i] = 1f / scales;
					gammaWeights[i] = 1f / scales;
				}
			}

			// Constants to stabilize division with weak denominators
			const float C1 = 0.01f * 0.01f; // (K1*L)^2, K1=0.01, L=1 (assumed dynamic range)
			const float C2 = 0.03f * 0.03f; // (K2*L)^2, K2=0.03, L=1

			// Create working copies of the images
			using var originalCopy = original.Clone();
			using var otherCopy = other.Clone();

			// Base window size for local statistics (typically 11×11)
			const int baseWindowSize = 11;

			// Actual window size will be adapted based on current image dimensions

			float msssimResult = 1.0f;
			int currentWidth = original.Width;
			int currentHeight = original.Height;

			// Process each scale
			for (int scale = 0; scale < scales; scale++)
			{
				// Check if we're at the final (lowest resolution) scale
				bool isFinalScale = (scale == scales - 1);

				// Calculate SSIM at current scale
				float luminance = 0;
				float contrast = 0;
				float structure = 0;

				// Adapt window size to current image dimensions
				// Ensure window is never larger than the image and always has at least 3×3 size
				int windowSize = Math.Min(baseWindowSize, Math.Min(currentWidth, currentHeight) - 2);
				windowSize = Math.Max(windowSize, 3); // Ensure at least 3×3 window
				windowSize = windowSize % 2 == 0 ? windowSize - 1 : windowSize; // Ensure odd size for symmetry
				int windowRadius = windowSize / 2;

				float[] channelLuminance = new float[4];
				float[] channelContrast = new float[4];
				float[] channelStructure = new float[4];

				// For each channel
				for (int channel = 0; channel < 4; channel++)
				{
					// Skip channels not in mask
					if ((channel == 0 && !useRed) ||
					    (channel == 1 && !useGreen) ||
					    (channel == 2 && !useBlue) ||
					    (channel == 3 && !useAlpha))
					{
						continue;
					}

					// Calculate local statistics (mean, variance, covariance)
					// This is the core of the algorithm - sliding window calculation
					float localContrast = 0;
					float localStructure = 0;

					// Compute local statistics for valid windows
					int validWindows = 0;

					for (int y = windowRadius; y < currentHeight - windowRadius; y++)
					{
						for (int x = windowRadius; x < currentWidth - windowRadius; x++)
						{
							// Calculate statistics in window around (x,y)
							float sumX = 0, sumY = 0;
							float sumXX = 0, sumYY = 0, sumXY = 0;

							// Sample window pixels
							for (int wy = -windowRadius; wy <= windowRadius; wy++)
							{
								for (int wx = -windowRadius; wx <= windowRadius; wx++)
								{
									// Get channel value (r, g, b, or a)
									float xVal = GetChannelValue(originalCopy[x + wx, y + wy], channel);
									float yVal = GetChannelValue(otherCopy[x + wx, y + wy], channel);

									sumX += xVal;
									sumY += yVal;
									sumXX += xVal * xVal;
									sumYY += yVal * yVal;
									sumXY += xVal * yVal;
								}
							}

							// Window pixel count
							int N = windowSize * windowSize;

							// Local statistics
							float muX = sumX / N;
							float muY = sumY / N;
							float sigmaX2 = (sumXX / N) - (muX * muX);
							float sigmaY2 = (sumYY / N) - (muY * muY);
							float sigmaXY = (sumXY / N) - (muX * muY);

							// Guard against negative variances due to floating-point errors
							sigmaX2 = Math.Max(0, sigmaX2);
							sigmaY2 = Math.Max(0, sigmaY2);

							// Calculate SSIM components
							float l = (2 * muX * muY + C1) / (muX * muX + muY * muY + C1);
							float c = (2 * MathF.Sqrt(sigmaX2) * MathF.Sqrt(sigmaY2) + C2) / (sigmaX2 + sigmaY2 + C2);
							float s = (sigmaXY + C2 / 2) / (MathF.Sqrt(sigmaX2) * MathF.Sqrt(sigmaY2) + C2 / 2);

							// For final scale, include luminance component
							if (isFinalScale)
							{
								channelLuminance[channel] += l;
							}

							localContrast += c;
							localStructure += s;
							validWindows++;
						}
					}

					// Average components across all windows
					if (validWindows > 0)
					{
						if (isFinalScale)
						{
							channelLuminance[channel] /= validWindows;
							// luminance += channelLuminance / validWindows;
						}

						channelContrast[channel] = localContrast / validWindows;
						channelStructure[channel] = localStructure / validWindows;
					}
				}

				// Apply perceptual weights to all components
				float totalWeight = 0;
				luminance = 0;
				contrast = 0;
				structure = 0;

				if (useRed) {
					if (isFinalScale) {
						luminance += channelLuminance[0] * standardChannelWeights[0];
					}
					contrast += channelContrast[0] * standardChannelWeights[0];
					structure += channelStructure[0] * standardChannelWeights[0];
					totalWeight += standardChannelWeights[0];
				}

				if (useGreen) {
					if (isFinalScale) {
						luminance += channelLuminance[1] * standardChannelWeights[1];
					}
					contrast += channelContrast[1] * standardChannelWeights[1];
					structure += channelStructure[1] * standardChannelWeights[1];
					totalWeight += standardChannelWeights[1];
				}

				if (useBlue) {
					if (isFinalScale) {
						luminance += channelLuminance[2] * standardChannelWeights[2];
					}
					contrast += channelContrast[2] * standardChannelWeights[2];
					structure += channelStructure[2] * standardChannelWeights[2];
					totalWeight += standardChannelWeights[2];
				}

				if (useAlpha) {
					if (isFinalScale) {
						luminance += channelLuminance[3] * standardChannelWeights[3];
					}
					contrast += channelContrast[3] * standardChannelWeights[3];
					structure += channelStructure[3] * standardChannelWeights[3];
					totalWeight += standardChannelWeights[3];
				}

				if (totalWeight > 0) {
					if (isFinalScale) {
						luminance /= totalWeight;
					}
					contrast /= totalWeight;
					structure /= totalWeight;
				}

				// Calculate the MS-SSIM components with weights
				// According to the Wang et al. paper:
				// 1. Luminance component is only used at the lowest scale (final scale)
				// 2. Contrast and structure each have their own weights at every scale
				if (isFinalScale)
				{
					// At the lowest scale, include luminance component with alpha weight (typically 1.0)
					msssimResult *= luminance;
				}

				// Apply specific weights to contrast and structure at each scale
				msssimResult *= MathF.Pow(contrast, betaWeights[scale]) * MathF.Pow(structure, gammaWeights[scale]);

				// Downsample for next scale if not at the last scale
				if (scale < scales - 1)
				{
					DownsampleImages(originalCopy, otherCopy);
					currentWidth /= 2;
					currentHeight /= 2;
				}
			}

			return msssimResult;
		}

		/// <summary>
		/// Calculates the standard Structural Similarity Index (SSIM) between two images.
		/// </summary>
		/// <param name="original">The original image.</param>
		/// <param name="other">The comparison image.</param>
		/// <param name="channelMask">String indicating which channels to include ("rgba"). Case-insensitive.</param>
		/// <returns>SSIM value between 0-1, where 1 indicates identical images.</returns>
		/// <remarks>
		/// SSIM is designed to better match human perception of image quality than metrics like PSNR.
		/// It measures structural information changes, perceived as variations in image structure.
		/// </remarks>
		public static float SingleScaleStructuralSimilarity(
			Image<RgbaVector> original,
			Image<RgbaVector> other,
			string channelMask)
		{
			// Single-scale SSIM is just MS-SSIM with one scale
			return MultiScaleStructuralSimilarity(original, other, channelMask, scales: 1);
		}

		/// <summary>
		/// Gets the channel value from a ColorRgbaFloat based on index.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float GetChannelValue(RgbaVector color, int channel)
		{
			switch (channel)
			{
				case 0: return color.R;
				case 1: return color.G;
				case 2: return color.B;
				case 3: return color.A;
				default: throw new ArgumentOutOfRangeException(nameof(channel));
			}
		}

		/// <summary>
		/// Downsamples both images using Gaussian filtering.
		/// </summary>
		/// <remarks>
		/// Uses ImageSharp's Gaussian filtering for high-quality downsampling that reduces aliasing artifacts.
		/// This produces better results than a simple box filter when calculating structural similarity.
		/// </remarks>
		private static void DownsampleImages(
			Image<RgbaVector> original,
			Image<RgbaVector> other)
		{
			int newWidth = original.Width / 2;
			int newHeight = original.Height / 2;

			// Configure resize options with Gaussian resampling
			var resizeOptions = new ResizeOptions
			{
				Size = new Size(newWidth, newHeight),
				Mode = ResizeMode.Stretch,
				Sampler = new BicubicResampler()
			};

			// Apply Gaussian blur before resizing to reduce aliasing
			float sigma = 0.8f; // Standard deviation for Gaussian kernel

			original.Mutate(ctx => ctx
				.GaussianBlur(sigma)
				.Resize(resizeOptions));

			other.Mutate(ctx => ctx
				.GaussianBlur(sigma)
				.Resize(resizeOptions));
		}
	}
}
