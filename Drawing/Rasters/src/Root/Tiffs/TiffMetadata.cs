// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Drawing.Rasters.Tiffs;

/// <summary>Represents metadata information for TIFF images.</summary>
public class TiffMetadata
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
	public DateTime? DateTime { get; set; }
	
	/// <summary>Gets or sets the horizontal resolution in pixels per inch.</summary>
	public double? XResolution { get; set; }
	
	/// <summary>Gets or sets the vertical resolution in pixels per inch.</summary>
	public double? YResolution { get; set; }
	
	/// <summary>Gets or sets the resolution unit (1 = no unit, 2 = inches, 3 = centimeters).</summary>
	public int? ResolutionUnit { get; set; }
	
	/// <summary>Gets or sets additional custom metadata tags.</summary>
	public Dictionary<int, object> CustomTags { get; set; } = new();
}