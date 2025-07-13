# Chapter 4: Image Representation and Data Structures

The foundation of any graphics processing system lies in how it represents and manages image data. This chapter explores
the intricate details of pixel formats, memory layouts, buffer management strategies, and metadata architectures that
enable high-performance image processing in .NET 9.0. Understanding these fundamentals is crucial for building efficient
graphics applications that can handle diverse image formats while maintaining optimal performance characteristics.

## 4.1 Pixel Formats and Color Spaces

### Understanding pixel representation fundamentals

At its core, a digital image consists of a two-dimensional array of pixels, where each pixel contains color information
encoded in a specific format. The choice of pixel format fundamentally impacts memory usage, processing performance, and
color fidelity. In .NET graphics processing, pixel formats determine how color data is stored, accessed, and manipulated
throughout the processing pipeline.

The most basic pixel format is **8-bit grayscale**, where each pixel occupies a single byte representing 256 possible
intensity levels. This format offers simplicity and minimal memory usage but lacks color information. Moving to color
representations, **RGB24** uses three bytes per pixel for red, green, and blue channels, providing 16.7 million possible
colors. However, modern graphics systems typically use **RGBA32**, adding an alpha channel for transparency, which
aligns naturally with 32-bit memory boundaries and improves processing efficiency through better memory alignment.

### Memory layout patterns and their performance implications

The arrangement of pixel data in memory significantly affects processing performance. **Packed pixel formats** store all
channels of a pixel contiguously, optimizing spatial locality for per-pixel operations. In contrast, **planar formats**
separate each channel into its own continuous memory region, enabling efficient channel-specific processing and better
SIMD utilization.

```csharp
// Packed pixel format (RGBA32) - channels interleaved
public struct Rgba32
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    // Efficient for per-pixel operations
    public static Rgba32 Blend(Rgba32 source, Rgba32 dest)
    {
        float alpha = source.A / 255f;
        return new Rgba32
        {
            R = (byte)(source.R * alpha + dest.R * (1 - alpha)),
            G = (byte)(source.G * alpha + dest.G * (1 - alpha)),
            B = (byte)(source.B * alpha + dest.B * (1 - alpha)),
            A = 255
        };
    }
}

// Planar format representation - channels separated
public class PlanarImage
{
    public byte[] RedChannel;
    public byte[] GreenChannel;
    public byte[] BlueChannel;
    public byte[] AlphaChannel;

    // Efficient for channel-specific operations
    public void ApplyGammaToRed(float gamma)
    {
        var lookup = BuildGammaLookup(gamma);

        // Process entire red channel with SIMD
        int vectorSize = Vector256<byte>.Count;
        for (int i = 0; i <= RedChannel.Length - vectorSize; i += vectorSize)
        {
            var vector = Vector256.Load(RedChannel.AsSpan(i));
            // Apply lookup table using gather operations
            var processed = GatherLookup(vector, lookup);
            processed.Store(RedChannel.AsSpan(i));
        }
    }
}
```

### Extended pixel formats for specialized applications

Beyond standard 8-bit per channel formats, modern graphics applications increasingly require extended precision. *
*16-bit per channel formats** (48-bit RGB or 64-bit RGBA) provide greater color depth for professional photography and
medical imaging. These formats prevent banding artifacts in gradients and preserve detail during intensive processing
operations.

**High Dynamic Range (HDR) formats** use floating-point representations to capture a wider range of luminance values.
The **scRGBA** format employs 32-bit floats per channel, enabling representation of colors outside the traditional [0,1]
range. This capability is essential for modern displays supporting HDR10+ and Dolby Vision standards.

```csharp
// HDR pixel format using half-precision floats
[StructLayout(LayoutKind.Sequential)]
public struct RgbaHalf
{
    public Half R;
    public Half G;
    public Half B;
    public Half A;

    // Convert from standard dynamic range with tone mapping
    public static RgbaHalf FromSDR(Rgba32 sdr, float exposure)
    {
        const float inv255 = 1f / 255f;

        // Apply exposure adjustment in linear space
        float r = GammaToLinear(sdr.R * inv255) * exposure;
        float g = GammaToLinear(sdr.G * inv255) * exposure;
        float b = GammaToLinear(sdr.B * inv255) * exposure;

        return new RgbaHalf
        {
            R = (Half)r,
            G = (Half)g,
            B = (Half)b,
            A = (Half)(sdr.A * inv255)
        };
    }

    private static float GammaToLinear(float value)
    {
        return value <= 0.04045f
            ? value / 12.92f
            : MathF.Pow((value + 0.055f) / 1.055f, 2.4f);
    }
}
```

### Color space considerations in pixel format design

Pixel formats are intrinsically linked to color spaces, which define how numeric values map to perceived colors. The
ubiquitous **sRGB color space** uses a non-linear gamma curve approximating human perception, making it suitable for
display devices but complicating mathematical operations. Linear color spaces simplify calculations but require
conversion for display.

**Wide gamut color spaces** like Adobe RGB and Display P3 require careful handling to prevent color shifts. When
processing images in these spaces, maintaining the original color space throughout the pipeline prevents unintended
saturation loss or hue shifts. The .NET ecosystem provides comprehensive color management through libraries like
ImageSharp, which implements ICC profile support for accurate color space conversions.

```csharp
// Color space aware pixel operations
public class ColorManagedImage
{
    private readonly IColorSpace _colorSpace;
    private readonly float[] _pixelData; // Linear light values

    public void ConvertToColorSpace(IColorSpace targetSpace)
    {
        // Build conversion matrix from source to CIE XYZ to target
        var toXYZ = _colorSpace.GetRGBToXYZMatrix();
        var fromXYZ = targetSpace.GetXYZToRGBMatrix();
        var conversion = Matrix4x4.Multiply(fromXYZ, toXYZ);

        // Apply conversion to all pixels
        Parallel.For(0, _pixelData.Length / 3, i =>
        {
            int idx = i * 3;
            var rgb = new Vector3(
                _pixelData[idx],
                _pixelData[idx + 1],
                _pixelData[idx + 2]);

            var converted = Vector3.Transform(rgb, conversion);

            _pixelData[idx] = converted.X;
            _pixelData[idx + 1] = converted.Y;
            _pixelData[idx + 2] = converted.Z;
        });

        _colorSpace = targetSpace;
    }
}
```

