// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Tiffs;

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

	/// <summary>Gets or sets the strip offsets (locations of data strips in the file).</summary>
	public int[]? StripOffsets { get; set; }

	/// <summary>Gets or sets the strip byte counts (sizes of data strips).</summary>
	public int[]? StripByteCounts { get; set; }

	/// <summary>Gets or sets the tile offsets for tiled images.</summary>
	public int[]? TileOffsets { get; set; }

	/// <summary>Gets or sets the tile byte counts for tiled images.</summary>
	public int[]? TileByteCounts { get; set; }

	/// <summary>Gets or sets the color map for palette-indexed images.</summary>
	public ushort[]? ColorMap { get; set; }

	/// <summary>Gets or sets the transfer function for color correction.</summary>
	public ushort[]? TransferFunction { get; set; }

	/// <summary>Gets or sets the white point chromaticity coordinates.</summary>
	public double[]? WhitePoint { get; set; }

	/// <summary>Gets or sets the primary chromaticity coordinates.</summary>
	public double[]? PrimaryChromaticities { get; set; }

	/// <summary>Gets or sets the YCbCr coefficients for color space conversion.</summary>
	public double[]? YCbCrCoefficients { get; set; }

	/// <summary>Gets or sets the reference black and white values.</summary>
	public double[]? ReferenceBlackWhite { get; set; }

	/// <summary>Gets or sets EXIF metadata as a byte array.</summary>
	public byte[]? ExifIfd { get; set; }

	/// <summary>Gets or sets GPS metadata as a byte array.</summary>
	public byte[]? GpsIfd { get; set; }

	/// <summary>Gets or sets ICC color profile data.</summary>
	public byte[]? IccProfile { get; set; }

	/// <summary>Gets or sets XMP metadata as a byte array.</summary>
	public byte[]? XmpData { get; set; }

	/// <summary>Gets or sets IPTC metadata as a byte array.</summary>
	public byte[]? IptcData { get; set; }
}
