# JPEG (Joint Photographic Experts Group) Technical Specification

## Overview

The JPEG implementation in the Planet Graphics library provides comprehensive support for the JPEG/JFIF image format
with advanced metadata handling, progressive encoding, and optimization capabilities.
This implementation is designed for high-performance image processing with extensive support for photography, web
applications, and professional imaging workflows.

## Key Features

### Core JPEG Capabilities

- **DCT-based compression**: Discrete Cosine Transform with configurable quality levels
- **Progressive JPEG**: Interlaced encoding for improved web loading experience
- **Subsampling modes**: 4:4:4, 4:2:2, 4:2:0 chroma subsampling for size optimization
- **Color spaces**: RGB, YCbCr, CMYK, and Grayscale support
- **Quality control**: Precision quality settings from 1-100 with custom quantization tables

### Advanced Metadata Support

- **EXIF data**: Complete EXIF 2.3 specification support with GPS coordinates
- **XMP metadata**: Extensible Metadata Platform for rich descriptive information
- **ICC profiles**: Color management with embedded ICC color profiles
- **JFIF headers**: Standard JPEG File Interchange Format support
- **Adobe markers**: Adobe-specific markers for CMYK and color space information

### Performance Optimizations

- **Memory efficiency**: Intelligent memory management with streaming support
- **Async operations**: Non-blocking I/O for better application responsiveness
- **Batch processing**: Optimized for processing multiple images
- **Quality estimation**: Predictive quality metrics for compression planning

## Architecture

### Core Classes

#### `JpegRaster`

```csharp
public class JpegRaster : Raster, IJpegRaster
{
    // Core properties
    public int Quality { get; set; }
    public JpegSubsampling SubsamplingMode { get; set; }
    public bool IsProgressive { get; set; }
    public JpegColorSpace ColorSpace { get; set; }

    // Metadata access
    public JpegMetadata Metadata { get; }

    // Encoding operations
    public Task<byte[]> EncodeAsync(JpegEncodingOptions? options = null);
    public byte[] Encode(JpegEncodingOptions? options = null);

    // Quality operations
    public float EstimateQuality(byte[] jpegData);
    public JpegQualityMetrics AnalyzeQuality();
}
```

#### `JpegMetadata`

```csharp
public class JpegMetadata : RasterMetadata, IJpegMetadata
{
    // EXIF metadata
    public ExifData ExifData { get; set; }
    public Dictionary<ExifTag, object> CustomExifTags { get; set; }

    // XMP metadata
    public string? XmpData { get; set; }
    public Dictionary<string, string> XmpProperties { get; set; }

    // ICC color profile
    public byte[]? IccProfile { get; set; }
    public string? ColorSpaceName { get; set; }

    // JPEG-specific
    public JpegMarker[] Markers { get; set; }
    public QuantizationTable[] QuantizationTables { get; set; }
    public HuffmanTable[] HuffmanTables { get; set; }

    // Quality metrics
    public float EstimatedQuality { get; set; }
    public JpegQualityMetrics QualityMetrics { get; set; }
}
```

#### `JpegValidator`

```csharp
public static class JpegValidator
{
    public static JpegValidationResult Validate(IJpegRaster jpeg);
    public static bool IsValidJpegSignature(byte[] data);
    public static JpegFormat DetectJpegFormat(byte[] data);
    public static bool HasValidExif(byte[] exifData);
    public static bool HasValidIccProfile(byte[] iccData);
}
```

## Usage Examples

### Basic JPEG Creation

```csharp
// Create a standard JPEG image
using var jpeg = new JpegRaster(1920, 1080, 3);
jpeg.Quality = 85;
jpeg.SubsamplingMode = JpegSubsampling.S420; // 4:2:0 chroma subsampling
jpeg.ColorSpace = JpegColorSpace.YCbCr;

// Encode to JPEG format
var jpegData = await jpeg.EncodeAsync();
```

### High-Quality Photography

```csharp
// Professional photography settings
using var photoJpeg = JpegExamples.CreateProfessionalPhoto(3840, 2160);
photoJpeg.Quality = 95;
photoJpeg.SubsamplingMode = JpegSubsampling.S444; // No chroma subsampling
photoJpeg.IsProgressive = false; // Baseline for maximum compatibility

// Add professional metadata
photoJpeg.Metadata.ExifData.Camera = "Canon EOS R5";
photoJpeg.Metadata.ExifData.Lens = "RF 24-70mm f/2.8L IS USM";
photoJpeg.Metadata.ExifData.ISO = 100;
photoJpeg.Metadata.ExifData.Aperture = 2.8f;
photoJpeg.Metadata.ExifData.ShutterSpeed = "1/125";

var professionalData = await photoJpeg.EncodeAsync();
```

