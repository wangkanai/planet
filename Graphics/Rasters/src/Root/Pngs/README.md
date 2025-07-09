# PNG Technical Specification

## Overview

The PNG (Portable Network Graphics) format implementation provides comprehensive support for PNG images in the Planet
graphics library.
This implementation adheres to the PNG specification (RFC 2083) and offers high-performance processing with full
metadata support.

**Namespace**: `Wangkanai.Graphics.Rasters.Pngs`
**Main Class**: `PngRaster`
**Interface**: `IPngRaster`
**Metadata Class**: `PngMetadata`
**File Extension**: `.png`
**MIME Type**: `image/png`

## Format Specifications

### Supported Features

#### **Image Dimensions**

- **Width**: 1 to 2,147,483,647 pixels (int.MaxValue)
- **Height**: 1 to 2,147,483,647 pixels (int.MaxValue)
- **Practical Limits**: Memory and processing constraints apply

#### **Color Types**

- **Grayscale** (Type 0): Single grayscale channel
- **Truecolor** (Type 2): RGB color model
- **Indexed Color** (Type 3): Palette-based color with up to 256 entries
- **Grayscale with Alpha** (Type 4): Grayscale with transparency
- **Truecolor with Alpha** (Type 6): RGB with transparency

#### **Bit Depths**

- **Grayscale**: 1, 2, 4, 8, 16 bits
- **Truecolor**: 8, 16 bits per channel
- **Indexed Color**: 1, 2, 4, 8 bits
- **Grayscale with Alpha**: 8, 16 bits per channel
- **Truecolor with Alpha**: 8, 16 bits per channel

#### **Compression Methods**

- **DEFLATE**: Standard PNG compression using LZ77 and Huffman coding
- **Compression Levels**: 0-9 (0=no compression, 9=maximum compression)
- **Filter Methods**: Standard PNG filtering (None, Sub, Up, Average, Paeth)

#### **Interlacing Methods**

- **None**: Sequential pixel order
- **Adam7**: Progressive interlacing for web display

## Technical Implementation

### Core Classes

#### PngRaster Class

```csharp
public sealed class PngRaster : Raster, IPngRaster
{
    // Core properties
    public PngColorType ColorType { get; set; }
    public byte BitDepth { get; set; }
    public PngCompression Compression { get; set; }
    public PngFilterMethod FilterMethod { get; set; }
    public PngInterlaceMethod InterlaceMethod { get; set; }
    public int CompressionLevel { get; set; }

    // Transparency and palette support
    public bool HasTransparency { get; set; }
    public bool HasAlphaChannel { get; set; }
    public bool UsesPalette { get; set; }
    public ReadOnlyMemory<byte> PaletteData { get; set; }
    public ReadOnlyMemory<byte> TransparencyData { get; set; }

    // Computed properties
    public int SamplesPerPixel { get; }
    public PngMetadata PngMetadata { get; }
}
```

#### PngMetadata Class

```csharp
public class PngMetadata : RasterMetadataBase
{
    // Standard metadata
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? Modified { get; set; }

    // Color management
    public double? Gamma { get; set; }
    public byte? SrgbRenderingIntent { get; set; }
    public (uint x, uint y)? WhitePoint { get; set; }
    public (uint x, uint y)? RedPrimary { get; set; }
    public (uint x, uint y)? GreenPrimary { get; set; }
    public (uint x, uint y)? BluePrimary { get; set; }
    public uint? BackgroundColor { get; set; }

    // Resolution
    public uint? XResolution { get; set; }
    public uint? YResolution { get; set; }
    public byte? ResolutionUnit { get; set; }

    // Text chunks
    public Dictionary<string, string> TextChunks { get; }
    public Dictionary<string, string> CompressedTextChunks { get; }
    public Dictionary<string, (string? languageTag, string? translatedKeyword, string text)> InternationalTextChunks { get; }
    public Dictionary<string, byte[]> CustomChunks { get; }
}
```

### Enumerations

#### PngColorType

```csharp
public enum PngColorType : byte
{
    Grayscale = 0,
    Truecolor = 2,
    IndexedColor = 3,
    GrayscaleWithAlpha = 4,
    TruecolorWithAlpha = 6
}
```

#### PngCompression

```csharp
public enum PngCompression : byte
{
    Deflate = 0  // Only compression method in PNG
}
```

#### PngInterlaceMethod

```csharp
public enum PngInterlaceMethod : byte
{
    None = 0,
    Adam7 = 1
}
```

