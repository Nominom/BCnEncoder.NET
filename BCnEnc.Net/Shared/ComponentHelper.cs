using System;
using BCnEncoder.Encoder;
using BCnEncoder.Shared.Colors;

namespace BCnEncoder.Shared
{
	internal static class ComponentHelper
	{
		public static ColorRgba32 ComponentToColor(ColorComponent component, byte componentValue)
		{
			switch (component)
			{
				case ColorComponent.R:
					return new ColorRgba32(componentValue, 0, 0, 255);

				case ColorComponent.G:
					return new ColorRgba32(0, componentValue, 0, 255);

				case ColorComponent.B:
					return new ColorRgba32(0, 0, componentValue, 255);

				case ColorComponent.A:
					return new ColorRgba32(0, 0, 0, componentValue);

				case ColorComponent.Luminance:
					return new ColorRgba32(componentValue, componentValue, componentValue, 255);

				default:
					throw new InvalidOperationException("Unsupported component.");
			}
		}

		public static ColorRgba32 ComponentToColor(ColorRgba32 existingColor, ColorComponent component, byte componentValue)
		{
			switch (component)
			{
				case ColorComponent.R:
					existingColor.r = componentValue;
					break;

				case ColorComponent.G:
					existingColor.g = componentValue;
					break;

				case ColorComponent.B:
					existingColor.b = componentValue;
					break;

				case ColorComponent.A:
					existingColor.a = componentValue;
					break;

				case ColorComponent.Luminance:
					existingColor.r = existingColor.g = existingColor.b = componentValue;
					break;

				default:
					throw new InvalidOperationException("Unsupported component.");
			}

			return existingColor;
		}

		public static byte ColorToComponent(ColorRgba32 color, ColorComponent component)
		{
			switch (component)
			{
				case ColorComponent.R:
					return color.r;

				case ColorComponent.G:
					return color.g;

				case ColorComponent.B:
					return color.b;

				case ColorComponent.A:
					return color.a;

				case ColorComponent.Luminance:
					return (byte)(color.As<ColorYCbCr>().y * 255);

				default:
					throw new InvalidOperationException("Unsupported component.");
			}
		}
	}
}
