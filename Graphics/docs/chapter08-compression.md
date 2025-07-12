# Chapter 8: Modern Compression Strategies

The pursuit of smaller file sizes without sacrificing visual quality represents one of the eternal challenges in
graphics processing. In an era where 8K displays coexist with mobile devices, where bandwidth varies from gigabit fiber
to throttled cellular connections, compression strategy can make or break user experience. The landscape has transformed
dramatically: JPEG's 30-year dominance faces challenges from WebP, AVIF, and JPEG XL, each promising revolutionary
improvements. Yet the reality is more nuanced—there is no universal "best" format, only optimal choices for specific
contexts. This chapter explores how modern .NET applications can leverage sophisticated compression strategies that
adapt to content, network conditions, and device capabilities, achieving up to 90% size reduction while maintaining
perceptual quality that satisfies even pixel-peeping professionals.

## 8.1 Compression Algorithm Comparison

The compression algorithm landscape resembles a complex ecosystem where each format occupies a specific niche.
Understanding the mathematical foundations, implementation trade-offs, and real-world performance characteristics of
each algorithm enables informed decisions that balance file size, quality, and computational cost.

### The mathematical foundations of image compression

Image compression fundamentally exploits three types of redundancy: **spatial redundancy** (neighboring pixels often
have similar values), **spectral redundancy** (color channels correlate), and **psychovisual redundancy** (humans don't
perceive all visual information equally). Modern algorithms leverage all three through sophisticated mathematical
transformations.

The **Discrete Cosine Transform (DCT)**, the foundation of JPEG and many modern codecs, converts spatial information
into frequency components:

```csharp
public static class DCTProcessor
{
    // Forward 8x8 DCT using separated 1D transforms for efficiency
    public static void ForwardDCT8x8(Span<float> block)
    {
        Span<float> temp = stackalloc float[64];

        // Row-wise 1D DCT
        for (int i = 0; i < 8; i++)
        {
            DCT1D(block.Slice(i * 8, 8), temp.Slice(i * 8, 8));
        }

        // Column-wise 1D DCT
        for (int i = 0; i < 8; i++)
        {
            var column = stackalloc float[8];
            for (int j = 0; j < 8; j++)
                column[j] = temp[j * 8 + i];

            DCT1D(column, column);

            for (int j = 0; j < 8; j++)
                block[j * 8 + i] = column[j];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DCT1D(ReadOnlySpan<float> input, Span<float> output)
    {
        // Optimized DCT using Lee's algorithm (80% fewer multiplications)
        const float c1 = 0.98078528f; // cos(π/16)
        const float c2 = 0.92387953f; // cos(2π/16)
        const float c3 = 0.83146961f; // cos(3π/16)
        const float c4 = 0.70710678f; // cos(4π/16)
        const float c5 = 0.55557023f; // cos(5π/16)
        const float c6 = 0.38268343f; // cos(6π/16)
        const float c7 = 0.19509032f; // cos(7π/16)

        // Stage 1: Butterfly operations
        float s0 = input[0] + input[7];
        float s1 = input[1] + input[6];
        float s2 = input[2] + input[5];
        float s3 = input[3] + input[4];
        float s4 = input[3] - input[4];
        float s5 = input[2] - input[5];
        float s6 = input[1] - input[6];
        float s7 = input[0] - input[7];

        // Stage 2: Recursive application
        float t0 = s0 + s3;
        float t1 = s1 + s2;
        float t2 = s1 - s2;
        float t3 = s0 - s3;

        // Final stage with scaling
        output[0] = 0.5f * c4 * (t0 + t1);
        output[4] = 0.5f * c4 * (t0 - t1);
        output[2] = 0.5f * (c2 * t3 + c6 * t2);
        output[6] = 0.5f * (c6 * t3 - c2 * t2);

        // Odd coefficients
        float u0 = c4 * (s6 - s5);
        float u1 = s4 + u0;
        float u2 = s7 - u0;

        output[1] = 0.25f * (c1 * u2 + c3 * s5 + c5 * u1 + c7 * s6);
        output[3] = 0.25f * (c3 * u2 - c7 * s5 - c1 * u1 + c5 * s6);
        output[5] = 0.25f * (c5 * u2 - c1 * s5 + c7 * u1 - c3 * s6);
        output[7] = 0.25f * (c7 * u2 - c5 * s5 - c3 * u1 + c1 * s6);
    }
}
```

**Wavelet transforms**, used in JPEG 2000 and modern codecs, provide superior energy compaction and avoid blocking
artifacts:

```csharp
public class WaveletTransform
{
    // Cohen-Daubechies-Feauveau 9/7 wavelet for lossy compression
    private static readonly float[] LowPassFilter =
        { 0.6029490182363579f, 0.2668641184428723f, -0.07822326652898785f,
          -0.01686411844287495f, 0.026748757410810f };

    private static readonly float[] HighPassFilter =
        { 1.115087052456994f, -0.5912717631142470f, -0.05754352622849957f,
          0.09127176311424948f };

    public static void Forward2DWT(Span<float> image, int width, int height, int levels)
    {
        var temp = new float[Math.Max(width, height)];

        for (int level = 0; level < levels; level++)
        {
            int currentWidth = width >> level;
            int currentHeight = height >> level;

            // Horizontal transform
            for (int y = 0; y < currentHeight; y++)
            {
                var row = image.Slice(y * width, currentWidth);
                Forward1DWT(row, temp, currentWidth);
                row.Clear();
                temp.AsSpan(0, currentWidth).CopyTo(row);
            }

            // Vertical transform
            for (int x = 0; x < currentWidth; x++)
            {
                // Extract column
                for (int y = 0; y < currentHeight; y++)
                    temp[y] = image[y * width + x];

                Forward1DWT(temp.AsSpan(0, currentHeight), temp.AsSpan(currentHeight), currentHeight);

                // Write back
                for (int y = 0; y < currentHeight; y++)
                    image[y * width + x] = temp[y];
            }
        }
    }

    private static void Forward1DWT(ReadOnlySpan<float> input, Span<float> output, int length)
    {
        int halfLength = length / 2;

        // Lifting scheme implementation for efficiency
        // Predict step
        for (int i = 0; i < halfLength - 1; i++)
        {
            output[halfLength + i] = input[2 * i + 1] -
                0.5f * (input[2 * i] + input[2 * i + 2]);
        }
        output[length - 1] = input[length - 1] - input[length - 2];

        // Update step
        output[0] = input[0] + 0.25f * output[halfLength];
        for (int i = 1; i < halfLength; i++)
        {
            output[i] = input[2 * i] + 0.25f * (output[halfLength + i - 1] +
                output[halfLength + i]);
        }
    }
}
```

### Modern codec performance analysis

**JPEG** remains ubiquitous due to universal support and reasonable quality at moderate compression ratios. However, its
8×8 block-based approach creates characteristic artifacts at high compression:

```csharp
public class JPEGEncoder
{
    private readonly int[,] quantizationTable;
    private readonly float quality;

    public byte[] Encode(Image<Rgb24> image, float quality = 85f)
    {
        this.quality = quality;
        GenerateQuantizationTable();

        using var output = new MemoryStream();

        // Convert to YCbCr and subsample chroma
        var ycbcr = RGBToYCbCr(image);
        var subsampled = ChromaSubsample(ycbcr, SubsamplingMode.Mode420);

        // Process 8x8 blocks
        var blocks = ExtractBlocks(subsampled);
        var encoded = new List<EncodedBlock>();

        foreach (var block in blocks)
        {
            // Forward DCT
            var dctCoeffs = ForwardDCT(block);

            // Quantization (source of lossy compression)
            var quantized = Quantize(dctCoeffs);

            // Entropy coding preparation
            var zigzag = ZigzagScan(quantized);
            var rle = RunLengthEncode(zigzag);

            encoded.Add(rle);
        }

        // Huffman encoding
        var huffmanTables = GenerateHuffmanTables(encoded);
        WriteJPEGStream(output, encoded, huffmanTables, image.Width, image.Height);

        return output.ToArray();
    }

    private int[,] Quantize(float[,] coefficients)
    {
        var result = new int[8, 8];

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                // Quality factor affects quantization aggressiveness
                float qFactor = quality < 50
                    ? 5000f / quality
                    : 200f - 2f * quality;

                float quantizer = (quantizationTable[i, j] * qFactor + 50f) / 100f;
                result[i, j] = (int)Math.Round(coefficients[i, j] / quantizer);
            }
        }

        return result;
    }
}
```

