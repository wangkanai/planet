# Chapter 18: Cloud-Ready Architecture

Modern graphics processing applications increasingly operate in cloud environments, requiring architectural patterns
that embrace distributed computing, containerization, and cloud-native services. This chapter explores how to design and
implement graphics processing systems that scale horizontally, leverage cloud storage efficiently, and operate reliably
in distributed environments. The patterns and techniques presented here transform traditional monolithic graphics
applications into resilient, scalable services capable of handling enterprise workloads.

## 18.1 Microservice Design Patterns

The transition from monolithic graphics applications to microservice architectures represents a fundamental shift in how
we approach image processing at scale. Unlike traditional desktop applications where all processing occurs within a
single process, microservice architectures decompose graphics operations into discrete, independently deployable
services that communicate through well-defined interfaces.

### Decomposing graphics operations into services

The key to successful microservice design lies in identifying natural service boundaries that align with both technical
capabilities and business domains. Graphics processing naturally decomposes into several core services, each responsible
for a specific aspect of the processing pipeline.

The **image ingestion service** handles the critical task of accepting uploads, validating formats, and performing
initial preprocessing. This service acts as the gateway to the system, implementing robust error handling and format
detection:

```csharp
public class ImageIngestionService : BackgroundService
{
    private readonly IMessageQueue _queue;
    private readonly IObjectStorage _storage;
    private readonly IImageValidator _validator;
    private readonly IMetrics _metrics;

    public async Task<IngestionResult> IngestImageAsync(Stream imageStream, IngestionOptions options)
    {
        // Record ingestion metrics for monitoring
        using var timer = _metrics.StartTimer("image_ingestion_duration");

        try
        {
            // Validate image format and integrity
            var validationResult = await _validator.ValidateAsync(imageStream);
            if (!validationResult.IsValid)
            {
                _metrics.IncrementCounter("image_ingestion_rejected");
                return IngestionResult.Failed(validationResult.Errors);
            }

            // Generate unique identifier for tracking
            var imageId = GenerateImageId(options);

            // Store original image in cloud storage
            var storageKey = $"originals/{imageId}/{options.FileName}";
            await _storage.UploadAsync(storageKey, imageStream, new StorageOptions
            {
                ContentType = validationResult.MimeType,
                Metadata = new Dictionary<string, string>
                {
                    ["width"] = validationResult.Width.ToString(),
                    ["height"] = validationResult.Height.ToString(),
                    ["format"] = validationResult.Format.ToString(),
                    ["upload_time"] = DateTime.UtcNow.ToString("O")
                }
            });

            // Queue for downstream processing
            var processingMessage = new ImageProcessingMessage
            {
                ImageId = imageId,
                StorageKey = storageKey,
                RequestedOperations = options.InitialOperations,
                Priority = CalculatePriority(validationResult, options)
            };

            await _queue.PublishAsync("image-processing", processingMessage);

            _metrics.IncrementCounter("image_ingestion_success");
            return IngestionResult.Success(imageId);
        }
        catch (Exception ex)
        {
            _metrics.IncrementCounter("image_ingestion_error");
            _logger.LogError(ex, "Failed to ingest image");
            throw;
        }
    }

    // Priority calculation based on image characteristics and business rules
    private ProcessingPriority CalculatePriority(ValidationResult validation, IngestionOptions options)
    {
        // Smaller images get higher priority for better user experience
        if (validation.Width * validation.Height < 1_000_000)
            return ProcessingPriority.High;

        // Premium customers get elevated priority
        if (options.CustomerTier == CustomerTier.Premium)
            return ProcessingPriority.High;

        // Large images process with normal priority
        if (validation.Width * validation.Height > 10_000_000)
            return ProcessingPriority.Low;

        return ProcessingPriority.Normal;
    }
}
```

The **transformation service** handles the core graphics operations, designed as a stateless worker that can scale
horizontally based on queue depth. This service implements operation chaining and efficient resource management:

```csharp
public class TransformationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageQueue _queue;
    private readonly IObjectStorage _storage;
    private readonly TransformationPipeline _pipeline;

    public async Task ProcessTransformationAsync(TransformationMessage message)
    {
        // Create scoped context for this transformation
        using var scope = _serviceProvider.CreateScope();
        var context = new TransformationContext
        {
            ImageId = message.ImageId,
            TraceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString()
        };

        try
        {
            // Download source image with retry logic
            var sourceImage = await DownloadWithRetryAsync(message.SourceKey, context);

            // Apply transformation chain
            using (sourceImage)
            {
                var result = await _pipeline.ExecuteAsync(sourceImage, message.Operations, context);

                // Upload transformed result
                var outputKey = GenerateOutputKey(message);
                await _storage.UploadAsync(outputKey, result.ImageStream, new StorageOptions
                {
                    ContentType = result.MimeType,
                    CacheControl = "public, max-age=31536000",
                    Metadata = result.Metadata
                });

                // Notify completion
                await PublishCompletionEventAsync(message, outputKey, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transformation failed for image {ImageId}", message.ImageId);
            await HandleFailureAsync(message, ex, context);
        }
    }

    // Resilient download with exponential backoff
    private async Task<Image<Rgba32>> DownloadWithRetryAsync(string key, TransformationContext context)
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Download retry {RetryCount} after {TimeSpan}ms for key {Key}",
                        retryCount, timeSpan.TotalMilliseconds, key);
                });

        return await retryPolicy.ExecuteAsync(async () =>
        {
            var stream = await _storage.DownloadAsync(key);
            return await Image.LoadAsync<Rgba32>(stream);
        });
    }
}
```

