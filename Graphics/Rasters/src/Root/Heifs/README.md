# HEIF (High Efficiency Image File Format) Technical Specification

## Overview

The HEIF (High Efficiency Image File Format) implementation provides comprehensive support for Apple's advanced image
container format. HEIF is a modern image format that offers superior compression efficiency, HDR support, and advanced
features like image sequences, auxiliary images, and lossless transformations.

## Key Features

### 🚀 **Compression Efficiency**

- **50-80% smaller** file sizes compared to JPEG
- Multiple codec support: HEVC (H.265), AVC (H.264), AV1, VVC (H.266)
- Lossless and near-lossless compression modes
- Advanced chroma subsampling options

### 🎨 **Advanced Color Support**

- **10-bit and 12-bit** color depth support
- HDR10, HLG, and Dolby Vision metadata
- Wide color gamut support (BT.2020, Display P3)
- ICC color profile embedding
- Multiple color spaces (sRGB, BT.709, BT.2020)

### 📱 **Modern Features**

- Image sequences (burst photos, Live Photos)
- Auxiliary images (depth maps, alpha channels)
- Lossless transformations (rotation, cropping)
- Multi-resolution storage
- Progressive decoding support

### 🔧 **Professional Capabilities**

- Comprehensive EXIF metadata support
- GPS location embedding
- Camera settings preservation
- Professional photography workflows
- Batch processing optimization

## Technical Specifications

### Supported Codecs

| Codec            | Performance | Quality | Compatibility | Use Case       |
|------------------|-------------|---------|---------------|----------------|
| **HEVC (H.265)** | ⭐⭐⭐⭐        | ⭐⭐⭐⭐⭐   | ⭐⭐⭐⭐          | Primary choice |
| **AVC (H.264)**  | ⭐⭐⭐⭐⭐       | ⭐⭐⭐     | ⭐⭐⭐⭐⭐         | Compatibility  |
| **AV1**          | ⭐⭐⭐         | ⭐⭐⭐⭐⭐   | ⭐⭐⭐           | Next-gen       |
| **VVC (H.266)**  | ⭐⭐          | ⭐⭐⭐⭐⭐   | ⭐⭐            | Future-proof   |

### Color Space Support

```csharp
public enum HeifColorSpace
{
    Srgb,           // Standard web/display
    DisplayP3,      // Wide gamut display
    Bt709,          // HD video standard
    Bt2020Ncl,      // 4K/UHD standard
    Bt2100Pq,       // HDR10
    Bt2100Hlg,      // HLG HDR
    LinearRgb,      // Professional workflow
    Xyz             // CIE XYZ
}
```

### Chroma Subsampling Options

```csharp
public enum HeifChromaSubsampling
{
    Yuv444,  // 4:4:4 - No subsampling (highest quality)
    Yuv422,  // 4:2:2 - Horizontal subsampling
    Yuv420,  // 4:2:0 - Both directions (most efficient)
    Yuv400   // 4:0:0 - Monochrome
}
```

## Usage Examples

### Basic Usage

```csharp
using Wangkanai.Graphics.Rasters.Heifs;

// Create new HEIF image
var heif = new HeifRaster(1920, 1080, hasAlpha: false)
{
    Quality = 85,
    Compression = HeifCompression.Hevc,
    ColorSpace = HeifColorSpace.Srgb,
    BitDepth = 8
};

// Encode to bytes
var encodedData = await heif.EncodeAsync();

// Decode from bytes
await heif.DecodeAsync(encodedData);
```

### Factory Methods for Common Scenarios

```csharp
// Web-optimized images
var webImage = HeifExamples.CreateWebOptimized(1920, 1080);

// High-quality photography
var photo = HeifExamples.CreateHighQuality(4000, 3000);

// HDR content
var hdrImage = HeifExamples.CreateHdr(3840, 2160);

// Mobile-optimized
var mobileImage = HeifExamples.CreateMobile(1080, 1920);

// Lossless archival
var archival = HeifExamples.CreateLossless(6000, 4000);
```

### HDR Implementation

