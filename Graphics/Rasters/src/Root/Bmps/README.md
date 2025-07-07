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

### Creating Indexed Color Images

```csharp
// Create an 8-bit grayscale image
var bmp = BmpExamples.CreateGrayscale8(800, 600);
Console.WriteLine($"Palette colors: {bmp.Metadata.PaletteColors}");
Console.WriteLine($"Palette size: {bmp.Metadata.PaletteSizeInBytes} bytes");

// Create a 4-bit 16-color image
var color16 = BmpExamples.Create16Color(800, 600);
Console.WriteLine($"16-color palette: {color16.Metadata.PaletteColors} colors");

// Create a 1-bit monochrome image
var mono = BmpExamples.CreateMonochrome(800, 600);
Console.WriteLine($"Monochrome: {mono.ColorDepth}");
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

### Web-Optimized BMPs

```csharp
// Create a BMP optimized for web usage with sRGB color space
var webBmp = BmpExamples.CreateForWeb(800, 600);
Console.WriteLine($"Web BMP: {webBmp.Metadata.HeaderType}, Color Space: {webBmp.Metadata.ColorSpaceType}");
```

### RLE Compression

```csharp
// Create an 8-bit BMP with RLE8 compression
var rle8Bmp = BmpExamples.CreateRle8(800, 600);
Console.WriteLine($"RLE8 Compression: {rle8Bmp.Compression}");
```

### Custom Bit Masks

```csharp
// Create a 16-bit BMP with custom color masks
var customBmp = BmpExamples.CreateWithCustomMasks(800, 600, 
    redMask: 0xF800,   // 5 bits
    greenMask: 0x07E0, // 6 bits  
    blueMask: 0x001F   // 5 bits
);
var (r, g, b, a) = customBmp.GetBitMasks();
Console.WriteLine($"Custom masks - R:0x{r:X}, G:0x{g:X}, B:0x{b:X}");
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

## Available Factory Methods

The `BmpExamples` class provides comprehensive factory methods for common BMP scenarios:

### Basic Formats
- `CreateRgb24(width, height)` - 24-bit true color RGB
- `CreateArgb32(width, height)` - 32-bit ARGB with alpha channel
- `CreateRgb565(width, height)` - 16-bit RGB565 high color
- `CreateGrayscale8(width, height)` - 8-bit grayscale with palette
- `CreateMonochrome(width, height)` - 1-bit black and white
- `Create16Color(width, height)` - 4-bit with standard VGA palette

### Specialized Formats
- `CreateForWeb(width, height)` - Web-optimized with sRGB color space
- `CreateWithV5Header(width, height)` - V5 header for ICC profiles
- `CreateRle8(width, height)` - 8-bit with RLE8 compression
- `CreateTopDown(width, height)` - Top-down format (negative height)
- `CreateWithResolution(width, height, dpi)` - Custom DPI setting
- `CreateWithCustomMasks(width, height, red, green, blue, alpha)` - Custom bit masks
- `CreateMinimal()` - Smallest possible BMP (1x1 monochrome)

### Demonstration
- `DemonstrateOperations()` - Showcases all features with examples
- `ValidateExample(bmp)` - Demonstrates validation with detailed output

## API Reference

### Core Classes
- **`BmpRaster`** - Main BMP implementation
- **`BmpMetadata`** - Comprehensive metadata for all header variants
- **`BmpValidator`** - Format validation with detailed error reporting
- **`BmpValidationResult`** - Validation results with errors and warnings
- **`BmpConstants`** - All BMP format specifications and bit masks

### Enumerations
- **`BmpColorDepth`** - Supported color depths (1, 4, 8, 16, 24, 32-bit)
- **`BmpCompression`** - Compression types (RGB, RLE4, RLE8, BITFIELDS, JPEG, PNG)

### Key Properties
- `Width`, `Height` - Image dimensions
- `ColorDepth` - Color depth enumeration value
- `Compression` - Compression method
- `RowStride` - Row size in bytes (4-byte aligned)
- `PixelDataSize` - Total pixel data size
- `HasPalette` - Whether image uses indexed colors
- `HasTransparency` - Whether image supports alpha channel
- `IsTopDown` - Whether image is stored top-down

### Key Methods
- `SetBitMasks(red, green, blue, alpha)` - Configure custom color masks
- `GetBitMasks()` - Retrieve current color masks
- `ApplyPalette(paletteData)` - Set color palette for indexed images
- `ConvertToRgb()` - Convert to uncompressed RGB format
- `GetEstimatedFileSize()` - Calculate expected file size
- `IsValid()` - Quick validation check

For more examples and detailed usage, see `BmpExamples.cs`.