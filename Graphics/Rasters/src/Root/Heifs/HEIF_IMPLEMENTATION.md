# HEIF (High Efficiency Image File Format) Implementation

## Overview

The HEIF implementation in the Wangkanai Graphics Rasters library provides comprehensive support for the High Efficiency Image File Format, a versatile container format that supports multiple codecs and advanced features like HDR, image sequences, and auxiliary images.

## Key Features

### Codec Support
- **HEVC (H.265)**: Primary codec with excellent compression efficiency
- **AVC (H.264)**: Backward compatibility codec
- **AV1**: Modern codec option for improved efficiency
- **VVC (H.266)**: Next-generation video coding support
- **JPEG**: For thumbnails and compatibility

### Container Features
- **Image Collections**: Multiple images in a single file
- **Image Sequences**: Animation and burst photo support
- **Auxiliary Images**: Depth maps, alpha channels, thumbnails
- **Derived Images**: Non-destructive edits and transformations
- **Multi-resolution**: Multiple versions at different resolutions

### Advanced Capabilities
- **HDR Support**: HDR10, HLG, Dolby Vision metadata
- **Clean Aperture**: Crop information without re-encoding
- **Rotation/Mirror**: Lossless transformations
- **Grid Images**: Large images composed of smaller tiles
- **Thumbnails**: Automatic generation and storage

## Architecture

### Core Classes

#### HeifRaster
The main implementation class that extends the abstract `Raster` base class and implements `IHeifRaster`.

```csharp
public sealed class HeifRaster : Raster, IHeifRaster
{
    // Core properties
    public HeifCompression Compression { get; set; }
    public HeifProfile Profile { get; set; }
    public HeifBrand Brand { get; set; }
    
    // Metadata
    public HeifMetadata Metadata { get; set; }
    
    // Processing capabilities
    public override async Task<IRaster> ResizeAsync(int width, int height);
    public override async Task<IRaster> CropAsync(Rectangle region);
}
```

#### HeifMetadata
Extends `RasterMetadataBase` to provide HEIF-specific metadata while inheriting common properties.

```csharp
public sealed class HeifMetadata : RasterMetadataBase
{
    // HEIF-specific properties
    public HeifCompression Compression { get; set; }
    public HeifProfile Profile { get; set; }
    public CameraMetadata? Camera { get; set; }
    public HdrMetadata? HdrMetadata { get; set; }
    
    // Container features
    public int ImageCount { get; set; }
    public bool IsImageSequence { get; set; }
    public List<AuxiliaryImageInfo>? AuxiliaryImages { get; set; }
}
```

#### HeifEncodingOptions
Provides comprehensive encoding configuration extending the base options.

```csharp
public sealed class HeifEncodingOptions : RasterEncodingOptionsBase
{
    // Codec configuration
    public HeifCompression Compression { get; set; }
    public HeifProfile Profile { get; set; }
    
    // Container options
    public bool EnableThumbnails { get; set; }
    public bool EnableMetadata { get; set; }
    
    // Factory methods
    public static HeifEncodingOptions CreateHighQuality();
    public static HeifEncodingOptions CreateWebOptimized();
    public static HeifEncodingOptions CreateHdr();
}
```

## Usage Examples

### Basic HEIF Creation
```csharp
using Wangkanai.Graphics.Rasters.Heifs;

// Create a new HEIF image
var heif = new HeifRaster
{
    Width = 3840,
    Height = 2160,
    Compression = HeifCompression.Hevc,
    Profile = HeifProfile.Main10,
    BitDepth = 10
};

// Set metadata
heif.Metadata = new HeifMetadata
{
    Width = 3840,
    Height = 2160,
    BitDepth = 10,
    Camera = new CameraMetadata
    {
        CameraMake = "Apple",
        CameraModel = "iPhone 15 Pro",
        FocalLength = 24.0,
        Aperture = 1.78
    }
};
```

### Using Factory Methods
```csharp
// Create optimized configurations
var webImage = HeifExamples.CreateWebOptimized(1920, 1080);
var hdrImage = HeifExamples.CreateHdr(3840, 2160);
var photoSequence = HeifExamples.CreatePhotoSequence(1920, 1080, 30);

// Professional photography
var proPhoto = HeifExamples.CreateProfessional(6000, 4000)
    .WithCamera("Canon", "EOS R5", 50.0, 1.2)
    .WithGps(37.7749, -122.4194, 10.0);
```

### HDR Content
```csharp
// Create HDR HEIF
var hdrHeif = new HeifRaster
{
    Compression = HeifCompression.Hevc,
    Profile = HeifProfile.Main10,
    BitDepth = 10
};

// Set HDR metadata
hdrHeif.Metadata.HdrMetadata = new HdrMetadata
{
    Format = HdrFormat.Hdr10,
    MaxLuminance = 1000.0,
    MinLuminance = 0.0001,
    ColorPrimaries = HdrColorPrimaries.Bt2020,
    TransferCharacteristics = HdrTransferCharacteristics.SmpteSt2084
};
```

