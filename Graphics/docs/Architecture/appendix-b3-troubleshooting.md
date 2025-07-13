# Appendix B.3: Troubleshooting Guides

## Introduction

Graphics processing applications face unique challenges that can manifest as performance degradation, visual artifacts,
memory issues, or complete failures. This comprehensive troubleshooting guide provides systematic approaches to diagnose
and resolve common problems, complete with code examples and diagnostic tools.

## Performance Issues

### Symptom: Unexpectedly Slow Processing

When graphics operations take significantly longer than expected, systematic investigation helps identify bottlenecks.

#### Diagnostic Approach

```csharp
public class PerformanceDiagnostics
{
    private readonly Dictionary<string, OperationMetrics> _metrics = new();
    private readonly Stopwatch _stopwatch = new();

    /// <summary>
    /// Comprehensive performance profiling for image operations
    /// </summary>
    public async Task<DiagnosticReport> ProfileImageOperation(
        Func<Task> operation,
        string operationName,
        int iterations = 10)
    {
        var report = new DiagnosticReport
        {
            OperationName = operationName,
            Timestamp = DateTime.UtcNow
        };

        // Warm-up run to ensure JIT compilation
        await operation();

        // Collect baseline system metrics
        var baselineMetrics = CollectSystemMetrics();

        // Profile the operation
        var timings = new List<long>();
        var memoryDeltas = new List<long>();

        for (int i = 0; i < iterations; i++)
        {
            // Force garbage collection for consistent measurements
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var startMemory = GC.GetTotalMemory(false);

            _stopwatch.Restart();
            await operation();
            _stopwatch.Stop();

            var endMemory = GC.GetTotalMemory(false);

            timings.Add(_stopwatch.ElapsedMilliseconds);
            memoryDeltas.Add(endMemory - startMemory);

            // Check for thermal throttling
            if (i > 0 && timings[i] > timings[0] * 1.2)
            {
                report.Warnings.Add($"Potential thermal throttling detected at iteration {i}");
            }
        }

        // Analyze results
        report.AverageTime = timings.Average();
        report.MinTime = timings.Min();
        report.MaxTime = timings.Max();
        report.StandardDeviation = CalculateStandardDeviation(timings);
        report.MemoryAllocation = memoryDeltas.Average();

        // Identify specific bottlenecks
        await IdentifyBottlenecks(report, baselineMetrics);

        return report;
    }

    /// <summary>
    /// Identify specific performance bottlenecks
    /// </summary>
    private async Task IdentifyBottlenecks(
        DiagnosticReport report,
        SystemMetrics baseline)
    {
        var current = CollectSystemMetrics();

        // CPU bottleneck detection
        if (current.CpuUsage > 90)
        {
            report.Bottlenecks.Add(new Bottleneck
            {
                Type = BottleneckType.CPU,
                Severity = Severity.High,
                Description = "CPU usage exceeds 90%, indicating CPU-bound operation",
                Recommendations = new[]
                {
                    "Enable SIMD optimizations if not already enabled",
                    "Implement parallel processing for independent operations",
                    "Consider GPU acceleration for suitable workloads",
                    "Profile with Intel VTune or AMD uProf for detailed analysis"
                }
            });
        }

        // Memory bandwidth bottleneck
        if (report.MemoryAllocation > 100_000_000) // 100MB
        {
            report.Bottlenecks.Add(new Bottleneck
            {
                Type = BottleneckType.MemoryBandwidth,
                Severity = Severity.High,
                Description = "Excessive memory allocation detected",
                Recommendations = new[]
                {
                    "Implement object pooling for frequently allocated objects",
                    "Use ArrayPool<T> for temporary buffers",
                    "Consider in-place operations to reduce allocations",
                    "Enable server GC mode for better throughput"
                }
            });
        }

        // Cache efficiency
        if (report.StandardDeviation / report.AverageTime > 0.2)
        {
            report.Bottlenecks.Add(new Bottleneck
            {
                Type = BottleneckType.CacheEfficiency,
                Severity = Severity.Medium,
                Description = "High variance in execution times suggests cache issues",
                Recommendations = new[]
                {
                    "Optimize data layout for better cache locality",
                    "Implement tiling for large images",
                    "Process data in cache-friendly order",
                    "Consider data prefetching for predictable access patterns"
                }
            });
        }
    }
}
```

#### Common Causes and Solutions

**1. Insufficient SIMD Utilization**