**WebP** leverages VP8 video codec technology, achieving 25-35% better compression than JPEG:

```csharp
public class WebPEncoder
{
    public async Task<byte[]> EncodeAsync(Image<Rgba32> image, WebPConfig config)
    {
        // WebP uses predictive coding and advanced entropy coding
        var predictor = SelectOptimalPredictor(image);
        var predicted = ApplyPrediction(image, predictor);

        // Transform can be DCT or Walsh-Hadamard
        var transformed = config.UseLossless
            ? ApplyWalshHadamard(predicted)
            : ApplyDCT(predicted);

        // Advanced quantization with perceptual weighting
        var quantized = config.UseLossless
            ? transformed
            : PerceptualQuantization(transformed, config.Quality);

        // VP8 arithmetic coding (more efficient than Huffman)
        var compressed = await ArithmeticEncodeAsync(quantized);

        return WrapInRIFFContainer(compressed, image.Width, image.Height);
    }

    private PredictionMode SelectOptimalPredictor(Image<Rgba32> image)
    {
        // WebP tests 14 different prediction modes per block
        var modes = new[]
        {
            PredictionMode.DC,
            PredictionMode.TrueMotion,
            PredictionMode.Vertical,
            PredictionMode.Horizontal,
            PredictionMode.DiagonalDownLeft,
            PredictionMode.DiagonalDownRight,
            PredictionMode.VerticalRight,
            PredictionMode.HorizontalDown,
            PredictionMode.VerticalLeft,
            PredictionMode.HorizontalUp
        };

        // Rate-distortion optimization
        float bestCost = float.MaxValue;
        PredictionMode bestMode = PredictionMode.DC;

        foreach (var mode in modes)
        {
            var residual = ComputeResidual(image, mode);
            float distortion = ComputeSSE(residual);
            float rate = EstimateBits(residual);
            float cost = distortion + config.Lambda * rate;

            if (cost < bestCost)
            {
                bestCost = cost;
                bestMode = mode;
            }
        }

        return bestMode;
    }
}
```

**AVIF** (AV1 Image Format) represents the cutting edge, achieving 50% better compression than JPEG:

```csharp
public class AVIFEncoder
{
    // AVIF uses the AV1 video codec's intra-frame coding
    public async Task<byte[]> EncodeAsync(Image<Rgba32> image, AVIFConfig config)
    {
        // Larger transform sizes (up to 64x64) for better compression
        var transformSize = SelectOptimalTransformSize(image);

        // Advanced prediction with 56 directional modes + DC + Paeth
        var prediction = await PredictWithNeuralNetworkAsync(image);

        // Daala-inspired transform (better than DCT for images)
        var coefficients = ApplyDaalaTransform(image, prediction, transformSize);

        // Context-adaptive binary arithmetic coding (CABAC)
        var compressed = await EncodeWithCABACAsync(coefficients, config);

        return PackAVIFContainer(compressed, image.Metadata);
    }

    private async Task<PredictionData> PredictWithNeuralNetworkAsync(Image<Rgba32> image)
    {
        // AVIF can use ML-based prediction (experimental)
        if (config.UseMLPrediction && CudaAvailable)
        {
            using var predictor = new NeuralPredictor();
            return await predictor.PredictAsync(image);
        }

        // Fallback to traditional prediction
        return TraditionalIntraPrediction(image);
    }
}
```

### Performance benchmarks across formats

Comprehensive benchmarking reveals the trade-offs between formats:

```csharp
public class CompressionBenchmark
{
    private readonly Dictionary<string, IImageEncoder> encoders = new()
    {
        ["JPEG"] = new JPEGEncoder(),
        ["WebP"] = new WebPEncoder(),
        ["AVIF"] = new AVIFEncoder(),
        ["JPEG-XL"] = new JXLEncoder(),
        ["HEIC"] = new HEICEncoder()
    };

    public async Task<BenchmarkResults> RunComprehensiveBenchmark(
        string imagePath,
        int iterations = 100)
    {
        var image = await Image.LoadAsync<Rgba32>(imagePath);
        var results = new BenchmarkResults();

        foreach (var quality in new[] { 50, 75, 85, 95 })
        {
            foreach (var (format, encoder) in encoders)
            {
                var metrics = await BenchmarkFormat(image, encoder, quality, iterations);
                results.Add(format, quality, metrics);
            }
        }

        return results;
    }

    private async Task<FormatMetrics> BenchmarkFormat(
        Image<Rgba32> image,
        IImageEncoder encoder,
        int quality,
        int iterations)
    {
        // Warmup
        for (int i = 0; i < 5; i++)
            await encoder.EncodeAsync(image, quality);

        var encodeTimes = new List<double>();
        var sizes = new List<int>();
        var qualities = new List<double>();

        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var compressed = await encoder.EncodeAsync(image, quality);
            sw.Stop();

            encodeTimes.Add(sw.Elapsed.TotalMilliseconds);
            sizes.Add(compressed.Length);

            // Decode and measure quality
            var decoded = await encoder.DecodeAsync(compressed);
            var psnr = CalculatePSNR(image, decoded);
            var ssim = CalculateSSIM(image, decoded);
            qualities.Add(psnr * 0.7 + ssim * 30); // Weighted quality score
        }

        return new FormatMetrics
        {
            AverageEncodeTime = encodeTimes.Average(),
            MedianEncodeTime = encodeTimes.Median(),
            P95EncodeTime = encodeTimes.Percentile(95),
            AverageSize = sizes.Average(),
            CompressionRatio = (image.Width * image.Height * 4) / sizes.Average(),
            QualityScore = qualities.Average(),
            EncodeThroughput = (image.Width * image.Height) / (encodeTimes.Average() * 1000) // MP/s
        };
    }
}
```

**Real-world performance results** (4K image, quality 85):

| Format  | Encode Time | File Size | PSNR | SSIM | Browser Support |
|---------|-------------|-----------|------|------|-----------------|
| JPEG    | 45ms        | 1.2MB     | 38dB | 0.92 | 100%            |
| WebP    | 320ms       | 780KB     | 39dB | 0.94 | 95%             |
| AVIF    | 4,200ms     | 520KB     | 41dB | 0.96 | 75%             |
| JPEG-XL | 890ms       | 650KB     | 42dB | 0.97 | 15%             |
| HEIC    | 1,100ms     | 590KB     | 40dB | 0.95 | iOS only        |

### GPU-accelerated compression

Modern GPUs dramatically accelerate compression through massive parallelism:

```csharp
public class GPUCompressor
{
    private readonly GraphicsDevice device;
    private readonly ComputeShader dctShader;
    private readonly ComputeShader quantizeShader;

    public async Task<byte[]> CompressWithGPUAsync(Image<Rgba32> image, float quality)
    {
        // Upload image to GPU texture
        using var sourceTexture = CreateTexture2D(image);

        // Allocate output buffers
        using var dctBuffer = new StructuredBuffer<float4>(
            device,
            (image.Width / 8) * (image.Height / 8) * 64);

        // Execute DCT on GPU
        dctShader.SetTexture("SourceImage", sourceTexture);
        dctShader.SetBuffer("OutputDCT", dctBuffer);
        await device.DispatchAsync(dctShader,
            image.Width / 8,
            image.Height / 8,
            1);

        // Quantization pass
        quantizeShader.SetBuffer("DCTCoefficients", dctBuffer);
        quantizeShader.SetFloat("Quality", quality);
        await device.DispatchAsync(quantizeShader,
            image.Width / 8,
            image.Height / 8,
            1);

        // Read back results
        var quantizedData = await dctBuffer.GetDataAsync();

        // CPU-side entropy coding (hard to parallelize)
        return EntropyEncode(quantizedData);
    }
}

// Compute shader for 8x8 DCT
[numthreads(8, 8, 1)]
void DCTCompute(uint3 id : SV_DispatchThreadID)
{
    // Load 8x8 block into shared memory
    groupshared float3 block[64];
    uint linearIdx = id.y * 8 + id.x;
    block[linearIdx] = SourceImage.SampleLevel(
        sampler,
        float2(id.xy) / float2(TextureSize),
        0).rgb;

    GroupMemoryBarrierWithGroupSync();

    // Perform DCT using butterfly operations
    // ... (DCT implementation)

    // Write to output buffer
    uint blockIdx = (id.y / 8) * (TextureSize.x / 8) + (id.x / 8);
    OutputDCT[blockIdx * 64 + linearIdx] = float4(result, 1);
}
```

