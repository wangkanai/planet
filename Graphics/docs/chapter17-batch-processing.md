# Chapter 17: Batch Processing Systems

Batch processing systems represent a fundamental architecture pattern in high-performance graphics processing, enabling
the efficient transformation of large volumes of images through automated workflows. In the context of .NET 9.0 graphics
applications, these systems orchestrate complex pipelines that can process thousands or millions of images while
managing resources, handling failures gracefully, and providing detailed performance insights. This chapter explores the
design and implementation of production-grade batch processing systems that leverage .NET 9.0's enhanced parallelization
capabilities, improved memory management, and sophisticated monitoring infrastructure.

## 17.1 Workflow Engine Design

### Understanding Workflow Fundamentals

A workflow engine serves as the orchestration layer that coordinates the execution of image processing tasks through
defined pipelines. Unlike simple sequential processing, modern workflow engines must handle complex dependencies,
conditional execution paths, and dynamic resource allocation while maintaining high throughput and reliability. The
architecture must balance flexibility with performance, enabling both simple linear workflows and sophisticated directed
acyclic graphs (DAGs) that represent complex processing dependencies.

The core abstraction in workflow design centers around the concept of **workflow nodes** and **execution contexts**.
Each node represents a discrete processing operation, while the execution context maintains state, manages resources,
and facilitates communication between nodes. This separation of concerns enables workflows to be composed, tested, and
optimized independently while maintaining clear boundaries between processing stages.

### Implementing a Flexible Workflow Architecture

The foundation of our workflow engine begins with defining the core abstractions that enable both simple and complex
processing patterns:

```csharp
public abstract class WorkflowNode
{
    public string Id { get; }
    public string Name { get; set; }
    public WorkflowNodeStatus Status { get; private set; }
    public List<WorkflowNode> Dependencies { get; } = new();
    public Dictionary<string, object> Configuration { get; } = new();

    protected WorkflowNode(string id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Status = WorkflowNodeStatus.Pending;
    }

    public abstract Task<WorkflowResult> ExecuteAsync(
        WorkflowContext context,
        CancellationToken cancellationToken);

    public virtual async Task<bool> CanExecuteAsync(WorkflowContext context)
    {
        // Check if all dependencies have completed successfully
        foreach (var dependency in Dependencies)
        {
            if (dependency.Status != WorkflowNodeStatus.Completed)
                return false;
        }

        return await ValidatePrerequisitesAsync(context);
    }

    protected virtual Task<bool> ValidatePrerequisitesAsync(WorkflowContext context)
    {
        return Task.FromResult(true);
    }
}

public class WorkflowContext
{
    private readonly ConcurrentDictionary<string, object> _sharedState = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowContext> _logger;

    public string WorkflowId { get; }
    public DateTime StartTime { get; }
    public WorkflowMetrics Metrics { get; }
    public IResourcePool ResourcePool { get; }

    public WorkflowContext(
        string workflowId,
        IServiceProvider serviceProvider,
        IResourcePool resourcePool)
    {
        WorkflowId = workflowId;
        _serviceProvider = serviceProvider;
        ResourcePool = resourcePool;
        _logger = serviceProvider.GetRequiredService<ILogger<WorkflowContext>>();
        StartTime = DateTime.UtcNow;
        Metrics = new WorkflowMetrics();
    }

    public T GetSharedValue<T>(string key, T defaultValue = default)
    {
        return _sharedState.TryGetValue(key, out var value)
            ? (T)value
            : defaultValue;
    }

    public void SetSharedValue<T>(string key, T value)
    {
        _sharedState[key] = value;
        _logger.LogDebug("Workflow {WorkflowId}: Set shared value {Key}",
            WorkflowId, key);
    }

    public T GetService<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}
```

### Building Specialized Workflow Nodes

With the core abstractions in place, we can implement specialized nodes that handle specific image processing tasks.
These nodes encapsulate both the processing logic and resource management requirements:

```csharp
public class ImageLoadNode : WorkflowNode
{
    private readonly string _inputPath;
    private readonly ImageLoadOptions _options;

    public ImageLoadNode(string id, string inputPath, ImageLoadOptions options = null)
        : base(id)
    {
        _inputPath = inputPath;
        _options = options ?? new ImageLoadOptions();
        Name = $"Load: {Path.GetFileName(inputPath)}";
    }

    public override async Task<WorkflowResult> ExecuteAsync(
        WorkflowContext context,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Acquire memory from the resource pool
            var memoryToken = await context.ResourcePool
                .AcquireMemoryAsync(_options.EstimatedMemoryUsage, cancellationToken);

            using (memoryToken)
            {
                // Load image with optimized settings
                var image = await LoadImageOptimizedAsync(_inputPath, _options, cancellationToken);

                // Store in context for downstream nodes
                context.SetSharedValue($"image_{Id}", image);
                context.SetSharedValue($"image_metadata_{Id}", image.Metadata);

                // Update metrics
                context.Metrics.RecordNodeExecution(Id, stopwatch.Elapsed);
                context.Metrics.RecordMemoryUsage(Id, image.CalculateMemoryFootprint());

                return WorkflowResult.Success($"Loaded image: {image.Width}x{image.Height}");
            }
        }
        catch (Exception ex)
        {
            context.Metrics.RecordNodeFailure(Id, ex);
            return WorkflowResult.Failure($"Failed to load image: {ex.Message}", ex);
        }
    }

    private async Task<Image<Rgba32>> LoadImageOptimizedAsync(
        string path,
        ImageLoadOptions options,
        CancellationToken cancellationToken)
    {
        var configuration = Configuration.Default.Clone();
        configuration.PreferContiguousImageBuffers = true;

        if (options.MaxDimensions.HasValue)
        {
            configuration.StreamProcessingBufferSize =
                Math.Min(options.MaxDimensions.Value.Width * 4, 81920);
        }

        using var stream = File.OpenRead(path);
        var image = await Image.LoadAsync<Rgba32>(configuration, stream, cancellationToken);

        // Apply initial transformations if specified
        if (options.AutoOrient && image.Metadata.ExifProfile != null)
        {
            image.Mutate(x => x.AutoOrient());
        }

        return image;
    }
}

public class ParallelProcessingNode : WorkflowNode
{
    private readonly Func<Image<Rgba32>, int, Task<Image<Rgba32>>> _processor;
    private readonly ParallelOptions _parallelOptions;

    public ParallelProcessingNode(
        string id,
        Func<Image<Rgba32>, int, Task<Image<Rgba32>>> processor,
        int maxDegreeOfParallelism = -1) : base(id)
    {
        _processor = processor;
        _parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism > 0
                ? maxDegreeOfParallelism
                : Environment.ProcessorCount
        };
    }

    public override async Task<WorkflowResult> ExecuteAsync(
        WorkflowContext context,
        CancellationToken cancellationToken)
    {
        var inputImages = GetInputImages(context);
        var processedImages = new ConcurrentBag<(int index, Image<Rgba32> image)>();

        // Process images in parallel with resource constraints
        await Parallel.ForEachAsync(
            inputImages.Select((img, idx) => (img, idx)),
            _parallelOptions,
            async (item, ct) =>
            {
                var memoryToken = await context.ResourcePool
                    .AcquireMemoryAsync(item.img.CalculateMemoryFootprint() * 2, ct);

                using (memoryToken)
                {
                    var processed = await _processor(item.img, item.idx);
                    processedImages.Add((item.idx, processed));
                }
            });

        // Store results maintaining order
        var orderedResults = processedImages
            .OrderBy(x => x.index)
            .Select(x => x.image)
            .ToList();

        context.SetSharedValue($"processed_images_{Id}", orderedResults);

        return WorkflowResult.Success($"Processed {orderedResults.Count} images");
    }
}
```

### Workflow Execution Engine