```csharp
public static class SimdDiagnostics
{
    /// <summary>
    /// Verify SIMD acceleration is working correctly
    /// </summary>
    public static SimdCapabilityReport CheckSimdCapabilities()
    {
        var report = new SimdCapabilityReport
        {
            IsHardwareAccelerated = Vector.IsHardwareAccelerated,
            VectorSize = Vector<float>.Count * sizeof(float),
            SupportedInstructionSets = new List<string>()
        };

        // Check specific instruction set support
        if (Avx2.IsSupported)
        {
            report.SupportedInstructionSets.Add("AVX2");
            report.RecommendedVectorSize = 32; // 256-bit
        }
        if (Avx512F.IsSupported)
        {
            report.SupportedInstructionSets.Add("AVX-512");
            report.RecommendedVectorSize = 64; // 512-bit
        }
        if (Arm64.IsSupported)
        {
            report.SupportedInstructionSets.Add("ARM NEON");
            report.RecommendedVectorSize = 16; // 128-bit
        }

        // Test actual performance
        report.SimdSpeedup = MeasureSimdSpeedup();

        if (report.SimdSpeedup < 2.0)
        {
            report.Issues.Add("SIMD speedup below expected threshold");
            report.Recommendations.Add("Verify data alignment (16-byte for SSE, 32-byte for AVX)");
            report.Recommendations.Add("Check for scalar code in inner loops");
            report.Recommendations.Add("Ensure .NET runtime optimizations are enabled");
        }

        return report;
    }

    private static double MeasureSimdSpeedup()
    {
        const int size = 1_000_000;
        var data = new float[size];
        var random = new Random(42);

        for (int i = 0; i < size; i++)
        {
            data[i] = (float)random.NextDouble();
        }

        // Measure scalar performance
        var scalarTime = MeasureOperation(() =>
        {
            float sum = 0;
            for (int i = 0; i < size; i++)
            {
                sum += data[i] * data[i];
            }
            return sum;
        });

        // Measure SIMD performance
        var simdTime = MeasureOperation(() =>
        {
            var vSum = Vector<float>.Zero;
            var vectorSize = Vector<float>.Count;

            int i = 0;
            for (; i <= size - vectorSize; i += vectorSize)
            {
                var v = new Vector<float>(data, i);
                vSum += v * v;
            }

            float sum = Vector.Dot(vSum, Vector<float>.One);
            for (; i < size; i++)
            {
                sum += data[i] * data[i];
            }
            return sum;
        });

        return scalarTime / simdTime;
    }
}
```

**2. Memory Allocation Pressure**

```csharp
public static class MemoryDiagnostics
{
    /// <summary>
    /// Track and diagnose memory allocation patterns
    /// </summary>
    public static void DiagnoseMemoryIssues(Action operation)
    {
        // Setup allocation tracking
        var gen0Before = GC.CollectionCount(0);
        var gen1Before = GC.CollectionCount(1);
        var gen2Before = GC.CollectionCount(2);
        var allocatedBefore = GC.GetTotalMemory(false);

        // Run operation
        var stopwatch = Stopwatch.StartNew();
        operation();
        stopwatch.Stop();

        // Collect metrics
        var gen0After = GC.CollectionCount(0);
        var gen1After = GC.CollectionCount(1);
        var gen2After = GC.CollectionCount(2);
        var allocatedAfter = GC.GetTotalMemory(false);

        // Analyze results
        var report = new MemoryDiagnosticReport
        {
            Gen0Collections = gen0After - gen0Before,
            Gen1Collections = gen1After - gen1Before,
            Gen2Collections = gen2After - gen2Before,
            BytesAllocated = allocatedAfter - allocatedBefore,
            ExecutionTime = stopwatch.ElapsedMilliseconds
        };

        // Provide recommendations
        if (report.Gen2Collections > 0)
        {
            Console.WriteLine("WARNING: Gen2 collections detected - indicates LOH allocations");
            Console.WriteLine("Recommendations:");
            Console.WriteLine("- Use ArrayPool<T> for large temporary buffers");
            Console.WriteLine("- Pre-allocate large arrays and reuse them");
            Console.WriteLine("- Consider unmanaged memory for very large buffers");
        }

        if (report.Gen0Collections > report.ExecutionTime / 10)
        {
            Console.WriteLine("WARNING: Excessive Gen0 collections");
            Console.WriteLine("Recommendations:");
            Console.WriteLine("- Implement object pooling for frequently created objects");
            Console.WriteLine("- Use value types where appropriate");
            Console.WriteLine("- Avoid unnecessary boxing operations");
        }
    }
}
```

