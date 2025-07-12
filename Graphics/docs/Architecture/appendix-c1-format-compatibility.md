# Appendix C.1: Format Compatibility Matrix

## Introduction

This comprehensive reference provides detailed compatibility information for image and video formats across different
libraries, platforms, and graphics APIs. Understanding format compatibility is crucial for building robust graphics
processing systems that work reliably across diverse environments.

## Image Format Compatibility

### Core .NET Format Support

| Format          | System.Drawing | ImageSharp | SkiaSharp  | Windows Imaging | Native Support |
|-----------------|----------------|------------|------------|-----------------|----------------|
| **JPEG/JPG**    | ✅ Full         | ✅ Full     | ✅ Full     | ✅ Full          | All platforms  |
| **PNG**         | ✅ Full         | ✅ Full     | ✅ Full     | ✅ Full          | All platforms  |
| **BMP**         | ✅ Full         | ✅ Full     | ✅ Full     | ✅ Full          | All platforms  |
| **GIF**         | ✅ Full         | ✅ Full     | ✅ Full     | ✅ Full          | All platforms  |
| **TIFF**        | ✅ Full         | ✅ Full     | ✅ Full     | ✅ Full          | All platforms  |
| **WebP**        | ❌ No           | ✅ Full     | ✅ Full     | ✅ Win10+        | Limited        |
| **HEIF/HEIC**   | ❌ No           | ⚠️ Plugin  | ⚠️ Limited | ✅ Win10+        | iOS/macOS      |
| **AVIF**        | ❌ No           | ✅ 3.0+     | ⚠️ Limited | ❌ No            | Browser only   |
| **ICO**         | ✅ Full         | ✅ Full     | ✅ Full     | ✅ Full          | Windows        |
| **TGA**         | ❌ No           | ✅ Full     | ✅ Full     | ❌ No            | Legacy         |
| **PBM/PGM/PPM** | ❌ No           | ✅ Full     | ⚠️ Limited | ❌ No            | Unix           |
| **EXR**         | ❌ No           | ❌ No       | ⚠️ Plugin  | ❌ No            | Professional   |
| **DDS**         | ❌ No           | ⚠️ Plugin  | ❌ No       | ✅ DirectX       | Gaming         |
| **PSD**         | ❌ No           | ⚠️ Basic   | ⚠️ Limited | ❌ No            | Adobe only     |

**Legend:**

- ✅ Full: Complete support with all features
- ⚠️ Limited/Plugin: Partial support or requires additional packages
- ❌ No: Not supported

### Pixel Format Support by Library

| Pixel Format   | Bits | ImageSharp | SkiaSharp | System.Drawing | GPU Compatible |
|----------------|------|------------|-----------|----------------|----------------|
| **A8**         | 8    | ✅          | ✅         | ❌              | ✅              |
| **L8**         | 8    | ✅          | ✅         | ⚠️             | ✅              |
| **L16**        | 16   | ✅          | ❌         | ❌              | ✅              |
| **La16**       | 16   | ✅          | ❌         | ❌              | ✅              |
| **La32**       | 32   | ✅          | ❌         | ❌              | ✅              |
| **Rgb24**      | 24   | ✅          | ✅         | ✅              | ⚠️             |
| **Rgba32**     | 32   | ✅          | ✅         | ✅              | ✅              |
| **Argb32**     | 32   | ✅          | ✅         | ✅              | ✅              |
| **Bgr24**      | 24   | ✅          | ✅         | ✅              | ⚠️             |
| **Bgra32**     | 32   | ✅          | ✅         | ✅              | ✅              |
| **Rgb48**      | 48   | ✅          | ❌         | ❌              | ⚠️             |
| **Rgba64**     | 64   | ✅          | ❌         | ⚠️             | ✅              |
| **RgbaVector** | 128  | ✅          | ❌         | ❌              | ✅              |
| **Indexed8**   | 8    | ✅          | ✅         | ✅              | ⚠️             |

### Platform-Specific Format Support

| Format   | Windows  | Linux      | macOS        | iOS        | Android  | Web         |
|----------|----------|------------|--------------|------------|----------|-------------|
| **JPEG** | ✅ Native | ✅ libjpeg  | ✅ Native     | ✅ Native   | ✅ Native | ✅ Native    |
| **PNG**  | ✅ Native | ✅ libpng   | ✅ Native     | ✅ Native   | ✅ Native | ✅ Native    |
| **WebP** | ✅ Win10+ | ✅ libwebp  | ✅ Safari 14+ | ✅ iOS 14+  | ✅ 4.0+   | ✅ Modern    |
| **HEIF** | ✅ Win10+ | ⚠️ libheif | ✅ Native     | ✅ Native   | ⚠️ 9.0+  | ⚠️ Limited  |
| **AVIF** | ⚠️ App   | ✅ libavif  | ✅ macOS 11+  | ✅ iOS 14+  | ⚠️ 12+   | ✅ Chrome/FF |
| **Raw**  | ⚠️ WIC   | ✅ libraw   | ✅ Native     | ⚠️ Limited | ❌ No     | ❌ No        |

