// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Jpegs;

namespace Wangkanai.Graphics.Rasters.Extensions;

/// <summary>
/// Extension methods for JpegMetadata providing JPEG-specific utility functions.
/// </summary>
public static class JpegMetadataExtensions
{
	/// <summary>
	/// Determines if the JPEG has camera metadata.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to check.</param>
	/// <returns>True if camera make and/or model are present.</returns>
	public static bool HasCameraMetadata(this JpegMetadata metadata)
	{
		return !string.IsNullOrWhiteSpace(metadata.Make) || !string.IsNullOrWhiteSpace(metadata.Model);
	}

	/// <summary>
	/// Determines if the JPEG has exposure data.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to check.</param>
	/// <returns>True if exposure settings are present.</returns>
	public static bool HasExposureData(this JpegMetadata metadata)
	{
		return metadata.ExposureTime.HasValue || 
		       metadata.FNumber.HasValue || 
		       metadata.IsoSpeedRating.HasValue || 
		       metadata.FocalLength.HasValue;
	}

	/// <summary>
	/// Determines if the JPEG has IPTC data.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to check.</param>
	/// <returns>True if IPTC tags are present.</returns>
	public static bool HasIptcData(this JpegMetadata metadata)
	{
		return metadata.IptcTags.Count > 0;
	}

	/// <summary>
	/// Determines if the JPEG has XMP tags.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to check.</param>
	/// <returns>True if XMP tags are present.</returns>
	public static bool HasXmpTags(this JpegMetadata metadata)
	{
		return metadata.XmpTags.Count > 0;
	}

	/// <summary>
	/// Gets camera metadata as a structured object.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to extract from.</param>
	/// <returns>Camera metadata information.</returns>
	public static CameraMetadata GetCameraMetadata(this JpegMetadata metadata)
	{
		return new CameraMetadata
		{
			Make = metadata.Make,
			Model = metadata.Model,
			ExposureTime = metadata.ExposureTime,
			FNumber = metadata.FNumber,
			IsoSpeedRating = metadata.IsoSpeedRating,
			FocalLength = metadata.FocalLength,
			WhiteBalance = metadata.WhiteBalance,
			CaptureDateTime = metadata.CaptureDateTime
		};
	}

	/// <summary>
	/// Gets the camera identifier string.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to read from.</param>
	/// <returns>Camera identifier in "Make Model" format.</returns>
	public static string GetCameraIdentifier(this JpegMetadata metadata)
	{
		var parts = new List<string>();
		
		if (!string.IsNullOrWhiteSpace(metadata.Make))
			parts.Add(metadata.Make.Trim());
		
		if (!string.IsNullOrWhiteSpace(metadata.Model))
			parts.Add(metadata.Model.Trim());

		return parts.Count > 0 ? string.Join(" ", parts) : "Unknown Camera";
	}

	/// <summary>
	/// Gets the exposure summary string.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to summarize.</param>
	/// <returns>Exposure summary in standard format.</returns>
	public static string GetExposureSummary(this JpegMetadata metadata)
	{
		var parts = new List<string>();

		if (metadata.ExposureTime.HasValue)
		{
			var exposure = metadata.ExposureTime.Value;
			if (exposure >= 1)
				parts.Add($"{exposure:F1}s");
			else
				parts.Add($"1/{Math.Round(1 / exposure)}s");
		}

		if (metadata.FNumber.HasValue)
			parts.Add($"f/{metadata.FNumber:F1}");

		if (metadata.IsoSpeedRating.HasValue)
			parts.Add($"ISO {metadata.IsoSpeedRating}");

		if (metadata.FocalLength.HasValue)
			parts.Add($"{metadata.FocalLength:F0}mm");

		return parts.Count > 0 ? string.Join(", ", parts) : "No exposure data";
	}

