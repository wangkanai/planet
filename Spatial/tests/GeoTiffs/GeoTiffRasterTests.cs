// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Rasters.Tiffs;

namespace Wangkanai.Spatial.GeoTiffs;

public class GeoTiffRasterTests
{
	[Fact]
	public void Constructor_ShouldInheritTiffRasterDefaults()
	{
		// Arrange & Act
		var geoTiffRaster = new GeoTiffRaster();

		// Assert
		Assert.Equal(TiffColorDepth.TwentyFourBit, geoTiffRaster.ColorDepth);
		Assert.Equal(TiffCompression.None, geoTiffRaster.Compression);
		Assert.Equal(PhotometricInterpretation.Rgb, geoTiffRaster.PhotometricInterpretation);
		Assert.False(geoTiffRaster.IsGeoreferenced);
	}

	[Fact]
	public void Constructor_WithDimensionsAndGeospatialData_ShouldSetProperties()
	{
		// Arrange
		const int width = 1024;
		const int height = 768;
		const string crs = "EPSG:4326";
		var extent = new MapExtent(0, 0, 10, 10);

		// Act
		var geoTiffRaster = new GeoTiffRaster(width, height, crs, extent);

		// Assert
		Assert.Equal(width, geoTiffRaster.Width);
		Assert.Equal(height, geoTiffRaster.Height);
		Assert.Equal(crs, geoTiffRaster.CoordinateReferenceSystem);
		Assert.Equal(extent, geoTiffRaster.Extent);
		Assert.True(geoTiffRaster.IsGeoreferenced);
	}

	[Fact]
	public void IsGeoreferenced_WithCrs_ShouldReturnTrue()
	{
		// Arrange
		var geoTiffRaster = new GeoTiffRaster
		{
			CoordinateReferenceSystem = "EPSG:3857"
		};

		// Act & Assert
		Assert.True(geoTiffRaster.IsGeoreferenced);
	}

	[Fact]
	public void IsGeoreferenced_WithGeoTransform_ShouldReturnTrue()
	{
		// Arrange
		var geoTiffRaster = new GeoTiffRaster
		{
			GeoTransform = new[] { 0.0, 1.0, 0.0, 0.0, 0.0, -1.0 }
		};

		// Act & Assert
		Assert.True(geoTiffRaster.IsGeoreferenced);
	}

	[Fact]
	public void IsGeoreferenced_WithExtent_ShouldReturnTrue()
	{
		// Arrange
		var geoTiffRaster = new GeoTiffRaster
		{
			Extent = new MapExtent(0, 0, 100, 100)
		};

		// Act & Assert
		Assert.True(geoTiffRaster.IsGeoreferenced);
	}

	[Fact]
	public void IsGeoreferenced_WithoutGeospatialData_ShouldReturnFalse()
	{
		// Arrange
		var geoTiffRaster = new GeoTiffRaster();

		// Act & Assert
		Assert.False(geoTiffRaster.IsGeoreferenced);
	}

	[Fact]
	public void PixelSize_WithGeoTransform_ShouldReturnCorrectValues()
	{
		// Arrange
		var geoTiffRaster = new GeoTiffRaster
		{
			GeoTransform = new[] { 100.0, 0.5, 0.0, 200.0, 0.0, -0.5 }
		};

		// Act & Assert
		Assert.Equal(0.5, geoTiffRaster.PixelSizeX);
		Assert.Equal(-0.5, geoTiffRaster.PixelSizeY);
	}

	[Fact]
	public void PixelSize_WithoutGeoTransform_ShouldReturnNull()
	{
		// Arrange
		var geoTiffRaster = new GeoTiffRaster();

		// Act & Assert
		Assert.Null(geoTiffRaster.PixelSizeX);
		Assert.Null(geoTiffRaster.PixelSizeY);
	}
}
