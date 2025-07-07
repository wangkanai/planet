## Wangkanai Graphics Vectors

**Namespace:** `Wangkanai.Graphics.Vectors`

A comprehensive vector graphics processing library designed for scalable shape manipulation, geometric operations, and vector-to-raster conversions. Provides efficient handling of mathematical vector operations and geometric transformations.

## Features

- **Scalable Graphics**: Resolution-independent vector shape processing
- **Mathematical Operations**: Vector mathematics and geometric transformations
- **Shape Primitives**: Support for points, lines, polygons, and complex shapes
- **Rendering Support**: Vector-to-raster conversion capabilities
- **Metadata Management**: Rich metadata support for vector graphics
- **Format Support**: Multiple vector format handling and conversion
- **SVG Support**: Full SVG 1.1/2.0 specification compliance with geospatial integration
- **Coordinate Systems**: Geographic coordinate transformations (WGS84, Web Mercator)
- **Performance Optimization**: Efficient algorithms for vector operations

## Core Components

### Vector Processing
- **`Vector`** - Main vector graphics class implementing `IVector`
- **`SvgVector`** - SVG implementation with full SVG 1.1/2.0 compliance and geospatial features
- **Shape Primitives** - Basic geometric shapes and operations
- **Transformation Engine** - Matrix-based transformations (translate, rotate, scale)
- **Path Operations** - Complex path creation and manipulation

### SVG Processing
- **`SvgMetadata`** - Comprehensive SVG metadata management with performance tracking
- **`GeographicBounds`** - Geographic bounding box operations for spatial data
- **Coordinate Transformations** - Geographic to SVG coordinate space conversions
- **Compression Support** - SVGZ (compressed SVG) format support

## Vector Operations

### Mathematical Functions
- **Vector Addition/Subtraction** - Basic vector arithmetic
- **Dot and Cross Products** - Vector mathematical operations
- **Normalization** - Unit vector calculations
- **Distance Calculations** - Euclidean and Manhattan distances
- **Angle Computations** - Vector angle calculations

### Geometric Transformations
- **Translation** - Move vectors in 2D/3D space
- **Rotation** - Rotate vectors around points or axes
- **Scaling** - Resize vectors with aspect ratio control
- **Reflection** - Mirror vectors across axes
- **Shearing** - Skew transformations

### Shape Operations
- **Point Operations** - Single coordinate manipulations
- **Line Processing** - Line segment creation and intersection
- **Polygon Handling** - Complex polygon operations and tessellation
- **Curve Support** - Bezier curves and spline interpolation

## Usage

### Basic Vector Operations

```csharp
using Wangkanai.Graphics.Vectors;

// Create a vector
var vector = new Vector();

// Perform vector operations
var point1 = new Vector2(10, 20);
var point2 = new Vector2(30, 40);
var result = point1 + point2;

// Apply transformations
var transformed = vector.Rotate(45).Scale(2.0).Translate(100, 100);
```

### SVG Vector Graphics

```csharp
using Wangkanai.Graphics.Vectors.Svgs;
using Wangkanai.Spatial.Coordinates;

// Create an SVG vector with dimensions
using var svg = new SvgVector(800, 600);

// Load from file (supports both SVG and compressed SVGZ)
using var svgFromFile = new SvgVector("/path/to/map.svg", true);

// Set coordinate reference system
svg.SetCoordinateReferenceSystem("EPSG:4326");

// Transform geographic coordinates to SVG space
var geodetic = new Geodetic(45.0, -122.0); // Portland, OR
var bounds = new GeographicBounds(44.0, 46.0, -123.0, -121.0);
var svgCoordinate = svg.TransformToSvgSpace(geodetic, bounds);

// Transform coordinate systems
svg.TransformCoordinateSystem("EPSG:4326", "EPSG:3857", bounds);

// Optimize and save
svg.Optimize();
await svg.SaveToFileAsync("/path/to/output.svg", compressed: true);
```

## Vector Graphics Capabilities

- **Metadata Management**: Extract and modify vector metadata
- **Drawing Operations**: Programmatic vector shape creation
- **Compression**: Optimize vector data for storage
- **Format Conversion**: Convert between vector and raster formats
- **File Operations**: Save and load vector graphics in various formats
- **Rendering**: Convert vectors to raster images with anti-aliasing

## Supported Formats

### Implemented
- **SVG 1.1/2.0** - Scalable Vector Graphics with full specification compliance
- **SVGZ** - Compressed SVG format using GZip compression

### Geospatial Features
- **Coordinate Reference Systems**: EPSG:4326 (WGS84), EPSG:3857 (Web Mercator)
- **Geographic Bounds**: Bounding box operations for spatial data
- **Coordinate Transformations**: Bidirectional geographic ↔ SVG coordinate conversion

### Planned
- **PDF Vector** - Portable Document Format vector elements
- **PostScript** - PostScript vector format
- **WMF/EMF** - Windows Metafile formats

## Performance Features

- **SIMD Optimization** - Vectorized operations using System.Numerics
- **Parallel Processing** - Multi-threaded operations for large datasets
- **Memory Efficiency** - Optimized memory usage for complex shapes
- **Caching** - Intelligent caching for repeated operations

## Dependencies

- **Wangkanai.Graphics.Abstractions** - Core graphics interfaces
- **Wangkanai.Spatial** - Geospatial coordinate systems and transformations
- **.NET 9.0** - Target framework
- **System.Numerics** - Vector mathematics and SIMD operations
- **System.Drawing** - Basic graphics support
- **System.IO.Compression** - SVGZ compression support

## Integration

The Vectors library integrates seamlessly with:
- **Graphics.Rasters** - Vector-to-raster conversion
- **Spatial Library** - Geographic vector data processing
- **Graphics.Abstractions** - Common graphics interfaces

## Testing

Comprehensive unit tests covering:
- Vector mathematical operations
- Geometric transformations
- Shape manipulations
- SVG parsing and serialization
- Coordinate system transformations
- Geographic bounds operations
- Performance benchmarks
- Format conversions
- Error handling and disposal patterns