### Specialized formats for performance optimization

Graphics applications often employ specialized pixel formats optimized for specific use cases. **Indexed color formats**
use a palette to reduce memory usage, particularly effective for images with limited color ranges. **YCbCr formats**
separate luminance from chrominance, enabling subsampling strategies that reduce data size while preserving perceived
quality.

**GPU-friendly formats** like BC7 (Block Compression 7) provide hardware-accelerated decompression, reducing memory
bandwidth requirements. These formats are particularly valuable for texture-heavy applications like games and
visualization systems. Modern .NET libraries expose these formats through abstractions that handle platform-specific
details while maintaining cross-platform compatibility.

## 4.2 Image Buffer Management

### Buffer allocation strategies for different scenarios

Efficient image buffer management forms the cornerstone of high-performance graphics applications. The allocation
strategy must balance memory usage, access patterns, and lifetime requirements. **Contiguous buffers** provide optimal
cache performance for sequential processing but may struggle with large images due to memory fragmentation. **Chunked
buffers** divide images into smaller blocks, enabling better memory utilization and supporting images larger than
available contiguous memory.

```csharp
// Adaptive buffer allocation strategy
public class AdaptiveImageBuffer
{
    private const int ChunkThreshold = 16 * 1024 * 1024; // 16MB
    private const int ChunkSize = 4 * 1024 * 1024; // 4MB chunks

    public IImageBuffer AllocateBuffer(int width, int height, int bytesPerPixel)
    {
        long totalSize = (long)width * height * bytesPerPixel;

        if (totalSize <= ChunkThreshold)
        {
            // Use contiguous buffer for smaller images
            return new ContiguousImageBuffer(width, height, bytesPerPixel);
        }
        else
        {
            // Use chunked buffer for larger images
            int rowsPerChunk = ChunkSize / (width * bytesPerPixel);
            return new ChunkedImageBuffer(width, height, bytesPerPixel, rowsPerChunk);
        }
    }
}

// Contiguous buffer implementation
public class ContiguousImageBuffer : IImageBuffer
{
    private readonly Memory<byte> _buffer;
    private readonly int _stride;

    public ContiguousImageBuffer(int width, int height, int bytesPerPixel)
    {
        _stride = width * bytesPerPixel;
        var array = ArrayPool<byte>.Shared.Rent(_stride * height);
        _buffer = new Memory<byte>(array, 0, _stride * height);
    }

    public Span<byte> GetRowSpan(int y)
    {
        int offset = y * _stride;
        return _buffer.Span.Slice(offset, _stride);
    }
}

// Chunked buffer for large images
public class ChunkedImageBuffer : IImageBuffer
{
    private readonly List<Memory<byte>> _chunks;
    private readonly int _rowsPerChunk;
    private readonly int _stride;

    public Span<byte> GetRowSpan(int y)
    {
        int chunkIndex = y / _rowsPerChunk;
        int rowInChunk = y % _rowsPerChunk;
        int offset = rowInChunk * _stride;

        return _chunks[chunkIndex].Span.Slice(offset, _stride);
    }
}
```

### Memory pooling for buffer reuse

Buffer pooling dramatically reduces allocation overhead and GC pressure in graphics applications. The strategy involves
maintaining pools of pre-allocated buffers that can be rented and returned, amortizing allocation costs across multiple
operations. **Size-bucketed pools** organize buffers by size ranges, preventing excessive memory waste from oversized
allocations.

```csharp
// High-performance image buffer pool
public class ImageBufferPool
{
    private readonly ConcurrentBag<PooledBuffer>[] _buckets;
    private readonly int[] _bucketSizes;
    private long _totalPooledMemory;
    private readonly long _maxPoolMemory;

    public ImageBufferPool(long maxPoolMemory = 256 * 1024 * 1024) // 256MB default
    {
        _maxPoolMemory = maxPoolMemory;

        // Define bucket sizes (powers of 2 for efficiency)
        _bucketSizes = new[]
        {
            64 * 1024,      // 64KB
            256 * 1024,     // 256KB
            1024 * 1024,    // 1MB
            4 * 1024 * 1024,  // 4MB
            16 * 1024 * 1024  // 16MB
        };

        _buckets = new ConcurrentBag<PooledBuffer>[_bucketSizes.Length];
        for (int i = 0; i < _buckets.Length; i++)
        {
            _buckets[i] = new ConcurrentBag<PooledBuffer>();
        }
    }

    public PooledImageBuffer Rent(int minimumSize)
    {
        int bucketIndex = GetBucketIndex(minimumSize);

        // Try to get from pool
        if (bucketIndex < _buckets.Length &&
            _buckets[bucketIndex].TryTake(out var pooled))
        {
            Interlocked.Add(ref _totalPooledMemory, -pooled.Length);
            return new PooledImageBuffer(pooled.Buffer, pooled.Length, this);
        }

        // Allocate new buffer
        int size = bucketIndex < _bucketSizes.Length
            ? _bucketSizes[bucketIndex]
            : minimumSize;

        var buffer = GC.AllocateUninitializedArray<byte>(size, pinned: true);
        return new PooledImageBuffer(buffer, size, this);
    }

    internal void Return(byte[] buffer, int length)
    {
        // Don't pool if it would exceed memory limit
        if (Interlocked.Read(ref _totalPooledMemory) + length > _maxPoolMemory)
        {
            return;
        }

        int bucketIndex = GetBucketIndex(length);
        if (bucketIndex < _buckets.Length)
        {
            _buckets[bucketIndex].Add(new PooledBuffer(buffer, length));
            Interlocked.Add(ref _totalPooledMemory, length);
        }
    }
}
```

### Stride alignment and padding considerations

Memory alignment significantly impacts processing performance, particularly for SIMD operations. **Stride alignment**
ensures each row begins at an address suitable for vectorized operations. Common alignment requirements include 16-byte
alignment for SSE operations, 32-byte alignment for AVX, and 64-byte alignment for cache line optimization.

