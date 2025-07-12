# Chapter 9: Streaming and Tiling Architecture

The transformation from loading entire images into memory to streaming tiles on demand represents one of the most
significant architectural shifts in modern graphics processing. Consider the challenge: a single aerial photograph of
Manhattan at 1cm resolution would require 400GB of storage, yet users expect instant, smooth panning and zooming on
their mobile devices. The solution lies in sophisticated streaming and tiling architectures that slice massive datasets
into manageable chunks, predict what users will need next, and deliver it just in time. This chapter explores how .NET
9.0's advanced streaming capabilities, combined with modern HTTP protocols and intelligent caching strategies, enable
applications to handle terabyte-scale imagery while maintaining sub-second response times. From Google Maps serving
billions of tile requests daily to medical imaging systems streaming multi-gigapixel pathology slides, these patterns
have become fundamental to how we interact with visual data at scale.

## 9.1 Tile-Based Rendering Systems

The evolution from immediate-mode to tile-based rendering represents a fundamental rethinking of how graphics hardware
processes pixels. Modern mobile GPUs demonstrate this shift dramatically—where traditional desktop GPUs might consume
100W processing a complex scene, a mobile GPU achieves similar results at 5W through intelligent tile-based
architectures.

### Understanding modern tile-based GPU architectures

Tile-based rendering divides the screen into small rectangular regions, typically 16×16 to 32×32 pixels, processing each
tile to completion before moving to the next. This **locality of reference** transforms memory access patterns from
random to sequential, reducing bandwidth requirements by up to 10x compared to traditional immediate-mode rendering.

```csharp
public class TileBasedRenderer
{
    private readonly int tileWidth;
    private readonly int tileHeight;
    private readonly MemoryPool<byte> tilePool;
    private readonly Channel<TileRenderRequest> renderQueue;

    public TileBasedRenderer(int tileSize = 256)
    {
        tileWidth = tileHeight = tileSize;
        tilePool = MemoryPool<byte>.Shared;
        renderQueue = Channel.CreateUnbounded<TileRenderRequest>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
    }

    public async Task RenderSceneAsync(Scene scene, RenderTarget target)
    {
        // Phase 1: Geometry processing - bin primitives to tiles
        var tileBins = await BinGeometryToTilesAsync(scene);

        // Phase 2: Parallel tile rendering
        var tileCount = (target.Width / tileWidth) * (target.Height / tileHeight);
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);

        await Parallel.ForEachAsync(
            Partitioner.Create(0, tileCount, tileCount / Environment.ProcessorCount),
            async (range, ct) =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    for (int tileIndex = range.Item1; tileIndex < range.Item2; tileIndex++)
                    {
                        await RenderTileAsync(tileIndex, tileBins[tileIndex], target);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });
    }

    private async Task<TileBin[]> BinGeometryToTilesAsync(Scene scene)
    {
        var tilesX = (scene.ViewportWidth + tileWidth - 1) / tileWidth;
        var tilesY = (scene.ViewportHeight + tileHeight - 1) / tileHeight;
        var bins = new TileBin[tilesX * tilesY];

        // Initialize bins
        for (int i = 0; i < bins.Length; i++)
        {
            bins[i] = new TileBin(tileWidth, tileHeight);
        }

        // Parallel primitive processing
        await Parallel.ForEachAsync(scene.Primitives, async (primitive, ct) =>
        {
            // Transform vertices
            var transformedPrimitive = await TransformPrimitiveAsync(primitive, scene.ViewProjectionMatrix);

            // Determine which tiles this primitive touches
            var tileBounds = CalculateTileBounds(transformedPrimitive, tilesX, tilesY);

            // Add to appropriate bins
            for (int y = tileBounds.MinY; y <= tileBounds.MaxY; y++)
            {
                for (int x = tileBounds.MinX; x <= tileBounds.MaxX; x++)
                {
                    var binIndex = y * tilesX + x;
                    bins[binIndex].AddPrimitive(transformedPrimitive);
                }
            }
        });

        return bins;
    }
}
```

Modern GPU architectures employ sophisticated **two-phase rendering**: the binning phase assigns geometry to tiles,
while the rendering phase processes each tile using fast on-chip memory. ARM Mali GPUs use 16×16 pixel tiles optimized
for memory efficiency, PowerVR employs 32×32 tiles with hardware-managed parameter buffers, and Apple's TBDR (Tile-Based
Deferred Rendering) adds hidden surface removal, eliminating overdraw entirely.

### Optimal tile sizing strategies

Tile size selection profoundly impacts performance, memory usage, and visual quality. The trade-offs are complex and
application-specific:

```csharp
public class AdaptiveTileManager
{
    private readonly struct TileMetrics
    {
        public int Size { get; init; }
        public double MemoryUsage { get; init; }
        public double RenderTime { get; init; }
        public double NetworkLatency { get; init; }
    }

    public int DetermineOptimalTileSize(
        DeviceCapabilities device,
        NetworkConditions network,
        ContentComplexity content)
    {
        // Base tile size on device characteristics
        int baseSize = device.PixelDensity switch
        {
            <= 1.0f => 256,  // Standard displays
            <= 2.0f => 512,  // Retina displays
            _ => 1024        // Ultra-high DPI
        };

        // Adjust for available memory
        var availableMemory = GC.GetTotalMemory(false);
        var memoryPressure = GC.GetTotalMemory(false) / (double)device.TotalMemory;

        if (memoryPressure > 0.8)
        {
            baseSize = Math.Max(128, baseSize / 2);
        }

        // Consider network conditions
        if (network.EffectiveBandwidth < 1_000_000) // Less than 1 Mbps
        {
            baseSize = Math.Min(256, baseSize);
        }

        // Adapt to content complexity
        baseSize = content.Type switch
        {
            ContentType.SolidColor => Math.Min(512, baseSize * 2),
            ContentType.SimpleGradient => baseSize,
            ContentType.ComplexPhoto => baseSize,
            ContentType.DetailedMap => Math.Max(256, baseSize / 2),
            _ => baseSize
        };

        return ValidateTileSize(baseSize);
    }

    private int ValidateTileSize(int size)
    {
        // Ensure power of 2 for GPU efficiency
        return (int)Math.Pow(2, Math.Round(Math.Log2(size)));
    }

    // Benchmark different tile sizes
    public async Task<TileMetrics> BenchmarkTileSizeAsync(int tileSize, TestDataset dataset)
    {
        var stopwatch = Stopwatch.StartNew();
        var memoryBefore = GC.GetTotalMemory(true);

        // Render test scene
        using var renderer = new TileBasedRenderer(tileSize);
        await renderer.RenderSceneAsync(dataset.Scene, dataset.Target);

        var memoryAfter = GC.GetTotalMemory(false);
        stopwatch.Stop();

        return new TileMetrics
        {
            Size = tileSize,
            MemoryUsage = memoryAfter - memoryBefore,
            RenderTime = stopwatch.Elapsed.TotalMilliseconds,
            NetworkLatency = await MeasureNetworkLatencyAsync(tileSize)
        };
    }
}
```

Performance measurements across different architectures reveal optimal configurations:

- **Mobile GPUs**: 16×16 tiles minimize on-chip memory usage, crucial for power efficiency
- **Desktop GPUs**: 64×64 tiles balance parallelism with cache efficiency
- **Web mapping**: 256×256 standard, with 512×512 for high-DPI displays
- **Medical imaging**: 1024×1024 for gigapixel pathology slides
- **Game engines**: Adaptive 32-128 pixels based on scene complexity

### Memory management and caching strategies

Efficient tile caching transforms perceived performance by serving frequently accessed tiles from memory rather than
regenerating or refetching them:

```csharp
public class HierarchicalTileCache
{
    private readonly struct CacheEntry
    {
        public byte[] Data { get; init; }
        public DateTime LastAccess { get; init; }
        public int AccessCount { get; init; }
        public TileMetadata Metadata { get; init; }
        public CompressionType Compression { get; init; }
    }

    private readonly Dictionary<TileKey, CacheEntry> l1Cache; // Hot tiles in memory
    private readonly LRUCache<TileKey, CacheEntry> l2Cache;  // Recently used
    private readonly IDistributedCache l3Cache;              // Redis/distributed

    private readonly long maxL1Size;
    private readonly long maxL2Size;
    private long currentL1Usage;
    private long currentL2Usage;

    public async Task<TileData> GetTileAsync(TileKey key, CancellationToken ct = default)
    {
        // L1 cache - fastest path
        if (l1Cache.TryGetValue(key, out var l1Entry))
        {
            UpdateAccessStatistics(ref l1Entry);
            return DecompressTile(l1Entry);
        }

        // L2 cache - still in-process
        if (l2Cache.TryGetValue(key, out var l2Entry))
        {
            // Promote to L1 if hot
            if (ShouldPromoteToL1(l2Entry))
            {
                await PromoteToL1Async(key, l2Entry);
            }
            return DecompressTile(l2Entry);
        }

        // L3 cache - distributed
        var l3Data = await l3Cache.GetAsync(key.ToString(), ct);
        if (l3Data != null)
        {
            var entry = DeserializeCacheEntry(l3Data);
            await AddToL2Async(key, entry);
            return DecompressTile(entry);
        }

        // Cache miss - generate or fetch
        var tileData = await GenerateTileAsync(key, ct);
        await CacheTileAsync(key, tileData);
        return tileData;
    }

    private async Task CacheTileAsync(TileKey key, TileData data)
    {
        var compressed = await CompressTileAsync(data);
        var entry = new CacheEntry
        {
            Data = compressed.Data,
            Compression = compressed.Type,
            LastAccess = DateTime.UtcNow,
            AccessCount = 1,
            Metadata = data.Metadata
        };

        // Determine cache level based on tile importance
        var importance = CalculateTileImportance(key, data);

        if (importance > 0.8 && currentL1Usage + compressed.Data.Length <= maxL1Size)
        {
            await AddToL1Async(key, entry);
        }
        else if (currentL2Usage + compressed.Data.Length <= maxL2Size)
        {
            await AddToL2Async(key, entry);
        }
        else
        {
            // Only L3 - distributed cache
            await l3Cache.SetAsync(key.ToString(), SerializeCacheEntry(entry));
        }
    }

    private async Task<CompressedTile> CompressTileAsync(TileData data)
    {
        // Choose compression based on content
        var analysis = AnalyzeTileContent(data);

        if (analysis.UniformColor)
        {
            // Single color - extreme compression
            return new CompressedTile
            {
                Type = CompressionType.SingleColor,
                Data = BitConverter.GetBytes(analysis.DominantColor)
            };
        }
        else if (analysis.ColorCount < 256)
        {
            // Indexed color
            return await CompressIndexedAsync(data, analysis);
        }
        else if (analysis.HasPatterns)
        {
            // RLE for patterns
            return await CompressRLEAsync(data);
        }
        else
        {
            // General compression
            return await CompressLZ4Async(data);
        }
    }

    // Sophisticated eviction policy
    private async Task EvictTilesAsync(long bytesNeeded)
    {
        var candidates = l1Cache.Values
            .OrderBy(e => CalculateEvictionScore(e))
            .ToList();

        long evicted = 0;
        foreach (var entry in candidates)
        {
            if (evicted >= bytesNeeded) break;

            var key = l1Cache.First(kvp => kvp.Value.Equals(entry)).Key;
            l1Cache.Remove(key);

            // Demote to L2
            await AddToL2Async(key, entry);

            evicted += entry.Data.Length;
            currentL1Usage -= entry.Data.Length;
        }
    }

    private double CalculateEvictionScore(CacheEntry entry)
    {
        var age = (DateTime.UtcNow - entry.LastAccess).TotalSeconds;
        var frequency = entry.AccessCount;
        var size = entry.Data.Length;

        // Lower score = more likely to evict
        // Balances recency, frequency, and size
        return (frequency * 1000.0) / (age * Math.Log(size + 1));
    }
}
```

### Spatial indexing with quadtrees and R-trees

Efficient spatial indexing enables rapid tile lookup and culling operations essential for interactive performance:

```csharp
public class SpatialTileIndex
{
    private readonly QuadTree<TileNode> quadTree;
    private readonly RTree<TileNode> rTree;
    private readonly HilbertCurveIndex hilbertIndex;

    public class TileNode
    {
        public TileKey Key { get; init; }
        public Bounds2D Bounds { get; init; }
        public int Level { get; init; }
        public TileState State { get; init; }
        public List<TileNode> Children { get; init; }
    }

    public SpatialTileIndex(Bounds2D worldBounds, int maxDepth)
    {
        quadTree = new QuadTree<TileNode>(worldBounds, maxDepth);
        rTree = new RTree<TileNode>(maxNodeEntries: 8);
        hilbertIndex = new HilbertCurveIndex(maxDepth);
    }

    public IEnumerable<TileNode> QueryVisibleTiles(
        Frustum viewFrustum,
        float lodBias = 1.0f)
    {
        // Early frustum culling using quadtree
        var candidates = quadTree.Query(viewFrustum.ToBounds2D());

        // Refine with exact frustum test
        foreach (var node in candidates)
        {
            if (!viewFrustum.Intersects(node.Bounds))
                continue;

            // Calculate screen space error for LOD selection
            var screenError = CalculateScreenSpaceError(node, viewFrustum);
            var threshold = GetLODThreshold(node.Level) * lodBias;

            if (screenError < threshold || node.Children == null)
            {
                // This tile is sufficient detail
                yield return node;
            }
            else
            {
                // Need more detail - recurse to children
                foreach (var child in QueryChildrenRecursive(node, viewFrustum, lodBias))
                {
                    yield return child;
                }
            }
        }
    }

    private float CalculateScreenSpaceError(TileNode node, Frustum frustum)
    {
        // Project tile bounds to screen space
        var screenBounds = frustum.ProjectToScreen(node.Bounds);

        // Calculate geometric error
        var worldSize = node.Bounds.Size;
        var screenSize = screenBounds.Size;
        var distance = frustum.DistanceToPoint(node.Bounds.Center);

        // Screen space error in pixels
        return (worldSize / distance) * frustum.ScreenHeight;
    }

    // Hilbert curve for cache-optimal traversal
    public IEnumerable<TileNode> TraverseCacheOptimal(Bounds2D region)
    {
        var tiles = quadTree.Query(region).ToList();

        // Sort by Hilbert curve order for cache locality
        var sorted = tiles.OrderBy(t => hilbertIndex.GetIndex(t.Bounds.Center));

        foreach (var tile in sorted)
        {
            yield return tile;
        }
    }

    // R-tree for complex geometries
    public void IndexComplexRegion(ComplexGeometry geometry)
    {
        // Decompose into tiles that cover the geometry
        var coveringTiles = DecomposeGeometry(geometry);

        foreach (var tile in coveringTiles)
        {
            rTree.Insert(tile);
        }
    }

    // Predictive prefetching based on movement
    public async Task<IEnumerable<TileNode>> PredictNextTilesAsync(
        MovementHistory history,
        Frustum currentView)
    {
        // Calculate velocity and acceleration
        var velocity = history.GetVelocity();
        var acceleration = history.GetAcceleration();

        // Predict future position
        var predictedPosition = currentView.Position + velocity * PredictionTime;
        var predictedView = currentView.MoveTo(predictedPosition);

        // Get tiles for predicted view
        var predictedTiles = QueryVisibleTiles(predictedView, lodBias: 1.2f);

        // Filter out already visible tiles
        var currentTiles = new HashSet<TileKey>(
            QueryVisibleTiles(currentView).Select(t => t.Key));

        return predictedTiles.Where(t => !currentTiles.Contains(t.Key));
    }
}
```

