# Async Disposal Benchmarks

This document describes the async disposal benchmarks added to measure the performance impact of GitHub Issue #80 implementation.

## Overview

The async disposal benchmarks evaluate the performance characteristics of the new `IAsyncDisposable` implementation for IImage interfaces, specifically focusing on scenarios with large metadata that benefit from non-blocking cleanup.

## Benchmark Categories

### 1. Small Metadata Benchmarks
Tests disposal performance for images with metadata < 1MB threshold:
- **Purpose**: Establish baseline overhead of async disposal
- **Expected**: Minimal performance difference between sync/async
- **Use Cases**: Normal image processing scenarios

### 2. Large Metadata Benchmarks  
Tests disposal performance for images with metadata > 1MB threshold:
- **Purpose**: Measure benefits of async disposal for large metadata
- **Expected**: Async disposal provides better responsiveness
- **Use Cases**: GeoTIFF files, images with extensive EXIF/XMP data

### 3. Batch Disposal Benchmarks
Tests disposal of multiple images in sequence and concurrently:
- **Purpose**: Evaluate scalability of async disposal
- **Variants**: Sequential async, concurrent async
- **Use Cases**: Batch image processing applications

### 4. Metadata Size Estimation Benchmarks
Tests performance of metadata size calculation methods:
- **Purpose**: Measure overhead of large metadata detection
- **Metrics**: `EstimatedMetadataSize`, `HasLargeMetadata` property access
- **Use Cases**: Pre-processing decisions for disposal strategy

## Running Benchmarks

### Quick Demo
```bash
dotnet run --async-demo
```
Runs a simple performance demonstration showing:
- Small vs large metadata disposal times
- Sync vs async disposal comparison
- Batch disposal performance

### Full Benchmarks
```bash
dotnet run --async
```
Runs comprehensive BenchmarkDotNet tests with:
- Memory diagnostics
- Statistical analysis
- Performance regression detection

### All Benchmarks
```bash
dotnet run --all
```
Runs both TIFF raster optimization benchmarks and async disposal benchmarks.

## Benchmark Implementation

### TiffRasterBaseline Updates
The baseline implementation was updated to support the new IImage interface:
- Added `HasLargeMetadata` and `EstimatedMetadataSize` properties
- Implemented simple async disposal with basic yielding
- Maintains compatibility for performance comparisons

### AsyncDisposalBenchmark Class
New benchmark class with comprehensive test coverage:
- **Format Coverage**: WebP, JPEG, TIFF, PNG
- **Metadata Sizes**: Small (< 1MB) and Large (> 1MB)
- **Disposal Patterns**: Sync, async sequential, async concurrent
- **Memory Diagnostics**: Allocation tracking and GC pressure analysis

### AsyncDisposalDemo Class
Interactive demonstration showing real-world performance benefits:
- Creates representative test data
- Measures actual execution times
- Provides user-friendly output with performance insights

## Expected Results

### Small Metadata (< 1MB)
- **Sync vs Async**: Minimal difference (< 5% overhead)
- **Recommendation**: Use either disposal method
- **Rationale**: Async overhead not justified for small metadata

### Large Metadata (> 1MB)
- **Sync vs Async**: Async provides better responsiveness
- **Benefits**: Non-blocking cleanup, better thread pool utilization
- **Recommendation**: Prefer async disposal for large metadata

### Batch Processing
- **Sequential**: Async shows incremental benefits
- **Concurrent**: Significant improvements in throughput
- **Scalability**: Better resource utilization under load

## Integration with GitHub Issue #80

These benchmarks directly validate the implementation of GitHub Issue #80:

1. **Validates Interface Changes**: Tests the new IImage properties
2. **Measures Performance Impact**: Quantifies async disposal overhead
3. **Demonstrates Benefits**: Shows scenarios where async disposal excels
4. **Regression Testing**: Ensures no performance degradation in common cases

## Continuous Integration

The benchmarks can be integrated into CI/CD pipelines to:
- **Monitor Performance**: Track changes over time
- **Prevent Regressions**: Alert on significant performance changes  
- **Validate Optimizations**: Measure improvement from code changes
- **Documentation**: Auto-generate performance reports

## Usage Recommendations

Based on benchmark results:

### When to Use Async Disposal
- ✅ Large metadata (> 1MB)
- ✅ Batch processing scenarios
- ✅ Performance-sensitive applications
- ✅ WebAssembly environments

### When Sync Disposal is Fine
- ✅ Small metadata (< 1MB)
- ✅ Simple applications
- ✅ Synchronous processing pipelines
- ✅ Legacy compatibility requirements

## Future Enhancements

Planned benchmark improvements:
- **Real-world Dataset Testing**: Use actual image files
- **Memory Pressure Simulation**: Test under various GC conditions
- **Platform Comparison**: Compare performance across different runtimes
- **Long-running Process Testing**: Evaluate memory leaks and resource cleanup