// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpeg2000s;

/// <summary>Provides validation functionality for JPEG2000 raster images and format compliance.</summary>
public static class Jpeg2000Validator
{
	/// <summary>Validates a JPEG2000 raster image and returns detailed validation results.</summary>
	/// <param name="jpeg2000">The JPEG2000 raster to validate.</param>
	/// <returns>A validation result containing any errors or warnings.</returns>
	public static Jpeg2000ValidationResult Validate(IJpeg2000Raster jpeg2000)
	{
		var result = new Jpeg2000ValidationResult();

		ValidateDimensions(jpeg2000, result);
		ValidateComponents(jpeg2000, result);
		ValidateCompressionSettings(jpeg2000, result);
		ValidateTilingSettings(jpeg2000, result);
		ValidateProgressionSettings(jpeg2000, result);
		ValidateQualitySettings(jpeg2000, result);
		ValidateRegionOfInterest(jpeg2000, result);
		ValidateMetadata(jpeg2000, result);
		ValidateGeospatialData(jpeg2000, result);
		ValidateMemoryConstraints(jpeg2000, result);

		return result;
	}

	/// <summary>Validates the image dimensions and basic properties.</summary>
	private static void ValidateDimensions(IJpeg2000Raster jpeg2000, Jpeg2000ValidationResult result)
	{
		if (jpeg2000.Width <= 0)
			result.AddError($"Invalid width: {jpeg2000.Width}. Width must be greater than 0.");

		if (jpeg2000.Height <= 0)
			result.AddError($"Invalid height: {jpeg2000.Height}. Height must be greater than 0.");

		if (jpeg2000.Width > Jpeg2000Constants.MaxWidth)
			result.AddError($"Width exceeds maximum: {jpeg2000.Width} > {Jpeg2000Constants.MaxWidth}.");

		if (jpeg2000.Height > Jpeg2000Constants.MaxHeight)
			result.AddError($"Height exceeds maximum: {jpeg2000.Height} > {Jpeg2000Constants.MaxHeight}.");

		// Check for extremely large images
		var totalPixels = (long)jpeg2000.Width * jpeg2000.Height;
		if (totalPixels > int.MaxValue)
			result.AddWarning($"Very large image: {totalPixels:N0} pixels. Consider using tiling for better performance.");

		// Validate dimension compatibility with decomposition levels
		var minDimension           = Math.Min(jpeg2000.Width, jpeg2000.Height);
		var maxDecompositionLevels = (int)Math.Floor(Math.Log2(minDimension));
		if (jpeg2000.DecompositionLevels > maxDecompositionLevels)
			result.AddWarning($"Too many decomposition levels ({jpeg2000.DecompositionLevels}) for image size. Maximum recommended: {maxDecompositionLevels}.");
	}

	/// <summary>Validates component configuration and bit depth.</summary>
	private static void ValidateComponents(IJpeg2000Raster jpeg2000, Jpeg2000ValidationResult result)
	{
		if (jpeg2000.Metadata.Components <= 0)
			result.AddError($"Invalid component count: {jpeg2000.Metadata.Components}. Must be greater than 0.");

		if (jpeg2000.Metadata.Components > Jpeg2000Constants.MaxComponents)
			result.AddError($"Too many components: {jpeg2000.Metadata.Components} > {Jpeg2000Constants.MaxComponents}.");

		if (jpeg2000.Metadata.BitDepth <= 0 || jpeg2000.Metadata.BitDepth > Jpeg2000Constants.MaxBitDepth)
			result.AddError($"Invalid bit depth: {jpeg2000.Metadata.BitDepth}. Must be between 1 and {Jpeg2000Constants.MaxBitDepth}.");

		// Validate reasonable component count
		if (jpeg2000.Metadata.Components == 1)
		{
			// Grayscale - valid
		}
		else if (jpeg2000.Metadata.Components == 3)
		{
			// RGB - valid
		}
		else if (jpeg2000.Metadata.Components == 4)
		{
			// RGBA - valid
		}
		else if (jpeg2000.Metadata.Components > 4)
		{
			// Multi-spectral or hyperspectral data - valid but warn if many components
			if (jpeg2000.Metadata.Components > 16)
				result.AddWarning($"Large number of components ({jpeg2000.Metadata.Components}) may impact performance.");
		}

		// Validate bit depth for common use cases
		if (jpeg2000.Metadata.BitDepth > 16 && jpeg2000.Metadata.Components > 1)
			result.AddWarning($"High bit depth ({jpeg2000.Metadata.BitDepth}) with multiple components may cause performance issues.");
	}

