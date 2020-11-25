[![Nuget](https://img.shields.io/nuget/v/BCnEncoder.Net)](https://www.nuget.org/packages/BCnEncoder.Net/)
![Tests](https://github.com/Nominom/BCnEncoder.NET/workflows/Tests/badge.svg)

# BCnEncoder.NET
A Cross-platform BCn / DXT encoding libary for .NET

# What is it?
BCnEncoder.NET is a library for compressing rgba images to different block-compressed formats. It has no native dependencies and is .NET Standard 2.1 compatible.

Supported formats are:
 - Raw unsigned byte R, RG, RGB and RGBA formats
 - BC1 (S3TC DXT1)
 - BC2 (S3TC DXT3)
 - BC3 (S3TC DXT5)
 - BC4 (RGTC1)
 - BC5 (RGTC2)
 - BC7 (BPTC)

# Current state
The current state of this library is in development but quite usable. I'm planning on implementing support for more codecs and 
different algorithms. The current version is capable of encoding and decoding BC1-BC5 and BC7 images in both KTX or DDS formats.

Please note, that the API might change between versions. I'm still trying to figure it out as I go.
# Dependencies
Current dependencies are:
* [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) licenced under the [Apache 2.0](https://www.apache.org/licenses/LICENSE-2.0) licence for image loading and saving

# API
For more detailed usage examples, you can go look at the unit tests. 

Here's an example on how to encode a png image to BC1 without alpha, and save it to a file.
```CSharp
using Image<Rgba32> image = Image.Load<Rgba32>("example.png");

BcEncoder encoder = new BcEncoder();

encoder.OutputOptions.generateMipMaps = true;
encoder.OutputOptions.quality = CompressionQuality.Balanced;
encoder.OutputOptions.format = CompressionFormat.BC1;
encoder.OutputOptions.fileFormat = OutputFileFormat.Ktx; //Change to Dds for a dds file.

using FileStream fs = File.OpenWrite("example.ktx");
encoder.Encode(image, fs);
```

And how to decode a compressed image from a KTX file and save it to png format.
```CSharp
using FileStream fs = File.OpenRead("compressed_bc1.ktx");

BcDecoder decoder = new BcDecoder();
using Image<Rgba32> image = decoder.Decode(fs);

using FileStream outFs = File.OpenWrite("decoding_test_bc1.png");
image.SaveAsPng(outFs);
```

# TO-DO

- [x] BC1 / DXT1 Encoding Without Alpha
- [x] BC1 / DXT1 Encoding With 1bit of alpha
- [x] BC2 / DXT3 Encoding
- [x] BC3 / DXT5 Encoding
- [x] BC4 Encoding
- [x] BC5 Encoding
- [x] BC7 / BPTC Encoding
- [x] DDS file support
- [x] Implement PCA to remove Accord.Statistics dependency
- [ ] BC6H HDR Encoding
- [ ] ETC / ETC2 Encoding?
- [ ] Implement saving and loading basic image formats to remove ImageSharp dependency

# Contributing
All contributions are welcome. I'll try to respond to bug reports and feature requests as fast as possible, but you can also fix things yourself and submit a pull request. Please note, that by submitting a pull request you accept that your code will be dual licensed under MIT and public domain Unlicense.

# License
This library is dual-licensed under the [Unlicense](https://unlicense.org/), and [MIT](https://opensource.org/licenses/MIT) licenses.

You may use this code under the terms of either license.

Please note, that any dependencies of this project are licensed under their own respective licenses.