The execution engine coordinates the workflow nodes, managing dependencies and ensuring optimal resource utilization:

```csharp
public class WorkflowEngine
{
    private readonly ILogger<WorkflowEngine> _logger;
    private readonly IResourcePool _resourcePool;
    private readonly WorkflowMetricsCollector _metricsCollector;

    public WorkflowEngine(
        ILogger<WorkflowEngine> logger,
        IResourcePool resourcePool,
        WorkflowMetricsCollector metricsCollector)
    {
        _logger = logger;
        _resourcePool = resourcePool;
        _metricsCollector = metricsCollector;
    }

    public async Task<WorkflowExecutionResult> ExecuteAsync(
        Workflow workflow,
        CancellationToken cancellationToken = default)
    {
        var context = new WorkflowContext(
            Guid.NewGuid().ToString(),
            workflow.ServiceProvider,
            _resourcePool);

        var executionPlan = BuildExecutionPlan(workflow);
        var completedNodes = new HashSet<string>();
        var failedNodes = new List<(WorkflowNode node, WorkflowResult result)>();

        _logger.LogInformation("Starting workflow {WorkflowId} with {NodeCount} nodes",
            context.WorkflowId, executionPlan.Count);

        while (completedNodes.Count < executionPlan.Count && !cancellationToken.IsCancellationRequested)
        {
            // Find nodes ready for execution
            var readyNodes = await GetReadyNodesAsync(
                executionPlan,
                completedNodes,
                context);

            if (!readyNodes.Any())
            {
                if (failedNodes.Any())
                {
                    // No progress possible due to failures
                    break;
                }

                // Possible circular dependency
                throw new InvalidOperationException(
                    "No nodes ready for execution - possible circular dependency");
            }

            // Execute ready nodes in parallel
            var executionTasks = readyNodes
                .Select(node => ExecuteNodeAsync(node, context, cancellationToken))
                .ToList();

            var results = await Task.WhenAll(executionTasks);

            // Process results
            foreach (var (node, result) in readyNodes.Zip(results))
            {
                if (result.Success)
                {
                    completedNodes.Add(node.Id);
                    _logger.LogInformation("Node {NodeId} completed successfully", node.Id);
                }
                else
                {
                    failedNodes.Add((node, result));
                    _logger.LogError("Node {NodeId} failed: {Error}",
                        node.Id, result.ErrorMessage);
                }
            }
        }

        // Generate execution summary
        return new WorkflowExecutionResult
        {
            WorkflowId = context.WorkflowId,
            Success = failedNodes.Count == 0,
            CompletedNodes = completedNodes.Count,
            FailedNodes = failedNodes.Select(f => new FailedNodeInfo
            {
                NodeId = f.node.Id,
                NodeName = f.node.Name,
                ErrorMessage = f.result.ErrorMessage,
                Exception = f.result.Exception
            }).ToList(),
            Metrics = context.Metrics,
            Duration = DateTime.UtcNow - context.StartTime
        };
    }

    private async Task<List<WorkflowNode>> GetReadyNodesAsync(
        List<WorkflowNode> allNodes,
        HashSet<string> completedNodes,
        WorkflowContext context)
    {
        var readyNodes = new List<WorkflowNode>();

        foreach (var node in allNodes)
        {
            if (completedNodes.Contains(node.Id))
                continue;

            if (await node.CanExecuteAsync(context))
            {
                readyNodes.Add(node);
            }
        }

        return readyNodes;
    }

    private async Task<WorkflowResult> ExecuteNodeAsync(
        WorkflowNode node,
        WorkflowContext context,
        CancellationToken cancellationToken)
    {
        using var activity = Activity.StartActivity($"WorkflowNode.{node.Name}");
        activity?.SetTag("node.id", node.Id);
        activity?.SetTag("workflow.id", context.WorkflowId);

        try
        {
            node.Status = WorkflowNodeStatus.Running;
            var result = await node.ExecuteAsync(context, cancellationToken);

            node.Status = result.Success
                ? WorkflowNodeStatus.Completed
                : WorkflowNodeStatus.Failed;

            _metricsCollector.RecordNodeExecution(node, result, context);

            return result;
        }
        catch (Exception ex)
        {
            node.Status = WorkflowNodeStatus.Failed;
            _logger.LogError(ex, "Unhandled exception in node {NodeId}", node.Id);

            return WorkflowResult.Failure(
                $"Unhandled exception: {ex.Message}",
                ex);
        }
    }
}
```

## 17.2 Resource Pool Management

### Understanding Resource Constraints in Batch Processing

Resource pool management represents one of the most critical aspects of batch processing systems, particularly when
dealing with high-resolution images that can consume gigabytes of memory. The challenge extends beyond simple memory
allocation to encompass GPU resources, thread pool management, file handle limits, and network bandwidth allocation.
Effective resource management must balance throughput optimization with system stability, preventing resource exhaustion
while maximizing hardware utilization.

The complexity of resource management in graphics processing stems from the variable nature of image data. A batch might
contain thumbnails requiring kilobytes alongside panoramic images demanding gigabytes. Processing operations themselves
vary dramatically in resource consumption - a simple crop operation requires minimal additional memory, while a complex
filter might need multiple intermediate buffers. This variability demands dynamic resource allocation strategies that
adapt to changing workload characteristics.

### Implementing a Comprehensive Resource Pool

Our resource pool implementation provides fine-grained control over multiple resource types while maintaining high
performance through lock-free algorithms where possible:

```csharp
public interface IResourcePool
{
    Task<IResourceToken> AcquireMemoryAsync(long bytes, CancellationToken cancellationToken);
    Task<IResourceToken> AcquireThreadAsync(CancellationToken cancellationToken);
    Task<IResourceToken> AcquireGpuResourceAsync(GpuResourceType type, CancellationToken cancellationToken);
    Task<IResourceToken> AcquireCompositeAsync(ResourceRequirements requirements, CancellationToken cancellationToken);

    ResourcePoolStatus GetStatus();
    void UpdateConfiguration(ResourcePoolConfiguration configuration);
}

public class AdvancedResourcePool : IResourcePool
{
    private readonly MemoryResourceManager _memoryManager;
    private readonly ThreadResourceManager _threadManager;
    private readonly GpuResourceManager _gpuManager;
    private readonly ResourcePoolMetrics _metrics;
    private readonly ILogger<AdvancedResourcePool> _logger;

    private volatile ResourcePoolConfiguration _configuration;

    public AdvancedResourcePool(
        ResourcePoolConfiguration configuration,
        ILogger<AdvancedResourcePool> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _metrics = new ResourcePoolMetrics();

        _memoryManager = new MemoryResourceManager(configuration.Memory, _metrics);
        _threadManager = new ThreadResourceManager(configuration.Threading, _metrics);
        _gpuManager = new GpuResourceManager(configuration.Gpu, _metrics);
    }

    public async Task<IResourceToken> AcquireMemoryAsync(
        long bytes,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var token = await _memoryManager.AcquireAsync(bytes, cancellationToken);
            _metrics.RecordAcquisition(ResourceType.Memory, stopwatch.Elapsed, true);

            return token;
        }
        catch (OperationCanceledException)
        {
            _metrics.RecordAcquisition(ResourceType.Memory, stopwatch.Elapsed, false);
            throw;
        }
    }

    public async Task<IResourceToken> AcquireCompositeAsync(
        ResourceRequirements requirements,
        CancellationToken cancellationToken)
    {
        var acquiredTokens = new List<IResourceToken>();

        try
        {
            // Acquire resources in order of scarcity
            var orderedRequirements = OrderByScarcity(requirements);

            foreach (var requirement in orderedRequirements)
            {
                var token = requirement.Type switch
                {
                    ResourceType.Memory => await AcquireMemoryAsync(
                        requirement.Amount, cancellationToken),
                    ResourceType.Thread => await AcquireThreadAsync(
                        cancellationToken),
                    ResourceType.Gpu => await AcquireGpuResourceAsync(
                        requirement.GpuType.Value, cancellationToken),
                    _ => throw new ArgumentException($"Unknown resource type: {requirement.Type}")
                };

                acquiredTokens.Add(token);
            }

            return new CompositeResourceToken(acquiredTokens);
        }
        catch
        {
            // Release any acquired resources on failure
            foreach (var token in acquiredTokens)
            {
                token.Dispose();
            }
            throw;
        }
    }

    private IEnumerable<ResourceRequirement> OrderByScarcity(ResourceRequirements requirements)
    {
        return requirements.Requirements
            .OrderByDescending(r => GetResourceScarcity(r.Type))
            .ToList();
    }

    private double GetResourceScarcity(ResourceType type)
    {
        return type switch
        {
            ResourceType.Memory => _memoryManager.GetUtilization(),
            ResourceType.Thread => _threadManager.GetUtilization(),
            ResourceType.Gpu => _gpuManager.GetUtilization(),
            _ => 0.0
        };
    }
}

public class MemoryResourceManager
{
    private readonly MemoryConfiguration _configuration;
    private readonly ResourcePoolMetrics _metrics;
    private readonly SemaphoreSlim _allocationSemaphore;
    private long _allocatedBytes;
    private readonly Channel<MemoryRequest> _requestQueue;

    public MemoryResourceManager(
        MemoryConfiguration configuration,
        ResourcePoolMetrics metrics)
    {
        _configuration = configuration;
        _metrics = metrics;
        _allocationSemaphore = new SemaphoreSlim(1, 1);

        // Unbounded channel for memory requests
        _requestQueue = Channel.CreateUnbounded<MemoryRequest>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        // Start the allocation processor
        _ = ProcessAllocationRequestsAsync();
    }

    public async Task<IResourceToken> AcquireAsync(
        long bytes,
        CancellationToken cancellationToken)
    {
        // Validate request
        if (bytes > _configuration.MaxAllocationSize)
        {
            throw new InvalidOperationException(
                $"Requested allocation {bytes} exceeds maximum {_configuration.MaxAllocationSize}");
        }

        // Fast path for small allocations
        if (bytes < _configuration.SmallAllocationThreshold)
        {
            return await FastAcquireAsync(bytes, cancellationToken);
        }

        // Queue larger allocations
        var request = new MemoryRequest
        {
            Bytes = bytes,
            CompletionSource = new TaskCompletionSource<IResourceToken>(),
            CancellationToken = cancellationToken
        };

        await _requestQueue.Writer.WriteAsync(request, cancellationToken);
        return await request.CompletionSource.Task;
    }

    private async Task<IResourceToken> FastAcquireAsync(
        long bytes,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var currentAllocated = Interlocked.Read(ref _allocatedBytes);

            if (currentAllocated + bytes <= _configuration.MaxMemoryBytes)
            {
                var newAllocated = Interlocked.Add(ref _allocatedBytes, bytes);

                if (newAllocated <= _configuration.MaxMemoryBytes)
                {
                    _metrics.UpdateMemoryUsage(newAllocated);
                    return new MemoryResourceToken(this, bytes);
                }

                // Allocation pushed us over limit, rollback
                Interlocked.Add(ref _allocatedBytes, -bytes);
            }

            // Wait with exponential backoff
            var delay = TimeSpan.FromMilliseconds(Math.Min(100 * Math.Pow(2, 3), 1000));
            await Task.Delay(delay, cancellationToken);
        }
    }

    private async Task ProcessAllocationRequestsAsync()
    {
        await foreach (var request in _requestQueue.Reader.ReadAllAsync())
        {
            try
            {
                if (request.CancellationToken.IsCancellationRequested)
                {
                    request.CompletionSource.SetCanceled();
                    continue;
                }

                await _allocationSemaphore.WaitAsync(request.CancellationToken);

                try
                {
                    // Wait for available memory
                    while (_allocatedBytes + request.Bytes > _configuration.MaxMemoryBytes)
                    {
                        await Task.Delay(100, request.CancellationToken);
                    }

                    // Allocate memory
                    _allocatedBytes += request.Bytes;
                    _metrics.UpdateMemoryUsage(_allocatedBytes);

                    var token = new MemoryResourceToken(this, request.Bytes);
                    request.CompletionSource.SetResult(token);
                }
                finally
                {
                    _allocationSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                request.CompletionSource.SetCanceled();
            }
            catch (Exception ex)
            {
                request.CompletionSource.SetException(ex);
            }
        }
    }

    internal void Release(long bytes)
    {
        var newAllocated = Interlocked.Add(ref _allocatedBytes, -bytes);
        _metrics.UpdateMemoryUsage(newAllocated);
    }

    public double GetUtilization()
    {
        return (double)_allocatedBytes / _configuration.MaxMemoryBytes;
    }
}
```

### Advanced Resource Management Strategies

Beyond basic allocation and deallocation, sophisticated resource management requires predictive algorithms and adaptive
strategies:

```csharp
public class PredictiveResourceManager
{
    private readonly IResourcePool _resourcePool;
    private readonly ResourcePredictionModel _predictionModel;
    private readonly ILogger<PredictiveResourceManager> _logger;

    public async Task<ResourceAllocationPlan> PlanBatchExecutionAsync(
        BatchProcessingJob job,
        CancellationToken cancellationToken)
    {
        var plan = new ResourceAllocationPlan();

        // Analyze job characteristics
        var jobProfile = await AnalyzeJobProfileAsync(job);

        // Predict resource requirements
        var predictions = _predictionModel.PredictResourceUsage(jobProfile);

        // Build execution stages with resource pre-allocation
        foreach (var stage in job.Stages)
        {
            var stageRequirements = predictions.GetStageRequirements(stage.Id);

            var allocation = new StageResourceAllocation
            {
                StageId = stage.Id,
                MemoryRequirement = stageRequirements.Memory,
                ThreadRequirement = stageRequirements.Threads,
                GpuRequirement = stageRequirements.Gpu,
                Priority = CalculateStagePriority(stage, jobProfile)
            };

            // Consider resource dependencies
            if (stage.ProducesIntermediateData)
            {
                allocation.MemoryRequirement *= 1.5; // Buffer for intermediate storage
            }

            plan.StageAllocations.Add(allocation);
        }

        // Optimize allocation order
        plan.OptimizeAllocationOrder();

        return plan;
    }

    private async Task<JobProfile> AnalyzeJobProfileAsync(BatchProcessingJob job)
    {
        var profile = new JobProfile
        {
            TotalImages = job.InputImages.Count,
            AverageImageSize = await EstimateAverageImageSizeAsync(job.InputImages),
            ProcessingComplexity = EstimateProcessingComplexity(job.Stages),
            ParallelismFactor = CalculateOptimalParallelism(job)
        };

        // Historical data integration
        var historicalData = await _predictionModel
            .GetHistoricalDataAsync(job.GetSignature());

        if (historicalData != null)
        {
            profile.HistoricalMemoryUsage = historicalData.AverageMemoryUsage;
            profile.HistoricalProcessingTime = historicalData.AverageProcessingTime;
        }

        return profile;
    }
}

public class ResourcePredictionModel
{
    private readonly IMLModel _mlModel;
    private readonly IHistoricalDataStore _dataStore;

    public ResourcePrediction PredictResourceUsage(JobProfile profile)
    {
        var features = ExtractFeatures(profile);
        var prediction = _mlModel.Predict(features);

        return new ResourcePrediction
        {
            BaseMemoryRequirement = prediction.Memory,
            MemoryVariance = prediction.MemoryVariance,
            ThreadingRecommendation = DetermineThreadingStrategy(prediction),
            GpuUtilization = prediction.GpuUtilization,
            EstimatedDuration = TimeSpan.FromSeconds(prediction.DurationSeconds),
            ConfidenceLevel = prediction.Confidence
        };
    }

    private MLFeatures ExtractFeatures(JobProfile profile)
    {
        return new MLFeatures
        {
            ImageCount = profile.TotalImages,
            AverageImageSizeMB = profile.AverageImageSize / (1024.0 * 1024.0),
            StageCount = profile.ProcessingComplexity.StageCount,
            ComplexityScore = profile.ProcessingComplexity.Score,
            HasGpuOperations = profile.ProcessingComplexity.RequiresGpu,
            HistoricalMemoryUsage = profile.HistoricalMemoryUsage ?? 0,
            TimeOfDay = DateTime.UtcNow.Hour,
            DayOfWeek = (int)DateTime.UtcNow.DayOfWeek
        };
    }
}
```