### Web-Optimized JPEG

```csharp
// Web optimization with progressive loading
using var webJpeg = JpegExamples.CreateWebOptimized(1200, 800);
webJpeg.Quality = 80;
webJpeg.IsProgressive = true;
webJpeg.SubsamplingMode = JpegSubsampling.S420;

// Web-specific encoding options
var webOptions = new JpegEncodingOptions
{
    OptimizeHuffmanTables = true,
    RemoveMetadata = true, // Remove EXIF for smaller file size
    EnableArithmeticCoding = false // Better compatibility
};

var webData = await webJpeg.EncodeAsync(webOptions);
```

### EXIF Metadata Handling

```csharp
using var jpeg = new JpegRaster(2048, 1536, 3);

// Add comprehensive EXIF data
jpeg.Metadata.ExifData.DateTime = DateTime.Now;
jpeg.Metadata.ExifData.Artist = "Professional Photographer";
jpeg.Metadata.ExifData.Copyright = "© 2025 Photography Studio";
jpeg.Metadata.ExifData.Software = "Planet Graphics Library";

// GPS coordinates
jpeg.Metadata.ExifData.GpsLatitude = 40.7128;
jpeg.Metadata.ExifData.GpsLongitude = -74.0060;
jpeg.Metadata.ExifData.GpsAltitude = 10.0;

// Camera settings
jpeg.Metadata.ExifData.FocalLength = 50.0f;
jpeg.Metadata.ExifData.FocalLengthIn35mm = 50;
jpeg.Metadata.ExifData.ExposureTime = "1/60";
jpeg.Metadata.ExifData.ExposureMode = ExposureMode.Manual;

// Custom EXIF tags
jpeg.Metadata.CustomExifTags[ExifTag.UserComment] = "Captured in golden hour";
jpeg.Metadata.CustomExifTags[ExifTag.WhiteBalance] = WhiteBalance.Daylight;
```

### Progressive JPEG for Web

```csharp
// Create progressive JPEG with multiple scans
using var progressiveJpeg = JpegExamples.CreateProgressive(1920, 1080);
progressiveJpeg.Quality = 85;

// Configure progressive scan structure
var progressiveOptions = new JpegEncodingOptions
{
    IsProgressive = true,
    ProgressiveScans = new[]
    {
        new JpegScan { ComponentStart = 0, ComponentEnd = 0, SpectralStart = 0, SpectralEnd = 5 },
        new JpegScan { ComponentStart = 1, ComponentEnd = 2, SpectralStart = 0, SpectralEnd = 1 },
        new JpegScan { ComponentStart = 0, ComponentEnd = 0, SpectralStart = 6, SpectralEnd = 63 },
        new JpegScan { ComponentStart = 1, ComponentEnd = 2, SpectralStart = 2, SpectralEnd = 63 }
    }
};

var progressiveData = await progressiveJpeg.EncodeAsync(progressiveOptions);
```

### XMP Metadata Integration

```csharp
using var jpeg = new JpegRaster(1600, 1200, 3);

// Add XMP metadata
jpeg.Metadata.XmpData = @"<?xml version='1.0' encoding='UTF-8'?>
<x:xmpmeta xmlns:x='adobe:ns:meta/'>
  <rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>
    <rdf:Description rdf:about='' xmlns:dc='http://purl.org/dc/elements/1.1/'>
      <dc:title>Professional Landscape Photography</dc:title>
      <dc:creator>John Smith</dc:creator>
      <dc:subject>landscape, nature, mountains</dc:subject>
    </rdf:Description>
  </rdf:RDF>
</x:xmpmeta>";

// Add XMP properties
jpeg.Metadata.XmpProperties["dc:rights"] = "© 2025 John Smith Photography";
jpeg.Metadata.XmpProperties["photoshop:ColorMode"] = "RGB";
jpeg.Metadata.XmpProperties["tiff:Software"] = "Planet Graphics Library";
```

### ICC Color Profile Support

```csharp
using var jpeg = new JpegRaster(2048, 1536, 3);

// Load and apply ICC color profile
var iccProfileData = await File.ReadAllBytesAsync("sRGB_v4_ICC_preference.icc");
jpeg.Metadata.IccProfile = iccProfileData;
jpeg.Metadata.ColorSpaceName = "sRGB IEC61966-2.1";

// Professional color space
var adobeRgbProfile = await LoadAdobeRgbProfileAsync();
jpeg.Metadata.IccProfile = adobeRgbProfile;
jpeg.Metadata.ColorSpaceName = "Adobe RGB (1998)";
```

