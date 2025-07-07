// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters;

/// <summary>
/// Represents GPS coordinates for geotagged images.
/// </summary>
public class GpsCoordinates
{
	/// <summary>Gets or sets the latitude in decimal degrees.</summary>
	public double Latitude { get; set; }

	/// <summary>Gets or sets the longitude in decimal degrees.</summary>
	public double Longitude { get; set; }

	/// <summary>Gets or sets the altitude in meters.</summary>
	public double? Altitude { get; set; }

	/// <summary>Gets or sets the GPS timestamp.</summary>
	public DateTimeOffset? Timestamp { get; set; }

	/// <summary>Creates a copy of the GPS coordinates.</summary>
	public GpsCoordinates Clone()
	{
		return new GpsCoordinates
		{
			Latitude = Latitude,
			Longitude = Longitude,
			Altitude = Altitude,
			Timestamp = Timestamp
		};
	}
}

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

/// <summary>
/// Enhanced HDR metadata with additional properties for both AVIF and HEIF formats.
/// </summary>
public class HdrMetadata
{
	/// <summary>Gets or sets the maximum luminance in nits.</summary>
	public double MaxLuminance { get; set; } = 1000.0;

	/// <summary>Gets or sets the minimum luminance in nits.</summary>
	public double MinLuminance { get; set; } = 0.005;

	/// <summary>Gets or sets the maximum content light level in nits.</summary>
	public double MaxContentLightLevel { get; set; }

	/// <summary>Gets or sets the maximum frame-average light level in nits.</summary>
	public double MaxFrameAverageLightLevel { get; set; }

	/// <summary>Gets or sets the HDR format type.</summary>
	public HdrFormat Format { get; set; } = HdrFormat.Hdr10;

	/// <summary>Gets or sets the color primaries.</summary>
	public HdrColorPrimaries ColorPrimaries { get; set; } = HdrColorPrimaries.Bt2020;

	/// <summary>Gets or sets the transfer characteristics.</summary>
	public HdrTransferCharacteristics TransferCharacteristics { get; set; } = HdrTransferCharacteristics.Pq;

	/// <summary>Gets or sets the matrix coefficients.</summary>
	public HdrMatrixCoefficients MatrixCoefficients { get; set; } = HdrMatrixCoefficients.Bt2020Ncl;

	/// <summary>Gets or sets custom color volume metadata.</summary>
	public ColorVolumeMetadata? ColorVolume { get; set; }

	/// <summary>Creates a copy of the HDR metadata.</summary>
	public HdrMetadata Clone()
	{
		return new HdrMetadata
		{
			MaxLuminance = MaxLuminance,
			MinLuminance = MinLuminance,
			MaxContentLightLevel = MaxContentLightLevel,
			MaxFrameAverageLightLevel = MaxFrameAverageLightLevel,
			Format = Format,
			ColorPrimaries = ColorPrimaries,
			TransferCharacteristics = TransferCharacteristics,
			MatrixCoefficients = MatrixCoefficients,
			ColorVolume = ColorVolume?.Clone()
		};
	}
}

/// <summary>
/// Represents color volume metadata for HDR.
/// </summary>
public class ColorVolumeMetadata
{
	/// <summary>Gets or sets the red primary X coordinate.</summary>
	public double RedPrimaryX { get; set; }

	/// <summary>Gets or sets the red primary Y coordinate.</summary>
	public double RedPrimaryY { get; set; }

	/// <summary>Gets or sets the green primary X coordinate.</summary>
	public double GreenPrimaryX { get; set; }

	/// <summary>Gets or sets the green primary Y coordinate.</summary>
	public double GreenPrimaryY { get; set; }

	/// <summary>Gets or sets the blue primary X coordinate.</summary>
	public double BluePrimaryX { get; set; }

	/// <summary>Gets or sets the blue primary Y coordinate.</summary>
	public double BluePrimaryY { get; set; }

	/// <summary>Gets or sets the white point X coordinate.</summary>
	public double WhitePointX { get; set; }

	/// <summary>Gets or sets the white point Y coordinate.</summary>
	public double WhitePointY { get; set; }

	/// <summary>Gets or sets the luminance range minimum.</summary>
	public double LuminanceMin { get; set; }

	/// <summary>Gets or sets the luminance range maximum.</summary>
	public double LuminanceMax { get; set; }

