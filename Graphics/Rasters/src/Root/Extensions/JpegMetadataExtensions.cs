// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Text;
using Wangkanai.Graphics.Extensions;
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
		var sb = new StringBuilder(MetadataConstants.Performance.ExposureSummaryCapacity); // Pre-allocate reasonable capacity
		var hasAnyData = false;

		if (metadata.ExposureTime.HasValue)
		{
			var exposure = metadata.ExposureTime.Value;
			if (exposure >= MetadataConstants.Performance.LongExposureThreshold)
				sb.Append($"{exposure:F1}s");
			else
				sb.Append($"1/{Math.Round(1 / exposure)}s");
			hasAnyData = true;
		}

		if (metadata.FNumber.HasValue)
		{
			if (hasAnyData) sb.Append(", ");
			sb.Append($"f/{metadata.FNumber:F1}");
			hasAnyData = true;
		}

		if (metadata.IsoSpeedRating.HasValue)
		{
			if (hasAnyData) sb.Append(", ");
			sb.Append($"ISO {metadata.IsoSpeedRating}");
			hasAnyData = true;
		}

		if (metadata.FocalLength.HasValue)
		{
			if (hasAnyData) sb.Append(", ");
			sb.Append($"{metadata.FocalLength:F0}mm");
			hasAnyData = true;
		}

		return hasAnyData ? sb.ToString() : "No exposure data";
	}

	/// <summary>
	/// Determines if the exposure settings are valid for photography.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to validate.</param>
	/// <returns>True if exposure settings are within reasonable ranges.</returns>
	public static bool HasValidExposureSettings(this JpegMetadata metadata)
	{
		// Validate exposure time
		if (metadata.ExposureTime.HasValue && 
		    (metadata.ExposureTime <= 0 || metadata.ExposureTime > MetadataConstants.CameraValidation.MaxExposureTime))
			return false;

		// Validate F-number
		if (metadata.FNumber.HasValue && 
		    (metadata.FNumber < MetadataConstants.CameraValidation.MinFNumber || 
		     metadata.FNumber > MetadataConstants.CameraValidation.MaxFNumber))
			return false;

		// Validate ISO speed rating
		if (metadata.IsoSpeedRating.HasValue && 
		    (metadata.IsoSpeedRating < MetadataConstants.CameraValidation.MinIsoSpeed || 
		     metadata.IsoSpeedRating > MetadataConstants.CameraValidation.MaxIsoSpeed))
			return false;

		// Validate focal length
		if (metadata.FocalLength.HasValue && 
		    (metadata.FocalLength < MetadataConstants.CameraValidation.MinFocalLength || 
		     metadata.FocalLength > MetadataConstants.CameraValidation.MaxFocalLength))
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
		return GetProfessionalPhotoScore(metadata) >= 0.6;
	}

	/// <summary>
	/// Calculates a comprehensive professional photography score based on multiple factors.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to analyze.</param>
	/// <returns>Professional score from 0.0 to 1.0.</returns>
	public static double GetProfessionalPhotoScore(this JpegMetadata metadata)
	{
		var totalScore = 0.0;

		// Camera equipment score (40% weight)
		var cameraScore = ProfessionalCameraDetection.CalculateProfessionalCameraScore(metadata.Make, metadata.Model);
		totalScore += cameraScore * 0.4;

		// Exposure settings score (30% weight)
		var exposureScore = ProfessionalCameraDetection.CalculateProfessionalExposureScore(
			metadata.ExposureTime, metadata.FNumber, metadata.IsoSpeedRating, metadata.WhiteBalance);
		totalScore += exposureScore * 0.3;

		// Lens characteristics score (15% weight)
		var lensScore = ProfessionalCameraDetection.IsProfessionalLens(metadata.FocalLength, metadata.FNumber) ? 1.0 : 0.0;
		totalScore += lensScore * 0.15;

		// Professional metadata score (15% weight)
		var metadataScore = 0.0;
		if (metadata.HasIptcData()) metadataScore += 0.4;
		if (metadata.HasXmpTags()) metadataScore += 0.3;
		if (!string.IsNullOrWhiteSpace(metadata.Copyright)) metadataScore += 0.3;
		totalScore += Math.Min(metadataScore, 1.0) * 0.15;

		return Math.Min(totalScore, 1.0);
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
		if (pixelCount > MetadataConstants.QualityThresholds.ProfessionalMegapixels) score += 3;
		else if (pixelCount > MetadataConstants.QualityThresholds.HighQualityMegapixels) score += 2;
		else if (pixelCount > MetadataConstants.QualityThresholds.StandardQualityMegapixels) score += 1;

		// Score based on metadata completeness
		if (metadata.HasCameraMetadata()) score += 1;
		if (metadata.HasExposureData()) score += 1;
		if (metadata.HasGpsCoordinates()) score += 1;
		if (metadata.HasIccProfile()) score += 1;

		return score switch
		{
			<= MetadataConstants.QualityThresholds.StandardQualityScore => JpegQualityLevel.Basic,
			<= MetadataConstants.QualityThresholds.HighQualityScore => JpegQualityLevel.Standard,
			<= MetadataConstants.QualityThresholds.ProfessionalQualityScore => JpegQualityLevel.High,
			_ => JpegQualityLevel.Professional
		};
	}

	/// <summary>
	/// Adds an IPTC tag to the metadata.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to modify.</param>
	/// <param name="tag">IPTC tag name.</param>
	/// <param name="value">Tag value.</param>
	/// <exception cref="ArgumentException">Thrown if tag name or value is invalid.</exception>
	public static void AddIptcTag(this JpegMetadata metadata, string tag, string value)
	{
		if (!MetadataValidationHelpers.IsValidTagName(tag))
			throw new ArgumentException("Invalid IPTC tag name format.", nameof(tag));

		var sanitizedValue = MetadataValidationHelpers.ValidateAndSanitizeString(value, "IPTC tag value", allowEmpty: true);
		
		// Check total custom data limits
		var currentSize = 0;
		foreach (var tagValue in metadata.IptcTags.Values)
		{
			if (tagValue != null) currentSize += tagValue.Length;
		}
		var validation = MetadataValidationHelpers.ValidateCustomDataLimits(currentSize, metadata.IptcTags.Count + 1);
		
		if (!validation.IsValid)
			throw new InvalidOperationException($"Cannot add IPTC tag: {string.Join("; ", validation.Errors)}");

		metadata.IptcTags[tag] = sanitizedValue ?? string.Empty;
	}

	/// <summary>
	/// Adds an XMP tag to the metadata.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to modify.</param>
	/// <param name="tag">XMP tag name.</param>
	/// <param name="value">Tag value.</param>
	/// <exception cref="ArgumentException">Thrown if tag name or value is invalid.</exception>
	public static void AddXmpTag(this JpegMetadata metadata, string tag, string value)
	{
		if (!MetadataValidationHelpers.IsValidTagName(tag))
			throw new ArgumentException("Invalid XMP tag name format.", nameof(tag));

		var sanitizedValue = MetadataValidationHelpers.ValidateAndSanitizeString(value, "XMP tag value", allowEmpty: true);
		
		// Check total custom data limits
		var currentSize = 0;
		foreach (var tagValue in metadata.XmpTags.Values)
		{
			if (tagValue != null) currentSize += tagValue.Length;
		}
		var validation = MetadataValidationHelpers.ValidateCustomDataLimits(currentSize, metadata.XmpTags.Count + 1);
		
		if (!validation.IsValid)
			throw new InvalidOperationException($"Cannot add XMP tag: {string.Join("; ", validation.Errors)}");

		metadata.XmpTags[tag] = sanitizedValue ?? string.Empty;
	}

	/// <summary>
	/// Sets complete camera information.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to modify.</param>
	/// <param name="make">Camera make.</param>
	/// <param name="model">Camera model.</param>
	/// <param name="captureDateTime">Capture date and time.</param>
	/// <exception cref="ArgumentException">Thrown if camera make or model is invalid.</exception>
	public static void SetCameraInfo(this JpegMetadata metadata, string make, string model, DateTime? captureDateTime = null)
	{
		if (!MetadataValidationHelpers.IsValidCameraMakeModel(make))
			throw new ArgumentException("Invalid camera make format.", nameof(make));
		
		if (!MetadataValidationHelpers.IsValidCameraMakeModel(model))
			throw new ArgumentException("Invalid camera model format.", nameof(model));

		metadata.Make = MetadataValidationHelpers.ValidateAndSanitizeString(make, "camera make", 128, false);
		metadata.Model = MetadataValidationHelpers.ValidateAndSanitizeString(model, "camera model", 128, false);
		
		if (captureDateTime.HasValue)
		{
			if (captureDateTime.Value < new DateTime(1970, 1, 1) || captureDateTime.Value > DateTime.Now.AddYears(1))
				throw new ArgumentException("Capture date and time is outside reasonable range.", nameof(captureDateTime));
			
			metadata.CaptureDateTime = captureDateTime;
		}
	}

	/// <summary>
	/// Sets complete exposure information.
	/// </summary>
	/// <param name="metadata">The JPEG metadata to modify.</param>
	/// <param name="exposureTime">Exposure time in seconds.</param>
	/// <param name="fNumber">F-number (aperture).</param>
	/// <param name="isoSpeed">ISO speed rating.</param>
	/// <param name="focalLength">Focal length in millimeters.</param>
	/// <exception cref="ArgumentException">Thrown if any exposure setting is invalid.</exception>
	public static void SetExposureInfo(this JpegMetadata metadata, double? exposureTime = null, 
		double? fNumber = null, int? isoSpeed = null, double? focalLength = null)
	{
		var validation = MetadataValidationHelpers.ValidateCameraSettings(exposureTime, fNumber, isoSpeed, focalLength);
		if (!validation.IsValid)
			throw new ArgumentException($"Invalid camera settings: {string.Join("; ", validation.Errors)}");

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
		var optimized = (JpegMetadata)metadata.Clone();
		
		// Ensure sRGB color space for web compatibility
		if (!optimized.ColorSpace.HasValue)
			optimized.ColorSpace = MetadataConstants.ProfessionalDetection.SrgbColorSpace;

		// Set standard resolution if not present
		if (!optimized.HasResolution())
			optimized.SetResolution(MetadataConstants.ProfessionalDetection.StandardPrintDpi, 
			                       MetadataConstants.ProfessionalDetection.StandardPrintDpi, 
			                       MetadataConstants.ProfessionalDetection.InchesResolutionUnit);

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