Performance characteristics of spatial indices:

- **Quadtree**: O(log n) queries, ideal for uniform tile distributions
- **R-tree**: Better for overlapping regions and complex shapes
- **Hilbert curves**: 30-50% better cache locality than Z-order
- **Combined approach**: 15-25% query performance improvement

### GPU tile-based deferred rendering

Modern GPUs implement sophisticated tile-based deferred rendering (TBDR) that eliminates overdraw through hardware
hidden surface removal:

```csharp
public class TBDRPipeline
{
    private readonly ComputeShader tileClassificationShader;
    private readonly ComputeShader tileShadingShader;
    private readonly int tileSize;

    public async Task RenderFrameAsync(
        CommandBuffer cmd,
        RenderTexture target,
        Scene scene)
    {
        // Phase 1: Z-prepass and primitive binning
        using (cmd.BeginSample("TBDR_Binning"))
        {
            // Clear tile lists
            cmd.SetComputeBufferParam(tileClassificationShader,
                "TileLists", tileLists);
            cmd.DispatchCompute(tileClassificationShader,
                clearKernel, tileCountX, tileCountY, 1);

            // Render z-prepass and bin primitives
            foreach (var primitive in scene.OpaqueGeometry)
            {
                // Vertex shader tags primitives with tile IDs
                cmd.DrawMeshInstanced(primitive.Mesh,
                    primitive.Material,
                    zPrepassMaterial);
            }
        }

        // Phase 2: Per-tile shading
        using (cmd.BeginSample("TBDR_Shading"))
        {
            // Process each tile independently
            cmd.SetComputeTextureParam(tileShadingShader,
                shadingKernel, "Output", target);

            // Dispatch one thread group per tile
            cmd.DispatchCompute(tileShadingShader,
                shadingKernel, tileCountX, tileCountY, 1);
        }

        // Phase 3: Resolve and post-processing
        await ResolveMultisamplingAsync(cmd, target);
    }

    [ComputeShader("TileShading.compute")]
    private const string TileShadingKernel = @"
        #pragma kernel TileShading

        // Shared memory for tile data
        groupshared uint s_PrimitiveList[MAX_PRIMITIVES_PER_TILE];
        groupshared uint s_PrimitiveCount;
        groupshared float s_MinDepth;
        groupshared float s_MaxDepth;

        [numthreads(TILE_SIZE, TILE_SIZE, 1)]
        void TileShading(uint3 id : SV_DispatchThreadID,
                        uint3 groupId : SV_GroupID,
                        uint3 groupThreadId : SV_GroupThreadID)
        {
            uint tileIndex = groupId.y * TilesX + groupId.x;

            // First thread loads tile primitive list
            if (all(groupThreadId.xy == 0))
            {
                s_PrimitiveCount = TileLists[tileIndex].count;
                s_MinDepth = TileLists[tileIndex].minDepth;
                s_MaxDepth = TileLists[tileIndex].maxDepth;

                // Load primitive indices
                for (uint i = 0; i < s_PrimitiveCount; i++)
                {
                    s_PrimitiveList[i] = TileLists[tileIndex].primitives[i];
                }
            }

            GroupMemoryBarrierWithGroupSync();

            // Early Z-rejection
            float pixelDepth = DepthTexture.Load(int3(id.xy, 0)).r;
            if (pixelDepth < s_MinDepth || pixelDepth > s_MaxDepth)
                return;

            // Shade pixel using only primitives in this tile
            float3 color = float3(0, 0, 0);

            for (uint i = 0; i < s_PrimitiveCount; i++)
            {
                uint primIndex = s_PrimitiveList[i];

                // Test if primitive covers this pixel
                if (TestPrimitiveCoverage(primIndex, id.xy))
                {
                    color += ShadePrimitive(primIndex, id.xy);
                }
            }

            Output[id.xy] = float4(color, 1.0);
        }
    ";
}
```

## 9.2 Progressive Loading Patterns

Progressive loading transforms user perception of performance by providing immediate visual feedback while full-quality
content loads in the background. This psychological optimization often matters more than actual load times—users
perceive progressive interfaces as 40% faster than equivalent blocking loads.

### JPEG progressive encoding strategies

Progressive JPEG encoding reorganizes image data from the traditional raster scan order into multiple scans of
increasing quality:

```csharp
public class ProgressiveJPEGEncoder
{
    private readonly struct ScanScript
    {
        public int StartComponent { get; init; }
        public int EndComponent { get; init; }
        public int StartCoefficient { get; init; }
        public int EndCoefficient { get; init; }
        public int SuccessiveBit { get; init; }
    }

    // Optimal scan script for web delivery
    private static readonly ScanScript[] WebOptimizedScans = new[]
    {
        // Scan 1: DC coefficients only (very low quality)
        new ScanScript { StartComponent = 0, EndComponent = 2,
                        StartCoefficient = 0, EndCoefficient = 0 },

        // Scan 2: First 5 AC coefficients (basic structure)
        new ScanScript { StartComponent = 0, EndComponent = 2,
                        StartCoefficient = 1, EndCoefficient = 5 },

        // Scan 3: Next 9 AC coefficients (improved detail)
        new ScanScript { StartComponent = 0, EndComponent = 2,
                        StartCoefficient = 6, EndCoefficient = 14 },

        // Scan 4-7: Successive approximation for refinement
        // ... additional scans for quality improvement
    };

    public async Task<Stream> EncodeProgressiveAsync(
        Image<Rgb24> image,
        int quality = 85)
    {
        var output = new MemoryStream();

        // Compute DCT coefficients for all blocks
        var dctCoefficients = await ComputeDCTCoefficientsAsync(image);

        // Quantize based on quality setting
        var quantized = QuantizeCoefficients(dctCoefficients, quality);

        // Write JPEG header
        WriteJPEGHeader(output, image.Width, image.Height, WebOptimizedScans);

        // Encode each scan
        foreach (var scan in WebOptimizedScans)
        {
            await EncodeScanAsync(output, quantized, scan);

            // Flush to enable progressive display
            await output.FlushAsync();
        }

        // Write EOI marker
        output.WriteByte(0xFF);
        output.WriteByte(0xD9);

        output.Position = 0;
        return output;
    }

    private async Task EncodeScanAsync(
        Stream output,
        QuantizedCoefficients coefficients,
        ScanScript scan)
    {
        // Start of scan marker
        output.WriteByte(0xFF);
        output.WriteByte(0xDA);

        var entropy = new ArithmeticEncoder(output);

        // Encode specified coefficient range
        for (int block = 0; block < coefficients.BlockCount; block++)
        {
            for (int comp = scan.StartComponent; comp <= scan.EndComponent; comp++)
            {
                for (int coef = scan.StartCoefficient; coef <= scan.EndCoefficient; coef++)
                {
                    var value = coefficients.GetCoefficient(block, comp, coef);

                    if (scan.SuccessiveBit > 0)
                    {
                        // Successive approximation - send refinement bits
                        entropy.EncodeBit((value >> scan.SuccessiveBit) & 1);
                    }
                    else
                    {
                        // First scan for this coefficient
                        entropy.EncodeSymbol(value);
                    }
                }
            }

            // Allow cancellation for large images
            if (block % 1000 == 0)
            {
                await Task.Yield();
            }
        }

        entropy.Flush();
    }
}

// Client-side progressive decoder
public class ProgressiveImageLoader
{
    private readonly HttpClient httpClient;
    private readonly Channel<ImageUpdate> updateChannel;

    public async IAsyncEnumerable<ImageUpdate> LoadProgressiveAsync(
        Uri imageUri,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(imageUri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var decoder = new ProgressiveJPEGDecoder();

        await foreach (var update in decoder.DecodeStreamAsync(stream, cancellationToken))
        {
            yield return new ImageUpdate
            {
                Image = update.Image,
                Quality = update.ScanNumber / (float)update.TotalScans,
                BytesLoaded = update.BytesProcessed,
                IsComplete = update.IsComplete
            };
        }
    }

    // Intelligent quality scheduling based on viewport
    public async Task LoadImageWithPriorityAsync(
        Uri imageUri,
        Rectangle viewport,
        Action<ImageUpdate> onUpdate)
    {
        var updates = LoadProgressiveAsync(imageUri).ConfigureAwait(false);

        await foreach (var update in updates)
        {
            // For viewport area, show every update
            if (IsInViewport(update.Image.Bounds, viewport))
            {
                onUpdate(update);
            }
            // For off-viewport, skip intermediate quality levels
            else if (update.Quality > 0.8f || update.IsComplete)
            {
                onUpdate(update);
            }
        }
    }
}
```

### Implementing LQIP and Blurhash algorithms

Low Quality Image Placeholders (LQIP) and Blurhash provide instant visual feedback with minimal data transfer:

```csharp
public class ModernPlaceholderGenerator
{
    // Blurhash implementation
    public class BlurhashEncoder
    {
        private const int MinComponents = 1;
        private const int MaxComponents = 9;

        public string Encode(
            ReadOnlySpan<Rgba32> pixels,
            int width,
            int height,
            int componentsX = 4,
            int componentsY = 3)
        {
            componentsX = Math.Clamp(componentsX, MinComponents, MaxComponents);
            componentsY = Math.Clamp(componentsY, MinComponents, MaxComponents);

            // Calculate DCT coefficients
            var coefficients = new Vector3[componentsX * componentsY];

            for (int y = 0; y < componentsY; y++)
            {
                for (int x = 0; x < componentsX; x++)
                {
                    coefficients[y * componentsX + x] =
                        CalculateDCTCoefficient(pixels, width, height, x, y);
                }
            }

            // Encode to base83 string
            return EncodeCoefficients(coefficients, componentsX, componentsY);
        }

        private Vector3 CalculateDCTCoefficient(
            ReadOnlySpan<Rgba32> pixels,
            int width,
            int height,
            int componentX,
            int componentY)
        {
            var normX = componentX == 0 ? 1f : 2f;
            var normY = componentY == 0 ? 1f : 2f;
            var scale = normX * normY / (width * height);

            var r = 0f;
            var g = 0f;
            var b = 0f;

            // Optimized DCT calculation using lookup tables
            var cosinesX = GetCosineTable(componentX, width);
            var cosinesY = GetCosineTable(componentY, height);

            for (int y = 0; y < height; y++)
            {
                var cosY = cosinesY[y];
                var rowOffset = y * width;

                for (int x = 0; x < width; x++)
                {
                    var pixel = pixels[rowOffset + x];
                    var basis = cosinesX[x] * cosY;

                    r += SRGBToLinear(pixel.R / 255f) * basis;
                    g += SRGBToLinear(pixel.G / 255f) * basis;
                    b += SRGBToLinear(pixel.B / 255f) * basis;
                }
            }

            return new Vector3(r * scale, g * scale, b * scale);
        }

        // Cached cosine tables for performance
        private readonly Dictionary<(int component, int size), float[]> cosineCache = new();

        private float[] GetCosineTable(int component, int size)
        {
            var key = (component, size);

            if (!cosineCache.TryGetValue(key, out var table))
            {
                table = new float[size];
                var factor = MathF.PI * component / size;

                for (int i = 0; i < size; i++)
                {
                    table[i] = MathF.Cos(factor * (i + 0.5f));
                }

                cosineCache[key] = table;
            }

            return table;
        }
    }

    // LQIP with WebP encoding for maximum compression
    public async Task<LQIPData> GenerateLQIPAsync(
        Image<Rgba32> originalImage,
        int targetSize = 32,
        int blurRadius = 5)
    {
        // Resize maintaining aspect ratio
        var aspectRatio = originalImage.Width / (float)originalImage.Height;
        int width, height;

        if (aspectRatio > 1)
        {
            width = targetSize;
            height = (int)(targetSize / aspectRatio);
        }
        else
        {
            width = (int)(targetSize * aspectRatio);
            height = targetSize;
        }

        // High-quality downsampling
        using var resized = originalImage.Clone(ctx => ctx
            .Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Sampler = KnownResamplers.Lanczos3,
                Compand = true
            }));

        // Apply gaussian blur to hide compression artifacts
        resized.Mutate(ctx => ctx.GaussianBlur(blurRadius));

        // Generate multiple formats
        var result = new LQIPData
        {
            Width = width,
            Height = height,
            AspectRatio = aspectRatio
        };

        // WebP for modern browsers (best compression)
        using (var webpStream = new MemoryStream())
        {
            await resized.SaveAsWebpAsync(webpStream, new WebpEncoder
            {
                Quality = 20,
                Method = WebpEncodingMethod.BestQuality
            });
            result.WebPData = webpStream.ToArray();
        }

        // JPEG for fallback
        using (var jpegStream = new MemoryStream())
        {
            await resized.SaveAsJpegAsync(jpegStream, new JpegEncoder
            {
                Quality = 15,
                Subsample = JpegSubsample.Ratio420
            });
            result.JpegData = jpegStream.ToArray();
        }

        // Generate blurhash for instant preview
        result.Blurhash = new BlurhashEncoder().Encode(
            resized.GetPixelMemoryGroup()[0].Span,
            width, height);

        // Dominant colors for CSS background
        result.DominantColors = ExtractDominantColors(resized, 3);

        return result;
    }

    // Inline SVG placeholder with blur filter
    public string GenerateSVGPlaceholder(LQIPData lqip)
    {
        var colors = lqip.DominantColors;
        var base64 = Convert.ToBase64String(lqip.WebPData);

        return $@"
            <svg viewBox='0 0 {lqip.Width} {lqip.Height}'
                 xmlns='http://www.w3.org/2000/svg'>
                <defs>
                    <linearGradient id='g'>
                        <stop offset='0%' stop-color='{colors[0]}'/>
                        <stop offset='100%' stop-color='{colors[1]}'/>
                    </linearGradient>
                    <filter id='b'>
                        <feGaussianBlur stdDeviation='20'/>
                    </filter>
                </defs>
                <rect fill='url(#g)' width='100%' height='100%'/>
                <image
                    href='data:image/webp;base64,{base64}'
                    width='100%'
                    height='100%'
                    filter='url(#b)'
                    preserveAspectRatio='none'/>
            </svg>".Trim();
    }
}
```

