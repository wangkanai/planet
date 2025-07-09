# Graphics Format Specifications

## Overview
This document provides comprehensive technical specifications for all graphics file formats supported in the Planet project. The project implements a unified graphics architecture with support for both raster and vector formats, including specialized geospatial formats.

## Architecture

### Base Graphics Hierarchy
```
IImage (interface)
└── IMetadata (interface)
    └── MetadataBase (abstract class)
        ├── RasterMetadataBase (abstract class)
        │   └── Format-specific raster metadata implementations
        └── VectorMetadataBase (abstract class)
            └── Format-specific vector metadata implementations
```

### Core Components
- **Graphics Core**: Common interfaces and base classes (`IImage`, `IMetadata`, `MetadataBase`)
- **Raster Graphics**: Raster image processing with comprehensive format support
- **Vector Graphics**: Vector graphics processing with SVG implementation
- **Geospatial Integration**: GeoTIFF support through Spatial library integration

---

## Raster Formats

### TIFF (Tagged Image File Format)

#### **Technical Specifications**
- **Namespace**: `Wangkanai.Graphics.Rasters.Tiffs`
- **Main Class**: `TiffRaster`
- **Interface**: `ITiffRaster`
- **Metadata**: `TiffMetadata`
- **File Extensions**: `.tif`, `.tiff`
- **MIME Type**: `image/tiff`

#### **Format Capabilities**
- **Dimensions**: No practical limits (handled by int.MaxValue)
- **Color Depths**: 1, 4, 8, 16, 24, 32, 48, 64-bit
- **Color Models**: RGB, CMYK, YCbCr, CIE L*a*b*, ICC L*a*b*
- **Compression**: None, LZW, JPEG, PackBits, Deflate, CCITT Group 3/4, JBIG, JPEG 2000
- **Features**: Multi-page, tiled organization, strips, alpha channels, ICC profiles

#### **Implementation Details**
```csharp
// Color depth enumeration
public enum TiffColorDepth
{
    Bilevel = 1,
    FourBit = 4,
    EightBit = 8,
    SixteenBit = 16,
    TwentyFourBit = 24,
    ThirtyTwoBit = 32,
    FortyEightBit = 48,
    SixtyFourBit = 64
}

// Compression algorithms
public enum TiffCompression
{
    None = 1,
    CcittGroup3 = 2,
    CcittGroup4 = 4,
    Lzw = 5,
    Jpeg = 7,
    PackBits = 32773,
    Deflate = 32946
}
```

#### **Metadata Support**
- **Standard Tags**: Width, Height, BitsPerSample, Compression, PhotometricInterpretation
- **Camera Data**: Make, Model, DateTime, Artist, Copyright
- **Resolution**: XResolution, YResolution, ResolutionUnit
- **Color Management**: ICC Profile, WhitePoint, PrimaryChromaticities
- **Advanced**: Custom tags, EXIF IFD, GPS IFD, IPTC data

#### **Performance Optimizations**
- Optimized bits-per-sample storage using inline fields for common cases (≤4 samples)
- Efficient memory management with async disposal for large metadata
- Custom tag support with minimal overhead

---

### PNG (Portable Network Graphics)

#### **Technical Specifications**
- **Namespace**: `Wangkanai.Graphics.Rasters.Pngs`
- **Main Class**: `PngRaster`
- **Interface**: `IPngRaster`
- **Metadata**: `PngMetadata`
- **File Extensions**: `.png`
- **MIME Type**: `image/png`

#### **Format Capabilities**
- **Dimensions**: 1×1 to 2,147,483,647×2,147,483,647 pixels
- **Color Types**: Grayscale, RGB, Palette, Grayscale+Alpha, RGB+Alpha
- **Bit Depths**: 1, 2, 4, 8, 16 (varies by color type)
- **Compression**: DEFLATE with levels 0-9
- **Features**: Transparency, interlacing, gamma correction, color profiles

#### **Implementation Details**
```csharp
// Color type enumeration
public enum PngColorType : byte
{
    Grayscale = 0,
    Truecolor = 2,
    IndexedColor = 3,
    GrayscaleWithAlpha = 4,
    TruecolorWithAlpha = 6
}

// Samples per pixel calculation
public int SamplesPerPixel => ColorType switch
{
    PngColorType.Grayscale => 1,
    PngColorType.Truecolor => 3,
    PngColorType.IndexedColor => 1,
    PngColorType.GrayscaleWithAlpha => 2,
    PngColorType.TruecolorWithAlpha => 4
};
```