	/// <summary>Validates compression settings and compatibility.</summary>
	private static void ValidateCompressionSettings(IJpeg2000Raster jpeg2000, Jpeg2000ValidationResult result)
	{
		// Validate lossless compression settings
		if (jpeg2000.IsLossless)
		{
			if (jpeg2000.Metadata.WaveletTransform != Jpeg2000Constants.WaveletTransforms.Reversible53)
				result.AddError("Lossless compression requires 5/3 reversible wavelet transform.");

			if (jpeg2000.CompressionRatio != Jpeg2000Constants.DefaultCompressionRatio && jpeg2000.CompressionRatio < 50.0f)
				result.AddWarning("Compression ratio setting is ignored for lossless compression.");
		}
		else
		{
			// Validate lossy compression settings
			if (jpeg2000.CompressionRatio <= 1.0f)
				result.AddError($"Invalid compression ratio for lossy compression: {jpeg2000.CompressionRatio}. Must be greater than 1.0.");

			if (jpeg2000.CompressionRatio > 200.0f)
				result.AddWarning($"Very high compression ratio ({jpeg2000.CompressionRatio}:1) may result in poor image quality.");

			if (jpeg2000.Metadata.WaveletTransform == Jpeg2000Constants.WaveletTransforms.Reversible53)
				result.AddWarning("Using reversible wavelet for lossy compression. Consider 9/7 irreversible for better compression.");
		}

		// Validate decomposition levels
		if (jpeg2000.DecompositionLevels < 0)
			result.AddError($"Invalid decomposition levels: {jpeg2000.DecompositionLevels}. Cannot be negative.");

		if (jpeg2000.DecompositionLevels > Jpeg2000Constants.MaxDecompositionLevels)
			result.AddError($"Too many decomposition levels: {jpeg2000.DecompositionLevels} > {Jpeg2000Constants.MaxDecompositionLevels}.");

		if (jpeg2000.DecompositionLevels == 0)
			result.AddWarning("Zero decomposition levels disable wavelet transform compression benefits.");
	}

	/// <summary>Validates tiling configuration.</summary>
	private static void ValidateTilingSettings(IJpeg2000Raster jpeg2000, Jpeg2000ValidationResult result)
	{
		if (jpeg2000.TileWidth <= 0 || jpeg2000.TileHeight <= 0)
		{
			result.AddError($"Invalid tile dimensions: {jpeg2000.TileWidth}x{jpeg2000.TileHeight}. Must be positive.");
			return; // Skip further tile validation if dimensions are invalid
		}

		if (jpeg2000.TileWidth > jpeg2000.Width || jpeg2000.TileHeight > jpeg2000.Height)
			result.AddError("Tile dimensions cannot exceed image dimensions.");

		// Check for efficient tile sizes
		if (jpeg2000.SupportsTiling)
		{
			if (jpeg2000.TileWidth < Jpeg2000Constants.Memory.MinEfficientTileSize ||
			    jpeg2000.TileHeight < Jpeg2000Constants.Memory.MinEfficientTileSize)
				result.AddWarning($"Small tile size ({jpeg2000.TileWidth}x{jpeg2000.TileHeight}) may reduce compression efficiency.");

			if (jpeg2000.TileWidth > Jpeg2000Constants.Memory.MaxEfficientTileSize ||
			    jpeg2000.TileHeight > Jpeg2000Constants.Memory.MaxEfficientTileSize)
				result.AddWarning($"Large tile size ({jpeg2000.TileWidth}x{jpeg2000.TileHeight}) may increase memory usage.");

			// Check for power-of-2 tile sizes (optimal for wavelet transform)
			if (!IsPowerOfTwo(jpeg2000.TileWidth) || !IsPowerOfTwo(jpeg2000.TileHeight))
				result.AddWarning("Non-power-of-2 tile sizes may be less efficient for wavelet transform.");

			// Validate total tile count (only if tile dimensions are valid)
			try
			{
				if (jpeg2000.TotalTiles > 10000)
					result.AddWarning($"Large number of tiles ({jpeg2000.TotalTiles}) may impact performance.");
			}
			catch (DivideByZeroException)
			{
				// Already handled by the tile dimension check above
			}
		}
		else if ((long)jpeg2000.Width * jpeg2000.Height > 100_000_000) // 100 megapixels
			result.AddWarning("Large image without tiling may cause memory issues. Consider enabling tiling.");
	}

