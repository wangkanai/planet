# Wangkanai Spatial GeoPackages

**Namespace:** `Wangkanai.Spatial.GeoPackages`

GeoPackage format support for geospatial data containers based on the OGC GeoPackage standard. Provides comprehensive support for storing and managing both raster and vector geospatial data in a single SQLite database file with extensive metadata management and spatial indexing capabilities.

## Project Overview

The GeoPackages library implements the Open Geospatial Consortium (OGC) GeoPackage standard, providing a unified container for geospatial data storage. This library enables efficient storage, retrieval, and management of vector features, raster tiles, and non-spatial data in a single, portable SQLite database file.

## Technical Specifications

### GeoPackage Standard Compliance
- **OGC GeoPackage 1.3.1**: Complete implementation of the latest GeoPackage specification
- **SQLite 3.x Database**: Leverages SQLite for reliable and portable data storage
- **Spatial Extensions**: Full support for SQLite spatial extensions and operations
- **Multi-Format Support**: Store raster imagery, vector features, and tiles in one container

### Core Capabilities
- **Vector Features**: Store point, line, and polygon geometries with attributes
- **Raster Tiles**: Tiled imagery and elevation data with spatial indexing
- **Non-Spatial Data**: Tabular data without geographic components
- **Extensions**: Support for custom extensions and additional functionality

## Implementation Status

### ✅ Completed Features
- **OGC Standard Compliance**: Full implementation of the OGC GeoPackage specification
- **Multi-Format Support**: Store raster imagery, vector features, and tiles in one container
- **SQLite-Based**: Leverages SQLite for reliable and portable data storage
- **Spatial Indexing**: Efficient spatial queries and data retrieval
- **Metadata Management**: Rich metadata support for data discovery and documentation
- **Cross-Platform**: Works across different GIS applications and platforms

### 🚧 In Progress
- **Advanced Raster Operations**: Enhanced raster tile processing and manipulation
- **Spatial Analysis**: Advanced spatial query and analysis capabilities
- **Performance Optimization**: Further optimization for large datasets

## Core Components

### Primary Classes
- **`GeoPackageDatabase`** - Main database operations and container management
- **`GeoPackageFeatures`** - Vector feature storage and retrieval
- **`GeoPackageRasters`** - Raster tile management and processing
- **`GeoPackageMetadata`** - Metadata handling and validation

### Data Storage Architecture
```csharp
// GeoPackage data management interface
public interface IGeoPackageContainer
{
    Task<FeatureCollection> GetFeaturesAsync(string tableName);
    Task StoreFeatureAsync(string tableName, Feature feature);
    Task<byte[]> GetRasterTileAsync(string tableName, int zoom, int column, int row);
    Task StoreRasterTileAsync(string tableName, int zoom, int column, int row, byte[] tileData);
    Task<GeoPackageMetadata> GetMetadataAsync();
    ValidationResult ValidateContainer();
}
```

## GeoPackage Capabilities

### Vector Data Management
- **Feature Storage**: Store point, line, and polygon geometries with attributes
- **Spatial Indexing**: R-tree spatial indexing for efficient spatial queries
- **Attribute Management**: Rich attribute data storage and retrieval
- **Spatial Relationships**: Support for spatial relationship queries

### Raster Data Management
- **Tile Pyramids**: Multi-resolution tile pyramid storage
- **Raster Metadata**: Comprehensive raster metadata and georeferencing
- **Compression Support**: Multiple compression formats for raster tiles
- **Spatial Reference**: Full coordinate reference system support

### Metadata Framework
- **Data Discovery**: Rich metadata for data discovery and documentation
- **Standards Compliance**: ISO 19115 metadata standards support
- **Custom Extensions**: Support for application-specific metadata
- **Validation**: Built-in metadata validation and compliance checking

## Usage Examples

### Basic GeoPackage Operations
```csharp
using Wangkanai.Spatial.GeoPackages;

// Create or open GeoPackage
var geoPackage = GeoPackageDatabase.Open("data.gpkg");

// Work with vector features
var features = await geoPackage.GetFeaturesAsync("cities");
var newFeature = new Feature
{
    Geometry = new Point(longitude: -122.4194, latitude: 37.7749),
    Properties = new Dictionary<string, object> { ["name"] = "San Francisco" }
};
await geoPackage.StoreFeatureAsync("cities", newFeature);

// Work with raster tiles
var tileData = await geoPackage.GetRasterTileAsync("imagery", zoom: 10, column: 163, row: 395);
```

### Advanced Data Management
```csharp
using Wangkanai.Spatial.GeoPackages;

// Create new GeoPackage with schema
var geoPackage = GeoPackageDatabase.Create("new_data.gpkg");

// Define vector feature table
await geoPackage.CreateFeatureTableAsync("roads", new FeatureSchema
{
    GeometryType = GeometryType.LineString,
    SpatialReference = "EPSG:4326",
    Attributes = new[]
    {
        new AttributeDefinition("name", AttributeType.Text),
        new AttributeDefinition("type", AttributeType.Text),
        new AttributeDefinition("speed_limit", AttributeType.Integer)
    }
});

// Define raster tile table
await geoPackage.CreateRasterTableAsync("satellite", new RasterSchema
{
    TileFormat = "PNG",
    SpatialReference = "EPSG:3857",
    MinZoom = 0,
    MaxZoom = 18,
    Bounds = new BoundingBox(-180, -85, 180, 85)
});
```

