// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics;

/// <summary>Constants used throughout the Graphics component library.</summary>
public static class ImageConstants
{
	/// <summary>The suggested batch size for processing large collections of metadata items during async disposal.</summary>
	/// <remarks>Used to determine when to yield control during large metadata cleanup operations.</remarks>
	public const int DisposalBatchSize = 100;

	/// <summary>The threshold in bytes for determining if metadata is considered "large" and benefits from async disposal.</summary>
	/// <remarks>Images with metadata larger than this threshold will use batched async disposal to avoid blocking operations. Set to 1MB based on performance testing across various image formats. </remarks>
	public const long LargeMetadataThreshold = 1_000_000; // 1MB

	/// <summary>The threshold in bytes for very large metadata that may benefit from explicit garbage collection.</summary>
	/// <remarks>Used primarily in TIFF and other metadata-heavy formats to determine when to suggest the GC collection. Set to 10MB based on memory pressure testing.</remarks>
	public const long VeryLargeMetadataThreshold = 10_000_000; // 10MB
}