### Quality Analysis and Optimization

```csharp
using var jpeg = new JpegRaster(1920, 1080, 3);

// Analyze quality metrics
var qualityMetrics = jpeg.AnalyzeQuality();
Console.WriteLine($"Estimated quality: {qualityMetrics.EstimatedQuality}");
Console.WriteLine($"File size: {qualityMetrics.EstimatedFileSize:N0} bytes");
Console.WriteLine($"Compression ratio: {qualityMetrics.CompressionRatio:F1}:1");

// Optimize for target file size
var targetSize = 500 * 1024; // 500KB
var optimizedQuality = jpeg.OptimizeForFileSize(targetSize);
jpeg.Quality = optimizedQuality;

Console.WriteLine($"Optimized quality: {optimizedQuality}");
```

## Technical Specifications

### Supported JPEG Features

- **Baseline JPEG**: Standard DCT-based compression
- **Progressive JPEG**: Multi-scan progressive encoding
- **Extended JPEG**: Support for 12-bit precision
- **Arithmetic coding**: Optional arithmetic entropy coding
- **Restart markers**: Error recovery and parallel processing
- **Custom quantization**: User-defined quantization tables

### Color Spaces and Subsampling

- **RGB**: Direct red, green, blue color space
- **YCbCr**: Luminance and chrominance (standard for JPEG)
- **CMYK**: Cyan, magenta, yellow, key (black) for printing
- **Grayscale**: Single-channel monochrome images

### Subsampling Modes

- **4:4:4**: No subsampling (highest quality)
- **4:2:2**: Horizontal subsampling (good balance)
- **4:2:0**: Horizontal and vertical subsampling (smallest files)
- **4:1:1**: Aggressive subsampling (compatibility mode)

### Quality Levels

- **1-30**: Low quality, small file size
- **31-50**: Medium quality, moderate file size
- **51-80**: Good quality, balanced size
- **81-95**: High quality, larger file size
- **96-100**: Maximum quality, largest file size

## Performance Characteristics

### Memory Usage

- **Baseline encoding**: ~3x image size in memory
- **Progressive encoding**: ~4x image size in memory
- **Metadata overhead**: 1-50KB depending on EXIF/XMP data
- **ICC profiles**: 1-5KB additional overhead

### Processing Speed

- **Baseline JPEG**: Fastest encoding/decoding
- **Progressive JPEG**: ~15% slower encoding, similar decoding
- **High quality (90+)**: Exponentially slower encoding
- **Arithmetic coding**: 10-15% slower than Huffman coding

### File Size Optimization

- **Quality 85**: Good balance for most applications
- **Progressive**: 2-8% smaller files for web
- **Optimized Huffman**: 3-5% smaller files
- **Metadata removal**: 10-50KB reduction

## Constants and Configuration

### JPEG Constants

```csharp
public static class JpegConstants
{
    // File signatures
    public static readonly byte[] JpegSignature = { 0xFF, 0xD8, 0xFF };
    public static readonly byte[] JfifSignature = { 0x4A, 0x46, 0x49, 0x46, 0x00 };
    public static readonly byte[] ExifSignature = { 0x45, 0x78, 0x69, 0x66, 0x00, 0x00 };

    // Quality ranges
    public const int MinQuality = 1;
    public const int MaxQuality = 100;
    public const int DefaultQuality = 85;

    // Dimensions
    public const int MaxDimension = 65535;
    public const int MinDimension = 1;

    // Markers
    public const byte StartOfImage = 0xD8;
    public const byte EndOfImage = 0xD9;
    public const byte StartOfFrame = 0xC0;
    public const byte StartOfScan = 0xDA;
}
```

### Quality Presets

```csharp
public static class JpegQualityPresets
{
    public const int Thumbnail = 60;
    public const int Web = 80;
    public const int Standard = 85;
    public const int Professional = 90;
    public const int Archival = 95;
}
```

## Validation and Error Handling

### Comprehensive Validation

