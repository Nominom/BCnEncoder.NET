using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.TextureFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BCnEncTests.Support
{
	public static class ImageLoader
	{
		public const string TestImageRawFolder = "../../../testImages/raw";
		public const string TestImageEncodedFolder = "../../../testImages/encoded";
		public const string TestImageReferenceFolder = "../../../testImages/reference";

		public static (string, System.Type)[] EncodedFileEndingPatterns => new (string, Type)[]
		{
			("ktx", typeof(KtxFile)),
			("dds", typeof(DdsFile)),
			("hdr", typeof(RadianceFile))
		};

		public static readonly List<(string, Exception)> ReadThrownExceptions = new List<(string, Exception)>();

		public static readonly IReadOnlyDictionary<string, Image<Rgba32>> TestRawImages = FindRawTestImages();
		public static readonly IReadOnlyDictionary<string, (ITextureFileFormat, Image<Rgba32>)> TestEncodedImages = FindEncodedTestImages();


		private static Dictionary<string, Image<Rgba32>> FindRawTestImages()
		{
			var rawImages = new Dictionary<string, Image<Rgba32>>();

			if (!new DirectoryInfo(TestImageRawFolder).Exists)
				throw new FileNotFoundException($"Raw test folder does not exist! {TestImageRawFolder}");

			foreach (var file in Directory.EnumerateFiles(TestImageRawFolder, "*.png", new EnumerationOptions
			         {
						 IgnoreInaccessible = true,
						 RecurseSubdirectories = false,
						 AttributesToSkip = FileAttributes.Directory | FileAttributes.System | FileAttributes.Hidden,
			         }))
			{
				try
				{
					var img = Image.Load<Rgba32>(file);
					var name = Path.GetFileNameWithoutExtension(file);
					rawImages.Add(name, img);
				}
				catch (Exception e)
				{
					ReadThrownExceptions.Add((file, e));
				}
			}

			return rawImages;
		}

		private static Dictionary<string, (ITextureFileFormat, Image<Rgba32>)> FindEncodedTestImages()
		{
			var encodedImages = new Dictionary<string, (ITextureFileFormat, Image<Rgba32>)>();

			if (!new DirectoryInfo(TestImageEncodedFolder).Exists)
				throw new FileNotFoundException($"Encoded test folder does not exist! {TestImageEncodedFolder}");

			if (!new DirectoryInfo(TestImageReferenceFolder).Exists)
				throw new FileNotFoundException($"Reference test folder does not exist! {TestImageReferenceFolder}");

			foreach (var file in Directory.EnumerateFiles(TestImageEncodedFolder, "*.*", new EnumerationOptions
			         {
				         IgnoreInaccessible = true,
				         RecurseSubdirectories = false,
				         AttributesToSkip = FileAttributes.Directory | FileAttributes.System | FileAttributes.Hidden
			         }))
			{
				try
				{
					var fileType = EncodedFileEndingPatterns.FirstOrDefault(x => file.EndsWith(x.Item1)).Item2;
					if (fileType == null)
					{
						continue;
					}

					var texture = Activator.CreateInstance(fileType) as ITextureFileFormat;
					using var fs = File.OpenRead(file);

					texture.ReadFromStream(fs);

					var referenceFileName = Path.Join(TestImageReferenceFolder,
						Path.GetFileNameWithoutExtension(file) + ".png");

					var reference = Image.Load<Rgba32>(referenceFileName);
					var name = Path.GetFileNameWithoutExtension(file);

					encodedImages.Add(name, (texture, reference));
				}
				catch (Exception e)
				{
					ReadThrownExceptions.Add((file, e));
				}
			}

			return encodedImages;
		}

		public static Image<Rgba32>[] TestCubemap { get; } = {
			LoadTestImage("../../../testImages/cubemap/right.png"),
			LoadTestImage("../../../testImages/cubemap/left.png"),
			LoadTestImage("../../../testImages/cubemap/top.png"),
			LoadTestImage("../../../testImages/cubemap/bottom.png"),
			LoadTestImage("../../../testImages/cubemap/back.png"),
			LoadTestImage("../../../testImages/cubemap/forward.png")
		};

		public static Image<Rgba32>[] TestCubemap2 { get; } = {
			LoadTestImage("../../../testImages/cubemap/cube_2_posx.jpg"),
			LoadTestImage("../../../testImages/cubemap/cube_2_negx.jpg"),
			LoadTestImage("../../../testImages/cubemap/cube_2_posy.jpg"),
			LoadTestImage("../../../testImages/cubemap/cube_2_negy.jpg"),
			LoadTestImage("../../../testImages/cubemap/cube_2_posz.jpg"),
			LoadTestImage("../../../testImages/cubemap/cube_2_negz.jpg")
		};

		public static RadianceFile TestHdr1 => TestEncodedImages["hdr_1_rgbe"].Item1 as RadianceFile;
		public static RadianceFile TestHdr2 => TestEncodedImages["hdr_2_rgbe"].Item1 as RadianceFile;

		public static Image<Rgba32> TestLdrRgba => TestRawImages["rgba_1"];

		internal static Image<Rgba32> LoadTestImage(string filename)
		{
			return Image.Load<Rgba32>(filename);
		}

		internal static BCnTextureData LoadTestData(string name)
		{
			return TestRawImages.ContainsKey(name) ?
				TestRawImages[name].ToBCnTextureData() :
				TestEncodedImages[name].Item1.ToTextureData();
		}
	}
}