```csharp
// Stride calculation with alignment
public static class StrideCalculator
{
    public static int CalculateStride(int width, int bytesPerPixel, int alignment = 32)
    {
        int minStride = width * bytesPerPixel;

        // Round up to nearest multiple of alignment
        int padding = (alignment - (minStride % alignment)) % alignment;
        return minStride + padding;
    }

    // Optimized stride for SIMD operations
    public static int CalculateSimdOptimalStride<T>(int width) where T : struct
    {
        int elementSize = Unsafe.SizeOf<T>();
        int minStride = width * elementSize;

        // Determine optimal alignment based on available SIMD support
        int alignment = Vector512.IsHardwareAccelerated ? 64 :
                       Vector256.IsHardwareAccelerated ? 32 :
                       Vector128.IsHardwareAccelerated ? 16 : 8;

        return CalculateStride(width, elementSize, alignment);
    }
}

// Buffer with optimized stride
public class AlignedImageBuffer
{
    private readonly byte[] _data;
    private readonly int _width;
    private readonly int _height;
    private readonly int _stride;
    private readonly int _bytesPerPixel;

    public unsafe AlignedImageBuffer(int width, int height, int bytesPerPixel)
    {
        _width = width;
        _height = height;
        _bytesPerPixel = bytesPerPixel;
        _stride = StrideCalculator.CalculateSimdOptimalStride<byte>(width * bytesPerPixel);

        // Allocate with extra space for alignment
        int totalSize = _stride * height + 63; // Extra 63 bytes for alignment
        _data = GC.AllocateUninitializedArray<byte>(totalSize, pinned: true);

        // Ensure 64-byte alignment
        fixed (byte* ptr = _data)
        {
            long address = (long)ptr;
            AlignmentOffset = (int)((64 - (address % 64)) % 64);
        }
    }

    public int AlignmentOffset { get; }

    public Span<byte> GetAlignedRowSpan(int y)
    {
        int offset = AlignmentOffset + (y * _stride);
        return _data.AsSpan(offset, _width * _bytesPerPixel);
    }
}
```

### Buffer lifetime and disposal patterns

Proper buffer lifetime management prevents memory leaks while avoiding premature disposal that could corrupt active
operations. The **reference counting pattern** tracks buffer usage across multiple consumers, while **dispose tokens**
enable safe asynchronous disposal after all operations complete.

```csharp
// Reference-counted buffer with safe disposal
public class RefCountedImageBuffer : IDisposable
{
    private readonly byte[] _buffer;
    private readonly ImageBufferPool _pool;
    private int _refCount = 1;
    private int _disposed = 0;

    public IImageBufferLease Lease()
    {
        if (Interlocked.Increment(ref _refCount) > 1)
        {
            return new BufferLease(this);
        }

        throw new ObjectDisposedException(nameof(RefCountedImageBuffer));
    }

    private class BufferLease : IImageBufferLease
    {
        private RefCountedImageBuffer _owner;

        public BufferLease(RefCountedImageBuffer owner)
        {
            _owner = owner;
        }

        public Span<byte> GetSpan() => _owner._buffer;

        public void Dispose()
        {
            _owner?.Release();
            _owner = null;
        }
    }

    private void Release()
    {
        if (Interlocked.Decrement(ref _refCount) == 0)
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            _pool?.Return(_buffer, _buffer.Length);
        }
    }
}
```

### Cross-platform buffer considerations

Different platforms impose varying constraints on image buffer management. Windows systems benefit from large page
support for buffers exceeding 2MB, while Linux systems may require specific mmap flags for optimal performance. Mobile
platforms introduce additional considerations around memory pressure notifications and background processing
limitations.

```csharp
// Platform-specific buffer allocation
public static class PlatformOptimizedBuffer
{
    public static unsafe byte[] AllocateLargeBuffer(int size)
    {
        if (OperatingSystem.IsWindows() && size >= 2 * 1024 * 1024)
        {
            // Use large pages on Windows for 2MB+ allocations
            return AllocateWindowsLargePage(size);
        }
        else if (OperatingSystem.IsLinux())
        {
            // Use mmap with hugepage hints on Linux
            return AllocateLinuxHugePage(size);
        }
        else
        {
            // Standard allocation for other platforms
            return GC.AllocateUninitializedArray<byte>(size, pinned: true);
        }
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr VirtualAlloc(
        IntPtr lpAddress,
        UIntPtr dwSize,
        uint flAllocationType,
        uint flProtect);

    private static byte[] AllocateWindowsLargePage(int size)
    {
        const uint MEM_COMMIT = 0x1000;
        const uint MEM_RESERVE = 0x2000;
        const uint MEM_LARGE_PAGES = 0x20000000;
        const uint PAGE_READWRITE = 0x04;

        IntPtr ptr = VirtualAlloc(
            IntPtr.Zero,
            new UIntPtr((uint)size),
            MEM_COMMIT | MEM_RESERVE | MEM_LARGE_PAGES,
            PAGE_READWRITE);

        if (ptr != IntPtr.Zero)
        {
            // Wrap in managed array (requires custom memory manager)
            return CreateManagedWrapper(ptr, size);
        }

        // Fallback to standard allocation
        return GC.AllocateUninitializedArray<byte>(size, pinned: true);
    }
}
```

## 4.3 Coordinate Systems and Transformations

### Understanding coordinate system fundamentals

Graphics applications must navigate between multiple coordinate systems, each optimized for different aspects of image
processing. The **pixel coordinate system** uses discrete integer coordinates with the origin typically at the top-left
corner, following raster scan order. This differs from mathematical coordinate systems where the origin lies at
bottom-left with y-axis pointing upward.

The distinction between **pixel centers and pixel corners** critically affects sampling and transformation accuracy.
When a pixel at coordinates (x, y) represents a sample at the pixel's center, its actual coverage extends from (x-0.5,
y-0.5) to (x+0.5, y+0.5). This half-pixel offset must be considered during transformations to prevent systematic shifts
in the output image.

