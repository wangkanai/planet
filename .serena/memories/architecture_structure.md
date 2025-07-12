# Architecture & Solution Structure

## Modular Architecture
Planet follows a **modular architecture** with clear separation of concerns across multiple libraries:

## Core Components

### **Portal** - Web Application
- **Blazor Server/WASM hybrid** web application
- **Clean Architecture** patterns (Domain, Application, Infrastructure, Persistence)
- **ASP.NET Core Identity** for authentication
- **SQLite database** with Entity Framework Core
- **Custom entities**: `PlanetUser`, `PlanetRole`
- **Projects**:
  - `Portal/src/Server` - Main Blazor Server application
  - `Portal/src/Client` - WebAssembly client components
  - `Portal/src/Application` - Business logic and Identity
  - `Portal/src/Domain` - Domain entities
  - `Portal/src/Infrastructure` - External integrations
  - `Portal/src/Persistence` - Entity Framework data access

### **Engine** - Console Application
- **Map tile processing** and rendering
- **Domain-driven design**
- **Projects**:
  - `Engine/src/Console` - CLI operations
  - `Engine/src/Domain` - Core business logic

### **Graphics** - Image Processing Library
- **Namespace**: `Wangkanai.Graphics`
- **Multi-format support**: TIFF, PNG, JPEG, WebP, AVIF, HEIF
- **Performance optimizations** and benchmarking
- **Projects**:
  - `Graphics/Abstractions/src` - Core interfaces and contracts
  - `Graphics/Rasters/src/Root` - Raster image processing
  - `Graphics/Vectors/src/Root` - Vector graphics processing
  - `Graphics/Rasters/benchmark` - Performance benchmarks

### **Spatial** - Geospatial Data Library
- **Namespace**: `Wangkanai.Spatial`
- **Coordinate systems**: Geodetic, Mercator
- **Format support**: MBTiles, GeoPackages, GeoTIFF, Shapefiles
- **Projects**:
  - `Spatial/src/Root` - Core coordinate systems and calculations
  - `Spatial/src/MbTiles` - MBTiles format (SQLite-based)
  - `Spatial/src/GeoPackages` - GeoPackage format
  - `Spatial/src/GeoTiffs` - GeoTIFF format with Graphics integration
  - `Spatial/src/ShapeFiles` - Shapefile format
  - `Spatial/src/MtPkgs` - Map tile package format

### **Providers** - External Service Integrations
- **Map service providers**: Bing Maps, Google Maps
- **Projects**:
  - `Providers/src/Root` - Provider implementations

### **Protocols** - Map Service Protocol Support
- **Protocol implementations**: WMS, WMTS, XYZ Tiles
- **Projects**:
  - `Protocols/src/Root` - Protocol abstractions and implementations

### **Extensions** - Utilities and Extensions
- **Ecosystem utilities** and extension methods
- **Projects**:
  - `Extensions/Datastore/src` - Data storage utilities

## Build Configuration
- **Central package management**: `Directory.Packages.props`
- **Common properties**: `Directory.Build.props`
- **Solution format**: `.slnx` (new Visual Studio format)
- **Testing strategy**: Comprehensive unit tests with BenchmarkDotNet

## Database Design
- **Portal**: `PlanetDbContext` with SQLite
- **Migrations**: Located in `Portal/src/Persistence/Migrations`
- **Identity system**: Custom user and role entities