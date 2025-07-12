# Chapter 12: Asynchronous Processing Patterns

The transformation of graphics processing from synchronous, blocking operations to sophisticated asynchronous pipelines
represents a fundamental shift in how we architect high-performance systems. In an era where users expect instant
responsiveness—where a 100-millisecond delay feels like an eternity—asynchronous patterns have evolved from optional
optimizations to essential architectural foundations. Modern graphics applications must juggle multiple concerns
simultaneously: rendering UI at 60+ FPS, processing multi-gigabyte images, streaming data from cloud services, and
responding to user input without a hint of lag. The async/await revolution in C# has matured into a rich ecosystem of
patterns that, when properly applied, transform seemingly impossible performance requirements into elegant, maintainable
solutions. This chapter explores how .NET 9.0's enhanced asynchronous capabilities enable graphics applications to
achieve unprecedented responsiveness while managing complex resource lifecycles and maintaining code clarity.

## 12.1 Task-Based Asynchronous Patterns

The Task-based Asynchronous Pattern (TAP) has become the lingua franca of asynchronous programming in .NET, but its
application to graphics processing demands careful consideration of thread affinity, resource ownership, and performance
characteristics. Unlike typical I/O-bound operations, graphics processing straddles the boundary between CPU-intensive
computation and I/O operations, requiring nuanced approaches that leverage both Task and ValueTask appropriately.

### Fundamental async patterns for graphics operations

The foundation of asynchronous graphics processing rests on understanding when and how to apply async patterns. Not
every operation benefits from asynchronization—the overhead of task scheduling can exceed the operation cost for simple
transformations. The key lies in identifying operations that either block on I/O or consume significant CPU time:

```csharp
public class AsyncImageProcessor
{
    private readonly IMemoryPool<byte> _memoryPool;
    private readonly SemaphoreSlim _processingThrottle;
    private readonly int _maxConcurrency;

    public AsyncImageProcessor(int maxConcurrency = 4)
    {
        _maxConcurrency = maxConcurrency;
        _processingThrottle = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        _memoryPool = MemoryPool<byte>.Shared;
    }

    // Async loading with memory pooling
    public async ValueTask<Image<Rgba32>> LoadImageAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        // Use ValueTask for hot path optimization
        if (ImageCache.TryGetCached(path, out var cached))
        {
            return cached;
        }

        await _processingThrottle.WaitAsync(cancellationToken);
        try
        {
            // Async file I/O with pooled buffers
            await using var fileStream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            // Rent buffer from pool for streaming
            using var memoryOwner = _memoryPool.Rent(81920); // 80KB buffer
            var buffer = memoryOwner.Memory;

            // Stream into memory asynchronously
            using var memoryStream = new MemoryStream();
            int bytesRead;

            while ((bytesRead = await fileStream.ReadAsync(
                buffer, cancellationToken)) > 0)
            {
                await memoryStream.WriteAsync(
                    buffer.Slice(0, bytesRead), cancellationToken);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);

            // Decode on thread pool to avoid blocking
            return await Task.Run(() =>
                Image.Load<Rgba32>(memoryStream), cancellationToken);
        }
        finally
        {
            _processingThrottle.Release();
        }
    }

    // CPU-bound operations with configurable parallelism
    public async Task<Image<Rgba32>> ApplyFiltersAsync(
        Image<Rgba32> source,
        IEnumerable<IImageFilter> filters,
        CancellationToken cancellationToken = default)
    {
        // Clone for non-destructive processing
        var working = source.Clone();

        foreach (var filter in filters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (filter.IsComputeIntensive)
            {
                // Offload heavy computation to thread pool
                await Task.Run(() =>
                    filter.Apply(working), cancellationToken);
            }
            else
            {
                // Light operations can run synchronously
                filter.Apply(working);
            }
        }

        return working;
    }
}
```

### ValueTask optimization for hot paths

Graphics applications often have "hot paths"—code executed thousands of times per second during rendering or real-time
processing. ValueTask provides crucial optimizations for these scenarios by avoiding heap allocations when operations
complete synchronously:

```csharp
public class OptimizedRenderPipeline
{
    private readonly Channel<RenderCommand> _commandQueue;
    private readonly Dictionary<int, RenderTarget> _renderTargets;
    private readonly object _cacheLock = new();

    // ValueTask for synchronous completion common case
    public ValueTask<RenderTarget> GetRenderTargetAsync(
        int targetId,
        CancellationToken cancellationToken = default)
    {
        lock (_cacheLock)
        {
            if (_renderTargets.TryGetValue(targetId, out var target))
            {
                // Synchronous completion - no allocation
                return new ValueTask<RenderTarget>(target);
            }
        }

        // Async fallback for cache miss
        return new ValueTask<RenderTarget>(
            LoadRenderTargetAsync(targetId, cancellationToken));
    }

    private async Task<RenderTarget> LoadRenderTargetAsync(
        int targetId,
        CancellationToken cancellationToken)
    {
        var target = await RenderTarget.CreateAsync(targetId, cancellationToken);

        lock (_cacheLock)
        {
            _renderTargets[targetId] = target;
        }

        return target;
    }

    // Async enumerable for streaming results
    public async IAsyncEnumerable<PixelRegion> ProcessRegionsAsync(
        Image<Rgba32> image,
        int tileSize = 256,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var bounds = image.Bounds();

        for (int y = 0; y < bounds.Height; y += tileSize)
        {
            for (int x = 0; x < bounds.Width; x += tileSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var region = new Rectangle(
                    x, y,
                    Math.Min(tileSize, bounds.Width - x),
                    Math.Min(tileSize, bounds.Height - y));

                // Process region asynchronously
                var pixelRegion = await Task.Run(() =>
                {
                    var buffer = new PixelRegion();
                    image.ProcessPixelRows(accessor =>
                    {
                        for (int py = region.Y; py < region.Bottom; py++)
                        {
                            var row = accessor.GetRowSpan(py);
                            buffer.AddRow(row.Slice(region.X, region.Width));
                        }
                    });
                    return buffer;
                }, cancellationToken);

                yield return pixelRegion;
            }
        }
    }
}
```

