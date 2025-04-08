using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using BCnEncoder.Shared.Colors;
using Xunit;
using Xunit.Abstractions;

namespace BCnEncTests.Colors
{
    /// <summary>
    /// Tests for all raw color types, except for shared exponent types.
    /// </summary>
    public class ColorTests
    {
	    private readonly ITestOutputHelper testOutputHelper;

	    public ColorTests(ITestOutputHelper testOutputHelper)
	    {
		    this.testOutputHelper = testOutputHelper;
	    }

        #region Type Info and Test Data

	    /// <summary>
        /// Defines the format type of a color.
        /// </summary>
        public enum ColorFormatType
        {
            Unorm,
            Snorm,
            Float
        }

        /// <summary>
        /// Stores metadata about a color type for testing purposes.
        /// </summary>
        /// <typeparam name="TColor">The color type.</typeparam>
        /// <typeparam name="TRaw">The raw component type.</typeparam>
        public class ColorTypeInfo<TColor, TRaw>
            where TColor : unmanaged, IColor<TColor>
            where TRaw : unmanaged, INumber<TRaw>
        {
	        public class ChannelInfo
	        {
		        /// <summary>The number of bits in the color type's components.</summary>
		        public int BitCount { get; init; }
		        /// <summary>The maximum raw value for the color type's components.</summary>
		        public TRaw MaxValue { get; init; }

		        /// <summary>The minimum raw value for the color type's components (for signed types).</summary>
		        public TRaw MinValue { get; init; }

		        /// <summary>Tolerance value to use for floating-point comparisons.</summary>
		        public float Tolerance { get; init; }
	        }

            /// <summary>An instance of the color type.</summary>
            public TColor Instance { get; }

            /// <summary>The format type of the color.</summary>
            public ColorFormatType FormatType { get; }

            /// <summary>The number of channels in the color type.</summary>
            public int ChannelCount { get; }

            public ChannelInfo[] Channels { get; }

            public string Name => typeof(TColor).Name;


            /// <summary>
            /// Creates a new ColorTypeInfo instance.
            /// </summary>
            public ColorTypeInfo(
                TColor instance,
                ColorFormatType formatType,
                ChannelInfo[] channelInfos)
            {
                Instance = instance;
                FormatType = formatType;
                ChannelCount = channelInfos.Length;
                Channels = channelInfos;
            }

            public override string ToString()
            {
	            return $"{nameof(Name)}: {Name}, {nameof(FormatType)}: {FormatType}, {nameof(ChannelCount)}: {ChannelCount}";
            }
        }