```csharp
// Coordinate system abstraction
public abstract class CoordinateSystem
{
    public abstract Point2D Transform(Point2D point);
    public abstract Point2D InverseTransform(Point2D point);

    // Transform with subpixel precision
    public virtual Vector2 TransformPrecise(Vector2 point)
    {
        var p = Transform(new Point2D((int)point.X, (int)point.Y));
        var fractionalX = point.X - MathF.Floor(point.X);
        var fractionalY = point.Y - MathF.Floor(point.Y);

        return new Vector2(p.X + fractionalX, p.Y + fractionalY);
    }
}

// Image coordinate system with configurable origin
public class ImageCoordinateSystem : CoordinateSystem
{
    private readonly int _width;
    private readonly int _height;
    private readonly OriginLocation _origin;

    public enum OriginLocation
    {
        TopLeft,
        BottomLeft,
        Center
    }

    public override Point2D Transform(Point2D point)
    {
        return _origin switch
        {
            OriginLocation.TopLeft => point,
            OriginLocation.BottomLeft => new Point2D(point.X, _height - 1 - point.Y),
            OriginLocation.Center => new Point2D(
                point.X - _width / 2,
                point.Y - _height / 2),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
```

### Transformation matrices and their applications

Affine transformations form the backbone of geometric image operations, preserving parallel lines while enabling
translation, rotation, scaling, and shearing. The homogeneous coordinate system extends 2D points to 3D by adding a
w-component, enabling representation of all affine transformations as matrix multiplications.

```csharp
// High-performance transformation matrix implementation
[StructLayout(LayoutKind.Sequential)]
public struct AffineTransform2D
{
    // Layout optimized for SIMD operations
    public float M11, M12, M13;  // First row: [m11, m12, tx]
    public float M21, M22, M23;  // Second row: [m21, m22, ty]
    // Implicit third row: [0, 0, 1]

    public static AffineTransform2D Identity => new()
    {
        M11 = 1, M12 = 0, M13 = 0,
        M21 = 0, M22 = 1, M23 = 0
    };

    // Efficient point transformation using SIMD
    public Vector2 Transform(Vector2 point)
    {
        if (Vector128.IsHardwareAccelerated)
        {
            var p = Vector128.Create(point.X, point.Y, 1f, 0f);
            var row1 = Vector128.Create(M11, M12, M13, 0f);
            var row2 = Vector128.Create(M21, M22, M23, 0f);

            var x = Vector128.Dot(p, row1);
            var y = Vector128.Dot(p, row2);

            return new Vector2(x, y);
        }
        else
        {
            // Scalar fallback
            return new Vector2(
                M11 * point.X + M12 * point.Y + M13,
                M21 * point.X + M22 * point.Y + M23);
        }
    }

    // Batch transformation for performance
    public void TransformPoints(ReadOnlySpan<Vector2> input, Span<Vector2> output)
    {
        if (Vector256.IsHardwareAccelerated && input.Length >= 4)
        {
            // Process 4 points simultaneously with AVX
            TransformPointsVectorized256(input, output);
        }
        else
        {
            // Scalar transformation
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = Transform(input[i]);
            }
        }
    }

    // Compose transformations
    public static AffineTransform2D operator *(
        AffineTransform2D a, AffineTransform2D b)
    {
        return new AffineTransform2D
        {
            M11 = a.M11 * b.M11 + a.M12 * b.M21,
            M12 = a.M11 * b.M12 + a.M12 * b.M22,
            M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13,

            M21 = a.M21 * b.M11 + a.M22 * b.M21,
            M22 = a.M21 * b.M12 + a.M22 * b.M22,
            M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23
        };
    }
}

// Builder pattern for complex transformations
public class TransformBuilder
{
    private AffineTransform2D _transform = AffineTransform2D.Identity;

    public TransformBuilder Translate(float dx, float dy)
    {
        var translation = new AffineTransform2D
        {
            M11 = 1, M12 = 0, M13 = dx,
            M21 = 0, M22 = 1, M23 = dy
        };
        _transform = translation * _transform;
        return this;
    }

    public TransformBuilder Rotate(float angleRadians, Vector2? center = null)
    {
        var cos = MathF.Cos(angleRadians);
        var sin = MathF.Sin(angleRadians);

        if (center.HasValue)
        {
            // Translate to origin, rotate, translate back
            Translate(-center.Value.X, -center.Value.Y);
        }

        var rotation = new AffineTransform2D
        {
            M11 = cos, M12 = -sin, M13 = 0,
            M21 = sin, M22 = cos, M23 = 0
        };
        _transform = rotation * _transform;

        if (center.HasValue)
        {
            Translate(center.Value.X, center.Value.Y);
        }

        return this;
    }

    public TransformBuilder Scale(float sx, float sy, Vector2? center = null)
    {
        if (center.HasValue)
        {
            Translate(-center.Value.X, -center.Value.Y);
        }

        var scale = new AffineTransform2D
        {
            M11 = sx, M12 = 0, M13 = 0,
            M21 = 0, M22 = sy, M23 = 0
        };
        _transform = scale * _transform;

        if (center.HasValue)
        {
            Translate(center.Value.X, center.Value.Y);
        }

        return this;
    }

    public AffineTransform2D Build() => _transform;
}
```

### Inverse transformations and numerical stability

Computing inverse transformations requires careful attention to numerical stability, particularly when transformations
approach singularity. The determinant indicates invertibility, with values near zero suggesting numerical instability.
Robust implementations use condition number analysis and provide fallback strategies for degenerate cases.

```csharp
public struct RobustTransform2D
{
    private readonly AffineTransform2D _forward;
    private readonly AffineTransform2D _inverse;
    private readonly float _conditionNumber;

    public bool TryInvert(out AffineTransform2D inverse)
    {
        // Calculate determinant
        float det = _forward.M11 * _forward.M22 - _forward.M12 * _forward.M21;

        // Check for numerical stability
        const float epsilon = 1e-6f;
        if (MathF.Abs(det) < epsilon)
        {
            inverse = AffineTransform2D.Identity;
            return false;
        }

        // Compute inverse using cofactor method
        float invDet = 1f / det;
        inverse = new AffineTransform2D
        {
            M11 = _forward.M22 * invDet,
            M12 = -_forward.M12 * invDet,
            M13 = (_forward.M12 * _forward.M23 - _forward.M22 * _forward.M13) * invDet,

            M21 = -_forward.M21 * invDet,
            M22 = _forward.M11 * invDet,
            M23 = (_forward.M21 * _forward.M13 - _forward.M11 * _forward.M23) * invDet
        };

        // Verify inverse accuracy
        var identity = _forward * inverse;
        float error = MathF.Abs(identity.M11 - 1) + MathF.Abs(identity.M12) +
                     MathF.Abs(identity.M21) + MathF.Abs(identity.M22 - 1);

        return error < epsilon * 10;
    }

    // Compute condition number for stability analysis
    public float ComputeConditionNumber()
    {
        // Use SVD for accurate condition number
        var matrix = new float[,]
        {
            { _forward.M11, _forward.M12 },
            { _forward.M21, _forward.M22 }
        };

        var (s1, s2) = ComputeSingularValues(matrix);
        return s2 > 0 ? s1 / s2 : float.PositiveInfinity;
    }
}
```

