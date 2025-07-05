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

## MBTiles Format

MBTiles is a specification for storing tiled map data in SQLite databases. It provides:

- **Standardized Storage**: Common format for map tile distribution
- **Efficient Access**: Database indexing for fast tile retrieval  
- **Metadata Support**: Built-in metadata and attribution
- **Cross-Platform**: Works across different mapping platforms

## Dependencies

- **Wangkanai.Spatial** - Core spatial data types and operations
- **Microsoft.Data.Sqlite** - SQLite database operations