        /// <summary>
        /// Gets all color types for testing, grouped by format and channel count.
        /// </summary>
        public static IEnumerable<object[]> GetAllColorTypes()
        {
            // Single-channel unorm types
            yield return CreateUnormInfo<ColorR8, byte>([8]);
            yield return CreateUnormInfo<ColorR16, ushort>([16]);

            // Single-channel snorm types
            yield return CreateSnormInfo<ColorR8S, sbyte>([8]);
            yield return CreateSnormInfo<ColorR16S, short>([16]);

            // Single-channel float types
            yield return CreateFloatInfo<ColorR16F, Half>(1, Half.MaxValue, Half.MinValue);
            yield return CreateFloatInfo<ColorR32F, float>(1, float.MaxValue, float.MinValue);

            // Dual-channel types
            yield return CreateUnormInfo<ColorR8G8, byte>([8, 8]);
            yield return CreateSnormInfo<ColorR8G8S, sbyte>([8, 8]);
            yield return CreateUnormInfo<ColorR16G16, ushort>([16, 16]);
            yield return CreateSnormInfo<ColorR16G16S, short>([16, 16]);
            yield return CreateFloatInfo<ColorR16G16F, Half>(2, Half.MaxValue, Half.MinValue);
            yield return CreateFloatInfo<ColorR32G32F, float>(2, float.MaxValue, float.MinValue);

            // Triple-channel (RGB) unorm types
            yield return CreateUnormInfo<ColorB5G6R5Packed, ushort>([5, 6, 5]);
            yield return CreateUnormInfo<ColorR5G6B5Packed, ushort>([5, 6, 5]);
            yield return CreateUnormInfo<ColorB5G5R5M1Packed, ushort>([5, 5, 5]);
			yield return CreateUnormInfo<ColorRgb24, byte>([8, 8, 8]);
			yield return CreateUnormInfo<ColorBgr24, byte>([8, 8, 8]);
            yield return CreateFloatInfo<ColorRgbHalf, Half>(3, Half.MaxValue, Half.MinValue, tolerance: 0.0002f);
            yield return CreateFloatInfo<ColorRgbFloat, float>(3, float.MaxValue, float.MinValue);

            // Quad-channel (RGBA) unorm types with 4 bits per channel
            yield return CreateUnormInfo<ColorB4G4R4A4Packed, ushort>([4, 4, 4, 4]);
            yield return CreateUnormInfo<ColorR4G4B4A4Packed, ushort>([4, 4, 4, 4]);
            yield return CreateUnormInfo<ColorA4B4G4R4Packed, ushort>([4, 4, 4, 4]);
            yield return CreateUnormInfo<ColorA4R4G4B4Packed, ushort>([4, 4, 4, 4]);

            // Quad-channel (RGBA) unorm types with 5-5-5-1 bit patterns
            yield return CreateUnormInfo<ColorA1B5G5R5Packed, ushort>([5, 5, 5, 1]);
            yield return CreateUnormInfo<ColorA1R5G5B5Packed, ushort>([5, 5, 5, 1]);
            yield return CreateUnormInfo<ColorB5G5R5A1Packed, ushort>([5, 5, 5, 1]);
            yield return CreateUnormInfo<ColorR5G5B5A1Packed, ushort>([5, 5, 5, 1]);

            // Quad-channel (RGBA) unorm types with 8 bits per channel
            yield return CreateUnormInfo<ColorRgba32, byte>([8, 8, 8, 8]);
            yield return CreateUnormInfo<ColorBgra32, byte>([8, 8, 8, 8]);

            // Quad-channel (RGBA) unorm types with 10-10-10-2 bit pattern
            yield return CreateUnormInfo<ColorR10G10B10A2Packed, uint>([10, 10, 10, 2]);
            yield return CreateUnormInfo<ColorB10G10R10A2Packed, uint>([10, 10, 10, 2]);

            // Quad-channel (RGBA) float types
            yield return CreateFloatInfo<ColorRgbaHalf, Half>(4, Half.MaxValue, Half.MinValue, tolerance: 0.0002f);
            yield return CreateFloatInfo<ColorRgbaFloat, float>(4, float.MaxValue, float.MinValue);
        }

        public static IEnumerable<object[]> GetAllPackedColorTypes()
        {
	        foreach (var colorType in GetAllColorTypes())
	        {
		        var interfaceType = colorType[0].GetType().GetGenericArguments()[0] // From ColorTypeInfo<TColor, TRaw>
			        .GetInterfaces()
			        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IColorPacked<>));

