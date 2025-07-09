# Wangkanai Graphics Rasters

**Namespace:** `Wangkanai.Graphics.Rasters`

A comprehensive raster image processing library designed for high-performance image manipulation with universal format support. Built on a unified abstraction layer, this library provides format-agnostic operations while maintaining specialized optimizations for each supported image format.

## Purpose and Scope

The Rasters component serves as a **universal raster image processing engine** that operates seamlessly across all supported image formats through a common abstraction layer. This design enables consistent processing workflows while leveraging format-specific optimizations for maximum performance.

### Key Objectives

- **Universal Image Processing**: Provide format-agnostic operations that work consistently across all raster formats
- **High Performance**: Deliver optimized processing through parallel CPU operations and memory-efficient algorithms
- **Comprehensive Format Support**: Support major raster formats with complete specification compliance
- **Professional Quality**: Handle everything from web images to professional photography and scientific imaging
- **Extensible Architecture**: Enable easy addition of new formats through interface-based design

## Supported File Formats and Capabilities

### ✅ Fully Implemented Formats

#### TIFF (Tagged Image File Format)
- **File Extensions**: `.tiff`, `.tif`
- **Specification**: Complete TIFF 6.0 specification compliance
- **Color Modes**: Bilevel, Grayscale (4/8/16-bit), Palette, RGB, CMYK, LAB
- **Compression**: Uncompressed, LZW, JPEG, PackBits, Deflate/ZIP, CCITT Group 3/4
- **Features**: Multiple images per file, strips/tiles, BigTIFF (>4GB), GeoTIFF, EXIF/XMP metadata
- **Professional Use**: Archival imaging, professional photography, scientific imaging, GIS

#### JPEG (Joint Photographic Experts Group)
- **File Extensions**: `.jpg`, `.jpeg`, `.jpe`, `.jfif`
- **Specification**: Complete JPEG/JFIF specification compliance
- **Color Modes**: Grayscale, RGB, CMYK, YCbCr
- **Compression**: Baseline JPEG, Progressive JPEG, quality levels 0-100
- **Features**: EXIF metadata, IPTC/XMP support, chroma subsampling (4:4:4, 4:2:2, 4:2:0)
- **Professional Use**: Photography, web imaging, digital cameras, print workflows

#### AVIF (AV1 Image File Format)
- **File Extensions**: `.avif`, `.avifs`
- **Specification**: Complete AVIF specification with AV1 codec
- **Color Modes**: sRGB, Display P3, Rec. 2020, HDR10, HLG, Dolby Vision
- **Bit Depths**: 8, 10, 12 bits per channel
- **Features**: HDR support, alpha channel, film grain synthesis, lossless compression
- **Professional Use**: Next-generation web images, HDR content, high-quality compression

#### HEIF (High Efficiency Image File Format)
- **File Extensions**: `.heif`, `.heic`, `.hif`
- **Specification**: Complete HEIF specification with multiple codec support
- **Codecs**: HEVC (H.265), AVC (H.264), AV1, VVC (H.266)
- **Features**: Image sequences, auxiliary images, multi-resolution, depth maps
- **Professional Use**: Mobile photography, professional workflows, Apple ecosystem

#### JPEG 2000
- **File Extensions**: `.jp2`, `.j2k`, `.jpf`, `.jpx`, `.jpm`
- **Specification**: Complete JPEG 2000 specification
- **Compression**: Wavelet-based, lossless and lossy modes
- **Features**: Progressive transmission, ROI encoding, multiple components (up to 16,384)
- **Professional Use**: Medical imaging, digital cinema, archival storage

### 🔄 Architecture Ready (Planned)

#### PNG (Portable Network Graphics)
- **File Extensions**: `.png`
- **Features**: Lossless compression, transparency, gamma correction

#### WebP
- **File Extensions**: `.webp`
- **Features**: Lossy and lossless compression, animation support

#### BMP (Windows Bitmap)
- **File Extensions**: `.bmp`
- **Features**: Uncompressed raster format, various bit depths

#### GIF (Graphics Interchange Format)
- **File Extensions**: `.gif`
- **Features**: Animation support, palette-based colors

## Architecture and Key Classes

### Core Abstraction Layer

