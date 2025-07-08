// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Drawing;

namespace Wangkanai.Graphics.Rasters.Jpeg2000s;

/// <summary>Represents a JPEG2000 raster image with advanced wavelet-based compression capabilities.</summary>
/// <remarks>
/// JPEG2000 provides superior compression efficiency, lossless compression options, multi-resolution support,
/// progressive transmission, and advanced features like region-of-interest encoding. It's extensively used
/// in professional GIS, satellite imagery, digital cinema, and medical imaging applications.
/// </remarks>
public interface IJpeg2000Raster : IRaster
{
	/// <summary>Comprehensive JPEG2000 metadata including JP2 boxes and codestream parameters.</summary>
	new Jpeg2000Metadata Metadata { get; set; }

	/// <summary>Indicates if the image uses lossless compression.</summary>
	/// <remarks>
	/// Lossless compression uses the 5/3 reversible wavelet transform and allows perfect reconstruction.
	/// Lossy compression uses the 9/7 irreversible wavelet transform for higher compression ratios.
	/// </remarks>
	bool IsLossless { get; set; }

	/// <summary>Target compression ratio for lossy encoding (e.g., 20:1).</summary>
	/// <remarks>
	/// Only applies to lossy compression. Higher values result in smaller files but lower quality.
	/// Typical values range from 5:1 (high quality) to 100:1 (very high compression).
	/// </remarks>
	float CompressionRatio { get; set; }

	/// <summary>Number of DWT decomposition levels (0-32).</summary>
	/// <remarks>
	/// More levels provide better compression for large images but increase complexity.
	/// Typical values: 3-5 for small images, 5-8 for large images.
	/// </remarks>
	int DecompositionLevels { get; set; }

	/// <summary>Progression order for codestream organization.</summary>
	/// <remarks>
	/// Determines how the image data is organized for progressive transmission.
	/// LRCP is best for quality progression, RLCP for resolution progression.
	/// </remarks>
	Jpeg2000Progression ProgressionOrder { get; set; }

	/// <summary>Tile width for tiled images (pixels).</summary>
	/// <remarks>
	/// Tiling enables processing of very large images by dividing them into manageable blocks.
	/// Typical tile sizes: 512x512, 1024x1024, or 2048x2048 pixels.
	/// </remarks>
	int TileWidth { get; set; }

	/// <summary>Tile height for tiled images (pixels).</summary>
	int TileHeight { get; set; }

	/// <summary>Number of quality layers for progressive transmission.</summary>
	/// <remarks>
	/// Quality layers enable progressive quality enhancement during transmission.
	/// More layers provide finer quality control but increase overhead.
	/// </remarks>
	int QualityLayers { get; set; }

	/// <summary>Gets the number of available resolution levels for progressive decoding.</summary>
	/// <remarks>
	/// Resolution levels enable progressive resolution enhancement from thumbnail to full resolution.
	/// Level 0 is full resolution, level 1 is half resolution, etc.
	/// </remarks>
	int AvailableResolutionLevels { get; }

	/// <summary>Current region of interest for enhanced quality encoding.</summary>
	/// <remarks>
	/// ROI allows selective quality enhancement of specific image regions while maintaining
	/// lower quality in background areas for improved compression efficiency.
	/// </remarks>
	Rectangle? RegionOfInterest { get; set; }

	/// <summary>Quality enhancement factor for ROI (1.0 = no enhancement, 2.0 = double quality).</summary>
	float RoiQualityFactor { get; set; }

	/// <summary>Indicates if the image uses tiling for large image support.</summary>
	bool SupportsTiling { get; }

	/// <summary>Indicates if geospatial metadata (GeoJP2) is present.</summary>
	bool HasGeospatialMetadata { get; }

	/// <summary>Indicates if ICC color profile is embedded.</summary>
	bool HasIccProfile { get; }

	/// <summary>Gets the total number of tiles in the image.</summary>
	int TotalTiles { get; }