### Event-driven architecture for image processing

Event-driven patterns enable loose coupling between services while maintaining system coherence. Each service publishes
events about significant state changes, allowing other services to react without direct dependencies. This architecture
provides natural audit trails and enables complex workflows through event choreography.

The event bus implementation ensures reliable message delivery and ordering:

```csharp
public class ImageProcessingEventBus
{
    private readonly IEventStore _eventStore;
    private readonly IMessageBroker _broker;
    private readonly ISerializer _serializer;

    public async Task PublishAsync<TEvent>(TEvent eventData) where TEvent : ImageEvent
    {
        // Persist event for durability and audit
        var eventEnvelope = new EventEnvelope
        {
            EventId = Guid.NewGuid(),
            EventType = typeof(TEvent).Name,
            AggregateId = eventData.ImageId,
            Timestamp = DateTime.UtcNow,
            Data = _serializer.Serialize(eventData),
            Metadata = CaptureMetadata()
        };

        await _eventStore.AppendAsync(eventEnvelope);

        // Publish to message broker for real-time processing
        await _broker.PublishAsync(
            topic: $"images.{eventData.EventType.ToLower()}",
            message: eventEnvelope,
            headers: new Dictionary<string, string>
            {
                ["trace-id"] = Activity.Current?.TraceId.ToString(),
                ["correlation-id"] = eventData.CorrelationId
            });
    }

    // Event sourcing for complete processing history
    public async Task<ProcessingHistory> GetHistoryAsync(string imageId)
    {
        var events = await _eventStore.GetEventsAsync(imageId);

        return new ProcessingHistory
        {
            ImageId = imageId,
            Events = events.Select(e => new ProcessingEvent
            {
                EventType = e.EventType,
                Timestamp = e.Timestamp,
                Details = _serializer.Deserialize<ImageEvent>(e.Data),
                Metadata = e.Metadata
            }).ToList(),
            CurrentState = DeriveStateFromEvents(events)
        };
    }
}
```

### Service mesh considerations for graphics workloads

Service mesh technologies like Istio or Linkerd provide crucial infrastructure for microservice deployments, but
graphics workloads present unique challenges. Large image transfers can overwhelm sidecar proxies designed for typical
HTTP traffic, requiring careful configuration of timeouts, buffer sizes, and circuit breaker thresholds.

For optimal performance, graphics services often bypass the service mesh for data plane operations while maintaining
control plane integration:

```csharp
public class GraphicsServiceMeshAdapter
{
    private readonly IServiceDiscovery _discovery;
    private readonly ILoadBalancer _loadBalancer;
    private readonly HttpClient _dataPlaneClient;
    private readonly HttpClient _controlPlaneClient;

    public async Task<TransformationResult> InvokeTransformationAsync(
        TransformationRequest request,
        CancellationToken cancellationToken)
    {
        // Use service mesh for endpoint discovery
        var endpoints = await _discovery.GetHealthyEndpointsAsync("transformation-service");
        var endpoint = _loadBalancer.SelectEndpoint(endpoints, request);

        // Bypass sidecar for large data transfers
        if (request.EstimatedSize > 10 * 1024 * 1024) // 10MB threshold
        {
            return await DirectDataPlaneInvocationAsync(endpoint, request, cancellationToken);
        }

        // Use sidecar for smaller requests benefiting from mesh features
        return await MeshProxiedInvocationAsync(endpoint, request, cancellationToken);
    }

    private async Task<TransformationResult> DirectDataPlaneInvocationAsync(
        ServiceEndpoint endpoint,
        TransformationRequest request,
        CancellationToken cancellationToken)
    {
        // Direct connection with custom retry and circuit breaker logic
        var circuitBreaker = CircuitBreakerFactory.Create(endpoint.Id, new CircuitBreakerOptions
        {
            FailureThreshold = 5,
            SuccessThreshold = 2,
            Timeout = TimeSpan.FromMinutes(5), // Longer timeout for large images
            HalfOpenTestInterval = TimeSpan.FromSeconds(30)
        });

        return await circuitBreaker.ExecuteAsync(async () =>
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint.DataPlaneUrl)
            {
                Content = new StreamContent(request.ImageStream)
            };

            // Add distributed tracing headers
            InjectTracingHeaders(httpRequest);

            var response = await _dataPlaneClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await DeserializeResponseAsync(response);
        });
    }
}
```

## 18.2 Containerization Strategies

Containerizing graphics applications requires careful consideration of image size, layer caching, and runtime
performance. Unlike typical web services, graphics containers often include large libraries, GPU drivers, and
specialized dependencies that can result in multi-gigabyte images if not properly optimized.

