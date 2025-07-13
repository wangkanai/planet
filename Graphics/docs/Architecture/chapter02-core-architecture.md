# Chapter 2: Core Architecture Patterns

## 2.1 Pipeline Architecture Fundamentals

Graphics pipelines form the backbone of high-performance image and graphics processing systems, enabling efficient
transformation of data through sequential stages while maximizing parallelization opportunities. In .NET 9.0, these
architectures benefit from significant performance improvements including enhanced SIMD operations, Native AOT
compilation, and improved JIT optimizations.

### Core Concepts and Purpose

A graphics pipeline represents a conceptual model describing the sequence of operations that transform input data into
processed output. Modern implementations leverage **three key parallelization strategies**: parallel processing of
multiple data elements simultaneously, pipelined execution where different stages operate concurrently on different
data, and GPU acceleration through specialized hardware. The pipeline architecture improves performance by enabling each
stage to process data independently while the next stage begins processing previously completed work, creating a
continuous flow of data transformation.

The relationship between CPU and GPU pipelines involves careful orchestration. The CPU submits commands to GPU queues
for asynchronous execution, manages shared memory spaces with coherency requirements, and uses synchronization
primitives like fences and semaphores to coordinate execution. This architecture allows the GPU to process commands
independently while the CPU prepares the next batch of work, maximizing hardware utilization.

### Pipeline Stages in Detail

**Vertex Processing** forms the first major stage, transforming vertices from model space to screen space through
model-view-projection transformations. Each vertex is processed independently, enabling massive parallelization. Modern
.NET graphics libraries like Silk.NET provide direct access to vertex shader capabilities, while higher-level libraries
like Win2D abstract these details for easier use.

**Geometry Processing** encompasses tessellation for increased detail, geometry shaders that can generate new
primitives, and clipping/culling operations to remove invisible geometry. This stage demonstrates the power of
breadth-first processing, where all primitives at each sub-stage are processed before moving to the next operation.

**Rasterization** converts geometric primitives into discrete fragments, determining pixel coverage and performing depth
testing. This highly parallel operation benefits from GPU acceleration and represents a critical performance bottleneck
in many graphics applications.

**Fragment/Pixel Processing** executes shaders on each fragment, performing texture sampling, lighting calculations, and
other per-pixel operations. ImageSharp leverages SIMD instructions to accelerate these operations on the CPU, achieving
performance improvements of up to 10x for vectorizable operations.

**Output Merging** performs final operations including depth and stencil testing, alpha blending, and frame buffer
updates. Modern APIs expose fine-grained control over these operations, as demonstrated in Vortice.Windows's Direct3D 12
wrapper:

```csharp
var pipelineDesc = new GraphicsPipelineStateDescription
{
    RootSignature = rootSignature,
    VertexShader = vertexShader,
    PixelShader = pixelShader,
    BlendState = new BlendStateDescription
    {
        RenderTarget = new[]
        {
            new RenderTargetBlendDescription
            {
                BlendEnable = true,
                SourceBlend = Blend.SourceAlpha,
                DestinationBlend = Blend.InverseSourceAlpha
            }
        }
    }
};
```

### Data Flow and Buffering Strategies

Pipeline architectures employ sophisticated buffering strategies to maintain smooth data flow. **Double and triple
buffering** eliminate visual artifacts by ensuring the display always has a complete frame while the next frame renders.
Ring buffers enable efficient memory reuse for streaming operations, particularly important for real-time graphics
applications processing continuous data streams.

Command buffers store GPU commands for later execution, enabling multi-threaded command generation and deterministic
execution order. This pattern is essential for modern graphics APIs like DirectX 12 and Vulkan, accessible through .NET
via Silk.NET:

```csharp
// Silk.NET command buffer recording
commandBuffer.Begin(new CommandBufferBeginInfo());
commandBuffer.CmdBindPipeline(PipelineBindPoint.Graphics, graphicsPipeline);
commandBuffer.CmdDraw(vertexCount, instanceCount, firstVertex, firstInstance);
commandBuffer.End();
```

### Synchronization Mechanisms

Graphics pipelines require careful synchronization to prevent race conditions and ensure correct operation order. *
*Fences** provide GPU-to-CPU synchronization, blocking CPU execution until GPU operations complete. **Barriers** handle
GPU-only synchronization, controlling execution order within the pipeline and ensuring memory visibility between stages.

**Semaphores** enable cross-queue synchronization without CPU involvement. Timeline semaphores, available in modern
graphics APIs, support complex dependency graphs through counter-based synchronization, enabling more sophisticated
pipeline orchestration than traditional binary semaphores.

