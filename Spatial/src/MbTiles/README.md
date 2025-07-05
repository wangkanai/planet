## Wangkanai Spatial MbTiles

**Namespace:** `Wangkanai.Spatial.MbTiles`

MBTiles format support with SQLite-based tile storage for efficient raster and vector map tile management. Provides comprehensive implementation of the MBTiles specification with high-performance tile operations and metadata management.

## Project Overview

The MbTiles library provides complete support for the MBTiles specification, a standard for storing map tiles in SQLite databases. This library enables efficient storage, retrieval, and management of both raster and vector map tiles, making it ideal for offline mapping applications, content delivery networks, and geospatial analysis workflows.

## Technical Specifications

### MBTiles Format Compliance
- **MBTiles 1.3 Specification**: Complete implementation of the MBTiles standard
- **SQLite 3.x Database**: Efficient SQLite-based tile storage and retrieval
- **Tile Format Support**: PNG, JPEG, WebP, and vector PBF tiles
- **Metadata Management**: Comprehensive tileset metadata and attribution

### Performance Features
- **Database Indexing**: Optimized indexes for fast tile retrieval
- **ACID Compliance**: SQLite ensures data integrity and consistency
- **Cross-Platform**: Works across different operating systems and platforms
- **Efficient Storage**: Compressed tile storage with optimized schema

## Implementation Status

### ✅ Completed Features
- **MBTiles Specification**: Complete implementation of the MBTiles standard
- **SQLite Storage**: Efficient database-based tile storage and retrieval
- **Format Support**: Both raster and vector tile formats
- **Metadata Management**: Tile metadata and attribution handling
- **Performance Optimized**: Fast tile access and caching capabilities

### 🚧 In Progress
- **Advanced Tile Operations**: Enhanced tile manipulation and processing
- **Bulk Import/Export**: Optimized operations for large tile datasets
- **Compression Optimization**: Advanced compression strategies for tile storage

## Core Components

### Key Classes

- **`MbTileFormat`** - Enumeration of supported MBTiles formats (PNG, JPEG, WebP, PBF)
- **`MbTileType`** - Type definitions for MBTiles content (raster, vector, overlay)
- **`MbTileDatabase`** - Core database operations and tile management
- **`MbTileMetadata`** - Metadata handling and validation

### Tile Processing Pipeline
```csharp
// MBTiles processing workflow
public interface IMbTileProcessor
{
    Task<byte[]> GetTileAsync(int zoom, int column, int row);
    Task StoreTileAsync(int zoom, int column, int row, byte[] tileData);
    Task<MbTileMetadata> GetMetadataAsync();
    Task SetMetadataAsync(string key, string value);
    ValidationResult ValidateDatabase();
}
```

## Usage Examples

### Basic MBTiles Operations
```csharp
using Wangkanai.Spatial.MbTiles;

// Work with MBTiles format types
var format = MbTileFormat.PNG;
var tileType = MbTileType.Raster;

// Open MBTiles database
var mbTilesDb = new MbTileDatabase("path/to/tiles.mbtiles");

// Retrieve tile data
var tileData = await mbTilesDb.GetTileAsync(zoom: 10, column: 163, row: 395);

// Store tile data
await mbTilesDb.StoreTileAsync(zoom: 10, column: 163, row: 395, tileData);
```

### Advanced Tile Management
```csharp
using Wangkanai.Spatial.MbTiles;

// Create new MBTiles database
var mbTilesDb = MbTileDatabase.Create("new_tileset.mbtiles");

// Set metadata
await mbTilesDb.SetMetadataAsync("name", "My Tileset");
await mbTilesDb.SetMetadataAsync("type", "baselayer");
await mbTilesDb.SetMetadataAsync("format", "png");
await mbTilesDb.SetMetadataAsync("bounds", "-180,-85,180,85");

// Bulk tile operations
var tiles = await mbTilesDb.GetTilesInBoundsAsync(
    minZoom: 0, maxZoom: 18,
    west: -122.5, south: 37.7, east: -122.3, north: 37.8
);

// Validate database integrity
var validation = mbTilesDb.ValidateDatabase();
```

