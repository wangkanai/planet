# Appendix C.1: Format Compatibility Matrix

## Introduction

Understanding format compatibility is crucial for developing robust graphics processing applications. This comprehensive
matrix details the capabilities, limitations, and interoperability of various image formats, codecs, and color spaces.
Use this reference to make informed decisions about format selection and conversion strategies.

## Image Format Capabilities

### Raster Format Feature Matrix

| Format        | Max Resolution | Color Depth        | Alpha Channel | Compression        | Animation     | Metadata   | ICC Profiles | Hardware Decode |
|---------------|----------------|--------------------|---------------|--------------------|---------------|------------|--------------|-----------------|
| **JPEG**      | 65,535×65,535  | 8-bit/channel      | No            | Lossy (DCT)        | No            | EXIF, XMP  | Yes          | Yes (Wide)      |
| **JPEG 2000** | 2³²-1×2³²-1    | Up to 16-bit       | Optional      | Lossy/Lossless     | No            | XML boxes  | Yes          | Limited         |
| **PNG**       | 2³¹-1×2³¹-1    | Up to 16-bit       | Yes           | Lossless (DEFLATE) | No (APNG yes) | tEXt, iTXt | Yes          | Yes             |
| **WebP**      | 16,383×16,383  | 8-bit/channel      | Yes           | Lossy/Lossless     | Yes           | EXIF, XMP  | Yes          | Yes (Growing)   |
| **AVIF**      | 65,535×65,535  | Up to 12-bit       | Yes           | Lossy/Lossless     | Yes           | EXIF, XMP  | Yes          | Limited         |
| **HEIF/HEIC** | Device limited | Up to 16-bit       | Yes           | Lossy (HEVC)       | Yes           | Full       | Yes          | iOS/macOS       |
| **BMP**       | 32,767×32,767  | Up to 32-bit       | Yes           | None/RLE           | No            | Limited    | No           | Yes             |
| **TIFF**      | 2³²-1×2³²-1    | Up to 32-bit float | Yes           | Various            | Multi-page    | Extensive  | Yes          | Yes             |
| **GIF**       | 65,535×65,535  | 8-bit indexed      | Binary        | LZW                | Yes           | Limited    | No           | Yes             |
| **TGA**       | 65,535×65,535  | Up to 32-bit       | Yes           | None/RLE           | No            | Limited    | No           | Limited         |
| **EXR**       | No limit       | 32-bit float       | Yes           | Various            | No            | Extensive  | Yes          | GPU (Pro)       |
| **DDS**       | GPU limited    | Various            | Yes           | DXT/BC             | No            | Limited    | No           | GPU Native      |
| **RAW**       | Sensor limited | 12-16 bit          | No            | Various            | No            | Extensive  | Yes          | Limited         |

### Format Compression Characteristics

| Format            | Compression Ratio | Quality Loss | Encode Speed | Decode Speed | Memory Usage |
|-------------------|-------------------|--------------|--------------|--------------|--------------|
| **JPEG (Q=90)**   | 10:1              | Minimal      | Fast         | Very Fast    | Low          |
| **JPEG (Q=75)**   | 20:1              | Noticeable   | Fast         | Very Fast    | Low          |
| **JPEG 2000**     | 20:1              | Minimal      | Slow         | Moderate     | High         |
| **PNG**           | 2-3:1             | None         | Moderate     | Fast         | Moderate     |
| **WebP Lossy**    | 25-35:1           | Minimal      | Moderate     | Fast         | Low          |
| **WebP Lossless** | 2:1               | None         | Slow         | Fast         | Moderate     |
| **AVIF**          | 30-40:1           | Minimal      | Very Slow    | Slow         | High         |
| **HEIF**          | 2x JPEG           | Minimal      | Slow         | Moderate     | Moderate     |

## Color Space Support Matrix

### Format Color Space Capabilities

