// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpegs;

/// <summary>Represents metadata information for JPEG images.</summary>
public class JpegMetadata
{
	/// <summary>Gets or sets the image description.</summary>
	public string? ImageDescription { get; set; }

	/// <summary>Gets or sets the camera make.</summary>
	public string? Make { get; set; }

	/// <summary>Gets or sets the camera model.</summary>
	public string? Model { get; set; }

	/// <summary>Gets or sets the software used to create the image.</summary>
	public string? Software { get; set; }

	/// <summary>Gets or sets the copyright information.</summary>
	public string? Copyright { get; set; }

	/// <summary>Gets or sets the artist/photographer.</summary>
	public string? Artist { get; set; }

	/// <summary>Gets or sets the creation date and time.</summary>
	public DateTime? CaptureDateTime { get; set; }

	/// <summary>Gets or sets the horizontal resolution in pixels per inch.</summary>
	public double? XResolution { get; set; }

	/// <summary>Gets or sets the vertical resolution in pixels per inch.</summary>
	public double? YResolution { get; set; }

	/// <summary>Gets or sets the resolution unit (1 = no unit, 2 = inches, 3 = centimeters).</summary>
	public int? ResolutionUnit { get; set; }

	/// <summary>Gets or sets the orientation of the image.</summary>
	public int? Orientation { get; set; }

	/// <summary>Gets or sets the exposure time in seconds.</summary>
	public double? ExposureTime { get; set; }

	/// <summary>Gets or sets the F-number (aperture).</summary>
	public double? FNumber { get; set; }

	/// <summary>Gets or sets the ISO speed rating.</summary>
	public int? IsoSpeedRating { get; set; }

	/// <summary>Gets or sets the focal length in millimeters.</summary>
	public double? FocalLength { get; set; }

	/// <summary>Gets or sets the GPS latitude.</summary>
	public double? GpsLatitude { get; set; }

	/// <summary>Gets or sets the GPS longitude.</summary>
	public double? GpsLongitude { get; set; }

	/// <summary>Gets or sets the color space information.</summary>
	public int? ColorSpace { get; set; }

	/// <summary>Gets or sets the white balance setting.</summary>
	public int? WhiteBalance { get; set; }

	/// <summary>Gets or sets additional custom EXIF tags.</summary>
	public Dictionary<int, object> CustomExifTags { get; set; } = new();

	/// <summary>Gets or sets IPTC metadata for image descriptions and keywords.</summary>
	public Dictionary<string, string> IptcTags { get; set; } = new();

	/// <summary>Gets or sets XMP metadata for additional information.</summary>
	public Dictionary<string, string> XmpTags { get; set; } = new();

	/// <summary>Gets or sets ICC color profile information.</summary>
	public byte[]? IccProfile { get; set; }
}
