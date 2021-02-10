using System;
using BCnEncoder.Shared;

namespace BCnEncoder.Decoder
{
	internal interface IRawDecoder
	{
		ColorRgba32[] Decode(ReadOnlyMemory<byte> data, OperationContext context);
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
		/// <param name="redAsLuminance">If the decoded component should be used as the red component or luminance.</param>
		public RawRDecoder(bool redAsLuminance)
		{
			this.redAsLuminance = redAsLuminance;
		}

		/// <summary>
		/// Decode the data to color components.
		/// </summary>
		/// <param name="data">The data to decode.</param>
		/// <param name="context">The context of the current operation.</param>
		/// <returns>The decoded color components.</returns>
		public ColorRgba32[] Decode(ReadOnlyMemory<byte> data, OperationContext context)
		{
			var output = new ColorRgba32[data.Length];

			// HINT: Ignoring parallel execution since we wouldn't gain performance from it.

			var span = data.Span;
			for (var i = 0; i < output.Length; i++)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (redAsLuminance)
				{
					output[i].r = span[i];
					output[i].g = span[i];
					output[i].b = span[i];
				}
				else
				{
					output[i].r = span[i];
					output[i].g = 0;
					output[i].b = 0;
				}

				output[i].a = 255;
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
		/// <param name="context">The context of the current operation.</param>
		/// <returns>The decoded color components.</returns>
		public ColorRgba32[] Decode(ReadOnlyMemory<byte> data, OperationContext context)
		{
			var output = new ColorRgba32[data.Length / 2];

			// HINT: Ignoring parallel execution since we wouldn't gain performance from it.

			var span = data.Span;
			for (var i = 0; i < output.Length; i++)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				output[i].r = span[i * 2];
				output[i].g = span[i * 2 + 1];
				output[i].b = 0;
				output[i].a = 255;
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
		/// <param name="context">The context of the current operation.</param>
		/// <returns>The decoded color components.</returns>
		public ColorRgba32[] Decode(ReadOnlyMemory<byte> data, OperationContext context)
		{
			var output = new ColorRgba32[data.Length / 3];

			// HINT: Ignoring parallel execution since we wouldn't gain performance from it.

			var span = data.Span;
			for (var i = 0; i < output.Length; i++)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				output[i].r = span[i * 3];
				output[i].g = span[i * 3 + 1];
				output[i].b = span[i * 3 + 2];
				output[i].a = 255;
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
		/// <param name="context">The context of the current operation.</param>
		/// <returns>The decoded color components.</returns>
		public ColorRgba32[] Decode(ReadOnlyMemory<byte> data, OperationContext context)
		{
			var output = new ColorRgba32[data.Length / 4];

			// HINT: Ignoring parallel execution since we wouldn't gain performance from it.

			var span = data.Span;
			for (var i = 0; i < output.Length; i++)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				output[i].r = span[i * 4];
				output[i].g = span[i * 4 + 1];
				output[i].b = span[i * 4 + 2];
				output[i].a = span[i * 4 + 3];
			}

			return output;
		}
	}

	/// <summary>
	/// A class to decode data to BGRA components.
	/// </summary>
	public class RawBgraDecoder : IRawDecoder
	{
		/// <summary>
		/// Decode the data to color components.
		/// </summary>
		/// <param name="data">The data to decode.</param>
		/// <param name="context">The context of the current operation.</param>
		/// <returns>The decoded color components.</returns>
		public ColorRgba32[] Decode(ReadOnlyMemory<byte> data, OperationContext context)
		{
			var output = new ColorRgba32[data.Length / 4];

			// HINT: Ignoring parallel execution since we wouldn't gain performance from it.

			var span = data.Span;
			for (var i = 0; i < output.Length; i++)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				output[i].b = span[i * 4];
				output[i].g = span[i * 4 + 1];
				output[i].r = span[i * 4 + 2];
				output[i].a = span[i * 4 + 3];
			}

			return output;
		}
	}
}