### Symptom: Inconsistent Frame Rates

Variable performance often indicates resource contention or improper synchronization.

```csharp
public class FrameRateDiagnostics
{
    private readonly CircularBuffer<FrameMetrics> _frameHistory;
    private readonly int _historySize;

    public FrameRateDiagnostics(int historySize = 1000)
    {
        _historySize = historySize;
        _frameHistory = new CircularBuffer<FrameMetrics>(historySize);
    }

    /// <summary>
    /// Analyze frame timing for stuttering and inconsistencies
    /// </summary>
    public FrameAnalysis AnalyzeFrameTiming()
    {
        var analysis = new FrameAnalysis();
        var frameTimes = _frameHistory.ToArray();

        if (frameTimes.Length < 100)
        {
            analysis.Status = "Insufficient data for analysis";
            return analysis;
        }

        // Calculate statistics
        var frameDeltas = frameTimes.Select(f => f.FrameDuration).ToArray();
        analysis.AverageFrameTime = frameDeltas.Average();
        analysis.TargetFps = 1000.0 / analysis.AverageFrameTime;

        // Detect frame spikes
        var spikeThreshold = analysis.AverageFrameTime * 2;
        var spikes = frameDeltas.Where(d => d > spikeThreshold).Count();
        analysis.FrameSpikes = spikes;
        analysis.SpikePercentage = (double)spikes / frameDeltas.Length * 100;

        // Analyze frame time distribution
        analysis.Percentile95 = CalculatePercentile(frameDeltas, 95);
        analysis.Percentile99 = CalculatePercentile(frameDeltas, 99);

        // Identify patterns
        if (analysis.SpikePercentage > 5)
        {
            analysis.Issues.Add("Frequent frame spikes detected");

            // Check for periodic spikes (garbage collection)
            if (DetectPeriodicSpikes(frameTimes))
            {
                analysis.Issues.Add("Periodic frame spikes suggest GC pressure");
                analysis.Recommendations.Add("Reduce allocations in render loop");
                analysis.Recommendations.Add("Use object pooling for temporary objects");
            }

            // Check for GPU sync issues
            if (DetectGpuSyncIssues(frameTimes))
            {
                analysis.Issues.Add("GPU synchronization issues detected");
                analysis.Recommendations.Add("Implement double/triple buffering");
                analysis.Recommendations.Add("Use async GPU operations where possible");
            }
        }

        return analysis;
    }

    /// <summary>
    /// Detect periodic spikes that might indicate GC
    /// </summary>
    private bool DetectPeriodicSpikes(FrameMetrics[] frames)
    {
        var spikeIntervals = new List<int>();
        var lastSpike = -1;

        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i].FrameDuration > frames[i].AverageFrameTime * 2)
            {
                if (lastSpike >= 0)
                {
                    spikeIntervals.Add(i - lastSpike);
                }
                lastSpike = i;
            }
        }

        if (spikeIntervals.Count < 3)
            return false;

        // Check if intervals are roughly consistent
        var avgInterval = spikeIntervals.Average();
        var variance = spikeIntervals.Select(i => Math.Abs(i - avgInterval)).Average();

        return variance < avgInterval * 0.2; // 20% variance threshold
    }
}
```

## Memory Issues

### Symptom: Out of Memory Exceptions

Memory exhaustion requires identifying allocation patterns and implementing proper resource management.

