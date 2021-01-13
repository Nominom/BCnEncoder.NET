using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Threading.Tasks;
using BCnEncoder.Shared;

namespace BCnEncoder.Decoder
{
	internal interface IRawDecoder
	{
		Rgba32[] Decode(ReadOnlyMemory<byte> data, int pixelWidth, int pixelHeight, OperationContext context);
	}

	/// <summary>
	/// A class to decode data to R components.
	/// </summary>
	public class RawRDecoder : IRawDecoder
	{
		private readonly bool redAsLuminance;

		/// <summary>
		/// Create a new instance of <see cref="RawRDecoder"/>.
		/// </summary>
		/// <param name="redAsLuminance">If the decoded component should be used as the red component or luminence.</param>
		public RawRDecoder(bool redAsLuminance)
		{
			this.redAsLuminance = redAsLuminance;
		}

		/// <summary>
		/// Decode the data to color components.
		/// </summary>
		/// <param name="data">The data to decode.</param>
		/// <param name="pixelWidth">The width of the image in pixels.</param>
		/// <param name="pixelHeight">The height of the image in pixels.</param>
		/// <param name="context">The context of the current operation.</param>
		/// <returns>The decoded color components.</returns>
		public Rgba32[] Decode(ReadOnlyMemory<byte> data, int pixelWidth, int pixelHeight, OperationContext context)
		{
			var output = new Rgba32[pixelWidth * pixelHeight];

			if (context.IsParallel)
			{
				var options = new ParallelOptions
				{
					MaxDegreeOfParallelism = context.TaskCount,
					CancellationToken = context.CancellationToken
				};
				Parallel.For(0, pixelWidth * pixelHeight, options, i =>
				{
					var span = data.Span;
					if (redAsLuminance)
					{
						output[i].R = span[i];
						output[i].G = span[i];
						output[i].B = span[i];
					}
					else
					{
						output[i].R = span[i];
						output[i].G = 0;
						output[i].B = 0;
					}

					output[i].A = 255;
				});
			}
			else
			{
				var span = data.Span;
				for (var i = 0; i < output.Length; i++)
				{
					if (redAsLuminance)
					{
						output[i].R = span[i];
						output[i].G = span[i];
						output[i].B = span[i];
					}
					else
					{
						output[i].R = span[i];
						output[i].G = 0;
						output[i].B = 0;
					}

					output[i].A = 255;
				}
			}

			return output;
		}
	}

	/// <summary>
	/// A class to decode data to RG components.
	/// </summary>
	public class RawRgDecoder : IRawDecoder
	{
		/// <summary>
		/// Decode the data to color components.
		/// </summary>
		/// <param name="data">The data to decode.</param>
		/// <param name="pixelWidth">The width of the image in pixels.</param>
		/// <param name="pixelHeight">The height of the image in pixels.</param>
		/// <param name="context">The context of the current operation.</param>
		/// <returns>The decoded color components.</returns>
		public Rgba32[] Decode(ReadOnlyMemory<byte> data, int pixelWidth, int pixelHeight, OperationContext context)
		{
			var output = new Rgba32[pixelWidth * pixelHeight];

			if (context.IsParallel)
			{
				var options = new ParallelOptions
				{
					MaxDegreeOfParallelism = context.TaskCount,
					CancellationToken = context.CancellationToken
				};
				Parallel.For(0, pixelWidth * pixelHeight, options, i =>
				{
					var span = data.Span;
					output[i].R = span[i * 2];
					output[i].G = span[i * 2 + 1];
					output[i].B = 0;
					output[i].A = 255;
				});
			}
			else
			{
				var span = data.Span;
				for (var i = 0; i < output.Length; i++)
				{
					output[i].R = span[i * 2];
					output[i].G = span[i * 2 + 1];
					output[i].B = 0;
					output[i].A = 255;
				}
			}

			return output;
		}
	}

	/// <summary>
	/// A class to decode data to RGB components.
	/// </summary>
	public class RawRgbDecoder : IRawDecoder
	{
		/// <summary>
		/// Decode the data to color components.
		/// </summary>
		/// <param name="data">The data to decode.</param>
		/// <param name="pixelWidth">The width of the image in pixels.</param>
		/// <param name="pixelHeight">The height of the image in pixels.</param>
		/// <param name="context">The context of the current operation.</param>
		/// <returns>The decoded color components.</returns>
		public Rgba32[] Decode(ReadOnlyMemory<byte> data, int pixelWidth, int pixelHeight, OperationContext context)
		{
			var output = new Rgba32[pixelWidth * pixelHeight];

			if (context.IsParallel)
			{
				var options = new ParallelOptions
				{
					MaxDegreeOfParallelism = context.TaskCount,
					CancellationToken = context.CancellationToken
				};
				Parallel.For(0, pixelWidth * pixelHeight, options, i =>
				{
					var span = data.Span;
					output[i].R = span[i * 3];
					output[i].G = span[i * 3 + 1];
					output[i].B = span[i * 3 + 2];
					output[i].A = 255;
				});
			}
			else
			{
				var span = data.Span;
				for (var i = 0; i < output.Length; i++)
				{
					output[i].R = span[i * 3];
					output[i].G = span[i * 3 + 1];
					output[i].B = span[i * 3 + 2];
					output[i].A = 255;
				}
			}

			return output;
		}
	}

	/// <summary>
	/// A class to decode data to RGBA components.
	/// </summary>
	public class RawRgbaDecoder : IRawDecoder
	{
		/// <summary>
		/// Decode the data to color components.
		/// </summary>
		/// <param name="data">The data to decode.</param>
		/// <param name="pixelWidth">The width of the image in pixels.</param>
		/// <param name="pixelHeight">The height of the image in pixels.</param>
		/// <param name="context">The context of the current operation.</param>
		/// <returns>The decoded color components.</returns>
		public Rgba32[] Decode(ReadOnlyMemory<byte> data, int pixelWidth, int pixelHeight, OperationContext context)
		{
			var output = new Rgba32[pixelWidth * pixelHeight];

			if (context.IsParallel)
			{
				var options = new ParallelOptions
				{
					MaxDegreeOfParallelism = context.TaskCount,
					CancellationToken = context.CancellationToken
				};
				Parallel.For(0, pixelWidth * pixelHeight, options, i =>
				{
					var span = data.Span;
					output[i].R = span[i * 4];
					output[i].G = span[i * 4 + 1];
					output[i].B = span[i * 4 + 2];
					output[i].A = span[i * 4 + 3];
				});
			}
			else
			{
				var span = data.Span;
				for (var i = 0; i < output.Length; i++)
				{
					output[i].R = span[i * 4];
					output[i].G = span[i * 4 + 1];
					output[i].B = span[i * 4 + 2];
					output[i].A = span[i * 4 + 3];
				}
			}

			return output;
		}
	}
}
