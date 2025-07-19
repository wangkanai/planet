# 🚀 Developer Onboarding Guide - Planet Solution

> **Welcome to Planet!** Your comprehensive guide to becoming productive with the Planet geospatial mapping platform.

## 📋 Quick Start Checklist

### Day 1: Environment Setup
- [ ] Clone repository: `git clone https://github.com/wangkanai/planet.git`
- [ ] Install .NET 9.0 SDK
- [ ] Install Visual Studio 2022 or JetBrains Rider
- [ ] Setup PostgreSQL (production) or use SQLite (development)
- [ ] Install Node.js for frontend build tools
- [ ] Run first build: `./build.ps1`
- [ ] Verify tests pass: `dotnet test`

### Day 2: Architecture Understanding
- [ ] Read [Architecture Index](ARCHITECTURE_INDEX.md)
- [ ] Review [API Documentation](API_DOCUMENTATION_INDEX.md)
- [ ] Explore domain boundaries and module structure
- [ ] Understand Clean Architecture principles used in Portal

### Day 3: First Contribution
- [ ] Pick a good first issue from GitHub
- [ ] Create feature branch
- [ ] Make change following coding guidelines
- [ ] Write tests for your change
- [ ] Submit pull request

---

## 🏗️ Development Environment Setup

### Prerequisites

#### Required Software
```yaml
.NET 9.0 SDK: Latest version
IDE: Visual Studio 2022 17.8+ or JetBrains Rider 2024.3+
Database: PostgreSQL 16+ (production) or SQLite (development)
Node.js: 20+ LTS for frontend tooling
PowerShell: 7.0+ for build scripts
Git: 2.40+ with LFS support
```

#### Optional but Recommended
```yaml
Docker Desktop: For containerized dependencies
Azure CLI: For cloud resources
SonarLint: Code quality analysis
GitHub CLI: For workflow automation
```

### Quick Setup Script

```powershell
# Clone and setup
git clone https://github.com/wangkanai/planet.git
cd planet

# Install dependencies
dotnet restore
npm install

# Initial build
./build.ps1

# Setup database (development)
cd Portal
./db.ps1 -update

# Run application
dotnet run --project Portal/src/Server
```

### IDE Configuration

#### Visual Studio 2022 Setup
```xml
<!-- Add to .editorconfig -->
[*.cs]
dotnet_style_qualification_for_field = false
dotnet_style_qualification_for_property = false
csharp_prefer_var_when_type_is_apparent = true
csharp_new_line_before_open_brace = all
```

#### JetBrains Rider Setup
- Enable nullable reference type analysis
- Configure code style to match project conventions
- Install SonarLint plugin for code quality
- Setup Git integration with conventional commits

---

## 🏛️ Architecture Deep Dive

### Solution Structure Overview

```
planet/
├── Portal/          # 🌐 Blazor web application (hybrid Server/WASM)
├── Engine/          # ⚙️ Console application for tile processing
├── Spatial/         # 📍 Geospatial data handling (coordinate systems, tiles)
├── Graphics/        # 🎨 Image processing (TIFF, PNG, JPEG, WebP, AVIF)
├── Providers/       # 🔌 External map service integrations (Bing, Google)
├── Protocols/       # 📡 Map service protocols (WMS implementations)
├── Extensions/      # 🛠️ Utilities and extension methods
└── docs/           # 📚 Documentation and guides
```

### Domain Boundaries & Responsibilities

#### Portal Domain - User Experience Layer
```yaml
Purpose: Web application, user identity, authentication
Technology: Blazor Server + WASM, ASP.NET Core Identity
Database: PostgreSQL with Entity Framework Core
Key Components:
  - Authentication/Authorization (PlanetUser, PlanetRole)
  - User interface components
  - Application services
  - Domain entities
```