#### Universal Interfaces
```csharp
/// <summary>Represents a raster image</summary>
public interface IRaster : IImage
{
    // Inherits Width, Height, Metadata from IImage
    // Universal operations available through base interface
}

/// <summary>Defines the contract for raster image metadata across all formats</summary>
public interface IRasterMetadata : IMetadata
{
    int BitDepth { get; set; }
    byte[]? ExifData { get; set; }
    string? XmpData { get; set; }
    byte[]? IccProfile { get; set; }
    void Clear();
}
```

#### Universal Base Classes
```csharp
/// <summary>Base class for all raster image implementations</summary>
public abstract class Raster : IRaster
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }
    public abstract IMetadata Metadata { get; }
    
    // Implements both sync and async disposal patterns
    public void Dispose() { /* Implementation */ }
    public virtual async ValueTask DisposeAsync() { /* Implementation */ }
}
```

### Format-Specific Implementations

#### TIFF Implementation
```csharp
public interface ITiffRaster : IRaster
{
    TiffColorDepth ColorDepth { get; set; }
    TiffCompression Compression { get; set; }
    PhotometricInterpretation PhotometricInterpretation { get; set; }
    TiffMetadata Metadata { get; set; }
}

public class TiffRaster : Raster, ITiffRaster
{
    // TIFF-specific implementation with full specification support
}
```

#### JPEG Implementation
```csharp
public interface IJpegRaster : IRaster
{
    JpegColorMode ColorMode { get; set; }
    int Quality { get; set; }
    JpegEncoding Encoding { get; set; }
    bool IsProgressive { get; set; }
    JpegChromaSubsampling ChromaSubsampling { get; set; }
}

public class JpegRaster : Raster, IJpegRaster
{
    // JPEG-specific implementation with quality optimization
}
```

#### Modern Format Implementations
```csharp
// AVIF with HDR support
public interface IAvifRaster : IRaster
{
    AvifColorSpace ColorSpace { get; set; }
    HdrMetadata? HdrMetadata { get; set; }
    bool EnableFilmGrain { get; set; }
    AvifCompression Compression { get; set; }
}

// HEIF with multi-codec support
public interface IHeifRaster : IRaster
{
    HeifCodec Codec { get; set; }
    HeifProfile Profile { get; set; }
    List<HeifAuxiliaryImage> AuxiliaryImages { get; }
}
```

### Shared Components Architecture

#### Metadata System
```csharp
public abstract class RasterMetadataBase : MetadataBase, IRasterMetadata
{
    public virtual int BitDepth { get; set; }
    public virtual byte[]? ExifData { get; set; }
    public virtual string? XmpData { get; set; }
    public virtual byte[]? IccProfile { get; set; }
    
    protected override long GetBaseMemorySize()
    {
        var baseSize = base.GetBaseMemorySize();
        baseSize += EstimateByteArraySize(ExifData);
        baseSize += EstimateStringSize(XmpData);
        baseSize += EstimateByteArraySize(IccProfile);
        return baseSize;
    }
}
```

#### Shared Metadata Components
```csharp
/// <summary>Camera and photography metadata</summary>
public class CameraMetadata
{
    public string? CameraMake { get; set; }
    public string? CameraModel { get; set; }
    public double? FocalLength { get; set; }
    public double? Aperture { get; set; }
    public int? IsoSensitivity { get; set; }
    public DateTime? DateTaken { get; set; }
}

/// <summary>HDR metadata for advanced formats</summary>
public class HdrMetadata
{
    public HdrFormat Format { get; set; }
    public double MaxLuminance { get; set; }
    public double MinLuminance { get; set; }
    public HdrColorPrimaries ColorPrimaries { get; set; }
    public HdrTransferCharacteristics TransferCharacteristics { get; set; }
}

/// <summary>GPS coordinates for geotagged images</summary>
public class GpsCoordinates
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
    public DateTime? Timestamp { get; set; }
}
```

### Processing Pipeline Architecture

#### Universal Processing Operations
```csharp
/// <summary>Universal raster processing pipeline</summary>
public class RasterProcessingPipeline
{
    public async Task<IRaster> ProcessAsync(IRaster input)
    {
        // Format-agnostic processing pipeline
        var result = input;
        
        foreach (var operation in _operations)
        {
            result = await operation.ExecuteAsync(result);
        }
        
        return result;
    }
}

/// <summary>Universal operations that work across all formats</summary>
public interface IRasterOperation
{
    Task<IRaster> ExecuteAsync(IRaster input);
    string Name { get; }
    OperationPriority Priority { get; }
}
```