| Format   | sRGB | Adobe RGB | ProPhoto RGB | P3 | Rec. 2020 | LAB | CMYK | HDR |
|----------|------|-----------|--------------|----|-----------|-----|------|-----|
| **JPEG** | ✓    | ✓         | ✓            | ✓  | ✓         | ✓¹  | ✓    | ✗   |
| **PNG**  | ✓    | ✓         | ✓            | ✓  | ✓         | ✗   | ✗    | ✗   |
| **WebP** | ✓    | ✓²        | ✓²           | ✓² | ✗         | ✗   | ✗    | ✗   |
| **AVIF** | ✓    | ✓         | ✓            | ✓  | ✓         | ✗   | ✗    | ✓   |
| **HEIF** | ✓    | ✓         | ✓            | ✓  | ✓         | ✗   | ✗    | ✓   |
| **TIFF** | ✓    | ✓         | ✓            | ✓  | ✓         | ✓   | ✓    | ✓³  |
| **EXR**  | ✓    | ✓         | ✓            | ✓  | ✓         | ✗   | ✗    | ✓   |
| **DDS**  | ✓    | ✗         | ✗            | ✗  | ✗         | ✗   | ✗    | ✓⁴  |

¹ Through ICC profile
² With ICC profile embedding
³ 32-bit float support
⁴ BC6H format for HDR

### Color Depth and Precision

| Bit Depth        | Formats Supporting    | Colors     | Dynamic Range | Use Cases                     |
|------------------|-----------------------|------------|---------------|-------------------------------|
| **8-bit**        | All formats           | 16.7M      | 256:1         | Web, general photography      |
| **10-bit**       | HEIF, AVIF, some TIFF | 1.07B      | 1024:1        | Professional photo, HDR video |
| **12-bit**       | RAW, AVIF, TIFF       | 68.7B      | 4096:1        | Cinema, high-end photography  |
| **16-bit**       | PNG, TIFF, PSD        | 281T       | 65536:1       | Professional editing, medical |
| **16-bit float** | EXR, TIFF             | Continuous | 10⁹:1         | VFX, HDR imaging              |
| **32-bit float** | EXR, TIFF             | Continuous | 10³⁸:1        | Scientific, extreme HDR       |

## Platform and Application Support

### Operating System Native Support

| Format   | Windows 11 | macOS 13+ | Ubuntu 22.04 | Android 13 | iOS 16     |
|----------|------------|-----------|--------------|------------|------------|
| **JPEG** | Native     | Native    | Native       | Native     | Native     |
| **PNG**  | Native     | Native    | Native       | Native     | Native     |
| **WebP** | Native     | Native    | Package      | Native     | Native     |
| **AVIF** | Native¹    | Native    | Package      | Native     | Safari 16+ |
| **HEIF** | Native     | Native    | Package²     | Limited    | Native     |
| **BMP**  | Native     | Native    | Native       | Native     | View only  |
| **TIFF** | Native     | Native    | Native       | Limited    | Limited    |
| **GIF**  | Native     | Native    | Native       | Native     | Native     |
| **RAW**  | WIC Codecs | Native    | dcraw        | Limited    | Limited    |

¹ With codec pack
² Requires additional libraries

### Browser Compatibility (2024)

| Format      | Chrome 120 | Firefox 120 | Safari 17 | Edge 120 | Support Level |
|-------------|------------|-------------|-----------|----------|---------------|
| **JPEG**    | ✓          | ✓           | ✓         | ✓        | Universal     |
| **PNG**     | ✓          | ✓           | ✓         | ✓        | Universal     |
| **WebP**    | ✓          | ✓           | ✓         | ✓        | Universal     |
| **AVIF**    | ✓          | ✓           | ✓¹        | ✓        | Growing       |
| **GIF**     | ✓          | ✓           | ✓         | ✓        | Universal     |
| **SVG**     | ✓          | ✓           | ✓         | ✓        | Universal     |
| **JPEG XL** | Flag²      | ✗           | ✗         | Flag²    | Experimental  |
| **HEIF**    | ✗          | ✗           | ✓         | ✗        | Limited       |

¹ macOS/iOS only
² Behind experimental flag

## Conversion Compatibility

### Lossless Conversion Paths

| From → To         | PNG | TIFF | BMP | WebP Lossless | Comments         |
|-------------------|-----|------|-----|---------------|------------------|
| **PNG**           | —   | ✓    | ✓   | ✓             | Full fidelity    |
| **TIFF**          | ✓¹  | —    | ✓¹  | ✓¹            | ¹If uncompressed |
| **BMP**           | ✓   | ✓    | —   | ✓             | No compression   |
| **WebP Lossless** | ✓   | ✓    | ✓   | —             | Full fidelity    |
| **GIF**           | ✓²  | ✓    | ✓   | ✓             | ²Indexed color   |

