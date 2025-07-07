# Raster Metadata Architecture

## Overview

The Metadatas namespace provides a comprehensive set of base classes, interfaces, and utilities for managing metadata across all raster image formats in the Wangkanai Graphics library. This architecture promotes code reuse, consistency, and maintainability while allowing format-specific customization.

## Core Components

### 1. Metadata Classes

#### IRasterMetadata Interface
- Defines the contract for all raster metadata implementations
- Common properties: dimensions, bit depth, standard metadata (EXIF, XMP, ICC)
- Memory management: EstimatedMemoryUsage, HasLargeMetadata
- Core methods: Clone(), Clear()

#### RasterMetadataBase Abstract Class
- Provides base implementation of IRasterMetadata
- Handles common metadata properties and disposal pattern
- Implements async disposal for large metadata
- Protected helper methods for cloning (CopyBaseTo)

#### Specialized Metadata Classes
- **CameraMetadata**: Camera and photography-specific metadata
- **HdrMetadata**: HDR-specific metadata (luminance, color primaries, etc.)
- **ColorVolumeMetadata**: Color volume information for HDR
- **GpsCoordinates**: GPS location data

### 2. Encoding Options

#### IRasterEncodingOptions Interface
- Common encoding configuration across all formats
- Quality, Speed, ChromaSubsampling settings
- Memory constraints and thread configuration
- Validation and cloning support

#### RasterEncodingOptionsBase Abstract Class
- Default implementations for common encoding options
- Static factory methods for common presets
- Base validation logic with customization points
- Protected CopyTo method for cloning

### 3. Validation

#### RasterValidatorBase Static Class
- Common validation methods for all formats
- Dimension, bit depth, quality validation
- File signature verification
- Memory constraint checking
- Camera metadata validation

#### RasterValidationResult Class
- Unified validation result structure
- Supports errors, warnings, and information messages
- Merge capability for combining multiple validations
- Summary generation for user-friendly output

### 4. Common Enumerations

- **ImageOrientation**: EXIF orientation values
- **ChromaSubsampling**: YUV subsampling modes
- **HdrFormat**: HDR format types (HDR10, HLG, etc.)
- **HdrColorPrimaries**: Color primary standards
- **HdrTransferCharacteristics**: Transfer functions
- **HdrMatrixCoefficients**: Matrix coefficients

### 5. Constants

#### RasterConstants Static Class
Organized into nested classes:
- **QualityPresets**: Standard quality levels (0-100)
- **SpeedPresets**: Encoding speed levels (0-10)
- **Memory**: Buffer sizes and limits
- **Dimensions**: Common resolutions and limits
- **FileSizes**: Size thresholds
- **BitDepths**: Common bit depth values
- **Resolutions**: DPI constants

## Architecture Benefits

1. **Code Reuse**: Eliminate duplicate implementations across formats
2. **Consistency**: Uniform behavior and API across all formats
3. **Maintainability**: Single point of update for common functionality
4. **Testing**: Shared test utilities and validation logic
5. **Extensibility**: Easy to add new formats following established patterns
6. **Memory Management**: Consistent disposal patterns and thresholds

## Migration Guide

### Updating Existing Metadata Classes

1. Change inheritance to RasterMetadataBase:
```csharp
public sealed class JpegMetadata : RasterMetadataBase
{
    // Format-specific properties only
}
```

2. Remove properties now in base class:
- Width, Height, BitDepth
- ExifData, XmpData, IccProfile
- CreationTime, ModificationTime, Software, etc.

3. Use CameraMetadata for camera properties:
```csharp
public CameraMetadata? Camera { get; set; }
```

4. Override abstract members:
```csharp
public override long EstimatedMemoryUsage { get; }
public override bool HasLargeMetadata { get; }
```

5. Update Clone method:
```csharp
public override IRasterMetadata Clone()
{
    var clone = new JpegMetadata();
    CopyBaseTo(clone);
    // Copy format-specific properties
    return clone;
}
```

### Updating Encoding Options

1. Inherit from RasterEncodingOptionsBase:
```csharp
public sealed class JpegEncodingOptions : RasterEncodingOptionsBase
{
    // Format-specific properties only
}
```

2. Override validation if needed:
```csharp
protected override void ValidateCore(List<string> errors)
{
    base.ValidateCore(errors);
    // Add format-specific validation
}
```

### Updating Validators

1. Use RasterValidatorBase methods:
```csharp
public static RasterValidationResult ValidateJpeg(IJpegRaster raster)
{
    var result = new RasterValidationResult();
    
    RasterValidatorBase.ValidateDimensions(
        raster.Width, raster.Height, 
        1, 65535, result);
    
    RasterValidatorBase.ValidateBitDepth(
        raster.BitDepth, 
        new[] { 8, 12 }, result);
    
    return result;
}
```

## Usage Examples

### Creating Metadata
```csharp
var metadata = new JpegMetadata
{
    Width = 1920,
    Height = 1080,
    BitDepth = 8,
    Camera = new CameraMetadata
    {
        CameraMake = "Canon",
        CameraModel = "EOS R5",
        FocalLength = 50.0,
        Aperture = 1.8
    }
};
```

### Encoding Options Factory
```csharp
// Use base factory methods
var options = JpegEncodingOptions.CreateWebOptimized();

// Or create custom
var custom = new JpegEncodingOptions
{
    Quality = RasterConstants.QualityPresets.Professional,
    Speed = RasterConstants.SpeedPresets.Slow
};
```

### Validation
```csharp
var result = JpegValidator.Validate(raster, options);
if (!result.IsValid)
{
    Console.WriteLine(result.GetSummary());
}
```

## Best Practices

1. **Always use base constants** instead of magic numbers
2. **Leverage base validation** methods for consistency
3. **Override only what's necessary** in derived classes
4. **Document format-specific** properties and behavior
5. **Use factory methods** for common configurations
6. **Handle disposal properly** especially for large metadata

## Future Enhancements

1. **Metadata Converters**: Convert between format-specific metadata
2. **Validation Profiles**: Predefined validation rules for different use cases
3. **Metadata Sanitizers**: Clean and normalize metadata
4. **Performance Counters**: Track encoding/decoding performance
5. **Format Negotiation**: Choose optimal format based on requirements