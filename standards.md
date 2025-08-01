# Development Standards
## Wangkanai Planet Project

> **Version:** 1.0  
> **Date:** January 2025  
> **Last Updated:** January 2025

---

## 📋 Table of Contents

- [🎯 Overview](#-overview)
- [💻 Coding Standards](#-coding-standards)
- [🏗️ Architecture Standards](#️-architecture-standards)
- [📝 Documentation Standards](#-documentation-standards)
- [🧪 Testing Standards](#-testing-standards)
- [🔧 Development Workflow](#-development-workflow)
- [📦 Package Management](#-package-management)
- [🚀 Deployment Standards](#-deployment-standards)
- [🔒 Security Standards](#-security-standards)
- [📊 Performance Standards](#-performance-standards)

---

## 🎯 Overview

This document establishes the development standards for the Wangkanai Planet project. All contributors must adhere to these standards to ensure code quality, maintainability, and consistency across the entire codebase.

### Principles
- **Consistency** - Uniform coding style and patterns
- **Quality** - High code quality with comprehensive testing
- **Maintainability** - Clean, readable, and well-documented code
- **Performance** - Efficient and optimized implementations
- **Security** - Secure coding practices and vulnerability prevention

---

## 💻 Coding Standards

### .NET / C# Standards

#### Code Style
- **Framework:** .NET 9.0 target framework
- **Language Version:** C# 12.0 or latest stable
- **Style Guide:** Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **EditorConfig:** Use `.editorconfig` file for consistent formatting

#### Naming Conventions
```csharp
// Classes and Methods - PascalCase
public class TileProcessor
{
    public async Task ProcessTileAsync(GeoTiffFile input) { }
}

// Properties and Fields - PascalCase
public string OutputDirectory { get; set; }
private readonly ILogger _logger;

// Parameters and Variables - camelCase
public void ProcessFile(string inputPath, int zoomLevel) { }

// Constants - UPPER_CASE
public const int MAX_ZOOM_LEVEL = 22;

// Interfaces - I prefix
public interface ITileGenerator { }

// Generic Type Parameters - T prefix
public class Repository<TEntity> where TEntity : class { }
```

#### File Organization
```
Project/
├── src/
│   ├── Application/      # Application layer
│   ├── Domain/          # Domain models
│   ├── Infrastructure/  # External concerns
│   └── Presentation/    # Controllers, UI
├── tests/
│   ├── Unit/           # Unit tests
│   ├── Integration/    # Integration tests
│   └── Performance/    # Performance tests
└── docs/               # Project documentation
```

#### Code Quality Rules

**Required Practices:**
- Use meaningful variable and method names
- Keep methods under 50 lines when possible
- Limit class size to 500 lines maximum
- Use dependency injection for all external dependencies
- Implement proper error handling with specific exceptions
- Use async/await for I/O operations

**Forbidden Practices:**
- No hardcoded strings (use constants or configuration)
- No magic numbers (use named constants)
- No commented-out code in commits
- No public fields (use properties)
- No empty catch blocks

#### Example Implementation
```csharp
public class TileGenerator : ITileGenerator
{
    private readonly ILogger<TileGenerator> _logger;
    private readonly TileProcessingOptions _options;
    
    public TileGenerator(
        ILogger<TileGenerator> logger,
        IOptions<TileProcessingOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }
    
    public async Task<TileResult> GenerateTileAsync(
        TileRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting tile generation for {Request}", request);
            
            var result = await ProcessTileInternalAsync(request, cancellationToken);
            
            _logger.LogInformation("Tile generation completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate tile for {Request}", request);
            throw new TileGenerationException("Tile generation failed", ex);
        }
    }
}
```

---

## 🏗️ Architecture Standards

### Clean Architecture Pattern

#### Layer Responsibilities
```
┌─────────────────────────────────────────┐
│              Presentation               │  ← Controllers, UI, APIs
├─────────────────────────────────────────┤
│              Application                │  ← Use Cases, Services
├─────────────────────────────────────────┤
│                Domain                   │  ← Entities, Business Logic
├─────────────────────────────────────────┤
│             Infrastructure              │  ← Data Access, External APIs
└─────────────────────────────────────────┘
```

#### Dependency Rules
- **Inner layers** cannot depend on outer layers
- **Outer layers** can depend on inner layers
- **Dependencies** point inward only
- **Interfaces** defined in inner layers

### Design Patterns

#### Required Patterns
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **Factory Pattern** - Object creation
- **Strategy Pattern** - Algorithm selection
- **Command Pattern** - Request encapsulation

#### Example Repository Implementation
```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public class TileRepository : IRepository<Tile>
{
    private readonly DbContext _context;
    
    public TileRepository(DbContext context)
    {
        _context = context;
    }
    
    public async Task<Tile> GetByIdAsync(int id)
    {
        return await _context.Tiles
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
```

### Dependency Injection

#### Service Registration
```csharp
// Program.cs or Startup.cs
services.AddScoped<ITileGenerator, TileGenerator>();
services.AddScoped<IRepository<Tile>, TileRepository>();
services.AddSingleton<IConfiguration>(configuration);
services.AddTransient<IEmailService, EmailService>();
```

#### Lifetime Management
- **Singleton** - Configuration, logging
- **Scoped** - Business services, repositories
- **Transient** - Lightweight services, factories

---

## 📝 Documentation Standards

### Code Documentation

#### XML Documentation Comments
```csharp
/// <summary>
/// Processes a GeoTIFF file and generates map tiles.
/// </summary>
/// <param name="inputPath">Path to the input GeoTIFF file</param>
/// <param name="outputPath">Directory for generated tiles</param>
/// <param name="zoomLevels">Range of zoom levels to generate</param>
/// <returns>Task representing the processing operation</returns>
/// <exception cref="FileNotFoundException">Thrown when input file doesn't exist</exception>
/// <exception cref="TileProcessingException">Thrown when processing fails</exception>
public async Task ProcessGeoTiffAsync(
    string inputPath, 
    string outputPath, 
    ZoomRange zoomLevels)
```

#### README Requirements
Each project must include:
- **Purpose** - What the component does
- **Installation** - How to install/setup
- **Usage** - Basic usage examples
- **API Reference** - Key classes and methods
- **Contributing** - How to contribute

#### API Documentation
- Use **Swagger/OpenAPI** for REST APIs
- Generate documentation from XML comments
- Include example requests/responses
- Document error codes and messages

### Documentation Structure
```markdown
# Component Name

## Overview
Brief description of the component's purpose.

## Installation
```bash
dotnet add package Wangkanai.Planet.ComponentName
```

## Quick Start
```csharp
var processor = new TileProcessor();
await processor.ProcessAsync(inputFile);
```

## API Reference
Link to generated API documentation.

## Examples
Practical usage examples.

## Contributing
Guidelines for contributors.
```

---

## 🧪 Testing Standards

### Test Strategy

#### Test Pyramid
```
       /\
      /  \    ← E2E Tests (Few)
     /____\
    /      \   ← Integration Tests (Some)
   /________\
  /          \  ← Unit Tests (Many)
 /____________\
```

#### Test Types
- **Unit Tests** - Test individual components in isolation
- **Integration Tests** - Test component interactions
- **Performance Tests** - Test performance characteristics
- **End-to-End Tests** - Test complete user workflows

### Unit Testing Standards

#### Test Naming Convention
```csharp
[Test]
public void ProcessTile_WhenInputIsValid_ShouldReturnSuccessResult()
{
    // Arrange
    var processor = new TileProcessor();
    var input = CreateValidInput();
    
    // Act
    var result = processor.ProcessTile(input);
    
    // Assert
    Assert.That(result.IsSuccess, Is.True);
}
```

#### Test Structure
- **Arrange** - Set up test data and dependencies
- **Act** - Execute the method under test
- **Assert** - Verify the expected outcome

#### Coverage Requirements
- **Minimum Coverage** - 80% line coverage
- **Critical Path Coverage** - 100% for business logic
- **Public API Coverage** - 100% for public methods

#### Example Test Class
```csharp
[TestFixture]
public class TileGeneratorTests
{
    private TileGenerator _generator;
    private Mock<ILogger<TileGenerator>> _mockLogger;
    
    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<TileGenerator>>();
        _generator = new TileGenerator(_mockLogger.Object);
    }
    
    [Test]
    public async Task GenerateTileAsync_WhenRequestIsValid_ShouldReturnTile()
    {
        // Arrange
        var request = new TileRequest { X = 1, Y = 1, Z = 1 };
        
        // Act
        var result = await _generator.GenerateTileAsync(request);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data, Is.Not.Empty);
    }
    
    [Test]
    public void GenerateTileAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            () => _generator.GenerateTileAsync(null));
    }
}
```

### Integration Testing

#### Database Testing
```csharp
[TestFixture]
public class TileRepositoryIntegrationTests : DatabaseTestBase
{
    [Test]
    public async Task SaveTileAsync_WhenTileIsValid_ShouldPersistToDatabase()
    {
        // Arrange
        using var context = CreateTestContext();
        var repository = new TileRepository(context);
        var tile = new Tile { X = 1, Y = 1, Z = 1, Data = new byte[100] };
        
        // Act
        await repository.SaveAsync(tile);
        await context.SaveChangesAsync();
        
        // Assert
        var savedTile = await repository.GetByCoordinatesAsync(1, 1, 1);
        Assert.That(savedTile, Is.Not.Null);
    }
}
```

---

## 🔧 Development Workflow

### Git Workflow

#### Branch Strategy
```
main
├── develop
│   ├── feature/tile-processing
│   ├── feature/web-portal
│   └── hotfix/critical-bug
└── release/v1.0.0
```

#### Branch Naming
- **Feature branches** - `feature/description`
- **Bug fixes** - `bugfix/issue-number`
- **Hotfixes** - `hotfix/description`
- **Releases** - `release/version`

#### Commit Messages
```
type(scope): brief description

Detailed explanation of what changed and why.

Fixes #123
```

**Types:**
- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation
- `style` - Formatting, missing semicolons
- `refactor` - Code refactoring
- `test` - Adding tests
- `chore` - Maintenance tasks

#### Pull Request Process
1. **Create branch** from develop
2. **Implement changes** following standards
3. **Write tests** for new functionality
4. **Update documentation** if needed
5. **Create pull request** with description
6. **Code review** by team members
7. **Merge** after approval and tests pass

### Code Review Standards

#### Review Checklist
- [ ] Code follows established standards
- [ ] Tests are comprehensive and pass
- [ ] Documentation is updated
- [ ] No security vulnerabilities
- [ ] Performance considerations addressed
- [ ] Error handling is appropriate

#### Review Comments
- **Constructive** feedback with suggestions
- **Specific** comments about code changes
- **Educational** explanations for standards
- **Respectful** tone and language

---

## 📦 Package Management

### NuGet Package Standards

#### Package Versioning
- Use **Semantic Versioning** (SemVer)
- Format: `MAJOR.MINOR.PATCH`
- **MAJOR** - Breaking changes
- **MINOR** - New features (backward compatible)
- **PATCH** - Bug fixes (backward compatible)

#### Package Metadata
```xml
<PropertyGroup>
    <PackageId>Wangkanai.Planet.ComponentName</PackageId>
    <Version>1.0.0</Version>
    <Authors>Wangkanai</Authors>
    <Description>Component description</Description>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/wangkanai/planet</RepositoryUrl>
    <PackageTags>geospatial;mapping;tiles</PackageTags>
</PropertyGroup>
```

#### Dependency Management
- **Central Package Management** - Use Directory.Packages.props
- **Version Consistency** - Same version across solution
- **Security Updates** - Regular dependency updates
- **License Compatibility** - Check license compatibility

---

## 🚀 Deployment Standards

### Environment Configuration

#### Configuration Hierarchy
1. **appsettings.json** - Default settings
2. **appsettings.{Environment}.json** - Environment-specific
3. **Environment Variables** - Runtime overrides
4. **Command Line Arguments** - Deployment overrides

#### Docker Standards
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/", "src/"]
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wangkanai.Planet.Portal.dll"]
```

#### Health Checks
```csharp
services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<FileSystemHealthCheck>("filesystem")
    .AddCheck<ExternalServiceHealthCheck>("external-services");
```

---

## 🔒 Security Standards

### Authentication & Authorization

#### Authentication Requirements
- **JWT Tokens** for API authentication
- **ASP.NET Core Identity** for web authentication
- **Token Expiration** - Short-lived tokens (15 minutes)
- **Refresh Tokens** - Long-lived refresh capability

#### Authorization Patterns
```csharp
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteTile(int id)
{
    // Implementation
}

[Authorize(Policy = "TileAccess")]
public async Task<IActionResult> GetTile(int x, int y, int z)
{
    // Implementation
}
```

### Data Protection

#### Sensitive Data Handling
- **Encrypt** sensitive data at rest
- **Use HTTPS** for all communications
- **Sanitize** user inputs
- **Log security events** for auditing

#### Input Validation
```csharp
public class TileRequest
{
    [Range(0, 22)]
    public int ZoomLevel { get; set; }
    
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9\-_]+$")]
    public string LayerName { get; set; }
}
```

---

## 📊 Performance Standards

### Performance Requirements

#### Response Time Targets
- **Tile Requests** - < 200ms (cached), < 1s (generated)
- **API Requests** - < 100ms for data retrieval
- **File Processing** - < 30 seconds per GB
- **Database Queries** - < 50ms for simple queries

#### Memory Management
- **Dispose** resources properly
- **Use async/await** for I/O operations
- **Stream** large data processing
- **Monitor** memory usage in production

#### Example Performance Optimization
```csharp
public async Task<byte[]> ProcessLargeFileAsync(Stream input)
{
    using var memoryStream = new MemoryStream();
    
    // Process in chunks to avoid memory issues
    var buffer = new byte[8192];
    int bytesRead;
    
    while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
    {
        // Process chunk
        var processedChunk = ProcessChunk(buffer, bytesRead);
        await memoryStream.WriteAsync(processedChunk);
    }
    
    return memoryStream.ToArray();
}
```

### Monitoring & Metrics

#### Required Metrics
- **Request Rate** - Requests per second
- **Response Time** - Average and percentile response times
- **Error Rate** - Error percentage
- **Resource Usage** - CPU, memory, disk usage

#### Logging Standards
```csharp
_logger.LogInformation("Processing tile request {@Request}", request);
_logger.LogWarning("Slow tile generation: {Duration}ms", duration);
_logger.LogError(exception, "Failed to process tile {@Request}", request);
```

---

## 📝 Compliance & Quality Gates

### Pre-commit Checks
- [ ] Code compiles without warnings
- [ ] All tests pass
- [ ] Code coverage meets minimum threshold
- [ ] Security scan passes
- [ ] Performance benchmarks within limits

### Continuous Integration
- [ ] Automated build and test
- [ ] Code quality analysis (SonarQube)
- [ ] Security vulnerability scanning
- [ ] Package dependency checking
- [ ] Documentation generation

### Release Criteria
- [ ] All functionality implemented
- [ ] Test coverage > 80%
- [ ] Performance requirements met
- [ ] Security review completed
- [ ] Documentation updated

---

## 📞 Enforcement & Review

### Standard Updates
- **Review Schedule** - Quarterly review of standards
- **Update Process** - Team discussion and approval
- **Communication** - Announce changes to all team members
- **Training** - Provide training on new standards

### Quality Assurance
- **Code Reviews** - Enforce standards during review
- **Automated Checks** - Use tools to enforce standards
- **Team Training** - Regular training sessions
- **Metrics Tracking** - Monitor compliance metrics

---

*This document is reviewed and updated regularly to ensure it remains current with best practices and project needs.*