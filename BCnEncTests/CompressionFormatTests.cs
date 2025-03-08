using System;
using System.Linq;
using BCnEncoder.Shared;
using Xunit;

namespace BCnEncTests;

public class CompressionFormatTests
{
	private static readonly string[] blockCompressedFormatFilter = { "Bc, Atc" };

	[Fact]
	public void CheckFilters()
	{
		var values = Enum.GetValues<CompressionFormat>();

		foreach (CompressionFormat format in values)
		{
			if (format.ToString().EndsWith("sRGB", StringComparison.OrdinalIgnoreCase))
				Assert.True(format.IsSRGBFormat());
			else
				Assert.False(format.IsSRGBFormat());

			if (blockCompressedFormatFilter.Any(filter => format.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)))
				Assert.True(format.IsBlockCompressedFormat());
			else
				Assert.False(format.IsBlockCompressedFormat());
		}
	}
}
