## Wangkanai Planet Protocols

**Namespace:** `Wangkanai.Planet.Protocols`

A comprehensive collection of map service protocol implementations for serving map tiles through various standardized services. Provides support for industry-standard protocols and custom implementations for efficient map tile delivery.

## Features

- **Multiple Protocols**: Support for various map service protocols
- **Standards Compliance**: Implementation of OGC and industry standards
- **Extensible Architecture**: Easy addition of new protocol implementations
- **Performance Optimized**: Efficient tile serving and caching
- **Cross-Platform**: Works across different mapping platforms and clients

## Supported Protocols

### Web Map Service (WMS)
- **WMS Implementation**: Complete OGC WMS specification support
- **Version Support**: Multiple WMS version compatibility
- **GetMap Operations**: Dynamic map generation and serving
- **Layer Management**: Multi-layer map composition
- **Styling Support**: SLD (Styled Layer Descriptor) integration

### Web Map Tile Service (WMTS)
- **WMTS Standard**: OGC WMTS specification implementation
- **Tile Matrix Sets**: Predefined tile grids and zoom levels
- **RESTful Interface**: HTTP-based tile access
- **Metadata Support**: Service metadata and capabilities

### Tile Map Service (TMS)
- **TMS Protocol**: Open-source tile serving standard
- **Directory Structure**: File-based tile organization
- **Zoom Level Support**: Multi-resolution tile serving
- **Tile Indexing**: Efficient tile addressing scheme

### Web Map Service Cache (WMSC)
- **Cached WMS**: Pre-rendered tile caching for WMS
- **Performance Enhancement**: Faster map rendering through caching
- **OGC Compliance**: Standards-based caching implementation

### XYZ Tiles
- **Simple Protocol**: Straightforward tile access using XYZ coordinates
- **Web Mapping**: Common protocol for web mapping applications
- **TileJSON Support**: Tile metadata in JSON format
- **URL Templates**: Parameterized tile URL generation

### Static Raster Tiles
- **File-Based Serving**: Direct file system tile access
- **HTTP Serving**: Simple HTTP-based tile delivery
- **Directory Structure**: Organized tile directory layouts

## Core Components

### Protocol Abstractions
- **Base Protocols**: Common interfaces and base classes
- **Service Endpoints**: RESTful API endpoint definitions
- **Request Handling**: HTTP request processing and routing
- **Response Formatting**: Standardized response formats

### WMS Implementation
- **WmsVersions**: Version management and compatibility
- **GetCapabilities**: Service capability advertisement
- **GetMap**: Dynamic map image generation
- **GetFeatureInfo**: Feature information retrieval

## Usage

```csharp
using Wangkanai.Planet.Protocols;
using Wangkanai.Planet.Protocols.Wms;

// Configure WMS service
var wmsService = new WmsService();
wmsService.Configure(options => {
    options.SupportedVersions = WmsVersions.All;
    options.DefaultFormat = "image/png";
});

// Handle WMS GetMap request
var mapRequest = new GetMapRequest {
    Layers = "layer1,layer2",
    Bbox = "minx,miny,maxx,maxy",
    Width = 256,
    Height = 256,
    Srs = "EPSG:4326"
};

var mapResponse = await wmsService.GetMapAsync(mapRequest);
```

## Protocol Configuration

Each protocol supports configuration for:
- **Service Metadata**: Title, description, and contact information
- **Supported Operations**: Available service operations and capabilities
- **Output Formats**: Supported image and data formats
- **Coordinate Systems**: Spatial reference system support
- **Access Control**: Authentication and authorization
- **Caching**: Performance optimization settings

## Standards Compliance

### OGC Standards
- **WMS 1.1.1/1.3.0**: Web Map Service specifications
- **WMTS 1.0.0**: Web Map Tile Service standard
- **SLD**: Styled Layer Descriptor for map styling

### Industry Standards
- **TMS**: Tile Map Service specification
- **TileJSON**: Tile metadata JSON schema
- **HTTP/REST**: RESTful web service patterns

## Integration

The Protocols library integrates with:
- **Wangkanai.Spatial** - Coordinate systems and tile calculations
- **Wangkanai.Planet.Providers** - Map service providers
- **ASP.NET Core** - Web service hosting and routing

## Testing

Comprehensive testing includes:
- Protocol compliance testing
- Service endpoint testing
- Request/response validation
- Performance benchmarking
- Interoperability testing

## Dependencies

- **.NET 9.0** - Target framework
- **ASP.NET Core** - Web service framework
- **System.Xml** - XML processing for WMS
- **System.Text.Json** - JSON serialization
- **Wangkanai.Spatial** - Spatial operations