		        if (interfaceType != null)
		        {
			        var packedType = interfaceType.GetGenericArguments()[0];
			        object packedInstance = Activator.CreateInstance(packedType);

			        yield return [colorType[0], packedInstance];
		        }
	        }
        }

        /// <summary>
        /// Helper method to create ColorTypeInfo for unsigned normalized color types.
        /// </summary>
        private static object[] CreateUnormInfo<TColor, TRaw>(int[] channelBits)
            where TColor : unmanaged, IColor<TColor>
            where TRaw : unmanaged, INumber<TRaw>
        {
	        ColorTypeInfo<TColor, TRaw>.ChannelInfo[] channels = new ColorTypeInfo<TColor, TRaw>.ChannelInfo[channelBits.Length];

	        for (int i = 0; i < channelBits.Length; i++)
	        {
		        TRaw maxValue = TRaw.CreateChecked((1 << channelBits[i]) - 1);
		        TRaw minValue = TRaw.Zero;
		        float rangeFloat = float.CreateChecked(long.CreateChecked(maxValue) - long.CreateChecked(minValue));

		        channels[i] = new ColorTypeInfo<TColor,TRaw>.ChannelInfo()
		        {
			        BitCount = channelBits[i],
			        MaxValue = maxValue,
			        MinValue = minValue,
			        Tolerance = 1 / rangeFloat,
		        };
	        }

	        return [new ColorTypeInfo<TColor, TRaw>(
		        new TColor(),
		        ColorFormatType.Unorm,
		        channels
	        )];
        }

        /// <summary>
        /// Helper method to create ColorTypeInfo for signed normalized color types.
        /// </summary>
        private static object[] CreateSnormInfo<TColor, TRaw>(int[] channelBits)
            where TColor : unmanaged, IColor<TColor>
            where TRaw : unmanaged, INumber<TRaw>
        {
            ColorTypeInfo<TColor, TRaw>.ChannelInfo[] channels = new ColorTypeInfo<TColor, TRaw>.ChannelInfo[channelBits.Length];

            for (int i = 0; i < channelBits.Length; i++)
            {
                // For SNORM, max is 2^(bits-1)-1, min is -max
                TRaw maxValue = TRaw.CreateChecked((1 << (channelBits[i] - 1)) - 1);
                TRaw minValue = TRaw.CreateChecked(-maxValue);
                float rangeFloat = float.CreateChecked(long.CreateChecked(maxValue) - long.CreateChecked(minValue));

                channels[i] = new ColorTypeInfo<TColor, TRaw>.ChannelInfo()
                {
	                BitCount = channelBits[i],
                    MaxValue = maxValue,
                    MinValue = minValue,
                    Tolerance = 1 / rangeFloat,
                };
            }

            return [new ColorTypeInfo<TColor, TRaw>(
                new TColor(),
                ColorFormatType.Snorm,
                channels
            )];
        }

        /// <summary>
        /// Helper method to create ColorTypeInfo for floating point color types.
        /// </summary>
        private static object[] CreateFloatInfo<TColor, TRaw>(int numChannels, TRaw maxValue, TRaw minValue, float tolerance = 0.0001f)
            where TColor : unmanaged, IColor<TColor>
            where TRaw : unmanaged, INumber<TRaw>
        {
            ColorTypeInfo<TColor, TRaw>.ChannelInfo[] channels = new ColorTypeInfo<TColor, TRaw>.ChannelInfo[numChannels];

            var bitCount = 8 * Unsafe.SizeOf<TRaw>();

            for (int i = 0; i < numChannels; i++)
            {
                channels[i] = new ColorTypeInfo<TColor, TRaw>.ChannelInfo()
                {
	                BitCount = bitCount,
                    MaxValue = maxValue,
                    MinValue = minValue,
                    Tolerance = tolerance,
                };
            }

            return [new ColorTypeInfo<TColor, TRaw>(
                new TColor(),
                ColorFormatType.Float,
                channels
            )];
        }

        #endregion

        #region Component Test Helpers

        /// <summary>
        /// Defines test operations for a specific component of a color type.
        /// </summary>
        /// <typeparam name="TColor">The color type.</typeparam>
        /// <typeparam name="TRaw">The raw component type.</typeparam>
        private class ComponentTestHelper<TColor, TRaw>(
	        string componentName,
	        ColorTypeInfo<TColor, TRaw>.ChannelInfo channelInfo,
	        Func<TColor, float> getFloatComponent,
	        Func<TColor, float, TColor> setFloatComponent,
	        Func<TColor, TRaw> getRawComponent,
	        Func<TColor, TRaw, TColor> setRawComponent)
	        where TColor : unmanaged, IColor<TColor>
	        where TRaw : unmanaged, INumber<TRaw>
        {
	        public string ComponentName => componentName;
	        public ColorTypeInfo<TColor, TRaw>.ChannelInfo ChannelInfo => channelInfo;

	        public float GetValue(TColor color) => getFloatComponent(color);

            public TColor SetValue(TColor color, float value) => setFloatComponent(color, value);

            public TRaw GetRawValue(TColor color) => getRawComponent(color);

            public TColor SetRawValue(TColor color, TRaw value) => setRawComponent(color, value);
        }

        private IEnumerable<ComponentTestHelper<TColor, TRaw>> GetComponents<TColor, TRaw>(ColorTypeInfo<TColor, TRaw> typeInfo)
            where TColor : unmanaged, IColor<TColor>
            where TRaw : unmanaged, INumber<TRaw>
        {
            if (typeof(IColorRed<TColor, TRaw>).IsAssignableFrom(typeof(TColor)))
            {
                yield return new ComponentTestHelper<TColor, TRaw>(
                    "R",
					typeInfo.Channels[0],
                    color => ((IColorRed<TColor, TRaw>)color).R,
                    (color, value) =>
                    {
	                    var c = (IColorRed<TColor, TRaw>)color;
	                    c.R = value;
	                    return (TColor)c;
                    },
                    color => ((IColorRed<TColor, TRaw>)color).RawR,
                    (color, value) => {
                        var c = (IColorRed<TColor, TRaw>)color;
                        c.RawR = value;
                        return (TColor)c;
                    }
                );
            }

            if (typeof(IColorRedGreen<TColor, TRaw>).IsAssignableFrom(typeof(TColor)))
            {
                yield return new ComponentTestHelper<TColor, TRaw>(
                    "G",
                    typeInfo.Channels[1],
                    color => ((IColorRedGreen<TColor, TRaw>)color).G,
                    (color, value) =>
                    {
	                    var c = (IColorRedGreen<TColor, TRaw>)color;
	                    c.G = value;
	                    return (TColor)c;
                    },
                    color => ((IColorRedGreen<TColor, TRaw>)color).RawG,
                    (color, value) => {
                        var c = (IColorRedGreen<TColor, TRaw>)color;
                        c.RawG = value;
                        return (TColor)c;
                    }
                );
            }

            if (typeof(IColorRgb<TColor, TRaw>).IsAssignableFrom(typeof(TColor)))
            {
                yield return new ComponentTestHelper<TColor, TRaw>(
                    "B",
                    typeInfo.Channels[2],
                    color => ((IColorRgb<TColor, TRaw>)color).B,
                    (color, value) =>
                    {
	                    var c = (IColorRgb<TColor, TRaw>)color;
	                    c.B = value;
	                    return (TColor)c;
                    },
                    color => ((IColorRgb<TColor, TRaw>)color).RawB,
                    (color, value) => {
                        var c = (IColorRgb<TColor, TRaw>)color;
                        c.RawB = value;
                        return (TColor)c;
                    }
                );
            }

            if (typeof(IColorRgba<TColor, TRaw>).IsAssignableFrom(typeof(TColor)))
            {
                yield return new ComponentTestHelper<TColor, TRaw>(
                    "A",
                    typeInfo.Channels[3],
                    color => ((IColorRgba<TColor, TRaw>)color).A,
                    (color, value) =>
                    {
	                    var c = (IColorRgba<TColor, TRaw>)color;
	                    c.A = value;
	                    return (TColor)c;
                    },
                    color => ((IColorRgba<TColor, TRaw>)color).RawA,
                    (color, value) => {
                        var c = (IColorRgba<TColor, TRaw>)color;
                        c.RawA = value;
                        return (TColor)c;
                    }
                );
            }
        }

        #endregion

        [Theory]
        [MemberData(nameof(GetAllColorTypes))]
        public void TestComponentGetters<TColor, TRaw>(ColorTypeInfo<TColor, TRaw> typeInfo)
	        where TColor : unmanaged, IColor<TColor>, IEquatable<TColor>
	        where TRaw : unmanaged, INumber<TRaw>
        {
	        var components = GetComponents(typeInfo).ToArray();

	        Assert.True(components.Length > 0, "Color type must have at least one component.");
	        Assert.Equal(typeInfo.ChannelCount, components.Length);
        }

        [Theory]
        [MemberData(nameof(GetAllColorTypes))]
        public void TestSize<TColor, TRaw>(ColorTypeInfo<TColor, TRaw> typeInfo)
	        where TColor : unmanaged, IColor<TColor>, IEquatable<TColor>
	        where TRaw : unmanaged, INumber<TRaw>
        {
	        var components = GetComponents(typeInfo).ToArray();

	        var expectedSize = (components.Sum(c => c.ChannelInfo.BitCount) + 7) / 8;

	        Assert.Equal(expectedSize, Unsafe.SizeOf<TColor>());
        }

        [Theory]
        [MemberData(nameof(GetAllColorTypes))]
        public void TestName<TColor, TRaw>(ColorTypeInfo<TColor, TRaw> typeInfo)
	        where TColor : unmanaged, IColor<TColor>, IEquatable<TColor>
	        where TRaw : unmanaged, INumber<TRaw>
        {
	        var components = GetComponents(typeInfo).ToArray();

	        // Pattern to match color channels from the type name.
	        // E.g. "ColorR10G10B10A2Packed" will match "R", "G", "B", and "A".
	        // "ColorRgb24 will match "Rgb"
	        // Etc.
	        Regex regex = new Regex(@"(Rgba|Rgb|Bgra|Bgr|R|G|B|A(?=\d|Float|Half))", RegexOptions.Compiled); // Match both combined channel names and individual channels

	        List<char> channels = new List<char>();

	        var matches = regex.Matches(typeInfo.Name);

	        foreach (Match match in matches)
	        {
		        foreach (char c in match.Value)
		        {
			        channels.Add(Char.ToUpperInvariant(c));
		        }
	        }

	        Assert.True(channels.Count > 0, "Color type must have at least one component.");
	        Assert.Equivalent(channels, components.Select(c => c.ComponentName[0]), true);
        }

        [Theory]
        [MemberData(nameof(GetAllColorTypes))]
        public void TestValueClamping<TColor, TRaw>(ColorTypeInfo<TColor, TRaw> typeInfo)
            where TColor : unmanaged, IColor<TColor>, IEquatable<TColor>
            where TRaw : unmanaged, INumber<TRaw>
        {
	        bool isFloatType = typeInfo.FormatType == ColorFormatType.Float;

            // For each component in the color type
            foreach (var component in GetComponents(typeInfo))
            {
	            var channelInfo = component.ChannelInfo;

                // Test clamping of values > 1.0 to 1.0
                TColor colorHigh = new TColor();
                const float highValue = 2f;

                float expectedHigh = typeInfo.FormatType switch
                {
	                ColorFormatType.Unorm => 1.0f,
	                ColorFormatType.Snorm => 1.0f,
	                ColorFormatType.Float => highValue
                };

                colorHigh = component.SetValue(colorHigh, highValue);

                // Check that high float value is properly clamped to 1.0
                Assert.Equal(expectedHigh, component.GetValue(colorHigh), channelInfo.Tolerance);

                // Don't check raw value for float types
                if (!isFloatType)
                {
	                Assert.Equal(channelInfo.MaxValue, component.GetRawValue(colorHigh));
                }

                // Test clamping of values < 0.0 to 0.0
                TColor colorLow = new TColor();
                const float lowValue = -2f;

				colorLow = component.SetValue(colorLow, lowValue);

				float expected = typeInfo.FormatType switch
				{
					ColorFormatType.Unorm => 0.0f,
					ColorFormatType.Snorm => -1.0f,
					ColorFormatType.Float => lowValue
				};

				Assert.Equal(expected, component.GetValue(colorLow), channelInfo.Tolerance);

				// Don't check raw value for float types
				if (!isFloatType)
				{
					Assert.Equal(channelInfo.MinValue, component.GetRawValue(colorLow));
				}
            }
        }

        [Theory]
        [MemberData(nameof(GetAllColorTypes))]
        public void TestValueRounding<TColor, TRaw>(ColorTypeInfo<TColor, TRaw> typeInfo)
            where TColor : unmanaged, IColor<TColor>, IEquatable<TColor>
            where TRaw : unmanaged, INumber<TRaw>
        {
	        if (typeInfo.FormatType == ColorFormatType.Float)
		        return;

            // For each component in the color type
            foreach (var component in GetComponents(typeInfo))
            {
	            var channelInfo = component.ChannelInfo;

	            double start = typeInfo.FormatType == ColorFormatType.Unorm ? 0.0 : -1.0;
	            double interval = typeInfo.FormatType == ColorFormatType.Unorm ?
		            1.0 / ((1 << channelInfo.BitCount) - 1) :
		            1.0 / ((1 << (channelInfo.BitCount - 1)) - 1);

	            for (double value = start; value <= 1.0f; value += interval)
	            {
		            TColor color = new TColor();
		            color = component.SetValue(color, (float)value);
		            Assert.Equal(value, component.GetValue(color), 0.000001);

		            TColor colorPlus = new TColor();

		            colorPlus = component.SetValue(colorPlus, (float)(value + interval * 0.49));
		            Assert.Equal(value, component.GetValue(colorPlus), 0.000001);

		            TColor colorMinus = new TColor();

		            colorMinus = component.SetValue(colorMinus, (float)(value - interval * 0.49));
		            Assert.Equal(value, component.GetValue(colorMinus), 0.000001);
	            }
            }
        }

        /// <summary>
        /// Test component accessors for all color types.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetAllColorTypes))]
        public void TestComponentAccessors<TColor, TRaw>(ColorTypeInfo<TColor, TRaw> typeInfo)
            where TColor : unmanaged, IColor<TColor>, IEquatable<TColor>
            where TRaw : unmanaged, INumber<TRaw>
        {
	        ColorRgbaFloat expectedColor = new ColorRgbaFloat(0.2f, 0.6f, 0.7f, 0.8f);

	        TColor color = new TColor();

            foreach (var componentTest in GetComponents(typeInfo))
            {
	            float valueToSet = componentTest.ComponentName switch
	            {
		            "R" => expectedColor.R,
		            "G" => expectedColor.G,
		            "B" => expectedColor.B,
		            "A" => expectedColor.A
	            };

				color = componentTest.SetValue(color, valueToSet);
            }

            foreach (var componentTest in GetComponents(typeInfo))
            {
	            float valueToTest = componentTest.ComponentName switch
	            {
		            "R" => expectedColor.R,
		            "G" => expectedColor.G,
		            "B" => expectedColor.B,
		            "A" => expectedColor.A
	            };

	            Assert.Equal(valueToTest, componentTest.GetValue(color), componentTest.ChannelInfo.Tolerance);
            }
        }

        /// <summary>
        /// Tests conversion to and from ColorRgbaFloat for all color types.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetAllColorTypes))]
        public void TestConversionToAndFromRgbaFloat<TColor, TRaw>(ColorTypeInfo<TColor, TRaw> typeInfo)
            where TColor : unmanaged, IColor<TColor>, IEquatable<TColor>
            where TRaw : unmanaged, INumber<TRaw>
        {
            testOutputHelper.WriteLine($"Testing conversion to/from ColorRgbaFloat for {typeInfo.Name}");

            // Create a color with values in each available component
            TColor originalColor = new TColor();

            int i = 0;
            // For each component, set a value based on format type
            foreach (var componentTest in GetComponents(typeInfo))
            {
                float componentValue = 0.0f;

                switch (typeInfo.FormatType)
                {
                    case ColorFormatType.Unorm:
                        componentValue = 0.5f + (i * .1f); // Use 0.5 + (i * .1) for unorm
                        break;
                    case ColorFormatType.Snorm:
                        componentValue = -0.5f + (i * .1f); // Use -0.5 + (i * .1) for snorm
                        break;
                    case ColorFormatType.Float:
                        componentValue = 1.25f + (i * .5f); // Use 1.25 + (i * .5) for float
                        break;
                }

                originalColor = componentTest.SetValue(originalColor, componentValue);

                i++;
            }

            // Convert to ColorRgbaFloat
            ColorRgbaFloat rgbaFloat = originalColor.ToColorRgbaFloat();

            // Log the conversion results
            testOutputHelper.WriteLine($"Original: {originalColor}, Converted to RGBA Float: {rgbaFloat}");

            // Convert back to original type
            TColor roundTripColor = new TColor();
            roundTripColor.FromColorRgbaFloat(rgbaFloat);

            // Check that the round-trip conversion preserves values accurately
            // For floating-point formats, we need exact equality
            if (typeInfo.FormatType == ColorFormatType.Float)
            {
                // For float formats, each component should match exactly within tolerance
                foreach (var componentTest in GetComponents(typeInfo))
                {
	                var channelInfo = componentTest.ChannelInfo;

                    float originalValue = componentTest.GetValue(originalColor);
                    float roundTripValue = componentTest.GetValue(roundTripColor);

                    testOutputHelper.WriteLine($"Component {componentTest.ComponentName}: Original={originalValue}, RoundTrip={roundTripValue}");
                    Assert.Equal(originalValue, roundTripValue, channelInfo.Tolerance);
                }
            }
            else
            {
                // For unorm/snorm formats, we can compare the entire struct for equality
                Assert.Equal(originalColor, roundTripColor);
            }
        }

        /// <summary>
        /// Tests equality operations and hash code consistency for all color types.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetAllColorTypes))]
        public void TestEqualityAndHashCode<TColor, TRaw>(ColorTypeInfo<TColor, TRaw> typeInfo)
            where TColor : unmanaged, IColor<TColor>, IEquatable<TColor>
            where TRaw : unmanaged, INumber<TRaw>
        {
            testOutputHelper.WriteLine($"Testing equality and hash code for {typeInfo.Name}");

            // Create two identical colors and one different color
            TColor color1 = new TColor();
            TColor color2 = new TColor();
            TColor colorDifferent = new TColor();
            TColor zeroColor = new TColor();

            // Set different component values based on the available components
            foreach (var component in GetComponents(typeInfo))
            {
	            TColor color1OneComponent = new TColor();
	            TColor color2OneComponent = new TColor();
	            TColor colorDifferentOneComponent = new TColor();

                float componentValue = 0.0f;

                switch (typeInfo.FormatType)
                {
                    case ColorFormatType.Unorm:
                        componentValue = 0.75f; // Use 0.75 for unorm
                        break;
                    case ColorFormatType.Snorm:
                        componentValue = -0.5f; // Use -0.5 for snorm
                        break;
                    case ColorFormatType.Float:
                        componentValue = 1.25f; // Use 1.25 for float
                        break;
                }

                // Set component values
                zeroColor = component.SetValue(zeroColor, 0.0f);
                color1 = component.SetValue(color1, componentValue);
                color2 = component.SetValue(color2, componentValue);
                color1OneComponent = component.SetValue(color1OneComponent, componentValue);
                color2OneComponent = component.SetValue(color2OneComponent, componentValue);

                // For colorDifferent, use 0.25
                colorDifferent = component.SetValue(color1, 0.25f);
                colorDifferentOneComponent = component.SetValue(colorDifferentOneComponent, 0.25f);

                // 1. IEquatable<T>.Equals
                Assert.True(color1OneComponent.Equals(color2OneComponent),
	                $"IEquatable.Equals should return true for identical colors. Color1: {color1OneComponent}, Color2: {color2OneComponent}");
                Assert.False(color1OneComponent.Equals(colorDifferentOneComponent),
	                $"IEquatable.Equals should return false for different colors. Color1: {color1OneComponent}, ColorDifferent: {colorDifferentOneComponent}");

                // 2. Object.Equals override
                Assert.True(color1OneComponent.Equals((object)color2OneComponent),
	                $"Object.Equals should return true for identical colors. Color1: {color1OneComponent}, Color2: {color2OneComponent}");
                Assert.False(color1OneComponent.Equals((object)colorDifferentOneComponent),
	                $"Object.Equals should return false for different colors. Color1: {color1OneComponent}, ColorDifferent: {colorDifferentOneComponent}");

                // 3. Hash code consistency
                Assert.Equal(color1OneComponent.GetHashCode(), color2OneComponent.GetHashCode());
                Assert.NotEqual(color1OneComponent.GetHashCode(), colorDifferentOneComponent.GetHashCode());
            }

            // Test equality methods

            // 1. IEquatable<T>.Equals
            Assert.True(color1.Equals(color2),
                $"IEquatable.Equals should return true for identical colors. Color1: {color1}, Color2: {color2}");
            Assert.False(color1.Equals(colorDifferent),
                $"IEquatable.Equals should return false for different colors. Color1: {color1}, ColorDifferent: {colorDifferent}");

            // 2. Object.Equals override
            Assert.True(color1.Equals((object)color2),
                $"Object.Equals should return true for identical colors. Color1: {color1}, Color2: {color2}");
            Assert.False(color1.Equals((object)colorDifferent),
                $"Object.Equals should return false for different colors. Color1: {color1}, ColorDifferent: {colorDifferent}");
            Assert.False(color1.Equals(null),
                "Object.Equals should return false for null");

            // 3. Hash code consistency
            Assert.Equal(color1.GetHashCode(), color2.GetHashCode());
            Assert.NotEqual(color1.GetHashCode(), colorDifferent.GetHashCode());

            // 4. Test with default color
            var defaultColor = default(TColor);

            // For colors default and zero-initialized should be equal
            Assert.Equal(defaultColor, zeroColor);
            Assert.Equal(defaultColor.GetHashCode(), zeroColor.GetHashCode());
        }

        /// <summary>
        /// Tests equality operations and hash code consistency for all color types.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetAllPackedColorTypes))]
        public void TestPackedColors<TColor, TRaw, TPacked>(ColorTypeInfo<TColor, TRaw> typeInfo, TPacked _)
	        where TColor : unmanaged, IColor<TColor>, IEquatable<TColor>, IColorPacked<TPacked>
	        where TRaw : unmanaged, INumber<TRaw>
	        where TPacked : unmanaged, INumber<TPacked>, IBinaryInteger<TPacked>
        {
	        testOutputHelper.WriteLine($"Testing packed color operations for {typeInfo.Name}");

	        PackedRgbaMask channelMask = TColor.ChannelMask;
	        // Check that channel mask is a subset of the color type name
	        Assert.Contains(channelMask.ToString(), typeInfo.Name, StringComparison.OrdinalIgnoreCase);

	        TColor colorFull = new TColor();

	        colorFull.Data = TPacked.CreateTruncating((ulong)0xFFFF_FFFF_FFFF_FFFF);

	        var components = GetComponents(typeInfo).ToArray();
	        var redComponent = components.FirstOrDefault(c => c.ComponentName == "R");
	        var greenComponent = components.FirstOrDefault(c => c.ComponentName == "G");
	        var blueComponent = components.FirstOrDefault(c => c.ComponentName == "B");
	        var alphaComponent = components.FirstOrDefault(c => c.ComponentName == "A");

	        // Should have at least red component
	        Assert.NotNull(redComponent);

	        Assert.Equal(channelMask.HasRed, redComponent != null);
	        Assert.Equal(channelMask.HasGreen, greenComponent != null);
	        Assert.Equal(channelMask.HasBlue, blueComponent != null);
	        Assert.Equal(channelMask.HasAlpha, alphaComponent != null);

	        if (channelMask.HasRed)
	        {
		        TColor fullNoRed = colorFull;
		        Assert.Equal(channelMask.RedMaxValue, ulong.CreateTruncating(redComponent.GetRawValue(fullNoRed)));
		        Assert.Equal(channelMask.GetRed(fullNoRed.Data), TPacked.CreateTruncating(redComponent.GetRawValue(fullNoRed)));

		        fullNoRed = redComponent.SetValue(fullNoRed, 0f);

		        TPacked rMask = TPacked.CreateTruncating(channelMask.RedMask);
		        TPacked nRMask = ~rMask;

		        Assert.Equal(0UL, ulong.CreateTruncating(fullNoRed.Data & rMask));
		        Assert.Equal(nRMask, fullNoRed.Data);
	        }
        }

    }
}