### Raster Tile Integration
```csharp
using Wangkanai.Spatial.MbTiles;
using Wangkanai.Graphics.Rasters;

// Process raster tiles with Graphics.Rasters integration
var mbTilesDb = new MbTileDatabase("raster_tiles.mbtiles");
var tileData = await mbTilesDb.GetTileAsync(zoom: 10, column: 163, row: 395);

// Process tile as raster image
var rasterProcessor = new RasterProcessor();
var processedTile = await rasterProcessor.ProcessTileAsync(tileData);

// Store processed tile back
await mbTilesDb.StoreTileAsync(zoom: 10, column: 163, row: 395, processedTile);
```

## MBTiles Technical Specifications

MBTiles is a specification for storing tiled map data in SQLite databases for efficient map tile distribution and offline mapping applications.

### File Format Overview

**File Extension:** `.mbtiles`  
**Database Type:** SQLite 3  
**Coordinate System:** Web Mercator (EPSG:3857)  
**Tile Scheme:** TMS (Tile Map Service) with inverted Y coordinate

### Database Schema

#### Core Tables

##### 1. `metadata` Table
Stores key-value pairs for tileset metadata.

```sql
CREATE TABLE metadata (
    name TEXT,
    value TEXT
);
```

**Required Metadata Fields:**
- `name` - Human-readable name of the tileset
- `type` - Type of data: `baselayer`, `overlay`
- `version` - Version of the tileset
- `description` - Description of the layer
- `format` - Tile format: `png`, `jpg`, `webp`, `pbf`

**Optional Metadata Fields:**
- `bounds` - Bounding box: `left,bottom,right,top` in WGS84
- `center` - Default center point: `longitude,latitude,zoom`
- `minzoom` - Minimum zoom level (integer)
- `maxzoom` - Maximum zoom level (integer)
- `attribution` - Attribution text for the layer

##### 2. `tiles` Table
Stores the actual tile data.

```sql
CREATE TABLE tiles (
    zoom_level INTEGER,
    tile_column INTEGER,
    tile_row INTEGER,
    tile_data BLOB,
    PRIMARY KEY (zoom_level, tile_column, tile_row)
);
```

**Field Specifications:**
- `zoom_level` - Zoom level (0-22)
- `tile_column` - Column coordinate (X)
- `tile_row` - Row coordinate (Y, inverted TMS scheme)
- `tile_data` - Binary tile data (PNG, JPG, WebP, or PBF)

#### Optional Tables

##### `grids` Table (UTFGrid interaction)
```sql
CREATE TABLE grids (
    zoom_level INTEGER,
    tile_column INTEGER,
    tile_row INTEGER,
    grid BLOB,
    PRIMARY KEY (zoom_level, tile_column, tile_row)
);
```

##### `images` and `map` Tables (Tile deduplication)
```sql
CREATE TABLE images (
    tile_data BLOB,
    tile_id TEXT,
    PRIMARY KEY (tile_id)
);

CREATE TABLE map (
    zoom_level INTEGER,
    tile_column INTEGER,
    tile_row INTEGER,
    tile_id TEXT,
    PRIMARY KEY (zoom_level, tile_column, tile_row)
);
```

### Supported Formats

#### Raster Formats
- **PNG** - Portable Network Graphics (lossless, supports transparency)
- **JPG/JPEG** - JPEG format (lossy compression, no transparency)
- **WebP** - Google WebP format (efficient compression, modern browsers)

#### Vector Formats
- **PBF** - Protobuf vector tiles (Mapbox Vector Tiles format)

### Coordinate System Specifications

#### Tile Addressing
- **Scheme**: TMS (Tile Map Service) with Y-axis inversion
- **Origin**: Top-left corner at zoom level 0
- **Tile Size**: 256×256 pixels (standard)
- **Coordinate Range**: 0 ≤ x,y < 2^zoom

#### Zoom Levels
- **Range**: 0-22 (practical limit)
- **Zoom 0**: Single tile covering entire world
- **Zoom n**: 2^n × 2^n tiles covering the world

### Implementation Features

- **Standardized Storage**: Common format for map tile distribution
- **Efficient Access**: Database indexing for fast tile retrieval
- **Metadata Support**: Built-in metadata and attribution
- **Cross-Platform**: Works across different mapping platforms
- **ACID Compliance**: SQLite ensures data integrity
- **Offline Capability**: Perfect for mobile and offline applications

### Usage Patterns

#### Reading Tiles
```sql
SELECT tile_data FROM tiles 
WHERE zoom_level = ? AND tile_column = ? AND tile_row = ?;
```