```csharp
public static class MemoryExhaustionDiagnostics
{
    /// <summary>
    /// Monitor and prevent memory exhaustion
    /// </summary>
    public class MemoryGuard : IDisposable
    {
        private readonly Timer _monitorTimer;
        private readonly long _memoryThreshold;
        private readonly Action<MemoryStatus> _alertCallback;
        private bool _isDisposed;

        public MemoryGuard(
            long thresholdBytes = 1_000_000_000, // 1GB
            Action<MemoryStatus> alertCallback = null)
        {
            _memoryThreshold = thresholdBytes;
            _alertCallback = alertCallback;

            _monitorTimer = new Timer(
                CheckMemoryStatus,
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1));
        }

        private void CheckMemoryStatus(object state)
        {
            var status = new MemoryStatus
            {
                TotalMemory = GC.GetTotalMemory(false),
                Gen0Size = GC.GetGeneration(0),
                Gen1Size = GC.GetGeneration(1),
                Gen2Size = GC.GetGeneration(2),
                Timestamp = DateTime.UtcNow
            };

            // Check if approaching threshold
            if (status.TotalMemory > _memoryThreshold * 0.8)
            {
                status.Warning = MemoryWarning.ApproachingLimit;
                status.Message = $"Memory usage at {status.TotalMemory / 1_000_000}MB, " +
                                $"approaching threshold of {_memoryThreshold / 1_000_000}MB";

                // Attempt to free memory
                GC.Collect(2, GCCollectionMode.Forced, true);
                GC.WaitForPendingFinalizers();
                GC.Collect(2, GCCollectionMode.Forced, true);

                var afterGC = GC.GetTotalMemory(false);
                status.MemoryFreed = status.TotalMemory - afterGC;

                _alertCallback?.Invoke(status);
            }

            // Critical threshold
            if (status.TotalMemory > _memoryThreshold)
            {
                status.Warning = MemoryWarning.Critical;
                status.Message = "Memory threshold exceeded - immediate action required";

                _alertCallback?.Invoke(status);

                // Force aggressive cleanup
                EmergencyCleanup();
            }
        }

        private void EmergencyCleanup()
        {
            // Clear all caches
            MemoryCache.Default.Trim(100);

            // Force LOH compaction
            GCSettings.LargeObjectHeapCompactionMode =
                GCLargeObjectHeapCompactionMode.CompactOnce;

            GC.Collect(2, GCCollectionMode.Forced, true, true);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _monitorTimer?.Dispose();
                _isDisposed = true;
            }
        }
    }

    /// <summary>
    /// Track large object allocations
    /// </summary>
    public static void TrackLargeAllocations(Action operation)
    {
        var allocations = new List<AllocationInfo>();

        // Hook into allocation events (requires EventListener)
        using (var listener = new AllocationEventListener(allocations))
        {
            operation();
        }

        // Analyze large allocations
        var largeAllocations = allocations
            .Where(a => a.Size > 85_000) // LOH threshold
            .OrderByDescending(a => a.Size)
            .ToList();

        if (largeAllocations.Any())
        {
            Console.WriteLine("Large Object Heap Allocations Detected:");
            foreach (var alloc in largeAllocations.Take(10))
            {
                Console.WriteLine($"  {alloc.Type}: {alloc.Size:N0} bytes at {alloc.StackTrace}");
            }

            Console.WriteLine("\nRecommendations:");
            Console.WriteLine("- Use ArrayPool<T> for temporary large arrays");
            Console.WriteLine("- Consider unmanaged memory for very large buffers");
            Console.WriteLine("- Implement streaming for large data processing");
        }
    }
}
```

### Symptom: Memory Leaks

Identifying and fixing memory leaks requires systematic tracking of object lifetimes.

```csharp
public class MemoryLeakDetector
{
    private readonly Dictionary<Type, TypeMemoryInfo> _typeTracking = new();
    private readonly WeakReferenceCollection _trackedObjects = new();

    /// <summary>
    /// Track object allocations for leak detection
    /// </summary>
    public void TrackObject<T>(T obj) where T : class
    {
        var type = typeof(T);

        if (!_typeTracking.ContainsKey(type))
        {
            _typeTracking[type] = new TypeMemoryInfo { TypeName = type.Name };
        }

        _typeTracking[type].AllocationCount++;
        _trackedObjects.Add(new WeakReference(obj));
    }

    /// <summary>
    /// Analyze tracked objects for potential leaks
    /// </summary>
    public LeakAnalysisReport AnalyzeLeaks()
    {
        // Force garbage collection to ensure weak references are updated
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var report = new LeakAnalysisReport();

        // Check which objects are still alive
        var aliveObjects = new Dictionary<Type, int>();
        foreach (var weakRef in _trackedObjects)
        {
            if (weakRef.IsAlive)
            {
                var obj = weakRef.Target;
                if (obj != null)
                {
                    var type = obj.GetType();
                    aliveObjects[type] = aliveObjects.GetValueOrDefault(type) + 1;
                }
            }
        }

        // Identify potential leaks
        foreach (var kvp in aliveObjects)
        {
            var type = kvp.Key;
            var aliveCount = kvp.Value;

            if (_typeTracking.TryGetValue(type, out var info))
            {
                var survivalRate = (double)aliveCount / info.AllocationCount;

                if (survivalRate > 0.8 && aliveCount > 100)
                {
                    report.PotentialLeaks.Add(new LeakInfo
                    {
                        TypeName = type.Name,
                        AliveInstances = aliveCount,
                        TotalAllocated = info.AllocationCount,
                        SurvivalRate = survivalRate,
                        Severity = survivalRate > 0.95 ? "High" : "Medium"
                    });
                }
            }
        }

        // Provide specific recommendations
        foreach (var leak in report.PotentialLeaks)
        {
            if (leak.TypeName.Contains("Texture") || leak.TypeName.Contains("Image"))
            {
                leak.Recommendations.Add("Ensure Dispose() is called on all graphics resources");
                leak.Recommendations.Add("Use 'using' statements for automatic disposal");
                leak.Recommendations.Add("Implement finalizers as safety net for unmanaged resources");
            }

            if (leak.TypeName.Contains("Buffer") || leak.TypeName.Contains("Array"))
            {
                leak.Recommendations.Add("Return buffers to ArrayPool when done");
                leak.Recommendations.Add("Clear references to large arrays after use");
                leak.Recommendations.Add("Consider using Memory<T> for zero-copy slicing");
            }
        }

        return report;
    }
}
```

