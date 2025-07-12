# Appendix C.2: Algorithm Complexity Analysis

## Introduction

Understanding the computational complexity of graphics algorithms is essential for making informed implementation
decisions. This appendix provides detailed analysis of time and space complexity for common graphics operations, along
with practical performance implications and optimization strategies.

## Fundamental Operations Complexity

### Pixel-Level Operations

| Operation                | Time Complexity | Space Complexity | Cache Behavior | Parallelizable | SIMD Suitable |
|--------------------------|-----------------|------------------|----------------|----------------|---------------|
| **Brightness Adjust**    | O(n)            | O(1)             | Sequential     | Embarrassingly | Excellent     |
| **Contrast Adjust**      | O(n)            | O(1)             | Sequential     | Embarrassingly | Excellent     |
| **Gamma Correction**     | O(n)            | O(1)¹            | Sequential     | Embarrassingly | Good²         |
| **Threshold**            | O(n)            | O(1)             | Sequential     | Embarrassingly | Excellent     |
| **Color Inversion**      | O(n)            | O(1)             | Sequential     | Embarrassingly | Excellent     |
| **Channel Swap**         | O(n)            | O(1)             | Sequential     | Embarrassingly | Excellent     |
| **Pixel Format Convert** | O(n)            | O(1)             | Sequential     | Embarrassingly | Good          |

¹ O(k) with lookup table, where k is bit depth
² Requires approximation for SIMD efficiency

### Geometric Transformations

| Operation                  | Time Complexity | Space Complexity | Memory Access | Quality Impact | Optimization       |
|----------------------------|-----------------|------------------|---------------|----------------|--------------------|
| **Nearest Neighbor Scale** | O(w₂h₂)         | O(1)             | Random        | Poor           | Cache prefetch     |
| **Bilinear Scale**         | O(4w₂h₂)        | O(1)             | Semi-random   | Good           | Tile processing    |
| **Bicubic Scale**          | O(16w₂h₂)       | O(1)             | Random        | Excellent      | Separable filter   |
| **Lanczos Scale**          | O(k²w₂h₂)       | O(k)             | Random        | Best           | Precompute weights |
| **Rotation (90°)**         | O(n)            | O(n)³            | Strided       | Perfect        | Block transpose    |
| **Rotation (arbitrary)**   | O(4n)           | O(n)             | Random        | Good           | Tile + SIMD        |
| **Affine Transform**       | O(n)            | O(n)             | Random        | Good           | Inverse mapping    |
| **Perspective Transform**  | O(n)            | O(n)             | Random        | Good           | Homogeneous coords |

³ In-place algorithms exist with O(1) extra space but complex implementation

### Convolution Operations

| Kernel Size       | Time Complexity | Space Complexity | Optimization Strategy | Speedup Factor |
|-------------------|-----------------|------------------|-----------------------|----------------|
| **3×3**           | O(9n)           | O(1)             | Direct SIMD           | 4-8×           |
| **5×5**           | O(25n)          | O(1)             | Separable + SIMD      | 10-15×         |
| **7×7**           | O(49n)          | O(1)             | Separable + SIMD      | 14-20×         |
| **Arbitrary k×k** | O(k²n)          | O(1)             | FFT if k > 15         | Variable       |
| **Separable k×k** | O(2kn)          | O(n)             | Two-pass              | k/2 reduction  |
| **Gaussian**      | O(2kn)          | O(k)             | Separable always      | Optimal        |

### Advanced Filtering Complexity

| Algorithm            | Time Complexity | Space Complexity | Preprocessing   | Real-time Suitable |
|----------------------|-----------------|------------------|-----------------|--------------------|
| **Box Blur**         | O(n)            | O(n)             | O(n) integral   | Yes                |
| **Gaussian Blur**    | O(kn)           | O(k)             | O(k) kernel     | Yes                |
| **Median Filter**    | O(n log k)      | O(k)             | None            | Limited            |
| **Bilateral Filter** | O(k²n)          | O(1)             | Optional LUT    | No                 |
| **Non-local Means**  | O(s²n)          | O(n)             | None            | No                 |
| **Guided Filter**    | O(n)            | O(n)             | O(n) box filter | Yes                |
| **Domain Transform** | O(n)            | O(n)             | O(n)            | Yes                |

