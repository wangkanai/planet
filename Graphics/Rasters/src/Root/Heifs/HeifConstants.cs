// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Collections.Immutable;

namespace Wangkanai.Graphics.Rasters.Heifs;

/// <summary>
/// Defines constants for HEIF format specifications and container structure.
/// </summary>
public static class HeifConstants
{
	/// <summary>HEIF file type brand for still images.</summary>
	public static readonly ImmutableArray<byte> HeifBrand = "heic"u8.ToImmutableArray();

	/// <summary>HEIF file type brand for image sequences.</summary>
	public static readonly ImmutableArray<byte> HeifSequenceBrand = "heis"u8.ToImmutableArray();

	/// <summary>HEIF file type brand for image collections.</summary>
	public static readonly ImmutableArray<byte> HeifCollectionBrand = "hevc"u8.ToImmutableArray();

	/// <summary>AVIF alternative brand for HEIF containers.</summary>
	public static readonly ImmutableArray<byte> AvifBrand = "avif"u8.ToImmutableArray();

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

	/// <summary>HEVC codec configuration box type ("hvcC").</summary>
	public static readonly ImmutableArray<byte> HevcConfigBoxType = "hvcC"u8.ToImmutableArray();

	/// <summary>AVC codec configuration box type ("avcC").</summary>
	public static readonly ImmutableArray<byte> AvcConfigBoxType = "avcC"u8.ToImmutableArray();

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
	public const int MaxBitDepth = 16;

	/// <summary>Minimum quality value.</summary>
	public const int MinQuality = 0;

	/// <summary>Maximum quality value.</summary>
	public const int MaxQuality = 100;

	/// <summary>Default quality value.</summary>
	public const int DefaultQuality = 80;

	/// <summary>Minimum speed value (slowest, best quality).</summary>
	public const int MinSpeed = 0;

	/// <summary>Maximum speed value (fastest, lower quality).</summary>
	public const int MaxSpeed = 9;

	/// <summary>Default speed value.</summary>
	public const int DefaultSpeed = 5;

	/// <summary>Default thread count (0 = auto-detect).</summary>
	public const int DefaultThreadCount = 0;

	/// <summary>Maximum dimension for HEIF images.</summary>
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

		/// <summary>Professional quality for photography.</summary>
		public const int Professional = 90;

		/// <summary>High quality for archival purposes.</summary>
		public const int High = 85;

		/// <summary>Standard quality for general use.</summary>
		public const int Standard = 80;

		/// <summary>Good quality for web use.</summary>
		public const int Web = 70;

		/// <summary>Acceptable quality for thumbnails.</summary>
		public const int Thumbnail = 60;

		/// <summary>Low quality for previews.</summary>
		public const int Preview = 40;

		/// <summary>Mobile-optimized quality.</summary>
		public const int Mobile = 65;
	}

	/// <summary>
	/// Speed presets for encoding.
	/// </summary>
	public static class SpeedPresets
	{
		/// <summary>Slowest speed, highest quality.</summary>
		public const int Slowest = 0;

		/// <summary>Very slow speed, very high quality.</summary>
		public const int VerySlow = 1;

		/// <summary>Slow speed, high quality.</summary>
		public const int Slow = 2;

		/// <summary>Medium slow speed, good quality.</summary>
		public const int MediumSlow = 3;

		/// <summary>Medium speed, balanced quality.</summary>
		public const int Medium = 4;

		/// <summary>Default balanced speed and quality.</summary>
		public const int Default = 5;

		/// <summary>Medium fast speed, good quality.</summary>
		public const int MediumFast = 6;

		/// <summary>Fast speed, acceptable quality.</summary>
		public const int Fast = 7;

		/// <summary>Very fast speed, lower quality.</summary>
		public const int VeryFast = 8;

		/// <summary>Fastest speed, minimal quality.</summary>
		public const int Fastest = 9;
	}

	/// <summary>
	/// Memory usage constants.
	/// </summary>
	public static class Memory
	{
		/// <summary>Default pixel buffer size in MB.</summary>
		public const int DefaultPixelBufferSizeMB = 512;

		/// <summary>Maximum pixel buffer size in MB.</summary>
		public const int MaxPixelBufferSizeMB = 2048;

		/// <summary>Default metadata buffer size in MB.</summary>
		public const int DefaultMetadataBufferSizeMB = 64;

		/// <summary>Maximum metadata buffer size in MB.</summary>
		public const int MaxMetadataBufferSizeMB = 256;

		/// <summary>Tile size for encoding large images.</summary>
		public const int DefaultTileSize = 1024;

		/// <summary>Maximum concurrent encoding threads.</summary>
		public const int MaxThreads = 128;

		/// <summary>Large image threshold in pixels.</summary>
		public const long LargeImageThreshold = 100_000_000; // 100 megapixels
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

		/// <summary>HLG peak brightness (nits).</summary>
		public const double HlgPeakBrightness = 1000.0;
	}

	/// <summary>
	/// Codec-specific constants.
	/// </summary>
	public static class Codecs
	{
		/// <summary>HEVC profile constants.</summary>
		public static class Hevc
		{
			/// <summary>Main profile for 8-bit content.</summary>
			public const int MainProfile = 1;

			/// <summary>Main 10 profile for 10-bit content.</summary>
			public const int Main10Profile = 2;

			/// <summary>Main Still Picture profile.</summary>
			public const int MainStillPictureProfile = 3;

			/// <summary>Default level for HD content.</summary>
			public const int DefaultLevel = 120; // Level 4.0

			/// <summary>High level for 4K content.</summary>
			public const int HighLevel = 150; // Level 5.0
		}

		/// <summary>AVC profile constants.</summary>
		public static class Avc
		{
			/// <summary>Baseline profile.</summary>
			public const int BaselineProfile = 66;

			/// <summary>Main profile.</summary>
			public const int MainProfile = 77;

			/// <summary>High profile.</summary>
			public const int HighProfile = 100;

			/// <summary>Default level for HD content.</summary>
			public const int DefaultLevel = 40; // Level 4.0
		}
	}

	/// <summary>
	/// Container format constants.
	/// </summary>
	public static class Container
	{
		/// <summary>Minimum box size.</summary>
		public const int MinBoxSize = 8;

		/// <summary>Maximum standard box size.</summary>
		public const uint MaxStandardBoxSize = 0xFFFFFFFF;

		/// <summary>Extended box size indicator.</summary>
		public const uint ExtendedBoxSizeIndicator = 1;

		/// <summary>UUID box type indicator.</summary>
		public static readonly ImmutableArray<byte> UuidBoxType = "uuid"u8.ToImmutableArray();

		/// <summary>Free space box type.</summary>
		public static readonly ImmutableArray<byte> FreeBoxType = "free"u8.ToImmutableArray();

		/// <summary>Skip box type.</summary>
		public static readonly ImmutableArray<byte> SkipBoxType = "skip"u8.ToImmutableArray();
	}
}