### Sampling and resampling considerations

Coordinate transformations necessitate resampling when source and destination grids don't align. The choice of sampling
strategy significantly impacts both quality and performance. **Nearest neighbor sampling** offers speed but produces
aliasing, while **bilinear interpolation** provides smoother results at moderate computational cost.

```csharp
// High-performance resampling with multiple strategies
public interface IResampler
{
    Vector4 Sample(IImageBuffer source, Vector2 position);
}

public class BilinearResampler : IResampler
{
    public Vector4 Sample(IImageBuffer source, Vector2 position)
    {
        // Separate integer and fractional parts
        int x0 = (int)MathF.Floor(position.X);
        int y0 = (int)MathF.Floor(position.Y);
        float fx = position.X - x0;
        float fy = position.Y - y0;

        // Clamp to image bounds
        x0 = Math.Clamp(x0, 0, source.Width - 1);
        y0 = Math.Clamp(y0, 0, source.Height - 1);
        int x1 = Math.Min(x0 + 1, source.Width - 1);
        int y1 = Math.Min(y0 + 1, source.Height - 1);

        // Sample four neighboring pixels
        var p00 = source.GetPixel(x0, y0);
        var p10 = source.GetPixel(x1, y0);
        var p01 = source.GetPixel(x0, y1);
        var p11 = source.GetPixel(x1, y1);

        // Bilinear interpolation
        var p0 = Vector4.Lerp(p00, p10, fx);
        var p1 = Vector4.Lerp(p01, p11, fx);
        return Vector4.Lerp(p0, p1, fy);
    }
}

// Optimized transform with resampling
public class TransformProcessor
{
    private readonly IResampler _resampler;

    public void ApplyTransform(
        IImageBuffer source,
        IImageBuffer destination,
        AffineTransform2D transform)
    {
        // Compute inverse transform for backward mapping
        if (!transform.TryInvert(out var inverse))
        {
            throw new ArgumentException("Transform is not invertible");
        }

        // Process in parallel for performance
        Parallel.For(0, destination.Height, y =>
        {
            var destRow = destination.GetRowSpan<Vector4>(y);

            for (int x = 0; x < destination.Width; x++)
            {
                // Transform destination coordinate to source
                var destPoint = new Vector2(x + 0.5f, y + 0.5f);
                var sourcePoint = inverse.Transform(destPoint);

                // Sample from source image
                destRow[x] = _resampler.Sample(source, sourcePoint - new Vector2(0.5f, 0.5f));
            }
        });
    }
}
```

### Projective transformations for advanced scenarios

While affine transformations suffice for many operations, perspective correction and lens distortion require projective
transformations. These transformations use the full 3×3 homogeneous matrix, enabling representation of vanishing points
and non-linear distortions.

```csharp
// Full projective transformation
public struct ProjectiveTransform2D
{
    // Full 3x3 matrix
    public float M11, M12, M13;
    public float M21, M22, M23;
    public float M31, M32, M33;

    public Vector2 Transform(Vector2 point)
    {
        float w = M31 * point.X + M32 * point.Y + M33;

        // Check for division by zero
        if (MathF.Abs(w) < float.Epsilon)
        {
            return new Vector2(float.NaN, float.NaN);
        }

        float invW = 1f / w;
        return new Vector2(
            (M11 * point.X + M12 * point.Y + M13) * invW,
            (M21 * point.X + M22 * point.Y + M23) * invW);
    }

    // Compute transform from four point correspondences
    public static ProjectiveTransform2D FromQuadrilateral(
        Vector2[] source, Vector2[] destination)
    {
        if (source.Length != 4 || destination.Length != 4)
        {
            throw new ArgumentException("Exactly 4 points required");
        }

        // Build linear system Ax = b
        var A = new float[8, 8];
        var b = new float[8];

        for (int i = 0; i < 4; i++)
        {
            float sx = source[i].X;
            float sy = source[i].Y;
            float dx = destination[i].X;
            float dy = destination[i].Y;

            // Row for x coordinate
            A[i * 2, 0] = sx;
            A[i * 2, 1] = sy;
            A[i * 2, 2] = 1;
            A[i * 2, 6] = -dx * sx;
            A[i * 2, 7] = -dx * sy;
            b[i * 2] = dx;

            // Row for y coordinate
            A[i * 2 + 1, 3] = sx;
            A[i * 2 + 1, 4] = sy;
            A[i * 2 + 1, 5] = 1;
            A[i * 2 + 1, 6] = -dy * sx;
            A[i * 2 + 1, 7] = -dy * sy;
            b[i * 2 + 1] = dy;
        }

        // Solve linear system
        var solution = SolveLinearSystem(A, b);

        return new ProjectiveTransform2D
        {
            M11 = solution[0], M12 = solution[1], M13 = solution[2],
            M21 = solution[3], M22 = solution[4], M23 = solution[5],
            M31 = solution[6], M32 = solution[7], M33 = 1
        };
    }
}
```

## 4.4 Metadata Architecture and Design

### Comprehensive metadata model design

Image metadata encompasses far more than basic properties like dimensions and format. A robust metadata architecture
must accommodate standard formats (EXIF, IPTC, XMP), custom application-specific data, and maintain relationships
between different metadata namespaces. The design should support lazy loading for performance, modification tracking for
non-destructive editing, and extensibility for future standards.

