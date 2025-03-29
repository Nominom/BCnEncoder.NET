using System;
using System.Linq;
using BCnEncoder.Shared;
using Xunit;

namespace BCnEncTests;

public class CompressionFormatTests
{
	private static readonly string[] blockCompressedFormatFilter = { "Bc", "Atc" };

	[Fact]
	public void CheckFilters()
	{
		string[] blockCompressedFormatFilter = { "bc", "atc" };

		var values = Enum.GetValues<CompressionFormat>();

		foreach (CompressionFormat format in values)
		{
			if (format == CompressionFormat.Unknown)
				continue;

			CompressionFormatInfo info = format.GetInfo();

			Assert.Equal(format, info.Format);

			if (format.ToString().Contains("Bc6u", StringComparison.OrdinalIgnoreCase))
			{
				Assert.Equal(CompressionFormatType.BlockUFloat, info.FormatType);
				Assert.False(format.IsSignedFormat(), $"Format {format} should not be signed: Info={info}");
				Assert.False(format.IsSNormFormat(), $"Format {format} should not be SNorm: Info={info}");
				Assert.False(format.IsUNormFormat(), $"Format {format} should not be UNorm: Info={info}");
				Assert.True(format.IsHdrFormat(), $"Format {format} should be HDR: Info={info}");
			}
			else if (format.ToString().Contains("Bc6s", StringComparison.OrdinalIgnoreCase))
			{
				Assert.Equal(CompressionFormatType.BlockSFloat, info.FormatType);
				Assert.True(format.IsSignedFormat(), $"Format {format} should not be signed: Info={info}");
				Assert.False(format.IsSNormFormat(), $"Format {format} should not be SNorm: Info={info}");
				Assert.False(format.IsUNormFormat(), $"Format {format} should not be UNorm: Info={info}");
				Assert.True(format.IsHdrFormat(), $"Format {format} should be HDR: Info={info}");
			}
			else if (format.ToString().EndsWith("S", StringComparison.OrdinalIgnoreCase))
			{
				Assert.True(format.IsSignedFormat(), $"Format {format} should be signed: Info={info}");
				Assert.True(format.IsSNormFormat(), $"Format {format} should be SNorm: Info={info}");
				Assert.False(format.IsUNormFormat(), $"Format {format} should not be UNorm: Info={info}");
				Assert.False(format.IsHdrFormat(), $"Format {format} should not be HDR: Info={info}");
			}
			else if (format.ToString().EndsWith("Half", StringComparison.OrdinalIgnoreCase))
			{
				Assert.Equal(CompressionFormatType.RawFloat, info.FormatType);
				Assert.True(format.IsSignedFormat(), $"Format {format} should be signed: Info={info}");
				Assert.False(format.IsSNormFormat(), $"Format {format} should not be SNorm: Info={info}");
				Assert.False(format.IsUNormFormat(), $"Format {format} should not be UNorm: Info={info}");
				Assert.True(format.IsHdrFormat(), $"Format {format} should be HDR: Info={info}");
			}
			else if (format.ToString().EndsWith("Float", StringComparison.OrdinalIgnoreCase))
			{
				Assert.Equal(CompressionFormatType.RawFloat, info.FormatType);
				Assert.True(format.IsSignedFormat(), $"Format {format} should be signed: Info={info}");
				Assert.False(format.IsSNormFormat(), $"Format {format} should not be SNorm: Info={info}");
				Assert.False(format.IsUNormFormat(), $"Format {format} should not be UNorm: Info={info}");
				Assert.True(format.IsHdrFormat(), $"Format {format} should be HDR: Info={info}");
			}
			else if (format.ToString().EndsWith("e", StringComparison.OrdinalIgnoreCase))
			{
				Assert.Equal(CompressionFormatType.RawSharedExponent, info.FormatType);
				Assert.False(format.IsSignedFormat(), $"Format {format} should not be signed: Info={info}");
				Assert.False(format.IsSNormFormat(), $"Format {format} should not be SNorm: Info={info}");
				Assert.False(format.IsUNormFormat(), $"Format {format} should not be UNorm: Info={info}");
				Assert.True(format.IsHdrFormat(), $"Format {format} should be HDR: Info={info}");
			}
			else
			{
				Assert.False(format.IsSignedFormat(), $"Format {format} should not be signed: Info={info}");
				Assert.False(format.IsSNormFormat(), $"Format {format} should not be SNorm: Info={info}");
				Assert.True(format.IsUNormFormat(), $"Format {format} should be UNorm: Info={info}");
				Assert.False(format.IsHdrFormat(), $"Format {format} should not be HDR: Info={info}");
			}

			if (format.ToString().EndsWith("sRGB", StringComparison.OrdinalIgnoreCase))
				Assert.True(format.IsSRGBFormat() && format.IsUNormFormat(), $"Format {format} should be sRGB and UNorm: IsSRGB={format.IsSRGBFormat()}, IsUNorm={format.IsUNormFormat()}, Info={info}");
			else
				Assert.False(format.IsSRGBFormat(), $"Format {format} should not be sRGB: Info={info}");

			if (blockCompressedFormatFilter.Any(filter => format.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)))
			{
				Assert.True(format.IsBlockCompressedFormat(),
					$"Format {format} should be block compressed: Info={info}");
				Assert.Equal(BlockPixelSize.Size4x4x1, format.GetBlockPixelSize());
			}
			else
			{
				Assert.False(format.IsBlockCompressedFormat(),
					$"Format {format} should not be block compressed: Info={info}");
				Assert.Equal(BlockPixelSize.Size1x1x1, format.GetBlockPixelSize());
			}
		}
	}
}
