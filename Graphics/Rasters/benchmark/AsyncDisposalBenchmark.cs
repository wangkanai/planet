// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;

using Wangkanai.Graphics.Rasters.WebPs;
using Wangkanai.Graphics.Rasters.Jpegs;
using Wangkanai.Graphics.Rasters.Tiffs;
using Wangkanai.Graphics.Rasters.Pngs;

namespace Wangkanai.Graphics.Rasters.Benchmark;

/// <summary>Benchmarks for async disposal performance across different raster formats and metadata sizes.</summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class AsyncDisposalBenchmark
{
	private WebPRaster _webpSmall = null!;
	private WebPRaster _webpLarge = null!;
	private JpegRaster _jpegSmall = null!;
	private JpegRaster _jpegLarge = null!;
	private TiffRaster _tiffSmall = null!;
	private TiffRaster _tiffLarge = null!;
	private PngRaster _pngSmall = null!;
	private PngRaster _pngLarge = null!;

	[GlobalSetup]
	public void Setup()
	{
		// Setup small metadata versions
		_webpSmall = new WebPRaster(800, 600);
		_webpSmall.Metadata.IccProfile = new byte[1000]; // 1KB

		_jpegSmall = new JpegRaster(800, 600);
		_jpegSmall.Metadata.IccProfile = new byte[1000]; // 1KB

		_tiffSmall = new TiffRaster(800, 600);
		_tiffSmall.Metadata.ImageDescription = "Small test image";

		_pngSmall = new PngRaster(800, 600);
		_pngSmall.Metadata.TextChunks.Add("Description", "Small test image");

		// Setup large metadata versions
		_webpLarge = new WebPRaster(4096, 4096);
		_webpLarge.Metadata.IccProfile = new byte[2_000_000]; // 2MB
		_webpLarge.Metadata.ExifData = new byte[500_000];     // 500KB
		_webpLarge.Metadata.XmpData = new byte[300_000];      // 300KB
		
		// Add many animation frames for large WebP
		for (int i = 0; i < 100; i++)
		{
			_webpLarge.Metadata.AnimationFrames.Add(new WebPAnimationFrame { Data = new byte[10_000] });
		}
		_webpLarge.Metadata.HasAnimation = true;

		_jpegLarge = new JpegRaster(4096, 4096);
		_jpegLarge.Metadata.IccProfile = new byte[1_500_000]; // 1.5MB
		
		// Add many EXIF tags
		for (int i = 0; i < 10_000; i++)
		{
			_jpegLarge.Metadata.CustomExifTags.Add(i, $"Large EXIF tag value {i} with substantial content");
		}

		_tiffLarge = new TiffRaster(4096, 4096);
		_tiffLarge.Metadata.ImageDescription = new string('A', 500_000); // 500KB string
		_tiffLarge.Metadata.Make = "Test Camera Manufacturer";
		_tiffLarge.Metadata.Model = "Test Camera Model XYZ";
		
		// Add many custom tags
		for (int i = 0; i < 5_000; i++)
		{
			_tiffLarge.Metadata.CustomTags.Add(i, $"Large custom tag {i}");
		}

		_pngLarge = new PngRaster(4096, 4096);
		
		// Add many text chunks to create large metadata
		for (int i = 0; i < 5_000; i++)
		{
			_pngLarge.Metadata.TextChunks.Add($"key{i}", new string('X', 200)); // 200 chars each
		}
		
		// Add custom chunks
		for (int i = 0; i < 500; i++)
		{
			_pngLarge.Metadata.CustomChunks.Add($"CHNK{i:D4}", new byte[2000]);
		}
	}

	// ===================================================================================
	// SYNCHRONOUS DISPOSAL BENCHMARKS - Baseline performance
	// ===================================================================================

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("SmallMetadata")]
	public void SyncDisposal_WebP_Small()
	{
		var webp = new WebPRaster(800, 600);
		webp.Metadata.IccProfile = new byte[1000];
		webp.Dispose();
	}

	[Benchmark]
	[BenchmarkCategory("SmallMetadata")]
	public async Task AsyncDisposal_WebP_Small()
	{
		var webp = new WebPRaster(800, 600);
		webp.Metadata.IccProfile = new byte[1000];
		await webp.DisposeAsync();
	}

	[Benchmark]
	[BenchmarkCategory("SmallMetadata")]
	public void SyncDisposal_JPEG_Small()
	{
		var jpeg = new JpegRaster(800, 600);
		jpeg.Metadata.IccProfile = new byte[1000];
		jpeg.Dispose();
	}

	[Benchmark]
	[BenchmarkCategory("SmallMetadata")]
	public async Task AsyncDisposal_JPEG_Small()
	{
		var jpeg = new JpegRaster(800, 600);
		jpeg.Metadata.IccProfile = new byte[1000];
		await jpeg.DisposeAsync();
	}

	// ===================================================================================
	// LARGE METADATA DISPOSAL BENCHMARKS - Test async disposal benefits
	// ===================================================================================

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("LargeMetadata")]
	public void SyncDisposal_WebP_Large()
	{
		var webp = new WebPRaster(4096, 4096);
		webp.Metadata.IccProfile = new byte[2_000_000];
		webp.Metadata.ExifData = new byte[500_000];
		webp.Metadata.XmpData = new byte[300_000];
		webp.Dispose();
	}

	[Benchmark]
	[BenchmarkCategory("LargeMetadata")]
	public async Task AsyncDisposal_WebP_Large()
	{
		var webp = new WebPRaster(4096, 4096);
		webp.Metadata.IccProfile = new byte[2_000_000];
		webp.Metadata.ExifData = new byte[500_000];
		webp.Metadata.XmpData = new byte[300_000];
		await webp.DisposeAsync();
	}

	[Benchmark]
	[BenchmarkCategory("LargeMetadata")]
	public void SyncDisposal_TIFF_Large()
	{
		var tiff = new TiffRaster(4096, 4096);
		tiff.Metadata.ImageDescription = new string('A', 500_000);
		tiff.Dispose();
	}

	[Benchmark]
	[BenchmarkCategory("LargeMetadata")]
	public async Task AsyncDisposal_TIFF_Large()
	{
		var tiff = new TiffRaster(4096, 4096);
		tiff.Metadata.ImageDescription = new string('A', 500_000);
		await tiff.DisposeAsync();
	}

	// ===================================================================================
	// BATCH DISPOSAL BENCHMARKS - Test disposal of multiple images
	// ===================================================================================

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("BatchDisposal")]
	public void SyncBatchDisposal_Mixed()
	{
		var webp = new WebPRaster(1920, 1080);
		var jpeg = new JpegRaster(1920, 1080);
		var tiff = new TiffRaster(1920, 1080);
		var png = new PngRaster(1920, 1080);

		webp.Dispose();
		jpeg.Dispose();
		tiff.Dispose();
		png.Dispose();
	}

	[Benchmark]
	[BenchmarkCategory("BatchDisposal")]
	public async Task AsyncBatchDisposal_Mixed()
	{
		var webp = new WebPRaster(1920, 1080);
		var jpeg = new JpegRaster(1920, 1080);
		var tiff = new TiffRaster(1920, 1080);
		var png = new PngRaster(1920, 1080);

		await webp.DisposeAsync();
		await jpeg.DisposeAsync();
		await tiff.DisposeAsync();
		await png.DisposeAsync();
	}

	[Benchmark]
	[BenchmarkCategory("BatchDisposal")]
	public async Task AsyncBatchDisposal_Concurrent()
	{
		var webp = new WebPRaster(1920, 1080);
		var jpeg = new JpegRaster(1920, 1080);
		var tiff = new TiffRaster(1920, 1080);
		var png = new PngRaster(1920, 1080);

		var tasks = new Task[]
		{
			webp.DisposeAsync().AsTask(),
			jpeg.DisposeAsync().AsTask(),
			tiff.DisposeAsync().AsTask(),
			png.DisposeAsync().AsTask()
		};
		
		await Task.WhenAll(tasks);
	}

	// ===================================================================================
	// METADATA SIZE ESTIMATION BENCHMARKS
	// ===================================================================================

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("MetadataEstimation")]
	public long EstimateMetadataSize_WebP_Large()
	{
		return _webpLarge.EstimatedMetadataSize;
	}

	[Benchmark]
	[BenchmarkCategory("MetadataEstimation")]
	public bool HasLargeMetadata_WebP_Large()
	{
		return _webpLarge.HasLargeMetadata;
	}

	[Benchmark]
	[BenchmarkCategory("MetadataEstimation")]
	public long EstimateMetadataSize_TIFF_Large()
	{
		return _tiffLarge.EstimatedMetadataSize;
	}

	[Benchmark]
	[BenchmarkCategory("MetadataEstimation")]
	public long EstimateMetadataSize_PNG_Large()
	{
		return _pngLarge.EstimatedMetadataSize;
	}

	// ===================================================================================
	// REPEATED DISPOSAL BENCHMARKS - Test multiple disposal calls
	// ===================================================================================

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("RepeatedDisposal")]
	public async Task RepeatedAsyncDisposal_Safe()
	{
		var webp = new WebPRaster(800, 600);
		webp.Metadata.IccProfile = new byte[100];
		
		// Multiple calls should be safe
		await webp.DisposeAsync();
		await webp.DisposeAsync();
		await webp.DisposeAsync();
	}

	[Benchmark]
	[BenchmarkCategory("RepeatedDisposal")]
	public void RepeatedSyncDisposal_Safe()
	{
		var webp = new WebPRaster(800, 600);
		webp.Metadata.IccProfile = new byte[100];
		
		// Multiple calls should be safe
		webp.Dispose();
		webp.Dispose();
		webp.Dispose();
	}
}