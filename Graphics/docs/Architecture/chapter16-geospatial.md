# Chapter 16: Geospatial Image Processing

The intersection of geographic information systems (GIS) and high-performance image processing presents unique
challenges that push the boundaries of modern computing. Geospatial imagery routinely involves datasets measured in
gigabytes or terabytes, coordinate systems requiring millimeter precision across continental scales, and real-time
serving requirements for millions of concurrent users. This chapter explores the architectural patterns, performance
optimizations, and practical implementations necessary for handling geospatial imagery in .NET 9.0.

Modern geospatial applications demand processing capabilities that traditional image handling libraries cannot provide.
Satellite imagery from platforms like Sentinel-2 produces individual scenes exceeding 1GB, while aerial photography
campaigns generate datasets requiring distributed storage and processing. The emergence of cloud-native geospatial
formats, streaming protocols, and web mapping standards has fundamentally transformed how we approach these challenges,
moving from desktop-centric workflows to distributed, scalable architectures.

The .NET ecosystem offers powerful capabilities for geospatial image processing through careful integration of
specialized libraries, memory-mapped file operations, and SIMD-accelerated transformations. By leveraging BigTIFF
support for files exceeding traditional 4GB limits, implementing Cloud-Optimized GeoTIFF (COG) for efficient streaming,
and generating map tiles for web distribution, .NET applications can compete with specialized GIS software while
maintaining the development productivity and performance characteristics of managed code.

## 16.1 Large TIFF and BigTIFF Handling

### Understanding TIFF limitations and BigTIFF evolution

The Tagged Image File Format (TIFF) has served as the foundation of geospatial imagery for decades, but its original
32-bit offset design imposes a hard 4GB file size limit. This limitation becomes critical when handling modern
geospatial datasets: a single orthorectified aerial photograph at 5cm resolution covering a metropolitan area easily
exceeds 10GB, while hyperspectral satellite imagery with hundreds of bands requires even larger storage.

BigTIFF extends the TIFF specification by using 64-bit offsets throughout the file structure, enabling files up to
18,000 petabytes. The format maintains backward compatibility through a different version number (43 instead of 42) in
the file header, allowing applications to gracefully detect and handle both formats. Understanding the structural
differences between TIFF and BigTIFF is essential for implementing efficient readers and writers.

```csharp
public class BigTIFFReader : IDisposable
{
    private readonly Stream _stream;
    private readonly bool _isLittleEndian;
    private readonly bool _isBigTIFF;
    private readonly long _firstIFDOffset;
    private readonly object _lock = new object();

    public BigTIFFReader(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));

        // Read and validate header
        var header = new byte[16];
        _stream.Read(header, 0, 16);

        // Check byte order
        if (header[0] == 0x49 && header[1] == 0x49)
        {
            _isLittleEndian = true;
        }
        else if (header[0] == 0x4D && header[1] == 0x4D)
        {
            _isLittleEndian = false;
        }
        else
        {
            throw new InvalidDataException("Invalid TIFF byte order marker");
        }

        // Check version (42 for TIFF, 43 for BigTIFF)
        ushort version = ReadUInt16(header, 2);
        _isBigTIFF = version == 43;

        if (_isBigTIFF)
        {
            // BigTIFF: next 2 bytes should be 8 (offset byte size)
            ushort offsetSize = ReadUInt16(header, 4);
            if (offsetSize != 8)
            {
                throw new InvalidDataException($"Invalid BigTIFF offset size: {offsetSize}");
            }

            // Skip 2 reserved bytes
            _firstIFDOffset = ReadInt64(header, 8);
        }
        else if (version == 42)
        {
            // Classic TIFF
            _firstIFDOffset = ReadUInt32(header, 4);
        }
        else
        {
            throw new InvalidDataException($"Unknown TIFF version: {version}");
        }
    }

    public async Task<GeoTIFFImage> ReadImageAsync(int ifdIndex = 0)
    {
        var ifd = await ReadIFDAsync(ifdIndex);
        var tags = ParseTags(ifd);

        // Extract image dimensions
        var width = GetRequiredTag<uint>(tags, TIFFTag.ImageWidth);
        var height = GetRequiredTag<uint>(tags, TIFFTag.ImageLength);
        var samplesPerPixel = GetTag(tags, TIFFTag.SamplesPerPixel, 1);
        var bitsPerSample = GetTagArray(tags, TIFFTag.BitsPerSample, samplesPerPixel);

        // Determine data layout
        var compression = GetTag(tags, TIFFTag.Compression, CompressionType.None);
        var planarConfig = GetTag(tags, TIFFTag.PlanarConfiguration, PlanarConfiguration.Chunky);
        var photometric = GetTag(tags, TIFFTag.PhotometricInterpretation, PhotometricInterpretation.MinIsBlack);

        // Handle tiled vs stripped storage
        bool isTiled = tags.ContainsKey(TIFFTag.TileWidth);

        if (isTiled)
        {
            return await ReadTiledImageAsync(tags, width, height);
        }
        else
        {
            return await ReadStrippedImageAsync(tags, width, height);
        }
    }

    private async Task<GeoTIFFImage> ReadTiledImageAsync(
        Dictionary<TIFFTag, TIFFFieldValue> tags,
        uint width,
        uint height)
    {
        var tileWidth = GetRequiredTag<uint>(tags, TIFFTag.TileWidth);
        var tileHeight = GetRequiredTag<uint>(tags, TIFFTag.TileLength);
        var tileOffsets = GetTagArray<long>(tags, TIFFTag.TileOffsets);
        var tileByteCounts = GetTagArray<long>(tags, TIFFTag.TileByteCounts);

        // Calculate tile grid dimensions
        var tilesAcross = (width + tileWidth - 1) / tileWidth;
        var tilesDown = (height + tileHeight - 1) / tileHeight;
        var totalTiles = tilesAcross * tilesDown;

        if (tileOffsets.Length != totalTiles)
        {
            throw new InvalidDataException(
                $"Tile count mismatch: expected {totalTiles}, found {tileOffsets.Length}");
        }

        // Create memory-mapped view for efficient tile access
        var image = new GeoTIFFImage(width, height);

        if (_stream is FileStream fileStream)
        {
            using var mmf = MemoryMappedFile.CreateFromFile(
                fileStream, null, 0,
                MemoryMappedFileAccess.Read,
                HandleInheritability.None,
                leaveOpen: true);

            // Process tiles in parallel for optimal performance
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            await Parallel.ForEachAsync(
                Enumerable.Range(0, (int)totalTiles),
                parallelOptions,
                async (tileIndex, ct) =>
                {
                    await ProcessTileAsync(mmf, image, tileIndex, tags,
                        tileWidth, tileHeight, tilesAcross,
                        tileOffsets[tileIndex], tileByteCounts[tileIndex]);
                });
        }
        else
        {
            // Fallback for non-file streams
            for (int i = 0; i < totalTiles; i++)
            {
                await ReadTileFromStreamAsync(image, i, tags,
                    tileWidth, tileHeight, tilesAcross,
                    tileOffsets[i], tileByteCounts[i]);
            }
        }

        return image;
    }
}
```

### Memory-mapped file strategies for gigapixel images

Processing gigapixel geospatial imagery requires sophisticated memory management strategies that go beyond traditional
file I/O. Memory-mapped files provide virtual memory backed by file storage, enabling applications to work with files
larger than available RAM while maintaining high performance through demand paging and OS-level caching.

The key to efficient memory-mapped processing lies in understanding access patterns and optimizing for spatial locality.
Geospatial operations typically exhibit strong spatial coherence—pixels near each other in image space are frequently
accessed together. By organizing data layout to match access patterns and implementing intelligent prefetching
strategies, we can minimize page faults and maximize cache efficiency.

```csharp
public class MemoryMappedGeoTIFF : IDisposable
{
    private readonly MemoryMappedFile _mmf;
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly TIFFMetadata _metadata;
    private readonly long _dataOffset;
    private readonly int _tileSize;
    private readonly LRUCache<TileKey, byte[]> _tileCache;

    public MemoryMappedGeoTIFF(string filePath, int maxCachedTiles = 256)
    {
        var fileInfo = new FileInfo(filePath);
        _mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null,
            fileInfo.Length, MemoryMappedFileAccess.Read);

        _accessor = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

        // Read metadata to understand layout
        _metadata = ReadMetadata();
        _dataOffset = _metadata.FirstTileOffset;
        _tileSize = _metadata.TileWidth * _metadata.TileHeight *
                    _metadata.BytesPerPixel;

        // Initialize LRU cache for decompressed tiles
        _tileCache = new LRUCache<TileKey, byte[]>(maxCachedTiles);
    }

    public async Task<Rectangle<float>> ReadRegionAsync(
        BoundingBox bounds,
        int targetWidth,
        int targetHeight)
    {
        // Convert geographic bounds to pixel coordinates
        var pixelBounds = _metadata.GeographicToPixel(bounds);

        // Calculate required tiles
        var startTileX = pixelBounds.Left / _metadata.TileWidth;
        var startTileY = pixelBounds.Top / _metadata.TileHeight;
        var endTileX = (pixelBounds.Right + _metadata.TileWidth - 1) /
                       _metadata.TileWidth;
        var endTileY = (pixelBounds.Bottom + _metadata.TileHeight - 1) /
                       _metadata.TileHeight;

        // Determine optimal processing strategy based on region size
        var tileCount = (endTileX - startTileX) * (endTileY - startTileY);

        if (tileCount > 100)
        {
            // Large region: use streaming approach
            return await StreamLargeRegionAsync(
                pixelBounds, targetWidth, targetHeight,
                startTileX, startTileY, endTileX, endTileY);
        }
        else
        {
            // Small region: load all tiles to memory
            return await LoadSmallRegionAsync(
                pixelBounds, targetWidth, targetHeight,
                startTileX, startTileY, endTileX, endTileY);
        }
    }

    private async Task<Rectangle<float>> StreamLargeRegionAsync(
        Rectangle pixelBounds, int targetWidth, int targetHeight,
        int startTileX, int startTileY, int endTileX, int endTileY)
    {
        var result = new Rectangle<float>(targetWidth, targetHeight);

        // Create resampling buffers
        var resampleBuffer = ArrayPool<float>.Shared.Rent(
            _metadata.TileWidth * _metadata.TileHeight * 4);

        try
        {
            // Process in horizontal strips to maximize cache efficiency
            var stripHeight = Math.Min(_metadata.TileHeight * 4, targetHeight);
            var strips = (targetHeight + stripHeight - 1) / stripHeight;

            for (int strip = 0; strip < strips; strip++)
            {
                var stripStartY = strip * stripHeight;
                var stripEndY = Math.Min(stripStartY + stripHeight, targetHeight);

                // Calculate source tile range for this strip
                var stripStartTileY = startTileY +
                    (strip * stripHeight * (endTileY - startTileY)) / targetHeight;
                var stripEndTileY = startTileY +
                    ((strip + 1) * stripHeight * (endTileY - startTileY)) / targetHeight;

                // Process strip
                await ProcessStripAsync(
                    result, pixelBounds, resampleBuffer,
                    startTileX, endTileX, stripStartTileY, stripEndTileY,
                    stripStartY, stripEndY, targetWidth);
            }
        }
        finally
        {
            ArrayPool<float>.Shared.Return(resampleBuffer);
        }

        return result;
    }

    private unsafe byte[] ReadTileOptimized(int tileX, int tileY)
    {
        var key = new TileKey(tileX, tileY);

        // Check cache first
        if (_tileCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        // Calculate tile offset
        var tileIndex = tileY * _metadata.TilesAcross + tileX;
        var offset = _metadata.TileOffsets[tileIndex];
        var size = _metadata.TileByteCounts[tileIndex];

        // Use unsafe code for maximum performance
        var tileData = new byte[size];

        fixed (byte* pDest = tileData)
        {
            var span = new Span<byte>(pDest, (int)size);
            _accessor.ReadArray(offset, span);
        }

        // Decompress if necessary
        if (_metadata.Compression != CompressionType.None)
        {
            tileData = DecompressTile(tileData, _metadata.Compression);
        }

        // Add to cache
        _tileCache.AddOrUpdate(key, tileData);

        return tileData;
    }
}
```

