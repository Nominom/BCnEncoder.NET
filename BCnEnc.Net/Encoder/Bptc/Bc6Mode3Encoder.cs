using System;
using System.Collections.Generic;
using System.Text;
using BCnEncoder.Shared;

namespace BCnEncoder.Encoder.Bptc
{
	internal static class Bc6Mode3Encoder
	{
		private const int endpointBits = 10;

		public static Bc6Block EncodeBlock(RawBlock4X4RgbFloat block, ColorRgbFloat initialEndpoint0,
			ColorRgbFloat initialEndpoint1, bool variate, float targetError, bool signed)
		{
			var initialPreQuantizedEp0 = Bc6EncodingHelpers.PreQuantizeRawEndpoint(initialEndpoint0, signed);
			var initialPreQuantizedEp1 = Bc6EncodingHelpers.PreQuantizeRawEndpoint(initialEndpoint1, signed);

			var initialQuantizedEp0 =
				Bc6EncodingHelpers.FinishQuantizeEndpoint(initialPreQuantizedEp0, endpointBits, signed);
			var initialQuantizedEp1 =
				Bc6EncodingHelpers.FinishQuantizeEndpoint(initialPreQuantizedEp1, endpointBits, signed);

			var unquantizedEndpoint0 = Bc6Block.UnQuantize(initialQuantizedEp0, endpointBits, signed);
			var unquantizedEndpoint1 = Bc6Block.UnQuantize(initialQuantizedEp1, endpointBits, signed);

			Span<byte> indices = stackalloc byte[16];

			if (variate)
			{
				(unquantizedEndpoint0, unquantizedEndpoint1) = Bc6EncodingHelpers.VariateEndpoints1Sub(block, unquantizedEndpoint0, unquantizedEndpoint1, indices,
					endpointBits, signed, targetError);
			}
			else
			{
				Bc6EncodingHelpers.FindOptimalIndices1Sub(block, unquantizedEndpoint0, unquantizedEndpoint1, indices, signed);
			}

			Bc6EncodingHelpers.SwapIndicesIfNecessary1Sub(block, ref unquantizedEndpoint0, ref unquantizedEndpoint1, indices, signed);

			var quantEp0 =
				Bc6EncodingHelpers.FinishQuantizeEndpoint(unquantizedEndpoint0, endpointBits, signed);
			var quantEp1 =
				Bc6EncodingHelpers.FinishQuantizeEndpoint(unquantizedEndpoint1, endpointBits, signed);
			

			return Bc6Block.PackType3(quantEp0, quantEp1, indices);
		}
	}
}