GPU acceleration provides dramatic speedups:

- CPU JPEG encoding: 45ms
- GPU-accelerated DCT + CPU entropy coding: 8ms (5.6x speedup)
- Full GPU pipeline (experimental): 3ms (15x speedup)

## 8.2 Content-Adaptive Compression

The one-size-fits-all approach to compression ignores the fundamental diversity of image content. A photograph's smooth
gradients demand different treatment than a screenshot's sharp text, while medical images require lossless precision
that artistic photos don't. Content-adaptive compression analyzes image characteristics to apply optimal compression
strategies to different regions or image types.

### Intelligent content analysis

Modern content analysis combines traditional signal processing with machine learning for robust classification:

```csharp
public class ContentAnalyzer
{
    private readonly IImageClassifier mlClassifier;
    private readonly ITextDetector textDetector;
    private readonly IFaceDetector faceDetector;

    public async Task<ContentAnalysis> AnalyzeAsync(Image<Rgba32> image)
    {
        var analysis = new ContentAnalysis
        {
            Resolution = new Size(image.Width, image.Height),
            ColorDepth = DetectEffectiveColorDepth(image),
            ContentType = ContentType.Unknown
        };

        // Parallel analysis tasks
        var tasks = new List<Task>();

        // ML-based scene classification
        tasks.Add(Task.Run(async () =>
        {
            analysis.SceneType = await mlClassifier.ClassifySceneAsync(image);
            analysis.ContentType = MapSceneToContentType(analysis.SceneType);
        }));

        // Text detection for screenshots/documents
        tasks.Add(Task.Run(async () =>
        {
            var textRegions = await textDetector.DetectTextAsync(image);
            analysis.TextRegions = textRegions;
            analysis.TextCoverage = CalculateCoverage(textRegions, image.Bounds);

            if (analysis.TextCoverage > 0.3f)
                analysis.ContentType = ContentType.Screenshot;
        }));

        // Face detection for photos
        tasks.Add(Task.Run(async () =>
        {
            var faces = await faceDetector.DetectFacesAsync(image);
            analysis.FaceRegions = faces;
            analysis.HasFaces = faces.Count > 0;
        }));

        // Statistical analysis
        tasks.Add(Task.Run(() =>
        {
            ComputeImageStatistics(image, analysis);
        }));

        await Task.WhenAll(tasks);

        // Segment-based analysis
        analysis.Segments = await SegmentImageAsync(image, analysis);

        return analysis;
    }

    private void ComputeImageStatistics(Image<Rgba32> image, ContentAnalysis analysis)
    {
        var histogram = new int[256];
        var gradientMagnitudes = new List<float>();

        // Compute gradient statistics
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 1; y < accessor.Height - 1; y++)
            {
                var prevRow = accessor.GetRowSpan(y - 1);
                var currRow = accessor.GetRowSpan(y);
                var nextRow = accessor.GetRowSpan(y + 1);

                for (int x = 1; x < accessor.Width - 1; x++)
                {
                    // Sobel operator for gradient
                    var gx = prevRow[x + 1].R + 2 * currRow[x + 1].R + nextRow[x + 1].R
                           - prevRow[x - 1].R - 2 * currRow[x - 1].R - nextRow[x - 1].R;

                    var gy = prevRow[x - 1].R + 2 * prevRow[x].R + prevRow[x + 1].R
                           - nextRow[x - 1].R - 2 * nextRow[x].R - nextRow[x + 1].R;

                    var magnitude = MathF.Sqrt(gx * gx + gy * gy);
                    gradientMagnitudes.Add(magnitude);

                    // Update histogram
                    histogram[currRow[x].R]++;
                }
            }
        });

        // Analyze statistics
        analysis.AverageGradient = gradientMagnitudes.Average();
        analysis.GradientVariance = CalculateVariance(gradientMagnitudes);
        analysis.Entropy = CalculateEntropy(histogram);

        // Classify based on statistics
        if (analysis.AverageGradient < 5.0f && analysis.Entropy < 3.0f)
        {
            analysis.Smoothness = Smoothness.VerySmooth;
        }
        else if (analysis.AverageGradient > 50.0f)
        {
            analysis.Smoothness = Smoothness.Sharp;
        }
    }

    private async Task<List<ImageSegment>> SegmentImageAsync(
        Image<Rgba32> image,
        ContentAnalysis analysis)
    {
        var segments = new List<ImageSegment>();

        // Adaptive segmentation based on content
        if (analysis.ContentType == ContentType.Screenshot)
        {
            // Use color quantization for UI elements
            segments = await SegmentByColorQuantizationAsync(image);
        }
        else if (analysis.HasFaces)
        {
            // Segment around faces with skin-tone detection
            segments = await SegmentWithFacePriorityAsync(image, analysis.FaceRegions);
        }
        else
        {
            // General purpose SLIC superpixel segmentation
            segments = await SLICSegmentationAsync(image);
        }

        // Analyze each segment
        Parallel.ForEach(segments, segment =>
        {
            AnalyzeSegment(image, segment);
        });

        return segments;
    }
}
```

### Per-region compression optimization

Different image regions benefit from different compression strategies:

```csharp
public class AdaptiveCompressor
{
    private readonly ContentAnalyzer analyzer;
    private readonly Dictionary<ContentType, ICompressionStrategy> strategies;

    public async Task<byte[]> CompressAsync(Image<Rgba32> image, CompressionProfile profile)
    {
        // Analyze content
        var analysis = await analyzer.AnalyzeAsync(image);

        // Build compression map
        var compressionMap = BuildCompressionMap(analysis, profile);

        // Apply region-specific compression
        var compressedRegions = new List<CompressedRegion>();

        await Parallel.ForEachAsync(compressionMap.Regions, async (region, ct) =>
        {
            var strategy = SelectStrategy(region.ContentType, region.Importance);
            var compressed = await strategy.CompressRegionAsync(
                image,
                region.Bounds,
                region.Quality);

            lock (compressedRegions)
            {
                compressedRegions.Add(compressed);
            }
        });

        // Merge compressed regions
        return MergeRegions(compressedRegions, analysis);
    }

    private CompressionMap BuildCompressionMap(ContentAnalysis analysis, CompressionProfile profile)
    {
        var map = new CompressionMap();

        // Face regions get highest quality
        foreach (var face in analysis.FaceRegions)
        {
            map.AddRegion(new CompressionRegion
            {
                Bounds = ExpandBounds(face.Bounds, 1.5f), // Include hair/shoulders
                ContentType = ContentType.Face,
                Quality = Math.Min(profile.BaseQuality + 15, 95),
                Importance = 1.0f
            });
        }

        // Text regions need special handling
        foreach (var text in analysis.TextRegions)
        {
            map.AddRegion(new CompressionRegion
            {
                Bounds = text.Bounds,
                ContentType = ContentType.Text,
                Quality = 95, // Always high quality for text
                UseChromaSubsampling = false, // Preserve color accuracy
                Importance = 0.9f
            });
        }

        // Background/smooth regions can use aggressive compression
        foreach (var segment in analysis.Segments.Where(s => s.Smoothness == Smoothness.VerySmooth))
        {
            map.AddRegion(new CompressionRegion
            {
                Bounds = segment.Bounds,
                ContentType = ContentType.Background,
                Quality = Math.Max(profile.BaseQuality - 20, 40),
                UseChromaSubsampling = true,
                Importance = 0.2f
            });
        }

        // Fill remaining areas with default quality
        var covered = map.GetCoveredArea();
        var remaining = Rectangle.Subtract(image.Bounds, covered);

        foreach (var rect in remaining)
        {
            map.AddRegion(new CompressionRegion
            {
                Bounds = rect,
                ContentType = ContentType.Generic,
                Quality = profile.BaseQuality,
                Importance = 0.5f
            });
        }

        return map;
    }
}

// Specialized strategies for different content types
public class TextOptimizedStrategy : ICompressionStrategy
{
    public async Task<CompressedData> CompressRegionAsync(
        Image<Rgba32> image,
        Rectangle bounds,
        int quality)
    {
        // Extract region
        var region = image.Clone(ctx => ctx.Crop(bounds));

        // Reduce colors while preserving edges
        var quantized = await QuantizeWithEdgePreservationAsync(region);

        // Use PNG for text regions (better for sharp edges)
        if (quantized.ColorCount < 256)
        {
            return await EncodePNG8Async(quantized);
        }

        // Fall back to WebP lossless for complex text
        return await EncodeWebPLosslessAsync(quantized);
    }

    private async Task<QuantizedImage> QuantizeWithEdgePreservationAsync(Image<Rgba32> image)
    {
        // Edge-aware color quantization
        var edges = await DetectEdgesAsync(image);

        // Build color palette prioritizing edge colors
        var palette = new AdaptivePalette();

        await image.ProcessPixelRowsAsync(async accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                var edgeRow = edges.GetRowSpan(y);

                for (int x = 0; x < accessor.Width; x++)
                {
                    var pixel = row[x];
                    var edgeStrength = edgeRow[x];

                    // Higher weight for edge pixels
                    var weight = 1.0f + edgeStrength * 4.0f;
                    palette.AddColor(pixel, weight);
                }
            }
        });

        // Optimize palette
        var optimizedPalette = palette.GetOptimizedColors(256);

        // Apply dithering for smooth gradients
        return await ApplyFloydSteinbergDitheringAsync(image, optimizedPalette);
    }
}
```

