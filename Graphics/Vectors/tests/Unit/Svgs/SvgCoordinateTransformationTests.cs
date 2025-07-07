// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Wangkanai.Graphics.Vectors.Svgs;
using Wangkanai.Spatial;
using Wangkanai.Spatial.Coordinates;

namespace Wangkanai.Graphics.Vectors.Tests.Svgs;

public class SvgCoordinateTransformationTests
{
	[Fact]
	public void TransformToSvgSpace_WithGeodeticCoordinate_ShouldReturnCorrectSvgCoordinate()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);
		var geodetic = new Geodetic(45.0, 10.0); // Lat, Lon
		var bounds = new GeographicBounds(40.0, 50.0, 0.0, 20.0); // MinLat, MaxLat, MinLon, MaxLon

		// Act
		var svgCoord = svg.TransformToSvgSpace(geodetic, bounds);

		// Assert
		// Longitude 10 is halfway between 0 and 20, so X should be 50
		// Latitude 45 is halfway between 40 and 50, but Y is inverted, so Y should be 50
		Assert.Equal(50.0, svgCoord.X, 0.001);
		Assert.Equal(50.0, svgCoord.Y, 0.001);
	}

	[Fact]
	public void TransformToGeographic_WithSvgCoordinate_ShouldReturnCorrectGeodeticCoordinate()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);
		var svgCoord = new Coordinate(50.0, 50.0);
		var bounds = new GeographicBounds(40.0, 50.0, 0.0, 20.0);

		// Act
		var geodetic = svg.TransformToGeographic(svgCoord, bounds);

		// Assert
		// X=50 should map to longitude 10 (halfway between 0 and 20)
		// Y=50 should map to latitude 45 (halfway between 40 and 50, accounting for Y inversion)
		Assert.Equal(45.0, geodetic.Latitude, 0.001);
		Assert.Equal(10.0, geodetic.Longitude, 0.001);
	}

	[Fact]
	public void SetCoordinateReferenceSystem_ShouldUpdateMetadataAndDocument()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);
		var crs = "EPSG:4326";

		// Act
		svg.SetCoordinateReferenceSystem(crs);

		// Assert
		Assert.Equal(crs, svg.Metadata.CoordinateReferenceSystem);
		Assert.Contains("data-crs=\"EPSG:4326\"", svg.ToXmlString());
	}

	[Fact]
	public void TransformCoordinateSystem_FromWgs84ToWebMercator_ShouldUpdateViewBox()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);
		var bounds = new GeographicBounds(-85.0, 85.0, -180.0, 180.0);
		var originalViewBox = svg.Metadata.ViewBox;

		// Act
		svg.TransformCoordinateSystem("EPSG:4326", "EPSG:3857", bounds);

		// Assert
		Assert.Equal("EPSG:3857", svg.Metadata.CoordinateReferenceSystem);
		Assert.NotEqual(originalViewBox, svg.Metadata.ViewBox);
		// ViewBox should now contain Mercator coordinates (much larger numbers)
		Assert.True(Math.Abs(svg.Metadata.ViewBox.Width) > 1000000);
	}

	[Fact]
	public void TransformCoordinateSystem_FromWebMercatorToWgs84_ShouldUpdateViewBox()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);
		var bounds = new GeographicBounds(-85.0, 85.0, -180.0, 180.0);
		
		// First transform to Web Mercator
		svg.TransformCoordinateSystem("EPSG:4326", "EPSG:3857", bounds);
		
		// Act - Transform back to WGS84
		svg.TransformCoordinateSystem("EPSG:3857", "EPSG:4326", bounds);

		// Assert
		Assert.Equal("EPSG:4326", svg.Metadata.CoordinateReferenceSystem);
		// ViewBox should be back to geographic coordinates
		Assert.True(Math.Abs(svg.Metadata.ViewBox.Width) < 1000);
	}

	[Fact]
	public void TransformToSvgSpace_WithCustomViewBox_ShouldRespectViewBoxOffset()
	{
		// Arrange
		using var svg = new SvgVector(100, 100);
		// Set a custom viewBox with offset
		svg.Metadata.ViewBox = new SvgViewBox(10, 20, 100, 100);
		
		var geodetic = new Geodetic(45.0, 10.0);
		var bounds = new GeographicBounds(40.0, 50.0, 0.0, 20.0);

		// Act
		var svgCoord = svg.TransformToSvgSpace(geodetic, bounds);

		// Assert
		// Should include the viewBox offset
		Assert.Equal(60.0, svgCoord.X, 0.001); // 50 + 10 offset
		Assert.Equal(70.0, svgCoord.Y, 0.001); // 50 + 20 offset
	}

	[Fact]
	public void TransformToSvgSpace_DisposedObject_ShouldThrowException()
	{
		// Arrange
		var svg = new SvgVector(100, 100);
		svg.Dispose();
		var geodetic = new Geodetic(45.0, 10.0);
		var bounds = new GeographicBounds(40.0, 50.0, 0.0, 20.0);

		// Act & Assert
		Assert.Throws<ObjectDisposedException>(() => svg.TransformToSvgSpace(geodetic, bounds));
	}

	[Fact]
	public void TransformToGeographic_DisposedObject_ShouldThrowException()
	{
		// Arrange
		var svg = new SvgVector(100, 100);
		svg.Dispose();
		var svgCoord = new Coordinate(50.0, 50.0);
		var bounds = new GeographicBounds(40.0, 50.0, 0.0, 20.0);

		// Act & Assert
		Assert.Throws<ObjectDisposedException>(() => svg.TransformToGeographic(svgCoord, bounds));
	}

	[Fact]
	public void SetCoordinateReferenceSystem_DisposedObject_ShouldThrowException()
	{
		// Arrange
		var svg = new SvgVector(100, 100);
		svg.Dispose();

		// Act & Assert
		Assert.Throws<ObjectDisposedException>(() => svg.SetCoordinateReferenceSystem("EPSG:4326"));
	}

	[Fact]
	public void TransformCoordinateSystem_DisposedObject_ShouldThrowException()
	{
		// Arrange
		var svg = new SvgVector(100, 100);
		svg.Dispose();
		var bounds = new GeographicBounds(-85.0, 85.0, -180.0, 180.0);

		// Act & Assert
		Assert.Throws<ObjectDisposedException>(() => 
			svg.TransformCoordinateSystem("EPSG:4326", "EPSG:3857", bounds));
	}

	[Theory]
	[InlineData(0.0, 0.0, 10.0, 30.0)] // Bottom-left corner
	[InlineData(90.0, 180.0, 90.0, 30.0)] // Top-right corner
	[InlineData(45.0, 90.0, 50.0, 30.0)] // Center
	public void TransformRoundTrip_ShouldPreserveOriginalCoordinates(double lat, double lon, double expectedX, double expectedY)
	{
		// Arrange
		using var svg = new SvgVector(100, 100);
		var originalGeodetic = new Geodetic(lat, lon);
		var bounds = new GeographicBounds(0.0, 90.0, 0.0, 180.0);

		// Act
		var svgCoord = svg.TransformToSvgSpace(originalGeodetic, bounds);
		var roundTripGeodetic = svg.TransformToGeographic(svgCoord, bounds);

		// Assert
		Assert.Equal(expectedX, svgCoord.X, 0.001);
		Assert.Equal(expectedY, svgCoord.Y, 0.001);
		Assert.Equal(originalGeodetic.Latitude, roundTripGeodetic.Latitude, 0.001);
		Assert.Equal(originalGeodetic.Longitude, roundTripGeodetic.Longitude, 0.001);
	}
}