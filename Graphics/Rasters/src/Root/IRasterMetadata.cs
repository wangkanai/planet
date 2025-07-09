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

	/// <summary>Gets or sets the horizontal resolution.</summary>
	double? XResolution { get; set; }

	/// <summary>Gets or sets the vertical resolution.</summary>
	double? YResolution { get; set; }

	/// <summary>Gets or sets the resolution unit.</summary>
	int? ResolutionUnit { get; set; }

	/// <summary>Gets or sets the color space identifier.</summary>
	int? ColorSpace { get; set; }

	/// <summary>Gets or sets the GPS latitude.</summary>
	double? GpsLatitude { get; set; }

	/// <summary>Gets or sets the GPS longitude.</summary>
	double? GpsLongitude { get; set; }

	/// <summary>Creates a deep copy of the raster metadata.</summary>
	new IRasterMetadata Clone();
}
