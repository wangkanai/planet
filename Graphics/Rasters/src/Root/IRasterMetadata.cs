// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters;

/// <summary>Defines the contract for raster image metadata across all image formats.</summary>
public interface IRasterMetadata : IMetadata
{
	/// <summary>Gets or sets the image width in pixels.</summary>
	int Width { get; set; }

	/// <summary>Gets or sets the image height in pixels.</summary>
	int Height { get; set; }

	/// <summary>Gets or sets the bit depth per channel.</summary>
	int BitDepth { get; set; }

	/// <summary>Gets or sets the EXIF metadata as a byte array.</summary>
	byte[]? ExifData { get; set; }

	/// <summary>Gets or sets the XMP metadata.</summary>
	string? XmpData { get; set; }

	/// <summary>Gets or sets the ICC color profile data.</summary>
	byte[]? IccProfile { get; set; }

	/// <summary>Gets or sets the creation date and time.</summary>
	DateTime? CreationTime { get; set; }

	/// <summary>Gets or sets the modification date and time.</summary>
	DateTime? ModificationTime { get; set; }

	/// <summary>Gets or sets the software used to create or modify the image.</summary>
	string? Software { get; set; }

	/// <summary>Gets or sets the image description.</summary>
	string? Description { get; set; }

	/// <summary>Gets or sets the copyright information.</summary>
	string? Copyright { get; set; }

	/// <summary>Gets or sets the author or artist name.</summary>
	string? Author { get; set; }

	/// <summary>Creates a deep copy of the metadata.</summary>
	/// <returns>A new instance with the same values.</returns>
	IRasterMetadata Clone();

	/// <summary>Clears all metadata values to their defaults.</summary>
	void Clear();
}
