# Quality benchmark image generator
# Generates compressed test images for benchmarking quality metrics

# Define hardcoded paths
$inputDir = "testImages\raw"
$outputDir = "testImages\Quality"

# Create output directory if needed
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
    Write-Output "Created directory $outputDir"
}

# Source files to use
$rgbFile = "$inputDir\rgb_hard.png"
$normalFile = "$inputDir\normal_1.png"
$specularFile = "$inputDir\height_1.png" # Using as stand-in for specular map

# Copy reference images
Copy-Item -Path $rgbFile -Destination "$outputDir\albedo_reference.png" -Force
Copy-Item -Path $normalFile -Destination "$outputDir\normal_reference.png" -Force
Copy-Item -Path $specularFile -Destination "$outputDir\specular_reference.png" -Force

Write-Output "Copied reference images"

# Function to extract the base filename from a path
function Get-BaseFileName {
    param([string]$path)
    # Get just the filename without the path
    $filename = Split-Path -Leaf $path
    # Get the filename without the extension
    $basename = [System.IO.Path]::GetFileNameWithoutExtension($filename)
    return $basename
}

# Define all compression tasks in a single list
$compressionTasks = @(
    # BC1 albedo image
    @{
        Name = "BC1 albedo image"
        InputFile = $rgbFile
        # Base output name - the tool name will be inserted
        BaseName = "albedo"
        # Format-specific configuration for each tool
        ToolConfig = @{
            DirectXTex = @{
                Format = "BC1_UNORM"
                # Use sRGB for albedo textures
                AdditionalFlags = "-srgbi -srgbo -bc d"
            }
            PVRTexTool = @{
                Format = "BC1,UBN,sRGB"
                # Specify input colorspace as sRGB with -ics sRGB
                AdditionalFlags = "-ics sRGB -q pvrtcbest"
            }
        }
    },
    # BC3 albedo image
    @{
        Name = "BC3 albedo image"
        InputFile = $rgbFile
        # Base output name - the tool name will be inserted
        BaseName = "albedo"
        # Format-specific configuration for each tool
        ToolConfig = @{
            DirectXTex = @{
                Format = "BC3_UNORM"
                # Use sRGB for albedo textures
                AdditionalFlags = "-srgbi -srgbo -bc d"
            }
            PVRTexTool = @{
                Format = "BC3,UBN,sRGB"
                # Specify input colorspace as sRGB with -ics sRGB
                AdditionalFlags = "-ics sRGB -q pvrtcbest"
            }
        }
    },
    # BC6H albedo image
    @{
        Name = "BC6H albedo image"
        InputFile = $rgbFile
        # Base output name - the tool name will be inserted
        BaseName = "albedo"
        # Format-specific configuration for each tool
        ToolConfig = @{
            DirectXTex = @{
                Format = "BC6H_UF16"
                # Use sRGB for albedo textures
                AdditionalFlags = "-srgbi -srgbo"
            }
        }
    },
    # BC6HS albedo image
    @{
        Name = "BC6H Signed albedo image"
        InputFile = $rgbFile
        # Base output name - the tool name will be inserted
        BaseName = "albedo"
        # Format-specific configuration for each tool
        ToolConfig = @{
            DirectXTex = @{
                Format = "BC6H_SF16"
                # Use sRGB for albedo textures
                AdditionalFlags = "-srgbi -srgbo"
            }
        }
    },
    # BC7 albedo image
    @{
        Name = "BC7 albedo image"
        InputFile = $rgbFile
        # Base output name - the tool name will be inserted
        BaseName = "albedo"
        # Format-specific configuration for each tool
        ToolConfig = @{
            DirectXTex = @{
                Format = "BC7_UNORM"
                # Use sRGB for albedo textures
                AdditionalFlags = "-srgbi -srgbo -bc x"
            }
        }
    },
    # BC5 normal map image
    @{
        Name = "BC5 normal map"
        InputFile = $normalFile
        # Base output name - the tool name will be inserted
        BaseName = "normal"
        # Format-specific configuration for each tool
        ToolConfig = @{
            DirectXTex = @{
                Format = "BC5_UNORM"
                # For normal maps, we need DX10 header and linear color space
                AdditionalFlags = "-dx10"
            }
            PVRTexTool = @{
                Format = "BC5,UBN,lRGB"
                # For normal maps, explicitly tell it the input is linear with -ics lRGB flag
                AdditionalFlags = "-ics lRGB -q pvrtcbest"
            }
        }
    },
    # BC1 specular map image
    @{
        Name = "BC1 specular map"
        InputFile = $specularFile
        # Base output name - the tool name will be inserted
        BaseName = "specular"
        # Format-specific configuration for each tool
        ToolConfig = @{
            DirectXTex = @{
                Format = "BC1_UNORM"
                # For specular maps, keep in linear color space
                AdditionalFlags = ""
            }
            PVRTexTool = @{
                Format = "BC1,UBN,lRGB"
                # For specular maps, use premultiplied alpha with -p for BC1 format
                AdditionalFlags = "-ics lRGB -p -q pvrtcbest"
            }
        }
    },
    # BC4 specular map image
    @{
        Name = "BC4 specular map"
        InputFile = $specularFile
        # Base output name - the tool name will be inserted
        BaseName = "specular"
        # Format-specific configuration for each tool
        ToolConfig = @{
            DirectXTex = @{
                Format = "BC4_UNORM"
                # For specular maps, keep in linear color space
                AdditionalFlags = ""
            }
            PVRTexTool = @{
                Format = "BC4,UBN,lRGB"
                # For specular maps, use premultiplied alpha with -p for BC1 format
                AdditionalFlags = "-ics lRGB -p -q pvrtcbest"
            }
        }
    }
)

