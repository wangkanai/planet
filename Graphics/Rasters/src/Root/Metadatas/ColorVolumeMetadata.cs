// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

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
		=> new()
		   {
			   RedPrimaryX   = RedPrimaryX,
			   RedPrimaryY   = RedPrimaryY,
			   GreenPrimaryX = GreenPrimaryX,
			   GreenPrimaryY = GreenPrimaryY,
			   BluePrimaryX  = BluePrimaryX,
			   BluePrimaryY  = BluePrimaryY,
			   WhitePointX   = WhitePointX,
			   WhitePointY   = WhitePointY,
			   LuminanceMin  = LuminanceMin,
			   LuminanceMax  = LuminanceMax
		   };
}