### Async coordination patterns

Complex graphics operations often require coordinating multiple asynchronous operations. Producer-consumer patterns,
parallel pipelines, and fork-join scenarios demand sophisticated coordination:

```csharp
public class AsyncCoordinationPatterns
{
    // Producer-consumer with backpressure
    public class AsyncImagePipeline
    {
        private readonly Channel<ProcessingItem> _channel;
        private readonly int _boundedCapacity;

        public AsyncImagePipeline(int boundedCapacity = 100)
        {
            _boundedCapacity = boundedCapacity;

            var options = new BoundedChannelOptions(boundedCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = false
            };

            _channel = Channel.CreateBounded<ProcessingItem>(options);
        }

        // Producer with backpressure
        public async Task ProduceAsync(
            IEnumerable<string> imagePaths,
            CancellationToken cancellationToken = default)
        {
            var writer = _channel.Writer;

            try
            {
                foreach (var path in imagePaths)
                {
                    var item = new ProcessingItem
                    {
                        Id = Guid.NewGuid(),
                        SourcePath = path,
                        Timestamp = DateTime.UtcNow
                    };

                    // Wait if channel is full (backpressure)
                    await writer.WriteAsync(item, cancellationToken);
                }
            }
            finally
            {
                writer.TryComplete();
            }
        }

        // Multiple consumers processing in parallel
        public async Task ConsumeAsync(
            int consumerCount,
            Func<ProcessingItem, Task> processor,
            CancellationToken cancellationToken = default)
        {
            var reader = _channel.Reader;

            var consumers = Enumerable.Range(0, consumerCount)
                .Select(async consumerId =>
                {
                    await foreach (var item in reader.ReadAllAsync(cancellationToken))
                    {
                        try
                        {
                            await processor(item);
                        }
                        catch (Exception ex)
                        {
                            // Log and continue processing
                            await HandleProcessingError(item, ex);
                        }
                    }
                })
                .ToArray();

            await Task.WhenAll(consumers);
        }
    }

    // Fork-join pattern for parallel processing
    public async Task<CompositeResult> ForkJoinProcessingAsync(
        Image<Rgba32> source,
        CancellationToken cancellationToken = default)
    {
        // Fork into parallel tasks
        var tasks = new[]
        {
            Task.Run(() => ExtractHistogram(source), cancellationToken),
            Task.Run(() => DetectEdges(source), cancellationToken),
            Task.Run(() => AnalyzeColorSpace(source), cancellationToken),
            Task.Run(() => ComputeMetrics(source), cancellationToken)
        };

        // Join results
        await Task.WhenAll(tasks);

        return new CompositeResult
        {
            Histogram = await tasks[0],
            EdgeMap = await tasks[1],
            ColorAnalysis = await tasks[2],
            Metrics = await tasks[3]
        };
    }

    // Async semaphore for resource limiting
    public class AsyncResourcePool<T> where T : IDisposable
    {
        private readonly Func<Task<T>> _factory;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentBag<T> _pool;

        public AsyncResourcePool(int maxResources, Func<Task<T>> factory)
        {
            _factory = factory;
            _semaphore = new SemaphoreSlim(maxResources, maxResources);
            _pool = new ConcurrentBag<T>();
        }

        public async Task<ResourceLease<T>> AcquireAsync(
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            if (!_pool.TryTake(out var resource))
            {
                resource = await _factory();
            }

            return new ResourceLease<T>(resource, this);
        }

        private void Release(T resource)
        {
            _pool.Add(resource);
            _semaphore.Release();
        }

        public readonly struct ResourceLease<TResource> : IAsyncDisposable
            where TResource : IDisposable
        {
            private readonly TResource _resource;
            private readonly AsyncResourcePool<TResource> _pool;

            public ResourceLease(TResource resource, AsyncResourcePool<TResource> pool)
            {
                _resource = resource;
                _pool = pool;
            }

            public TResource Resource => _resource;

            public ValueTask DisposeAsync()
            {
                _pool.Release(_resource);
                return ValueTask.CompletedTask;
            }
        }
    }
}
```

## 12.2 Pipeline Parallelism

Pipeline parallelism transforms sequential image processing into concurrent workflows where different stages execute
simultaneously on different data. This pattern excels when processing multiple images or video frames, enabling
continuous throughput rather than start-stop batch processing. The key insight is treating computation as a flowing
stream rather than discrete operations.

### Dataflow-based processing architectures

The TPL Dataflow library provides the foundation for building sophisticated processing pipelines that automatically
handle concurrency, buffering, and error propagation:

```csharp
public class DataflowImagePipeline
{
    private readonly TransformBlock<string, RawImageData> _loadBlock;
    private readonly TransformBlock<RawImageData, DecodedImage> _decodeBlock;
    private readonly TransformBlock<DecodedImage, ProcessedImage> _processBlock;
    private readonly ActionBlock<ProcessedImage> _saveBlock;

    public DataflowImagePipeline(PipelineOptions options)
    {
        // Configure execution options for each stage
        var loadOptions = new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = options.LoadBufferSize,
            MaxDegreeOfParallelism = options.MaxLoaders,
            CancellationToken = options.CancellationToken
        };

        var processOptions = new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = options.ProcessBufferSize,
            MaxDegreeOfParallelism = options.MaxProcessors,
            CancellationToken = options.CancellationToken
        };

        // Stage 1: Async file loading
        _loadBlock = new TransformBlock<string, RawImageData>(
            async path =>
            {
                var data = new RawImageData { Path = path };

                await using var stream = File.OpenRead(path);
                data.Bytes = new byte[stream.Length];
                await stream.ReadAsync(data.Bytes.AsMemory());

                data.LoadedAt = DateTime.UtcNow;
                return data;
            },
            loadOptions);

        // Stage 2: Parallel decoding
        _decodeBlock = new TransformBlock<RawImageData, DecodedImage>(
            rawData =>
            {
                using var stream = new MemoryStream(rawData.Bytes);
                var image = Image.Load<Rgba32>(stream);

                return new DecodedImage
                {
                    Path = rawData.Path,
                    Image = image,
                    LoadedAt = rawData.LoadedAt,
                    DecodedAt = DateTime.UtcNow
                };
            },
            processOptions);

        // Stage 3: Image processing with dynamic parallelism
        _processBlock = new TransformBlock<DecodedImage, ProcessedImage>(
            async decoded =>
            {
                var processor = new AdaptiveProcessor();

                // Dynamically adjust parallelism based on image size
                var parallelism = CalculateOptimalParallelism(decoded.Image);

                var processed = await processor.ProcessAsync(
                    decoded.Image,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = parallelism
                    });

                return new ProcessedImage
                {
                    Path = decoded.Path,
                    Original = decoded.Image,
                    Processed = processed,
                    ProcessingTime = DateTime.UtcNow - decoded.DecodedAt
                };
            },
            processOptions);

        // Stage 4: Async saving with format selection
        _saveBlock = new ActionBlock<ProcessedImage>(
            async processed =>
            {
                var outputPath = GenerateOutputPath(processed.Path);
                var format = SelectOptimalFormat(processed.Processed);

                await using var output = File.Create(outputPath);
                await processed.Processed.SaveAsync(output, format);

                // Cleanup
                processed.Original?.Dispose();
                processed.Processed?.Dispose();

                await LogCompletionAsync(processed);
            },
            new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = options.SaveBufferSize,
                MaxDegreeOfParallelism = options.MaxSavers,
                CancellationToken = options.CancellationToken
            });

        // Link pipeline stages with propagation
        var linkOptions = new DataflowLinkOptions
        {
            PropagateCompletion = true
        };

        _loadBlock.LinkTo(_decodeBlock, linkOptions);
        _decodeBlock.LinkTo(_processBlock, linkOptions);
        _processBlock.LinkTo(_saveBlock, linkOptions);
    }

    public async Task ProcessBatchAsync(IEnumerable<string> imagePaths)
    {
        // Post all paths to the pipeline
        foreach (var path in imagePaths)
        {
            await _loadBlock.SendAsync(path);
        }

        // Signal completion
        _loadBlock.Complete();

        // Wait for pipeline to drain
        await _saveBlock.Completion;
    }

    // Advanced pipeline with branching and merging
    public class BranchingPipeline
    {
        private readonly BroadcastBlock<DecodedImage> _broadcast;
        private readonly TransformBlock<DecodedImage, Thumbnail> _thumbnailBranch;
        private readonly TransformBlock<DecodedImage, Analysis> _analysisBranch;
        private readonly TransformBlock<DecodedImage, Processed> _processingBranch;
        private readonly JoinBlock<Thumbnail, Analysis, Processed> _join;

        public BranchingPipeline()
        {
            // Broadcast to multiple branches
            _broadcast = new BroadcastBlock<DecodedImage>(
                img => img.Clone(),
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = 10
                });

            // Parallel branches with different processing
            _thumbnailBranch = new TransformBlock<DecodedImage, Thumbnail>(
                async img => await GenerateThumbnailAsync(img));

            _analysisBranch = new TransformBlock<DecodedImage, Analysis>(
                async img => await AnalyzeImageAsync(img));

            _processingBranch = new TransformBlock<DecodedImage, Processed>(
                async img => await ProcessImageAsync(img));

            // Join results from all branches
            _join = new JoinBlock<Thumbnail, Analysis, Processed>();

            // Link the pipeline
            _broadcast.LinkTo(_thumbnailBranch);
            _broadcast.LinkTo(_analysisBranch);
            _broadcast.LinkTo(_processingBranch);

            _thumbnailBranch.LinkTo(_join.Target1);
            _analysisBranch.LinkTo(_join.Target2);
            _processingBranch.LinkTo(_join.Target3);
        }
    }
}
```

### Async streams and IAsyncEnumerable patterns

IAsyncEnumerable enables elegant streaming patterns that process data as it arrives rather than waiting for complete
batches:

```csharp
public class StreamingImageProcessor
{
    // Streaming file discovery with async enumerable
    public async IAsyncEnumerable<FileInfo> DiscoverImagesAsync(
        string rootPath,
        SearchOption searchOption = SearchOption.AllDirectories,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var extensions = new[] { ".jpg", ".png", ".webp", ".tiff", ".bmp" };
        var directories = new Queue<string>();
        directories.Enqueue(rootPath);

        while (directories.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentDir = directories.Dequeue();

            // Yield files as we find them
            foreach (var file in Directory.EnumerateFiles(currentDir))
            {
                var info = new FileInfo(file);
                if (extensions.Contains(info.Extension.ToLowerInvariant()))
                {
                    yield return info;
                }
            }

            // Add subdirectories for recursive search
            if (searchOption == SearchOption.AllDirectories)
            {
                foreach (var subDir in Directory.EnumerateDirectories(currentDir))
                {
                    directories.Enqueue(subDir);
                }
            }

            // Yield control periodically
            await Task.Yield();
        }
    }

    // Streaming processing with transformation
    public async IAsyncEnumerable<ProcessingResult> ProcessStreamAsync(
        IAsyncEnumerable<FileInfo> files,
        ProcessingOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var semaphore = new SemaphoreSlim(options.MaxConcurrency);

        await foreach (var file in files.WithCancellation(cancellationToken))
        {
            await semaphore.WaitAsync(cancellationToken);

            // Process without blocking the enumeration
            var resultTask = ProcessFileAsync(file, options, cancellationToken)
                .ContinueWith(t =>
                {
                    semaphore.Release();
                    return t.Result;
                }, TaskContinuationOptions.ExecuteSynchronously);

            yield return await resultTask;
        }
    }

    // Batch processing with streaming
    public async IAsyncEnumerable<Batch<T>> BatchAsync<T>(
        IAsyncEnumerable<T> source,
        int batchSize,
        TimeSpan maxDelay,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var batch = new List<T>(batchSize);
        var batchTimer = new CancellationTokenSource();
        var timerTask = Task.Delay(maxDelay, batchTimer.Token);

        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            batch.Add(item);

            // Yield full batch immediately
            if (batch.Count >= batchSize)
            {
                yield return new Batch<T>(batch.ToArray(), BatchReason.Size);
                batch.Clear();

                // Reset timer
                batchTimer.Cancel();
                batchTimer = new CancellationTokenSource();
                timerTask = Task.Delay(maxDelay, batchTimer.Token);
            }
            // Check if time limit reached
            else if (timerTask.IsCompleted)
            {
                yield return new Batch<T>(batch.ToArray(), BatchReason.Timeout);
                batch.Clear();

                // Reset timer
                batchTimer = new CancellationTokenSource();
                timerTask = Task.Delay(maxDelay, batchTimer.Token);
            }
        }

        // Yield remaining items
        if (batch.Count > 0)
        {
            yield return new Batch<T>(batch.ToArray(), BatchReason.EndOfStream);
        }

        batchTimer.Cancel();
    }
}
```

