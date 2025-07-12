# Appendix C.3: Memory Usage Guidelines

## Introduction

Efficient memory management is critical for high-performance graphics applications. This appendix provides comprehensive
guidelines for memory allocation, usage patterns, and optimization strategies specific to graphics processing workloads.
Understanding these principles enables developers to build applications that scale effectively while maintaining optimal
performance.

## Memory Requirements by Image Type

### Uncompressed Image Memory Footprint

| Resolution | Name    | Pixels | 8-bit RGB | 8-bit RGBA | 16-bit RGB | 32-bit Float RGBA |
|------------|---------|--------|-----------|------------|------------|-------------------|
| 640×480    | VGA     | 307K   | 0.9 MB    | 1.2 MB     | 1.8 MB     | 4.7 MB            |
| 1280×720   | HD 720p | 922K   | 2.8 MB    | 3.5 MB     | 5.5 MB     | 14.1 MB           |
| 1920×1080  | Full HD | 2.1M   | 6.2 MB    | 7.9 MB     | 12.4 MB    | 31.6 MB           |
| 2560×1440  | QHD     | 3.7M   | 11.1 MB   | 14.1 MB    | 22.1 MB    | 56.3 MB           |
| 3840×2160  | 4K UHD  | 8.3M   | 24.9 MB   | 31.6 MB    | 49.8 MB    | 126.6 MB          |
| 7680×4320  | 8K UHD  | 33.2M  | 99.6 MB   | 126.6 MB   | 199.1 MB   | 506.3 MB          |

### Working Memory Requirements

Processing operations typically require additional working memory beyond source images:

| Operation          | Additional Memory | Formula               | Example (4K Image) |
|--------------------|-------------------|-----------------------|--------------------|
| **Simple Filter**  | 0-1×              | Input only            | 31.6 MB            |
| **Convolution**    | 1×                | Input + output        | 63.2 MB            |
| **Resize**         | Variable          | Input + output        | 31.6 MB + output   |
| **Rotation**       | 1×                | Input + output        | 63.2 MB            |
| **Multi-pass**     | 2-3×              | Input + temp + output | 94.8-126.4 MB      |
| **FFT-based**      | 4×                | Complex input/output  | 126.4 MB           |
| **Pyramid/Mipmap** | 1.33×             | All levels            | 42.1 MB            |

## Memory Allocation Strategies

### Allocation Pattern Guidelines

| Pattern            | Use Case       | Advantages            | Disadvantages         |
|--------------------|----------------|-----------------------|-----------------------|
| **Pre-allocation** | Known sizes    | No runtime allocation | Memory waste possible |
| **Pool-based**     | Repeated ops   | Reduced GC pressure   | Complex management    |
| **On-demand**      | Variable sizes | Memory efficient      | Allocation overhead   |
| **Memory-mapped**  | Large files    | Virtual memory        | Page fault potential  |
| **Pinned memory**  | GPU transfer   | DMA capable           | Limited resource      |

### Buffer Pool Sizing

```csharp
public static class BufferPoolConfiguration
{
    public static PoolSettings CalculateOptimalSettings(
        int maxImageSize,
        int concurrentOperations,
        double memoryBudgetGB)
    {
        // Base calculation
        var bufferSize = maxImageSize * 4; // RGBA bytes
        var buffersNeeded = concurrentOperations * 2; // Input + output

        // Add working buffer requirements
        var workingBuffers = concurrentOperations; // Temporary storage
        var totalBuffers = buffersNeeded + workingBuffers;

        // Calculate with overhead
        var overheadFactor = 1.2; // 20% overhead for fragmentation
        var totalMemoryMB = (totalBuffers * bufferSize * overheadFactor) / (1024 * 1024);

        // Ensure within budget
        var budgetMB = memoryBudgetGB * 1024;
        if (totalMemoryMB > budgetMB)
        {
            // Scale down buffer count
            totalBuffers = (int)(budgetMB / (bufferSize * overheadFactor / (1024 * 1024)));
        }

        return new PoolSettings
        {
            BufferSize = bufferSize,
            MinBuffers = concurrentOperations,
            MaxBuffers = totalBuffers,
            PreAllocate = buffersNeeded
        };
    }
}
```

## Platform Memory Limits

### Operating System Constraints

| Platform          | Process Limit (64-bit) | Practical Limit  | Large Address Aware | Notes                 |
|-------------------|------------------------|------------------|---------------------|-----------------------|
| **Windows 10/11** | 128 TB                 | ~2-4 TB          | Default on x64      | Virtual address space |
| **Linux**         | 128 TB                 | System RAM × 2   | N/A                 | Overcommit settings   |
| **macOS**         | 128 TB                 | System RAM × 1.5 | N/A                 | Compressed memory     |
| **Android**       | Device dependent       | 512 MB - 8 GB    | N/A                 | Per-app limit         |
| **iOS**           | Device dependent       | 1-4 GB           | N/A                 | Aggressive limits     |