#### Spatial Domain - Geospatial Intelligence
```yaml
Purpose: Coordinate systems, map calculations, tile addressing
Namespace: Wangkanai.Spatial
Key Types:
  - Coordinate, Geodetic, Mercator
  - TileIndex, TileAddress, MapExtent
  - Resolution, Attribution
Formats: MBTiles, GeoPackages, GeoTIFF, Shapefiles
```

#### Graphics Domain - Image Processing
```yaml
Purpose: Multi-format image processing and metadata management
Namespace: Wangkanai.Graphics
Key Interfaces: IImage, IRaster, IVector, IMetadata
Formats: JPEG, PNG, TIFF, WebP, AVIF, HEIF, BMP, JPEG2000, SVG
Features: Async disposal, validation, metadata extraction
```

#### Providers Domain - External Integration
```yaml
Purpose: Map service provider abstraction
Key Interface: IRemoteProvider
Implementations: BingProvider, GoogleProvider
Pattern: Strategy pattern for different tile sources
```

#### Protocols Domain - Service Standards
```yaml
Purpose: Map service protocol implementations
Standards: WMS (Web Map Service)
Versions: 1.0.0, 1.1.0, 1.1.1, 1.3.0
Pattern: Protocol abstraction with version-specific implementations
```

---

## 💻 Development Workflows

### Daily Development Cycle

#### 1. Start Development Session
```bash
# Pull latest changes
git pull origin main

# Create feature branch
git checkout -b feature/your-feature-name

# Verify build
./build.ps1

# Run tests
dotnet test
```

#### 2. Development Process
```bash
# Make changes following coding guidelines
# Write tests for your changes
# Run specific tests
dotnet test --project YourModule.Tests

# Check code quality
# IDE shows SonarLint warnings inline
```

#### 3. Pre-Commit Checklist
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build -c Release`
- [ ] No new warnings introduced
- [ ] Code follows style guidelines
- [ ] Documentation updated if needed

#### 4. Commit and Push
```bash
# Stage changes
git add .

# Commit with conventional message
git commit -m "feat(spatial): add coordinate validation for tile boundaries"

# Push feature branch
git push origin feature/your-feature-name
```

### Testing Strategy

#### Test Organization
```
YourModule/
├── src/Root/                    # Production code
├── tests/
│   ├── Unit/                    # Unit tests (fast, isolated)
│   ├── Integration/             # Integration tests (database, external services)
│   └── Platform/                # Test utilities, mocks, examples
│       ├── Mocks/
│       ├── Examples/
│       └── Extensions/          # Validation and comparison logic
```

#### Test Categories
```yaml
Unit Tests:
  Framework: xUnit v3
  Pattern: Arrange-Act-Assert
  Coverage Target: 80%+
  Speed: <1ms per test

Integration Tests:
  Database: In-memory SQLite for fast execution
  External Services: Mock providers with test data
  Coverage: Critical user journeys

Platform Tests:
  Purpose: Reusable test infrastructure
  Components: Mocks, test data, validation helpers
```

#### Example Test Structure
```csharp
// Unit test example
[Fact]
public void Coordinate_Constructor_SetsXYCorrectly()
{
    // Arrange
    var x = 123.45;
    var y = 67.89;
    
    // Act
    var coordinate = new Coordinate(x, y);
    
    // Assert
    Assert.Equal(x, coordinate.X);
    Assert.Equal(y, coordinate.Y);
}

// Integration test example
[Fact]
public async Task TileGeneration_ValidCoordinates_ReturnsValidTile()
{
    // Arrange
    using var context = TestDbContext.Create();
    var service = new TileGenerationService(context);
    var coordinate = new TileCoordinate { X = 1, Y = 1, Z = 2 };
    
    // Act
    var result = await service.GenerateTileAsync(coordinate);
    
    // Assert
    Assert.NotNull(result);
    Assert.True(result.IsValid);
}
```

---

## 🎯 Coding Guidelines & Best Practices

### C# Style Guidelines

#### Naming Conventions
```csharp
// Public members: PascalCase
public class TileProcessor { }
public void ProcessTile() { }
public int Width { get; set; }