	/// <summary>
	/// Determines if the exposure settings are valid for photography.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to validate.</param>
	/// <returns>True if exposure settings are within reasonable ranges.</returns>
	public static bool HasValidExposureSettings(this JpegMetadata metadata)
	{
		// Validate exposure time (0.000001s to 30s)
		if (metadata.ExposureTime.HasValue && 
		    (metadata.ExposureTime <= 0 || metadata.ExposureTime > 30))
			return false;

		// Validate F-number (f/0.5 to f/64)
		if (metadata.FNumber.HasValue && 
		    (metadata.FNumber < 0.5 || metadata.FNumber > 64))
			return false;

		// Validate ISO (6 to 6400000)
		if (metadata.IsoSpeedRating.HasValue && 
		    (metadata.IsoSpeedRating < 6 || metadata.IsoSpeedRating > 6400000))
			return false;

		// Validate focal length (1mm to 2000mm)
		if (metadata.FocalLength.HasValue && 
		    (metadata.FocalLength < 1 || metadata.FocalLength > 2000))
			return false;

		return true;
	}

	/// <summary>
	/// Gets the white balance description.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to describe.</param>
	/// <returns>White balance description.</returns>
	public static string GetWhiteBalanceDescription(this JpegMetadata metadata)
	{
		if (!metadata.WhiteBalance.HasValue)
			return "Auto";

		return metadata.WhiteBalance.Value switch
		{
			0 => "Auto",
			1 => "Manual",
			2 => "Daylight",
			3 => "Cloudy",
			4 => "Tungsten",
			5 => "Fluorescent",
			6 => "Flash",
			_ => $"Custom ({metadata.WhiteBalance.Value})"
		};
	}

	/// <summary>
	/// Determines if the JPEG is a professional photograph based on metadata.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to analyze.</param>
	/// <returns>True if the image appears to be professionally captured.</returns>
	public static bool IsProfessionalPhoto(this JpegMetadata metadata)
	{
		// Check for professional camera indicators
		var hasProfessionalCamera = metadata.HasCameraMetadata() && 
		                           (metadata.Make?.Contains("Canon", StringComparison.OrdinalIgnoreCase) == true ||
		                            metadata.Make?.Contains("Nikon", StringComparison.OrdinalIgnoreCase) == true ||
		                            metadata.Make?.Contains("Sony", StringComparison.OrdinalIgnoreCase) == true) &&
		                           metadata.Model?.Contains("EOS", StringComparison.OrdinalIgnoreCase) == true;

		// Check for manual exposure settings
		var hasManualSettings = metadata.HasExposureData() && 
		                       metadata.WhiteBalance == 1; // Manual white balance

		// Check for professional metadata
		var hasProfessionalMetadata = metadata.HasIptcData() || 
		                             metadata.HasXmpTags() ||
		                             !string.IsNullOrWhiteSpace(metadata.Copyright);

		return hasProfessionalCamera || hasManualSettings || hasProfessionalMetadata;
	}

	/// <summary>
	/// Gets the estimated quality level of the JPEG.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to analyze.</param>
	/// <returns>Quality level enumeration.</returns>
	public static JpegQualityLevel GetQualityLevel(this JpegMetadata metadata)
	{
		var score = 0;

		// Score based on resolution
		var pixelCount = metadata.GetPixelCount();
		if (pixelCount > 20000000) score += 3; // > 20MP
		else if (pixelCount > 10000000) score += 2; // > 10MP
		else if (pixelCount > 5000000) score += 1; // > 5MP

		// Score based on metadata completeness
		if (metadata.HasCameraMetadata()) score += 1;
		if (metadata.HasExposureData()) score += 1;
		if (metadata.HasGpsCoordinates()) score += 1;
		if (metadata.HasIccProfile()) score += 1;

		return score switch
		{
			<= 2 => JpegQualityLevel.Basic,
			<= 4 => JpegQualityLevel.Standard,
			<= 6 => JpegQualityLevel.High,
			_ => JpegQualityLevel.Professional
		};
	}

