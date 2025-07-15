// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.

using Wangkanai.Graphics.Validation;

namespace Wangkanai.Graphics.Extensions;

/// <summary>
/// Extension methods for comprehensive metadata validation across all formats.
/// </summary>
public static class MetadataValidationExtensions
{
	/// <summary>
	/// Performs comprehensive validation of metadata including format-specific checks.
	/// </summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <returns>Comprehensive validation result.</returns>
	public static ValidationResult ValidateComprehensive(this IMetadata metadata)
	{
		var result = new ValidationResult();

		// Basic validation
		if (!metadata.ValidateMetadata())
			result.AddError("Basic metadata validation failed.");

		// Dimension validation
		if (!metadata.IsValidDimensions())
			result.AddError($"Invalid dimensions: {metadata.Width}x{metadata.Height}");

		// Orientation validation
		if (!metadata.IsValidOrientation())
			result.AddError($"Invalid orientation value: {metadata.Orientation}");

		// Size validation
		if (metadata.EstimatedMetadataSize < 0)
			result.AddError("Negative metadata size estimate.");

		// Large metadata warning
		if (metadata.IsConsideredLarge())
			result.AddWarning($"Large metadata size: {metadata.GetEstimatedSizeInMB():F2} MB");

		// Title validation
		if (metadata.HasTitle() && metadata.Title!.Length > 1000)
			result.AddWarning("Title is very long and may cause compatibility issues.");

		return result;
	}

	/// <summary>
	/// Validates metadata for web compatibility.
	/// </summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <returns>Web compatibility validation result.</returns>
	public static ValidationResult ValidateWebCompatibility(this IMetadata metadata)
	{
		var result = new ValidationResult();

		// Check dimensions for web use
		var pixelCount = metadata.GetPixelCount();
		if (pixelCount > 100000000) // 100 megapixels
			result.AddWarning("Very high resolution may cause performance issues on web.");

		// Check metadata size
		if (metadata.GetEstimatedSizeInMB() > 10)
			result.AddWarning("Large metadata may slow down web page loading.");

		// Check an aspect ratio for common web formats
		var aspectRatio = metadata.GetAspectRatio();
		if (aspectRatio < 0.1 || aspectRatio > 10)
			result.AddWarning("Extreme aspect ratio may cause display issues on web.");

		return result;
	}

	/// <summary>
	/// Validates metadata for print compatibility.
	/// </summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <returns>Print compatibility validation result.</returns>
	public static ValidationResult ValidatePrintCompatibility(this IMetadata metadata)
	{
		var result = new ValidationResult();

		// Check minimum resolution for print
		var pixelCount = metadata.GetPixelCount();
		if (pixelCount < 1000000) // Less than 1 megapixel
			result.AddWarning("Low resolution may not be suitable for high-quality printing.");

		// Check for extremely high pixel counts that might cause memory issues
		if (pixelCount > 500000000) // 500 megapixels
			result.AddWarning("Extremely high resolution may cause memory issues during printing.");

		return result;
	}

	/// <summary>
	/// Validates metadata against security best practices.
	/// </summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <returns>Security validation result.</returns>
	public static ValidationResult ValidateSecurity(this IMetadata metadata)
	{
		var result = new ValidationResult();

		// Check for potentially sensitive information in title
		if (metadata.HasTitle())
		{
			var title             = metadata.Title!.ToLowerInvariant();
			var sensitiveKeywords = new[] { "password", "secret", "private", "confidential", "internal" };

			if (sensitiveKeywords.Any(keyword => title.Contains(keyword)))
				result.AddWarning("Title may contain sensitive information.");
		}

		// Check for extremely large metadata that could be used for attacks
		if (metadata.GetEstimatedSizeInMB() > 100)
			result.AddError("Extremely large metadata may indicate a security threat.");

		return result;
	}

	/// <summary>
	/// Validates metadata for archival storage compatibility.
	/// </summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <returns>Archival compatibility validation result.</returns>
	public static ValidationResult ValidateArchivalCompatibility(this IMetadata metadata)
	{
		var result = new ValidationResult();

		// Check for complete metadata
		if (!metadata.HasTitle())
			result.AddWarning("Missing title may make archival organization difficult.");

		// Check for reasonable file sizes for long-term storage
		if (metadata.GetEstimatedSizeInMB() > 50)
			result.AddWarning("Large metadata may increase archival storage costs.");

		// Validate that basic information is present
		if (!metadata.HasDimensions())
			result.AddError("Missing dimensions critical for archival indexing.");

		return result;
	}

	/// <summary>
	/// Performs performance-focused validation.
	/// </summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <returns>Performance validation result.</returns>
	public static ValidationResult ValidatePerformance(this IMetadata metadata)
	{
		var result = new ValidationResult();

		// Check if async disposal is needed but not supported
		if (metadata.RequiresAsyncDisposal())
			result.AddInfo("Large metadata detected. Consider using async disposal patterns.");

		// Check for performance-impacting sizes
		var pixelCount = metadata.GetPixelCount();
		if (pixelCount > 50000000) // 50 megapixels
			result.AddWarning("High pixel count may impact processing performance.");

		// Check metadata overhead
		var metadataRatio = metadata.EstimatedMetadataSize / (double)pixelCount;
		if (metadataRatio > 10) // More than 10 bytes of metadata per pixel
			result.AddWarning("High metadata-to-pixel ratio may impact performance.");

		return result;
	}

	/// <summary>
	/// Validates metadata completeness for professional use.
	/// </summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <returns>Professional use validation result.</returns>
	public static ValidationResult ValidateProfessionalUse(this IMetadata metadata)
	{
		var result = new ValidationResult();

		// Check for essential professional metadata
		if (!metadata.HasTitle())
			result.AddWarning("Professional images should have descriptive titles.");

		if (!metadata.HasOrientation())
			result.AddWarning("Orientation information missing - may cause display issues.");

		// Check dimensions are reasonable for professional use
		if (!metadata.HasDimensions())
			result.AddError("Dimensions are required for professional image processing.");
		else
		{
			var pixelCount = metadata.GetPixelCount();
			if (pixelCount < 2000000) // Less than 2 megapixels
				result.AddWarning("Low resolution may not meet professional quality standards.");
		}

		return result;
	}

	/// <summary>
	/// Combines multiple validation types into a comprehensive report.
	/// </summary>
	/// <param name="metadata">The metadata to validate.</param>
	/// <param name="validationTypes">Types of validation to perform.</param>
	/// <returns>Combined validation result.</returns>
	public static ValidationResult ValidateMultiple(this IMetadata metadata, ValidationTypes validationTypes)
	{
		var combinedResult = new ValidationResult();

		if (validationTypes.HasFlag(ValidationTypes.Basic))
			combinedResult.Merge(metadata.ValidateComprehensive());

		if (validationTypes.HasFlag(ValidationTypes.Web))
			combinedResult.Merge(metadata.ValidateWebCompatibility());

		if (validationTypes.HasFlag(ValidationTypes.Print))
			combinedResult.Merge(metadata.ValidatePrintCompatibility());

		if (validationTypes.HasFlag(ValidationTypes.Security))
			combinedResult.Merge(metadata.ValidateSecurity());

		if (validationTypes.HasFlag(ValidationTypes.Archival))
			combinedResult.Merge(metadata.ValidateArchivalCompatibility());

		if (validationTypes.HasFlag(ValidationTypes.Performance))
			combinedResult.Merge(metadata.ValidatePerformance());

		if (validationTypes.HasFlag(ValidationTypes.Professional))
			combinedResult.Merge(metadata.ValidateProfessionalUse());

		return combinedResult;
	}
}