## Visual Artifacts

### Symptom: Corrupted Images

Image corruption often results from incorrect pixel format handling or buffer overruns.

```csharp
public static class VisualArtifactDiagnostics
{
    /// <summary>
    /// Diagnose common image corruption issues
    /// </summary>
    public static CorruptionDiagnosisReport DiagnoseImageCorruption(
        byte[] imageData,
        int expectedWidth,
        int expectedHeight,
        PixelFormat expectedFormat)
    {
        var report = new CorruptionDiagnosisReport();

        // Check data size consistency
        var expectedSize = CalculateExpectedSize(
            expectedWidth, expectedHeight, expectedFormat);

        if (imageData.Length != expectedSize)
        {
            report.Issues.Add(new CorruptionIssue
            {
                Type = CorruptionType.SizeMismatch,
                Description = $"Data size {imageData.Length} doesn't match " +
                             $"expected {expectedSize} bytes",
                PossibleCauses = new[]
                {
                    "Incorrect pixel format specification",
                    "Missing or extra padding bytes",
                    "Partial data transfer"
                },
                Solutions = new[]
                {
                    $"Verify pixel format (expected {expectedFormat})",
                    "Check for row padding/alignment requirements",
                    "Ensure complete data transfer"
                }
            });
        }

        // Check for common patterns indicating corruption
        if (DetectStripePattern(imageData, expectedWidth, expectedFormat))
        {
            report.Issues.Add(new CorruptionIssue
            {
                Type = CorruptionType.StripePattern,
                Description = "Horizontal stripe pattern detected",
                PossibleCauses = new[]
                {
                    "Incorrect stride/pitch calculation",
                    "Row alignment issues",
                    "Endianness mismatch"
                },
                Solutions = new[]
                {
                    "Verify stride calculation includes padding",
                    "Check 4-byte row alignment requirements",
                    "Verify byte order for multi-byte formats"
                }
            });
        }

        // Check for color channel issues
        var channelStats = AnalyzeColorChannels(
            imageData, expectedWidth, expectedHeight, expectedFormat);

        if (channelStats.MaxChannelImbalance > 0.9)
        {
            report.Issues.Add(new CorruptionIssue
            {
                Type = CorruptionType.ChannelCorruption,
                Description = $"Color channel imbalance detected " +
                             $"(channel {channelStats.MostImbalancedChannel})",
                PossibleCauses = new[]
                {
                    "Incorrect channel order (RGB vs BGR)",
                    "Missing or swapped channels",
                    "Bit depth mismatch"
                },
                Solutions = new[]
                {
                    "Verify channel order matches format",
                    "Check for channel swapping in processing",
                    "Confirm bit depth per channel"
                }
            });
        }

        return report;
    }

    /// <summary>
    /// Validate pixel format conversions
    /// </summary>
    public static ValidationReport ValidateFormatConversion(
        PixelFormat sourceFormat,
        PixelFormat targetFormat,
        ConversionOptions options = null)
    {
        var report = new ValidationReport();

        // Check for potential data loss
        if (GetBitsPerPixel(sourceFormat) > GetBitsPerPixel(targetFormat))
        {
            report.Warnings.Add("Conversion will result in data loss");

            if (HasAlphaChannel(sourceFormat) && !HasAlphaChannel(targetFormat))
            {
                report.Warnings.Add("Alpha channel will be lost");
                report.Recommendations.Add("Consider premultiplying alpha before conversion");
            }
        }

        // Check for color space issues
        if (GetColorSpace(sourceFormat) != GetColorSpace(targetFormat))
        {
            report.Warnings.Add("Color space conversion required");
            report.Recommendations.Add("Apply appropriate color profile transformation");
        }

        // Provide conversion code template
        report.ConversionCode = GenerateConversionCode(
            sourceFormat, targetFormat, options);

        return report;
    }
}
```

