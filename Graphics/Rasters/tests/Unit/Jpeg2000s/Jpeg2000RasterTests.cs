// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Drawing;
using Wangkanai.Graphics.Rasters.Jpeg2000s;

namespace Wangkanai.Graphics.Rasters.Jpeg2000s;

public class Jpeg2000RasterTests
{
	[Fact]
	public void Constructor_Default_ShouldInitializeWithDefaults()
	{
		// Act
		var jpeg2000 = new Jpeg2000Raster();

		// Assert
		Assert.NotNull(jpeg2000.Metadata);
	}

	[Theory]
	[InlineData(800, 600, 1)]
	[InlineData(800, 600, 3)]
	[InlineData(800, 600, 4)]
	[InlineData(800, 600, 2)]
	public void Constructor_WithDimensions_ShouldInitializeCorrectly(int width, int height, int components)
	{
		// Act
		var jpeg2000 = new Jpeg2000Raster(width, height, components);

		// Assert
		Assert.Equal(width, jpeg2000.Width);
		Assert.Equal(height, jpeg2000.Height);
		Assert.Equal(width, jpeg2000.Metadata.Width);
		Assert.Equal(height, jpeg2000.Metadata.Height);
		Assert.Equal(components, jpeg2000.Metadata.Components);
	}

	[Theory]
	[InlineData(0, 600, 3)]
	[InlineData(-1, 600, 3)]
	[InlineData(800, 0, 3)]
	[InlineData(800, -1, 3)]
	public void Constructor_WithInvalidDimensions_ShouldThrowArgumentException(int width, int height, int components)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new Jpeg2000Raster(width, height, components));
	}

	[Theory]
	[InlineData(800, 600, 0)]
	[InlineData(800, 600, -1)]
	[InlineData(800, 600, 20000)] // Exceeds MaxComponents
	public void Constructor_WithInvalidComponents_ShouldThrowArgumentException(int width, int height, int components)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new Jpeg2000Raster(width, height, components));
	}

	[Fact]
	public void IsLossless_SetToTrue_ShouldUseReversibleWavelet()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);

		// Act
		jpeg2000.IsLossless = true;

		// Assert
		Assert.True(jpeg2000.IsLossless);
		Assert.Equal(Jpeg2000Constants.WaveletTransforms.Reversible53, jpeg2000.Metadata.WaveletTransform);
	}

	[Fact]
	public void IsLossless_SetToFalse_ShouldUseIrreversibleWavelet()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);

		// Act
		jpeg2000.IsLossless = false;

		// Assert
		Assert.False(jpeg2000.IsLossless);
		Assert.Equal(Jpeg2000Constants.WaveletTransforms.Irreversible97, jpeg2000.Metadata.WaveletTransform);
	}

	[Theory]
	[InlineData(10.0f)]
	[InlineData(50.0f)]
	[InlineData(100.0f)]
	public void CompressionRatio_SetValidValue_ShouldUpdateCorrectly(float ratio)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);
		jpeg2000.IsLossless = false; // Enable lossy compression

		// Act
		jpeg2000.CompressionRatio = ratio;

		// Assert
		Assert.Equal(ratio, jpeg2000.CompressionRatio);
		Assert.Equal(ratio, jpeg2000.Metadata.CompressionRatio);
	}

	[Theory]
	[InlineData(0.0f)]
	[InlineData(0.5f)]
	[InlineData(1.0f)]
	public void CompressionRatio_SetInvalidValueForLossyCompression_ShouldThrowArgumentException(float ratio)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);
		jpeg2000.IsLossless = false; // Enable lossy compression

		// Act & Assert
		Assert.Throws<ArgumentException>(() => jpeg2000.CompressionRatio = ratio);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(3)]
	[InlineData(8)]
	[InlineData(32)]
	public void DecompositionLevels_SetValidValue_ShouldUpdateCorrectly(int levels)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);

		// Act
		jpeg2000.DecompositionLevels = levels;

		// Assert
		Assert.Equal(levels, jpeg2000.DecompositionLevels);
		Assert.Equal(levels, jpeg2000.Metadata.DecompositionLevels);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(50)] // Exceeds MaxDecompositionLevels
	public void DecompositionLevels_SetInvalidValue_ShouldThrowArgumentException(int levels)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);

		// Act & Assert
		Assert.Throws<ArgumentException>(() => jpeg2000.DecompositionLevels = levels);
	}

	[Theory]
	[InlineData(512)]
	[InlineData(1024)]
	[InlineData(2048)]
	public void TileWidth_SetValidValue_ShouldUpdateCorrectly(int tileWidth)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(2048, 1536);

		// Act
		jpeg2000.TileWidth = tileWidth;

		// Assert
		Assert.Equal(tileWidth, jpeg2000.TileWidth);
		Assert.Equal(tileWidth, jpeg2000.Metadata.TileWidth);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void TileWidth_SetInvalidValue_ShouldThrowArgumentException(int tileWidth)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);

		// Act & Assert
		Assert.Throws<ArgumentException>(() => jpeg2000.TileWidth = tileWidth);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(5)]
	[InlineData(10)]
	public void QualityLayers_SetValidValue_ShouldUpdateCorrectly(int layers)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);

		// Act
		jpeg2000.QualityLayers = layers;

		// Assert
		Assert.Equal(layers, jpeg2000.QualityLayers);
		Assert.Equal(layers, jpeg2000.Metadata.QualityLayers);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(70000)] // Exceeds MaxLayers
	public void QualityLayers_SetInvalidValue_ShouldThrowArgumentException(int layers)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);

		// Act & Assert
		Assert.Throws<ArgumentException>(() => jpeg2000.QualityLayers = layers);
	}

	[Fact]
	public void AvailableResolutionLevels_ShouldReturnCorrectValue()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);
		jpeg2000.DecompositionLevels = 3;

		// Act
		var availableLevels = jpeg2000.AvailableResolutionLevels;

		// Assert
		Assert.Equal(4, availableLevels); // DecompositionLevels + 1
	}

	[Theory]
	[InlineData(1024, 800, 600, false)] // Single tile
	[InlineData(512, 800, 600, true)]   // Multiple tiles
	[InlineData(400, 800, 600, true)]   // Multiple tiles
	public void SupportsTiling_ShouldReturnCorrectValue(int tileSize, int width, int height, bool expectedSupport)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(width, height);
		jpeg2000.TileWidth = tileSize;
		jpeg2000.TileHeight = tileSize;

		// Act
		var supportsTiling = jpeg2000.SupportsTiling;

		// Assert
		Assert.Equal(expectedSupport, supportsTiling);
	}

	[Fact]
	public void HasGeospatialMetadata_WithGeoTransform_ShouldReturnTrue()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);
		jpeg2000.Metadata.GeoTransform = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };

		// Act
		var hasGeospatial = jpeg2000.HasGeospatialMetadata;

		// Assert
		Assert.True(hasGeospatial);
	}

	[Fact]
	public void HasIccProfile_WithProfile_ShouldReturnTrue()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);
		jpeg2000.Metadata.HasIccProfile = true;
		jpeg2000.Metadata.IccProfile = new byte[1024];

		// Act
		var hasIccProfile = jpeg2000.HasIccProfile;

		// Assert
		Assert.True(hasIccProfile);
	}

	[Fact]
	public void TotalTiles_ShouldMatchMetadata()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600);
		jpeg2000.TileWidth = 400;
		jpeg2000.TileHeight = 300;

		// Act
		var totalTiles = jpeg2000.TotalTiles;

		// Assert
		Assert.Equal(4, totalTiles); // 2x2 tiles
		Assert.Equal(jpeg2000.Metadata.TotalTiles, totalTiles);
	}

	[Fact]
	public async Task EncodeAsync_WithDefaultOptions_ShouldReturnData()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(100, 100, 3);

		// Act
		var encodedData = await jpeg2000.EncodeAsync();

		// Assert
		Assert.NotNull(encodedData);
		Assert.NotEmpty(encodedData);
	}

	[Fact]
	public async Task EncodeAsync_WithCustomOptions_ShouldApplyOptions()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		var options = new Jpeg2000EncodingOptions
		{
			IsLossless = false,
			CompressionRatio = 30.0f,
			DecompositionLevels = 6,
			QualityLayers = 3,
			EnableTiling = true,
			TileWidth = 256,
			TileHeight = 256
		};

		// Act
		var encodedData = await jpeg2000.EncodeAsync(options);

		// Assert
		Assert.NotNull(encodedData);
		Assert.False(jpeg2000.IsLossless);
		Assert.Equal(30.0f, jpeg2000.CompressionRatio);
		Assert.Equal(6, jpeg2000.DecompositionLevels);
		Assert.Equal(3, jpeg2000.QualityLayers);
		Assert.Equal(256, jpeg2000.TileWidth);
		Assert.Equal(256, jpeg2000.TileHeight);
	}

	[Fact]
	public async Task DecodeAsync_WithValidData_ShouldComplete()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster();
		var testData = new byte[1024]; // Placeholder data

		// Act & Assert
		await jpeg2000.DecodeAsync(testData);
		// Should complete without throwing
	}

	[Theory]
	[InlineData(null)]
	[InlineData(new byte[0])]
	public async Task DecodeAsync_WithInvalidData_ShouldThrowArgumentException(byte[] data)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster();

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentException>(() => jpeg2000.DecodeAsync(data));
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(10)] // Exceeds available levels
	public async Task DecodeAsync_WithInvalidResolutionLevel_ShouldThrowArgumentException(int resolutionLevel)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster();
		jpeg2000.DecompositionLevels = 3; // 4 available levels (0-3)
		var testData = new byte[1024];

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentException>(() => jpeg2000.DecodeAsync(testData, resolutionLevel));
	}

	[Fact]
	public async Task DecodeRegionAsync_WithValidRegion_ShouldReturnData()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		var region = new Rectangle(100, 100, 200, 200);

		// Act
		var regionData = await jpeg2000.DecodeRegionAsync(region);

		// Assert
		Assert.NotNull(regionData);
		Assert.NotEmpty(regionData);
	}

	[Theory]
	[InlineData(-10, 100, 200, 200)]
	[InlineData(100, -10, 200, 200)]
	[InlineData(700, 100, 200, 200)] // Extends beyond width
	[InlineData(100, 500, 200, 200)] // Extends beyond height
	public async Task DecodeRegionAsync_WithInvalidRegion_ShouldThrowArgumentException(int x, int y, int width, int height)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		var region = new Rectangle(x, y, width, height);

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentException>(() => jpeg2000.DecodeRegionAsync(region));
	}

	[Fact]
	public void SetRegionOfInterest_WithValidRegion_ShouldUpdateProperties()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		var roi = new Rectangle(100, 100, 200, 200);
		var qualityFactor = 2.5f;

		// Act
		jpeg2000.SetRegionOfInterest(roi, qualityFactor);

		// Assert
		Assert.Equal(roi, jpeg2000.RegionOfInterest);
		Assert.Equal(qualityFactor, jpeg2000.RoiQualityFactor);
	}

	[Theory]
	[InlineData(-10, 100, 200, 200)]
	[InlineData(700, 100, 200, 200)] // Extends beyond bounds
	public void SetRegionOfInterest_WithInvalidRegion_ShouldThrowArgumentException(int x, int y, int width, int height)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		var roi = new Rectangle(x, y, width, height);

		// Act & Assert
		Assert.Throws<ArgumentException>(() => jpeg2000.SetRegionOfInterest(roi));
	}

	[Theory]
	[InlineData(0.0f)]
	[InlineData(-1.0f)]
	public void SetRegionOfInterest_WithInvalidQualityFactor_ShouldThrowArgumentException(float qualityFactor)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		var roi = new Rectangle(100, 100, 200, 200);

		// Act & Assert
		Assert.Throws<ArgumentException>(() => jpeg2000.SetRegionOfInterest(roi, qualityFactor));
	}

	[Fact]
	public void ClearRegionOfInterest_ShouldResetProperties()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.SetRegionOfInterest(new Rectangle(100, 100, 200, 200), 2.0f);

		// Act
		jpeg2000.ClearRegionOfInterest();

		// Assert
		Assert.Null(jpeg2000.RegionOfInterest);
		Assert.Equal(1.0f, jpeg2000.RoiQualityFactor);
	}

	[Fact]
	public void GetAvailableResolutions_ShouldReturnCorrectArray()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.DecompositionLevels = 3;

		// Act
		var resolutions = jpeg2000.GetAvailableResolutions();

		// Assert
		Assert.Equal(new[] { 0, 1, 2, 3 }, resolutions);
	}

	[Theory]
	[InlineData(0, 800, 600)]
	[InlineData(1, 400, 300)]
	[InlineData(2, 200, 150)]
	[InlineData(3, 100, 75)]
	public void GetResolutionDimensions_ShouldReturnCorrectDimensions(int resolutionLevel, int expectedWidth, int expectedHeight)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.DecompositionLevels = 3;

		// Act
		var (width, height) = jpeg2000.GetResolutionDimensions(resolutionLevel);

		// Assert
		Assert.Equal(expectedWidth, width);
		Assert.Equal(expectedHeight, height);
	}

	[Fact]
	public void GetEstimatedFileSize_ShouldReturnReasonableSize()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		var expectedBaseSize = 800L * 600 * 3; // Width * Height * Components

		// Act
		var estimatedSize = jpeg2000.GetEstimatedFileSize();

		// Assert
		Assert.True(estimatedSize > 0);
		Assert.True(estimatedSize < expectedBaseSize); // Should be compressed
	}

	[Fact]
	public void ApplyGeospatialMetadata_WithValidData_ShouldUpdateMetadata()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		var geoTransform = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };
		var coordinateSystem = "EPSG:4326";
		var geoTiffTags = new byte[] { 1, 2, 3, 4 };

		// Act
		jpeg2000.ApplyGeospatialMetadata(geoTransform, coordinateSystem, geoTiffTags);

		// Assert
		Assert.Equal(geoTransform, jpeg2000.Metadata.GeoTransform);
		Assert.Equal(coordinateSystem, jpeg2000.Metadata.CoordinateReferenceSystem);
		Assert.Equal(geoTiffTags, jpeg2000.Metadata.GeoTiffMetadata);
		Assert.True(jpeg2000.HasGeospatialMetadata);
	}

	[Theory]
	[InlineData(null)]
	[InlineData(new double[] { 1.0, 2.0 })] // Wrong length
	public void ApplyGeospatialMetadata_WithInvalidGeoTransform_ShouldThrowArgumentException(double[] geoTransform)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);

		// Act & Assert
		Assert.Throws<ArgumentException>(() => jpeg2000.ApplyGeospatialMetadata(geoTransform, "EPSG:4326"));
	}

	[Fact]
	public void ApplyIccProfile_WithValidData_ShouldUpdateMetadata()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		var profileData = new byte[1024];

		// Act
		jpeg2000.ApplyIccProfile(profileData);

		// Assert
		Assert.Equal(profileData, jpeg2000.Metadata.IccProfile);
		Assert.True(jpeg2000.Metadata.HasIccProfile);
		Assert.True(jpeg2000.HasIccProfile);
	}

	[Theory]
	[InlineData(null)]
	[InlineData(new byte[0])]
	public void ApplyIccProfile_WithInvalidData_ShouldThrowArgumentException(byte[] profileData)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);

		// Act & Assert
		Assert.Throws<ArgumentException>(() => jpeg2000.ApplyIccProfile(profileData));
	}

	[Fact]
	public void AddUuidMetadata_WithValidData_ShouldUpdateMetadata()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		var uuid = "test-uuid";
		var data = new byte[] { 1, 2, 3, 4 };

		// Act
		jpeg2000.AddUuidMetadata(uuid, data);

		// Assert
		Assert.True(jpeg2000.Metadata.UuidBoxes.ContainsKey(uuid));
		Assert.Equal(data, jpeg2000.Metadata.UuidBoxes[uuid]);
	}

	[Fact]
	public void AddXmlMetadata_WithValidData_ShouldUpdateMetadata()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		var xmlContent = "<metadata>test</metadata>";

		// Act
		jpeg2000.AddXmlMetadata(xmlContent);

		// Assert
		Assert.Contains(xmlContent, jpeg2000.Metadata.XmlMetadata);
	}

	[Theory]
	[InlineData(0, 0, 0, 400, 300)]
	[InlineData(1, 400, 0, 400, 300)]
	[InlineData(2, 0, 300, 400, 300)]
	[InlineData(3, 400, 300, 400, 300)]
	public void GetTileBounds_ShouldReturnCorrectBounds(int tileIndex, int expectedX, int expectedY, int expectedWidth, int expectedHeight)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.TileWidth = 400;
		jpeg2000.TileHeight = 300;

		// Act
		var bounds = jpeg2000.GetTileBounds(tileIndex);

		// Assert
		Assert.Equal(expectedX, bounds.X);
		Assert.Equal(expectedY, bounds.Y);
		Assert.Equal(expectedWidth, bounds.Width);
		Assert.Equal(expectedHeight, bounds.Height);
	}

	[Theory]
	[InlineData(100, 100, 0)]
	[InlineData(500, 100, 1)]
	[InlineData(100, 400, 2)]
	[InlineData(500, 400, 3)]
	public void GetTileIndex_ShouldReturnCorrectIndex(int x, int y, int expectedIndex)
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		jpeg2000.TileWidth = 400;
		jpeg2000.TileHeight = 300;

		// Act
		var tileIndex = jpeg2000.GetTileIndex(x, y);

		// Assert
		Assert.Equal(expectedIndex, tileIndex);
	}

	[Fact]
	public void SetTileSize_ShouldUpdateTileSettings()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(2048, 1536, 3);

		// Act
		jpeg2000.SetTileSize(512, 512);

		// Assert
		Assert.Equal(512, jpeg2000.TileWidth);
		Assert.Equal(512, jpeg2000.TileHeight);
		Assert.True(jpeg2000.SupportsTiling);
	}

	[Fact]
	public void IsValid_WithValidConfiguration_ShouldReturnTrue()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);

		// Act
		var isValid = jpeg2000.IsValid();

		// Assert
		Assert.True(isValid);
	}

	[Fact]
	public void Dispose_ShouldCleanupResources()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);

		// Act
		jpeg2000.Dispose();

		// Assert
		// Should not throw when calling methods on disposed object
		Assert.Throws<ObjectDisposedException>(() => jpeg2000.Width);
	}

	[Fact]
	public async Task DisposeAsync_ShouldCleanupResourcesAsynchronously()
	{
		// Arrange
		var jpeg2000 = new Jpeg2000Raster(800, 600, 3);
		// Set up large metadata to trigger async disposal
		jpeg2000.Metadata.IccProfile = new byte[2 * 1024 * 1024];

		// Act
		await jpeg2000.DisposeAsync();

		// Assert
		// Should complete without throwing
	}
}