### Efficient pyramid generation for overview levels

Geospatial imagery requires multiple resolution levels (pyramids or overviews) to enable efficient visualization at
different scales. Generating these pyramids presents unique challenges: maintaining geometric accuracy during
downsampling, preserving data type precision for scientific analysis, and optimizing I/O patterns for datasets that
don't fit in memory.

The pyramid generation process must balance quality and performance. While simple box filtering provides fast
downsampling, geospatial applications often require more sophisticated approaches like Lanczos or Mitchell filtering to
preserve feature clarity. The implementation leverages SIMD instructions for filter kernel application and processes
data in tiles to maintain memory efficiency.

```csharp
public class GeospatialPyramidBuilder
{
    private readonly int _tileSize;
    private readonly ResamplingKernel _kernel;
    private readonly int _maxParallelism;

    public GeospatialPyramidBuilder(
        int tileSize = 512,
        ResamplingKernel kernel = null,
        int maxParallelism = -1)
    {
        _tileSize = tileSize;
        _kernel = kernel ?? ResamplingKernel.Lanczos3;
        _maxParallelism = maxParallelism > 0 ? maxParallelism :
                         Environment.ProcessorCount;
    }

    public async Task BuildPyramidsAsync(
        string inputPath,
        string outputPath,
        PyramidOptions options = null)
    {
        options ??= PyramidOptions.Default;

        using var reader = new BigTIFFReader(File.OpenRead(inputPath));
        var sourceMetadata = await reader.ReadMetadataAsync();

        // Calculate pyramid levels needed
        var levels = CalculatePyramidLevels(
            sourceMetadata.Width,
            sourceMetadata.Height,
            options.MinSize);

        // Create output BigTIFF with space for all levels
        using var writer = new BigTIFFWriter(File.Create(outputPath));

        // Copy base level with potential reformatting
        await CopyBaseLevelAsync(reader, writer, sourceMetadata, options);

        // Generate each pyramid level
        for (int level = 1; level < levels.Count; level++)
        {
            var levelInfo = levels[level];

            await GeneratePyramidLevelAsync(
                writer,
                level - 1,
                level,
                levelInfo,
                options);
        }

        // Update metadata with pyramid information
        await writer.WritePyramidMetadataAsync(levels);
    }

    private async Task GeneratePyramidLevelAsync(
        BigTIFFWriter writer,
        int sourceLevel,
        int targetLevel,
        PyramidLevel levelInfo,
        PyramidOptions options)
    {
        // Calculate processing parameters
        var sourceDims = writer.GetLevelDimensions(sourceLevel);
        var targetDims = levelInfo.Dimensions;
        var scaleFactor = sourceDims.Width / (double)targetDims.Width;

        // Determine tile grid for target level
        var tilesAcross = (targetDims.Width + _tileSize - 1) / _tileSize;
        var tilesDown = (targetDims.Height + _tileSize - 1) / _tileSize;

        // Process tiles in parallel with memory constraints
        using var semaphore = new SemaphoreSlim(options.MaxConcurrentTiles);
        var tasks = new List<Task>();

        for (int tileY = 0; tileY < tilesDown; tileY++)
        {
            for (int tileX = 0; tileX < tilesAcross; tileX++)
            {
                await semaphore.WaitAsync();

                var task = ProcessPyramidTileAsync(
                    writer, sourceLevel, targetLevel,
                    tileX, tileY, scaleFactor, options)
                    .ContinueWith(t => semaphore.Release());

                tasks.Add(task);
            }
        }

        await Task.WhenAll(tasks);
    }

    private async Task ProcessPyramidTileAsync(
        BigTIFFWriter writer,
        int sourceLevel,
        int targetLevel,
        int tileX,
        int tileY,
        double scaleFactor,
        PyramidOptions options)
    {
        // Calculate source region needed for this target tile
        var targetBounds = new Rectangle(
            tileX * _tileSize,
            tileY * _tileSize,
            Math.Min(_tileSize, writer.GetLevelDimensions(targetLevel).Width -
                     tileX * _tileSize),
            Math.Min(_tileSize, writer.GetLevelDimensions(targetLevel).Height -
                     tileY * _tileSize));

        // Expand bounds to account for filter kernel support
        var kernelSupport = _kernel.Support;
        var sourceBounds = new Rectangle(
            (int)Math.Floor(targetBounds.X * scaleFactor - kernelSupport),
            (int)Math.Floor(targetBounds.Y * scaleFactor - kernelSupport),
            (int)Math.Ceiling(targetBounds.Width * scaleFactor + 2 * kernelSupport),
            (int)Math.Ceiling(targetBounds.Height * scaleFactor + 2 * kernelSupport));

        // Clamp to source dimensions
        var sourceDims = writer.GetLevelDimensions(sourceLevel);
        sourceBounds = Rectangle.Intersect(sourceBounds,
            new Rectangle(0, 0, sourceDims.Width, sourceDims.Height));

        // Read source data
        var sourceData = await writer.ReadRegionAsync(sourceLevel, sourceBounds);

        // Apply resampling
        var targetData = ResampleWithSIMD(
            sourceData, sourceBounds, targetBounds, scaleFactor);

        // Write target tile
        await writer.WriteTileAsync(targetLevel, tileX, tileY, targetData);
    }

    private unsafe float[] ResampleWithSIMD(
        float[] source,
        Rectangle sourceBounds,
        Rectangle targetBounds,
        double scale)
    {
        var sourceWidth = sourceBounds.Width;
        var sourceHeight = sourceBounds.Height;
        var targetWidth = targetBounds.Width;
        var targetHeight = targetBounds.Height;
        var channels = source.Length / (sourceWidth * sourceHeight);

        var target = new float[targetWidth * targetHeight * channels];

        // Pre-calculate filter weights for each output pixel
        var weightsX = PrecomputeFilterWeights(targetWidth, sourceWidth, scale);
        var weightsY = PrecomputeFilterWeights(targetHeight, sourceHeight, scale);

        // Process using SIMD where possible
        fixed (float* pSource = source)
        fixed (float* pTarget = target)
        {
            Parallel.For(0, targetHeight, y =>
            {
                var yWeights = weightsY[y];
                var targetRowPtr = pTarget + y * targetWidth * channels;

                for (int x = 0; x < targetWidth; x++)
                {
                    var xWeights = weightsX[x];
                    var targetPtr = targetRowPtr + x * channels;

                    // Initialize accumulators
                    var accumulators = stackalloc Vector256<float>[channels];
                    for (int c = 0; c < channels; c++)
                    {
                        accumulators[c] = Vector256<float>.Zero;
                    }

                    // Apply 2D filter kernel
                    foreach (var (sy, wy) in yWeights)
                    {
                        var sourceRowPtr = pSource + sy * sourceWidth * channels;

                        foreach (var (sx, wx) in xWeights)
                        {
                            var weight = wx * wy;
                            var weightVec = Vector256.Create(weight);
                            var sourcePtr = sourceRowPtr + sx * channels;

                            // Vectorized multiply-accumulate
                            for (int c = 0; c < channels; c++)
                            {
                                var sourceVec = Vector256.Create(sourcePtr[c]);
                                accumulators[c] = Vector256.Add(
                                    accumulators[c],
                                    Vector256.Multiply(sourceVec, weightVec));
                            }
                        }
                    }

                    // Extract results
                    for (int c = 0; c < channels; c++)
                    {
                        targetPtr[c] = Vector256.Sum(accumulators[c]);
                    }
                }
            });
        }

        return target;
    }
}
```

## 16.2 Cloud-Optimized GeoTIFF (COG)

### COG structure and HTTP range request optimization

Cloud-Optimized GeoTIFF represents a fundamental shift in how geospatial imagery is stored and accessed in cloud
environments. Unlike traditional GeoTIFF files that require sequential reading, COG files are structured to enable
efficient partial reads through HTTP range requests, transforming multi-gigabyte images into streamable resources that
can be visualized without downloading entire files.

The key to COG's efficiency lies in its carefully designed internal structure: a specific ordering of image data and
metadata that aligns with cloud access patterns. Image tiles are arranged in a predictable order, metadata and image
file directories (IFDs) are consolidated at the file's beginning or end, and overview levels are stored in descending
resolution order. This organization enables clients to read just the file header to understand the entire image
structure, then request only the specific tiles needed for visualization.

