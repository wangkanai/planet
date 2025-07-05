// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

using System.Diagnostics;

using Wangkanai.Graphics.Rasters.Tiffs;

namespace Wangkanai.Graphics.Rasters.Benchmark;

/// <summary>Simple performance demonstration showing the benefits of ReadOnlySpan optimization.</summary>
public static class PerformanceDemo
{
	public static void RunDemo()
	{
		Console.WriteLine("=== TIFF BitsPerSample Performance Comparison ===\n");

		// Test data for common TIFF scenarios
		var testCases = new[]
		{
			("Grayscale", new[] { 8 }),
			("RGB", new[] { 8, 8, 8 }),
			("RGBA", new[] { 8, 8, 8, 8 }),
			("CMYK", new[] { 8, 8, 8, 8 }),
			("Large", new[] { 8, 8, 8, 8, 8, 8, 8, 8 })
		};

		foreach (var (name, bits) in testCases)
		{
			Console.WriteLine($"--- {name} ({bits.Length} samples) ---");
			TestScenario(name, bits, iterations: 100_000);
			Console.WriteLine();
		}

		Console.WriteLine("--- Memory Allocation Test ---");
		TestMemoryAllocations(iterations: 10_000);
		Console.WriteLine();

		Console.WriteLine("--- Access Performance Test ---");
		TestAccessPerformance(iterations: 1_000_000);
	}

	private static void TestScenario(string name, int[] bits, int iterations)
	{
		// Baseline test
		var baselineTime = MeasureTime(() =>
		{
			for (int i = 0; i < iterations; i++)
			{
				var raster = new TiffRasterBaseline();
				raster.SetBitsPerSample(bits);
				var span = raster.BitsPerSample;
				var sum = 0;
				for (int j = 0; j < span.Length; j++)
				{
					sum += span[j];
				}
			}
		});

		// Optimized test
		var optimizedTime = MeasureTime(() =>
		{
			for (int i = 0; i < iterations; i++)
			{
				var raster = new TiffRaster();
				raster.SetBitsPerSample(bits);
				var span = raster.BitsPerSample;
				var sum = 0;
				for (int j = 0; j < span.Length; j++)
				{
					sum += span[j];
				}
			}
		});

		var improvement = (baselineTime - optimizedTime) / (double)baselineTime * 100;
		Console.WriteLine($"  Baseline:  {baselineTime:F2} ms");
		Console.WriteLine($"  Optimized: {optimizedTime:F2} ms");
		Console.WriteLine($"  Improvement: {improvement:F1}%");
	}

	private static void TestMemoryAllocations(int iterations)
	{
		// Force GC before measurement
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		var baselineMemoryBefore = GC.GetTotalMemory(false);
		for (int i = 0; i < iterations; i++)
		{
			var raster = new TiffRasterBaseline();
			raster.SetBitsPerSample(new[] { 8, 8, 8 });
		}
		var baselineMemoryAfter = GC.GetTotalMemory(false);
		var baselineAllocated = baselineMemoryAfter - baselineMemoryBefore;

		// Force GC before optimized measurement
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		var optimizedMemoryBefore = GC.GetTotalMemory(false);
		for (int i = 0; i < iterations; i++)
		{
			var raster = new TiffRaster();
			raster.SetBitsPerSample(new[] { 8, 8, 8 });
		}
		var optimizedMemoryAfter = GC.GetTotalMemory(false);
		var optimizedAllocated = optimizedMemoryAfter - optimizedMemoryBefore;

		var memoryReduction = (baselineAllocated - optimizedAllocated) / (double)baselineAllocated * 100;
		Console.WriteLine($"  Baseline allocated:  {baselineAllocated:N0} bytes");
		Console.WriteLine($"  Optimized allocated: {optimizedAllocated:N0} bytes");
		Console.WriteLine($"  Memory reduction: {memoryReduction:F1}%");
	}

	private static void TestAccessPerformance(int iterations)
	{
		var baselineRaster = new TiffRasterBaseline();
		baselineRaster.SetBitsPerSample(new[] { 8, 8, 8 });

		var optimizedRaster = new TiffRaster();
		optimizedRaster.SetBitsPerSample(new[] { 8, 8, 8 });

		// Baseline access test
		var baselineTime = MeasureTime(() =>
		{
			var sum = 0;
			for (int i = 0; i < iterations; i++)
			{
				var span = baselineRaster.BitsPerSample;
				for (int j = 0; j < span.Length; j++)
				{
					sum += span[j];
				}
			}
		});

		// Optimized access test
		var optimizedTime = MeasureTime(() =>
		{
			var sum = 0;
			for (int i = 0; i < iterations; i++)
			{
				var span = optimizedRaster.BitsPerSample;
				for (int j = 0; j < span.Length; j++)
				{
					sum += span[j];
				}
			}
		});

		var improvement = (baselineTime - optimizedTime) / (double)baselineTime * 100;
		Console.WriteLine($"  Baseline:  {baselineTime:F2} ms");
		Console.WriteLine($"  Optimized: {optimizedTime:F2} ms");
		Console.WriteLine($"  Improvement: {improvement:F1}%");
	}

	private static double MeasureTime(Action action)
	{
		var stopwatch = Stopwatch.StartNew();
		action();
		stopwatch.Stop();
		return stopwatch.Elapsed.TotalMilliseconds;
	}
}
