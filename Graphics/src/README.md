# Wangkanai Graphics

**Namespace:** `Wangkanai.Graphics`

The core and foundational interfaces for the Wangkanai Graphics library, providing the fundamental contracts that enable consistent and extensible image processing operations across all graphics components. This foundational layer establishes the common interfaces, base types, and resource management patterns that all graphics implementations must follow.

## Purpose and Scope

Graphics serves as the architectural foundation for the entire graphics processing ecosystem, defining the core interfaces and contracts that ensure consistency, interoperability, and extensibility across raster, vector, and future image processing operations.

### Key Responsibilities

- **Interface Definitions**: Provides the fundamental `IImage` and `IMetadata` interfaces that all graphics components implement
- **Resource Management**: Establishes consistent disposal patterns with both synchronous and asynchronous disposal support
- **Metadata Foundation**: Defines the base metadata architecture with intelligent disposal optimization
- **Constants and Thresholds**: Centralizes performance-related constants and memory management thresholds
- **Extensibility Framework**: Provides the base classes and patterns for extending graphics capabilities

## Supported Capabilities

### Core Interfaces

#### IImage Interface
The primary representing any image object in the graphics system:

```csharp
/// <summary>Represents an image object with width and height properties.</summary>
public interface IImage : IDisposable, IAsyncDisposable
{
    /// <summary>Gets and sets the width of the image.</summary>
    int Width { get; set; }

    /// <summary>Gets and sets the height of the image.</summary>
    int Height { get; set; }

    /// <summary>Gets the metadata associated with this image.</summary>
    IMetadata Metadata { get; }
}
```

#### IMetadata Interface
The base interface for all metadata implementations with advanced disposal patterns:

```csharp
/// <summary>
/// Base interface for all metadata implementations in the Graphics library.
/// Provides a common contract for resource cleanup and size estimation.
/// </summary>
public interface IMetadata : IDisposable, IAsyncDisposable
{
    /// <summary>Gets or sets the image width in pixels.</summary>
    int Width { get; set; }

    /// <summary>Gets or sets the image height in pixels.</summary>
    int Height { get; set; }

    /// <summary>Gets a value indicating whether the metadata is large and benefits from async disposal.</summary>
    bool HasLargeMetadata { get; }

    /// <summary>Gets the estimated size of metadata in bytes.</summary>
    long EstimatedMetadataSize { get; }
}
```

### Resource Management Architecture

#### Advanced Disposal Patterns
The Graphics library implements sophisticated resource management with intelligent disposal strategies:

```csharp
/// <summary>Constants used throughout the Graphics component library.</summary>
public static class ImageConstants
{
    /// <summary>The threshold in bytes for determining if metadata is considered "large".</summary>
    public const long LargeMetadataThreshold = 1_000_000; // 1MB

    /// <summary>The threshold in bytes for very large metadata requiring explicit GC.</summary>
    public const long VeryLargeMetadataThreshold = 10_000_000; // 10MB

    /// <summary>The suggested batch size for processing large collections during disposal.</summary>
    public const int DisposalBatchSize = 100;
}
```

#### Intelligent Disposal Strategies
- **Small Metadata (<1MB)**: Synchronous disposal for optimal performance
- **Large Metadata (>1MB)**: Asynchronous disposal with yielding to prevent blocking
- **Very Large Metadata (>10MB)**: Includes explicit garbage collection suggestions
- **Batched Processing**: Large collections are processed in batches with `Task.Yield()`

### Metadata Base Implementation

#### MetadataBase Abstract Class
Provides common functionality for all metadata implementations:

```csharp
public abstract class MetadataBase : IMetadata
{
    /// <summary>Gets or sets the author or artist name.</summary>
    public virtual string? Author { get; set; }

    /// <summary>Gets or sets the copyright information.</summary>
    public virtual string? Copyright { get; set; }

    /// <summary>Gets or sets the description.</summary>
    public virtual string? Description { get; set; }

    /// <summary>Gets or sets the software used to create or modify the content.</summary>
    public virtual string? Software { get; set; }

    /// <summary>Gets or sets the creation date and time.</summary>
    public virtual DateTime? CreationTime { get; set; }

    /// <summary>Gets or sets the modification date and time.</summary>
    public virtual DateTime? ModificationTime { get; set; }

    /// <summary>Gets whether the metadata is large and benefits from async disposal.</summary>
    public virtual bool HasLargeMetadata => EstimatedMetadataSize > ImageConstants.LargeMetadataThreshold;

    /// <summary>Gets the estimated size of metadata in bytes.</summary>
    public abstract long EstimatedMetadataSize { get; }
}
```

## Architecture and Key Classes

### Class Hierarchy