// Private members: camelCase
private readonly ITileService _tileService;
private int _cacheSize;

// Constants: PascalCase
public const string DefaultFormat = "PNG";

// Local variables: camelCase with var when type obvious
var coordinate = new Coordinate(x, y);
IEnumerable<Tile> tiles = GetTiles();
```

#### Modern C# Patterns
```csharp
// Primary constructors (C# 12)
public class TileService(ITileRepository repository, ILogger<TileService> logger)
{
    private readonly ITileRepository _repository = repository;
    private readonly ILogger<TileService> _logger = logger;
}

// Required properties
public class PlanetUser : IdentityUser<int>
{
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
}

// File-scoped namespaces
namespace Wangkanai.Graphics.Rasters;

// Global using statements (in GlobalUsings.cs)
global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore;
```

#### Async/Await Best Practices
```csharp
// Correct: ConfigureAwait(false) in libraries
public async Task<Tile> GetTileAsync(TileCoordinate coordinate)
{
    var data = await _repository.GetTileDataAsync(coordinate).ConfigureAwait(false);
    return new Tile(data);
}

// Correct: ValueTask for frequently synchronous operations
public ValueTask<bool> IsCachedAsync(TileCoordinate coordinate)
{
    if (_cache.ContainsKey(coordinate))
        return ValueTask.FromResult(true);
    
    return CheckRemoteCacheAsync(coordinate);
}

// Correct: IAsyncDisposable pattern
public async ValueTask DisposeAsync()
{
    if (_disposed) return;
    
    try
    {
        await _httpClient.DisposeAsync().ConfigureAwait(false);
        _cache?.Dispose();
    }
    finally
    {
        _disposed = true;
    }
}
```

### Architecture Patterns

#### Dependency Injection
```csharp
// Service registration (Program.cs)
builder.Services.AddScoped<ITileGenerationService, TileGenerationService>();
builder.Services.AddScoped<ICoordinateTransformationService, CoordinateTransformationService>();

// Constructor injection
public class TileController(ITileGenerationService tileService, ILogger<TileController> logger)
{
    private readonly ITileGenerationService _tileService = tileService;
    private readonly ILogger<TileController> _logger = logger;
}
```

#### Repository Pattern
```csharp
// Repository interface
public interface ITileRepository
{
    Task<Tile?> GetTileAsync(TileCoordinate coordinate);
    Task SaveTileAsync(Tile tile);
    Task<bool> ExistsAsync(TileCoordinate coordinate);
}

// Implementation with EF Core
public class TileRepository(PlanetDbContext context) : ITileRepository
{
    public async Task<Tile?> GetTileAsync(TileCoordinate coordinate)
    {
        return await context.Tiles
            .FirstOrDefaultAsync(t => t.X == coordinate.X && t.Y == coordinate.Y && t.Z == coordinate.Z);
    }
}
```

#### Domain Events
```csharp
// Domain event
public record TileGenerationCompleted(TileCoordinate Coordinate, TimeSpan Duration);

// Event handler
public class TileGenerationHandler(ILogger<TileGenerationHandler> logger) : INotificationHandler<TileGenerationCompleted>
{
    public Task Handle(TileGenerationCompleted notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Tile {Coordinate} generated in {Duration}ms", 
            notification.Coordinate, notification.Duration.TotalMilliseconds);
        return Task.CompletedTask;
    }
}
```

---

## 🗂️ Module-Specific Guides

### Graphics Module Development

#### Key Interfaces
```csharp
// Core image abstraction
public interface IImage : IDisposable, IAsyncDisposable
{
    int Width { get; set; }
    int Height { get; set; }
    IMetadata Metadata { get; }
}

// Raster-specific interface
public interface IRaster : IImage { }

// Format-specific implementations
public interface IJpegRaster : IRaster { }
public interface IPngRaster : IRaster { }
public interface ITiffRaster : IRaster { }
```

#### Working with Metadata
```csharp
// Reading JPEG metadata
using var jpeg = new JpegRaster();
await jpeg.LoadAsync(stream);

