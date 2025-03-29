[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$InputImage,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet('linear', 'srgb')]
    [string]$ColorSpace = "linear",  # default to linear
   
    [Parameter(Mandatory=$false)]
    [string]$OutputDir = "./testktx",
    
    [Parameter(Mandatory=$false)]
    [string]$PVRTexToolPath = "PVRTexToolCLI",
    
    [Parameter(Mandatory=$false)]
    [switch]$Help
)

# Show help if requested
if ($Help -or [string]::IsNullOrEmpty($InputImage)) {
    Write-Host "GenerateTestKtx.ps1 - Generate test KTX1 files for BCnEncoder.NET"
    Write-Host "Usage: .\GenerateTestKtxWithPVR.ps1 -InputImage <path> -ColorSpace <linear|srgb> [-OutputDir <path>] [-PVRTexToolPath <path>] [-KtxToolPath <path>] [-Formats <format1> <format2> ...]"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -InputImage       Path to the input image file"
    Write-Host "  -ColorSpace       Color space of the input image: 'linear' or 'srgb'"
    Write-Host "  -OutputDir        Output directory for generated files (default: ./testktx)"
    Write-Host "  -PVRTexToolPath   Path to PVRTexToolCLI.exe (default: uses PVRTexToolCLI from PATH)"
    Write-Host "  -Help             Show this help message"
    exit 0
}

# Input validation when not showing help
if ([string]::IsNullOrEmpty($InputImage)) {
    Write-Error "InputImage parameter is required. Use -Help for usage information."
    exit 1
}

if ([string]::IsNullOrEmpty($ColorSpace)) {
    Write-Error "ColorSpace parameter is required. Use -Help for usage information."
    exit 1
}

# Create output directory if it doesn't exist
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
    Write-Verbose "Created output directory: $OutputDir"
}

# Create reference directory for decoded PNGs
$referenceDir = Join-Path -Path $OutputDir -ChildPath "reference"
if (-not (Test-Path $referenceDir)) {
    New-Item -ItemType Directory -Path $referenceDir | Out-Null
    Write-Verbose "Created reference directory: $referenceDir"
}

# Find tools
$PVRTexToolPath = "PVRTexToolCLI"
$KtxToolPath = "ktx"

