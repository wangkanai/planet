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

- **[Spatial Root](src/Root)** - Core coordinate systems, map extent, and tile calculations
- **[MbTiles](src/MbTiles)** - MBTiles format support with SQLite-based tile storage
- **[GeoPackages](src/GeoPackages)** - GeoPackage format support for geospatial data containers
- **[GeoTiffs](src/GeoTiffs)** - GeoTIFF format support with Graphics.Rasters integration
- **[ShapeFiles](src/ShapeFiles)** - Shapefile format support for vector geospatial data
- **[MtPkgs](src/MtPkgs)** - Map tile package format support

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