#### **Metadata Support**
- **Text chunks**: tEXt, zTXt, iTXt with full Unicode support
- **Standard chunks**: gAMA, pHYs, tIME, bKGD, tRNS
- **Color management**: sRGB, cHRM, iCCP chunks
- **Custom chunks**: Extensible metadata system
- **Validation**: Comprehensive metadata validation

#### **Performance Features**
- Automatic bit depth validation per color type
- Efficient chunk-based metadata storage
- Optimized compression level selection
- Memory-efficient transparency handling

---

### JPEG (Joint Photographic Experts Group)

#### **Technical Specifications**
- **Namespace**: `Wangkanai.Graphics.Rasters.Jpegs`
- **Main Class**: `JpegRaster`
- **Interface**: `IJpegRaster`
- **Metadata**: `JpegMetadata`
- **File Extensions**: `.jpg`, `.jpeg`, `.jpe`, `.jfif`
- **MIME Type**: `image/jpeg`

#### **Format Capabilities**
- **Dimensions**: Up to 65,535×65,535 pixels
- **Color Modes**: Grayscale, RGB, YCbCr, CMYK
- **Bit Depth**: 8 bits per component
- **Quality**: 0-100 (lossy compression)
- **Features**: Progressive encoding, EXIF, IPTC, XMP metadata

#### **Implementation Details**
```csharp
// Color mode support
public enum JpegColorMode
{
    Grayscale = 1,
    Rgb = 3,
    Cmyk = 4,
    YCbCr = 6
}

// Encoding types
public enum JpegEncoding
{
    Baseline = 0,
    Progressive = 1,
    Jpeg2000 = 2
}
```

#### **Metadata Support**
- **EXIF**: Camera settings, GPS data, technical parameters
- **IPTC**: Keywords, categories, editorial information
- **XMP**: Extensive metadata schema support
- **JFIF**: Resolution, thumbnail data
- **Custom**: Extensible metadata framework

#### **Compression Features**
- Dynamic compression ratio estimation
- Chroma subsampling support
- Progressive scan optimization
- Quality-based file size estimation

---

### WebP (Web Picture format)

#### **Technical Specifications**
- **Namespace**: `Wangkanai.Graphics.Rasters.WebPs`
- **Main Class**: `WebPRaster`
- **Interface**: `IWebPRaster`
- **Metadata**: `WebPMetadata`
- **File Extensions**: `.webp`
- **MIME Type**: `image/webp`

#### **Format Capabilities**
- **Dimensions**: 1×1 to 16,383×16,383 pixels
- **Color Modes**: RGB, RGBA
- **Compression**: VP8 (lossy), VP8L (lossless)
- **Quality**: 0-100 (lossy), 0-9 (lossless)
- **Features**: Animation, transparency, metadata chunks

#### **Implementation Details**
```csharp
// Format types
public enum WebPFormat : byte
{
    Simple = 0,     // VP8 lossy
    Lossless = 1,   // VP8L lossless
    Extended = 2    // VP8X with features
}

// Compression algorithms
public enum WebPCompression
{
    VP8,    // Lossy compression
    VP8L    // Lossless compression
}
```

#### **Advanced Features**
- **Animation**: Frame-based animation with disposal methods
- **Extended Format**: ICC profiles, EXIF, XMP support
- **Presets**: Picture, Photo, Drawing, Icon, Text optimizations
- **Memory Management**: Efficient ReadOnlyMemory usage

#### **Animation Support**
```csharp
public class WebPAnimationFrame
{
    public ushort OffsetX { get; set; }
    public ushort OffsetY { get; set; }
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public uint Duration { get; set; }
    public WebPDisposalMethod DisposalMethod { get; set; }
    public WebPBlendingMethod BlendingMethod { get; set; }
}
```

---

### AVIF (AV1 Image File Format)

#### **Technical Specifications**
- **Namespace**: `Wangkanai.Graphics.Rasters.Avifs`
- **Main Class**: `AvifRaster`
- **Interface**: `IAvifRaster`
- **Metadata**: `AvifMetadata`
- **File Extensions**: `.avif`
- **MIME Type**: `image/avif`