### Symptom: Color Shifts

Incorrect color reproduction often results from color space mismatches or improper gamma handling.

```csharp
public static class ColorDiagnostics
{
    /// <summary>
    /// Diagnose color accuracy issues
    /// </summary>
    public static ColorAccuracyReport AnalyzeColorAccuracy(
        byte[] sourceImage,
        byte[] processedImage,
        ImageMetadata metadata)
    {
        var report = new ColorAccuracyReport();

        // Check gamma settings
        if (Math.Abs(metadata.SourceGamma - metadata.TargetGamma) > 0.1)
        {
            report.Issues.Add(new ColorIssue
            {
                Type = ColorIssueType.GammaMismatch,
                Description = $"Gamma mismatch: source {metadata.SourceGamma}, " +
                             $"target {metadata.TargetGamma}",
                Impact = "Images will appear too dark or too bright",
                Solution = "Apply gamma correction during processing"
            });
        }

        // Check color profile
        if (metadata.SourceColorProfile != metadata.TargetColorProfile)
        {
            report.Issues.Add(new ColorIssue
            {
                Type = ColorIssueType.ProfileMismatch,
                Description = "Color profile mismatch detected",
                Impact = "Colors will appear shifted or incorrect",
                Solution = "Convert between color profiles using ICC profiles"
            });
        }

        // Calculate color difference metrics
        var colorDifference = CalculateAverageColorDifference(
            sourceImage, processedImage, metadata);

        if (colorDifference.DeltaE > 5.0)
        {
            report.Issues.Add(new ColorIssue
            {
                Type = ColorIssueType.SignificantShift,
                Description = $"Significant color shift detected (ΔE = {colorDifference.DeltaE:F2})",
                Impact = "Visible color differences in output",
                Solution = "Review color processing pipeline for accuracy"
            });
        }

        return report;
    }
}
```

## GPU-Specific Issues

### Symptom: GPU Memory Exhaustion

GPU memory management requires different strategies than system memory.

```csharp
public class GpuMemoryDiagnostics
{
    private readonly GraphicsDevice _device;

    /// <summary>
    /// Monitor GPU memory usage and prevent exhaustion
    /// </summary>
    public async Task<GpuMemoryReport> AnalyzeGpuMemoryAsync()
    {
        var report = new GpuMemoryReport
        {
            DeviceName = _device.Name,
            TotalMemory = _device.DedicatedMemorySize,
            Timestamp = DateTime.UtcNow
        };

        // Get current usage
        report.UsedMemory = await GetGpuMemoryUsageAsync();
        report.AvailableMemory = report.TotalMemory - report.UsedMemory;
        report.UsagePercentage = (double)report.UsedMemory / report.TotalMemory * 100;

        // Check for issues
        if (report.UsagePercentage > 90)
        {
            report.Status = GpuMemoryStatus.Critical;
            report.Recommendations.Add("Immediate texture cleanup required");
            report.Recommendations.Add("Reduce texture resolution or compression");
            report.Recommendations.Add("Implement texture streaming for large assets");
        }
        else if (report.UsagePercentage > 75)
        {
            report.Status = GpuMemoryStatus.Warning;
            report.Recommendations.Add("Consider texture atlasing to reduce overhead");
            report.Recommendations.Add("Enable texture compression where possible");
        }

        // Analyze allocation patterns
        var allocations = await GetTextureAllocationsAsync();
        report.LargestAllocations = allocations
            .OrderByDescending(a => a.Size)
            .Take(10)
            .ToList();

        // Identify potential leaks
        var potentialLeaks = allocations
            .Where(a => a.LastAccessTime < DateTime.UtcNow.AddMinutes(-5))
            .Where(a => a.Size > 10_000_000) // 10MB
            .ToList();

        if (potentialLeaks.Any())
        {
            report.PotentialLeaks = potentialLeaks.Count;
            report.Recommendations.Add($"Found {potentialLeaks.Count} textures not accessed recently");
            report.Recommendations.Add("Implement automatic texture eviction policy");
        }

        return report;
    }
}
```