### Resource Pooling for Specialized Hardware

Graphics processing often requires specialized hardware resources that demand careful management:

```csharp
public class GpuResourceManager
{
    private readonly GpuConfiguration _configuration;
    private readonly Dictionary<int, GpuDevice> _devices;
    private readonly Channel<GpuAllocationRequest> _allocationQueue;

    public GpuResourceManager(GpuConfiguration configuration, ResourcePoolMetrics metrics)
    {
        _configuration = configuration;
        _devices = InitializeGpuDevices();

        _allocationQueue = Channel.CreateUnbounded<GpuAllocationRequest>(
            new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });

        // Start GPU allocation processors for each device
        foreach (var device in _devices.Values)
        {
            _ = ProcessGpuAllocationsAsync(device);
        }
    }

    private Dictionary<int, GpuDevice> InitializeGpuDevices()
    {
        var devices = new Dictionary<int, GpuDevice>();

        for (int i = 0; i < _configuration.DeviceCount; i++)
        {
            var device = new GpuDevice
            {
                Id = i,
                TotalMemory = _configuration.DeviceMemoryBytes[i],
                ComputeUnits = _configuration.DeviceComputeUnits[i],
                MemoryAllocator = new GpuMemoryAllocator(_configuration.DeviceMemoryBytes[i]),
                CommandQueues = InitializeCommandQueues(i),
                CurrentLoad = 0
            };

            devices[i] = device;
        }

        return devices;
    }

    public async Task<IResourceToken> AcquireGpuResourceAsync(
        GpuResourceType type,
        CancellationToken cancellationToken)
    {
        var request = new GpuAllocationRequest
        {
            Type = type,
            CompletionSource = new TaskCompletionSource<IResourceToken>(),
            CancellationToken = cancellationToken
        };

        // Select optimal device based on current load
        var targetDevice = SelectOptimalDevice(type);
        request.PreferredDeviceId = targetDevice.Id;

        await _allocationQueue.Writer.WriteAsync(request, cancellationToken);
        return await request.CompletionSource.Task;
    }

    private GpuDevice SelectOptimalDevice(GpuResourceType type)
    {
        // Load balancing across devices
        return type switch
        {
            GpuResourceType.Compute => _devices.Values
                .OrderBy(d => d.CurrentLoad)
                .ThenBy(d => d.ComputeQueueDepth)
                .First(),

            GpuResourceType.Memory => _devices.Values
                .OrderBy(d => d.MemoryAllocator.GetFragmentation())
                .ThenBy(d => d.MemoryAllocator.GetUtilization())
                .First(),

            GpuResourceType.Transfer => _devices.Values
                .OrderBy(d => d.TransferQueueDepth)
                .First(),

            _ => _devices.Values.OrderBy(d => d.CurrentLoad).First()
        };
    }

    private async Task ProcessGpuAllocationsAsync(GpuDevice device)
    {
        await foreach (var request in _allocationQueue.Reader.ReadAllAsync())
        {
            if (request.PreferredDeviceId != device.Id)
                continue;

            try
            {
                var token = request.Type switch
                {
                    GpuResourceType.Compute => await AllocateComputeResourceAsync(
                        device, request.CancellationToken),
                    GpuResourceType.Memory => await AllocateMemoryResourceAsync(
                        device, request.MemorySize, request.CancellationToken),
                    GpuResourceType.Transfer => await AllocateTransferResourceAsync(
                        device, request.CancellationToken),
                    _ => throw new ArgumentException($"Unknown GPU resource type: {request.Type}")
                };

                request.CompletionSource.SetResult(token);
            }
            catch (OperationCanceledException)
            {
                request.CompletionSource.SetCanceled();
            }
            catch (Exception ex)
            {
                request.CompletionSource.SetException(ex);
            }
        }
    }
}
```

## 17.3 Error Handling and Recovery

### Comprehensive Error Management in Batch Processing

Error handling in batch processing systems extends far beyond simple exception catching. Production systems must
differentiate between transient failures that merit retry and permanent failures requiring intervention. They must
maintain data consistency when processing fails midway through a batch, provide detailed diagnostics for
troubleshooting, and implement recovery strategies that minimize data loss and processing time. The architecture must
assume that failures will occur and design for resilience from the ground up.

The complexity of error handling in graphics processing stems from the multiple failure modes possible at each stage.
Input files might be corrupted, processing operations might exhaust memory, GPU operations might timeout, and output
operations might encounter disk space limitations. Each failure mode requires specific detection mechanisms, appropriate
retry strategies, and careful state management to enable recovery without data corruption or loss.

### Building a Resilient Error Handling Framework

Our error handling framework implements multiple layers of protection, from low-level operation retries to high-level
workflow recovery:

```csharp
public interface IErrorHandler
{
    Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        RetryPolicy policy,
        CancellationToken cancellationToken);

    Task<ErrorRecoveryPlan> AnalyzeFailureAsync(
        ProcessingFailure failure,
        WorkflowContext context);

    Task<bool> AttemptRecoveryAsync(
        ErrorRecoveryPlan plan,
        CancellationToken cancellationToken);
}

public class ComprehensiveErrorHandler : IErrorHandler
{
    private readonly ILogger<ComprehensiveErrorHandler> _logger;
    private readonly IErrorAnalyzer _errorAnalyzer;
    private readonly IRecoveryStrategies _recoveryStrategies;
    private readonly ErrorMetrics _metrics;

    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        RetryPolicy policy,
        CancellationToken cancellationToken)
    {
        var attempts = 0;
        var exceptions = new List<Exception>();
        var backoffMs = policy.InitialBackoffMs;

        while (attempts < policy.MaxAttempts)
        {
            attempts++;

            try
            {
                // Execute with timeout protection
                using var timeoutCts = CancellationTokenSource
                    .CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(policy.OperationTimeout);

                var result = await operation().ConfigureAwait(false);

                if (attempts > 1)
                {
                    _logger.LogInformation(
                        "Operation succeeded after {Attempts} attempts",
                        attempts);
                }

                return result;
            }
            catch (Exception ex) when (ShouldRetry(ex, policy))
            {
                exceptions.Add(ex);
                _metrics.RecordRetryableError(ex.GetType().Name);

                if (attempts >= policy.MaxAttempts)
                {
                    throw new AggregateException(
                        $"Operation failed after {attempts} attempts",
                        exceptions);
                }

                _logger.LogWarning(ex,
                    "Attempt {Attempt}/{MaxAttempts} failed, retrying in {BackoffMs}ms",
                    attempts, policy.MaxAttempts, backoffMs);

                await Task.Delay(backoffMs, cancellationToken);

                // Exponential backoff with jitter
                backoffMs = Math.Min(
                    (int)(backoffMs * policy.BackoffMultiplier * (1 + Random.Shared.NextDouble() * 0.1)),
                    policy.MaxBackoffMs);
            }
            catch (Exception ex)
            {
                _metrics.RecordNonRetryableError(ex.GetType().Name);
                throw new NonRetryableException(
                    "Operation failed with non-retryable error",
                    ex);
            }
        }

        throw new InvalidOperationException("Retry loop exited unexpectedly");
    }

    private bool ShouldRetry(Exception ex, RetryPolicy policy)
    {
        // Check explicit retry predicates
        if (policy.RetryPredicates?.Any(p => p(ex)) == true)
            return true;

        // Default retry conditions
        return ex switch
        {
            TaskCanceledException => false,
            OutOfMemoryException => false,
            AccessViolationException => false,
            StackOverflowException => false,
            ThreadAbortException => false,
            IOException ioEx when IsNonTransientIoError(ioEx) => false,
            HttpRequestException httpEx when IsNonTransientHttpError(httpEx) => false,
            _ => true
        };
    }

    public async Task<ErrorRecoveryPlan> AnalyzeFailureAsync(
        ProcessingFailure failure,
        WorkflowContext context)
    {
        var analysis = await _errorAnalyzer.AnalyzeAsync(failure);

        var plan = new ErrorRecoveryPlan
        {
            FailureId = failure.Id,
            FailureType = analysis.Type,
            Severity = analysis.Severity,
            ImpactedNodes = analysis.ImpactedNodes,
            RecoveryStrategies = new List<RecoveryStrategy>()
        };

        // Determine applicable recovery strategies
        foreach (var strategy in _recoveryStrategies.GetApplicableStrategies(analysis))
        {
            var viability = await strategy.AssessViabilityAsync(failure, context);

            if (viability.IsViable)
            {
                plan.RecoveryStrategies.Add(new RecoveryStrategy
                {
                    Type = strategy.Type,
                    Priority = viability.Priority,
                    EstimatedDataLoss = viability.EstimatedDataLoss,
                    EstimatedRecoveryTime = viability.EstimatedRecoveryTime,
                    Implementation = strategy
                });
            }
        }

        // Order strategies by priority and data preservation
        plan.RecoveryStrategies = plan.RecoveryStrategies
            .OrderByDescending(s => s.Priority)
            .ThenBy(s => s.EstimatedDataLoss)
            .ToList();

        return plan;
    }
}

public class CheckpointRecoveryStrategy : IRecoveryStrategy
{
    private readonly ICheckpointManager _checkpointManager;
    private readonly ILogger<CheckpointRecoveryStrategy> _logger;

    public RecoveryStrategyType Type => RecoveryStrategyType.CheckpointRestore;

    public async Task<RecoveryViability> AssessViabilityAsync(
        ProcessingFailure failure,
        WorkflowContext context)
    {
        // Check if we have a valid checkpoint
        var checkpoint = await _checkpointManager
            .GetNearestCheckpointAsync(context.WorkflowId, failure.FailedNode.Id);

        if (checkpoint == null)
        {
            return RecoveryViability.NotViable("No checkpoint available");
        }

        var timeSinceCheckpoint = DateTime.UtcNow - checkpoint.Timestamp;
        var estimatedDataLoss = CalculateDataLoss(checkpoint, failure, context);

        return new RecoveryViability
        {
            IsViable = true,
            Priority = CalculatePriority(timeSinceCheckpoint, estimatedDataLoss),
            EstimatedDataLoss = estimatedDataLoss,
            EstimatedRecoveryTime = EstimateRecoveryTime(checkpoint, context),
            RequiredResources = checkpoint.RequiredResources
        };
    }

    public async Task<RecoveryResult> ExecuteRecoveryAsync(
        ProcessingFailure failure,
        WorkflowContext context,
        CancellationToken cancellationToken)
    {
        var checkpoint = await _checkpointManager
            .GetNearestCheckpointAsync(context.WorkflowId, failure.FailedNode.Id);

        _logger.LogInformation(
            "Starting checkpoint recovery for workflow {WorkflowId} from checkpoint {CheckpointId}",
            context.WorkflowId, checkpoint.Id);

        try
        {
            // Restore workflow state
            await RestoreWorkflowStateAsync(checkpoint, context);

            // Restore intermediate data
            await RestoreIntermediateDataAsync(checkpoint, context);

            // Update workflow to resume from checkpoint
            var resumePoint = DetermineResumePoint(checkpoint, failure);
            context.SetSharedValue("resume_from_node", resumePoint.NodeId);
            context.SetSharedValue("resume_from_index", resumePoint.DataIndex);

            return RecoveryResult.Success(
                $"Recovered from checkpoint {checkpoint.Id}",
                dataLoss: checkpoint.ProcessedItems - resumePoint.DataIndex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Checkpoint recovery failed for workflow {WorkflowId}",
                context.WorkflowId);

            return RecoveryResult.Failure(
                $"Checkpoint recovery failed: {ex.Message}",
                ex);
        }
    }
}
```

### Implementing Sophisticated Retry Mechanisms

Beyond simple retry loops, production systems require intelligent retry mechanisms that adapt to failure patterns:

```csharp
public class AdaptiveRetryManager
{
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly IRetryPolicyProvider _policyProvider;
    private readonly RetryMetrics _metrics;

    public async Task<T> ExecuteWithAdaptiveRetryAsync<T>(
        string operationName,
        Func<Task<T>> operation,
        CancellationToken cancellationToken)
    {
        // Check circuit breaker state
        if (!_circuitBreaker.IsOperationAllowed(operationName))
        {
            throw new CircuitBreakerOpenException(
                $"Circuit breaker is open for operation: {operationName}");
        }

        // Get adaptive retry policy based on recent failure patterns
        var policy = _policyProvider.GetAdaptivePolicy(operationName, _metrics);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await ExecuteWithPolicyAsync(
                operation,
                policy,
                operationName,
                cancellationToken);

            // Record success
            _circuitBreaker.RecordSuccess(operationName);
            _metrics.RecordSuccess(operationName, stopwatch.Elapsed);

            return result;
        }
        catch (Exception ex)
        {
            // Record failure
            _circuitBreaker.RecordFailure(operationName);
            _metrics.RecordFailure(operationName, stopwatch.Elapsed, ex);

            throw;
        }
    }

    private async Task<T> ExecuteWithPolicyAsync<T>(
        Func<Task<T>> operation,
        AdaptiveRetryPolicy policy,
        string operationName,
        CancellationToken cancellationToken)
    {
        var attempt = 0;
        var delay = policy.InitialDelay;

        while (attempt < policy.MaxAttempts)
        {
            attempt++;

            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < policy.MaxAttempts &&
                                      policy.ShouldRetry(ex, attempt))
            {
                // Calculate adaptive delay based on failure patterns
                delay = policy.CalculateDelay(attempt, _metrics.GetRecentFailures(operationName));

                await Task.Delay(delay, cancellationToken);

                // Potentially degrade operation for subsequent attempts
                if (policy.EnableDegradation && attempt > policy.DegradationThreshold)
                {
                    operation = () => ExecuteDegradedOperationAsync<T>(
                        operationName,
                        operation);
                }
            }
        }

        throw new MaxRetriesExceededException(
            $"Operation {operationName} failed after {policy.MaxAttempts} attempts");
    }
}

public class CircuitBreaker : ICircuitBreaker
{
    private readonly CircuitBreakerConfiguration _configuration;
    private readonly ConcurrentDictionary<string, CircuitState> _circuits;

    public bool IsOperationAllowed(string operationName)
    {
        var state = _circuits.GetOrAdd(operationName, _ => new CircuitState());

        return state.State switch
        {
            BreakerState.Closed => true,
            BreakerState.Open => CheckIfShouldAttemptReset(state),
            BreakerState.HalfOpen => true,
            _ => false
        };
    }

    public void RecordSuccess(string operationName)
    {
        if (_circuits.TryGetValue(operationName, out var state))
        {
            lock (state.Lock)
            {
                state.ConsecutiveFailures = 0;

                if (state.State == BreakerState.HalfOpen)
                {
                    state.State = BreakerState.Closed;
                    state.LastStateChange = DateTime.UtcNow;
                }
            }
        }
    }

    public void RecordFailure(string operationName)
    {
        var state = _circuits.GetOrAdd(operationName, _ => new CircuitState());

        lock (state.Lock)
        {
            state.ConsecutiveFailures++;

            if (state.State == BreakerState.Closed &&
                state.ConsecutiveFailures >= _configuration.FailureThreshold)
            {
                state.State = BreakerState.Open;
                state.LastStateChange = DateTime.UtcNow;
                state.OpenUntil = DateTime.UtcNow.Add(_configuration.OpenDuration);
            }
            else if (state.State == BreakerState.HalfOpen)
            {
                state.State = BreakerState.Open;
                state.LastStateChange = DateTime.UtcNow;
                state.OpenUntil = DateTime.UtcNow.Add(_configuration.OpenDuration);
            }
        }
    }

    private bool CheckIfShouldAttemptReset(CircuitState state)
    {
        lock (state.Lock)
        {
            if (DateTime.UtcNow >= state.OpenUntil)
            {
                state.State = BreakerState.HalfOpen;
                state.LastStateChange = DateTime.UtcNow;
                return true;
            }

            return false;
        }
    }
}
```