### Multi-stage builds for minimal runtime images

Multi-stage Docker builds enable separation of build-time and runtime dependencies, dramatically reducing final image
size. The build stage includes compilers, development headers, and build tools, while the runtime stage contains only
essential libraries:

```dockerfile
# Build stage with full SDK and build tools
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install native dependencies for image processing
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libjpeg-dev \
    libpng-dev \
    libwebp-dev \
    libavif-dev \
    build-essential \
    cmake

# Copy and restore dependencies separately for layer caching
COPY ["src/ImageProcessor/ImageProcessor.csproj", "ImageProcessor/"]
COPY ["src/ImageProcessor.Core/ImageProcessor.Core.csproj", "ImageProcessor.Core/"]
RUN dotnet restore "ImageProcessor/ImageProcessor.csproj"

# Copy source and build
COPY src/ .
RUN dotnet build "ImageProcessor/ImageProcessor.csproj" -c Release -o /app/build

# Publish with native AOT for reduced memory footprint
FROM build AS publish
RUN dotnet publish "ImageProcessor/ImageProcessor.csproj" \
    -c Release \
    -o /app/publish \
    -p:PublishAot=true \
    -p:StripSymbols=true \
    -p:TrimMode=full

# Runtime stage with minimal dependencies
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-alpine AS runtime
WORKDIR /app

# Install only runtime libraries
RUN apk add --no-cache \
    libjpeg-turbo \
    libpng \
    libwebp \
    libavif \
    && rm -rf /var/cache/apk/*

# Create non-root user for security
RUN addgroup -g 1000 imageproc && \
    adduser -u 1000 -G imageproc -D imageproc

# Copy published application
COPY --from=publish --chown=imageproc:imageproc /app/publish .

# Configure runtime
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
    ASPNETCORE_URLS=http://+:8080 \
    PROCESSOR_MEMORY_LIMIT=2GB \
    PROCESSOR_THREAD_COUNT=4

USER imageproc
EXPOSE 8080

ENTRYPOINT ["./ImageProcessor"]
```

### GPU support in containers

Graphics-intensive operations benefit significantly from GPU acceleration, requiring specialized container
configurations. NVIDIA Container Toolkit enables GPU access within containers, but requires careful resource allocation
and driver compatibility:

```dockerfile
# GPU-enabled runtime stage
FROM nvidia/cuda:12.2.0-runtime-ubuntu22.04 AS gpu-runtime

# Install .NET runtime and graphics libraries
RUN apt-get update && apt-get install -y \
    dotnet-runtime-9.0 \
    libcudnn8 \
    libnpp-12-2 \
    && rm -rf /var/lib/apt/lists/*

# Copy application with GPU-accelerated libraries
COPY --from=publish /app/publish .

# Configure GPU resource requirements
ENV NVIDIA_VISIBLE_DEVICES=all \
    NVIDIA_DRIVER_CAPABILITIES=compute,utility,graphics \
    CUDA_CACHE_MAXSIZE=2147483648 \
    PROCESSOR_GPU_ENABLED=true

# Health check including GPU availability
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD test -e /dev/nvidia0 && dotnet ImageProcessor.dll --health || exit 1
```

The corresponding Kubernetes deployment ensures proper GPU scheduling:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
	name: image-processor-gpu
spec:
	replicas: 3
	selector:
		matchLabels:
			app: image-processor-gpu
	template:
		metadata:
			labels:
				app: image-processor-gpu
		spec:
			nodeSelector:
				accelerator: nvidia-tesla-t4
			containers:
				-   name: processor
					image: myregistry/image-processor:gpu-latest
					resources:
						limits:
							nvidia.com/gpu: 1
							memory: "8Gi"
							cpu: "4"
						requests:
							nvidia.com/gpu: 1
							memory: "4Gi"
							cpu: "2"
					volumeMounts:
						-   name: dshm
							mountPath: /dev/shm
					env:
						-   name: PROCESSOR_MODE
							value: "GPU_ACCELERATED"
						-   name: BATCH_SIZE
							value: "32"
			volumes:
				-   name: dshm
					emptyDir:
						medium: Memory
						sizeLimit: 2Gi
```

### Optimizing container startup times

Graphics containers often suffer from slow startup times due to library initialization and model loading. Several
strategies mitigate this issue. Pre-warming containers through readiness probes ensures full initialization before
receiving traffic. Lazy loading defers expensive operations until needed. Shared memory volumes enable fast
inter-process communication for model sharing:

```csharp
public class ContainerOptimizedStartup : IHostedService
{
    private readonly ILogger<ContainerOptimizedStartup> _logger;
    private readonly IHealthCheckService _healthCheck;
    private readonly ModelCache _modelCache;
    private readonly ConcurrentDictionary<string, Task> _initializationTasks;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting container initialization");

        // Phase 1: Critical path initialization
        await InitializeCriticalComponentsAsync(cancellationToken);
        _healthCheck.SetLiveness(true);