```csharp
public class CloudOptimizedGeoTIFF
{
    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly COGMetadata _metadata;
    private readonly TileCache _cache;

    public CloudOptimizedGeoTIFF(string url, HttpClient httpClient = null)
    {
        _url = url;
        _httpClient = httpClient ?? new HttpClient();
        _cache = new TileCache(maxSizeBytes: 100 * 1024 * 1024); // 100MB cache
    }

    public async Task<COGMetadata> InitializeAsync()
    {
        // Read header with minimal data transfer
        const int headerSize = 16384; // 16KB usually sufficient

        var headerRequest = new HttpRequestMessage(HttpMethod.Get, _url);
        headerRequest.Headers.Range = new RangeHeaderValue(0, headerSize - 1);

        using var headerResponse = await _httpClient.SendAsync(headerRequest);
        headerResponse.EnsureSuccessStatusCode();

        var headerData = await headerResponse.Content.ReadAsByteArrayAsync();

        // Parse TIFF structure
        using var headerStream = new MemoryStream(headerData);
        var reader = new TIFFReader(headerStream);

        _metadata = new COGMetadata
        {
            ByteOrder = reader.ByteOrder,
            IsBigTIFF = reader.IsBigTIFF,
            FileSize = GetFileSize(headerResponse),
            IFDs = new List<IFDInfo>()
        };

        // Read IFD chain
        var ifdOffset = reader.FirstIFDOffset;
        var ifdIndex = 0;

        while (ifdOffset > 0 && ifdIndex < 20) // Limit to prevent infinite loops
        {
            IFDInfo ifd;

            if (ifdOffset < headerData.Length)
            {
                // IFD is in header - parse directly
                headerStream.Position = ifdOffset;
                ifd = ParseIFD(reader, ifdIndex);
            }
            else
            {
                // Need to fetch IFD
                ifd = await FetchAndParseIFDAsync(ifdOffset, ifdIndex);
            }

            _metadata.IFDs.Add(ifd);
            ifdOffset = ifd.NextIFDOffset;
            ifdIndex++;
        }

        // Validate COG compliance
        ValidateCOGStructure();

        return _metadata;
    }

    public async Task<Image<Rgba32>> ReadRegionAsync(
        BoundingBox geoBounds,
        int maxPixelWidth,
        int level = -1)
    {
        // Select appropriate resolution level
        if (level < 0)
        {
            level = SelectOptimalLevel(geoBounds, maxPixelWidth);
        }

        var ifd = _metadata.IFDs[level];

        // Convert geographic bounds to pixel coordinates
        var pixelBounds = ifd.GeoBoundsToPixel(geoBounds);

        // Calculate required tiles
        var tileRangeX = GetTileRange(pixelBounds.X, pixelBounds.Width,
                                     ifd.TileWidth, ifd.ImageWidth);
        var tileRangeY = GetTileRange(pixelBounds.Y, pixelBounds.Height,
                                     ifd.TileHeight, ifd.ImageHeight);

        // Fetch tiles with intelligent batching
        var tiles = await FetchTilesOptimizedAsync(
            level, tileRangeX, tileRangeY);

        // Assemble and crop result
        return AssembleTiles(tiles, pixelBounds, ifd);
    }

    private async Task<Dictionary<(int x, int y), byte[]>> FetchTilesOptimizedAsync(
        int level,
        (int start, int end) tileRangeX,
        (int start, int end) tileRangeY)
    {
        var ifd = _metadata.IFDs[level];
        var tiles = new Dictionary<(int x, int y), byte[]>();
        var tilesToFetch = new List<(int x, int y, long offset, long size)>();

        // Check cache and build fetch list
        for (int ty = tileRangeY.start; ty <= tileRangeY.end; ty++)
        {
            for (int tx = tileRangeX.start; tx <= tileRangeX.end; tx++)
            {
                var key = (tx, ty);
                var cacheKey = $"{level}_{tx}_{ty}";

                if (_cache.TryGet(cacheKey, out byte[] cachedTile))
                {
                    tiles[key] = cachedTile;
                }
                else
                {
                    var tileIndex = ty * ifd.TilesAcross + tx;
                    tilesToFetch.Add((tx, ty,
                        ifd.TileOffsets[tileIndex],
                        ifd.TileByteCounts[tileIndex]));
                }
            }
        }

        if (tilesToFetch.Count == 0)
            return tiles;

        // Sort by offset for sequential access
        tilesToFetch.Sort((a, b) => a.offset.CompareTo(b.offset));

        // Coalesce adjacent tiles into single requests
        var requests = CoalesceRangeRequests(tilesToFetch,
            maxGapSize: 8192); // 8KB gap threshold

        // Execute parallel requests with concurrency limit
        var semaphore = new SemaphoreSlim(4); // Max 4 concurrent requests
        var tasks = requests.Select(async request =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await FetchRangeAsync(request);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToArray();

        var results = await Task.WhenAll(tasks);

        // Extract individual tiles from coalesced responses
        foreach (var result in results)
        {
            ExtractTilesFromRange(result, tiles, ifd);
        }

        // Update cache
        foreach (var (key, data) in tiles)
        {
            _cache.Set($"{level}_{key.x}_{key.y}", data);
        }

        return tiles;
    }

    private List<RangeRequest> CoalesceRangeRequests(
        List<(int x, int y, long offset, long size)> tiles,
        long maxGapSize)
    {
        var requests = new List<RangeRequest>();

        if (tiles.Count == 0)
            return requests;

        var currentRequest = new RangeRequest
        {
            Start = tiles[0].offset,
            End = tiles[0].offset + tiles[0].size - 1,
            Tiles = new List<(int x, int y, long offset, long size)> { tiles[0] }
        };

        for (int i = 1; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            var gap = tile.offset - (currentRequest.End + 1);

            if (gap <= maxGapSize)
            {
                // Extend current request
                currentRequest.End = tile.offset + tile.size - 1;
                currentRequest.Tiles.Add(tile);
            }
            else
            {
                // Start new request
                requests.Add(currentRequest);
                currentRequest = new RangeRequest
                {
                    Start = tile.offset,
                    End = tile.offset + tile.size - 1,
                    Tiles = new List<(int x, int y, long offset, long size)> { tile }
                };
            }
        }

        requests.Add(currentRequest);
        return requests;
    }
}
```

### Implementing efficient tile caching strategies

Effective tile caching transforms COG performance from network-bound to CPU-bound, especially for interactive
applications like web maps. The caching strategy must balance memory usage, cache hit rates, and eviction overhead while
considering geospatial access patterns that exhibit strong spatial and temporal locality.

Modern caching implementations employ multi-level strategies combining in-memory caches for hot tiles, disk caches for
warm data, and intelligent prefetching based on user interaction patterns. The cache key design incorporates spatial
hierarchy, enabling efficient bulk operations like clearing all tiles for a specific zoom level or geographic region.

```csharp
public class HierarchicalTileCache
{
    private readonly IMemoryCache _l1Cache; // Hot tiles in memory
    private readonly IDiskCache _l2Cache;   // Warm tiles on disk
    private readonly IPredictiveModel _predictor;
    private readonly CacheMetrics _metrics;

    public HierarchicalTileCache(HierarchicalCacheOptions options)
    {
        _l1Cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = options.MaxMemoryBytes,
            CompactionPercentage = 0.25
        });

        _l2Cache = new DiskCache(options.CacheDirectory,
            options.MaxDiskBytes);

        _predictor = new SpatialAccessPredictor();
        _metrics = new CacheMetrics();
    }

    public async Task<TileData> GetAsync(TileKey key)
    {
        _metrics.RecordRequest(key);

        // L1 lookup
        if (_l1Cache.TryGetValue(key, out TileData cached))
        {
            _metrics.RecordHit(CacheLevel.L1);
            UpdateAccessPattern(key);
            return cached;
        }

        // L2 lookup
        var l2Data = await _l2Cache.GetAsync(key);
        if (l2Data != null)
        {
            _metrics.RecordHit(CacheLevel.L2);

            // Promote to L1 if access pattern suggests it
            if (ShouldPromoteToL1(key))
            {
                await PromoteToL1Async(key, l2Data);
            }

            UpdateAccessPattern(key);
            return l2Data;
        }

        _metrics.RecordMiss();
        return null;
    }

    public async Task SetAsync(TileKey key, TileData data)
    {
        // Determine cache level based on predicted access frequency
        var accessProbability = _predictor.PredictAccessProbability(key);

        if (accessProbability > 0.7)
        {
            // High probability - straight to L1
            SetL1WithEviction(key, data);
        }
        else if (accessProbability > 0.3)
        {
            // Medium probability - L2 only
            await _l2Cache.SetAsync(key, data);
        }
        // Low probability tiles are not cached

        // Trigger predictive prefetching
        await PrefetchRelatedTilesAsync(key);
    }

    private async Task PrefetchRelatedTilesAsync(TileKey key)
    {
        // Get tiles likely to be accessed next
        var predictions = _predictor.PredictNextTiles(key, maxPredictions: 8);

        // Filter already cached tiles
        var toPrefetch = predictions
            .Where(p => p.Probability > 0.5)
            .Where(p => !_l1Cache.TryGetValue(p.Key, out _))
            .Take(4)
            .ToList();

        if (toPrefetch.Any())
        {
            // Schedule background prefetch
            _ = Task.Run(async () =>
            {
                foreach (var prediction in toPrefetch)
                {
                    try
                    {
                        await PrefetchTileAsync(prediction.Key);
                    }
                    catch
                    {
                        // Don't let prefetch errors bubble up
                    }
                }
            });
        }
    }

    private void SetL1WithEviction(TileKey key, TileData data)
    {
        var entryOptions = new MemoryCacheEntryOptions()
            .SetSize(data.SizeInBytes)
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .RegisterPostEvictionCallback(OnL1Eviction);

        _l1Cache.Set(key, data, entryOptions);
    }

    private void OnL1Eviction(object key, object value,
        EvictionReason reason, object state)
    {
        if (reason == EvictionReason.Capacity ||
            reason == EvictionReason.Expired)
        {
            var tileKey = (TileKey)key;
            var tileData = (TileData)value;

            // Demote to L2 if still potentially useful
            if (_predictor.GetHistoricalAccessCount(tileKey) > 1)
            {
                _ = _l2Cache.SetAsync(tileKey, tileData);
            }
        }
    }
}

// Spatial access prediction using Markov chain model
public class SpatialAccessPredictor : IPredictiveModel
{
    private readonly MarkovChain<TileKey> _spatialChain;
    private readonly TimeSeriesModel _temporalModel;
    private readonly CircularBuffer<AccessRecord> _accessHistory;

    public SpatialAccessPredictor()
    {
        _spatialChain = new MarkovChain<TileKey>(order: 2);
        _temporalModel = new TimeSeriesModel(windowSize: 1000);
        _accessHistory = new CircularBuffer<AccessRecord>(10000);
    }

    public List<TilePrediction> PredictNextTiles(TileKey current, int maxPredictions)
    {
        var predictions = new List<TilePrediction>();

        // Get spatial predictions from Markov chain
        var spatialPredictions = _spatialChain.PredictNext(current, maxPredictions * 2);

        // Get temporal boost factors
        var now = DateTimeOffset.UtcNow;
        var temporalFactors = _temporalModel.GetAccessProbabilities(now);

        // Combine spatial and temporal predictions
        foreach (var spatial in spatialPredictions)
        {
            var tileKey = spatial.State;

            // Calculate combined probability
            var temporalBoost = temporalFactors.GetValueOrDefault(
                tileKey.Level, 1.0f);
            var combinedProb = spatial.Probability * temporalBoost;

            // Apply spatial coherence bonus
            var distance = CalculateTileDistance(current, tileKey);
            var distanceBonus = Math.Exp(-distance / 2.0); // Exponential decay

            predictions.Add(new TilePrediction
            {
                Key = tileKey,
                Probability = combinedProb * distanceBonus,
                Source = PredictionSource.SpatialTemporal
            });
        }

        // Add adjacent tiles with base probability
        var adjacent = GetAdjacentTiles(current);
        foreach (var adj in adjacent)
        {
            if (!predictions.Any(p => p.Key.Equals(adj)))
            {
                predictions.Add(new TilePrediction
                {
                    Key = adj,
                    Probability = 0.3, // Base probability for adjacency
                    Source = PredictionSource.Adjacency
                });
            }
        }

        return predictions
            .OrderByDescending(p => p.Probability)
            .Take(maxPredictions)
            .ToList();
    }
}
```

### Building progressive loading systems

Progressive loading transforms the user experience of viewing large geospatial imagery by providing immediate visual
feedback followed by incremental quality improvements. This approach leverages COG's multi-resolution structure to load
overview levels first, then progressively fetch higher-resolution tiles as needed, creating a smooth zoom and pan
experience even over limited bandwidth connections.

The implementation must coordinate multiple concerns: network request scheduling to avoid overwhelming connections,
priority queuing based on viewport visibility, smooth visual transitions between resolution levels, and memory
management to prevent excessive resource usage. Modern approaches employ WebGL or GPU-accelerated rendering to handle
smooth blending between tile levels.

