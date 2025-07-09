# Unified Graphics Metadata Architecture

## Overview
This document describes the unified metadata architecture implemented for both Raster and Vector graphics in the Planet Graphics library.

## Architecture

### Base Hierarchy
```
IMetadata (interface)
└── MetadataBase (abstract class)
    ├── RasterMetadataBase (abstract class)
    │   └── Format-specific raster metadata (PNG, JPEG, WebP, etc.)
    └── VectorMetadataBase (abstract class)
        └── Format-specific vector metadata (SVG, etc.)
```

### Common Properties (MetadataBase)
All metadata implementations now share these common properties:
- `Width: int` - Width in pixels
- `Height: int` - Height in pixels  
- `Author: string?` - Author or artist name
- `Copyright: string?` - Copyright information
- `Description: string?` - Description
- `Software: string?` - Software used to create/modify
- `CreationTime: DateTime?` - Creation date and time
- `ModificationTime: DateTime?` - Modification date and time

### Raster-Specific Properties (RasterMetadataBase)
- `BitDepth: int` - Bit depth per channel
- `ExifData: byte[]?` - EXIF metadata
- `XmpData: string?` - XMP metadata
- `IccProfile: byte[]?` - ICC color profile

### Vector-Specific Properties (VectorMetadataBase)
- `ViewBoxWidth: double` - Viewbox width
- `ViewBoxHeight: double` - Viewbox height
- `ViewBoxX: double` - Viewbox X coordinate
- `ViewBoxY: double` - Viewbox Y coordinate
- `Title: string?` - Title of the vector graphic

## Key Features

### 1. Unified Resource Management
- All metadata implements `IDisposable` and `IAsyncDisposable`
- Large metadata (>1MB) automatically uses async disposal
- Memory estimation through `EstimatedMetadataSize` property

### 2. Cloning Support
- Base `Clone()` method returns `IMetadata`
- Raster formats implement `CloneRaster()` returning `IRasterMetadata`
- Vector formats implement `CloneVector()` returning `IVectorMetadata`

### 3. Clear Method
- Virtual `Clear()` method in base class
- Derived classes override and call base implementation
- Resets all properties to defaults

### 4. Memory Estimation Helpers
- `EstimateStringSize()` - UTF-8 byte count
- `EstimateByteArraySize()` - Direct byte count
- `EstimateDictionarySize()` - Dictionary overhead + content
- `EstimateDictionaryByteArraySize()` - For byte array dictionaries
- `EstimateDictionaryObjectSize()` - For object dictionaries

## Migration Notes

### For Existing Code
1. Common properties (Width, Height, Author, etc.) are now in `MetadataBase`
2. `Clone()` method signature changed to return `IMetadata`
3. Use `CloneRaster()` or `CloneVector()` for type-specific cloning
4. Call `base.Clear()` in overridden Clear methods
5. Call `base.CopyBaseTo()` when implementing clone methods

### Benefits
- Reduced code duplication
- Consistent metadata handling across graphics types
- Easier maintenance and future enhancements
- Improved type safety and validation
- Unified resource management

## Example Usage

```csharp
// Raster example
var pngMetadata = new PngMetadata
{
    Width = 1920,
    Height = 1080,
    Author = "John Doe",
    CreationTime = DateTime.UtcNow,
    BitDepth = 8
};

// Vector example  
var svgMetadata = new SvgMetadata
{
    Width = 800,
    Height = 600,
    Author = "Jane Smith",
    ViewBoxWidth = 800,
    ViewBoxHeight = 600
};

// Both can be treated as IMetadata
IMetadata metadata = pngMetadata; // or svgMetadata
var size = metadata.EstimatedMetadataSize;
if (metadata.HasLargeMetadata)
{
    await metadata.DisposeAsync();
}
```