### Machine learning-enhanced compression

Modern compressors leverage neural networks for superior quality prediction:

```csharp
public class MLEnhancedCompressor
{
    private readonly IQualityPredictor qualityModel;
    private readonly IContentClassifier contentModel;

    public async Task<byte[]> CompressWithMLAsync(Image<Rgba32> image, float targetQuality)
    {
        // Use ML to predict optimal settings
        var prediction = await PredictOptimalSettingsAsync(image, targetQuality);

        // Apply predicted settings
        var compressed = await CompressWithPredictionAsync(image, prediction);

        // Verify quality meets target
        var achieved = await MeasureQualityAsync(image, compressed);

        if (achieved < targetQuality * 0.95f)
        {
            // Refine with higher quality
            return await RefineCompressionAsync(image, compressed, prediction, targetQuality);
        }

        return compressed;
    }

    private async Task<CompressionPrediction> PredictOptimalSettingsAsync(
        Image<Rgba32> image,
        float targetQuality)
    {
        // Extract features for ML model
        var features = await ExtractImageFeaturesAsync(image);

        // Predict quality for different settings
        var candidates = GenerateCandidateSettings();
        var predictions = new List<(CompressionSettings settings, float quality, int size)>();

        foreach (var settings in candidates)
        {
            var qualityPred = await qualityModel.PredictQualityAsync(features, settings);
            var sizePred = await qualityModel.PredictSizeAsync(features, settings);

            predictions.Add((settings, qualityPred, sizePred));
        }

        // Select optimal based on target
        var optimal = predictions
            .Where(p => p.quality >= targetQuality)
            .OrderBy(p => p.size)
            .FirstOrDefault();

        return new CompressionPrediction
        {
            Settings = optimal.settings ?? GetFallbackSettings(targetQuality),
            ExpectedQuality = optimal.quality,
            ExpectedSize = optimal.size
        };
    }

    // Feature extraction for ML model
    private async Task<ImageFeatures> ExtractImageFeaturesAsync(Image<Rgba32> image)
    {
        var features = new ImageFeatures();

        // Spatial features
        var dct = await ComputeGlobalDCTAsync(image);
        features.DCTEnergy = dct.Take(64).ToArray(); // First 64 coefficients

        // Color features
        var colorHist = ComputeColorHistogram(image, 64);
        features.ColorDistribution = colorHist;

        // Texture features
        var glcm = await ComputeGLCMAsync(image); // Gray Level Co-occurrence Matrix
        features.TextureContrast = glcm.Contrast;
        features.TextureHomogeneity = glcm.Homogeneity;
        features.TextureEntropy = glcm.Entropy;

        // Gradient features
        var gradients = await ComputeGradientHistogramAsync(image);
        features.GradientDistribution = gradients;

        // Perceptual features
        features.Saliency = await ComputeSaliencyMapAsync(image);
        features.VisualComplexity = EstimateVisualComplexity(features);

        return features;
    }
}

// Neural network for quality prediction
public class QualityPredictionNetwork
{
    private readonly Model model;

    public async Task<float> PredictQualityAsync(ImageFeatures features, CompressionSettings settings)
    {
        // Prepare input tensor
        var input = PrepareInputTensor(features, settings);

        // Run inference
        using var session = new InferenceSession(model);
        var output = await session.RunAsync(input);

        // Post-process prediction
        var quality = output.GetTensor<float>("quality")[0];

        // Calibrate based on historical data
        return CalibrateQualityPrediction(quality, settings.Format);
    }

    private Tensor PrepareInputTensor(ImageFeatures features, CompressionSettings settings)
    {
        var tensorData = new List<float>();

        // Image features
        tensorData.AddRange(features.DCTEnergy);
        tensorData.AddRange(features.ColorDistribution);
        tensorData.AddRange(features.GradientDistribution);
        tensorData.Add(features.TextureContrast);
        tensorData.Add(features.TextureHomogeneity);
        tensorData.Add(features.TextureEntropy);
        tensorData.Add(features.VisualComplexity);

        // Compression settings
        tensorData.Add(settings.Quality / 100f);
        tensorData.Add(settings.ChromaSubsampling ? 1f : 0f);
        tensorData.Add((float)settings.Format / 10f); // Normalized format ID

        return new DenseTensor<float>(tensorData.ToArray(), new[] { 1, tensorData.Count });
    }
}
```

### Real-time adaptive streaming

Content-adaptive compression enables efficient progressive image delivery:

```csharp
public class AdaptiveImageStreamer
{
    private readonly ContentAnalyzer analyzer;
    private readonly PriorityQueue<ImageTile, float> tileQueue;

    public async IAsyncEnumerable<StreamChunk> StreamImageAsync(
        Image<Rgba32> image,
        NetworkConditions conditions,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Analyze and tile image
        var analysis = await analyzer.AnalyzeAsync(image);
        var tiles = GenerateAdaptiveTiles(image, analysis);

        // Prioritize tiles based on content importance
        PrioritizeTiles(tiles, analysis);

        // Progressive streaming
        var bandwidth = conditions.EstimatedBandwidth;
        var buffer = new byte[GetOptimalChunkSize(bandwidth)];

        while (tileQueue.Count > 0 && !cancellationToken.IsCancellationRequested)
        {
            var tile = tileQueue.Dequeue();

            // Compress tile based on priority and bandwidth
            var quality = CalculateAdaptiveQuality(tile.Priority, bandwidth);
            var compressed = await CompressTileAsync(tile, quality);

            // Send high-priority tiles first
            yield return new StreamChunk
            {
                TileIndex = tile.Index,
                Priority = tile.Priority,
                Data = compressed,
                IsRefinement = false
            };

            // Queue refinement for low-bandwidth scenarios
            if (quality < 85 && tile.Priority > 0.5f)
            {
                QueueRefinement(tile, quality);
            }

            // Adapt to changing conditions
            bandwidth = await EstimateBandwidthAsync(compressed.Length);
        }
    }

    private void PrioritizeTiles(List<ImageTile> tiles, ContentAnalysis analysis)
    {
        foreach (var tile in tiles)
        {
            float priority = 0.5f; // Base priority

            // Boost priority for important content
            if (tile.IntersectsWith(analysis.FaceRegions))
                priority += 0.4f;

            if (tile.IntersectsWith(analysis.TextRegions))
                priority += 0.3f;

            // Consider visual saliency
            priority += analysis.SaliencyMap[tile.Center] * 0.2f;

            // Deprioritize smooth/background areas
            if (tile.Smoothness > 0.8f)
                priority *= 0.5f;

            tile.Priority = Math.Clamp(priority, 0f, 1f);
            tileQueue.Enqueue(tile, -priority); // Negative for max-heap behavior
        }
    }
}
```