### Error Recovery Through State Reconstruction

When failures occur during complex workflows, the ability to reconstruct state and resume processing becomes critical:

```csharp
public class StateRecoveryManager
{
    private readonly IStateStore _stateStore;
    private readonly IDataReconstructor _dataReconstructor;
    private readonly ILogger<StateRecoveryManager> _logger;

    public async Task<WorkflowState> RecoverStateAsync(
        string workflowId,
        DateTime? targetTime = null)
    {
        // Get the latest consistent state
        var baseState = await _stateStore.GetLatestConsistentStateAsync(workflowId);

        if (baseState == null)
        {
            throw new StateRecoveryException(
                $"No consistent state found for workflow {workflowId}");
        }

        // Apply transaction log to reconstruct state
        var transactions = await _stateStore.GetTransactionsAfterAsync(
            workflowId,
            baseState.Timestamp,
            targetTime ?? DateTime.UtcNow);

        var recoveredState = await ApplyTransactionsAsync(baseState, transactions);

        // Validate recovered state
        var validation = await ValidateRecoveredStateAsync(recoveredState);

        if (!validation.IsValid)
        {
            _logger.LogWarning(
                "Recovered state validation failed: {ValidationErrors}",
                string.Join(", ", validation.Errors));

            // Attempt partial recovery
            recoveredState = await AttemptPartialRecoveryAsync(
                baseState,
                transactions,
                validation);
        }

        return recoveredState;
    }

    private async Task<WorkflowState> ApplyTransactionsAsync(
        WorkflowState baseState,
        IEnumerable<StateTransaction> transactions)
    {
        var state = baseState.DeepClone();

        foreach (var transaction in transactions)
        {
            try
            {
                switch (transaction.Type)
                {
                    case TransactionType.NodeCompleted:
                        ApplyNodeCompletion(state, transaction);
                        break;

                    case TransactionType.DataProduced:
                        await ApplyDataProductionAsync(state, transaction);
                        break;

                    case TransactionType.ResourceAllocated:
                        ApplyResourceAllocation(state, transaction);
                        break;

                    case TransactionType.CheckpointCreated:
                        await ApplyCheckpointAsync(state, transaction);
                        break;

                    default:
                        _logger.LogWarning(
                            "Unknown transaction type: {TransactionType}",
                            transaction.Type);
                        break;
                }

                state.LastAppliedTransaction = transaction.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to apply transaction {TransactionId}",
                    transaction.Id);

                // Mark state as potentially inconsistent
                state.ConsistencyMarkers.Add(new InconsistencyMarker
                {
                    TransactionId = transaction.Id,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        return state;
    }

    private async Task<WorkflowState> AttemptPartialRecoveryAsync(
        WorkflowState baseState,
        IEnumerable<StateTransaction> transactions,
        StateValidation validation)
    {
        _logger.LogInformation(
            "Attempting partial state recovery for workflow {WorkflowId}",
            baseState.WorkflowId);

        var partialState = baseState.DeepClone();
        var recoveryPlan = BuildPartialRecoveryPlan(validation);

        foreach (var step in recoveryPlan.Steps)
        {
            try
            {
                switch (step.Type)
                {
                    case RecoveryStepType.ReconstructData:
                        var data = await _dataReconstructor
                            .ReconstructDataAsync(step.DataIdentifier);
                        partialState.SetData(step.DataIdentifier, data);
                        break;

                    case RecoveryStepType.SkipNode:
                        partialState.MarkNodeSkipped(step.NodeId, step.Reason);
                        break;

                    case RecoveryStepType.UseDefault:
                        partialState.SetData(
                            step.DataIdentifier,
                            step.DefaultValue);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Partial recovery step failed: {StepType} for {Identifier}",
                    step.Type, step.DataIdentifier ?? step.NodeId);
            }
        }

        partialState.IsPartiallyRecovered = true;
        partialState.RecoveryMetadata = recoveryPlan.ToMetadata();

        return partialState;
    }
}
```

## 17.4 Performance Monitoring

### Comprehensive Performance Metrics Collection

Performance monitoring in batch processing systems requires a multi-dimensional approach that captures not just
execution times but resource utilization, queue depths, error rates, and business metrics. The monitoring infrastructure
must operate with minimal overhead while providing the granularity necessary for performance optimization and capacity
planning. Modern observability practices demand that metrics be correlated with traces and logs to provide complete
system visibility.

The challenge in monitoring graphics processing workloads lies in the high data volumes and the need for fine-grained
metrics without impacting performance. A single batch job might process thousands of images, each requiring multiple
operations that could benefit from instrumentation. The monitoring system must intelligently sample and aggregate data
to provide meaningful insights without overwhelming storage or analysis systems.

### Building a High-Performance Monitoring Infrastructure

Our monitoring implementation leverages .NET 9.0's improved diagnostics APIs and integrates with industry-standard
observability platforms:

```csharp
public interface IPerformanceMonitor
{
    void RecordOperation(string operation, double duration, Dictionary<string, object> tags = null);
    void RecordThroughput(string metric, double value, string unit);
    void RecordResourceUsage(ResourceUsageSnapshot snapshot);
    IDisposable BeginOperation(string operation, Dictionary<string, object> tags = null);
    Task<PerformanceReport> GenerateReportAsync(TimeSpan period);
}

public class HighPerformanceMonitor : IPerformanceMonitor
{
    private readonly IMetricsCollector _metricsCollector;
    private readonly ITracer _tracer;
    private readonly Channel<MetricEvent> _metricsChannel;
    private readonly PerformanceCounterManager _counterManager;

    public HighPerformanceMonitor(
        IMetricsCollector metricsCollector,
        ITracer tracer,
        MonitoringConfiguration configuration)
    {
        _metricsCollector = metricsCollector;
        _tracer = tracer;

        // High-performance metrics channel
        _metricsChannel = Channel.CreateUnbounded<MetricEvent>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });

        _counterManager = new PerformanceCounterManager(configuration);

        // Start background metrics processor
        _ = ProcessMetricsAsync();
    }

    public void RecordOperation(
        string operation,
        double duration,
        Dictionary<string, object> tags = null)
    {
        var @event = new MetricEvent
        {
            Type = MetricEventType.Operation,
            Name = operation,
            Value = duration,
            Tags = tags ?? new Dictionary<string, object>(),
            Timestamp = DateTime.UtcNow
        };

        // Non-blocking write to channel
        if (!_metricsChannel.Writer.TryWrite(@event))
        {
            // Channel full, increment dropped metric counter
            Interlocked.Increment(ref _droppedMetrics);
        }
    }

    public IDisposable BeginOperation(
        string operation,
        Dictionary<string, object> tags = null)
    {
        // Create activity for distributed tracing
        var activity = Activity.StartActivity(operation, ActivityKind.Internal);

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                activity?.SetTag(tag.Key, tag.Value);
            }
        }

        var stopwatch = Stopwatch.StartNew();

        return new OperationScope(this, operation, stopwatch, activity, tags);
    }

    private async Task ProcessMetricsAsync()
    {
        var buffer = new List<MetricEvent>(1000);
        var flushTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (await _metricsChannel.Reader.WaitToReadAsync())
        {
            // Batch metrics for efficient processing
            while (_metricsChannel.Reader.TryRead(out var metric))
            {
                buffer.Add(metric);

                if (buffer.Count >= 1000)
                {
                    await FlushMetricsAsync(buffer);
                    buffer.Clear();
                }
            }

            // Flush on timer
            if (await flushTimer.WaitForNextTickAsync())
            {
                if (buffer.Count > 0)
                {
                    await FlushMetricsAsync(buffer);
                    buffer.Clear();
                }
            }
        }
    }

    private async Task FlushMetricsAsync(List<MetricEvent> metrics)
    {
        // Group metrics by type and name for aggregation
        var grouped = metrics
            .GroupBy(m => new { m.Type, m.Name })
            .Select(g => new AggregatedMetric
            {
                Type = g.Key.Type,
                Name = g.Key.Name,
                Count = g.Count(),
                Sum = g.Sum(m => m.Value),
                Min = g.Min(m => m.Value),
                Max = g.Max(m => m.Value),
                Average = g.Average(m => m.Value),
                Percentiles = CalculatePercentiles(g.Select(m => m.Value)),
                Tags = g.First().Tags
            });

        // Send to metrics collector
        foreach (var metric in grouped)
        {
            await _metricsCollector.RecordAsync(metric);
        }
    }
}

public class ResourceUsageMonitor
{
    private readonly Process _currentProcess;
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memoryCounter;
    private readonly IGpuMonitor _gpuMonitor;
    private readonly Timer _samplingTimer;

    public ResourceUsageMonitor(IGpuMonitor gpuMonitor)
    {
        _currentProcess = Process.GetCurrentProcess();
        _gpuMonitor = gpuMonitor;

        // Initialize performance counters
        _cpuCounter = new PerformanceCounter(
            "Process",
            "% Processor Time",
            _currentProcess.ProcessName);

        _memoryCounter = new PerformanceCounter(
            "Process",
            "Working Set - Private",
            _currentProcess.ProcessName);

        // Start sampling timer
        _samplingTimer = new Timer(
            SampleResourceUsage,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(1));
    }

    private async void SampleResourceUsage(object state)
    {
        try
        {
            var snapshot = new ResourceUsageSnapshot
            {
                Timestamp = DateTime.UtcNow,
                ProcessId = _currentProcess.Id,

                // CPU metrics
                CpuUsagePercent = _cpuCounter.NextValue(),
                ThreadCount = _currentProcess.Threads.Count,

                // Memory metrics
                WorkingSetBytes = _currentProcess.WorkingSet64,
                PrivateBytesBytes = _currentProcess.PrivateMemorySize64,
                ManagedMemoryBytes = GC.GetTotalMemory(false),
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),

                // GPU metrics
                GpuMetrics = await _gpuMonitor.GetCurrentMetricsAsync()
            };

            // Check for resource pressure
            if (snapshot.CpuUsagePercent > 90)
            {
                OnHighCpuDetected(snapshot);
            }

            if (snapshot.ManagedMemoryBytes > _memoryThreshold)
            {
                OnHighMemoryDetected(snapshot);
            }

            // Record snapshot
            await _metricsCollector.RecordResourceUsageAsync(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sample resource usage");
        }
    }
}
```

### Advanced Performance Analytics

Beyond basic metrics collection, sophisticated analytics provide insights into performance patterns and optimization
opportunities:

```csharp
public class PerformanceAnalyzer
{
    private readonly IMetricsRepository _metricsRepository;
    private readonly IStatisticalAnalyzer _statisticalAnalyzer;
    private readonly IMachineLearning _mlAnalyzer;

    public async Task<PerformanceAnalysis> AnalyzeWorkflowPerformanceAsync(
        string workflowId,
        TimeSpan analysisWindow)
    {
        var metrics = await _metricsRepository.GetMetricsAsync(
            workflowId,
            DateTime.UtcNow - analysisWindow,
            DateTime.UtcNow);

        var analysis = new PerformanceAnalysis
        {
            WorkflowId = workflowId,
            AnalysisWindow = analysisWindow,
            TotalExecutions = metrics.Count(m => m.Type == MetricType.WorkflowCompleted)
        };

        // Analyze execution patterns
        analysis.ExecutionPatterns = AnalyzeExecutionPatterns(metrics);

        // Identify bottlenecks
        analysis.Bottlenecks = await IdentifyBottlenecksAsync(metrics);

        // Resource utilization analysis
        analysis.ResourceUtilization = AnalyzeResourceUtilization(metrics);

        // Predictive analysis
        analysis.Predictions = await _mlAnalyzer.PredictFuturePerformanceAsync(metrics);

        // Generate optimization recommendations
        analysis.Recommendations = GenerateOptimizationRecommendations(analysis);

        return analysis;
    }

    private ExecutionPatterns AnalyzeExecutionPatterns(IEnumerable<PerformanceMetric> metrics)
    {
        var operationMetrics = metrics
            .Where(m => m.Type == MetricType.OperationDuration)
            .GroupBy(m => m.OperationName);

        var patterns = new ExecutionPatterns();

        foreach (var operation in operationMetrics)
        {
            var durations = operation.Select(m => m.Value).ToList();

            var pattern = new OperationPattern
            {
                OperationName = operation.Key,
                ExecutionCount = durations.Count,
                AverageDuration = durations.Average(),
                MedianDuration = CalculateMedian(durations),
                StandardDeviation = CalculateStandardDeviation(durations),
                Percentiles = new Dictionary<int, double>
                {
                    [50] = CalculatePercentile(durations, 50),
                    [90] = CalculatePercentile(durations, 90),
                    [95] = CalculatePercentile(durations, 95),
                    [99] = CalculatePercentile(durations, 99)
                }
            };

            // Detect anomalies
            pattern.Anomalies = DetectAnomalies(durations);

            // Analyze trend
            pattern.Trend = AnalyzeTrend(
                operation.OrderBy(m => m.Timestamp).ToList());

            patterns.Operations.Add(pattern);
        }

        return patterns;
    }

    private async Task<List<PerformanceBottleneck>> IdentifyBottlenecksAsync(
        IEnumerable<PerformanceMetric> metrics)
    {
        var bottlenecks = new List<PerformanceBottleneck>();

        // Analyze critical path
        var criticalPath = await AnalyzeCriticalPathAsync(metrics);

        foreach (var node in criticalPath.Nodes)
        {
            if (node.ContributionPercentage > 20) // Significant contribution
            {
                var bottleneck = new PerformanceBottleneck
                {
                    Type = BottleneckType.ProcessingTime,
                    Location = node.OperationName,
                    Impact = node.ContributionPercentage,
                    Description = $"{node.OperationName} accounts for {node.ContributionPercentage:F1}% of total execution time"
                };

                // Analyze why this operation is slow
                var analysis = await AnalyzeOperationPerformanceAsync(
                    node.OperationName,
                    metrics);

                bottleneck.Causes = analysis.IdentifiedCauses;
                bottleneck.Recommendations = analysis.Recommendations;

                bottlenecks.Add(bottleneck);
            }
        }

        // Analyze resource contention
        var contentionAnalysis = AnalyzeResourceContention(metrics);
        bottlenecks.AddRange(contentionAnalysis);

        return bottlenecks;
    }
}

public class RealTimePerformanceDashboard
{
    private readonly IHubContext<PerformanceHub> _hubContext;
    private readonly IMetricsAggregator _aggregator;
    private readonly Timer _broadcastTimer;

    public RealTimePerformanceDashboard(
        IHubContext<PerformanceHub> hubContext,
        IMetricsAggregator aggregator)
    {
        _hubContext = hubContext;
        _aggregator = aggregator;

        // Broadcast updates every second
        _broadcastTimer = new Timer(
            BroadcastMetrics,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(1));
    }

    private async void BroadcastMetrics(object state)
    {
        var snapshot = _aggregator.GetCurrentSnapshot();

        var dashboardData = new DashboardUpdate
        {
            Timestamp = DateTime.UtcNow,

            // Real-time metrics
            CurrentThroughput = snapshot.GetMetric("images.processed.rate"),
            ActiveWorkflows = snapshot.GetMetric("workflows.active"),
            QueueDepth = snapshot.GetMetric("queue.depth"),

            // Resource utilization
            CpuUsage = snapshot.GetMetric("cpu.usage.percent"),
            MemoryUsage = snapshot.GetMetric("memory.usage.bytes"),
            GpuUsage = snapshot.GetMetric("gpu.usage.percent"),

            // Error rates
            ErrorRate = snapshot.GetMetric("errors.rate"),
            RetryRate = snapshot.GetMetric("retries.rate"),

            // Performance percentiles
            ResponseTimeP50 = snapshot.GetMetric("response.time.p50"),
            ResponseTimeP99 = snapshot.GetMetric("response.time.p99"),

            // Active operations
            ActiveOperations = snapshot.GetActiveOperations()
                .Select(op => new ActiveOperationInfo
                {
                    Id = op.Id,
                    Type = op.Type,
                    StartTime = op.StartTime,
                    Duration = DateTime.UtcNow - op.StartTime,
                    Progress = op.Progress
                })
                .ToList()
        };

        // Broadcast to all connected clients
        await _hubContext.Clients.All.SendAsync(
            "UpdateDashboard",
            dashboardData);
    }
}
```