## Color Space Conversion Complexity

### Standard Color Space Transforms

| Conversion        | Time per Pixel      | Space | SIMD Benefit | Lookup Table | Notes           |
|-------------------|---------------------|-------|--------------|--------------|-----------------|
| **RGB → Gray**    | O(1) - 3 mul, 2 add | O(1)  | 4-8×         | No           | Weighted sum    |
| **RGB → HSV**     | O(1) - 9 ops        | O(1)  | 2-4×         | Partial      | Branch heavy    |
| **RGB → LAB**     | O(1) - 15 ops       | O(1)  | 3-5×         | Yes (³√)     | Nonlinear       |
| **RGB → YCbCr**   | O(1) - 9 ops        | O(1)  | 4-8×         | No           | Matrix multiply |
| **sRGB → Linear** | O(1) - 5 ops        | O(1)  | 2-3×         | Yes          | Piecewise       |
| **LAB → RGB**     | O(1) - 18 ops       | O(1)  | 3-4×         | Yes          | Inverse ³√      |

### Color Management Pipeline

```
Input → Linearize → Matrix Transform → Output Transform → Quantize
O(1)      O(1)           O(1)              O(1)            O(1)

Total: O(n) for n pixels, but constant factors matter significantly
```

## Compression Algorithm Complexity

### Image Compression Algorithms

| Algorithm         | Encode Time | Decode Time | Space | Compression Ratio | Parallelizable |
|-------------------|-------------|-------------|-------|-------------------|----------------|
| **RLE**           | O(n)        | O(n)        | O(1)  | 2-10×             | No             |
| **LZW (GIF)**     | O(n)        | O(n)        | O(d)  | 2-5×              | Limited        |
| **DEFLATE (PNG)** | O(n log n)  | O(n)        | O(w)  | 2-4×              | Block level    |
| **JPEG (DCT)**    | O(n)        | O(n)        | O(1)  | 10-20×            | MCU level      |
| **JPEG 2000**     | O(n log n)  | O(n log n)  | O(n)  | 20-40×            | Tile level     |
| **WebP**          | O(n)        | O(n)        | O(k)  | 25-35×            | Block level    |
| **AVIF**          | O(n²)⁴      | O(n)        | O(k)  | 30-50×            | CTU level      |

⁴ Depends on encoder settings and search depth

### Video Codec Complexity

| Codec          | Encode Complexity | Decode Complexity    | Memory Usage | Key Features          |
|----------------|-------------------|----------------------|--------------|-----------------------|
| **H.264**      | O(n²) per frame   | O(n) per frame       | O(f) frames  | Mature, wide support  |
| **H.265/HEVC** | O(n²·⁵) per frame | O(n log n) per frame | O(f) frames  | 50% better than H.264 |
| **VP9**        | O(n²) per frame   | O(n) per frame       | O(f) frames  | Royalty-free          |
| **AV1**        | O(n³) per frame   | O(n log n) per frame | O(f) frames  | Best compression      |

## Feature Detection Algorithms

### Edge Detection Complexity

| Algorithm           | Time Complexity     | Space Complexity | Accuracy  | Noise Sensitivity |
|---------------------|---------------------|------------------|-----------|-------------------|
| **Sobel**           | O(9n)               | O(1)             | Good      | High              |
| **Prewitt**         | O(9n)               | O(1)             | Good      | High              |
| **Roberts**         | O(4n)               | O(1)             | Fair      | Very High         |
| **Laplacian**       | O(9n)               | O(1)             | Fair      | Very High         |
| **Canny**           | O(n) + O(9n) + O(n) | O(n)             | Excellent | Low               |
| **Structured Edge** | O(n log n)          | O(n)             | Best      | Very Low          |

### Corner Detection

| Algorithm      | Time Complexity | Space Complexity | Repeatability | Scale Invariant |
|----------------|-----------------|------------------|---------------|-----------------|
| **Harris**     | O(k²n)          | O(n)             | Good          | No              |
| **Shi-Tomasi** | O(k²n)          | O(n)             | Good          | No              |
| **FAST**       | O(16n)          | O(1)             | Excellent     | No              |
| **SIFT**       | O(n log n)      | O(n)             | Excellent     | Yes             |
| **SURF**       | O(n)            | O(n)             | Very Good     | Yes             |
| **ORB**        | O(n)            | O(1)             | Good          | Partial         |