        // Phase 2: Parallel non-critical initialization
        var nonCriticalTasks = new[]
        {
            Task.Run(() => PreloadFrequentModelsAsync(cancellationToken)),
            Task.Run(() => WarmConnectionPoolsAsync(cancellationToken)),
            Task.Run(() => InitializeGPUContextAsync(cancellationToken))
        };

        // Don't block startup on non-critical tasks
        _ = Task.WhenAll(nonCriticalTasks).ContinueWith(t =>
        {
            if (t.IsFaulted)
                _logger.LogError(t.Exception, "Non-critical initialization failed");
            else
                _healthCheck.SetReadiness(true);
        });

        _logger.LogInformation("Container startup completed in {ElapsedMs}ms",
            Environment.TickCount64 - Process.GetCurrentProcess().StartTime.Ticks / 10000);
    }

    private async Task PreloadFrequentModelsAsync(CancellationToken cancellationToken)
    {
        // Load models based on historical usage patterns
        var frequentModels = new[] { "resize", "jpeg-encoder", "webp-encoder" };

        await Parallel.ForEachAsync(frequentModels, cancellationToken, async (model, ct) =>
        {
            try
            {
                await _modelCache.PreloadAsync(model, ct);
                _logger.LogDebug("Preloaded model: {Model}", model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to preload model: {Model}", model);
            }
        });
    }
}
```

## 18.3 Distributed Processing

Distributed processing transforms graphics operations from single-machine limitations to cloud-scale capabilities. This
approach requires careful orchestration, data partitioning strategies, and coordination mechanisms that maintain
consistency while maximizing parallelism.

### Work queue patterns for image operations

Work queues decouple request ingestion from processing, enabling elastic scaling based on workload. The queue-based
architecture provides natural buffering, priority handling, and failure isolation:

```csharp
public class DistributedImageProcessor
{
    private readonly IMessageQueue _queue;
    private readonly IDistributedCache _cache;
    private readonly IObjectStorage _storage;
    private readonly ProcessorPool _processorPool;

    public async Task<ProcessingResult> ProcessDistributedAsync(
        DistributedProcessingRequest request)
    {
        // Partition large images for parallel processing
        var partitions = await PartitionImageAsync(request.ImageId, request.Operation);

        // Create distributed processing job
        var job = new DistributedJob
        {
            JobId = Guid.NewGuid().ToString(),
            ImageId = request.ImageId,
            TotalPartitions = partitions.Count,
            CreatedAt = DateTime.UtcNow,
            Status = JobStatus.Pending
        };

        // Store job metadata in distributed cache
        await _cache.SetAsync($"job:{job.JobId}", job, new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(1)
        });

        // Queue partition tasks
        var partitionTasks = partitions.Select((partition, index) =>
            QueuePartitionTaskAsync(job, partition, index)).ToList();

        await Task.WhenAll(partitionTasks);

        // Monitor job completion
        return await MonitorJobCompletionAsync(job);
    }

    private async Task<List<ImagePartition>> PartitionImageAsync(
        string imageId,
        ProcessingOperation operation)
    {
        var metadata = await GetImageMetadataAsync(imageId);

        // Determine optimal partition size based on operation type
        var partitionStrategy = operation switch
        {
            ProcessingOperation.Resize => new TilePartitionStrategy(512, 512),
            ProcessingOperation.Filter => new StripPartitionStrategy(metadata.Height / Environment.ProcessorCount),
            ProcessingOperation.ColorCorrection => new QuadrantPartitionStrategy(),
            _ => new SinglePartitionStrategy()
        };

        return partitionStrategy.CreatePartitions(metadata);
    }

    private async Task QueuePartitionTaskAsync(
        DistributedJob job,
        ImagePartition partition,
        int partitionIndex)
    {
        var task = new PartitionProcessingTask
        {
            JobId = job.JobId,
            PartitionIndex = partitionIndex,
            Bounds = partition.Bounds,
            SourceKey = partition.SourceKey,
            Operation = job.Operation,
            Priority = CalculatePriority(job, partition)
        };

        // Use consistent hashing for partition affinity
        var queuePartition = GetQueuePartition(task.SourceKey);

        await _queue.PublishAsync($"image-processing.{queuePartition}", task, new PublishOptions
        {
            TTL = TimeSpan.FromHours(1),
            MaxRetries = 3,
            Headers = new Dictionary<string, string>
            {
                ["job-id"] = job.JobId,
                ["partition-count"] = job.TotalPartitions.ToString()
            }
        });
    }
}
```

### Coordination strategies for parallel processing

Coordinating distributed image processing requires sophisticated synchronization mechanisms. The coordinator pattern
ensures all partitions complete successfully before assembling the final result:

```csharp
public class ProcessingCoordinator
{
    private readonly IDistributedLock _distributedLock;
    private readonly IEventBus _eventBus;
    private readonly IStateStore _stateStore;

