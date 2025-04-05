namespace BCnEncoder.TextureFormats;

using System.Collections.Generic;
using BCnEncoder.Shared;

public enum D3DFormat : uint {
    D3DFormatUnknown              =  0,

    D3DFormatR8G8B8               = 20,
    D3DFormatA8R8G8B8             = 21,
    D3DFormatX8R8G8B8             = 22,
    D3DFormatR5G6B5               = 23,
    D3DFormatX1R5G5B5             = 24,
    D3DFormatA1R5G5B5             = 25,
    D3DFormatA4R4G4B4             = 26,
    D3DFormatR3G3B2               = 27,
    D3DFormatA8                   = 28,
    D3DFormatA8R3G3B2             = 29,
    D3DFormatX4R4G4B4             = 30,
    D3DFormatA2B10G10R10          = 31,
    D3DFormatA8B8G8R8             = 32,
    D3DFormatX8B8G8R8             = 33,
    D3DFormatG16R16               = 34,
    D3DFormatA2R10G10B10          = 35,
    D3DFormatA16B16G16R16         = 36,

    D3DFormatA8P8                 = 40,
    D3DFormatP8                   = 41,

    D3DFormatL8                   = 50,
    D3DFormatA8L8                 = 51,
    D3DFormatA4L4                 = 52,

    D3DFormatV8U8                 = 60,
    D3DFormatL6V5U5               = 61,
    D3DFormatX8L8V8U8             = 62,
    D3DFormatQ8W8V8U8             = 63,
    D3DFormatV16U16               = 64,
    D3DFormatA2W10V10U10          = 67,

    // FourCC formats
    D3DFormatUYVY                 = ('U' | ('Y' << 8) | ('V' << 16) | ('Y' << 24)),
    D3DFormatR8G8_B8G8            = ('R' | ('G' << 8) | ('B' << 16) | ('G' << 24)),
    D3DFormatYUY2                 = ('Y' | ('U' << 8) | ('Y' << 16) | ('2' << 24)),
    D3DFormatG8R8_G8B8            = ('G' | ('R' << 8) | ('G' << 16) | ('B' << 24)),
    D3DFormatDXT1                 = ('D' | ('X' << 8) | ('T' << 16) | ('1' << 24)),
    D3DFormatDXT2                 = ('D' | ('X' << 8) | ('T' << 16) | ('2' << 24)),
    D3DFormatDXT3                 = ('D' | ('X' << 8) | ('T' << 16) | ('3' << 24)),
    D3DFormatDXT4                 = ('D' | ('X' << 8) | ('T' << 16) | ('4' << 24)),
    D3DFormatDXT5                 = ('D' | ('X' << 8) | ('T' << 16) | ('5' << 24)),

    D3DFormatD16Lockable          = 70,
    D3DFormatD32                  = 71,
    D3DFormatD15S1                = 73,
    D3DFormatD24S8                = 75,
    D3DFormatD24X8                = 77,
    D3DFormatD24X4S4              = 79,
    D3DFormatD16                  = 80,

    D3DFormatD32FLockable         = 82,
    D3DFormatD24FS8               = 83,

    D3DFormatL16                  = 81,

    D3DFormatVertexData           = 100,
    D3DFormatIndex16              = 101,
    D3DFormatIndex32              = 102,

    D3DFormatQ16W16V16U16         = 110,

    D3DFormatR16F                 = 111,
    D3DFormatG16R16F              = 112,
    D3DFormatA16B16G16R16F        = 113,

    D3DFormatR32F                 = 114,
    D3DFormatG32R32F              = 115,
    D3DFormatA32B32G32R32F        = 116,

    D3DFormatCxV8U8               = 117,
}

public static class D3DFormatExtensions
{
    /// <summary>
    /// Convert a CompressionFormat to D3DFormat
    /// </summary>
    public static D3DFormat ToD3DFormat(this CompressionFormat format) =>
        FormatMapping.TryGetValue(format, out var d3dFormat) ? d3dFormat : D3DFormat.D3DFormatUnknown;

    /// <summary>
    /// Convert a D3DFormat to CompressionFormat
    /// </summary>
    public static CompressionFormat ToCompressionFormat(this D3DFormat format) =>
        FormatMappingReverse.TryGetValue(format, out var bcnFormat) ? bcnFormat : CompressionFormat.Unknown;