```csharp
public class ProgressiveGeoTIFFLoader
{
    private readonly CloudOptimizedGeoTIFF _cog;
    private readonly IRenderer _renderer;
    private readonly PriorityQueue<TileRequest> _requestQueue;
    private readonly Dictionary<TileKey, LoadingState> _loadingStates;
    private readonly SemaphoreSlim _concurrencyLimit;

    public ProgressiveGeoTIFFLoader(
        CloudOptimizedGeoTIFF cog,
        IRenderer renderer,
        int maxConcurrentRequests = 6)
    {
        _cog = cog;
        _renderer = renderer;
        _requestQueue = new PriorityQueue<TileRequest>(TileRequestComparer.Instance);
        _loadingStates = new Dictionary<TileKey, LoadingState>();
        _concurrencyLimit = new SemaphoreSlim(maxConcurrentRequests);
    }

    public async Task LoadViewAsync(ViewportState viewport)
    {
        // Calculate visible tile ranges for each resolution level
        var tileRanges = CalculateVisibleTileRanges(viewport);

        // Cancel out-of-view pending requests
        CancelInvisibleRequests(tileRanges);

        // Queue new tile requests with priorities
        QueueTileRequests(tileRanges, viewport);

        // Process queue
        await ProcessRequestQueueAsync();
    }

    private List<LevelTileRange> CalculateVisibleTileRanges(ViewportState viewport)
    {
        var ranges = new List<LevelTileRange>();
        var metadata = _cog.Metadata;

        // Start from coarsest resolution
        for (int level = metadata.IFDs.Count - 1; level >= 0; level--)
        {
            var ifd = metadata.IFDs[level];
            var resolution = ifd.PixelSize; // meters per pixel

            // Check if this level is appropriate for current zoom
            var viewportResolution = viewport.GetResolution();
            var resolutionRatio = viewportResolution / resolution;

            if (resolutionRatio < 0.25)
            {
                // Too detailed for current view
                continue;
            }

            // Calculate visible tile bounds
            var geoBounds = viewport.GetGeographicBounds();
            var pixelBounds = ifd.GeoBoundsToPixel(geoBounds);

            var tileMinX = Math.Max(0, pixelBounds.Left / ifd.TileWidth);
            var tileMaxX = Math.Min(ifd.TilesAcross - 1,
                                  pixelBounds.Right / ifd.TileWidth);
            var tileMinY = Math.Max(0, pixelBounds.Top / ifd.TileHeight);
            var tileMaxY = Math.Min(ifd.TilesDown - 1,
                                  pixelBounds.Bottom / ifd.TileHeight);

            ranges.Add(new LevelTileRange
            {
                Level = level,
                ResolutionRatio = resolutionRatio,
                TileBounds = new TileBounds(tileMinX, tileMinY, tileMaxX, tileMaxY),
                Priority = CalculateLevelPriority(level, resolutionRatio)
            });

            if (resolutionRatio > 2.0)
            {
                // No need for coarser levels
                break;
            }
        }

        return ranges;
    }

    private void QueueTileRequests(
        List<LevelTileRange> tileRanges,
        ViewportState viewport)
    {
        foreach (var range in tileRanges)
        {
            var bounds = range.TileBounds;

            for (int y = bounds.MinY; y <= bounds.MaxY; y++)
            {
                for (int x = bounds.MinX; x <= bounds.MaxX; x++)
                {
                    var key = new TileKey(range.Level, x, y);

                    // Skip if already loaded or loading
                    if (_loadingStates.TryGetValue(key, out var state))
                    {
                        if (state.Status == LoadStatus.Loaded ||
                            state.Status == LoadStatus.Loading)
                        {
                            continue;
                        }
                    }

                    // Calculate tile-specific priority
                    var tilePriority = CalculateTilePriority(
                        key, range, viewport);

                    var request = new TileRequest
                    {
                        Key = key,
                        Priority = tilePriority,
                        RequestTime = DateTimeOffset.UtcNow,
                        ViewportVersion = viewport.Version
                    };

                    _requestQueue.Enqueue(request);
                    _loadingStates[key] = new LoadingState
                    {
                        Status = LoadStatus.Queued
                    };
                }
            }
        }
    }

    private double CalculateTilePriority(
        TileKey key,
        LevelTileRange range,
        ViewportState viewport)
    {
        // Base priority from resolution match
        double priority = range.Priority;

        // Distance from viewport center
        var tileCenter = GetTileCenter(key);
        var distance = viewport.Center.DistanceTo(tileCenter);
        var maxDistance = viewport.Diagonal;
        var distanceFactor = 1.0 - Math.Min(distance / maxDistance, 1.0);
        priority *= (1.0 + distanceFactor);

        // Boost for tiles already partially visible
        if (_renderer.IsTilePartiallyVisible(key))
        {
            priority *= 1.5;
        }

        // Penalize very fine detail levels
        if (range.ResolutionRatio < 0.5)
        {
            priority *= 0.7;
        }

        return priority;
    }

    private async Task ProcessRequestQueueAsync()
    {
        var activeTasks = new List<Task>();

        while (_requestQueue.Count > 0 || activeTasks.Count > 0)
        {
            // Start new requests up to concurrency limit
            while (_requestQueue.Count > 0 &&
                   activeTasks.Count < _concurrencyLimit.CurrentCount)
            {
                if (!_requestQueue.TryDequeue(out var request))
                    break;

                // Check if request is still valid
                if (!IsRequestValid(request))
                    continue;

                await _concurrencyLimit.WaitAsync();

                var task = LoadTileAsync(request)
                    .ContinueWith(t => _concurrencyLimit.Release());

                activeTasks.Add(task);
            }

            if (activeTasks.Count > 0)
            {
                // Wait for any task to complete
                var completed = await Task.WhenAny(activeTasks);
                activeTasks.Remove(completed);
            }
        }
    }

    private async Task LoadTileAsync(TileRequest request)
    {
        var key = request.Key;

        try
        {
            // Update state
            _loadingStates[key] = new LoadingState
            {
                Status = LoadStatus.Loading,
                StartTime = DateTimeOffset.UtcNow
            };

            // Check cache first
            var cached = await _cog.Cache.GetAsync(key);
            if (cached != null)
            {
                await RenderTileAsync(key, cached, immediate: true);
                return;
            }

            // Fetch from network
            var tileData = await _cog.FetchTileAsync(key);

            // Decode in background thread
            var decoded = await Task.Run(() => DecodeTile(tileData, key));

            // Render with smooth transition
            await RenderTileAsync(key, decoded, immediate: false);

            // Update state
            _loadingStates[key] = new LoadingState
            {
                Status = LoadStatus.Loaded,
                LoadTime = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            _loadingStates[key] = new LoadingState
            {
                Status = LoadStatus.Failed,
                Error = ex
            };

            // Log error
            LogTileLoadError(key, ex);
        }
    }

    private async Task RenderTileAsync(
        TileKey key,
        DecodedTile tile,
        bool immediate)
    {
        if (immediate)
        {
            await _renderer.RenderTileAsync(key, tile);
        }
        else
        {
            // Smooth fade-in transition
            await _renderer.RenderTileWithTransitionAsync(
                key, tile,
                transitionDuration: TimeSpan.FromMilliseconds(200));
        }

        // Check if we can remove lower resolution tiles
        CheckForObsoleteTiles(key);
    }
}
```

## 16.3 Coordinate System Integration

### Implementing projection transformations

Geospatial data exists in hundreds of coordinate reference systems (CRS), each optimized for different regions and use
cases. Accurate transformation between these systems requires understanding the mathematical models underlying map
projections, implementing high-precision transformation algorithms, and handling edge cases like datum shifts and
coordinate epoch changes.

The implementation must balance accuracy and performance. While iterative methods provide highest accuracy for complex
transformations, many use cases can employ faster approximate methods. The system implements a hierarchy of
transformation strategies, selecting the optimal approach based on accuracy requirements and transformation complexity.

```csharp
public class CoordinateTransformEngine
{
    private readonly IProjDatabase _projDb;
    private readonly TransformCache _cache;
    private readonly Dictionary<int, CoordinateSystem> _crsCache;

    public CoordinateTransformEngine(string projDatabasePath = null)
    {
        _projDb = new ProjDatabase(projDatabasePath ?? GetDefaultProjPath());
        _cache = new TransformCache(maxEntries: 1000);
        _crsCache = new Dictionary<int, CoordinateSystem>();
    }

    public ICoordinateTransform CreateTransform(int sourceSrid, int targetSrid)
    {
        var key = new TransformKey(sourceSrid, targetSrid);

        // Check cache
        if (_cache.TryGetTransform(key, out var cached))
        {
            return cached;
        }

        // Build transformation
        var source = GetOrCreateCRS(sourceSrid);
        var target = GetOrCreateCRS(targetSrid);

        var transform = BuildOptimalTransform(source, target);
        _cache.AddTransform(key, transform);

        return transform;
    }

    private ICoordinateTransform BuildOptimalTransform(
        CoordinateSystem source,
        CoordinateSystem target)
    {
        // Check for identity transform
        if (source.IsEquivalent(target))
        {
            return new IdentityTransform();
        }

        // Check for simple datum shift
        if (source.Projection.Equals(target.Projection) &&
            !source.Datum.Equals(target.Datum))
        {
            return new DatumTransform(source.Datum, target.Datum);
        }

        // Check for common transformation paths
        var directPath = _projDb.FindDirectTransformation(source, target);
        if (directPath != null)
        {
            return CreateDirectTransform(directPath);
        }

        // Build composite transformation through WGS84
        var toWgs84 = BuildTransformToWgs84(source);
        var fromWgs84 = BuildTransformFromWgs84(target);

        return new CompositeTransform(toWgs84, fromWgs84);
    }

    // High-performance transformation for point arrays
    public unsafe void TransformPoints(
        Span<GeoPoint> points,
        ICoordinateTransform transform)
    {
        // Check for SIMD-optimizable transformations
        if (transform is AffineTransform affine)
        {
            TransformPointsSIMD(points, affine);
            return;
        }

        // Use parallel processing for complex transformations
        if (points.Length > 1000 && transform.IsThreadSafe)
        {
            Parallel.For(0, points.Length, i =>
            {
                points[i] = transform.Transform(points[i]);
            });
        }
        else
        {
            // Sequential transformation
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = transform.Transform(points[i]);
            }
        }
    }

    private unsafe void TransformPointsSIMD(
        Span<GeoPoint> points,
        AffineTransform transform)
    {
        // Extract transformation matrix components
        var m = transform.Matrix;
        var a = Vector256.Create(m.A);
        var b = Vector256.Create(m.B);
        var c = Vector256.Create(m.C);
        var d = Vector256.Create(m.D);
        var tx = Vector256.Create(m.Tx);
        var ty = Vector256.Create(m.Ty);

        fixed (GeoPoint* pPoints = points)
        {
            var count = points.Length;
            var simdCount = count - (count % 4);

            // Process 4 points at a time
            for (int i = 0; i < simdCount; i += 4)
            {
                // Load X coordinates
                var x = Vector256.Create(
                    pPoints[i].X,
                    pPoints[i + 1].X,
                    pPoints[i + 2].X,
                    pPoints[i + 3].X,
                    0, 0, 0, 0);

                // Load Y coordinates
                var y = Vector256.Create(
                    pPoints[i].Y,
                    pPoints[i + 1].Y,
                    pPoints[i + 2].Y,
                    pPoints[i + 3].Y,
                    0, 0, 0, 0);

                // Apply transformation: x' = ax + by + tx
                var xPrime = Vector256.Add(
                    Vector256.Add(
                        Vector256.Multiply(a, x),
                        Vector256.Multiply(b, y)),
                    tx);

                // y' = cx + dy + ty
                var yPrime = Vector256.Add(
                    Vector256.Add(
                        Vector256.Multiply(c, x),
                        Vector256.Multiply(d, y)),
                    ty);

                // Store results
                pPoints[i].X = xPrime.GetElement(0);
                pPoints[i].Y = yPrime.GetElement(0);
                pPoints[i + 1].X = xPrime.GetElement(1);
                pPoints[i + 1].Y = yPrime.GetElement(1);
                pPoints[i + 2].X = xPrime.GetElement(2);
                pPoints[i + 2].Y = yPrime.GetElement(2);
                pPoints[i + 3].X = xPrime.GetElement(3);
                pPoints[i + 3].Y = yPrime.GetElement(3);
            }

            // Handle remaining points
            for (int i = simdCount; i < count; i++)
            {
                var p = pPoints[i];
                pPoints[i].X = m.A * p.X + m.B * p.Y + m.Tx;
                pPoints[i].Y = m.C * p.X + m.D * p.Y + m.Ty;
            }
        }
    }
}

// Complex projection transformations
public class ProjectionTransform : ICoordinateTransform
{
    private readonly IProjection _sourceProj;
    private readonly IProjection _targetProj;
    private readonly IDatum _sourceDatum;
    private readonly IDatum _targetDatum;

    public GeoPoint Transform(GeoPoint point)
    {
        // Step 1: Unproject from source to geographic
        var geographic = _sourceProj.Inverse(point);

        // Step 2: Apply datum transformation if needed
        if (!_sourceDatum.Equals(_targetDatum))
        {
            geographic = TransformDatum(geographic, _sourceDatum, _targetDatum);
        }

        // Step 3: Project to target
        return _targetProj.Forward(geographic);
    }

    private GeoPoint TransformDatum(
        GeoPoint point,
        IDatum source,
        IDatum target)
    {
        // Convert to geocentric coordinates
        var geocentric = source.Ellipsoid.ToGeocentric(point);

        // Apply datum shift parameters
        if (source.HasTransformationTo(target))
        {
            var transform = source.GetTransformationTo(target);
            geocentric = ApplyHelmertTransform(geocentric, transform);
        }
        else
        {
            // Use WGS84 as pivot
            var toWgs84 = source.ToWGS84Parameters;
            var fromWgs84 = target.FromWGS84Parameters;

            geocentric = ApplyHelmertTransform(geocentric, toWgs84);
            geocentric = ApplyHelmertTransform(geocentric, fromWgs84.Inverse());
        }

        // Convert back to geographic
        return target.Ellipsoid.ToGeographic(geocentric);
    }

    private GeocentricPoint ApplyHelmertTransform(
        GeocentricPoint point,
        HelmertParameters parameters)
    {
        // 7-parameter Helmert transformation
        var scale = 1.0 + parameters.Scale * 1e-6;

        // Rotation matrix from small angles
        var rx = parameters.RotX * Math.PI / 648000.0; // arc seconds to radians
        var ry = parameters.RotY * Math.PI / 648000.0;
        var rz = parameters.RotZ * Math.PI / 648000.0;

        // Apply transformation
        var x = scale * (point.X + rz * point.Y - ry * point.Z) + parameters.Dx;
        var y = scale * (-rz * point.X + point.Y + rx * point.Z) + parameters.Dy;
        var z = scale * (ry * point.X - rx * point.Y + point.Z) + parameters.Dz;

        return new GeocentricPoint(x, y, z);
    }
}
```

