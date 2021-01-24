using BCnEncoder.Decoder.Options;
using BCnEncoder.Shared;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Shared.ImageFiles;

namespace BCnEncoder.Decoder
{
	/// <summary>
	/// Decodes compressed files into Rgba Format.
	/// </summary>
	public class BcDecoder
	{
		/// <summary>
		/// The input options of the decoder.
		/// </summary>
		public DecoderInputOptions InputOptions { get; } = new DecoderInputOptions();

		/// <summary>
		/// The options for the decoder.
		/// </summary>
		public DecoderOptions Options { get; } = new DecoderOptions();

		/// <summary>
		/// The output options of the decoder.
		/// </summary>
		public DecoderOutputOptions OutputOptions { get; } = new DecoderOutputOptions();

		#region Async Api

		/// <summary>
		/// DecodeInternal raw encoded image asynchronously.
		/// </summary>
		/// <param name="inputStream">The stream containing the encoded data.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded Rgba32 image.</returns>
		public Task<ColorRgba32[]> DecodeRawAsync(Stream inputStream, CompressionFormat format, int pixelWidth, int pixelHeight, CancellationToken token = default)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			inputStream.Read(dataArray, 0, dataArray.Length);

			return Task.Run(() => DecodeRawInternal(dataArray, pixelWidth, pixelHeight, format, token), token);
		}

		/// <summary>
		/// DecodeInternal raw encoded image data asynchronously.
		/// </summary>
		/// <param name="input">The <see cref="ReadOnlyMemory{T}"/> containing the encoded data.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded Rgba32 image.</returns>
		public Task<ColorRgba32[]> DecodeRawAsync(ReadOnlyMemory<byte> input, CompressionFormat format, int pixelWidth, int pixelHeight, CancellationToken token = default)
		{
			return Task.Run(() => DecodeRawInternal(input, pixelWidth, pixelHeight, format, token), token);
		}

