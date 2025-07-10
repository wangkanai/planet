// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics;

/// <summary>
/// Base interface for all metadata implementations in the Graphics library.
/// Provides a common contract for resource cleanup and size estimation.
/// </summary>
public interface IMetadata : IDisposable, IAsyncDisposable
{
	/// <summary>Gets or sets the image width in pixels.</summary>
	int Width { get; set; }

	/// <summary>Gets or sets the image height in pixels.</summary>
	int Height { get; set; }

	/// <summary>Gets or sets the image/document title.</summary>
	string? Title { get; set; }

	/// <summary>Gets or sets the orientation of the content.</summary>
	int? Orientation { get; set; }

	/// <summary>Gets a value indicating whether the metadata is large and benefits from async disposal.</summary>
	bool HasLargeMetadata { get; }

	/// <summary>Gets the estimated size of metadata in bytes.</summary>
	long EstimatedMetadataSize { get; }

	/// <summary>Validates the metadata for compliance with format specifications.</summary>
	bool ValidateMetadata();

	/// <summary>Clears all metadata values to their defaults.</summary>
	void Clear();

	/// <summary>Creates a deep copy of the metadata.</summary>
	IMetadata Clone();
}