```csharp
// Hierarchical metadata architecture
public interface IMetadataContainer
{
    IMetadataNamespace GetNamespace(string uri);
    IEnumerable<IMetadataNamespace> GetAllNamespaces();
    void SetNamespace(string uri, IMetadataNamespace namespace);
    bool RemoveNamespace(string uri);
}

public interface IMetadataNamespace
{
    string Uri { get; }
    string Prefix { get; }
    IMetadataValue GetValue(string key);
    void SetValue(string key, IMetadataValue value);
    IEnumerable<KeyValuePair<string, IMetadataValue>> GetAllValues();
}

// Type-safe metadata values
public abstract class MetadataValue : IMetadataValue
{
    public abstract Type ValueType { get; }
    public abstract object GetValue();
    public abstract T GetValue<T>();
}

public class TypedMetadataValue<T> : MetadataValue
{
    private readonly T _value;

    public TypedMetadataValue(T value)
    {
        _value = value;
    }

    public override Type ValueType => typeof(T);
    public override object GetValue() => _value;
    public override TResult GetValue<TResult>()
    {
        if (typeof(TResult) == typeof(T))
        {
            return (TResult)(object)_value;
        }

        // Attempt conversion
        return (TResult)Convert.ChangeType(_value, typeof(TResult));
    }
}

// Lazy-loading metadata implementation
public class LazyMetadataContainer : IMetadataContainer
{
    private readonly Dictionary<string, Lazy<IMetadataNamespace>> _namespaces;
    private readonly IMetadataReader _reader;
    private readonly Stream _source;

    public LazyMetadataContainer(Stream source, IMetadataReader reader)
    {
        _source = source;
        _reader = reader;
        _namespaces = new Dictionary<string, Lazy<IMetadataNamespace>>();

        // Register lazy loaders for known namespaces
        RegisterStandardNamespaces();
    }

    private void RegisterStandardNamespaces()
    {
        // EXIF namespace
        _namespaces["http://ns.adobe.com/exif/1.0/"] =
            new Lazy<IMetadataNamespace>(() => _reader.ReadExif(_source));

        // XMP namespace
        _namespaces["http://ns.adobe.com/xap/1.0/"] =
            new Lazy<IMetadataNamespace>(() => _reader.ReadXmp(_source));

        // IPTC namespace
        _namespaces["http://iptc.org/std/Iptc4xmpCore/1.0/xmlns/"] =
            new Lazy<IMetadataNamespace>(() => _reader.ReadIptc(_source));
    }

    public IMetadataNamespace GetNamespace(string uri)
    {
        if (_namespaces.TryGetValue(uri, out var lazy))
        {
            return lazy.Value; // Triggers loading if needed
        }
        return null;
    }
}
```

### EXIF data handling and optimization

EXIF (Exchangeable Image File Format) metadata requires special handling due to its binary format and complex type
system. The architecture must efficiently parse TIFF-based IFD structures, handle both standard and maker-specific tags,
and preserve byte order (endianness) for round-trip fidelity.

```csharp
// EXIF parser with optimized tag reading
public class ExifReader
{
    private readonly Dictionary<ushort, ExifTag> _standardTags;
    private readonly Dictionary<uint, MakerNoteParser> _makerNoteParsers;

    public ExifNamespace ReadExif(Stream stream)
    {
        var reader = new BinaryReader(stream);

        // Check for EXIF marker
        if (!ValidateExifHeader(reader))
        {
            return null;
        }

        // Read TIFF header
        var endianness = ReadEndianness(reader);
        reader = new EndiannessAwareBinaryReader(stream, endianness);

        // Verify TIFF magic number
        ushort magic = reader.ReadUInt16();
        if (magic != 0x002A)
        {
            throw new InvalidDataException("Invalid TIFF magic number");
        }

        // Read IFD offset
        uint ifdOffset = reader.ReadUInt32();

        // Parse IFD chain
        var namespace = new ExifNamespace();
        ParseIfdChain(reader, ifdOffset, namespace);

        return namespace;
    }

    private void ParseIfdChain(
        EndiannessAwareBinaryReader reader,
        uint offset,
        ExifNamespace namespace)
    {
        var processedOffsets = new HashSet<uint>();

        while (offset != 0 && processedOffsets.Add(offset))
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);

            ushort entryCount = reader.ReadUInt16();

            // Read directory entries
            for (int i = 0; i < entryCount; i++)
            {
                var entry = ReadDirectoryEntry(reader);
                ProcessDirectoryEntry(reader, entry, namespace);
            }

            // Next IFD offset
            offset = reader.ReadUInt32();
        }
    }

    private DirectoryEntry ReadDirectoryEntry(EndiannessAwareBinaryReader reader)
    {
        return new DirectoryEntry
        {
            Tag = reader.ReadUInt16(),
            Type = (ExifType)reader.ReadUInt16(),
            Count = reader.ReadUInt32(),
            ValueOffset = reader.ReadUInt32()
        };
    }

    private void ProcessDirectoryEntry(
        EndiannessAwareBinaryReader reader,
        DirectoryEntry entry,
        ExifNamespace namespace)
    {
        // Get tag definition
        if (!_standardTags.TryGetValue(entry.Tag, out var tagDef))
        {
            // Unknown tag - preserve as raw data
            tagDef = new ExifTag(entry.Tag, $"Unknown_{entry.Tag}", entry.Type);
        }

        // Read value based on type and size
        var value = ReadTagValue(reader, entry, tagDef);

        // Special handling for specific tags
        switch (entry.Tag)
        {
            case 0x8769: // EXIF SubIFD
                ParseIfdChain(reader, entry.ValueOffset, namespace);
                break;

            case 0x8825: // GPS IFD
                var gpsNamespace = new GpsNamespace();
                ParseIfdChain(reader, entry.ValueOffset, gpsNamespace);
                namespace.SetGpsData(gpsNamespace);
                break;

            case 0x927C: // MakerNote
                ProcessMakerNote(reader, entry, namespace);
                break;

            default:
                namespace.SetValue(tagDef.Name, value);
                break;
        }
    }
}

// Type-safe EXIF value handling
public class ExifValue : MetadataValue
{
    private readonly object _value;
    private readonly ExifType _type;

    public ExifValue(object value, ExifType type)
    {
        _value = value;
        _type = type;
    }

    public override Type ValueType => _type switch
    {
        ExifType.Byte => typeof(byte),
        ExifType.Short => typeof(ushort),
        ExifType.Long => typeof(uint),
        ExifType.Rational => typeof(Rational),
        ExifType.Ascii => typeof(string),
        _ => typeof(object)
    };

    // Rational number representation
    public struct Rational
    {
        public uint Numerator { get; }
        public uint Denominator { get; }

        public Rational(uint numerator, uint denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public double ToDouble() =>
            Denominator != 0 ? (double)Numerator / Denominator : 0;

        public override string ToString() => $"{Numerator}/{Denominator}";
    }
}
```