```csharp
// Create HDR10 image
var hdr = HeifExamples.CreateHdr(3840, 2160);

// Configure HDR metadata
hdr.SetHdrMetadata(new HdrMetadata
{
    MaxLuminance = 1000.0,      // 1000 nits
    MinLuminance = 0.0001,      // 0.0001 nits
    MaxContentLightLevel = 1000.0,
    MaxFrameAverageLightLevel = 400.0,
    ColorPrimaries = HdrColorPrimaries.Bt2020,
    TransferCharacteristics = HdrTransferCharacteristics.Pq,
    MatrixCoefficients = HdrMatrixCoefficients.Bt2020Ncl
});
```

### Professional Photography

```csharp
// Professional photo with comprehensive metadata
var photo = HeifExamples.CreateProfessionalPhoto(6000, 4000);

// Access camera metadata
var camera = photo.HeifMetadata.CameraMetadata;
camera.CameraMake = "Canon";
camera.CameraModel = "EOS R5";
camera.FocalLength = 85.0;
camera.Aperture = 1.2;
camera.IsoSensitivity = 100;

// Add GPS location
photo.HeifMetadata.GpsCoordinates = new GpsCoordinates
{
    Latitude = 37.7749,
    Longitude = -122.4194,
    Altitude = 10.0
};
```

### Encoding Options

```csharp
// Create custom encoding options
var options = new HeifEncodingOptions
{
    Quality = 90,
    Speed = HeifConstants.SpeedPresets.Slow,
    ChromaSubsampling = HeifChromaSubsampling.Yuv444,
    Compression = HeifCompression.Hevc,
    Profile = HeifProfile.Main10,
    IsLossless = false,
    GenerateThumbnails = true,
    ThreadCount = 0 // Auto-detect
};

// Encode with options
var data = await heif.EncodeAsync(options);
```

### Validation

```csharp
// Validate HEIF configuration
var validationResult = HeifValidator.ValidateRaster(heif);

if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}

// Validate encoding options
var optionsResult = HeifValidator.ValidateEncodingOptions(options);
optionsResult.ThrowIfInvalid();
```

## Performance Characteristics

### Compression Efficiency

| Quality Setting    | File Size Reduction | Visual Quality | Encoding Speed |
|--------------------|---------------------|----------------|----------------|
| Lossless (100)     | 30-50% vs JPEG      | Perfect        | Slow           |
| Near-lossless (95) | 40-60% vs JPEG      | Excellent      | Slow           |
| High (85)          | 50-70% vs JPEG      | Very Good      | Medium         |
| Standard (75)      | 60-80% vs JPEG      | Good           | Fast           |

### Memory Usage

```csharp
// Memory-efficient processing
var options = new HeifEncodingOptions
{
    MaxPixelBufferSizeMB = 512,
    MaxMetadataBufferSizeMB = 64,
    TileSize = 1024
};

// Large image threshold
if (heif.Width * heif.Height > HeifConstants.Memory.LargeImageThreshold)
{
    // Enable tiling for large images
    options.TileSize = HeifConstants.Memory.DefaultTileSize;
}
```

### Multi-threading

```csharp
// Optimize for different scenarios
public static class ThreadingStrategies
{
    public static int GetOptimalThreadCount(HeifUseCase useCase)
    {
        return useCase switch
        {
            HeifUseCase.RealTime => Math.Min(4, Environment.ProcessorCount),
            HeifUseCase.Mobile => Math.Min(2, Environment.ProcessorCount),
            HeifUseCase.Professional => Environment.ProcessorCount,
            HeifUseCase.Archival => Environment.ProcessorCount,
            _ => 0 // Auto-detect
        };
    }
}
```

## Implementation Details

### Core Architecture