### Bandwidth-adaptive quality selection

Modern applications must adapt to varying network conditions, from 5G to congested public WiFi:

```csharp
public class AdaptiveImageLoader
{
    private readonly NetworkMonitor networkMonitor;
    private readonly HttpClient httpClient;

    public async Task<AdaptiveImage> LoadImageAsync(
        ImageSource source,
        ViewportInfo viewport,
        CancellationToken cancellationToken = default)
    {
        var networkInfo = await networkMonitor.GetNetworkInfoAsync();
        var strategy = DetermineLoadingStrategy(networkInfo, viewport, source);

        return strategy.Type switch
        {
            LoadingStrategyType.Progressive =>
                await LoadProgressiveAsync(source, strategy, cancellationToken),

            LoadingStrategyType.ResponsiveImages =>
                await LoadResponsiveAsync(source, strategy, cancellationToken),

            LoadingStrategyType.TiledProgressive =>
                await LoadTiledProgressiveAsync(source, strategy, cancellationToken),

            _ => throw new NotSupportedException()
        };
    }

    private LoadingStrategy DetermineLoadingStrategy(
        NetworkInfo network,
        ViewportInfo viewport,
        ImageSource source)
    {
        // Estimate bandwidth requirements
        var pixelsInViewport = viewport.Width * viewport.Height * viewport.PixelRatio;
        var estimatedSize = pixelsInViewport * source.BitsPerPixel / 8;
        var downloadTime = estimatedSize / network.EffectiveBandwidth;

        // Choose strategy based on conditions
        if (network.Type == NetworkType.Cellular && network.SaveData)
        {
            return new LoadingStrategy
            {
                Type = LoadingStrategyType.Progressive,
                InitialQuality = 0.1f,
                TargetQuality = 0.5f,
                ChunkSize = 16 * 1024 // 16KB chunks
            };
        }
        else if (downloadTime > 3.0) // More than 3 seconds
        {
            return new LoadingStrategy
            {
                Type = LoadingStrategyType.TiledProgressive,
                TileSize = 256,
                InitialTiles = GetViewportTiles(viewport, 256),
                PrefetchMargin = 1 // One tile border
            };
        }
        else if (network.EffectiveBandwidth > 10_000_000) // 10+ Mbps
        {
            return new LoadingStrategy
            {
                Type = LoadingStrategyType.ResponsiveImages,
                TargetFormat = "avif",
                FallbackFormat = "webp",
                Quality = 0.85f
            };
        }
        else
        {
            return new LoadingStrategy
            {
                Type = LoadingStrategyType.Progressive,
                InitialQuality = 0.3f,
                TargetQuality = 0.8f,
                ChunkSize = 64 * 1024 // 64KB chunks
            };
        }
    }

    // Adaptive streaming with quality adjustment
    private async Task<AdaptiveImage> LoadProgressiveAsync(
        ImageSource source,
        LoadingStrategy strategy,
        CancellationToken cancellationToken)
    {
        var result = new AdaptiveImage();
        var buffer = new MemoryStream();
        var decoder = new ProgressiveDecoder();

        // Configure adaptive streaming
        using var request = new HttpRequestMessage(HttpMethod.Get, source.Uri);
        request.Headers.Range = new RangeHeaderValue(0, strategy.ChunkSize);

        var bandwidthController = new BandwidthController(strategy.InitialQuality);

        while (!cancellationToken.IsCancellationRequested)
        {
            var chunkStart = buffer.Length;
            var chunkSize = bandwidthController.GetNextChunkSize();

            // Request next chunk
            request.Headers.Range = new RangeHeaderValue(chunkStart, chunkStart + chunkSize - 1);

            var stopwatch = Stopwatch.StartNew();
            using var response = await httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            // Copy to buffer and measure bandwidth
            await response.Content.CopyToAsync(buffer, cancellationToken);
            stopwatch.Stop();

            var bandwidth = chunkSize / stopwatch.Elapsed.TotalSeconds;
            bandwidthController.UpdateMeasurement(bandwidth);

            // Try to decode with current data
            buffer.Position = 0;
            if (decoder.TryDecodePartial(buffer, out var image))
            {
                result.CurrentImage = image;
                result.Quality = decoder.EstimateQuality();
                result.BytesLoaded = buffer.Length;

                // Notify update
                await OnImageUpdateAsync(result);

                // Check if quality target reached
                if (result.Quality >= strategy.TargetQuality)
                {
                    break;
                }
            }

            // Adaptive quality decision
            if (bandwidthController.ShouldReduceQuality())
            {
                strategy.TargetQuality = Math.Max(0.5f, strategy.TargetQuality - 0.1f);
            }
            else if (bandwidthController.ShouldIncreaseQuality())
            {
                strategy.TargetQuality = Math.Min(1.0f, strategy.TargetQuality + 0.1f);
            }
        }

        return result;
    }

    // Bandwidth measurement and adaptation
    private class BandwidthController
    {
        private readonly Queue<BandwidthMeasurement> measurements = new();
        private readonly TimeSpan measurementWindow = TimeSpan.FromSeconds(5);
        private float currentQuality;

        public int GetNextChunkSize()
        {
            var effectiveBandwidth = GetEffectiveBandwidth();

            // Target 0.5 second download time per chunk
            var targetSize = (int)(effectiveBandwidth * 0.5);

            // Clamp to reasonable range
            return Math.Clamp(targetSize, 16 * 1024, 1024 * 1024);
        }

        public void UpdateMeasurement(double bytesPerSecond)
        {
            measurements.Enqueue(new BandwidthMeasurement
            {
                Timestamp = DateTime.UtcNow,
                BytesPerSecond = bytesPerSecond
            });

            // Remove old measurements
            var cutoff = DateTime.UtcNow - measurementWindow;
            while (measurements.Count > 0 && measurements.Peek().Timestamp < cutoff)
            {
                measurements.Dequeue();
            }
        }

        private double GetEffectiveBandwidth()
        {
            if (measurements.Count == 0)
                return 1_000_000; // 1 Mbps default

            // Use harmonic mean for conservative estimate
            var harmonicSum = measurements.Sum(m => 1.0 / m.BytesPerSecond);
            return measurements.Count / harmonicSum;
        }
    }
}
```

### Priority-based viewport loading

Intelligent prioritization ensures users see the most important content first:

```csharp
public class ViewportPriorityLoader
{
    private readonly PriorityQueue<TileRequest, float> loadQueue;
    private readonly CancellationTokenSource cancellationSource;
    private readonly int maxConcurrentLoads;

    public async Task LoadSceneAsync(
        Scene scene,
        Viewport viewport,
        IProgress<LoadProgress> progress = null)
    {
        // Calculate tile priorities
        var tiles = CalculateTilePriorities(scene, viewport);

        // Enqueue all tiles with priorities
        foreach (var (tile, priority) in tiles)
        {
            loadQueue.Enqueue(new TileRequest(tile), priority);
        }

        // Process queue with limited concurrency
        using var semaphore = new SemaphoreSlim(maxConcurrentLoads);
        var tasks = new List<Task>();

        while (loadQueue.Count > 0 || tasks.Count > 0)
        {
            // Start new loads up to concurrency limit
            while (loadQueue.Count > 0 && semaphore.CurrentCount > 0)
            {
                await semaphore.WaitAsync();

                if (loadQueue.TryDequeue(out var request, out _))
                {
                    var task = LoadTileAsync(request, semaphore);
                    tasks.Add(task);
                }
            }

            // Wait for any task to complete
            if (tasks.Count > 0)
            {
                var completed = await Task.WhenAny(tasks);
                tasks.Remove(completed);

                // Report progress
                progress?.Report(new LoadProgress
                {
                    LoadedTiles = scene.LoadedTileCount,
                    TotalTiles = scene.TotalTileCount,
                    Percentage = scene.LoadedTileCount / (float)scene.TotalTileCount
                });
            }

            // Recompute priorities if viewport changed
            if (viewport.HasChanged)
            {
                ReprioritizeTiles(viewport);
            }
        }
    }

    private IEnumerable<(Tile, float)> CalculateTilePriorities(
        Scene scene,
        Viewport viewport)
    {
        foreach (var tile in scene.Tiles)
        {
            var priority = CalculateTilePriority(tile, viewport);
            yield return (tile, priority);
        }
    }

    private float CalculateTilePriority(Tile tile, Viewport viewport)
    {
        // Multiple factors contribute to priority
        var factors = new Dictionary<string, float>();

        // 1. Distance from viewport center (most important)
        var distance = Vector2.Distance(tile.Center, viewport.Center);
        factors["distance"] = 1f / (1f + distance / viewport.Radius);

        // 2. Intersection with viewport
        if (viewport.Intersects(tile.Bounds))
        {
            var intersectionArea = viewport.GetIntersectionArea(tile.Bounds);
            factors["intersection"] = intersectionArea / tile.Bounds.Area;
        }
        else
        {
            factors["intersection"] = 0f;
        }

        // 3. Predicted view direction
        if (viewport.Velocity.LengthSquared() > 0)
        {
            var predictedCenter = viewport.Center + viewport.Velocity * PredictionTime;
            var predictedDistance = Vector2.Distance(tile.Center, predictedCenter);
            factors["predicted"] = 1f / (1f + predictedDistance / viewport.Radius);
        }
        else
        {
            factors["predicted"] = 0f;
        }

        // 4. Tile importance (e.g., contains points of interest)
        factors["importance"] = tile.ImportanceScore;

        // 5. Already partially loaded
        if (tile.PartialData != null)
        {
            factors["partial"] = 0.5f + (tile.LoadedBytes / (float)tile.TotalBytes) * 0.5f;
        }
        else
        {
            factors["partial"] = 0f;
        }

        // Weighted combination
        var weights = new Dictionary<string, float>
        {
            ["distance"] = 0.3f,
            ["intersection"] = 0.3f,
            ["predicted"] = 0.2f,
            ["importance"] = 0.1f,
            ["partial"] = 0.1f
        };

        return factors.Sum(f => f.Value * weights[f.Key]);
    }

    // Dynamic reprioritization
    private void ReprioritizeTiles(Viewport newViewport)
    {
        var items = new List<(TileRequest request, float priority)>();

        // Extract all items
        while (loadQueue.TryDequeue(out var request, out var oldPriority))
        {
            var newPriority = CalculateTilePriority(request.Tile, newViewport);
            items.Add((request, newPriority));
        }

        // Re-enqueue with new priorities
        foreach (var (request, priority) in items.OrderByDescending(x => x.priority))
        {
            loadQueue.Enqueue(request, priority);
        }
    }

    // Intersection Observer pattern for web
    public class IntersectionObserverLoader
    {
        private readonly IJSRuntime jsRuntime;

        public async Task ObserveImagesAsync(
            IEnumerable<ImageElement> images,
            Action<ImageElement> onIntersection)
        {
            var dotNetRef = DotNetObjectReference.Create(
                new IntersectionCallback(onIntersection));

            await jsRuntime.InvokeVoidAsync(
                "imageLoader.observe",
                dotNetRef,
                images.Select(img => img.ElementId));
        }

        private class IntersectionCallback
        {
            private readonly Action<ImageElement> callback;

            public IntersectionCallback(Action<ImageElement> callback)
            {
                this.callback = callback;
            }

            [JSInvokable]
            public void OnIntersection(string elementId, bool isIntersecting, double intersectionRatio)
            {
                if (isIntersecting)
                {
                    var element = ImageElement.FindById(elementId);

                    // Prioritize based on intersection ratio
                    element.LoadPriority = intersectionRatio > 0.5
                        ? LoadPriority.High
                        : LoadPriority.Medium;

                    callback(element);
                }
            }
        }
    }
}
```

## 9.3 Pyramidal Image Structures

Pyramidal image structures provide the mathematical foundation for efficient multi-scale image processing, enabling
everything from smooth zooming in mapping applications to level-of-detail systems in 3D rendering.

### Gaussian and Laplacian pyramid construction

The Gaussian pyramid represents the cornerstone of scale-space theory, providing theoretically optimal lowpass
filtering:

```csharp
public class PyramidGenerator
{
    // Gaussian pyramid with optimal filter design
    public class GaussianPyramid
    {
        private readonly List<PyramidLevel> levels;
        private readonly float[] gaussianKernel;
        private readonly int kernelRadius;

        public GaussianPyramid(Image<Rgba32> baseImage, int maxLevels = -1)
        {
            // Generate optimal Gaussian kernel
            var sigma = 0.5f * Math.Sqrt(2.0f); // Nyquist-optimal
            (gaussianKernel, kernelRadius) = GenerateGaussianKernel(sigma);

            levels = new List<PyramidLevel>();
            BuildPyramid(baseImage, maxLevels);
        }

        private void BuildPyramid(Image<Rgba32> baseImage, int maxLevels)
        {
            var currentImage = baseImage.Clone();
            int level = 0;

            while (currentImage.Width > 1 && currentImage.Height > 1 &&
                   (maxLevels < 0 || level < maxLevels))
            {
                // Store current level
                levels.Add(new PyramidLevel
                {
                    Image = currentImage.Clone(),
                    Level = level,
                    Scale = (float)Math.Pow(2, level)
                });

                // Generate next level
                currentImage = DownsampleWithAntialiasing(currentImage);
                level++;
            }
        }

        private Image<Rgba32> DownsampleWithAntialiasing(Image<Rgba32> source)
        {
            var filtered = source.Clone();

            // Apply separable Gaussian filter
            ApplySeparableGaussianFilter(filtered);

            // Subsample by factor of 2
            var width = (source.Width + 1) / 2;
            var height = (source.Height + 1) / 2;
            var downsampled = new Image<Rgba32>(width, height);

            downsampled.ProcessPixelRows(source, (destAccessor, srcAccessor) =>
            {
                for (int y = 0; y < height; y++)
                {
                    var destRow = destAccessor.GetRowSpan(y);
                    var srcRow = srcAccessor.GetRowSpan(y * 2);

                    for (int x = 0; x < width; x++)
                    {
                        destRow[x] = srcRow[x * 2];
                    }
                }
            });

            filtered.Dispose();
            return downsampled;
        }

        private void ApplySeparableGaussianFilter(Image<Rgba32> image)
        {
            // Horizontal pass
            image.ProcessPixelRows(accessor =>
            {
                Parallel.For(0, accessor.Height, y =>
                {
                    var row = accessor.GetRowSpan(y);
                    var temp = new Rgba32[row.Length];

                    for (int x = 0; x < row.Length; x++)
                    {
                        var sum = Vector4.Zero;
                        var weightSum = 0f;

                        for (int k = -kernelRadius; k <= kernelRadius; k++)
                        {
                            var sampleX = Math.Clamp(x + k, 0, row.Length - 1);
                            var weight = gaussianKernel[k + kernelRadius];

                            sum += row[sampleX].ToVector4() * weight;
                            weightSum += weight;
                        }

                        temp[x] = new Rgba32(sum / weightSum);
                    }

                    temp.CopyTo(row);
                });
            });

            // Vertical pass (similar implementation)
            // ...
        }

        // Access methods with trilinear interpolation
        public Color SampleAtScale(float x, float y, float scale)
        {
            // Find bracketing levels
            var levelF = Math.Log2(scale);
            var level0 = (int)Math.Floor(levelF);
            var level1 = Math.Min(level0 + 1, levels.Count - 1);
            var alpha = levelF - level0;

            // Sample from both levels
            var color0 = SampleLevel(level0, x / (float)Math.Pow(2, level0),
                                            y / (float)Math.Pow(2, level0));
            var color1 = SampleLevel(level1, x / (float)Math.Pow(2, level1),
                                            y / (float)Math.Pow(2, level1));

            // Interpolate between levels
            return Color.Lerp(color0, color1, alpha);
        }
    }

    // Laplacian pyramid for detail preservation
    public class LaplacianPyramid
    {
        private readonly List<LaplacianLevel> levels;
        private readonly GaussianPyramid gaussianPyramid;

        public LaplacianPyramid(Image<Rgba32> baseImage)
        {
            gaussianPyramid = new GaussianPyramid(baseImage);
            levels = new List<LaplacianLevel>();
            BuildLaplacianPyramid();
        }

        private void BuildLaplacianPyramid()
        {
            for (int i = 0; i < gaussianPyramid.LevelCount - 1; i++)
            {
                var current = gaussianPyramid.GetLevel(i);
                var next = gaussianPyramid.GetLevel(i + 1);

                // Upsample next level
                var upsampled = UpsampleWithInterpolation(next, current.Width, current.Height);

                // Compute difference (bandpass filter result)
                var laplacian = new Image<Rgba32>(current.Width, current.Height);

                laplacian.ProcessPixelRows(current, upsampled, (laplacianAccessor, currentAccessor, upsampledAccessor) =>
                {
                    for (int y = 0; y < laplacianAccessor.Height; y++)
                    {
                        var laplacianRow = laplacianAccessor.GetRowSpan(y);
                        var currentRow = currentAccessor.GetRowSpan(y);
                        var upsampledRow = upsampledAccessor.GetRowSpan(y);

                        for (int x = 0; x < laplacianRow.Length; x++)
                        {
                            var diff = currentRow[x].ToVector4() - upsampledRow[x].ToVector4();

                            // Store with offset to handle negative values
                            laplacianRow[x] = new Rgba32((diff + Vector4.One) * 0.5f);
                        }
                    }
                });

                levels.Add(new LaplacianLevel
                {
                    DetailImage = laplacian,
                    Level = i
                });

                upsampled.Dispose();
            }

            // Store residual (lowest frequency)
            levels.Add(new LaplacianLevel
            {
                DetailImage = gaussianPyramid.GetLevel(gaussianPyramid.LevelCount - 1).Clone(),
                Level = gaussianPyramid.LevelCount - 1,
                IsResidual = true
            });
        }

        // Perfect reconstruction
        public Image<Rgba32> Reconstruct(int startLevel = 0)
        {
            // Start with residual
            var current = levels[levels.Count - 1].DetailImage.Clone();

            // Add details from coarse to fine
            for (int i = levels.Count - 2; i >= startLevel; i--)
            {
                var detail = levels[i].DetailImage;
                var upsampled = UpsampleWithInterpolation(current, detail.Width, detail.Height);

                current = new Image<Rgba32>(detail.Width, detail.Height);

                current.ProcessPixelRows(detail, upsampled, (currentAccessor, detailAccessor, upsampledAccessor) =>
                {
                    for (int y = 0; y < currentAccessor.Height; y++)
                    {
                        var currentRow = currentAccessor.GetRowSpan(y);
                        var detailRow = detailAccessor.GetRowSpan(y);
                        var upsampledRow = upsampledAccessor.GetRowSpan(y);

                        for (int x = 0; x < currentRow.Length; x++)
                        {
                            // Decode from offset representation
                            var decodedDetail = detailRow[x].ToVector4() * 2f - Vector4.One;
                            var sum = upsampledRow[x].ToVector4() + decodedDetail;

                            currentRow[x] = new Rgba32(Vector4.Clamp(sum, Vector4.Zero, Vector4.One));
                        }
                    }
                });

                upsampled.Dispose();
            }

            return current;
        }

        private Image<Rgba32> UpsampleWithInterpolation(Image<Rgba32> source, int targetWidth, int targetHeight)
        {
            var result = new Image<Rgba32>(targetWidth, targetHeight);

            result.ProcessPixelRows(accessor =>
            {
                Parallel.For(0, targetHeight, y =>
                {
                    var row = accessor.GetRowSpan(y);
                    var srcY = y / 2f;
                    var srcY0 = (int)srcY;
                    var srcY1 = Math.Min(srcY0 + 1, source.Height - 1);
                    var fy = srcY - srcY0;

                    for (int x = 0; x < targetWidth; x++)
                    {
                        var srcX = x / 2f;
                        var srcX0 = (int)srcX;
                        var srcX1 = Math.Min(srcX0 + 1, source.Width - 1);
                        var fx = srcX - srcX0;

                        // Bilinear interpolation
                        var p00 = source[srcX0, srcY0].ToVector4();
                        var p10 = source[srcX1, srcY0].ToVector4();
                        var p01 = source[srcX0, srcY1].ToVector4();
                        var p11 = source[srcX1, srcY1].ToVector4();

                        var p0 = Vector4.Lerp(p00, p10, fx);
                        var p1 = Vector4.Lerp(p01, p11, fx);
                        var result = Vector4.Lerp(p0, p1, fy);

                        row[x] = new Rgba32(result);
                    }
                });
            });

            return result;
        }
    }

    // Optimal kernel generation
    private static (float[] kernel, int radius) GenerateGaussianKernel(float sigma)
    {
        // Kernel radius for 99.7% of distribution
        int radius = (int)Math.Ceiling(sigma * 3);
        var kernel = new float[radius * 2 + 1];

        float sum = 0;
        float twoSigmaSquared = 2 * sigma * sigma;

        for (int i = -radius; i <= radius; i++)
        {
            kernel[i + radius] = MathF.Exp(-(i * i) / twoSigmaSquared);
            sum += kernel[i + radius];
        }

        // Normalize
        for (int i = 0; i < kernel.Length; i++)
        {
            kernel[i] /= sum;
        }

        return (kernel, radius);
    }
}
```

### Mipmap generation and GPU acceleration

Modern GPUs provide hardware-accelerated mipmap generation, crucial for real-time rendering performance:

```csharp
public class GPUMipmapGenerator
{
    private readonly GraphicsDevice device;
    private readonly ComputeShader downsampleShader;

    public Texture2D GenerateMipmaps(Texture2D sourceTexture, MipmapFilter filter = MipmapFilter.Kaiser)
    {
        var mipLevels = CalculateMipLevels(sourceTexture.Width, sourceTexture.Height);

        var mipmappedTexture = new Texture2D(device, new TextureDescription
        {
            Width = sourceTexture.Width,
            Height = sourceTexture.Height,
            MipLevels = mipLevels,
            ArraySize = 1,
            Format = sourceTexture.Format,
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None
        });

        // Copy base level
        device.ImmediateContext.CopySubresourceRegion(
            sourceTexture, 0, null,
            mipmappedTexture, 0, 0, 0, 0);

        // Generate mips using compute shader
        using (var commandList = device.CreateCommandList())
        {
            for (int level = 1; level < mipLevels; level++)
            {
                var sourceLevel = level - 1;
                var srcWidth = Math.Max(1, sourceTexture.Width >> sourceLevel);
                var srcHeight = Math.Max(1, sourceTexture.Height >> sourceLevel);
                var dstWidth = Math.Max(1, sourceTexture.Width >> level);
                var dstHeight = Math.Max(1, sourceTexture.Height >> level);

                GenerateMipLevel(commandList, mipmappedTexture,
                    sourceLevel, level,
                    srcWidth, srcHeight,
                    dstWidth, dstHeight,
                    filter);
            }

            commandList.Close();
            device.ImmediateContext.ExecuteCommandList(commandList);
        }

        return mipmappedTexture;
    }

    private void GenerateMipLevel(
        CommandList commandList,
        Texture2D texture,
        int sourceLevel,
        int destLevel,
        int srcWidth, int srcHeight,
        int dstWidth, int dstHeight,
        MipmapFilter filter)
    {
        // Set compute shader
        commandList.SetComputeShader(downsampleShader);

        // Create views for source and destination levels
        using var srcView = new ShaderResourceView(device, texture,
            new ShaderResourceViewDescription
            {
                Format = texture.Format,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new Texture2DShaderResourceView
                {
                    MipLevels = 1,
                    MostDetailedMip = sourceLevel
                }
            });

        using var dstView = new UnorderedAccessView(device, texture,
            new UnorderedAccessViewDescription
            {
                Format = texture.Format,
                Dimension = UnorderedAccessViewDimension.Texture2D,
                Texture2D = new Texture2DUnorderedAccessView
                {
                    MipSlice = destLevel
                }
            });

        // Set resources
        commandList.SetComputeShaderResource(0, srcView);
        commandList.SetComputeUnorderedAccessView(0, dstView);

        // Set constants
        var constants = new MipmapConstants
        {
            SrcDimensions = new Vector4(srcWidth, srcHeight, 1f / srcWidth, 1f / srcHeight),
            DstDimensions = new Vector4(dstWidth, dstHeight, 1f / dstWidth, 1f / dstHeight),
            FilterType = (int)filter
        };

        commandList.SetComputeConstantBuffer(0, constants);

        // Dispatch
        int threadGroupsX = (dstWidth + 7) / 8;
        int threadGroupsY = (dstHeight + 7) / 8;
        commandList.Dispatch(threadGroupsX, threadGroupsY, 1);

        // Barrier for next level
        commandList.ResourceBarrier(texture, ResourceStates.UnorderedAccess, ResourceStates.ShaderResource);
    }

    // High-quality downsampling compute shader
    [ComputeShader("MipmapGeneration.hlsl")]
    private const string MipmapShader = @"
        cbuffer Constants : register(b0)
        {
            float4 SrcDimensions; // xy: size, zw: 1/size
            float4 DstDimensions;
            int FilterType;
        }

        Texture2D<float4> SrcTexture : register(t0);
        RWTexture2D<float4> DstTexture : register(u0);

        SamplerState LinearClampSampler : register(s0);

        // Kaiser filter for high quality
        float KaiserFilter(float x, float alpha)
        {
            if (abs(x) >= 1.0)
                return 0.0;

            float x2 = x * x;
            float response = sinh(alpha * sqrt(1.0 - x2)) / (alpha * sqrt(1.0 - x2));
            return response;
        }

        [numthreads(8, 8, 1)]
        void CSMain(uint3 id : SV_DispatchThreadID)
        {
            if (any(id.xy >= uint2(DstDimensions.xy)))
                return;

            float2 texCoord = (float2(id.xy) + 0.5) * DstDimensions.zw;

            float4 result = float4(0, 0, 0, 0);

            if (FilterType == 0) // Box filter (fastest)
            {
                result = SrcTexture.SampleLevel(LinearClampSampler, texCoord, 0);
            }
            else if (FilterType == 1) // Kaiser filter (highest quality)
            {
                const float alpha = 3.0;
                const int radius = 2;
                float weightSum = 0.0;

                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        float2 offset = float2(x, y) * SrcDimensions.zw;
                        float2 samplePos = texCoord + offset;

                        float weight = KaiserFilter(length(float2(x, y)) / float(radius), alpha);

                        result += SrcTexture.SampleLevel(LinearClampSampler, samplePos, 0) * weight;
                        weightSum += weight;
                    }
                }

                result /= weightSum;
            }
            else if (FilterType == 2) // Lanczos (good quality/performance)
            {
                // Lanczos implementation...
            }

            DstTexture[id.xy] = result;
        }
    ";

    // Performance comparison
    public async Task<MipmapBenchmark> BenchmarkMipmapGenerationAsync(Texture2D texture)
    {
        var results = new MipmapBenchmark();

        // CPU implementation
        var cpuStopwatch = Stopwatch.StartNew();
        var cpuMips = GenerateMipmapsCPU(texture);
        cpuStopwatch.Stop();
        results.CPUTime = cpuStopwatch.Elapsed;

        // GPU box filter
        var gpuBoxStopwatch = Stopwatch.StartNew();
        var gpuBoxMips = GenerateMipmaps(texture, MipmapFilter.Box);
        await device.WaitForIdleAsync();
        gpuBoxStopwatch.Stop();
        results.GPUBoxTime = gpuBoxStopwatch.Elapsed;

        // GPU Kaiser filter
        var gpuKaiserStopwatch = Stopwatch.StartNew();
        var gpuKaiserMips = GenerateMipmaps(texture, MipmapFilter.Kaiser);
        await device.WaitForIdleAsync();
        gpuKaiserStopwatch.Stop();
        results.GPUKaiserTime = gpuKaiserStopwatch.Elapsed;

        // Hardware mipmap generation (if available)
        if (device.Features.HardwareMipmapGeneration)
        {
            var hwStopwatch = Stopwatch.StartNew();
            texture.GenerateMips(device.ImmediateContext);
            await device.WaitForIdleAsync();
            hwStopwatch.Stop();
            results.HardwareTime = hwStopwatch.Elapsed;
        }

        return results;
    }
}
```

