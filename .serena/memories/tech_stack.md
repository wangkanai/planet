# Technology Stack

## Core Framework
- **.NET 9.0** - Primary development framework
- **C#** - Main programming language with nullable reference types enabled
- **Target Framework**: net9.0

## Web Technologies
- **Blazor Server + WebAssembly** (hybrid hosting model)
- **ASP.NET Core Identity** for authentication
- **Tabler UI framework** for styling
- **Sass/SCSS** for CSS preprocessing
- **NPM** for frontend asset management

## Data & Storage
- **Entity Framework Core** with SQLite and PostgreSQL support
- **SQLite** - Primary database for Portal application
- **MBTiles format** - SQLite-based tile storage
- **GeoPackage** - OGC standard for geospatial data
- **GeoTIFF** - Georeferenced raster imagery

## Testing & Quality
- **xUnit v3** - Unit testing framework with testing platform support
- **BenchmarkDotNet** - Performance testing and benchmarking
- **SonarCloud** - Code quality analysis

## Development Tools
- **PowerShell scripts** for automation and build processes
- **JetBrains Rider** - Primary IDE with MCP integration
- **GitHub Actions** - CI/CD pipelines
- **EditorConfig** - Code formatting standards

## Graphics & Spatial Processing
- **Multi-format image support**: TIFF, PNG, JPEG, WebP, AVIF, HEIF
- **Geospatial libraries** for coordinate systems and projections
- **EPSG database** support (6000+ coordinate systems)
- **Async disposal patterns** for resource management