### Backpressure and flow control

Managing backpressure prevents pipeline stages from overwhelming downstream consumers. Sophisticated flow control
ensures smooth operation under varying loads:

```csharp
public class BackpressureAwarePipeline
{
    private readonly Channel<WorkItem> _inputChannel;
    private readonly IProgress<PipelineMetrics> _metricsReporter;

    public BackpressureAwarePipeline(BackpressureOptions options)
    {
        // Create channel with capacity and drop policy
        var channelOptions = new BoundedChannelOptions(options.ChannelCapacity)
        {
            FullMode = options.FullMode switch
            {
                OverflowStrategy.DropOldest => BoundedChannelFullMode.DropOldest,
                OverflowStrategy.DropNewest => BoundedChannelFullMode.DropNewest,
                OverflowStrategy.Wait => BoundedChannelFullMode.Wait,
                _ => BoundedChannelFullMode.Wait
            },
            SingleWriter = options.SingleProducer,
            SingleReader = false
        };

        _inputChannel = Channel.CreateBounded<WorkItem>(channelOptions);
    }

    // Adaptive processing with dynamic concurrency
    public async Task RunAdaptivePipelineAsync(
        CancellationToken cancellationToken = default)
    {
        var metrics = new PipelineMetrics();
        var adaptiveThrottle = new AdaptiveThrottle(
            initialConcurrency: 4,
            minConcurrency: 1,
            maxConcurrency: Environment.ProcessorCount * 2);

        var processingTasks = new List<Task>();

        await foreach (var item in _inputChannel.Reader.ReadAllAsync(cancellationToken))
        {
            // Measure queue depth and adjust concurrency
            metrics.QueueDepth = _inputChannel.Reader.Count;
            metrics.ActiveTasks = processingTasks.Count(t => !t.IsCompleted);

            // Adjust concurrency based on metrics
            var targetConcurrency = adaptiveThrottle.CalculateOptimalConcurrency(metrics);

            // Wait if we're at capacity
            while (processingTasks.Count >= targetConcurrency)
            {
                var completed = await Task.WhenAny(processingTasks);
                processingTasks.Remove(completed);

                // Handle any errors
                if (completed.IsFaulted)
                {
                    await HandleProcessingError(completed.Exception);
                }
            }

            // Start new processing task
            var task = ProcessItemWithMetricsAsync(item, metrics, cancellationToken);
            processingTasks.Add(task);

            // Report metrics
            _metricsReporter?.Report(metrics);
        }

        // Wait for remaining tasks
        await Task.WhenAll(processingTasks);
    }

    private async Task ProcessItemWithMetricsAsync(
        WorkItem item,
        PipelineMetrics metrics,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await ProcessItemAsync(item, cancellationToken);

            // Update success metrics
            Interlocked.Increment(ref metrics.ProcessedCount);
            metrics.AverageProcessingTime =
                (metrics.AverageProcessingTime * (metrics.ProcessedCount - 1) +
                 stopwatch.ElapsedMilliseconds) / metrics.ProcessedCount;
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref metrics.ErrorCount);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    // Priority-based processing
    public class PriorityPipeline<T>
    {
        private readonly PriorityChannel<T> _priorityChannel;

        public async Task ProcessWithPriorityAsync(
            Func<T, Priority> prioritySelector,
            Func<T, Task> processor,
            CancellationToken cancellationToken = default)
        {
            var reader = _priorityChannel.Reader;

            await Parallel.ForEachAsync(
                reader.ReadAllAsync(cancellationToken),
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = cancellationToken
                },
                async (item, ct) =>
                {
                    await processor(item);
                });
        }
    }
}

// Custom priority channel implementation
public class PriorityChannel<T>
{
    private readonly SortedDictionary<Priority, Queue<T>> _queues;
    private readonly SemaphoreSlim _semaphore;
    private readonly object _lock = new();

    public ChannelWriter<T> Writer => new PriorityChannelWriter(this);
    public ChannelReader<T> Reader => new PriorityChannelReader(this);

    private class PriorityChannelReader : ChannelReader<T>
    {
        private readonly PriorityChannel<T> _channel;

        public override async ValueTask<T> ReadAsync(
            CancellationToken cancellationToken = default)
        {
            await _channel._semaphore.WaitAsync(cancellationToken);

            lock (_channel._lock)
            {
                // Get highest priority item
                foreach (var (priority, queue) in _channel._queues.Reverse())
                {
                    if (queue.Count > 0)
                    {
                        return queue.Dequeue();
                    }
                }

                throw new InvalidOperationException("No items available");
            }
        }
    }
}
```

## 12.3 Resource Management in Async Context

Asynchronous resource management presents unique challenges beyond traditional using blocks. Graphics resources—GPU
buffers, image memory, file handles—must be carefully managed across asynchronous boundaries while preventing resource
exhaustion and ensuring proper cleanup even when operations are cancelled or fail.

### IAsyncDisposable patterns for graphics resources

The IAsyncDisposable interface, introduced in C# 8.0, enables proper asynchronous cleanup of resources that require
async operations for disposal:

```csharp
public class AsyncGraphicsResource : IAsyncDisposable
{
    private readonly GpuBuffer _gpuBuffer;
    private readonly SemaphoreSlim _disposalLock;
    private readonly List<IAsyncDisposable> _childResources;
    private bool _disposed;

    public AsyncGraphicsResource()
    {
        _disposalLock = new SemaphoreSlim(1, 1);
        _childResources = new List<IAsyncDisposable>();
        _gpuBuffer = GpuBuffer.Allocate(BufferSize);
    }

    // Async disposal with proper ordering
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await _disposalLock.WaitAsync();
        try
        {
            if (_disposed)
                return;

            // Dispose child resources first
            foreach (var child in _childResources)
            {
                await child.DisposeAsync();
            }

            // Flush any pending operations
            await FlushPendingOperationsAsync();

            // Release GPU resources
            await _gpuBuffer.ReleaseAsync();

            // Dispose synchronous resources
            _disposalLock?.Dispose();

            _disposed = true;
        }
        finally
        {
            _disposalLock?.Release();
        }
    }

    // Pattern for async using in complex scenarios
    public async Task<ProcessingResult> ProcessWithResourcesAsync()
    {
        await using var texture = await GpuTexture.CreateAsync();
        await using var shader = await ComputeShader.LoadAsync("process.hlsl");
        await using var output = await RenderTarget.CreateAsync();

        // Resources automatically disposed in reverse order
        return await RenderAsync(texture, shader, output);
    }
}

// Advanced resource pooling with async lifecycle
public class AsyncResourcePool<T> : IAsyncDisposable
    where T : class, IAsyncDisposable
{
    private readonly Func<Task<T>> _factory;
    private readonly Func<T, ValueTask<bool>> _validator;
    private readonly ConcurrentBag<T> _available;
    private readonly HashSet<T> _all;
    private readonly SemaphoreSlim _semaphore;
    private readonly Timer _cleanupTimer;
    private readonly int _maxPoolSize;
    private readonly TimeSpan _idleTimeout;
    private bool _disposed;

    public AsyncResourcePool(
        PoolConfiguration<T> config)
    {
        _factory = config.Factory;
        _validator = config.Validator ?? (_ => new ValueTask<bool>(true));
        _maxPoolSize = config.MaxPoolSize;
        _idleTimeout = config.IdleTimeout;

        _available = new ConcurrentBag<T>();
        _all = new HashSet<T>();
        _semaphore = new SemaphoreSlim(_maxPoolSize, _maxPoolSize);

        // Periodic cleanup of idle resources
        _cleanupTimer = new Timer(
            async _ => await CleanupIdleResourcesAsync(),
            null,
            _idleTimeout,
            _idleTimeout);
    }

    public async Task<PooledResource<T>> AcquireAsync(
        CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AsyncResourcePool<T>));

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            T resource = null;

            // Try to get existing resource
            while (_available.TryTake(out resource))
            {
                if (await _validator(resource))
                {
                    break;
                }

                // Invalid resource, dispose it
                await DisposeResourceAsync(resource);
                resource = null;
            }

            // Create new resource if needed
            if (resource == null)
            {
                resource = await _factory();
                lock (_all)
                {
                    _all.Add(resource);
                }
            }

            return new PooledResource<T>(resource, this);
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }

    private async ValueTask ReturnAsync(T resource)
    {
        if (resource == null || _disposed)
        {
            _semaphore.Release();
            return;
        }

        if (await _validator(resource))
        {
            _available.Add(resource);
        }
        else
        {
            await DisposeResourceAsync(resource);
        }

        _semaphore.Release();
    }

    private async Task CleanupIdleResourcesAsync()
    {
        if (_disposed)
            return;

        var toDispose = new List<T>();
        var temp = new List<T>();

        // Check all available resources
        while (_available.TryTake(out var resource))
        {
            if (ShouldDisposeIdle(resource))
            {
                toDispose.Add(resource);
            }
            else
            {
                temp.Add(resource);
            }
        }

        // Return non-disposed resources
        foreach (var resource in temp)
        {
            _available.Add(resource);
        }

        // Dispose idle resources
        foreach (var resource in toDispose)
        {
            await DisposeResourceAsync(resource);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        await _cleanupTimer.DisposeAsync();

        // Dispose all resources
        lock (_all)
        {
            var disposeTasks = _all.Select(DisposeResourceAsync);
            await Task.WhenAll(disposeTasks);
            _all.Clear();
        }

        _semaphore?.Dispose();
    }

    // Pooled resource wrapper
    public readonly struct PooledResource<TResource> : IAsyncDisposable
        where TResource : class, IAsyncDisposable
    {
        private readonly TResource _resource;
        private readonly AsyncResourcePool<TResource> _pool;

        public PooledResource(TResource resource, AsyncResourcePool<TResource> pool)
        {
            _resource = resource;
            _pool = pool;
        }

        public TResource Resource => _resource;

        public ValueTask DisposeAsync()
        {
            return _pool.ReturnAsync(_resource);
        }
    }
}
```

### Memory pressure and async operations

Graphics operations often create significant memory pressure. Managing this in async contexts requires careful
coordination between the garbage collector and async operations:

```csharp
public class MemoryAwareAsyncProcessor
{
    private readonly MemoryPressureMonitor _memoryMonitor;
    private readonly AdaptiveThrottle _adaptiveThrottle;
    private long _allocatedBytes;

    public async Task<ProcessedImage> ProcessLargeImageAsync(
        string imagePath,
        ProcessingOptions options,
        CancellationToken cancellationToken = default)
    {
        // Check memory before starting
        await _memoryMonitor.WaitForMemoryAsync(
            options.EstimatedMemoryUsage,
            cancellationToken);

        // Track allocation
        GC.AddMemoryPressure(options.EstimatedMemoryUsage);
        Interlocked.Add(ref _allocatedBytes, options.EstimatedMemoryUsage);

        try
        {
            // Use pooled buffers for large allocations
            using var bufferLease = MemoryPool<byte>.Shared.Rent(
                options.BufferSize);

            var image = await LoadWithMemoryConstraintsAsync(
                imagePath,
                bufferLease.Memory,
                cancellationToken);

            // Process with memory-aware parallelism
            var parallelism = _adaptiveThrottle.CalculateParallelism(
                new ThrottleContext
                {
                    AvailableMemory = _memoryMonitor.AvailableMemory,
                    ImageSize = image.Width * image.Height * 4,
                    CurrentLoad = _allocatedBytes
                });

            return await ProcessWithConstraintsAsync(
                image,
                parallelism,
                cancellationToken);
        }
        finally
        {
            // Release memory pressure
            GC.RemoveMemoryPressure(options.EstimatedMemoryUsage);
            Interlocked.Add(ref _allocatedBytes, -options.EstimatedMemoryUsage);

            // Force collection if under pressure
            if (_memoryMonitor.IsUnderPressure)
            {
                await Task.Run(() =>
                {
                    GC.Collect(2, GCCollectionMode.Aggressive, blocking: true);
                    GC.WaitForPendingFinalizers();
                    GC.Collect(2, GCCollectionMode.Aggressive, blocking: true);
                });
            }
        }
    }

    // Memory-aware batch processing
    public async IAsyncEnumerable<ProcessingResult> ProcessBatchWithMemoryLimitsAsync(
        IAsyncEnumerable<string> imagePaths,
        MemoryConstraints constraints,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var currentBatchMemory = 0L;
        var batch = new List<Task<ProcessingResult>>();

        await foreach (var path in imagePaths.WithCancellation(cancellationToken))
        {
            var estimatedSize = await EstimateMemoryUsageAsync(path);

            // Process batch if adding this image would exceed limit
            if (currentBatchMemory + estimatedSize > constraints.MaxBatchMemory
                && batch.Count > 0)
            {
                await foreach (var result in ProcessCurrentBatch(batch))
                {
                    yield return result;
                }

                batch.Clear();
                currentBatchMemory = 0;

                // Allow GC to run
                await Task.Yield();
            }

            // Add to batch
            var task = ProcessImageAsync(path, cancellationToken);
            batch.Add(task);
            currentBatchMemory += estimatedSize;
        }

        // Process remaining
        if (batch.Count > 0)
        {
            await foreach (var result in ProcessCurrentBatch(batch))
            {
                yield return result;
            }
        }
    }
}

// Memory pressure monitor
public class MemoryPressureMonitor
{
    private readonly Timer _monitorTimer;
    private readonly long _lowMemoryThreshold;
    private readonly long _criticalMemoryThreshold;
    private MemoryStatus _currentStatus;

    public async Task WaitForMemoryAsync(
        long requiredBytes,
        CancellationToken cancellationToken = default)
    {
        var spinWait = new SpinWait();
        var backoffMs = 100;

        while (AvailableMemory < requiredBytes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_currentStatus == MemoryStatus.Critical)
            {
                // Force aggressive GC
                await Task.Run(() =>
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    GC.WaitForPendingFinalizers();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                });

                // Exponential backoff
                await Task.Delay(backoffMs, cancellationToken);
                backoffMs = Math.Min(backoffMs * 2, 5000);
            }
            else
            {
                spinWait.SpinOnce();

                if (spinWait.NextSpinWillYield)
                {
                    await Task.Yield();
                }
            }
        }
    }
}
```

## 12.4 Cancellation and Progress Reporting

Cancellation and progress reporting transform opaque long-running operations into responsive, user-friendly experiences.
Modern async patterns in .NET provide sophisticated mechanisms for cooperative cancellation and granular progress
tracking that maintain UI responsiveness while processing gigapixel images or video streams.

### Cooperative cancellation patterns

Effective cancellation requires careful propagation through all async layers while ensuring resources are properly
cleaned up:

```csharp
public class CancellationAwareProcessor
{
    // Hierarchical cancellation with linked tokens
    public async Task ProcessWithTimeoutAsync(
        ProcessingRequest request,
        TimeSpan timeout,
        CancellationToken userCancellation = default)
    {
        // Create timeout cancellation
        using var timeoutCts = new CancellationTokenSource(timeout);

        // Link user and timeout cancellation
        using var linkedCts = CancellationTokenSource
            .CreateLinkedTokenSource(userCancellation, timeoutCts.Token);

        try
        {
            await ProcessInternalAsync(request, linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"Processing exceeded timeout of {timeout.TotalSeconds}s");
        }
    }

    // Cancellation with cleanup
    private async Task ProcessInternalAsync(
        ProcessingRequest request,
        CancellationToken cancellationToken)
    {
        var cleanupActions = new Stack<Func<Task>>();

        try
        {
            // Allocate GPU resources
            var gpuContext = await AllocateGpuContextAsync(cancellationToken);
            cleanupActions.Push(async () => await gpuContext.DisposeAsync());

            // Start background monitoring
            var monitorTask = MonitorProgressAsync(cancellationToken);
            cleanupActions.Push(async () => await monitorTask);

            // Process with cancellation checks
            await foreach (var chunk in LoadChunksAsync(request, cancellationToken))
            {
                // Check cancellation before expensive operation
                cancellationToken.ThrowIfCancellationRequested();

                await ProcessChunkAsync(chunk, gpuContext, cancellationToken);
            }
        }
        finally
        {
            // Execute cleanup in reverse order
            while (cleanupActions.Count > 0)
            {
                var cleanup = cleanupActions.Pop();
                try
                {
                    await cleanup();
                }
                catch (Exception ex)
                {
                    // Log but don't throw from cleanup
                    LogCleanupError(ex);
                }
            }
        }
    }

    // Graceful cancellation with savepoints
    public async Task<ProcessingResult> ProcessWithSavepointsAsync(
        LargeDataset dataset,
        CancellationToken cancellationToken = default)
    {
        var checkpoint = await LoadCheckpointAsync() ?? new ProcessingCheckpoint();
        var result = new ProcessingResult { StartedFrom = checkpoint };

        try
        {
            await foreach (var batch in dataset.GetBatchesAsync(
                startFrom: checkpoint.LastProcessedIndex,
                cancellationToken: cancellationToken))
            {
                // Process batch
                var batchResult = await ProcessBatchAsync(batch, cancellationToken);
                result.Merge(batchResult);

                // Save checkpoint periodically
                if (batch.Index % 10 == 0)
                {
                    checkpoint.LastProcessedIndex = batch.Index;
                    checkpoint.PartialResults = result;
                    await SaveCheckpointAsync(checkpoint);
                }

                // Check for graceful shutdown
                if (cancellationToken.IsCancellationRequested)
                {
                    result.WasGracefullyCancelled = true;
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            result.WasForcefullyCancelled = true;
            await SaveCheckpointAsync(checkpoint);
            throw;
        }

        return result;
    }
}
```

