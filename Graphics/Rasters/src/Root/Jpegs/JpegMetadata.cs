// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.Jpegs;

/// <summary>Represents metadata information for JPEG images.</summary>
public class JpegMetadata : RasterMetadata
{
	/// <summary>Gets or sets the image description.</summary>
	/// <remarks>Maps to the Description property from base class for backward compatibility.</remarks>
	public string? ImageDescription
	{
		get => Description;
		set => Description = value;
	}

	/// <summary>Gets or sets the camera make.</summary>
	public string? Make { get; set; }

	/// <summary>Gets or sets the camera model.</summary>
	public string? Model { get; set; }

	/// <summary>Gets or sets the artist/photographer.</summary>
	/// <remarks>Maps to the Author property from base class for backward compatibility.</remarks>
	public string? Artist
	{
		get => Author;
		set => Author = value;
	}

	/// <summary>Gets or sets the JPEG-specific capture date and time.</summary>
	public DateTime? CaptureDateTime
	{
		get => CreationTime;
		set => CreationTime = value;
	}

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

	// Note: IccProfile is inherited from base class

	/// <inheritdoc />
	public override long EstimatedMetadataSize
	{
		get
		{
			var size = base.EstimatedMetadataSize;

			// Add custom EXIF tags size
			size += EstimateDictionaryObjectSize(CustomExifTags);

			// Add IPTC tags size
			size += EstimateDictionarySize(IptcTags);

			// Add XMP tags size
			size += EstimateDictionarySize(XmpTags);

			// Add JPEG-specific string properties
			size += EstimateStringSize(Make);
			size += EstimateStringSize(Model);

			return size;
		}
	}

	/// <inheritdoc />
	protected override void DisposeManagedResources()
	{
		base.DisposeManagedResources();
		
		// Clear JPEG-specific collections
		CustomExifTags.Clear();
		IptcTags.Clear();
		XmpTags.Clear();
	}

	/// <inheritdoc />
	public override IMetadata Clone() => CloneRaster();
	
	/// <inheritdoc />
	public override IRasterMetadata CloneRaster()
	{
		var clone = new JpegMetadata();
		CopyRasterTo(clone);
		
		// Copy JPEG-specific properties
		clone.Make = Make;
		clone.Model = Model;
		clone.XResolution = XResolution;
		clone.YResolution = YResolution;
		clone.ResolutionUnit = ResolutionUnit;
		clone.Orientation = Orientation;
		clone.ExposureTime = ExposureTime;
		clone.FNumber = FNumber;
		clone.IsoSpeedRating = IsoSpeedRating;
		clone.FocalLength = FocalLength;
		clone.GpsLatitude = GpsLatitude;
		clone.GpsLongitude = GpsLongitude;
		clone.ColorSpace = ColorSpace;
		clone.WhiteBalance = WhiteBalance;
		clone.CustomExifTags = new Dictionary<int, object>(CustomExifTags);
		clone.IptcTags = new Dictionary<string, string>(IptcTags);
		clone.XmpTags = new Dictionary<string, string>(XmpTags);
		
		return clone;
	}
	
	/// <inheritdoc />
	public override void Clear()
	{
		base.Clear();
		
		// Clear JPEG-specific properties
		Make = null;
		Model = null;
		XResolution = null;
		YResolution = null;
		ResolutionUnit = null;
		Orientation = null;
		ExposureTime = null;
		FNumber = null;
		IsoSpeedRating = null;
		FocalLength = null;
		GpsLatitude = null;
		GpsLongitude = null;
		ColorSpace = null;
		WhiteBalance = null;
		CustomExifTags.Clear();
		IptcTags.Clear();
		XmpTags.Clear();
	}
}
