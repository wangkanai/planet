## Unit Tests Code Guidelines

### Overview

This document provides comprehensive guidelines for writing unit tests in the Graphics Rasters module. The testing approach follows xUnit v3 patterns with a focus on comprehensive validation, async disposal testing, and format specification compliance.

### Testing Framework Configuration

- **xUnit v3** with Microsoft Testing Platform support
- **FluentAssertions** for expressive assertions
- **Moq** for mocking dependencies
- **coverlet.collector** for code coverage
- Global using directives in `Usings.cs`
- JSON schema-based `xunit.runner.json` configuration

### Test Structure Patterns

#### Directory Organization
Tests are organized by format, matching the source code structure:
```
Graphics/Rasters/tests/Unit/
├── Tiffs/
├── Heifs/
├── Jpegs/
├── Pngs/
├── Webps/
├── Avifs/
├── Bmps/
└── Usings.cs
```

#### Test File Types
Each format typically includes:
- `*RasterTests.cs` - Main raster class tests
- `*ValidatorTests.cs` - Validation logic tests  
- `*MetadataTests.cs` - Metadata handling tests
- `*ConstantsTests.cs` - Constants and enumerations tests
- `*ExamplesTests.cs` - Example/utility class tests
- `*EncodingOptionsTests.cs` - Encoding options tests

### Testing Patterns

#### 1. AAA Pattern (Arrange-Act-Assert)
All tests follow the standard AAA structure:

```csharp
[Fact]
public void Constructor_WithValidDimensions_SetsProperties()
{
    // Arrange
    const int width = 1920;
    const int height = 1080;
    
    // Act
    var raster = new TiffRaster(width, height);
    
    // Assert
    raster.Width.Should().Be(width);
    raster.Height.Should().Be(height);
}
```

#### 2. Theory Tests with InlineData
Use `[Theory]` with `[InlineData]` for parameterized testing:

```csharp
[Theory]
[InlineData(1, 1)]
[InlineData(100, 100)]
[InlineData(8192, 8192)]
public void Constructor_WithValidDimensions_CreatesInstance(int width, int height)
{
    // Act
    var raster = new TiffRaster(width, height);
    
    // Assert
    raster.Should().NotBeNull();
    raster.Width.Should().Be(width);
    raster.Height.Should().Be(height);
}
```

#### 3. Comprehensive Validation Testing
Test both valid and invalid configurations with specific error messages:

```csharp
[Theory]
[InlineData(0, 100, "width")]
[InlineData(-1, 100, "width")]
[InlineData(100, 0, "height")]
[InlineData(100, -1, "height")]
public void Constructor_WithInvalidDimensions_ThrowsArgumentException(int width, int height, string paramName)
{
    // Act & Assert
    var exception = Assert.Throws<ArgumentException>(() => new TiffRaster(width, height));
    exception.ParamName.Should().Be(paramName);
}
```

#### 4. Async Testing and Error Handling
Test async operations and their error conditions:

```csharp
[Fact]
public async Task DisposeAsync_WithLargeMetadata_CompletesSuccessfully()
{
    // Arrange
    var raster = new TiffRaster(8192, 8192);
    // ... setup large metadata
    
    // Act
    var disposing = raster.DisposeAsync();
    
    // Assert
    await disposing.Should().NotThrowAsync();
}
```

#### 5. Exception Testing
Thoroughly test argument validation and error conditions:

```csharp
[Fact]
public void SetBitsPerSample_WithNullArray_ThrowsArgumentNullException()
{
    // Arrange
    var raster = new TiffRaster(100, 100);
    
    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() => raster.SetBitsPerSample(null!));
    exception.ParamName.Should().Be("bitsPerSample");
}
```

#### 6. Disposal Testing
Test resource cleanup and disposal patterns:

```csharp
[Fact]
public async Task DisposeAsync_CalledMultipleTimes_DoesNotThrow()
{
    // Arrange
    var raster = new TiffRaster(100, 100);
    
    // Act & Assert
    await raster.DisposeAsync();
    await raster.DisposeAsync(); // Should not throw
}
```

#### 7. Constants Testing
Verify constant values and their relationships:

```csharp
[Fact]
public void TiffSignature_HasCorrectLength()
{
    // Assert
    TiffConstants.TiffSignatureBE.Length.Should().Be(4);
    TiffConstants.TiffSignatureLE.Length.Should().Be(4);
}

[Fact]
public void CompressionTypes_AreDistinct()
{
    // Arrange
    var values = new[]
    {
        TiffCompressionType.None,
        TiffCompressionType.Ccitt1D,
        TiffCompressionType.Group3Fax,
        TiffCompressionType.Group4Fax,
        TiffCompressionType.Lzw,
        TiffCompressionType.Jpeg,
        TiffCompressionType.PackBits
    };
    
    // Assert
    values.Should().OnlyHaveUniqueItems();
}
```

