// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Diagnostics;
using Wangkanai.Planet.Drawing.Rasters.Tiffs;

namespace Wangkanai.Planet.Drawing.Rasters.Benchmark;

/// <summary>Realistic performance demonstration showing real-world benefits of ReadOnlySpan optimization.</summary>
public static class RealisticPerformanceDemo
{
	public static void RunDemo()
	{
		Console.WriteLine("=== Realistic TIFF Processing Performance Analysis ===\n");

		Console.WriteLine("--- Real-World Memory Pressure Test ---");
		TestRealWorldMemoryPressure();
		Console.WriteLine();

		Console.WriteLine("--- TIFF Processing Pipeline Simulation ---");
		TestTiffProcessingPipeline();
		Console.WriteLine();

		Console.WriteLine("--- Object Creation Performance ---");
		TestObjectCreationPerformance();
		Console.WriteLine();

		Console.WriteLine("--- Validation Performance ---");
		TestValidationPerformance();
	}

	private static void TestRealWorldMemoryPressure()
	{
		const int iterations = 50_000;
		
		// Simulate processing many TIFF files with different configurations
		Console.WriteLine($"Processing {iterations:N0} TIFF files (mixed types)...");

		// Force GC before baseline test
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		var baselineMemoryBefore = GC.GetTotalMemory(false);
		var baselineTime = MeasureTime(() =>
		{
			for (int i = 0; i < iterations; i++)
			{
				var imageType = i % 4;
				var raster = new TiffRasterBaseline();
				
				switch (imageType)
				{
					case 0: // Grayscale
						raster.SetBitsPerSample(new[] { 8 });
						break;
					case 1: // RGB
						raster.SetBitsPerSample(new[] { 8, 8, 8 });
						break;
					case 2: // RGBA
						raster.SetBitsPerSample(new[] { 8, 8, 8, 8 });
						break;
					case 3: // CMYK
						raster.SetBitsPerSample(new[] { 8, 8, 8, 8 });
						break;
				}

				// Simulate some processing
				var bits = raster.BitsPerSample;
				var isValid = TiffValidator.IsValidBitsPerSample(bits, bits.Length);
			}
		});
		var baselineMemoryAfter = GC.GetTotalMemory(false);
		var baselineAllocated = baselineMemoryAfter - baselineMemoryBefore;

		// Force GC before optimized test
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		var optimizedMemoryBefore = GC.GetTotalMemory(false);
		var optimizedTime = MeasureTime(() =>
		{
			for (int i = 0; i < iterations; i++)
			{
				var imageType = i % 4;
				var raster = new TiffRaster();
				
				switch (imageType)
				{
					case 0: // Grayscale
						raster.SetBitsPerSample(new[] { 8 });
						break;
					case 1: // RGB
						raster.SetBitsPerSample(new[] { 8, 8, 8 });
						break;
					case 2: // RGBA
						raster.SetBitsPerSample(new[] { 8, 8, 8, 8 });
						break;
					case 3: // CMYK
						raster.SetBitsPerSample(new[] { 8, 8, 8, 8 });
						break;
				}

				// Simulate some processing
				var bits = raster.BitsPerSample;
				var isValid = TiffValidator.IsValidBitsPerSample(bits, bits.Length);
			}
		});
		var optimizedMemoryAfter = GC.GetTotalMemory(false);
		var optimizedAllocated = optimizedMemoryAfter - optimizedMemoryBefore;

		var memoryReduction = (baselineAllocated - optimizedAllocated) / (double)baselineAllocated * 100;
		var timeImprovement = (baselineTime - optimizedTime) / (double)baselineTime * 100;

		Console.WriteLine($"  Baseline:  {baselineTime:F0} ms, {baselineAllocated:N0} bytes allocated");
		Console.WriteLine($"  Optimized: {optimizedTime:F0} ms, {optimizedAllocated:N0} bytes allocated");
		Console.WriteLine($"  Memory reduction: {memoryReduction:F1}%");
		Console.WriteLine($"  Time improvement: {timeImprovement:F1}%");
	}