```
IImage (Interface)
├── IDisposable
├── IAsyncDisposable
└── IMetadata Metadata { get; }

IMetadata (Interface)
├── IDisposable
├── IAsyncDisposable
├── bool HasLargeMetadata { get; }
└── long EstimatedMetadataSize { get; }

MetadataBase (Abstract Class)
├── Implements IMetadata
├── Common Properties (Author, Copyright, Description, etc.)
├── Memory Estimation Methods
├── Intelligent Disposal Logic
└── Abstract Methods for Derived Classes
```

### Key Design Patterns

#### Interface Segregation
- **Focused Interfaces**: Each interface has a single, well-defined responsibility
- **Minimal Surface Area**: Only essential properties and methods to reduce overhead
- **Composable Design**: Interfaces can be combined for complex scenarios

#### Resource Management
- **Dual Disposal**: Both synchronous and asynchronous disposal patterns
- **Intelligent Optimization**: Automatic selection of disposal strategy based on metadata size
- **Memory Estimation**: Sophisticated memory usage estimation for optimization decisions

#### Extensibility
- **Abstract Base Classes**: Provide common functionality while allowing specialization
- **Virtual Methods**: Allow derived classes to override behavior as needed
- **Template Method Pattern**: Consistent disposal workflow with customizable steps

## Usage Examples and Code Samples

### Basic Image Interface Implementation

```csharp
using Wangkanai.Graphics;

public class CustomImage : IImage
{
    private readonly CustomMetadata _metadata;
    private bool _disposed;

    public CustomImage(int width, int height)
    {
        Width = width;
        Height = height;
        _metadata = new CustomMetadata { Width = width, Height = height };
    }

    public int Width { get; set; }
    public int Height { get; set; }
    public IMetadata Metadata => _metadata;

    // Proper disposal implementation
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _metadata?.Dispose();
            _disposed = true;
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_metadata != null)
            await _metadata.DisposeAsync().ConfigureAwait(false);
        _disposed = true;
    }
}
```

### Custom Metadata Implementation

```csharp
using Wangkanai.Graphics;

public class CustomMetadata : MetadataBase
{
    private readonly Dictionary<string, object> _customProperties;

    public CustomMetadata()
    {
        _customProperties = new Dictionary<string, object>();
    }

    public Dictionary<string, object> CustomProperties => _customProperties;

    public override long EstimatedMetadataSize
    {
        get
        {
            var baseSize = GetBaseMemorySize();
            var customSize = EstimateDictionaryObjectSize(_customProperties);
            return baseSize + customSize;
        }
    }

    public override IMetadata Clone()
    {
        var clone = new CustomMetadata();
        CopyBaseTo(clone);

        // Copy custom properties
        foreach (var (key, value) in _customProperties)
            clone._customProperties[key] = value;

        return clone;
    }

    protected override void DisposeManagedResources()
    {
        _customProperties.Clear();
    }
}
```

### Working with Large Metadata

```csharp
using Wangkanai.Graphics;

public async Task ProcessLargeImageAsync()
{
    // Create image with potentially large metadata
    await using var image = new CustomImage(4096, 4096);

    // Check if metadata is large and requires optimization
    if (image.Metadata.HasLargeMetadata)
    {
        Console.WriteLine($"Large metadata detected: {image.Metadata.EstimatedMetadataSize:N0} bytes");
        Console.WriteLine("Using async disposal for optimal performance");
    }

    // Process image...
    await ProcessImageOperations(image);

    // Metadata is automatically disposed asynchronously when leaving scope
}

private async Task ProcessImageOperations(IImage image)
{
    // Common operations that work with any IImage implementation
    Console.WriteLine($"Processing image: {image.Width}x{image.Height}");

    // Access metadata through common interface
    var metadata = image.Metadata;
    metadata.Author = "Graphics Processing System";
    metadata.ModificationTime = DateTime.UtcNow;

    // Simulate processing work
    await Task.Delay(100);
}
```

### Memory Estimation and Optimization

```csharp
using Wangkanai.Graphics;

public class PerformanceOptimizedMetadata : MetadataBase
{
    private readonly List<byte[]> _binaryData;
    private readonly Dictionary<string, string> _stringData;

    public PerformanceOptimizedMetadata()
    {
        _binaryData = new List<byte[]>();
        _stringData = new Dictionary<string, string>();
    }

    public override long EstimatedMetadataSize
    {
        get
        {
            var baseSize = GetBaseMemorySize();

            // Estimate binary data size
            var binarySize = _binaryData.Sum(data => data.Length);

            // Estimate string data size
            var stringSize = EstimateDictionarySize(_stringData);

            return baseSize + binarySize + stringSize;
        }
    }

    public void AddBinaryData(byte[] data)
    {
        _binaryData.Add(data);

        // Check if we're approaching large metadata threshold
        if (EstimatedMetadataSize > ImageConstants.LargeMetadataThreshold * 0.8)
        {
            Console.WriteLine("Approaching large metadata threshold - consider optimization");
        }
    }

    public override IMetadata Clone()
    {
        var clone = new PerformanceOptimizedMetadata();
        CopyBaseTo(clone);

        // Deep copy binary data
        foreach (var data in _binaryData)
        {
            var copy = new byte[data.Length];
            Array.Copy(data, copy, data.Length);
            clone._binaryData.Add(copy);
        }

        // Copy string data
        foreach (var (key, value) in _stringData)
            clone._stringData[key] = value;

        return clone;
    }

    protected override void DisposeManagedResources()
    {
        _binaryData.Clear();
        _stringData.Clear();
    }
}
```