#### Format Detection and Factory
```csharp
/// <summary>Intelligent format detection and creation</summary>
public static class RasterFactory
{
    public static async Task<IRaster> CreateFromFileAsync(string filePath)
    {
        var data = await File.ReadAllBytesAsync(filePath);
        return CreateFromData(data, Path.GetExtension(filePath));
    }
    
    public static IRaster CreateFromData(ReadOnlySpan<byte> data, string? extension = null)
    {
        // Auto-detect format based on file signature
        if (TiffValidator.IsValidTiffSignature(data))
            return CreateTiffFromData(data);
        if (JpegValidator.IsValidJpegSignature(data))
            return CreateJpegFromData(data);
        if (AvifValidator.IsValidAvifSignature(data))
            return CreateAvifFromData(data);
        if (HeifValidator.IsValidHeifSignature(data))
            return CreateHeifFromData(data);
        if (Jpeg2000Validator.IsValidJpeg2000Signature(data))
            return CreateJpeg2000FromData(data);
            
        throw new UnsupportedFormatException("Unable to detect raster format");
    }
}
```

## Usage Examples and Code Samples

### Universal Operations (Format-Agnostic)

#### Basic Image Processing
```csharp
using Wangkanai.Graphics.Rasters;

// Load any supported format through universal interface
IRaster image = await RasterFactory.CreateFromFileAsync("image.jpg"); // Works with .tiff, .avif, .heif, etc.

// Universal operations work regardless of format
Console.WriteLine($"Image: {image.Width}x{image.Height}");

// Access metadata through common interface
var metadata = image.Metadata;
metadata.Author = "Graphics Processing System";
metadata.ModificationTime = DateTime.UtcNow;

// Universal validation
var isValid = RasterValidator.ValidateFormat(image);
Console.WriteLine($"Format valid: {isValid}");

// Proper disposal
await image.DisposeAsync();
```

#### Cross-Format Processing Pipeline
```csharp
using Wangkanai.Graphics.Rasters;

public async Task ProcessImageCollectionAsync(string[] imagePaths)
{
    var pipeline = new RasterProcessingPipeline()
        .AddOperation(new ResizeOperation(800, 600))
        .AddOperation(new CompressionOperation(85))
        .AddOperation(new MetadataCleanupOperation());
        
    foreach (var path in imagePaths)
    {
        // Load any format through universal interface
        await using var image = await RasterFactory.CreateFromFileAsync(path);
        
        // Process with format-agnostic pipeline
        await using var processed = await pipeline.ProcessAsync(image);
        
        // Save in optimal format
        await SaveOptimalFormatAsync(processed, GetOutputPath(path));
    }
}
```

### Format-Specific Operations

#### TIFF Processing
```csharp
using Wangkanai.Graphics.Rasters.Tiffs;

// Create TIFF with specific settings
var tiff = new TiffRaster
{
    Width = 3000,
    Height = 2000,
    ColorDepth = TiffColorDepth.TrueColor24Bit,
    Compression = TiffCompression.LZW,
    PhotometricInterpretation = PhotometricInterpretation.RGB
};

// Configure TIFF-specific metadata
tiff.Metadata.XResolution = 300.0;
tiff.Metadata.YResolution = 300.0;
tiff.Metadata.ResolutionUnit = ResolutionUnit.Inch;
tiff.Metadata.Software = "Wangkanai Graphics";

// Add EXIF data
tiff.Metadata.ExifData = CreateExifData();

// Save with TIFF-specific options
await tiff.SaveAsync("output.tiff", new TiffSaveOptions
{
    Compression = TiffCompression.JPEG,
    Quality = 90,
    TileSize = 256
});
```

#### JPEG Processing with Quality Control
```csharp
using Wangkanai.Graphics.Rasters.Jpegs;

// Create JPEG with quality settings
var jpeg = new JpegRaster
{
    Width = 1920,
    Height = 1080,
    Quality = 85,
    ColorMode = JpegColorMode.RGB,
    IsProgressive = true,
    ChromaSubsampling = JpegChromaSubsampling.YCbCr420
};

// Configure JPEG metadata
jpeg.Metadata.Camera = new CameraMetadata
{
    CameraMake = "Canon",
    CameraModel = "EOS R5",
    FocalLength = 85.0,
    Aperture = 1.8,
    IsoSensitivity = 400
};

// Save with quality optimization
await jpeg.SaveAsync("output.jpg", JpegSaveOptions.HighQuality);
```

