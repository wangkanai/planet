# Wangkanai Spatial Core

**Namespace:** `Wangkanai.Spatial`

Core spatial data handling library providing fundamental coordinate systems, map extent calculations, tile operations, and the foundational abstractions for all spatial data processing within the Planet ecosystem. This library serves as both the core implementation and abstraction layer for spatial operations.

## Project Overview

The Spatial Core library provides the fundamental building blocks for geospatial data handling in the Wangkanai Planet ecosystem. It implements essential coordinate system transformations, map extent calculations, tile indexing, and serves as the foundation for all format-specific spatial libraries.

## Technical Specifications

### Coordinate Systems
- **Geodetic Coordinates**: WGS84 ellipsoid-based geographic coordinates (latitude/longitude)
- **Mercator Projection**: Web Mercator projection (EPSG:3857) for web mapping applications
- **Coordinate Transformations**: High-performance coordinate system conversions

### Tile Operations
- **Tile Indexing**: Efficient tile addressing and indexing system
- **Zoom Level Calculations**: Resolution calculations across different zoom levels
- **Tile Pixel Mapping**: Coordinate-to-pixel conversions for tile-based rendering

### Map Extent Management
- **Spatial Extent**: Bounding box calculations and spatial extent management
- **Extent Operations**: Intersection, union, and containment operations
- **Coordinate Bounds**: Efficient bounds checking and validation

## Core Components

### Coordinate System Classes
- **`Coordinate`** - Basic coordinate representation with X/Y values
- **`Geodetic`** - Geographic coordinate system operations and transformations
- **`Mercator`** - Web Mercator projection calculations and conversions

### Spatial Extent Management
- **`MapExtent`** - Spatial extent management and boundary operations
- **`Extent`** - Core extent calculations and spatial bounds

### Tile System
- **`TileIndex`** - Tile indexing and addressing system
- **`TileAddress`** - Tile coordinate addressing
- **`TileInfo`** - Tile metadata and information
- **`TilePixel`** - Pixel-level tile operations
- **`Resolution`** - Resolution calculations for different zoom levels

### Interfaces and Abstractions
- **`ITileSource`** - Abstract interface for tile data sources
- **`ILocalTileSource`** - Interface for local tile data sources
- **`ITileSchema`** - Interface for tile schema definitions

## Implementation Status

### ✅ Completed Features
- **Core Coordinate Systems**: Full implementation of Geodetic and Mercator projections
- **Tile Indexing**: Complete tile addressing and indexing system
- **Map Extent Operations**: Comprehensive spatial extent management
- **Coordinate Transformations**: High-performance coordinate system conversions
- **Resolution Calculations**: Multi-zoom level resolution support

### 🚧 In Progress
- **Advanced Tile Operations**: Enhanced tile manipulation and processing
- **Coordinate System Extensions**: Additional projection systems
- **Performance Optimizations**: Further performance improvements for large datasets

## Usage Examples

### Basic Coordinate Operations
```csharp
using Wangkanai.Spatial;
using Wangkanai.Spatial.Coordinates;

// Create coordinate instances
var coordinate = new Coordinate(x: -122.4194, y: 37.7749);
var geodetic = new Geodetic(longitude: -122.4194, latitude: 37.7749);

// Mercator projection operations
var mercator = new Mercator();
var meters = mercator.LatLonToMeters(longitude: -122.4194, latitude: 37.7749);
var pixels = mercator.MetersToPixels(meters.X, meters.Y, zoom: 10);
```

### Map Extent Operations
```csharp
using Wangkanai.Spatial;

// Create and work with map extents
var extent = new MapExtent();
extent.SetBounds(minX: -122.5, minY: 37.7, maxX: -122.3, maxY: 37.8);

// Check if coordinate is within extent
bool isWithin = extent.Contains(coordinate);

// Get extent center
var center = extent.Center;
```

### Tile Operations
```csharp
using Wangkanai.Spatial;

// Create tile index
var tileIndex = new TileIndex(x: 163, y: 395, zoom: 10);

// Get tile address
var address = new TileAddress(tileIndex);

// Calculate resolution for zoom level
var resolution = Resolution.Calculate(zoom: 10);
```

## Dependencies

- **.NET 9.0** - Target framework
- **System.Numerics** - High-performance numeric operations
- **System.Text.Json** - JSON serialization for configuration and metadata

## Related Issues

### Core Spatial Operations
- Issue #49: [General raster image manipulation capabilities](https://github.com/wangkanai/planet/issues/49) - Foundation for raster data integration
- Issue #50: [Raster image manipulation library](https://github.com/wangkanai/planet/issues/50) - Core raster processing support

### Coordinate System Enhancements
- Future coordinate system extensions will be tracked in additional GitHub issues
- Performance optimizations for large-scale coordinate transformations

## Integration with Raster Components

The Spatial Core library provides the foundational coordinate system and extent management capabilities that are essential for raster data processing:

### Raster Data Support
- **Coordinate Integration**: Provides coordinate system transformations for georeferenced raster data
- **Extent Calculations**: Supports spatial extent management for raster datasets
- **Tile Coordination**: Enables tile-based raster processing and rendering

### Format-Specific Integration
- **GeoTIFF Support**: Coordinate systems for georeferenced TIFF imagery
- **Tile-Based Formats**: Spatial indexing for MBTiles and other tile formats
- **Projection Support**: Web Mercator and other projections for raster display

## Performance Considerations

### Optimizations
- **Span-based Operations**: Memory-efficient coordinate transformations
- **Vectorized Calculations**: SIMD-optimized operations where possible
- **Lazy Loading**: Efficient resource management for large datasets
- **Parallel Processing**: Multi-threaded operations for bulk coordinate transformations

### Scalability
- **Large Dataset Support**: Designed for processing millions of coordinates
- **Memory Efficiency**: Minimal memory footprint for coordinate operations
- **Batch Processing**: Optimized for bulk coordinate system operations

## Architecture

The Spatial Core follows a layered architecture:

```
┌─────────────────────────────────────┐
│         Format-Specific             │
│      (GeoTIFF, MBTiles, etc.)      │
├─────────────────────────────────────┤
│      Spatial Core Abstractions     │
│    (Coordinates, Extents, Tiles)    │
├─────────────────────────────────────┤
│     Core Geometric Operations       │
│   (Projections, Transformations)    │
└─────────────────────────────────────┘
```

This architecture ensures:
- **Separation of Concerns**: Clear boundaries between core operations and format-specific implementations
- **Extensibility**: Easy addition of new coordinate systems and spatial operations
- **Performance**: Optimized core operations that serve as building blocks for complex spatial processing
- **Reusability**: Common spatial operations shared across all format-specific implementations