## Usage Examples

### Basic Usage

#### Creating a Simple PNG

```csharp
// Create a basic truecolor PNG
var png = new PngRaster(800, 600)
{
    ColorType = PngColorType.Truecolor,
    BitDepth = 8,
    CompressionLevel = 6
};

// Validate the configuration
var validationResult = png.Validate();
if (!validationResult.IsValid)
{
    Console.WriteLine($"Validation failed: {validationResult.GetSummary()}");
}
```

#### PNG with Alpha Channel

```csharp
var pngWithAlpha = new PngRaster(1920, 1080)
{
    ColorType = PngColorType.TruecolorWithAlpha,
    BitDepth = 8,
    HasAlphaChannel = true,
    CompressionLevel = 6
};
```

### Advanced Usage

#### Indexed Color PNG with Palette

```csharp
var indexedPng = new PngRaster(640, 480)
{
    ColorType = PngColorType.IndexedColor,
    BitDepth = 8,
    UsesPalette = true
};

// Create a grayscale palette
var paletteData = new byte[256 * 3];
for (int i = 0; i < 256; i++)
{
    paletteData[i * 3] = (byte)i;     // Red
    paletteData[i * 3 + 1] = (byte)i; // Green
    paletteData[i * 3 + 2] = (byte)i; // Blue
}
indexedPng.PaletteData = new ReadOnlyMemory<byte>(paletteData);
```

#### High-Quality PNG with Metadata

```csharp
var highQualityPng = new PngRaster(2048, 2048)
{
    ColorType = PngColorType.TruecolorWithAlpha,
    BitDepth = 16,
    CompressionLevel = 9,
    HasAlphaChannel = true
};

// Add metadata
highQualityPng.PngMetadata.Title = "High Quality Image";
highQualityPng.PngMetadata.Software = "Planet Graphics Library";
highQualityPng.PngMetadata.Created = DateTime.UtcNow;
highQualityPng.PngMetadata.Gamma = 2.2;
highQualityPng.PngMetadata.SrgbRenderingIntent = 0; // Perceptual
```

### Using PNG Examples Helper

```csharp
// Use built-in examples for common configurations
var webPng = PngExamples.CreateWebOptimized(1200, 800, useAlpha: true);
var grayscalePng = PngExamples.CreateGrayscale(800, 600, bitDepth: 8);
var highQualityPng = PngExamples.CreateHighQuality(2048, 2048);
```

## Performance Characteristics

### Memory Usage

- **Base Overhead**: ~200 bytes per PNG instance
- **Metadata**: Variable based on text chunks and custom data
- **Palette**: 3 bytes per color entry (RGB)
- **Transparency**: 1-6 bytes depending on color type

### Processing Performance

- **Validation**: O(1) for structure validation
- **Metadata Access**: O(1) for property access
- **Compression Estimation**: O(1) calculation based on image parameters

### Compression Efficiency

| Compression Level | Speed    | Size     | Use Case                 |
|-------------------|----------|----------|--------------------------|
| 0                 | Fastest  | Largest  | Real-time processing     |
| 1-3               | Fast     | Large    | Interactive applications |
| 4-6               | Balanced | Medium   | General purpose          |
| 7-9               | Slow     | Smallest | Archival storage         |

## Validation and Error Handling

### Validation Features

```csharp
var validationResult = png.Validate();

// Check validation status
if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }

    foreach (var warning in validationResult.Warnings)
    {
        Console.WriteLine($"Warning: {warning}");
    }
}
```

### Common Validation Errors

- Invalid width/height dimensions
- Incompatible color type and bit depth combinations
- Missing palette for indexed color images
- Invalid compression or filter methods
- Malformed metadata

### Performance Validation

```csharp
// Check if PNG configuration is valid
if (png.IsValid())
{
    // Estimate file size
    var estimatedSize = png.GetEstimatedFileSize();
    Console.WriteLine($"Estimated file size: {estimatedSize} bytes");

    // Get color depth
    var colorDepth = png.GetColorDepth();
    Console.WriteLine($"Color depth: {colorDepth} bits per pixel");
}
```

## PNG Format Constants

### File Signature

```csharp
// PNG signature: 89 50 4E 47 0D 0A 1A 0A
var signature = PngConstants.Signature;
var isValidPng = PngValidator.IsValidPngSignature(fileData);
```

### Chunk Types