#### Modern Format Processing (AVIF/HEIF)
```csharp
using Wangkanai.Graphics.Rasters.Avifs;
using Wangkanai.Graphics.Rasters.Heifs;

// Create AVIF with HDR support
var avif = new AvifRaster
{
    Width = 3840,
    Height = 2160,
    Quality = 90,
    ColorSpace = AvifColorSpace.Rec2020,
    EnableFilmGrain = true,
    BitDepth = 10
};

// Configure HDR metadata
avif.Metadata.HdrMetadata = new HdrMetadata
{
    Format = HdrFormat.Hdr10,
    MaxLuminance = 1000.0,
    MinLuminance = 0.01,
    ColorPrimaries = HdrColorPrimaries.Bt2020
};

// Create HEIF with multi-codec support
var heif = new HeifRaster
{
    Width = 4096,
    Height = 2160,
    Codec = HeifCodec.Hevc,
    Profile = HeifProfile.Main10,
    Quality = 88
};

// Add auxiliary images (depth map, alpha channel)
heif.AuxiliaryImages.Add(new HeifAuxiliaryImage
{
    Type = HeifAuxiliaryType.DepthMap,
    Data = depthMapData
});
```

### Advanced Processing Scenarios

#### Batch Processing with Format Optimization
```csharp
using Wangkanai.Graphics.Rasters;

public async Task OptimizeImageCollectionAsync(string inputDir, string outputDir)
{
    var imageFiles = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories)
        .Where(f => IsImageFile(f));
        
    var tasks = imageFiles.Select(async file =>
    {
        await using var image = await RasterFactory.CreateFromFileAsync(file);
        
        // Choose optimal format based on image characteristics
        var outputFormat = DetermineOptimalFormat(image);
        var outputPath = Path.Combine(outputDir, 
            Path.GetFileNameWithoutExtension(file) + outputFormat.Extension);
            
        // Apply format-specific optimizations
        await using var optimized = await OptimizeForFormat(image, outputFormat);
        
        // Save with optimal settings
        await optimized.SaveAsync(outputPath, outputFormat.Options);
        
        Console.WriteLine($"Processed: {file} -> {outputPath}");
    });
    
    await Task.WhenAll(tasks);
}

private async Task<IRaster> OptimizeForFormat(IRaster image, OutputFormat format)
{
    return format.Type switch
    {
        ImageFormat.Jpeg => await OptimizeForJpeg(image),
        ImageFormat.Avif => await OptimizeForAvif(image),
        ImageFormat.Heif => await OptimizeForHeif(image),
        ImageFormat.Tiff => await OptimizeForTiff(image),
        _ => image
    };
}
```

#### Memory-Efficient Large Image Processing
```csharp
using Wangkanai.Graphics.Rasters;

public async Task ProcessLargeImageAsync(string inputPath, string outputPath)
{
    await using var image = await RasterFactory.CreateFromFileAsync(inputPath);
    
    // Check if image requires tiled processing
    if (image.Width > 10000 || image.Height > 10000)
    {
        Console.WriteLine("Large image detected - using tiled processing");
        await ProcessImageInTiles(image, outputPath);
    }
    else
    {
        await ProcessImageDirect(image, outputPath);
    }
}

private async Task ProcessImageInTiles(IRaster image, string outputPath)
{
    const int tileSize = 1024;
    var tilesX = (int)Math.Ceiling((double)image.Width / tileSize);
    var tilesY = (int)Math.Ceiling((double)image.Height / tileSize);
    
    var tasks = new List<Task>();
    
    for (int y = 0; y < tilesY; y++)
    {
        for (int x = 0; x < tilesX; x++)
        {
            var tileTask = ProcessTileAsync(image, x, y, tileSize);
            tasks.Add(tileTask);
        }
    }
    
    await Task.WhenAll(tasks);
    await AssembleTiles(outputPath, tilesX, tilesY);
}
```

### Metadata Management