	/// <summary>Validates progression order settings.</summary>
	private static void ValidateProgressionSettings(IJpeg2000Raster jpeg2000, Jpeg2000ValidationResult result)
	{
		if (!Enum.IsDefined(typeof(Jpeg2000Progression), jpeg2000.ProgressionOrder))
			result.AddError($"Invalid progression order: {jpeg2000.ProgressionOrder}.");

		// Validate progression order against a use case
		if (jpeg2000.SupportsTiling && !jpeg2000.ProgressionOrder.SupportsEfficientSpatialAccess())
			result.AddWarning($"Progression order {jpeg2000.ProgressionOrder} may not be optimal for tiled images. Consider RPCL or PCRL.");

		if (jpeg2000.QualityLayers > 1 && !jpeg2000.ProgressionOrder.SupportsEfficientQualityScaling())
			result.AddWarning($"Progression order {jpeg2000.ProgressionOrder} may not be optimal for quality progression. Consider LRCP.");

		if (jpeg2000.AvailableResolutionLevels > 3 && !jpeg2000.ProgressionOrder.SupportsEfficientResolutionScaling())
			result.AddWarning($"Progression order {jpeg2000.ProgressionOrder} may not be optimal for resolution progression. Consider RLCP.");
	}

	/// <summary>Validates quality layer settings.</summary>
	private static void ValidateQualitySettings(IJpeg2000Raster jpeg2000, Jpeg2000ValidationResult result)
	{
		if (jpeg2000.QualityLayers < Jpeg2000Constants.QualityLayers.MinLayers)
			result.AddError($"Too few quality layers: {jpeg2000.QualityLayers} < {Jpeg2000Constants.QualityLayers.MinLayers}.");

		if (jpeg2000.QualityLayers > Jpeg2000Constants.QualityLayers.MaxLayers)
			result.AddError($"Too many quality layers: {jpeg2000.QualityLayers} > {Jpeg2000Constants.QualityLayers.MaxLayers}.");

		if (jpeg2000 is { IsLossless: true, QualityLayers: > 1 })
			result.AddWarning("Multiple quality layers have limited benefit for lossless compression.");

		if (jpeg2000.QualityLayers > 20)
			result.AddWarning($"Large number of quality layers ({jpeg2000.QualityLayers}) may increase overhead without significant benefit.");
	}

	/// <summary>Validates region of interest settings.</summary>
	private static void ValidateRegionOfInterest(IJpeg2000Raster jpeg2000, Jpeg2000ValidationResult result)
	{
		if (jpeg2000.RegionOfInterest.HasValue)
		{
			var roi = jpeg2000.RegionOfInterest.Value;

			if (roi.Width <= 0 || roi.Height <= 0)
				result.AddError($"Invalid ROI dimensions: {roi.Width}x{roi.Height}. Must be positive.");

			if (roi.X < 0 || roi.Y < 0 || roi.Right > jpeg2000.Width || roi.Bottom > jpeg2000.Height)
				result.AddError("ROI extends outside image bounds.");

			if (jpeg2000.RoiQualityFactor <= 0.0f)
				result.AddError($"Invalid ROI quality factor: {jpeg2000.RoiQualityFactor}. Must be positive.");

			if (jpeg2000.RoiQualityFactor > 10.0f)
				result.AddWarning($"Very high ROI quality factor ({jpeg2000.RoiQualityFactor}) may cause significant quality differences.");

			// Check ROI size relative to image
			var roiPixels     = roi.Width * roi.Height;
			var totalPixels   = jpeg2000.Width * jpeg2000.Height;
			var roiPercentage = (double)roiPixels / totalPixels * 100;

			if (roiPercentage > 80)
				result.AddWarning($"Large ROI ({roiPercentage:F1}% of image) may not provide significant compression benefits.");

			if (roiPercentage < 1)
				result.AddWarning($"Very small ROI ({roiPercentage:F1}% of image) may have minimal visual impact.");
		}
	}