### Working with EPSG codes and spatial reference systems

The EPSG database contains over 10,000 coordinate reference system definitions, making it the de facto standard for CRS
identification. Efficient integration requires parsing and indexing EPSG data, implementing WKT (Well-Known Text)
parsing and generation, and providing user-friendly APIs that hide complexity while maintaining precision.

The implementation uses a lazy-loading approach to minimize memory usage while providing fast access to commonly used
CRS definitions. Spatial indexing enables efficient searches for CRS by geographic area, while caching ensures repeated
operations maintain high performance.

```csharp
public class EPSGDatabase
{
    private readonly string _databasePath;
    private readonly Dictionary<int, CRSDefinition> _crsCache;
    private readonly SpatialIndex<int> _spatialIndex;
    private readonly object _lock = new object();
    private bool _isInitialized;

    public EPSGDatabase(string databasePath)
    {
        _databasePath = databasePath;
        _crsCache = new Dictionary<int, CRSDefinition>();
        _spatialIndex = new RTreeSpatialIndex<int>();
    }

    public async Task<CoordinateReferenceSystem> GetCRSAsync(int epsgCode)
    {
        await EnsureInitializedAsync();

        // Check cache
        lock (_lock)
        {
            if (_crsCache.TryGetValue(epsgCode, out var cached))
            {
                return cached.ToCRS();
            }
        }

        // Load from database
        var definition = await LoadCRSDefinitionAsync(epsgCode);
        if (definition == null)
        {
            throw new ArgumentException($"EPSG:{epsgCode} not found");
        }

        // Parse and cache
        var crs = ParseCRSDefinition(definition);

        lock (_lock)
        {
            _crsCache[epsgCode] = definition;
        }

        return crs;
    }

    public async Task<List<CRSInfo>> FindCRSForLocationAsync(
        double longitude,
        double latitude)
    {
        await EnsureInitializedAsync();

        var point = new Point(longitude, latitude);
        var candidates = _spatialIndex.Query(point);

        var results = new List<CRSInfo>();

        foreach (var epsgCode in candidates)
        {
            var crs = await GetCRSAsync(epsgCode);
            var info = new CRSInfo
            {
                EPSGCode = epsgCode,
                Name = crs.Name,
                Type = crs.Type,
                AreaOfUse = crs.AreaOfUse,
                Deprecated = crs.IsDeprecated
            };

            // Calculate suitability score
            info.Suitability = CalculateSuitability(crs, longitude, latitude);
            results.Add(info);
        }

        return results
            .Where(r => r.Suitability > 0)
            .OrderByDescending(r => r.Suitability)
            .ToList();
    }

    private CoordinateReferenceSystem ParseCRSDefinition(CRSDefinition definition)
    {
        // Handle different CRS types
        switch (definition.Type)
        {
            case CRSType.Geographic2D:
                return ParseGeographicCRS(definition);

            case CRSType.Projected:
                return ParseProjectedCRS(definition);

            case CRSType.Compound:
                return ParseCompoundCRS(definition);

            case CRSType.Engineering:
                return ParseEngineeringCRS(definition);

            default:
                throw new NotSupportedException(
                    $"CRS type {definition.Type} not supported");
        }
    }

    private ProjectedCRS ParseProjectedCRS(CRSDefinition definition)
    {
        // Parse base geographic CRS
        var baseCRS = ParseGeographicCRS(definition.BaseCRS);

        // Parse projection
        var projection = CreateProjection(definition.Projection);

        // Parse coordinate system
        var cs = ParseCoordinateSystem(definition.CoordinateSystem);

        return new ProjectedCRS
        {
            Code = definition.Code,
            Name = definition.Name,
            BaseCRS = baseCRS,
            Projection = projection,
            CoordinateSystem = cs,
            AreaOfUse = ParseAreaOfUse(definition.AreaOfUse)
        };
    }

    private IProjection CreateProjection(ProjectionDefinition projDef)
    {
        var factory = ProjectionFactory.Instance;

        // Create projection with parameters
        var projection = factory.CreateProjection(projDef.Method);

        foreach (var param in projDef.Parameters)
        {
            projection.SetParameter(param.Name, param.Value, param.Unit);
        }

        projection.Initialize();
        return projection;
    }
}

// WKT parsing and generation
public class WKTParser
{
    private readonly Stack<Token> _tokens;
    private Token _current;

    public CoordinateReferenceSystem Parse(string wkt)
    {
        _tokens = new Stack<Token>(Tokenize(wkt).Reverse());
        _current = _tokens.Pop();

        return ParseCRS();
    }

    private CoordinateReferenceSystem ParseCRS()
    {
        switch (_current.Value)
        {
            case "PROJCS":
                return ParseProjectedCRS();

            case "GEOGCS":
                return ParseGeographicCRS();

            case "COMPD_CS":
                return ParseCompoundCRS();

            default:
                throw new FormatException($"Unknown CRS type: {_current.Value}");
        }
    }

    private ProjectedCRS ParseProjectedCRS()
    {
        Expect("PROJCS");
        Expect("[");

        var name = ParseQuotedString();
        Expect(",");

        var geogCS = ParseGeographicCRS();
        Expect(",");

        var projection = ParseProjection();

        var parameters = new List<ProjectionParameter>();
        while (_current.Type == TokenType.Identifier &&
               _current.Value == "PARAMETER")
        {
            parameters.Add(ParseParameter());

            if (_current.Value == ",")
            {
                Advance();
            }
        }

        var unit = ParseUnit();

        // Optional axes
        var axes = new List<Axis>();
        while (_current.Type == TokenType.Identifier &&
               _current.Value == "AXIS")
        {
            axes.Add(ParseAxis());

            if (_current.Value == ",")
            {
                Advance();
            }
        }

        // Optional authority
        Authority authority = null;
        if (_current.Type == TokenType.Identifier &&
            _current.Value == "AUTHORITY")
        {
            authority = ParseAuthority();
        }

        Expect("]");

        return new ProjectedCRS
        {
            Name = name,
            BaseCRS = geogCS,
            Projection = CreateProjectionFromWKT(projection, parameters),
            Unit = unit,
            Axes = axes,
            Authority = authority
        };
    }
}

// Spatial index for CRS area of use
public class CRSSpatialIndex
{
    private readonly STRtree _index;
    private readonly Dictionary<int, Envelope> _envelopes;

    public CRSSpatialIndex()
    {
        _index = new STRtree();
        _envelopes = new Dictionary<int, Envelope>();
    }

    public void AddCRS(int epsgCode, BoundingBox areaOfUse)
    {
        var envelope = new Envelope(
            areaOfUse.MinX, areaOfUse.MaxX,
            areaOfUse.MinY, areaOfUse.MaxY);

        _envelopes[epsgCode] = envelope;
        _index.Insert(envelope, epsgCode);
    }

    public List<int> Query(double longitude, double latitude)
    {
        var point = new Coordinate(longitude, latitude);
        var envelope = new Envelope(point);

        return _index.Query(envelope)
            .Cast<int>()
            .Where(epsgCode =>
            {
                var crsEnvelope = _envelopes[epsgCode];
                return crsEnvelope.Contains(point);
            })
            .ToList();
    }

    public void Build()
    {
        _index.Build();
    }
}
```

### Handling datum transformations and accuracy

Datum transformations introduce complexity beyond simple mathematical projections, involving physical models of Earth's
shape and temporal variations. High-accuracy transformations must account for tectonic plate movement, gravitational
variations, and coordinate epoch differences. The implementation provides multiple transformation paths with explicit
accuracy estimates, enabling applications to choose appropriate methods based on requirements.

Modern datum transformations increasingly rely on grid-based methods that provide centimeter-level accuracy. The system
implements efficient grid interpolation with caching strategies to balance accuracy and performance, while providing
fallback to parametric transformations when grids are unavailable.