### Cloud-Optimized GeoTIFF format

COG represents a breakthrough in cloud-native geospatial imaging, organizing massive TIFF files for efficient HTTP
access:

```csharp
public class CloudOptimizedGeoTIFF
{
    private readonly HttpClient httpClient;
    private readonly TIFFReader reader;
    private readonly COGMetadata metadata;

    public class COGMetadata
    {
        public int Width { get; init; }
        public int Height { get; init; }
        public int TileWidth { get; init; }
        public int TileHeight { get; init; }
        public List<IFDOffset> Overviews { get; init; }
        public Dictionary<int, long> TileOffsets { get; init; }
        public Dictionary<int, long> TileByteCounts { get; init; }
        public CompressionType Compression { get; init; }
    }

    public async Task<COGMetadata> ReadMetadataAsync(Uri cogUri)
    {
        // Read header and IFDs with minimal requests
        var headerSize = 16384; // Usually sufficient for header + IFDs

        using var request = new HttpRequestMessage(HttpMethod.Get, cogUri);
        request.Headers.Range = new RangeHeaderValue(0, headerSize - 1);

        using var response = await httpClient.SendAsync(request);
        var headerData = await response.Content.ReadAsByteArrayAsync();

        return ParseCOGStructure(headerData, response.Content.Headers.ContentRange.Length.Value);
    }

    public async Task<TileData> ReadTileAsync(int level, int col, int row)
    {
        var ifd = level == 0 ? metadata : metadata.Overviews[level - 1];
        var tilesPerRow = (ifd.Width + ifd.TileWidth - 1) / ifd.TileWidth;
        var tileIndex = row * tilesPerRow + col;

        // Get tile offset and size from metadata
        var offset = ifd.TileOffsets[tileIndex];
        var size = ifd.TileByteCounts[tileIndex];

        // Single range request for tile data
        using var request = new HttpRequestMessage(HttpMethod.Get, cogUri);
        request.Headers.Range = new RangeHeaderValue(offset, offset + size - 1);

        using var response = await httpClient.SendAsync(request);
        var compressedData = await response.Content.ReadAsByteArrayAsync();

        // Decompress based on compression type
        return await DecompressTileAsync(compressedData, ifd.Compression);
    }

    // Efficient multi-tile reading
    public async IAsyncEnumerable<TileData> ReadTilesAsync(
        Rectangle region,
        int level,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var ifd = level == 0 ? metadata : metadata.Overviews[level - 1];

        // Calculate tile bounds
        var startCol = region.Left / ifd.TileWidth;
        var endCol = (region.Right + ifd.TileWidth - 1) / ifd.TileWidth;
        var startRow = region.Top / ifd.TileHeight;
        var endRow = (region.Bottom + ifd.TileHeight - 1) / ifd.TileHeight;

        // Group adjacent tiles for multi-range requests
        var tileGroups = GroupAdjacentTiles(startCol, endCol, startRow, endRow);

        foreach (var group in tileGroups)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            // Multi-range request for efficiency
            using var request = new HttpRequestMessage(HttpMethod.Get, cogUri);

            var ranges = new List<RangeItemHeaderValue>();
            foreach (var (col, row) in group)
            {
                var tilesPerRow = (ifd.Width + ifd.TileWidth - 1) / ifd.TileWidth;
                var tileIndex = row * tilesPerRow + col;
                var offset = ifd.TileOffsets[tileIndex];
                var size = ifd.TileByteCounts[tileIndex];

                ranges.Add(new RangeItemHeaderValue(offset, offset + size - 1));
            }

            request.Headers.Range = new RangeHeaderValue(ranges);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.Content.Headers.ContentType.MediaType == "multipart/byteranges")
            {
                // Parse multipart response
                await foreach (var part in ParseMultipartResponse(response, cancellationToken))
                {
                    yield return await DecompressTileAsync(part.Data, ifd.Compression);
                }
            }
            else
            {
                // Single part response
                var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                yield return await DecompressTileAsync(data, ifd.Compression);
            }
        }
    }

    // COG validation and optimization
    public class COGValidator
    {
        public ValidationResult Validate(string filePath)
        {
            var result = new ValidationResult();

            using var file = TIFFReader.Open(filePath);

            // Check tile organization
            if (!file.IsTiled)
            {
                result.AddError("Image must be tiled, not stripped");
            }

            // Check tile size
            if (file.TileWidth % 16 != 0 || file.TileHeight % 16 != 0)
            {
                result.AddWarning("Tile dimensions should be multiples of 16");
            }

            if (file.TileWidth != file.TileHeight)
            {
                result.AddWarning("Square tiles recommended for optimal performance");
            }

            // Check overview presence
            if (file.OverviewCount == 0)
            {
                result.AddError("Overviews are required for COG");
            }
            else
            {
                // Validate overview scales
                var expectedScale = 2;
                for (int i = 0; i < file.OverviewCount; i++)
                {
                    var overview = file.GetOverview(i);
                    var scale = file.Width / overview.Width;

                    if (Math.Abs(scale - expectedScale) > 0.1)
                    {
                        result.AddWarning($"Overview {i} has unexpected scale {scale}, expected {expectedScale}");
                    }

                    expectedScale *= 2;
                }
            }

            // Check data organization
            if (!AreIFDsAtEnd(file))
            {
                result.AddError("IFDs must be at end of file for HTTP optimization");
            }

            // Check compression
            if (file.Compression == CompressionType.None)
            {
                result.AddWarning("Compression recommended for bandwidth efficiency");
            }

            return result;
        }

        private bool AreIFDsAtEnd(TIFFReader file)
        {
            // IFDs should be after all image data
            var lastDataOffset = file.GetLastTileOffset() + file.GetLastTileSize();
            var firstIFDOffset = file.GetIFDOffset(0);

            return firstIFDOffset > lastDataOffset;
        }
    }

    // COG creation with gdal_translate equivalent
    public async Task CreateCOGAsync(
        string sourcePath,
        string outputPath,
        COGCreationOptions options = null)
    {
        options ??= new COGCreationOptions();

        using var source = await Image.LoadAsync<Rgba32>(sourcePath);
        using var output = TIFFWriter.Create(outputPath);

        // Configure COG-compliant settings
        output.TileWidth = options.TileSize;
        output.TileHeight = options.TileSize;
        output.Compression = options.Compression;
        output.PhotometricInterpretation = PhotometricInterpretation.RGB;

        // Write main image
        await output.WriteImageAsync(source);

        // Generate overviews
        var currentLevel = source;
        var overviewCount = CalculateOverviewLevels(source.Width, source.Height, options.TileSize);

        for (int i = 0; i < overviewCount; i++)
        {
            var scale = (int)Math.Pow(2, i + 1);
            var overviewWidth = Math.Max(1, source.Width / scale);
            var overviewHeight = Math.Max(1, source.Height / scale);

            using var overview = currentLevel.Clone(ctx => ctx
                .Resize(new ResizeOptions
                {
                    Size = new Size(overviewWidth, overviewHeight),
                    Sampler = KnownResamplers.Lanczos3,
                    Compand = true
                }));

            await output.WriteOverviewAsync(overview, i);
            currentLevel = overview;
        }

        // Reorder file structure for COG compliance
        await output.ReorderForCloudOptimizationAsync();
    }

    private static int CalculateOverviewLevels(int width, int height, int tileSize)
    {
        var maxDimension = Math.Max(width, height);
        return (int)Math.Ceiling(Math.Log2(maxDimension / (double)tileSize));
    }
}
```

### Wavelet-based pyramid decomposition

Wavelets provide superior energy compaction compared to traditional pyramids, enabling better compression and
progressive transmission:

```csharp
public class WaveletPyramid
{
    // Haar wavelet for simplicity, extendable to Daubechies, CDF, etc.
    public class HaarWaveletTransform
    {
        public WaveletDecomposition Decompose(Image<L16> image, int levels)
        {
            var width = image.Width;
            var height = image.Height;
            var data = new float[width * height];

            // Convert to float array
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < width; x++)
                    {
                        data[y * width + x] = row[x].PackedValue / 65535f;
                    }
                }
            });

            var decomposition = new WaveletDecomposition
            {
                Width = width,
                Height = height,
                Levels = levels,
                Coefficients = data
            };

            // Perform 2D wavelet transform
            for (int level = 0; level < levels; level++)
            {
                var levelWidth = width >> level;
                var levelHeight = height >> level;

                // Horizontal transform
                HorizontalTransform(data, width, levelWidth, levelHeight);

                // Vertical transform
                VerticalTransform(data, width, levelWidth, levelHeight);
            }

            return decomposition;
        }

        private void HorizontalTransform(float[] data, int stride, int width, int height)
        {
            var temp = new float[width];

            for (int y = 0; y < height; y++)
            {
                var offset = y * stride;

                // Copy row to temp buffer
                for (int x = 0; x < width; x++)
                {
                    temp[x] = data[offset + x];
                }

                // Haar transform
                int half = width / 2;
                for (int i = 0; i < half; i++)
                {
                    var a = temp[2 * i];
                    var b = temp[2 * i + 1];

                    // Low frequency (average)
                    data[offset + i] = (a + b) * 0.5f;

                    // High frequency (difference)
                    data[offset + half + i] = (a - b) * 0.5f;
                }
            }
        }

        private void VerticalTransform(float[] data, int stride, int width, int height)
        {
            var temp = new float[height];

            for (int x = 0; x < width; x++)
            {
                // Copy column to temp buffer
                for (int y = 0; y < height; y++)
                {
                    temp[y] = data[y * stride + x];
                }

                // Haar transform
                int half = height / 2;
                for (int i = 0; i < half; i++)
                {
                    var a = temp[2 * i];
                    var b = temp[2 * i + 1];

                    // Low frequency (average)
                    data[i * stride + x] = (a + b) * 0.5f;

                    // High frequency (difference)
                    data[(half + i) * stride + x] = (a - b) * 0.5f;
                }
            }
        }

        public Image<L16> Reconstruct(WaveletDecomposition decomposition)
        {
            var data = (float[])decomposition.Coefficients.Clone();
            var width = decomposition.Width;
            var height = decomposition.Height;

            // Inverse transform from coarsest to finest level
            for (int level = decomposition.Levels - 1; level >= 0; level--)
            {
                var levelWidth = width >> level;
                var levelHeight = height >> level;

                // Vertical inverse transform
                VerticalInverseTransform(data, width, levelWidth, levelHeight);

                // Horizontal inverse transform
                HorizontalInverseTransform(data, width, levelWidth, levelHeight);
            }

            // Convert back to image
            var image = new Image<L16>(width, height);

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < width; x++)
                    {
                        var value = Math.Clamp(data[y * width + x], 0f, 1f);
                        row[x] = new L16((ushort)(value * 65535));
                    }
                }
            });

            return image;
        }
    }

    // Lifting scheme for integer wavelets (lossless)
    public class LiftingSchemeWavelet
    {
        public void Forward5_3Transform(int[] data, int width, int height)
        {
            // CDF 5/3 wavelet using lifting scheme
            // Used in lossless JPEG 2000

            // Horizontal pass
            for (int y = 0; y < height; y++)
            {
                Predict5_3(data, y * width, width, 1);
                Update5_3(data, y * width, width, 1);
            }

            // Vertical pass
            for (int x = 0; x < width; x++)
            {
                Predict5_3(data, x, height, width);
                Update5_3(data, x, height, width);
            }
        }

        private void Predict5_3(int[] data, int offset, int length, int stride)
        {
            int half = length / 2;

            // Predict odd samples from even samples
            for (int i = 1; i < length - 1; i += 2)
            {
                data[offset + i * stride] -= (data[offset + (i - 1) * stride] +
                                              data[offset + (i + 1) * stride]) >> 1;
            }

            // Handle boundary
            if (length % 2 == 0)
            {
                data[offset + (length - 1) * stride] -= data[offset + (length - 2) * stride];
            }
        }

        private void Update5_3(int[] data, int offset, int length, int stride)
        {
            // Update even samples using predicted odd samples
            for (int i = 2; i < length; i += 2)
            {
                data[offset + i * stride] += (data[offset + (i - 1) * stride] +
                                              data[offset + (i + 1) * stride] + 2) >> 2;
            }

            // Handle boundaries
            data[offset] += (data[offset + stride] + 1) >> 1;
        }
    }

    // Progressive transmission using wavelet coefficients
    public class ProgressiveWaveletTransmitter
    {
        public async IAsyncEnumerable<ProgressiveUpdate> TransmitProgressivelyAsync(
            WaveletDecomposition decomposition,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var width = decomposition.Width;
            var height = decomposition.Height;
            var levels = decomposition.Levels;

            // Start with lowest resolution (LL band of highest decomposition level)
            var llSize = 1 << (levels - 1);
            var llBand = ExtractLLBand(decomposition, levels - 1);

            yield return new ProgressiveUpdate
            {
                Data = llBand,
                Level = levels - 1,
                Band = WaveletBand.LL,
                Quality = 0.1f
            };

            // Progressively send detail bands from coarse to fine
            for (int level = levels - 1; level >= 0; level--)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                // Send LH, HL, HH bands for this level
                var bands = new[] { WaveletBand.LH, WaveletBand.HL, WaveletBand.HH };

                foreach (var band in bands)
                {
                    var bandData = ExtractBand(decomposition, level, band);

                    // Quantize for compression
                    var quantized = QuantizeBand(bandData, level, band);

                    yield return new ProgressiveUpdate
                    {
                        Data = quantized,
                        Level = level,
                        Band = band,
                        Quality = CalculateQuality(level, levels)
                    };

                    // Simulate network delay
                    await Task.Delay(10, cancellationToken);
                }
            }
        }

        private float[] ExtractBand(WaveletDecomposition decomposition, int level, WaveletBand band)
        {
            var width = decomposition.Width;
            var height = decomposition.Height;
            var data = decomposition.Coefficients;

            var bandWidth = width >> level;
            var bandHeight = height >> level;

            var (xOffset, yOffset) = band switch
            {
                WaveletBand.LL => (0, 0),
                WaveletBand.LH => (0, bandHeight / 2),
                WaveletBand.HL => (bandWidth / 2, 0),
                WaveletBand.HH => (bandWidth / 2, bandHeight / 2),
                _ => throw new ArgumentException()
            };

            var extractedWidth = bandWidth / 2;
            var extractedHeight = bandHeight / 2;
            var extracted = new float[extractedWidth * extractedHeight];

            for (int y = 0; y < extractedHeight; y++)
            {
                for (int x = 0; x < extractedWidth; x++)
                {
                    var srcX = xOffset + x;
                    var srcY = yOffset + y;
                    extracted[y * extractedWidth + x] = data[srcY * width + srcX];
                }
            }

            return extracted;
        }
    }
}
```