	/// <summary>Validates metadata consistency and completeness.</summary>
	private static void ValidateMetadata(IJpeg2000Raster jpeg2000, Jpeg2000ValidationResult result)
	{
		var metadata = jpeg2000.Metadata;

		// Validate basic metadata consistency
		if (metadata.Width != jpeg2000.Width)
			result.AddError($"Width mismatch: Raster={jpeg2000.Width}, Metadata={metadata.Width}.");

		if (metadata.Height != jpeg2000.Height)
			result.AddError($"Height mismatch: Raster={jpeg2000.Height}, Metadata={metadata.Height}.");

		// Validate color space
		if (metadata.ColorSpace == 0)
			result.AddWarning("Color space not specified. Assuming sRGB.");

		// Validate ICC profile
		if (metadata.HasIccProfile)
		{
			if (metadata.IccProfile == null || metadata.IccProfile.Length == 0)
				result.AddError("ICC profile flag set but no profile data provided.");
			else if (metadata.IccProfile.Length < 128)
				result.AddWarning("ICC profile data seems too small to be valid.");
		}

		// Validate resolution information
		if (metadata.CaptureResolutionX <= 0 || metadata.CaptureResolutionY <= 0)
			result.AddWarning("Invalid or missing capture resolution information.");

		if (metadata.DisplayResolutionX <= 0 || metadata.DisplayResolutionY <= 0)
			result.AddWarning("Invalid or missing display resolution information.");

		// Validate timestamps
		if (metadata.CreationTime > DateTime.UtcNow.AddDays(1))
			result.AddWarning("Creation time is in the future.");

		if (metadata.ModificationTime < metadata.CreationTime)
			result.AddWarning("Modification time is before creation time.");
	}

	/// <summary>Validates geospatial metadata (GeoJP2) if present.</summary>
	private static void ValidateGeospatialData(IJpeg2000Raster jpeg2000, Jpeg2000ValidationResult result)
	{
		if (!jpeg2000.HasGeospatialMetadata)
			return;

		var metadata = jpeg2000.Metadata;

		// Validate GeoTransform
		if (metadata.GeoTransform != null)
		{
			if (metadata.GeoTransform.Length != 6)
				result.AddError($"Invalid GeoTransform length: {metadata.GeoTransform.Length}. Must be 6 elements.");
			else
			{
				var gt = metadata.GeoTransform;
				// Use epsilon comparison for floating-point values
				const double epsilon = 1e-10;
				if (Math.Abs(gt[1]) < epsilon && Math.Abs(gt[5]) < epsilon)
					result.AddError("Invalid GeoTransform: Both X and Y pixel sizes are zero.");

				if (Math.Abs(gt[2]) > Math.Abs(gt[1]) || Math.Abs(gt[4]) > Math.Abs(gt[5]))
					result.AddWarning("GeoTransform indicates significant rotation or skew.");
			}
		}

		// Validate coordinate reference system
		if (string.IsNullOrEmpty(metadata.CoordinateReferenceSystem))
			result.AddWarning("Geospatial metadata present but coordinate reference system not specified.");

		// Validate GeoTIFF metadata
		if (metadata.GeoTiffMetadata != null && metadata.GeoTiffMetadata.Length > 0)
		{
			if (metadata.GeoTiffMetadata.Length < 16)
				result.AddWarning("GeoTIFF metadata seems too small to contain valid geo-referencing information.");
		}

		// Validate GML data
		if (!string.IsNullOrEmpty(metadata.GmlData))
		{
			if (!metadata.GmlData.TrimStart().StartsWith("<?xml") && !metadata.GmlData.TrimStart().StartsWith("<"))
				result.AddWarning("GML data does not appear to be valid XML.");
		}
	}

	/// <summary>Validates memory and performance constraints.</summary>
	private static void ValidateMemoryConstraints(IJpeg2000Raster jpeg2000, Jpeg2000ValidationResult result)
	{
		// Estimate memory usage
		var estimatedMemory = jpeg2000.Metadata.EstimatedMetadataSize;
		if (estimatedMemory > 1024 * 1024 * 1024) // 1 GB
			result.AddWarning($"Large metadata size ({estimatedMemory / (1024 * 1024):N0} MB) may impact performance.");

		// Check for very large files
		var estimatedFileSize = jpeg2000.GetEstimatedFileSize();
		if (estimatedFileSize > 2L * 1024 * 1024 * 1024) // 2 GB
			result.AddWarning($"Very large estimated file size ({estimatedFileSize / (1024 * 1024):N0} MB). Consider using tiling or higher compression.");

		// Validate tile cache implications
		if (jpeg2000.SupportsTiling && jpeg2000.TileWidth > 0 && jpeg2000.TileHeight > 0)
		{
			var bytesPerTile = jpeg2000.TileWidth * jpeg2000.TileHeight * jpeg2000.Metadata.Components *
			                   ((jpeg2000.Metadata.BitDepth + 7) / 8);
			var defaultCacheSize = Jpeg2000Constants.Memory.DefaultTileCacheSizeMB * 1024 * 1024;
			var tilesInCache     = defaultCacheSize / bytesPerTile;

			if (tilesInCache < 4)
				result.AddWarning("Large tile size may exceed default cache capacity. Consider smaller tiles or increased cache size.");
		}
	}