## Morphological Operations

### Basic Morphology Complexity

| Operation    | Time (k×k kernel) | Space | Optimization | Decomposable |
|--------------|-------------------|-------|--------------|--------------|
| **Erosion**  | O(k²n)            | O(1)  | Van Herk     | Yes (linear) |
| **Dilation** | O(k²n)            | O(1)  | Van Herk     | Yes (linear) |
| **Opening**  | O(2k²n)           | O(n)  | Cascade      | Yes          |
| **Closing**  | O(2k²n)           | O(n)  | Cascade      | Yes          |
| **Gradient** | O(2k²n)           | O(n)  | Parallel     | Yes          |
| **Top Hat**  | O(3k²n)           | O(n)  | Combined     | Partial      |

### Optimized Morphology Algorithms

| Algorithm        | Standard | Van Herk/Gil-Werman | Improvement Factor     |
|------------------|----------|---------------------|------------------------|
| **1D Erosion**   | O(kn)    | O(3n)               | k/3                    |
| **2D Erosion**   | O(k²n)   | O(6n)               | k²/6                   |
| **Arbitrary SE** | O(kn)    | O(dn)               | k/d (d=decompositions) |

## Transform Domain Algorithms

### Frequency Domain Transforms

| Transform   | Forward    | Inverse    | Space | Applications           |
|-------------|------------|------------|-------|------------------------|
| **DFT**     | O(n²)      | O(n²)      | O(n)  | Reference only         |
| **FFT**     | O(n log n) | O(n log n) | O(n)  | Convolution, filtering |
| **DCT**     | O(n log n) | O(n log n) | O(n)  | JPEG compression       |
| **Wavelet** | O(n)       | O(n)       | O(n)  | Multi-resolution       |
| **Gabor**   | O(kn)      | N/A        | O(k)  | Texture analysis       |

### FFT-Based Convolution Analysis

```
Direct Convolution: O(k²n)
FFT Convolution: O(n log n) + O(n) + O(n log n) = O(n log n)

Break-even kernel size:
k² ≈ log n
For 1024×1024 image: k ≈ 5
For 4096×4096 image: k ≈ 7
```

## Optimization Strategy Selection

### Algorithm Selection by Image Size

| Image Size              | Pixels   | Preferred Algorithms | Avoid             |
|-------------------------|----------|----------------------|-------------------|
| **Thumbnail** (<256²)   | <65K     | Direct convolution   | FFT methods       |
| **Small** (256²-512²)   | 65K-262K | SIMD direct          | Complex iterative |
| **Medium** (512²-2048²) | 262K-4M  | Separable filters    | Naive O(n²)       |
| **Large** (2048²-4096²) | 4M-16M   | Tiled + parallel     | Single-threaded   |
| **Huge** (>4096²)       | >16M     | Streaming + GPU      | Full memory load  |

### Memory Hierarchy Optimization

| Cache Level | Size      | Latency     | Optimization Target |
|-------------|-----------|-------------|---------------------|
| **L1**      | 32-64KB   | 4 cycles    | Inner loop data     |
| **L2**      | 256KB-1MB | 12 cycles   | Working set         |
| **L3**      | 8-32MB    | 40 cycles   | Thread-shared data  |
| **RAM**     | GBs       | 100+ cycles | Sequential access   |

### Tile Size Selection

```csharp
public static int CalculateOptimalTileSize(int imageWidth, int cacheSize)
{
    // Account for input and output data
    int bytesPerPixel = 4; // RGBA
    int workingSetMultiplier = 2; // Input + output

    // Leave room for other data (90% usage)
    int availableCache = (int)(cacheSize * 0.9);

    // Calculate tile size that fits in cache
    int pixelsInCache = availableCache / (bytesPerPixel * workingSetMultiplier);
    int tileSize = (int)Math.Sqrt(pixelsInCache);

    // Align to cache line (64 bytes = 16 pixels for RGBA)
    tileSize = (tileSize / 16) * 16;

    // Ensure minimum size for efficiency
    return Math.Max(tileSize, 64);
}
```