### Graphics API Memory Limits

| API            | Texture Size Limit | Buffer Size Limit | Total VRAM | Allocation Overhead |
|----------------|--------------------|-------------------|------------|---------------------|
| **DirectX 12** | 16384×16384        | 2 GB              | Hardware   | ~5%                 |
| **Vulkan**     | Hardware limited   | Hardware limited  | Hardware   | ~2%                 |
| **Metal**      | 16384×16384        | Hardware limited  | Unified    | ~3%                 |
| **OpenGL 4.6** | 16384×16384        | Hardware limited  | Hardware   | ~10%                |
| **WebGL 2.0**  | 4096×4096          | Platform limited  | 1-2 GB     | ~15%                |

## Memory Access Patterns

### Cache-Friendly Access Patterns

| Pattern        | Description            | Cache Efficiency   | Example Use Case      |
|----------------|------------------------|--------------------|-----------------------|
| **Sequential** | Linear array traversal | Excellent (>95%)   | Brightness adjustment |
| **Strided**    | Fixed-step access      | Good (70-90%)      | Column processing     |
| **Tiled**      | Block-wise access      | Very Good (80-95%) | Image filtering       |
| **Random**     | Unpredictable access   | Poor (<30%)        | Warping, lookup       |
| **Gathered**   | Indexed access         | Fair (40-60%)      | Palette mapping       |

### Memory Bandwidth Optimization

```csharp
public static class MemoryBandwidthOptimizer
{
    // Example: Process image in cache-friendly tiles
    public static void ProcessTiled(
        float[] image,
        int width,
        int height,
        int tileSize,
        Action<float[], int, int, int, int> processTile)
    {
        // Calculate based on L2 cache size
        int l2CacheSize = 1024 * 1024; // 1MB typical
        int pixelsPerTile = l2CacheSize / (sizeof(float) * 4) / 2; // Input + output
        int optimalTileSize = (int)Math.Sqrt(pixelsPerTile);

        // Align to cache line
        tileSize = Math.Min(tileSize, optimalTileSize);
        tileSize = (tileSize / 16) * 16; // 64-byte cache line / 4 bytes per float

        Parallel.For(0, (height + tileSize - 1) / tileSize, tileY =>
        {
            for (int tileX = 0; tileX < width; tileX += tileSize)
            {
                int actualWidth = Math.Min(tileSize, width - tileX);
                int actualHeight = Math.Min(tileSize, height - tileY * tileSize);

                processTile(image, tileX, tileY * tileSize, actualWidth, actualHeight);
            }
        });
    }
}
```

## Garbage Collection Impact

### GC Pressure Mitigation Strategies

| Strategy             | Impact    | Implementation Complexity | Use When             |
|----------------------|-----------|---------------------------|----------------------|
| **Object Pooling**   | High      | Medium                    | Frequent allocations |
| **Struct Usage**     | Medium    | Low                       | Small objects        |
| **Stack Allocation** | High      | Medium                    | Temporary data       |
| **Native Memory**    | Very High | High                      | Large buffers        |
| **Gen2 Avoidance**   | Medium    | Low                       | Long-lived objects   |

### Large Object Heap Management

Objects larger than 85,000 bytes allocate on the LOH, which has different GC behavior:

```csharp
public static class LohOptimization
{
    private const int LohThreshold = 85000;

    public static ArraySegment<T> AllocateOptimal<T>(int count) where T : struct
    {
        int sizeInBytes = count * Marshal.SizeOf<T>();

        if (sizeInBytes >= LohThreshold)
        {
            // Use pooled or native memory for LOH-sized allocations
            return MemoryPool<T>.Shared.Rent(count).Memory.Slice(0, count);
        }
        else
        {
            // Regular heap allocation
            return new ArraySegment<T>(new T[count]);
        }
    }

    public static void ConfigureForLargeImages()
    {
        // Configure GC for large object scenarios
        GCSettings.LargeObjectHeapCompactionMode =
            GCLargeObjectHeapCompactionMode.CompactOnce;

        // Use server GC for better throughput
        // Configured in app.config or runtimeconfig.json
    }
}
```

## GPU Memory Management

### GPU Memory Types and Usage

