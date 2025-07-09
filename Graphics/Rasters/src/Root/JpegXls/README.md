# JPEG XL (JXL) Next-Generation Image Technical Specification

## Overview

The JPEG XL implementation in the Planet Graphics library provides comprehensive support for the next-generation JPEG XL (JXL) image format, offering superior compression efficiency, wide color gamut support, and advanced features for modern imaging applications. JPEG XL represents a significant advancement over traditional formats, providing substantial benefits for geospatial applications processing large raster datasets.

**Specification Reference**: [GitHub Issue #90](https://github.com/wangkanai/planet/issues/90)
**ISO Standard**: ISO/IEC 18181-1:2022
**Namespace**: `Wangkanai.Graphics.Rasters.JpegXls`
**File Extensions**: `.jxl`
**MIME Type**: `image/jxl`

## Key Features

### Next-Generation Compression
- **Superior Efficiency**: 60-80% smaller files than JPEG at equivalent quality
- **Lossless Compression**: 10-35% smaller than PNG with perfect reconstruction
- **Near-lossless**: Mathematically lossless within specified error bounds
- **Progressive Decoding**: Multi-resolution image loading for streaming applications

### Advanced Image Capabilities
- **High Bit Depths**: 1-32 bits per channel (integer and floating-point)
- **Wide Color Gamuts**: HDR, Rec. 2020, Display P3, and custom color spaces
- **Alpha Channels**: Full transparency support with separate compression
- **Animation Support**: Multi-frame sequences with variable timing
- **Metadata Preservation**: EXIF, XMP, and custom metadata support

### Professional Features
- **Butteraugli Perceptual Quality**: Psychovisual-based quality control
- **Modular Mode**: Advanced entropy coding for maximum compression
- **Container Format**: ISOBMFF-based with rich metadata support
- **Streaming Support**: Process large images without full memory loading

## Architecture

### Core Classes

#### `JpegXlRaster`
```csharp
public class JpegXlRaster : Raster, IJpegXlRaster
{
    // Core properties
    public JpegXlCompressionMode CompressionMode { get; set; }
    public int Effort { get; set; } = 7; // 1-9 encoding effort
    public float Quality { get; set; } = 90.0f; // 0-100 quality
    public float Distance { get; set; } = 1.0f; // Butteraugli distance
    public bool IsProgressive { get; set; } = true;
    public bool LosslessAlpha { get; set; } = true;
    public JpegXlColorSpace ColorSpace { get; set; } = JpegXlColorSpace.sRGB;

    // Metadata access
    public JpegXlMetadata Metadata { get; }

    // Encoding operations
    public Task<byte[]> EncodeAsync(JpegXlEncodingOptions? options = null);
    public byte[] Encode(JpegXlEncodingOptions? options = null);

    // Decoding operations
    public Task DecodeAsync(byte[] jxlData, JpegXlDecodingOptions? options = null);
    public void Decode(byte[] jxlData, JpegXlDecodingOptions? options = null);

    // Quality analysis
    public JpegXlQualityMetrics AnalyzeQuality();
    public float EstimateCompressionRatio();
}
```

#### `JpegXlMetadata`
```csharp
public class JpegXlMetadata : RasterMetadata, IJpegXlMetadata
{
    // Format-specific metadata
    public JpegXlProfile Profile { get; set; }
    public JpegXlColorEncoding ColorEncoding { get; set; }
    public JpegXlIntensityTarget IntensityTarget { get; set; }
    public JpegXlToneMapping ToneMapping { get; set; }

    // Animation support
    public bool IsAnimation { get; set; }
    public JpegXlAnimationFrame[] Frames { get; set; }
    public TimeSpan TotalDuration { get; set; }

    // Advanced features
    public JpegXlBlendInfo BlendInfo { get; set; }
    public byte[]? PreviewImage { get; set; }
    public JpegXlExtraChannels ExtraChannels { get; set; }

    // Container format
    public bool UseContainer { get; set; }
    public JpegXlBox[] Boxes { get; set; }

    // Quality metrics
    public float ButterflyScore { get; set; }
    public JpegXlDistortionMetrics DistortionMetrics { get; set; }
}
```

#### `JpegXlEncodingOptions`
```csharp
public class JpegXlEncodingOptions
{
    // Compression settings
    public JpegXlCompressionMode Mode { get; set; } = JpegXlCompressionMode.Lossy;
    public int Effort { get; set; } = 7; // 1-9, higher = better compression
    public float Quality { get; set; } = 90.0f; // 0-100 for lossy mode
    public float Distance { get; set; } = 1.0f; // Butteraugli distance

    // Format options
    public bool UseContainer { get; set; } = true;
    public bool Progressive { get; set; } = true;
    public bool LosslessAlpha { get; set; } = true;
    public bool Modular { get; set; } = false; // Use modular mode

    // Color management
    public JpegXlColorSpace ColorSpace { get; set; } = JpegXlColorSpace.sRGB;
    public byte[]? IccProfile { get; set; }
    public JpegXlIntensityTarget? IntensityTarget { get; set; }

    // Performance
    public int ThreadCount { get; set; } = Environment.ProcessorCount;
    public bool UseSimd { get; set; } = true;

    // Advanced options
    public bool PreserveMetadata { get; set; } = true;
    public JpegXlPhotonNoiseIso PhotonNoiseIso { get; set; } = JpegXlPhotonNoiseIso.None;
    public float? SaliencyMap { get; set; }
}
```

#### `JpegXlValidator`
```csharp
public static class JpegXlValidator
{
    public static JpegXlValidationResult Validate(IJpegXlRaster jxl);
    public static bool IsValidJxlSignature(byte[] data);
    public static JpegXlFormat DetectJxlFormat(byte[] data); // Codestream vs Container
    public static bool HasValidColorEncoding(JpegXlColorEncoding encoding);
    public static bool IsAnimationValid(JpegXlAnimationFrame[] frames);
    public static JpegXlCompliance CheckCompliance(byte[] jxlData);
}
```

## Usage Examples

### Basic JPEG XL Creation
```csharp
// Create a high-quality JPEG XL image
using var jxl = new JpegXlRaster(3840, 2160, 3);
jxl.Quality = 95.0f;
jxl.Effort = 7;
jxl.CompressionMode = JpegXlCompressionMode.Lossy;
jxl.ColorSpace = JpegXlColorSpace.DisplayP3;

// Encode to JPEG XL format
var jxlData = await jxl.EncodeAsync();
```

### Lossless Compression for Archival
```csharp
// Create lossless JPEG XL for archival purposes
using var archivalJxl = JpegXlExamples.CreateLossless(4096, 3072);
archivalJxl.Effort = 9; // Maximum compression effort
archivalJxl.Modular = true; // Use modular mode for better lossless compression

// Configure for archival quality
var archivalOptions = new JpegXlEncodingOptions
{
    Mode = JpegXlCompressionMode.Lossless,
    Effort = 9,
    UseContainer = true,
    PreserveMetadata = true,
    ThreadCount = Environment.ProcessorCount
};

var archivalData = await archivalJxl.EncodeAsync(archivalOptions);
```

### Web-Optimized Progressive JPEG XL
```csharp
// Create web-optimized progressive JPEG XL
using var webJxl = JpegXlExamples.CreateWebOptimized(1920, 1080);
webJxl.Quality = 85.0f;
webJxl.IsProgressive = true;
webJxl.Effort = 5; // Balanced speed/compression for web

// Web-specific encoding options
var webOptions = new JpegXlEncodingOptions
{
    Mode = JpegXlCompressionMode.Lossy,
    Quality = 85.0f,
    Progressive = true,
    UseContainer = false, // Smaller codestream format
    PreserveMetadata = false, // Remove metadata for smaller size
    ColorSpace = JpegXlColorSpace.sRGB
};

var webData = await webJxl.EncodeAsync(webOptions);
```

### HDR Image Processing
```csharp
// Create HDR JPEG XL with wide color gamut
using var hdrJxl = JpegXlExamples.CreateHdr(3840, 2160, bitDepth: 16);
hdrJxl.ColorSpace = JpegXlColorSpace.Rec2020;
hdrJxl.Quality = 90.0f;

// HDR-specific settings
hdrJxl.Metadata.IntensityTarget = new JpegXlIntensityTarget
{
    TargetNits = 4000.0f,
    RelativeToMaxDisplay = false
};

hdrJxl.Metadata.ToneMapping = new JpegXlToneMapping
{
    IntensityTarget = 255.0f,
    MinNits = 0.01f,
    RelativeToMaxDisplay = true,
    LinearBelow = 0.125f
};

var hdrData = await hdrJxl.EncodeAsync();
```

### Animation Support
```csharp
// Create animated JPEG XL sequence
using var animatedJxl = JpegXlExamples.CreateAnimation(800, 600, frameCount: 30);
animatedJxl.Quality = 80.0f;
animatedJxl.Effort = 6;

// Configure animation frames
animatedJxl.Metadata.IsAnimation = true;
animatedJxl.Metadata.Frames = new JpegXlAnimationFrame[]
{
    new() { Duration = TimeSpan.FromMilliseconds(100), BlendMode = JpegXlBlendMode.Replace },
    new() { Duration = TimeSpan.FromMilliseconds(100), BlendMode = JpegXlBlendMode.Blend }
    // ... additional frames
};

var animationData = await animatedJxl.EncodeAsync();
```

### Professional Photography Workflow
```csharp
// Professional photography with maximum quality
using var professionalJxl = JpegXlExamples.CreateProfessional(6000, 4000);
professionalJxl.Quality = 98.0f;
professionalJxl.Effort = 9;
professionalJxl.ColorSpace = JpegXlColorSpace.ProPhotoRgb;

// Add professional metadata
professionalJxl.Metadata.ExifData.Camera = "Canon EOS R5";
professionalJxl.Metadata.ExifData.Lens = "RF 85mm f/1.2L USM";
professionalJxl.Metadata.ExifData.ISO = 100;
professionalJxl.Metadata.ExifData.Aperture = 1.2f;

// Apply ICC color profile
var professionalProfile = await LoadIccProfileAsync("ProPhoto_RGB.icc");
professionalJxl.Metadata.IccProfile = professionalProfile;

var professionalData = await professionalJxl.EncodeAsync();
```

### Quality Analysis and Optimization
```csharp
using var jxl = new JpegXlRaster(2048, 1536, 3);

// Analyze quality metrics
var qualityMetrics = jxl.AnalyzeQuality();
Console.WriteLine($"Butteraugli score: {qualityMetrics.ButterflyScore:F3}");
Console.WriteLine($"SSIM: {qualityMetrics.SSIM:F4}");
Console.WriteLine($"PSNR: {qualityMetrics.PSNR:F2} dB");

// Optimize for target file size
var targetSize = 1024 * 1024; // 1MB
var optimizedSettings = jxl.OptimizeForFileSize(targetSize);
jxl.Quality = optimizedSettings.Quality;
jxl.Effort = optimizedSettings.Effort;

Console.WriteLine($"Optimized quality: {optimizedSettings.Quality:F1}");
Console.WriteLine($"Estimated compression ratio: {jxl.EstimateCompressionRatio():F1}:1");
```

## Technical Specifications

### Supported JPEG XL Features
- **Baseline Profile**: Essential features for maximum compatibility
- **Progressive Decoding**: Multi-resolution streaming support
- **Animation**: Variable-frame-rate sequences with blending modes
- **Alpha Channels**: Separate compression for transparency
- **Wide Color Gamuts**: HDR and professional color spaces
- **High Bit Depths**: 1-32 bits per channel support

### Compression Modes
- **Lossless**: Perfect reconstruction with superior compression
- **Lossy**: Perceptually optimized compression with Butteraugli
- **Near-lossless**: Mathematically lossless within error bounds
- **Modular**: Advanced entropy coding for specialized content

### Color Spaces and Profiles
- **sRGB**: Standard web color space
- **Display P3**: Wide gamut for modern displays
- **Rec. 2020**: Ultra-wide gamut for HDR content
- **Pro Photo RGB**: Professional photography color space
- **CMYK**: Printing applications
- **Custom ICC**: Arbitrary color profile support

### File Format Variants
- **Codestream**: Direct bitstream encoding (smaller, faster)
- **Container**: ISOBMFF-based with rich metadata support
- **Magic Numbers**:
  - Codestream: `0xFF0A`
  - Container: `0x0000000C4A584C20`

## Performance Characteristics

### Compression Efficiency
- **vs JPEG**: 60-80% smaller at equivalent quality
- **vs PNG**: 10-35% smaller for lossless compression
- **vs WebP**: 20-50% smaller across all quality levels
- **vs AVIF**: Comparable or better compression with faster encoding

### Processing Performance
- **Encoding Speed**: Variable by effort level (1-9)
- **Effort 1**: Fastest encoding, good compression
- **Effort 7**: Balanced speed/compression (default)
- **Effort 9**: Slowest encoding, maximum compression
- **Decoding Speed**: Consistently fast across all quality levels

### Memory Usage
- **Encoding**: 2-4x image size depending on settings
- **Decoding**: 1.5-2x image size for progressive loading
- **Streaming**: Constant memory usage for large images
- **Animation**: Frame-dependent memory allocation

## Constants and Configuration

### JPEG XL Constants
```csharp
public static class JpegXlConstants
{
    // File signatures
    public static readonly byte[] CodestreamSignature = { 0xFF, 0x0A };
    public static readonly byte[] ContainerSignature = { 0x00, 0x00, 0x00, 0x0C, 0x4A, 0x58, 0x4C, 0x20 };

    // Quality ranges
    public const float MinQuality = 0.0f;
    public const float MaxQuality = 100.0f;
    public const float DefaultQuality = 90.0f;

    // Effort levels
    public const int MinEffort = 1;
    public const int MaxEffort = 9;
    public const int DefaultEffort = 7;

    // Distance ranges (Butteraugli)
    public const float MinDistance = 0.0f;
    public const float MaxDistance = 15.0f;
    public const float DefaultDistance = 1.0f;

    // Dimensions
    public const int MaxDimension = 1073741824; // 2^30
    public const int MinDimension = 1;
}
```

### Quality Presets
```csharp
public static class JpegXlQualityPresets
{
    public const float Preview = 50.0f;
    public const float Web = 80.0f;
    public const float Standard = 90.0f;
    public const float Professional = 95.0f;
    public const float NearLossless = 99.0f;
    public const float Lossless = 100.0f;
}
```

### Effort Presets
```csharp
public static class JpegXlEffortPresets
{
    public const int Fastest = 1;
    public const int Fast = 3;
    public const int Balanced = 7;
    public const int Slow = 8;
    public const int Slowest = 9;
}
```

## Validation and Error Handling

### Comprehensive Validation
```csharp
// Validate JPEG XL configuration
var validation = JpegXlValidator.Validate(jxl);
if (!validation.IsValid)
{
    Console.WriteLine($"Validation failed: {validation.GetSummary()}");
    foreach (var error in validation.Errors)
        Console.WriteLine($"Error: {error}");
    foreach (var warning in validation.Warnings)
        Console.WriteLine($"Warning: {warning}");
}

// Check format compliance
var compliance = JpegXlValidator.CheckCompliance(jxlData);
Console.WriteLine($"Compliance level: {compliance.Level}");
Console.WriteLine($"Supported features: {string.Join(", ", compliance.SupportedFeatures)}");
```

### Common Validation Errors
| Error | Description | Solution |
|-------|-------------|----------|
| InvalidDimensions | Dimensions exceed JPEG XL limits | Use smaller dimensions or tile processing |
| InvalidQuality | Quality outside 0-100 range | Use valid quality value |
| InvalidEffort | Effort outside 1-9 range | Use valid effort level |
| UnsupportedColorSpace | Color space not supported | Use supported color space or ICC profile |
| InvalidAnimationFrame | Animation frame configuration error | Fix frame timing and blend modes |

## Testing

### Unit Tests
```csharp
[Fact]
public void Constructor_WithValidDimensions_ShouldInitialize()
{
    using var jxl = new JpegXlRaster(1920, 1080, 3);
    Assert.Equal(1920, jxl.Width);
    Assert.Equal(1080, jxl.Height);
    Assert.Equal(3, jxl.Components);
}

[Theory]
[InlineData(JpegXlCompressionMode.Lossless)]
[InlineData(JpegXlCompressionMode.Lossy)]
[InlineData(JpegXlCompressionMode.NearLossless)]
public void SetCompressionMode_WithValidMode_ShouldSucceed(JpegXlCompressionMode mode)
{
    using var jxl = new JpegXlRaster(800, 600, 3);
    jxl.CompressionMode = mode;

    var validation = JpegXlValidator.Validate(jxl);
    Assert.True(validation.IsValid);
}
```

### Performance Tests
```csharp
[Fact]
public async Task EncodeAsync_WithLargeImage_ShouldCompleteWithinTimeout()
{
    using var jxl = new JpegXlRaster(4096, 4096, 3);
    jxl.Quality = 85.0f;
    jxl.Effort = 7;

    var stopwatch = Stopwatch.StartNew();
    var data = await jxl.EncodeAsync();
    stopwatch.Stop();

    Assert.True(stopwatch.ElapsedMilliseconds < 10000); // 10 second timeout
    Assert.True(data.Length > 0);
}
```

### Compliance Tests
```csharp
[Fact]
public void Encode_ShouldProduceCompliantJxlFile()
{
    using var jxl = JpegXlExamples.CreateStandard(512, 512);
    var data = jxl.Encode();

    var compliance = JpegXlValidator.CheckCompliance(data);
    Assert.Equal(JpegXlComplianceLevel.Full, compliance.Level);
}
```

## Integration with Planet Ecosystem

### Graphics Library Integration
```csharp
// JPEG XL inherits from Raster base class
Raster raster = new JpegXlRaster(2048, 1536, 3);

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

// GeoJXL support for geospatial applications
var geoJxl = JpegXlExamples.CreateGeospatial(4096, 4096, crs: "EPSG:4326");
geoJxl.Metadata.GeospatialMetadata = new GeospatialMetadata
{
    CoordinateReferenceSystem = "EPSG:4326",
    GeoTransform = new[] { -180.0, 0.1, 0.0, 90.0, 0.0, -0.1 }
};

// Integration with map tile generation
var tileJxl = JpegXlExamples.CreateMapTile(256, 256, zoomLevel: 12);
tileJxl.Quality = 85.0f;
tileJxl.Effort = 5; // Balanced for tile generation performance
```

## Best Practices

### Compression Optimization
1. **Use quality 85-90**: Excellent balance for most applications
2. **Effort 7 for production**: Good balance of speed and compression
3. **Lossless for archival**: Perfect quality preservation
4. **Progressive for web**: Better loading experience

### Color Management
1. **sRGB for web**: Maximum compatibility
2. **Display P3 for modern displays**: Wide gamut support
3. **Rec. 2020 for HDR**: Ultra-wide gamut content
4. **ICC profiles for professional**: Accurate color reproduction

### Performance Optimization
1. **Use container format**: Better metadata support
2. **Enable SIMD**: Hardware acceleration when available
3. **Multi-threading**: Parallel processing for large images
4. **Streaming for large files**: Constant memory usage

### Memory Management
1. **Use using statements**: Proper disposal of resources
2. **Async operations**: Non-blocking I/O for large images
3. **Progressive loading**: Incremental image loading
4. **Monitor memory usage**: Check estimated memory requirements

## Contributing

### Development Setup
1. Install .NET 9.0 SDK
2. Clone the repository
3. Navigate to `Graphics/Rasters/src/Root/JpegXls`
4. Install libjxl native library
5. Run `dotnet build` to build the project

### Dependencies
- **libjxl**: Official JPEG XL reference implementation
- **System.Buffers**: Memory management for large images
- **Wangkanai.Graphics.Abstractions**: Core graphics interfaces

### Running Tests
```bash
dotnet test Graphics/Rasters/tests/Unit/JpegXls/
```

### Code Standards
- Follow the coding guidelines in CLAUDE.md
- Use PascalCase for public members
- Include comprehensive XML documentation
- Write unit tests for all public methods
- Use async/await for I/O operations

## Future Enhancements

### Planned Features
1. **GPU Acceleration**: Hardware-accelerated encoding/decoding
2. **Advanced Animation**: Full animation specification support
3. **Streaming API**: Low-latency streaming for real-time applications
4. **SIMD Optimization**: Vector instruction optimization
5. **Color Management**: Advanced color space conversions

### Integration Roadmap
1. **Phase 1**: Core decoder implementation (MVP)
2. **Phase 2**: Full encoder with all compression modes
3. **Phase 3**: Advanced features (animation, HDR, wide gamut)
4. **Phase 4**: Performance optimization and production readiness

## References

- [JPEG XL Official Specification](https://jpeg.org/jpegxl/)
- [libjxl Reference Implementation](https://github.com/libjxl/libjxl)
- [ISO/IEC 18181-1:2022 Standard](https://www.iso.org/standard/77977.html)
- [JPEG XL Whitepaper](https://ds.jpeg.org/whitepapers/jpeg-xl-whitepaper.pdf)
- [Butteraugli Perceptual Metric](https://github.com/google/butteraugli)

This JPEG XL implementation provides a cutting-edge foundation for next-generation image processing with superior compression efficiency, wide color gamut support, and advanced features for the Planet Graphics ecosystem.