```csharp
public class DatumTransformationEngine
{
    private readonly GridShiftRepository _gridRepo;
    private readonly TransformationPathFinder _pathFinder;
    private readonly AccuracyEstimator _accuracyEstimator;

    public DatumTransformationEngine(string gridDataPath)
    {
        _gridRepo = new GridShiftRepository(gridDataPath);
        _pathFinder = new TransformationPathFinder();
        _accuracyEstimator = new AccuracyEstimator();
    }

    public async Task<TransformationResult> TransformAsync(
        GeoPoint point,
        Datum sourceDatum,
        Datum targetDatum,
        double? sourceEpoch = null,
        double? targetEpoch = null,
        AccuracyRequirement requirement = AccuracyRequirement.Default)
    {
        // Find all possible transformation paths
        var paths = _pathFinder.FindPaths(sourceDatum, targetDatum);

        if (paths.Count == 0)
        {
            throw new TransformationException(
                $"No transformation path found from {sourceDatum} to {targetDatum}");
        }

        // Evaluate paths based on accuracy and performance
        var evaluatedPaths = await EvaluatePathsAsync(
            paths, point, requirement);

        // Select optimal path
        var selectedPath = SelectOptimalPath(evaluatedPaths, requirement);

        // Apply transformation
        var result = await ApplyTransformationPathAsync(
            point, selectedPath, sourceEpoch, targetEpoch);

        return result;
    }

    private async Task<TransformationResult> ApplyTransformationPathAsync(
        GeoPoint point,
        TransformationPath path,
        double? sourceEpoch,
        double? targetEpoch)
    {
        var currentPoint = point;
        var currentEpoch = sourceEpoch;
        var totalError = 0.0;

        foreach (var step in path.Steps)
        {
            var stepResult = await ApplyTransformationStepAsync(
                currentPoint, step, currentEpoch);

            currentPoint = stepResult.TransformedPoint;
            currentEpoch = step.TargetEpoch ?? currentEpoch;
            totalError += stepResult.EstimatedError;
        }

        // Apply epoch transformation if needed
        if (targetEpoch.HasValue && currentEpoch.HasValue &&
            Math.Abs(targetEpoch.Value - currentEpoch.Value) > 0.001)
        {
            var epochResult = ApplyEpochTransformation(
                currentPoint, path.TargetDatum,
                currentEpoch.Value, targetEpoch.Value);

            currentPoint = epochResult.TransformedPoint;
            totalError += epochResult.EstimatedError;
        }

        return new TransformationResult
        {
            TransformedPoint = currentPoint,
            EstimatedError = totalError,
            TransformationPath = path,
            QualityIndicators = CalculateQualityIndicators(path, totalError)
        };
    }

    private async Task<StepResult> ApplyTransformationStepAsync(
        GeoPoint point,
        TransformationStep step,
        double? epoch)
    {
        switch (step.Method)
        {
            case TransformationMethod.GridShift:
                return await ApplyGridShiftAsync(point, step);

            case TransformationMethod.Helmert7Parameter:
                return ApplyHelmert7Parameter(point, step);

            case TransformationMethod.Molodensky:
                return ApplyMolodensky(point, step);

            case TransformationMethod.NTv2:
                return await ApplyNTv2GridAsync(point, step);

            case TransformationMethod.NADCON5:
                return await ApplyNADCON5Async(point, step, epoch);

            default:
                throw new NotSupportedException(
                    $"Transformation method {step.Method} not supported");
        }
    }

    private async Task<StepResult> ApplyGridShiftAsync(
        GeoPoint point,
        TransformationStep step)
    {
        // Load grid file
        var grid = await _gridRepo.LoadGridAsync(step.GridFile);

        // Check if point is within grid bounds
        if (!grid.Contains(point))
        {
            throw new TransformationException(
                $"Point {point} is outside grid bounds");
        }

        // Perform bilinear interpolation
        var shift = InterpolateGridShift(grid, point);

        // Apply shift
        var transformed = new GeoPoint(
            point.Longitude + shift.DeltaLongitude,
            point.Latitude + shift.DeltaLatitude,
            point.Height + shift.DeltaHeight);

        // Estimate interpolation error
        var error = EstimateInterpolationError(grid, point);

        return new StepResult
        {
            TransformedPoint = transformed,
            EstimatedError = error
        };
    }

    private GridShift InterpolateGridShift(ShiftGrid grid, GeoPoint point)
    {
        // Find surrounding grid cells
        var gridX = (point.Longitude - grid.MinLongitude) / grid.CellSizeLongitude;
        var gridY = (point.Latitude - grid.MinLatitude) / grid.CellSizeLatitude;

        var x0 = (int)Math.Floor(gridX);
        var y0 = (int)Math.Floor(gridY);
        var x1 = x0 + 1;
        var y1 = y0 + 1;

        // Get shift values at corners
        var s00 = grid.GetShift(x0, y0);
        var s10 = grid.GetShift(x1, y0);
        var s01 = grid.GetShift(x0, y1);
        var s11 = grid.GetShift(x1, y1);

        // Bilinear interpolation weights
        var fx = gridX - x0;
        var fy = gridY - y0;

        // Interpolate longitude shift
        var deltaLon = (1 - fx) * (1 - fy) * s00.DeltaLongitude +
                       fx * (1 - fy) * s10.DeltaLongitude +
                       (1 - fx) * fy * s01.DeltaLongitude +
                       fx * fy * s11.DeltaLongitude;

        // Interpolate latitude shift
        var deltaLat = (1 - fx) * (1 - fy) * s00.DeltaLatitude +
                       fx * (1 - fy) * s10.DeltaLatitude +
                       (1 - fx) * fy * s01.DeltaLatitude +
                       fx * fy * s11.DeltaLatitude;

        // Interpolate height shift if available
        var deltaHeight = 0.0;
        if (grid.HasHeightShifts)
        {
            deltaHeight = (1 - fx) * (1 - fy) * s00.DeltaHeight +
                         fx * (1 - fy) * s10.DeltaHeight +
                         (1 - fx) * fy * s01.DeltaHeight +
                         fx * fy * s11.DeltaHeight;
        }

        return new GridShift(deltaLon, deltaLat, deltaHeight);
    }

    // High-accuracy epoch transformations for plate motion
    private EpochResult ApplyEpochTransformation(
        GeoPoint point,
        Datum datum,
        double fromEpoch,
        double toEpoch)
    {
        var deltaTime = toEpoch - fromEpoch;

        // Get plate motion model for point location
        var plateModel = GetPlateMotionModel(point, datum);

        if (plateModel == null)
        {
            // No plate motion model available
            return new EpochResult
            {
                TransformedPoint = point,
                EstimatedError = Math.Abs(deltaTime) * 0.001 // 1mm/year default
            };
        }

        // Convert to cartesian for velocity application
        var cartesian = datum.Ellipsoid.ToCartesian(point);

        // Apply plate motion velocities
        var velocity = plateModel.GetVelocity(point);
        cartesian.X += velocity.Vx * deltaTime;
        cartesian.Y += velocity.Vy * deltaTime;
        cartesian.Z += velocity.Vz * deltaTime;

        // Convert back to geographic
        var transformed = datum.Ellipsoid.ToGeographic(cartesian);

        // Estimate error based on velocity uncertainty
        var error = Math.Sqrt(
            Math.Pow(velocity.SigmaVx * deltaTime, 2) +
            Math.Pow(velocity.SigmaVy * deltaTime, 2) +
            Math.Pow(velocity.SigmaVz * deltaTime, 2));

        return new EpochResult
        {
            TransformedPoint = transformed,
            EstimatedError = error
        };
    }
}
```

## 16.4 Map Tile Generation

### Implementing slippy map tile standards

The slippy map tile standard, pioneered by OpenStreetMap, has become the de facto standard for web mapping. This z/x/y
tile scheme uses a quadtree structure where each zoom level contains 4^z tiles, with x representing the column and y the
row. Implementing this standard requires understanding the mathematical relationship between geographic coordinates and
tile indices, handling projection transformations efficiently, and generating tiles that seamlessly align at boundaries.

The tile generation system must handle multiple challenges: edge cases at projection boundaries, particularly near poles
where Mercator projection becomes undefined; tile boundary alignment to prevent visible seams; and efficient batch
processing for millions of tiles. The implementation uses parallel processing with careful memory management to maximize
throughput while preventing resource exhaustion.