| Memory Type       | Bandwidth  | Latency        | Size       | Best Use          |
|-------------------|------------|----------------|------------|-------------------|
| **Registers**     | 8 TB/s     | 0 cycles       | 256 KB     | Active data       |
| **Shared Memory** | 4 TB/s     | ~5 cycles      | 48-96 KB   | Work group share  |
| **L1 Cache**      | 2 TB/s     | ~28 cycles     | 16-48 KB   | Spatial locality  |
| **L2 Cache**      | 1 TB/s     | ~200 cycles    | 2-6 MB     | Temporal locality |
| **Global Memory** | 0.5-1 TB/s | ~400 cycles    | 4-24 GB    | Main storage      |
| **Host Memory**   | 16-32 GB/s | ~10,000 cycles | System RAM | Staging           |

### GPU Memory Allocation Strategies

```csharp
public class GpuMemoryManager
{
    private readonly Dictionary<int, Queue<GpuBuffer>> _bufferPools = new();
    private readonly object _lock = new();
    private long _totalAllocated;
    private readonly long _maxMemory;

    public GpuMemoryManager(long maxMemoryBytes)
    {
        _maxMemory = maxMemoryBytes;
    }

    public GpuBuffer RentBuffer(int sizeBytes)
    {
        // Round up to power of 2 for better pooling
        int poolSize = NextPowerOfTwo(sizeBytes);

        lock (_lock)
        {
            if (_bufferPools.TryGetValue(poolSize, out var pool) && pool.Count > 0)
            {
                return pool.Dequeue();
            }

            // Check memory budget
            if (_totalAllocated + poolSize > _maxMemory)
            {
                // Trigger cleanup
                CompactPools();

                if (_totalAllocated + poolSize > _maxMemory)
                {
                    throw new OutOfMemoryException("GPU memory exhausted");
                }
            }

            // Allocate new buffer
            var buffer = new GpuBuffer(poolSize);
            _totalAllocated += poolSize;
            return buffer;
        }
    }

    public void ReturnBuffer(GpuBuffer buffer)
    {
        lock (_lock)
        {
            if (!_bufferPools.ContainsKey(buffer.Size))
            {
                _bufferPools[buffer.Size] = new Queue<GpuBuffer>();
            }

            _bufferPools[buffer.Size].Enqueue(buffer);
        }
    }
}
```

## Memory Usage Profiling

### Key Metrics to Monitor

| Metric            | Target Range | Warning Level | Critical Level | Action             |
|-------------------|--------------|---------------|----------------|--------------------|
| **Working Set**   | 50-70% RAM   | >80% RAM      | >90% RAM       | Reduce quality     |
| **Private Bytes** | <2× working  | >3× working   | >4× working    | Check leaks        |
| **Gen 2 Size**    | <100 MB      | >500 MB       | >1 GB          | Optimize lifetime  |
| **LOH Size**      | <500 MB      | >1 GB         | >2 GB          | Use pooling        |
| **GC Time %**     | <5%          | >10%          | >20%           | Reduce allocations |

### Memory Profiling Code

```csharp
public class MemoryProfiler
{
    private readonly Timer _timer;
    private readonly List<MemorySnapshot> _history = new();

    public void StartProfiling(TimeSpan interval)
    {
        _timer = new Timer(_ => TakeSnapshot(), null, TimeSpan.Zero, interval);
    }

    private void TakeSnapshot()
    {
        var snapshot = new MemorySnapshot
        {
            Timestamp = DateTime.UtcNow,
            ManagedHeapBytes = GC.GetTotalMemory(false),
            WorkingSetBytes = Process.GetCurrentProcess().WorkingSet64,
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
        };

        // Get detailed heap info
        var gcInfo = GC.GetGCMemoryInfo();
        snapshot.HeapSizeBytes = gcInfo.HeapSizeBytes;
        snapshot.FragmentedBytes = gcInfo.FragmentedBytes;
        snapshot.HighMemoryLoadThresholdBytes = gcInfo.HighMemoryLoadThresholdBytes;

        _history.Add(snapshot);

        // Analyze trends
        if (_history.Count > 10)
        {
            AnalyzeTrends();
        }
    }

    private void AnalyzeTrends()
    {
        var recent = _history.TakeLast(10).ToList();
        var growth = recent.Last().ManagedHeapBytes - recent.First().ManagedHeapBytes;
        var timeSpan = recent.Last().Timestamp - recent.First().Timestamp;
        var growthRate = growth / timeSpan.TotalSeconds;

        if (growthRate > 1_000_000) // 1MB/second
        {
            Console.WriteLine($"WARNING: Rapid memory growth detected: {growthRate:N0} bytes/sec");
        }
    }
}
```

## Memory-Efficient Design Patterns

### Streaming Processing Pattern

For images too large to fit in memory:

```csharp
public async Task ProcessLargeImageStreaming(
    Stream input,
    Stream output,
    int imageWidth,
    int imageHeight,
    int bandHeight = 128)
{
    var bandsCount = (imageHeight + bandHeight - 1) / bandHeight;
    var bytesPerPixel = 4; // RGBA
    var bandSizeBytes = imageWidth * bandHeight * bytesPerPixel;

    // Rent buffers for streaming
    using var inputBuffer = MemoryPool<byte>.Shared.Rent(bandSizeBytes);
    using var outputBuffer = MemoryPool<byte>.Shared.Rent(bandSizeBytes);

    for (int band = 0; band < bandsCount; band++)
    {
        var currentBandHeight = Math.Min(bandHeight, imageHeight - band * bandHeight);
        var currentBandBytes = imageWidth * currentBandHeight * bytesPerPixel;

        // Read band
        await input.ReadAsync(inputBuffer.Memory.Slice(0, currentBandBytes));

        // Process band
        ProcessBand(
            inputBuffer.Memory.Slice(0, currentBandBytes),
            outputBuffer.Memory.Slice(0, currentBandBytes),
            imageWidth,
            currentBandHeight);

        // Write band
        await output.WriteAsync(outputBuffer.Memory.Slice(0, currentBandBytes));
    }
}
```

### Zero-Copy Patterns

Minimize memory copies using spans and memory mapping:

```csharp
public unsafe class ZeroCopyProcessor
{
    public void ProcessInPlace(Memory<byte> imageData, int width, int height)
    {
        using (var handle = imageData.Pin())
        {
            var ptr = (byte*)handle.Pointer;
            var pixels = new Span<Rgba32>(ptr, width * height);

            // Process directly on pinned memory
            ProcessPixelsVectorized(pixels);
        }
    }

    private void ProcessPixelsVectorized(Span<Rgba32> pixels)
    {
        // Direct SIMD operations on pinned memory
        // No intermediate copies required
    }
}
```

## Memory Budget Planning

### Application Memory Budget Allocation

| Component          | Desktop (%) | Mobile (%) | Server (%) | Notes             |
|--------------------|-------------|------------|------------|-------------------|
| **Image Buffers**  | 40-50%      | 60-70%     | 30-40%     | Primary data      |
| **Working Memory** | 20-30%      | 15-20%     | 20-30%     | Temporary buffers |
| **Cache/Pool**     | 15-20%      | 10-15%     | 30-40%     | Reusable memory   |
| **UI/Framework**   | 10-15%      | 5-10%      | 5-10%      | System overhead   |
| **Reserve**        | 5-10%       | 5-10%      | 5-10%      | Spike handling    |

### Dynamic Memory Management

```csharp
public class DynamicMemoryManager
{
    private readonly long _minMemory;
    private readonly long _maxMemory;
    private readonly long _targetMemory;
    private QualityLevel _currentQuality = QualityLevel.High;

    public DynamicMemoryManager(long targetMemoryMB)
    {
        _targetMemory = targetMemoryMB * 1024 * 1024;
        _minMemory = _targetMemory / 2;
        _maxMemory = _targetMemory * 2;
    }

    public void AdjustQualityBasedOnMemory()
    {
        var currentUsage = GC.GetTotalMemory(false);
        var workingSet = Process.GetCurrentProcess().WorkingSet64;

        if (workingSet > _maxMemory)
        {
            // Emergency: Reduce quality immediately
            _currentQuality = QualityLevel.Low;
            ForceCleanup();
        }
        else if (workingSet > _targetMemory)
        {
            // Warning: Gradually reduce quality
            if (_currentQuality > QualityLevel.Medium)
                _currentQuality--;
        }
        else if (workingSet < _minMemory && _currentQuality < QualityLevel.High)
        {
            // Headroom available: Increase quality
            _currentQuality++;
        }
    }

    private void ForceCleanup()
    {
        // Clear caches
        ImageCache.Instance.Clear();

        // Force GC
        GC.Collect(2, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, true);

        // Compact LOH
        GCSettings.LargeObjectHeapCompactionMode =
            GCLargeObjectHeapCompactionMode.CompactOnce;
    }
}
```

## Summary

Effective memory management in graphics applications requires understanding and applying these key principles:

1. **Know Your Footprint**: Calculate memory requirements accurately including working buffers
2. **Pool Resources**: Reuse buffers to minimize allocation overhead and GC pressure
3. **Access Patterns Matter**: Design algorithms for cache-friendly memory access
4. **Platform Awareness**: Respect platform-specific limits and behaviors
5. **Monitor and Adapt**: Implement dynamic adjustment based on runtime conditions
6. **Plan for Scale**: Design memory strategies that work from thumbnails to 8K images

These guidelines provide a foundation for building memory-efficient graphics applications that maintain performance
while scaling to handle diverse workloads and platforms.
