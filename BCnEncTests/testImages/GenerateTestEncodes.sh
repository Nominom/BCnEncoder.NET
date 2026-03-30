#!/usr/bin/env bash
# GenerateTestEncodes.sh
# Generates test DDS and KTX texture files using cuttlefish, then decodes
# reference PNGs using tacentview.
# Replaces GenerateTestDds.ps1 and GenerateTestKtx.ps1.
#
# Notes:
#  - Only DX10-header DDS files are generated; cuttlefish does not support DX9/legacy headers.
#  - ETC/EAC/ASTC/PVRTC formats are KTX-only (not valid in DDS containers).
#  - tacentview is used for reference decoding; if not found the decode step is skipped.

set -euo pipefail

usage() {
    cat <<'EOF'
GenerateTestEncodes.sh - Generate test DDS and KTX files for BCnEncoder.NET

USAGE:
    ./GenerateTestEncodes.sh -i <input> [OPTIONS]
    ./GenerateTestEncodes.sh --no-encode [OPTIONS]

OPTIONS:
    -i, --input <path>       Path to input image (required unless --no-encode)
    -d, --dds-dir <path>     DDS directory (default: ./testdds)
    -k, --ktx-dir <path>     KTX directory (default: ./testktx)
        --no-encode          Skip encoding; only decode reference PNGs from
                             existing files in the DDS and KTX directories
    -h, --help               Show this help

TOOLS:
    cuttlefish    Required for encoding (not needed with --no-encode)
    tacentview    Required for reference PNG decoding
EOF
}

INPUT=""
DDS_DIR="./testdds"
KTX_DIR="./testktx"
NO_ENCODE=0

while [[ $# -gt 0 ]]; do
    case "$1" in
        -i|--input)   INPUT="$2";   shift 2 ;;
        -d|--dds-dir) DDS_DIR="$2"; shift 2 ;;
        -k|--ktx-dir) KTX_DIR="$2"; shift 2 ;;
        --no-encode)  NO_ENCODE=1;  shift ;;
        -h|--help)    usage; exit 0 ;;
        *) echo "Unknown option: $1" >&2; usage >&2; exit 1 ;;
    esac
done

if [[ "$NO_ENCODE" == "0" ]]; then
    [[ -z "$INPUT" ]] && { echo "Error: -i/--input is required" >&2; usage >&2; exit 1; }
    [[ ! -f "$INPUT" ]] && { echo "Error: Input file not found: $INPUT" >&2; exit 1; }
    command -v cuttlefish &>/dev/null || { echo "Error: cuttlefish not found in PATH" >&2; exit 1; }
fi

HAS_TACENTVIEW=0
command -v tacentview &>/dev/null && HAS_TACENTVIEW=1 \
    || echo "Warning: tacentview not found — reference PNG decode step will be skipped"

if [[ "$NO_ENCODE" == "0" ]]; then
    BASE_NAME="$(basename "${INPUT%.*}")"
    mkdir -p "$DDS_DIR" "$KTX_DIR"
fi

run_cf() {
    local output="$1"; shift
    echo "  cuttlefish$(printf ' %q' "$@") -o $output"
    if [[ -f "$output" ]]; then
        echo "  Skipped (already exists): $output"
        return
    fi
    if cuttlefish "$@" -o "$output"; then
        echo "  Created $output"
    else
        echo "  Warning: cuttlefish failed for $output" >&2
    fi
}

# encode <name> <output> <cf_format> <cf_type> [extra_flags...]
# cf_type may be empty when the format name implies its type (e.g. B10G11R11_UFloat)
encode() {
    local name="$1" output="$2" cf_fmt="$3" cf_type="$4"
    shift 4
    local flags=(-i "$INPUT" -f "$cf_fmt" -Q highest)
    [[ -n "$cf_type" ]] && flags+=(-t "$cf_type")
    flags+=("$@")
    run_cf "$output" "${flags[@]}"
}

# ─── Format tables ─────────────────────────────────────────────────────────────
# Fields: name | cuttlefish_format | cuttlefish_type
# cuttlefish_type is left empty when the format name implies its own data type
# (e.g. B10G11R11_UFloat) or when the cuttlefish default (unorm) is desired.