### .NET 9.0 Performance Enhancements

.NET 9.0 introduces several enhancements that significantly benefit graphics pipelines. **Native AOT compilation**
reduces startup times by 15% and memory footprint by 30-40%, crucial for graphics applications that often have large
working sets. The improved JIT compiler with profile-guided optimization generates better code for rendering loops and
mathematical operations.

**Enhanced SIMD support** includes new Vector512<T> types and improved vectorization, enabling processing of 16
single-precision floats simultaneously on supported hardware. Graphics operations like color space conversions and
geometric transformations see substantial performance improvements from these enhancements.

## 2.2 Fluent vs. Imperative Design Patterns

The choice between fluent interfaces and imperative APIs significantly impacts both developer experience and runtime
performance in graphics programming. Understanding the trade-offs enables informed architectural decisions based on
specific requirements.

### Fluent Interface Patterns

Fluent interfaces leverage method chaining to create readable, domain-specific languages for graphics operations.
ImageSharp exemplifies sophisticated fluent API design with its Mutate and Clone patterns:

```csharp
// Fluent operation chain
using (var image = Image.Load<Rgba32>("input.jpg"))
{
    image.Mutate(ctx => ctx
        .Resize(new Size(800, 600))
        .Grayscale()
        .GaussianBlur(5.0f)
        .Rotate(RotateMode.Rotate90));

    await image.SaveAsync("output.jpg");
}
```

This pattern offers several advantages: operations read like natural language, IDE IntelliSense guides developers
through available operations, and the API can enforce valid operation sequences at compile time. ImageSharp's design
decision to separate Mutate (in-place modification) from Clone (creates new image) operations provides clarity about
side effects while maintaining the fluent interface benefits.

The builder pattern underlies many fluent implementations, accumulating state before executing operations. This approach
enables optimizations like operation reordering and batching, though it may increase memory usage for complex operation
chains.

### Imperative API Design

Imperative APIs provide explicit control over each operation, making state changes and execution order clear.
System.Drawing and low-level graphics APIs exemplify this approach:

```csharp
using (var graphics = Graphics.FromImage(bitmap))
{
    graphics.CompositingQuality = CompositingQuality.HighQuality;
    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

    using (var brush = new SolidBrush(Color.Blue))
    {
        graphics.FillRectangle(brush, 0, 0, 100, 100);
    }

    graphics.DrawImage(sourceImage, destRect, srcRect, GraphicsUnit.Pixel);
}
```

Imperative APIs excel in scenarios requiring fine-grained control, explicit resource management, and predictable
performance characteristics. They facilitate debugging by allowing breakpoints at each operation and make memory
allocation patterns explicit.

### Performance Analysis

Research demonstrates that modern JIT compilers effectively optimize method chaining, often inlining methods that return
`this`. Benchmark results show minimal performance differences between well-implemented fluent and imperative APIs:

```
Method          | Mean      | Error     | StdDev    | Allocated
----------------|-----------|-----------|-----------|----------
FluentChain     | 1.234 μs  | 0.012 μs  | 0.011 μs  | 248 B
ImperativeCall  | 1.198 μs  | 0.009 μs  | 0.008 μs  | 224 B
```

The slight overhead in fluent interfaces comes from potential temporary object creation and additional method calls,
though these are often eliminated through compiler optimizations. .NET 9.0's improved inlining and profile-guided
optimization further reduce this gap.

Memory allocation patterns differ more significantly. Fluent interfaces may create intermediate objects for protocol
enforcement, while imperative APIs typically have more predictable allocation patterns. However, short-lived objects
created by fluent interfaces are efficiently handled by the generational garbage collector.

### Choosing the Right Pattern

**Use fluent interfaces when**: building configuration APIs or DSLs, operations naturally chain together, readability
and developer experience are priorities, or the API benefits from compile-time validation of operation sequences.

**Choose imperative APIs for**: performance-critical hot paths where every allocation matters, low-level hardware
abstraction, scenarios requiring explicit resource management, or when debugging and profiling requirements demand
maximum transparency.

### Hybrid Approaches

Many successful libraries combine both patterns effectively. SkiaSharp primarily uses imperative APIs but provides
extension methods for common operations. This hybrid approach offers flexibility while maintaining performance:

```csharp
// Hybrid approach combining imperative base with fluent extensions
canvas.SaveState();
canvas.ApplyEffects(effects => effects
    .Blur(5.0f)
    .Brightness(0.8f)
    .Saturation(1.2f));
canvas.DrawBitmap(bitmap, destRect);
canvas.RestoreState();
```