```csharp
// Critical chunks
PngConstants.ChunkTypes.IHDR  // Image header
PngConstants.ChunkTypes.PLTE  // Palette
PngConstants.ChunkTypes.IDAT  // Image data
PngConstants.ChunkTypes.IEND  // Image trailer

// Ancillary chunks
PngConstants.ChunkTypes.tRNS  // Transparency
PngConstants.ChunkTypes.gAMA  // Gamma
PngConstants.ChunkTypes.cHRM  // Chromaticity
PngConstants.ChunkTypes.sRGB  // sRGB color space
PngConstants.ChunkTypes.pHYs  // Physical dimensions
PngConstants.ChunkTypes.tEXt  // Text data
PngConstants.ChunkTypes.zTXt  // Compressed text
PngConstants.ChunkTypes.iTXt  // International text
```

## Testing

### Unit Tests

The PNG implementation includes comprehensive unit tests covering:

- Construction and initialization
- Color type and bit depth validation
- Compression settings
- Metadata handling
- Palette and transparency validation
- Performance characteristics

### Test Examples

```csharp
// Run tests using xUnit
dotnet test Graphics/Rasters/tests/Unit/Pngs/

// Specific test categories
dotnet test --filter "Category=PngValidation"
dotnet test --filter "Category=PngMetadata"
dotnet test --filter "Category=PngPerformance"
```

### Validation Testing

```csharp
[Theory]
[InlineData(PngColorType.Grayscale, 1, true)]
[InlineData(PngColorType.Grayscale, 16, true)]
[InlineData(PngColorType.Truecolor, 8, true)]
[InlineData(PngColorType.Truecolor, 4, false)]
public void ValidateColorTypeAndBitDepth(PngColorType colorType, byte bitDepth, bool expected)
{
    var png = new PngRaster { ColorType = colorType, BitDepth = bitDepth };
    var result = png.Validate();
    Assert.Equal(expected, result.IsValid);
}
```

## Best Practices

### Performance Optimization

1. **Choose appropriate compression levels**:
	- Use level 6 for general purposes
	- Use level 9 for archival storage
	- Use level 1-3 for real-time processing

2. **Optimize color types**:
	- Use grayscale for monochrome images
	- Use indexed color for images with ≤256 colors
	- Use truecolor with alpha only when transparency is needed

3. **Memory management**:
	- Dispose of PNG instances when done
	- Use `await using` for async disposal
	- Clear large metadata collections manually if needed

### Code Quality

1. **Always validate PNG configurations**:
   ```csharp
   var result = png.Validate();
   if (!result.IsValid)
       throw new InvalidOperationException(result.GetSummary());
   ```

2. **Use appropriate bit depths**:
	- 8-bit for standard images
	- 16-bit for high-quality images
	- 1-4 bit for simple graphics

3. **Handle metadata properly**:
	- Set creation timestamps
	- Include software identification
	- Use appropriate color management settings

## Integration with Planet Graphics

### Raster Processing Pipeline

```csharp
// Integration with raster processing
IRaster raster = new PngRaster(800, 600);
var processor = new RasterProcessor();
var result = await processor.ProcessAsync(raster);
```

### Metadata Integration

```csharp
// Access metadata through base interfaces
IMetadata metadata = png.Metadata;
IRasterMetadata rasterMetadata = png.Metadata;
PngMetadata pngMetadata = png.PngMetadata;
```

### Validation Integration

```csharp
// Use with validation pipeline
var validator = new RasterValidator();
var validationResult = validator.Validate(png);
```

## Contributing

When contributing to the PNG implementation:

1. **Follow coding standards**:
	- Use expression bodies for simple properties
	- Validate input parameters
	- Include comprehensive XML documentation

2. **Add tests for new features**:
	- Unit tests for all public methods
	- Integration tests for complex scenarios
	- Performance tests for critical paths

3. **Maintain backward compatibility**:
	- Don't break existing APIs
	- Use appropriate versioning
	- Document breaking changes

4. **Performance considerations**:
	- Optimize hot paths
	- Use appropriate data structures
	- Consider memory allocation patterns

## References

- [PNG Specification (RFC 2083)](https://tools.ietf.org/html/rfc2083)
- [PNG Specification (W3C)](https://www.w3.org/TR/PNG/)
- [PNG Format Overview](https://en.wikipedia.org/wiki/Portable_Network_Graphics)
- [Planet Graphics Architecture](../../README.md)

## License

Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0
