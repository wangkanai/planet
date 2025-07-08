// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Drawing;

namespace Wangkanai.Graphics.Rasters.Jpeg2000s;

/// <summary>
/// Provides factory methods and common usage patterns for JPEG2000 rasters.
/// This class demonstrates various scenarios and configurations for working with JPEG2000 format.
/// </summary>
public static class Jpeg2000Examples
{
	/// <summary>
	/// Creates a basic JPEG2000 raster with default settings.
	/// Best for: Simple image encoding with balanced quality and compression.
	/// </summary>
	/// <param name="width">Image width in pixels</param>
	/// <param name="height">Image height in pixels</param>
	/// <param name="components">Number of color components (1=grayscale, 3=RGB, 4=RGBA)</param>
	/// <returns>Configured JPEG2000 raster with default settings</returns>
	public static Jpeg2000Raster CreateBasic(int width, int height, int components = 3)
	{
		return new Jpeg2000Raster(width, height, components);
	}

	/// <summary>
	/// Creates a high-quality JPEG2000 raster optimized for archival storage.
	/// Best for: Digital preservation, medical imaging, scientific data.
	/// Features: Lossless compression, high decomposition levels, multiple quality layers.
	/// </summary>
	/// <param name="width">Image width in pixels</param>
	/// <param name="height">Image height in pixels</param>
	/// <param name="components">Number of color components</param>
	/// <returns>High-quality archival JPEG2000 raster</returns>
	public static Jpeg2000Raster CreateArchival(int width, int height, int components = 3)
	{
		var jpeg2000 = new Jpeg2000Raster(width, height, components)
		               {
			               IsLossless          = true,
			               DecompositionLevels = Math.Min(8, (int)Math.Floor(Math.Log2(Math.Min(width, height) / 32.0))),
			               QualityLayers       = 1 // Single layer for lossless
		               };

		// Use optimal tile size for large images
		if (width > 4096 || height > 4096) jpeg2000.SetTileSize(1024, 1024);

		return jpeg2000;
	}

	/// <summary>
	/// Creates a web-optimized JPEG2000 raster for progressive streaming.
	/// Best for: Web applications, progressive image loading, bandwidth-constrained environments.
	/// Features: Quality progression, moderate compression, optimized for streaming.
	/// </summary>
	/// <param name="width">Image width in pixels</param>
	/// <param name="height">Image height in pixels</param>
	/// <param name="compressionRatio">Target compression ratio (20-50 recommended)</param>
	/// <returns>Web-optimized JPEG2000 raster</returns>
	public static Jpeg2000Raster CreateWebOptimized(int width, int height, float compressionRatio = 30.0f)
	{
		var jpeg2000 = new Jpeg2000Raster(width, height, 3)
		               {
			               IsLossless          = false,
			               CompressionRatio    = compressionRatio,
			               DecompositionLevels = 5,
			               QualityLayers       = 8 // Multiple layers for progressive quality
		               };

		// Set progression order for quality-first streaming
		jpeg2000.Jpeg2000Metadata.ProgressionOrder = Jpeg2000Progression.LayerResolutionComponentPosition;

		return jpeg2000;
	}

	/// <summary>
	/// Creates a geospatial JPEG2000 raster (GeoJP2) for geographic applications.
	/// Best for: Satellite imagery, aerial photography, GIS applications.
	/// Features: Geospatial metadata support, tiled for large datasets, optimized decomposition.
	/// </summary>
	/// <param name="width">Image width in pixels</param>
	/// <param name="height">Image height in pixels</param>
	/// <param name="geoTransform">Geospatial transformation matrix (6 elements)</param>
	/// <param name="coordinateSystem">Coordinate reference system (e.g., "EPSG:4326")</param>
	/// <param name="components">Number of spectral bands/components</param>
	/// <returns>Geospatial JPEG2000 raster</returns>
	public static Jpeg2000Raster CreateGeospatial(int width,            int height, double[] geoTransform,
		string                                        coordinateSystem, int components = 3)
	{
		var jpeg2000 = new Jpeg2000Raster(width, height, components)
		               {
			               IsLossless          = false,
			               CompressionRatio    = 10.0f, // Conservative compression for scientific data
			               DecompositionLevels = Math.Min(6, (int)Math.Floor(Math.Log2(Math.Min(width, height) / 64.0))),
			               QualityLayers       = 5
		               };

		// Apply geospatial metadata
		jpeg2000.ApplyGeospatialMetadata(geoTransform, coordinateSystem);

		// Use tiling for large geospatial datasets
		if (width > 2048 || height > 2048) jpeg2000.SetTileSize(512, 512);

		return jpeg2000;
	}

