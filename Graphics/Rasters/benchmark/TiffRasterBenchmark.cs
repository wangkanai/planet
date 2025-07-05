// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;

using Wangkanai.Graphics.Rasters.Tiffs;

namespace Wangkanai.Graphics.Rasters.Benchmark;

/// <summary>Comprehensive benchmarks comparing ReadOnlySpan&lt;int&gt; optimization vs int[] baseline for TiffRaster BitsPerSample.</summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class TiffRasterBenchmark
{
	private TiffRaster _optimizedRaster = null!;
	private TiffRasterBaseline _baselineRaster = null!;

	private readonly int[] _grayscaleBits = { 8 };
	private readonly int[] _rgbBits = { 8, 8, 8 };
	private readonly int[] _rgbaBits = { 8, 8, 8, 8 };
	private readonly int[] _cmykBits = { 8, 8, 8, 8 };
	private readonly int[] _largeBits = { 8, 8, 8, 8, 8, 8, 8, 8 };

	[GlobalSetup]
	public void Setup()
	{
		_optimizedRaster = new TiffRaster();
		_baselineRaster = new TiffRasterBaseline();
	}

	// ===================================================================================
	// CREATION BENCHMARKS - Measure object creation and initialization costs
	// ===================================================================================

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("Creation")]
	public TiffRasterBaseline CreateBaseline_RGB()
	{
		var raster = new TiffRasterBaseline();
		raster.SetBitsPerSample(_rgbBits);
		return raster;
	}

	[Benchmark]
	[BenchmarkCategory("Creation")]
	public TiffRaster CreateOptimized_RGB()
	{
		var raster = new TiffRaster();
		raster.SetBitsPerSample(_rgbBits);
		return raster;
	}

	// ===================================================================================
	// SETTING BENCHMARKS - Measure SetBitsPerSample performance for different sample counts
	// ===================================================================================

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("Set")]
	public void SetBitsPerSample_Baseline_Grayscale()
	{
		_baselineRaster.SetBitsPerSample(_grayscaleBits);
	}

	[Benchmark]
	[BenchmarkCategory("Set")]
	public void SetBitsPerSample_Optimized_Grayscale()
	{
		_optimizedRaster.SetBitsPerSample(_grayscaleBits);
	}

	[Benchmark]
	[BenchmarkCategory("Set")]
	public void SetBitsPerSample_Baseline_RGB()
	{
		_baselineRaster.SetBitsPerSample(_rgbBits);
	}

	[Benchmark]
	[BenchmarkCategory("Set")]
	public void SetBitsPerSample_Optimized_RGB()
	{
		_optimizedRaster.SetBitsPerSample(_rgbBits);
	}

	[Benchmark]
	[BenchmarkCategory("Set")]
	public void SetBitsPerSample_Baseline_RGBA()
	{
		_baselineRaster.SetBitsPerSample(_rgbaBits);
	}

	[Benchmark]
	[BenchmarkCategory("Set")]
	public void SetBitsPerSample_Optimized_RGBA()
	{
		_optimizedRaster.SetBitsPerSample(_rgbaBits);
	}

	[Benchmark]
	[BenchmarkCategory("Set")]
	public void SetBitsPerSample_Baseline_Large()
	{
		_baselineRaster.SetBitsPerSample(_largeBits);
	}

	[Benchmark]
	[BenchmarkCategory("Set")]
	public void SetBitsPerSample_Optimized_Large()
	{
		_optimizedRaster.SetBitsPerSample(_largeBits);
	}

	// ===================================================================================
	// ACCESS BENCHMARKS - Measure BitsPerSample property access performance
	// ===================================================================================

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("Access")]
	public ReadOnlySpan<int> AccessBitsPerSample_Baseline_RGB()
	{
		_baselineRaster.SetBitsPerSample(_rgbBits);
		return _baselineRaster.BitsPerSample;
	}

	[Benchmark]
	[BenchmarkCategory("Access")]
	public ReadOnlySpan<int> AccessBitsPerSample_Optimized_RGB()
	{
		_optimizedRaster.SetBitsPerSample(_rgbBits);
		return _optimizedRaster.BitsPerSample;
	}

	[Benchmark]
	[BenchmarkCategory("Access")]
	public ReadOnlySpan<int> AccessBitsPerSample_Baseline_Grayscale()
	{
		_baselineRaster.SetBitsPerSample(_grayscaleBits);
		return _baselineRaster.BitsPerSample;
	}

	[Benchmark]
	[BenchmarkCategory("Access")]
	public ReadOnlySpan<int> AccessBitsPerSample_Optimized_Grayscale()
	{
		_optimizedRaster.SetBitsPerSample(_grayscaleBits);
		return _optimizedRaster.BitsPerSample;
	}

	// ===================================================================================
	// REPEATED ACCESS BENCHMARKS - Measure performance under high-frequency access patterns
	// ===================================================================================

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("RepeatedAccess")]
	public int RepeatedAccess_Baseline_RGB()
	{
		_baselineRaster.SetBitsPerSample(_rgbBits);
		var sum = 0;
		for (int i = 0; i < 1000; i++)
		{
			var bits = _baselineRaster.BitsPerSample;
			for (int j = 0; j < bits.Length; j++)
			{
				sum += bits[j];
			}
		}
		return sum;
	}

	[Benchmark]
	[BenchmarkCategory("RepeatedAccess")]
	public int RepeatedAccess_Optimized_RGB()
	{
		_optimizedRaster.SetBitsPerSample(_rgbBits);
		var sum = 0;
		for (int i = 0; i < 1000; i++)
		{
			var bits = _optimizedRaster.BitsPerSample;
			for (int j = 0; j < bits.Length; j++)
			{
				sum += bits[j];
			}
		}
		return sum;
	}

	// ===================================================================================
	// VALIDATION BENCHMARKS - Measure TiffValidator performance with both implementations
	// ===================================================================================

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("Validation")]
	public bool ValidateBitsPerSample_Baseline_RGB()
	{
		_baselineRaster.SetBitsPerSample(_rgbBits);
		return TiffValidator.IsValidBitsPerSample(_baselineRaster.BitsPerSample, 3);
	}

	[Benchmark]
	[BenchmarkCategory("Validation")]
	public bool ValidateBitsPerSample_Optimized_RGB()
	{
		_optimizedRaster.SetBitsPerSample(_rgbBits);
		return TiffValidator.IsValidBitsPerSample(_optimizedRaster.BitsPerSample, 3);
	}

	// ===================================================================================
	// MEMORY ALLOCATION BENCHMARKS - Measure GC pressure and allocation patterns
	// ===================================================================================

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("Memory")]
	public TiffRasterBaseline[] CreateMultiple_Baseline()
	{
		var rasters = new TiffRasterBaseline[100];
		for (int i = 0; i < 100; i++)
		{
			rasters[i] = new TiffRasterBaseline();
			rasters[i].SetBitsPerSample(_rgbBits);
		}
		return rasters;
	}

	[Benchmark]
	[BenchmarkCategory("Memory")]
	public TiffRaster[] CreateMultiple_Optimized()
	{
		var rasters = new TiffRaster[100];
		for (int i = 0; i < 100; i++)
		{
			rasters[i] = new TiffRaster();
			rasters[i].SetBitsPerSample(_rgbBits);
		}
		return rasters;
	}

	// ===================================================================================
	// MIXED WORKLOAD BENCHMARKS - Simulate real-world usage patterns
	// ===================================================================================

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("MixedWorkload")]
	public int MixedWorkload_Baseline()
	{
		var raster = new TiffRasterBaseline();
		var sum = 0;

		// Simulate typical TIFF processing workflow
		for (int i = 0; i < 10; i++)
		{
			// Create different image types
			raster.SetBitsPerSample(_grayscaleBits);
			sum += ProcessBitsPerSample(raster.BitsPerSample);

			raster.SetBitsPerSample(_rgbBits);
			sum += ProcessBitsPerSample(raster.BitsPerSample);

			raster.SetBitsPerSample(_rgbaBits);
			sum += ProcessBitsPerSample(raster.BitsPerSample);

			raster.SetBitsPerSample(_cmykBits);
			sum += ProcessBitsPerSample(raster.BitsPerSample);
		}

		return sum;
	}

	[Benchmark]
	[BenchmarkCategory("MixedWorkload")]
	public int MixedWorkload_Optimized()
	{
		var raster = new TiffRaster();
		var sum = 0;

		// Simulate typical TIFF processing workflow
		for (int i = 0; i < 10; i++)
		{
			// Create different image types
			raster.SetBitsPerSample(_grayscaleBits);
			sum += ProcessBitsPerSample(raster.BitsPerSample);

			raster.SetBitsPerSample(_rgbBits);
			sum += ProcessBitsPerSample(raster.BitsPerSample);

			raster.SetBitsPerSample(_rgbaBits);
			sum += ProcessBitsPerSample(raster.BitsPerSample);

			raster.SetBitsPerSample(_cmykBits);
			sum += ProcessBitsPerSample(raster.BitsPerSample);
		}

		return sum;
	}

	/// <summary>Helper method to simulate processing bits per sample data.</summary>
	private static int ProcessBitsPerSample(ReadOnlySpan<int> bitsPerSample)
	{
		var sum = 0;
		foreach (var bits in bitsPerSample)
		{
			sum += bits * 2; // Simulate some computation
		}
		return sum;
	}
}
