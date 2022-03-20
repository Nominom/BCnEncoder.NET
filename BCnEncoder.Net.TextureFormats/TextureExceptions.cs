using System;
using System.Collections.Generic;
using System.Text;

namespace BCnEncoder.TextureFormats
{
	/// <summary>
	/// Thrown when something goes wrong with converting to different texture file formats.
	/// </summary>
	public class TextureFormatException : FormatException
	{
		/// <inheritdoc />
		public TextureFormatException(string message) : base(message)
		{
		}
	}
}
