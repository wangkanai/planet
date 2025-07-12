# Design Patterns & Guidelines

## Architectural Patterns

### Clean Architecture (Portal)
- **Domain Layer**: Core business entities and rules
- **Application Layer**: Use cases and business logic
- **Infrastructure Layer**: External concerns (databases, APIs)
- **Presentation Layer**: UI and controllers

### Dependency Injection
- Use **`IServiceCollection`** for service registration
- Follow **interface segregation** principle
- Prefer **constructor injection** over service locator pattern

### Repository Pattern
- Used in Portal for data access abstraction
- Entity Framework DbContext as Unit of Work
- Repository interfaces in Domain layer

## C# Specific Patterns

### Async/Await Pattern
```csharp
// Preferred async method pattern
public async Task<IEnumerable<T>> GetItemsAsync()
{
    // Implementation
}
```

### Resource Management
```csharp
// Use async disposal for Graphics operations
public async ValueTask DisposeAsync()
{
    // Cleanup resources
}
```

### Configuration Pattern
```csharp
// Strongly typed configuration
public class MyOptions
{
    public string Value { get; set; }
}

// Registration
services.Configure<MyOptions>(configuration.GetSection("MySection"));
```

## Graphics Processing Patterns

### Stream Processing
- **Async disposal** for efficient resource cleanup
- **Stream-based processing** for large images
- **Memory-efficient** operations for GeoTIFF handling

### Format Abstraction
- **Interface-based design** for multiple format support
- **Strategy pattern** for format-specific operations
- **Factory pattern** for format detection and creation

## Spatial Data Patterns

### Coordinate System Abstraction
- **Geodetic** and **Mercator** coordinate systems
- **Projection transformation** interfaces
- **Extent calculation** utilities

### Tile Processing
- **Map extent** and **tile calculation** abstractions
- **SQLite-based storage** for MBTiles and GeoPackages
- **Streaming tile generation** for large datasets

## Testing Patterns

### Unit Testing with xUnit v3
```csharp
[Fact]
public void Should_ReturnExpectedResult_When_ValidInput()
{
    // Arrange
    // Act
    // Assert
}

[Theory]
[InlineData(input1, expected1)]
[InlineData(input2, expected2)]
public void Should_HandleVariousInputs(input, expected)
{
    // Test implementation
}
```

### Performance Testing
- **BenchmarkDotNet** for performance measurements
- **Baseline comparisons** for regression testing
- **Memory allocation tracking**

## Error Handling Patterns

### Exception Handling
- Use **specific exceptions** rather than generic ones
- **Log errors** using `ILogger<T>`
- **Graceful degradation** for non-critical failures

### Validation
- **Input validation** at API boundaries
- **Domain validation** in business logic
- **Data annotation** for model validation

## Naming Conventions

### Projects and Namespaces
- **`Wangkanai.Planet.*`** for Planet-specific components
- **`Wangkanai.Graphics.*`** for graphics libraries
- **`Wangkanai.Spatial.*`** for spatial libraries

### File Organization
- **Feature-based** folder structure
- **Separate concerns** into different files
- **Consistent naming** across similar components