		/// <summary>
		/// Read a Ktx or a Dds file from a stream and decode it asynchronously.
		/// </summary>
		/// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded Rgba32 image.</returns>
		public Task<ColorRgba32[]> DecodeAsync(Stream inputStream, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(inputStream, false, token)[0], token);
		}

		/// <summary>
		/// Read a Ktx or a Dds file from a stream and decode it.
		/// </summary>
		/// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded Rgba32 image.</returns>
		public Task<ColorRgba32[][]> DecodeAllMipMapsAsync(Stream inputStream, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(inputStream, true, token), token);
		}

		/// <summary>
		/// Read a Ktx file and decode it.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded Rgba32 image.</returns>
		public Task<ColorRgba32[]> DecodeAsync(KtxFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(file, false, token)[0], token);
		}

		/// <summary>
		/// Read a Ktx file and decode it.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded Rgba32 image.</returns>
		public Task<ColorRgba32[][]> DecodeAllMipMapsAsync(KtxFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(file, true, token), token);
		}

		/// <summary>
		/// Read a Dds file and decode it.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded Rgba32 image.</returns>
		public Task<ColorRgba32[]> DecodeAsync(DdsFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(file, false, token)[0], token);
		}

		/// <summary>
		/// Read a Dds file and decode it.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <param name="token">The cancellation token for this asynchronous operation.</param>
		/// <returns>The awaitable operation to retrieve the decoded Rgba32 image.</returns>
		public Task<ColorRgba32[][]> DecodeAllMipMapsAsync(DdsFile file, CancellationToken token = default)
		{
			return Task.Run(() => DecodeInternal(file, true, token), token);
		}

		#endregion

		#region Sync API

		/// <summary>
		/// DecodeInternal raw encoded image data.
		/// </summary>
		/// <param name="inputStream">The stream containing the encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public ColorRgba32[] DecodeRaw(Stream inputStream, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			var dataArray = new byte[GetBufferSize(format, pixelWidth, pixelHeight)];
			inputStream.Read(dataArray, 0, dataArray.Length);

			return DecodeRaw(dataArray, pixelWidth, pixelHeight, format);
		}

		/// <summary>
		/// DecodeInternal raw encoded image data.
		/// </summary>
		/// <param name="input">The array containing the encoded data.</param>
		/// <param name="pixelWidth">The pixelWidth of the image.</param>
		/// <param name="pixelHeight">The pixelHeight of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public ColorRgba32[] DecodeRaw(byte[] input, int pixelWidth, int pixelHeight, CompressionFormat format)
		{
			return DecodeRawInternal(input, pixelWidth, pixelHeight, format, default);
		}

		/// <summary>
		/// Read a Ktx or a Dds file from a stream and decode it.
		/// </summary>
		/// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public ColorRgba32[] Decode(Stream inputStream)
		{
			return DecodeInternal(inputStream, false, default)[0];
		}

		/// <summary>
		/// Read a Ktx or a Dds file from a stream and decode it.
		/// </summary>
		/// <param name="inputStream">The stream containing a Ktx or Dds file.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		public ColorRgba32[][] DecodeAllMipMaps(Stream inputStream)
		{
			return DecodeInternal(inputStream, true, default);
		}

		/// <summary>
		/// Read a Ktx file and decode it.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public ColorRgba32[] Decode(KtxFile file)
		{
			return DecodeInternal(file, false, default)[0];
		}

		/// <summary>
		/// Read a Ktx file and decode it.
		/// </summary>
		/// <param name="file">The loaded Ktx file.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		public ColorRgba32[][] DecodeAllMipMaps(KtxFile file)
		{
			return DecodeInternal(file, true, default);
		}

		/// <summary>
		/// Read a Dds file and decode it.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		public ColorRgba32[] Decode(DdsFile file)
		{
			return DecodeInternal(file, false, default)[0];
		}

		/// <summary>
		/// Read a Dds file and decode it.
		/// </summary>
		/// <param name="file">The loaded Dds file.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		public ColorRgba32[][] DecodeAllMipMaps(DdsFile file)
		{
			return DecodeInternal(file, true, default);
		}

		#endregion

		/// <summary>
		/// Load a stream and extract either the main image or all mip maps.
		/// </summary>
		/// <param name="stream">The stream containing the image file.</param>
		/// <param name="allMipMaps">If all mip maps or only the main image should be decoded.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		private ColorRgba32[][] DecodeInternal(Stream stream, bool allMipMaps, CancellationToken token)
		{
			var format = ImageFile.DetermineImageFormat(stream);

			switch (format)
			{
				case ImageFileFormat.Dds:
					return DecodeInternal(DdsFile.Load(stream), allMipMaps, token);

				case ImageFileFormat.Ktx:
					return DecodeInternal(KtxFile.Load(stream), allMipMaps, token);

				default:
					throw new InvalidOperationException("Unknown image format.");
			}
		}

		/// <summary>
		/// Load a KTX file and extract either the main image or all mip maps.
		/// </summary>
		/// <param name="file">The Ktx file to decode.</param>
		/// <param name="allMipMaps">If all mip maps or only the main image should be decoded.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		private ColorRgba32[][] DecodeInternal(KtxFile file, bool allMipMaps, CancellationToken token)
		{
			var colors = new ColorRgba32[file.MipMaps.Count][];

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var blockSize = GetBlockSize(file.header.GlInternalFormat);
			var totalBlocks = file.MipMaps.Sum(m => m.Faces[0].Data.Length / blockSize);

			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			if (IsSupportedRawFormat(file.header.GlInternalFormat))
			{
				var decoder = GetRawDecoder(file.header.GlInternalFormat);

				var mipMaps = allMipMaps ? file.MipMaps.Count : 1;
				for (var mip = 0; mip < mipMaps; mip++)
				{
					var data = file.MipMaps[mip].Faces[0].Data;

					colors[mip] = decoder.Decode(data, context);

					context.Progress.SetProcessedBlocks(file.MipMaps.Take(mip + 1).Sum(x => x.Faces[0].Data.Length / blockSize));
				}
			}
			else
			{
				var decoder = GetDecoder(file.header.GlInternalFormat);
				if (decoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {file.header.GlInternalFormat}");
				}

				var mipMaps = allMipMaps ? file.MipMaps.Count : 1;
				for (var mip = 0; mip < mipMaps; mip++)
				{
					var data = file.MipMaps[mip].Faces[0].Data;
					var pixelWidth = file.MipMaps[mip].Width;
					var pixelHeight = file.MipMaps[mip].Height;

					var blocks = decoder.Decode(data, context);

					colors[mip] = ImageToBlocks.ColorsFromRawBlocks(blocks, (int)pixelWidth, (int)pixelHeight);

					context.Progress.SetProcessedBlocks(file.MipMaps.Take(mip + 1).Sum(x => x.Faces[0].Data.Length / blockSize));
				}
			}

			return colors;
		}

		/// <summary>
		/// Load a DDS file and extract either the main image or all mip maps.
		/// </summary>
		/// <param name="file">The Dds file to decode.</param>
		/// <param name="allMipMaps">If all mip maps or only the main image should be decoded.</param>
		/// <param name="token">The cancellation token for this operation. Can be default, if the operation is not asynchronous.</param>
		/// <returns>An array of decoded Rgba32 images.</returns>
		private ColorRgba32[][] DecodeInternal(DdsFile file, bool allMipMaps, CancellationToken token)
		{
			var colors = new ColorRgba32[file.header.dwMipMapCount][];

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var blockSize = GetBlockSize(file);
			var totalBlocks = file.Faces[0].MipMaps.Sum(m => m.Data.Length / blockSize);

			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			if (IsSupportedRawFormat(file))
			{
				var decoder = GetRawDecoder(file);

				var mipMaps = allMipMaps ? file.header.dwMipMapCount : 1;
				for (var mip = 0; mip < mipMaps; mip++)
				{
					var data = file.Faces[0].MipMaps[mip].Data;

					colors[mip] = decoder.Decode(data, context);

					context.Progress.SetProcessedBlocks(file.Faces[0].MipMaps.Take(mip + 1).Sum(x => x.Data.Length / blockSize));
				}
			}
			else
			{
				var format = file.header.ddsPixelFormat.IsDxt10Format ?
					file.dx10Header.dxgiFormat :
					file.header.ddsPixelFormat.DxgiFormat;
				var decoder = GetDecoder(file);

				if (decoder == null)
				{
					throw new NotSupportedException($"This Format is not supported: {format}");
				}

				for (var mip = 0; mip < file.header.dwMipMapCount; mip++)
				{
					var data = file.Faces[0].MipMaps[mip].Data;
					var pixelWidth = file.Faces[0].MipMaps[mip].Width;
					var pixelHeight = file.Faces[0].MipMaps[mip].Height;

					var blocks = decoder.Decode(data, context);

					var image = ImageToBlocks.ColorsFromRawBlocks(blocks, (int)pixelWidth, (int)pixelHeight);

					colors[mip] = image;

					context.Progress.SetProcessedBlocks(file.Faces[0].MipMaps.Take(mip + 1).Sum(x => x.Data.Length / blockSize));
				}
			}

			return colors;
		}

		/// <summary>
		/// DecodeInternal raw encoded image asynchronously.
		/// </summary>
		/// <param name="input">The <see cref="ReadOnlyMemory{T}"/> containing the encoded data.</param>
		/// <param name="pixelWidth">The width of the image.</param>
		/// <param name="pixelHeight">The height of the image.</param>
		/// <param name="format">The Format the encoded data is in.</param>
		/// <param name="token">The cancellation token for this operation. May be default, if the operation is not asynchronous.</param>
		/// <returns>The decoded Rgba32 image.</returns>
		private ColorRgba32[] DecodeRawInternal(ReadOnlyMemory<byte> input, int pixelWidth, int pixelHeight, CompressionFormat format, CancellationToken token)
		{
			if (input.Length % GetBlockSize(format) != 0)
			{
				throw new ArgumentException("The size of the input buffer does not align with the compression format.");
			}

			var context = new OperationContext
			{
				CancellationToken = token,
				IsParallel = Options.IsParallel,
				TaskCount = Options.TaskCount
			};

			// Calculate total blocks
			var blockSize = GetBlockSize(format);
			var totalBlocks = input.Length / blockSize;

			context.Progress = new OperationProgress(Options.Progress, totalBlocks);

			var isCompressedFormat = format.IsCompressedFormat();
			if (isCompressedFormat)
			{
				// DecodeInternal as compressed data
				var decoder = GetDecoder(format);
				var blocks = decoder.Decode(input, context);

				return ImageToBlocks.ColorsFromRawBlocks(blocks, pixelWidth, pixelHeight); ;
			}

			// DecodeInternal as raw data
			var rawDecoder = GetRawDecoder(format);

			return rawDecoder.Decode(input, context);
		}

		#region Support

		#region Is supported format

		private bool IsSupportedRawFormat(GlInternalFormat format)
		{
			return IsSupportedRawFormat(GetCompressionFormat(format));
		}

		private bool IsSupportedRawFormat(DdsFile file)
		{
			return IsSupportedRawFormat(GetCompressionFormat(file));
		}

		private bool IsSupportedRawFormat(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R:
				case CompressionFormat.Rg:
				case CompressionFormat.Rgb:
				case CompressionFormat.Rgba:
				case CompressionFormat.Bgra:
					return true;

				default:
					return false;
			}
		}

		#endregion

		#region Get decoder

		private IBcBlockDecoder GetDecoder(GlInternalFormat format)
		{
			return GetDecoder(GetCompressionFormat(format));
		}

		private IBcBlockDecoder GetDecoder(DdsFile file)
		{
			return GetDecoder(GetCompressionFormat(file));
		}

		private IBcBlockDecoder GetDecoder(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.Bc1:
					return new Bc1NoAlphaDecoder();

				case CompressionFormat.Bc1WithAlpha:
					return new Bc1ADecoder();

				case CompressionFormat.Bc2:
					return new Bc2Decoder();

				case CompressionFormat.Bc3:
					return new Bc3Decoder();

				case CompressionFormat.Bc4:
					return new Bc4Decoder(OutputOptions.RedAsLuminance);

				case CompressionFormat.Bc5:
					return new Bc5Decoder();

				case CompressionFormat.Bc7:
					return new Bc7Decoder();

				case CompressionFormat.Atc:
					return new AtcDecoder();

				case CompressionFormat.AtcExplicitAlpha:
					return new AtcExplicitAlphaDecoder();

				case CompressionFormat.AtcInterpolatedAlpha:
					return new AtcInterpolatedAlphaDecoder();

				default:
					return null;
			}
		}

		#endregion

		#region Get raw decoder

		private IRawDecoder GetRawDecoder(GlInternalFormat format)
		{
			return GetRawDecoder(GetCompressionFormat(format));
		}

		private IRawDecoder GetRawDecoder(DdsFile file)
		{
			return GetRawDecoder(GetCompressionFormat(file));
		}

		private IRawDecoder GetRawDecoder(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R:
					return new RawRDecoder(OutputOptions.RedAsLuminance);

				case CompressionFormat.Rg:
					return new RawRgDecoder();

				case CompressionFormat.Rgb:
					return new RawRgbDecoder();

				case CompressionFormat.Rgba:
					return new RawRgbaDecoder();

				case CompressionFormat.Bgra:
					return new RawBgraDecoder();

				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		#endregion

		#region Get block size

		private int GetBlockSize(GlInternalFormat format)
		{
			return GetBlockSize(GetCompressionFormat(format));
		}

		private int GetBlockSize(DdsFile file)
		{
			return GetBlockSize(GetCompressionFormat(file));
		}

		private int GetBlockSize(CompressionFormat format)
		{
			switch (format)
			{
				case CompressionFormat.R:
					return 1;

				case CompressionFormat.Rg:
					return 2;

				case CompressionFormat.Rgb:
					return 3;

				case CompressionFormat.Rgba:
					return 4;

				case CompressionFormat.Bgra:
					return 4;

				case CompressionFormat.Bc1:
				case CompressionFormat.Bc1WithAlpha:
					return Unsafe.SizeOf<Bc1Block>();

				case CompressionFormat.Bc2:
					return Unsafe.SizeOf<Bc2Block>();

				case CompressionFormat.Bc3:
					return Unsafe.SizeOf<Bc3Block>();

				case CompressionFormat.Bc4:
					return Unsafe.SizeOf<Bc4Block>();

				case CompressionFormat.Bc5:
					return Unsafe.SizeOf<Bc5Block>();

				case CompressionFormat.Bc7:
					return Unsafe.SizeOf<Bc7Block>();

				case CompressionFormat.Atc:
					return Unsafe.SizeOf<AtcBlock>();

				case CompressionFormat.AtcExplicitAlpha:
					return Unsafe.SizeOf<AtcExplicitAlphaBlock>();

				case CompressionFormat.AtcInterpolatedAlpha:
					return Unsafe.SizeOf<AtcInterpolatedAlphaBlock>();

				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		#endregion

		private CompressionFormat GetCompressionFormat(GlInternalFormat format)
		{
			switch (format)
			{
				case GlInternalFormat.GlR8:
					return CompressionFormat.R;

				case GlInternalFormat.GlRg8:
					return CompressionFormat.Rg;

				case GlInternalFormat.GlRgb8:
					return CompressionFormat.Rgb;

				case GlInternalFormat.GlRgba8:
					return CompressionFormat.Rgba;

				// HINT: Bgra is only an extension by apple to the ktx format
				case GlInternalFormat.GlBgra8Extension:
					return CompressionFormat.Bgra;

				case GlInternalFormat.GlCompressedRgbS3TcDxt1Ext:
					return CompressionFormat.Bc1;

				case GlInternalFormat.GlCompressedRgbaS3TcDxt1Ext:
					return CompressionFormat.Bc1WithAlpha;

				case GlInternalFormat.GlCompressedRgbaS3TcDxt3Ext:
					return CompressionFormat.Bc2;

				case GlInternalFormat.GlCompressedRgbaS3TcDxt5Ext:
					return CompressionFormat.Bc3;

				case GlInternalFormat.GlCompressedRedRgtc1Ext:
					return CompressionFormat.Bc4;

				case GlInternalFormat.GlCompressedRedGreenRgtc2Ext:
					return CompressionFormat.Bc5;

				// TODO: Not sure what to do with SRGB input.
				case GlInternalFormat.GlCompressedRgbaBptcUnormArb:
				case GlInternalFormat.GlCompressedSrgbAlphaBptcUnormArb:
					return CompressionFormat.Bc7;

				case GlInternalFormat.GlCompressedRgbAtc:
					return CompressionFormat.Atc;

				case GlInternalFormat.GlCompressedRgbaAtcExplicitAlpha:
					return CompressionFormat.AtcExplicitAlpha;

				case GlInternalFormat.GlCompressedRgbaAtcInterpolatedAlpha:
					return CompressionFormat.AtcInterpolatedAlpha;

				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		private CompressionFormat GetCompressionFormat(DdsFile file)
		{
			var format = file.header.ddsPixelFormat.IsDxt10Format ?
				file.dx10Header.dxgiFormat :
				file.header.ddsPixelFormat.DxgiFormat;

			switch (format)
			{
				case DxgiFormat.DxgiFormatR8Unorm:
					return CompressionFormat.R;

				case DxgiFormat.DxgiFormatR8G8Unorm:
					return CompressionFormat.Rg;

				// HINT: R8G8B8 has no DxgiFormat to convert from

				case DxgiFormat.DxgiFormatR8G8B8A8Unorm:
					return CompressionFormat.Rgba;

				case DxgiFormat.DxgiFormatB8G8R8A8Unorm:
					return CompressionFormat.Bgra;

				case DxgiFormat.DxgiFormatBc1Unorm:
				case DxgiFormat.DxgiFormatBc1UnormSrgb:
				case DxgiFormat.DxgiFormatBc1Typeless:
					if (file.header.ddsPixelFormat.dwFlags.HasFlag(PixelFormatFlags.DdpfAlphapixels))
						return CompressionFormat.Bc1WithAlpha;

					if (InputOptions.DdsBc1ExpectAlpha)
						return CompressionFormat.Bc1WithAlpha;

					return CompressionFormat.Bc1;

				case DxgiFormat.DxgiFormatBc2Unorm:
				case DxgiFormat.DxgiFormatBc2UnormSrgb:
				case DxgiFormat.DxgiFormatBc2Typeless:
					return CompressionFormat.Bc2;

				case DxgiFormat.DxgiFormatBc3Unorm:
				case DxgiFormat.DxgiFormatBc3UnormSrgb:
				case DxgiFormat.DxgiFormatBc3Typeless:
					return CompressionFormat.Bc3;

				case DxgiFormat.DxgiFormatBc4Unorm:
				case DxgiFormat.DxgiFormatBc4Snorm:
				case DxgiFormat.DxgiFormatBc4Typeless:
					return CompressionFormat.Bc4;

				case DxgiFormat.DxgiFormatBc5Unorm:
				case DxgiFormat.DxgiFormatBc5Snorm:
				case DxgiFormat.DxgiFormatBc5Typeless:
					return CompressionFormat.Bc5;

				case DxgiFormat.DxgiFormatBc7Unorm:
				case DxgiFormat.DxgiFormatBc7UnormSrgb:
				case DxgiFormat.DxgiFormatBc7Typeless:
					return CompressionFormat.Bc7;

				case DxgiFormat.DxgiFormatAtc:
					return CompressionFormat.Atc;

				case DxgiFormat.DxgiFormatAtcExplicitAlpha:
					return CompressionFormat.AtcExplicitAlpha;

				case DxgiFormat.DxgiFormatAtcInterpolatedAlpha:
					return CompressionFormat.AtcInterpolatedAlpha;

				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		private int GetBufferSize(CompressionFormat format, int pixelWidth, int pixelHeight)
		{
			switch (format)
			{
				case CompressionFormat.R:
					return pixelWidth * pixelHeight;

				case CompressionFormat.Rg:
					return 2 * pixelWidth * pixelHeight;

				case CompressionFormat.Rgb:
					return 3 * pixelWidth * pixelHeight;

				case CompressionFormat.Rgba:
				case CompressionFormat.Bgra:
					return 4 * pixelWidth * pixelHeight;

				case CompressionFormat.Bc1:
				case CompressionFormat.Bc1WithAlpha:
				case CompressionFormat.Bc2:
				case CompressionFormat.Bc3:
				case CompressionFormat.Bc4:
				case CompressionFormat.Bc5:
				case CompressionFormat.Bc7:
				case CompressionFormat.Atc:
				case CompressionFormat.AtcExplicitAlpha:
				case CompressionFormat.AtcInterpolatedAlpha:
					return GetBlockSize(format) * ImageToBlocks.CalculateNumOfBlocks(pixelWidth, pixelHeight);

				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		#endregion
	}
}