### Progress reporting in multi-stage pipelines

Complex pipelines require sophisticated progress tracking that aggregates progress from multiple concurrent operations:

```csharp
public class MultiStageProgressReporter
{
    // Hierarchical progress tracking
    public class HierarchicalProgress : IProgress<CompositeProgress>
    {
        private readonly IProgress<CompositeProgress> _parent;
        private readonly string _stageName;
        private readonly double _weight;
        private readonly Dictionary<string, StageProgress> _stages;
        private readonly object _lock = new();

        public HierarchicalProgress(
            IProgress<CompositeProgress> parent = null,
            string stageName = "Root",
            double weight = 1.0)
        {
            _parent = parent;
            _stageName = stageName;
            _weight = weight;
            _stages = new Dictionary<string, StageProgress>();
        }

        public IProgress<T> CreateSubProgress<T>(
            string stageName,
            double weight = 1.0,
            Func<T, double> progressExtractor = null)
        {
            lock (_lock)
            {
                var stageProgress = new StageProgress
                {
                    Name = stageName,
                    Weight = weight,
                    Progress = 0
                };

                _stages[stageName] = stageProgress;

                return new SubProgress<T>(this, stageName, progressExtractor);
            }
        }

        public void Report(CompositeProgress value)
        {
            lock (_lock)
            {
                // Calculate weighted progress
                var totalWeight = _stages.Values.Sum(s => s.Weight);
                var weightedProgress = _stages.Values
                    .Sum(s => s.Progress * s.Weight / totalWeight);

                var composite = new CompositeProgress
                {
                    StageName = _stageName,
                    OverallProgress = weightedProgress,
                    StageDetails = _stages.Values.ToList(),
                    Message = value?.Message
                };

                _parent?.Report(composite);
            }
        }

        private class SubProgress<T> : IProgress<T>
        {
            private readonly HierarchicalProgress _parent;
            private readonly string _stageName;
            private readonly Func<T, double> _extractor;

            public SubProgress(
                HierarchicalProgress parent,
                string stageName,
                Func<T, double> extractor)
            {
                _parent = parent;
                _stageName = stageName;
                _extractor = extractor ?? DefaultExtractor;
            }

            public void Report(T value)
            {
                var progress = _extractor(value);

                lock (_parent._lock)
                {
                    if (_parent._stages.TryGetValue(_stageName, out var stage))
                    {
                        stage.Progress = progress;
                        _parent.Report(null);
                    }
                }
            }

            private double DefaultExtractor(T value)
            {
                return value switch
                {
                    double d => d,
                    float f => f,
                    int i => i,
                    IProgressable p => p.Progress,
                    _ => 0
                };
            }
        }
    }

    // Real-world usage example
    public async Task<ProcessingResult> ProcessWithDetailedProgressAsync(
        ImageBatch batch,
        IProgress<CompositeProgress> progress,
        CancellationToken cancellationToken = default)
    {
        var hierarchicalProgress = new HierarchicalProgress(progress, "Batch Processing");

        // Create sub-progress for each stage
        var loadProgress = hierarchicalProgress.CreateSubProgress<double>("Loading", 0.2);
        var decodeProgress = hierarchicalProgress.CreateSubProgress<double>("Decoding", 0.3);
        var processProgress = hierarchicalProgress.CreateSubProgress<double>("Processing", 0.4);
        var saveProgress = hierarchicalProgress.CreateSubProgress<double>("Saving", 0.1);

        // Stage 1: Load images
        var images = await LoadImagesAsync(
            batch.Paths,
            loadProgress,
            cancellationToken);

        // Stage 2: Decode in parallel with progress
        var decoded = await DecodeImagesAsync(
            images,
            decodeProgress,
            cancellationToken);

        // Stage 3: Process with detailed sub-progress
        var processSubProgress = new HierarchicalProgress(
            processProgress,
            "Image Processing");

        var filterProgress = processSubProgress.CreateSubProgress<FilterProgress>(
            "Filters", 0.6,
            f => f.CompletedFilters / (double)f.TotalFilters);

        var analysisProgress = processSubProgress.CreateSubProgress<int>(
            "Analysis", 0.4,
            completed => completed / (double)decoded.Count);

        var processed = await ProcessImagesAsync(
            decoded,
            filterProgress,
            analysisProgress,
            cancellationToken);

        // Stage 4: Save results
        return await SaveResultsAsync(
            processed,
            saveProgress,
            cancellationToken);
    }
}

// Throttled progress reporting
public class ThrottledProgress<T> : IProgress<T>
{
    private readonly IProgress<T> _innerProgress;
    private readonly TimeSpan _minInterval;
    private DateTime _lastReport = DateTime.MinValue;
    private T _latestValue;
    private Timer _timer;
    private readonly object _lock = new();

    public ThrottledProgress(
        IProgress<T> innerProgress,
        TimeSpan minInterval)
    {
        _innerProgress = innerProgress;
        _minInterval = minInterval;
    }

    public void Report(T value)
    {
        lock (_lock)
        {
            _latestValue = value;

            var now = DateTime.UtcNow;
            var elapsed = now - _lastReport;

            if (elapsed >= _minInterval)
            {
                _innerProgress.Report(value);
                _lastReport = now;
            }
            else
            {
                // Schedule delayed report
                _timer?.Dispose();
                var delay = _minInterval - elapsed;
                _timer = new Timer(
                    _ => ReportLatest(),
                    null,
                    delay,
                    Timeout.InfiniteTimeSpan);
            }
        }
    }

    private void ReportLatest()
    {
        lock (_lock)
        {
            _innerProgress.Report(_latestValue);
            _lastReport = DateTime.UtcNow;
            _timer?.Dispose();
            _timer = null;
        }
    }
}
```

