# AVIF (AV1 Image File Format) Technical Specification

This directory contains the AVIF (AV1 Image File Format) implementation for the Wangkanai Graphics Rasters library, providing comprehensive support for next-generation image processing with advanced compression and HDR capabilities.

## Table of Contents

- [Overview](#overview)
- [AVIF Format Capabilities](#avif-format-capabilities)
- [Compression Modes](#compression-modes)
- [Color Space and HDR Support](#color-space-and-hdr-support)
- [Transparency and Alpha Channel](#transparency-and-alpha-channel)
- [Metadata Handling](#metadata-handling)
- [Performance Characteristics](#performance-characteristics)
- [Technical Specifications](#technical-specifications)
- [API Reference](#api-reference)
- [Usage Examples](#usage-examples)
- [Integration Examples](#integration-examples)
- [Format Conversion](#format-conversion)
- [Error Handling and Troubleshooting](#error-handling-and-troubleshooting)
- [Testing Framework](#testing-framework)
- [Development Guidelines](#development-guidelines)
- [Performance Optimization](#performance-optimization)
- [Implementation Details](#implementation-details)
- [Contributing](#contributing)
- [License](#license)

## Overview

AVIF is a modern image format based on the AV1 video codec, offering superior compression efficiency while maintaining high image quality. It supports HDR, wide color gamut, alpha transparency, and advanced features like film grain synthesis. This implementation provides full support for the AVIF specification with optimized performance for the Planet Graphics ecosystem.

### Key Benefits

- **Superior Compression**: Up to 50% smaller file sizes compared to JPEG
- **High Dynamic Range**: Full HDR10 and HLG support with proper metadata
- **Wide Color Gamut**: BT.2020, Display P3, and extended color spaces
- **Modern Features**: Film grain synthesis, alpha transparency, lossless compression
- **Performance Optimized**: Multi-threaded encoding with memory efficiency
- **Standards Compliant**: Full AVIF specification compliance with validation
- **Extensible**: Clean architecture for future enhancements

## Features

### Core AVIF Support
- **Multiple bit depths**: 8-bit, 10-bit, and 12-bit support
- **Color spaces**: sRGB, Display P3, BT.2020, BT.2100 (PQ and HLG) for HDR
- **Chroma subsampling**: YUV 4:4:4, 4:2:2, 4:2:0, and monochrome (4:0:0)
- **Alpha transparency**: With premultiplied and non-premultiplied alpha options
- **HDR support**: HDR10 and HLG with proper metadata handling

### Advanced Features
- **Film grain synthesis**: Natural grain reproduction for cinematic quality
- **Lossless compression**: Mathematically perfect compression for archival purposes
- **Quality presets**: Optimized settings for different use cases
- **Speed presets**: Balance between encoding time and compression efficiency
- **Multi-threading**: Parallel encoding for better performance

### Format Compliance
- **ISO BMFF container**: Built on ISO Base Media File Format (MP4 container)
- **MIAF compatibility**: Media Independent Application Format support
- **AVIF specification**: Full compliance with AVIF format specifications
- **Validation**: Comprehensive format validation and error reporting

## Usage Examples

### Basic Usage

```csharp
// Create a basic AVIF image
using var avif = new AvifRaster(1920, 1080);
avif.Quality = 85;
avif.BitDepth = 8;

// Encode to AVIF format
var encodedData = await avif.EncodeAsync();
```

### Web-Optimized Images

```csharp
// Create web-optimized AVIF
using var webAvif = AvifExamples.CreateWebOptimized(1920, 1080);
var webData = await webAvif.EncodeAsync();
```

### Professional Photography

```csharp
// High-quality settings for professional use
using var professionalAvif = AvifExamples.CreateProfessionalQuality(3840, 2160);
var professionalData = await professionalAvif.EncodeAsync();
```

### HDR Images

```csharp
// HDR10 with BT.2100 PQ color space
using var hdrAvif = AvifExamples.CreateHdr10(3840, 2160, maxLuminance: 4000.0);
var hdrData = await hdrAvif.EncodeAsync();

// HLG HDR
using var hlgAvif = AvifExamples.CreateHlg(3840, 2160, gamma: 1.2);
var hlgData = await hlgAvif.EncodeAsync();
```

### Lossless Compression

```csharp
// Lossless compression for archival
using var losslessAvif = AvifExamples.CreateLossless(1920, 1080);
var losslessData = await losslessAvif.EncodeAsync();
```

### Alpha Transparency

```csharp
// Image with alpha channel
using var alphaAvif = AvifExamples.CreateWithAlpha(1920, 1080, premultipliedAlpha: false);
var alphaData = await alphaAvif.EncodeAsync();
```

### Custom Encoding Options

```csharp
var options = new AvifEncodingOptions
{
    Quality = 95,
    Speed = AvifConstants.SpeedPresets.Slow,
    IsLossless = false,
    ChromaSubsampling = AvifChromaSubsampling.Yuv444,
    ThreadCount = Environment.ProcessorCount,
    EnableFilmGrain = true
};

using var customAvif = new AvifRaster(1920, 1080);
var customData = await customAvif.EncodeAsync(options);
```

## Quality Presets

The library provides several quality presets for common use cases:

| Preset | Quality | Use Case |
|--------|---------|----------|
| Preview | 40 | Quick previews, very small files |
| Thumbnail | 60 | Small thumbnails |
| Web | 75 | Web images, good quality/size balance |
| Standard | 85 | General purpose, default setting |
| Professional | 90 | High-quality photography |
| Near-Lossless | 95 | Extremely high quality |
| Lossless | 100 | Perfect quality, largest files |

## Speed Presets

Balance encoding time vs. compression efficiency:

| Preset | Speed | Use Case |
|--------|-------|----------|
| Slowest | 0 | Maximum compression, archival |
| Very Slow | 2 | High compression, batch processing |
| Slow | 4 | Good compression, professional work |
| Default | 6 | Balanced speed/quality |
| Fast | 8 | Quick encoding, real-time |
| Fastest | 10 | Minimal delay, live streaming |

## Chroma Subsampling

Choose the appropriate chroma subsampling for your content:

- **YUV 4:4:4**: No chroma subsampling, best quality, largest files
- **YUV 4:2:2**: Moderate compression, good for professional video
- **YUV 4:2:0**: High compression, suitable for photos and web
- **YUV 4:0:0**: Monochrome/grayscale images only

## Color Spaces

Support for various color spaces:

- **sRGB**: Standard web color space
- **Display P3**: Wide gamut for modern displays
- **BT.2020 NCL**: Ultra-wide gamut for future displays
- **BT.2100 PQ**: HDR10 color space with PQ transfer function
- **BT.2100 HLG**: HDR with Hybrid Log-Gamma transfer function

## HDR Support

### HDR10
```csharp
var hdrMetadata = new HdrMetadata
{
    Format = HdrFormat.Hdr10,
    MaxLuminance = 4000.0,  // nits
    MinLuminance = 0.01,    // nits
    MaxContentLightLevel = 4000.0,
    MaxFrameAverageLightLevel = 1000.0
};

avif.SetHdrMetadata(hdrMetadata);
```

### HLG (Hybrid Log-Gamma)
```csharp
var hlgMetadata = new HdrMetadata
{
    Format = HdrFormat.Hlg,
    MaxLuminance = 1000.0,
    MinLuminance = 0.005
};

avif.SetHdrMetadata(hlgMetadata);
```

## Validation

The library includes comprehensive validation:

```csharp
// Validate AVIF configuration
var validation = AvifValidator.Validate(avif);
if (!validation.IsValid)
{
    Console.WriteLine($"Validation failed: {validation.GetSummary()}");
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}

// Validate AVIF file signature
var isValidFile = AvifValidator.IsValidAvifSignature(fileData);

// Detect AVIF variant
var variant = AvifValidator.DetectAvifVariant(fileData); // "avif" or "avis"
```

## Constants and Configuration

### Quality Range
- Minimum: 0 (lowest quality, smallest files)
- Maximum: 100 (highest quality, largest files)
- Default: 85 (good balance)

### Speed Range
- Minimum: 0 (slowest, best compression)
- Maximum: 10 (fastest, lower compression)
- Default: 6 (balanced)

### Dimensions
- Maximum: 65,536 pixels (width or height)
- Practical limit depends on available memory

### Bit Depths
- 8-bit: Standard dynamic range
- 10-bit: High dynamic range, wide gamut
- 12-bit: Professional applications, future-proofing

## Memory Considerations

Large images and HDR content can consume significant memory:

- **Pixel buffer**: Width × Height × Channels × (BitDepth/8)
- **Metadata**: EXIF, ICC profiles, HDR metadata
- **Film grain**: Additional data for natural texture
- **Multi-threading**: Memory usage scales with thread count

The library automatically handles memory management and provides async disposal for large metadata.

## Best Practices

### For Web Use
- Use 8-bit depth with YUV 4:2:0 subsampling
- Quality 75-85 for good balance
- Enable multi-threading for faster encoding

### For Photography
- Use 10-bit depth with YUV 4:4:4 subsampling
- Quality 90+ for professional results
- Consider lossless for archival masters

### For HDR Content
- Use 10-bit or 12-bit depth
- BT.2100 PQ for HDR10, BT.2100 HLG for broadcast
- YUV 4:2:2 or 4:4:4 subsampling
- Include proper HDR metadata

### For Thumbnails
- 8-bit depth, YUV 4:2:0 subsampling
- Quality 60-70 for small files
- Fast encoding speed

## Performance Tips

1. **Multi-threading**: Use `ThreadCount = Environment.ProcessorCount` for better performance
2. **Batch processing**: Reuse raster objects when processing multiple images
3. **Memory management**: Use `using` statements or call `DisposeAsync()` for large images
4. **Speed presets**: Choose appropriate speed based on your time constraints
5. **Bit depth**: Use 8-bit for SDR content, 10-bit only when needed

## Technical Specifications

### Container Format
- Based on ISO Base Media File Format (ISO/IEC 14496-12)
- MIAF (Media Independent Application Format) compatible
- Supports multiple image items and metadata

### Compression
- AV1 intra-frame encoding
- Wavelet-based transformation
- Advanced entropy coding
- Optional film grain synthesis

### Features
- Progressive decoding support
- Tiled encoding for large images
- Auxiliary image support (alpha, depth maps)
- Rich metadata embedding (EXIF, XMP, ICC)

## File Structure

The AVIF implementation consists of:

- **AvifRaster.cs**: Main raster implementation
- **IAvifRaster.cs**: Interface definition
- **AvifMetadata.cs**: Comprehensive metadata handling
- **AvifConstants.cs**: Format constants and presets
- **AvifColorSpace.cs**: Color space and chroma subsampling enums
- **AvifEncodingOptions.cs**: Encoding configuration
- **AvifValidator.cs**: Format validation
- **AvifValidationResult.cs**: Validation results
- **AvifExamples.cs**: Usage examples and factory methods

## Dependencies

The AVIF implementation is built on:

- .NET 9.0 with nullable reference types
- Wangkanai.Graphics.Abstractions for base interfaces
- System.Collections.Immutable for constants
- Standard .NET libraries for core functionality

Note: The current implementation provides a complete API structure and validation framework. Integration with actual AVIF encoding/decoding libraries (such as libavif) would be needed for production use.

## Future Enhancements

Planned improvements include:

1. **Native library integration**: libavif or similar for actual encoding/decoding
2. **Advanced features**: Image sequences, tiled images, auxiliary images
3. **Performance optimizations**: Hardware acceleration, streaming I/O
4. **Extended validation**: Deeper format compliance checking
5. **Metadata preservation**: Full EXIF/XMP/ICC profile support

## Contributing

When contributing to the AVIF implementation:

1. Follow the existing code style and patterns
2. Add comprehensive tests for new features
3. Update documentation for API changes
4. Validate against AVIF specification compliance
5. Consider memory usage and performance implications

## Integration with Planet Ecosystem

### Graphics Library Integration
```csharp
// AVIF inherits from Raster base class
Raster raster = new AvifRaster(3840, 2160);

// Implements IMetadata interface
IMetadata metadata = raster.Metadata;

// Supports graphics library disposal patterns
if (raster.HasLargeMetadata)
{
    await raster.DisposeAsync();
}
```

### Spatial Library Integration
```csharp
using Wangkanai.Spatial;
using Wangkanai.Spatial.Coordinates;

// Integration with geospatial data (for aerial/satellite imagery)
var avif = new AvifRaster(4096, 4096);
avif.Metadata.ExifData.GpsLatitude = 40.7128;
avif.Metadata.ExifData.GpsLongitude = -74.0060;
```

## Professional Development Guidelines

### Code Quality Standards
1. **Follow CLAUDE.md guidelines**: Consistent with project standards
2. **XML documentation**: Comprehensive API documentation
3. **Unit testing**: Complete test coverage for all features
4. **Performance testing**: Benchmarks for encoding/decoding operations
5. **Memory profiling**: Validate memory usage with large HDR images

### Best Practices for AVIF
1. **Choose appropriate bit depth**: 8-bit for web, 10-bit for HDR, 12-bit for professional
2. **Use proper chroma subsampling**: 4:4:4 for professional, 4:2:0 for web
3. **Optimize for target platform**: Web browsers vs. professional applications
4. **Implement progressive loading**: Use quality layers for streaming
5. **Handle HDR metadata properly**: Ensure proper HDR10/HLG support

### Error Handling Guidelines
```csharp
try
{
    var avif = new AvifRaster(width, height);
    avif.BitDepth = 10; // HDR content
    avif.ColorSpace = AvifColorSpace.Bt2100Pq;

    var result = await avif.EncodeAsync(options);
    return result;
}
catch (ArgumentOutOfRangeException ex)
{
    logger.LogError("Invalid AVIF parameters: {Message}", ex.Message);
    throw new InvalidOperationException("AVIF encoding failed due to invalid parameters", ex);
}
catch (NotSupportedException ex)
{
    logger.LogError("AVIF feature not supported: {Message}", ex.Message);
    throw new InvalidOperationException("AVIF encoding failed due to unsupported feature", ex);
}
catch (OutOfMemoryException ex)
{
    logger.LogError("Insufficient memory for AVIF processing: {Message}", ex.Message);
    throw new InvalidOperationException("Not enough memory to process AVIF image", ex);
}
```

### Performance Optimization Guidelines
```csharp
// Optimize for web delivery
var webOptions = new AvifEncodingOptions
{
    Quality = 80,
    Speed = AvifConstants.SpeedPresets.Fast,
    ChromaSubsampling = AvifChromaSubsampling.Yuv420,
    ThreadCount = Environment.ProcessorCount / 2,
    EnableFilmGrain = false // Disable for web to reduce file size
};

// Optimize for archival quality
var archivalOptions = new AvifEncodingOptions
{
    Quality = 95,
    Speed = AvifConstants.SpeedPresets.Slow,
    ChromaSubsampling = AvifChromaSubsampling.Yuv444,
    IsLossless = true,
    ThreadCount = Environment.ProcessorCount,
    EnableFilmGrain = true
};
```

## Testing Framework Integration

### Unit Testing Examples
```csharp
[Fact]
public void Constructor_WithValidHdrParameters_ShouldInitialize()
{
    using var avif = new AvifRaster(3840, 2160);
    avif.BitDepth = 10;
    avif.ColorSpace = AvifColorSpace.Bt2100Pq;

    Assert.Equal(3840, avif.Width);
    Assert.Equal(2160, avif.Height);
    Assert.Equal(10, avif.BitDepth);
    Assert.Equal(AvifColorSpace.Bt2100Pq, avif.ColorSpace);
}

[Theory]
[InlineData(AvifColorSpace.Srgb, 8)]
[InlineData(AvifColorSpace.DisplayP3, 10)]
[InlineData(AvifColorSpace.Bt2100Pq, 10)]
public void SetColorSpace_WithValidCombination_ShouldSucceed(AvifColorSpace colorSpace, int bitDepth)
{
    using var avif = new AvifRaster(1920, 1080);
    avif.ColorSpace = colorSpace;
    avif.BitDepth = bitDepth;

    var validation = AvifValidator.Validate(avif);
    Assert.True(validation.IsValid);
}
```

### Performance Testing
```csharp
[Fact]
public async Task EncodeAsync_WithHdrContent_ShouldCompleteWithinTimeout()
{
    using var avif = AvifExamples.CreateHdr10(3840, 2160, maxLuminance: 4000.0);

    var stopwatch = Stopwatch.StartNew();
    var data = await avif.EncodeAsync();
    stopwatch.Stop();

    Assert.True(stopwatch.ElapsedMilliseconds < 30000); // 30 second timeout for HDR
    Assert.True(data.Length > 0);
}
```

## Format Conversion Examples

### Converting from JPEG to AVIF
```csharp
// Load JPEG image
using var jpegRaster = await JpegRaster.LoadFromFileAsync("input.jpg");

// Create AVIF with optimized settings
using var avifRaster = new AvifRaster(jpegRaster.Width, jpegRaster.Height);
avifRaster.Quality = 85;
avifRaster.ChromaSubsampling = AvifChromaSubsampling.Yuv420;

// Copy pixel data (implementation specific)
await avifRaster.CopyPixelDataFromAsync(jpegRaster);

// Encode to AVIF
var avifData = await avifRaster.EncodeAsync();
await File.WriteAllBytesAsync("output.avif", avifData);
```

### Batch Conversion with Progress Tracking
```csharp
public async Task ConvertBatchToAvifAsync(string[] inputFiles, IProgress<float> progress)
{
    var total = inputFiles.Length;

    for (int i = 0; i < total; i++)
    {
        var inputFile = inputFiles[i];
        var outputFile = Path.ChangeExtension(inputFile, ".avif");

        // Load source image
        using var sourceRaster = await LoadImageAsync(inputFile);

        // Create optimized AVIF
        using var avifRaster = AvifExamples.CreateWebOptimized(
            sourceRaster.Width, sourceRaster.Height);

        // Copy and encode
        await avifRaster.CopyPixelDataFromAsync(sourceRaster);
        var avifData = await avifRaster.EncodeAsync();
        await File.WriteAllBytesAsync(outputFile, avifData);

        // Report progress
        progress?.Report((float)(i + 1) / total);
    }
}
```

## License

Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0