#### **Format Capabilities**
- **Dimensions**: Up to 65,536×65,536 pixels
- **Bit Depth**: 8-12 bits per channel
- **Color Spaces**: sRGB, Display P3, Rec. 2020
- **Compression**: AV1 codec with quality 0-100
- **Features**: HDR support, alpha channels, image sequences

#### **Implementation Details**
```csharp
// HDR support constants
public static class Hdr
{
    public const double SdrPeakBrightness = 100.0;
    public const double Hdr10PeakBrightness = 1000.0;
    public const double Hdr10PlusPeakBrightness = 4000.0;
    public const double DolbyVisionPeakBrightness = 10000.0;
}

// Quality presets
public static class QualityPresets
{
    public const int Lossless = 100;
    public const int Professional = 90;
    public const int Standard = 85;
    public const int Web = 75;
}
```

#### **Advanced Features**
- **HDR**: High Dynamic Range imaging support
- **Wide Color Gamut**: Extended color space support
- **Efficiency**: Superior compression compared to JPEG
- **Modern Standards**: Next-generation image format

---

### HEIF (High Efficiency Image Format)

#### **Technical Specifications**
- **Namespace**: `Wangkanai.Graphics.Rasters.Heifs`
- **Main Class**: `HeifRaster`
- **Interface**: `IHeifRaster`
- **Metadata**: `HeifMetadata`
- **File Extensions**: `.heic`, `.heif`
- **MIME Type**: `image/heif`

#### **Format Capabilities**
- **Dimensions**: Up to 65,536×65,536 pixels
- **Bit Depth**: 8-16 bits per channel
- **Compression**: HEVC/H.265 codec
- **Features**: Image sequences, burst photography, depth maps

#### **Container Features**
- **Multi-layered**: Support for multiple images in one file
- **Metadata Rich**: Extensive metadata support
- **Efficient**: Better compression than JPEG
- **Apple Integration**: Native support in iOS/macOS

---

### BMP (Bitmap Image File)

#### **Technical Specifications**
- **Namespace**: `Wangkanai.Graphics.Rasters.Bmps`
- **Main Class**: `BmpRaster`
- **Interface**: `IBmpRaster`
- **Metadata**: `BmpMetadata`
- **File Extensions**: `.bmp`
- **MIME Type**: `image/bmp`

#### **Format Capabilities**
- **Dimensions**: Up to 2,147,483,647×2,147,483,647 pixels
- **Color Depths**: 1, 4, 8, 16, 24, 32-bit
- **Compression**: None, RLE4, RLE8, Bit fields
- **Features**: Color palettes, alpha channels, compression

#### **Implementation Details**
```csharp
// Color depth enumeration
public enum BmpColorDepth
{
    Monochrome = 1,
    FourBit = 4,
    EightBit = 8,
    SixteenBit = 16,
    TwentyFourBit = 24,
    ThirtyTwoBit = 32
}

// Compression types
public enum BmpCompression
{
    None = 0,
    RLE8 = 1,
    RLE4 = 2,
    Bitfields = 3
}
```

---

### JPEG 2000

#### **Technical Specifications**
- **Namespace**: `Wangkanai.Graphics.Rasters.Jpeg2000s`
- **Main Class**: `Jpeg2000Raster`
- **Interface**: `IJpeg2000Raster`
- **Metadata**: `Jpeg2000Metadata`
- **File Extensions**: `.jp2`, `.j2k`, `.jpc`
- **MIME Type**: `image/jp2`

#### **Format Capabilities**
- **Dimensions**: Virtually unlimited
- **Bit Depth**: Up to 38 bits per component
- **Compression**: Wavelet-based, lossless/lossy
- **Features**: Progressive decoding, region of interest, error resilience

#### **Implementation Details**
```csharp
// Progression orders
public static class ProgressionOrders
{
    public const byte LRCP = 0; // Layer-Resolution-Component-Position
    public const byte RLCP = 1; // Resolution-Layer-Component-Position
    public const byte RPCL = 2; // Resolution-Position-Component-Layer
    public const byte PCRL = 3; // Position-Component-Resolution-Layer
    public const byte CPRL = 4; // Component-Position-Resolution-Layer
}

// Wavelet transforms
public static class WaveletTransforms
{
    public const byte Irreversible97 = 0; // 9/7 irreversible
    public const byte Reversible53 = 1;   // 5/3 reversible
}
```