```csharp
public sealed class HeifRaster : Raster, IHeifRaster
{
    // Core properties
    public HeifColorSpace ColorSpace { get; set; }
    public HeifCompression Compression { get; set; }
    public HeifProfile Profile { get; set; }
    public int BitDepth { get; set; }
    public bool HasAlpha { get; set; }

    // Metadata
    public HeifMetadata HeifMetadata { get; set; }

    // Processing methods
    public async Task<byte[]> EncodeAsync(HeifEncodingOptions? options = null);
    public async Task DecodeAsync(byte[] data);
    public async Task<byte[]> CreateThumbnailAsync(int maxWidth, int maxHeight);
    public void SetHdrMetadata(HdrMetadata hdrMetadata);
    public HeifFeatures GetSupportedFeatures();
}
```

### Metadata Structure

```csharp
public sealed class HeifMetadata : RasterMetadataBase
{
    // HEIF-specific metadata
    public HdrMetadata? HdrMetadata { get; set; }
    public CameraMetadata? CameraMetadata { get; set; }
    public GpsCoordinates? GpsCoordinates { get; set; }
    public ImageOrientation Orientation { get; set; }

    // Container features
    public byte[]? ThumbnailData { get; set; }
    public byte[]? PreviewData { get; set; }
    public byte[]? DepthMapData { get; set; }
    public Dictionary<string, byte[]>? AuxiliaryImages { get; set; }

    // Codec parameters
    public Dictionary<string, object>? CodecParameters { get; set; }
}
```

### Container Format Support

```csharp
public sealed class HeifContainerInfo
{
    public string MajorBrand { get; set; }       // "heic", "heis", "hevc"
    public uint MinorVersion { get; set; }
    public string[] CompatibleBrands { get; set; }
    public bool HasThumbnails { get; set; }
    public bool HasImageSequences { get; set; }
    public bool HasAuxiliaryImages { get; set; }
    public int ItemCount { get; set; }
    public int BoxCount { get; set; }
}
```

## Testing

### Unit Tests

The implementation includes comprehensive test coverage:

```csharp
// Test files structure
├── HeifRasterTests.cs           // Core functionality
├── HeifMetadataTests.cs         // Metadata handling
├── HeifEncodingOptionsTests.cs  // Encoding configuration
├── HeifValidatorTests.cs        // Validation logic
├── HeifExamplesTests.cs         // Factory methods
├── HeifConstantsTests.cs        // Constants validation
└── HeifPerformanceTests.cs      // Performance benchmarks
```

### Example Test Cases

```csharp
[Fact]
public async Task EncodeDecodeRoundTrip_PreservesImageData()
{
    // Arrange
    var original = HeifExamples.CreateHighQuality(1920, 1080);
    var options = HeifEncodingOptions.CreateLossless();

    // Act
    var encoded = await original.EncodeAsync(options);
    var decoded = new HeifRaster(encoded);
    await decoded.DecodeAsync(encoded);

    // Assert
    Assert.Equal(original.Width, decoded.Width);
    Assert.Equal(original.Height, decoded.Height);
    Assert.Equal(original.BitDepth, decoded.BitDepth);
}

[Fact]
public void ValidateRaster_InvalidDimensions_ReturnsError()
{
    // Arrange
    var heif = new HeifRaster(0, 1080);

    // Act
    var result = HeifValidator.ValidateRaster(heif);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains("Width must be greater than 0", result.Errors);
}
```

### Performance Benchmarks

```csharp
[Benchmark]
public async Task EncodeHeifHighQuality()
{
    var heif = HeifExamples.CreateHighQuality(1920, 1080);
    var options = HeifEncodingOptions.CreateHighQuality();
    await heif.EncodeAsync(options);
}

[Benchmark]
public async Task EncodeHeifWebOptimized()
{
    var heif = HeifExamples.CreateWebOptimized(1920, 1080);
    var options = HeifEncodingOptions.CreateWebOptimized();
    await heif.EncodeAsync(options);
}
```

## Advanced Usage

### Custom Codec Parameters

```csharp
// Configure HEVC-specific parameters
var codecParams = new Dictionary<string, object>
{
    ["profile"] = HeifProfile.Main10,
    ["level"] = 5.0,
    ["tier"] = "main",
    ["bitrate"] = 10_000_000,
    ["keyframe_interval"] = 1,
    ["b_frames"] = 0,
    ["ref_frames"] = 1
};

heif.SetCodecParameters(codecParams);
```

