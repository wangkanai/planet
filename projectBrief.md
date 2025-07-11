# Wangkanai Planet - Project Brief

## 🎯 Project Overview

**Wangkanai Planet** is a modern, high-performance geospatial data processing and tile serving platform built on .NET 9.0. It provides a comprehensive solution for converting raster imagery (GeoTIFF, JPEG, PNG) and vector data into web-optimized map tiles, serving them through standard protocols like WMS, WMTS, and XYZ tiles.

**Repository:** https://github.com/wangkanai/planet  
**Solution Structure:** Planet.slnx (.NET 9.0 solution format)  
**Target Framework:** net9.0  
**IDE Optimization:** JetBrains Rider with MCP integration

## 🚀 Main Goal & Purpose

The primary goal of Planet is to democratize geospatial data processing by providing:

- **Efficient tile processing** from various geospatial formats
- **Standards-compliant map serving** through web protocols
- **Cross-platform deployment** supporting Windows, Linux, and macOS
- **Production-ready performance** for enterprise-scale deployments

## 🏗️ Solution Architecture & Components

### Core Processing Libraries

#### **Wangkanai.Spatial** - Geospatial Data Handling
- **`Spatial/src/Root/`** - Core coordinate systems (Geodetic, Mercator), map extent calculations
- **`Spatial/src/MbTiles/`** - SQLite-based MBTiles format support
- **`Spatial/src/GeoPackages/`** - OGC GeoPackage format containers
- **`Spatial/src/GeoTiffs/`** - GeoTIFF format with Graphics.Rasters integration
- **`Spatial/src/ShapeFiles/`** - ESRI Shapefile vector data support
- **`Spatial/src/MtPkgs/`** - Map tile package format support

#### **Wangkanai.Graphics** - Image Processing
- **`Graphics/Abstractions/src/`** - Core image processing interfaces
- **`Graphics/Rasters/src/Root/`** - Multi-format raster processing (TIFF, PNG, JPEG, WebP, AVIF, HEIF)
- **`Graphics/Vectors/src/Root/`** - Vector graphics processing and manipulation
- **`Graphics/Rasters/src/Root/Graphics.Rasters.Benchmarks/`** - Performance benchmarking with BenchmarkDotNet

### Application Components

#### **Wangkanai.Planet.Portal** - Web Application
- **`Portal/src/Server/`** - ASP.NET Core Blazor Server host
- **`Portal/src/Client/`** - Blazor WebAssembly client components
- **`Portal/src/Application/`** - Business logic and Identity configuration
- **`Portal/src/Domain/`** - Domain entities with custom Identity models
- **`Portal/src/Infrastructure/`** - External service integrations
- **`Portal/src/Persistence/`** - Entity Framework Core with SQLite

#### **Wangkanai.Planet.Engine** - Processing Engine
- **`Engine/src/Console/`** - Command-line tile processing interface
- **`Engine/src/Domain/`** - Core processing business logic

#### **Wangkanai.Planet.Protocols** - Service Protocols
- **`Protocols/src/Root/`** - WMS/WMTS protocol implementations
- **`Protocols/WMS/`** - Web Map Service protocol
- **`Protocols/WMTS/`** - Web Map Tile Service protocol

#### **Wangkanai.Planet.Providers** - External Integrations
- **`Providers/src/Root/`** - Provider abstractions
- **`Providers/Bing/`** - Microsoft Bing Maps integration
- **`Providers/Google/`** - Google Maps integration

#### **Wangkanai.Planet.Extensions** - Utilities
- **`Extensions/Datastore/src/`** - Advanced data storage operations

## 🎯 Target Audience & Use Cases

### Primary Users
- **GIS Professionals** - Cartographers, spatial analysts, GIS developers
- **Web Developers** - Building location-based applications with custom tile servers
- **Enterprise Organizations** - Deploying on-premises geospatial infrastructure
- **Government Agencies** - Managing spatial data with security requirements

### Development Use Cases
- **Custom map tile servers** for web applications
- **Offline map solutions** for mobile applications
- **Geospatial data preprocessing** pipelines
- **Enterprise spatial data infrastructure** deployment

## 🏛️ Technical Architecture

### Modern .NET Architecture Patterns
- **Clean Architecture** - Domain, Application, Infrastructure, Persistence layers
- **Dependency Injection** - Built-in .NET DI container throughout
- **Async/await patterns** - Non-blocking I/O operations
- **Repository pattern** - Data access abstraction
- **CQRS patterns** - Command Query Responsibility Segregation where applicable

### Technology Stack
- **.NET 9.0** - Latest framework with nullable reference types enabled
- **ASP.NET Core** - Web framework for portal and API endpoints
- **Blazor Server + WebAssembly** - Hybrid hosting model
- **Entity Framework Core** - Data access with SQLite and PostgreSQL support
- **SQLite** - Default database for tiles and metadata
- **xUnit v3** - Testing framework with testing platform support
- **Docker** - Containerization for deployment

### Performance Architecture
- **Memory-efficient processing** - Large dataset handling with minimal memory footprint
- **GPU acceleration** - ILGPU integration for parallel processing
- **Async disposal patterns** - Proper resource management
- **BenchmarkDotNet** - Performance validation and regression testing

## 🔧 Development Environment & Tools

