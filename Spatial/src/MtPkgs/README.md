## Wangkanai Spatial MtPkgs

**Namespace:** `Wangkanai.Spatial.MtPkgs`

Map tile package format support for multi-layer storage, providing a standardized approach for packaging raster and vector tiles in a single, efficient container format.

## Features

- **Multi-Layer Support**: Store multiple tile layers in a single package
- **Mixed Content**: Combine raster and vector tiles in one container
- **Efficient Storage**: Optimized packaging for reduced file sizes
- **Layer Management**: Organize and access tiles by layer and zoom level
- **Metadata Support**: Rich metadata for tile packages and individual layers
- **Compression**: Built-in compression for optimal storage efficiency

## Package Structure

MtPkgs format provides:

- **Layer Organization**: Logical separation of different map layers
- **Zoom Level Management**: Efficient storage across multiple zoom levels
- **Tile Indexing**: Fast tile retrieval through optimized indexing
- **Format Flexibility**: Support for various tile formats within the package

## Usage

```csharp
using Wangkanai.Spatial.MtPkgs;

// Work with map tile packages
// Implementation classes will be available here
```

## Advantages

- **Single File Distribution**: Easier deployment and management
- **Reduced File Count**: Eliminates thousands of individual tile files
- **Atomic Operations**: Ensures data consistency across layers
- **Network Efficiency**: Optimized for web distribution and caching
- **Cross-Platform**: Compatible across different mapping platforms

## Use Cases

- **Offline Maps**: Store complete map data for offline applications
- **Map Distribution**: Package and distribute custom map layers
- **Data Archiving**: Long-term storage of map tile collections
- **Bandwidth Optimization**: Reduce network overhead for map services

## Dependencies

- **Wangkanai.Spatial** - Core spatial data types and operations
- **System.IO.Compression** - Package compression and extraction