## GPU Format Compatibility

### DirectX Format Support

| Format                 | DX11 | DX12 | Feature Level | Common Use         |
|------------------------|------|------|---------------|--------------------|
| **R8G8B8A8_UNORM**     | ✅    | ✅    | 9.1+          | General purpose    |
| **B8G8R8A8_UNORM**     | ✅    | ✅    | 9.1+          | Display/swap chain |
| **R16G16B16A16_FLOAT** | ✅    | ✅    | 10.0+         | HDR rendering      |
| **R32G32B32A32_FLOAT** | ✅    | ✅    | 10.0+         | Compute            |
| **R10G10B10A2_UNORM**  | ✅    | ✅    | 10.0+         | HDR display        |
| **R11G11B10_FLOAT**    | ✅    | ✅    | 10.0+         | HDR storage        |
| **BC1_UNORM**          | ✅    | ✅    | 9.1+          | Compressed RGB     |
| **BC3_UNORM**          | ✅    | ✅    | 9.1+          | Compressed RGBA    |
| **BC5_UNORM**          | ✅    | ✅    | 10.0+         | Normal maps        |
| **BC7_UNORM**          | ✅    | ✅    | 11.0+         | High quality       |

### Vulkan Format Support

| Format                                | Required | Optimal | Linear | Storage | Features  |
|---------------------------------------|----------|---------|--------|---------|-----------|
| **VK_FORMAT_R8G8B8A8_UNORM**          | ✅        | ✅       | ✅      | ✅       | Universal |
| **VK_FORMAT_B8G8R8A8_UNORM**          | ✅        | ✅       | ✅      | ✅       | Swapchain |
| **VK_FORMAT_R8G8B8A8_SRGB**           | ✅        | ✅       | ⚠️     | ✅       | sRGB      |
| **VK_FORMAT_R16G16B16A16_SFLOAT**     | ⚠️       | ✅       | ⚠️     | ✅       | HDR       |
| **VK_FORMAT_R32G32B32A32_SFLOAT**     | ⚠️       | ✅       | ❌      | ✅       | Compute   |
| **VK_FORMAT_BC1_RGB_UNORM_BLOCK**     | ⚠️       | ✅       | ❌      | ❌       | Desktop   |
| **VK_FORMAT_ETC2_R8G8B8_UNORM_BLOCK** | ⚠️       | ✅       | ❌      | ❌       | Mobile    |
| **VK_FORMAT_ASTC_4x4_UNORM_BLOCK**    | ⚠️       | ✅       | ❌      | ❌       | Mobile    |

### OpenGL Format Compatibility

| Internal Format                  | Type  | Normalized | Filterable | Renderable | Version |
|----------------------------------|-------|------------|------------|------------|---------|
| **GL_RGBA8**                     | UNORM | Yes        | ✅          | ✅          | 1.0     |
| **GL_RGB8**                      | UNORM | Yes        | ✅          | ✅          | 1.0     |
| **GL_RGBA16F**                   | FLOAT | No         | ✅          | ✅          | 3.0     |
| **GL_RGBA32F**                   | FLOAT | No         | ⚠️         | ✅          | 3.0     |
| **GL_R11F_G11F_B10F**            | FLOAT | No         | ✅          | ✅          | 3.0     |
| **GL_RGB10_A2**                  | UNORM | Yes        | ✅          | ✅          | 3.3     |
| **GL_SRGB8_ALPHA8**              | UNORM | Yes        | ✅          | ✅          | 2.1     |
| **GL_COMPRESSED_RGBA_S3TC_DXT5** | UNORM | Yes        | ✅          | ❌          | EXT     |

## Video Format Compatibility

### Container Format Support

| Container   | FFmpeg | MediaFoundation | AVFoundation | Hardware Decode | Streaming   |
|-------------|--------|-----------------|--------------|-----------------|-------------|
| **MP4**     | ✅      | ✅               | ✅            | ✅ All           | ✅ HLS/DASH  |
| **MKV**     | ✅      | ⚠️              | ⚠️           | ⚠️ Limited      | ⚠️ Limited  |
| **WebM**    | ✅      | ✅ Win10+        | ⚠️           | ⚠️ VP9 only     | ✅ Native    |
| **AVI**     | ✅      | ✅               | ⚠️           | ❌               | ❌ Legacy    |
| **MOV**     | ✅      | ✅               | ✅            | ✅               | ✅ HLS       |
| **FLV**     | ✅      | ⚠️              | ❌            | ❌               | ⚠️ RTMP     |
| **MPEG-TS** | ✅      | ✅               | ✅            | ✅               | ✅ Broadcast |

