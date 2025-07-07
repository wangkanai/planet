// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Jpeg2000s;

public class Jpeg2000MetadataTests
{
	[Fact]
	public void Constructor_ShouldInitializeWithDefaults()
	{
		// Act
		var metadata = new Jpeg2000Metadata();

		// Assert
		Assert.Equal(0, metadata.Width);
		Assert.Equal(0, metadata.Height);
		Assert.Equal(3, metadata.Components);
		Assert.Equal(8, metadata.BitDepth);
		Assert.False(metadata.IsSigned);
		Assert.True(metadata.IsLossless);
		Assert.Equal(Jpeg2000Constants.DefaultCompressionRatio, metadata.CompressionRatio);
		Assert.Equal(Jpeg2000Constants.DefaultDecompositionLevels, metadata.DecompositionLevels);
		Assert.Equal(Jpeg2000Progression.LayerResolutionComponentPosition, metadata.ProgressionOrder);
		Assert.Equal(Jpeg2000Constants.QualityLayers.DefaultLayers, metadata.QualityLayers);
		Assert.Equal(Jpeg2000Constants.ColorSpaces.sRGB, metadata.ColorSpace);
		Assert.False(metadata.HasIccProfile);
		Assert.Null(metadata.IccProfile);
		Assert.Equal(Jpeg2000Constants.DefaultTileSize, metadata.TileWidth);
		Assert.Equal(Jpeg2000Constants.DefaultTileSize, metadata.TileHeight);
	}

