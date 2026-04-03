#!/usr/bin/env bash
# GenerateTestEncodes.sh
# Generates test DDS, KTX, and KTX2 texture files using cuttlefish, then decodes
# reference PNGs. KTX2 files are converted from KTX using ktx2ktx2. Reference
# PNGs for KTX/KTX2 are extracted with ktx extract (KTX2 only); DDS reference
# PNGs are decoded with tacentview.
# Replaces GenerateTestDds.ps1 and GenerateTestKtx.ps1.
#
# Notes:
#  - Only DX10-header DDS files are generated; cuttlefish does not support DX9/legacy headers.
#  - ETC/EAC/ASTC/PVRTC formats are KTX-only (not valid in DDS containers).

set -euo pipefail

usage() {
    cat <<'EOF'
GenerateTestEncodes.sh - Generate test DDS, KTX, and KTX2 files for BCnEncoder.NET

USAGE:
    ./GenerateTestEncodes.sh -i <input> [OPTIONS]
    ./GenerateTestEncodes.sh --no-encode [OPTIONS]

OPTIONS:
    -i, --input <path>       Path to input image (required unless --no-encode)
    -d, --dds-dir <path>     DDS directory (default: ./testdds)
    -k, --ktx-dir <path>     KTX directory (default: ./testktx)
    -K, --ktx2-dir <path>    KTX2 directory (default: ./testktx2)
        --no-encode          Skip encoding; only decode reference PNGs from
                             existing files in the DDS, KTX, and KTX2 directories
        --tacentview-ktx     Use tacentview for KTX reference decoding instead
                             of the default ktx extract method

TOOLS:
    cuttlefish    Required for encoding (not needed with --no-encode)
    ktx2ktx2      Required for KTX -> KTX2 conversion
    ktx           Required for KTX2 reference PNG extraction
    tacentview    Required for DDS reference PNG decoding; also used for KTX
                  decoding if --tacentview-ktx is set
EOF
}

INPUT=""
DDS_DIR="./testdds"
KTX_DIR="./testktx"
KTX2_DIR="./testktx2"
NO_ENCODE=0
TACENTVIEW_KTX=0

while [[ $# -gt 0 ]]; do
    case "$1" in
        -i|--input)        INPUT="$2";   shift 2 ;;
        -d|--dds-dir)      DDS_DIR="$2"; shift 2 ;;
        -k|--ktx-dir)      KTX_DIR="$2"; shift 2 ;;
        -K|--ktx2-dir)     KTX2_DIR="$2"; shift 2 ;;
        --no-encode)       NO_ENCODE=1;  shift ;;
        --tacentview-ktx)  TACENTVIEW_KTX=1; shift ;;
        -h|--help)         usage; exit 0 ;;
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
    || echo "Warning: tacentview not found — DDS reference PNG decoding will be skipped"

HAS_KTX_TOOLS=0
command -v ktx2ktx2 &>/dev/null && command -v ktx &>/dev/null && HAS_KTX_TOOLS=1 \
    || echo "Warning: ktx2ktx2/ktx not found — KTX2 conversion and reference PNG extraction will be skipped"

if [[ "$NO_ENCODE" == "0" ]]; then
    BASE_NAME="$(basename "${INPUT%.*}")"
    mkdir -p "$DDS_DIR" "$KTX_DIR" "$KTX2_DIR"
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
    "bc4s|BC4|snorm"
    # BC5
    "bc5|BC5|unorm"
    "bc5s|BC5|snorm"
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
    "r8s|R8|snorm"
    "r8g8|R8G8|unorm"
    "r8g8s|R8G8|snorm"
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
    "eac-r11s|EAC_R11|snorm"
    "eac-rg11|EAC_R11G11|unorm"
    "eac-rg11s|EAC_R11G11|snorm"
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

# ─── Convert KTX -> KTX2 ─────────────────────────────────────────────────────
# ktx extract (used for reference PNGs) only supports KTX2, so convert each
# KTX file. The pixel data is identical; KTX2 is just a different container.

