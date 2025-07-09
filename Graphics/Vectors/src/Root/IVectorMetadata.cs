// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors;

/// <summary>
/// Defines the contract for vector graphics metadata.
/// This interface serves as a marker for vector-specific metadata implementations.
/// </summary>
/// <remarks>
/// Unlike raster images which have fixed pixel dimensions, vector graphics
/// are resolution-independent and may have different metadata requirements
/// such as viewbox dimensions, path counts, or coordinate systems.
/// </remarks>
public interface IVectorMetadata : IMetadata
{
	/// <summary>Gets or sets the coordinate reference system.</summary>
	string? CoordinateReferenceSystem { get; set; }

	/// <summary>Gets or sets the color space for vector graphics.</summary>
	string? ColorSpace { get; set; }

	/// <summary>Gets or sets the element count for performance optimization.</summary>
	int ElementCount { get; set; }

	/// <summary>Creates a deep copy of the vector metadata.</summary>
	new IVectorMetadata Clone();
}