### Symptom: Shader Compilation Failures

Shader issues require specialized debugging approaches.

```csharp
public static class ShaderDiagnostics
{
    /// <summary>
    /// Diagnose shader compilation issues
    /// </summary>
    public static ShaderDiagnosticReport DiagnoseShaderIssue(
        string shaderCode,
        ShaderType type,
        string errorMessage)
    {
        var report = new ShaderDiagnosticReport
        {
            ShaderType = type,
            OriginalError = errorMessage
        };

        // Parse error message for common patterns
        var errorPattern = ParseShaderError(errorMessage);

        switch (errorPattern.Type)
        {
            case ShaderErrorType.SyntaxError:
                report.Issues.Add(new ShaderIssue
                {
                    Line = errorPattern.Line,
                    Description = "Syntax error in shader code",
                    PossibleCause = "Invalid HLSL/GLSL syntax",
                    Solution = "Check syntax at specified line",
                    CodeSnippet = ExtractCodeSnippet(shaderCode, errorPattern.Line)
                });
                break;

            case ShaderErrorType.UndeclaredIdentifier:
                report.Issues.Add(new ShaderIssue
                {
                    Line = errorPattern.Line,
                    Description = $"Undeclared identifier: {errorPattern.Identifier}",
                    PossibleCause = "Missing variable declaration or typo",
                    Solution = "Declare variable or check spelling",
                    CodeSnippet = ExtractCodeSnippet(shaderCode, errorPattern.Line)
                });
                break;

            case ShaderErrorType.TypeMismatch:
                report.Issues.Add(new ShaderIssue
                {
                    Line = errorPattern.Line,
                    Description = "Type mismatch in operation",
                    PossibleCause = "Incompatible types in expression",
                    Solution = "Ensure types match or add explicit cast",
                    CodeSnippet = ExtractCodeSnippet(shaderCode, errorPattern.Line)
                });
                break;
        }

        // Check for common issues
        if (shaderCode.Contains("texture2D") && type == ShaderType.Compute)
        {
            report.Warnings.Add("texture2D is deprecated, use texture.Sample()");
        }

        if (!shaderCode.Contains("[numthreads") && type == ShaderType.Compute)
        {
            report.Issues.Add(new ShaderIssue
            {
                Description = "Missing [numthreads] attribute",
                PossibleCause = "Compute shader requires thread group size",
                Solution = "Add [numthreads(x,y,z)] before compute shader function"
            });
        }

        return report;
    }
}
```

## Platform-Specific Issues

### Windows-Specific Issues

```csharp
public static class WindowsDiagnostics
{
    /// <summary>
    /// Check Windows-specific graphics settings
    /// </summary>
    public static WindowsGraphicsReport CheckWindowsGraphicsSettings()
    {
        var report = new WindowsGraphicsReport();

        // Check WDDM version
        var wddmVersion = GetWddmVersion();
        if (wddmVersion < new Version(2, 7))
        {
            report.Issues.Add("Outdated WDDM version may limit performance");
            report.Recommendations.Add("Update graphics drivers to latest version");
        }

        // Check Hardware Acceleration settings
        if (!IsHardwareAccelerationEnabled())
        {
            report.Issues.Add("Hardware acceleration is disabled");
            report.Recommendations.Add("Enable hardware acceleration in Windows settings");
        }

        // Check GPU scheduling
        if (!IsHardwareAcceleratedGpuSchedulingEnabled())
        {
            report.Warnings.Add("Hardware-accelerated GPU scheduling is disabled");
            report.Recommendations.Add("Enable in Graphics Settings for reduced latency");
        }

        // Check for TDR issues
        var tdrDelay = GetTdrDelay();
        if (tdrDelay < 10)
        {
            report.Warnings.Add($"TDR delay is {tdrDelay}s - may cause timeout with long operations");
            report.Recommendations.Add("Consider increasing TdrDelay for compute workloads");
        }

        return report;
    }
}
```

### Linux-Specific Issues