### Spatial Query Operations
```csharp
using Wangkanai.Spatial.GeoPackages;

// Perform spatial queries
var geoPackage = GeoPackageDatabase.Open("data.gpkg");

// Spatial intersection query
var intersectingFeatures = await geoPackage.GetFeaturesIntersectingAsync(
    tableName: "buildings",
    geometry: searchArea
);

// Spatial buffer query
var nearbyFeatures = await geoPackage.GetFeaturesWithinDistanceAsync(
    tableName: "points_of_interest",
    center: new Point(-122.4194, 37.7749),
    distance: 1000 // meters
);
```

## Raster Image Library Integration

The GeoPackages library provides comprehensive integration with raster image processing capabilities:

### Raster Tile Processing
- **Issue #49**: [General raster image manipulation capabilities](https://github.com/wangkanai/planet/issues/49) - Foundation for raster tile operations
- **Issue #50**: [Raster image manipulation library](https://github.com/wangkanai/planet/issues/50) - Core raster processing for tile data

### Supported Raster Formats
- **PNG Tiles**: High-quality raster tiles with transparency support (Issue #58)
- **JPEG Tiles**: Efficient raster tiles for photographic imagery (Issue #53)
- **WebP Tiles**: Modern compression format for optimized tile storage (Issue #59)
- **TIFF Integration**: Support for TIFF-based tile processing (Issue #54)
- **GeoTIFF Support**: Integration with GeoTIFF processing (Issue #60)

### Tile Processing Capabilities
- **Tile Compression**: Optimized compression for different raster tile formats
- **Tile Conversion**: Format conversion between PNG, JPEG, WebP, and other formats
- **Tile Validation**: Format-specific validation for tile data integrity
- **Tile Optimization**: Performance optimization for tile storage and retrieval

## Performance and Optimization

### Database Performance
- **Spatial Indexing**: R-tree spatial indexing for efficient spatial queries
- **Connection Pooling**: Efficient database connection management
- **Bulk Operations**: Optimized bulk insert and update operations
- **Query Optimization**: Efficient SQL queries for spatial operations

### Memory Management
- **Streaming Operations**: Memory-efficient processing of large datasets
- **Lazy Loading**: On-demand data loading for memory optimization
- **Caching Strategy**: Intelligent caching for frequently accessed data
- **Transaction Management**: Efficient transaction handling for data integrity

## Standards Compliance

This implementation follows:
- **OGC GeoPackage 1.3.1** specification
- **SQLite 3.x** database format
- **ISO SQL** standards for spatial operations
- **OGC Simple Features** for geometry handling
- **ISO 19115** metadata standards

## Dependencies

- **Wangkanai.Spatial** - Core spatial data types and operations
- **Microsoft.Data.Sqlite** - SQLite database operations
- **System.Text.Json** - JSON metadata serialization
- **NetTopologySuite** - Geometry operations and spatial analysis

## Related Issues

### Core Raster Processing
- **Issue #49**: [General raster image manipulation capabilities](https://github.com/wangkanai/planet/issues/49) - Foundation for raster tile operations
- **Issue #50**: [Raster image manipulation library](https://github.com/wangkanai/planet/issues/50) - Core raster processing implementation

### Format-Specific Support
- **Issue #53**: [JPEG specifications support](https://github.com/wangkanai/planet/issues/53) - JPEG tile format support
- **Issue #54**: [TIFF specifications support](https://github.com/wangkanai/planet/issues/54) - TIFF-based tile operations
- **Issue #58**: [PNG specifications support](https://github.com/wangkanai/planet/issues/58) - PNG tile format support
- **Issue #59**: [WebP specifications support](https://github.com/wangkanai/planet/issues/59) - WebP tile format support
- **Issue #60**: [GeoTIFF specifications support](https://github.com/wangkanai/planet/issues/60) - GeoTIFF integration for georeferenced tiles

## Architecture Integration

The GeoPackages library provides a unified container for diverse geospatial data types:

```
┌─────────────────────────────────────┐
│        GIS Applications             │
├─────────────────────────────────────┤
│     Spatial.GeoPackages Library    │
│   (Unified Geospatial Container)    │
├─────────────────────────────────────┤
│  Vector Features │  Raster Tiles   │
│     Storage      │     Storage      │
├─────────────────────────────────────┤
│        SQLite Database Core        │
│  (Spatial Extensions & Indexing)    │
└─────────────────────────────────────┘
```

This architecture provides:
- **Unified Storage**: Single container for all geospatial data types
- **Standards Compliance**: Full OGC GeoPackage specification implementation
- **Performance**: Optimized spatial indexing and query operations
- **Portability**: Single-file database format for easy data sharing
