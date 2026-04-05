using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests.Support
{
	/// <summary>
	/// Validates the PSNR and SSIM implementations against ImageMagick's reference output.
	/// Tests are skipped when ImageMagick is not available on the system.
	/// </summary>
	public class ImageQualityTests
	{
		private readonly ITestOutputHelper _output;

		public ImageQualityTests(ITestOutputHelper output)
		{
			_output = output;
		}

		private static bool IsImageMagickAvailable()
		{
			try
			{
				using var proc = Process.Start(new ProcessStartInfo("compare", "--version")
				{
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false
				});
				proc?.WaitForExit(3000);
				return proc?.ExitCode == 0;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Runs ImageMagick compare with the given metric and returns combined stdout+stderr output.
		/// ImageMagick writes metric values to stderr.
		/// -alpha off disables alpha premultiplication so only RGB channels are compared,
		/// matching our channelMask="rgb" straight-alpha computation.
		/// </summary>
		private static string RunImageMagickMetric(string metric, string img1, string img2)
		{
			var psi = new ProcessStartInfo("compare", $"-alpha off -metric {metric} \"{img1}\" \"{img2}\" null:")
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			};

			using var proc = Process.Start(psi)!;
			string stdout = proc.StandardOutput.ReadToEnd();
			string stderr = proc.StandardError.ReadToEnd();
			proc.WaitForExit();

			// ImageMagick writes the value to stderr; stdout may be empty
			return stderr.Length > 0 ? stderr : stdout;
		}

		/// <summary>
		/// Parses the first floating-point number from ImageMagick output.
		/// Used for PSNR which outputs the dB value directly as the first token.
		/// </summary>
		private static double ParseFirstDouble(string output)
		{
			var match = Regex.Match(output.Trim(), @"[\d]+(?:\.[\d]+)?");
			if (!match.Success)
				throw new InvalidOperationException($"Could not parse metric value from ImageMagick output: '{output}'");
			return double.Parse(match.Value, System.Globalization.CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Parses the parenthetical (normalized) value from ImageMagick output.
		/// Used for SSIM: ImageMagick outputs DSSIM in parentheses, so SSIM = 1 - parenthetical.
		/// Example output: "3607.72 (0.0550502)"
		/// </summary>
		private static double ParseParenthetical(string output)
		{
			var match = Regex.Match(output.Trim(), @"\(([\d]+(?:\.[\d]+)?)\)");
			if (!match.Success)
				throw new InvalidOperationException($"Could not parse parenthetical value from ImageMagick output: '{output}'");
			return double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
		}

		[SkippableFact]
		public void PsnrBc1Matches()
		{
			Skip.If(!IsImageMagickAvailable(), "ImageMagick compare not available");

			var rawPath = Path.Combine(TestHelper.GetProjectRoot(), "testImages", "raw.png");
			var refPath = Path.Combine(TestHelper.GetProjectRoot(), "testImages", "reference", "bc1_unorm.png");

			var imOutput = RunImageMagickMetric("PSNR", rawPath, refPath);
			var imPsnr = ParseFirstDouble(imOutput);

			using var rawImage = Image.Load<RgbaVector>(rawPath);
			using var refImage = Image.Load<RgbaVector>(refPath);
			var ourPsnr = ImageQuality.CalculatePsnr(rawImage, refImage, "rgb");

			_output.WriteLine($"BC1 PSNR — ImageMagick: {imPsnr:F4} dB, Ours: {ourPsnr:F4} dB");

			Assert.True(Math.Abs(ourPsnr - imPsnr) < 1.5,
				$"PSNR mismatch: ours={ourPsnr:F4} dB, ImageMagick={imPsnr:F4} dB (tolerance 1.5 dB)");
		}

		[SkippableFact]
		public void PsnrBc7Matches()
		{
			Skip.If(!IsImageMagickAvailable(), "ImageMagick compare not available");

			var rawPath = Path.Combine(TestHelper.GetProjectRoot(), "testImages", "raw.png");
			var refPath = Path.Combine(TestHelper.GetProjectRoot(), "testImages", "reference", "bc7_unorm.png");

			var imOutput = RunImageMagickMetric("PSNR", rawPath, refPath);
			var imPsnr = ParseFirstDouble(imOutput);

			using var rawImage = Image.Load<RgbaVector>(rawPath);
			using var refImage = Image.Load<RgbaVector>(refPath);
			var ourPsnr = ImageQuality.CalculatePsnr(rawImage, refImage, "rgb");

			_output.WriteLine($"BC7 PSNR — ImageMagick: {imPsnr:F4} dB, Ours: {ourPsnr:F4} dB");

			Assert.True(Math.Abs(ourPsnr - imPsnr) < 1.5,
				$"PSNR mismatch: ours={ourPsnr:F4} dB, ImageMagick={imPsnr:F4} dB (tolerance 1.5 dB)");
		}

		[SkippableFact]
		public void SsimBc1Matches()
		{
			Skip.If(!IsImageMagickAvailable(), "ImageMagick compare not available");

			var rawPath = Path.Combine(TestHelper.GetProjectRoot(), "testImages", "raw.png");
			var refPath = Path.Combine(TestHelper.GetProjectRoot(), "testImages", "reference", "bc1_unorm.png");

			// ImageMagick outputs DSSIM (dissimilarity); SSIM = 1 - DSSIM
			var imOutput = RunImageMagickMetric("SSIM", rawPath, refPath);
			var imSsim = 1.0 - ParseParenthetical(imOutput);

			using var rawImage = Image.Load<RgbaVector>(rawPath);
			using var refImage = Image.Load<RgbaVector>(refPath);
			var ourSsim = StructuralSimilarity.SingleScaleStructuralSimilarity(rawImage, refImage, "rgb", false, false).Average;

			_output.WriteLine($"BC1 SSIM — ImageMagick: {imSsim:F6}, Ours: {ourSsim:F6}");

			Assert.True(Math.Abs(ourSsim - imSsim) < 0.05,
				$"SSIM mismatch: ours={ourSsim:F6}, ImageMagick={imSsim:F6} (tolerance 0.05)");
		}

		[SkippableFact]
		public void SsimBc7Matches()
		{
			Skip.If(!IsImageMagickAvailable(), "ImageMagick compare not available");

			var rawPath = Path.Combine(TestHelper.GetProjectRoot(), "testImages", "raw.png");
			var refPath = Path.Combine(TestHelper.GetProjectRoot(), "testImages", "reference", "bc7_unorm.png");

			// ImageMagick outputs DSSIM (dissimilarity); SSIM = 1 - DSSIM
			var imOutput = RunImageMagickMetric("SSIM", rawPath, refPath);
			var imSsim = 1.0 - ParseParenthetical(imOutput);

			using var rawImage = Image.Load<RgbaVector>(rawPath);
			using var refImage = Image.Load<RgbaVector>(refPath);
			var ourSsim = StructuralSimilarity.SingleScaleStructuralSimilarity(rawImage, refImage, "rgb", false, false).Average;

			_output.WriteLine($"BC7 SSIM — ImageMagick: {imSsim:F6}, Ours: {ourSsim:F6}");

			Assert.True(Math.Abs(ourSsim - imSsim) < 0.05,
				$"SSIM mismatch: ours={ourSsim:F6}, ImageMagick={imSsim:F6} (tolerance 0.05)");
		}
	}
}