## 2.3 Depth-First vs. Breadth-First Processing Strategies

The choice between depth-first and breadth-first processing strategies fundamentally impacts performance
characteristics, memory access patterns, and parallelization opportunities in graphics processing pipelines.

### Depth-First Processing Characteristics

Depth-first processing completes all operations on individual elements before moving to the next, maximizing cache
locality for algorithms with spatial locality. This approach excels when working sets fit within CPU cache hierarchies:

```csharp
// Depth-first convolution - process each pixel completely
for (int y = 1; y < height - 1; y++)
{
    for (int x = 1; x < width - 1; x++)
    {
        float sum = 0;
        // Complete convolution for this pixel
        for (int ky = -1; ky <= 1; ky++)
        {
            for (int kx = -1; kx <= 1; kx++)
            {
                sum += image[y + ky, x + kx] * kernel[ky + 1, kx + 1];
            }
        }
        result[y, x] = sum;
    }
}
```

**Cache efficiency benefits** emerge from processing related data together. Modern CPUs have 64-byte cache lines, so
accessing neighboring pixels sequentially maximizes cache utilization. L1 cache (typically 32KB) can hold small image
regions entirely, providing 1-3 cycle access latency compared to 100+ cycles for main memory access.

Depth-first strategies suit algorithms with recursive structures like flood fill operations, region growing, and
tree-based image processing. These algorithms naturally follow depth-first patterns and benefit from stack-based memory
allocation.

### Breadth-First Processing Advantages

Breadth-first processing applies each operation to all elements before proceeding to the next stage, enabling superior
vectorization and parallelization:

```csharp
// Breadth-first SIMD processing
public static void ProcessPixelsSIMD(ReadOnlySpan<float> input, Span<float> output)
{
    int vectorSize = Vector256<float>.Count;
    int i = 0;

    // Process 8 pixels simultaneously with AVX
    for (; i <= input.Length - vectorSize; i += vectorSize)
    {
        var vector = Vector256.Load(input[i..]);
        var result = vector * 0.5f + Vector256.Create(128f);
        result.Store(output[i..]);
    }

    // Handle remaining pixels
    for (; i < input.Length; i++)
    {
        output[i] = input[i] * 0.5f + 128f;
    }
}
```

**SIMD optimization potential** makes breadth-first processing attractive for modern hardware. .NET 9.0's Vector512<T>
support enables processing 16 single-precision floats simultaneously on AVX-512 hardware, providing up to 10x
performance improvements for suitable algorithms.

**GPU compatibility** strongly favors breadth-first approaches. GPU architectures execute 32-thread warps (NVIDIA) or
64-thread wavefronts (AMD) in lockstep, making breadth-first processing natural. ILGPU facilitates GPU programming in
.NET:

```csharp
static void GaussianBlurKernel(
    Index2D index,
    ArrayView2D<float, Stride2D.DenseX> input,
    ArrayView2D<float, Stride2D.DenseX> output,
    ArrayView<float> kernel)
{
    if (index.X < input.IntExtent.X && index.Y < input.IntExtent.Y)
    {
        float sum = 0.0f;
        for (int ky = -1; ky <= 1; ky++)
        {
            for (int kx = -1; kx <= 1; kx++)
            {
                var sampleX = Math.Clamp(index.X + kx, 0, input.IntExtent.X - 1);
                var sampleY = Math.Clamp(index.Y + ky, 0, input.IntExtent.Y - 1);
                sum += input[sampleX, sampleY] * kernel[(ky + 1) * 3 + (kx + 1)];
            }
        }
        output[index] = sum;
    }
}
```

### Memory Access Patterns and Performance

**Cache hierarchy impact** varies significantly between strategies. Depth-first processing achieves 95%+ L1 cache hit
rates for small working sets but suffers when data exceeds cache capacity. Breadth-first processing maintains consistent
memory bandwidth utilization but may experience more cache misses.

**Performance measurements** demonstrate the trade-offs:

- Depth-first convolution (3x3 kernel): 1,200 MB/s memory bandwidth, 98% L1 hit rate
- Breadth-first vectorized: 4,800 MB/s memory bandwidth, 75% L1 hit rate, 4x faster overall

**Parallel.For performance** strongly favors breadth-first processing:

```csharp
// Breadth-first parallel processing
Parallel.For(0, height, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, y =>
{
    var rowSpan = image.GetRowSpan(y);
    ProcessRowSIMD(rowSpan, resultBuffer.GetRowSpan(y));
});
```

Benchmarks show 3x speedup (793ms vs 2,317ms) with near-linear scaling up to core count.