if [[ "$HAS_KTX_TOOLS" == "1" ]]; then
    mkdir -p "$KTX2_DIR"
    ktx_files=("$KTX_DIR"/*.ktx)
    if [[ -f "${ktx_files[0]}" ]]; then
        echo ""
        echo "Converting KTX -> KTX2..."
        total="${#ktx_files[@]}" count=0
        for ktx in "${ktx_files[@]}"; do
            count=$((count + 1))
            name="$(basename "${ktx%.*}")"
            ktx2="$KTX2_DIR/${name}.ktx2"
            printf '[%d/%d] %s\n' "$count" "$total" "$name"
            if [[ -f "$ktx2" ]]; then
                echo "  Skipped (already exists): $ktx2"
            elif ktx2ktx2 -o "$ktx2" "$ktx"; then
                echo "  Created $ktx2"
            else
                echo "  Warning: ktx2ktx2 failed for $ktx" >&2
            fi
        done
        echo "KTX2 conversion complete. Files written to: $KTX2_DIR"
    else
        echo "No KTX files found in $KTX_DIR — skipping KTX2 conversion"
    fi
else
    echo ""
    echo "Skipping KTX2 conversion (ktx2ktx2 not available)."
fi

# ─── Reference PNG decode ─────────────────────────────────────────────────────
# DDS:      decoded with tacentview
# KTX/KTX2: decoded with ktx extract (KTX2 only); PNGs are then copied from
#            the KTX2 reference folder into the KTX reference folder since the
#            pixel data is identical. Falls back to tacentview if --tacentview-ktx.
#
# HDR formats (BC6H, float, ufloat) get tone=1.0 for tacentview so the preview
# is representable as an 8-bit PNG.

decode_dds_references() {
    local dir="$1"
    local ref_dir="$dir/reference"
    mkdir -p "$ref_dir"

    local files=("$dir"/*.dds)
    [[ -f "${files[0]}" ]] || { echo "No .dds files found in $dir"; return; }

    local total="${#files[@]}" count=0
    echo "Decoding DDS reference PNGs with tacentview..."

    for src in "${files[@]}"; do
        count=$((count + 1))
        local name; name="$(basename "${src%.*}")"
        local out_png="${src%.*}.png"
        local ref_png="$ref_dir/${name}.png"

        printf '[%d/%d] %s\n' "$count" "$total" "$name"

        if [[ -f "$ref_png" ]]; then
            echo "  Skipped (already exists): $ref_png"
            continue
        fi

        local params="corr=none"

        if tacentview -c -w --inDDS "$params" -o png "$src"; then
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

    echo "DDS reference PNGs written to: $ref_dir"
}

decode_ktx2_references() {
    local ktx2_dir="$1" ktx_dir="$2"
    local ktx2_ref_dir="$ktx2_dir/reference"
    local ktx_ref_dir="$ktx_dir/reference"
    mkdir -p "$ktx2_ref_dir" "$ktx_ref_dir"

    local files=("$ktx2_dir"/*.ktx2)
    [[ -f "${files[0]}" ]] || { echo "No .ktx2 files found in $ktx2_dir"; return; }

    local total="${#files[@]}" count=0
    echo "Extracting KTX2 reference PNGs with ktx extract (falling back to tacentview for block-compressed)..."

    for src in "${files[@]}"; do
        count=$((count + 1))
        local name; name="$(basename "${src%.*}")"
        local ktx_src="$ktx_dir/${name}.ktx"
        local ktx2_ref_png="$ktx2_ref_dir/${name}.png"
        local ktx_ref_png="$ktx_ref_dir/${name}.png"

        printf '[%d/%d] %s\n' "$count" "$total" "$name"

        if [[ ! -f "$ktx2_ref_png" ]]; then
            # ktx extract only decodes uncompressed KTX2 to PNG; block-compressed
            # KTX2 files (BCn, ETC, ASTC) are not "transcodable" without BasisLZ
            # supercompression, so extract won't produce a PNG for those. We detect
            # this by checking whether the output file was actually created.
            ktx extract "$src" "$ktx2_ref_png" 2>/dev/null || true
            if [[ -f "$ktx2_ref_png" ]]; then
                echo "  Created $ktx2_ref_png"
            elif [[ "$HAS_TACENTVIEW" == "1" && -f "$ktx_src" ]]; then
                echo "  ktx extract produced no PNG (block-compressed); falling back to tacentview"
                local out_png="${ktx_src%.*}.png"
                local params="corr=none"
                if tacentview -c -w --inKTX "$params" -o png "$ktx_src" && [[ -f "$out_png" ]]; then
                    mv "$out_png" "$ktx2_ref_png"
                    echo "  Created $ktx2_ref_png"
                else
                    echo "  Warning: tacentview fallback also failed for $name" >&2
                    continue
                fi
            else
                echo "  Warning: no PNG produced for $name (block-compressed; tacentview unavailable)" >&2
                continue
            fi
        else
            echo "  Skipped (already exists): $ktx2_ref_png"
        fi

        # Copy to KTX reference folder — pixel data is identical
        if [[ -f "$ktx_ref_png" ]]; then
            echo "  Skipped (already exists): $ktx_ref_png"
        else
            cp "$ktx2_ref_png" "$ktx_ref_png"
            echo "  Copied to $ktx_ref_png"
        fi
    done

    echo "KTX2 reference PNGs written to: $ktx2_ref_dir"
    echo "KTX  reference PNGs written to: $ktx_ref_dir"
}

decode_ktx_references_tacentview() {
    local dir="$1"
    local ref_dir="$dir/reference"
    mkdir -p "$ref_dir"

    local files=("$dir"/*.ktx)
    [[ -f "${files[0]}" ]] || { echo "No .ktx files found in $dir"; return; }

    local total="${#files[@]}" count=0
    echo "Decoding KTX reference PNGs with tacentview..."

    for src in "${files[@]}"; do
        count=$((count + 1))
        local name; name="$(basename "${src%.*}")"
        local out_png="${src%.*}.png"
        local ref_png="$ref_dir/${name}.png"

        printf '[%d/%d] %s\n' "$count" "$total" "$name"

        if [[ -f "$ref_png" ]]; then
            echo "  Skipped (already exists): $ref_png"
            continue
        fi

        local params="corr=none"

        if tacentview -c -w --inKTX "$params" -o png "$src"; then
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

    echo "KTX reference PNGs written to: $ref_dir"
}

echo ""
if [[ "$HAS_TACENTVIEW" == "1" ]]; then
    [[ -d "$DDS_DIR" ]] && decode_dds_references "$DDS_DIR" \
        || echo "Skipping DDS decode: $DDS_DIR does not exist"
else
    echo "Skipping DDS reference PNG decode (tacentview not available)."
fi

echo ""
if [[ "$TACENTVIEW_KTX" == "1" ]]; then
    if [[ "$HAS_TACENTVIEW" == "1" ]]; then
        [[ -d "$KTX_DIR" ]] && decode_ktx_references_tacentview "$KTX_DIR" \
            || echo "Skipping KTX decode: $KTX_DIR does not exist"
    else
        echo "Skipping KTX reference PNG decode (tacentview not available)."
    fi
elif [[ "$HAS_KTX_TOOLS" == "1" ]]; then
    if [[ -d "$KTX2_DIR" ]]; then
        decode_ktx2_references "$KTX2_DIR" "$KTX_DIR"
    else
        echo "Skipping KTX/KTX2 decode: $KTX2_DIR does not exist"
    fi
else
    echo "Skipping KTX reference PNG decode (ktx2ktx2/ktx not available; use --tacentview-ktx to use tacentview instead)."
fi

echo ""
echo "All test images generated successfully!"