#### **Advanced Features**
- **Scalability**: Resolution, quality, and spatial scalability
- **ROI**: Region of Interest encoding
- **Error Resilience**: Robust to transmission errors
- **Geospatial**: GeoJP2 extensions for geospatial metadata

---

## Vector Formats

### SVG (Scalable Vector Graphics)

#### **Technical Specifications**
- **Namespace**: `Wangkanai.Graphics.Vectors.Svgs`
- **Main Class**: `SvgVector`
- **Interface**: `ISvgVector`
- **Metadata**: `SvgMetadata`
- **File Extensions**: `.svg`, `.svgz` (compressed)
- **MIME Type**: `image/svg+xml`

#### **Format Capabilities**
- **Scalability**: Vector-based, resolution-independent
- **Versions**: SVG 1.0, 1.1, 2.0 support
- **Features**: Animations, scripting, styling, filters
- **Compression**: GZIP compression for SVGZ format

#### **Implementation Details**
```csharp
// ViewBox structure
public readonly struct SvgViewBox
{
    public double X { get; }
    public double Y { get; }
    public double Width { get; }
    public double Height { get; }
    public double AspectRatio => Height != 0 ? Width / Height : 0;
}

// Color space support
public enum SvgColorSpace
{
    sRGB,
    LinearRGB,
    DisplayP3,
    Rec2020,
    Custom
}
```

#### **Geospatial Integration**
- **Coordinate Systems**: Support for various CRS including WGS84, Web Mercator
- **Transformations**: Geographic to SVG coordinate space conversion
- **Projections**: Built-in support for map projections
- **Optimization**: Performance optimization for large datasets

```csharp
// Coordinate transformation
public Coordinate TransformToSvgSpace(Geodetic geodetic, GeographicBounds boundingBox)
{
    var normalizedX = (geodetic.Longitude - boundingBox.MinLongitude) /
                      (boundingBox.MaxLongitude - boundingBox.MinLongitude);
    var normalizedY = (boundingBox.MaxLatitude - geodetic.Latitude) /
                      (boundingBox.MaxLatitude - boundingBox.MinLatitude);
    
    var svgX = normalizedX * ViewBox.Width + ViewBox.X;
    var svgY = normalizedY * ViewBox.Height + ViewBox.Y;
    
    return new Coordinate(svgX, svgY);
}
```

#### **Performance Features**
- **Streaming**: Support for large SVG files
- **Compression**: Automatic GZIP compression for large files
- **Optimization**: Element count tracking and optimization
- **Memory Management**: Efficient async disposal for large documents

---

## Geospatial Formats

### GeoTIFF (Geographic Tagged Image File Format)

#### **Technical Specifications**
- **Namespace**: `Wangkanai.Spatial`
- **Main Class**: `GeoTiffRaster`
- **Interface**: `IGeoTiffRaster`
- **Base Class**: `TiffRaster` (inherits all TIFF capabilities)
- **File Extensions**: `.tif`, `.tiff` (with geospatial metadata)

#### **Geospatial Capabilities**
- **Coordinate Systems**: Full CRS support via EPSG codes
- **Georeferencing**: Geographic extent and transformation matrices
- **Projections**: Support for various map projections
- **Pixel Mapping**: Pixel-to-geographic coordinate transformation

#### **Implementation Details**
```csharp
public class GeoTiffRaster : TiffRaster, IGeoTiffRaster
{
    public string? CoordinateReferenceSystem { get; set; }
    public MapExtent? Extent { get; set; }
    public double[]? GeoTransform { get; set; }
    public bool IsGeoreferenced { get; }
    public double? PixelSizeX { get; }
    public double? PixelSizeY { get; }
}
```

#### **Geospatial Features**
- **Map Extent**: Geographic bounding box support
- **Transformation**: 6-parameter affine transformation
- **Pixel Size**: Geographic units per pixel
- **Integration**: Seamless integration with spatial operations

---

## Unified Metadata Architecture

### Base Metadata Properties
All formats share common metadata properties through the unified architecture:

