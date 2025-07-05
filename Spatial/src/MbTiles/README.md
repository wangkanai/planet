## Wangkanai Spatial MbTiles

**Namespace:** `Wangkanai.Spatial.MbTiles`

MBTiles format support with SQLite-based tile storage for efficient raster and vector map tile management.

## Features

- **MBTiles Specification**: Complete implementation of the MBTiles standard
- **SQLite Storage**: Efficient database-based tile storage and retrieval
- **Format Support**: Both raster and vector tile formats
- **Metadata Management**: Tile metadata and attribution handling
- **Performance Optimized**: Fast tile access and caching capabilities

## Key Classes

- `MbTileFormat` - Enumeration of supported MBTiles formats
- `MbTileType` - Type definitions for MBTiles content

## Usage

```csharp
using Wangkanai.Spatial.MbTiles;

// Work with MBTiles format types
var format = MbTileFormat.PNG;
var tileType = MbTileType.Raster;
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

## Dependencies

- **Wangkanai.Spatial** - Core spatial data types and operations
- **Microsoft.Data.Sqlite** - SQLite database operations
