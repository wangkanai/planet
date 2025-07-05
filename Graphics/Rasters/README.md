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

The Tagged Image File Format (TIFF) is a versatile raster graphic format used for storing images. It supports a wide range of color depths and compression algorithms, making it suitable for various applications. TIFF files are commonly used in the printing and publishing industries due to their ability to retain high-quality image data.

#### File Structure

##### Header (8 bytes)
- **Byte order indicator**: "II" (0x4949) for little-endian or "MM" (0x4D4D) for big-endian
- **Magic number**: 42 (0x002A)
- **Offset to first IFD**: Image File Directory pointer

##### Image File Directory (IFD)
- Contains metadata about the image through tags
- Each IFD entry is 12 bytes:
  - Tag identifier (2 bytes)
  - Data type (2 bytes)
  - Count of values (4 bytes)
  - Value offset or actual value (4 bytes)

#### Core Tags and Functions

##### Required Tags
- **ImageWidth (256)**: Number of columns in the image
- **ImageLength (257)**: Number of rows in the image
- **BitsPerSample (258)**: Number of bits per component
- **Compression (259)**: Compression scheme used
- **PhotometricInterpretation (262)**: Color space of image data
- **StripOffsets (273)**: Byte offsets to image data strips
- **RowsPerStrip (278)**: Number of rows in each strip
- **StripByteCounts (279)**: Bytes in each strip
- **XResolution (282)**: Pixels per unit in X direction
- **YResolution (283)**: Pixels per unit in Y direction
- **ResolutionUnit (296)**: Unit of measurement for resolution

#### Supported Features

##### Color Modes
- **Bilevel (1-bit)** - Black and white
- **Grayscale (4, 8, 16-bit)** - Single-channel intensity
- **Palette color (4, 8-bit)** - Color-mapped images
- **RGB (8, 16-bit per channel)** - True color
- **CMYK (8, 16-bit per channel)** - Cyan, Magenta, Yellow, Key (black)
- **LAB color space** - CIE L*a*b* color space

##### Compression Methods
- **None** - Uncompressed data
- **CCITT Group 3/4** - For bilevel images
- **LZW** - Lempel-Ziv-Welch compression
- **JPEG** - Baseline and progressive JPEG compression
- **PackBits** - Run-length encoding
- **Deflate/ZIP** - ZIP-style compression

##### Data Organizations
- **Strips** - Image divided into horizontal bands
- **Tiles** - Image divided into rectangular blocks
- **Single strip** - Entire image as one strip

#### Additional Capabilities

##### Multiple Images
- Support for multiple images in one file through IFD chaining
- Each IFD points to the next IFD offset

##### Metadata Support
- **EXIF data** - Camera settings and image information
- **IPTC/XMP metadata** - Professional metadata standards
- **GeoTIFF tags** - Georeferencing information
- **Custom private tags** - Application-specific metadata

##### Advanced Features
- **Alpha channels** - Transparency support
- **Multiple resolution images** - Pyramid structures
- **Planar configuration** - Chunky or planar data arrangement
- **Predictor** - Improved compression efficiency
- **Sample format** - Integer, floating-point, complex data types

#### File Size Considerations
- **Maximum file size**: 4GB for standard TIFF
- **BigTIFF extension**: Support for files >4GB with 64-bit offsets
- **Strip/tile size**: Affects memory usage and access speed

#### Common Use Cases
- **Professional photography** - Uncompressed or lossless storage
- **Document imaging** - Archival and scanning applications
- **Scientific imaging** - Medical and research applications
- **Geographic systems** - GeoTIFF for spatial data
- **Prepress workflows** - Printing and publishing industries

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
