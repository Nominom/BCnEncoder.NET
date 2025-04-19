using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Processing.Processors.Filters;

namespace BCnEncTests.Support
{
	public class StructuralSimilarityResult
	{
		public float Average { get; set; } = 1f;
		public float Percentile5 { get; set; } = 1f;
		public float Percentile10 { get; set; } = 1f;
		public float Percentile20 { get; set; } = 1f;

		public override string ToString()
		{
			return
				$"{nameof(Average)}: {Average:F3}, 5th percentile: {Percentile5:F3}, 10th percentile: {Percentile10:F3}, 20th percentile: {Percentile20:F3}";
		}
	}

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
		public static StructuralSimilarityResult MultiScaleStructuralSimilarity(
			Image<RgbaVector> original,
			Image<RgbaVector> other,
			string channelMask,
			int scales = 5)
		{
			return MultiScaleStructuralSimilarity_Simd(original, other, channelMask, scales);
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
		internal static StructuralSimilarityResult SingleScaleStructuralSimilarity(
			Image<RgbaVector> original,
			Image<RgbaVector> other,
			string channelMask)
		{
			// Single-scale SSIM is just MS-SSIM with one scale
			return MultiScaleStructuralSimilarity_Simd(original, other, channelMask, scales: 1);
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
				Sampler = new BicubicResampler(),
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

		/// <summary>
		/// Calculate a percentile value from a list of values.
		/// The list must be sorted in ascending order.
		/// </summary>
		/// <param name="values"></param>
		/// <param name="percentile"></param>
		/// <returns></returns>
		private static float CalculatePercentile(List<float> values, float percentile)
		{
			int count = values.Count;

			if (count == 0)
			{
				return 0;
			}

			// Calculate the index for the percentile
			int index = (int)(percentile * count);

			// Ensure index is valid
			index = Math.Max(0, Math.Min(count - 1, index));

			// Return the value at the percentile position
			return values[index];
		}

		private static float CalculateAverage(List<float> values)
		{
			int count = values.Count;

			if (count == 0)
			{
				return 0;
			}

			float sum = 0;
			foreach (var value in values)
			{
				sum += value;
			}

			return sum / count;
		}

		private static float[] CreateGaussianKernel(int windowSize, int windowRadius)
		{
			// Before your nested window loops, create a Gaussian kernel:
			float[] gaussianKernel = new float[windowSize * windowSize];
			float sigma = windowSize / 6.0f; // Standard deviation - typically 1/6 of the window size
			float sum = 0;

			// Pre-compute the 2D Gaussian kernel
			for (int wy = -windowRadius; wy <= windowRadius; wy++)
			{
				for (int wx = -windowRadius; wx <= windowRadius; wx++)
				{
					int idx = (wy + windowRadius) * windowSize + (wx + windowRadius);
					// Gaussian function: exp(-(x² + y²)/(2*σ²))
					gaussianKernel[idx] = MathF.Exp(-(wx * wx + wy * wy) / (2 * sigma * sigma));
					sum += gaussianKernel[idx];
				}
			}

			// Normalize the kernel
			for (int i = 0; i < gaussianKernel.Length; i++)
			{
				gaussianKernel[i] /= sum;
			}

			return gaussianKernel;
		}

		#region Simd

		private class SsimScaleValues
		{
			public List<float> SsimValues { get; } = new List<float>();
		}

		/// <inheritdoc cref="MultiScaleStructuralSimilarity"/>
		internal static StructuralSimilarityResult MultiScaleStructuralSimilarity_Simd(
			Image<RgbaVector> original,
			Image<RgbaVector> other,
			string channelMask,
			int scales)
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
			int maxScales = (int)Math.Log2(minDimension) - 3; // Ensure window size is at least 16x16
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

			// Standard scale weights from the paper
			float[] standardWeights =
			[
				0.0448f * 5,
				0.2856f * 5,
				0.3001f * 5,
				0.2363f * 5,
				0.1333f * 5
			];

			// Standard channel weights to approximate human perception
			Vector128<float> standardChannelWeights = Vector128.Create(
				0.25f, // R
				0.54f, // G
				0.11f, // B
				0.10f  // A
				);

			// Define MS-SSIM weights (from Wang et al. paper)
			float[] scaleWeights = new float[scales];

			// Fill with standard weights
			for (int i = 0; i < scales; i++)
			{
				// If 5 scales or less, use standard scales
				if (scales <= 5)
				{
					scaleWeights[i] = standardWeights[i] / scales;
				}
				else // Otherwise, use uniform scales
				{
					scaleWeights[i] = 1f / scales;
				}
			}

			// Constants to stabilize division with weak denominators
			const float c1 = 0.01f * 0.01f; // (K1*L)^2, K1=0.01, L=1 (assumed dynamic range)
			const float c2 = 0.03f * 0.03f; // (K2*L)^2, K2=0.03, L=1

			Vector128<float> c1Vector = Vector128.Create(c1);
			Vector128<float> c2Vector = Vector128.Create(c2);

			var config = new Configuration()
			{
				PreferContiguousImageBuffers = true,
			};

			// Create working copies of the images with contiguous buffers
			using var originalCopy = original.Clone(config);
			using var otherCopy = other.Clone(config);

			// Base window size for local statistics
			const int windowSize = 11;
			const int windowRadius = windowSize / 2;

			// Initialize Gaussian kernel
			float[] gaussianKernel = CreateGaussianKernel(windowSize, windowRadius);

			StructuralSimilarityResult msssimResult = new StructuralSimilarityResult();

			int currentWidth = original.Width;
			int currentHeight = original.Height;

			// Create a list to store SSIM maps for each scale
			var ssimMaps = new SsimScaleValues[scales];
			for (int i = 0; i < scales; i++)
			{
				ssimMaps[i] = new SsimScaleValues();
			}

			Vector128<float> channelsOnOffVector = Vector128.Create(
				useRed ? 1f : 0,
				useGreen ? 1f : 0,
				useBlue ? 1f : 0,
				useAlpha ? 1f: 0
			);

			Vector128<float> channelWeights = standardChannelWeights * channelsOnOffVector;
			float totalWeight = Vector128.Sum(channelWeights);

			if (totalWeight == 0)
			{
				throw new InvalidOperationException("Something is wrong with channel weights. Please fix.");
			}

			// Normalize weights
			channelWeights /= totalWeight;

			// Process each scale
			for (int scale = 0; scale < scales; scale++)
			{
				if (!originalCopy.DangerousTryGetSinglePixelMemory(out Memory<RgbaVector> originalMemory) || !otherCopy.DangerousTryGetSinglePixelMemory(out Memory<RgbaVector> otherMemory))
				{
					throw new Exception("Failed to get single pixel memory");
				}

				// Cast pixels to Vector128<float>
				Span<Vector128<float>> originalSpan = originalMemory.Span.Cast<RgbaVector, Vector128<float>>();
				Span<Vector128<float>> otherSpan = otherMemory.Span.Cast<RgbaVector, Vector128<float>>();

				// Calculate local statistics (mean, variance, covariance)
				// This is the core of the algorithm - sliding window calculation
				// Compute local statistics for valid windows
				int validWindows = 0;

				for (int y = windowRadius; y < currentHeight - windowRadius; y++)
				{
					for (int x = windowRadius; x < currentWidth - windowRadius; x++)
					{
						// Calculate statistics in window around (x,y)
						Vector128<float> sumX = default, sumY = default;
						Vector128<float> sumXX = default, sumYY = default, sumXY = default;

						// Sample window pixels
						for (int wy = -windowRadius; wy <= windowRadius; wy++)
						{
							for (int wx = -windowRadius; wx <= windowRadius; wx++)
							{
								int index = (y + wy) * currentWidth + (x + wx);

								// Get channel values (r, g, b, a)
								Vector128<float> xVal = originalSpan[index];
								Vector128<float> yVal = otherSpan[index];

								// Get Gaussian weight for this position
								// Weights is prenormalized to sum to 1
								int idx = (wy + windowRadius) * windowSize + (wx + windowRadius);
								float weight = gaussianKernel[idx];

								Vector128<float> weightVector = Vector128.Create(weight);

								// Apply weighted contribution
								sumX += xVal * weightVector;
								sumY += yVal * weightVector;
								sumXX += xVal * xVal * weightVector;
								sumYY += yVal * yVal * weightVector;
								sumXY += xVal * yVal * weightVector;
							}
						}

						// Local statistics
						Vector128<float> muX = sumX;
						Vector128<float> muY = sumY;
						Vector128<float> sigmaX2 = sumXX - (muX * muX);
						Vector128<float> sigmaY2 = sumYY - (muY * muY);
						Vector128<float> sigmaXY = sumXY - (muX * muY);

						var zeroVector = Vector128.Create(0f);
						var twoVector = Vector128.Create(2f);

						// Guard against negative variances due to floating-point errors
						sigmaX2 = Vector128.Max(zeroVector, sigmaX2);
						sigmaY2 = Vector128.Max(zeroVector, sigmaY2);

						Vector128<float> sqrtSigmaX2SigmaY2 = Vector128.Sqrt(sigmaX2 * sigmaY2);

						// Calculate SSIM components
						Vector128<float> l = (twoVector * muX * muY + c1Vector) / (muX * muX + muY * muY + c1Vector);
						Vector128<float> c = (twoVector * sqrtSigmaX2SigmaY2 + c2Vector) / (sigmaX2 + sigmaY2 + c2Vector);
						Vector128<float> s = (sigmaXY + c2Vector / twoVector) / (sqrtSigmaX2SigmaY2 + c2Vector / twoVector);

						// Horizontal add the channels multiplied by their weights (already normalized)
						float luminance = Vector128.Sum(l * channelWeights);
						float contrast =   Vector128.Sum(c * channelWeights);
						float structure = Vector128.Sum(s * channelWeights);

						ssimMaps[scale].SsimValues.Add(luminance * contrast * structure);

						validWindows++;
					}
				}

				if (validWindows == 0)
				{
					throw new Exception("No valid windows found");
				}

				// Sort the SSIM values (ascending)
				ssimMaps[scale].SsimValues.Sort();

				// Calculate average SSIM
				float averageSsim = CalculateAverage(ssimMaps[scale].SsimValues);
				float percentile5 = CalculatePercentile(ssimMaps[scale].SsimValues, 0.05f);
				float percentile10 = CalculatePercentile(ssimMaps[scale].SsimValues, 0.1f);
				float percentile20 = CalculatePercentile(ssimMaps[scale].SsimValues, 0.2f);

				// Ensure values are non-negative before applying power function
				// This follows image quality research convention where absolute structural similarity is used
				float absAverageSsim = Math.Abs(averageSsim);
				float absPercentile5 = Math.Abs(percentile5);
				float absPercentile10 = Math.Abs(percentile10);
				float absPercentile20 = Math.Abs(percentile20);

				msssimResult.Average *= MathF.Pow(absAverageSsim, scaleWeights[scale]);
				msssimResult.Percentile5 *= MathF.Pow(absPercentile5, scaleWeights[scale]);
				msssimResult.Percentile10 *= MathF.Pow(absPercentile10, scaleWeights[scale]);
				msssimResult.Percentile20 *= MathF.Pow(absPercentile20, scaleWeights[scale]);

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

		#endregion

		#region Non-SIMD

		private class SsimMapScale
		{
			public int ValidWindows { get; set; }

			public List<float> SsimValues { get; } = new List<float>();
			public List<float>[] LuminanceValues { get; }
			public List<float>[] ContrastValues { get; }
			public List<float>[] StructureValues { get; }

			public SsimMapScale()
			{
				LuminanceValues = new List<float>[4];
				ContrastValues = new List<float>[4];
				StructureValues = new List<float>[4];

				for (int i = 0; i < 4; i++)
				{
					LuminanceValues[i] = new List<float>();
					ContrastValues[i] = new List<float>();
					StructureValues[i] = new List<float>();
				}
			}
		}

		/// <inheritdoc cref="MultiScaleStructuralSimilarity"/>
		internal static StructuralSimilarityResult MultiScaleStructuralSimilarity_NonSimd(
			Image<RgbaVector> original,
			Image<RgbaVector> other,
			string channelMask,
			int scales)
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
			int maxScales = (int)Math.Log2(minDimension) - 3; // Ensure window size is at least 16x16
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

			// Standard scale weights from the paper
			float[] standardWeights =
			[
				0.0448f * 5,
				0.2856f * 5,
				0.3001f * 5,
				0.2363f * 5,
				0.1333f * 5
			];

			// Standard channel weights to approximate human perception
			float[] standardChannelWeights =
			[
				0.25f,
				0.54f,
				0.11f,
				0.10f
			];

			// Define MS-SSIM weights (from Wang et al. paper)
			float[] scaleWeights = new float[scales];

			// Fill with standard weights (can be adjusted)
			for (int i = 0; i < scales; i++)
			{
				// If 5 scales or less, use standard scales
				if (scales <= 5)
				{
					scaleWeights[i] = standardWeights[i] / scales;
				}
				else // Otherwise, use uniform scales
				{
					scaleWeights[i] = 1f / scales;
				}
			}

			// Constants to stabilize division with weak denominators
			const float C1 = 0.01f * 0.01f; // (K1*L)^2, K1=0.01, L=1 (assumed dynamic range)
			const float C2 = 0.03f * 0.03f; // (K2*L)^2, K2=0.03, L=1

			// Create working copies of the images
			using var originalCopy = original.Clone();
			using var otherCopy = other.Clone();

			// Base window size for local statistics (typically 11×11)
			const int windowSize = 11;
			const int windowRadius = windowSize / 2;

			float[] gaussianKernel = CreateGaussianKernel(windowSize, windowRadius);

			// Actual window size will be adapted based on current image dimensions

			StructuralSimilarityResult msssimResult = new StructuralSimilarityResult();
			int currentWidth = original.Width;
			int currentHeight = original.Height;

			// Create a list to store SSIM maps for each scale
			var ssimMaps = new SsimMapScale[scales];
			for (int i = 0; i < scales; i++)
			{
				ssimMaps[i] = new SsimMapScale();
			}

			// Process each scale
			for (int scale = 0; scale < scales; scale++)
			{
				// Calculate SSIM at current scale
				float luminance = 0;
				float contrast = 0;
				float structure = 0;

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

									// Get Gaussian weight for this position
									int idx = (wy + windowRadius) * windowSize + (wx + windowRadius);
									float weight = gaussianKernel[idx];

									// Apply weighted contribution
									sumX += xVal * weight;
									sumY += yVal * weight;
									sumXX += xVal * xVal * weight;
									sumYY += yVal * yVal * weight;
									sumXY += xVal * yVal * weight;
								}
							}

							// Local statistics
							float muX = sumX;
							float muY = sumY;
							float sigmaX2 = sumXX - (muX * muX);
							float sigmaY2 = sumYY - (muY * muY);
							float sigmaXY = sumXY - (muX * muY);

							// Guard against negative variances due to floating-point errors
							sigmaX2 = Math.Max(0, sigmaX2);
							sigmaY2 = Math.Max(0, sigmaY2);

							// Calculate SSIM components
							float l = (2 * muX * muY + C1) / (muX * muX + muY * muY + C1);
							float c = (2 * MathF.Sqrt(sigmaX2) * MathF.Sqrt(sigmaY2) + C2) / (sigmaX2 + sigmaY2 + C2);
							float s = (sigmaXY + C2 / 2) / (MathF.Sqrt(sigmaX2 * sigmaY2) + C2 / 2);

							ssimMaps[scale].LuminanceValues[channel].Add(l);
							ssimMaps[scale].ContrastValues[channel].Add(c);
							ssimMaps[scale].StructureValues[channel].Add(s);

							validWindows++;
						}
					}