	/// <summary>
	/// Adds an IPTC tag to the metadata.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to modify.</param>
	/// <param name="tag">IPTC tag name.</param>
	/// <param name="value">Tag value.</param>
	public static void AddIptcTag(this JpegMetadata metadata, string tag, string value)
	{
		if (string.IsNullOrWhiteSpace(tag))
			throw new ArgumentException("Tag name cannot be null or empty.", nameof(tag));

		metadata.IptcTags[tag] = value ?? string.Empty;
	}

	/// <summary>
	/// Adds an XMP tag to the metadata.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to modify.</param>
	/// <param name="tag">XMP tag name.</param>
	/// <param name="value">Tag value.</param>
	public static void AddXmpTag(this JpegMetadata metadata, string tag, string value)
	{
		if (string.IsNullOrWhiteSpace(tag))
			throw new ArgumentException("Tag name cannot be null or empty.", nameof(tag));

		metadata.XmpTags[tag] = value ?? string.Empty;
	}

	/// <summary>
	/// Sets complete camera information.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to modify.</param>
	/// <param name="make">Camera make.</param>
	/// <param name="model">Camera model.</param>
	/// <param name="captureDateTime">Capture date and time.</param>
	public static void SetCameraInfo(this JpegMetadata metadata, string make, string model, DateTime? captureDateTime = null)
	{
		metadata.Make = make;
		metadata.Model = model;
		if (captureDateTime.HasValue)
			metadata.CaptureDateTime = captureDateTime;
	}

	/// <summary>
	/// Sets complete exposure information.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to modify.</param>
	/// <param name="exposureTime">Exposure time in seconds.</param>
	/// <param name="fNumber">F-number (aperture).</param>
	/// <param name="isoSpeed">ISO speed rating.</param>
	/// <param name="focalLength">Focal length in millimeters.</param>
	public static void SetExposureInfo(this JpegMetadata metadata, double? exposureTime = null, 
		double? fNumber = null, int? isoSpeed = null, double? focalLength = null)
	{
		if (exposureTime.HasValue) metadata.ExposureTime = exposureTime;
		if (fNumber.HasValue) metadata.FNumber = fNumber;
		if (isoSpeed.HasValue) metadata.IsoSpeedRating = isoSpeed;
		if (focalLength.HasValue) metadata.FocalLength = focalLength;
	}

	/// <summary>
	/// Creates a photography-optimized copy of the JPEG metadata.
	/// </summary>
	/// <param name="metadata">The source JPEG metadata.</param>
	/// <returns>Photography-optimized JPEG metadata.</returns>
	public static JpegMetadata CreatePhotographyOptimized(this JpegMetadata metadata)
	{
		var optimized = (JpegMetadata)metadata.CloneRaster();
		
		// Ensure sRGB color space for web compatibility
		if (!optimized.ColorSpace.HasValue)
			optimized.ColorSpace = 1; // sRGB

		// Set standard resolution if not present
		if (!optimized.HasResolution())
			optimized.SetResolution(300, 300, 2); // 300 DPI

		return optimized;
	}
}

/// <summary>
/// Camera metadata information structure.
/// </summary>
public record CameraMetadata
{
	public string? Make { get; init; }
	public string? Model { get; init; }
	public double? ExposureTime { get; init; }
	public double? FNumber { get; init; }
	public int? IsoSpeedRating { get; init; }
	public double? FocalLength { get; init; }
	public int? WhiteBalance { get; init; }
	public DateTime? CaptureDateTime { get; init; }
}

/// <summary>
/// JPEG quality level enumeration.
/// </summary>
public enum JpegQualityLevel
{
	/// <summary>Basic quality with minimal metadata.</summary>
	Basic,
	
	/// <summary>Standard quality with some metadata.</summary>
	Standard,
	
	/// <summary>High quality with comprehensive metadata.</summary>
	High,
	
	/// <summary>Professional quality with complete metadata.</summary>
	Professional
}