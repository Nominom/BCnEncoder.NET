using System;
using BCnEncoder.Encoder;

namespace BCnEncoder.Shared
{
	internal static class ComponentHelper
	{
		public static ColorRgba32 ComponentToColor(Bc4Component component, byte componentValue)
		{
			switch (component)
			{
				case Bc4Component.R:
					return new ColorRgba32(componentValue, 0, 0, 255);

				case Bc4Component.G:
					return new ColorRgba32(0, componentValue, 0, 255);

				case Bc4Component.B:
					return new ColorRgba32(0, 0, componentValue, 255);

				case Bc4Component.A:
					return new ColorRgba32(0, 0, 0, componentValue);

				case Bc4Component.Luminance:
					return new ColorRgba32(componentValue, componentValue, componentValue, 255);

				default:
					throw new InvalidOperationException("Unsupported component.");
			}
		}

		public static ColorRgba32 ComponentToColor(ColorRgba32 existingColor, Bc4Component component, byte componentValue)
		{
			switch (component)
			{
				case Bc4Component.R:
					existingColor.r = componentValue;
					break;

				case Bc4Component.G:
					existingColor.g = componentValue;
					break;

				case Bc4Component.B:
					existingColor.b = componentValue;
					break;

				case Bc4Component.A:
					existingColor.a = componentValue;
					break;

				case Bc4Component.Luminance:
					existingColor.r = existingColor.g = existingColor.b = componentValue;
					break;

				default:
					throw new InvalidOperationException("Unsupported component.");
			}

			return existingColor;
		}

		public static byte ColorToComponent(ColorRgba32 color, Bc4Component component)
		{
			switch (component)
			{
				case Bc4Component.R:
					return color.r;

				case Bc4Component.G:
					return color.g;

				case Bc4Component.B:
					return color.b;

				case Bc4Component.A:
					return color.a;

				case Bc4Component.Luminance:
					return (byte)(new ColorYCbCr(color).y * 255);

				default:
					throw new InvalidOperationException("Unsupported component.");
			}
		}
	}
}
