// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Wangkanai.Graphics.Rasters.WebPs;

namespace Wangkanai.Graphics.Rasters.Benchmark;

/// <summary>Benchmarks comparing byte[] vs ImmutableArray&lt;byte&gt; performance in WebP constants.</summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class WebPConstantsBenchmark
{
	private static readonly byte[] _testData = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x10, 0x00, 0x57, 0x45, 0x42, 0x50, 0x56, 0x50, 0x38, 0x20 };

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("SequenceEqual")]
	public bool SequenceEqual_ByteArray_Signature()
	{
		return _testData.AsSpan()[..4].SequenceEqual(WebPConstantsBaseline.Signature);
	}

	[Benchmark]
	[BenchmarkCategory("SequenceEqual")]
	public bool SequenceEqual_ImmutableArray_Signature()
	{
		return _testData.AsSpan()[..4].SequenceEqual(WebPConstants.Signature.AsSpan());
	}

	[Benchmark]
	[BenchmarkCategory("SequenceEqual")]
	public bool SequenceEqual_ByteArray_FormatId()
	{
		return _testData.AsSpan().Slice(8, 4).SequenceEqual(WebPConstantsBaseline.FormatId);
	}

	[Benchmark]
	[BenchmarkCategory("SequenceEqual")]
	public bool SequenceEqual_ImmutableArray_FormatId()
	{
		return _testData.AsSpan().Slice(8, 4).SequenceEqual(WebPConstants.FormatId.AsSpan());
	}

	[Benchmark]
	[BenchmarkCategory("SequenceEqual")]
	public bool SequenceEqual_ByteArray_VP8ChunkId()
	{
		return _testData.AsSpan().Slice(12, 4).SequenceEqual(WebPConstantsBaseline.VP8ChunkId);
	}

	[Benchmark]
	[BenchmarkCategory("SequenceEqual")]
	public bool SequenceEqual_ImmutableArray_VP8ChunkId()
	{
		return _testData.AsSpan().Slice(12, 4).SequenceEqual(WebPConstants.VP8ChunkId.AsSpan());
	}

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("Length")]
	public int Length_ByteArray()
	{
		return WebPConstantsBaseline.Signature.Length + 
		       WebPConstantsBaseline.FormatId.Length + 
		       WebPConstantsBaseline.VP8ChunkId.Length;
	}

	[Benchmark]
	[BenchmarkCategory("Length")]
	public int Length_ImmutableArray()
	{
		return WebPConstants.Signature.Length + 
		       WebPConstants.FormatId.Length + 
		       WebPConstants.VP8ChunkId.Length;
	}

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("Access")]
	public byte Access_ByteArray()
	{
		return WebPConstantsBaseline.Signature[0];
	}

	[Benchmark]
	[BenchmarkCategory("Access")]
	public byte Access_ImmutableArray()
	{
		return WebPConstants.Signature[0];
	}

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("AsSpan")]
	public ReadOnlySpan<byte> AsSpan_ByteArray()
	{
		return WebPConstantsBaseline.Signature.AsSpan();
	}

	[Benchmark]
	[BenchmarkCategory("AsSpan")]
	public ReadOnlySpan<byte> AsSpan_ImmutableArray()
	{
		return WebPConstants.Signature.AsSpan();
	}

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("Memory")]
	public long MemoryFootprint_ByteArray()
	{
		// Simulate memory footprint calculation
		long totalSize = 0;
		totalSize += WebPConstantsBaseline.Signature.Length;
		totalSize += WebPConstantsBaseline.FormatId.Length;
		totalSize += WebPConstantsBaseline.VP8ChunkId.Length;
		totalSize += WebPConstantsBaseline.VP8LChunkId.Length;
		totalSize += WebPConstantsBaseline.VP8XChunkId.Length;
		return totalSize;
	}

	[Benchmark]
	[BenchmarkCategory("Memory")]
	public long MemoryFootprint_ImmutableArray()
	{
		// Simulate memory footprint calculation
		long totalSize = 0;
		totalSize += WebPConstants.Signature.Length;
		totalSize += WebPConstants.FormatId.Length;
		totalSize += WebPConstants.VP8ChunkId.Length;
		totalSize += WebPConstants.VP8LChunkId.Length;
		totalSize += WebPConstants.VP8XChunkId.Length;
		return totalSize;
	}

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("Validation")]
	public bool ValidateWebPSignature_ByteArray()
	{
		var data = _testData.AsSpan();
		
		// Check RIFF header
		if (!data[..4].SequenceEqual(WebPConstantsBaseline.Signature))
			return false;

		// Check WEBP format identifier
		return data.Slice(8, 4).SequenceEqual(WebPConstantsBaseline.FormatId);
	}

	[Benchmark]
	[BenchmarkCategory("Validation")]
	public bool ValidateWebPSignature_ImmutableArray()
	{
		var data = _testData.AsSpan();
		
		// Check RIFF header
		if (!data[..4].SequenceEqual(WebPConstants.Signature.AsSpan()))
			return false;

		// Check WEBP format identifier
		return data.Slice(8, 4).SequenceEqual(WebPConstants.FormatId.AsSpan());
	}

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("FormatDetection")]
	public int DetectFormat_ByteArray()
	{
		var data = _testData.AsSpan();
		var chunkId = data.Slice(12, 4);

		if (chunkId.SequenceEqual(WebPConstantsBaseline.VP8ChunkId))
			return 1; // Simple
		if (chunkId.SequenceEqual(WebPConstantsBaseline.VP8LChunkId))
			return 2; // Lossless
		if (chunkId.SequenceEqual(WebPConstantsBaseline.VP8XChunkId))
			return 3; // Extended

		return 0; // Unknown
	}

	[Benchmark]
	[BenchmarkCategory("FormatDetection")]
	public int DetectFormat_ImmutableArray()
	{
		var data = _testData.AsSpan();
		var chunkId = data.Slice(12, 4);

		if (chunkId.SequenceEqual(WebPConstants.VP8ChunkId.AsSpan()))
			return 1; // Simple
		if (chunkId.SequenceEqual(WebPConstants.VP8LChunkId.AsSpan()))
			return 2; // Lossless
		if (chunkId.SequenceEqual(WebPConstants.VP8XChunkId.AsSpan()))
			return 3; // Extended

		return 0; // Unknown
	}
}