	/// <summary>
	/// Creates a region-of-interest optimized JPEG2000 raster.
	/// Best for: Medical imaging, surveillance, detailed analysis of specific areas.
	/// Features: ROI encoding with enhanced quality, spatial progression.
	/// </summary>
	/// <param name="width">Image width in pixels</param>
	/// <param name="height">Image height in pixels</param>
	/// <param name="roiRegion">Region of interest bounds</param>
	/// <param name="roiQualityFactor">Quality enhancement factor for ROI (1.5-5.0 recommended)</param>
	/// <param name="components">Number of color components</param>
	/// <returns>ROI-optimized JPEG2000 raster</returns>
	public static Jpeg2000Raster CreateRegionOfInterest(int width,                   int height, Rectangle roiRegion,
		float                                               roiQualityFactor = 3.0f, int components = 3)
	{
		var jpeg2000 = new Jpeg2000Raster(width, height, components)
		               {
			               IsLossless          = false,
			               CompressionRatio    = 25.0f,
			               DecompositionLevels = 6,
			               QualityLayers       = 6
		               };

		// Set spatial progression for efficient ROI access
		jpeg2000.Jpeg2000Metadata.ProgressionOrder = Jpeg2000Progression.PositionComponentResolutionLayer;

		// Configure region of interest
		jpeg2000.SetRegionOfInterest(roiRegion, roiQualityFactor);

		return jpeg2000;
	}

	/// <summary>
	/// Creates a multi-spectral JPEG2000 raster for scientific imaging.
	/// Best for: Satellite imagery, hyperspectral data, scientific analysis.
	/// Features: Many spectral bands, component-based progression, conservative compression.
	/// </summary>
	/// <param name="width">Image width in pixels</param>
	/// <param name="height">Image height in pixels</param>
	/// <param name="spectralBands">Number of spectral bands/components</param>
	/// <param name="bitDepth">Bit depth per component (8, 12, 16)</param>
	/// <returns>Multi-spectral JPEG2000 raster</returns>
	public static Jpeg2000Raster CreateMultiSpectral(int width, int height, int spectralBands, int bitDepth = 12)
	{
		var jpeg2000 = new Jpeg2000Raster(width, height, spectralBands)
		               {
			               IsLossless          = true, // Preserve scientific accuracy
			               DecompositionLevels = 5,
			               QualityLayers       = 1 // Single layer for lossless
		               };

		// Set component-first progression for spectral analysis
		jpeg2000.Jpeg2000Metadata.ProgressionOrder = Jpeg2000Progression.ComponentPositionResolutionLayer;
		jpeg2000.Jpeg2000Metadata.BitDepth         = bitDepth;

		// Use larger tiles for multi-spectral data
		if (width > 1024 || height > 1024) jpeg2000.SetTileSize(512, 512);

		return jpeg2000;
	}

	/// <summary>
	/// Creates a thumbnail-optimized JPEG2000 raster for fast preview generation.
	/// Best for: Image galleries, quick previews, multi-resolution displays.
	/// Features: Resolution progression, multiple decomposition levels, moderate compression.
	/// </summary>
	/// <param name="width">Image width in pixels</param>
	/// <param name="height">Image height in pixels</param>
	/// <param name="maxResolutionLevels">Maximum resolution levels to generate</param>
	/// <returns>Thumbnail-optimized JPEG2000 raster</returns>
	public static Jpeg2000Raster CreateThumbnailOptimized(int width, int height, int maxResolutionLevels = 6)
	{
		var jpeg2000 = new Jpeg2000Raster(width, height, 3)
		               {
			               IsLossless       = false,
			               CompressionRatio = 40.0f, // Higher compression for thumbnails
			               DecompositionLevels = Math.Min(maxResolutionLevels,
				               (int)Math.Floor(Math.Log2(Math.Min(width, height) / 16.0))),
			               QualityLayers = 4
		               };

		// Set resolution-first progression for fast thumbnail access
		jpeg2000.Jpeg2000Metadata.ProgressionOrder = Jpeg2000Progression.ResolutionLayerComponentPosition;

		return jpeg2000;
	}

	/// <summary>
	/// Creates a maximum quality JPEG2000 raster with color profile support.
	/// Best for: Professional photography, print preparation, color-critical applications.
	/// Features: Lossless compression, ICC profile support, optimal settings for color accuracy.
	/// </summary>
	/// <param name="width">Image width in pixels</param>
	/// <param name="height">Image height in pixels</param>
	/// <param name="iccProfile">ICC color profile data (optional)</param>
	/// <param name="components">Number of color components</param>
	/// <returns>Maximum quality JPEG2000 raster</returns>
	public static Jpeg2000Raster CreateMaximumQuality(int width, int height, byte[]? iccProfile = null, int components = 3)
	{
		var jpeg2000 = new Jpeg2000Raster(width, height, components)
		               {
			               IsLossless          = true,
			               DecompositionLevels = Math.Min(7, (int)Math.Floor(Math.Log2(Math.Min(width, height) / 32.0))),
			               QualityLayers       = 1 // Single layer for lossless
		               };

		// Apply ICC profile if provided
		if (iccProfile != null) jpeg2000.ApplyIccProfile(iccProfile);

		// Use optimal tile size for large images
		if (width > 2048 || height > 2048) jpeg2000.SetTileSize(1024, 1024);

		return jpeg2000;
	}

