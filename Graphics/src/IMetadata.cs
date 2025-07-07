// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics;

/// <summary>
/// Base interface for all metadata implementations in the Graphics library.
/// Provides a common contract for resource cleanup and size estimation.
/// </summary>
public interface IMetadata : IDisposable, IAsyncDisposable 
{
	/// <summary>Gets a value indicating whether the metadata is large and benefits from async disposal.</summary>
	bool HasLargeMetadata { get; }

	/// <summary>Gets the estimated size of metadata in bytes.</summary>
	long EstimatedMetadataSize { get; }
}