## 9.4 HTTP Range Request Optimization

HTTP range requests transform how we deliver large images over networks, enabling partial content retrieval that makes
gigapixel imagery practical for web delivery. This section explores advanced techniques for optimizing these requests in
.NET 9.0.

### Implementing efficient byte-range strategies

HTTP range requests allow clients to request specific byte ranges of a resource, crucial for streaming large images
without downloading entire files:

```csharp
public class RangeRequestHandler
{
    private readonly HttpClient httpClient;
    private readonly IMemoryCache cache;
    private readonly RangeRequestOptions options;

    public class RangeRequestOptions
    {
        public int MaxChunkSize { get; set; } = 1024 * 1024; // 1MB default
        public int MinChunkSize { get; set; } = 64 * 1024;   // 64KB minimum
        public int MaxConcurrentRequests { get; set; } = 6;
        public TimeSpan ChunkTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public bool EnableMultiRange { get; set; } = true;
    }

    public async Task<Stream> GetRangeAsync(
        Uri resourceUri,
        long start,
        long end,
        CancellationToken cancellationToken = default)
    {
        // Check if server supports range requests
        var capabilities = await GetServerCapabilitiesAsync(resourceUri);

        if (!capabilities.AcceptsRanges)
        {
            throw new NotSupportedException("Server does not support range requests");
        }

        // Optimize request strategy based on range size
        var rangeSize = end - start + 1;

        if (rangeSize <= options.MaxChunkSize)
        {
            // Single range request
            return await GetSingleRangeAsync(resourceUri, start, end, cancellationToken);
        }
        else if (options.EnableMultiRange && capabilities.SupportsMultipartRanges)
        {
            // Multi-range request for better efficiency
            return await GetMultiRangeAsync(resourceUri, start, end, cancellationToken);
        }
        else
        {
            // Chunked download with parallel requests
            return await GetChunkedRangeAsync(resourceUri, start, end, cancellationToken);
        }
    }

    private async Task<Stream> GetSingleRangeAsync(
        Uri resourceUri,
        long start,
        long end,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, resourceUri);
        request.Headers.Range = new RangeHeaderValue(start, end);

        var response = await httpClient.SendAsync(request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.PartialContent)
        {
            throw new HttpRequestException($"Expected 206 Partial Content, got {response.StatusCode}");
        }

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    private async Task<Stream> GetMultiRangeAsync(
        Uri resourceUri,
        long start,
        long end,
        CancellationToken cancellationToken)
    {
        // Calculate optimal chunk boundaries
        var chunks = CalculateOptimalChunks(start, end);

        using var request = new HttpRequestMessage(HttpMethod.Get, resourceUri);

        // Add multiple ranges to single request
        var ranges = chunks.Select(c => new RangeItemHeaderValue(c.Start, c.End));
        request.Headers.Range = new RangeHeaderValue(ranges);

        var response = await httpClient.SendAsync(request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.Content.Headers.ContentType?.MediaType == "multipart/byteranges")
        {
            // Parse multipart response
            return await ParseMultipartRangeResponseAsync(response, cancellationToken);
        }
        else
        {
            // Server may have combined ranges
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }
    }

    private async Task<Stream> GetChunkedRangeAsync(
        Uri resourceUri,
        long start,
        long end,
        CancellationToken cancellationToken)
    {
        var result = new MemoryStream();
        var chunks = CalculateOptimalChunks(start, end);

        // Use semaphore to limit concurrent requests
        using var semaphore = new SemaphoreSlim(options.MaxConcurrentRequests);

        // Process chunks in parallel
        await Parallel.ForEachAsync(
            chunks,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = options.MaxConcurrentRequests
            },
            async (chunk, ct) =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    var chunkData = await GetSingleRangeAsync(
                        resourceUri,
                        chunk.Start,
                        chunk.End,
                        ct);

                    // Thread-safe write to result
                    lock (result)
                    {
                        result.Position = chunk.Start - start;
                        await chunkData.CopyToAsync(result, ct);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

        result.Position = 0;
        return result;
    }

    private List<(long Start, long End)> CalculateOptimalChunks(long start, long end)
    {
        var chunks = new List<(long Start, long End)>();
        var totalSize = end - start + 1;

        // Determine optimal chunk size based on total range
        var chunkSize = totalSize switch
        {
            < 1024 * 1024 => options.MinChunkSize,        // < 1MB: use minimum
            < 10 * 1024 * 1024 => 256 * 1024,            // < 10MB: 256KB chunks
            < 100 * 1024 * 1024 => 512 * 1024,           // < 100MB: 512KB chunks
            _ => options.MaxChunkSize                      // > 100MB: max chunk size
        };

        // Align chunks to boundaries for better caching
        var alignedChunkSize = AlignToBlockBoundary(chunkSize);

        for (long pos = start; pos <= end; pos += alignedChunkSize)
        {
            var chunkEnd = Math.Min(pos + alignedChunkSize - 1, end);
            chunks.Add((pos, chunkEnd));
        }

        return chunks;
    }

    private int AlignToBlockBoundary(int size)
    {
        const int blockSize = 4096; // Common filesystem block size
        return ((size + blockSize - 1) / blockSize) * blockSize;
    }

    // HTTP/2 and HTTP/3 optimization
    public class Http2RangeOptimizer
    {
        private readonly SocketsHttpHandler handler;

        public Http2RangeOptimizer()
        {
            handler = new SocketsHttpHandler
            {
                // Enable HTTP/2 and HTTP/3
                EnableMultipleHttp2Connections = true,
                MaxConnectionsPerServer = 256,

                // Connection pooling
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),

                // Request version policy
                RequestVersionOrHigher = HttpVersion.Version20,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
            };

            // Configure for HTTP/3 if available
            if (HttpVersion.Version30.Major >= 3)
            {
                handler.RequestVersionOrHigher = HttpVersion.Version30;
            }
        }

        public async Task<List<byte[]>> GetMultipleRangesAsync(
            Uri resourceUri,
            List<(long Start, long End)> ranges,
            CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient(handler);

            // Use HTTP/2 multiplexing for parallel requests
            var tasks = ranges.Select(async range =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, resourceUri);
                request.Headers.Range = new RangeHeaderValue(range.Start, range.End);
                request.Version = HttpVersion.Version20;

                var response = await client.SendAsync(request,
                    HttpCompletionOption.ResponseContentRead,
                    cancellationToken);

                return await response.Content.ReadAsByteArrayAsync(cancellationToken);
            });

            return (await Task.WhenAll(tasks)).ToList();
        }

        // HTTP/3 with QUIC for improved mobile performance
        public async Task<Stream> GetRangeWithQuicAsync(
            Uri resourceUri,
            long start,
            long end,
            CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient(new SocketsHttpHandler
            {
                RequestVersionOrHigher = HttpVersion.Version30,
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,

                // QUIC-specific settings
                QuicImplementationProvider = QuicImplementationProviders.MsQuic,
                EnableMultipleHttp3Connections = true
            });

            var request = new HttpRequestMessage(HttpMethod.Get, resourceUri)
            {
                Version = HttpVersion.Version30,
                Headers = { Range = new RangeHeaderValue(start, end) }
            };

            var response = await client.SendAsync(request, cancellationToken);
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }
    }
}
```

### CDN integration and edge caching

Content Delivery Networks dramatically improve tile delivery performance through geographic distribution and intelligent
caching:

```csharp
public class CDNOptimizedTileService
{
    private readonly Dictionary<string, CDNProvider> providers;
    private readonly ITelemetryClient telemetry;

    public interface ICDNProvider
    {
        Task<TileResponse> GetTileAsync(TileRequest request);
        Task WarmCacheAsync(IEnumerable<TileRequest> tiles);
        Task InvalidateAsync(string pattern);
        CDNMetrics GetMetrics();
    }

    // Cloudflare Workers integration
    public class CloudflareProvider : ICDNProvider
    {
        private readonly HttpClient httpClient;
        private readonly CloudflareOptions options;

        public async Task<TileResponse> GetTileAsync(TileRequest request)
        {
            var url = BuildCDNUrl(request);

            // Add cache control headers
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Add("CF-Cache-Control", "max-age=31536000");
            httpRequest.Headers.Add("CF-Cache-Key", GenerateCacheKey(request));

            var response = await httpClient.SendAsync(httpRequest);

            // Extract CDN metrics from headers
            var metrics = new CDNMetrics
            {
                CacheStatus = response.Headers.GetValues("CF-Cache-Status").FirstOrDefault(),
                EdgeLocation = response.Headers.GetValues("CF-Ray").FirstOrDefault(),
                ResponseTime = response.Headers.Age?.TotalMilliseconds ?? 0
            };

            return new TileResponse
            {
                Data = await response.Content.ReadAsByteArrayAsync(),
                Metrics = metrics,
                Headers = response.Headers
            };
        }

        public async Task WarmCacheAsync(IEnumerable<TileRequest> tiles)
        {
            // Use Workers KV for tile metadata
            var manifest = new TileManifest
            {
                Tiles = tiles.Select(t => new TileEntry
                {
                    Key = GenerateCacheKey(t),
                    Url = BuildCDNUrl(t),
                    Priority = t.Priority,
                    Size = t.EstimatedSize
                }).ToList()
            };

            // Deploy to Workers KV
            await DeployManifestToWorkersKV(manifest);

            // Trigger cache warming through Workers
            await TriggerCacheWarmingWorker(manifest);
        }

        private string GenerateCacheKey(TileRequest request)
        {
            // Include all parameters that affect tile content
            var keyComponents = new[]
            {
                request.Layer,
                request.Z.ToString(),
                request.X.ToString(),
                request.Y.ToString(),
                request.Format,
                request.Scale?.ToString() ?? "1",
                request.Style ?? "default"
            };

            return string.Join("/", keyComponents);
        }

        // Cloudflare Worker script for intelligent caching
        private const string WorkerScript = @"
            addEventListener('fetch', event => {
                event.respondWith(handleRequest(event.request))
            })

            async function handleRequest(request) {
                const cache = caches.default
                const cacheKey = new Request(request.url, request)

                // Check cache
                let response = await cache.match(cacheKey)

                if (response) {
                    // Cache hit - add analytics
                    const newHeaders = new Headers(response.headers)
                    newHeaders.set('CF-Cache-Status', 'HIT')
                    newHeaders.set('X-Cache-Age', getAgeSeconds(response))

                    return new Response(response.body, {
                        status: response.status,
                        statusText: response.statusText,
                        headers: newHeaders
                    })
                }

                // Cache miss - fetch from origin
                response = await fetch(request)

                // Cache successful responses
                if (response.status === 200) {
                    const headers = new Headers(response.headers)
                    headers.set('CF-Cache-Status', 'MISS')
                    headers.set('Cache-Control', 'public, max-age=31536000')

                    // Clone response for caching
                    const responseToCache = new Response(response.body, {
                        status: response.status,
                        statusText: response.statusText,
                        headers: headers
                    })

                    // Don't block on cache write
                    event.waitUntil(cache.put(cacheKey, responseToCache.clone()))

                    return responseToCache
                }

                return response
            }

            // Prefetch adjacent tiles
            async function prefetchAdjacent(tileX, tileY, tileZ) {
                const adjacentTiles = [
                    [tileX - 1, tileY], [tileX + 1, tileY],
                    [tileX, tileY - 1], [tileX, tileY + 1]
                ]

                const prefetchPromises = adjacentTiles.map(([x, y]) => {
                    const url = `/tiles/${tileZ}/${x}/${y}.png`
                    return cache.match(url).then(cached => {
                        if (!cached) {
                            return fetch(url).then(response => {
                                if (response.status === 200) {
                                    return cache.put(url, response)
                                }
                            })
                        }
                    })
                })

                await Promise.all(prefetchPromises)
            }
        ";
    }

    // Multi-CDN strategy for redundancy
    public class MultiCDNStrategy
    {
        private readonly List<ICDNProvider> providers;
        private readonly IHealthChecker healthChecker;

        public async Task<TileResponse> GetTileWithFallbackAsync(TileRequest request)
        {
            var healthyProviders = await GetHealthyProvidersAsync();

            foreach (var provider in healthyProviders.OrderBy(p => p.Latency))
            {
                try
                {
                    var response = await provider.GetTileAsync(request)
                        .WaitAsync(TimeSpan.FromSeconds(5));

                    if (response.Success)
                    {
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    // Log and try next provider
                    await LogProviderFailureAsync(provider, ex);
                }
            }

            // All CDNs failed - fallback to origin
            return await GetFromOriginAsync(request);
        }

        private async Task<List<CDNProviderHealth>> GetHealthyProvidersAsync()
        {
            var healthChecks = providers.Select(async p => new CDNProviderHealth
            {
                Provider = p,
                IsHealthy = await healthChecker.CheckHealthAsync(p),
                Latency = await MeasureLatencyAsync(p)
            });

            var results = await Task.WhenAll(healthChecks);

            return results
                .Where(r => r.IsHealthy)
                .OrderBy(r => r.Latency)
                .ToList();
        }
    }

    // Smart cache invalidation
    public class CacheInvalidationStrategy
    {
        private readonly ICDNProvider cdnProvider;
        private readonly IMessageQueue queue;

        public async Task InvalidateTilesAsync(BoundingBox area, int minZoom, int maxZoom)
        {
            var tilesToInvalidate = CalculateAffectedTiles(area, minZoom, maxZoom);

            // Batch invalidations to avoid API limits
            var batches = tilesToInvalidate
                .Select((tile, index) => new { tile, index })
                .GroupBy(x => x.index / 1000) // 1000 tiles per batch
                .Select(g => g.Select(x => x.tile).ToList());

            foreach (var batch in batches)
            {
                await queue.PublishAsync(new InvalidationRequest
                {
                    Tiles = batch,
                    Timestamp = DateTime.UtcNow,
                    Priority = InvalidationPriority.Normal
                });
            }
        }

        private List<TileCoordinate> CalculateAffectedTiles(
            BoundingBox area,
            int minZoom,
            int maxZoom)
        {
            var tiles = new List<TileCoordinate>();

            for (int z = minZoom; z <= maxZoom; z++)
            {
                var (minTileX, minTileY) = LatLonToTile(area.MinLat, area.MinLon, z);
                var (maxTileX, maxTileY) = LatLonToTile(area.MaxLat, area.MaxLon, z);

                for (int x = minTileX; x <= maxTileX; x++)
                {
                    for (int y = minTileY; y <= maxTileY; y++)
                    {
                        tiles.Add(new TileCoordinate { X = x, Y = y, Z = z });
                    }
                }
            }

            return tiles;
        }
    }
}
```