var metadata = jpeg.Metadata as JpegMetadata;
if (metadata != null)
{
    Console.WriteLine($"Camera: {metadata.Camera}");
    Console.WriteLine($"Encoding: {metadata.Encoding}");
}

// Validating metadata
var validation = metadata.ValidateMetadata();
if (!validation.IsValid)
{
    foreach (var error in validation.Errors)
        logger.LogError("Metadata error: {Error}", error);
}
```

### Spatial Module Development

#### Coordinate Transformations
```csharp
// Geographic to Mercator conversion
var geodetic = new Geodetic { Latitude = 40.7128, Longitude = -74.0060 };
var mercator = Mercator.FromGeodetic(geodetic);

// Tile coordinate calculation
var tileIndex = TileCalculator.GetTileIndex(geodetic, zoomLevel: 12);
Console.WriteLine($"Tile: {tileIndex.X}, {tileIndex.Y}");

// Map extent calculations
var extent = new MapExtent
{
    MinX = -180, MinY = -85,
    MaxX = 180, MaxY = 85
};
var center = extent.Center;
```

### Portal Module Development

#### Identity Management
```csharp
// Custom user model
public sealed class PlanetUser : IdentityUser<int>
{
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public DateOnly Birthday { get; set; }
    public PlanetTheme Theme { get; set; }
}

// User creation
var user = new PlanetUser
{
    UserName = "john.doe",
    Email = "john@example.com",
    Firstname = "John",
    Lastname = "Doe",
    Theme = PlanetTheme.Dark
};

var result = await userManager.CreateAsync(user, password);
```

#### Blazor Components
```razor
@page "/dashboard"
@using Wangkanai.Planet.Portal.Domain

<PageTitle>Dashboard - Planet</PageTitle>

<div class="container-fluid">
    <div class="row">
        <div class="col-12">
            <h1>Welcome, @currentUser?.Firstname</h1>
        </div>
    </div>
    
    <div class="row">
        <div class="col-md-6">
            <TileMapComponent />
        </div>
        <div class="col-md-6">
            <UserStatsComponent />
        </div>
    </div>
</div>

@code {
    private PlanetUser? currentUser;
    
    protected override async Task OnInitializedAsync()
    {
        currentUser = await GetCurrentUserAsync();
    }
}
```

---

## 🔧 Build System & Tools

### Build Scripts

#### Primary Build Script
```powershell
# build.ps1 - Complete build pipeline
./build.ps1                    # Full clean, restore, build
./build.ps1 -Configuration Debug  # Debug build
./build.ps1 -SkipTests         # Build without running tests
```

#### Database Scripts
```powershell
# Portal/db.ps1 - Database management
./db.ps1 -add "AddUserTheme"   # Add new migration
./db.ps1 -update              # Apply migrations
./db.ps1 -list                # List all migrations
./db.ps1 -remove              # Remove last migration
./db.ps1 -reset               # Reset all migrations
```

#### Frontend Build
```bash
# NPM scripts for frontend assets
npm run build                 # Build CSS from SCSS
npm run watch                 # Watch and rebuild on changes
npm run clean                 # Clean generated files
npm run deploy                # Full deployment build
```

### Development Commands

#### Common Development Tasks
```bash
# Run specific test project
dotnet test --project Graphics/Rasters/tests/Unit

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "TestMethodName"

# Run Portal application
dotnet run --project Portal/src/Server

# Run Engine console
dotnet run --project Engine/src/Console

# Build release version
dotnet build -c Release -tl
```

#### Performance Testing
```bash
# Run graphics benchmarks
dotnet run --project Graphics/Rasters/src/Root/Graphics.Rasters.Benchmarks -c Release