### Choosing Processing Strategies

**Depth-first is optimal for**: algorithms with strong spatial locality, recursive processing patterns,
memory-constrained environments, or operations on small data regions that fit in cache.

**Breadth-first excels with**: vectorizable operations, large datasets requiring parallelization, GPU acceleration
scenarios, or algorithms with regular memory access patterns.

**Hybrid approaches** often provide the best performance by tiling large images into cache-friendly blocks, processing
each tile depth-first while coordinating tiles breadth-first:

```csharp
const int TileSize = 64; // Fits in L2 cache

Parallel.For(0, (height + TileSize - 1) / TileSize, tileY =>
{
    Parallel.For(0, (width + TileSize - 1) / TileSize, tileX =>
    {
        // Process tile depth-first
        ProcessTileDepthFirst(image, tileX * TileSize, tileY * TileSize, TileSize);
    });
});
```

## 2.4 Building Extensible Processing Pipelines

Creating extensible graphics processing pipelines requires careful architectural design to balance performance,
maintainability, and flexibility. Modern .NET provides powerful patterns and frameworks for building robust pipeline
systems.

### Core Design Patterns

**The Pipeline Pattern** forms the foundation, with each stage performing a specific transformation and passing results
to the next stage. Stages run independently, communicate through thread-safe channels, and maintain no shared state:

```csharp
public interface IPipelineStage<TIn, TOut>
{
    Task<TOut> ProcessAsync(TIn input, CancellationToken cancellationToken);
    bool CanProcess(TIn input);
    StageMetadata Metadata { get; }
}

public class Pipeline<TIn, TOut>
{
    private readonly List<IPipelineStage<object, object>> _stages;

    public async Task<TOut> ExecuteAsync(TIn input, CancellationToken cancellationToken)
    {
        object current = input;
        foreach (var stage in _stages)
        {
            if (!stage.CanProcess(current))
                throw new InvalidOperationException($"Stage {stage.Metadata.Name} cannot process input");

            current = await stage.ProcessAsync(current, cancellationToken);
        }
        return (TOut)current;
    }
}
```

**Strategy Pattern** enables runtime algorithm selection, crucial for filters supporting multiple implementations:

```csharp
public interface IResampler
{
    void Resample(ImageBuffer source, ImageBuffer destination);
}

public class ResampleStage : IPipelineStage<ImageData, ImageData>
{
    private readonly Dictionary<ResampleMode, IResampler> _resamplers = new()
    {
        [ResampleMode.Bilinear] = new BilinearResampler(),
        [ResampleMode.Bicubic] = new BicubicResampler(),
        [ResampleMode.Lanczos] = new LanczosResampler()
    };

    public async Task<ImageData> ProcessAsync(ImageData input, CancellationToken cancellationToken)
    {
        var resampler = _resamplers[input.Options.ResampleMode];
        return await Task.Run(() => resampler.Resample(input), cancellationToken);
    }
}
```

### Plugin Architecture with MEF

The Managed Extensibility Framework enables dynamic pipeline extension without recompilation:

```csharp
[Export(typeof(IGraphicsFilter))]
[ExportMetadata("Name", "GaussianBlur")]
[ExportMetadata("Version", "1.0")]
public class GaussianBlurFilter : IGraphicsFilter
{
    public async Task<ImageData> ApplyAsync(ImageData input, FilterParameters parameters)
    {
        var radius = parameters.GetValue<float>("Radius", 5.0f);
        // Implementation
    }
}

// Discovery and loading
public class FilterManager
{
    [ImportMany]
    private IEnumerable<Lazy<IGraphicsFilter, IFilterMetadata>> _filters;

    public void Initialize()
    {
        var catalog = new DirectoryCatalog(@".\Plugins");
        var container = new CompositionContainer(catalog);
        container.ComposeParts(this);
    }
}
```

### Async Pipeline Implementation

Modern pipelines leverage **System.Threading.Channels** for efficient async communication between stages:

```csharp
public class AsyncPipeline<T>
{
    public async Task RunAsync(
        ChannelReader<T> input,
        Func<T, Task<T>> processor,
        ChannelWriter<T> output,
        int maxConcurrency = 4,
        CancellationToken cancellationToken = default)
    {
        var semaphore = new SemaphoreSlim(maxConcurrency);
        var tasks = new List<Task>();

        await foreach (var item in input.ReadAllAsync(cancellationToken))
        {
            await semaphore.WaitAsync(cancellationToken);

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var result = await processor(item);
                    await output.WriteAsync(result, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
        output.Complete();
    }
}
```

