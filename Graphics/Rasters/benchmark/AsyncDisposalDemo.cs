// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Diagnostics;
using Wangkanai.Graphics.Rasters.WebPs;
using Wangkanai.Graphics.Rasters.Jpegs;
using Wangkanai.Graphics.Rasters.Tiffs;
using Wangkanai.Graphics.Rasters.Pngs;

namespace Wangkanai.Graphics.Rasters.Benchmark;

/// <summary>Simple demonstration of async disposal performance benefits for large metadata.</summary>
public static class AsyncDisposalDemo
{
	public static async Task RunDemo()
	{
		Console.WriteLine("🚀 Async Disposal Performance Demo");
		Console.WriteLine("====================================");
		Console.WriteLine();

		// Test with small metadata
		Console.WriteLine("📦 Testing Small Metadata (< 1MB):");
		await TestSmallMetadata();
		Console.WriteLine();

		// Test with large metadata  
		Console.WriteLine("📈 Testing Large Metadata (> 1MB):");
		await TestLargeMetadata();
		Console.WriteLine();

		// Test batch disposal
		Console.WriteLine("🔄 Testing Batch Disposal:");
		await TestBatchDisposal();
		Console.WriteLine();

		Console.WriteLine("✅ Demo completed! For detailed benchmarks, run with --async flag.");
	}

	private static async Task TestSmallMetadata()
	{
		const int iterations = 1000;
		
		// Test WebP with small metadata
		var sw = Stopwatch.StartNew();
		for (int i = 0; i < iterations; i++)
		{
			var webp = new WebPRaster(800, 600);
			webp.Metadata.IccProfile = new byte[1000]; // 1KB
			webp.Dispose();
		}
		sw.Stop();
		var syncTime = sw.ElapsedMilliseconds;

		sw.Restart();
		for (int i = 0; i < iterations; i++)
		{
			var webp = new WebPRaster(800, 600);
			webp.Metadata.IccProfile = new byte[1000]; // 1KB
			await webp.DisposeAsync();
		}
		sw.Stop();
		var asyncTime = sw.ElapsedMilliseconds;

		Console.WriteLine($"  WebP Small Metadata ({iterations:N0} iterations):");
		Console.WriteLine($"    Sync Disposal:  {syncTime,4} ms");
		Console.WriteLine($"    Async Disposal: {asyncTime,4} ms");
		Console.WriteLine($"    Overhead:       {asyncTime - syncTime,4} ms ({(double)(asyncTime - syncTime) / syncTime * 100:F1}%)");
	}

	private static async Task TestLargeMetadata()
	{
		const int iterations = 100;
		
		// Test WebP with large metadata
		var sw = Stopwatch.StartNew();
		for (int i = 0; i < iterations; i++)
		{
			var webp = new WebPRaster(4096, 4096);
			webp.Metadata.IccProfile = new byte[2_000_000]; // 2MB
			webp.Metadata.ExifData = new byte[500_000];     // 500KB
			webp.Metadata.XmpData = new byte[300_000];      // 300KB
			webp.Dispose();
		}
		sw.Stop();
		var syncTime = sw.ElapsedMilliseconds;

		sw.Restart();
		for (int i = 0; i < iterations; i++)
		{
			var webp = new WebPRaster(4096, 4096);
			webp.Metadata.IccProfile = new byte[2_000_000]; // 2MB
			webp.Metadata.ExifData = new byte[500_000];     // 500KB
			webp.Metadata.XmpData = new byte[300_000];      // 300KB
			await webp.DisposeAsync();
		}
		sw.Stop();
		var asyncTime = sw.ElapsedMilliseconds;

		Console.WriteLine($"  WebP Large Metadata ({iterations:N0} iterations):");
		Console.WriteLine($"    Sync Disposal:  {syncTime,4} ms");
		Console.WriteLine($"    Async Disposal: {asyncTime,4} ms");
		Console.WriteLine($"    Difference:     {asyncTime - syncTime,4} ms");
		
		// Test TIFF with large metadata
		sw.Restart();
		for (int i = 0; i < iterations; i++)
		{
			var tiff = new TiffRaster(4096, 4096);
			tiff.Metadata.ImageDescription = new string('A', 500_000); // 500KB
			for (int j = 0; j < 1000; j++)
			{
				tiff.Metadata.CustomTags.Add(j, $"Tag {j}");
			}
			tiff.Dispose();
		}
		sw.Stop();
		var tiffSyncTime = sw.ElapsedMilliseconds;

		sw.Restart();
		for (int i = 0; i < iterations; i++)
		{
			var tiff = new TiffRaster(4096, 4096);
			tiff.Metadata.ImageDescription = new string('A', 500_000); // 500KB
			for (int j = 0; j < 1000; j++)
			{
				tiff.Metadata.CustomTags.Add(j, $"Tag {j}");
			}
			await tiff.DisposeAsync();
		}
		sw.Stop();
		var tiffAsyncTime = sw.ElapsedMilliseconds;

		Console.WriteLine($"  TIFF Large Metadata ({iterations:N0} iterations):");
		Console.WriteLine($"    Sync Disposal:  {tiffSyncTime,4} ms");
		Console.WriteLine($"    Async Disposal: {tiffAsyncTime,4} ms");
		Console.WriteLine($"    Difference:     {tiffAsyncTime - tiffSyncTime,4} ms");
	}