    public async Task<CoordinationResult> CoordinateProcessingAsync(
        string jobId,
        Func<ImagePartition, Task<PartitionResult>> processFunc)
    {
        var job = await _stateStore.GetJobAsync(jobId);
        var completionSource = new TaskCompletionSource<CoordinationResult>();

        // Subscribe to partition completion events
        var subscription = await _eventBus.SubscribeAsync<PartitionCompletedEvent>(
            $"job.{jobId}.partition.completed",
            async (evt) => await HandlePartitionCompletionAsync(evt, job, completionSource));

        try
        {
            // Process partitions with work stealing for load balancing
            await ProcessWithWorkStealingAsync(job, processFunc);

            // Wait for all partitions to complete
            var result = await completionSource.Task.WaitAsync(
                TimeSpan.FromMinutes(job.EstimatedDuration));

            return result;
        }
        finally
        {
            await subscription.DisposeAsync();
        }
    }

    private async Task ProcessWithWorkStealingAsync(
        DistributedJob job,
        Func<ImagePartition, Task<PartitionResult>> processFunc)
    {
        var workQueue = new ConcurrentQueue<ImagePartition>(job.Partitions);
        var workers = new Task[Environment.ProcessorCount];

        for (int i = 0; i < workers.Length; i++)
        {
            workers[i] = Task.Run(async () =>
            {
                while (workQueue.TryDequeue(out var partition))
                {
                    try
                    {
                        var result = await processFunc(partition);
                        await PublishPartitionResultAsync(job.JobId, partition, result);
                    }
                    catch (Exception ex)
                    {
                        await HandlePartitionFailureAsync(job.JobId, partition, ex);
                    }
                }

                // Attempt to steal work from slower workers
                await AttemptWorkStealingAsync(job.JobId, processFunc);
            });
        }

        await Task.WhenAll(workers);
    }

    private async Task HandlePartitionCompletionAsync(
        PartitionCompletedEvent evt,
        DistributedJob job,
        TaskCompletionSource<CoordinationResult> completionSource)
    {
        // Use distributed lock for atomic state updates
        await using var lockHandle = await _distributedLock.AcquireAsync(
            $"job:{job.JobId}:completion",
            TimeSpan.FromSeconds(30));

        var jobState = await _stateStore.GetJobStateAsync(job.JobId);
        jobState.CompletedPartitions.Add(evt.PartitionIndex);

        if (jobState.CompletedPartitions.Count == job.TotalPartitions)
        {
            // All partitions complete - trigger assembly
            var assemblyResult = await AssemblePartitionsAsync(job);
            completionSource.SetResult(new CoordinationResult
            {
                JobId = job.JobId,
                Status = JobStatus.Completed,
                OutputKey = assemblyResult.OutputKey,
                ProcessingTime = DateTime.UtcNow - job.CreatedAt
            });
        }
        else
        {
            // Update progress
            await _stateStore.UpdateJobStateAsync(job.JobId, jobState);
            await PublishProgressUpdateAsync(job, jobState);
        }
    }
}
```

### Handling failures in distributed systems

Distributed systems must gracefully handle various failure modes including partial failures, network partitions, and
node crashes. The implementation employs multiple strategies for resilience:

```csharp
public class ResilientProcessingStrategy
{
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly IRetryPolicy _retryPolicy;
    private readonly ICompensationService _compensation;

    public async Task<ProcessingResult> ProcessWithResilienceAsync(
        ProcessingRequest request,
        ProcessingContext context)
    {
        var attempt = 0;
        var lastException = default(Exception);

        while (attempt < context.MaxAttempts)
        {
            try
            {
                // Check circuit breaker state
                if (!_circuitBreaker.AllowRequest(request.TargetService))
                {
                    throw new CircuitBreakerOpenException(
                        $"Circuit breaker open for {request.TargetService}");
                }

                // Execute with timeout
                using var cts = new CancellationTokenSource(context.Timeout);
                var result = await ExecuteProcessingAsync(request, cts.Token);

                // Success - notify circuit breaker
                _circuitBreaker.RecordSuccess(request.TargetService);
                return result;
            }
            catch (Exception ex) when (IsRetriableException(ex))
            {
                lastException = ex;
                attempt++;

                // Record failure for circuit breaker
                _circuitBreaker.RecordFailure(request.TargetService);

                // Calculate backoff delay
                var delay = _retryPolicy.GetDelay(attempt, ex);

                _logger.LogWarning(
                    "Processing attempt {Attempt} failed, retrying after {Delay}ms",
                    attempt, delay.TotalMilliseconds);

                await Task.Delay(delay);

                // Attempt compensation if available
                if (_compensation.CanCompensate(request))
                {
                    await _compensation.CompensateAsync(request, ex);
                }
            }
            catch (Exception ex)
            {
                // Non-retriable error - fail fast
                _logger.LogError(ex, "Non-retriable processing error");
                throw;
            }
        }

        throw new ProcessingException(
            $"Processing failed after {attempt} attempts",
            lastException);
    }

    private bool IsRetriableException(Exception ex)
    {
        return ex switch
        {
            TaskCanceledException => true,
            HttpRequestException => true,
            IOException => true,
            TimeoutException => true,
            CircuitBreakerOpenException => false,
            OutOfMemoryException => false,
            _ => false
        };
    }

