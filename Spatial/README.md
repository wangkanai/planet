# Wangkanai Spatial

**Namespace:** `Wangkanai.Spatial`

A comprehensive geospatial data handling library providing coordinate systems, map extent calculations, and support for
multiple geospatial data formats. Built with .NET 9.0 and designed for high-performance spatial operations.

## Features

- **Core Spatial Types**: Coordinate systems (Geodetic, Mercator), map extents, and tile calculations
- **Multiple Format Support**: Comprehensive support for various geospatial data formats
- **Performance Optimized**: Efficient coordinate transformations and tile operations
- **Extensible Architecture**: Modular design for easy extension and customization

## Components

### Core Components

- **[Spatial Core](src/Root/README.md)** - Core coordinate systems, map extent, and tile calculations with foundational abstractions
- **[MbTiles](src/MbTiles/README.md)** - MBTiles format support with SQLite-based tile storage
- **[GeoPackages](src/GeoPackages/README.md)** - GeoPackage format support for geospatial data containers
- **[GeoTiffs](src/GeoTiffs/README.md)** - GeoTIFF format support with Graphics.Rasters integration
- **[ShapeFiles](src/ShapeFiles)** - Shapefile format support for vector geospatial data
- **[MtPkgs](src/MtPkgs)** - Map tile package format support

Each component provides comprehensive documentation covering technical specifications, implementation status, usage examples, and integration details. For detailed information about each component, please refer to their individual README files.

### Key Classes

- `Coordinate` - Basic coordinate representation
- `Geodetic` - Geographic coordinate system operations
- `Mercator` - Web Mercator projection calculations
- `MapExtent` - Spatial extent management
- `TileIndex` - Tile indexing and addressing
- `Resolution` - Resolution calculations for different zoom levels

## Coordinate Systems

### Geodetic Coordinates

Geographic coordinates using latitude and longitude on the WGS84 ellipsoid.

### Mercator Projection

Web Mercator projection (EPSG:3857) commonly used for web mapping applications.

## Supported Formats

| Format         | Type      | Description                                 |
|----------------|-----------|---------------------------------------------|
| **MBTiles**    | Tiles     | SQLite database for raster and vector tiles |
| **GeoPackage** | Container | OGC standard for geospatial data storage    |
| **GeoTIFF**    | Raster    | Georeferenced raster imagery                |
| **Shapefile**  | Vector    | ESRI vector data format                     |
| **MtPkgs**     | Tiles     | Map tile package format                     |

## Architecture

The library follows a modular architecture with clear separation between:

- Core spatial operations and coordinate systems
- Format-specific implementations
- Tile management and indexing
- Integration with external graphics libraries

### Raster Image Integration

The Spatial library provides comprehensive integration with the Wangkanai Graphics.Rasters library for advanced raster image processing:

- **GeoTIFF Processing**: Complete georeferenced TIFF support with Graphics.Rasters integration
- **Tile-Based Raster Operations**: Efficient raster tile processing for MBTiles and GeoPackage formats
- **Format Support**: PNG, JPEG, WebP, and TIFF raster processing capabilities
- **Performance Optimization**: High-performance raster operations optimized for spatial data

### Related GitHub Issues

The spatial library addresses several key raster processing requirements:

- **Issue #49**: [General raster image manipulation capabilities](https://github.com/wangkanai/planet/issues/49) - Foundation for raster operations
- **Issue #50**: [Raster image manipulation library](https://github.com/wangkanai/planet/issues/50) - Core raster processing implementation
- **Issue #53**: [JPEG specifications support](https://github.com/wangkanai/planet/issues/53) - JPEG format support
- **Issue #54**: [TIFF specifications support](https://github.com/wangkanai/planet/issues/54) - Enhanced TIFF capabilities
- **Issue #58**: [PNG specifications support](https://github.com/wangkanai/planet/issues/58) - PNG format support
- **Issue #59**: [WebP specifications support](https://github.com/wangkanai/planet/issues/59) - WebP format support
- **Issue #60**: [GeoTIFF specifications support](https://github.com/wangkanai/planet/issues/60) - Complete GeoTIFF implementation

## Dependencies

- **.NET 9.0** - Target framework
- **Wangkanai.Graphics** - Graphics processing integration for raster formats
- **System.Text.Json** - JSON serialization
- **Microsoft.Data.Sqlite** - SQLite database operations

## Getting Started

```csharp
using Wangkanai.Spatial;
using Wangkanai.Spatial.Coordinates;

// Create a Mercator projection
var mercator = new Mercator();

// Convert latitude/longitude to meters
var meters = mercator.LatLonToMeters(longitude: -122.4194, latitude: 37.7749);

// Convert meters to pixels at zoom level 10
var pixels = mercator.MetersToPixels(meters.X, meters.Y, zoom: 10);

// Work with map extents
var extent = new MapExtent();
var coordinate = new Coordinate(x: -122.4194, y: 37.7749);
```