**Backpressure handling** prevents memory exhaustion through bounded channels:

```csharp
var channel = Channel.CreateBounded<ImageData>(new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleWriter = false,
    SingleReader = false
});
```

### Dependency Injection Integration

Microsoft.Extensions.DependencyInjection provides flexible pipeline configuration:

```csharp
services.AddSingleton<IImageCache, DistributedImageCache>();
services.AddScoped<IPipelineExecutor, PipelineExecutor>();
services.AddTransient<IGraphicsFilter, ResizeFilter>();
services.AddTransient<IGraphicsFilter, WatermarkFilter>();

// Factory pattern for dynamic filter creation
services.AddSingleton<IFilterFactory>(provider =>
{
    return new FilterFactory(type =>
        (IGraphicsFilter)provider.GetRequiredService(type));
});

// Configuration-based pipeline
services.Configure<PipelineOptions>(configuration.GetSection("Pipeline"));
```

### Error Handling and Resilience

Robust pipelines implement comprehensive error handling strategies:

```csharp
public class ResilientPipelineStage<TIn, TOut> : IPipelineStage<TIn, TOut>
{
    private readonly IPipelineStage<TIn, TOut> _innerStage;
    private readonly IAsyncPolicy<TOut> _retryPolicy;

    public ResilientPipelineStage(IPipelineStage<TIn, TOut> innerStage)
    {
        _innerStage = innerStage;
        _retryPolicy = Policy<TOut>
            .Handle<TransientException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Logger.LogWarning($"Retry {retryCount} after {timespan}s");
                });
    }

    public async Task<TOut> ProcessAsync(TIn input, CancellationToken cancellationToken)
    {
        return await _retryPolicy.ExecuteAsync(
            async ct => await _innerStage.ProcessAsync(input, ct),
            cancellationToken);
    }
}
```

### Performance Monitoring with OpenTelemetry

Modern observability requires comprehensive instrumentation:

```csharp
public class InstrumentedPipelineStage<TIn, TOut> : IPipelineStage<TIn, TOut>
{
    private static readonly ActivitySource ActivitySource = new("GraphicsPipeline");
    private static readonly Histogram<double> ProcessingTime = Metrics
        .CreateHistogram<double>("pipeline.stage.duration", "ms");

    public async Task<TOut> ProcessAsync(TIn input, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("ProcessStage");
        activity?.SetTag("stage.name", Metadata.Name);
        activity?.SetTag("input.size", GetInputSize(input));

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await _innerStage.ProcessAsync(input, cancellationToken);

            ProcessingTime.Record(stopwatch.ElapsedMilliseconds,
                new KeyValuePair<string, object>("stage", Metadata.Name),
                new KeyValuePair<string, object>("success", true));

            return result;
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            ProcessingTime.Record(stopwatch.ElapsedMilliseconds,
                new KeyValuePair<string, object>("stage", Metadata.Name),
                new KeyValuePair<string, object>("success", false));
            throw;
        }
    }
}
```

### .NET 9.0 Enhancements for Pipelines

**.NET 9.0 introduces several features** that enhance pipeline implementation:

**Generic Math Interfaces** enable type-agnostic mathematical operations in filters:

```csharp
public class MathFilter<T> : IGraphicsFilter where T : INumber<T>
{
    public void Apply(Span<T> data, T multiplier)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = data[i] * multiplier;
        }
    }
}
```

**Native AOT compatibility** provides faster startup and reduced memory usage, critical for serverless image processing
scenarios. Source generators reduce runtime reflection overhead by generating pipeline configurations at compile time.

**Improved diagnostics APIs** enhance debugging capabilities for complex async pipelines, while new ActivitySource
features provide better integration with distributed tracing systems.

### Best Practices for Extensible Pipelines

1. **Design stages to be independently testable** with clear input/output contracts
2. **Implement comprehensive error handling** including retry policies and circuit breakers
3. **Use bounded channels** to prevent memory exhaustion under load
4. **Monitor performance metrics** from day one using OpenTelemetry
5. **Version your plugin interfaces** to maintain backward compatibility
6. **Document threading models** and concurrency expectations clearly
7. **Provide configuration schemas** for pipeline definitions
8. **Implement health checks** for each pipeline stage
9. **Use structured logging** with correlation IDs for request tracing
10. **Design for horizontal scaling** across multiple machines when needed

By following these patterns and leveraging .NET 9.0's enhancements, developers can build graphics processing pipelines
that are performant, maintainable, and ready for production workloads. The combination of async processing,
comprehensive error handling, and modern observability creates systems that scale effectively while remaining debuggable
and operationally excellent.