# Define the tools we'll use for compression
$compressionTools = @(
    @{
        Name = "DirectXTex"
    },
    @{
        Name = "PVRTexTool"
    }
)

# Process each compression tool
foreach ($tool in $compressionTools) {
    $toolName = $tool.Name
    
    # Generate compressed images
    Write-Output "

Generating $toolName compressed images..."
    
    # Process each compression task
    foreach ($task in $compressionTasks) {
        Write-Output "Processing $($task.Name)..."
        
        # Get tool-specific configuration
        $toolConfig = $task.ToolConfig[$toolName]

        if ($null -eq $toolConfig) {
            Write-Output "No configuration found for $toolName and $($task.Name)"
            continue
        }
        
        # Determine output filename
        # Format: basename_toolname_format.dds
        # Example: albedo_directxtex_bc1.dds or albedo_pvrtextool_bc3.dds
        $taskFormat = $toolConfig.Format -replace "_UNORM", "" # Remove _UNORM for filename
        $taskFormat = $taskFormat.Split(',')[0].ToLower() # Take first part and lowercase it
        $toolNameLower = $toolName.ToLower()
        $outputFile = "$outputDir\$($task.BaseName)_${toolNameLower}_$taskFormat.dds"
        
        if ($toolName -eq "DirectXTex") {
            # texconv expects the -o parameter to be a directory, not a file
            $cmd = "texconv -f $($toolConfig.Format) -y -nologo $($toolConfig.AdditionalFlags) `"$($task.InputFile)`" -o `"$outputDir`""
            Write-Output "Running: $cmd"
            Invoke-Expression "$cmd"
            
            # Calculate the temporary file name that texconv creates (input filename with .dds extension)
            $baseName = Get-BaseFileName $task.InputFile
            $tempFile = "$outputDir\$baseName.dds"
            
            # Rename the temporary file to the desired output file
            if (Test-Path $tempFile) {
                Move-Item -Path $tempFile -Destination $outputFile -Force
                Write-Output "Created $outputFile"
            }
        }
        elseif ($toolName -eq "PVRTexTool") {
            # PVRTexTool can output directly to the filename
            $cmd = "PVRTexToolCLI -i `"$($task.InputFile)`" -o `"$outputFile`" -f $($toolConfig.Format) $($toolConfig.AdditionalFlags)"
            Write-Output "Running: $cmd"
            try {
                Invoke-Expression "$cmd"
                if (Test-Path $outputFile) {
                    Write-Output "Created $outputFile"
                }
            } catch {
                Write-Output "Error running PVRTexToolCLI: $_"
            }
        }
    }
}

# Note: PVRTexToolCLI doesn't support BC7 format

Write-Output "

Test images generated successfully!"
Write-Output "You can now run the ImageQualityBenchmark test to analyze these images."
