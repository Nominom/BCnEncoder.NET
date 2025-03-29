[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$InputImage,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet('linear', 'srgb')]
    [string]$ColorSpace = "linear",  # default to linear
    
    [Parameter(Mandatory=$false)]
    [string]$OutputDir = "./testdds",
    
    [Parameter(Mandatory=$false)]
    [string]$TexConvPath,
    
    [Parameter(Mandatory=$false)]
    [switch]$Help
)

# Set VerbosePreference based on -Verbose parameter
if ($Verbose) {
    $VerbosePreference = "Continue"
} else {
    $VerbosePreference = "SilentlyContinue"
}

# Show help if requested
if ($Help -or [string]::IsNullOrEmpty($InputImage)) {
    $helpText = @"
    DDS Test Image Generator - BCnEncoder.NET

    USAGE:
        .\GenerateTestDds.ps1 -InputImage <path> -ColorSpace <linear|srgb> [OPTIONS]

    PARAMETERS:
        -InputImage <path>         Path to input image
        -ColorSpace <linear|srgb>  Input color space
        -OutputDir <path>          Output directory (default: current)
        -TexConvPath <path>        Path to texconv.exe (auto-detected from PATH by default)
        -Verbose                   Show detailed output
        -Help                      Display this help

    EXAMPLE:
        .\GenerateTestDds.ps1 -InputImage texture.png -ColorSpace linear
"@
    
    Write-Host $helpText
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

# Find texconv.exe in the PATH if not specified
if ([string]::IsNullOrEmpty($TexConvPath)) {
    $TexConvPath = Get-Command texconv.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
    
    if ([string]::IsNullOrEmpty($TexConvPath)) {
        Write-Error "Could not find texconv.exe in PATH. Please specify the path using -TexConvPath."
        exit 1
    } else {
        Write-Verbose "Found texconv.exe at: $TexConvPath"
    }
}

# Verify texconv exists
if (!(Test-Path -Path $TexConvPath)) {
    Write-Error "Could not find texconv.exe at $TexConvPath. Please specify the correct path."
    exit 1
}

# Ensure output directory exists
if (!(Test-Path -Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

# Input validation
if (!(Test-Path -Path $InputImage)) {
    Write-Error "Input image not found: $InputImage"
    exit 1
}

$inputBaseName = [System.IO.Path]::GetFileNameWithoutExtension($InputImage)

# Setup colorspace arguments
$colorSpaceArg = if ($ColorSpace -eq "srgb") { "-srgbi" } else { "" }

# Define format mappings
$formatMappings = @(
    # BC1 (DXT1) formats
    @{
        Name = "bc1-linear";
        Format = "BC1_UNORM";
        DX10Support = $true;
        LegacySupport = $true;
        LegacyFormat = "DXT1";
        ColorSpace = "linear";
    },
    @{
        Name = "bc1-srgb";
        Format = "BC1_UNORM_SRGB";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "srgb";
    },
    @{
        Name = "bc1-alpha-linear";
        Format = "BC1_UNORM";
        DX10Support = $true;
        LegacySupport = $true;
        LegacyFormat = "DXT1";
        ColorSpace = "linear";
        ExtraArgs = "-at 0.5";
    },
    @{
        Name = "bc1-alpha-srgb";
        Format = "BC1_UNORM_SRGB";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "srgb";
        ExtraArgs = "-at 0.5";
    },
    
    # BC2 (DXT3) formats
    @{
        Name = "bc2-linear";
        Format = "BC2_UNORM";
        DX10Support = $true;
        LegacySupport = $true;
        LegacyFormat = "DXT3";
        ColorSpace = "linear";
    },
    @{
        Name = "bc2-srgb";
        Format = "BC2_UNORM_SRGB";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "srgb";
    },
    
    # BC3 (DXT5) formats
    @{
        Name = "bc3-linear";
        Format = "BC3_UNORM";
        DX10Support = $true;
        LegacySupport = $true;
        LegacyFormat = "DXT5";
        ColorSpace = "linear";
    },
    @{
        Name = "bc3-srgb";
        Format = "BC3_UNORM_SRGB";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "srgb";
    },
    
    # BC4 formats
    @{
        Name = "bc4-linear";
        Format = "BC4_UNORM";
        DX10Support = $true;
        LegacySupport = $true;
        LegacyFormat = "BC4_UNORM";
        ColorSpace = "linear";
    },
    @{
        Name = "bc4s-linear";
        Format = "BC4_SNORM";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "linear";
    },
    
    # BC5 formats
    @{
        Name = "bc5-linear";
        Format = "BC5_UNORM";
        DX10Support = $true;
        LegacySupport = $true;
        LegacyFormat = "BC5_UNORM";
        ColorSpace = "linear";
    },
    @{
        Name = "bc5s-linear";
        Format = "BC5_SNORM";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "linear";
    },
    
    # BC6H formats
    @{
        Name = "bc6u-linear";
        Format = "BC6H_UF16";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "linear";
    },
    @{
        Name = "bc6s-linear";
        Format = "BC6H_SF16";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "linear";
    },
    
    # BC7 formats
    @{
        Name = "bc7-linear";
        Format = "BC7_UNORM";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "linear";
    },
    @{
        Name = "bc7-srgb";
        Format = "BC7_UNORM_SRGB";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "srgb";
    },
    
    # R8 formats
    @{
        Name = "r8-linear";
        Format = "R8_UNORM";
        DX10Support = $true;
        LegacySupport = $true;
        ColorSpace = "linear";
    },
    @{
        Name = "r8s-linear";
        Format = "R8_SNORM";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "linear";
    },
    
    # R8G8 formats
    @{
        Name = "r8g8-linear";
        Format = "R8G8_UNORM";
        DX10Support = $true;
        LegacySupport = $true;
        ColorSpace = "linear";
    },
    @{
        Name = "r8g8s-linear";
        Format = "R8G8_SNORM";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "linear";
    },
    
    # R10G10B10A2 format
    @{
        Name = "r10g10b10a2-linear";
        Format = "R10G10B10A2_UNORM";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "linear";
    },
    
    # RGBA8 formats
    @{
        Name = "rgba32-linear";
        Format = "R8G8B8A8_UNORM";
        DX10Support = $true;
        LegacySupport = $true;
        ColorSpace = "linear";
    },
    @{
        Name = "rgba32-srgb";
        Format = "R8G8B8A8_UNORM_SRGB";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "srgb";
    },
    
    # BGRA formats
    @{
        Name = "bgra32-linear";
        Format = "B8G8R8A8_UNORM";
        DX10Support = $true;
        LegacySupport = $true;
        ColorSpace = "linear";
    },
    @{
        Name = "bgra32-srgb";
        Format = "B8G8R8A8_UNORM_SRGB";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "srgb";
    },
    
    # Float formats
    @{
        Name = "rgba-float-linear";
        Format = "R32G32B32A32_FLOAT";
        DX10Support = $true;
        LegacySupport = $true;
        LegacyFormat = "FP32";
        ColorSpace = "linear";
    },
    @{
        Name = "rgba-half-linear";
        Format = "R16G16B16A16_FLOAT";
        DX10Support = $true;
        LegacySupport = $true;
        LegacyFormat = "FP16";
        ColorSpace = "linear";
    },
    @{
        Name = "rgb-float-linear";
        Format = "R32G32B32_FLOAT";
        DX10Support = $true;
        LegacySupport = $false;
        ColorSpace = "linear";
    }
)

# Calculate total operations (DX10 + legacy formats when supported)
$totalCount = ($formatMappings | Where-Object { $_.DX10Support }).Count + ($formatMappings | Where-Object { $_.LegacySupport }).Count
$currentCount = 0

Write-Host "Generating encoded DDS files..." -ForegroundColor Cyan

# Process each format
foreach ($format in $formatMappings) {
    # Generate colorspace arg based on the target format colorspace
    $formatColorSpaceArg = ""
    
    # Determine colorspace conversion flags
    # NOTE: texconv handles colorspace in two ways:
    # 1. Formats with _SRGB in their name (e.g. R8G8B8A8_UNORM_SRGB) automatically apply linear→sRGB conversion
    # 2. Command flags (-srgbi, -srgbo) can also specify conversions
    # Be careful not to double-convert by using both methods together
    if ($format.ColorSpace -eq "srgb" -and $ColorSpace -eq "linear") {
        # Converting from linear input to sRGB output
        # For linear→sRGB, just use the sRGB DX10 format without explicit conversion flag
        # The _SRGB format suffix will handle the conversion automatically
        $formatColorSpaceArg = ""
    }
    elseif ($format.ColorSpace -eq "srgb" -and $ColorSpace -eq "srgb") {
        # Keep as sRGB - input is sRGB, output is sRGB
        # Tell texconv the input is sRGB so it doesn't treat it as linear
        $formatColorSpaceArg = "-srgbi"
    }
    elseif ($format.ColorSpace -eq "linear" -and $ColorSpace -eq "srgb") {
        # Converting from sRGB input to linear output
        # Tell texconv the input is sRGB
        $formatColorSpaceArg = "-srgbi"
    }
    # For linear to linear, don't specify any flags
    elseif ($format.ColorSpace -eq "linear" -and $ColorSpace -eq "linear") {
        # texconv treats inputs without -srgbi as linear by default
        $formatColorSpaceArg = ""
    }
    
    # Set extra args
    $extraArgs = if ($format.ExtraArgs) { $format.ExtraArgs } else { "" }
    
    # Use premultiplied alpha when supported, but not for BC4 and BC5 formats
    $premultipliedAlpha = ""
    if (($format.Name -match "bc[1-3]" -or $format.Name -match "bc[67]" -or $format.Name -match "rgba" -or $format.Name -match "bgra") -and 
        (-not ($format.Name -match "bc4" -or $format.Name -match "bc5"))) {
        $premultipliedAlpha = "-pmalpha"
    }
    
    # Force single channel input for R8 formats (use only red channel)
    $channelSelect = ""
    if ($format.Name -match "^r8(-|s-)" -and -not ($format.Name -match "^r8g8")) {
        $channelSelect = "--swizzle rrrr"
    }
    
    # Generate DX10 header version
    if ($format.DX10Support) {
        $outputFile = Join-Path -Path $OutputDir -ChildPath "$inputBaseName-$($format.Name)-dx10.dds"
        $currentCount++
        
        Write-Progress -Activity "Generating test images" -Status "[$currentCount of $totalCount] Creating $outputFile" -PercentComplete (($currentCount / $totalCount) * 100)
        
        $cmd = "& `"$TexConvPath`" `"$InputImage`" -o `"$OutputDir`" -y -f $($format.Format) -dx10 $formatColorSpaceArg $premultipliedAlpha $channelSelect $extraArgs -sx -$($format.Name)-dx10"
        Write-Verbose "Executing: $cmd"
        
        # Execute command and only show output if there's an error
        $output = Invoke-Expression "$cmd 2>&1"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error executing command: $cmd" -ForegroundColor Red
            Write-Host $output -ForegroundColor Red
        }
    }
    
    # Generate legacy header version if supported
    if ($format.LegacySupport) {
        $legacyFormat = if ($format.LegacyFormat) { $format.LegacyFormat } else { $format.Format }
        $outputFile = Join-Path -Path $OutputDir -ChildPath "$inputBaseName-$($format.Name).dds"
        $currentCount++
        
        Write-Progress -Activity "Generating test images" -Status "[$currentCount of $totalCount] Creating $outputFile" -PercentComplete (($currentCount / $totalCount) * 100)
        
        $cmd = "& `"$TexConvPath`" `"$InputImage`" -o `"$OutputDir`" -y -f $legacyFormat -dx9 $formatColorSpaceArg $premultipliedAlpha $channelSelect $extraArgs -sx -$($format.Name)"
        Write-Verbose "Executing: $cmd"
        
        # Execute command and only show output if there's an error
        $output = Invoke-Expression "$cmd 2>&1"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error executing command: $cmd" -ForegroundColor Red
            Write-Host $output -ForegroundColor Red
        }
    }
}

Write-Progress -Activity "Generating test images" -Completed
Write-Host "All test images have been generated in $OutputDir" -ForegroundColor Green

# After all formats are processed, decode each generated DDS file back to PNG for reference

# Check if reference directory exists, if not create it
$referenceDir = Join-Path -Path $OutputDir -ChildPath "reference"
if (-not (Test-Path -Path $referenceDir)) {
    New-Item -ItemType Directory -Path $referenceDir | Out-Null
}

# Get all generated DDS files
$ddsFiles = Get-ChildItem -Path $OutputDir -Filter "*.dds"
$totalDdsFiles = $ddsFiles.Count
$currentDdsFile = 0

Write-Host "Generating reference decoded PNGs..." -ForegroundColor Cyan

# Process each DDS file to create a reference PNG
foreach ($ddsFile in $ddsFiles) {
    $currentDdsFile++
    $ddsPath = $ddsFile.FullName
    $ddsName = $ddsFile.BaseName
    $refPngPath = Join-Path -Path $referenceDir -ChildPath "$ddsName.png"
    
    # Show progress
    Write-Progress -Activity "Decoding DDS files to reference PNGs" -Status "Processing $ddsName" -PercentComplete (($currentDdsFile / $totalDdsFiles) * 100)
    
    # Check if this is a format that used premultiplied alpha during encoding
    $needsUnpremultiply = $ddsName -match "-bgra32-|rgba32-|bc1-alpha-|bc2-|bc3-|bc7-"
    $isLinear = $ddsName -match "linear"

    # Decode the DDS to PNG using texconv
    # -f R8G8B8A8_UNORM ensures consistent output format
    # -ft png specifies the output format as PNG
    # -alpha converts premultiplied alpha to straight alpha
    # $alphaFlag = if ($needsUnpremultiply) { "-alpha" } else { "" }
    $outputPixelFormat = if ($isLinear) { "R8G8B8A8_UNORM" } else { "R8G8B8A8_UNORM_SRGB" }

    $decodeCmd = "& `"$TexConvPath`" `"$ddsPath`" -o `"$referenceDir`" -f $outputPixelFormat -ft png -y -nologo"
    Write-Verbose "Decoding DDS to PNG: $decodeCmd"
    
    $cmdResult = Invoke-Expression $decodeCmd 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error decoding $ddsPath to PNG:" -ForegroundColor Red
        Write-Host $cmdResult -ForegroundColor Red
    }
}

Write-Progress -Activity "Decoding DDS files to reference PNGs" -Completed
Write-Host "Reference decoded PNGs have been generated in $referenceDir" -ForegroundColor Green