### Predictive prefetching algorithms

Intelligent prefetching dramatically improves perceived performance by loading tiles before users request them:

```csharp
public class PredictiveTilePrefetcher
{
    private readonly ITileCache cache;
    private readonly ITileLoader loader;
    private readonly PredictionEngine predictionEngine;

    public class PredictionEngine
    {
        private readonly MarkovChainPredictor markovPredictor;
        private readonly VelocityPredictor velocityPredictor;
        private readonly MLPredictor mlPredictor;
        private readonly UserBehaviorAnalyzer behaviorAnalyzer;

        public async Task<List<TileCoordinate>> PredictNextTilesAsync(
            ViewportState currentState,
            UserInteractionHistory history)
        {
            // Combine multiple prediction strategies
            var predictions = await Task.WhenAll(
                markovPredictor.PredictAsync(history),
                velocityPredictor.PredictAsync(currentState),
                mlPredictor.PredictAsync(currentState, history),
                behaviorAnalyzer.PredictAsync(history)
            );

            // Weight and merge predictions
            return MergePredictions(predictions, new PredictionWeights
            {
                Markov = 0.25f,
                Velocity = 0.35f,
                ML = 0.30f,
                Behavior = 0.10f
            });
        }

        // Markov chain based on tile transitions
        public class MarkovChainPredictor
        {
            private readonly Dictionary<TileTransition, TransitionProbability> transitionMatrix;

            public async Task<List<TileCoordinate>> PredictAsync(UserInteractionHistory history)
            {
                // Get recent tile transitions
                var recentTransitions = history.GetRecentTransitions(lookback: 5);

                if (recentTransitions.Count < 2)
                {
                    return new List<TileCoordinate>();
                }

                // Find matching patterns in transition matrix
                var currentPattern = new TileTransition(
                    recentTransitions[^2],
                    recentTransitions[^1]
                );

                if (!transitionMatrix.TryGetValue(currentPattern, out var probabilities))
                {
                    return new List<TileCoordinate>();
                }

                // Return tiles ordered by probability
                return probabilities.NextTiles
                    .OrderByDescending(t => t.Probability)
                    .Take(10)
                    .Select(t => t.Tile)
                    .ToList();
            }

            public void UpdateTransitionMatrix(TileTransition transition)
            {
                if (!transitionMatrix.ContainsKey(transition))
                {
                    transitionMatrix[transition] = new TransitionProbability();
                }

                transitionMatrix[transition].RecordTransition(transition.To);

                // Prune low-probability transitions periodically
                if (transitionMatrix.Count > 10000)
                {
                    PruneTransitionMatrix();
                }
            }
        }

        // Physics-based velocity prediction
        public class VelocityPredictor
        {
            private readonly KalmanFilter kalmanFilter;

            public async Task<List<TileCoordinate>> PredictAsync(ViewportState state)
            {
                // Update Kalman filter with current position
                kalmanFilter.Update(state.Center, state.Timestamp);

                // Predict future positions
                var predictions = new List<TileCoordinate>();
                var timeSteps = new[] { 0.5f, 1.0f, 2.0f }; // seconds

                foreach (var dt in timeSteps)
                {
                    var predictedPosition = kalmanFilter.Predict(dt);
                    var predictedViewport = state.Viewport.MoveTo(predictedPosition);

                    var tiles = GetTilesInViewport(predictedViewport, state.ZoomLevel);
                    predictions.AddRange(tiles);
                }

                return predictions.Distinct().ToList();
            }

            private class KalmanFilter
            {
                private Vector2 position;
                private Vector2 velocity;
                private Matrix2x2 stateCovariance;

                public void Update(Vector2 measurement, DateTime timestamp)
                {
                    // Standard Kalman filter update
                    var dt = (float)(timestamp - lastUpdate).TotalSeconds;

                    // Predict
                    position += velocity * dt;
                    velocity *= 0.95f; // Friction

                    // Update
                    var innovation = measurement - position;
                    var kalmanGain = stateCovariance / (stateCovariance + measurementNoise);

                    position += kalmanGain * innovation;
                    velocity += kalmanGain * (innovation / dt);

                    stateCovariance = (Matrix2x2.Identity - kalmanGain) * stateCovariance;
                    lastUpdate = timestamp;
                }

                public Vector2 Predict(float deltaTime)
                {
                    return position + velocity * deltaTime;
                }
            }
        }

        // Machine learning predictor
        public class MLPredictor
        {
            private readonly ITensorFlowModel model;
            private readonly FeatureExtractor featureExtractor;

            public async Task<List<TileCoordinate>> PredictAsync(
                ViewportState state,
                UserInteractionHistory history)
            {
                // Extract features
                var features = featureExtractor.Extract(state, history);

                // Run inference
                using var session = model.CreateSession();
                var input = CreateTensor(features);
                var output = await session.RunAsync(input);

                // Convert predictions to tile coordinates
                return ConvertPredictionsToTiles(output, state.ZoomLevel);
            }

            private Tensor CreateTensor(Features features)
            {
                // Feature vector includes:
                // - Current viewport center (normalized)
                // - Velocity vector
                // - Acceleration vector
                // - Time of day (cyclical encoding)
                // - Day of week (one-hot)
                // - Historical tile access patterns
                // - Zoom level
                // - Device type

                var tensor = new float[1, FeatureDimension];

                tensor[0, 0] = features.NormalizedX;
                tensor[0, 1] = features.NormalizedY;
                tensor[0, 2] = features.VelocityX;
                tensor[0, 3] = features.VelocityY;
                // ... additional features

                return new Tensor(tensor);
            }
        }
    }

    // Adaptive prefetching based on network conditions
    public class AdaptivePrefetcher
    {
        private readonly NetworkMonitor networkMonitor;
        private readonly PrefetchStrategy strategy;

        public async Task PrefetchTilesAsync(
            List<TileCoordinate> predictedTiles,
            CancellationToken cancellationToken)
        {
            var networkInfo = await networkMonitor.GetNetworkInfoAsync();
            var budget = CalculatePrefetchBudget(networkInfo);

            // Prioritize tiles by prediction confidence
            var prioritizedTiles = predictedTiles
                .OrderByDescending(t => t.PredictionConfidence)
                .Take(budget.MaxTiles)
                .ToList();

            // Use appropriate strategy based on network
            if (networkInfo.Type == NetworkType.WiFi && networkInfo.SignalStrength > 0.8)
            {
                // Aggressive prefetching on good WiFi
                await ParallelPrefetchAsync(prioritizedTiles, maxConcurrency: 8);
            }
            else if (networkInfo.Type == NetworkType.Cellular)
            {
                // Conservative prefetching on cellular
                await SequentialPrefetchAsync(
                    prioritizedTiles.Take(budget.MaxTiles / 2),
                    delayBetweenTiles: TimeSpan.FromMilliseconds(100));
            }

            // Monitor performance and adjust
            await MonitorAndAdjustStrategy(networkInfo);
        }

        private PrefetchBudget CalculatePrefetchBudget(NetworkInfo network)
        {
            return new PrefetchBudget
            {
                MaxTiles = network.Type switch
                {
                    NetworkType.WiFi => 50,
                    NetworkType.Cellular5G => 30,
                    NetworkType.Cellular4G => 20,
                    NetworkType.Cellular3G => 10,
                    _ => 5
                },
                MaxBandwidth = network.EffectiveBandwidth * 0.2f, // Use 20% for prefetch
                TimeWindow = TimeSpan.FromSeconds(5)
            };
        }
    }

    // Cache warming strategies
    public class CacheWarmingService
    {
        private readonly ITileService tileService;
        private readonly IUsageAnalytics analytics;

        public async Task WarmCacheAsync(Region region, DateTimeOffset scheduledTime)
        {
            // Analyze historical usage patterns
            var usagePattern = await analytics.GetUsagePatternAsync(region);

            // Identify hot tiles
            var hotTiles = usagePattern.GetMostAccessedTiles(percentile: 95);

            // Schedule warming during low-traffic periods
            await ScheduleWarmingJobAsync(new WarmingJob
            {
                Tiles = hotTiles,
                ScheduledTime = scheduledTime,
                Priority = WarmingPriority.Low,
                Strategy = WarmingStrategy.Progressive
            });
        }

        public async Task ProactiveWarmingAsync()
        {
            // Warm cache based on predicted user patterns
            var predictions = await analytics.PredictTomorrowsHotspots();

            foreach (var hotspot in predictions)
            {
                // Generate tiles for predicted zoom levels
                var tiles = GenerateTilesForHotspot(hotspot);

                // Warm with exponential backoff to avoid overload
                await WarmWithBackoffAsync(tiles);
            }
        }

        private async Task WarmWithBackoffAsync(List<TileCoordinate> tiles)
        {
            var delay = TimeSpan.FromMilliseconds(10);
            var maxDelay = TimeSpan.FromSeconds(1);

            foreach (var batch in tiles.Batch(10))
            {
                await Task.WhenAll(batch.Select(t => tileService.PreloadTileAsync(t)));

                await Task.Delay(delay);

                // Exponential backoff
                delay = TimeSpan.FromMilliseconds(
                    Math.Min(delay.TotalMilliseconds * 1.5, maxDelay.TotalMilliseconds));
            }
        }
    }
}
```

### WebSocket and Server-Sent Events integration

Real-time tile updates enable collaborative features and live data visualization:

```csharp
public class RealtimeTileService
{
    private readonly IHubContext<TileHub> hubContext;
    private readonly ITileUpdateQueue updateQueue;

    // SignalR Hub for WebSocket communication
    public class TileHub : Hub
    {
        private readonly ITileSubscriptionManager subscriptionManager;

        public async Task Subscribe(TileSubscription subscription)
        {
            // Add client to subscription groups
            foreach (var tile in subscription.Tiles)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetTileGroup(tile));
            }

            // Store subscription for intelligent updates
            await subscriptionManager.AddSubscriptionAsync(Context.ConnectionId, subscription);

            // Send current tile versions
            await SendCurrentTileVersionsAsync(subscription.Tiles);
        }

        public async Task Unsubscribe(List<TileCoordinate> tiles)
        {
            foreach (var tile in tiles)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetTileGroup(tile));
            }

            await subscriptionManager.RemoveSubscriptionAsync(Context.ConnectionId, tiles);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await subscriptionManager.RemoveAllSubscriptionsAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        private string GetTileGroup(TileCoordinate tile) => $"tile:{tile.Z}/{tile.X}/{tile.Y}";
    }

    // Efficient delta updates
    public class TileDeltaCompressor
    {
        public async Task<TileDelta> ComputeDeltaAsync(
            byte[] oldTile,
            byte[] newTile,
            CompressionStrategy strategy)
        {
            return strategy switch
            {
                CompressionStrategy.PixelDiff => await ComputePixelDiffAsync(oldTile, newTile),
                CompressionStrategy.BinaryDelta => await ComputeBinaryDeltaAsync(oldTile, newTile),
                CompressionStrategy.StructuralDiff => await ComputeStructuralDiffAsync(oldTile, newTile),
                _ => throw new NotSupportedException()
            };
        }

        private async Task<TileDelta> ComputePixelDiffAsync(byte[] oldTile, byte[] newTile)
        {
            using var oldImage = Image.Load<Rgba32>(oldTile);
            using var newImage = Image.Load<Rgba32>(newTile);

            var changedPixels = new List<PixelChange>();

            oldImage.ProcessPixelRows(newImage, (oldAccessor, newAccessor) =>
            {
                for (int y = 0; y < oldAccessor.Height; y++)
                {
                    var oldRow = oldAccessor.GetRowSpan(y);
                    var newRow = newAccessor.GetRowSpan(y);

                    for (int x = 0; x < oldRow.Length; x++)
                    {
                        if (oldRow[x] != newRow[x])
                        {
                            changedPixels.Add(new PixelChange
                            {
                                X = x,
                                Y = y,
                                NewValue = newRow[x]
                            });
                        }
                    }
                }
            });

            // Compress changed pixels
            var compressed = await CompressPixelChangesAsync(changedPixels);

            return new TileDelta
            {
                Type = DeltaType.PixelDiff,
                Data = compressed,
                OriginalSize = newTile.Length,
                DeltaSize = compressed.Length
            };
        }
    }

    // Server-Sent Events for one-way updates
    public class TileSSEService
    {
        public async Task StreamUpdatesAsync(
            HttpContext context,
            TileSubscription subscription)
        {
            context.Response.Headers.Add("Content-Type", "text/event-stream");
            context.Response.Headers.Add("Cache-Control", "no-cache");
            context.Response.Headers.Add("X-Accel-Buffering", "no");

            await using var writer = new StreamWriter(context.Response.Body);

            // Send initial tiles
            foreach (var tile in subscription.Tiles)
            {
                var tileData = await GetTileDataAsync(tile);
                await writer.WriteLineAsync($"event: tile");
                await writer.WriteLineAsync($"data: {JsonSerializer.Serialize(tileData)}");
                await writer.WriteLineAsync();
                await writer.FlushAsync();
            }

            // Subscribe to updates
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                context.RequestAborted);

            await foreach (var update in GetTileUpdatesAsync(subscription, cts.Token))
            {
                if (cts.Token.IsCancellationRequested)
                    break;

                // Send update event
                await writer.WriteLineAsync($"event: update");
                await writer.WriteLineAsync($"id: {update.UpdateId}");
                await writer.WriteLineAsync($"data: {JsonSerializer.Serialize(update)}");
                await writer.WriteLineAsync();
                await writer.FlushAsync();

                // Send heartbeat every 30 seconds
                if (DateTime.UtcNow - lastHeartbeat > TimeSpan.FromSeconds(30))
                {
                    await writer.WriteLineAsync(": heartbeat");
                    await writer.FlushAsync();
                    lastHeartbeat = DateTime.UtcNow;
                }
            }
        }

        private async IAsyncEnumerable<TileUpdate> GetTileUpdatesAsync(
            TileSubscription subscription,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var updateChannel = Channel.CreateUnbounded<TileUpdate>();

            // Subscribe to tile changes
            foreach (var tile in subscription.Tiles)
            {
                updateQueue.Subscribe(tile, update =>
                {
                    updateChannel.Writer.TryWrite(update);
                });
            }

            // Read updates from channel
            await foreach (var update in updateChannel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return update;
            }
        }
    }

    // Hybrid approach for maximum compatibility
    public class HybridRealtimeService
    {
        private readonly TileHub tileHub;
        private readonly TileSSEService sseService;
        private readonly IFeatureDetector featureDetector;

        public async Task ConnectClientAsync(HttpContext context)
        {
            var capabilities = await featureDetector.DetectCapabilitiesAsync(context);

            if (capabilities.SupportsWebSockets)
            {
                // Preferred: Use SignalR/WebSockets
                await context.Response.WriteAsync(
                    $"<script>startWebSocketConnection('{GetWebSocketUrl()}');</script>");
            }
            else if (capabilities.SupportsSSE)
            {
                // Fallback: Use Server-Sent Events
                await sseService.StreamUpdatesAsync(context,
                    ExtractSubscription(context.Request));
            }
            else
            {
                // Last resort: Long polling
                await StartLongPollingAsync(context);
            }
        }

        // Optimized update broadcasting
        public async Task BroadcastTileUpdateAsync(TileCoordinate tile, TileUpdate update)
        {
            // Optimize for different update types
            if (update.Type == UpdateType.FullReplace)
            {
                // Send complete tile
                await hubContext.Clients
                    .Group(GetTileGroup(tile))
                    .SendAsync("TileReplaced", tile, update.Data);
            }
            else if (update.Type == UpdateType.Delta)
            {
                // Send only changes
                var delta = await ComputeDeltaAsync(tile, update);
                await hubContext.Clients
                    .Group(GetTileGroup(tile))
                    .SendAsync("TileDelta", tile, delta);
            }
            else if (update.Type == UpdateType.Invalidate)
            {
                // Just notify to refetch
                await hubContext.Clients
                    .Group(GetTileGroup(tile))
                    .SendAsync("TileInvalidated", tile, update.Version);
            }

            // Update metrics
            await RecordBroadcastMetricsAsync(tile, update);
        }
    }
}
```

