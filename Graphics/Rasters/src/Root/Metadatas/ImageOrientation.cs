// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

/// <summary>
/// Defines image orientation values based on EXIF orientation tag.
/// </summary>
public enum ImageOrientation
{
	/// <summary>Normal orientation (0° rotation).</summary>
	Normal = 1,

	/// <summary>Flipped horizontally.</summary>
	FlipHorizontal = 2,

	/// <summary>Rotated 180°.</summary>
	Rotate180 = 3,

	/// <summary>Flipped vertically.</summary>
	FlipVertical = 4,

	/// <summary>Rotated 90° counter-clockwise and flipped horizontally.</summary>
	Transpose = 5,

	/// <summary>Rotated 90° clockwise.</summary>
	Rotate90Clockwise = 6,

	/// <summary>Rotated 90° clockwise and flipped horizontally.</summary>
	Transverse = 7,

	/// <summary>Rotated 90° counter-clockwise.</summary>
	Rotate90CounterClockwise = 8
}