	private static async Task TestBatchDisposal()
	{
		const int batchSize = 50;
		
		// Create mixed format images with large metadata
		var images = new List<object>();
		for (int i = 0; i < batchSize; i++)
		{
			if (i % 4 == 0)
			{
				var webp = new WebPRaster(2048, 2048);
				webp.Metadata.IccProfile = new byte[1_000_000]; // 1MB
				images.Add(webp);
			}
			else if (i % 4 == 1)
			{
				var jpeg = new JpegRaster(2048, 2048);
				jpeg.Metadata.IccProfile = new byte[800_000]; // 800KB
				for (int j = 0; j < 1000; j++)
				{
					jpeg.Metadata.CustomExifTags.Add(j, $"EXIF {j}");
				}
				images.Add(jpeg);
			}
			else if (i % 4 == 2)
			{
				var tiff = new TiffRaster(2048, 2048);
				tiff.Metadata.ImageDescription = new string('B', 300_000); // 300KB
				images.Add(tiff);
			}
			else
			{
				var png = new PngRaster(2048, 2048);
				for (int j = 0; j < 1000; j++)
				{
					png.Metadata.TextChunks.Add($"key{j}", new string('C', 100));
				}
				images.Add(png);
			}
		}

		// Test sequential sync disposal
		var sw = Stopwatch.StartNew();
		foreach (var image in images)
		{
			switch (image)
			{
				case WebPRaster webp:
					webp.Dispose();
					break;
				case JpegRaster jpeg:
					jpeg.Dispose();
					break;
				case TiffRaster tiff:
					tiff.Dispose();
					break;
				case PngRaster png:
					png.Dispose();
					break;
			}
		}
		sw.Stop();
		var sequentialSyncTime = sw.ElapsedMilliseconds;

		// Recreate images for next test
		images.Clear();
		for (int i = 0; i < batchSize; i++)
		{
			if (i % 4 == 0)
			{
				var webp = new WebPRaster(2048, 2048);
				webp.Metadata.IccProfile = new byte[1_000_000];
				images.Add(webp);
			}
			else if (i % 4 == 1)
			{
				var jpeg = new JpegRaster(2048, 2048);
				jpeg.Metadata.IccProfile = new byte[800_000];
				images.Add(jpeg);
			}
			else if (i % 4 == 2)
			{
				var tiff = new TiffRaster(2048, 2048);
				tiff.Metadata.ImageDescription = new string('B', 300_000);
				images.Add(tiff);
			}
			else
			{
				var png = new PngRaster(2048, 2048);
				for (int j = 0; j < 1000; j++)
				{
					png.Metadata.TextChunks.Add($"key{j}", new string('C', 100));
				}
				images.Add(png);
			}
		}

		// Test sequential async disposal
		sw.Restart();
		foreach (var image in images)
		{
			switch (image)
			{
				case WebPRaster webp:
					await webp.DisposeAsync();
					break;
				case JpegRaster jpeg:
					await jpeg.DisposeAsync();
					break;
				case TiffRaster tiff:
					await tiff.DisposeAsync();
					break;
				case PngRaster png:
					await png.DisposeAsync();
					break;
			}
		}
		sw.Stop();
		var sequentialAsyncTime = sw.ElapsedMilliseconds;

		Console.WriteLine($"  Sequential Disposal ({batchSize} mixed images):");
		Console.WriteLine($"    Sync Sequential:  {sequentialSyncTime,4} ms");
		Console.WriteLine($"    Async Sequential: {sequentialAsyncTime,4} ms");
		Console.WriteLine($"    Async Benefit:    {sequentialSyncTime - sequentialAsyncTime,4} ms");
		
		if (sequentialAsyncTime < sequentialSyncTime)
		{
			Console.WriteLine($"    🎉 Async disposal is {(double)sequentialSyncTime / sequentialAsyncTime:F1}x faster for large metadata!");
		}
	}
}