```csharp
public class SlippyMapTileGenerator
{
    private readonly ITileRenderer _renderer;
    private readonly ITileStorage _storage;
    private readonly ParallelOptions _parallelOptions;
    private readonly TileGenerationMetrics _metrics;

    public SlippyMapTileGenerator(
        ITileRenderer renderer,
        ITileStorage storage,
        int maxParallelism = -1)
    {
        _renderer = renderer;
        _storage = storage;
        _parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxParallelism > 0 ?
                maxParallelism : Environment.ProcessorCount * 2
        };
        _metrics = new TileGenerationMetrics();
    }

    public async Task GenerateTilesAsync(
        IGeospatialDataSource source,
        TileGenerationOptions options)
    {
        // Validate zoom range
        if (options.MinZoom < 0 || options.MaxZoom > 24)
        {
            throw new ArgumentException("Zoom levels must be between 0 and 24");
        }

        // Calculate total tile count for progress reporting
        var totalTiles = CalculateTotalTiles(
            options.MinZoom, options.MaxZoom, options.BoundingBox);

        var progress = new Progress<TileGenerationProgress>(
            p => options.ProgressCallback?.Invoke(p));

        // Generate tiles level by level
        for (int zoom = options.MinZoom; zoom <= options.MaxZoom; zoom++)
        {
            await GenerateZoomLevelAsync(
                source, zoom, options, progress, totalTiles);
        }

        // Generate metadata files
        await GenerateMetadataAsync(options);
    }

    private async Task GenerateZoomLevelAsync(
        IGeospatialDataSource source,
        int zoom,
        TileGenerationOptions options,
        IProgress<TileGenerationProgress> progress,
        long totalTiles)
    {
        // Calculate tile bounds for this zoom level
        var tileBounds = CalculateTileBounds(zoom, options.BoundingBox);

        // Create concurrent collections for tile generation
        var tileQueue = new ConcurrentQueue<TileCoordinate>();
        var completedTiles = new ConcurrentBag<TileResult>();

        // Populate tile queue
        for (int x = tileBounds.MinX; x <= tileBounds.MaxX; x++)
        {
            for (int y = tileBounds.MinY; y <= tileBounds.MaxY; y++)
            {
                tileQueue.Enqueue(new TileCoordinate(zoom, x, y));
            }
        }

        // Process tiles in parallel
        var tileCount = tileQueue.Count;
        var processed = 0;

        await Parallel.ForEachAsync(
            Enumerable.Range(0, tileCount),
            _parallelOptions,
            async (_, cancellationToken) =>
            {
                if (!tileQueue.TryDequeue(out var tile))
                    return;

                try
                {
                    var result = await GenerateTileAsync(
                        source, tile, options, cancellationToken);

                    completedTiles.Add(result);

                    // Report progress
                    var currentProcessed = Interlocked.Increment(ref processed);
                    if (currentProcessed % 100 == 0)
                    {
                        progress.Report(new TileGenerationProgress
                        {
                            CurrentZoom = zoom,
                            TilesProcessed = _metrics.TotalProcessed,
                            TotalTiles = totalTiles,
                            CurrentTile = tile,
                            TilesPerSecond = _metrics.GetTilesPerSecond()
                        });
                    }
                }
                catch (Exception ex)
                {
                    _metrics.RecordError(tile, ex);

                    if (!options.ContinueOnError)
                        throw;
                }
            });

        // Save completed tiles
        await SaveTilesBatchAsync(completedTiles, options);
    }

    private async Task<TileResult> GenerateTileAsync(
        IGeospatialDataSource source,
        TileCoordinate tile,
        TileGenerationOptions options,
        CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();

        // Calculate geographic bounds for tile
        var bounds = TileToGeographicBounds(tile);

        // Transform to data source CRS if needed
        if (source.CRS != EPSG.WebMercator)
        {
            bounds = TransformBounds(bounds, EPSG.WGS84, source.CRS);
        }

        // Expand bounds slightly to prevent edge artifacts
        var expandedBounds = ExpandBounds(bounds, pixels: 2, tile: tile);

        // Fetch data for tile
        var data = await source.GetDataAsync(expandedBounds, cancellationToken);

        if (data.IsEmpty && options.SkipEmptyTiles)
        {
            return new TileResult
            {
                Tile = tile,
                IsEmpty = true,
                GenerationTime = Stopwatch.GetElapsedTime(startTime)
            };
        }

        // Render tile
        var renderedTile = await _renderer.RenderAsync(
            data,
            new RenderContext
            {
                OutputSize = new Size(options.TileSize, options.TileSize),
                Bounds = bounds,
                TargetCRS = EPSG.WebMercator,
                RenderOptions = options.RenderOptions
            },
            cancellationToken);

        // Apply post-processing if configured
        if (options.PostProcessors?.Any() == true)
        {
            foreach (var processor in options.PostProcessors)
            {
                renderedTile = await processor.ProcessAsync(
                    renderedTile, tile, cancellationToken);
            }
        }

        // Encode tile
        var encoded = await EncodeTileAsync(
            renderedTile, options.Format, options.EncodingOptions);

        var generationTime = Stopwatch.GetElapsedTime(startTime);
        _metrics.RecordTileGenerated(tile, encoded.Length, generationTime);

        return new TileResult
        {
            Tile = tile,
            Data = encoded,
            IsEmpty = false,
            GenerationTime = generationTime,
            Metrics = new TileMetrics
            {
                UncompressedSize = renderedTile.Width * renderedTile.Height * 4,
                CompressedSize = encoded.Length,
                RenderTime = renderedTile.RenderTime,
                EncodingTime = generationTime - renderedTile.RenderTime
            }
        };
    }

    // Tile coordinate conversions
    public static BoundingBox TileToGeographicBounds(TileCoordinate tile)
    {
        var n = Math.Pow(2, tile.Zoom);

        var minLon = tile.X / n * 360.0 - 180.0;
        var maxLon = (tile.X + 1) / n * 360.0 - 180.0;

        var minLatRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * (tile.Y + 1) / n)));
        var maxLatRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * tile.Y / n)));

        var minLat = minLatRad * 180.0 / Math.PI;
        var maxLat = maxLatRad * 180.0 / Math.PI;

        return new BoundingBox(minLon, minLat, maxLon, maxLat);
    }

    public static TileCoordinate GeographicToTile(
        double longitude,
        double latitude,
        int zoom)
    {
        var n = Math.Pow(2, zoom);

        var x = (int)Math.Floor((longitude + 180.0) / 360.0 * n);

        var latRad = latitude * Math.PI / 180.0;
        var y = (int)Math.Floor(
            (1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI)
            / 2.0 * n);

        // Clamp to valid range
        x = Math.Max(0, Math.Min((int)n - 1, x));
        y = Math.Max(0, Math.Min((int)n - 1, y));

        return new TileCoordinate(zoom, x, y);
    }

    // Metatile support for reducing edge artifacts
    private async Task<List<TileResult>> GenerateMetatileAsync(
        IGeospatialDataSource source,
        MetatileCoordinate metatile,
        TileGenerationOptions options,
        CancellationToken cancellationToken)
    {
        // Render larger area covering multiple tiles
        var metaBounds = CalculateMetatileBounds(metatile);

        var data = await source.GetDataAsync(metaBounds, cancellationToken);

        var renderedMetatile = await _renderer.RenderAsync(
            data,
            new RenderContext
            {
                OutputSize = new Size(
                    options.TileSize * metatile.Size,
                    options.TileSize * metatile.Size),
                Bounds = metaBounds,
                TargetCRS = EPSG.WebMercator,
                RenderOptions = options.RenderOptions
            },
            cancellationToken);

        // Split metatile into individual tiles
        var results = new List<TileResult>();

        for (int dx = 0; dx < metatile.Size; dx++)
        {
            for (int dy = 0; dy < metatile.Size; dy++)
            {
                var tile = new TileCoordinate(
                    metatile.Zoom,
                    metatile.X + dx,
                    metatile.Y + dy);

                // Extract tile from metatile
                var tileBitmap = ExtractTileFromMetatile(
                    renderedMetatile, dx, dy, options.TileSize);

                // Encode and save
                var encoded = await EncodeTileAsync(
                    tileBitmap, options.Format, options.EncodingOptions);

                results.Add(new TileResult
                {
                    Tile = tile,
                    Data = encoded,
                    IsEmpty = false
                });
            }
        }

        return results;
    }
}
```

### Optimizing tile rendering performance

High-performance tile rendering requires careful orchestration of CPU and GPU resources, intelligent caching of
intermediate results, and vectorized operations for common transformations. The implementation employs multiple
optimization strategies: spatial indexing for efficient data queries, level-of-detail selection based on zoom level, and
parallel rendering pipelines that maximize hardware utilization.

Modern tile renderers must handle diverse data types—vector features, raster imagery, and terrain elevation models—while
maintaining consistent performance. The system implements specialized rendering paths for each data type, with automatic
fallback to software rendering when GPU acceleration is unavailable.

```csharp
public class OptimizedTileRenderer : ITileRenderer
{
    private readonly IRenderPipelineFactory _pipelineFactory;
    private readonly RenderCache _cache;
    private readonly TileRenderMetrics _metrics;
    private readonly int _gpuDeviceCount;

    public OptimizedTileRenderer(TileRendererOptions options)
    {
        _pipelineFactory = new RenderPipelineFactory(options);
        _cache = new RenderCache(options.CacheSize);
        _metrics = new TileRenderMetrics();
        _gpuDeviceCount = GetAvailableGPUCount();
    }

    public async Task<RenderedTile> RenderAsync(
        ISpatialData data,
        RenderContext context,
        CancellationToken cancellationToken = default)
    {
        var startTime = Stopwatch.GetTimestamp();

        // Check cache for previously rendered tile
        var cacheKey = GenerateCacheKey(data, context);
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            _metrics.RecordCacheHit();
            return cached;
        }

        // Select optimal rendering pipeline
        var pipeline = SelectRenderPipeline(data, context);

        // Prepare render target
        using var renderTarget = CreateRenderTarget(context.OutputSize);

        // Execute rendering pipeline
        var rendered = await pipeline.ExecuteAsync(
            data, renderTarget, context, cancellationToken);

        // Cache result
        _cache.SetValue(cacheKey, rendered);

        // Record metrics
        var renderTime = Stopwatch.GetElapsedTime(startTime);
        _metrics.RecordRender(pipeline.Type, renderTime);

        rendered.RenderTime = renderTime;
        return rendered;
    }

    private IRenderPipeline SelectRenderPipeline(
        ISpatialData data,
        RenderContext context)
    {
        // Analyze data characteristics
        var analysis = AnalyzeData(data);

        // GPU-accelerated paths
        if (_gpuDeviceCount > 0 && context.AllowGPU)
        {
            if (analysis.IsRasterData && analysis.PixelCount > 1_000_000)
            {
                return _pipelineFactory.CreateGPURasterPipeline();
            }

            if (analysis.IsVectorData && analysis.FeatureCount > 10_000)
            {
                return _pipelineFactory.CreateGPUVectorPipeline();
            }

            if (analysis.HasTerrainData)
            {
                return _pipelineFactory.CreateGPUTerrainPipeline();
            }
        }

        // CPU-optimized paths
        if (analysis.IsVectorData)
        {
            return analysis.FeatureCount < 1000 ?
                _pipelineFactory.CreateSimpleVectorPipeline() :
                _pipelineFactory.CreateSIMDVectorPipeline();
        }

        if (analysis.IsRasterData)
        {
            return _pipelineFactory.CreateSIMDRasterPipeline();
        }

        // Fallback
        return _pipelineFactory.CreateGeneralPipeline();
    }
}

// SIMD-accelerated raster rendering
public class SIMDRasterPipeline : IRenderPipeline
{
    public async Task<RenderedTile> ExecuteAsync(
        ISpatialData data,
        RenderTarget target,
        RenderContext context,
        CancellationToken cancellationToken)
    {
        var rasterData = (RasterData)data;

        // Calculate transformation matrix
        var transform = CalculateTransform(
            rasterData.Bounds, context.Bounds, context.OutputSize);

        // Prepare output buffer
        var outputBuffer = target.GetPixelBuffer();
        var width = context.OutputSize.Width;
        var height = context.OutputSize.Height;

        // Process in parallel strips for cache efficiency
        var stripHeight = 64; // Optimize for L2 cache
        var stripCount = (height + stripHeight - 1) / stripHeight;

        await Parallel.ForEachAsync(
            Enumerable.Range(0, stripCount),
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            },
            async (stripIndex, ct) =>
            {
                await ProcessRasterStripSIMD(
                    rasterData, outputBuffer, transform,
                    stripIndex * stripHeight,
                    Math.Min(stripHeight, height - stripIndex * stripHeight),
                    width, ct);
            });

        return new RenderedTile
        {
            Width = width,
            Height = height,
            PixelData = outputBuffer,
            Format = PixelFormat.RGBA8
        };
    }

    private unsafe Task ProcessRasterStripSIMD(
        RasterData source,
        byte[] output,
        Matrix3x2 transform,
        int startY,
        int stripHeight,
        int outputWidth,
        CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            // Prepare inverse transform for source sampling
            Matrix3x2.Invert(transform, out var invTransform);

            // Extract transform components for SIMD
            var m11 = Vector256.Create(invTransform.M11);
            var m12 = Vector256.Create(invTransform.M12);
            var m21 = Vector256.Create(invTransform.M21);
            var m22 = Vector256.Create(invTransform.M22);
            var dx = Vector256.Create(invTransform.M31);
            var dy = Vector256.Create(invTransform.M32);

            // Process 8 pixels at a time
            var xIndices = Vector256.Create(0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f);

            fixed (byte* pOutput = output)
            fixed (byte* pSource = source.PixelData)
            {
                for (int y = startY; y < startY + stripHeight; y++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    var yVec = Vector256.Create((float)y);
                    var outputRow = pOutput + (y * outputWidth * 4);

                    for (int x = 0; x < outputWidth; x += 8)
                    {
                        var xVec = Vector256.Add(Vector256.Create((float)x), xIndices);

                        // Transform coordinates
                        var srcX = Vector256.Add(
                            Vector256.Add(
                                Vector256.Multiply(m11, xVec),
                                Vector256.Multiply(m12, yVec)),
                            dx);

                        var srcY = Vector256.Add(
                            Vector256.Add(
                                Vector256.Multiply(m21, xVec),
                                Vector256.Multiply(m22, yVec)),
                            dy);

                        // Sample source with bilinear interpolation
                        SampleBilinearSIMD(
                            pSource, source.Width, source.Height,
                            srcX, srcY, outputRow + x * 4);
                    }
                }
            }
        }, cancellationToken);
    }

    private unsafe void SampleBilinearSIMD(
        byte* source,
        int sourceWidth,
        int sourceHeight,
        Vector256<float> x,
        Vector256<float> y,
        byte* output)
    {
        // Floor coordinates
        var x0 = Vector256.ConvertToInt32(Vector256.Floor(x));
        var y0 = Vector256.ConvertToInt32(Vector256.Floor(y));

        // Fractional parts for interpolation
        var fx = Vector256.Subtract(x, Vector256.Floor(x));
        var fy = Vector256.Subtract(y, Vector256.Floor(y));

        // Process each sample
        for (int i = 0; i < 8; i++)
        {
            var sx = x0.GetElement(i);
            var sy = y0.GetElement(i);

            // Bounds check
            if (sx < 0 || sx >= sourceWidth - 1 ||
                sy < 0 || sy >= sourceHeight - 1)
            {
                // Transparent pixel for out-of-bounds
                output[i * 4] = 0;
                output[i * 4 + 1] = 0;
                output[i * 4 + 2] = 0;
                output[i * 4 + 3] = 0;
                continue;
            }

            // Get four surrounding pixels
            var p00 = source + (sy * sourceWidth + sx) * 4;
            var p10 = p00 + 4;
            var p01 = p00 + sourceWidth * 4;
            var p11 = p01 + 4;

            var fracX = fx.GetElement(i);
            var fracY = fy.GetElement(i);

            // Bilinear interpolation for each channel
            for (int c = 0; c < 4; c++)
            {
                var v00 = p00[c];
                var v10 = p10[c];
                var v01 = p01[c];
                var v11 = p11[c];

                var v0 = v00 + fracX * (v10 - v00);
                var v1 = v01 + fracX * (v11 - v01);
                var v = v0 + fracY * (v1 - v0);

                output[i * 4 + c] = (byte)Math.Round(v);
            }
        }
    }
}
```

