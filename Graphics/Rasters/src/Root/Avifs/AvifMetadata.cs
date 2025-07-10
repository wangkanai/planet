// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Metadatas;

namespace Wangkanai.Graphics.Rasters.Avifs;

/// <summary>
/// Represents comprehensive metadata for an AVIF image including color, HDR, and auxiliary information.
/// </summary>
public class AvifMetadata : RasterMetadata
{

	// Note: Width, Height, and BitDepth are inherited from base class

	/// <summary>Gets or sets the color space.</summary>
	public AvifColorSpace ColorSpace { get; set; } = AvifColorSpace.Srgb;

	/// <summary>Gets or sets the chroma subsampling mode.</summary>
	public AvifChromaSubsampling ChromaSubsampling { get; set; } = AvifChromaSubsampling.Yuv420;

	/// <summary>Gets or sets whether the image has an alpha channel.</summary>
	public bool HasAlpha { get; set; }

	/// <summary>Gets or sets whether the alpha channel is premultiplied.</summary>
	public bool AlphaPremultiplied { get; set; }

	/// <summary>Gets or sets the quality level used for encoding (0-100).</summary>
	public int Quality { get; set; } = AvifConstants.DefaultQuality;

	/// <summary>Gets or sets the speed setting used for encoding (0-10).</summary>
	public int Speed { get; set; } = AvifConstants.DefaultSpeed;

	/// <summary>Gets or sets whether lossless compression was used.</summary>
	public bool IsLossless { get; set; }

	/// <summary>Gets or sets whether film grain synthesis is enabled.</summary>
	public bool UsesFilmGrain { get; set; }

	// Note: ExifData, XmpData, and IccProfile are inherited from base class

	/// <summary>Gets or sets HDR metadata if present.</summary>
	public HdrMetadata? HdrInfo { get; set; }

	/// <summary>Gets or sets color volume metadata.</summary>
	public ColorVolumeMetadata? ColorVolume { get; set; }

	/// <summary>Gets or sets the AV1 codec configuration record.</summary>
	public byte[]? CodecConfigurationRecord { get; set; }

	// Note: CreationTime and ModificationTime are inherited from base class

	/// <summary>Gets or sets the encoder name/version.</summary>
	public string? EncoderInfo { get; set; }

	/// <summary>Gets or sets clean aperture information.</summary>
	public CleanAperture? CleanAperture { get; set; }

	/// <summary>Gets or sets image rotation in degrees (0, 90, 180, 270).</summary>
	public int Rotation { get; set; }

	/// <summary>Gets or sets whether the image is mirrored horizontally.</summary>
	public bool IsMirrored { get; set; }

	/// <summary>Gets or sets the pixel aspect ratio.</summary>
	public double PixelAspectRatio { get; set; } = 1.0;

	/// <summary>Gets or sets additional metadata properties.</summary>
	public Dictionary<string, object> ExtendedProperties { get; set; } = new();

	/// <inheritdoc />
	public override long EstimatedMetadataSize
	{
		get
		{
			long size = base.EstimatedMetadataSize;

			if (CodecConfigurationRecord != null)
				size += CodecConfigurationRecord.Length;

			size += EstimateDictionaryObjectSize(ExtendedProperties);

			return size;
		}
	}


	/// <inheritdoc />
	public override IMetadata Clone() => CloneRaster();
	
	/// <inheritdoc />
	public override IRasterMetadata CloneRaster()
	{
		var clone = new AvifMetadata();
		CopyRasterTo(clone);
		
		// Copy AVIF-specific properties
		clone.ColorSpace = ColorSpace;
		clone.ChromaSubsampling = ChromaSubsampling;
		clone.HasAlpha = HasAlpha;
		clone.AlphaPremultiplied = AlphaPremultiplied;
		clone.Quality = Quality;
		clone.Speed = Speed;
		clone.IsLossless = IsLossless;
		clone.UsesFilmGrain = UsesFilmGrain;
		clone.HdrInfo = HdrInfo?.Clone();
		clone.ColorVolume = ColorVolume?.Clone();
		clone.CodecConfigurationRecord = CodecConfigurationRecord?.ToArray();
		clone.EncoderInfo = EncoderInfo;
		clone.CleanAperture = CleanAperture?.Clone();
		clone.Rotation = Rotation;
		clone.IsMirrored = IsMirrored;
		clone.PixelAspectRatio = PixelAspectRatio;
		clone.ExtendedProperties = new Dictionary<string, object>(ExtendedProperties);
		
		return clone;
	}
	