    // Saga pattern for distributed transactions
    public async Task<SagaResult> ExecuteDistributedSagaAsync(
        ImageProcessingSaga saga,
        SagaContext context)
    {
        var executedSteps = new Stack<ISagaStep>();

        try
        {
            foreach (var step in saga.Steps)
            {
                _logger.LogInformation("Executing saga step: {StepName}", step.Name);

                await step.ExecuteAsync(context);
                executedSteps.Push(step);

                // Checkpoint after each step
                await SaveSagaCheckpointAsync(saga.Id, step.Name, context);
            }

            return SagaResult.Success(saga.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga execution failed at step {CurrentStep}",
                executedSteps.Peek()?.Name);

            // Compensate in reverse order
            await CompensateSagaAsync(executedSteps, context, ex);

            return SagaResult.Failed(saga.Id, ex);
        }
    }
}
```

## 18.4 Cloud Storage Integration

Cloud storage serves as the backbone for distributed graphics processing, requiring sophisticated integration patterns
that balance performance, cost, and reliability. Modern cloud storage services offer features specifically designed for
large media files, but effective utilization requires understanding their characteristics and limitations.

### Object storage patterns for images

Object storage systems like Amazon S3, Azure Blob Storage, and Google Cloud Storage provide virtually unlimited capacity
with high durability, but their eventual consistency model and request-based pricing require careful design
consideration:

```csharp
public class CloudOptimizedImageStorage
{
    private readonly IObjectStorageClient _client;
    private readonly IDistributedCache _metadataCache;
    private readonly StorageConfiguration _config;

    public async Task<StorageResult> StoreImageAsync(
        ProcessedImage image,
        StorageOptions options)
    {
        // Generate hierarchical key structure for efficient listing
        var key = GenerateStorageKey(image, options);

        // Prepare storage metadata
        var metadata = new Dictionary<string, string>
        {
            ["content-hash"] = image.ContentHash,
            ["dimensions"] = $"{image.Width}x{image.Height}",
            ["format"] = image.Format.ToString(),
            ["processing-date"] = DateTime.UtcNow.ToString("O"),
            ["quality"] = image.Quality.ToString(),
            ["color-profile"] = image.ColorProfile ?? "sRGB"
        };

        // Configure storage class based on access patterns
        var storageClass = DetermineStorageClass(options);

        // Upload with multipart for large files
        if (image.Size > _config.MultipartThreshold)
        {
            return await MultipartUploadAsync(image, key, metadata, storageClass);
        }

        // Standard upload for smaller files
        var result = await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _config.BucketName,
            Key = key,
            InputStream = image.Stream,
            ContentType = image.MimeType,
            Metadata = metadata,
            StorageClass = storageClass,
            ServerSideEncryption = ServerSideEncryption.AES256,
            CacheControl = GenerateCacheControl(options)
        });

        // Cache metadata for fast retrieval
        await CacheMetadataAsync(key, image, metadata);

        return new StorageResult
        {
            Key = key,
            ETag = result.ETag,
            VersionId = result.VersionId,
            StorageClass = storageClass
        };
    }

    private async Task<StorageResult> MultipartUploadAsync(
        ProcessedImage image,
        string key,
        Dictionary<string, string> metadata,
        StorageClass storageClass)
    {
        var uploadId = await InitiateMultipartUploadAsync(key, metadata, storageClass);
        var parts = new ConcurrentBag<UploadPartResponse>();

        try
        {
            // Calculate optimal part size (5MB - 100MB range)
            var partSize = CalculateOptimalPartSize(image.Size);
            var totalParts = (int)Math.Ceiling(image.Size / (double)partSize);

            // Upload parts in parallel
            await Parallel.ForEachAsync(
                Enumerable.Range(1, totalParts),
                new ParallelOptions { MaxDegreeOfParallelism = _config.MaxConcurrentUploads },
                async (partNumber, ct) =>
                {
                    var offset = (partNumber - 1) * partSize;
                    var size = Math.Min(partSize, image.Size - offset);

                    var partResponse = await UploadPartAsync(
                        key, uploadId, partNumber, image.Stream, offset, size, ct);

                    parts.Add(partResponse);
                });

            // Complete multipart upload
            return await CompleteMultipartUploadAsync(key, uploadId, parts.ToList());
        }
        catch
        {
            // Abort on failure to avoid orphaned parts
            await AbortMultipartUploadAsync(key, uploadId);
            throw;
        }
    }

    private string GenerateStorageKey(ProcessedImage image, StorageOptions options)
    {
        // Hierarchical structure optimized for common access patterns
        var components = new List<string>();

        // Date-based partitioning for time-series queries
        if (options.UseTimestampPartitioning)
        {
            var timestamp = image.ProcessingTimestamp;
            components.AddRange(new[]
            {
                timestamp.Year.ToString(),
                timestamp.Month.ToString("D2"),
                timestamp.Day.ToString("D2")
            });
        }

        // Content-based partitioning for efficient filtering
        components.Add(image.Format.ToString().ToLowerInvariant());

        // Size-based grouping for batch operations
        var sizeCategory = image.Size switch
        {
            < 100_000 => "small",
            < 1_000_000 => "medium",
            < 10_000_000 => "large",
            _ => "xlarge"
        };
        components.Add(sizeCategory);

        // Unique identifier
        components.Add($"{image.Id}_{image.Version}");

        return string.Join("/", components);
    }
}
```

### CDN integration strategies

Content Delivery Networks dramatically improve image delivery performance but require careful integration to maximize
cache hit rates and minimize origin requests:

```csharp
public class CDNOptimizedDelivery
{
    private readonly ICDNManager _cdnManager;
    private readonly IOriginStorage _originStorage;
    private readonly CacheStrategy _cacheStrategy;