	private static void TestTiffProcessingPipeline()
	{
		const int iterations = 10_000;
		
		Console.WriteLine($"Simulating TIFF processing pipeline ({iterations:N0} images)...");

		var baselineTime = MeasureTime(() =>
		{
			for (int i = 0; i < iterations; i++)
			{
				// Create multiple rasters for a multi-page TIFF
				var rasters = new TiffRasterBaseline[5];
				for (int j = 0; j < 5; j++)
				{
					rasters[j] = new TiffRasterBaseline();
					rasters[j].SetBitsPerSample(new[] { 8, 8, 8 });
					
					// Simulate metadata processing
					var bits = rasters[j].BitsPerSample;
					var totalBits = 0;
					for (int k = 0; k < bits.Length; k++)
					{
						totalBits += bits[k];
					}
				}
			}
		});

		var optimizedTime = MeasureTime(() =>
		{
			for (int i = 0; i < iterations; i++)
			{
				// Create multiple rasters for a multi-page TIFF
				var rasters = new TiffRaster[5];
				for (int j = 0; j < 5; j++)
				{
					rasters[j] = new TiffRaster();
					rasters[j].SetBitsPerSample(new[] { 8, 8, 8 });
					
					// Simulate metadata processing
					var bits = rasters[j].BitsPerSample;
					var totalBits = 0;
					for (int k = 0; k < bits.Length; k++)
					{
						totalBits += bits[k];
					}
				}
			}
		});

		var improvement = (baselineTime - optimizedTime) / (double)baselineTime * 100;
		Console.WriteLine($"  Baseline:  {baselineTime:F0} ms");
		Console.WriteLine($"  Optimized: {optimizedTime:F0} ms");
		Console.WriteLine($"  Improvement: {improvement:F1}%");
	}

	private static void TestObjectCreationPerformance()
	{
		const int iterations = 100_000;
		
		Console.WriteLine($"Object creation test ({iterations:N0} objects)...");

		var baselineTime = MeasureTime(() =>
		{
			for (int i = 0; i < iterations; i++)
			{
				var raster = new TiffRasterBaseline();
				// Default constructor already sets RGB bits
			}
		});

		var optimizedTime = MeasureTime(() =>
		{
			for (int i = 0; i < iterations; i++)
			{
				var raster = new TiffRaster();
				// Default constructor already sets RGB bits
			}
		});

		var improvement = (baselineTime - optimizedTime) / (double)baselineTime * 100;
		Console.WriteLine($"  Baseline:  {baselineTime:F0} ms");
		Console.WriteLine($"  Optimized: {optimizedTime:F0} ms");
		Console.WriteLine($"  Improvement: {improvement:F1}%");
	}

	private static void TestValidationPerformance()
	{
		const int iterations = 1_000_000;
		
		Console.WriteLine($"Validation performance test ({iterations:N0} validations)...");

		var baselineRaster = new TiffRasterBaseline();
		baselineRaster.SetBitsPerSample(new[] { 8, 8, 8 });

		var optimizedRaster = new TiffRaster();
		optimizedRaster.SetBitsPerSample(new[] { 8, 8, 8 });

		var baselineTime = MeasureTime(() =>
		{
			for (int i = 0; i < iterations; i++)
			{
				var isValid = TiffValidator.IsValidBitsPerSample(baselineRaster.BitsPerSample, 3);
			}
		});

		var optimizedTime = MeasureTime(() =>
		{
			for (int i = 0; i < iterations; i++)
			{
				var isValid = TiffValidator.IsValidBitsPerSample(optimizedRaster.BitsPerSample, 3);
			}
		});

		var improvement = (baselineTime - optimizedTime) / (double)baselineTime * 100;
		Console.WriteLine($"  Baseline:  {baselineTime:F0} ms");
		Console.WriteLine($"  Optimized: {optimizedTime:F0} ms");
		Console.WriteLine($"  Improvement: {improvement:F1}%");
	}

	private static double MeasureTime(Action action)
	{
		// Warm up
		action();
		
		// Measure
		var stopwatch = Stopwatch.StartNew();
		action();
		stopwatch.Stop();
		return stopwatch.Elapsed.TotalMilliseconds;
	}
}