```csharp
// Validate JPEG configuration
var validation = JpegValidator.Validate(jpeg);
if (!validation.IsValid)
{
    Console.WriteLine($"Validation failed: {validation.GetSummary()}");
    foreach (var error in validation.Errors)
        Console.WriteLine($"Error: {error}");
    foreach (var warning in validation.Warnings)
        Console.WriteLine($"Warning: {warning}");
}

// Validate EXIF data
if (jpeg.Metadata.ExifData != null)
{
    var exifValidation = JpegValidator.ValidateExif(jpeg.Metadata.ExifData);
    Console.WriteLine($"EXIF valid: {exifValidation.IsValid}");
}
```

### Common Validation Errors

| Error              | Description                    | Solution                       |
|--------------------|--------------------------------|--------------------------------|
| InvalidDimensions  | Width or height exceeds limits | Resize image or use JPEG2000   |
| InvalidQuality     | Quality outside 1-100 range    | Use valid quality value        |
| InvalidSubsampling | Incompatible subsampling mode  | Choose appropriate subsampling |
| CorruptedExif      | EXIF data structure invalid    | Repair or remove EXIF data     |
| InvalidIccProfile  | ICC profile format error       | Use valid ICC profile          |

## Testing

### Unit Tests

```csharp
[Fact]
public void Constructor_WithValidDimensions_ShouldInitialize()
{
    using var jpeg = new JpegRaster(800, 600, 3);
    Assert.Equal(800, jpeg.Width);
    Assert.Equal(600, jpeg.Height);
    Assert.Equal(3, jpeg.Components);
}

[Theory]
[InlineData(1, 1, 1)]
[InlineData(65535, 65535, 4)]
public void Constructor_WithValidParameters_ShouldSucceed(int width, int height, int components)
{
    using var jpeg = new JpegRaster(width, height, components);
    Assert.Equal(width, jpeg.Width);
    Assert.Equal(height, jpeg.Height);
    Assert.Equal(components, jpeg.Components);
}
```

### Performance Tests

```csharp
[Fact]
public async Task EncodeAsync_WithLargeImage_ShouldCompleteWithinTimeout()
{
    using var jpeg = new JpegRaster(4096, 4096, 3);
    jpeg.Quality = 85;

    var stopwatch = Stopwatch.StartNew();
    var data = await jpeg.EncodeAsync();
    stopwatch.Stop();

    Assert.True(stopwatch.ElapsedMilliseconds < 5000); // 5 second timeout
    Assert.True(data.Length > 0);
}
```

## Integration with Planet Ecosystem

### Spatial Library Integration

```csharp
using Wangkanai.Spatial;
using Wangkanai.Spatial.Coordinates;

// Add geospatial metadata to JPEG
jpeg.Metadata.ExifData.GpsLatitude = 40.7128;
jpeg.Metadata.ExifData.GpsLongitude = -74.0060;

// Convert to spatial coordinate
var spatial = new Geodetic(
    jpeg.Metadata.ExifData.GpsLatitude,
    jpeg.Metadata.ExifData.GpsLongitude);
```

### Graphics Library Integration

```csharp
// JPEG inherits from Raster base class
Raster raster = new JpegRaster(800, 600, 3);

// Implements IMetadata interface
IMetadata metadata = raster.Metadata;

// Supports graphics library disposal patterns
if (raster.HasLargeMetadata)
{
    await raster.DisposeAsync();
}
```

## Best Practices

### Image Quality Optimization

1. **Use quality 85**: Good balance for most applications
2. **Choose appropriate subsampling**: 4:2:0 for web, 4:4:4 for professional
3. **Progressive for web**: Better loading experience
4. **Remove metadata for web**: Smaller file sizes

### Memory Management

1. **Use using statements**: Proper disposal of resources
2. **Async operations**: Non-blocking I/O for large images
3. **Batch processing**: Reuse objects when possible
4. **Monitor memory usage**: Check estimated memory usage

### Performance Optimization

1. **Optimize Huffman tables**: 3-5% smaller files
2. **Use appropriate quality**: Higher quality = slower encoding
3. **Parallel processing**: Multiple threads for batch operations
4. **Cache ICC profiles**: Reuse color profiles

## Contributing

### Development Setup

1. Install .NET 9.0 SDK
2. Clone the repository
3. Navigate to `Graphics/Rasters/src/Root/Jpegs`
4. Run `dotnet build` to build the project

### Running Tests

```bash
dotnet test Graphics/Rasters/tests/Unit/Jpegs/
```

### Code Standards

- Follow the coding guidelines in CLAUDE.md
- Use PascalCase for public members
- Include comprehensive XML documentation
- Write unit tests for all public methods
- Use async/await for I/O operations

This JPEG implementation provides a robust foundation for professional image processing with extensive metadata support
and optimization capabilities for the Planet Graphics ecosystem.