	/// <summary>Encodes the raster image to JPEG2000 format with specified options.</summary>
	/// <param name="options">Encoding options for compression, quality, and advanced features.</param>
	/// <returns>The encoded JPEG2000 data as a byte array.</returns>
	/// <exception cref="InvalidOperationException">Thrown when encoding fails due to invalid parameters.</exception>
	/// <exception cref="OutOfMemoryException">Thrown when insufficient memory for encoding.</exception>
	Task<byte[]> EncodeAsync(Jpeg2000EncodingOptions? options = null);

	/// <summary>Decodes JPEG2000 data into the raster image.</summary>
	/// <param name="data">The JPEG2000 encoded data.</param>
	/// <param name="resolutionLevel">Target resolution level (0 = full resolution).</param>
	/// <param name="qualityLayer">Target quality layer (-1 = all layers).</param>
	/// <returns>A task representing the async decode operation.</returns>
	/// <exception cref="ArgumentException">Thrown when data is invalid or corrupted.</exception>
	/// <exception cref="NotSupportedException">Thrown when format features are unsupported.</exception>
	Task DecodeAsync(byte[] data, int resolutionLevel = 0, int qualityLayer = -1);

	/// <summary>Decodes a specific region of the JPEG2000 image.</summary>
	/// <param name="region">The rectangular region to decode (in full resolution coordinates).</param>
	/// <param name="resolutionLevel">Target resolution level for the region.</param>
	/// <returns>The decoded region data as a byte array.</returns>
	/// <remarks>
	/// Enables efficient access to specific image regions without decoding the entire image.
	/// Particularly useful for large satellite or medical images.
	/// </remarks>
	Task<byte[]> DecodeRegionAsync(Rectangle region, int resolutionLevel = 0);

	/// <summary>Decodes the image at a specific resolution level for progressive display.</summary>
	/// <param name="resolutionLevel">The resolution level to decode (0 = full, 1 = half, etc.).</param>
	/// <returns>The decoded image data at the specified resolution.</returns>
	/// <remarks>
	/// Enables fast preview generation by decoding only the required resolution level.
	/// </remarks>
	Task<byte[]> DecodeResolutionAsync(int resolutionLevel);

	/// <summary>Sets a region of interest with enhanced quality settings.</summary>
	/// <param name="roi">The region of interest rectangle.</param>
	/// <param name="qualityFactor">Quality enhancement factor (1.0 = normal, 2.0 = double quality).</param>
	/// <remarks>
	/// ROI encoding allocates more bits to important image regions while reducing
	/// quality in background areas for improved overall compression efficiency.
	/// </remarks>
	void SetRegionOfInterest(Rectangle roi, float qualityFactor = 2.0f);

	/// <summary>Clears the current region of interest setting.</summary>
	void ClearRegionOfInterest();

	/// <summary>Gets available resolution levels for progressive decoding.</summary>
	/// <returns>Array of available resolution levels (0 = full resolution).</returns>
	int[] GetAvailableResolutions();

	/// <summary>Gets the image dimensions at a specific resolution level.</summary>
	/// <param name="resolutionLevel">The resolution level to query.</param>
	/// <returns>The dimensions (width, height) at the specified resolution level.</returns>
	(int Width, int Height) GetResolutionDimensions(int resolutionLevel);

	/// <summary>Estimates the file size for current encoding settings.</summary>
	/// <returns>Estimated file size in bytes.</returns>
	/// <remarks>
	/// Provides size estimation based on compression ratio, image dimensions, and bit depth.
	/// Actual size may vary depending on image content and complexity.
	/// </remarks>
	long GetEstimatedFileSize();

	/// <summary>Validates JPEG2000 format compliance and settings.</summary>
	/// <returns>True if the raster configuration is valid for JPEG2000 encoding.</returns>
	bool IsValid();

	/// <summary>Applies geospatial metadata for GeoJP2 support.</summary>
	/// <param name="geoTransform">Geographic transformation parameters.</param>
	/// <param name="coordinateSystem">Coordinate reference system as WKT.</param>
	/// <param name="geoTiffTags">Optional GeoTIFF tag data.</param>
	void ApplyGeospatialMetadata(double[] geoTransform, string coordinateSystem, byte[]? geoTiffTags = null);

