# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Coding Guidelines

- Always use a descriptive variable name

## Commands

### Build Commands
- `dotnet build -c Release -tl` - Build the entire solution in Release configuration
- `dotnet clean -c Release -tl` - Clean build artifacts
- `./build.ps1` - Full build script that includes clean, restore, build sequence

### Test Commands
- `dotnet test` - Run all tests across the solution
- `dotnet test --project <specific-test-project>` - Run tests for a specific project
- Tests use xUnit v3 framework with testing platform support enabled (check xunit.runner.json files in test projects)

### Development Commands
- `dotnet restore` - Restore NuGet packages
- `dotnet run --project Portal/src/Server` - Run the Portal web application
- `dotnet run --project Engine/src/Console` - Run the Engine console application

### Database Commands (Portal)
- `./Portal/db.ps1 -add "<migration-name>"` - Add new Entity Framework migration
- `./Portal/db.ps1 -list` - List all migrations
- `./Portal/db.ps1 -remove` - Remove the last migration
- `./Portal/db.ps1 -update` - Update database to latest migration
- `./Portal/db.ps1 -clean` - Clean all migration files
- `./Portal/db.ps1 -reset` - Clean all migrations and create initial migration

### Engine Console Build
- `./Engine/src/Console/build.ps1` - Build and publish Engine console as 'tiler' executable

### Frontend Commands (Portal)
- `npm run build` - Build CSS from SCSS sources
- `npm run watch` - Watch and rebuild CSS on changes
- `npm run lib` - Copy library files to wwwroot
- `npm run clean` - Clean generated files
- `npm run deploy` - Full deployment build (clean, lib, build)

## Architecture

### Solution Structure
The Planet solution follows a modular architecture with these main components organized in separate libraries for clear separation of concerns:

**Portal** - Blazor Server/WASM hybrid web application with ASP.NET Core Identity
- Uses Clean Architecture patterns (Domain, Application, Infrastructure, Persistence layers)
- Client project for WebAssembly components
- Server project for Blazor Server hosting
- SQLite database with Entity Framework Core

**Engine** - Console application for map tile processing
- Domain layer for core business logic
- Console layer for CLI operations

**Spatial** - Geospatial data handling library (namespace: `Wangkanai.Spatial`)
- Root: Core coordinate systems (Geodetic, Mercator), map extent and tile calculations
- MbTiles: MBTiles format support with SQLite-based tile storage
- GeoPackages: GeoPackage format support for geospatial data containers
- GeoTiffs: GeoTIFF format support for georeferenced raster imagery
- ShapeFiles: Shapefile format support for vector geospatial data
- MtPkgs: Map tile package format support

**Providers** - External map service integrations
- Bing Maps provider
- Google Maps provider
- Each provider has corresponding test projects

**Graphics** - Graphics processing and image handling library (namespace: `Wangkanai.Graphics`)
- Abstractions: Core image processing interfaces and contracts
- Rasters: Raster image processing with TIFF format support, metadata handling, and performance optimizations
- Vectors: Vector graphics processing and manipulation
- Includes comprehensive benchmarking and validation tools

**Protocols** - Map service protocol implementations
- WMS (Web Map Service) protocol support
- Root protocol abstractions and utilities
- Protocol-specific implementations for serving map tiles

**Extensions** - Extension methods and utilities for the Planet ecosystem
- Datastore: Data storage extensions and utilities

### Key Technologies
- .NET 9.0 with nullable reference types enabled
- Blazor Server + WebAssembly (hybrid hosting model)
- ASP.NET Core Identity for authentication
- Entity Framework Core with SQLite and PostgreSQL support
- xUnit v3 for testing with testing platform support
- PowerShell scripts for automation
- Sass/SCSS for styling with Tabler UI framework
- NPM for frontend asset management
- Graphics processing with TIFF format support and performance benchmarking
- Geospatial data handling with multiple format support (MBTiles, GeoPackages, GeoTIFF, Shapefiles)

### Database Context
- Portal uses `PlanetDbContext` with SQLite connection
- Identity system with custom `PlanetUser` and `PlanetRole` entities
- Migrations located in Portal/src/Persistence/Migrations

### Testing Strategy
- All major components have corresponding test projects
- Tests use xUnit v3 framework with testing platform support enabled
- Test projects follow naming convention: `<ProjectName>.Tests`

### Build Configuration
- Central package management via Directory.Packages.props
- Directory.Build.props defines common MSBuild properties
- Target framework: net9.0
- Solution uses .slnx format (new VS solution format)
- Frontend assets managed via NPM with Tabler UI components

### Project Structure Details
- **Portal/src/Server**: Main Blazor Server application with hybrid WASM components
- **Portal/src/Client**: Blazor WebAssembly client components
- **Portal/src/Application**: Application layer with business logic and Identity configuration
- **Portal/src/Domain**: Domain entities including custom Identity models
- **Portal/src/Infrastructure**: Infrastructure services and external integrations
- **Portal/src/Persistence**: Entity Framework data access with SQLite
- **Engine/src/Console**: Console application for tile processing operations
- **Engine/src/Domain**: Engine domain logic
- **Graphics/Abstractions/src**: Core graphics interfaces and abstractions
- **Graphics/Rasters/src/Root**: Raster image processing with TIFF support
- **Graphics/Vectors/src/Root**: Vector graphics processing capabilities
- **Spatial/src/Root**: Core spatial data types and coordinate systems (namespace: `Wangkanai.Spatial`)
- **Spatial/src/MbTiles**: MBTiles format implementation
- **Spatial/src/GeoPackages**: GeoPackage format support
- **Spatial/src/GeoTiffs**: GeoTIFF format support with Graphics.Rasters integration
- **Spatial/src/ShapeFiles**: Shapefile format support
- **Spatial/src/MtPkgs**: Map tile package format support
- **Protocols/src/Root**: Protocol abstractions and WMS implementations
- **Providers/src/Root**: Map service provider implementations
- **Extensions/Datastore/src**: Data storage extensions and utilities

## Memories

- Claude Code now has access to MCP
- SonarCloud Code Quality report is available https://sonarcloud.io/project/overview?id=wangkanai_planet via MCP
- GitHub repo is at https://github.com/wangkanai/planet
- Work item backlogs are in the GitHub issues https://github.com/wangkanai/planet/issues
- Discussion board is at https://github.com/wangkanai/planet/discussions
- Project planning is at https://github.com/wangkanai/planet/projects
- CI/CD pipelines are configured in the GitHub Actions workflows https://github.com/wangkanai/planet/actions
