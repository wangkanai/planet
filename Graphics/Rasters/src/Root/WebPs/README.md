# WebP Technical Specification

This directory contains the WebP format implementation for the Wangkanai Graphics Rasters library, providing
comprehensive support for WebP image processing with high-performance optimizations.

## Table of Contents

- [Overview](#overview)
- [WebP Format Capabilities](#webp-format-capabilities)
- [Compression Modes](#compression-modes)
- [Animation Support](#animation-support)
- [Transparency Support](#transparency-support)
- [Metadata Handling](#metadata-handling)
- [Performance Characteristics](#performance-characteristics)
- [API Reference](#api-reference)
- [Usage Examples](#usage-examples)
- [Implementation Details](#implementation-details)
- [Testing](#testing)
- [Contributing](#contributing)

## Overview

WebP is a modern image format developed by Google that provides superior compression compared to JPEG and PNG while
maintaining high image quality. This implementation supports all WebP features including lossy and lossless compression,
animation, transparency, and comprehensive metadata handling.

### Key Features

- **Lossy Compression (VP8)**: Smaller file sizes with quality control
- **Lossless Compression (VP8L)**: Perfect quality preservation
- **Animation Support**: Multi-frame animations with timing control
- **Alpha Channel**: Full transparency support
- **Metadata**: EXIF, XMP, and ICC profile support
- **Performance Optimized**: High-performance implementation with memory efficiency
- **Validation**: Comprehensive validation and error handling

## WebP Format Capabilities

### Format Types

The implementation supports three WebP format types:

| Format       | Description                   | Use Case                    |
|--------------|-------------------------------|-----------------------------|
| **Simple**   | Basic VP8 lossy compression   | Standard web images         |
| **Lossless** | VP8L lossless compression     | High-quality preservation   |
| **Extended** | VP8X with additional features | Animations, metadata, alpha |

### Dimensional Constraints

- **Width Range**: 1 to 16,383 pixels
- **Height Range**: 1 to 16,383 pixels
- **Bit Depth**: 8 bits per channel (fixed)
- **Color Channels**: 3 (RGB) or 4 (RGBA)

## Compression Modes

### Lossy Compression (VP8)

```csharp
// Create lossy WebP with quality control
var webp = new WebPRaster(800, 600);
webp.ConfigureLossy(85); // Quality 0-100
```

**Quality Guidelines:**

- **100**: Near-lossless (archival quality)
- **90**: Excellent (professional photography)
- **85**: High quality (detailed images)
- **75**: Good quality (default web quality)
- **60**: Medium quality (thumbnails)
- **40**: Low quality (high compression)

### Lossless Compression (VP8L)

```csharp
// Create lossless WebP with compression level control
var webp = new WebPRaster(800, 600);
webp.ConfigureLossless();
webp.CompressionLevel = 6; // 0-9 (0=fast, 9=best compression)
```

**Compression Level Impact:**

- **0-3**: Fast encoding, moderate compression
- **4-6**: Balanced encoding speed and compression
- **7-9**: Slow encoding, maximum compression

## Animation Support

WebP supports multi-frame animations with sophisticated timing and blending controls:

```csharp
// Create animated WebP
var webp = WebPExamples.CreateAnimated(400, 300, loops: 0); // 0 = infinite loops

// Add animation frames
webp.WebPMetadata.AnimationFrames.Add(new WebPAnimationFrame
{
    Width = 400,
    Height = 300,
    Duration = 100, // milliseconds
    DisposalMethod = WebPDisposalMethod.Background,
    BlendingMethod = WebPBlendingMethod.AlphaBlend,
    Data = frameData
});
```

### Animation Properties

- **Loops**: 0 (infinite) to 65,535
- **Frame Duration**: Milliseconds per frame
- **Disposal Methods**: None, Background
- **Blending Methods**: Alpha blend, No blend
- **Background Color**: RGBA background for transparent frames

## Transparency Support

### Alpha Channel Support

```csharp
// Create WebP with alpha channel
var webp = WebPExamples.CreateWithAlpha(800, 600, quality: 85);
webp.SetColorMode(WebPColorMode.Rgba); // 4 channels (RGBA)
```

### Transparency Features

- **RGBA Color Mode**: Direct alpha channel support
- **Separate Alpha Channel**: VP8X format with separate alpha
- **Alpha Preprocessing**: Optimized alpha compression
- **Transparency Metadata**: Alpha channel presence flags

## Metadata Handling

### Supported Metadata Standards

```csharp
var webp = new WebPRaster(800, 600);
webp.EnableExtendedFeatures(); // Required for metadata

// EXIF metadata
webp.WebPMetadata.ExifData = exifBytes;
webp.WebPMetadata.HasExif = true;

// XMP metadata
webp.WebPMetadata.XmpData = xmpString;
webp.WebPMetadata.HasXmp = true;

// ICC color profile
webp.WebPMetadata.IccProfile = iccBytes;
webp.WebPMetadata.HasIccProfile = true;
```

### Metadata Types

| Type       | Description                 | Format       |
|------------|-----------------------------|--------------|
| **EXIF**   | Camera and image metadata   | Binary data  |
| **XMP**    | Extensible metadata         | UTF-8 string |
| **ICC**    | Color profile               | Binary data  |
| **Custom** | Application-specific chunks | Binary data  |

### Standard Properties

- **Title**: Image title
- **Artist**: Creator name
- **Copyright**: Copyright notice
- **Description**: Image description
- **Software**: Creating software
- **Creation Date**: Image creation timestamp

## Performance Characteristics

### Memory Optimization

The implementation includes several performance optimizations:

```csharp
// Large metadata disposal optimization
if (webp.WebPMetadata.HasLargeMetadata)
{
    await webp.DisposeAsync(); // Asynchronous disposal for large datasets
}

// Batch processing for animation frames
const int batchSize = 50;
for (int i = 0; i < frames.Count; i += batchSize)
{
    // Process frames in batches to manage memory
}
```

### Performance Guidelines

- **Large Images**: Use lower quality for images > 4MP
- **Animation**: Limit to < 100 frames for optimal performance
- **Metadata**: Monitor total metadata size < 1MB
- **Async Disposal**: Use for large datasets > 10MB

### Compression Ratios

| Content Type    | Lossy Ratio  | Lossless Ratio |
|-----------------|--------------|----------------|
| **Photography** | 8:1 to 20:1  | 2:1 to 3:1     |
| **Graphics**    | 10:1 to 30:1 | 3:1 to 5:1     |
| **Screenshots** | 15:1 to 40:1 | 4:1 to 8:1     |

## API Reference

### Core Classes

- **`WebPRaster`**: Main WebP implementation class
- **`WebPMetadata`**: Metadata container with WebP-specific properties
- **`WebPValidator`**: Validation and error checking
- **`WebPExamples`**: Usage examples and factory methods

### Key Interfaces

- **`IWebPRaster`**: WebP-specific raster interface
- **`IRaster`**: Base raster interface
- **`IMetadata`**: Base metadata interface

### Enumerations

- **`WebPFormat`**: Simple, Lossless, Extended
- **`WebPCompression`**: VP8, VP8L
- **`WebPColorMode`**: RGB, RGBA
- **`WebPPreset`**: Default, Picture, Photo, Drawing, Icon, Text

## Usage Examples

### Basic WebP Creation

```csharp
// Create web-optimized WebP
var webp = WebPExamples.CreateWebOptimized(800, 600, quality: 75);

// Create lossless WebP
var lossless = WebPExamples.CreateLossless(800, 600, compressionLevel: 6);

// Create WebP with alpha
var alpha = WebPExamples.CreateWithAlpha(800, 600, quality: 85);
```

### Content-Optimized WebP

```csharp
// Photography
var photo = WebPExamples.CreateForPhotography(1920, 1080);

// Graphics and drawings
var drawing = WebPExamples.CreateForDrawing(800, 600);

// Icons
var icon = WebPExamples.CreateIcon(256); // 256x256 icon

// Text content
var text = WebPExamples.CreateForText(800, 600);
```

### Performance-Optimized WebP

```csharp
// Large image optimization
var optimized = WebPExamples.CreatePerformanceOptimized(4000, 3000);

// Custom optimization
var custom = new WebPRaster(2000, 1500);
if (custom.Width * custom.Height > 4_000_000)
{
    custom.ConfigureLossy(70); // Lower quality for large images
}
```

### Animation Creation

```csharp
// Create animated WebP
var animated = WebPExamples.CreateAnimated(400, 300, loops: 0);

// Add frames
for (int i = 0; i < frameCount; i++)
{
    animated.WebPMetadata.AnimationFrames.Add(new WebPAnimationFrame
    {
        Width = 400,
        Height = 300,
        Duration = 100,
        DisposalMethod = WebPDisposalMethod.Background,
        BlendingMethod = WebPBlendingMethod.AlphaBlend,
        Data = GetFrameData(i)
    });
}
```

### Comprehensive Metadata

```csharp
var webp = WebPExamples.CreateWithMetadata(800, 600);

// Set metadata
webp.WebPMetadata.Title = "Sample Image";
webp.WebPMetadata.Artist = "Photographer Name";
webp.WebPMetadata.Copyright = "Copyright 2025";
webp.WebPMetadata.Description = "Sample WebP image";
webp.WebPMetadata.CreationDateTime = DateTime.UtcNow;

// Add technical metadata
webp.WebPMetadata.IccProfile = colorProfile;
webp.WebPMetadata.ExifData = exifData;
webp.WebPMetadata.XmpData = xmpData;
```

### Validation

```csharp
// Validate WebP
var result = webp.Validate();

if (!result.IsValid)
{
    Console.WriteLine($"Validation failed with {result.Errors.Count} errors:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  - {error}");
    }
}

if (result.Warnings.Count > 0)
{
    Console.WriteLine($"Warnings ({result.Warnings.Count}):");
    foreach (var warning in result.Warnings)
    {
        Console.WriteLine($"  - {warning}");
    }
}
```

## Implementation Details

### Architecture

The WebP implementation follows a layered architecture:

1. **Interface Layer**: `IWebPRaster` defines the contract
2. **Implementation Layer**: `WebPRaster` provides the core functionality
3. **Metadata Layer**: `WebPMetadata` handles format-specific metadata
4. **Validation Layer**: `WebPValidator` provides comprehensive validation
5. **Utility Layer**: `WebPExamples` provides usage patterns

### Key Design Patterns

- **Factory Pattern**: `WebPExamples` for common configurations
- **Strategy Pattern**: Different compression strategies
- **Validation Pattern**: Comprehensive error checking
- **Dispose Pattern**: Proper resource management

### Memory Management

- **Async Disposal**: For large datasets
- **Batch Processing**: For animation frames
- **Memory Monitoring**: Threshold-based optimizations
- **Resource Cleanup**: Proper disposal of managed resources

### Thread Safety

The implementation is **not thread-safe** by design for performance reasons. Use appropriate synchronization when
accessing WebP objects from multiple threads.

## Testing

### Test Categories

The test suite covers:

1. **Unit Tests**: Individual component testing
2. **Integration Tests**: Component interaction testing
3. **Performance Tests**: Benchmarking and optimization
4. **Validation Tests**: Error handling and edge cases

### Test Files

- **`WebPRasterTests.cs`**: Core functionality tests
- **`WebPMetadataTests.cs`**: Metadata handling tests
- **`WebPValidatorTests.cs`**: Validation logic tests
- **`WebPConstantsTests.cs`**: Constants validation tests
- **`WebPExamplesTests.cs`**: Usage example tests

### Running Tests

```bash
# Run all WebP tests
dotnet test --filter "WebP"

# Run specific test class
dotnet test --filter "WebPRasterTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Coverage

The test suite aims for comprehensive coverage:

- **Line Coverage**: > 90%
- **Branch Coverage**: > 85%
- **Function Coverage**: 100%

## Contributing

### Development Guidelines

1. **Code Style**: Follow C# coding conventions
2. **Documentation**: Document all public APIs
3. **Testing**: Write tests for all new features
4. **Performance**: Consider performance implications
5. **Validation**: Add appropriate validation

### Contribution Process

1. **Fork**: Create a fork of the repository
2. **Branch**: Create a feature branch
3. **Implement**: Add your changes with tests
4. **Validate**: Ensure all tests pass
5. **Pull Request**: Submit a pull request

### Performance Considerations

When contributing:

- **Memory Usage**: Monitor allocation patterns
- **CPU Usage**: Profile performance-critical paths
- **Large Data**: Consider async patterns for large datasets
- **Validation**: Balance thoroughness with performance

### Code Review Checklist

- [ ] Code follows established patterns
- [ ] All public APIs are documented
- [ ] Tests cover new functionality
- [ ] Performance impact is considered
- [ ] Validation is appropriate
- [ ] Memory management is correct
- [ ] Error handling is comprehensive

## Resources

### WebP Specification

- [WebP Container Specification](https://developers.google.com/speed/webp/docs/riff_container)
- [VP8 Specification](https://tools.ietf.org/html/rfc6386)
- [VP8L Specification](https://developers.google.com/speed/webp/docs/webp_lossless_bitstream_specification)

### Related Documentation

- [Graphics Rasters Documentation](../README.md)
- [Metadata Handling Guide](../Metadatas/README.md)
- [Performance Optimization Guide](../../../docs/Performance.md)

---

*This implementation is part of the Wangkanai Graphics Rasters library, providing comprehensive WebP format support with
high-performance optimizations.*
