// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Extensions;

/// <summary>
/// Constants for metadata processing and validation.
/// </summary>
public static class MetadataConstants
{
	/// <summary>Quality assessment thresholds for pixel counts.</summary>
	public static class QualityThresholds
	{
		/// <summary>Pixel count threshold for professional quality (20+ megapixels).</summary>
		public const long ProfessionalMegapixels = 20_000_000;
		
		/// <summary>Pixel count threshold for high quality (10+ megapixels).</summary>
		public const long HighQualityMegapixels = 10_000_000;
		
		/// <summary>Pixel count threshold for standard quality (5+ megapixels).</summary>
		public const long StandardQualityMegapixels = 5_000_000;
		
		/// <summary>Quality score threshold for professional classification.</summary>
		public const int ProfessionalQualityScore = 6;
		
		/// <summary>Quality score threshold for high classification.</summary>
		public const int HighQualityScore = 4;
		
		/// <summary>Quality score threshold for standard classification.</summary>
		public const int StandardQualityScore = 2;
	}

	/// <summary>Validation ranges for camera settings.</summary>
	public static class CameraValidation
	{
		/// <summary>Minimum valid exposure time in seconds.</summary>
		public const double MinExposureTime = 0.000001;
		
		/// <summary>Maximum valid exposure time in seconds.</summary>
		public const double MaxExposureTime = 30.0;
		
		/// <summary>Minimum valid F-number (aperture).</summary>
		public const double MinFNumber = 0.5;
		
		/// <summary>Maximum valid F-number (aperture).</summary>
		public const double MaxFNumber = 64.0;
		
		/// <summary>Minimum valid ISO speed rating.</summary>
		public const int MinIsoSpeed = 6;
		
		/// <summary>Maximum valid ISO speed rating.</summary>
		public const int MaxIsoSpeed = 6_400_000;
		
		/// <summary>Minimum valid focal length in millimeters.</summary>
		public const double MinFocalLength = 1.0;
		
		/// <summary>Maximum valid focal length in millimeters.</summary>
		public const double MaxFocalLength = 2000.0;
	}

	/// <summary>Professional photography detection criteria.</summary>
	public static class ProfessionalDetection
	{
		/// <summary>Manual white balance value indicating professional setting.</summary>
		public const int ManualWhiteBalance = 1;
		
		/// <summary>sRGB color space identifier.</summary>
		public const int SrgbColorSpace = 1;
		
		/// <summary>Standard print resolution in DPI.</summary>
		public const int StandardPrintDpi = 300;
		
		/// <summary>Standard resolution unit (inches).</summary>
		public const int InchesResolutionUnit = 2;
	}

	/// <summary>PNG-specific validation limits.</summary>
	public static class PngLimits
	{
		/// <summary>Maximum keyword length for PNG text chunks.</summary>
		public const int MaxKeywordLength = 79;
		
		/// <summary>Warning threshold for total text size in bytes.</summary>
		public const int LargeTextSizeThreshold = 1_000_000; // 1MB
		
		/// <summary>Warning threshold for number of custom chunks.</summary>
		public const int ManyCustomChunksThreshold = 50;
		
		/// <summary>Web optimization resolution range minimum DPI.</summary>
		public const int WebOptimizedMinDpi = 72;
		
		/// <summary>Web optimization resolution range maximum DPI.</summary>
		public const int WebOptimizedMaxDpi = 300;
	}

	/// <summary>Performance and similarity calculation parameters.</summary>
	public static class Performance
	{
		/// <summary>Default resolution similarity tolerance (5%).</summary>
		public const double DefaultResolutionTolerance = 0.05;
		
		/// <summary>Print-quality resolution similarity tolerance (1%).</summary>
		public const double PrintResolutionTolerance = 0.01;
		
		/// <summary>Weight for dimension similarity in overall similarity calculation.</summary>
		public const double DimensionSimilarityWeight = 0.3;
		
		/// <summary>Weight for metadata presence similarity in overall calculation.</summary>
		public const double MetadataSimilarityWeight = 0.2;
		
		/// <summary>Pre-allocated StringBuilder capacity for exposure summaries.</summary>
		public const int ExposureSummaryCapacity = 64;
		
		/// <summary>Minimum exposure time for long exposure detection (1 second).</summary>
		public const double LongExposureThreshold = 1.0;
		
		/// <summary>Standard professional shutter speed range minimum (1/1000s).</summary>
		public const double ProfessionalShutterSpeedMin = 1.0 / 1000.0;
		
		/// <summary>Standard professional shutter speed range maximum (1/60s).</summary>
		public const double ProfessionalShutterSpeedMax = 1.0 / 60.0;
		
		/// <summary>Professional ISO speed threshold for clean images.</summary>
		public const int CleanIsoThreshold = 800;
		
		/// <summary>Acceptable professional ISO speed threshold.</summary>
		public const int AcceptableIsoThreshold = 3200;
	}
}