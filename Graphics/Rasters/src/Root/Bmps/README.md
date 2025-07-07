# BMP (Bitmap) Raster Support

This directory contains the implementation of BMP (Windows Bitmap) format support for the Wangkanai Graphics Rasters library.

## Overview

BMP is a fundamental uncompressed raster format widely used in Windows environments. It serves as an excellent foundation for testing and basic image operations due to its simple structure and well-documented specification.

## Features

### Supported BMP Variants
- **BITMAPINFOHEADER** (40 bytes) - Most common variant
- **BITMAPV4HEADER** (108 bytes) - Extended with color space information  
- **BITMAPV5HEADER** (124 bytes) - Latest version with ICC color profiles

### Color Depth Support
- **1-bit** - Monochrome (black and white)
- **4-bit** - 16-color palette
- **8-bit** - 256-color palette
- **16-bit** - High color (RGB555, RGB565)
- **24-bit** - True color (RGB888)
- **32-bit** - True color with alpha channel (ARGB8888)

### Compression Types
- **BI_RGB** - Uncompressed (most common)
- **BI_RLE8** - 8-bit run-length encoding
- **BI_RLE4** - 4-bit run-length encoding
- **BI_BITFIELDS** - Uncompressed with custom bit masks

## File Structure

```
Bmps/
├── IBmpRaster.cs              # Interface definition
├── BmpRaster.cs               # Main implementation
├── BmpMetadata.cs             # Metadata handling
├── BmpConstants.cs            # Format constants
├── BmpColorDepth.cs           # Color depth enumeration
├── BmpCompression.cs          # Compression types
├── BmpValidator.cs            # Format validation
├── BmpValidationResult.cs     # Validation results
├── BmpExamples.cs             # Usage examples
└── README.md                  # This documentation
```

## Usage Examples

### Creating a Simple RGB BMP

```csharp
using Wangkanai.Graphics.Rasters.Bmps;

// Create a 24-bit RGB image
var bmp = BmpExamples.CreateRgb24(800, 600);

// Validate the image
var validation = BmpValidator.Validate(bmp);
if (validation.IsValid)
{
    Console.WriteLine($"BMP created successfully: {bmp.GetEstimatedFileSize():N0} bytes");
}
```

### Creating an Image with Alpha Channel

```csharp
// Create a 32-bit ARGB image
var bmp = BmpExamples.CreateArgb32(800, 600);

// Check transparency support
if (bmp.HasTransparency)
{
    Console.WriteLine("Image supports alpha channel");
}
```

### Creating an Indexed Color Image

```csharp
// Create an 8-bit grayscale image
var bmp = BmpExamples.CreateGrayscale8(800, 600);

// The palette is automatically generated
Console.WriteLine($"Palette colors: {bmp.Metadata.PaletteColors}");
Console.WriteLine($"Palette size: {bmp.Metadata.PaletteSizeInBytes} bytes");
```

### Custom Bit Masks for 16-bit Images

```csharp
// Create RGB565 format
var bmp = BmpExamples.CreateRgb565(800, 600);

// Get bit masks
var (red, green, blue, alpha) = bmp.GetBitMasks();
Console.WriteLine($"RGB565 - R:{red:X}, G:{green:X}, B:{blue:X}");
```

### Format Conversion

```csharp
// Create a 32-bit image and convert to RGB
var bmp = BmpExamples.CreateArgb32(800, 600);
Console.WriteLine($"Before: {bmp.ColorDepth}");

bmp.ConvertToRgb();
Console.WriteLine($"After: {bmp.ColorDepth}");
```

## Validation

The BMP implementation includes comprehensive validation:

```csharp
var bmp = new BmpRaster(800, 600, BmpColorDepth.TwentyFourBit);
var result = BmpValidator.Validate(bmp);

Console.WriteLine($"Validation: {result.GetSummary()}");
foreach (var error in result.Errors)
    Console.WriteLine($"Error: {error}");
foreach (var warning in result.Warnings)
    Console.WriteLine($"Warning: {warning}");
```

## Advanced Features

### Top-Down Images

```csharp
// Create top-down format (negative height)
var bmp = BmpExamples.CreateTopDown(800, 600);
Console.WriteLine($"Top-down: {bmp.IsTopDown}");
```

### Custom Resolution

```csharp
// Set custom DPI
var bmp = BmpExamples.CreateWithResolution(800, 600, 300);
Console.WriteLine($"Resolution: {bmp.HorizontalResolution} pixels/meter");
```

### V5 Header with ICC Profile Support

```csharp
// Use V5 header for advanced color management
var bmp = BmpExamples.CreateWithV5Header(800, 600);
Console.WriteLine($"Header: {bmp.Metadata.HeaderType}");
```

## Memory Management

The BMP implementation follows the Graphics library's disposal patterns:

```csharp
// Synchronous disposal for small images
using var bmp = BmpExamples.CreateRgb24(100, 100);

// Asynchronous disposal for large images or metadata
await using var largeBmp = BmpExamples.CreateRgb24(10000, 10000);
```

## Performance Characteristics

### Memory Usage
- **24-bit RGB**: 3 bytes per pixel + headers
- **32-bit ARGB**: 4 bytes per pixel + headers  
- **8-bit indexed**: 1 byte per pixel + 1KB palette
- **Row padding**: Aligned to 4-byte boundaries

### Processing Speed
- **Fast reading/writing**: Simple uncompressed structure
- **No decompression overhead**: For BI_RGB format
- **RLE formats**: May require additional processing time

## Standards Compliance

The implementation follows official Microsoft BMP specifications:
- Windows Bitmap File Format specification
- Device Independent Bitmap (DIB) header formats
- Color space and ICC profile standards (V5 headers)

## Thread Safety

- **Immutable constants**: All BmpConstants are thread-safe
- **Instance safety**: Individual BmpRaster instances are not thread-safe
- **Validation**: BmpValidator static methods are thread-safe

## Integration

The BMP implementation integrates seamlessly with the Graphics library:

```csharp
// Works with base IRaster interface
IRaster raster = new BmpRaster(800, 600);

// Supports async disposal patterns
if (raster.HasLargeMetadata)
{
    await raster.DisposeAsync();
}
```

## Error Handling

Common validation errors and their solutions:

| Error | Solution |
|-------|----------|
| Invalid width/height | Ensure dimensions are positive and within limits |
| Compression mismatch | Match compression type to color depth |
| Missing palette | Provide palette for 1, 4, and 8-bit images |
| Invalid bit masks | Ensure masks don't overlap for BI_BITFIELDS |

For more examples and detailed usage, see `BmpExamples.cs`.