## Wangkanai Planet Providers

**Namespace:** `Wangkanai.Planet.Providers`

External map service integrations providing access to various map tile providers and data sources. Designed with a modular and extensible architecture for easy integration of multiple map service providers.

## Features

- **Multi-Provider Support**: Integrated support for major map service providers
- **Extensible Architecture**: Easy addition of new map service providers
- **Unified Interface**: Common interface for all map service providers
- **Tile Extensions**: Utility extensions for tile operations and management
- **Performance Optimized**: Efficient tile fetching and caching

## Supported Providers

### Bing Maps Provider
- **BingProvider**: Complete integration with Bing Maps services
- **Tile Access**: Bing Maps tile retrieval and management
- **API Integration**: Bing Maps API connectivity

### Google Maps Provider  
- **GoogleProvider**: Full Google Maps service integration
- **Tile Services**: Google Maps tile access and management
- **API Support**: Google Maps API connectivity

## Core Components

### Provider Interface
- **`IRemoteProvider`**: Common interface for all remote map providers
- **`RemoteProviders`**: Provider management and registry
- **Standardized Methods**: Consistent API across all providers

### Tile Extensions
- **`TileExtensions`**: Utility methods for tile operations
- **Coordinate Conversion**: Tile coordinate transformations
- **URL Generation**: Provider-specific tile URL generation
- **Caching Support**: Tile caching and optimization

## Usage

```csharp
using Wangkanai.Planet.Providers;
using Wangkanai.Planet.Providers.Extensions;

// Use Bing Maps provider
var bingProvider = new BingProvider();
var tiles = await bingProvider.GetTilesAsync(zoom, x, y);

// Use Google Maps provider
var googleProvider = new GoogleProvider();
var mapData = await googleProvider.GetMapDataAsync(coordinates);

// Use tile extensions
var tileUrl = TileExtensions.GenerateTileUrl(provider, x, y, zoom);
```

## Provider Configuration

Each provider supports configuration for:
- **API Keys**: Authentication credentials for service access
- **Service Endpoints**: Custom service URLs and endpoints
- **Rate Limiting**: Request throttling and quota management
- **Caching**: Local tile caching configuration
- **Format Preferences**: Preferred tile formats and quality settings

## Integration

The Providers library integrates with:
- **Wangkanai.Spatial** - Coordinate systems and tile calculations
- **Wangkanai.Planet.Protocols** - Map service protocols
- **HTTP Clients** - Network communication for tile retrieval

## Testing

Comprehensive unit tests covering:
- Provider functionality testing
- Tile extension methods
- API integration testing
- Error handling and resilience
- Performance benchmarking

## Dependencies

- **.NET 9.0** - Target framework
- **System.Net.Http** - HTTP client operations
- **System.Text.Json** - JSON serialization
- **Wangkanai.Spatial** - Spatial operations and tile calculations
