# 🌍 Wangkanai Planet Documentation

> **A modern, high-performance geospatial data processing and tile serving platform built on .NET 9.0**

Welcome to the comprehensive documentation for the Wangkanai Planet project. This documentation provides everything you need to understand, contribute to, and deploy the Planet geospatial platform.

## 📋 Table of Contents

- [🚀 Getting Started](#-getting-started)
- [📖 Project Documentation](#-project-documentation)
- [🏗️ Architecture & Components](#️-architecture--components)
- [🔧 Development Resources](#-development-resources)
- [📚 API Documentation](#-api-documentation)
- [🧪 Testing & Quality](#-testing--quality)
- [🚢 Deployment & Operations](#-deployment--operations)
- [🤝 Contributing](#-contributing)

---

## 🚀 Getting Started

### Quick Start Guide
- **[Main README](../README.md)** - Project overview and quick setup
- **[Development Setup](technical-implementation-guide.md#-development-workflow)** - Development environment setup
- **[First Run Tutorial](technical-implementation-guide.md#-immediate-technical-priorities)** - Your first tile generation

### Key Concepts
- **Geospatial Processing** - Converting GeoTIFF files to web-optimized tiles
- **Tile Serving** - Delivering map tiles via standard protocols (WMS, WMTS)
- **Multi-Format Support** - Handle various spatial data formats (GeoTIFF, MBTiles, GeoPackage)

---

## 📖 Project Documentation

### Strategic Planning
- **[Development Roadmap](development-roadmap.md)** - Complete project timeline and milestones
- **[Scope Clarity & Recommendations](scope-clarity-recommendations.md)** - Strategic focus and prioritization
- **[Technical Implementation Guide](technical-implementation-guide.md)** - Detailed technical specifications

### Project Vision
- **Phase 1**: Core foundation with MVP functionality
- **Phase 2**: Format & protocol expansion
- **Phase 3**: Performance optimization & production readiness

---

## 🏗️ Architecture & Components

### Core Libraries

#### 🗺️ **Spatial Processing**
- **[Spatial Core](../Spatial/README.md)** - Coordinate systems and spatial operations
  - **[GeoTIFF Support](../Spatial/src/GeoTiffs/README.md)** - GeoTIFF file processing
  - **[MBTiles Support](../Spatial/src/MbTiles/README.md)** - SQLite-based tile storage
  - **[GeoPackage Support](../Spatial/src/GeoPackages/README.md)** - OGC GeoPackage format
  - **[Shapefile Support](../Spatial/src/ShapeFiles/README.md)** - ESRI Shapefile format
  - **[MapTiler Packages](../Spatial/src/MtPkgs/README.md)** - MapTiler package support

#### 🎨 **Graphics Processing**
- **[Graphics Overview](../Graphics/README.md)** - Image processing and manipulation
  - **[Abstractions](../Graphics/Abstractions/README.md)** - Core graphics interfaces
  - **[Raster Processing](../Graphics/Rasters/README.md)** - Raster image operations
    - **[TIFF Processing](../Graphics/Rasters/src/Root/Tiffs/README.md)** - TIFF format support
    - **[JPEG Processing](../Graphics/Rasters/src/Root/Jpegs/README.md)** - JPEG format support
  - **[Vector Processing](../Graphics/Vectors/README.md)** - Vector graphics operations

#### ⚙️ **Processing Engine**
- **[Engine Overview](../Engine/README.md)** - Core processing engine
  - **[Console Application](../Engine/src/Console/)** - Command-line interface
  - **[Domain Models](../Engine/src/Domain/)** - Business logic and models

#### 🌐 **Web Portal**
- **[Portal Overview](../Portal/README.md)** - Web-based management interface
  - **[Server Application](../Portal/src/Server/)** - ASP.NET Core server
  - **[Client Application](../Portal/src/Client/)** - Blazor WebAssembly client
  - **[Administrative Interface](../Portal/src/Application/)** - Admin functionality

### Protocol & Provider Support

#### 📡 **Protocols**
- **[Protocol Support](../Protocols/README.md)** - Standard geospatial protocols
  - WMS (Web Map Service)
  - WMTS (Web Map Tile Service)
  - TMS (Tile Map Service)

#### 🔌 **External Providers**
- **[Provider Integration](../Providers/README.md)** - Third-party service integration
  - **[Bing Maps](../Providers/Bing/)** - Microsoft Bing Maps integration
  - **[Google Maps](../Providers/Google/)** - Google Maps integration

#### 🔧 **Extensions**
- **[Extensions Overview](../Extensions/README.md)** - Extended functionality
  - **[Datastore Extensions](../Extensions/Datastore/)** - Advanced data operations

---

## 🔧 Development Resources

### Technical Implementation
- **[Architecture Overview](technical-implementation-guide.md#-architecture-implementation-patterns)** - System architecture patterns
- **[Performance Guidelines](technical-implementation-guide.md#-advanced-features-phase-2)** - Performance optimization strategies
- **[Quality Gates](technical-implementation-guide.md#-quality-gates--validation)** - Code quality requirements

### Development Workflow
- **[Git Strategy](technical-implementation-guide.md#-development-workflow)** - Branching and commit guidelines
- **[Testing Strategy](technical-implementation-guide.md#-testing-strategy)** - Unit and integration testing
- **[Continuous Integration](technical-implementation-guide.md#-continuous-integration)** - CI/CD pipeline

### Technology Stack
- **.NET 9.0** - Core framework
- **ASP.NET Core** - Web framework
- **Blazor WebAssembly** - Client-side UI
- **Entity Framework Core** - Data access
- **SQLite/PostgreSQL** - Database support
- **Docker** - Containerization

---

## 📚 API Documentation

### Command Line Interface
```bash
# Core tile processing commands
tiler process --input data.tiff --output tiles/ --format mbtiles --zoom 0-18
tiler info --file data.tiff
tiler validate --mbtiles output.mbtiles
```

### REST API Endpoints
```http
# Tile serving endpoints
GET /tiles/{z}/{x}/{y}.png
GET /wms?SERVICE=WMS&REQUEST=GetMap&...
GET /wmts/{layer}/{tilematrixset}/{z}/{x}/{y}.png
```

### Library APIs
- **[Spatial API](../Spatial/README.md)** - Coordinate transformations and spatial operations
- **[Graphics API](../Graphics/README.md)** - Image processing operations
- **[Protocol API](../Protocols/README.md)** - Standard protocol implementations

---

## 🧪 Testing & Quality

### Test Coverage
- **Unit Tests** - Component-level testing (>80% coverage target)
- **Integration Tests** - End-to-end workflow validation
- **Performance Tests** - Benchmark and load testing
- **Compliance Tests** - Protocol standard validation

### Quality Metrics
- **Code Quality** - SonarQube analysis and standards
- **Performance Benchmarks** - Processing time and memory usage
- **Security Scanning** - Vulnerability assessment
- **Documentation Coverage** - API and user documentation

---

## 🚢 Deployment & Operations

### Deployment Options
- **Docker Containers** - Containerized deployment
- **Kubernetes** - Container orchestration
- **Cloud Platforms** - Azure, AWS, GCP support
- **On-Premises** - Self-hosted deployment

### Operations & Monitoring
- **Logging** - Structured logging with Serilog
- **Metrics** - Performance and usage metrics
- **Health Checks** - System health monitoring
- **Alerting** - Operational alerts and notifications

---

## 🤝 Contributing

### Development Setup
1. **Prerequisites** - .NET 9.0 SDK, Docker, Git
2. **Clone Repository** - `git clone https://github.com/wangkanai/planet.git`
3. **Build Solution** - `dotnet build`
4. **Run Tests** - `dotnet test`

### Contribution Guidelines
- **[Code Standards](technical-implementation-guide.md#-development-workflow)** - Coding conventions and standards
- **[Pull Request Process](scope-clarity-recommendations.md#-decision-framework-for-future-features)** - PR submission guidelines
- **[Issue Templates]** - Bug reports and feature requests

### Community Resources
- **GitHub Issues** - Bug reports and feature requests
- **Discussions** - Community Q&A and ideas
- **Wiki** - Community-maintained documentation

---

## 📝 Document Status

| Document | Status | Last Updated |
|----------|--------|--------------|
| [Development Roadmap](development-roadmap.md) | ✅ Current | 2025-01-07 |
| [Technical Implementation Guide](technical-implementation-guide.md) | ✅ Current | 2025-01-07 |
| [Scope Clarity & Recommendations](scope-clarity-recommendations.md) | ✅ Current | 2025-01-07 |
| Component READMEs | 🔄 In Progress | Ongoing |
| API Documentation | 📋 Planned | TBD |

---

## 🎯 Quick Navigation

**For Developers:**
- [Technical Implementation Guide](technical-implementation-guide.md)
- [Architecture Overview](technical-implementation-guide.md#-architecture-implementation-patterns)
- [Development Workflow](technical-implementation-guide.md#-development-workflow)

**For Project Managers:**
- [Development Roadmap](development-roadmap.md)
- [Scope Clarity & Recommendations](scope-clarity-recommendations.md)
- [Success Milestones](scope-clarity-recommendations.md#-strategic-recommendations)

**For Users:**
- [Getting Started Guide](../README.md)
- [Command Line Reference](technical-implementation-guide.md#-engine-console-command-structure)
- [API Documentation](#-api-documentation)

---

*This documentation is actively maintained and updated as the project evolves. For the most current information, please refer to the individual component documentation and the project repository.*