	/// <summary>Creates a copy of the color volume metadata.</summary>
	public ColorVolumeMetadata Clone()
	{
		return new ColorVolumeMetadata
		{
			RedPrimaryX = RedPrimaryX,
			RedPrimaryY = RedPrimaryY,
			GreenPrimaryX = GreenPrimaryX,
			GreenPrimaryY = GreenPrimaryY,
			BluePrimaryX = BluePrimaryX,
			BluePrimaryY = BluePrimaryY,
			WhitePointX = WhitePointX,
			WhitePointY = WhitePointY,
			LuminanceMin = LuminanceMin,
			LuminanceMax = LuminanceMax
		};
	}
}

/// <summary>
/// Defines HDR format types.
/// </summary>
public enum HdrFormat
{
	/// <summary>Standard dynamic range.</summary>
	Sdr,

	/// <summary>HDR10 format.</summary>
	Hdr10,

	/// <summary>HDR10+ format.</summary>
	Hdr10Plus,

	/// <summary>Dolby Vision format.</summary>
	DolbyVision,

	/// <summary>Hybrid Log-Gamma format.</summary>
	Hlg
}

/// <summary>
/// Defines HDR color primaries.
/// </summary>
public enum HdrColorPrimaries
{
	/// <summary>BT.709 primaries.</summary>
	Bt709 = 1,

	/// <summary>BT.470M primaries.</summary>
	Bt470M = 4,

	/// <summary>BT.470BG primaries.</summary>
	Bt470Bg = 5,

	/// <summary>BT.601/SMPTE 170M primaries.</summary>
	Bt601 = 6,

	/// <summary>SMPTE 240M primaries.</summary>
	Smpte240M = 7,

	/// <summary>Generic film primaries.</summary>
	GenericFilm = 8,

	/// <summary>BT.2020 primaries.</summary>
	Bt2020 = 9,

	/// <summary>XYZ primaries.</summary>
	Xyz = 10,

	/// <summary>DCI-P3 primaries.</summary>
	DciP3 = 11,

	/// <summary>Display P3 primaries.</summary>
	DisplayP3 = 12
}

/// <summary>
/// Defines HDR transfer characteristics.
/// </summary>
public enum HdrTransferCharacteristics
{
	/// <summary>BT.709 transfer.</summary>
	Bt709 = 1,

	/// <summary>Gamma 2.2.</summary>
	Gamma22 = 4,

	/// <summary>Gamma 2.8.</summary>
	Gamma28 = 5,

	/// <summary>BT.601.</summary>
	Bt601 = 6,

	/// <summary>SMPTE 240M.</summary>
	Smpte240M = 7,

	/// <summary>Linear.</summary>
	Linear = 8,

	/// <summary>sRGB.</summary>
	Srgb = 13,

	/// <summary>BT.2020 10-bit.</summary>
	Bt2020_10bit = 14,

	/// <summary>BT.2020 12-bit.</summary>
	Bt2020_12bit = 15,

	/// <summary>SMPTE 2084 (PQ).</summary>
	Pq = 16,

	/// <summary>SMPTE 428.</summary>
	Smpte428 = 17,

	/// <summary>ARIB STD-B67 (HLG).</summary>
	Hlg = 18
}

/// <summary>
/// Defines HDR matrix coefficients.
/// </summary>
public enum HdrMatrixCoefficients
{
	/// <summary>RGB or GBR (identity).</summary>
	Identity = 0,

	/// <summary>BT.709.</summary>
	Bt709 = 1,

	/// <summary>BT.470M.</summary>
	Bt470M = 4,

	/// <summary>BT.470BG.</summary>
	Bt470Bg = 5,

	/// <summary>BT.601.</summary>
	Bt601 = 6,

	/// <summary>SMPTE 240M.</summary>
	Smpte240M = 7,

	/// <summary>YCoCg.</summary>
	YCoCg = 8,

	/// <summary>BT.2020 non-constant luminance.</summary>
	Bt2020Ncl = 9,

	/// <summary>BT.2020 constant luminance.</summary>
	Bt2020Cl = 10,

	/// <summary>SMPTE 2085.</summary>
	Smpte2085 = 11,

	/// <summary>Chromaticity-derived non-constant luminance.</summary>
	ChromaDerivedNcl = 12,

	/// <summary>Chromaticity-derived constant luminance.</summary>
	ChromaDerivedCl = 13,

	/// <summary>ICtCp.</summary>
	ICtCp = 14
}