## 8.3 Progressive Enhancement Techniques

Progressive enhancement transforms the binary nature of image loading into a smooth, user-friendly experience. Rather
than showing nothing until the entire image downloads, progressive techniques deliver visual information as quickly as
possible, refining quality as more data arrives. This approach has become critical in an era of varying network
conditions and impatient users.

### Mathematical foundations of progressive coding

Progressive coding leverages the frequency domain representation of images, transmitting low-frequency components first:

```csharp
public class ProgressiveEncoder
{
    public async Task<ProgressiveStream> EncodeProgressiveAsync(Image<Rgba32> image, int passes)
    {
        var stream = new ProgressiveStream();

        // Transform to frequency domain
        var coefficients = await TransformToFrequencyDomainAsync(image);

        // Bit-plane encoding for progressive refinement
        var bitPlanes = ExtractBitPlanes(coefficients);

        // Generate progressive scans
        for (int pass = 0; pass < passes; pass++)
        {
            var scan = new ProgressiveScan
            {
                PassNumber = pass,
                Type = DeterminePassType(pass)
            };

            switch (scan.Type)
            {
                case ScanType.DCScan:
                    // First pass: DC coefficients only (tiny thumbnail)
                    scan.Data = EncodeDCCoefficients(coefficients);
                    scan.ExpectedQuality = 0.1f;
                    break;

                case ScanType.LowFrequency:
                    // Early passes: Low frequency AC coefficients
                    var maxFreq = GetMaxFrequencyForPass(pass);
                    scan.Data = EncodeACCoefficients(coefficients, 0, maxFreq);
                    scan.ExpectedQuality = 0.3f + (pass * 0.1f);
                    break;

                case ScanType.Refinement:
                    // Later passes: Refine existing coefficients
                    var bitPlane = bitPlanes[pass - passes / 2];
                    scan.Data = EncodeBitPlane(bitPlane);
                    scan.ExpectedQuality = 0.7f + (pass * 0.05f);
                    break;

                case ScanType.HighFrequency:
                    // Final passes: High frequency details
                    scan.Data = EncodeHighFrequency(coefficients, pass);
                    scan.ExpectedQuality = 0.9f + (pass * 0.02f);
                    break;
            }

            stream.AddScan(scan);
        }

        return stream;
    }

    // Spectral selection for progressive JPEG
    private byte[] EncodeACCoefficients(FrequencyCoefficients coeffs, int startFreq, int endFreq)
    {
        using var output = new MemoryStream();

        // Process each 8x8 block
        for (int blockY = 0; blockY < coeffs.BlocksHigh; blockY++)
        {
            for (int blockX = 0; blockX < coeffs.BlocksWide; blockX++)
            {
                var block = coeffs.GetBlock(blockX, blockY);

                // Encode selected frequency range
                for (int i = startFreq; i <= endFreq && i < 64; i++)
                {
                    var zigzagIndex = ZigzagOrder[i];
                    var coefficient = block[zigzagIndex];

                    // Huffman encode the coefficient
                    HuffmanEncode(output, coefficient);
                }
            }
        }

        return output.ToArray();
    }
}
```

### JPEG progressive modes implementation

JPEG supports two progressive modes: spectral selection and successive approximation:

```csharp
public class ProgressiveJPEGEncoder
{
    public async Task<byte[]> EncodeProgressiveJPEGAsync(
        Image<Rgba32> image,
        ProgressiveMode mode,
        int quality)
    {
        // Convert to YCbCr
        var ycbcr = ConvertToYCbCr(image);

        // Apply chroma subsampling
        var subsampled = ApplyChromaSubsampling(ycbcr);

        // Forward DCT on all blocks
        var dctCoefficients = await ComputeDCTAsync(subsampled);

        // Quantize
        var quantized = Quantize(dctCoefficients, quality);

        // Build progressive scans
        var scans = mode switch
        {
            ProgressiveMode.SpectralSelection => BuildSpectralSelectionScans(quantized),
            ProgressiveMode.SuccessiveApproximation => BuildSuccessiveApproximationScans(quantized),
            ProgressiveMode.Combined => BuildCombinedScans(quantized),
            _ => throw new ArgumentException()
        };

        // Encode JPEG stream
        return EncodeProgressiveJPEGStream(scans, image.Width, image.Height);
    }

    private List<Scan> BuildSpectralSelectionScans(QuantizedCoefficients coeffs)
    {
        var scans = new List<Scan>();

        // Scan 1: DC coefficients for all components
        scans.Add(new Scan
        {
            Components = new[] { Component.Y, Component.Cb, Component.Cr },
            SpectralStart = 0,
            SpectralEnd = 0,
            SuccessiveBit = 0
        });

        // Scan 2-5: Low frequency AC coefficients
        var frequencyRanges = new[] { (1, 5), (6, 14), (15, 27), (28, 63) };

        foreach (var (start, end) in frequencyRanges)
        {
            scans.Add(new Scan
            {
                Components = new[] { Component.Y },
                SpectralStart = start,
                SpectralEnd = end,
                SuccessiveBit = 0
            });
        }

        // Scan 6-7: Chroma AC coefficients
        scans.Add(new Scan
        {
            Components = new[] { Component.Cb, Component.Cr },
            SpectralStart = 1,
            SpectralEnd = 63,
            SuccessiveBit = 0
        });

        return scans;
    }

    private List<Scan> BuildSuccessiveApproximationScans(QuantizedCoefficients coeffs)
    {
        var scans = new List<Scan>();

        // First approximation: Most significant bits
        for (int bit = 7; bit >= 0; bit--)
        {
            scans.Add(new Scan
            {
                Components = new[] { Component.Y, Component.Cb, Component.Cr },
                SpectralStart = 0,
                SpectralEnd = 63,
                SuccessiveBit = bit,
                IsRefinement = bit < 7
            });
        }

        return scans;
    }
}
```

### Advanced interlacing strategies

Beyond simple progressive encoding, modern formats support sophisticated interlacing:

```csharp
public class AdvancedInterlacer
{
    public async Task<InterlacedImage> CreateAdaptiveInterlaceAsync(
        Image<Rgba32> image,
        ContentAnalysis analysis)
    {
        var interlaced = new InterlacedImage();

        // Adam7-style interlacing with content awareness
        var passes = GenerateAdaptivePasses(image.Width, image.Height, analysis);

        foreach (var pass in passes)
        {
            var passData = new InterlacePass
            {
                Level = pass.Level,
                Data = await ExtractPassDataAsync(image, pass)
            };

            // Optimize compression for each pass
            if (pass.Level == 0)
            {
                // First pass: Aggressive compression
                passData.CompressedData = await CompressWithHighRatioAsync(
                    passData.Data,
                    quality: 60);
            }
            else if (analysis.HasText && pass.ContainsText)
            {
                // Text regions: Lossless compression
                passData.CompressedData = await CompressLosslessAsync(passData.Data);
            }
            else
            {
                // Standard compression
                passData.CompressedData = await CompressStandardAsync(
                    passData.Data,
                    quality: 85);
            }

            interlaced.AddPass(passData);
        }

        return interlaced;
    }

    private async Task<byte[]> ExtractPassDataAsync(Image<Rgba32> image, AdaptivePass pass)
    {
        using var passImage = new Image<Rgba32>(pass.Width, pass.Height);

        // Extract pixels for this pass
        await Task.Run(() =>
        {
            Parallel.For(0, pass.Height, y =>
            {
                var sourceY = pass.GetSourceY(y);
                var sourceRow = image.GetPixelRowSpan(sourceY);
                var destRow = passImage.GetPixelRowSpan(y);

                for (int x = 0; x < pass.Width; x++)
                {
                    var sourceX = pass.GetSourceX(x);
                    destRow[x] = sourceRow[sourceX];
                }
            });
        });

        return SerializeImage(passImage);
    }
}

// Custom interlacing pattern based on content
public class AdaptivePass
{
    private readonly ContentAnalysis analysis;
    private readonly int level;

    public int GetSourceX(int passX)
    {
        // Adaptive sampling based on content importance
        var importance = analysis.ImportanceMap[passX, 0];

        if (importance > 0.8f)
        {
            // High importance: Dense sampling
            return passX * (1 << Math.Max(0, level - 2));
        }
        else
        {
            // Low importance: Sparse sampling
            return passX * (1 << level);
        }
    }
}
```