# Formats written to both DDS and KTX
COMMON_FORMATS=(
    # BC1
    "bc1-rgb|BC1_RGB|unorm"
    "bc1-rgba|BC1_RGBA|unorm"
    # BC2 (DXT3)
    "bc2|BC2|unorm"
    # BC3 (DXT5)
    "bc3|BC3|unorm"
    # BC4
    "bc4|BC4|unorm"
    "bc4-snorm|BC4|snorm"
    # BC5
    "bc5|BC5|unorm"
    "bc5-snorm|BC5|snorm"
    # BC6H
    "bc6h|BC6H|ufloat"
    "bc6h-signed|BC6H|float"
    # BC7
    "bc7|BC7|unorm"
    # Uncompressed 4-bit
    "r4g4|R4G4|unorm"
    "r4g4b4a4|R4G4B4A4|unorm"
    "b4g4r4a4|B4G4R4A4|unorm"
    "a4r4g4b4|A4R4G4B4|unorm"
    # Uncompressed 5/6-bit
    "r5g6b5|R5G6B5|unorm"
    "b5g6r5|B5G6R5|unorm"
    "r5g5b5a1|R5G5B5A1|unorm"
    "b5g5r5a1|B5G5R5A1|unorm"
    "a1r5g5b5|A1R5G5B5|unorm"
    # Uncompressed 8-bit
    "r8|R8|unorm"
    "r8-snorm|R8|snorm"
    "r8g8|R8G8|unorm"
    "r8g8-snorm|R8G8|snorm"
    "r8g8b8|R8G8B8|unorm"
    "b8g8r8|B8G8R8|unorm"
    "r8g8b8a8|R8G8B8A8|unorm"
    "b8g8r8a8|B8G8R8A8|unorm"
    "a8b8g8r8|A8B8G8R8|unorm"
    # Uncompressed 10-bit
    # A2R10G10B10 = DirectX R10G10B10A2 (same layout, different naming convention)
    # A2B10G10R10 = BGRA variant
    "a2r10g10b10|A2R10G10B10|unorm"
    "a2b10g10r10|A2B10G10R10|unorm"
    # Uncompressed 16-bit
    "r16|R16|unorm"
    "r16-float|R16|float"
    "r16g16|R16G16|unorm"
    "r16g16-float|R16G16|float"
    "r16g16b16|R16G16B16|unorm"
    "r16g16b16-float|R16G16B16|float"
    "r16g16b16a16|R16G16B16A16|unorm"
    "r16g16b16a16-float|R16G16B16A16|float"
    # Uncompressed 32-bit (float only — 32-bit unorm is not practical for textures)
    "r32-float|R32|float"
    "r32g32-float|R32G32|float"
    "r32g32b32-float|R32G32B32|float"
    "r32g32b32a32-float|R32G32B32A32|float"
    # Special float formats (type is inherent in the format name, no -t flag needed)
    "b10g11r11-ufloat|B10G11R11_UFloat|"
    "e5b9g9r9-ufloat|E5B9G9R9_UFloat|"
)

# Formats for KTX only — not valid in DDS containers
KTX_EXTRA_FORMATS=(
    # ETC1
    "etc1|ETC1|unorm"
    # ETC2
    "etc2-rgb|ETC2_R8G8B8|unorm"
    "etc2-rgba|ETC2_R8G8B8A8|unorm"
    "etc2-rgb-a1|ETC2_R8G8B8A1|unorm"
    # EAC (single- and dual-channel, signed and unsigned)
    "eac-r11|EAC_R11|unorm"
    "eac-r11-snorm|EAC_R11|snorm"
    "eac-rg11|EAC_R11G11|unorm"
    "eac-rg11-snorm|EAC_R11G11|snorm"
    # ASTC
    "astc-4x4|ASTC_4x4|"
    "astc-5x4|ASTC_5x4|"
    "astc-5x5|ASTC_5x5|"
    "astc-6x5|ASTC_6x5|"
    "astc-6x6|ASTC_6x6|"
    "astc-8x5|ASTC_8x5|"
    "astc-8x6|ASTC_8x6|"
    "astc-8x8|ASTC_8x8|"
    "astc-10x5|ASTC_10x5|"
    "astc-10x6|ASTC_10x6|"
    "astc-10x8|ASTC_10x8|"
    "astc-10x10|ASTC_10x10|"
    "astc-12x10|ASTC_12x10|"
    "astc-12x12|ASTC_12x12|"
    # PVRTC
    "pvrtc1-rgb-2bpp|PVRTC1_RGB_2BPP|"
    "pvrtc1-rgba-2bpp|PVRTC1_RGBA_2BPP|"
    "pvrtc1-rgb-4bpp|PVRTC1_RGB_4BPP|"
    "pvrtc1-rgba-4bpp|PVRTC1_RGBA_4BPP|"
    "pvrtc2-rgba-2bpp|PVRTC2_RGBA_2BPP|"
    "pvrtc2-rgba-4bpp|PVRTC2_RGBA_4BPP|"
)