# Verify PVRTexToolCLI is available
try {
    $PVRTexToolVersion = Invoke-Expression "& `"$PVRTexToolPath`" -version 2>&1"
    Write-Verbose "Using PVRTexToolCLI: $PVRTexToolVersion"
} catch {
    Write-Host "Error: PVRTexToolCLI is not available in the path." -ForegroundColor Red
    Write-Host "Please install it from: https://developer.imaginationtech.com/pvrtextool/" -ForegroundColor Red
    exit 1
}

# Verify the input image exists
if (!(Test-Path -Path $InputImage)) {
    Write-Host "Error: Input image $InputImage not found." -ForegroundColor Red
    exit 1
}

# Get input image base name for output file naming
$inputBaseName = [System.IO.Path]::GetFileNameWithoutExtension($InputImage)
Write-Verbose "Using input image: $InputImage (Base name: $inputBaseName)"
Write-Verbose "Color space: $ColorSpace"

# Define format mappings - each format gets a name, and a PVRTexToolCLI format string
$formatMappings = @(
    # BC1 (DXT1) - no alpha
    @{
        Name = "bc1-linear"
        Format = "BC1,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $false
        ExtraOptions = "-q pvrtcbest"
    },
    @{
        Name = "bc1-srgb"
        Format = "BC1,UBN,sRGB"
        ColorSpace = "srgb"
        SupportsAlpha = $false
        ExtraOptions = "-q pvrtcbest"
    },
    
    # BC1 (DXT1) - with alpha (1-bit)
    @{
        Name = "bc1-alpha-linear"
        Format = "BC1,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q pvrtcbest"
    },
    @{
        Name = "bc1-alpha-srgb"
        Format = "BC1,UBN,sRGB"
        ColorSpace = "srgb"
        SupportsAlpha = $true
        ExtraOptions = "-q pvrtcbest"
    },
    
    # BC2 (DXT3)
    @{
        Name = "bc2-linear"
        Format = "BC2,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q pvrtcbest"
    },
    @{
        Name = "bc2-srgb"
        Format = "BC2,UBN,sRGB"
        ColorSpace = "srgb"
        SupportsAlpha = $true
        ExtraOptions = "-q pvrtcbest"
    },
    
    # BC3 (DXT5)
    @{
        Name = "bc3-linear"
        Format = "BC3,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q pvrtcbest"
    },
    @{
        Name = "bc3-srgb"
        Format = "BC3,UBN,sRGB"
        ColorSpace = "srgb"
        SupportsAlpha = $true
        ExtraOptions = "-q pvrtcbest"
    },
    
    # BC4 (unsigned)
    @{
        Name = "bc4-linear"
        Format = "BC4,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $false
        ExtraOptions = "-q pvrtcbest"
    },
    
    # BC4 (signed)
    @{
        Name = "bc4s-linear"
        Format = "BC4,SBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $false
        ExtraOptions = "-q pvrtcbest"
    },
    
    # BC5 (unsigned)
    @{
        Name = "bc5-linear"
        Format = "BC5,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $false
        ExtraOptions = "-q pvrtcbest"
    },
    
    # BC5 (signed)
    @{
        Name = "bc5s-linear"
        Format = "BC5,SBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $false
        ExtraOptions = "-q pvrtcbest"
    },
    
    # 16-bit color formats
	@{
        Name = "etc1-linear"
        Format = "ETC1,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $false
        ExtraOptions = "-q ETCSLOW"
    },
	@{
        Name = "etc1-srgb"
        Format = "ETC1,UBN,sRGB"
        ColorSpace = "srgb"
        SupportsAlpha = $false
        ExtraOptions = "-q ETCSLOW"
    },
    @{
        Name = "etc2-rgb-linear"
        Format = "ETC2_RGB,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $false
        ExtraOptions = "-q ETCSLOW"
    },
    @{
        Name = "etc2-rgb-srgb"
        Format = "ETC2_RGB,UBN,sRGB"
        ColorSpace = "srgb"
        SupportsAlpha = $false
        ExtraOptions = "-q ETCSLOW"
    },
    @{
        Name = "etc2-rgba-linear"
        Format = "ETC2_RGBA,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ETCSLOW"
    },
    @{
        Name = "etc2-rgba-srgb"
        Format = "ETC2_RGBA,UBN,sRGB"
        ColorSpace = "srgb"
        SupportsAlpha = $true
        ExtraOptions = "-q ETCSLOW"
    },
    @{
        Name = "etc2-rgb-a1-linear"
        Format = "ETC2_RGB_A1,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ETCSLOW"
    },
    @{
        Name = "etc2-rgb-a1-srgb"
        Format = "ETC2_RGB_A1,UBN,sRGB"
        ColorSpace = "srgb"
        SupportsAlpha = $true
        ExtraOptions = "-q ETCSLOW"
    },
    @{
        Name = "eac-r11-linear"
        Format = "EAC_R11,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $false
        ExtraOptions = "-q ETCSLOW"
    },
    @{
        Name = "eac-rg11-linear"
        Format = "EAC_RG11,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $false
        ExtraOptions = "-q ETCSLOW"
    },
    
    # ASTC formats
    @{
        Name = "astc-4x4-linear"
        Format = "ASTC_4X4,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-4x4-srgb"
        Format = "ASTC_4X4,UBN,sRGB"
        ColorSpace = "srgb"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-5x4-linear"
        Format = "ASTC_5X4,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-5x5-linear"
        Format = "ASTC_5X5,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-6x5-linear"
        Format = "ASTC_6X5,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-6x6-linear"
        Format = "ASTC_6X6,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-8x5-linear"
        Format = "ASTC_8X5,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-8x6-linear"
        Format = "ASTC_8X6,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-8x8-linear"
        Format = "ASTC_8X8,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-10x5-linear"
        Format = "ASTC_10X5,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-10x6-linear"
        Format = "ASTC_10X6,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-10x8-linear"
        Format = "ASTC_10X8,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-10x10-linear"
        Format = "ASTC_10X10,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-12x10-linear"
        Format = "ASTC_12X10,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    @{
        Name = "astc-12x12-linear"
        Format = "ASTC_12X12,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
        ExtraOptions = "-q ASTCEXHAUSTIVE"
    },
    
    # Uncompressed formats
    @{
        Name = "r8-linear"
        Format = "R8,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $false
    },
    @{
        Name = "r8g8-linear"
        Format = "R8G8,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $false
    },
    @{
        Name = "rgba32-linear"
        Format = "R8G8B8A8,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
    },
    @{
        Name = "rgba32-srgb"
        Format = "R8G8B8A8,UBN,sRGB"
        ColorSpace = "srgb"
        SupportsAlpha = $true
    },
    @{
        Name = "bgra32-linear"
        Format = "B8G8R8A8,UBN,lRGB"
        ColorSpace = "linear"
        SupportsAlpha = $true
    },
    @{
        Name = "bgra32-srgb"
        Format = "B8G8R8A8,UBN,sRGB"
        ColorSpace = "srgb"
        SupportsAlpha = $true
    }
)

# Calculate total operations
$totalCount = $formatMappings.Count
$currentCount = 0

Write-Host "Generating encoded KTX files..." -ForegroundColor Cyan

# Process each format

foreach ($format in $formatMappings) {
    $currentCount++
    $outputName = "$inputBaseName-$($format.Name)"
    $outputFile = Join-Path -Path $OutputDir -ChildPath "$outputName.ktx"
    
    Write-Progress -Activity "Generating test images" -Status "[$currentCount of $totalCount] Creating $outputFile" -PercentComplete (($currentCount / $totalCount) * 100)
    
    # Determine color space flags for PVRTexToolCLI
    $colorSpaceFlags = ""
    
    # PVRTexToolCLI assumes input is sRGB by default
    # We need to explicitly tell it the input colorspace
    if ($ColorSpace -eq "linear") {
        # Specify that input is already linear
        $colorSpaceFlags = "-ics lRGB"
    } 
    else { # sRGB input
        $colorSpaceFlags = "-ics sRGB"
    }
    
    # Handle premultiplied alpha for formats that support it
    # BGRA32, RGBA32, BC1 with alpha, BC2, BC3, and BC7 need premultiplied alpha
    $alphaPremulFlag = ""
    if ($format.SupportsAlpha -and ($format.Name -match "bc[1-3]" -or $format.Name -match "rgba" -or $format.Name -match "bgra")) {
        $alphaPremulFlag = "-p"
    }
    
    # Extra options specific to this format
    $extraOptions = if ($format.ExtraOptions) { $format.ExtraOptions } else { "" }
    
    # Build the command
    $cmd = "& `"$PVRTexToolPath`" -i `"$InputImage`" -o `"$outputFile`" -f $($format.Format) -m -dither -pot"
    
    # Add colorspace input flag
    $cmd += " $colorSpaceFlags $alphaPremulFlag $extraOptions"
    
    Write-Verbose "Executing: $cmd"
    
    # Execute command and capture any errors
    $output = Invoke-Expression "$cmd 2>&1"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error executing command: $cmd" -ForegroundColor Red
        Write-Host $output -ForegroundColor Red
    } else {
        Write-Verbose "Created $outputFile"
    }
}

Write-Progress -Activity "Generating test images" -Completed
Write-Host "All test images have been generated in $OutputDir" -ForegroundColor Green

# Generate reference PNGs for visualization
Write-Host "Generating reference decoded PNGs..." -ForegroundColor Cyan
Write-Progress -Activity "Generating reference PNGs" -Status "Starting..." -PercentComplete 0

$totalKtx = @(Get-ChildItem -Path $OutputDir -Filter "*.ktx").Count
$currentKtx = 0

# Generate reference PNGs for all formats using PVRTexToolCLI's decompression
foreach ($ktxFile in Get-ChildItem -Path $OutputDir -Filter "*.ktx") {
    $currentKtx++
    $ktxName = [System.IO.Path]::GetFileNameWithoutExtension($ktxFile)
    $ktxPath = $ktxFile.FullName
    $refPngPath = Join-Path -Path $referenceDir -ChildPath "$ktxName.png"
    
    Write-Progress -Activity "Generating reference PNGs" -Status "Processing $ktxName" -PercentComplete (($currentKtx / $totalKtx) * 100)
    Write-Verbose "Processing $ktxName for reference PNG generation"

    # Use PVRTexToolCLI to decompress directly to PNG
    $decompressCmd = "& `"$PVRTexToolPath`" -i `"$ktxPath`" -ics srgb -d `"$refPngPath`" -noout"
    
    Write-Verbose "Command: $decompressCmd"
    try {
        $output = Invoke-Expression "$decompressCmd 2>&1"
        
        if (Test-Path $refPngPath) {
            Write-Verbose "Created reference PNG for $ktxName"
        } else {
            Write-Host "Unable to create reference PNG for $ktxName" -ForegroundColor Yellow
            Write-Host "Error: $output" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "An error occurred while generating reference PNG for $ktxName" -ForegroundColor Red
        Write-Host "$_" -ForegroundColor Red
    }
}

Write-Progress -Activity "Generating reference PNGs" -Completed
Write-Host "Reference decoded PNGs have been generated in $referenceDir" -ForegroundColor Green
