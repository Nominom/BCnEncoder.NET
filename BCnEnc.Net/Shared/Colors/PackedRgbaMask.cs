using System;
using System.Collections.Generic;

namespace BCnEncoder.Shared.Colors;

using System.Numerics;

/// <summary>
/// A color format mask, such as "B5G6R5" or "R10G10B10A2".
/// The order of components in the mask string is from least significant bit to most significant bit.
/// This is opposite to the Khronos VK_FORMAT_{component-format}_{numeric-format}_PACKxx convention.
/// For example, "B10G10R10A2" would place alpha in the most significant bits and blue in the least significant bits.
/// </summary>
public record PackedRgbaMask
{
	public int RedShift { get; init; }
	public int GreenShift { get; init; }
	public int BlueShift { get; init; }
	public int AlphaShift { get; init; }

	public ulong RedMask { get; init; }
	public ulong GreenMask { get; init; }
	public ulong BlueMask { get; init; }
	public ulong AlphaMask { get; init; }

	public ulong RedMaxValue { get; init; }
	public ulong GreenMaxValue { get; init; }
	public ulong BlueMaxValue { get; init; }
	public ulong AlphaMaxValue { get; init; }

	public bool HasRed => RedMask != 0;
	public bool HasGreen => GreenMask != 0;
	public bool HasBlue => BlueMask != 0;
	public bool HasAlpha => AlphaMask != 0;

	/// <summary>
	/// Construct a RgbaMask from a mask string, such as "R8G8B8A8", or "R5G6B5A1".
	/// The order and number of the components determines the shift and mask values.
	/// Parsing is case-insensitive.
	/// </summary>
	/// <param name="mask">A string in the format of component letter followed by bit count (e.g., "R8G8B8A8")</param>
	public PackedRgbaMask(string mask)
	{
		if (string.IsNullOrEmpty(mask))
		{
			throw new ArgumentException("Mask string cannot be null or empty", nameof(mask));
		}

		RedMask = 0;
		GreenMask = 0;
		BlueMask = 0;
		AlphaMask = 0;
		RedShift = 0;
		GreenShift = 0;
		BlueShift = 0;
		AlphaShift = 0;
		RedMaxValue = 0;
		GreenMaxValue = 0;
		BlueMaxValue = 0;
		AlphaMaxValue = 0;


		// Now process the components from least significant to most significant bit
		int currentShift = 0;
		int i = 0;

		while (i < mask.Length)
		{
			char component = char.ToUpperInvariant(mask[i]);
			i++;

			// Extract bit count
			int bitCount = 0;
			while (i < mask.Length && char.IsDigit(mask[i]))
			{
				bitCount = bitCount * 10 + (mask[i] - '0');
				i++;
			}

			if (bitCount <= 0 || bitCount > 64)
			{
				throw new ArgumentException($"Invalid bit count {bitCount} for component {component}", nameof(mask));
			}

			// Calculate mask (2^bitCount - 1)
			ulong componentMask = (1UL << bitCount) - 1;
			ulong maxValue = componentMask;

			// Assign shift, mask, and max values based on component
			switch (component)
			{
				case 'R':
					RedShift = currentShift;
					RedMask = componentMask << currentShift;
					RedMaxValue = maxValue;
					break;
				case 'G':
					GreenShift = currentShift;
					GreenMask = componentMask << currentShift;
					GreenMaxValue = maxValue;
					break;
				case 'B':
					BlueShift = currentShift;
					BlueMask = componentMask << currentShift;
					BlueMaxValue = maxValue;
					break;
				case 'A':
					AlphaShift = currentShift;
					AlphaMask = componentMask << currentShift;
					AlphaMaxValue = maxValue;
					break;
			}

			currentShift += bitCount;
		}
	}

	/// <summary>
	/// Returns a string representation of the color mask, such as "R8G8B8A8" or "B10G10R10A2".
	/// Components are returned in order from least significant bits to most significant bits.
	/// </summary>
	/// <returns>A string representation of the mask</returns>
	public override string ToString()
	{
		var result = new System.Text.StringBuilder();

		// Create a list of component information (component character, mask, shift)
		var components = new List<(char Component, ulong Mask, int Shift)>();
		if (HasRed) components.Add(('R', RedMask, RedShift));
		if (HasGreen) components.Add(('G', GreenMask, GreenShift));
		if (HasBlue) components.Add(('B', BlueMask, BlueShift));
		if (HasAlpha) components.Add(('A', AlphaMask, AlphaShift));

		// Sort by position in the packed value (lowest bit position first)
		// For masks with contiguous bits, we can simply compare their numeric values
		// as masks with higher bits set will have larger values
		components.Sort((a, b) => a.Mask.CompareTo(b.Mask));

		// Append components in sorted order
		foreach (var (component, mask, shift) in components)
		{
			AppendComponentIfPresent(result, component, mask, shift);
		}

		return result.ToString();
	}

	private void AppendComponentIfPresent(System.Text.StringBuilder builder, char component, ulong mask, int shift)
	{
		if (mask == 0)
		{
			return;
		}

		// Calculate the bit count by finding the number of bits set in the mask after shifting back
		ulong shiftedMask = mask >> shift;
		int bitCount = 0;
		while (shiftedMask > 0)
		{
			bitCount += (int)(shiftedMask & 1);
			shiftedMask >>= 1;
		}

		builder.Append(component);
		builder.Append(bitCount);
	}

	/// <summary>
	/// Extracts the red component from a packed color value using this mask.
	/// </summary>
	/// <typeparam name="T">The numeric type of the packed color</typeparam>
	/// <param name="packedColor">The packed color value to extract from</param>
	/// <returns>The extracted red component</returns>
	public T GetRed<T>(T packedColor) where T : INumber<T>
	{
		if (!HasRed)
		{
			return T.Zero;
		}

		ulong value = ulong.CreateChecked(packedColor);
		ulong result = (value & RedMask) >> RedShift;
		return T.CreateChecked(result);
	}