#### Comprehensive Metadata Handling
```csharp
using Wangkanai.Graphics.Rasters;

public async Task ManageImageMetadataAsync(string imagePath)
{
    await using var image = await RasterFactory.CreateFromFileAsync(imagePath);
    var metadata = image.Metadata as IRasterMetadata;
    
    // Universal metadata properties
    metadata.Author = "Professional Photographer";
    metadata.Copyright = "© 2025 Photography Studio";
    metadata.Description = "High-quality professional photograph";
    metadata.Software = "Wangkanai Graphics Rasters";
    metadata.CreationTime = DateTime.UtcNow;
    
    // Format-specific metadata
    if (metadata is TiffMetadata tiffMeta)
    {
        tiffMeta.XResolution = 300.0;
        tiffMeta.YResolution = 300.0;
        tiffMeta.ResolutionUnit = ResolutionUnit.Inch;
        tiffMeta.ColorSpace = TiffColorSpace.sRGB;
    }
    else if (metadata is JpegMetadata jpegMeta)
    {
        jpegMeta.Quality = 95;
        jpegMeta.IsProgressive = true;
        jpegMeta.ChromaSubsampling = JpegChromaSubsampling.YCbCr422;
    }
    
    // Add GPS coordinates
    if (metadata is IRasterMetadata rasterMeta)
    {
        rasterMeta.GpsCoordinates = new GpsCoordinates
        {
            Latitude = 37.7749,
            Longitude = -122.4194,
            Altitude = 52.0,
            Timestamp = DateTime.UtcNow
        };
    }
    
    // Save metadata changes
    await image.SaveMetadataAsync();
}
```

## Performance Considerations

### Memory Management Optimization

#### Intelligent Disposal Patterns
```csharp
// Automatic disposal optimization based on image size
public async Task ProcessImageWithOptimalDisposal(string path)
{
    await using var image = await RasterFactory.CreateFromFileAsync(path);
    
    // Check if image has large metadata requiring special handling
    if (image.Metadata.HasLargeMetadata)
    {
        Console.WriteLine($"Large metadata detected: {image.Metadata.EstimatedMetadataSize:N0} bytes");
        Console.WriteLine("Using optimized async disposal");
        
        // Process with memory-conscious operations
        await ProcessLargeImageAsync(image);
    }
    else
    {
        // Standard processing for smaller images
        await ProcessStandardImageAsync(image);
    }
    
    // Disposal is automatically optimized based on image size
}
```

#### Parallel Processing Architecture
```csharp
/// <summary>High-performance parallel processing for large images</summary>
public static class ParallelRasterProcessor
{
    public static async Task ProcessTilesAsync<T>(T raster, Func<Rectangle, Task> processor, int tileSize = 512) 
        where T : IRaster
    {
        var tiles = GenerateTiles(raster.Width, raster.Height, tileSize);
        
        await Parallel.ForEachAsync(tiles, new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = CancellationToken.None
        }, async (tile, ct) =>
        {
            await processor(tile);
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

### Processing Optimization Strategies

#### Format-Specific Performance Tuning
```csharp
/// <summary>Performance-optimized processing based on format characteristics</summary>
public class FormatOptimizedProcessor
{
    public async Task<IRaster> ProcessAsync(IRaster image)
    {
        return image switch
        {
            ITiffRaster tiff => await ProcessTiffOptimized(tiff),
            IJpegRaster jpeg => await ProcessJpegOptimized(jpeg),
            IAvifRaster avif => await ProcessAvifOptimized(avif),
            IHeifRaster heif => await ProcessHeifOptimized(heif),
            _ => await ProcessGeneric(image)
        };
    }
    
    private async Task<IRaster> ProcessTiffOptimized(ITiffRaster tiff)
    {
        // TIFF-specific optimizations
        if (tiff.Compression == TiffCompression.LZW)
        {
            // Optimize for LZW compression
            return await ProcessWithLzwOptimization(tiff);
        }
        
        if (tiff.ColorDepth == TiffColorDepth.TrueColor48Bit)
        {
            // Handle high bit depth efficiently
            return await ProcessHighBitDepth(tiff);
        }
        
        return await ProcessStandardTiff(tiff);
    }
    