```csharp
// Common to all formats
public abstract class MetadataBase : IMetadata
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string? Author { get; set; }
    public string? Copyright { get; set; }
    public string? Description { get; set; }
    public string? Software { get; set; }
    public DateTime? CreationTime { get; set; }
    public DateTime? ModificationTime { get; set; }
    public virtual bool HasLargeMetadata { get; }
    public virtual long EstimatedMetadataSize { get; }
}
```

### Raster-Specific Metadata
```csharp
public abstract class RasterMetadataBase : MetadataBase, IRasterMetadata
{
    public int BitDepth { get; set; }
    public byte[]? ExifData { get; set; }
    public string? XmpData { get; set; }
    public byte[]? IccProfile { get; set; }
}
```

### Vector-Specific Metadata
```csharp
public abstract class VectorMetadataBase : MetadataBase, IVectorMetadata
{
    public double ViewBoxWidth { get; set; }
    public double ViewBoxHeight { get; set; }
    public double ViewBoxX { get; set; }
    public double ViewBoxY { get; set; }
    public string? Title { get; set; }
}
```

### Resource Management
- **Disposable Pattern**: All metadata implements `IDisposable` and `IAsyncDisposable`
- **Memory Estimation**: Accurate memory usage estimation
- **Large Metadata Handling**: Optimized disposal for large metadata (>1MB)
- **Async Disposal**: Non-blocking disposal for large objects

### Memory Optimization
- **Efficient Storage**: Optimized data structures for common cases
- **Batch Operations**: Efficient batch processing for large datasets
- **Streaming**: Support for streaming large files
- **Caching**: Intelligent caching for frequently accessed metadata

---

## Performance Characteristics

### Format Comparison

| Format | Compression | Quality | Features | Use Case |
|--------|-------------|---------|----------|----------|
| **TIFF** | Lossless/Lossy | Excellent | Professional | Archival, Professional |
| **PNG** | Lossless | Excellent | Web-friendly | Web, Transparency |
| **JPEG** | Lossy | Good | Compact | Photography, Web |
| **WebP** | Both | Very Good | Modern | Web, Animation |
| **AVIF** | Both | Excellent | Next-gen | Modern Web, HDR |
| **HEIF** | Lossy | Excellent | Mobile | Mobile, Efficiency |
| **BMP** | None/RLE | Good | Simple | Legacy, Uncompressed |
| **JPEG 2000** | Both | Excellent | Advanced | Professional, Medical |
| **SVG** | Text/GZIP | Perfect | Scalable | Web, Icons, Maps |
| **GeoTIFF** | Various | Excellent | Geospatial | GIS, Mapping |

### Memory Usage
- **Small Images** (<1MB): Direct synchronous processing
- **Medium Images** (1-10MB): Optimized buffering and processing
- **Large Images** (>10MB): Streaming and async processing
- **Very Large Images** (>100MB): Tiled processing and memory management

### Performance Optimizations
- **Inline Storage**: Optimized storage for common cases
- **Batch Processing**: Efficient batch operations
- **Async Operations**: Non-blocking I/O operations
- **Memory Pooling**: Efficient memory management
- **Streaming**: Support for large file processing

---

## File Format Support Matrix

### Supported Features by Format

| Feature | TIFF | PNG | JPEG | WebP | AVIF | HEIF | BMP | JPEG2000 | SVG |
|---------|------|-----|------|------|------|------|-----|----------|-----|
| **Lossless** | ✓ | ✓ | ✗ | ✓ | ✓ | ✗ | ✓ | ✓ | ✓ |
| **Lossy** | ✓ | ✗ | ✓ | ✓ | ✓ | ✓ | ✗ | ✓ | ✗ |
| **Transparency** | ✓ | ✓ | ✗ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Animation** | ✓ | ✗ | ✗ | ✓ | ✓ | ✓ | ✗ | ✗ | ✓ |
| **Multi-page** | ✓ | ✗ | ✗ | ✗ | ✓ | ✓ | ✗ | ✗ | ✗ |
| **HDR** | ✓ | ✗ | ✗ | ✗ | ✓ | ✓ | ✗ | ✗ | ✗ |
| **Vector** | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ | ✓ |
| **Geospatial** | ✓* | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ | ✓* | ✓* |