	/// <summary>
	/// Extracts the green component from a packed color value using this mask.
	/// </summary>
	/// <typeparam name="T">The numeric type of the packed color</typeparam>
	/// <param name="packedColor">The packed color value to extract from</param>
	/// <returns>The extracted green component</returns>
	public T GetGreen<T>(T packedColor) where T : INumber<T>
	{
		if (!HasGreen)
		{
			return T.Zero;
		}

		ulong value = ulong.CreateChecked(packedColor);
		ulong result = (value & GreenMask) >> GreenShift;
		return T.CreateChecked(result);
	}

	/// <summary>
	/// Extracts the blue component from a packed color value using this mask.
	/// </summary>
	/// <typeparam name="T">The numeric type of the packed color</typeparam>
	/// <param name="packedColor">The packed color value to extract from</param>
	/// <returns>The extracted blue component</returns>
	public T GetBlue<T>(T packedColor) where T : INumber<T>
	{
		if (!HasBlue)
		{
			return T.Zero;
		}

		ulong value = ulong.CreateChecked(packedColor);
		ulong result = (value & BlueMask) >> BlueShift;
		return T.CreateChecked(result);
	}

	/// <summary>
	/// Extracts the alpha component from a packed color value using this mask.
	/// </summary>
	/// <typeparam name="T">The numeric type of the packed color</typeparam>
	/// <param name="packedColor">The packed color value to extract from</param>
	/// <returns>The extracted alpha component</returns>
	public T GetAlpha<T>(T packedColor) where T : INumber<T>
	{
		if (!HasAlpha)
		{
			return T.Zero;
		}

		ulong value = ulong.CreateChecked(packedColor);
		ulong result = (value & AlphaMask) >> AlphaShift;
		return T.CreateChecked(result);
	}

	/// <summary>
	/// Sets the red component in a packed color value using this mask.
	/// </summary>
	/// <typeparam name="T">The numeric type of the packed color</typeparam>
	/// <param name="packedColor">The packed color value to modify</param>
	/// <param name="red">The red component value to set</param>
	public void SetRed<T>(ref T packedColor, T red) where T : INumber<T>
	{
		if (!HasRed)
		{
			return;
		}

		ulong packedValue = ulong.CreateChecked(packedColor);
		ulong componentValue = ulong.CreateChecked(red);

		// Clear the previous value for this component
		packedValue &= ~RedMask;

		// Ensure component value is within mask range
		componentValue = Math.Min(componentValue, RedMaxValue);

		// Set the new value
		packedValue |= (componentValue << RedShift);
		packedColor = T.CreateChecked(packedValue);
	}

	/// <summary>
	/// Sets the green component in a packed color value using this mask.
	/// </summary>
	/// <typeparam name="T">The numeric type of the packed color</typeparam>
	/// <param name="packedColor">The packed color value to modify</param>
	/// <param name="green">The green component value to set</param>
	public void SetGreen<T>(ref T packedColor, T green) where T : INumber<T>
	{
		if (!HasGreen)
		{
			return;
		}

		ulong packedValue = ulong.CreateChecked(packedColor);
		ulong componentValue = ulong.CreateChecked(green);

		// Clear the previous value for this component
		packedValue &= ~GreenMask;

		// Ensure component value is within mask range
		componentValue = Math.Min(componentValue, GreenMaxValue);

		// Set the new value
		packedValue |= (componentValue << GreenShift);
		packedColor = T.CreateChecked(packedValue);
	}

	/// <summary>
	/// Sets the blue component in a packed color value using this mask.
	/// </summary>
	/// <typeparam name="T">The numeric type of the packed color</typeparam>
	/// <param name="packedColor">The packed color value to modify</param>
	/// <param name="blue">The blue component value to set</param>
	public void SetBlue<T>(ref T packedColor, T blue) where T : INumber<T>
	{
		if (!HasBlue)
		{
			return;
		}

		ulong packedValue = ulong.CreateChecked(packedColor);
		ulong componentValue = ulong.CreateChecked(blue);

		// Clear the previous value for this component
		packedValue &= ~BlueMask;

		// Ensure component value is within mask range
		componentValue = Math.Min(componentValue, BlueMaxValue);

		// Set the new value
		packedValue |= (componentValue << BlueShift);
		packedColor = T.CreateChecked(packedValue);
	}

	/// <summary>
	/// Sets the alpha component in a packed color value using this mask.
	/// </summary>
	/// <typeparam name="T">The numeric type of the packed color</typeparam>
	/// <param name="packedColor">The packed color value to modify</param>
	/// <param name="alpha">The alpha component value to set</param>
	public void SetAlpha<T>(ref T packedColor, T alpha) where T : INumber<T>
	{
		if (!HasAlpha)
		{
			return;
		}

		ulong packedValue = ulong.CreateChecked(packedColor);
		ulong componentValue = ulong.CreateChecked(alpha);

		// Clear the previous value for this component
		packedValue &= ~AlphaMask;

		// Ensure component value is within mask range
		componentValue = Math.Min(componentValue, AlphaMaxValue);

		// Set the new value
		packedValue |= (componentValue << AlphaShift);
		packedColor = T.CreateChecked(packedValue);
	}

	/// <summary>
	/// Implicitly converts a string to a RgbaMask.
	/// </summary>
	/// <param name="maskString">A string in the format of component letter followed by bit count (e.g., "R8G8B8A8")</param>
	public static implicit operator PackedRgbaMask(string maskString) => new PackedRgbaMask(maskString);
}