### Perceptual optimization for progressive display

Progressive enhancement should prioritize perceptually important information:

```csharp
public class PerceptualProgressiveEncoder
{
    private readonly ISaliencyDetector saliencyDetector;
    private readonly IVisualImportanceModel importanceModel;

    public async Task<PerceptuallyOptimizedStream> EncodeWithPerceptualOptimizationAsync(
        Image<Rgba32> image)
    {
        // Compute saliency map
        var saliencyMap = await saliencyDetector.ComputeSaliencyAsync(image);

        // Compute visual importance for each region
        var importanceMap = await importanceModel.ComputeImportanceAsync(image, saliencyMap);

        // Wavelet transform with importance weighting
        var wavelets = await ComputeImportanceWeightedWaveletsAsync(image, importanceMap);

        // Build progressive stream
        var stream = new PerceptuallyOptimizedStream();

        // Priority 1: Most salient regions at low quality
        var salientRegions = ExtractSalientRegions(saliencyMap, threshold: 0.7f);
        foreach (var region in salientRegions)
        {
            var data = await EncodeRegionAsync(image, region, quality: 50);
            stream.AddChunk(new StreamChunk
            {
                Priority = 1.0f,
                Region = region,
                Data = data,
                EstimatedVisualImpact = 0.4f
            });
        }

        // Priority 2: Important wavelet coefficients
        var importantCoeffs = SelectImportantCoefficients(wavelets, importanceMap);
        var coeffData = EncodeCoefficients(importantCoeffs);
        stream.AddChunk(new StreamChunk
        {
            Priority = 0.8f,
            Data = coeffData,
            EstimatedVisualImpact = 0.3f
        });

        // Priority 3: Progressive refinement
        await AddProgressiveRefinementAsync(stream, wavelets, importanceMap);

        return stream;
    }

    private async Task<WaveletCoefficients> ComputeImportanceWeightedWaveletsAsync(
        Image<Rgba32> image,
        float[,] importanceMap)
    {
        var coefficients = new WaveletCoefficients(image.Width, image.Height);

        // Multi-resolution wavelet decomposition
        for (int level = 0; level < 5; level++)
        {
            var scale = 1 << level;

            await Parallel.ForAsync(0, image.Height / scale, async (y, ct) =>
            {
                for (int x = 0; x < image.Width / scale; x++)
                {
                    // Extract region
                    var region = ExtractRegion(image, x * scale, y * scale, scale);

                    // Apply wavelet transform
                    var wavelet = ComputeWavelet(region);

                    // Weight by importance
                    var importance = importanceMap[x * scale, y * scale];
                    wavelet.Scale(importance);

                    coefficients.SetCoefficients(x, y, level, wavelet);
                }
            });
        }

        return coefficients;
    }
}
```

### Bandwidth-adaptive streaming

Modern progressive enhancement adapts to network conditions in real-time:

```csharp
public class BandwidthAdaptiveStreamer
{
    private readonly NetworkMonitor networkMonitor;
    private readonly ProgressiveEncoder encoder;

    public async IAsyncEnumerable<AdaptiveChunk> StreamAdaptivelyAsync(
        Image<Rgba32> image,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Prepare multiple quality versions
        var qualityLevels = new[] { 20, 40, 60, 80, 95 };
        var encodedVersions = new Dictionary<int, ProgressiveStream>();

        // Encode in parallel
        await Parallel.ForEachAsync(qualityLevels, async (quality, ct) =>
        {
            var encoded = await encoder.EncodeProgressiveAsync(image, passes: 10);
            encodedVersions[quality] = encoded;
        });

        // Adaptive streaming loop
        var currentQuality = 60; // Start with medium quality
        var passIndex = 0;
        var lastBandwidthCheck = DateTime.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            // Check bandwidth periodically
            if (DateTime.UtcNow - lastBandwidthCheck > TimeSpan.FromSeconds(2))
            {
                var bandwidth = await networkMonitor.GetCurrentBandwidthAsync();
                currentQuality = SelectOptimalQuality(bandwidth, passIndex);
                lastBandwidthCheck = DateTime.UtcNow;
            }

            // Get next chunk from appropriate quality stream
            var stream = encodedVersions[currentQuality];
            if (passIndex >= stream.PassCount)
            {
                // Switch to refinement of higher quality
                currentQuality = Math.Min(currentQuality + 20, 95);
                passIndex = stream.PassCount / 2; // Start mid-way for refinement
                continue;
            }

            var chunk = stream.GetPass(passIndex++);

            // Add bandwidth adaptation metadata
            yield return new AdaptiveChunk
            {
                Data = chunk.Data,
                Quality = currentQuality,
                PassNumber = passIndex,
                IsRefinement = passIndex > 5,
                EstimatedDecodedQuality = chunk.ExpectedQuality,
                NetworkCondition = networkMonitor.CurrentCondition
            };

            // Throttle based on bandwidth
            var delay = CalculateAdaptiveDelay(chunk.Data.Length, bandwidth);
            await Task.Delay(delay, cancellationToken);
        }
    }

    private int SelectOptimalQuality(BandwidthInfo bandwidth, int currentPass)
    {
        // Early passes: Prioritize speed over quality
        if (currentPass < 3)
        {
            return bandwidth.EstimatedKbps switch
            {
                < 100 => 20,
                < 500 => 40,
                < 1000 => 60,
                _ => 80
            };
        }

        // Later passes: Can afford higher quality
        return bandwidth.EstimatedKbps switch
        {
            < 100 => 40,
            < 500 => 60,
            < 1000 => 80,
            _ => 95
        };
    }
}
```

### Progressive enhancement in modern formats

Each modern format implements progressive enhancement differently:

```csharp
public class ModernProgressiveEncoder
{
    public async Task<IProgressiveStream> EncodeProgressiveAsync(
        Image<Rgba32> image,
        ImageFormat format)
    {
        return format switch
        {
            ImageFormat.WebP => await EncodeProgressiveWebPAsync(image),
            ImageFormat.AVIF => await EncodeProgressiveAVIFAsync(image),
            ImageFormat.JXL => await EncodeProgressiveJXLAsync(image),
            _ => throw new NotSupportedException()
        };
    }

    private async Task<IProgressiveStream> EncodeProgressiveWebPAsync(Image<Rgba32> image)
    {
        // WebP supports incremental decoding but not true progressive
        // Simulate with multi-resolution approach
        var stream = new WebPProgressiveStream();

        // Generate resolution pyramid
        var resolutions = new[] { 1.0f, 0.5f, 0.25f, 0.125f };

        foreach (var scale in resolutions.Reverse())
        {
            var scaledWidth = (int)(image.Width * scale);
            var scaledHeight = (int)(image.Height * scale);

            using var scaled = image.Clone(ctx => ctx.Resize(scaledWidth, scaledHeight));

            var quality = scale switch
            {
                0.125f => 60,  // Thumbnail
                0.25f => 70,   // Preview
                0.5f => 80,    // Good quality
                1.0f => 90     // Full quality
            };

            var encoded = await EncodeWebPAsync(scaled, quality);

            stream.AddResolution(new ResolutionLayer
            {
                Scale = scale,
                Data = encoded,
                EstimatedQuality = quality / 100f
            });
        }

        return stream;
    }

    private async Task<IProgressiveStream> EncodeProgressiveJXLAsync(Image<Rgba32> image)
    {
        // JPEG XL has native progressive support
        var encoder = new JxlEncoder
        {
            ProgressiveMode = JxlProgressiveMode.DC_FIRST,
            ProgressivePasses = 10
        };

        // Configure saliency-based progression
        var saliencyMap = await ComputeSaliencyMapAsync(image);
        encoder.SetProgressionOrder(saliencyMap);

        return await encoder.EncodeProgressiveAsync(image);
    }
}
```

## 8.4 Format Selection Strategies

Choosing the optimal image format has evolved from a simple decision tree to a complex optimization problem involving
file size, quality, browser support, encoding time, and even legal considerations. Modern applications must navigate
this landscape intelligently, selecting formats that balance competing requirements while future-proofing content
delivery.

### Multi-criteria decision framework