	/// <summary>
	/// Creates a bandwidth-constrained JPEG2000 raster for mobile/low-bandwidth scenarios.
	/// Best for: Mobile applications, satellite connections, limited bandwidth environments.
	/// Features: High compression, minimal decomposition levels, single quality layer.
	/// </summary>
	/// <param name="width">Image width in pixels</param>
	/// <param name="height">Image height in pixels</param>
	/// <param name="targetCompressionRatio">Target compression ratio (50-200 for high compression)</param>
	/// <returns>Bandwidth-optimized JPEG2000 raster</returns>
	public static Jpeg2000Raster CreateBandwidthConstrained(int width, int height, float targetCompressionRatio = 100.0f)
	{
		var jpeg2000 = new Jpeg2000Raster(width, height, 3)
		               {
			               IsLossless          = false,
			               CompressionRatio    = targetCompressionRatio,
			               DecompositionLevels = 3, // Minimal for bandwidth constraints
			               QualityLayers       = 1  // Single layer for simplicity
		               };

		// Use smaller tiles to reduce memory usage
		if (width > 512 || height > 512) jpeg2000.SetTileSize(256, 256);

		return jpeg2000;
	}

	/// <summary>
	/// Creates a JPEG2000 raster optimized for large format printing.
	/// Best for: Large format printing, billboard graphics, high-resolution displays.
	/// Features: High bit depth support, conservative compression, large tile sizes.
	/// </summary>
	/// <param name="width">Image width in pixels</param>
	/// <param name="height">Image height in pixels</param>
	/// <param name="bitDepth">Bit depth per component (typically 8 or 16)</param>
	/// <param name="components">Number of color components</param>
	/// <returns>Print-optimized JPEG2000 raster</returns>
	public static Jpeg2000Raster CreatePrintOptimized(int width, int height, int bitDepth = 8, int components = 4)
	{
		var jpeg2000 = new Jpeg2000Raster(width, height, components)
		               {
			               IsLossless          = bitDepth > 8,               // Use lossless for high bit depths
			               CompressionRatio    = bitDepth > 8 ? 1.0f : 5.0f, // Conservative compression
			               DecompositionLevels = 4,                          // Moderate levels for print
			               QualityLayers       = bitDepth > 8 ? 1 : 3
		               };

		jpeg2000.Jpeg2000Metadata.BitDepth = bitDepth;

		// Use large tiles for print workflows
		jpeg2000.SetTileSize(2048, 2048);

		return jpeg2000;
	}

	/// <summary>
	/// Demonstrates common encoding patterns and validates the created raster.
	/// </summary>
	/// <param name="jpeg2000">JPEG2000 raster to validate and demonstrate</param>
	/// <returns>Validation result with recommendations</returns>
	public static string ValidateAndGetRecommendations(Jpeg2000Raster jpeg2000)
	{
		var validation = Jpeg2000Validator.Validate(jpeg2000);

		if (validation.IsValid)
		{
			var summary = $"✓ Valid JPEG2000 configuration\n";
			summary += $"  Dimensions: {jpeg2000.Width}×{jpeg2000.Height}\n";
			summary += $"  Components: {jpeg2000.Jpeg2000Metadata.Components}\n";
			summary += $"  Compression: {(jpeg2000.IsLossless ? "Lossless" : $"Lossy ({jpeg2000.CompressionRatio:F1}:1)")}\n";
			summary += $"  Decomposition Levels: {jpeg2000.DecompositionLevels}\n";
			summary += $"  Quality Layers: {jpeg2000.QualityLayers}\n";
			summary += $"  Tiling: {(jpeg2000.SupportsTiling ? $"{jpeg2000.TileWidth}×{jpeg2000.TileHeight}" : "Single tile")}\n";
			summary += $"  Progression: {jpeg2000.Jpeg2000Metadata.ProgressionOrder.GetDescription()}\n";
			summary += $"  Estimated Size: {jpeg2000.GetEstimatedFileSize() / 1024:F1} KB";

			if (validation.Warnings.Any())
			{
				summary += $"\n\n⚠ Warnings:\n";
				summary += string.Join("\n", validation.Warnings.Select(w => $"  • {w}"));
			}

			return summary;
		}

		return validation.GetFormattedResults();
	}
}