### Quality Retention Guidelines

| Source Format | Target Format | Quality Retention | Recommended Settings   |
|---------------|---------------|-------------------|------------------------|
| **RAW**       | TIFF          | 100%              | 16-bit, ProPhoto RGB   |
| **RAW**       | JPEG          | 85-90%            | Quality 95+, Adobe RGB |
| **PNG**       | JPEG          | 85-95%            | Quality 90+, 4:4:4     |
| **JPEG**      | PNG           | 100%¹             | ¹But larger file       |
| **JPEG**      | WebP          | 95-98%            | Quality 85+            |
| **TIFF**      | JPEG          | 85-95%            | Quality 95+            |
| **EXR**       | PNG           | 60-70%            | 16-bit PNG             |
| **HEIF**      | JPEG          | 90-95%            | Quality 90+            |

## Performance Characteristics

### Decode Performance Comparison

| Format         | Relative Speed | Memory Usage | CPU Usage | GPU Accelerated |
|----------------|----------------|--------------|-----------|-----------------|
| **BMP**        | 100 (baseline) | Low          | Minimal   | No              |
| **JPEG**       | 95             | Low          | Low       | Yes             |
| **PNG**        | 70             | Moderate     | Moderate  | Partial         |
| **WebP**       | 80             | Low          | Moderate  | Yes             |
| **AVIF**       | 20             | High         | Very High | Limited         |
| **HEIF**       | 40             | Moderate     | High      | Yes (Apple)     |
| **TIFF (LZW)** | 60             | High         | Moderate  | No              |
| **JPEG 2000**  | 30             | High         | High      | Limited         |

### Encode Performance Comparison

| Format   | Relative Speed | Quality/Size | Parallelizable | Hardware Encode |
|----------|----------------|--------------|----------------|-----------------|
| **BMP**  | 100 (baseline) | Poor         | Yes            | No              |
| **JPEG** | 85             | Good         | Partial        | Yes             |
| **PNG**  | 40             | Good         | Limited        | No              |
| **WebP** | 50             | Excellent    | Yes            | Limited         |
| **AVIF** | 5              | Best         | Yes            | Emerging        |
| **HEIF** | 20             | Excellent    | Yes            | Yes (Apple)     |

## Advanced Format Features

### Metadata Capabilities

| Format   | EXIF       | XMP        | IPTC       | Custom      | Thumbnail | Color Profile |
|----------|------------|------------|------------|-------------|-----------|---------------|
| **JPEG** | Full       | Full       | Full       | APP markers | Yes       | Yes           |
| **PNG**  | Via chunks | Via chunks | Via chunks | tEXt chunks | No        | Yes           |
| **WebP** | Full       | Full       | No         | Limited     | Yes       | Yes           |
| **TIFF** | Full       | Full       | Full       | IFD tags    | Yes       | Yes           |
| **HEIF** | Full       | Full       | Limited    | Yes         | Yes       | Yes           |
| **AVIF** | Full       | Full       | No         | Limited     | Yes       | Yes           |

### Animation Support

| Format   | Animation | Max Frames | Frame Rate | Compression | Alpha  | Use Cases         |
|----------|-----------|------------|------------|-------------|--------|-------------------|
| **GIF**  | Yes       | No limit   | Variable   | LZW         | Binary | Simple animations |
| **WebP** | Yes       | No limit   | Variable   | VP8         | Full   | Modern web        |
| **AVIF** | Yes       | No limit   | Variable   | AV1         | Full   | Next-gen web      |
| **HEIF** | Yes       | No limit   | Variable   | HEVC        | Full   | iOS ecosystem     |
| **APNG** | Yes       | No limit   | Variable   | DEFLATE     | Full   | Fallback to PNG   |

## API and Library Support

### Major Graphics Libraries