	/// <summary>Applies an ICC color profile for accurate color reproduction.</summary>
	/// <param name="profileData">The ICC profile data.</param>
	void ApplyIccProfile(byte[] profileData);

	/// <summary>Adds custom metadata as UUID box.</summary>
	/// <param name="uuid">The UUID identifier for the metadata.</param>
	/// <param name="data">The metadata content.</param>
	void AddUuidMetadata(string uuid, byte[] data);

	/// <summary>Adds XML metadata box.</summary>
	/// <param name="xmlContent">The XML metadata content.</param>
	void AddXmlMetadata(string xmlContent);

	/// <summary>Gets tile bounds for the specified tile index.</summary>
	/// <param name="tileIndex">The linear tile index.</param>
	/// <returns>The tile bounds rectangle in image coordinates.</returns>
	Rectangle GetTileBounds(int tileIndex);

	/// <summary>Gets the tile index for the specified image coordinates.</summary>
	/// <param name="x">The X coordinate in the image.</param>
	/// <param name="y">The Y coordinate in the image.</param>
	/// <returns>The tile index containing the specified coordinates.</returns>
	int GetTileIndex(int x, int y);

	/// <summary>Sets the tile size for tiled image organization.</summary>
	/// <param name="tileWidth">The desired tile width.</param>
	/// <param name="tileHeight">The desired tile height.</param>
	/// <remarks>
	/// Tiling is beneficial for very large images as it enables efficient processing
	/// and memory management by dividing the image into smaller, manageable blocks.
	/// </remarks>
	void SetTileSize(int tileWidth = Jpeg2000Constants.DefaultTileSize, int tileHeight = Jpeg2000Constants.DefaultTileSize);
}

/// <summary>Encoding options for JPEG2000 compression.</summary>
public class Jpeg2000EncodingOptions
{
	/// <summary>Use lossless compression (5/3 reversible wavelet).</summary>
	public bool IsLossless { get; set; } = true;

	/// <summary>Target compression ratio for lossy encoding.</summary>
	public float CompressionRatio { get; set; } = Jpeg2000Constants.DefaultCompressionRatio;

	/// <summary>Number of DWT decomposition levels.</summary>
	public int DecompositionLevels { get; set; } = Jpeg2000Constants.DefaultDecompositionLevels;

	/// <summary>Number of quality layers.</summary>
	public int QualityLayers { get; set; } = Jpeg2000Constants.QualityLayers.DefaultLayers;

	/// <summary>Progression order.</summary>
	public Jpeg2000Progression ProgressionOrder { get; set; } = Jpeg2000Progression.LayerResolutionComponentPosition;

	/// <summary>Enable tiling with specified tile size.</summary>
	public bool EnableTiling { get; set; } = false;

	/// <summary>Tile width for tiled encoding.</summary>
	public int TileWidth { get; set; } = Jpeg2000Constants.DefaultTileSize;

	/// <summary>Tile height for tiled encoding.</summary>
	public int TileHeight { get; set; } = Jpeg2000Constants.DefaultTileSize;

	/// <summary>Error resilience features.</summary>
	public byte ErrorResilience { get; set; } = Jpeg2000Constants.ErrorResilience.None;

	/// <summary>Region of interest for enhanced quality.</summary>
	public Rectangle? RegionOfInterest { get; set; }

	/// <summary>ROI quality enhancement factor.</summary>
	public float RoiQualityFactor { get; set; } = 2.0f;

	/// <summary>Include ICC color profile.</summary>
	public bool IncludeIccProfile { get; set; } = true;

	/// <summary>Include geospatial metadata (GeoJP2).</summary>
	public bool IncludeGeospatialMetadata { get; set; } = true;

	/// <summary>Target file size in bytes (alternative to compression ratio).</summary>
	public long? TargetFileSize { get; set; }

	/// <summary>Maximum memory usage during encoding (MB).</summary>
	public int MaxMemoryUsageMB { get; set; } = Jpeg2000Constants.Memory.DefaultTileCacheSizeMB;
}
