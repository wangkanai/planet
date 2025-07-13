# Appendix B.1: Complete Pipeline Examples

## Introduction

This section provides complete, production-ready implementations of graphics processing pipelines that demonstrate best
practices covered throughout this book. Each example represents a real-world scenario with full error handling, resource
management, and performance optimizations. These implementations serve as templates that can be adapted for specific
requirements while maintaining architectural integrity and performance characteristics.

## High-Performance Image Processing Pipeline

### Overview

This comprehensive pipeline demonstrates a complete image processing system capable of handling multiple format inputs,
applying complex transformations, and producing optimized outputs. The implementation showcases proper resource
management, error handling, and performance optimization techniques suitable for production deployment.

```csharp
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Graphics.Pipeline.Examples
{
    /// <summary>
    /// High-performance image processing pipeline with automatic format detection,
    /// parallel processing, and intelligent resource management.
    /// </summary>
    public class ProductionImagePipeline : IAsyncDisposable
    {
        private readonly ILogger<ProductionImagePipeline> _logger;
        private readonly PipelineConfiguration _config;
        private readonly ConcurrentBag<MemoryPool<byte>> _memoryPools;
        private readonly Channel<ProcessingRequest> _inputChannel;
        private readonly Channel<ProcessingResult> _outputChannel;
        private readonly SemaphoreSlim _resourceThrottle;
        private readonly CancellationTokenSource _shutdownTokenSource;
        private readonly Task[] _processingTasks;
        private readonly PerformanceCounters _performanceCounters;

        // Performance tracking
        private long _processedImages;
        private long _totalProcessingTime;
        private long _failedOperations;

        public ProductionImagePipeline(
            ILogger<ProductionImagePipeline> logger,
            PipelineConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // Initialize resource pools
            _memoryPools = new ConcurrentBag<MemoryPool<byte>>();
            for (int i = 0; i < config.MemoryPoolCount; i++)
            {
                _memoryPools.Add(MemoryPool<byte>.Create());
            }

            // Configure channels with bounded capacity to prevent memory exhaustion
            var channelOptions = new BoundedChannelOptions(config.MaxQueueSize)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            };

            _inputChannel = Channel.CreateBounded<ProcessingRequest>(channelOptions);
            _outputChannel = Channel.CreateBounded<ProcessingResult>(channelOptions);

            // Resource throttling to prevent system overload
            _resourceThrottle = new SemaphoreSlim(
                config.MaxConcurrentOperations,
                config.MaxConcurrentOperations);

            _shutdownTokenSource = new CancellationTokenSource();
            _performanceCounters = new PerformanceCounters();

            // Start processing tasks based on available CPU cores
            int workerCount = Math.Min(
                Environment.ProcessorCount,
                config.MaxWorkerThreads);

            _processingTasks = new Task[workerCount];
            for (int i = 0; i < workerCount; i++)
            {
                int workerId = i;
                _processingTasks[i] = Task.Run(
                    () => ProcessingWorkerAsync(workerId, _shutdownTokenSource.Token));
            }

            _logger.LogInformation(
                "Pipeline initialized with {WorkerCount} workers, " +
                "{MemoryPools} memory pools, max queue size {QueueSize}",
                workerCount, config.MemoryPoolCount, config.MaxQueueSize);
        }

        /// <summary>
        /// Submit an image for processing through the pipeline
        /// </summary>
        public async Task<ProcessingResult> ProcessImageAsync(
            string inputPath,
            ProcessingOptions options,
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Input file not found: {inputPath}");
            }

            // Create processing request
            var request = new ProcessingRequest
            {
                Id = Guid.NewGuid(),
                InputPath = inputPath,
                Options = options,
                SubmittedAt = DateTime.UtcNow,
                CompletionSource = new TaskCompletionSource<ProcessingResult>()
            };

            // Submit to pipeline
            await _inputChannel.Writer.WriteAsync(request, cancellationToken);

            // Wait for completion
            return await request.CompletionSource.Task;
        }

        /// <summary>
        /// Core processing worker that handles queued requests
        /// </summary>
        private async Task ProcessingWorkerAsync(int workerId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Worker {WorkerId} started", workerId);

            try
            {
                await foreach (var request in _inputChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    await _resourceThrottle.WaitAsync(cancellationToken);

                    try
                    {
                        var result = await ProcessSingleImageAsync(request, workerId, cancellationToken);
                        request.CompletionSource.SetResult(result);

                        // Track metrics
                        Interlocked.Increment(ref _processedImages);
                        Interlocked.Add(ref _totalProcessingTime, result.ProcessingTime);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Worker {WorkerId} failed processing request {RequestId}",
                            workerId, request.Id);

                        Interlocked.Increment(ref _failedOperations);
                        request.CompletionSource.SetException(ex);
                    }
                    finally
                    {
                        _resourceThrottle.Release();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker {WorkerId} shutting down", workerId);
            }
        }

        /// <summary>
        /// Process a single image with full error handling and optimization
        /// </summary>
        private async Task<ProcessingResult> ProcessSingleImageAsync(
            ProcessingRequest request,
            int workerId,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new ProcessingResult
            {
                RequestId = request.Id,
                Success = false
            };

            // Get a memory pool for this operation
            if (!_memoryPools.TryTake(out var memoryPool))
            {
                memoryPool = MemoryPool<byte>.Create();
            }

            try
            {
                // Load image with automatic format detection
                using var image = await Image.LoadAsync<Rgba32>(
                    request.InputPath,
                    cancellationToken);

                _logger.LogDebug(
                    "Worker {WorkerId} loaded image {Path} ({Width}x{Height})",
                    workerId, request.InputPath, image.Width, image.Height);

                // Apply processing operations based on hardware capabilities
                if (Avx2.IsSupported && image.Width * image.Height > 1_000_000)
                {
                    await ProcessWithSIMDAsync(image, request.Options, memoryPool, cancellationToken);
                }
                else
                {
                    await ProcessWithStandardAsync(image, request.Options, cancellationToken);
                }

                // Generate output
                var outputPath = GenerateOutputPath(request.InputPath, request.Options);
                await SaveOptimizedAsync(image, outputPath, request.Options, cancellationToken);

                result.Success = true;
                result.OutputPath = outputPath;
                result.ProcessingTime = stopwatch.ElapsedMilliseconds;
                result.InputSize = new FileInfo(request.InputPath).Length;
                result.OutputSize = new FileInfo(outputPath).Length;

                _performanceCounters.RecordSuccess(result.ProcessingTime);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTime = stopwatch.ElapsedMilliseconds;

                _performanceCounters.RecordFailure();
                throw;
            }
            finally
            {
                _memoryPools.Add(memoryPool);
            }

            return result;
        }

        /// <summary>
        /// SIMD-optimized processing path for large images
        /// </summary>
        private async Task ProcessWithSIMDAsync(
            Image<Rgba32> image,
            ProcessingOptions options,
            MemoryPool<byte> memoryPool,
            CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                // Extract pixel data for direct manipulation
                if (!image.DangerousTryGetSinglePixelMemory(out var pixelMemory))
                {
                    throw new InvalidOperationException("Unable to get pixel memory");
                }

                var pixels = pixelMemory.Span;
                var vectorSize = Vector<float>.Count;

                // Rent working memory from pool
                var workingBuffer = memoryPool.Rent(pixels.Length * sizeof(float) * 4);
                try
                {
                    var floatSpan = MemoryMarshal.Cast<byte, float>(workingBuffer.Memory.Span);

                    // Convert to float for processing
                    ConvertToFloatVectorized(pixels, floatSpan);

                    // Apply operations
                    if (options.AdjustBrightness)
                    {
                        ApplyBrightnessVectorized(floatSpan, options.BrightnessValue, vectorSize);
                    }

                    if (options.AdjustContrast)
                    {
                        ApplyContrastVectorized(floatSpan, options.ContrastValue, vectorSize);
                    }

                    if (options.ApplyGaussianBlur)
                    {
                        ApplyGaussianBlurOptimized(floatSpan, image.Width, image.Height,
                            options.BlurRadius, memoryPool);
                    }

                    // Convert back to pixel format
                    ConvertFromFloatVectorized(floatSpan, pixels);
                }
                finally
                {
                    workingBuffer.Dispose();
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Vectorized brightness adjustment using SIMD operations
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ApplyBrightnessVectorized(
            Span<float> pixels,
            float brightness,
            int vectorSize)
        {
            var brightVector = new Vector<float>(brightness);
            var maxVector = new Vector<float>(255f);
            var minVector = Vector<float>.Zero;

            int i = 0;
            for (; i <= pixels.Length - vectorSize; i += vectorSize)
            {
                var pixel = new Vector<float>(pixels.Slice(i, vectorSize));
                pixel += brightVector;
                pixel = Vector.Min(Vector.Max(pixel, minVector), maxVector);
                pixel.CopyTo(pixels.Slice(i, vectorSize));
            }

            // Handle remaining pixels
            for (; i < pixels.Length; i++)
            {
                pixels[i] = Math.Clamp(pixels[i] + brightness, 0f, 255f);
            }
        }

        /// <summary>
        /// Optimized Gaussian blur implementation using separable filters
        /// </summary>
        private static void ApplyGaussianBlurOptimized(
            Span<float> pixels,
            int width,
            int height,
            float radius,
            MemoryPool<byte> memoryPool)
        {
            // Generate Gaussian kernel
            var kernelSize = (int)(radius * 2 + 1);
            var kernel = GenerateGaussianKernel(radius);

            // Rent temporary buffer for separable convolution
            using var tempBuffer = memoryPool.Rent(pixels.Length * sizeof(float));
            var tempSpan = MemoryMarshal.Cast<byte, float>(tempBuffer.Memory.Span);

            // Horizontal pass
            ApplyHorizontalConvolution(pixels, tempSpan, width, height, kernel);

            // Vertical pass
            ApplyVerticalConvolution(tempSpan, pixels, width, height, kernel);
        }

        /// <summary>
        /// Generate output path based on processing options
        /// </summary>
        private string GenerateOutputPath(string inputPath, ProcessingOptions options)
        {
            var directory = Path.GetDirectoryName(inputPath);
            var filename = Path.GetFileNameWithoutExtension(inputPath);
            var extension = Path.GetExtension(inputPath);

            var suffix = new StringBuilder();
            if (options.AdjustBrightness) suffix.Append($"_b{options.BrightnessValue}");
            if (options.AdjustContrast) suffix.Append($"_c{options.ContrastValue}");
            if (options.ApplyGaussianBlur) suffix.Append($"_blur{options.BlurRadius}");
            if (options.ResizeWidth > 0) suffix.Append($"_w{options.ResizeWidth}");

            var outputFilename = $"{filename}{suffix}{extension}";
            return Path.Combine(directory, "output", outputFilename);
        }

        /// <summary>
        /// Save image with format-specific optimizations
        /// </summary>
        private async Task SaveOptimizedAsync(
            Image<Rgba32> image,
            string outputPath,
            ProcessingOptions options,
            CancellationToken cancellationToken)
        {
            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            Directory.CreateDirectory(outputDir);

            // Apply final resize if requested
            if (options.ResizeWidth > 0 || options.ResizeHeight > 0)
            {
                var targetWidth = options.ResizeWidth > 0 ? options.ResizeWidth : image.Width;
                var targetHeight = options.ResizeHeight > 0 ? options.ResizeHeight : image.Height;

                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(targetWidth, targetHeight),
                    Mode = ResizeMode.Max,
                    Sampler = KnownResamplers.Lanczos3
                }));
            }

            // Save with appropriate encoder settings
            var extension = Path.GetExtension(outputPath).ToLowerInvariant();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    await image.SaveAsJpegAsync(outputPath, new JpegEncoder
                    {
                        Quality = options.JpegQuality,
                        Subsample = JpegSubsample.Ratio420,
                        ColorType = JpegColorType.YCbCrRatio420
                    }, cancellationToken);
                    break;

                case ".png":
                    await image.SaveAsPngAsync(outputPath, new PngEncoder
                    {
                        CompressionLevel = PngCompressionLevel.BestCompression,
                        ColorType = PngColorType.RgbWithAlpha,
                        FilterMethod = PngFilterMethod.Adaptive
                    }, cancellationToken);
                    break;

                case ".webp":
                    await image.SaveAsWebpAsync(outputPath, new WebpEncoder
                    {
                        Quality = options.WebPQuality,
                        Method = WebpEncodingMethod.BestQuality,
                        FileFormat = WebpFileFormatType.Lossless
                    }, cancellationToken);
                    break;

                default:
                    await image.SaveAsync(outputPath, cancellationToken);
                    break;
            }
        }

        /// <summary>
        /// Cleanup resources and wait for pipeline shutdown
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            _logger.LogInformation("Shutting down image processing pipeline");

            // Signal shutdown
            _inputChannel.Writer.TryComplete();
            _shutdownTokenSource.Cancel();

            // Wait for workers to complete
            await Task.WhenAll(_processingTasks);

            // Cleanup resources
            foreach (var pool in _memoryPools)
            {
                pool.Dispose();
            }

            _resourceThrottle?.Dispose();
            _shutdownTokenSource?.Dispose();

            // Log final statistics
            _logger.LogInformation(
                "Pipeline shutdown complete. Processed {Count} images, " +
                "average time {AvgTime}ms, failed {Failed}",
                _processedImages,
                _processedImages > 0 ? _totalProcessingTime / _processedImages : 0,
                _failedOperations);
        }

        // Supporting classes and structures
        public class PipelineConfiguration
        {
            public int MaxQueueSize { get; set; } = 1000;
            public int MaxConcurrentOperations { get; set; } = Environment.ProcessorCount;
            public int MaxWorkerThreads { get; set; } = Environment.ProcessorCount;
            public int MemoryPoolCount { get; set; } = Environment.ProcessorCount * 2;
        }

        public class ProcessingRequest
        {
            public Guid Id { get; set; }
            public string InputPath { get; set; }
            public ProcessingOptions Options { get; set; }
            public DateTime SubmittedAt { get; set; }
            public TaskCompletionSource<ProcessingResult> CompletionSource { get; set; }
        }

        public class ProcessingOptions
        {
            public bool AdjustBrightness { get; set; }
            public float BrightnessValue { get; set; }
            public bool AdjustContrast { get; set; }
            public float ContrastValue { get; set; }
            public bool ApplyGaussianBlur { get; set; }
            public float BlurRadius { get; set; }
            public int ResizeWidth { get; set; }
            public int ResizeHeight { get; set; }
            public int JpegQuality { get; set; } = 90;
            public int WebPQuality { get; set; } = 85;
        }

        public class ProcessingResult
        {
            public Guid RequestId { get; set; }
            public bool Success { get; set; }
            public string OutputPath { get; set; }
            public string ErrorMessage { get; set; }
            public long ProcessingTime { get; set; }
            public long InputSize { get; set; }
            public long OutputSize { get; set; }
        }

        private class PerformanceCounters
        {
            private long _successCount;
            private long _failureCount;
            private long _totalProcessingTime;

            public void RecordSuccess(long processingTime)
            {
                Interlocked.Increment(ref _successCount);
                Interlocked.Add(ref _totalProcessingTime, processingTime);
            }

            public void RecordFailure()
            {
                Interlocked.Increment(ref _failureCount);
            }
        }
    }
}
```