### XMP integration and RDF handling

XMP (Extensible Metadata Platform) uses RDF/XML to provide a flexible, extensible metadata format. The architecture must
parse XML efficiently, handle multiple XMP packets within a single file, and support schema extensions while maintaining
compatibility with Adobe's XMP specification.

```csharp
// XMP parser with schema support
public class XmpReader
{
    private readonly Dictionary<string, IXmpSchema> _schemas;

    public XmpReader()
    {
        _schemas = new Dictionary<string, IXmpSchema>
        {
            ["http://ns.adobe.com/xap/1.0/"] = new XmpBasicSchema(),
            ["http://ns.adobe.com/xap/1.0/rights/"] = new XmpRightsSchema(),
            ["http://purl.org/dc/elements/1.1/"] = new DublinCoreSchema()
        };
    }

    public XmpNamespace ReadXmp(Stream stream)
    {
        // Find XMP packet in stream
        var packet = FindXmpPacket(stream);
        if (packet == null)
        {
            return null;
        }

        // Parse XML
        var doc = XDocument.Parse(packet);
        var rdfRoot = doc.Root?.Element(XName.Get("RDF", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"));

        if (rdfRoot == null)
        {
            return null;
        }

        var namespace = new XmpNamespace();

        // Process each Description element
        foreach (var description in rdfRoot.Elements(
            XName.Get("Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#")))
        {
            ProcessDescription(description, namespace);
        }

        return namespace;
    }

    private void ProcessDescription(XElement description, XmpNamespace namespace)
    {
        // Process attributes (simple properties)
        foreach (var attr in description.Attributes())
        {
            if (attr.Name.Namespace == XNamespace.Xmlns)
                continue;

            var schema = GetSchema(attr.Name.NamespaceName);
            var property = schema?.ParseProperty(attr.Name.LocalName, attr.Value);

            if (property != null)
            {
                namespace.SetValue(attr.Name.ToString(), property);
            }
        }

        // Process elements (complex properties)
        foreach (var element in description.Elements())
        {
            ProcessXmpElement(element, namespace);
        }
    }

    private void ProcessXmpElement(XElement element, XmpNamespace namespace)
    {
        var schema = GetSchema(element.Name.NamespaceName);

        // Check for RDF constructs
        if (element.Elements().Any(e => e.Name.LocalName == "Seq" ||
                                       e.Name.LocalName == "Bag" ||
                                       e.Name.LocalName == "Alt"))
        {
            // Array property
            var items = ParseRdfArray(element);
            namespace.SetValue(element.Name.ToString(),
                new XmpArrayValue(items, GetArrayType(element)));
        }
        else if (element.Attributes().Any(a => a.Name.LocalName == "parseType" &&
                                              a.Value == "Resource"))
        {
            // Struct property
            var struct = ParseRdfStruct(element);
            namespace.SetValue(element.Name.ToString(), new XmpStructValue(struct));
        }
        else
        {
            // Simple property
            var value = schema?.ParseProperty(element.Name.LocalName, element.Value);
            if (value != null)
            {
                namespace.SetValue(element.Name.ToString(), value);
            }
        }
    }
}

// Type-safe XMP value representations
public class XmpArrayValue : MetadataValue
{
    public enum ArrayType { Seq, Bag, Alt }

    private readonly List<MetadataValue> _items;
    private readonly ArrayType _type;

    public XmpArrayValue(IEnumerable<MetadataValue> items, ArrayType type)
    {
        _items = items.ToList();
        _type = type;
    }

    public override Type ValueType => typeof(IList<MetadataValue>);
    public override object GetValue() => _items.AsReadOnly();

    public IReadOnlyList<MetadataValue> Items => _items.AsReadOnly();
    public ArrayType Type => _type;
}
```

### Metadata preservation strategies

Non-destructive editing requires preserving all metadata, including unknown or proprietary formats. The architecture
implements copy-on-write semantics for metadata modification, maintains original byte sequences for unmodified data, and
tracks changes through a versioning system.

```csharp
// Metadata preservation with change tracking
public class PreservingMetadataContainer : IMetadataContainer
{
    private readonly IMetadataContainer _original;
    private readonly Dictionary<string, MetadataChange> _changes;
    private readonly List<MetadataAction> _history;

    private class MetadataChange
    {
        public ChangeType Type { get; set; }
        public IMetadataNamespace OriginalValue { get; set; }
        public IMetadataNamespace NewValue { get; set; }
    }

    private enum ChangeType
    {
        Added,
        Modified,
        Removed
    }

    public PreservingMetadataContainer(IMetadataContainer original)
    {
        _original = original;
        _changes = new Dictionary<string, MetadataChange>();
        _history = new List<MetadataAction>();
    }

    public IMetadataNamespace GetNamespace(string uri)
    {
        // Check for changes first
        if (_changes.TryGetValue(uri, out var change))
        {
            return change.Type == ChangeType.Removed ? null : change.NewValue;
        }

        // Return original if unchanged
        return _original.GetNamespace(uri);
    }

    public void SetNamespace(string uri, IMetadataNamespace namespace)
    {
        var original = _original.GetNamespace(uri);

        _changes[uri] = new MetadataChange
        {
            Type = original == null ? ChangeType.Added : ChangeType.Modified,
            OriginalValue = original,
            NewValue = namespace
        };

        _history.Add(new MetadataAction
        {
            Timestamp = DateTime.UtcNow,
            Action = original == null ? "Add" : "Modify",
            Namespace = uri
        });
    }

    public byte[] SerializeWithPreservation(
        Stream originalStream,
        IMetadataWriter writer)
    {
        // Start with copy of original
        var output = new MemoryStream();
        originalStream.CopyTo(output);
        output.Position = 0;

        // Apply changes while preserving unknown metadata
        foreach (var change in _changes)
        {
            switch (change.Value.Type)
            {
                case ChangeType.Added:
                    writer.AddNamespace(output, change.Key, change.Value.NewValue);
                    break;

                case ChangeType.Modified:
                    writer.UpdateNamespace(output, change.Key, change.Value.NewValue);
                    break;

                case ChangeType.Removed:
                    writer.RemoveNamespace(output, change.Key);
                    break;
            }
        }

        return output.ToArray();
    }
}
```

