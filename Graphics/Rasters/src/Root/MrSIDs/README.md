# ECW/MrSID Enterprise Geospatial Technical Specification

## Overview

The ECW/MrSID implementation in the Planet Graphics library provides comprehensive support for two industry-standard proprietary wavelet compression formats widely used in the geospatial industry: ECW (Enhanced Compression Wavelet) and MrSID (Multi-resolution Seamless Image Database). These formats are essential for handling large-scale aerial and satellite imagery archives with superior compression efficiency and multi-resolution capabilities.

**Specification Reference**: [GitHub Issue #88](https://github.com/wangkanai/planet/issues/88)
**Namespace**: `Wangkanai.Graphics.Rasters.MrSIDs`
**Supported Formats**: ECW (`.ecw`), MrSID (`.sid`)
**Primary Use Case**: Enterprise geospatial imagery and satellite data processing

## Key Features

### ECW (Enhanced Compression Wavelet)
- **Proprietary Wavelet Compression**: Optimized for geospatial imagery with 10:1 to 100:1 compression ratios
- **Progressive Decompression**: Multi-resolution display with selective region access
- **High Image Quality**: Maintains visual quality at extreme compression ratios
- **Geospatial Integration**: Native support for coordinate systems and projections
- **Enterprise Grade**: Widely used in aerial and satellite imagery archives

### MrSID (Multi-resolution Seamless Image Database)
- **Advanced Compression Algorithm**: Efficient compression for large geospatial datasets
- **Multi-resolution Architecture**: Quick access to overview layers at different scales
- **Selective Decompression**: Region-of-interest (ROI) access without full decompression
- **Seamless Mosaicking**: Support for large image collections as single datasets
- **Industry Standard**: Common in government and commercial satellite repositories

### Professional Geospatial Features
- **Coordinate Reference Systems**: Full CRS support with EPSG integration
- **Georeferencing**: Embedded spatial reference information
- **Metadata Preservation**: Complete metadata support for geospatial workflows
- **Tile Generation**: Optimized for map tile pyramid generation
- **Performance Optimized**: Streaming access for large datasets

## Architecture

### Core Classes

#### `MrSidRaster`
```csharp
public class MrSidRaster : Raster, IMrSidRaster
{
    // Format identification
    public MrSidFormat Format { get; } // ECW or MrSID
    public string FormatVersion { get; }
    public bool IsLicensed { get; }

    // Compression properties
    public float CompressionRatio { get; }
    public MrSidCompressionType CompressionType { get; }
    public int QualityLevel { get; }

    // Multi-resolution support
    public int ResolutionLevels { get; }
    public MrSidResolution[] AvailableResolutions { get; }
    public int CurrentResolutionLevel { get; set; }

    // Geospatial properties
    public string CoordinateReferenceSystem { get; }
    public GeoTransform GeoTransform { get; }
    public GeographicBounds GeographicBounds { get; }

    // Metadata access
    public MrSidMetadata Metadata { get; }

    // Decoding operations
    public Task<byte[]> DecodeAsync(MrSidDecodingOptions? options = null);
    public Task<byte[]> DecodeRegionAsync(Rectangle region, int resolutionLevel = 0);
    public Task<byte[]> DecodeResolutionAsync(int resolutionLevel);

    // Multi-resolution access
    public MrSidResolution GetResolution(int level);
    public Task<byte[]> GetOverviewAsync(int overviewLevel);
    public Task<byte[]> GetThumbnailAsync(int maxSize = 256);

    // Encoding operations (requires licensing)
    public Task<byte[]> EncodeAsync(MrSidEncodingOptions options);
    public bool CanEncode { get; }
}
```

#### `MrSidMetadata`
```csharp
public class MrSidMetadata : RasterMetadata, IMrSidMetadata
{
    // Format-specific metadata
    public MrSidFormat Format { get; set; }
    public string FormatVersion { get; set; }
    public MrSidCompressionInfo CompressionInfo { get; set; }
    public MrSidLicenseInfo LicenseInfo { get; set; }

    // Geospatial metadata
    public string CoordinateReferenceSystem { get; set; }
    public GeoTransform GeoTransform { get; set; }
    public GeographicBounds GeographicBounds { get; set; }
    public string[] GeodeticParameters { get; set; }

    // Multi-resolution metadata
    public int ResolutionLevels { get; set; }
    public MrSidResolution[] Resolutions { get; set; }
    public MrSidOverviewInfo[] Overviews { get; set; }

    // Image characteristics
    public MrSidColorSpace ColorSpace { get; set; }
    public int BitsPerPixel { get; set; }
    public bool HasAlpha { get; set; }
    public MrSidPixelType PixelType { get; set; }

    // Compression metrics
    public float CompressionRatio { get; set; }
    public long UncompressedSize { get; set; }
    public long CompressedSize { get; set; }
    public MrSidQualityMetrics QualityMetrics { get; set; }

    // Acquisition metadata
    public DateTime? AcquisitionDate { get; set; }
    public string? SensorType { get; set; }
    public string? Platform { get; set; }
    public MrSidBandInfo[] BandInformation { get; set; }
}
```

#### `MrSidDecodingOptions`
```csharp
public class MrSidDecodingOptions
{
    // Resolution control
    public int ResolutionLevel { get; set; } = 0;
    public double Scale { get; set; } = 1.0;
    public MrSidResamplingMethod ResamplingMethod { get; set; } = MrSidResamplingMethod.Bilinear;

    // Region of interest
    public Rectangle? RegionOfInterest { get; set; }
    public GeographicBounds? GeographicRegion { get; set; }

    // Output format
    public MrSidPixelType OutputPixelType { get; set; } = MrSidPixelType.Native;
    public MrSidColorSpace OutputColorSpace { get; set; } = MrSidColorSpace.Native;
    public bool PreserveAlpha { get; set; } = true;

    // Performance options
    public int ThreadCount { get; set; } = Environment.ProcessorCount;
    public bool UseMemoryMapping { get; set; } = true;
    public int CacheSize { get; set; } = 64 * 1024 * 1024; // 64MB

    // Quality settings
    public MrSidQualityMode QualityMode { get; set; } = MrSidQualityMode.Balanced;
    public bool EnableProgressiveRendering { get; set; } = true;
}
```

#### `MrSidEncodingOptions`
```csharp
public class MrSidEncodingOptions
{
    // Compression settings
    public MrSidFormat TargetFormat { get; set; } = MrSidFormat.MrSID;
    public float CompressionRatio { get; set; } = 20.0f;
    public int QualityLevel { get; set; } = 85;
    public MrSidCompressionType CompressionType { get; set; } = MrSidCompressionType.Wavelet;

    // Multi-resolution settings
    public int ResolutionLevels { get; set; } = 6;
    public bool GenerateOverviews { get; set; } = true;
    public MrSidOverviewMode OverviewMode { get; set; } = MrSidOverviewMode.Automatic;

    // Geospatial settings
    public string? CoordinateReferenceSystem { get; set; }
    public GeoTransform? GeoTransform { get; set; }
    public bool EmbedGeoreferencing { get; set; } = true;

    // Tiling options
    public int TileSize { get; set; } = 1024;
    public bool EnableTiling { get; set; } = true;
    public MrSidTileOrganization TileOrganization { get; set; } = MrSidTileOrganization.Optimized;

    // Performance options
    public int ThreadCount { get; set; } = Environment.ProcessorCount;
    public bool OptimizeForStreaming { get; set; } = true;

    // Licensing
    public MrSidLicenseInfo LicenseInfo { get; set; }
}
```

#### `MrSidValidator`
```csharp
public static class MrSidValidator
{
    public static MrSidValidationResult Validate(IMrSidRaster raster);
    public static bool IsValidEcwSignature(byte[] data);
    public static bool IsValidMrSidSignature(byte[] data);
    public static MrSidFormat DetectFormat(byte[] data);
    public static bool IsLicenseValid(MrSidLicenseInfo license);
    public static bool CanEncode(MrSidFormat format);
    public static MrSidCapabilities GetCapabilities();
}
```

## Usage Examples

### Basic ECW/MrSID Loading
```csharp
// Load ECW file
using var ecwRaster = new MrSidRaster("/path/to/aerial.ecw");
Console.WriteLine($"Format: {ecwRaster.Format}");
Console.WriteLine($"Compression: {ecwRaster.CompressionRatio:F1}:1");
Console.WriteLine($"Resolution levels: {ecwRaster.ResolutionLevels}");

// Load MrSID file
using var mrsidRaster = new MrSidRaster("/path/to/satellite.sid");
Console.WriteLine($"CRS: {mrsidRaster.CoordinateReferenceSystem}");
Console.WriteLine($"Bounds: {mrsidRaster.GeographicBounds}");
```

### Multi-Resolution Access
```csharp
using var raster = new MrSidRaster("/path/to/large_image.sid");

// Get available resolutions
foreach (var resolution in raster.AvailableResolutions)
{
    Console.WriteLine($"Level {resolution.Level}: {resolution.Width}x{resolution.Height} " +
                      $"(Scale: 1:{resolution.Scale})");
}

// Decode at different resolution levels
var fullRes = await raster.DecodeResolutionAsync(0); // Full resolution
var halfRes = await raster.DecodeResolutionAsync(1); // Half resolution
var quarterRes = await raster.DecodeResolutionAsync(2); // Quarter resolution

// Get thumbnail
var thumbnail = await raster.GetThumbnailAsync(maxSize: 512);
```

### Region of Interest (ROI) Processing
```csharp
using var raster = new MrSidRaster("/path/to/large_satellite.sid");

// Define geographic region of interest
var roiGeographic = new GeographicBounds
{
    MinLatitude = 40.0,
    MaxLatitude = 41.0,
    MinLongitude = -74.0,
    MaxLongitude = -73.0
};

// Decode specific geographic region
var roiOptions = new MrSidDecodingOptions
{
    GeographicRegion = roiGeographic,
    ResolutionLevel = 1, // Half resolution for performance
    ResamplingMethod = MrSidResamplingMethod.Bicubic
};

var roiData = await raster.DecodeAsync(roiOptions);
```

### Geospatial Integration
```csharp
using var raster = new MrSidRaster("/path/to/georeferenced.ecw");

// Access geospatial metadata
Console.WriteLine($"CRS: {raster.CoordinateReferenceSystem}");
Console.WriteLine($"Pixel size: {raster.GeoTransform.PixelSizeX}, {raster.GeoTransform.PixelSizeY}");
Console.WriteLine($"Origin: {raster.GeoTransform.OriginX}, {raster.GeoTransform.OriginY}");

// Transform pixel coordinates to geographic
var pixelCoord = new Point(1000, 1000);
var geoCoord = raster.GeoTransform.TransformToGeographic(pixelCoord);
Console.WriteLine($"Geographic: {geoCoord.Latitude}, {geoCoord.Longitude}");

// Get geographic bounds
var bounds = raster.GeographicBounds;
Console.WriteLine($"Bounds: {bounds.MinLongitude}, {bounds.MinLatitude} to " +
                  $"{bounds.MaxLongitude}, {bounds.MaxLatitude}");
```

### Professional Workflow Integration
```csharp
// Large satellite image processing workflow
using var satelliteImage = new MrSidRaster("/path/to/satellite_scene.sid");

// Get image characteristics
var metadata = satelliteImage.Metadata;
Console.WriteLine($"Sensor: {metadata.SensorType}");
Console.WriteLine($"Platform: {metadata.Platform}");
Console.WriteLine($"Acquisition: {metadata.AcquisitionDate}");

// Process each spectral band
foreach (var band in metadata.BandInformation)
{
    Console.WriteLine($"Band {band.Number}: {band.Description} " +
                      $"({band.MinValue} - {band.MaxValue})");
}

// Generate map tiles at multiple zoom levels
for (int zoomLevel = 0; zoomLevel < 18; zoomLevel++)
{
    var tileOptions = new MrSidDecodingOptions
    {
        ResolutionLevel = Math.Min(zoomLevel / 3, satelliteImage.ResolutionLevels - 1),
        ResamplingMethod = MrSidResamplingMethod.Bilinear
    };

    // Generate tiles for this zoom level
    await GenerateTilesForZoomLevel(satelliteImage, zoomLevel, tileOptions);
}
```

### Encoding Support (Requires Licensing)
```csharp
// Note: Encoding requires valid SDK license
if (MrSidValidator.CanEncode(MrSidFormat.ECW))
{
    using var sourceRaster = new TiffRaster("/path/to/source.tif");

    var encodingOptions = new MrSidEncodingOptions
    {
        TargetFormat = MrSidFormat.ECW,
        CompressionRatio = 25.0f,
        QualityLevel = 90,
        ResolutionLevels = 6,
        CoordinateReferenceSystem = "EPSG:4326",
        LicenseInfo = new MrSidLicenseInfo
        {
            LicenseKey = "your-license-key",
            LicenseType = MrSidLicenseType.Commercial
        }
    };

    var encodedData = await sourceRaster.EncodeToMrSidAsync(encodingOptions);
    await File.WriteAllBytesAsync("/path/to/output.ecw", encodedData);
}
```

## Technical Specifications

### Supported Formats
- **ECW (Enhanced Compression Wavelet)**
  - File Extensions: `.ecw`
  - Compression: Wavelet-based with 10:1 to 100:1 ratios
  - Multi-resolution: Progressive decompression
  - Geospatial: Native CRS and georeferencing support
  - Usage: Aerial and satellite imagery archives

- **MrSID (Multi-resolution Seamless Image Database)**
  - File Extensions: `.sid`
  - Compression: Proprietary wavelet algorithm
  - Multi-resolution: Built-in overview layers
  - Features: Selective decompression, seamless mosaicking
  - Usage: Government and commercial satellite repositories

### Compression Characteristics
- **ECW**: 10:1 to 100:1 compression ratios with excellent quality preservation
- **MrSID**: Efficient compression optimized for large geospatial datasets
- **Progressive Access**: Multi-resolution pyramid structure for quick overviews
- **Region Access**: Selective decompression of specific areas without full decode

### Multi-Resolution Architecture
- **Pyramid Structure**: Multiple resolution levels with 2:1 scale factors
- **Overview Layers**: Pre-computed reduced resolution images
- **Streaming Access**: Efficient access to any resolution level
- **Memory Efficient**: Only loads required resolution data

## Performance Characteristics

### Compression Efficiency
- **ECW**: Excellent compression for aerial photography (20:1 to 50:1 typical)
- **MrSID**: Optimized for satellite imagery (15:1 to 40:1 typical)
- **Quality Preservation**: Maintains visual quality at high compression ratios
- **File Size**: Significant reduction compared to uncompressed formats

### Access Performance
- **Multi-resolution**: Instant access to overview levels
- **Region Access**: Fast extraction of specific areas
- **Memory Usage**: Minimal memory footprint with streaming
- **Caching**: Intelligent caching for repeated access patterns

### Scalability
- **Large Images**: Efficient handling of multi-gigabyte images
- **Concurrent Access**: Thread-safe multi-user access
- **Network Streaming**: Optimized for remote access scenarios
- **Tile Generation**: Fast tile pyramid generation

## Integration with Planet Ecosystem

### Graphics Library Integration
```csharp
// ECW/MrSID inherits from Raster base class
Raster raster = new MrSidRaster("/path/to/image.ecw");

// Implements IMetadata interface
IMetadata metadata = raster.Metadata;

// Supports graphics library disposal patterns
if (raster.HasLargeMetadata)
{
    await raster.DisposeAsync();
}
```

### Spatial Library Integration
```csharp
using Wangkanai.Spatial;
using Wangkanai.Spatial.Coordinates;

// Native geospatial integration
var raster = new MrSidRaster("/path/to/georeferenced.sid");
var spatialRef = new SpatialReference(raster.CoordinateReferenceSystem);

// Coordinate transformations
var transformer = new CoordinateTransformer(spatialRef, SpatialReference.WGS84);
var geoCoord = transformer.Transform(pixelCoord);
```

### Tile Generation Integration
```csharp
// Optimized tile generation for web maps
var tileGenerator = new TileGenerator(raster);
var tiles = await tileGenerator.GenerateTilesAsync(
    zoomLevel: 12,
    tileSize: 256,
    format: TileFormat.PNG
);
```

## Licensing and SDK Requirements

### ECW Format
- **Read Support**: Free for non-commercial use
- **Write Support**: Requires commercial license from Hexagon Geospatial
- **SDK**: ERDAS ECW/JP2 SDK integration
- **Licensing Model**: Per-application or per-server licensing

### MrSID Format
- **Read Support**: Free with LizardTech SDK
- **Write Support**: Requires commercial license from Extensis
- **SDK**: GeoExpress SDK integration
- **Licensing Model**: Development and deployment licenses

### Implementation Approach
```csharp
// License validation
public static class MrSidLicenseManager
{
    public static bool ValidateLicense(MrSidLicenseInfo license)
    {
        // Validate license with SDK
        return SdkLicenseValidator.Validate(license);
    }

    public static MrSidCapabilities GetLicensedCapabilities()
    {
        // Return capabilities based on license
        return new MrSidCapabilities
        {
            CanReadEcw = true,
            CanReadMrSid = true,
            CanWriteEcw = HasCommercialLicense(),
            CanWriteMrSid = HasCommercialLicense()
        };
    }
}
```

## Best Practices

### Performance Optimization
1. **Use appropriate resolution levels**: Match resolution to display scale
2. **Implement caching**: Cache frequently accessed tiles and regions
3. **Memory management**: Use streaming for large images
4. **Thread management**: Limit concurrent decode operations

### Geospatial Workflows
1. **CRS validation**: Always validate coordinate reference systems
2. **Bounds checking**: Validate geographic regions before processing
3. **Metadata preservation**: Maintain geospatial metadata through processing
4. **Projection handling**: Use appropriate projections for analysis

### Enterprise Integration
1. **License management**: Implement proper license validation
2. **Error handling**: Robust error handling for licensing issues
3. **Performance monitoring**: Monitor decode performance and memory usage
4. **Security**: Secure license key storage and validation

## Contributing

### Development Setup
1. Install .NET 9.0 SDK
2. Obtain appropriate SDK licenses (ECW/MrSID)
3. Configure SDK integration
4. Clone the repository
5. Navigate to `Graphics/Rasters/src/Root/MrSIDs`
6. Run `dotnet build` to build the project

### Dependencies
- **ECW SDK**: ERDAS ECW/JP2 SDK
- **MrSID SDK**: GeoExpress SDK
- **GDAL**: Optional for fallback support
- **Wangkanai.Graphics.Abstractions**: Core graphics interfaces
- **Wangkanai.Spatial**: Geospatial coordinate support

### Running Tests
```bash
dotnet test Graphics/Rasters/tests/Unit/MrSIDs/
```

### Code Standards
- Follow the coding guidelines in CLAUDE.md
- Use PascalCase for public members
- Include comprehensive XML documentation
- Write unit tests for all public methods
- Use async/await for I/O operations

## Implementation Phases

### Phase 1: Core Reading Support
- Basic ECW/MrSID file reading
- Format detection and validation
- Metadata extraction
- Integration with Graphics.Rasters interfaces

### Phase 2: Multi-Resolution Access
- Resolution level enumeration
- Selective resolution decoding
- Overview generation
- Performance optimization

### Phase 3: Advanced Features
- Region of interest processing
- Geospatial coordinate integration
- Tile generation optimization
- Streaming access patterns

### Phase 4: Enterprise Features
- Encoding support (with licensing)
- Advanced compression options
- Professional workflow integration
- Performance monitoring and optimization

## References

- [ECW Technical Specifications](https://www.hexagongeospatial.com/products/producer-suite/erdas-imagine)
- [MrSID Technical Specifications](https://www.extensis.com/support/mrsid-sdk)
- [GDAL ECW Driver](https://gdal.org/drivers/raster/ecw.html)
- [GDAL MrSID Driver](https://gdal.org/drivers/raster/mrsid.html)
- [Hexagon Geospatial SDK Documentation](https://hexagongeospatial.com/products/developer-apis)
- [Extensis GeoExpress SDK](https://www.extensis.com/support/mrsid-sdk)

This ECW/MrSID implementation provides enterprise-grade support for industry-standard geospatial imagery formats, enabling efficient processing of large-scale aerial and satellite imagery in the Planet Graphics ecosystem.