## GPU-Accelerated Video Processing Pipeline

### Overview

This example demonstrates a complete video processing pipeline that leverages GPU acceleration for real-time effects and
encoding. The implementation shows how to efficiently manage GPU resources, implement frame buffering strategies, and
maintain smooth playback while applying complex transformations.

```csharp
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ComputeSharp;
using FFMpegCore;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Logging;

namespace Graphics.Pipeline.Examples
{
    /// <summary>
    /// GPU-accelerated video processing pipeline with real-time effects
    /// and hardware encoding support
    /// </summary>
    public class GpuVideoProcessingPipeline : IAsyncDisposable
    {
        private readonly ILogger<GpuVideoProcessingPipeline> _logger;
        private readonly VideoProcessingConfiguration _config;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Channel<VideoFrame> _inputFrameChannel;
        private readonly Channel<ProcessedFrame> _outputFrameChannel;
        private readonly ConcurrentQueue<ReadBackTexture2D<Rgba32>> _texturePool;
        private readonly SemaphoreSlim _gpuResourceLimiter;
        private readonly CancellationTokenSource _cancellationSource;

        // Pipeline stages
        private readonly Task _decodingTask;
        private readonly Task[] _processingTasks;
        private readonly Task _encodingTask;

        // Performance tracking
        private readonly PerformanceMonitor _performanceMonitor;
        private long _framesProcessed;
        private long _framesDropped;

        public GpuVideoProcessingPipeline(
            ILogger<GpuVideoProcessingPipeline> logger,
            VideoProcessingConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // Initialize GPU resources
            _graphicsDevice = GraphicsDevice.GetDefault();
            _logger.LogInformation(
                "Initialized GPU device: {Device} with {Memory}MB VRAM",
                _graphicsDevice.Name,
                _graphicsDevice.DedicatedMemorySize / (1024 * 1024));

            // Create processing channels
            var channelOptions = new BoundedChannelOptions(_config.FrameBufferSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = false,
                SingleWriter = true
            };

            _inputFrameChannel = Channel.CreateBounded<VideoFrame>(channelOptions);
            _outputFrameChannel = Channel.CreateBounded<ProcessedFrame>(channelOptions);

            // Initialize texture pool for zero-copy operations
            _texturePool = new ConcurrentQueue<ReadBackTexture2D<Rgba32>>();
            for (int i = 0; i < _config.TexturePoolSize; i++)
            {
                _texturePool.Enqueue(_graphicsDevice.AllocateReadBackTexture2D<Rgba32>(
                    _config.MaxWidth, _config.MaxHeight));
            }

            // GPU resource limiting
            _gpuResourceLimiter = new SemaphoreSlim(
                _config.MaxConcurrentGpuOperations,
                _config.MaxConcurrentGpuOperations);

            _cancellationSource = new CancellationTokenSource();
            _performanceMonitor = new PerformanceMonitor();

            // Start pipeline stages
            _decodingTask = Task.Run(() => RunDecodingStageAsync(_cancellationSource.Token));

            _processingTasks = new Task[_config.GpuWorkerCount];
            for (int i = 0; i < _config.GpuWorkerCount; i++)
            {
                int workerId = i;
                _processingTasks[i] = Task.Run(
                    () => RunProcessingStageAsync(workerId, _cancellationSource.Token));
            }

            _encodingTask = Task.Run(() => RunEncodingStageAsync(_cancellationSource.Token));
        }

        /// <summary>
        /// Process a video file with GPU acceleration
        /// </summary>
        public async Task ProcessVideoAsync(
            string inputPath,
            string outputPath,
            VideoEffects effects,
            IProgress<ProcessingProgress> progress = null,
            CancellationToken cancellationToken = default)
        {
            var mediaInfo = await FFProbe.AnalyseAsync(inputPath, cancellationToken);
            _logger.LogInformation(
                "Processing video: {Path}, Duration: {Duration}, " +
                "Resolution: {Width}x{Height}, FPS: {Fps}",
                inputPath, mediaInfo.Duration,
                mediaInfo.PrimaryVideoStream.Width,
                mediaInfo.PrimaryVideoStream.Height,
                mediaInfo.PrimaryVideoStream.FrameRate);

            // Configure pipeline for specific video
            await ConfigurePipelineAsync(mediaInfo, effects);

            // Start processing
            var processingTask = ProcessVideoInternalAsync(
                inputPath, outputPath, effects, progress, cancellationToken);

            await processingTask;
        }

        /// <summary>
        /// Decoding stage - extracts frames from input video
        /// </summary>
        private async Task RunDecodingStageAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Decoding stage started");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Wait for decode requests
                    await Task.Delay(100, cancellationToken);

                    // In production, this would read from FFmpeg pipe
                    // For demonstration, we simulate frame generation
                    var frame = await ReadNextFrameAsync(cancellationToken);
                    if (frame != null)
                    {
                        await _inputFrameChannel.Writer.WriteAsync(frame, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Decoding stage shutting down");
            }
            finally
            {
                _inputFrameChannel.Writer.TryComplete();
            }
        }

        /// <summary>
        /// GPU processing stage - applies effects to frames
        /// </summary>
        private async Task RunProcessingStageAsync(int workerId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("GPU processing stage {WorkerId} started", workerId);

            try
            {
                await foreach (var frame in _inputFrameChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    await _gpuResourceLimiter.WaitAsync(cancellationToken);

                    try
                    {
                        var processedFrame = await ProcessFrameOnGpuAsync(
                            frame, workerId, cancellationToken);

                        await _outputFrameChannel.Writer.WriteAsync(
                            processedFrame, cancellationToken);

                        Interlocked.Increment(ref _framesProcessed);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "GPU worker {WorkerId} failed processing frame {FrameNumber}",
                            workerId, frame.Number);

                        Interlocked.Increment(ref _framesDropped);
                    }
                    finally
                    {
                        _gpuResourceLimiter.Release();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("GPU processing stage {WorkerId} shutting down", workerId);
            }
        }

        /// <summary>
        /// Process a single frame on GPU with configured effects
        /// </summary>
        private async Task<ProcessedFrame> ProcessFrameOnGpuAsync(
            VideoFrame inputFrame,
            int workerId,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            // Get texture from pool
            if (!_texturePool.TryDequeue(out var texture))
            {
                texture = _graphicsDevice.AllocateReadBackTexture2D<Rgba32>(
                    inputFrame.Width, inputFrame.Height);
            }

            try
            {
                // Upload frame to GPU
                using var gpuTexture = _graphicsDevice.AllocateTexture2D<Rgba32>(
                    inputFrame.Data, inputFrame.Width, inputFrame.Height);

                // Apply effects based on configuration
                if (_config.CurrentEffects.HasFlag(VideoEffects.ColorCorrection))
                {
                    await ApplyColorCorrectionAsync(gpuTexture, cancellationToken);
                }

                if (_config.CurrentEffects.HasFlag(VideoEffects.Denoise))
                {
                    await ApplyDenoiseFilterAsync(gpuTexture, cancellationToken);
                }

                if (_config.CurrentEffects.HasFlag(VideoEffects.Sharpen))
                {
                    await ApplySharpenFilterAsync(gpuTexture, cancellationToken);
                }

                if (_config.CurrentEffects.HasFlag(VideoEffects.MotionBlur))
                {
                    await ApplyMotionBlurAsync(gpuTexture, inputFrame.MotionVectors, cancellationToken);
                }

                // Read back processed frame
                gpuTexture.CopyTo(texture);
                var processedData = new byte[inputFrame.Width * inputFrame.Height * 4];
                texture.CopyTo(processedData);

                return new ProcessedFrame
                {
                    Number = inputFrame.Number,
                    Timestamp = inputFrame.Timestamp,
                    Data = processedData,
                    Width = inputFrame.Width,
                    Height = inputFrame.Height,
                    ProcessingTime = stopwatch.ElapsedMilliseconds
                };
            }
            finally
            {
                // Return texture to pool
                _texturePool.Enqueue(texture);
            }
        }

        /// <summary>
        /// GPU shader for color correction
        /// </summary>
        [AutoConstructor]
        [ThreadGroupSize(32, 32, 1)]
        [GeneratedComputeShaderDescriptor]
        internal readonly partial struct ColorCorrectionShader : IComputeShader
        {
            public readonly ReadWriteTexture2D<Rgba32> texture;
            public readonly float brightness;
            public readonly float contrast;
            public readonly float saturation;
            public readonly float gamma;

            public void Execute()
            {
                uint2 index = ThreadIds.XY;

                if (index.X >= (uint)texture.Width || index.Y >= (uint)texture.Height)
                    return;

                // Read pixel
                float4 color = texture[index];

                // Apply brightness
                color.XYZ += brightness;

                // Apply contrast
                color.XYZ = (color.XYZ - 0.5f) * contrast + 0.5f;

                // Apply saturation
                float luminance = Hlsl.Dot(color.XYZ, float3(0.299f, 0.587f, 0.114f));
                color.XYZ = Hlsl.Lerp(float3(luminance), color.XYZ, saturation);

                // Apply gamma correction
                color.XYZ = Hlsl.Pow(color.XYZ, 1.0f / gamma);

                // Clamp and write back
                color = Hlsl.Saturate(color);
                texture[index] = color;
            }
        }

        /// <summary>
        /// Apply color correction using GPU compute shader
        /// </summary>
        private async Task ApplyColorCorrectionAsync(
            ReadWriteTexture2D<Rgba32> texture,
            CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                var shader = new ColorCorrectionShader(
                    texture,
                    _config.ColorSettings.Brightness,
                    _config.ColorSettings.Contrast,
                    _config.ColorSettings.Saturation,
                    _config.ColorSettings.Gamma);

                _graphicsDevice.For(
                    texture.Width,
                    texture.Height,
                    shader);
            }, cancellationToken);
        }

        /// <summary>
        /// Encoding stage - writes processed frames to output
        /// </summary>
        private async Task RunEncodingStageAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Encoding stage started");

            try
            {
                var frameBuffer = new ConcurrentQueue<ProcessedFrame>();
                var encoderReady = new SemaphoreSlim(0);

                // Start encoder process
                var encodingTask = StartHardwareEncoderAsync(frameBuffer, encoderReady, cancellationToken);

                await foreach (var frame in _outputFrameChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    frameBuffer.Enqueue(frame);
                    encoderReady.Release();

                    // Maintain buffer size limits
                    while (frameBuffer.Count > _config.EncoderBufferSize)
                    {
                        await Task.Delay(10, cancellationToken);
                    }
                }

                await encodingTask;
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Encoding stage shutting down");
            }
            finally
            {
                _outputFrameChannel.Writer.TryComplete();
            }
        }

        /// <summary>
        /// Hardware-accelerated video encoding
        /// </summary>
        private async Task StartHardwareEncoderAsync(
            ConcurrentQueue<ProcessedFrame> frameBuffer,
            SemaphoreSlim frameReady,
            CancellationToken cancellationToken)
        {
            var args = FFMpegArguments
                .FromPipeInput(new StreamPipeSource(new FrameOutputStream(frameBuffer, frameReady)))
                .OutputToFile(_config.OutputPath, true, options => options
                    .WithVideoCodec("h264_nvenc") // NVIDIA hardware encoding
                    .WithConstantRateFactor(21)
                    .WithVideoBitrate(8000)
                    .WithFramerate(30)
                    .ForceFormat("mp4"));

            await args.ProcessAsynchronously();
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            _logger.LogInformation("Shutting down GPU video processing pipeline");

            _cancellationSource.Cancel();

            // Wait for all stages to complete
            await Task.WhenAll(
                _decodingTask,
                Task.WhenAll(_processingTasks),
                _encodingTask);

            // Cleanup GPU resources
            while (_texturePool.TryDequeue(out var texture))
            {
                texture.Dispose();
            }

            _graphicsDevice?.Dispose();
            _gpuResourceLimiter?.Dispose();
            _cancellationSource?.Dispose();

            _logger.LogInformation(
                "Pipeline shutdown complete. Processed {Processed} frames, dropped {Dropped}",
                _framesProcessed, _framesDropped);
        }

        // Supporting classes
        public class VideoProcessingConfiguration
        {
            public int FrameBufferSize { get; set; } = 60;
            public int TexturePoolSize { get; set; } = 10;
            public int MaxConcurrentGpuOperations { get; set; } = 4;
            public int GpuWorkerCount { get; set; } = 2;
            public int EncoderBufferSize { get; set; } = 30;
            public int MaxWidth { get; set; } = 3840;
            public int MaxHeight { get; set; } = 2160;
            public string OutputPath { get; set; }
            public VideoEffects CurrentEffects { get; set; }
            public ColorCorrectionSettings ColorSettings { get; set; } = new();
        }

        [Flags]
        public enum VideoEffects
        {
            None = 0,
            ColorCorrection = 1,
            Denoise = 2,
            Sharpen = 4,
            MotionBlur = 8
        }

        public class ColorCorrectionSettings
        {
            public float Brightness { get; set; } = 0f;
            public float Contrast { get; set; } = 1f;
            public float Saturation { get; set; } = 1f;
            public float Gamma { get; set; } = 1f;
        }
    }
}
```