	/// <inheritdoc />
	public override void Clear()
	{
		base.Clear();
		
		// Reset AVIF-specific properties to defaults
		ColorSpace = AvifColorSpace.Srgb;
		ChromaSubsampling = AvifChromaSubsampling.Yuv420;
		HasAlpha = false;
		AlphaPremultiplied = false;
		Quality = AvifConstants.DefaultQuality;
		Speed = AvifConstants.DefaultSpeed;
		IsLossless = false;
		UsesFilmGrain = false;
		HdrInfo = null;
		ColorVolume = null;
		CodecConfigurationRecord = null;
		EncoderInfo = null;
		CleanAperture = null;
		Rotation = 0;
		IsMirrored = false;
		PixelAspectRatio = 1.0;
		ExtendedProperties.Clear();
	}

	/// <inheritdoc />
	protected override void DisposeManagedResources()
	{
		base.DisposeManagedResources();
		
		// Clear AVIF-specific resources
		CodecConfigurationRecord = null;
		ExtendedProperties.Clear();
	}
}

/// <summary>
/// Represents HDR metadata for high dynamic range images.
/// </summary>
public class HdrMetadata
{
	/// <summary>Gets or sets the maximum luminance in nits.</summary>
	public double MaxLuminance { get; set; } = AvifConstants.Hdr.Hdr10PeakBrightness;

	/// <summary>Gets or sets the minimum luminance in nits.</summary>
	public double MinLuminance { get; set; } = 0.005;

	/// <summary>Gets or sets the maximum content light level in nits.</summary>
	public double MaxContentLightLevel { get; set; }

	/// <summary>Gets or sets the maximum frame-average light level in nits.</summary>
	public double MaxFrameAverageLightLevel { get; set; }

	/// <summary>Gets or sets the HDR format type.</summary>
	public HdrFormat Format { get; set; } = HdrFormat.Hdr10;

	/// <summary>Gets or sets the color primaries.</summary>
	public ColorPrimaries? Primaries { get; set; }

	/// <summary>Gets or sets the transfer characteristics.</summary>
	public TransferCharacteristics? Transfer { get; set; }

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
			Primaries = Primaries?.Clone(),
			Transfer = Transfer
		};
	}
}

/// <summary>
/// Represents color volume metadata.
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
/// Represents clean aperture (crop) information.
/// </summary>
public class CleanAperture
{
	/// <summary>Gets or sets the clean aperture width.</summary>
	public int Width { get; set; }

	/// <summary>Gets or sets the clean aperture height.</summary>
	public int Height { get; set; }

	/// <summary>Gets or sets the horizontal offset.</summary>
	public int HorizontalOffset { get; set; }

	/// <summary>Gets or sets the vertical offset.</summary>
	public int VerticalOffset { get; set; }

	/// <summary>Creates a copy of the clean aperture.</summary>
	public CleanAperture Clone()
	{
		return new CleanAperture
		{
			Width = Width,
			Height = Height,
			HorizontalOffset = HorizontalOffset,
			VerticalOffset = VerticalOffset
		};
	}
}

/// <summary>
/// Represents color primaries information.
/// </summary>
public class ColorPrimaries
{
	/// <summary>Gets or sets the color primaries type.</summary>
	public int Type { get; set; }

	/// <summary>Gets or sets custom primary values if applicable.</summary>
	public double[]? CustomValues { get; set; }

	/// <summary>Creates a copy of the color primaries.</summary>
	public ColorPrimaries Clone()
	{
		return new ColorPrimaries
		{
			Type = Type,
			CustomValues = CustomValues?.ToArray()
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
/// Defines transfer characteristics for HDR.
/// </summary>
public enum TransferCharacteristics
{
	/// <summary>BT.709 transfer.</summary>
	Bt709 = 1,

	/// <summary>Unspecified.</summary>
	Unspecified = 2,

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

	/// <summary>Logarithmic 100:1.</summary>
	Log100 = 9,

	/// <summary>Logarithmic 316:1.</summary>
	Log316 = 10,

	/// <summary>IEC 61966-2-4.</summary>
	Iec61966_2_4 = 11,

	/// <summary>BT.1361.</summary>
	Bt1361 = 12,

	/// <summary>sRGB.</summary>
	Srgb = 13,

	/// <summary>BT.2020 10-bit.</summary>
	Bt2020_10bit = 14,

	/// <summary>BT.2020 12-bit.</summary>
	Bt2020_12bit = 15,

	/// <summary>SMPTE 2084 (PQ).</summary>
	Smpte2084 = 16,

	/// <summary>SMPTE 428.</summary>
	Smpte428 = 17,

	/// <summary>ARIB STD-B67 (HLG).</summary>
	AribStdB67 = 18
}