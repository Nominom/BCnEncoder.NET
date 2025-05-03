using System;
using System.Drawing;
using System.Numerics;
using BCnEncoder.Shared;
using BCnEncoder.Shared.Colors;
using CommunityToolkit.HighPerformance;

namespace BCnEncoder.Encoder
{
	internal static class ColorChooser
	{
		public static int ChooseClosestRgbColor4(ReadOnlySpan<Vector4> colors, Vector4 color, RgbWeights weights, out float error)
		{
			int idx = 0;
			error = weights.CalculateColorDiff(colors[0], color);
			for (int i = 1; i < 4; i++)
			{
				float d = weights.CalculateColorDiff(colors[i], color);
				if (d < error)
				{
					idx = i;
					error = d;
				}
			}
			return idx;
		}


		public static int ChooseClosestRgbColor4AlphaCutoff(ReadOnlySpan<Vector4> colors, Vector4 color, RgbWeights weights, float alphaCutoff, bool hasAlpha, out float error)
		{
			if (hasAlpha && color.W < alphaCutoff)
			{
				error = 0;
				return 3;
			}

			int idx = 0;
			error = weights.CalculateColorDiff(colors[0], color);
			for (int i = 1; i < 4; i++)
			{
				float d = weights.CalculateColorDiff(colors[i], color);
				if (d < error)
				{
					idx = i;
					error = d;
				}
			}

			return idx;
		}
	}
}