#### Writing Metadata
```sql
INSERT OR REPLACE INTO metadata (name, value) VALUES (?, ?);
```

### Performance Optimization

```sql
-- Essential indexes for performance
CREATE UNIQUE INDEX tile_index ON tiles (zoom_level, tile_column, tile_row);
CREATE INDEX metadata_index ON metadata (name);
```

### Common Use Cases

- **Offline Mapping**: Mobile applications with offline map capability
- **Map Tile Distribution**: Content delivery networks (CDN)
- **Geospatial Analysis**: Tile-based spatial analysis and map data archival

## Raster Image Library Integration

The MbTiles library provides comprehensive integration with raster image processing capabilities:

### Raster Tile Processing
- **Issue #49**: [General raster image manipulation capabilities](https://github.com/wangkanai/planet/issues/49) - Foundation for tile-based raster operations
- **Issue #50**: [Raster image manipulation library](https://github.com/wangkanai/planet/issues/50) - Core raster processing for tile data

### Supported Tile Formats
- **PNG Tiles**: High-quality raster tiles with transparency support (Issue #58)
- **JPEG Tiles**: Efficient raster tiles for photographic imagery (Issue #53)
- **WebP Tiles**: Modern compression format for optimized tile storage (Issue #59)
- **TIFF Integration**: Support for TIFF-based tile processing (Issue #54)

### Tile Processing Capabilities
- **Tile Compression**: Optimized compression for different raster tile formats
- **Tile Conversion**: Format conversion between PNG, JPEG, WebP, and other formats
- **Tile Validation**: Format-specific validation for tile data integrity
- **Tile Optimization**: Performance optimization for tile storage and retrieval

## Performance and Optimization

### Database Performance
- **Optimized Indexing**: Essential database indexes for fast tile retrieval
- **Connection Pooling**: Efficient database connection management
- **Bulk Operations**: Optimized bulk insert and update operations
- **Query Optimization**: Efficient SQL queries for spatial tile operations

### Memory Management
- **Streaming Operations**: Memory-efficient processing of large tile datasets
- **Lazy Loading**: On-demand tile loading for memory optimization
- **Caching Strategy**: Intelligent caching for frequently accessed tiles
- **Garbage Collection**: Optimized memory management for long-running applications

## Dependencies

- **Wangkanai.Spatial** - Core spatial data types and coordinate operations
- **Microsoft.Data.Sqlite** - SQLite database operations and management
- **System.Text.Json** - JSON serialization for metadata handling

## Related Issues

### Core Raster Processing
- **Issue #49**: [General raster image manipulation capabilities](https://github.com/wangkanai/planet/issues/49) - Foundation for tile-based raster operations
- **Issue #50**: [Raster image manipulation library](https://github.com/wangkanai/planet/issues/50) - Core raster processing implementation

### Format-Specific Support
- **Issue #53**: [JPEG specifications support](https://github.com/wangkanai/planet/issues/53) - JPEG tile format support
- **Issue #54**: [TIFF specifications support](https://github.com/wangkanai/planet/issues/54) - TIFF-based tile operations
- **Issue #58**: [PNG specifications support](https://github.com/wangkanai/planet/issues/58) - PNG tile format support
- **Issue #59**: [WebP specifications support](https://github.com/wangkanai/planet/issues/59) - WebP tile format support

### Geospatial Integration
- **Issue #60**: [GeoTIFF specifications support](https://github.com/wangkanai/planet/issues/60) - Integration with GeoTIFF processing for georeferenced tiles

## Architecture Integration

The MbTiles library integrates seamlessly with the spatial and raster processing ecosystem:

```
┌─────────────────────────────────────┐
│        Mapping Applications         │
├─────────────────────────────────────┤
│      Spatial.MbTiles Library       │
│    (SQLite-based Tile Storage)      │
├─────────────────────────────────────┤
│     Graphics.Rasters Integration   │
│   (Tile Format Processing)          │
├─────────────────────────────────────┤
│        Spatial Core Library        │
│  (Coordinate Systems & Indexing)    │
└─────────────────────────────────────┘
```

This architecture provides:
- **Unified Storage**: Single SQLite database for all tile types
- **Format Flexibility**: Support for multiple raster and vector tile formats
- **Performance**: Optimized database operations and indexing
- **Standards Compliance**: Full MBTiles specification implementation
