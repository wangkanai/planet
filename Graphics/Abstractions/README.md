## Wangkanai Graphics Abstractions

**Namespace:** `Wangkanai.Graphics`

The core abstractions and interfaces for the Wangkanai Graphics library, providing fundamental contracts for image processing operations across all graphics components. This foundational layer establishes the common interfaces and base types that enable consistent and extensible image manipulation capabilities throughout the graphics ecosystem.

## Project Overview

Graphics Abstractions serves as the foundational layer for the entire graphics processing system, defining the core interfaces and contracts that all graphics components must implement. This ensures consistency, interoperability, and extensibility across raster, vector, and other image processing operations.

## Technical Specifications

### Core Interfaces

#### IImage Interface
The primary abstraction representing any image object in the graphics system:

```csharp
/// <summary>Represents an image object with width and height properties.</summary>
public interface IImage : IDisposable
{
    /// <summary>Gets and sets the width of the image.</summary>
    int Width { get; set; }

    /// <summary>Gets and sets the height of the image.</summary>
    int Height { get; set; }
}
```

### Key Design Principles

- **Platform Agnostic**: Abstractions work across Windows, macOS, and Linux
- **Resource Management**: Implements `IDisposable` for proper resource cleanup
- **Extensibility**: Designed to support various image formats and processing operations
- **Performance**: Minimal overhead abstractions for high-performance scenarios
- **Consistency**: Common interface ensures uniform behavior across components

## Implementation Status

### ✅ Completed Features
- **Core IImage Interface**: Fundamental image abstraction with width/height properties
- **Resource Management**: IDisposable implementation for proper cleanup
- **Cross-Platform Support**: Platform-agnostic design
- **Foundation Layer**: Stable base for all graphics components

### 🚧 Current Implementation
The current implementation provides the essential foundation with:
- Basic image dimensions (Width, Height)
- Proper disposal pattern
- Clean namespace organization
- Project structure for extensibility

### 🔮 Future Enhancements
- **Extended Metadata Support**: Color depth, pixel format, compression info
- **Advanced Image Properties**: Resolution, color profile, orientation
- **Validation Framework**: Common validation interfaces
- **Performance Metrics**: Benchmarking and profiling interfaces

## Usage Examples

### Basic Image Interface Usage

```csharp
using Wangkanai.Graphics;

// Working with any image type through the common interface
public void ProcessImage(IImage image)
{
    Console.WriteLine($"Processing image: {image.Width}x{image.Height}");
    
    // Perform operations common to all image types
    if (image.Width > 1920 || image.Height > 1080)
    {
        // Handle large images
        Console.WriteLine("Large image detected");
    }
    
    // Always dispose properly
    using (image)
    {
        // Work with the image
    }
}
```

### Implementing Custom Image Types

```csharp
using Wangkanai.Graphics;

public class CustomImage : IImage
{
    public int Width { get; set; }
    public int Height { get; set; }
    
    private bool _disposed = false;
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
            }
            
            // Dispose unmanaged resources
            _disposed = true;
        }
    }
}
```

## Dependencies

- **.NET 9.0** - Target framework
- **System** - Core .NET types and interfaces

## Architecture Integration

The Graphics Abstractions layer serves as the foundation for:

### Dependent Components
- **[Graphics.Rasters](../Rasters)** - Raster image processing (extends IImage with IRaster)
- **[Graphics.Vectors](../Vectors)** - Vector graphics processing (extends IImage with IVector)
- **Future Components** - Any new graphics components will extend these abstractions

### Integration Points
- **Raster Components**: `IRaster : IImage` provides raster-specific functionality
- **Vector Components**: `IVector : IImage` provides vector-specific functionality
- **Processing Pipelines**: Common interface enables unified processing workflows
- **Resource Management**: Consistent disposal patterns across all components

## Performance Considerations

### Design Optimizations
- **Minimal Interface Surface**: Only essential properties to reduce overhead
- **Value Type Properties**: Width and Height as integers for performance
- **Lazy Loading Support**: Interface design supports deferred loading patterns
- **Memory Efficiency**: Minimal memory footprint for the abstraction layer

### Best Practices
- **Always Dispose**: Implement proper disposal patterns
- **Interface Segregation**: Keep interfaces focused and minimal
- **Performance Profiling**: Use abstractions that support benchmarking
- **Resource Pooling**: Design supports object pooling patterns

## Development Guidelines

### Extending the Abstractions
When adding new interfaces to the abstractions layer:

1. **Follow Interface Segregation**: Keep interfaces focused and minimal
2. **Maintain Backward Compatibility**: Preserve existing interface contracts
3. **Document Performance Impact**: Consider the performance implications
4. **Support Resource Management**: Implement proper disposal patterns
5. **Enable Testing**: Design for unit testing and mocking

### Testing Strategy
- **Interface Contracts**: Test all interface implementations
- **Resource Management**: Verify proper disposal behavior
- **Performance Benchmarks**: Measure abstraction overhead
- **Integration Tests**: Test with actual graphics components

## Related Projects

This abstractions layer is designed to work with:
- **Graphics.Rasters**: For raster image processing
- **Graphics.Vectors**: For vector graphics processing
- **System.Drawing**: For compatibility with existing graphics APIs
- **BenchmarkDotNet**: For performance analysis and optimization

## References

- [.NET Graphics APIs](https://docs.microsoft.com/en-us/dotnet/api/system.drawing)
- [IDisposable Pattern](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose)
- [Interface Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/interface)
- [Performance Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/performance/)