## Real-Time Streaming Pipeline

### Overview

This pipeline demonstrates a complete real-time image streaming system capable of handling multiple concurrent streams
with dynamic quality adaptation based on network conditions. The implementation showcases advanced buffer management,
adaptive bitrate streaming, and efficient resource pooling.

```csharp
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace Graphics.Pipeline.Examples
{
    /// <summary>
    /// Real-time streaming pipeline with adaptive quality and
    /// efficient resource management
    /// </summary>
    public class RealTimeStreamingPipeline : IAsyncDisposable
    {
        private readonly ILogger<RealTimeStreamingPipeline> _logger;
        private readonly StreamingConfiguration _config;
        private readonly ConcurrentDictionary<Guid, StreamingSession> _activeSessions;
        private readonly Channel<StreamingCommand> _commandChannel;
        private readonly ArrayPool<byte> _bufferPool;
        private readonly SemaphoreSlim _sessionLimiter;
        private readonly Timer _qualityAdaptationTimer;
        private readonly CancellationTokenSource _shutdownSource;

        // Performance tracking
        private readonly StreamingMetrics _metrics;
        private long _totalBytesStreamed;
        private long _totalFramesStreamed;

        public RealTimeStreamingPipeline(
            ILogger<RealTimeStreamingPipeline> logger,
            StreamingConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _activeSessions = new ConcurrentDictionary<Guid, StreamingSession>();
            _commandChannel = Channel.CreateUnbounded<StreamingCommand>();
            _bufferPool = ArrayPool<byte>.Create(
                _config.MaxBufferSize,
                _config.BufferPoolSize);

            _sessionLimiter = new SemaphoreSlim(
                _config.MaxConcurrentStreams,
                _config.MaxConcurrentStreams);

            _metrics = new StreamingMetrics();
            _shutdownSource = new CancellationTokenSource();

            // Start quality adaptation timer
            _qualityAdaptationTimer = new Timer(
                AdaptStreamQuality,
                null,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1));

            // Start command processor
            _ = Task.Run(() => ProcessCommandsAsync(_shutdownSource.Token));

            _logger.LogInformation(
                "Streaming pipeline initialized with {MaxStreams} max concurrent streams",
                _config.MaxConcurrentStreams);
        }

        /// <summary>
        /// Start a new streaming session
        /// </summary>
        public async Task<StreamingSession> StartStreamAsync(
            WebSocket webSocket,
            StreamingParameters parameters,
            CancellationToken cancellationToken = default)
        {
            await _sessionLimiter.WaitAsync(cancellationToken);

            var session = new StreamingSession
            {
                Id = Guid.NewGuid(),
                WebSocket = webSocket,
                Parameters = parameters,
                StartTime = DateTime.UtcNow,
                State = StreamingState.Active,
                QualityLevel = DetermineInitialQuality(parameters),
                FrameChannel = Channel.CreateBounded<StreamFrame>(
                    new BoundedChannelOptions(parameters.BufferFrames)
                    {
                        FullMode = BoundedChannelFullMode.DropOldest
                    }),
                Metrics = new SessionMetrics(),
                CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken, _shutdownSource.Token)
            };

            if (!_activeSessions.TryAdd(session.Id, session))
            {
                _sessionLimiter.Release();
                throw new InvalidOperationException("Failed to register streaming session");
            }

            // Start session handlers
            session.SendTask = Task.Run(
                () => HandleSessionSendAsync(session),
                session.CancellationTokenSource.Token);

            session.ReceiveTask = Task.Run(
                () => HandleSessionReceiveAsync(session),
                session.CancellationTokenSource.Token);

            _logger.LogInformation(
                "Started streaming session {SessionId} with quality {Quality}",
                session.Id, session.QualityLevel);

            return session;
        }

        /// <summary>
        /// Send a frame to specific streaming session
        /// </summary>
        public async Task SendFrameAsync(
            Guid sessionId,
            ReadOnlyMemory<byte> frameData,
            FrameMetadata metadata,
            CancellationToken cancellationToken = default)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                throw new ArgumentException($"Session {sessionId} not found");
            }

            if (session.State != StreamingState.Active)
            {
                return; // Silently drop frames for non-active sessions
            }

            var frame = new StreamFrame
            {
                Data = frameData,
                Metadata = metadata,
                Timestamp = DateTime.UtcNow
            };

            // Apply dynamic quality adjustment
            if (session.QualityLevel != QualityLevel.Original)
            {
                frame = await AdjustFrameQualityAsync(frame, session.QualityLevel, cancellationToken);
            }

            // Try to send frame, drop if buffer full
            await session.FrameChannel.Writer.WriteAsync(frame, cancellationToken);

            session.Metrics.FramesQueued++;
        }

        /// <summary>
        /// Handle sending frames to WebSocket
        /// </summary>
        private async Task HandleSessionSendAsync(StreamingSession session)
        {
            var stopwatch = new Stopwatch();
            var buffer = _bufferPool.Rent(_config.MaxBufferSize);

            try
            {
                await foreach (var frame in session.FrameChannel.Reader.ReadAllAsync(
                    session.CancellationTokenSource.Token))
                {
                    stopwatch.Restart();

                    try
                    {
                        // Encode frame for transmission
                        var encodedSize = await EncodeFrameAsync(
                            frame, buffer, session.Parameters.Format);

                        // Send over WebSocket
                        await session.WebSocket.SendAsync(
                            new ArraySegment<byte>(buffer, 0, encodedSize),
                            WebSocketMessageType.Binary,
                            true,
                            session.CancellationTokenSource.Token);

                        // Update metrics
                        session.Metrics.FramesSent++;
                        session.Metrics.BytesSent += encodedSize;
                        session.Metrics.LastFrameLatency = stopwatch.ElapsedMilliseconds;

                        Interlocked.Add(ref _totalBytesStreamed, encodedSize);
                        Interlocked.Increment(ref _totalFramesStreamed);

                        // Apply frame rate limiting if configured
                        if (session.Parameters.TargetFps > 0)
                        {
                            var targetFrameTime = 1000 / session.Parameters.TargetFps;
                            var elapsed = stopwatch.ElapsedMilliseconds;
                            if (elapsed < targetFrameTime)
                            {
                                await Task.Delay(
                                    TimeSpan.FromMilliseconds(targetFrameTime - elapsed),
                                    session.CancellationTokenSource.Token);
                            }
                        }
                    }
                    catch (WebSocketException ex)
                    {
                        _logger.LogWarning(ex,
                            "WebSocket error in session {SessionId}", session.Id);
                        session.State = StreamingState.Error;
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Send handler for session {SessionId} cancelled", session.Id);
            }
            finally
            {
                _bufferPool.Return(buffer);
            }
        }

        /// <summary>
        /// Handle receiving control messages from WebSocket
        /// </summary>
        private async Task HandleSessionReceiveAsync(StreamingSession session)
        {
            var buffer = new ArraySegment<byte>(new byte[1024]);

            try
            {
                while (!session.CancellationTokenSource.Token.IsCancellationRequested &&
                       session.WebSocket.State == WebSocketState.Open)
                {
                    var result = await session.WebSocket.ReceiveAsync(
                        buffer,
                        session.CancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        session.State = StreamingState.Closing;
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = System.Text.Encoding.UTF8.GetString(
                            buffer.Array, 0, result.Count);

                        await ProcessControlMessageAsync(session, message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Receive handler for session {SessionId} cancelled", session.Id);
            }
            catch (WebSocketException ex)
            {
                _logger.LogWarning(ex,
                    "WebSocket error in session {SessionId}", session.Id);
                session.State = StreamingState.Error;
            }
        }

        /// <summary>
        /// Adjust frame quality based on current level
        /// </summary>
        private async Task<StreamFrame> AdjustFrameQualityAsync(
            StreamFrame originalFrame,
            QualityLevel qualityLevel,
            CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                using var image = Image.Load<Rgba32>(originalFrame.Data.Span);

                // Apply quality adjustments
                var (scale, quality) = GetQualityParameters(qualityLevel);

                if (scale < 1.0f)
                {
                    var newWidth = (int)(image.Width * scale);
                    var newHeight = (int)(image.Height * scale);

                    image.Mutate(x => x.Resize(newWidth, newHeight));
                }

                // Re-encode with adjusted quality
                using var ms = new MemoryStream();
                image.SaveAsJpeg(ms, new JpegEncoder { Quality = quality });

                return new StreamFrame
                {
                    Data = ms.ToArray(),
                    Metadata = originalFrame.Metadata,
                    Timestamp = originalFrame.Timestamp
                };
            }, cancellationToken);
        }

        /// <summary>
        /// Periodically adapt stream quality based on metrics
        /// </summary>
        private void AdaptStreamQuality(object state)
        {
            foreach (var session in _activeSessions.Values)
            {
                if (session.State != StreamingState.Active)
                    continue;

                var metrics = session.Metrics;
                var dropRate = (float)metrics.FramesDropped /
                    Math.Max(1, metrics.FramesQueued);

                var avgLatency = metrics.GetAverageLatency();

                // Adjust quality based on performance
                if (dropRate > 0.1f || avgLatency > 100)
                {
                    // Decrease quality
                    if (session.QualityLevel < QualityLevel.Low)
                    {
                        session.QualityLevel++;
                        _logger.LogInformation(
                            "Decreased quality for session {SessionId} to {Quality}",
                            session.Id, session.QualityLevel);
                    }
                }
                else if (dropRate < 0.01f && avgLatency < 50)
                {
                    // Increase quality
                    if (session.QualityLevel > QualityLevel.Original)
                    {
                        session.QualityLevel--;
                        _logger.LogInformation(
                            "Increased quality for session {SessionId} to {Quality}",
                            session.Id, session.QualityLevel);
                    }
                }

                // Reset metrics for next period
                metrics.Reset();
            }
        }

        /// <summary>
        /// Get quality parameters for given level
        /// </summary>
        private (float scale, int quality) GetQualityParameters(QualityLevel level)
        {
            return level switch
            {
                QualityLevel.Original => (1.0f, 95),
                QualityLevel.High => (0.75f, 85),
                QualityLevel.Medium => (0.5f, 75),
                QualityLevel.Low => (0.25f, 65),
                _ => (1.0f, 95)
            };
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            _logger.LogInformation("Shutting down streaming pipeline");

            _shutdownSource.Cancel();
            _qualityAdaptationTimer?.Dispose();

            // Close all active sessions
            var closeTasks = new List<Task>();
            foreach (var session in _activeSessions.Values)
            {
                closeTasks.Add(CloseSessionAsync(session));
            }

            await Task.WhenAll(closeTasks);

            _sessionLimiter?.Dispose();
            _shutdownSource?.Dispose();

            _logger.LogInformation(
                "Streaming pipeline shutdown complete. " +
                "Total streamed: {Frames} frames, {Bytes} bytes",
                _totalFramesStreamed,
                _totalBytesStreamed);
        }

        /// <summary>
        /// Close a streaming session
        /// </summary>
        private async Task CloseSessionAsync(StreamingSession session)
        {
            session.State = StreamingState.Closing;
            session.CancellationTokenSource.Cancel();

            try
            {
                if (session.SendTask != null)
                    await session.SendTask;
                if (session.ReceiveTask != null)
                    await session.ReceiveTask;

                if (session.WebSocket.State == WebSocketState.Open)
                {
                    await session.WebSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Pipeline shutdown",
                        CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Error closing session {SessionId}", session.Id);
            }
            finally
            {
                _activeSessions.TryRemove(session.Id, out _);
                _sessionLimiter.Release();
                session.CancellationTokenSource?.Dispose();
            }
        }

        // Supporting classes
        public class StreamingConfiguration
        {
            public int MaxConcurrentStreams { get; set; } = 100;
            public int MaxBufferSize { get; set; } = 4 * 1024 * 1024; // 4MB
            public int BufferPoolSize { get; set; } = 50;
        }

        public class StreamingSession
        {
            public Guid Id { get; set; }
            public WebSocket WebSocket { get; set; }
            public StreamingParameters Parameters { get; set; }
            public DateTime StartTime { get; set; }
            public StreamingState State { get; set; }
            public QualityLevel QualityLevel { get; set; }
            public Channel<StreamFrame> FrameChannel { get; set; }
            public SessionMetrics Metrics { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
            public Task SendTask { get; set; }
            public Task ReceiveTask { get; set; }
        }

        public enum StreamingState
        {
            Active,
            Paused,
            Closing,
            Closed,
            Error
        }

        public enum QualityLevel
        {
            Original = 0,
            High = 1,
            Medium = 2,
            Low = 3
        }
    }
}
```

## Summary

These complete pipeline examples demonstrate production-ready implementations that combine the concepts covered
throughout this book. Each pipeline showcases different aspects of high-performance graphics processing:

The **Image Processing Pipeline** demonstrates efficient resource management through memory pooling, parallel processing
with proper synchronization, and format-specific optimizations. It handles error cases gracefully while maintaining high
throughput for batch processing scenarios.

The **GPU Video Processing Pipeline** illustrates real-time processing with hardware acceleration, showing how to manage
GPU resources efficiently and implement complex effects using compute shaders. The pipeline maintains smooth frame rates
while applying sophisticated transformations.

The **Real-Time Streaming Pipeline** exemplifies adaptive quality management and efficient network utilization. It
demonstrates how to handle multiple concurrent streams while dynamically adjusting quality based on network conditions
and client capabilities.

Each implementation follows best practices for error handling, resource cleanup, and performance monitoring. They serve
as templates that can be adapted for specific use cases while maintaining the architectural principles that ensure
scalability, reliability, and performance.