### Performance optimization for metadata operations

Metadata processing can impact overall image loading performance. The architecture implements several optimization
strategies including parallel parsing of independent metadata blocks, caching of frequently accessed values, and
deferred parsing for rarely used metadata types.

```csharp
// High-performance metadata cache
public class CachedMetadataContainer : IMetadataContainer
{
    private readonly IMetadataContainer _source;
    private readonly MemoryCache _cache;
    private readonly SemaphoreSlim _loadLock;

    public CachedMetadataContainer(IMetadataContainer source)
    {
        _source = source;
        _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 100, // Maximum cached namespaces
            CompactionPercentage = 0.25
        });
        _loadLock = new SemaphoreSlim(1, 1);
    }

    public async Task<IMetadataNamespace> GetNamespaceAsync(string uri)
    {
        // Try cache first
        if (_cache.TryGetValue(uri, out IMetadataNamespace cached))
        {
            return cached;
        }

        // Load with lock to prevent duplicate loading
        await _loadLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cache.TryGetValue(uri, out cached))
            {
                return cached;
            }

            // Load from source
            var namespace = await Task.Run(() => _source.GetNamespace(uri));

            if (namespace != null)
            {
                // Cache with size estimate
                var options = new MemoryCacheEntryOptions()
                    .SetSize(EstimateNamespaceSize(namespace))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(uri, namespace, options);
            }

            return namespace;
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private long EstimateNamespaceSize(IMetadataNamespace namespace)
    {
        // Estimate memory usage for cache eviction
        long size = 0;

        foreach (var (key, value) in namespace.GetAllValues())
        {
            size += key.Length * 2; // String overhead
            size += EstimateValueSize(value);
        }

        return size;
    }
}

// Parallel metadata extraction
public class ParallelMetadataReader
{
    private readonly List<IMetadataExtractor> _extractors;

    public async Task<IMetadataContainer> ReadAllMetadataAsync(Stream source)
    {
        // Create seekable buffer for parallel access
        var buffer = await CreateSeekableBufferAsync(source);

        // Extract metadata in parallel
        var tasks = _extractors.Select(extractor =>
            Task.Run(() => extractor.Extract(buffer.CreateView()))
        ).ToArray();

        var results = await Task.WhenAll(tasks);

        // Merge results
        var container = new CompositeMetadataContainer();
        foreach (var (extractor, result) in _extractors.Zip(results))
        {
            if (result != null)
            {
                container.AddNamespace(extractor.NamespaceUri, result);
            }
        }

        return container;
    }
}
```

### Security considerations for metadata handling

Metadata can contain sensitive information and potential security vulnerabilities. The architecture implements
sanitization for user-provided metadata, validates structure to prevent buffer overflows, and provides options for
metadata stripping based on privacy requirements.

```csharp
// Secure metadata sanitizer
public class MetadataSanitizer
{
    private readonly HashSet<string> _sensitiveKeys;
    private readonly IMetadataValidator _validator;

    public MetadataSanitizer()
    {
        _sensitiveKeys = new HashSet<string>
        {
            "GPS:Latitude",
            "GPS:Longitude",
            "XMP:CreatorContactInfo",
            "EXIF:SerialNumber",
            "EXIF:LensSerialNumber"
        };
    }

    public IMetadataContainer Sanitize(
        IMetadataContainer source,
        SanitizationLevel level)
    {
        var sanitized = new FilteredMetadataContainer(source);

        foreach (var namespace in source.GetAllNamespaces())
        {
            var sanitizedNamespace = SanitizeNamespace(namespace, level);
            if (sanitizedNamespace != null)
            {
                sanitized.SetNamespace(namespace.Uri, sanitizedNamespace);
            }
        }

        return sanitized;
    }

    private IMetadataNamespace SanitizeNamespace(
        IMetadataNamespace namespace,
        SanitizationLevel level)
    {
        var sanitized = new MetadataNamespace(namespace.Uri, namespace.Prefix);

        foreach (var (key, value) in namespace.GetAllValues())
        {
            // Skip sensitive data based on level
            if (level >= SanitizationLevel.RemoveLocation &&
                key.StartsWith("GPS:"))
            {
                continue;
            }

            if (level >= SanitizationLevel.RemovePersonal &&
                _sensitiveKeys.Contains(key))
            {
                continue;
            }

            // Validate and sanitize value
            if (_validator.IsValid(key, value))
            {
                sanitized.SetValue(key, SanitizeValue(value));
            }
        }

        return sanitized;
    }

    private IMetadataValue SanitizeValue(IMetadataValue value)
    {
        // Remove potentially dangerous content
        if (value is StringMetadataValue stringValue)
        {
            var sanitized = RemoveControlCharacters(stringValue.Value);
            sanitized = TruncateIfNeeded(sanitized, 1024); // Prevent DoS
            return new StringMetadataValue(sanitized);
        }

        return value;
    }
}

public enum SanitizationLevel
{
    None,
    RemoveLocation,
    RemovePersonal,
    MinimalMetadata
}
```

## Conclusion

Image representation and data structures form the critical foundation upon which all graphics processing operations
build. The careful design of pixel formats determines memory efficiency and processing performance, while sophisticated
buffer management strategies enable handling of images ranging from thumbnails to gigapixel panoramas. Coordinate
systems and transformations provide the mathematical framework for geometric operations, requiring careful attention to
numerical precision and sampling quality.

The metadata architecture demonstrates how modern software design principles apply to graphics processing, with lazy
loading improving startup performance, extensible schemas supporting future standards, and preservation strategies
enabling non-destructive workflows. Security considerations remind us that image data often contains more than pixels,
requiring thoughtful handling of embedded information.

By understanding these fundamental concepts and implementing them with the performance optimizations available in .NET
9.0, developers can build graphics applications that are not only fast and efficient but also robust, secure, and
maintainable. The patterns and architectures presented in this chapter serve as building blocks for the advanced
processing techniques explored in subsequent chapters, forming a solid foundation for high-performance graphics
processing in the modern .NET ecosystem.