	[Theory]
	[InlineData(800, 600, 1024, 1024, 1, 1)]
	[InlineData(2048, 1536, 512, 512, 4, 3)]
	[InlineData(100, 100, 50, 50, 2, 2)]
	public void TileCalculations_ShouldReturnCorrectValues(int width, int height, int tileWidth, int tileHeight, int expectedTilesAcross, int expectedTilesDown)
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			Width = width,
			Height = height,
			TileWidth = tileWidth,
			TileHeight = tileHeight
		};

		// Act & Assert
		Assert.Equal(expectedTilesAcross, metadata.TilesAcross);
		Assert.Equal(expectedTilesDown, metadata.TilesDown);
		Assert.Equal(expectedTilesAcross * expectedTilesDown, metadata.TotalTiles);
	}

	[Fact]
	public void HeaderType_WithBasicConfiguration_ShouldReturnJP2Basic()
	{
		// Arrange
		var metadata = new Jpeg2000Metadata();

		// Act
		var headerType = metadata.HeaderType;

		// Assert
		Assert.Equal("JP2 Basic", headerType);
	}

	[Fact]
	public void HeaderType_WithIccProfile_ShouldReturnJP2WithICC()
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			HasIccProfile = true,
			IccProfile = new byte[1024]
		};

		// Act
		var headerType = metadata.HeaderType;

		// Assert
		Assert.Equal("JP2 with ICC", headerType);
	}

	[Fact]
	public void HeaderType_WithGeospatialData_ShouldReturnGeoJP2()
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			GmlData = "<gml>test</gml>"
		};

		// Act
		var headerType = metadata.HeaderType;

		// Assert
		Assert.Equal("GeoJP2", headerType);
	}

	[Fact]
	public void HeaderType_WithExtendedMetadata_ShouldReturnJP2Extended()
	{
		// Arrange
		var metadata = new Jpeg2000Metadata();
		metadata.XmlMetadata.Add("<metadata>test</metadata>");

		// Act
		var headerType = metadata.HeaderType;

		// Assert
		Assert.Equal("JP2 Extended", headerType);
	}

	[Fact]
	public void IsValid_WithValidConfiguration_ShouldReturnTrue()
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			Width = 800,
			Height = 600,
			Components = 3,
			BitDepth = 8,
			DecompositionLevels = 5,
			QualityLayers = 3,
			TileWidth = 512,
			TileHeight = 512,
			IsLossless = false,
			CompressionRatio = 20.0f
		};

		// Act
		var isValid = metadata.IsValid();

		// Assert
		Assert.True(isValid);
	}

	[Theory]
	[InlineData(0, 600)]
	[InlineData(-1, 600)]
	[InlineData(800, 0)]
	[InlineData(800, -1)]
	public void IsValid_WithInvalidDimensions_ShouldReturnFalse(int width, int height)
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			Width = width,
			Height = height
		};

		// Act
		var isValid = metadata.IsValid();

		// Assert
		Assert.False(isValid);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(20000)] // Exceeds MaxComponents
	public void IsValid_WithInvalidComponents_ShouldReturnFalse(int components)
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			Width = 800,
			Height = 600,
			Components = components
		};

		// Act
		var isValid = metadata.IsValid();

		// Assert
		Assert.False(isValid);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(50)] // Exceeds MaxBitDepth
	public void IsValid_WithInvalidBitDepth_ShouldReturnFalse(int bitDepth)
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			Width = 800,
			Height = 600,
			BitDepth = bitDepth
		};

		// Act
		var isValid = metadata.IsValid();

		// Assert
		Assert.False(isValid);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(50)] // Exceeds MaxDecompositionLevels
	public void IsValid_WithInvalidDecompositionLevels_ShouldReturnFalse(int decompositionLevels)
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			Width = 800,
			Height = 600,
			DecompositionLevels = decompositionLevels
		};

		// Act
		var isValid = metadata.IsValid();

		// Assert
		Assert.False(isValid);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(70000)] // Exceeds MaxLayers
	public void IsValid_WithInvalidQualityLayers_ShouldReturnFalse(int qualityLayers)
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			Width = 800,
			Height = 600,
			QualityLayers = qualityLayers
		};

		// Act
		var isValid = metadata.IsValid();

		// Assert
		Assert.False(isValid);
	}

	[Theory]
	[InlineData(0, 512)]
	[InlineData(-1, 512)]
	[InlineData(512, 0)]
	[InlineData(512, -1)]
	public void IsValid_WithInvalidTileDimensions_ShouldReturnFalse(int tileWidth, int tileHeight)
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			Width = 800,
			Height = 600,
			TileWidth = tileWidth,
			TileHeight = tileHeight
		};

		// Act
		var isValid = metadata.IsValid();

		// Assert
		Assert.False(isValid);
	}

	[Fact]
	public void IsValid_WithInvalidCompressionRatio_ShouldReturnFalse()
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			Width = 800,
			Height = 600,
			IsLossless = false,
			CompressionRatio = 0.5f // Invalid for lossy compression
		};

		// Act
		var isValid = metadata.IsValid();

		// Assert
		Assert.False(isValid);
	}

	[Fact]
	public void HasLargeMetadata_WithSmallData_ShouldReturnFalse()
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			Width = 100,
			Height = 100,
			IccProfile = new byte[1024]
		};

		// Act
		var hasLargeMetadata = metadata.HasLargeMetadata;

		// Assert
		Assert.False(hasLargeMetadata);
	}

	[Fact]
	public void HasLargeMetadata_WithLargeIccProfile_ShouldReturnTrue()
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			IccProfile = new byte[2 * 1024 * 1024] // 2 MB
		};

		// Act
		var hasLargeMetadata = metadata.HasLargeMetadata;

		// Assert
		Assert.True(hasLargeMetadata);
	}

	[Fact]
	public void HasLargeMetadata_WithManyTiles_ShouldReturnTrue()
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			Width = 10000,
			Height = 10000,
			TileWidth = 64,
			TileHeight = 64
		};

		// Act
		var hasLargeMetadata = metadata.HasLargeMetadata;

		// Assert
		Assert.True(hasLargeMetadata);
		Assert.True(metadata.TotalTiles > 10000);
	}

	[Fact]
	public void EstimatedMemoryUsage_ShouldIncludeAllComponents()
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			IccProfile = new byte[1024],
			GeoTiffMetadata = new byte[512],
			PaletteData = new byte[256]
		};
		metadata.UuidBoxes.Add("test", new byte[2048]);
		metadata.XmlMetadata.Add("test metadata");
		metadata.Comments.Add("test comment");
		metadata.ChannelDefinitions.Add(new ChannelDefinition());
		metadata.ComponentMappings.Add(new ComponentMapping());
		metadata.Boxes.Add(new BoxInfo());
		metadata.Markers.Add(new MarkerInfo());

		// Act
		var estimatedSize = metadata.EstimatedMemoryUsage;

		// Assert
		Assert.True(estimatedSize > 1024 + 512 + 256 + 2048); // At least the byte arrays
		Assert.True(estimatedSize > 4000); // Should include overhead and other components
	}

	[Fact]
	public void Clone_ShouldCreateDeepCopy()
	{
		// Arrange
		var original = new Jpeg2000Metadata
		{
			Width = 800,
			Height = 600,
			Components = 3,
			BitDepth = 8,
			IccProfile = new byte[] { 1, 2, 3, 4 },
			GeoTransform = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 },
			CoordinateReferenceSystem = "EPSG:4326"
		};
		original.UuidBoxes.Add("test", new byte[] { 5, 6, 7, 8 });
		original.XmlMetadata.Add("test xml");
		original.Comments.Add("test comment");
		original.ChannelDefinitions.Add(new ChannelDefinition { ChannelIndex = 1 });

		// Act
		var clone = original.Clone();

		// Assert
		Assert.Equal(original.Width, clone.Width);
		Assert.Equal(original.Height, clone.Height);
		Assert.Equal(original.Components, clone.Components);
		Assert.Equal(original.BitDepth, clone.BitDepth);
		Assert.Equal(original.CoordinateReferenceSystem, clone.CoordinateReferenceSystem);

		// Verify deep copy of arrays
		Assert.NotSame(original.IccProfile, clone.IccProfile);
		Assert.Equal(original.IccProfile, clone.IccProfile);

		Assert.NotSame(original.GeoTransform, clone.GeoTransform);
		Assert.Equal(original.GeoTransform, clone.GeoTransform);

		// Verify deep copy of collections
		Assert.NotSame(original.UuidBoxes, clone.UuidBoxes);
		Assert.Equal(original.UuidBoxes.Count, clone.UuidBoxes.Count);
		Assert.NotSame(original.UuidBoxes["test"], clone.UuidBoxes["test"]);
		Assert.Equal(original.UuidBoxes["test"], clone.UuidBoxes["test"]);

		Assert.NotSame(original.XmlMetadata, clone.XmlMetadata);
		Assert.Equal(original.XmlMetadata, clone.XmlMetadata);

		Assert.NotSame(original.ChannelDefinitions, clone.ChannelDefinitions);
		Assert.Equal(original.ChannelDefinitions.Count, clone.ChannelDefinitions.Count);
		Assert.Equal(original.ChannelDefinitions[0].ChannelIndex, clone.ChannelDefinitions[0].ChannelIndex);
	}

	[Fact]
	public void Dispose_ShouldClearLargeData()
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			IccProfile = new byte[1024],
			GeoTiffMetadata = new byte[512],
			PaletteData = new byte[256]
		};
		metadata.UuidBoxes.Add("test", new byte[128]);
		metadata.XmlMetadata.Add("test");

		// Act
		metadata.Dispose();

		// Assert
		Assert.Null(metadata.IccProfile);
		Assert.Null(metadata.GeoTiffMetadata);
		Assert.Null(metadata.PaletteData);
		Assert.Empty(metadata.UuidBoxes);
		Assert.Empty(metadata.XmlMetadata);
	}

	[Fact]
	public async Task DisposeAsync_WithLargeMetadata_ShouldDisposeCorrectly()
	{
		// Arrange
		var metadata = new Jpeg2000Metadata
		{
			IccProfile = new byte[2 * 1024 * 1024], // 2 MB - triggers HasLargeMetadata
			TileWidth = 100,
			TileHeight = 100,
			Width = 10000,
			Height = 10000
		};

		// Act
		await metadata.DisposeAsync();

		// Assert - Should not throw and should clean up resources
		Assert.Null(metadata.IccProfile);
	}

	[Fact]
	public void ChannelDefinition_Clone_ShouldCreateCopy()
	{
		// Arrange
		var original = new ChannelDefinition
		{
			ChannelIndex = 1,
			ChannelType = 2,
			ChannelAssociation = 3
		};

		// Act
		var clone = original.Clone();

		// Assert
		Assert.NotSame(original, clone);
		Assert.Equal(original.ChannelIndex, clone.ChannelIndex);
		Assert.Equal(original.ChannelType, clone.ChannelType);
		Assert.Equal(original.ChannelAssociation, clone.ChannelAssociation);
	}

	[Fact]
	public void ComponentMapping_Clone_ShouldCreateCopy()
	{
		// Arrange
		var original = new ComponentMapping
		{
			ComponentIndex = 1,
			MappingType = 2,
			PaletteColumn = 3
		};

		// Act
		var clone = original.Clone();

		// Assert
		Assert.NotSame(original, clone);
		Assert.Equal(original.ComponentIndex, clone.ComponentIndex);
		Assert.Equal(original.MappingType, clone.MappingType);
		Assert.Equal(original.PaletteColumn, clone.PaletteColumn);
	}

	[Fact]
	public void BoxInfo_Clone_ShouldCreateCopy()
	{
		// Arrange
		var original = new BoxInfo
		{
			BoxType = "test",
			BoxSize = 1024,
			BoxOffset = 512,
			IsExtendedSize = true
		};

		// Act
		var clone = original.Clone();

		// Assert
		Assert.NotSame(original, clone);
		Assert.Equal(original.BoxType, clone.BoxType);
		Assert.Equal(original.BoxSize, clone.BoxSize);
		Assert.Equal(original.BoxOffset, clone.BoxOffset);
		Assert.Equal(original.IsExtendedSize, clone.IsExtendedSize);
	}

	[Fact]
	public void MarkerInfo_Clone_ShouldCreateCopy()
	{
		// Arrange
		var original = new MarkerInfo
		{
			MarkerType = 0xFF4F,
			MarkerOffset = 1024,
			SegmentLength = 256
		};

		// Act
		var clone = original.Clone();

		// Assert
		Assert.NotSame(original, clone);
		Assert.Equal(original.MarkerType, clone.MarkerType);
		Assert.Equal(original.MarkerOffset, clone.MarkerOffset);
		Assert.Equal(original.SegmentLength, clone.SegmentLength);
	}
}