### Performance monitoring and optimization

Continuous monitoring and optimization ensure streaming architectures maintain peak performance:

```csharp
public class StreamingPerformanceMonitor
{
    private readonly ITelemetryService telemetry;
    private readonly IMetricsCollector metrics;
    private readonly PerformanceThresholds thresholds;

    public class TileLoadMetrics
    {
        public TimeSpan LoadTime { get; set; }
        public long BytesTransferred { get; set; }
        public int HttpRequests { get; set; }
        public float CacheHitRate { get; set; }
        public NetworkType NetworkType { get; set; }
        public string CDNNode { get; set; }
        public Dictionary<string, object> CustomMetrics { get; set; }
    }

    // Real-time performance tracking
    public async Task<PerformanceReport> AnalyzePerformanceAsync(TimeSpan window)
    {
        var endTime = DateTime.UtcNow;
        var startTime = endTime - window;

        // Collect metrics from various sources
        var loadMetrics = await metrics.GetTileLoadMetricsAsync(startTime, endTime);
        var networkMetrics = await metrics.GetNetworkMetricsAsync(startTime, endTime);
        var cacheMetrics = await metrics.GetCacheMetricsAsync(startTime, endTime);

        var report = new PerformanceReport
        {
            Period = new DateRange(startTime, endTime),
            TileMetrics = AnalyzeTileMetrics(loadMetrics),
            NetworkMetrics = AnalyzeNetworkMetrics(networkMetrics),
            CacheMetrics = AnalyzeCacheMetrics(cacheMetrics),
            Bottlenecks = IdentifyBottlenecks(loadMetrics, networkMetrics, cacheMetrics),
            Recommendations = GenerateRecommendations(loadMetrics, networkMetrics, cacheMetrics)
        };

        // Alert on threshold violations
        await CheckThresholdsAsync(report);

        return report;
    }

    private TilePerformanceAnalysis AnalyzeTileMetrics(List<TileLoadMetrics> metrics)
    {
        return new TilePerformanceAnalysis
        {
            AverageLoadTime = TimeSpan.FromMilliseconds(
                metrics.Average(m => m.LoadTime.TotalMilliseconds)),

            P95LoadTime = TimeSpan.FromMilliseconds(
                metrics.OrderBy(m => m.LoadTime)
                      .Skip((int)(metrics.Count * 0.95))
                      .First().LoadTime.TotalMilliseconds),

            P99LoadTime = TimeSpan.FromMilliseconds(
                metrics.OrderBy(m => m.LoadTime)
                      .Skip((int)(metrics.Count * 0.99))
                      .First().LoadTime.TotalMilliseconds),

            TotalBytesTransferred = metrics.Sum(m => m.BytesTransferred),

            AverageBytesPerTile = metrics.Average(m => m.BytesTransferred),

            LoadTimeByNetworkType = metrics
                .GroupBy(m => m.NetworkType)
                .ToDictionary(
                    g => g.Key,
                    g => TimeSpan.FromMilliseconds(g.Average(m => m.LoadTime.TotalMilliseconds))
                ),

            CDNPerformance = metrics
                .GroupBy(m => m.CDNNode)
                .Select(g => new CDNNodeMetrics
                {
                    Node = g.Key,
                    AverageLoadTime = TimeSpan.FromMilliseconds(
                        g.Average(m => m.LoadTime.TotalMilliseconds)),
                    RequestCount = g.Count(),
                    ErrorRate = g.Count(m => m.LoadTime > thresholds.MaxAcceptableLoadTime) / (float)g.Count()
                })
                .ToList()
        };
    }

    // A/B testing for optimization strategies
    public class OptimizationExperiment
    {
        private readonly IExperimentService experimentService;
        private readonly Random random = new();

        public async Task<ExperimentResult> RunTileSizeExperimentAsync()
        {
            var variants = new[]
            {
                new Variant { Name = "256px", TileSize = 256 },
                new Variant { Name = "512px", TileSize = 512 },
                new Variant { Name = "Adaptive", TileSize = -1 } // Dynamic sizing
            };

            var results = new Dictionary<string, VariantMetrics>();

            // Run experiment for specified duration
            await experimentService.RunAsync("tile-size-optimization", async (userId) =>
            {
                // Assign variant
                var variant = variants[random.Next(variants.Length)];

                // Configure tile service for user
                await ConfigureTileServiceAsync(userId, variant);

                // Track metrics
                await TrackUserMetricsAsync(userId, variant.Name);
            });

            // Analyze results
            foreach (var variant in variants)
            {
                var metrics = await experimentService.GetMetricsAsync(variant.Name);
                results[variant.Name] = new VariantMetrics
                {
                    AverageLoadTime = metrics.GetAverage("load_time"),
                    UserSatisfaction = metrics.GetAverage("satisfaction_score"),
                    BandwidthUsage = metrics.GetSum("bytes_transferred"),
                    ConversionRate = metrics.GetConversionRate()
                };
            }

            // Statistical significance testing
            var winner = DetermineWinner(results);

            return new ExperimentResult
            {
                Winner = winner,
                Confidence = CalculateConfidence(results),
                Results = results
            };
        }
    }

    // Continuous optimization
    public class AutoOptimizer
    {
        private readonly MachineLearningService mlService;
        private readonly ConfigurationService configService;

        public async Task OptimizeStreamingParametersAsync()
        {
            // Collect recent performance data
            var trainingData = await CollectTrainingDataAsync(days: 7);

            // Train optimization model
            var model = await mlService.TrainOptimizationModelAsync(trainingData);

            // Generate optimal configuration
            var currentConditions = await GetCurrentConditionsAsync();
            var optimalConfig = await model.PredictOptimalConfigurationAsync(currentConditions);

            // Apply with gradual rollout
            await ApplyConfigurationGraduallyAsync(optimalConfig);
        }

        private async Task ApplyConfigurationGraduallyAsync(StreamingConfiguration config)
        {
            // Start with 1% of traffic
            var rolloutPercentage = 0.01f;

            while (rolloutPercentage < 1.0f)
            {
                await configService.ApplyToPercentageAsync(config, rolloutPercentage);

                // Monitor for regressions
                await Task.Delay(TimeSpan.FromMinutes(30));

                var metrics = await GetRolloutMetricsAsync();
                if (metrics.ShowsRegression)
                {
                    // Rollback
                    await configService.RollbackAsync();
                    await AlertOpsTeamAsync("Configuration rollback triggered", metrics);
                    break;
                }

                // Increase rollout
                rolloutPercentage = Math.Min(rolloutPercentage * 2, 1.0f);
            }
        }
    }

    // Performance debugging tools
    public class TileLoadTracer
    {
        public async Task<TraceResult> TraceRequestAsync(TileRequest request)
        {
            var trace = new TraceResult
            {
                RequestId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Request = request
            };

            // DNS resolution
            using (trace.BeginSegment("DNS"))
            {
                trace.DnsResolutionTime = await MeasureDnsAsync(request.Uri.Host);
            }

            // TCP connection
            using (trace.BeginSegment("TCP"))
            {
                trace.TcpConnectionTime = await MeasureTcpConnectionAsync(request.Uri);
            }

            // TLS handshake
            using (trace.BeginSegment("TLS"))
            {
                trace.TlsHandshakeTime = await MeasureTlsHandshakeAsync(request.Uri);
            }

            // HTTP request
            using (trace.BeginSegment("HTTP"))
            {
                var httpTrace = await TraceHttpRequestAsync(request);
                trace.HttpSegments = httpTrace;
            }

            // Rendering
            using (trace.BeginSegment("Render"))
            {
                trace.RenderTime = await MeasureRenderTimeAsync(request);
            }

            // Generate waterfall diagram
            trace.WaterfallDiagram = GenerateWaterfallDiagram(trace);

            return trace;
        }

        private WaterfallDiagram GenerateWaterfallDiagram(TraceResult trace)
        {
            var diagram = new WaterfallDiagram();

            foreach (var segment in trace.GetAllSegments())
            {
                diagram.AddBar(new WaterfallBar
                {
                    Name = segment.Name,
                    StartTime = segment.StartTime - trace.Timestamp,
                    Duration = segment.Duration,
                    Color = GetSegmentColor(segment.Type)
                });
            }

            return diagram;
        }
    }
}
```

## Conclusion

The evolution from monolithic image loading to sophisticated streaming and tiling architectures represents one of the
most significant advances in modern graphics processing. Through this chapter, we've explored how tile-based rendering
systems leverage hardware capabilities to achieve 10x memory bandwidth reductions, how progressive loading patterns
transform user perception of performance, how pyramidal structures enable seamless multi-scale access, and how HTTP
optimization strategies maximize network efficiency.

The key insights that emerge from our exploration center on the fundamental shift from pushing pixels to orchestrating
systems. Modern graphics applications no longer simply load and display images; they predict user behavior, adapt to
network conditions, leverage global CDN infrastructure, and stream precisely what users need exactly when they need it.
The streaming architectures we've examined demonstrate that **performance is no longer about raw speed but about
perceived responsiveness**.

.NET 9.0 provides a remarkable platform for implementing these sophisticated patterns. The combination of improved
HttpClient performance, native HTTP/3 support, enhanced SIMD capabilities, and powerful async primitives like Channel<T>
and IAsyncEnumerable enables developers to build streaming systems that rival those of tech giants. The 20% improvement
in HTTP request handling, 25% reduction in latency, and support for memory-mapped files with 4x performance gains
transform theoretical architectures into practical realities.

Real-world implementations validate these approaches. Google Maps serves billions of tile requests daily using 256×256
pixel tiles with sophisticated caching strategies. Cesium demonstrates 10x performance improvements through intelligent
tile prioritization. Cloud-Optimized GeoTIFF enables partial access to terabyte-scale imagery with 50-90% bandwidth
savings. These success stories prove that the patterns and techniques presented in this chapter scale from mobile
applications to planetary-scale systems.

The architectural principles that emerge from our analysis provide clear guidance for implementation:

1. **Embrace lazy evaluation**: Load only what's visible, predict what's next, and discard what's no longer needed.

2. **Layer your caching**: Combine memory, disk, and CDN caches with intelligent invalidation strategies.

3. **Adapt to conditions**: Monitor network quality, device capabilities, and user behavior to optimize delivery.

4. **Leverage parallelism**: Use HTTP/2 multiplexing, parallel tile loading, and GPU acceleration wherever possible.

5. **Design for failure**: Implement fallback strategies, progressive enhancement, and graceful degradation.

Looking forward, the convergence of 5G networks, edge computing, and WebGPU promises even more dramatic improvements.
Streaming architectures will evolve to leverage distributed compute at the edge, enabling real-time image processing
without centralized servers. Machine learning models will predict not just which tiles users need next, but generate
them on demand using neural synthesis.

The journey from static images to dynamic streaming represents a fundamental reimagining of how we interact with visual
data. As datasets grow from gigabytes to petabytes and user expectations shift from patient waiting to instant
gratification, the streaming and tiling architectures explored in this chapter provide the foundation for meeting these
challenges. The combination of mathematical elegance, engineering pragmatism, and relentless optimization creates
systems that feel magical to users while remaining maintainable for developers.

In the end, the best streaming architecture is invisible—users simply see smooth, responsive imagery without awareness
of the complex orchestration making it possible. By mastering the patterns and techniques presented in this chapter,
developers can create these invisible marvels, transforming massive datasets into fluid, interactive experiences that
delight users and push the boundaries of what's possible in modern graphics processing.