### Performance Optimization Recommendations

The monitoring system not only collects data but provides actionable recommendations for performance improvement:

```csharp
public class PerformanceOptimizationAdvisor
{
    private readonly IPerformanceAnalyzer _analyzer;
    private readonly IConfigurationManager _configManager;
    private readonly IOptimizationRules _rules;

    public async Task<OptimizationPlan> GenerateOptimizationPlanAsync(
        WorkflowConfiguration currentConfig,
        PerformanceAnalysis analysis)
    {
        var plan = new OptimizationPlan
        {
            CurrentPerformance = analysis.Summary,
            Recommendations = new List<OptimizationRecommendation>()
        };

        // Analyze each bottleneck
        foreach (var bottleneck in analysis.Bottlenecks)
        {
            var recommendations = await GenerateRecommendationsForBottleneckAsync(
                bottleneck,
                currentConfig,
                analysis);

            plan.Recommendations.AddRange(recommendations);
        }

        // Check for configuration optimizations
        var configOptimizations = AnalyzeConfigurationOptimizations(
            currentConfig,
            analysis);

        plan.Recommendations.AddRange(configOptimizations);

        // Prioritize recommendations by impact
        plan.Recommendations = plan.Recommendations
            .OrderByDescending(r => r.EstimatedImpact)
            .ThenBy(r => r.ImplementationEffort)
            .ToList();

        // Calculate overall potential improvement
        plan.PotentialImprovement = CalculatePotentialImprovement(plan.Recommendations);

        return plan;
    }

    private async Task<List<OptimizationRecommendation>> GenerateRecommendationsForBottleneckAsync(
        PerformanceBottleneck bottleneck,
        WorkflowConfiguration config,
        PerformanceAnalysis analysis)
    {
        var recommendations = new List<OptimizationRecommendation>();

        switch (bottleneck.Type)
        {
            case BottleneckType.ProcessingTime:
                if (bottleneck.Location.Contains("ImageResize"))
                {
                    recommendations.Add(new OptimizationRecommendation
                    {
                        Type = OptimizationType.Algorithm,
                        Title = "Switch to GPU-accelerated resizing",
                        Description = "Current CPU-based resizing is limiting throughput",
                        EstimatedImpact = 0.65, // 65% improvement
                        ImplementationEffort = ImplementationEffort.Medium,
                        ConfigurationChanges = new Dictionary<string, object>
                        {
                            ["ResizeProcessor"] = "GpuAcceleratedResize",
                            ["GpuDeviceIndex"] = 0
                        }
                    });
                }
                break;

            case BottleneckType.MemoryPressure:
                var currentBatchSize = config.BatchSize;
                var optimalBatchSize = CalculateOptimalBatchSize(analysis);

                if (optimalBatchSize < currentBatchSize * 0.8)
                {
                    recommendations.Add(new OptimizationRecommendation
                    {
                        Type = OptimizationType.Configuration,
                        Title = "Reduce batch size to alleviate memory pressure",
                        Description = $"Reduce batch size from {currentBatchSize} to {optimalBatchSize}",
                        EstimatedImpact = 0.25,
                        ImplementationEffort = ImplementationEffort.Trivial,
                        ConfigurationChanges = new Dictionary<string, object>
                        {
                            ["BatchSize"] = optimalBatchSize
                        }
                    });
                }
                break;

            case BottleneckType.IOWait:
                recommendations.Add(new OptimizationRecommendation
                {
                    Type = OptimizationType.Architecture,
                    Title = "Implement read-ahead caching",
                    Description = "Pre-fetch next batch while processing current batch",
                    EstimatedImpact = 0.30,
                    ImplementationEffort = ImplementationEffort.Low,
                    CodeChanges = GenerateReadAheadImplementation()
                });
                break;
        }

        return recommendations;
    }
}
```

## Conclusion

Throughout this chapter, we've explored the intricate architecture of batch processing systems for high-performance
graphics applications in .NET 9.0. The journey from workflow engine design through resource management, error handling,
and performance monitoring reveals the complexity inherent in building production-grade systems that can process
millions of images reliably and efficiently.

The workflow engine architecture we've developed demonstrates how modern .NET features enable sophisticated
orchestration patterns. By leveraging async/await patterns, channels for high-performance communication, and
activity-based tracing, we've created systems that can coordinate complex processing graphs while maintaining
observability and debuggability. The separation of workflow definition from execution enables both flexibility and
performance optimization.

Resource pool management emerges as a critical component for system stability and performance. Our implementation shows
how careful resource allocation, predictive modeling, and adaptive strategies prevent system overload while maximizing
throughput. The integration of memory, CPU, and GPU resource management into a unified framework enables holistic
optimization that considers all system constraints.

Error handling and recovery mechanisms transform batch processing from a fragile operation into a resilient system
capable of handling the inevitable failures in distributed processing. Through checkpoint-based recovery, intelligent
retry mechanisms, and comprehensive state reconstruction, we've built systems that minimize data loss and automatically
recover from transient failures while providing clear diagnostics for permanent issues.

Performance monitoring ties everything together, providing the visibility necessary for continuous optimization. The
combination of real-time metrics, historical analysis, and predictive modeling enables both reactive troubleshooting and
proactive optimization. The integration of machine learning for performance prediction and anomaly detection represents
the cutting edge of system observability.

Looking forward, the patterns and architectures presented in this chapter provide a foundation for building
next-generation batch processing systems. As hardware capabilities continue to evolve with faster GPUs, increased memory
bandwidth, and improved storage systems, these architectural patterns will adapt to leverage new capabilities while
maintaining the robustness and observability that production systems demand. The future of batch processing lies not
just in raw performance but in intelligent systems that self-optimize, predict failures, and adapt to changing workloads
automatically.
