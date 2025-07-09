// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters;

/// <summary>Defines the contract for raster image metadata across all image formats.</summary>
public interface IRasterMetadata : IMetadata
{

	/// <summary>Gets or sets the bit depth per channel.</summary>
	int BitDepth { get; set; }

	/// <summary>Gets or sets the EXIF metadata as a byte array.</summary>
	byte[]? ExifData { get; set; }

	/// <summary>Gets or sets the XMP metadata.</summary>
	string? XmpData { get; set; }

	/// <summary>Gets or sets the ICC color profile data.</summary>
	byte[]? IccProfile { get; set; }

	/// <summary>Clears all metadata values to their defaults.</summary>
	void Clear();
}
