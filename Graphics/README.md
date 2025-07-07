## Wangkanai Graphics

**Namespace:** `Wangkanai.Graphics`

A comprehensive graphics processing and image handling library designed for high-performance image manipulation with cross-platform support.
Provides modular components for raster and vector graphics processing, with emphasis on TIFF format support, performance optimization, and modern async disposal patterns for large metadata handling.

## Features

- **High Performance**: Optimized for parallel CPU processing with cross-platform support
- **Async Disposal**: Modern IAsyncDisposable implementation for handling large metadata efficiently
- **Modular Architecture**: Clear separation between abstractions, raster, and vector components
- **Multi-Format Support**: Comprehensive support for JPEG, PNG, TIFF, and WebP formats
- **Metadata Management**: Intelligent metadata size estimation and optimized disposal patterns
- **Performance Benchmarking**: Built-in benchmarking tools for performance and disposal analysis
- **Extensible Design**: Interface-based architecture for easy extension and customization

## Components

### Core Components
- **[Graphics Core](src)** - Core image processing interfaces and contracts ([📖 Read more](src/README.md))
- **[Graphics Rasters](Rasters)** - Raster image processing with comprehensive format support ([📖 Read more](Rasters/README.md))
- **[Graphics Vectors](Vectors)** - Vector graphics processing and manipulation ([📖 Read more](Vectors/README.md))

## Key Features by Component

### Core (Wangkanai.Graphics)
- Core `IImage` interface with `IAsyncDisposable` support for image processing contracts
- `IMetadata` base interface for all metadata implementations with disposable patterns
- `ImageConstants` for standardized thresholds and disposal configuration
- Foundation for all graphics operations with async disposal patterns
- Platform-agnostic abstractions

### Rasters (Wangkanai.Graphics.Rasters)
- **Multi-Format Support**: Complete implementation for JPEG, PNG, TIFF, WebP, AVIF, HEIF, and JPEG 2000 formats
- **Metadata Management**: Rich metadata handling with `IRasterMetadata` interface extending `IMetadata`
- **Async Disposal**: Optimized disposal patterns for large metadata (>1MB) with batched operations
- **Performance Optimization**: Benchmarked operations with disposal performance analysis
- **Format Validation**: Built-in compliance checking for all supported formats
- **Inheritance Hierarchy**: Standardized base `Raster` class with virtual disposal methods
- **Modern Format Support**: Including next-generation formats like AVIF and HEIF

### Vectors (Wangkanai.Graphics.Vectors)
- **Vector Graphics**: Scalable vector shape processing with async disposal support
- **Metadata Interface**: `IVectorMetadata` for vector-specific metadata requirements
- **Mathematical Operations**: Vector mathematics and transformations
- **Rendering Support**: Vector-to-raster conversion capabilities
- **Resource Management**: Efficient disposal patterns for vector-specific resources

## Performance

The library includes comprehensive benchmarking tools to ensure optimal performance:
- **Memory Management**: Optimized disposal patterns with async batching for large metadata
- **Processing Speed**: Benchmarked image operations across all supported formats
- **Disposal Performance**: Dedicated async disposal benchmarks and real-world demos
- **Comparative Analysis**: Performance comparison between sync and async disposal methods
- **Large Metadata Handling**: Specialized handling for metadata >1MB with yielding patterns
- **Garbage Collection**: Intelligent GC suggestions for very large metadata (>10MB)

## Architecture

### Project Structure
- `Graphics/src/` - Core abstractions and interfaces (Wangkanai.Graphics)
- `Graphics/Rasters/src/Root/` - Raster image implementations
- `Graphics/Vectors/src/Root/` - Vector graphics implementations
- `Graphics/benchmark/` - Performance benchmarking tools
- `Graphics/tests/Unit/` - Unit tests for all components

## Async Disposal & Resource Management

The Graphics library implements modern async disposal patterns to efficiently handle large image metadata:

### Key Features
- **IAsyncDisposable Implementation**: All image classes and metadata implement `IAsyncDisposable` for non-blocking resource cleanup
- **Interface Hierarchy**: `IMetadata` base interface provides disposable patterns for all metadata types
- **Intelligent Thresholds**: Automatic detection of large metadata (>1MB) for optimized disposal strategies
- **Batched Operations**: Large metadata collections are cleared in batches with `Task.Yield()` for responsiveness
- **Standardized Constants**: Centralized configuration through `ImageConstants` class

### Disposal Strategies
- **Small Metadata (<1MB)**: Synchronous disposal for optimal performance
- **Large Metadata (>1MB)**: Asynchronous disposal with yielding to avoid blocking
- **Very Large Metadata (>10MB)**: Includes explicit garbage collection suggestions

### Usage Examples

#### Basic Image Processing
```csharp
await using var image = new TiffRaster(width, height);
// Large metadata operations...
// Automatic async disposal when exiting scope
```

#### Working with Metadata
```csharp
// Raster metadata
IRasterMetadata rasterMeta = image.Metadata;
rasterMeta.Width = 1920;
rasterMeta.Height = 1080;

// Vector metadata (custom implementations)
IVectorMetadata vectorMeta = vectorGraphic.Metadata;
// Vector-specific metadata operations
```

## Dependencies

- **.NET 9.0** - Target framework
- **System.Drawing** - Basic graphics support
- **System.Numerics** - Vector mathematics
- **BenchmarkDotNet** - Performance benchmarking (in benchmark projects)

## Development Goals

- **High-Performance Operations**: Image manipulation utilizing parallel processing and optimized async disposal
- **Cross-Platform Compatibility**: Full support for Windows, macOS, and Linux environments
- **Memory Efficiency**: Advanced memory management with intelligent disposal patterns for large datasets
- **Modern .NET Patterns**: Implementation of latest .NET async disposal and resource management practices
- **GPU Acceleration**: Planned future enhancement for compute-intensive operations
- **Scalable Architecture**: Designed to handle large-scale image processing workflows efficiently

## References

- https://github.com/emgucv/emgucv
- https://github.com/JimBobSquarePants/ImageProcessor/tree/master
- https://github.com/kunzmi/managedCuda
- https://bitmiracle.com/libtiff/
- https://products.aspose.com/imaging/net/
- https://github.com/iron-software/IronSoftware.System.Drawing
- https://github.com/veldrid/veldrid
