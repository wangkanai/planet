# Planet Solution - API Documentation Index

> **Generated on**: 2025-01-19
> **Version**: 1.0.0
> **Target Framework**: .NET 9.0

## Overview

This comprehensive API documentation index covers all public APIs across the Planet solution modules: Graphics, Spatial,
Portal, Providers, and Protocols. The solution follows clean architecture principles with clear separation of concerns
and domain-driven design patterns.

## Table of Contents

- [Core Graphics APIs](#core-graphics-apis)
- [Spatial Data APIs](#spatial-data-apis)
- [Portal APIs](#portal-apis)
- [Provider APIs](#provider-apis)
- [Protocol APIs](#protocol-apis)
- [Extension Methods](#extension-methods)
- [Configuration APIs](#configuration-apis)
- [Error Handling](#error-handling)
- [Usage Patterns](#usage-patterns)

---

## Core Graphics APIs

### Base Interfaces

#### `IImage`

**Namespace**: `Wangkanai.Graphics`
**Purpose**: Base interface for all image objects

```csharp
public interface IImage : IDisposable, IAsyncDisposable
{
    int Width { get; set; }
    int Height { get; set; }
    IMetadata Metadata { get; }
}
```

**Usage Example**:

```csharp
using var image = new JpegRaster();
image.Width = 1920;
image.Height = 1080;
var metadata = image.Metadata;
```

#### `IMetadata`

**Namespace**: `Wangkanai.Graphics`
**Purpose**: Base metadata contract for all image formats

```csharp
public interface IMetadata : IDisposable, IAsyncDisposable
{
    int Width { get; set; }
    int Height { get; set; }
    string? Title { get; set; }
    int? Orientation { get; set; }
    bool HasLargeMetadata { get; }
    long EstimatedMetadataSize { get; }

    bool ValidateMetadata();
    void Clear();
    IMetadata Clone();
}
```

### Raster Image APIs

#### Core Raster Interface

```csharp
// Namespace: Wangkanai.Graphics.Rasters
public interface IRaster : IImage { }
```

#### Format-Specific Raster Interfaces

| Interface         | Namespace                    | Purpose               | Key Features                       |
|-------------------|------------------------------|-----------------------|------------------------------------|
| `IJpegRaster`     | `Wangkanai.Graphics.Rasters` | JPEG image processing | Compression, EXIF, quality control |
| `IPngRaster`      | `Wangkanai.Graphics.Rasters` | PNG image processing  | Transparency, compression levels   |
| `ITiffRaster`     | `Wangkanai.Graphics.Rasters` | TIFF image processing | Multi-page, compression options    |
| `IWebPRaster`     | `Wangkanai.Graphics.Rasters` | WebP image processing | Lossy/lossless, animation support  |
| `IAvifRaster`     | `Wangkanai.Graphics.Rasters` | AVIF image processing | High compression, HDR support      |
| `IHeifRaster`     | `Wangkanai.Graphics.Rasters` | HEIF image processing | Apple ecosystem, live photos       |
| `IBmpRaster`      | `Wangkanai.Graphics.Rasters` | BMP image processing  | Windows bitmap format              |
| `IJpeg2000Raster` | `Wangkanai.Graphics.Rasters` | JPEG 2000 processing  | Wavelet compression                |

#### Raster Metadata Types

```csharp
// JPEG Metadata
public class JpegMetadata : RasterMetadata
{
    public JpegChromaSubsampling ChromaSubsampling { get; set; }
    public JpegColorMode ColorMode { get; set; }
    public JpegEncoding Encoding { get; set; }
    // Camera, EXIF, GPS data...
}

// PNG Metadata
public class PngMetadata : RasterMetadata
{
    public PngColorType ColorType { get; set; }
    public PngCompression Compression { get; set; }
    public PngFilterMethod FilterMethod { get; set; }
    public PngInterlaceMethod InterlaceMethod { get; set; }
}

// WebP Metadata
public class WebPMetadata : RasterMetadata
{
    public WebPFormat Format { get; set; }
    public WebPCompression Compression { get; set; }
    public WebPColorMode ColorMode { get; set; }
    public WebPPreset Preset { get; set; }
}
```

### Vector Graphics APIs

#### Core Vector Interface

```csharp
// Namespace: Wangkanai.Graphics.Vectors
public interface IVector : IImage { }
public interface IVectorMetadata : IMetadata { }
```

#### SVG-Specific APIs

```csharp
// SVG Vector Interface
public interface ISvgVector : IVector { }

// SVG Metadata Interface
public interface ISvgMetadata : IVectorMetadata
{
    SvgVersion Version { get; set; }
    SvgColorSpace ColorSpace { get; set; }
    SvgCoordinateSystem CoordinateSystem { get; set; }
    GeographicBounds? GeographicBounds { get; set; }
}

// Geographic Bounds for SVG
public class GeographicBounds
{
    public double MinLatitude { get; set; }
    public double MaxLatitude { get; set; }
    public double MinLongitude { get; set; }
    public double MaxLongitude { get; set; }
}
```

### Validation APIs

#### Core Validation Types

```csharp
// Namespace: Wangkanai.Graphics.Validation
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationIssue> Issues { get; set; }
    public ValidationSeverity HighestSeverity { get; set; }
}

public enum ValidationSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}

public enum ValidationTypes
{
    Format = 1,
    Metadata = 2,
    Content = 4,
    Performance = 8,
    Security = 16,
    Compatibility = 32
}
```

#### Format-Specific Validators

```csharp
// JPEG Validation
public class JpegValidationResult
{
    public bool IsValidFormat { get; set; }
    public bool HasValidMarkers { get; set; }
    public List<string> Errors { get; set; }
    public List<string> Warnings { get; set; }
}

// PNG Validation
public class PngValidationResult
{
    public bool IsValidFormat { get; set; }
    public bool HasValidCrc { get; set; }
    public List<string> Errors { get; set; }
}
```

---

## Spatial Data APIs

### Core Spatial Types

#### `Coordinate`

**Namespace**: `Wangkanai.Spatial`
**Purpose**: Represents a 2D coordinate pair

```csharp
public class Coordinate
{
    public Coordinate() { }
    public Coordinate(double x, double y) { X = x; Y = y; }

    public double X { get; set; }  // Horizontal position
    public double Y { get; set; }  // Vertical position

    public override string ToString() => $"({X}, {Y})";
}
```

#### Coordinate System Implementations

```csharp
// Geodetic Coordinates (Lat/Lon)
public class Geodetic
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
}

// Mercator Projection Coordinates
public class Mercator
{
    public double X { get; set; }
    public double Y { get; set; }

    // Conversion methods
    public static Mercator FromGeodetic(Geodetic geodetic) { /* */ }
    public Geodetic ToGeodetic() { /* */ }
}
```

### Tile System APIs

#### Core Tile Interfaces

```csharp
public interface ITileSource
{
    string Name { get; }
    ITileSchema Schema { get; }
    Attribution Attribution { get; }
}

public interface ILocalTileSource : ITileSource { }

public interface ITileSchema
{
    string Name { get; }
    string Srs { get; }
    List<Resolution> Resolutions { get; }
    Extent Extent { get; }
}
```

#### Tile Data Types

```csharp
// Tile Information
public class TileInfo
{
    public TileIndex Index { get; set; }
    public byte[]? Data { get; set; }
    public DateTime? LastModified { get; set; }
}

// Tile Addressing
public class TileAddress
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }  // Zoom level
}

// Tile Pixel Coordinates
public class TilePixel
{
    public int X { get; set; }
    public int Y { get; set; }
    public TileIndex TileIndex { get; set; }
}
```

### Map Extent and Resolution

```csharp
// Map Extent (Bounding Box)
public class MapExtent
{
    public double MinX { get; set; }
    public double MinY { get; set; }
    public double MaxX { get; set; }
    public double MaxY { get; set; }

    public double Width => MaxX - MinX;
    public double Height => MaxY - MinY;
    public Coordinate Center => new((MinX + MaxX) / 2, (MinY + MaxY) / 2);
}

// Resolution Definition
public class Resolution
{
    public int Id { get; set; }
    public double UnitsPerPixel { get; set; }
    public double ScaleDenominator { get; set; }
}
```

### Format-Specific APIs

#### MBTiles Support

```csharp
// Namespace: Wangkanai.Spatial.MbTiles
public enum MbTileFormat
{
    Png,
    Jpg,
    WebP,
    Pbf
}

public enum MbTileType
{
    BaseLayer,
    Overlay
}
```

#### GeoTIFF Integration

```csharp
// Namespace: Wangkanai.Spatial.GeoTiffs
public interface IGeoTiffRaster : ITiffRaster
{
    GeodeticCoordinates GeodeticBounds { get; set; }
    ProjectionInfo ProjectionInfo { get; set; }
}
```

---

## Portal APIs

### Identity and User Management

#### Core Identity Types

```csharp
// Namespace: Wangkanai.Planet.Portal.Identity
public sealed class PlanetUser : IdentityUser<int>
{
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public DateOnly Birthday { get; set; }
    public PlanetTheme Theme { get; set; }
}

public sealed class PlanetRole : IdentityRole<int>
{
    // Extended role properties
}
```

#### Permission and Module System

```csharp
public enum PlanetPermissions
{
    Read = 1,
    Write = 2,
    Delete = 4,
    Admin = 8
}

public enum PlanetModules
{
    Dashboard = 1,
    Maps = 2,
    Graphics = 4,
    Administration = 8
}

public enum PlanetTheme
{
    Light,
    Dark,
    Auto
}
```

### Domain Models

#### Generic Types

```csharp
// Namespace: Wangkanai.Planet.Portal.Domain.Generic
public enum Color
{
    Primary,
    Secondary,
    Success,
    Warning,
    Danger,
    Info,
    Light,
    Dark
}
```

### Data Context

```csharp
// Namespace: Wangkanai.Planet.Portal.Persistence
public class PlanetDbContext : IdentityDbContext<PlanetUser, PlanetRole, int>
{
    public PlanetDbContext(DbContextOptions<PlanetDbContext> options) : base(options) { }

    // DbSets for domain entities
    protected override void OnModelCreating(ModelBuilder builder) { /* */ }
}
```

---

## Provider APIs

### Remote Map Service Providers

#### Core Provider Interface

```csharp
// Namespace: Wangkanai.Planet.Providers
public interface IRemoteProvider
{
    /// <summary>Generates a URL for a tile based on coordinates and zoom level</summary>
    string GetTileUrl(int x, int y, int z);
}
```

#### Provider Implementations

```csharp
public enum RemoteProviders
{
    Bing,
    Google,
    OpenStreetMap
}

// Bing Maps Provider
public class BingProvider : IRemoteProvider
{
    public string GetTileUrl(int x, int y, int z) { /* */ }
}

// Google Maps Provider
public class GoogleProvider : IRemoteProvider
{
    public string GetTileUrl(int x, int y, int z) { /* */ }
}
```

---

## Protocol APIs

### Web Map Service (WMS)

#### WMS Version Support

```csharp
// Namespace: Wangkanai.Planet.Protocols.Wms
public enum WmsVersions
{
    V1_0_0,
    V1_1_0,
    V1_1_1,
    V1_3_0
}
```

---

## Extension Methods

### Graphics Extensions

#### Metadata Extensions

```csharp
// Namespace: Wangkanai.Graphics.Extensions
public static class MetadataExtensions
{
    public static bool IsEmpty(this IMetadata metadata) { /* */ }
    public static void CopyTo(this IMetadata source, IMetadata target) { /* */ }
    public static TMetadata As<TMetadata>(this IMetadata metadata) where TMetadata : IMetadata { /* */ }
}

public static class MetadataValidationExtensions
{
    public static ValidationResult ValidateCompleteness(this IMetadata metadata) { /* */ }
    public static ValidationResult ValidateFormat(this IMetadata metadata) { /* */ }
}

public static class MetadataComparisonExtensions
{
    public static MetadataComparisonResult Compare(this IMetadata source, IMetadata target) { /* */ }
    public static bool IsEquivalent(this IMetadata source, IMetadata target) { /* */ }
}
```

#### Format-Specific Extensions

```csharp
// JPEG Metadata Extensions
public static class JpegMetadataExtensions
{
    public static void AddExifTag(this JpegMetadata metadata, string tag, object value) { /* */ }
    public static void AddIptcTag(this JpegMetadata metadata, string tag, string value) { /* */ }
    public static void AddXmpTag(this JpegMetadata metadata, string namespace, string tag, string value) { /* */ }
    public static ValidationResult ValidateCameraSettings(this JpegMetadata metadata) { /* */ }
}

// PNG Metadata Extensions
public static class PngMetadataExtensions
{
    public static void AddTextChunk(this PngMetadata metadata, string keyword, string text) { /* */ }
    public static void AddColorProfile(this PngMetadata metadata, byte[] profile) { /* */ }
}
```

### Raster Extensions

```csharp
// Namespace: Wangkanai.Graphics.Rasters.Extensions
public static class RasterMetadataExtensions
{
    public static bool IsHighDynamicRange(this IRasterMetadata metadata) { /* */ }
    public static bool HasAlphaChannel(this IRasterMetadata metadata) { /* */ }
    public static ColorSpace GetColorSpace(this IRasterMetadata metadata) { /* */ }
}

public static class RasterMetadataComparisonExtensions
{
    public static RasterComparisonResult CompareImageProperties(this IRasterMetadata source, IRasterMetadata target) { /* */ }
    public static bool HasSimilarQuality(this IRasterMetadata source, IRasterMetadata target) { /* */ }
}
```

### Vector Extensions

```csharp
// Namespace: Wangkanai.Graphics.Vectors.Extensions
public static class VectorMetadataExtensions
{
    public static VectorComplexityLevel AnalyzeComplexity(this IVectorMetadata metadata) { /* */ }
    public static bool IsGeospatialVector(this IVectorMetadata metadata) { /* */ }
}

public static class SvgMetadataExtensions
{
    public static SvgComplexityLevel AnalyzeComplexity(this ISvgMetadata metadata) { /* */ }
    public static bool IsInteractive(this ISvgMetadata metadata) { /* */ }
    public static bool IsAnimated(this ISvgMetadata metadata) { /* */ }
}
```

### Spatial Extensions

```csharp
// Namespace: Wangkanai.Planet.Providers.Extensions
public static class TileExtensions
{
    public static string FormatTileUrl(this string template, int x, int y, int z) { /* */ }
    public static bool IsValidTileCoordinate(int x, int y, int z) { /* */ }
}
```

---

## Configuration APIs

### Application Configuration

```csharp
// Namespace: Wangkanai.Planet.Portal.Application
public static class PlanetConstants
{
    public const string DatabaseConnectionString = "DefaultConnection";
    public const string ApplicationName = "Planet Portal";
    public const string Version = "1.0.0";
}
```

### Identity Configuration

```csharp
// Namespace: Wangkanai.Planet.Portal.Application.Identity
public class UserConfiguration : IEntityTypeConfiguration<PlanetUser>
{
    public void Configure(EntityTypeBuilder<PlanetUser> builder) { /* */ }
}

public class RoleConfiguration : IEntityTypeConfiguration<PlanetRole>
{
    public void Configure(EntityTypeBuilder<PlanetRole> builder) { /* */ }
}
```

---

## Error Handling

### Exception Types

#### Graphics Exceptions

```csharp
// Namespace: Wangkanai.Graphics.Exceptions
public class ImageException : Exception
{
    public ImageException() { }
    public ImageException(string message) : base(message) { }
    public ImageException(string message, Exception innerException) : base(message, innerException) { }
}
```

#### Common Exception Patterns

```csharp
// Invalid Operations
throw new InvalidOperationException($"Cannot add text chunk: {string.Join("; ", validation.Errors)}");

// Not Supported Operations
throw new NotSupportedException($"Unsupported color depth: {ColorDepth}");

// Argument Validation
throw new ArgumentException($"Invalid camera settings: {string.Join("; ", validation.Errors)}");

// Data Validation
throw new ArgumentException("Invalid HEIF data: too small", nameof(data));
```

### Error Handling Patterns

#### Validation Results

```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);
}
```

#### Async Error Handling

```csharp
// Proper disposal patterns
public async ValueTask DisposeAsync()
{
    try
    {
        // Cleanup resources
        await CleanupAsync();
    }
    catch (Exception ex)
    {
        // Log but don't throw in disposal
        Logger.LogError(ex, "Error during disposal");
    }
}
```

---

## Usage Patterns

### Basic Image Processing

```csharp
// JPEG Processing
using var jpeg = new JpegRaster();
jpeg.Width = 1920;
jpeg.Height = 1080;

var metadata = jpeg.Metadata as JpegMetadata;
metadata.ChromaSubsampling = JpegChromaSubsampling.Yuv420;
metadata.AddExifTag("Camera", "Canon EOS R5");

var validation = jpeg.ValidateMetadata();
if (!validation.IsValid)
{
    foreach (var error in validation.Errors)
        Console.WriteLine($"Error: {error}");
}
```

### Spatial Data Processing

```csharp
// Coordinate Transformation
var geodetic = new Geodetic { Latitude = 40.7128, Longitude = -74.0060 };
var mercator = Mercator.FromGeodetic(geodetic);

// Tile URL Generation
var provider = new BingProvider();
var tileUrl = provider.GetTileUrl(x: 1205, y: 1539, z: 12);
```

### Metadata Comparison

```csharp
// Compare two images
var result = image1.Metadata.Compare(image2.Metadata);
if (result.AreSimilar)
{
    Console.WriteLine($"Images are {result.SimilarityScore:P} similar");
}
```

### Async Resource Management

```csharp
// Proper async disposal
await using var raster = new AvifRaster();
await raster.LoadAsync(stream);

// Process image
var processed = await raster.ProcessAsync();

// Resources are automatically disposed
```

---

## Version Information

- **Current Version**: 1.0.0
- **Target Framework**: .NET 9.0
- **Language Features**: C# 13, Nullable Reference Types
- **Architecture**: Clean Architecture, Domain-Driven Design
- **Testing**: xUnit v3, BenchmarkDotNet for performance testing

## Additional Resources

- **Architecture Documentation**: [ARCHITECTURE_INDEX.md](ARCHITECTURE_INDEX.md)
- **Development Guide**: [technical-implementation-guide.md](technical-implementation-guide.md)
- **Graphics Documentation**: [Graphics/docs/README.md](../Graphics/docs/README.md)
- **Project Roadmap**: [development-roadmap.md](development-roadmap.md)

---

*This documentation is automatically maintained and reflects the current state of the Planet solution APIs.*
