// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Vectors.Svgs;

namespace Wangkanai.Graphics.Vectors.Tests.Svgs;

public class SvgMetadataTests
{
	[Fact]
	public void Constructor_ShouldInitializeDefaultValues()
	{
		// Act
		using var metadata = new SvgMetadata();

		// Assert
		Assert.Equal(SvgConstants.DefaultVersion, metadata.Version);
		Assert.Equal(SvgViewBox.Default, metadata.ViewBox);
		Assert.Equal(100, metadata.ViewportWidth);
		Assert.Equal(100, metadata.ViewportHeight);
		Assert.Equal(SvgConstants.DefaultCrs, metadata.CoordinateReferenceSystem);
		Assert.False(metadata.IsCompressed);
		Assert.Equal(SvgConstants.DefaultCompressionLevel, metadata.CompressionLevel);
		Assert.Equal(0, metadata.ElementCount);
		Assert.Equal(0, metadata.TotalPathLength);
		Assert.Equal(SvgColorSpace.sRGB, metadata.ColorSpace);
		Assert.NotNull(metadata.Namespaces);
		Assert.NotNull(metadata.CustomProperties);
	}

	[Fact]
	public void Namespaces_ShouldContainStandardNamespaces()
	{
		// Act
		using var metadata = new SvgMetadata();

		// Assert
		Assert.Contains("svg", metadata.Namespaces.Keys);
		Assert.Contains("xlink", metadata.Namespaces.Keys);
		Assert.Contains("xml", metadata.Namespaces.Keys);
		Assert.Equal(SvgConstants.SvgNamespace, metadata.Namespaces["svg"]);
		Assert.Equal(SvgConstants.XLinkNamespace, metadata.Namespaces["xlink"]);
		Assert.Equal(SvgConstants.XmlNamespace, metadata.Namespaces["xml"]);
	}

	[Theory]
	[InlineData("1.0")]
	[InlineData("1.1")]
	[InlineData("2.0")]
	public void Version_SetSupportedVersions_ShouldSetCorrectly(string version)
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act
		metadata.Version = version;