## Performance Considerations

### Memory Management Optimization

#### Intelligent Disposal Strategies
The abstractions layer implements sophisticated memory management based on metadata size:

```csharp
// Automatic disposal strategy selection
public virtual async ValueTask DisposeAsync()
{
    if (HasLargeMetadata)
        await Task.Run(() => Dispose(true)).ConfigureAwait(false);
    else
        Dispose(true);

    GC.SuppressFinalize(this);
}
```

#### Memory Estimation Utilities
Built-in methods for accurate memory usage estimation:

```csharp
// String size estimation using UTF-8 encoding
protected static long EstimateStringSize(string? str)
{
    if (string.IsNullOrEmpty(str)) return 0;
    return Encoding.UTF8.GetByteCount(str);
}

// Dictionary size estimation with overhead calculation
protected static long EstimateDictionarySize<TKey>(Dictionary<TKey, string>? dictionary)
{
    if (dictionary == null || dictionary.Count == 0) return 0;

    long size = dictionary.Count * 64; // Overhead per entry
    foreach (var value in dictionary.Values)
        size += EstimateStringSize(value);

    return size;
}
```

### Design Optimizations

#### Minimal Interface Overhead
- **Value Type Properties**: Width and Height as integers for performance
- **Single Responsibility**: Each interface has one clear purpose
- **Lazy Loading Support**: Interface design supports deferred initialization

#### Efficient Resource Management
- **Batch Processing**: Large collections processed in batches with yielding
- **Memory Thresholds**: Intelligent thresholds for disposal strategy selection
- **GC Optimization**: Explicit garbage collection suggestions for very large metadata

### Performance Best Practices

#### Implementation Guidelines
1. **Implement Both Disposal Patterns**: Always provide both sync and async disposal
2. **Accurate Size Estimation**: Implement EstimatedMetadataSize carefully for optimization
3. **Efficient Cloning**: Use deep copying only when necessary
4. **Resource Cleanup**: Always clear collections and null references in disposal

#### Usage Recommendations
1. **Use Async Disposal**: Prefer `await using` for large metadata scenarios
2. **Monitor Memory Usage**: Check `HasLargeMetadata` for optimization decisions
3. **Minimize Allocations**: Reuse objects when possible
4. **Proper Disposal**: Always dispose images and metadata properly

## Testing Information

### Unit Testing Strategy

#### Interface Contract Testing
```csharp
[Fact]
public void IImage_ShouldHaveRequiredProperties()
{
    // Arrange
    var image = new TestImage(1920, 1080);

    // Act & Assert
    Assert.True(image.Width > 0);
    Assert.True(image.Height > 0);
    Assert.NotNull(image.Metadata);
}

[Fact]
public async Task IImage_ShouldDisposeProperlyAsync()
{
    // Arrange
    var image = new TestImage(1920, 1080);

    // Act
    await image.DisposeAsync();

    // Assert
    Assert.Throws<ObjectDisposedException>(() => image.Width);
}
```

#### Metadata Testing
```csharp
[Fact]
public void MetadataBase_ShouldEstimateSizeAccurately()
{
    // Arrange
    var metadata = new TestMetadata();
    metadata.Author = "Test Author";
    metadata.Description = "Test Description";

    // Act
    var estimatedSize = metadata.EstimatedMetadataSize;

    // Assert
    Assert.True(estimatedSize > 0);
    Assert.True(estimatedSize < 10000); // Reasonable upper bound
}

[Theory]
[InlineData(500_000, false)]  // Below threshold
[InlineData(1_500_000, true)] // Above threshold
public void MetadataBase_ShouldDetectLargeMetadata(long size, bool expectedLarge)
{
    // Arrange
    var metadata = new TestMetadata { SimulatedSize = size };

    // Act
    var isLarge = metadata.HasLargeMetadata;

    // Assert
    Assert.Equal(expectedLarge, isLarge);
}
```

### Performance Testing

