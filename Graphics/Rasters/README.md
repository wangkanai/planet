## Wangkanai Graphics Rasters

**Namespace:** `Wangkanai.Graphics.Rasters`

A comprehensive raster image processing library with specialized support for TIFF and JPEG format specifications. Designed for high-performance pixel manipulation, image processing, and metadata handling with extensive benchmarking and validation capabilities.

## Features

- **Multi-Format Support**: Complete TIFF and JPEG format implementations with full specification support
- **High-Performance Processing**: Optimized for parallel CPU processing with cross-platform support
- **Image Processing**: Comprehensive pixel manipulation and transformation capabilities
- **Metadata Management**: Rich metadata support including EXIF, IPTC/XMP, camera settings, and custom tags
- **Performance Optimization**: Benchmarked operations with performance analysis tools
- **Format Validation**: Built-in validation for TIFF and JPEG specification compliance
- **Cross-Platform**: Works across Windows, macOS, and Linux environments
- **Large Dataset Support**: Designed for high-performance raster manipulation on large image datasets

## Core Components

### Core Abstractions
- **`IRaster`** - Base interface for all raster image types
- **`RasterProcessor`** - High-performance image processing engine
- **`RasterValidator`** - Format-agnostic validation framework
- **`RasterMetadata`** - Universal metadata management system

### TIFF Implementation
- **`ITiffRaster`** - TIFF-specific interface extending `IRaster`
- **`TiffRaster`** - Main TIFF processing class implementing `ITiffRaster`
- **`TiffMetadata`** - Comprehensive TIFF metadata handling
- **`TiffValidator`** - Format validation and compliance checking
- **`TiffConstants`** - TIFF specification constants and definitions

### JPEG Implementation
- **`IJpegRaster`** - JPEG-specific interface extending `IRaster`
- **`JpegRaster`** - Main JPEG processing class implementing `IJpegRaster`
- **`JpegMetadata`** - EXIF, IPTC, and XMP metadata handling
- **`JpegValidator`** - JPEG format validation and quality checking
- **`JpegConstants`** - JPEG specification constants and markers
- **`JpegExamples`** - Usage patterns and quality recommendations

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

## JPEG Specifications Support

The Joint Photographic Experts Group (JPEG) format is a widely used lossy compression standard for digital images. It's optimized for photographs and continuous-tone images with excellent compression ratios.

### JPEG Features

#### Color Modes
- **Grayscale (8-bit)** - Single-channel intensity
- **RGB (24-bit)** - True color with 8 bits per channel
- **CMYK (32-bit)** - Print color mode with 8 bits per channel
- **YCbCr (24-bit)** - Luminance-chrominance color space (most common for JPEG)

#### Encoding Types
- **Baseline JPEG** - Standard format, most widely supported
- **Progressive JPEG** - Loads in multiple passes from low to high quality
- **JPEG 2000** - Newer standard with better compression (less common)

#### Chroma Subsampling
- **4:4:4 (None)** - No subsampling, highest quality, larger file size
- **4:2:2 (Horizontal)** - Horizontal subsampling, good quality, moderate compression
- **4:2:0 (Both)** - Both horizontal and vertical subsampling, standard compression

#### Quality and Compression
- **Quality Range**: 0-100 (0 = maximum compression, 100 = minimal compression)
- **Recommended Settings**:
  - 95-100: Excellent quality for professional photography
  - 85: High quality, good balance for print
  - 75: Good quality, standard for web
  - 60: Medium quality, suitable for thumbnails
  - 40 and below: Low quality, high compression

#### JPEG Constants and Specifications
- **Maximum Dimensions**: 65,535 × 65,535 pixels
- **Bits Per Sample**: 8 bits per channel
- **File Extensions**: .jpg, .jpeg, .jpe, .jfif
- **MIME Types**: image/jpeg, image/jpg
- **Standard Markers**: SOI (0xFFD8), EOI (0xFFD9), APP0-APP15, SOF, DHT, DQT

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

## Fundamental Code Architecture

### Core Abstraction Layer

The raster image library follows a layered architecture with format-agnostic abstractions at the core:

```csharp
// Base raster interface
public interface IRaster : IDisposable
{
    int Width { get; }
    int Height { get; }
    IRasterMetadata Metadata { get; }
    bool IsValid();
    long GetEstimatedFileSize();
    int GetColorDepth();
}

// Generic raster processor for common operations
public abstract class RasterProcessor<T> where T : IRaster
{
    public abstract Task<T> ResizeAsync(T raster, int width, int height);
    public abstract Task<T> CropAsync(T raster, Rectangle region);
    public abstract Task<byte[]> CompressAsync(T raster);
    public abstract Task<T> DecompressAsync(byte[] data);
    public abstract ValidationResult Validate(T raster);
}

// Universal metadata management
public interface IRasterMetadata
{
    string? Title { get; set; }
    string? Description { get; set; }
    string? Software { get; set; }
    DateTime? Created { get; set; }
    DateTime? Modified { get; set; }
    Dictionary<string, object> CustomTags { get; }
}
```

### Format-Specific Implementations

Each supported format extends the core abstractions:

```csharp
// JPEG-specific interface
public interface IJpegRaster : IRaster
{
    JpegColorMode ColorMode { get; set; }
    int Quality { get; set; }
    JpegEncoding Encoding { get; set; }
    JpegMetadata Metadata { get; set; }
    int SamplesPerPixel { get; set; }
    int BitsPerSample { get; set; }
    JpegChromaSubsampling ChromaSubsampling { get; set; }
    bool IsProgressive { get; set; }
    bool IsOptimized { get; set; }
    double CompressionRatio { get; set; }
}

// TIFF-specific interface
public interface ITiffRaster : IRaster
{
    TiffColorDepth ColorDepth { get; set; }
    TiffCompression Compression { get; set; }
    PhotometricInterpretation PhotometricInterpretation { get; set; }
    TiffMetadata Metadata { get; set; }
    // Additional TIFF-specific properties...
}
```

### Processing Pipeline Architecture

```csharp
// High-performance processing pipeline
public class RasterProcessingPipeline
{
    private readonly List<IRasterOperation> _operations = new();
    
    public RasterProcessingPipeline AddOperation(IRasterOperation operation)
    {
        _operations.Add(operation);
        return this;
    }
    
    public async Task<T> ProcessAsync<T>(T input) where T : IRaster
    {
        var result = input;
        foreach (var operation in _operations)
        {
            result = await operation.ExecuteAsync(result);
        }
        return result;
    }
}

// Operation interface for pipeline processing
public interface IRasterOperation
{
    Task<IRaster> ExecuteAsync(IRaster input);
    string Name { get; }
    OperationPriority Priority { get; }
}

// Built-in operations
public class ResizeOperation : IRasterOperation
{
    public int TargetWidth { get; }
    public int TargetHeight { get; }
    public ResizeAlgorithm Algorithm { get; }
    
    public async Task<IRaster> ExecuteAsync(IRaster input)
    {
        // Implementation for resize operation
    }
}
```

### Factory Pattern for Format Detection

```csharp
// Raster factory for automatic format detection
public static class RasterFactory
{
    public static async Task<IRaster> CreateFromFileAsync(string filePath)
    {
        var data = await File.ReadAllBytesAsync(filePath);
        return CreateFromData(data, Path.GetExtension(filePath));
    }
    
    public static IRaster CreateFromData(ReadOnlySpan<byte> data, string? extension = null)
    {
        // Auto-detect format based on signature
        if (JpegValidator.IsValidJpegSignature(data))
            return CreateJpegFromData(data);
        if (TiffValidator.IsValidTiffSignature(data))
            return CreateTiffFromData(data);
            
        throw new UnsupportedFormatException("Unable to detect raster format");
    }
    
    private static IJpegRaster CreateJpegFromData(ReadOnlySpan<byte> data)
    {
        // JPEG-specific creation logic
    }
    
    private static ITiffRaster CreateTiffFromData(ReadOnlySpan<byte> data)
    {
        // TIFF-specific creation logic
    }
}
```

### Validation Framework

```csharp
// Generic validation framework
public abstract class RasterValidator<T> where T : IRaster
{
    public abstract ValidationResult Validate(T raster);
    
    protected ValidationResult CreateResult() => new ValidationResult();
    
    protected void ValidateDimensions(T raster, ValidationResult result)
    {
        if (raster.Width <= 0)
            result.AddError($"Invalid width: {raster.Width}");
        if (raster.Height <= 0)
            result.AddError($"Invalid height: {raster.Height}");
    }
}

// Validation result with error aggregation
public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    
    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);
    
    public string GetSummary()
    {
        var summary = new List<string>();
        if (Errors.Count > 0)
            summary.AddRange(Errors.Select(e => $"Error: {e}"));
        if (Warnings.Count > 0)
            summary.AddRange(Warnings.Select(w => $"Warning: {w}"));
        return string.Join(Environment.NewLine, summary);
    }
}
```

### Performance-Optimized Components

```csharp
// Memory-efficient pixel processing
public ref struct PixelSpan
{
    private readonly Span<byte> _data;
    public int Width { get; }
    public int Height { get; }
    public int SamplesPerPixel { get; }
    
    public PixelSpan(Span<byte> data, int width, int height, int samplesPerPixel)
    {
        _data = data;
        Width = width;
        Height = height;
        SamplesPerPixel = samplesPerPixel;
    }
    
    public Span<byte> GetPixel(int x, int y)
    {
        var index = (y * Width + x) * SamplesPerPixel;
        return _data.Slice(index, SamplesPerPixel);
    }
}

// Parallel processing utilities
public static class ParallelRasterProcessor
{
    public static async Task ProcessTilesAsync<T>(T raster, Func<Rectangle, Task> tileProcessor, int tileSize = 512) where T : IRaster
    {
        var tiles = GenerateTiles(raster.Width, raster.Height, tileSize);
        await Parallel.ForEachAsync(tiles, async (tile, ct) =>
        {
            await tileProcessor(tile);
        });
    }
    
    private static IEnumerable<Rectangle> GenerateTiles(int width, int height, int tileSize)
    {
        for (int y = 0; y < height; y += tileSize)
        {
            for (int x = 0; x < width; x += tileSize)
            {
                yield return new Rectangle(x, y, 
                    Math.Min(tileSize, width - x), 
                    Math.Min(tileSize, height - y));
            }
        }
    }
}
```

This architecture provides:
- **Extensibility**: Easy addition of new formats through interface implementation
- **Performance**: Memory-efficient processing with parallel operation support
- **Maintainability**: Clear separation of concerns with format-specific implementations
- **Testability**: Abstract base classes and interfaces enable comprehensive unit testing
- **Flexibility**: Pipeline-based processing allows complex operation chaining

## Testing

Comprehensive unit tests are available covering:
- TIFF format validation
- JPEG format validation and processing
- Metadata handling
- Compression algorithms
- Performance benchmarks
- Format compliance
- Pipeline operations
- Parallel processing
