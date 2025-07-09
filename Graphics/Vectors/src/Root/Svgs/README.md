# SVG Vector Graphics Technical Specification

## Overview

The SVG (Scalable Vector Graphics) implementation in the Planet Graphics library provides comprehensive support for SVG
1.1/2.0 specifications with advanced geospatial integration capabilities. This implementation is designed for
high-performance map tile rendering, geospatial data visualization, and scalable vector graphics processing.

## Key Features

### Core SVG Capabilities

- **Full SVG 1.1/2.0 Support**: Complete implementation of SVG specifications
- **Compressed Format Support**: SVGZ (gzip-compressed SVG) reading and writing
- **XML Processing**: Robust XML parsing and serialization with proper encoding
- **Document Optimization**: Performance-focused optimization for rendering
- **Validation**: Comprehensive SVG document validation

### Geospatial Integration

- **Coordinate System Transformations**: Support for EPSG:4326 (WGS84) and EPSG:3857 (Web Mercator)
- **Geographic Bounds**: Integration with geographic coordinate systems
- **Map Tile Rendering**: Optimized for map tile generation and visualization
- **CRS Metadata**: Coordinate reference system metadata handling

### Performance Optimizations

- **Streaming Support**: Efficient handling of large SVG files
- **Memory Management**: Intelligent disposal patterns for memory efficiency
- **Optimization Engine**: Built-in SVG optimization for better performance
- **Coordinate Precision**: Configurable coordinate precision for file size optimization

## Architecture

### Core Classes

#### `SvgVector`

```csharp
public class SvgVector : Vector, ISvgVector
{
    // Core properties
    public XDocument? Document { get; }
    public bool IsCompressed { get; }
    public string? SourceFilePath { get; }

    // Geospatial methods
    public Coordinate TransformToSvgSpace(Geodetic geodetic, GeographicBounds boundingBox);
    public Geodetic TransformToGeographic(Coordinate svgCoordinate, GeographicBounds boundingBox);
    public void SetCoordinateReferenceSystem(string crs);
    public void TransformCoordinateSystem(string fromCrs, string toCrs, GeographicBounds boundingBox);

    // File operations
    public Task SaveToFileAsync(string filePath, bool compressed = false);
    public Task LoadFromFileAsync(string filePath);

    // Document operations
    public string ToXmlString();
    public string ToFormattedXmlString();
    public void Optimize();
    public bool ValidateDocument();
}
```

#### `SvgMetadata`

```csharp
public class SvgMetadata : VectorMetadata, ISvgMetadata
{
    // Viewport properties
    public double ViewportWidth { get; set; }
    public double ViewportHeight { get; set; }
    public SvgViewBox ViewBox { get; set; }

    // Document properties
    public string Version { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int ElementCount { get; set; }
    public double TotalPathLength { get; set; }
    public bool IsCompressed { get; set; }

    // Geospatial properties
    public string? CoordinateReferenceSystem { get; set; }
    public Dictionary<string, string> Namespaces { get; set; }
}
```

#### `GeographicBounds`

```csharp
public class GeographicBounds
{
    public double MinLatitude { get; set; }
    public double MaxLatitude { get; set; }
    public double MinLongitude { get; set; }
    public double MaxLongitude { get; set; }

    public double Width { get; }
    public double Height { get; }
    public Geodetic Center { get; }
}
```

## Usage Examples

### Basic SVG Creation

```csharp
// Create a new SVG with dimensions
using var svg = new SvgVector(800, 600);

// Set title and description
svg.Metadata.Title = "My Map";
svg.Metadata.Description = "A sample geographic visualization";

// Save to file
await svg.SaveToFileAsync("map.svg");
```

### Loading and Processing SVG

```csharp
// Load existing SVG
using var svg = new SvgVector("input.svg", isFilePath: true);

// Optimize for performance
svg.Optimize();

// Validate document
if (svg.ValidateDocument())
{
    // Save optimized version
    await svg.SaveToFileAsync("optimized.svg");
}
```

### Geospatial Coordinate Transformation

```csharp
using var svg = new SvgVector(1024, 1024);

// Define geographic bounds
var bounds = new GeographicBounds
{
    MinLatitude = 35.0,
    MaxLatitude = 45.0,
    MinLongitude = -125.0,
    MaxLongitude = -115.0
};

// Transform geographic coordinate to SVG space
var geodetic = new Geodetic(40.0, -120.0);
var svgCoord = svg.TransformToSvgSpace(geodetic, bounds);

// Transform back to geographic coordinates
var backToGeo = svg.TransformToGeographic(svgCoord, bounds);
```

### Coordinate System Transformation

```csharp
using var svg = new SvgVector("wgs84_map.svg", isFilePath: true);

// Transform from WGS84 to Web Mercator
svg.TransformCoordinateSystem("EPSG:4326", "EPSG:3857", bounds);

// Set coordinate reference system
svg.SetCoordinateReferenceSystem("EPSG:3857");

await svg.SaveToFileAsync("web_mercator_map.svg");
```

### Compressed SVG (SVGZ) Support