### Managing tile caching and CDN distribution

Efficient tile distribution requires sophisticated caching strategies that span from local disk caches through CDN edge
nodes to origin servers. The implementation provides flexible cache key generation supporting multiple tile schemes,
intelligent cache warming based on access patterns, and integration with major CDN providers for global distribution.

Cache invalidation presents particular challenges for geospatial data where updates may affect specific geographic
regions or zoom levels. The system implements hierarchical invalidation allowing efficient purging of affected tiles
while preserving valid cached data, reducing both regeneration costs and cache miss rates during updates.

```csharp
public class TileDistributionManager
{
    private readonly ITileStorage _originStorage;
    private readonly ICDNProvider _cdnProvider;
    private readonly IAnalyticsCollector _analytics;
    private readonly DistributionConfig _config;

    public TileDistributionManager(
        ITileStorage originStorage,
        ICDNProvider cdnProvider,
        IAnalyticsCollector analytics,
        DistributionConfig config)
    {
        _originStorage = originStorage;
        _cdnProvider = cdnProvider;
        _analytics = analytics;
        _config = config;
    }

    public async Task<TileDistributionResult> DistributeTilesAsync(
        TileSet tileSet,
        DistributionOptions options)
    {
        var result = new TileDistributionResult();

        // Generate CDN-optimized file structure
        var cdnStructure = await OptimizeForCDNAsync(tileSet, options);

        // Upload to origin with parallelism control
        await UploadToOriginAsync(cdnStructure, options);

        // Configure CDN caching rules
        await ConfigureCDNCachingAsync(tileSet, options);

        // Warm critical tiles
        if (options.WarmCache)
        {
            await WarmCacheTiersAsync(tileSet, options);
        }

        // Set up analytics tracking
        await ConfigureAnalyticsAsync(tileSet);

        return result;
    }

    private async Task ConfigureCDNCachingAsync(
        TileSet tileSet,
        DistributionOptions options)
    {
        // Configure cache headers based on zoom level
        var cacheRules = new List<CacheRule>();

        // Base maps (z0-z10) - cache for 30 days
        cacheRules.Add(new CacheRule
        {
            PathPattern = $"/{tileSet.Id}/{{z:[0-9]|10}}/*/{{*.png}}",
            CacheControl = "public, max-age=2592000, immutable",
            EdgeTTL = TimeSpan.FromDays(30),
            BrowserTTL = TimeSpan.FromDays(30)
        });

        // Mid-level tiles (z11-z15) - cache for 7 days
        cacheRules.Add(new CacheRule
        {
            PathPattern = $"/{tileSet.Id}/{{z:1[1-5]}}/*/{{*.png}}",
            CacheControl = "public, max-age=604800",
            EdgeTTL = TimeSpan.FromDays(7),
            BrowserTTL = TimeSpan.FromDays(1)
        });

        // Detailed tiles (z16+) - cache for 1 day
        cacheRules.Add(new CacheRule
        {
            PathPattern = $"/{tileSet.Id}/{{z:1[6-9]|2[0-4]}}/*/{{*.png}}",
            CacheControl = "public, max-age=86400",
            EdgeTTL = TimeSpan.FromDays(1),
            BrowserTTL = TimeSpan.FromHours(1),
            StaleWhileRevalidate = TimeSpan.FromHours(6)
        });

        await _cdnProvider.SetCacheRulesAsync(cacheRules);

        // Configure origin shield for popular regions
        if (options.EnableOriginShield)
        {
            await ConfigureOriginShieldAsync(tileSet);
        }
    }

    private async Task WarmCacheTiersAsync(
        TileSet tileSet,
        DistributionOptions options)
    {
        // Analyze access patterns to determine warming strategy
        var accessPatterns = await _analytics.GetAccessPatternsAsync(
            tileSet.Id,
            lookbackDays: 30);

        var warmingStrategy = DetermineWarmingStrategy(
            accessPatterns,
            options.WarmingBudget);

        // Warm edge locations based on strategy
        var edgeLocations = await _cdnProvider.GetEdgeLocationsAsync();

        await Parallel.ForEachAsync(
            edgeLocations,
            new ParallelOptions { MaxDegreeOfParallelism = 10 },
            async (location, ct) =>
            {
                await WarmEdgeLocationAsync(
                    location, tileSet, warmingStrategy, ct);
            });
    }

    private async Task WarmEdgeLocationAsync(
        EdgeLocation location,
        TileSet tileSet,
        WarmingStrategy strategy,
        CancellationToken cancellationToken)
    {
        var tilesToWarm = strategy.GetTilesForLocation(location);
        var warmingBatches = tilesToWarm.Chunk(100); // Batch for efficiency

        foreach (var batch in warmingBatches)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var warmRequests = batch.Select(tile => new WarmingRequest
            {
                Url = BuildTileUrl(tileSet, tile),
                Priority = CalculateWarmingPriority(tile, location, strategy),
                Headers = new Dictionary<string, string>
                {
                    ["X-Warming-Request"] = "true",
                    ["X-Edge-Location"] = location.Code
                }
            }).ToList();

            await _cdnProvider.WarmCacheAsync(location, warmRequests);

            // Respect rate limits
            await Task.Delay(100, cancellationToken);
        }
    }

    // Intelligent cache invalidation
    public async Task InvalidateRegionAsync(
        string tileSetId,
        BoundingBox region,
        InvalidationOptions options)
    {
        var affectedTiles = new List<TileCoordinate>();

        // Calculate affected tiles for each zoom level
        for (int z = options.MinZoom; z <= options.MaxZoom; z++)
        {
            var tileBounds = CalculateTileBounds(z, region);

            for (int x = tileBounds.MinX; x <= tileBounds.MaxX; x++)
            {
                for (int y = tileBounds.MinY; y <= tileBounds.MaxY; y++)
                {
                    affectedTiles.Add(new TileCoordinate(z, x, y));
                }
            }
        }

        // Group by invalidation strategy
        if (options.Strategy == InvalidationStrategy.Surgical)
        {
            // Invalidate only specific tiles
            await InvalidateTilesAsync(tileSetId, affectedTiles);
        }
        else if (options.Strategy == InvalidationStrategy.Hierarchical)
        {
            // Invalidate by path prefix for efficiency
            var pathPrefixes = GetHierarchicalPrefixes(affectedTiles);
            await InvalidateByPrefixAsync(tileSetId, pathPrefixes);
        }

        // Tag cache entries for soft invalidation if supported
        if (_cdnProvider.SupportsSoftInvalidation)
        {
            await TagForRevalidationAsync(tileSetId, affectedTiles);
        }
    }
}

// Smart cache key generation with versioning
public class TileCacheKeyGenerator
{
    private readonly HashAlgorithm _hasher;
    private readonly CacheKeyConfig _config;

    public TileCacheKeyGenerator(CacheKeyConfig config)
    {
        _config = config;
        _hasher = SHA256.Create();
    }

    public string GenerateKey(TileRequest request)
    {
        var components = new List<string>
        {
            request.TileSet,
            request.Z.ToString(),
            request.X.ToString(),
            request.Y.ToString()
        };

        // Add optional components
        if (_config.IncludeFormat)
        {
            components.Add(request.Format ?? "png");
        }

        if (_config.IncludeScale && request.Scale != 1.0)
        {
            components.Add($"@{request.Scale}x");
        }

        if (_config.IncludeStyle && !string.IsNullOrEmpty(request.Style))
        {
            components.Add(request.Style);
        }

        // Add version for cache busting
        if (_config.VersioningStrategy == VersioningStrategy.Global)
        {
            components.Add($"v{_config.GlobalVersion}");
        }
        else if (_config.VersioningStrategy == VersioningStrategy.PerTileSet)
        {
            var version = GetTileSetVersion(request.TileSet);
            components.Add($"v{version}");
        }
        else if (_config.VersioningStrategy == VersioningStrategy.ContentHash)
        {
            var contentHash = ComputeContentHash(request);
            components.Add(contentHash.Substring(0, 8));
        }

        // Build cache key
        var key = string.Join("/", components);

        // Add query parameters if configured
        if (_config.UseQueryParameters && request.QueryParameters?.Any() == true)
        {
            var queryString = BuildQueryString(request.QueryParameters);
            key += "?" + queryString;
        }

        return key;
    }

    private string ComputeContentHash(TileRequest request)
    {
        // Hash relevant request parameters
        var data = Encoding.UTF8.GetBytes(
            $"{request.TileSet}:{request.Z}:{request.X}:{request.Y}:" +
            $"{request.Format}:{request.Style}:{request.Scale}");

        var hash = _hasher.ComputeHash(data);
        return Convert.ToBase64String(hash)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
```

This completes Chapter 16 on Geospatial Image Processing. The chapter provides comprehensive coverage of handling large
geospatial datasets in .NET 9.0, from BigTIFF support and Cloud-Optimized GeoTIFF implementation through coordinate
system transformations to efficient tile generation and distribution. The code examples demonstrate production-ready
patterns for building high-performance geospatial applications that can scale from desktop tools to global web mapping
services.
