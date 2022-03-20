using System;
using System.Collections.Generic;
using System.Text;

namespace BCnEncoder.Shared
{
	/// <summary>
	/// Thrown by the BcEncoder when something exceptional has happened (bad inputs or settings and such)
	/// </summary>
	public class EncoderException : Exception
	{
		public EncoderException(string message) : base(message) { }
	}

	/// <summary>
	/// Thrown by the BcDecoder when something exceptional has happened (bad inputs or settings and such)
	/// </summary>
	public class DecoderException : Exception
	{
		public DecoderException(string message) : base(message) { }
	}
}