    private async Task<IRaster> ProcessJpegOptimized(IJpegRaster jpeg)
    {
        // JPEG-specific optimizations
        if (jpeg.IsProgressive)
        {
            // Handle progressive JPEG efficiently
            return await ProcessProgressiveJpeg(jpeg);
        }
        
        // Optimize based on quality settings
        return jpeg.Quality switch
        {
            >= 90 => await ProcessHighQualityJpeg(jpeg),
            >= 70 => await ProcessStandardQualityJpeg(jpeg),
            _ => await ProcessLowQualityJpeg(jpeg)
        };
    }
}
```

#### Memory-Efficient Pixel Processing
```csharp
/// <summary>Memory-efficient pixel processing using spans</summary>
public ref struct PixelSpan
{
    private readonly Span<byte> _data;
    public int Width { get; }
    public int Height { get; }
    public int BytesPerPixel { get; }
    
    public PixelSpan(Span<byte> data, int width, int height, int bytesPerPixel)
    {
        _data = data;
        Width = width;
        Height = height;
        BytesPerPixel = bytesPerPixel;
    }
    
    public Span<byte> GetPixel(int x, int y)
    {
        var index = (y * Width + x) * BytesPerPixel;
        return _data.Slice(index, BytesPerPixel);
    }
    
    public void SetPixel(int x, int y, ReadOnlySpan<byte> pixel)
    {
        var index = (y * Width + x) * BytesPerPixel;
        pixel.CopyTo(_data.Slice(index, BytesPerPixel));
    }
}
```

### Benchmarking and Performance Analysis

#### Built-in Performance Monitoring
```csharp
/// <summary>Performance monitoring for raster operations</summary>
public class RasterPerformanceMonitor
{
    private readonly Dictionary<string, List<TimeSpan>> _operationTimes = new();
    
    public async Task<T> MeasureAsync<T>(string operation, Func<Task<T>> func)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            return await func();
        }
        finally
        {
            stopwatch.Stop();
            RecordOperation(operation, stopwatch.Elapsed);
        }
    }
    
    private void RecordOperation(string operation, TimeSpan duration)
    {
        if (!_operationTimes.ContainsKey(operation))
            _operationTimes[operation] = new List<TimeSpan>();
            
        _operationTimes[operation].Add(duration);
    }
    
    public void PrintStatistics()
    {
        foreach (var (operation, times) in _operationTimes)
        {
            var average = times.Average(t => t.TotalMilliseconds);
            var min = times.Min(t => t.TotalMilliseconds);
            var max = times.Max(t => t.TotalMilliseconds);
            
            Console.WriteLine($"{operation}: Avg={average:F2}ms, Min={min:F2}ms, Max={max:F2}ms");
        }
    }
}
```

## Testing Information

### Comprehensive Test Suite

#### Unit Testing Strategy
```csharp
/// <summary>Universal raster interface testing</summary>
[TestClass]
public class RasterInterfaceTests
{
    [TestMethod]
    public async Task IRaster_ShouldImplementUniversalInterface()
    {
        // Test with multiple formats
        var formats = new[]
        {
            await RasterFactory.CreateFromFileAsync("test.tiff"),
            await RasterFactory.CreateFromFileAsync("test.jpg"),
            await RasterFactory.CreateFromFileAsync("test.avif")
        };
        
        foreach (var raster in formats)
        {
            Assert.IsTrue(raster.Width > 0);
            Assert.IsTrue(raster.Height > 0);
            Assert.IsNotNull(raster.Metadata);
            
            await raster.DisposeAsync();
        }
    }
    
    [TestMethod]
    public async Task RasterMetadata_ShouldHandleLargeMetadata()
    {
        var raster = new TiffRaster();
        var metadata = raster.Metadata as IRasterMetadata;
        
        // Add large EXIF data
        metadata.ExifData = new byte[2_000_000]; // 2MB
        
        Assert.IsTrue(metadata.HasLargeMetadata);
        Assert.IsTrue(metadata.EstimatedMetadataSize > 1_000_000);
        
        // Test async disposal for large metadata
        var stopwatch = Stopwatch.StartNew();
        await raster.DisposeAsync();
        stopwatch.Stop();
        
        // Should complete within reasonable time
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000);
    }
}
```

#### Format-Specific Testing
```csharp
/// <summary>TIFF format specification testing</summary>
[TestClass]
public class TiffFormatTests
{
    [TestMethod]
    public void TiffRaster_ShouldSupportAllColorDepths()
    {
        var colorDepths = new[]
        {
            TiffColorDepth.Monochrome1Bit,
            TiffColorDepth.Grayscale8Bit,
            TiffColorDepth.TrueColor24Bit,
            TiffColorDepth.TrueColor48Bit
        };
        
        foreach (var depth in colorDepths)
        {
            var tiff = new TiffRaster { ColorDepth = depth };
            Assert.AreEqual(depth, tiff.ColorDepth);
        }
    }
    
