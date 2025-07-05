## Wangkanai Graphics

**Namespace:** `Wangkanai.Graphics`

A comprehensive graphics processing and image handling library designed for high-performance image manipulation with cross-platform support.
Provides modular components for raster and vector graphics processing, with particular emphasis on TIFF format support and performance optimization.

- Clarifies that parallel CPU processing is currently implemented
- Indicates GPU acceleration is planned for future releases
- Emphasizes cross-platform compatibility
- Provides more context about hardware resource utilization
- Maintains consistency with the performance goals mentioned later in the document

## Features

- **High Performance**: Optimized for parallel CPU processing with cross-platform support
- **Modular Architecture**: Clear separation between abstractions, raster, and vector components
- **TIFF Specialization**: Comprehensive TIFF format support with metadata handling
- **Performance Benchmarking**: Built-in benchmarking tools for performance analysis
- **Extensible Design**: Interface-based architecture for easy extension and customization

## Components

### Core Components
- **[Graphics Abstractions](Abstractions)** - Core image processing interfaces and contracts
- **[Graphics Rasters](Rasters)** - Raster image processing with comprehensive TIFF support
- **[Graphics Vectors](Vectors)** - Vector graphics processing and manipulation

## Architecture

The library follows a layered architecture:

```
Graphics.Abstractions (Core Interfaces)
    ↓
Graphics.Rasters (TIFF Implementation)
Graphics.Vectors (Vector Implementation)
```

## Key Features by Component

### Abstractions
- Core `IImage` interface for image processing contracts
- Foundation for all graphics operations
- Platform-agnostic abstractions

### Rasters
- **TIFF Processing**: Complete TIFF format implementation
- **Metadata Support**: Rich TIFF metadata handling and validation
- **Performance Optimization**: Benchmarked and optimized operations
- **Format Validation**: Built-in TIFF compliance checking

### Vectors
- **Vector Graphics**: Scalable vector shape processing
- **Mathematical Operations**: Vector mathematics and transformations
- **Rendering Support**: Vector-to-raster conversion capabilities

## Performance

The library includes comprehensive benchmarking tools to ensure optimal performance:
- Memory usage optimization
- Processing speed benchmarks
- Comparative performance analysis
- Real-world scenario testing

## Usage

```csharp
using Wangkanai.Graphics.Abstractions;
using Wangkanai.Graphics.Rasters;
using Wangkanai.Graphics.Vectors;

// Work with raster images
var raster = new Raster();

// Process TIFF images
var tiffRaster = new TiffRaster();

// Handle vector graphics
var vector = new Vector();
```

## Dependencies

- **.NET 9.0** - Target framework
- **System.Drawing** - Basic graphics support
- **System.Numerics** - Vector mathematics
- **BenchmarkDotNet** - Performance benchmarking (in benchmark projects)

## Development Goals

- High-performance image manipulation utilizing parallel processing
- Cross-platform compatibility (Windows, macOS, Linux)
- GPU acceleration support (future enhancement)
- Memory-efficient operations for large image datasets


## References

- https://github.com/emgucv/emgucv
- https://github.com/JimBobSquarePants/ImageProcessor/tree/master
- https://github.com/kunzmi/managedCuda
- https://bitmiracle.com/libtiff/
- https://products.aspose.com/imaging/net/
- https://github.com/iron-software/IronSoftware.System.Drawing
- https://github.com/veldrid/veldrid
