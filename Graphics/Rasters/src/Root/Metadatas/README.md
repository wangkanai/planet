# Raster Metadata Architecture

This directory contains the base metadata infrastructure for all raster image formats in the Planet Graphics library.

## Core Components

### IRasterMetadata
The base interface that defines the contract for all raster metadata implementations. It includes:
- Common properties (Width, Height, BitDepth, EXIF, XMP, ICC Profile)
- Content metadata (Description, Copyright, Author, Software)
- Timestamps (CreationTime, ModificationTime)
- Memory management (EstimatedMemoryUsage, HasLargeMetadata)
- Disposal pattern (IDisposable, IAsyncDisposable)
- Clone and Clear methods

### RasterMetadataBase
Abstract base class that provides common implementation for:
- All IRasterMetadata properties
- Memory estimation logic
- Disposal pattern with async support for large metadata
- Base cloning functionality via CopyBaseTo method
- Clear method to reset all values

### CameraMetadata
Specialized class for camera and photographic metadata including:
- Camera information (make, model, serial number)
- Lens information (make, model, specifications)
- Exposure settings (aperture, shutter speed, ISO)
- Photography settings (white balance, metering, flash)
- GPS location data
- Resolution information

### Common Enumerations

#### ChromaSubsampling
Defines standard chroma subsampling formats used across image formats:
- Yuv444 (4:4:4) - No subsampling
- Yuv422 (4:2:2) - Horizontal subsampling
- Yuv420 (4:2:0) - Both horizontal and vertical subsampling
- Yuv411 (4:1:1) - 4x horizontal subsampling
- Yuv400 (4:0:0) - Monochrome
- Yuv440 (4:4:0) - Vertical subsampling

#### ImageOrientation
Standard EXIF orientation values for image rotation and flipping.

### HDR Support
- HdrMetadata - HDR metadata with luminance, color primaries, and transfer characteristics
- ColorVolumeMetadata - Color volume information for HDR content
- Supporting enums for HDR formats, color primaries, transfer characteristics, and matrix coefficients

## Usage Pattern

Format-specific metadata classes should:

1. Inherit from `RasterMetadataBase`
2. Add format-specific properties
3. Override `EstimatedMemoryUsage` to include format-specific data
4. Override `Clone()` to properly copy all data
5. Override `Clear()` to reset format-specific properties
6. Override `Dispose(bool)` if needed for cleanup

Example:
```csharp
public class FormatSpecificMetadata : RasterMetadataBase
{
    public CameraMetadata? Camera { get; set; }
    public ChromaSubsampling ChromaSubsampling { get; set; }
    
    public override IRasterMetadata Clone()
    {
        var clone = new FormatSpecificMetadata
        {
            Camera = Camera?.Clone(),
            ChromaSubsampling = ChromaSubsampling
        };
        CopyBaseTo(clone);
        return clone;
    }
}
```

## Migration Guide

To update existing metadata classes:

1. Change inheritance to `RasterMetadataBase`
2. Implement `IRasterMetadata` interface
3. Remove duplicate common properties (they're in the base class)
4. Move camera-related properties to use `CameraMetadata` class
5. Update chroma subsampling to use common `ChromaSubsampling` enum
6. Override necessary virtual methods
7. Ensure proper disposal pattern is followed