```csharp
using var svg = new SvgVector(800, 600);

// ... create SVG content ...

// Save as compressed SVGZ
await svg.SaveToFileAsync("compressed.svgz", compressed: true);

// Load compressed SVG
using var loadedSvg = new SvgVector("compressed.svgz", isFilePath: true);
Console.WriteLine($"Is compressed: {loadedSvg.IsCompressed}");
```

## Technical Specifications

### Supported SVG Features

- **Elements**: All standard SVG elements (path, circle, rect, line, polygon, etc.)
- **Attributes**: Complete attribute support with validation
- **Namespaces**: XML namespace handling including custom namespaces
- **Styling**: CSS styling and inline styles
- **Text**: Text elements with proper encoding
- **Gradients**: Linear and radial gradients
- **Patterns**: Pattern definitions and usage
- **Clipping**: Clipping paths and masks

### File Format Support

- **SVG**: Standard SVG 1.1/2.0 format
- **SVGZ**: Gzip-compressed SVG format
- **Encoding**: UTF-8 encoding with optional BOM handling
- **XML Declaration**: Proper XML declaration handling

### Performance Characteristics

- **Memory Usage**: Efficient memory management with streaming support
- **File Size**: Optimization reduces file size by ~15-30%
- **Processing Speed**: Optimized XML processing with XDocument
- **Coordinate Precision**: Configurable precision (default: 3 decimal places)

## Integration with Planet Ecosystem

### Spatial Library Integration

```csharp
using Wangkanai.Spatial;
using Wangkanai.Spatial.Coordinates;

// Use spatial library coordinate types
var mercator = new Mercator();
var webMercatorCoord = mercator.LatLonToMeters(longitude, latitude);
```

### Graphics Library Integration

```csharp
// SVG inherits from Vector base class
Vector vector = new SvgVector(800, 600);

// Implements IMetadata interface
IMetadata metadata = vector.Metadata;
```

## Constants and Configuration

### SVG Constants

```csharp
public static class SvgConstants
{
    public const string SvgNamespace = "http://www.w3.org/2000/svg";
    public const string XLinkNamespace = "http://www.w3.org/1999/xlink";
    public const string DefaultVersion = "1.1";
    public const string CompressedFileExtension = ".svgz";
    public const int CoordinatePrecision = 3;
}
```

### SVG Types

```csharp
public struct SvgViewBox
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public static SvgViewBox Parse(string viewBoxString);
    public override string ToString();
}
```

## Testing

### Unit Tests

```csharp
[Fact]
public void Constructor_ShouldInitializeWithDefaultDimensions()
{
    using var svg = new SvgVector();
    Assert.Equal(100, svg.Width);
    Assert.Equal(100, svg.Height);
}

[Theory]
[InlineData(800, 600)]
[InlineData(1024, 768)]
public void Constructor_ShouldInitializeWithSpecifiedDimensions(int width, int height)
{
    using var svg = new SvgVector(width, height);
    Assert.Equal(width, svg.Width);
    Assert.Equal(height, svg.Height);
}
```

### Performance Tests

```csharp
[Fact]
public async Task SaveToFileAsync_ShouldOptimizeFileSize()
{
    using var svg = new SvgVector(1000, 1000);
    // ... populate with content ...

    await svg.SaveToFileAsync("unoptimized.svg");
    var originalSize = new FileInfo("unoptimized.svg").Length;

    svg.Optimize();
    await svg.SaveToFileAsync("optimized.svg");
    var optimizedSize = new FileInfo("optimized.svg").Length;

    Assert.True(optimizedSize < originalSize);
}
```

## Best Practices

### Performance Optimization

1. **Use Optimization**: Always call `Optimize()` before saving for production
2. **Coordinate Precision**: Use appropriate precision for your use case
3. **Compression**: Use SVGZ format for network transfer
4. **Memory Management**: Dispose SVG objects properly

### Geospatial Usage

1. **Coordinate Systems**: Always specify CRS when working with geographic data
2. **Bounds Validation**: Validate geographic bounds before transformations
3. **Precision**: Use appropriate coordinate precision for your zoom level
4. **Transformations**: Cache transformation results for repeated operations

### Code Quality

1. **Error Handling**: Always validate SVG documents after loading
2. **Resource Management**: Use `using` statements for proper disposal
3. **Async Operations**: Use async methods for file I/O operations
4. **Validation**: Validate input parameters and document structure

## Contributing

### Development Setup

1. Install .NET 9.0 SDK
2. Clone the repository
3. Navigate to `Graphics/Vectors/src/Root/Svgs`
4. Run `dotnet build` to build the project

### Running Tests

```bash
dotnet test Graphics/Vectors/tests/
```

### Code Standards

- Follow the coding guidelines in CLAUDE.md
- Use PascalCase for public members
- Include comprehensive XML documentation
- Write unit tests for all public methods
- Use async/await for I/O operations

### Performance Considerations

- Profile memory usage for large SVG files
- Benchmark coordinate transformations
- Optimize XML processing for better performance
- Consider streaming for very large documents

This SVG implementation provides a robust foundation for scalable vector graphics processing with specialized support
for geospatial applications and map tile rendering in the Planet ecosystem.