# Engine performance test
./Engine/src/Console/build.ps1
./tiler --benchmark
```

---

## 🚨 Troubleshooting Guide

### Common Development Issues

#### Build Problems
```yaml
Issue: "CS0108 Member hides inherited member"
Location: Graphics module
Solution: Add 'new' keyword or fix inheritance hierarchy
Timeline: Immediate priority
```

```yaml
Issue: "NPM vulnerabilities detected"
Location: Portal frontend dependencies
Solution: npm audit fix
Timeline: Security priority
```

```yaml
Issue: "Database connection failed"
Cause: PostgreSQL not running or connection string incorrect
Solution: 
  - Check PostgreSQL service status
  - Verify connection string in appsettings.json
  - Use SQLite for development: "Data Source=planet.db"
```

#### Runtime Issues
```yaml
Issue: "Tile generation timeout"
Cause: External provider rate limiting
Solution:
  - Implement retry with exponential backoff
  - Add circuit breaker pattern
  - Cache frequently requested tiles
```

```yaml
Issue: "Memory leak in image processing"
Cause: Missing disposal of IImage instances
Solution:
  - Use 'using' statements or 'await using' for async
  - Implement IAsyncDisposable properly
  - Monitor memory usage in tests
```

### Development Environment Issues

#### IDE Configuration
```yaml
Issue: "IntelliSense not working"
Solution:
  - Clean and rebuild solution
  - Delete bin/obj folders
  - Restart IDE
  - Check .NET SDK version
```

```yaml
Issue: "Tests not discovered"
Cause: xUnit v3 configuration issue
Solution:
  - Check xunit.runner.json settings
  - Verify test project references
  - Rebuild test projects
```

---

## 📈 Performance Guidelines

### Graphics Processing Optimization

#### Memory Management
```csharp
// Correct: Dispose pattern
using var image = new JpegRaster();
await image.LoadAsync(stream);
// Automatically disposed