#### 8. Metadata Testing
Test size estimation and metadata handling:

```csharp
[Fact]
public void EstimatedMetadataSize_WithBasicProperties_ReturnsExpectedSize()
{
    // Arrange
    var metadata = new TiffMetadata
    {
        Width = 1920,
        Height = 1080,
        BitsPerSample = new[] { 8, 8, 8 }
    };
    
    // Act
    var size = metadata.EstimatedMetadataSize;
    
    // Assert
    size.Should().BeGreaterThan(0);
    size.Should().BeLessThan(1000); // Should be small for basic properties
}
```

#### 9. Format Compliance Testing
Test file format specifications and signatures:

```csharp
[Theory]
[InlineData(new byte[] { 0x4D, 0x4D, 0x00, 0x2A })] // Big-endian
[InlineData(new byte[] { 0x49, 0x49, 0x2A, 0x00 })] // Little-endian
public void IsValidTiffSignature_WithValidSignature_ReturnsTrue(byte[] signature)
{
    // Act
    var result = TiffValidator.IsValidSignature(signature);
    
    // Assert
    result.Should().BeTrue();
}
```

#### 10. Performance Testing
Test memory allocation and performance characteristics:

```csharp
[Fact]
public void BitsPerSample_WithFourOrFewerSamples_DoesNotAllocateArray()
{
    // Arrange
    var raster = new TiffRaster(100, 100);
    var samples = new[] { 8, 8, 8, 8 };
    
    // Act
    raster.SetBitsPerSample(samples);
    
    // Assert
    // This tests that inline storage is used for common cases
    raster.BitsPerSample.Should().Equal(samples);
}
```

### Advanced Testing Patterns

#### Collection and Span Tests
Test ReadOnlySpan operations and sequence comparisons:

```csharp
[Fact]
public void GetSignature_ReturnsCorrectSpan()
{
    // Act
    var signature = TiffConstants.GetSignature(ByteOrder.BigEndian);
    
    // Assert
    signature.ToArray().Should().Equal(new byte[] { 0x4D, 0x4D, 0x00, 0x2A });
}
```

#### Memory Allocation Tests
Test stack vs heap allocation patterns:

```csharp
[Fact]
public void SmallMetadata_UsesStackAllocation()
{
    // Arrange
    var metadata = new TiffMetadata();
    
    // Act
    var size = metadata.EstimatedMetadataSize;
    
    // Assert
    size.Should().BeLessThan(ImageConstants.LargeMetadataThreshold);
}
```

### Naming Conventions

#### Test Classes
- Use descriptive names ending with `Tests`
- Match the class being tested: `TiffRasterTests`, `TiffValidatorTests`
- Group related functionality: `TiffMetadataTests`, `TiffConstantsTests`

#### Test Methods
- Use descriptive names that explain the scenario and expected outcome
- Format: `MethodName_Scenario_ExpectedResult`
- Examples:
  - `Constructor_WithValidDimensions_SetsProperties`
  - `SetBitsPerSample_WithNullArray_ThrowsArgumentNullException`
  - `DisposeAsync_WithLargeMetadata_CompletesSuccessfully`

### Assertion Guidelines

#### FluentAssertions Usage
Use FluentAssertions for expressive and readable assertions:

```csharp
// Good
result.Should().BeTrue();
collection.Should().HaveCount(3);
exception.Should().BeOfType<ArgumentException>();

// Avoid
Assert.True(result);
Assert.Equal(3, collection.Count);
Assert.IsType<ArgumentException>(exception);
```

#### Specific Assertions
Use specific assertions for better error messages:

```csharp
// Good
values.Should().OnlyHaveUniqueItems();
text.Should().StartWith("Expected");

// Less specific
values.Should().Equal(values.Distinct());
text.Substring(0, 8).Should().Be("Expected");
```

### Best Practices

1. **Test One Thing**: Each test should verify one specific behavior
2. **Clear Naming**: Test names should clearly describe what is being tested
3. **Comprehensive Coverage**: Cover both happy path and error conditions
4. **Maintainable Tests**: Keep tests simple and focused
5. **Performance Aware**: Consider memory allocation and performance implications
6. **Format Compliance**: Ensure tests verify compliance with format specifications
7. **Resource Management**: Test proper disposal and cleanup patterns
8. **Thread Safety**: Consider concurrent access patterns where applicable

### Common Test Patterns Summary

- **Constructor Tests**: Validate parameter checking and property setting
- **Validation Tests**: Test format compliance and validation logic
- **Metadata Tests**: Verify metadata size estimation and handling
- **Disposal Tests**: Test async disposal and resource cleanup
- **Constants Tests**: Verify constant values and relationships
- **Exception Tests**: Test error conditions and argument validation
- **Performance Tests**: Test memory allocation and performance characteristics
- **Format Tests**: Test file format signatures and compliance
- **Collection Tests**: Test array and span operations
- **Async Tests**: Test asynchronous operations and error handling