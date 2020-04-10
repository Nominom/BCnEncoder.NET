# BCnEncoder.NET
A Cross-platform BCn / DXT encoding libary for .NET

# What is it?
This library is my attempt at writing a texture block compression library. 

# Current state
The current state of this library is in its baby shoes, but I'm planning on implementing support for more codecs and 
different algorithms. The current version is capable of encoding BC1 / DXT1 without alpha, and decoding BC1 images stored in KTX format.
The library can save compressed images to KTX format but I might implement DDS support later.

# Dependencies
Current dependencies are:
* [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) licenced under the [Apache 2.0](https://www.apache.org/licenses/LICENSE-2.0) licence for image loading and saving
* [Accord.Statistics](http://accord-framework.net/) licenced under the [LGPL 2.1](https://www.gnu.org/licenses/old-licenses/lgpl-2.1.html) licence for Principal Component Analysis (PCA).

# API
For more detailed usage examples, you can go look at the unit tests. 

Here's an example on how to encode a png image to BC1 without alpha, and save it to a file.
```CSharp
using Image<Rgba32> image = Image.Load<Rgba32>("example.png");

BcEncoder encoder = new BcEncoder();

encoder.OutputOptions.generateMipMaps = true;
encoder.OutputOptions.quality = EncodingQuality.Balanced;
encoder.OutputOptions.format = CompressionFormat.BC1;

using FileStream fs = File.OpenWrite("example.ktx");
encoder.Encode(image, fs);
```

And how to decode a compressed image from a KTX file and save it to png format.
```CSharp
using FileStream fs = File.OpenRead("compressed_bc1.ktx");
KtxFile file = KtxFile.Load(fs);

BcDecoder decoder = new BcDecoder();
using Image<Rgba32> image = decoder.Decode(file);

using FileStream outFs = File.OpenWrite("decoding_test_bc1.png");
image.SaveAsPng(outFs);
```

# TO-DO

- [x] BC1 / DXT1 Encoding Without Alpha
- [x] BC1 / DXT1 Encoding With 1bit of alpha
- [ ] BC2 / DXT2 & DXT3 Encoding
- [ ] BC3 / DXT4 & DXT5 Encoding
- [ ] Implement PCA to remove Accord.Statistics dependecy
- [ ] Implement saving and loading basic image formats to remove ImageSharp dependency

# License
This library is licenced under the [Unlicense](https://unlicense.org/), which means that it's under public domain. 
Please note, that any dependencies of this project are licensed under their own respective licenses.