### Video Codec Support

| Codec          | Encode Support | Decode Support | Hardware Accel | Platforms |
|----------------|----------------|----------------|----------------|-----------|
| **H.264/AVC**  | ✅ Universal    | ✅ Universal    | ✅ All GPUs     | All       |
| **H.265/HEVC** | ✅ Most         | ✅ Most         | ✅ Modern GPUs  | Most      |
| **VP8**        | ✅ Software     | ✅ Universal    | ⚠️ Limited     | All       |
| **VP9**        | ✅ Software     | ✅ Modern       | ✅ Intel/AMD    | Modern    |
| **AV1**        | ⚠️ Slow        | ✅ Growing      | ✅ Latest GPUs  | Latest    |
| **ProRes**     | ⚠️ macOS       | ✅              | ✅ macOS        | Apple     |
| **DNxHD/HR**   | ✅ FFmpeg       | ✅              | ❌              | Pro       |

## Color Space Compatibility

### Color Space Support by Library

| Color Space    | ImageSharp | SkiaSharp | DirectX | OpenGL | Metal |
|----------------|------------|-----------|---------|--------|-------|
| **sRGB**       | ✅          | ✅         | ✅       | ✅      | ✅     |
| **Linear RGB** | ✅          | ✅         | ✅       | ✅      | ✅     |
| **Display P3** | ⚠️         | ✅         | ✅ 11+   | ⚠️     | ✅     |
| **Adobe RGB**  | ⚠️         | ⚠️        | ❌       | ❌      | ⚠️    |
| **Rec. 709**   | ✅          | ✅         | ✅       | ✅      | ✅     |
| **Rec. 2020**  | ⚠️         | ⚠️        | ✅ 11+   | ⚠️     | ✅     |
| **DCI-P3**     | ⚠️         | ✅         | ✅ 11+   | ⚠️     | ✅     |
| **Lab**        | ✅          | ❌         | ❌       | ❌      | ❌     |
| **HSV/HSL**    | ✅          | ✅         | ❌       | ❌      | ❌     |

### HDR Format Support

| Format           | Bits  | Windows  | macOS | Linux | Mobile    | Use Case        |
|------------------|-------|----------|-------|-------|-----------|-----------------|
| **HDR10**        | 10    | ✅ Win10+ | ✅     | ⚠️    | ✅ Latest  | Video/Gaming    |
| **HDR10+**       | 10    | ✅ Win11  | ❌     | ❌     | ✅ Samsung | Dynamic HDR     |
| **Dolby Vision** | 12    | ✅ Win10+ | ✅ TV  | ❌     | ✅ Premium | Premium content |
| **HLG**          | 10    | ✅        | ✅     | ⚠️    | ✅         | Broadcast       |
| **PQ (ST.2084)** | 10-12 | ✅        | ✅     | ⚠️    | ✅         | Mastering       |

## Compression Format Compatibility

### Texture Compression Formats

| Format       | Desktop GPU | Mobile GPU | Quality  | Compression | Alpha        |
|--------------|-------------|------------|----------|-------------|--------------|
| **BC1/DXT1** | ✅           | ❌          | Low      | 6:1         | 1-bit        |
| **BC2/DXT3** | ✅           | ❌          | Medium   | 4:1         | Explicit     |
| **BC3/DXT5** | ✅           | ❌          | Medium   | 4:1         | Interpolated |
| **BC4**      | ✅           | ❌          | High     | 2:1         | No           |
| **BC5**      | ✅           | ❌          | High     | 2:1         | No           |
| **BC6H**     | ✅ DX11+     | ❌          | High     | 6:1         | No           |
| **BC7**      | ✅ DX11+     | ❌          | Highest  | 3:1         | Yes          |
| **ETC1**     | ⚠️          | ✅          | Low      | 6:1         | No           |
| **ETC2**     | ⚠️          | ✅          | Medium   | 6:1         | Yes          |
| **PVRTC**    | ❌           | ✅ iOS      | Low      | 6:1/8:1     | Yes          |
| **ASTC**     | ⚠️          | ✅ Modern   | Variable | Variable    | Yes          |

## Metadata Format Support

### EXIF/Metadata Support

| Metadata Type | JPEG | PNG | TIFF | WebP | HEIF | Raw |
|---------------|------|-----|------|------|------|-----|
| **EXIF**      | ✅    | ⚠️  | ✅    | ✅    | ✅    | ✅   |