### Real-time progress visualization

Modern applications demand rich progress visualization that goes beyond simple percentage bars:

```csharp
public class RealTimeProgressVisualizer
{
    // Progress with ETA calculation
    public class ProgressWithEta : IProgress<DetailedProgress>
    {
        private readonly IProgress<ProgressInfo> _output;
        private readonly Queue<ProgressSnapshot> _history;
        private readonly int _historySize;
        private DateTime _startTime;
        private readonly object _lock = new();

        public ProgressWithEta(IProgress<ProgressInfo> output, int historySize = 10)
        {
            _output = output;
            _history = new Queue<ProgressSnapshot>(historySize);
            _historySize = historySize;
            _startTime = DateTime.UtcNow;
        }

        public void Report(DetailedProgress value)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var snapshot = new ProgressSnapshot
                {
                    Timestamp = now,
                    Progress = value.PercentComplete,
                    ItemsProcessed = value.ItemsProcessed,
                    TotalItems = value.TotalItems
                };

                _history.Enqueue(snapshot);
                if (_history.Count > _historySize)
                {
                    _history.Dequeue();
                }

                var info = CalculateProgressInfo(value, now);
                _output.Report(info);
            }
        }

        private ProgressInfo CalculateProgressInfo(DetailedProgress current, DateTime now)
        {
            var info = new ProgressInfo
            {
                PercentComplete = current.PercentComplete,
                CurrentOperation = current.CurrentOperation,
                ItemsProcessed = current.ItemsProcessed,
                TotalItems = current.TotalItems
            };

            // Calculate rates
            var elapsed = now - _startTime;
            info.ElapsedTime = elapsed;

            if (_history.Count >= 2)
            {
                var oldest = _history.First();
                var timeSpan = now - oldest.Timestamp;
                var itemsDelta = current.ItemsProcessed - oldest.ItemsProcessed;

                info.ItemsPerSecond = itemsDelta / timeSpan.TotalSeconds;

                // Estimate remaining time
                if (info.ItemsPerSecond > 0 && current.TotalItems > 0)
                {
                    var remainingItems = current.TotalItems - current.ItemsProcessed;
                    var remainingSeconds = remainingItems / info.ItemsPerSecond;
                    info.EstimatedTimeRemaining = TimeSpan.FromSeconds(remainingSeconds);
                    info.EstimatedCompletion = now + info.EstimatedTimeRemaining;
                }
            }

            // Memory and performance metrics
            info.MemoryUsageMB = GC.GetTotalMemory(false) / (1024 * 1024);
            info.Gen0Collections = GC.CollectionCount(0);
            info.Gen1Collections = GC.CollectionCount(1);
            info.Gen2Collections = GC.CollectionCount(2);

            return info;
        }
    }

    // Live progress streaming
    public class ProgressHub
    {
        private readonly Channel<ProgressUpdate> _channel;
        private readonly ConcurrentDictionary<Guid, OperationProgress> _operations;

        public ProgressHub()
        {
            var options = new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            };

            _channel = Channel.CreateUnbounded<ProgressUpdate>(options);
            _operations = new ConcurrentDictionary<Guid, OperationProgress>();
        }

        public IProgress<T> CreateProgressReporter<T>(
            Guid operationId,
            string operationName)
        {
            var operation = new OperationProgress
            {
                Id = operationId,
                Name = operationName,
                StartTime = DateTime.UtcNow,
                Status = OperationStatus.Running
            };

            _operations[operationId] = operation;

            return new ChannelProgress<T>(this, operationId);
        }

        public async IAsyncEnumerable<ProgressUpdate> StreamProgressAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var update in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return update;
            }
        }

        private class ChannelProgress<T> : IProgress<T>
        {
            private readonly ProgressHub _hub;
            private readonly Guid _operationId;

            public ChannelProgress(ProgressHub hub, Guid operationId)
            {
                _hub = hub;
                _operationId = operationId;
            }

            public void Report(T value)
            {
                if (_hub._operations.TryGetValue(_operationId, out var operation))
                {
                    var update = new ProgressUpdate
                    {
                        OperationId = _operationId,
                        Timestamp = DateTime.UtcNow,
                        Data = value,
                        Operation = operation
                    };

                    // Update operation state
                    if (value is IProgressData progressData)
                    {
                        operation.Progress = progressData.Progress;
                        operation.Message = progressData.Message;
                    }

                    // Non-blocking write
                    _hub._channel.Writer.TryWrite(update);
                }
            }
        }
    }
}
```

## Conclusion

Asynchronous processing patterns have evolved from simple async/await usage to sophisticated architectures that
elegantly handle the complex requirements of modern graphics applications. The patterns explored in this chapter—from
basic Task-based operations to complex pipeline parallelism, from resource management to progress visualization—provide
the foundation for building responsive, scalable graphics processing systems.

The key insights to remember:

1. **Choose the right async primitive**: Use Task for general async operations, ValueTask for hot paths,
   IAsyncEnumerable for streaming, and Channels for producer-consumer scenarios.

2. **Design for cancellation**: Build cancellation deeply into your architecture, not as an afterthought. Graceful
   cancellation with checkpoints enables robust long-running operations.

3. **Manage resources carefully**: IAsyncDisposable and proper pooling patterns prevent resource leaks in complex async
   scenarios. Memory pressure awareness is crucial for graphics applications.

4. **Embrace pipeline parallelism**: Modern CPUs and GPUs excel at parallel processing. Dataflow patterns enable elegant
   expression of complex processing pipelines.

5. **Provide rich progress feedback**: Users expect detailed progress information. Hierarchical progress tracking with
   ETA calculation transforms opaque operations into transparent processes.

Looking forward, the continued evolution of C# async patterns promises even more powerful abstractions. The integration
of async streams with LINQ, improvements in ValueTask performance, and potential hardware-accelerated async primitives
will further enhance our ability to build responsive graphics applications. By mastering these patterns today,
developers prepare themselves for the high-performance, real-time graphics applications of tomorrow.