### Thumbnail Generation

```csharp
// Generate optimized thumbnails
var thumbnailData = await heif.CreateThumbnailAsync(256, 256);

// Access embedded thumbnails
if (heif.HeifMetadata.ThumbnailData != null)
{
    var thumbnail = new HeifRaster(heif.HeifMetadata.ThumbnailData);
    await thumbnail.DecodeAsync(heif.HeifMetadata.ThumbnailData);
}
```

### Color Profile Management

```csharp
// Apply ICC color profile
var iccProfile = await File.ReadAllBytesAsync("sRGB.icc");
heif.ApplyColorProfile(iccProfile);

// Verify color space
var containerInfo = heif.GetContainerInfo();
var features = heif.GetSupportedFeatures();

if (features.HasFlag(HeifFeatures.IccProfile))
{
    // Color profile is supported
}
```

## Best Practices

### 1. **Quality Settings**

```csharp
// Choose appropriate quality for use case
var quality = useCase switch
{
    HeifUseCase.Archival => HeifConstants.QualityPresets.Lossless,
    HeifUseCase.Photography => HeifConstants.QualityPresets.Professional,
    HeifUseCase.WebOptimized => HeifConstants.QualityPresets.Web,
    HeifUseCase.Mobile => HeifConstants.QualityPresets.Mobile,
    HeifUseCase.Thumbnail => HeifConstants.QualityPresets.Thumbnail,
    _ => HeifConstants.QualityPresets.Standard
};
```

### 2. **Codec Selection**

```csharp
// Select codec based on requirements
var codec = requirements switch
{
    { MaxCompatibility: true } => HeifCompression.Avc,
    { BestCompression: true } => HeifCompression.Av1,
    { FastEncoding: true } => HeifCompression.Hevc,
    { FutureProof: true } => HeifCompression.Vvc,
    _ => HeifCompression.Hevc
};
```

### 3. **Memory Management**

```csharp
// Proper disposal pattern
await using var heif = new HeifRaster(1920, 1080);

// Or explicit disposal
using (var heif = new HeifRaster(1920, 1080))
{
    // Process image
    var encoded = await heif.EncodeAsync();
} // Automatically disposed
```

### 4. **Error Handling**

```csharp
try
{
    var heif = new HeifRaster(width, height);

    // Validate before processing
    var validation = HeifValidator.ValidateRaster(heif);
    validation.ThrowIfInvalid();

    // Process image
    var result = await heif.EncodeAsync();
}
catch (HeifException ex)
{
    // Handle HEIF-specific errors
    Logger.LogError("HEIF operation failed: {Message}", ex.Message);
}
catch (OutOfMemoryException ex)
{
    // Handle memory issues
    Logger.LogError("Insufficient memory for HEIF operation");
}
```

## Contributing

### Development Setup

1. **Prerequisites**
	- .NET 9.0 SDK
	- Visual Studio 2022 or JetBrains Rider
	- Git

2. **Building**
   ```bash
   dotnet build -c Release
   ```

3. **Running Tests**
   ```bash
   dotnet test
   ```

4. **Performance Testing**
   ```bash
   dotnet run --project Graphics/Rasters/benchmark -c Release
   ```

### Code Style

Follow the project's coding guidelines:

- Use PascalCase for public members
- Use camelCase for private members
- Use expression bodies for single-line methods
- Always use `var` when type is obvious
- Include comprehensive XML documentation

### Submitting Changes

1. Fork the repository
2. Create a feature branch
3. Add comprehensive tests
4. Update documentation
5. Submit a pull request

## License

This implementation is licensed under the Apache License, Version 2.0.

---

## Related Documentation

- [HEIF Implementation Details](HEIF_IMPLEMENTATION.md)
- [HEIF Usage Guide](HEIF_USAGE_GUIDE.md)
- [Graphics Format Specifications](../../GRAPHICS_FORMAT_SPECIFICATIONS.md)
- [Unified Metadata](../../UNIFIED_METADATA.md)