### Image Collections
```csharp
// Create multi-image HEIF
var collection = new HeifRaster
{
    Brand = HeifBrand.Hevc  // Collection brand
};

collection.Metadata.ImageCount = 5;
collection.Metadata.IsImageCollection = true;

// Add auxiliary images
collection.Metadata.AuxiliaryImages = new List<AuxiliaryImageInfo>
{
    new() { Type = AuxiliaryImageType.DepthMap, Index = 1 },
    new() { Type = AuxiliaryImageType.AlphaChannel, Index = 2 },
    new() { Type = AuxiliaryImageType.Thumbnail, Index = 3 }
};
```

### Validation
```csharp
// Validate HEIF configuration
var validator = new HeifValidator();
var result = validator.Validate(heif);

if (!result.IsValid)
{
    Console.WriteLine($"Validation failed: {result.GetSummary()}");
}

// Validate with options
var options = new HeifEncodingOptions
{
    Quality = 85,
    Compression = HeifCompression.Hevc,
    Profile = HeifProfile.Main
};

result = HeifValidator.ValidateWithOptions(heif, options);
```

## Best Practices

### 1. Codec Selection
- Use **HEVC** for best compression and wide support
- Use **AVC** for maximum compatibility with older devices
- Use **AV1** for next-generation efficiency
- Use **VVC** for cutting-edge compression

### 2. Profile Selection
```csharp
// Standard content
heif.Profile = HeifProfile.Main;

// HDR content
heif.Profile = HeifProfile.Main10;

// Professional content
heif.Profile = HeifProfile.Main444;
```

### 3. Quality Settings
```csharp
// Web images
options.Quality = RasterConstants.QualityPresets.Web;     // 75

// Professional photography
options.Quality = RasterConstants.QualityPresets.Professional; // 90

// Archival
options.Quality = RasterConstants.QualityPresets.Lossless;    // 100
```

### 4. Memory Management
```csharp
// For large collections
await using var heif = new HeifRaster();
heif.Metadata.MaxPixelBufferSizeMB = RasterConstants.Memory.MaxPixelBufferSizeMB;

// Dispose properly
heif.Dispose();
```

### 5. HDR Handling
```csharp
// Check HDR support
if (heif.Profile == HeifProfile.Main10 || 
    heif.Profile == HeifProfile.Main12)
{
    // Configure HDR metadata
    heif.Metadata.HdrMetadata = new HdrMetadata
    {
        Format = HdrFormat.Hdr10,
        MaxLuminance = 1000.0,
        ColorPrimaries = HdrColorPrimaries.Bt2020
    };
}
```

## Performance Considerations

### Encoding Speed
```csharp
// Fastest encoding
options.Speed = RasterConstants.SpeedPresets.Fastest;

// Balanced
options.Speed = RasterConstants.SpeedPresets.Default;

// Best quality
options.Speed = RasterConstants.SpeedPresets.Slowest;
```

### Multi-threading
```csharp
// Auto-detect optimal threads
options.ThreadCount = 0;

// Manual thread control
options.ThreadCount = Environment.ProcessorCount / 2;
```

### Tile Processing
```csharp
// Enable tiling for large images
options.UseTiling = true;
options.TileSize = 512;
```

## Error Handling

```csharp
try
{
    var heif = await HeifRaster.LoadAsync("image.heif");
    
    // Process image
    var resized = await heif.ResizeAsync(1920, 1080);
    
    // Save with options
    var options = HeifEncodingOptions.CreateHighQuality();
    await resized.SaveAsync("output.heif", options);
}
catch (HeifException ex)
{
    Console.WriteLine($"HEIF error: {ex.Message}");
}
catch (UnsupportedCodecException ex)
{
    Console.WriteLine($"Codec not supported: {ex.CodecName}");
}
```

## Testing

The HEIF implementation includes comprehensive test coverage:

- **HeifRasterTests**: Core functionality tests
- **HeifMetadataTests**: Metadata handling and validation
- **HeifValidatorTests**: Format validation tests
- **HeifEncodingOptionsTests**: Encoding configuration tests
- **HeifExamplesTests**: Factory method tests
- **HeifConstantsTests**: Constants validation

## Future Enhancements

1. **Live Photos**: Support for Apple's Live Photo format
2. **Derived Images**: Non-destructive edit chains
3. **Grid Images**: Large image tiling support
4. **Multi-view**: Stereoscopic image support
5. **Advanced Metadata**: IPTC, XMP integration
6. **Hardware Acceleration**: GPU-based encoding/decoding