#### Disposal Performance Benchmarks
```csharp
[Fact]
public async Task MetadataDisposal_ShouldBeEfficient()
{
    // Arrange
    var metadata = new TestMetadata();
    var stopwatch = Stopwatch.StartNew();

    // Act
    await metadata.DisposeAsync();
    stopwatch.Stop();

    // Assert
    Assert.True(stopwatch.ElapsedMilliseconds < 100);
}
```

#### Memory Usage Validation
```csharp
[Fact]
public void MemoryEstimation_ShouldBeAccurate()
{
    // Arrange
    var metadata = new TestMetadata();
    var largeData = new byte[1_000_000];

    // Act
    var estimatedBefore = metadata.EstimatedMetadataSize;
    metadata.AddBinaryData(largeData);
    var estimatedAfter = metadata.EstimatedMetadataSize;

    // Assert
    Assert.True(estimatedAfter > estimatedBefore);
    Assert.True(estimatedAfter >= largeData.Length);
}
```

## Contributing Guidelines

### Development Standards

#### Code Style Requirements
1. **Consistent Naming**: Use descriptive names for all interfaces and classes
2. **XML Documentation**: Document all public members with comprehensive XML comments
3. **Null Safety**: Use nullable reference types and handle null cases appropriately
4. **Async Patterns**: Follow async/await best practices for all async operations

#### Interface Design Guidelines
1. **Single Responsibility**: Each interface should have one clear purpose
2. **Minimal Surface Area**: Include only essential members in interfaces
3. **Backward Compatibility**: Maintain interface stability across versions
4. **Extensibility**: Design interfaces to support future enhancements

### Testing Requirements

#### Unit Test Coverage
- **Interface Contracts**: Test all interface implementations
- **Disposal Patterns**: Verify both sync and async disposal work correctly
- **Memory Estimation**: Validate size estimation accuracy
- **Performance**: Benchmark disposal and memory usage

#### Integration Testing
- **Cross-Component**: Test interface usage across different graphics components
- **Large Data**: Test behavior with large metadata scenarios
- **Resource Cleanup**: Verify proper resource cleanup under various conditions

### Documentation Standards

#### API Documentation
- **Comprehensive XML Comments**: Document all public members
- **Usage Examples**: Provide clear examples for common scenarios
- **Performance Notes**: Document performance implications
- **Thread Safety**: Specify thread safety guarantees

#### Architecture Documentation
- **Design Decisions**: Document architectural choices and trade-offs
- **Extension Points**: Explain how to extend the abstractions
- **Migration Guides**: Provide guidance for version migrations

## Dependencies

### Required Dependencies
- **.NET 9.0** - Target framework for modern language features
- **System** - Core .NET types and interfaces
- **System.Text** - UTF-8 encoding for string size estimation

### Optional Dependencies
- **System.Diagnostics** - For performance monitoring and debugging
- **System.Memory** - For advanced memory management scenarios

## Integration with Graphics Components

### Raster Graphics Integration
```csharp
// Raster components extend the base abstractions
public interface IRaster : IImage { }
public interface IRasterMetadata : IMetadata
{
    int BitDepth { get; set; }
    byte[]? ExifData { get; set; }
    // Additional raster-specific properties
}
```

### Vector Graphics Integration
```csharp
// Vector components extend the base abstractions
public interface IVector : IImage { }
public interface IVectorMetadata : IMetadata
{
    // Vector-specific metadata properties
}
```

### Processing Pipeline Integration
```csharp
// Common processing operations work with any IImage
public async Task ProcessImageAsync(IImage image)
{
    // Universal operations using the common interface
    Console.WriteLine($"Processing {image.Width}x{image.Height} image");

    // Access metadata through common interface
    var metadata = image.Metadata;
    metadata.ModificationTime = DateTime.UtcNow;

    // Proper async disposal
    await image.DisposeAsync();
}
```

## Future Enhancements

### Planned Features
- **Extended Metadata Support**: Additional common metadata properties
- **Validation Framework**: Common validation interfaces for image compliance
- **Performance Metrics**: Built-in performance monitoring interfaces
- **Streaming Support**: Interfaces for streaming large image data

### API Evolution
- **Backward Compatibility**: Maintain interface stability
- **Versioning Strategy**: Semantic versioning for interface changes
- **Migration Support**: Tools and guidance for API migrations

---

## Summary

The Graphics Abstractions layer provides the essential foundation for the entire Wangkanai Graphics ecosystem. By establishing consistent interfaces, sophisticated resource management patterns, and extensible base classes, it enables the development of high-performance, maintainable graphics processing components.

The intelligent disposal patterns, memory optimization strategies, and comprehensive metadata architecture ensure that graphics applications can handle everything from small thumbnails to large professional images efficiently and reliably.

This foundation enables developers to build graphics applications that are both performant and maintainable, while providing the flexibility to extend and customize the system for specific needs.
