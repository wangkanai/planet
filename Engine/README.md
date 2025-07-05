## Wangkanai Planet Engine

**Namespace:** `Wangkanai.Planet.Engine`

A powerful console application designed for map tile processing and rendering, serving as the core processing engine for geospatial data transformation and tile generation. Built for high-performance processing of raster GeoTIFF files and other geospatial formats.

## Features

- **Raster Tile Generation**: Converts raster GeoTIFF files into map tiles suitable for web and mobile applications
- **Multi-Format Support**: Processes various geospatial formats through integration with Spatial and Graphics libraries
- **Command-Line Interface**: Comprehensive CLI with command-line parsing for automation and workflow integration
- **High Performance**: Optimized processing algorithms for large geospatial datasets
- **Extensible Architecture**: Modular design allowing integration with other Wangkanai Planet components
- **Cross-Platform**: Runs on Windows, macOS, and Linux with .NET 9.0 support
- **Tile Indexing**: Efficient tile indexing and organization for fast retrieval

## Architecture

The Engine follows a layered architecture:

```
Engine.Console (CLI Application)
    ↓
Engine.Domain (Core Business Logic)
    ↓
Spatial Library (Geospatial Operations)
Graphics Library (Image Processing)
```

## Components

### Console Application
- **Command-Line Interface**: Full CLI with argument parsing and command routing
- **Command Handlers**: Specialized handlers for different processing operations
- **Output Management**: Structured output and progress reporting
- **Error Handling**: Comprehensive error handling and logging

### Domain Layer
- **Processing Logic**: Core algorithms for tile generation and processing
- **Business Rules**: Validation and business logic for geospatial operations
- **Data Models**: Domain models for representing geospatial data and operations

## Executable Output

The Engine builds as `tiler` executable for easy command-line usage:

```bash
# Build the tiler executable
./Engine/src/Console/build.ps1

# Use the tiler command
tiler --help
```

## Getting Started

### Build and Run

1. **Build the Solution**:
   ```bash
   dotnet build -c Release
   ```

2. **Run the Engine**:
   ```bash
   dotnet run --project Engine/src/Console
   ```

3. **Build Standalone Executable**:
   ```bash
   ./Engine/src/Console/build.ps1
   ```

### Command Usage

```bash
# Display help
tiler --help

# Process GeoTIFF to tiles
tiler process --input data.tiff --output tiles/ --format mbtiles

# Get information about supported formats
tiler info --formats
```

## Integration

The Engine integrates with:

- **Wangkanai.Spatial** - Geospatial data processing and coordinate transformations
- **Wangkanai.Graphics** - Image processing and TIFF handling
- **External Services** - Map service providers and protocols

## Supported Input Formats

- **GeoTIFF** - Georeferenced raster imagery
- **TIFF** - Standard raster images
- **JPEG/PNG** - Common image formats
- **Other formats** - Via Graphics library support

## Supported Output Formats

- **MBTiles** - SQLite-based tile storage
- **File System** - Traditional tile directory structure
- **GeoPackage** - OGC standard containers
- **Custom Formats** - Via extensible format providers

## Performance Features

- **Parallel Processing**: Multi-threaded tile generation
- **Memory Optimization**: Efficient memory usage for large datasets
- **Progress Reporting**: Real-time progress updates
- **Batch Processing**: Process multiple files efficiently

## Command Structure

```bash
tiler <command> [options]

Commands:
  get         Retrieve and process geospatial data
  process     Process input files to generate tiles
  info        Display information about formats and capabilities
  help        Display help information
```

## Dependencies

- **.NET 9.0** - Target framework
- **Wangkanai.Spatial** - Geospatial data handling
- **Wangkanai.Graphics** - Image processing capabilities
- **System.CommandLine** - Command-line interface framework

## Testing

Comprehensive testing coverage includes:
- Unit tests for domain logic
- Integration tests for processing workflows
- Performance tests for large datasets
- CLI command testing