## Parallel Scalability Analysis

### Amdahl's Law Application

| Algorithm             | Serial Portion | Max Speedup (∞ cores) | Practical Speedup (8 cores) |
|-----------------------|----------------|-----------------------|-----------------------------|
| **Pixel Operations**  | <0.1%          | >1000×                | 7.9×                        |
| **Convolution**       | 1%             | 100×                  | 7.5×                        |
| **FFT**               | 10%            | 10×                   | 4.7×                        |
| **Compression**       | 30%            | 3.3×                  | 2.5×                        |
| **Feature Detection** | 20%            | 5×                    | 3.3×                        |

### GPU Acceleration Potential

| Operation Class       | GPU Speedup | Limiting Factor    | Optimal Problem Size |
|-----------------------|-------------|--------------------|----------------------|
| **Pixel-wise**        | 50-100×     | Memory bandwidth   | >1MP                 |
| **Convolution**       | 20-50×      | Memory bandwidth   | >0.5MP               |
| **Geometric**         | 10-30×      | Random access      | >2MP                 |
| **Morphological**     | 30-60×      | Memory bandwidth   | >1MP                 |
| **Feature Detection** | 5-20×       | Divergent branches | >4MP                 |

## Real-World Performance Models

### Combined Operation Complexity

Many real-world pipelines combine multiple operations:

```
Pipeline: Load → Decode → Resize → Filter → Encode → Save

Total Time = T_io + T_decode + T_resize + T_filter + T_encode + T_io
           = O(n) + O(n) + O(m) + O(kn) + O(n log n) + O(n)
           = O(n log n) dominated by encoding
```

### Cache-Aware Complexity

Traditional big-O notation doesn't capture cache effects:

```
Matrix Transpose:
- Naive: O(n²) time, O(1) space
- Cache-oblivious: O(n²) time, O(1) space
- Real performance: 10× difference

Practical complexity: O(n² · (1 + cache_miss_rate · miss_penalty))
```

### SIMD Impact on Constants

| Operation           | Scalar    | SSE (4-wide) | AVX2 (8-wide) | AVX-512 (16-wide) |
|---------------------|-----------|--------------|---------------|-------------------|
| **Vector Add**      | n cycles  | n/4 cycles   | n/8 cycles    | n/16 cycles       |
| **Dot Product**     | 2n cycles | n/2 cycles   | n/4 cycles    | n/8 cycles        |
| **Matrix Multiply** | 2n³ ops   | n³/2 ops     | n³/4 ops      | n³/8 ops          |

## Complexity-Based Decision Trees

### Filter Selection Decision

```
if (kernel_size < 5) {
    use_direct_convolution();  // O(k²n)
} else if (kernel_is_separable) {
    use_separable_filter();     // O(2kn)
} else if (kernel_size > 15) {
    use_fft_convolution();      // O(n log n)
} else {
    use_optimized_direct();     // O(k²n) with SIMD
}
```

### Scaling Algorithm Selection

```
if (scale_factor == 0.5 || scale_factor == 2.0) {
    use_specialized_2x();       // O(n) with SIMD
} else if (quality_requirement == "fast") {
    use_nearest_neighbor();     // O(m)
} else if (quality_requirement == "good") {
    use_bilinear();            // O(4m)
} else if (quality_requirement == "best") {
    use_lanczos();             // O(k²m)
}
```

## Summary

Understanding algorithm complexity enables informed decisions about implementation strategies. Key insights include:

1. **Big-O is not enough**: Cache behavior, SIMD utilization, and parallelization potential often dominate real-world
   performance
2. **Problem size matters**: Different algorithms excel at different scales
3. **Hardware awareness**: Modern processors reward algorithms that exploit parallelism and cache hierarchy
4. **Composition effects**: Pipeline performance is often dominated by the slowest component
5. **Trade-off navigation**: Balance between quality, speed, and resource usage requires understanding complexity
   implications

This complexity analysis serves as a foundation for algorithm selection and optimization strategies in graphics
processing applications.