    private static Dictionary<CompressionFormat, D3DFormat> FormatMapping { get; } =
        new() {
            // Red-only formats
            { CompressionFormat.R8, D3DFormat.D3DFormatL8 },              // L8 is closest match for R8
            { CompressionFormat.R16, D3DFormat.D3DFormatL16 },             // L16 is closest match for R16
            { CompressionFormat.R16F, D3DFormat.D3DFormatR16F },           // Direct match
            { CompressionFormat.R32F, D3DFormat.D3DFormatR32F },           // Direct match

            // Red-green formats
            { CompressionFormat.R8G8, D3DFormat.D3DFormatA8L8 },           // A8L8 is closest for R8G8
            { CompressionFormat.R16G16, D3DFormat.D3DFormatG16R16 },       // G16R16 in D3D9 has memory order RG
            { CompressionFormat.R16G16F, D3DFormat.D3DFormatG16R16F },     // G16R16F in D3D9 has memory order RG
            { CompressionFormat.R32G32F, D3DFormat.D3DFormatG32R32F },     // G32R32F in D3D9 has memory order RG

            // RGBA formats
            { CompressionFormat.R10G10B10A2_Packed, D3DFormat.D3DFormatA2B10G10R10 }, // Note component order
            { CompressionFormat.Rgba32, D3DFormat.D3DFormatA8B8G8R8 },     // A8B8G8R8 in D3D9 has memory order RGBA
            { CompressionFormat.Rgba32_sRGB, D3DFormat.D3DFormatA8B8G8R8 }, // D3D9 doesn't have sRGB explicitly
            { CompressionFormat.RgbaFloat, D3DFormat.D3DFormatA32B32G32R32F },
            { CompressionFormat.RgbaHalf, D3DFormat.D3DFormatA16B16G16R16F },

            // BGR formats
            { CompressionFormat.Bgr24, D3DFormat.D3DFormatR8G8B8 },
            { CompressionFormat.Bgr24_sRGB, D3DFormat.D3DFormatR8G8B8 },
            { CompressionFormat.B5G6R5_Packed, D3DFormat.D3DFormatR5G6B5 },        // D3D9 RGB order is BGR in memory
            { CompressionFormat.Bgr24x8, D3DFormat.D3DFormatX8R8G8B8 },
            { CompressionFormat.Bgr24x8_sRGB, D3DFormat.D3DFormatX8R8G8B8 },

            // BGRA formats
            { CompressionFormat.B5G5R5A1_Packed, D3DFormat.D3DFormatA1R5G5B5 },    // D3D9 BGRA order is ARGB in memory (perfect match)
            { CompressionFormat.B4G4R4A4_Packed, D3DFormat.D3DFormatA4R4G4B4 },    // D3D9 BGRA order is ARGB in memory (perfect match)
            { CompressionFormat.Bgra32, D3DFormat.D3DFormatA8R8G8B8 },     // A8R8G8B8 in D3D9 has memory order BGRA
            { CompressionFormat.Bgra32_sRGB, D3DFormat.D3DFormatA8R8G8B8 },

            // BC formats
            { CompressionFormat.Bc1, D3DFormat.D3DFormatDXT1 },
            { CompressionFormat.Bc1_sRGB, D3DFormat.D3DFormatDXT1 },        // D3D9 doesn't distinguish sRGB variants
            { CompressionFormat.Bc1WithAlpha, D3DFormat.D3DFormatDXT1 },
            { CompressionFormat.Bc1WithAlpha_sRGB, D3DFormat.D3DFormatDXT1 },
            { CompressionFormat.Bc2, D3DFormat.D3DFormatDXT3 },            // DXT3 = BC2
            { CompressionFormat.Bc2_sRGB, D3DFormat.D3DFormatDXT3 },
            { CompressionFormat.Bc3, D3DFormat.D3DFormatDXT5 },            // DXT5 = BC3
            { CompressionFormat.Bc3_sRGB, D3DFormat.D3DFormatDXT5 },

            // No D3D9 equivalents for the newer BC formats
            // BC4, BC5, BC6, BC7 were introduced with Direct3D 10
        };

    private static Dictionary<D3DFormat, CompressionFormat> FormatMappingReverse { get; } =
        new() {
            // Red-only formats
            { D3DFormat.D3DFormatL8, CompressionFormat.R8 },
            { D3DFormat.D3DFormatL16, CompressionFormat.R16 },
            { D3DFormat.D3DFormatR16F, CompressionFormat.R16F },
            { D3DFormat.D3DFormatR32F, CompressionFormat.R32F },

            // Red-green formats
            { D3DFormat.D3DFormatA8L8, CompressionFormat.R8G8 },
            { D3DFormat.D3DFormatG16R16, CompressionFormat.R16G16 },
            { D3DFormat.D3DFormatG16R16F, CompressionFormat.R16G16F },
            { D3DFormat.D3DFormatG32R32F, CompressionFormat.R32G32F },

            // RGBA formats
            { D3DFormat.D3DFormatA2B10G10R10, CompressionFormat.R10G10B10A2_Packed },
            { D3DFormat.D3DFormatA8B8G8R8, CompressionFormat.Rgba32 },
            { D3DFormat.D3DFormatA32B32G32R32F, CompressionFormat.RgbaFloat },
            { D3DFormat.D3DFormatA16B16G16R16F, CompressionFormat.RgbaHalf },

            // BGR formats
            { D3DFormat.D3DFormatR8G8B8, CompressionFormat.Bgr24 },
            { D3DFormat.D3DFormatR5G6B5, CompressionFormat.B5G6R5_Packed },
            { D3DFormat.D3DFormatX8R8G8B8, CompressionFormat.Bgr24x8 },

            // BGRA formats
            { D3DFormat.D3DFormatA1R5G5B5, CompressionFormat.B5G5R5A1_Packed },
            { D3DFormat.D3DFormatA4R4G4B4, CompressionFormat.B4G4R4A4_Packed },
            { D3DFormat.D3DFormatA8R8G8B8, CompressionFormat.Bgra32 },

            // BC formats
            { D3DFormat.D3DFormatDXT1, CompressionFormat.Bc1 },
            { D3DFormat.D3DFormatDXT3, CompressionFormat.Bc2 },
            { D3DFormat.D3DFormatDXT5, CompressionFormat.Bc3 }
        };
}
