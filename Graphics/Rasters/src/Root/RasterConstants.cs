// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters;

/// <summary>
/// Defines common constants used across raster image formats.
/// </summary>
public static class RasterConstants
{
	/// <summary>
	/// Minimum quality value for lossy compression.
	/// </summary>
	public const int MinQuality = 0;

	/// <summary>
	/// Maximum quality value (lossless).
	/// </summary>
	public const int MaxQuality = 100;

	/// <summary>
	/// Minimum speed value (slowest, best quality).
	/// </summary>
	public const int MinSpeed = 0;

	/// <summary>
	/// Maximum speed value (fastest, lower quality).
	/// </summary>
	public const int MaxSpeed = 10;

	/// <summary>
	/// Minimum dimension for raster images.
	/// </summary>
	public const int MinDimension = 1;

	/// <summary>
	/// Common maximum dimension for raster images.
	/// </summary>
	public const int MaxDimension = 65536;

	/// <summary>
	/// Quality presets for common use cases.
	/// </summary>
	public static class QualityPresets
	{
		/// <summary>Lossless compression (100).</summary>
		public const int Lossless = 100;

		/// <summary>Near-lossless compression (95).</summary>
		public const int NearLossless = 95;

		/// <summary>Professional quality (90).</summary>
		public const int Professional = 90;

		/// <summary>High quality (88).</summary>
		public const int High = 88;

		/// <summary>Standard quality (85).</summary>
		public const int Standard = 85;

		/// <summary>Good quality (80).</summary>
		public const int Good = 80;

		/// <summary>Web quality (75).</summary>
		public const int Web = 75;

		/// <summary>Email quality (70).</summary>
		public const int Email = 70;

		/// <summary>Thumbnail quality (60).</summary>
		public const int Thumbnail = 60;

		/// <summary>Preview quality (40).</summary>
		public const int Preview = 40;
	}

	/// <summary>
	/// Speed presets for encoding performance.
	/// </summary>
	public static class SpeedPresets
	{
		/// <summary>Slowest speed, highest quality (0).</summary>
		public const int Slowest = 0;

		/// <summary>Very slow speed (1).</summary>
		public const int VerySlow = 1;

		/// <summary>Slow speed (2).</summary>
		public const int Slow = 2;

		/// <summary>Below default speed (4).</summary>
		public const int BelowDefault = 4;

		/// <summary>Default balanced speed (5).</summary>
		public const int Default = 5;

		/// <summary>Above default speed (6).</summary>
		public const int AboveDefault = 6;

		/// <summary>Fast speed (8).</summary>
		public const int Fast = 8;

		/// <summary>Very fast speed (9).</summary>
		public const int VeryFast = 9;

		/// <summary>Fastest speed, lower quality (10).</summary>
		public const int Fastest = 10;
	}

	/// <summary>
	/// Memory usage constants.
	/// </summary>
	public static class Memory
	{
		/// <summary>Minimum pixel buffer size in MB.</summary>
		public const int MinPixelBufferSizeMB = 16;

		/// <summary>Default pixel buffer size in MB.</summary>
		public const int DefaultPixelBufferSizeMB = 256;

		/// <summary>Maximum pixel buffer size in MB.</summary>
		public const int MaxPixelBufferSizeMB = 2048;

		/// <summary>Minimum metadata buffer size in MB.</summary>
		public const int MinMetadataBufferSizeMB = 1;

		/// <summary>Default metadata buffer size in MB.</summary>
		public const int DefaultMetadataBufferSizeMB = 16;

		/// <summary>Maximum metadata buffer size in MB.</summary>
		public const int MaxMetadataBufferSizeMB = 256;

		/// <summary>Maximum concurrent encoding threads.</summary>
		public const int MaxThreads = 64;

		/// <summary>Default tile size for large image processing.</summary>
		public const int DefaultTileSize = 512;

		/// <summary>Maximum tile size for large image processing.</summary>
		public const int MaxTileSize = 4096;
	}

	/// <summary>
	/// Common dimension limits.
	/// </summary>
	public static class Dimensions
	{
		/// <summary>HD resolution width (1920x1080).</summary>
		public const int HdWidth = 1920;

		/// <summary>HD resolution height (1920x1080).</summary>
		public const int HdHeight = 1080;

		/// <summary>4K resolution width (3840x2160).</summary>
		public const int FourKWidth = 3840;

		/// <summary>4K resolution height (3840x2160).</summary>
		public const int FourKHeight = 2160;

		/// <summary>8K resolution width (7680x4320).</summary>
		public const int EightKWidth = 7680;

		/// <summary>8K resolution height (7680x4320).</summary>
		public const int EightKHeight = 4320;

		/// <summary>Maximum thumbnail dimension.</summary>
		public const int MaxThumbnailDimension = 256;

		/// <summary>Maximum preview dimension.</summary>
		public const int MaxPreviewDimension = 1024;
	}

	/// <summary>
	/// File size thresholds.
	/// </summary>
	public static class FileSizes
	{
		/// <summary>1 MB in bytes.</summary>
		public const long OneMegabyte = 1024L * 1024L;

		/// <summary>10 MB in bytes.</summary>
		public const long TenMegabytes = 10L * OneMegabyte;

		/// <summary>100 MB in bytes.</summary>
		public const long HundredMegabytes = 100L * OneMegabyte;

		/// <summary>1 GB in bytes.</summary>
		public const long OneGigabyte = 1024L * OneMegabyte;

		/// <summary>Maximum file size for web upload (typically 10MB).</summary>
		public const long MaxWebUploadSize = TenMegabytes;

		/// <summary>Maximum file size for email attachment (typically 25MB).</summary>
		public const long MaxEmailAttachmentSize = 25L * OneMegabyte;
	}

	/// <summary>
	/// Common bit depths.
	/// </summary>
	public static class BitDepths
	{
		/// <summary>1-bit monochrome.</summary>
		public const int Monochrome = 1;

		/// <summary>8-bit standard.</summary>
		public const int Standard = 8;

		/// <summary>10-bit high color.</summary>
		public const int HighColor = 10;

		/// <summary>12-bit professional.</summary>
		public const int Professional = 12;

		/// <summary>16-bit full.</summary>
		public const int Full = 16;
	}

	/// <summary>
	/// Resolution constants.
	/// </summary>
	public static class Resolutions
	{
		/// <summary>Standard screen resolution (72 DPI).</summary>
		public const double Screen = 72.0;

		/// <summary>Web resolution (96 DPI).</summary>
		public const double Web = 96.0;

		/// <summary>Print resolution (300 DPI).</summary>
		public const double Print = 300.0;

		/// <summary>High quality print resolution (600 DPI).</summary>
		public const double HighQualityPrint = 600.0;

		/// <summary>Professional print resolution (1200 DPI).</summary>
		public const double ProfessionalPrint = 1200.0;

		/// <summary>Minimum valid resolution.</summary>
		public const double MinResolution = 1.0;

		/// <summary>Maximum reasonable resolution.</summary>
		public const double MaxResolution = 10000.0;
	}
}