    public async Task<DeliveryConfiguration> ConfigureDeliveryAsync(
        ImageAsset asset,
        DeliveryRequirements requirements)
    {
        // Generate CDN-optimized URL structure
        var urlStrategy = new ImmutableUrlStrategy
        {
            // Include version in URL for cache busting
            Pattern = "/{category}/{id}/v{version}/{variant}.{format}",

            // Use fingerprinting for static assets
            EnableFingerprinting = true,

            // Consistent ordering for variant parameters
            ParameterOrdering = new[] { "width", "height", "quality", "format" }
        };

        // Configure edge rules for image optimization
        var edgeRules = new EdgeRulesConfiguration
        {
            Rules = new[]
            {
                // Automatic format selection based on Accept header
                new EdgeRule
                {
                    Name = "auto-format",
                    Condition = "req.http.Accept ~ \"image/webp\"",
                    Actions = new[]
                    {
                        new SetVariableAction("req.url.format", "webp"),
                        new SetHeaderAction("Vary", "Accept")
                    }
                },

                // Device-based optimization
                new EdgeRule
                {
                    Name = "mobile-optimization",
                    Condition = "req.http.User-Agent ~ \"Mobile\"",
                    Actions = new[]
                    {
                        new SetVariableAction("req.url.quality", "85"),
                        new SetVariableAction("req.url.dpr", "2")
                    }
                },

                // Security headers
                new EdgeRule
                {
                    Name = "security-headers",
                    Condition = "resp.status == 200",
                    Actions = new[]
                    {
                        new SetHeaderAction("X-Content-Type-Options", "nosniff"),
                        new SetHeaderAction("Content-Security-Policy", "default-src 'none'; img-src 'self'")
                    }
                }
            }
        };

        // Configure cache hierarchy
        var cacheConfig = new HierarchicalCacheConfiguration
        {
            // Edge cache (global POPs)
            EdgeCache = new CacheTier
            {
                TTL = TimeSpan.FromDays(365),
                Key = "edge:${req.url}",
                BypassConditions = new[] { "req.http.Cache-Control ~ \"no-cache\"" }
            },

            // Regional cache (shield POPs)
            RegionalCache = new CacheTier
            {
                TTL = TimeSpan.FromDays(30),
                Key = "region:${req.url}:${geo.region}",
                EnableSoftPurge = true
            },

            // Origin shield
            OriginShield = new OriginShieldConfiguration
            {
                Enabled = true,
                Location = DetermineOptimalShieldLocation(requirements),
                ErrorCaching = TimeSpan.FromMinutes(5)
            }
        };

        return new DeliveryConfiguration
        {
            UrlStrategy = urlStrategy,
            EdgeRules = edgeRules,
            CacheConfiguration = cacheConfig,
            Analytics = ConfigureAnalytics(requirements)
        };
    }

    // Implement cache warming for critical assets
    public async Task WarmCachesAsync(
        IEnumerable<CriticalAsset> assets,
        WarmingStrategy strategy)
    {
        var warmingTasks = new List<Task>();

        foreach (var popLocation in strategy.TargetPOPs)
        {
            var popWarmer = new POPWarmer(popLocation);

            warmingTasks.Add(Task.Run(async () =>
            {
                foreach (var asset in assets)
                {
                    // Generate variant URLs based on common requests
                    var variants = GenerateWarmingVariants(asset, strategy);

                    await popWarmer.WarmAsync(variants, new WarmingOptions
                    {
                        Concurrency = 5,
                        RetryPolicy = RetryPolicy.Linear(3, TimeSpan.FromSeconds(2)),
                        ValidateResponse = true
                    });
                }
            }));
        }

        await Task.WhenAll(warmingTasks);

        // Verify warming success
        var verificationResults = await VerifyWarmingAsync(assets, strategy.TargetPOPs);

        _logger.LogInformation(
            "Cache warming completed: {SuccessRate}% success rate across {POPCount} POPs",
            verificationResults.SuccessRate, strategy.TargetPOPs.Count);
    }
}
```

### Cost optimization through intelligent tiering

Cloud storage costs accumulate quickly with large image libraries. Intelligent tiering based on access patterns
significantly reduces storage costs while maintaining performance:

```csharp
public class StorageLifecycleManager
{
    private readonly IStorageAnalytics _analytics;
    private readonly ILifecyclePolicyEngine _policyEngine;
    private readonly ITieringOrchestrator _tieringOrchestrator;