		// Assert
		Assert.Equal(version, metadata.Version);
	}

	[Theory]
	[InlineData(0, 0, 100, 100)]
	[InlineData(10, 20, 200, 300)]
	[InlineData(-5, -10, 50, 75)]
	public void ViewBox_SetValues_ShouldSetCorrectly(double x, double y, double width, double height)
	{
		// Arrange
		using var metadata = new SvgMetadata();
		var viewBox = new SvgViewBox(x, y, width, height);

		// Act
		metadata.ViewBox = viewBox;

		// Assert
		Assert.Equal(viewBox, metadata.ViewBox);
	}

	[Theory]
	[InlineData(100.5, 200.7)]
	[InlineData(1920, 1080)]
	[InlineData(0.1, 0.1)]
	public void ViewportDimensions_SetValues_ShouldSetCorrectly(double width, double height)
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act
		metadata.ViewportWidth = width;
		metadata.ViewportHeight = height;

		// Assert
		Assert.Equal(width, metadata.ViewportWidth);
		Assert.Equal(height, metadata.ViewportHeight);
	}

	[Theory]
	[InlineData("EPSG:4326")]
	[InlineData("EPSG:3857")]
	[InlineData("EPSG:32633")]
	[InlineData(null)]
	public void CoordinateReferenceSystem_SetValues_ShouldSetCorrectly(string? crs)
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act
		metadata.CoordinateReferenceSystem = crs;

		// Assert
		Assert.Equal(crs, metadata.CoordinateReferenceSystem);
	}

	[Theory]
	[InlineData("Test Title")]
	[InlineData("")]
	[InlineData(null)]
	public void Title_SetValues_ShouldSetCorrectly(string? title)
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act
		metadata.Title = title;

		// Assert
		Assert.Equal(title, metadata.Title);
	}

	[Theory]
	[InlineData("Test Description")]
	[InlineData("")]
	[InlineData(null)]
	public void Description_SetValues_ShouldSetCorrectly(string? description)
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act
		metadata.Description = description;

		// Assert
		Assert.Equal(description, metadata.Description);
	}

	[Theory]
	[InlineData("John Doe")]
	[InlineData("")]
	[InlineData(null)]
	public void Creator_SetValues_ShouldSetCorrectly(string? creator)
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act
		metadata.Creator = creator;

		// Assert
		Assert.Equal(creator, metadata.Creator);
	}

	[Theory]
	[InlineData(true, 1)]
	[InlineData(true, 6)]
	[InlineData(true, 9)]
	[InlineData(false, 0)]
	public void Compression_SetValues_ShouldSetCorrectly(bool isCompressed, int level)
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act
		metadata.IsCompressed = isCompressed;
		metadata.CompressionLevel = level;

		// Assert
		Assert.Equal(isCompressed, metadata.IsCompressed);
		Assert.Equal(level, metadata.CompressionLevel);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(10)]
	[InlineData(1000)]
	[InlineData(5000)]
	public void ElementCount_SetValues_ShouldSetCorrectly(int count)
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act
		metadata.ElementCount = count;

		// Assert
		Assert.Equal(count, metadata.ElementCount);
	}

	[Theory]
	[InlineData(0.0)]
	[InlineData(100.5)]
	[InlineData(1000.0)]
	public void TotalPathLength_SetValues_ShouldSetCorrectly(double length)
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act
		metadata.TotalPathLength = length;

		// Assert
		Assert.Equal(length, metadata.TotalPathLength);
	}

	[Theory]
	[InlineData(SvgColorSpace.sRGB)]
	[InlineData(SvgColorSpace.LinearRGB)]
	[InlineData(SvgColorSpace.DisplayP3)]
	[InlineData(SvgColorSpace.Rec2020)]
	[InlineData(SvgColorSpace.Custom)]
	public void ColorSpace_SetValues_ShouldSetCorrectly(SvgColorSpace colorSpace)
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act
		metadata.ColorSpace = colorSpace;

		// Assert
		Assert.Equal(colorSpace, metadata.ColorSpace);
	}

	[Fact]
	public void CalculateEstimatedMemoryUsage_EmptyMetadata_ShouldReturnBaseSize()
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act
		var memoryUsage = metadata.CalculateEstimatedMemoryUsage();

		// Assert
		Assert.True(memoryUsage > 0);
		Assert.True(memoryUsage >= 1024); // Base size
	}

	[Fact]
	public void CalculateEstimatedMemoryUsage_WithElements_ShouldIncludeElementCost()
	{
		// Arrange
		using var metadata = new SvgMetadata();
		metadata.ElementCount = 100;

		// Act
		var memoryUsage = metadata.CalculateEstimatedMemoryUsage();

		// Assert
		var expectedMinimum = 1024 + (100 * SvgConstants.MemoryPerElement);
		Assert.True(memoryUsage >= expectedMinimum);
	}

	[Fact]
	public void CalculateEstimatedMemoryUsage_WithPaths_ShouldIncludePathCost()
	{
		// Arrange
		using var metadata = new SvgMetadata();
		metadata.TotalPathLength = 1000;

		// Act
		var memoryUsage = metadata.CalculateEstimatedMemoryUsage();

		// Assert
		var expectedMinimum = 1024 + (1000 * SvgConstants.MemoryPerPathSegment);
		Assert.True(memoryUsage >= expectedMinimum);
	}

	[Fact]
	public void CalculateEstimatedMemoryUsage_WithStringProperties_ShouldIncludeStringCost()
	{
		// Arrange
		using var metadata = new SvgMetadata();
		metadata.Title = "Test Title";
		metadata.Description = "Test Description";
		metadata.Creator = "Test Creator";

		// Act
		var memoryUsage = metadata.CalculateEstimatedMemoryUsage();

		// Assert
		var expectedStringCost = ("Test Title".Length + "Test Description".Length + "Test Creator".Length) * 2;
		Assert.True(memoryUsage >= 1024 + expectedStringCost);
	}

	[Theory]
	[InlineData("1.1", true)]
	[InlineData("2.0", true)]
	[InlineData("1.0", true)]
	[InlineData("3.0", false)]
	[InlineData("invalid", false)]
	public void ValidateCompliance_VersionCheck_ShouldReturnExpectedResult(string version, bool expectedValid)
	{
		// Arrange
		using var metadata = new SvgMetadata();
		metadata.Version = version;

		// Act
		var isValid = metadata.ValidateCompliance();

		// Assert
		Assert.Equal(expectedValid, isValid);
	}

	[Theory]
	[InlineData(0, 100, false)]   // Zero width
	[InlineData(100, 0, false)]   // Zero height
	[InlineData(-10, 100, false)] // Negative width
	[InlineData(100, -10, false)] // Negative height
	[InlineData(100, 100, true)]  // Valid dimensions
	public void ValidateCompliance_ViewBoxDimensions_ShouldReturnExpectedResult(double width, double height, bool expectedValid)
	{
		// Arrange
		using var metadata = new SvgMetadata();
		metadata.ViewBox = new SvgViewBox(0, 0, width, height);

		// Act
		var isValid = metadata.ValidateCompliance();

		// Assert
		Assert.Equal(expectedValid, isValid);
	}

	[Theory]
	[InlineData(0, false)]   // Zero width
	[InlineData(-10, false)] // Negative width
	[InlineData(100, true)]  // Valid width
	public void ValidateCompliance_ViewportDimensions_ShouldReturnExpectedResult(double dimension, bool expectedValid)
	{
		// Arrange
		using var metadata = new SvgMetadata();
		metadata.ViewportWidth = dimension;
		metadata.ViewportHeight = dimension;

		// Act
		var isValid = metadata.ValidateCompliance();

		// Assert
		Assert.Equal(expectedValid, isValid);
	}

	[Theory]
	[InlineData(true, 0, false)]  // Compressed with invalid level
	[InlineData(true, 10, false)] // Compressed with invalid level
	[InlineData(true, 6, true)]   // Compressed with valid level
	[InlineData(false, 0, true)]  // Not compressed
	public void ValidateCompliance_CompressionSettings_ShouldReturnExpectedResult(bool isCompressed, int level, bool expectedValid)
	{
		// Arrange
		using var metadata = new SvgMetadata();
		metadata.IsCompressed = isCompressed;
		metadata.CompressionLevel = level;

		// Act
		var isValid = metadata.ValidateCompliance();

		// Assert
		Assert.Equal(expectedValid, isValid);
	}

	[Theory]
	[InlineData(-1, false)] // Negative count
	[InlineData(0, true)]   // Zero count
	[InlineData(100, true)] // Positive count
	public void ValidateCompliance_ElementCount_ShouldReturnExpectedResult(int count, bool expectedValid)
	{
		// Arrange
		using var metadata = new SvgMetadata();
		metadata.ElementCount = count;

		// Act
		var isValid = metadata.ValidateCompliance();

		// Assert
		Assert.Equal(expectedValid, isValid);
	}

	[Theory]
	[InlineData(-1.0, false)] // Negative length
	[InlineData(0.0, true)]   // Zero length
	[InlineData(100.0, true)] // Positive length
	public void ValidateCompliance_TotalPathLength_ShouldReturnExpectedResult(double length, bool expectedValid)
	{
		// Arrange
		using var metadata = new SvgMetadata();
		metadata.TotalPathLength = length;

		// Act
		var isValid = metadata.ValidateCompliance();

		// Assert
		Assert.Equal(expectedValid, isValid);
	}

	[Fact]
	public void Clear_ShouldResetAllPropertiesToDefaults()
	{
		// Arrange
		using var metadata = new SvgMetadata();
		
		// Set some non-default values
		metadata.Version = "2.0";
		metadata.ViewBox = new SvgViewBox(10, 20, 300, 400);
		metadata.ViewportWidth = 500;
		metadata.ViewportHeight = 600;
		metadata.Title = "Custom Title";
		metadata.Description = "Custom Description";
		metadata.Creator = "Custom Creator";
		metadata.IsCompressed = true;
		metadata.CompressionLevel = 9;
		metadata.ElementCount = 100;
		metadata.TotalPathLength = 1000;
		metadata.ColorSpace = SvgColorSpace.DisplayP3;
		metadata.Namespaces["custom"] = "http://example.com";
		metadata.CustomProperties["custom"] = "value";

		// Act
		metadata.Clear();

		// Assert
		Assert.Equal(SvgConstants.DefaultVersion, metadata.Version);
		Assert.Equal(SvgViewBox.Default, metadata.ViewBox);
		Assert.Equal(100, metadata.ViewportWidth);
		Assert.Equal(100, metadata.ViewportHeight);
		Assert.Null(metadata.Title);
		Assert.Null(metadata.Description);
		Assert.Null(metadata.Creator);
		Assert.False(metadata.IsCompressed);
		Assert.Equal(SvgConstants.DefaultCompressionLevel, metadata.CompressionLevel);
		Assert.Equal(0, metadata.ElementCount);
		Assert.Equal(0, metadata.TotalPathLength);
		Assert.Equal(SvgColorSpace.sRGB, metadata.ColorSpace);
		Assert.DoesNotContain("custom", metadata.Namespaces.Keys);
		Assert.Empty(metadata.CustomProperties);
	}

	[Fact]
	public void IsLargeSvg_SmallSvg_ShouldReturnFalse()
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act & Assert
		Assert.False(metadata.IsLargeSvg);
	}

	[Fact]
	public void IsVeryLargeSvg_SmallSvg_ShouldReturnFalse()
	{
		// Arrange
		using var metadata = new SvgMetadata();

		// Act & Assert
		Assert.False(metadata.IsVeryLargeSvg);
	}

	[Fact]
	public void RequiresOptimization_FewElements_ShouldReturnFalse()
	{
		// Arrange
		using var metadata = new SvgMetadata();
		metadata.ElementCount = 100;

		// Act & Assert
		Assert.False(metadata.RequiresOptimization);
	}

	[Fact]
	public void RequiresOptimization_ManyElements_ShouldReturnTrue()
	{
		// Arrange
		using var metadata = new SvgMetadata();
		metadata.ElementCount = 2000;

		// Act & Assert
		Assert.True(metadata.RequiresOptimization);
	}

	[Fact]
	public async Task DisposeAsync_ShouldDisposeResourcesAsync()
	{
		// Arrange
		var metadata = new SvgMetadata();

		// Act
		await metadata.DisposeAsync();

		// Assert
		Assert.Throws<ObjectDisposedException>(() => metadata.CalculateEstimatedMemoryUsage());
	}

	[Fact]
	public void Dispose_ShouldDisposeResources()
	{
		// Arrange
		var metadata = new SvgMetadata();

		// Act
		metadata.Dispose();

		// Assert
		Assert.Throws<ObjectDisposedException>(() => metadata.CalculateEstimatedMemoryUsage());
	}

	[Fact]
	public void CreationDate_ShouldBeSetOnConstruction()
	{
		// Arrange
		var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

		// Act
		using var metadata = new SvgMetadata();
		var afterCreation = DateTime.UtcNow.AddSeconds(1);

		// Assert
		Assert.True(metadata.CreationDate >= beforeCreation);
		Assert.True(metadata.CreationDate <= afterCreation);
	}

	[Fact]
	public void ModificationDate_ShouldBeSetOnConstruction()
	{
		// Arrange
		var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

		// Act
		using var metadata = new SvgMetadata();
		var afterCreation = DateTime.UtcNow.AddSeconds(1);

		// Assert
		Assert.True(metadata.ModificationDate >= beforeCreation);
		Assert.True(metadata.ModificationDate <= afterCreation);
	}
}