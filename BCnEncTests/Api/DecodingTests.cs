using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BCnEncoder.Decoder;
using BCnEncoder.Decoder.Options;
using BCnEncoder.Shared;
using BCnEncoder.TextureFormats;
using BCnEncTests.Support;
using Xunit;

namespace BCnEncTests.Api;

public class DecodingTests
{
	enum FileType
	{
		Dds,
		Ktx,
		Ktx2
	}

	const string testImageFolderRoot = "testimages";
	const string referenceFolder = "reference";

	static string[] unsupportedFilter = new string[] { "astc", "eac", "etc1", "etc2", "bc4s", "bc5s", "basisu" };

	private static DecoderOutputOptions defaultOptions = new DecoderOutputOptions()
	{
		AlphaHandling = DecoderAlphaHandling.KeepAsIs,
		InputColorSpace = InputColorSpaceAssumption.Auto,
		OutputColorSpace = OutputColorSpaceTarget.ProcessLinearPreserveColorSpace
	};

	[SkippableTheory]
	[MemberData(nameof(GetTestFiles), "testdds", ".dds")]
	public void TestDecodingDds(string testImage, string referenceImage, string path)
	{
		CheckSkip(testImage);
		var decoder = new BcDecoder(defaultOptions);

		// Load the DDS file
		var file = DdsFile.Load(File.OpenRead(Path.Combine(path, testImage)));

		var reference = TestHelper.GetReferenceImage(Path.Combine(path, referenceImage));

		var tolerance = GetTolerance(file.ToTextureData().Format, FileType.Dds);

		TestHelper.TestDecoding(testImage, file, reference, true, decoder, null, tolerance);
	}

	[SkippableTheory]
	[MemberData(nameof(GetTestFiles), "testktx", ".ktx")]
	public void TestDecodingKtx(string testImage, string referenceImage, string path)
	{
		CheckSkip(testImage);
		var decoder = new BcDecoder(defaultOptions);

		// Load the KTX file
		var file = KtxFile.Load(File.OpenRead(Path.Combine(path, testImage)));

		var reference = TestHelper.GetReferenceImage(Path.Combine(path, referenceImage));

		var tolerance = GetTolerance(file.ToTextureData().Format, FileType.Ktx);

		// Very high tolerance due to differences in decoding methods.
		TestHelper.TestDecoding(testImage, file, reference, true, decoder, null, tolerance);
	}

	[SkippableTheory(Skip = "Ktx2 not implemented")]
	[MemberData(nameof(GetTestFiles), "testktx2", ".ktx2")]
	public void TestDecodingKtx2(string testImage, string referenceImage, string path)
	{
		Skip.If(true, "Ktx2 not implemented");
	}

	public static void CheckSkip(string testImage)
	{
		string skipFound = unsupportedFilter.FirstOrDefault(filter =>
			testImage.Contains(filter, StringComparison.OrdinalIgnoreCase));

		Skip.If(skipFound != null, $"Unsupported format matching {skipFound}");
	}

	public static IEnumerable<object[]> GetTestFiles(string path, string extension)
	{
		path = Path.Combine(TestHelper.GetProjectRoot(), testImageFolderRoot, path);

		// Check if path exists
		if (!Directory.Exists(path))
		{
			throw new ArgumentException($"Path does not exist: {path}");
		}

		// Find the reference folder (should be in the test directory)
		string refPath = Path.Combine(path, referenceFolder);
		if (!Directory.Exists(refPath))
		{
			// If no reference folder found in test directory, use the explicit path
			refPath = referenceFolder;
			if (!Directory.Exists(refPath))
			{
				throw new ArgumentException($"Path does not exist: {path}");
			}
		}

		var testFiles = Directory.GetFiles(path, $"*{extension}")
			.Select(f => new FileInfo(f));

		var result = new List<object[]>();

		foreach (var testFile in testFiles)
		{
			// For each test file, find the corresponding reference PNG
			string refImageName = $"{Path.GetFileNameWithoutExtension(testFile.Name)}.png";
			string refImagePath = Path.Combine(refPath, refImageName);

			if (File.Exists(refImagePath))
			{
				result.Add(new object[] { Path.GetRelativePath(path, testFile.FullName), Path.GetRelativePath(path, refImagePath), path });
			}
			else
			{
				throw new ArgumentException($"Reference image not found: {refImagePath}");
			}
		}

		return result;
	}

	private static float GetTolerance(CompressionFormat format, FileType type)
	{
		const float minTolerance = 0.001f;
		const float one = 1.5f / 255f;
		const float three = 3.5f / 255f;


		return (format, type) switch
		{
			(_, _) when format.IsRawPixelFormat() => minTolerance,
			(CompressionFormat.Bc7, _)            => minTolerance,
			(CompressionFormat.Bc7_sRGB, _)       => minTolerance,
			(CompressionFormat.Bc6S, _)           => minTolerance,
			(CompressionFormat.Bc6U, _)           => minTolerance,
			(_, FileType.Dds)                     => one, // Dds has a small tolerance
			(_, FileType.Ktx)                     => three, // Ktx has a large tolerance due to different decoding methods
		};
	}
}
