// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics;

/// <summary>
/// Metadata summary structure for quick analysis.
/// </summary>
public record MetadataSummary
{
	public (int width, int height) Dimensions { get; init; }

	public string Type           { get; init; } = string.Empty;
	public long   PixelCount     { get; init; }
	public float  AspectRatio    { get; init; }
	public long   MetadataSize   { get; init; }
	public bool   IsLarge        { get; init; }
	public bool   HasTitle       { get; init; }
	public bool   HasOrientation { get; init; }

	// Raster-specific properties
	public int? BitDepth        { get; init; }
	public bool HasResolution   { get; init; }
	public bool HasGpsData      { get; init; }
	public bool HasColorProfile { get; init; }

	// Vector-specific properties
	public int?    ElementCount        { get; init; }
	public bool    HasCoordinateSystem { get; init; }
	public string? ComplexityLevel     { get; init; }
}