// Correct: Async disposal
await using var raster = new AvifRaster();
await raster.ProcessAsync();
// Async cleanup performed
```

#### Large Image Handling
```csharp
// Streaming approach for large files
public async Task<ProcessingResult> ProcessLargeImageAsync(Stream input)
{
    const int bufferSize = 8192;
    using var bufferedStream = new BufferedStream(input, bufferSize);
    
    // Process in chunks to avoid memory pressure
    await foreach (var chunk in ReadChunksAsync(bufferedStream))
    {
        await ProcessChunkAsync(chunk);
    }
}
```

### Database Performance

#### Query Optimization
```csharp
// Efficient tile querying
public async Task<IEnumerable<Tile>> GetTilesInRegionAsync(MapExtent extent, int zoomLevel)
{
    return await context.Tiles
        .Where(t => t.Z == zoomLevel)
        .Where(t => t.X >= extent.MinX && t.X <= extent.MaxX)
        .Where(t => t.Y >= extent.MinY && t.Y <= extent.MaxY)
        .AsNoTracking()  // Read-only optimization
        .ToListAsync();
}
```

#### Connection Management
```csharp
// Repository pattern with scoped lifetime
public class TileRepository(PlanetDbContext context) : ITileRepository
{
    // Context automatically managed by DI container
    // Connection pooling handled by EF Core
}
```

---

## 🎓 Learning Resources

### Essential Reading

#### Architecture & Design
- [Clean Architecture by Robert Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design Fundamentals](https://www.pluralsight.com/courses/domain-driven-design-fundamentals)
- [Microservices Patterns by Chris Richardson](https://microservices.io/patterns/)

#### .NET & C# Development
- [.NET 9.0 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/)
- [Entity Framework Core Performance](https://docs.microsoft.com/en-us/ef/core/performance/)

#### Geospatial Development
- [PostGIS Documentation](https://postgis.net/documentation/)
- [Web Map Tile Services](https://en.wikipedia.org/wiki/Tile_Map_Service)
- [Coordinate Reference Systems](https://spatialreference.org/)

### Video Resources
- [.NET Conf Sessions on Performance](https://www.youtube.com/dotnetconf)
- [NDC Conferences - Architecture Talks](https://www.youtube.com/ndcconferences)
- [Microsoft Build - .NET Sessions](https://mybuild.microsoft.com/)

### Community Resources
- [.NET Community Discord](https://discord.gg/dotnet)
- [Stack Overflow - .NET Tag](https://stackoverflow.com/questions/tagged/.net)
- [Reddit - r/dotnet](https://reddit.com/r/dotnet)

---

## 🏆 Career Development

### Skill Development Path

#### Level 1: Foundation (0-3 months)
- [ ] Master C# 12/13 language features
- [ ] Understand Clean Architecture principles
- [ ] Learn Entity Framework Core basics
- [ ] Contribute to bug fixes and small features
- [ ] Write comprehensive unit tests

#### Level 2: Proficiency (3-12 months)
- [ ] Design and implement new modules
- [ ] Optimize database queries and application performance
- [ ] Lead feature development from concept to deployment
- [ ] Mentor new team members
- [ ] Contribute to architectural decisions

#### Level 3: Expertise (12+ months)
- [ ] Design microservice extraction strategies
- [ ] Lead performance optimization initiatives
- [ ] Drive architectural evolution
- [ ] Represent team in cross-functional planning
- [ ] Contribute to open source geospatial libraries

### Contribution Opportunities

#### Code Contributions
- **Bug Fixes**: Start with GitHub issues labeled "good first issue"
- **Feature Development**: Pick up features aligned with your interests
- **Performance Optimization**: Focus on graphics processing or tile generation
- **Testing**: Improve test coverage from current 2.4% to target 80%+

#### Documentation Contributions
- **API Documentation**: Expand inline documentation and examples
- **Tutorials**: Create step-by-step guides for common scenarios
- **Architecture Decisions**: Document new patterns and decisions

#### Community Involvement
- **Tech Talks**: Share knowledge about geospatial development
- **Blog Posts**: Write about performance optimizations or architecture decisions
- **Open Source**: Contribute to related .NET geospatial libraries

---

## 🔗 Quick Reference Links

### Documentation
- [Architecture Index](ARCHITECTURE_INDEX.md) - Complete architectural overview
- [API Documentation](API_DOCUMENTATION_INDEX.md) - Comprehensive API reference
- [Technical Guide](technical-implementation-guide.md) - Implementation details
- [CLAUDE.md](../CLAUDE.md) - AI assistant guidelines and project context

### Development Resources
- [GitHub Repository](https://github.com/wangkanai/planet) - Source code and issues
- [GitHub Projects](https://github.com/wangkanai/planet/projects) - Project planning
- [GitHub Actions](https://github.com/wangkanai/planet/actions) - CI/CD pipelines
- [Discussions](https://github.com/wangkanai/planet/discussions) - Team collaboration

### External Tools & Services
- [SonarCloud Quality Gate](https://sonarcloud.io/project/overview?id=wangkanai_planet) - Code quality metrics
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/) - Official .NET docs
- [Entity Framework Core Docs](https://docs.microsoft.com/en-us/ef/core/) - Database access

---

## 📞 Getting Help

### Team Contacts
- **Architecture Questions**: Review architecture documentation or raise in discussions
- **Development Issues**: Create GitHub issue with detailed reproduction steps
- **Performance Concerns**: Reference performance benchmarking results and analysis

### Support Channels
1. **GitHub Issues**: Technical problems, bug reports, feature requests
2. **GitHub Discussions**: Design questions, brainstorming, general discussion
3. **Code Reviews**: Submit pull requests for collaborative development
4. **Documentation**: Update this guide based on your onboarding experience

### Emergency Contacts
- **Production Issues**: Follow incident response procedures
- **Security Concerns**: Report immediately through secure channels
- **Critical Bugs**: Mark issues with "critical" label for immediate attention

---

*Welcome to the Planet development team! This guide will evolve based on your feedback and experience. Please contribute improvements as you learn and grow with the platform.*