    public async Task<TieringPlan> GenerateOptimalTieringPlanAsync(
        string bucketName,
        TimeSpan analysisWindow)
    {
        // Analyze access patterns
        var accessPatterns = await _analytics.AnalyzeAccessPatternsAsync(
            bucketName,
            DateTime.UtcNow.Subtract(analysisWindow),
            DateTime.UtcNow);

        // Group objects by access frequency
        var objectGroups = accessPatterns.GroupBy(p => ClassifyAccessPattern(p))
            .ToDictionary(g => g.Key, g => g.ToList());

        var plan = new TieringPlan();

        // Hot tier: Frequently accessed (>10 requests/month)
        foreach (var hotObject in objectGroups[AccessTier.Hot])
        {
            plan.AddTransition(new TierTransition
            {
                ObjectKey = hotObject.Key,
                CurrentTier = hotObject.CurrentStorageClass,
                TargetTier = StorageClass.Standard,
                EstimatedMonthlySavings = CalculateSavings(hotObject, StorageClass.Standard),
                Rationale = "High access frequency justifies standard storage costs"
            });
        }

        // Warm tier: Occasional access (1-10 requests/month)
        foreach (var warmObject in objectGroups[AccessTier.Warm])
        {
            plan.AddTransition(new TierTransition
            {
                ObjectKey = warmObject.Key,
                CurrentTier = warmObject.CurrentStorageClass,
                TargetTier = StorageClass.StandardIA,
                EstimatedMonthlySavings = CalculateSavings(warmObject, StorageClass.StandardIA),
                Rationale = "Infrequent access pattern suits IA storage"
            });
        }

        // Cool tier: Rare access (<1 request/month)
        foreach (var coolObject in objectGroups[AccessTier.Cool])
        {
            // Consider Glacier for very large files
            var targetTier = coolObject.Size > 100_000_000
                ? StorageClass.Glacier
                : StorageClass.StandardIA;

            plan.AddTransition(new TierTransition
            {
                ObjectKey = coolObject.Key,
                CurrentTier = coolObject.CurrentStorageClass,
                TargetTier = targetTier,
                EstimatedMonthlySavings = CalculateSavings(coolObject, targetTier),
                Rationale = "Rare access allows for cold storage optimization"
            });
        }

        // Archive tier: No access for >90 days
        foreach (var archiveObject in objectGroups[AccessTier.Archive])
        {
            plan.AddTransition(new TierTransition
            {
                ObjectKey = archiveObject.Key,
                CurrentTier = archiveObject.CurrentStorageClass,
                TargetTier = StorageClass.GlacierDeepArchive,
                EstimatedMonthlySavings = CalculateSavings(archiveObject, StorageClass.GlacierDeepArchive),
                Rationale = "No recent access justifies deep archive storage"
            });
        }

        // Generate lifecycle policy
        plan.LifecyclePolicy = GenerateLifecyclePolicy(plan.Transitions);
        plan.EstimatedTotalSavings = plan.Transitions.Sum(t => t.EstimatedMonthlySavings);

        return plan;
    }

    public async Task<TieringExecutionResult> ExecuteTieringPlanAsync(
        TieringPlan plan,
        ExecutionOptions options)
    {
        var result = new TieringExecutionResult();
        var semaphore = new SemaphoreSlim(options.MaxConcurrentTransitions);

        // Group transitions by priority
        var prioritizedTransitions = plan.Transitions
            .OrderByDescending(t => t.EstimatedMonthlySavings)
            .ThenBy(t => t.ObjectSize);

        await Parallel.ForEachAsync(prioritizedTransitions, async (transition, ct) =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                var transitionResult = await ExecuteTransitionAsync(transition, options);
                result.AddTransitionResult(transitionResult);

                if (transitionResult.Success)
                {
                    // Update metadata cache
                    await UpdateStorageMetadataAsync(transition.ObjectKey, transition.TargetTier);
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        // Apply lifecycle policy for future objects
        if (options.ApplyLifecyclePolicy)
        {
            await ApplyLifecyclePolicyAsync(plan.LifecyclePolicy);
        }

        return result;
    }
}
```

## Summary

Cloud-ready architecture transforms traditional graphics processing into scalable, distributed systems capable of
handling modern workloads. The microservice approach provides flexibility and independent scaling, while
containerization ensures consistent deployment across environments. Distributed processing patterns enable horizontal
scaling beyond single-machine limitations, and intelligent cloud storage integration optimizes both performance and
cost.

These architectural patterns work together to create resilient systems that gracefully handle failures, scale
elastically with demand, and optimize resource utilization. By embracing cloud-native principles while respecting the
unique requirements of graphics processing, developers can build systems that deliver high performance at scale while
maintaining operational efficiency.

The journey from monolithic applications to cloud-native architectures requires careful consideration of data flow,
state management, and failure handling. However, the benefits of increased scalability, improved reliability, and
operational flexibility make this transformation essential for modern graphics processing systems. As cloud platforms
continue to evolve with better GPU support and specialized services for media processing, these architectural patterns
will become increasingly important for competitive advantage in the digital media landscape.