### JetBrains Rider Integration
- **Solution Format** - .slnx (new Visual Studio solution format)
- **Run Configurations** - Pre-configured for Portal, Engine, and test projects
- **Debugging** - Full debugging support across all components
- **Testing** - Integrated xUnit v3 test runner
- **Code Analysis** - SonarQube integration for code quality

### Build & Development Commands
```bash
# Core build commands
dotnet build -c Release -tl                    # Build entire solution
dotnet test                                    # Run all tests
./build.ps1                                   # Full build script

# Portal development
dotnet run --project Portal/src/Server        # Run web portal
npm run deploy                                # Frontend asset build

# Engine development  
dotnet run --project Engine/src/Console       # Run tile processor
./Engine/src/Console/build.ps1               # Build as 'tiler' executable

# Database management
./Portal/db.ps1 -add "MigrationName"         # Add EF migration
./Portal/db.ps1 -update                      # Update database
```

### Testing Strategy
- **Unit Tests** - Component-level testing (>80% coverage target)
- **Integration Tests** - End-to-end workflow validation
- **Performance Tests** - BenchmarkDotNet for performance validation
- **Test Naming** - No '*Tests' suffix in namespaces (per project guidelines)

## 📊 Development Phases & Milestones

### Phase 1: Core Foundation (3-4 months) - **CURRENT**
- ✅ **Spatial.Root** - Coordinate systems and tile calculations
- ✅ **Graphics.Rasters** - Basic TIFF processing capabilities
- 🔄 **Engine.Console** - GeoTIFF to MBTiles conversion
- 🔄 **Portal.Server** - Basic tile viewing interface
- 🔄 **MBTiles support** - SQLite tile storage implementation

### Phase 2: Format & Protocol Expansion (2-3 months)
- 📋 **Extended format support** - GeoPackage, Shapefile processing
- 📋 **WMS/WMTS protocols** - Standard-compliant implementations
- 📋 **Administrative interface** - Comprehensive tile management
- 📋 **Provider integrations** - Bing/Google Maps connectivity

### Phase 3: Performance & Production (2-3 months)
- 📋 **GPU acceleration** - ILGPU integration for processing
- 📋 **Production deployment** - Container orchestration
- 📋 **Monitoring & observability** - Comprehensive logging and metrics
- 📋 **API documentation** - Complete OpenAPI specifications

## 🎯 Success Metrics & Quality Gates

### Technical Metrics
- **Processing Performance** - Handle multi-GB GeoTIFF files efficiently
- **Protocol Compliance** - Pass OGC WMS/WMTS test suites
- **Code Quality** - Maintain >80% test coverage, SonarQube quality gates
- **Memory Efficiency** - Process large datasets with controlled memory usage

### Development Metrics
- **Build Success** - All CI/CD pipelines passing
- **Test Coverage** - Comprehensive unit and integration test suites
- **Documentation** - Complete API documentation and developer guides
- **Cross-platform** - Windows, Linux, macOS deployment verification

## 🌟 Unique Value Proposition

**Wangkanai Planet** differentiates itself through:

1. **Modern .NET 9.0 Stack** - Leveraging latest framework features and performance improvements
2. **Comprehensive Format Support** - Single platform handling multiple geospatial formats
3. **Production-Ready Performance** - Enterprise-scale processing with GPU acceleration
4. **Standards Compliance** - Full OGC protocol support (WMS, WMTS, WCS)
5. **Developer-Friendly** - Clean architecture with comprehensive testing
6. **Cross-Platform Deployment** - Docker-based deployment across platforms

## 🔮 Technical Roadmap

### Immediate Priorities (Next 3 months)
- Complete MVP tile processing pipeline
- Implement basic WMS protocol support
- Establish CI/CD pipeline with automated testing
- Create comprehensive developer documentation

### Advanced Features (6-12 months)
- **Real-time processing** - Stream processing capabilities
- **Cloud-native deployment** - Kubernetes orchestration
- **AI-powered optimization** - Intelligent tile generation
- **Plugin architecture** - Extensible processing workflows

## 🛠️ Development Context for AI Assistants

### Key Patterns to Follow
- **Coding Guidelines** - PascalCase public members, camelCase private members
- **Async Patterns** - Use async/await for I/O operations
- **Dependency Injection** - Constructor injection throughout
- **Testing** - xUnit v3 with Fact/Theory attributes
- **Error Handling** - Proper exception handling with logging

### Project Structure Navigation
- **Core Libraries** - `Spatial/`, `Graphics/`, `Protocols/`
- **Applications** - `Portal/`, `Engine/`
- **Infrastructure** - `Extensions/`, `Providers/`
- **Documentation** - `docs/`, `README.md files`
- **Configuration** - `Directory.Build.props`, `Directory.Packages.props`

### Common Development Tasks
- **Adding new formats** - Extend `Spatial/` libraries
- **Performance optimization** - Use `Graphics.Rasters.Benchmarks`
- **Protocol implementation** - Extend `Protocols/` components
- **UI development** - Work in `Portal/src/Client/` and `Portal/src/Server/`
- **Testing** - Create corresponding test projects with proper naming

---

*This project brief is optimized for JetBrains Rider development and provides comprehensive context for AI-assisted development workflows. For detailed technical specifications, refer to the documentation in `/docs/` and component-specific README files.*