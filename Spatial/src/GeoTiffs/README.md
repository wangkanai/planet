# Wangkanai Spatial GeoTiffs

**Namespace:** `Wangkanai.Spatial.GeoTiffs`

GeoTIFF format support for georeferenced raster imagery with comprehensive integration to the Wangkanai.Graphics.Rasters library. Provides complete TIFF processing capabilities with geospatial metadata handling, coordinate reference systems, and high-performance raster operations.

## Project Overview

The GeoTiffs library extends the core Graphics.Rasters TIFF implementation with geospatial capabilities, providing comprehensive support for georeferenced raster imagery. This library serves as the primary interface for handling geospatially-aware TIFF images within the Planet ecosystem, combining the high-performance raster processing capabilities of Graphics.Rasters with spatial coordinate systems and metadata management.

## Technical Specifications

### GeoTIFF Format Support
- **TIFF 6.0 Specification**: Complete baseline and extended TIFF support
- **GeoTIFF Specification**: Full implementation of GeoTIFF georeferencing standard
- **Coordinate Reference Systems**: Support for geographic and projected coordinate systems
- **Geospatial Metadata**: Geographic extent, pixel size, and transformation matrices

### Raster Processing Integration
- **Graphics.Rasters Integration**: Full utilization of high-performance TIFF processing
- **Format Validation**: Built-in validation for both TIFF and GeoTIFF compliance
- **Metadata Handling**: Comprehensive geospatial and standard TIFF metadata support
- **Performance Optimized**: Efficient processing of large georeferenced raster datasets

## Implementation Status

### ✅ Completed Features
- **GeoTIFF Support**: Complete implementation of georeferenced TIFF images
- **Graphics Integration**: Built on Wangkanai.Graphics.Rasters for enhanced TIFF processing
- **Coordinate Reference Systems**: Support for various CRS formats and transformations
- **Geospatial Metadata**: Geographic extent, pixel size, and transformation matrices
- **Format Validation**: Built-in validation for GeoTIFF compliance and standards
- **Performance Optimized**: Efficient processing of large raster datasets

### 🚧 In Progress
- **Advanced Raster Operations**: Enhanced raster manipulation and processing
- **Multi-Band Support**: Extended support for multi-spectral imagery
- **Projection Transformations**: On-the-fly coordinate system transformations

## Core Components

### Main Classes
- **`GeoTiffRaster`** - Main class for GeoTIFF raster operations, extends `TiffRaster`
- **`IGeoTiffRaster`** - Interface defining GeoTIFF capabilities, extends `ITiffRaster`

### Raster Processing Pipeline
The GeoTiffs library leverages the complete Graphics.Rasters processing pipeline:

```csharp
// GeoTIFF-specific interface extending core raster capabilities
public interface IGeoTiffRaster : ITiffRaster
{
    string? CoordinateReferenceSystem { get; set; }
    GeoExtent? GeographicExtent { get; set; }
    double[]? GeoTransform { get; set; }
    SpatialReference? SpatialReference { get; set; }
    // Inherits all ITiffRaster capabilities including:
    // - TiffColorDepth, TiffCompression, PhotometricInterpretation
    // - High-performance processing operations
    // - Metadata management and validation
}
```

## Usage Examples

### Basic GeoTIFF Operations
```csharp
using Wangkanai.Spatial.GeoTiffs;
using Wangkanai.Graphics.Rasters;

// Create a GeoTIFF raster instance
var geoTiffRaster = new GeoTiffRaster();

// Access coordinate reference system
string? crs = geoTiffRaster.CoordinateReferenceSystem;

// Get geographic extent
var extent = geoTiffRaster.GeographicExtent;

// Access underlying TIFF capabilities
var tiffMetadata = geoTiffRaster.Metadata;
var compression = geoTiffRaster.Compression;
```

### Advanced Raster Processing
```csharp
using Wangkanai.Spatial.GeoTiffs;
using Wangkanai.Graphics.Rasters;

// Load GeoTIFF file
var geoTiffRaster = await GeoTiffRaster.LoadAsync("path/to/geotiff.tif");

// Validate GeoTIFF compliance
var validationResult = geoTiffRaster.Validate();
if (validationResult.IsValid)
{
    // Process raster data
    var processedRaster = await geoTiffRaster.ResizeAsync(newWidth: 1024, newHeight: 1024);
    
    // Extract geographic information
    var spatialRef = geoTiffRaster.SpatialReference;
    var transform = geoTiffRaster.GeoTransform;
}
```

