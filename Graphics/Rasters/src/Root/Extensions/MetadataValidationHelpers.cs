// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Text.RegularExpressions;

namespace Wangkanai.Graphics.Rasters.Extensions;

/// <summary>
/// Comprehensive validation helpers for metadata processing with security considerations.
/// </summary>
public static class MetadataValidationHelpers
{
	/// <summary>Security limits for metadata processing.</summary>
	public static class SecurityLimits
	{
		/// <summary>Maximum safe string length for metadata fields.</summary>
		public const int MaxSafeStringLength = 8192; // 8KB

		/// <summary>Maximum number of custom tags/chunks per metadata.</summary>
		public const int MaxCustomTags = 1000;

		/// <summary>Maximum total size for all custom data in bytes.</summary>
		public const int MaxCustomDataSize = 10_485_760; // 10MB

		/// <summary>Maximum recursion depth for nested metadata processing.</summary>
		public const int MaxRecursionDepth = 32;

		/// <summary>Timeout for metadata processing operations in milliseconds.</summary>
		public const int ProcessingTimeoutMs = 30_000; // 30 seconds
	}

	/// <summary>Regex patterns for validating various metadata fields.</summary>
	private static class ValidationPatterns
	{
		/// <summary>Valid characters for PNG text chunk keywords.</summary>
		public static readonly Regex PngKeywordPattern = new(@"^[\x20-\x7E\xA1-\xFF]+$", 
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		/// <summary>Valid copyright string pattern.</summary>
		public static readonly Regex CopyrightPattern = new(@"^[\p{L}\p{N}\p{P}\p{S}\s]+$", 
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		/// <summary>Valid camera make/model pattern.</summary>
		public static readonly Regex CameraMakeModelPattern = new(@"^[\p{L}\p{N}\p{P}\s\-_]+$", 
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		/// <summary>Valid XMP/IPTC tag name pattern.</summary>
		public static readonly Regex TagNamePattern = new(@"^[a-zA-Z][a-zA-Z0-9:_\-\.]*$", 
			RegexOptions.Compiled | RegexOptions.CultureInvariant);
	}

	/// <summary>
	/// Validates and sanitizes a string field for metadata use.
	/// </summary>
	/// <param name="value">The string value to validate.</param>
	/// <param name="fieldName">Name of the field for error reporting.</param>
	/// <param name="maxLength">Maximum allowed length (default: safe limit).</param>
	/// <param name="allowEmpty">Whether empty/null values are allowed.</param>
	/// <returns>Validated and sanitized string.</returns>
	/// <exception cref="ArgumentException">Thrown if validation fails.</exception>
	public static string? ValidateAndSanitizeString(string? value, string fieldName, 
		int maxLength = SecurityLimits.MaxSafeStringLength, bool allowEmpty = true)
	{
		if (string.IsNullOrEmpty(value))
		{
			if (!allowEmpty)
				throw new ArgumentException($"{fieldName} cannot be null or empty.", nameof(value));
			return value;
		}

		if (value.Length > maxLength)
			throw new ArgumentException($"{fieldName} exceeds maximum length of {maxLength} characters.", nameof(value));

		// Remove potentially dangerous characters (control characters except tab/newline)
		var sanitized = RemoveControlCharacters(value);
		
		if (sanitized.Length != value.Length)
			throw new ArgumentException($"{fieldName} contains invalid control characters.", nameof(value));

		return sanitized;
	}

	/// <summary>
	/// Validates a PNG keyword according to PNG specification.
	/// </summary>
	/// <param name="keyword">The keyword to validate.</param>
	/// <returns>True if the keyword is valid.</returns>
	public static bool IsValidPngKeyword(string? keyword)
	{
		if (string.IsNullOrEmpty(keyword))
			return false;

		if (keyword.Length > MetadataConstants.PngLimits.MaxKeywordLength)
			return false;

		if (keyword.StartsWith(' ') || keyword.EndsWith(' '))
			return false;

		if (keyword.Contains("  ")) // No consecutive spaces
			return false;

		return ValidationPatterns.PngKeywordPattern.IsMatch(keyword);
	}

	/// <summary>
	/// Validates a camera make or model string.
	/// </summary>
	/// <param name="value">The make/model string to validate.</param>
	/// <returns>True if the value is valid.</returns>
	public static bool IsValidCameraMakeModel(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return true; // Optional field

		if (value.Length > 128) // Reasonable limit for camera names
			return false;

		return ValidationPatterns.CameraMakeModelPattern.IsMatch(value);
	}

	/// <summary>
	/// Validates a copyright string.
	/// </summary>
	/// <param name="copyright">The copyright string to validate.</param>
	/// <returns>True if the copyright string is valid.</returns>
	public static bool IsValidCopyright(string? copyright)
	{
		if (string.IsNullOrWhiteSpace(copyright))
			return true; // Optional field

		if (copyright.Length > 512) // Reasonable limit for copyright
			return false;

		return ValidationPatterns.CopyrightPattern.IsMatch(copyright);
	}

	/// <summary>
	/// Validates an XMP or IPTC tag name.
	/// </summary>
	/// <param name="tagName">The tag name to validate.</param>
	/// <returns>True if the tag name is valid.</returns>
	public static bool IsValidTagName(string? tagName)
	{
		if (string.IsNullOrWhiteSpace(tagName))
			return false;

		if (tagName.Length > 256) // Reasonable limit for tag names
			return false;

		return ValidationPatterns.TagNamePattern.IsMatch(tagName);
	}

	/// <summary>
	/// Validates numeric ranges for camera settings.
	/// </summary>
	/// <param name="exposureTime">Exposure time in seconds.</param>
	/// <param name="fNumber">F-number (aperture).</param>
	/// <param name="isoSpeed">ISO speed rating.</param>
	/// <param name="focalLength">Focal length in millimeters.</param>
	/// <returns>Validation result with details.</returns>
	public static CameraValidationResult ValidateCameraSettings(double? exposureTime, double? fNumber, 
		int? isoSpeed, double? focalLength)
	{
		var result = new CameraValidationResult { IsValid = true };

		if (exposureTime.HasValue)
		{
			if (exposureTime <= 0 || exposureTime > MetadataConstants.CameraValidation.MaxExposureTime)
			{
				result.IsValid = false;
				result.Errors.Add($"Exposure time {exposureTime} is outside valid range (0 to {MetadataConstants.CameraValidation.MaxExposureTime}s).");
			}
		}

		if (fNumber.HasValue)
		{
			if (fNumber < MetadataConstants.CameraValidation.MinFNumber || 
			    fNumber > MetadataConstants.CameraValidation.MaxFNumber)
			{
				result.IsValid = false;
				result.Errors.Add($"F-number {fNumber} is outside valid range ({MetadataConstants.CameraValidation.MinFNumber} to {MetadataConstants.CameraValidation.MaxFNumber}).");
			}
		}

		if (isoSpeed.HasValue)
		{
			if (isoSpeed < MetadataConstants.CameraValidation.MinIsoSpeed || 
			    isoSpeed > MetadataConstants.CameraValidation.MaxIsoSpeed)
			{
				result.IsValid = false;
				result.Errors.Add($"ISO speed {isoSpeed} is outside valid range ({MetadataConstants.CameraValidation.MinIsoSpeed} to {MetadataConstants.CameraValidation.MaxIsoSpeed}).");
			}
		}

		if (focalLength.HasValue)
		{
			if (focalLength < MetadataConstants.CameraValidation.MinFocalLength || 
			    focalLength > MetadataConstants.CameraValidation.MaxFocalLength)
			{
				result.IsValid = false;
				result.Errors.Add($"Focal length {focalLength} is outside valid range ({MetadataConstants.CameraValidation.MinFocalLength} to {MetadataConstants.CameraValidation.MaxFocalLength}mm).");
			}
		}

		return result;
	}

	/// <summary>
	/// Validates GPS coordinates.
	/// </summary>
	/// <param name="latitude">Latitude in decimal degrees.</param>
	/// <param name="longitude">Longitude in decimal degrees.</param>
	/// <returns>True if coordinates are valid.</returns>
	public static bool IsValidGpsCoordinates(double? latitude, double? longitude)
	{
		if (!latitude.HasValue || !longitude.HasValue)
			return true; // Optional coordinates

		return latitude >= -90.0 && latitude <= 90.0 && 
		       longitude >= -180.0 && longitude <= 180.0;
	}

	/// <summary>
	/// Validates the total size of custom metadata to prevent memory issues.
	/// </summary>
	/// <param name="customDataSize">Total size of custom data in bytes.</param>
	/// <param name="customItemCount">Number of custom items.</param>
	/// <returns>Validation result with recommendations.</returns>
	public static CustomDataValidationResult ValidateCustomDataLimits(long customDataSize, int customItemCount)
	{
		var result = new CustomDataValidationResult { IsValid = true };

		if (customDataSize > SecurityLimits.MaxCustomDataSize)
		{
			result.IsValid = false;
			result.Errors.Add($"Custom data size ({customDataSize:N0} bytes) exceeds safe limit ({SecurityLimits.MaxCustomDataSize:N0} bytes).");
		}
		else if (customDataSize > SecurityLimits.MaxCustomDataSize / 2)
		{
			result.Warnings.Add($"Custom data size ({customDataSize:N0} bytes) is approaching the safe limit.");
		}

		if (customItemCount > SecurityLimits.MaxCustomTags)
		{
			result.IsValid = false;
			result.Errors.Add($"Number of custom items ({customItemCount}) exceeds safe limit ({SecurityLimits.MaxCustomTags}).");
		}
		else if (customItemCount > SecurityLimits.MaxCustomTags / 2)
		{
			result.Warnings.Add($"Number of custom items ({customItemCount}) is approaching the safe limit.");
		}

		return result;
	}

	/// <summary>
	/// Removes control characters from a string, keeping only tab and newline.
	/// </summary>
	/// <param name="input">Input string to sanitize.</param>
	/// <returns>Sanitized string.</returns>
	private static string RemoveControlCharacters(string input)
	{
		return new string(input.Where(c => !char.IsControl(c) || c == '\t' || c == '\n' || c == '\r').ToArray());
	}
}

/// <summary>Result of camera settings validation.</summary>
public record CameraValidationResult
{
	public bool IsValid { get; set; }
	public List<string> Errors { get; init; } = new();
	public List<string> Warnings { get; init; } = new();
}

/// <summary>Result of custom data validation.</summary>
public record CustomDataValidationResult
{
	public bool IsValid { get; set; }
	public List<string> Errors { get; init; } = new();
	public List<string> Warnings { get; init; } = new();
}