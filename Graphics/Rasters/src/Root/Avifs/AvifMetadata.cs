// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Avifs;

/// <summary>
/// Represents comprehensive metadata for an AVIF image including color, HDR, and auxiliary information.
/// </summary>
public class AvifMetadata : IAsyncDisposable, IDisposable
{
	private bool _disposed;

	/// <summary>Gets or sets the image width in pixels.</summary>
	public int Width { get; set; }

	/// <summary>Gets or sets the image height in pixels.</summary>
	public int Height { get; set; }

	/// <summary>Gets or sets the bit depth per channel (8, 10, or 12).</summary>
	public int BitDepth { get; set; } = 8;

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

	/// <summary>Gets or sets the EXIF metadata.</summary>
	public byte[]? ExifData { get; set; }

	/// <summary>Gets or sets the XMP metadata.</summary>
	public string? XmpData { get; set; }

	/// <summary>Gets or sets the ICC color profile.</summary>
	public byte[]? IccProfile { get; set; }

	/// <summary>Gets or sets HDR metadata if present.</summary>
	public HdrMetadata? HdrInfo { get; set; }

	/// <summary>Gets or sets color volume metadata.</summary>
	public ColorVolumeMetadata? ColorVolume { get; set; }

	/// <summary>Gets or sets the AV1 codec configuration record.</summary>
	public byte[]? CodecConfigurationRecord { get; set; }

	/// <summary>Gets or sets the creation timestamp.</summary>
	public DateTime CreationTime { get; set; } = DateTime.UtcNow;

	/// <summary>Gets or sets the modification timestamp.</summary>
	public DateTime ModificationTime { get; set; } = DateTime.UtcNow;

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

	/// <summary>Gets the estimated metadata size in bytes.</summary>
	public long EstimatedMetadataSize
	{
		get
		{
			long size = 256; // Base object size

			if (ExifData != null)
				size += ExifData.Length;

			if (XmpData != null)
				size += XmpData.Length * 2; // Unicode string

			if (IccProfile != null)
				size += IccProfile.Length;

			if (CodecConfigurationRecord != null)
				size += CodecConfigurationRecord.Length;

			size += ExtendedProperties.Count * 64; // Estimate per property

			return size;
		}
	}

	/// <summary>Gets whether this metadata contains large data that benefits from async disposal.</summary>
	public bool HasLargeMetadata => EstimatedMetadataSize > ImageConstants.LargeMetadataThreshold;

	/// <summary>Creates a deep copy of the metadata.</summary>
	public AvifMetadata Clone()
	{
		return new AvifMetadata
		{
			Width = Width,
			Height = Height,
			BitDepth = BitDepth,
			ColorSpace = ColorSpace,
			ChromaSubsampling = ChromaSubsampling,
			HasAlpha = HasAlpha,
			AlphaPremultiplied = AlphaPremultiplied,
			Quality = Quality,
			Speed = Speed,
			IsLossless = IsLossless,
			UsesFilmGrain = UsesFilmGrain,
			ExifData = ExifData?.ToArray(),
			XmpData = XmpData,
			IccProfile = IccProfile?.ToArray(),
			HdrInfo = HdrInfo?.Clone(),
			ColorVolume = ColorVolume?.Clone(),
			CodecConfigurationRecord = CodecConfigurationRecord?.ToArray(),
			CreationTime = CreationTime,
			ModificationTime = ModificationTime,
			EncoderInfo = EncoderInfo,
			CleanAperture = CleanAperture?.Clone(),
			Rotation = Rotation,
			IsMirrored = IsMirrored,
			PixelAspectRatio = PixelAspectRatio,
			ExtendedProperties = new Dictionary<string, object>(ExtendedProperties)
		};
	}

	/// <summary>Disposes of the metadata resources.</summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Asynchronously disposes of the metadata resources.</summary>
	public async ValueTask DisposeAsync()
	{
		if (HasLargeMetadata)
		{
			await Task.Run(() => Dispose(true)).ConfigureAwait(false);
		}
		else
		{
			Dispose(true);
		}
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases resources.</summary>
	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			// Clear large arrays
			ExifData = null;
			IccProfile = null;
			CodecConfigurationRecord = null;
			ExtendedProperties.Clear();
		}

		_disposed = true;
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