#!/usr/bin/env bash
# Quality benchmark image generator
# Generates compressed test images for benchmarking quality metrics using cuttlefish

set -euo pipefail

# Define hardcoded paths
INPUT_DIR="testImages/raw"
OUTPUT_DIR="testImages/Quality"

# Create output directory if needed
mkdir -p "$OUTPUT_DIR"

# Source files to use
RGB_FILE="$INPUT_DIR/rgb_hard.png"
NORMAL_FILE="$INPUT_DIR/normal_1.png"
SPECULAR_FILE="$INPUT_DIR/height_1.png" # Using as stand-in for specular map

# Copy reference images
cp -f "$RGB_FILE" "$OUTPUT_DIR/albedo_reference.png"
cp -f "$NORMAL_FILE" "$OUTPUT_DIR/normal_reference.png"
cp -f "$SPECULAR_FILE" "$OUTPUT_DIR/specular_reference.png"

echo "Copied reference images"

run_cuttlefish() {
    local input="$1"
    local output="$2"
    shift 2
    echo "Running: cuttlefish -i \"$input\" -o \"$output\" $*"
    cuttlefish -i "$input" -o "$output" "$@"
    if [[ -f "$output" ]]; then
        echo "Created $output"
    fi
}

echo ""
echo "Generating cuttlefish compressed images..."

# BC1 albedo image (sRGB)
run_cuttlefish "$RGB_FILE" "$OUTPUT_DIR/albedo_cuttlefish_bc1.dds" \
    -f BC1_RGB --srgb -Q highest

# BC3 albedo image (sRGB)
run_cuttlefish "$RGB_FILE" "$OUTPUT_DIR/albedo_cuttlefish_bc3.dds" \
    -f BC3 --srgb -Q highest

# BC6H albedo image (unsigned, sRGB input)
run_cuttlefish "$RGB_FILE" "$OUTPUT_DIR/albedo_cuttlefish_bc6h.dds" \
    -f BC6H -t ufloat --srgb -Q highest

# BC6H Signed albedo image (signed, sRGB input)
run_cuttlefish "$RGB_FILE" "$OUTPUT_DIR/albedo_cuttlefish_bc6h_sf16.dds" \
    -f BC6H -t float --srgb -Q highest

# BC7 albedo image (sRGB)
run_cuttlefish "$RGB_FILE" "$OUTPUT_DIR/albedo_cuttlefish_bc7.dds" \
    -f BC7 --srgb -Q highest

# BC5 normal map (linear)
run_cuttlefish "$NORMAL_FILE" "$OUTPUT_DIR/normal_cuttlefish_bc5.dds" \
    -f BC5 -Q highest

# BC1 specular map (linear)
run_cuttlefish "$SPECULAR_FILE" "$OUTPUT_DIR/specular_cuttlefish_bc1.dds" \
    -f BC1_RGB -Q highest

# BC4 specular map (linear)
run_cuttlefish "$SPECULAR_FILE" "$OUTPUT_DIR/specular_cuttlefish_bc4.dds" \
    -f BC4 -Q highest

echo ""
echo "Test images generated successfully!"
echo "You can now run the ImageQualityBenchmark test to analyze these images."
