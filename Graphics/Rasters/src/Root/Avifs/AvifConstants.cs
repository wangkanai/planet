// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Collections.Immutable;

namespace Wangkanai.Graphics.Rasters.Avifs;

/// <summary>
/// Defines constants for AVIF format specifications and container structure.
/// </summary>
public static class AvifConstants
{
	/// <summary>AVIF file type brand for still images.</summary>
	public static readonly ImmutableArray<byte> AvifBrand = "avif"u8.ToImmutableArray();

	/// <summary>AVIF file type brand for image sequences.</summary>
	public static readonly ImmutableArray<byte> AvisBrand = "avis"u8.ToImmutableArray();

	/// <summary>MIF1 brand for MIAF compatibility.</summary>
	public static readonly ImmutableArray<byte> Mif1Brand = "mif1"u8.ToImmutableArray();

	/// <summary>File type box type ("ftyp").</summary>
	public static readonly ImmutableArray<byte> FileTypeBoxType = "ftyp"u8.ToImmutableArray();

	/// <summary>Meta box type ("meta").</summary>
	public static readonly ImmutableArray<byte> MetaBoxType = "meta"u8.ToImmutableArray();

	/// <summary>Handler box type ("hdlr").</summary>
	public static readonly ImmutableArray<byte> HandlerBoxType = "hdlr"u8.ToImmutableArray();

	/// <summary>Primary item box type ("pitm").</summary>
	public static readonly ImmutableArray<byte> PrimaryItemBoxType = "pitm"u8.ToImmutableArray();

	/// <summary>Item location box type ("iloc").</summary>
	public static readonly ImmutableArray<byte> ItemLocationBoxType = "iloc"u8.ToImmutableArray();

	/// <summary>Item information box type ("iinf").</summary>
	public static readonly ImmutableArray<byte> ItemInfoBoxType = "iinf"u8.ToImmutableArray();

	/// <summary>Item properties box type ("iprp").</summary>
	public static readonly ImmutableArray<byte> ItemPropertiesBoxType = "iprp"u8.ToImmutableArray();

	/// <summary>Item reference box type ("iref").</summary>
	public static readonly ImmutableArray<byte> ItemReferenceBoxType = "iref"u8.ToImmutableArray();

	/// <summary>Media data box type ("mdat").</summary>
	public static readonly ImmutableArray<byte> MediaDataBoxType = "mdat"u8.ToImmutableArray();

	/// <summary>AV1 codec configuration box type ("av1C").</summary>
	public static readonly ImmutableArray<byte> Av1ConfigBoxType = "av1C"u8.ToImmutableArray();

	/// <summary>Color information box type ("colr").</summary>
	public static readonly ImmutableArray<byte> ColorInfoBoxType = "colr"u8.ToImmutableArray();

	/// <summary>Image spatial extents property type ("ispe").</summary>
	public static readonly ImmutableArray<byte> ImageSpatialExtentsType = "ispe"u8.ToImmutableArray();

	/// <summary>Pixel information property type ("pixi").</summary>
	public static readonly ImmutableArray<byte> PixelInfoType = "pixi"u8.ToImmutableArray();

	/// <summary>Alpha channel property type ("auxC").</summary>
	public static readonly ImmutableArray<byte> AlphaChannelType = "auxC"u8.ToImmutableArray();

	/// <summary>Clean aperture box type ("clap").</summary>
	public static readonly ImmutableArray<byte> CleanApertureType = "clap"u8.ToImmutableArray();

	/// <summary>Image rotation box type ("irot").</summary>
	public static readonly ImmutableArray<byte> ImageRotationType = "irot"u8.ToImmutableArray();

	/// <summary>Image mirror box type ("imir").</summary>
	public static readonly ImmutableArray<byte> ImageMirrorType = "imir"u8.ToImmutableArray();

	/// <summary>Minimum supported bit depth.</summary>
	public const int MinBitDepth = 8;

	/// <summary>Maximum supported bit depth.</summary>
	public const int MaxBitDepth = 12;

	/// <summary>Minimum quality value.</summary>
	public const int MinQuality = 0;

	/// <summary>Maximum quality value.</summary>
	public const int MaxQuality = 100;

	/// <summary>Default quality value.</summary>
	public const int DefaultQuality = 85;

	/// <summary>Minimum speed value (slowest, best quality).</summary>
	public const int MinSpeed = 0;

	/// <summary>Maximum speed value (fastest, lower quality).</summary>
	public const int MaxSpeed = 10;

	/// <summary>Default speed value.</summary>
	public const int DefaultSpeed = 6;

	/// <summary>Default thread count (0 = auto-detect).</summary>
	public const int DefaultThreadCount = 0;

	/// <summary>Maximum dimension for AVIF images.</summary>
	public const int MaxDimension = 65536;

	/// <summary>Box header size in bytes.</summary>
	public const int BoxHeaderSize = 8;

	/// <summary>Extended box size field length.</summary>
	public const int ExtendedBoxSizeLength = 8;

	/// <summary>
	/// Quality settings for common use cases.
	/// </summary>
	public static class QualityPresets
	{
		/// <summary>Lossless compression.</summary>
		public const int Lossless = 100;

		/// <summary>Near-lossless compression.</summary>
		public const int NearLossless = 95;

		/// <summary>High quality for professional use.</summary>
		public const int Professional = 90;

		/// <summary>Standard quality for general use.</summary>
		public const int Standard = 85;

		/// <summary>Good quality for web use.</summary>
		public const int Web = 75;

		/// <summary>Acceptable quality for thumbnails.</summary>
		public const int Thumbnail = 60;

		/// <summary>Low quality for previews.</summary>
		public const int Preview = 40;
	}

	/// <summary>
	/// Speed presets for encoding.
	/// </summary>
	public static class SpeedPresets
	{
		/// <summary>Slowest speed, highest quality.</summary>
		public const int Slowest = 0;

		/// <summary>Very slow speed, very high quality.</summary>
		public const int VerySlow = 2;

		/// <summary>Slow speed, high quality.</summary>
		public const int Slow = 4;

		/// <summary>Default balanced speed and quality.</summary>
		public const int Default = 6;

		/// <summary>Fast speed, good quality.</summary>
		public const int Fast = 8;

		/// <summary>Fastest speed, acceptable quality.</summary>
		public const int Fastest = 10;
	}

	/// <summary>
	/// Memory usage constants.
	/// </summary>
	public static class Memory
	{
		/// <summary>Default pixel buffer size in MB.</summary>
		public const int DefaultPixelBufferSizeMB = 256;

		/// <summary>Maximum pixel buffer size in MB.</summary>
		public const int MaxPixelBufferSizeMB = 1024;

		/// <summary>Tile size for encoding large images.</summary>
		public const int DefaultTileSize = 512;

		/// <summary>Maximum concurrent encoding threads.</summary>
		public const int MaxThreads = 64;
	}

	/// <summary>
	/// HDR constants.
	/// </summary>
	public static class Hdr
	{
		/// <summary>Standard dynamic range peak brightness (nits).</summary>
		public const double SdrPeakBrightness = 100.0;

		/// <summary>HDR10 peak brightness (nits).</summary>
		public const double Hdr10PeakBrightness = 1000.0;

		/// <summary>HDR10+ peak brightness (nits).</summary>
		public const double Hdr10PlusPeakBrightness = 4000.0;

		/// <summary>Dolby Vision peak brightness (nits).</summary>
		public const double DolbyVisionPeakBrightness = 10000.0;
	}
}