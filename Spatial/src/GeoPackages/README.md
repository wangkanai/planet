## Wangkanai Spatial GeoPackages

**Namespace:** `Wangkanai.Spatial.GeoPackages`

GeoPackage format support for geospatial data containers based on the OGC GeoPackage standard. Provides comprehensive support for storing and managing both raster and vector geospatial data in a single SQLite database file.

## Features

- **OGC Standard Compliance**: Full implementation of the OGC GeoPackage specification
- **Multi-Format Support**: Store raster imagery, vector features, and tiles in one container
- **SQLite-Based**: Leverages SQLite for reliable and portable data storage
- **Spatial Indexing**: Efficient spatial queries and data retrieval
- **Metadata Management**: Rich metadata support for data discovery and documentation
- **Cross-Platform**: Works across different GIS applications and platforms

## GeoPackage Capabilities

GeoPackage is an OGC standard that provides:

- **Vector Features**: Store point, line, and polygon geometries with attributes
- **Raster Tiles**: Tiled imagery and elevation data
- **Non-Spatial Data**: Tabular data without geographic components  
- **Extensions**: Support for custom extensions and additional functionality

## Usage

```csharp
using Wangkanai.Spatial.GeoPackages;

// Work with GeoPackage data containers
// Implementation classes will be available here
```

## Standards Compliance

This implementation follows:
- **OGC GeoPackage 1.3** specification
- **SQLite 3.x** database format
- **ISO SQL** standards for spatial operations
- **OGC Simple Features** for geometry handling

## Dependencies

- **Wangkanai.Spatial** - Core spatial data types and operations  
- **Microsoft.Data.Sqlite** - SQLite database operations
- **System.Text.Json** - JSON metadata serialization