	/// <summary>Validates a JP2 file signature from raw data.</summary>
	/// <param name="data">The file data to validate.</param>
	/// <returns>True if the data starts with a valid JP2 signature, false otherwise.</returns>
	public static bool IsValidJp2Signature(ReadOnlySpan<byte> data)
	{
		if (data.Length < 12) // Minimum size for signature box
			return false;

		// Check signature box size (should be 12) - JPEG2000 uses big-endian
		var boxSize = (uint)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);
		if (boxSize != 12)
			return false;

		// Check signature box type ("jP  ")
		if (!data.Slice(4, 4).SequenceEqual(Jpeg2000Constants.SignatureBoxType.AsSpan()))
			return false;

		// Check signature data
		return data.Slice(8, 4).SequenceEqual(Jpeg2000Constants.SignatureData.AsSpan());
	}

	/// <summary>Detects the JPEG2000 format variant from file data.</summary>
	/// <param name="data">The file data to analyze.</param>
	/// <returns>The detected format variant, or empty string if invalid.</returns>
	public static string DetectJpeg2000Variant(ReadOnlySpan<byte> data)
	{
		if (!IsValidJp2Signature(data))
			return string.Empty;

		if (data.Length < 32) // Need enough data to check ftyp box
			return "JP2";

		// Look for File Type box
		if (data.Length >= 24 && data.Slice(16, 4).SequenceEqual(Jpeg2000Constants.FileTypeBoxType.AsSpan()))
		{
			var brand = data.Slice(20, 4);
			if (brand.SequenceEqual(Jpeg2000Constants.Jp2Brand.AsSpan()))
				return "JP2";
		}

		return "J2K"; // Raw codestream
	}

	/// <summary>Checks if a number is a power of two.</summary>
	private static bool IsPowerOfTwo(int value)
		=> value > 0 && (value & (value - 1)) == 0;
}

/// <summary>Represents the result of JPEG2000 format validation.</summary>
public class Jpeg2000ValidationResult
{
	/// <summary>List of validation errors that prevent proper JPEG2000 encoding/decoding.</summary>
	public List<string> Errors { get; } = new();

	/// <summary>List of validation warnings that may affect quality or performance.</summary>
	public List<string> Warnings { get; } = new();

	/// <summary>Indicates if the JPEG2000 raster is valid (no errors).</summary>
	public bool IsValid => Errors.Count == 0;

	/// <summary>Indicates if there are any warnings.</summary>
	public bool HasWarnings => Warnings.Count > 0;

	/// <summary>Adds a validation error.</summary>
	/// <param name="error">The error message.</param>
	public void AddError(string error)
	{
		if (!string.IsNullOrEmpty(error))
			Errors.Add(error);
	}

	/// <summary>Adds a validation warning.</summary>
	/// <param name="warning">The warning message.</param>
	public void AddWarning(string warning)
	{
		if (!string.IsNullOrEmpty(warning))
			Warnings.Add(warning);
	}

	/// <summary>Gets a summary of the validation results.</summary>
	/// <returns>A human-readable summary string.</returns>
	public string GetSummary()
	{
		if (IsValid && !HasWarnings)
			return "Valid JPEG2000 configuration with no issues.";

		if (IsValid && HasWarnings)
			return $"Valid JPEG2000 configuration with {Warnings.Count} warning(s).";

		return $"Invalid JPEG2000 configuration: {Errors.Count} error(s), {Warnings.Count} warning(s).";
	}

	/// <summary>Gets all issues as a formatted string.</summary>
	/// <returns>A formatted string containing all errors and warnings.</returns>
	public string GetFormattedResults()
	{
		var result = new List<string> { GetSummary() };

		if (Errors.Count > 0)
		{
			result.Add("\nErrors:");
			result.AddRange(Errors.Select(e => $"  - {e}"));
		}

		if (Warnings.Count > 0)
		{
			result.Add("\nWarnings:");
			result.AddRange(Warnings.Select(w => $"  - {w}"));
		}

		return string.Join("\n", result);
	}
}