### Coordinate System Integration
```csharp
using Wangkanai.Spatial.GeoTiffs;
using Wangkanai.Spatial.Coordinates;

// Access spatial coordinate integration
var geoTiffRaster = new GeoTiffRaster();
var extent = geoTiffRaster.GeographicExtent;

// Convert to Mercator coordinates
var mercator = new Mercator();
var mercatorExtent = mercator.TransformExtent(extent);
```

## Raster Image Library Integration

The GeoTiffs library provides comprehensive integration with the Wangkanai Graphics.Rasters library, supporting all raster image manipulation capabilities:

### TIFF Specifications Support
- **Issue #50**: Complete TIFF specifications implementation through Graphics.Rasters integration
- **Issue #54**: Enhanced TIFF format support with geospatial extensions
- **Issue #60**: Comprehensive GeoTIFF specifications support

### Supported Raster Operations
- **Image Compression**: All TIFF compression algorithms (LZW, JPEG, PackBits, Deflate, etc.)
- **Image Conversion**: Format conversion with geospatial metadata preservation
- **Image Cropping**: Spatial extent-aware cropping operations
- **Image Resizing**: Scale operations with coordinate system adjustments
- **Image Geo-tagging**: Native GeoTIFF georeferencing capabilities
- **Image Grid Splitting**: Tile-based processing with spatial indexing

### Format Integration
- **JPEG Integration**: JPEG-compressed TIFF support (Issue #53)
- **PNG Integration**: PNG-based TIFF support (Issue #58)
- **WebP Integration**: WebP-compressed TIFF support (Issue #59)
- **Multi-Format Support**: All Graphics.Rasters supported formats with geospatial metadata

## Performance and Optimization

### High-Performance Processing
- **Span-based Operations**: Memory-efficient pixel processing for large GeoTIFF files
- **Parallel Processing**: Multi-threaded operations for bulk raster processing
- **Streaming Support**: Efficient processing of large raster datasets
- **Memory Management**: Optimized memory usage for geospatial raster operations

### Scalability Features
- **Large Dataset Support**: Designed for processing massive GeoTIFF files
- **Tiled Processing**: Support for tiled TIFF processing with spatial indexing
- **On-Demand Loading**: Lazy loading of raster data for memory efficiency
- **Batch Operations**: Optimized bulk processing of multiple GeoTIFF files

## Dependencies

- **Wangkanai.Graphics.Rasters** - Core TIFF processing capabilities and raster manipulation
- **Wangkanai.Spatial** - Core spatial data types and coordinate operations
- **System.Numerics** - High-performance numeric operations for geospatial calculations

## Related Issues

### Core Raster Processing
- **Issue #49**: [General raster image manipulation capabilities](https://github.com/wangkanai/planet/issues/49) - Foundation for all raster operations
- **Issue #50**: [Raster image manipulation library](https://github.com/wangkanai/planet/issues/50) - Core TIFF processing implementation

### Format-Specific Support
- **Issue #53**: [JPEG specifications support](https://github.com/wangkanai/planet/issues/53) - JPEG-compressed TIFF support
- **Issue #54**: [TIFF specifications support](https://github.com/wangkanai/planet/issues/54) - Enhanced TIFF format capabilities
- **Issue #58**: [PNG specifications support](https://github.com/wangkanai/planet/issues/58) - PNG-based TIFF operations
- **Issue #59**: [WebP specifications support](https://github.com/wangkanai/planet/issues/59) - WebP-compressed TIFF support
- **Issue #60**: [GeoTIFF specifications support](https://github.com/wangkanai/planet/issues/60) - Complete GeoTIFF implementation

## Architecture Integration

The GeoTiffs library sits at the intersection of spatial operations and raster processing:

```
┌─────────────────────────────────────┐
│         GeoTIFF Applications        │
├─────────────────────────────────────┤
│      Spatial.GeoTiffs Library      │
│    (Geospatial TIFF Integration)    │
├─────────────────────────────────────┤
│     Graphics.Rasters Library       │
│   (Core TIFF Processing Engine)     │
├─────────────────────────────────────┤
│        Spatial Core Library        │
│  (Coordinate Systems & Extents)     │
└─────────────────────────────────────┘
```

This architecture provides:
- **Unified Interface**: Single API for both spatial and raster operations
- **Performance**: Leverages optimized Graphics.Rasters processing engine
- **Extensibility**: Easy integration of new geospatial capabilities
- **Standards Compliance**: Full GeoTIFF and TIFF specification support
