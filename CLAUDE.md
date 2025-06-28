# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Build Commands
- `dotnet build -c Release -tl` - Build the entire solution in Release configuration
- `dotnet clean -c Release -tl` - Clean build artifacts
- `./build.ps1` - Full build script that includes clean, restore, build sequence

### Test Commands
- `dotnet test` - Run all tests across the solution
- `dotnet test --project <specific-test-project>` - Run tests for a specific project
- Tests use xUnit framework (check xunit.runner.json files in test projects)

### Development Commands
- `dotnet restore` - Restore NuGet packages
- `dotnet run --project Portal/src/Server` - Run the Portal web application
- `dotnet run --project Engine/src/Console` - Run the Engine console application

### Database Commands (Portal)
- `./Portal/db.ps1 -add "<migration-name>"` - Add new Entity Framework migration
- `./Portal/db.ps1 -list` - List all migrations
- `./Portal/db.ps1 -remove` - Remove the last migration
- `./Portal/db.ps1 -reset` - Clean all migrations and create initial migration

### Engine Console Build
- `./Engine/src/Console/build.ps1` - Build and publish Engine console as 'tiler' executable

## Architecture

### Solution Structure
The Planet solution follows a modular architecture with these main components:

**Portal** - Blazor Server/WASM hybrid web application with ASP.NET Core Identity
- Uses Clean Architecture patterns (Domain, Application, Infrastructure, Persistence layers)
- Client project for WebAssembly components
- Server project for Blazor Server hosting
- SQLite database with Entity Framework Core

**Engine** - Console application for map tile processing
- Domain layer for core business logic
- Console layer for CLI operations

**Spatial** - Geospatial data handling library
- Core coordinate systems (Geodetic, Mercator)
- Map extent and tile calculations
- Abstractions layer for extensibility

**Providers** - External map service integrations
- Bing Maps provider
- Google Maps provider
- Each provider has corresponding test projects

**Common** - Shared components and utilities

### Key Technologies
- .NET 9.0 with nullable reference types enabled
- Blazor Server + WebAssembly (hybrid hosting model)
- ASP.NET Core Identity for authentication
- Entity Framework Core with SQLite
- xUnit for testing
- PowerShell scripts for automation

### Database Context
- Portal uses `PlanetDbContext` with SQLite connection
- Identity system with custom `PlanetUser` and `PlanetRole` entities
- Migrations located in Portal/src/Persistence/Migrations

### Testing Strategy
- All major components have corresponding test projects
- Tests use xUnit framework
- Test projects follow naming convention: `<ProjectName>.Tests`

### Build Configuration
- Central package management via Directory.Packages.props
- Directory.Build.props defines common MSBuild properties
- Target framework: net9.0
- Solution uses .slnx format (new VS solution format)