					ssimMaps[scale].ValidWindows = validWindows;
				}

				// Apply perceptual weights to all components
				for (int w = 0; w < ssimMaps[scale].ValidWindows; w++)
				{
					float totalWeight = 0;
					luminance = 0;
					contrast = 0;
					structure = 0;

					if (useRed)
					{
						luminance += ssimMaps[scale].LuminanceValues[0][w] * standardChannelWeights[0];
						contrast += ssimMaps[scale].ContrastValues[0][w] * standardChannelWeights[0];
						structure += ssimMaps[scale].StructureValues[0][w] * standardChannelWeights[0];
						totalWeight += standardChannelWeights[0];
					}

					if (useGreen)
					{
						luminance += ssimMaps[scale].LuminanceValues[1][w] * standardChannelWeights[1];
						contrast += ssimMaps[scale].ContrastValues[1][w] * standardChannelWeights[1];
						structure += ssimMaps[scale].StructureValues[1][w] * standardChannelWeights[1];
						totalWeight += standardChannelWeights[1];
					}

					if (useBlue)
					{
						luminance += ssimMaps[scale].LuminanceValues[2][w] * standardChannelWeights[2];
						contrast += ssimMaps[scale].ContrastValues[2][w] * standardChannelWeights[2];
						structure += ssimMaps[scale].StructureValues[2][w] * standardChannelWeights[2];
						totalWeight += standardChannelWeights[2];
					}

					if (useAlpha)
					{
						luminance += ssimMaps[scale].LuminanceValues[3][w] * standardChannelWeights[3];
						contrast += ssimMaps[scale].ContrastValues[3][w] * standardChannelWeights[3];
						structure += ssimMaps[scale].StructureValues[3][w] * standardChannelWeights[3];
						totalWeight += standardChannelWeights[3];
					}

					if (totalWeight > 0)
					{
						luminance /= totalWeight;
						contrast /= totalWeight;
						structure /= totalWeight;
					}

					ssimMaps[scale].SsimValues.Add(luminance * contrast * structure);
				}


				// Sort the SSIM values (ascending)
				ssimMaps[scale].SsimValues.Sort();

				// Calculate average SSIM
				float averageSsim = CalculateAverage(ssimMaps[scale].SsimValues);
				float percentile5 = CalculatePercentile(ssimMaps[scale].SsimValues, 0.05f);
				float percentile10 = CalculatePercentile(ssimMaps[scale].SsimValues, 0.1f);
				float percentile20 = CalculatePercentile(ssimMaps[scale].SsimValues, 0.2f);

				// Ensure values are non-negative before applying power function
				// This follows image quality research convention where absolute structural similarity is used
				float absAverageSsim = Math.Abs(averageSsim);
				float absPercentile5 = Math.Abs(percentile5);
				float absPercentile10 = Math.Abs(percentile10);
				float absPercentile20 = Math.Abs(percentile20);

				msssimResult.Average *= MathF.Pow(absAverageSsim, scaleWeights[scale]);
				msssimResult.Percentile5 *= MathF.Pow(absPercentile5, scaleWeights[scale]);
				msssimResult.Percentile10 *= MathF.Pow(absPercentile10, scaleWeights[scale]);
				msssimResult.Percentile20 *= MathF.Pow(absPercentile20, scaleWeights[scale]);

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

		#endregion
	}
}