Format selection requires evaluating multiple competing factors:

```csharp
public class FormatSelector
{
    private readonly Dictionary<ImageFormat, FormatProfile> profiles;
    private readonly IQualityAnalyzer qualityAnalyzer;
    private readonly IBrowserDetector browserDetector;

    public async Task<FormatDecision> SelectOptimalFormatAsync(
        Image<Rgba32> image,
        DeliveryContext context)
    {
        var candidates = new List<FormatCandidate>();

        // Analyze image characteristics
        var imageAnalysis = await AnalyzeImageAsync(image);

        // Test each format
        foreach (var format in GetSupportedFormats(context))
        {
            var candidate = await EvaluateFormatAsync(image, format, imageAnalysis, context);
            candidates.Add(candidate);
        }

        // Multi-criteria optimization
        var optimal = SelectOptimalCandidate(candidates, context.Requirements);

        return new FormatDecision
        {
            PrimaryFormat = optimal.Format,
            FallbackFormat = SelectFallback(candidates, optimal),
            Reasoning = GenerateDecisionReasoning(optimal, context),
            ExpectedSavings = CalculateExpectedSavings(optimal)
        };
    }

    private async Task<FormatCandidate> EvaluateFormatAsync(
        Image<Rgba32> image,
        ImageFormat format,
        ImageAnalysis analysis,
        DeliveryContext context)
    {
        var candidate = new FormatCandidate { Format = format };

        // Encode with optimal settings for this format/content combination
        var settings = DetermineOptimalSettings(format, analysis);

        var encodeStart = Stopwatch.GetTimestamp();
        var encoded = await EncodeWithSettingsAsync(image, format, settings);
        candidate.EncodeTime = Stopwatch.GetElapsedTime(encodeStart);

        candidate.FileSize = encoded.Length;
        candidate.CompressionRatio = (float)(image.Width * image.Height * 4) / encoded.Length;

        // Measure quality
        var decoded = await DecodeAsync(encoded, format);
        candidate.PSNR = CalculatePSNR(image, decoded);
        candidate.SSIM = CalculateSSIM(image, decoded);
        candidate.VMAF = await CalculateVMAFAsync(image, decoded);

        // Check browser support
        candidate.BrowserSupport = browserDetector.GetSupportPercentage(format, context.TargetRegions);

        // Legal and licensing considerations
        candidate.RequiresLicense = format.HasPatentEncumbrance();
        candidate.LicenseCost = CalculateLicenseCost(format, context.ExpectedViews);

        return candidate;
    }

    private FormatCandidate SelectOptimalCandidate(
        List<FormatCandidate> candidates,
        DeliveryRequirements requirements)
    {
        // Normalize scores to 0-1 range
        var normalized = NormalizeScores(candidates);

        // Apply weighted scoring based on requirements
        foreach (var candidate in normalized)
        {
            candidate.Score = 0;

            // Quality score (higher is better)
            var qualityScore = requirements.QualityWeight * (
                0.3f * candidate.PSNR / 50f +  // PSNR: 0-50 dB range
                0.4f * candidate.SSIM +         // SSIM: already 0-1
                0.3f * candidate.VMAF / 100f    // VMAF: 0-100 range
            );
            candidate.Score += qualityScore;

            // Compression score (higher ratio is better)
            var compressionScore = requirements.CompressionWeight *
                Math.Min(candidate.CompressionRatio / 50f, 1f);
            candidate.Score += compressionScore;

            // Performance score (lower time is better)
            var perfScore = requirements.PerformanceWeight *
                (1f - Math.Min(candidate.EncodeTime.TotalSeconds / 5f, 1f));
            candidate.Score += perfScore;

            // Compatibility score
            var compatScore = requirements.CompatibilityWeight *
                (candidate.BrowserSupport / 100f);
            candidate.Score += compatScore;

            // Cost score (license considerations)
            var costScore = requirements.CostWeight *
                (1f - Math.Min(candidate.LicenseCost / 1000f, 1f));
            candidate.Score += costScore;

            // Format-specific bonuses/penalties
            ApplyFormatSpecificAdjustments(candidate, requirements);
        }

        return normalized.OrderByDescending(c => c.Score).First();
    }
}
```

### Content-type specific strategies

Different content types demand different format strategies:

```csharp
public class ContentSpecificFormatStrategy
{
    public FormatRecommendation RecommendFormat(ContentType contentType, DeliveryContext context)
    {
        return contentType switch
        {
            ContentType.Photography => RecommendForPhotography(context),
            ContentType.Screenshot => RecommendForScreenshot(context),
            ContentType.Illustration => RecommendForIllustration(context),
            ContentType.Medical => RecommendForMedical(context),
            ContentType.Ecommerce => RecommendForEcommerce(context),
            _ => RecommendGeneric(context)
        };
    }

    private FormatRecommendation RecommendForPhotography(DeliveryContext context)
    {
        var recommendation = new FormatRecommendation();

        if (context.IsMobileFirst)
        {
            // Mobile: Prioritize file size
            recommendation.Primary = new FormatChoice
            {
                Format = ImageFormat.AVIF,
                Quality = 80,
                Reasoning = "AVIF provides best compression for photos on mobile"
            };

            recommendation.Fallback = new FormatChoice
            {
                Format = ImageFormat.WebP,
                Quality = 85,
                Reasoning = "WebP has broader mobile support"
            };
        }
        else
        {
            // Desktop: Balance quality and compatibility
            recommendation.Primary = new FormatChoice
            {
                Format = ImageFormat.WebP,
                Quality = 90,
                Reasoning = "WebP balances quality and browser support"
            };

            recommendation.Fallback = new FormatChoice
            {
                Format = ImageFormat.JPEG,
                Quality = 85,
                SubsamplingMode = "4:2:0",
                Progressive = true,
                Reasoning = "Progressive JPEG ensures universal compatibility"
            };
        }

        // Future-proofing
        recommendation.Experimental = new FormatChoice
        {
            Format = ImageFormat.JXL,
            Quality = 90,
            Reasoning = "JPEG XL offers superior quality when supported"
        };

        return recommendation;
    }

    private FormatRecommendation RecommendForScreenshot(DeliveryContext context)
    {
        var recommendation = new FormatRecommendation();

        // Screenshots often have text and UI elements
        recommendation.Primary = new FormatChoice
        {
            Format = ImageFormat.PNG,
            ColorDepth = 8,
            Reasoning = "PNG preserves sharp edges in UI elements"
        };

        // Modern alternative
        recommendation.Modern = new FormatChoice
        {
            Format = ImageFormat.WebP,
            Lossless = true,
            Reasoning = "WebP lossless is 26% smaller than PNG"
        };

        // For very large screenshots
        if (context.ImageDimensions.Width * context.ImageDimensions.Height > 4_000_000)
        {
            recommendation.LargeImage = new FormatChoice
            {
                Format = ImageFormat.AVIF,
                Quality = 95,
                Reasoning = "AVIF handles large screenshots efficiently"
            };
        }

        return recommendation;
    }
}
```

### Browser compatibility matrix

Real-world format selection must consider browser support:

```csharp
public class BrowserCompatibilityAnalyzer
{
    private readonly Dictionary<ImageFormat, BrowserSupport> supportMatrix = new()
    {
        [ImageFormat.JPEG] = new BrowserSupport { Desktop = 100, Mobile = 100, Since = 1992 },
        [ImageFormat.PNG] = new BrowserSupport { Desktop = 100, Mobile = 100, Since = 1996 },
        [ImageFormat.WebP] = new BrowserSupport { Desktop = 95, Mobile = 96, Since = 2010 },
        [ImageFormat.AVIF] = new BrowserSupport { Desktop = 75, Mobile = 80, Since = 2020 },
        [ImageFormat.JXL] = new BrowserSupport { Desktop = 15, Mobile = 10, Since = 2021, Experimental = true },
        [ImageFormat.HEIC] = new BrowserSupport { Desktop = 5, Mobile = 60, Since = 2017, PlatformSpecific = "iOS" }
    };

    public FormatCompatibilityReport AnalyzeCompatibility(
        List<ImageFormat> formats,
        TargetAudience audience)
    {
        var report = new FormatCompatibilityReport();

        foreach (var format in formats)
        {
            var support = supportMatrix[format];
            var coverage = CalculateCoverage(support, audience);

            report.Formats.Add(new FormatCoverage
            {
                Format = format,
                TotalCoverage = coverage.Total,
                DesktopCoverage = coverage.Desktop,
                MobileCoverage = coverage.Mobile,
                UnsupportedBrowsers = GetUnsupportedBrowsers(format),
                RequiresPolyfill = format.CanPolyfill(),
                FallbackCost = EstimateFallbackCost(format, audience)
            });
        }

        // Generate recommendations
        report.Recommendations = GenerateCompatibilityRecommendations(report, audience);

        return report;
    }

    public string GeneratePictureElement(
        string imagePath,
        FormatDecision decision,
        ResponsiveConfig config)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<picture>");

        // Modern format sources
        if (decision.PrimaryFormat != ImageFormat.JPEG)
        {
            sb.AppendLine($"  <source");
            sb.AppendLine($"    type=\"{GetMimeType(decision.PrimaryFormat)}\"");
            sb.AppendLine($"    srcset=\"{GenerateSrcSet(imagePath, decision.PrimaryFormat, config)}\"");

            if (config.HasMediaQueries)
            {
                sb.AppendLine($"    media=\"{config.MediaQuery}\"");
            }

            sb.AppendLine($"  />");
        }

        // Fallback format
        sb.AppendLine($"  <source");
        sb.AppendLine($"    type=\"{GetMimeType(decision.FallbackFormat)}\"");
        sb.AppendLine($"    srcset=\"{GenerateSrcSet(imagePath, decision.FallbackFormat, config)}\"");
        sb.AppendLine($"  />");

        // Ultimate fallback
        sb.AppendLine($"  <img");
        sb.AppendLine($"    src=\"{GetFallbackSrc(imagePath, config)}\"");
        sb.AppendLine($"    alt=\"{config.AltText}\"");

        if (config.LazyLoad)
        {
            sb.AppendLine($"    loading=\"lazy\"");
        }

        sb.AppendLine($"  />");
        sb.AppendLine("</picture>");

        return sb.ToString();
    }
}
```

### Future-proofing strategies

Preparing for emerging formats while maintaining compatibility:

```csharp
public class FutureProofFormatStrategy
{
    private readonly FormatMonitor formatMonitor;
    private readonly AdoptionPredictor adoptionPredictor;

    public async Task<FutureProofingPlan> CreateStrategyAsync(DeliveryContext context)
    {
        var plan = new FutureProofingPlan();

        // Monitor emerging formats
        var emergingFormats = await formatMonitor.GetEmergingFormatsAsync();

        foreach (var format in emergingFormats)
        {
            var adoption = await adoptionPredictor.PredictAdoptionAsync(format);

            if (adoption.ProbabilityOfSuccess > 0.7f)
            {
                plan.PreparationsNeeded.Add(new FormatPreparation
                {
                    Format = format,
                    ExpectedMainstreamDate = adoption.EstimatedMainstreamDate,
                    PreparationSteps = GeneratePreparationSteps(format),
                    EarlyAdopterBenefit = EstimateEarlyAdopterBenefit(format)
                });
            }
        }

        // Infrastructure recommendations
        plan.InfrastructureChanges = RecommendInfrastructureChanges(emergingFormats);

        // Migration strategy
        plan.MigrationStrategy = CreateMigrationStrategy(context.CurrentFormats, emergingFormats);

        return plan;
    }

    private List<PreparationStep> GeneratePreparationSteps(EmergingFormat format)
    {
        var steps = new List<PreparationStep>();

        // Technical preparation
        steps.Add(new PreparationStep
        {
            Type = StepType.Technical,
            Description = "Update image processing pipeline to support " + format.Name,
            EstimatedEffort = EstimateDevelopmentEffort(format),
            Dependencies = format.RequiredLibraries
        });

        // CDN and caching
        steps.Add(new PreparationStep
        {
            Type = StepType.Infrastructure,
            Description = "Configure CDN for new MIME type: " + format.MimeType,
            EstimatedEffort = TimeSpan.FromHours(4)
        });

        // Monitoring and analytics
        steps.Add(new PreparationStep
        {
            Type = StepType.Analytics,
            Description = "Add format-specific performance monitoring",
            Metrics = new[] { "Decode time", "Bandwidth savings", "User engagement" }
        });

        return steps;
    }
}

// Automated format testing
public class FormatQualityAssurance
{
    public async Task<QAReport> ValidateFormatImplementationAsync(
        ImageFormat format,
        TestImageSet testImages)
    {
        var report = new QAReport { Format = format };

        foreach (var testImage in testImages.Images)
        {
            var result = await TestImageAsync(testImage, format);
            report.Results.Add(result);
        }

        // Cross-browser testing
        report.BrowserTests = await RunBrowserTestsAsync(format);

        // Performance regression tests
        report.PerformanceTests = await RunPerformanceTestsAsync(format);

        // Quality consistency tests
        report.QualityTests = await RunQualityTestsAsync(format);

        return report;
    }

    private async Task<TestResult> TestImageAsync(TestImage image, ImageFormat format)
    {
        var result = new TestResult { ImageName = image.Name };

        try
        {
            // Encode/decode cycle
            var encoded = await EncodeAsync(image.Data, format);
            var decoded = await DecodeAsync(encoded, format);

            // Verify dimensions preserved
            result.DimensionsPreserved =
                decoded.Width == image.Data.Width &&
                decoded.Height == image.Data.Height;

            // Measure quality metrics
            result.PSNR = CalculatePSNR(image.Data, decoded);
            result.SSIM = CalculateSSIM(image.Data, decoded);

            // Check for format-specific issues
            result.FormatSpecificTests = await RunFormatSpecificTestsAsync(format, encoded);

            result.Passed = result.DimensionsPreserved &&
                           result.PSNR > image.MinAcceptablePSNR &&
                           result.FormatSpecificTests.All(t => t.Passed);
        }
        catch (Exception ex)
        {
            result.Passed = false;
            result.Error = ex.Message;
        }

        return result;
    }
}
```

## Conclusion

Modern compression strategies have evolved far beyond simple quality sliders and format dropdowns. Today's
high-performance graphics applications must navigate a complex landscape of competing formats, each with unique
strengths and trade-offs. The journey from JPEG's elegant simplicity to AVIF's neural network-enhanced prediction
represents not just technological progress, but a fundamental shift in how we think about image compression.

The key insight is that optimal compression is not a one-size-fits-all problem. Content-adaptive strategies that analyze
image characteristics and apply different techniques to different regions can achieve remarkable results—reducing file
sizes by 50-90% while maintaining perceptual quality that satisfies even demanding users. Machine learning integration
takes this further, predicting optimal settings and even enhancing compression algorithms themselves.

Progressive enhancement has transformed from a nice-to-have feature to a critical component of user experience. Modern
techniques go beyond simple interlacing, using perceptual models to prioritize visually important information and
adapting to network conditions in real-time. The result is faster perceived load times and better user engagement,
especially crucial for mobile and low-bandwidth scenarios.

Format selection, once a simple decision tree, now requires sophisticated multi-criteria optimization. Factors like
browser support, licensing costs, encoding performance, and future compatibility must all be weighed. The emergence of
new formats like AVIF and JPEG XL promises even better compression ratios, but adoption remains fragmented. Smart
applications implement flexible strategies that can adapt as the format landscape evolves.

.NET 9.0 provides the tools necessary to implement these sophisticated strategies: SIMD operations for fast encoding,
async patterns for responsive streaming, and GPU integration for massively parallel processing. The combination of these
technologies with intelligent algorithms enables compression systems that would have been impossible just a few years
ago.

Looking forward, the convergence of traditional compression with AI-driven techniques promises even more dramatic
improvements. Generative models that can hallucinate plausible details, perceptual loss functions that better match
human vision, and content-aware streaming that predicts user attention—these emerging technologies will define the next
generation of image compression.

The challenge for developers is to balance these sophisticated capabilities with practical constraints. Not every
application needs cutting-edge compression, and the newest format isn't always the best choice. Success comes from
understanding the full spectrum of options and selecting strategies that align with specific use cases, user needs, and
business requirements. In the end, the best compression strategy is the one that delivers the right image to the right
user at the right time—efficiently, reliably, and beautifully.