    [TestMethod]
    public void TiffRaster_ShouldSupportAllCompressionTypes()
    {
        var compressions = new[]
        {
            TiffCompression.None,
            TiffCompression.LZW,
            TiffCompression.JPEG,
            TiffCompression.PackBits,
            TiffCompression.Deflate
        };
        
        foreach (var compression in compressions)
        {
            var tiff = new TiffRaster { Compression = compression };
            Assert.AreEqual(compression, tiff.Compression);
        }
    }
}
```

#### Performance Testing
```csharp
/// <summary>Performance benchmarking tests</summary>
[TestClass]
public class RasterPerformanceTests
{
    [TestMethod]
    public async Task RasterProcessing_ShouldMeetPerformanceTargets()
    {
        var testImage = CreateTestImage(1920, 1080);
        var monitor = new RasterPerformanceMonitor();
        
        // Test universal operations performance
        await monitor.MeasureAsync("Resize", async () =>
        {
            return await ResizeImageAsync(testImage, 800, 600);
        });
        
        await monitor.MeasureAsync("Compression", async () =>
        {
            return await CompressImageAsync(testImage, 85);
        });
        
        // Verify performance targets
        monitor.PrintStatistics();
        
        // Example assertions (adjust based on actual requirements)
        Assert.IsTrue(monitor.GetAverageTime("Resize") < TimeSpan.FromMilliseconds(500));
        Assert.IsTrue(monitor.GetAverageTime("Compression") < TimeSpan.FromMilliseconds(1000));
    }
    
    [TestMethod]
    public async Task LargeImageProcessing_ShouldUseOptimalMemory()
    {
        var largeImage = CreateTestImage(10000, 10000);
        var initialMemory = GC.GetTotalMemory(false);
        
        // Process large image
        await ProcessLargeImageAsync(largeImage);
        
        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        var finalMemory = GC.GetTotalMemory(false);
        var memoryIncrease = finalMemory - initialMemory;
        
        // Memory increase should be reasonable
        Assert.IsTrue(memoryIncrease < 100_000_000); // Less than 100MB
    }
}
```

### Integration Testing

#### Cross-Format Compatibility
```csharp
[TestMethod]
public async Task CrossFormatProcessing_ShouldWorkSeamlessly()
{
    var formats = new[] { "test.tiff", "test.jpg", "test.avif", "test.heif" };
    var pipeline = new RasterProcessingPipeline()
        .AddOperation(new ResizeOperation(800, 600))
        .AddOperation(new MetadataOperation());
    
    foreach (var format in formats)
    {
        await using var input = await RasterFactory.CreateFromFileAsync(format);
        await using var output = await pipeline.ProcessAsync(input);
        
        // Verify universal operations worked
        Assert.AreEqual(800, output.Width);
        Assert.AreEqual(600, output.Height);
        Assert.IsNotNull(output.Metadata);
    }
}
```

#### Validation Testing
```csharp
[TestMethod]
public void FormatValidation_ShouldDetectComplianceIssues()
{
    var validators = new IRasterValidator[]
    {
        new TiffValidator(),
        new JpegValidator(),
        new AvifValidator(),
        new HeifValidator()
    };
    
    foreach (var validator in validators)
    {
        var validImage = CreateValidImage(validator.SupportedFormat);
        var invalidImage = CreateInvalidImage(validator.SupportedFormat);
        
        var validResult = validator.Validate(validImage);
        var invalidResult = validator.Validate(invalidImage);
        
        Assert.IsTrue(validResult.IsValid);
        Assert.IsFalse(invalidResult.IsValid);
        Assert.IsTrue(invalidResult.Errors.Count > 0);
    }
}
```

## Contributing Guidelines

### Development Standards

#### Code Quality Requirements
1. **Comprehensive Testing**: All new formats must have >90% test coverage
2. **Performance Benchmarks**: Include performance tests for all operations
3. **Documentation**: Complete XML documentation for all public APIs
4. **Format Compliance**: Implement full specification compliance with validation
5. **Memory Efficiency**: Optimize for both memory usage and processing speed

#### Format Implementation Guidelines
1. **Universal Interface**: All formats must implement `IRaster` interface
2. **Metadata Support**: Comprehensive metadata handling with disposal optimization
3. **Validation**: Include format-specific validation with detailed error reporting
4. **Examples**: Provide usage examples and factory methods
5. **Constants**: Define format-specific constants and presets

### Adding New Format Support

#### Implementation Checklist
```csharp
// 1. Create format-specific interface
public interface INewFormatRaster : IRaster
{
    // Format-specific properties
    NewFormatColorSpace ColorSpace { get; set; }
    NewFormatCompression Compression { get; set; }
    NewFormatMetadata Metadata { get; set; }
}

