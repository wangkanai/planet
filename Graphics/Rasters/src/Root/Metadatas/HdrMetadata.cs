// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Metadatas;

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