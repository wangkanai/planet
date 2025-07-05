## Wangkanai Graphics Rasters

**Namespace:** `Wangkanai.Graphics.Rasters`

A comprehensive raster image processing library with specialized support for TIFF format specifications. Designed for high-performance pixel manipulation, image processing, and metadata handling with extensive benchmarking and validation capabilities.

## Features

- **TIFF Specialization**: Complete TIFF format implementation with full specification support
- **Image Processing**: Comprehensive pixel manipulation and transformation capabilities
- **Metadata Management**: Rich metadata support including camera settings and custom tags
- **Performance Optimization**: Benchmarked operations with performance analysis tools
- **Format Validation**: Built-in validation for TIFF specification compliance
- **Cross-Platform**: Works across Windows, macOS, and Linux environments

## Core Components

### TIFF Implementation
- **`TiffRaster`** - Main TIFF processing class implementing `ITiffRaster`
- **`TiffMetadata`** - Comprehensive TIFF metadata handling
- **`TiffValidator`** - Format validation and compliance checking
- **`TiffConstants`** - TIFF specification constants and definitions

### TIFF Specifications Support

#### Color Depths
- **1-bit** - Bilevel (black and white)
- **4-bit** - 16-color palette
- **8-bit** - 256-color palette or grayscale
- **16-bit** - High color depth
- **24-bit** - True color RGB
- **32-bit** - RGBA with alpha channel
- **48-bit** - Extended color depth
- **64-bit** - Maximum color precision

#### Compression Algorithms
- **None** - Uncompressed data
- **LZW** - Lempel-Ziv-Welch compression
- **JPEG** - JPEG compression within TIFF
- **PackBits** - Run-length encoding
- **CCITT** - Group 3 and Group 4 fax compression
- **Deflate** - ZIP-style compression

#### Photometric Interpretations
- **RGB** - Red, Green, Blue color model
- **CMYK** - Cyan, Magenta, Yellow, Key (black)
- **Grayscale** - Single-channel intensity
- **Palette** - Color-mapped images
- **Lab** - CIE L*a*b* color space
- **YCbCr** - Luminance-chrominance color space

## Performance Benchmarking

The library includes comprehensive benchmarking tools:

- **`TiffRasterBenchmark`** - Performance testing for TIFF operations
- **`RealisticPerformanceDemo`** - Real-world scenario testing
- **`PerformanceDemo`** - General performance demonstrations
- **Baseline Comparisons** - Performance baseline measurements

### Benchmark Results
Performance analysis is available in `BENCHMARK_RESULTS.md` and `PERFORMANCE_ANALYSIS.md`.

## Usage

```csharp
using Wangkanai.Graphics.Rasters;
using Wangkanai.Graphics.Rasters.Tiffs;

// Create a TIFF raster
var tiffRaster = new TiffRaster();

// Set TIFF properties
tiffRaster.ColorDepth = TiffColorDepth.TrueColor24Bit;
tiffRaster.Compression = TiffCompression.LZW;
tiffRaster.PhotometricInterpretation = PhotometricInterpretation.RGB;

// Add metadata
tiffRaster.Metadata.Description = "Processed imagery";
tiffRaster.Metadata.Software = "Wangkanai Graphics";

// Validate TIFF compliance
var validator = new TiffValidator();
bool isValid = validator.ValidateFormat(tiffRaster);
```

## Image Processing Capabilities

- **Metadata Extraction**: Extract and modify image metadata
- **Compression**: Apply various compression algorithms
- **Format Conversion**: Convert between different image formats
- **Cropping**: Extract regions of interest from images
- **Resizing**: Scale images with multiple algorithm options
- **Geo-tagging**: Add geographical information to images
- **Grid Splitting**: Divide large images into smaller tiles

## Validation and Compliance

- **Specification Compliance**: Ensures TIFF files meet format standards
- **Metadata Validation**: Validates metadata structure and content
- **Format Verification**: Checks file integrity and format compliance
- **Error Handling**: Comprehensive error reporting for format issues

## Dependencies

- **Wangkanai.Graphics.Abstractions** - Core graphics interfaces
- **.NET 9.0** - Target framework
- **System.Drawing** - Basic graphics support
- **BenchmarkDotNet** - Performance benchmarking (in benchmark projects)

## Testing

Comprehensive unit tests are available covering:
- TIFF format validation
- Metadata handling
- Compression algorithms
- Performance benchmarks
- Format compliance