*\* Via extensions (GeoTIFF, GeoJP2, GeospatialSVG)*

### Metadata Support

| Metadata Type | TIFF | PNG | JPEG | WebP | AVIF | HEIF | BMP | JPEG2000 | SVG |
|---------------|------|-----|------|------|------|------|-----|----------|-----|
| **EXIF** | ✓ | ✗ | ✓ | ✓ | ✓ | ✓ | ✗ | ✗ | ✗ |
| **XMP** | ✓ | ✗ | ✓ | ✓ | ✓ | ✓ | ✗ | ✓ | ✓ |
| **ICC Profile** | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Text/Comments** | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✗ | ✓ | ✓ |
| **Custom** | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✗ | ✓ | ✓ |

---

## Usage Examples

### Basic Raster Operations
```csharp
// Create a PNG image
using var png = new PngRaster(1920, 1080)
{
    ColorType = PngColorType.TruecolorWithAlpha,
    CompressionLevel = 9
};

png.PngMetadata.Author = "John Doe";
png.PngMetadata.Title = "Sample Image";
png.PngMetadata.CreationTime = DateTime.UtcNow;

// Create a TIFF image
using var tiff = new TiffRaster(800, 600)
{
    ColorDepth = TiffColorDepth.TwentyFourBit,
    Compression = TiffCompression.Lzw
};

tiff.TiffMetadata.Make = "Canon";
tiff.TiffMetadata.Model = "EOS R5";
```

### Vector Operations
```csharp
// Create an SVG
using var svg = new SvgVector(1000, 800);
svg.SetCoordinateReferenceSystem("EPSG:4326");

// Transform geographic coordinates
var geographic = new Geodetic(40.7128, -74.0060); // New York
var bounds = new GeographicBounds(-75, -73, 40, 41);
var svgPoint = svg.TransformToSvgSpace(geographic, bounds);
```

### Geospatial Operations
```csharp
// Create a GeoTIFF
var extent = new MapExtent(-180, -90, 180, 90);
using var geoTiff = new GeoTiffRaster(3600, 1800, "EPSG:4326", extent)
{
    GeoTransform = new double[] { -180, 0.1, 0, 90, 0, -0.1 }
};

// Check if georeferenced
if (geoTiff.IsGeoreferenced)
{
    var pixelSizeX = geoTiff.PixelSizeX; // 0.1 degrees
    var pixelSizeY = geoTiff.PixelSizeY; // -0.1 degrees
}
```

---

## Best Practices

### Format Selection
- **Web Images**: PNG (lossless), JPEG (photos), WebP (modern)
- **Professional Photography**: TIFF, JPEG 2000
- **Next-Generation**: AVIF, HEIF for modern applications
- **Vector Graphics**: SVG for scalable graphics
- **Geospatial**: GeoTIFF for raster geographic data

### Performance Optimization
- Use appropriate compression levels
- Implement async operations for large files
- Utilize metadata estimation for memory planning
- Consider streaming for very large files

### Memory Management
- Always dispose of resources properly
- Use async disposal for large metadata
- Monitor memory usage with EstimatedMetadataSize
- Implement appropriate caching strategies

### Error Handling
- Validate format-specific constraints
- Handle compression errors gracefully
- Implement robust metadata parsing
- Provide meaningful error messages

---

## Limitations and Considerations

### Format-Specific Limitations
- **JPEG**: 8-bit limitation, lossy compression only
- **PNG**: No built-in animation support
- **WebP**: Dimension limitations (16,383×16,383)
- **BMP**: Limited compression options
- **SVG**: Text-based, can be large for complex graphics

### Implementation Limitations
- Platform-specific codec availability
- Memory constraints for very large images
- Performance considerations for real-time applications
- Threading limitations for concurrent operations

### Future Enhancements
- Additional format support (JXL, JPEG XL)
- Hardware acceleration integration
- Advanced compression algorithms
- Enhanced geospatial capabilities
- WebAssembly support for web applications

---

## Conclusion

The Planet Graphics library provides comprehensive support for modern image formats with a unified architecture that enables efficient processing, metadata management, and geospatial integration. The implementation focuses on performance, memory efficiency, and extensibility while maintaining compatibility with industry standards.

For specific implementation details, refer to the source code in the respective namespace directories and the accompanying unit tests for usage examples.