if [[ "$NO_ENCODE" == "0" ]]; then
# ─── Generate DDS ────────────────────────────────────────────────────────────
echo "Generating DDS files..."
total=${#COMMON_FORMATS[@]}
count=0

for entry in "${COMMON_FORMATS[@]}"; do
    count=$((count + 1))
    IFS='|' read -r name cf_fmt cf_type <<< "$entry"
    output="$DDS_DIR/${BASE_NAME}-${name}-dx10.dds"
    printf '[%d/%d] %s\n' "$count" "$total" "$name"
    encode "$name" "$output" "$cf_fmt" "$cf_type"
done

echo "DDS generation complete. Files written to: $DDS_DIR"

# ─── Generate KTX ────────────────────────────────────────────────────────────
echo ""
echo "Generating KTX files..."
KTX_FORMATS=("${COMMON_FORMATS[@]}" "${KTX_EXTRA_FORMATS[@]}")
total=${#KTX_FORMATS[@]}
count=0

for entry in "${KTX_FORMATS[@]}"; do
    count=$((count + 1))
    IFS='|' read -r name cf_fmt cf_type <<< "$entry"
    output="$KTX_DIR/${BASE_NAME}-${name}.ktx"
    printf '[%d/%d] %s\n' "$count" "$total" "$name"
    # -m generates a full mipmap chain
    encode "$name" "$output" "$cf_fmt" "$cf_type" -m
done

echo "KTX generation complete. Files written to: $KTX_DIR"
fi # NO_ENCODE

# ─── Reference PNG decode ──────────────────────────────────────────────────────
# Decodes each generated DDS/KTX back to PNG for visual inspection.
# tacentview outputs the PNG alongside the source file, which is then moved
# into a reference/ subdirectory.
#
# HDR formats (BC6H, float, ufloat) get tone=1.0 (neutral exposure) so the
# result is representable as an 8-bit PNG. corr=auto respects any sRGB tagging
# embedded in the file by cuttlefish.

decode_references() {
    local dir="$1" ext="$2" in_flag="$3"
    local ref_dir="$dir/reference"
    mkdir -p "$ref_dir"

    local files=("$dir"/*."$ext")
    [[ -f "${files[0]}" ]] || { echo "No .$ext files found in $dir"; return; }

    local total="${#files[@]}" count=0
    echo "Decoding $ext reference PNGs..."

    for src in "${files[@]}"; do
        count=$((count + 1))
        local name; name="$(basename "${src%.*}")"
        local out_png="${src%.*}.png"
        local ref_png="$ref_dir/${name}.png"

        printf '[%d/%d] %s\n' "$count" "$total" "$name"

        # Apply neutral tone-map for HDR/float formats so the preview is
        # representable as an 8-bit PNG
        local params="corr=auto"
        if [[ "$name" == *bc6h* ]] || [[ "$name" == *float* ]] || [[ "$name" == *ufloat* ]]; then
            params="corr=auto,tone=1.0"
        fi

        if [[ -f "$ref_png" ]]; then
            echo "  Skipped (already exists): $ref_png"
            continue
        fi

        # tacentview writes output alongside the input file; we then move it
        if tacentview -c -w "$in_flag" "$params" -o png "$src"; then
            if [[ -f "$out_png" ]]; then
                mv "$out_png" "$ref_png"
                echo "  Created $ref_png"
            else
                echo "  Warning: expected $out_png not found" >&2
            fi
        else
            echo "  Warning: tacentview failed for $src" >&2
        fi
    done

    echo "$ext reference PNGs written to: $ref_dir"
}

if [[ "$HAS_TACENTVIEW" == "1" ]]; then
    echo ""
    [[ -d "$DDS_DIR" ]] && decode_references "$DDS_DIR" "dds" "--inDDS" \
        || echo "Skipping DDS decode: $DDS_DIR does not exist"
    echo ""
    [[ -d "$KTX_DIR" ]] && decode_references "$KTX_DIR" "ktx" "--inKTX" \
        || echo "Skipping KTX decode: $KTX_DIR does not exist"
else
    echo ""
    echo "Skipping reference PNG decode (tacentview not available)."
fi

echo ""
echo "All test images generated successfully!"
