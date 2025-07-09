# GeoJSON Vector Technical Specification

## Overview

The GeoJSON implementation in the Planet Graphics library provides comprehensive support for the GeoJSON format according to RFC 7946 standards. This implementation enables robust vector geospatial data handling with seamless integration into the Planet ecosystem, supporting all seven geometry types, feature collections, and coordinate reference systems for modern geospatial applications.

**Specification Reference**: [GitHub Issue #102](https://github.com/wangkanai/planet/issues/102)  
**Standard**: RFC 7946 - The GeoJSON Format  
**Namespace**: `Wangkanai.Graphics.Vectors.GeoJsons`  
**File Extensions**: `.geojson`, `.json`  
**MIME Type**: `application/geo+json`

## Key Features

### RFC 7946 Compliance
- **Complete Geometry Support**: All seven geometry types (Point, MultiPoint, LineString, MultiLineString, Polygon, MultiPolygon, GeometryCollection)
- **Feature Objects**: Full support for Feature and FeatureCollection with properties
- **Coordinate Reference System**: WGS84 (EPSG:4326) as default with named CRS support
- **Validation Framework**: Comprehensive RFC 7946 compliance validation

### Advanced Processing Capabilities
- **Streaming Support**: Memory-efficient processing for large datasets
- **System.Text.Json Integration**: High-performance JSON parsing and serialization for .NET 9.0
- **Spatial Indexing**: R-tree indexing for efficient spatial queries
- **Cross-Format Conversion**: Seamless conversion to/from Shapefile, WKT, and other formats

### Professional Features
- **Large Dataset Handling**: Chunk-based processing and memory-mapped file support
- **Coordinate Transformations**: Integration with Wangkanai.Spatial coordinate systems
- **Bounding Box Calculations**: Automatic spatial extent computation
- **Property Schema Validation**: Custom metadata and properties validation

## Architecture

### Core Classes

#### `GeoJsonVector`
```csharp
public class GeoJsonVector : Vector, IGeoJsonVector
{
    // Core properties
    public GeoJsonFeatureCollection FeatureCollection { get; set; }
    public string CoordinateReferenceSystem { get; set; } = "EPSG:4326";
    public GeoJsonBounds BoundingBox { get; set; }
    public bool HasBoundingBox { get; set; }

    // Metadata access
    public GeoJsonMetadata Metadata { get; }

    // I/O operations
    public Task LoadFromFileAsync(string filePath, GeoJsonReadOptions? options = null);
    public Task SaveToFileAsync(string filePath, GeoJsonWriteOptions? options = null);
    public Task LoadFromStreamAsync(Stream stream, GeoJsonReadOptions? options = null);
    public Task SaveToStreamAsync(Stream stream, GeoJsonWriteOptions? options = null);

    // Streaming operations
    public IAsyncEnumerable<GeoJsonFeature> ReadFeaturesAsync(Stream stream);
    public Task WriteFeaturesAsync(IAsyncEnumerable<GeoJsonFeature> features, Stream stream);

    // Geometry operations
    public void AddFeature(GeoJsonFeature feature);
    public void RemoveFeature(string id);
    public GeoJsonFeature? GetFeature(string id);
    public IEnumerable<GeoJsonFeature> GetFeaturesInBounds(GeoJsonBounds bounds);

    // Spatial operations
    public GeoJsonBounds CalculateBounds();
    public void UpdateBoundingBox();
    public GeoJsonFeatureCollection FilterByBounds(GeoJsonBounds bounds);

    // Validation
    public GeoJsonValidationResult Validate();
    public bool IsValidRfc7946();
}
```

#### `GeoJsonFeatureCollection`
```csharp
public class GeoJsonFeatureCollection
{
    public string Type { get; } = "FeatureCollection";
    public GeoJsonFeature[] Features { get; set; }
    public GeoJsonBounds? BoundingBox { get; set; }
    public Dictionary<string, object>? ForeignMembers { get; set; }

    // Collection operations
    public void Add(GeoJsonFeature feature);
    public bool Remove(string id);
    public GeoJsonFeature? GetById(string id);
    public IEnumerable<GeoJsonFeature> Filter(Func<GeoJsonFeature, bool> predicate);

    // Spatial queries
    public IEnumerable<GeoJsonFeature> GetFeaturesIntersecting(GeoJsonGeometry geometry);
    public IEnumerable<GeoJsonFeature> GetFeaturesWithin(GeoJsonBounds bounds);
    public GeoJsonFeatureCollection Clip(GeoJsonBounds bounds);

    // Statistics
    public int Count { get; }
    public GeoJsonBounds CalculateBounds();
    public Dictionary<string, int> GetGeometryTypeCounts();
}
```

#### `GeoJsonFeature`
```csharp
public class GeoJsonFeature
{
    public string Type { get; } = "Feature";
    public string? Id { get; set; }
    public GeoJsonGeometry? Geometry { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
    public GeoJsonBounds? BoundingBox { get; set; }

    // Property access
    public T? GetProperty<T>(string key);
    public void SetProperty<T>(string key, T value);
    public bool HasProperty(string key);
    public void RemoveProperty(string key);

    // Geometry operations
    public GeoJsonBounds CalculateBounds();
    public bool Intersects(GeoJsonGeometry other);
    public bool Contains(GeoJsonPoint point);
    public double CalculateArea();
    public double CalculateLength();
}
```

#### `GeoJsonGeometry` (Base Class)
```csharp
public abstract class GeoJsonGeometry
{
    public abstract string Type { get; }
    public GeoJsonBounds? BoundingBox { get; set; }
    
    // Spatial operations
    public abstract GeoJsonBounds CalculateBounds();
    public abstract bool IsValid();
    public abstract GeoJsonGeometry Transform(ICoordinateTransformation transformation);
    
    // Factory methods
    public static GeoJsonPoint CreatePoint(double longitude, double latitude, double? altitude = null);
    public static GeoJsonLineString CreateLineString(GeoJsonPosition[] coordinates);
    public static GeoJsonPolygon CreatePolygon(GeoJsonPosition[][] rings);
}
```

#### `GeoJsonPoint`
```csharp
public class GeoJsonPoint : GeoJsonGeometry
{
    public override string Type => "Point";
    public GeoJsonPosition Coordinates { get; set; }

    public GeoJsonPoint(double longitude, double latitude, double? altitude = null)
    {
        Coordinates = new GeoJsonPosition(longitude, latitude, altitude);
    }

    // Point-specific operations
    public double DistanceTo(GeoJsonPoint other);
    public GeoJsonPoint ProjectTo(string targetCrs);
    public bool Equals(GeoJsonPoint other, double tolerance = 1e-9);
}
```

#### `GeoJsonLineString`
```csharp
public class GeoJsonLineString : GeoJsonGeometry
{
    public override string Type => "LineString";
    public GeoJsonPosition[] Coordinates { get; set; }

    // LineString operations
    public double CalculateLength();
    public GeoJsonPoint GetPointAt(double distance);
    public GeoJsonLineString Simplify(double tolerance);
    public bool IsClosed();
    public GeoJsonLineString Reverse();
}
```

#### `GeoJsonPolygon`
```csharp
public class GeoJsonPolygon : GeoJsonGeometry
{
    public override string Type => "Polygon";
    public GeoJsonPosition[][] Coordinates { get; set; } // [exterior, ...holes]

    // Polygon operations
    public double CalculateArea();
    public double CalculatePerimeter();
    public bool ContainsPoint(GeoJsonPoint point);
    public GeoJsonPolygon Buffer(double distance);
    public bool IsClockwise();
    public void EnsureClockwise();
    public bool HasHoles();
    public GeoJsonLineString ExteriorRing { get; }
    public GeoJsonLineString[] InteriorRings { get; }
}
```

#### `GeoJsonMultiPoint`
```csharp
public class GeoJsonMultiPoint : GeoJsonGeometry
{
    public override string Type => "MultiPoint";
    public GeoJsonPosition[] Coordinates { get; set; }

    // MultiPoint operations
    public GeoJsonPoint[] GetPoints();
    public void AddPoint(GeoJsonPoint point);
    public bool RemovePoint(GeoJsonPoint point);
    public GeoJsonPoint GetNearestPoint(GeoJsonPoint target);
}
```

#### `GeoJsonMultiLineString`
```csharp
public class GeoJsonMultiLineString : GeoJsonGeometry
{
    public override string Type => "MultiLineString";
    public GeoJsonPosition[][] Coordinates { get; set; }

    // MultiLineString operations
    public GeoJsonLineString[] GetLineStrings();
    public double CalculateTotalLength();
    public void AddLineString(GeoJsonLineString lineString);
    public bool RemoveLineString(int index);
}
```

#### `GeoJsonMultiPolygon`
```csharp
public class GeoJsonMultiPolygon : GeoJsonGeometry
{
    public override string Type => "MultiPolygon";
    public GeoJsonPosition[][][] Coordinates { get; set; }

    // MultiPolygon operations
    public GeoJsonPolygon[] GetPolygons();
    public double CalculateTotalArea();
    public void AddPolygon(GeoJsonPolygon polygon);
    public bool RemovePolygon(int index);
    public bool ContainsPoint(GeoJsonPoint point);
}
```

#### `GeoJsonGeometryCollection`
```csharp
public class GeoJsonGeometryCollection : GeoJsonGeometry
{
    public override string Type => "GeometryCollection";
    public GeoJsonGeometry[] Geometries { get; set; }

    // Collection operations
    public void AddGeometry(GeoJsonGeometry geometry);
    public bool RemoveGeometry(GeoJsonGeometry geometry);
    public T[] GetGeometriesOfType<T>() where T : GeoJsonGeometry;
    public int Count { get; }
}
```

#### `GeoJsonPosition`
```csharp
public struct GeoJsonPosition : IEquatable<GeoJsonPosition>
{
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public double? Altitude { get; set; }

    public GeoJsonPosition(double longitude, double latitude, double? altitude = null)
    {
        Longitude = longitude;
        Latitude = latitude;
        Altitude = altitude;
    }

    // Conversion methods
    public Geodetic ToGeodetic();
    public Mercator ToMercator();
    public static implicit operator GeoJsonPosition(double[] coordinates);
    public static implicit operator double[](GeoJsonPosition position);

    // Utility methods
    public double DistanceTo(GeoJsonPosition other);
    public GeoJsonPosition Transform(ICoordinateTransformation transformation);
    public bool IsValid();
}
```

#### `GeoJsonMetadata`
```csharp
public class GeoJsonMetadata : VectorMetadata, IGeoJsonMetadata
{
    // Format-specific metadata
    public string Version { get; set; } = "1.0";
    public string CoordinateReferenceSystem { get; set; } = "EPSG:4326";
    public GeoJsonBounds? BoundingBox { get; set; }

    // Feature statistics
    public int FeatureCount { get; set; }
    public Dictionary<string, int> GeometryTypeCounts { get; set; }
    public string[] PropertyNames { get; set; }

    // Spatial metadata
    public double TotalArea { get; set; }
    public double TotalLength { get; set; }
    public GeoJsonPosition Centroid { get; set; }

    // Validation results
    public bool IsRfc7946Compliant { get; set; }
    public GeoJsonValidationResult ValidationResults { get; set; }

    // Processing metadata
    public DateTime LastModified { get; set; }
    public long FileSizeBytes { get; set; }
    public string? Source { get; set; }
    public Dictionary<string, object> CustomMetadata { get; set; }
}
```

#### `GeoJsonReader`
```csharp
public class GeoJsonReader : IGeoJsonReader
{
    public Task<GeoJsonFeatureCollection> ReadAsync(Stream stream, GeoJsonReadOptions? options = null);
    public Task<GeoJsonFeatureCollection> ReadAsync(string filePath, GeoJsonReadOptions? options = null);
    public IAsyncEnumerable<GeoJsonFeature> ReadFeaturesAsync(Stream stream, GeoJsonReadOptions? options = null);

    // Streaming methods
    public Task<GeoJsonGeometry> ReadGeometryAsync(Stream stream);
    public Task<GeoJsonFeature> ReadFeatureAsync(Stream stream);
    
    // Validation during read
    public Task<GeoJsonValidationResult> ValidateAsync(Stream stream);
    public bool ValidateWhileReading { get; set; } = true;
}
```

#### `GeoJsonWriter`
```csharp
public class GeoJsonWriter : IGeoJsonWriter
{
    public Task WriteAsync(GeoJsonFeatureCollection collection, Stream stream, GeoJsonWriteOptions? options = null);
    public Task WriteAsync(GeoJsonFeatureCollection collection, string filePath, GeoJsonWriteOptions? options = null);
    public Task WriteFeaturesAsync(IAsyncEnumerable<GeoJsonFeature> features, Stream stream, GeoJsonWriteOptions? options = null);

    // Individual object writing
    public Task WriteGeometryAsync(GeoJsonGeometry geometry, Stream stream);
    public Task WriteFeatureAsync(GeoJsonFeature feature, Stream stream);

    // Formatting options
    public bool PrettyPrint { get; set; } = false;
    public bool IncludeBoundingBoxes { get; set; } = false;
    public int CoordinatePrecision { get; set; } = 6;
}
```

#### `GeoJsonValidator`
```csharp
public static class GeoJsonValidator
{
    public static GeoJsonValidationResult Validate(GeoJsonFeatureCollection collection);
    public static GeoJsonValidationResult ValidateGeometry(GeoJsonGeometry geometry);
    public static GeoJsonValidationResult ValidateFeature(GeoJsonFeature feature);
    
    // Specific validations
    public static bool IsValidCoordinate(GeoJsonPosition position);
    public static bool IsValidPolygonRing(GeoJsonPosition[] ring);
    public static bool IsValidLineString(GeoJsonPosition[] coordinates);
    public static bool IsRfc7946Compliant(GeoJsonFeatureCollection collection);
    
    // Geometry validations
    public static bool IsClockwise(GeoJsonPosition[] ring);
    public static bool IsRingClosed(GeoJsonPosition[] ring);
    public static bool AreRingsValid(GeoJsonPosition[][] rings);
}
```

## Usage Examples

### Basic GeoJSON Creation
```csharp
// Create a simple point feature
var point = new GeoJsonPoint(-122.4194, 37.7749); // San Francisco
var feature = new GeoJsonFeature
{
    Id = "sf-point",
    Geometry = point,
    Properties = new Dictionary<string, object>
    {
        ["name"] = "San Francisco",
        ["population"] = 883305,
        ["state"] = "California"
    }
};

// Create feature collection
var collection = new GeoJsonFeatureCollection();
collection.Add(feature);

// Save to file
using var geoJson = new GeoJsonVector();
geoJson.FeatureCollection = collection;
await geoJson.SaveToFileAsync("san_francisco.geojson");
```

### Loading and Processing GeoJSON
```csharp
// Load GeoJSON from file
using var geoJson = new GeoJsonVector();
await geoJson.LoadFromFileAsync("world_cities.geojson");

// Validate RFC 7946 compliance
var validation = geoJson.Validate();
if (!validation.IsValid)
{
    Console.WriteLine($"Validation errors: {string.Join(", ", validation.Errors)}");
}

// Filter features by property
var largeCities = geoJson.FeatureCollection.Filter(f => 
    f.GetProperty<int>("population") > 1_000_000);

Console.WriteLine($"Found {largeCities.Count()} cities with population > 1M");
```

### Complex Geometry Operations
```csharp
// Create a polygon with holes
var exteriorRing = new[]
{
    new GeoJsonPosition(-122.5, 37.7),
    new GeoJsonPosition(-122.3, 37.7),
    new GeoJsonPosition(-122.3, 37.8),
    new GeoJsonPosition(-122.5, 37.8),
    new GeoJsonPosition(-122.5, 37.7) // Close the ring
};

var hole = new[]
{
    new GeoJsonPosition(-122.45, 37.73),
    new GeoJsonPosition(-122.35, 37.73),
    new GeoJsonPosition(-122.35, 37.77),
    new GeoJsonPosition(-122.45, 37.77),
    new GeoJsonPosition(-122.45, 37.73) // Close the hole
};

var polygon = new GeoJsonPolygon
{
    Coordinates = new[] { exteriorRing, hole }
};

// Validate polygon structure
Console.WriteLine($"Polygon area: {polygon.CalculateArea():F2} sq degrees");
Console.WriteLine($"Has holes: {polygon.HasHoles()}");
Console.WriteLine($"Is valid: {polygon.IsValid()}");
```

### Streaming Large Datasets
```csharp
// Stream process large GeoJSON file
using var fileStream = File.OpenRead("large_dataset.geojson");
using var reader = new GeoJsonReader();

int processedCount = 0;
await foreach (var feature in reader.ReadFeaturesAsync(fileStream))
{
    // Process each feature individually
    if (feature.GetProperty<string>("country") == "USA")
    {
        // Process US features
        ProcessUsFeature(feature);
        processedCount++;
    }
    
    if (processedCount % 1000 == 0)
    {
        Console.WriteLine($"Processed {processedCount} features");
    }
}
```

### Coordinate Reference System Transformation
```csharp
using var geoJson = new GeoJsonVector();
await geoJson.LoadFromFileAsync("wgs84_data.geojson");

// Transform from WGS84 to Web Mercator
var transformer = new CoordinateTransformer("EPSG:4326", "EPSG:3857");

foreach (var feature in geoJson.FeatureCollection.Features)
{
    if (feature.Geometry != null)
    {
        feature.Geometry = feature.Geometry.Transform(transformer);
    }
}

// Update CRS metadata
geoJson.CoordinateReferenceSystem = "EPSG:3857";
geoJson.Metadata.CoordinateReferenceSystem = "EPSG:3857";

await geoJson.SaveToFileAsync("web_mercator_data.geojson");
```

### Spatial Queries and Analysis
```csharp
using var geoJson = new GeoJsonVector();
await geoJson.LoadFromFileAsync("world_countries.geojson");

// Define bounding box for Europe
var europeBounds = new GeoJsonBounds
{
    West = -10.0,
    South = 35.0,
    East = 40.0,
    North = 70.0
};

// Get countries within Europe
var europeanCountries = geoJson.FeatureCollection.GetFeaturesWithin(europeBounds);

// Calculate statistics
var totalArea = europeanCountries.Sum(country => 
    country.Geometry?.CalculateArea() ?? 0);

Console.WriteLine($"European countries: {europeanCountries.Count()}");
Console.WriteLine($"Total area: {totalArea:F2} sq degrees");

// Find country containing a specific point
var parisPoint = new GeoJsonPoint(2.3522, 48.8566);
var countryContainingParis = geoJson.FeatureCollection.Features
    .FirstOrDefault(f => f.Geometry?.Contains(parisPoint) == true);

Console.WriteLine($"Paris is in: {countryContainingParis?.GetProperty<string>("name")}");
```

### Advanced Feature Collection Operations
```csharp
// Load multiple datasets and merge
using var cities = new GeoJsonVector();
using var countries = new GeoJsonVector();

await cities.LoadFromFileAsync("world_cities.geojson");
await countries.LoadFromFileAsync("world_countries.geojson");

// Create combined feature collection
var combined = new GeoJsonFeatureCollection();

// Add all cities with additional metadata
foreach (var city in cities.FeatureCollection.Features)
{
    city.SetProperty("layer", "cities");
    city.SetProperty("symbolSize", city.GetProperty<int>("population") / 100000);
    combined.Add(city);
}

// Add country boundaries
foreach (var country in countries.FeatureCollection.Features)
{
    country.SetProperty("layer", "countries");
    country.SetProperty("fillOpacity", 0.3);
    combined.Add(country);
}

// Save combined dataset
using var result = new GeoJsonVector();
result.FeatureCollection = combined;
result.UpdateBoundingBox();

await result.SaveToFileAsync("world_map_layers.geojson", new GeoJsonWriteOptions
{
    PrettyPrint = true,
    IncludeBoundingBoxes = true,
    CoordinatePrecision = 4
});
```

### Real-time Data Processing
```csharp
// Process streaming GeoJSON data
public async Task ProcessRealtimeGeoJsonAsync(Stream dataStream)
{
    using var reader = new GeoJsonReader();
    
    await foreach (var feature in reader.ReadFeaturesAsync(dataStream))
    {
        // Real-time validation
        var validation = GeoJsonValidator.ValidateFeature(feature);
        if (!validation.IsValid)
        {
            LogValidationError(feature.Id, validation.Errors);
            continue;
        }

        // Extract geometry for spatial indexing
        if (feature.Geometry != null)
        {
            var bounds = feature.Geometry.CalculateBounds();
            await spatialIndex.IndexFeatureAsync(feature.Id, bounds);
        }

        // Process properties
        ProcessFeatureProperties(feature);
        
        // Notify subscribers
        await NotifyFeatureProcessedAsync(feature);
    }
}
```

## Technical Specifications

### Supported GeoJSON Objects
- **Point**: Single position coordinate
- **MultiPoint**: Array of position coordinates
- **LineString**: Array of two or more positions (linear ring)
- **MultiLineString**: Array of LineString coordinate arrays
- **Polygon**: Array of linear ring coordinate arrays (exterior + holes)
- **MultiPolygon**: Array of Polygon coordinate arrays
- **GeometryCollection**: Array of heterogeneous geometry objects
- **Feature**: Geometry with properties and optional id
- **FeatureCollection**: Array of Feature objects

### Coordinate Reference Systems
- **Default**: WGS84 (EPSG:4326) longitude/latitude
- **Supported**: Named CRS via EPSG codes
- **Integration**: Wangkanai.Spatial coordinate transformations
- **Validation**: CRS compliance checking

### RFC 7946 Compliance Features
- **Right-hand Rule**: Polygon exterior rings wound clockwise
- **Antimeridian Handling**: Proper handling of ±180° longitude
- **Linear Ring Closure**: Automatic ring closure validation
- **Coordinate Precision**: Configurable decimal precision
- **Bounding Box**: Optional bbox member support

## Performance Characteristics

### Memory Usage
- **Streaming Mode**: Constant memory usage regardless of file size
- **Collection Mode**: ~2-3x file size in memory for full collections
- **Spatial Indexing**: Additional ~20-30% overhead for R-tree index
- **Large Datasets**: Memory-mapped file support for multi-GB files

### Processing Speed
- **JSON Parsing**: System.Text.Json optimized for .NET 9.0
- **Validation**: ~100-500k features/second depending on complexity
- **Coordinate Transformation**: ~1M coordinates/second
- **Spatial Queries**: Sub-millisecond with proper indexing

### File Size Optimization
- **Coordinate Precision**: Configurable decimal places (default: 6)
- **Pretty Printing**: Optional formatting for readability
- **Bounding Boxes**: Optional bbox inclusion
- **Property Optimization**: Efficient property serialization

## Constants and Configuration

### GeoJSON Constants
```csharp
public static class GeoJsonConstants
{
    // Type names
    public const string Point = "Point";
    public const string MultiPoint = "MultiPoint";
    public const string LineString = "LineString";
    public const string MultiLineString = "MultiLineString";
    public const string Polygon = "Polygon";
    public const string MultiPolygon = "MultiPolygon";
    public const string GeometryCollection = "GeometryCollection";
    public const string Feature = "Feature";
    public const string FeatureCollection = "FeatureCollection";

    // Default settings
    public const string DefaultCrs = "EPSG:4326";
    public const int DefaultCoordinatePrecision = 6;
    public const int MinCoordinatePrecision = 0;
    public const int MaxCoordinatePrecision = 15;

    // Validation limits
    public const int MaxCoordinatesPerGeometry = 1_000_000;
    public const int MaxFeaturesPerCollection = 100_000;
    public const double MinValidLongitude = -180.0;
    public const double MaxValidLongitude = 180.0;
    public const double MinValidLatitude = -90.0;
    public const double MaxValidLatitude = 90.0;
}
```

### Processing Options
```csharp
public class GeoJsonReadOptions
{
    public bool ValidateWhileReading { get; set; } = true;
    public bool BuildSpatialIndex { get; set; } = false;
    public int MaxFeaturesToRead { get; set; } = int.MaxValue;
    public string? TargetCrs { get; set; }
    public GeoJsonBounds? FilterBounds { get; set; }
    public Func<GeoJsonFeature, bool>? FeatureFilter { get; set; }
}

public class GeoJsonWriteOptions
{
    public bool PrettyPrint { get; set; } = false;
    public bool IncludeBoundingBoxes { get; set; } = false;
    public int CoordinatePrecision { get; set; } = GeoJsonConstants.DefaultCoordinatePrecision;
    public bool ValidateBeforeWrite { get; set; } = true;
    public string? TargetCrs { get; set; }
    public bool WriteNullGeometries { get; set; } = false;
}
```

## Validation and Error Handling

### Comprehensive Validation
```csharp
// Validate complete GeoJSON structure
var validation = GeoJsonValidator.Validate(featureCollection);
if (!validation.IsValid)
{
    Console.WriteLine($"Validation Summary: {validation.GetSummary()}");
    
    foreach (var error in validation.Errors)
        Console.WriteLine($"Error: {error.Code} - {error.Message} at {error.Location}");
        
    foreach (var warning in validation.Warnings)
        Console.WriteLine($"Warning: {warning.Code} - {warning.Message}");
}

// RFC 7946 specific validation
var rfc7946Compliant = GeoJsonValidator.IsRfc7946Compliant(featureCollection);
Console.WriteLine($"RFC 7946 Compliant: {rfc7946Compliant}");
```

### Common Validation Issues
| Issue | Description | Solution |
|-------|-------------|----------|
| InvalidCoordinates | Coordinates outside valid range | Validate longitude [-180,180], latitude [-90,90] |
| UnclosedRing | Linear ring not properly closed | Ensure first and last coordinates are identical |
| InvalidPolygon | Polygon ring winding order incorrect | Use right-hand rule (clockwise exterior) |
| InvalidGeometry | Geometry structure doesn't match type | Verify coordinate array structure |
| InvalidCrs | Coordinate reference system not supported | Use supported CRS or transform coordinates |
| LargeDataset | Dataset exceeds processing limits | Use streaming mode or filter data |

## Integration with Planet Ecosystem

### Graphics Library Integration
```csharp
// GeoJSON inherits from Vector base class
Vector vector = new GeoJsonVector();
await vector.LoadFromFileAsync("data.geojson");

// Implements IMetadata interface
IMetadata metadata = vector.Metadata;

// Supports graphics library disposal patterns
if (vector.HasLargeMetadata)
{
    await vector.DisposeAsync();
}
```

### Spatial Library Integration
```csharp
using Wangkanai.Spatial;
using Wangkanai.Spatial.Coordinates;

// Native coordinate integration
var geoJsonPoint = new GeoJsonPoint(-122.4194, 37.7749);
var geodetic = geoJsonPoint.Coordinates.ToGeodetic();
var mercator = geoJsonPoint.Coordinates.ToMercator();

// Coordinate transformations
var transformer = new CoordinateTransformer("EPSG:4326", "EPSG:3857");
var transformedGeometry = geometry.Transform(transformer);

// Spatial extent integration
var mapExtent = new MapExtent(geoJson.CalculateBounds());
var tileIndices = mapExtent.GetTileIndices(zoomLevel: 10);
```

### Protocol Integration
```csharp
// WFS service integration
public class GeoJsonWfsService : IWfsService
{
    public async Task<GeoJsonFeatureCollection> GetFeaturesAsync(WfsRequest request)
    {
        using var geoJson = new GeoJsonVector();
        await geoJson.LoadFromFileAsync(request.TypeName + ".geojson");
        
        // Apply spatial filter
        if (request.BoundingBox != null)
        {
            return geoJson.FeatureCollection.FilterByBounds(request.BoundingBox);
        }
        
        return geoJson.FeatureCollection;
    }
}
```

## Best Practices

### Performance Optimization
1. **Use streaming for large files**: Avoid loading entire datasets into memory
2. **Enable spatial indexing**: For frequent spatial queries
3. **Optimize coordinate precision**: Use appropriate decimal places for use case
4. **Validate efficiently**: Enable validation only when necessary

### Data Quality
1. **Always validate input**: Use built-in validation before processing
2. **Handle CRS properly**: Ensure coordinate system consistency
3. **Manage precision**: Balance accuracy with file size
4. **Check geometry validity**: Validate polygon rings and line strings

### Memory Management
1. **Use using statements**: Proper disposal of GeoJSON objects
2. **Stream large datasets**: Process features individually for large files
3. **Monitor memory usage**: Check estimated memory requirements
4. **Clear collections**: Explicitly clear large feature collections

### Integration Guidelines
1. **Follow RFC 7946**: Ensure standard compliance
2. **Use appropriate CRS**: WGS84 for interchange, projected for analysis
3. **Validate foreign members**: Handle custom properties appropriately
4. **Test with real data**: Validate with actual geospatial datasets

## Contributing

### Development Setup
1. Install .NET 9.0 SDK
2. Clone the repository
3. Navigate to `Graphics/Vectors/src/Root/GeoJsons`
4. Run `dotnet build` to build the project

### Dependencies
- **System.Text.Json**: High-performance JSON processing
- **Wangkanai.Graphics.Abstractions**: Core graphics interfaces
- **Wangkanai.Spatial**: Coordinate system support
- **NetTopologySuite**: Advanced spatial operations (optional)

### Running Tests
```bash
dotnet test Graphics/Vectors/tests/Unit/GeoJsons/
```

### Code Standards
- Follow the coding guidelines in CLAUDE.md
- Use PascalCase for public members
- Include comprehensive XML documentation
- Write unit tests for all public methods
- Use async/await for I/O operations

## Implementation Phases

### Phase 1: Core Geometry Objects
- Basic geometry type implementations
- Position and coordinate handling
- Simple validation framework
- Integration with Vector base class

### Phase 2: Feature and Collection Support
- Feature and FeatureCollection classes
- Property handling and metadata
- Basic I/O operations
- Memory-efficient processing

### Phase 3: Advanced Features
- Streaming JSON processing
- Spatial indexing and queries
- Coordinate transformations
- RFC 7946 compliance validation

### Phase 4: Performance and Integration
- Performance optimization
- Planet ecosystem integration
- Cross-format conversion
- Production-ready validation

## References

- [RFC 7946 - The GeoJSON Format](https://tools.ietf.org/html/rfc7946)
- [GeoJSON.org Official Documentation](https://geojson.org/)
- [OGC GeoJSON Standard](https://www.ogc.org/standards/geojson)
- [System.Text.Json Documentation](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)
- [Spatial Data on the Web Best Practices](https://www.w3.org/TR/sdw-bp/)

This GeoJSON implementation provides comprehensive RFC 7946 compliant vector geospatial data handling with advanced features for modern mapping applications in the Planet Graphics ecosystem.