| Library            | JPEG | PNG | WebP | AVIF | HEIF | TIFF | EXR | DDS |
|--------------------|------|-----|------|------|------|------|-----|-----|
| **ImageSharp**     | ✓    | ✓   | ✓    | ✓    | ✗    | ✓    | ✗   | ✗   |
| **SkiaSharp**      | ✓    | ✓   | ✓    | ✗    | ✓¹   | ✗    | ✗   | ✗   |
| **System.Drawing** | ✓    | ✓   | ✗    | ✗    | ✗    | ✓    | ✗   | ✗   |
| **Magick.NET**     | ✓    | ✓   | ✓    | ✓    | ✓    | ✓    | ✓   | ✓   |
| **FreeImage**      | ✓    | ✓   | ✓    | ✗    | ✗    | ✓    | ✓   | ✓   |

¹ Platform-dependent

### Codec Availability

| Format   | Windows Codec | macOS Codec | Linux Codec | Android Codec |
|----------|---------------|-------------|-------------|---------------|
| **JPEG** | Built-in      | Built-in    | libjpeg     | Built-in      |
| **PNG**  | Built-in      | Built-in    | libpng      | Built-in      |
| **WebP** | WIC           | Built-in    | libwebp     | Built-in      |
| **AVIF** | AV1 codec     | Built-in    | libavif     | Built-in      |
| **HEIF** | HEVC codec    | Built-in    | libheif     | Limited       |

## Format Selection Guidelines

### Use Case Recommendations

| Use Case          | Primary Format | Alternative | Avoid | Reasoning            |
|-------------------|----------------|-------------|-------|----------------------|
| **Web Photos**    | WebP           | JPEG        | BMP   | Size/quality balance |
| **Web Graphics**  | PNG            | WebP        | GIF¹  | Transparency support |
| **Photography**   | JPEG           | HEIF        | GIF   | Color depth          |
| **Print**         | TIFF           | PNG         | JPEG  | Lossless quality     |
| **HDR Content**   | EXR            | HEIF        | JPEG  | Dynamic range        |
| **Game Textures** | DDS            | PNG         | JPEG  | GPU optimization     |
| **Archives**      | TIFF           | PNG         | JPEG  | Preservation         |
| **Social Media**  | JPEG           | WebP        | TIFF  | Compatibility        |

¹ Except for simple animations

### Decision Matrix

| Requirement        | Best Format | Good Alternative | Acceptable |
|--------------------|-------------|------------------|------------|
| **Smallest Size**  | AVIF        | WebP             | JPEG       |
| **Fastest Decode** | BMP         | JPEG             | PNG        |
| **Best Quality**   | EXR         | TIFF             | PNG        |
| **Wide Support**   | JPEG        | PNG              | GIF        |
| **Future Proof**   | AVIF        | WebP             | HEIF       |
| **Transparency**   | PNG         | WebP             | AVIF       |
| **Animation**      | WebP        | AVIF             | GIF        |
| **Metadata Rich**  | TIFF        | JPEG             | HEIF       |

## Conversion Code Examples

### Safe Format Conversion

```csharp
public static class FormatConverter
{
    public static ConversionResult Convert(
        string sourcePath,
        string targetFormat,
        ConversionOptions options = null)
    {
        options ??= ConversionOptions.Default;

        var sourceFormat = DetectFormat(sourcePath);
        var compatibility = GetCompatibility(sourceFormat, targetFormat);

        if (compatibility.QualityLoss > options.MaxQualityLoss)
        {
            return new ConversionResult
            {
                Success = false,
                Error = $"Conversion would result in {compatibility.QualityLoss}% quality loss"
            };
        }

        // Proceed with conversion using appropriate settings
        var settings = GetOptimalSettings(sourceFormat, targetFormat, options);
        return PerformConversion(sourcePath, targetFormat, settings);
    }

    private static FormatCompatibility GetCompatibility(
        string source,
        string target)
    {
        // Use compatibility matrix to determine conversion characteristics
        return CompatibilityMatrix[source][target];
    }
}
```

## Summary

This format compatibility matrix serves as a comprehensive reference for making informed decisions about image format
selection and conversion. Key considerations include:

1. **Format Selection**: Choose formats based on specific requirements balancing size, quality, and compatibility
2. **Conversion Planning**: Understand quality implications when converting between formats
3. **Platform Considerations**: Account for varying support across different platforms and browsers
4. **Performance Trade-offs**: Balance encoding/decoding speed with file size and quality
5. **Future Compatibility**: Consider emerging formats like AVIF for future-proofing

Regular updates to this matrix ensure alignment with evolving standards and platform capabilities.