// 2. Implement format class
public class NewFormatRaster : Raster, INewFormatRaster
{
    // Implementation with full specification support
}

// 3. Create metadata implementation
public class NewFormatMetadata : RasterMetadataBase, INewFormatMetadata
{
    // Format-specific metadata properties
}

// 4. Add validation
public class NewFormatValidator : RasterValidator<INewFormatRaster>
{
    // Format compliance validation
}

// 5. Add constants and examples
public static class NewFormatConstants
{
    // Format-specific constants
}

public static class NewFormatExamples
{
    // Usage examples and factory methods
}
```

#### Testing Requirements
1. **Unit Tests**: Test all format-specific functionality
2. **Integration Tests**: Test universal interface compliance
3. **Performance Tests**: Benchmark processing operations
4. **Validation Tests**: Test format compliance checking
5. **Memory Tests**: Verify disposal optimization

### Performance Optimization Guidelines

#### Memory Management
1. **Disposal Patterns**: Implement intelligent disposal based on metadata size
2. **Span Usage**: Use spans for efficient pixel processing
3. **Parallel Processing**: Support parallel operations for large images
4. **Memory Estimation**: Accurate memory usage estimation
5. **Resource Cleanup**: Proper cleanup of unmanaged resources

#### Processing Optimization
1. **Format-Specific**: Implement format-specific optimizations
2. **Tiled Processing**: Support tiled processing for large images
3. **SIMD Operations**: Use SIMD where appropriate
4. **Caching**: Implement intelligent caching strategies
5. **Batch Processing**: Optimize for batch operations

### Documentation Standards

#### API Documentation
1. **XML Comments**: Complete documentation for all public members
2. **Usage Examples**: Provide clear examples for common scenarios
3. **Performance Notes**: Document performance characteristics
4. **Thread Safety**: Specify thread safety guarantees
5. **Error Handling**: Document exception conditions

#### Format Documentation
1. **Specification Support**: Document which specification features are supported
2. **Limitations**: Clearly document any limitations
3. **Performance Characteristics**: Document memory and processing requirements
4. **Best Practices**: Provide usage recommendations
5. **Migration Guides**: Help users migrate between formats

## Dependencies

### Core Dependencies
- **Wangkanai.Graphics** - Core graphics abstractions and interfaces
- **.NET 9.0** - Target framework for modern language features
- **System.Drawing** - Basic graphics support and color management
- **System.Memory** - High-performance memory operations with spans
- **System.Numerics** - SIMD operations for pixel processing

### Format-Specific Dependencies
- **System.IO.Compression** - For formats supporting compression
- **System.Text.Json** - For metadata serialization
- **System.Xml** - For XML-based metadata formats

### Testing Dependencies
- **Microsoft.NET.Test.Sdk** - Test framework support
- **xunit** - Unit testing framework
- **xunit.runner.visualstudio** - Visual Studio test runner
- **BenchmarkDotNet** - Performance benchmarking

### Optional Dependencies
- **System.Drawing.Common** - Extended graphics support on Windows
- **SkiaSharp** - Cross-platform graphics operations
- **ImageSharp** - Alternative image processing library

---

## Summary

The Wangkanai Graphics Rasters library provides a comprehensive, high-performance solution for raster image processing with universal format support. Built on a unified abstraction layer, it enables developers to work with images consistently across formats while maintaining access to format-specific optimizations.

### Key Strengths

1. **Universal Interface**: Work with any supported format through common interfaces
2. **Performance Optimized**: Intelligent disposal, parallel processing, and memory management
3. **Comprehensive Format Support**: Full specification compliance for major formats
4. **Extensible Architecture**: Easy addition of new formats through interface implementation
5. **Professional Quality**: Handles everything from web images to professional photography

### Use Cases

- **Web Development**: Optimize images for web delivery with format-specific compression
- **Professional Photography**: Process RAW and high-quality images with metadata preservation
- **Scientific Imaging**: Handle specialized formats with precise data integrity
- **Digital Asset Management**: Batch process large collections with format optimization
- **Cross-Platform Applications**: Consistent image processing across different platforms

The library's intelligent resource management, comprehensive testing suite, and extensive documentation make it suitable for both small applications and large-scale enterprise solutions requiring reliable, high-performance image processing capabilities.