```csharp
public static class LinuxDiagnostics
{
    /// <summary>
    /// Diagnose Linux graphics stack issues
    /// </summary>
    public static LinuxGraphicsReport DiagnoseLinuxGraphics()
    {
        var report = new LinuxGraphicsReport();

        // Check kernel version
        var kernelVersion = GetKernelVersion();
        if (kernelVersion < new Version(5, 10))
        {
            report.Warnings.Add("Older kernel may lack latest GPU driver features");
        }

        // Check graphics drivers
        var driverInfo = GetGraphicsDriverInfo();
        switch (driverInfo.Type)
        {
            case "nouveau":
                report.Issues.Add("Using open-source Nouveau driver");
                report.Recommendations.Add("Install proprietary NVIDIA driver for better performance");
                break;

            case "radeon":
                report.Warnings.Add("Using older Radeon driver");
                report.Recommendations.Add("Consider AMDGPU driver for newer cards");
                break;
        }

        // Check Vulkan support
        if (!IsVulkanAvailable())
        {
            report.Issues.Add("Vulkan not available");
            report.Recommendations.Add("Install vulkan-tools and appropriate ICD loader");
        }

        return report;
    }
}
```

## Diagnostic Tools and Utilities

### Comprehensive Diagnostic Suite

```csharp
public class GraphicsDiagnosticSuite
{
    private readonly List<IDiagnosticModule> _modules = new();

    public GraphicsDiagnosticSuite()
    {
        // Register all diagnostic modules
        _modules.Add(new PerformanceDiagnosticModule());
        _modules.Add(new MemoryDiagnosticModule());
        _modules.Add(new GpuDiagnosticModule());
        _modules.Add(new VisualQualityModule());
        _modules.Add(new PlatformSpecificModule());
    }

    /// <summary>
    /// Run comprehensive diagnostics
    /// </summary>
    public async Task<ComprehensiveReport> RunFullDiagnosticsAsync(
        DiagnosticOptions options = null)
    {
        options ??= DiagnosticOptions.Default;
        var report = new ComprehensiveReport
        {
            StartTime = DateTime.UtcNow,
            SystemInfo = GatherSystemInfo()
        };

        // Run each diagnostic module
        foreach (var module in _modules)
        {
            if (options.EnabledModules.Contains(module.Name))
            {
                try
                {
                    var moduleReport = await module.RunDiagnosticsAsync(options);
                    report.ModuleReports.Add(module.Name, moduleReport);
                }
                catch (Exception ex)
                {
                    report.Errors.Add($"Module {module.Name} failed: {ex.Message}");
                }
            }
        }

        // Generate overall health score
        report.HealthScore = CalculateHealthScore(report);

        // Generate prioritized recommendations
        report.PrioritizedRecommendations = PrioritizeRecommendations(report);

        report.EndTime = DateTime.UtcNow;
        return report;
    }

    /// <summary>
    /// Export diagnostic report
    /// </summary>
    public async Task ExportReportAsync(
        ComprehensiveReport report,
        string filePath,
        ReportFormat format = ReportFormat.Html)
    {
        switch (format)
        {
            case ReportFormat.Html:
                await ExportHtmlReport(report, filePath);
                break;

            case ReportFormat.Json:
                await ExportJsonReport(report, filePath);
                break;

            case ReportFormat.Markdown:
                await ExportMarkdownReport(report, filePath);
                break;
        }
    }
}
```

## Quick Reference Troubleshooting Matrix

| Symptom          | Common Causes   | Quick Checks                   | Solutions                    |
|------------------|-----------------|--------------------------------|------------------------------|
| Slow processing  | Missing SIMD    | `Vector.IsHardwareAccelerated` | Enable SIMD, align data      |
| Memory leaks     | Missing Dispose | Memory profiler                | Using statements, finalizers |
| GPU timeout      | Long kernels    | Check TDR settings             | Split work, increase timeout |
| Color shifts     | Gamma mismatch  | Compare profiles               | Apply correction             |
| Corrupted output | Format mismatch | Verify stride/pitch            | Fix alignment                |
| Frame drops      | GC pressure     | GC.CollectionCount             | Object pooling               |
| Artifacts        | Buffer overrun  | Check bounds                   | Validate sizes               |
| Crashes          | Native interop  | Event logs                     | Check marshaling             |

## Summary

This troubleshooting guide provides systematic approaches to diagnose and resolve common issues in graphics processing
applications. Key principles include:

1. **Systematic diagnosis** - Use structured approaches to identify root causes
2. **Comprehensive monitoring** - Track metrics across all system components
3. **Preventive measures** - Implement guards against common issues
4. **Platform awareness** - Consider platform-specific behaviors and limitations
5. **Tool utilization** - Leverage diagnostic tools for deep analysis

Regular application of these diagnostic techniques helps maintain optimal performance and reliability in production
graphics processing systems.
