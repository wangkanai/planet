## Wangkanai Spatial GeoTiffs

**Namespace:** `Wangkanai.Spatial.GeoTiffs`

GeoTIFF format support for georeferenced raster imagery with integration to the Wangkanai.Graphics.Rasters library for comprehensive TIFF processing capabilities.

## Features

- **GeoTIFF Support**: Complete implementation of georeferenced TIFF images
- **Graphics Integration**: Built on Wangkanai.Graphics.Rasters for enhanced TIFF processing
- **Coordinate Reference Systems**: Support for various CRS formats and transformations
- **Geospatial Metadata**: Geographic extent, pixel size, and transformation matrices
- **Format Validation**: Built-in validation for GeoTIFF compliance and standards
- **Performance Optimized**: Efficient processing of large raster datasets

## Key Classes

- `GeoTiffRaster` - Main class for GeoTIFF raster operations, extends `TiffRaster`
- `IGeoTiffRaster` - Interface defining GeoTIFF capabilities, extends `ITiffRaster`

## Usage

```csharp
using Wangkanai.Spatial.GeoTiffs;

// Create a GeoTIFF raster instance
var geoTiffRaster = new GeoTiffRaster();

// Access coordinate reference system
string? crs = geoTiffRaster.CoordinateReferenceSystem;

// Get geographic extent
var extent = geoTiffRaster.GeographicExtent;
```

## Dependencies

- **Wangkanai.Graphics.Rasters** - Core TIFF processing capabilities
- **Wangkanai.Spatial** - Core spatial data types and operations
